using System;

public class ChunkSnapshotUtil : IRegionFileChunkSnapshotUtil
{
	public ChunkSnapshotUtil(RegionFileAccessAbstract regionFileAccess)
	{
		this.regionFileAccess = regionFileAccess;
		this.chunkReader = new RegionFileChunkReader(regionFileAccess);
		this.chunkWriter = new RegionFileChunkWriter(regionFileAccess);
	}

	public IRegionFileChunkSnapshot TakeSnapshot(Chunk chunk, bool saveIfUnchanged)
	{
		RegionFileChunkSnapshot regionFileChunkSnapshot = this.poolSnapshots.AllocSync(true);
		regionFileChunkSnapshot.Update(chunk, saveIfUnchanged);
		return regionFileChunkSnapshot;
	}

	public void WriteSnapshot(IRegionFileChunkSnapshot snapshot, string dir, int chunkX, int chunkZ)
	{
		snapshot.Write(this.chunkWriter, dir, chunkX, chunkZ);
	}

	public Chunk LoadChunk(string dir, long key)
	{
		int chunkX = WorldChunkCache.extractX(key);
		int chunkZ = WorldChunkCache.extractZ(key);
		try
		{
			uint version;
			PooledBinaryReader pooledBinaryReader = this.chunkReader.readIntoLoadStream(dir, chunkX, chunkZ, "7rg", out version);
			if (pooledBinaryReader == null)
			{
				return null;
			}
			Chunk chunk = MemoryPools.PoolChunks.AllocSync(true);
			chunk.load(pooledBinaryReader, version);
			chunk.NeedsRegeneration = true;
			return chunk;
		}
		catch (Exception e)
		{
			Log.Error(string.Concat(new string[]
			{
				"EXCEPTION: In load chunk (chunkX=",
				chunkX.ToString(),
				" chunkZ=",
				chunkZ.ToString(),
				")"
			}));
			Log.Exception(e);
			try
			{
				this.chunkReader.WriteBackup(dir, chunkX, chunkZ);
			}
			catch (Exception e2)
			{
				Log.Error("Error backing up data:");
				Log.Exception(e2);
			}
		}
		try
		{
			this.regionFileAccess.Remove(dir, chunkX, chunkZ);
		}
		catch (Exception ex)
		{
			Log.Error(string.Concat(new string[]
			{
				"In remove chunk (chunkX=",
				chunkX.ToString(),
				" chunkZ=",
				chunkZ.ToString(),
				"):",
				ex.Message
			}));
		}
		return null;
	}

	public void Free(IRegionFileChunkSnapshot iSnapshot)
	{
		if (iSnapshot == null)
		{
			return;
		}
		RegionFileChunkSnapshot regionFileChunkSnapshot = iSnapshot as RegionFileChunkSnapshot;
		if (regionFileChunkSnapshot != null)
		{
			this.poolSnapshots.FreeSync(regionFileChunkSnapshot);
			return;
		}
		Log.Error("Attempting to free snapshot of wrong type. Expected: RegionFileChunkSnapshot, Actual: " + iSnapshot.GetType().Name);
	}

	public void Cleanup()
	{
		this.poolSnapshots.Cleanup();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public RegionFileAccessAbstract regionFileAccess;

	[PublicizedFrom(EAccessModifier.Private)]
	public RegionFileChunkReader chunkReader;

	[PublicizedFrom(EAccessModifier.Private)]
	public RegionFileChunkWriter chunkWriter;

	[PublicizedFrom(EAccessModifier.Private)]
	public MemoryPooledObject<RegionFileChunkSnapshot> poolSnapshots = new MemoryPooledObject<RegionFileChunkSnapshot>(255);
}
