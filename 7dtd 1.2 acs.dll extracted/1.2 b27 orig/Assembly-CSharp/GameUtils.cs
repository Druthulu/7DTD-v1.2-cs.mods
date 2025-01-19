using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml;
using System.Xml.Linq;
using Epic.OnlineServices.AntiCheatCommon;
using GamePath;
using Platform;
using UnityEngine;
using UnityEngine.Rendering;

public class GameUtils
{
	public static bool FindMasterBlockForEntityModelBlock(World _world, Vector3 _dirNormalized, string _phsxTag, Vector3 _hitPointPos, Transform _hitTransform, WorldRayHitInfo _hitInfo)
	{
		int num = 0;
		if (_phsxTag.Length > 2)
		{
			char c = _phsxTag[_phsxTag.Length - 2];
			char c2 = _phsxTag[_phsxTag.Length - 1];
			if (c >= '0' && c <= '9' && c2 >= '0' && c2 <= '9')
			{
				num += (int)((c - '0') * '\n');
				num += (int)(c2 - '0');
			}
		}
		ChunkCluster chunkCluster = _world.ChunkClusters[num];
		if (chunkCluster == null)
		{
			return false;
		}
		Vector3 vector = chunkCluster.ToLocalPosition(_hitPointPos);
		Vector3 vector2 = chunkCluster.ToLocalVector(_dirNormalized);
		Vector3i vector3i = World.worldToBlockPos(vector);
		Transform parentTransform = RootTransformRefParent.FindRoot(_hitTransform);
		int num2 = World.toBlockXZ(vector3i.x);
		int num3 = World.toBlockXZ(vector3i.z);
		int num4 = World.toChunkXZ(vector3i.x);
		int num5 = World.toChunkXZ(vector3i.z);
		BlockEntityData blockEntityData;
		if (GameUtils.checkChunk(chunkCluster, num4, num5, parentTransform, vector, vector2, _hitInfo, out blockEntityData))
		{
			_hitInfo.hit.pos = _hitPointPos;
			_hitInfo.lastBlockPos = World.worldToBlockPos(_hitInfo.hit.pos);
			if (!blockEntityData.blockValue.Block.isMultiBlock || blockEntityData.blockValue.Block.multiBlockPos.ContainsPos(_world, blockEntityData.pos, blockEntityData.blockValue, _hitInfo.lastBlockPos))
			{
				BlockValue blockValue;
				_hitInfo.lastBlockPos = Voxel.GoBackOnVoxels(chunkCluster, new Ray(chunkCluster.ToLocalPosition(_hitPointPos + vector2 * 0.01f), -vector2), out blockValue);
			}
			return true;
		}
		if (GameUtils.checkChunk(chunkCluster, (num2 < 8) ? (num4 - 1) : (num4 + 1), num5, parentTransform, vector, vector2, _hitInfo, out blockEntityData))
		{
			_hitInfo.hit.pos = _hitPointPos;
			_hitInfo.lastBlockPos = World.worldToBlockPos(_hitInfo.hit.pos);
			if (!blockEntityData.blockValue.Block.isMultiBlock || blockEntityData.blockValue.Block.multiBlockPos.ContainsPos(_world, blockEntityData.pos, blockEntityData.blockValue, _hitInfo.lastBlockPos))
			{
				BlockValue blockValue;
				_hitInfo.lastBlockPos = Voxel.GoBackOnVoxels(chunkCluster, new Ray(chunkCluster.ToLocalPosition(_hitPointPos + vector2 * 0.01f), -vector2), out blockValue);
			}
			return true;
		}
		if (GameUtils.checkChunk(chunkCluster, num4, (num3 < 8) ? (num5 - 1) : (num5 + 1), parentTransform, vector, vector2, _hitInfo, out blockEntityData))
		{
			_hitInfo.hit.pos = _hitPointPos;
			_hitInfo.lastBlockPos = World.worldToBlockPos(_hitInfo.hit.pos);
			if (!blockEntityData.blockValue.Block.isMultiBlock || blockEntityData.blockValue.Block.multiBlockPos.ContainsPos(_world, blockEntityData.pos, blockEntityData.blockValue, _hitInfo.lastBlockPos))
			{
				BlockValue blockValue;
				_hitInfo.lastBlockPos = Voxel.GoBackOnVoxels(chunkCluster, new Ray(chunkCluster.ToLocalPosition(_hitPointPos + vector2 * 0.01f), -vector2), out blockValue);
			}
			return true;
		}
		if (GameUtils.checkChunk(chunkCluster, (num2 < 8) ? (num4 - 1) : (num4 + 1), (num3 < 8) ? (num5 - 1) : (num5 + 1), parentTransform, vector, vector2, _hitInfo, out blockEntityData))
		{
			_hitInfo.hit.pos = _hitPointPos;
			_hitInfo.lastBlockPos = World.worldToBlockPos(_hitInfo.hit.pos);
			if (!blockEntityData.blockValue.Block.isMultiBlock || blockEntityData.blockValue.Block.multiBlockPos.ContainsPos(_world, blockEntityData.pos, blockEntityData.blockValue, _hitInfo.lastBlockPos))
			{
				BlockValue blockValue;
				_hitInfo.lastBlockPos = Voxel.GoBackOnVoxels(chunkCluster, new Ray(chunkCluster.ToLocalPosition(_hitPointPos + vector2 * 0.01f), -vector2), out blockValue);
			}
			return true;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool checkChunk(ChunkCluster cc, int cX, int cZ, Transform parentTransform, Vector3 localHitPos, Vector3 localDirNormalized, WorldRayHitInfo _hitInfo, out BlockEntityData _ebcd)
	{
		_ebcd = null;
		IChunk chunkSync = cc.GetChunkSync(cX, cZ);
		if (chunkSync == null)
		{
			return false;
		}
		BlockEntityData blockEntity;
		_ebcd = (blockEntity = chunkSync.GetBlockEntity(parentTransform));
		if (blockEntity != null)
		{
			_hitInfo.hit.clrIdx = 0;
			_hitInfo.hit.blockPos = _ebcd.pos;
			_hitInfo.hit.voxelData = HitInfoDetails.VoxelData.GetFrom(chunkSync, World.toBlockXZ(_ebcd.pos.x), World.toBlockY(_ebcd.pos.y), World.toBlockXZ(_ebcd.pos.z));
			Ray ray = new Ray(localHitPos, -1f * localDirNormalized);
			int num = 0;
			do
			{
				Vector3 a;
				BlockFace blockFace;
				_hitInfo.lastBlockPos = Voxel.OneVoxelStep(Vector3i.FromVector3Rounded(ray.origin), ray.origin, ray.direction, out a, out blockFace);
				ray.origin = a + localDirNormalized * 0.001f;
			}
			while (!cc.GetBlock(_hitInfo.lastBlockPos).isair && num++ < 3);
			return true;
		}
		return false;
	}

	public static void EnableRagdoll(GameObject _model, bool _bEnable, bool _bUseGravity)
	{
		foreach (Rigidbody rigidbody in _model.GetComponentsInChildren<Rigidbody>())
		{
			rigidbody.isKinematic = !_bEnable;
			rigidbody.useGravity = _bUseGravity;
		}
		Collider[] componentsInChildren2 = _model.GetComponentsInChildren<Collider>();
		for (int i = 0; i < componentsInChildren2.Length; i++)
		{
			componentsInChildren2[i].enabled = _bEnable;
		}
	}

	public static Entity GetHitRootEntity(string _tag, Transform _hitTransform)
	{
		if (_tag.StartsWith("E_BP_"))
		{
			RootTransformRefEntity rootTransformRefEntity;
			if (_hitTransform.TryGetComponent<RootTransformRefEntity>(out rootTransformRefEntity))
			{
				if (rootTransformRefEntity.RootTransform)
				{
					return rootTransformRefEntity.RootTransform.GetComponent<Entity>();
				}
			}
			else
			{
				Transform transform = RootTransformRefEntity.FindEntityUpwards(_hitTransform);
				if (transform)
				{
					return transform.GetComponent<Entity>();
				}
			}
		}
		else if (_tag.StartsWith("E_Vehicle"))
		{
			return CollisionCallForward.FindEntity(_hitTransform);
		}
		return null;
	}

	public static Transform GetHitRootTransform(string _tag, Transform _hitTransform)
	{
		if (!_tag.StartsWith("E_BP_"))
		{
			if (_tag.Equals("E_Vehicle"))
			{
				Entity entity = CollisionCallForward.FindEntity(_hitTransform);
				if (entity)
				{
					return entity.transform;
				}
			}
			return _hitTransform;
		}
		RootTransformRefEntity rootTransformRefEntity;
		if (_hitTransform.TryGetComponent<RootTransformRefEntity>(out rootTransformRefEntity))
		{
			return rootTransformRefEntity.RootTransform;
		}
		return RootTransformRefEntity.FindEntityUpwards(_hitTransform);
	}

	public static string GetTransformPath(Transform _t)
	{
		if (!_t)
		{
			return "null";
		}
		if (!_t.parent)
		{
			return _t.name;
		}
		return GameUtils.GetTransformPath(_t.parent) + "/" + _t.name;
	}

	public static string GetChildTransformPath(Transform _parent, Transform _child)
	{
		if (_child.parent == null)
		{
			throw new Exception(string.Concat(new string[]
			{
				"GetChildTransformPath: '",
				_child.name,
				"' is a root object and not in the path underneath '",
				_parent.name,
				"'"
			}));
		}
		if (_child.parent == _parent)
		{
			return _child.name;
		}
		return GameUtils.GetChildTransformPath(_parent, _child.parent) + "/" + _child.name;
	}

	public static void FindTagInChilds(Transform _parent, string _tag, List<Transform> _list)
	{
		int childCount = _parent.childCount;
		for (int i = 0; i < childCount; i++)
		{
			Transform child = _parent.GetChild(i);
			if (child.CompareTag(_tag))
			{
				_list.Add(child);
			}
			GameUtils.FindTagInChilds(child, _tag, _list);
		}
	}

	public static Transform FindTagInChilds(Transform _parent, string _tag)
	{
		int childCount = _parent.childCount;
		for (int i = 0; i < childCount; i++)
		{
			Transform child = _parent.GetChild(i);
			if (child.CompareTag(_tag))
			{
				return child;
			}
		}
		for (int j = 0; j < childCount; j++)
		{
			Transform transform = GameUtils.FindTagInChilds(_parent.GetChild(j), _tag);
			if (transform != null)
			{
				return transform;
			}
		}
		return null;
	}

	public static Transform FindTagInDirectChilds(Transform _parent, string _tag)
	{
		int childCount = _parent.childCount;
		for (int i = 0; i < childCount; i++)
		{
			Transform child = _parent.GetChild(i);
			if (child.CompareTag(_tag))
			{
				return child;
			}
		}
		return null;
	}

	public static Transform FindChildWithPartialName(Transform root, params string[] names)
	{
		foreach (string b in names)
		{
			if (root.name.ContainsCaseInsensitive(b))
			{
				return root;
			}
			for (int j = 0; j < root.childCount; j++)
			{
				Transform child = root.GetChild(j);
				if (child.name.ContainsCaseInsensitive(b))
				{
					return child;
				}
			}
		}
		return null;
	}

	public static void FindDeepChildWithPartialName(Transform root, string name, ref List<Transform> found)
	{
		if (root.name.ContainsCaseInsensitive(name))
		{
			found.Add(root);
		}
		for (int i = 0; i < root.childCount; i++)
		{
			GameUtils.FindDeepChildWithPartialName(root.GetChild(i), name, ref found);
		}
	}

	[Conditional("UNITY_EDITOR")]
	public static void HideObjectInEditor(GameObject _obj)
	{
		UnityEngine.Object.DontDestroyOnLoad(_obj);
	}

	public static bool IsColliderWithinBlock(Vector3i blockPosition, BlockValue blockValue)
	{
		int num = 3899392;
		Quaternion rotation = blockValue.Block.shape.GetRotation(blockValue);
		Bounds blockPlacementBounds = GameUtils.GetBlockPlacementBounds(blockValue.Block);
		Vector3 a = blockPlacementBounds.size;
		Vector3 vector = World.blockToTransformPos(blockPosition) - Origin.position + new Vector3(0f, 0.5f, 0f);
		if (blockPlacementBounds.center != Vector3.zero)
		{
			vector += rotation * blockPlacementBounds.center;
		}
		if (blockValue.Block.isOversized)
		{
			num |= 1082130432;
			a -= new Vector3(0.1f, 0.1f, 0.1f);
		}
		else if (blockValue.Block.shape.IsTerrain())
		{
			vector -= new Vector3(0f, 0.25f, 0f);
		}
		Vector3 halfExtents = a * 0.5f;
		int num2 = Physics.OverlapBoxNonAlloc(vector, halfExtents, GameUtils.overlapBoxHits, rotation, num);
		if (num2 == GameUtils.overlapBoxHits.Length)
		{
			UnityEngine.Debug.LogError(string.Format("OverlapBox reached maximum hit count ({0}); overlapBoxHits array size may be insufficient.", num2));
		}
		for (int i = 0; i < num2; i++)
		{
			if (blockValue.Block.isOversized)
			{
				if (!GameUtils.overlapBoxHits[i].CompareTag("T_Mesh"))
				{
					return true;
				}
			}
			else
			{
				if (!GameUtils.IsBlockOrTerrain(GameUtils.overlapBoxHits[i]) && !GameUtils.overlapBoxHits[i].CompareTag("Item"))
				{
					return true;
				}
				if (GameUtils.overlapBoxHits[i].CompareTag("T_Block"))
				{
					Transform entityParentTransform = RootTransformRefParent.FindRoot(GameUtils.overlapBoxHits[i].transform);
					BlockEntityData blockEntityData;
					if (GameUtils.TryFindEntityData(blockPosition, entityParentTransform, out blockEntityData) && blockEntityData.blockValue.Block.isOversized)
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	public static Vector3 GetMultiBlockBoundsOffset(Vector3 multiBlockDim)
	{
		return new Vector3((multiBlockDim.x % 2f == 0f) ? -0.5f : 0f, multiBlockDim.y / 2f - 0.5f, (multiBlockDim.z % 2f == 0f) ? -0.5f : 0f);
	}

	public static Bounds GetBlockPlacementBounds(Block block)
	{
		if (block.isOversized)
		{
			return block.oversizedBounds;
		}
		if (block.isMultiBlock)
		{
			Vector3 vector = block.multiBlockPos.dim;
			return new Bounds(GameUtils.GetMultiBlockBoundsOffset(vector), vector);
		}
		return new Bounds(Vector3.zero, Vector3.one);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool TryFindEntityData(Vector3i entityWorldHitPosition, Transform entityParentTransform, out BlockEntityData ebcd)
	{
		int num = World.toBlockXZ(entityWorldHitPosition.x);
		int num2 = World.toBlockXZ(entityWorldHitPosition.z);
		int num3 = World.toChunkXZ(entityWorldHitPosition.x);
		int num4 = World.toChunkXZ(entityWorldHitPosition.z);
		int num5 = (num < 8) ? -1 : 1;
		int num6 = (num2 < 8) ? -1 : 1;
		ChunkCluster chunkCache = GameManager.Instance.World.ChunkCache;
		if (chunkCache != null)
		{
			for (int i = 0; i < 2; i++)
			{
				for (int j = 0; j < 2; j++)
				{
					Chunk chunkSync = chunkCache.GetChunkSync(num3 + j * num5, num4 + i * num6);
					ebcd = ((chunkSync != null) ? chunkSync.GetBlockEntity(entityParentTransform) : null);
					if (ebcd != null)
					{
						return true;
					}
				}
			}
		}
		UnityEngine.Debug.LogWarning(string.Format("Failed to find entity data for Transform \"{0}\" with hit position \"{1}\"", entityParentTransform.name, entityWorldHitPosition), entityParentTransform);
		ebcd = null;
		return false;
	}

	public static void DebugDrawPathFromEntity(Entity _e, PathEntity _path, Color _color)
	{
	}

	public static void CreateEmptyFlatLevel(string _worldName, int _worldSize, int _terrainHeight = 60)
	{
		PathAbstractions.AbstractedLocation worldLocation = new PathAbstractions.AbstractedLocation(PathAbstractions.EAbstractedLocationType.UserDataPath, _worldName, GameIO.GetUserGameDataDir() + "/GeneratedWorlds/" + _worldName, null, true, null);
		SdDirectory.CreateDirectory(worldLocation.FullPath);
		World world = new World();
		WorldState worldState = new WorldState();
		worldState.SetFrom(world, EnumChunkProviderId.ChunkDataDriven);
		worldState.ResetDynamicData();
		worldState.Save(worldLocation.FullPath + "/main.ttw");
		GameUtils.CreateWorldFilesForFlatLevel(_worldName, _worldSize, worldLocation, _terrainHeight);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void CreateWorldFilesForFlatLevel(string _worldName, int _worldSize, PathAbstractions.AbstractedLocation _worldLocation, int _terrainHeight)
	{
		int num = _worldSize * 2;
		byte[] array = new byte[num];
		for (int i = 0; i < _worldSize; i++)
		{
			array[2 * i] = 0;
			array[2 * i + 1] = 60;
		}
		using (BufferedStream bufferedStream = new BufferedStream(SdFile.OpenWrite(_worldLocation.FullPath + "/dtm.raw")))
		{
			for (int j = 0; j < _worldSize; j++)
			{
				bufferedStream.Write(array, 0, num);
			}
		}
		new GameUtils.WorldInfo(_worldName, "Empty World", new string[]
		{
			"Survival",
			"Creative"
		}, new Vector2i(_worldSize, _worldSize), 1, false, false, Constants.cVersionInformation, null).Save(_worldLocation);
		new SpawnPointManager(true)
		{
			spawnPointList = 
			{
				new SpawnPoint(new Vector3i(0, _terrainHeight + 1, 0))
			}
		}.Save(_worldLocation.FullPath);
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.CreateXmlDeclaration();
		xmlDocument.AddXmlElement("WaterSources");
		xmlDocument.SdSave(_worldLocation.FullPath + "/water_info.xml");
		GameUtils.CreateForestBiomeMap(_worldSize, _worldLocation, BiomeDefinition.BiomeType.PineForest);
		XmlDocument xmlDocument2 = new XmlDocument();
		xmlDocument2.CreateXmlDeclaration();
		xmlDocument2.AddXmlElement("prefabs");
		xmlDocument2.SdSave(_worldLocation.FullPath + "/prefabs.xml");
		GameUtils.CreateSimpleRadiationMap(_worldSize, _worldLocation, (_worldSize <= 1024) ? 128 : ((_worldSize <= 2048) ? 256 : 512), 16);
		GameUtils.CreateEmptySplatMap(_worldSize, _worldLocation);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void CreateSimpleRadiationMap(int _worldSize, PathAbstractions.AbstractedLocation _worldLocation, int _radiationBorderSize = 128, int _downScale = 16)
	{
		int num = _worldSize / _downScale;
		Color red = Color.red;
		Color32 color = new Color32(0, 0, 0, byte.MaxValue);
		Texture2D texture2D = new Texture2D(num, num, TextureFormat.RGBA32, false);
		texture2D.FillTexture(red, false, false);
		int num2 = _radiationBorderSize / _downScale;
		int num3 = num - 2 * num2;
		Color32[] array = new Color32[num3];
		for (int i = 0; i < num3; i++)
		{
			array[i] = color;
		}
		for (int j = num2; j < num - num2; j++)
		{
			texture2D.SetPixels32(num2, j, num3, 1, array);
		}
		texture2D.Apply();
		SdFile.WriteAllBytes(_worldLocation.FullPath + "/radiation.png", texture2D.EncodeToPNG());
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void CreateEmptySplatMap(int _worldSize, PathAbstractions.AbstractedLocation _worldLocation)
	{
		Color32 c = new Color32(0, 0, 0, 0);
		Texture2D texture2D = new Texture2D(_worldSize, _worldSize, TextureFormat.RGBA32, false);
		texture2D.FillTexture(c, false, false);
		texture2D.Apply();
		byte[] bytes = texture2D.EncodeToPNG();
		MicroStopwatch microStopwatch = new MicroStopwatch(true);
		SdFile.WriteAllBytes(_worldLocation.FullPath + "/splat1.png", bytes);
		SdFile.WriteAllBytes(_worldLocation.FullPath + "/splat2.png", bytes);
		SdFile.WriteAllBytes(_worldLocation.FullPath + "/splat3.png", bytes);
		Log.Out(string.Format("Write tex took {0} ms", microStopwatch.ElapsedMilliseconds));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void CreateForestBiomeMap(int _worldSize, PathAbstractions.AbstractedLocation _worldLocation, BiomeDefinition.BiomeType _biome = BiomeDefinition.BiomeType.PineForest)
	{
		Color32 c = GameUtils.UIntToColor(BiomeDefinition.GetBiomeColor(_biome), false);
		Texture2D texture2D = new Texture2D(_worldSize, _worldSize, TextureFormat.RGBA32, false);
		texture2D.FillTexture(c, false, false);
		texture2D.Apply();
		MicroStopwatch microStopwatch = new MicroStopwatch(true);
		SdFile.WriteAllBytes(_worldLocation.FullPath + "/biomes.png", texture2D.EncodeToPNG());
		Log.Out(string.Format("Write tex took {0} ms", microStopwatch.ElapsedMilliseconds));
	}

	public static void DeleteWorld(PathAbstractions.AbstractedLocation _worldLocation)
	{
		if (string.IsNullOrEmpty(_worldLocation.Name))
		{
			return;
		}
		SdDirectory.Delete(_worldLocation.FullPath, true);
		string saveGameDir = GameIO.GetSaveGameDir(_worldLocation.Name);
		if (SdDirectory.Exists(saveGameDir))
		{
			SdDirectory.Delete(saveGameDir, true);
		}
	}

	public static int WorldTimeToDays(ulong _worldTime)
	{
		return (int)(_worldTime / 24000UL + 1UL);
	}

	public static int WorldTimeToHours(ulong _worldTime)
	{
		return (int)(_worldTime / 1000UL) % 24;
	}

	public static int WorldTimeToMinutes(ulong _worldTime)
	{
		return (int)(_worldTime / 1000.0 * 60.0) % 60;
	}

	public static float WorldTimeToTotalSeconds(float _worldTime)
	{
		return _worldTime * 3.6f;
	}

	public static uint WorldTimeToTotalMinutes(ulong _worldTime)
	{
		return (uint)(_worldTime * 0.06);
	}

	public static int WorldTimeToTotalHours(ulong _worldTime)
	{
		return (int)(_worldTime / 1000UL);
	}

	public static ulong TotalMinutesToWorldTime(uint _totalMinutes)
	{
		return (ulong)(_totalMinutes / 0.06);
	}

	[return: TupleElementNames(new string[]
	{
		"Days",
		"Hours",
		"Minutes"
	})]
	public static ValueTuple<int, int, int> WorldTimeToElements(ulong _worldTime)
	{
		int item = (int)(_worldTime / 24000UL + 1UL);
		int item2 = (int)(_worldTime / 1000UL) % 24;
		int item3 = (int)(_worldTime * 0.06) % 60;
		return new ValueTuple<int, int, int>(item, item2, item3);
	}

	public static string WorldTimeToString(ulong _worldTime)
	{
		ValueTuple<int, int, int> valueTuple = GameUtils.WorldTimeToElements(_worldTime);
		int item = valueTuple.Item1;
		int item2 = valueTuple.Item2;
		int item3 = valueTuple.Item3;
		return string.Format("{0} {1:D2}:{2:D2}", item, item2, item3);
	}

	public static string WorldTimeDeltaToString(ulong _worldTime)
	{
		ValueTuple<int, int, int> valueTuple = GameUtils.WorldTimeToElements(_worldTime);
		int item = valueTuple.Item1;
		int item2 = valueTuple.Item2;
		int item3 = valueTuple.Item3;
		return string.Format("{0} {1:D2}:{2:D2}", item - 1, item2, item3);
	}

	public static ulong DayTimeToWorldTime(int _day, int _hours, int _minutes)
	{
		if (_day < 1)
		{
			return 0UL;
		}
		return (ulong)((long)(_day - 1) * 24000L + (long)(_hours * 1000) + (long)(_minutes * 1000 / 60));
	}

	public static ulong DaysToWorldTime(int _day)
	{
		if (_day < 1)
		{
			return 0UL;
		}
		return (ulong)(((long)_day - 1L) * 24000L);
	}

	public static ulong DaysToWorldTimeMidnight(int _day)
	{
		return GameUtils.DaysToWorldTime(_day) + 16000UL;
	}

	[return: TupleElementNames(new string[]
	{
		"duskHour",
		"dawnHour"
	})]
	public static ValueTuple<int, int> CalcDuskDawnHours(int _dayLightLength)
	{
		ValueTuple<int, int> valueTuple;
		valueTuple.Item1 = 22;
		if (_dayLightLength > 22)
		{
			valueTuple.Item1 = Mathf.Clamp(_dayLightLength, 0, 23);
		}
		valueTuple.Item2 = Mathf.Clamp(valueTuple.Item1 - _dayLightLength, 0, 23);
		return valueTuple;
	}

	public static bool IsBloodMoonTime(ulong _worldTime, [TupleElementNames(new string[]
	{
		"duskHour",
		"dawnHour"
	})] ValueTuple<int, int> _duskDawnTimes, int _bmDay)
	{
		ValueTuple<int, int, int> valueTuple = GameUtils.WorldTimeToElements(_worldTime);
		int item = valueTuple.Item1;
		int item2 = valueTuple.Item2;
		if (item == _bmDay)
		{
			if (item2 >= _duskDawnTimes.Item1)
			{
				return true;
			}
		}
		else if (item > 1 && item == _bmDay + 1 && item2 < _duskDawnTimes.Item2)
		{
			return true;
		}
		return false;
	}

	public static List<string> GetWorldFilesToTransmitToClient(string _worldFolder)
	{
		string[] files = SdDirectory.GetFiles(_worldFolder);
		for (int i = 0; i < files.Length; i++)
		{
			files[i] = GameIO.GetFilenameFromPath(files[i]);
		}
		return GameUtils.GetWorldFilesToTransmitToClient(files);
	}

	public static List<string> GetWorldFilesToTransmitToClient(ICollection<string> _files)
	{
		HashSet<string> hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		foreach (string filepath in _files)
		{
			hashSet.Add(GameIO.GetFilenameFromPathWithoutExtension(filepath));
		}
		List<string> list = new List<string>();
		foreach (string text in _files)
		{
			string filenameFromPathWithoutExtension = GameIO.GetFilenameFromPathWithoutExtension(text);
			if (true & !hashSet.Contains(filenameFromPathWithoutExtension + "_processed") & !text.ContainsCaseInsensitive("GenerationInfo") & !text.EndsWith(".bak", StringComparison.OrdinalIgnoreCase) & !text.ContainsCaseInsensitive("Version.txt") & !text.ContainsCaseInsensitive("checksums.txt"))
			{
				list.Add(text);
			}
		}
		return list;
	}

	public static void DebugOutputGamePrefs(GameUtils.OutputDelegate _output)
	{
		SortedList<string, string> sortedList = new SortedList<string, string>();
		for (int num = 0; num != 269; num++)
		{
			string text = ((EnumGamePrefs)num).ToStringCached<EnumGamePrefs>();
			if (!text.Contains("Password") && text != "ServerHistoryCache")
			{
				SortedList<string, string> sortedList2 = sortedList;
				string key = text;
				string str = text;
				string str2 = " = ";
				object @object = GamePrefs.GetObject((EnumGamePrefs)num);
				sortedList2.Add(key, str + str2 + ((@object != null) ? @object.ToString() : null));
			}
		}
		foreach (string key2 in sortedList.Keys)
		{
			_output(sortedList[key2]);
		}
	}

	public static void DebugOutputGameStats(GameUtils.OutputDelegate _output)
	{
		SortedList<string, string> sortedList = new SortedList<string, string>();
		for (int num = 0; num != 66; num++)
		{
			string text = ((EnumGameStats)num).ToStringCached<EnumGameStats>();
			SortedList<string, string> sortedList2 = sortedList;
			string key = text;
			string str = text;
			string str2 = " = ";
			object @object = GameStats.GetObject((EnumGameStats)num);
			sortedList2.Add(key, str + str2 + ((@object != null) ? @object.ToString() : null));
		}
		foreach (string key2 in sortedList.Keys)
		{
			_output(sortedList[key2]);
		}
	}

	public static void KickPlayerForClientInfo(ClientInfo _cInfo, GameUtils.KickPlayerData _kickData)
	{
		_cInfo.SendPackage(NetPackageManager.GetPackage<NetPackagePlayerDenied>().Setup(_kickData));
		string str = _cInfo.ToString();
		string str2 = "Kicking player (";
		GameUtils.KickPlayerData kickPlayerData = _kickData;
		Log.Out(str2 + kickPlayerData.ToString() + "): " + str);
		ThreadManager.StartCoroutine(GameUtils.disconnectLater(0.5f, _cInfo));
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public static IEnumerator disconnectLater(float _delayInSec, ClientInfo _clientInfo)
	{
		_clientInfo.disconnecting = true;
		yield return new WaitForSecondsRealtime(_delayInSec);
		SingletonMonoBehaviour<ConnectionManager>.Instance.DisconnectClient(_clientInfo, false, false);
		yield break;
	}

	public static void ForceDisconnect()
	{
		ThreadManager.StartCoroutine(GameUtils.ForceDisconnectRoutine(0.5f));
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public static IEnumerator ForceDisconnectRoutine(float delay)
	{
		yield return new WaitForSeconds(delay);
		GameManager.Instance.Disconnect();
		if (!GameManager.IsDedicatedServer)
		{
			yield return new WaitForSeconds(0.5f);
			GameUtils.KickPlayerData kickData = default(GameUtils.KickPlayerData);
			kickData.reason = GameUtils.EKickReason.InternalNetConnectionError;
			GameManager.Instance.ShowMessagePlayerDenied(kickData);
		}
		yield break;
	}

	public static void WriteItemStack(BinaryWriter _bw, IList<ItemStack> _itemStack)
	{
		_bw.Write((ushort)_itemStack.Count);
		for (int i = 0; i < _itemStack.Count; i++)
		{
			_itemStack[i].Write(_bw);
		}
	}

	public static ItemStack[] ReadItemStackOld(BinaryReader _br)
	{
		int num = (int)_br.ReadUInt16();
		ItemStack[] array = ItemStack.CreateArray(num);
		for (int i = 0; i < num; i++)
		{
			array[i].ReadOld(_br);
			if (ItemClass.GetForId(array[i].itemValue.type) == null)
			{
				array[i] = ItemStack.Empty.Clone();
			}
		}
		return array;
	}

	public static ItemStack[] ReadItemStack(BinaryReader _br)
	{
		int num = (int)_br.ReadUInt16();
		ItemStack[] array = ItemStack.CreateArray(num);
		for (int i = 0; i < num; i++)
		{
			array[i].Read(_br);
			if (ItemClass.GetForId(array[i].itemValue.type) == null)
			{
				array[i] = ItemStack.Empty.Clone();
			}
		}
		return array;
	}

	public static void WriteItemValueArray(BinaryWriter _bw, ItemValue[] _items)
	{
		_bw.Write((ushort)_items.Length);
		foreach (ItemValue itemValue in _items)
		{
			bool flag = itemValue != null;
			_bw.Write(flag);
			if (flag)
			{
				itemValue.Write(_bw);
			}
		}
	}

	public static ItemValue[] ReadItemValueArray(BinaryReader _br)
	{
		ItemValue[] array = new ItemValue[(int)_br.ReadUInt16()];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = new ItemValue();
			if (_br.ReadBoolean())
			{
				array[i].Read(_br);
			}
		}
		return array;
	}

	public static void HarvestOnAttack(ItemActionData _actionData, Dictionary<string, ItemActionAttack.Bonuses> ToolBonuses)
	{
		if (_actionData.invData.world.IsEditor() || !(_actionData.invData.holdingEntity is EntityPlayerLocal) || _actionData.attackDetails == null || _actionData.attackDetails.itemsToDrop == null)
		{
			return;
		}
		if (GameUtils.random == null)
		{
			GameUtils.random = GameRandomManager.Instance.CreateGameRandom();
			GameUtils.random.SetSeed((int)Stopwatch.GetTimestamp());
		}
		Block block = _actionData.attackDetails.blockBeingDamaged.Block;
		if (block.RepairItemsMeshDamage != null)
		{
			BlockValue blockBeingDamaged = _actionData.attackDetails.blockBeingDamaged;
			blockBeingDamaged.damage += _actionData.attackDetails.damageGiven;
			_actionData.attackDetails.bKilled = (blockBeingDamaged.damage < block.MaxDamage && block.shape.UseRepairDamageState(blockBeingDamaged));
		}
		if (_actionData.attackDetails.bKilled)
		{
			if (!_actionData.attackDetails.itemsToDrop.ContainsKey(EnumDropEvent.Destroy))
			{
				if (!_actionData.attackDetails.blockBeingDamaged.isair && _actionData.attackDetails.bBlockHit)
				{
					ItemValue iv = _actionData.attackDetails.blockBeingDamaged.ToItemValue();
					int count = 1;
					GameUtils.collectHarvestedItem(_actionData, iv, count, 1f, false);
				}
			}
			else
			{
				List<Block.SItemDropProb> list = _actionData.attackDetails.itemsToDrop[EnumDropEvent.Destroy];
				for (int i = 0; i < list.Count; i++)
				{
					if (_actionData.attackDetails.bBlockHit && list[i].name.Equals("[recipe]"))
					{
						List<Recipe> recipes = CraftingManager.GetRecipes(_actionData.attackDetails.blockBeingDamaged.Block.GetBlockName());
						if (recipes.Count > 0)
						{
							for (int j = 0; j < recipes[0].ingredients.Count; j++)
							{
								if (recipes[0].ingredients[j].count / 2 > 0)
								{
									GameUtils.collectHarvestedItem(_actionData, recipes[0].ingredients[j].itemValue, recipes[0].ingredients[j].count / 2, 1f, false);
								}
							}
						}
					}
					else
					{
						float num = 1f;
						if (list[i].toolCategory != null)
						{
							num = 0f;
							if (ToolBonuses != null && ToolBonuses.ContainsKey(list[i].toolCategory))
							{
								num = ToolBonuses[list[i].toolCategory].Tool;
							}
						}
						num = EffectManager.GetValue(PassiveEffects.HarvestCount, _actionData.invData.itemValue, num, _actionData.invData.holdingEntity, null, FastTags<TagGroup.Global>.Parse(list[i].tag), true, true, true, true, true, 1, true, false);
						ItemValue itemValue = list[i].name.Equals("*") ? _actionData.attackDetails.blockBeingDamaged.ToItemValue() : new ItemValue(ItemClass.GetItem(list[i].name, false).type, false);
						if (itemValue.type != 0 && ItemClass.list[itemValue.type] != null && (list[i].prob > 0.999f || GameUtils.random.RandomFloat <= list[i].prob))
						{
							int num2 = (int)((float)GameUtils.random.RandomRange(list[i].minCount, list[i].maxCount + 1) * num);
							if (num2 > 0)
							{
								GameUtils.collectHarvestedItem(_actionData, itemValue, num2, 1f, false);
							}
						}
					}
				}
			}
		}
		if (_actionData.attackDetails.bBlockHit)
		{
			_actionData.invData.holdingEntity.MinEventContext.BlockValue = _actionData.attackDetails.blockBeingDamaged;
			_actionData.invData.holdingEntity.FireEvent(MinEventTypes.onSelfHarvestBlock, true);
		}
		else
		{
			_actionData.invData.holdingEntity.MinEventContext.Other = (_actionData.attackDetails.entityHit as EntityAlive);
			_actionData.invData.holdingEntity.FireEvent(MinEventTypes.onSelfHarvestOther, true);
		}
		if (_actionData.attackDetails.itemsToDrop.ContainsKey(EnumDropEvent.Harvest))
		{
			List<Block.SItemDropProb> list2 = _actionData.attackDetails.itemsToDrop[EnumDropEvent.Harvest];
			for (int k = 0; k < list2.Count; k++)
			{
				float num3 = 0f;
				if (list2[k].toolCategory != null)
				{
					num3 = 0f;
					if (ToolBonuses != null && ToolBonuses.ContainsKey(list2[k].toolCategory))
					{
						num3 = ToolBonuses[list2[k].toolCategory].Tool;
					}
				}
				ItemValue itemValue2 = list2[k].name.Equals("*") ? _actionData.attackDetails.blockBeingDamaged.ToItemValue() : new ItemValue(ItemClass.GetItem(list2[k].name, false).type, false);
				if (itemValue2.type != 0 && ItemClass.list[itemValue2.type] != null)
				{
					num3 = EffectManager.GetValue(PassiveEffects.HarvestCount, _actionData.invData.itemValue, num3, _actionData.invData.holdingEntity, null, FastTags<TagGroup.Global>.Parse(list2[k].tag), true, true, true, true, true, 1, true, false);
					int num4 = (int)((float)GameUtils.random.RandomRange(list2[k].minCount, list2[k].maxCount + 1) * num3);
					int num5 = num4 - num4 / 3;
					if (num5 > 0)
					{
						GameUtils.collectHarvestedItem(_actionData, itemValue2, num5, list2[k].prob, true);
					}
					if (_actionData.attackDetails.bKilled)
					{
						num5 = num4 / 3;
						float num6 = list2[k].prob;
						float resourceScale = list2[k].resourceScale;
						if (resourceScale > 0f && resourceScale < 1f)
						{
							num6 /= resourceScale;
							num5 = (int)((float)num5 * resourceScale);
							if (num5 < 1)
							{
								num5++;
							}
						}
						if (num5 > 0)
						{
							GameUtils.collectHarvestedItem(_actionData, itemValue2, num5, num6, false);
						}
					}
				}
			}
		}
		_actionData.attackDetails.blockBeingDamaged = BlockValue.Air;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void collectHarvestedItem(ItemActionData _actionData, ItemValue _iv, int _count, float _prob, bool _bScaleCountOnDamage = true)
	{
		if (GameUtils.random == null)
		{
			GameUtils.random = GameRandomManager.Instance.CreateGameRandom();
			GameUtils.random.SetSeed((int)Stopwatch.GetTimestamp());
		}
		if (_bScaleCountOnDamage)
		{
			float num = (float)_actionData.attackDetails.damageMax / (float)_count;
			int num2 = (int)((Utils.FastMin(_actionData.attackDetails.damageTotalOfTarget, (float)_actionData.attackDetails.damageMax) - (float)_actionData.attackDetails.damageGiven) / num + 0.5f);
			int num3 = Mathf.Min((int)(_actionData.attackDetails.damageTotalOfTarget / num + 0.5f), _count);
			int b = _count;
			_count = num3 - num2;
			if (_actionData.attackDetails.damageTotalOfTarget > (float)_actionData.attackDetails.damageMax)
			{
				_count = Mathf.Min(_count, b);
			}
		}
		if (GameUtils.random.RandomFloat <= _prob && _count > 0)
		{
			ItemStack itemStack = new ItemStack(_iv, _count);
			LocalPlayerUI uiforPlayer = LocalPlayerUI.GetUIForPlayer(_actionData.invData.holdingEntity as EntityPlayerLocal);
			XUiM_PlayerInventory playerInventory = uiforPlayer.xui.PlayerInventory;
			QuestEventManager.Current.HarvestedItem(_actionData.invData.itemValue, itemStack, _actionData.attackDetails.blockBeingDamaged);
			if (!playerInventory.AddItem(itemStack))
			{
				GameManager.Instance.ItemDropServer(new ItemStack(_iv, itemStack.count), GameManager.Instance.World.GetPrimaryPlayer().GetDropPosition(), new Vector3(0.5f, 0.5f, 0.5f), GameManager.Instance.World.GetPrimaryPlayerId(), 60f, false);
			}
			uiforPlayer.entityPlayer.Progression.AddLevelExp((int)(itemStack.itemValue.ItemClass.MadeOfMaterial.Experience * (float)_count), "_xpFromHarvesting", Progression.XPTypes.Harvesting, true, true);
		}
	}

	public static void DrawCube(Vector3 _pos, Color _col)
	{
		UnityEngine.Debug.DrawLine(_pos + new Vector3(0.1f, 0.1f, 0.1f), _pos + new Vector3(0.9f, 0.1f, 0.1f), _col, 10f);
		UnityEngine.Debug.DrawLine(_pos + new Vector3(0.1f, 0.1f, 0.1f), _pos + new Vector3(0.1f, 0.1f, 0.9f), _col, 10f);
		UnityEngine.Debug.DrawLine(_pos + new Vector3(0.9f, 0.1f, 0.1f), _pos + new Vector3(0.9f, 0.1f, 0.9f), _col, 10f);
		UnityEngine.Debug.DrawLine(_pos + new Vector3(0.9f, 0.1f, 0.9f), _pos + new Vector3(0.1f, 0.1f, 0.9f), _col, 10f);
		UnityEngine.Debug.DrawLine(_pos + new Vector3(0.1f, 0.9f, 0.1f), _pos + new Vector3(0.9f, 0.9f, 0.1f), _col, 10f);
		UnityEngine.Debug.DrawLine(_pos + new Vector3(0.1f, 0.9f, 0.1f), _pos + new Vector3(0.1f, 0.9f, 0.9f), _col, 10f);
		UnityEngine.Debug.DrawLine(_pos + new Vector3(0.9f, 0.9f, 0.1f), _pos + new Vector3(0.9f, 0.9f, 0.9f), _col, 10f);
		UnityEngine.Debug.DrawLine(_pos + new Vector3(0.9f, 0.9f, 0.9f), _pos + new Vector3(0.1f, 0.9f, 0.9f), _col, 10f);
		UnityEngine.Debug.DrawLine(_pos + new Vector3(0.1f, 0.1f, 0.1f), _pos + new Vector3(0.1f, 0.9f, 0.1f), _col, 10f);
		UnityEngine.Debug.DrawLine(_pos + new Vector3(0.1f, 0.1f, 0.9f), _pos + new Vector3(0.1f, 0.9f, 0.9f), _col, 10f);
		UnityEngine.Debug.DrawLine(_pos + new Vector3(0.9f, 0.1f, 0.1f), _pos + new Vector3(0.9f, 0.9f, 0.1f), _col, 10f);
		UnityEngine.Debug.DrawLine(_pos + new Vector3(0.9f, 0.1f, 0.9f), _pos + new Vector3(0.9f, 0.9f, 0.9f), _col, 10f);
	}

	public static string SafeStringFormat(string _s)
	{
		return _s.Replace("{", "{{").Replace("}", "}}");
	}

	public static Vector3 GetNormalFromHitInfo(Vector3i _blockPos, Collider _hitCollider, int _hitTriangleIdx, out Vector3 _hitFaceCenter)
	{
		_hitFaceCenter = Vector3.zero;
		if (_hitTriangleIdx < 0)
		{
			return Vector3.zero;
		}
		MeshCollider meshCollider = _hitCollider as MeshCollider;
		if (!(meshCollider != null) || !(meshCollider.sharedMesh != null) || !meshCollider.sharedMesh.isReadable)
		{
			return Vector3.zero;
		}
		Mesh sharedMesh = meshCollider.sharedMesh;
		GameUtils.tempVertices.Clear();
		sharedMesh.GetVertices(GameUtils.tempVertices);
		GameUtils.tempTriangles.Clear();
		sharedMesh.GetTriangles(GameUtils.tempTriangles, 0);
		int num = _hitTriangleIdx * 3;
		if (num >= GameUtils.tempTriangles.Count || GameUtils.tempTriangles[num] >= GameUtils.tempVertices.Count)
		{
			return Vector3.zero;
		}
		Vector3 a = GameUtils.tempVertices[GameUtils.tempTriangles[num]];
		Vector3 b = GameUtils.tempVertices[GameUtils.tempTriangles[num + 1]];
		Vector3 b2 = GameUtils.tempVertices[GameUtils.tempTriangles[num + 2]];
		Vector3 result = Vector3.Cross(a - b, a - b2);
		Vector3 a2 = (a + b + b2) / 3f;
		_hitFaceCenter = a2 + World.toChunkXyzWorldPos(_blockPos);
		return result;
	}

	public static BlockFace GetBlockFaceFromHitInfo(Vector3i _blockPos, BlockValue _blockValue, Collider _hitCollider, int _hitTriangleIdx, out Vector3 _hitFaceCenter, out Vector3 _hitFaceNormal)
	{
		_hitFaceCenter = Vector3.zero;
		_hitFaceNormal = Vector3.zero;
		if (_hitTriangleIdx < 0)
		{
			return BlockFace.None;
		}
		MeshCollider meshCollider = _hitCollider as MeshCollider;
		if (meshCollider != null && meshCollider.sharedMesh != null && meshCollider.sharedMesh.isReadable)
		{
			Mesh sharedMesh = meshCollider.sharedMesh;
			GameUtils.tempVertices.Clear();
			sharedMesh.GetVertices(GameUtils.tempVertices);
			GameUtils.tempTriangles.Clear();
			sharedMesh.GetTriangles(GameUtils.tempTriangles, 0);
			int num = _hitTriangleIdx * 3;
			if (num >= GameUtils.tempTriangles.Count || GameUtils.tempTriangles[num] >= GameUtils.tempVertices.Count)
			{
				return BlockFace.None;
			}
			Vector3 vector = GameUtils.tempVertices[GameUtils.tempTriangles[num]];
			Vector3 vector2 = GameUtils.tempVertices[GameUtils.tempTriangles[num + 1]];
			Vector3 vector3 = GameUtils.tempVertices[GameUtils.tempTriangles[num + 2]];
			_hitFaceNormal = Vector3.Cross(vector - vector2, vector - vector3);
			Vector3 a = (vector + vector2 + vector3) / 3f;
			_hitFaceCenter = a + World.toChunkXyzWorldPos(_blockPos);
			Vector3 b = World.toBlock(_blockPos).ToVector3();
			vector -= b;
			vector2 -= b;
			vector3 -= b;
			if (!_blockValue.Block.isMultiBlock)
			{
				if ((double)vector.x < -0.001)
				{
					vector.x += 16f;
				}
				else if (vector.x > 15f)
				{
					vector.x -= 16f;
				}
				if ((double)vector.y < -0.001)
				{
					vector.y += 16f;
				}
				else if (vector.y > 15f)
				{
					vector.y -= 16f;
				}
				if ((double)vector.z < -0.001)
				{
					vector.z += 16f;
				}
				else if (vector.z > 15f)
				{
					vector.z -= 16f;
				}
				if ((double)vector2.x < -0.001)
				{
					vector2.x += 16f;
				}
				else if (vector2.x > 15f)
				{
					vector2.x -= 16f;
				}
				if ((double)vector2.y < -0.001)
				{
					vector2.y += 16f;
				}
				else if (vector2.y > 15f)
				{
					vector2.y -= 16f;
				}
				if ((double)vector2.z < -0.001)
				{
					vector2.z += 16f;
				}
				else if (vector2.z > 15f)
				{
					vector2.z -= 16f;
				}
				if ((double)vector3.x < -0.001)
				{
					vector3.x += 16f;
				}
				else if (vector3.x > 15f)
				{
					vector3.x -= 16f;
				}
				if ((double)vector3.y < -0.001)
				{
					vector3.y += 16f;
				}
				else if (vector3.y > 15f)
				{
					vector3.y -= 16f;
				}
				if ((double)vector3.z < -0.001)
				{
					vector3.z += 16f;
				}
				else if (vector3.z > 15f)
				{
					vector3.z -= 16f;
				}
			}
			BlockShapeNew blockShapeNew = _blockValue.Block.shape as BlockShapeNew;
			if (blockShapeNew != null)
			{
				Vector3 b2 = Vector3.one * 0.5f;
				Quaternion rotation = Quaternion.Inverse(blockShapeNew.GetRotation(_blockValue));
				vector = rotation * (vector - b2) + b2;
				vector2 = rotation * (vector2 - b2) + b2;
				vector3 = rotation * (vector3 - b2) + b2;
				return blockShapeNew.GetBlockFaceFromColliderTriangle(_blockValue, vector, vector2, vector3);
			}
		}
		return BlockFace.None;
	}

	public static string GetLaunchArgument(string _argumentName)
	{
		if (GameUtils.arguments == null)
		{
			GameUtils.arguments = new CaseInsensitiveStringDictionary<string>();
			string[] commandLineArgs = GameStartupHelper.GetCommandLineArgs();
			for (int i = 0; i < commandLineArgs.Length; i++)
			{
				if (!string.IsNullOrEmpty(commandLineArgs[i]) && commandLineArgs[i][0] == '-')
				{
					int num = commandLineArgs[i].IndexOf('=');
					string key;
					string value;
					if (num >= 0)
					{
						key = commandLineArgs[i].Substring(1, num - 1);
						value = commandLineArgs[i].Substring(num + 1);
					}
					else
					{
						key = commandLineArgs[i].Substring(1);
						value = string.Empty;
					}
					GameUtils.arguments[key] = value;
				}
			}
		}
		if (GameUtils.arguments.ContainsKey(_argumentName))
		{
			return GameUtils.arguments[_argumentName];
		}
		return null;
	}

	public static bool IsBlockOrTerrain(string _tag)
	{
		return _tag == "B_Mesh" || _tag == "T_Mesh" || _tag == "T_Mesh_B" || _tag == "T_Block" || _tag == "T_Deco";
	}

	public static bool IsBlockOrTerrain(Component component)
	{
		return component.CompareTag("B_Mesh") || component.CompareTag("T_Mesh") || component.CompareTag("T_Mesh_B") || component.CompareTag("T_Block") || component.CompareTag("T_Deco");
	}

	public static ulong Vector3iToUInt64(Vector3i _v)
	{
		return (ulong)((long)(_v.x + 32768 & 65535) << 32 | (long)(_v.y + 32768 & 65535) << 16 | ((long)(_v.z + 32768) & 65535L));
	}

	public static Vector3i UInt64ToVector3i(ulong _fullValue)
	{
		return new Vector3i((int)(_fullValue >> 32 & 65535UL) - 32768, (int)(_fullValue >> 16 & 65535UL) - 32768, (int)(_fullValue & 65535UL) - 32768);
	}

	public static char ValidateGameNameInput(string _text, int _charIndex, char _addedChar)
	{
		if (_addedChar >= 'Ā')
		{
			return _addedChar;
		}
		if ((_addedChar >= 'a' && _addedChar <= 'z') || (_addedChar >= 'A' && _addedChar <= 'Z') || (_addedChar >= '0' && _addedChar <= '9'))
		{
			return _addedChar;
		}
		if (_addedChar == '_' || _addedChar == '-')
		{
			return _addedChar;
		}
		if (_charIndex > 0 && (_addedChar == '.' || _addedChar == ' '))
		{
			return _addedChar;
		}
		return '\0';
	}

	public static char ValidateHexInput(string _text, int _charIndex, char _addedChar)
	{
		if ((_addedChar >= 'a' && _addedChar <= 'f') || (_addedChar >= 'A' && _addedChar <= 'F') || (_addedChar >= '0' && _addedChar <= '9'))
		{
			return _addedChar;
		}
		return '\0';
	}

	public static bool ValidateGameName(string _gameName)
	{
		string text = _gameName.Trim();
		if (string.IsNullOrEmpty(text) || text.Length != _gameName.Length)
		{
			return false;
		}
		for (int i = 0; i < _gameName.Length; i++)
		{
			if (GameUtils.ValidateGameNameInput(_gameName, i, _gameName[i]) == '\0')
			{
				return false;
			}
		}
		return !_gameName.EndsWith(".");
	}

	public static PrefabInstance FindPrefabForBlockPos(List<PrefabInstance> prefabs, Vector3i hitPointBlockPos)
	{
		for (int i = 0; i < prefabs.Count; i++)
		{
			if (prefabs[i].boundingBoxPosition.x <= hitPointBlockPos.x && prefabs[i].boundingBoxPosition.x + prefabs[i].boundingBoxSize.x >= hitPointBlockPos.x && prefabs[i].boundingBoxPosition.z <= hitPointBlockPos.z && prefabs[i].boundingBoxPosition.z + prefabs[i].boundingBoxSize.z >= hitPointBlockPos.z)
			{
				return prefabs[i];
			}
		}
		return null;
	}

	public static int FindPaintIdForBlockFace(BlockValue _bv, BlockFace blockFace, out string _name)
	{
		int sideTextureId = _bv.Block.GetSideTextureId(_bv, blockFace);
		for (int i = 0; i < BlockTextureData.list.Length; i++)
		{
			if (BlockTextureData.list[i] != null && (int)BlockTextureData.list[i].TextureID == sideTextureId)
			{
				_name = BlockTextureData.list[i].Name;
				return i;
			}
		}
		_name = string.Empty;
		return 0;
	}

	public static Vector3i Mirror(EnumMirrorAlong _axis, Vector3i _pos, Vector3i _prefabSize)
	{
		if (_axis == EnumMirrorAlong.XAxis)
		{
			return new Vector3i(_prefabSize.x - _pos.x - 1, _pos.y, _pos.z);
		}
		if (_axis != EnumMirrorAlong.YAxis)
		{
			return new Vector3i(_pos.x, _pos.y, _prefabSize.z - _pos.z - 1);
		}
		return new Vector3i(_pos.x, _prefabSize.y - _pos.y - 1, _pos.z);
	}

	public static Vector3 Mirror(EnumMirrorAlong _axis, Vector3 _pos, Vector3i _prefabSize)
	{
		if (_axis == EnumMirrorAlong.XAxis)
		{
			return new Vector3((float)_prefabSize.x - _pos.x, _pos.y, _pos.z);
		}
		if (_axis != EnumMirrorAlong.YAxis)
		{
			return new Vector3(_pos.x, _pos.y, (float)_prefabSize.z - _pos.z);
		}
		return new Vector3(_pos.x, (float)_prefabSize.y - _pos.y, _pos.z);
	}

	public static void TakeScreenShot(GameUtils.EScreenshotMode _screenshotMode, string _overrideScreenshotFilePath = null, float _borderPerc = 0f, bool _b4to3 = false, int _rescaleToW = 0, int _rescaleToH = 0, bool _isSaveTGA = false)
	{
		ThreadManager.StartCoroutine(GameUtils.TakeScreenshotEnum(_screenshotMode, _overrideScreenshotFilePath, _borderPerc, _b4to3, _rescaleToW, _rescaleToH, _isSaveTGA));
	}

	public static IEnumerator TakeScreenshotEnum(GameUtils.EScreenshotMode _screenshotMode, string _overrideScreenshotFilePath = null, float _borderPerc = 0f, bool _b4to3 = false, int _rescaleToW = 0, int _rescaleToH = 0, bool _isSaveTGA = false)
	{
		yield return new WaitForEndOfFrame();
		Rect screenshotRect = GameUtils.GetScreenshotRect(_borderPerc, _b4to3);
		Texture2D texture2D = new Texture2D((int)screenshotRect.width, (int)screenshotRect.height, TextureFormat.RGB24, false);
		texture2D.ReadPixels(screenshotRect, 0, 0);
		if (_rescaleToW != 0 && _rescaleToH != 0)
		{
			TextureScale.Bilinear(texture2D, _rescaleToW, _rescaleToH);
		}
		texture2D.Apply();
		if (_screenshotMode != GameUtils.EScreenshotMode.File)
		{
			TextureUtils.CopyToClipboard(texture2D);
		}
		if (_screenshotMode != GameUtils.EScreenshotMode.Clipboard)
		{
			string text3;
			if (_overrideScreenshotFilePath == null)
			{
				string text = GameIO.GetUserGameDataDir() + "/Screenshots";
				if (!SdDirectory.Exists(text))
				{
					SdDirectory.CreateDirectory(text);
				}
				string text2 = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
				text3 = string.Concat(new string[]
				{
					text,
					"/",
					Constants.cVersionInformation.ShortString,
					"_",
					text2
				});
			}
			else
			{
				text3 = _overrideScreenshotFilePath;
			}
			text3 += (_isSaveTGA ? ".tga" : ".jpg");
			GameUtils.lastSavedScreenshotFilename = text3;
			if (_isSaveTGA)
			{
				SdFile.WriteAllBytes(text3, texture2D.EncodeToTGA());
			}
			else
			{
				SdFile.WriteAllBytes(text3, texture2D.EncodeToJPG());
			}
		}
		UnityEngine.Object.Destroy(texture2D);
		yield break;
	}

	public static Rect GetScreenshotRect(float _borderPerc = 0f, bool _b4to3 = false)
	{
		float num = 0f;
		float num2 = 0f;
		float num3 = (float)Screen.width;
		float num4 = (float)Screen.height;
		if (_borderPerc > 0.001f)
		{
			float num5 = (float)Screen.width * _borderPerc;
			float num6 = (float)Screen.height * _borderPerc;
			num += num5;
			num3 -= num5 * 2f;
			num2 += num6;
			num4 -= num6 * 2f;
		}
		if (_b4to3)
		{
			num3 = 1.33333337f * num4;
			num = ((float)Screen.width - num3) / 2f;
		}
		return new Rect(num, num2, num3, num4);
	}

	public static void StartPlaytesting()
	{
		if (string.IsNullOrEmpty(GamePrefs.GetString(EnumGamePrefs.LastLoadedPrefab)))
		{
			return;
		}
		GameManager.bHideMainMenuNextTime = true;
		GameManager.Instance.Disconnect();
		ThreadManager.StartCoroutine(GameUtils.startPlaytestLater());
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static IEnumerator startPlaytestLater()
	{
		yield return new WaitForSeconds(2f);
		string @string = GamePrefs.GetString(EnumGamePrefs.LastLoadedPrefab);
		GamePrefs.Set(EnumGamePrefs.GameWorld, "Playtesting");
		GamePrefs.Set(EnumGamePrefs.GameMode, EnumGameMode.Survival.ToStringCached<EnumGameMode>());
		GamePrefs.Set(EnumGamePrefs.GameName, @string);
		string saveGameDir = GameIO.GetSaveGameDir();
		if (SdDirectory.Exists(saveGameDir))
		{
			SdDirectory.Delete(saveGameDir, true);
		}
		SingletonMonoBehaviour<ConnectionManager>.Instance.StartServers(GamePrefs.GetString(EnumGamePrefs.ServerPassword), false);
		yield break;
	}

	public static bool IsPlaytesting()
	{
		return GamePrefs.GetString(EnumGamePrefs.GameWorld) == "Playtesting";
	}

	public static bool IsWorldEditor()
	{
		return GameModeEditWorld.TypeName.Equals(GamePrefs.GetString(EnumGamePrefs.GameMode)) && GamePrefs.GetString(EnumGamePrefs.GameName) == "WorldEditor";
	}

	public static void StartSinglePrefabEditing()
	{
		GameManager.bHideMainMenuNextTime = true;
		GameManager.Instance.Disconnect();
		ThreadManager.StartCoroutine(GameUtils.startSinglePrefabEditingLater());
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static IEnumerator startSinglePrefabEditingLater()
	{
		yield return new WaitForSeconds(2f);
		GamePrefs.Set(EnumGamePrefs.GameWorld, "Empty");
		GamePrefs.Set(EnumGamePrefs.GameMode, GameModeEditWorld.TypeName);
		GamePrefs.Set(EnumGamePrefs.GameName, "PrefabEditor");
		SingletonMonoBehaviour<ConnectionManager>.Instance.StartServers(GamePrefs.GetString(EnumGamePrefs.ServerPassword), false);
		yield break;
	}

	public static float GetOreNoiseAt(TS_PerlinNoise _noise, int _x, int _y, int _z)
	{
		return (_noise.noise((float)_x * 0.05f, (float)_y * 0.05f, (float)_z * 0.05f) - 0.333f) * 3f;
	}

	public static bool CheckOreNoiseAt(TS_PerlinNoise _noise, int _x, int _y, int _z)
	{
		return GameUtils.GetOreNoiseAt(_noise, _x, _y, _z) > 0f;
	}

	public static Color32 UIntToColor(uint color, bool _includeAlpha = false)
	{
		if (_includeAlpha)
		{
			return new Color32((byte)(color >> 16), (byte)(color >> 8), (byte)color, (byte)(color >> 24));
		}
		return new Color32((byte)(color >> 16), (byte)(color >> 8), (byte)color, byte.MaxValue);
	}

	public static uint ColorToUInt(Color32 color, bool _includeAlpha = false)
	{
		if (_includeAlpha)
		{
			return (uint)((int)color.r << 24 | (int)color.r << 16 | (int)color.g << 8 | (int)color.b);
		}
		return (uint)((int)color.r << 16 | (int)color.g << 8 | (int)color.b);
	}

	public static void WaterFloodFill(GridCompressedData<byte> _cols, byte[] _waterChunks16x16Height, int _width, HeightMap _heightMap, int _posX, int _maxY, int _posZ, byte _colWater, byte _colBorder, List<Vector2i> _listPos, int _minX = -2147483648, int _maxX = 2147483647, int _minZ = -2147483648, int _maxZ = 2147483647, int _worldScale = 1)
	{
		int num = _heightMap.GetHeight() * _worldScale;
		do
		{
			int num2 = _posX + _width / 2;
			int num3 = _posZ + num / 2;
			if (_heightMap.GetAt(num2, num3) < (float)(_maxY + 1))
			{
				_cols.SetValue(num2, num3, _colWater);
				_waterChunks16x16Height[num2 / 16 + num3 / 16 * _width / 16] = (byte)_maxY;
				Vector2i vector2i;
				if (num2 < _width - 1 && _posX < _maxX && _cols.GetValue(num2 + 1, num3) == 0 && _listPos.Count < 100000)
				{
					vector2i.x = _posX + 1;
					vector2i.y = _posZ;
					_listPos.Add(vector2i);
				}
				if (num2 > 0 && _posX > _minX && _cols.GetValue(num2 - 1, num3) == 0 && _listPos.Count < 100000)
				{
					vector2i.x = _posX - 1;
					vector2i.y = _posZ;
					_listPos.Add(vector2i);
				}
				if (num3 > 0 && _posZ > _minZ && _cols.GetValue(num2, num3 - 1) == 0 && _listPos.Count < 100000)
				{
					vector2i.x = _posX;
					vector2i.y = _posZ - 1;
					_listPos.Add(vector2i);
				}
				if (num3 < num - 1 && _posZ < _maxZ && _cols.GetValue(num2, num3 + 1) == 0 && _listPos.Count < 100000)
				{
					vector2i.x = _posX;
					vector2i.y = _posZ + 1;
					_listPos.Add(vector2i);
				}
			}
			else
			{
				_cols.SetValue(num2, num3, _colBorder);
			}
			int count = _listPos.Count;
			if (count > 0)
			{
				Vector2i vector2i = _listPos[count - 1];
				_posX = vector2i.x;
				_posZ = vector2i.y;
				_listPos.RemoveAt(count - 1);
			}
		}
		while (_listPos.Count > 0);
	}

	public static GameUtils.EPlayerHomeType CheckForAnyPlayerHome(World world, Vector3i BoxMin, Vector3i BoxMax)
	{
		double num = (double)GameStats.GetInt(EnumGameStats.LandClaimExpiryTime) * 24.0;
		double num2 = (double)GameStats.GetInt(EnumGameStats.BedrollExpiryTime) * 24.0;
		int @int = GamePrefs.GetInt(EnumGamePrefs.BedrollDeadZoneSize);
		Vector3i other = new Vector3i(@int, @int, @int);
		Vector3i vector3i = BoxMin - other;
		Vector3i vector3i2 = BoxMax + other;
		int int2 = GamePrefs.GetInt(EnumGamePrefs.LandClaimSize);
		int num3 = int2 / 2;
		foreach (KeyValuePair<PlatformUserIdentifierAbs, PersistentPlayerData> keyValuePair in GameManager.Instance.GetPersistentPlayerList().Players)
		{
			if (keyValuePair.Value.OfflineHours < num2 && keyValuePair.Value.HasBedrollPos)
			{
				Vector3i bedrollPos = keyValuePair.Value.BedrollPos;
				if (bedrollPos.x >= vector3i.x && bedrollPos.x < vector3i2.x && bedrollPos.z >= vector3i.z && bedrollPos.z < vector3i2.z)
				{
					return GameUtils.EPlayerHomeType.Bedroll;
				}
			}
			List<Vector3i> lpblocks = keyValuePair.Value.LPBlocks;
			if (keyValuePair.Value.OfflineHours < num && lpblocks != null && lpblocks.Count > 0)
			{
				for (int i = 0; i < lpblocks.Count; i++)
				{
					Vector3i vector3i3 = lpblocks[i];
					vector3i3.x -= num3;
					vector3i3.z -= num3;
					if (vector3i3.x <= BoxMax.x && vector3i3.x + int2 >= BoxMin.x && vector3i3.z <= BoxMax.z && vector3i3.z + int2 >= BoxMin.z)
					{
						return GameUtils.EPlayerHomeType.Landclaim;
					}
				}
			}
		}
		return GameUtils.EPlayerHomeType.None;
	}

	public static Transform FindDeepChild(Transform _parent, string _transformName)
	{
		Transform transform = _parent.Find(_transformName);
		if (transform != null)
		{
			return transform;
		}
		int childCount = _parent.childCount;
		for (int i = 0; i < childCount; i++)
		{
			transform = GameUtils.FindDeepChild(_parent.GetChild(i), _transformName);
			if (transform != null)
			{
				return transform;
			}
		}
		return transform;
	}

	public static Transform FindDeepChildActive(Transform _parent, string _transformName)
	{
		Transform transform = _parent.Find(_transformName);
		if (transform != null && transform.gameObject.activeSelf)
		{
			return transform;
		}
		int childCount = _parent.childCount;
		for (int i = 0; i < childCount; i++)
		{
			Transform child = _parent.GetChild(i);
			if (child.gameObject.activeSelf)
			{
				transform = GameUtils.FindDeepChildActive(child, _transformName);
				if (transform != null)
				{
					return transform;
				}
			}
		}
		return transform;
	}

	public static int GetViewDistance()
	{
		if (GamePrefs.GetString(EnumGamePrefs.GameWorld) == "Empty")
		{
			return 12;
		}
		return GameStats.GetInt(EnumGameStats.AllowedViewDistance);
	}

	public static Vector3 GetUpdatedNormalAtPosition(Vector3i _worldPos, int _clrIdx, bool _saveNrmToChunk = false)
	{
		int terrainHeight = (int)GameManager.Instance.World.GetTerrainHeight(_worldPos.x, _worldPos.z);
		int terrainHeight2 = (int)GameManager.Instance.World.GetTerrainHeight(_worldPos.x + 1, _worldPos.z);
		float terrainHeight3 = (float)GameManager.Instance.World.GetTerrainHeight(_worldPos.x, _worldPos.z + 1);
		float num = (float)GameManager.Instance.World.GetDensity(_clrIdx, _worldPos.x, terrainHeight, _worldPos.z) / -128f;
		float num2 = (float)GameManager.Instance.World.GetDensity(_clrIdx, _worldPos.x + 1, terrainHeight, _worldPos.z) / -128f;
		float num3 = (float)GameManager.Instance.World.GetDensity(_clrIdx, _worldPos.x, terrainHeight, _worldPos.z + 1) / -128f;
		float num4 = (float)GameManager.Instance.World.GetDensity(_clrIdx, _worldPos.x, terrainHeight + 1, _worldPos.z) / -128f;
		float num5 = (float)GameManager.Instance.World.GetDensity(_clrIdx, _worldPos.x + 1, terrainHeight + 1, _worldPos.z) / -128f;
		float num6 = (float)GameManager.Instance.World.GetDensity(_clrIdx, _worldPos.x, terrainHeight + 1, _worldPos.z + 1) / -128f;
		if (num > 0.999f && num4 > 0.999f)
		{
			num = 0.5f;
		}
		if (num2 > 0.999f && num5 > 0.999f)
		{
			num2 = 0.5f;
		}
		if (num3 > 0.999f && num6 > 0.999f)
		{
			num3 = 0.5f;
		}
		float y = (float)terrainHeight + num;
		float y2 = (float)terrainHeight2 + num2;
		float y3 = terrainHeight3 + num3;
		Vector3 lhs = new Vector3(0f, y3, 1f) - new Vector3(0f, y, 0f);
		Vector3 rhs = new Vector3(1f, y2, 0f) - new Vector3(0f, y, 0f);
		return Vector3.Cross(lhs, rhs).normalized;
	}

	public static GameUtils.DirEightWay GetDirByNormal(Vector2 _normal)
	{
		_normal.Normalize();
		return GameUtils.GetDirByNormal(new Vector2i(Mathf.RoundToInt(_normal.x), Mathf.RoundToInt(_normal.y)));
	}

	public static GameUtils.DirEightWay GetDirByNormal(Vector2i _normal)
	{
		for (int i = 0; i < GameUtils.NeighborsEightWay.Count; i++)
		{
			if (GameUtils.NeighborsEightWay[i] == _normal)
			{
				return (GameUtils.DirEightWay)i;
			}
		}
		return GameUtils.DirEightWay.None;
	}

	public static GameUtils.DirEightWay GetClosestDirection(float _rotation, bool _limitTo90Degress = false)
	{
		_rotation = MathUtils.Mod(_rotation, 360f);
		if (_limitTo90Degress)
		{
			if (_rotation > 315f || _rotation <= 45f)
			{
				return GameUtils.DirEightWay.N;
			}
			if (_rotation <= 135f)
			{
				return GameUtils.DirEightWay.E;
			}
			if (_rotation <= 225f)
			{
				return GameUtils.DirEightWay.S;
			}
			return GameUtils.DirEightWay.W;
		}
		else
		{
			if ((double)_rotation > 337.5 || (double)_rotation <= 22.5)
			{
				return GameUtils.DirEightWay.N;
			}
			if ((double)_rotation <= 67.5)
			{
				return GameUtils.DirEightWay.NE;
			}
			if ((double)_rotation <= 112.5)
			{
				return GameUtils.DirEightWay.E;
			}
			if ((double)_rotation <= 157.5)
			{
				return GameUtils.DirEightWay.SE;
			}
			if ((double)_rotation <= 202.5)
			{
				return GameUtils.DirEightWay.S;
			}
			if ((double)_rotation <= 247.5)
			{
				return GameUtils.DirEightWay.SW;
			}
			if ((double)_rotation <= 292.5)
			{
				return GameUtils.DirEightWay.W;
			}
			return GameUtils.DirEightWay.NW;
		}
	}

	public static void DestroyAllChildrenBut(Transform t, string _excluded)
	{
		bool isPlaying = Application.isPlaying;
		int num = 0;
		List<string> list = new List<string>(_excluded.Split(',', StringSplitOptions.None));
		while (t.childCount != num)
		{
			Transform child = t.GetChild(num);
			if (list.Contains(child.name))
			{
				num++;
			}
			else if (isPlaying)
			{
				child.parent = null;
				UnityEngine.Object.Destroy(child.gameObject);
			}
			else
			{
				UnityEngine.Object.DestroyImmediate(child.gameObject);
			}
		}
	}

	public static void DestroyAllChildrenBut(Transform t, List<string> _excluded)
	{
		bool isPlaying = Application.isPlaying;
		for (int i = t.childCount - 1; i >= 0; i--)
		{
			Transform child = t.GetChild(i);
			if (!_excluded.Contains(child.name))
			{
				if (isPlaying)
				{
					child.SetParent(null, false);
					UnityEngine.Object.Destroy(child.gameObject);
				}
				else
				{
					UnityEngine.Object.DestroyImmediate(child.gameObject);
				}
			}
		}
	}

	public static void SetMeshVertexAttributes(Mesh mesh, bool compressPosition = true)
	{
		if (!GameUtils.applyVertCompression)
		{
			return;
		}
		VertexAttributeDescriptor[] vertexAttributes = mesh.GetVertexAttributes();
		for (int i = 0; i < vertexAttributes.Length; i++)
		{
			VertexAttributeDescriptor vertexAttributeDescriptor = vertexAttributes[i];
			VertexAttribute attribute = vertexAttributeDescriptor.attribute;
			if (attribute == VertexAttribute.Position && !compressPosition)
			{
				vertexAttributeDescriptor.format = VertexAttributeFormat.Float32;
				vertexAttributeDescriptor.dimension = 3;
			}
			else if (attribute == VertexAttribute.Position || attribute == VertexAttribute.Normal)
			{
				vertexAttributeDescriptor.format = VertexAttributeFormat.Float16;
				vertexAttributeDescriptor.dimension = 4;
			}
			else if (attribute == VertexAttribute.Color || attribute == VertexAttribute.Tangent)
			{
				vertexAttributeDescriptor.format = VertexAttributeFormat.Float16;
				vertexAttributeDescriptor.dimension = 4;
			}
			else if (attribute == VertexAttribute.TexCoord0 || attribute == VertexAttribute.TexCoord1 || attribute == VertexAttribute.TexCoord2 || attribute == VertexAttribute.TexCoord3)
			{
				vertexAttributeDescriptor.format = VertexAttributeFormat.Float16;
				vertexAttributeDescriptor.dimension = 2;
			}
			vertexAttributes[i] = vertexAttributeDescriptor;
		}
		mesh.SetVertexBufferParams(mesh.vertexCount, vertexAttributes);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static Collider[] overlapBoxHits = new Collider[50];

	[PublicizedFrom(EAccessModifier.Private)]
	public static GameRandom random;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly List<Vector3> tempVertices = new List<Vector3>(16384);

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly List<int> tempTriangles = new List<int>(16384);

	[PublicizedFrom(EAccessModifier.Private)]
	public static Dictionary<string, string> arguments;

	public static string lastSavedScreenshotFilename;

	public static List<Vector2i> NeighborsEightWay = new List<Vector2i>
	{
		new Vector2i(0, 1),
		new Vector2i(1, 1),
		new Vector2i(1, 0),
		new Vector2i(1, -1),
		new Vector2i(0, -1),
		new Vector2i(-1, -1),
		new Vector2i(-1, 0),
		new Vector2i(-1, 1)
	};

	public static bool applyVertCompression = true;

	public class WorldInfo
	{
		public Vector2i WorldSize
		{
			get
			{
				return this.HeightmapSize * this.Scale;
			}
		}

		public WorldInfo(string _name, string _description, string[] _modes, Vector2i _heightmapSize, int _scale, bool _fixedWaterLevel, bool _randomGeneratedWorld, VersionInformation _gameVersionCreated, DynamicProperties _dynamicProperties = null)
		{
			this.Valid = true;
			this.Name = _name;
			this.Description = _description;
			this.Modes = _modes;
			this.HeightmapSize = _heightmapSize;
			this.Scale = _scale;
			this.FixedWaterLevel = _fixedWaterLevel;
			this.RandomGeneratedWorld = _randomGeneratedWorld;
			this.GameVersionCreated = _gameVersionCreated;
			this.DynamicProperties = _dynamicProperties;
		}

		public void Save(PathAbstractions.AbstractedLocation _worldLocation)
		{
			if (_worldLocation.Type == PathAbstractions.EAbstractedLocationType.None)
			{
				Log.Warning("No world location given");
				return;
			}
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.CreateXmlDeclaration();
			XmlElement node = xmlDocument.AddXmlElement("MapInfo");
			node.AddXmlElement("property").SetAttrib("name", "Name").SetAttrib("value", this.Name);
			node.AddXmlElement("property").SetAttrib("name", "Modes").SetAttrib("value", string.Join(",", this.Modes));
			node.AddXmlElement("property").SetAttrib("name", "Description").SetAttrib("value", this.Description);
			node.AddXmlElement("property").SetAttrib("name", "Scale").SetAttrib("value", this.Scale.ToString());
			node.AddXmlElement("property").SetAttrib("name", "HeightMapSize").SetAttrib("value", this.HeightmapSize.ToString());
			node.AddXmlElement("property").SetAttrib("name", "FixedWaterLevel").SetAttrib("value", this.FixedWaterLevel.ToString());
			node.AddXmlElement("property").SetAttrib("name", "RandomGeneratedWorld").SetAttrib("value", this.RandomGeneratedWorld.ToString());
			node.AddXmlElement("property").SetAttrib("name", "GameVersion").SetAttrib("value", this.GameVersionCreated.SerializableString);
			xmlDocument.SdSave(_worldLocation.FullPath + "/map_info.xml");
		}

		public static GameUtils.WorldInfo LoadWorldInfo(PathAbstractions.AbstractedLocation _worldLocation)
		{
			if (_worldLocation.Type == PathAbstractions.EAbstractedLocationType.None)
			{
				return null;
			}
			string text = _worldLocation.FullPath + "/map_info.xml";
			if (!SdFile.Exists(text))
			{
				return null;
			}
			IEnumerable<XElement> enumerable = from s in SdXDocument.Load(text).Elements("MapInfo")
			from p in s.Elements("property")
			select p;
			DynamicProperties dynamicProperties = new DynamicProperties();
			foreach (XElement propertyNode in enumerable)
			{
				dynamicProperties.Add(propertyNode, true);
			}
			string name = null;
			string description = null;
			string[] modes = null;
			int x = 4096;
			int y = 4096;
			int scale = 1;
			bool fixedWaterLevel = false;
			bool randomGeneratedWorld = false;
			VersionInformation gameVersionCreated = new VersionInformation(VersionInformation.EGameReleaseType.Alpha, -1, -1, -1);
			if (dynamicProperties.Values.ContainsKey("Name"))
			{
				name = dynamicProperties.Values["Name"];
			}
			if (dynamicProperties.Values.ContainsKey("Modes"))
			{
				modes = dynamicProperties.Values["Modes"].Replace(" ", "").Split(',', StringSplitOptions.None);
			}
			if (dynamicProperties.Values.ContainsKey("Description"))
			{
				description = Localization.Get(dynamicProperties.Values["Description"], false);
			}
			if (dynamicProperties.Values.ContainsKey("Scale"))
			{
				scale = int.Parse(dynamicProperties.Values["Scale"]);
			}
			if (dynamicProperties.Values.ContainsKey("HeightMapSize"))
			{
				Vector2i vector2i = StringParsers.ParseVector2i(dynamicProperties.Values["HeightMapSize"], ',');
				x = vector2i.x;
				y = vector2i.y;
			}
			if (dynamicProperties.Values.ContainsKey("FixedWaterLevel"))
			{
				fixedWaterLevel = StringParsers.ParseBool(dynamicProperties.Values["FixedWaterLevel"], 0, -1, true);
			}
			if (dynamicProperties.Values.ContainsKey("RandomGeneratedWorld"))
			{
				randomGeneratedWorld = StringParsers.ParseBool(dynamicProperties.Values["RandomGeneratedWorld"], 0, -1, true);
			}
			if (dynamicProperties.Values.ContainsKey("GameVersion") && !VersionInformation.TryParseSerializedString(dynamicProperties.Values["GameVersion"], out gameVersionCreated))
			{
				gameVersionCreated = new VersionInformation(VersionInformation.EGameReleaseType.Alpha, -1, -1, -1);
				Log.Warning("World '" + _worldLocation.Name + "' has an invalid GameVersion value: " + dynamicProperties.Values["GameVersion"]);
			}
			return new GameUtils.WorldInfo(name, description, modes, new Vector2i(x, y), scale, fixedWaterLevel, randomGeneratedWorld, gameVersionCreated, dynamicProperties);
		}

		public readonly bool Valid;

		public readonly string Name;

		public readonly string Description;

		public readonly string[] Modes;

		public readonly Vector2i HeightmapSize;

		public readonly int Scale;

		public readonly bool FixedWaterLevel;

		public readonly bool RandomGeneratedWorld;

		public readonly VersionInformation GameVersionCreated;

		public readonly DynamicProperties DynamicProperties;
	}

	public delegate void OutputDelegate(string _text);

	public enum EKickReason
	{
		EmptyNameOrPlayerID,
		InvalidUserId,
		DuplicatePlayerID,
		InvalidAuthTicket,
		VersionMismatch,
		PlayerLimitExceeded,
		Banned,
		NotOnWhitelist,
		PlatformAuthenticationBeginFailed,
		PlatformAuthenticationFailed,
		ManualKick,
		EacViolation,
		EacBan,
		PlayerLimitExceededNonVIP,
		GameStillLoading,
		GamePaused,
		ModDecision,
		FriendsOnly,
		UnknownNetPackage,
		EncryptionFailure,
		UnsupportedPlatform,
		CrossPlatformAuthenticationBeginFailed,
		CrossPlatformAuthenticationFailed,
		WrongCrossPlatform,
		EosEacViolation,
		MultiplayerBlockedForHostAccount,
		BadMTUPackets,
		CrossplayDisabled,
		InternalNetConnectionError,
		InviteOnly,
		SessionClosed,
		PersistentPlayerDataExceeded,
		PlatformPlayerLimitExceeded
	}

	public struct KickPlayerData
	{
		public KickPlayerData(GameUtils.EKickReason _kickReason, int _apiResponseEnum = 0, DateTime _banUntil = default(DateTime), string _customReason = "")
		{
			this.reason = _kickReason;
			this.apiResponseEnum = _apiResponseEnum;
			this.banUntil = _banUntil;
			this.customReason = (_customReason ?? string.Empty);
		}

		public string LocalizedMessage()
		{
			switch (this.reason)
			{
			case GameUtils.EKickReason.EmptyNameOrPlayerID:
			case GameUtils.EKickReason.InvalidUserId:
			case GameUtils.EKickReason.DuplicatePlayerID:
			case GameUtils.EKickReason.InvalidAuthTicket:
			case GameUtils.EKickReason.NotOnWhitelist:
			case GameUtils.EKickReason.PersistentPlayerDataExceeded:
				return Localization.Get("auth_" + this.reason.ToStringCached<GameUtils.EKickReason>(), false);
			case GameUtils.EKickReason.VersionMismatch:
				return string.Format(Localization.Get("auth_VersionMismatch", false), Constants.cVersionInformation.LongStringNoBuild, this.customReason);
			case GameUtils.EKickReason.PlayerLimitExceeded:
				return string.Format(Localization.Get("auth_PlayerLimitExceeded", false), this.customReason);
			case GameUtils.EKickReason.Banned:
				return string.Format(Localization.Get("auth_Banned", false), this.banUntil.ToCultureInvariantString()) + (string.IsNullOrEmpty(this.customReason) ? string.Empty : ("\n" + string.Format(Localization.Get("auth_reason", false), this.customReason)));
			case GameUtils.EKickReason.PlatformAuthenticationBeginFailed:
			{
				EBeginUserAuthenticationResult ebeginUserAuthenticationResult = (EBeginUserAuthenticationResult)this.apiResponseEnum;
				if (ebeginUserAuthenticationResult - EBeginUserAuthenticationResult.InvalidTicket <= 4)
				{
					return string.Format(Localization.Get("platformauth_" + ebeginUserAuthenticationResult.ToStringCached<EBeginUserAuthenticationResult>(), false), PlatformManager.NativePlatform.PlatformDisplayName);
				}
				return string.Format(Localization.Get("platformauth_unknown", false), PlatformManager.NativePlatform.PlatformDisplayName);
			}
			case GameUtils.EKickReason.PlatformAuthenticationFailed:
			{
				EUserAuthenticationResult euserAuthenticationResult = (EUserAuthenticationResult)this.apiResponseEnum;
				if (euserAuthenticationResult - EUserAuthenticationResult.UserNotConnectedToPlatform <= 7)
				{
					return string.Format(Localization.Get("platformauth_" + euserAuthenticationResult.ToStringCached<EUserAuthenticationResult>(), false), PlatformManager.NativePlatform.PlatformDisplayName);
				}
				if (euserAuthenticationResult != EUserAuthenticationResult.PublisherIssuedBan)
				{
					return string.Format(Localization.Get("platformauth_unknown", false), PlatformManager.NativePlatform.PlatformDisplayName);
				}
				if (this.banUntil == default(DateTime))
				{
					return string.Format(Localization.Get("platformauth_" + euserAuthenticationResult.ToStringCached<EUserAuthenticationResult>(), false), PlatformManager.NativePlatform.PlatformDisplayName) + (string.IsNullOrEmpty(this.customReason) ? string.Empty : ("\n" + string.Format(Localization.Get("auth_reason", false), Array.Empty<object>())));
				}
				return string.Format("\n" + Localization.Get("auth_Banned", false), this.banUntil.ToCultureInvariantString()) + (string.IsNullOrEmpty(this.customReason) ? string.Empty : ("\n" + string.Format(Localization.Get("auth_reason", false), this.customReason)));
			}
			case GameUtils.EKickReason.ManualKick:
				return string.Format(Localization.Get("auth_ManualKick", false), Array.Empty<object>()) + (string.IsNullOrEmpty(this.customReason) ? string.Empty : ("\n" + string.Format(Localization.Get("auth_reason", false), this.customReason)));
			case GameUtils.EKickReason.PlayerLimitExceededNonVIP:
				return string.Format(Localization.Get("auth_PlayerLimitExceededNonVIP", false), this.customReason);
			case GameUtils.EKickReason.GameStillLoading:
				return Localization.Get("auth_stillloading", false);
			case GameUtils.EKickReason.GamePaused:
				return Localization.Get("auth_gamepaused", false);
			case GameUtils.EKickReason.ModDecision:
			{
				string text = Localization.Get("auth_mod", false);
				if (!string.IsNullOrEmpty(this.customReason))
				{
					text = text + "\n" + this.customReason;
				}
				return text;
			}
			case GameUtils.EKickReason.FriendsOnly:
				return Localization.Get("auth_friendsonly", false);
			case GameUtils.EKickReason.UnknownNetPackage:
				return Localization.Get("auth_unknownnetpackage", false);
			case GameUtils.EKickReason.EncryptionFailure:
				return Localization.Get("auth_encryptionfailure", false);
			case GameUtils.EKickReason.UnsupportedPlatform:
				return string.Format(Localization.Get("auth_unsupportedplatform", false), Localization.Get("platformName" + this.customReason, false));
			case GameUtils.EKickReason.CrossPlatformAuthenticationBeginFailed:
			{
				EBeginUserAuthenticationResult ebeginUserAuthenticationResult2 = (EBeginUserAuthenticationResult)this.apiResponseEnum;
				if (ebeginUserAuthenticationResult2 - EBeginUserAuthenticationResult.InvalidTicket <= 4)
				{
					return string.Format(Localization.Get("platformauth_" + ebeginUserAuthenticationResult2.ToStringCached<EBeginUserAuthenticationResult>(), false), PlatformManager.CrossplatformPlatform.PlatformDisplayName);
				}
				return string.Format(Localization.Get("platformauth_unknown", false), PlatformManager.CrossplatformPlatform.PlatformDisplayName);
			}
			case GameUtils.EKickReason.CrossPlatformAuthenticationFailed:
			{
				EUserAuthenticationResult euserAuthenticationResult2 = (EUserAuthenticationResult)this.apiResponseEnum;
				if (euserAuthenticationResult2 - EUserAuthenticationResult.UserNotConnectedToPlatform <= 8)
				{
					return string.Format(Localization.Get("platformauth_" + euserAuthenticationResult2.ToStringCached<EUserAuthenticationResult>(), false), PlatformManager.CrossplatformPlatform.PlatformDisplayName);
				}
				if (euserAuthenticationResult2 != EUserAuthenticationResult.EosTicketFailed)
				{
					return string.Format(Localization.Get("platformauth_unknown", false), PlatformManager.CrossplatformPlatform.PlatformDisplayName);
				}
				return string.Format(Localization.Get("platformauth_" + euserAuthenticationResult2.ToStringCached<EUserAuthenticationResult>(), false), PlatformManager.CrossplatformPlatform.PlatformDisplayName, this.customReason);
			}
			case GameUtils.EKickReason.WrongCrossPlatform:
				return string.Format(Localization.Get("auth_wrongcrossplatform", false), Localization.Get("platformName" + this.customReason, false));
			case GameUtils.EKickReason.EosEacViolation:
			{
				AntiCheatCommonClientActionReason antiCheatCommonClientActionReason = (AntiCheatCommonClientActionReason)this.apiResponseEnum;
				if (antiCheatCommonClientActionReason > AntiCheatCommonClientActionReason.PermanentBanned)
				{
					return Localization.Get("eacauth_unknown", false);
				}
				string arg = Localization.Get("eacauth_known_" + ((AntiCheatCommonClientActionReason)this.apiResponseEnum).ToStringCached<AntiCheatCommonClientActionReason>(), false);
				if (string.IsNullOrEmpty(this.customReason))
				{
					return string.Format(Localization.Get("eacauth_known", false), arg);
				}
				return string.Format(Localization.Get("eacauth_known_with_text", false), arg, this.customReason);
			}
			case GameUtils.EKickReason.MultiplayerBlockedForHostAccount:
				return Localization.Get("auth_multiplayerblocked", false);
			case GameUtils.EKickReason.BadMTUPackets:
				return Localization.Get("auth_badPackets", false);
			case GameUtils.EKickReason.CrossplayDisabled:
				return Localization.Get("auth_crossplaydisabled", false);
			case GameUtils.EKickReason.InternalNetConnectionError:
				return Localization.Get("auth_internalnetconnectionerror", false);
			case GameUtils.EKickReason.InviteOnly:
				return Localization.Get("auth_inviteOnly", false);
			case GameUtils.EKickReason.SessionClosed:
				return Localization.Get("auth_sessionClosed", false);
			case GameUtils.EKickReason.PlatformPlayerLimitExceeded:
				return string.Format(Localization.Get("auth_PlatformPlayerLimitExceeded", false), this.customReason);
			}
			return Localization.Get("auth_unknown", false);
		}

		public override string ToString()
		{
			switch (this.reason)
			{
			case GameUtils.EKickReason.EmptyNameOrPlayerID:
				return "Empty name or player ID";
			case GameUtils.EKickReason.InvalidUserId:
				return "Invalid SteamID";
			case GameUtils.EKickReason.DuplicatePlayerID:
				return "Duplicate player ID";
			case GameUtils.EKickReason.InvalidAuthTicket:
				return "Invalid authentication ticket";
			case GameUtils.EKickReason.VersionMismatch:
				return "Version mismatch";
			case GameUtils.EKickReason.PlayerLimitExceeded:
				return "Player limit exceeded";
			case GameUtils.EKickReason.Banned:
				return "Banned until: " + this.banUntil.ToCultureInvariantString() + (string.IsNullOrEmpty(this.customReason) ? "" : (", reason: " + this.customReason));
			case GameUtils.EKickReason.NotOnWhitelist:
				return "Not on whitelist";
			case GameUtils.EKickReason.PlatformAuthenticationBeginFailed:
				return "Platform auth failed: " + ((EBeginUserAuthenticationResult)this.apiResponseEnum).ToStringCached<EBeginUserAuthenticationResult>();
			case GameUtils.EKickReason.PlatformAuthenticationFailed:
				return "Platform auth failed: " + ((EUserAuthenticationResult)this.apiResponseEnum).ToStringCached<EUserAuthenticationResult>();
			case GameUtils.EKickReason.ManualKick:
				return "Kick: " + ((this.customReason != null) ? this.customReason : "no reason given");
			case GameUtils.EKickReason.PlayerLimitExceededNonVIP:
				return "Player limit for non VIPs / unreserved slots exceeded";
			case GameUtils.EKickReason.GameStillLoading:
				return "Server is still initializing";
			case GameUtils.EKickReason.GamePaused:
				return "Server is paused";
			case GameUtils.EKickReason.ModDecision:
				return "Denied by mod";
			case GameUtils.EKickReason.FriendsOnly:
				return "Friends Only host";
			case GameUtils.EKickReason.UnknownNetPackage:
				return "Unknown NetPackage";
			case GameUtils.EKickReason.EncryptionFailure:
				return "Encryption failure";
			case GameUtils.EKickReason.UnsupportedPlatform:
				return "Unsupported client platform: " + this.customReason;
			case GameUtils.EKickReason.CrossPlatformAuthenticationBeginFailed:
				return "Cross platform auth failed: " + ((EBeginUserAuthenticationResult)this.apiResponseEnum).ToStringCached<EBeginUserAuthenticationResult>() + (string.IsNullOrEmpty(this.customReason) ? "" : (" - " + this.customReason));
			case GameUtils.EKickReason.CrossPlatformAuthenticationFailed:
				return "Cross platform auth failed: " + ((EUserAuthenticationResult)this.apiResponseEnum).ToStringCached<EUserAuthenticationResult>() + (string.IsNullOrEmpty(this.customReason) ? "" : (" - " + this.customReason));
			case GameUtils.EKickReason.WrongCrossPlatform:
				return "Unsupported client cross platform: " + this.customReason;
			case GameUtils.EKickReason.EosEacViolation:
				return "EOS-ACS violation: " + ((AntiCheatCommonClientActionReason)this.apiResponseEnum).ToStringCached<AntiCheatCommonClientActionReason>() + (string.IsNullOrEmpty(this.customReason) ? "" : (" - " + this.customReason));
			case GameUtils.EKickReason.MultiplayerBlockedForHostAccount:
				return "Multiplayer blocked for host's account";
			case GameUtils.EKickReason.CrossplayDisabled:
				return "Crossplay disabled for host's account";
			case GameUtils.EKickReason.InviteOnly:
				return "Invite Only host";
			case GameUtils.EKickReason.SessionClosed:
				return "Session is Closed";
			}
			return "Unknown reason";
		}

		public GameUtils.EKickReason reason;

		public int apiResponseEnum;

		public DateTime banUntil;

		public string customReason;
	}

	public enum EScreenshotMode
	{
		File,
		Clipboard,
		Both
	}

	public enum EPlayerHomeType
	{
		None,
		Landclaim,
		Bedroll
	}

	public enum DirEightWay : sbyte
	{
		None = -1,
		N,
		NE,
		E,
		SE,
		S,
		SW,
		W,
		NW,
		COUNT
	}
}
