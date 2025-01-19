using System;

public sealed class WaterBuffNotification : BuffEntityUINotification
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
			return base.EntityStats.Water.Max;
		}
	}

	public override float MinWarningLevel
	{
		get
		{
			return base.EntityStats.Water.Max * 0.25f;
		}
	}

	public override float MaxWarningLevel
	{
		get
		{
			return base.EntityStats.Water.Max;
		}
	}

	public override float CurrentValue
	{
		get
		{
			return base.EntityStats.Water.Value;
		}
	}

	public override string Units
	{
		get
		{
			return "";
		}
	}

	public override string Description
	{
		get
		{
			return "You are getting thirsty";
		}
	}

	public override EnumEntityUINotificationDisplayMode DisplayMode
	{
		get
		{
			return EnumEntityUINotificationDisplayMode.IconPlusCurrentValue;
		}
	}

	public override EnumEntityUINotificationSubject Subject
	{
		get
		{
			return EnumEntityUINotificationSubject.Water;
		}
	}
}
