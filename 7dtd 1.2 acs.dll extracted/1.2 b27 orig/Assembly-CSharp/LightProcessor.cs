using System;
using System.Collections.Generic;
using UnityEngine;

public class LightProcessor : ILightProcessor
{
	public LightProcessor(IChunkAccess _world)
	{
		this.m_World = _world;
	}

	public void GenerateSunlight(Chunk chunk, bool bSpreadLight)
	{
		for (int i = 0; i < 16; i++)
		{
			for (int j = 0; j < 16; j++)
			{
				this.RefreshSunlightAtLocalPos(chunk, i, j, bSpreadLight, bSpreadLight);
			}
		}
	}

	public void LightChunk(Chunk c)
	{
		int maxHeight = (int)c.GetMaxHeight();
		for (int i = 0; i < 16; i++)
		{
			for (int j = 0; j < 16; j++)
			{
				for (int k = maxHeight; k >= 0; k--)
				{
					byte light = c.GetLight(i, k, j, Chunk.LIGHT_TYPE.SUN);
					if (light > 0)
					{
						this.SpreadLight(c, i, k, j, light, Chunk.LIGHT_TYPE.SUN, false);
					}
				}
			}
		}
	}

	public void SpreadBlockLightFromLightSources(Chunk c)
	{
		for (int i = 0; i < 16; i++)
		{
			for (int j = 0; j < 16; j++)
			{
				for (int k = 255; k >= 0; k--)
				{
					BlockValue blockNoDamage = c.GetBlockNoDamage(i, k, j);
					Block block = blockNoDamage.Block;
					if (block.GetLightValue(blockNoDamage) > 0)
					{
						this.SpreadLight(c, i, k, j, block.GetLightValue(blockNoDamage), Chunk.LIGHT_TYPE.BLOCK, true);
					}
				}
			}
		}
	}

	public void RefreshSunlightAtLocalPos(Chunk c, int x, int z, bool bSpreadLight, bool refreshSunlight)
	{
		bool flag = false;
		int num = 15;
		for (int i = 255; i >= 0; i--)
		{
			int lightOpacity = c.GetBlockNoDamage(x, i, z).Block.lightOpacity;
			if (lightOpacity == 255)
			{
				flag = true;
			}
			byte light = c.GetLight(x, i, z, Chunk.LIGHT_TYPE.SUN);
			byte b;
			if (!flag)
			{
				num = Utils.FastMax(0, num - lightOpacity);
				c.SetLight(x, i, z, (byte)num, Chunk.LIGHT_TYPE.SUN);
				b = (byte)num;
			}
			else
			{
				c.SetLight(x, i, z, 0, Chunk.LIGHT_TYPE.SUN);
				b = 0;
				if (refreshSunlight)
				{
					b = this.RefreshLightAtLocalPos(c, x, i, z, Chunk.LIGHT_TYPE.SUN);
				}
			}
			if (bSpreadLight)
			{
				if (light > b)
				{
					this.UnspreadLight(c, x, i, z, light, Chunk.LIGHT_TYPE.SUN);
				}
				else if (light < b)
				{
					this.SpreadLight(c, x, i, z, b, Chunk.LIGHT_TYPE.SUN, true);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public byte getLightAtWorldPos(int worldX, int worldY, int worldZ, Chunk.LIGHT_TYPE type)
	{
		IChunk chunkFromWorldPos = this.m_World.GetChunkFromWorldPos(worldX, worldY, worldZ);
		if (chunkFromWorldPos != null)
		{
			return chunkFromWorldPos.GetLight(World.toBlockXZ(worldX), World.toBlockY(worldY), World.toBlockXZ(worldZ), type);
		}
		return 0;
	}

	public byte RefreshLightAtLocalPos(Chunk c, int x, int y, int z, Chunk.LIGHT_TYPE type)
	{
		int blockWorldPosX = c.GetBlockWorldPosX(x);
		int blockWorldPosZ = c.GetBlockWorldPosZ(z);
		BlockValue blockNoDamage = c.GetBlockNoDamage(x, y, z);
		byte b = 0;
		int lightOpacity = blockNoDamage.Block.lightOpacity;
		if (lightOpacity == 255)
		{
			c.SetLight(x, y, z, 0, type);
		}
		else
		{
			byte b2 = this.getLightAtWorldPos(blockWorldPosX, y, blockWorldPosZ, type);
			byte b3 = this.getLightAtWorldPos(blockWorldPosX + 1, y, blockWorldPosZ, type);
			byte b4 = this.getLightAtWorldPos(blockWorldPosX - 1, y, blockWorldPosZ, type);
			byte b5 = this.getLightAtWorldPos(blockWorldPosX, y, blockWorldPosZ + 1, type);
			byte b6 = this.getLightAtWorldPos(blockWorldPosX, y, blockWorldPosZ - 1, type);
			byte b7 = this.getLightAtWorldPos(blockWorldPosX, y + 1, blockWorldPosZ, type);
			byte b8 = this.getLightAtWorldPos(blockWorldPosX, y - 1, blockWorldPosZ, type);
			if (b2 == 255)
			{
				b2 = 0;
			}
			if (b3 == 255)
			{
				b3 = 0;
			}
			if (b4 == 255)
			{
				b4 = 0;
			}
			if (b5 == 255)
			{
				b5 = 0;
			}
			if (b6 == 255)
			{
				b6 = 0;
			}
			if (b7 == 255)
			{
				b7 = 0;
			}
			if (b8 == 255)
			{
				b8 = 0;
			}
			int num = (int)((byte)Mathf.Max(Mathf.Max(Mathf.Max((int)b3, (int)b4), Mathf.Max((int)b5, (int)b6)), Mathf.Max((int)b7, (int)b8)));
			num = num - 1 - lightOpacity;
			if (num < 0)
			{
				num = 0;
			}
			b = (byte)Mathf.Max(num, (int)b2);
			c.SetLight(x, y, z, b, type);
		}
		return b;
	}

	public void UnspreadLight(Chunk c, int x, int y, int z, byte lightValue, Chunk.LIGHT_TYPE type)
	{
		this.brightSpots.Clear();
		this.unspreadLight(c, x, y, z, lightValue, 0, type, this.brightSpots);
		foreach (Vector3i vector3i in this.brightSpots)
		{
			Chunk chunk = (Chunk)this.m_World.GetChunkFromWorldPos(vector3i.x, vector3i.y, vector3i.z);
			if (chunk != null)
			{
				byte lightAtWorldPos = this.getLightAtWorldPos(vector3i.x, vector3i.y, vector3i.z, type);
				if (lightAtWorldPos < 255)
				{
					this.spreadLight(chunk, World.toBlockXZ(vector3i.x), World.toBlockY(vector3i.y), World.toBlockXZ(vector3i.z), lightAtWorldPos, 0, type, true);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void unspreadLight(Chunk _chunk, int _blockX, int _blockY, int _blockZ, byte lightValue, int depth, Chunk.LIGHT_TYPE type, List<Vector3i> brightSpots)
	{
		_chunk.SetLight(_blockX, _blockY, _blockZ, 0, type);
		for (int i = 0; i < Vector3i.AllDirections.Length; i++)
		{
			int num = _blockX + Vector3i.AllDirections[i].x;
			int num2 = _blockY + Vector3i.AllDirections[i].y;
			int num3 = _blockZ + Vector3i.AllDirections[i].z;
			if (num2 >= 0 && num2 <= 255)
			{
				Chunk chunk = _chunk;
				if (num < 0 || num >= 16 || num3 < 0 || num3 >= 16)
				{
					chunk = (Chunk)this.m_World.GetChunkFromWorldPos(_chunk.GetBlockWorldPosX(num), num2, _chunk.GetBlockWorldPosZ(num3));
					num = World.toBlockXZ(num);
					num3 = World.toBlockXZ(num3);
				}
				if (chunk != null)
				{
					byte light = chunk.GetLight(num, num2, num3, type);
					if (light < 255)
					{
						if (light < lightValue && light != 0)
						{
							int type2 = chunk.GetBlockNoDamage(num, num2, num3).type;
							int num4 = (int)this.calcNextLightStep(lightValue, type2);
							if (num4 > 0)
							{
								this.unspreadLight(chunk, num, num2, num3, (byte)num4, depth + 1, type, brightSpots);
							}
						}
						else if (light >= lightValue)
						{
							brightSpots.Add(new Vector3i(chunk.GetBlockWorldPosX(num), num2, chunk.GetBlockWorldPosZ(num3)));
						}
					}
				}
			}
		}
	}

	public void SpreadLight(Chunk c, int blockX, int blockY, int blockZ, byte lightValue, Chunk.LIGHT_TYPE type, bool bSetAtStarterPos = true)
	{
		this.spreadLight(c, blockX, blockY, blockZ, lightValue, 0, type, bSetAtStarterPos);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public byte calcNextLightStep(byte _currentLight, int _blockType)
	{
		int lightOpacity = Block.list[_blockType].lightOpacity;
		int num = (int)_currentLight - ((lightOpacity != 0) ? lightOpacity : 1);
		return (byte)((num >= 0) ? num : 0);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void spreadLight(Chunk _chunk, int _blockX, int _blockY, int _blockZ, byte lightValue, int depth, Chunk.LIGHT_TYPE type, bool bSetAtStarterPos = true)
	{
		if (bSetAtStarterPos)
		{
			_chunk.SetLight(_blockX, _blockY, _blockZ, lightValue, type);
		}
		if (lightValue == 0)
		{
			return;
		}
		for (int i = Vector3i.AllDirections.Length - 1; i >= 0; i--)
		{
			Vector3i vector3i = Vector3i.AllDirections[i];
			int num = _blockX + vector3i.x;
			int num2 = _blockY + vector3i.y;
			int num3 = _blockZ + vector3i.z;
			if (num2 >= 0 && num2 <= 255)
			{
				Chunk chunk = _chunk;
				if (num < 0 || num >= 16 || num3 < 0 || num3 >= 16)
				{
					chunk = (Chunk)this.m_World.GetChunkFromWorldPos(_chunk.GetBlockWorldPosX(num), num2, _chunk.GetBlockWorldPosZ(num3));
					num = World.toBlockXZ(num);
					num3 = World.toBlockXZ(num3);
					if (chunk == null)
					{
						goto IL_100;
					}
				}
				byte light = chunk.GetLight(num, num2, num3, type);
				if (light < 15)
				{
					int type2 = chunk.GetBlockNoDamage(num, num2, num3).type;
					byte b = this.calcNextLightStep(lightValue, type2);
					if (light < b)
					{
						this.spreadLight(chunk, num, num2, num3, b, depth + 1, type, true);
					}
				}
			}
			IL_100:;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly IChunkAccess m_World;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<Vector3i> brightSpots = new List<Vector3i>();
}
