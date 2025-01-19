using System;
using System.Collections;
using System.Collections.Generic;
using UniLinq;
using UnityEngine;

namespace WorldGenerationEngineFinal
{
	public class TownPlanner
	{
		public TownPlanner(WorldBuilder _worldBuilder)
		{
			this.worldBuilder = _worldBuilder;
		}

		public IEnumerator Plan(DynamicProperties _properties, int worldSeed)
		{
			MicroStopwatch ms = new MicroStopwatch(true);
			Dictionary<BiomeType, List<Vector2i>> biomeStreetTiles = this.getStreetTilesByBiome();
			Dictionary<int, TownPlanner.TownshipSpawnInfo> townshipSpawnInfos = new Dictionary<int, TownPlanner.TownshipSpawnInfo>();
			this.getTownshipCounts(_properties, townshipSpawnInfos);
			int townID = 0;
			int roadsideId = TownshipStatic.TypesByName["roadside"];
			List<int> list = new List<int>();
			townshipSpawnInfos.CopyKeysTo(list);
			list.Sort((int key1, int key2) => townshipSpawnInfos[key2].max.CompareTo(townshipSpawnInfos[key1].max));
			int rndAdd = 1982;
			GameRandom rnd = GameRandomManager.Instance.CreateGameRandom();
			Func<Vector2i, int> <>9__1;
			foreach (int townshipTypeId in list)
			{
				string townshipTypeName;
				WorldBuilder.TownshipData townshipData;
				if (TownshipStatic.NamesByType.TryGetValue(townshipTypeId, out townshipTypeName) && WorldBuilderStatic.townshipDatas.TryGetValue(townshipTypeName, out townshipData))
				{
					yield return this.worldBuilder.SetMessage(string.Format(Localization.Get("xuiRwgTownPlanning", false), townshipTypeName), false, false);
					GameRandom rnd2 = rnd;
					int num = rndAdd;
					rndAdd = num + 1;
					rnd2.SetSeed(worldSeed + num);
					bool flag = townshipTypeId == roadsideId;
					int num2 = this.worldBuilder.StreetTileMapSize / 10;
					if (flag)
					{
						num2 = this.worldBuilder.StreetTileMapSize / 8;
					}
					int num3 = this.worldBuilder.StreetTileMap.GetLength(0) - num2;
					int num4 = this.worldBuilder.StreetTileMap.GetLength(1) - num2;
					int num5 = 80;
					TownPlanner.TownshipSpawnInfo townshipSpawnInfo = townshipSpawnInfos[townshipTypeId];
					BiomeType biomeType = BiomeType.none;
					int num6 = 0;
					for (int i = 0; i < townshipSpawnInfo.count; i++)
					{
						if (biomeType == BiomeType.none)
						{
							biomeType = this.getBiomeWithMostAvailableSpace(townshipData);
						}
						List<Vector2i> list2 = null;
						BiomeType biomeType2 = biomeType;
						for (int j = 0; j < 5; j++)
						{
							List<Vector2i> collection;
							if (biomeStreetTiles.TryGetValue(biomeType, out collection))
							{
								list2 = new List<Vector2i>(collection);
								for (int k = list2.Count - 1; k >= 0; k--)
								{
									Vector2i vector2i = list2[k];
									if (vector2i.x <= num2 || vector2i.y <= num2 || vector2i.x >= num3 || vector2i.y >= num4 || this.tooClose(vector2i, flag, townshipSpawnInfo))
									{
										list2.RemoveAt(k);
									}
								}
								if (list2.Count > 0)
								{
									break;
								}
							}
							biomeType = this.nextBiomeType(biomeType, townshipData);
							if (biomeType == biomeType2)
							{
								break;
							}
						}
						if (list2 == null || list2.Count == 0)
						{
							break;
						}
						List<StreetTile> list3 = new List<StreetTile>();
						List<StreetTile> list4 = new List<StreetTile>();
						Township township = new Township(this.worldBuilder)
						{
							Type = townshipTypeId,
							BiomeType = biomeType
						};
						int num7 = townshipSpawnInfo.min;
						if (townshipSpawnInfo.max >= 10)
						{
							num7 = (townshipSpawnInfo.min + townshipSpawnInfo.max) / 2;
						}
						int l = rnd.RandomRange(num7, townshipSpawnInfo.max + 1);
						l = Utils.FastMax(1, l);
						if (num6 == 0)
						{
							num7 = Utils.FastMax(1, num7 - 5);
						}
						while (l >= townshipSpawnInfo.min)
						{
							int num8 = rnd.RandomRange(0, list2.Count);
							int num9 = (rnd.RandomFloat < 0.5f) ? (list2.Count - 1) : 1;
							for (int m = 0; m < list2.Count; m++)
							{
								Vector2i vector2i2 = list2[num8];
								township.GridCenter = vector2i2;
								num8 = (num8 + num9) % list2.Count;
								for (int n = 0; n < 12; n++)
								{
									this.getStreetLayout(vector2i2, l, rnd, flag, list3);
									if (list3.Count >= l && (townshipData.OutskirtDistrictPercent <= 0f || this.grow(townshipData, list3, list4)))
									{
										m = 999999;
										l = -1;
										break;
									}
								}
							}
							l--;
						}
						if (l >= 0)
						{
							if (num5 > 0)
							{
								num5--;
								i--;
								biomeType = this.nextBiomeType(biomeType, townshipData);
							}
							else
							{
								num5 = 80;
								biomeType = BiomeType.none;
							}
						}
						else
						{
							if (townshipData.OutskirtDistrictPercent > 0f)
							{
								string outskirtDistrict = WorldBuilderStatic.townshipDatas[TownshipStatic.NamesByType[townshipTypeId]].OutskirtDistrict;
								foreach (StreetTile streetTile in list4)
								{
									streetTile.Township = township;
									streetTile.District = DistrictPlannerStatic.Districts[outskirtDistrict];
									township.Streets[streetTile.GridPosition] = streetTile;
								}
							}
							foreach (StreetTile streetTile2 in list3)
							{
								streetTile2.Township = township;
								township.Streets[streetTile2.GridPosition] = streetTile2;
								streetTile2.District = null;
							}
							this.worldBuilder.DistrictPlanner.PlanTownship(township);
							foreach (StreetTile streetTile3 in township.Streets.Values)
							{
								if (streetTile3.District.type != District.Type.Gateway && !(streetTile3.District.name == "roadside"))
								{
									IEnumerable<Vector2i> source = this.dir4way;
									Func<Vector2i, int> keySelector;
									if ((keySelector = <>9__1) == null)
									{
										keySelector = (<>9__1 = ((Vector2i d2) => rnd.RandomRange(0, 100)));
									}
									foreach (Vector2i vector2i3 in source.OrderBy(keySelector).ToList<Vector2i>())
									{
										StreetTile neighbor = streetTile3.GetNeighbor(vector2i3);
										if (neighbor != null)
										{
											bool flag2 = true;
											if (neighbor.District != null && neighbor.District.type != District.Type.Gateway && neighbor.District != streetTile3.District && townshipData.OutskirtDistrictPercent >= 0.6f)
											{
												flag2 = (rnd.RandomFloat < 0.5f);
											}
											if (neighbor.Township != streetTile3.Township)
											{
												flag2 = false;
											}
											if (flag2)
											{
												streetTile3.SetExitUsed(streetTile3.getHighwayExitPositionByDirection(vector2i3));
											}
											else
											{
												streetTile3.SetExitUnUsed(streetTile3.getHighwayExitPositionByDirection(vector2i3));
											}
										}
									}
								}
							}
							township.CleanupStreets();
							Township township2 = township;
							num = townID;
							townID = num + 1;
							township2.ID = num;
							this.worldBuilder.Townships.Add(township);
							num6++;
							TownPlanner.BiomeStats biomeStats;
							if (!this.biomeStats.TryGetValue(biomeType, out biomeStats))
							{
								biomeStats = new TownPlanner.BiomeStats();
								this.biomeStats.Add(biomeType, biomeStats);
							}
							biomeStats.townshipCount++;
							int num10;
							if (!biomeStats.counts.TryGetValue(townshipTypeName, out num10))
							{
								biomeStats.counts.Add(townshipTypeName, 1);
							}
							else
							{
								biomeStats.counts[townshipTypeName] = num10 + 1;
							}
							biomeType = this.nextBiomeType(biomeType, townshipData);
							num5 = 80;
						}
					}
					townshipTypeName = null;
					townshipData = null;
				}
			}
			List<int>.Enumerator enumerator = default(List<int>.Enumerator);
			Log.Out("TownPlanner Plan {0} in {1}", new object[]
			{
				this.worldBuilder.Townships.Count,
				(float)ms.ElapsedMilliseconds * 0.001f
			});
			for (int num11 = 0; num11 < 5; num11++)
			{
				BiomeType biomeType3 = (BiomeType)num11;
				TownPlanner.BiomeStats biomeStats2;
				if (this.biomeStats.TryGetValue(biomeType3, out biomeStats2))
				{
					string text = "";
					foreach (KeyValuePair<string, int> keyValuePair in biomeStats2.counts)
					{
						text += string.Format(", {0} {1}", keyValuePair.Key, keyValuePair.Value);
					}
					Log.Out("TownPlanner {0} has {1} townships{2}", new object[]
					{
						biomeType3,
						biomeStats2.townshipCount,
						text
					});
				}
			}
			this.biomeStats.Clear();
			yield return this.worldBuilder.SetMessage(Localization.Get("xuiRwgTownPlanningFinished", false), false, false);
			yield break;
			yield break;
		}

		public IEnumerator SpawnPrefabs()
		{
			yield return null;
			MicroStopwatch ms = new MicroStopwatch(true);
			MicroStopwatch msReset = new MicroStopwatch(true);
			foreach (Township township in this.worldBuilder.Townships)
			{
				township.SpawnPrefabs();
				if (msReset.ElapsedMilliseconds > 500L)
				{
					yield return null;
					msReset.ResetAndRestart();
				}
			}
			List<Township>.Enumerator enumerator = default(List<Township>.Enumerator);
			Log.Out(string.Format("TownPlanner SpawnPrefabs in {0}, r={1:x}", (float)ms.ElapsedMilliseconds * 0.001f, Rand.Instance.PeekSample()));
			yield break;
			yield break;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public bool tooClose(Vector2i position, bool isRoadsideTownship, TownPlanner.TownshipSpawnInfo tsSpawnInfo)
		{
			int num = isRoadsideTownship ? 3 : tsSpawnInfo.distance;
			int num2 = 5;
			foreach (Township township in this.worldBuilder.Townships)
			{
				bool flag = township.Type == TownshipStatic.TypesByName["roadside"];
				foreach (Vector2i vector2i in township.Streets.Keys)
				{
					if (isRoadsideTownship && flag && Utils.FastAbs((float)(position.x - vector2i.x)) < (float)num2 && Utils.FastAbs((float)(position.y - vector2i.y)) < (float)num2)
					{
						return true;
					}
					if (Utils.FastAbs((float)(position.x - vector2i.x)) < (float)num && Utils.FastAbs((float)(position.y - vector2i.y)) < (float)num)
					{
						return true;
					}
				}
			}
			return false;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public bool grow(WorldBuilder.TownshipData _data, List<StreetTile> baseTiles, List<StreetTile> finalTiles)
		{
			finalTiles.Clear();
			Vector2i[] array = (_data.OutskirtDistrictPercent < 1f) ? this.dir4way : this.dir8way;
			foreach (StreetTile streetTile in baseTiles)
			{
				foreach (Vector2i other in array)
				{
					StreetTile streetTileGrid = this.worldBuilder.GetStreetTileGrid(streetTile.GridPosition + other);
					if (streetTileGrid != null && !baseTiles.Contains(streetTileGrid) && !finalTiles.Contains(streetTileGrid))
					{
						if (!streetTileGrid.IsValidForStreetTile)
						{
							return false;
						}
						StreetTile[] neighbors = streetTileGrid.GetNeighbors();
						for (int j = 0; j < neighbors.Length; j++)
						{
							if (neighbors[j].Township != null)
							{
								return false;
							}
						}
						finalTiles.Add(streetTileGrid);
					}
				}
			}
			if (_data.OutskirtDistrictPercent < 1f)
			{
				float num = 1f - _data.OutskirtDistrictPercent;
				float num2 = 0f;
				for (int k = finalTiles.Count - 1; k >= 0; k--)
				{
					num2 += num;
					if (num2 >= 1f)
					{
						num2 -= 1f;
						finalTiles.RemoveAt(k);
					}
				}
			}
			return true;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public int getTownshipCounts(DynamicProperties _properties, Dictionary<int, TownPlanner.TownshipSpawnInfo> townshipSpawnInfo)
		{
			int num = 0;
			foreach (string text in TownshipStatic.TypesByName.Keys)
			{
				if (!(text == "wilderness") && !(text == "highway"))
				{
					string text2 = text.ToLower();
					float num2 = 1f;
					float num3 = 1f;
					_properties.ParseVec(string.Format("{0}.tiles", text2), ref num2, ref num3);
					int count = this.worldBuilder.GetCount(text2, this.worldBuilder.Towns);
					int distance = 5;
					_properties.ParseInt(string.Format("{0}.distance", text2), ref distance);
					if (count >= 1 && num2 >= 1f && num3 >= 1f)
					{
						townshipSpawnInfo.Add(TownshipStatic.TypesByName[text], new TownPlanner.TownshipSpawnInfo((int)num2, (int)num3, count, distance));
					}
					num += count;
				}
			}
			return num;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public BiomeType nextBiomeType(BiomeType _current, WorldBuilder.TownshipData _townshipData)
		{
			int num = (int)_current;
			for (int i = 0; i < 4; i++)
			{
				num = (num + 1) % 5;
				if (_townshipData.Biomes.IsEmpty || _townshipData.Biomes.Test_Bit(this.worldBuilder.biomeTagBits[num]))
				{
					return (BiomeType)num;
				}
			}
			return BiomeType.none;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public BiomeType getBiomeWithMostAvailableSpace(WorldBuilder.TownshipData _townshipData)
		{
			int num = 0;
			int num2 = 255;
			List<Vector2i> list = new List<Vector2i>();
			for (int i = 0; i < 5; i++)
			{
				if (_townshipData.Biomes.IsEmpty || _townshipData.Biomes.Test_Bit(this.worldBuilder.biomeTagBits[i]))
				{
					this.getStreetTilesForBiome((BiomeType)i, list);
					if (list.Count > num)
					{
						num = list.Count;
						num2 = i;
					}
				}
			}
			return (BiomeType)num2;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public Dictionary<BiomeType, List<Vector2i>> getStreetTilesByBiome()
		{
			Dictionary<BiomeType, List<Vector2i>> dictionary = new Dictionary<BiomeType, List<Vector2i>>();
			for (int i = 0; i < 5; i++)
			{
				List<Vector2i> list = new List<Vector2i>();
				this.getStreetTilesForBiome((BiomeType)i, list);
				if (list.Count > 0)
				{
					dictionary.Add((BiomeType)i, list);
				}
			}
			return dictionary;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void getStreetTilesForBiome(BiomeType _biomeType, List<Vector2i> _biomeStreetTiles)
		{
			_biomeStreetTiles.Clear();
			StreetTile[,] streetTileMap = this.worldBuilder.StreetTileMap;
			int upperBound = streetTileMap.GetUpperBound(0);
			int upperBound2 = streetTileMap.GetUpperBound(1);
			for (int i = streetTileMap.GetLowerBound(0); i <= upperBound; i++)
			{
				for (int j = streetTileMap.GetLowerBound(1); j <= upperBound2; j++)
				{
					StreetTile streetTile = streetTileMap[i, j];
					if (streetTile.IsValidForStreetTile && streetTile.Township == null && !streetTile.Used && streetTile.BiomeType == _biomeType)
					{
						_biomeStreetTiles.Add(streetTile.GridPosition);
					}
				}
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public List<TileGroup> getStreetTileGroups()
		{
			List<TileGroup> list = new List<TileGroup>();
			Dictionary<Vector2i, int> dictionary = new Dictionary<Vector2i, int>();
			StreetTile[,] streetTileMap = this.worldBuilder.StreetTileMap;
			int upperBound = streetTileMap.GetUpperBound(0);
			int upperBound2 = streetTileMap.GetUpperBound(1);
			for (int i = streetTileMap.GetLowerBound(0); i <= upperBound; i++)
			{
				for (int j = streetTileMap.GetLowerBound(1); j <= upperBound2; j++)
				{
					StreetTile streetTile = streetTileMap[i, j];
					int num = list.Count;
					if (streetTile.IsValidForStreetTile)
					{
						for (int k = 0; k < this.dir4way.Length; k++)
						{
							if (streetTile.GridPosition.x + this.dir4way[k].x >= 0 && streetTile.GridPosition.x + this.dir4way[k].x < this.worldBuilder.StreetTileMap.GetLength(0) && streetTile.GridPosition.y + this.dir4way[k].y >= 0 && streetTile.GridPosition.y + this.dir4way[k].y < this.worldBuilder.StreetTileMap.GetLength(1))
							{
								Vector2i vector2i = streetTile.GridPosition + this.dir4way[k];
								int num2;
								if (this.worldBuilder.StreetTileMap[vector2i.x, vector2i.y].BiomeType == streetTile.BiomeType && this.worldBuilder.StreetTileMap[vector2i.x, vector2i.y].IsValidForStreetTile && dictionary.TryGetValue(vector2i, out num2))
								{
									num = num2;
								}
							}
						}
						dictionary[streetTile.GridPosition] = num;
						if (num == list.Count)
						{
							list.Add(new TileGroup
							{
								Biome = streetTile.BiomeType
							});
						}
						list[num].Positions.Add(streetTile.GridPosition);
					}
				}
			}
			return list;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void getStreetLayout(Vector2i startPosition, int townSize, GameRandom rnd, bool isRoadside, List<StreetTile> townTiles)
		{
			townTiles.Clear();
			StreetTile streetTileGrid = this.worldBuilder.GetStreetTileGrid(startPosition);
			if (!isRoadside)
			{
				using (List<Township>.Enumerator enumerator = this.worldBuilder.Townships.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						Township township = enumerator.Current;
						if (streetTileGrid.Area.Overlaps(township.BufferArea) || township.BufferArea.Contains(streetTileGrid.Area.center) || township.BufferArea.Overlaps(streetTileGrid.Area))
						{
							return;
						}
					}
					goto IL_166;
				}
			}
			if (streetTileGrid.Township != null || streetTileGrid.District != null)
			{
				return;
			}
			foreach (Township township2 in this.worldBuilder.Townships)
			{
				if (streetTileGrid.Area.Overlaps(township2.BufferArea) || township2.BufferArea.Contains(streetTileGrid.Area.center) || township2.BufferArea.Overlaps(streetTileGrid.Area))
				{
					return;
				}
			}
			foreach (StreetTile streetTile in streetTileGrid.GetNeighbors8way())
			{
				if (streetTile.Township != null || streetTile.District != null)
				{
					return;
				}
			}
			IL_166:
			if (townSize == 1)
			{
				townTiles.Add(streetTileGrid);
				return;
			}
			List<StreetTile> list = new List<StreetTile>();
			if (townSize <= 16)
			{
				townTiles.Add(streetTileGrid);
				list.Add(streetTileGrid);
			}
			else
			{
				int num = (int)Mathf.Sqrt((float)townSize) - 1;
				int num2 = num / -2;
				int num3 = num / 2 - 1;
				for (int j = num2; j <= num3; j++)
				{
					Vector2i pos;
					pos.y = startPosition.y + j;
					bool flag = j == num2 || j == num3;
					for (int k = num2; k <= num3; k++)
					{
						pos.x = startPosition.x + k;
						StreetTile streetTile2 = this.StreetLayoutCheckPos(pos);
						if (streetTile2 == null)
						{
							townTiles.Clear();
							return;
						}
						townTiles.Add(streetTile2);
						if (flag || k == num2 || k == num3)
						{
							list.Add(streetTile2);
						}
					}
				}
				if (townTiles.Count >= townSize)
				{
					return;
				}
			}
			while (list.Count > 0)
			{
				int index = rnd.RandomRange(0, list.Count);
				StreetTile streetTile3 = list[index];
				list.RemoveAt(index);
				int num4 = rnd.RandomRange(4);
				for (int l = 0; l < 4; l++)
				{
					Vector2i pos2 = streetTile3.GridPosition + this.dir4way[l + num4 & 3];
					StreetTile streetTile4 = this.StreetLayoutCheckPos(pos2);
					if (streetTile4 != null && !townTiles.Contains(streetTile4))
					{
						townTiles.Add(streetTile4);
						if (townTiles.Count >= townSize)
						{
							return;
						}
						list.Add(streetTile4);
					}
				}
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public StreetTile StreetLayoutCheckPos(Vector2i _pos)
		{
			StreetTile streetTileGrid = this.worldBuilder.GetStreetTileGrid(_pos);
			if (streetTileGrid != null && streetTileGrid.IsValidForStreetTile)
			{
				foreach (Township township in this.worldBuilder.Townships)
				{
					if (streetTileGrid.Area.Overlaps(township.BufferArea) || township.BufferArea.Contains(streetTileGrid.Area.center) || township.BufferArea.Overlaps(streetTileGrid.Area))
					{
						return null;
					}
				}
				return streetTileGrid;
			}
			return null;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public const int cTriesPerTownshipSpawnInfo = 80;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly WorldBuilder worldBuilder;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly Dictionary<BiomeType, TownPlanner.BiomeStats> biomeStats = new Dictionary<BiomeType, TownPlanner.BiomeStats>();

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly Vector2i[] dir4way = new Vector2i[]
		{
			Vector2i.up,
			Vector2i.right,
			Vector2i.down,
			Vector2i.left
		};

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly Vector2i[] dir8way = new Vector2i[]
		{
			Vector2i.up,
			Vector2i.up + Vector2i.right,
			Vector2i.right,
			Vector2i.right + Vector2i.down,
			Vector2i.down,
			Vector2i.down + Vector2i.left,
			Vector2i.left,
			Vector2i.left + Vector2i.up
		};

		[PublicizedFrom(EAccessModifier.Private)]
		public class BiomeStats
		{
			public int townshipCount;

			public Dictionary<string, int> counts = new Dictionary<string, int>();
		}

		public class TownshipSpawnInfo
		{
			public TownshipSpawnInfo(int _min, int _max, int _count, int _distance)
			{
				this.min = _min;
				this.max = _max;
				this.count = _count;
				this.distance = _distance;
			}

			public int min;

			public int max;

			public int count;

			public int distance;
		}
	}
}
