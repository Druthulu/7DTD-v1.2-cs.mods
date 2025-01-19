using System;

[PublicizedFrom(EAccessModifier.Internal)]
public class TerrainFromChunk : TerrainGeneratorWithBiomeResource
{
	public void Init(RegionFileManager _regionFileManager, IBiomeProvider _biomeProvider, int _seed)
	{
		base.Init(null, _biomeProvider, _seed);
		this.regionFileManager = _regionFileManager;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void checkTerrainChunk(int _x, int _z)
	{
		int num = World.toChunkXZ(_x);
		int num2 = World.toChunkXZ(_z);
		if (this.terrainChunk == null || this.terrainChunk.X != num || this.terrainChunk.Z != num2)
		{
			this.terrainChunk = this.regionFileManager.GetChunkSync(WorldChunkCache.MakeChunkKey(num, num2));
		}
	}

	public override byte GetTerrainHeightAt(int _x, int _z, BiomeDefinition _bd, float _intensity)
	{
		this.checkTerrainChunk(_x, _z);
		if (this.terrainChunk == null)
		{
			return 0;
		}
		return this.terrainChunk.GetHeight(World.toBlockXZ(_x), World.toBlockXZ(_z));
	}

	public override sbyte GetDensityAt(int _xWorld, int _yWorld, int _zWorld, BiomeDefinition _bd, float _biomeIntensity)
	{
		this.checkTerrainChunk(_xWorld, _zWorld);
		if (this.terrainChunk == null)
		{
			return MarchingCubes.DensityAir;
		}
		return this.terrainChunk.GetDensity(_xWorld, _yWorld, _zWorld);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool isTerrainAt(int _x, int _y, int _z)
	{
		return !this.terrainChunk.GetBlockNoDamage(_x, _y, _z).isair;
	}

	public void SetTerrainChunk(Chunk _terrainChunk)
	{
		this.terrainChunk = _terrainChunk;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void fillDensityInBlock(Chunk _chunk, int _x, int _y, int _z, BlockValue _bv)
	{
		sbyte b = this.terrainChunk.GetDensity(_x, _y, _z);
		if (_bv.Block.shape.IsTerrain() && b >= 0)
		{
			b = -1;
		}
		_chunk.SetDensity(_x, _y, _z, b);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Chunk terrainChunk;

	[PublicizedFrom(EAccessModifier.Private)]
	public RegionFileManager regionFileManager;
}
