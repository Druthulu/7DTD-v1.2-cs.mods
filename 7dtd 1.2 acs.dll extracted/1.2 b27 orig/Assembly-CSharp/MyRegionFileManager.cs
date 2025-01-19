using System;

[PublicizedFrom(EAccessModifier.Internal)]
public class MyRegionFileManager : RegionFileManager
{
	public MyRegionFileManager(World _world, IChunkProvider _chunkProvider, RegionFileManager _terrainRegionCache, string _loadDirectory, string _saveDirectory, int _maxChunksInCache, bool _bAutoSaveOnChunkDrop) : base(_loadDirectory, _saveDirectory, _maxChunksInCache, _bAutoSaveOnChunkDrop)
	{
		this.terrainRegionManager = _terrainRegionCache;
		this.world = _world;
		this.chunkProvider = _chunkProvider;
	}

	public override void SaveChunkSnapshot(Chunk _chunk, bool _saveIfUnchanged)
	{
		if (this.world.IsEditor() && !this.chunkProvider.IsDecorationsEnabled())
		{
			Chunk chunk = MemoryPools.PoolChunks.AllocSync(true);
			chunk.X = _chunk.X;
			chunk.Z = _chunk.Z;
			Chunk.ToTerrain(_chunk, chunk);
			chunk.NeedsDecoration = false;
			this.terrainRegionManager.AddChunkSync(chunk, false);
		}
		base.SaveChunkSnapshot(_chunk, _saveIfUnchanged);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public RegionFileManager terrainRegionManager;

	[PublicizedFrom(EAccessModifier.Private)]
	public World world;

	[PublicizedFrom(EAccessModifier.Private)]
	public IChunkProvider chunkProvider;
}
