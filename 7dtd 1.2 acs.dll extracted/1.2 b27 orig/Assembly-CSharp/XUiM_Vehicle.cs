using System;
using Audio;

public class XUiM_Vehicle : XUiModel
{
	public static string GetEntityName(XUi _xui)
	{
		if (!(_xui.vehicle != null))
		{
			return "";
		}
		return _xui.vehicle.EntityName;
	}

	public static float GetSpeed(XUi _xui)
	{
		if (!(_xui.vehicle != null))
		{
			return 0f;
		}
		return _xui.vehicle.GetVehicle().MaxPossibleSpeed;
	}

	public static string GetNoise(XUi _xui)
	{
		if (_xui.vehicle == null)
		{
			return "";
		}
		XUiM_Vehicle.checkLocalization();
		float noise = _xui.vehicle.GetVehicle().GetNoise();
		if (noise <= 0.33f)
		{
			return XUiM_Vehicle.lblNoiseSoft;
		}
		if (noise <= 0.66f)
		{
			return XUiM_Vehicle.lblNoiseModerate;
		}
		return XUiM_Vehicle.lblNoiseLoud;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void checkLocalization()
	{
		if (!XUiM_Vehicle.HasLocalizationBeenCached)
		{
			XUiM_Vehicle.lblNoiseSoft = Localization.Get("xuiVehicleNoiseSoft", false);
			XUiM_Vehicle.lblNoiseModerate = Localization.Get("xuiVehicleNoiseModerate", false);
			XUiM_Vehicle.lblNoiseLoud = Localization.Get("xuiVehicleNoiseLoud", false);
			XUiM_Vehicle.lblSpeedNone = Localization.Get("xuiVehicleSpeedNone", false);
			XUiM_Vehicle.lblSpeedSlow = Localization.Get("xuiVehicleSpeedSlow", false);
			XUiM_Vehicle.lblSpeedNormal = Localization.Get("xuiVehicleSpeedNormal", false);
			XUiM_Vehicle.lblSpeedFast = Localization.Get("xuiVehicleSpeedFast", false);
			XUiM_Vehicle.HasLocalizationBeenCached = true;
		}
	}

	public static float GetProtection(XUi _xui)
	{
		if (_xui.vehicle == null)
		{
			return 0f;
		}
		return (1f - _xui.vehicle.GetVehicle().GetPlayerDamagePercent()) * 100f;
	}

	public static float GetFuelLevel(XUi _xui)
	{
		if (!(_xui.vehicle != null))
		{
			return 0f;
		}
		return _xui.vehicle.GetVehicle().GetFuelPercent() * 100f;
	}

	public static float GetFuelFill(XUi _xui)
	{
		if (!(_xui.vehicle != null))
		{
			return 0f;
		}
		return _xui.vehicle.GetVehicle().GetFuelPercent();
	}

	public static int GetPassengers(XUi _xui)
	{
		if (!(_xui.vehicle != null))
		{
			return 1;
		}
		return _xui.vehicle.GetAttachMaxCount();
	}

	public static string GetSpeedText(XUi _xui)
	{
		float num = (_xui.vehicle != null) ? _xui.vehicle.GetVehicle().MaxPossibleSpeed : 0f;
		XUiM_Vehicle.checkLocalization();
		if (num <= 0f)
		{
			return XUiM_Vehicle.lblSpeedNone;
		}
		if (num <= 9f)
		{
			return XUiM_Vehicle.lblSpeedSlow;
		}
		if (num <= 12f)
		{
			return XUiM_Vehicle.lblSpeedNormal;
		}
		return XUiM_Vehicle.lblSpeedFast;
	}

	public bool SetPart(XUi _xui, string vehicleSlotName, ItemStack stack, out ItemStack resultStack)
	{
		Log.Warning("XUiM_Vehicle SetPart {0}", new object[]
		{
			vehicleSlotName
		});
		_xui.vehicle == null;
		resultStack = stack;
		return false;
	}

	public void RefreshVehicle()
	{
	}

	public static bool RepairVehicle(XUi _xui, Vehicle vehicle = null)
	{
		if (vehicle == null)
		{
			vehicle = _xui.vehicle.GetVehicle();
		}
		ItemValue item = ItemClass.GetItem("resourceRepairKit", false);
		if (item.ItemClass != null)
		{
			EntityPlayerLocal entityPlayer = _xui.playerUI.entityPlayer;
			LocalPlayerUI playerUI = _xui.playerUI;
			int itemCount = entityPlayer.bag.GetItemCount(item, -1, -1, true);
			int itemCount2 = entityPlayer.inventory.GetItemCount(item, false, -1, -1, true);
			int repairAmountNeeded = vehicle.GetRepairAmountNeeded();
			if (itemCount + itemCount2 > 0 && repairAmountNeeded > 0)
			{
				float num = 0f;
				ProgressionValue progressionValue = entityPlayer.Progression.GetProgressionValue("perkGreaseMonkey");
				if (progressionValue != null)
				{
					num += (float)progressionValue.Level * 0.1f;
				}
				vehicle.RepairParts(1000, num);
				if (itemCount2 > 0)
				{
					entityPlayer.inventory.DecItem(item, 1, false, null);
				}
				else
				{
					entityPlayer.bag.DecItem(item, 1, false, null);
				}
				playerUI.xui.CollectedItemList.RemoveItemStack(new ItemStack(item, 1));
				Manager.PlayInsidePlayerHead("craft_complete_item", -1, 0f, false, false);
				return true;
			}
			if (repairAmountNeeded > itemCount + itemCount2)
			{
				Manager.PlayInsidePlayerHead("misc/missingitemtorepair", -1, 0f, false, false);
			}
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cRepairBase = 1000;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cRepairPercent = 0f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cRepairPerkPercent = 0.1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public static string lblNoiseSoft;

	[PublicizedFrom(EAccessModifier.Private)]
	public static string lblNoiseModerate;

	[PublicizedFrom(EAccessModifier.Private)]
	public static string lblNoiseLoud;

	[PublicizedFrom(EAccessModifier.Private)]
	public static string lblSpeedNone;

	[PublicizedFrom(EAccessModifier.Private)]
	public static string lblSpeedSlow;

	[PublicizedFrom(EAccessModifier.Private)]
	public static string lblSpeedNormal;

	[PublicizedFrom(EAccessModifier.Private)]
	public static string lblSpeedFast;

	public static bool HasLocalizationBeenCached;
}
