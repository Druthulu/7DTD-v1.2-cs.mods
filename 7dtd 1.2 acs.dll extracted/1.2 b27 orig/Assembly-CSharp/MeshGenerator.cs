using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Unity.Profiling;
using UnityEngine;

public class MeshGenerator : IMeshGenerator
{
	public MeshGenerator(INeighborBlockCache _nBlocks)
	{
		this.nBlocks = _nBlocks;
	}

	public void GenerateMesh(Vector3i _chunkPos, int _layerIdx, VoxelMesh[] _meshes)
	{
		Vector3i start = new Vector3i(0, Utils.FastMax(0, _layerIdx * 16), 0);
		Vector3i end = new Vector3i(15, _layerIdx * 16 + 16 - 1, 15);
		this.CreateMesh(_chunkPos, Vector3.zero, start, end, _meshes, true, false);
		for (int i = 0; i < _meshes.Length; i++)
		{
			VoxelMesh voxelMesh = _meshes[i];
			voxelMesh.Finished();
			if (voxelMesh.m_Uvs.Count > 0 && voxelMesh.m_Normals.Count > 0 && voxelMesh.m_Tangents.Count == 0)
			{
				Utils.CalculateMeshTangents(voxelMesh, i == 5);
			}
		}
	}

	public virtual bool IsLayerEmpty(int _layerIdx)
	{
		return this.IsLayerEmpty(_layerIdx, _layerIdx);
	}

	public virtual bool IsLayerEmpty(int _startLayerIdx, int _endLayerIdx)
	{
		return this.mcLayerIsEmpty(_startLayerIdx, _endLayerIdx);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool mcLayerIsEmpty(int _startLayerIdx, int _endLayerIdx)
	{
		int num = _startLayerIdx * 4;
		int num2 = (_endLayerIdx + 1) * 4;
		this.nBlocks.Init(0, 0);
		IChunk neighborChunk = this.nBlocks.GetNeighborChunk(0, 0);
		if (neighborChunk.IsOnlyTerrainLayer(num) || neighborChunk.IsEmptyLayer(num))
		{
			IChunk neighborChunk2 = this.nBlocks.GetNeighborChunk(1, 0);
			IChunk neighborChunk3 = this.nBlocks.GetNeighborChunk(-1, 0);
			IChunk neighborChunk4 = this.nBlocks.GetNeighborChunk(0, 1);
			IChunk neighborChunk5 = this.nBlocks.GetNeighborChunk(0, -1);
			for (int i = num - 1; i <= num2 + 1; i++)
			{
				if ((!neighborChunk.IsOnlyTerrainLayer(i) && !neighborChunk.IsEmptyLayer(i)) || (!neighborChunk2.IsOnlyTerrainLayer(i) && !neighborChunk2.IsEmptyLayer(i)) || (!neighborChunk3.IsOnlyTerrainLayer(i) && !neighborChunk3.IsEmptyLayer(i)) || (!neighborChunk4.IsOnlyTerrainLayer(i) && !neighborChunk4.IsEmptyLayer(i)) || (!neighborChunk5.IsOnlyTerrainLayer(i) && !neighborChunk5.IsEmptyLayer(i)))
				{
					return false;
				}
			}
			return true;
		}
		return false;
	}

	public void GenerateMesh(Vector3i _worldStartPos, Vector3i _worldEndPos, VoxelMesh[] _meshes)
	{
		this.CreateMesh(Vector3i.zero, Vector3.zero, _worldStartPos, _worldEndPos, _meshes, true, false);
		for (int i = 0; i < _meshes.Length; i++)
		{
			_meshes[i].Finished();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Lighting maxLight(Lighting _lMiddle, Lighting _lL1N1, Lighting _lL1N2, Lighting _lL1C, Lighting _lL2, Lighting _lL2N1, Lighting _lL2N2, Lighting _lL2C)
	{
		int num = 0;
		int num2 = 0;
		if (_lL1N1.sun != 0)
		{
			num += (int)_lL1N1.sun;
			num2++;
		}
		if (_lL1N2.sun != 0)
		{
			num += (int)_lL1N2.sun;
			num2++;
		}
		if (_lL1C.sun != 0)
		{
			num += (int)_lL1C.sun;
			num2++;
		}
		if (_lMiddle.sun != 0)
		{
			num += (int)_lMiddle.sun;
			num2++;
		}
		if (_lL2.sun != 0)
		{
			num += (int)_lL2.sun;
			num2++;
		}
		if (_lL2N1.sun != 0)
		{
			num += (int)_lL2N1.sun;
			num2++;
		}
		if (_lL2N2.sun != 0)
		{
			num += (int)_lL2N2.sun;
			num2++;
		}
		if (_lL2C.sun != 0)
		{
			num += (int)_lL2C.sun;
			num2++;
		}
		Lighting result;
		result.sun = (byte)((float)num / (float)num2);
		result.block = 0;
		result.stability = 0;
		return result;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SwizzleCopy(ref Vector3[] to, int index0, int index1, int index2, int index3)
	{
		to[0] = this.verticesCube[index0];
		to[1] = this.verticesCube[index1];
		to[2] = this.verticesCube[index2];
		to[3] = this.verticesCube[index3];
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RenderLiquidTriNorth(Vector3[] v4Arr, VoxelMesh[] _meshes, int y)
	{
		Vector2 uvdata = new Vector2(1f, 0f);
		bool flag = this.nBlocks.IsWater(1, y + 1, -1);
		bool flag2 = this.nBlocks.IsWater(-1, y + 1, -1);
		if (flag)
		{
			this.cpyVerts[0] = v4Arr[0];
			this.cpyVerts[1] = v4Arr[1];
			this.cpyVerts[2] = v4Arr[3];
			this.cpyVerts[3] = v4Arr[0];
			Vector3[] array = this.cpyVerts;
			int num = 1;
			array[num].x = array[num].x + 0.75f;
			for (int i = 0; i < 4; i++)
			{
				Vector3[] array2 = this.cpyVerts;
				int num2 = i;
				array2[num2].y = array2[num2].y + -0.5f;
			}
			WaterMeshUtils.RenderFace(this.cpyVerts, this.lightingAround, 0L, _meshes, uvdata, false);
		}
		if (flag2)
		{
			this.cpyVerts[0] = v4Arr[0];
			this.cpyVerts[1] = v4Arr[1];
			this.cpyVerts[2] = v4Arr[2];
			Vector3[] array3 = this.cpyVerts;
			int num3 = 0;
			array3[num3].x = array3[num3].x - 0.75f;
			this.cpyVerts[3] = this.cpyVerts[0];
			for (int j = 0; j < 4; j++)
			{
				Vector3[] array4 = this.cpyVerts;
				int num4 = j;
				array4[num4].y = array4[num4].y + -0.5f;
			}
			WaterMeshUtils.RenderFace(this.cpyVerts, this.lightingAround, 0L, _meshes, uvdata, false);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RenderLiquidTriSouth(Vector3[] v4Arr, VoxelMesh[] _meshes, int y)
	{
		Vector2 uvdata = new Vector2(1f, 0f);
		bool flag = this.nBlocks.IsWater(1, y + 1, 1);
		bool flag2 = this.nBlocks.IsWater(-1, y + 1, 1);
		if (flag)
		{
			this.cpyVerts[0] = v4Arr[0];
			this.cpyVerts[1] = v4Arr[1];
			this.cpyVerts[2] = v4Arr[2];
			Vector3[] array = this.cpyVerts;
			int num = 0;
			array[num].x = array[num].x + 0.75f;
			this.cpyVerts[3] = this.cpyVerts[0];
			for (int i = 0; i < 4; i++)
			{
				Vector3[] array2 = this.cpyVerts;
				int num2 = i;
				array2[num2].y = array2[num2].y + -0.5f;
			}
			WaterMeshUtils.RenderFace(this.cpyVerts, this.lightingAround, 0L, _meshes, uvdata, false);
		}
		if (flag2)
		{
			this.cpyVerts[0] = v4Arr[0];
			this.cpyVerts[1] = v4Arr[1];
			this.cpyVerts[2] = v4Arr[3];
			Vector3[] array3 = this.cpyVerts;
			int num3 = 1;
			array3[num3].x = array3[num3].x - 0.75f;
			this.cpyVerts[3] = v4Arr[0];
			for (int j = 0; j < 4; j++)
			{
				Vector3[] array4 = this.cpyVerts;
				int num4 = j;
				array4[num4].y = array4[num4].y + -0.5f;
			}
			WaterMeshUtils.RenderFace(this.cpyVerts, this.lightingAround, 0L, _meshes, uvdata, false);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RenderLiquidTriEast(Vector3[] v4Arr, VoxelMesh[] _meshes, int y)
	{
		Vector2 uvdata = new Vector2(0f, 1f);
		bool flag = this.nBlocks.IsWater(-1, y + 1, 1);
		bool flag2 = this.nBlocks.IsWater(-1, y + 1, -1);
		if (flag)
		{
			this.cpyVerts[0] = v4Arr[0];
			this.cpyVerts[1] = v4Arr[1];
			this.cpyVerts[2] = v4Arr[2];
			Vector3[] array = this.cpyVerts;
			int num = 0;
			array[num].z = array[num].z + 0.75f;
			this.cpyVerts[3] = this.cpyVerts[0];
			for (int i = 0; i < 4; i++)
			{
				Vector3[] array2 = this.cpyVerts;
				int num2 = i;
				array2[num2].y = array2[num2].y + -0.5f;
			}
			WaterMeshUtils.RenderFace(this.cpyVerts, this.lightingAround, 0L, _meshes, uvdata, false);
		}
		if (flag2)
		{
			this.cpyVerts[0] = v4Arr[0];
			this.cpyVerts[1] = v4Arr[1];
			this.cpyVerts[2] = v4Arr[3];
			Vector3[] array3 = this.cpyVerts;
			int num3 = 1;
			array3[num3].z = array3[num3].z - 0.75f;
			this.cpyVerts[3] = v4Arr[0];
			for (int j = 0; j < 4; j++)
			{
				Vector3[] array4 = this.cpyVerts;
				int num4 = j;
				array4[num4].y = array4[num4].y + -0.5f;
			}
			WaterMeshUtils.RenderFace(this.cpyVerts, this.lightingAround, 0L, _meshes, uvdata, false);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RenderLiquidTriWest(Vector3[] v4Arr, VoxelMesh[] _meshes, int y)
	{
		Vector2 uvdata = new Vector2(0f, 1f);
		bool flag = this.nBlocks.IsWater(1, y + 1, 1);
		bool flag2 = this.nBlocks.IsWater(1, y + 1, -1);
		if (flag)
		{
			this.cpyVerts[0] = v4Arr[0];
			this.cpyVerts[1] = v4Arr[1];
			this.cpyVerts[2] = v4Arr[3];
			this.cpyVerts[3] = v4Arr[0];
			Vector3[] array = this.cpyVerts;
			int num = 1;
			array[num].z = array[num].z + 0.75f;
			for (int i = 0; i < 4; i++)
			{
				Vector3[] array2 = this.cpyVerts;
				int num2 = i;
				array2[num2].y = array2[num2].y + -0.5f;
			}
			WaterMeshUtils.RenderFace(this.cpyVerts, this.lightingAround, 0L, _meshes, uvdata, false);
		}
		if (flag2)
		{
			this.cpyVerts[0] = v4Arr[0];
			this.cpyVerts[1] = v4Arr[1];
			this.cpyVerts[2] = v4Arr[2];
			Vector3[] array3 = this.cpyVerts;
			int num3 = 0;
			array3[num3].z = array3[num3].z - 0.75f;
			this.cpyVerts[3] = this.cpyVerts[0];
			for (int j = 0; j < 4; j++)
			{
				Vector3[] array4 = this.cpyVerts;
				int num4 = j;
				array4[num4].y = array4[num4].y + -0.5f;
			}
			WaterMeshUtils.RenderFace(this.cpyVerts, this.lightingAround, 0L, _meshes, uvdata, false);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RenderLiquidFaceNorth(Vector3[] v4Arr, VoxelMesh[] _meshes, int cpyX, int cpyZ)
	{
		Vector2 uvdata = new Vector2(1f, 0f);
		this.cpyVerts[0].z = v4Arr[0].z;
		this.cpyVerts[1].z = v4Arr[1].z;
		this.cpyVerts[2].z = v4Arr[2].z;
		this.cpyVerts[3].z = v4Arr[3].z;
		this.cpyVerts[0].x = v4Arr[0].x - (float)cpyZ * 0.25f;
		this.cpyVerts[1].x = this.cpyVerts[0].x - 0.25f;
		this.cpyVerts[2].x = this.cpyVerts[0].x - 0.25f;
		this.cpyVerts[3].x = v4Arr[0].x - (float)cpyZ * 0.25f;
		this.cpyVerts[0].y = -0.5f + v4Arr[0].y + (float)cpyX * 0.25f;
		this.cpyVerts[1].y = -0.5f + v4Arr[0].y + (float)cpyX * 0.25f;
		this.cpyVerts[2].y = this.cpyVerts[0].y + 0.25f;
		this.cpyVerts[3].y = this.cpyVerts[0].y + 0.25f;
		WaterMeshUtils.RenderFace(this.cpyVerts, this.lightingAround, 0L, _meshes, uvdata, false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RenderLiquidFaceSouth(Vector3[] v4Arr, VoxelMesh[] _meshes, int cpyX, int cpyZ)
	{
		Vector2 uvdata = new Vector2(1f, 0f);
		this.cpyVerts[0].z = v4Arr[0].z;
		this.cpyVerts[1].z = v4Arr[1].z;
		this.cpyVerts[2].z = v4Arr[2].z;
		this.cpyVerts[3].z = v4Arr[3].z;
		this.cpyVerts[0].x = v4Arr[0].x + (float)cpyZ * 0.25f;
		this.cpyVerts[1].x = this.cpyVerts[0].x + 0.25f;
		this.cpyVerts[2].x = this.cpyVerts[0].x + 0.25f;
		this.cpyVerts[3].x = v4Arr[0].x + (float)cpyZ * 0.25f;
		this.cpyVerts[0].y = -0.5f + v4Arr[0].y + (float)cpyX * 0.25f;
		this.cpyVerts[1].y = -0.5f + v4Arr[0].y + (float)cpyX * 0.25f;
		this.cpyVerts[2].y = this.cpyVerts[0].y + 0.25f;
		this.cpyVerts[3].y = this.cpyVerts[0].y + 0.25f;
		WaterMeshUtils.RenderFace(this.cpyVerts, this.lightingAround, 0L, _meshes, uvdata, false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RenderLiquidFaceEast(Vector3[] v4Arr, VoxelMesh[] _meshes, int cpyX, int cpyZ)
	{
		Vector2 uvdata = new Vector2(0f, 1f);
		this.cpyVerts[0].x = v4Arr[0].x;
		this.cpyVerts[1].x = v4Arr[1].x;
		this.cpyVerts[2].x = v4Arr[2].x;
		this.cpyVerts[3].x = v4Arr[3].x;
		this.cpyVerts[0].z = v4Arr[0].z + (float)cpyZ * 0.25f;
		this.cpyVerts[1].z = this.cpyVerts[0].z + 0.25f;
		this.cpyVerts[2].z = this.cpyVerts[0].z + 0.25f;
		this.cpyVerts[3].z = v4Arr[0].z + (float)cpyZ * 0.25f;
		this.cpyVerts[0].y = -0.5f + v4Arr[0].y + (float)cpyX * 0.25f;
		this.cpyVerts[1].y = -0.5f + v4Arr[0].y + (float)cpyX * 0.25f;
		this.cpyVerts[2].y = this.cpyVerts[0].y + 0.25f;
		this.cpyVerts[3].y = this.cpyVerts[0].y + 0.25f;
		WaterMeshUtils.RenderFace(this.cpyVerts, this.lightingAround, 0L, _meshes, uvdata, false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RenderLiquidFaceWest(Vector3[] v4Arr, VoxelMesh[] _meshes, int cpyX, int cpyZ)
	{
		Vector2 uvdata = new Vector2(0f, 1f);
		this.cpyVerts[0].x = v4Arr[0].x;
		this.cpyVerts[1].x = v4Arr[1].x;
		this.cpyVerts[2].x = v4Arr[2].x;
		this.cpyVerts[3].x = v4Arr[3].x;
		this.cpyVerts[0].z = v4Arr[0].z - (float)cpyZ * 0.25f;
		this.cpyVerts[1].z = this.cpyVerts[0].z - 0.25f;
		this.cpyVerts[2].z = this.cpyVerts[0].z - 0.25f;
		this.cpyVerts[3].z = v4Arr[0].z - (float)cpyZ * 0.25f;
		this.cpyVerts[0].y = -0.5f + v4Arr[0].y + (float)cpyX * 0.25f;
		this.cpyVerts[1].y = -0.5f + v4Arr[0].y + (float)cpyX * 0.25f;
		this.cpyVerts[2].y = this.cpyVerts[0].y + 0.25f;
		this.cpyVerts[3].y = this.cpyVerts[0].y + 0.25f;
		WaterMeshUtils.RenderFace(this.cpyVerts, this.lightingAround, 0L, _meshes, uvdata, false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool IsAir(int type)
	{
		return type == 0;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool IsPlant(int type)
	{
		Block block = Block.list[type];
		return block != null && block.blockMaterial != null && block.blockMaterial.IsPlant;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool IsTerrain(int type)
	{
		Block block = Block.list[type];
		return block != null && block.shape != null && block.shape.IsTerrain();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool IsSolidCube(int type)
	{
		Block block = Block.list[type];
		return block != null && block.shape != null && block.shape.IsSolidCube;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DrawSide(BlockFace _face, BlockValue midBV, INeighborBlockCache nBlocks, Vector3[] v4Arr, float biomeTemperature, ref bool isWaterTopDrawn, ref bool isWaterLayerStarted, Vector3 drawPos, VoxelMesh[] _meshes, Vector3i posI, Vector3i _worldPos)
	{
		int x = posI.x;
		int y = posI.y;
		int z = posI.z;
		int num = 0;
		int num2 = 0;
		switch (_face)
		{
		case BlockFace.North:
			num2 = -1;
			break;
		case BlockFace.West:
			num = 1;
			break;
		case BlockFace.South:
			num2 = 1;
			break;
		case BlockFace.East:
			num = -1;
			break;
		}
		BlockValue blockValue = nBlocks.Get(num, y, num2);
		Block block = blockValue.Block;
		if (block != null)
		{
			byte stab = nBlocks.GetStab(num, y, num2);
			this.lightingAround.SetStab(stab);
			bool flag = nBlocks.IsWater(num, y, num2) && MeshGenerator.FacePermitsFlow(blockValue, _face);
			if (flag && (this.IsTerrain(midBV.type) || (!this.IsSolidCube(midBV.type) && !this.IsPlant(midBV.type) && !midBV.isair)))
			{
				bool flag2 = isWaterLayerStarted;
				isWaterTopDrawn = true;
				isWaterLayerStarted = true;
				if (!flag2 && !nBlocks.IsWater(num, y + 1, num2))
				{
					this.SwizzleCopy(ref v4Arr, 1, 2, 6, 5);
					this.RenderTopWater(midBV, v4Arr, _meshes, posI, _worldPos, true);
				}
			}
			switch (_face)
			{
			case BlockFace.North:
				this.SwizzleCopy(ref v4Arr, 3, 0, 1, 2);
				break;
			case BlockFace.West:
				this.SwizzleCopy(ref v4Arr, 7, 3, 2, 6);
				break;
			case BlockFace.South:
				this.SwizzleCopy(ref v4Arr, 4, 7, 6, 5);
				break;
			case BlockFace.East:
				this.SwizzleCopy(ref v4Arr, 0, 4, 5, 1);
				break;
			}
			if (block.shape.isRenderFace(blockValue, _face, midBV))
			{
				Vector3 drawPos2 = drawPos + Vector3.forward;
				switch (_face)
				{
				case BlockFace.North:
					drawPos2 = drawPos - Vector3.forward;
					break;
				case BlockFace.West:
					drawPos2 = drawPos + Vector3.right;
					break;
				case BlockFace.South:
					drawPos2 = drawPos + Vector3.forward;
					break;
				case BlockFace.East:
					drawPos2 = drawPos - Vector3.right;
					break;
				}
				IChunk chunk = nBlocks.GetChunk(num, num2);
				int x2 = World.toBlockXZ(x + num);
				int z2 = World.toBlockXZ(z + num2);
				long textureFull = chunk.GetTextureFull(x2, y, z2);
				block.shape.renderFace(_worldPos, blockValue, drawPos2, _face, v4Arr, this.lightingAround, textureFull, _meshes, BlockShape.MeshPurpose.World);
			}
			if (flag && !isWaterLayerStarted)
			{
				bool flag3 = nBlocks.IsWater(num, y + 1, num2);
				Block block2 = nBlocks.Get(num, y + 1, num2).Block;
				if (flag3 || block2.shape.IsTerrain())
				{
					for (int i = 0; i < 4; i++)
					{
						for (int j = 0; j < 4; j++)
						{
							switch (_face)
							{
							case BlockFace.North:
								this.RenderLiquidFaceNorth(v4Arr, _meshes, i, j);
								break;
							case BlockFace.West:
								this.RenderLiquidFaceWest(v4Arr, _meshes, i, j);
								break;
							case BlockFace.South:
								this.RenderLiquidFaceSouth(v4Arr, _meshes, i, j);
								break;
							case BlockFace.East:
								this.RenderLiquidFaceEast(v4Arr, _meshes, i, j);
								break;
							}
						}
					}
					return;
				}
				switch (_face)
				{
				case BlockFace.North:
					this.RenderLiquidTriNorth(v4Arr, _meshes, y);
					return;
				case BlockFace.West:
					this.RenderLiquidTriWest(v4Arr, _meshes, y);
					break;
				case BlockFace.South:
					this.RenderLiquidTriSouth(v4Arr, _meshes, y);
					return;
				case BlockFace.East:
					this.RenderLiquidTriEast(v4Arr, _meshes, y);
					return;
				default:
					return;
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool FacePermitsFlow(BlockValue bv, BlockFace worldspaceFace)
	{
		return (bv.rotatedWaterFlowMask & BlockFaceFlags.FromBlockFace(worldspaceFace)) == BlockFaceFlag.None;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool FacePermitsFlow(BlockFaceFlag flags, BlockFace worldspaceFace)
	{
		return (flags & BlockFaceFlags.FromBlockFace(worldspaceFace)) == BlockFaceFlag.None;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void CreateMesh(Vector3i _worldPos, Vector3 _drawPosOffset, Vector3i _start, Vector3i _end, VoxelMesh[] _meshes, bool _bCalcAmbientLight, bool _bOnlyDistortVertices)
	{
		if (this.mcLayerIsEmpty(_start.y / 16, _end.y / 16))
		{
			return;
		}
		World world = GameManager.Instance.World;
		Vector3 zero = Vector3.zero;
		Vector3i zero2 = Vector3i.zero;
		Vector3[] array = new Vector3[4];
		this.lightCube.SetBlockCache(this.nBlocks);
		for (int i = _start.x; i <= _end.x; i++)
		{
			for (int j = _start.z; j <= _end.z; j++)
			{
				float temperature = VoxelMesh.GetTemperature(world.GetBiome(_worldPos.x + i, _worldPos.z + j));
				this.nBlocks.Init(i, j);
				int y = _start.y;
				this.heights[0] = (int)this.nBlocks.GetChunk(0, 0).GetHeight(ChunkBlockLayerLegacy.CalcOffset(i, j));
				this.heights[1] = (int)this.nBlocks.GetChunk(1, 0).GetHeight(ChunkBlockLayerLegacy.CalcOffset(i + 1, j));
				this.heights[2] = (int)this.nBlocks.GetChunk(0, -1).GetHeight(ChunkBlockLayerLegacy.CalcOffset(i, j - 1));
				this.heights[3] = (int)this.nBlocks.GetChunk(-1, 0).GetHeight(ChunkBlockLayerLegacy.CalcOffset(i - 1, j));
				this.heights[4] = (int)this.nBlocks.GetChunk(0, 1).GetHeight(ChunkBlockLayerLegacy.CalcOffset(i, j + 1));
				this.heights[5] = (this.heights[6] = (this.heights[7] = (this.heights[8] = 0)));
				int num = this.heights[0];
				for (int k = 1; k < this.heights.Length; k++)
				{
					if (num < this.heights[k])
					{
						num = this.heights[k];
					}
				}
				num++;
				int num2 = Utils.FastMin(_end.y, num + 1);
				num2 = Utils.FastMin(254, num2);
				bool flag = false;
				for (int l = num2; l >= y; l--)
				{
					BlockValue blockValue = this.nBlocks.Get(0, l, 0);
					Block block = blockValue.Block;
					if (block != null && (blockValue.ischild || !block.shape.IsSolidCube || block.shape.IsTerrain() || (l > 0 && this.nBlocks.IsWater(0, l - 1, 0) && !this.nBlocks.IsWater(-1, l, 0) && !this.nBlocks.IsWater(1, l, 0) && !this.nBlocks.IsWater(0, l, -1) && !this.nBlocks.IsWater(0, l, 1))))
					{
						bool flag2 = this.nBlocks.IsWater(0, l, 0);
						if (!flag && flag2)
						{
							flag = true;
						}
						zero.x = _drawPosOffset.x + (float)i;
						zero.y = _drawPosOffset.y + (float)l;
						zero.z = _drawPosOffset.z + (float)j;
						zero2.x = i;
						zero2.y = l;
						zero2.z = j;
						for (int m = 0; m < 8; m++)
						{
							this.verticesCube[m].x = BlockShapeCube.Cube[m].x + zero.x;
							this.verticesCube[m].y = BlockShapeCube.Cube[m].y + zero.y;
							this.verticesCube[m].z = BlockShapeCube.Cube[m].z + zero.z;
						}
						if (!_bOnlyDistortVertices)
						{
							this.lightCube.SetPosition(zero2);
							Lighting lighting = this.lightCube[0, 0, 0];
							this.lightingAround[LightingAround.Pos.Middle] = lighting;
							this.lightingAround[LightingAround.Pos.X0Y0Z0] = this.maxLight(lighting, this.lightCube[0, 0, -1], this.lightCube[-1, 0, 0], this.lightCube[-1, 0, -1], this.lightCube[0, -1, 0], this.lightCube[0, -1, -1], this.lightCube[-1, -1, 0], this.lightCube[-1, -1, -1]);
							this.lightingAround[LightingAround.Pos.X1Y0Z0] = this.maxLight(lighting, this.lightCube[0, 0, -1], this.lightCube[1, 0, 0], this.lightCube[1, 0, -1], this.lightCube[0, -1, 0], this.lightCube[0, -1, -1], this.lightCube[1, -1, 0], this.lightCube[1, -1, -1]);
							this.lightingAround[LightingAround.Pos.X1Y0Z1] = this.maxLight(lighting, this.lightCube[0, 0, 1], this.lightCube[1, 0, 0], this.lightCube[1, 0, 1], this.lightCube[0, -1, 0], this.lightCube[0, -1, 1], this.lightCube[1, -1, 0], this.lightCube[1, -1, 1]);
							this.lightingAround[LightingAround.Pos.X0Y0Z1] = this.maxLight(lighting, this.lightCube[0, 0, 1], this.lightCube[-1, 0, 0], this.lightCube[-1, 0, 1], this.lightCube[0, -1, 0], this.lightCube[0, -1, 1], this.lightCube[-1, -1, 0], this.lightCube[-1, -1, 1]);
							this.lightingAround[LightingAround.Pos.X0Y1Z0] = this.maxLight(lighting, this.lightCube[0, 0, -1], this.lightCube[-1, 0, 0], this.lightCube[-1, 0, -1], this.lightCube[0, 1, 0], this.lightCube[0, 1, -1], this.lightCube[-1, 1, 0], this.lightCube[-1, 1, -1]);
							this.lightingAround[LightingAround.Pos.X1Y1Z0] = this.maxLight(lighting, this.lightCube[0, 0, -1], this.lightCube[1, 0, 0], this.lightCube[1, 0, -1], this.lightCube[0, 1, 0], this.lightCube[0, 1, -1], this.lightCube[1, 1, 0], this.lightCube[1, 1, -1]);
							this.lightingAround[LightingAround.Pos.X1Y1Z1] = this.maxLight(lighting, this.lightCube[0, 0, 1], this.lightCube[1, 0, 0], this.lightCube[1, 0, 1], this.lightCube[0, 1, 0], this.lightCube[0, 1, 1], this.lightCube[1, 1, 0], this.lightCube[1, 1, 1]);
							this.lightingAround[LightingAround.Pos.X0Y1Z1] = this.maxLight(lighting, this.lightCube[0, 0, 1], this.lightCube[-1, 0, 0], this.lightCube[-1, 0, 1], this.lightCube[0, 1, 0], this.lightCube[0, 1, 1], this.lightCube[-1, 1, 0], this.lightCube[-1, 1, 1]);
							int facesDrawnFullBitfield = block.shape.getFacesDrawnFullBitfield(blockValue);
							bool flag3 = false;
							Vector3i posI = new Vector3i(i, l, j);
							if ((facesDrawnFullBitfield & 16) == 0)
							{
								this.DrawSide(BlockFace.North, blockValue, this.nBlocks, array, temperature, ref flag3, ref flag, zero, _meshes, posI, _worldPos);
							}
							if ((facesDrawnFullBitfield & 4) == 0)
							{
								this.DrawSide(BlockFace.South, blockValue, this.nBlocks, array, temperature, ref flag3, ref flag, zero, _meshes, posI, _worldPos);
							}
							if ((facesDrawnFullBitfield & 8) == 0)
							{
								this.DrawSide(BlockFace.East, blockValue, this.nBlocks, array, temperature, ref flag3, ref flag, zero, _meshes, posI, _worldPos);
							}
							if ((facesDrawnFullBitfield & 32) == 0)
							{
								this.DrawSide(BlockFace.West, blockValue, this.nBlocks, array, temperature, ref flag3, ref flag, zero, _meshes, posI, _worldPos);
							}
							if ((facesDrawnFullBitfield & 1) == 0)
							{
								BlockValue blockValue2 = this.nBlocks.Get(0, l + 1, 0);
								Block block2 = blockValue2.Block;
								if (block2 != null && block2.shape.isRenderFace(blockValue2, BlockFace.Bottom, blockValue))
								{
									if (block2.GetLightValue(blockValue2) <= 0)
									{
										float color_BOTTOM = VoxelMesh.COLOR_BOTTOM;
									}
									else
									{
										float color_TOP = VoxelMesh.COLOR_TOP;
									}
									IChunk chunk = this.nBlocks.GetChunk(0, 0);
									long textureFull = chunk.GetTextureFull(i, l + 1, j);
									this.SwizzleCopy(ref array, 2, 1, 5, 6);
									byte stability = chunk.GetStability(i, l + 1, j);
									this.lightingAround.SetStab(stability);
									block2.shape.renderFace(_worldPos, blockValue2, zero + Vector3.up, BlockFace.Bottom, array, this.lightingAround, textureFull, _meshes, BlockShape.MeshPurpose.World);
								}
							}
							BlockValue blockValue3 = this.nBlocks.Get(0, l - 1, 0);
							Block block3 = blockValue3.Block;
							if (block3 != null && (facesDrawnFullBitfield & 2) == 0 && block3.shape.isRenderFace(blockValue3, BlockFace.Top, blockValue))
							{
								IChunk chunk2 = this.nBlocks.GetChunk(0, 0);
								long textureFull2 = chunk2.GetTextureFull(i, l - 1, j);
								this.SwizzleCopy(ref array, 0, 3, 7, 4);
								byte stability2 = chunk2.GetStability(i, l - 1, j);
								this.lightingAround.SetStab(stability2);
								float num3 = 0f;
								if (block3.shape.IsTerrain() && l > 0)
								{
									sbyte density = this.nBlocks.GetChunk(0, 0).GetDensity(i, l, j);
									sbyte density2 = this.nBlocks.GetChunk(0, 0).GetDensity(i, l - 1, j);
									num3 = MarchingCubes.GetDecorationOffsetY(density, density2);
								}
								block3.shape.renderFace(_worldPos, blockValue3, zero + new Vector3(0f, num3 - 1f, 0f), BlockFace.Top, array, this.lightingAround, textureFull2, _meshes, BlockShape.MeshPurpose.World);
							}
							if (this.nBlocks.IsWater(0, l - 1, 0) && !flag2 && !flag3)
							{
								IChunk chunk3 = this.nBlocks.GetChunk(0, 0);
								this.SwizzleCopy(ref array, 0, 3, 7, 4);
								byte stability3 = chunk3.GetStability(i, l - 1, j);
								this.lightingAround.SetStab(stability3);
								this.RenderTopWater(blockValue, array, _meshes, zero2, _worldPos, false);
							}
							if (block != null && !blockValue.ischild && block.shape.IsRenderDecoration())
							{
								IChunk chunk4 = this.nBlocks.GetChunk(0, 0);
								float y2 = 0f;
								if (block.IsTerrainDecoration && block3.shape.IsTerrain() && l > 0)
								{
									sbyte density3 = chunk4.GetDensity(i, l, j);
									sbyte density4 = chunk4.GetDensity(i, l - 1, j);
									y2 = MarchingCubes.GetDecorationOffsetY(density3, density4);
								}
								for (int n = 0; n < _meshes.Length; n++)
								{
									_meshes[n].SetTemperature(temperature);
								}
								byte stability4 = chunk4.GetStability(i, l, j);
								this.lightingAround.SetStab(stability4);
								long textureFull3 = chunk4.GetTextureFull(i, l, j);
								block.RenderDecorations(_worldPos, blockValue, zero + new Vector3(0f, y2, 0f), this.verticesCube, this.lightingAround, textureFull3, _meshes, this.nBlocks);
							}
						}
					}
				}
			}
		}
	}

	public void Test()
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool IsRaisable(int type)
	{
		return type != 0 && (this.IsSolidCube(type) || !this.IsPlant(type));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool IsRaisableNeighbor(int _y, int _x, int _z)
	{
		return this.neighborIsWater[_y, _x, _z] || this.IsRaisable(this.neighborType[_y, _x, _z]);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool CanWaterTaper(int type)
	{
		return type == 0 || this.IsPlant(type);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool IsSolid(int type)
	{
		return this.IsTerrain(type) || (this.IsSolidCube(type) && type != 0);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool IsAirNeighbor(int _y, int _x, int _z)
	{
		return this.IsAir(this.neighborType[_y, _x, _z]) && !this.neighborIsWater[_y, _x, _z];
	}

	[Conditional("NEW_WATER_MESH_DEBUG")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void WatermeshDebugLog(string message)
	{
		UnityEngine.Debug.Log("[watermesh] " + message);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RenderTopWater(BlockValue _middleBV, Vector3[] v4Arr, VoxelMesh[] _meshes, Vector3i _blockPos, Vector3i _worldPos, bool isInsideTerrain = false)
	{
		_worldPos + _blockPos;
		int y = _blockPos.y;
		int num = isInsideTerrain ? y : (y - 1);
		Vector3i v3i = new Vector3i(_blockPos.x, num, _blockPos.z);
		this.neighborType[0, 1, 1] = 0;
		this.neighborType[1, 1, 1] = _middleBV.type;
		for (int i = 0; i < 2; i++)
		{
			for (int j = 0; j < 3; j++)
			{
				for (int k = 0; k < 3; k++)
				{
					this.neighborIsWater[i, k, j] = this.nBlocks.IsWater(k - 1, num + i, j - 1);
					if ((!isInsideTerrain || j != 1 || k != 1) && (isInsideTerrain || i != 1 || j != 1 || k != 1))
					{
						BlockValue blockValue = this.nBlocks.Get(k - 1, num + i, j - 1);
						this.neighborType[i, k, j] = blockValue.type;
					}
				}
			}
		}
		int type = _middleBV.type;
		if (!this.IsSolidCube(type) && !this.nBlocks.IsAir(0, y, 0) && !this.neighborIsWater[1, 1, 1] && (this.neighborIsWater[1, 0, 1] || this.neighborIsWater[1, 1, 2] || this.neighborIsWater[1, 2, 1] || this.neighborIsWater[1, 1, 0]))
		{
			return;
		}
		bool flag = this.IsSolid(this.neighborType[0, 0, 0]) && this.IsSolid(this.neighborType[0, 1, 0]) && this.IsSolid(this.neighborType[0, 2, 0]) && this.IsSolid(this.neighborType[0, 2, 1]) && this.IsSolid(this.neighborType[0, 2, 2]) && this.IsSolid(this.neighborType[0, 1, 2]) && this.IsSolid(this.neighborType[0, 0, 2]) && this.IsSolid(this.neighborType[0, 0, 1]);
		BlockValue waterClippingBV = isInsideTerrain ? _middleBV : this.nBlocks.Get(0, num, 0);
		bool flag2 = this.<RenderTopWater>g__TryPrepareWaterClippingVolume|49_0(waterClippingBV);
		using (MeshGenerator.renderTopWaterOldMarker.Auto())
		{
			for (int l = 0; l < 4; l++)
			{
				for (int m = 0; m < 4; m++)
				{
					this.cpyVerts[0].x = v4Arr[0].x + (float)l * 0.25f;
					this.cpyVerts[1].x = this.cpyVerts[0].x + 0.25f;
					this.cpyVerts[2].x = this.cpyVerts[0].x + 0.25f;
					this.cpyVerts[3].x = v4Arr[0].x + (float)l * 0.25f;
					this.cpyVerts[0].z = v4Arr[0].z + (float)m * 0.25f;
					this.cpyVerts[1].z = v4Arr[0].z + (float)m * 0.25f;
					this.cpyVerts[2].z = this.cpyVerts[0].z + 0.25f;
					this.cpyVerts[3].z = this.cpyVerts[0].z + 0.25f;
					this.cpyVerts[0].y = v4Arr[0].y + -1.5f;
					this.cpyVerts[1].y = v4Arr[1].y + -1.5f;
					this.cpyVerts[2].y = v4Arr[2].y + -1.5f;
					this.cpyVerts[3].y = v4Arr[3].y + -1.5f;
					bool flag3 = false;
					if (isInsideTerrain && !this.nBlocks.IsAir(0, y + 1, 0) && this.IsSolidCube(this.nBlocks.Get(0, y + 1, 0).type))
					{
						flag3 = true;
						bool flag4 = false;
						bool flag5 = false;
						bool flag6 = false;
						bool flag7 = false;
						if (l == 0)
						{
							if (m <= 2)
							{
								flag6 = true;
							}
							if (m >= 1)
							{
								flag5 = true;
							}
							if (m == 3)
							{
								if (!this.IsAirNeighbor(0, 1, 2) && !this.IsAirNeighbor(1, 1, 2))
								{
									flag6 = true;
								}
								if ((this.neighborIsWater[0, 0, 2] && !this.IsAirNeighbor(1, 0, 2)) || (this.neighborIsWater[0, 1, 2] && !this.IsAirNeighbor(1, 1, 2)) || (this.neighborIsWater[0, 0, 1] && !this.IsAirNeighbor(1, 0, 1)))
								{
									flag7 = true;
								}
								if (!this.IsAirNeighbor(0, 0, 1) && !this.IsAirNeighbor(1, 0, 1))
								{
									flag4 = true;
								}
							}
							else if (m == 0)
							{
								if ((this.neighborIsWater[0, 0, 0] && !this.IsAirNeighbor(1, 0, 0)) || (this.neighborIsWater[0, 1, 0] && !this.IsAirNeighbor(1, 1, 0)) || (this.neighborIsWater[0, 0, 1] && !this.IsAirNeighbor(1, 0, 1)))
								{
									flag4 = true;
								}
								if (!this.IsAirNeighbor(0, 1, 0) && !this.IsAirNeighbor(1, 1, 0))
								{
									flag5 = true;
								}
								if (!this.IsAirNeighbor(0, 0, 1) && !this.IsAirNeighbor(1, 0, 1))
								{
									flag7 = true;
								}
							}
							else
							{
								if (!this.IsAirNeighbor(0, 0, 1) && !this.IsAirNeighbor(1, 0, 1))
								{
									flag4 = true;
									flag7 = true;
								}
								if (!this.IsAirNeighbor(0, 2, 1) && !this.IsAirNeighbor(1, 2, 1))
								{
									flag5 = true;
									flag6 = true;
								}
							}
						}
						else if (l == 3)
						{
							if (m <= 2)
							{
								flag7 = true;
							}
							if (m >= 1)
							{
								flag4 = true;
							}
							if (m == 3)
							{
								if (!this.IsAirNeighbor(0, 2, 1) && !this.IsAirNeighbor(1, 2, 1))
								{
									flag5 = true;
								}
								if ((this.neighborIsWater[0, 2, 2] && !this.IsAirNeighbor(1, 2, 2)) || (this.neighborIsWater[0, 1, 2] && !this.IsAirNeighbor(1, 1, 2)) || (this.neighborIsWater[0, 2, 1] && !this.IsAirNeighbor(1, 2, 1)))
								{
									flag6 = true;
								}
								if (!this.IsAirNeighbor(0, 1, 2) && !this.IsAirNeighbor(1, 1, 2))
								{
									flag7 = true;
								}
							}
							else if (m == 0)
							{
								if ((this.neighborIsWater[0, 2, 0] && !this.IsAirNeighbor(1, 2, 0)) || (this.neighborIsWater[0, 1, 0] && !this.IsAirNeighbor(1, 1, 0)) || (this.neighborIsWater[0, 2, 1] && !this.IsAirNeighbor(1, 2, 1)))
								{
									flag5 = true;
								}
								if (!this.IsAirNeighbor(0, 1, 0) && !this.IsAirNeighbor(1, 1, 0))
								{
									flag4 = true;
								}
								if (!this.IsAirNeighbor(0, 2, 1) && !this.IsAirNeighbor(1, 2, 1))
								{
									flag6 = true;
								}
							}
							else
							{
								if (!this.IsAirNeighbor(0, 0, 1) && !this.IsAirNeighbor(1, 0, 1))
								{
									flag4 = true;
									flag7 = true;
								}
								if (!this.IsAirNeighbor(0, 2, 1) && !this.IsAirNeighbor(1, 2, 1))
								{
									flag5 = true;
									flag6 = true;
								}
							}
						}
						else
						{
							if (m <= 2)
							{
								flag6 = true;
								flag7 = true;
							}
							else if (!this.IsAirNeighbor(0, 1, 2) && !this.IsAirNeighbor(1, 1, 2))
							{
								flag6 = true;
								flag7 = true;
							}
							if (m >= 1)
							{
								flag4 = true;
								flag5 = true;
							}
							else if (!this.IsAirNeighbor(0, 1, 0) && !this.IsAirNeighbor(1, 1, 0))
							{
								flag4 = true;
								flag5 = true;
							}
						}
						if (flag4)
						{
							Vector3[] array = this.cpyVerts;
							int num2 = 0;
							array[num2].y = array[num2].y + 0.5f;
						}
						if (flag5)
						{
							Vector3[] array2 = this.cpyVerts;
							int num3 = 1;
							array2[num3].y = array2[num3].y + 0.5f;
						}
						if (flag6)
						{
							Vector3[] array3 = this.cpyVerts;
							int num4 = 2;
							array3[num4].y = array3[num4].y + 0.5f;
						}
						if (flag7)
						{
							Vector3[] array4 = this.cpyVerts;
							int num5 = 3;
							array4[num5].y = array4[num5].y + 0.5f;
						}
					}
					if (!isInsideTerrain && !this.IsAirNeighbor(1, 1, 1) && this.IsSolidCube(this.neighborType[1, 1, 1]))
					{
						flag3 = true;
						if (m == 0)
						{
							if (l == 0)
							{
								if (!this.IsAirNeighbor(0, 0, 1) && !this.IsAirNeighbor(0, 0, 0) && !this.IsAirNeighbor(0, 1, 0))
								{
									Vector3[] array5 = this.cpyVerts;
									int num6 = 0;
									array5[num6].y = array5[num6].y + 0.5f;
								}
								if (!this.IsAirNeighbor(0, 1, 0))
								{
									Vector3[] array6 = this.cpyVerts;
									int num7 = 1;
									array6[num7].y = array6[num7].y + 0.5f;
								}
								if (!this.IsAirNeighbor(0, 0, 1))
								{
									Vector3[] array7 = this.cpyVerts;
									int num8 = 3;
									array7[num8].y = array7[num8].y + 0.5f;
								}
								Vector3[] array8 = this.cpyVerts;
								int num9 = 2;
								array8[num9].y = array8[num9].y + 0.5f;
							}
							else if (l <= 2)
							{
								if (!this.IsAirNeighbor(0, 1, 0))
								{
									Vector3[] array9 = this.cpyVerts;
									int num10 = 0;
									array9[num10].y = array9[num10].y + 0.5f;
									Vector3[] array10 = this.cpyVerts;
									int num11 = 1;
									array10[num11].y = array10[num11].y + 0.5f;
								}
								Vector3[] array11 = this.cpyVerts;
								int num12 = 2;
								array11[num12].y = array11[num12].y + 0.5f;
								Vector3[] array12 = this.cpyVerts;
								int num13 = 3;
								array12[num13].y = array12[num13].y + 0.5f;
							}
							else if (l == 3)
							{
								if (!this.IsAirNeighbor(0, 1, 0) && !this.IsAirNeighbor(0, 2, 0) && !this.IsAirNeighbor(0, 2, 1))
								{
									Vector3[] array13 = this.cpyVerts;
									int num14 = 1;
									array13[num14].y = array13[num14].y + 0.5f;
								}
								if (!this.IsAirNeighbor(0, 1, 0))
								{
									Vector3[] array14 = this.cpyVerts;
									int num15 = 0;
									array14[num15].y = array14[num15].y + 0.5f;
								}
								if (!this.IsAirNeighbor(0, 2, 1))
								{
									Vector3[] array15 = this.cpyVerts;
									int num16 = 2;
									array15[num16].y = array15[num16].y + 0.5f;
								}
								Vector3[] array16 = this.cpyVerts;
								int num17 = 3;
								array16[num17].y = array16[num17].y + 0.5f;
							}
						}
						else if (m <= 2)
						{
							if (l == 0)
							{
								if (!this.IsAirNeighbor(0, 0, 1))
								{
									Vector3[] array17 = this.cpyVerts;
									int num18 = 0;
									array17[num18].y = array17[num18].y + 0.5f;
									Vector3[] array18 = this.cpyVerts;
									int num19 = 3;
									array18[num19].y = array18[num19].y + 0.5f;
								}
								Vector3[] array19 = this.cpyVerts;
								int num20 = 1;
								array19[num20].y = array19[num20].y + 0.5f;
								Vector3[] array20 = this.cpyVerts;
								int num21 = 2;
								array20[num21].y = array20[num21].y + 0.5f;
							}
							else if (l <= 2)
							{
								Vector3[] array21 = this.cpyVerts;
								int num22 = 0;
								array21[num22].y = array21[num22].y + 0.5f;
								Vector3[] array22 = this.cpyVerts;
								int num23 = 1;
								array22[num23].y = array22[num23].y + 0.5f;
								Vector3[] array23 = this.cpyVerts;
								int num24 = 2;
								array23[num24].y = array23[num24].y + 0.5f;
								Vector3[] array24 = this.cpyVerts;
								int num25 = 3;
								array24[num25].y = array24[num25].y + 0.5f;
							}
							else if (l == 3)
							{
								if (!this.IsAirNeighbor(0, 2, 1))
								{
									Vector3[] array25 = this.cpyVerts;
									int num26 = 1;
									array25[num26].y = array25[num26].y + 0.5f;
									Vector3[] array26 = this.cpyVerts;
									int num27 = 2;
									array26[num27].y = array26[num27].y + 0.5f;
								}
								Vector3[] array27 = this.cpyVerts;
								int num28 = 0;
								array27[num28].y = array27[num28].y + 0.5f;
								Vector3[] array28 = this.cpyVerts;
								int num29 = 3;
								array28[num29].y = array28[num29].y + 0.5f;
							}
						}
						else if (m == 3)
						{
							if (l == 0)
							{
								if (!this.IsAirNeighbor(0, 0, 1) && !this.IsAirNeighbor(0, 0, 2) && !this.IsAirNeighbor(0, 1, 2))
								{
									Vector3[] array29 = this.cpyVerts;
									int num30 = 3;
									array29[num30].y = array29[num30].y + 0.5f;
								}
								if (!this.IsAirNeighbor(0, 0, 1))
								{
									Vector3[] array30 = this.cpyVerts;
									int num31 = 0;
									array30[num31].y = array30[num31].y + 0.5f;
								}
								if (!this.IsAirNeighbor(0, 1, 2))
								{
									Vector3[] array31 = this.cpyVerts;
									int num32 = 2;
									array31[num32].y = array31[num32].y + 0.5f;
								}
								Vector3[] array32 = this.cpyVerts;
								int num33 = 1;
								array32[num33].y = array32[num33].y + 0.5f;
							}
							else if (l <= 2)
							{
								if (!this.IsAirNeighbor(0, 1, 2))
								{
									Vector3[] array33 = this.cpyVerts;
									int num34 = 2;
									array33[num34].y = array33[num34].y + 0.5f;
									Vector3[] array34 = this.cpyVerts;
									int num35 = 3;
									array34[num35].y = array34[num35].y + 0.5f;
								}
								Vector3[] array35 = this.cpyVerts;
								int num36 = 0;
								array35[num36].y = array35[num36].y + 0.5f;
								Vector3[] array36 = this.cpyVerts;
								int num37 = 1;
								array36[num37].y = array36[num37].y + 0.5f;
							}
							else if (l == 3)
							{
								if (!this.IsAirNeighbor(0, 1, 2) && !this.IsAirNeighbor(0, 2, 2) && !this.IsAirNeighbor(0, 2, 1))
								{
									Vector3[] array37 = this.cpyVerts;
									int num38 = 2;
									array37[num38].y = array37[num38].y + 0.5f;
								}
								if (!this.IsAirNeighbor(0, 1, 2))
								{
									Vector3[] array38 = this.cpyVerts;
									int num39 = 3;
									array38[num39].y = array38[num39].y + 0.5f;
								}
								if (!this.IsAirNeighbor(0, 2, 1))
								{
									Vector3[] array39 = this.cpyVerts;
									int num40 = 1;
									array39[num40].y = array39[num40].y + 0.5f;
								}
								Vector3[] array40 = this.cpyVerts;
								int num41 = 0;
								array40[num41].y = array40[num41].y + 0.5f;
							}
						}
					}
					if (flag && !isInsideTerrain)
					{
						for (int n = 0; n < 4; n++)
						{
							Vector3[] array41 = this.cpyVerts;
							int num42 = n;
							array41[num42].y = array41[num42].y - -1.5f;
						}
						if (l == 0)
						{
							Vector3[] array42 = this.cpyVerts;
							int num43 = 0;
							array42[num43].x = array42[num43].x - 0.125f;
							Vector3[] array43 = this.cpyVerts;
							int num44 = 3;
							array43[num44].x = array43[num44].x - 0.125f;
							Vector3[] array44 = this.cpyVerts;
							int num45 = 0;
							array44[num45].y = array44[num45].y - 0.125f;
							Vector3[] array45 = this.cpyVerts;
							int num46 = 3;
							array45[num46].y = array45[num46].y - 0.125f;
						}
						if (l == 3)
						{
							Vector3[] array46 = this.cpyVerts;
							int num47 = 1;
							array46[num47].x = array46[num47].x + 0.125f;
							Vector3[] array47 = this.cpyVerts;
							int num48 = 2;
							array47[num48].x = array47[num48].x + 0.125f;
							Vector3[] array48 = this.cpyVerts;
							int num49 = 1;
							array48[num49].y = array48[num49].y - 0.125f;
							Vector3[] array49 = this.cpyVerts;
							int num50 = 2;
							array49[num50].y = array49[num50].y - 0.125f;
						}
						if (m == 0)
						{
							Vector3[] array50 = this.cpyVerts;
							int num51 = 0;
							array50[num51].z = array50[num51].z - 0.125f;
							Vector3[] array51 = this.cpyVerts;
							int num52 = 1;
							array51[num52].z = array51[num52].z - 0.125f;
							Vector3[] array52 = this.cpyVerts;
							int num53 = 0;
							array52[num53].y = array52[num53].y - 0.125f;
							Vector3[] array53 = this.cpyVerts;
							int num54 = 1;
							array53[num54].y = array53[num54].y - 0.125f;
						}
						if (m == 3)
						{
							Vector3[] array54 = this.cpyVerts;
							int num55 = 2;
							array54[num55].z = array54[num55].z + 0.125f;
							Vector3[] array55 = this.cpyVerts;
							int num56 = 3;
							array55[num56].z = array55[num56].z + 0.125f;
							Vector3[] array56 = this.cpyVerts;
							int num57 = 2;
							array56[num57].y = array56[num57].y - 0.125f;
							Vector3[] array57 = this.cpyVerts;
							int num58 = 3;
							array57[num58].y = array57[num58].y - 0.125f;
						}
					}
					else
					{
						for (int num59 = 0; num59 < 4; num59++)
						{
							this.raiseHeight[num59] = 0f;
						}
						this.IsRaisableNeighbor(0, 0, 0);
						if (m == 0)
						{
							if (l == 0)
							{
								if (!flag3)
								{
									if ((this.neighborIsWater[0, 0, 0] && !this.IsAirNeighbor(1, 0, 0) && this.IsSolidCube(this.neighborType[1, 0, 0])) || (this.neighborIsWater[0, 0, 1] && !this.IsAirNeighbor(1, 0, 1) && this.IsSolidCube(this.neighborType[1, 0, 1])) || (this.neighborIsWater[0, 1, 0] && !this.IsAirNeighbor(1, 1, 0) && this.IsSolidCube(this.neighborType[1, 1, 0])))
									{
										Vector3[] array58 = this.cpyVerts;
										int num60 = 0;
										array58[num60].y = array58[num60].y + 0.5f;
									}
									if (this.neighborIsWater[0, 0, 1] && !this.IsAirNeighbor(1, 0, 1) && this.IsSolidCube(this.neighborType[1, 0, 1]))
									{
										Vector3[] array59 = this.cpyVerts;
										int num61 = 3;
										array59[num61].y = array59[num61].y + 0.5f;
									}
									if (this.neighborIsWater[0, 1, 0] && !this.IsAirNeighbor(1, 1, 0) && this.IsSolidCube(this.neighborType[1, 1, 0]))
									{
										Vector3[] array60 = this.cpyVerts;
										int num62 = 1;
										array60[num62].y = array60[num62].y + 0.5f;
									}
								}
								this.raiseHeight[0] += ((this.IsRaisableNeighbor(0, 0, 0) && this.IsRaisableNeighbor(0, 0, 1) && this.IsRaisableNeighbor(0, 1, 0)) ? 1f : 0f);
								this.raiseHeight[1] += ((this.IsRaisableNeighbor(0, 0, 0) && this.IsRaisableNeighbor(0, 0, 1) && this.IsRaisableNeighbor(0, 1, 0)) ? 1f : 0f);
								this.raiseHeight[2] += ((this.IsRaisableNeighbor(0, 0, 0) && this.IsRaisableNeighbor(0, 0, 1) && this.IsRaisableNeighbor(0, 1, 0)) ? 1f : 0.796f);
								this.raiseHeight[3] += ((this.IsRaisableNeighbor(0, 0, 0) && this.IsRaisableNeighbor(0, 0, 1) && this.IsRaisableNeighbor(0, 1, 0)) ? 1f : 0f);
							}
							else if (l == 1)
							{
								if (!flag3 && this.neighborIsWater[0, 1, 0] && !this.IsAirNeighbor(1, 1, 0) && this.IsSolidCube(this.neighborType[1, 1, 0]))
								{
									Vector3[] array61 = this.cpyVerts;
									int num63 = 0;
									array61[num63].y = array61[num63].y + 0.5f;
									Vector3[] array62 = this.cpyVerts;
									int num64 = 1;
									array62[num64].y = array62[num64].y + 0.5f;
								}
								this.raiseHeight[0] += ((this.IsRaisableNeighbor(0, 0, 0) && this.IsRaisableNeighbor(0, 0, 1) && this.IsRaisableNeighbor(0, 1, 0)) ? 1f : 0f);
								this.raiseHeight[1] += (this.IsRaisableNeighbor(0, 1, 0) ? 1f : 0f);
								this.raiseHeight[2] += (this.IsRaisableNeighbor(0, 1, 0) ? 1f : 0.9f);
								this.raiseHeight[3] += ((this.IsRaisableNeighbor(0, 0, 0) && this.IsRaisableNeighbor(0, 0, 1) && this.IsRaisableNeighbor(0, 1, 0)) ? 1f : 0.796f);
							}
							else if (l == 2)
							{
								if (!flag3 && this.neighborIsWater[0, 1, 0] && !this.IsAirNeighbor(1, 1, 0) && this.IsSolidCube(this.neighborType[1, 1, 0]))
								{
									Vector3[] array63 = this.cpyVerts;
									int num65 = 0;
									array63[num65].y = array63[num65].y + 0.5f;
									Vector3[] array64 = this.cpyVerts;
									int num66 = 1;
									array64[num66].y = array64[num66].y + 0.5f;
								}
								this.raiseHeight[0] += (this.IsRaisableNeighbor(0, 1, 0) ? 1f : 0f);
								this.raiseHeight[1] += ((this.IsRaisableNeighbor(0, 2, 0) && this.IsRaisableNeighbor(0, 2, 1) && this.IsRaisableNeighbor(0, 1, 0)) ? 1f : 0f);
								this.raiseHeight[2] += ((this.IsRaisableNeighbor(0, 2, 0) && this.IsRaisableNeighbor(0, 2, 1) && this.IsRaisableNeighbor(0, 1, 0)) ? 1f : 0.796f);
								this.raiseHeight[3] += (this.IsRaisableNeighbor(0, 1, 0) ? 1f : 0.9f);
							}
							else if (l == 3)
							{
								if (!flag3)
								{
									if ((this.neighborIsWater[0, 2, 0] && !this.IsAirNeighbor(1, 2, 0) && this.IsSolidCube(this.neighborType[1, 2, 0])) || (this.neighborIsWater[0, 2, 1] && !this.IsAirNeighbor(1, 2, 1) && this.IsSolidCube(this.neighborType[1, 2, 1])) || (this.neighborIsWater[0, 1, 0] && !this.IsAirNeighbor(1, 1, 0) && this.IsSolidCube(this.neighborType[1, 1, 0])))
									{
										Vector3[] array65 = this.cpyVerts;
										int num67 = 1;
										array65[num67].y = array65[num67].y + 0.5f;
									}
									if (this.neighborIsWater[0, 2, 1] && !this.IsAirNeighbor(1, 2, 1) && this.IsSolidCube(this.neighborType[1, 2, 1]))
									{
										Vector3[] array66 = this.cpyVerts;
										int num68 = 2;
										array66[num68].y = array66[num68].y + 0.5f;
									}
									if (this.neighborIsWater[0, 1, 0] && !this.IsAirNeighbor(1, 1, 0) && this.IsSolidCube(this.neighborType[1, 1, 0]))
									{
										Vector3[] array67 = this.cpyVerts;
										int num69 = 0;
										array67[num69].y = array67[num69].y + 0.5f;
									}
								}
								this.raiseHeight[0] += ((this.IsRaisableNeighbor(0, 2, 0) && this.IsRaisableNeighbor(0, 2, 1) && this.IsRaisableNeighbor(0, 1, 0)) ? 1f : 0f);
								this.raiseHeight[1] += ((this.IsRaisableNeighbor(0, 2, 0) && this.IsRaisableNeighbor(0, 2, 1) && this.IsRaisableNeighbor(0, 1, 0)) ? 1f : 0f);
								this.raiseHeight[2] += ((this.IsRaisableNeighbor(0, 2, 0) && this.IsRaisableNeighbor(0, 2, 1) && this.IsRaisableNeighbor(0, 1, 0)) ? 1f : 0f);
								this.raiseHeight[3] += ((this.IsRaisableNeighbor(0, 2, 0) && this.IsRaisableNeighbor(0, 2, 1) && this.IsRaisableNeighbor(0, 1, 0)) ? 1f : 0.796f);
							}
						}
						else if (m == 1)
						{
							if (l == 0)
							{
								if (!flag3 && this.neighborIsWater[0, 0, 1] && !this.IsAirNeighbor(1, 0, 1) && this.IsSolidCube(this.neighborType[1, 0, 1]))
								{
									Vector3[] array68 = this.cpyVerts;
									int num70 = 0;
									array68[num70].y = array68[num70].y + 0.5f;
									Vector3[] array69 = this.cpyVerts;
									int num71 = 3;
									array69[num71].y = array69[num71].y + 0.5f;
								}
								this.raiseHeight[0] += ((this.IsRaisableNeighbor(0, 0, 0) && this.IsRaisableNeighbor(0, 0, 1) && this.IsRaisableNeighbor(0, 1, 0)) ? 1f : 0f);
								this.raiseHeight[1] += ((this.IsRaisableNeighbor(0, 0, 0) && this.IsRaisableNeighbor(0, 0, 1) && this.IsRaisableNeighbor(0, 1, 0)) ? 1f : 0.796f);
								this.raiseHeight[2] += (this.IsRaisableNeighbor(0, 0, 1) ? 1f : 0.9f);
								this.raiseHeight[3] += (this.IsRaisableNeighbor(0, 0, 1) ? 1f : 0f);
							}
							else if (l == 1)
							{
								this.raiseHeight[0] += ((this.IsRaisableNeighbor(0, 0, 0) && this.IsRaisableNeighbor(0, 0, 1) && this.IsRaisableNeighbor(0, 1, 0)) ? 1f : 0.796f);
								this.raiseHeight[1] += (this.IsRaisableNeighbor(0, 1, 0) ? 1f : 0.9f);
								this.raiseHeight[2] += 1f;
								this.raiseHeight[3] += (this.IsRaisableNeighbor(0, 0, 1) ? 1f : 0.9f);
							}
							else if (l == 2)
							{
								this.raiseHeight[0] += (this.IsRaisableNeighbor(0, 1, 0) ? 1f : 0.9f);
								this.raiseHeight[1] += ((this.IsRaisableNeighbor(0, 2, 0) && this.IsRaisableNeighbor(0, 2, 1) && this.IsRaisableNeighbor(0, 1, 0)) ? 1f : 0.796f);
								this.raiseHeight[2] += (this.IsRaisableNeighbor(0, 2, 1) ? 1f : 0.9f);
								this.raiseHeight[3] += 1f;
							}
							else if (l == 3)
							{
								if (!flag3 && this.neighborIsWater[0, 2, 1] && !this.IsAirNeighbor(1, 2, 1) && this.IsSolidCube(this.neighborType[1, 2, 1]))
								{
									Vector3[] array70 = this.cpyVerts;
									int num72 = 1;
									array70[num72].y = array70[num72].y + 0.5f;
									Vector3[] array71 = this.cpyVerts;
									int num73 = 2;
									array71[num73].y = array71[num73].y + 0.5f;
								}
								this.raiseHeight[0] += ((this.IsRaisableNeighbor(0, 2, 0) && this.IsRaisableNeighbor(0, 2, 1) && this.IsRaisableNeighbor(0, 1, 0)) ? 1f : 0.796f);
								this.raiseHeight[1] += ((this.IsRaisableNeighbor(0, 2, 0) && this.IsRaisableNeighbor(0, 2, 1) && this.IsRaisableNeighbor(0, 1, 0)) ? 1f : 0f);
								this.raiseHeight[2] += (this.IsRaisableNeighbor(0, 2, 1) ? 1f : 0f);
								this.raiseHeight[3] += (this.IsRaisableNeighbor(0, 2, 1) ? 1f : 0.9f);
							}
						}
						else if (m == 2)
						{
							if (l == 0)
							{
								if (!flag3 && this.neighborIsWater[0, 0, 1] && !this.IsAirNeighbor(1, 0, 1) && this.IsSolidCube(this.neighborType[1, 0, 1]))
								{
									Vector3[] array72 = this.cpyVerts;
									int num74 = 0;
									array72[num74].y = array72[num74].y + 0.5f;
									Vector3[] array73 = this.cpyVerts;
									int num75 = 3;
									array73[num75].y = array73[num75].y + 0.5f;
								}
								this.raiseHeight[0] += (this.IsRaisableNeighbor(0, 0, 1) ? 1f : 0f);
								this.raiseHeight[1] += (this.IsRaisableNeighbor(0, 0, 1) ? 1f : 0.9f);
								this.raiseHeight[2] += ((this.IsRaisableNeighbor(0, 0, 1) && this.IsRaisableNeighbor(0, 0, 2) && this.IsRaisableNeighbor(0, 1, 2)) ? 1f : 0.796f);
								this.raiseHeight[3] += ((this.IsRaisableNeighbor(0, 0, 1) && this.IsRaisableNeighbor(0, 0, 2) && this.IsRaisableNeighbor(0, 1, 2)) ? 1f : 0f);
							}
							else if (l == 1)
							{
								this.raiseHeight[0] += (this.IsRaisableNeighbor(0, 0, 1) ? 1f : 0.9f);
								this.raiseHeight[1] += 1f;
								this.raiseHeight[2] += (this.IsRaisableNeighbor(0, 1, 2) ? 1f : 0.9f);
								this.raiseHeight[3] += ((this.IsRaisableNeighbor(0, 0, 1) && this.IsRaisableNeighbor(0, 0, 2) && this.IsRaisableNeighbor(0, 1, 2)) ? 1f : 0.796f);
							}
							else if (l == 2)
							{
								this.raiseHeight[0] += 1f;
								this.raiseHeight[1] += (this.IsRaisableNeighbor(0, 2, 1) ? 1f : 0.9f);
								this.raiseHeight[2] += ((this.IsRaisableNeighbor(0, 2, 1) && this.IsRaisableNeighbor(0, 2, 2) && this.IsRaisableNeighbor(0, 1, 2)) ? 1f : 0.796f);
								this.raiseHeight[3] += (this.IsRaisableNeighbor(0, 1, 2) ? 1f : 0.9f);
							}
							else if (l == 3)
							{
								if (!flag3 && this.neighborIsWater[0, 2, 1] && !this.IsAirNeighbor(1, 2, 1) && this.IsSolidCube(this.neighborType[1, 2, 1]))
								{
									Vector3[] array74 = this.cpyVerts;
									int num76 = 1;
									array74[num76].y = array74[num76].y + 0.5f;
									Vector3[] array75 = this.cpyVerts;
									int num77 = 2;
									array75[num77].y = array75[num77].y + 0.5f;
								}
								this.raiseHeight[0] += (this.IsRaisableNeighbor(0, 2, 1) ? 1f : 0.9f);
								this.raiseHeight[1] += (this.IsRaisableNeighbor(0, 2, 1) ? 1f : 0f);
								this.raiseHeight[2] += ((this.IsRaisableNeighbor(0, 2, 1) && this.IsRaisableNeighbor(0, 2, 2) && this.IsRaisableNeighbor(0, 1, 2)) ? 1f : 0f);
								this.raiseHeight[3] += ((this.IsRaisableNeighbor(0, 2, 1) && this.IsRaisableNeighbor(0, 2, 2) && this.IsRaisableNeighbor(0, 1, 2)) ? 1f : 0.796f);
							}
						}
						else if (m == 3)
						{
							if (l == 0)
							{
								if (!flag3)
								{
									if ((this.neighborIsWater[0, 0, 2] && !this.IsAirNeighbor(1, 0, 2) && this.IsSolidCube(this.neighborType[1, 0, 2])) || (this.neighborIsWater[0, 1, 2] && !this.IsAirNeighbor(1, 1, 2) && this.IsSolidCube(this.neighborType[1, 1, 2])) || (this.neighborIsWater[0, 0, 1] && !this.IsAirNeighbor(1, 0, 1) && this.IsSolidCube(this.neighborType[1, 0, 1])))
									{
										Vector3[] array76 = this.cpyVerts;
										int num78 = 3;
										array76[num78].y = array76[num78].y + 0.5f;
									}
									if (this.neighborIsWater[0, 1, 2] && !this.IsAirNeighbor(1, 1, 2) && this.IsSolidCube(this.neighborType[1, 1, 2]))
									{
										Vector3[] array77 = this.cpyVerts;
										int num79 = 2;
										array77[num79].y = array77[num79].y + 0.5f;
									}
									if (this.neighborIsWater[0, 0, 1] && !this.IsAirNeighbor(1, 0, 1) && this.IsSolidCube(this.neighborType[1, 0, 1]))
									{
										Vector3[] array78 = this.cpyVerts;
										int num80 = 0;
										array78[num80].y = array78[num80].y + 0.5f;
									}
								}
								this.raiseHeight[0] += ((this.IsRaisableNeighbor(0, 0, 1) && this.IsRaisableNeighbor(0, 0, 2) && this.IsRaisableNeighbor(0, 1, 2)) ? 1f : 0f);
								this.raiseHeight[1] += ((this.IsRaisableNeighbor(0, 0, 1) && this.IsRaisableNeighbor(0, 0, 2) && this.IsRaisableNeighbor(0, 1, 2)) ? 1f : 0.796f);
								this.raiseHeight[2] += ((this.IsRaisableNeighbor(0, 0, 1) && this.IsRaisableNeighbor(0, 0, 2) && this.IsRaisableNeighbor(0, 1, 2)) ? 1f : 0f);
								this.raiseHeight[3] += ((this.IsRaisableNeighbor(0, 0, 1) && this.IsRaisableNeighbor(0, 0, 2) && this.IsRaisableNeighbor(0, 1, 2)) ? 1f : 0f);
							}
							else if (l == 1)
							{
								if (!flag3 && this.neighborIsWater[0, 1, 2] && !this.IsAirNeighbor(1, 1, 2) && this.IsSolidCube(this.neighborType[1, 1, 2]))
								{
									Vector3[] array79 = this.cpyVerts;
									int num81 = 2;
									array79[num81].y = array79[num81].y + 0.5f;
									Vector3[] array80 = this.cpyVerts;
									int num82 = 3;
									array80[num82].y = array80[num82].y + 0.5f;
								}
								this.raiseHeight[0] += ((this.IsRaisableNeighbor(0, 0, 1) && this.IsRaisableNeighbor(0, 0, 2) && this.IsRaisableNeighbor(0, 1, 2)) ? 1f : 0.796f);
								this.raiseHeight[1] += (this.IsRaisableNeighbor(0, 1, 2) ? 1f : 0.9f);
								this.raiseHeight[2] += (this.IsRaisableNeighbor(0, 1, 2) ? 1f : 0f);
								this.raiseHeight[3] += ((this.IsRaisableNeighbor(0, 0, 1) && this.IsRaisableNeighbor(0, 0, 2) && this.IsRaisableNeighbor(0, 1, 2)) ? 1f : 0f);
							}
							else if (l == 2)
							{
								if (!flag3 && this.neighborIsWater[0, 1, 2] && !this.IsAirNeighbor(1, 1, 2) && this.IsSolidCube(this.neighborType[1, 1, 2]))
								{
									Vector3[] array81 = this.cpyVerts;
									int num83 = 2;
									array81[num83].y = array81[num83].y + 0.5f;
									Vector3[] array82 = this.cpyVerts;
									int num84 = 3;
									array82[num84].y = array82[num84].y + 0.5f;
								}
								this.raiseHeight[0] += (this.IsRaisableNeighbor(0, 1, 2) ? 1f : 0.9f);
								this.raiseHeight[1] += ((this.IsRaisableNeighbor(0, 1, 2) && this.IsRaisableNeighbor(0, 2, 2) && this.IsRaisableNeighbor(0, 2, 1)) ? 1f : 0.796f);
								this.raiseHeight[2] += ((this.IsRaisableNeighbor(0, 1, 2) && this.IsRaisableNeighbor(0, 2, 2) && this.IsRaisableNeighbor(0, 2, 1)) ? 1f : 0f);
								this.raiseHeight[3] += (this.IsRaisableNeighbor(0, 1, 2) ? 1f : 0f);
							}
							else if (l == 3)
							{
								if (!flag3)
								{
									if ((this.neighborIsWater[0, 2, 2] && !this.IsAirNeighbor(1, 2, 2) && this.IsSolidCube(this.neighborType[1, 2, 2])) || (this.neighborIsWater[0, 1, 2] && !this.IsAirNeighbor(1, 1, 2) && this.IsSolidCube(this.neighborType[1, 1, 2])) || (this.neighborIsWater[0, 2, 1] && !this.IsAirNeighbor(1, 2, 1) && this.IsSolidCube(this.neighborType[1, 2, 1])))
									{
										Vector3[] array83 = this.cpyVerts;
										int num85 = 2;
										array83[num85].y = array83[num85].y + 0.5f;
									}
									if (this.neighborIsWater[0, 1, 2] && !this.IsAirNeighbor(1, 1, 2) && this.IsSolidCube(this.neighborType[1, 1, 2]))
									{
										Vector3[] array84 = this.cpyVerts;
										int num86 = 3;
										array84[num86].y = array84[num86].y + 0.5f;
									}
									if (this.neighborIsWater[0, 2, 1] && !this.IsAirNeighbor(1, 2, 1) && this.IsSolidCube(this.neighborType[1, 2, 1]))
									{
										Vector3[] array85 = this.cpyVerts;
										int num87 = 1;
										array85[num87].y = array85[num87].y + 0.5f;
									}
								}
								this.raiseHeight[0] += ((this.IsRaisableNeighbor(0, 1, 2) && this.IsRaisableNeighbor(0, 2, 2) && this.IsRaisableNeighbor(0, 2, 1)) ? 1f : 0.796f);
								this.raiseHeight[1] += ((this.IsRaisableNeighbor(0, 1, 2) && this.IsRaisableNeighbor(0, 2, 2) && this.IsRaisableNeighbor(0, 2, 1)) ? 1f : 0f);
								this.raiseHeight[2] += ((this.IsRaisableNeighbor(0, 1, 2) && this.IsRaisableNeighbor(0, 2, 2) && this.IsRaisableNeighbor(0, 2, 1)) ? 1f : 0f);
								this.raiseHeight[3] += ((this.IsRaisableNeighbor(0, 1, 2) && this.IsRaisableNeighbor(0, 2, 2) && this.IsRaisableNeighbor(0, 2, 1)) ? 1f : 0f);
							}
						}
						if (l == 0)
						{
							if (this.neighborIsWater[1, 0, 1])
							{
								this.raiseHeight[0] = (this.raiseHeight[3] = 1f);
							}
							if (m == 0)
							{
								if (this.neighborIsWater[1, 0, 0])
								{
									this.raiseHeight[0] = 1f;
								}
								if (this.neighborIsWater[1, 1, 0])
								{
									this.raiseHeight[0] = (this.raiseHeight[1] = 1f);
								}
							}
							if (m == 3)
							{
								if (this.neighborIsWater[1, 0, 2])
								{
									this.raiseHeight[3] = 1f;
								}
								if (this.neighborIsWater[1, 1, 2])
								{
									this.raiseHeight[2] = (this.raiseHeight[3] = 1f);
								}
							}
						}
						else if (l == 3)
						{
							if (this.neighborIsWater[1, 2, 1])
							{
								this.raiseHeight[1] = (this.raiseHeight[2] = 1f);
							}
							if (m == 0)
							{
								if (this.neighborIsWater[1, 2, 0])
								{
									this.raiseHeight[1] = 1f;
								}
								if (this.neighborIsWater[1, 1, 0])
								{
									this.raiseHeight[0] = (this.raiseHeight[1] = 1f);
								}
							}
							if (m == 3)
							{
								if (this.neighborIsWater[1, 2, 2])
								{
									this.raiseHeight[2] = 1f;
								}
								if (this.neighborIsWater[1, 1, 2])
								{
									this.raiseHeight[2] = (this.raiseHeight[3] = 1f);
								}
							}
						}
						else if (m == 0)
						{
							if (this.neighborIsWater[1, 1, 0])
							{
								this.raiseHeight[0] = (this.raiseHeight[1] = 1f);
							}
						}
						else if (m == 3 && this.neighborIsWater[1, 1, 2])
						{
							this.raiseHeight[2] = (this.raiseHeight[3] = 1f);
						}
						this.dropY[0] = (this.dropY[1] = (this.dropY[2] = (this.dropY[3] = 0f)));
						for (int num88 = 0; num88 < 4; num88++)
						{
							Vector3[] array86 = this.cpyVerts;
							int num89 = num88;
							array86[num89].y = array86[num89].y + (this.raiseHeight[num88] - this.dropY[num88] * this.raiseHeight[num88]);
						}
					}
					if (flag2)
					{
						for (int num90 = 0; num90 < 4; num90++)
						{
							Vector3 a = this.cpyVerts[num90] - v3i;
							this.waterClippingVolume.ApplyClipping(ref a);
							this.cpyVerts[num90] = a + v3i;
						}
					}
					WaterMeshUtils.RenderFace(this.cpyVerts, this.lightingAround, 0L, _meshes, new Vector2(0f, 0f), false);
				}
			}
		}
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool <RenderTopWater>g__TryPrepareWaterClippingVolume|49_0(BlockValue _waterClippingBV)
	{
		if (!_waterClippingBV.Block.WaterClipEnabled)
		{
			return false;
		}
		Plane waterClipPlane = _waterClippingBV.Block.WaterClipPlane;
		Quaternion rotationStatic = BlockShapeNew.GetRotationStatic((int)_waterClippingBV.rotation);
		GeometryUtils.RotatePlaneAroundPoint(ref waterClipPlane, WaterClippingUtils.CubeBounds.center, rotationStatic);
		bool flag = this.neighborIsWater[0, 1, 2];
		bool flag2 = this.neighborIsWater[0, 2, 1];
		bool flag3 = this.neighborIsWater[0, 1, 0];
		bool flag4 = this.neighborIsWater[0, 0, 1];
		if (flag && flag2 && flag3 && flag4)
		{
			return false;
		}
		bool flag5 = this.neighborIsWater[0, 1, 1];
		bool flag6 = false;
		bool flag7 = false;
		if (flag5)
		{
			if (waterClipPlane.GetDistanceToPoint(new Vector3(0.5f, 0.5f, 0.5f)) > 0f)
			{
				flag6 = true;
			}
			else
			{
				flag7 = true;
			}
		}
		if (flag)
		{
			if (waterClipPlane.GetDistanceToPoint(new Vector3(0.5f, 0.5f, 1f)) > 0f)
			{
				flag6 = true;
			}
			else
			{
				flag7 = true;
			}
		}
		if (flag2)
		{
			if (waterClipPlane.GetDistanceToPoint(new Vector3(1f, 0.5f, 0.5f)) > 0f)
			{
				flag6 = true;
			}
			else
			{
				flag7 = true;
			}
		}
		if (flag3)
		{
			if (waterClipPlane.GetDistanceToPoint(new Vector3(0.5f, 0.5f, 0f)) > 0f)
			{
				flag6 = true;
			}
			else
			{
				flag7 = true;
			}
		}
		if (flag4)
		{
			if (waterClipPlane.GetDistanceToPoint(new Vector3(0f, 0.5f, 0.5f)) > 0f)
			{
				flag6 = true;
			}
			else
			{
				flag7 = true;
			}
		}
		if (flag6 == flag7)
		{
			return false;
		}
		if (flag6)
		{
			waterClipPlane.Flip();
		}
		this.waterClippingVolume.Prepare(waterClipPlane);
		return true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public INeighborBlockCache nBlocks;

	[PublicizedFrom(EAccessModifier.Private)]
	public LightingAround lightingAround = new LightingAround(0, 0, 0);

	[PublicizedFrom(EAccessModifier.Private)]
	public Lighting3DArray lightCube = new Lighting3DArray();

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3[] verticesCube = new Vector3[8];

	[PublicizedFrom(EAccessModifier.Protected)]
	public int[] heights = new int[9];

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cWaterDensity = 4;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cWaterSize = 0.25f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cWaterFloatHeight = -1.5f;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3[] cpyVerts = new Vector3[4];

	[PublicizedFrom(EAccessModifier.Private)]
	public const float ringHeight1 = 0.9f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float ringHeight1c = 0.796f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float ringHeight2 = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float[] raiseHeight = new float[4];

	[PublicizedFrom(EAccessModifier.Private)]
	public float[] dropY = new float[4];

	[PublicizedFrom(EAccessModifier.Private)]
	public int[,,] neighborType = new int[2, 3, 3];

	[PublicizedFrom(EAccessModifier.Private)]
	public bool[,,] neighborIsWater = new bool[2, 3, 3];

	[PublicizedFrom(EAccessModifier.Private)]
	public WaterClippingVolume waterClippingVolume = new WaterClippingVolume();

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerMarker renderTopWaterOldMarker = new ProfilerMarker("MeshGenerator.RenderTopWaterOld");
}
