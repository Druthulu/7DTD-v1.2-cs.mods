using System;
using System.Collections.Generic;
using UnityEngine;

public static class MeshLodOptimization
{
	public static void Apply(ref Transform prefab)
	{
		if (!PlatformOptimizations.MeshLodReduction)
		{
			return;
		}
		if (prefab.GetComponentInChildren<Tree>())
		{
			return;
		}
		int instanceID = prefab.GetInstanceID();
		if (Application.isEditor)
		{
			if (MeshLodOptimization.editorCache == null)
			{
				MeshLodOptimization.editorCache = new MeshLodOptimization.EditorCloneCache();
			}
			prefab = MeshLodOptimization.editorCache.CacheClone(instanceID, prefab);
		}
		if (MeshLodOptimization.processed.Contains(instanceID))
		{
			return;
		}
		MeshLodOptimization.RemoveLod1(prefab);
		MeshLodOptimization.processed.Add(instanceID);
	}

	public static void RemoveLod1(Transform prefab)
	{
		MeshLodOptimization.lodGroupBuffer.Clear();
		prefab.GetComponentsInChildren<LODGroup>(MeshLodOptimization.lodGroupBuffer);
		foreach (LODGroup lodgroup in MeshLodOptimization.lodGroupBuffer)
		{
			if (lodgroup.lodCount > 2)
			{
				LOD[] lods = lodgroup.GetLODs();
				LOD[] array = new LOD[lodgroup.lodCount - 1];
				array[0] = lods[0];
				int num = 1;
				for (int i = 2; i < lodgroup.lodCount; i++)
				{
					array[num] = lods[i];
					num++;
				}
				lodgroup.SetLODs(array);
				foreach (Renderer renderer in lods[1].renderers)
				{
					if (!(renderer == null))
					{
						bool flag = false;
						LOD[] array2 = array;
						for (int k = 0; k < array2.Length; k++)
						{
							Renderer[] renderers2 = array2[k].renderers;
							for (int l = 0; l < renderers2.Length; l++)
							{
								if (renderers2[l] == renderer)
								{
									flag = true;
									break;
								}
							}
						}
						if (!flag)
						{
							MeshFilter obj;
							if (renderer.TryGetComponent<MeshFilter>(out obj))
							{
								UnityEngine.Object.Destroy(obj);
							}
							UnityEngine.Object.Destroy(renderer);
						}
					}
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static MeshLodOptimization.EditorCloneCache editorCache;

	[PublicizedFrom(EAccessModifier.Private)]
	public static HashSet<int> processed = new HashSet<int>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static List<LODGroup> lodGroupBuffer = new List<LODGroup>();

	public class EditorCloneCache
	{
		public EditorCloneCache()
		{
			this.cloneParent = new GameObject("LodCullingCache");
			this.cloneParent.SetActive(false);
			GameManager.Instance.OnWorldChanged += delegate(World world)
			{
				if (world == null)
				{
					this.cloneParent.transform.DestroyChildren();
					this.clones.Clear();
				}
			};
		}

		public Transform CacheClone(int id, Transform prefab)
		{
			Transform transform;
			if (this.clones.TryGetValue(id, out transform))
			{
				return transform;
			}
			string name = prefab.name;
			transform = UnityEngine.Object.Instantiate<Transform>(prefab, this.cloneParent.transform);
			transform.name = name;
			this.clones.Add(id, transform);
			return transform;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public GameObject cloneParent;

		[PublicizedFrom(EAccessModifier.Private)]
		public Dictionary<int, Transform> clones = new Dictionary<int, Transform>();
	}
}
