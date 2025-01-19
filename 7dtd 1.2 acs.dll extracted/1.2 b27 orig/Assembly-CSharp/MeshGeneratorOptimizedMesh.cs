using System;
using UnityEngine;

public class MeshGeneratorOptimizedMesh
{
	public MeshGeneratorOptimizedMesh(INeighborBlockCache _nBlocks)
	{
		this.nBlocks = _nBlocks;
	}

	public void GenerateCollisionMesh(Vector3i _startPos, Vector3i _endPos, VoxelMesh _voxelMesh)
	{
		Vector3i vector3i = _endPos - _startPos;
		int num = Math.Max(Math.Max(vector3i.x, vector3i.y), vector3i.z);
		bool[,] array = new bool[num, num];
		bool[,] array2 = new bool[num, num];
		int x = _startPos.x;
		int y = _startPos.y;
		int z = _startPos.z;
		for (int i = y + vector3i.y - 1; i >= y; i--)
		{
			if (i != 0 && i != 255)
			{
				int num2 = 0;
				int num3 = 0;
				for (int j = x; j < x + vector3i.x; j++)
				{
					for (int k = z; k < z + vector3i.z; k++)
					{
						this.nBlocks.Init(j, k);
						Block block = this.nBlocks.Get(0, i, 0).Block;
						Block block2 = this.nBlocks.Get(0, i + 1, 0).Block;
						bool flag = block.IsCollideMovement && !block2.IsCollideMovement;
						if (flag)
						{
							num2++;
						}
						array[j - x, k - z] = flag;
						block2 = this.nBlocks.Get(0, i - 1, 0).Block;
						flag = (block.IsCollideMovement && !block2.IsCollideMovement);
						if (flag)
						{
							num3++;
						}
						array2[j - x, k - z] = flag;
					}
				}
				if (num2 > 0)
				{
					this.createFaces(array, num2, BlockFace.Top, i - y, _voxelMesh);
				}
				if (num3 > 0)
				{
					this.createFaces(array2, num3, BlockFace.Bottom, i - y, _voxelMesh);
				}
			}
		}
		for (int l = z; l < z + vector3i.z; l++)
		{
			int num4 = 0;
			int num5 = 0;
			for (int m = x; m < x + vector3i.x; m++)
			{
				this.nBlocks.Init(m, l);
				for (int n = y; n < y + vector3i.y; n++)
				{
					Block block3 = this.nBlocks.Get(0, n, 0).Block;
					Block block4 = this.nBlocks.Get(0, n, 1).Block;
					bool flag2 = block3.IsCollideMovement && !block4.IsCollideMovement;
					if (flag2)
					{
						num4++;
					}
					array[m - x, n - y] = flag2;
					block4 = this.nBlocks.Get(0, n, -1).Block;
					flag2 = (block3.IsCollideMovement && !block4.IsCollideMovement);
					if (flag2)
					{
						num5++;
					}
					array2[m - x, n - y] = flag2;
				}
			}
			if (num4 > 0)
			{
				this.createFaces(array, num4, BlockFace.North, l - z, _voxelMesh);
			}
			if (num5 > 0)
			{
				this.createFaces(array2, num5, BlockFace.South, l - z, _voxelMesh);
			}
		}
		for (int num6 = x; num6 < x + vector3i.x; num6++)
		{
			int num7 = 0;
			int num8 = 0;
			for (int num9 = y; num9 < y + vector3i.y; num9++)
			{
				for (int num10 = z; num10 < z + vector3i.z; num10++)
				{
					this.nBlocks.Init(num6, num10);
					Block block5 = this.nBlocks.Get(0, num9, 0).Block;
					Block block6 = this.nBlocks.Get(1, num9, 0).Block;
					bool flag3 = block5.IsCollideMovement && !block6.IsCollideMovement;
					if (flag3)
					{
						num7++;
					}
					array[num9 - y, num10 - z] = flag3;
					block6 = this.nBlocks.Get(-1, num9, 0).Block;
					flag3 = (block5.IsCollideMovement && !block6.IsCollideMovement);
					if (flag3)
					{
						num8++;
					}
					array2[num9 - y, num10 - z] = flag3;
				}
			}
			if (num7 > 0)
			{
				this.createFaces(array, num7, BlockFace.East, num6 - x, _voxelMesh);
			}
			if (num8 > 0)
			{
				this.createFaces(array2, num8, BlockFace.West, num6 - x, _voxelMesh);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void createFaces(bool[,] _array, int _count, BlockFace _direction, int _y, VoxelMesh _voxelMesh)
	{
		int length = _array.GetLength(0);
		int num = 0;
		for (int i = 0; i < length; i++)
		{
			for (int j = 0; j < length; j++)
			{
				if (_array[i, j])
				{
					num++;
					int num2 = j;
					for (int k = j + 1; k < length; k++)
					{
						num2 = k;
						if (!_array[i, k])
						{
							num2 = k - 1;
							break;
						}
					}
					int num3 = num2 - j + 1;
					int num4 = i;
					for (int l = i + 1; l < length; l++)
					{
						num4 = l;
						for (int m = j; m < j + num3; m++)
						{
							if (!_array[l, m])
							{
								num4 = l - 1;
								goto IL_9D;
							}
						}
					}
					IL_9D:
					int num5 = num4 - i + 1;
					for (int n = i; n < i + num5; n++)
					{
						for (int num6 = j; num6 < j + num3; num6++)
						{
							_array[n, num6] = false;
						}
					}
					switch (_direction)
					{
					case BlockFace.Top:
						_voxelMesh.AddRectXZFacingUp((float)i, (float)(_y + 1), (float)j, num5, num3);
						break;
					case BlockFace.Bottom:
						_voxelMesh.AddRectXZFacingDown((float)i, (float)_y, (float)j, num5, num3);
						break;
					case BlockFace.North:
						_voxelMesh.AddRectXYFacingNorth((float)i, (float)j, (float)(_y + 1), num5, num3);
						break;
					case BlockFace.West:
						_voxelMesh.AddRectYZFacingWest((float)_y, (float)i, (float)j, num5, num3);
						break;
					case BlockFace.South:
						_voxelMesh.AddRectXYFacingSouth((float)i, (float)j, (float)_y, num5, num3);
						break;
					case BlockFace.East:
						_voxelMesh.AddRectYZFacingEast((float)(_y + 1), (float)i, (float)j, num5, num3);
						break;
					}
				}
				if (num == _count)
				{
					return;
				}
			}
		}
	}

	public void GenerateColorCubeMesh(Vector3i _startPos, Vector3i _endPos, VoxelMesh _voxelMesh)
	{
		Vector3i vector3i = _endPos - _startPos;
		int num = Math.Max(Math.Max(vector3i.x, vector3i.y), vector3i.z);
		BlockValue[,] array = new BlockValue[num, num];
		BlockValue[,] array2 = new BlockValue[num, num];
		int x = _startPos.x;
		int y = _startPos.y;
		int z = _startPos.z;
		for (int i = y + vector3i.y - 1; i >= y; i--)
		{
			if (i != 0 && i != 255)
			{
				int num2 = 0;
				int num3 = 0;
				for (int j = x; j < x + vector3i.x; j++)
				{
					for (int k = z; k < z + vector3i.z; k++)
					{
						this.nBlocks.Init(j, k);
						BlockValue blockValue = this.nBlocks.Get(0, i, 0);
						BlockValue blockValue2 = this.nBlocks.Get(0, i + 1, 0);
						bool flag = blockValue.Block.IsCollideMovement && !blockValue2.Block.IsCollideMovement;
						if (flag)
						{
							num2++;
						}
						array[j - x, k - z] = (flag ? blockValue : BlockValue.Air);
						blockValue2 = this.nBlocks.Get(0, i - 1, 0);
						flag = (blockValue.Block.IsCollideMovement && !blockValue2.Block.IsCollideMovement);
						if (flag)
						{
							num3++;
						}
						array2[j - x, k - z] = (flag ? blockValue : BlockValue.Air);
					}
				}
				if (num2 > 0)
				{
					this.createFaces2(array, num2, BlockFace.Top, i - y, _voxelMesh);
				}
				if (num3 > 0)
				{
					this.createFaces2(array2, num3, BlockFace.Bottom, i - y, _voxelMesh);
				}
			}
		}
		for (int l = z; l < z + vector3i.z; l++)
		{
			int num4 = 0;
			int num5 = 0;
			for (int m = x; m < x + vector3i.x; m++)
			{
				this.nBlocks.Init(m, l);
				for (int n = y; n < y + vector3i.y; n++)
				{
					BlockValue blockValue3 = this.nBlocks.Get(0, n, 0);
					BlockValue blockValue4 = this.nBlocks.Get(0, n, 1);
					bool flag2 = blockValue3.Block.IsCollideMovement && !blockValue4.Block.IsCollideMovement;
					if (flag2)
					{
						num4++;
					}
					array[m - x, n - y] = (flag2 ? blockValue3 : BlockValue.Air);
					blockValue4 = this.nBlocks.Get(0, n, -1);
					flag2 = (blockValue3.Block.IsCollideMovement && !blockValue4.Block.IsCollideMovement);
					if (flag2)
					{
						num5++;
					}
					array2[m - x, n - y] = (flag2 ? blockValue3 : BlockValue.Air);
				}
			}
			if (num4 > 0)
			{
				this.createFaces2(array, num4, BlockFace.North, l - z, _voxelMesh);
			}
			if (num5 > 0)
			{
				this.createFaces2(array2, num5, BlockFace.South, l - z, _voxelMesh);
			}
		}
		for (int num6 = x; num6 < x + vector3i.x; num6++)
		{
			int num7 = 0;
			int num8 = 0;
			for (int num9 = y; num9 < y + vector3i.y; num9++)
			{
				for (int num10 = z; num10 < z + vector3i.z; num10++)
				{
					this.nBlocks.Init(num6, num10);
					BlockValue blockValue5 = this.nBlocks.Get(0, num9, 0);
					BlockValue blockValue6 = this.nBlocks.Get(1, num9, 0);
					bool flag3 = blockValue5.Block.IsCollideMovement && !blockValue6.Block.IsCollideMovement;
					if (flag3)
					{
						num7++;
					}
					array[num9 - y, num10 - z] = (flag3 ? blockValue5 : BlockValue.Air);
					blockValue6 = this.nBlocks.Get(-1, num9, 0);
					flag3 = (blockValue5.Block.IsCollideMovement && !blockValue6.Block.IsCollideMovement);
					if (flag3)
					{
						num8++;
					}
					array2[num9 - y, num10 - z] = (flag3 ? blockValue5 : BlockValue.Air);
				}
			}
			if (num7 > 0)
			{
				this.createFaces2(array, num7, BlockFace.East, num6 - x, _voxelMesh);
			}
			if (num8 > 0)
			{
				this.createFaces2(array2, num8, BlockFace.West, num6 - x, _voxelMesh);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void createFaces2(BlockValue[,] _array, int _count, BlockFace _face, int _y, VoxelMesh _voxelMesh)
	{
		int length = _array.GetLength(0);
		int num = 0;
		for (int i = 0; i < length; i++)
		{
			for (int j = 0; j < length; j++)
			{
				if (!_array[i, j].Equals(BlockValue.Air))
				{
					num++;
					int num2 = j;
					for (int k = j + 1; k < length; k++)
					{
						num2 = k;
						if (!_array[i, k].Equals(_array[i, j]))
						{
							num2 = k - 1;
							break;
						}
					}
					int num3 = num2 - j + 1;
					int num4 = i;
					for (int l = i + 1; l < length; l++)
					{
						num4 = l;
						for (int m = j; m < j + num3; m++)
						{
							if (!_array[l, m].Equals(_array[i, j]))
							{
								num4 = l - 1;
								goto IL_C1;
							}
						}
					}
					IL_C1:
					int num5 = num4 - i + 1;
					for (int n = i; n < i + num5; n++)
					{
						for (int num6 = j; num6 < j + num3; num6++)
						{
							_array[n, num6] = BlockValue.Air;
						}
					}
					Color white = Color.white;
					switch (_face)
					{
					case BlockFace.Top:
						_voxelMesh.AddRectXZFacingUp((float)i, (float)(_y + 1), (float)j, num5, num3, white);
						break;
					case BlockFace.Bottom:
						_voxelMesh.AddRectXZFacingDown((float)i, (float)_y, (float)j, num5, num3, white);
						break;
					case BlockFace.North:
						_voxelMesh.AddRectXYFacingNorth((float)i, (float)j, (float)(_y + 1), num5, num3, white);
						break;
					case BlockFace.West:
						_voxelMesh.AddRectYZFacingWest((float)_y, (float)i, (float)j, num5, num3, white);
						break;
					case BlockFace.South:
						_voxelMesh.AddRectXYFacingSouth((float)i, (float)j, (float)_y, num5, num3, white);
						break;
					case BlockFace.East:
						_voxelMesh.AddRectYZFacingEast((float)(_y + 1), (float)i, (float)j, num5, num3, white);
						break;
					}
				}
				if (num == _count)
				{
					return;
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public INeighborBlockCache nBlocks;

	public delegate bool DelegateBlocksHaveSameFaced(Block _b1, Block _b2);
}
