using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WorldGenerationEngineFinal
{
	public class HighwayPlanner
	{
		public HighwayPlanner(WorldBuilder _worldBuilder)
		{
			this.worldBuilder = _worldBuilder;
		}

		public IEnumerator Plan(DynamicProperties thisWorldProperties, int worldSeed)
		{
			yield return this.worldBuilder.SetMessage(Localization.Get("xuiRwgHighways", false), false, false);
			MicroStopwatch ms = new MicroStopwatch(true);
			this.ExitConnections.Clear();
			foreach (Township township2 in this.worldBuilder.Townships)
			{
				foreach (StreetTile streetTile in township2.Gateways)
				{
					streetTile.SetAllExistingNeighborsForGateway();
				}
			}
			List<Township> highwayTownships = this.worldBuilder.Townships.FindAll((Township _township) => WorldBuilderStatic.townshipDatas[_township.GetTypeName()].SpawnGateway);
			if (highwayTownships.Count > 0)
			{
				highwayTownships.Sort((Township _t1, Township _t2) => _t2.Streets.Count.CompareTo(_t1.Streets.Count));
				int num;
				for (int i = 0; i < highwayTownships.Count; i = num)
				{
					Township township = highwayTownships[i];
					yield return this.ConnectClosest(township, highwayTownships);
					if (township.IsBig())
					{
						yield return this.ConnectSelf(township);
					}
					township = null;
					num = i + 1;
				}
				Log.Out(string.Format("HighwayPlanner Plan townships in {0}", (float)ms.ElapsedMilliseconds * 0.001f));
				yield return this.cleanupHighwayConnections(highwayTownships);
			}
			yield return this.runTownshipDirtRoads();
			Log.Out(string.Format("HighwayPlanner Plan in {0}, r={1:x}", (float)ms.ElapsedMilliseconds * 0.001f, Rand.Instance.PeekSample()));
			yield break;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public IEnumerator cleanupHighwayConnections(List<Township> highwayTownships)
		{
			MicroStopwatch ms = new MicroStopwatch(true);
			List<Vector2i> tilesToRemove = new List<Vector2i>();
			List<Path> pathsToRemove = new List<Path>();
			foreach (Township township in highwayTownships)
			{
				if (township.Gateways.Count == 0)
				{
					Log.Error("cleanupHighwayConnections township {0} in {1} has no gateways!", new object[]
					{
						township.GetTypeName(),
						township.BiomeType
					});
				}
				else
				{
					foreach (StreetTile streetTile in township.Gateways)
					{
						for (int i = 0; i < 4; i++)
						{
							StreetTile neighborByIndex = streetTile.GetNeighborByIndex(i);
							Vector2i highwayExitPosition = streetTile.getHighwayExitPosition(i);
							if (!streetTile.UsedExitList.Contains(highwayExitPosition) && (neighborByIndex.Township != streetTile.Township || !neighborByIndex.HasExitTo(streetTile)))
							{
								streetTile.SetExitUnUsed(highwayExitPosition);
							}
						}
					}
				}
			}
			foreach (HighwayPlanner.ExitConnection exitConnection in this.ExitConnections)
			{
				exitConnection.SetExitUsedManually();
			}
			foreach (Township t in highwayTownships)
			{
				yield return this.worldBuilder.SetMessage(Localization.Get("xuiRwgHighwaysConnections", false), false, false);
				if (WorldBuilderStatic.townshipDatas[t.GetTypeName()].SpawnGateway && t.Gateways.Count != 0)
				{
					foreach (StreetTile streetTile2 in t.Gateways)
					{
						for (int j = 0; j < 4; j++)
						{
							streetTile2.GetNeighborByIndex(j);
							Vector2i highwayExitPosition2 = streetTile2.getHighwayExitPosition(j);
							if (!streetTile2.UsedExitList.Contains(highwayExitPosition2))
							{
								streetTile2.SetExitUnUsed(highwayExitPosition2);
							}
						}
						if (streetTile2.UsedExitList.Count < 2)
						{
							for (int k = 0; k < 4; k++)
							{
								StreetTile neighborByIndex2 = streetTile2.GetNeighborByIndex(k);
								streetTile2.SetExitUnUsed(streetTile2.getHighwayExitPosition(k));
								if (neighborByIndex2.Township == streetTile2.Township)
								{
									neighborByIndex2.SetExitUnUsed(neighborByIndex2.getHighwayExitPosition(neighborByIndex2.GetNeighborIndex(streetTile2)));
								}
							}
							foreach (Path path in streetTile2.ConnectedHighways)
							{
								StreetTile streetTileWorld;
								if (this.worldBuilder.GetStreetTileWorld(path.StartPosition) == streetTile2)
								{
									streetTileWorld = this.worldBuilder.GetStreetTileWorld(path.EndPosition);
									streetTileWorld.SetExitUnUsed(path.EndPosition);
								}
								else
								{
									streetTileWorld = this.worldBuilder.GetStreetTileWorld(path.StartPosition);
									streetTileWorld.SetExitUnUsed(path.StartPosition);
								}
								if (streetTileWorld.UsedExitList.Count < 2)
								{
									tilesToRemove.Add(streetTileWorld.GridPosition);
								}
								pathsToRemove.Add(path);
							}
							tilesToRemove.Add(streetTile2.GridPosition);
						}
					}
					foreach (Vector2i vector2i in tilesToRemove)
					{
						StreetTile streetTileGrid = this.worldBuilder.GetStreetTileGrid(vector2i);
						if (streetTileGrid.Township != null)
						{
							streetTileGrid.Township.Gateways.Remove(streetTileGrid);
							streetTileGrid.Township.Streets.Remove(vector2i);
						}
						streetTileGrid.StreetTilePrefabDatas.Clear();
						streetTileGrid.District = null;
						streetTileGrid.Township = null;
					}
					tilesToRemove.Clear();
					foreach (Path path2 in pathsToRemove)
					{
						path2.Dispose();
						this.worldBuilder.paths.Remove(path2);
					}
					pathsToRemove.Clear();
					t = null;
				}
			}
			List<Township>.Enumerator enumerator4 = default(List<Township>.Enumerator);
			Log.Out(string.Format("HighwayPlanner cleanupHighwayConnections in {0}, r={1:x}", (float)ms.ElapsedMilliseconds * 0.001f, Rand.Instance.PeekSample()));
			yield break;
			yield break;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public IEnumerator runTownshipDirtRoads()
		{
			MicroStopwatch ms = new MicroStopwatch(true);
			List<Township> countryTownships = this.worldBuilder.Townships.FindAll((Township _township) => !WorldBuilderStatic.townshipDatas[_township.GetTypeName()].SpawnGateway);
			int num3;
			for (int i = 0; i < countryTownships.Count; i = num3 + 1)
			{
				Township t = countryTownships[i];
				string msg = string.Format(Localization.Get("xuiRwgHighwaysTownship", false), i + 1, countryTownships.Count);
				yield return this.worldBuilder.SetMessage(msg, false, false);
				MicroStopwatch ms2 = new MicroStopwatch(true);
				using (Dictionary<Vector2i, StreetTile>.ValueCollection.Enumerator enumerator = t.Streets.Values.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						StreetTile streetTile = enumerator.Current;
						if (streetTile.GetNumTownshipNeighbors() == 1)
						{
							int num = -1;
							for (int j = 0; j < 4; j++)
							{
								StreetTile neighborByIndex = streetTile.GetNeighborByIndex(j);
								if (neighborByIndex != null && neighborByIndex.Township == streetTile.Township)
								{
									num = j;
									break;
								}
							}
							switch (num)
							{
							case 0:
								streetTile.SetRoadExit(2, true);
								goto IL_1BA;
							case 1:
								streetTile.SetRoadExit(3, true);
								goto IL_1BA;
							case 2:
								streetTile.SetRoadExit(0, true);
								goto IL_1BA;
							case 3:
								streetTile.SetRoadExit(1, true);
								goto IL_1BA;
							default:
								goto IL_1BA;
							}
						}
					}
					IL_1BA:;
				}
				ms2.ResetAndRestart();
				int count = 0;
				foreach (Vector2i exit in t.GetUnusedTownExits(4))
				{
					Vector2i closestPoint = Vector2i.zero;
					float closestDist = float.MaxValue;
					foreach (Path path in this.worldBuilder.paths)
					{
						if (!path.isCountryRoad)
						{
							foreach (Vector2 vector in path.FinalPathPoints)
							{
								Vector2i vector2i;
								vector2i.x = Utils.Fastfloor(vector.x);
								vector2i.y = Utils.Fastfloor(vector.y);
								float num2 = Vector2i.DistanceSqr(exit, vector2i);
								if (num2 < closestDist)
								{
									if (this.worldBuilder.PathingUtils.HasValidPath(exit, vector2i, true))
									{
										closestDist = num2;
										closestPoint = vector2i;
									}
									num3 = count;
									count = num3 + 1;
									if (this.worldBuilder.IsMessageElapsed())
									{
										yield return this.worldBuilder.SetMessage(msg + " " + string.Format(Localization.Get("xuiRwgHighwaysTownExits", false), count), false, false);
									}
								}
							}
							List<Vector2>.Enumerator enumerator4 = default(List<Vector2>.Enumerator);
						}
					}
					List<Path>.Enumerator enumerator3 = default(List<Path>.Enumerator);
					if (this.worldBuilder.IsMessageElapsed())
					{
						yield return this.worldBuilder.SetMessage(msg, false, false);
					}
					Path path2 = new Path(this.worldBuilder, exit, closestPoint, 2, true, false);
					if (path2.IsValid)
					{
						foreach (StreetTile streetTile2 in t.Streets.Values)
						{
							for (int k = 0; k < 4; k++)
							{
								if (Vector2i.Distance(streetTile2.getHighwayExitPosition(k), exit) < 10f)
								{
									streetTile2.SetExitUsed(exit);
								}
							}
						}
						this.worldBuilder.paths.Add(path2);
					}
				}
				List<Vector2i>.Enumerator enumerator2 = default(List<Vector2i>.Enumerator);
				Log.Out(string.Format("HighwayPlanner runTownshipDirtRoads #{0} unused exits c{1} in {2}, r={3:x}", new object[]
				{
					i,
					count,
					(float)ms2.ElapsedMilliseconds * 0.001f,
					Rand.Instance.PeekSample()
				}));
				t = null;
				msg = null;
				ms2 = null;
				num3 = i;
			}
			Log.Out(string.Format("HighwayPlanner runTownshipDirtRoads, countryTownships {0}, in {1}, r={2:x}", countryTownships.Count, (float)ms.ElapsedMilliseconds * 0.001f, Rand.Instance.PeekSample()));
			yield break;
			yield break;
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
		public IEnumerator ConnectClosest(Township _township, List<Township> highwayTownships)
		{
			Predicate<Township> <>9__0;
			int num2;
			for (int i = 0; i < _township.Gateways.Count; i = num2 + 1)
			{
				HighwayPlanner.<>c__DisplayClass9_1 CS$<>8__locals2 = new HighwayPlanner.<>c__DisplayClass9_1();
				CS$<>8__locals2.gateway = _township.Gateways[i];
				if (CS$<>8__locals2.gateway.UsedExitList.Count < 2)
				{
					Predicate<Township> match;
					if ((match = <>9__0) == null)
					{
						match = (<>9__0 = delegate(Township t)
						{
							int num4;
							return (!_township.TownshipConnectionCounts.TryGetValue(t, out num4) || num4 <= 1) && t.ID != _township.ID && WorldBuilderStatic.townshipDatas[t.GetTypeName()].SpawnGateway;
						});
					}
					List<Township> list = highwayTownships.FindAll(match);
					list.Sort((Township _t1, Township _t2) => Vector2i.DistanceSqr(CS$<>8__locals2.gateway.GridPosition, _t1.GridCenter).CompareTo(Vector2i.DistanceSqr(CS$<>8__locals2.gateway.GridPosition, _t2.GridCenter)));
					Path closePath = null;
					Township closeTownship = null;
					int tries = 0;
					foreach (Township townshipNear in list)
					{
						yield return this.GetPathToTownship(CS$<>8__locals2.gateway, townshipNear);
						Path path = this.getPathToTownshipResult;
						if (path != null)
						{
							this.getPathToTownshipResult = null;
							int num;
							_township.TownshipConnectionCounts.TryGetValue(townshipNear, out num);
							if (_township.Streets.Count <= 1 || townshipNear.Streets.Count <= 1)
							{
								if (num > 0)
								{
									path.Cost *= 4;
								}
							}
							else if (num > 0)
							{
								path.Cost = (int)((float)path.Cost * 1.6f);
							}
							if (closePath == null || path.Cost < closePath.Cost)
							{
								closePath = path;
								closeTownship = townshipNear;
							}
							num2 = tries + 1;
							tries = num2;
							if (num2 >= 3)
							{
								break;
							}
						}
						townshipNear = null;
					}
					List<Township>.Enumerator enumerator = default(List<Township>.Enumerator);
					if (closePath != null)
					{
						int num3;
						_township.TownshipConnectionCounts.TryGetValue(closeTownship, out num3);
						_township.TownshipConnectionCounts[closeTownship] = num3 + 1;
						closeTownship.TownshipConnectionCounts.TryGetValue(_township, out num3);
						closeTownship.TownshipConnectionCounts[_township] = num3 + 1;
						this.worldBuilder.paths.Add(closePath);
						this.SetTileExits(closePath);
						closePath.commitPathingMapData();
					}
					CS$<>8__locals2 = null;
					closePath = null;
					closeTownship = null;
				}
				num2 = i;
			}
			yield break;
			yield break;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public IEnumerator ConnectSelf(Township _township)
		{
			_township.SortGatewaysClockwise();
			int count = _township.Gateways.Count;
			int num;
			for (int i = 0; i < count; i = num + 1)
			{
				StreetTile gateway = _township.Gateways[i];
				if (gateway.UsedExitList.Count < 4)
				{
					StreetTile gateway2 = _township.Gateways[(i + 1) % count];
					if (gateway2.UsedExitList.Count < 4)
					{
						int closeDist = int.MaxValue;
						Path closePath = null;
						List<Vector2i> highwayExits = gateway.GetHighwayExits(true);
						foreach (Vector2i startPosition in highwayExits)
						{
							foreach (Vector2i endPosition in gateway2.GetHighwayExits(true))
							{
								Path path = new Path(this.worldBuilder, startPosition, endPosition, 4, false, false);
								if (path.IsValid)
								{
									int cost = path.Cost;
									if (cost < closeDist)
									{
										closeDist = cost;
										closePath = path;
									}
								}
								path.Dispose();
							}
							if (this.worldBuilder.IsMessageElapsed())
							{
								yield return this.worldBuilder.SetMessage(string.Format(Localization.Get("xuiRwgHighwaysTownExitsSelf", false), gateway.Township.GetTypeName()), false, false);
							}
						}
						List<Vector2i>.Enumerator enumerator = default(List<Vector2i>.Enumerator);
						if (closePath != null)
						{
							this.worldBuilder.paths.Add(closePath);
							this.SetTileExits(closePath);
							closePath.commitPathingMapData();
						}
						gateway = null;
						gateway2 = null;
						closePath = null;
					}
				}
				num = i;
			}
			yield break;
			yield break;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void SetTileExits(Path path)
		{
			this.SetTileExit(path, path.StartPosition);
			this.SetTileExit(path, path.EndPosition);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void SetTileExit(Path currentPath, Vector2i exit)
		{
			StreetTile streetTile = this.worldBuilder.GetStreetTileWorld(exit);
			if (streetTile != null)
			{
				if (streetTile.District != null && streetTile.District.name == "gateway")
				{
					this.ExitConnections.Add(new HighwayPlanner.ExitConnection(streetTile, exit, currentPath));
					return;
				}
				foreach (StreetTile streetTile2 in streetTile.GetNeighbors())
				{
					if (streetTile2 != null && streetTile2.District != null && streetTile2.District.name == "gateway")
					{
						this.ExitConnections.Add(new HighwayPlanner.ExitConnection(streetTile2, exit, currentPath));
						return;
					}
				}
				streetTile = null;
			}
			if (streetTile == null)
			{
				Township township = null;
				foreach (Township township2 in this.worldBuilder.Townships)
				{
					if (township2.Area.Contains(exit.AsVector2()))
					{
						township = township2;
						break;
					}
				}
				if (township != null)
				{
					foreach (StreetTile streetTile3 in township.Gateways)
					{
						for (int j = 0; j < 4; j++)
						{
							if (streetTile3.getHighwayExitPosition(j) == exit || Vector2i.DistanceSqr(streetTile3.getHighwayExitPosition(j), exit) < 100f)
							{
								this.ExitConnections.Add(new HighwayPlanner.ExitConnection(streetTile3, exit, currentPath));
								return;
							}
						}
					}
				}
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static int GetDist(Township thisTownship, Township otherTownship, ref int closestDist)
		{
			foreach (Vector2i a in thisTownship.GetUnusedTownExits(4))
			{
				foreach (Vector2i b in otherTownship.GetUnusedTownExits(4))
				{
					int num = Vector2i.DistanceSqrInt(a, b);
					if (num < closestDist)
					{
						closestDist = num;
					}
				}
			}
			return closestDist;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public IEnumerator GetPathToTownship(StreetTile gateway, Township otherTownship)
		{
			int closestDist = int.MaxValue;
			Path closestPath = null;
			List<Vector2i> highwayExits = gateway.GetHighwayExits(true);
			foreach (Vector2i startPosition in highwayExits)
			{
				foreach (Vector2i endPosition in otherTownship.GetUnusedTownExits(3))
				{
					Path path = new Path(this.worldBuilder, startPosition, endPosition, 4, false, false);
					if (path.IsValid)
					{
						int cost = path.Cost;
						if (cost < closestDist)
						{
							closestDist = cost;
							closestPath = path;
						}
					}
					path.Dispose();
				}
				if (this.worldBuilder.IsMessageElapsed())
				{
					yield return this.worldBuilder.SetMessage(string.Format(Localization.Get("xuiRwgHighwaysTownExitsOther", false), gateway.Township.GetTypeName()), false, false);
				}
			}
			List<Vector2i>.Enumerator enumerator = default(List<Vector2i>.Enumerator);
			this.getPathToTownshipResult = closestPath;
			yield break;
			yield break;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public int GetClosestPathToTownship(Township thisTownship, Township otherTownship)
		{
			int num = int.MaxValue;
			foreach (Vector2i start in thisTownship.GetUnusedTownExits(4))
			{
				foreach (Vector2i end in otherTownship.GetUnusedTownExits(4))
				{
					int pathCost = this.worldBuilder.PathingUtils.GetPathCost(start, end, false);
					if (pathCost >= 3 && pathCost < num)
					{
						num = pathCost;
					}
				}
			}
			return num;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static int distanceSqr(Vector2i pointA, Vector2i pointB)
		{
			Vector2i vector2i = pointA - pointB;
			return vector2i.x * vector2i.x + vector2i.y * vector2i.y;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly WorldBuilder worldBuilder;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly List<HighwayPlanner.ExitConnection> ExitConnections = new List<HighwayPlanner.ExitConnection>();

		[PublicizedFrom(EAccessModifier.Private)]
		public Path getPathToTownshipResult;

		public enum CDirs
		{
			North,
			East,
			South,
			West,
			Invalid = -1
		}

		public class ExitConnection
		{
			public ExitConnection(StreetTile parent, Vector2i worldPos, Path connectedPath = null)
			{
				this.ParentTile = parent;
				this.WorldPosition = worldPos;
				this.ConnectedPath = connectedPath;
				for (int i = 0; i < 4; i++)
				{
					if (parent.getHighwayExitPosition(i) == this.WorldPosition)
					{
						this.ExitDir = i;
						break;
					}
				}
				parent.SetExitUsed(this.WorldPosition);
			}

			public bool SetExitUsedManually()
			{
				return (this.ParentTile.UsedExitList.Contains(this.ParentTile.getHighwayExitPosition(this.ExitDir)) && (this.ParentTile.ConnectedExits & 1 << this.ExitDir) > 0 && this.ParentTile.RoadExits[this.ExitDir]) || this.ParentTile.SetExitUsed(this.WorldPosition);
			}

			public StreetTile ParentTile;

			public Vector2i WorldPosition;

			public int ExitDir = -1;

			public Path ConnectedPath;
		}
	}
}
