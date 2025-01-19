using System;

public sealed class UnderwaterBuffNotification : BuffEntityUINotification
{
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
			return 1f;
		}
	}

	public override float MinWarningLevel
	{
		get
		{
			return 0f;
		}
	}

	public override float MaxWarningLevel
	{
		get
		{
			return 1f;
		}
	}

	public override float CurrentValue
	{
		get
		{
			if (this.Buff.BuffClass.DurationMax != 0f)
			{
				return this.Buff.DurationInSeconds / this.Buff.BuffClass.DurationMax;
			}
			return 0f;
		}
	}

	public override string Units
	{
		get
		{
			return "%";
		}
	}

	public override string Description
	{
		get
		{
			return "You are underwater";
		}
	}

	public override EnumEntityUINotificationDisplayMode DisplayMode
	{
		get
		{
			return EnumEntityUINotificationDisplayMode.IconPlusCurrentValue;
		}
	}
}
