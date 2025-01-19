using System;

public sealed class FoodBuffNotification : BuffEntityUINotification
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
			return base.EntityStats.Food.Max;
		}
	}

	public override float MinWarningLevel
	{
		get
		{
			return base.EntityStats.Food.Max * 0.25f;
		}
	}

	public override float MaxWarningLevel
	{
		get
		{
			return base.EntityStats.Food.Max;
		}
	}

	public override float CurrentValue
	{
		get
		{
			return base.EntityStats.Food.Value;
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
			return "You are getting hungry";
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
			return EnumEntityUINotificationSubject.Food;
		}
	}
}
