﻿using System;
using System.Collections.Generic;
using UnityEngine;

public class VoxelMeshTerrain : VoxelMesh
{
	[PublicizedFrom(EAccessModifier.Private)]
	public void InitStatic()
	{
		VoxelMeshTerrain.mainTexPropertyIds = new int[3];
		VoxelMeshTerrain.mainTexPropertyNames = new string[3];
		VoxelMeshTerrain.bumpMapPropertyIds = new int[3];
		VoxelMeshTerrain.bumpMapPropertyNames = new string[3];
		VoxelMeshTerrain.sideTexPropertyIds = new int[3];
		VoxelMeshTerrain.sideTexPropertyNames = new string[3];
		VoxelMeshTerrain.sideBumpMapPropertyIds = new int[3];
		VoxelMeshTerrain.sideBumpMapPropertyNames = new string[3];
		for (int i = 0; i < VoxelMeshTerrain.mainTexPropertyIds.Length; i++)
		{
			VoxelMeshTerrain.mainTexPropertyNames[i] = "_MainTex" + ((i > 0) ? (i + 1).ToString() : "");
			VoxelMeshTerrain.mainTexPropertyIds[i] = Shader.PropertyToID(VoxelMeshTerrain.mainTexPropertyNames[i]);
			VoxelMeshTerrain.bumpMapPropertyNames[i] = "_BumpMap" + ((i > 0) ? (i + 1).ToString() : "");
			VoxelMeshTerrain.bumpMapPropertyIds[i] = Shader.PropertyToID(VoxelMeshTerrain.bumpMapPropertyNames[i]);
			VoxelMeshTerrain.sideTexPropertyNames[i] = "_SideTex" + ((i > 0) ? (i + 1).ToString() : "");
			VoxelMeshTerrain.sideTexPropertyIds[i] = Shader.PropertyToID(VoxelMeshTerrain.sideTexPropertyNames[i]);
			VoxelMeshTerrain.sideBumpMapPropertyNames[i] = "_SideBumpMap" + ((i > 0) ? (i + 1).ToString() : "");
			VoxelMeshTerrain.sideBumpMapPropertyIds[i] = Shader.PropertyToID(VoxelMeshTerrain.sideBumpMapPropertyNames[i]);
		}
		VoxelMeshTerrain.InitMicroSplat();
		VoxelMeshTerrain.isInitStatic = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void InitMicroSplat()
	{
		VoxelMeshTerrain.msPropData = LoadManager.LoadAssetFromAddressables<MicroSplatPropData>("TerrainTextures", "Microsplat/MicroSplatTerrainInGame_propdata.asset", null, null, false, true).Asset;
		VoxelMeshTerrain.msPropTex = VoxelMeshTerrain.msPropData.GetTexture();
		VoxelMeshTerrain.msProcData = LoadManager.LoadAssetFromAddressables<MicroSplatProceduralTextureConfig>("TerrainTextures", "Microsplat/MicroSplatTerrainInGame_proceduraltexture.asset", null, null, false, true).Asset;
		VoxelMeshTerrain.msProcCurveTex = VoxelMeshTerrain.msProcData.GetCurveTexture();
		VoxelMeshTerrain.msProcParamTex = VoxelMeshTerrain.msProcData.GetParamTexture();
	}

	public VoxelMeshTerrain(int _meshIndex, int _minSize = 500) : base(_meshIndex, _minSize, VoxelMesh.CreateFlags.Default)
	{
		this.m_Uvs3 = new ArrayListMP<Vector2>(MemoryPools.poolVector2, _minSize);
		this.m_Uvs4 = new ArrayListMP<Vector2>(MemoryPools.poolVector2, _minSize);
	}

	public static int EncodeTexIds(int _mainTexId, int _sideTexId)
	{
		return _mainTexId << 16 | _sideTexId;
	}

	public static int DecodeMainTexId(int _fullTexId)
	{
		return _fullTexId >> 16;
	}

	public static int DecodeSideTexId(int _fullTexId)
	{
		return _fullTexId & 65535;
	}

	public override Color[] UpdateColors(byte _suncolor, byte _blockcolor)
	{
		float b = (float)_suncolor / 15f;
		float a = (float)_blockcolor / 15f;
		for (int i = 0; i < this.m_ColorVertices.Count; i++)
		{
			Color value = this.m_ColorVertices[i];
			value.b = b;
			value.a = a;
			this.m_ColorVertices[i] = value;
		}
		return this.m_ColorVertices.ToArray();
	}

	public override void GetColorForTextureId(int _subMeshIdx, int _fullTexId, bool _bTopSoil, out Color _color, out Vector2 _uv, out Vector2 _uv2, out Vector2 _uv3, out Vector2 _uv4)
	{
		if (!World.IsSplatMapAvailable || this.IsPreviewVoxelMesh)
		{
			_uv = (_uv2 = (_uv3 = (_uv4 = VoxelMeshTerrain.cUvEmpty)));
			_color = this.submeshes[_subMeshIdx].GetColorForTextureId(_fullTexId);
			return;
		}
		int texId = VoxelMeshTerrain.DecodeMainTexId(_fullTexId);
		this.GetColorForTextureId(texId, _bTopSoil, out _color, out _uv, out _uv2, out _uv3, out _uv4);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void GetColorForTextureId(int _texId, bool _bTopSoil, out Color _color, out Vector2 _uv, out Vector2 _uv2, out Vector2 _uv3, out Vector2 _uv4)
	{
		_uv = (_uv2 = (_uv3 = (_uv4 = VoxelMeshTerrain.cUvEmpty)));
		if (_bTopSoil)
		{
			_color = VoxelMeshTerrain.cColSplatMap;
			return;
		}
		_color = VoxelMeshTerrain.cColUnderTerrain4to10;
		if (_texId <= 33)
		{
			if (_texId <= 2)
			{
				if (_texId == 1)
				{
					_uv2 = VoxelMeshTerrain.cUvUnderTerrain0_1;
					return;
				}
				if (_texId == 2)
				{
					_color = VoxelMeshTerrain.cColUnderTerrain1;
					return;
				}
			}
			else
			{
				if (_texId == 10)
				{
					_uv = VoxelMeshTerrain.cUvUnderTerrain1_0;
					return;
				}
				if (_texId == 11)
				{
					_color = VoxelMeshTerrain.cColUnderTerrain2;
					return;
				}
				if (_texId == 33)
				{
					_uv = VoxelMeshTerrain.cUvUnderTerrain0_1;
					return;
				}
			}
		}
		else if (_texId <= 300)
		{
			if (_texId == 34)
			{
				_color = VoxelMeshTerrain.cColUnderTerrain3;
				return;
			}
			if (_texId == 184)
			{
				_uv3 = VoxelMeshTerrain.cUvUnderTerrain1_0;
				return;
			}
			if (_texId == 300)
			{
				_uv2 = VoxelMeshTerrain.cUvUnderTerrain1_0;
				return;
			}
		}
		else
		{
			if (_texId == 316)
			{
				_uv4 = VoxelMeshTerrain.cUvUnderTerrain1_0;
				return;
			}
			if (_texId == 438)
			{
				_uv4 = VoxelMeshTerrain.cUvUnderTerrain0_1;
				return;
			}
			if (_texId == 440)
			{
				_uv3 = VoxelMeshTerrain.cUvUnderTerrain0_1;
				return;
			}
		}
		_color = VoxelMeshTerrain.cColSplatMap;
	}

	public override int FindOrCreateSubMesh(int _t0, int _t1, int _t2)
	{
		this.texIds[0] = _t0;
		this.texIds[1] = ((_t1 != _t0) ? _t1 : -1);
		this.texIds[2] = ((_t2 != _t0 && _t2 != _t1) ? _t2 : -1);
		for (int i = 0; i < this.submeshes.Count; i++)
		{
			if (this.submeshes[i].Contains(this.texIds))
			{
				return i;
			}
		}
		for (int j = 0; j < this.submeshes.Count; j++)
		{
			if (this.submeshes[j].CanAdd(this.texIds))
			{
				return j;
			}
		}
		TerrainSubMesh item = new TerrainSubMesh(this.submeshes, 4096);
		item.Add(this.texIds);
		this.submeshes.Add(item);
		return this.submeshes.Count - 1;
	}

	public override void AddIndices(int _i0, int _i1, int _i2, int _submesh)
	{
		ArrayListMP<int> triangles = this.submeshes[_submesh].triangles;
		int num = triangles.Alloc(3);
		triangles.Items[num] = _i0;
		triangles.Items[num + 1] = _i1;
		triangles.Items[num + 2] = _i2;
	}

	public override void ClearMesh()
	{
		base.ClearMesh();
		this.submeshes.Clear();
	}

	public override void Finished()
	{
		this.m_Triangles = 0;
		for (int i = 0; i < this.submeshes.Count; i++)
		{
			this.m_Triangles += this.submeshes[i].triangles.Count / 3;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ConfigureTerrainMaterial(Material mat, ChunkProviderGenerateWorldFromRaw cpr)
	{
		mat.SetTexture("_CustomControl0", cpr.splats[0]);
		mat.SetTexture("_CustomControl1", cpr.splats[1]);
		if (VoxelMeshTerrain.msPropTex)
		{
			mat.SetTexture("_PerTexProps", VoxelMeshTerrain.msPropTex);
		}
		if (VoxelMeshTerrain.msProcData)
		{
			mat.SetTexture("_ProcTexCurves", VoxelMeshTerrain.msProcCurveTex);
			mat.SetTexture("_ProcTexParams", VoxelMeshTerrain.msProcParamTex);
			mat.SetInt("_PCLayerCount", VoxelMeshTerrain.msProcData.layers.Count);
			if (cpr.procBiomeMask1 != null && mat.HasProperty("_ProcTexBiomeMask"))
			{
				mat.SetTexture("_ProcTexBiomeMask", cpr.procBiomeMask1);
			}
			if (cpr.procBiomeMask2 != null && mat.HasProperty("_ProcTexBiomeMask2"))
			{
				mat.SetTexture("_ProcTexBiomeMask2", cpr.procBiomeMask2);
			}
		}
		Vector2i worldSize = cpr.GetWorldSize();
		mat.SetVector("_WorldDim", new Vector4((float)worldSize.x, (float)worldSize.y));
	}

	public void ApplyMaterials(MeshRenderer _mr, TextureAtlasTerrain _ta, float _tilingFac, bool _bDistant = false)
	{
		if (!VoxelMeshTerrain.isInitStatic)
		{
			this.InitStatic();
		}
		if (World.IsSplatMapAvailable && !this.IsPreviewVoxelMesh)
		{
			MeshDescription meshDescription = MeshDescription.meshes[5];
			ChunkProviderGenerateWorldFromRaw cpr;
			if (GameManager.Instance != null && GameManager.Instance.World != null && (cpr = (GameManager.Instance.World.ChunkCache.ChunkProvider as ChunkProviderGenerateWorldFromRaw)) != null)
			{
				this.ConfigureTerrainMaterial(meshDescription.material, cpr);
				this.ConfigureTerrainMaterial(meshDescription.materialDistant, cpr);
			}
			Material[] array = _mr.sharedMaterials;
			if (this.submeshes.Count != array.Length)
			{
				array = new Material[this.submeshes.Count];
			}
			for (int i = 0; i < this.submeshes.Count; i++)
			{
				if (VoxelMeshTerrain.DecodeMainTexId(this.submeshes[i].textureIds.Data[0]) >= 5000)
				{
					array[i] = new Material(_bDistant ? MeshDescription.meshes[1].materialDistant : MeshDescription.meshes[1].material);
				}
				else
				{
					array[i] = meshDescription.material;
				}
			}
			_mr.sharedMaterials = array;
			return;
		}
		Material[] array2 = _mr.sharedMaterials;
		Utils.CleanupMaterials<Material[]>(array2);
		if (this.submeshes.Count != array2.Length)
		{
			array2 = new Material[this.submeshes.Count];
		}
		for (int j = 0; j < this.submeshes.Count; j++)
		{
			if (VoxelMeshTerrain.DecodeMainTexId(this.submeshes[j].textureIds.Data[0]) >= 5000)
			{
				array2[j] = new Material(_bDistant ? MeshDescription.meshes[1].materialDistant : MeshDescription.meshes[1].material);
			}
			else if (this.IsPreviewVoxelMesh && World.IsSplatMapAvailable)
			{
				array2[j] = new Material(_bDistant ? MeshDescription.meshes[5].prefabTerrainMaterialDistant : MeshDescription.meshes[5].prefabPreviewMaterial);
			}
			else
			{
				array2[j] = new Material(_bDistant ? MeshDescription.meshes[5].materialDistant : MeshDescription.meshes[5].material);
			}
			Utils.MarkMaterialAsSafeForManualCleanup(array2[j]);
		}
		for (int k = 0; k < this.submeshes.Count; k++)
		{
			for (int l = 0; l < this.submeshes[k].textureIds.Size; l++)
			{
				if (this.submeshes[k].textureIds.DataAvail[l])
				{
					int fullTexId = this.submeshes[k].textureIds.Data[l];
					int num = VoxelMeshTerrain.DecodeMainTexId(fullTexId);
					int num2 = VoxelMeshTerrain.DecodeSideTexId(fullTexId);
					if (num < 5000)
					{
						if (num < 0 || num >= _ta.uvMapping.Length)
						{
							Log.Error(string.Format("Error in terrain mesh generation, texture id {0} not found", num));
							return;
						}
						Vector2 value = new Vector2(1f / ((float)_ta.uvMapping[num].blockW * _tilingFac), 1f / ((float)_ta.uvMapping[num].blockH * _tilingFac));
						array2[k].SetTexture(VoxelMeshTerrain.mainTexPropertyIds[l], _ta.diffuse[num]);
						array2[k].SetTextureScale(VoxelMeshTerrain.mainTexPropertyNames[l], value);
						array2[k].SetTexture(VoxelMeshTerrain.bumpMapPropertyIds[l], _ta.normal[num]);
						array2[k].SetTextureScale(VoxelMeshTerrain.bumpMapPropertyNames[l], value);
						value = new Vector2(1f / ((float)_ta.uvMapping[num2].blockW * _tilingFac), 1f / ((float)_ta.uvMapping[num2].blockH * _tilingFac));
						array2[k].SetTexture(VoxelMeshTerrain.sideTexPropertyIds[l], _ta.diffuse[num2]);
						array2[k].SetTextureScale(VoxelMeshTerrain.sideTexPropertyNames[l], value);
						array2[k].SetTexture(VoxelMeshTerrain.sideBumpMapPropertyIds[l], _ta.normal[num2]);
						array2[k].SetTextureScale(VoxelMeshTerrain.sideBumpMapPropertyNames[l], value);
					}
				}
			}
		}
		_mr.sharedMaterials = array2;
	}

	public override int CopyToMesh(MeshFilter[] _mf, MeshRenderer[] _mr, int _lodLevel)
	{
		MeshFilter meshFilter = _mf[0];
		Mesh mesh = meshFilter.sharedMesh;
		int count = this.m_Vertices.Count;
		if (count == 0)
		{
			if (mesh)
			{
				meshFilter.sharedMesh = null;
				VoxelMesh.AddPooledMesh(mesh);
			}
			return 0;
		}
		if (!mesh)
		{
			mesh = VoxelMesh.GetPooledMesh();
			meshFilter.sharedMesh = mesh;
		}
		else
		{
			mesh.Clear(false);
		}
		MeshRenderer mr = _mr[0];
		TextureAtlasTerrain ta = (TextureAtlasTerrain)MeshDescription.meshes[5].textureAtlas;
		this.ApplyMaterials(mr, ta, 1f, false);
		if (count != this.m_ColorVertices.Count)
		{
			Log.Error("ERROR: VMT.mesh[{2}].Vertices.Count ({0}) != VMT.mesh[{2}].ColorSides.Count ({1})", new object[]
			{
				count,
				this.m_ColorVertices.Count,
				this.meshIndex
			});
		}
		if (count != this.m_Uvs.Count)
		{
			Log.Error("ERROR: VMT.mesh[{2}].Vertices.Count ({0}) != VMT.mesh[{2}].Uvs.Count ({1})", new object[]
			{
				count,
				this.m_Uvs.Count,
				this.meshIndex
			});
		}
		MeshUnsafeCopyHelper.CopyVertices(this.m_Vertices, mesh);
		MeshUnsafeCopyHelper.CopyUV(this.m_Uvs, mesh);
		if (this.UvsCrack != null)
		{
			MeshUnsafeCopyHelper.CopyUV2(this.UvsCrack, mesh);
		}
		if (this.m_Uvs3 != null && this.m_Uvs3.Items != null)
		{
			MeshUnsafeCopyHelper.CopyUV3(this.m_Uvs3, mesh);
		}
		if (this.m_Uvs4 != null && this.m_Uvs4.Items != null)
		{
			MeshUnsafeCopyHelper.CopyUV4(this.m_Uvs4, mesh);
		}
		MeshUnsafeCopyHelper.CopyColors(this.m_ColorVertices, mesh);
		mesh.subMeshCount = this.submeshes.Count;
		for (int i = 0; i < this.submeshes.Count; i++)
		{
			MeshUnsafeCopyHelper.CopyTriangles(this.submeshes[i].triangles, mesh, i);
		}
		if (this.m_Normals.Count == 0)
		{
			mesh.RecalculateNormals();
		}
		else
		{
			if (count != this.m_Normals.Count)
			{
				Log.Error("ERROR: Vertices.Count ({0}) != Normals.Count ({1})", new object[]
				{
					count,
					this.m_Normals.Count,
					this.CurTriangleIndex
				});
			}
			MeshUnsafeCopyHelper.CopyNormals(this.m_Normals, mesh);
		}
		mesh.RecalculateTangents();
		mesh.RecalculateUVDistributionMetrics(1E-09f);
		GameUtils.SetMeshVertexAttributes(mesh, false);
		mesh.UploadMeshData(false);
		return this.m_Triangles;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string MakeTransformPathName(Transform _t, int _parentMax = 1)
	{
		string text = _t.name;
		for (int i = 0; i < _parentMax; i++)
		{
			_t = _t.parent;
			if (!_t)
			{
				break;
			}
			text = _t.name + "," + text;
		}
		return text;
	}

	public override int CopyToColliders(int _clrIdx, MeshCollider _meshCollider, out Mesh mesh)
	{
		if (this.m_Vertices.Count == 0)
		{
			GameManager.Instance.World.m_ChunkManager.BakeDestroyCancel(_meshCollider);
			mesh = null;
			return 0;
		}
		mesh = base.ResetMesh(_meshCollider);
		int num = 0;
		mesh.subMeshCount = this.submeshes.Count;
		MeshUnsafeCopyHelper.CopyVertices(this.m_Vertices, mesh);
		for (int i = 0; i < this.submeshes.Count; i++)
		{
			MeshUnsafeCopyHelper.CopyTriangles(this.submeshes[i].triangles, mesh, i);
			num += this.submeshes[i].triangles.Count / 3;
		}
		return num;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool isInitStatic;

	[PublicizedFrom(EAccessModifier.Private)]
	public static int[] mainTexPropertyIds;

	[PublicizedFrom(EAccessModifier.Private)]
	public static string[] mainTexPropertyNames;

	[PublicizedFrom(EAccessModifier.Private)]
	public static int[] bumpMapPropertyIds;

	[PublicizedFrom(EAccessModifier.Private)]
	public static string[] bumpMapPropertyNames;

	[PublicizedFrom(EAccessModifier.Private)]
	public static int[] sideTexPropertyIds;

	[PublicizedFrom(EAccessModifier.Private)]
	public static string[] sideTexPropertyNames;

	[PublicizedFrom(EAccessModifier.Private)]
	public static int[] sideBumpMapPropertyIds;

	[PublicizedFrom(EAccessModifier.Private)]
	public static string[] sideBumpMapPropertyNames;

	[PublicizedFrom(EAccessModifier.Private)]
	public static MicroSplatPropData msPropData;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Texture2D msPropTex;

	[PublicizedFrom(EAccessModifier.Private)]
	public static MicroSplatProceduralTextureConfig msProcData;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Texture2D msProcCurveTex;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Texture2D msProcParamTex;

	public bool IsPreviewVoxelMesh;

	public List<TerrainSubMesh> submeshes = new List<TerrainSubMesh>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static Color cColSplatMap = new Color(0f, 0f, 0f, 0f);

	[PublicizedFrom(EAccessModifier.Private)]
	public static Color cColUnderTerrain4to10 = new Color(0f, 0f, 0f, 1f);

	[PublicizedFrom(EAccessModifier.Private)]
	public static Color cColUnderTerrain1 = new Color(1f, 0f, 0f, 1f);

	[PublicizedFrom(EAccessModifier.Private)]
	public static Color cColUnderTerrain2 = new Color(0f, 1f, 0f, 1f);

	[PublicizedFrom(EAccessModifier.Private)]
	public static Color cColUnderTerrain3 = new Color(0f, 0f, 1f, 1f);

	[PublicizedFrom(EAccessModifier.Private)]
	public static Vector2 cUvEmpty = Vector2.zero;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Vector2 cUvUnderTerrain1_0 = new Vector2(1f, 0f);

	[PublicizedFrom(EAccessModifier.Private)]
	public static Vector2 cUvUnderTerrain0_1 = new Vector2(0f, 1f);

	[PublicizedFrom(EAccessModifier.Private)]
	public int[] texIds = new int[3];
}
