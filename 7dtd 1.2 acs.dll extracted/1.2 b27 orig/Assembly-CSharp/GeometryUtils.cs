using System;
using UnityEngine;

public class GeometryUtils
{
	public static Vector3 NearestPointOnLine(Vector3 fromPoint, Vector3 lineStart, Vector3 lineEnd)
	{
		Vector3 lhs = fromPoint - lineStart;
		Vector3 vector = lineEnd - lineStart;
		float d = Mathf.Clamp01(Vector3.Dot(lhs, vector) / Vector3.Dot(vector, vector));
		return lineStart + d * vector;
	}

	public static Vector3 NearestPointOnEdgeLoop(Vector3 fromPoint, Vector3[] loopPoints, int loopPointCount)
	{
		ValueTuple<float, Vector3> valueTuple = new ValueTuple<float, Vector3>(float.MaxValue, fromPoint);
		for (int i = 0; i < loopPointCount; i++)
		{
			Vector3 lineStart = loopPoints[i];
			Vector3 lineEnd = loopPoints[(i + 1) % loopPointCount];
			Vector3 vector = GeometryUtils.NearestPointOnLine(fromPoint, lineStart, lineEnd);
			float num = Vector3.Distance(fromPoint, vector);
			if (num < valueTuple.Item1)
			{
				valueTuple = new ValueTuple<float, Vector3>(num, vector);
			}
		}
		return valueTuple.Item2;
	}

	public static void RotatePlaneAroundPoint(ref Plane plane, Vector3 pivot, Quaternion rotation)
	{
		if (rotation == Quaternion.identity)
		{
			return;
		}
		Vector3 vector = plane.normal;
		Vector3 vector2 = plane.normal * -plane.distance;
		vector2 = rotation * (vector2 - pivot) + pivot;
		vector = rotation * vector;
		plane.SetNormalAndPosition(vector, vector2);
	}
}
