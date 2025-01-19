using System;
using UnityEngine;

public class PlayerTemperatureUINotification : EntityStatUINotification
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnTick(float dt, bool firstTick)
	{
		float currentValue = this.CurrentValue;
		if (currentValue < 45f)
		{
			if (currentValue <= 30f && this.AnyBuffsPresent(PlayerTemperatureUINotification.coldBuffs))
			{
				base.SetVisible(false, 0f);
				this.PlayWarningSoundIfTime(dt);
			}
			else
			{
				if (this.previousValue >= 45f || firstTick)
				{
					this.PlayWarningSound();
				}
				base.SetVisible(true, 0f);
			}
		}
		else if (currentValue >= 90f)
		{
			if (currentValue >= 100f && this.AnyBuffsPresent(PlayerTemperatureUINotification.hotBuffs))
			{
				base.SetVisible(false, 0f);
				this.PlayWarningSoundIfTime(dt);
			}
			else
			{
				if (this.previousValue < 90f || firstTick)
				{
					this.PlayWarningSound();
				}
				base.SetVisible(true, 0f);
			}
		}
		else if (base.isPermenentlyVisible)
		{
			base.SetVisible(true, 3f);
		}
		this.previousValue = currentValue;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PlayWarningSound()
	{
		if (this.CurrentValue >= 90f)
		{
			base.EntityStats.Entity.PlayOneShot("Player*Hot", false, false, false);
		}
		else
		{
			base.EntityStats.Entity.PlayOneShot("Player*Cold", false, false, false);
		}
		this.nextSoundTime = 35f + UnityEngine.Random.value * 15f;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PlayWarningSoundIfTime(float dt)
	{
		this.nextSoundTime -= dt;
		if (this.nextSoundTime <= 0f)
		{
			this.PlayWarningSound();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool AnyBuffsPresent(string[] buffs)
	{
		for (int i = 0; i < buffs.Length; i++)
		{
			if (base.EntityStats.Entity.Buffs.GetBuff(buffs[i]) != null)
			{
				return true;
			}
		}
		return false;
	}

	public override float MinValue
	{
		get
		{
			return 0f;
		}
	}

	public override float MaxValue
	{
		get
		{
			return 0f;
		}
	}

	public override float MinWarningLevel
	{
		get
		{
			return 40f;
		}
	}

	public override float MaxWarningLevel
	{
		get
		{
			return 90f;
		}
	}

	public override float CurrentValue
	{
		get
		{
			return base.EntityStats.CoreTemp.Value;
		}
	}

	public override string Units
	{
		get
		{
			return "°";
		}
	}

	public override string Description
	{
		get
		{
			if (this.CurrentValue < 50f)
			{
				return "You are getting cold";
			}
			return "You are getting hot";
		}
	}

	public override string Icon
	{
		get
		{
			float value = base.EntityStats.CoreTemp.Value;
			if (value >= 100f)
			{
				return "ui_game_symbol_hot";
			}
			if (value <= 30f)
			{
				return "ui_game_symbol_cold";
			}
			return "ui_game_symbol_temperature";
		}
	}

	public override EnumEntityUINotificationSubject Subject
	{
		get
		{
			return EnumEntityUINotificationSubject.CoreTemp;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public float previousValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public float nextSoundTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly string[] hotBuffs = new string[]
	{
		"overheated",
		"heat1",
		"heat2"
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly string[] coldBuffs = new string[]
	{
		"freezing",
		"hypo1",
		"hypo2",
		"hypo3"
	};
}
