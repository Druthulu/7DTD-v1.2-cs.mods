using System;

public class AtmosphereEffect
{
	public static AtmosphereEffect Load(string _folder, AtmosphereEffect _default)
	{
		AtmosphereEffect atmosphereEffect = new AtmosphereEffect();
		string str = "Textures/Environment/Spectrums/" + ((_folder != null) ? (_folder + "/") : "");
		atmosphereEffect.spectrums[0] = ColorSpectrum.FromResource(str + "sky");
		atmosphereEffect.spectrums[1] = ColorSpectrum.FromResource(str + "ambient");
		atmosphereEffect.spectrums[2] = ColorSpectrum.FromResource(str + "sun");
		atmosphereEffect.spectrums[3] = ColorSpectrum.FromResource(str + "moon");
		atmosphereEffect.spectrums[4] = ColorSpectrum.FromResource(str + "fog");
		atmosphereEffect.spectrums[5] = ColorSpectrum.FromResource(str + "fogfade");
		if (_default != null)
		{
			for (int i = 0; i < atmosphereEffect.spectrums.Length; i++)
			{
				if (atmosphereEffect.spectrums[i] == null)
				{
					atmosphereEffect.spectrums[i] = _default.spectrums[i];
				}
			}
		}
		return atmosphereEffect;
	}

	public ColorSpectrum[] spectrums = new ColorSpectrum[6];

	public enum ESpecIdx
	{
		Sky,
		Ambient,
		Sun,
		Moon,
		Fog,
		FogFade,
		Count
	}
}
