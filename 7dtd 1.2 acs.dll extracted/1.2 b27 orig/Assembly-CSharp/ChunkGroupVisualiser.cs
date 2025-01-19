using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class ChunkGroupVisualiser : SingletonMonoBehaviour<ChunkGroupVisualiser>
{
	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDrawGizmos()
	{
		Vector3 size = Vector3.one * 16f;
		size.y = 128f;
		Vector3 vector = -Origin.Instance.OriginPos;
		vector.y = 0f;
		Gizmos.matrix = Matrix4x4.Translate(vector);
		foreach (ValueTuple<Color, List<Vector3>> valueTuple in this.groupInfos)
		{
			Gizmos.color = valueTuple.Item1;
			foreach (Vector3 center in valueTuple.Item2)
			{
				Gizmos.DrawCube(center, size);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public void SetGroups(IEnumerable<HashSetLong> chunkGroups)
	{
		this.groupInfos.Clear();
		UnityEngine.Random.InitState(42);
		foreach (HashSetLong hashSetLong in chunkGroups)
		{
			ValueTuple<Color, List<Vector3>> valueTuple = new ValueTuple<Color, List<Vector3>>(UnityEngine.Random.ColorHSV(0f, 1f, 0.8f, 1f, 0.8f, 1f, 0.25f, 0.25f), new List<Vector3>(hashSetLong.Count));
			foreach (long key in hashSetLong)
			{
				int num = WorldChunkCache.extractX(key) * 16 + 8;
				int num2 = WorldChunkCache.extractZ(key) * 16 + 8;
				valueTuple.Item2.Add(new Vector3((float)num, 8f, (float)num2));
			}
			this.groupInfos.Add(valueTuple);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float alpha = 0.25f;

	[TupleElementNames(new string[]
	{
		"color",
		"positions"
	})]
	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public List<ValueTuple<Color, List<Vector3>>> groupInfos = new List<ValueTuple<Color, List<Vector3>>>();
}
