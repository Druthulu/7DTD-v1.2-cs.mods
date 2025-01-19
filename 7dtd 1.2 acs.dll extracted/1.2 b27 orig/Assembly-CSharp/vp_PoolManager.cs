using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class vp_PoolManager : MonoBehaviour
{
	[Preserve]
	public static vp_PoolManager Instance
	{
		get
		{
			return vp_PoolManager.m_Instance;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Awake()
	{
		vp_PoolManager.m_Instance = this;
		this.m_Transform = base.transform;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Start()
	{
		foreach (vp_PoolManager.vp_CustomPooledObject vp_CustomPooledObject in this.CustomPrefabs)
		{
			this.AddObjects(vp_CustomPooledObject.Prefab, Vector3.zero, Quaternion.identity, vp_CustomPooledObject.Buffer);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnEnable()
	{
		vp_GlobalEventReturn<UnityEngine.Object, Vector3, Quaternion, UnityEngine.Object>.Register("vp_PoolManager Instantiate", new vp_GlobalCallbackReturn<UnityEngine.Object, Vector3, Quaternion, UnityEngine.Object>(this.InstantiateInternal));
		vp_GlobalEvent<UnityEngine.Object, float>.Register("vp_PoolManager Destroy", new vp_GlobalCallback<UnityEngine.Object, float>(this.DestroyInternal));
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnDisable()
	{
		vp_GlobalEventReturn<UnityEngine.Object, Vector3, Quaternion, UnityEngine.Object>.Unregister("vp_PoolManager Instantiate", new vp_GlobalCallbackReturn<UnityEngine.Object, Vector3, Quaternion, UnityEngine.Object>(this.InstantiateInternal));
		vp_GlobalEvent<UnityEngine.Object, float>.Unregister("vp_PoolManager Destroy", new vp_GlobalCallback<UnityEngine.Object, float>(this.DestroyInternal));
	}

	public virtual void AddObjects(UnityEngine.Object obj, Vector3 position, Quaternion rotation, int amount = 1)
	{
		if (obj == null)
		{
			return;
		}
		if (!this.m_AvailableObjects.ContainsKey(obj.name))
		{
			this.m_AvailableObjects.Add(obj.name, new List<UnityEngine.Object>());
			this.m_UsedObjects.Add(obj.name, new List<UnityEngine.Object>());
		}
		for (int i = 0; i < amount; i++)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(obj, position, rotation) as GameObject;
			gameObject.name = obj.name;
			gameObject.transform.parent = this.m_Transform;
			vp_Utility.Activate(gameObject, false);
			this.m_AvailableObjects[obj.name].Add(gameObject);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual UnityEngine.Object InstantiateInternal(UnityEngine.Object original, Vector3 position, Quaternion rotation)
	{
		if (this.IgnoredPrefabs.FirstOrDefault((GameObject obj) => obj.name == original.name || obj.name == original.name + "(Clone)") != null)
		{
			return UnityEngine.Object.Instantiate(original, position, rotation);
		}
		List<UnityEngine.Object> list = null;
		List<UnityEngine.Object> list2 = null;
		if (this.m_AvailableObjects.TryGetValue(original.name, out list))
		{
			GameObject gameObject;
			for (;;)
			{
				this.m_UsedObjects.TryGetValue(original.name, out list2);
				int num = list.Count + list2.Count;
				if (this.CustomPrefabs.FirstOrDefault((vp_PoolManager.vp_CustomPooledObject obj) => obj.Prefab.name == original.name) == null && num < this.MaxAmount && list.Count == 0)
				{
					this.AddObjects(original, position, rotation, 1);
				}
				if (list.Count == 0)
				{
					gameObject = (list2.FirstOrDefault<UnityEngine.Object>() as GameObject);
					if (gameObject == null)
					{
						list2.Remove(gameObject);
					}
					else
					{
						vp_Utility.Activate(gameObject, false);
						list2.Remove(gameObject);
						list.Add(gameObject);
					}
				}
				else
				{
					gameObject = (list.FirstOrDefault<UnityEngine.Object>() as GameObject);
					if (!(gameObject == null))
					{
						break;
					}
					list.Remove(gameObject);
				}
			}
			gameObject.transform.position = position;
			gameObject.transform.rotation = rotation;
			list.Remove(gameObject);
			list2.Add(gameObject);
			vp_Utility.Activate(gameObject, true);
			return gameObject;
		}
		this.AddObjects(original, position, rotation, 1);
		return this.InstantiateInternal(original, position, rotation);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void DestroyInternal(UnityEngine.Object obj, float t)
	{
		if (obj == null)
		{
			return;
		}
		if (this.IgnoredPrefabs.FirstOrDefault((GameObject o) => o.name == obj.name || o.name == obj.name + "(Clone)") != null || (!this.m_AvailableObjects.ContainsKey(obj.name) && !this.PoolOnDestroy))
		{
			UnityEngine.Object.Destroy(obj, t);
			return;
		}
		if (t != 0f)
		{
			vp_Timer.In(t, delegate()
			{
				this.DestroyInternal(obj, 0f);
			}, null);
			return;
		}
		if (!this.m_AvailableObjects.ContainsKey(obj.name))
		{
			this.AddObjects(obj, Vector3.zero, Quaternion.identity, 1);
			return;
		}
		List<UnityEngine.Object> list = null;
		List<UnityEngine.Object> list2 = null;
		this.m_AvailableObjects.TryGetValue(obj.name, out list);
		this.m_UsedObjects.TryGetValue(obj.name, out list2);
		GameObject gameObject = list2.FirstOrDefault((UnityEngine.Object o) => o.GetInstanceID() == obj.GetInstanceID()) as GameObject;
		if (gameObject == null)
		{
			return;
		}
		gameObject.transform.parent = this.m_Transform;
		vp_Utility.Activate(gameObject, false);
		list2.Remove(gameObject);
		list.Add(gameObject);
	}

	public int MaxAmount = 25;

	public bool PoolOnDestroy = true;

	public List<GameObject> IgnoredPrefabs = new List<GameObject>();

	public List<vp_PoolManager.vp_CustomPooledObject> CustomPrefabs = new List<vp_PoolManager.vp_CustomPooledObject>();

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Transform m_Transform;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Dictionary<string, List<UnityEngine.Object>> m_AvailableObjects = new Dictionary<string, List<UnityEngine.Object>>();

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Dictionary<string, List<UnityEngine.Object>> m_UsedObjects = new Dictionary<string, List<UnityEngine.Object>>();

	[Preserve]
	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public static vp_PoolManager m_Instance;

	[Serializable]
	public class vp_CustomPooledObject
	{
		public GameObject Prefab;

		public int Buffer = 15;

		public int MaxAmount = 25;
	}
}
