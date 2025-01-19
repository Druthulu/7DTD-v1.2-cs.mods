using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class BlockShapeCubeCutoutBackFaces : BlockShapeCubeCutout
{
	public BlockShapeCubeCutoutBackFaces()
	{
		this.IsSolidCube = false;
	}

	public override int getFacesDrawnFullBitfield(BlockValue _blockValue)
	{
		return 0;
	}

	public override bool isRenderFace(BlockValue _blockValue, BlockFace _face, BlockValue _adjBlockValue)
	{
		return _blockValue.type != _adjBlockValue.type;
	}

	public override void renderFace(Vector3i _worldPos, BlockValue _blockValue, Vector3 _drawPos, BlockFace _face, Vector3[] _vertices, LightingAround _lightingAround, long _textureFull, VoxelMesh[] _meshes, BlockShape.MeshPurpose _purpose = BlockShape.MeshPurpose.World)
	{
		base.renderFace(_worldPos, _blockValue, _drawPos, _face, _vertices, _lightingAround, _textureFull, _meshes, BlockShape.MeshPurpose.World);
		byte meshIndex = _blockValue.Block.MeshIndex;
		Rect uvrectFromSideAndMetadata = this.block.getUVRectFromSideAndMetadata((int)meshIndex, _face, _vertices, _blockValue);
		_meshes[(int)meshIndex].AddQuadWithCracks(_vertices[3], Color.white, _vertices[2], Color.white, _vertices[1], Color.white, _vertices[0], Color.white, uvrectFromSideAndMetadata, WorldConstants.MapDamageToUVRect(_blockValue), false);
	}
}
