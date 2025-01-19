using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class BlockShapeNew : BlockShape
{
	public BlockShapeNew()
	{
		this.IsSolidCube = false;
		this.IsSolidSpace = false;
		this.IsRotatable = true;
	}

	public override void Init(Block _block)
	{
		this.ShapeName = _block.Properties.Values["Model"];
		if (this.ShapeName == null)
		{
			throw new Exception("No model specified on block with name " + _block.GetBlockName());
		}
		Vector3 vector = new Vector3(1f, 0f, 1f);
		_block.Properties.ParseVec("ModelOffset", ref vector);
		BlockShapeNew.MeshData meshData;
		if (!BlockShapeNew.meshData.TryGetValue(this.ShapeName, out meshData))
		{
			meshData = new BlockShapeNew.MeshData();
			BlockShapeNew.meshData.Add(this.ShapeName, meshData);
		}
		BlockShapeNew.MeshData.Arrays arrays;
		if (!meshData.posArrays.TryGetValue(vector, out arrays))
		{
			if (!meshData.obj)
			{
				GameObject gameObject = DataLoader.LoadAsset<GameObject>(DataLoader.IsInResources(this.ShapeName) ? ("Shapes/" + this.ShapeName) : this.ShapeName);
				if (!gameObject)
				{
					throw new Exception("Model with name " + this.ShapeName + " not found");
				}
				meshData.obj = gameObject;
			}
			arrays = new BlockShapeNew.MeshData.Arrays();
			meshData.posArrays.Add(vector, arrays);
			this.ParseModel(meshData, arrays, vector);
		}
		this.visualMeshes = arrays.meshes;
		this.colliderMeshes = arrays.colliderMeshes;
		this.faceInfo = arrays.faceInfo;
		this.boundsRotations = arrays.boundsRotations;
		if (meshData.symTypeOverride != -1)
		{
			this.SymmetryType = meshData.symTypeOverride;
		}
		this.IsSolidCube = meshData.IsSolidCube;
		if (_block.PathType < 0)
		{
			this.boundsPathOffsetRotations = new Vector2[32];
			for (int i = 0; i < 28; i++)
			{
				Bounds bounds = this.boundsRotations[i];
				Vector3 min = bounds.min;
				Vector3 max = bounds.max;
				Vector2 vector2;
				vector2.x = 0f;
				float num = min.x + BlockShapeNew.centerOffsetV.x;
				if (num >= -0.01f)
				{
					vector2.x = (-0.5f + num) * 0.5f;
				}
				num = max.x + BlockShapeNew.centerOffsetV.x;
				if (num <= 0.01f)
				{
					vector2.x = (0.5f + num) * 0.5f;
				}
				vector2.y = 0f;
				float num2 = min.z + BlockShapeNew.centerOffsetV.z;
				if (num2 >= -0.01f)
				{
					vector2.y = (-0.5f + num2) * 0.5f;
				}
				num2 = max.z + BlockShapeNew.centerOffsetV.z;
				if (num2 <= 0.01f)
				{
					vector2.y = (0.5f + num2) * 0.5f;
				}
				if (vector2.x != 0f || vector2.y != 0f)
				{
					_block.PathType = -1;
					this.boundsPathOffsetRotations[i] = vector2;
				}
			}
		}
		base.Init(_block);
		if (meshData.obj)
		{
			foreach (MeshRenderer meshRenderer in meshData.obj.GetComponentsInChildren<MeshRenderer>())
			{
				Material sharedMaterial = meshRenderer.sharedMaterial;
				if (sharedMaterial != null)
				{
					meshRenderer.sharedMaterial = null;
					Resources.UnloadAsset(sharedMaterial);
				}
			}
			MeshFilter[] componentsInChildren2 = meshData.obj.GetComponentsInChildren<MeshFilter>();
			for (int j = 0; j < componentsInChildren2.Length; j++)
			{
				Mesh sharedMesh = componentsInChildren2[j].sharedMesh;
				if (sharedMesh != null)
				{
					meshData.obj = null;
					Resources.UnloadAsset(sharedMesh);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ParseModel(BlockShapeNew.MeshData _data, BlockShapeNew.MeshData.Arrays _arrays, Vector3 _modelOffset)
	{
		if (BlockShapeNew.convertRotationCached == null)
		{
			BlockShapeNew.convertRotationCached = new int[32, 7];
			for (int i = 0; i < 32; i++)
			{
				for (int j = 0; j < 7; j++)
				{
					BlockShapeNew.convertRotationCached[i, j] = BlockShapeNew.convertRotation((BlockFace)j, i);
				}
			}
		}
		Transform transform = _data.obj.transform;
		for (int k = 0; k < transform.childCount; k++)
		{
			Transform child = transform.GetChild(k);
			string name = child.name;
			if (name == "Solid")
			{
				_data.IsSolidCube = true;
			}
			else if (name == "LOD0")
			{
				for (int l = 0; l < child.childCount; l++)
				{
					Transform child2 = child.GetChild(l);
					string name2 = child2.name;
					int num = this.CharToFaceIndex(name2[0]);
					if (num != -1)
					{
						_arrays.meshes[num] = this.CreateMeshFromMeshFilter(child2, _modelOffset);
						if (name2.Length > 2)
						{
							for (int m = 2; m < name2.Length; m++)
							{
								char c = name2[m];
								if (c == 'F')
								{
									_arrays.faceInfo[num] = BlockShapeNew.EnumFaceOcclusionInfo.Full;
								}
								else if (c == 'P')
								{
									_arrays.faceInfo[num] = BlockShapeNew.EnumFaceOcclusionInfo.Part;
								}
								else if (c == 'A')
								{
									_arrays.faceInfo[num] = BlockShapeNew.EnumFaceOcclusionInfo.Remove;
								}
								else if (c == 'C')
								{
									_arrays.faceInfo[num] = BlockShapeNew.EnumFaceOcclusionInfo.Continuous;
								}
								else if (c == 'Y')
								{
									_arrays.faceInfo[num] = BlockShapeNew.EnumFaceOcclusionInfo.RemoveIfAny;
								}
								else if (c == 'O')
								{
									_arrays.faceInfo[num] = BlockShapeNew.EnumFaceOcclusionInfo.OwnFaces;
								}
								else if (c == 'H')
								{
									_arrays.faceInfo[num] = BlockShapeNew.EnumFaceOcclusionInfo.HideIfSame;
								}
								else if (c == 'T')
								{
									_arrays.faceInfo[num] = BlockShapeNew.EnumFaceOcclusionInfo.Transparent;
								}
							}
						}
					}
				}
			}
			else if (name == "Collider")
			{
				for (int n = 0; n < child.childCount; n++)
				{
					Transform child3 = child.GetChild(n);
					string name3 = child3.name;
					int num2 = this.CharToFaceIndex(name3[0]);
					if (num2 != -1)
					{
						_arrays.colliderMeshes[num2] = this.CreateMeshFromMeshFilter(child3, _modelOffset);
					}
				}
			}
			else if (name == "SymType_0")
			{
				_data.symTypeOverride = 0;
			}
			else if (name == "SymType_2")
			{
				_data.symTypeOverride = 2;
			}
			else if (name == "SymType_3")
			{
				_data.symTypeOverride = 3;
			}
			else if (name == "SymType_4")
			{
				_data.symTypeOverride = 4;
			}
		}
		this.CalcBounds(_arrays);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CalcBounds(BlockShapeNew.MeshData.Arrays _arrays)
	{
		Bounds bounds = default(Bounds);
		for (int i = 0; i < 28; i++)
		{
			Quaternion rotation = BlockShapeNew.rotationsToQuats[i];
			Vector3 vector = Vector3.positiveInfinity;
			Vector3 vector2 = Vector3.negativeInfinity;
			for (int j = 0; j < 6; j++)
			{
				BlockShapeNew.MySimpleMesh mySimpleMesh = _arrays.meshes[j];
				if (mySimpleMesh != null)
				{
					List<Vector3> vertices = mySimpleMesh.Vertices;
					int count = vertices.Count;
					for (int k = 0; k < count; k++)
					{
						Vector3 vector3 = rotation * (vertices[k] + BlockShapeNew.centerOffsetV);
						if (vector3.x < vector.x)
						{
							vector.x = vector3.x;
						}
						if (vector3.x > vector2.x)
						{
							vector2.x = vector3.x;
						}
						if (vector3.y < vector.y)
						{
							vector.y = vector3.y;
						}
						if (vector3.y > vector2.y)
						{
							vector2.y = vector3.y;
						}
						if (vector3.z < vector.z)
						{
							vector.z = vector3.z;
						}
						if (vector3.z > vector2.z)
						{
							vector2.z = vector3.z;
						}
					}
				}
			}
			vector -= BlockShapeNew.centerOffsetV;
			vector2 -= BlockShapeNew.centerOffsetV;
			bounds.SetMinMax(vector, vector2);
			bounds.extents = new Vector3(Utils.FastMax(bounds.extents.x, 0.1f), Utils.FastMax(bounds.extents.y, 0.1f), Utils.FastMax(bounds.extents.z, 0.1f));
			_arrays.boundsRotations[i] = bounds;
		}
	}

	public static void Cleanup()
	{
		BlockShapeNew.meshData.Clear();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int CharToFaceIndex(char _c)
	{
		if (_c <= 'E')
		{
			if (_c == 'B')
			{
				return 1;
			}
			if (_c == 'E')
			{
				return 5;
			}
		}
		else
		{
			if (_c == 'M')
			{
				return 6;
			}
			if (_c == 'N')
			{
				return 2;
			}
			switch (_c)
			{
			case 'S':
				return 4;
			case 'T':
				return 0;
			case 'W':
				return 3;
			}
		}
		return -1;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static int convertRotation(BlockFace _face, int _rotation)
	{
		Vector3 vector = Vector3.zero;
		Vector3 vector2;
		switch (_face)
		{
		case BlockFace.Top:
			vector2 = Vector3.up;
			break;
		case BlockFace.Bottom:
			vector2 = Vector3.down;
			break;
		case BlockFace.North:
			vector2 = Vector3.forward;
			break;
		case BlockFace.West:
			vector2 = Vector3.left;
			break;
		case BlockFace.South:
			vector2 = Vector3.back;
			break;
		case BlockFace.East:
			vector2 = Vector3.right;
			break;
		default:
			vector2 = Vector3.zero;
			break;
		}
		Quaternion rotation = Quaternion.Inverse(BlockShapeNew.rotationsToQuats[_rotation]);
		vector = rotation * vector;
		vector2 = rotation * vector2;
		Vector3 vector3 = vector2 - vector;
		int result;
		if (vector3.x > 0.9f)
		{
			result = 5;
		}
		else if (vector3.x < -0.9f)
		{
			result = 3;
		}
		else if (vector3.y > 0.9f)
		{
			result = 0;
		}
		else if (vector3.y < -0.9f)
		{
			result = 1;
		}
		else if (vector3.z > 0.9f)
		{
			result = 2;
		}
		else if (vector3.z < -0.9f)
		{
			result = 4;
		}
		else
		{
			result = (int)_face;
		}
		return result;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void convertRotationUVDirs(BlockFace _face, int _rotation, out Vector3 _dX, out Vector3 _dY)
	{
		Vector3 point;
		Vector3 point2;
		switch (_face)
		{
		case BlockFace.Top:
			point = Vector3.right;
			point2 = Vector3.forward;
			break;
		case BlockFace.Bottom:
			point = Vector3.left;
			point2 = Vector3.forward;
			break;
		case BlockFace.North:
			point = Vector3.left;
			point2 = Vector3.up;
			break;
		case BlockFace.West:
			point = Vector3.back;
			point2 = Vector3.up;
			break;
		case BlockFace.South:
			point = Vector3.right;
			point2 = Vector3.up;
			break;
		case BlockFace.East:
			point = Vector3.forward;
			point2 = Vector3.up;
			break;
		case BlockFace.Middle:
			point = Vector3.left;
			point2 = Vector3.up;
			break;
		default:
			point = Vector3.zero;
			point2 = Vector3.zero;
			break;
		}
		Quaternion rotation = BlockShapeNew.rotationsToQuats[_rotation];
		_dX = rotation * point;
		_dY = rotation * point2;
	}

	public static int ConvertRotationFree(int _rotation, Quaternion _q, bool _bApplyRotFirst = false)
	{
		Vector3 vector = new Vector3(-0.5f, -0.5f, -0.5f);
		Vector3 vector2 = Vector3.up;
		Quaternion rotationStatic = BlockShapeNew.GetRotationStatic(_rotation);
		if (_bApplyRotFirst)
		{
			vector = _q * vector;
			vector2 = _q * vector2;
			vector = rotationStatic * vector;
			vector2 = rotationStatic * vector2;
		}
		else
		{
			vector = rotationStatic * vector;
			vector2 = rotationStatic * vector2;
			vector = _q * vector;
			vector2 = _q * vector2;
		}
		vector.x += 0.5f;
		vector.y += 0.5f;
		vector.z += 0.5f;
		Vector3i vector3i = new Vector3i(Mathf.RoundToInt(vector.x), Mathf.RoundToInt(vector.y), Mathf.RoundToInt(vector.z));
		Vector3i vector3i2 = new Vector3i(Mathf.RoundToInt(vector2.x), Mathf.RoundToInt(vector2.y), Mathf.RoundToInt(vector2.z));
		if (vector3i2.x == 0)
		{
			if (vector3i2.z == 0)
			{
				if (vector3i2.y == 1)
				{
					if (vector3i.y == 0)
					{
						int num = BlockShapeNew.BlockFaceToRot(BlockFace.Top);
						if (vector3i.x == 0)
						{
							if (vector3i.z == 0)
							{
								return num;
							}
							return num + 1;
						}
						else
						{
							if (vector3i.z == 1)
							{
								return num + 2;
							}
							return num + 3;
						}
					}
				}
				else if (vector3i.y == 1)
				{
					int num2 = BlockShapeNew.BlockFaceToRot(BlockFace.Bottom);
					if (vector3i.x == 1)
					{
						if (vector3i.z == 0)
						{
							return num2;
						}
						return num2 + 1;
					}
					else
					{
						if (vector3i.z == 1)
						{
							return num2 + 2;
						}
						return num2 + 3;
					}
				}
			}
			if (vector3i2.y == 0)
			{
				if (vector3i2.z == 1)
				{
					if (vector3i.z == 0)
					{
						int num3 = BlockShapeNew.BlockFaceToRot(BlockFace.North);
						if (vector3i.x == 1)
						{
							if (vector3i.y == 0)
							{
								return num3;
							}
							return num3 + 1;
						}
						else
						{
							if (vector3i.y == 1)
							{
								return num3 + 2;
							}
							return num3 + 3;
						}
					}
				}
				else if (vector3i.z == 1)
				{
					int num4 = BlockShapeNew.BlockFaceToRot(BlockFace.South);
					if (vector3i.x == 0)
					{
						if (vector3i.y == 0)
						{
							return num4;
						}
						return num4 + 1;
					}
					else
					{
						if (vector3i.y == 1)
						{
							return num4 + 2;
						}
						return num4 + 3;
					}
				}
			}
		}
		else if (vector3i2.y == 0 && vector3i2.z == 0)
		{
			if (vector3i2.x == -1)
			{
				if (vector3i.x == 1)
				{
					int num5 = BlockShapeNew.BlockFaceToRot(BlockFace.West);
					if (vector3i.y == 0)
					{
						if (vector3i.z == 0)
						{
							return num5;
						}
						return num5 + 1;
					}
					else
					{
						if (vector3i.z == 1)
						{
							return num5 + 2;
						}
						return num5 + 3;
					}
				}
			}
			else if (vector3i.x == 0)
			{
				int num6 = BlockShapeNew.BlockFaceToRot(BlockFace.East);
				if (vector3i.y == 1)
				{
					if (vector3i.z == 0)
					{
						return num6;
					}
					return num6 + 1;
				}
				else
				{
					if (vector3i.z == 1)
					{
						return num6 + 2;
					}
					return num6 + 3;
				}
			}
		}
		return 0;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public BlockShapeNew.MySimpleMesh CreateMeshFromMeshFilter(Transform _transform, Vector3 _modelOffset)
	{
		MeshFilter component = _transform.GetComponent<MeshFilter>();
		if (component == null)
		{
			return null;
		}
		Mesh sharedMesh = component.sharedMesh;
		if (!sharedMesh.isReadable)
		{
			Log.Error("Mesh '" + sharedMesh.name + "' not readable in shape with Model=" + this.ShapeName);
			return null;
		}
		BlockShapeNew.MySimpleMesh mySimpleMesh = new BlockShapeNew.MySimpleMesh();
		Matrix4x4 localToWorldMatrix = _transform.localToWorldMatrix;
		localToWorldMatrix.m03 += _modelOffset.x;
		localToWorldMatrix.m13 += _modelOffset.y;
		localToWorldMatrix.m23 += _modelOffset.z;
		sharedMesh.GetVertices(mySimpleMesh.Vertices);
		int count = mySimpleMesh.Vertices.Count;
		for (int i = 0; i < count; i++)
		{
			Vector3 vector = mySimpleMesh.Vertices[i];
			vector = localToWorldMatrix.MultiplyPoint3x4(vector);
			mySimpleMesh.Vertices[i] = vector;
		}
		if (sharedMesh.subMeshCount == 1)
		{
			sharedMesh.GetTriangles(mySimpleMesh.Indices, 0, true);
		}
		else
		{
			int[] triangles = sharedMesh.triangles;
			for (int j = 0; j < triangles.Length; j++)
			{
				mySimpleMesh.Indices.Add((ushort)triangles[j]);
			}
		}
		sharedMesh.GetNormals(mySimpleMesh.Normals);
		int count2 = mySimpleMesh.Normals.Count;
		for (int k = 0; k < count2; k++)
		{
			Vector3 vector2 = mySimpleMesh.Normals[k];
			vector2 = localToWorldMatrix.MultiplyVector(vector2);
			mySimpleMesh.Normals[k] = vector2.normalized;
		}
		sharedMesh.GetUVs(0, mySimpleMesh.Uvs);
		return mySimpleMesh;
	}

	public override int getFacesDrawnFullBitfield(BlockValue _blockValue)
	{
		int num = 0;
		for (int i = 0; i < this.faceInfo.Length; i++)
		{
			int num2 = BlockShapeNew.convertRotationCached[(int)_blockValue.rotation, i];
			if (this.faceInfo[num2] == BlockShapeNew.EnumFaceOcclusionInfo.Full)
			{
				num |= 1 << i;
			}
		}
		return num;
	}

	public override bool isRenderFace(BlockValue _blockValue, BlockFace _face, BlockValue _adjBlockValue)
	{
		if (_blockValue.ischild)
		{
			return false;
		}
		int num = BlockShapeNew.convertRotationCached[(int)_blockValue.rotation, (int)_face];
		if (this.visualMeshes[num] == null)
		{
			return false;
		}
		Block block = _blockValue.Block;
		switch (this.faceInfo[num])
		{
		case BlockShapeNew.EnumFaceOcclusionInfo.None:
			return false;
		case BlockShapeNew.EnumFaceOcclusionInfo.Part:
		{
			BlockShapeNew blockShapeNew;
			if (!_adjBlockValue.ischild && block.MeshIndex == _adjBlockValue.Block.MeshIndex && (blockShapeNew = (_adjBlockValue.Block.shape as BlockShapeNew)) != null)
			{
				int num2 = BlockShapeNew.convertRotationCached[(int)_adjBlockValue.rotation, (int)BlockFaceFlags.OppositeFace(_face)];
				if (_adjBlockValue.rotation == _blockValue.rotation && blockShapeNew.ShapeName == this.ShapeName && blockShapeNew.faceInfo[num2] == BlockShapeNew.EnumFaceOcclusionInfo.Part)
				{
					return false;
				}
				if (blockShapeNew.faceInfo[num2] == BlockShapeNew.EnumFaceOcclusionInfo.Full)
				{
					return false;
				}
			}
			break;
		}
		case BlockShapeNew.EnumFaceOcclusionInfo.Remove:
		{
			BlockShapeNew blockShapeNew2 = _adjBlockValue.Block.shape as BlockShapeNew;
			if (blockShapeNew2 != null)
			{
				int num3 = BlockShapeNew.convertRotationCached[(int)_adjBlockValue.rotation, (int)BlockFaceFlags.OppositeFace(_face)];
				if (blockShapeNew2.faceInfo[num3] == BlockShapeNew.EnumFaceOcclusionInfo.Remove)
				{
					return false;
				}
			}
			return true;
		}
		case BlockShapeNew.EnumFaceOcclusionInfo.Continuous:
		{
			BlockShapeNew blockShapeNew3;
			if (!_adjBlockValue.ischild && block.MeshIndex == _adjBlockValue.Block.MeshIndex && (blockShapeNew3 = (_adjBlockValue.Block.shape as BlockShapeNew)) != null)
			{
				int num4 = BlockShapeNew.convertRotationCached[(int)_adjBlockValue.rotation, (int)BlockFaceFlags.OppositeFace(_face)];
				if (blockShapeNew3.faceInfo[num4] == BlockShapeNew.EnumFaceOcclusionInfo.Full)
				{
					return false;
				}
			}
			break;
		}
		case BlockShapeNew.EnumFaceOcclusionInfo.RemoveIfAny:
			if (!_adjBlockValue.isair)
			{
				return false;
			}
			break;
		case BlockShapeNew.EnumFaceOcclusionInfo.OwnFaces:
			if (_adjBlockValue.type == _blockValue.type)
			{
				return false;
			}
			break;
		case BlockShapeNew.EnumFaceOcclusionInfo.HideIfSame:
			if (_adjBlockValue.type == _blockValue.type && _adjBlockValue.rotation == _blockValue.rotation)
			{
				return false;
			}
			break;
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int roundToIntAndMod(float x, int mod)
	{
		if (mod == 0)
		{
			return 0;
		}
		if (x < 0f)
		{
			return (mod + (int)(x - 0.9999999f) % mod) % mod;
		}
		return (int)x % mod;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public BlockShapeNew.MySimpleMesh getVisualMesh(int _idx)
	{
		return this.visualMeshes[_idx];
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public BlockShapeNew.MySimpleMesh getColliderMesh(int _idx, BlockShapeNew.MySimpleMesh _visualMesh)
	{
		if (this.colliderMeshes[_idx] != null)
		{
			return this.colliderMeshes[_idx];
		}
		return _visualMesh;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public float DOT(Vector3 A, Vector3 B)
	{
		return A.x * B.x + A.y * B.y + A.z * B.z;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool EpsilonEqual(float A, float B, float epsilon = 0.0001f)
	{
		return A <= B + epsilon && A >= B - epsilon;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool AbsEpsilonEqual(float A, float B, float epsilon = 0.0001f)
	{
		return Mathf.Abs(A) <= Mathf.Abs(B) + epsilon && Mathf.Abs(A) >= Mathf.Abs(B) - epsilon;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool AbsEpsilonLessEqual(float A, float B, float epsilon = 0.0001f)
	{
		return Mathf.Abs(A) <= Mathf.Abs(B) || this.AbsEpsilonEqual(A, B, epsilon);
	}

	public override void renderFace(Vector3i _chunkPos, BlockValue _blockValue, Vector3 _drawPos, BlockFace _face, Vector3[] _vertices, LightingAround _lightingAround, long _textureFull, VoxelMesh[] _meshes, BlockShape.MeshPurpose _purpose = BlockShape.MeshPurpose.World)
	{
		if (_blockValue.ischild)
		{
			return;
		}
		int num = BlockShapeNew.convertRotationCached[(int)_blockValue.rotation, (int)_face];
		BlockShapeNew.MySimpleMesh visualMesh = this.getVisualMesh(num);
		if (visualMesh == null)
		{
			return;
		}
		Block block = _blockValue.Block;
		int num2 = 0;
		int num3;
		if (this.faceInfo[num] == BlockShapeNew.EnumFaceOcclusionInfo.Transparent)
		{
			num3 = 2;
			_face = BlockFace.North;
		}
		else
		{
			num3 = (int)block.MeshIndex;
			if (num3 == 0)
			{
				int num4 = Chunk.Value64FullToIndex(_textureFull, (BlockFace)num);
				BlockTextureData blockTextureData = BlockTextureData.list[num4];
				if (blockTextureData == null)
				{
					if (!DynamicMeshBlockSwap.InvalidPaintIds.Contains(num4))
					{
						DynamicMeshBlockSwap.InvalidPaintIds.Add(num4);
						Log.Out(string.Concat(new string[]
						{
							"Missing paint ID XML entry: ",
							num4.ToString(),
							" for block '",
							block.GetBlockName(),
							"'"
						}));
					}
				}
				else
				{
					num2 = (int)blockTextureData.TextureID;
				}
			}
		}
		VoxelMesh voxelMesh = _meshes[num3];
		if (voxelMesh == null)
		{
			return;
		}
		MeshDescription meshDescription = MeshDescription.meshes[num3];
		int num5 = (num2 == 0) ? block.GetSideTextureId(_blockValue, (BlockFace)num) : num2;
		if ((ulong)num5 >= (ulong)((long)meshDescription.textureAtlas.uvMapping.Length))
		{
			return;
		}
		UVRectTiling uvrectTiling = meshDescription.textureAtlas.uvMapping[num5];
		if (uvrectTiling.blockW == 0 || uvrectTiling.blockH == 0)
		{
			Log.Error("Block with name '{0}' uses a texture id {1} that is not in the atlas!", new object[]
			{
				block.GetBlockName(),
				num5
			});
			return;
		}
		int num6 = uvrectTiling.blockW;
		int num7 = uvrectTiling.blockH;
		bool flag = uvrectTiling.bGlobalUV;
		Block.UVMode uvmode = block.GetUVMode(num);
		if (uvmode != Block.UVMode.Global)
		{
			if (uvmode == Block.UVMode.Local)
			{
				flag = false;
			}
		}
		else
		{
			flag = true;
		}
		flag &= (_purpose != BlockShape.MeshPurpose.Local);
		bool bTextureArray = meshDescription.bTextureArray;
		Vector3 vector = _drawPos;
		if (_purpose == BlockShape.MeshPurpose.Preview)
		{
			vector.x += (float)(_chunkPos.x + 1);
			vector.y += (float)(_chunkPos.y + 1);
			vector.z += (float)(_chunkPos.z + 1);
		}
		Vector3 vector2;
		Vector3 vector3;
		this.convertRotationUVDirs((BlockFace)num, (int)_blockValue.rotation, out vector2, out vector3);
		float num8 = 0f;
		float num9 = 0f;
		float x = 0f;
		float x2 = 0f;
		if (!flag || !bTextureArray)
		{
			switch (_face)
			{
			case BlockFace.Top:
			case BlockFace.Bottom:
				if (vector2.x < -0.9f || vector2.x > 0.9f)
				{
					num8 = vector2.x;
					x = vector.x;
				}
				else
				{
					num8 = vector2.z;
					x = vector.z;
				}
				if (vector3.z < -0.9f || vector3.z > 0.9f)
				{
					num9 = vector3.z;
					x2 = vector.z;
				}
				else
				{
					num9 = vector3.x;
					x2 = vector.x;
				}
				break;
			case BlockFace.North:
			case BlockFace.South:
				if (vector2.x < -0.9f || vector2.x > 0.9f)
				{
					num8 = vector2.x;
					x = vector.x;
				}
				else
				{
					num8 = vector2.y;
					x = vector.y;
				}
				if (vector3.y < -0.9f || vector3.y > 0.9f)
				{
					num9 = vector3.y;
					x2 = vector.y;
				}
				else
				{
					num9 = vector3.x;
					x2 = vector.x;
				}
				break;
			case BlockFace.West:
			case BlockFace.East:
				if (vector2.z < -0.9f || vector2.z > 0.9f)
				{
					num8 = vector2.z;
					x = vector.z;
				}
				else
				{
					num8 = vector2.y;
					x = vector.y;
				}
				if (vector3.y < -0.9f || vector3.y > 0.9f)
				{
					num9 = vector3.y;
					x2 = vector.y;
				}
				else
				{
					num9 = vector3.z;
					x2 = vector.z;
				}
				break;
			}
		}
		int num10 = (int)uvrectTiling.uv.width;
		int num11 = (int)uvrectTiling.uv.height;
		int num12 = 0;
		Vector2 vector4;
		if (!bTextureArray)
		{
			vector4.x = ((num8 > 0f) ? (uvrectTiling.uv.x + (float)this.roundToIntAndMod(x, num6) * uvrectTiling.uv.width) : (uvrectTiling.uv.x + uvrectTiling.uv.width * (float)(num6 - 1) - (float)this.roundToIntAndMod(x, num6) * uvrectTiling.uv.width));
			vector4.y = ((num9 > 0f) ? (uvrectTiling.uv.y + (float)this.roundToIntAndMod(x2, num7) * uvrectTiling.uv.height) : (uvrectTiling.uv.y + uvrectTiling.uv.height * (float)(num7 - 1) - (float)this.roundToIntAndMod(x2, num7) * uvrectTiling.uv.height));
		}
		else
		{
			num6 = Utils.FastMax(num6, num10);
			num7 = Utils.FastMax(num7, num11);
			if (flag)
			{
				switch (_face)
				{
				case BlockFace.Top:
				case BlockFace.Bottom:
					x = vector.x;
					x2 = vector.z;
					break;
				case BlockFace.North:
				case BlockFace.South:
					x = vector.x;
					x2 = vector.y;
					break;
				case BlockFace.West:
				case BlockFace.East:
					x = vector.z;
					x2 = vector.y;
					break;
				}
				vector4.x = (float)this.roundToIntAndMod(x, num6);
				vector4.y = (float)this.roundToIntAndMod(x2, num7);
			}
			else
			{
				vector4.x = (float)((num8 > 0f) ? this.roundToIntAndMod(x, num6) : (num6 - 1 - this.roundToIntAndMod(x, num6)));
				vector4.y = (float)((num9 > 0f) ? this.roundToIntAndMod(x2, num7) : (num7 - 1 - this.roundToIntAndMod(x2, num7)));
			}
			if (num10 > 1)
			{
				num12 += (int)(vector4.x % (float)num10);
			}
			if (num11 > 1)
			{
				num12 += (int)(vector4.y % (float)num11) * num10;
			}
			vector4.x -= (float)((int)vector4.x);
			vector4.y -= (float)((int)vector4.y);
		}
		int num13 = uvrectTiling.index + num12;
		Quaternion rotation = BlockShapeNew.rotationsToQuats[(int)_blockValue.rotation];
		Vector3 b = BlockShapeNew.centerOffsetV;
		Vector3 b2 = _drawPos - BlockShapeNew.centerOffsetV + this.GetRotationOffset(_blockValue);
		Color color;
		if (bTextureArray)
		{
			color.g = (float)num13;
		}
		else
		{
			color.g = (float)(block.Properties.Contains("Frame") ? 1 : 0);
		}
		color.b = (float)_lightingAround[LightingAround.Pos.Middle].stability / 15f;
		color.a = (float)(flag ? 1 : 0);
		int count = voxelMesh.m_Vertices.Count;
		ArrayListMP<Vector3> collVertices = voxelMesh.m_CollVertices;
		int num14 = (collVertices != null) ? collVertices.Count : 0;
		int count2 = visualMesh.Vertices.Count;
		voxelMesh.CheckVertexLimit(count2);
		if (count2 + voxelMesh.m_Vertices.Count > 786432)
		{
			return;
		}
		int num15 = voxelMesh.m_Vertices.Alloc(count2);
		voxelMesh.m_Normals.Alloc(count2);
		voxelMesh.m_ColorVertices.Alloc(count2);
		for (int i = 0; i < count2; i++)
		{
			int idx = num15 + i;
			Vector3 vector5 = rotation * (visualMesh.Vertices[i] + b) + b2;
			voxelMesh.m_Vertices[idx] = vector5;
			Vector3 value = rotation * visualMesh.Normals[i];
			voxelMesh.m_Normals[idx] = value;
			vector5 -= _drawPos;
			float num16 = (float)_lightingAround[LightingAround.Pos.X0Y0Z0].sun * (1f - vector5.x) * (1f - vector5.y) * (1f - vector5.z) + (float)_lightingAround[LightingAround.Pos.X1Y0Z0].sun * vector5.x * (1f - vector5.y) * (1f - vector5.z) + (float)_lightingAround[LightingAround.Pos.X0Y0Z1].sun * (1f - vector5.x) * (1f - vector5.y) * vector5.z + (float)_lightingAround[LightingAround.Pos.X1Y0Z1].sun * vector5.x * (1f - vector5.y) * vector5.z + (float)_lightingAround[LightingAround.Pos.X0Y1Z0].sun * (1f - vector5.x) * vector5.y * (1f - vector5.z) + (float)_lightingAround[LightingAround.Pos.X0Y1Z1].sun * (1f - vector5.x) * vector5.y * vector5.z + (float)_lightingAround[LightingAround.Pos.X1Y1Z0].sun * vector5.x * vector5.y * (1f - vector5.z) + (float)_lightingAround[LightingAround.Pos.X1Y1Z1].sun * vector5.x * vector5.y * vector5.z;
			color.r = num16 / 15f;
			voxelMesh.m_ColorVertices[idx] = color;
		}
		int num17 = voxelMesh.m_Indices.Alloc(visualMesh.Indices.Count);
		for (int j = 0; j < visualMesh.Indices.Count; j++)
		{
			voxelMesh.m_Indices[num17 + j] = voxelMesh.CurTriangleIndex + (int)visualMesh.Indices[j];
		}
		voxelMesh.CurTriangleIndex += visualMesh.Vertices.Count;
		if (voxelMesh.m_CollVertices != null)
		{
			BlockShapeNew.MySimpleMesh colliderMesh = this.getColliderMesh(num, visualMesh);
			num17 = voxelMesh.m_CollVertices.Alloc(colliderMesh.Vertices.Count);
			if (colliderMesh != visualMesh)
			{
				for (int k = 0; k < colliderMesh.Vertices.Count; k++)
				{
					Vector3 value2 = rotation * (colliderMesh.Vertices[k] + b) + b2;
					voxelMesh.m_CollVertices[num17 + k] = value2;
				}
			}
			else
			{
				for (int l = 0; l < colliderMesh.Vertices.Count; l++)
				{
					voxelMesh.m_CollVertices[num17 + l] = voxelMesh.m_Vertices[l + count];
				}
			}
			num17 = voxelMesh.m_CollIndices.Alloc(colliderMesh.Indices.Count);
			for (int m = 0; m < colliderMesh.Indices.Count; m++)
			{
				voxelMesh.m_CollIndices[num17 + m] = num14 + (int)colliderMesh.Indices[m];
			}
		}
		int count3 = visualMesh.Uvs.Count;
		Rect rect = WorldConstants.MapDamageToUVRect(_blockValue);
		if (bTextureArray)
		{
			num17 = voxelMesh.m_Uvs.Alloc(count3);
			Vector2 vector6;
			for (int n = 0; n < count3; n++)
			{
				int idx2 = num17 + n;
				if (flag)
				{
					vector6.x = (float)(num6 / num10);
					vector6.y = (float)(num7 / num11);
				}
				else
				{
					vector6.x = vector4.x + visualMesh.Uvs[n].x;
					vector6.y = vector4.y + visualMesh.Uvs[n].y;
				}
				voxelMesh.m_Uvs[idx2] = vector6;
			}
			if (voxelMesh.UvsCrack != null)
			{
				voxelMesh.UvsCrack.Alloc(count3);
				for (int num18 = 0; num18 < count3; num18++)
				{
					int idx3 = num17 + num18;
					if (!BlockShapeNew.bImposterGenerationActive)
					{
						vector6.x = rect.x + visualMesh.Uvs[num18].x * rect.width;
						vector6.y = rect.y + visualMesh.Uvs[num18].y * rect.height;
					}
					else
					{
						vector6.x = color.g;
						vector6.y = color.a;
					}
					voxelMesh.UvsCrack[idx3] = vector6;
				}
				return;
			}
		}
		else
		{
			if (!flag)
			{
				for (int num19 = 0; num19 < count3; num19++)
				{
					Vector2 vector6;
					vector6.x = vector4.x + visualMesh.Uvs[num19].x * uvrectTiling.uv.width;
					vector6.y = vector4.y + visualMesh.Uvs[num19].y * uvrectTiling.uv.height;
					voxelMesh.m_Uvs.Add(vector6);
					if (voxelMesh.UvsCrack != null)
					{
						vector6.x = rect.x + visualMesh.Uvs[num19].x * rect.width;
						vector6.y = rect.y + visualMesh.Uvs[num19].y * rect.height;
						voxelMesh.UvsCrack.Add(vector6);
					}
				}
				return;
			}
			Vector3i vector3i;
			vector3i.x = num6;
			vector3i.y = num7;
			vector3i.z = num6;
			Vector3i vector3i2;
			vector3i2.x = Mathf.Abs(_chunkPos.x + (int)_drawPos.x);
			vector3i2.y = Mathf.Abs(_chunkPos.y + (int)_drawPos.y);
			vector3i2.z = Mathf.Abs(_chunkPos.z + (int)_drawPos.z);
			if (_chunkPos.x < 0)
			{
				vector3i2.x = Mathf.Abs(_chunkPos.x + 16 - (int)_drawPos.x);
			}
			if (_chunkPos.y < 0)
			{
				vector3i2.y = Mathf.Abs(_chunkPos.y + 16 - (int)_drawPos.y);
			}
			if (_chunkPos.z < 0)
			{
				vector3i2.z = Mathf.Abs(_chunkPos.z + 16 - (int)_drawPos.z);
			}
			Vector3 vector7;
			vector7.x = (float)(vector3i2.x % vector3i.x);
			vector7.y = (float)(vector3i2.y % vector3i.y);
			vector7.z = (float)(vector3i2.z % vector3i.z);
			bool flag2 = false;
			for (int num20 = 0; num20 < 6; num20++)
			{
				BlockShapeNew.MySimpleMesh visualMesh2 = this.getVisualMesh(num20);
				if (visualMesh2 != null && visualMesh2.Uvs.Count > 6)
				{
					flag2 = true;
					break;
				}
			}
			for (int num21 = 0; num21 < count3; num21++)
			{
				Vector3 vector8 = voxelMesh.m_Normals[count];
				object obj = Mathf.Abs(vector8.y) > Mathf.Abs(vector8.x) && Mathf.Abs(vector8.y) > Mathf.Abs(vector8.z);
				bool flag3 = Mathf.Abs(vector8.x) > Mathf.Abs(vector8.y) && Mathf.Abs(vector8.x) > Mathf.Abs(vector8.z);
				object obj2 = obj;
				if (obj2 == null)
				{
					bool flag4 = !flag3;
				}
				if (obj2 != null)
				{
					vector3i.z = uvrectTiling.blockH;
					vector7.z = (float)(vector3i2.z % vector3i.z);
				}
				Vector2 zero = Vector2.zero;
				vector8 = voxelMesh.m_Normals[count + num21];
				if (!flag2)
				{
					BlockFace blockFace = BlockFace.Top;
					if (vector8.z > 0.95f)
					{
						blockFace = BlockFace.North;
					}
					else if (vector8.z < -0.95f)
					{
						blockFace = BlockFace.South;
					}
					else if (vector8.x > 0.95f)
					{
						blockFace = BlockFace.East;
					}
					else if (vector8.x < -0.95f)
					{
						blockFace = BlockFace.West;
					}
					else if (vector8.y < 0f)
					{
						blockFace = BlockFace.Bottom;
					}
					bool flag5 = Mathf.Abs(vector8.z) > 0.0001f;
					bool flag6 = vector8.y < 0f;
					bool flag7 = Mathf.Abs(vector8.y) > 0.99f;
					bool flag8 = Mathf.Abs(vector8.x) > 0.0001f;
					bool flag9 = vector8.x > 0.0001f;
					bool flag10 = vector8.z > 0.0001f;
					bool flag11 = Mathf.Abs(vector8.x) > 0.1f && Mathf.Abs(vector8.y) > 0.1f && Mathf.Abs(vector8.z) > 0.1f;
					if ((blockFace == BlockFace.Top || blockFace == BlockFace.Bottom) && flag8 && !flag5)
					{
						vector3i.x = uvrectTiling.blockH;
						vector3i.z = uvrectTiling.blockW;
					}
					Vector3 vector9 = voxelMesh.m_Vertices[count + num21] - _drawPos;
					vector9.x = Mathf.Abs(vector9.x);
					vector9.y = Mathf.Abs(vector9.y);
					vector9.z = Mathf.Abs(vector9.z);
					vector7.x = Mathf.Abs(vector7.x);
					vector7.y = Mathf.Abs(vector7.y);
					vector7.z = Mathf.Abs(vector7.z);
					Vector3 vector10 = vector7 + vector9;
					vector10.x = (((int)Mathf.Abs(vector10.x) != vector3i.x) ? (Mathf.Abs(vector10.x) % (float)vector3i.x) : Mathf.Abs(vector10.x));
					vector10.y = (((int)Mathf.Abs(vector10.y) != vector3i.y) ? (Mathf.Abs(vector10.y) % (float)vector3i.y) : Mathf.Abs(vector10.y));
					vector10.z = (((int)Mathf.Abs(vector10.z) != vector3i.z) ? (Mathf.Abs(vector10.z) % (float)vector3i.z) : Mathf.Abs(vector10.z));
					if ((int)vector10.x > vector3i.x)
					{
						vector10.x %= (float)vector3i.x;
					}
					if ((int)vector10.y > vector3i.y)
					{
						vector10.y %= (float)vector3i.y;
					}
					if ((int)vector10.z > vector3i.z)
					{
						vector10.z %= (float)vector3i.z;
					}
					Vector3 vector11 = vector3i.ToVector3() - vector10;
					float num22 = ((int)Mathf.Abs(vector10.x) != vector3i.x) ? (Mathf.Abs(vector10.x) % (float)vector3i.x) : Mathf.Abs(vector10.x);
					float y = ((int)Mathf.Abs(vector10.z) != vector3i.z) ? (Mathf.Abs(vector10.z) % (float)vector3i.z) : Mathf.Abs(vector10.z);
					switch (blockFace)
					{
					case BlockFace.Top:
					case BlockFace.Bottom:
						if (!flag7)
						{
							if (flag8)
							{
								if (flag9)
								{
									if (flag5)
									{
										if (flag10)
										{
											if (flag11)
											{
												zero.x = (vector10.x + vector11.z) * 0.5f;
											}
											else
											{
												zero.x = (float)vector3i.x - num22;
											}
										}
										else if (flag11)
										{
											zero.x = (vector11.x + vector11.z) * 0.5f;
										}
										else
										{
											zero.x = num22;
										}
										zero.y = vector10.y;
									}
									else
									{
										zero.x = vector10.z;
										if (flag6)
										{
											zero.y = vector10.x;
										}
										else
										{
											zero.y = vector11.x;
										}
									}
								}
								else if (flag5)
								{
									if (flag10)
									{
										if (flag11)
										{
											zero.x = (vector11.x + vector11.z) * 0.5f;
										}
										else
										{
											zero.x = (float)vector3i.x - num22;
										}
									}
									else if (flag11)
									{
										zero.x = (vector10.x + vector11.z) * 0.5f;
									}
									else
									{
										zero.x = num22;
									}
									zero.y = vector10.y;
								}
								else
								{
									zero.x = vector11.z;
									if (flag6)
									{
										zero.y = vector11.x;
									}
									else
									{
										zero.y = vector10.x;
									}
								}
							}
							else if (flag10)
							{
								zero.x = vector11.x;
								if (flag6)
								{
									zero.y = vector10.z;
								}
								else
								{
									zero.y = vector11.z;
								}
							}
							else
							{
								zero.x = vector10.x;
								if (flag6)
								{
									zero.y = vector11.z;
								}
								else
								{
									zero.y = vector10.z;
								}
							}
						}
						else
						{
							zero.x = num22;
							zero.y = y;
						}
						break;
					case BlockFace.North:
						zero.x = vector11.x;
						zero.y = vector10.y;
						break;
					case BlockFace.West:
						zero.x = vector11.z;
						zero.y = vector10.y;
						break;
					case BlockFace.South:
						goto IL_15AF;
					case BlockFace.East:
						zero.x = vector10.z;
						zero.y = vector10.y;
						break;
					default:
						goto IL_15AF;
					}
					IL_15CB:
					voxelMesh.m_Uvs.Add(new Vector2(uvrectTiling.uv.x + zero.x * uvrectTiling.uv.width, uvrectTiling.uv.y + zero.y * uvrectTiling.uv.height));
					goto IL_16A6;
					IL_15AF:
					zero.x = vector10.x;
					zero.y = vector10.y;
					goto IL_15CB;
				}
				zero.x = Mathf.Floor(1f / uvrectTiling.uv.width * 1000f);
				zero.y = (float)uvrectTiling.blockW + Mathf.Floor((float)(uvrectTiling.blockH * 1000));
				zero.x += uvrectTiling.uv.x;
				zero.y += uvrectTiling.uv.y;
				voxelMesh.m_Uvs.Add(zero);
				IL_16A6:
				if (voxelMesh.UvsCrack != null)
				{
					voxelMesh.UvsCrack.Add(new Vector2(rect.x + visualMesh.Uvs[num21].x * rect.width, rect.y + visualMesh.Uvs[num21].y * rect.height));
				}
			}
		}
	}

	public override bool IsRenderDecoration()
	{
		return true;
	}

	public override void renderDecorations(Vector3i _worldPos, BlockValue _blockValue, Vector3 _drawPos, Vector3[] _vertices, LightingAround _lightingAround, long _textureFull, VoxelMesh[] _meshes, INeighborBlockCache _nBlocks)
	{
		if (_blockValue.ischild)
		{
			return;
		}
		for (int i = 0; i < this.faceInfo.Length; i++)
		{
			int num = BlockShapeNew.convertRotationCached[(int)_blockValue.rotation, i];
			if (this.visualMeshes[num] != null)
			{
				BlockShapeNew.EnumFaceOcclusionInfo enumFaceOcclusionInfo = this.faceInfo[num];
				if (enumFaceOcclusionInfo != BlockShapeNew.EnumFaceOcclusionInfo.None)
				{
					if (enumFaceOcclusionInfo != BlockShapeNew.EnumFaceOcclusionInfo.OwnFaces)
					{
						if (enumFaceOcclusionInfo != BlockShapeNew.EnumFaceOcclusionInfo.Transparent)
						{
							goto IL_14F;
						}
					}
					else
					{
						BlockFace face = (BlockFace)i;
						Vector3i vector3i = BlockFaceFlags.OffsetIForFace(face);
						BlockValue blockValue = _nBlocks.Get(vector3i.x, vector3i.y + (int)_drawPos.y, vector3i.z);
						Block block = blockValue.Block;
						bool flag = !blockValue.ischild && block.shape.IsSolidCube && !block.shape.IsTerrain();
						if (!flag)
						{
							int num2 = 0;
							switch (face)
							{
							case BlockFace.Top:
								num2 = 2;
								break;
							case BlockFace.Bottom:
								num2 = 1;
								break;
							case BlockFace.North:
								num2 = 16;
								break;
							case BlockFace.West:
								num2 = 32;
								break;
							case BlockFace.South:
								num2 = 4;
								break;
							case BlockFace.East:
								num2 = 8;
								break;
							}
							int facesDrawnFullBitfield = block.shape.getFacesDrawnFullBitfield(blockValue);
							flag = (flag || (facesDrawnFullBitfield & num2) != 0);
						}
						if (flag && this.isRenderFace(_blockValue, face, blockValue))
						{
							this.renderFace(_worldPos, _blockValue, _drawPos, face, _vertices, _lightingAround, _textureFull, _meshes, BlockShape.MeshPurpose.World);
							goto IL_14F;
						}
						goto IL_14F;
					}
				}
				this.renderFace(_worldPos, _blockValue, _drawPos, (BlockFace)i, _vertices, _lightingAround, _textureFull, _meshes, BlockShape.MeshPurpose.World);
			}
			IL_14F:;
		}
	}

	public override void renderFull(Vector3i _worldPos, BlockValue _blockValue, Vector3 _drawPos, Vector3[] _vertices, LightingAround _lightingAround, long _textureFull, VoxelMesh[] _meshes, BlockShape.MeshPurpose _purpose = BlockShape.MeshPurpose.World)
	{
		if (_blockValue.ischild)
		{
			return;
		}
		int num = this.visualMeshes.Length;
		for (int i = 0; i < num; i++)
		{
			this.renderFace(_worldPos, _blockValue, _drawPos, (BlockFace)i, _vertices, _lightingAround, _textureFull, _meshes, _purpose);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static int BlockFaceToRot(BlockFace _blockFace)
	{
		return (int)((int)_blockFace << 2);
	}

	public static BlockFace RotToBlockFace(int _rotation)
	{
		int num = _rotation >> 2 & 7;
		if (num <= 5)
		{
			return (BlockFace)num;
		}
		return BlockFace.Top;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static int RotToLocalRot(int _rotation)
	{
		return _rotation & 3;
	}

	public static Quaternion GetRotationStatic(int _rotation)
	{
		return BlockShapeNew.rotationsToQuats[_rotation];
	}

	public override Quaternion GetRotation(BlockValue _blockValue)
	{
		return BlockShapeNew.rotationsToQuats[(int)_blockValue.rotation];
	}

	public override byte Rotate(bool _bLeft, int _rotation)
	{
		_rotation += (_bLeft ? -1 : 1);
		if (_rotation > 23)
		{
			_rotation = 0;
		}
		if (_rotation < 0)
		{
			_rotation = 23;
		}
		return (byte)_rotation;
	}

	public override Quaternion GetPreviewRotation()
	{
		return Quaternion.AngleAxis(180f, Vector3.up);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	static BlockShapeNew()
	{
		for (int i = 1; i < 4; i++)
		{
			for (byte b = 0; b < 28; b += 1)
			{
				BlockShapeNew.rotations[i - 1, (int)b] = BlockShapeNew.CalcRotation(b, i);
			}
		}
	}

	public override BlockValue RotateY(bool _bLeft, BlockValue _blockValue, int _rotCount)
	{
		if (_rotCount == 0)
		{
			return _blockValue;
		}
		if (_bLeft)
		{
			_rotCount = 4 - _rotCount;
		}
		_blockValue.rotation = BlockShapeNew.rotations[_rotCount - 1, (int)_blockValue.rotation];
		return _blockValue;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static byte CalcRotation(byte _rotation, int _rotCount)
	{
		if (_rotation >= 24)
		{
			for (int i = 0; i < _rotCount; i++)
			{
				_rotation += 1;
				if (_rotation > 27)
				{
					_rotation = 24;
				}
				else if (_rotation < 24)
				{
					_rotation = 27;
				}
			}
		}
		else
		{
			int num = 90 * _rotCount;
			_rotation = (byte)BlockShapeNew.ConvertRotationFree((int)_rotation, Quaternion.AngleAxis((float)num, Vector3.up), false);
		}
		return _rotation;
	}

	public static int MirrorStatic(EnumMirrorAlong _axis, int _rotation, int _symType = 1)
	{
		switch (_symType)
		{
		case 0:
			return _rotation;
		default:
		{
			Quaternion quaternion = BlockShapeNew.GetRotationStatic(_rotation);
			switch (_axis)
			{
			case EnumMirrorAlong.XAxis:
				quaternion.x = -quaternion.x;
				quaternion.w = -quaternion.w;
				break;
			case EnumMirrorAlong.YAxis:
				quaternion.y = -quaternion.y;
				quaternion.w = -quaternion.w;
				quaternion *= Quaternion.AngleAxis(180f, Vector3.forward);
				break;
			case EnumMirrorAlong.ZAxis:
				quaternion.z = -quaternion.z;
				quaternion.w = -quaternion.w;
				quaternion *= Quaternion.AngleAxis(180f, Vector3.up);
				break;
			}
			return BlockShapeNew.ConvertRotationFree(0, quaternion, false);
		}
		case 2:
		{
			BlockFace blockFace = BlockShapeNew.RotToBlockFace(_rotation);
			int num = 0;
			switch (_axis)
			{
			case EnumMirrorAlong.XAxis:
				if (blockFace != BlockFace.West)
				{
					if (blockFace == BlockFace.East)
					{
						blockFace = BlockFace.West;
					}
				}
				else
				{
					blockFace = BlockFace.East;
				}
				break;
			case EnumMirrorAlong.YAxis:
				switch (blockFace)
				{
				case BlockFace.Top:
					blockFace = BlockFace.Bottom;
					break;
				case BlockFace.Bottom:
					blockFace = BlockFace.Top;
					break;
				case BlockFace.North:
				case BlockFace.South:
					num = 1;
					break;
				}
				break;
			case EnumMirrorAlong.ZAxis:
				if (blockFace != BlockFace.North)
				{
					if (blockFace != BlockFace.South)
					{
						num = 1;
					}
					else
					{
						blockFace = BlockFace.North;
					}
				}
				else
				{
					blockFace = BlockFace.South;
				}
				break;
			}
			int num2 = BlockShapeNew.RotToLocalRot(_rotation);
			if (num != 0)
			{
				if (num == 1)
				{
					switch (num2)
					{
					case 0:
						num2 = 1;
						break;
					case 1:
						num2 = 0;
						break;
					case 2:
						num2 = 3;
						break;
					case 3:
						num2 = 2;
						break;
					}
				}
			}
			else
			{
				switch (num2)
				{
				case 0:
					num2 = 3;
					break;
				case 1:
					num2 = 2;
					break;
				case 2:
					num2 = 1;
					break;
				case 3:
					num2 = 0;
					break;
				}
			}
			return BlockShapeNew.BlockFaceToRot(blockFace) | num2;
		}
		case 3:
		{
			BlockFace blockFace2 = BlockShapeNew.RotToBlockFace(_rotation);
			int num3 = 1;
			switch (_axis)
			{
			case EnumMirrorAlong.XAxis:
				if (blockFace2 != BlockFace.West)
				{
					if (blockFace2 == BlockFace.East)
					{
						blockFace2 = BlockFace.West;
					}
				}
				else
				{
					blockFace2 = BlockFace.East;
				}
				break;
			case EnumMirrorAlong.YAxis:
				switch (blockFace2)
				{
				case BlockFace.Top:
					blockFace2 = BlockFace.Bottom;
					break;
				case BlockFace.Bottom:
					blockFace2 = BlockFace.Top;
					break;
				case BlockFace.North:
				case BlockFace.South:
					num3 = 0;
					break;
				}
				break;
			case EnumMirrorAlong.ZAxis:
				if (blockFace2 != BlockFace.North)
				{
					if (blockFace2 != BlockFace.South)
					{
						num3 = 0;
					}
					else
					{
						blockFace2 = BlockFace.North;
					}
				}
				else
				{
					blockFace2 = BlockFace.South;
				}
				break;
			}
			int num4 = BlockShapeNew.RotToLocalRot(_rotation);
			if (num3 != 0)
			{
				if (num3 == 1)
				{
					switch (num4)
					{
					case 0:
						num4 = 1;
						break;
					case 1:
						num4 = 0;
						break;
					case 2:
						num4 = 3;
						break;
					case 3:
						num4 = 2;
						break;
					}
				}
			}
			else
			{
				switch (num4)
				{
				case 0:
					num4 = 3;
					break;
				case 1:
					num4 = 2;
					break;
				case 2:
					num4 = 1;
					break;
				case 3:
					num4 = 0;
					break;
				}
			}
			return BlockShapeNew.BlockFaceToRot(blockFace2) | num4;
		}
		case 4:
			BlockShapeNew.GetRotationStatic(_rotation);
			switch (_axis)
			{
			case EnumMirrorAlong.XAxis:
				_rotation = BlockShapeNew.ConvertRotationFree(_rotation, Quaternion.AngleAxis(180f, Vector3.up), true);
				return BlockShapeNew.ConvertRotationFree(_rotation, Quaternion.AngleAxis(180f, Vector3.right), false);
			case EnumMirrorAlong.YAxis:
				_rotation = BlockShapeNew.ConvertRotationFree(_rotation, Quaternion.AngleAxis(180f, Vector3.up), true);
				return BlockShapeNew.ConvertRotationFree(_rotation, Quaternion.AngleAxis(180f, Vector3.up), false);
			case EnumMirrorAlong.ZAxis:
				_rotation = BlockShapeNew.ConvertRotationFree(_rotation, Quaternion.AngleAxis(180f, Vector3.up), true);
				return BlockShapeNew.ConvertRotationFree(_rotation, Quaternion.AngleAxis(180f, Vector3.forward), false);
			default:
				return _rotation;
			}
			break;
		}
	}

	public BlockFace GetBlockFaceFromColliderTriangle(BlockValue _blockValue, Vector3 _v1, Vector3 _v2, Vector3 _v3)
	{
		int num = this.visualMeshes.Length;
		for (int i = 0; i < num; i++)
		{
			BlockShapeNew.MySimpleMesh colliderMesh = this.getColliderMesh(i, this.getVisualMesh(i));
			if (colliderMesh != null)
			{
				int num2 = BlockShapeNew.convertRotationCached[(int)_blockValue.rotation, i];
				for (int j = 0; j < colliderMesh.Indices.Count; j += 3)
				{
					if ((_v1 - colliderMesh.Vertices[(int)colliderMesh.Indices[j]]).sqrMagnitude < 0.001f && (_v2 - colliderMesh.Vertices[(int)colliderMesh.Indices[j + 1]]).sqrMagnitude < 0.001f && (_v3 - colliderMesh.Vertices[(int)colliderMesh.Indices[j + 2]]).sqrMagnitude < 0.001f)
					{
						for (int k = 0; k < BlockShapeNew.convertRotationCached.GetLength(1); k++)
						{
							if (BlockShapeNew.convertRotationCached[(int)_blockValue.rotation, k] == num2)
							{
								return (BlockFace)k;
							}
						}
					}
				}
			}
		}
		return BlockFace.None;
	}

	public override Vector2 GetPathOffset(int _rotation)
	{
		return this.boundsPathOffsetRotations[_rotation];
	}

	public override float GetStepHeight(BlockValue blockDef, BlockFace crossingFace)
	{
		return (float)(blockDef.Block.IsCollideMovement ? 1 : 0);
	}

	public override Bounds[] GetBounds(BlockValue _blockValue)
	{
		this.boundsArr[0] = this.boundsRotations[(int)_blockValue.rotation];
		return this.boundsArr;
	}

	public override BlockFace GetRotatedBlockFace(BlockValue _blockValue, BlockFace _face)
	{
		return (BlockFace)BlockShapeNew.convertRotationCached[(int)_blockValue.rotation, (int)_face];
	}

	public override void MirrorFace(EnumMirrorAlong _axis, int _sourceRot, int _targetRot, BlockFace _face, out BlockFace _sourceFace, out BlockFace _targetFace)
	{
		_sourceFace = (BlockFace)BlockShapeNew.convertRotationCached[_sourceRot, (int)_face];
		switch (_axis)
		{
		case EnumMirrorAlong.XAxis:
			switch (_face)
			{
			case BlockFace.Top:
				_face = BlockFace.Top;
				break;
			case BlockFace.Bottom:
				_face = BlockFace.Bottom;
				break;
			case BlockFace.North:
				_face = BlockFace.North;
				break;
			case BlockFace.West:
				_face = BlockFace.East;
				break;
			case BlockFace.South:
				_face = BlockFace.South;
				break;
			case BlockFace.East:
				_face = BlockFace.West;
				break;
			}
			break;
		case EnumMirrorAlong.YAxis:
			switch (_face)
			{
			case BlockFace.Top:
				_face = BlockFace.Bottom;
				break;
			case BlockFace.Bottom:
				_face = BlockFace.Top;
				break;
			case BlockFace.North:
				_face = BlockFace.North;
				break;
			case BlockFace.West:
				_face = BlockFace.West;
				break;
			case BlockFace.South:
				_face = BlockFace.South;
				break;
			case BlockFace.East:
				_face = BlockFace.East;
				break;
			}
			break;
		case EnumMirrorAlong.ZAxis:
			switch (_face)
			{
			case BlockFace.Top:
				_face = BlockFace.Top;
				break;
			case BlockFace.Bottom:
				_face = BlockFace.Bottom;
				break;
			case BlockFace.North:
				_face = BlockFace.South;
				break;
			case BlockFace.West:
				_face = BlockFace.West;
				break;
			case BlockFace.South:
				_face = BlockFace.North;
				break;
			case BlockFace.East:
				_face = BlockFace.East;
				break;
			}
			break;
		}
		_targetFace = (BlockFace)BlockShapeNew.convertRotationCached[_targetRot, (int)_face];
	}

	public override int GetVertexCount()
	{
		int num = 0;
		for (int i = 0; i < this.visualMeshes.Length; i++)
		{
			num += ((this.visualMeshes[i] != null) ? this.visualMeshes[i].Vertices.Count : 0);
		}
		return num;
	}

	public override int GetTriangleCount()
	{
		int num = 0;
		for (int i = 0; i < this.visualMeshes.Length; i++)
		{
			num += ((this.visualMeshes[i] != null) ? (this.visualMeshes[i].Indices.Count / 3) : 0);
		}
		return num;
	}

	public override string GetName()
	{
		return this.ShapeName;
	}

	public BlockShapeNew.EnumFaceOcclusionInfo GetFaceInfo(BlockValue _blockValue, BlockFace _face)
	{
		_face = this.GetRotatedBlockFace(_blockValue, _face);
		return this.faceInfo[(int)_face];
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cRotationsUsed = 28;

	public static bool bImposterGenerationActive;

	public string ShapeName;

	[PublicizedFrom(EAccessModifier.Private)]
	public BlockShapeNew.MySimpleMesh[] visualMeshes;

	[PublicizedFrom(EAccessModifier.Private)]
	public BlockShapeNew.MySimpleMesh[] colliderMeshes;

	[PublicizedFrom(EAccessModifier.Private)]
	public BlockShapeNew.EnumFaceOcclusionInfo[] faceInfo;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Vector3 centerOffsetV = new Vector3(-0.5f, -0.5f, -0.5f);

	[PublicizedFrom(EAccessModifier.Private)]
	public Bounds[] boundsRotations;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2[] boundsPathOffsetRotations;

	[PublicizedFrom(EAccessModifier.Private)]
	public static int[,] convertRotationCached;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Dictionary<string, BlockShapeNew.MeshData> meshData = new Dictionary<string, BlockShapeNew.MeshData>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Quaternion[] rotationsToQuats = new Quaternion[]
	{
		Quaternion.AngleAxis(0f, Vector3.up),
		Quaternion.AngleAxis(90f, Vector3.up),
		Quaternion.AngleAxis(180f, Vector3.up),
		Quaternion.AngleAxis(270f, Vector3.up),
		Quaternion.AngleAxis(180f, Vector3.right) * Quaternion.AngleAxis(180f, Vector3.up),
		Quaternion.AngleAxis(180f, Vector3.right) * Quaternion.AngleAxis(270f, Vector3.up),
		Quaternion.AngleAxis(180f, Vector3.right) * Quaternion.AngleAxis(360f, Vector3.up),
		Quaternion.AngleAxis(180f, Vector3.right) * Quaternion.AngleAxis(450f, Vector3.up),
		Quaternion.AngleAxis(90f, Vector3.right) * Quaternion.AngleAxis(180f, Vector3.up),
		Quaternion.AngleAxis(90f, Vector3.right) * Quaternion.AngleAxis(270f, Vector3.up),
		Quaternion.AngleAxis(90f, Vector3.right) * Quaternion.AngleAxis(360f, Vector3.up),
		Quaternion.AngleAxis(90f, Vector3.right) * Quaternion.AngleAxis(450f, Vector3.up),
		Quaternion.AngleAxis(90f, Vector3.forward) * Quaternion.AngleAxis(0f, Vector3.up),
		Quaternion.AngleAxis(90f, Vector3.forward) * Quaternion.AngleAxis(90f, Vector3.up),
		Quaternion.AngleAxis(90f, Vector3.forward) * Quaternion.AngleAxis(180f, Vector3.up),
		Quaternion.AngleAxis(90f, Vector3.forward) * Quaternion.AngleAxis(270f, Vector3.up),
		Quaternion.AngleAxis(270f, Vector3.right) * Quaternion.AngleAxis(0f, Vector3.up),
		Quaternion.AngleAxis(270f, Vector3.right) * Quaternion.AngleAxis(90f, Vector3.up),
		Quaternion.AngleAxis(270f, Vector3.right) * Quaternion.AngleAxis(180f, Vector3.up),
		Quaternion.AngleAxis(270f, Vector3.right) * Quaternion.AngleAxis(270f, Vector3.up),
		Quaternion.AngleAxis(270f, Vector3.forward) * Quaternion.AngleAxis(0f, Vector3.up),
		Quaternion.AngleAxis(270f, Vector3.forward) * Quaternion.AngleAxis(90f, Vector3.up),
		Quaternion.AngleAxis(270f, Vector3.forward) * Quaternion.AngleAxis(180f, Vector3.up),
		Quaternion.AngleAxis(270f, Vector3.forward) * Quaternion.AngleAxis(270f, Vector3.up),
		Quaternion.AngleAxis(45f, Vector3.up),
		Quaternion.AngleAxis(135f, Vector3.up),
		Quaternion.AngleAxis(225f, Vector3.up),
		Quaternion.AngleAxis(315f, Vector3.up),
		Quaternion.identity,
		Quaternion.identity,
		Quaternion.identity,
		Quaternion.identity
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public static byte[,] rotations = new byte[3, 28];

	public enum EnumFaceOcclusionInfo
	{
		None,
		Part,
		Full,
		Remove,
		Continuous,
		RemoveIfAny,
		OwnFaces,
		HideIfSame,
		Transparent
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public class MySimpleMesh
	{
		public List<ushort> Indices = new List<ushort>();

		public List<Vector2> Uvs = new List<Vector2>();

		public List<Vector3> Vertices = new List<Vector3>();

		public List<Vector3> Normals = new List<Vector3>();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public class MeshData
	{
		public GameObject obj;

		public int symTypeOverride = -1;

		public bool IsSolidCube;

		public Dictionary<Vector3, BlockShapeNew.MeshData.Arrays> posArrays = new Dictionary<Vector3, BlockShapeNew.MeshData.Arrays>();

		public class Arrays
		{
			public BlockShapeNew.MySimpleMesh[] meshes = new BlockShapeNew.MySimpleMesh[7];

			public BlockShapeNew.MySimpleMesh[] colliderMeshes = new BlockShapeNew.MySimpleMesh[7];

			public BlockShapeNew.EnumFaceOcclusionInfo[] faceInfo = new BlockShapeNew.EnumFaceOcclusionInfo[7];

			public Bounds[] boundsRotations = new Bounds[32];
		}
	}
}
