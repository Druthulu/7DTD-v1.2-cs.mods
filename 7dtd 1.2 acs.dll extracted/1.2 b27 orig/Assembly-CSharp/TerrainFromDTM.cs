using System;

[PublicizedFrom(EAccessModifier.Internal)]
public class TerrainFromDTM : TerrainGeneratorWithBiomeResource
{
	public void Init(ArrayWithOffset<byte> _dtm, IBiomeProvider _biomeProvider, string levelName, int _seed)
	{
		base.Init(null, _biomeProvider, _seed);
		this.m_DTM = _dtm;
		this.heightData = HeightMapUtils.ConvertDTMToHeightData(levelName);
		this.heightData = HeightMapUtils.SmoothTerrain(5, this.heightData);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void fillDensityInBlock(Chunk _chunk, int _x, int _y, int _z, BlockValue _bv)
	{
		int num = (_chunk.X << 4) + _x + this.heightData.GetLength(0) / 2;
		int num2 = (_chunk.Z << 4) + _z + this.heightData.GetLength(1) / 2;
		if (num < 0 || num2 < 0 || num >= this.heightData.GetLength(0) || num2 >= this.heightData.GetLength(1))
		{
			return;
		}
		float num3 = this.heightData[num, num2] + 0.5f;
		int num4 = (int)num3;
		sbyte density;
		if (_y < num4)
		{
			density = MarchingCubes.DensityTerrain;
		}
		else if (_y > num4 + 1)
		{
			density = MarchingCubes.DensityAir;
		}
		else
		{
			float num5 = num3 - (float)num4;
			if (num4 == _y)
			{
				density = (sbyte)((float)MarchingCubes.DensityTerrain * num5);
			}
			else
			{
				density = (sbyte)((float)MarchingCubes.DensityAir * (1f - num5));
			}
		}
		_chunk.SetDensity(_x, _y, _z, density);
	}

	public override sbyte GetDensityAt(int _xWorld, int _yWorld, int _zWorld, BiomeDefinition _bd, float _biomeIntensity)
	{
		if ((int)this.m_DTM[_xWorld, _zWorld] > _yWorld)
		{
			return MarchingCubes.DensityAir;
		}
		return MarchingCubes.DensityTerrain;
	}

	public override byte GetTerrainHeightAt(int _x, int _z, BiomeDefinition _bd, float _biomeIntensity)
	{
		int num = _x + this.heightData.GetLength(0) / 2;
		int num2 = _z + this.heightData.GetLength(1) / 2;
		if (num < 0 || num2 < 0 || num >= this.heightData.GetLength(0) || num2 >= this.heightData.GetLength(1))
		{
			return 0;
		}
		return (byte)((int)(this.heightData[num, num2] + 0.5f));
	}

	public override float GetTerrainHeightAt(int x, int z)
	{
		float result;
		try
		{
			x += this.heightData.GetLength(0) / 2;
			z += this.heightData.GetLength(1) / 2;
			result = this.heightData[x, z];
		}
		catch (Exception)
		{
			result = 0f;
		}
		return result;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public ArrayWithOffset<byte> m_DTM;

	[PublicizedFrom(EAccessModifier.Private)]
	public float[,] heightData;
}
