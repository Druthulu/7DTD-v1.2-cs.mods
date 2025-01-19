using System;
using System.Collections;
using UnityEngine;

public class ChunkProviderTerrainFromDisc : ChunkProviderGenerateWorld
{
	public ChunkProviderTerrainFromDisc(ChunkCluster _cc, string _levelName) : base(_cc, _levelName, false)
	{
		this.SetDecorationsEnabled(true);
	}

	public override IEnumerator Init(World _world)
	{
		yield return this.<>n__0(_world);
		this.m_BiomeProvider = new WorldBiomeProviderFromImage(this.levelName, _world.Biomes, 4096);
		WorldDecoratorPOIFromImage worldDecoratorPOIFromImage = new WorldDecoratorPOIFromImage(this.levelName, this.GetDynamicPrefabDecorator(), 6144, 6144, null, false, 1, null, null);
		this.m_Decorators.Add(worldDecoratorPOIFromImage);
		yield return worldDecoratorPOIFromImage.InitData();
		this.m_Decorators.Add(new WorldDecoratorBlocksFromBiome(this.m_BiomeProvider, this.GetDynamicPrefabDecorator()));
		string saveGameRegionDirDefault = GameIO.GetSaveGameRegionDirDefault();
		string saveDirectory = _world.IsEditor() ? GameIO.GetSaveGameRegionDirDefault() : null;
		this.terrainRegionFileManager = new RegionFileManager(saveGameRegionDirDefault, saveDirectory, 0, true);
		this.m_TerrainGenerator = new TerrainFromChunk();
		((TerrainFromChunk)this.m_TerrainGenerator).Init(this.terrainRegionFileManager, this.m_BiomeProvider, _world.Seed);
		string text = _world.IsEditor() ? null : GameIO.GetSaveGameRegionDir();
		this.m_RegionFileManager = new MyRegionFileManager(_world, this, this.terrainRegionFileManager, text, text, 0, !_world.IsEditor());
		yield return null;
		yield break;
	}

	public override void Cleanup()
	{
		base.Cleanup();
		this.terrainRegionFileManager.Cleanup();
	}

	public override void SaveAll()
	{
		base.SaveAll();
		if (this.world.IsEditor())
		{
			this.m_RegionFileManager.MakePersistent(this.world.ChunkCache, false);
			this.m_RegionFileManager.WaitSaveDone();
			this.terrainRegionFileManager.MakePersistent(null, false);
			this.terrainRegionFileManager.WaitSaveDone();
		}
	}

	public override bool GetOverviewMap(Vector2i _startPos, Vector2i _size, Color[] mapColors)
	{
		this.terrainRegionFileManager.SetCacheSize(1000);
		bool overviewMap = base.GetOverviewMap(_startPos, _size, mapColors);
		this.terrainRegionFileManager.SetCacheSize(0);
		return overviewMap;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void generateTerrain(World _world, Chunk _chunk, GameRandom _random)
	{
		long key = _chunk.Key;
		if (!this.terrainRegionFileManager.ContainsChunkSync(key))
		{
			return;
		}
		Chunk chunkSync = this.terrainRegionFileManager.GetChunkSync(key);
		if (chunkSync == null)
		{
			return;
		}
		((TerrainFromChunk)this.m_TerrainGenerator).SetTerrainChunk(chunkSync);
		this.m_TerrainGenerator.GenerateTerrain(_world, _chunk, _random);
		((TerrainFromChunk)this.m_TerrainGenerator).SetTerrainChunk(null);
		_chunk.CopyLightsFrom(chunkSync);
		_chunk.isModified = false;
		this.terrainRegionFileManager.RemoveChunkSync(key);
		MemoryPools.PoolChunks.FreeSync(chunkSync);
	}

	public override EnumChunkProviderId GetProviderId()
	{
		return EnumChunkProviderId.ChunkDataDriven;
	}

	public override bool GetWorldExtent(out Vector3i _minSize, out Vector3i _maxSize)
	{
		Vector2i minPos = ((WorldDecoratorPOIFromImage)this.m_Decorators[0]).m_Poi.MinPos;
		Vector2i maxPos = ((WorldDecoratorPOIFromImage)this.m_Decorators[0]).m_Poi.MaxPos;
		_minSize = new Vector3i(minPos.x, 0, minPos.y) + new Vector3i(80, 0, 80);
		_maxSize = new Vector3i(maxPos.x, 255, maxPos.y) - new Vector3i(80, 0, 80);
		return true;
	}

	public override int GetPOIBlockIdOverride(int _x, int _z)
	{
		WorldGridCompressedData<byte> poi = ((WorldDecoratorPOIFromImage)this.m_Decorators[0]).m_Poi;
		if (!poi.Contains(_x, _z))
		{
			return 0;
		}
		byte data = poi.GetData(_x, _z);
		if (data == 0 || data == 255)
		{
			return 0;
		}
		PoiMapElement poiForColor = this.world.Biomes.getPoiForColor((uint)data);
		if (poiForColor == null)
		{
			return 0;
		}
		return poiForColor.m_BlockValue.type;
	}

	public override float GetPOIHeightOverride(int x, int z)
	{
		WorldGridCompressedData<byte> poi = ((WorldDecoratorPOIFromImage)this.m_Decorators[0]).m_Poi;
		byte data;
		if (!poi.Contains(x, z) || (data = poi.GetData(x, z)) == 255 || data == 0)
		{
			return 0f;
		}
		PoiMapElement poiForColor = this.world.Biomes.getPoiForColor((uint)data);
		if (poiForColor == null)
		{
			return 0f;
		}
		if (!poiForColor.m_BlockValue.Block.blockMaterial.IsLiquid)
		{
			return 0f;
		}
		return (float)poiForColor.m_YPosFill;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public RegionFileManager terrainRegionFileManager;
}
