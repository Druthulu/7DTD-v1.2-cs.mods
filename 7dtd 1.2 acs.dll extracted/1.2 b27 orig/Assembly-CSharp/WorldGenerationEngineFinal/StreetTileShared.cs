using System;
using System.Collections.Generic;

namespace WorldGenerationEngineFinal
{
	public class StreetTileShared
	{
		public StreetTileShared(WorldBuilder _worldBuilder)
		{
			bool[][] array = new bool[5][];
			int num = 0;
			bool[] array2 = new bool[4];
			array2[0] = true;
			array2[2] = true;
			array[num] = array2;
			array[1] = new bool[]
			{
				true,
				true,
				true,
				false
			};
			array[2] = new bool[]
			{
				true,
				true,
				true,
				true
			};
			int num2 = 3;
			bool[] array3 = new bool[4];
			array3[2] = true;
			array[num2] = array3;
			int num3 = 4;
			bool[] array4 = new bool[4];
			array4[1] = true;
			array4[2] = true;
			array[num3] = array4;
			this.RoadShapeExits = array;
			this.RoadShapeExitCounts = new List<int>
			{
				2,
				3,
				4,
				1,
				2
			};
			this.RoadShapeExitsPerRotation = new List<bool[][]>();
			this.dir4way = new Vector2i[]
			{
				new Vector2i(0, 1),
				new Vector2i(1, 0),
				new Vector2i(0, -1),
				new Vector2i(-1, 0)
			};
			this.dir8way = new Vector2i[]
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
			this.dir9way = new Vector2i[]
			{
				new Vector2i(0, 1),
				new Vector2i(1, 1),
				new Vector2i(1, 0),
				new Vector2i(1, -1),
				new Vector2i(0, 0),
				new Vector2i(0, -1),
				new Vector2i(-1, -1),
				new Vector2i(-1, 0),
				new Vector2i(-1, 1)
			};
			base..ctor();
			this.worldBuilder = _worldBuilder;
			for (int i = 0; i < this.RoadShapeExits.Length; i++)
			{
				bool[][] array5 = new bool[4][];
				for (int j = 0; j < 4; j++)
				{
					bool[][] array6 = array5;
					int num4 = j;
					bool[] array7;
					switch (j)
					{
					case 1:
						array7 = new bool[]
						{
							this.RoadShapeExits[i][3],
							this.RoadShapeExits[i][0],
							this.RoadShapeExits[i][1],
							this.RoadShapeExits[i][2]
						};
						break;
					case 2:
						array7 = new bool[]
						{
							this.RoadShapeExits[i][2],
							this.RoadShapeExits[i][3],
							this.RoadShapeExits[i][0],
							this.RoadShapeExits[i][1]
						};
						break;
					case 3:
						array7 = new bool[]
						{
							this.RoadShapeExits[i][1],
							this.RoadShapeExits[i][2],
							this.RoadShapeExits[i][3],
							this.RoadShapeExits[i][0]
						};
						break;
					default:
						array7 = new bool[]
						{
							this.RoadShapeExits[i][0],
							this.RoadShapeExits[i][1],
							this.RoadShapeExits[i][2],
							this.RoadShapeExits[i][3]
						};
						break;
					}
					array6[num4] = array7;
				}
				this.RoadShapeExitsPerRotation.Add(array5);
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly WorldBuilder worldBuilder;

		public readonly FastTags<TagGroup.Poi> traderTag = FastTags<TagGroup.Poi>.Parse("trader");

		public readonly FastTags<TagGroup.Poi> wildernessTag = FastTags<TagGroup.Poi>.Parse("wilderness");

		public readonly string[] RoadShapes = new string[]
		{
			"rwg_tile_straight",
			"rwg_tile_t",
			"rwg_tile_intersection",
			"rwg_tile_cap",
			"rwg_tile_corner"
		};

		public readonly string[] RoadShapesDistrict = new string[]
		{
			"rwg_tile_{0}straight",
			"rwg_tile_{0}t",
			"rwg_tile_{0}intersection",
			"rwg_tile_{0}cap",
			"rwg_tile_{0}corner"
		};

		[PublicizedFrom(EAccessModifier.Private)]
		public bool[][] RoadShapeExits;

		public readonly List<int> RoadShapeExitCounts;

		public readonly List<bool[][]> RoadShapeExitsPerRotation;

		public readonly Vector2i[] dir4way;

		public readonly Vector2i[] dir8way;

		public readonly Vector2i[] dir9way;
	}
}
