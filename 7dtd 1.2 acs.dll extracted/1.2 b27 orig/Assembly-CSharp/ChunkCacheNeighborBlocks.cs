using System;

public class ChunkCacheNeighborBlocks : INeighborBlockCache
{
	public ChunkCacheNeighborBlocks(ChunkCacheNeighborChunks _nChunks)
	{
		IChunk[] array = new Chunk[9];
		this.chunks = array;
		this.centerBX = int.MaxValue;
		this.centerBZ = int.MaxValue;
		this.curChunkX = int.MaxValue;
		this.curChunkZ = int.MaxValue;
		this.firstException = true;
		base..ctor();
		this.nChunks = _nChunks;
	}

	public void Init(int _bX, int _bZ)
	{
		IChunk chunk = this.nChunks[0, 0];
		if (_bX == this.centerBX && _bZ == this.centerBZ && chunk.X == this.curChunkX && chunk.Z == this.curChunkZ)
		{
			return;
		}
		this.centerBX = _bX;
		this.centerBZ = _bZ;
		this.curChunkX = chunk.X;
		this.curChunkZ = chunk.Z;
		this.chunks[0] = ((_bX > 0) ? ((_bZ > 0) ? chunk : this.nChunks[0, -1]) : ((_bZ > 0) ? this.nChunks[-1, 0] : this.nChunks[-1, -1]));
		this.chunks[1] = ((_bZ > 0) ? chunk : this.nChunks[0, -1]);
		this.chunks[2] = ((_bX < 15) ? ((_bZ > 0) ? chunk : this.nChunks[0, -1]) : ((_bZ > 0) ? this.nChunks[1, 0] : this.nChunks[1, -1]));
		this.chunks[3] = ((_bX > 0) ? chunk : this.nChunks[-1, 0]);
		this.chunks[4] = chunk;
		this.chunks[5] = ((_bX < 15) ? chunk : this.nChunks[1, 0]);
		this.chunks[6] = ((_bX > 0) ? ((_bZ < 15) ? chunk : this.nChunks[0, 1]) : ((_bZ < 15) ? this.nChunks[-1, 0] : this.nChunks[-1, 1]));
		this.chunks[7] = ((_bZ < 15) ? chunk : this.nChunks[0, 1]);
		this.chunks[8] = ((_bX < 15) ? ((_bZ < 15) ? chunk : this.nChunks[0, 1]) : ((_bZ < 15) ? this.nChunks[1, 0] : this.nChunks[1, 1]));
	}

	public void Clear()
	{
		this.centerBX = int.MaxValue;
		this.centerBZ = int.MaxValue;
		this.curChunkX = int.MaxValue;
		this.curChunkZ = int.MaxValue;
		Array.Clear(this.chunks, 0, this.chunks.Length);
	}

	public IChunk GetChunk(int x, int z)
	{
		return this.chunks[x + 1 + (z + 1) * 3];
	}

	public IChunk GetNeighborChunk(int x, int z)
	{
		return this.nChunks[x, z];
	}

	public bool IsBlockInCache(int relx, int absy, int relz)
	{
		if (absy < 0 || absy > 255)
		{
			return false;
		}
		int num = relx + 1 + (relz + 1) * 3;
		return num <= 8 && this.chunks[num] != null;
	}

	public Vector3i GetChunkPos(int relx, int absy, int relz)
	{
		return new Vector3i(this.centerBX + relx & 15, absy, this.centerBZ + relz & 15);
	}

	public BlockValue Get(int relx, int absy, int relz)
	{
		int num = relx + 1 + (relz + 1) * 3;
		if (absy < 0 || num >= 9)
		{
			return BlockValue.Air;
		}
		BlockValue result;
		try
		{
			BlockValue blockValue = this.chunks[num].GetBlock(this.centerBX + relx & 15, absy, this.centerBZ + relz & 15);
			if (!GameManager.bShowDecorBlocks && blockValue.Block.IsDecoration)
			{
				blockValue = BlockValue.Air;
			}
			if (!GameManager.bShowLootBlocks)
			{
				if (blockValue.Block is BlockLoot)
				{
					blockValue = BlockValue.Air;
				}
				else
				{
					BlockCompositeTileEntity blockCompositeTileEntity = blockValue.Block as BlockCompositeTileEntity;
					if (blockCompositeTileEntity != null && blockCompositeTileEntity.CompositeData.HasFeature<ITileEntityLootable>())
					{
						blockValue = BlockValue.Air;
					}
				}
			}
			result = blockValue;
		}
		catch (Exception)
		{
			if (this.firstException)
			{
				Log.Error(string.Concat(new string[]
				{
					"ChunkCacheNeighborBlocks.Get: relX=",
					relx.ToString(),
					" relz=",
					relz.ToString(),
					" len=",
					this.chunks.Length.ToString()
				}));
				this.firstException = false;
			}
			result = BlockValue.Air;
		}
		return result;
	}

	public byte GetStab(int relx, int absy, int relz)
	{
		byte result = 0;
		try
		{
			result = this.chunks[relx + 1 + (relz + 1) * 3].GetStability(this.centerBX + relx & 15, absy, this.centerBZ + relz & 15);
		}
		catch (Exception ex)
		{
			Log.Out(string.Concat(new string[]
			{
				"Bad ChunkCacheNeighborBlocks index (",
				relx.ToString(),
				", ",
				absy.ToString(),
				", ",
				relz.ToString(),
				"), \nException: ",
				ex.ToString()
			}));
		}
		return result;
	}

	public bool IsWater(int relx, int absy, int relz)
	{
		int num = relx + 1 + (relz + 1) * 3;
		if (absy < 0 || num >= 9)
		{
			return false;
		}
		bool result = false;
		try
		{
			result = this.chunks[num].IsWater(this.centerBX + relx & 15, absy, this.centerBZ + relz & 15);
		}
		catch (Exception ex)
		{
			Log.Out(string.Concat(new string[]
			{
				"Bad ChunkCacheNeighborBlocks index (",
				relx.ToString(),
				", ",
				absy.ToString(),
				", ",
				relz.ToString(),
				"), \nException: ",
				ex.ToString()
			}));
		}
		return result;
	}

	public bool IsAir(int relx, int absy, int relz)
	{
		int num = relx + 1 + (relz + 1) * 3;
		if (absy < 0 || num >= 9)
		{
			return false;
		}
		int x = this.centerBX + relx & 15;
		int z = this.centerBZ + relz & 15;
		bool result = false;
		try
		{
			IChunk chunk = this.chunks[num];
			return chunk.GetBlock(x, absy, z).isair && !chunk.IsWater(x, absy, z);
		}
		catch (Exception ex)
		{
			Log.Out(string.Concat(new string[]
			{
				"Bad ChunkCacheNeighborBlocks index (",
				relx.ToString(),
				", ",
				absy.ToString(),
				", ",
				relz.ToString(),
				"), \nException: ",
				ex.ToString()
			}));
		}
		return result;
	}

	public override string ToString()
	{
		return string.Format("BlockCache -- Chunk Pos ({0}, {1}) -- Block Pos ({2}, {3})", new object[]
		{
			this.curChunkX,
			this.curChunkZ,
			this.centerBX,
			this.centerBZ
		});
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IChunk[] chunks;

	[PublicizedFrom(EAccessModifier.Private)]
	public ChunkCacheNeighborChunks nChunks;

	[PublicizedFrom(EAccessModifier.Private)]
	public int centerBX;

	[PublicizedFrom(EAccessModifier.Private)]
	public int centerBZ;

	[PublicizedFrom(EAccessModifier.Private)]
	public int curChunkX;

	[PublicizedFrom(EAccessModifier.Private)]
	public int curChunkZ;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool firstException;
}
