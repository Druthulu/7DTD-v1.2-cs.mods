using System;
using System.Runtime.CompilerServices;
using UnityEngine;

public static class DecoUtils
{
	public static bool IsBigDeco(BlockValue blockValue, Block block)
	{
		return block.SmallDecorationRadius > 0 || block.BigDecorationRadius > 0 || block.isOversized;
	}

	public static int GetDecoRadius(BlockValue blockValue, Block block)
	{
		int num = Math.Max(block.SmallDecorationRadius, block.BigDecorationRadius);
		if (block.isOversized)
		{
			Bounds oversizedBounds = block.oversizedBounds;
			oversizedBounds.extents += DecoUtils.OVERSIZED_BOUNDS_PADDING_VEC;
			Vector3 extents = oversizedBounds.extents;
			num = Math.Max(num, Math.Max((int)(extents.x + 0.5f), (int)(extents.z + 0.5f)));
		}
		return num;
	}

	public static bool CanPlaceDeco(Chunk cX0Z0, Chunk cX1Z0, Chunk cX0Z1, Chunk cX1Z1, Vector3i blockPos, BlockValue blockValue, DecoUtils.DecoAllowedTest additionalTest = null)
	{
		return DecoUtils.CanPlaceDeco(cX0Z0, blockPos, blockValue, additionalTest) && DecoUtils.CanPlaceDeco(cX1Z0, blockPos, blockValue, additionalTest) && DecoUtils.CanPlaceDeco(cX0Z1, blockPos, blockValue, additionalTest) && DecoUtils.CanPlaceDeco(cX1Z1, blockPos, blockValue, additionalTest);
	}

	public static bool CanPlaceDeco(Chunk chunk, Vector3i blockPos, BlockValue blockValue, DecoUtils.DecoAllowedTest additionalTest = null)
	{
		DecoUtils.<>c__DisplayClass7_0 CS$<>8__locals1;
		CS$<>8__locals1.chunk = chunk;
		CS$<>8__locals1.additionalTest = additionalTest;
		if (CS$<>8__locals1.additionalTest == null)
		{
			CS$<>8__locals1.additionalTest = DecoUtils.DECO_ALLOWED_TEST_DEFAULT;
		}
		if (blockValue.isair)
		{
			return false;
		}
		Block block = blockValue.Block;
		if (block.isMultiBlock && blockValue.ischild)
		{
			return false;
		}
		int num = CS$<>8__locals1.chunk.X * 16;
		int num2 = CS$<>8__locals1.chunk.Z * 16;
		CS$<>8__locals1.x = blockPos.x - num;
		CS$<>8__locals1.z = blockPos.z - num2;
		if (DecoUtils.IsBigDeco(blockValue, block))
		{
			int cxMax = num + 16 - 1;
			int czMax = num2 + 16 - 1;
			return DecoUtils.<CanPlaceDeco>g__CanPlaceBigDecoForBlockPos|7_0(ref CS$<>8__locals1) && DecoUtils.CanPlaceBigDecoForBlockDecorationRadius(CS$<>8__locals1.chunk, num, num2, cxMax, czMax, blockPos, blockValue, block, CS$<>8__locals1.additionalTest) && DecoUtils.CanPlaceBigDecoForBlockOversized(CS$<>8__locals1.chunk, num, num2, cxMax, czMax, blockPos, blockValue, block, CS$<>8__locals1.additionalTest);
		}
		if (CS$<>8__locals1.x < 0 || CS$<>8__locals1.x >= 16 || CS$<>8__locals1.z < 0 || CS$<>8__locals1.z >= 16)
		{
			return true;
		}
		EnumDecoAllowed decoAllowedAt = CS$<>8__locals1.chunk.GetDecoAllowedAt(CS$<>8__locals1.x, CS$<>8__locals1.z);
		return decoAllowedAt.GetSize() < EnumDecoAllowedSize.NoBigNoSmall && CS$<>8__locals1.additionalTest(decoAllowedAt);
	}

	public static bool HasDecoAllowed(BlockValue blockValue)
	{
		if (blockValue.isair)
		{
			return false;
		}
		Block block = blockValue.Block;
		return (!block.isMultiBlock || !blockValue.ischild) && (block.SmallDecorationRadius > 0 || block.BigDecorationRadius > 0 || block.isOversized);
	}

	public static void ApplyDecoAllowed(Chunk cX0Z0, Chunk cX1Z0, Chunk cX0Z1, Chunk cX1Z1, Vector3i blockPos, BlockValue blockValue)
	{
		DecoUtils.ApplyDecoAllowed(cX0Z0, blockPos, blockValue);
		DecoUtils.ApplyDecoAllowed(cX1Z0, blockPos, blockValue);
		DecoUtils.ApplyDecoAllowed(cX0Z1, blockPos, blockValue);
		DecoUtils.ApplyDecoAllowed(cX1Z1, blockPos, blockValue);
	}

	public static void ApplyDecoAllowed(Chunk chunk, Vector3i blockPos, BlockValue blockValue)
	{
		if (!DecoUtils.HasDecoAllowed(blockValue))
		{
			return;
		}
		Block block = blockValue.Block;
		int num = chunk.X * 16;
		int num2 = chunk.Z * 16;
		int cxMax = num + 16 - 1;
		int czMax = num2 + 16 - 1;
		DecoUtils.ApplyDecoAllowedForBlockDecorationRadius(chunk, num, num2, cxMax, czMax, blockPos, blockValue, block);
		DecoUtils.ApplyDecoAllowedForBlockOversized(chunk, num, num2, cxMax, czMax, blockPos, blockValue, block);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool CanPlaceBigDecoForBlockDecorationRadius(Chunk chunk, int cxMin, int czMin, int cxMax, int czMax, Vector3i blockPos, BlockValue blockValue, Block block, DecoUtils.DecoAllowedTest additionalTest)
	{
		int num = Math.Max(block.SmallDecorationRadius, block.BigDecorationRadius);
		if (num <= 0)
		{
			return true;
		}
		int num2 = blockPos.x - num;
		int num3 = blockPos.z - num;
		int num4 = blockPos.x + num;
		int num5 = blockPos.z + num;
		if (num2 > cxMax || num3 > czMax || num4 < cxMin || num5 < czMin)
		{
			return true;
		}
		int num6 = Math.Clamp(num2 - cxMin, 0, 15);
		int num7 = Math.Clamp(num3 - czMin, 0, 15);
		int num8 = Math.Clamp(num4 - cxMin, 0, 15);
		int num9 = Math.Clamp(num5 - czMin, 0, 15);
		for (int i = num7; i <= num9; i++)
		{
			for (int j = num6; j <= num8; j++)
			{
				EnumDecoAllowed decoAllowedAt = chunk.GetDecoAllowedAt(j, i);
				if (decoAllowedAt.GetSize() >= EnumDecoAllowedSize.NoBigOnlySmall && additionalTest(decoAllowedAt))
				{
					return false;
				}
			}
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ApplyDecoAllowedForBlockDecorationRadius(Chunk chunk, int cxMin, int czMin, int cxMax, int czMax, Vector3i blockPos, BlockValue blockValue, Block block)
	{
		int smallDecorationRadius = block.SmallDecorationRadius;
		int num = Math.Max(smallDecorationRadius, block.BigDecorationRadius);
		if (num <= 0)
		{
			return;
		}
		int num2 = blockPos.x - num;
		int num3 = blockPos.z - num;
		int num4 = blockPos.x + num;
		int num5 = blockPos.z + num;
		if (num2 > cxMax || num3 > czMax || num4 < cxMin || num5 < czMin)
		{
			return;
		}
		int num6 = Math.Clamp(num2 - cxMin, 0, 15);
		int num7 = Math.Clamp(num3 - czMin, 0, 15);
		int num8 = Math.Clamp(num4 - cxMin, 0, 15);
		int num9 = Math.Clamp(num5 - czMin, 0, 15);
		for (int i = num7; i <= num9; i++)
		{
			for (int j = num6; j <= num8; j++)
			{
				if (smallDecorationRadius == num || (smallDecorationRadius > 0 && Math.Max(Math.Abs(i), Math.Abs(j)) <= smallDecorationRadius))
				{
					chunk.SetDecoAllowedSizeAt(j, i, EnumDecoAllowedSize.NoBigNoSmall);
				}
				else
				{
					chunk.SetDecoAllowedSizeAt(j, i, EnumDecoAllowedSize.NoBigOnlySmall);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool CanPlaceBigDecoForBlockOversized(Chunk chunk, int cxMin, int czMin, int cxMax, int czMax, Vector3i blockPos, BlockValue blockValue, Block block, DecoUtils.DecoAllowedTest additionalTest)
	{
		Bounds oversizedBounds = block.oversizedBounds;
		oversizedBounds.extents += DecoUtils.OVERSIZED_BOUNDS_PADDING_VEC;
		Quaternion rotation = block.shape.GetRotation(blockValue);
		Vector3i vector3i;
		Vector3i vector3i2;
		OversizedBlockUtils.GetWorldAlignedBoundsExtents(blockPos, rotation, oversizedBounds, out vector3i, out vector3i2);
		if (vector3i.x > cxMax || vector3i.z > czMax || vector3i2.x < cxMin || vector3i2.z < czMin)
		{
			return true;
		}
		int num = Math.Clamp(vector3i.x - cxMin, 0, 15);
		int num2 = Math.Clamp(vector3i.z - czMin, 0, 15);
		int num3 = Math.Clamp(vector3i2.x - cxMin, 0, 15);
		int num4 = Math.Clamp(vector3i2.z - czMin, 0, 15);
		Matrix4x4 blockWorldToLocalMatrix = OversizedBlockUtils.GetBlockWorldToLocalMatrix(blockPos, rotation);
		Vector3i blockPosition = blockPos;
		for (int i = num2; i <= num4; i++)
		{
			blockPosition.z = czMin + i;
			for (int j = num; j <= num3; j++)
			{
				blockPosition.x = cxMin + j;
				if (OversizedBlockUtils.IsBlockCenterWithinBounds(blockPosition, oversizedBounds, blockWorldToLocalMatrix))
				{
					EnumDecoAllowed decoAllowedAt = chunk.GetDecoAllowedAt(j, i);
					if (decoAllowedAt.GetSize() >= EnumDecoAllowedSize.NoBigOnlySmall && additionalTest(decoAllowedAt))
					{
						return false;
					}
				}
			}
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ApplyDecoAllowedForBlockOversized(Chunk chunk, int cxMin, int czMin, int cxMax, int czMax, Vector3i blockPos, BlockValue blockValue, Block block)
	{
		Bounds oversizedBounds = block.oversizedBounds;
		oversizedBounds.extents += DecoUtils.OVERSIZED_BOUNDS_PADDING_VEC;
		Quaternion rotation = block.shape.GetRotation(blockValue);
		Vector3i vector3i;
		Vector3i vector3i2;
		OversizedBlockUtils.GetWorldAlignedBoundsExtents(blockPos, rotation, oversizedBounds, out vector3i, out vector3i2);
		if (vector3i.x > cxMax || vector3i.z > czMax || vector3i2.x < cxMin || vector3i2.z < czMin)
		{
			return;
		}
		int num = Math.Clamp(vector3i.x - cxMin, 0, 15);
		int num2 = Math.Clamp(vector3i.z - czMin, 0, 15);
		int num3 = Math.Clamp(vector3i2.x - cxMin, 0, 15);
		int num4 = Math.Clamp(vector3i2.z - czMin, 0, 15);
		Matrix4x4 blockWorldToLocalMatrix = OversizedBlockUtils.GetBlockWorldToLocalMatrix(blockPos, rotation);
		Vector3i blockPosition = blockPos;
		for (int i = num2; i <= num4; i++)
		{
			blockPosition.z = czMin + i;
			for (int j = num; j <= num3; j++)
			{
				blockPosition.x = cxMin + j;
				if (OversizedBlockUtils.IsBlockCenterWithinBounds(blockPosition, oversizedBounds, blockWorldToLocalMatrix))
				{
					chunk.SetDecoAllowedSizeAt(j, i, EnumDecoAllowedSize.NoBigNoSmall);
				}
				else
				{
					chunk.SetDecoAllowedSizeAt(j, i, EnumDecoAllowedSize.NoBigOnlySmall);
				}
			}
		}
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Internal)]
	public static bool <CanPlaceDeco>g__CanPlaceBigDecoForBlockPos|7_0(ref DecoUtils.<>c__DisplayClass7_0 A_0)
	{
		if (A_0.x < 0 || A_0.x >= 16 || A_0.z < 0 || A_0.z >= 16)
		{
			return true;
		}
		EnumDecoAllowed decoAllowedAt = A_0.chunk.GetDecoAllowedAt(A_0.x, A_0.z);
		return decoAllowedAt.GetSize() < EnumDecoAllowedSize.NoBigOnlySmall && A_0.additionalTest(decoAllowedAt);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const float OVERSIZED_BOUNDS_PADDING = 0.71f;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Vector3 OVERSIZED_BOUNDS_PADDING_VEC = new Vector3(0.71f, 0.71f, 0.71f);

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly DecoUtils.DecoAllowedTest DECO_ALLOWED_TEST_DEFAULT = (EnumDecoAllowed _) => true;

	public delegate bool DecoAllowedTest(EnumDecoAllowed decoAllowed);
}
