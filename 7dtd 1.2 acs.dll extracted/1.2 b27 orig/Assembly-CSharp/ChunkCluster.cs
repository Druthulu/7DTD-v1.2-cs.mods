using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkCluster : WorldChunkCache, IChunkAccess
{
	public event ChunkCluster.OnBlockDamagedDelegate OnBlockDamagedDelegates;

	public event ChunkCluster.OnChunksFinishedLoadingDelegate OnChunksFinishedLoadingDelegates;

	public event ChunkCluster.OnChunksFinishedDisplayingDelegate OnChunksFinishedDisplayingDelegates;

	public event ChunkCluster.OnChunkVisibleDelegate OnChunkVisibleDelegates;

	public event ChunkCluster.OnBlockChangedDelegate OnBlockChangedDelegates;

	public ChunkCluster(World _world, string _name, Dictionary<string, int> _layerMappingTable)
	{
		this.Name = _name;
		this.world = _world;
		this.LayerMappingTable = _layerMappingTable;
	}

	public IEnumerator Init(EnumChunkProviderId _providerId)
	{
		this.nChunks = new ChunkCacheNeighborChunks(this);
		this.nBlocks = new ChunkCacheNeighborBlocks(this.nChunks);
		this.meshGenerator = new MeshGeneratorMC2(this.nBlocks, this.nChunks);
		this.stabilityCalcMainThread = new StabilityCalculator();
		this.stabilityCalcLightingThread = new StabilityInitializer(this.world);
		this.m_LightProcessorMainThread = new LightProcessor(this);
		this.m_LightProcessorLightingThread = new LightProcessor(this);
		if (this.world.GetGameManager() != null)
		{
			this.stabilityCalcMainThread.Init(this.world);
		}
		this.ChunkProvider = null;
		switch (_providerId)
		{
		case EnumChunkProviderId.Disc:
			this.ChunkProvider = new ChunkProviderDisc(this, this.Name);
			break;
		case EnumChunkProviderId.GenerateFromDtm:
			this.ChunkProvider = new ChunkProviderGenerateWorldFromImage(this, this.Name, true);
			break;
		case EnumChunkProviderId.NetworkClient:
			if (!this.IsFixedSize)
			{
				this.ChunkProvider = new ChunkProviderGenerateWorldFromRaw(this, this.Name, true, true);
			}
			else
			{
				this.ChunkProvider = new ChunkProviderDummy();
			}
			break;
		case EnumChunkProviderId.ChunkDataDriven:
			this.ChunkProvider = new ChunkProviderGenerateWorldFromRaw(this, this.Name, false, false);
			break;
		case EnumChunkProviderId.FlatWorld:
			this.ChunkProvider = new ChunkProviderGenerateFlat(this, this.Name);
			break;
		}
		yield return null;
		if (this.ChunkProvider != null)
		{
			yield return this.ChunkProvider.Init(this.world);
		}
		yield break;
	}

	public void Cleanup()
	{
		ChunkManager chunkManager = this.world.m_ChunkManager;
		if (this.ChunkProvider != null)
		{
			this.ChunkProvider.StopUpdate();
		}
		List<Chunk> chunkArrayCopySync = base.GetChunkArrayCopySync();
		for (int i = 0; i < chunkArrayCopySync.Count; i++)
		{
			Chunk chunk = chunkArrayCopySync[i];
			this.RemoveChunk(chunk);
			this.UnloadChunk(chunk);
		}
		chunkManager.ClearChunksForAllObservers(this);
		if (this.ChunkProvider != null)
		{
			this.ChunkProvider.Cleanup();
			this.ChunkProvider = null;
		}
		DictionarySave<long, ChunkGameObject> displayedChunkGameObjects = this.DisplayedChunkGameObjects;
		lock (displayedChunkGameObjects)
		{
			long[] array = new long[this.DisplayedChunkGameObjects.Count];
			this.DisplayedChunkGameObjects.Dict.CopyKeysTo(array);
			foreach (long num in array)
			{
				ChunkGameObject chunkGameObject = this.DisplayedChunkGameObjects[num];
				chunkManager.FreeChunkGameObject(this, num);
			}
			this.DisplayedChunkGameObjects.Clear();
		}
		if (this.stabilityCalcMainThread != null)
		{
			this.stabilityCalcMainThread.Cleanup();
			this.stabilityCalcMainThread = null;
		}
		this.stabilityCalcLightingThread = null;
		this.OnBlockDamagedDelegates = null;
		this.OnChunksFinishedLoadingDelegates = null;
		this.OnChunksFinishedDisplayingDelegates = null;
		this.OnChunkVisibleDelegates = null;
		this.OnBlockChangedDelegates = null;
	}

	public World GetWorld()
	{
		return this.world;
	}

	public List<Vector3i> GetIndexedBlocks(string _name)
	{
		List<Vector3i> list = new List<Vector3i>();
		List<Chunk> chunkArrayCopySync = base.GetChunkArrayCopySync();
		for (int i = 0; i < chunkArrayCopySync.Count; i++)
		{
			Chunk obj = chunkArrayCopySync[i];
			lock (obj)
			{
				if (!chunkArrayCopySync[i].InProgressUnloading)
				{
					List<Vector3i> list2 = chunkArrayCopySync[i].IndexedBlocks[_name];
					if (list2 != null)
					{
						for (int j = 0; j < list2.Count; j++)
						{
							Vector3i pos = list2[j];
							list.Add(chunkArrayCopySync[i].ToWorldPos(pos));
						}
					}
				}
			}
		}
		return list;
	}

	public IChunk GetChunkSync(int chunkX, int chunkY, int chunkZ)
	{
		return base.GetChunkSync(chunkX, chunkZ);
	}

	public IChunk GetChunkFromWorldPos(int x, int y, int z)
	{
		return base.GetChunkSync(World.toChunkXZ(x), World.toChunkXZ(z));
	}

	public IChunk GetChunkFromWorldPos(Vector3i _blockPos)
	{
		return base.GetChunkSync(World.toChunkXZ(_blockPos.x), World.toChunkXZ(_blockPos.z));
	}

	public override bool AddChunkSync(Chunk _chunk, bool _bOmitCallbacks = false)
	{
		bool result = base.AddChunkSync(_chunk, _bOmitCallbacks);
		if (!_bOmitCallbacks && this.IsFixedSize && !this.bFinishedLoadingDelegateCalled)
		{
			if (this.chunkKeysNeedLoading == null)
			{
				this.chunkKeysNeedLoading = new HashSetLong();
				for (int i = this.ChunkMinPos.x; i <= this.ChunkMaxPos.x; i++)
				{
					for (int j = this.ChunkMinPos.y; j <= this.ChunkMaxPos.y; j++)
					{
						long item = WorldChunkCache.MakeChunkKey(i, j);
						this.chunkKeysNeedLoading.Add(item);
					}
				}
			}
			if (this.chunkKeysNeedDisplaying == null)
			{
				this.chunkKeysNeedDisplaying = new HashSetLong();
				for (int k = this.ChunkMinPos.x + 2; k <= this.ChunkMaxPos.x - 2; k++)
				{
					for (int l = this.ChunkMinPos.y + 2; l <= this.ChunkMaxPos.y - 2; l++)
					{
						long item2 = WorldChunkCache.MakeChunkKey(k, l);
						this.chunkKeysNeedDisplaying.Add(item2);
					}
				}
			}
			this.chunkKeysNeedLoading.Remove(_chunk.Key);
			if (this.chunkKeysNeedDisplaying.Count > 0 && _chunk.IsEmpty())
			{
				this.chunkKeysNeedDisplaying.Remove(_chunk.Key);
			}
			if (this.chunkKeysNeedLoading.Count == 0)
			{
				this.NotifyOnChunksFinishedLoading();
			}
		}
		return result;
	}

	public void NotifyOnChunksFinishedLoading()
	{
		if (this.OnChunksFinishedLoadingDelegates != null)
		{
			this.OnChunksFinishedLoadingDelegates();
			this.bFinishedLoadingDelegateCalled = true;
		}
	}

	public void RemoveChunk(Chunk _chunk)
	{
		_chunk.OnUnload(this.world);
		this.RemoveChunkSync(_chunk.Key);
	}

	public void UnloadChunk(Chunk _chunk)
	{
		if (this.ChunkProvider != null)
		{
			_chunk.NeedsRegeneration = true;
			this.ChunkProvider.UnloadChunk(_chunk);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void addDistantDecorationBlocks(Chunk _chunk)
	{
		this.multiBlockList.Clear();
		World world = GameManager.Instance.World;
		DecoManager.Instance.GetDecorationsOnChunk(_chunk.X, _chunk.Z, this.multiBlockList);
		for (int i = this.multiBlockList.Count - 1; i >= 0; i--)
		{
			SBlockPosValue sblockPosValue = this.multiBlockList[i];
			int x = World.toBlockXZ(sblockPosValue.blockPos.x);
			int y = sblockPosValue.blockPos.y;
			int z = World.toBlockXZ(sblockPosValue.blockPos.z);
			if (!_chunk.GetBlock(x, y, z).Block.isMultiBlock)
			{
				Block block = sblockPosValue.blockValue.Block;
				if (block.isMultiBlock)
				{
					for (int j = block.multiBlockPos.Length - 1; j >= 0; j--)
					{
						Vector3i vector3i = block.multiBlockPos.Get(j, sblockPosValue.blockValue.type, (int)sblockPosValue.blockValue.rotation);
						Vector3i vector3i2 = sblockPosValue.blockPos + vector3i;
						BlockValue blockValue = sblockPosValue.blockValue;
						if (vector3i.x != 0 || vector3i.y != 0 || vector3i.z != 0)
						{
							blockValue.ischild = true;
							blockValue.parentx = -vector3i.x;
							blockValue.parenty = -vector3i.y;
							blockValue.parentz = -vector3i.z;
						}
						sbyte density = this.GetDensity(vector3i2);
						this.SetBlockRaw(vector3i2, blockValue);
						this.SetDensityRaw(vector3i2, density);
						this.SetStability(vector3i2, 15);
					}
				}
				else if (world.GetBlock(sblockPosValue.blockPos).isair)
				{
					this.SetBlockRaw(sblockPosValue.blockPos, sblockPosValue.blockValue);
				}
			}
		}
	}

	public void LightChunk(Chunk chunk, Chunk[] _neighbours)
	{
		this.addDistantDecorationBlocks(chunk);
		if (this.m_LightProcessorLightingThread != null)
		{
			this.m_LightProcessorLightingThread.LightChunk(chunk);
			chunk.CheckSameLight();
		}
		this.CalcStability(chunk);
		chunk.CalcBiomeIntensity(_neighbours);
		chunk.CalcDominantBiome();
	}

	public void CalcStability(Chunk chunk)
	{
		if (this.stabilityCalcLightingThread != null)
		{
			this.stabilityCalcLightingThread.DistributeStability(chunk);
			chunk.CheckSameStability();
		}
	}

	public void RegenerateChunk(Chunk _chunk, Chunk[] _neighbours)
	{
		this.nChunks.Init(_chunk, _neighbours);
		while (_chunk.NeedsRegeneration)
		{
			VoxelMeshLayer voxelMeshLayer = null;
			for (int i = 0; i < 16; i++)
			{
				if ((_chunk.NeedsRegenerationAt & 1 << i) != 0)
				{
					if (!this.meshGenerator.IsLayerEmpty(i))
					{
						voxelMeshLayer = MemoryPools.poolVML.AllocSync(true);
						voxelMeshLayer.idx = i;
						voxelMeshLayer.SizeToChunkDefaults();
						break;
					}
					_chunk.ClearNeedsRegenerationAt(i);
				}
			}
			if (voxelMeshLayer == null)
			{
				this.nChunks.Clear();
				return;
			}
			_chunk.ClearNeedsRegenerationAt(voxelMeshLayer.idx);
			this.nChunks.Init(_chunk, _neighbours);
			Vector3i chunkPos = new Vector3i(_chunk.X << 4, _chunk.Y << 8 - voxelMeshLayer.idx * 16, _chunk.Z << 4);
			this.meshGenerator.GenerateMesh(chunkPos, voxelMeshLayer.idx, voxelMeshLayer.meshes);
			_chunk.AddMeshLayer(voxelMeshLayer);
		}
		this.nBlocks.Clear();
		this.nChunks.Clear();
	}

	public bool IsOnBorder(Chunk _c)
	{
		return this.IsFixedSize && (_c.X == this.ChunkMinPos.x || _c.X == this.ChunkMaxPos.x || _c.Z == this.ChunkMinPos.y || _c.Z == this.ChunkMaxPos.y);
	}

	public sbyte GetDensity(Vector3i _worldPos)
	{
		Chunk chunkSync = base.GetChunkSync(World.toChunkXZ(_worldPos.x), World.toChunkXZ(_worldPos.z));
		if (chunkSync == null)
		{
			return MarchingCubes.DensityAir;
		}
		Vector3i vector3i = World.toBlock(_worldPos);
		return chunkSync.GetDensity(vector3i.x, vector3i.y, vector3i.z);
	}

	public void SetDensity(Vector3i _pos, sbyte _density, bool _isForceDensity = false)
	{
		this.SetBlock(_pos, false, BlockValue.Air, true, _density, false, false, _isForceDensity, false, -1);
	}

	public void SetDensityRaw(Vector3i _pos, sbyte _density)
	{
		Chunk chunk = (Chunk)this.GetChunkFromWorldPos(_pos);
		if (chunk == null)
		{
			return;
		}
		int x = World.toBlockXZ(_pos.x);
		int z = World.toBlockXZ(_pos.z);
		int y = World.toBlockY(_pos.y);
		chunk.SetDensity(x, y, z, _density);
	}

	public void SetStability(Vector3i _pos, byte _v)
	{
		Chunk chunk = (Chunk)this.GetChunkFromWorldPos(_pos);
		if (chunk == null)
		{
			return;
		}
		int x = World.toBlockXZ(_pos.x);
		int z = World.toBlockXZ(_pos.z);
		int y = World.toBlockY(_pos.y);
		chunk.SetStability(x, y, z, _v);
	}

	public WaterValue GetWater(Vector3i _pos)
	{
		if (_pos.y < 256)
		{
			IChunk chunkFromWorldPos = this.GetChunkFromWorldPos(_pos);
			if (chunkFromWorldPos != null)
			{
				return chunkFromWorldPos.GetWater(World.toBlockXZ(_pos.x), _pos.y, World.toBlockXZ(_pos.z));
			}
		}
		return WaterValue.Empty;
	}

	public void SetWater(Vector3i _pos, WaterValue _waterData)
	{
		Chunk chunk = (Chunk)this.GetChunkFromWorldPos(_pos);
		if (chunk == null)
		{
			return;
		}
		int num = World.toBlockXZ(_pos.x);
		int num2 = World.toBlockXZ(_pos.z);
		int num3 = World.toBlockY(_pos.y);
		chunk.SetWater(num, num3, num2, _waterData);
		this.chunkPosNeedsRegeneration(chunk, num, num3, num2, false);
	}

	public BlockValue GetBlock(Vector3i _pos)
	{
		if (_pos.y < 256)
		{
			IChunk chunkFromWorldPos = this.GetChunkFromWorldPos(_pos);
			if (chunkFromWorldPos != null)
			{
				return chunkFromWorldPos.GetBlock(World.toBlockXZ(_pos.x), _pos.y, World.toBlockXZ(_pos.z));
			}
		}
		return BlockValue.Air;
	}

	public BlockValue SetBlock(Vector3i _pos, BlockValue _bv, bool _isNotify, bool _isUpdateLight)
	{
		return this.SetBlock(_pos, true, _bv, false, 0, _isNotify, _isUpdateLight, false, false, -1);
	}

	public BlockValue SetBlock(Vector3i _pos, bool _isChangeBV, BlockValue _bv, bool _isChangeDensity, sbyte _density, bool _isNotify, bool _isUpdateLight, bool _isForceDensity = false, bool _wasChild = false, int _changedByEntityId = -1)
	{
		if (_pos.y <= 0 || _pos.y >= 255)
		{
			return BlockValue.Air;
		}
		Block block = _bv.Block;
		if (block == null)
		{
			return BlockValue.Air;
		}
		int num = World.toChunkXZ(_pos.x);
		int num2 = World.toChunkXZ(_pos.z);
		if (this.IsFixedSize && SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			if (num <= this.ChunkMinPos.x + 2)
			{
				int x = this.ChunkMinPos.x;
				this.ChunkMinPos.x = num - 3;
				for (int i = this.ChunkMinPos.x; i < x; i++)
				{
					for (int j = this.ChunkMinPos.y; j <= this.ChunkMaxPos.y; j++)
					{
						this.AddChunkSync(new Chunk(i, j), false);
					}
				}
			}
			if (num >= this.ChunkMaxPos.x - 2)
			{
				int x2 = this.ChunkMaxPos.x;
				this.ChunkMaxPos.x = num + 3;
				for (int k = x2 + 1; k <= this.ChunkMaxPos.x; k++)
				{
					for (int l = this.ChunkMinPos.y; l <= this.ChunkMaxPos.y; l++)
					{
						this.AddChunkSync(new Chunk(k, l), false);
					}
				}
			}
			if (num2 <= this.ChunkMinPos.y + 2)
			{
				int y = this.ChunkMinPos.y;
				this.ChunkMinPos.y = num2 - 3;
				for (int m = this.ChunkMinPos.y; m < y; m++)
				{
					for (int n = this.ChunkMinPos.x; n <= this.ChunkMaxPos.x; n++)
					{
						this.AddChunkSync(new Chunk(n, m), false);
					}
				}
			}
			if (num2 >= this.ChunkMaxPos.y - 2)
			{
				int y2 = this.ChunkMaxPos.y;
				this.ChunkMaxPos.y = num2 + 3;
				for (int num3 = y2 + 1; num3 <= this.ChunkMaxPos.y; num3++)
				{
					for (int num4 = this.ChunkMinPos.x; num4 <= this.ChunkMaxPos.x; num4++)
					{
						this.AddChunkSync(new Chunk(num4, num3), false);
					}
				}
			}
		}
		Chunk chunkSync = base.GetChunkSync(num, num2);
		if (chunkSync == null)
		{
			if (_bv.isair)
			{
				DecoManager.Instance.SetBlock(this.world, _pos, BlockValue.Air);
			}
			else if (block.IsDistantDecoration)
			{
				DecoManager.Instance.SetBlock(this.world, _pos, _bv);
			}
			return BlockValue.Air;
		}
		int num5 = World.toBlockXZ(_pos.x);
		int blockY = World.toBlockY(_pos.y);
		int num6 = World.toBlockXZ(_pos.z);
		BlockValue blockValue = BlockValue.Air;
		if (_isChangeBV)
		{
			blockValue = chunkSync.SetBlock(this.world, num5, _pos.y, num6, _bv, true, !_wasChild, false, false, _changedByEntityId);
		}
		bool flag;
		bool flag2;
		bool flag3;
		blockValue.Block.CheckUpdate(blockValue, _bv, out flag, out flag2, out flag3);
		if (_isNotify && !flag2)
		{
			_isNotify = false;
		}
		if (_isUpdateLight && !flag3)
		{
			_isUpdateLight = false;
		}
		if (!_isChangeBV)
		{
			_bv = chunkSync.GetBlock(num5, _pos.y, num6);
			block = _bv.Block;
		}
		sbyte density = chunkSync.GetDensity(num5, _pos.y, num6);
		if (!_isChangeDensity)
		{
			_density = density;
		}
		if (!_isForceDensity)
		{
			if (block.shape.IsTerrain())
			{
				if (_density > MarchingCubes.DensityAirHi)
				{
					_density = MarchingCubes.DensityAirHi;
					_isChangeDensity = true;
				}
			}
			else if (_density < MarchingCubes.DensityTerrainHi)
			{
				_density = MarchingCubes.DensityTerrainHi;
				_isChangeDensity = true;
			}
		}
		if (_isChangeDensity)
		{
			chunkSync.SetDensity(num5, _pos.y, num6, _density);
		}
		long textureFull = chunkSync.GetTextureFull(num5, _pos.y, num6);
		if (_isChangeBV && _isNotify && !this.isInNotify && (!blockValue.Equals(_bv) || blockValue.damage != _bv.damage))
		{
			this.isInNotify = true;
			if (!this.world.IsRemote())
			{
				this.notifyBlocksOfNeighborChange(_pos, _bv, blockValue);
			}
			if (GameManager.bPhysicsActive && !chunkSync.StopStabilityCalculation && !blockValue.Equals(_bv))
			{
				bool isair = _bv.isair;
				if (!isair && !block.blockMaterial.IsLiquid)
				{
					bool stabilityFull = _bv.Block.StabilityFull;
					this.stabilityCalcMainThread.BlockPlacedAt(_pos, stabilityFull);
					if (block.isMultiBlock)
					{
						for (int num7 = block.multiBlockPos.Length - 1; num7 >= 0; num7--)
						{
							Vector3i vector3i = _pos + block.multiBlockPos.Get(num7, _bv.type, (int)_bv.rotation);
							if (vector3i.x != 0 || vector3i.y != 0 || vector3i.z != 0)
							{
								this.stabilityCalcMainThread.BlockPlacedAt(vector3i, stabilityFull);
							}
						}
					}
				}
				else if (isair && !blockValue.Block.blockMaterial.IsLiquid)
				{
					this.stabilityCalcMainThread.BlockRemovedAt(_pos);
					Block block2 = blockValue.Block;
					if (block2.isMultiBlock)
					{
						for (int num8 = block2.multiBlockPos.Length - 1; num8 >= 0; num8--)
						{
							Vector3i vector3i2 = block2.multiBlockPos.Get(num8, blockValue.type, (int)blockValue.rotation);
							if (vector3i2.x != 0 || vector3i2.y != 0 || vector3i2.z != 0)
							{
								Vector3i pos = _pos + vector3i2;
								this.stabilityCalcMainThread.BlockRemovedAt(pos);
							}
						}
					}
				}
				if (MeshDescription.bDebugStability)
				{
					for (int num9 = num2 - 1; num9 <= num2 + 1; num9++)
					{
						for (int num10 = num - 1; num10 <= num + 1; num10++)
						{
							Chunk chunkSync2 = base.GetChunkSync(num10, num9);
							if (chunkSync2 != null)
							{
								chunkSync2.NeedsRegeneration = true;
							}
						}
					}
				}
			}
			this.isInNotify = false;
		}
		if (flag)
		{
			this.chunkPosNeedsRegeneration(chunkSync, num5, blockY, num6, _isChangeDensity || blockValue.Block.shape.IsTerrain() || block.shape.IsTerrain());
		}
		if (_isChangeBV && _isUpdateLight)
		{
			this.m_LightProcessorMainThread.RefreshSunlightAtLocalPos(chunkSync, num5, num6, true, true);
			byte light = chunkSync.GetLight(num5, _pos.y, num6, Chunk.LIGHT_TYPE.BLOCK);
			byte b;
			if (!blockValue.isair && _bv.isair)
			{
				chunkSync.SetLight(num5, _pos.y, num6, 0, Chunk.LIGHT_TYPE.BLOCK);
				this.m_LightProcessorMainThread.RefreshLightAtLocalPos(chunkSync, num5, _pos.y, num6, Chunk.LIGHT_TYPE.BLOCK);
				b = chunkSync.GetLight(num5, _pos.y, num6, Chunk.LIGHT_TYPE.BLOCK);
			}
			else
			{
				b = block.GetLightValue(_bv);
				chunkSync.SetLight(num5, _pos.y, num6, b, Chunk.LIGHT_TYPE.BLOCK);
			}
			if (b > light)
			{
				this.m_LightProcessorMainThread.SpreadLight(chunkSync, num5, _pos.y, num6, b, Chunk.LIGHT_TYPE.BLOCK, true);
			}
			else if (b < light)
			{
				this.m_LightProcessorMainThread.UnspreadLight(chunkSync, num5, _pos.y, num6, light, Chunk.LIGHT_TYPE.BLOCK);
			}
		}
		if (chunkSync.GetTextureFull(num5, _pos.y, num6) != 0L && !blockValue.isair && _bv.isair)
		{
			chunkSync.SetTextureFull(num5, _pos.y, num6, 0L);
		}
		if (this.OnBlockChangedDelegates != null)
		{
			this.OnBlockChangedDelegates(_pos, blockValue, density, textureFull, _bv);
		}
		return blockValue;
	}

	public void SetBlockRaw(Vector3i _worldBlockPos, BlockValue _blockValue)
	{
		Chunk chunkSync = base.GetChunkSync(World.toChunkXZ(_worldBlockPos.x), World.toChunkXZ(_worldBlockPos.z));
		if (chunkSync == null)
		{
			return;
		}
		chunkSync.SetBlockRaw(World.toBlockXZ(_worldBlockPos.x), _worldBlockPos.y, World.toBlockXZ(_worldBlockPos.z), _blockValue);
	}

	public byte GetLight(Vector3i _blockPos, Chunk.LIGHT_TYPE type)
	{
		IChunk chunkFromWorldPos = this.GetChunkFromWorldPos(_blockPos);
		if (chunkFromWorldPos != null)
		{
			return chunkFromWorldPos.GetLight(World.toBlockXZ(_blockPos.x), World.toBlockY(_blockPos.y), World.toBlockXZ(_blockPos.z), type);
		}
		return 0;
	}

	public ChunkGameObject RemoveDisplayedChunkGameObject(long _key)
	{
		DictionarySave<long, ChunkGameObject> displayedChunkGameObjects = this.DisplayedChunkGameObjects;
		ChunkGameObject result;
		lock (displayedChunkGameObjects)
		{
			result = this.DisplayedChunkGameObjects[_key];
			this.DisplayedChunkGameObjects.Remove(_key);
		}
		return result;
	}

	public void SetDisplayedChunkGameObject(long _key, ChunkGameObject _cgo)
	{
		DictionarySave<long, ChunkGameObject> displayedChunkGameObjects = this.DisplayedChunkGameObjects;
		lock (displayedChunkGameObjects)
		{
			this.DisplayedChunkGameObjects[_key] = _cgo;
		}
	}

	public void ChunkPosNeedsRegeneration_DelayedStart()
	{
		this.delayedRegenCount++;
		if (this.delayedRegenCount == 1)
		{
			Dictionary<Chunk, int> obj = this.delayedRegenChunks;
			lock (obj)
			{
				this.delayedRegenChunks.Clear();
			}
		}
	}

	public void ChunkPosNeedsRegeneration_DelayedStop()
	{
		this.delayedRegenCount--;
		if (this.delayedRegenCount == 0)
		{
			Dictionary<Chunk, int> obj = this.delayedRegenChunks;
			lock (obj)
			{
				foreach (KeyValuePair<Chunk, int> keyValuePair in this.delayedRegenChunks)
				{
					keyValuePair.Key.SetNeedsRegenerationRaw(keyValuePair.Value);
				}
				this.delayedRegenChunks.Clear();
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void chunkRegenerateAt(Chunk _c, int _yPos)
	{
		if (this.delayedRegenCount == 0)
		{
			_c.NeedsRegenerationAt = _yPos;
			return;
		}
		Dictionary<Chunk, int> obj = this.delayedRegenChunks;
		lock (obj)
		{
			int needsRegenerationAt;
			if (!this.delayedRegenChunks.TryGetValue(_c, out needsRegenerationAt))
			{
				needsRegenerationAt = _c.NeedsRegenerationAt;
				this.delayedRegenChunks.Add(_c, 0);
			}
			this.delayedRegenChunks[_c] = (needsRegenerationAt | 1 << _yPos / 16);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void chunkPosNeedsRegeneration(Chunk _chunk, int _blockX, int _blockY, int _blockZ, bool _bTerrainBlockChanged)
	{
		this.chunksNeedingRegThisCall.Clear();
		this.chunkRegenerateAt(_chunk, _blockY);
		this.chunksNeedingRegThisCall.Add(_chunk);
		if (_blockY > 0 && _blockY % 16 == 0)
		{
			this.chunkRegenerateAt(_chunk, _blockY - 1);
		}
		else if (_blockY < 255 && (_blockY + 1) % 16 == 0)
		{
			this.chunkRegenerateAt(_chunk, _blockY + 1);
		}
		if (_blockX == 15)
		{
			Chunk chunkSync = base.GetChunkSync(_chunk.X + 1, _chunk.Z);
			if (chunkSync != null)
			{
				this.chunkRegenerateAt(chunkSync, _blockY);
				this.chunksNeedingRegThisCall.Add(chunkSync);
				if (_blockY > 0 && _blockY % 16 == 0)
				{
					this.chunkRegenerateAt(chunkSync, _blockY - 1);
				}
				else if (_blockY < 255 && (_blockY + 1) % 16 == 0)
				{
					this.chunkRegenerateAt(chunkSync, _blockY + 1);
				}
			}
		}
		else if (_blockX == 0)
		{
			Chunk chunkSync2 = base.GetChunkSync(_chunk.X - 1, _chunk.Z);
			if (chunkSync2 != null)
			{
				this.chunkRegenerateAt(chunkSync2, _blockY);
				this.chunksNeedingRegThisCall.Add(chunkSync2);
				if (_blockY > 0 && _blockY % 16 == 0)
				{
					this.chunkRegenerateAt(chunkSync2, _blockY - 1);
				}
				else if (_blockY < 255 && (_blockY + 1) % 16 == 0)
				{
					this.chunkRegenerateAt(chunkSync2, _blockY + 1);
				}
			}
		}
		if (_blockZ == 0)
		{
			Chunk chunkSync3 = base.GetChunkSync(_chunk.X, _chunk.Z - 1);
			if (chunkSync3 != null)
			{
				this.chunkRegenerateAt(chunkSync3, _blockY);
				this.chunksNeedingRegThisCall.Add(chunkSync3);
				if (_blockY > 0 && _blockY % 16 == 0)
				{
					this.chunkRegenerateAt(chunkSync3, _blockY - 1);
				}
				else if (_blockY < 255 && (_blockY + 1) % 16 == 0)
				{
					this.chunkRegenerateAt(chunkSync3, _blockY + 1);
				}
			}
		}
		else if (_blockZ == 15)
		{
			Chunk chunkSync4 = base.GetChunkSync(_chunk.X, _chunk.Z + 1);
			if (chunkSync4 != null)
			{
				this.chunkRegenerateAt(chunkSync4, _blockY);
				this.chunksNeedingRegThisCall.Add(chunkSync4);
				if (_blockY > 0 && _blockY % 16 == 0)
				{
					this.chunkRegenerateAt(chunkSync4, _blockY - 1);
				}
				else if (_blockY < 255 && (_blockY + 1) % 16 == 0)
				{
					this.chunkRegenerateAt(chunkSync4, _blockY + 1);
				}
			}
		}
		if (_bTerrainBlockChanged)
		{
			if (_blockX == 0 && _blockZ == 0)
			{
				Chunk chunkSync5 = base.GetChunkSync(_chunk.X - 1, _chunk.Z - 1);
				if (chunkSync5 != null)
				{
					this.chunkRegenerateAt(chunkSync5, _blockY);
					this.chunksNeedingRegThisCall.Add(chunkSync5);
					if (_blockY > 0 && _blockY % 16 == 0)
					{
						this.chunkRegenerateAt(chunkSync5, _blockY - 1);
					}
					else if (_blockY < 255 && (_blockY + 1) % 16 == 0)
					{
						this.chunkRegenerateAt(chunkSync5, _blockY + 1);
					}
				}
			}
			if (_blockX == 0 && _blockZ == 15)
			{
				Chunk chunkSync6 = base.GetChunkSync(_chunk.X - 1, _chunk.Z + 1);
				if (chunkSync6 != null)
				{
					this.chunkRegenerateAt(chunkSync6, _blockY);
					this.chunksNeedingRegThisCall.Add(chunkSync6);
					if (_blockY > 0 && _blockY % 16 == 0)
					{
						this.chunkRegenerateAt(chunkSync6, _blockY - 1);
					}
					else if (_blockY < 255 && (_blockY + 1) % 16 == 0)
					{
						this.chunkRegenerateAt(chunkSync6, _blockY + 1);
					}
				}
			}
			if (_blockX == 15 && _blockZ == 0)
			{
				Chunk chunkSync7 = base.GetChunkSync(_chunk.X + 1, _chunk.Z - 1);
				if (chunkSync7 != null)
				{
					this.chunkRegenerateAt(chunkSync7, _blockY);
					this.chunksNeedingRegThisCall.Add(chunkSync7);
					if (_blockY > 0 && _blockY % 16 == 0)
					{
						this.chunkRegenerateAt(chunkSync7, _blockY - 1);
					}
					else if (_blockY < 255 && (_blockY + 1) % 16 == 0)
					{
						this.chunkRegenerateAt(chunkSync7, _blockY + 1);
					}
				}
			}
			if (_blockX == 15 && _blockZ == 15)
			{
				Chunk chunkSync8 = base.GetChunkSync(_chunk.X + 1, _chunk.Z + 1);
				if (chunkSync8 != null)
				{
					this.chunkRegenerateAt(chunkSync8, _blockY);
					this.chunksNeedingRegThisCall.Add(chunkSync8);
					if (_blockY > 0 && _blockY % 16 == 0)
					{
						this.chunkRegenerateAt(chunkSync8, _blockY - 1);
					}
					else if (_blockY < 255 && (_blockY + 1) % 16 == 0)
					{
						this.chunkRegenerateAt(chunkSync8, _blockY + 1);
					}
				}
			}
		}
		EntityPlayerLocal primaryPlayer = this.world.GetPrimaryPlayer();
		if (primaryPlayer != null && this.chunksNeedingRegThisCall.Count > 0)
		{
			Vector3i vector3i = World.worldToBlockPos(this.ToLocalPosition(primaryPlayer.GetPosition()));
			int num = World.toChunkXZ(vector3i.x);
			int num2 = World.toChunkXZ(vector3i.z);
			ChunkManager chunkManager = this.world.m_ChunkManager;
			chunkManager.ResetChunksToCopyInOneFrame();
			for (int i = 0; i < this.chunksNeedingRegThisCall.Count; i++)
			{
				Chunk chunk = this.chunksNeedingRegThisCall[i];
				int num3 = num - chunk.X;
				int num4 = num2 - chunk.Z;
				if (num3 < 0)
				{
					num3 = -num3;
				}
				if (num4 < 0)
				{
					num4 = -num4;
				}
				if (num3 <= 1 && num4 <= 1)
				{
					chunkManager.ChunksToCopyInOneFrame.Add(chunk);
				}
			}
		}
		this.chunksNeedingRegThisCall.Clear();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void notifyBlocksOfNeighborChange(Vector3i _worldBlockPos, BlockValue _newBlockValue, BlockValue _oldBlockValue)
	{
		for (int i = 0; i < Vector3i.AllDirections.Length; i++)
		{
			this.notifyBlockOfNeighborChange(_worldBlockPos + Vector3i.AllDirections[i], _newBlockValue, _oldBlockValue, _worldBlockPos);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void notifyBlockOfNeighborChange(Vector3i _myBlockPos, BlockValue _newBlockValue, BlockValue _oldBlockValue, Vector3i _blockPosThatChanged)
	{
		if (this.world.IsRemote())
		{
			return;
		}
		BlockValue block = this.world.GetBlock(_myBlockPos);
		if (!block.isair)
		{
			block.Block.OnNeighborBlockChange(this.world, 0, _myBlockPos, block, _blockPosThatChanged, _newBlockValue, _oldBlockValue);
		}
	}

	public Vector3 ToWorldPosition(Vector3 _localPos)
	{
		return _localPos + this.Position;
	}

	public Vector3 ToLocalPosition(Vector3 _worldPos)
	{
		_worldPos.x -= this.Position.x;
		_worldPos.y -= this.Position.y;
		_worldPos.z -= this.Position.z;
		return _worldPos;
	}

	public Vector3 ToLocalVector(Vector3 _vector)
	{
		return _vector;
	}

	public long ToLocalKey(long _key)
	{
		int num = World.toChunkXZ(Mathf.FloorToInt(this.Position.x));
		int num2 = World.toChunkXZ(Mathf.FloorToInt(this.Position.z));
		int x = WorldChunkCache.extractX(_key) - num;
		int y = WorldChunkCache.extractZ(_key) - num2;
		return WorldChunkCache.MakeChunkKey(x, y);
	}

	public List<BlockEntityData> GetBlockEntities(string _indexedBlockKey)
	{
		List<BlockEntityData> list = new List<BlockEntityData>();
		List<Chunk> chunkArrayCopySync = base.GetChunkArrayCopySync();
		for (int i = 0; i < chunkArrayCopySync.Count; i++)
		{
			List<Vector3i> list2 = chunkArrayCopySync[i].IndexedBlocks[_indexedBlockKey];
			if (list2 != null)
			{
				for (int j = 0; j < list2.Count; j++)
				{
					Vector3i pos = list2[j];
					Vector3i worldPos = chunkArrayCopySync[i].ToWorldPos(pos);
					BlockEntityData blockEntity = chunkArrayCopySync[i].GetBlockEntity(worldPos);
					if (blockEntity != null)
					{
						list.Add(blockEntity);
					}
				}
			}
		}
		return list;
	}

	public BlockEntityData GetBlockEntity(Vector3i _blockLocalPos)
	{
		IChunk chunkFromWorldPos = this.GetChunkFromWorldPos(_blockLocalPos);
		if (chunkFromWorldPos == null)
		{
			return null;
		}
		return chunkFromWorldPos.GetBlockEntity(_blockLocalPos);
	}

	public void DebugOnGUI(float middleX, float middleY, float size)
	{
		List<Chunk> chunkArrayCopySync = base.GetChunkArrayCopySync();
		Vector2 vector = new Vector2(float.MaxValue, float.MaxValue);
		Vector2 vector2 = new Vector2(float.MinValue, float.MinValue);
		for (int i = 0; i < chunkArrayCopySync.Count; i++)
		{
			Chunk chunk = chunkArrayCopySync[i];
			vector.x = Utils.FastMin((float)chunk.X, vector.x);
			vector.y = Utils.FastMin((float)chunk.Z, vector.y);
			vector2.x = Utils.FastMax((float)chunk.X, vector2.x);
			vector2.y = Utils.FastMax((float)chunk.Z, vector2.y);
		}
		vector *= size;
		vector2 *= size;
		float num = middleX + vector.x;
		if (num < 0f)
		{
			middleX -= num;
		}
		float num2 = middleY + vector.y;
		if (num2 < 0f)
		{
			middleY += num2;
		}
		num = middleX - vector2.x;
		if (num < 0f)
		{
			middleX += num;
		}
		num2 = middleY - vector2.y;
		if (num2 < 0f)
		{
			middleY -= num2;
		}
		for (int j = 0; j < chunkArrayCopySync.Count; j++)
		{
			Chunk chunk2 = chunkArrayCopySync[j];
			bool flag = this.DisplayedChunkGameObjects.ContainsKey(chunk2.Key);
			Color colFill = new Color(0.1f, 0.5f, 0.1f);
			Color black = Color.black;
			if (flag)
			{
				black = new Color(0.6f, 0.6f, 0.6f);
			}
			if (chunk2.NeedsDecoration)
			{
				colFill = Color.red;
			}
			else if (chunk2.NeedsLightCalculation)
			{
				colFill = new Color(0.7f, 0.7f, 0f);
			}
			else if (chunk2.NeedsRegeneration)
			{
				colFill = new Color(0.1f, 0.1f, 0.7f);
			}
			else if (chunk2.NeedsCopying)
			{
				colFill = new Color(0.7f, 0.1f, 0.7f);
			}
			else if (chunk2.NeedsOnlyCollisionMesh)
			{
				colFill = Color.gray;
			}
			colFill.a = 0.7f;
			GUIUtils.DrawFilledRect(new Rect(middleX + (float)chunk2.X * size - size * 0.5f, middleY - (float)chunk2.Z * size - size * 0.5f, size, size), colFill, true, black);
		}
	}

	public void SnapTerrainToPositionAroundLocal(Vector3i _worldPos)
	{
		this.SnapTerrainToPositionAroundRPC(null, _worldPos);
	}

	public void SnapTerrainToPositionAroundRPC(WorldBase _world, Vector3i _worldPos)
	{
		if (!this.GetBlock(_worldPos).Block.shape.IsTerrain())
		{
			return;
		}
		this.snapTerrainToPosition(_world, _worldPos, false, false);
		this.snapTerrainToPosition(_world, _worldPos + Vector3i.right, false, true);
		this.snapTerrainToPosition(_world, _worldPos - Vector3i.right, false, true);
		this.snapTerrainToPosition(_world, _worldPos + Vector3i.forward, false, true);
		this.snapTerrainToPosition(_world, _worldPos - Vector3i.forward, false, true);
	}

	public void SnapTerrainToPositionAtLocal(Vector3i _worldPos, bool _bLiftUpTerrainByOneIfNeeded, bool _bUseHalfTerrainDensity)
	{
		this.snapTerrainToPosition(null, _worldPos, _bLiftUpTerrainByOneIfNeeded, _bUseHalfTerrainDensity);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void snapTerrainToPosition(WorldBase _world, Vector3i _worldPos, bool _bLiftUpTerrainByOneIfNeeded, bool _bUseHalfTerrainDensity)
	{
		if (_worldPos.y < 1)
		{
			return;
		}
		BlockValue block = this.GetBlock(_worldPos);
		if (!block.Block.shape.IsTerrain())
		{
			if (!_bLiftUpTerrainByOneIfNeeded)
			{
				return;
			}
			if (!block.isair)
			{
				return;
			}
			BlockValue block2 = this.GetBlock(_worldPos - Vector3i.up);
			if (block2.Block.shape.IsTerrain())
			{
				sbyte density = this.GetDensity(_worldPos);
				sbyte b = _bUseHalfTerrainDensity ? (MarchingCubes.DensityTerrain / 2) : MarchingCubes.DensityTerrain;
				if (_world == null)
				{
					this.SetBlockRaw(_worldPos, block2);
					if (density > b)
					{
						this.SetDensityRaw(_worldPos, b);
						return;
					}
				}
				else
				{
					if (density > b)
					{
						_world.SetBlockRPC(_worldPos, block2, b);
						return;
					}
					_world.SetBlockRPC(_worldPos, block2);
					return;
				}
			}
		}
		else
		{
			if (this.GetBlock(_worldPos + Vector3i.up).Block.IsTerrainDecoration)
			{
				return;
			}
			sbyte density2 = this.GetDensity(_worldPos);
			sbyte b2 = _bUseHalfTerrainDensity ? (MarchingCubes.DensityTerrain / 2) : MarchingCubes.DensityTerrain;
			if (density2 > b2)
			{
				if (_world == null)
				{
					this.SetDensityRaw(_worldPos, b2);
					return;
				}
				_world.SetBlockRPC(_worldPos, b2);
			}
		}
	}

	public void InvokeOnBlockDamagedDelegates(Vector3i _blockPos, BlockValue _blockValue, int _damage, int _attackerEntityId)
	{
		if (this.OnBlockDamagedDelegates != null)
		{
			this.OnBlockDamagedDelegates(_blockPos, _blockValue, _damage, _attackerEntityId);
		}
	}

	public bool Overlaps(Bounds _boundsInWorldCoord)
	{
		return true;
	}

	public void OnChunkDisplayed(long _key, bool _isDisplayed)
	{
		if (this.OnChunkVisibleDelegates != null)
		{
			this.OnChunkVisibleDelegates(_key, _isDisplayed);
		}
		if (!this.bFinishedDisplayingDelegateCalled && this.chunkKeysNeedDisplaying != null)
		{
			this.chunkKeysNeedDisplaying.Remove(_key);
			if (this.chunkKeysNeedDisplaying.Count == 0)
			{
				this.bFinishedDisplayingDelegateCalled = true;
				if (this.OnChunksFinishedDisplayingDelegates != null)
				{
					this.OnChunksFinishedDisplayingDelegates();
				}
			}
		}
	}

	public void CheckCollisionWithBlocks(Entity _entity)
	{
		Vector3 vector = this.ToLocalPosition(_entity.boundingBox.min + new Vector3(0.001f, 0.001f, 0.001f));
		Vector3 vector2 = this.ToLocalPosition(_entity.boundingBox.max - new Vector3(0.001f, 0.001f, 0.001f));
		int num = Utils.Fastfloor(vector.x - 0.5f);
		int num2 = Utils.Fastfloor(vector.y - 0.5f);
		int num3 = Utils.Fastfloor(vector.z - 0.5f);
		int num4 = Utils.Fastfloor(vector2.x + 0.5f);
		int num5 = Utils.Fastfloor(vector2.y + 0.5f);
		int num6 = Utils.Fastfloor(vector2.z + 0.5f);
		Bounds aabb = default(Bounds);
		aabb.SetMinMax(vector, vector2);
		aabb.Expand(new Vector3(0.05f, 0.05f, 0.05f));
		CharacterControllerAbstract characterController = _entity.m_characterController;
		float num7;
		if (characterController != null)
		{
			num7 = characterController.GetSkinWidth();
		}
		else
		{
			num7 = 0.08f;
		}
		aabb.min = new Vector3(aabb.min.x, aabb.min.y - num7, aabb.min.z);
		if (num2 <= 0)
		{
			num2 = 1;
		}
		if (num5 >= 256)
		{
			num5 = 255;
		}
		IChunk chunk = null;
		for (int i = num; i <= num4; i++)
		{
			int j = num3;
			while (j <= num6)
			{
				int num8 = World.toChunkXZ(i);
				int num9 = World.toChunkXZ(j);
				if (chunk != null && chunk.X == num8 && chunk.Z == num9)
				{
					goto IL_1A8;
				}
				chunk = base.GetChunkSync(num8, num9);
				if (chunk != null)
				{
					goto IL_1A8;
				}
				IL_27D:
				j++;
				continue;
				IL_1A8:
				int x = World.toBlockXZ(i);
				int z = World.toBlockXZ(j);
				for (int k = num2; k <= num5; k++)
				{
					BlockValue block = chunk.GetBlock(x, k, z);
					if (!block.isair)
					{
						Block block2 = block.Block;
						if (block2.IsCheckCollideWithEntity)
						{
							Vector3i vector3i = new Vector3i(i, k, j);
							if (block2.isMultiBlock && block.ischild)
							{
								Vector3i parentPos = block2.multiBlockPos.GetParentPos(vector3i, block);
								block = this.world.GetBlock(parentPos);
								vector3i = parentPos;
							}
							if (block2.HasCollidingAABB(block, vector3i.x, vector3i.y, vector3i.z, 0f, aabb))
							{
								block2.OnEntityCollidedWithBlock(this.world, 0, vector3i, block, _entity);
							}
						}
					}
				}
				goto IL_27D;
			}
		}
	}

	public void Save()
	{
		if (this.ChunkProvider != null)
		{
			this.ChunkProvider.SaveAll();
		}
	}

	public int GetBlockFaceTexture(Vector3i _blockPos, BlockFace _blockFace)
	{
		Chunk chunk = (Chunk)this.GetChunkFromWorldPos(_blockPos);
		if (chunk == null)
		{
			return 0;
		}
		return chunk.GetBlockFaceTexture(World.toBlockXZ(_blockPos.x), World.toBlockY(_blockPos.y), World.toBlockXZ(_blockPos.z), _blockFace);
	}

	public void SetBlockFaceTexture(Vector3i _blockPos, BlockFace _blockFace, int _textureIdx)
	{
		Chunk chunk = (Chunk)this.GetChunkFromWorldPos(_blockPos);
		if (chunk == null)
		{
			return;
		}
		int num = World.toBlockXZ(_blockPos.x);
		int num2 = World.toBlockY(_blockPos.y);
		int num3 = World.toBlockXZ(_blockPos.z);
		long texOld = chunk.SetBlockFaceTexture(num, num2, num3, _blockFace, _textureIdx);
		this.chunkPosNeedsRegeneration(chunk, num, num2, num3, false);
		if (this.OnBlockChangedDelegates != null)
		{
			BlockValue block = this.GetBlock(_blockPos);
			this.OnBlockChangedDelegates(_blockPos, block, this.GetDensity(_blockPos), texOld, block);
		}
	}

	public void SetTextureFull(Vector3i _blockPos, long _textureFull)
	{
		Chunk chunk = (Chunk)this.GetChunkFromWorldPos(_blockPos);
		if (chunk == null)
		{
			return;
		}
		int num = World.toBlockXZ(_blockPos.x);
		int num2 = World.toBlockY(_blockPos.y);
		int num3 = World.toBlockXZ(_blockPos.z);
		long texOld = chunk.SetTextureFull(num, num2, num3, _textureFull);
		this.chunkPosNeedsRegeneration(chunk, num, num2, num3, false);
		if (this.OnBlockChangedDelegates != null)
		{
			BlockValue block = this.GetBlock(_blockPos);
			this.OnBlockChangedDelegates(_blockPos, block, this.GetDensity(_blockPos), texOld, block);
		}
	}

	public long GetTextureFull(Vector3i _blockPos)
	{
		Chunk chunk = (Chunk)this.GetChunkFromWorldPos(_blockPos);
		if (chunk == null)
		{
			return 0L;
		}
		return chunk.GetTextureFull(World.toBlockXZ(_blockPos.x), World.toBlockY(_blockPos.y), World.toBlockXZ(_blockPos.z));
	}

	public void MGTest()
	{
		this.meshGenerator.Test();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bFinishedLoadingDelegateCalled;

	[PublicizedFrom(EAccessModifier.Private)]
	public HashSetLong chunkKeysNeedLoading;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bFinishedDisplayingDelegateCalled;

	[PublicizedFrom(EAccessModifier.Private)]
	public HashSetLong chunkKeysNeedDisplaying;

	public string Name;

	public bool IsFixedSize;

	public readonly int ClusterIdx;

	public int LayerMappingId;

	public Dictionary<string, int> LayerMappingTable;

	public DictionarySave<long, ChunkGameObject> DisplayedChunkGameObjects = new DictionarySave<long, ChunkGameObject>();

	public Vector3 Position;

	[PublicizedFrom(EAccessModifier.Private)]
	public ILightProcessor m_LightProcessorMainThread;

	[PublicizedFrom(EAccessModifier.Private)]
	public ILightProcessor m_LightProcessorLightingThread;

	[PublicizedFrom(EAccessModifier.Private)]
	public StabilityCalculator stabilityCalcMainThread;

	[PublicizedFrom(EAccessModifier.Private)]
	public StabilityInitializer stabilityCalcLightingThread;

	[PublicizedFrom(EAccessModifier.Private)]
	public ChunkCacheNeighborChunks nChunks;

	[PublicizedFrom(EAccessModifier.Private)]
	public ChunkCacheNeighborBlocks nBlocks;

	[PublicizedFrom(EAccessModifier.Private)]
	public MeshGenerator meshGenerator;

	[PublicizedFrom(EAccessModifier.Private)]
	public World world;

	public IChunkProvider ChunkProvider;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<SBlockPosValue> multiBlockList = new List<SBlockPosValue>();

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isInNotify;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<Chunk> chunksNeedingRegThisCall = new List<Chunk>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<Chunk, int> delayedRegenChunks = new Dictionary<Chunk, int>();

	[PublicizedFrom(EAccessModifier.Private)]
	public int delayedRegenCount;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<Bounds> aabbList = new List<Bounds>();

	public delegate void OnBlockDamagedDelegate(Vector3i _blockPos, BlockValue _blockValue, int _damage, int _attackerEntityId);

	public delegate void OnChunksFinishedLoadingDelegate();

	public delegate void OnChunksFinishedDisplayingDelegate();

	public delegate void OnChunkVisibleDelegate(long _key, bool _isDisplayed);

	public delegate void OnBlockChangedDelegate(Vector3i pos, BlockValue bvOld, sbyte densOld, long texOld, BlockValue bvNew);
}
