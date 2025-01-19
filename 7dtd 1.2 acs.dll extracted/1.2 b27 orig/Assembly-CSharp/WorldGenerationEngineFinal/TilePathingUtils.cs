using System;
using System.Collections.Generic;
using UnityEngine;

namespace WorldGenerationEngineFinal
{
	public class TilePathingUtils
	{
		public TilePathingUtils(WorldBuilder _worldBuilder)
		{
			this.worldBuilder = _worldBuilder;
		}

		public List<StreetTile> CreatePath(StreetTile start, StreetTile end, Vector2i dir)
		{
			TilePathingUtils.PathNode pathNode = this.FindPath(start, end, dir);
			List<StreetTile> list = new List<StreetTile>();
			while (pathNode != null)
			{
				StreetTile streetTileGrid = this.worldBuilder.GetStreetTileGrid(pathNode.position);
				list.Add(streetTileGrid);
				pathNode = pathNode.next;
			}
			return list;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public TilePathingUtils.PathNode FindPath(StreetTile start, StreetTile end, Vector2i dir)
		{
			TilePathingUtils.MinHeap minHeap = new TilePathingUtils.MinHeap();
			minHeap.Add(new TilePathingUtils.PathNode(start.GridPosition, 0, null));
			bool[,] array = new bool[this.worldBuilder.StreetTileMap.GetLength(0), this.worldBuilder.StreetTileMap.GetLength(1)];
			array[start.GridPosition.x, start.GridPosition.y] = true;
			TilePathingUtils.PathNode pathNode = null;
			while (minHeap.HasNext())
			{
				pathNode = minHeap.ExtractFirst();
				Vector2i position = pathNode.position;
				if (position == end.GridPosition)
				{
					return pathNode;
				}
				StreetTile streetTileGrid = this.worldBuilder.GetStreetTileGrid(position.x, position.y);
				if (streetTileGrid != null)
				{
					foreach (StreetTile streetTile in streetTileGrid.GetNeighbors())
					{
						if (streetTile != null && !array[streetTile.GridPosition.x, streetTile.GridPosition.y] && !streetTile.OverlapsRadiation && !streetTile.OverlapsWater && !streetTile.HasSteepSlope && Mathf.CeilToInt(Mathf.Abs(streetTileGrid.PositionHeight - streetTile.PositionHeight)) <= 10 && streetTile.TerrainType != TerrainType.mountains)
						{
							bool flag = true;
							StreetTile[] neighbors2 = streetTile.GetNeighbors();
							for (int j = 0; j < neighbors2.Length; j++)
							{
								if (neighbors2[j].TerrainType == TerrainType.mountains)
								{
									flag = false;
									break;
								}
							}
							if (flag && (streetTile.Township == null || !(streetTile.District.name != "highway")))
							{
								int num = TilePathingUtils.distanceSqr(streetTile.WorldPosition, end.WorldPosition);
								if (streetTile.District != null && streetTile.District.name == "highway")
								{
									num /= 5;
								}
								int pathCost = pathNode.pathCost + num;
								minHeap.Add(new TilePathingUtils.PathNode(streetTile.GridPosition, pathCost, pathNode));
								array[streetTile.GridPosition.x, streetTile.GridPosition.y] = true;
							}
						}
					}
				}
			}
			Log.Error("Could not find path, outputting what WAS found for testing. \n Desired Start Position {0} \n Desired End Position {1}", new object[]
			{
				start.GridPosition.ToString(),
				end.GridPosition.ToString()
			});
			return pathNode;
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
		public readonly List<Vector2i> dir4way = new List<Vector2i>
		{
			new Vector2i(0, 1),
			new Vector2i(-1, 0),
			new Vector2i(1, 0),
			new Vector2i(0, -1)
		};

		public class PathNode
		{
			public PathNode(Vector2i position, int pathCost, TilePathingUtils.PathNode next)
			{
				this.position = position;
				this.pathCost = pathCost;
				this.next = next;
			}

			public Vector2i position;

			public int pathCost;

			public TilePathingUtils.PathNode next;

			public TilePathingUtils.PathNode nextListElem;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public class MinHeap
		{
			public bool HasNext()
			{
				return this.listHead != null;
			}

			public void Add(TilePathingUtils.PathNode item)
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
				TilePathingUtils.PathNode nextListElem = this.listHead;
				while (nextListElem.nextListElem != null && nextListElem.nextListElem.pathCost < item.pathCost)
				{
					nextListElem = nextListElem.nextListElem;
				}
				item.nextListElem = nextListElem.nextListElem;
				nextListElem.nextListElem = item;
			}

			public TilePathingUtils.PathNode ExtractFirst()
			{
				TilePathingUtils.PathNode result = this.listHead;
				this.listHead = this.listHead.nextListElem;
				return result;
			}

			[PublicizedFrom(EAccessModifier.Private)]
			public TilePathingUtils.PathNode listHead;
		}
	}
}
