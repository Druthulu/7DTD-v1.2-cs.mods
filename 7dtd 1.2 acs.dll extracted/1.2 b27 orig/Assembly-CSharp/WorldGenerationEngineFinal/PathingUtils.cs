using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace WorldGenerationEngineFinal
{
	public class PathingUtils
	{
		public PathingUtils(WorldBuilder _worldBuilder)
		{
			this.worldBuilder = _worldBuilder;
		}

		public bool HasValidPath(Vector2i start, Vector2i end, bool isCountryRoad = false)
		{
			Vector2i vector2i = new Vector2i(Mathf.Min(start.x, end.x), Mathf.Min(start.y, end.y)) / 10;
			vector2i.x = Mathf.Max(vector2i.x - 15, 0);
			vector2i.y = Mathf.Max(vector2i.y - 15, 0);
			Vector2i vector2i2 = new Vector2i(Mathf.Max(start.x, end.x), Mathf.Max(start.y, end.y)) / 10;
			vector2i2.x = Mathf.Min(vector2i2.x + 15, this.worldBuilder.WorldSize / 10 - 1);
			vector2i2.y = Mathf.Min(vector2i2.y + 15, this.worldBuilder.WorldSize / 10 - 1);
			bool result = this.FindDetailedPath(start / 10, end / 10, isCountryRoad, false, vector2i, vector2i2, 200) != null;
			this.nodePool.ReturnAll();
			return result;
		}

		public int GetPathCost(Vector2i start, Vector2i end, bool isCountryRoad = false)
		{
			PathNode pathNode = this.FindDetailedPath(start / 10, end / 10, isCountryRoad, false);
			int num = 0;
			while (pathNode != null)
			{
				num++;
				pathNode = pathNode.next;
			}
			this.nodePool.ReturnAll();
			return num;
		}

		public List<Vector2i> GetPath(Vector2i start, Vector2i end, bool isCountryRoad)
		{
			PathNode pathNode = this.FindDetailedPath(start / 10, end / 10, isCountryRoad, false, new Vector2i(Mathf.Min(start.x, end.x), Mathf.Min(start.y, end.y)) / 10, new Vector2i(Mathf.Max(start.x, end.x), Mathf.Max(start.y, end.y)) / 10, 200);
			this.path.Clear();
			while (pathNode != null)
			{
				this.pathTilePosition = pathNode.position * 10 + PathNode.offset;
				this.path.Add(this.pathTilePosition);
				pathNode = pathNode.next;
			}
			this.nodePool.ReturnAll();
			return this.path;
		}

		public List<Vector2i> GetPath(Path p, Vector2i start, Vector2i end)
		{
			PathNode pathNode = this.FindDetailedPath(start / 10, end / 10, p.isCountryRoad, p.isRiver, new Vector2i(Mathf.Min(start.x, end.x), Mathf.Min(start.y, end.y)) / 10, new Vector2i(Mathf.Max(start.x, end.x), Mathf.Max(start.y, end.y)) / 10, 200);
			this.path.Clear();
			while (pathNode != null)
			{
				this.pathTilePosition = pathNode.position * 10 + PathNode.offset;
				this.path.Add(this.pathTilePosition);
				pathNode = pathNode.next;
			}
			this.nodePool.ReturnAll();
			return this.path;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public PathNode FindDetailedPath(Vector2i startPos, Vector2i endPos, bool _isCountryRoad, bool isRiver, Vector2i boundsMin, Vector2i boundsMax, int padding = 200)
		{
			int num = this.worldBuilder.WorldSize / 10 + 1;
			if (this.cachedClosedList == null || this.cachedClosedList.GetLength(0) != num)
			{
				this.cachedClosedList = new bool[num, num];
			}
			PathingUtils.MinHeapBinned minHeapBinned = new PathingUtils.MinHeapBinned(this.nodeBins);
			bool[,] array = this.cachedClosedList;
			Array.Clear(array, 0, array.Length);
			PathNode pathNode = this.nodePool.Alloc();
			pathNode.Set(startPos, 0f, null);
			minHeapBinned.Add(pathNode);
			array[startPos.x, startPos.y] = true;
			if (!this.InBounds(startPos))
			{
				return null;
			}
			if (!this.InBounds(endPos))
			{
				return null;
			}
			int num2 = Mathf.Max(0, boundsMin.x - padding);
			int num3 = Mathf.Max(0, boundsMin.y - padding);
			int num4 = Mathf.Min(boundsMax.x + padding, array.GetLength(0) - 1);
			int num5 = Mathf.Min(boundsMax.y + padding, array.GetLength(1) - 1);
			float num6 = _isCountryRoad ? 6.5f : 11f;
			PathNode pathNode2;
			while ((pathNode2 = minHeapBinned.ExtractFirst()) != null)
			{
				Vector2i position = pathNode2.position;
				if (position == endPos)
				{
					return pathNode2;
				}
				for (int i = 0; i < 8; i++)
				{
					Vector2i vector2i = this.normalNeighbors[i];
					Vector2i vector2i2 = pathNode2.position + vector2i;
					if (vector2i2.x >= num2 && vector2i2.y >= num3 && vector2i2.x < num4 && vector2i2.y < num5 && !array[vector2i2.x, vector2i2.y])
					{
						if (vector2i2 != endPos && vector2i2 != startPos && this.IsBlocked(vector2i2.x, vector2i2.y, isRiver))
						{
							array[vector2i2.x, vector2i2.y] = true;
						}
						else
						{
							float num7 = Utils.FastAbs(this.GetHeight(position) - this.GetHeight(vector2i2));
							if (num7 <= num6)
							{
								num7 *= 10f;
								float num8 = Vector2i.Distance(vector2i2, endPos) + num7;
								if (!_isCountryRoad)
								{
									StreetTile streetTileWorld = this.worldBuilder.GetStreetTileWorld(vector2i2 * 10);
									if (streetTileWorld != null && streetTileWorld.ContainsHighway)
									{
										if (streetTileWorld.ConnectedHighways.Count > 2)
										{
											goto IL_447;
										}
										if ((vector2i2.x != endPos.x || vector2i2.y != endPos.y) && (vector2i2.x != startPos.x || vector2i2.y != startPos.y))
										{
											PathTile pathTile = this.worldBuilder.PathingGrid[vector2i2.x, vector2i2.y];
											bool flag = pathTile != null && pathTile.TileState == PathTile.PathTileStates.Highway;
											if (vector2i.x != 0 && vector2i.y != 0)
											{
												for (int j = 0; j < 2; j++)
												{
													Vector2i vector2i3;
													if (j != 0)
													{
														if (j != 1)
														{
															throw new IndexOutOfRangeException("FindDetailedPath direction loop iterating past defined Vectors");
														}
														vector2i3 = new Vector2i(0, -vector2i.y);
													}
													else
													{
														vector2i3 = new Vector2i(-vector2i.x, 0);
													}
													Vector2i vector2i4 = vector2i3;
													if (this.IsBlocked(vector2i2.x + vector2i4.x, vector2i2.y + vector2i4.y, false))
													{
														flag = true;
													}
													else
													{
														PathTile pathTile2 = this.worldBuilder.PathingGrid[vector2i2.x + vector2i4.x, vector2i2.y + vector2i4.y];
														if (pathTile2 != null && pathTile2.TileState == PathTile.PathTileStates.Highway)
														{
															flag = true;
														}
													}
												}
											}
											if (flag)
											{
												goto IL_447;
											}
										}
										num8 *= 2f;
									}
								}
								if (vector2i.x != 0 && vector2i.y != 0)
								{
									num8 *= 1.2f;
								}
								if (this.pathingGrid != null)
								{
									int num9 = (int)this.pathingGrid[vector2i2.x + vector2i2.y * this.pathingGridSize];
									if (num9 > 0)
									{
										num8 *= (float)num9;
									}
								}
								array[vector2i2.x, vector2i2.y] = true;
								PathNode pathNode3 = this.nodePool.Alloc();
								pathNode3.Set(vector2i2, pathNode2.pathCost + num8, pathNode2);
								minHeapBinned.Add(pathNode3);
							}
						}
					}
					IL_447:;
				}
			}
			return null;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public PathNode FindDetailedPath(Vector2i startIndex, Vector2i endIndex, bool isCountryRoad, bool isRiver = false)
		{
			return this.FindDetailedPath(startIndex, endIndex, isCountryRoad, isRiver, Vector2i.zero, Vector2i.one * (this.worldBuilder.WorldSize / 10 + 1), 200);
		}

		public bool IsBlocked(int pathX, int pathY, bool isRiver = false)
		{
			if (this.IsPathBlocked(pathX, pathY))
			{
				return true;
			}
			Vector2i vector2i = this.pathPositionToWorldCenter(pathX, pathY);
			if (!this.InWorldBounds(vector2i.x, vector2i.y))
			{
				return true;
			}
			StreetTile streetTileWorld = this.worldBuilder.GetStreetTileWorld(vector2i.x, vector2i.y);
			return this.InCityLimits(streetTileWorld) || this.IsRadiation(streetTileWorld) || (!isRiver && this.IsWater(pathX, pathY));
		}

		public bool InBounds(Vector2i pos)
		{
			return this.InBounds(pos.x, pos.y);
		}

		public bool InBounds(int pathX, int pathY)
		{
			Vector2i vector2i = this.pathPositionToWorldCenter(pathX, pathY);
			return (ulong)vector2i.x < (ulong)((long)this.worldBuilder.WorldSize) && (ulong)vector2i.y < (ulong)((long)this.worldBuilder.WorldSize);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool InWorldBounds(int x, int y)
		{
			return (ulong)x < (ulong)((long)this.worldBuilder.WorldSize) && (ulong)y < (ulong)((long)this.worldBuilder.WorldSize);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IsRadiation(StreetTile st)
		{
			return (st == null || st.OverlapsRadiation) && this.worldBuilder.GetRad(this.wPos.x, this.wPos.y) > 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool InCityLimits(StreetTile st)
		{
			return st != null && st.Township != null && st.Township.Type != TownshipStatic.TypesByName["wilderness"];
		}

		public bool IsWater(Vector2i pos)
		{
			return this.IsWater(pos.x, pos.y);
		}

		public bool IsWater(int pathX, int pathY)
		{
			Vector2i vector2i = this.pathPositionToWorldMin(pathX, pathY);
			StreetTile streetTileWorld = this.worldBuilder.GetStreetTileWorld(vector2i);
			if (streetTileWorld == null || streetTileWorld.OverlapsWater)
			{
				for (int i = vector2i.y; i < vector2i.y + 10; i++)
				{
					for (int j = vector2i.x; j < vector2i.x + 10; j++)
					{
						if (this.worldBuilder.GetWater(j, i) > 0)
						{
							return true;
						}
					}
				}
			}
			return false;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public float GetHeight(Vector2i pos)
		{
			return this.GetHeight(pos.x, pos.y);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public float GetHeight(int pathX, int pathY)
		{
			return this.worldBuilder.GetHeight(this.pathPositionToWorldCenter(pathX, pathY));
		}

		public BiomeType GetBiome(Vector2i pos)
		{
			return this.GetBiome(pos.x, pos.y);
		}

		public BiomeType GetBiome(int pathX, int pathY)
		{
			return this.worldBuilder.GetBiome(this.pathPositionToWorldCenter(pathX, pathY));
		}

		[PublicizedFrom(EAccessModifier.Private)]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Vector2i pathPositionToWorldCenter(int pathX, int pathY)
		{
			this.wPos.x = pathX * 10 + 5;
			this.wPos.y = pathY * 10 + 5;
			return this.wPos;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public Vector2i pathPositionToWorldMin(int pathX, int pathY)
		{
			this.wPos.x = pathX * 10;
			this.wPos.y = pathY * 10;
			return this.wPos;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public Vector2i pathPositionToWorldMax(int pathX, int pathY)
		{
			return new Vector2i(pathX * 10 + 10, pathY * 10 + 10);
		}

		public void AddPrefabRect(Rect r)
		{
			int num = (int)r.yMin;
			while ((float)num < r.yMax)
			{
				int num2 = (int)r.xMin;
				while ((float)num2 < r.xMax)
				{
					this.SetPathBlocked(num2 / 10, num / 10, true);
					num2 += 10;
				}
				num += 10;
			}
		}

		public void AddMoveLimitArea(Rect r)
		{
			int num = (int)r.xMin;
			int num2 = (int)r.yMin;
			num /= 10;
			num2 /= 10;
			for (int i = 0; i < 15; i++)
			{
				for (int j = 0; j < 15; j++)
				{
					if (j != 7 && i != 7)
					{
						this.SetPathBlocked(num + j, num2 + i, true);
					}
				}
			}
		}

		public void RemoveFullyBlockedArea(Rect r)
		{
			int num = (int)r.xMin;
			int num2 = (int)r.yMin;
			num /= 10;
			num2 /= 10;
			for (int i = 0; i < 15; i++)
			{
				for (int j = 0; j < 15; j++)
				{
					this.SetPathBlocked(num + j, num2 + i, false);
				}
			}
		}

		public void AddFullyBlockedArea(Rect r)
		{
			int num = (int)(r.xMin + 0.5f);
			int num2 = (int)(r.yMin + 0.5f);
			num /= 10;
			num2 /= 10;
			for (int i = 0; i < 15; i++)
			{
				for (int j = 0; j < 15; j++)
				{
					this.SetPathBlocked(num + j, num2 + i, true);
				}
			}
		}

		public void SetPathBlocked(Vector2i pos, bool isBlocked)
		{
			this.SetPathBlocked(pos.x, pos.y, isBlocked);
		}

		public void SetPathBlocked(int x, int y, bool isBlocked)
		{
			this.SetPathBlocked(x, y, isBlocked ? sbyte.MinValue : 0);
		}

		public void SetPathBlocked(int x, int y, sbyte costMult)
		{
			if (this.pathingGrid == null)
			{
				this.SetupPathingGrid();
			}
			if ((ulong)x >= (ulong)((long)this.pathingGridSize) || (ulong)y >= (ulong)((long)this.pathingGridSize))
			{
				return;
			}
			this.pathingGrid[x + y * this.pathingGridSize] = costMult;
		}

		public bool IsPathBlocked(Vector2i pos)
		{
			return this.IsPathBlocked(pos.x, pos.y);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IsPathBlocked(int x, int y)
		{
			return (ulong)x < (ulong)((long)this.pathingGridSize) && (ulong)y < (ulong)((long)this.pathingGridSize) && this.pathingGrid[x + y * this.pathingGridSize] == sbyte.MinValue;
		}

		public bool IsPointOnHighwayWorld(int x, int y)
		{
			return this.worldBuilder.PathingGrid[x / 10, y / 10] != null && this.worldBuilder.PathingGrid[x, y].TileState == PathTile.PathTileStates.Highway;
		}

		public bool IsPointOnCountryRoadWorld(int x, int y)
		{
			return this.worldBuilder.PathingGrid[x / 10, y / 10] != null && this.worldBuilder.PathingGrid[x, y].TileState == PathTile.PathTileStates.Country;
		}

		public void SetupPathingGrid()
		{
			this.pathingGridSize = this.worldBuilder.WorldSize / 10;
			this.pathingGrid = new sbyte[this.pathingGridSize * this.pathingGridSize];
		}

		public void Cleanup()
		{
			this.cachedClosedList = null;
			this.pathingGrid = null;
			this.pathingGridSize = 0;
			this.nodeBins = null;
			this.nodePool.Cleanup();
		}

		public const int PATHING_GRID_TILE_SIZE = 10;

		[PublicizedFrom(EAccessModifier.Private)]
		public const float cRoadCountryMaxStepH = 6.5f;

		[PublicizedFrom(EAccessModifier.Private)]
		public const float cRoadHighwayMaxStepH = 11f;

		[PublicizedFrom(EAccessModifier.Private)]
		public const float cHeightCostScale = 10f;

		[PublicizedFrom(EAccessModifier.Private)]
		public const int cNormalNeighborsCount = 8;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly Vector2i[] normalNeighbors = new Vector2i[]
		{
			new Vector2i(0, 1),
			new Vector2i(1, 1),
			new Vector2i(1, 0),
			new Vector2i(1, -1),
			new Vector2i(0, -1),
			new Vector2i(-1, -1),
			new Vector2i(-1, 0),
			new Vector2i(-1, 1)
		};

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly Vector2i[] normalNeighbors4way = new Vector2i[]
		{
			new Vector2i(0, 1),
			new Vector2i(1, 0),
			new Vector2i(0, -1),
			new Vector2i(-1, 0)
		};

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly WorldBuilder worldBuilder;

		[PublicizedFrom(EAccessModifier.Private)]
		public sbyte[] pathingGrid;

		[PublicizedFrom(EAccessModifier.Private)]
		public Vector2i pathTilePosition;

		[PublicizedFrom(EAccessModifier.Private)]
		public List<Vector2i> path = new List<Vector2i>();

		[PublicizedFrom(EAccessModifier.Private)]
		public bool[,] cachedClosedList;

		[PublicizedFrom(EAccessModifier.Private)]
		public PathNodePool nodePool = new PathNodePool(100000);

		[PublicizedFrom(EAccessModifier.Private)]
		public PathNode[] nodeBins;

		[PublicizedFrom(EAccessModifier.Private)]
		public Vector2i wPos;

		[PublicizedFrom(EAccessModifier.Private)]
		public int pathingGridSize;

		[PublicizedFrom(EAccessModifier.Private)]
		public class MinHeap
		{
			public void Add(PathNode item)
			{
				if (this.listHead == null)
				{
					this.listHead = item;
					return;
				}
				if (this.listHead.next == null && item.pathCost <= this.listHead.pathCost)
				{
					item.nextListElem = this.listHead;
					this.listHead = item;
					return;
				}
				PathNode pathNode = this.listHead;
				PathNode nextListElem = pathNode.nextListElem;
				while (nextListElem != null && nextListElem.pathCost < item.pathCost)
				{
					pathNode = nextListElem;
					nextListElem = pathNode.nextListElem;
				}
				item.nextListElem = nextListElem;
				pathNode.nextListElem = item;
			}

			public PathNode ExtractFirst()
			{
				PathNode pathNode = this.listHead;
				if (pathNode != null)
				{
					this.listHead = this.listHead.nextListElem;
				}
				return pathNode;
			}

			[PublicizedFrom(EAccessModifier.Private)]
			public PathNode listHead;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public class MinHeapBinned
		{
			public MinHeapBinned(PathNode[] _nodeBins)
			{
				this.nodeBins = _nodeBins;
				if (this.nodeBins == null)
				{
					this.nodeBins = new PathNode[32768];
					return;
				}
				Array.Clear(this.nodeBins, 0, 32768);
			}

			public PathNode ExtractFirst()
			{
				if (this.lowBin <= this.highBin)
				{
					PathNode pathNode = this.nodeBins[this.lowBin];
					this.nodeBins[this.lowBin] = pathNode.nextListElem;
					if (pathNode.nextListElem == null)
					{
						int num;
						do
						{
							num = this.lowBin + 1;
							this.lowBin = num;
						}
						while (num <= this.highBin && this.nodeBins[this.lowBin] == null);
						if (this.lowBin > this.highBin)
						{
							this.lowBin = 32768;
							this.highBin = 0;
						}
					}
					return pathNode;
				}
				return null;
			}

			public void Add(PathNode item)
			{
				int num = (int)(item.pathCost * 0.07f);
				if (num >= 32768)
				{
					num = 32767;
				}
				if (num < this.lowBin)
				{
					this.lowBin = num;
				}
				if (num > this.highBin)
				{
					this.highBin = num;
				}
				PathNode pathNode = this.nodeBins[num];
				if (pathNode == null)
				{
					this.nodeBins[num] = item;
					return;
				}
				if (pathNode.next == null && item.pathCost <= pathNode.pathCost)
				{
					item.nextListElem = pathNode;
					this.nodeBins[num] = item;
					return;
				}
				PathNode pathNode2 = pathNode;
				PathNode nextListElem = pathNode2.nextListElem;
				while (nextListElem != null && nextListElem.pathCost < item.pathCost)
				{
					pathNode2 = nextListElem;
					nextListElem = pathNode2.nextListElem;
				}
				item.nextListElem = nextListElem;
				pathNode2.nextListElem = item;
			}

			[PublicizedFrom(EAccessModifier.Private)]
			public readonly PathNode[] nodeBins;

			[PublicizedFrom(EAccessModifier.Private)]
			public const int cBins = 32768;

			[PublicizedFrom(EAccessModifier.Private)]
			public const float cScale = 0.07f;

			[PublicizedFrom(EAccessModifier.Private)]
			public int lowBin = 32768;

			[PublicizedFrom(EAccessModifier.Private)]
			public int highBin;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public enum PathNodeType
		{
			Free,
			Road,
			Prefab,
			CityLimits = 4,
			Blocked = 8
		}
	}
}
