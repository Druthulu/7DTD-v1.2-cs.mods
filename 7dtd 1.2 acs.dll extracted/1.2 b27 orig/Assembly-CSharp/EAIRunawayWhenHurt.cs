using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class EAIRunawayWhenHurt : EAIRunAway
{
	public EAIRunawayWhenHurt()
	{
		this.MutexBits = 1;
	}

	public override void SetData(DictionarySave<string, string> data)
	{
		base.SetData(data);
		string input;
		if (data.TryGetValue("runChance", out input))
		{
			this.lowHealthPercent = 0f;
			if (StringParsers.ParseFloat(input, 0, -1, NumberStyles.Any) >= base.RandomFloat)
			{
				base.GetData(data, "healthPer", ref this.lowHealthPercent);
				if (data.TryGetValue("healthPerMax", out input))
				{
					float num = StringParsers.ParseFloat(input, 0, -1, NumberStyles.Any);
					this.lowHealthPercent += base.RandomFloat * (num - this.lowHealthPercent);
				}
			}
		}
	}

	public override bool CanExecute()
	{
		if (!this.theEntity.GetRevengeTarget())
		{
			return false;
		}
		if (this.lowHealthPercent < 1f)
		{
			if ((float)this.theEntity.Health / (float)this.theEntity.GetMaxHealth() >= this.lowHealthPercent)
			{
				return false;
			}
			this.theEntity.SetRevengeTimer((60 + base.GetRandom(60)) * 20);
		}
		return base.CanExecute();
	}

	public override bool Continue()
	{
		EntityAlive revengeTarget = this.theEntity.GetRevengeTarget();
		return revengeTarget && this.theEntity.GetDistanceSq(revengeTarget) < 2025f && base.Continue();
	}

	public override void Update()
	{
		base.Update();
		this.theEntity.navigator.setMoveSpeed(this.theEntity.IsInWater() ? this.theEntity.GetMoveSpeed() : this.theEntity.GetMoveSpeedPanic());
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override Vector3 GetFleeFromPos()
	{
		EntityAlive revengeTarget = this.theEntity.GetRevengeTarget();
		if (revengeTarget)
		{
			return revengeTarget.position;
		}
		return this.theEntity.position;
	}

	public override string ToString()
	{
		return string.Format("{0}, per {1}", base.ToString(), this.lowHealthPercent);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cSafeDistance = 45;

	[PublicizedFrom(EAccessModifier.Private)]
	public float lowHealthPercent = 1f;
}
