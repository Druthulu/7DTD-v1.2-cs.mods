using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Profiling;

public class DynamicMeshUnity
{
	public static int RoundChunk(int value)
	{
		if (value < 0)
		{
			value -= 15;
		}
		return value / 16 * 16;
	}

	public static int RoundChunk(float value)
	{
		return DynamicMeshUnity.RoundChunk((int)value);
	}

	public static string GetItemPath(long key)
	{
		return DynamicMeshFile.MeshLocation + key.ToString() + ".update";
	}

	public static int GetChunkPositionFromWorldPosition(float pos)
	{
		return DynamicMeshUnity.RoundChunk((int)pos);
	}

	public static int GetChunkPositionFromWorldPosition(int pos)
	{
		return DynamicMeshUnity.RoundChunk(pos);
	}

	public static long GetRegionKeyFromItemKey(long itemKey)
	{
		return DynamicMeshUnity.GetRegionKeyFromWorldPosition(DynamicMeshUnity.GetWorldPosFromKey(itemKey));
	}

	public static long GetRegionKeyFromWorldPosition(Vector3i pos)
	{
		return WorldChunkCache.MakeChunkKey(World.toChunkXZ(DynamicMeshUnity.RoundRegion(pos.x)), World.toChunkXZ(DynamicMeshUnity.RoundRegion(pos.z)));
	}

	public static Vector2 GetXZFromKey(long key)
	{
		return new Vector2((float)(WorldChunkCache.extractX(key) * 16), (float)(WorldChunkCache.extractZ(key) * 16));
	}

	public static Vector3i GetWorldPosFromKey(long key)
	{
		return new Vector3i(WorldChunkCache.extractX(key) * 16, 0, WorldChunkCache.extractZ(key) * 16);
	}

	public static string GetDebugPositionFromKey(long key)
	{
		return string.Format("{0},{1}", WorldChunkCache.extractX(key) * 16, WorldChunkCache.extractZ(key) * 16);
	}

	public static string GetDebugPositionKey(long key)
	{
		return string.Format("{0},{1}", WorldChunkCache.extractX(key) * 16, WorldChunkCache.extractZ(key) * 16);
	}

	public static int GetChunkSectionX(long key)
	{
		return WorldChunkCache.extractX(key);
	}

	public static int GetChunkSectionZ(long key)
	{
		return WorldChunkCache.extractZ(key);
	}

	public static int GetWorldXFromKey(long key)
	{
		return WorldChunkCache.extractX(key) * 16;
	}

	public static int GetWorldZFromKey(long key)
	{
		return WorldChunkCache.extractZ(key) * 16;
	}

	public static long GetRegionKeyFromWorldPosition(int worldX, int worldZ)
	{
		return WorldChunkCache.MakeChunkKey(World.toChunkXZ(DynamicMeshUnity.RoundRegion(worldX)), World.toChunkXZ(DynamicMeshUnity.RoundRegion(worldZ)));
	}

	public static Vector3i GetRegionPositionFromWorldPosition(int worldX, int worldZ)
	{
		return new Vector3i(DynamicMeshUnity.RoundRegion(worldX), 0, DynamicMeshUnity.RoundRegion(worldZ));
	}

	public static Vector3i GetRegionPositionFromWorldPosition(Vector3i worldPos)
	{
		return new Vector3i(DynamicMeshUnity.RoundRegion(worldPos.x), 0, DynamicMeshUnity.RoundRegion(worldPos.z));
	}

	public static int RoundRegion(int value)
	{
		if (value < 0)
		{
			value -= 159;
		}
		return value / 160 * 160;
	}

	public static int RoundRegion(float value)
	{
		return DynamicMeshUnity.RoundRegion((int)value);
	}

	public static Vector3i GetRegionPositionFromWorldPosition(Vector3 worldPos)
	{
		return new Vector3i(DynamicMeshUnity.RoundRegion(worldPos.x), 0, DynamicMeshUnity.RoundRegion(worldPos.z));
	}

	public static long GetItemKey(int worldX, int worldZ)
	{
		return WorldChunkCache.MakeChunkKey(World.toChunkXZ(worldX), World.toChunkXZ(worldZ));
	}

	public static int GetItemPosition(int pos)
	{
		return World.toChunkXZ(pos) * 16;
	}

	public static float Distance(Vector3i a, Vector3i b)
	{
		return Mathf.Abs(Mathf.Sqrt(Mathf.Pow((float)(a.x - b.x), 2f) + Mathf.Pow((float)(a.z - b.z), 2f)));
	}

	public static float Distance(Vector3i a, Vector3 b)
	{
		return Mathf.Abs(Mathf.Sqrt(Mathf.Pow((float)a.x - b.x, 2f) + Mathf.Pow((float)a.z - b.z, 2f)));
	}

	public static float Distance(int x1, int y1, int x2, int y2)
	{
		return Mathf.Abs(Mathf.Sqrt(Mathf.Pow((float)(x1 - x2), 2f) + Mathf.Pow((float)(y1 - y2), 2f)));
	}

	public static void log(string msg)
	{
		if (DynamicMeshManager.ShowDebug && DynamicMeshManager.DoLog)
		{
			DynamicMeshManager.LogMsg(msg);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void LogMsg(string msg)
	{
		if (DynamicMeshManager.DoLog)
		{
			DynamicMeshManager.LogMsg(msg);
		}
	}

	public static Vector3i GetChunkPositionFromWorldPosition(Vector3i worldPosition)
	{
		return new Vector3i(DynamicMeshUnity.RoundChunk(worldPosition.x), worldPosition.y, DynamicMeshUnity.RoundChunk(worldPosition.z));
	}

	public static bool IsInBuffer(float x, float z, int bufferSize, int xIndex, int zIndex)
	{
		int num = (int)(x / 160f);
		int num2 = (int)(z / 160f);
		if (x < 0f)
		{
			num--;
		}
		if (z < 0f)
		{
			num2--;
		}
		return zIndex >= num2 - bufferSize && zIndex <= num2 + bufferSize && xIndex >= num - bufferSize && xIndex <= num + bufferSize;
	}

	public static void WaitCoroutine(IEnumerator func)
	{
		while (func.MoveNext())
		{
			if (func.Current != null)
			{
				IEnumerator func2;
				try
				{
					func2 = (IEnumerator)func.Current;
				}
				catch (InvalidCastException)
				{
					if (func.Current.GetType() == typeof(WaitForSeconds))
					{
						UnityEngine.Debug.LogWarning("Skipped call to WaitForSeconds. Use WaitForSecondsRealtime instead.");
					}
					break;
				}
				DynamicMeshUnity.WaitCoroutine(func2);
			}
		}
	}

	public static long GetMeshSize(GameObject go)
	{
		long num = 0L;
		MeshFilter component = go.GetComponent<MeshFilter>();
		if (component != null)
		{
			Mesh sharedMesh = component.sharedMesh;
			if (sharedMesh != null)
			{
				num += Profiler.GetRuntimeMemorySizeLong(sharedMesh);
			}
		}
		foreach (object obj in go.transform)
		{
			Transform transform = (Transform)obj;
			num += DynamicMeshUnity.GetMeshSize(transform.gameObject);
		}
		return num;
	}

	public static Bounds GetBoundsFromVertsJustY(ArrayListMP<Vector3> verts, Bounds bounds)
	{
		float num = verts[0].y;
		float num2 = verts[0].y;
		for (int i = 0; i < verts.Count; i++)
		{
			Vector3 vector = verts[i];
			num = Math.Min(num, vector.y);
			num2 = Math.Max(num2, vector.y);
		}
		Vector3 min = new Vector3(0f, num, 0f);
		Vector3 max = new Vector3(0f, num2, 0f);
		bounds.SetMinMax(min, max);
		return bounds;
	}

	public static void DeleteDynamicMeshData(ICollection<long> chunks)
	{
		string str;
		HashSetLong hashSetLong;
		DynamicMeshUnity.GetOrCreateDynamicMeshChunksList(out str, out hashSetLong);
		if (!hashSetLong.Overlaps(chunks))
		{
			return;
		}
		if (DynamicMeshManager.Instance != null && GamePrefs.GetBool(EnumGamePrefs.DynamicMeshEnabled))
		{
			DynamicMeshUnity.tempRegions.Clear();
			foreach (long num in chunks)
			{
				if (hashSetLong.Contains(num))
				{
					DynamicMeshManager instance = DynamicMeshManager.Instance;
					DynamicMeshItem dynamicMeshItem = (instance != null) ? instance.GetItemOrNull(num) : null;
					if (dynamicMeshItem == null)
					{
						UnityEngine.Debug.LogError(string.Format("Failed to retrieve valid DynamicMeshItem for cached dynamic mesh chunk key: {0}.", num));
					}
					else
					{
						DynamicMeshManager.Instance.RemoveItem(dynamicMeshItem, true);
						string itemPath = DynamicMeshUnity.GetItemPath(num);
						if (SdFile.Exists(itemPath))
						{
							SdFile.Delete(itemPath);
						}
						DynamicMeshRegion region = dynamicMeshItem.GetRegion();
						DynamicMeshUnity.tempRegions.Add(region);
					}
				}
			}
			foreach (DynamicMeshRegion dynamicMeshRegion in DynamicMeshUnity.tempRegions)
			{
				dynamicMeshRegion.CleanUp();
				if (dynamicMeshRegion.LoadedItems.Count == 0 && dynamicMeshRegion.UnloadedItems.Count == 0)
				{
					string path = dynamicMeshRegion.Path;
					if (SdFile.Exists(path))
					{
						SdFile.Delete(path);
					}
				}
				else
				{
					DynamicMeshThread.AddRegionUpdateData(dynamicMeshRegion.WorldPosition.x, dynamicMeshRegion.WorldPosition.z, true);
				}
			}
			DynamicMeshUnity.tempRegions.Clear();
			return;
		}
		DynamicMeshUnity.tempRegionKeys.Clear();
		foreach (long num2 in chunks)
		{
			if (hashSetLong.Contains(num2))
			{
				string path2 = str + num2.ToString() + ".update";
				if (SdFile.Exists(path2))
				{
					SdFile.Delete(path2);
				}
				int value = WorldChunkCache.extractX(num2) * 16;
				int value2 = WorldChunkCache.extractZ(num2) * 16;
				long item = WorldChunkCache.MakeChunkKey(World.toChunkXZ(DynamicMeshUnity.RoundRegion(value)), World.toChunkXZ(DynamicMeshUnity.RoundRegion(value2)));
				DynamicMeshUnity.tempRegionKeys.Add(item);
				hashSetLong.Remove(num2);
			}
		}
		foreach (long num3 in DynamicMeshUnity.tempRegionKeys)
		{
			string path3 = str + num3.ToString() + ".group";
			if (SdFile.Exists(path3))
			{
				SdFile.Delete(path3);
			}
		}
		DynamicMeshUnity.tempRegionKeys.Clear();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void GetOrCreateDynamicMeshChunksList(out string meshLocation, out HashSetLong keys)
	{
		string text = SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer ? GameIO.GetSaveGameDir() : GameIO.GetSaveGameLocalDir();
		if (!DynamicMeshUnity.cachedDynamicMeshChunksList.Item1.StartsWith(text, StringComparison.InvariantCultureIgnoreCase))
		{
			string text2 = text + "/DynamicMeshes/";
			DynamicMeshUnity.cachedDynamicMeshChunksList.Item1 = text2;
			DynamicMeshUnity.cachedDynamicMeshChunksList.Item2.Clear();
			SdDirectoryInfo sdDirectoryInfo = new SdDirectoryInfo(text2);
			if (sdDirectoryInfo.Exists)
			{
				using (IEnumerator<SdFileInfo> enumerator = sdDirectoryInfo.EnumerateFiles("*.update", SearchOption.TopDirectoryOnly).GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						long item;
						if (long.TryParse(Path.GetFileNameWithoutExtension(enumerator.Current.Name), out item))
						{
							DynamicMeshUnity.cachedDynamicMeshChunksList.Item2.Add(item);
						}
					}
				}
			}
		}
		meshLocation = DynamicMeshUnity.cachedDynamicMeshChunksList.Item1;
		keys = DynamicMeshUnity.cachedDynamicMeshChunksList.Item2;
	}

	public static void AddDisabledImposterChunk(long key)
	{
		string text;
		HashSetLong hashSetLong;
		DynamicMeshUnity.GetOrCreateDynamicMeshChunksList(out text, out hashSetLong);
		hashSetLong.Add(key);
	}

	public static void RemoveDisabledImposterChunk(long key)
	{
		string text;
		HashSetLong hashSetLong;
		DynamicMeshUnity.GetOrCreateDynamicMeshChunksList(out text, out hashSetLong);
		hashSetLong.Remove(key);
	}

	public static void ClearCachedDynamicMeshChunksList()
	{
		DynamicMeshUnity.cachedDynamicMeshChunksList.Item1 = string.Empty;
		DynamicMeshUnity.cachedDynamicMeshChunksList.Item2.Clear();
	}

	[Conditional("UNITY_STANDALONE")]
	public static void EnsureDMDirectoryExists()
	{
		if (!SdDirectory.Exists(DynamicMeshFile.MeshLocation))
		{
			SdDirectory.CreateDirectory(DynamicMeshFile.MeshLocation);
		}
	}

	public const int RegionSize = 160;

	[PublicizedFrom(EAccessModifier.Private)]
	public static HashSet<DynamicMeshRegion> tempRegions = new HashSet<DynamicMeshRegion>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static HashSet<long> tempRegionKeys = new HashSet<long>();

	[TupleElementNames(new string[]
	{
		"path",
		"keys"
	})]
	[PublicizedFrom(EAccessModifier.Private)]
	public static ValueTuple<string, HashSetLong> cachedDynamicMeshChunksList = new ValueTuple<string, HashSetLong>(string.Empty, new HashSetLong());
}
