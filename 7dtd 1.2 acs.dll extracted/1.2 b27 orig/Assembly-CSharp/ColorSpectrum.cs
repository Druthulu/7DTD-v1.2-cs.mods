using System;
using Unity.Collections;
using UnityEngine;

public class ColorSpectrum
{
	public static ColorSpectrum FromResource(string _filename)
	{
		Texture2D texture2D = Resources.Load(_filename) as Texture2D;
		if (texture2D == null)
		{
			return null;
		}
		ColorSpectrum result = new ColorSpectrum(_filename, texture2D);
		Resources.UnloadAsset(texture2D);
		return result;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool IsSupportedRawTextureFormat(TextureFormat _format)
	{
		return _format - TextureFormat.RGB24 <= 2 || _format == TextureFormat.BGRA32;
	}

	public ColorSpectrum(string name, Texture2D _tex)
	{
		int width = _tex.width;
		this.values = new Color[width];
		if (!this.IsSupportedRawTextureFormat(_tex.format))
		{
			Log.Warning("Color Spectrum texture " + name + " is not in a format supported for non-allocating GetRawTextureData access. Falling back to GetPixels.");
			Color[] pixels = _tex.GetPixels();
			for (int i = 0; i < width; i++)
			{
				this.values[i] = pixels[i].linear;
			}
			return;
		}
		TextureFormat format = _tex.format;
		switch (format)
		{
		case TextureFormat.RGB24:
		{
			NativeArray<TextureUtils.ColorRGB24> rawTextureData = _tex.GetRawTextureData<TextureUtils.ColorRGB24>();
			for (int j = 0; j < width; j++)
			{
				this.values[j] = TextureUtils.GetLinearColor(rawTextureData[j]);
			}
			return;
		}
		case TextureFormat.RGBA32:
		{
			NativeArray<Color32> rawTextureData2 = _tex.GetRawTextureData<Color32>();
			for (int k = 0; k < width; k++)
			{
				this.values[k] = TextureUtils.GetLinearColor(rawTextureData2[k]);
			}
			return;
		}
		case TextureFormat.ARGB32:
		{
			NativeArray<TextureUtils.ColorARGB32> rawTextureData3 = _tex.GetRawTextureData<TextureUtils.ColorARGB32>();
			for (int l = 0; l < width; l++)
			{
				this.values[l] = TextureUtils.GetLinearColor(rawTextureData3[l]);
			}
			return;
		}
		default:
		{
			if (format != TextureFormat.BGRA32)
			{
				return;
			}
			NativeArray<TextureUtils.ColorBGRA32> rawTextureData4 = _tex.GetRawTextureData<TextureUtils.ColorBGRA32>();
			for (int m = 0; m < width; m++)
			{
				this.values[m] = TextureUtils.GetLinearColor(rawTextureData4[m]);
			}
			return;
		}
		}
	}

	public Color GetValue(float _v)
	{
		int num = this.values.Length;
		float num2 = (float)num * _v;
		int num3 = (int)num2;
		Color a = this.values[num3];
		Color b = this.values[(num3 + 1) % num];
		return Color.LerpUnclamped(a, b, num2 - (float)num3);
	}

	public static bool Exists(string _filename)
	{
		return Resources.Load(_filename) != null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Color[] values;
}
