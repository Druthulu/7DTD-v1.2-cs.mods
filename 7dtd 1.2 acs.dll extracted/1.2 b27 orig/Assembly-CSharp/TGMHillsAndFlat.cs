using System;
using LibNoise;
using LibNoise.Modifiers;

public class TGMHillsAndFlat : TGMAbstract
{
	public TGMHillsAndFlat()
	{
		int num = 0;
		IModule source = new ScaleBiasOutput(new FastBillow
		{
			Frequency = 4.0
		})
		{
			Scale = 0.4,
			Bias = 1.0
		};
		IModule source2 = new ScaleBiasOutput(new FastTurbulence(new ScaleBiasOutput(new FastRidgedMultifractal(num)
		{
			Frequency = 5.0
		})
		{
			Scale = 1.2,
			Bias = 4.0
		})
		{
			Power = 0.45,
			Frequency = 3.0,
			Roughness = 3
		})
		{
			Scale = 0.800000011920929,
			Bias = 9.0
		};
		Select select = new Select(new FastNoise(num + 1)
		{
			Frequency = 3.0
		}, source, source2);
		select.SetBounds(0.0, 0.5);
		select.EdgeFalloff = 0.5;
		this.outputModule = select;
		this.IsSeedSet = true;
	}

	public override void SetSeed(int _seed)
	{
	}

	public override float GetValue(float _x, float _z, float _biomeIntens)
	{
		return 1f * _biomeIntens + (float)this.outputModule.GetValue((double)_x, 0.0, (double)_z) * _biomeIntens;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IModule outputModule;
}
