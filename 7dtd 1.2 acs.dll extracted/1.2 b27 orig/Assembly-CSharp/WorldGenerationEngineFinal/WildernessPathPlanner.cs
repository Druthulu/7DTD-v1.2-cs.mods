using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WorldGenerationEngineFinal
{
	public class WildernessPathPlanner
	{
		public WildernessPathPlanner(WorldBuilder _worldBuilder)
		{
			this.worldBuilder = _worldBuilder;
		}

		public IEnumerator Plan(int worldSeed)
		{
			MicroStopwatch microStopwatch = new MicroStopwatch(true);
			bool flag = this.worldBuilder.paths.Count > 0;
			for (int i = 0; i < this.worldBuilder.WildernessPlanner.WildernessPathInfos.Count; i++)
			{
				WorldBuilder.WildernessPathInfo wildernessPathInfo = this.worldBuilder.WildernessPlanner.WildernessPathInfos[i];
				if (wildernessPathInfo.Path == null)
				{
					Vector2i endPosition = Vector2i.zero;
					float num = float.MaxValue;
					bool connectsToHighway = false;
					foreach (Path path in this.worldBuilder.paths)
					{
						foreach (Vector2 vector in path.FinalPathPoints)
						{
							float num2 = Vector2i.Distance(wildernessPathInfo.Position, new Vector2i(vector));
							if (num2 < num)
							{
								num = num2;
								endPosition.x = (int)vector.x;
								endPosition.y = (int)vector.y;
								connectsToHighway = true;
							}
						}
					}
					foreach (WorldBuilder.WildernessPathInfo wildernessPathInfo2 in this.worldBuilder.WildernessPlanner.WildernessPathInfos)
					{
						if (wildernessPathInfo2 != wildernessPathInfo)
						{
							if (wildernessPathInfo2.Path == null)
							{
								if (!flag)
								{
									float num3 = Vector2i.Distance(wildernessPathInfo.Position, wildernessPathInfo2.Position);
									if (num3 < num)
									{
										num = num3;
										endPosition = wildernessPathInfo2.Position;
									}
								}
							}
							else if (wildernessPathInfo2.Path.connectsToHighway)
							{
								foreach (Vector2 vector2 in wildernessPathInfo2.Path.FinalPathPoints)
								{
									float num4 = Vector2i.Distance(wildernessPathInfo.Position, new Vector2i(vector2));
									if (num4 < num)
									{
										num = num4;
										endPosition.x = (int)vector2.x;
										endPosition.y = (int)vector2.y;
									}
								}
							}
						}
					}
					if (num < 3.40282347E+38f)
					{
						this.worldBuilder.IsMessageElapsed();
						Path path2 = new Path(this.worldBuilder, wildernessPathInfo.Position, endPosition, wildernessPathInfo.PathRadius, true, false);
						if (path2.IsValid)
						{
							path2.connectsToHighway = connectsToHighway;
							wildernessPathInfo.Path = path2;
							this.worldBuilder.wildernessPaths.Add(path2);
							this.createTraderSpawnIfAble(path2.FinalPathPoints);
						}
					}
				}
			}
			Log.Out(string.Format("WildernessPathPlanner Plan #{0} in {1}, r={2:x}", this.worldBuilder.WildernessPlanner.WildernessPathInfos.Count, (float)microStopwatch.ElapsedMilliseconds * 0.001f, Rand.Instance.PeekSample()));
			yield return null;
			yield break;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void createTraderSpawnIfAble(List<Vector2> pathPoints)
		{
			if (pathPoints.Count < 5)
			{
				return;
			}
			if (this.worldBuilder.ForestBiomeWeight > 0)
			{
				BiomeType biomeType = BiomeType.none;
				for (int i = 2; i < pathPoints.Count - 2; i++)
				{
					biomeType = this.worldBuilder.GetBiome((int)pathPoints[i].x, (int)pathPoints[i].y);
					if (biomeType == BiomeType.forest)
					{
						break;
					}
				}
				if (biomeType != BiomeType.forest)
				{
					return;
				}
			}
			for (int j = 2; j < pathPoints.Count - 2; j++)
			{
				if (this.worldBuilder.ForestBiomeWeight <= 0 || this.worldBuilder.GetBiome((int)pathPoints[j].x, (int)pathPoints[j].y) == BiomeType.forest)
				{
					Vector2i vector2i;
					vector2i.x = (int)pathPoints[j].x;
					vector2i.y = (int)pathPoints[j].y;
					StreetTile streetTileWorld = this.worldBuilder.GetStreetTileWorld(vector2i);
					if (streetTileWorld != null && streetTileWorld.HasPrefabs)
					{
						bool flag = true;
						using (List<PrefabDataInstance>.Enumerator enumerator = streetTileWorld.StreetTilePrefabDatas.GetEnumerator())
						{
							while (enumerator.MoveNext())
							{
								if (enumerator.Current.prefab.DifficultyTier > 1)
								{
									flag = false;
									break;
								}
							}
						}
						if (flag)
						{
							this.worldBuilder.CreatePlayerSpawn(vector2i, false);
						}
					}
				}
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public int getMaxTraderDistance()
		{
			return (int)(0.1f * (float)this.worldBuilder.WorldSize);
		}

		public IEnumerator Plan2(int worldSeed)
		{
			yield return this.worldBuilder.SetMessage(Localization.Get("xuiRwgWildernessPaths", false), false, false);
			List<List<WorldBuilder.WildernessPathInfo>> list = new List<List<WorldBuilder.WildernessPathInfo>>();
			for (byte b = 0; b < 4; b += 1)
			{
				list.Add(new List<WorldBuilder.WildernessPathInfo>());
				foreach (WorldBuilder.WildernessPathInfo wildernessPathInfo in this.worldBuilder.WildernessPlanner.WildernessPathInfos)
				{
					if (wildernessPathInfo.Biome == (BiomeType)b)
					{
						list[(int)b].Add(wildernessPathInfo);
					}
				}
			}
			byte b2 = 0;
			while ((int)b2 < list.Count)
			{
				List<WorldBuilder.WildernessPathInfo> list2 = list[(int)b2];
				WildernessPathPlanner.Shuffle<WorldBuilder.WildernessPathInfo>(worldSeed + "CountryRoadPlanner.Plan".GetHashCode(), ref list2);
				if (list2.Count != 0)
				{
					for (WildernessPathPlanner.WildernessConnectionNode wildernessConnectionNode = this.primsAlgo(list2[0], false); wildernessConnectionNode != null; wildernessConnectionNode = wildernessConnectionNode.next)
					{
						if (wildernessConnectionNode.Path != null)
						{
							this.worldBuilder.wildernessPaths.Add(wildernessConnectionNode.Path);
						}
						if (wildernessConnectionNode.next == null)
						{
							break;
						}
					}
				}
				b2 += 1;
			}
			yield break;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public WildernessPathPlanner.WildernessConnectionNode primsAlgo(WorldBuilder.WildernessPathInfo startingWildernessPOI, bool onlyNonConnected = false)
		{
			List<WorldBuilder.WildernessPathInfo> list = new List<WorldBuilder.WildernessPathInfo>();
			WildernessPathPlanner.WildernessConnectionNode wildernessConnectionNode = new WildernessPathPlanner.WildernessConnectionNode(startingWildernessPOI);
			WildernessPathPlanner.WildernessConnectionNode wildernessConnectionNode2 = wildernessConnectionNode;
			Vector2i endPosition = Vector2i.zero;
			while (wildernessConnectionNode2 != null)
			{
				int num = 262144;
				bool flag = false;
				WorldBuilder.WildernessPathInfo wildernessPathInfo = null;
				Vector2i position = wildernessConnectionNode2.PathInfo.Position;
				foreach (WorldBuilder.WildernessPathInfo wildernessPathInfo2 in this.worldBuilder.WildernessPlanner.WildernessPathInfos)
				{
					if (!list.Contains(wildernessPathInfo2))
					{
						int num2 = Vector2i.DistanceSqrInt(wildernessPathInfo2.Position, position);
						if (num2 < num && this.worldBuilder.PathingUtils.HasValidPath(position, wildernessPathInfo2.Position, true))
						{
							endPosition = wildernessPathInfo2.Position;
							num = num2;
							wildernessPathInfo = wildernessPathInfo2;
							flag = true;
						}
					}
				}
				if (!flag)
				{
					wildernessConnectionNode2 = wildernessConnectionNode2.next;
				}
				else
				{
					wildernessConnectionNode2.Path = new Path(this.worldBuilder, position, endPosition, wildernessConnectionNode2.PathInfo.PathRadius, true, false);
					if (!wildernessConnectionNode2.Path.IsValid)
					{
						wildernessConnectionNode2.Path = null;
						wildernessConnectionNode2 = wildernessConnectionNode2.next;
					}
					else
					{
						list.Add(wildernessPathInfo);
						wildernessConnectionNode2.next = new WildernessPathPlanner.WildernessConnectionNode(wildernessPathInfo);
						wildernessConnectionNode2 = wildernessConnectionNode2.next;
					}
				}
			}
			return wildernessConnectionNode;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static void Shuffle<T>(int seed, ref List<T> list)
		{
			int i = list.Count;
			GameRandom gameRandom = GameRandomManager.Instance.CreateGameRandom(seed);
			while (i > 1)
			{
				i--;
				int index = gameRandom.RandomRange(0, i) % i;
				T value = list[index];
				list[index] = list[i];
				list[i] = value;
			}
			GameRandomManager.Instance.FreeGameRandom(gameRandom);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly WorldBuilder worldBuilder;

		[PublicizedFrom(EAccessModifier.Private)]
		public class WildernessConnectionNode
		{
			public WildernessConnectionNode(WorldBuilder.WildernessPathInfo wpi)
			{
				this.PathInfo = wpi;
			}

			public WildernessPathPlanner.WildernessConnectionNode next;

			public WorldBuilder.WildernessPathInfo PathInfo;

			public Path Path;

			public float Distance;
		}
	}
}
