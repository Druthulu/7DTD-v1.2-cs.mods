using System;

public sealed class TemperatureBuffNotification : BuffEntityUINotification
{
	public override float MinValue
	{
		get
		{
			return float.MinValue;
		}
	}

	public override float MaxValue
	{
		get
		{
			return float.MaxValue;
		}
	}

	public override float MinWarningLevel
	{
		get
		{
			return 30f;
		}
	}

	public override float MaxWarningLevel
	{
		get
		{
			return 100f;
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

	public override EnumEntityUINotificationDisplayMode DisplayMode
	{
		get
		{
			return EnumEntityUINotificationDisplayMode.IconPlusCurrentValue;
		}
	}
}
