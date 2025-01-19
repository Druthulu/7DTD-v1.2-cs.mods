using System;
using System.Collections;
using UnityEngine;

public static class GCUtils
{
	public static void Collect()
	{
		GC.Collect();
		GC.WaitForPendingFinalizers();
	}

	public static void UnloadAndCollectStart()
	{
		GCUtils.isWorking = true;
		ThreadManager.StartCoroutine(GCUtils.UnloadAndCollectCo());
	}

	public static IEnumerator UnloadAndCollectCo()
	{
		GCUtils.isWorking = true;
		GCUtils.Collect();
		yield return Resources.UnloadUnusedAssets();
		GCUtils.Collect();
		GCUtils.isWorking = false;
		yield break;
	}

	public static IEnumerator WaitForIdle()
	{
		while (GCUtils.isWorking)
		{
			yield return null;
		}
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool isWorking;
}
