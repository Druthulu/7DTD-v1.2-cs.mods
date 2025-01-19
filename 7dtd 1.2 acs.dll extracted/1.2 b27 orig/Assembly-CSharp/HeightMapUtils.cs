using System;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

public static class HeightMapUtils
{
	public static float[,] ConvertDTMToHeightData(string levelName)
	{
		Texture2D texture2D = Resources.Load("Data/Worlds/" + levelName + "/dtm", typeof(Texture2D)) as Texture2D;
		if (texture2D == null)
		{
			return HeightMapUtils.ConvertDTMToHeightDataExternal(levelName, true);
		}
		float[,] result = HeightMapUtils.ConvertDTMToHeightData(texture2D, false);
		Resources.UnloadAsset(texture2D);
		return result;
	}

	public static float[,] ConvertDTMToHeightDataExternal(string levelName, bool loadPNG = true)
	{
		PathAbstractions.AbstractedLocation location = PathAbstractions.WorldsSearchPaths.GetLocation(levelName, null, null);
		if (location.Type == PathAbstractions.EAbstractedLocationType.None)
		{
			throw new FileNotFoundException();
		}
		string str = location.FullPath + "/dtm";
		Texture2D texture2D;
		if (loadPNG && SdFile.Exists(str + ".png"))
		{
			texture2D = TextureUtils.LoadTexture(str + ".png", FilterMode.Point, false, false, null);
		}
		else
		{
			if (!SdFile.Exists(str + ".tga"))
			{
				throw new FileNotFoundException();
			}
			texture2D = TextureUtils.LoadTexture(str + ".tga", FilterMode.Point, false, false, null);
		}
		float[,] result = HeightMapUtils.ConvertDTMToHeightData(texture2D, false);
		UnityEngine.Object.Destroy(texture2D);
		return result;
	}

	public static float[,] ConvertDTMToHeightData(Color[] dtmPixs, int dtmSize, bool _bFlip = false)
	{
		float[,] array = new float[dtmSize, dtmSize];
		if (!_bFlip)
		{
			for (int i = 0; i < dtmSize; i++)
			{
				for (int j = 0; j < dtmSize; j++)
				{
					array[i, j] = dtmPixs[i + j * dtmSize].r * 255f;
				}
			}
		}
		else
		{
			for (int k = 0; k < dtmSize; k++)
			{
				for (int l = 0; l < dtmSize; l++)
				{
					array[k, dtmSize - l - 1] = dtmPixs[k + l * dtmSize].r * 255f;
				}
			}
		}
		return array;
	}

	public static float[,] ConvertDTMToHeightData(Texture2D dtm, bool _bFlip = false)
	{
		Color[] pixels = dtm.GetPixels();
		float[,] array = new float[dtm.width, dtm.height];
		if (!_bFlip)
		{
			for (int i = 0; i < dtm.width; i++)
			{
				for (int j = 0; j < dtm.height; j++)
				{
					array[i, j] = pixels[i + j * dtm.width].r * 255f;
				}
			}
		}
		else
		{
			int height = dtm.height;
			int width = dtm.width;
			for (int k = 0; k < width; k++)
			{
				for (int l = 0; l < height; l++)
				{
					array[k, height - l - 1] = pixels[k + l * dtm.width].r * 255f;
				}
			}
		}
		return array;
	}

	public static float[,] ConvertDTMToTerrainStampData(Texture2D dtm)
	{
		Color[] pixels = dtm.GetPixels();
		float[,] array = new float[dtm.width, dtm.height];
		for (int i = 0; i < dtm.width; i++)
		{
			for (int j = 0; j < dtm.height; j++)
			{
				array[i, j] = pixels[i + j * dtm.width].r;
			}
		}
		return array;
	}

	public static float[,] ConvertDTMToHeightData(Color32[] dtmPixs, int w, int h, bool _bFlip = false)
	{
		float[,] array = new float[w, h];
		if (!_bFlip)
		{
			for (int i = 0; i < w; i++)
			{
				for (int j = 0; j < h; j++)
				{
					array[i, j] = (float)dtmPixs[i + j * w].r;
					if (dtmPixs[i + j * w].r != dtmPixs[i + j * w].g)
					{
						array[i, j] += (float)dtmPixs[i + j * w].g / 255f;
					}
				}
			}
		}
		else
		{
			for (int k = 0; k < w; k++)
			{
				for (int l = 0; l < h; l++)
				{
					array[k, h - l - 1] = (float)dtmPixs[k + l * w].r;
					if (dtmPixs[k + l * w].r != dtmPixs[k + l * w].g)
					{
						array[k, h - l - 1] += (float)dtmPixs[k + l * w].g / 255f;
					}
				}
			}
		}
		return array;
	}

	public static float[,] LoadRAWToHeightData(string _filePath)
	{
		ushort[] array = HeightMapUtils.LoadHeightMapRAW(_filePath);
		int num = (int)Mathf.Sqrt((float)array.Length);
		int num2 = num;
		float[,] array2 = new float[num, num2];
		for (int i = num - 1; i >= 0; i--)
		{
			for (int j = num2 - 1; j >= 0; j--)
			{
				ushort num3 = array[i + j * num];
				array2[i, j] = (float)num3 / 65280f * 255f;
			}
		}
		return array2;
	}

	public static ushort[] LoadHeightMapRAW(string _filePath)
	{
		ushort[] result;
		using (BufferedStream bufferedStream = new BufferedStream(SdFile.OpenRead(_filePath)))
		{
			int num = (int)Mathf.Sqrt((float)bufferedStream.Length);
			byte[] array = new byte[8192];
			ushort[] array2 = new ushort[bufferedStream.Length / 2L];
			int num2 = 0;
			int num3 = 0;
			int num4 = 0;
			while ((long)num4 < bufferedStream.Length)
			{
				int num5 = bufferedStream.Read(array, 0, array.Length);
				num4 += num5;
				int i = 0;
				int num6 = num2 + num3 * num;
				while (i < num5)
				{
					byte b = array[i];
					byte b2 = array[i + 1];
					array2[num6++] = (ushort)((int)b2 * 256 + (int)b);
					num2++;
					if (num2 >= num)
					{
						num2 = 0;
						num3++;
						num6 = num2 + num3 * num;
					}
					i += 2;
				}
			}
			result = array2;
		}
		return result;
	}

	public unsafe static IBackedArray<ushort> LoadHeightMapRAW(string _filePath, int w, int h, float _fac = 1f, int _clampHeight = 1)
	{
		_clampHeight *= 256;
		if (_clampHeight <= 0)
		{
			_clampHeight = int.MaxValue;
		}
		ushort num = (ushort)_clampHeight;
		IBackedArray<ushort> result;
		using (Stream stream = SdFile.OpenRead(_filePath))
		{
			int num2 = (int)stream.Length / 2 * 2;
			IBackedArray<ushort> backedArray = BackedArrays.Create<ushort>(w * h);
			int i = 0;
			while (i < num2)
			{
				int start = i / 2;
				int num3 = Math.Min(num2 - i, 1048576);
				int length = num3 / 2;
				Span<ushort> span2;
				using (backedArray.GetSpan(start, length, out span2))
				{
					Span<byte> span3 = MemoryMarshal.Cast<ushort, byte>(span2);
					int j;
					int num4;
					for (j = 0; j < num3; j += num4)
					{
						num4 = stream.Read(span3.Slice(j, num3 - j));
						if (num4 <= 0)
						{
							throw new IOException(string.Format("Unexpected end of stream. Expected {0} bytes but only read {1} bytes.", num2, i));
						}
					}
					int num5 = j / 2;
					for (int k = 0; k < num5; k++)
					{
						if ((int)(*span2[k]) > _clampHeight)
						{
							*span2[k] = num;
						}
					}
					i += j;
				}
			}
			result = backedArray;
		}
		return result;
	}

	public static float[,] LoadHeightMapRAWAsUnityHeightMap(string _filePath, int w, int h, float _fac = 1f)
	{
		float[,] result;
		using (BufferedStream bufferedStream = new BufferedStream(SdFile.OpenRead(_filePath)))
		{
			byte[] array = new byte[8192];
			float[,] array2 = new float[h, w];
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			while ((long)num3 < bufferedStream.Length)
			{
				int num4 = bufferedStream.Read(array, 0, array.Length);
				num3 += num4;
				for (int i = 0; i < num4; i += 2)
				{
					byte b = array[i];
					byte b2 = array[i + 1];
					array2[num2, num] = (float)((int)b2 * 256 + (int)b) / 65535f;
					num++;
					if (num >= w)
					{
						num = 0;
						num2++;
					}
				}
			}
			result = array2;
		}
		return result;
	}

	public static float[,] LoadHeightMapRAWAsStampData(string _filePath, float multiplier = 1f)
	{
		float[,] result;
		using (BufferedStream bufferedStream = new BufferedStream(SdFile.OpenRead(_filePath)))
		{
			byte[] array = new byte[8192];
			int num = (int)Mathf.Sqrt((float)(bufferedStream.Length / 2L));
			float[,] array2 = new float[num, num];
			int num2 = 0;
			int num3 = 0;
			int num4 = 0;
			while ((long)num4 < bufferedStream.Length)
			{
				int num5 = bufferedStream.Read(array, 0, array.Length);
				num4 += num5;
				for (int i = 0; i < num5; i += 2)
				{
					byte b = array[i];
					byte b2 = array[i + 1];
					array2[num3, num2] = (float)((int)b2 * 256 + (int)b) / 65535f * multiplier;
					num2++;
					if (num2 >= num)
					{
						num2 = 0;
						num3++;
					}
				}
			}
			result = array2;
		}
		return result;
	}

	public static void SaveHeightMapRAW(string _filePath, int w, int h, float[] _data)
	{
		using (BufferedStream bufferedStream = new BufferedStream(SdFile.OpenWrite(_filePath)))
		{
			int num = w * 2;
			byte[] array = new byte[num];
			float num2 = 256f;
			for (int i = 0; i < h; i++)
			{
				int num3 = i * w;
				for (int j = 0; j < w; j++)
				{
					uint num4 = (uint)(_data[num3 + j] * num2);
					array[2 * j] = (byte)(num4 & 255U);
					array[2 * j + 1] = (byte)(num4 / 256U);
				}
				bufferedStream.Write(array, 0, num);
			}
		}
	}

	public static void SaveHeightMapRAW(string _filePath, int w, int h, IBackedArray<ushort> _data)
	{
		using (Stream stream = SdFile.OpenWrite(_filePath))
		{
			int num = w * h;
			int i = 0;
			while (i < num)
			{
				int num2 = Math.Min(524288, num - i);
				ReadOnlySpan<ushort> span;
				using (_data.GetReadOnlySpan(i, num2, out span))
				{
					ReadOnlySpan<byte> buffer = MemoryMarshal.Cast<ushort, byte>(span);
					stream.Write(buffer);
					i += num2;
				}
			}
		}
	}

	public static void SaveHeightMapRAW(string _filePath, int w, int h, float[,] _data)
	{
		using (BufferedStream bufferedStream = new BufferedStream(SdFile.OpenWrite(_filePath)))
		{
			int num = w * 2;
			byte[] array = new byte[num];
			float num2 = 256f;
			for (int i = 0; i < h; i++)
			{
				for (int j = 0; j < w; j++)
				{
					uint num3 = (uint)(_data[j, i] * num2);
					array[2 * j] = (byte)(num3 & 255U);
					array[2 * j + 1] = (byte)(num3 / 256U);
				}
				bufferedStream.Write(array, 0, num);
			}
		}
	}

	public static void SaveHeightMapRAW(Stream _stream, float[] _data, int _mask = 65535)
	{
		for (int i = 0; i < _data.Length; i++)
		{
			int num = (int)(_data[i] * 256f) & _mask;
			_stream.WriteByte((byte)num);
			_stream.WriteByte((byte)(num >> 8));
		}
	}

	public static float[,] SmoothTerrain(int Passes, float[,] HeightData)
	{
		int length = HeightData.GetLength(0);
		int length2 = HeightData.GetLength(1);
		float[,] array = new float[length, length2];
		while (Passes > 0)
		{
			Passes--;
			for (int i = 0; i < length; i++)
			{
				for (int j = 0; j < length2; j++)
				{
					int num = 0;
					float num2 = 0f;
					if (i - 1 > 0)
					{
						num2 += HeightData[i - 1, j];
						num++;
						if (j - 1 > 0)
						{
							num2 += HeightData[i - 1, j - 1];
							num++;
						}
						if (j + 1 < length2)
						{
							num2 += HeightData[i - 1, j + 1];
							num++;
						}
					}
					if (i + 1 < length)
					{
						num2 += HeightData[i + 1, j];
						num++;
						if (j - 1 > 0)
						{
							num2 += HeightData[i + 1, j - 1];
							num++;
						}
						if (j + 1 < length2)
						{
							num2 += HeightData[i + 1, j + 1];
							num++;
						}
					}
					if (j - 1 > 0)
					{
						num2 += HeightData[i, j - 1];
						num++;
					}
					if (j + 1 < length2)
					{
						num2 += HeightData[i, j + 1];
						num++;
					}
					array[i, j] = (HeightData[i, j] + num2 / (float)num) * 0.5f;
				}
			}
			float[,] array2 = array;
			array = HeightData;
			HeightData = array2;
		}
		return HeightData;
	}

	public static float[,][,] ConvertAndSliceUnityHeightmap(ushort[] _rawHeightMap, int _heightMapWidth, int _heightMapHeight, int _sliceAtPix, int _resStep)
	{
		int num = _heightMapWidth / _sliceAtPix;
		int num2 = _heightMapHeight / _sliceAtPix;
		float[,][,] array = new float[num, num2][,];
		for (int i = 0; i < num2; i++)
		{
			for (int j = 0; j < num; j++)
			{
				float[,] array2 = new float[_sliceAtPix / _resStep + 1, _sliceAtPix / _resStep + 1];
				for (int k = _sliceAtPix; k >= 0; k -= _resStep)
				{
					for (int l = _sliceAtPix; l >= 0; l -= _resStep)
					{
						int num3 = j * _sliceAtPix + k;
						int num4 = i * _sliceAtPix + l;
						if (num3 >= _heightMapWidth)
						{
							num3 = _heightMapWidth - 1;
						}
						if (num4 >= _heightMapHeight)
						{
							num4 = _heightMapHeight - 1;
						}
						ushort num5 = _rawHeightMap[num3 + num4 * _heightMapWidth];
						array2[l / _resStep, k / _resStep] = (float)num5 / 65280f;
					}
				}
				array[j, i] = array2;
			}
		}
		return array;
	}

	public static float[,][,] ConvertAndSliceUnityHeightmapQuartered(IBackedArray<ushort> _rawHeightMap, int _heightMapWidth, int _heightMapHeight, int _sliceAtPix)
	{
		int num = _heightMapWidth / _sliceAtPix;
		int num2 = _heightMapHeight / _sliceAtPix;
		float[,][,] array = new float[num, num2][,];
		for (int i = 0; i < num2; i++)
		{
			for (int j = 0; j < num; j++)
			{
				array[j, i] = HeightMapUtils.ConvertUnityHeightmapSliceQuartered(_rawHeightMap, _heightMapWidth, _heightMapHeight, j, i, _sliceAtPix);
			}
		}
		return array;
	}

	public unsafe static float[,] ConvertUnityHeightmapSliceQuartered(IBackedArray<ushort> _rawHeightMap, int _heightMapWidth, int _heightMapHeight, int _sliceX, int _sliceZ, int _sliceAtPix)
	{
		float[,] array = new float[_sliceAtPix / 2 + 1, _sliceAtPix / 2 + 1];
		float[,] array2;
		float* pointer;
		if ((array2 = array) == null || array2.Length == 0)
		{
			pointer = null;
		}
		else
		{
			pointer = &array2[0, 0];
		}
		Span<float> tempHeightMapData = new Span<float>((void*)pointer, array.GetLength(0) * array.GetLength(1));
		HeightMapUtils.ConvertUnityHeightmapSliceQuartered(_rawHeightMap, _heightMapWidth, _heightMapHeight, _sliceX, _sliceZ, _sliceAtPix, tempHeightMapData);
		array2 = null;
		return array;
	}

	public static TileFile<float> ConvertAndSliceUnityHeightmapQuarteredToFile(IBackedArray<ushort> _rawHeightMap, int _heightMapWidth, int _heightMapHeight, int _sliceAtPix)
	{
		int num = _heightMapWidth / _sliceAtPix;
		int num2 = _heightMapHeight / _sliceAtPix;
		int num3 = _sliceAtPix / 2 + 1;
		FileBackedArray<float> fileBackedArray = new FileBackedArray<float>(num * num2 * num3 * num3);
		int num4 = num3 * num3;
		for (int i = 0; i < num2; i++)
		{
			for (int j = 0; j < num; j++)
			{
				int start = i * num4 * num + j * num4;
				Span<float> tempHeightMapData;
				using (fileBackedArray.GetSpan(start, num4, out tempHeightMapData))
				{
					HeightMapUtils.ConvertUnityHeightmapSliceQuartered(_rawHeightMap, _heightMapWidth, _heightMapHeight, j, i, _sliceAtPix, tempHeightMapData);
				}
			}
		}
		return new TileFile<float>(fileBackedArray, num3, num, num2);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public unsafe static void ConvertUnityHeightmapSliceQuartered(IBackedArray<ushort> _rawHeightMap, int _heightMapWidth, int _heightMapHeight, int _sliceX, int _sliceZ, int _sliceAtPix, Span<float> tempHeightMapData)
	{
		int num = _heightMapWidth - 1;
		int num2 = _heightMapHeight - 1;
		int num3 = Math.Max((_sliceZ * _sliceAtPix - 1) * _heightMapWidth, 0);
		int length = Math.Min(((_sliceZ + 1) * _sliceAtPix + 2) * _heightMapWidth, _rawHeightMap.Length) - num3;
		ReadOnlySpan<ushort> readOnlySpan2;
		using (_rawHeightMap.GetReadOnlySpan(num3, length, out readOnlySpan2))
		{
			int num4 = _sliceAtPix / 2 + 1;
			for (int i = 0; i <= _sliceAtPix; i += 2)
			{
				int num5 = _sliceZ * _sliceAtPix + i;
				if (num5 >= num2)
				{
					num5 = _heightMapHeight - 2;
				}
				else if (num5 < 1)
				{
					num5 = 1;
				}
				int num6 = num5 * _heightMapWidth;
				for (int j = 0; j <= _sliceAtPix; j += 2)
				{
					int num7 = _sliceX * _sliceAtPix + j;
					if (num7 >= num)
					{
						num7 = _heightMapWidth - 2;
					}
					else if (num7 < 1)
					{
						num7 = 1;
					}
					int num8 = num7 + num6 - num3;
					uint num9 = (uint)(*readOnlySpan2[num8 - _heightMapWidth]);
					uint num10 = (uint)(*readOnlySpan2[num8 - 1]);
					uint num11 = (uint)(*readOnlySpan2[num8]);
					uint num12 = (uint)(*readOnlySpan2[num8 + 1]);
					uint num13 = (uint)(*readOnlySpan2[num8 + _heightMapWidth]);
					ushort num14 = (ushort)((num10 + num11 + num12 + num9 + num13) / 5U);
					int num15 = i / 2;
					int num16 = j / 2;
					*tempHeightMapData[num15 * num4 + num16] = (float)num14 / 65280f;
				}
			}
		}
	}

	public static Color32[,][] ConvertAndSliceSplatmap(Color32[] _splat, int _splatMapWidth, int _splatMapHeight, int _overlapBorderPix, int _sliceAtWidth, int _resStep)
	{
		int num = _splatMapWidth / _sliceAtWidth;
		int num2 = _splatMapHeight / _sliceAtWidth;
		Color32[,][] array = new Color32[num, num2][];
		for (int i = 0; i < num2; i++)
		{
			for (int j = 0; j < num; j++)
			{
				int num3 = _sliceAtWidth / _resStep + 2 * _overlapBorderPix;
				int num4 = _sliceAtWidth / _resStep + 2 * _overlapBorderPix;
				Color32[] array2 = new Color32[num3 * num4];
				for (int k = -_resStep * _overlapBorderPix; k < _sliceAtWidth + _overlapBorderPix; k += _resStep)
				{
					for (int l = -_resStep * _overlapBorderPix; l < _sliceAtWidth + _overlapBorderPix; l += _resStep)
					{
						int num5 = j * _sliceAtWidth + k;
						int num6 = i * _sliceAtWidth + l;
						if (num5 < 0)
						{
							num5 += _splatMapHeight;
						}
						else if (num5 >= _splatMapWidth)
						{
							num5 -= _splatMapWidth;
						}
						if (num6 < 0)
						{
							num6 += _splatMapHeight;
						}
						else if (num6 >= _splatMapHeight)
						{
							num6 -= _splatMapHeight;
						}
						Color32 color = _splat[num5 + num6 * _splatMapWidth];
						array2[k / _resStep + _overlapBorderPix + (l / _resStep + _overlapBorderPix) * num3] = color;
					}
				}
				array[j, i] = array2;
			}
		}
		return array;
	}
}
