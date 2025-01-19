﻿using System;
using UnityEngine;

public static class BlockFaceFlags
{
	[PublicizedFrom(EAccessModifier.Private)]
	static BlockFaceFlags()
	{
		for (int i = 0; i < 24; i++)
		{
			for (int j = 0; j < 6; j++)
			{
				BlockFaceFlags.faceRotShiftValues[i * 6 + j] = (int)(BlockFaces.RotateFace((BlockFace)j, i) - (BlockFace)j);
			}
		}
	}

	public static BlockFaceFlag RotateFlags(BlockFaceFlag mask, byte blockRotation)
	{
		if (mask == BlockFaceFlag.None || mask == BlockFaceFlag.All || blockRotation > 23)
		{
			return mask;
		}
		int num = 0;
		for (int i = 0; i < 6; i++)
		{
			int num2 = (int)(mask & (BlockFaceFlag)(1 << i));
			if (num2 != 0)
			{
				int num3 = BlockFaceFlags.faceRotShiftValues[(int)(blockRotation * 6) + i];
				if (num3 > 0)
				{
					num2 <<= num3;
				}
				else
				{
					num2 >>= -num3;
				}
				num |= num2;
			}
		}
		return (BlockFaceFlag)num;
	}

	public static BlockFace ToBlockFace(BlockFaceFlag flags)
	{
		if ((flags & BlockFaceFlag.Top) != BlockFaceFlag.None)
		{
			return BlockFace.Top;
		}
		if ((flags & BlockFaceFlag.Bottom) != BlockFaceFlag.None)
		{
			return BlockFace.Bottom;
		}
		if ((flags & BlockFaceFlag.North) != BlockFaceFlag.None)
		{
			return BlockFace.North;
		}
		if ((flags & BlockFaceFlag.South) != BlockFaceFlag.None)
		{
			return BlockFace.South;
		}
		if ((flags & BlockFaceFlag.East) != BlockFaceFlag.None)
		{
			return BlockFace.East;
		}
		if ((flags & BlockFaceFlag.West) != BlockFaceFlag.None)
		{
			return BlockFace.West;
		}
		return BlockFace.None;
	}

	public static BlockFaceFlag FromBlockFace(BlockFace face)
	{
		if (face == BlockFace.None)
		{
			return BlockFaceFlag.None;
		}
		return (BlockFaceFlag)(1 << (int)face);
	}

	public static BlockFace OppositeFace(BlockFace face)
	{
		switch (face)
		{
		case BlockFace.Top:
			return BlockFace.Bottom;
		case BlockFace.Bottom:
			return BlockFace.Top;
		case BlockFace.North:
			return BlockFace.South;
		case BlockFace.West:
			return BlockFace.East;
		case BlockFace.South:
			return BlockFace.North;
		case BlockFace.East:
			return BlockFace.West;
		default:
			return BlockFace.None;
		}
	}

	public static Vector3 OffsetForFace(BlockFace face)
	{
		switch (face)
		{
		case BlockFace.Top:
			return Vector3.up;
		case BlockFace.Bottom:
			return Vector3.down;
		case BlockFace.North:
			return Vector3.forward;
		case BlockFace.West:
			return Vector3.left;
		case BlockFace.South:
			return Vector3.back;
		case BlockFace.East:
			return Vector3.right;
		default:
			return Vector3.zero;
		}
	}

	public static Vector3i OffsetIForFace(BlockFace face)
	{
		switch (face)
		{
		case BlockFace.Top:
			return Vector3i.up;
		case BlockFace.Bottom:
			return Vector3i.down;
		case BlockFace.North:
			return Vector3i.forward;
		case BlockFace.West:
			return Vector3i.left;
		case BlockFace.South:
			return Vector3i.back;
		case BlockFace.East:
			return Vector3i.right;
		default:
			return Vector3i.zero;
		}
	}

	public static BlockFaceFlag OppositeFaceFlag(BlockFace face)
	{
		return BlockFaceFlags.FromBlockFace(BlockFaceFlags.OppositeFace(face));
	}

	public static float YawForDirection(BlockFace face)
	{
		switch (face)
		{
		case BlockFace.West:
			return 270f;
		case BlockFace.South:
			return 180f;
		case BlockFace.East:
			return 90f;
		default:
			return 0f;
		}
	}

	public static BlockFaceFlag FrontSidesFromPosition(Vector3i blockPos, Vector3 entityPos)
	{
		BlockFaceFlag blockFaceFlag = BlockFaceFlag.None;
		if (entityPos.x < (float)blockPos.x)
		{
			blockFaceFlag |= BlockFaceFlag.West;
		}
		if (entityPos.x >= (float)(blockPos.x + 1))
		{
			blockFaceFlag |= BlockFaceFlag.East;
		}
		if (entityPos.y < (float)blockPos.y)
		{
			blockFaceFlag |= BlockFaceFlag.Bottom;
		}
		if (entityPos.y >= (float)(blockPos.y + 1))
		{
			blockFaceFlag |= BlockFaceFlag.Top;
		}
		if (entityPos.z < (float)blockPos.z)
		{
			blockFaceFlag |= BlockFaceFlag.South;
		}
		if (entityPos.z >= (float)(blockPos.z + 1))
		{
			blockFaceFlag |= BlockFaceFlag.North;
		}
		return blockFaceFlag;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly int[] faceRotShiftValues = new int[144];
}
