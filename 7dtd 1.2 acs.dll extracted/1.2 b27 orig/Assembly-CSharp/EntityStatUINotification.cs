using System;
using UnityEngine;

public abstract class EntityStatUINotification : EntityUINotification
{
	public void Tick(float dt)
	{
		if (this.visible && this.shouldBeVisible == this.visible && this._displayTime > 0f)
		{
			this._displayTime -= dt;
			if (this._displayTime <= 0f)
			{
				this.shouldBeVisible = false;
			}
		}
		this.OnTick(dt, this.firstTick);
		this.firstTick = false;
		if (this.shouldBeVisible != this.visible)
		{
			this.visible = this.shouldBeVisible;
			if (this.shouldBeVisible)
			{
				this.stats.NotificationAdded(this);
			}
		}
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
		if (this.Buff != null && this.Buff.BuffClass != null)
		{
			return this.Buff.BuffClass.IconColor;
		}
		return Color.white;
	}

	public virtual Color WarningColor
	{
		get
		{
			return Color.white;
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

	[PublicizedFrom(EAccessModifier.Protected)]
	public abstract void OnTick(float dt, bool firstTick);

	public float displayTime
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return this._displayTime;
		}
	}

	public bool isPermenentlyVisible
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return this.visible && this._displayTime == 0f;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void SetVisible(bool visible, float displayTime)
	{
		this.shouldBeVisible = visible;
		this._displayTime = displayTime;
	}

	public void SetBuff(BuffValue _buff)
	{
	}

	public void SetStats(EntityStats _stats)
	{
		this.stats = _stats;
	}

	public void Reset()
	{
		this.visible = false;
	}

	public void NotifyBuffRemoved()
	{
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

	public abstract float MinWarningLevel { get; }

	public abstract float MaxWarningLevel { get; }

	public abstract float CurrentValue { get; }

	public abstract string Units { get; }

	public abstract string Icon { get; }

	public float FadeOutTime
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
			return null;
		}
	}

	public virtual string Description
	{
		get
		{
			return "";
		}
	}

	public EntityStats EntityStats
	{
		get
		{
			return this.stats;
		}
	}

	public virtual bool Visible
	{
		get
		{
			return this.visible;
		}
	}

	public virtual bool Expired
	{
		get
		{
			return !this.Visible;
		}
	}

	public virtual EnumEntityUINotificationDisplayMode DisplayMode
	{
		get
		{
			return EnumEntityUINotificationDisplayMode.IconPlusCurrentValue;
		}
	}

	public abstract EnumEntityUINotificationSubject Subject { get; }

	[PublicizedFrom(EAccessModifier.Protected)]
	public EntityStatUINotification()
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityStats stats;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool visible;

	[PublicizedFrom(EAccessModifier.Private)]
	public float _displayTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool shouldBeVisible;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool firstTick = true;

	[PublicizedFrom(EAccessModifier.Protected)]
	public const float MaxWaitTime = 2f;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float waitTime = 2f;
}
