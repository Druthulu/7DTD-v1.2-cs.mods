using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageWeather : NetPackage
{
	public override NetPackageDirection PackageDirection
	{
		get
		{
			return NetPackageDirection.ToClient;
		}
	}

	public NetPackageWeather Setup(WeatherPackage[] _packages, ulong _lightningTime, Vector3 _lightningPos)
	{
		this.weatherPackages = _packages;
		this.lightningTime = _lightningTime;
		this.lightningPos = _lightningPos;
		return this;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void InitPackages()
	{
		if (GameManager.Instance != null && GameManager.Instance.World != null && GameManager.Instance.World.Biomes != null)
		{
			Dictionary<uint, BiomeDefinition> biomeMap = GameManager.Instance.World.Biomes.GetBiomeMap();
			if (biomeMap != null)
			{
				this.weatherPackages = new WeatherPackage[biomeMap.Count];
				for (int i = 0; i < biomeMap.Count; i++)
				{
					this.weatherPackages[i] = new WeatherPackage();
				}
			}
		}
	}

	public override void read(PooledBinaryReader _br)
	{
		if (this.weatherPackages == null)
		{
			this.InitPackages();
			if (this.weatherPackages == null)
			{
				return;
			}
		}
		for (int i = 0; i < this.weatherPackages.Length; i++)
		{
			WeatherPackage weatherPackage = this.weatherPackages[i];
			for (int j = 0; j < weatherPackage.param.Length; j++)
			{
				weatherPackage.param[j] = _br.ReadSingle();
			}
			weatherPackage.particleRain = _br.ReadSingle();
			weatherPackage.particleSnow = _br.ReadSingle();
			weatherPackage.surfaceWet = _br.ReadSingle();
			weatherPackage.surfaceSnow = _br.ReadSingle();
			weatherPackage.biomeID = _br.ReadByte();
			weatherPackage.weatherSpectrum = _br.ReadInt16();
		}
		this.lightningTime = _br.ReadUInt64();
		this.lightningPos.x = _br.ReadSingle();
		this.lightningPos.y = _br.ReadSingle();
		this.lightningPos.z = _br.ReadSingle();
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		for (int i = 0; i < this.weatherPackages.Length; i++)
		{
			WeatherPackage weatherPackage = this.weatherPackages[i];
			for (int j = 0; j < weatherPackage.param.Length; j++)
			{
				_bw.Write(weatherPackage.param[j]);
			}
			_bw.Write(weatherPackage.particleRain);
			_bw.Write(weatherPackage.particleSnow);
			_bw.Write(weatherPackage.surfaceWet);
			_bw.Write(weatherPackage.surfaceSnow);
			_bw.Write(weatherPackage.biomeID);
			_bw.Write(weatherPackage.weatherSpectrum);
		}
		_bw.Write(this.lightningTime);
		_bw.Write(this.lightningPos.x);
		_bw.Write(this.lightningPos.y);
		_bw.Write(this.lightningPos.z);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null)
		{
			return;
		}
		if (WeatherManager.Instance)
		{
			if (this.lightningTime > 0UL)
			{
				WeatherManager.Instance.TriggerThunder(this.lightningTime, this.lightningPos);
			}
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsClient && this.weatherPackages != null)
			{
				WeatherManager.Instance.ClientProcessPackages(this.weatherPackages);
			}
		}
	}

	public override int GetLength()
	{
		return 0;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public WeatherPackage[] weatherPackages;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 lightningPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public ulong lightningTime;
}
