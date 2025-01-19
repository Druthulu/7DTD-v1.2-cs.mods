using System;
using UnityEngine;

public class BuffEntityUINotification : EntityUINotification
{
	public virtual void Tick(float dt)
	{
	}

	public void SetBuff(BuffValue buff)
	{
		this.buff = buff;
	}

	public void SetStats(EntityStats stats)
	{
		this.stats = stats;
		this.owner = stats.Entity;
	}

	public void NotifyBuffRemoved()
	{
		this.expired = true;
	}

	public virtual Color GetColor()
	{
		if (this.MinValue != this.MaxValue)
		{
			if (this.CurrentValue <= Mathf.Lerp(this.MinValue, this.MaxValue, 0.25f))
			{
				return this.EmergencyColor;
			}
			if (this.CurrentValue <= Mathf.Lerp(this.MinValue, this.MaxValue, 0.5f))
			{
				return this.AlertColor;
			}
			if (this.CurrentValue <= Mathf.Lerp(this.MinValue, this.MaxValue, 0.75f))
			{
				return this.WarningColor;
			}
		}
		return this.Buff.BuffClass.IconColor;
	}

	public virtual float MinValue
	{
		get
		{
			return 0f;
		}
	}

	public virtual float MaxValue
	{
		get
		{
			return 0f;
		}
	}

	public virtual float MinWarningLevel
	{
		get
		{
			return float.MinValue;
		}
	}

	public virtual float MaxWarningLevel
	{
		get
		{
			return float.MaxValue;
		}
	}

	public virtual float CurrentValue
	{
		get
		{
			if (this.buff != null && this.buff.BuffClass != null && this.buff.BuffClass.DisplayValueCVar != null)
			{
				return this.owner.Buffs.GetCustomVar(this.buff.BuffClass.DisplayValueCVar, 0f);
			}
			return 0f;
		}
	}

	public virtual string Units
	{
		get
		{
			if (this.buff == null || this.buff.BuffClass == null || this.buff.BuffClass.DisplayValueCVar == null)
			{
				return "";
			}
			if (this.buff.BuffClass.DisplayValueCVar.StartsWith("$") || this.buff.BuffClass.DisplayValueCVar.StartsWith(".") || this.buff.BuffClass.DisplayValueCVar.StartsWith("_"))
			{
				return "cvar";
			}
			return this.buff.BuffClass.DisplayValueCVar;
		}
	}

	public virtual string Icon
	{
		get
		{
			return this.buff.BuffClass.Icon;
		}
	}

	public virtual bool IconBlink
	{
		get
		{
			return this.buff.BuffClass.IconBlink || EffectManager.GetValue(PassiveEffects.BuffBlink, null, 0f, this.owner, null, this.buff.BuffClass.NameTag, false, false, false, true, true, 1, true, false) >= 1f;
		}
	}

	public virtual float FadeOutTime
	{
		get
		{
			return 0.15f;
		}
	}

	public virtual BuffValue Buff
	{
		get
		{
			return this.buff;
		}
	}

	public virtual Color WarningColor
	{
		get
		{
			return Color.yellow;
		}
	}

	public virtual Color AlertColor
	{
		get
		{
			return Color.yellow + Color.red;
		}
	}

	public virtual Color EmergencyColor
	{
		get
		{
			return Color.red;
		}
	}

	public virtual string Description
	{
		get
		{
			if (this.buff == null)
			{
				return "";
			}
			return this.buff.BuffClass.Description;
		}
	}

	public EntityStats EntityStats
	{
		get
		{
			return this.stats;
		}
	}

	public virtual bool Expired
	{
		get
		{
			return this.expired;
		}
	}

	public virtual bool Visible
	{
		get
		{
			return this.buff != null && !this.buff.BuffClass.Hidden && !this.buff.Paused;
		}
	}

	public virtual EnumEntityUINotificationDisplayMode DisplayMode
	{
		get
		{
			EnumEntityUINotificationDisplayMode result = (this.buff != null && this.buff.BuffClass != null) ? this.buff.BuffClass.DisplayType : EnumEntityUINotificationDisplayMode.IconOnly;
			if (this.buff.BuffClass.DisplayValueCVar != null && this.buff.BuffClass.DisplayValueCVar != "")
			{
				result = EnumEntityUINotificationDisplayMode.IconPlusCurrentValue;
			}
			return result;
		}
	}

	public virtual EnumEntityUINotificationSubject Subject
	{
		get
		{
			return EnumEntityUINotificationSubject.Buff;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public BuffValue buff;

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityStats stats;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool expired;

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityAlive owner;
}
