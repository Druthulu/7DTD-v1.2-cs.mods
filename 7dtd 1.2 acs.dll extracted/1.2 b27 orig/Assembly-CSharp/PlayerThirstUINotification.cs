using System;
using Audio;
using UnityEngine;

public class PlayerThirstUINotification : EntityStatUINotification
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnTick(float dt, bool firstTick)
	{
		if (this.waitTime > 0f)
		{
			this.waitTime -= dt;
		}
		this.waitTime = 2f;
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
		else if (Mathf.FloorToInt(currentValue * 100f) > Mathf.FloorToInt(this.previousValue * 100f))
		{
			base.SetVisible(true, 2f);
		}
		this.previousValue = currentValue;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PlayWarningSound()
	{
		Manager.BroadcastPlay("Player*Thirsty");
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
			if (base.EntityStats == null || base.EntityStats.Water == null)
			{
				return 100f;
			}
			return (float)Mathf.RoundToInt(base.EntityStats.Water.Value + base.EntityStats.Entity.Buffs.GetCustomVar("$waterAmount", 0f));
		}
	}

	public override string Units
	{
		get
		{
			return "";
		}
	}

	public override string Icon
	{
		get
		{
			return "ui_game_symbol_thirst";
		}
	}

	public override EnumEntityUINotificationSubject Subject
	{
		get
		{
			return EnumEntityUINotificationSubject.Water;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public float previousValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public float nextSoundTime;
}
