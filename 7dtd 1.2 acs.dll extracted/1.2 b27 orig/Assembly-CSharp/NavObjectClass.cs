using System;
using System.Collections.Generic;

public class NavObjectClass
{
	public static void Reset()
	{
		NavObjectClass.NavObjectClassList.Clear();
	}

	public bool IsOnMap(bool isActive)
	{
		return this.GetMapSettings(isActive) != null;
	}

	public bool IsOnCompass(bool isActive)
	{
		return this.GetCompassSettings(isActive) != null;
	}

	public bool IsOnScreen(bool isActive)
	{
		return this.GetOnScreenSettings(isActive) != null;
	}

	public NavObjectMapSettings GetMapSettings(bool isActive)
	{
		if (isActive)
		{
			return this.MapSettings;
		}
		return this.InactiveMapSettings;
	}

	public NavObjectCompassSettings GetCompassSettings(bool isActive)
	{
		if (isActive)
		{
			return this.CompassSettings;
		}
		return this.InactiveCompassSettings;
	}

	public NavObjectScreenSettings GetOnScreenSettings(bool isActive)
	{
		if (isActive)
		{
			return this.OnScreenSettings;
		}
		return this.InactiveOnScreenSettings;
	}

	public NavObjectClass(string name)
	{
		this.NavObjectClassName = name;
	}

	public static NavObjectClass GetNavObjectClass(string className)
	{
		for (int i = 0; i < NavObjectClass.NavObjectClassList.Count; i++)
		{
			if (NavObjectClass.NavObjectClassList[i].NavObjectClassName == className)
			{
				return NavObjectClass.NavObjectClassList[i];
			}
		}
		return null;
	}

	public void Init()
	{
		if (this.Properties.Values.ContainsKey("requirement_type"))
		{
			if (!Enum.TryParse<NavObjectClass.RequirementTypes>(this.Properties.Values["requirement_type"], out this.RequirementType))
			{
				this.RequirementType = NavObjectClass.RequirementTypes.None;
			}
			if (this.RequirementType != NavObjectClass.RequirementTypes.None && this.Properties.Values.ContainsKey("requirement_name"))
			{
				this.RequirementName = this.Properties.Values["requirement_name"];
			}
		}
		if (this.Properties.Values.ContainsKey("tag"))
		{
			this.Tag = this.Properties.Values["tag"];
		}
		if (this.Properties.Values.ContainsKey("use_override_icon"))
		{
			this.UseOverrideIcon = StringParsers.ParseBool(this.Properties.Values["use_override_icon"], 0, -1, true);
		}
	}

	public static List<NavObjectClass> NavObjectClassList = new List<NavObjectClass>();

	public DynamicProperties Properties = new DynamicProperties();

	public string NavObjectClassName = "";

	public NavObjectClass.RequirementTypes RequirementType;

	public string RequirementName = "";

	public bool UseOverrideIcon;

	public string Tag;

	public NavObjectMapSettings MapSettings;

	public NavObjectCompassSettings CompassSettings;

	public NavObjectScreenSettings OnScreenSettings;

	public NavObjectMapSettings InactiveMapSettings;

	public NavObjectCompassSettings InactiveCompassSettings;

	public NavObjectScreenSettings InactiveOnScreenSettings;

	public enum RequirementTypes
	{
		None,
		CVar,
		QuestBounds,
		Tracking,
		NoTag,
		InParty,
		IsAlly,
		IsPlayer,
		IsVehicleOwner,
		IsOwner,
		NoActiveQuests,
		MinimumTreasureRadius,
		IsTwitchSpawnedSelf,
		IsTwitchSpawnedOther
	}
}
