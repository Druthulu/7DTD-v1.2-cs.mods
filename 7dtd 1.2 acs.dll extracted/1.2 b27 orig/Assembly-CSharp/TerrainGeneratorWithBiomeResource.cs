using System;
using UnityEngine;

public abstract class TerrainGeneratorWithBiomeResource : ITerrainGenerator
{
	public virtual void Init(World _world, IBiomeProvider _biomeProvider, int _seed)
	{
		this.biomeProvider = _biomeProvider;
		this.perlinNoise = new TS_PerlinNoise(_seed);
		this.perlinNoise.setOctaves(1);
	}

	public abstract byte GetTerrainHeightAt(int _xWorld, int _zWorld, BiomeDefinition _bd, float _biomeIntensity);

	public virtual Vector3 GetTerrainNormalAt(int _xWorld, int _zWorld, BiomeDefinition _bd, float _biomeIntensity)
	{
		return Vector3.up;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual bool isTerrainAt(int _x, int _y, int _z)
	{
		return true;
	}

	public abstract sbyte GetDensityAt(int _xWorld, int _yWorld, int _zWorld, BiomeDefinition _bd, float _biomeIntensity);

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void fillDensityInBlock(Chunk _chunk, int _x, int _y, int _z, BlockValue _bv)
	{
		sbyte density = _bv.Block.shape.IsTerrain() ? MarchingCubes.DensityTerrain : MarchingCubes.DensityAir;
		_chunk.SetDensity(_x, _y, _z, density);
	}

	public virtual void GenerateTerrain(World _world, Chunk _chunk, GameRandom _random)
	{
		this.GenerateTerrain(_world, _chunk, _random, Vector3i.zero, Vector3i.zero, false, false);
	}

	public virtual void GenerateTerrain(World _world, Chunk _chunk, GameRandom _random, Vector3i _areaStart, Vector3i _areaSize, bool _bFillEmptyBlocks, bool _isReset)
	{
		int num = 0;
		int num2 = 16;
		int num3 = 0;
		int num4 = 16;
		if (_areaSize.x > 0 && _areaSize.z > 0)
		{
			Vector3i vector3i = _chunk.ToWorldPos(Vector3i.zero);
			Vector3i vector3i2 = vector3i + new Vector3i(16, 0, 16);
			if (vector3i2.x <= _areaStart.x || vector3i.x > _areaStart.x + _areaSize.x || vector3i2.z <= _areaStart.z || vector3i.z > _areaStart.z + _areaSize.z)
			{
				return;
			}
			num = _areaStart.x - vector3i.x;
			num2 = vector3i2.x - (_areaStart.x + _areaSize.x);
			num3 = _areaStart.z - vector3i.z;
			num4 = vector3i2.z - (_areaStart.z + _areaSize.z);
			if (num < 0)
			{
				num = 0;
			}
			if (num3 < 0)
			{
				num3 = 0;
			}
			if (num2 < 0)
			{
				num2 = 16;
			}
			else
			{
				num2 = 16 - num2;
			}
			if (num4 < 0)
			{
				num4 = 16;
			}
			else
			{
				num4 = 16 - num4;
			}
		}
		float biomeIntensity = 1f;
		for (int i = num3; i < num4; i++)
		{
			int blockWorldPosZ = _chunk.GetBlockWorldPosZ(i);
			for (int j = num; j < num2; j++)
			{
				int blockWorldPosX = _chunk.GetBlockWorldPosX(j);
				BiomeDefinition biomeDefinition = this.biomeProvider.GetBiomeAt(blockWorldPosX, blockWorldPosZ);
				if (biomeDefinition != null)
				{
					_chunk.SetBiomeId(j, i, biomeDefinition.m_Id);
					byte b;
					if (_isReset)
					{
						b = _chunk.GetTerrainHeight(j, i);
					}
					else
					{
						b = this.GetTerrainHeightAt(blockWorldPosX, blockWorldPosZ, biomeDefinition, biomeIntensity);
						_chunk.SetTerrainHeight(j, i, b);
					}
					_chunk.SetHeight(j, i, b);
					int subBiomeIdxAt = this.biomeProvider.GetSubBiomeIdxAt(biomeDefinition, blockWorldPosX, (int)b, blockWorldPosZ);
					if (subBiomeIdxAt >= 0 && subBiomeIdxAt < biomeDefinition.subbiomes.Count)
					{
						biomeDefinition = biomeDefinition.subbiomes[subBiomeIdxAt];
					}
					int num5 = (int)(b + 1 & byte.MaxValue);
					if (_bFillEmptyBlocks)
					{
						for (int k = 255; k >= num5; k--)
						{
							_chunk.SetBlockRaw(j, k, i, BlockValue.Air);
							_chunk.SetDensity(j, k, i, MarchingCubes.DensityAir);
						}
					}
					this.fillDensityInBlock(_chunk, j, num5, i, BlockValue.Air);
					num5--;
					if (num5 >= 0)
					{
						if (biomeDefinition.m_Layers.Count > 0)
						{
							BiomeLayer biomeLayer = biomeDefinition.m_Layers[0];
							if (biomeLayer.m_FillUpTo > 0)
							{
								BlockValue blockValue = biomeLayer.m_Block.blockValue;
								Block block = blockValue.Block;
								sbyte density = block.shape.IsTerrain() ? -1 : MarchingCubes.DensityAir;
								for (int l = num5; l <= biomeLayer.m_FillUpTo; l++)
								{
									_chunk.SetBlockRaw(j, l, i, blockValue);
									_chunk.SetDensity(j, l, i, density);
								}
								if (block.blockMaterial.IsLiquid)
								{
									_chunk.SetHeight(j, i, (byte)biomeLayer.m_FillUpTo);
								}
								if (block.shape.IsTerrain())
								{
									_chunk.SetTerrainHeight(j, i, (byte)biomeLayer.m_FillUpTo);
								}
							}
						}
						bool flag = true;
						int num6 = 0;
						while (num6 < biomeDefinition.m_Layers.Count && num5 >= 0)
						{
							BiomeLayer biomeLayer2 = biomeDefinition.m_Layers[num6];
							int num7 = biomeLayer2.m_Depth;
							if (num7 == -1)
							{
								num7 = (int)b - biomeDefinition.TotalLayerDepth;
							}
							int num8 = 0;
							while (num8 < num7 && num5 >= 0)
							{
								if (num6 != biomeDefinition.m_Layers.Count - 1 && num5 < biomeDefinition.m_Layers[biomeDefinition.m_Layers.Count - 1].m_Depth)
								{
									num6 = biomeDefinition.m_Layers.Count - 2;
									break;
								}
								BlockValue blockValue2 = BlockValue.Air;
								if (this.isTerrainAt(j, num5, i))
								{
									if (flag)
									{
										blockValue2 = this.biomeProvider.GetTopmostBlockValue(blockWorldPosX, blockWorldPosZ);
										flag = false;
									}
									int count = biomeLayer2.m_Resources.Count;
									if (count > 0 && GameUtils.GetOreNoiseAt(this.perlinNoise, blockWorldPosX, num5, blockWorldPosZ) > 0f)
									{
										float randomFloat = _random.RandomFloat;
										for (int m = 0; m < count; m++)
										{
											if (randomFloat < biomeLayer2.SumResourceProbs[m])
											{
												blockValue2 = biomeLayer2.m_Resources[m].blockValue;
												break;
											}
										}
									}
									if (blockValue2.isair)
									{
										blockValue2 = biomeLayer2.m_Block.blockValue;
									}
									_chunk.SetBlockRaw(j, num5, i, blockValue2);
								}
								this.fillDensityInBlock(_chunk, j, num5, i, blockValue2);
								num8++;
								num5--;
							}
							num6++;
						}
					}
				}
			}
		}
	}

	public virtual float GetTerrainHeightAt(int x, int z)
	{
		return 0f;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public TerrainGeneratorWithBiomeResource()
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IBiomeProvider biomeProvider;

	[PublicizedFrom(EAccessModifier.Private)]
	public TS_PerlinNoise perlinNoise;
}
