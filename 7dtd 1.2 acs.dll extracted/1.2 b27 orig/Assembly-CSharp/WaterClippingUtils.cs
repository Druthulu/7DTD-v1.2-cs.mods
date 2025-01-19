using System;
using System.Diagnostics;
using UnityEngine;

public class WaterClippingUtils
{
	public static bool GetCubePlaneIntersectionEdgeLoop(Plane plane, ref Vector3[] intersectionPoints, out int count)
	{
		count = 0;
		for (int i = 0; i < WaterClippingUtils.cubeVerts.Length; i++)
		{
			WaterClippingUtils.cubeVertDistances[i] = plane.GetDistanceToPoint(WaterClippingUtils.cubeVerts[i]);
		}
		Vector3 vector = Vector3.zero;
		for (int j = 0; j < WaterClippingUtils.cubeEdges.Length; j += 2)
		{
			float num = WaterClippingUtils.cubeVertDistances[WaterClippingUtils.cubeEdges[j]];
			float num2 = WaterClippingUtils.cubeVertDistances[WaterClippingUtils.cubeEdges[j + 1]];
			if (Mathf.Sign(num) != Mathf.Sign(num2))
			{
				Vector3 b = WaterClippingUtils.cubeVerts[WaterClippingUtils.cubeEdges[j]];
				Vector3 a = WaterClippingUtils.cubeVerts[WaterClippingUtils.cubeEdges[j + 1]];
				float t = -num2 / (num - num2);
				Vector3 vector2 = Vector3.Lerp(a, b, t);
				intersectionPoints[count] = vector2;
				vector += vector2;
				count++;
			}
		}
		if (count < 3)
		{
			return false;
		}
		vector /= (float)count;
		Vector3 from = intersectionPoints[0] - vector;
		WaterClippingUtils.hullVertAngles[0] = 0f;
		for (int k = 1; k < count; k++)
		{
			float num3 = Vector3.SignedAngle(from, intersectionPoints[k] - vector, plane.normal);
			WaterClippingUtils.hullVertAngles[k] = ((num3 < 0f) ? (num3 + 360f) : num3);
		}
		for (int l = count; l < 6; l++)
		{
			WaterClippingUtils.hullVertAngles[l] = 1000f + (float)l;
		}
		Array.Sort<float, Vector3>(WaterClippingUtils.hullVertAngles, intersectionPoints);
		return true;
	}

	[Conditional("DEBUG_WATER_CLIPPING")]
	[PublicizedFrom(EAccessModifier.Private)]
	public static void DebugDrawIntersectionSurface(Plane plane, Vector3[] intersectionPoints, int count, Vector3 hullCenter)
	{
		for (int i = 0; i < count; i++)
		{
			Vector3 vector = intersectionPoints[i];
			UnityEngine.Debug.DrawLine(vector + 0.1f * Vector3.down, vector + 0.1f * Vector3.up, Color.white);
			UnityEngine.Debug.DrawLine(vector + 0.1f * Vector3.left, vector + 0.1f * Vector3.right, Color.white);
			UnityEngine.Debug.DrawLine(vector + 0.1f * Vector3.forward, vector + 0.1f * Vector3.back, Color.white);
			UnityEngine.Debug.DrawLine(hullCenter, vector, Color.white);
			UnityEngine.Debug.DrawLine(vector, intersectionPoints[(i + 1) % count], Color.white);
		}
		for (int j = 0; j < WaterClippingUtils.cubeVerts.Length; j++)
		{
			Vector3 vector2 = WaterClippingUtils.cubeVerts[j] + WaterClippingUtils.cubeVertDistances[j] * -plane.normal;
			if (!WaterClippingUtils.CubeBounds.Contains(vector2))
			{
				Vector3 end = GeometryUtils.NearestPointOnEdgeLoop(vector2, intersectionPoints, count);
				UnityEngine.Debug.DrawLine(vector2, end, Color.yellow);
			}
		}
	}

	[Conditional("DEBUG_WATER_CLIPPING")]
	[PublicizedFrom(EAccessModifier.Private)]
	public static void DebugDrawCubeVertPlaneOffsets(Plane plane)
	{
		for (int i = 0; i < WaterClippingUtils.cubeVerts.Length; i++)
		{
			float num = WaterClippingUtils.cubeVertDistances[i];
			UnityEngine.Debug.DrawRay(WaterClippingUtils.cubeVerts[i], num * -plane.normal, (Mathf.Sign(num) > 0f) ? Color.green : Color.red);
		}
	}

	public const string PropWaterClipPlane = "WaterClipPlane";

	public const string PropWaterFlow = "WaterFlow";

	public static readonly Bounds CubeBounds = new Bounds(new Vector3(0.5f, 0.5f, 0.5f), Vector3.one);

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Vector3[] cubeVerts = new Vector3[]
	{
		new Vector3(0f, 0f, 0f),
		new Vector3(0f, 1f, 0f),
		new Vector3(1f, 1f, 0f),
		new Vector3(1f, 0f, 0f),
		new Vector3(0f, 0f, 1f),
		new Vector3(0f, 1f, 1f),
		new Vector3(1f, 1f, 1f),
		new Vector3(1f, 0f, 1f)
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly int[] cubeEdges = new int[]
	{
		0,
		1,
		1,
		2,
		2,
		3,
		3,
		0,
		4,
		5,
		5,
		6,
		6,
		7,
		7,
		4,
		0,
		4,
		1,
		5,
		2,
		6,
		3,
		7
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly float[] cubeVertDistances = new float[8];

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly float[] hullVertAngles = new float[6];
}
