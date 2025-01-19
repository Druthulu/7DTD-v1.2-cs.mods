using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using Audio;
using DynamicMusic;
using DynamicMusic.Factories;
using GamePath;
using UnityEngine;

public class World : WorldBase, IBlockAccess, IChunkAccess, IChunkCallback
{
	public event World.OnEntityLoadedDelegate EntityLoadedDelegates;

	public event World.OnEntityUnloadedDelegate EntityUnloadedDelegates;

	public event World.OnWorldChangedEvent OnWorldChanged;

	public virtual void Init(IGameManager _gameManager, WorldBiomes _biomes)
	{
		this.gameManager = _gameManager;
		this.m_ChunkManager = new ChunkManager();
		this.m_ChunkManager.Init(this);
		this.m_SharedChunkObserverCache = new SharedChunkObserverCache(this.m_ChunkManager, 3, new NoThreadingSemantics());
		LightManager.Init();
		this.triggerManager = new TriggerManager();
		this.Biomes = _biomes;
		if (_biomes != null)
		{
			this.biomeSpawnManager = new SpawnManagerBiomes(this);
		}
		this.audioManager = Manager.Instance;
		this.BiomeAtmosphereEffects = new BiomeAtmosphereEffects();
		this.BiomeAtmosphereEffects.Init(this);
	}

	public IEnumerator LoadWorld(string _sWorldName, bool _fixedSizeCC = false)
	{
		Log.Out("World.Load: " + _sWorldName);
		GamePrefs.Set(EnumGamePrefs.GameWorld, _sWorldName);
		World.IsSplatMapAvailable = GameManager.IsSplatMapAvailable();
		this.DuskDawnInit();
		this.wcd = new WorldCreationData(GameIO.GetWorldDir());
		this.worldState = new WorldState();
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && _sWorldName != null)
		{
			string text;
			if (this.IsEditor())
			{
				text = PathAbstractions.WorldsSearchPaths.GetLocation(_sWorldName, null, null).FullPath + "/main.ttw";
			}
			else
			{
				text = GameIO.GetSaveGameDir() + "/main.ttw";
				if (!SdFile.Exists(text))
				{
					if (!SdDirectory.Exists(GameIO.GetSaveGameDir()))
					{
						SdDirectory.CreateDirectory(GameIO.GetSaveGameDir());
					}
					Log.Out("Loading base world file header...");
					this.worldState.Load(GameIO.GetWorldDir() + "/main.ttw", false, true, false);
					this.worldState.GenerateNewGuid();
					this.Seed = GamePrefs.GetString(EnumGamePrefs.GameName).GetHashCode();
					this.worldState.SetFrom(this, this.worldState.providerId);
					this.worldState.worldTime = 7000UL;
					this.worldState.saveDataLimit = SaveDataLimit.GetLimitFromPref();
					this.worldState.Save(text);
				}
			}
			if (!this.worldState.Load(text, true, false, !this.IsEditor()))
			{
				Log.Error("Could not load file '" + text + "'!");
			}
			else
			{
				this.Seed = this.worldState.seed;
			}
		}
		this.wcd.Apply(this, this.worldState);
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsClient)
		{
			this.Seed = GamePrefs.GetString(EnumGamePrefs.GameNameClient).GetHashCode();
		}
		GameRandomManager.Instance.SetBaseSeed(this.Seed);
		this.rand = GameRandomManager.Instance.CreateGameRandom();
		this.rand.SetLock();
		this.worldTime = ((!this.IsEditor()) ? this.worldState.worldTime : 12000UL);
		GameTimer.Instance.ticks = this.worldState.timeInTicks;
		EntityFactory.nextEntityID = this.worldState.nextEntityID;
		if (PlatformOptimizations.LimitedSaveData && this.worldState.saveDataLimit < 0L)
		{
			GameUtils.WorldInfo worldInfo = GameUtils.WorldInfo.LoadWorldInfo(PathAbstractions.WorldsSearchPaths.GetLocation(_sWorldName, null, null));
			SaveDataLimit.SetLimitToPref(SaveDataLimitType.VeryLong.CalculateTotalSize(worldInfo.WorldSize));
		}
		else
		{
			SaveDataLimit.SetLimitToPref(this.worldState.saveDataLimit);
		}
		this.clientLastEntityId = -2;
		if (_sWorldName != null)
		{
			this.EntitiesTransform = GameObject.Find("/Entities").transform;
			EntityFactory.Init(this.EntitiesTransform);
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			this.dynamicSpawnManager = new SpawnManagerDynamic(this, null);
			if (this.worldState.dynamicSpawnerState != null && this.worldState.dynamicSpawnerState.Length > 0L)
			{
				using (PooledBinaryReader pooledBinaryReader = MemoryPools.poolBinaryReader.AllocSync(false))
				{
					pooledBinaryReader.SetBaseStream(this.worldState.dynamicSpawnerState);
					this.dynamicSpawnManager.Read(pooledBinaryReader);
				}
			}
			this.entityDistributer = new NetEntityDistribution(this, 0);
			this.worldBlockTicker = new WorldBlockTicker(this);
			this.aiDirector = new AIDirector(this);
			if (this.worldState.aiDirectorState != null)
			{
				using (PooledBinaryReader pooledBinaryReader2 = MemoryPools.poolBinaryReader.AllocSync(false))
				{
					pooledBinaryReader2.SetBaseStream(this.worldState.aiDirectorState);
					this.aiDirector.Load(pooledBinaryReader2);
				}
			}
			if (this.worldState.sleeperVolumeState != null)
			{
				using (PooledBinaryReader pooledBinaryReader3 = MemoryPools.poolBinaryReader.AllocSync(false))
				{
					pooledBinaryReader3.SetBaseStream(this.worldState.sleeperVolumeState);
					this.ReadSleeperVolumes(pooledBinaryReader3);
					goto IL_42D;
				}
			}
			this.sleeperVolumes.Clear();
			IL_42D:
			if (this.worldState.triggerVolumeState != null)
			{
				using (PooledBinaryReader pooledBinaryReader4 = MemoryPools.poolBinaryReader.AllocSync(false))
				{
					pooledBinaryReader4.SetBaseStream(this.worldState.triggerVolumeState);
					this.ReadTriggerVolumes(pooledBinaryReader4);
					goto IL_47A;
				}
			}
			this.triggerVolumes.Clear();
			IL_47A:
			if (this.worldState.wallVolumeState != null)
			{
				using (PooledBinaryReader pooledBinaryReader5 = MemoryPools.poolBinaryReader.AllocSync(false))
				{
					pooledBinaryReader5.SetBaseStream(this.worldState.wallVolumeState);
					this.ReadWallVolumes(pooledBinaryReader5);
					goto IL_4C7;
				}
			}
			this.wallVolumes.Clear();
			IL_4C7:
			SleeperVolume.WorldInit();
		}
		DecoManager.Instance.IsEnabled = (_sWorldName != "Empty");
		yield return null;
		ChunkCluster cc = null;
		yield return this.CreateChunkCluster(SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer ? this.worldState.providerId : EnumChunkProviderId.NetworkClient, GamePrefs.GetString(EnumGamePrefs.GameWorld), 0, _fixedSizeCC, delegate(ChunkCluster _cluster)
		{
			cc = _cluster;
		});
		yield return null;
		string typeName = "WorldEnvironment";
		if (this.wcd.Properties.Values.ContainsKey("WorldEnvironment.Class"))
		{
			typeName = this.wcd.Properties.Values["WorldEnvironment.Class"];
		}
		GameObject gameObject = new GameObject("WorldEnvironment");
		this.m_WorldEnvironment = (gameObject.AddComponent(Type.GetType(typeName)) as WorldEnvironment);
		this.m_WorldEnvironment.Init(this.wcd, this);
		DynamicPrefabDecorator dynamicPrefabDecorator = cc.ChunkProvider.GetDynamicPrefabDecorator();
		SelectionBoxManager.Instance.GetCategory("DynamicPrefabs").SetCallback(dynamicPrefabDecorator);
		if (GameManager.Instance.IsEditMode() && !PrefabEditModeManager.Instance.IsActive())
		{
			SelectionBoxManager.Instance.GetCategory("DynamicPrefabs").SetVisible(true);
		}
		if (DecoManager.Instance.IsEnabled)
		{
			IChunkProvider chunkProvider = this.ChunkCache.ChunkProvider;
			yield return DecoManager.Instance.OnWorldLoaded(chunkProvider.GetWorldSize().x, chunkProvider.GetWorldSize().y, this, chunkProvider);
			this.m_WorldEnvironment.CreateUnityTerrain();
		}
		if (!this.IsEditor())
		{
			(this.dmsConductor = Factory.CreateConductor()).Init(true);
		}
		this.SetupTraders();
		this.SetupSleeperVolumes();
		this.SetupTriggerVolumes();
		this.SetupWallVolumes();
		if (!GameManager.IsDedicatedServer && GameManager.IsSplatMapAvailable())
		{
			if (UnityDistantTerrainTest.Instance == null)
			{
				UnityDistantTerrainTest.Create();
			}
			if (!this.isUnityTerrainConfigured)
			{
				this.isUnityTerrainConfigured = true;
				ChunkProviderGenerateWorldFromRaw chunkProviderGenerateWorldFromRaw = this.ChunkCache.ChunkProvider as ChunkProviderGenerateWorldFromRaw;
				UnityDistantTerrainTest instance = UnityDistantTerrainTest.Instance;
				if (chunkProviderGenerateWorldFromRaw != null)
				{
					instance.HeightMap = chunkProviderGenerateWorldFromRaw.heightData;
					instance.hmWidth = chunkProviderGenerateWorldFromRaw.GetWorldSize().x;
					instance.hmHeight = chunkProviderGenerateWorldFromRaw.GetWorldSize().y;
					instance.TerrainMaterial = MeshDescription.meshes[5].materialDistant;
					instance.TerrainMaterial.renderQueue = 2490;
					instance.WaterMaterial = MeshDescription.meshes[1].materialDistant;
					instance.WaterMaterial.SetVector("_WorldDim", new Vector4((float)chunkProviderGenerateWorldFromRaw.GetWorldSize().x, (float)chunkProviderGenerateWorldFromRaw.GetWorldSize().y, 0f, 0f));
					chunkProviderGenerateWorldFromRaw.GetWaterChunks16x16(out instance.WaterChunks16x16Width, out instance.WaterChunks16x16);
					instance.LoadTerrain();
				}
			}
		}
		if (this.OnWorldChanged != null)
		{
			this.OnWorldChanged(_sWorldName);
		}
		yield break;
	}

	public void Save()
	{
		this.worldState.SetFrom(this, this.ChunkCache.ChunkProvider.GetProviderId());
		if (this.IsEditor())
		{
			this.worldState.ResetDynamicData();
			this.worldState.nextEntityID = 171;
			this.worldState.Save(PathAbstractions.WorldsSearchPaths.GetLocation(GamePrefs.GetString(EnumGamePrefs.GameWorld), null, null).FullPath + "/main.ttw");
		}
		else
		{
			this.worldState.Save(GameIO.GetSaveGameDir() + "/main.ttw");
		}
		for (int i = 0; i < this.ChunkClusters.Count; i++)
		{
			ChunkCluster chunkCluster = this.ChunkClusters[i];
			if (chunkCluster != null)
			{
				chunkCluster.Save();
			}
		}
		this.SaveDecorations();
		SaveDataUtils.SaveDataManager.CommitAsync();
	}

	public void SaveDecorations()
	{
		DecoManager.Instance.Save();
	}

	public void SaveWorldState()
	{
		this.worldState.SetFrom(this, this.ChunkCache.ChunkProvider.GetProviderId());
		this.worldState.Save(GameIO.GetSaveGameDir() + "/main.ttw");
	}

	public virtual void UnloadWorld(bool _bUnloadRespawnableEntities)
	{
		Log.Out("World.Unload");
		if (this.m_WorldEnvironment != null)
		{
			this.m_WorldEnvironment.Cleanup();
			UnityEngine.Object.Destroy(this.m_WorldEnvironment.gameObject);
			this.m_WorldEnvironment = null;
		}
		this.ChunkCache = null;
		this.ChunkClusters.Cleanup();
		this.UnloadEntities(this.Entities.list);
		EntityFactory.Cleanup();
		if (BlockToolSelection.Instance != null)
		{
			BlockToolSelection.Instance.SelectionActive = false;
		}
		SelectionBoxManager.Instance.GetCategory("DynamicPrefabs").Clear();
		SelectionBoxManager.Instance.GetCategory("TraderTeleport").Clear();
		SelectionBoxManager.Instance.GetCategory("SleeperVolume").Clear();
		SelectionBoxManager.Instance.GetCategory("TriggerVolume").Clear();
		DecoManager.Instance.OnWorldUnloaded();
		Block.OnWorldUnloaded();
		if (UnityDistantTerrainTest.Instance != null)
		{
			UnityDistantTerrainTest.Instance.Cleanup();
			this.isUnityTerrainConfigured = false;
		}
	}

	public virtual void Cleanup()
	{
		Log.Out("World.Cleanup");
		if (this.m_ChunkManager != null)
		{
			this.m_ChunkManager.Cleanup();
			this.m_ChunkManager = null;
		}
		if (this.audioManager != null)
		{
			this.audioManager.Dispose();
			Manager.CleanUp();
			this.audioManager = null;
		}
		if (this.dmsConductor != null)
		{
			this.dmsConductor.CleanUp();
			this.dmsConductor.OnWorldExit();
			this.dmsConductor = null;
		}
		LightManager.Dispose();
		for (int i = 0; i < this.Entities.list.Count; i++)
		{
			UnityEngine.Object.Destroy(this.Entities.list[i].RootTransform.gameObject);
		}
		this.Entities.Clear();
		this.EntityAlives.Clear();
		if (this.Biomes != null)
		{
			this.Biomes.Cleanup();
		}
		if (this.entityDistributer != null)
		{
			this.entityDistributer.Cleanup();
			this.entityDistributer = null;
		}
		if (this.biomeSpawnManager != null)
		{
			this.biomeSpawnManager.Cleanup();
			this.biomeSpawnManager = null;
		}
		this.dynamicSpawnManager = null;
		if (this.worldBlockTicker != null)
		{
			this.worldBlockTicker.Cleanup();
			this.worldBlockTicker = null;
		}
		BlockShapeNew.Cleanup();
		this.Biomes = null;
		if (this.objectsOnMap != null)
		{
			this.objectsOnMap.Clear();
		}
		this.m_LocalPlayerEntity = null;
		this.aiDirector = null;
		PathFinderThread.Instance = null;
		this.wcd = null;
		this.worldState = null;
		this.BiomeAtmosphereEffects = null;
		DynamicMeshUnity.ClearCachedDynamicMeshChunksList();
	}

	public void ClearCaches()
	{
		this.m_ChunkManager.FreePools();
		PathPoint.CompactPool();
		for (int i = 0; i < this.ChunkClusters.Count; i++)
		{
			ChunkCluster chunkCluster = this.ChunkClusters[i];
			if (chunkCluster != null)
			{
				chunkCluster.ChunkProvider.ClearCaches();
			}
		}
	}

	public string Guid
	{
		get
		{
			if (this.worldState != null)
			{
				return this.worldState.Guid;
			}
			return null;
		}
	}

	public long GetNextChunkToProvide()
	{
		return this.m_ChunkManager.GetNextChunkToProvide();
	}

	public virtual IEnumerator CreateChunkCluster(EnumChunkProviderId _chunkProviderId, string _clusterName, int _forceClrIdx, bool _bFixedSize, Action<ChunkCluster> _resultHandler)
	{
		ChunkCluster cc = new ChunkCluster(this, _clusterName, this.ChunkClusters.LayerMappingTable[0]);
		if (_forceClrIdx != -1)
		{
			this.ChunkClusters.AddFixed(cc, _forceClrIdx);
			this.ChunkCache = this.ChunkClusters.Cluster0;
		}
		cc.IsFixedSize = _bFixedSize;
		cc.AddChunkCallback(this);
		WaterSimulationNative.Instance.Init(cc);
		yield return cc.Init(_chunkProviderId);
		_resultHandler(cc);
		yield break;
	}

	public override void AddLocalPlayer(EntityPlayerLocal _localPlayer)
	{
		if (!this.m_LocalPlayerEntities.Contains(_localPlayer))
		{
			this.m_LocalPlayerEntities.Add(_localPlayer);
		}
		if (this.objectsOnMap == null)
		{
			this.objectsOnMap = new MapObjectManager();
		}
	}

	public override void RemoveLocalPlayer(EntityPlayerLocal _localPlayer)
	{
		this.m_LocalPlayerEntities.Remove(_localPlayer);
	}

	public override List<EntityPlayerLocal> GetLocalPlayers()
	{
		return this.m_LocalPlayerEntities;
	}

	public override bool IsLocalPlayer(int _playerId)
	{
		Entity entity = this.GetEntity(_playerId);
		return entity != null && entity is EntityPlayerLocal;
	}

	public override EntityPlayerLocal GetLocalPlayerFromID(int _playerId)
	{
		return this.GetEntity(_playerId) as EntityPlayerLocal;
	}

	public override EntityPlayerLocal GetClosestLocalPlayer(Vector3 _position)
	{
		EntityPlayerLocal result = this.GetPrimaryPlayer();
		if (this.m_LocalPlayerEntities.Count > 1)
		{
			float num = float.MaxValue;
			for (int i = 0; i < this.m_LocalPlayerEntities.Count; i++)
			{
				float sqrMagnitude = (this.m_LocalPlayerEntities[i].GetPosition() - _position).sqrMagnitude;
				if (sqrMagnitude < num)
				{
					num = sqrMagnitude;
					result = this.m_LocalPlayerEntities[i];
				}
			}
		}
		return result;
	}

	public override Vector3 GetVectorToClosestLocalPlayer(Vector3 _position)
	{
		return this.GetClosestLocalPlayer(_position).GetPosition() - _position;
	}

	public override float GetSquaredDistanceToClosestLocalPlayer(Vector3 _position)
	{
		return this.GetVectorToClosestLocalPlayer(_position).sqrMagnitude;
	}

	public override float GetDistanceToClosestLocalPlayer(Vector3 _position)
	{
		return this.GetVectorToClosestLocalPlayer(_position).magnitude;
	}

	public void SetLocalPlayer(EntityPlayerLocal _thePlayer)
	{
		this.m_LocalPlayerEntity = _thePlayer;
		this.audioManager.AttachLocalPlayer(_thePlayer, this);
		LightManager.AttachLocalPlayer(_thePlayer, this);
		OcclusionManager.Instance.SetSourceDepthCamera(_thePlayer.playerCamera);
	}

	public override EntityPlayerLocal GetPrimaryPlayer()
	{
		return this.m_LocalPlayerEntity;
	}

	public int GetPrimaryPlayerId()
	{
		if (!(this.m_LocalPlayerEntity != null))
		{
			return -1;
		}
		return this.m_LocalPlayerEntity.entityId;
	}

	public override List<EntityPlayer> GetPlayers()
	{
		return this.Players.list;
	}

	public void GetSunAndBlockColors(Vector3i _worldBlockPos, out byte sunLight, out byte blockLight)
	{
		sunLight = 0;
		blockLight = 0;
		IChunk chunkFromWorldPos = this.GetChunkFromWorldPos(_worldBlockPos);
		if (chunkFromWorldPos != null)
		{
			int x = World.toBlockXZ(_worldBlockPos.x);
			int y = World.toBlockY(_worldBlockPos.y);
			int z = World.toBlockXZ(_worldBlockPos.z);
			sunLight = chunkFromWorldPos.GetLight(x, y, z, Chunk.LIGHT_TYPE.SUN);
			blockLight = chunkFromWorldPos.GetLight(x, y, z, Chunk.LIGHT_TYPE.BLOCK);
		}
	}

	public override float GetLightBrightness(Vector3i blockPos)
	{
		IChunk chunkFromWorldPos = this.GetChunkFromWorldPos(blockPos);
		if (chunkFromWorldPos != null)
		{
			int x = World.toBlockXZ(blockPos.x);
			int y = World.toBlockY(blockPos.y);
			int z = World.toBlockXZ(blockPos.z);
			return chunkFromWorldPos.GetLightBrightness(x, y, z, 0);
		}
		if (!this.IsDaytime())
		{
			return 0.1f;
		}
		return 0.65f;
	}

	public override int GetBlockLightValue(int _clrIdx, Vector3i blockPos)
	{
		ChunkCluster chunkCluster = this.ChunkClusters[_clrIdx];
		if (chunkCluster == null)
		{
			return (int)MarchingCubes.DensityAir;
		}
		IChunk chunkFromWorldPos = chunkCluster.GetChunkFromWorldPos(blockPos);
		if (chunkFromWorldPos != null)
		{
			int x = World.toBlockXZ(blockPos.x);
			int y = World.toBlockY(blockPos.y);
			int z = World.toBlockXZ(blockPos.z);
			return chunkFromWorldPos.GetLightValue(x, y, z, 0);
		}
		return 0;
	}

	public override sbyte GetDensity(int _clrIdx, Vector3i _blockPos)
	{
		return this.GetDensity(_clrIdx, _blockPos.x, _blockPos.y, _blockPos.z);
	}

	public override sbyte GetDensity(int _clrIdx, int _x, int _y, int _z)
	{
		ChunkCluster chunkCluster = this.ChunkClusters[_clrIdx];
		if (chunkCluster == null)
		{
			return MarchingCubes.DensityAir;
		}
		IChunk chunkFromWorldPos = chunkCluster.GetChunkFromWorldPos(_x, _y, _z);
		if (chunkFromWorldPos != null)
		{
			return chunkFromWorldPos.GetDensity(World.toBlockXZ(_x), World.toBlockY(_y), World.toBlockXZ(_z));
		}
		return MarchingCubes.DensityAir;
	}

	public void SetDensity(int _clrIdx, Vector3i _pos, sbyte _density, bool _bFoceDensity = false)
	{
		ChunkCluster chunkCluster = this.ChunkClusters[_clrIdx];
		if (chunkCluster == null)
		{
			return;
		}
		chunkCluster.SetDensity(_pos, _density, _bFoceDensity);
	}

	public long GetTexture(int _x, int _y, int _z)
	{
		Chunk chunk = (Chunk)this.ChunkCache.GetChunkFromWorldPos(_x, _y, _z);
		if (chunk != null)
		{
			return chunk.GetTextureFull(World.toBlockXZ(_x), World.toBlockY(_y), World.toBlockXZ(_z));
		}
		return 0L;
	}

	public void SetTexture(int _clrIdx, int _x, int _y, int _z, long _tex)
	{
		this.ChunkClusters[_clrIdx].SetTextureFull(new Vector3i(_x, _y, _z), _tex);
	}

	public override byte GetStability(int worldX, int worldY, int worldZ)
	{
		IChunk chunkSync = this.GetChunkSync(World.toChunkXZ(worldX), worldY, World.toChunkXZ(worldZ));
		if (chunkSync != null)
		{
			return chunkSync.GetStability(World.toBlockXZ(worldX), World.toBlockY(worldY), World.toBlockXZ(worldZ));
		}
		return 0;
	}

	public override byte GetStability(Vector3i _pos)
	{
		return this.GetStability(_pos.x, _pos.y, _pos.z);
	}

	public override void SetStability(int worldX, int worldY, int worldZ, byte stab)
	{
		IChunk chunkSync = this.GetChunkSync(World.toChunkXZ(worldX), worldY, World.toChunkXZ(worldZ));
		if (chunkSync != null)
		{
			chunkSync.SetStability(World.toBlockXZ(worldX), World.toBlockY(worldY), World.toBlockXZ(worldZ), stab);
		}
	}

	public override void SetStability(Vector3i _pos, byte stab)
	{
		this.SetStability(_pos.x, _pos.y, _pos.z, stab);
	}

	public override byte GetHeight(int worldX, int worldZ)
	{
		IChunk chunkSync = this.GetChunkSync(World.toChunkXZ(worldX), 0, World.toChunkXZ(worldZ));
		if (chunkSync != null)
		{
			return chunkSync.GetHeight(World.toBlockXZ(worldX), World.toBlockXZ(worldZ));
		}
		return 0;
	}

	public byte GetTerrainHeight(int worldX, int worldZ)
	{
		Chunk chunk = (Chunk)this.GetChunkSync(World.toChunkXZ(worldX), 0, World.toChunkXZ(worldZ));
		if (chunk != null)
		{
			return chunk.GetTerrainHeight(World.toBlockXZ(worldX), World.toBlockXZ(worldZ));
		}
		return 0;
	}

	public float GetHeightAt(float worldX, float worldZ)
	{
		IChunkProvider chunkProvider = this.ChunkCache.ChunkProvider;
		ITerrainGenerator terrainGenerator = (chunkProvider != null) ? chunkProvider.GetTerrainGenerator() : null;
		if (terrainGenerator != null)
		{
			return terrainGenerator.GetTerrainHeightAt((int)worldX, (int)worldZ);
		}
		return 0f;
	}

	public bool GetWaterAt(float worldX, float worldZ)
	{
		ChunkProviderGenerateWorldFromRaw chunkProviderGenerateWorldFromRaw = this.ChunkCache.ChunkProvider as ChunkProviderGenerateWorldFromRaw;
		if (chunkProviderGenerateWorldFromRaw == null)
		{
			return false;
		}
		WorldDecoratorPOIFromImage poiFromImage = chunkProviderGenerateWorldFromRaw.poiFromImage;
		if (poiFromImage == null)
		{
			return false;
		}
		if (!poiFromImage.m_Poi.Contains((int)worldX, (int)worldZ))
		{
			return false;
		}
		byte data = poiFromImage.m_Poi.GetData((int)worldX, (int)worldZ);
		if (data == 0)
		{
			return false;
		}
		PoiMapElement poiForColor = this.Biomes.getPoiForColor((uint)data);
		return poiForColor != null && poiForColor.m_BlockValue.type == 240;
	}

	public override bool IsWater(int _x, int _y, int _z)
	{
		if (_y < 256)
		{
			IChunk chunkFromWorldPos = this.GetChunkFromWorldPos(_x, _y, _z);
			if (chunkFromWorldPos != null)
			{
				_x &= 15;
				_y &= 255;
				_z &= 15;
				return chunkFromWorldPos.IsWater(_x, _y, _z);
			}
		}
		return false;
	}

	public override bool IsWater(Vector3i _pos)
	{
		return this.IsWater(_pos.x, _pos.y, _pos.z);
	}

	public override bool IsWater(Vector3 _pos)
	{
		return this.IsWater(World.worldToBlockPos(_pos));
	}

	public override bool IsAir(int _x, int _y, int _z)
	{
		if (_y < 256)
		{
			IChunk chunkFromWorldPos = this.GetChunkFromWorldPos(_x, _y, _z);
			if (chunkFromWorldPos != null)
			{
				_x &= 15;
				_y &= 255;
				_z &= 15;
				return chunkFromWorldPos.IsAir(_x, _y, _z);
			}
		}
		return true;
	}

	public bool CheckForLevelNearbyHeights(float worldX, float worldZ, int distance)
	{
		IChunkProvider chunkProvider = this.ChunkCache.ChunkProvider;
		ITerrainGenerator terrainGenerator = (chunkProvider != null) ? chunkProvider.GetTerrainGenerator() : null;
		if (terrainGenerator != null)
		{
			float terrainHeightAt = terrainGenerator.GetTerrainHeightAt((int)worldX, (int)worldZ);
			float num = terrainHeightAt;
			float num2 = terrainHeightAt;
			terrainHeightAt = terrainGenerator.GetTerrainHeightAt((int)worldX + distance, (int)worldZ);
			if (terrainHeightAt < num)
			{
				num = terrainHeightAt;
			}
			else if (terrainHeightAt > num2)
			{
				num2 = terrainHeightAt;
			}
			terrainHeightAt = terrainGenerator.GetTerrainHeightAt((int)worldX - distance, (int)worldZ);
			if (terrainHeightAt < num)
			{
				num = terrainHeightAt;
			}
			else if (terrainHeightAt > num2)
			{
				num2 = terrainHeightAt;
			}
			terrainHeightAt = terrainGenerator.GetTerrainHeightAt((int)worldX, (int)worldZ + distance);
			if (terrainHeightAt < num)
			{
				num = terrainHeightAt;
			}
			else if (terrainHeightAt > num2)
			{
				num2 = terrainHeightAt;
			}
			terrainHeightAt = terrainGenerator.GetTerrainHeightAt((int)worldX, (int)worldZ - distance);
			if (terrainHeightAt < num)
			{
				num = terrainHeightAt;
			}
			else if (terrainHeightAt > num2)
			{
				num2 = terrainHeightAt;
			}
			return Mathf.Abs(num2 - num) <= 2f;
		}
		return false;
	}

	public bool FindRandomSpawnPointNearRandomPlayer(int maxLightValue, out int x, out int y, out int z)
	{
		if (this.Players.list.Count == 0)
		{
			x = (y = (z = 0));
			return false;
		}
		Entity entityPlayer = null;
		int num = this.GetGameRandom().RandomRange(this.Players.list.Count);
		for (int i = 0; i < this.Players.list.Count; i++)
		{
			entityPlayer = this.Players.list[i];
			if (num-- == 0)
			{
				break;
			}
		}
		return this.FindRandomSpawnPointNearPlayer(entityPlayer, maxLightValue, out x, out y, out z, 32);
	}

	public bool FindRandomSpawnPointNearPlayer(Entity _entityPlayer, int maxLightValue, out int x, out int y, out int z, int maxDistance)
	{
		return this.FindRandomSpawnPointNearPosition(_entityPlayer.GetPosition(), maxLightValue, out x, out y, out z, new Vector3((float)maxDistance, (float)maxDistance, (float)maxDistance), true, false);
	}

	public bool FindRandomSpawnPointNearPositionUnderground(Vector3 _pos, int maxLightValue, out int x, out int y, out int z, Vector3 maxDistance)
	{
		x = (y = (z = 0));
		for (int i = 0; i < 5; i++)
		{
			x = Utils.Fastfloor(_pos.x + this.RandomRange(-maxDistance.x / 2f, maxDistance.x / 2f));
			z = Utils.Fastfloor(_pos.z + this.RandomRange(-maxDistance.z / 2f, maxDistance.z / 2f));
			Chunk chunk = (Chunk)this.GetChunkFromWorldPos(x, 0, z);
			if (chunk != null && this.IsInPlayfield(chunk))
			{
				int x2 = World.toBlockXZ(x);
				int z2 = World.toBlockXZ(z);
				int num = Utils.Fastfloor(_pos.y - maxDistance.y / 2f);
				int num2 = Utils.Fastfloor(_pos.y + maxDistance.y / 2f);
				int num3 = (int)_pos.y;
				if (num3 >= num && num3 <= num2 && chunk.CanMobsSpawnAtPos(x2, num3, z2, false, true))
				{
					y = num3;
					return true;
				}
				if (chunk.FindSpawnPointAtXZ(x2, z2, out y, maxLightValue, 0, num, num2, false))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool FindRandomSpawnPointNearPosition(Vector3 _pos, int maxLightValue, out int x, out int y, out int z, Vector3 maxDistance, bool _bOnGround, bool _bIgnoreCanMobsSpawnOn = false)
	{
		x = (y = (z = 0));
		for (int i = 0; i < 5; i++)
		{
			x = Utils.Fastfloor(_pos.x + this.RandomRange(-maxDistance.x / 2f, maxDistance.x / 2f));
			z = Utils.Fastfloor(_pos.z + this.RandomRange(-maxDistance.z / 2f, maxDistance.z / 2f));
			Chunk chunk = (Chunk)this.GetChunkFromWorldPos(x, 0, z);
			if (chunk != null && this.IsInPlayfield(chunk))
			{
				if (!_bOnGround)
				{
					y = Utils.Fastfloor(_pos.y + this.RandomRange(-maxDistance.y / 2f, maxDistance.y / 2f));
					return true;
				}
				int x2 = World.toBlockXZ(x);
				int z2 = World.toBlockXZ(z);
				int num = Utils.Fastfloor(_pos.y - maxDistance.y / 2f);
				int num2 = Utils.Fastfloor(_pos.y + maxDistance.y / 2f);
				int num3 = (int)(chunk.GetHeight(x2, z2) + 1);
				if (num3 >= num && num3 <= num2 && chunk.CanMobsSpawnAtPos(x2, num3, z2, _bIgnoreCanMobsSpawnOn, true))
				{
					y = num3;
					return true;
				}
				if (chunk.FindSpawnPointAtXZ(x2, z2, out y, maxLightValue, 0, num, num2, _bIgnoreCanMobsSpawnOn))
				{
					return true;
				}
			}
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isPositionInRangeOfBedrolls(Vector3 _position)
	{
		int num = GamePrefs.GetInt(EnumGamePrefs.BedrollDeadZoneSize);
		num *= num;
		for (int i = 0; i < this.Players.list.Count; i++)
		{
			EntityBedrollPositionList spawnPoints = this.Players.list[i].SpawnPoints;
			int count = spawnPoints.Count;
			for (int j = 0; j < count; j++)
			{
				if ((spawnPoints[j].ToVector3() - _position).sqrMagnitude < (float)num)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool GetRandomSpawnPositionMinMaxToRandomPlayer(int _minRange, int _maxRange, bool _bConsiderBedrolls, out EntityPlayer _player, out Vector3 _position)
	{
		_position = Vector3.zero;
		_player = null;
		if (this.Players.list.Count == 0)
		{
			return false;
		}
		if (_maxRange - _minRange <= 0)
		{
			return false;
		}
		int num = this.rand.RandomRange(this.Players.list.Count);
		for (int i = 0; i < this.Players.list.Count; i++)
		{
			if (num-- == 0)
			{
				_player = this.Players.list[i];
				break;
			}
		}
		int num2 = _minRange * _minRange;
		for (int j = 0; j < 10; j++)
		{
			Vector2 vector = Vector2.zero;
			do
			{
				vector = this.rand.RandomInsideUnitCircle * (float)(_maxRange - _minRange);
			}
			while ((double)vector.sqrMagnitude < 0.01);
			vector += vector * ((float)_minRange / vector.magnitude);
			_position = _player.GetPosition() + new Vector3(vector.x, 0f, vector.y);
			Vector3i vector3i = World.worldToBlockPos(_position);
			Chunk chunk = (Chunk)this.GetChunkFromWorldPos(vector3i);
			if (chunk != null)
			{
				int x = World.toBlockXZ(vector3i.x);
				int z = World.toBlockXZ(vector3i.z);
				vector3i.y = (int)(chunk.GetHeight(x, z) + 1);
				_position.y = (float)vector3i.y;
				if ((!_bConsiderBedrolls || !this.isPositionInRangeOfBedrolls(vector3i.ToVector3())) && chunk.CanMobsSpawnAtPos(x, Utils.Fastfloor(_position.y), z, false, true))
				{
					bool flag = true;
					for (int k = 0; k < this.Players.list.Count; k++)
					{
						EntityPlayer entityPlayer = this.Players.list[k];
						if (entityPlayer.GetDistanceSq(_position) < (float)num2)
						{
							flag = false;
							break;
						}
						if (entityPlayer.CanSee(_position))
						{
							flag = false;
							break;
						}
					}
					if (flag)
					{
						_position = vector3i.ToVector3() + new Vector3(0.5f, this.GetTerrainOffset(0, vector3i), 0.5f);
						return true;
					}
				}
			}
		}
		return false;
	}

	public bool GetMobRandomSpawnPosWithWater(Vector3 _targetPos, int _minRange, int _maxRange, int _minPlayerRange, bool _checkBedrolls, out Vector3 _position)
	{
		return this.GetRandomSpawnPositionMinMaxToPosition(_targetPos, _minRange, _maxRange, _minPlayerRange, _checkBedrolls, out _position, false, true, 20) || this.GetRandomSpawnPositionMinMaxToPosition(_targetPos, _minRange, _maxRange, _minPlayerRange, _checkBedrolls, out _position, false, false, 20);
	}

	public bool GetRandomSpawnPositionMinMaxToPosition(Vector3 _targetPos, int _minRange, int _maxRange, int _minPlayerRange, bool _checkBedrolls, out Vector3 _position, bool _isPlayer = false, bool _checkWater = true, int _retryCount = 50)
	{
		_position = Vector3.zero;
		int num = _maxRange - _minRange;
		if (num <= 0)
		{
			return false;
		}
		for (int i = 0; i < _retryCount; i++)
		{
			Vector2 vector;
			do
			{
				vector = this.rand.RandomInsideUnitCircle * (float)num;
			}
			while (vector.sqrMagnitude < 0.01f);
			vector += vector * ((float)_minRange / vector.magnitude);
			_position.x = _targetPos.x + vector.x;
			_position.y = _targetPos.y;
			_position.z = _targetPos.z + vector.y;
			Vector3i vector3i = World.worldToBlockPos(_position);
			Chunk chunk = (Chunk)this.GetChunkFromWorldPos(vector3i);
			if (chunk != null)
			{
				int x = World.toBlockXZ(vector3i.x);
				int z = World.toBlockXZ(vector3i.z);
				vector3i.y = (int)(chunk.GetHeight(x, z) + 1);
				_position.y = (float)vector3i.y;
				if (!_checkBedrolls || !this.isPositionInRangeOfBedrolls(vector3i.ToVector3()))
				{
					if (!_isPlayer)
					{
						if (!chunk.CanMobsSpawnAtPos(x, Utils.Fastfloor(_position.y), z, false, _checkWater))
						{
							goto IL_19A;
						}
					}
					else if (!chunk.CanPlayersSpawnAtPos(x, Utils.Fastfloor(_position.y), z, false) || !chunk.IsPositionOnTerrain(x, vector3i.y, z) || this.GetPOIAtPosition(_position, true) != null)
					{
						goto IL_19A;
					}
					if (this.isPositionFarFromPlayers(_position, _minPlayerRange))
					{
						_position = vector3i.ToVector3() + new Vector3(0.5f, this.GetTerrainOffset(0, vector3i), 0.5f);
						return true;
					}
				}
			}
			IL_19A:;
		}
		_position = Vector3.zero;
		return false;
	}

	public bool GetRandomSpawnPositionInAreaMinMaxToPlayers(Rect _area, int _minDistance, int UNUSED_maxDistance, bool _checkBedrolls, out Vector3 _position)
	{
		_position = Vector3.zero;
		if (this.Players.list.Count == 0)
		{
			return false;
		}
		for (int i = 0; i < 10; i++)
		{
			_position.x = _area.x + this.RandomRange(0f, _area.width - 1f);
			_position.y = 0f;
			_position.z = _area.y + this.RandomRange(0f, _area.height - 1f);
			Vector3i vector3i = World.worldToBlockPos(_position);
			Chunk chunk = (Chunk)this.GetChunkFromWorldPos(vector3i);
			if (chunk != null)
			{
				int x = World.toBlockXZ(vector3i.x);
				int z = World.toBlockXZ(vector3i.z);
				vector3i.y = (int)(chunk.GetHeight(x, z) + 1);
				_position.y = (float)vector3i.y;
				if ((!_checkBedrolls || !this.isPositionInRangeOfBedrolls(vector3i.ToVector3())) && chunk.CanMobsSpawnAtPos(x, Utils.Fastfloor(_position.y), z, false, true))
				{
					bool flag = this.isPositionFarFromPlayers(_position, _minDistance);
					if (flag)
					{
						for (int j = 0; j < this.Players.list.Count; j++)
						{
							EntityPlayer entityPlayer = this.Players.list[j];
							if ((_position - entityPlayer.position).sqrMagnitude < 2500f && entityPlayer.IsInViewCone(_position))
							{
								flag = false;
								break;
							}
						}
						if (flag)
						{
							_position = vector3i.ToVector3() + new Vector3(0.5f, this.GetTerrainOffset(0, vector3i), 0.5f);
							return true;
						}
					}
				}
			}
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isPositionFarFromPlayers(Vector3 _position, int _minDistance)
	{
		int num = _minDistance * _minDistance;
		for (int i = 0; i < this.Players.list.Count; i++)
		{
			if (this.Players.list[i].GetDistanceSq(_position) < (float)num)
			{
				return false;
			}
		}
		return true;
	}

	public Vector3 FindSupportingBlockPos(Vector3 pos)
	{
		Vector3i vector3i = World.worldToBlockPos(pos);
		BlockValue block = this.GetBlock(vector3i);
		Block block2 = block.Block;
		if (block2.IsMovementBlocked(this, vector3i, block, BlockFace.Top))
		{
			return pos;
		}
		if (block2.IsElevator())
		{
			return pos;
		}
		vector3i.y++;
		block = this.GetBlock(vector3i);
		block2 = block.Block;
		if (block2.IsElevator((int)block.rotation))
		{
			return pos;
		}
		vector3i.y -= 2;
		block = this.GetBlock(vector3i);
		block2 = block.Block;
		if (!block2.IsElevator() && !block2.IsMovementBlocked(this, vector3i, block, BlockFace.Top))
		{
			Vector3 b = new Vector3((float)vector3i.x + 0.5f, pos.y, (float)vector3i.z + 0.5f);
			Vector3 vector = pos - b;
			int num = Mathf.RoundToInt((Mathf.Atan2(vector.x, vector.z) * 57.29578f + 22.5f) / 45f) & 7;
			int[] array = World.supportOrder[num];
			Vector3i vector3i2;
			vector3i2.y = vector3i.y;
			for (int i = 0; i < 8; i++)
			{
				int num2 = array[i] * 2;
				vector3i2.x = vector3i.x + World.supportOffsets[num2];
				vector3i2.z = vector3i.z + World.supportOffsets[num2 + 1];
				block = this.GetBlock(vector3i2);
				block2 = block.Block;
				if (block2.IsMovementBlocked(this, vector3i2, block, BlockFace.Top))
				{
					pos.x = (float)vector3i2.x + 0.5f;
					pos.z = (float)vector3i2.z + 0.5f;
					break;
				}
			}
		}
		return pos;
	}

	public float GetTerrainOffset(int _clrIdx, Vector3i _blockPos)
	{
		float result = 0f;
		if (this.GetBlock(_clrIdx, _blockPos - Vector3i.up).Block.shape.IsTerrain())
		{
			sbyte density = this.GetDensity(_clrIdx, _blockPos);
			sbyte density2 = this.GetDensity(_clrIdx, _blockPos - Vector3i.up);
			result = MarchingCubes.GetDecorationOffsetY(density, density2);
		}
		return result;
	}

	public bool IsInPlayfield(Chunk _c)
	{
		ChunkCluster chunkCache = this.ChunkCache;
		return !chunkCache.IsFixedSize || this.IsEditor() || (_c.X > chunkCache.ChunkMinPos.x && _c.Z > chunkCache.ChunkMinPos.y && _c.X < chunkCache.ChunkMaxPos.x && _c.Z < chunkCache.ChunkMaxPos.y);
	}

	public override BlockValue GetBlock(Vector3i _pos)
	{
		ChunkCluster chunkCache = this.ChunkCache;
		if (chunkCache == null)
		{
			return BlockValue.Air;
		}
		return chunkCache.GetBlock(_pos);
	}

	public override BlockValue GetBlock(int _clrIdx, Vector3i _pos)
	{
		ChunkCluster chunkCluster = this.ChunkClusters[_clrIdx];
		if (chunkCluster == null)
		{
			return BlockValue.Air;
		}
		return chunkCluster.GetBlock(_pos);
	}

	public override BlockValue GetBlock(int _x, int _y, int _z)
	{
		return this.GetBlock(new Vector3i(_x, _y, _z));
	}

	public override BlockValue GetBlock(int _clrIdx, int _x, int _y, int _z)
	{
		ChunkCluster chunkCluster = this.ChunkClusters[_clrIdx];
		if (chunkCluster == null)
		{
			return BlockValue.Air;
		}
		return chunkCluster.GetBlock(new Vector3i(_x, _y, _z));
	}

	public WaterValue GetWater(int _x, int _y, int _z)
	{
		return this.GetWater(new Vector3i(_x, _y, _z));
	}

	public WaterValue GetWater(Vector3i _pos)
	{
		ChunkCluster chunkCache = this.ChunkCache;
		if (chunkCache == null)
		{
			return WaterValue.Empty;
		}
		return chunkCache.GetWater(_pos);
	}

	public float GetWaterPercent(Vector3i _pos)
	{
		ChunkCluster chunkCache = this.ChunkCache;
		if (chunkCache == null)
		{
			return 0f;
		}
		return chunkCache.GetWater(_pos).GetMassPercent();
	}

	public BiomeDefinition GetBiome(string _name)
	{
		return this.Biomes.GetBiome(_name);
	}

	public BiomeDefinition GetBiome(int _x, int _z)
	{
		Chunk chunk = (Chunk)this.GetChunkFromWorldPos(_x, 0, _z);
		if (chunk != null)
		{
			byte biomeId = chunk.GetBiomeId(World.toBlockXZ(_x), World.toBlockXZ(_z));
			return this.Biomes.GetBiome(biomeId);
		}
		return null;
	}

	public IChunk GetChunkFromWorldPos(int x, int z)
	{
		return this.GetChunkSync(World.toChunkXZ(x), World.toChunkXZ(z));
	}

	public override IChunk GetChunkFromWorldPos(int x, int y, int z)
	{
		return this.GetChunkSync(World.toChunkXZ(x), y, World.toChunkXZ(z));
	}

	public override IChunk GetChunkFromWorldPos(Vector3i _blockPos)
	{
		return this.GetChunkSync(World.toChunkXZ(_blockPos.x), _blockPos.y, World.toChunkXZ(_blockPos.z));
	}

	public override void GetChunkFromWorldPos(int _blockX, int _blockZ, ref IChunk _chunk)
	{
		_blockX >>= 4;
		_blockZ >>= 4;
		if (_chunk == null || _chunk.X != _blockX || _chunk.Z != _blockZ)
		{
			_chunk = this.GetChunkSync(_blockX, 0, _blockZ);
		}
	}

	public override bool GetChunkFromWorldPos(Vector3i _blockPos, ref IChunk _chunk)
	{
		Vector3i vector3i = World.toChunkXYZ(_blockPos);
		if (_chunk == null || _chunk.ChunkPos != vector3i)
		{
			_chunk = this.GetChunkSync(vector3i);
		}
		return _chunk != null;
	}

	public override IChunk GetChunkSync(Vector3i chunkPos)
	{
		return this.GetChunkSync(chunkPos.x, chunkPos.y, chunkPos.z);
	}

	public override IChunk GetChunkSync(int chunkX, int chunkY, int chunkZ)
	{
		ChunkCluster chunkCache = this.ChunkCache;
		if (chunkCache == null)
		{
			return null;
		}
		return chunkCache.GetChunkSync(chunkX, chunkZ);
	}

	public IChunk GetChunkSync(int chunkX, int chunkZ)
	{
		ChunkCluster chunkCache = this.ChunkCache;
		if (chunkCache == null)
		{
			return null;
		}
		return chunkCache.GetChunkSync(chunkX, chunkZ);
	}

	public IChunk GetChunkSync(long _key)
	{
		ChunkCluster chunkCache = this.ChunkCache;
		if (chunkCache == null)
		{
			return null;
		}
		return chunkCache.GetChunkSync(_key);
	}

	public bool IsChunkAreaLoaded(Vector3 _position)
	{
		return this.IsChunkAreaLoaded(Utils.Fastfloor(_position.x), 0, Utils.Fastfloor(_position.z));
	}

	public bool IsChunkAreaLoaded(int _blockPosX, int _, int _blockPosZ)
	{
		int num = World.toChunkXZ(_blockPosX - 8);
		int num2 = World.toChunkXZ(_blockPosZ - 8);
		int num3 = World.toChunkXZ(_blockPosX + 8);
		int num4 = World.toChunkXZ(_blockPosZ + 8);
		for (int i = num; i <= num3; i++)
		{
			for (int j = num2; j <= num4; j++)
			{
				if (this.GetChunkSync(i, j) == null)
				{
					return false;
				}
			}
		}
		return true;
	}

	public bool IsChunkAreaCollidersLoaded(Vector3 _position)
	{
		return this.IsChunkAreaCollidersLoaded(Utils.Fastfloor(_position.x), Utils.Fastfloor(_position.z));
	}

	public bool IsChunkAreaCollidersLoaded(int _blockPosX, int _blockPosZ)
	{
		int num = World.toChunkXZ(_blockPosX - 8);
		int num2 = World.toChunkXZ(_blockPosZ - 8);
		int num3 = World.toChunkXZ(_blockPosX + 8);
		int num4 = World.toChunkXZ(_blockPosZ + 8);
		for (int i = num; i <= num3; i++)
		{
			for (int j = num2; j <= num4; j++)
			{
				Chunk chunk = (Chunk)this.GetChunkSync(i, j);
				if (chunk == null || !chunk.IsCollisionMeshGenerated)
				{
					return false;
				}
			}
		}
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int toChunkXZ(int _v)
	{
		return _v >> 4;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector2i toChunkXZ(Vector2i _v)
	{
		return new Vector2i(_v.x >> 4, _v.y >> 4);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector2i toChunkXZ(Vector3 _v)
	{
		return new Vector2i(Utils.Fastfloor(_v.x) >> 4, Utils.Fastfloor(_v.z) >> 4);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector2i toChunkXZ(Vector3i _v)
	{
		return new Vector2i(_v.x >> 4, _v.z >> 4);
	}

	public static Vector3i toChunkXYZCube(Vector3 _v)
	{
		return new Vector3i(Utils.Fastfloor(_v.x) >> 4, Utils.Fastfloor(_v.y) >> 4, Utils.Fastfloor(_v.z) >> 4);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector3i toChunkXYZ(Vector3i _v)
	{
		return new Vector3i(_v.x >> 4, _v.y >> 8, _v.z >> 4);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int toChunkY(int _v)
	{
		return _v >> 8;
	}

	public static Vector3 toChunkXyzWorldPos(Vector3i _v)
	{
		return new Vector3((float)(_v.x & -16), (float)(_v.y & -256), (float)(_v.z & -16));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector3i toBlock(Vector3i _p)
	{
		_p.x &= 15;
		_p.y &= 255;
		_p.z &= 15;
		return _p;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector3i toBlock(int _x, int _y, int _z)
	{
		return new Vector3i(_x & 15, _y & 255, _z & 15);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int toBlockXZ(int _v)
	{
		return _v & 15;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int toBlockY(int _v)
	{
		return _v & 255;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector3 blockToTransformPos(Vector3i _blockPos)
	{
		return new Vector3((float)_blockPos.x + 0.5f, (float)_blockPos.y, (float)_blockPos.z + 0.5f);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector3i worldToBlockPos(Vector3 _worldPos)
	{
		return new Vector3i(Utils.Fastfloor(_worldPos.x), Utils.Fastfloor(_worldPos.y), Utils.Fastfloor(_worldPos.z));
	}

	public override void SetBlockRPC(int _clrIdx, Vector3i _blockPos, BlockValue _blockValue)
	{
		this.gameManager.SetBlocksRPC(new List<BlockChangeInfo>
		{
			new BlockChangeInfo(_clrIdx, _blockPos, _blockValue, true)
		}, null);
	}

	public override void SetBlockRPC(Vector3i _blockPos, BlockValue _blockValue)
	{
		this.gameManager.SetBlocksRPC(new List<BlockChangeInfo>
		{
			new BlockChangeInfo(_blockPos, _blockValue, true, false)
		}, null);
	}

	public override void SetBlockRPC(Vector3i _blockPos, BlockValue _blockValue, sbyte _density)
	{
		this.gameManager.SetBlocksRPC(new List<BlockChangeInfo>
		{
			new BlockChangeInfo(_blockPos, _blockValue, _density)
		}, null);
	}

	public override void SetBlockRPC(int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, sbyte _density)
	{
		this.gameManager.SetBlocksRPC(new List<BlockChangeInfo>
		{
			new BlockChangeInfo(_clrIdx, _blockPos, _blockValue, _density)
		}, null);
	}

	public override void SetBlockRPC(Vector3i _blockPos, sbyte _density)
	{
		this.gameManager.SetBlocksRPC(new List<BlockChangeInfo>
		{
			new BlockChangeInfo(0, _blockPos, _density, false)
		}, null);
	}

	public override void SetBlocksRPC(List<BlockChangeInfo> _blockChangeInfo)
	{
		this.gameManager.SetBlocksRPC(_blockChangeInfo, null);
	}

	public override void SetBlockRPC(int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, sbyte _density, int _changingEntityId)
	{
		this.gameManager.SetBlocksRPC(new List<BlockChangeInfo>
		{
			new BlockChangeInfo(_clrIdx, _blockPos, _blockValue, _density, _changingEntityId)
		}, null);
	}

	public override void SetBlockRPC(int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, int _changingEntityId)
	{
		this.gameManager.SetBlocksRPC(new List<BlockChangeInfo>
		{
			new BlockChangeInfo(_clrIdx, _blockPos, _blockValue, true, _changingEntityId)
		}, null);
	}

	public BlockValue SetBlock(int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, bool bNotify, bool updateLight)
	{
		return this.ChunkClusters[_clrIdx].SetBlock(_blockPos, _blockValue, bNotify, updateLight);
	}

	public bool IsDaytime()
	{
		return !this.IsDark();
	}

	public bool IsDark()
	{
		float num = this.worldTime % 24000UL / 1000f;
		return num < (float)this.DawnHour || num > (float)this.DuskHour;
	}

	public override TileEntity GetTileEntity(int _clrIdx, Vector3i _pos)
	{
		return this.GetTileEntity(_pos);
	}

	public override TileEntity GetTileEntity(Vector3i _pos)
	{
		ChunkCluster chunkCache = this.ChunkCache;
		if (chunkCache == null)
		{
			return null;
		}
		Chunk chunk = (Chunk)chunkCache.GetChunkFromWorldPos(_pos);
		if (chunk == null)
		{
			return null;
		}
		Vector3i blockPosInChunk = new Vector3i(World.toBlockXZ(_pos.x), World.toBlockY(_pos.y), World.toBlockXZ(_pos.z));
		return chunk.GetTileEntity(blockPosInChunk);
	}

	public TileEntity GetTileEntity(int _entityId)
	{
		Entity entity = this.GetEntity(_entityId);
		if (entity == null)
		{
			return null;
		}
		if (entity is EntityTrader && entity.IsAlive())
		{
			return ((EntityTrader)entity).TileEntityTrader;
		}
		if (entity.lootContainer == null)
		{
			string lootList = entity.GetLootList();
			if (!string.IsNullOrEmpty(lootList))
			{
				entity.lootContainer = new TileEntityLootContainer(null);
				entity.lootContainer.entityId = entity.entityId;
				entity.lootContainer.lootListName = lootList;
				entity.lootContainer.SetContainerSize(LootContainer.GetLootContainer(lootList, true).size, true);
			}
		}
		return entity.lootContainer;
	}

	public void RemoveTileEntity(TileEntity _te)
	{
		Chunk chunk = _te.GetChunk();
		if (chunk != null)
		{
			chunk.RemoveTileEntity(this, _te);
			return;
		}
		Log.Error("RemoveTileEntity: chunk not found!");
	}

	public BlockTrigger GetBlockTrigger(int _clrIdx, Vector3i _pos)
	{
		ChunkCluster chunkCluster = this.ChunkClusters[_clrIdx];
		if (chunkCluster == null)
		{
			return null;
		}
		Chunk chunk = (Chunk)chunkCluster.GetChunkFromWorldPos(_pos);
		if (chunk == null)
		{
			return null;
		}
		Vector3i blockPosInChunk = new Vector3i(World.toBlockXZ(_pos.x), World.toBlockY(_pos.y), World.toBlockXZ(_pos.z));
		return chunk.GetBlockTrigger(blockPosInChunk);
	}

	public void OnUpdateTick(float _partialTicks, ArraySegment<long> _activeChunks)
	{
		this.updateChunkAddedRemovedCallbacks();
		this.WorldEventUpdateTime();
		WaterSplashCubes.Update();
		DecoManager.Instance.UpdateTick(this);
		MultiBlockManager.Instance.MainThreadUpdate();
		if (!this.IsEditor())
		{
			this.dmsConductor.Update();
		}
		this.checkPOIUnculling();
		this.updateChunksToUncull();
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			return;
		}
		this.worldBlockTicker.Tick(_activeChunks, this.m_LocalPlayerEntity, this.rand);
		if (GameTimer.Instance.ticks % 20UL == 0UL)
		{
			bool @bool = GameStats.GetBool(EnumGameStats.IsSpawnEnemies);
			int num = 0;
			ChunkCluster chunkCluster = this.ChunkClusters[num];
			bool flag = GameTimer.Instance.ticks % 40UL == 0UL;
			for (int i = _activeChunks.Offset; i < _activeChunks.Count; i++)
			{
				long num2 = _activeChunks.Array[i];
				if ((!flag || num2 % 2L == 0L) && (flag || num2 % 2L != 0L))
				{
					int num3 = WorldChunkCache.extractClrIdx(num2);
					if (num3 != num)
					{
						ChunkCluster chunkCluster2 = this.ChunkClusters[num3];
						if (chunkCluster2 == null)
						{
							goto IL_1EA;
						}
						chunkCluster = chunkCluster2;
						num = num3;
					}
					Chunk chunk = (chunkCluster != null) ? chunkCluster.GetChunkSync(num2) : null;
					if (chunk != null)
					{
						if (chunk.NeedsTicking)
						{
							chunk.UpdateTick(this, @bool);
						}
						if (!this.IsEditor() && chunk.IsAreaMaster() && chunk.IsAreaMasterDominantBiomeInitialized(chunkCluster))
						{
							ChunkAreaBiomeSpawnData chunkBiomeSpawnData = chunk.GetChunkBiomeSpawnData();
							if (chunkBiomeSpawnData != null && chunkBiomeSpawnData.IsSpawnNeeded(this.Biomes, this.worldTime) && chunk.IsAreaMasterCornerChunksLoaded(chunkCluster))
							{
								if (this.areaMasterChunksToLock.ContainsKey(chunk.Key))
								{
									chunk.isModified |= chunkBiomeSpawnData.DelayAllEnemySpawningUntil(this.areaMasterChunksToLock[chunk.Key], this.Biomes);
									this.areaMasterChunksToLock.Remove(chunk.Key);
								}
								else
								{
									this.biomeSpawnManager.Update(string.Empty, @bool, chunkBiomeSpawnData);
								}
							}
						}
					}
				}
				IL_1EA:;
			}
		}
		if (GameTimer.Instance.ticks % 16UL == 0UL && GamePrefs.GetString(EnumGamePrefs.DynamicSpawner).Length > 0)
		{
			this.dynamicSpawnManager.Update(GamePrefs.GetString(EnumGamePrefs.DynamicSpawner), GameStats.GetBool(EnumGameStats.IsSpawnEnemies), null);
		}
		this.aiDirector.Tick((double)(_partialTicks / 20f));
		this.TickSleeperVolumes();
	}

	public bool UncullPOI(PrefabInstance _pi)
	{
		if (_pi.AddChunksToUncull(this, this.chunksToUncull))
		{
			Log.Out("Unculling POI {0} {1}", new object[]
			{
				_pi.location.Name,
				_pi.boundingBoxPosition
			});
			return true;
		}
		return false;
	}

	public void UncullChunk(Chunk _c)
	{
		if (_c.IsInternalBlocksCulled)
		{
			this.chunksToUncull.Add(_c);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void checkPOIUnculling()
	{
		if (GameTimer.Instance.ticks % 38UL != 0UL || GameStats.GetInt(EnumGameStats.OptionsPOICulling) == 0)
		{
			return;
		}
		List<EntityPlayer> list = GameManager.Instance.World.Players.list;
		for (int i = 0; i < list.Count; i++)
		{
			EntityPlayer entityPlayer = list[i];
			if (entityPlayer.Spawned)
			{
				Dictionary<int, PrefabInstance> prefabsAroundNear = entityPlayer.GetPrefabsAroundNear();
				if (prefabsAroundNear != null)
				{
					foreach (KeyValuePair<int, PrefabInstance> keyValuePair in prefabsAroundNear)
					{
						PrefabInstance value = keyValuePair.Value;
						if (value.Overlaps(entityPlayer.position, 6f))
						{
							this.UncullPOI(value);
						}
					}
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateChunksToUncull()
	{
		if (this.chunksToUncull.list.Count == 0)
		{
			return;
		}
		this.msUnculling.ResetAndRestart();
		this.chunksToRegenerate.Clear();
		for (int i = this.chunksToUncull.list.Count - 1; i >= 0; i--)
		{
			Chunk chunk = this.chunksToUncull.list[i];
			if (chunk.InProgressUnloading)
			{
				this.chunksToUncull.Remove(chunk);
			}
			else
			{
				BlockFaceFlag blockFaceFlag = chunk.RestoreCulledBlocks(this);
				this.chunksToUncull.Remove(chunk);
				if (!this.chunksToRegenerate.hashSet.Contains(chunk))
				{
					this.chunksToRegenerate.Add(chunk);
				}
				Chunk chunk2;
				if ((blockFaceFlag & BlockFaceFlag.West) != BlockFaceFlag.None && (chunk2 = (Chunk)this.GetChunkSync(chunk.X - 1, chunk.Z)) != null && !this.chunksToRegenerate.hashSet.Contains(chunk2))
				{
					this.chunksToRegenerate.Add(chunk2);
				}
				if ((blockFaceFlag & BlockFaceFlag.East) != BlockFaceFlag.None && (chunk2 = (Chunk)this.GetChunkSync(chunk.X + 1, chunk.Z)) != null && !this.chunksToRegenerate.hashSet.Contains(chunk2))
				{
					this.chunksToRegenerate.Add(chunk2);
				}
				if ((blockFaceFlag & BlockFaceFlag.North) != BlockFaceFlag.None && (chunk2 = (Chunk)this.GetChunkSync(chunk.X, chunk.Z + 1)) != null && !this.chunksToRegenerate.hashSet.Contains(chunk2))
				{
					this.chunksToRegenerate.Add(chunk2);
				}
				if ((blockFaceFlag & BlockFaceFlag.South) != BlockFaceFlag.None && (chunk2 = (Chunk)this.GetChunkSync(chunk.X, chunk.Z - 1)) != null && !this.chunksToRegenerate.hashSet.Contains(chunk2))
				{
					this.chunksToRegenerate.Add(chunk2);
				}
				if (this.msUnculling.ElapsedMilliseconds > 5L)
				{
					break;
				}
			}
		}
		for (int j = this.chunksToRegenerate.list.Count - 1; j >= 0; j--)
		{
			this.chunksToRegenerate.list[j].NeedsRegeneration = true;
		}
	}

	public Vector3[] GetRandomSpawnPointPositions(int _count)
	{
		Vector3[] array = new Vector3[_count];
		List<Chunk> chunkArrayCopySync = this.ChunkCache.GetChunkArrayCopySync();
		int count = chunkArrayCopySync.Count;
		while (_count > 0)
		{
			for (int i = 0; i < chunkArrayCopySync.Count; i++)
			{
				Chunk chunk = chunkArrayCopySync[i];
				if (this.GetGameRandom().RandomRange(count) == 1)
				{
					Chunk[] neighbours = new Chunk[8];
					if (this.ChunkCache.GetNeighborChunks(chunk, neighbours))
					{
						int num;
						int num2;
						int num3;
						if (chunk.FindRandomTopSoilPoint(this, out num, out num2, out num3, 5))
						{
							array[array.Length - _count] = new Vector3((float)num, (float)num2, (float)num3);
							_count--;
						}
						if (_count == 0)
						{
							break;
						}
					}
				}
			}
		}
		return array;
	}

	public Vector3 ClipBoundsMove(Entity _entity, Bounds _aabb, Vector3 move, Vector3 expandDir, float stepHeight)
	{
		if (stepHeight > 0f)
		{
			move.y = stepHeight;
		}
		Bounds bounds = BoundsUtils.ExpandDirectional(_aabb, expandDir);
		int num = Utils.Fastfloor(bounds.min.x - 0.5f);
		int num2 = Utils.Fastfloor(bounds.max.x + 1.5f);
		int num3 = Utils.Fastfloor(bounds.min.y - 0.5f);
		int num4 = Utils.Fastfloor(bounds.max.y + 1f);
		int num5 = Utils.Fastfloor(bounds.min.z - 0.5f);
		int num6 = Utils.Fastfloor(bounds.max.z + 1.5f);
		World.ClipBlock.ResetStorage();
		int num7 = 0;
		int num8 = 0;
		Chunk chunk = null;
		Vector3 blockPos = default(Vector3);
		for (int i = num; i < num2; i++)
		{
			blockPos.x = (float)i;
			int j = num5;
			while (j < num6)
			{
				blockPos.z = (float)j;
				if (chunk != null && chunk.X == World.toChunkXZ(i) && chunk.Z == World.toChunkXZ(j))
				{
					goto IL_147;
				}
				chunk = (Chunk)this.GetChunkFromWorldPos(i, 64, j);
				if (chunk != null)
				{
					if (!this.IsInPlayfield(chunk))
					{
						this._clipBounds[num8++] = chunk.GetAABB();
						goto IL_147;
					}
					goto IL_147;
				}
				IL_1B2:
				j++;
				continue;
				IL_147:
				for (int k = num3; k < num4; k++)
				{
					if (k > 0 && k < 256)
					{
						BlockValue block = this.GetBlock(i, k, j);
						Block block2 = block.Block;
						if (block2.IsCollideMovement)
						{
							float yDistort = 0f;
							blockPos.y = (float)k;
							this._clipBlocks[num7++] = World.ClipBlock.New(block, block2, yDistort, blockPos, _aabb);
						}
					}
				}
				goto IL_1B2;
			}
		}
		Vector3 min = _aabb.min;
		Vector3 max = _aabb.max;
		if (move.y != 0f && num7 > 0)
		{
			for (int l = 0; l < num7; l++)
			{
				World.ClipBlock clipBlock = this._clipBlocks[l];
				IList<Bounds> clipBoundsList = clipBlock.block.GetClipBoundsList(clipBlock.value, clipBlock.pos);
				move.y = BoundsUtils.ClipBoundsMoveY(clipBlock.bmins, clipBlock.bmaxs, move.y, clipBoundsList, clipBoundsList.Count);
				if (move.y == 0f)
				{
					break;
				}
			}
		}
		if (move.y != 0f)
		{
			if (num8 > 0)
			{
				move.y = BoundsUtils.ClipBoundsMoveY(min, max, move.y, this._clipBounds, num8);
			}
			min.y += move.y;
			max.y += move.y;
			for (int m = 0; m < num7; m++)
			{
				World.ClipBlock clipBlock2 = this._clipBlocks[m];
				clipBlock2.bmins.y = clipBlock2.bmins.y + move.y;
				clipBlock2.bmaxs.y = clipBlock2.bmaxs.y + move.y;
			}
		}
		if (move.x != 0f && num7 > 0)
		{
			for (int n = 0; n < num7; n++)
			{
				World.ClipBlock clipBlock3 = this._clipBlocks[n];
				IList<Bounds> clipBoundsList2 = clipBlock3.block.GetClipBoundsList(clipBlock3.value, clipBlock3.pos);
				move.x = BoundsUtils.ClipBoundsMoveX(clipBlock3.bmins, clipBlock3.bmaxs, move.x, clipBoundsList2, clipBoundsList2.Count);
				if (move.x == 0f)
				{
					break;
				}
			}
		}
		if (move.x != 0f)
		{
			if (num8 > 0)
			{
				move.x = BoundsUtils.ClipBoundsMoveX(min, max, move.x, this._clipBounds, num8);
			}
			min.x += move.x;
			max.x += move.x;
			for (int num9 = 0; num9 < num7; num9++)
			{
				World.ClipBlock clipBlock4 = this._clipBlocks[num9];
				clipBlock4.bmins.x = clipBlock4.bmins.x + move.x;
				clipBlock4.bmaxs.x = clipBlock4.bmaxs.x + move.x;
			}
		}
		if (move.z != 0f && num7 > 0)
		{
			for (int num10 = 0; num10 < num7; num10++)
			{
				World.ClipBlock clipBlock5 = this._clipBlocks[num10];
				IList<Bounds> clipBoundsList3 = clipBlock5.block.GetClipBoundsList(clipBlock5.value, clipBlock5.pos);
				move.z = BoundsUtils.ClipBoundsMoveZ(clipBlock5.bmins, clipBlock5.bmaxs, move.z, clipBoundsList3, clipBoundsList3.Count);
				if (move.z == 0f)
				{
					break;
				}
			}
		}
		if (move.z != 0f)
		{
			if (num8 > 0)
			{
				move.z = BoundsUtils.ClipBoundsMoveZ(min, max, move.z, this._clipBounds, num8);
			}
			min.z += move.z;
			max.z += move.z;
			for (int num11 = 0; num11 < num7; num11++)
			{
				World.ClipBlock clipBlock6 = this._clipBlocks[num11];
				clipBlock6.bmins.z = clipBlock6.bmins.z + move.z;
				clipBlock6.bmaxs.z = clipBlock6.bmaxs.z + move.z;
			}
		}
		if (stepHeight > 0f)
		{
			stepHeight = -stepHeight;
			if (num7 > 0)
			{
				for (int num12 = 0; num12 < num7; num12++)
				{
					World.ClipBlock clipBlock7 = this._clipBlocks[num12];
					IList<Bounds> clipBoundsList4 = clipBlock7.block.GetClipBoundsList(clipBlock7.value, clipBlock7.pos);
					stepHeight = BoundsUtils.ClipBoundsMoveY(clipBlock7.bmins, clipBlock7.bmaxs, stepHeight, clipBoundsList4, clipBoundsList4.Count);
					if (stepHeight == 0f)
					{
						break;
					}
				}
			}
			if (stepHeight != 0f && num8 > 0)
			{
				stepHeight = BoundsUtils.ClipBoundsMoveY(min, max, stepHeight, this._clipBounds, num8);
			}
			move.y += stepHeight;
		}
		return move;
	}

	public List<Bounds> GetCollidingBounds(Entity _entity, Bounds _aabb, List<Bounds> collidingBoundingBoxes)
	{
		int num = Utils.Fastfloor(_aabb.min.x - 0.5f);
		int num2 = Utils.Fastfloor(_aabb.max.x + 0.5f);
		int num3 = Utils.Fastfloor(_aabb.min.y - 1f);
		int num4 = Utils.Fastfloor(_aabb.max.y + 1f);
		int num5 = Utils.Fastfloor(_aabb.min.z - 0.5f);
		int num6 = Utils.Fastfloor(_aabb.max.z + 0.5f);
		Chunk chunk = null;
		int i = num - 1;
		int num7 = 0;
		while (i <= num2 + 1)
		{
			if (num7 >= 50)
			{
				Log.Warning(string.Format("1BB exceeded size {0}: BB={1}", 50, _aabb.ToCultureInvariantString()));
				return collidingBoundingBoxes;
			}
			int j = num5 - 1;
			int num8 = 0;
			while (j <= num6 + 1)
			{
				if (num8 >= 50)
				{
					Log.Warning(string.Format("2BB exceeded size {0}: BB={1}", 50, _aabb.ToCultureInvariantString()));
					return collidingBoundingBoxes;
				}
				if (chunk != null && chunk.X == World.toChunkXZ(i) && chunk.Z == World.toChunkXZ(j))
				{
					goto IL_14D;
				}
				chunk = (Chunk)this.GetChunkFromWorldPos(i, 64, j);
				if (chunk != null)
				{
					if (!this.IsInPlayfield(chunk))
					{
						collidingBoundingBoxes.Add(chunk.GetAABB());
						goto IL_14D;
					}
					goto IL_14D;
				}
				IL_1ED:
				j++;
				num8++;
				continue;
				IL_14D:
				int x = World.toBlockXZ(i);
				int z = World.toBlockXZ(j);
				int k = num3;
				int num9 = 0;
				while (k < num4)
				{
					if (k > 0 && k < 255)
					{
						BlockValue block = chunk.GetBlock(x, k, z);
						if (num9 >= 50)
						{
							Log.Warning(string.Format("3BB exceeded size {0}: BB={1}", 50, _aabb.ToCultureInvariantString()));
							return collidingBoundingBoxes;
						}
						this.collBlockCache[num7, num9, num8] = block;
						this.collDensityCache[num7, num9, num8] = chunk.GetDensity(x, k, z);
					}
					k++;
					num9++;
				}
				goto IL_1ED;
			}
			i++;
			num7++;
		}
		int l = num;
		int num10 = 0;
		while (l <= num2)
		{
			if (num10 >= 50)
			{
				Log.Warning(string.Format("4BB exceeded size {0}: BB={1}", 50, _aabb.ToCultureInvariantString()));
				return collidingBoundingBoxes;
			}
			int m = num5;
			int num11 = 0;
			while (m <= num6)
			{
				if (num11 >= 50)
				{
					Log.Warning(string.Format("5BB exceeded size {0}: BB={1}", 50, _aabb.ToCultureInvariantString()));
					return collidingBoundingBoxes;
				}
				int n = num3;
				int num12 = 0;
				while (n < num4)
				{
					if (n > 0 && n < 255)
					{
						if (num12 >= 50)
						{
							Log.Warning(string.Format("6BB exceeded size {0}: BB={1}", 50, _aabb.ToCultureInvariantString()));
							return collidingBoundingBoxes;
						}
						BlockValue blockValue = this.collBlockCache[num10 + 1, num12, num11 + 1];
						Block block2 = blockValue.Block;
						if (block2.IsCollideMovement)
						{
							float distortedAddY = 0f;
							if (block2.shape.IsTerrain())
							{
								distortedAddY = MarchingCubes.GetDecorationOffsetY(this.collDensityCache[num10 + 1, num12 + 1, num11 + 1], this.collDensityCache[num10 + 1, num12, num11 + 1]);
							}
							block2.GetCollidingAABB(blockValue, l, n, m, distortedAddY, _aabb, collidingBoundingBoxes);
						}
					}
					n++;
					num12++;
				}
				m++;
				num11++;
			}
			l++;
			num10++;
		}
		Bounds aabbOfEntity = _aabb;
		aabbOfEntity.Expand(0.25f);
		List<Entity> entitiesInBounds = this.GetEntitiesInBounds(_entity, aabbOfEntity);
		for (int num13 = 0; num13 < entitiesInBounds.Count; num13++)
		{
			Bounds boundingBox = entitiesInBounds[num13].getBoundingBox();
			if (boundingBox.Intersects(_aabb))
			{
				collidingBoundingBoxes.Add(boundingBox);
			}
			boundingBox = _entity.getBoundingBox();
			if (boundingBox.Intersects(_aabb))
			{
				collidingBoundingBoxes.Add(boundingBox);
			}
		}
		return collidingBoundingBoxes;
	}

	public List<Entity> GetEntitiesInBounds(Entity _excludeEntity, Bounds _aabbOfEntity)
	{
		this.entitiesWithinAABBExcludingEntity.Clear();
		int num = Utils.Fastfloor((_aabbOfEntity.min.x - 5f) / 16f);
		int num2 = Utils.Fastfloor((_aabbOfEntity.max.x + 5f) / 16f);
		int num3 = Utils.Fastfloor((_aabbOfEntity.min.z - 5f) / 16f);
		int num4 = Utils.Fastfloor((_aabbOfEntity.max.z + 5f) / 16f);
		for (int i = num; i <= num2; i++)
		{
			for (int j = num3; j <= num4; j++)
			{
				Chunk chunkSync = this.ChunkCache.GetChunkSync(i, j);
				if (chunkSync != null)
				{
					chunkSync.GetEntitiesInBounds(_excludeEntity, _aabbOfEntity, this.entitiesWithinAABBExcludingEntity, true);
				}
			}
		}
		return this.entitiesWithinAABBExcludingEntity;
	}

	public List<Entity> GetEntitiesInBounds(Entity _excludeEntity, Bounds _aabbOfEntity, bool _isAlive)
	{
		this.entitiesWithinAABBExcludingEntity.Clear();
		int num = Utils.Fastfloor((_aabbOfEntity.min.x - 5f) / 16f);
		int num2 = Utils.Fastfloor((_aabbOfEntity.max.x + 5f) / 16f);
		int num3 = Utils.Fastfloor((_aabbOfEntity.min.z - 5f) / 16f);
		int num4 = Utils.Fastfloor((_aabbOfEntity.max.z + 5f) / 16f);
		for (int i = num; i <= num2; i++)
		{
			for (int j = num3; j <= num4; j++)
			{
				Chunk chunkSync = this.ChunkCache.GetChunkSync(i, j);
				if (chunkSync != null)
				{
					chunkSync.GetEntitiesInBounds(_excludeEntity, _aabbOfEntity, this.entitiesWithinAABBExcludingEntity, _isAlive);
				}
			}
		}
		return this.entitiesWithinAABBExcludingEntity;
	}

	public List<EntityAlive> GetLivingEntitiesInBounds(EntityAlive _excludeEntity, Bounds _aabbOfEntity)
	{
		this.livingEntitiesWithinAABBExcludingEntity.Clear();
		int num = Utils.Fastfloor((_aabbOfEntity.min.x - 5f) / 16f);
		int num2 = Utils.Fastfloor((_aabbOfEntity.max.x + 5f) / 16f);
		int num3 = Utils.Fastfloor((_aabbOfEntity.min.z - 5f) / 16f);
		int num4 = Utils.Fastfloor((_aabbOfEntity.max.z + 5f) / 16f);
		for (int i = num; i <= num2; i++)
		{
			for (int j = num3; j <= num4; j++)
			{
				Chunk chunkSync = this.ChunkCache.GetChunkSync(i, j);
				if (chunkSync != null)
				{
					chunkSync.GetLivingEntitiesInBounds(_excludeEntity, _aabbOfEntity, this.livingEntitiesWithinAABBExcludingEntity);
				}
			}
		}
		return this.livingEntitiesWithinAABBExcludingEntity;
	}

	public void GetEntitiesInBounds(FastTags<TagGroup.Global> _tags, Bounds _bb, List<Entity> _list)
	{
		int num = Utils.Fastfloor((_bb.min.x - 5f) / 16f);
		int num2 = Utils.Fastfloor((_bb.max.x + 5f) / 16f);
		int num3 = Utils.Fastfloor((_bb.min.z - 5f) / 16f);
		int num4 = Utils.Fastfloor((_bb.max.z + 5f) / 16f);
		for (int i = num3; i <= num4; i++)
		{
			for (int j = num; j <= num2; j++)
			{
				Chunk chunk = (Chunk)this.GetChunkSync(j, i);
				if (chunk != null)
				{
					chunk.GetEntitiesInBounds(_tags, _bb, _list);
				}
			}
		}
	}

	public List<Entity> GetEntitiesInBounds(Type _class, Bounds _bb, List<Entity> _list)
	{
		int num = Utils.Fastfloor((_bb.min.x - 5f) / 16f);
		int num2 = Utils.Fastfloor((_bb.max.x + 5f) / 16f);
		int num3 = Utils.Fastfloor((_bb.min.z - 5f) / 16f);
		int num4 = Utils.Fastfloor((_bb.max.z + 5f) / 16f);
		for (int i = num3; i <= num4; i++)
		{
			for (int j = num; j <= num2; j++)
			{
				Chunk chunk = (Chunk)this.GetChunkSync(j, i);
				if (chunk != null)
				{
					chunk.GetEntitiesInBounds(_class, _bb, _list);
				}
			}
		}
		return _list;
	}

	public void GetEntitiesAround(EntityFlags _mask, Vector3 _pos, float _radius, List<Entity> _list)
	{
		int num = Utils.Fastfloor((_pos.x - _radius) / 16f);
		int num2 = Utils.Fastfloor((_pos.x + _radius) / 16f);
		int num3 = Utils.Fastfloor((_pos.z - _radius) / 16f);
		int num4 = Utils.Fastfloor((_pos.z + _radius) / 16f);
		for (int i = num3; i <= num4; i++)
		{
			for (int j = num; j <= num2; j++)
			{
				Chunk chunk = (Chunk)this.GetChunkSync(j, i);
				if (chunk != null)
				{
					chunk.GetEntitiesAround(_mask, _pos, _radius, _list);
				}
			}
		}
	}

	public void GetEntitiesAround(EntityFlags _flags, EntityFlags _mask, Vector3 _pos, float _radius, List<Entity> _list)
	{
		_flags &= _mask;
		int num = Utils.Fastfloor((_pos.x - _radius) / 16f);
		int num2 = Utils.Fastfloor((_pos.x + _radius) / 16f);
		int num3 = Utils.Fastfloor((_pos.z - _radius) / 16f);
		int num4 = Utils.Fastfloor((_pos.z + _radius) / 16f);
		for (int i = num3; i <= num4; i++)
		{
			for (int j = num; j <= num2; j++)
			{
				Chunk chunk = (Chunk)this.GetChunkSync(j, i);
				if (chunk != null)
				{
					chunk.GetEntitiesAround(_flags, _mask, _pos, _radius, _list);
				}
			}
		}
	}

	public int GetEntityAliveCount(EntityFlags _flags, EntityFlags _mask)
	{
		int num = 0;
		int count = this.EntityAlives.Count;
		for (int i = 0; i < count; i++)
		{
			if ((this.EntityAlives[i].entityFlags & _mask) == _flags)
			{
				num++;
			}
		}
		return num;
	}

	public void GetPlayersAround(Vector3 _pos, float _radius, List<EntityPlayer> _list)
	{
		float num = _radius * _radius;
		for (int i = this.Players.list.Count - 1; i >= 0; i--)
		{
			EntityPlayer entityPlayer = this.Players.list[i];
			if ((entityPlayer.position - _pos).sqrMagnitude <= num)
			{
				_list.Add(entityPlayer);
			}
		}
	}

	public void SetEntitiesVisibleNearToLocalPlayer()
	{
		EntityPlayerLocal primaryPlayer = this.GetPrimaryPlayer();
		if (primaryPlayer == null)
		{
			return;
		}
		bool aimingGun = primaryPlayer.AimingGun;
		Vector3 b = primaryPlayer.cameraTransform.position + Origin.position;
		for (int i = this.Entities.list.Count - 1; i >= 0; i--)
		{
			Entity entity = this.Entities.list[i];
			if (entity != primaryPlayer)
			{
				entity.VisiblityCheck((entity.position - b).sqrMagnitude, aimingGun);
			}
		}
	}

	public void TickEntities(float _partialTicks)
	{
		int frameCount = Time.frameCount;
		int num = frameCount - this.tickEntityFrameCount;
		if (num <= 0)
		{
			num = 1;
		}
		this.tickEntityFrameCount = frameCount;
		this.tickEntityFrameCountAverage = this.tickEntityFrameCountAverage * 0.8f + (float)num * 0.2f;
		this.tickEntityPartialTicks = _partialTicks;
		this.tickEntityIndex = 0;
		this.tickEntityList.Clear();
		Entity primaryPlayer = this.GetPrimaryPlayer();
		int count = this.Entities.list.Count;
		for (int i = 0; i < count; i++)
		{
			Entity entity = this.Entities.list[i];
			if (entity != primaryPlayer)
			{
				this.tickEntityList.Add(entity);
			}
		}
		if (primaryPlayer)
		{
			this.TickEntity(primaryPlayer, _partialTicks);
		}
		this.EntityActivityUpdate();
		int num2 = (int)(this.tickEntityFrameCountAverage + 0.4f) - 1;
		if (num2 <= 0)
		{
			this.TickEntitiesFlush();
			return;
		}
		int num3 = (this.tickEntityList.Count - 25) / (num2 + 1);
		if (num3 < 0)
		{
			num3 = 0;
		}
		this.tickEntitySliceCount = (this.tickEntityList.Count - num3) / num2 + 1;
	}

	public void TickEntitiesFlush()
	{
		this.TickEntitiesSlice(this.tickEntityList.Count);
	}

	public void TickEntitiesSlice()
	{
		this.TickEntitiesSlice(this.tickEntitySliceCount);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TickEntitiesSlice(int count)
	{
		int num = Utils.FastMin(this.tickEntityIndex + count, this.tickEntityList.Count);
		for (int i = this.tickEntityIndex; i < num; i++)
		{
			Entity entity = this.tickEntityList[i];
			if (entity)
			{
				this.TickEntity(entity, this.tickEntityPartialTicks);
			}
		}
		this.tickEntityIndex = num;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TickEntity(Entity e, float _partialTicks)
	{
		e.SetLastTickPos(e.position);
		e.OnUpdatePosition(_partialTicks);
		e.CheckPosition();
		if (e.IsSpawned() && !e.IsMarkedForUnload())
		{
			Chunk chunk = (Chunk)this.GetChunkSync(e.chunkPosAddedEntityTo.x, e.chunkPosAddedEntityTo.z);
			bool flag = false;
			if (chunk != null)
			{
				if (!chunk.hasEntities)
				{
					flag = true;
				}
				else
				{
					chunk.AdJustEntityTracking(e);
				}
			}
			int num = World.toChunkXZ(Utils.Fastfloor(e.position.x));
			int num2 = World.toChunkXZ(Utils.Fastfloor(e.position.z));
			if (flag || !e.addedToChunk || e.chunkPosAddedEntityTo.x != num || e.chunkPosAddedEntityTo.z != num2)
			{
				if (e.addedToChunk && chunk != null)
				{
					chunk.RemoveEntityFromChunk(e);
				}
				chunk = (Chunk)this.GetChunkSync(num, num2);
				if (chunk != null)
				{
					e.addedToChunk = true;
					chunk.AddEntityToChunk(e);
				}
				else
				{
					e.addedToChunk = false;
				}
			}
			if (e is EntityPlayer || e.IsEntityUpdatedInUnloadedChunk || this.IsChunkAreaLoaded(e.position))
			{
				if (e.CanUpdateEntity())
				{
					e.OnUpdateEntity();
				}
				else if (e is EntityAlive)
				{
					((EntityAlive)e).CheckDespawn();
				}
			}
			else
			{
				EntityAlive entityAlive = e as EntityAlive;
				if (entityAlive != null)
				{
					entityAlive.SetAttackTarget(null, 0);
					entityAlive.CheckDespawn();
				}
			}
		}
		if (e.IsMarkedForUnload() && !e.isEntityRemote && !e.bWillRespawn)
		{
			this.unloadEntity(e, e.IsDespawned ? EnumRemoveEntityReason.Despawned : EnumRemoveEntityReason.Killed);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TickEntityRemove(Entity e)
	{
		int num = this.tickEntityList.IndexOf(e);
		if (num >= this.tickEntityIndex)
		{
			this.tickEntityList[num] = null;
		}
	}

	public void EntityActivityUpdate()
	{
		List<EntityPlayer> list = this.Players.list;
		if (list.Count == 0)
		{
			return;
		}
		for (int i = list.Count - 1; i >= 0; i--)
		{
			list[i].aiClosest.Clear();
		}
		int count = this.EntityAlives.Count;
		for (int j = 0; j < count; j++)
		{
			EntityAlive entityAlive = this.EntityAlives[j];
			EntityPlayer closestPlayer = this.GetClosestPlayer(entityAlive.position, -1f, false);
			if (closestPlayer)
			{
				closestPlayer.aiClosest.Add(entityAlive);
				entityAlive.aiClosestPlayer = closestPlayer;
				entityAlive.aiClosestPlayerDistSq = (closestPlayer.position - entityAlive.position).sqrMagnitude;
			}
			else
			{
				entityAlive.aiClosestPlayer = null;
				entityAlive.aiClosestPlayerDistSq = float.MaxValue;
			}
		}
		Vector3 b = Vector3.zero;
		float num = 0f;
		if (this.m_LocalPlayerEntity)
		{
			b = this.m_LocalPlayerEntity.cameraTransform.position + Origin.position;
			this.m_LocalPlayerEntity.emodel.ClothSimOn(!this.m_LocalPlayerEntity.AttachedToEntity);
			num = 625f;
			if (this.m_LocalPlayerEntity.AimingGun)
			{
				num = 3025f;
			}
		}
		int num2 = Utils.FastClamp(60 / list.Count, 4, 20);
		for (int k = list.Count - 1; k >= 0; k--)
		{
			EntityPlayer entityPlayer = list[k];
			entityPlayer.aiClosest.Sort((EntityAlive e1, EntityAlive e2) => e1.aiClosestPlayerDistSq.CompareTo(e2.aiClosestPlayerDistSq));
			for (int l = 0; l < entityPlayer.aiClosest.Count; l++)
			{
				EntityAlive entityAlive2 = entityPlayer.aiClosest[l];
				if (l < num2 || entityAlive2.aiClosestPlayerDistSq < 64f)
				{
					entityAlive2.aiActiveScale = 1f;
					bool on = entityAlive2.aiClosestPlayerDistSq < 36f;
					entityAlive2.emodel.JiggleOn(on);
				}
				else
				{
					float aiActiveScale = (entityAlive2.aiClosestPlayerDistSq < 225f) ? 0.3f : 0.1f;
					entityAlive2.aiActiveScale = aiActiveScale;
					entityAlive2.emodel.JiggleOn(false);
				}
			}
			if (entityPlayer != this.m_LocalPlayerEntity)
			{
				bool on2 = !entityPlayer.AttachedToEntity && (entityPlayer.position - b).sqrMagnitude < num;
				entityPlayer.emodel.ClothSimOn(on2);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void addToChunk(Entity e)
	{
		if (!e.addedToChunk)
		{
			Chunk chunk = (Chunk)this.GetChunkFromWorldPos(e.GetBlockPosition());
			if (chunk != null)
			{
				chunk.AddEntityToChunk(e);
			}
		}
	}

	public override void UnloadEntities(List<Entity> _entityList)
	{
		for (int i = _entityList.Count - 1; i >= 0; i--)
		{
			Entity entity = _entityList[i];
			if (!entity.bWillRespawn && (!(entity.AttachedMainEntity != null) || !entity.AttachedMainEntity.bWillRespawn))
			{
				this.unloadEntity(entity, EnumRemoveEntityReason.Unloaded);
			}
		}
	}

	public override Entity RemoveEntity(int _entityId, EnumRemoveEntityReason _reason)
	{
		Entity entity = this.GetEntity(_entityId);
		if (entity != null)
		{
			entity.MarkToUnload();
			this.unloadEntity(entity, _reason);
		}
		return entity;
	}

	public void unloadEntity(Entity _e, EnumRemoveEntityReason _reason)
	{
		EnumRemoveEntityReason unloadReason = _e.unloadReason;
		_e.unloadReason = _reason;
		if (!this.Entities.dict.ContainsKey(_e.entityId))
		{
			Log.Warning("{0} World unloadEntity !dict {1}, {2}, was {3}", new object[]
			{
				GameManager.frameCount,
				_e,
				_reason,
				unloadReason
			});
			return;
		}
		if (this.EntityUnloadedDelegates != null)
		{
			this.EntityUnloadedDelegates(_e, _reason);
		}
		if (_e.NavObject != null)
		{
			if (_reason == EnumRemoveEntityReason.Unloaded && _e is EntitySupplyCrate)
			{
				_e.NavObject.PauseEntityUpdate();
			}
			else
			{
				NavObjectManager.Instance.UnRegisterNavObject(_e.NavObject);
			}
		}
		_e.OnEntityUnload();
		this.Entities.Remove(_e.entityId);
		this.TickEntityRemove(_e);
		EntityAlive entityAlive = _e as EntityAlive;
		if (entityAlive)
		{
			this.EntityAlives.Remove(entityAlive);
		}
		this.RemoveEntityFromMap(_e, _reason);
		if (_e.addedToChunk && _e.IsMarkedForUnload())
		{
			Chunk chunk = (Chunk)this.GetChunkSync(_e.chunkPosAddedEntityTo.x, _e.chunkPosAddedEntityTo.z);
			if (chunk != null && !chunk.InProgressUnloading)
			{
				chunk.RemoveEntityFromChunk(_e);
			}
		}
		if (!this.IsRemote())
		{
			if (VehicleManager.Instance != null)
			{
				EntityVehicle entityVehicle = _e as EntityVehicle;
				if (entityVehicle)
				{
					VehicleManager.Instance.RemoveTrackedVehicle(entityVehicle, _reason);
				}
			}
			if (DroneManager.Instance != null)
			{
				EntityDrone entityDrone = _e as EntityDrone;
				if (entityDrone)
				{
					DroneManager.Instance.RemoveTrackedDrone(entityDrone, _reason);
				}
			}
			if (TurretTracker.Instance != null)
			{
				EntityTurret entityTurret = _e as EntityTurret;
				if (entityTurret)
				{
					TurretTracker.Instance.RemoveTrackedTurret(entityTurret, _reason);
				}
			}
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			this.entityDistributer.Remove(_e, _reason);
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && _e is EntityAlive && PathFinderThread.Instance != null)
		{
			PathFinderThread.Instance.RemovePathsFor(_e.entityId);
		}
		if (_e is EntityPlayer)
		{
			this.Players.Remove(_e.entityId);
			this.gameManager.HandlePersistentPlayerDisconnected(_e.entityId);
			this.playerEntityUpdateCount++;
			NavObjectManager.Instance.UnRegisterNavObjectByOwnerEntity(_e, "sleeping_bag");
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			this.aiDirector.RemoveEntity(_e);
		}
		this.audioManager.EntityRemovedFromWorld(_e, this);
		WeatherManager.EntityRemovedFromWorld(_e);
		LightManager.EntityRemovedFromWorld(_e, this);
	}

	public override Entity GetEntity(int _entityId)
	{
		Entity result;
		this.Entities.dict.TryGetValue(_entityId, out result);
		return result;
	}

	public override void ChangeClientEntityIdToServer(int _clientEntityId, int _serverEntityId)
	{
		Entity entity = this.GetEntity(_clientEntityId);
		if (entity)
		{
			this.Entities.Remove(_clientEntityId);
			entity.entityId = _serverEntityId;
			entity.clientEntityId = 0;
			this.Entities.Add(_serverEntityId, entity);
		}
	}

	public void SpawnEntityInWorld(Entity _entity)
	{
		if (_entity == null)
		{
			Log.Warning("Ignore spawning of empty entity");
			return;
		}
		if (this.EntityLoadedDelegates != null)
		{
			this.EntityLoadedDelegates(_entity);
		}
		this.AddEntityToMap(_entity);
		this.Entities.Add(_entity.entityId, _entity);
		this.addToChunk(_entity);
		EntityPlayer entityPlayer = _entity as EntityPlayer;
		EntityAlive entityAlive = (!entityPlayer) ? (_entity as EntityAlive) : null;
		if (entityAlive)
		{
			this.EntityAlives.Add(entityAlive);
		}
		if (!this.IsRemote())
		{
			EntityVehicle entityVehicle = _entity as EntityVehicle;
			if (entityVehicle != null && VehicleManager.Instance != null)
			{
				VehicleManager.Instance.AddTrackedVehicle(entityVehicle);
			}
			EntityDrone entityDrone = _entity as EntityDrone;
			if (entityDrone != null)
			{
				if (DroneManager.Instance != null)
				{
					DroneManager.Instance.AddTrackedDrone(entityDrone);
				}
				if (entityDrone.OriginalItemValue == null)
				{
					entityDrone.InitDynamicSpawn();
				}
			}
			EntityTurret entityTurret = _entity as EntityTurret;
			if (entityTurret != null)
			{
				if (TurretTracker.Instance != null)
				{
					TurretTracker.Instance.AddTrackedTurret(entityTurret);
				}
				if (entityTurret.OriginalItemValue.ItemClass == null)
				{
					entityTurret.InitDynamicSpawn();
				}
			}
		}
		if (this.audioManager != null)
		{
			this.audioManager.EntityAddedToWorld(_entity, this);
		}
		WeatherManager.EntityAddedToWorld(_entity);
		LightManager.EntityAddedToWorld(_entity, this);
		_entity.OnAddedToWorld();
		if (_entity.position.y < 1f)
		{
			Log.Warning(string.Concat(new string[]
			{
				"Spawned entity with wrong pos: ",
				(_entity != null) ? _entity.ToString() : null,
				" id=",
				_entity.entityId.ToString(),
				" pos=",
				_entity.position.ToCultureInvariantString()
			}));
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			this.entityDistributer.Add(_entity);
		}
		if (entityPlayer)
		{
			this.Players.Add(_entity.entityId, entityPlayer);
			this.playerEntityUpdateCount++;
		}
		else if (entityAlive)
		{
			entityAlive.Spawned = true;
			GameEventManager.Current.HandleSpawnModifier(entityAlive);
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			this.aiDirector.AddEntity(_entity);
		}
	}

	public void AddEntityToMap(Entity _entity)
	{
		if (_entity == null)
		{
			return;
		}
		if (_entity.HasUIIcon() && _entity.GetMapObjectType() == EnumMapObjectType.Entity)
		{
			if (_entity is EntityVehicle)
			{
				EntityPlayerLocal primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
				if (primaryPlayer != null)
				{
					LocalPlayerUI uiforPlayer = LocalPlayerUI.GetUIForPlayer(primaryPlayer);
					if (uiforPlayer != null && uiforPlayer.xui != null && uiforPlayer.xui.GetWindow("mapArea") != null)
					{
						((XUiC_MapArea)uiforPlayer.xui.GetWindow("mapArea").Controller).RemoveVehicleLastKnownWaypoint(_entity as EntityVehicle);
						return;
					}
				}
			}
			else
			{
				if (_entity is EntityEnemy || _entity is EntityEnemyAnimal)
				{
					this.ObjectOnMapAdd(new MapObjectZombie(_entity));
					return;
				}
				if (_entity is EntityAnimal)
				{
					this.ObjectOnMapAdd(new MapObjectAnimal(_entity));
					return;
				}
				this.ObjectOnMapAdd(new MapObject(EnumMapObjectType.Entity, Vector3.zero, (long)_entity.entityId, _entity, false));
			}
		}
	}

	public void RemoveEntityFromMap(Entity _entity, EnumRemoveEntityReason _reason)
	{
		if (_entity == null)
		{
			return;
		}
		EnumMapObjectType mapObjectType = _entity.GetMapObjectType();
		if (mapObjectType == EnumMapObjectType.SupplyDrop)
		{
			if (_reason == EnumRemoveEntityReason.Killed)
			{
				this.ObjectOnMapRemove(_entity.GetMapObjectType(), _entity.entityId);
				return;
			}
		}
		else
		{
			if (_entity is EntityVehicle)
			{
				EntityVehicle entityVehicle = _entity as EntityVehicle;
				EntityPlayerLocal primaryPlayer = this.GetPrimaryPlayer();
				if (primaryPlayer != null)
				{
					LocalPlayerUI uiforPlayer = LocalPlayerUI.GetUIForPlayer(primaryPlayer);
					if (uiforPlayer != null)
					{
						if (_reason == EnumRemoveEntityReason.Unloaded)
						{
							if (entityVehicle.GetOwner() != null && entityVehicle.LocalPlayerIsOwner())
							{
								((XUiC_MapArea)uiforPlayer.xui.GetWindow("mapArea").Controller).CreateVehicleLastKnownWaypoint(entityVehicle);
							}
						}
						else if (_reason == EnumRemoveEntityReason.Killed)
						{
							((XUiC_MapArea)uiforPlayer.xui.GetWindow("mapArea").Controller).RemoveVehicleLastKnownWaypoint(entityVehicle);
						}
					}
				}
			}
			this.ObjectOnMapRemove(mapObjectType, _entity.entityId);
		}
	}

	public void RefreshEntitiesOnMap()
	{
		foreach (Entity entity in this.Entities.list)
		{
			this.RemoveEntityFromMap(entity, EnumRemoveEntityReason.Undef);
			this.AddEntityToMap(entity);
		}
	}

	public void LockAreaMasterChunksAround(Vector3i _blockPos, ulong _worldTimeToLock)
	{
		for (int i = -2; i <= 2; i++)
		{
			for (int j = -2; j <= 2; j++)
			{
				Vector3i vector3i = Chunk.ToAreaMasterChunkPos(new Vector3i(_blockPos.x + i * 80, 0, _blockPos.z + j * 80));
				Chunk chunk = (Chunk)this.GetChunkSync(vector3i.x, vector3i.z);
				if (chunk != null && chunk.GetChunkBiomeSpawnData() != null)
				{
					chunk.isModified |= chunk.GetChunkBiomeSpawnData().DelayAllEnemySpawningUntil(_worldTimeToLock, this.Biomes);
				}
				else
				{
					this.areaMasterChunksToLock[WorldChunkCache.MakeChunkKey(vector3i.x, vector3i.z)] = _worldTimeToLock;
				}
			}
		}
	}

	public bool IsWaterInBounds(Bounds _aabb)
	{
		Vector3 min = _aabb.min;
		Vector3 max = _aabb.max;
		int num = Utils.Fastfloor(min.x);
		int num2 = Utils.Fastfloor(max.x + 1f);
		int num3 = Utils.Fastfloor(min.y);
		int num4 = Utils.Fastfloor(max.y + 1f);
		int num5 = Utils.Fastfloor(min.z);
		int num6 = Utils.Fastfloor(max.z + 1f);
		for (int i = num; i < num2; i++)
		{
			for (int j = num3; j < num4; j++)
			{
				for (int k = num5; k < num6; k++)
				{
					if (this.IsWater(i, j, k))
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	public bool IsMaterialInBounds(Bounds _aabb, MaterialBlock _material)
	{
		int num = Utils.Fastfloor(_aabb.min.x);
		int num2 = Utils.Fastfloor(_aabb.max.x + 1f);
		int num3 = Utils.Fastfloor(_aabb.min.y);
		int num4 = Utils.Fastfloor(_aabb.max.y + 1f);
		int num5 = Utils.Fastfloor(_aabb.min.z);
		int num6 = Utils.Fastfloor(_aabb.max.z + 1f);
		for (int i = num; i < num2; i++)
		{
			for (int j = num3; j < num4; j++)
			{
				for (int k = num5; k < num6; k++)
				{
					if (this.GetBlock(i, j, k).Block.blockMaterial == _material)
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	public Dictionary<Vector3i, float> fallingBlocksHashSet
	{
		get
		{
			return this.fallingBlocksMap;
		}
	}

	public override void AddFallingBlocks(IList<Vector3i> _list)
	{
		for (int i = 0; i < _list.Count; i++)
		{
			this.AddFallingBlock(_list[i], false);
		}
	}

	public void AddFallingBlock(Vector3i _blockPos, bool includeOversized = false)
	{
		if (!this.fallingBlocksMap.ContainsKey(_blockPos))
		{
			BlockValue block = this.GetBlock(_blockPos);
			if (block.ischild || block.Block.StabilityIgnore || block.isair || (!includeOversized && block.Block.isOversized))
			{
				return;
			}
			DynamicMeshManager.AddFallingBlockObserver(_blockPos);
			this.fallingBlocks.Enqueue(_blockPos);
			this.fallingBlocksMap[_blockPos] = Time.time;
		}
	}

	public void LetBlocksFall()
	{
		if (this.fallingBlocks.Count == 0)
		{
			return;
		}
		int num = 0;
		Vector3i zero = Vector3i.zero;
		while (this.fallingBlocks.Count > 0 && num < 2)
		{
			Vector3i vector3i = this.fallingBlocks.Dequeue();
			if (zero.Equals(vector3i))
			{
				this.fallingBlocks.Enqueue(vector3i);
				return;
			}
			this.fallingBlocksMap.Remove(vector3i);
			BlockValue block = this.GetBlock(vector3i.x, vector3i.y, vector3i.z);
			if (!block.isair)
			{
				long texture = this.GetTexture(vector3i.x, vector3i.y, vector3i.z);
				Block block2 = block.Block;
				block2.OnBlockStartsToFall(this, vector3i, block);
				DynamicMeshManager.ChunkChanged(vector3i, -1, block.type);
				if (block2.ShowModelOnFall())
				{
					Vector3 transformPos = new Vector3((float)vector3i.x + 0.5f + this.RandomRange(-0.1f, 0.1f), (float)vector3i.y + 0.5f, (float)vector3i.z + 0.5f + this.RandomRange(-0.1f, 0.1f));
					EntityFallingBlock entity = (EntityFallingBlock)EntityFactory.CreateEntity(EntityClass.FromString("fallingBlock"), -1, block, texture, 1, transformPos, Vector3.zero, -1f, -1, null, -1, "");
					this.SpawnEntityInWorld(entity);
					num++;
				}
			}
		}
	}

	public override IGameManager GetGameManager()
	{
		return this.gameManager;
	}

	public override Manager GetAudioManager()
	{
		return this.audioManager;
	}

	public override AIDirector GetAIDirector()
	{
		return this.aiDirector;
	}

	public override bool IsRemote()
	{
		return !SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer;
	}

	public EntityPlayer GetClosestPlayer(float _x, float _y, float _z, int _notFromThisTeam, double _maxDistance)
	{
		float num = -1f;
		EntityPlayer result = null;
		for (int i = 0; i < this.Players.list.Count; i++)
		{
			EntityPlayer entityPlayer = this.Players.list[i];
			if (!entityPlayer.IsDead() && entityPlayer.Spawned && (_notFromThisTeam == 0 || entityPlayer.TeamNumber != _notFromThisTeam))
			{
				float distanceSq = entityPlayer.GetDistanceSq(new Vector3(_x, _y, _z));
				if ((_maxDistance < 0.0 || (double)distanceSq < _maxDistance * _maxDistance) && (num == -1f || distanceSq < num))
				{
					num = distanceSq;
					result = entityPlayer;
				}
			}
		}
		return result;
	}

	public EntityPlayer GetClosestPlayer(Entity _entity, float _distMax, bool _isDead)
	{
		return this.GetClosestPlayer(_entity.position, _distMax, _isDead);
	}

	public EntityPlayer GetClosestPlayer(Vector3 _pos, float _distMax, bool _isDead)
	{
		if (_distMax < 0f)
		{
			_distMax = float.MaxValue;
		}
		float num = _distMax * _distMax;
		EntityPlayer result = null;
		float num2 = float.MaxValue;
		for (int i = this.Players.list.Count - 1; i >= 0; i--)
		{
			EntityPlayer entityPlayer = this.Players.list[i];
			if (entityPlayer.IsDead() == _isDead && entityPlayer.Spawned)
			{
				float distanceSq = entityPlayer.GetDistanceSq(_pos);
				if (distanceSq < num2 && distanceSq <= num)
				{
					num2 = distanceSq;
					result = entityPlayer;
				}
			}
		}
		return result;
	}

	public EntityPlayer GetClosestPlayerSeen(EntityAlive _entity, float _distMax, float lightMin)
	{
		Vector3 position = _entity.position;
		if (_distMax < 0f)
		{
			_distMax = float.MaxValue;
		}
		float num = _distMax * _distMax;
		EntityPlayer result = null;
		float num2 = float.MaxValue;
		for (int i = this.Players.list.Count - 1; i >= 0; i--)
		{
			EntityPlayer entityPlayer = this.Players.list[i];
			if (!entityPlayer.IsDead() && entityPlayer.Spawned)
			{
				float distanceSq = entityPlayer.GetDistanceSq(position);
				if (distanceSq < num2 && distanceSq <= num && entityPlayer.Stealth.lightLevel >= lightMin && _entity.CanSee(entityPlayer))
				{
					num2 = distanceSq;
					result = entityPlayer;
				}
			}
		}
		return result;
	}

	public bool IsPlayerAliveAndNear(Vector3 _pos, float _distMax)
	{
		float num = _distMax * _distMax;
		for (int i = this.Players.list.Count - 1; i >= 0; i--)
		{
			EntityPlayer entityPlayer = this.Players.list[i];
			if (!entityPlayer.IsDead() && entityPlayer.Spawned && (entityPlayer.position - _pos).sqrMagnitude <= num)
			{
				return true;
			}
		}
		return false;
	}

	public override WorldBlockTicker GetWBT()
	{
		return this.worldBlockTicker;
	}

	public override bool IsOpenSkyAbove(int _clrIdx, int _x, int _y, int _z)
	{
		return this.ChunkClusters[_clrIdx] == null || ((Chunk)this.GetChunkSync(_x >> 4, _z >> 4)).IsOpenSkyAbove(_x & 15, _y, _z & 15);
	}

	public override bool IsEditor()
	{
		return GameManager.Instance.IsEditMode();
	}

	public int GetGameMode()
	{
		return GameStats.GetInt(EnumGameStats.GameModeId);
	}

	public SpawnManagerDynamic GetDynamiceSpawnManager()
	{
		return this.dynamicSpawnManager;
	}

	public override bool CanPlaceLandProtectionBlockAt(Vector3i blockPos, PersistentPlayerData lpRelative)
	{
		if (GameStats.GetInt(EnumGameStats.GameModeId) != 1)
		{
			return true;
		}
		if (this.InBoundsForPlayersPercent(blockPos.ToVector3CenterXZ()) < 0.5f)
		{
			return false;
		}
		this.m_lpChunkList.Clear();
		int num = GameStats.GetInt(EnumGameStats.LandClaimSize) - 1;
		int num2 = GameStats.GetInt(EnumGameStats.LandClaimDeadZone) + num;
		int num3 = num2 / 16 + 1;
		int num4 = num2 / 16 + 1;
		for (int i = -num3; i <= num3; i++)
		{
			int x = blockPos.x + i * 16;
			for (int j = -num4; j <= num4; j++)
			{
				int z = blockPos.z + j * 16;
				Chunk chunk = (Chunk)this.GetChunkFromWorldPos(new Vector3i(x, blockPos.y, z));
				if (chunk != null && !this.m_lpChunkList.Contains(chunk))
				{
					this.m_lpChunkList.Add(chunk);
					if (this.IsLandProtectedBlock(chunk, blockPos, lpRelative, num, num2, true))
					{
						this.m_lpChunkList.Clear();
						return false;
					}
				}
			}
		}
		int num5 = num2 / 2;
		Vector3i other = new Vector3i(num5, num5, num5);
		Vector3i minPos = blockPos - other;
		Vector3i maxPos = blockPos + other;
		if (this.IsWithinTraderArea(minPos, maxPos))
		{
			return false;
		}
		this.m_lpChunkList.Clear();
		return true;
	}

	public bool IsEmptyPosition(Vector3i blockPos)
	{
		if (this.IsWithinTraderArea(blockPos))
		{
			return false;
		}
		if (GameStats.GetInt(EnumGameStats.GameModeId) != 1)
		{
			return true;
		}
		this.m_lpChunkList.Clear();
		int @int = GameStats.GetInt(EnumGameStats.LandClaimSize);
		int num = (@int - 1) / 2;
		int num2 = @int / 16 + 1;
		int num3 = @int / 16 + 1;
		int num4 = blockPos.x - num;
		int num5 = blockPos.z - num;
		for (int i = -num2; i <= num2; i++)
		{
			int x = num4 + i * 16;
			for (int j = -num3; j <= num3; j++)
			{
				int z = num5 + j * 16;
				Chunk chunk = (Chunk)this.GetChunkFromWorldPos(new Vector3i(x, blockPos.y, z));
				if (chunk != null && !this.m_lpChunkList.Contains(chunk))
				{
					this.m_lpChunkList.Add(chunk);
					if (this.IsLandProtectedBlock(chunk, blockPos, null, num, num, false))
					{
						this.m_lpChunkList.Clear();
						return false;
					}
				}
			}
		}
		this.m_lpChunkList.Clear();
		return true;
	}

	public override bool CanPickupBlockAt(Vector3i blockPos, PersistentPlayerData lpRelative)
	{
		return !this.IsWithinTraderArea(blockPos) && this.CanPlaceBlockAt(blockPos, lpRelative, false);
	}

	public override bool CanPlaceBlockAt(Vector3i blockPos, PersistentPlayerData lpRelative, bool traderAllowed = false)
	{
		if (!traderAllowed && this.IsWithinTraderArea(blockPos))
		{
			return false;
		}
		if (this.InBoundsForPlayersPercent(blockPos.ToVector3CenterXZ()) < 0.5f)
		{
			return false;
		}
		if (GameStats.GetInt(EnumGameStats.GameModeId) != 1)
		{
			return true;
		}
		this.m_lpChunkList.Clear();
		int @int = GameStats.GetInt(EnumGameStats.LandClaimSize);
		int num = (@int - 1) / 2;
		int num2 = @int / 16 + 1;
		int num3 = @int / 16 + 1;
		int num4 = blockPos.x - num;
		int num5 = blockPos.z - num;
		for (int i = -num2; i <= num2; i++)
		{
			int x = num4 + i * 16;
			for (int j = -num3; j <= num3; j++)
			{
				int z = num5 + j * 16;
				Chunk chunk = (Chunk)this.GetChunkFromWorldPos(new Vector3i(x, blockPos.y, z));
				if (chunk != null && !this.m_lpChunkList.Contains(chunk))
				{
					this.m_lpChunkList.Add(chunk);
					if (this.IsLandProtectedBlock(chunk, blockPos, lpRelative, num, num, false))
					{
						this.m_lpChunkList.Clear();
						return false;
					}
				}
			}
		}
		this.m_lpChunkList.Clear();
		return true;
	}

	public override float GetLandProtectionHardnessModifier(Vector3i blockPos, EntityAlive lpRelative, PersistentPlayerData ppData)
	{
		if (GameStats.GetInt(EnumGameStats.GameModeId) != 1)
		{
			return 1f;
		}
		if (lpRelative is EntityEnemy || lpRelative == null)
		{
			return 1f;
		}
		float num = 1f;
		BlockValue block = this.GetBlock(blockPos);
		if (!block.Equals(BlockValue.Air))
		{
			num = block.Block.LPHardnessScale;
			if (num == 0f)
			{
				return 1f;
			}
		}
		this.m_lpChunkList.Clear();
		int @int = GameStats.GetInt(EnumGameStats.LandClaimSize);
		int num2 = (@int - 1) / 2;
		int num3 = @int / 16 + 1;
		int num4 = @int / 16 + 1;
		int num5 = blockPos.x - num2;
		int num6 = blockPos.z - num2;
		float num7 = 1f;
		for (int i = -num3; i <= num3; i++)
		{
			int x = num5 + i * 16;
			for (int j = -num4; j <= num4; j++)
			{
				int z = num6 + j * 16;
				Chunk chunk = (Chunk)this.GetChunkFromWorldPos(new Vector3i(x, blockPos.y, z));
				if (chunk != null && !this.m_lpChunkList.Contains(chunk))
				{
					this.m_lpChunkList.Add(chunk);
					float landProtectionHardnessModifier = this.GetLandProtectionHardnessModifier(chunk, blockPos, ppData, num2);
					if (landProtectionHardnessModifier < 1f)
					{
						this.m_lpChunkList.Clear();
						return landProtectionHardnessModifier;
					}
					num7 = Math.Max(num7, landProtectionHardnessModifier);
				}
			}
		}
		this.m_lpChunkList.Clear();
		if (num7 > 1f)
		{
			if (lpRelative is EntityVehicle)
			{
				num7 *= 2f;
			}
			return num7 * num;
		}
		return num7;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public float GetLandProtectionHardnessModifier(Chunk chunk, Vector3i blockPos, PersistentPlayerData lpRelative, int halfClaimSize)
	{
		float num = 1f;
		PersistentPlayerList persistentPlayerList = this.gameManager.GetPersistentPlayerList();
		List<Vector3i> list = chunk.IndexedBlocks["lpblock"];
		if (list != null)
		{
			Vector3i worldPos = chunk.GetWorldPos();
			for (int i = 0; i < list.Count; i++)
			{
				Vector3i vector3i = list[i] + worldPos;
				if (BlockLandClaim.IsPrimary(chunk.GetBlock(list[i])))
				{
					PersistentPlayerData landProtectionBlockOwner = persistentPlayerList.GetLandProtectionBlockOwner(vector3i);
					if (landProtectionBlockOwner != null && (lpRelative == null || (landProtectionBlockOwner != lpRelative && (blockPos == vector3i || landProtectionBlockOwner.ACL == null || !landProtectionBlockOwner.ACL.Contains(lpRelative.PrimaryId)))))
					{
						int num2 = Math.Abs(vector3i.x - blockPos.x);
						int num3 = Math.Abs(vector3i.z - blockPos.z);
						if (num2 <= halfClaimSize && num3 <= halfClaimSize)
						{
							float landProtectionHardnessModifierForPlayer = this.GetLandProtectionHardnessModifierForPlayer(landProtectionBlockOwner);
							if (landProtectionHardnessModifierForPlayer < 1f)
							{
								return landProtectionHardnessModifierForPlayer;
							}
							num = Mathf.Max(num, landProtectionHardnessModifierForPlayer);
							if (lpRelative != null)
							{
								EntityPlayer entityPlayer = this.GetEntity(lpRelative.EntityId) as EntityPlayer;
								num = EffectManager.GetValue(PassiveEffects.LandClaimDamageModifier, entityPlayer.inventory.holdingItemItemValue, num, entityPlayer, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
							}
						}
					}
				}
			}
		}
		return num;
	}

	public override bool IsMyLandProtectedBlock(Vector3i worldBlockPos, PersistentPlayerData lpRelative, bool traderAllowed = false)
	{
		if (GameStats.GetInt(EnumGameStats.GameModeId) != 1)
		{
			return true;
		}
		if (!traderAllowed && this.IsWithinTraderArea(worldBlockPos))
		{
			return false;
		}
		this.m_lpChunkList.Clear();
		int @int = GameStats.GetInt(EnumGameStats.LandClaimSize);
		int num = (@int - 1) / 2;
		int num2 = @int / 16 + 1;
		int num3 = @int / 16 + 1;
		int num4 = worldBlockPos.x - num;
		int num5 = worldBlockPos.z - num;
		for (int i = -num2; i <= num2; i++)
		{
			int x = num4 + i * 16;
			for (int j = -num3; j <= num3; j++)
			{
				int z = num5 + j * 16;
				Chunk chunk = (Chunk)this.GetChunkFromWorldPos(new Vector3i(x, worldBlockPos.y, z));
				if (chunk != null && !this.m_lpChunkList.Contains(chunk))
				{
					this.m_lpChunkList.Add(chunk);
					if (this.IsMyLandClaimInChunk(chunk, worldBlockPos, lpRelative, num, num, false))
					{
						this.m_lpChunkList.Clear();
						return true;
					}
				}
			}
		}
		this.m_lpChunkList.Clear();
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool IsLandProtectedBlock(Chunk chunk, Vector3i blockPos, PersistentPlayerData lpRelative, int claimSize, int deadZone, bool forKeystone)
	{
		PersistentPlayerList persistentPlayerList = this.gameManager.GetPersistentPlayerList();
		List<Vector3i> list = chunk.IndexedBlocks["lpblock"];
		if (list != null)
		{
			Vector3i worldPos = chunk.GetWorldPos();
			for (int i = 0; i < list.Count; i++)
			{
				Vector3i vector3i = list[i] + worldPos;
				if (BlockLandClaim.IsPrimary(chunk.GetBlock(list[i])))
				{
					int num = Math.Abs(vector3i.x - blockPos.x);
					int num2 = Math.Abs(vector3i.z - blockPos.z);
					if (num <= deadZone && num2 <= deadZone)
					{
						PersistentPlayerData landProtectionBlockOwner = persistentPlayerList.GetLandProtectionBlockOwner(vector3i);
						if (landProtectionBlockOwner != null)
						{
							bool flag = this.IsLandProtectionValidForPlayer(landProtectionBlockOwner);
							if (flag && lpRelative != null)
							{
								if (lpRelative == landProtectionBlockOwner)
								{
									flag = false;
								}
								else if (landProtectionBlockOwner.ACL != null && landProtectionBlockOwner.ACL.Contains(lpRelative.PrimaryId))
								{
									flag = (num <= claimSize && num2 <= claimSize && forKeystone);
								}
							}
							if (flag)
							{
								return true;
							}
						}
					}
				}
			}
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool IsMyLandClaimInChunk(Chunk chunk, Vector3i blockPos, PersistentPlayerData lpRelative, int claimSize, int deadZone, bool forKeystone)
	{
		PersistentPlayerList persistentPlayerList = this.gameManager.GetPersistentPlayerList();
		List<Vector3i> list = chunk.IndexedBlocks["lpblock"];
		if (list != null)
		{
			Vector3i worldPos = chunk.GetWorldPos();
			for (int i = 0; i < list.Count; i++)
			{
				Vector3i vector3i = list[i] + worldPos;
				if (BlockLandClaim.IsPrimary(chunk.GetBlock(list[i])))
				{
					int num = Math.Abs(vector3i.x - blockPos.x);
					int num2 = Math.Abs(vector3i.z - blockPos.z);
					if (num <= deadZone && num2 <= deadZone)
					{
						PersistentPlayerData landProtectionBlockOwner = persistentPlayerList.GetLandProtectionBlockOwner(vector3i);
						if (landProtectionBlockOwner != null)
						{
							bool flag = this.IsLandProtectionValidForPlayer(landProtectionBlockOwner);
							if (flag && lpRelative != null)
							{
								if (lpRelative == landProtectionBlockOwner)
								{
									flag = (num <= claimSize && num2 <= claimSize);
								}
								else
								{
									flag = (landProtectionBlockOwner.ACL != null && landProtectionBlockOwner.ACL.Contains(lpRelative.PrimaryId) && (num <= claimSize && num2 <= claimSize && forKeystone));
								}
							}
							if (flag)
							{
								return true;
							}
						}
					}
				}
			}
		}
		return false;
	}

	public override EnumLandClaimOwner GetLandClaimOwner(Vector3i worldBlockPos, PersistentPlayerData lpRelative)
	{
		if (GameStats.GetInt(EnumGameStats.GameModeId) != 1)
		{
			return EnumLandClaimOwner.Self;
		}
		if (this.IsWithinTraderArea(worldBlockPos))
		{
			return EnumLandClaimOwner.None;
		}
		this.m_lpChunkList.Clear();
		int @int = GameStats.GetInt(EnumGameStats.LandClaimSize);
		int num = (@int - 1) / 2;
		int num2 = @int / 16 + 1;
		int num3 = @int / 16 + 1;
		int num4 = worldBlockPos.x - num;
		int num5 = worldBlockPos.z - num;
		for (int i = -num2; i <= num2; i++)
		{
			int x = num4 + i * 16;
			for (int j = -num3; j <= num3; j++)
			{
				int z = num5 + j * 16;
				Chunk chunk = (Chunk)this.GetChunkFromWorldPos(new Vector3i(x, worldBlockPos.y, z));
				if (chunk != null && !this.m_lpChunkList.Contains(chunk))
				{
					this.m_lpChunkList.Add(chunk);
					EnumLandClaimOwner landClaimOwner = this.GetLandClaimOwner(chunk, worldBlockPos, lpRelative, num, num, false);
					if (landClaimOwner != EnumLandClaimOwner.None)
					{
						this.m_lpChunkList.Clear();
						return landClaimOwner;
					}
				}
			}
		}
		this.m_lpChunkList.Clear();
		return EnumLandClaimOwner.None;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public EnumLandClaimOwner GetLandClaimOwner(Chunk chunk, Vector3i blockPos, PersistentPlayerData lpRelative, int claimSize, int deadZone, bool forKeystone)
	{
		EnumLandClaimOwner result = EnumLandClaimOwner.None;
		PersistentPlayerList persistentPlayerList = this.gameManager.GetPersistentPlayerList();
		List<Vector3i> list = chunk.IndexedBlocks["lpblock"];
		if (list != null)
		{
			Vector3i worldPos = chunk.GetWorldPos();
			for (int i = 0; i < list.Count; i++)
			{
				Vector3i vector3i = list[i] + worldPos;
				if (BlockLandClaim.IsPrimary(chunk.GetBlock(list[i])))
				{
					int num = Math.Abs(vector3i.x - blockPos.x);
					int num2 = Math.Abs(vector3i.z - blockPos.z);
					if (num <= deadZone && num2 <= deadZone)
					{
						PersistentPlayerData landProtectionBlockOwner = persistentPlayerList.GetLandProtectionBlockOwner(vector3i);
						if (landProtectionBlockOwner != null && this.IsLandProtectionValidForPlayer(landProtectionBlockOwner) && lpRelative != null)
						{
							if (lpRelative == landProtectionBlockOwner)
							{
								result = EnumLandClaimOwner.Self;
							}
							else if (landProtectionBlockOwner.ACL != null && landProtectionBlockOwner.ACL.Contains(lpRelative.PrimaryId))
							{
								result = EnumLandClaimOwner.Ally;
							}
							else
							{
								result = EnumLandClaimOwner.Other;
							}
						}
					}
				}
			}
		}
		return result;
	}

	public bool GetLandClaimOwnerInParty(EntityPlayer player, PersistentPlayerData lpRelative)
	{
		if (GameStats.GetInt(EnumGameStats.GameModeId) != 1)
		{
			return false;
		}
		Vector3i blockPosition = player.GetBlockPosition();
		if (this.IsWithinTraderArea(blockPosition))
		{
			return false;
		}
		this.m_lpChunkList.Clear();
		int @int = GameStats.GetInt(EnumGameStats.LandClaimSize);
		int num = (@int - 1) / 2;
		int num2 = @int / 16 + 1;
		int num3 = @int / 16 + 1;
		int num4 = blockPosition.x - num;
		int num5 = blockPosition.z - num;
		for (int i = -num2; i <= num2; i++)
		{
			int x = num4 + i * 16;
			for (int j = -num3; j <= num3; j++)
			{
				int z = num5 + j * 16;
				Chunk chunk = (Chunk)this.GetChunkFromWorldPos(new Vector3i(x, blockPosition.y, z));
				if (chunk != null && !this.m_lpChunkList.Contains(chunk))
				{
					this.m_lpChunkList.Add(chunk);
					if (this.GetLandClaimOwnerInParty(chunk, player, blockPosition, lpRelative, num, num))
					{
						this.m_lpChunkList.Clear();
						return true;
					}
				}
			}
		}
		this.m_lpChunkList.Clear();
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool GetLandClaimOwnerInParty(Chunk chunk, EntityPlayer player, Vector3i blockPos, PersistentPlayerData lpRelative, int claimSize, int deadZone)
	{
		PersistentPlayerList persistentPlayerList = this.gameManager.GetPersistentPlayerList();
		bool flag = player.Party != null;
		List<Vector3i> list = chunk.IndexedBlocks["lpblock"];
		if (list != null)
		{
			Vector3i worldPos = chunk.GetWorldPos();
			for (int i = 0; i < list.Count; i++)
			{
				Vector3i vector3i = list[i] + worldPos;
				if (BlockLandClaim.IsPrimary(chunk.GetBlock(list[i])))
				{
					int num = Math.Abs(vector3i.x - blockPos.x);
					int num2 = Math.Abs(vector3i.z - blockPos.z);
					if (num <= deadZone && num2 <= deadZone)
					{
						PersistentPlayerData landProtectionBlockOwner = persistentPlayerList.GetLandProtectionBlockOwner(vector3i);
						if (landProtectionBlockOwner != null)
						{
							if (lpRelative == null && player != null && landProtectionBlockOwner.EntityId == player.entityId)
							{
								lpRelative = landProtectionBlockOwner;
							}
							if (this.IsLandProtectionValidForPlayer(landProtectionBlockOwner) && lpRelative != null)
							{
								if (lpRelative == landProtectionBlockOwner)
								{
									if (num <= claimSize && num2 <= claimSize)
									{
										return true;
									}
								}
								else if (flag && landProtectionBlockOwner.ACL != null && landProtectionBlockOwner.ACL.Contains(lpRelative.PrimaryId) && player.Party.ContainsMember(landProtectionBlockOwner.EntityId) && num <= claimSize && num2 <= claimSize)
								{
									return true;
								}
							}
							return false;
						}
					}
				}
			}
		}
		return false;
	}

	public bool IsLandProtectionValidForPlayer(PersistentPlayerData ppData)
	{
		double num = (double)GameStats.GetInt(EnumGameStats.LandClaimExpiryTime) * 24.0;
		return ppData.OfflineHours <= num;
	}

	public float GetLandProtectionHardnessModifierForPlayer(PersistentPlayerData ppData)
	{
		float result = (float)GameStats.GetInt(EnumGameStats.LandClaimOnlineDurabilityModifier);
		if (ppData.EntityId != -1)
		{
			return result;
		}
		double offlineHours = ppData.OfflineHours;
		double offlineMinutes = ppData.OfflineMinutes;
		float num = (float)GameStats.GetInt(EnumGameStats.LandClaimOfflineDelay);
		if (num != 0f && offlineMinutes <= (double)num)
		{
			return result;
		}
		double num2 = (double)GameStats.GetInt(EnumGameStats.LandClaimExpiryTime) * 24.0;
		if (offlineHours > num2)
		{
			return 1f;
		}
		EnumLandClaimDecayMode @int = (EnumLandClaimDecayMode)GameStats.GetInt(EnumGameStats.LandClaimDecayMode);
		float num3 = (float)GameStats.GetInt(EnumGameStats.LandClaimOfflineDurabilityModifier);
		if (num3 == 0f)
		{
			return 0f;
		}
		if (@int == EnumLandClaimDecayMode.DecaySlowly)
		{
			double num4 = (offlineHours - 24.0) / (num2 - 24.0);
			return Mathf.Max(1f, (float)(1.0 - num4) * num3);
		}
		if (@int == EnumLandClaimDecayMode.BuffedUntilExpired)
		{
			return num3;
		}
		double num5 = (offlineHours - 24.0) / (num2 - 24.0);
		return Mathf.Max(1f, (float)((1.0 - num5) * (1.0 - num5)) * num3);
	}

	public float GetDecorationOffsetY(Vector3i _blockPos)
	{
		sbyte density = this.GetDensity(0, _blockPos);
		sbyte density2 = this.GetDensity(0, _blockPos - Vector3i.up);
		return MarchingCubes.GetDecorationOffsetY(density, density2);
	}

	public EnumDecoAllowed GetDecoAllowedAt(int _x, int _z)
	{
		Chunk chunk = (Chunk)this.GetChunkFromWorldPos(_x, 0, _z);
		if (chunk != null)
		{
			return chunk.GetDecoAllowedAt(World.toBlockXZ(_x), World.toBlockXZ(_z));
		}
		return EnumDecoAllowed.Nothing;
	}

	public void SetDecoAllowedAt(int _x, int _z, EnumDecoAllowed _decoAllowed)
	{
		Chunk chunk = (Chunk)this.GetChunkFromWorldPos(_x, 0, _z);
		if (chunk != null)
		{
			chunk.SetDecoAllowedAt(World.toBlockXZ(_x), World.toBlockXZ(_z), _decoAllowed);
		}
	}

	public Vector3 GetTerrainNormalAt(int _x, int _z)
	{
		Chunk chunk = (Chunk)this.GetChunkFromWorldPos(_x, 0, _z);
		if (chunk != null)
		{
			return chunk.GetTerrainNormal(World.toBlockXZ(_x), World.toBlockXZ(_z));
		}
		return Vector3.zero;
	}

	public bool GetWorldExtent(out Vector3i _minSize, out Vector3i _maxSize)
	{
		return this.ChunkCache.ChunkProvider.GetWorldExtent(out _minSize, out _maxSize);
	}

	public virtual bool IsPositionAvailable(int _clrIdx, Vector3 _position)
	{
		ChunkCluster chunkCluster = this.ChunkClusters[_clrIdx];
		if (chunkCluster == null)
		{
			return false;
		}
		Vector3i one = World.worldToBlockPos(_position);
		for (int i = 0; i < Vector3i.MIDDLE_AND_HORIZONTAL_DIRECTIONS_DIAGONAL.Length; i++)
		{
			Vector3i other = Vector3i.MIDDLE_AND_HORIZONTAL_DIRECTIONS_DIAGONAL[i] * 16;
			IChunk chunkFromWorldPos = chunkCluster.GetChunkFromWorldPos(one + other);
			if (chunkFromWorldPos == null || !chunkFromWorldPos.GetAvailable())
			{
				return false;
			}
		}
		return true;
	}

	public bool GetBiomeIntensity(Vector3i _position, out BiomeIntensity _biomeIntensity)
	{
		Chunk chunk = (Chunk)this.GetChunkFromWorldPos(_position);
		if (chunk != null && !chunk.NeedsLightCalculation)
		{
			_biomeIntensity = chunk.GetBiomeIntensity(World.toBlockXZ(_position.x), World.toBlockXZ(_position.z));
			return true;
		}
		_biomeIntensity = BiomeIntensity.Default;
		return false;
	}

	public bool CanMobsSpawnAtPos(Vector3 _pos)
	{
		Vector3i vector3i = World.worldToBlockPos(_pos);
		Chunk chunk = (Chunk)this.GetChunkFromWorldPos(vector3i);
		return chunk != null && chunk.CanMobsSpawnAtPos(World.toBlockXZ(vector3i.x), World.toBlockY(vector3i.y), World.toBlockXZ(vector3i.z), false, true);
	}

	public bool CanPlayersSpawnAtPos(Vector3 _pos, bool _bAllowToSpawnOnAirPos = false)
	{
		Vector3i vector3i = World.worldToBlockPos(_pos);
		Chunk chunk = (Chunk)this.GetChunkFromWorldPos(vector3i);
		return chunk != null && chunk.CanPlayersSpawnAtPos(World.toBlockXZ(vector3i.x), World.toBlockY(vector3i.y), World.toBlockXZ(vector3i.z), _bAllowToSpawnOnAirPos);
	}

	public void CheckEntityCollisionWithBlocks(Entity _entity)
	{
		if (!_entity.CanCollideWithBlocks())
		{
			return;
		}
		for (int i = 0; i < this.ChunkClusters.Count; i++)
		{
			ChunkCluster chunkCluster = this.ChunkClusters[i];
			if (chunkCluster != null && chunkCluster.Overlaps(_entity.boundingBox))
			{
				chunkCluster.CheckCollisionWithBlocks(_entity);
			}
		}
	}

	public void OnChunkAdded(Chunk _c)
	{
		List<long> obj = this.newlyLoadedChunksThisUpdate;
		lock (obj)
		{
			this.newlyLoadedChunksThisUpdate.Add(_c.Key);
		}
	}

	public void OnChunkBeforeRemove(Chunk _c)
	{
		List<long> obj = this.newlyLoadedChunksThisUpdate;
		lock (obj)
		{
			this.newlyLoadedChunksThisUpdate.Remove(_c.Key);
		}
		if (this.worldBlockTicker != null)
		{
			this.worldBlockTicker.OnChunkRemoved(_c);
		}
		GameManager.Instance.prefabLODManager.TriggerUpdate();
	}

	public void OnChunkBeforeSave(Chunk _c)
	{
		if (this.worldBlockTicker != null)
		{
			this.worldBlockTicker.OnChunkBeforeSave(_c);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateChunkAddedRemovedCallbacks()
	{
		List<long> obj = this.newlyLoadedChunksThisUpdate;
		lock (obj)
		{
			int num = 0;
			ChunkCluster chunkCluster = this.ChunkClusters[num];
			for (int i = this.newlyLoadedChunksThisUpdate.Count - 1; i >= 0; i--)
			{
				long key = this.newlyLoadedChunksThisUpdate[i];
				int num2 = WorldChunkCache.extractClrIdx(key);
				if (num2 != num)
				{
					num = num2;
					chunkCluster = this.ChunkClusters[num];
				}
				if (chunkCluster != null)
				{
					Chunk chunkSync = chunkCluster.GetChunkSync(key);
					if (chunkSync != null && !chunkSync.NeedsDecoration)
					{
						chunkSync.OnLoad(this);
						if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
						{
							this.worldBlockTicker.OnChunkAdded(this, chunkSync, this.rand);
						}
						this.newlyLoadedChunksThisUpdate.RemoveAt(i);
					}
				}
			}
		}
	}

	public override ulong GetWorldTime()
	{
		return this.worldTime;
	}

	public override WorldCreationData GetWorldCreationData()
	{
		return this.wcd;
	}

	public bool IsEntityInRange(int _entityId, int _refEntity, int _range)
	{
		Entity entity;
		Entity other;
		return _entityId == _refEntity || (this.Entities.dict.TryGetValue(_entityId, out entity) && this.Entities.dict.TryGetValue(_refEntity, out other) && entity.GetDistanceSq(other) <= (float)(_range * _range));
	}

	public bool IsEntityInRange(int _entityId, Vector3 _position, int _range)
	{
		Entity entity;
		return this.Entities.dict.TryGetValue(_entityId, out entity) && entity.GetDistanceSq(_position) <= (float)(_range * _range);
	}

	public bool IsPositionInBounds(Vector3 position)
	{
		Vector3i vector3i;
		Vector3i vector3i2;
		this.GetWorldExtent(out vector3i, out vector3i2);
		if (GamePrefs.GetString(EnumGamePrefs.GameWorld) == "Navezgane")
		{
			vector3i = new Vector3i(-2400, vector3i.y, -2400);
			vector3i2 = new Vector3i(2400, vector3i2.y, 2400);
		}
		else if (!GameUtils.IsPlaytesting())
		{
			vector3i = new Vector3i(vector3i.x + 320, vector3i.y, vector3i.z + 320);
			vector3i2 = new Vector3i(vector3i2.x - 320, vector3i2.y, vector3i2.z - 320);
		}
		Vector3Int vector3Int = vector3i;
		Vector3Int a = vector3i2;
		BoundsInt boundsInt = new BoundsInt(vector3Int, a - vector3Int);
		return boundsInt.Contains(Vector3Int.RoundToInt(position));
	}

	public float InBoundsForPlayersPercent(Vector3 _pos)
	{
		Vector3i vector3i;
		Vector3i vector3i2;
		this.GetWorldExtent(out vector3i, out vector3i2);
		if (vector3i2.x - vector3i.x < 1024)
		{
			return 1f;
		}
		Vector2 vector;
		vector.x = (float)(vector3i.x + vector3i2.x) * 0.5f;
		vector.y = (float)(vector3i.z + vector3i2.z) * 0.5f;
		float num;
		if (_pos.x < vector.x)
		{
			num = (_pos.x - ((float)vector3i.x + 50f)) / 80f;
		}
		else
		{
			num = ((float)vector3i2.x - 50f - _pos.x) / 80f;
		}
		num = Utils.FastClamp01(num);
		float num2;
		if (_pos.z < vector.y)
		{
			num2 = (_pos.z - ((float)vector3i.z + 50f)) / 80f;
		}
		else
		{
			num2 = ((float)vector3i2.z - 50f - _pos.z) / 80f;
		}
		num2 = Utils.FastClamp01(num2);
		return Utils.FastMin(num, num2);
	}

	public bool AdjustBoundsForPlayers(ref Vector3 _pos, float _padPercent)
	{
		Vector3i vector3i;
		Vector3i vector3i2;
		this.GetWorldExtent(out vector3i, out vector3i2);
		if (vector3i2.x - vector3i.x < 1024)
		{
			return false;
		}
		if (vector3i2.x == 0)
		{
			return false;
		}
		int num = (int)(50f + 80f * _padPercent);
		vector3i.x += num;
		vector3i.z += num;
		vector3i2.x -= num;
		vector3i2.z -= num;
		bool result = false;
		if (_pos.x < (float)vector3i.x)
		{
			_pos.x = (float)vector3i.x;
			result = true;
		}
		else if (_pos.x > (float)vector3i2.x)
		{
			_pos.x = (float)vector3i2.x;
			result = true;
		}
		if (_pos.z < (float)vector3i.z)
		{
			_pos.z = (float)vector3i.z;
			result = true;
		}
		else if (_pos.z > (float)vector3i2.z)
		{
			_pos.z = (float)vector3i2.z;
			result = true;
		}
		return result;
	}

	public bool IsPositionRadiated(Vector3 position)
	{
		IChunkProvider chunkProvider = this.ChunkCache.ChunkProvider;
		IBiomeProvider biomeProvider;
		return chunkProvider != null && (biomeProvider = chunkProvider.GetBiomeProvider()) != null && biomeProvider.GetRadiationAt((int)position.x, (int)position.z) > 0f;
	}

	public bool IsPositionWithinPOI(Vector3 position, int offset)
	{
		return this.ChunkCache.ChunkProvider.GetDynamicPrefabDecorator().GetPrefabFromWorldPosInsideWithOffset((int)position.x, (int)position.z, offset) != null;
	}

	public PrefabInstance GetPOIAtPosition(Vector3 _position, bool _checkTags = true)
	{
		DynamicPrefabDecorator dynamicPrefabDecorator = this.ChunkCache.ChunkProvider.GetDynamicPrefabDecorator();
		if (dynamicPrefabDecorator == null)
		{
			return null;
		}
		return dynamicPrefabDecorator.GetPrefabAtPosition(_position, _checkTags);
	}

	public void GetPOIsAtXZ(int _xMin, int _xMax, int _zMin, int _zMax, List<PrefabInstance> _list)
	{
		DynamicPrefabDecorator dynamicPrefabDecorator = this.ChunkCache.ChunkProvider.GetDynamicPrefabDecorator();
		if (dynamicPrefabDecorator != null)
		{
			dynamicPrefabDecorator.GetPrefabsAtXZ(_xMin, _xMax, _zMin, _zMax, _list);
		}
	}

	public Vector3 ClampToValidWorldPos(Vector3 position)
	{
		Vector3i vector3i;
		Vector3i vector3i2;
		this.GetWorldExtent(out vector3i, out vector3i2);
		if (GamePrefs.GetString(EnumGamePrefs.GameWorld) == "Navezgane")
		{
			vector3i = new Vector3i(-2400, vector3i.y, -2400);
			vector3i2 = new Vector3i(2400, vector3i2.y, 2400);
		}
		else if (!GameUtils.IsPlaytesting())
		{
			vector3i = new Vector3i(vector3i.x + 320, vector3i.y, vector3i.z + 320);
			vector3i2 = new Vector3i(vector3i2.x - 320, vector3i2.y, vector3i2.z - 320);
		}
		float x = Mathf.Clamp(position.x, (float)vector3i.x, (float)vector3i2.x);
		float y = Mathf.Clamp(position.y, (float)vector3i.y, (float)vector3i2.y);
		float z = Mathf.Clamp(position.z, (float)vector3i.z, (float)vector3i2.z);
		return new Vector3(x, y, z);
	}

	public Vector3 ClampToValidWorldPosForMap(Vector2 position)
	{
		Vector3i vector3i;
		Vector3i vector3i2;
		this.GetWorldExtent(out vector3i, out vector3i2);
		if (GamePrefs.GetString(EnumGamePrefs.GameWorld) == "Navezgane")
		{
			vector3i = new Vector3i(-2550, vector3i.y, -2550);
			vector3i2 = new Vector3i(2550, vector3i2.y, 2550);
		}
		float x = Mathf.Clamp(position.x, (float)vector3i.x, (float)vector3i2.x);
		float y = Mathf.Clamp(position.y, (float)vector3i.z, (float)vector3i2.z);
		return new Vector2(x, y);
	}

	public void ObjectOnMapAdd(MapObject _mo)
	{
		if (this.objectsOnMap != null)
		{
			this.objectsOnMap.Add(_mo);
		}
	}

	public void ObjectOnMapRemove(EnumMapObjectType _type, int _key)
	{
		if (this.objectsOnMap != null)
		{
			this.objectsOnMap.Remove(_type, _key);
		}
	}

	public void ObjectOnMapRemove(EnumMapObjectType _type, Vector3 _position)
	{
		if (this.objectsOnMap != null)
		{
			this.objectsOnMap.RemoveByPosition(_type, _position);
		}
	}

	public void ObjectOnMapRemove(EnumMapObjectType _type)
	{
		if (this.objectsOnMap != null)
		{
			this.objectsOnMap.RemoveByType(_type);
		}
	}

	public List<MapObject> GetObjectOnMapList(EnumMapObjectType _type)
	{
		if (this.objectsOnMap != null)
		{
			return this.objectsOnMap.GetList(_type);
		}
		return new List<MapObject>();
	}

	public void DebugAddSpawnedEntity(Entity entity)
	{
		if (this.GetPrimaryPlayer() == null || !(entity is EntityAlive))
		{
			return;
		}
		EntityAlive entityAlive = (EntityAlive)entity;
		SSpawnedEntity item = default(SSpawnedEntity);
		item.distanceToLocalPlayer = (entityAlive.GetPosition() - this.GetPrimaryPlayer().GetPosition()).magnitude;
		item.name = entityAlive.EntityName;
		item.pos = entityAlive.GetPosition();
		item.timeSpawned = Time.time;
		this.Last4Spawned.Add(item);
		if (this.Last4Spawned.Count > 4)
		{
			this.Last4Spawned.RemoveAt(0);
		}
	}

	public static void SetWorldAreas(List<TraderArea> _traders)
	{
		World.traderAreas = _traders;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetupTraders()
	{
		if (World.traderAreas != null)
		{
			DynamicPrefabDecorator dynamicPrefabDecorator = this.ChunkCache.ChunkProvider.GetDynamicPrefabDecorator();
			if (dynamicPrefabDecorator != null)
			{
				dynamicPrefabDecorator.ClearTraders();
				for (int i = 0; i < World.traderAreas.Count; i++)
				{
					dynamicPrefabDecorator.AddTrader(World.traderAreas[i]);
				}
			}
			World.traderAreas = null;
		}
	}

	public List<TraderArea> TraderAreas
	{
		get
		{
			DynamicPrefabDecorator dynamicPrefabDecorator = this.ChunkCache.ChunkProvider.GetDynamicPrefabDecorator();
			if (dynamicPrefabDecorator == null)
			{
				return null;
			}
			return dynamicPrefabDecorator.GetTraderAreas();
		}
	}

	public bool IsWithinTraderArea(Vector3i _worldBlockPos)
	{
		return this.GetTraderAreaAt(_worldBlockPos) != null;
	}

	public bool IsWithinTraderPlacingProtection(Vector3i _worldBlockPos)
	{
		DynamicPrefabDecorator dynamicPrefabDecorator = this.ChunkCache.ChunkProvider.GetDynamicPrefabDecorator();
		return dynamicPrefabDecorator != null && dynamicPrefabDecorator.GetTraderAtPosition(_worldBlockPos, 2) != null;
	}

	public bool IsWithinTraderPlacingProtection(Bounds _bounds)
	{
		DynamicPrefabDecorator dynamicPrefabDecorator = this.ChunkCache.ChunkProvider.GetDynamicPrefabDecorator();
		if (dynamicPrefabDecorator != null)
		{
			_bounds.Expand(4f);
			Vector3i minPos = World.worldToBlockPos(_bounds.min);
			Vector3i maxPos = World.worldToBlockPos(_bounds.max);
			return dynamicPrefabDecorator.IsWithinTraderArea(minPos, maxPos);
		}
		return false;
	}

	public bool IsWithinTraderArea(Vector3i _minPos, Vector3i _maxPos)
	{
		DynamicPrefabDecorator dynamicPrefabDecorator = this.ChunkCache.ChunkProvider.GetDynamicPrefabDecorator();
		return dynamicPrefabDecorator != null && dynamicPrefabDecorator.IsWithinTraderArea(_minPos, _maxPos);
	}

	public TraderArea GetTraderAreaAt(Vector3i _pos)
	{
		DynamicPrefabDecorator dynamicPrefabDecorator = this.ChunkCache.ChunkProvider.GetDynamicPrefabDecorator();
		if (dynamicPrefabDecorator != null)
		{
			return dynamicPrefabDecorator.GetTraderAtPosition(_pos, 0);
		}
		return null;
	}

	public override int AddSleeperVolume(SleeperVolume _sleeperVolume)
	{
		List<SleeperVolume> obj = this.sleeperVolumes;
		int result;
		lock (obj)
		{
			this.sleeperVolumes.Add(_sleeperVolume);
			List<int> list;
			if (!this.sleeperVolumeMap.TryGetValue(_sleeperVolume.BoxMin, out list))
			{
				list = new List<int>();
				this.sleeperVolumeMap.Add(_sleeperVolume.BoxMin, list);
			}
			list.Add(this.sleeperVolumes.Count - 1);
			result = this.sleeperVolumes.Count - 1;
		}
		return result;
	}

	public override int FindSleeperVolume(Vector3i mins, Vector3i maxs)
	{
		List<int> list;
		if (this.sleeperVolumeMap.TryGetValue(mins, out list))
		{
			for (int i = 0; i < list.Count; i++)
			{
				int num = list[i];
				if (this.sleeperVolumes[num].BoxMax == maxs)
				{
					return num;
				}
			}
		}
		return -1;
	}

	public override int GetSleeperVolumeCount()
	{
		return this.sleeperVolumes.Count;
	}

	public override SleeperVolume GetSleeperVolume(int index)
	{
		return this.sleeperVolumes[index];
	}

	public void CheckSleeperVolumeTouching(EntityPlayer _player)
	{
		if (!GameStats.GetBool(EnumGameStats.IsSpawnEnemies))
		{
			return;
		}
		Vector3i blockPosition = _player.GetBlockPosition();
		Chunk chunk = (Chunk)this.GetChunkFromWorldPos(blockPosition);
		if (chunk != null)
		{
			List<int> list = chunk.GetSleeperVolumes();
			List<SleeperVolume> obj = this.sleeperVolumes;
			lock (obj)
			{
				for (int i = 0; i < list.Count; i++)
				{
					int num = list[i];
					if (num < this.sleeperVolumes.Count)
					{
						this.sleeperVolumes[num].CheckTouching(this, _player);
					}
				}
			}
		}
	}

	public void CheckSleeperVolumeNoise(Vector3 position)
	{
		if (!GameStats.GetBool(EnumGameStats.IsSpawnEnemies))
		{
			return;
		}
		position.y += 0.1f;
		Chunk chunk = (Chunk)this.GetChunkFromWorldPos(World.worldToBlockPos(position));
		if (chunk != null)
		{
			List<int> list = chunk.GetSleeperVolumes();
			List<SleeperVolume> obj = this.sleeperVolumes;
			lock (obj)
			{
				for (int i = 0; i < list.Count; i++)
				{
					int num = list[i];
					if (num < this.sleeperVolumes.Count)
					{
						this.sleeperVolumes[num].CheckNoise(this, position);
					}
				}
			}
		}
	}

	public void WriteSleeperVolumes(BinaryWriter _bw)
	{
		_bw.Write(this.sleeperVolumes.Count);
		for (int i = 0; i < this.sleeperVolumes.Count; i++)
		{
			this.sleeperVolumes[i].Write(_bw);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ReadSleeperVolumes(BinaryReader _br)
	{
		this.sleeperVolumes.Clear();
		this.sleeperVolumeMap.Clear();
		int num = _br.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			SleeperVolume sleeperVolume = SleeperVolume.Read(_br);
			this.sleeperVolumes.Add(sleeperVolume);
			List<int> list;
			if (!this.sleeperVolumeMap.TryGetValue(sleeperVolume.BoxMin, out list))
			{
				list = new List<int>();
				this.sleeperVolumeMap.Add(sleeperVolume.BoxMin, list);
			}
			list.Add(this.sleeperVolumes.Count - 1);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetupSleeperVolumes()
	{
		for (int i = 0; i < this.sleeperVolumes.Count; i++)
		{
			this.sleeperVolumes[i].AddToPrefabInstance();
		}
	}

	public void NotifySleeperVolumesEntityDied(EntityAlive entity)
	{
		List<SleeperVolume> obj = this.sleeperVolumes;
		lock (obj)
		{
			for (int i = 0; i < this.sleeperVolumes.Count; i++)
			{
				this.sleeperVolumes[i].EntityDied(entity);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TickSleeperVolumes()
	{
		if (!GameStats.GetBool(EnumGameStats.IsSpawnEnemies))
		{
			return;
		}
		List<SleeperVolume> obj = this.sleeperVolumes;
		lock (obj)
		{
			SleeperVolume.TickSpawnCount = 0;
			for (int i = 0; i < this.sleeperVolumes.Count; i++)
			{
				this.sleeperVolumes[i].Tick(this);
			}
		}
	}

	public override int AddTriggerVolume(TriggerVolume _triggerVolume)
	{
		List<TriggerVolume> obj = this.triggerVolumes;
		int result;
		lock (obj)
		{
			this.triggerVolumes.Add(_triggerVolume);
			List<int> list;
			if (!this.triggerVolumeMap.TryGetValue(_triggerVolume.BoxMin, out list))
			{
				list = new List<int>();
				this.triggerVolumeMap.Add(_triggerVolume.BoxMin, list);
			}
			list.Add(this.triggerVolumes.Count - 1);
			result = this.triggerVolumes.Count - 1;
		}
		return result;
	}

	public override void ResetTriggerVolumes(long chunkKey)
	{
		Vector2i vector2i = WorldChunkCache.ChunkPositionFromKey(chunkKey);
		Bounds bounds = Chunk.CalculateAABB(vector2i.x, 0, vector2i.y);
		foreach (TriggerVolume triggerVolume in this.triggerVolumes)
		{
			if (triggerVolume.Intersects(bounds))
			{
				triggerVolume.Reset();
			}
		}
	}

	public override void ResetSleeperVolumes(long chunkKey)
	{
		Vector2i vector2i = WorldChunkCache.ChunkPositionFromKey(chunkKey);
		Bounds bounds = Chunk.CalculateAABB(vector2i.x, 0, vector2i.y);
		foreach (SleeperVolume sleeperVolume in this.sleeperVolumes)
		{
			if (sleeperVolume.Intersects(bounds))
			{
				sleeperVolume.DespawnAndReset(this);
			}
		}
	}

	public override int FindTriggerVolume(Vector3i mins, Vector3i maxs)
	{
		List<int> list;
		if (this.triggerVolumeMap.TryGetValue(mins, out list))
		{
			for (int i = 0; i < list.Count; i++)
			{
				int num = list[i];
				if (this.triggerVolumes[num].BoxMax == maxs)
				{
					return num;
				}
			}
		}
		return -1;
	}

	public override int GetTriggerVolumeCount()
	{
		return this.triggerVolumes.Count;
	}

	public override TriggerVolume GetTriggerVolume(int index)
	{
		return this.triggerVolumes[index];
	}

	public void CheckTriggerVolumeTrigger(EntityPlayer _player)
	{
		Vector3i blockPosition = _player.GetBlockPosition();
		Chunk chunk = (Chunk)this.GetChunkFromWorldPos(blockPosition);
		if (chunk != null)
		{
			List<int> list = chunk.GetTriggerVolumes();
			List<TriggerVolume> obj = this.triggerVolumes;
			lock (obj)
			{
				for (int i = 0; i < list.Count; i++)
				{
					int num = list[i];
					if (num < this.triggerVolumes.Count)
					{
						this.triggerVolumes[num].CheckTouching(this, _player);
					}
				}
			}
		}
	}

	public void WriteTriggerVolumes(BinaryWriter _bw)
	{
		_bw.Write(this.triggerVolumes.Count);
		for (int i = 0; i < this.triggerVolumes.Count; i++)
		{
			this.triggerVolumes[i].Write(_bw);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ReadTriggerVolumes(BinaryReader _br)
	{
		this.triggerVolumes.Clear();
		this.triggerVolumeMap.Clear();
		int num = _br.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			TriggerVolume triggerVolume = TriggerVolume.Read(_br);
			this.triggerVolumes.Add(triggerVolume);
			List<int> list;
			if (!this.triggerVolumeMap.TryGetValue(triggerVolume.BoxMin, out list))
			{
				list = new List<int>();
				this.triggerVolumeMap.Add(triggerVolume.BoxMin, list);
			}
			list.Add(this.triggerVolumes.Count - 1);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetupTriggerVolumes()
	{
		for (int i = 0; i < this.triggerVolumes.Count; i++)
		{
			this.triggerVolumes[i].AddToPrefabInstance();
		}
	}

	public override int AddWallVolume(WallVolume _wallVolume)
	{
		List<WallVolume> obj = this.wallVolumes;
		int result;
		lock (obj)
		{
			this.wallVolumes.Add(_wallVolume);
			List<int> list;
			if (!this.wallVolumeMap.TryGetValue(_wallVolume.BoxMin, out list))
			{
				list = new List<int>();
				this.wallVolumeMap.Add(_wallVolume.BoxMin, list);
			}
			list.Add(this.wallVolumes.Count - 1);
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				NetPackageWallVolume package = NetPackageManager.GetPackage<NetPackageWallVolume>();
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(package.Setup(_wallVolume), false, -1, -1, -1, null, 192);
			}
			result = this.wallVolumes.Count - 1;
		}
		return result;
	}

	public override int FindWallVolume(Vector3i mins, Vector3i maxs)
	{
		List<int> list;
		if (this.wallVolumeMap.TryGetValue(mins, out list))
		{
			for (int i = 0; i < list.Count; i++)
			{
				int num = list[i];
				if (this.wallVolumes[num].BoxMax == maxs)
				{
					return num;
				}
			}
		}
		return -1;
	}

	public override int GetWallVolumeCount()
	{
		return this.wallVolumes.Count;
	}

	public override WallVolume GetWallVolume(int index)
	{
		if (index >= this.wallVolumes.Count)
		{
			Debug.LogWarning(string.Format("Wall Volume Error: Index {0} | wallVolumeCount: {1}", index, this.wallVolumes.Count));
		}
		return this.wallVolumes[index];
	}

	public override List<WallVolume> GetAllWallVolumes()
	{
		return this.wallVolumes;
	}

	public void WriteWallVolumes(BinaryWriter _bw)
	{
		_bw.Write(this.wallVolumes.Count);
		for (int i = 0; i < this.wallVolumes.Count; i++)
		{
			this.wallVolumes[i].Write(_bw);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ReadWallVolumes(BinaryReader _br)
	{
		this.wallVolumes.Clear();
		this.wallVolumeMap.Clear();
		int num = _br.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			WallVolume wallVolume = WallVolume.Read(_br);
			this.wallVolumes.Add(wallVolume);
			List<int> list;
			if (!this.wallVolumeMap.TryGetValue(wallVolume.BoxMin, out list))
			{
				list = new List<int>();
				this.wallVolumeMap.Add(wallVolume.BoxMin, list);
			}
			list.Add(this.wallVolumes.Count - 1);
		}
	}

	public void SetWallVolumesForClient(List<WallVolume> wallVolumeData)
	{
		this.wallVolumes.Clear();
		this.wallVolumeMap.Clear();
		foreach (WallVolume wallVolume in wallVolumeData)
		{
			this.wallVolumes.Add(wallVolume);
			List<int> list;
			if (!this.wallVolumeMap.TryGetValue(wallVolume.BoxMin, out list))
			{
				list = new List<int>();
				this.wallVolumeMap.Add(wallVolume.BoxMin, list);
			}
			list.Add(this.wallVolumes.Count - 1);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetupWallVolumes()
	{
		for (int i = 0; i < this.wallVolumes.Count; i++)
		{
			this.wallVolumes[i].AddToPrefabInstance();
		}
	}

	public void AddBlockData(Vector3i v3i, object bd)
	{
		this.blockData.Add(v3i, bd);
	}

	public object GetBlockData(Vector3i v3i)
	{
		object result;
		if (!this.blockData.TryGetValue(v3i, out result))
		{
			result = null;
		}
		return result;
	}

	public void ClearBlockData(Vector3i v3i)
	{
		this.blockData.Remove(v3i);
	}

	public void RebuildTerrain(HashSetLong _chunks, Vector3i _areaStart, Vector3i _areaSize, bool _bStopStabilityUpdate, bool _bRegenerateChunk, bool _bFillEmptyBlocks, bool _isReset = false)
	{
		this.ChunkCache.ChunkProvider.RebuildTerrain(_chunks, _areaStart, _areaSize, _bStopStabilityUpdate, _bRegenerateChunk, _bFillEmptyBlocks, _isReset);
	}

	public override GameRandom GetGameRandom()
	{
		return this.rand;
	}

	public float RandomRange(float _min, float _max)
	{
		return this.rand.RandomFloat * (_max - _min) + _min;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DuskDawnInit()
	{
		this.DuskHour = 22;
		int @int = GameStats.GetInt(EnumGameStats.DayLightLength);
		if (@int > 22)
		{
			this.DuskHour = Mathf.Clamp(@int, 0, 23);
		}
		this.DawnHour = Mathf.Clamp(this.DuskHour - @int, 0, 23);
	}

	public void SetTime(ulong _time)
	{
		this.worldTime = _time;
		if (this.m_WorldEnvironment)
		{
			this.m_WorldEnvironment.WorldTimeChanged();
		}
	}

	public void SetTimeJump(ulong _time, bool _isSeek = false)
	{
		this.SetTime(_time);
		SkyManager.bUpdateSunMoonNow = true;
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			this.aiDirector.BloodMoonComponent.TimeChanged(_isSeek);
		}
	}

	public bool IsWorldEvent(World.WorldEvent _event)
	{
		return _event == World.WorldEvent.BloodMoon && this.isEventBloodMoon;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void WorldEventUpdateTime()
	{
		this.WorldDay = GameUtils.WorldTimeToDays(this.worldTime);
		this.WorldHour = GameUtils.WorldTimeToHours(this.worldTime);
		int num = GameUtils.WorldTimeToDays(this.eventWorldTime);
		int num2 = GameUtils.WorldTimeToHours(this.eventWorldTime);
		if (num == this.WorldDay && num2 == this.WorldHour)
		{
			return;
		}
		this.eventWorldTime = this.worldTime;
		int num3 = 22;
		int @int = GameStats.GetInt(EnumGameStats.DayLightLength);
		if (@int > 22)
		{
			num3 = Mathf.Clamp(@int, 0, 23);
		}
		int num4 = Mathf.Clamp(num3 - @int, 0, 23);
		bool flag = this.isEventBloodMoon;
		this.isEventBloodMoon = false;
		int int2 = GameStats.GetInt(EnumGameStats.BloodMoonDay);
		if (this.WorldDay == int2)
		{
			if (this.WorldHour >= num3)
			{
				this.isEventBloodMoon = true;
			}
		}
		else if (this.WorldDay > 1 && this.WorldDay == int2 + 1 && this.WorldHour < num4)
		{
			this.isEventBloodMoon = true;
		}
		if (flag != this.isEventBloodMoon)
		{
			EntityPlayerLocal primaryPlayer = this.GetPrimaryPlayer();
			if (primaryPlayer)
			{
				if (this.isEventBloodMoon && this.WorldHour == num3)
				{
					primaryPlayer.BloodMoonParticipation = true;
					return;
				}
				if (!this.isEventBloodMoon && this.WorldHour == num4 && primaryPlayer.BloodMoonParticipation)
				{
					QuestEventManager.Current.BloodMoonSurvived();
					primaryPlayer.BloodMoonParticipation = false;
				}
			}
		}
	}

	public override void AddPendingDowngradeBlock(Vector3i _blockPos)
	{
		this.pendingUpgradeDowngradeBlocks.Add(_blockPos);
	}

	public override bool TryRetrieveAndRemovePendingDowngradeBlock(Vector3i _blockPos)
	{
		if (this.pendingUpgradeDowngradeBlocks.Contains(_blockPos))
		{
			this.pendingUpgradeDowngradeBlocks.Remove(_blockPos);
			return true;
		}
		return false;
	}

	public IEnumerator ResetPOIS(List<PrefabInstance> prefabInstances, FastTags<TagGroup.Global> questTags, int entityID, int[] sharedWith, QuestClass questClass)
	{
		int num;
		for (int i = 0; i < prefabInstances.Count; i = num + 1)
		{
			PrefabInstance prefabInstance = prefabInstances[i];
			yield return prefabInstance.ResetTerrain(this);
			num = i;
		}
		for (int j = 0; j < prefabInstances.Count; j++)
		{
			PrefabInstance prefabInstance2 = prefabInstances[j];
			this.triggerManager.RemoveFromUpdateList(prefabInstance2);
			prefabInstance2.LastQuestClass = questClass;
			prefabInstance2.ResetBlocksAndRebuild(this, questTags);
			for (int k = 0; k < prefabInstance2.prefab.SleeperVolumes.Count; k++)
			{
				Vector3i startPos = prefabInstance2.prefab.SleeperVolumes[k].startPos;
				Vector3i size = prefabInstance2.prefab.SleeperVolumes[k].size;
				int num2 = GameManager.Instance.World.FindSleeperVolume(prefabInstance2.boundingBoxPosition + startPos, prefabInstance2.boundingBoxPosition + startPos + size);
				if (num2 != -1)
				{
					this.GetSleeperVolume(num2).DespawnAndReset(this);
				}
			}
			for (int l = 0; l < prefabInstance2.prefab.TriggerVolumes.Count; l++)
			{
				Vector3i startPos2 = prefabInstance2.prefab.TriggerVolumes[l].startPos;
				Vector3i size2 = prefabInstance2.prefab.TriggerVolumes[l].size;
				int num3 = GameManager.Instance.World.FindTriggerVolume(prefabInstance2.boundingBoxPosition + startPos2, prefabInstance2.boundingBoxPosition + startPos2 + size2);
				if (num3 != -1)
				{
					this.GetTriggerVolume(num3).Reset();
				}
			}
			this.triggerManager.RefreshTriggers(prefabInstance2, questTags);
			if (prefabInstance2.prefab.GetQuestTag(questTags) && (prefabInstance2.lockInstance == null || prefabInstance2.lockInstance.CheckQuestLock()))
			{
				prefabInstance2.lockInstance = new QuestLockInstance(entityID);
				if (sharedWith != null)
				{
					prefabInstance2.lockInstance.AddQuesters(sharedWith);
				}
			}
		}
		bool finished = false;
		while (!finished)
		{
			int num4 = 0;
			while (num4 < prefabInstances.Count && prefabInstances[num4].bPrefabCopiedIntoWorld)
			{
				num4++;
			}
			finished = (num4 >= prefabInstances.Count);
			if (!finished)
			{
				yield return null;
			}
		}
		yield break;
	}

	public const int cCollisionBlocks = 5;

	public ulong worldTime;

	public int DawnHour;

	public int DuskHour;

	public float Gravity = 0.08f;

	public DictionaryList<int, Entity> Entities = new DictionaryList<int, Entity>();

	public DictionaryList<int, EntityPlayer> Players = new DictionaryList<int, EntityPlayer>();

	public List<EntityAlive> EntityAlives = new List<EntityAlive>();

	public NetEntityDistribution entityDistributer;

	public AIDirector aiDirector;

	public Manager audioManager;

	public Conductor dmsConductor;

	public IGameManager gameManager;

	public int Seed;

	public WorldBiomes Biomes;

	public SpawnManagerBiomes biomeSpawnManager;

	public BiomeIntensity LocalPlayerBiomeIntensityStandingOn = BiomeIntensity.Default;

	public WorldCreationData wcd;

	[PublicizedFrom(EAccessModifier.Private)]
	public WorldState worldState;

	public ChunkManager m_ChunkManager;

	public SharedChunkObserverCache m_SharedChunkObserverCache;

	public WorldEnvironment m_WorldEnvironment;

	public BiomeAtmosphereEffects BiomeAtmosphereEffects;

	public static bool IsSplatMapAvailable;

	public List<SSpawnedEntity> Last4Spawned = new List<SSpawnedEntity>();

	public int playerEntityUpdateCount;

	public int clientLastEntityId;

	public Transform EntitiesTransform;

	[PublicizedFrom(EAccessModifier.Private)]
	public GameRandom rand;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isUnityTerrainConfigured;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<EntityPlayerLocal> m_LocalPlayerEntities = new List<EntityPlayerLocal>();

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityPlayerLocal m_LocalPlayerEntity;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<Entity> entitiesWithinAABBExcludingEntity = new List<Entity>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<EntityAlive> livingEntitiesWithinAABBExcludingEntity = new List<EntityAlive>();

	[PublicizedFrom(EAccessModifier.Private)]
	public MapObjectManager objectsOnMap;

	[PublicizedFrom(EAccessModifier.Private)]
	public SpawnManagerDynamic dynamicSpawnManager;

	[PublicizedFrom(EAccessModifier.Private)]
	public WorldBlockTicker worldBlockTicker;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<SleeperVolume> sleeperVolumes = new List<SleeperVolume>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<Vector3i, List<int>> sleeperVolumeMap = new Dictionary<Vector3i, List<int>>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<TriggerVolume> triggerVolumes = new List<TriggerVolume>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<Vector3i, List<int>> triggerVolumeMap = new Dictionary<Vector3i, List<int>>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<WallVolume> wallVolumes = new List<WallVolume>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<Vector3i, List<int>> wallVolumeMap = new Dictionary<Vector3i, List<int>>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<Vector3i, object> blockData = new Dictionary<Vector3i, object>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<long> newlyLoadedChunksThisUpdate = new List<long>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<long, ulong> areaMasterChunksToLock = new Dictionary<long, ulong>();

	public TriggerManager triggerManager;

	[PublicizedFrom(EAccessModifier.Private)]
	public static int[][] supportOrder = new int[][]
	{
		new int[]
		{
			0,
			7,
			1,
			6,
			2,
			4,
			3,
			5
		},
		new int[]
		{
			0,
			2,
			1,
			7,
			3,
			4,
			6,
			5
		},
		new int[]
		{
			2,
			1,
			3,
			0,
			4,
			6,
			5,
			7
		},
		new int[]
		{
			2,
			4,
			3,
			1,
			5,
			6,
			0,
			7
		},
		new int[]
		{
			4,
			3,
			5,
			2,
			6,
			0,
			7,
			1
		},
		new int[]
		{
			4,
			6,
			5,
			3,
			7,
			0,
			2,
			1
		},
		new int[]
		{
			6,
			5,
			7,
			4,
			0,
			2,
			1,
			3
		},
		new int[]
		{
			6,
			0,
			7,
			5,
			1,
			2,
			4,
			3
		}
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public static int[] supportOffsets = new int[]
	{
		0,
		1,
		1,
		1,
		1,
		0,
		1,
		-1,
		0,
		-1,
		-1,
		-1,
		-1,
		0,
		-1,
		1
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public MicroStopwatch msUnculling = new MicroStopwatch();

	[PublicizedFrom(EAccessModifier.Private)]
	public HashSetList<Chunk> chunksToUncull = new HashSetList<Chunk>();

	[PublicizedFrom(EAccessModifier.Private)]
	public HashSetList<Chunk> chunksToRegenerate = new HashSetList<Chunk>();

	[PublicizedFrom(EAccessModifier.Private)]
	public World.ClipBlock[] _clipBlocks = new World.ClipBlock[32];

	[PublicizedFrom(EAccessModifier.Private)]
	public Bounds[] _clipBounds = new Bounds[16];

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cCollCacheSize = 50;

	[PublicizedFrom(EAccessModifier.Private)]
	public BlockValue[,,] collBlockCache = new BlockValue[50, 50, 50];

	[PublicizedFrom(EAccessModifier.Private)]
	public sbyte[,,] collDensityCache = new sbyte[50, 50, 50];

	[PublicizedFrom(EAccessModifier.Private)]
	public int tickEntityFrameCount;

	[PublicizedFrom(EAccessModifier.Private)]
	public float tickEntityFrameCountAverage = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<Entity> tickEntityList = new List<Entity>();

	[PublicizedFrom(EAccessModifier.Private)]
	public float tickEntityPartialTicks;

	[PublicizedFrom(EAccessModifier.Private)]
	public int tickEntityIndex;

	[PublicizedFrom(EAccessModifier.Private)]
	public int tickEntitySliceCount;

	[PublicizedFrom(EAccessModifier.Protected)]
	public Queue<Vector3i> fallingBlocks = new Queue<Vector3i>();

	[PublicizedFrom(EAccessModifier.Protected)]
	public Dictionary<Vector3i, float> fallingBlocksMap = new Dictionary<Vector3i, float>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<Chunk> m_lpChunkList = new List<Chunk>();

	public const float cEdgeHard = 50f;

	public const float cEdgeSoft = 80f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cEdgeMinWorldSize = 1024;

	[PublicizedFrom(EAccessModifier.Private)]
	public static List<TraderArea> traderAreas;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cTraderPlacingProtection = 2;

	public bool isEventBloodMoon;

	public ulong eventWorldTime;

	public int WorldDay;

	public int WorldHour;

	[PublicizedFrom(EAccessModifier.Protected)]
	public HashSet<Vector3i> pendingUpgradeDowngradeBlocks = new HashSet<Vector3i>();

	public delegate void OnEntityLoadedDelegate(Entity _entity);

	public delegate void OnEntityUnloadedDelegate(Entity _entity, EnumRemoveEntityReason _reason);

	public delegate void OnWorldChangedEvent(string _sWorldName);

	[PublicizedFrom(EAccessModifier.Private)]
	public class ClipBlock
	{
		public static void ResetStorage()
		{
			World.ClipBlock._storageIndex = 0;
		}

		public static World.ClipBlock New(BlockValue _value, Block _block, float _yDistort, Vector3 _blockPos, Bounds _bounds)
		{
			World.ClipBlock clipBlock = World.ClipBlock._storage[World.ClipBlock._storageIndex];
			if (clipBlock == null)
			{
				clipBlock = new World.ClipBlock();
				World.ClipBlock._storage[World.ClipBlock._storageIndex] = clipBlock;
			}
			clipBlock.Init(_value, _block, _yDistort, _blockPos, _bounds);
			World.ClipBlock._storageIndex++;
			return clipBlock;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void Init(BlockValue _value, Block _block, float _yDistort, Vector3 _blockPos, Bounds _bounds)
		{
			this.value = _value;
			this.block = _block;
			this.pos = _blockPos;
			Bounds bounds = _bounds;
			bounds.center -= _blockPos;
			bounds.min -= new Vector3(0f, _yDistort, 0f);
			this.bmins = bounds.min;
			this.bmaxs = bounds.max;
		}

		public const int kMaxBlocks = 32;

		public BlockValue value;

		public Vector3 pos;

		public Block block;

		public Vector3 bmins;

		public Vector3 bmaxs;

		[PublicizedFrom(EAccessModifier.Private)]
		public static int _storageIndex = 0;

		[PublicizedFrom(EAccessModifier.Private)]
		public static World.ClipBlock[] _storage = new World.ClipBlock[32];
	}

	public enum WorldEvent
	{
		BloodMoon
	}
}
