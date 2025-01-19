using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using UniLinq;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Scripting;

public class Prefab : INeighborBlockCache, IChunkAccess
{
	public string PrefabName
	{
		get
		{
			return this.location.FileNameNoExtension ?? "";
		}
	}

	public string LocalizedName
	{
		get
		{
			return Localization.Get(this.PrefabName, false);
		}
	}

	public string LocalizedEnglishName
	{
		get
		{
			return Localization.Get(this.PrefabName, "english", false);
		}
	}

	public float DensityScore
	{
		get
		{
			if (this.renderingCost != null)
			{
				return (float)this.renderingCost.TotalVertices / 100000f;
			}
			return 0f;
		}
	}

	public bool bSleeperVolumes
	{
		get
		{
			return this.SleeperVolumes != null && this.SleeperVolumes.Count > 0;
		}
	}

	public bool bInfoVolumes
	{
		get
		{
			return this.InfoVolumes != null && this.InfoVolumes.Count > 0;
		}
	}

	public bool bWallVolumes
	{
		get
		{
			return this.WallVolumes != null && this.WallVolumes.Count > 0;
		}
	}

	public bool bTriggerVolumes
	{
		get
		{
			return this.TriggerVolumes != null && this.TriggerVolumes.Count > 0;
		}
	}

	public bool bPOIMarkers
	{
		get
		{
			return this.POIMarkers != null && this.POIMarkers.Count > 0;
		}
	}

	public WorldStats RenderingCostStats
	{
		get
		{
			return this.renderingCost;
		}
		set
		{
			this.renderingCost = value;
		}
	}

	public FastTags<TagGroup.Poi> Tags
	{
		get
		{
			return this.tags;
		}
		set
		{
			this.tags = value;
		}
	}

	public FastTags<TagGroup.Poi> ThemeTags
	{
		get
		{
			return this.themeTags;
		}
		set
		{
			this.themeTags = value;
		}
	}

	public int ThemeRepeatDistance
	{
		get
		{
			return this.themeRepeatDistance;
		}
		set
		{
			this.themeRepeatDistance = value;
		}
	}

	public int DuplicateRepeatDistance
	{
		get
		{
			return this.duplicateRepeatDistance;
		}
		set
		{
			this.duplicateRepeatDistance = value;
		}
	}

	public static PathAbstractions.AbstractedLocation LocationForNewPrefab(string _name, string _prefabsSubfolder = null)
	{
		string launchArgument = GameUtils.GetLaunchArgument("newprefabsmod");
		if (!string.IsNullOrEmpty(launchArgument))
		{
			Mod mod = ModManager.GetMod(launchArgument, true);
			if (mod != null)
			{
				return new PathAbstractions.AbstractedLocation(PathAbstractions.EAbstractedLocationType.Mods, _name, mod.Path + "/Prefabs" + ((_prefabsSubfolder != null) ? ("/" + _prefabsSubfolder) : ""), _prefabsSubfolder, _name, ".tts", false, mod);
			}
			Log.Warning("Argument -newprefabsmod given but mod with name '" + launchArgument + "' not found, ignoring!");
		}
		return new PathAbstractions.AbstractedLocation(PathAbstractions.EAbstractedLocationType.UserDataPath, _name, GameIO.GetUserGameDataDir() + "/LocalPrefabs" + ((_prefabsSubfolder != null) ? ("/" + _prefabsSubfolder) : ""), _prefabsSubfolder, _name, ".tts", false, null);
	}

	public static bool CanSaveIn(PathAbstractions.AbstractedLocation _location)
	{
		return _location.Type != PathAbstractions.EAbstractedLocationType.GameData;
	}

	public Prefab()
	{
	}

	public Prefab(Prefab _other, bool sharedData = false)
	{
		this.size = _other.size;
		if (sharedData)
		{
			this.isCellsDataOwner = false;
			this.blockCells = _other.blockCells;
			this.damageCells = _other.damageCells;
			this.densityCells = _other.densityCells;
			this.textureCells = _other.textureCells;
			this.waterCells = _other.waterCells;
		}
		else
		{
			this.blockCells = _other.blockCells.Clone();
			this.damageCells = _other.damageCells.Clone();
			this.densityCells = _other.densityCells.Clone();
			this.textureCells = _other.textureCells.Clone();
			this.waterCells = _other.waterCells.Clone();
		}
		if (sharedData)
		{
			this.multiBlockParentIndices = _other.multiBlockParentIndices;
			this.decoAllowedBlockIndices = _other.decoAllowedBlockIndices;
		}
		else
		{
			this.multiBlockParentIndices = new List<int>(_other.multiBlockParentIndices);
			this.decoAllowedBlockIndices = new List<int>(_other.decoAllowedBlockIndices);
		}
		this.location = _other.location;
		this.bCopyAirBlocks = _other.bCopyAirBlocks;
		this.bExcludeDistantPOIMesh = _other.bExcludeDistantPOIMesh;
		this.bExcludePOICulling = _other.bExcludePOICulling;
		this.distantPOIYOffset = _other.distantPOIYOffset;
		this.distantPOIOverride = _other.distantPOIOverride;
		this.bAllowTopSoilDecorations = _other.bAllowTopSoilDecorations;
		this.bTraderArea = _other.bTraderArea;
		this.TraderAreaProtect = _other.TraderAreaProtect;
		List<Prefab.PrefabSleeperVolume> sleeperVolumes;
		if (!_other.bSleeperVolumes)
		{
			sleeperVolumes = new List<Prefab.PrefabSleeperVolume>();
		}
		else
		{
			sleeperVolumes = _other.SleeperVolumes.ConvertAll<Prefab.PrefabSleeperVolume>((Prefab.PrefabSleeperVolume _input) => new Prefab.PrefabSleeperVolume(_input));
		}
		this.SleeperVolumes = sleeperVolumes;
		List<Prefab.PrefabTeleportVolume> teleportVolumes;
		if (!_other.bTraderArea)
		{
			teleportVolumes = new List<Prefab.PrefabTeleportVolume>();
		}
		else
		{
			teleportVolumes = _other.TeleportVolumes.ConvertAll<Prefab.PrefabTeleportVolume>((Prefab.PrefabTeleportVolume _input) => new Prefab.PrefabTeleportVolume(_input));
		}
		this.TeleportVolumes = teleportVolumes;
		List<Prefab.PrefabInfoVolume> infoVolumes;
		if (!_other.bInfoVolumes)
		{
			infoVolumes = new List<Prefab.PrefabInfoVolume>();
		}
		else
		{
			infoVolumes = _other.InfoVolumes.ConvertAll<Prefab.PrefabInfoVolume>((Prefab.PrefabInfoVolume _input) => new Prefab.PrefabInfoVolume(_input));
		}
		this.InfoVolumes = infoVolumes;
		List<Prefab.PrefabWallVolume> wallVolumes;
		if (!_other.bWallVolumes)
		{
			wallVolumes = new List<Prefab.PrefabWallVolume>();
		}
		else
		{
			wallVolumes = _other.WallVolumes.ConvertAll<Prefab.PrefabWallVolume>((Prefab.PrefabWallVolume _input) => new Prefab.PrefabWallVolume(_input));
		}
		this.WallVolumes = wallVolumes;
		List<Prefab.PrefabTriggerVolume> triggerVolumes;
		if (!_other.bTriggerVolumes)
		{
			triggerVolumes = new List<Prefab.PrefabTriggerVolume>();
		}
		else
		{
			triggerVolumes = _other.TriggerVolumes.ConvertAll<Prefab.PrefabTriggerVolume>((Prefab.PrefabTriggerVolume _input) => new Prefab.PrefabTriggerVolume(_input));
		}
		this.TriggerVolumes = triggerVolumes;
		this.yOffset = _other.yOffset;
		this.rotationToFaceNorth = _other.rotationToFaceNorth;
		this.allowedBiomes = new List<string>(_other.allowedBiomes);
		this.allowedTownships = new List<string>(_other.allowedTownships);
		this.allowedZones = new List<string>(_other.allowedZones);
		this.tags = new FastTags<TagGroup.Poi>(_other.tags);
		this.themeTags = new FastTags<TagGroup.Poi>(_other.themeTags);
		this.themeRepeatDistance = _other.themeRepeatDistance;
		this.duplicateRepeatDistance = _other.duplicateRepeatDistance;
		this.StaticSpawnerClass = _other.StaticSpawnerClass;
		this.StaticSpawnerSize = _other.StaticSpawnerSize;
		this.StaticSpawnerTrigger = _other.StaticSpawnerTrigger;
		this.questTags = _other.questTags;
		this.DifficultyTier = _other.DifficultyTier;
		this.ShowQuestClearCount = _other.ShowQuestClearCount;
		this.localRotation = _other.localRotation;
		for (int i = 0; i < _other.entities.Count; i++)
		{
			EntityCreationData entityCreationData = _other.entities[i];
			this.entities.Add(entityCreationData.Clone());
		}
		foreach (KeyValuePair<Vector3i, TileEntity> keyValuePair in _other.tileEntities)
		{
			this.tileEntities.Add(keyValuePair.Key, keyValuePair.Value);
		}
		this.POIMarkers = new List<Prefab.Marker>();
		for (int j = 0; j < _other.POIMarkers.Count; j++)
		{
			this.POIMarkers.Add(new Prefab.Marker(_other.POIMarkers[j]));
		}
		this.insidePos = _other.insidePos.Clone();
		foreach (KeyValuePair<Vector3i, BlockTrigger> keyValuePair2 in _other.triggerData)
		{
			this.triggerData.Add(keyValuePair2.Key, keyValuePair2.Value);
		}
		for (int k = 0; k < _other.TriggerLayers.Count; k++)
		{
			this.TriggerLayers.Add(_other.TriggerLayers[k]);
		}
		this.renderingCost = _other.renderingCost;
	}

	public Prefab(Vector3i _size)
	{
		this.size = _size;
		this.localRotation = 0;
		this.InitData();
	}

	public int EstimateOwnedBytes()
	{
		int num = 0;
		if (this.isCellsDataOwner)
		{
			num += IntPtr.Size;
			if (this.blockCells != null)
			{
				int num2;
				int num3;
				int num4;
				int num5;
				int num6;
				this.blockCells.Stats(out num2, out num3, out num4, out num5, out num6);
				num += num5 + num2 * IntPtr.Size;
			}
			num += IntPtr.Size;
			if (this.damageCells != null)
			{
				int num2;
				int num3;
				int num4;
				int num5;
				int num6;
				this.damageCells.Stats(out num2, out num3, out num4, out num5, out num6);
				num += num5 + num2 * IntPtr.Size;
			}
			num += IntPtr.Size;
			if (this.densityCells != null)
			{
				int num2;
				int num3;
				int num4;
				int num5;
				int num6;
				this.densityCells.Stats(out num2, out num3, out num4, out num5, out num6);
				num += num5 + num2 * IntPtr.Size;
			}
			num += IntPtr.Size;
			if (this.textureCells != null)
			{
				int num2;
				int num3;
				int num4;
				int num5;
				int num6;
				this.textureCells.Stats(out num2, out num3, out num4, out num5, out num6);
				num += num5 + num2 * IntPtr.Size;
			}
			num += IntPtr.Size;
			if (this.waterCells != null)
			{
				int num2;
				int num3;
				int num4;
				int num5;
				int num6;
				this.waterCells.Stats(out num2, out num3, out num4, out num5, out num6);
				num += num5 + num2 * IntPtr.Size;
			}
		}
		return num + MemoryTracker.GetSize<byte>(this.TriggerLayers);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void InitData()
	{
		if (!this.isCellsDataOwner)
		{
			Log.Error("InitData failed: Cannot set block data on non-owning Prefab instance.");
			return;
		}
		this.blockCells = new Prefab.Cells<uint>(this.size.y, 0U);
		this.damageCells = new Prefab.Cells<ushort>(this.size.y, 0);
		this.densityCells = new Prefab.Cells<sbyte>(this.size.y, MarchingCubes.DensityAir);
		this.textureCells = new Prefab.Cells<long>(this.size.y, 0L);
		this.waterCells = new Prefab.Cells<WaterValue>(this.size.y, WaterValue.Empty);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void InitTerrainFillers()
	{
		this.terrainFillerType = Block.GetBlockValue(Constants.cTerrainFillerBlockName, false).type;
		this.terrainFiller2Type = Block.GetBlockValue(Constants.cTerrainFiller2BlockName, false).type;
	}

	public Prefab Clone(bool sharedData = false)
	{
		return new Prefab(this, sharedData);
	}

	public int GetLocalRotation()
	{
		return this.localRotation;
	}

	public void SetLocalRotation(int _rot)
	{
		this.localRotation = _rot;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int CoordToOffset(int _localRotation, int _x, int _y, int _z)
	{
		int result;
		switch (_localRotation)
		{
		default:
			result = _x + _y * this.size.x + _z * this.size.x * this.size.y;
			break;
		case 1:
			result = _z + _y * this.size.z + (this.size.x - _x - 1) * this.size.z * this.size.y;
			break;
		case 2:
			result = this.size.x - _x - 1 + _y * this.size.x + (this.size.z - _z - 1) * this.size.x * this.size.y;
			break;
		case 3:
			result = this.size.z - _z - 1 + _y * this.size.z + _x * this.size.z * this.size.y;
			break;
		}
		return result;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void offsetToCoord(int _offset, out int _x, out int _y, out int _z)
	{
		int num = this.size.x * this.size.y;
		_z = _offset / num;
		_offset %= num;
		_y = _offset / this.size.x;
		_x = _offset % this.size.x;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void offsetToCoordRotated(int _offset, out int _x, out int _y, out int _z)
	{
		switch (this.localRotation)
		{
		default:
		{
			int num = this.size.x * this.size.y;
			_z = _offset / num;
			_offset %= num;
			_y = _offset / this.size.x;
			_x = _offset % this.size.x;
			return;
		}
		case 1:
			_x = -(_offset / (this.size.z * this.size.y) - this.size.x + 1);
			_offset %= this.size.z * this.size.y;
			_y = _offset / this.size.z;
			_z = _offset % this.size.z;
			return;
		case 2:
			_z = -(_offset / (this.size.x * this.size.y) - this.size.z + 1);
			_offset %= this.size.x * this.size.y;
			_y = _offset / this.size.x;
			_offset %= this.size.x;
			_x = -(_offset - this.size.x + 1);
			return;
		case 3:
			_x = _offset / (this.size.z * this.size.y);
			_offset %= this.size.z * this.size.y;
			_y = _offset / this.size.z;
			_offset %= this.size.z;
			_z = -(_offset - this.size.z + 1);
			return;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RotateCoords(ref int _x, ref int _z)
	{
		switch (this.localRotation)
		{
		case 1:
		{
			int num = _x;
			_x = _z;
			_z = this.size.x - num - 1;
			return;
		}
		case 2:
			_x = this.size.x - _x - 1;
			_z = this.size.z - _z - 1;
			return;
		case 3:
		{
			int num = _x;
			_x = this.size.z - _z - 1;
			_z = num;
			return;
		}
		default:
			return;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RotateCoords(int _rot, ref int _x, ref int _z)
	{
		switch (_rot)
		{
		case 1:
		{
			int num = _x;
			_x = _z;
			_z = this.size.x - num - 1;
			return;
		}
		case 2:
			_x = this.size.x - _x - 1;
			_z = this.size.z - _z - 1;
			return;
		case 3:
		{
			int num = _x;
			_x = this.size.z - _z - 1;
			_z = num;
			return;
		}
		default:
			return;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void InverseRotateRelative(ref int _x, ref int _z)
	{
		switch (this.localRotation)
		{
		case 1:
		{
			int num = _x;
			_x = -_z;
			_z = num;
			return;
		}
		case 2:
			_x = -_x;
			_z = -_z;
			return;
		case 3:
		{
			int num = _x;
			_x = _z;
			_z = -num;
			return;
		}
		default:
			return;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RotateRelative(ref int _x, ref int _z)
	{
		switch (this.localRotation)
		{
		case 1:
		{
			int num = _x;
			_x = _z;
			_z = -num;
			return;
		}
		case 2:
			_x = -_x;
			_z = -_z;
			return;
		case 3:
		{
			int num = _x;
			_x = -_z;
			_z = num;
			return;
		}
		default:
			return;
		}
	}

	public void SetBlock(int _x, int _y, int _z, BlockValue _bv)
	{
		if (_bv.isWater)
		{
			Log.Warning("Prefabs should no longer store water blocks. Please use SetWater instead");
			this.SetWater(_x, _y, _z, WaterValue.Full);
			return;
		}
		if (!this.isCellsDataOwner)
		{
			Log.Error("SetBlock failed: Cannot set block data on non-owning Prefab instance.");
			return;
		}
		if ((ulong)_x >= (ulong)((long)this.size.x) || (ulong)_y >= (ulong)((long)this.size.y) || (ulong)_z >= (ulong)((long)this.size.z))
		{
			return;
		}
		this.RotateCoords(ref _x, ref _z);
		this.blockCells.SetData(_x, _y, _z, _bv.rawData);
		this.damageCells.SetData(_x, _y, _z, (ushort)_bv.damage);
	}

	public float GetHeight(int _x, int _z, bool _terrainOnly = true)
	{
		for (int i = this.size.y; i >= 0; i--)
		{
			BlockValue block = this.GetBlock(_x, i, _z);
			if (!block.isair && (block.Block.shape.IsTerrain() || !_terrainOnly))
			{
				float num = 1f - (float)((byte)block.Block.Density) / 255f;
				return (float)(i - 1) + num;
			}
		}
		return 0f;
	}

	public BlockValue GetBlock(int _x, int _y, int _z)
	{
		if ((ulong)_x >= (ulong)((long)this.size.x) || (ulong)_y >= (ulong)((long)this.size.y) || (ulong)_z >= (ulong)((long)this.size.z))
		{
			return BlockValue.Air;
		}
		BlockValue air = BlockValue.Air;
		this.RotateCoords(ref _x, ref _z);
		Prefab.Cells<uint>.Cell cell = this.blockCells.GetCell(_x, _y, _z);
		if (cell.a != null)
		{
			air.rawData = cell.Get(_x, _z);
			Prefab.Cells<ushort>.Cell cell2 = this.damageCells.GetCell(_x, _y, _z);
			if (cell2.a != null)
			{
				air.damage = (int)cell2.Get(_x, _z);
			}
			if (!this.isCellsDataOwner && this.localRotation != 0)
			{
				this.ApplyRotation(ref air);
			}
		}
		return air;
	}

	public BlockValue GetBlockNoDamage(int _localRotation, int _x, int _y, int _z)
	{
		BlockValue air = BlockValue.Air;
		this.RotateCoords(_localRotation, ref _x, ref _z);
		Prefab.Cells<uint>.Cell cell = this.blockCells.GetCell(_x, _y, _z);
		if (cell.a != null)
		{
			air.rawData = cell.Get(_x, _z);
			if (!this.isCellsDataOwner && this.localRotation != 0)
			{
				this.ApplyRotation(ref air);
			}
		}
		return air;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ApplyRotation(ref BlockValue bv)
	{
		if (bv.ischild)
		{
			int parentx = bv.parentx;
			int parentz = bv.parentz;
			if (parentx != 0 || parentz != 0)
			{
				this.InverseRotateRelative(ref parentx, ref parentz);
				bv.parentx = parentx;
				bv.parentz = parentz;
				return;
			}
		}
		else
		{
			bv = bv.Block.shape.RotateY(true, bv, this.localRotation);
		}
	}

	public BlockValue GetBlockNoDamage(int _offs)
	{
		return BlockValue.Air;
	}

	public int GetBlockCount()
	{
		return this.size.x * this.size.y * this.size.z;
	}

	public WaterValue GetWater(int _x, int _y, int _z)
	{
		this.RotateCoords(ref _x, ref _z);
		return this.waterCells.GetData(_x, _y, _z);
	}

	public void SetWater(int _x, int _y, int _z, WaterValue _wv)
	{
		if ((ulong)_x >= (ulong)((long)this.size.x) || (ulong)_y >= (ulong)((long)this.size.y) || (ulong)_z >= (ulong)((long)this.size.z))
		{
			return;
		}
		this.RotateCoords(ref _x, ref _z);
		this.waterCells.SetData(_x, _y, _z, _wv);
	}

	public byte GetStab(int relx, int absy, int relz)
	{
		return 0;
	}

	public void SetDensity(int _x, int _y, int _z, sbyte _density)
	{
		this.RotateCoords(ref _x, ref _z);
		this.densityCells.SetData(_x, _y, _z, _density);
	}

	public sbyte GetDensity(int _x, int _y, int _z)
	{
		this.RotateCoords(ref _x, ref _z);
		return this.densityCells.GetData(_x, _y, _z);
	}

	public sbyte GetDensity(int _localRotation, int _x, int _y, int _z)
	{
		this.RotateCoords(_localRotation, ref _x, ref _z);
		return this.densityCells.GetData(_x, _y, _z);
	}

	public void SetTexture(int _x, int _y, int _z, long _fulltexture)
	{
		this.RotateCoords(ref _x, ref _z);
		this.textureCells.SetData(_x, _y, _z, _fulltexture);
	}

	public long GetTexture(int _x, int _y, int _z)
	{
		this.RotateCoords(ref _x, ref _z);
		return this.textureCells.GetData(_x, _y, _z);
	}

	public bool IsInsidePrefab(int _x, int _y, int _z)
	{
		int x;
		int y;
		int z;
		switch (this.localRotation)
		{
		default:
			x = _x;
			y = _y;
			z = _z;
			break;
		case 1:
			x = _z;
			y = _y;
			z = this.size.x - _x - 1;
			break;
		case 2:
			x = this.size.x - _x - 1;
			y = _y;
			z = this.size.z - _z - 1;
			break;
		case 3:
			x = this.size.z - _z - 1;
			y = _y;
			z = _x;
			break;
		}
		return this.insidePos.Contains(x, y, z);
	}

	public void ToggleQuestTag(FastTags<TagGroup.Global> questTag)
	{
		if (this.GetQuestTag(questTag))
		{
			this.questTags = this.questTags.Remove(questTag);
			return;
		}
		this.questTags |= questTag;
	}

	public FastTags<TagGroup.Global> GetQuestTags()
	{
		return new FastTags<TagGroup.Global>(this.questTags);
	}

	public bool GetQuestTag(FastTags<TagGroup.Global> questTag)
	{
		return this.questTags.Test_AllSet(questTag);
	}

	public bool HasAnyQuestTag(FastTags<TagGroup.Global> questTag)
	{
		return this.questTags.Test_AnySet(questTag);
	}

	public bool HasQuestTag()
	{
		return !this.questTags.IsEmpty;
	}

	public TileEntity GetTileEntity(Vector3i _blockPos)
	{
		switch (this.localRotation)
		{
		case 1:
		{
			int x = _blockPos.x;
			_blockPos.x = _blockPos.z;
			_blockPos.z = this.size.x - x - 1;
			break;
		}
		case 2:
			_blockPos.x = this.size.x - _blockPos.x - 1;
			_blockPos.z = this.size.z - _blockPos.z - 1;
			break;
		case 3:
		{
			int x = _blockPos.x;
			_blockPos.x = this.size.z - _blockPos.z - 1;
			_blockPos.z = x;
			break;
		}
		}
		TileEntity result;
		if (this.tileEntities.TryGetValue(_blockPos, out result))
		{
			return result;
		}
		return null;
	}

	public BlockTrigger GetBlockTrigger(Vector3i _blockPos)
	{
		switch (this.localRotation)
		{
		case 1:
		{
			int x = _blockPos.x;
			_blockPos.x = _blockPos.z;
			_blockPos.z = this.size.x - x - 1;
			break;
		}
		case 2:
			_blockPos.x = this.size.x - _blockPos.x - 1;
			_blockPos.z = this.size.z - _blockPos.z - 1;
			break;
		case 3:
		{
			int x = _blockPos.x;
			_blockPos.x = this.size.z - _blockPos.z - 1;
			_blockPos.z = x;
			break;
		}
		}
		BlockTrigger result;
		if (this.triggerData.TryGetValue(_blockPos, out result))
		{
			return result;
		}
		return null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ReadFromProperties()
	{
		this.bCopyAirBlocks = this.properties.GetBool("CopyAirBlocks");
		this.bExcludeDistantPOIMesh = this.properties.GetBool("ExcludeDistantPOIMesh");
		this.bExcludePOICulling = this.properties.GetBool("ExcludePOICulling");
		this.distantPOIYOffset = this.properties.GetFloat("DistantPOIYOffset");
		this.properties.ParseString("DistantPOIOverride", ref this.distantPOIOverride);
		this.bAllowTopSoilDecorations = this.properties.GetBool("AllowTopSoilDecorations");
		this.editorGroups.Clear();
		if (this.properties.Values.ContainsKey("EditorGroups"))
		{
			this.editorGroups.AddRange(this.properties.GetStringValue("EditorGroups").Split(',', StringSplitOptions.None));
			for (int i = 0; i < this.editorGroups.Count; i++)
			{
				this.editorGroups[i] = this.editorGroups[i].Trim();
			}
		}
		if (this.properties.Values.ContainsKey("DifficultyTier"))
		{
			this.DifficultyTier = (byte)this.properties.GetInt("DifficultyTier");
		}
		this.properties.ParseInt("ShowQuestClearCount", ref this.ShowQuestClearCount);
		this.bTraderArea = this.properties.GetBool("TraderArea");
		this.TraderAreaProtect = (this.properties.Values.ContainsKey("TraderAreaProtect") ? StringParsers.ParseVector3i(this.properties.Values["TraderAreaProtect"], 0, -1, false) : Vector3i.zero);
		this.SleeperVolumes = new List<Prefab.PrefabSleeperVolume>();
		DictionarySave<string, string> values = this.properties.Values;
		if (values.ContainsKey("SleeperVolumeSize") && values.ContainsKey("SleeperVolumeStart"))
		{
			List<Vector3i> list = StringParsers.ParseList<Vector3i>(values["SleeperVolumeSize"], '#', (string _s, int _start, int _end) => StringParsers.ParseVector3i(_s, _start, _end, false));
			List<Vector3i> list2 = StringParsers.ParseList<Vector3i>(values["SleeperVolumeStart"], '#', (string _s, int _start, int _end) => StringParsers.ParseVector3i(_s, _start, _end, false));
			List<string> list3 = null;
			string text;
			if (values.TryGetValue("SleeperVolumeGroupId", out text))
			{
				list3 = new List<string>(text.Split(',', StringSplitOptions.None));
			}
			List<string> list4 = null;
			if (values.TryGetValue("SleeperVolumeGroup", out text))
			{
				list4 = new List<string>(text.Split(',', StringSplitOptions.None));
			}
			List<bool> list5;
			if (!values.ContainsKey("SleeperIsLootVolume"))
			{
				list5 = new List<bool>();
			}
			else
			{
				list5 = StringParsers.ParseList<bool>(values["SleeperIsLootVolume"], ',', (string _s, int _start, int _end) => StringParsers.ParseBool(_s, _start, _end, true));
			}
			List<bool> list6 = list5;
			List<bool> list7;
			if (!values.ContainsKey("SleeperIsQuestExclude"))
			{
				list7 = new List<bool>();
			}
			else
			{
				list7 = StringParsers.ParseList<bool>(values["SleeperIsQuestExclude"], ',', (string _s, int _start, int _end) => StringParsers.ParseBool(_s, _start, _end, true));
			}
			List<bool> list8 = list7;
			List<int> list9 = null;
			if (values.TryGetValue("SleeperVolumeFlags", out text))
			{
				list9 = StringParsers.ParseList<int>(text, ',', (string _s, int _start, int _end) => StringParsers.ParseSInt32(_s, _start, _end, NumberStyles.HexNumber));
			}
			List<string> list10 = null;
			if (values.TryGetValue("SleeperVolumeTriggeredBy", out text))
			{
				list10 = StringParsers.ParseList<string>(text, '#', (string _s, int _start, int _end) => _s.Substring(_start, (_end == -1) ? (_s.Length - _start) : (_end + 1 - _start)));
			}
			for (int j = 0; j < list2.Count; j++)
			{
				Vector3i startPos = list2[j];
				Vector3i vector3i = (j < list.Count) ? list[j] : Vector3i.one;
				short groupId = 0;
				string text2 = "???";
				short spawnMin = 5;
				short spawnMax = 5;
				if (list3 != null)
				{
					groupId = StringParsers.ParseSInt16(list3[j], 0, -1, NumberStyles.Integer);
				}
				if (list4 != null)
				{
					if (list4.Count == list2.Count)
					{
						text2 = list4[j];
					}
					else if (list4.Count == list2.Count * 3)
					{
						int num = j * 3;
						text2 = list4[num];
						spawnMin = StringParsers.ParseSInt16(list4[num + 1], 0, -1, NumberStyles.Integer);
						spawnMax = StringParsers.ParseSInt16(list4[num + 2], 0, -1, NumberStyles.Integer);
					}
					text2 = GameStageGroup.CleanName(text2);
				}
				bool isPriority = j < list6.Count && list6[j];
				bool isQuestExclude = j < list8.Count && list8[j];
				int flags = 0;
				if (list9 != null && j < list9.Count)
				{
					flags = list9[j];
				}
				Prefab.PrefabSleeperVolume prefabSleeperVolume = new Prefab.PrefabSleeperVolume();
				prefabSleeperVolume.Use(startPos, vector3i, groupId, text2, isPriority, isQuestExclude, (int)spawnMin, (int)spawnMax, flags);
				string @string = this.properties.GetString("SVS" + j.ToString());
				if (@string.Length > 0)
				{
					prefabSleeperVolume.minScript = @string;
				}
				if (list10 != null && list10[j].Trim() != "")
				{
					prefabSleeperVolume.triggeredByIndices = StringParsers.ParseList<byte>(list10[j], ',', (string _s, int _start, int _end) => StringParsers.ParseUInt8(_s, _start, _end, NumberStyles.Integer));
				}
				this.SleeperVolumes.Add(prefabSleeperVolume);
			}
		}
		this.TeleportVolumes = new List<Prefab.PrefabTeleportVolume>();
		if (values.ContainsKey("TeleportVolumeSize") && values.ContainsKey("TeleportVolumeStart"))
		{
			List<Vector3i> list11 = StringParsers.ParseList<Vector3i>(values["TeleportVolumeSize"], '#', (string _s, int _start, int _end) => StringParsers.ParseVector3i(_s, _start, _end, false));
			List<Vector3i> list12 = StringParsers.ParseList<Vector3i>(values["TeleportVolumeStart"], '#', (string _s, int _start, int _end) => StringParsers.ParseVector3i(_s, _start, _end, false));
			for (int k = 0; k < list12.Count; k++)
			{
				Vector3i startPos2 = list12[k];
				Vector3i vector3i2 = (k < list11.Count) ? list11[k] : Vector3i.one;
				Prefab.PrefabTeleportVolume prefabTeleportVolume = new Prefab.PrefabTeleportVolume();
				prefabTeleportVolume.Use(startPos2, vector3i2);
				this.TeleportVolumes.Add(prefabTeleportVolume);
			}
		}
		this.InfoVolumes = new List<Prefab.PrefabInfoVolume>();
		if (values.ContainsKey("InfoVolumeSize") && values.ContainsKey("InfoVolumeStart"))
		{
			List<Vector3i> list13 = StringParsers.ParseList<Vector3i>(values["InfoVolumeSize"], '#', (string _s, int _start, int _end) => StringParsers.ParseVector3i(_s, _start, _end, false));
			List<Vector3i> list14 = StringParsers.ParseList<Vector3i>(values["InfoVolumeStart"], '#', (string _s, int _start, int _end) => StringParsers.ParseVector3i(_s, _start, _end, false));
			for (int l = 0; l < list14.Count; l++)
			{
				Vector3i startPos3 = list14[l];
				Vector3i vector3i3 = (l < list13.Count) ? list13[l] : Vector3i.one;
				Prefab.PrefabInfoVolume prefabInfoVolume = new Prefab.PrefabInfoVolume();
				prefabInfoVolume.Use(startPos3, vector3i3);
				this.InfoVolumes.Add(prefabInfoVolume);
			}
		}
		this.WallVolumes = new List<Prefab.PrefabWallVolume>();
		if (values.ContainsKey("WallVolumeSize") && values.ContainsKey("WallVolumeStart"))
		{
			List<Vector3i> list15 = StringParsers.ParseList<Vector3i>(values["WallVolumeSize"], '#', (string _s, int _start, int _end) => StringParsers.ParseVector3i(_s, _start, _end, false));
			List<Vector3i> list16 = StringParsers.ParseList<Vector3i>(values["WallVolumeStart"], '#', (string _s, int _start, int _end) => StringParsers.ParseVector3i(_s, _start, _end, false));
			for (int m = 0; m < list16.Count; m++)
			{
				Vector3i startPos4 = list16[m];
				Vector3i vector3i4 = (m < list15.Count) ? list15[m] : Vector3i.one;
				Prefab.PrefabWallVolume prefabWallVolume = new Prefab.PrefabWallVolume();
				prefabWallVolume.Use(startPos4, vector3i4);
				this.WallVolumes.Add(prefabWallVolume);
			}
		}
		this.TriggerVolumes = new List<Prefab.PrefabTriggerVolume>();
		if (values.ContainsKey("TriggerVolumeSize") && values.ContainsKey("TriggerVolumeStart"))
		{
			List<Vector3i> list17 = StringParsers.ParseList<Vector3i>(values["TriggerVolumeSize"], '#', (string _s, int _start, int _end) => StringParsers.ParseVector3i(_s, _start, _end, false));
			List<Vector3i> list18 = StringParsers.ParseList<Vector3i>(values["TriggerVolumeStart"], '#', (string _s, int _start, int _end) => StringParsers.ParseVector3i(_s, _start, _end, false));
			List<string> list19 = StringParsers.ParseList<string>(values["TriggerVolumeTriggers"], '#', (string _s, int _start, int _end) => _s.Substring(_start, (_end == -1) ? (_s.Length - _start) : (_end + 1 - _start)));
			for (int n = 0; n < list18.Count; n++)
			{
				Vector3i startPos5 = list18[n];
				Vector3i vector3i5 = (n < list17.Count) ? list17[n] : Vector3i.one;
				Prefab.PrefabTriggerVolume prefabTriggerVolume = new Prefab.PrefabTriggerVolume();
				prefabTriggerVolume.Use(startPos5, vector3i5);
				if (list19[n].Trim() != "")
				{
					prefabTriggerVolume.TriggersIndices = StringParsers.ParseList<byte>(list19[n], ',', (string _s, int _start, int _end) => StringParsers.ParseUInt8(_s, _start, _end, NumberStyles.Integer));
				}
				this.TriggerVolumes.Add(prefabTriggerVolume);
				this.HandleAddingTriggerLayers(prefabTriggerVolume);
			}
		}
		if (values.ContainsKey("POIMarkerSize") && values.ContainsKey("POIMarkerStart"))
		{
			this.POIMarkers.Clear();
			List<Vector3i> list20 = StringParsers.ParseList<Vector3i>(values["POIMarkerSize"], '#', (string _s, int _start, int _end) => StringParsers.ParseVector3i(_s, _start, _end, false));
			List<Vector3i> list21 = StringParsers.ParseList<Vector3i>(values["POIMarkerStart"], '#', (string _s, int _start, int _end) => StringParsers.ParseVector3i(_s, _start, _end, false));
			List<Prefab.Marker.MarkerTypes> list22 = new List<Prefab.Marker.MarkerTypes>();
			if (values.ContainsKey("POIMarkerType"))
			{
				string[] array = values["POIMarkerType"].Split(',', StringSplitOptions.None);
				for (int num2 = 0; num2 < array.Length; num2++)
				{
					Prefab.Marker.MarkerTypes item;
					if (Enum.TryParse<Prefab.Marker.MarkerTypes>(array[num2], true, out item))
					{
						list22.Add(item);
					}
				}
			}
			List<FastTags<TagGroup.Poi>> list23 = new List<FastTags<TagGroup.Poi>>();
			if (values.ContainsKey("POIMarkerTags"))
			{
				string[] array = values["POIMarkerTags"].Split('#', StringSplitOptions.None);
				for (int num3 = 0; num3 < array.Length; num3++)
				{
					if (array[num3].Length > 0)
					{
						list23.Add(FastTags<TagGroup.Poi>.Parse(array[num3]));
					}
					else
					{
						list23.Add(FastTags<TagGroup.Poi>.none);
					}
				}
			}
			List<string> list24 = new List<string>();
			if (values.ContainsKey("POIMarkerGroup"))
			{
				list24.AddRange(values["POIMarkerGroup"].Split(',', StringSplitOptions.None));
			}
			List<string> list25 = new List<string>();
			if (values.ContainsKey("POIMarkerPartToSpawn"))
			{
				list25.AddRange(values["POIMarkerPartToSpawn"].Split(',', StringSplitOptions.None));
			}
			List<int> list26 = new List<int>();
			if (values.ContainsKey("POIMarkerPartRotations"))
			{
				string[] array = values["POIMarkerPartRotations"].Split(',', StringSplitOptions.None);
				string[] array2 = array;
				for (int num4 = 0; num4 < array2.Length; num4++)
				{
					int item2;
					if (StringParsers.TryParseSInt32(array2[num4], out item2, 0, -1, NumberStyles.Integer))
					{
						list26.Add(item2);
					}
					else
					{
						list26.Add(0);
					}
				}
			}
			List<float> list27 = new List<float>();
			if (values.ContainsKey("POIMarkerPartSpawnChance"))
			{
				string[] array = values["POIMarkerPartSpawnChance"].Split(',', StringSplitOptions.None);
				string[] array2 = array;
				for (int num4 = 0; num4 < array2.Length; num4++)
				{
					float item3;
					if (StringParsers.TryParseFloat(array2[num4], out item3, 0, -1, NumberStyles.Any))
					{
						list27.Add(item3);
					}
					else
					{
						list27.Add(0f);
					}
				}
			}
			for (int num5 = 0; num5 < list21.Count; num5++)
			{
				Prefab.Marker marker = new Prefab.Marker();
				marker.Start = list21[num5];
				if (num5 < list20.Count)
				{
					marker.Size = list20[num5];
				}
				if (num5 < list22.Count)
				{
					marker.MarkerType = list22[num5];
				}
				if (num5 < list24.Count)
				{
					marker.GroupName = list24[num5];
				}
				if (num5 < list23.Count)
				{
					marker.Tags = list23[num5];
				}
				if (num5 < list25.Count)
				{
					marker.PartToSpawn = list25[num5];
				}
				if (num5 < list26.Count)
				{
					marker.Rotations = (byte)list26[num5];
				}
				if (num5 < list27.Count)
				{
					marker.PartChanceToSpawn = list27[num5];
				}
				this.POIMarkers.Add(marker);
			}
		}
		this.yOffset = this.properties.GetInt("YOffset");
		if (this.size == Vector3i.zero && values.ContainsKey("PrefabSize"))
		{
			this.size = StringParsers.ParseVector3i(this.properties.Values["PrefabSize"], 0, -1, false);
		}
		this.rotationToFaceNorth = this.properties.GetInt("RotationToFaceNorth");
		if (this.properties.Values.ContainsKey("Tags"))
		{
			this.tags = FastTags<TagGroup.Poi>.Parse(this.properties.Values["Tags"].Replace(" ", ""));
		}
		if (this.properties.Values.ContainsKey("ThemeTags"))
		{
			this.themeTags = FastTags<TagGroup.Poi>.Parse(this.properties.Values["ThemeTags"].Replace(" ", ""));
		}
		if (this.properties.Values.ContainsKey("ThemeRepeatDistance"))
		{
			this.themeRepeatDistance = StringParsers.ParseSInt32(this.properties.Values["ThemeRepeatDistance"], 0, -1, NumberStyles.Integer);
		}
		if (this.properties.Values.ContainsKey("DuplicateRepeatDistance"))
		{
			this.duplicateRepeatDistance = StringParsers.ParseSInt32(this.properties.Values["DuplicateRepeatDistance"], 0, -1, NumberStyles.Integer);
		}
		this.indexedBlockOffsets.Clear();
		if (this.properties.Classes.ContainsKey("IndexedBlockOffsets"))
		{
			foreach (KeyValuePair<string, DynamicProperties> keyValuePair in this.properties.Classes["IndexedBlockOffsets"].Classes.Dict)
			{
				if (keyValuePair.Value.Values.Dict.Count > 0)
				{
					List<Vector3i> list28 = new List<Vector3i>();
					this.indexedBlockOffsets[keyValuePair.Key] = list28;
					foreach (KeyValuePair<string, string> keyValuePair2 in keyValuePair.Value.Values.Dict)
					{
						list28.Add(StringParsers.ParseVector3i(keyValuePair.Value.Values[keyValuePair2.Key], 0, -1, false));
					}
				}
			}
		}
		if (this.properties.Values.ContainsKey("QuestTags"))
		{
			this.questTags = FastTags<TagGroup.Global>.Parse(this.properties.Values["QuestTags"]);
		}
		this.properties.ParseString("StaticSpawner.Class", ref this.StaticSpawnerClass);
		if (this.properties.Values.ContainsKey("StaticSpawner.Size"))
		{
			string[] array3 = this.properties.Values["StaticSpawner.Size"].Replace(" ", "").Split(',', StringSplitOptions.None);
			int x = int.Parse(array3[0]);
			int y = int.Parse(array3[1]);
			int z = int.Parse(array3[2]);
			this.StaticSpawnerSize = new Vector3i(x, y, z);
		}
		this.properties.ParseInt("StaticSpawner.Trigger", ref this.StaticSpawnerTrigger);
		if (this.properties.Values.ContainsKey("AllowedTownships"))
		{
			this.allowedTownships.Clear();
			foreach (string text3 in this.properties.Values["AllowedTownships"].Replace(" ", "").Split(',', StringSplitOptions.None))
			{
				this.allowedTownships.Add(text3.ToLower());
			}
		}
		if (this.properties.Values.ContainsKey("AllowedBiomes"))
		{
			this.allowedBiomes.Clear();
			foreach (string text4 in this.properties.Values["AllowedBiomes"].Replace(" ", "").Split(',', StringSplitOptions.None))
			{
				this.allowedBiomes.Add(text4.ToLower());
			}
		}
		if (this.properties.Values.ContainsKey("Zoning"))
		{
			this.allowedZones.Clear();
			string[] array4 = this.properties.Values["Zoning"].Split(',', StringSplitOptions.None);
			for (int num6 = 0; num6 < array4.Length; num6++)
			{
				this.AddAllowedZone(array4[num6].Trim());
			}
		}
		else
		{
			this.allowedZones.Add("none");
		}
		if (this.properties.Classes.ContainsKey("Stats"))
		{
			this.renderingCost = WorldStats.FromProperties(this.properties.Classes["Stats"]);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void writeToProperties()
	{
		this.properties.Values["CopyAirBlocks"] = this.bCopyAirBlocks.ToString();
		this.properties.Values["ExcludeDistantPOIMesh"] = this.bExcludeDistantPOIMesh.ToString();
		this.properties.Values["ExcludePOICulling"] = this.bExcludePOICulling.ToString();
		this.properties.Values["DistantPOIYOffset"] = this.distantPOIYOffset.ToCultureInvariantString();
		if (this.distantPOIOverride != null)
		{
			this.properties.Values["DistantPOIOverride"] = this.distantPOIOverride;
		}
		this.properties.Values.Remove("EditorGroups");
		if (this.editorGroups.Count > 0)
		{
			string text = string.Empty;
			for (int i = 0; i < this.editorGroups.Count; i++)
			{
				text = text + this.editorGroups[i] + ((i < this.editorGroups.Count - 1) ? ", " : string.Empty);
			}
			this.properties.Values["EditorGroups"] = text;
		}
		this.properties.Values["AllowTopSoilDecorations"] = this.bAllowTopSoilDecorations.ToString();
		this.properties.Values["DifficultyTier"] = this.DifficultyTier.ToString();
		this.properties.Values["ShowQuestClearCount"] = this.ShowQuestClearCount.ToString();
		this.properties.Values["TraderArea"] = this.bTraderArea.ToString();
		if (this.bTraderArea)
		{
			StringBuilder stringBuilder = new StringBuilder();
			StringBuilder stringBuilder2 = new StringBuilder();
			foreach (Prefab.PrefabTeleportVolume prefabTeleportVolume in this.TeleportVolumes)
			{
				if (stringBuilder.Length > 0)
				{
					stringBuilder.Append('#');
					stringBuilder2.Append('#');
				}
				stringBuilder.Append(prefabTeleportVolume.size.ToString());
				stringBuilder2.Append(prefabTeleportVolume.startPos.ToString());
			}
			this.properties.Values["TeleportVolumeSize"] = stringBuilder.ToString();
			this.properties.Values["TeleportVolumeStart"] = stringBuilder2.ToString();
		}
		else
		{
			this.properties.Values.Remove("TeleportVolumeSize");
			this.properties.Values.Remove("TeleportVolumeStart");
		}
		foreach (KeyValuePair<string, string> keyValuePair in this.properties.Values.Dict)
		{
			if (keyValuePair.Key.StartsWith("SVS"))
			{
				this.properties.Values.MarkToRemove(keyValuePair.Key);
			}
		}
		this.properties.Values.RemoveAllMarked(delegate(string _key)
		{
			this.properties.Values.Remove(_key);
		});
		bool flag = true;
		if (this.bSleeperVolumes)
		{
			StringBuilder stringBuilder3 = new StringBuilder();
			StringBuilder stringBuilder4 = new StringBuilder();
			StringBuilder stringBuilder5 = new StringBuilder();
			StringBuilder stringBuilder6 = new StringBuilder();
			StringBuilder stringBuilder7 = new StringBuilder();
			StringBuilder stringBuilder8 = new StringBuilder();
			StringBuilder stringBuilder9 = new StringBuilder();
			StringBuilder stringBuilder10 = new StringBuilder();
			foreach (Prefab.PrefabSleeperVolume prefabSleeperVolume in this.SleeperVolumes)
			{
				if (prefabSleeperVolume.used)
				{
					if (stringBuilder3.Length > 0)
					{
						stringBuilder3.Append('#');
						stringBuilder4.Append('#');
						stringBuilder5.Append(',');
						stringBuilder6.Append(',');
						stringBuilder7.Append(',');
						stringBuilder8.Append(',');
						stringBuilder9.Append(',');
						stringBuilder10.Append('#');
					}
					stringBuilder3.Append(prefabSleeperVolume.size.ToString());
					stringBuilder4.Append(prefabSleeperVolume.startPos.ToString());
					stringBuilder5.Append(prefabSleeperVolume.groupId);
					stringBuilder6.Append(prefabSleeperVolume.groupName);
					stringBuilder6.Append(',');
					stringBuilder6.Append(prefabSleeperVolume.spawnCountMin.ToString());
					stringBuilder6.Append(',');
					stringBuilder6.Append(prefabSleeperVolume.spawnCountMax.ToString());
					stringBuilder7.Append(prefabSleeperVolume.isPriority.ToString());
					stringBuilder8.Append(prefabSleeperVolume.isQuestExclude.ToString());
					stringBuilder9.Append(prefabSleeperVolume.flags.ToString("x"));
					for (int j = 0; j < prefabSleeperVolume.triggeredByIndices.Count; j++)
					{
						if (j > 0)
						{
							stringBuilder10.Append(',');
						}
						stringBuilder10.Append(prefabSleeperVolume.triggeredByIndices[j].ToString());
					}
					if (prefabSleeperVolume.triggeredByIndices.Count == 0)
					{
						stringBuilder10.Append(" ");
					}
				}
			}
			if (stringBuilder3.Length > 0)
			{
				flag = false;
				this.properties.Values["SleeperVolumeSize"] = stringBuilder3.ToString();
				this.properties.Values["SleeperVolumeStart"] = stringBuilder4.ToString();
				this.properties.Values["SleeperVolumeGroupId"] = stringBuilder5.ToString();
				this.properties.Values["SleeperVolumeGroup"] = stringBuilder6.ToString();
				this.properties.Values["SleeperIsLootVolume"] = stringBuilder7.ToString();
				this.properties.Values["SleeperIsQuestExclude"] = stringBuilder8.ToString();
				this.properties.Values["SleeperVolumeFlags"] = stringBuilder9.ToString();
				this.properties.Values["SleeperVolumeTriggeredBy"] = stringBuilder10.ToString();
				int num = 0;
				for (int k = 0; k < this.SleeperVolumes.Count; k++)
				{
					Prefab.PrefabSleeperVolume prefabSleeperVolume2 = this.SleeperVolumes[k];
					if (prefabSleeperVolume2.used)
					{
						if (prefabSleeperVolume2.minScript != null)
						{
							this.properties.Values["SVS" + num.ToString()] = prefabSleeperVolume2.minScript;
						}
						num++;
					}
				}
			}
		}
		if (flag)
		{
			this.properties.Values.Remove("SleeperVolumeSize");
			this.properties.Values.Remove("SleeperVolumeStart");
			this.properties.Values.Remove("SleeperVolumeGroupId");
			this.properties.Values.Remove("SleeperVolumeGroup");
			this.properties.Values.Remove("SleeperIsLootVolume");
			this.properties.Values.Remove("SleeperIsQuestExclude");
			this.properties.Values.Remove("SleeperVolumeFlags");
			this.properties.Values.Remove("SleeperVolumeTriggeredBy");
		}
		if (this.bInfoVolumes)
		{
			StringBuilder stringBuilder11 = new StringBuilder();
			StringBuilder stringBuilder12 = new StringBuilder();
			foreach (Prefab.PrefabInfoVolume prefabInfoVolume in this.InfoVolumes)
			{
				if (stringBuilder11.Length > 0)
				{
					stringBuilder11.Append('#');
					stringBuilder12.Append('#');
				}
				stringBuilder11.Append(prefabInfoVolume.size.ToString());
				stringBuilder12.Append(prefabInfoVolume.startPos.ToString());
			}
			this.properties.Values["InfoVolumeSize"] = stringBuilder11.ToString();
			this.properties.Values["InfoVolumeStart"] = stringBuilder12.ToString();
		}
		else
		{
			this.properties.Values.Remove("InfoVolumeSize");
			this.properties.Values.Remove("InfoVolumeStart");
		}
		if (this.bWallVolumes)
		{
			StringBuilder stringBuilder13 = new StringBuilder();
			StringBuilder stringBuilder14 = new StringBuilder();
			foreach (Prefab.PrefabWallVolume prefabWallVolume in this.WallVolumes)
			{
				if (stringBuilder13.Length > 0)
				{
					stringBuilder13.Append('#');
					stringBuilder14.Append('#');
				}
				stringBuilder13.Append(prefabWallVolume.size.ToString());
				stringBuilder14.Append(prefabWallVolume.startPos.ToString());
			}
			this.properties.Values["WallVolumeSize"] = stringBuilder13.ToString();
			this.properties.Values["WallVolumeStart"] = stringBuilder14.ToString();
		}
		else
		{
			this.properties.Values.Remove("WallVolumeSize");
			this.properties.Values.Remove("WallVolumeStart");
		}
		if (this.bTriggerVolumes)
		{
			StringBuilder stringBuilder15 = new StringBuilder();
			StringBuilder stringBuilder16 = new StringBuilder();
			StringBuilder stringBuilder17 = new StringBuilder();
			foreach (Prefab.PrefabTriggerVolume prefabTriggerVolume in this.TriggerVolumes)
			{
				if (stringBuilder15.Length > 0)
				{
					stringBuilder15.Append('#');
					stringBuilder16.Append('#');
					stringBuilder17.Append('#');
				}
				for (int l = 0; l < prefabTriggerVolume.TriggersIndices.Count; l++)
				{
					if (l > 0)
					{
						stringBuilder17.Append(',');
					}
					stringBuilder17.Append(prefabTriggerVolume.TriggersIndices[l].ToString());
				}
				if (prefabTriggerVolume.TriggersIndices.Count == 0)
				{
					stringBuilder17.Append(" ");
				}
				stringBuilder15.Append(prefabTriggerVolume.size.ToString());
				stringBuilder16.Append(prefabTriggerVolume.startPos.ToString());
			}
			this.properties.Values["TriggerVolumeSize"] = stringBuilder15.ToString();
			this.properties.Values["TriggerVolumeStart"] = stringBuilder16.ToString();
			this.properties.Values["TriggerVolumeTriggers"] = stringBuilder17.ToString();
		}
		else
		{
			this.properties.Values.Remove("TriggerVolumeSize");
			this.properties.Values.Remove("TriggerVolumeStart");
			this.properties.Values.Remove("TriggerVolumeTriggers");
		}
		if (this.bPOIMarkers)
		{
			StringBuilder stringBuilder18 = new StringBuilder();
			StringBuilder stringBuilder19 = new StringBuilder();
			StringBuilder stringBuilder20 = new StringBuilder();
			StringBuilder stringBuilder21 = new StringBuilder();
			StringBuilder stringBuilder22 = new StringBuilder();
			StringBuilder stringBuilder23 = new StringBuilder();
			StringBuilder stringBuilder24 = new StringBuilder();
			StringBuilder stringBuilder25 = new StringBuilder();
			foreach (Prefab.Marker marker in this.POIMarkers)
			{
				if (stringBuilder19.Length > 0)
				{
					stringBuilder18.Append('#');
					stringBuilder19.Append('#');
					stringBuilder20.Append(',');
					stringBuilder21.Append('#');
					stringBuilder22.Append(',');
					stringBuilder23.Append(',');
					stringBuilder24.Append(',');
					stringBuilder25.Append(',');
				}
				stringBuilder18.Append(marker.Size.ToString());
				stringBuilder19.Append(marker.Start.ToString());
				stringBuilder20.Append(marker.GroupName);
				stringBuilder21.Append(marker.Tags.ToString());
				stringBuilder22.Append(marker.MarkerType.ToString());
				stringBuilder23.Append(marker.PartToSpawn);
				stringBuilder24.Append(marker.Rotations.ToString());
				stringBuilder25.Append(marker.PartChanceToSpawn.ToString());
			}
			this.properties.Values["POIMarkerSize"] = stringBuilder18.ToString();
			this.properties.Values["POIMarkerStart"] = stringBuilder19.ToString();
			this.properties.Values["POIMarkerGroup"] = stringBuilder20.ToString();
			this.properties.Values["POIMarkerTags"] = stringBuilder21.ToString();
			this.properties.Values["POIMarkerType"] = stringBuilder22.ToString();
			this.properties.Values["POIMarkerPartToSpawn"] = stringBuilder23.ToString();
			this.properties.Values["POIMarkerPartRotations"] = stringBuilder24.ToString();
			this.properties.Values["POIMarkerPartSpawnChance"] = stringBuilder25.ToString();
		}
		if (this.yOffset != 0)
		{
			this.properties.Values["YOffset"] = this.yOffset.ToString();
		}
		else
		{
			this.properties.Values.Remove("YOffset");
		}
		this.properties.Values["PrefabSize"] = this.size.ToString();
		this.properties.Values["RotationToFaceNorth"] = this.rotationToFaceNorth.ToString();
		if (this.StaticSpawnerClass != null)
		{
			this.properties.Values["StaticSpawner.Class"] = this.StaticSpawnerClass;
		}
		else
		{
			this.properties.Values.Remove("StaticSpawner.Class");
		}
		if (this.StaticSpawnerSize != Vector3i.zero)
		{
			this.properties.Values["StaticSpawner.Size"] = string.Concat(new string[]
			{
				this.StaticSpawnerSize.x.ToString(),
				",",
				this.StaticSpawnerSize.y.ToString(),
				",",
				this.StaticSpawnerSize.z.ToString()
			});
		}
		else
		{
			this.properties.Values.Remove("StaticSpawner.Size");
		}
		if (this.StaticSpawnerTrigger > 0)
		{
			this.properties.Values["StaticSpawner.Trigger"] = this.StaticSpawnerTrigger.ToString();
		}
		else
		{
			this.properties.Values.Remove("StaticSpawner.Trigger");
		}
		string text2 = "";
		for (int m = 0; m < this.allowedTownships.Count; m++)
		{
			text2 = text2 + this.allowedTownships[m] + ((m < this.allowedTownships.Count - 1) ? "," : "");
		}
		if (text2.Length > 0)
		{
			this.properties.Values["AllowedTownships"] = text2;
		}
		else
		{
			this.properties.Values.Remove("AllowedTownships");
		}
		text2 = "";
		for (int n = 0; n < this.allowedBiomes.Count; n++)
		{
			text2 = text2 + this.allowedBiomes[n] + ((n < this.allowedBiomes.Count - 1) ? "," : "");
		}
		if (text2.Length > 0)
		{
			this.properties.Values["AllowedBiomes"] = text2;
		}
		else
		{
			this.properties.Values.Remove("AllowedBiomes");
		}
		if (this.tags.ToString() != "")
		{
			this.properties.Values["Tags"] = this.tags.ToString();
		}
		else
		{
			this.properties.Values.Remove("Tags");
		}
		if (this.themeTags.ToString() != "")
		{
			this.properties.Values["ThemeTags"] = this.themeTags.ToString();
		}
		else
		{
			this.properties.Values.Remove("ThemeTags");
		}
		if (this.themeRepeatDistance != 300)
		{
			this.properties.Values["ThemeRepeatDistance"] = this.themeRepeatDistance.ToString(NumberFormatInfo.InvariantInfo);
		}
		else
		{
			this.properties.Values.Remove("ThemeRepeatDistance");
		}
		if (this.duplicateRepeatDistance != 1000)
		{
			this.properties.Values["DuplicateRepeatDistance"] = this.duplicateRepeatDistance.ToString(NumberFormatInfo.InvariantInfo);
		}
		else
		{
			this.properties.Values.Remove("DuplicateRepeatDistance");
		}
		if (this.indexedBlockOffsets.Any((KeyValuePair<string, List<Vector3i>> _pair) => _pair.Value.Count > 0))
		{
			DynamicProperties dynamicProperties = new DynamicProperties();
			this.properties.Classes["IndexedBlockOffsets"] = dynamicProperties;
			using (Dictionary<string, List<Vector3i>>.Enumerator enumerator8 = this.indexedBlockOffsets.GetEnumerator())
			{
				while (enumerator8.MoveNext())
				{
					KeyValuePair<string, List<Vector3i>> keyValuePair2 = enumerator8.Current;
					if (keyValuePair2.Value.Count > 0)
					{
						DynamicProperties dynamicProperties2 = new DynamicProperties();
						dynamicProperties.Classes[keyValuePair2.Key] = dynamicProperties2;
						for (int num2 = 0; num2 < keyValuePair2.Value.Count; num2++)
						{
							dynamicProperties2.Values[num2.ToString()] = keyValuePair2.Value[num2].ToString();
						}
					}
				}
				goto IL_125D;
			}
		}
		this.properties.Classes.Remove("IndexedBlockOffsets");
		IL_125D:
		if (!this.questTags.IsEmpty)
		{
			text2 = this.questTags.ToString();
		}
		if (text2.Length > 0)
		{
			this.properties.Values["QuestTags"] = text2;
		}
		else
		{
			this.properties.Values.Remove("QuestTags");
		}
		this.properties.Values.Remove("Zoning");
		if (this.allowedZones.Count > 0)
		{
			string text3 = string.Empty;
			for (int num3 = 0; num3 < this.allowedZones.Count; num3++)
			{
				text3 = text3 + this.allowedZones[num3] + ((num3 < this.allowedZones.Count - 1) ? ", " : string.Empty);
			}
			this.properties.Values["Zoning"] = text3;
		}
		if (this.renderingCost != null)
		{
			this.properties.Classes["Stats"] = this.renderingCost.ToProperties();
		}
	}

	public static bool PrefabExists(string _prefabFileName)
	{
		return PathAbstractions.PrefabsSearchPaths.GetLocation(_prefabFileName, null, null).Type != PathAbstractions.EAbstractedLocationType.None;
	}

	public bool Load(string _prefabName, bool _applyMapping = true, bool _fixChildblocks = true, bool _allowMissingBlocks = false, bool _skipLoadingBlockData = false)
	{
		return this.Load(PathAbstractions.PrefabsSearchPaths.GetLocation(_prefabName, null, null), _applyMapping, _fixChildblocks, _allowMissingBlocks, _skipLoadingBlockData);
	}

	public bool Load(PathAbstractions.AbstractedLocation _location, bool _applyMapping = true, bool _fixChildblocks = true, bool _allowMissingBlocks = false, bool _skipLoadingBlockData = false)
	{
		if (_location.Type == PathAbstractions.EAbstractedLocationType.None)
		{
			if (SingletonMonoBehaviour<ConnectionManager>.Instance != null && SingletonMonoBehaviour<ConnectionManager>.Instance.IsClient)
			{
				Log.Warning("Prefab loading failed. Prefab '{0}' does not exist!", new object[]
				{
					_location.Name
				});
			}
			else
			{
				Log.Error("Prefab loading failed. Prefab '{0}' does not exist!", new object[]
				{
					_location.Name
				});
			}
			return false;
		}
		this.location = _location;
		return (!_skipLoadingBlockData || this.loadSizeDataOnly(_location, _applyMapping, _fixChildblocks, _allowMissingBlocks, _skipLoadingBlockData)) && this.loadBlockData(_location, _applyMapping, _fixChildblocks, _allowMissingBlocks, _skipLoadingBlockData) && this.LoadXMLData(_location);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool loadSizeDataOnly(PathAbstractions.AbstractedLocation _location, bool _applyMapping, bool _fixChildblocks, bool _allowMissingBlocks, bool _skipLoadingBlockData = false)
	{
		using (Stream stream = SdFile.OpenRead(_location.FullPath))
		{
			using (PooledBinaryReader pooledBinaryReader = MemoryPools.poolBinaryReader.AllocSync(false))
			{
				pooledBinaryReader.SetBaseStream(stream);
				if (pooledBinaryReader.ReadChar() != 't' || pooledBinaryReader.ReadChar() != 't' || pooledBinaryReader.ReadChar() != 's' || pooledBinaryReader.ReadChar() != '\0')
				{
					return false;
				}
				pooledBinaryReader.ReadUInt32();
				this.size = default(Vector3i);
				this.size.x = (int)pooledBinaryReader.ReadInt16();
				this.size.y = (int)pooledBinaryReader.ReadInt16();
				this.size.z = (int)pooledBinaryReader.ReadInt16();
			}
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool loadBlockData(PathAbstractions.AbstractedLocation _location, bool _applyMapping, bool _fixChildblocks, bool _allowMissingBlocks, bool _skipLoadingBlockData = false)
	{
		bool result = true;
		ArrayListMP<int> arrayListMP = null;
		if (_applyMapping)
		{
			arrayListMP = this.loadIdMapping(_location.Folder, _location.FileNameNoExtension, _allowMissingBlocks);
			if (arrayListMP == null)
			{
				return false;
			}
		}
		try
		{
			using (Stream stream = SdFile.OpenRead(_location.FullPath))
			{
				using (PooledBinaryReader pooledBinaryReader = MemoryPools.poolBinaryReader.AllocSync(false))
				{
					pooledBinaryReader.SetBaseStream(stream);
					if (pooledBinaryReader.ReadChar() != 't' || pooledBinaryReader.ReadChar() != 't' || pooledBinaryReader.ReadChar() != 's' || pooledBinaryReader.ReadChar() != '\0')
					{
						return false;
					}
					uint num = pooledBinaryReader.ReadUInt32();
					if (!this.readBlockData(pooledBinaryReader, num, (arrayListMP != null) ? arrayListMP.Items : null, true))
					{
						return false;
					}
					if (num > 12U)
					{
						this.readTileEntities(pooledBinaryReader);
					}
					if (num > 15U)
					{
						this.readTriggerData(pooledBinaryReader);
					}
					this.insidePos.Load(_location.FullPathNoExtension + ".ins", this.size);
				}
			}
		}
		catch (Exception e)
		{
			Log.Exception(e);
			result = false;
		}
		return result;
	}

	public bool LoadXMLData(PathAbstractions.AbstractedLocation _location)
	{
		this.location = _location;
		if (!SdFile.Exists(_location.FullPathNoExtension + ".xml"))
		{
			return true;
		}
		if (!this.properties.Load(_location.Folder, _location.Name, false))
		{
			return false;
		}
		this.ReadFromProperties();
		return true;
	}

	public bool Save(string _prefabName, bool _createMapping = true)
	{
		return this.Save(PathAbstractions.PrefabsSearchPaths.GetLocation(_prefabName, null, null), _createMapping);
	}

	public bool Save(PathAbstractions.AbstractedLocation _location, bool _createMapping = true)
	{
		return this.saveBlockData(_location, _createMapping) && this.SaveXMLData(_location);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void AddAllChildBlocks()
	{
		if (!this.isCellsDataOwner)
		{
			return;
		}
		if (this.blockCells == null)
		{
			return;
		}
		if (this.blockCells.a == null)
		{
			return;
		}
		int num = this.blockCells.a.Length;
		for (int i = 0; i < num; i++)
		{
			Prefab.Cells<uint>.CellsAtZ cellsAtZ = this.blockCells.a[i];
			if (cellsAtZ != null)
			{
				int num2 = cellsAtZ.a.Length;
				for (int j = 0; j < num2; j++)
				{
					Prefab.Cells<uint>.CellsAtX cellsAtX = cellsAtZ.a[j];
					if (cellsAtX != null)
					{
						int num3 = cellsAtX.a.Length;
						for (int k = 0; k < num3; k++)
						{
							Prefab.Cells<uint>.Cell cell = cellsAtX.a[k];
							if (cell.a != null)
							{
								for (int l = 0; l < cell.a.Length; l++)
								{
									uint num4 = cell.a[l];
									if ((num4 & 65535U) != 0U)
									{
										BlockValue blockValue = new BlockValue(num4);
										if (blockValue.rawData != 0U && !blockValue.ischild)
										{
											Block block = blockValue.Block;
											if (block != null && block.isMultiBlock)
											{
												int num5 = (k << 2) + (l & 3);
												int num6 = i;
												int num7 = (j << 2) + (l >> 2);
												int rotation = (int)blockValue.rotation;
												for (int m = block.multiBlockPos.Length - 1; m >= 0; m--)
												{
													Vector3i vector3i = block.multiBlockPos.Get(m, blockValue.type, rotation);
													if (!(vector3i == Vector3i.zero))
													{
														int x = vector3i.x;
														int y = vector3i.y;
														int z = vector3i.z;
														blockValue.ischild = true;
														blockValue.parentx = -x;
														blockValue.parenty = -y;
														blockValue.parentz = -z;
														this.RotateRelative(ref x, ref z);
														int num8 = num5 + x;
														int num9 = num6 + y;
														int num10 = num7 + z;
														if ((ulong)num8 < (ulong)((long)this.size.x) && (ulong)num9 < (ulong)((long)this.size.y) && (ulong)num10 < (ulong)((long)this.size.z))
														{
															this.blockCells.SetData(num8, num9, num10, blockValue.rawData);
														}
													}
												}
											}
										}
									}
								}
							}
						}
					}
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RemoveAllChildAndOldBlocks()
	{
		for (int i = this.size.y - 1; i >= 0; i--)
		{
			for (int j = this.size.z - 1; j >= 0; j--)
			{
				for (int k = this.size.x - 1; k >= 0; k--)
				{
					BlockValue block = this.GetBlock(k, i, j);
					Block block2 = block.Block;
					if (block2 == null)
					{
						this.SetBlock(k, i, j, BlockValue.Air);
					}
					else if (block.ischild)
					{
						this.SetBlock(k, i, j, BlockValue.Air);
					}
					else if (block2 is BlockModelTree && (block.meta & 1) != 0)
					{
						this.SetBlock(k, i, j, BlockValue.Air);
					}
				}
			}
		}
	}

	public bool SaveXMLData(PathAbstractions.AbstractedLocation _location)
	{
		this.writeToProperties();
		return this.properties.Save("prefab", _location.Folder, _location.FileNameNoExtension);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool saveBlockData(PathAbstractions.AbstractedLocation _location, bool _createMapping)
	{
		this.RemoveAllChildAndOldBlocks();
		if (_createMapping)
		{
			NameIdMapping nameIdMapping = new NameIdMapping(_location.FullPathNoExtension + ".blocks.nim", Block.MAX_BLOCKS);
			for (int i = this.GetBlockCount() - 1; i >= 0; i--)
			{
				int x;
				int y;
				int z;
				this.offsetToCoord(i, out x, out y, out z);
				Block block = this.GetBlock(x, y, z).Block;
				nameIdMapping.AddMapping(block.blockID, block.GetBlockName(), false);
			}
			nameIdMapping.WriteToFile();
		}
		try
		{
			using (Stream stream = SdFile.Open(_location.FullPath, FileMode.Create))
			{
				using (PooledBinaryWriter pooledBinaryWriter = MemoryPools.poolBinaryWriter.AllocSync(false))
				{
					pooledBinaryWriter.SetBaseStream(stream);
					pooledBinaryWriter.Write('t');
					pooledBinaryWriter.Write('t');
					pooledBinaryWriter.Write('s');
					pooledBinaryWriter.Write(0);
					pooledBinaryWriter.Write((uint)Prefab.CurrentSaveVersion);
					this.writeBlockData(pooledBinaryWriter);
					this.writeTileEntities(pooledBinaryWriter);
					this.writeTriggerData(pooledBinaryWriter);
					if (this.IsCullThisPrefab())
					{
						this.insidePos.Save(_location.FullPathNoExtension + ".ins");
					}
					else
					{
						SdFile.Delete(_location.FullPathNoExtension + ".ins");
					}
				}
			}
			return true;
		}
		catch (Exception e)
		{
			Log.Exception(e);
		}
		return false;
	}

	public bool IsCullThisPrefab()
	{
		return !this.bExcludePOICulling;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void writeBlockData(BinaryWriter _bw)
	{
		_bw.Write((short)this.size.x);
		_bw.Write((short)this.size.y);
		_bw.Write((short)this.size.z);
		Prefab.Data data = this.CellsToArrays();
		uint[] blocks = data.m_Blocks;
		for (int i = 0; i < blocks.Length; i++)
		{
			_bw.Write(blocks[i]);
		}
		_bw.Write(data.m_Density);
		byte[] array = new byte[data.m_Damage.Length * 2];
		for (int j = 0; j < data.m_Damage.Length; j++)
		{
			array[j * 2] = (byte)(data.m_Damage[j] & 255);
			array[j * 2 + 1] = (byte)(data.m_Damage[j] >> 8 & 255);
		}
		_bw.Write(array);
		SimpleBitStream simpleBitStream = new SimpleBitStream(1000);
		for (int k = 0; k < data.m_Textures.Length; k++)
		{
			bool b = data.m_Textures[k] != 0L;
			simpleBitStream.Add(b);
		}
		simpleBitStream.Write(_bw);
		for (int l = 0; l < data.m_Textures.Length; l++)
		{
			if (data.m_Textures[l] != 0L)
			{
				_bw.Write(data.m_Textures[l]);
			}
		}
		SimpleBitStream simpleBitStream2 = new SimpleBitStream(1000);
		for (int m = 0; m < data.m_Water.Length; m++)
		{
			simpleBitStream2.Add(data.m_Water[m].HasMass());
		}
		simpleBitStream2.Write(_bw);
		for (int n = 0; n < data.m_Water.Length; n++)
		{
			WaterValue waterValue = data.m_Water[n];
			if (waterValue.HasMass())
			{
				waterValue.Write(_bw);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void writeTileEntities(BinaryWriter _bw)
	{
		_bw.Write((short)this.tileEntities.Count);
		foreach (KeyValuePair<Vector3i, TileEntity> keyValuePair in this.tileEntities)
		{
			using (PooledExpandableMemoryStream pooledExpandableMemoryStream = MemoryPools.poolMemoryStream.AllocSync(true))
			{
				using (PooledBinaryWriter pooledBinaryWriter = MemoryPools.poolBinaryWriter.AllocSync(true))
				{
					pooledBinaryWriter.SetBaseStream(pooledExpandableMemoryStream);
					keyValuePair.Value.write(pooledBinaryWriter, TileEntity.StreamModeWrite.Persistency);
				}
				_bw.Write((short)pooledExpandableMemoryStream.Length);
				_bw.Write((byte)keyValuePair.Value.GetTileEntityType());
				pooledExpandableMemoryStream.WriteTo(_bw.BaseStream);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void writeTriggerData(BinaryWriter _bw)
	{
		_bw.Write((short)this.triggerData.Count);
		foreach (KeyValuePair<Vector3i, BlockTrigger> keyValuePair in this.triggerData)
		{
			using (PooledExpandableMemoryStream pooledExpandableMemoryStream = MemoryPools.poolMemoryStream.AllocSync(true))
			{
				using (PooledBinaryWriter pooledBinaryWriter = MemoryPools.poolBinaryWriter.AllocSync(true))
				{
					pooledBinaryWriter.SetBaseStream(pooledExpandableMemoryStream);
					keyValuePair.Value.Write(pooledBinaryWriter);
				}
				_bw.Write((short)pooledExpandableMemoryStream.Length);
				StreamUtils.Write(_bw, keyValuePair.Key);
				pooledExpandableMemoryStream.WriteTo(_bw.BaseStream);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool readBlockData(PooledBinaryReader _br, uint _version, int[] _blockIdMapping, bool _fixChildblocks)
	{
		this.statistics.Clear();
		this.multiBlockParentIndices.Clear();
		this.decoAllowedBlockIndices.Clear();
		this.localRotation = 0;
		this.size.x = (int)_br.ReadInt16();
		this.size.y = (int)_br.ReadInt16();
		this.size.z = (int)_br.ReadInt16();
		int blockCount = this.GetBlockCount();
		this.InitData();
		Prefab.sharedData.Expand(blockCount);
		Prefab.Data data = Prefab.sharedData;
		if (_version >= 2U && _version < 7U)
		{
			this.bCopyAirBlocks = _br.ReadBoolean();
		}
		if (_version >= 3U && _version < 7U)
		{
			this.bAllowTopSoilDecorations = _br.ReadBoolean();
		}
		List<Vector3i> list = null;
		int num = this.blockTypeMissingBlock;
		if (_blockIdMapping != null && num >= 0)
		{
			list = new List<Vector3i>();
		}
		int num2 = blockCount * 4;
		if (Prefab.tempBuf == null || Prefab.tempBuf.Length < num2)
		{
			Prefab.tempBuf = new byte[Utils.FastMax(200000, num2)];
		}
		int num3 = 0;
		_br.Read(Prefab.tempBuf, 0, blockCount * 4);
		if (_version <= 4U)
		{
			for (int i = 0; i < this.size.x; i++)
			{
				for (int j = 0; j < this.size.z; j++)
				{
					for (int k = 0; k < this.size.y; k++)
					{
						BlockValue bv = new BlockValue((uint)((int)Prefab.tempBuf[num3] | (int)Prefab.tempBuf[num3 + 1] << 8 | (int)Prefab.tempBuf[num3 + 2] << 16 | (int)Prefab.tempBuf[num3 + 3] << 24));
						num3 += 4;
						if (_blockIdMapping != null)
						{
							int num4 = _blockIdMapping[bv.type];
							if (num4 < 0)
							{
								Log.Error(string.Concat(new string[]
								{
									"Loading prefab \"",
									this.location.ToString(),
									"\" failed: Block ",
									bv.type.ToString(),
									" used in prefab has no mapping."
								}));
								return false;
							}
							bv.type = num4;
							if (num >= 0 && bv.type == this.blockTypeMissingBlock)
							{
								list.Add(new Vector3i(i, k, j));
							}
						}
						if (bv.isWater)
						{
							this.SetWater(i, k, j, WaterValue.Full);
						}
						else
						{
							if (_fixChildblocks)
							{
								if (bv.ischild)
								{
									goto IL_240;
								}
								Block block = bv.Block;
								if (block == null || ((bv.meta & 1) != 0 && block is BlockModelTree))
								{
									goto IL_240;
								}
							}
							this.SetBlock(i, k, j, bv);
						}
						IL_240:;
					}
				}
			}
		}
		else
		{
			for (int l = 0; l < blockCount; l++)
			{
				uint num5 = (uint)((int)Prefab.tempBuf[num3] | (int)Prefab.tempBuf[num3 + 1] << 8 | (int)Prefab.tempBuf[num3 + 2] << 16 | (int)Prefab.tempBuf[num3 + 3] << 24);
				num3 += 4;
				data.m_Blocks[l] = 0U;
				if (num5 != 0U)
				{
					if (_version < 18U)
					{
						num5 = BlockValueV3.ConvertOldRawData(num5);
					}
					BlockValue blockValue = new BlockValue(num5);
					if (_blockIdMapping != null)
					{
						int type = blockValue.type;
						if (type != 0)
						{
							int num6 = _blockIdMapping[type];
							if (num6 < 0)
							{
								int num7;
								int num8;
								int num9;
								this.offsetToCoord(l, out num7, out num8, out num9);
								Log.Error(string.Concat(new string[]
								{
									"Loading prefab \"",
									this.location.ToString(),
									"\" failed: Block ",
									type.ToString(),
									" used in prefab at ",
									num7.ToString(),
									" / ",
									num8.ToString(),
									" / ",
									num9.ToString(),
									" has no mapping."
								}));
								return false;
							}
							blockValue.type = num6;
							if (num >= 0 && num6 == this.blockTypeMissingBlock)
							{
								int x;
								int y;
								int z;
								this.offsetToCoord(l, out x, out y, out z);
								list.Add(new Vector3i(x, y, z));
							}
						}
					}
					if (_version < 17U && blockValue.isWater)
					{
						data.m_Water[l] = WaterValue.Full;
					}
					else
					{
						Block block2 = blockValue.Block;
						this.updateBlockStatistics(blockValue, block2);
						if (!_fixChildblocks || (!blockValue.ischild && block2 != null && ((blockValue.meta & 1) == 0 || !(block2 is BlockModelTree))))
						{
							if (block2.isMultiBlock && !blockValue.ischild)
							{
								this.multiBlockParentIndices.Add(l);
							}
							if (DecoUtils.HasDecoAllowed(blockValue))
							{
								this.decoAllowedBlockIndices.Add(l);
							}
							data.m_Blocks[l] = blockValue.rawData;
						}
					}
				}
			}
			_br.Read(data.m_Density, 0, this.size.x * this.size.y * this.size.z);
		}
		if (_blockIdMapping != null && num >= 0)
		{
			foreach (Vector3i vector3i in list)
			{
				this.SetDensity(vector3i.x, vector3i.y, vector3i.z, MarchingCubes.DensityAir);
			}
		}
		if (_version > 8U)
		{
			_br.Read(Prefab.tempBuf, 0, blockCount * 2);
			for (int m = 0; m < blockCount; m++)
			{
				data.m_Damage[m] = (ushort)((int)Prefab.tempBuf[m * 2] | (int)Prefab.tempBuf[m * 2 + 1] << 8);
			}
		}
		if (_version >= 10U)
		{
			Prefab.simpleBitStreamReader.Reset();
			Prefab.simpleBitStreamReader.Read(_br);
			while ((num3 = Prefab.simpleBitStreamReader.GetNextOffset()) >= 0)
			{
				data.m_Textures[num3] = _br.ReadInt64();
			}
		}
		this.entities.Clear();
		if (_version >= 4U && _version < 12U)
		{
			int num10 = (int)_br.ReadInt16();
			for (int n = 0; n < num10; n++)
			{
				EntityCreationData entityCreationData = new EntityCreationData();
				entityCreationData.read(_br, false);
				this.entities.Add(entityCreationData);
			}
		}
		if (_version >= 17U)
		{
			Prefab.simpleBitStreamReader.Reset();
			Prefab.simpleBitStreamReader.Read(_br);
			while ((num3 = Prefab.simpleBitStreamReader.GetNextOffset()) >= 0)
			{
				data.m_Water[num3] = WaterValue.FromStream(_br);
			}
		}
		this.CellsFromArrays(ref data);
		if (_fixChildblocks)
		{
			this.AddAllChildBlocks();
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CellsFromArrays(ref Prefab.Data _data)
	{
		BlockValue air = BlockValue.Air;
		for (int i = 0; i < this.size.y; i++)
		{
			for (int j = this.size.z - 1; j >= 0; j--)
			{
				for (int k = this.size.x - 1; k >= 0; k--)
				{
					int num = this.CoordToOffset(0, k, i, j);
					air.rawData = _data.m_Blocks[num];
					if (!air.isair)
					{
						this.blockCells.AllocCell(k, i, j).Set(k, j, air.rawData);
					}
					ushort num2 = _data.m_Damage[num];
					if (num2 != 0)
					{
						this.damageCells.AllocCell(k, i, j).Set(k, j, num2);
					}
					sbyte b = (sbyte)_data.m_Density[num];
					if (b != this.densityCells.defaultValue)
					{
						this.densityCells.AllocCell(k, i, j).Set(k, j, b);
					}
					long num3 = _data.m_Textures[num];
					if (num3 != 0L)
					{
						this.textureCells.AllocCell(k, i, j).Set(k, j, num3);
					}
					WaterValue value = _data.m_Water[num];
					if (value.HasMass())
					{
						this.waterCells.AllocCell(k, i, j).Set(k, j, value);
					}
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Prefab.Data CellsToArrays()
	{
		Prefab.Data result;
		result.m_Blocks = this.blockCells.ToArray(this, this.size);
		result.m_Damage = this.damageCells.ToArray(this, this.size);
		result.m_Density = (byte[])this.densityCells.ToArray(this, this.size);
		result.m_Textures = this.textureCells.ToArray(this, this.size);
		result.m_Water = this.waterCells.ToArray(this, this.size);
		return result;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void readTileEntities(PooledBinaryReader _br)
	{
		this.tileEntities.Clear();
		int num = (int)_br.ReadInt16();
		for (int i = 0; i < num; i++)
		{
			int length = (int)_br.ReadInt16();
			TileEntityType type = (TileEntityType)_br.ReadByte();
			try
			{
				TileEntity tileEntity = TileEntity.Instantiate(type, null);
				using (PooledExpandableMemoryStream pooledExpandableMemoryStream = MemoryPools.poolMemoryStream.AllocSync(true))
				{
					StreamUtils.StreamCopy(_br.BaseStream, pooledExpandableMemoryStream, length, null, true);
					pooledExpandableMemoryStream.Position = 0L;
					using (PooledBinaryReader pooledBinaryReader = MemoryPools.poolBinaryReader.AllocSync(true))
					{
						pooledBinaryReader.SetBaseStream(pooledExpandableMemoryStream);
						tileEntity.read(pooledBinaryReader, TileEntity.StreamModeRead.Persistency);
					}
				}
				this.tileEntities.Add(tileEntity.localChunkPos, tileEntity);
			}
			catch (Exception e)
			{
				Log.Error(string.Format("Skipping loading of active block data for {0} because of the following exception:", this.PrefabName));
				Log.Exception(e);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void readTriggerData(PooledBinaryReader _br)
	{
		this.triggerData.Clear();
		this.TriggerLayers.Clear();
		int num = (int)_br.ReadInt16();
		for (int i = 0; i < num; i++)
		{
			int length = (int)_br.ReadInt16();
			Vector3i vector3i = StreamUtils.ReadVector3i(_br);
			try
			{
				BlockTrigger blockTrigger = new BlockTrigger(null);
				blockTrigger.LocalChunkPos = vector3i;
				using (PooledExpandableMemoryStream pooledExpandableMemoryStream = MemoryPools.poolMemoryStream.AllocSync(true))
				{
					StreamUtils.StreamCopy(_br.BaseStream, pooledExpandableMemoryStream, length, null, true);
					pooledExpandableMemoryStream.Position = 0L;
					using (PooledBinaryReader pooledBinaryReader = MemoryPools.poolBinaryReader.AllocSync(true))
					{
						pooledBinaryReader.SetBaseStream(pooledExpandableMemoryStream);
						blockTrigger.Read(pooledBinaryReader);
					}
				}
				if (!Block.BlocksLoaded || this.GetBlock(vector3i.x, vector3i.y, vector3i.z).Block.AllowBlockTriggers)
				{
					this.triggerData.Add(blockTrigger.LocalChunkPos, blockTrigger);
					this.HandleAddingTriggerLayers(blockTrigger);
				}
			}
			catch (Exception e)
			{
				Log.Error(string.Format("Skipping loading of active block data for {0} because of the following exception:", this.PrefabName));
				Log.Exception(e);
			}
		}
	}

	public void RotateY(bool _bLeft, int _rotCount)
	{
		if (_rotCount == 0)
		{
			return;
		}
		if (Block.BlocksLoaded && this.isCellsDataOwner)
		{
			int num = this.blockCells.a.Length;
			for (int i = 0; i < num; i++)
			{
				Prefab.Cells<uint>.CellsAtZ cellsAtZ = this.blockCells.a[i];
				if (cellsAtZ != null)
				{
					int num2 = cellsAtZ.a.Length;
					for (int j = 0; j < num2; j++)
					{
						Prefab.Cells<uint>.CellsAtX cellsAtX = cellsAtZ.a[j];
						if (cellsAtX != null)
						{
							int num3 = cellsAtX.a.Length;
							for (int k = 0; k < num3; k++)
							{
								Prefab.Cells<uint>.Cell cell = cellsAtX.a[k];
								if (cell.a != null)
								{
									for (int l = 0; l < cell.a.Length; l++)
									{
										uint num4 = cell.a[l];
										if ((num4 & 65535U) != 0U)
										{
											BlockValue blockValue = new BlockValue(num4);
											if (!blockValue.ischild)
											{
												Block block = blockValue.Block;
												if (block == null || ((blockValue.meta & 1) != 0 && block is BlockModelTree))
												{
													cell.a[l] = 0U;
												}
												else
												{
													blockValue = block.shape.RotateY(_bLeft, blockValue, _rotCount);
													cell.a[l] = blockValue.rawData;
												}
											}
										}
									}
								}
							}
						}
					}
				}
			}
		}
		for (int m = 0; m < _rotCount; m++)
		{
			this.localRotation += (_bLeft ? 1 : -1);
			this.localRotation &= 3;
			for (int n = 0; n < this.entities.Count; n++)
			{
				EntityCreationData entityCreationData = this.entities[n];
				if (_bLeft)
				{
					entityCreationData.pos = new Vector3((float)this.size.z - entityCreationData.pos.z, entityCreationData.pos.y, entityCreationData.pos.x);
					entityCreationData.rot = new Vector3(entityCreationData.rot.x, entityCreationData.rot.y - 90f, entityCreationData.rot.z);
				}
				else
				{
					entityCreationData.pos = new Vector3(entityCreationData.pos.z, entityCreationData.pos.y, (float)this.size.x - entityCreationData.pos.x);
					entityCreationData.rot = new Vector3(entityCreationData.rot.x, entityCreationData.rot.y + 90f, entityCreationData.rot.z);
				}
			}
			MathUtils.Swap(ref this.TraderAreaProtect.x, ref this.TraderAreaProtect.z);
			foreach (KeyValuePair<string, List<Vector3i>> keyValuePair in this.indexedBlockOffsets)
			{
				for (int num5 = 0; num5 < keyValuePair.Value.Count; num5++)
				{
					Vector3i value = keyValuePair.Value[num5];
					Prefab.RotatePointOnY(_bLeft, ref value);
					keyValuePair.Value[num5] = value;
				}
			}
			if (this.bTraderArea)
			{
				for (int num6 = 0; num6 < this.TeleportVolumes.Count; num6++)
				{
					Vector3i other = this.TeleportVolumes[num6].size;
					Vector3i startPos = this.TeleportVolumes[num6].startPos;
					Vector3i vector3i = startPos + other;
					if (_bLeft)
					{
						startPos = new Vector3i(this.size.z - startPos.z, startPos.y, startPos.x);
						vector3i = new Vector3i(this.size.z - vector3i.z, vector3i.y, vector3i.x);
					}
					else
					{
						startPos = new Vector3i(startPos.z, startPos.y, this.size.x - startPos.x);
						vector3i = new Vector3i(vector3i.z, vector3i.y, this.size.x - vector3i.x);
					}
					if (startPos.x > vector3i.x)
					{
						MathUtils.Swap(ref startPos.x, ref vector3i.x);
					}
					if (startPos.z > vector3i.z)
					{
						MathUtils.Swap(ref startPos.z, ref vector3i.z);
					}
					this.TeleportVolumes[num6].startPos = startPos;
					MathUtils.Swap(ref other.x, ref other.z);
					this.TeleportVolumes[num6].size = other;
				}
			}
			if (this.bSleeperVolumes)
			{
				for (int num7 = 0; num7 < this.SleeperVolumes.Count; num7++)
				{
					Vector3i other2 = this.SleeperVolumes[num7].size;
					Vector3i startPos2 = this.SleeperVolumes[num7].startPos;
					Vector3i vector3i2 = startPos2 + other2;
					if (_bLeft)
					{
						startPos2 = new Vector3i(this.size.z - startPos2.z, startPos2.y, startPos2.x);
						vector3i2 = new Vector3i(this.size.z - vector3i2.z, vector3i2.y, vector3i2.x);
					}
					else
					{
						startPos2 = new Vector3i(startPos2.z, startPos2.y, this.size.x - startPos2.x);
						vector3i2 = new Vector3i(vector3i2.z, vector3i2.y, this.size.x - vector3i2.x);
					}
					if (startPos2.x > vector3i2.x)
					{
						MathUtils.Swap(ref startPos2.x, ref vector3i2.x);
					}
					if (startPos2.z > vector3i2.z)
					{
						MathUtils.Swap(ref startPos2.z, ref vector3i2.z);
					}
					this.SleeperVolumes[num7].startPos = startPos2;
					MathUtils.Swap(ref other2.x, ref other2.z);
					this.SleeperVolumes[num7].size = other2;
				}
			}
			if (this.bInfoVolumes)
			{
				for (int num8 = 0; num8 < this.InfoVolumes.Count; num8++)
				{
					Vector3i other3 = this.InfoVolumes[num8].size;
					Vector3i startPos3 = this.InfoVolumes[num8].startPos;
					Vector3i vector3i3 = startPos3 + other3;
					if (_bLeft)
					{
						startPos3 = new Vector3i(this.size.z - startPos3.z, startPos3.y, startPos3.x);
						vector3i3 = new Vector3i(this.size.z - vector3i3.z, vector3i3.y, vector3i3.x);
					}
					else
					{
						startPos3 = new Vector3i(startPos3.z, startPos3.y, this.size.x - startPos3.x);
						vector3i3 = new Vector3i(vector3i3.z, vector3i3.y, this.size.x - vector3i3.x);
					}
					if (startPos3.x > vector3i3.x)
					{
						MathUtils.Swap(ref startPos3.x, ref vector3i3.x);
					}
					if (startPos3.z > vector3i3.z)
					{
						MathUtils.Swap(ref startPos3.z, ref vector3i3.z);
					}
					this.InfoVolumes[num8].startPos = startPos3;
					MathUtils.Swap(ref other3.x, ref other3.z);
					this.InfoVolumes[num8].size = other3;
				}
			}
			if (this.bWallVolumes)
			{
				for (int num9 = 0; num9 < this.WallVolumes.Count; num9++)
				{
					Vector3i other4 = this.WallVolumes[num9].size;
					Vector3i startPos4 = this.WallVolumes[num9].startPos;
					Vector3i vector3i4 = startPos4 + other4;
					if (_bLeft)
					{
						startPos4 = new Vector3i(this.size.z - startPos4.z, startPos4.y, startPos4.x);
						vector3i4 = new Vector3i(this.size.z - vector3i4.z, vector3i4.y, vector3i4.x);
					}
					else
					{
						startPos4 = new Vector3i(startPos4.z, startPos4.y, this.size.x - startPos4.x);
						vector3i4 = new Vector3i(vector3i4.z, vector3i4.y, this.size.x - vector3i4.x);
					}
					if (startPos4.x > vector3i4.x)
					{
						MathUtils.Swap(ref startPos4.x, ref vector3i4.x);
					}
					if (startPos4.z > vector3i4.z)
					{
						MathUtils.Swap(ref startPos4.z, ref vector3i4.z);
					}
					this.WallVolumes[num9].startPos = startPos4;
					MathUtils.Swap(ref other4.x, ref other4.z);
					this.WallVolumes[num9].size = other4;
				}
			}
			if (this.bTriggerVolumes)
			{
				for (int num10 = 0; num10 < this.TriggerVolumes.Count; num10++)
				{
					Vector3i other5 = this.TriggerVolumes[num10].size;
					Vector3i startPos5 = this.TriggerVolumes[num10].startPos;
					Vector3i vector3i5 = startPos5 + other5;
					if (_bLeft)
					{
						startPos5 = new Vector3i(this.size.z - startPos5.z, startPos5.y, startPos5.x);
						vector3i5 = new Vector3i(this.size.z - vector3i5.z, vector3i5.y, vector3i5.x);
					}
					else
					{
						startPos5 = new Vector3i(startPos5.z, startPos5.y, this.size.x - startPos5.x);
						vector3i5 = new Vector3i(vector3i5.z, vector3i5.y, this.size.x - vector3i5.x);
					}
					if (startPos5.x > vector3i5.x)
					{
						MathUtils.Swap(ref startPos5.x, ref vector3i5.x);
					}
					if (startPos5.z > vector3i5.z)
					{
						MathUtils.Swap(ref startPos5.z, ref vector3i5.z);
					}
					this.TriggerVolumes[num10].startPos = startPos5;
					MathUtils.Swap(ref other5.x, ref other5.z);
					this.TriggerVolumes[num10].size = other5;
				}
			}
			for (int num11 = 0; num11 < this.POIMarkers.Count; num11++)
			{
				Vector3i other6 = this.POIMarkers[num11].Size;
				Vector3i start = this.POIMarkers[num11].Start;
				Vector3i vector3i6 = start + other6;
				if (_bLeft)
				{
					start = new Vector3i(this.size.z - start.z, start.y, start.x);
					vector3i6 = new Vector3i(this.size.z - vector3i6.z, vector3i6.y, vector3i6.x);
				}
				else
				{
					start = new Vector3i(start.z, start.y, this.size.x - start.x);
					vector3i6 = new Vector3i(vector3i6.z, vector3i6.y, this.size.x - vector3i6.x);
				}
				if (start.x > vector3i6.x)
				{
					MathUtils.Swap(ref start.x, ref vector3i6.x);
				}
				if (start.z > vector3i6.z)
				{
					MathUtils.Swap(ref start.z, ref vector3i6.z);
				}
				this.POIMarkers[num11].Start = start;
				MathUtils.Swap(ref other6.x, ref other6.z);
				this.POIMarkers[num11].Size = other6;
			}
			MathUtils.Swap(ref this.size.x, ref this.size.z);
		}
		if (Block.BlocksLoaded)
		{
			this.AddAllChildBlocks();
		}
	}

	public void RotatePOIMarkers(bool _bLeft, int _rotCount)
	{
		Vector3i vector3i = this.size;
		for (int i = 0; i < _rotCount; i++)
		{
			for (int j = 0; j < this.POIMarkers.Count; j++)
			{
				Vector3i other = this.POIMarkers[j].Size;
				Vector3i start = this.POIMarkers[j].Start;
				Vector3i vector3i2 = start + other;
				if (_bLeft)
				{
					start = new Vector3i(vector3i.z - start.z, start.y, start.x);
					vector3i2 = new Vector3i(vector3i.z - vector3i2.z, vector3i2.y, vector3i2.x);
				}
				else
				{
					start = new Vector3i(start.z, start.y, vector3i.x - start.x);
					vector3i2 = new Vector3i(vector3i2.z, vector3i2.y, vector3i.x - vector3i2.x);
				}
				if (start.x > vector3i2.x)
				{
					MathUtils.Swap(ref start.x, ref vector3i2.x);
				}
				if (start.z > vector3i2.z)
				{
					MathUtils.Swap(ref start.z, ref vector3i2.z);
				}
				this.POIMarkers[j].Start = start;
				MathUtils.Swap(ref other.x, ref other.z);
				this.POIMarkers[j].Size = other;
			}
			MathUtils.Swap(ref vector3i.x, ref vector3i.z);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void RotatePointOnY(bool _bLeft, ref Vector3i _center)
	{
		Vector3 vector;
		if (_bLeft)
		{
			vector = Quaternion.AngleAxis(-90f, Vector3.up) * _center.ToVector3();
		}
		else
		{
			vector = Quaternion.AngleAxis(90f, Vector3.up) * _center.ToVector3();
		}
		_center = new Vector3i(Mathf.RoundToInt(vector.x), Mathf.RoundToInt(vector.y), Mathf.RoundToInt(vector.z));
	}

	public void Replace(BlockValue _src, BlockValue _dst, bool _bConsiderRotation, int _considerPaintId1 = -1, int _considerPaintId2 = -1)
	{
		for (int i = 0; i < this.size.x; i++)
		{
			for (int j = 0; j < this.size.z; j++)
			{
				for (int k = 0; k < this.size.y; k++)
				{
					BlockValue block = this.GetBlock(i, k, j);
					if (!block.ischild && block.type == _src.type && (!_bConsiderRotation || block.rotation == _src.rotation) && (_considerPaintId1 == -1 || this.hasTexture(this.GetTexture(i, k, j), _considerPaintId1)) && (_considerPaintId2 == -1 || this.hasTexture(this.GetTexture(i, k, j), _considerPaintId2)))
					{
						BlockValue bv = _dst;
						if (!_bConsiderRotation)
						{
							bv.rotation = block.rotation;
						}
						bv.meta = ((_dst.meta != 0) ? _dst.meta : block.meta);
						this.SetBlock(i, k, j, bv);
						bool flag = _src.Block.shape.IsTerrain();
						Block block2 = _dst.Block;
						bool flag2 = (block2 != null) ? block2.shape.IsTerrain() : flag;
						if (flag != flag2)
						{
							sbyte b = this.GetDensity(i, k, j);
							if (flag2)
							{
								b = MarchingCubes.DensityTerrain;
							}
							else if (b != 0)
							{
								b = MarchingCubes.DensityAir;
							}
							this.SetDensity(i, k, j, b);
						}
					}
				}
			}
		}
	}

	public void Replace(int _searchPaintId, int _replacePaintId, int _blockId = -1)
	{
		for (int i = 0; i < this.size.x; i++)
		{
			for (int j = 0; j < this.size.z; j++)
			{
				for (int k = 0; k < this.size.y; k++)
				{
					BlockValue block = this.GetBlock(i, k, j);
					if (!block.ischild && this.hasTexture(this.GetTexture(i, k, j), _searchPaintId) && (_blockId == -1 || _blockId == block.type))
					{
						long num = this.GetTexture(i, k, j);
						for (int l = 0; l < 6; l++)
						{
							if ((num >> l * 8 & 255L) == (long)_searchPaintId)
							{
								num &= ~(255L << l * 8);
								num |= (long)_replacePaintId << l * 8;
							}
						}
						this.SetTexture(i, k, j, num);
					}
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool hasTexture(long fulltexture, int textureIdx)
	{
		for (int i = 0; i < 6; i++)
		{
			if ((fulltexture >> i * 8 & 255L) == (long)textureIdx)
			{
				return true;
			}
		}
		return false;
	}

	public int Search(BlockValue _src, bool _bConsiderRotation, int _considerPaintId1 = -1, int _considerPaintId2 = -1)
	{
		int num = 0;
		for (int i = 0; i < this.size.x; i++)
		{
			for (int j = 0; j < this.size.z; j++)
			{
				for (int k = 0; k < this.size.y; k++)
				{
					BlockValue block = this.GetBlock(i, k, j);
					if (!block.ischild && block.type == _src.type && (!_bConsiderRotation || block.rotation == _src.rotation) && (_considerPaintId1 == -1 || this.hasTexture(this.GetTexture(i, k, j), _considerPaintId1)) && (_considerPaintId2 == -1 || this.hasTexture(this.GetTexture(i, k, j), _considerPaintId2)))
					{
						num++;
					}
				}
			}
		}
		return num;
	}

	public int Search(int _paintId, int _blockId = -1)
	{
		int num = 0;
		for (int i = 0; i < this.size.x; i++)
		{
			for (int j = 0; j < this.size.z; j++)
			{
				for (int k = 0; k < this.size.y; k++)
				{
					BlockValue block = this.GetBlock(i, k, j);
					if (!block.ischild && this.hasTexture(this.GetTexture(i, k, j), _paintId) && (_blockId == -1 || _blockId == block.type))
					{
						num++;
					}
				}
			}
		}
		return num;
	}

	public void CopyIntoRPC(GameManager _gm, Vector3i _destinationPos, bool _pasteAirBlocks = false)
	{
		List<BlockChangeInfo> list = new List<BlockChangeInfo>();
		NetPackageWaterSet package = NetPackageManager.GetPackage<NetPackageWaterSet>();
		if (_pasteAirBlocks)
		{
			this.AddAllChildBlocks();
		}
		if (this.bCopyAirBlocks)
		{
			NetPackageWaterSet package2 = NetPackageManager.GetPackage<NetPackageWaterSet>();
			for (int i = 0; i < this.size.y; i++)
			{
				for (int j = 0; j < this.size.x; j++)
				{
					for (int k = 0; k < this.size.z; k++)
					{
						int num = j + _destinationPos.x;
						int num2 = i + _destinationPos.y;
						int num3 = k + _destinationPos.z;
						if (!_gm.World.GetBlock(num, num2, num3).isair)
						{
							list.Add(new BlockChangeInfo(0, new Vector3i(num, num2, num3), BlockValue.Air));
						}
						if (_gm.World.GetWater(num, num2, num3).HasMass())
						{
							package2.AddChange(num, num2, num3, WaterValue.Empty);
						}
					}
				}
			}
			_gm.SetWaterRPC(package2);
		}
		Dictionary<Vector3i, TileEntity> dictionary = new Dictionary<Vector3i, TileEntity>();
		Dictionary<Vector3i, BlockTrigger> dictionary2 = new Dictionary<Vector3i, BlockTrigger>();
		for (int l = 0; l < this.size.y; l++)
		{
			for (int m = 0; m < this.size.x; m++)
			{
				for (int n = 0; n < this.size.z; n++)
				{
					WaterValue water = this.GetWater(m, l, n);
					if (water.HasMass())
					{
						package.AddChange(m + _destinationPos.x, l + _destinationPos.y, n + _destinationPos.z, water);
					}
					BlockValue block = this.GetBlock(m, l, n);
					Block block2 = block.Block;
					if (block2 != null && (!block.isair || _pasteAirBlocks) && (!_pasteAirBlocks || !block.ischild))
					{
						long texture = this.GetTexture(m, l, n);
						sbyte density = this.GetDensity(m, l, n);
						list.Add(new BlockChangeInfo(0, new Vector3i(m + _destinationPos.x, l + _destinationPos.y, n + _destinationPos.z), block, density, texture));
						Vector3i vector3i;
						if (block2.IsTileEntitySavedInPrefab())
						{
							vector3i = new Vector3i(m, l, n);
							TileEntity tileEntity;
							if ((tileEntity = this.GetTileEntity(vector3i)) != null)
							{
								dictionary.Add(vector3i, tileEntity);
							}
						}
						vector3i = new Vector3i(m, l, n);
						BlockTrigger blockTrigger;
						if ((blockTrigger = this.GetBlockTrigger(vector3i)) != null)
						{
							dictionary2.Add(vector3i, blockTrigger);
						}
					}
				}
			}
		}
		_gm.SetBlocksRPC(list, null);
		_gm.SetWaterRPC(package);
		if (_pasteAirBlocks)
		{
			this.AddAllChildBlocks();
		}
		bool flag = this.PrefabName.StartsWith("part_");
		foreach (KeyValuePair<Vector3i, TileEntity> keyValuePair in dictionary)
		{
			Vector3i vector3i2 = keyValuePair.Key + _destinationPos;
			TileEntity tileEntity2 = _gm.World.GetTileEntity(vector3i2);
			if (tileEntity2 == null || flag)
			{
				Chunk chunk = (Chunk)_gm.World.GetChunkFromWorldPos(vector3i2);
				Vector3i vector3i3 = World.toBlock(vector3i2);
				if (flag)
				{
					chunk.RemoveTileEntityAt<TileEntity>(_gm.World, vector3i3);
				}
				tileEntity2 = keyValuePair.Value.Clone();
				tileEntity2.SetChunk(chunk);
				tileEntity2.localChunkPos = vector3i3;
				chunk.AddTileEntity(tileEntity2);
			}
			Vector3i localChunkPos = tileEntity2.localChunkPos;
			tileEntity2.CopyFrom(keyValuePair.Value);
			tileEntity2.localChunkPos = localChunkPos;
			tileEntity2.SetModified();
		}
		foreach (KeyValuePair<Vector3i, BlockTrigger> keyValuePair2 in dictionary2)
		{
			Vector3i vector3i4 = keyValuePair2.Key + _destinationPos;
			BlockTrigger blockTrigger2 = _gm.World.GetBlockTrigger(0, vector3i4);
			if (blockTrigger2 == null || flag)
			{
				Chunk chunk2 = (Chunk)_gm.World.GetChunkFromWorldPos(vector3i4);
				Vector3i vector3i5 = World.toBlock(vector3i4);
				if (flag)
				{
					chunk2.RemoveTileEntityAt<TileEntity>(_gm.World, vector3i5);
				}
				blockTrigger2 = keyValuePair2.Value.Clone();
				blockTrigger2.Chunk = chunk2;
				blockTrigger2.LocalChunkPos = vector3i5;
				chunk2.AddBlockTrigger(blockTrigger2);
			}
			Vector3i localChunkPos2 = blockTrigger2.LocalChunkPos;
			blockTrigger2.CopyFrom(keyValuePair2.Value);
			blockTrigger2.LocalChunkPos = localChunkPos2;
		}
	}

	public void CountSleeperSpawnsInVolume(World _world, Vector3i _offset, int index)
	{
		this.Transient_NumSleeperSpawns = 0;
		Prefab.PrefabSleeperVolume prefabSleeperVolume = this.SleeperVolumes[index];
		Vector3i startPos = prefabSleeperVolume.startPos;
		Vector3i other = prefabSleeperVolume.size;
		Vector3i one = startPos + other;
		Vector3i vector3i = startPos + _offset;
		Vector3i vector3i2 = one + _offset;
		int x = vector3i.x;
		int y = vector3i.y;
		int z = vector3i.z;
		int x2 = vector3i2.x;
		int y2 = vector3i2.y;
		int z2 = vector3i2.z;
		for (int i = z; i < z2; i++)
		{
			for (int j = y; j < y2; j++)
			{
				for (int k = x; k < x2; k++)
				{
					if (!_world.GetBlock(k, j - 1, i).Block.IsSleeperBlock && _world.GetBlock(k, j, i).Block.IsSleeperBlock)
					{
						Vector3i pos = new Vector3i(k - _offset.x, j - _offset.y, i - _offset.z);
						if (!this.IsPosInSleeperPriorityVolume(pos, index))
						{
							this.Transient_NumSleeperSpawns++;
						}
					}
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CopySleeperBlocksContainedInVolume(int volumeIndex, Vector3i _offset, SleeperVolume _volume, Vector3i _volumeMins, Vector3i _volumeMaxs)
	{
		int num = Mathf.Max(_volumeMins.x, 0);
		int num2 = Mathf.Max(_volumeMins.y, 0);
		int num3 = Mathf.Max(_volumeMins.z, 0);
		int num4 = Mathf.Min(this.size.x, _volumeMaxs.x);
		int num5 = Mathf.Min(this.size.y, _volumeMaxs.y);
		int num6 = Mathf.Min(this.size.z, _volumeMaxs.z);
		for (int i = num; i < num4; i++)
		{
			int x = i + _offset.x;
			for (int j = num3; j < num6; j++)
			{
				int z = j + _offset.z;
				for (int k = num2; k < num5; k++)
				{
					if (k <= 0 || !this.GetBlockNoDamage(this.localRotation, i, k - 1, j).Block.IsSleeperBlock)
					{
						BlockValue block = this.GetBlock(i, k, j);
						Block block2 = block.Block;
						if (block2.IsSleeperBlock)
						{
							int y = k + _offset.y;
							Vector3i pos = new Vector3i(i, k, j);
							if (!this.IsPosInSleeperPriorityVolume(pos, volumeIndex))
							{
								_volume.AddSpawnPoint(x, y, z, (BlockSleeper)block2, block);
							}
						}
					}
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CopySleeperVolumes(WorldBase _world, Chunk _chunk, Vector3i _offset)
	{
		Vector3i vector3i = Vector3i.zero;
		Vector3i vector3i2 = Vector3i.zero;
		if (_chunk != null)
		{
			vector3i = _chunk.GetWorldPos();
			vector3i2 = vector3i + new Vector3i(16, 256, 16);
		}
		for (int i = 0; i < this.SleeperVolumes.Count; i++)
		{
			Prefab.PrefabSleeperVolume prefabSleeperVolume = this.SleeperVolumes[i];
			if (prefabSleeperVolume.used)
			{
				Vector3i startPos = prefabSleeperVolume.startPos;
				Vector3i volumeMaxs = startPos + prefabSleeperVolume.size;
				Vector3i vector3i3 = startPos + _offset;
				Vector3i vector3i4 = vector3i3 + prefabSleeperVolume.size;
				Vector3i vector3i5 = vector3i3 - SleeperVolume.chunkPadding;
				Vector3i vector3i6 = vector3i4 + SleeperVolume.chunkPadding;
				if (_chunk != null)
				{
					if (vector3i5.x < vector3i2.x && vector3i6.x > vector3i.x && vector3i5.y < vector3i2.y && vector3i6.y > vector3i.y && vector3i5.z < vector3i2.z && vector3i6.z > vector3i.z)
					{
						int num = _world.FindSleeperVolume(vector3i3, vector3i4);
						if (num < 0)
						{
							SleeperVolume volume = SleeperVolume.Create(prefabSleeperVolume, vector3i3, vector3i4);
							num = _world.AddSleeperVolume(volume);
							this.CopySleeperBlocksContainedInVolume(i, _offset, volume, startPos, volumeMaxs);
						}
						_chunk.AddSleeperVolumeId(num);
					}
				}
				else
				{
					int num2 = _world.FindSleeperVolume(vector3i3, vector3i4);
					if (num2 < 0)
					{
						SleeperVolume volume2 = SleeperVolume.Create(prefabSleeperVolume, vector3i3, vector3i4);
						num2 = _world.AddSleeperVolume(volume2);
						this.CopySleeperBlocksContainedInVolume(i, _offset, volume2, startPos, volumeMaxs);
					}
					int num3 = World.toChunkXZ(vector3i5.x);
					int num4 = World.toChunkXZ(vector3i6.x - 1);
					int num5 = World.toChunkXZ(vector3i5.z);
					int num6 = World.toChunkXZ(vector3i6.z - 1);
					for (int j = num3; j <= num4; j++)
					{
						for (int k = num5; k <= num6; k++)
						{
							Chunk chunk = (Chunk)_world.GetChunkSync(j, 0, k);
							if (chunk != null)
							{
								chunk.AddSleeperVolumeId(num2);
							}
						}
					}
				}
			}
		}
		for (int l = 0; l < this.TriggerVolumes.Count; l++)
		{
			Prefab.PrefabTriggerVolume prefabTriggerVolume = this.TriggerVolumes[l];
			Vector3i startPos2 = prefabTriggerVolume.startPos;
			startPos2 + prefabTriggerVolume.size;
			Vector3i vector3i7 = startPos2 + _offset;
			Vector3i vector3i8 = vector3i7 + prefabTriggerVolume.size;
			Vector3i vector3i9 = vector3i7 - SleeperVolume.chunkPadding;
			Vector3i vector3i10 = vector3i8 + SleeperVolume.chunkPadding;
			if (_chunk != null)
			{
				if (vector3i9.x < vector3i2.x && vector3i10.x > vector3i.x && vector3i9.y < vector3i2.y && vector3i10.y > vector3i.y && vector3i9.z < vector3i2.z && vector3i10.z > vector3i.z)
				{
					int num7 = _world.FindTriggerVolume(vector3i7, vector3i8);
					if (num7 < 0)
					{
						TriggerVolume volume3 = TriggerVolume.Create(prefabTriggerVolume, vector3i7, vector3i8);
						num7 = _world.AddTriggerVolume(volume3);
					}
					_chunk.AddTriggerVolumeId(num7);
				}
			}
			else
			{
				int num8 = _world.FindTriggerVolume(vector3i7, vector3i8);
				if (num8 < 0)
				{
					TriggerVolume volume4 = TriggerVolume.Create(prefabTriggerVolume, vector3i7, vector3i8);
					num8 = _world.AddTriggerVolume(volume4);
				}
				int num9 = World.toChunkXZ(vector3i9.x);
				int num10 = World.toChunkXZ(vector3i10.x - 1);
				int num11 = World.toChunkXZ(vector3i9.z);
				int num12 = World.toChunkXZ(vector3i10.z - 1);
				for (int m = num9; m <= num10; m++)
				{
					for (int n = num11; n <= num12; n++)
					{
						Chunk chunk2 = (Chunk)_world.GetChunkSync(m, 0, n);
						if (chunk2 != null)
						{
							chunk2.AddTriggerVolumeId(num8);
						}
					}
				}
			}
		}
		for (int num13 = 0; num13 < this.WallVolumes.Count; num13++)
		{
			Prefab.PrefabWallVolume prefabWallVolume = this.WallVolumes[num13];
			Vector3i startPos3 = prefabWallVolume.startPos;
			startPos3 + prefabWallVolume.size;
			Vector3i vector3i11 = startPos3 + _offset;
			Vector3i vector3i12 = vector3i11 + prefabWallVolume.size;
			Vector3i vector3i13 = vector3i11;
			Vector3i vector3i14 = vector3i12;
			if (_chunk != null)
			{
				if (vector3i13.x < vector3i2.x && vector3i14.x > vector3i.x && vector3i13.y < vector3i2.y && vector3i14.y > vector3i.y && vector3i13.z < vector3i2.z && vector3i14.z > vector3i.z)
				{
					int num14 = _world.FindWallVolume(vector3i11, vector3i12);
					if (num14 < 0)
					{
						WallVolume volume5 = WallVolume.Create(prefabWallVolume, vector3i11, vector3i12);
						num14 = _world.AddWallVolume(volume5);
					}
					_chunk.AddWallVolumeId(num14);
				}
			}
			else
			{
				int num15 = _world.FindWallVolume(vector3i11, vector3i12);
				if (num15 < 0)
				{
					WallVolume volume6 = WallVolume.Create(prefabWallVolume, vector3i11, vector3i12);
					num15 = _world.AddWallVolume(volume6);
				}
				int num16 = World.toChunkXZ(vector3i13.x);
				int num17 = World.toChunkXZ(vector3i14.x - 1);
				int num18 = World.toChunkXZ(vector3i13.z);
				int num19 = World.toChunkXZ(vector3i14.z - 1);
				for (int num20 = num16; num20 <= num17; num20++)
				{
					for (int num21 = num18; num21 <= num19; num21++)
					{
						Chunk chunk3 = (Chunk)_world.GetChunkSync(num20, 0, num21);
						if (chunk3 != null)
						{
							chunk3.AddWallVolumeId(num15);
						}
					}
				}
			}
		}
	}

	public Prefab.PrefabSleeperVolume FindSleeperVolume(Vector3i _pos)
	{
		Prefab.PrefabSleeperVolume result = null;
		for (int i = 0; i < this.SleeperVolumes.Count; i++)
		{
			Prefab.PrefabSleeperVolume prefabSleeperVolume = this.SleeperVolumes[i];
			if (prefabSleeperVolume.used && this.IsPosInSleeperVolume(prefabSleeperVolume, _pos))
			{
				result = prefabSleeperVolume;
				if (prefabSleeperVolume.isPriority)
				{
					break;
				}
			}
		}
		return result;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool IsPosInSleeperPriorityVolume(Vector3i _pos, int skipIndex)
	{
		for (int i = 0; i < this.SleeperVolumes.Count; i++)
		{
			if (i != skipIndex)
			{
				Prefab.PrefabSleeperVolume prefabSleeperVolume = this.SleeperVolumes[i];
				if (prefabSleeperVolume.used && prefabSleeperVolume.isPriority && this.IsPosInSleeperVolume(prefabSleeperVolume, _pos))
				{
					return true;
				}
			}
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool IsPosInSleeperVolume(Prefab.PrefabSleeperVolume volume, Vector3i _pos)
	{
		if (volume.used)
		{
			Vector3i startPos = volume.startPos;
			Vector3i vector3i = startPos + volume.size;
			if (_pos.x >= startPos.x && _pos.x < vector3i.x && _pos.y >= startPos.y && _pos.y < vector3i.y && _pos.z >= startPos.z && _pos.z < vector3i.z)
			{
				return true;
			}
		}
		return false;
	}

	public void MoveVolumes(Vector3i moveDistance)
	{
		for (int i = 0; i < this.SleeperVolumes.Count; i++)
		{
			this.SleeperVolumes[i].startPos += moveDistance;
		}
		for (int j = 0; j < this.TeleportVolumes.Count; j++)
		{
			this.TeleportVolumes[j].startPos += moveDistance;
		}
		for (int k = 0; k < this.TriggerVolumes.Count; k++)
		{
			this.TriggerVolumes[k].startPos += moveDistance;
		}
		for (int l = 0; l < this.InfoVolumes.Count; l++)
		{
			this.InfoVolumes[l].startPos += moveDistance;
		}
		for (int m = 0; m < this.WallVolumes.Count; m++)
		{
			this.WallVolumes[m].startPos += moveDistance;
		}
	}

	public static void TransientSleeperBlockIncrement(Vector3i point, int c)
	{
		if (XUiC_WoPropsSleeperVolume.selectedVolumeIndex >= 0)
		{
			PrefabInstance selectedPrefabInstance = XUiC_WoPropsSleeperVolume.selectedPrefabInstance;
			Prefab prefab = selectedPrefabInstance.prefab;
			if (XUiC_WoPropsSleeperVolume.selectedVolumeIndex >= prefab.SleeperVolumes.Count)
			{
				return;
			}
			if (prefab.IsPosInSleeperVolume(prefab.SleeperVolumes[XUiC_WoPropsSleeperVolume.selectedVolumeIndex], point - selectedPrefabInstance.boundingBoxPosition))
			{
				prefab.Transient_NumSleeperSpawns += c;
			}
		}
	}

	public string CalcSleeperInfo()
	{
		int num = 0;
		int num2 = 0;
		bool flag = false;
		for (int i = 0; i < this.SleeperVolumes.Count; i++)
		{
			Prefab.PrefabSleeperVolume prefabSleeperVolume = this.SleeperVolumes[i];
			int spawnCountMin = (int)prefabSleeperVolume.spawnCountMin;
			if (spawnCountMin < 0)
			{
				flag = true;
			}
			else
			{
				num += spawnCountMin;
			}
			int spawnCountMax = (int)prefabSleeperVolume.spawnCountMax;
			if (spawnCountMax < 0)
			{
				flag = true;
			}
			else
			{
				num2 += spawnCountMax;
			}
		}
		string text = string.Format("{0}, {1}-{2}", this.SleeperVolumes.Count, num, num2);
		if (flag)
		{
			text += "*";
		}
		return text;
	}

	public void CopyIntoLocal(ChunkCluster _cluster, Vector3i _destinationPos, bool _bOverwriteExistingBlocks, bool _bSetChunkToRegenerate, FastTags<TagGroup.Global> _questTags)
	{
		WorldBase world = _cluster.GetWorld();
		bool flag = world.IsEditor();
		if (!flag)
		{
			this.CopySleeperVolumes(world, null, _destinationPos);
		}
		Chunk chunkSync = _cluster.GetChunkSync(World.toChunkXZ(_destinationPos.x), World.toChunkXZ(_destinationPos.z));
		int seed = ((World)world).Seed;
		GameRandom gameRandom = (chunkSync != null) ? Utils.RandomFromSeedOnPos(chunkSync.X, chunkSync.Z, seed) : null;
		GameRandom gameRandom2 = GameRandomManager.Instance.CreateGameRandom((int)world.GetWorldTime());
		if (this.terrainFillerType == 0)
		{
			this.InitTerrainFillers();
		}
		for (int i = this.size.y + _destinationPos.y; i < 255; i++)
		{
			int y = World.toBlockY(i);
			bool flag2 = false;
			for (int j = 0; j < this.size.z; j++)
			{
				int v = j + _destinationPos.z;
				int num = World.toChunkXZ(v);
				int z = World.toBlockXZ(v);
				int k = 0;
				while (k < this.size.x)
				{
					int v2 = k + _destinationPos.x;
					int num2 = World.toChunkXZ(v2);
					int x = World.toBlockXZ(v2);
					if (chunkSync != null && chunkSync.X == num2 && chunkSync.Z == num)
					{
						goto IL_108;
					}
					chunkSync = _cluster.GetChunkSync(num2, num);
					if (chunkSync != null)
					{
						goto IL_108;
					}
					IL_156:
					k++;
					continue;
					IL_108:
					BlockValue block = chunkSync.GetBlock(x, y, z);
					if (block.isair || block.Block.shape.IsTerrain())
					{
						goto IL_156;
					}
					flag2 = true;
					if (!block.ischild)
					{
						chunkSync.SetBlock(world, x, y, z, BlockValue.Air, true, true, false, true, -1);
						goto IL_156;
					}
					goto IL_156;
				}
			}
			if (!flag2)
			{
				break;
			}
		}
		if (_bOverwriteExistingBlocks)
		{
			for (int l = 0; l < this.size.z; l++)
			{
				int num3 = l + _destinationPos.z;
				int num4 = World.toChunkXZ(num3);
				int z2 = World.toBlockXZ(num3);
				int m = 0;
				while (m < this.size.x)
				{
					int num5 = m + _destinationPos.x;
					int num6 = World.toChunkXZ(num5);
					int x2 = World.toBlockXZ(num5);
					if (chunkSync != null && chunkSync.X == num6 && chunkSync.Z == num4)
					{
						goto IL_250;
					}
					chunkSync = _cluster.GetChunkSync(num6, num4);
					if (chunkSync != null)
					{
						goto IL_250;
					}
					UnityEngine.Debug.LogError(string.Format("Chunk ({0}, {1}) unavailable during POI reset. Skipping reset for all POI blocks at XZ world position ({2},{3}).", new object[]
					{
						num6,
						num4,
						num5,
						num3
					}));
					IL_2B7:
					m++;
					continue;
					IL_250:
					for (int n = 0; n < this.size.y; n++)
					{
						int y2 = World.toBlockY(n + _destinationPos.y);
						BlockValue block2 = chunkSync.GetBlock(x2, y2, z2);
						if (block2.Block.isMultiBlock && !block2.ischild)
						{
							chunkSync.SetBlock(world, x2, y2, z2, BlockValue.Air, true, true, false, false, -1);
						}
					}
					goto IL_2B7;
				}
			}
		}
		for (int num7 = 0; num7 < this.size.z; num7++)
		{
			int num8 = num7 + _destinationPos.z;
			int num9 = World.toChunkXZ(num8);
			int num10 = World.toBlockXZ(num8);
			int num11 = 0;
			while (num11 < this.size.x)
			{
				int num12 = num11 + _destinationPos.x;
				int num13 = World.toChunkXZ(num12);
				int num14 = World.toBlockXZ(num12);
				if (chunkSync != null && chunkSync.X == num13 && chunkSync.Z == num9)
				{
					goto IL_3A7;
				}
				chunkSync = _cluster.GetChunkSync(num13, num9);
				GameRandomManager.Instance.FreeGameRandom(gameRandom);
				gameRandom = null;
				if (chunkSync != null)
				{
					goto IL_3A7;
				}
				UnityEngine.Debug.LogError(string.Format("Chunk ({0}, {1}) unavailable during POI reset. Skipping reset for all POI blocks at XZ world position ({2},{3}).", new object[]
				{
					num13,
					num9,
					num12,
					num8
				}));
				IL_7F4:
				num11++;
				continue;
				IL_3A7:
				if (gameRandom == null)
				{
					gameRandom = Utils.RandomFromSeedOnPos(num13, num9, seed);
				}
				bool flag3 = false;
				for (int num15 = 0; num15 < this.size.y; num15++)
				{
					WaterValue water = this.GetWater(num11, num15, num7);
					BlockValue blockValue = this.GetBlock(num11, num15, num7);
					if (this.bCopyAirBlocks || !blockValue.isair || water.HasMass())
					{
						int num16 = World.toBlockY(num15 + _destinationPos.y);
						BlockValue block3 = chunkSync.GetBlock(num14, num16, num10);
						BlockValue blockValue2 = blockValue;
						bool flag4 = false;
						if (!flag && blockValue.Block.IsSleeperBlock)
						{
							flag4 = true;
							blockValue = BlockValue.Air;
						}
						sbyte b = this.GetDensity(num11, num15, num7);
						if (!flag)
						{
							if (blockValue.type == this.terrainFillerType)
							{
								BlockValue blockValue3 = block3;
								Block block4 = blockValue3.Block;
								if (blockValue3.isair || block4 == null || !block4.shape.IsTerrain())
								{
									int terrainHeight = (int)chunkSync.GetTerrainHeight(num14, num10);
									blockValue3 = chunkSync.GetBlock(num14, terrainHeight, num10);
									block4 = blockValue3.Block;
									if (blockValue3.isair || block4 == null || !block4.shape.IsTerrain())
									{
										goto IL_7B8;
									}
								}
								blockValue = blockValue3;
								flag3 = true;
							}
							if (blockValue.type == this.terrainFiller2Type)
							{
								Block block5 = block3.Block;
								if (!block3.isair && block5 != null && block5.shape.IsTerrain())
								{
									blockValue = block3;
									b = 0;
								}
								else
								{
									blockValue = BlockValue.Air;
									b = MarchingCubes.DensityAir;
								}
							}
							if (blockValue.Block.isMultiBlock && MultiBlockManager.Instance.POIMBTrackingEnabled)
							{
								this.ProcessMultiBlock(ref blockValue, chunkSync, new Vector3i(num11, num15, num7), new Vector3i(num14, num16, num10), _questTags, _bOverwriteExistingBlocks);
							}
							else if (BlockPlaceholderMap.Instance.IsReplaceableBlockType(blockValue))
							{
								byte meta = blockValue.meta;
								blockValue = BlockPlaceholderMap.Instance.Replace(blockValue, GameManager.Instance.World.GetGameRandom(), chunkSync, num14, num16, num10, _questTags, _bOverwriteExistingBlocks, true);
								blockValue.meta = meta;
							}
						}
						if (b == 0)
						{
							b = MarchingCubes.DensityAir;
							if (blockValue.Block.shape.IsTerrain())
							{
								b = MarchingCubes.DensityTerrain;
							}
						}
						if (block3.ischild || (!_bOverwriteExistingBlocks && !block3.isair && !block3.Block.shape.IsTerrain()))
						{
							chunkSync.SetDensity(num14, num16, num10, b);
						}
						else
						{
							chunkSync.SetDecoAllowedSizeAt(num14, num10, EnumDecoAllowedSize.NoBigOnlySmall);
							if (!flag4)
							{
								long texture = this.GetTexture(num11, num15, num7);
								chunkSync.SetTextureFull(num14, num16, num10, texture);
							}
							chunkSync.SetBlock(world, num14, num16, num10, blockValue, true, true, !_questTags.IsEmpty, true, -1);
							chunkSync.SetWater(num14, num16, num10, water);
							Vector3i blockPos = new Vector3i(num11, num15, num7);
							TileEntity tileEntity;
							if (blockValue2.Block.IsTileEntitySavedInPrefab() && (tileEntity = this.GetTileEntity(blockPos)) != null)
							{
								TileEntity tileEntity2 = chunkSync.GetTileEntity(new Vector3i(num14, num16, num10));
								if (tileEntity2 == null)
								{
									tileEntity2 = tileEntity.Clone();
									tileEntity2.localChunkPos = new Vector3i(num14, num16, num10);
									tileEntity2.SetChunk(chunkSync);
									chunkSync.AddTileEntity(tileEntity2);
								}
								tileEntity2.CopyFrom(tileEntity);
								tileEntity2.localChunkPos = new Vector3i(num14, num16, num10);
							}
							BlockTrigger blockTrigger = this.GetBlockTrigger(blockPos);
							if (blockTrigger != null)
							{
								BlockTrigger blockTrigger2 = chunkSync.GetBlockTrigger(new Vector3i(num14, num16, num10));
								if (blockTrigger2 == null)
								{
									blockTrigger2 = blockTrigger.Clone();
									blockTrigger2.LocalChunkPos = new Vector3i(num14, num16, num10);
									blockTrigger2.Chunk = chunkSync;
									chunkSync.AddBlockTrigger(blockTrigger2);
								}
								blockTrigger2.CopyFrom(blockTrigger);
								blockTrigger2.LocalChunkPos = new Vector3i(num14, num16, num10);
								blockValue.Block.OnTriggerAddedFromPrefab(blockTrigger2, blockTrigger2.LocalChunkPos, blockValue, FastTags<TagGroup.Global>.Parse(this.questTags.ToString()));
							}
							if (blockValue.Block.shape.IsTerrain() && (int)chunkSync.GetTerrainHeight(num14, num10) < num16)
							{
								chunkSync.SetTerrainHeight(num14, num10, (byte)num16);
							}
							chunkSync.SetDensity(num14, num16, num10, b);
						}
					}
					IL_7B8:;
				}
				if (!flag3)
				{
					chunkSync.SetTopSoilBroken(num14, num10);
				}
				chunkSync.SetDecoAllowedSizeAt(num14, num10, EnumDecoAllowedSize.NoBigOnlySmall);
				if (_bSetChunkToRegenerate)
				{
					chunkSync.NeedsRegeneration = true;
					goto IL_7F4;
				}
				goto IL_7F4;
			}
		}
		this.ApplyDecoAllowed(_cluster, _destinationPos);
		GameRandomManager.Instance.FreeGameRandom(gameRandom);
		GameRandomManager.Instance.FreeGameRandom(gameRandom2);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ApplyDecoAllowed(ChunkCluster _cluster, Vector3i _prefabTargetPos)
	{
		int num = World.toChunkXZ(_prefabTargetPos.x);
		int num2 = World.toChunkXZ(_prefabTargetPos.z);
		int num3 = World.toChunkXZ(_prefabTargetPos.x + this.size.x - 1);
		int num4 = World.toChunkXZ(_prefabTargetPos.z + this.size.z - 1);
		for (int i = num2; i <= num4; i++)
		{
			for (int j = num; j <= num3; j++)
			{
				Chunk chunkSync = _cluster.GetChunkSync(j, i);
				if (chunkSync != null)
				{
					this.ApplyDecoAllowed(chunkSync, _prefabTargetPos);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ProcessMultiBlock(ref BlockValue targetBV, Chunk chunk, Vector3i prefabRelPos, Vector3i chunkRelPos, FastTags<TagGroup.Global> questTags, bool overwriteExistingBlocks)
	{
		if (!targetBV.Block.isMultiBlock)
		{
			UnityEngine.Debug.LogError("[MultiBlockManager] BlockValue passed into ProcessMultiBlock is not a MultiBlock.");
			return;
		}
		Vector3i one = chunk.GetWorldPos() + (chunkRelPos - prefabRelPos);
		Vector3i vector3i = prefabRelPos;
		if (targetBV.ischild)
		{
			vector3i += new Vector3i(targetBV.parentx, targetBV.parenty, targetBV.parentz);
		}
		Vector3i vector3i2 = one + vector3i;
		MultiBlockManager.TrackedBlockData trackedBlockData;
		BlockValue blockValue;
		if (MultiBlockManager.Instance.TryGetPOIMultiBlock(vector3i2, out trackedBlockData))
		{
			blockValue = new BlockValue(trackedBlockData.rawData);
		}
		else
		{
			BlockValue blockValue2;
			if (targetBV.ischild)
			{
				blockValue2 = this.GetBlock(vector3i.x, vector3i.y, vector3i.z);
			}
			else
			{
				blockValue2 = targetBV;
			}
			if (BlockPlaceholderMap.Instance.IsReplaceableBlockType(blockValue2))
			{
				byte meta = blockValue2.meta;
				blockValue = BlockPlaceholderMap.Instance.Replace(blockValue2, GameManager.Instance.World.GetGameRandom(), chunk, chunkRelPos.x, chunkRelPos.y, chunkRelPos.z, questTags, overwriteExistingBlocks, false);
				blockValue.meta = meta;
			}
			else
			{
				blockValue = blockValue2;
			}
			MultiBlockManager.Instance.DeregisterTrackedBlockData(vector3i2);
			if (!MultiBlockManager.Instance.TryRegisterPOIMultiBlock(vector3i2, blockValue))
			{
				UnityEngine.Debug.LogError("[MultiBlockManager] Failed to register POI MultiBlock.");
			}
		}
		if (blockValue.type == targetBV.type)
		{
			return;
		}
		if (blockValue.isair)
		{
			targetBV = BlockValue.Air;
			return;
		}
		if (blockValue.Block.isMultiBlock)
		{
			Vector3i dim = targetBV.Block.multiBlockPos.dim;
			Vector3i dim2 = blockValue.Block.multiBlockPos.dim;
			if (dim2 != dim)
			{
				if (dim2.x > dim.x || dim2.y > dim.y || dim2.z > dim.z)
				{
					UnityEngine.Debug.LogWarning(string.Concat(new string[]
					{
						"[MultiBlockManager] The replacement block \"",
						blockValue.Block.GetBlockName(),
						"\" is larger than the original block \"",
						targetBV.Block.GetBlockName(),
						"\" in dimensions. \n",
						string.Format("Replacement size: \"{0}\", Original size: \"{1}\". ", dim2, dim),
						string.Format("Parent world position: {0}.\n", vector3i2),
						"Child blocks of the replacement will not be placed outside the original block's dimensions. \nNote: We expect to see this warning when single-block helpers are used to place MultiBlocks at 45-degree rotations. Many of these instances will be resolved by converting to the new oversized block format in the near future. \nIn situations where 45-degree rotations aren't needed, helper blocks should be set to the maximum dimensions of any possible replacements. Affected prefabs may need to be re-saved to implement these changes."
					}));
				}
				if (dim2.x < dim.x || dim2.y < dim.y || dim2.z < dim.z)
				{
					Vector3i vector3i3;
					Vector3i vector3i4;
					MultiBlockManager.GetMinMaxWorldPositions(vector3i2, blockValue, out vector3i3, out vector3i4);
					Vector3i vector3i5 = one + prefabRelPos;
					if (vector3i5.x < vector3i3.x || vector3i5.x > vector3i4.x || vector3i5.y < vector3i3.y || vector3i5.y > vector3i4.y || vector3i5.z < vector3i3.z || vector3i5.z > vector3i4.z)
					{
						targetBV = BlockValue.Air;
						return;
					}
				}
			}
			targetBV.type = blockValue.type;
			if (!targetBV.ischild)
			{
				targetBV.rotation = blockValue.rotation;
			}
			return;
		}
		if (targetBV.ischild)
		{
			targetBV = BlockValue.Air;
			return;
		}
		targetBV.type = blockValue.type;
		targetBV.rotation = blockValue.rotation;
	}

	public void SnapTerrainToArea(ChunkCluster _cluster, Vector3i _destinationPos)
	{
		for (int i = -1; i < this.size.x + 1; i++)
		{
			for (int j = -1; j < this.size.z + 1; j++)
			{
				bool bUseHalfTerrainDensity = i == -1 || j == -1 || i == this.size.x || j == this.size.z;
				_cluster.SnapTerrainToPositionAtLocal(new Vector3i(_destinationPos.x + i, _destinationPos.y - 1, _destinationPos.z + j), true, bUseHalfTerrainDensity);
			}
		}
	}

	public void CopyEntitiesIntoWorld(World _world, Vector3i _destinationPos, ICollection<int> _entityIds, bool _bSpawnEnemies)
	{
		if (_entityIds != null)
		{
			_entityIds.Clear();
		}
		for (int i = 0; i < this.entities.Count; i++)
		{
			EntityCreationData entityCreationData = this.entities[i];
			entityCreationData.id = -1;
			if (_bSpawnEnemies || !EntityClass.list[entityCreationData.entityClass].bIsEnemyEntity)
			{
				Entity entity = EntityFactory.CreateEntity(entityCreationData);
				entity.SetPosition(entity.position + _destinationPos.ToVector3(), true);
				_world.SpawnEntityInWorld(entity);
				if (_entityIds != null)
				{
					_entityIds.Add(entity.entityId);
				}
			}
		}
	}

	public void CopyEntitiesIntoChunkStub(Chunk _chunk, Vector3i _destinationPos, ICollection<int> _entityIds, bool _bSpawnEnemies)
	{
		for (int i = 0; i < this.entities.Count; i++)
		{
			EntityCreationData entityCreationData = this.entities[i];
			if (EntityClass.list.ContainsKey(entityCreationData.entityClass) && (_bSpawnEnemies || !EntityClass.list[entityCreationData.entityClass].bIsEnemyEntity))
			{
				int v = Utils.Fastfloor(entityCreationData.pos.x) + _destinationPos.x;
				int v2 = Utils.Fastfloor(entityCreationData.pos.z) + _destinationPos.z;
				if (_chunk.X == World.toChunkXZ(v) && _chunk.Z == World.toChunkXZ(v2))
				{
					EntityCreationData entityCreationData2 = entityCreationData.Clone();
					entityCreationData2.pos += _destinationPos.ToVector3() + new Vector3(0f, 0.25f, 0f);
					entityCreationData2.id = EntityFactory.nextEntityID++;
					if (entityCreationData2.lootContainer != null)
					{
						entityCreationData2.lootContainer.entityId = entityCreationData2.id;
					}
					_chunk.AddEntityStub(entityCreationData2);
					if (_entityIds != null)
					{
						_entityIds.Add(entityCreationData2.id);
					}
				}
			}
		}
	}

	public static Vector3i SizeFromPositions(Vector3i _posStart, Vector3i _posEnd)
	{
		Vector3i vector3i = new Vector3i(Math.Min(_posStart.x, _posEnd.x), Math.Min(_posStart.y, _posEnd.y), Math.Min(_posStart.z, _posEnd.z));
		Vector3i vector3i2 = new Vector3i(Math.Max(_posStart.x, _posEnd.x), Math.Max(_posStart.y, _posEnd.y), Math.Max(_posStart.z, _posEnd.z));
		return new Vector3i(Math.Abs(vector3i2.x - vector3i.x) + 1, Math.Abs(vector3i2.y - vector3i.y) + 1, Math.Abs(vector3i2.z - vector3i.z) + 1);
	}

	public Vector3i copyFromWorld(World _world, Vector3i _posStart, Vector3i _posEnd)
	{
		Vector3i vector3i = Vector3i.Min(_posStart, _posEnd);
		Vector3i vector3i2 = Vector3i.Max(_posStart, _posEnd);
		this.size.x = Math.Abs(vector3i2.x - vector3i.x) + 1;
		this.size.y = Math.Abs(vector3i2.y - vector3i.y) + 1;
		this.size.z = Math.Abs(vector3i2.z - vector3i.z) + 1;
		this.localRotation = 0;
		this.InitData();
		this.tileEntities.Clear();
		int num = 0;
		int i = vector3i.y;
		while (i <= vector3i2.y)
		{
			int num2 = 0;
			int j = vector3i.x;
			while (j <= vector3i2.x)
			{
				int num3 = 0;
				int k = vector3i.z;
				while (k <= vector3i2.z)
				{
					BlockValue bv = _world.GetBlock(j, i, k);
					if (bv.isWater)
					{
						this.SetWater(j, i, k, WaterValue.Full);
						bv = BlockValue.Air;
					}
					if (!bv.ischild)
					{
						this.SetBlock(num2, num, num3, bv);
						this.SetWater(num2, num, num3, _world.GetWater(j, i, k));
						this.SetDensity(num2, num, num3, _world.GetDensity(0, j, i, k));
						this.SetTexture(num2, num, num3, _world.GetTexture(j, i, k));
						if (bv.Block.IsTileEntitySavedInPrefab())
						{
							Vector3i vector3i3 = new Vector3i(j, i, k);
							TileEntity tileEntity = _world.GetTileEntity(vector3i3);
							if (tileEntity != null)
							{
								TileEntity tileEntity2 = tileEntity.Clone();
								tileEntity2.localChunkPos = vector3i3 - vector3i;
								this.tileEntities.Add(tileEntity2.localChunkPos, tileEntity2);
							}
						}
					}
					k++;
					num3++;
				}
				j++;
				num2++;
			}
			i++;
			num++;
		}
		return vector3i;
	}

	public Vector3i CopyFromWorldWithEntities(World _world, Vector3i _posStart, Vector3i _posEnd, ICollection<int> _entityIds)
	{
		this.copyFromWorld(_world, _posStart, _posEnd);
		Vector3i vector3i = Vector3i.Min(_posStart, _posEnd);
		Vector3i vector3i2 = Vector3i.Max(_posStart, _posEnd);
		this.entities.Clear();
		int num = World.toChunkXZ(vector3i.x);
		int num2 = World.toChunkXZ(vector3i.z);
		int num3 = World.toChunkXZ(vector3i2.x);
		int num4 = World.toChunkXZ(vector3i2.z);
		Bounds bb = BoundsUtils.BoundsForMinMax((float)vector3i.x, (float)vector3i.y, (float)vector3i.z, (float)(vector3i2.x + 1), (float)(vector3i2.y + 1), (float)(vector3i2.z + 1));
		List<Entity> list = new List<Entity>();
		for (int i = num; i <= num3; i++)
		{
			for (int j = num2; j <= num4; j++)
			{
				Chunk chunk = (Chunk)_world.GetChunkSync(i, 0, j);
				if (chunk != null)
				{
					chunk.GetEntitiesInBounds(typeof(Entity), bb, list);
				}
			}
		}
		this.indexedBlockOffsets.Clear();
		this.triggerData.Clear();
		for (int k = num; k <= num3; k++)
		{
			for (int l = num2; l <= num4; l++)
			{
				Chunk chunk2 = (Chunk)_world.GetChunkSync(k, 0, l);
				if (chunk2 != null)
				{
					foreach (KeyValuePair<string, List<Vector3i>> keyValuePair in chunk2.IndexedBlocks.Dict)
					{
						if (keyValuePair.Value != null && keyValuePair.Value.Count > 0)
						{
							List<Vector3i> list2 = new List<Vector3i>();
							this.indexedBlockOffsets[keyValuePair.Key] = list2;
							foreach (Vector3i pos in keyValuePair.Value)
							{
								Vector3i one = chunk2.ToWorldPos(pos);
								Vector3 point = one.ToVector3();
								Vector3i item = one - vector3i;
								if (bb.Contains(point))
								{
									list2.Add(item);
								}
							}
						}
					}
					List<BlockTrigger> list3 = chunk2.GetBlockTriggers().list;
					for (int m = 0; m < list3.Count; m++)
					{
						BlockTrigger blockTrigger = list3[m].Clone();
						blockTrigger.LocalChunkPos = chunk2.ToWorldPos(list3[m].LocalChunkPos) - _posStart;
						this.triggerData.Add(blockTrigger.LocalChunkPos, blockTrigger);
					}
				}
			}
		}
		if (_entityIds != null)
		{
			_entityIds.Clear();
		}
		for (int n = 0; n < list.Count; n++)
		{
			Entity entity = list[n];
			if (!(entity is EntityPlayer))
			{
				EntityCreationData entityCreationData = new EntityCreationData(entity, true);
				entityCreationData.pos -= new Vector3(bb.min.x, bb.min.y, bb.min.z);
				this.entities.Add(entityCreationData);
				if (_entityIds != null)
				{
					_entityIds.Add(entity.entityId);
				}
			}
		}
		return vector3i;
	}

	public BlockValue Get(int relx, int absy, int relz)
	{
		int num = this.currX + relx;
		int num2 = this.currZ + relz;
		if (num >= 0 && num < this.size.x && absy >= 0 && absy < this.size.y && num2 >= 0 && num2 < this.size.z)
		{
			return this.GetBlock(num, absy, num2);
		}
		return BlockValue.Air;
	}

	public IChunk GetChunk(int x, int z)
	{
		long key = WorldChunkCache.MakeChunkKey(x, z);
		Prefab.PrefabChunk prefabChunk;
		if (!this.dictChunks.TryGetValue(key, out prefabChunk))
		{
			prefabChunk = new Prefab.PrefabChunk(this, x, z);
			this.dictChunks.Add(key, prefabChunk);
		}
		return prefabChunk;
	}

	public List<IChunk> GetChunks()
	{
		if (this.dictChunks.Count == 0)
		{
			int i = 0;
			int num = 0;
			while (i < this.size.x + 1)
			{
				int j = 0;
				int num2 = 0;
				while (j < this.size.z + 1)
				{
					this.GetChunk(num, num2);
					j += 16;
					num2++;
				}
				i += 16;
				num++;
			}
		}
		return this.dictChunks.Values.ToList<IChunk>();
	}

	public IChunk GetNeighborChunk(int x, int z)
	{
		return this.GetChunk(x, z);
	}

	public bool IsWater(int relx, int absy, int relz)
	{
		int num = this.currX + relx;
		int num2 = this.currZ + relz;
		return num >= 0 && num < this.size.x && absy >= 0 && absy < this.size.y && num2 >= 0 && num2 < this.size.z && this.GetWater(num, absy, num2).HasMass();
	}

	public bool IsAir(int relx, int absy, int relz)
	{
		int num = this.currX + relx;
		int num2 = this.currZ + relz;
		return num >= 0 && num < this.size.x && absy >= 0 && absy < this.size.y && num2 >= 0 && num2 < this.size.z && this.GetBlock(num, absy, num2).isair && !this.GetWater(num, absy, num2).HasMass();
	}

	public void Init(int _bX, int _bZ)
	{
		this.currX = _bX;
		this.currZ = _bZ;
		this.dictChunks = new Dictionary<long, Prefab.PrefabChunk>();
	}

	public void Clear()
	{
	}

	public void Cache()
	{
	}

	public void ToMesh(VoxelMesh[] _meshes)
	{
		new MeshGenerator(this).GenerateMesh(new Vector3i(-1, -1, -1), this.size + Vector3i.one, _meshes);
	}

	public void ToOptimizedColorCubeMesh(VoxelMesh _mesh)
	{
		new MeshGeneratorOptimizedMesh(this).GenerateColorCubeMesh(Vector3i.zero, this.size, _mesh);
	}

	public Transform ToTransform()
	{
		MeshFilter[][] array = new MeshFilter[MeshDescription.meshes.Length][];
		MeshRenderer[][] array2 = new MeshRenderer[MeshDescription.meshes.Length][];
		MeshCollider[][] array3 = new MeshCollider[MeshDescription.meshes.Length][];
		GameObject[] array4 = new GameObject[MeshDescription.meshes.Length];
		GameObject gameObject = new GameObject();
		gameObject.transform.parent = null;
		gameObject.name = "Prefab_" + this.PrefabName;
		GameObject gameObject2 = new GameObject("_BlockEntities");
		gameObject2.transform.parent = gameObject.transform;
		GameObject gameObject3 = new GameObject("Meshes");
		gameObject3.transform.parent = gameObject.transform;
		for (int i = 0; i < MeshDescription.meshes.Length; i++)
		{
			array4[i] = new GameObject(MeshDescription.meshes[i].Name);
			array4[i].transform.parent = gameObject3.transform;
			VoxelMesh.CreateMeshFilter(i, 0, array4[i], MeshDescription.meshes[i].Tag, false, out array[i], out array2[i], out array3[i]);
		}
		VoxelMesh[] array5 = new VoxelMesh[6];
		for (int j = 0; j < array5.Length; j++)
		{
			array5[j] = new VoxelMesh(j, 1024, VoxelMesh.CreateFlags.Default);
		}
		new MeshGenerator(this).GenerateMesh(new Vector3i(-1, -1, -1), this.size + Vector3i.one, array5);
		for (int k = 0; k < array5.Length; k++)
		{
			array5[k].CopyToMesh(array[k], array2[k], 0);
		}
		for (int l = 0; l < this.size.x; l++)
		{
			for (int m = 0; m < this.size.z; m++)
			{
				for (int n = 0; n < this.size.y; n++)
				{
					Vector3i vector3i = new Vector3i(l, n, m);
					BlockValue block = this.GetBlock(l, n, m);
					Block block2 = block.Block;
					if (!block2.isMultiBlock || !block.ischild)
					{
						BlockShapeModelEntity blockShapeModelEntity = block2.shape as BlockShapeModelEntity;
						if (blockShapeModelEntity != null)
						{
							Quaternion rotation = blockShapeModelEntity.GetRotation(block);
							Vector3 rotatedOffset = blockShapeModelEntity.GetRotatedOffset(block2, rotation);
							rotatedOffset.x += 0.5f;
							rotatedOffset.z += 0.5f;
							rotatedOffset.y += 0f;
							Vector3 localPosition = vector3i.ToVector3() + rotatedOffset;
							GameObject objectForType = GameObjectPool.Instance.GetObjectForType(blockShapeModelEntity.modelName);
							if (!(objectForType == null))
							{
								objectForType.SetActive(true);
								Transform transform = objectForType.transform;
								transform.parent = gameObject2.transform;
								transform.localScale = Vector3.one;
								transform.localPosition = localPosition;
								transform.localRotation = rotation;
							}
						}
					}
				}
			}
		}
		return gameObject.transform;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public List<BiomeDefinition> toBiomeArray(WorldBiomes _biomes, List<string> _biomeStrList)
	{
		List<BiomeDefinition> list = new List<BiomeDefinition>();
		for (int i = 0; i < _biomeStrList.Count; i++)
		{
			string name = _biomeStrList[i];
			BiomeDefinition biome;
			if ((biome = _biomes.GetBiome(name)) != null)
			{
				list.Add(biome);
			}
		}
		return list;
	}

	public string[] GetAllowedBiomes()
	{
		return this.allowedBiomes.ToArray();
	}

	public string[] GetAllowedZones()
	{
		return this.allowedZones.ToArray();
	}

	public bool IsAllowedZone(string _zone)
	{
		return this.allowedZones.ContainsCaseInsensitive(_zone);
	}

	public void AddAllowedZone(string _zone)
	{
		if (!this.IsAllowedZone(_zone))
		{
			this.allowedZones.Add(_zone);
		}
	}

	public void RemoveAllowedZone(string _zone)
	{
		int num = this.allowedZones.FindIndex((string _s) => _s.EqualsCaseInsensitive(_zone));
		if (num >= 0)
		{
			this.allowedZones.RemoveAt(num);
		}
	}

	public string[] GetAllowedTownships()
	{
		return this.allowedTownships.ToArray();
	}

	public void SetAllowedBiomes(string[] _b)
	{
		this.allowedBiomes = new List<string>(_b);
	}

	public List<BiomeDefinition> GetAllowedBiomes(WorldBiomes _biomes)
	{
		return this.toBiomeArray(_biomes, this.allowedBiomes);
	}

	public void CopyBlocksIntoChunkNoEntities(World _world, Chunk _chunk, Vector3i _prefabTargetPos, bool _bForceOverwriteBlocks)
	{
		bool flag = this.IsCullThisPrefab() && GameStats.GetInt(EnumGameStats.OptionsPOICulling) > 1;
		bool flag2 = _world.IsEditor();
		if (this.terrainFillerType == 0)
		{
			this.InitTerrainFillers();
		}
		Bounds aabb = _chunk.GetAABB();
		int num = 0;
		int num2 = 0;
		int num3 = _prefabTargetPos.x - (int)aabb.min.x;
		int num4;
		if (num3 >= 0)
		{
			num = num3;
			num4 = Utils.FastMin(16 - num3, this.size.x);
		}
		else
		{
			num2 = -1 * num3;
			num4 = Utils.FastMin(this.size.x + num3, 16);
		}
		int num5 = 0;
		int num6 = 0;
		int num7 = _prefabTargetPos.z - (int)aabb.min.z;
		int num8;
		if (num7 >= 0)
		{
			num5 = num7;
			num8 = Utils.FastMin(16 - num7, this.size.z);
		}
		else
		{
			num6 = -1 * num7;
			num8 = Utils.FastMin(this.size.z + num7, 16);
		}
		for (int i = 0; i < num8; i++)
		{
			int num9 = i + num5;
			for (int j = 0; j < num4; j++)
			{
				int num10 = j + num;
				int terrainHeight = (int)_chunk.GetTerrainHeight(num10, num9);
				int num11 = terrainHeight;
				BiomeDefinition biomeDefinition = null;
				BlockValue blockValue = BlockValue.Air;
				bool flag3 = false;
				for (int k = 0; k < this.size.y; k++)
				{
					BlockValue blockValue2 = this.GetBlock(j + num2, k, i + num6);
					WaterValue water = this.GetWater(j + num2, k, i + num6);
					Block block = blockValue2.Block;
					bool flag4 = false;
					if (block.IsSleeperBlock)
					{
						flag4 = true;
						blockValue2 = BlockValue.Air;
					}
					int num12 = k + _prefabTargetPos.y;
					if (num12 >= 0 && num12 < 255)
					{
						bool flag5 = this.bAllowTopSoilDecorations;
						if (blockValue2.type == this.terrainFillerType)
						{
							if (!flag2)
							{
								if (biomeDefinition == null)
								{
									int biomeId = (int)_chunk.GetBiomeId(num10, num9);
									biomeDefinition = _world.Biomes.GetBiome((byte)biomeId);
									if (biomeDefinition != null && biomeDefinition.m_TopSoilBlock != null)
									{
										blockValue = Block.GetBlockValue(biomeDefinition.m_TopSoilBlock, false);
									}
								}
								if (blockValue.isair)
								{
									BlockValue block2 = _chunk.GetBlock(num10, num12, num9);
									Block block3 = block2.Block;
									if (block2.isair || block3 == null || !block3.shape.IsTerrain())
									{
										int terrainHeight2 = (int)_chunk.GetTerrainHeight(num10, num9);
										block2 = _chunk.GetBlock(num10, terrainHeight2, num9);
									}
									blockValue = block2;
								}
								block = blockValue.Block;
								if (blockValue.isair || block == null || !block.shape.IsTerrain())
								{
									goto IL_715;
								}
								blockValue2 = blockValue;
							}
							if (block.multiBlockPos != null && block.multiBlockPos.dim.x != 1 && block.multiBlockPos.dim.y != 1)
							{
								int z = block.multiBlockPos.dim.z;
							}
						}
						sbyte b = this.GetDensity(j + num2, k, i + num6);
						if (blockValue2.type == this.terrainFiller2Type)
						{
							BlockValue block4 = _chunk.GetBlock(num10, num12, num9);
							Block block5 = block4.Block;
							if (!block4.isair && block5 != null && block5.shape.IsTerrain())
							{
								blockValue2 = block4;
								b = _chunk.GetDensity(num10, num12, num9);
							}
							else
							{
								blockValue2 = BlockValue.Air;
								b = MarchingCubes.DensityAir;
								if (num12 > 0 && _chunk.GetBlock(num10, num12 - 1, num9).Block.shape.IsTerrain())
								{
									sbyte density = _chunk.GetDensity(num10, num12 - 1, num9);
									b = MarchingCubes.DensityAir + density;
								}
							}
							block = blockValue2.Block;
						}
						if (!flag2)
						{
							if (blockValue2.Block.isMultiBlock && MultiBlockManager.Instance.POIMBTrackingEnabled)
							{
								this.ProcessMultiBlock(ref blockValue2, _chunk, new Vector3i(j + num2, k, i + num6), new Vector3i(num10, num12, num9), FastTags<TagGroup.Global>.none, _bForceOverwriteBlocks);
								block = blockValue2.Block;
							}
							else if (BlockPlaceholderMap.Instance.IsReplaceableBlockType(blockValue2))
							{
								blockValue2 = BlockPlaceholderMap.Instance.Replace(blockValue2, GameManager.Instance.World.GetGameRandom(), _chunk, num10, num12, num9, FastTags<TagGroup.Global>.none, _bForceOverwriteBlocks, true);
								block = blockValue2.Block;
							}
						}
						if (block.shape.IsTerrain() && k + _prefabTargetPos.y > num11)
						{
							num11 = num12;
							flag3 = true;
						}
						else if (!block.shape.IsTerrain() && k + _prefabTargetPos.y <= num11)
						{
							num11 = num12 - 1;
							flag3 = true;
						}
						if (b == 0)
						{
							if (block.shape.IsTerrain())
							{
								b = MarchingCubes.DensityTerrain;
							}
							else if (block.shape.IsSolidCube && k + _prefabTargetPos.y <= num11)
							{
								b = 1;
							}
							else
							{
								b = MarchingCubes.DensityAir;
							}
						}
						if (this.yOffset == 0)
						{
							sbyte density2 = _chunk.GetDensity(num10, num12, num9);
							if ((b >= 0 && density2 >= 0 && (density2 != MarchingCubes.DensityAir / 2 || (block.IsTerrainDecoration && !this.bCopyAirBlocks))) || (b < 0 && density2 < 0 && density2 != MarchingCubes.DensityTerrain / 2))
							{
								b = density2;
							}
						}
						_chunk.SetDecoAllowedSizeAt(num10, num9, EnumDecoAllowedSize.NoBigOnlySmall);
						Vector3i vector3i = new Vector3i(j + num2, k, i + num6);
						if (flag && !block.shape.IsTerrain() && this.IsInsidePrefab(vector3i.x, vector3i.y, vector3i.z))
						{
							_chunk.AddInsideDevicePosition(num10, num12, num9, blockValue2);
						}
						if (this.bCopyAirBlocks || !blockValue2.isair || k < -this.yOffset || water.HasMass())
						{
							BlockValue block6 = _chunk.GetBlock(num10, num12, num9);
							if (!_bForceOverwriteBlocks && !block6.Block.shape.IsTerrain() && !block6.isair && (block6.ischild || block6.type == blockValue2.type))
							{
								_chunk.SetDensity(num10, num12, num9, b);
							}
							else
							{
								if (!flag4)
								{
									long texture = this.GetTexture(j + num2, k, i + num6);
									_chunk.SetTextureFull(num10, num12, num9, texture);
								}
								_chunk.SetBlock(_world, num10, num12, num9, blockValue2, true, true, false, true, -1);
								_chunk.SetWater(num10, num12, num9, water);
								_chunk.SetDensity(num10, num12, num9, b);
								TileEntity tileEntity;
								if (blockValue2.Block.IsTileEntitySavedInPrefab() && (tileEntity = this.GetTileEntity(vector3i)) != null)
								{
									TileEntity tileEntity2 = _chunk.GetTileEntity(new Vector3i(num10, num12, num9));
									if (tileEntity2 == null)
									{
										tileEntity2 = tileEntity.Clone();
										tileEntity2.localChunkPos = new Vector3i(num10, num12, num9);
										tileEntity2.SetChunk(_chunk);
										_chunk.AddTileEntity(tileEntity2);
									}
									tileEntity2.CopyFrom(tileEntity);
									tileEntity2.localChunkPos = new Vector3i(num10, num12, num9);
								}
								BlockTrigger blockTrigger;
								if ((blockTrigger = this.GetBlockTrigger(vector3i)) != null)
								{
									BlockTrigger blockTrigger2 = _chunk.GetBlockTrigger(new Vector3i(num10, num12, num9));
									if (blockTrigger2 == null)
									{
										blockTrigger2 = blockTrigger.Clone();
										blockTrigger2.LocalChunkPos = new Vector3i(num10, num12, num9);
										blockTrigger2.Chunk = _chunk;
										_chunk.AddBlockTrigger(blockTrigger2);
									}
									blockTrigger2.CopyFrom(blockTrigger);
									blockTrigger2.LocalChunkPos = new Vector3i(num10, num12, num9);
								}
							}
						}
					}
					IL_715:;
				}
				if (flag3 && (num11 > terrainHeight || _prefabTargetPos.y + this.size.y >= terrainHeight))
				{
					_chunk.SetTerrainHeight(num10, num9, (byte)num11);
				}
				_chunk.SetTopSoilBroken(num10, num9);
			}
		}
		this.CopySleeperVolumes(_world, _chunk, _prefabTargetPos);
		this.ApplyDecoAllowed(_chunk, _prefabTargetPos);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ApplyDecoAllowed(Chunk _chunk, Vector3i _prefabTargetPos)
	{
		foreach (int offset in this.decoAllowedBlockIndices)
		{
			int x;
			int y;
			int z;
			this.offsetToCoordRotated(offset, out x, out y, out z);
			BlockValue block = this.GetBlock(x, y, z);
			DecoUtils.ApplyDecoAllowed(_chunk, _prefabTargetPos + new Vector3i(x, y, z), block);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateBlockStatistics(BlockValue bv, Block b)
	{
		if (!Block.BlocksLoaded || b == null)
		{
			return;
		}
		this.statistics.cntWindows = this.statistics.cntWindows + ((b.BlockTag == BlockTags.Window) ? 1 : 0);
		this.statistics.cntDoors = this.statistics.cntDoors + ((b.BlockTag == BlockTags.Door) ? 1 : 0);
		this.statistics.cntBlockEntities = this.statistics.cntBlockEntities + ((b.shape is BlockShapeModelEntity && !bv.ischild && (!(b is BlockModelTree) || bv.meta == 0)) ? 1 : 0);
		this.statistics.cntBlockModels = this.statistics.cntBlockModels + ((b.shape is BlockShapeExt3dModel && !bv.ischild) ? 1 : 0);
		this.statistics.cntSolid = this.statistics.cntSolid + ((!bv.isair) ? 1 : 0);
	}

	public Prefab.BlockStatistics GetBlockStatistics()
	{
		return this.statistics;
	}

	public List<EntityCreationData> GetEntities()
	{
		return this.entities;
	}

	public void Mirror(EnumMirrorAlong _axis)
	{
		Prefab.Data data = this.CellsToArrays();
		Prefab.Data data2 = default(Prefab.Data);
		data2.Init(this.GetBlockCount());
		BlockValue air = BlockValue.Air;
		for (int i = 0; i < this.size.x; i++)
		{
			for (int j = 0; j < this.size.z; j++)
			{
				for (int k = 0; k < this.size.y; k++)
				{
					int num = this.CoordToOffset(this.localRotation, i, k, j);
					WaterValue waterValue = data.m_Water[num];
					air.rawData = data.m_Blocks[num];
					if (!air.ischild && (!air.isair || waterValue.HasMass()))
					{
						Block block = air.Block;
						BlockShape shape = block.shape;
						int num2 = (int)((byte)BlockShapeNew.MirrorStatic(_axis, (int)air.rotation, shape.SymmetryType));
						Vector3i pos = new Vector3i(i, k, j);
						Vector3i vector3i = GameUtils.Mirror(_axis, pos, this.size);
						if (block.isMultiBlock)
						{
							Vector3 point = new Vector3((block.multiBlockPos.dim.x % 2 == 0) ? -0.5f : 0f, (block.multiBlockPos.dim.y % 2 == 0) ? -0.5f : 0f, (block.multiBlockPos.dim.z % 2 == 0) ? -0.5f : 0f);
							Vector3 b = BlockShapeNew.GetRotationStatic((int)air.rotation) * point;
							Vector3 pos2 = GameUtils.Mirror(_axis, vector3i.ToVector3() + new Vector3(0.5f, 0.5f, 0.5f), this.size) + b;
							Vector3 a = GameUtils.Mirror(_axis, pos2, this.size);
							Vector3 b2 = BlockShapeNew.GetRotationStatic(num2) * point;
							vector3i = World.worldToBlockPos(a - b2);
						}
						int num3 = this.CoordToOffset(this.localRotation, vector3i.x, vector3i.y, vector3i.z);
						if (block.MirrorSibling != 0)
						{
							air.type = block.MirrorSibling;
						}
						air.rotation = (byte)num2;
						data2.m_Blocks[num3] = air.rawData;
						data2.m_Damage[num3] = data.m_Damage[num];
						data2.m_Density[num3] = data.m_Density[num];
						data2.m_Textures[num3] = this.mirrorTexture(_axis, shape, (int)air.rotation, num2, data.m_Textures[num]);
						data2.m_Water[num3] = waterValue;
					}
				}
			}
		}
		this.CellsFromArrays(ref data2);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public long mirrorTexture(EnumMirrorAlong _axis, BlockShape _shape, int _sourceRot, int _targetRot, long _tex)
	{
		long num = 0L;
		for (int i = 0; i < 6; i++)
		{
			BlockFace face = (BlockFace)i;
			BlockFace blockFace;
			BlockFace blockFace2;
			_shape.MirrorFace(_axis, _sourceRot, _targetRot, face, out blockFace, out blockFace2);
			long num2 = _tex >> (int)((BlockFace)8 * blockFace) & 255L;
			num2 <<= (int)((BlockFace)8 * blockFace2);
			num |= num2;
		}
		return num;
	}

	public void CloneSleeperVolume(string name, Vector3i boundingBoxPosition, int idx)
	{
		Prefab.PrefabSleeperVolume prefabSleeperVolume = this.SleeperVolumes[idx];
		this.AddSleeperVolume(name, boundingBoxPosition, prefabSleeperVolume.startPos + new Vector3i(0, prefabSleeperVolume.size.y + 1, 0), prefabSleeperVolume.size, prefabSleeperVolume.groupId, prefabSleeperVolume.groupName, (int)prefabSleeperVolume.spawnCountMin, (int)prefabSleeperVolume.spawnCountMax);
	}

	public int AddSleeperVolume(string _prefabInstanceName, Vector3i bbPos, Vector3i startPos, Vector3i size, short groupId, string _groupName, int _spawnMin, int _spawnMax)
	{
		int result = -1;
		Prefab.PrefabSleeperVolume prefabSleeperVolume = null;
		for (int i = 0; i < this.SleeperVolumes.Count; i++)
		{
			if (!this.SleeperVolumes[i].used)
			{
				result = i;
				prefabSleeperVolume = this.SleeperVolumes[i];
				break;
			}
		}
		if (prefabSleeperVolume == null)
		{
			prefabSleeperVolume = new Prefab.PrefabSleeperVolume();
			result = this.SleeperVolumes.Count;
			this.SleeperVolumes.Add(prefabSleeperVolume);
		}
		prefabSleeperVolume.Use(startPos, size, groupId, _groupName, false, false, _spawnMin, _spawnMax, 0);
		string name = _prefabInstanceName + "_" + result.ToString();
		this.AddSleeperVolumeSelectionBox(prefabSleeperVolume, name, bbPos + startPos);
		SelectionBoxManager.Instance.SetActive("SleeperVolume", name, true);
		return result;
	}

	public void SetSleeperVolume(string _prefabInstanceName, Vector3i _prefabInstanceBoundingBox, int _index, Prefab.PrefabSleeperVolume _volumeSettings)
	{
		while (_index >= this.SleeperVolumes.Count)
		{
			this.SleeperVolumes.Add(new Prefab.PrefabSleeperVolume());
		}
		bool used = this.SleeperVolumes[_index].used;
		this.SleeperVolumes[_index] = _volumeSettings;
		string name = _prefabInstanceName + "_" + _index.ToString();
		if (!_volumeSettings.used)
		{
			if (used)
			{
				SelectionBoxManager.Instance.GetCategory("SleeperVolume").RemoveBox(name);
			}
			return;
		}
		if (!used)
		{
			this.AddSleeperVolumeSelectionBox(_volumeSettings, name, _prefabInstanceBoundingBox + _volumeSettings.startPos);
			SelectionBoxManager.Instance.SetActive("SleeperVolume", name, true);
			return;
		}
		SelectionBoxManager.Instance.GetCategory("SleeperVolume").GetBox(name).SetPositionAndSize(_prefabInstanceBoundingBox + _volumeSettings.startPos, _volumeSettings.size);
		SelectionBoxManager.Instance.SetUserData("SleeperVolume", name, _volumeSettings);
	}

	public void AddSleeperVolumeSelectionBox(Prefab.PrefabSleeperVolume _volume, string _name, Vector3i _pos)
	{
		SelectionBoxManager.Instance.GetCategory("SleeperVolume").AddBox(_name, _pos, _volume.size, false, false).UserData = _volume;
	}

	public short FindSleeperVolumeFreeGroupId()
	{
		int num = 0;
		for (int i = 0; i < this.SleeperVolumes.Count; i++)
		{
			Prefab.PrefabSleeperVolume prefabSleeperVolume = this.SleeperVolumes[i];
			if ((int)prefabSleeperVolume.groupId > num)
			{
				num = (int)prefabSleeperVolume.groupId;
			}
		}
		return (short)(num + 1);
	}

	public int AddTeleportVolume(string _prefabInstanceName, Vector3i bbPos, Vector3i startPos, Vector3i size)
	{
		Prefab.PrefabTeleportVolume prefabTeleportVolume = new Prefab.PrefabTeleportVolume();
		int count = this.TeleportVolumes.Count;
		this.TeleportVolumes.Add(prefabTeleportVolume);
		prefabTeleportVolume.Use(startPos, size);
		string name = _prefabInstanceName + "_" + count.ToString();
		this.AddTeleportVolumeSelectionBox(prefabTeleportVolume, name, bbPos + startPos);
		SelectionBoxManager.Instance.SetActive("TraderTeleport", name, true);
		return count;
	}

	public void SetTeleportVolume(string _prefabInstanceName, Vector3i _prefabInstanceBoundingBox, int _index, Prefab.PrefabTeleportVolume _volumeSettings, bool remove = false)
	{
		while (_index >= this.TeleportVolumes.Count)
		{
			this.TeleportVolumes.Add(new Prefab.PrefabTeleportVolume());
		}
		if (!remove)
		{
			this.TeleportVolumes[_index] = _volumeSettings;
		}
		else
		{
			this.TeleportVolumes.RemoveAt(_index);
		}
		string name = _prefabInstanceName + "_" + _index.ToString();
		SelectionBoxManager.Instance.GetCategory("TraderTeleport").RemoveBox(name);
		if (!remove)
		{
			this.AddTeleportVolumeSelectionBox(_volumeSettings, name, _prefabInstanceBoundingBox + _volumeSettings.startPos);
			SelectionBoxManager.Instance.SetActive("TraderTeleport", name, true);
		}
	}

	public void AddTeleportVolumeSelectionBox(Prefab.PrefabTeleportVolume _volume, string _name, Vector3i _pos)
	{
		SelectionBoxManager.Instance.GetCategory("TraderTeleport").AddBox(_name, _pos, _volume.size, false, false).UserData = _volume;
	}

	public int AddInfoVolume(string _prefabInstanceName, Vector3i bbPos, Vector3i startPos, Vector3i size)
	{
		Prefab.PrefabInfoVolume prefabInfoVolume = new Prefab.PrefabInfoVolume();
		int count = this.InfoVolumes.Count;
		this.InfoVolumes.Add(prefabInfoVolume);
		prefabInfoVolume.Use(startPos, size);
		string name = _prefabInstanceName + "_" + count.ToString();
		this.AddInfoVolumeSelectionBox(prefabInfoVolume, name, bbPos + startPos);
		SelectionBoxManager.Instance.SetActive("InfoVolume", name, true);
		return count;
	}

	public void SetInfoVolume(string _prefabInstanceName, Vector3i _prefabInstanceBoundingBox, int _index, Prefab.PrefabInfoVolume _volumeSettings, bool remove = false)
	{
		while (_index >= this.InfoVolumes.Count)
		{
			this.InfoVolumes.Add(new Prefab.PrefabInfoVolume());
		}
		if (!remove)
		{
			this.InfoVolumes[_index] = _volumeSettings;
		}
		else
		{
			this.InfoVolumes.RemoveAt(_index);
		}
		string name = _prefabInstanceName + "_" + _index.ToString();
		SelectionBoxManager.Instance.GetCategory("InfoVolume").RemoveBox(name);
		if (!remove)
		{
			this.AddInfoVolumeSelectionBox(_volumeSettings, name, _prefabInstanceBoundingBox + _volumeSettings.startPos);
			SelectionBoxManager.Instance.SetActive("InfoVolume", name, true);
		}
	}

	public void AddInfoVolumeSelectionBox(Prefab.PrefabInfoVolume _volume, string _name, Vector3i _pos)
	{
		SelectionBoxManager.Instance.GetCategory("InfoVolume").AddBox(_name, _pos, _volume.size, false, false).UserData = _volume;
	}

	public int AddWallVolume(string _prefabInstanceName, Vector3i bbPos, Vector3i startPos, Vector3i size)
	{
		Prefab.PrefabWallVolume prefabWallVolume = new Prefab.PrefabWallVolume();
		int count = this.WallVolumes.Count;
		this.WallVolumes.Add(prefabWallVolume);
		prefabWallVolume.Use(startPos, size);
		string name = _prefabInstanceName + "_" + count.ToString();
		this.AddWallVolumeSelectionBox(prefabWallVolume, name, bbPos + startPos);
		SelectionBoxManager.Instance.SetActive("WallVolume", name, true);
		return count;
	}

	public void SetWallVolume(string _prefabInstanceName, Vector3i _prefabInstanceBoundingBox, int _index, Prefab.PrefabWallVolume _volumeSettings, bool remove = false)
	{
		while (_index >= this.WallVolumes.Count)
		{
			this.WallVolumes.Add(new Prefab.PrefabWallVolume());
		}
		if (!remove)
		{
			this.WallVolumes[_index] = _volumeSettings;
		}
		else
		{
			this.WallVolumes.RemoveAt(_index);
		}
		string name = _prefabInstanceName + "_" + _index.ToString();
		SelectionBoxManager.Instance.GetCategory("WallVolume").RemoveBox(name);
		if (!remove)
		{
			this.AddWallVolumeSelectionBox(_volumeSettings, name, _prefabInstanceBoundingBox + _volumeSettings.startPos);
			SelectionBoxManager.Instance.SetActive("WallVolume", name, true);
		}
	}

	public void AddWallVolumeSelectionBox(Prefab.PrefabWallVolume _volume, string _name, Vector3i _pos)
	{
		SelectionBoxManager.Instance.GetCategory("WallVolume").AddBox(_name, _pos, _volume.size, false, false).UserData = _volume;
	}

	public int AddTriggerVolume(string _prefabInstanceName, Vector3i bbPos, Vector3i startPos, Vector3i size)
	{
		Prefab.PrefabTriggerVolume prefabTriggerVolume = new Prefab.PrefabTriggerVolume();
		int count = this.TriggerVolumes.Count;
		this.TriggerVolumes.Add(prefabTriggerVolume);
		prefabTriggerVolume.Use(startPos, size);
		string name = _prefabInstanceName + "_" + count.ToString();
		this.AddTriggerVolumeSelectionBox(prefabTriggerVolume, name, bbPos + startPos);
		SelectionBoxManager.Instance.SetActive("TriggerVolume", name, true);
		return count;
	}

	public void SetTriggerVolume(string _prefabInstanceName, Vector3i _prefabInstanceBoundingBox, int _index, Prefab.PrefabTriggerVolume _volumeSettings, bool remove = false)
	{
		while (_index >= this.TriggerVolumes.Count)
		{
			this.TriggerVolumes.Add(new Prefab.PrefabTriggerVolume());
		}
		if (!remove)
		{
			this.TriggerVolumes[_index] = _volumeSettings;
		}
		else
		{
			this.TriggerVolumes.RemoveAt(_index);
		}
		string name = _prefabInstanceName + "_" + _index.ToString();
		SelectionBoxManager.Instance.GetCategory("TriggerVolume").RemoveBox(name);
		if (!remove)
		{
			this.AddTriggerVolumeSelectionBox(_volumeSettings, name, _prefabInstanceBoundingBox + _volumeSettings.startPos);
			SelectionBoxManager.Instance.SetActive("TriggerVolume", name, true);
		}
	}

	public void AddTriggerVolumeSelectionBox(Prefab.PrefabTriggerVolume _volume, string _name, Vector3i _pos)
	{
		SelectionBoxManager.Instance.GetCategory("TriggerVolume").AddBox(_name, _pos, _volume.size, false, false).UserData = _volume;
	}

	public void AddNewPOIMarker(string _prefabInstanceName, Vector3i bbPos, Vector3i _start, Vector3i _size, string _group, FastTags<TagGroup.Poi> _tags, Prefab.Marker.MarkerTypes _type, bool isSelected = false)
	{
		this.POIMarkers.Add(new Prefab.Marker(_start, _size, _type, _group, _tags));
		this.AddPOIMarker(_prefabInstanceName, bbPos, _start, _size, _group, _tags, _type, this.POIMarkers.Count - 1, isSelected);
	}

	public void AddPOIMarker(string _prefabInstanceName, Vector3i bbPos, Vector3i _start, Vector3i _size, string _group, FastTags<TagGroup.Poi> _tags, Prefab.Marker.MarkerTypes _type, int _index, bool isSelected = false)
	{
		this.AddPOIMarkerSelectionBox(this.POIMarkers[_index], _index, bbPos + _start, isSelected);
	}

	public void AddPOIMarkerSelectionBox(Prefab.Marker _marker, int _index, Vector3i _pos, bool isSelected = false)
	{
		string name = "POIMarker_" + _index.ToString();
		_marker.Name = name;
		SelectionBox selectionBox = SelectionBoxManager.Instance.GetCategory("POIMarker").AddBox(name, _pos, _marker.Size, false, false);
		selectionBox.bDrawDirection = true;
		selectionBox.bAlwaysDrawDirection = true;
		SelectionBoxManager.Instance.SetUserData("POIMarker", name, _marker);
		SelectionBoxManager.Instance.SetActive("POIMarker", name, true);
		float facing = 0f;
		switch (_marker.Rotations)
		{
		case 1:
			facing = (float)((_marker.MarkerType == Prefab.Marker.MarkerTypes.PartSpawn) ? 90 : 270);
			break;
		case 2:
			facing = 180f;
			break;
		case 3:
			facing = (float)((_marker.MarkerType == Prefab.Marker.MarkerTypes.PartSpawn) ? 270 : 90);
			break;
		}
		SelectionBoxManager.Instance.SetFacingDirection("POIMarker", name, facing);
		POIMarkerToolManager.RegisterPOIMarker(selectionBox);
		if (isSelected)
		{
			POIMarkerToolManager.SelectionChanged(selectionBox);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public ArrayListMP<int> loadIdMapping(string _directory, string _prefabFileName, bool _allowMissingBlocks)
	{
		if (!Block.BlocksLoaded)
		{
			Log.Error("Block data not loaded");
			return null;
		}
		string text = _directory + "/" + _prefabFileName + ".blocks.nim";
		if (!SdFile.Exists(text))
		{
			Log.Error("Loading prefab \"" + _prefabFileName + "\" failed: Block name to ID mapping file missing.");
			return null;
		}
		ArrayListMP<int> result;
		using (NameIdMapping nameIdMapping = MemoryPools.poolNameIdMapping.AllocSync(true))
		{
			nameIdMapping.InitMapping(text, Block.MAX_BLOCKS);
			if (!nameIdMapping.LoadFromFile())
			{
				result = null;
			}
			else
			{
				Block missingBlock = null;
				if (_allowMissingBlocks)
				{
					missingBlock = Block.GetBlockByName(Prefab.MISSING_BLOCK_NAME, false);
					this.blockTypeMissingBlock = ((missingBlock != null) ? missingBlock.blockID : -1);
				}
				result = nameIdMapping.createIdTranslationTable(delegate(string _blockName)
				{
					Block blockByName = Block.GetBlockByName(_blockName, false);
					if (blockByName == null)
					{
						return -1;
					}
					return blockByName.blockID;
				}, delegate(string _name, int _id)
				{
					if (!_allowMissingBlocks)
					{
						Log.Error(string.Format("Loading prefab \"{0}\" failed: Block \"{1}\" ({2}) used in prefab is unknown.", _prefabFileName, _name, _id));
						return -1;
					}
					if (missingBlock == null)
					{
						Log.Error(string.Format("Loading prefab \"{0}\" failed: Block \"{1}\" ({2}) used in prefab is unknown and the replacement block \"{3}\" was not found.", new object[]
						{
							_prefabFileName,
							_name,
							_id,
							Prefab.MISSING_BLOCK_NAME
						}));
						return -1;
					}
					Log.Warning(string.Format("Loading prefab \"{0}\": Block \"{1}\" ({2}) used in prefab is unknown and getting replaced by \"{3}\".", new object[]
					{
						_prefabFileName,
						_name,
						_id,
						Prefab.MISSING_BLOCK_NAME
					}));
					return missingBlock.blockID;
				});
			}
		}
		return result;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool doRaycast(Ray ray, out RaycastHit hitInfo, Vector3i _min)
	{
		bool flag = Physics.Raycast(ray, out hitInfo, 255f, 1073807360);
		if (!flag)
		{
			return false;
		}
		Vector3 vector = hitInfo.point + ray.direction * 0.01f;
		Vector3i vector3i = new Vector3i(Utils.Fastfloor(vector.x), Utils.Fastfloor(vector.y), Utils.Fastfloor(vector.z));
		Block block = this.GetBlock(vector3i.x - _min.x, vector3i.y - _min.y, vector3i.z - _min.z).Block;
		if (block.bImposterDontBlock || block.bImposterExclude)
		{
			ray.origin = hitInfo.point + ray.direction * 0.01f;
			flag = Physics.Raycast(ray, out hitInfo, 255f, 1073807360);
		}
		return flag;
	}

	public EnumInsideOutside[] UpdateInsideOutside(Vector3i _min, Vector3i _max)
	{
		EnumInsideOutside[] array = new EnumInsideOutside[this.GetBlockCount()];
		BlockValue air = BlockValue.Air;
		uint[] blocks = this.CellsToArrays().m_Blocks;
		for (int i = _min.x; i <= _max.x; i++)
		{
			for (int j = _min.z; j <= _max.z; j++)
			{
				int num = _max.y;
				Ray ray = new Ray(Vector3.zero, Vector3.down);
				bool flag = false;
				float num2 = 0f;
				while (!flag && num2 <= 1f)
				{
					float num3 = 0f;
					while (!flag && num3 <= 1f)
					{
						ray.origin = new Vector3((float)i + num2, (float)(_max.y + 3), (float)j + num3);
						RaycastHit raycastHit;
						if (this.doRaycast(ray, out raycastHit, _min))
						{
							num = Utils.FastMin(num, Utils.Fastfloor(raycastHit.point.y + ray.direction.y * 0.1f));
						}
						else
						{
							num = _min.y;
							flag = true;
						}
						num3 += 0.25f;
					}
					num2 += 0.25f;
				}
				int k = i - _min.x + (num - _min.y) * this.size.x + (j - _min.z) * this.size.x * this.size.y;
				if (k >= 0 && k < array.Length)
				{
					while (k > 0)
					{
						air.rawData = blocks[k];
						if (!air.isair)
						{
							break;
						}
						k -= this.size.x;
						num--;
					}
					if (k > 0)
					{
						air.rawData = blocks[k];
						if (air.ischild)
						{
							int type = air.type;
							while (k > 0)
							{
								air.rawData = blocks[k];
								if (air.type != type)
								{
									break;
								}
								k -= this.size.x;
								num--;
							}
						}
					}
				}
				for (int l = _max.y; l >= num; l--)
				{
					k = i - _min.x + (l - _min.y) * this.size.x + (j - _min.z) * this.size.x * this.size.y;
					if (k >= 0 && k < array.Length)
					{
						array[k] = EnumInsideOutside.Outside;
					}
				}
			}
		}
		for (int m = _min.z; m <= _max.z; m++)
		{
			for (int n = _min.y; n <= _max.y; n++)
			{
				int num4 = _min.x;
				Ray ray = new Ray(Vector3.zero, Vector3.right);
				bool flag2 = false;
				float num5 = 0f;
				while (!flag2 && num5 <= 1f)
				{
					float num6 = 0f;
					while (!flag2 && num6 <= 1f)
					{
						ray.origin = new Vector3((float)(_min.x - 3), (float)n + num5, (float)m + num6);
						RaycastHit raycastHit;
						if (this.doRaycast(ray, out raycastHit, _min))
						{
							num4 = Utils.FastMax(num4, Utils.Fastfloor(raycastHit.point.x + ray.direction.x * 0.1f));
						}
						else
						{
							num4 = _max.x;
							flag2 = true;
						}
						num6 += 0.25f;
					}
					num5 += 0.25f;
				}
				int num7 = num4 - _min.x + (n - _min.y) * this.size.x + (m - _min.z) * this.size.x * this.size.y;
				if (num7 >= 0 && num7 < array.Length)
				{
					while (num7 < blocks.Length - 1)
					{
						air.rawData = blocks[num7];
						if (!air.isair)
						{
							break;
						}
						num7++;
						num4++;
					}
					if (num7 < array.Length)
					{
						air.rawData = blocks[num7];
						if (air.ischild)
						{
							int type2 = air.type;
							while (num7 > 0)
							{
								air.rawData = blocks[num7];
								if (air.type != type2)
								{
									break;
								}
								num7++;
								num4++;
							}
						}
					}
				}
				for (int num8 = _min.x; num8 <= num4; num8++)
				{
					num7 = num8 - _min.x + (n - _min.y) * this.size.x + (m - _min.z) * this.size.x * this.size.y;
					if (num7 >= 0 && num7 < array.Length)
					{
						array[num7] = EnumInsideOutside.Outside;
					}
				}
				int num9 = _max.x;
				ray = new Ray(Vector3.zero, Vector3.left);
				flag2 = false;
				float num10 = 0f;
				while (!flag2 && num10 <= 1f)
				{
					float num11 = 0f;
					while (!flag2 && num11 <= 1f)
					{
						ray.origin = new Vector3((float)(_max.x + 3), (float)n + num10, (float)m + num11);
						RaycastHit raycastHit;
						if (this.doRaycast(ray, out raycastHit, _min))
						{
							num9 = Utils.FastMin(num9, Utils.Fastfloor(raycastHit.point.x + ray.direction.x * 0.1f));
						}
						else
						{
							num9 = _min.x;
							flag2 = true;
						}
						num11 += 0.25f;
					}
					num10 += 0.25f;
				}
				num7 = num9 - _min.x + (n - _min.y) * this.size.x + (m - _min.z) * this.size.x * this.size.y;
				if (num7 >= 0 && num7 < array.Length)
				{
					while (num7 > 0)
					{
						air.rawData = blocks[num7];
						if (!air.isair)
						{
							break;
						}
						num7--;
						num9--;
					}
					if (num7 > 0)
					{
						air.rawData = blocks[num7];
						if (air.ischild)
						{
							int type3 = air.type;
							while (num7 > 0)
							{
								air.rawData = blocks[num7];
								if (air.type != type3)
								{
									break;
								}
								num7--;
								num9--;
							}
						}
					}
				}
				for (int num12 = _max.x; num12 >= num9; num12--)
				{
					num7 = num12 - _min.x + (n - _min.y) * this.size.x + (m - _min.z) * this.size.x * this.size.y;
					if (num7 >= 0 && num7 < array.Length)
					{
						array[num7] = EnumInsideOutside.Outside;
					}
				}
			}
		}
		for (int num13 = _min.x; num13 <= _max.x; num13++)
		{
			for (int num14 = _min.y; num14 <= _max.y; num14++)
			{
				int num15 = _min.z;
				Ray ray = new Ray(Vector3.zero, Vector3.forward);
				bool flag3 = false;
				float num16 = 0f;
				while (!flag3 && num16 <= 1f)
				{
					float num17 = 0f;
					while (!flag3 && num17 <= 1f)
					{
						ray.origin = new Vector3((float)num13 + num16, (float)num14 + num17, (float)(_min.z - 3));
						RaycastHit raycastHit;
						if (this.doRaycast(ray, out raycastHit, _min))
						{
							num15 = Utils.FastMax(num15, Utils.Fastfloor(raycastHit.point.z + ray.direction.z * 0.1f));
						}
						else
						{
							num15 = _max.z;
							flag3 = true;
						}
						num17 += 0.25f;
					}
					num16 += 0.25f;
				}
				int num18 = num13 - _min.x + (num14 - _min.y) * this.size.x + (num15 - _min.z) * this.size.x * this.size.y;
				if (num18 >= 0 && num18 < array.Length)
				{
					while (num18 < blocks.Length - 1)
					{
						air.rawData = blocks[num18];
						if (!air.isair)
						{
							break;
						}
						num18 += this.size.x * this.size.y;
						num15++;
					}
					if (num18 < array.Length)
					{
						air.rawData = blocks[num18];
						if (air.ischild)
						{
							int type4 = air.type;
							while (num18 > 0)
							{
								air.rawData = blocks[num18];
								if (air.type != type4)
								{
									break;
								}
								num18 += this.size.x * this.size.y;
								num15++;
							}
						}
					}
				}
				UnityEngine.Debug.DrawLine(ray.origin, new Vector3(ray.origin.x, ray.origin.y, (float)num15), Color.blue, 10f);
				for (int num19 = _min.z; num19 <= num15; num19++)
				{
					num18 = num13 - _min.x + (num14 - _min.y) * this.size.x + (num19 - _min.z) * this.size.x * this.size.y;
					if (num18 >= 0 && num18 < array.Length)
					{
						array[num18] = EnumInsideOutside.Outside;
					}
				}
				int num20 = _max.z;
				ray = new Ray(Vector3.zero, Vector3.back);
				flag3 = false;
				float num21 = 0f;
				while (!flag3 && num21 <= 1f)
				{
					float num22 = 0f;
					while (!flag3 && num22 <= 1f)
					{
						ray.origin = new Vector3((float)num13 + num21, (float)num14 + num22, (float)(_max.z + 3));
						RaycastHit raycastHit;
						if (this.doRaycast(ray, out raycastHit, _min))
						{
							num20 = Utils.FastMin(num20, Utils.Fastfloor(raycastHit.point.z + ray.direction.z * 0.1f));
						}
						else
						{
							num20 = _min.z;
							flag3 = true;
						}
						num22 += 0.25f;
					}
					num21 += 0.25f;
				}
				num18 = num13 - _min.x + (num14 - _min.y) * this.size.x + (num20 - _min.z) * this.size.x * this.size.y;
				if (num18 >= 0 && num18 < array.Length)
				{
					while (num18 > 0)
					{
						air.rawData = blocks[num18];
						if (!air.isair)
						{
							break;
						}
						num18 -= this.size.x * this.size.y;
						num15--;
					}
					if (num18 > 0)
					{
						air.rawData = blocks[num18];
						if (air.ischild)
						{
							int type5 = air.type;
							while (num18 > 0)
							{
								air.rawData = blocks[num18];
								if (air.type != type5)
								{
									break;
								}
								num18 -= this.size.x * this.size.y;
								num15--;
							}
						}
					}
				}
				for (int num23 = _max.z; num23 >= num20; num23--)
				{
					num18 = num13 - _min.x + (num14 - _min.y) * this.size.x + (num23 - _min.z) * this.size.x * this.size.y;
					if (num18 >= 0 && num18 < array.Length)
					{
						array[num18] = EnumInsideOutside.Outside;
					}
				}
			}
		}
		return array;
	}

	public void RecalcInsideDevices(EnumInsideOutside[] eInsideOutside)
	{
		this.insidePos.Init(this.size);
		if (!this.IsCullThisPrefab())
		{
			return;
		}
		int blockCount = this.GetBlockCount();
		for (int i = 0; i < blockCount; i++)
		{
			int x;
			int y;
			int z;
			this.offsetToCoord(i, out x, out y, out z);
			if (!this.GetBlock(x, y, z).Block.shape.IsTerrain() && eInsideOutside[i] == EnumInsideOutside.Inside)
			{
				this.insidePos.Add(i);
			}
		}
	}

	public Vector3i? GetFirstIndexedBlockOffsetOfType(string _indexName)
	{
		List<Vector3i> list;
		if (this.indexedBlockOffsets.TryGetValue(_indexName, out list) && list.Count > 0)
		{
			return new Vector3i?(list[0]);
		}
		return null;
	}

	public IChunk GetChunkSync(int chunkX, int chunkY, int chunkZ)
	{
		return this.GetChunk(chunkX, chunkZ);
	}

	public IChunk GetChunkFromWorldPos(int x, int y, int z)
	{
		return this.GetChunk(x / 16, z / 16);
	}

	public IChunk GetChunkFromWorldPos(Vector3i _blockPos)
	{
		return this.GetChunk(_blockPos.x / 16, _blockPos.z / 16);
	}

	public IEnumerator ToTransform(bool _genBlockModels, bool _genTerrain, bool _genBlockShapes, bool _fillEmptySpace, Transform _parent, string _name, Vector3 _position, int _heightLimit = 0)
	{
		MicroStopwatch ms = new MicroStopwatch(true);
		GameObject _go = new GameObject();
		_go.name = _name;
		_go.transform.SetParent(_parent);
		int ySize = 8;
		if (_heightLimit == 0)
		{
			_heightLimit = this.size.y;
		}
		else if (_heightLimit < 0)
		{
			_heightLimit = -this.yOffset - _heightLimit;
		}
		_heightLimit = Mathf.Clamp(_heightLimit, 0, this.size.y);
		int y = 0;
		int y2 = 0;
		while (y < _heightLimit + 1)
		{
			int x = 0;
			int x2 = 0;
			int num4;
			while (x < this.size.x + 1)
			{
				int z = 0;
				int z2 = 0;
				while (z < this.size.z + 1 && !(_go == null))
				{
					GameObject gameObject = new GameObject();
					gameObject.transform.parent = _go.transform;
					gameObject.name = string.Format("Chunk[{0},{1}]", x2, z2);
					MeshFilter[][] array = new MeshFilter[MeshDescription.meshes.Length][];
					MeshRenderer[][] array2 = new MeshRenderer[MeshDescription.meshes.Length][];
					MeshCollider[][] array3 = new MeshCollider[MeshDescription.meshes.Length][];
					GameObject[] array4 = new GameObject[MeshDescription.meshes.Length];
					GameObject gameObject2 = new GameObject("_BlockEntities");
					GameObject gameObject3 = new GameObject("Meshes");
					gameObject2.transform.parent = gameObject.transform;
					gameObject3.transform.parent = gameObject.transform;
					for (int i = 0; i < MeshDescription.meshes.Length; i++)
					{
						array4[i] = new GameObject(MeshDescription.meshes[i].Name);
						array4[i].transform.parent = gameObject3.transform;
						VoxelMesh.CreateMeshFilter(i, 0, array4[i], MeshDescription.meshes[i].Tag, false, out array[i], out array2[i], out array3[i]);
					}
					VoxelMesh[] array5 = new VoxelMesh[6];
					for (int j = 0; j < array5.Length; j++)
					{
						if (j == 5)
						{
							array5[j] = new VoxelMeshTerrain(j, 500)
							{
								IsPreviewVoxelMesh = true
							};
						}
						else
						{
							array5[j] = new VoxelMesh(j, 1024, VoxelMesh.CreateFlags.Default);
						}
					}
					MeshGeneratorPrefab meshGeneratorPrefab = new MeshGeneratorPrefab(this);
					Vector3i worldStartPos = new Vector3i(x, y, z);
					Vector3i worldEndPos = new Vector3i(x + 15, y + ySize, z + 16);
					if (_genTerrain && _genBlockShapes)
					{
						meshGeneratorPrefab.GenerateMeshOffset(worldStartPos, worldEndPos, array5);
					}
					else if (!_genTerrain && _genBlockShapes)
					{
						meshGeneratorPrefab.GenerateMeshNoTerrain(worldStartPos, worldEndPos, array5);
					}
					else if (_genTerrain && !_genBlockShapes)
					{
						meshGeneratorPrefab.GenerateMeshTerrainOnly(worldStartPos, worldEndPos, array5);
					}
					for (int k = 0; k < array5.Length; k++)
					{
						array5[k].CopyToMesh(array[k], array2[k], 0);
					}
					if (_genBlockModels)
					{
						int num = y;
						while (num < y + ySize && num < this.size.y)
						{
							int num2 = x;
							while (num2 < x + 16 && num2 < this.size.x)
							{
								int num3 = z;
								while (num3 < z + 16 && num3 < this.size.z)
								{
									Vector3i vector3i = new Vector3i(num2, num, num3);
									BlockValue block = this.GetBlock(num2, num, num3);
									if (!block.ischild)
									{
										Block block2 = block.Block;
										BlockShapeModelEntity blockShapeModelEntity = block2.shape as BlockShapeModelEntity;
										if (blockShapeModelEntity != null)
										{
											Quaternion rotation = blockShapeModelEntity.GetRotation(block);
											Vector3 rotatedOffset = blockShapeModelEntity.GetRotatedOffset(block2, rotation);
											rotatedOffset.x += 0.5f;
											rotatedOffset.z += 0.5f;
											rotatedOffset.y += 0f;
											Vector3 localPosition = vector3i.ToVector3() + rotatedOffset;
											GameObject objectForType = GameObjectPool.Instance.GetObjectForType(blockShapeModelEntity.modelName);
											if (!(objectForType == null))
											{
												Transform transform = objectForType.transform;
												transform.parent = gameObject2.transform;
												transform.localScale = Vector3.one;
												transform.localPosition = localPosition;
												transform.localRotation = rotation;
											}
										}
									}
									num3++;
								}
								num2++;
							}
							num++;
						}
					}
					yield return null;
					z += 16;
					num4 = z2;
					z2 = num4 + 1;
				}
				x += 16;
				num4 = x2;
				x2 = num4 + 1;
			}
			y += ySize;
			num4 = y2;
			y2 = num4 + 1;
		}
		if (_go != null)
		{
			_go.transform.localPosition = new Vector3(_position.x * _go.transform.localScale.x, _position.y * _go.transform.localScale.y, _position.z * _go.transform.localScale.z);
		}
		Log.Out(string.Format("Prefab preview generation took {0} seconds.", (float)ms.ElapsedMilliseconds / 1000f));
		yield break;
	}

	public void HandleAddingTriggerLayers(BlockTrigger trigger)
	{
		for (int i = 0; i < trigger.TriggersIndices.Count; i++)
		{
			if (!this.TriggerLayers.Contains(trigger.TriggersIndices[i]))
			{
				this.TriggerLayers.Add(trigger.TriggersIndices[i]);
			}
		}
		for (int j = 0; j < trigger.TriggeredByIndices.Count; j++)
		{
			if (!this.TriggerLayers.Contains(trigger.TriggeredByIndices[j]))
			{
				this.TriggerLayers.Add(trigger.TriggeredByIndices[j]);
			}
		}
	}

	public void HandleAddingTriggerLayers(Prefab.PrefabTriggerVolume trigger)
	{
		for (int i = 0; i < trigger.TriggersIndices.Count; i++)
		{
			if (!this.TriggerLayers.Contains(trigger.TriggersIndices[i]))
			{
				this.TriggerLayers.Add(trigger.TriggersIndices[i]);
			}
		}
	}

	public void AddInitialTriggerLayers()
	{
		for (byte b = 1; b < 6; b += 1)
		{
			this.TriggerLayers.Add(b);
		}
	}

	public void AddNewTriggerLayer()
	{
		this.TriggerLayers = (from i in this.TriggerLayers
		orderby i
		select i).ToList<byte>();
		if (this.TriggerLayers.Count > 0)
		{
			int num = (int)(this.TriggerLayers[this.TriggerLayers.Count - 1] + 1);
			if (num < 255 && num > 0)
			{
				this.TriggerLayers.Add((byte)num);
				return;
			}
		}
		else
		{
			this.TriggerLayers.Add(1);
		}
	}

	[Conditional("DEBUG_PREFABLOG")]
	[PublicizedFrom(EAccessModifier.Private)]
	public static void LogPrefab(string format, params object[] args)
	{
		format = string.Format("{0} Prefab {1}", GameManager.frameCount, format);
		Log.Warning(format, args);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static int CurrentSaveVersion = 18;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cMinimumSupportedVersion = 13;

	[PublicizedFrom(EAccessModifier.Private)]
	public const string cProp_CopyAirBlocks = "CopyAirBlocks";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string cProp_AllowTopSoilDecorations = "AllowTopSoilDecorations";

	public const string cProp_YOffset = "YOffset";

	public const string cProp_RotationToFaceNorth = "RotationToFaceNorth";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string cProp_ExcludeDistantPOIMesh = "ExcludeDistantPOIMesh";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string cProp_ExcludePOICulling = "ExcludePOICulling";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string cProp_DistantPOIYOffset = "DistantPOIYOffset";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string cProp_DistantPOIOverride = "DistantPOIOverride";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string cProp_EditorGroups = "EditorGroups";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string cProp_IsTraderArea = "TraderArea";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string cProp_TraderAreaProtect = "TraderAreaProtect";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string cProp_SleeperVolumeStart = "SleeperVolumeStart";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string cProp_SleeperVolumeSize = "SleeperVolumeSize";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string cProp_SleeperVolumeGroup = "SleeperVolumeGroup";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string cProp_SleeperVolumeGroupId = "SleeperVolumeGroupId";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string cProp_SleeperIsPriorityVolume = "SleeperIsLootVolume";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string cProp_SleeperIsQuestExclude = "SleeperIsQuestExclude";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string cProp_SleeperVolumeFlags = "SleeperVolumeFlags";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string cProp_SleeperVolumeTriggeredBy = "SleeperVolumeTriggeredBy";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string cProp_SleeperVolumeScript = "SVS";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string cProp_TeleportVolumeStart = "TeleportVolumeStart";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string cProp_TeleportVolumeSize = "TeleportVolumeSize";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string cProp_InfoVolumeStart = "InfoVolumeStart";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string cProp_InfoVolumeSize = "InfoVolumeSize";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string cProp_WallVolumeStart = "WallVolumeStart";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string cProp_WallVolumeSize = "WallVolumeSize";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string cProp_TriggerVolumeStart = "TriggerVolumeStart";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string cProp_TriggerVolumeSize = "TriggerVolumeSize";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string cProp_TriggerVolumeTriggers = "TriggerVolumeTriggers";

	public const string cProp_POIMarkerStart = "POIMarkerStart";

	public const string cProp_POIMarkerSize = "POIMarkerSize";

	public const string cProp_POIMarkerGroup = "POIMarkerGroup";

	public const string cProp_POIMarkerTags = "POIMarkerTags";

	public const string cProp_POIMarkerType = "POIMarkerType";

	public const string cProp_POIMarkerPartToSpawn = "POIMarkerPartToSpawn";

	public const string cProp_POIMarkerPartRotations = "POIMarkerPartRotations";

	public const string cProp_POIMarkerPartSpawnChance = "POIMarkerPartSpawnChance";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string cProp_Zoning = "Zoning";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string cProp_AllowedBiomes = "AllowedBiomes";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string cProp_AllowedTownships = "AllowedTownships";

	public const string cProp_Tags = "Tags";

	public const string cProp_ThemeTags = "ThemeTags";

	public const string cProp_ThemeRepeatDist = "ThemeRepeatDistance";

	public const string cProp_DuplicateRepeatDist = "DuplicateRepeatDistance";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string cProp_StaticSpawnerClass = "StaticSpawner.Class";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string cProp_StaticSpawnerSize = "StaticSpawner.Size";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string cProp_StaticSpawnerTrigger = "StaticSpawner.Trigger";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string cProp_IndexedBlockOffsets = "IndexedBlockOffsets";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string cProp_QuestTags = "QuestTags";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string cProp_ShowQuestClearCount = "ShowQuestClearCount";

	public const string cProp_DifficultyTier = "DifficultyTier";

	public const string cProp_PrefabSize = "PrefabSize";

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly string MISSING_BLOCK_NAME = "missingBlock";

	public Vector3i size;

	public PathAbstractions.AbstractedLocation location;

	public bool bCopyAirBlocks = true;

	public bool bExcludeDistantPOIMesh;

	public bool bExcludePOICulling;

	public float distantPOIYOffset;

	public string distantPOIOverride;

	public bool bAllowTopSoilDecorations;

	public bool bTraderArea;

	public Vector3i TraderAreaProtect;

	public List<Prefab.PrefabSleeperVolume> SleeperVolumes = new List<Prefab.PrefabSleeperVolume>();

	public List<Prefab.PrefabTeleportVolume> TeleportVolumes = new List<Prefab.PrefabTeleportVolume>();

	public List<Prefab.PrefabInfoVolume> InfoVolumes = new List<Prefab.PrefabInfoVolume>();

	public List<Prefab.PrefabWallVolume> WallVolumes = new List<Prefab.PrefabWallVolume>();

	public List<Prefab.PrefabTriggerVolume> TriggerVolumes = new List<Prefab.PrefabTriggerVolume>();

	public int yOffset;

	public int Transient_NumSleeperSpawns;

	public List<Prefab.Marker> POIMarkers = new List<Prefab.Marker>();

	public List<string> editorGroups = new List<string>();

	public int rotationToFaceNorth = 2;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<string> allowedZones = new List<string>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<string> allowedBiomes = new List<string>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<string> allowedTownships = new List<string>();

	[PublicizedFrom(EAccessModifier.Private)]
	public FastTags<TagGroup.Poi> tags;

	[PublicizedFrom(EAccessModifier.Private)]
	public FastTags<TagGroup.Poi> themeTags;

	[PublicizedFrom(EAccessModifier.Private)]
	public int themeRepeatDistance = 300;

	[PublicizedFrom(EAccessModifier.Private)]
	public int duplicateRepeatDistance = 1000;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<int> multiBlockParentIndices = new List<int>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<int> decoAllowedBlockIndices = new List<int>();

	public readonly Dictionary<string, List<Vector3i>> indexedBlockOffsets = new CaseInsensitiveStringDictionary<List<Vector3i>>();

	[PublicizedFrom(EAccessModifier.Private)]
	public FastTags<TagGroup.Global> questTags = FastTags<TagGroup.Global>.none;

	[PublicizedFrom(EAccessModifier.Private)]
	public Prefab.BlockStatistics statistics;

	[PublicizedFrom(EAccessModifier.Private)]
	public WorldStats renderingCost;

	public string StaticSpawnerClass;

	public Vector3i StaticSpawnerSize;

	public int StaticSpawnerTrigger;

	public bool StaticSpawnerCreated;

	public int ShowQuestClearCount = 1;

	public byte DifficultyTier;

	[PublicizedFrom(EAccessModifier.Private)]
	public int localRotation;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly bool isCellsDataOwner = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public Prefab.Cells<uint> blockCells;

	[PublicizedFrom(EAccessModifier.Private)]
	public Prefab.Cells<ushort> damageCells;

	[PublicizedFrom(EAccessModifier.Private)]
	public Prefab.Cells<sbyte> densityCells;

	[PublicizedFrom(EAccessModifier.Private)]
	public Prefab.Cells<long> textureCells;

	[PublicizedFrom(EAccessModifier.Private)]
	public Prefab.Cells<WaterValue> waterCells;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Prefab.Data sharedData = default(Prefab.Data);

	[PublicizedFrom(EAccessModifier.Private)]
	public List<EntityCreationData> entities = new List<EntityCreationData>();

	public DynamicProperties properties = new DynamicProperties();

	[PublicizedFrom(EAccessModifier.Private)]
	public int terrainFillerType;

	[PublicizedFrom(EAccessModifier.Private)]
	public int terrainFiller2Type;

	[PublicizedFrom(EAccessModifier.Private)]
	public int blockTypeMissingBlock = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<Vector3i, TileEntity> tileEntities = new Dictionary<Vector3i, TileEntity>();

	[PublicizedFrom(EAccessModifier.Private)]
	public PrefabInsideDataFile insidePos = new PrefabInsideDataFile();

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<Vector3i, BlockTrigger> triggerData = new Dictionary<Vector3i, BlockTrigger>();

	public List<byte> TriggerLayers = new List<byte>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static byte[] tempBuf;

	[PublicizedFrom(EAccessModifier.Private)]
	public static SimpleBitStream simpleBitStreamReader = new SimpleBitStream(1000);

	[PublicizedFrom(EAccessModifier.Private)]
	public int currX;

	[PublicizedFrom(EAccessModifier.Private)]
	public int currZ;

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<long, Prefab.PrefabChunk> dictChunks;

	public struct BlockStatistics
	{
		public void Clear()
		{
			this.cntWindows = 0;
			this.cntDoors = 0;
			this.cntBlockEntities = 0;
			this.cntSolid = 0;
			this.cntBlockModels = 0;
		}

		public override string ToString()
		{
			return string.Format("Blocks: {0} BEnts: {1} BMods: {2} Wdws: {3}", new object[]
			{
				this.cntSolid,
				this.cntBlockEntities,
				this.cntBlockModels,
				this.cntWindows
			});
		}

		public int cntWindows;

		public int cntDoors;

		public int cntBlockEntities;

		public int cntBlockModels;

		public int cntSolid;
	}

	public class PrefabSleeperVolume
	{
		public PrefabSleeperVolume()
		{
		}

		public PrefabSleeperVolume(Prefab.PrefabSleeperVolume _other)
		{
			this.used = _other.used;
			this.startPos = _other.startPos;
			this.size = _other.size;
			this.groupId = _other.groupId;
			this.groupName = _other.groupName;
			this.isPriority = _other.isPriority;
			this.isQuestExclude = _other.isQuestExclude;
			this.spawnCountMin = _other.spawnCountMin;
			this.spawnCountMax = _other.spawnCountMax;
			this.triggeredByIndices = _other.triggeredByIndices;
			this.flags = _other.flags;
			this.minScript = _other.minScript;
		}

		public void Use(Vector3i _startPos, Vector3i _size, short _groupId, string _groupName, bool _isPriority, bool _isQuestExclude, int _spawnMin, int _spawnMax, int _flags)
		{
			this.used = true;
			this.startPos = _startPos;
			this.size = _size;
			this.groupId = _groupId;
			this.groupName = _groupName;
			this.isPriority = _isPriority;
			this.isQuestExclude = _isQuestExclude;
			this.spawnCountMin = (short)_spawnMin;
			this.spawnCountMax = (short)_spawnMax;
			this.flags = _flags;
		}

		public void SetTrigger(SleeperVolume.ETriggerType type)
		{
			this.flags = ((this.flags & -8) | (int)type);
		}

		public void SetTriggeredByFlag(byte index)
		{
			if (!this.triggeredByIndices.Contains(index))
			{
				this.triggeredByIndices.Add(index);
			}
		}

		public void ClearTriggeredBy()
		{
			this.triggeredByIndices.Clear();
		}

		public void RemoveTriggeredByFlag(byte index)
		{
			this.triggeredByIndices.Remove(index);
		}

		public bool HasTriggeredBy(byte index)
		{
			return this.triggeredByIndices.Contains(index);
		}

		public bool HasAnyTriggeredBy()
		{
			return this.triggeredByIndices.Count > 0;
		}

		public bool used;

		public Vector3i startPos;

		public Vector3i size;

		public string groupName;

		public bool isPriority;

		public bool isQuestExclude;

		public short spawnCountMin;

		public short spawnCountMax;

		public short groupId;

		public int flags;

		public string minScript;

		public List<byte> triggeredByIndices = new List<byte>();
	}

	public class PrefabTeleportVolume
	{
		public PrefabTeleportVolume()
		{
		}

		public PrefabTeleportVolume(Prefab.PrefabTeleportVolume _other)
		{
			this.startPos = _other.startPos;
			this.size = _other.size;
		}

		public void Use(Vector3i _startPos, Vector3i _size)
		{
			this.used = true;
			this.startPos = _startPos;
			this.size = _size;
		}

		public Vector3i startPos;

		public Vector3i size;

		public bool used;
	}

	public class PrefabInfoVolume
	{
		public PrefabInfoVolume()
		{
		}

		public PrefabInfoVolume(Prefab.PrefabInfoVolume _other)
		{
			this.startPos = _other.startPos;
			this.size = _other.size;
		}

		public void Use(Vector3i _startPos, Vector3i _size)
		{
			this.used = true;
			this.startPos = _startPos;
			this.size = _size;
		}

		public Vector3i startPos;

		public Vector3i size;

		public bool used;
	}

	public class PrefabWallVolume
	{
		public PrefabWallVolume()
		{
		}

		public PrefabWallVolume(Prefab.PrefabWallVolume _other)
		{
			this.startPos = _other.startPos;
			this.size = _other.size;
		}

		public void Use(Vector3i _startPos, Vector3i _size)
		{
			this.startPos = _startPos;
			this.size = _size;
		}

		public Vector3i startPos;

		public Vector3i size;
	}

	[Preserve]
	public class PrefabTriggerVolume
	{
		public PrefabTriggerVolume()
		{
		}

		public PrefabTriggerVolume(Prefab.PrefabTriggerVolume _other)
		{
			this.startPos = _other.startPos;
			this.size = _other.size;
			this.TriggersIndices = _other.TriggersIndices;
		}

		public void Use(Vector3i _startPos, Vector3i _size)
		{
			this.startPos = _startPos;
			this.size = _size;
			this.used = true;
		}

		public void SetTriggersFlag(byte index)
		{
			if (!this.TriggersIndices.Contains(index))
			{
				this.TriggersIndices.Add(index);
			}
		}

		public void RemoveTriggersFlag(byte index)
		{
			this.TriggersIndices.Remove(index);
		}

		public void RemoveAllTriggersFlags()
		{
			this.TriggersIndices.Clear();
		}

		public bool HasTriggers(byte index)
		{
			return this.TriggersIndices.Contains(index);
		}

		public bool HasAnyTriggers()
		{
			return this.TriggersIndices.Count > 0;
		}

		public Vector3i startPos;

		public Vector3i size;

		public PrefabTriggerData TriggerDataOwner;

		public List<byte> TriggersIndices = new List<byte>();

		public bool used;
	}

	public class Marker
	{
		public string GroupName
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				return this.groupName;
			}
			set
			{
				if (this.groupName != value)
				{
					this.color = default(Color);
					this.groupId = -1;
					this.groupName = value;
				}
			}
		}

		public Color GroupColor
		{
			get
			{
				if (this.color == default(Color))
				{
					GameRandom tempGameRandom = GameRandomManager.Instance.GetTempGameRandom(this.GroupId);
					this.color = new Color32((byte)tempGameRandom.RandomRange(0, 256), (byte)tempGameRandom.RandomRange(0, 256), (byte)tempGameRandom.RandomRange(0, 256), (this.MarkerType == Prefab.Marker.MarkerTypes.PartSpawn) ? 32 : 128);
				}
				return this.color;
			}
		}

		public int GroupId
		{
			get
			{
				if (this.groupId == -1)
				{
					this.groupId = this.GroupName.GetHashCode();
				}
				return this.groupId;
			}
		}

		public Vector3i Start
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				return this.start;
			}
			set
			{
				if (this.start != value)
				{
					this.start = value;
				}
			}
		}

		public Vector3i Size
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				return this.size;
			}
			set
			{
				if (this.size != value)
				{
					this.size = value;
					this.PartDirty = true;
				}
			}
		}

		public Prefab.Marker.MarkerTypes MarkerType
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				return this.markerType;
			}
			set
			{
				if (this.markerType != value)
				{
					this.markerType = value;
					this.PartDirty = true;
				}
			}
		}

		public string Name
		{
			get
			{
				return this.name;
			}
			set
			{
				if (this.name != value)
				{
					this.name = value;
				}
			}
		}

		public FastTags<TagGroup.Poi> Tags
		{
			get
			{
				return this.tags;
			}
			set
			{
				if (!this.tags.Equals(value))
				{
					this.tags = value;
				}
			}
		}

		public string PartToSpawn
		{
			get
			{
				return this.partToSpawn;
			}
			set
			{
				if (this.partToSpawn != value)
				{
					this.partToSpawn = value;
					this.PartDirty = true;
				}
			}
		}

		public byte Rotations
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				return this.rotations;
			}
			set
			{
				if (this.rotations != value)
				{
					this.rotations = value;
					this.PartDirty = true;
				}
			}
		}

		public float PartChanceToSpawn
		{
			get
			{
				return this.partChanceToSpawn;
			}
			set
			{
				if ((float)this.rotations != value)
				{
					this.partChanceToSpawn = value;
					this.PartDirty = true;
				}
			}
		}

		public Marker()
		{
		}

		public Marker(Vector3i _start, Vector3i _size, Prefab.Marker.MarkerTypes _type, string _group, FastTags<TagGroup.Poi> _tags)
		{
			this.Start = _start;
			this.Size = _size;
			this.MarkerType = _type;
			this.GroupName = _group;
			this.Tags = _tags;
		}

		public Marker(Prefab.Marker _other)
		{
			this.Start = _other.Start;
			this.Size = _other.Size;
			this.MarkerType = _other.MarkerType;
			this.GroupName = _other.GroupName;
			this.Tags = _other.Tags;
			this.Name = _other.Name;
			this.PartToSpawn = _other.PartToSpawn;
			this.Rotations = _other.Rotations;
			this.PartChanceToSpawn = _other.PartChanceToSpawn;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public Vector3i start;

		[PublicizedFrom(EAccessModifier.Private)]
		public Vector3i size;

		[PublicizedFrom(EAccessModifier.Private)]
		public Prefab.Marker.MarkerTypes markerType;

		[PublicizedFrom(EAccessModifier.Private)]
		public string name;

		[PublicizedFrom(EAccessModifier.Private)]
		public FastTags<TagGroup.Poi> tags;

		[PublicizedFrom(EAccessModifier.Private)]
		public string partToSpawn;

		[PublicizedFrom(EAccessModifier.Private)]
		public byte rotations;

		[PublicizedFrom(EAccessModifier.Private)]
		public float partChanceToSpawn = 1f;

		public bool PartDirty = true;

		[PublicizedFrom(EAccessModifier.Private)]
		public int groupId = -1;

		[PublicizedFrom(EAccessModifier.Private)]
		public string groupName;

		[PublicizedFrom(EAccessModifier.Private)]
		public Color color;

		public static List<Vector3i> MarkerSizes = new List<Vector3i>
		{
			Vector3i.one,
			new Vector3i(25, 0, 25),
			new Vector3i(42, 0, 42),
			new Vector3i(60, 0, 60),
			new Vector3i(100, 0, 100)
		};

		public enum MarkerTypes : byte
		{
			None,
			POISpawn,
			RoadExit,
			PartSpawn
		}

		public enum MarkerSize : byte
		{
			One,
			ExtraSmall,
			Small,
			Medium,
			Large,
			Custom
		}
	}

	public struct Data
	{
		public void Init(int _count)
		{
			this.m_Blocks = new uint[_count];
			this.m_Damage = new ushort[_count];
			this.m_Density = new byte[_count];
			this.m_Textures = new long[_count];
			this.m_Water = new WaterValue[_count];
		}

		public void Expand(int _count)
		{
			int num = (this.m_Blocks != null) ? this.m_Blocks.Length : 0;
			if (_count > num)
			{
				this.m_Blocks = new uint[_count];
				this.m_Damage = new ushort[_count];
				this.m_Density = new byte[_count];
				this.m_Textures = new long[_count];
				this.m_Water = new WaterValue[_count];
			}
			for (int i = 0; i < _count; i++)
			{
				this.m_Textures[i] = 0L;
				this.m_Water[i] = WaterValue.Empty;
			}
		}

		public uint[] m_Blocks;

		public ushort[] m_Damage;

		public byte[] m_Density;

		public long[] m_Textures;

		public WaterValue[] m_Water;
	}

	public class Cells<[IsUnmanaged] T> where T : struct, ValueType
	{
		public Cells(int _sizeY, T _defaultValue)
		{
			this.sizeY = _sizeY;
			this.defaultValue = _defaultValue;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public Cells(Prefab.Cells<T> _template)
		{
			this.sizeY = _template.sizeY;
			this.defaultValue = _template.defaultValue;
		}

		public Prefab.Cells<T>.Cell AllocCell(int _x, int _y, int _z)
		{
			if (this.a == null)
			{
				this.a = new Prefab.Cells<T>.CellsAtZ[this.sizeY];
			}
			Prefab.Cells<T>.CellsAtZ cellsAtZ = this.a[_y];
			if (cellsAtZ == null)
			{
				cellsAtZ = new Prefab.Cells<T>.CellsAtZ();
				this.a[_y] = cellsAtZ;
			}
			int num = _z >> 2;
			if (cellsAtZ.a == null || num >= cellsAtZ.a.Length)
			{
				Array.Resize<Prefab.Cells<T>.CellsAtX>(ref cellsAtZ.a, num + 1);
			}
			Prefab.Cells<T>.CellsAtX cellsAtX = cellsAtZ.a[num];
			if (cellsAtX == null)
			{
				cellsAtX = new Prefab.Cells<T>.CellsAtX();
				cellsAtZ.a[num] = cellsAtX;
			}
			int num2 = _x >> 2;
			if (cellsAtX.a == null || num2 >= cellsAtX.a.Length)
			{
				Array.Resize<Prefab.Cells<T>.Cell>(ref cellsAtX.a, num2 + 1);
			}
			Prefab.Cells<T>.Cell cell = cellsAtX.a[num2];
			if (cell.a == null)
			{
				cell = new Prefab.Cells<T>.Cell(this.defaultValue);
				cellsAtX.a[num2] = cell;
			}
			return cell;
		}

		public Prefab.Cells<T>.Cell GetCell(int _x, int _y, int _z)
		{
			if (this.a == null)
			{
				return Prefab.Cells<T>.Cell.empty;
			}
			Prefab.Cells<T>.CellsAtZ cellsAtZ = this.a[_y];
			if (cellsAtZ == null)
			{
				return Prefab.Cells<T>.Cell.empty;
			}
			int num = _z >> 2;
			if (cellsAtZ.a == null || num >= cellsAtZ.a.Length)
			{
				return Prefab.Cells<T>.Cell.empty;
			}
			Prefab.Cells<T>.CellsAtX cellsAtX = cellsAtZ.a[num];
			if (cellsAtX == null)
			{
				return Prefab.Cells<T>.Cell.empty;
			}
			int num2 = _x >> 2;
			if (cellsAtX.a == null || num2 >= cellsAtX.a.Length)
			{
				return Prefab.Cells<T>.Cell.empty;
			}
			return cellsAtX.a[num2];
		}

		public T GetData(int _x, int _y, int _z)
		{
			Prefab.Cells<T>.Cell cell = this.GetCell(_x, _y, _z);
			if (cell.a == null)
			{
				return this.defaultValue;
			}
			return cell.Get(_x, _z);
		}

		public void SetData(int _x, int _y, int _z, T _data)
		{
			this.AllocCell(_x, _y, _z).Set(_x, _z, _data);
		}

		public Prefab.Cells<T> Clone()
		{
			Prefab.Cells<T> cells = new Prefab.Cells<T>(this.sizeY, this.defaultValue);
			if (this.a == null)
			{
				return cells;
			}
			cells.a = new Prefab.Cells<T>.CellsAtZ[this.sizeY];
			for (int i = 0; i < this.sizeY; i++)
			{
				Prefab.Cells<T>.CellsAtZ cellsAtZ = this.a[i];
				if (cellsAtZ != null)
				{
					Prefab.Cells<T>.CellsAtZ cellsAtZ2 = new Prefab.Cells<T>.CellsAtZ();
					cellsAtZ2.a = new Prefab.Cells<T>.CellsAtX[cellsAtZ.a.Length];
					cells.a[i] = cellsAtZ2;
					for (int j = 0; j < cellsAtZ.a.Length; j++)
					{
						Prefab.Cells<T>.CellsAtX cellsAtX = cellsAtZ.a[j];
						if (cellsAtX != null)
						{
							Prefab.Cells<T>.CellsAtX cellsAtX2 = new Prefab.Cells<T>.CellsAtX();
							cellsAtX2.a = new Prefab.Cells<T>.Cell[cellsAtX.a.Length];
							cellsAtZ2.a[j] = cellsAtX2;
							for (int k = 0; k < cellsAtX.a.Length; k++)
							{
								Prefab.Cells<T>.Cell cell = cellsAtX.a[k];
								if (cell.a != null)
								{
									cellsAtX2.a[k] = cell.Clone();
								}
							}
						}
					}
				}
			}
			return cells;
		}

		public void Stats(out int _arrayCount, out int _arraySize, out int _cellsCount, out int _cellsSize, out int _usedCount)
		{
			_arrayCount = 0;
			_arraySize = 0;
			_cellsCount = 0;
			_cellsSize = 0;
			_usedCount = 0;
			int num = (this.a != null) ? this.a.Length : 0;
			_arrayCount += num;
			_arraySize += num * 8 + 8;
			for (int i = 0; i < num; i++)
			{
				Prefab.Cells<T>.CellsAtZ cellsAtZ = this.a[i];
				if (cellsAtZ != null)
				{
					_arrayCount += cellsAtZ.a.Length;
					_arraySize += cellsAtZ.a.Length * 8 + 8;
					for (int j = 0; j < cellsAtZ.a.Length; j++)
					{
						Prefab.Cells<T>.CellsAtX cellsAtX = cellsAtZ.a[j];
						if (cellsAtX != null)
						{
							_arrayCount += cellsAtX.a.Length;
							_arraySize += cellsAtX.a.Length * 8 + 8;
							for (int k = 0; k < cellsAtX.a.Length; k++)
							{
								Prefab.Cells<T>.Cell cell = cellsAtX.a[k];
								if (cell.a != null)
								{
									_cellsCount += 16;
									_cellsSize += cell.Size();
									_usedCount += cell.UsedCount(this.defaultValue);
								}
							}
						}
					}
				}
			}
		}

		public void Load(PooledBinaryReader _br)
		{
			Array.Clear(this.a, 0, this.a.Length);
			if (_br.ReadByte() == 1 && _br.ReadUInt16() > 0)
			{
				this.LoadData(_br);
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void LoadData(PooledBinaryReader _br)
		{
			for (;;)
			{
				_br.Read(Prefab.Cells<T>.cellBytes, 0, 3);
				int num = (int)Prefab.Cells<T>.cellBytes[0];
				if (num == 255)
				{
					break;
				}
				int x = (int)Prefab.Cells<T>.cellBytes[1] << 2;
				int z = (int)Prefab.Cells<T>.cellBytes[2] << 2;
				this.AllocCell(x, num, z).Load(_br);
			}
		}

		public void Save(BinaryWriter _bw)
		{
			ushort num = (ushort)((this.a != null) ? this.a.Length : 0);
			if (num == 0)
			{
				_bw.Write(0);
				return;
			}
			_bw.Write(1);
			_bw.Write(num);
			for (int i = 0; i < (int)num; i++)
			{
				Prefab.Cells<T>.CellsAtZ cellsAtZ = this.a[i];
				if (cellsAtZ != null)
				{
					for (int j = 0; j < cellsAtZ.a.Length; j++)
					{
						Prefab.Cells<T>.CellsAtX cellsAtX = cellsAtZ.a[j];
						if (cellsAtX != null)
						{
							for (int k = 0; k < cellsAtX.a.Length; k++)
							{
								Prefab.Cells<T>.Cell cell = cellsAtX.a[k];
								if (cell.a != null)
								{
									Prefab.Cells<T>.cellBytes[0] = (byte)i;
									Prefab.Cells<T>.cellBytes[1] = (byte)k;
									Prefab.Cells<T>.cellBytes[2] = (byte)j;
									_bw.Write(Prefab.Cells<T>.cellBytes, 0, 3);
									cell.Save(_bw);
								}
							}
						}
					}
				}
			}
			Prefab.Cells<T>.cellBytes[0] = byte.MaxValue;
			_bw.Write(Prefab.Cells<T>.cellBytes, 0, 3);
		}

		public T[] ToArray(Prefab prefab, Vector3i _size)
		{
			T[] array = new T[_size.x * _size.y * _size.z];
			int num = (this.a != null) ? this.a.Length : 0;
			for (int i = 0; i < num; i++)
			{
				Prefab.Cells<T>.CellsAtZ cellsAtZ = this.a[i];
				if (cellsAtZ != null)
				{
					for (int j = 0; j < cellsAtZ.a.Length; j++)
					{
						Prefab.Cells<T>.CellsAtX cellsAtX = cellsAtZ.a[j];
						if (cellsAtX != null)
						{
							for (int k = 0; k < cellsAtX.a.Length; k++)
							{
								Prefab.Cells<T>.Cell cell = cellsAtX.a[k];
								if (cell.a != null)
								{
									int num2 = k << 2;
									int num3 = j << 2;
									int num4 = Utils.FastMin(_size.x - num2, 4);
									int num5 = Utils.FastMin(_size.z - num3, 4);
									for (int l = 0; l < num5; l++)
									{
										for (int m = 0; m < num4; m++)
										{
											T t = cell.Get(m, l);
											int num6 = prefab.CoordToOffset(0, num2 + m, i, num3 + l);
											array[num6] = t;
										}
									}
								}
							}
						}
					}
				}
			}
			return array;
		}

		public unsafe void CompareTest(Vector3i size, PooledBinaryReader _br)
		{
			Prefab.Cells<T> cells = new Prefab.Cells<T>(this);
			cells.Load(_br);
			if (this.a.Length != cells.a.Length)
			{
				Log.Error("Cells size");
			}
			for (int i = 0; i < size.y; i++)
			{
				for (int j = 0; j < size.z; j++)
				{
					for (int k = 0; k < size.x; k++)
					{
						Prefab.Cells<T>.Cell cell = this.GetCell(k, i, j);
						Prefab.Cells<T>.Cell cell2 = cells.GetCell(k, i, j);
						if (cell.a == null)
						{
							if (cell2.a != null)
							{
								Log.Error("Cells one is null {0} {1} {2}", new object[]
								{
									k,
									i,
									j
								});
							}
						}
						else if (cell2.a != null)
						{
							for (int l = 0; l < 4; l++)
							{
								for (int m = 0; m < 4; m++)
								{
									T t = cell.Get(m, l);
									T t2 = cell2.Get(m, l);
									byte* ptr = (byte*)UnsafeUtility.AddressOf<T>(ref t);
									byte* ptr2 = (byte*)UnsafeUtility.AddressOf<T>(ref t2);
									for (int n = 0; n < sizeof(T); n++)
									{
										if (ptr[n] != ptr2[n])
										{
											Log.Error("Cells data {0} {1} {2}, {3} != {4}", new object[]
											{
												k,
												i,
												j,
												ptr[n],
												ptr2[n]
											});
										}
									}
								}
							}
						}
					}
				}
			}
		}

		public T defaultValue;

		[PublicizedFrom(EAccessModifier.Private)]
		public int sizeY;

		public Prefab.Cells<T>.CellsAtZ[] a;

		[PublicizedFrom(EAccessModifier.Private)]
		public static byte[] cellBytes = new byte[256];

		public class CellsAtX
		{
			public Prefab.Cells<T>.Cell[] a;
		}

		public class CellsAtZ
		{
			public Prefab.Cells<T>.CellsAtX[] a;
		}

		public struct Cell
		{
			public Cell(T _defaultValue)
			{
				this.a = new T[16];
				for (int i = 0; i < 16; i++)
				{
					this.a[i] = _defaultValue;
				}
			}

			public Prefab.Cells<T>.Cell Clone()
			{
				Prefab.Cells<T>.Cell cell = default(Prefab.Cells<T>.Cell);
				if (this.a != null)
				{
					cell.a = new T[16];
					for (int i = 0; i < 16; i++)
					{
						cell.a[i] = this.a[i];
					}
				}
				return cell;
			}

			public override string ToString()
			{
				return string.Format("{0}", (this.a != null) ? this.a.Length : -1);
			}

			public int Size()
			{
				return 16 * sizeof(T);
			}

			public unsafe int UsedCount(T _defaultValue)
			{
				int num = 0;
				byte* ptr = (byte*)UnsafeUtility.AddressOf<T>(ref _defaultValue);
				for (int i = 0; i < 16; i++)
				{
					byte* ptr2 = (byte*)UnsafeUtility.AddressOf<T>(ref this.a[i]);
					for (int j = 0; j < sizeof(T); j++)
					{
						if (ptr[j] != ptr2[j])
						{
							num++;
							break;
						}
					}
				}
				return num;
			}

			public void Set(int _x, int _z, T _value)
			{
				int num = (_x & 3) + ((_z & 3) << 2);
				this.a[num] = _value;
			}

			public T Get(int _x, int _z)
			{
				int num = (_x & 3) + ((_z & 3) << 2);
				return this.a[num];
			}

			public unsafe void Load(PooledBinaryReader _br)
			{
				int num = (int)_br.BaseStream.Position;
				int num2 = (int)_br.ReadUInt16();
				_br.Read(Prefab.Cells<T>.cellBytes, 0, num2);
				Log.Warning("Cell Load at {0}, count{1}", new object[]
				{
					num,
					num2
				});
				int num3 = 0;
				byte* ptr = (byte*)UnsafeUtility.AddressOf<T>(ref this.a[0]);
				int i = 0;
				while (i < num2)
				{
					int num4 = (int)((sbyte)Prefab.Cells<T>.cellBytes[i++]);
					if (num4 >= 0)
					{
						for (int j = 0; j < num4; j++)
						{
							byte b = Prefab.Cells<T>.cellBytes[i++];
							ptr[num3++] = b;
						}
					}
					else
					{
						num4 = -num4;
						byte b2 = Prefab.Cells<T>.cellBytes[i++];
						for (int k = 0; k < num4; k++)
						{
							ptr[num3++] = b2;
						}
					}
				}
			}

			public unsafe void Save(BinaryWriter _bw)
			{
				byte* ptr = (byte*)UnsafeUtility.AddressOf<T>(ref this.a[0]);
				int num = 0;
				int num2 = this.a.Length * sizeof(T);
				int i = 0;
				while (i < num2)
				{
					int num3 = 1;
					byte b = ptr[i];
					if (i + 1 < num2)
					{
						byte b2 = ptr[i + 1];
						if (b == b2)
						{
							int num4 = 2;
							int num5 = i + 2;
							while (num5 < num2 && ptr[num5] == b)
							{
								num4++;
								if (num4 >= 128)
								{
									break;
								}
								num5++;
							}
							if (num4 >= 3)
							{
								num3 = -num4;
								i += num4;
							}
							else
							{
								num3 = num4;
							}
						}
						if (num3 >= 0)
						{
							for (int j = i + num3; j < num2; j++)
							{
								b2 = ptr[j];
								if (j + 2 < num2 && b2 == ptr[j + 1] && b2 == ptr[j + 2])
								{
									break;
								}
								num3++;
								if (num3 >= 127)
								{
									break;
								}
							}
						}
					}
					Prefab.Cells<T>.cellBytes[num++] = (byte)num3;
					if (num3 >= 0)
					{
						for (int k = 0; k < num3; k++)
						{
							byte b3 = ptr[i++];
							Prefab.Cells<T>.cellBytes[num++] = b3;
						}
					}
					else
					{
						Prefab.Cells<T>.cellBytes[num++] = b;
					}
				}
				_bw.Write((ushort)num);
				_bw.Write(Prefab.Cells<T>.cellBytes, 0, num);
			}

			public const int cSizeXZ = 4;

			public const int cSizeArray = 16;

			public const int cSizeXZMask = 3;

			public const int cSizeXZShift = 2;

			public static Prefab.Cells<T>.Cell empty;

			public T[] a;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public enum DataFormat
		{
			Empty,
			RLE
		}
	}

	public class PrefabChunk : IChunk
	{
		public PrefabChunk(Prefab _prefab, int _x, int _z)
		{
			this.prefab = _prefab;
			this.X = _x;
			this.Z = _z;
			this.Y = 0;
		}

		public int X { get; set; }

		public int Y { get; set; }

		public int Z { get; set; }

		public Vector3i ChunkPos { get; set; }

		public bool GetAvailable()
		{
			return true;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public bool checkCoordinates(ref int _x, ref int _y, ref int _z)
		{
			_x = this.X * 16 + _x;
			_y = this.Y * 256 + _y;
			_z = this.Z * 16 + _z;
			return _x >= 0 && _x < this.prefab.size.x && _y >= 0 && _y < this.prefab.size.y && _z >= 0 && _z < this.prefab.size.z;
		}

		public BlockValue GetBlock(int _x, int _y, int _z)
		{
			if (!this.checkCoordinates(ref _x, ref _y, ref _z))
			{
				return BlockValue.Air;
			}
			return this.prefab.GetBlock(_x, _y, _z);
		}

		public BlockValue GetBlockNoDamage(int _x, int _y, int _z)
		{
			return this.GetBlock(_x, _y, _z);
		}

		public BlockValue GetBlock(int _bos, int _y)
		{
			return this.GetBlock(ChunkBlockLayerLegacy.OffsetX(_bos), _y, ChunkBlockLayerLegacy.OffsetX(_bos));
		}

		public bool IsAir(int _x, int _y, int _z)
		{
			return this.checkCoordinates(ref _x, ref _y, ref _z) && this.prefab.GetBlock(_x, _y, _z).isair && !this.prefab.GetWater(_x, _y, _z).HasMass();
		}

		public WaterValue GetWater(int _x, int _y, int _z)
		{
			if (!this.checkCoordinates(ref _x, ref _y, ref _z))
			{
				return WaterValue.Empty;
			}
			return this.prefab.GetWater(_x, _y, _z);
		}

		public bool IsWater(int _x, int _y, int _z)
		{
			return this.GetWater(_x, _y, _z).HasMass();
		}

		public int GetBlockFaceTexture(int _x, int _y, int _z, BlockFace _blockFace)
		{
			if (!this.checkCoordinates(ref _x, ref _y, ref _z))
			{
				return 0;
			}
			return (int)(this.prefab.GetTexture(_x, _y, _z) >> (int)(_blockFace * BlockFace.Middle) & 63L);
		}

		public long GetTextureFull(int _x, int _y, int _z)
		{
			if (!this.checkCoordinates(ref _x, ref _y, ref _z))
			{
				return 0L;
			}
			return this.prefab.GetTexture(_x, _y, _z);
		}

		public bool IsOnlyTerrain(int _y)
		{
			return false;
		}

		public bool IsOnlyTerrainLayer(int _idx)
		{
			return false;
		}

		public bool IsEmpty()
		{
			return false;
		}

		public bool IsEmpty(int _y)
		{
			return false;
		}

		public bool IsEmptyLayer(int _y)
		{
			return false;
		}

		public byte GetStability(int _x, int _y, int _z)
		{
			return 15;
		}

		public byte GetStability(int _offs, int _y)
		{
			return 15;
		}

		public void SetStability(int _offs, int _y, byte _v)
		{
		}

		public void SetStability(int _x, int _y, int _z, byte _v)
		{
		}

		public byte GetLight(int x, int y, int z, Chunk.LIGHT_TYPE type)
		{
			return 15;
		}

		public int GetLightValue(int x, int y, int z, int _darknessV)
		{
			return 15;
		}

		public float GetLightBrightness(int x, int y, int z, int _darknessV)
		{
			return 1f;
		}

		public Vector3i GetWorldPos()
		{
			return new Vector3i(this.X, this.Y, this.Z);
		}

		public void SetVertexOffset(int x, int y, int z, Vector3 _vertexOffset)
		{
		}

		public bool GetVertexOffset(int _x, int _y, int _z, out Vector3 _vertexOffset)
		{
			_vertexOffset = Vector3.zero;
			return false;
		}

		public void SetVertexYOffset(int x, int y, int z, float _addYPos)
		{
		}

		public byte GetHeight(int _blockOffset)
		{
			return (byte)this.prefab.size.y;
		}

		public byte GetHeight(int _x, int _z)
		{
			return (byte)this.prefab.size.y;
		}

		public sbyte GetDensity(int _xzOffs, int _y)
		{
			return this.GetDensity(ChunkBlockLayerLegacy.OffsetX(_xzOffs), _y, ChunkBlockLayerLegacy.OffsetX(_xzOffs));
		}

		public sbyte GetDensity(int _x, int _y, int _z)
		{
			if (!this.checkCoordinates(ref _x, ref _y, ref _z))
			{
				return sbyte.MaxValue;
			}
			return this.prefab.GetDensity(_x, _y, _z);
		}

		public sbyte SetDensity(int _xzOffs, int _y, sbyte _density)
		{
			return 0;
		}

		public bool HasSameDensityValue(int _y)
		{
			return false;
		}

		public sbyte GetSameDensityValue(int _y)
		{
			return 0;
		}

		public BlockEntityData GetBlockEntity(Vector3i _blockPos)
		{
			return null;
		}

		public BlockEntityData GetBlockEntity(Transform _transform)
		{
			return null;
		}

		public void SetTopSoilBroken(int _x, int _z)
		{
		}

		public bool IsTopSoil(int _x, int _z)
		{
			return false;
		}

		public byte GetTerrainHeight(int _x, int _z)
		{
			for (int i = this.prefab.size.y - 1; i >= 0; i--)
			{
				if (this.GetBlock(_x, i, _z).Block.shape.IsTerrain())
				{
					return (byte)i;
				}
			}
			return 0;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public Prefab prefab;
	}
}
