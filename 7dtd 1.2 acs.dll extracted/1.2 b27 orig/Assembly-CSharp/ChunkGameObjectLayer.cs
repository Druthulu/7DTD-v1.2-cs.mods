using System;
using System.Collections.Generic;
using UnityEngine;

public class ChunkGameObjectLayer : IMemoryPoolableObject
{
	public ChunkGameObjectLayer()
	{
		int num = MeshDescription.meshes.Length;
		this.m_MeshFilter = new MeshFilter[num][];
		this.m_MeshRenderer = new MeshRenderer[num][];
		this.m_MeshCollider = new MeshCollider[num][];
		this.m_MeshesGO = new GameObject[num];
		this.m_ParentGO = new GameObject("CLayer");
		Transform transform = this.m_ParentGO.transform;
		for (int i = 0; i < num; i++)
		{
			MeshDescription meshDescription = MeshDescription.meshes[i];
			GameObject gameObject = new GameObject(meshDescription.Name);
			this.m_MeshesGO[i] = gameObject;
			gameObject.transform.SetParent(transform, false);
			VoxelMesh.CreateMeshFilter(i, 0, gameObject, meshDescription.Tag, true, out this.m_MeshFilter[i], out this.m_MeshRenderer[i], out this.m_MeshCollider[i]);
		}
		Occludee.Add(this.m_ParentGO);
		this.m_ParentGO.SetActive(false);
	}

	public void Init(int _chunkLayerIdx, Dictionary<string, int> _layerMappingTable, Transform _chunkT, bool _bStatic)
	{
		for (int i = 0; i < MeshDescription.meshes.Length; i++)
		{
			MeshDescription meshDescription = MeshDescription.meshes[i];
			GameObject gameObject = this.m_MeshesGO[i];
			gameObject.isStatic = _bStatic;
			if (!string.IsNullOrEmpty(meshDescription.MeshLayerName))
			{
				gameObject.layer = _layerMappingTable[meshDescription.MeshLayerName];
			}
			else
			{
				gameObject.layer = 0;
			}
			MeshCollider meshCollider = this.m_MeshCollider[i][0];
			if (meshCollider)
			{
				if (meshCollider.sharedMesh)
				{
					Log.Warning("ChunkGameObjectLayer Init collider '{0}' should be null", new object[]
					{
						meshCollider.sharedMesh.name
					});
				}
				GameObject gameObject2 = meshCollider.gameObject;
				gameObject2.isStatic = _bStatic;
				gameObject2.layer = _layerMappingTable[meshDescription.ColliderLayerName];
			}
		}
		this.m_ParentGO.name = "CLayer" + _chunkLayerIdx.ToString("00");
		this.m_ParentGO.transform.SetParent(_chunkT, false);
	}

	public void Reset()
	{
		int num = this.m_MeshFilter.Length;
		for (int i = 0; i < num; i++)
		{
			foreach (MeshFilter meshFilter in this.m_MeshFilter[i])
			{
				if (meshFilter)
				{
					Mesh sharedMesh = meshFilter.sharedMesh;
					if (sharedMesh)
					{
						meshFilter.sharedMesh = null;
						VoxelMesh.AddPooledMesh(sharedMesh);
					}
				}
			}
			MeshCollider meshCollider = this.m_MeshCollider[i][0];
			if (meshCollider)
			{
				Mesh sharedMesh2 = meshCollider.sharedMesh;
				if (sharedMesh2)
				{
					meshCollider.sharedMesh = null;
					UnityEngine.Object.Destroy(sharedMesh2);
				}
			}
		}
	}

	public void Cleanup()
	{
		for (int i = 0; i < this.m_MeshFilter.Length; i++)
		{
			for (int j = 0; j < this.m_MeshFilter[i].Length; j++)
			{
				MeshFilter meshFilter = this.m_MeshFilter[i][j];
				if (meshFilter)
				{
					Mesh sharedMesh = meshFilter.sharedMesh;
					if (sharedMesh)
					{
						meshFilter.sharedMesh = null;
						UnityEngine.Object.Destroy(sharedMesh);
					}
				}
			}
		}
		for (int k = 0; k < this.m_MeshRenderer.Length; k++)
		{
			MeshRenderer[] array = this.m_MeshRenderer[k];
			for (int l = 0; l < array.Length; l++)
			{
				MeshRenderer meshRenderer = array[l];
				if (meshRenderer)
				{
					UnityEngine.Object.Destroy(meshRenderer);
					array[l] = null;
				}
			}
		}
		for (int m = 0; m < this.m_MeshCollider.Length; m++)
		{
			if (this.m_MeshCollider[m][0] != null && this.m_MeshCollider[m][0].sharedMesh != null)
			{
				this.m_MeshCollider[m][0].sharedMesh.Clear(false);
				UnityEngine.Object.Destroy(this.m_MeshCollider[m][0].sharedMesh);
			}
		}
		for (int n = 0; n < this.m_MeshesGO.Length; n++)
		{
			if (this.m_MeshesGO[n] != null)
			{
				UnityEngine.Object.Destroy(this.m_MeshesGO[n]);
			}
		}
		UnityEngine.Object.Destroy(this.m_ParentGO);
	}

	public GameObject m_ParentGO;

	public MeshFilter[][] m_MeshFilter;

	public MeshRenderer[][] m_MeshRenderer;

	public MeshCollider[][] m_MeshCollider;

	public GameObject[] m_MeshesGO;

	public bool isGrassCastShadows;

	public static int InstanceCount;
}
