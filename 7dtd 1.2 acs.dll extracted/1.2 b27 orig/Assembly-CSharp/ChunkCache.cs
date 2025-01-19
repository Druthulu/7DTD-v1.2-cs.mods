using System;

public class ChunkCache : IBlockAccess
{
	public ChunkCache(int _dim)
	{
		this.chunkArray = new Chunk[_dim, _dim];
	}

	public void Init(World world, int x, int y, int z, int sx, int sy, int sz)
	{
		this.worldObj = world;
		this.chunkX = x >> 4;
		this.chunkZ = z >> 4;
		int num = sx >> 4;
		int num2 = sz >> 4;
		for (int i = this.chunkX; i <= num; i++)
		{
			for (int j = this.chunkZ; j <= num2; j++)
			{
				Chunk chunkSync = world.ChunkCache.GetChunkSync(i, j);
				if (chunkSync != null)
				{
					this.chunkArray[i - this.chunkX, j - this.chunkZ] = chunkSync;
				}
			}
		}
	}

	public void Clear()
	{
		Array.Clear(this.chunkArray, 0, this.chunkArray.GetLength(0) * this.chunkArray.GetLength(1));
	}

	public BlockValue GetBlock(Vector3i _pos)
	{
		return this.GetBlock(_pos.x, _pos.y, _pos.z);
	}

	public BlockValue GetBlock(int _x, int _y, int _z)
	{
		if (_y < 0)
		{
			return BlockValue.Air;
		}
		if (_y >= 256)
		{
			return BlockValue.Air;
		}
		int num = (_x >> 4) - this.chunkX;
		int num2 = (_z >> 4) - this.chunkZ;
		if (num < 0 || num >= this.chunkArray.GetLength(0) || num2 < 0 || num2 >= this.chunkArray.GetLength(1))
		{
			Chunk chunkSync = this.worldObj.ChunkCache.GetChunkSync(World.toChunkXZ(_x), World.toChunkXZ(_z));
			if (chunkSync == null)
			{
				return BlockValue.Air;
			}
			if (!chunkSync.IsInitialized)
			{
				return BlockValue.Air;
			}
			return chunkSync.GetBlock(_x & 15, _y, _z & 15);
		}
		else
		{
			Chunk chunk = this.chunkArray[num, num2];
			if (chunk == null)
			{
				return BlockValue.Air;
			}
			if (!chunk.IsInitialized)
			{
				return BlockValue.Air;
			}
			return chunk.GetBlock(_x & 15, _y, _z & 15);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int chunkX;

	[PublicizedFrom(EAccessModifier.Private)]
	public int chunkZ;

	[PublicizedFrom(EAccessModifier.Private)]
	public Chunk[,] chunkArray;

	[PublicizedFrom(EAccessModifier.Private)]
	public World worldObj;
}
