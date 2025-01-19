using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class BlockShapeGrassShort : BlockShapeGrass
{
	public override void renderFull(Vector3i _worldPos, BlockValue _blockValue, Vector3 _drawPos, Vector3[] _vertices, LightingAround _lightingAround, long _textureFull, VoxelMesh[] _meshes, BlockShape.MeshPurpose _purpose = BlockShape.MeshPurpose.World)
	{
		Vector3 drawPos;
		drawPos.x = _drawPos.x + (float)((int)_drawPos.z & 1) * 0.2f - 0.1f;
		drawPos.y = _drawPos.y;
		drawPos.z = _drawPos.z + (float)((int)_drawPos.x & 1) * 0.2f - 0.1f;
		byte meta2and = _blockValue.meta2and1;
		BlockShapeBillboardPlant.RenderData data;
		data.offsetY = -0.09f;
		data.scale = 1f;
		data.height = 0.33f;
		switch (meta2and >> 3 & 3)
		{
		case 0:
			data.count = 1 + MeshDescription.GrassQualityPlanes * 2;
			data.count2 = 2;
			data.sideShift = 0.22f;
			break;
		case 1:
			data.count = 2 + MeshDescription.GrassQualityPlanes * 2;
			data.count2 = 2;
			data.sideShift = 0.26f;
			break;
		case 2:
			data.count = 1 + MeshDescription.GrassQualityPlanes;
			data.count2 = 3;
			data.sideShift = 0.18f;
			break;
		default:
			data.count = 2 + MeshDescription.GrassQualityPlanes;
			data.count2 = 3;
			data.sideShift = 0.3f;
			break;
		}
		data.rotation = 10f + (float)(_blockValue.rotation & 7) * 22.5f;
		Block block = _blockValue.Block;
		VoxelMesh mesh = _meshes[(int)block.MeshIndex];
		int num = (int)(_blockValue.meta & 7);
		if (num >= 6)
		{
			num = 0;
		}
		BlockFace side = (BlockFace)num;
		Rect uvrectFromSideAndMetadata = block.getUVRectFromSideAndMetadata((int)block.MeshIndex, side, Vector3.zero, _blockValue);
		uvrectFromSideAndMetadata.height *= 0.33f;
		byte sun = _lightingAround[LightingAround.Pos.Middle].sun;
		byte block2 = _lightingAround[LightingAround.Pos.Middle].block;
		BlockShapeBillboardPlant.RenderGridMesh(mesh, drawPos, _vertices, uvrectFromSideAndMetadata, sun, block2, data);
		BlockShapeBillboardPlant.AddCollider(mesh, _drawPos, 0.5f);
	}
}
