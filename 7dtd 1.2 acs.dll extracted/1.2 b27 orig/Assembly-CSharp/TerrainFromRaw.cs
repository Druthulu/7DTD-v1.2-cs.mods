using System;
using System.Collections.Generic;
using UnityEngine;

[PublicizedFrom(EAccessModifier.Internal)]
public class TerrainFromRaw : TerrainGeneratorWithBiomeResource
{
	public void Init(HeightMap _heightMap, IBiomeProvider _biomeProvider, string levelName, int _seed)
	{
		base.Init(null, _biomeProvider, _seed);
		this.heightMap = _heightMap;
		this.terrainWidth = this.heightMap.GetWidth() << this.heightMap.GetScaleShift();
		this.terrainHeight = this.heightMap.GetHeight() << this.heightMap.GetScaleShift();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool checkCoordinates(ref int _x, ref int _z)
	{
		_x += this.terrainWidth / 2;
		_z += this.terrainHeight / 2;
		return _x >= 0 && _z >= 0 && _x < this.terrainWidth && _z < this.terrainHeight;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void fillDensityInBlock(Chunk _chunk, int _x, int _y, int _z, BlockValue _bv)
	{
		int x = (_chunk.X << 4) + _x;
		int z = (_chunk.Z << 4) + _z;
		if (!this.checkCoordinates(ref x, ref z))
		{
			return;
		}
		float num = this.heightMap.GetAt(x, z) + 0.5f;
		int num2 = (int)num;
		sbyte b;
		if (_y < num2)
		{
			b = MarchingCubes.DensityTerrain;
		}
		else if (_y > num2 + 1)
		{
			b = MarchingCubes.DensityAir;
		}
		else
		{
			float num3 = num - (float)num2;
			if (num2 == _y)
			{
				b = (sbyte)((float)MarchingCubes.DensityTerrain * num3);
			}
			else
			{
				b = (sbyte)((float)MarchingCubes.DensityAir * (1f - num3));
			}
			if (b == 0)
			{
				if (_bv.Block.shape.IsTerrain())
				{
					b = -1;
				}
				else
				{
					b = 1;
				}
			}
		}
		_chunk.SetDensity(_x, _y, _z, b);
	}

	public override sbyte GetDensityAt(int _xWorld, int _yWorld, int _zWorld, BiomeDefinition _bd, float _biomeIntensity)
	{
		if (!this.checkCoordinates(ref _xWorld, ref _zWorld))
		{
			return MarchingCubes.DensityAir;
		}
		if (this.heightMap.GetAt(_xWorld, _zWorld) > (float)_yWorld)
		{
			return MarchingCubes.DensityAir;
		}
		return MarchingCubes.DensityTerrain;
	}

	public override byte GetTerrainHeightAt(int _x, int _z, BiomeDefinition _bd, float _biomeIntensity)
	{
		if (!this.checkCoordinates(ref _x, ref _z))
		{
			return 0;
		}
		return (byte)((int)(this.heightMap.GetAt(_x, _z) + 0.5f));
	}

	public override float GetTerrainHeightAt(int _x, int _z)
	{
		if (!this.checkCoordinates(ref _x, ref _z))
		{
			return 0f;
		}
		return this.heightMap.GetAt(_x, _z);
	}

	public List<float[,]> ConvertToUnityHeightmap(int _sliceAtWidth)
	{
		int width = this.heightMap.GetWidth();
		int height = this.heightMap.GetHeight();
		int scaleSteps = this.heightMap.GetScaleSteps();
		List<float[,]> list = new List<float[,]>();
		int num = width / _sliceAtWidth;
		int num2 = height / _sliceAtWidth;
		new Terrain[num, num2];
		for (int i = 0; i < num2; i++)
		{
			for (int j = 0; j < num; j++)
			{
				float[,] array = new float[_sliceAtWidth, _sliceAtWidth];
				for (int k = _sliceAtWidth - 1; k >= 0; k--)
				{
					for (int l = _sliceAtWidth - 1; l >= 0; l--)
					{
						float at = this.heightMap.GetAt(j * _sliceAtWidth + k * scaleSteps, i * _sliceAtWidth + l * scaleSteps);
						array[l, k] = at / 256f;
					}
				}
				list.Add(array);
			}
		}
		return list;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int terrainWidth;

	[PublicizedFrom(EAccessModifier.Private)]
	public int terrainHeight;

	[PublicizedFrom(EAccessModifier.Private)]
	public HeightMap heightMap;
}
