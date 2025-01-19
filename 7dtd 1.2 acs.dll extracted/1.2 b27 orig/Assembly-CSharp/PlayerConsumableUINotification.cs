using System;
using Audio;
using UnityEngine;

public class PlayerConsumableUINotification : EntityStatUINotification
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnTick(float dt, bool firstTick)
	{
		float currentValue = this.CurrentValue;
		if (currentValue < 0.1f)
		{
			if (this.previousValue >= 0.1f)
			{
				this.PlayWarningSound();
			}
			else
			{
				this.PlayWarningSoundIfTime(dt);
			}
			base.SetVisible(true, 0f);
		}
		else if (currentValue <= 0.25f && (this.previousValue > 0.25f || firstTick))
		{
			base.SetVisible(true, 10f);
			this.PlayWarningSound();
		}
		else if (currentValue <= 0.5f && (this.previousValue > 0.5f || firstTick))
		{
			base.SetVisible(true, 10f);
		}
		else if (currentValue <= 0.75f && (this.previousValue > 0.75f || firstTick))
		{
			base.SetVisible(true, 10f);
		}
		else if (currentValue > this.previousValue)
		{
			base.SetVisible(true, 10f);
		}
		this.previousValue = currentValue;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PlayWarningSound()
	{
		if (this.subject == EnumEntityUINotificationSubject.Food)
		{
			Manager.BroadcastPlay("Player*Hungry");
		}
		else
		{
			Manager.BroadcastPlay("Player*Thirsty");
		}
		this.nextSoundTime = 60f + UnityEngine.Random.value * 15f;
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

	public override float MinWarningLevel
	{
		get
		{
			return 0.25f;
		}
	}

	public override float MaxWarningLevel
	{
		get
		{
			return float.MaxValue;
		}
	}

	public override float CurrentValue
	{
		get
		{
			return base.EntityStats.Water.ValuePercent;
		}
	}

	public override string Units
	{
		get
		{
			return "%";
		}
	}

	public override string Icon
	{
		get
		{
			if (this.subject == EnumEntityUINotificationSubject.Food)
			{
				return "ui_game_symbol_hunger";
			}
			return "ui_game_symbol_thirst";
		}
	}

	public override EnumEntityUINotificationSubject Subject
	{
		get
		{
			return this.subject;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Stat liveStat;

	[PublicizedFrom(EAccessModifier.Private)]
	public EnumEntityUINotificationSubject subject;

	[PublicizedFrom(EAccessModifier.Private)]
	public float previousValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public float nextSoundTime;
}
