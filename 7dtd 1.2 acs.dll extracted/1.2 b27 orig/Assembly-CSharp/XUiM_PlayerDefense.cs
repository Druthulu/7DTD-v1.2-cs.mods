﻿using System;

public class XUiM_PlayerDefense : XUiModel
{
	public static string GetBashing(EntityPlayer player)
	{
		return string.Format("{0}%", XUiM_PlayerDefense.GetDefenseFromPlayer(player, EnumDamageTypes.Bashing));
	}

	public static string GetPiercing(EntityPlayer player)
	{
		return string.Format("{0}%", XUiM_PlayerDefense.GetDefenseFromPlayer(player, EnumDamageTypes.Piercing));
	}

	public static string GetRadiation(EntityPlayer player)
	{
		return string.Format("{0}%", XUiM_PlayerDefense.GetDefenseFromPlayer(player, EnumDamageTypes.Radiation));
	}

	public static string GetWaterproof(EntityPlayer player)
	{
		return string.Format("{0}%", (int)(player.equipment.GetTotalWaterproof() * 100f));
	}

	public static string GetFireproof(EntityPlayer player)
	{
		return string.Format("{0}%", XUiM_PlayerDefense.GetDefenseFromPlayer(player, EnumDamageTypes.Heat));
	}

	public static string GetElectrical(EntityPlayer player)
	{
		return string.Format("{0}%", XUiM_PlayerDefense.GetDefenseFromPlayer(player, EnumDamageTypes.Electrical));
	}

	public static string GetInsulation(EntityPlayer player)
	{
		return ValueDisplayFormatters.TemperatureRelative(player.equipment.GetTotalInsulation(), 0);
	}

	public static string GetWeight(EntityPlayer player)
	{
		float num = 0f;
		int slotCount = player.equipment.GetSlotCount();
		for (int i = 0; i < slotCount; i++)
		{
			ItemValue slotItem = player.equipment.GetSlotItem(i);
			if (slotItem != null)
			{
				ItemClass itemClass = slotItem.ItemClass;
				if (itemClass != null)
				{
					num += itemClass.Encumbrance * 100f;
				}
			}
		}
		return string.Format("{0}%", num.ToCultureInvariantString());
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string GetDefenseFromPlayer(EntityPlayer _player, EnumDamageTypes _armorType)
	{
		float value = 0f;
		if (_armorType != EnumDamageTypes.None && _armorType < EnumDamageTypes.Disease)
		{
			if (_armorType <= EnumDamageTypes.Crushing)
			{
				value = EffectManager.GetValue(PassiveEffects.PhysicalDamageResist, null, 0f, _player, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
			}
			else if (_armorType <= EnumDamageTypes.Electrical)
			{
				value = EffectManager.GetValue(PassiveEffects.ElementalDamageResist, null, 0f, _player, null, FastTags<TagGroup.Global>.Parse(_armorType.ToStringCached<EnumDamageTypes>()), true, true, true, true, true, 1, true, false);
			}
		}
		return value.ToCultureInvariantString("00");
	}

	public static string GetBashing(ItemValue itemValue, XUi _xui)
	{
		return string.Format("{0}%", XUiM_PlayerDefense.GetDefenseFromItemValue(itemValue, EnumDamageTypes.Bashing, _xui));
	}

	public static string GetPiercing(ItemValue itemValue, XUi _xui)
	{
		return string.Format("{0}%", XUiM_PlayerDefense.GetDefenseFromItemValue(itemValue, EnumDamageTypes.Piercing, _xui));
	}

	public static string GetRadiation(ItemValue itemValue, XUi _xui)
	{
		return string.Format("{0}%", XUiM_PlayerDefense.GetDefenseFromItemValue(itemValue, EnumDamageTypes.Radiation, _xui));
	}

	public static string GetWaterproof(ItemValue itemValue)
	{
		float num = 0f;
		ItemClass itemClass = itemValue.ItemClass;
		if (itemClass != null)
		{
			num = itemClass.WaterProof;
		}
		return string.Format("{0}%", (int)(num * 100f));
	}

	public static string GetFireproof(ItemValue itemValue, XUi _xui)
	{
		return string.Format("{0}%", XUiM_PlayerDefense.GetDefenseFromItemValue(itemValue, EnumDamageTypes.Heat, _xui));
	}

	public static string GetElectrical(ItemValue itemValue, XUi _xui)
	{
		return string.Format("{0}%", XUiM_PlayerDefense.GetDefenseFromItemValue(itemValue, EnumDamageTypes.Electrical, _xui));
	}

	public static string GetInsulation(ItemValue itemValue)
	{
		float fahrenheit = 0f;
		ItemClass itemClass = itemValue.ItemClass;
		if (itemClass != null)
		{
			fahrenheit = itemClass.Insulation;
		}
		return ValueDisplayFormatters.TemperatureRelative(fahrenheit, 0);
	}

	public static string GetWeight(ItemValue itemValue)
	{
		float value = 0f;
		ItemClass itemClass = itemValue.ItemClass;
		if (itemClass != null)
		{
			value = itemClass.Encumbrance * 100f;
		}
		return string.Format("{0}%", value.ToCultureInvariantString());
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string GetDefenseFromItemValue(ItemValue _itemValue, EnumDamageTypes _damageType, XUi _xui)
	{
		return (EffectManager.GetValue(PassiveEffects.PhysicalDamageResist, _itemValue, 0f, _xui.playerUI.entityPlayer, null, FastTags<TagGroup.Global>.Parse(_damageType.ToStringCached<EnumDamageTypes>()), true, true, true, true, true, 1, true, false) * 100f).ToCultureInvariantString("00");
	}
}
