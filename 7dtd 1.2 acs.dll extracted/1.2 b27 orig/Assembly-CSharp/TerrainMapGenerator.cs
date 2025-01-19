using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class TerrainMapGenerator
{
	public void GenerateTerrain(IChunkProvider _chunkProvider)
	{
		World world = GameManager.Instance.World;
		this.xs = world.GetPrimaryPlayer().GetBlockPosition().x - 512;
		this.zs = world.GetPrimaryPlayer().GetBlockPosition().z - 512;
		MicroStopwatch microStopwatch = new MicroStopwatch();
		long elapsedMilliseconds = microStopwatch.ElapsedMilliseconds;
		int num = 1;
		HashSetLong hashSetLong = new HashSetLong();
		for (int i = 0; i < 1025; i++)
		{
			for (int j = 0; j < 1025; j++)
			{
				int num2 = (this.xs + i) * num;
				int num3 = (this.zs + j) * num;
				long item = WorldChunkCache.MakeChunkKey(num2 / 16, num3 / 16);
				if (!hashSetLong.Contains(item))
				{
					hashSetLong.Add(item);
					_chunkProvider.RequestChunk(num2 / 16, num3 / 16);
				}
			}
		}
		Log.Out("Request chunks: " + (microStopwatch.ElapsedMilliseconds - elapsedMilliseconds).ToString());
		elapsedMilliseconds = microStopwatch.ElapsedMilliseconds;
		bool flag = false;
		do
		{
			flag = false;
			foreach (long key in hashSetLong)
			{
				if (GameManager.Instance.World.GetChunkSync(key) == null)
				{
					flag = true;
					break;
				}
			}
			Thread.Sleep(300);
		}
		while (flag);
		Log.Out("Generate chunks: " + (microStopwatch.ElapsedMilliseconds - elapsedMilliseconds).ToString());
		elapsedMilliseconds = microStopwatch.ElapsedMilliseconds;
		for (int k = 0; k < 1025; k++)
		{
			for (int l = 0; l < 1025; l++)
			{
				int num4 = (this.xs + k) * num;
				int num5 = (this.zs + l) * num;
				long key2 = WorldChunkCache.MakeChunkKey(num4 / 16, num5 / 16);
				Chunk chunk = (Chunk)GameManager.Instance.World.GetChunkSync(key2);
				int x = World.toBlockXZ(num4);
				int z = World.toBlockXZ(num5);
				BiomeDefinition biome = world.Biomes.GetBiome(chunk.GetBiomeId(x, z));
				this.biomeDefs[k + l * 1025] = biome;
				this.heights[k + l * 1025] = (int)chunk.GetHeight(x, z);
				this.blockValues[k + l * 1025] = chunk.GetBlock(x, this.heights[k + l * 1025], z);
				sbyte density = chunk.GetDensity(x, this.heights[k + l * 1025], z);
				this.densitySub[k + l * 1025] = 1f - (float)density / -128f;
				this.subbiome[k + l * 1025] = (int)biome.m_Id;
			}
		}
		Log.Out("Cache data: " + (microStopwatch.ElapsedMilliseconds - elapsedMilliseconds).ToString());
		elapsedMilliseconds = microStopwatch.ElapsedMilliseconds;
		for (int m = 0; m < 1024; m++)
		{
			for (int n = 0; n < 1024; n++)
			{
				BiomeDefinition biomeDefinition = this.biomeDefs[m + n * 1025];
				if (biomeDefinition != null && biomeDefinition.m_Layers.Count != 0)
				{
					int num6 = this.heights[m + n * 1025];
					int num7 = this.subbiome[m + n * 1025];
					float num8 = this.biomeIntens[m + n * 1025];
					BlockValue blockValue = this.blockValues[m + n * 1025];
					Vector3 vector = Vector3.up;
					Color color = BlockLiquidv2.Color;
					bool flag2 = world.IsWater(m, num6, n);
					if (!flag2)
					{
						vector = this.calcNormal(m, n, 0, 1, 1, 0);
						if (m > 0 && n > 0)
						{
							Vector3 b = this.calcNormal(m, n, 0, -1, -1, 0);
							Vector3 b2 = this.calcNormal(m, n, 1, 1, 1, -1);
							Vector3 b3 = this.calcNormal(m, n, -1, -1, -1, 1);
							vector = (vector + b + b2 + b3) * 0.25f;
							vector = vector.normalized;
						}
					}
					vector = new Vector3(vector.z, vector.x, vector.y);
					this.normalArray[m + n * 1024] = new Color((vector.x + 1f) / 2f, (vector.y + 1f) / 2f, (vector.z + 1f) / 2f, 1f);
					if (!flag2)
					{
						color = blockValue.Block.GetMapColor(blockValue, vector, num6);
					}
					this.colArray[m + n * 1024] = color;
					this.colArray[m + n * 1024] = new Color(this.colArray[m + n * 1024].r, this.colArray[m + n * 1024].g, this.colArray[m + n * 1024].b, 1f);
				}
			}
		}
		Log.Out("Create image: " + (microStopwatch.ElapsedMilliseconds - elapsedMilliseconds).ToString());
		elapsedMilliseconds = microStopwatch.ElapsedMilliseconds;
		Texture2D texture2D = new Texture2D(1024, 1024);
		texture2D.SetPixels(this.colArray);
		texture2D.Apply();
		TextureUtils.SaveTexture(texture2D, "Map.png");
		UnityEngine.Object.Destroy(texture2D);
		GCUtils.UnloadAndCollectStart();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 calcNormal(int x, int z, int _xAdd1, int _zAdd1, int _xAdd2, int _zAdd2)
	{
		Vector3 up = Vector3.up;
		float y = (float)this.heights[x + z * 1025] - this.densitySub[x + z * 1025];
		float y2 = (float)this.heights[x + _xAdd1 + (z + _zAdd1) * 1025] - this.densitySub[x + _xAdd1 + (z + _zAdd1) * 1025];
		float y3 = (float)this.heights[x + _xAdd2 + (z + _zAdd2) * 1025] - this.densitySub[x + _xAdd2 + (z + _zAdd2) * 1025];
		Vector3 lhs = new Vector3((float)_xAdd1, y3, (float)_zAdd1) - new Vector3(0f, y, 0f);
		Vector3 rhs = new Vector3((float)_xAdd2, y2, (float)_zAdd2) - new Vector3(0f, y, 0f);
		return Vector3.Cross(lhs, rhs).normalized;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int xs;

	[PublicizedFrom(EAccessModifier.Private)]
	public int zs;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int w = 1024;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int h = 1024;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color[] colArray = new Color[1048576];

	[PublicizedFrom(EAccessModifier.Private)]
	public Color[] normalArray = new Color[1048576];

	[PublicizedFrom(EAccessModifier.Private)]
	public int[] heights = new int[1050625];

	[PublicizedFrom(EAccessModifier.Private)]
	public float[] densitySub = new float[1050625];

	[PublicizedFrom(EAccessModifier.Private)]
	public float[] biomeIntens = new float[1050625];

	[PublicizedFrom(EAccessModifier.Private)]
	public BlockValue[] blockValues = new BlockValue[1050625];

	[PublicizedFrom(EAccessModifier.Private)]
	public BiomeDefinition[] biomeDefs = new BiomeDefinition[1050625];

	[PublicizedFrom(EAccessModifier.Private)]
	public int[] subbiome = new int[1050625];
}
