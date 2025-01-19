using System;
using System.Collections.Generic;
using UnityEngine;

public static class CollectWaterUtils
{
	public static int CollectInCube(ChunkCluster cc, int requiredMass, Vector3i origin, int maxRadius, List<CollectWaterUtils.WaterPoint> points)
	{
		int num = requiredMass;
		for (int i = 0; i <= maxRadius; i++)
		{
			int num2 = 0;
			int num3 = 0;
			foreach (Vector3i pos in GenerateVoxelCubeSurface.GenerateCubeSurfacePositions(origin, i))
			{
				WaterValue water = cc.GetWater(pos);
				if (water.HasMass())
				{
					int mass = water.GetMass();
					points.Add(new CollectWaterUtils.WaterPoint(pos, mass));
					num2 += mass;
					num3++;
				}
			}
			if (num2 > num)
			{
				int j = num2 - num;
				int a = (num2 - num) / num3;
				a = Mathf.Max(a, 1);
				int num4 = points.Count - num3;
				while (j > 0)
				{
					CollectWaterUtils.WaterPoint waterPoint = points[num4];
					if (waterPoint.massToTake > 0)
					{
						int num5 = Mathf.Min(a, waterPoint.massToTake);
						waterPoint.massToTake -= num5;
						j -= num5;
						num2 -= num5;
						points[num4] = waterPoint;
					}
					num4++;
					if (num4 == points.Count)
					{
						num4 = points.Count - num3;
					}
				}
			}
			num -= num2;
			if (num <= 0)
			{
				break;
			}
		}
		return requiredMass - num;
	}

	public struct WaterPoint
	{
		public int finalMass
		{
			get
			{
				return this.mass - this.massToTake;
			}
		}

		public WaterPoint(Vector3i _pos, int _mass)
		{
			this.worldPos = _pos;
			this.mass = _mass;
			this.massToTake = _mass;
		}

		public Vector3i worldPos;

		public int mass;

		public int massToTake;
	}
}
