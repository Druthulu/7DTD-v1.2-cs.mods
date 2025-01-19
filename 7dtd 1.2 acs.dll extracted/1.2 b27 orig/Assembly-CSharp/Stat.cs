using System;
using System.IO;
using UnityEngine;

public sealed class Stat
{
	public Stat(EntityAlive entity, float value, float baseMax)
	{
		this.m_baseMax = baseMax;
		this.m_value = value;
		this.m_originalBaseMax = this.m_baseMax;
		this.m_originalValue = this.m_value;
		this.m_maxModifier = 0f;
		this.m_valueModifier = 0f;
		this.m_changed = false;
		this.Entity = entity;
		this.m_lastValue = this.m_value;
	}

	public float RegenerationAmount
	{
		get
		{
			return this.regenAmount;
		}
		set
		{
			this.regenAmount = value;
		}
	}

	public float RegenerationAmountUI { get; [PublicizedFrom(EAccessModifier.Private)] set; }

	public void Tick(float dt, ulong worldTime = 0UL, bool godMode = false)
	{
		if (this.MaxPassive != PassiveEffects.None)
		{
			this.BaseMax = EffectManager.GetValue(this.MaxPassive, null, this.m_originalBaseMax, this.Entity, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
		}
		this.m_isEntityAlive = (this.Entity != null);
		this.m_isEntityPlayer = (this.Entity as EntityPlayer != null);
		if ((this.StatType == Stat.StatTypes.Stamina || this.StatType == Stat.StatTypes.Health) && Mathf.Abs(this.m_lastValue - this.m_value) >= 1f)
		{
			if (this.m_value > this.m_lastValue && this.GainPassive != PassiveEffects.None)
			{
				this.m_value = Mathf.Clamp(this.m_lastValue + EffectManager.GetValue(this.GainPassive, null, this.m_value - this.m_lastValue, this.Entity, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false), 0f, this.m_baseMax);
			}
			else if (this.m_value < this.m_lastValue && this.LossPassive != PassiveEffects.None)
			{
				this.m_value = Mathf.Clamp(this.m_lastValue - EffectManager.GetValue(this.LossPassive, null, this.m_lastValue - this.m_value, this.Entity, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false), 0f, this.m_baseMax);
			}
		}
		if (this.m_value + this.RegenerationAmount > this.ModifiedMax)
		{
			this.RegenerationAmount = this.ModifiedMax - this.m_value;
		}
		this.RegenerationAmountUI = this.m_value - this.m_lastValue + this.RegenerationAmount / dt;
		this.m_value += this.RegenerationAmount;
		if (this.RegenerationAmount > 0f)
		{
			if (this.StatType == Stat.StatTypes.Stamina)
			{
				this.Entity.Stats.Water.RegenerationAmount -= this.RegenerationAmount * EffectManager.GetValue(PassiveEffects.WaterLossPerStaminaPointGained, null, 1f, this.Entity, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
				this.Entity.Stats.Food.RegenerationAmount -= this.RegenerationAmount * EffectManager.GetValue(PassiveEffects.FoodLossPerStaminaPointGained, null, 1f, this.Entity, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
			}
			else if (this.StatType == Stat.StatTypes.Health)
			{
				this.Entity.Stats.Water.RegenerationAmount -= this.RegenerationAmount * EffectManager.GetValue(PassiveEffects.WaterLossPerHealthPointGained, null, 1f, this.Entity, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
				this.Entity.Stats.Food.RegenerationAmount -= this.RegenerationAmount * EffectManager.GetValue(PassiveEffects.FoodLossPerHealthPointGained, null, 1f, this.Entity, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
			}
		}
		this.RegenerationAmount = this.m_value - this.m_lastValue;
		this.SetChangedFlag(this.m_value, this.m_lastValue);
		this.m_lastValue = this.m_value;
	}

	public void ResetAll()
	{
		this.ResetValue();
	}

	public void ResetValue()
	{
		this.m_value = this.m_originalValue;
		this.m_baseMax = this.m_originalBaseMax;
		this.m_maxModifier = 0f;
		this.m_valueModifier = 0f;
		this.m_changed = true;
	}

	public void SimpleAssignFrom(Stat stat)
	{
		this.m_baseMax = stat.m_baseMax;
		this.m_value = stat.m_value;
		this.m_maxModifier = stat.m_maxModifier;
		this.m_valueModifier = stat.m_valueModifier;
		this.m_originalValue = stat.m_originalValue;
		this.m_originalBaseMax = stat.m_originalBaseMax;
		this.m_changed = false;
	}

	public float Max
	{
		get
		{
			return this.m_baseMax;
		}
	}

	public float ModifiedMax
	{
		get
		{
			return this.m_baseMax + this.m_maxModifier;
		}
	}

	public float Value
	{
		get
		{
			if (this.GodModeEntity())
			{
				return this.ModifiedMax;
			}
			return Mathf.Clamp(this.m_value, 0f, this.ModifiedMax);
		}
		set
		{
			if (this.m_value != value)
			{
				this.m_value = Mathf.Clamp(value, 0f, this.ModifiedMax);
				this.SetChangedFlag(this.m_value, value);
			}
		}
	}

	public float ValuePercent
	{
		get
		{
			return Utils.FastClamp01(this.Value / this.ModifiedMax);
		}
	}

	public float ValuePercentUI
	{
		get
		{
			return Utils.FastClamp01(this.Value / this.Max);
		}
	}

	public float ModifiedMaxPercent
	{
		get
		{
			return Utils.FastClamp01(this.ModifiedMax / this.Max);
		}
	}

	public float UnclampedValue
	{
		get
		{
			return this.m_value;
		}
	}

	public float BaseMax
	{
		get
		{
			return this.m_baseMax;
		}
		set
		{
			if (this.m_baseMax != value)
			{
				this.SetChangedFlag(this.m_baseMax, value);
				this.m_baseMax = value;
			}
		}
	}

	public float MaxModifier
	{
		get
		{
			return this.m_maxModifier;
		}
		set
		{
			this.m_maxModifier = Mathf.Clamp(value, -(this.Max * 0.75f), 0f);
		}
	}

	public float OriginalValue
	{
		get
		{
			return this.m_originalValue;
		}
		set
		{
			this.m_originalValue = value;
		}
	}

	public float OriginalMax
	{
		get
		{
			return this.m_originalBaseMax;
		}
		set
		{
			this.m_originalBaseMax = value;
		}
	}

	public bool Changed
	{
		get
		{
			return this.m_changed;
		}
		set
		{
			this.m_changed = value;
		}
	}

	public void Write(BinaryWriter stream, ref ushort fileId)
	{
		stream.Write(5);
		stream.Write(this.m_value);
		stream.Write(this.m_maxModifier);
		stream.Write(this.m_valueModifier);
		stream.Write(this.m_baseMax);
		stream.Write(this.m_originalBaseMax);
		stream.Write(this.m_originalValue);
	}

	public void Read(BinaryReader stream)
	{
		stream.ReadInt32();
		this.m_value = stream.ReadSingle();
		this.m_maxModifier = stream.ReadSingle();
		this.m_valueModifier = stream.ReadSingle();
		this.m_baseMax = stream.ReadSingle();
		this.m_originalBaseMax = stream.ReadSingle();
		this.m_originalValue = stream.ReadSingle();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool GodModeEntity()
	{
		return this.Entity != null && this.Entity.entityId == this.Entity.world.GetPrimaryPlayerId() && !GameStats.GetBool(EnumGameStats.IsPlayerDamageEnabled);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetChangedFlag(float newValue, float oldValue)
	{
		this.m_changed = (this.m_changed || Mathf.FloorToInt(newValue) != Mathf.FloorToInt(oldValue));
	}

	public const int kBinaryVersion = 5;

	public Stat.StatTypes StatType;

	public PassiveEffects GainPassive;

	public PassiveEffects LossPassive;

	public PassiveEffects ChangeOTPassive;

	public PassiveEffects MaxPassive;

	[PublicizedFrom(EAccessModifier.Private)]
	public float m_baseMax;

	[PublicizedFrom(EAccessModifier.Private)]
	public float m_originalBaseMax;

	[PublicizedFrom(EAccessModifier.Private)]
	public float m_maxModifier;

	[PublicizedFrom(EAccessModifier.Private)]
	public float m_value;

	[PublicizedFrom(EAccessModifier.Private)]
	public float m_originalValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public float m_valueModifier;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool m_isEntityAlive;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool m_isEntityPlayer;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool m_changed;

	public EntityAlive Entity;

	[PublicizedFrom(EAccessModifier.Private)]
	public float m_gainPassive;

	[PublicizedFrom(EAccessModifier.Private)]
	public float m_lossPassive;

	[PublicizedFrom(EAccessModifier.Private)]
	public float m_lossMaxMult;

	[PublicizedFrom(EAccessModifier.Private)]
	public float regenAmount;

	[PublicizedFrom(EAccessModifier.Private)]
	public float m_lastValue;

	public enum StatTypes
	{
		Health,
		Stamina,
		Food,
		Water,
		CoreTemp,
		SpeedModifier
	}
}
