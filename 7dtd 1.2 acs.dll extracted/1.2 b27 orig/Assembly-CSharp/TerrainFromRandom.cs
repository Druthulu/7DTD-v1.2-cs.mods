using System;
using UnityEngine;

public class TerrainFromRandom : TerrainGeneratorWithBiomeResource
{
	public override void Init(World _world, IBiomeProvider _biomeProvider, int _seed)
	{
		base.Init(_world, _biomeProvider, _seed);
	}

	public override byte GetTerrainHeightAt(int worldX, int worldZ, BiomeDefinition _bd, float _biomeIntens)
	{
		TGMAbstract tgmabstract = (_bd.m_Terrain != null) ? _bd.m_Terrain : this.defaultGenerator;
		this.heightXZ = tgmabstract.GetValue((float)worldX * 0.00390625f, (float)worldZ * 0.00390625f, _biomeIntens);
		this.heightXp1Z = tgmabstract.GetValue((float)(worldX + 1) * 0.00390625f, (float)worldZ * 0.00390625f, _biomeIntens);
		this.heightXZp1 = tgmabstract.GetValue((float)worldX * 0.00390625f, (float)(worldZ + 1) * 0.00390625f, _biomeIntens);
		this.heightXm1Z = tgmabstract.GetValue((float)(worldX - 1) * 0.00390625f, (float)worldZ * 0.00390625f, _biomeIntens);
		this.heightXZm1 = tgmabstract.GetValue((float)worldX * 0.00390625f, (float)(worldZ - 1) * 0.00390625f, _biomeIntens);
		this.minYAround = Utils.FastMin(this.heightXp1Z, this.heightXZp1, this.heightXm1Z, this.heightXZm1);
		this.maxYAround = Utils.FastMax(this.heightXp1Z, this.heightXZp1, this.heightXm1Z, this.heightXZm1);
		this.baseHeight = tgmabstract.GetBaseHeight();
		this.heightAv = (this.heightXZ + this.heightXp1Z + this.heightXZp1 + this.heightXm1Z + this.heightXZm1) / 5f;
		return (byte)(this.heightAv + this.baseHeight);
	}

	public override sbyte GetDensityAt(int _xWorld, int _yWorld, int _zWorld, BiomeDefinition _bd, float _biomeIntensity)
	{
		return (sbyte)Utils.FastClamp(((float)_yWorld - (this.heightAv + this.baseHeight)) * 127f, -128f, 127f);
	}

	public override Vector3 GetTerrainNormalAt(int _xWorld, int _zWorld, BiomeDefinition _bd, float _biomeIntensity)
	{
		return ((_bd.m_Terrain != null) ? _bd.m_Terrain : this.defaultGenerator).GetNormal((float)_xWorld * 0.00390625f, (float)_zWorld * 0.00390625f, _biomeIntensity);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool isTerrainAt(int _x, int _y, int _z)
	{
		return (float)_y < this.heightAv + this.baseHeight;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void fillDensityInBlock(Chunk _chunk, int _x, int _y, int _z, BlockValue _bv)
	{
		float num = (float)_y - (this.heightAv + this.baseHeight);
		sbyte b;
		if (num >= 0f)
		{
			if ((float)(_y - 1) < this.heightAv + this.baseHeight && this.minYAround + 1f < this.heightAv && this.maxYAround - 1f > this.heightAv)
			{
				float num2 = this.heightAv + this.baseHeight - (float)_y;
				float num3 = this.heightAv - this.minYAround;
				float num4 = num2 / num3;
				if (num4 > 1f)
				{
					num4 = 1f;
				}
				num = -1f * num4;
				b = (sbyte)Utils.FastClamp(num * 127f, -128f, 127f);
				if (_y < 255)
				{
					int density = (int)_chunk.GetDensity(_x, _y + 1, _z);
					_chunk.SetDensity(_x, _y + 1, _z, (sbyte)((density + (int)b) / 2));
				}
			}
		}
		else if (this.minYAround + 1f < this.heightAv && this.maxYAround - 1f > this.heightAv)
		{
			float num5 = this.heightAv + this.baseHeight - (float)_y;
			float num6 = this.heightAv - this.minYAround;
			float num7 = num5 / num6;
			if (num7 > 1f)
			{
				num7 = 1f;
			}
			num = -1f * num7;
		}
		b = (sbyte)Utils.FastClamp(num * 127f, -128f, 127f);
		if (num < 0f && b == 0)
		{
			b = -1;
		}
		_chunk.SetDensity(_x, _y, _z, b);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public float baseHeight;

	[PublicizedFrom(EAccessModifier.Private)]
	public float heightAv;

	[PublicizedFrom(EAccessModifier.Private)]
	public float heightXZ;

	[PublicizedFrom(EAccessModifier.Private)]
	public float heightXp1Z;

	[PublicizedFrom(EAccessModifier.Private)]
	public float heightXZp1;

	[PublicizedFrom(EAccessModifier.Private)]
	public float heightXm1Z;

	[PublicizedFrom(EAccessModifier.Private)]
	public float heightXZm1;

	[PublicizedFrom(EAccessModifier.Private)]
	public float minYAround;

	[PublicizedFrom(EAccessModifier.Private)]
	public float maxYAround;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int SAMPLE_RATE_3D_HOR = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int SAMPLE_RATE_3D_VERT = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	public TGMAbstract defaultGenerator = new TerrainFromRandom.DefaultTGM();

	[PublicizedFrom(EAccessModifier.Private)]
	public const float xzStep = 0.00390625f;

	[PublicizedFrom(EAccessModifier.Private)]
	public class DefaultTGM : TGMAbstract
	{
		public override void SetSeed(int _seed)
		{
		}

		public override float GetValue(float _x, float _z, float _biomeIntens)
		{
			return 1f;
		}
	}
}
