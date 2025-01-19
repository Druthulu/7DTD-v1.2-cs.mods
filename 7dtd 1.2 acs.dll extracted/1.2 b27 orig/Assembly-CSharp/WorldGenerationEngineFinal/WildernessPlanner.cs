using System;
using System.Collections;
using System.Collections.Generic;
using UniLinq;
using UnityEngine;

namespace WorldGenerationEngineFinal
{
	public class WildernessPlanner
	{
		public WildernessPlanner(WorldBuilder _worldBuilder)
		{
			this.worldBuilder = _worldBuilder;
		}

		public IEnumerator Plan(DynamicProperties thisWorldProperties, int worldSeed)
		{
			yield return null;
			int count = this.worldBuilder.GetCount("wilderness", this.worldBuilder.Wilderness);
			int tries = 20;
			MicroStopwatch ms = new MicroStopwatch(true);
			int wildernessPOIsLeft = count;
			if (wildernessPOIsLeft == 0)
			{
				wildernessPOIsLeft = 200;
				Log.Warning("No wilderness settings in rwgmixer for this world size, using default count of {0}", new object[]
				{
					wildernessPOIsLeft
				});
			}
			int totalWildernessPOIs = wildernessPOIsLeft;
			List<StreetTile> validWildernessTiles = this.GetUnusedWildernessTiles();
			this.WildernessPathInfos.Clear();
			int seed = worldSeed + 409651;
			GameRandom rnd = GameRandomManager.Instance.CreateGameRandom(seed);
			while (wildernessPOIsLeft > 0)
			{
				validWildernessTiles = this.GetUnusedWildernessTiles();
				if (validWildernessTiles.Count == 0)
				{
					break;
				}
				if (tries <= 0)
				{
					int num = wildernessPOIsLeft;
					wildernessPOIsLeft = num - 1;
					tries = 20;
				}
				if (this.worldBuilder.IsMessageElapsed())
				{
					yield return this.worldBuilder.SetMessage(string.Format(Localization.Get("xuiRwgWildernessPOIs", false), Mathf.FloorToInt(100f * (1f - (float)wildernessPOIsLeft / (float)totalWildernessPOIs))), false, false);
				}
				StreetTile streetTile = validWildernessTiles[WildernessPlanner.getLowBiasedRandom(rnd, 0, validWildernessTiles.Count)];
				if (!streetTile.Used && streetTile.SpawnPrefabs())
				{
					streetTile.Used = true;
					tries = 0;
				}
				else
				{
					int num = tries;
					tries = num - 1;
				}
			}
			GameRandomManager.Instance.FreeGameRandom(rnd);
			this.WildernessPathInfos.Sort((WorldBuilder.WildernessPathInfo wp1, WorldBuilder.WildernessPathInfo wp2) => wp2.PathRadius.CompareTo(wp1.PathRadius));
			Log.Out(string.Format("WildernessPlanner Plan {0} prefabs spawned, in {1}, r={2:x}", this.worldBuilder.WildernessPrefabCount, (float)ms.ElapsedMilliseconds * 0.001f, Rand.Instance.PeekSample()));
			yield break;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static int getLowBiasedRandom(GameRandom rnd, int min, int max)
		{
			return Mathf.FloorToInt(Mathf.Abs(rnd.RandomRange(0f, 1f) - rnd.RandomRange(0f, 1f)) * (1f + (float)(max - 1) - (float)min) + (float)min);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public List<StreetTile> GetUnusedWildernessTiles()
		{
			new Vector2i(this.worldBuilder.WorldSize / 2, this.worldBuilder.WorldSize / 2);
			return (from StreetTile st in this.worldBuilder.StreetTileMap
			where !st.OverlapsRadiation && !st.AllIsWater && (st.District == null || st.District.name == "wilderness") && !st.Used && !WildernessPlanner.hasTownshipNeighbor(st) && !WildernessPlanner.hasPrefabNeighbor(st)
			orderby this.distanceFromClosestTownship(st) descending
			select st).ToList<StreetTile>();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static bool hasTownshipNeighbor(StreetTile st)
		{
			foreach (StreetTile streetTile in st.GetNeighbors8way())
			{
				if (streetTile.Township != null && streetTile.Township.GetTypeName() != "wilderness")
				{
					return true;
				}
			}
			return false;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public int distanceFromClosestTownship(StreetTile st)
		{
			int num = int.MaxValue;
			foreach (Township township in this.worldBuilder.Townships)
			{
				int num2 = Vector2i.DistanceSqrInt(st.WorldPositionCenter, this.worldBuilder.GetStreetTileGrid(township.GridCenter).WorldPositionCenter);
				if (num2 < num)
				{
					num = num2;
				}
			}
			return num;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static bool hasPrefabNeighbor(StreetTile st)
		{
			StreetTile[] neighbors8way = st.GetNeighbors8way();
			for (int i = 0; i < neighbors8way.Length; i++)
			{
				if (neighbors8way[i].HasPrefabs)
				{
					return true;
				}
			}
			return false;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public Vector2 getClosestConnectionPosition(Vector2 startPos, int _wildernessId, float _radius = 4f, BiomeType _biome = BiomeType.forest)
		{
			float num = 2.14748365E+09f;
			Vector2 result = Vector2.zero;
			bool flag = false;
			if (this.worldBuilder.paths.Count > 0)
			{
				foreach (Path path in this.worldBuilder.paths)
				{
					foreach (Vector2 vector in path.FinalPathPoints)
					{
						float num2 = WildernessPlanner.distanceSqr(startPos, vector);
						if (num2 < num)
						{
							num = num2;
							result = vector;
							flag = true;
						}
					}
				}
			}
			if (this.worldBuilder.wildernessPaths.Count > 0)
			{
				foreach (Path path2 in this.worldBuilder.wildernessPaths)
				{
					if ((!path2.IsPrefabPath || this.worldBuilder.Townships.Count <= 0) && path2.StartPointID != _wildernessId && path2.EndPointID != _wildernessId && path2.radius >= _radius)
					{
						for (int i = 2; i < path2.FinalPathPoints.Count - 2; i++)
						{
							Vector2 vector2 = path2.FinalPathPoints[i];
							float num3 = WildernessPlanner.distanceSqr(startPos, vector2);
							if (num3 < num && _biome == this.worldBuilder.GetBiome((int)vector2.x, (int)vector2.y))
							{
								num = num3;
								result = vector2;
								flag = true;
							}
						}
					}
				}
			}
			if (flag)
			{
				return result;
			}
			return startPos;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public int getClosestConnectionDirection(Vector2 startPos, int _wildernessId, float _radius = 4f, BiomeType _biome = BiomeType.forest)
		{
			Vector2 vector = this.getClosestConnectionPosition(startPos, _wildernessId, _radius, _biome);
			vector -= startPos;
			if (vector.x + vector.y == 0f)
			{
				return -1;
			}
			vector.Normalize();
			if (Mathf.Abs(vector.x) > Mathf.Abs(vector.y))
			{
				if (vector.x > 0f)
				{
					return 1;
				}
				return 3;
			}
			else
			{
				if (vector.y > 0f)
				{
					return 0;
				}
				return 2;
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static float distanceSqr(Vector2 pointA, Vector2 pointB)
		{
			Vector2 vector = pointA - pointB;
			return vector.x * vector.x + vector.y * vector.y;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public const int maxWildernessSpawnTries = 20;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly WorldBuilder worldBuilder;

		public readonly List<WorldBuilder.WildernessPathInfo> WildernessPathInfos = new List<WorldBuilder.WildernessPathInfo>();
	}
}
