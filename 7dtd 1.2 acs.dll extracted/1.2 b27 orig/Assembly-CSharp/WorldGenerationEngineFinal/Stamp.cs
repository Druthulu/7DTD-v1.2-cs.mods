﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace WorldGenerationEngineFinal
{
	public class Stamp
	{
		public int imageHeight
		{
			get
			{
				return this.stamp.height;
			}
		}

		public int imageWidth
		{
			get
			{
				return this.stamp.width;
			}
		}

		public Stamp(WorldBuilder _worldBuilder, RawStamp _stamp, TranslationData _transData, bool _isCustomColor = false, Color _customColor = default(Color), float _biomeAlphaCutoff = 0.1f, bool _isWater = false, string stampName = "")
		{
			this.worldBuilder = _worldBuilder;
			this.stamp = _stamp;
			this.transform = _transData;
			this.scale = this.transform.scale;
			this.isCustomColor = _isCustomColor;
			this.customColor = _customColor;
			this.biomeAlphaCutoff = _biomeAlphaCutoff;
			this.isWater = _isWater;
			this.Name = stampName;
			this.alpha = 1f;
			this.additive = false;
			int rotation = this.transform.rotation;
			int num = (int)((float)_stamp.width * this.scale * 1.4f);
			int num2 = (int)((float)_stamp.height * this.scale * 1.4f);
			int x = this.transform.x - num / 2;
			int x2 = this.transform.x + num / 2;
			int y = this.transform.y - num2 / 2;
			int y2 = this.transform.y + num2 / 2;
			int x3 = this.transform.x;
			int y3 = this.transform.y;
			Vector2i rotatedPoint = this.getRotatedPoint(x, y, x3, y3, rotation);
			Vector2i rotatedPoint2 = this.getRotatedPoint(x2, y, x3, y3, rotation);
			Vector2i rotatedPoint3 = this.getRotatedPoint(x, y2, x3, y3, rotation);
			Vector2i rotatedPoint4 = this.getRotatedPoint(x2, y2, x3, y3, rotation);
			Vector2 vector = new Vector2((float)Mathf.Min(Mathf.Min(rotatedPoint.x, rotatedPoint2.x), Mathf.Min(rotatedPoint3.x, rotatedPoint4.x)), (float)Mathf.Min(Mathf.Min(rotatedPoint.y, rotatedPoint2.y), Mathf.Min(rotatedPoint3.y, rotatedPoint4.y)));
			Vector2 a = new Vector2((float)Mathf.Max(Mathf.Max(rotatedPoint.x, rotatedPoint2.x), Mathf.Max(rotatedPoint3.x, rotatedPoint4.x)), (float)Mathf.Max(Mathf.Max(rotatedPoint.y, rotatedPoint2.y), Mathf.Max(rotatedPoint3.y, rotatedPoint4.y)));
			this.Area = new Rect(vector, a - vector);
			if (this.isWater)
			{
				if (this.worldBuilder.waterRects == null)
				{
					this.worldBuilder.waterRects = new List<Rect>();
				}
				this.worldBuilder.waterRects.Add(this.Area);
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public Color[] rotateColorArray(Color[] src, float angle, int width, int height)
		{
			Color[] array = new Color[width * height];
			double num = Math.Sin(0.017453292519943295 * (double)angle);
			double num2 = Math.Cos(0.017453292519943295 * (double)angle);
			int num3 = width / 2;
			int num4 = height / 2;
			for (int i = 0; i < height; i++)
			{
				for (int j = 0; j < width; j++)
				{
					float num5 = (float)(num2 * (double)(j - num3) + num * (double)(i - num4) + (double)num3);
					float num6 = (float)(-(float)num * (double)(j - num3) + num2 * (double)(i - num4) + (double)num4);
					int num7 = (int)num5;
					int num8 = (int)num6;
					num5 -= (float)num7;
					num6 -= (float)num8;
					if (num7 >= 0 && num7 < width && num8 >= 0 && num8 < height)
					{
						Color color = src[num8 * width + num7];
						Color rightVal = color;
						Color upVal = color;
						Color upRightVal = color;
						if (num7 + 1 < width)
						{
							rightVal = src[num8 * width + num7 + 1];
						}
						if (num8 + 1 < height)
						{
							upVal = src[(num8 + 1) * width + num7];
						}
						if (num7 + 1 < width && num8 + 1 < height)
						{
							upRightVal = src[(num8 + 1) * width + num7 + 1];
						}
						array[i * width + j] = this.QuadLerpColor(color, rightVal, upRightVal, upVal, num5, num6);
					}
				}
			}
			return array;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public Color QuadLerpColor(Color selfVal, Color rightVal, Color upRightVal, Color upVal, float horizontalPerc, float verticalPerc)
		{
			return Color.Lerp(Color.Lerp(selfVal, rightVal, horizontalPerc), Color.Lerp(upVal, upRightVal, horizontalPerc), verticalPerc);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public Vector2i getRotatedPoint(int x, int y, int cx, int cy, int angle)
		{
			double num = Math.Cos((double)angle);
			double num2 = Math.Sin((double)angle);
			return new Vector2i(Mathf.RoundToInt((float)((double)(x - cx) * num - (double)(y - cy) * num2 + (double)cx)), Mathf.RoundToInt((float)((double)(x - cx) * num2 + (double)(y - cy) * num + (double)cy)));
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly WorldBuilder worldBuilder;

		public TranslationData transform;

		public float alpha;

		public bool additive;

		public float scale;

		public bool isCustomColor;

		public Color customColor;

		public Rect Area;

		public float biomeAlphaCutoff;

		public bool isWater;

		public string Name = "";

		public RawStamp stamp;

		[PublicizedFrom(EAccessModifier.Private)]
		public const float oneByoneScale = 1.4f;
	}
}
