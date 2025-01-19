using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

public struct ExplosionData
{
	public ExplosionData(byte[] _explosionDataAsArr)
	{
		this.ParticleIndex = 0;
		this.Duration = 0f;
		this.BlockRadius = 0f;
		this.EntityRadius = 0;
		this.EntityDamage = 0f;
		this.BlockDamage = 0f;
		this.BlockTags = string.Empty;
		this.BlastPower = 100;
		this.damageMultiplier = null;
		this.BuffActions = null;
		this.IgnoreHeatMap = false;
		this.DamageType = EnumDamageTypes.Heat;
		using (PooledBinaryReader pooledBinaryReader = MemoryPools.poolBinaryReader.AllocSync(false))
		{
			pooledBinaryReader.SetBaseStream(new MemoryStream(_explosionDataAsArr));
			this.Read(pooledBinaryReader);
		}
	}

	public ExplosionData(DynamicProperties _properties, MinEffectController _effects = null)
	{
		this.ParticleIndex = 0;
		_properties.ParseInt("Explosion.ParticleIndex", ref this.ParticleIndex);
		this.Duration = 0f;
		_properties.ParseFloat("Explosion.Duration", ref this.Duration);
		this.BlockRadius = 1f;
		_properties.ParseFloat("Explosion.RadiusBlocks", ref this.BlockRadius);
		if (_properties.Values.ContainsKey("Explosion.BlockDamage"))
		{
			this.BlockDamage = StringParsers.ParseFloat(_properties.Values["Explosion.BlockDamage"], 0, -1, NumberStyles.Any);
		}
		else
		{
			this.BlockDamage = this.BlockRadius * this.BlockRadius;
		}
		this.BlockTags = string.Empty;
		_properties.ParseString("Explosion.BlockTags", ref this.BlockTags);
		this.EntityRadius = 0;
		if (_properties.Values.ContainsKey("Explosion.RadiusEntities"))
		{
			this.EntityRadius = (int)StringParsers.ParseFloat(_properties.Values["Explosion.RadiusEntities"], 0, -1, NumberStyles.Any);
		}
		if (_properties.Values.ContainsKey("Explosion.EntityDamage"))
		{
			this.EntityDamage = StringParsers.ParseFloat(_properties.Values["Explosion.EntityDamage"], 0, -1, NumberStyles.Any);
		}
		else
		{
			this.EntityDamage = 20f * (float)this.EntityRadius;
		}
		this.BlastPower = 100;
		if (_properties.Values.ContainsKey("Explosion.BlastPower"))
		{
			this.BlastPower = (int)StringParsers.ParseFloat(_properties.Values["Explosion.BlastPower"], 0, -1, NumberStyles.Any);
		}
		this.BuffActions = null;
		_properties.ParseFloat("Explosion.RadiusBlocks", ref this.BlockRadius);
		if (_properties.Values.ContainsKey("Explosion.Buff"))
		{
			string[] array = _properties.Values["Explosion.Buff"].Split(',', StringSplitOptions.None);
			for (int i = 0; i < array.Length; i++)
			{
				if (this.BuffActions == null)
				{
					this.BuffActions = new List<string>();
				}
				this.BuffActions.Add(array[i]);
			}
		}
		this.damageMultiplier = new DamageMultiplier(_properties, "Explosion.");
		this.IgnoreHeatMap = false;
		_properties.ParseBool("Explosion.IgnoreHeatMap", ref this.IgnoreHeatMap);
		this.DamageType = EnumDamageTypes.Heat;
		_properties.ParseEnum<EnumDamageTypes>("Explosion.DamageType", ref this.DamageType);
		if (_effects != null)
		{
			MinEffectGroup minEffectGroup = new MinEffectGroup
			{
				OwnerTiered = false
			};
			if (!_effects.PassivesIndex.Contains(PassiveEffects.ExplosionBlockDamage))
			{
				PassiveEffect pe = PassiveEffect.CreateEmptyPassiveEffect(PassiveEffects.ExplosionBlockDamage);
				MinEffectGroup.AddPassiveEffectToGroup(minEffectGroup, pe);
			}
			if (!_effects.PassivesIndex.Contains(PassiveEffects.ExplosionEntityDamage))
			{
				PassiveEffect pe2 = PassiveEffect.CreateEmptyPassiveEffect(PassiveEffects.ExplosionEntityDamage);
				MinEffectGroup.AddPassiveEffectToGroup(minEffectGroup, pe2);
			}
			if (minEffectGroup.PassiveEffects.Count > 0)
			{
				_effects.AddEffectGroup(minEffectGroup, 0, false);
			}
		}
	}

	public byte[] ToByteArray()
	{
		MemoryStream memoryStream = new MemoryStream();
		using (PooledBinaryWriter pooledBinaryWriter = MemoryPools.poolBinaryWriter.AllocSync(false))
		{
			pooledBinaryWriter.SetBaseStream(memoryStream);
			this.Write(pooledBinaryWriter);
		}
		return memoryStream.ToArray();
	}

	public void Read(BinaryReader _br)
	{
		this.ParticleIndex = (int)_br.ReadInt16();
		this.Duration = (float)_br.ReadInt16() * 0.1f;
		this.BlockRadius = (float)_br.ReadInt16() * 0.05f;
		this.EntityRadius = (int)_br.ReadInt16();
		this.BlastPower = (int)_br.ReadInt16();
		this.BlockDamage = _br.ReadSingle();
		this.EntityDamage = _br.ReadSingle();
		this.BlockTags = _br.ReadString();
		this.IgnoreHeatMap = _br.ReadBoolean();
		this.DamageType = (EnumDamageTypes)_br.ReadInt16();
		this.damageMultiplier = new DamageMultiplier();
		this.damageMultiplier.Read(_br);
		int num = (int)_br.ReadByte();
		if (num > 0)
		{
			this.BuffActions = new List<string>();
			for (int i = 0; i < num; i++)
			{
				this.BuffActions.Add(_br.ReadString());
			}
			return;
		}
		this.BuffActions = null;
	}

	public void Write(BinaryWriter _bw)
	{
		_bw.Write((short)this.ParticleIndex);
		_bw.Write((short)(this.Duration * 10f));
		_bw.Write((short)(this.BlockRadius * 20f));
		_bw.Write((short)this.EntityRadius);
		_bw.Write((short)this.BlastPower);
		_bw.Write(this.BlockDamage);
		_bw.Write(this.EntityDamage);
		_bw.Write(this.BlockTags);
		_bw.Write(this.IgnoreHeatMap);
		_bw.Write((short)this.DamageType);
		this.damageMultiplier.Write(_bw);
		if (this.BuffActions != null)
		{
			_bw.Write((byte)this.BuffActions.Count);
			for (int i = 0; i < this.BuffActions.Count; i++)
			{
				_bw.Write(this.BuffActions[i]);
			}
			return;
		}
		_bw.Write(0);
	}

	public const int cMaxBlastPower = 100;

	public int ParticleIndex;

	public float Duration;

	public float BlockRadius;

	public int EntityRadius;

	public int BlastPower;

	public float EntityDamage;

	public float BlockDamage;

	public string BlockTags;

	public bool IgnoreHeatMap;

	public EnumDamageTypes DamageType;

	public DamageMultiplier damageMultiplier;

	public List<string> BuffActions;

	[PublicizedFrom(EAccessModifier.Private)]
	public const string PREFIX = "Explosion.";
}
