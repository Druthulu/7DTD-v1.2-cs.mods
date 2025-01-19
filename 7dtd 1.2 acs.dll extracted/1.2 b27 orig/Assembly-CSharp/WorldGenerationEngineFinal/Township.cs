using System;
using System.Collections.Generic;
using UnityEngine;

namespace WorldGenerationEngineFinal
{
	public class Township
	{
		public int Type
		{
			get
			{
				return this.type;
			}
			set
			{
				this.type = value;
				this.typeName = TownshipStatic.NamesByType[this.type];
			}
		}

		public Township(WorldBuilder _worldBuilder)
		{
			this.worldBuilder = _worldBuilder;
		}

		public void Cleanup()
		{
			GameRandomManager.Instance.FreeGameRandom(this.rand);
		}

		public string GetTypeName()
		{
			if (this.typeName == null)
			{
				this.typeName = TownshipStatic.NamesByType[this.Type];
			}
			return this.typeName;
		}

		public bool IsBig()
		{
			return this.type == TownshipStatic.TypesByName["citybig"];
		}

		public void SortGatewaysClockwise()
		{
			this.Gateways.Sort(delegate(StreetTile _t1, StreetTile _t2)
			{
				float num = Mathf.Atan2((float)(_t1.GridPosition.y - this.GridCenter.y), (float)(_t1.GridPosition.x - this.GridCenter.x));
				float value = Mathf.Atan2((float)(_t2.GridPosition.y - this.GridCenter.y), (float)(_t2.GridPosition.x - this.GridCenter.x));
				return num.CompareTo(value);
			});
		}

		public void CleanupStreets()
		{
			if (this.Streets == null || this.Streets.Count == 0)
			{
				Log.Error("No Streets!");
				return;
			}
			this.rand = GameRandomManager.Instance.CreateGameRandom(this.worldBuilder.Seed + this.ID + this.Streets.Count);
			foreach (StreetTile streetTile in this.Streets.Values)
			{
				if (streetTile.District != null && streetTile.District.type != District.Type.Gateway)
				{
					int num = 0;
					if (this.commercialCap == null && streetTile.District.type == District.Type.Commercial)
					{
						for (int i = 0; i < this.worldBuilder.TownshipShared.dir4way.Length; i++)
						{
							StreetTile neighbor = streetTile.GetNeighbor(this.worldBuilder.TownshipShared.dir4way[i]);
							if (neighbor != null && neighbor.District == streetTile.District)
							{
								num++;
							}
							if (neighbor != null && neighbor == this.ruralCap)
							{
								num += 2;
							}
						}
						if (num == 1)
						{
							for (int j = 0; j < this.worldBuilder.TownshipShared.dir4way.Length; j++)
							{
								StreetTile neighbor2 = streetTile.GetNeighbor(this.worldBuilder.TownshipShared.dir4way[j]);
								if (neighbor2 != null && neighbor2.District == streetTile.District)
								{
									streetTile.SetExitUsed(streetTile.getHighwayExitPosition(j));
								}
								else
								{
									streetTile.SetExitUnUsed(streetTile.getHighwayExitPosition(j));
								}
							}
							this.commercialCap = streetTile;
						}
					}
					else if (this.ruralCap == null && streetTile.District.type == District.Type.Rural)
					{
						for (int k = 0; k < this.worldBuilder.TownshipShared.dir4way.Length; k++)
						{
							StreetTile neighbor3 = streetTile.GetNeighbor(this.worldBuilder.TownshipShared.dir4way[k]);
							if (neighbor3 != null && neighbor3.District == streetTile.District)
							{
								num++;
							}
							if (neighbor3 != null && neighbor3 == this.commercialCap)
							{
								num += 2;
							}
						}
						if (num >= 1 && num <= 2)
						{
							bool flag = false;
							for (int l = 0; l < this.worldBuilder.TownshipShared.dir4way.Length; l++)
							{
								StreetTile neighbor4 = streetTile.GetNeighbor(this.worldBuilder.TownshipShared.dir4way[l]);
								if (!flag && neighbor4 != null && neighbor4.District == streetTile.District)
								{
									streetTile.SetExitUsed(streetTile.getHighwayExitPosition(l));
									flag = true;
								}
								else
								{
									streetTile.SetExitUnUsed(streetTile.getHighwayExitPosition(l));
								}
							}
							this.ruralCap = streetTile;
						}
					}
					else
					{
						for (int m = 0; m < this.worldBuilder.TownshipShared.dir4way.Length; m++)
						{
							StreetTile neighbor5 = streetTile.GetNeighbor(this.worldBuilder.TownshipShared.dir4way[m]);
							if (neighbor5 != null && neighbor5.District != null && neighbor5.District.type == District.Type.Gateway)
							{
								num++;
							}
						}
						if (num >= 1)
						{
							for (int n = 0; n < this.worldBuilder.TownshipShared.dir4way.Length; n++)
							{
								StreetTile neighbor6 = streetTile.GetNeighbor(this.worldBuilder.TownshipShared.dir4way[n]);
								if (neighbor6 != null && (neighbor6.District == streetTile.District || neighbor6.District == DistrictPlannerStatic.Districts["gateway"]))
								{
									streetTile.SetExitUsed(streetTile.getHighwayExitPosition(n));
								}
							}
						}
					}
				}
			}
			this.cleanupLessThan();
			this.cleanupGreaterThan();
			this.cleanupNotEqual();
			this.cleanupLessThan();
			this.cleanupGreaterThan();
			this.cleanupNotEqual();
			int num2 = int.MaxValue;
			int num3 = int.MaxValue;
			int num4 = int.MinValue;
			int num5 = int.MinValue;
			foreach (StreetTile streetTile2 in this.Streets.Values)
			{
				num2 = Utils.FastMin(num2, streetTile2.WorldPosition.x);
				num3 = Utils.FastMin(num3, streetTile2.WorldPosition.y);
				num4 = Utils.FastMax(num4, streetTile2.WorldPositionMax.x);
				num5 = Utils.FastMax(num5, streetTile2.WorldPositionMax.y);
			}
			this.Area = new Rect((float)num2, (float)num3, (float)(num4 - num2), (float)(num5 - num3));
			this.BufferArea = new Rect(this.Area.xMin - 150f, this.Area.yMin - 150f, this.Area.width + 300f, this.Area.height + 300f);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void cleanupLessThan()
		{
			foreach (StreetTile streetTile in this.Streets.Values)
			{
				int roadExitCount = streetTile.RoadExitCount;
				int neighborExitCount = this.GetNeighborExitCount(streetTile);
				if (!(streetTile.District.name == "gateway") && streetTile != this.ruralCap && streetTile != this.commercialCap && roadExitCount < neighborExitCount)
				{
					for (int i = 0; i < this.worldBuilder.StreetTileShared.RoadShapeExitCounts.Count; i++)
					{
						if (this.worldBuilder.StreetTileShared.RoadShapeExitCounts[i] == neighborExitCount)
						{
							for (int j = 0; j < this.worldBuilder.TownshipShared.dir4way.Length; j++)
							{
								StreetTile neighbor = streetTile.GetNeighbor(this.worldBuilder.TownshipShared.dir4way[j]);
								if (neighbor.Township != streetTile.Township || !neighbor.HasExitTo(streetTile))
								{
									streetTile.SetExitUnUsed(streetTile.getHighwayExitPosition(j));
								}
								else
								{
									streetTile.SetExitUsed(streetTile.getHighwayExitPosition(j));
								}
							}
						}
					}
				}
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void cleanupGreaterThan()
		{
			foreach (StreetTile streetTile in this.Streets.Values)
			{
				int roadExitCount = streetTile.RoadExitCount;
				int neighborExitCount = this.GetNeighborExitCount(streetTile);
				if (!(streetTile.District.name == "gateway") && streetTile != this.ruralCap && streetTile != this.commercialCap && roadExitCount > neighborExitCount)
				{
					for (int i = 0; i < this.worldBuilder.StreetTileShared.RoadShapeExitCounts.Count; i++)
					{
						if (this.worldBuilder.StreetTileShared.RoadShapeExitCounts[i] == neighborExitCount)
						{
							for (int j = 0; j < this.worldBuilder.TownshipShared.dir4way.Length; j++)
							{
								StreetTile neighbor = streetTile.GetNeighbor(this.worldBuilder.TownshipShared.dir4way[j]);
								if (neighbor.Township != streetTile.Township || !neighbor.HasExitTo(streetTile))
								{
									streetTile.SetExitUnUsed(streetTile.getHighwayExitPosition(j));
								}
								else
								{
									streetTile.SetExitUsed(streetTile.getHighwayExitPosition(j));
								}
							}
						}
					}
				}
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void cleanupNotEqual()
		{
			foreach (StreetTile streetTile in this.Streets.Values)
			{
				int roadExitCount = streetTile.RoadExitCount;
				int neighborExitCount = this.GetNeighborExitCount(streetTile);
				if (!(streetTile.District.name == "gateway") && streetTile != this.ruralCap && streetTile != this.commercialCap && roadExitCount != neighborExitCount)
				{
					for (int i = 0; i < this.worldBuilder.StreetTileShared.RoadShapeExitCounts.Count; i++)
					{
						if (this.worldBuilder.StreetTileShared.RoadShapeExitCounts[i] == neighborExitCount)
						{
							for (int j = 0; j < this.worldBuilder.TownshipShared.dir4way.Length; j++)
							{
								StreetTile neighbor = streetTile.GetNeighbor(this.worldBuilder.TownshipShared.dir4way[j]);
								if (neighbor.Township != streetTile.Township || !neighbor.HasExitTo(streetTile))
								{
									streetTile.SetExitUnUsed(streetTile.getHighwayExitPosition(j));
								}
								else
								{
									streetTile.SetExitUsed(streetTile.getHighwayExitPosition(j));
								}
							}
						}
					}
				}
			}
		}

		public void SpawnPrefabs()
		{
			foreach (StreetTile streetTile in this.Streets.Values)
			{
				if (streetTile == null)
				{
					Log.Error("WorldTileData is null, this shouldn't happen!");
				}
				else
				{
					streetTile.SpawnPrefabs();
				}
			}
			this.Prefabs.Clear();
		}

		public void AddToUsedPOIList(string name)
		{
			this.worldBuilder.PrefabManager.AddUsedPrefab(name);
		}

		public List<Vector2i> GetUnusedTownExits(int _gatewayUnusedMax = 4)
		{
			this.list.Clear();
			if (WorldBuilderStatic.townshipDatas[this.GetTypeName()].SpawnGateway)
			{
				using (List<StreetTile>.Enumerator enumerator = this.Gateways.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						StreetTile streetTile = enumerator.Current;
						if (streetTile.UsedExitList.Count <= _gatewayUnusedMax)
						{
							foreach (Vector2i item in streetTile.GetHighwayExits(true))
							{
								this.list.Add(item);
							}
						}
					}
					goto IL_10C;
				}
			}
			foreach (StreetTile streetTile2 in this.Streets.Values)
			{
				foreach (Vector2i item2 in streetTile2.GetHighwayExits(false))
				{
					this.list.Add(item2);
				}
			}
			IL_10C:
			return this.list;
		}

		public void AddPrefab(PrefabDataInstance pdi)
		{
			this.Prefabs.Add(pdi);
			this.worldBuilder.PrefabManager.AddUsedPrefabWorld(this.ID, pdi);
		}

		public List<Vector2i> GetTownExits()
		{
			this.list.Clear();
			foreach (StreetTile streetTile in this.Gateways)
			{
				foreach (Vector2i item in streetTile.GetHighwayExits(true))
				{
					this.list.Add(item);
				}
			}
			return this.list;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public int GetNeighborExitCount(StreetTile current)
		{
			int num = 0;
			int num2 = -1;
			foreach (StreetTile streetTile in current.GetNeighbors())
			{
				num2++;
				if (streetTile != null && streetTile.District != null && streetTile.Township != null)
				{
					bool flag = current.District.name == "highway";
					bool flag2 = current.District.name == "gateway";
					bool flag3 = streetTile.District.name == "highway";
					bool flag4 = streetTile.District.name == "gateway";
					if ((streetTile.Township == current.Township || ((flag || flag4 || flag2 || flag3) && (!flag2 || flag3) && (!flag || flag4))) && (streetTile.RoadExits[num2 + 2 & 3] || (flag && flag3)))
					{
						num++;
					}
				}
			}
			return num;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public bool[] GetNeighborExits(StreetTile current)
		{
			bool[] array = new bool[4];
			int num = -1;
			foreach (StreetTile streetTile in current.GetNeighbors())
			{
				num++;
				if (streetTile != null && streetTile.District != null && streetTile.Township != null)
				{
					bool flag = current.District.name == "highway";
					bool flag2 = current.District.name == "gateway";
					bool flag3 = streetTile.District.name == "highway";
					bool flag4 = streetTile.District.name == "gateway";
					if ((streetTile.Township == current.Township || ((flag || flag4 || flag2 || flag3) && (!flag2 || flag3) && (!flag || flag4))) && (streetTile.HasExitTo(current) || (flag && flag3)))
					{
						array[num] = true;
					}
				}
			}
			return array;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public int GetNeighborCount(Vector2i current)
		{
			int num = 0;
			for (int i = 0; i < this.worldBuilder.TownshipShared.dir4way.Length; i++)
			{
				Vector2i key = current + this.worldBuilder.TownshipShared.dir4way[i];
				if (this.Streets.ContainsKey(key))
				{
					num++;
				}
			}
			return num;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public int GetCurrentExitCount(Vector2i current)
		{
			int num = 0;
			for (int i = 0; i < this.Streets[current].RoadExits.Length; i++)
			{
				if (this.Streets[current].RoadExits[i])
				{
					num++;
				}
			}
			return num;
		}

		public override string ToString()
		{
			return string.Format("Township {0} {1}", this.ID, TownshipStatic.NamesByType[this.Type]);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static bool HasExitWhenRotated(int hasThisDirExit, StreetTile.PrefabRotations _rots, bool[] _exits)
		{
			bool[] array = new bool[4];
			switch (_rots)
			{
			case StreetTile.PrefabRotations.None:
				array[0] = _exits[0];
				array[1] = _exits[1];
				array[2] = _exits[2];
				array[3] = _exits[3];
				break;
			case StreetTile.PrefabRotations.One:
				array[0] = _exits[3];
				array[1] = _exits[0];
				array[2] = _exits[1];
				array[3] = _exits[2];
				break;
			case StreetTile.PrefabRotations.Two:
				array[0] = _exits[2];
				array[1] = _exits[3];
				array[2] = _exits[0];
				array[3] = _exits[1];
				break;
			case StreetTile.PrefabRotations.Three:
				array[0] = _exits[1];
				array[1] = _exits[2];
				array[2] = _exits[3];
				array[3] = _exits[0];
				break;
			}
			return array[hasThisDirExit];
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public const int BUFFER_DISTANCE = 300;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly WorldBuilder worldBuilder;

		public int ID;

		public BiomeType BiomeType;

		public Rect Area;

		public Rect BufferArea;

		[PublicizedFrom(EAccessModifier.Private)]
		public StreetTile commercialCap;

		[PublicizedFrom(EAccessModifier.Private)]
		public StreetTile ruralCap;

		[PublicizedFrom(EAccessModifier.Private)]
		public int type;

		public Vector2i GridCenter;

		public Dictionary<Vector2i, StreetTile> Streets = new Dictionary<Vector2i, StreetTile>();

		public List<PrefabDataInstance> Prefabs = new List<PrefabDataInstance>();

		public List<StreetTile> Gateways = new List<StreetTile>();

		public Dictionary<Township, int> TownshipConnectionCounts = new Dictionary<Township, int>();

		public GameRandom rand;

		[PublicizedFrom(EAccessModifier.Private)]
		public string typeName;

		[PublicizedFrom(EAccessModifier.Private)]
		public List<Vector2i> list = new List<Vector2i>();
	}
}
