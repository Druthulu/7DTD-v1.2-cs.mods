﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace WorldGenerationEngineFinal
{
	public class StampManager
	{
		public StampManager(WorldBuilder _worldBuilder)
		{
			this.worldBuilder = _worldBuilder;
		}

		public void ClearStamps()
		{
			this.AllStamps.Clear();
		}

		public void DrawStampGroup(StampGroup _group, float[] _dest, float[] _waterDest, int size)
		{
			MicroStopwatch microStopwatch = new MicroStopwatch(true);
			for (int i = 0; i < _group.Stamps.Count; i++)
			{
				Stamp stamp = _group.Stamps[i];
				if (stamp != null)
				{
					this.DrawStamp(_dest, _waterDest, stamp);
					if (this.worldBuilder.IsCanceled)
					{
						break;
					}
				}
			}
			Log.Out("DrawStampGroup '{0}', count {1}, in {2}", new object[]
			{
				_group.Name,
				_group.Stamps.Count,
				(float)microStopwatch.ElapsedMilliseconds * 0.001f
			});
		}

		public void DrawStampGroup(StampGroup _group, Color32[] _image, int size, float _stampScale = 1f)
		{
			MicroStopwatch microStopwatch = new MicroStopwatch(true);
			for (int i = 0; i < _group.Stamps.Count; i++)
			{
				Stamp stamp = _group.Stamps[i];
				if (stamp != null)
				{
					int x = (int)((float)stamp.transform.x * _stampScale);
					int y = (int)((float)stamp.transform.y * _stampScale);
					StampManager.DrawStamp(_image, stamp.stamp, x, y, size, size, stamp.imageWidth, stamp.imageHeight, stamp.alpha, stamp.scale * _stampScale, stamp.isCustomColor, stamp.customColor, stamp.biomeAlphaCutoff, (float)stamp.transform.rotation, stamp.isWater);
					if (this.worldBuilder.IsCanceled)
					{
						break;
					}
				}
			}
			Log.Out("DrawStampGroup c32 '{0}', count {1}, in {2}", new object[]
			{
				_group.Name,
				_group.Stamps.Count,
				(float)microStopwatch.ElapsedMilliseconds * 0.001f
			});
		}

		public void DrawWaterStampGroup(StampGroup _group, float[] _dest, int _destSize)
		{
			MicroStopwatch microStopwatch = new MicroStopwatch(true);
			for (int i = 0; i < _group.Stamps.Count; i++)
			{
				Stamp stamp = _group.Stamps[i];
				if (stamp != null)
				{
					StampManager.DrawWaterStamp(stamp, _dest, _destSize);
					if (this.worldBuilder.IsCanceled)
					{
						break;
					}
				}
			}
			Log.Out("DrawWaterStampGroup '{0}', count {1}, in {2}", new object[]
			{
				_group.Name,
				_group.Stamps.Count,
				(float)microStopwatch.ElapsedMilliseconds * 0.001f
			});
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static float CalcRotatedValue(float x1, float y1, float[] src, double sine, double cosine, int width, int height, bool isWater = false)
		{
			int num = width >> 1;
			int num2 = height >> 1;
			float num3 = (float)(cosine * (double)(x1 - (float)num) + sine * (double)(y1 - (float)num2) + (double)num);
			int num4 = (int)num3;
			if ((ulong)num4 >= (ulong)((long)width))
			{
				return 0f;
			}
			float num5 = (float)(-(float)sine * (double)(x1 - (float)num) + cosine * (double)(y1 - (float)num2) + (double)num2);
			int num6 = (int)num5;
			if ((ulong)num6 >= (ulong)((long)height))
			{
				return 0f;
			}
			int num7 = num4 + num6 * width;
			float num8 = src[num7];
			float num9;
			if (num4 + 1 < width)
			{
				num9 = src[num7 + 1];
			}
			else
			{
				num9 = num8;
			}
			float num10;
			if (num6 + 1 < height)
			{
				num10 = src[num7 + width];
			}
			else
			{
				num10 = num8;
			}
			float num11;
			if (num4 + 1 < width && num6 + 1 < height)
			{
				num11 = src[num7 + width + 1];
			}
			else
			{
				num11 = num8;
			}
			if (isWater && (num8 > 0f || num10 > 0f || num9 > 0f || num11 > 0f))
			{
				return num8;
			}
			num3 -= (float)num4;
			num5 -= (float)num6;
			num8 += (num9 - num8) * num3;
			float num12 = num10 + (num11 - num10) * num3;
			return num8 + (num12 - num8) * num5;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static double CalcRotatedValue(StampManager.RotateParams _p, double x1, double y1, float[] src)
		{
			double num = _p.cosine * (x1 - _p.halfWidth) + _p.sine * (y1 - _p.halfHeight) + _p.halfWidth;
			int num2 = (int)num;
			if ((ulong)num2 >= (ulong)((long)_p.width))
			{
				return 0.0;
			}
			double num3 = -_p.sine * (x1 - _p.halfWidth) + _p.cosine * (y1 - _p.halfHeight) + _p.halfHeight;
			int num4 = (int)num3;
			if ((ulong)num4 >= (ulong)((long)_p.height))
			{
				return 0.0;
			}
			int num5 = num2 + num4 * _p.width;
			double num6 = (double)src[num5];
			double num7 = num6;
			if (num2 + 1 < _p.width)
			{
				num7 = (double)src[num5 + 1];
			}
			double num8 = num6;
			if (num4 + 1 < _p.height)
			{
				num8 = (double)src[num5 + _p.width];
			}
			double num9 = num6;
			if (num2 + 1 < _p.width && num4 + 1 < _p.height)
			{
				num9 = (double)src[num5 + _p.width + 1];
			}
			if (_p.isWater && (num6 > 0.0 || num8 > 0.0 || num7 > 0.0 || num9 > 0.0))
			{
				return num6;
			}
			num -= (double)num2;
			num3 -= (double)num4;
			double num10 = num6 + (num7 - num6) * num;
			double num11 = num8 + (num9 - num8) * num;
			return num10 + (num11 - num10) * num3;
		}

		public void DrawStamp(float[] _dest, float[] _waterDest, Stamp stamp)
		{
			int x = stamp.transform.x;
			int y = stamp.transform.y;
			int worldSize = this.worldBuilder.WorldSize;
			float num = (float)stamp.transform.rotation;
			StampManager.DrawStamp(_dest, _waterDest, stamp.stamp, x, y, worldSize, worldSize, (double)stamp.alpha, stamp.additive, (double)stamp.scale, stamp.isCustomColor, stamp.customColor, (double)stamp.biomeAlphaCutoff, (double)num);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static void DrawStamp(float[] _dest, float[] _waterDest, RawStamp _src, int _x, int _y, int _destWidth, int _destHeight, double _alpha, bool _additive, double _scale, bool _isCustomColor, Color _customColor, double _biomeCutoff, double _angle)
		{
			_x -= (int)((double)_src.width * _scale) / 2;
			_y -= (int)((double)_src.height * _scale) / 2;
			double num = 0.01745329238474369 * _angle;
			double sine = Math.Sin(num);
			double cosine = Math.Cos(num);
			int num2 = (int)Math.Floor((double)(((int)Math.Sqrt((double)(_src.width * _src.width + _src.height * _src.height)) - _src.width) / 2) * _scale);
			int num3 = (int)Math.Floor((double)_src.width * _scale + (double)num2);
			num2 = -num2;
			int num4 = num2;
			int num5 = _x + num2;
			if (num5 < 0)
			{
				num4 -= num5;
			}
			int num6 = num3;
			num5 = _x + num3;
			if (num5 >= _destWidth)
			{
				num6 -= num5 - _destWidth;
			}
			int num7 = num2;
			int num8 = _y + num2;
			if (num8 < 0)
			{
				num7 -= num8;
			}
			int num9 = num3;
			num8 = _y + num3;
			if (num8 >= _destHeight)
			{
				num9 -= num8 - _destHeight;
			}
			StampManager.RotateParams rotateParams = new StampManager.RotateParams();
			rotateParams.sine = sine;
			rotateParams.cosine = cosine;
			rotateParams.width = _src.width;
			rotateParams.height = _src.height;
			rotateParams.halfWidth = (double)(_src.width / 2);
			rotateParams.halfHeight = (double)(_src.height / 2);
			for (int i = num7; i < num9; i++)
			{
				int num10 = (_y + i) * _destWidth;
				double y = (double)i / _scale;
				for (int j = num4; j < num6; j++)
				{
					double x = (double)j / _scale;
					double num11 = (double)_src.alphaConst;
					if (_src.alphaPixels != null)
					{
						num11 = StampManager.CalcRotatedValue(rotateParams, x, y, _src.alphaPixels);
					}
					if (num11 >= 9.9999997473787516E-06)
					{
						int num12 = _x + j + num10;
						if (_isCustomColor)
						{
							if (num11 > _biomeCutoff)
							{
								_dest[num12] = _customColor.r;
								_waterDest[num12] = _customColor.b;
							}
						}
						else
						{
							double num13 = (double)_src.heightConst;
							if (_src.heightPixels != null)
							{
								num13 = StampManager.CalcRotatedValue(rotateParams, x, y, _src.heightPixels);
							}
							double num14 = num13;
							if (_src.waterPixels != null)
							{
								num14 = StampManager.CalcRotatedValue(rotateParams, x, y, _src.waterPixels);
							}
							double num15 = num11 * _alpha;
							double num16 = (double)_dest[num12];
							if (_additive)
							{
								num16 += num13 * num15;
							}
							else
							{
								num16 += (num13 - num16) * num15;
							}
							_dest[num12] = (float)num16;
							double num17 = (double)_waterDest[num12];
							num17 += (num14 - num17) * num15;
							_waterDest[num12] = (float)num17;
						}
					}
				}
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void DrawStamp(Color32[] _dest, Stamp stamp)
		{
			int x = stamp.transform.x;
			int y = stamp.transform.y;
			int worldSize = this.worldBuilder.WorldSize;
			float angle = (float)stamp.transform.rotation;
			StampManager.DrawStamp(_dest, stamp.stamp, x, y, worldSize, worldSize, stamp.imageWidth, stamp.imageHeight, stamp.alpha, stamp.scale, stamp.isCustomColor, stamp.customColor, stamp.biomeAlphaCutoff, angle, stamp.isWater);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static void DrawStamp(Color32[] _dest, RawStamp _src, int _x, int _y, int _destWidth, int _destHeight, int _srcWidth, int _srcHeight, float _alpha, float _scale, bool _isCustomColor, Color32 _customColor, float _biomeCutoff = 0.1f, float _angle = 0f, bool isWater = false)
		{
			_x -= (int)((float)_srcWidth * _scale) / 2;
			_y -= (int)((float)_srcHeight * _scale) / 2;
			double num = (double)(0.0174532924f * _angle);
			double sine = Math.Sin(num);
			double cosine = Math.Cos(num);
			int num2 = Mathf.FloorToInt((float)(((int)Mathf.Sqrt((float)(_srcWidth * _srcWidth + _srcHeight * _srcHeight)) - _srcWidth) / 2) * _scale);
			int num3 = Mathf.FloorToInt((float)_srcWidth * _scale + (float)num2);
			num2 = -num2;
			int num4 = num2;
			int num5 = _x + num2;
			if (num5 < 0)
			{
				num4 -= num5;
			}
			int num6 = num3;
			num5 = _x + num3;
			if (num5 >= _destWidth)
			{
				num6 -= num5 - _destWidth;
			}
			int num7 = num2;
			int num8 = _y + num2;
			if (num8 < 0)
			{
				num7 -= num8;
			}
			int num9 = num3;
			num8 = _y + num3;
			if (num8 >= _destHeight)
			{
				num9 -= num8 - _destHeight;
			}
			for (int i = num7; i < num9; i++)
			{
				int num10 = (_y + i) * _destWidth;
				float y = (float)i / _scale;
				for (int j = num4; j < num6; j++)
				{
					float num11 = _src.alphaConst;
					if (_src.alphaPixels != null)
					{
						num11 = StampManager.CalcRotatedValue((float)j / _scale, y, _src.alphaPixels, sine, cosine, _srcWidth, _srcHeight, isWater);
					}
					if (num11 > _biomeCutoff)
					{
						int num12 = _x + j + num10;
						_dest[num12] = _customColor;
					}
				}
			}
		}

		public static void DrawWaterStamp(Stamp stamp, float[] _dest, int _destSize)
		{
			if (!stamp.isWater)
			{
				throw new ArgumentException("DrawWaterStamp called with non-water stamp " + stamp.Name);
			}
			float[] array = stamp.stamp.waterPixels;
			if (array == null)
			{
				array = stamp.stamp.alphaPixels;
				if (array == null)
				{
					return;
				}
			}
			int x = stamp.transform.x;
			int y = stamp.transform.y;
			float angle = (float)stamp.transform.rotation;
			StampManager.DrawWaterStamp(_dest, array, x, y, _destSize, _destSize, stamp.imageWidth, stamp.imageHeight, stamp.alpha, (double)stamp.scale, stamp.isCustomColor, stamp.customColor, angle);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static void DrawWaterStamp(float[] _dest, float[] _src, int _x, int _y, int _destWidth, int _destHeight, int _srcWidth, int _srcHeight, float _alpha, double _scale, bool _isCustomColor, Color _customColor, float _angle)
		{
			_x -= (int)((double)_srcWidth * _scale) / 2;
			_y -= (int)((double)_srcHeight * _scale) / 2;
			double num = (double)(0.0174532924f * _angle);
			double sine = Math.Sin(num);
			double cosine = Math.Cos(num);
			int num2 = (int)Math.Floor((double)(((int)Math.Sqrt((double)(_srcWidth * _srcWidth + _srcHeight * _srcHeight)) - _srcWidth) / 2) * _scale);
			int num3 = (int)Math.Floor((double)_srcWidth * _scale + (double)num2);
			num2 = -num2;
			int num4 = num2;
			int num5 = _x + num2;
			if (num5 < 0)
			{
				num4 -= num5;
			}
			int num6 = num3;
			num5 = _x + num3;
			if (num5 >= _destWidth)
			{
				num6 -= num5 - _destWidth;
			}
			int num7 = num2;
			int num8 = _y + num2;
			if (num8 < 0)
			{
				num7 -= num8;
			}
			int num9 = num3;
			num8 = _y + num3;
			if (num8 >= _destHeight)
			{
				num9 -= num8 - _destHeight;
			}
			StampManager.RotateParams rotateParams = new StampManager.RotateParams();
			rotateParams.sine = sine;
			rotateParams.cosine = cosine;
			rotateParams.width = _srcWidth;
			rotateParams.height = _srcHeight;
			rotateParams.halfWidth = (double)(_srcWidth / 2);
			rotateParams.halfHeight = (double)(_srcHeight / 2);
			rotateParams.isWater = true;
			for (int i = num7; i < num9; i++)
			{
				int num10 = (_y + i) * _destWidth;
				double y = (double)i / _scale;
				for (int j = num4; j < num6; j++)
				{
					int num11 = _x + j + num10;
					if (_dest[num11] <= 0f)
					{
						float num12 = (float)StampManager.CalcRotatedValue(rotateParams, (double)j / _scale, y, _src);
						if (num12 >= 1E-05f)
						{
							if (_isCustomColor)
							{
								_dest[num11] = _customColor.b;
							}
							else
							{
								_dest[num11] = num12;
							}
						}
					}
				}
			}
		}

		public static void DrawBiomeStamp(Color32[] _dest, float[] _src, int _x, int _y, int _destWidth, int _destHeight, int _srcWidth, int _srcHeight, float _scale, Color32 _destColor, float _biomeCutoff = 0.1f, float _angle = 0f)
		{
			_x -= (int)((float)_srcWidth * _scale) / 2;
			_y -= (int)((float)_srcHeight * _scale) / 2;
			double num = (double)(0.0174532924f * _angle);
			double sine = Math.Sin(num);
			double cosine = Math.Cos(num);
			int num2 = Mathf.FloorToInt((float)(((int)Mathf.Sqrt((float)(_srcWidth * _srcWidth + _srcHeight * _srcHeight)) - _srcWidth) / 2) * _scale);
			int num3 = Mathf.FloorToInt((float)_srcWidth * _scale + (float)num2);
			num2 = -num2;
			int num4 = num2;
			int num5 = _x + num2;
			if (num5 < 0)
			{
				num4 -= num5;
			}
			int num6 = num3;
			num5 = _x + num3;
			if (num5 >= _destWidth)
			{
				num6 -= num5 - _destWidth;
			}
			int num7 = num2;
			int num8 = _y + num2;
			if (num8 < 0)
			{
				num7 -= num8;
			}
			int num9 = num3;
			num8 = _y + num3;
			if (num8 >= _destHeight)
			{
				num9 -= num8 - _destHeight;
			}
			for (int i = num7; i < num9; i++)
			{
				int num10 = (_y + i) * _destWidth;
				float y = (float)i / _scale;
				for (int j = num4; j < num6; j++)
				{
					int num11 = _x + j + num10;
					if (StampManager.CalcRotatedValue((float)j / _scale, y, _src, sine, cosine, _srcWidth, _srcHeight, false) > _biomeCutoff)
					{
						_dest[num11] = _destColor;
					}
				}
			}
		}

		public bool TryGetStamp(string terrainTypeName, string comboTypeName, out RawStamp tmp)
		{
			return this.TryGetStamp(comboTypeName, out tmp) || this.TryGetStamp(terrainTypeName, out tmp);
		}

		public bool TryGetStamp(string _baseName, out RawStamp _output)
		{
			return this.TryGetStamp(_baseName, out _output, Rand.Instance);
		}

		public bool TryGetStamp(string _baseName, out RawStamp _output, Rand _rand)
		{
			List<RawStamp> obj = this.tempGetStampList;
			bool result;
			lock (obj)
			{
				foreach (KeyValuePair<string, RawStamp> keyValuePair in this.AllStamps)
				{
					if (keyValuePair.Key.StartsWith(_baseName))
					{
						this.tempGetStampList.Add(keyValuePair.Value);
					}
				}
				if (this.tempGetStampList.Count == 0)
				{
					_output = null;
					result = false;
				}
				else
				{
					_output = this.tempGetStampList[_rand.Range(0, this.tempGetStampList.Count)];
					this.tempGetStampList.Clear();
					result = true;
				}
			}
			return result;
		}

		public RawStamp GetStamp(string _baseName, Rand _rand = null)
		{
			if (_rand == null)
			{
				_rand = Rand.Instance;
			}
			RawStamp result;
			if (!this.TryGetStamp(_baseName, out result, _rand))
			{
				return null;
			}
			return result;
		}

		public IEnumerator LoadStamps()
		{
			if (this.AllStamps.Count > 0)
			{
				yield break;
			}
			MicroStopwatch ms = new MicroStopwatch(true);
			this.AllStamps.Clear();
			List<PathAbstractions.AbstractedLocation> stampMaps = PathAbstractions.RwgStampsSearchPaths.GetAvailablePathsList(null, null, null, true);
			MicroStopwatch msRestart = new MicroStopwatch(true);
			int num;
			for (int i = 0; i < stampMaps.Count; i = num + 1)
			{
				string extension = stampMaps[i].Extension;
				if (!(extension != ".exr") || !(extension != ".raw"))
				{
					this.LoadStamp(stampMaps[i]);
					if (msRestart.ElapsedMilliseconds > 500L)
					{
						yield return null;
						msRestart.ResetAndRestart();
					}
				}
				num = i;
			}
			for (int i = 0; i < stampMaps.Count; i = num + 1)
			{
				this.LoadStamp(stampMaps[i]);
				if (msRestart.ElapsedMilliseconds > 500L)
				{
					yield return null;
					msRestart.ResetAndRestart();
				}
				num = i;
			}
			Log.Out("LoadStamps in {0}", new object[]
			{
				(float)ms.ElapsedMilliseconds * 0.001f
			});
			yield break;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void LoadStamp(PathAbstractions.AbstractedLocation _path)
		{
			string fileNameNoExtension = _path.FileNameNoExtension;
			if (this.AllStamps.ContainsKey(fileNameNoExtension))
			{
				return;
			}
			MicroStopwatch microStopwatch = new MicroStopwatch(true);
			Color[] array;
			if (_path.Extension == ".raw")
			{
				array = Utils.LoadRawStampFileArray(_path.FullPath);
			}
			else
			{
				if (_path.Extension == ".exr")
				{
					return;
				}
				Texture2D texture2D = TextureUtils.LoadTexture(_path.FullPath, FilterMode.Point, false, false, null);
				array = texture2D.GetPixels();
				if (Application.isPlaying)
				{
					UnityEngine.Object.Destroy(texture2D);
				}
			}
			if (array == null)
			{
				Log.Error("LoadStamp {0} failed", new object[]
				{
					_path.FullPath
				});
				return;
			}
			float[] array2 = null;
			float r = array[0].r;
			for (int i = 1; i < array.Length; i++)
			{
				if (r != array[i].r)
				{
					array2 = new float[array.Length];
					for (int j = 0; j < array.Length; j++)
					{
						array2[j] = array[j].r;
					}
					break;
				}
			}
			float[] array3 = null;
			float a = array[0].a;
			for (int k = 1; k < array.Length; k++)
			{
				if (a != array[k].a)
				{
					array3 = new float[array.Length];
					for (int l = 0; l < array.Length; l++)
					{
						array3[l] = array[l].a;
					}
					break;
				}
			}
			float[] array4 = null;
			if (!fileNameNoExtension.Contains("rwg_tile"))
			{
				for (int m = 0; m < array.Length; m++)
				{
					Color color = array[m];
					if (color.b != 0f)
					{
						if (array4 == null)
						{
							array4 = new float[array.Length];
						}
						array4[m] = color.b;
					}
				}
			}
			int num = (int)Mathf.Sqrt((float)array.Length);
			RawStamp rawStamp = new RawStamp
			{
				name = fileNameNoExtension,
				heightConst = r,
				heightPixels = array2,
				alphaConst = a,
				alphaPixels = array3,
				waterPixels = array4,
				height = num,
				width = num
			};
			this.AllStamps[fileNameNoExtension] = rawStamp;
			if (fileNameNoExtension.StartsWith("mountains_"))
			{
				rawStamp.SmoothAlpha(5);
			}
			if (fileNameNoExtension.StartsWith("desert_mountains_"))
			{
				rawStamp.SmoothAlpha(3);
			}
			Log.Out("LoadStamp {0}, size {1}, height {2} ({3}), alpha {4} ({5}), water {6}, in {7}", new object[]
			{
				_path.FullPath,
				num,
				array2 != null,
				r,
				array3 != null,
				a,
				array4 != null,
				(float)microStopwatch.ElapsedMilliseconds * 0.001f
			});
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static void SaveEXR(string cleanname, Color[] colors, bool _replace = true)
		{
			string text = GameIO.GetGameDir("Data/Stamps") + "/" + cleanname + ".exr";
			if (!_replace && File.Exists(text))
			{
				return;
			}
			int num = (int)Mathf.Sqrt((float)colors.Length);
			Texture2D texture2D = new Texture2D(num, num, TextureFormat.RGBAFloat, false);
			texture2D.SetPixels(colors);
			File.WriteAllBytes(text, texture2D.EncodeToEXR(Texture2D.EXRFlags.None));
			Log.Out("SaveEXR {0}", new object[]
			{
				text
			});
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public const float cAlphaCutoff = 1E-05f;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly WorldBuilder worldBuilder;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly Dictionary<string, RawStamp> AllStamps = new Dictionary<string, RawStamp>();

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly List<RawStamp> tempGetStampList = new List<RawStamp>();

		[PublicizedFrom(EAccessModifier.Private)]
		public class RotateParams
		{
			public double sine;

			public double cosine;

			public int width;

			public int height;

			public double halfWidth;

			public double halfHeight;

			public bool isWater;
		}
	}
}
