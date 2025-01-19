using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace WorldGenerationEngineFinal
{
	public class Path
	{
		public Path(WorldBuilder _worldBuilder, Vector2i _startPosition, Vector2i _endPosition, int _lanes, bool _isCountryRoad, bool _validityTest = false)
		{
			this.worldBuilder = _worldBuilder;
			this.StartPosition = _startPosition;
			this.EndPosition = _endPosition;
			this.lanes = _lanes;
			this.radius = (float)this.lanes * 0.5f * 4.5f;
			this.isCountryRoad = _isCountryRoad;
			this.validityTestOnly = _validityTest;
			this.CreatePath();
		}

		public Path(WorldBuilder _worldBuilder, Vector2i _startPosition, Vector2i _endPosition, float _radius, bool _isCountryRoad, bool _validityTest = false)
		{
			this.worldBuilder = _worldBuilder;
			this.StartPosition = _startPosition;
			this.EndPosition = _endPosition;
			this.radius = _radius;
			this.lanes = Mathf.CeilToInt(this.radius / 4.5f * 2f);
			this.isCountryRoad = _isCountryRoad;
			this.validityTestOnly = _validityTest;
			this.CreatePath();
		}

		public Path(WorldBuilder _worldBuilder, bool _isCountryRoad = false, float _radius = 8f, bool _validityTest = false)
		{
			this.worldBuilder = _worldBuilder;
			this.isCountryRoad = _isCountryRoad;
			this.FinalPathPoints = new List<Vector2>();
			this.radius = _radius;
			this.validityTestOnly = _validityTest;
		}

		public void Dispose()
		{
			if (this.isCountryRoad)
			{
				return;
			}
			StreetTile streetTile = null;
			for (int i = 0; i < this.FinalPathPoints.Count; i++)
			{
				Vector2i pos;
				pos.x = (int)this.FinalPathPoints[i].x;
				pos.y = (int)this.FinalPathPoints[i].y;
				StreetTile streetTileWorld = this.worldBuilder.GetStreetTileWorld(pos);
				if (streetTileWorld != streetTile && streetTileWorld != null)
				{
					streetTileWorld.ConnectedHighways.Remove(this);
				}
				streetTile = streetTileWorld;
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void ProcessPath()
		{
			int num = this.isCountryRoad ? 2 : 4;
			for (int i = 0; i < num; i++)
			{
				this.roundOffCorners();
			}
			float num2 = (float)this.worldBuilder.WorldSize;
			float num3 = 0f;
			for (int j = 0; j < this.FinalPathPoints.Count; j++)
			{
				float x = this.FinalPathPoints[j].x;
				if (x < 0f || x >= num2)
				{
					this.IsValid = false;
					return;
				}
				float y = this.FinalPathPoints[j].y;
				if (y < 0f || y >= num2)
				{
					this.IsValid = false;
					return;
				}
				if (j > 0)
				{
					num3 += Vector2.Distance(this.FinalPathPoints[j - 1], this.FinalPathPoints[j]);
				}
				this.pathPoints3d.Add(new Vector3(x, this.worldBuilder.GetHeight((int)x, (int)y), y));
			}
			this.Cost = Mathf.RoundToInt(num3);
			float[] heights = new float[this.pathPoints3d.Count];
			if (this.isCountryRoad)
			{
				for (int k = 0; k < 4; k++)
				{
					this.SmoothHeights(heights);
				}
			}
			else
			{
				float num4 = 0f;
				for (int l = 0; l < this.pathPoints3d.Count; l++)
				{
					num4 += this.pathPoints3d[l].y;
				}
				num4 /= (float)this.pathPoints3d.Count;
				num4 += 8f;
				for (int m = 0; m < this.pathPoints3d.Count; m++)
				{
					Vector3 vector = this.pathPoints3d[m];
					if (vector.y > num4)
					{
						vector.y = num4 * 0.3f + vector.y * 0.7f;
						this.pathPoints3d[m] = vector;
					}
				}
				for (int n = 0; n < 50; n++)
				{
					this.SmoothHeights(heights);
				}
			}
			this.FinalPathPoints.Clear();
			Vector2 zero = Vector2.zero;
			for (int num5 = 0; num5 < this.pathPoints3d.Count; num5++)
			{
				zero.x = (float)((int)(this.pathPoints3d[num5].x + 0.5f));
				zero.y = (float)((int)(this.pathPoints3d[num5].z + 0.5f));
				this.FinalPathPoints.Add(zero);
			}
		}

		public void DrawPathToRoadIds(byte[] ids)
		{
			float num = this.radius;
			if (this.isCountryRoad)
			{
				num += 6f;
			}
			else
			{
				num += 10f;
			}
			object obj = (this.lanes >= 2) ? (this.radius - 1f) : this.radius;
			float num2 = this.radius * this.radius;
			float num3 = num * num;
			object obj2 = obj;
			float num4 = obj2 * obj2;
			for (int i = 0; i < this.FinalPathPoints.Count - 1; i++)
			{
				float x = this.FinalPathPoints[i].x;
				float y = this.FinalPathPoints[i].y;
				float x2 = this.FinalPathPoints[i + 1].x;
				float y2 = this.FinalPathPoints[i + 1].y;
				int num5 = (int)(Utils.FastMin(x, x2) - num - 1.5f);
				num5 = Utils.FastMax(0, num5);
				int num6 = (int)(Utils.FastMax(x, x2) + num + 1.5f);
				num6 = Utils.FastMin(num6, this.worldBuilder.WorldSize - 1);
				int num7 = (int)(Utils.FastMin(y, y2) - num - 1.5f);
				num7 = Utils.FastMax(0, num7);
				int num8 = (int)(Utils.FastMax(y, y2) + num + 1.5f);
				num8 = Utils.FastMin(num8, this.worldBuilder.WorldSize - 1);
				for (int j = num7; j < num8; j++)
				{
					Vector2 point;
					point.y = (float)j;
					int k = num5;
					while (k < num6)
					{
						point.x = (float)k;
						Vector2 b;
						float num9 = this.GetPointToLineDistance2(point, this.FinalPathPoints[i], this.FinalPathPoints[i + 1], out b);
						float num10;
						if (num9 < num3)
						{
							num10 = Utils.FastClamp01(Vector2.Distance(this.FinalPathPoints[i], b) / Vector2.Distance(this.FinalPathPoints[i], this.FinalPathPoints[i + 1]));
							goto IL_276;
						}
						num9 = this.distanceSqr((float)k, (float)j, this.FinalPathPoints[i]);
						if (num9 < num3)
						{
							float num11 = this.distanceSqr((float)k, (float)j, this.FinalPathPoints[i + 1]);
							if (num9 <= num11)
							{
								if (i > 0)
								{
									float num12 = this.distanceSqr((float)k, (float)j, this.FinalPathPoints[i - 1]);
									Vector2 vector;
									if (num9 >= num12 || this.GetPointToLineDistance2(point, this.FinalPathPoints[i - 1], this.FinalPathPoints[i], out vector) < num3)
									{
										goto IL_425;
									}
								}
								num10 = -1f;
								goto IL_276;
							}
						}
						IL_425:
						k++;
						continue;
						IL_276:
						int num13 = k + j * this.worldBuilder.WorldSize;
						if (this.isRiver)
						{
							if (num9 <= num2)
							{
								ids[num13] = 4;
								if (this.worldBuilder.GetHeight(k, j) < (float)this.worldBuilder.WaterHeight)
								{
									this.worldBuilder.SetWater(k, j, (byte)this.worldBuilder.WaterHeight);
									goto IL_425;
								}
								this.worldBuilder.SetWater(k, j, (byte)this.worldBuilder.WaterHeight);
							}
						}
						else
						{
							int num14 = (int)ids[num13];
							if (num14 == 2 || (num9 > num2 && (num14 & 128) > 0))
							{
								goto IL_425;
							}
							if (!this.isCountryRoad)
							{
								if (num9 > num2)
								{
									int num15 = num13;
									ids[num15] |= 128;
								}
								else if (num9 > num4)
								{
									ids[num13] = 3;
								}
								else
								{
									ids[num13] = 2;
								}
							}
							else if (num9 <= num2)
							{
								ids[num13] = 1;
							}
						}
						float height = this.worldBuilder.GetHeight(k, j);
						float v = 3f;
						if (!this.isRiver)
						{
							v = (float)(Utils.FastMax((int)this.worldBuilder.GetWater(k, j), this.worldBuilder.WaterHeight) + 1);
						}
						float num16 = this.pathPoints3d[i].y;
						if (num10 > 0f)
						{
							num16 = Utils.FastLerpUnclamped(num16, this.pathPoints3d[i + 1].y, num10);
						}
						num16 = Utils.FastMax(v, num16);
						float num17 = Utils.FastClamp01((Mathf.Sqrt(num9) - this.radius) / (num - this.radius));
						num17 *= num17;
						num16 = Utils.FastLerpUnclamped(num16, height, num17);
						this.worldBuilder.SetHeightTrusted(k, j, num16);
						goto IL_425;
					}
				}
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public float GetPointToLineDistance2(Vector2 point, Vector2 lineStart, Vector2 lineEnd, out Vector2 pointOnLine)
		{
			Vector2 vector;
			vector.x = lineEnd.x - lineStart.x;
			vector.y = lineEnd.y - lineStart.y;
			float num = Mathf.Sqrt(vector.x * vector.x + vector.y * vector.y);
			vector.x /= num;
			vector.y /= num;
			float num2 = Vector2.Dot(point - lineStart, vector);
			if (num2 < 0f || num2 > num)
			{
				pointOnLine = new Vector2(100000f, 100000f);
				return float.MaxValue;
			}
			pointOnLine = lineStart + num2 * vector;
			return this.distanceSqr(point, pointOnLine);
		}

		public bool Crosses(Path path)
		{
			foreach (Vector2 v in this.FinalPathPoints)
			{
				foreach (Vector2 v2 in path.FinalPathPoints)
				{
					if (this.distanceSqr(v, v2) < 100f)
					{
						return true;
					}
				}
			}
			return false;
		}

		public bool IsConnectedTo(Path path)
		{
			if (this.Crosses(path))
			{
				return true;
			}
			foreach (Vector2 v in path.FinalPathPoints)
			{
				if (this.distanceSqr(this.StartPosition.AsVector2(), v) < 100f)
				{
					return true;
				}
				if (this.distanceSqr(this.EndPosition.AsVector2(), v) < 100f)
				{
					return true;
				}
			}
			return false;
		}

		public bool IsConnectedToHighway()
		{
			if (this.worldBuilder.PathingUtils.IsPointOnHighwayWorld(this.StartPosition.x, this.StartPosition.y))
			{
				return true;
			}
			if (this.worldBuilder.PathingUtils.IsPointOnHighwayWorld(this.EndPosition.x, this.EndPosition.y))
			{
				return true;
			}
			foreach (Vector2 vector in this.FinalPathPoints)
			{
				if (this.worldBuilder.PathingUtils.IsPointOnHighwayWorld((int)vector.x, (int)vector.y))
				{
					return true;
				}
			}
			return false;
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public float distanceSqr(Vector2i a, Vector2i b)
		{
			return (float)((a.x - b.x) * (a.x - b.x) + (a.y - b.y) * (a.y - b.y));
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public float distanceSqr(Vector2 v1, Vector2 v2)
		{
			float num = v1.x - v2.x;
			float num2 = v1.y - v2.y;
			return num * num + num2 * num2;
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public float distanceSqr(float x, float y, Vector2 v2)
		{
			float num = x - v2.x;
			float num2 = y - v2.y;
			return num * num + num2 * num2;
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public List<Vector3> romChain(List<Vector3> points, int numberOfPointsOnCurve = 5, float parametricSplineVal = 0.2f)
		{
			List<Vector3> list = new List<Vector3>(points.Count * numberOfPointsOnCurve + 1);
			for (int i = 0; i < points.Count - 3; i++)
			{
				list.AddRange(this.catmulRom(points[i], points[i + 1], points[i + 2], points[i + 3], numberOfPointsOnCurve, parametricSplineVal));
			}
			return list;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public List<Vector3> catmulRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, int numberOfPointsOnCurve, float parametricSplineVal)
		{
			List<Vector3> list = new List<Vector3>(numberOfPointsOnCurve + 1);
			float t = this.getT(0f, p0, p1, parametricSplineVal);
			float t2 = this.getT(t, p1, p2, parametricSplineVal);
			float t3 = this.getT(t2, p2, p3, parametricSplineVal);
			for (float num = t; num < t2; num += (t2 - t) / (float)numberOfPointsOnCurve)
			{
				Vector3 a = (t - num) / (t - 0f) * p0 + (num - 0f) / (t - 0f) * p1;
				Vector3 a2 = (t2 - num) / (t2 - t) * p1 + (num - t) / (t2 - t) * p2;
				Vector3 a3 = (t3 - num) / (t3 - t2) * p2 + (num - t2) / (t3 - t2) * p3;
				Vector3 a4 = (t2 - num) / (t2 - 0f) * a + (num - 0f) / (t2 - 0f) * a2;
				Vector3 a5 = (t3 - num) / (t3 - t) * a2 + (num - t) / (t3 - t) * a3;
				Vector3 item = (t2 - num) / (t2 - t) * a4 + (num - t) / (t2 - t) * a5;
				list.Add(item);
			}
			return list;
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public float getT(float t, Vector3 p0, Vector3 p1, float alpha)
		{
			if (p0 == p1)
			{
				return t;
			}
			return Mathf.Pow((p1 - p0).sqrMagnitude, 0.5f * alpha) + t;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void CreatePath()
		{
			if (this.StartPosition.x < 8 || this.StartPosition.y < 8)
			{
				Log.Error("Start position oob");
				return;
			}
			if (this.EndPosition.x < 8 || this.EndPosition.y < 8)
			{
				Log.Error("End position oob");
				return;
			}
			List<Vector2i> path = this.worldBuilder.PathingUtils.GetPath(this, this.StartPosition, this.EndPosition);
			if (path == null)
			{
				return;
			}
			if (path.Count <= 2)
			{
				return;
			}
			this.IsValid = true;
			if (this.validityTestOnly)
			{
				return;
			}
			this.FinalPathPoints = new List<Vector2>(16);
			Vector2 item = new Vector2((float)this.EndPosition.x, (float)this.EndPosition.y);
			this.FinalPathPoints.Add(item);
			for (int i = 1; i < path.Count; i++)
			{
				if (this.distanceSqr(path[i], path[i - 1]) >= 64f && this.distanceSqr(path[i], this.StartPosition) >= 64f && this.distanceSqr(path[i], this.EndPosition) >= 64f)
				{
					item.x = (float)path[i].x;
					item.y = (float)path[i].y;
					this.FinalPathPoints.Add(item);
				}
			}
			this.FinalPathPoints.Add(new Vector2((float)this.StartPosition.x, (float)this.StartPosition.y));
			this.ProcessPath();
		}

		public void commitPathingMapData()
		{
			for (int i = 0; i < this.FinalPathPoints.Count - 1; i++)
			{
				Vector2i vector2i;
				vector2i.x = (int)this.FinalPathPoints[i].x;
				vector2i.y = (int)this.FinalPathPoints[i].y;
				PathTile pathTileWorld = this.getPathTileWorld(vector2i);
				if (pathTileWorld != null)
				{
					pathTileWorld.TileState = (this.isCountryRoad ? PathTile.PathTileStates.Country : PathTile.PathTileStates.Highway);
					pathTileWorld.PathRadius = (byte)this.radius;
				}
				if (!this.isCountryRoad)
				{
					StreetTile streetTileWorld = this.worldBuilder.GetStreetTileWorld(vector2i);
					if (streetTileWorld != null && !streetTileWorld.ConnectedHighways.Contains(this))
					{
						streetTileWorld.ConnectedHighways.Add(this);
					}
				}
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public PathTile getPathTileWorld(Vector2i worldPosition)
		{
			return this.getPathTileWorld(worldPosition.x, worldPosition.y);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public PathTile getPathTileWorld(int x, int y)
		{
			x /= 10;
			y /= 10;
			if (x < 0 || x >= this.worldBuilder.PathingGrid.GetLength(0))
			{
				return null;
			}
			if (y < 0 || y >= this.worldBuilder.PathingGrid.GetLength(1))
			{
				return null;
			}
			if (this.worldBuilder.PathingGrid[x, y] == null)
			{
				this.worldBuilder.PathingGrid[x, y] = new PathTile();
			}
			return this.worldBuilder.PathingGrid[x, y];
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void roundOffCorners()
		{
			for (int i = 2; i < this.FinalPathPoints.Count - 2; i++)
			{
				Vector2 normalized = (this.FinalPathPoints[i] - this.FinalPathPoints[i - 1]).normalized;
				Vector2 normalized2 = (this.FinalPathPoints[i + 1] - this.FinalPathPoints[i]).normalized;
				if (normalized != normalized2)
				{
					this.FinalPathPoints[i] = (this.FinalPathPoints[i] + this.FinalPathPoints[i - 1] * 0.5f + this.FinalPathPoints[i + 1] * 0.5f) / 2f;
				}
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void SmoothHeights(float[] heights)
		{
			for (int i = 0; i < this.pathPoints3d.Count; i++)
			{
				heights[i] = this.pathPoints3d[i].y;
			}
			for (int j = 1; j < this.pathPoints3d.Count - 1; j++)
			{
				Vector3 value = this.pathPoints3d[j];
				value.y = (heights[j - 1] + heights[j] + heights[j + 1]) * 0.333333343f;
				this.pathPoints3d[j] = value;
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public const float cSingleLaneRadius = 4.5f;

		[PublicizedFrom(EAccessModifier.Private)]
		public const int cShoulderWidth = 1;

		[PublicizedFrom(EAccessModifier.Private)]
		public const float cBlendDistCountry = 6f;

		[PublicizedFrom(EAccessModifier.Private)]
		public const float cBlendDistHighway = 10f;

		[PublicizedFrom(EAccessModifier.Private)]
		public const float cHeightSmoothAverageBias = 8f;

		[PublicizedFrom(EAccessModifier.Private)]
		public const float cHeightSmoothDecreasePer = 0.3f;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly WorldBuilder worldBuilder;

		public bool IsPrefabPath;

		[PublicizedFrom(EAccessModifier.Private)]
		public int lanes;

		public float radius = 8f;

		public bool isCountryRoad;

		public bool isRiver;

		public bool connectsToHighway;

		public readonly Vector2i StartPosition;

		public readonly Vector2i EndPosition;

		public bool IsValid;

		public int StartPointID = -1;

		public int EndPointID = -1;

		public int Cost;

		public List<Vector2> FinalPathPoints = new List<Vector2>();

		public List<Vector3> pathPoints3d = new List<Vector3>();

		[PublicizedFrom(EAccessModifier.Private)]
		public bool validityTestOnly;

		[PublicizedFrom(EAccessModifier.Private)]
		public const int FreeId = 0;

		[PublicizedFrom(EAccessModifier.Private)]
		public const int CountryId = 1;

		[PublicizedFrom(EAccessModifier.Private)]
		public const int HighwayId = 2;

		[PublicizedFrom(EAccessModifier.Private)]
		public const int HighwayDirtId = 3;

		[PublicizedFrom(EAccessModifier.Private)]
		public const int WaterId = 4;

		[PublicizedFrom(EAccessModifier.Private)]
		public const int HighwayBlendIdMask = 128;
	}
}
