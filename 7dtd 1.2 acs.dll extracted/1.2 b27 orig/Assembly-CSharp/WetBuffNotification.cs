using System;
using UnityEngine;

public sealed class WetBuffNotification : BuffEntityUINotification
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
			return this.MinValue;
		}
	}

	public override float MaxWarningLevel
	{
		get
		{
			return this.MaxValue;
		}
	}

	public override float CurrentValue
	{
		get
		{
			return Mathf.Clamp01(base.EntityStats.WaterLevel + 0.01f);
		}
	}

	public override string Units
	{
		get
		{
			return "%";
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
