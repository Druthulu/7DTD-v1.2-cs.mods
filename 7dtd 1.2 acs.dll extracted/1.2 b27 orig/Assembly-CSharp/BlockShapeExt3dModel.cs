using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class BlockShapeExt3dModel : BlockShapeRotatedAbstract
{
	public BlockShapeExt3dModel()
	{
		this.IsSolidCube = false;
		this.IsSolidSpace = false;
		this.LightOpacity = 0;
	}

	public override void Init(Block _block)
	{
		this.ext3dModelName = _block.Properties.Values["Model"];
		if (this.ext3dModelName == null)
		{
			throw new Exception("No model specified on block with name " + _block.GetBlockName());
		}
		this.modelOffset = Vector3.zero;
		if (_block.Properties.Values.ContainsKey("ModelOffset"))
		{
			this.modelOffset = StringParsers.ParseVector3(_block.Properties.Values["ModelOffset"], 0, -1);
		}
		base.Init(_block);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void createBoundingBoxes()
	{
		if (this.modelMesh == null || this.modelMesh.boundingBoxMesh.Vertices.Count > 0)
		{
			return;
		}
		base.createBoundingBoxes();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void createVertices()
	{
		TextureAtlasExternalModels textureAtlasExternalModels = (MeshDescription.meshes != null) ? ((TextureAtlasExternalModels)MeshDescription.meshes[0].textureAtlas) : null;
		if (textureAtlasExternalModels == null)
		{
			return;
		}
		if (!textureAtlasExternalModels.Meshes.ContainsKey(this.ext3dModelName))
		{
			throw new Exception("External 3D model with name '" + this.ext3dModelName + "' not found! Maybe you need to create the atlas first?");
		}
		this.modelMesh = ((TextureAtlasExternalModels)MeshDescription.meshes[0].textureAtlas).Meshes[this.ext3dModelName];
		this.vertices = this.modelMesh.Vertices.ToArray();
		this.normals = this.modelMesh.Normals.ToArray();
		if (this.modelMesh != null && this.modelMesh.aabb != null && this.modelMesh.aabb.Length != 0)
		{
			this.boundsArr = new Bounds[this.modelMesh.aabb.Length];
			for (int i = 0; i < this.modelMesh.aabb.Length; i++)
			{
				this.boundsArr[i] = new Bounds(this.modelMesh.aabb[i].center, this.modelMesh.aabb[i].size);
			}
		}
	}

	public override Quaternion GetRotation(BlockValue _blockValue)
	{
		return BlockShapeNew.GetRotationStatic((int)_blockValue.rotation);
	}

	public override void renderFull(Vector3i _worldPos, BlockValue _blockValue, Vector3 _drawPos, Vector3[] _vertices, LightingAround _lightingAround, long _textureFull, VoxelMesh[] _meshes, BlockShape.MeshPurpose _purpose = BlockShape.MeshPurpose.World)
	{
		byte sun = _lightingAround[LightingAround.Pos.Middle].sun;
		byte block = _lightingAround[LightingAround.Pos.Middle].block;
		Vector3[] array = base.rotateVertices(this.vertices, _drawPos + this.modelOffset, _blockValue);
		Vector3[] array2 = this.rotateNormals(this.normals, _blockValue);
		Block block2 = _blockValue.Block;
		byte meshIndex = block2.MeshIndex;
		_meshes[(int)meshIndex].CheckVertexLimit(array.Length);
		_meshes[(int)meshIndex].AddMesh(_drawPos, this.vertices.Length, array, array2, this.modelMesh.Indices, this.modelMesh.Uvs, sun, block, this.GetBoundsMesh(_blockValue), (int)(10f * (float)_blockValue.damage) / block2.MaxDamage);
		MemoryPools.poolVector3.Free(array);
		MemoryPools.poolVector3.Free(array2);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector3[] rotateNormals(Vector3[] _normals, BlockValue _blockValue)
	{
		Quaternion rotation = this.GetRotation(_blockValue);
		Vector3[] array = MemoryPools.poolVector3.Alloc(_normals.Length);
		for (int i = 0; i < _normals.Length; i++)
		{
			array[i] = rotation * _normals[i];
		}
		return array;
	}

	public override float GetStepHeight(BlockValue _blockValue, BlockFace crossingFace)
	{
		if (_blockValue.Block.HasTag(BlockTags.Door) || _blockValue.Block.HasTag(BlockTags.Window))
		{
			return 0f;
		}
		return base.GetStepHeight(_blockValue, crossingFace);
	}

	public override bool IsMovementBlocked(BlockValue _blockValue, BlockFace crossingFace)
	{
		return _blockValue.Block.HasTag(BlockTags.Door) || _blockValue.Block.HasTag(BlockTags.Window) || base.IsMovementBlocked(_blockValue, crossingFace);
	}

	public override byte Rotate(bool _bLeft, int _rotation)
	{
		_rotation += (_bLeft ? -1 : 1);
		if (_rotation > 9)
		{
			_rotation = 0;
		}
		if (_rotation < 0)
		{
			_rotation = 9;
		}
		return (byte)_rotation;
	}

	public override BlockValue RotateY(bool _bLeft, BlockValue _blockValue, int _rotCount)
	{
		if (_bLeft)
		{
			_rotCount = -_rotCount;
		}
		int rotation = (int)_blockValue.rotation;
		if (rotation >= 24)
		{
			_blockValue.rotation = (byte)((rotation - 24 + _rotCount & 3) + 24);
		}
		else
		{
			int num = 90 * _rotCount;
			_blockValue.rotation = (byte)BlockShapeNew.ConvertRotationFree(rotation, Quaternion.AngleAxis((float)num, Vector3.up), false);
		}
		return _blockValue;
	}

	public override Quaternion GetPreviewRotation()
	{
		return Quaternion.AngleAxis(180f, Vector3.up);
	}

	public override VoxelMesh GetBoundsMesh(BlockValue _blockValue)
	{
		VoxelMesh voxelMesh = this.cachedRotatedBoundsMeshes[(int)_blockValue.rotation];
		if (voxelMesh == null)
		{
			Vector3[] vertices = this.modelMesh.boundingBoxMesh.Vertices.ToArray();
			Vector3[] array = base.rotateVertices(vertices, Vector3.zero, _blockValue);
			for (int i = 0; i < array.Length; i++)
			{
				array[i] += this.modelOffset;
			}
			voxelMesh = new VoxelMesh(-1, 0, VoxelMesh.CreateFlags.Default);
			voxelMesh.Vertices.AddRange(array, 0, array.Length);
			voxelMesh.Indices = this.modelMesh.boundingBoxMesh.Indices;
			MemoryPools.poolVector3.Free(array);
			this.cachedRotatedBoundsMeshes[(int)_blockValue.rotation] = voxelMesh;
		}
		return voxelMesh;
	}

	public string ext3dModelName;

	[PublicizedFrom(EAccessModifier.Protected)]
	public VoxelMeshExt3dModel modelMesh;

	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector3[] normals;

	[PublicizedFrom(EAccessModifier.Private)]
	public VoxelMesh[] cachedRotatedBoundsMeshes = new VoxelMesh[32];

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 modelOffset;
}
