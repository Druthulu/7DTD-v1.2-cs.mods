using System;
using SharpEXR.AttributeTypes;

namespace SharpEXR.ColorSpace
{
	public static class XYZ
	{
		public static tMat3x3 CalcColorSpaceConversion_RGB_to_XYZ(Chromaticities chromaticities)
		{
			return XYZ.CalcColorSpaceConversion_RGB_to_XYZ(new tVec2(chromaticities.RedX, chromaticities.RedY), new tVec2(chromaticities.GreenX, chromaticities.GreenY), new tVec2(chromaticities.BlueX, chromaticities.BlueY), new tVec2(chromaticities.WhiteX, chromaticities.WhiteY));
		}

		public static tMat3x3 CalcColorSpaceConversion_RGB_to_XYZ(tVec2 red_xy, tVec2 green_xy, tVec2 blue_xy, tVec2 white_xy)
		{
			tMat3x3 result = default(tMat3x3);
			tVec3 vec = new tVec3(red_xy.X, red_xy.Y, 1f - (red_xy.X + red_xy.Y));
			tVec3 vec2 = new tVec3(green_xy.X, green_xy.Y, 1f - (green_xy.X + green_xy.Y));
			tVec3 vec3 = new tVec3(blue_xy.X, blue_xy.Y, 1f - (blue_xy.X + blue_xy.Y));
			tVec3 vec4 = new tVec3(white_xy.X, white_xy.Y, 1f - (white_xy.X + white_xy.Y));
			vec4.X /= white_xy.Y;
			vec4.Y /= white_xy.Y;
			vec4.Z /= white_xy.Y;
			result.SetCol(0, vec);
			result.SetCol(1, vec2);
			result.SetCol(2, vec3);
			tMat3x3 mat;
			result.Invert(out mat);
			tVec3 tVec = mat * vec4;
			ref tMat3x3 ptr = ref result;
			ptr[0, 0] = ptr[0, 0] * tVec.X;
			ptr = ref result;
			ptr[1, 0] = ptr[1, 0] * tVec.X;
			ptr = ref result;
			ptr[2, 0] = ptr[2, 0] * tVec.X;
			ptr = ref result;
			ptr[0, 1] = ptr[0, 1] * tVec.Y;
			ptr = ref result;
			ptr[1, 1] = ptr[1, 1] * tVec.Y;
			ptr = ref result;
			ptr[2, 1] = ptr[2, 1] * tVec.Y;
			ptr = ref result;
			ptr[0, 2] = ptr[0, 2] * tVec.Z;
			ptr = ref result;
			ptr[1, 2] = ptr[1, 2] * tVec.Z;
			ptr = ref result;
			ptr[2, 2] = ptr[2, 2] * tVec.Z;
			return result;
		}
	}
}
