using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UniLinq;
using UnityEngine;

namespace WorldGenerationEngineFinal
{
	public class StreetTile
	{
		public int GroupID
		{
			get
			{
				if (this.Township == null)
				{
					return -1;
				}
				return this.Township.ID;
			}
		}

		public bool IsValidForStreetTile
		{
			get
			{
				return !this.OverlapsBiomes && !this.OverlapsWater && !this.OverlapsRadiation && !this.HasSteepSlope && this.TerrainType != TerrainType.mountains;
			}
		}

		public bool IsValidForGateway
		{
			get
			{
				return !this.OverlapsBiomes && !this.OverlapsWater && !this.OverlapsRadiation && !this.HasSteepSlope && this.TerrainType != TerrainType.mountains && !this.HasPrefabs;
			}
		}

		public bool IsBlocked
		{
			get
			{
				return this.AllIsWater || this.OverlapsWater || this.OverlapsRadiation || this.HasSteepSlope || this.TerrainType == TerrainType.mountains;
			}
		}

		public bool HasPrefabs
		{
			get
			{
				return this.StreetTilePrefabDatas != null && this.StreetTilePrefabDatas.Count > 0;
			}
		}

		public bool HasStreetTilePrefab
		{
			get
			{
				return this.Township != null && this.District != null && this.HasPrefabs;
			}
		}

		public Vector2i WorldPositionCenter
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				return this.WorldPosition + new Vector2i(75, 75);
			}
		}

		public Vector2i WorldPositionMax
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				return this.WorldPosition + new Vector2i(150, 150);
			}
		}

		public BiomeType BiomeType
		{
			get
			{
				return this.worldBuilder.GetBiome(this.WorldPositionCenter);
			}
		}

		public float PositionHeight
		{
			get
			{
				return this.worldBuilder.GetHeight(this.WorldPositionCenter);
			}
		}

		public TerrainType TerrainType
		{
			get
			{
				return this.worldBuilder.GetTerrainType(this.WorldPositionCenter);
			}
		}

		public int RoadExitCount
		{
			get
			{
				int num = 0;
				for (int i = 0; i < this.RoadExits.Length; i++)
				{
					if (this.RoadExits[i])
					{
						num++;
					}
				}
				return num;
			}
		}

		public bool[] RoadExits
		{
			get
			{
				return this.worldBuilder.StreetTileShared.RoadShapeExitsPerRotation[this.RoadShape][(int)this.Rotations];
			}
		}

		public string PrefabName
		{
			get
			{
				return this.worldBuilder.StreetTileShared.RoadShapesDistrict[this.RoadShape];
			}
		}

		public StreetTile.PrefabRotations Rotations
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				return this.rotations;
			}
			set
			{
				if (value != this.rotations || this.transData == null)
				{
					this.rotations = value;
					if (this.transData != null)
					{
						this.transData.rotation = (int)(this.rotations * (StreetTile.PrefabRotations)(-90));
						return;
					}
					this.transData = new TranslationData(this.WorldPositionCenter.x, this.WorldPositionCenter.y, 1f, (int)(this.rotations * (StreetTile.PrefabRotations)(-90)));
				}
			}
		}

		public Vector2i getHighwayExitPositionByDirection(Vector2i dir)
		{
			for (int i = 0; i < 4; i++)
			{
				if (dir == this.worldBuilder.StreetTileShared.dir4way[i])
				{
					return this.getHighwayExitPosition(i);
				}
			}
			return Vector2i.zero;
		}

		public Vector2i getHighwayExitPosition(int index)
		{
			if (this.highwayExits.Count == 0)
			{
				this.getAllHighwayExits();
			}
			return this.highwayExits[index];
		}

		public List<Vector2i> getAllHighwayExits()
		{
			if (this.highwayExits.Count == 0)
			{
				this.highwayExits.Add(this.highwayExitFromIndex(0));
				this.highwayExits.Add(this.highwayExitFromIndex(1));
				this.highwayExits.Add(this.highwayExitFromIndex(2));
				this.highwayExits.Add(this.highwayExitFromIndex(3));
			}
			return this.highwayExits;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public Vector2i highwayExitFromIndex(int index)
		{
			Vector2i result;
			if (index == 0)
			{
				result.x = this.WorldPositionCenter.x;
				result.y = this.WorldPositionMax.y - 1;
				return result;
			}
			if (index == 1)
			{
				result.x = this.WorldPositionMax.x - 1;
				result.y = this.WorldPositionCenter.y;
				return result;
			}
			if (index == 2)
			{
				result.x = this.WorldPositionCenter.x;
				result.y = this.WorldPosition.y;
				return result;
			}
			result.x = this.WorldPosition.x;
			result.y = this.WorldPositionCenter.y;
			return result;
		}

		public void SetAllExistingNeighborsForGateway()
		{
			for (int i = 0; i < 4; i++)
			{
				if (this.Township.Streets.ContainsKey(this.GridPosition + this.worldBuilder.StreetTileShared.dir4way[i]))
				{
					this.SetExitUsed(this.getHighwayExitPosition(i));
				}
			}
		}

		public List<Vector2i> GetHighwayExits(bool isGateway = false)
		{
			List<Vector2i> list = new List<Vector2i>();
			if (!isGateway)
			{
				for (int i = 0; i < 4; i++)
				{
					if (this.Township.Streets.ContainsKey(this.GridPosition + this.worldBuilder.StreetTileShared.dir4way[i]))
					{
						this.UsedExitList.Add(this.getHighwayExitPosition(i));
						this.ConnectedExits |= 1 << i;
					}
				}
			}
			if (this.UsedExitList.Count == 1)
			{
				int num = -1;
				for (int j = 0; j < 4; j++)
				{
					if ((this.ConnectedExits & 1 << j) > 0)
					{
						num = (j + 2 & 3);
						break;
					}
				}
				if (num != -1)
				{
					list.Add(this.getHighwayExitPosition(num));
				}
				else
				{
					Log.Error("Could not find opposite highway exit!");
				}
			}
			else
			{
				for (int k = 0; k < 4; k++)
				{
					if ((this.ConnectedExits & 1 << k) <= 0 && (isGateway || this.RoadExits[k]))
					{
						list.Add(this.getHighwayExitPosition(k));
					}
				}
			}
			return list;
		}

		public List<Vector2i> GetAllHighwayExits()
		{
			List<Vector2i> list = new List<Vector2i>();
			for (int i = 0; i < this.RoadExits.Length; i++)
			{
				list.Add(this.getHighwayExitPosition(i));
			}
			return list;
		}

		public bool HasExits()
		{
			for (int i = 0; i < this.RoadExits.Length; i++)
			{
				if (this.RoadExits[i])
				{
					return true;
				}
			}
			return false;
		}

		public int GetExistingExitCount()
		{
			int num = 0;
			for (int i = 0; i < this.RoadExits.Length; i++)
			{
				if (this.RoadExits[i])
				{
					num++;
				}
			}
			return num;
		}

		public StreetTile(WorldBuilder _worldBuilder, Vector2i gridPosition)
		{
			this.worldBuilder = _worldBuilder;
			this.GridPosition = gridPosition;
			this.WorldPosition = this.GridPosition * 150;
			this.Area = new Rect(new Vector2((float)this.WorldPosition.x, (float)this.WorldPosition.y), Vector2.one * 150f);
			GameRandom gameRandom = GameRandomManager.Instance.CreateGameRandom(this.worldBuilder.Seed + this.WorldPosition.ToString().GetHashCode());
			this.Rotations = (gameRandom.RandomRange(0, 4) + StreetTile.PrefabRotations.One & StreetTile.PrefabRotations.Three);
			GameRandomManager.Instance.FreeGameRandom(gameRandom);
			this.RoadShape = 2;
			if (this.GridPosition.x < 1 || this.GridPosition.x >= this.worldBuilder.StreetTileMapSize - 1)
			{
				this.OverlapsRadiation = true;
			}
			if (this.GridPosition.y < 1 || this.GridPosition.y >= this.worldBuilder.StreetTileMapSize - 1)
			{
				this.OverlapsRadiation = true;
			}
			float positionHeight = this.PositionHeight;
			for (int i = 0; i < this.worldBuilder.StreetTileShared.dir9way.Length; i++)
			{
				Vector2i vector2i = this.WorldPositionCenter + this.worldBuilder.StreetTileShared.dir9way[i] * 75;
				if (this.worldBuilder.GetRad(vector2i.x, vector2i.y) > 0)
				{
					this.OverlapsRadiation = true;
				}
				if (Utils.FastAbs(this.worldBuilder.GetHeight(vector2i.x, vector2i.y) - positionHeight) > 10f)
				{
					this.HasSteepSlope = true;
				}
			}
			BiomeType biomeType = this.BiomeType;
			int num = 0;
			int num2 = 0;
			Vector2i worldPositionMax = this.WorldPositionMax;
			for (int j = this.WorldPosition.y; j < worldPositionMax.y; j += 3)
			{
				for (int k = this.WorldPosition.x; k < worldPositionMax.x; k += 3)
				{
					num++;
					if (biomeType != this.worldBuilder.GetBiome(k, j))
					{
						this.OverlapsBiomes = true;
					}
					if (this.worldBuilder.GetWater(k, j) > 0)
					{
						num2++;
						this.OverlapsWater = true;
					}
				}
			}
			if ((float)num2 / (float)num > 0.9f)
			{
				this.AllIsWater = true;
			}
		}

		public void UpdateValidity()
		{
			float positionHeight = this.PositionHeight;
			foreach (Vector2i a in this.worldBuilder.StreetTileShared.dir9way)
			{
				Vector2i vector2i = this.WorldPositionCenter + a * 75;
				if (this.worldBuilder.GetRad(vector2i.x, vector2i.y) > 0)
				{
					this.OverlapsRadiation = true;
				}
				if (Utils.FastAbs(this.worldBuilder.GetHeight(vector2i.x, vector2i.y) - positionHeight) > 10f)
				{
					this.HasSteepSlope = true;
				}
			}
			BiomeType biomeType = this.BiomeType;
			Vector2i worldPositionMax = this.WorldPositionMax;
			for (int j = this.WorldPosition.y; j < worldPositionMax.y; j += 3)
			{
				for (int k = this.WorldPosition.x; k < worldPositionMax.x; k += 3)
				{
					if (biomeType != this.worldBuilder.GetBiome(k, j))
					{
						this.OverlapsBiomes = true;
					}
					if (this.worldBuilder.GetWater(k, j) > 0)
					{
						this.OverlapsWater = true;
					}
				}
			}
		}

		public Stamp[] GetStamps()
		{
			return new Stamp[]
			{
				new Stamp(this.worldBuilder, this.worldBuilder.StampManager.GetStamp(this.worldBuilder.StreetTileShared.RoadShapes[this.RoadShape], null), this.transData, true, new Color(1f, 0f, 0f, 0f), 0.1f, false, "")
			};
		}

		public StreetTile[] GetNeighbors()
		{
			if (this.neighbors == null)
			{
				this.neighbors = new StreetTile[4];
				for (int i = 0; i < this.worldBuilder.StreetTileShared.dir4way.Length; i++)
				{
					this.neighbors[i] = this.GetNeighbor(this.worldBuilder.StreetTileShared.dir4way[i]);
				}
			}
			return this.neighbors;
		}

		public int GetNeighborCount()
		{
			int num = 0;
			for (int i = 0; i < this.worldBuilder.StreetTileShared.dir4way.Length; i++)
			{
				if (this.GetNeighbor(this.worldBuilder.StreetTileShared.dir4way[i]) != null)
				{
					num++;
				}
			}
			return num;
		}

		public StreetTile[] GetNeighbors8way()
		{
			if (this.neighbors == null)
			{
				this.neighbors = new StreetTile[8];
				for (int i = 0; i < this.worldBuilder.StreetTileShared.dir8way.Length; i++)
				{
					this.neighbors[i] = this.GetNeighbor(this.worldBuilder.StreetTileShared.dir8way[i]);
				}
			}
			return this.neighbors;
		}

		public StreetTile GetNeighbor(Vector2i direction)
		{
			return this.worldBuilder.GetStreetTileGrid(this.GridPosition + direction);
		}

		public bool HasNeighbor(StreetTile otherTile)
		{
			StreetTile[] array = this.GetNeighbors();
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i] == otherTile)
				{
					return true;
				}
			}
			return false;
		}

		public StreetTile GetNeighborByIndex(int idx)
		{
			if (this.neighbors == null)
			{
				this.GetNeighbors();
			}
			if (idx < 0 || idx >= this.neighbors.Length)
			{
				return null;
			}
			return this.neighbors[idx];
		}

		public int GetNeighborIndex(StreetTile otherTile)
		{
			if (this.neighbors == null)
			{
				this.GetNeighbors();
			}
			for (int i = 0; i < this.neighbors.Length; i++)
			{
				if (this.neighbors[i] == otherTile)
				{
					return i;
				}
			}
			return -1;
		}

		public bool HasExitTo(StreetTile otherTile)
		{
			int neighborIndex;
			return (this.Township != null || this.District != null) && (otherTile.Township != null || otherTile.District != null) && (neighborIndex = this.GetNeighborIndex(otherTile)) >= 0 && neighborIndex < this.RoadExits.Length && this.RoadExits[neighborIndex];
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public int vectorToRotation(Vector2i direction)
		{
			for (int i = 0; i < this.worldBuilder.StreetTileShared.dir4way.Length; i++)
			{
				if (this.worldBuilder.StreetTileShared.dir4way[i] == direction)
				{
					return i;
				}
			}
			return -1;
		}

		public void SetPathingConstraintsForTile(bool allBlocked = false)
		{
			if (this.Township != null && this.District != null)
			{
				this.worldBuilder.PathingUtils.AddFullyBlockedArea(this.Area);
				this.isFullyBlocked = true;
				return;
			}
			if (allBlocked && !this.isFullyBlocked && !this.isPartBlocked)
			{
				this.worldBuilder.PathingUtils.AddFullyBlockedArea(this.Area);
				this.isFullyBlocked = true;
				return;
			}
			if (!allBlocked)
			{
				if (this.isFullyBlocked)
				{
					this.worldBuilder.PathingUtils.RemoveFullyBlockedArea(this.Area);
				}
				this.worldBuilder.PathingUtils.AddMoveLimitArea(this.Area);
				this.isPartBlocked = true;
			}
		}

		public void SetRoadExit(int dir, bool value)
		{
			if ((ulong)dir >= (ulong)((long)this.RoadExits.Length))
			{
				return;
			}
			bool[] array = (bool[])this.RoadExits.Clone();
			array[dir] = value;
			this.SetRoadExits(array);
		}

		public void SetRoadExits(bool _north, bool _east, bool _south, bool _west)
		{
			this.SetRoadExits(new bool[]
			{
				_north,
				_east,
				_south,
				_west
			});
			StreetTile neighbor = this.GetNeighbor(Vector2i.right);
			if (neighbor != null)
			{
				neighbor.SetPathingConstraintsForTile(!_east);
			}
			StreetTile neighbor2 = this.GetNeighbor(Vector2i.left);
			if (neighbor2 != null)
			{
				neighbor2.SetPathingConstraintsForTile(!_west);
			}
			StreetTile neighbor3 = this.GetNeighbor(Vector2i.up);
			if (neighbor3 != null)
			{
				neighbor3.SetPathingConstraintsForTile(!_north);
			}
			StreetTile neighbor4 = this.GetNeighbor(Vector2i.down);
			if (neighbor4 != null)
			{
				neighbor4.SetPathingConstraintsForTile(!_south);
			}
		}

		public void SetRoadExits(bool[] _exits)
		{
			StreetTile.PrefabRotations prefabRotations = this.Rotations;
			int roadShape = this.RoadShape;
			for (int i = 0; i < this.worldBuilder.StreetTileShared.RoadShapeExitCounts.Count; i++)
			{
				this.RoadShape = i;
				for (int j = 0; j < 4; j++)
				{
					this.Rotations = (StreetTile.PrefabRotations)j;
					if (_exits.SequenceEqual(this.RoadExits))
					{
						return;
					}
				}
			}
			this.Rotations = prefabRotations;
			this.RoadShape = roadShape;
		}

		public bool SetExitUsed(Vector2i exit)
		{
			for (int i = 0; i < this.RoadExits.Length; i++)
			{
				Vector2i highwayExitPosition = this.getHighwayExitPosition(i);
				if (highwayExitPosition == exit)
				{
					this.SetRoadExit(i, true);
					this.ConnectedExits |= 1 << i;
					if (!this.UsedExitList.Contains(highwayExitPosition))
					{
						this.UsedExitList.Add(highwayExitPosition);
					}
					return true;
				}
			}
			return false;
		}

		public void SetExitUnUsed(Vector2i exit)
		{
			for (int i = 0; i < this.RoadExits.Length; i++)
			{
				Vector2i highwayExitPosition = this.getHighwayExitPosition(i);
				if (highwayExitPosition == exit)
				{
					this.SetRoadExit(i, false);
					this.ConnectedExits &= ~(1 << i);
					this.UsedExitList.Remove(highwayExitPosition);
					return;
				}
			}
		}

		public bool ContainsHighway
		{
			get
			{
				return this.ConnectedHighways.Count > 0;
			}
		}

		public bool SpawnPrefabs()
		{
			if (this.District == null || this.District.name == "wilderness")
			{
				if (!this.ContainsHighway)
				{
					this.District = DistrictPlannerStatic.Districts["wilderness"];
					if (this.spawnWildernessPrefab())
					{
						return true;
					}
				}
				this.District = null;
				return false;
			}
			string streetPrefabName = string.Format(this.PrefabName, this.District.prefabName);
			this.spawnStreetTile(this.WorldPosition, streetPrefabName, (int)this.Rotations);
			return true;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public bool spawnStreetTile(Vector2i tileMinPositionWorld, string streetPrefabName, int baseRotations)
		{
			bool useExactString = false;
			PrefabData streetTile = this.worldBuilder.PrefabManager.GetStreetTile(streetPrefabName, this.WorldPositionCenter, useExactString);
			if (streetTile == null && string.Format(this.PrefabName, "") != streetPrefabName)
			{
				streetTile = this.worldBuilder.PrefabManager.GetStreetTile(string.Format(this.PrefabName, ""), this.WorldPositionCenter, useExactString);
			}
			if (streetTile == null)
			{
				return false;
			}
			if (this.worldBuilder.TownshipShared.Height + streetTile.yOffset < 3)
			{
				return false;
			}
			int num = baseRotations + (int)streetTile.RotationsToNorth & 3;
			if (num == 1)
			{
				num = 3;
			}
			else if (num == 3)
			{
				num = 1;
			}
			Vector3i position = new Vector3i(tileMinPositionWorld.x, this.worldBuilder.TownshipShared.Height + streetTile.yOffset + 1, tileMinPositionWorld.y) + this.worldBuilder.PrefabWorldOffset;
			int num2;
			if (this.worldBuilder.PrefabManager.StreetTilesUsed.TryGetValue(streetTile.Name, out num2))
			{
				this.worldBuilder.PrefabManager.StreetTilesUsed[streetTile.Name] = num2 + 1;
			}
			else
			{
				this.worldBuilder.PrefabManager.StreetTilesUsed.Add(streetTile.Name, 1);
			}
			float totalDensityLeft = 62f;
			float num3;
			if (PrefabManagerStatic.TileMaxDensityScore.TryGetValue(streetTile.Name, out num3))
			{
				totalDensityLeft = num3;
			}
			PrefabManager prefabManager = this.worldBuilder.PrefabManager;
			int prefabInstanceId = prefabManager.PrefabInstanceId;
			prefabManager.PrefabInstanceId = prefabInstanceId + 1;
			this.AddPrefab(new PrefabDataInstance(prefabInstanceId, position, (byte)num, streetTile));
			this.SpawnMarkerPartsAndPrefabs(streetTile, new Vector3i(this.WorldPosition.x, this.worldBuilder.TownshipShared.Height + streetTile.yOffset + 1, this.WorldPosition.y), num, 0, totalDensityLeft);
			this.smoothAround = true;
			return true;
		}

		public void SmoothWildernessTerrain()
		{
			this.SmoothTerrainBox(this.WildernessPOICenter, this.WildernessPOISize, this.WildernessPOIHeight);
		}

		public void SmoothTerrainPost()
		{
			if (this.smoothAround || (this.Township != null && this.District != null))
			{
				this.SmoothTerrainBox(this.WorldPositionCenter, 150, this.worldBuilder.TownshipShared.Height);
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void SmoothTerrainBox(Vector2i _centerPos, int _size, int _height)
		{
			int num = _size / 2 + 2;
			int num2 = (int)((float)num * 2.2f);
			float num3 = (float)num2;
			int num4 = _centerPos.x - num;
			int num5 = _centerPos.x + num;
			int num6 = _centerPos.y - num;
			int num7 = _centerPos.y + num;
			int num8 = Utils.FastMax(num4 - num2, 1);
			int num9 = Utils.FastMax(num6 - num2, 1);
			int num10 = Utils.FastMin(num5 + num2, this.worldBuilder.WorldSize);
			int num11 = Utils.FastMin(num7 + num2, this.worldBuilder.WorldSize);
			for (int i = num9; i < num11; i++)
			{
				bool flag = i >= num6 && i <= num7;
				for (int j = num8; j < num10; j++)
				{
					bool flag2 = j >= num4 && j <= num5;
					if (flag2 && flag)
					{
						this.worldBuilder.SetHeightTrusted(j, i, (float)_height);
					}
					else
					{
						int x = flag2 ? j : ((j < _centerPos.x) ? num4 : num5);
						int y = flag ? i : ((i < _centerPos.y) ? num6 : num7);
						float num12 = Mathf.Sqrt((float)this.distanceSqr(j, i, x, y)) / num3;
						if (num12 < 1f)
						{
							float height = this.worldBuilder.GetHeight(j, i);
							this.worldBuilder.SetHeightTrusted(j, i, StreetTile.SmoothStep((float)_height, height, (double)num12));
						}
					}
				}
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void SmoothTerrainCircle(Vector2i _centerPos, int _size, int _height)
		{
			int num = _size / 2;
			float num2 = (float)num * 1.8f;
			int num3 = Mathf.CeilToInt(num2 * num2);
			int num4 = (int)((float)num * 3.2f);
			float num5 = (float)num4 - num2;
			int num6 = Utils.FastMax(_centerPos.x - num4, 1);
			int num7 = Utils.FastMax(_centerPos.y - num4, 1);
			int num8 = Utils.FastMin(_centerPos.x + num4, this.worldBuilder.WorldSize);
			int num9 = Utils.FastMin(_centerPos.y + num4, this.worldBuilder.WorldSize);
			for (int i = num7; i < num9; i++)
			{
				for (int j = num6; j < num8; j++)
				{
					int num10 = this.distanceSqr(j, i, _centerPos.x, _centerPos.y);
					if (num10 <= num3)
					{
						this.worldBuilder.SetHeightTrusted(j, i, (float)_height);
					}
					else
					{
						float num11 = (Mathf.Sqrt((float)num10) - num2) / num5;
						if (num11 < 1f)
						{
							float height = this.worldBuilder.GetHeight(j, i);
							this.worldBuilder.SetHeightTrusted(j, i, StreetTile.SmoothStep((float)_height, height, (double)num11));
						}
					}
				}
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static float SmoothStep(float from, float to, double t)
		{
			t = -2.0 * t * t * t + 3.0 * t * t;
			return (float)((double)to * t + (double)from * (1.0 - t));
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public bool spawnWildernessPrefab()
		{
			GameRandom gameRandom = GameRandomManager.Instance.CreateGameRandom(this.worldBuilder.Seed + 4096953);
			FastTags<TagGroup.Poi> fastTags = (this.worldBuilder.Towns == WorldBuilder.GenerationSelections.None) ? FastTags<TagGroup.Poi>.none : this.worldBuilder.StreetTileShared.traderTag;
			PrefabManager prefabManager = this.worldBuilder.PrefabManager;
			FastTags<TagGroup.Poi> withoutTags = fastTags;
			FastTags<TagGroup.Poi> none = FastTags<TagGroup.Poi>.none;
			Vector2i worldPositionCenter = this.WorldPositionCenter;
			PrefabData wildernessPrefab = prefabManager.GetWildernessPrefab(withoutTags, none, default(Vector2i), default(Vector2i), worldPositionCenter, false);
			int num = -1;
			int num2;
			int num3;
			int num4;
			Vector2i vector2i;
			Rect rect;
			int num6;
			for (;;)
			{
				IL_79:
				num++;
				if (num >= 6)
				{
					break;
				}
				num2 = ((int)wildernessPrefab.RotationsToNorth + gameRandom.RandomRange(0, 4) & 3);
				num3 = wildernessPrefab.size.x;
				num4 = wildernessPrefab.size.z;
				if (num2 == 1 || num2 == 3)
				{
					num3 = wildernessPrefab.size.z;
					num4 = wildernessPrefab.size.x;
				}
				if (num3 > 150 || num4 > 150)
				{
					vector2i = this.WorldPositionCenter - new Vector2i((num3 - 150) / 2, (num4 - 150) / 2);
				}
				else
				{
					try
					{
						vector2i = new Vector2i(gameRandom.RandomRange(this.WorldPosition.x + 10, this.WorldPosition.x + 150 - num3 - 10), gameRandom.RandomRange(this.WorldPosition.y + 10, this.WorldPosition.y + 150 - num4 - 10));
					}
					catch
					{
						vector2i = this.WorldPositionCenter - new Vector2i(num3 / 2, num4 / 2);
					}
				}
				int num5 = (num3 > num4) ? num3 : num4;
				rect = new Rect((float)vector2i.x, (float)vector2i.y, (float)num5, (float)num5);
				new Rect(rect.min - new Vector2((float)num5, (float)num5) / 2f, rect.size + new Vector2((float)num5, (float)num5));
				Rect rect2 = new Rect(rect.min - new Vector2((float)num5, (float)num5) / 2f, rect.size + new Vector2((float)num5, (float)num5));
				rect2.center = new Vector2((float)(vector2i.x + num4 / 2), (float)(vector2i.y + num3 / 2));
				if (rect2.max.x < (float)this.worldBuilder.WorldSize && rect2.min.x >= 0f && rect2.max.y < (float)this.worldBuilder.WorldSize && rect2.min.y >= 0f)
				{
					BiomeType biome = this.worldBuilder.GetBiome((int)rect.center.x, (int)rect.center.y);
					num6 = Mathf.CeilToInt(this.worldBuilder.GetHeight((int)rect.center.x, (int)rect.center.y));
					List<int> list = new List<int>();
					for (int i = vector2i.x; i < vector2i.x + num3; i++)
					{
						for (int j = vector2i.y; j < vector2i.y + num4; j++)
						{
							if (i >= this.worldBuilder.WorldSize || i < 0 || j >= this.worldBuilder.WorldSize || j < 0 || this.worldBuilder.GetWater(i, j) > 0 || biome != this.worldBuilder.GetBiome(i, j) || Mathf.Abs(Mathf.CeilToInt(this.worldBuilder.GetHeight(i, j)) - num6) > 11)
							{
								goto IL_79;
							}
							list.Add((int)this.worldBuilder.GetHeight(i, j));
						}
					}
					num6 = this.getMedianHeight(list);
					if (num6 + wildernessPrefab.yOffset >= 2)
					{
						goto Block_20;
					}
				}
			}
			GameRandomManager.Instance.FreeGameRandom(gameRandom);
			return false;
			Block_20:
			Vector3i vector3i = new Vector3i(this.subHalfWorld(vector2i.x), this.getHeightCeil(rect.center) + wildernessPrefab.yOffset + 1, this.subHalfWorld(vector2i.y));
			Vector3i vector3i2 = new Vector3i(this.subHalfWorld(vector2i.x), this.getHeightCeil(rect.center), this.subHalfWorld(vector2i.y));
			PrefabManager prefabManager2 = this.worldBuilder.PrefabManager;
			int prefabInstanceId = prefabManager2.PrefabInstanceId;
			prefabManager2.PrefabInstanceId = prefabInstanceId + 1;
			int num7 = prefabInstanceId;
			gameRandom.SetSeed(vector2i.x + vector2i.x * vector2i.y + vector2i.y);
			num2 = gameRandom.RandomRange(0, 4);
			num2 = (num2 + (int)wildernessPrefab.RotationsToNorth & 3);
			Vector2 vector = new Vector2((float)(vector2i.x + num3 / 2), (float)(vector2i.y + num4 / 2));
			if (num2 == 0)
			{
				vector = new Vector2((float)(vector2i.x + num3 / 2), (float)vector2i.y);
			}
			else if (num2 == 1)
			{
				vector = new Vector2((float)(vector2i.x + num3), (float)(vector2i.y + num4 / 2));
			}
			else if (num2 == 2)
			{
				vector = new Vector2((float)(vector2i.x + num3 / 2), (float)(vector2i.y + num4));
			}
			else if (num2 == 3)
			{
				vector = new Vector2((float)vector2i.x, (float)(vector2i.y + num4 / 2));
			}
			float num8 = 0f;
			if (wildernessPrefab.POIMarkers != null)
			{
				List<Prefab.Marker> list2 = wildernessPrefab.RotatePOIMarkers(true, num2);
				for (int k = list2.Count - 1; k >= 0; k--)
				{
					if (list2[k].MarkerType != Prefab.Marker.MarkerTypes.RoadExit)
					{
						list2.RemoveAt(k);
					}
				}
				if (list2.Count > 0)
				{
					int index = gameRandom.RandomRange(0, list2.Count);
					Vector3i start = list2[index].Start;
					int num9 = (list2[index].Size.x > list2[index].Size.z) ? list2[index].Size.x : list2[index].Size.z;
					num8 = Mathf.Max(num8, (float)num9 / 2f);
					string groupName = list2[index].GroupName;
					Vector2 vector2 = new Vector2((float)start.x + (float)list2[index].Size.x / 2f, (float)start.z + (float)list2[index].Size.z / 2f);
					vector = new Vector2((float)vector2i.x + vector2.x, (float)vector2i.y + vector2.y);
					Vector2 vector3 = vector;
					bool isPrefabPath = false;
					if (list2.Count > 1)
					{
						list2 = wildernessPrefab.POIMarkers.FindAll((Prefab.Marker m) => m.MarkerType == Prefab.Marker.MarkerTypes.RoadExit && m.Start != start && m.GroupName == groupName);
						if (list2.Count > 0)
						{
							index = gameRandom.RandomRange(0, list2.Count);
							vector3 = new Vector2((float)(vector2i.x + list2[index].Start.x) + (float)list2[index].Size.x / 2f, (float)(vector2i.y + list2[index].Start.z) + (float)list2[index].Size.z / 2f);
						}
						isPrefabPath = true;
					}
					Path path = new Path(this.worldBuilder, true, num8, false);
					path.FinalPathPoints.Add(new Vector2(vector.x, vector.y));
					path.pathPoints3d.Add(new Vector3(vector.x, (float)vector3i2.y, vector.y));
					path.FinalPathPoints.Add(new Vector2(vector3.x, vector3.y));
					path.pathPoints3d.Add(new Vector3(vector3.x, (float)vector3i2.y, vector3.y));
					path.IsPrefabPath = isPrefabPath;
					path.StartPointID = num7;
					path.EndPointID = num7;
					this.worldBuilder.wildernessPaths.Add(path);
				}
			}
			this.SpawnMarkerPartsAndPrefabsWilderness(wildernessPrefab, new Vector3i(vector2i.x, Mathf.CeilToInt((float)(num6 + wildernessPrefab.yOffset + 1)), vector2i.y), (int)((byte)num2));
			PrefabDataInstance pdi = new PrefabDataInstance(num7, new Vector3i(vector3i.x, num6 + wildernessPrefab.yOffset + 1, vector3i.z), (byte)num2, wildernessPrefab);
			this.AddPrefab(pdi);
			this.worldBuilder.WildernessPrefabCount++;
			if (num6 != this.getHeightCeil(rect.min.x, rect.min.y) || num6 != this.getHeightCeil(rect.max.x, rect.min.y) || num6 != this.getHeightCeil(rect.min.x, rect.max.y) || num6 != this.getHeightCeil(rect.max.x, rect.max.y))
			{
				this.WildernessPOICenter = new Vector2i(rect.center);
				this.WildernessPOISize = Mathf.RoundToInt(Mathf.Max(rect.size.x, rect.size.y));
				this.WildernessPOIHeight = num6;
			}
			if (num8 != 0f)
			{
				this.worldBuilder.WildernessPlanner.WildernessPathInfos.Add(new WorldBuilder.WildernessPathInfo(new Vector2i(vector), num7, num8, this.worldBuilder.GetBiome((int)vector.x, (int)vector.y), 0, null));
			}
			int num10 = Mathf.FloorToInt(rect.x / 10f) - 1;
			int num11 = Mathf.CeilToInt(rect.xMax / 10f) + 1;
			int num12 = Mathf.FloorToInt(rect.y / 10f) - 1;
			int num13 = Mathf.CeilToInt(rect.yMax / 10f) + 1;
			for (int l = num10; l < num11; l++)
			{
				for (int m2 = num12; m2 < num13; m2++)
				{
					if (l >= 0 && l < this.worldBuilder.PathingGrid.GetLength(0) && m2 >= 0 && m2 < this.worldBuilder.PathingGrid.GetLength(1))
					{
						if (l == num10 || l == num11 - 1 || m2 == num12 || m2 == num13 - 1)
						{
							this.worldBuilder.PathingUtils.SetPathBlocked(l, m2, 2);
						}
						else
						{
							this.worldBuilder.PathingUtils.SetPathBlocked(l, m2, true);
						}
					}
				}
			}
			num10 = Mathf.FloorToInt(rect.x) - 1;
			num11 = Mathf.CeilToInt(rect.xMax) + 1;
			num12 = Mathf.FloorToInt(rect.y) - 1;
			num13 = Mathf.CeilToInt(rect.yMax) + 1;
			for (int n = num10; n < num11; n += 150)
			{
				for (int num14 = num12; num14 < num13; num14 += 150)
				{
					StreetTile streetTileWorld = this.worldBuilder.GetStreetTileWorld(n, num14);
					if (streetTileWorld != null)
					{
						streetTileWorld.Used = true;
					}
				}
			}
			GameRandomManager.Instance.FreeGameRandom(gameRandom);
			return true;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void AddPrefab(PrefabDataInstance pdi)
		{
			this.StreetTilePrefabDatas.Add(pdi);
			if (this.Township != null)
			{
				this.Township.AddPrefab(pdi);
				return;
			}
			this.worldBuilder.PrefabManager.AddUsedPrefabWorld(-1, pdi);
		}

		public bool NeedsWildernessSmoothing
		{
			get
			{
				return this.WildernessPOISize > 0;
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public int getMedianHeight(List<int> heights)
		{
			heights.Sort();
			int count = heights.Count;
			int num = count / 2;
			if (count % 2 == 0)
			{
				return (heights[num] + heights[num - 1]) / 2;
			}
			return heights[num];
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public int getAverageHeight(List<int> heights)
		{
			int num = 0;
			foreach (int num2 in heights)
			{
				num += num2;
			}
			return num / heights.Count;
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
						float num2 = this.distSqr(startPos, vector);
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
					if ((path2.connectsToHighway || this.worldBuilder.Townships.Count <= 0) && (!path2.IsPrefabPath || this.worldBuilder.Townships.Count <= 0) && path2.StartPointID != _wildernessId && path2.EndPointID != _wildernessId && path2.radius >= _radius)
					{
						for (int i = 2; i < path2.FinalPathPoints.Count - 2; i++)
						{
							Vector2 vector2 = path2.FinalPathPoints[i];
							float num3 = this.distSqr(startPos, vector2);
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
			vector.Normalize();
			if (vector.x + vector.y == 0f)
			{
				return -1;
			}
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
		public int getHeightCeil(float x, float y)
		{
			return Mathf.CeilToInt(this.worldBuilder.GetHeight(x, y));
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public int getHeightCeil(Vector2 r)
		{
			return Mathf.CeilToInt(this.worldBuilder.GetHeight(r));
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public int subHalfWorld(int pos)
		{
			return pos - this.worldBuilder.WorldSize / 2;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int distanceSqr(Vector2i v1, Vector2i v2)
		{
			int num = v1.x - v2.x;
			int num2 = v1.y - v2.y;
			return num * num + num2 * num2;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int distanceSqr(int x1, int y1, int x2, int y2)
		{
			int num = x1 - x2;
			int num2 = y1 - y2;
			return num * num + num2 * num2;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public float distSqr(Vector2 v1, Vector2 v2)
		{
			float num = v1.x - v2.x;
			float num2 = v1.y - v2.y;
			return num * num + num2 * num2;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void SpawnMarkerPartsAndPrefabs(PrefabData _parentPrefab, Vector3i _parentPosition, int _parentRotations, int _depth, float totalDensityLeft)
		{
			List<Prefab.Marker> list = _parentPrefab.RotatePOIMarkers(true, _parentRotations);
			if (list.Count == 0)
			{
				return;
			}
			FastTags<TagGroup.Poi> fastTags = FastTags<TagGroup.Poi>.Parse(this.District.name);
			this.worldBuilder.PathingUtils.AddFullyBlockedArea(this.Area);
			Vector3i size = _parentPrefab.size;
			if (_parentRotations % 2 == 1)
			{
				ref int ptr = ref size.z;
				int x = size.x;
				int num = size.z;
				ptr = x;
				size.x = num;
			}
			List<Prefab.Marker> list2 = list.FindAll((Prefab.Marker m) => m.MarkerType == Prefab.Marker.MarkerTypes.POISpawn);
			if (_depth < 5 && list2.Count > 0)
			{
				list2.Sort((Prefab.Marker m1, Prefab.Marker m2) => (m2.Size.x + m2.Size.y + m2.Size.z).CompareTo(m1.Size.x + m1.Size.y + m1.Size.z));
				List<string> list3 = new List<string>();
				for (int i = 0; i < list2.Count; i++)
				{
					if (!list3.Contains(list2[i].GroupName))
					{
						list3.Add(list2[i].GroupName);
					}
				}
				this.Township.rand.SetSeed(this.Township.ID + (_parentPosition.x * _parentPosition.x + _parentPosition.y * _parentPosition.y));
				using (List<string>.Enumerator enumerator = list3.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						string groupName = enumerator.Current;
						List<Prefab.Marker> list4 = (from m in list2
						where m.GroupName == groupName
						orderby this.Township.rand.RandomFloat descending
						select m).ToList<Prefab.Marker>();
						int j = 0;
						while (j < list4.Count)
						{
							Prefab.Marker marker = list4[j];
							Vector2i vector2i = new Vector2i(marker.Size.x, marker.Size.z);
							Vector2i one = new Vector2i(marker.Start.x, marker.Start.z);
							one + vector2i;
							Vector2i vector2i2 = one + vector2i / 2;
							Vector2i vector2i3 = vector2i;
							if (this.District.spawnCustomSizePrefabs)
							{
								int num2;
								if (this.District.name != "gateway" && (num2 = Prefab.Marker.MarkerSizes.IndexOf(new Vector3i(vector2i.x, 0, vector2i.y))) >= 0)
								{
									if (num2 > 0)
									{
										vector2i3 = new Vector2i(Prefab.Marker.MarkerSizes[num2 - 1].x + 1, Prefab.Marker.MarkerSizes[num2 - 1].z + 1);
									}
								}
								else
								{
									vector2i3 = vector2i / 2;
								}
							}
							Vector2i vector2i4 = new Vector2i(_parentPosition.x + vector2i2.x, _parentPosition.z + vector2i2.y);
							if (_depth == 0)
							{
								int halfWorldSize = this.worldBuilder.HalfWorldSize;
								Vector2 a = new Vector2((float)(vector2i4.x - halfWorldSize), (float)(vector2i4.y - halfWorldSize));
								float num3 = 0f;
								List<PrefabDataInstance> prefabs = this.Township.Prefabs;
								for (int k = 0; k < prefabs.Count; k++)
								{
									PrefabDataInstance prefabDataInstance = prefabs[k];
									float densityScore = prefabDataInstance.prefab.DensityScore;
									if (densityScore > 6f)
									{
										Vector2 centerXZV = prefabDataInstance.CenterXZV2;
										if (Vector2.Distance(a, centerXZV) < 190f)
										{
											if (densityScore >= 20f)
											{
												num3 += densityScore * 1.3f;
											}
											else
											{
												num3 += densityScore - 6f;
											}
										}
									}
								}
								if (num3 > 0f)
								{
									totalDensityLeft = Utils.FastMax(6f, totalDensityLeft - num3);
								}
							}
							PrefabData prefabWithDistrict = this.worldBuilder.PrefabManager.GetPrefabWithDistrict(this.District, marker.Tags, vector2i3, vector2i, vector2i4, totalDensityLeft, 1f);
							if (prefabWithDistrict != null)
							{
								goto IL_4D8;
							}
							prefabWithDistrict = this.worldBuilder.PrefabManager.GetPrefabWithDistrict(this.District, marker.Tags, vector2i3, vector2i, vector2i4, totalDensityLeft + 8f, 0.3f);
							if (prefabWithDistrict != null)
							{
								goto IL_4D8;
							}
							prefabWithDistrict = this.worldBuilder.PrefabManager.GetPrefabWithDistrict(this.District, marker.Tags, vector2i3, vector2i, vector2i4, 18f, 0f);
							if (prefabWithDistrict != null)
							{
								Log.Warning("SpawnMarkerPartsAndPrefabs retry2 {0}, tags {1}, size {2} {3}, totalDensityLeft {4}, picked {5}, density {6}", new object[]
								{
									this.District.name,
									marker.Tags,
									vector2i3,
									vector2i,
									totalDensityLeft,
									prefabWithDistrict.Name,
									prefabWithDistrict.DensityScore
								});
								goto IL_4D8;
							}
							Log.Warning("SpawnMarkerPartsAndPrefabs failed {0}, tags {1}, size {2} {3}, totalDensityLeft {4}", new object[]
							{
								this.District.name,
								marker.Tags,
								vector2i3,
								vector2i,
								totalDensityLeft
							});
							IL_8AB:
							j++;
							continue;
							IL_4D8:
							int num4 = _parentPosition.x + marker.Start.x;
							int num5 = _parentPosition.z + marker.Start.z;
							if (_parentPosition.y + marker.Start.y + prefabWithDistrict.yOffset < 3)
							{
								Log.Error("SpawnMarkerPartsAndPrefabs y low! {0}, pos {1} {2}", new object[]
								{
									prefabWithDistrict.Name,
									num4,
									num5
								});
								goto IL_8AB;
							}
							totalDensityLeft -= prefabWithDistrict.DensityScore;
							if (prefabWithDistrict.Tags.Test_AnySet(this.worldBuilder.StreetTileShared.traderTag) || prefabWithDistrict.Name.Contains("trader"))
							{
								Vector2i vector2i5;
								vector2i5.x = num4 + marker.Size.x / 2;
								vector2i5.y = num5 + marker.Size.z / 2;
								this.worldBuilder.TraderCenterPositions.Add(vector2i5);
								if (this.BiomeType == BiomeType.forest)
								{
									this.worldBuilder.TraderForestCenterPositions.Add(vector2i5);
								}
								this.HasTrader = true;
								Log.Out("Trader {0}, {1}, {2}, marker {3}, at {4}", new object[]
								{
									prefabWithDistrict.Name,
									this.BiomeType,
									this.District.name,
									marker.Name,
									vector2i5
								});
							}
							int num6 = (int)marker.Rotations;
							byte b = (byte)(_parentRotations + (int)prefabWithDistrict.RotationsToNorth + num6 & 3);
							int num7 = prefabWithDistrict.size.x;
							int num8 = prefabWithDistrict.size.z;
							int num;
							if (b == 1 || b == 3)
							{
								int num9 = num7;
								num = num8;
								num8 = num9;
								num7 = num;
							}
							if (num6 == 2)
							{
								num4 += vector2i.x / 2 - num7 / 2;
							}
							else if (num6 == 3)
							{
								num5 += vector2i.y / 2 - num8 / 2;
								num4 += vector2i.x;
								num4 -= num7;
							}
							else if (num6 == 0)
							{
								num4 += vector2i.x / 2 - num7 / 2;
								num5 += vector2i.y;
								num5 -= num8;
							}
							else if (num6 == 1)
							{
								num5 += vector2i.y / 2 - num8 / 2;
							}
							Vector3i position = new Vector3i(num4, _parentPosition.y + marker.Start.y + prefabWithDistrict.yOffset, num5) + this.worldBuilder.PrefabWorldOffset;
							PrefabManager prefabManager = this.worldBuilder.PrefabManager;
							num = prefabManager.PrefabInstanceId;
							prefabManager.PrefabInstanceId = num + 1;
							PrefabDataInstance prefabDataInstance2 = new PrefabDataInstance(num, position, b, prefabWithDistrict);
							Color preview_color = this.District.preview_color;
							if (prefabDataInstance2.prefab.Name.StartsWith("remnant_") || prefabDataInstance2.prefab.Name.StartsWith("abandoned_"))
							{
								preview_color.r *= 0.75f;
								preview_color.g *= 0.75f;
								preview_color.b *= 0.75f;
							}
							else if (prefabDataInstance2.prefab.DensityScore < 1f)
							{
								preview_color.r *= 0.4f;
								preview_color.g *= 0.4f;
								preview_color.b *= 0.4f;
							}
							else if (prefabDataInstance2.prefab.Name.StartsWith("trader_"))
							{
								preview_color = new Color(0.6f, 0.3f, 0.3f);
							}
							prefabDataInstance2.previewColor = preview_color;
							this.Township.AddPrefab(prefabDataInstance2);
							this.SpawnMarkerPartsAndPrefabs(prefabWithDistrict, new Vector3i(num4, _parentPosition.y + marker.Start.y + prefabWithDistrict.yOffset, num5), (int)b, _depth + 1, totalDensityLeft);
							break;
						}
					}
				}
			}
			List<Prefab.Marker> list5 = list.FindAll((Prefab.Marker m) => m.MarkerType == Prefab.Marker.MarkerTypes.PartSpawn);
			if (_depth < 20 && list5.Count > 0)
			{
				List<string> list6 = new List<string>();
				for (int l = 0; l < list5.Count; l++)
				{
					if (!list6.Contains(list5[l].GroupName))
					{
						list6.Add(list5[l].GroupName);
					}
				}
				this.Township.rand.SetSeed(this.Township.ID + (_parentPosition.x * _parentPosition.x + _parentPosition.y * _parentPosition.y) + 1);
				using (List<string>.Enumerator enumerator = list6.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						string groupName = enumerator.Current;
						List<Prefab.Marker> list7 = (from m in list5
						where m.GroupName == groupName
						orderby this.Township.rand.RandomFloat descending
						select m).ToList<Prefab.Marker>();
						float num10 = 1f;
						if (list7.Count > 1)
						{
							num10 = 0f;
							foreach (Prefab.Marker marker2 in list7)
							{
								num10 += marker2.PartChanceToSpawn;
							}
						}
						float num11 = 0f;
						using (List<Prefab.Marker>.Enumerator enumerator2 = list7.GetEnumerator())
						{
							IL_CBE:
							while (enumerator2.MoveNext())
							{
								Prefab.Marker marker3 = enumerator2.Current;
								num11 += marker3.PartChanceToSpawn / num10;
								if (this.Township.rand.RandomRange(0f, 1f) <= num11)
								{
									if (!marker3.Tags.IsEmpty)
									{
										if (_depth == 0)
										{
											if (!this.District.tag.Test_AnySet(marker3.Tags))
											{
												continue;
											}
										}
										else if (!marker3.Tags.IsEmpty && !fastTags.Test_AnySet(marker3.Tags))
										{
											continue;
										}
									}
									PrefabData prefabByName = this.worldBuilder.PrefabManager.GetPrefabByName(marker3.PartToSpawn);
									if (prefabByName == null)
									{
										Log.Error("Part to spawn {0} not found!", new object[]
										{
											marker3.PartToSpawn
										});
									}
									else
									{
										Vector3i vector3i = new Vector3i(_parentPosition.x + marker3.Start.x - this.worldBuilder.WorldSize / 2, _parentPosition.y + marker3.Start.y, _parentPosition.z + marker3.Start.z - this.worldBuilder.WorldSize / 2);
										if (vector3i.y > 0)
										{
											byte b2 = marker3.Rotations;
											if (b2 == 1)
											{
												b2 = 3;
											}
											else if (b2 == 3)
											{
												b2 = 1;
											}
											byte b3 = (byte)((_parentRotations + (int)prefabByName.RotationsToNorth + (int)b2) % 4);
											Vector3i size2 = prefabByName.size;
											if (b3 == 1 || b3 == 3)
											{
												size2 = new Vector3i(size2.z, size2.y, size2.x);
											}
											Bounds bounds = new Bounds(vector3i + size2 * 0.5f, size2 - Vector3.one);
											foreach (Bounds bounds2 in this.partBounds)
											{
												if (bounds2.Intersects(bounds))
												{
													goto IL_CBE;
												}
											}
											Township township = this.Township;
											PrefabManager prefabManager2 = this.worldBuilder.PrefabManager;
											int num = prefabManager2.PrefabInstanceId;
											prefabManager2.PrefabInstanceId = num + 1;
											township.AddPrefab(new PrefabDataInstance(num, vector3i, b3, prefabByName));
											totalDensityLeft -= prefabByName.DensityScore;
											this.partBounds.Add(bounds);
											this.SpawnMarkerPartsAndPrefabs(prefabByName, _parentPosition + marker3.Start, (int)b3, _depth + 1, totalDensityLeft);
											break;
										}
									}
								}
							}
						}
					}
				}
			}
			if (this.District != null && this.District.name == "gateway")
			{
				list5 = list.FindAll((Prefab.Marker m) => m.PartToSpawn.Contains("highway_transition"));
				if (list5.Count > 0)
				{
					foreach (Prefab.Marker marker4 in list5)
					{
						Vector2 vector = new Vector2((float)marker4.Start.x, (float)marker4.Start.z) - new Vector2((float)(size.x / 2), (float)(size.z / 2));
						if (Mathf.Abs(vector.x) > Mathf.Abs(vector.y))
						{
							if (vector.x > 0f)
							{
								if (!this.HasExitTo(this.GetNeighbor(Vector2i.right)))
								{
									continue;
								}
								if (this.GetNeighbor(Vector2i.right).Township != this.Township)
								{
									continue;
								}
							}
							else
							{
								if (!this.HasExitTo(this.GetNeighbor(Vector2i.left)))
								{
									continue;
								}
								if (this.GetNeighbor(Vector2i.left).Township != this.Township)
								{
									continue;
								}
							}
						}
						else if (vector.y > 0f)
						{
							if (!this.HasExitTo(this.GetNeighbor(Vector2i.up)))
							{
								continue;
							}
							if (this.GetNeighbor(Vector2i.up).Township != this.Township)
							{
								continue;
							}
						}
						else if (!this.HasExitTo(this.GetNeighbor(Vector2i.down)) || this.GetNeighbor(Vector2i.down).Township != this.Township)
						{
							continue;
						}
						PrefabData prefabByName2 = this.worldBuilder.PrefabManager.GetPrefabByName(marker4.PartToSpawn);
						if (prefabByName2 != null)
						{
							Vector3i vector3i2 = new Vector3i(_parentPosition.x + marker4.Start.x - this.worldBuilder.WorldSize / 2, _parentPosition.y + marker4.Start.y, _parentPosition.z + marker4.Start.z - this.worldBuilder.WorldSize / 2);
							if (vector3i2.y > 0)
							{
								byte b4 = marker4.Rotations;
								if (b4 == 1)
								{
									b4 = 3;
								}
								else if (b4 == 3)
								{
									b4 = 1;
								}
								byte b5 = (byte)((_parentRotations + (int)prefabByName2.RotationsToNorth + (int)b4) % 4);
								Vector3i size3 = prefabByName2.size;
								if (b5 == 1 || b5 == 3)
								{
									size3 = new Vector3i(size3.z, size3.y, size3.x);
								}
								Township township2 = this.Township;
								PrefabManager prefabManager3 = this.worldBuilder.PrefabManager;
								int num = prefabManager3.PrefabInstanceId;
								prefabManager3.PrefabInstanceId = num + 1;
								township2.AddPrefab(new PrefabDataInstance(num, vector3i2, b5, prefabByName2));
							}
						}
					}
				}
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void SpawnMarkerPartsAndPrefabsWilderness(PrefabData _parentPrefab, Vector3i _parentPosition, int _parentRotations)
		{
			GameRandom gameRandom = GameRandomManager.Instance.CreateGameRandom(_parentPosition.ToString().GetHashCode());
			List<Prefab.Marker> list = _parentPrefab.RotatePOIMarkers(true, _parentRotations);
			List<Prefab.Marker> list2 = list.FindAll((Prefab.Marker m) => m.MarkerType == Prefab.Marker.MarkerTypes.POISpawn);
			if (list2.Count > 0)
			{
				for (int i = 0; i < list2.Count; i++)
				{
					Prefab.Marker marker = list2[i];
					Vector2i vector2i = new Vector2i(marker.Size.x, marker.Size.z);
					Vector2i vector2i2 = new Vector2i(marker.Start.x, marker.Start.z) + vector2i / 2;
					Vector2i minSize = vector2i;
					PrefabData wildernessPrefab = this.worldBuilder.PrefabManager.GetWildernessPrefab(this.worldBuilder.StreetTileShared.traderTag, marker.Tags, minSize, vector2i, new Vector2i(_parentPosition.x + vector2i2.x, _parentPosition.z + vector2i2.y), false);
					if (wildernessPrefab != null)
					{
						int num = _parentPosition.x + marker.Start.x;
						int num2 = _parentPosition.z + marker.Start.z;
						int num3 = (int)marker.Rotations;
						byte b = (byte)(_parentRotations + (int)wildernessPrefab.RotationsToNorth + num3 & 3);
						int num4 = wildernessPrefab.size.x;
						int num5 = wildernessPrefab.size.z;
						if (b == 1 || b == 3)
						{
							int num6 = num4;
							num4 = num5;
							num5 = num6;
						}
						if (num3 == 2)
						{
							num += vector2i.x / 2 - num4 / 2;
						}
						else if (num3 == 3)
						{
							num2 += vector2i.y / 2 - num5 / 2;
							num += vector2i.x;
							num -= num4;
						}
						else if (num3 == 0)
						{
							num += vector2i.x / 2 - num4 / 2;
							num2 += vector2i.y;
							num2 -= num5;
						}
						else if (num3 == 1)
						{
							num2 += vector2i.y / 2 - num5 / 2;
						}
						Vector3i position = new Vector3i(num - this.worldBuilder.WorldSize / 2, _parentPosition.y + marker.Start.y + wildernessPrefab.yOffset, num2 - this.worldBuilder.WorldSize / 2);
						PrefabManager prefabManager = this.worldBuilder.PrefabManager;
						int prefabInstanceId = prefabManager.PrefabInstanceId;
						prefabManager.PrefabInstanceId = prefabInstanceId + 1;
						PrefabDataInstance pdi = new PrefabDataInstance(prefabInstanceId, position, b, wildernessPrefab);
						this.AddPrefab(pdi);
						this.worldBuilder.WildernessPrefabCount++;
						wildernessPrefab.RotatePOIMarkers(true, (int)b);
						this.SpawnMarkerPartsAndPrefabsWilderness(wildernessPrefab, new Vector3i(num, _parentPosition.y + marker.Start.y + wildernessPrefab.yOffset, num2), (int)b);
					}
				}
			}
			List<Prefab.Marker> list3 = list.FindAll((Prefab.Marker m) => m.MarkerType == Prefab.Marker.MarkerTypes.PartSpawn);
			if (list3.Count > 0)
			{
				List<string> list4 = new List<string>();
				for (int j = 0; j < list3.Count; j++)
				{
					if (!list4.Contains(list3[j].GroupName))
					{
						list4.Add(list3[j].GroupName);
					}
				}
				using (List<string>.Enumerator enumerator = list4.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						string groupName = enumerator.Current;
						List<Prefab.Marker> list5 = list3.FindAll((Prefab.Marker m) => m.GroupName == groupName);
						float num7 = 1f;
						if (list5.Count > 1)
						{
							num7 = 0f;
							foreach (Prefab.Marker marker2 in list5)
							{
								num7 += marker2.PartChanceToSpawn;
							}
						}
						float num8 = 0f;
						foreach (Prefab.Marker marker3 in list5)
						{
							num8 += marker3.PartChanceToSpawn / num7;
							if (gameRandom.RandomRange(0f, 1f) <= num8 && (marker3.Tags.IsEmpty || this.worldBuilder.StreetTileShared.wildernessTag.Test_AnySet(marker3.Tags)))
							{
								PrefabData prefabByName = this.worldBuilder.PrefabManager.GetPrefabByName(marker3.PartToSpawn);
								if (prefabByName != null)
								{
									Vector3i position2 = new Vector3i(_parentPosition.x + marker3.Start.x - this.worldBuilder.WorldSize / 2, _parentPosition.y + marker3.Start.y, _parentPosition.z + marker3.Start.z - this.worldBuilder.WorldSize / 2);
									byte b2 = marker3.Rotations;
									if (b2 == 1)
									{
										b2 = 3;
									}
									else if (b2 == 3)
									{
										b2 = 1;
									}
									byte b3 = (byte)((_parentRotations + (int)prefabByName.RotationsToNorth + (int)b2) % 4);
									PrefabManager prefabManager2 = this.worldBuilder.PrefabManager;
									int prefabInstanceId = prefabManager2.PrefabInstanceId;
									prefabManager2.PrefabInstanceId = prefabInstanceId + 1;
									PrefabDataInstance pdi2 = new PrefabDataInstance(prefabInstanceId, position2, b3, prefabByName);
									this.AddPrefab(pdi2);
									this.worldBuilder.WildernessPrefabCount++;
									this.SpawnMarkerPartsAndPrefabsWilderness(prefabByName, _parentPosition + marker3.Start, (int)b3);
									break;
								}
								Log.Error("Part to spawn {0} not found!", new object[]
								{
									marker3.PartToSpawn
								});
							}
						}
					}
				}
			}
			GameRandomManager.Instance.FreeGameRandom(gameRandom);
		}

		public int GetNumTownshipNeighbors()
		{
			int num = 0;
			StreetTile[] array = this.GetNeighbors();
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i].Township == this.Township)
				{
					num++;
				}
			}
			return num;
		}

		public const int TileSize = 150;

		[PublicizedFrom(EAccessModifier.Private)]
		public const int TileSizeHalf = 75;

		[PublicizedFrom(EAccessModifier.Private)]
		public const float cDensityRadius = 190f;

		[PublicizedFrom(EAccessModifier.Private)]
		public const float cDensityBase = 6f;

		[PublicizedFrom(EAccessModifier.Private)]
		public const float cDensityMid = 20f;

		[PublicizedFrom(EAccessModifier.Private)]
		public const float cDensityMidScale = 1.3f;

		[PublicizedFrom(EAccessModifier.Private)]
		public const float cDensityBudget = 62f;

		[PublicizedFrom(EAccessModifier.Private)]
		public const float cDensityRetry = 18f;

		[PublicizedFrom(EAccessModifier.Private)]
		public const int cRadiationEdgeSize = 1;

		[PublicizedFrom(EAccessModifier.Private)]
		public const float maxHeightDiff = 10f;

		[PublicizedFrom(EAccessModifier.Private)]
		public const int partDepthLimit = 20;

		[PublicizedFrom(EAccessModifier.Private)]
		public const int poiDepthLimit = 5;

		[PublicizedFrom(EAccessModifier.Private)]
		public const float cSmoothFullRadius = 1.8f;

		[PublicizedFrom(EAccessModifier.Private)]
		public const float cSmoothFadeRadius = 3.2f;

		[PublicizedFrom(EAccessModifier.Private)]
		public const float cSmoothBoxRadius = 2.2f;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly WorldBuilder worldBuilder;

		public Township Township;

		public District District;

		public readonly List<Vector2i> UsedExitList = new List<Vector2i>();

		public int ConnectedExits;

		public readonly List<Path> ConnectedHighways = new List<Path>();

		public readonly List<PrefabDataInstance> StreetTilePrefabDatas = new List<PrefabDataInstance>();

		public readonly Vector2i GridPosition;

		public readonly Vector2i WorldPosition;

		public readonly Rect Area;

		public bool OverlapsRadiation;

		public bool OverlapsWater;

		public bool OverlapsBiomes;

		public bool HasSteepSlope;

		public bool AllIsWater;

		public bool HasTrader;

		public bool HasFeature;

		[PublicizedFrom(EAccessModifier.Private)]
		public Vector2i WildernessPOICenter;

		[PublicizedFrom(EAccessModifier.Private)]
		public int WildernessPOISize;

		[PublicizedFrom(EAccessModifier.Private)]
		public int WildernessPOIHeight;

		[PublicizedFrom(EAccessModifier.Private)]
		public int RoadShape;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool smoothAround;

		[PublicizedFrom(EAccessModifier.Private)]
		public List<Bounds> partBounds = new List<Bounds>();

		public bool Used;

		[PublicizedFrom(EAccessModifier.Private)]
		public StreetTile.PrefabRotations rotations;

		[PublicizedFrom(EAccessModifier.Private)]
		public TranslationData transData;

		[PublicizedFrom(EAccessModifier.Private)]
		public List<Vector2i> highwayExits = new List<Vector2i>();

		[PublicizedFrom(EAccessModifier.Private)]
		public StreetTile[] neighbors;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool isFullyBlocked;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool isPartBlocked;

		[PublicizedFrom(EAccessModifier.Private)]
		public enum RoadShapeTypes
		{
			straight,
			t,
			intersection,
			cap,
			corner
		}

		public enum PrefabRotations
		{
			None,
			One,
			Two,
			Three
		}
	}
}
