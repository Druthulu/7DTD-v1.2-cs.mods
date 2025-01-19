using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class XUiM_ItemStack : XUiModel
{
	public static bool HasItemStats(ItemStack itemStack)
	{
		if (itemStack.itemValue.ItemClass == null)
		{
			return false;
		}
		if (itemStack.itemValue.ItemClass.IsBlock())
		{
			return Block.list[itemStack.itemValue.type].DisplayType != "";
		}
		return itemStack.itemValue.ItemClass.DisplayType != "";
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string BuffActionStrings(ItemAction itemAction, List<string> stringList)
	{
		if (itemAction.BuffActions == null || itemAction.BuffActions.Count == 0)
		{
			return "";
		}
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < itemAction.BuffActions.Count; i++)
		{
			BuffClass buff = BuffManager.GetBuff(itemAction.BuffActions[i]);
			if (buff != null && !string.IsNullOrEmpty(buff.Name))
			{
				stringList.Add(XUiM_ItemStack.StringFormatHandler(Localization.Get("lblEffect", false), string.Format("{0}", buff.Name)));
			}
		}
		return stringBuilder.ToString();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string getColoredItemStat(string _title, float _value)
	{
		if (_value > 0f)
		{
			return string.Format("{0}: [00ff00]+{1}[-]", _title, _value.ToCultureInvariantString());
		}
		if (_value < 0f)
		{
			return string.Format("{0}: [ff0000]{1}[-]", _title, _value.ToCultureInvariantString());
		}
		return "";
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string getColoredItemStatPercentage(string _title, float _value)
	{
		if (_value > 0f)
		{
			return string.Format("{0}: [00ff00]+{1}%[-]", _title, _value.ToCultureInvariantString("0.0"));
		}
		if (_value < 0f)
		{
			return string.Format("{0}: [ff0000]{1}%[-]", _title, _value.ToCultureInvariantString("0.0"));
		}
		return "";
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string StringFormatHandler(string title, object value)
	{
		return string.Format("{0}: [REPLACE_COLOR]{1}[-]\n", title, value);
	}

	public static string GetStatItemValueTextWithModInfo(ItemStack itemStack, EntityPlayer player, DisplayInfoEntry infoEntry)
	{
		FastTags<TagGroup.Global> tags = infoEntry.TagsSet ? infoEntry.Tags : (XUiM_ItemStack.primaryFastTags | XUiM_ItemStack.physicalDamageFastTags);
		MinEventParams.CachedEventParam.ItemValue = itemStack.itemValue;
		MinEventParams.CachedEventParam.Seed = (int)itemStack.itemValue.Seed;
		float num;
		float num2;
		if (infoEntry.CustomName == "")
		{
			num = EffectManager.GetValue(infoEntry.StatType, itemStack.itemValue, 0f, player, null, tags, false, false, false, false, true, 1, false, false);
			num2 = EffectManager.GetValue(infoEntry.StatType, itemStack.itemValue, 0f, player, null, tags, false, false, false, false, true, 1, true, false);
		}
		else
		{
			num = XUiM_ItemStack.GetCustomValue(infoEntry, itemStack, false);
			num2 = XUiM_ItemStack.GetCustomValue(infoEntry, itemStack, true);
		}
		if (((infoEntry.DisplayType == DisplayInfoEntry.DisplayTypes.Decimal1 || infoEntry.DisplayType == DisplayInfoEntry.DisplayTypes.Decimal2 || infoEntry.DisplayType == DisplayInfoEntry.DisplayTypes.Percent) && Mathf.Floor(num2 * 100f) != Mathf.Floor(num * 100f)) || (infoEntry.DisplayType == DisplayInfoEntry.DisplayTypes.Integer && Mathf.Floor(num2) != Mathf.Floor(num)))
		{
			if (infoEntry.DisplayType == DisplayInfoEntry.DisplayTypes.Percent)
			{
				num *= 100f;
				num = Mathf.Floor(num);
				num2 *= 100f;
				num2 = Mathf.Floor(num2);
				if (infoEntry.ShowInverted)
				{
					num -= 100f;
					num2 -= 100f;
				}
				float num3 = num2 - num;
				bool flag = num3 > 0f;
				bool flag2 = infoEntry.NegativePreferred ? (!flag) : flag;
				string text = (num2 > 0f && infoEntry.DisplayLeadingPlus) ? "+" : "";
				return string.Concat(new string[]
				{
					text,
					num2.ToString(),
					"% (",
					flag2 ? "[00FF00]" : "[FF0000]",
					flag ? "+" : "",
					num3.ToString(),
					"%[-])"
				});
			}
			if (infoEntry.DisplayType == DisplayInfoEntry.DisplayTypes.Decimal1)
			{
				num2 *= 10f;
				num2 = Mathf.Floor(num2);
				num2 /= 10f;
				num *= 10f;
				num = Mathf.Floor(num);
				num /= 10f;
			}
			else if (infoEntry.DisplayType == DisplayInfoEntry.DisplayTypes.Decimal2)
			{
				num2 *= 100f;
				num2 = Mathf.Floor(num2);
				num2 /= 100f;
				num *= 100f;
				num = Mathf.Floor(num);
				num /= 100f;
			}
			else
			{
				num2 = Mathf.Floor(num2);
				num = Mathf.Floor(num);
			}
			if (infoEntry.DisplayType == DisplayInfoEntry.DisplayTypes.Time)
			{
				float num4 = num2 - num;
				bool flag3 = num4 > 0f;
				bool flag4 = infoEntry.NegativePreferred ? (!flag3) : flag3;
				return string.Concat(new string[]
				{
					XUiM_PlayerBuffs.GetCVarValueAsTimeString(num2),
					" (",
					flag4 ? "[00FF00]" : "[FF0000]",
					flag3 ? "+" : "",
					XUiM_PlayerBuffs.GetCVarValueAsTimeString(num4),
					"[-])"
				});
			}
			if (infoEntry.ShowInverted)
			{
				num -= 1f;
				num2 -= 1f;
			}
			float num5 = num2 - num;
			bool flag5 = num5 > 0f;
			bool flag6 = infoEntry.NegativePreferred ? (!flag5) : flag5;
			string text2 = (num > 0f && infoEntry.DisplayLeadingPlus) ? "+" : "";
			return string.Concat(new string[]
			{
				text2,
				num2.ToString(),
				" (",
				flag6 ? "[00FF00]" : "[FF0000]",
				flag5 ? "+" : "",
				num5.ToString("0.##"),
				"[-])"
			});
		}
		else
		{
			if (infoEntry.DisplayType == DisplayInfoEntry.DisplayTypes.Percent)
			{
				num2 *= 100f;
				num2 = Mathf.Floor(num2);
				if (infoEntry.ShowInverted)
				{
					num2 -= 100f;
				}
				return ((num2 > 0f && infoEntry.DisplayLeadingPlus) ? "+" : "") + num2.ToString("0") + "%";
			}
			if (infoEntry.DisplayType == DisplayInfoEntry.DisplayTypes.Decimal1)
			{
				num2 *= 10f;
				num2 = Mathf.Floor(num2);
				num2 /= 10f;
			}
			else if (infoEntry.DisplayType == DisplayInfoEntry.DisplayTypes.Decimal2)
			{
				num2 *= 100f;
				num2 = Mathf.Floor(num2);
				num2 /= 100f;
			}
			else
			{
				num2 = Mathf.Floor(num2);
			}
			if (infoEntry.ShowInverted)
			{
				num2 -= 1f;
			}
			if (infoEntry.DisplayType == DisplayInfoEntry.DisplayTypes.Time)
			{
				return XUiM_PlayerBuffs.GetCVarValueAsTimeString(num2);
			}
			return ((num2 > 0f && infoEntry.DisplayLeadingPlus) ? "+" : "") + num2.ToString("0.##");
		}
	}

	public static string GetStatItemValueTextWithModColoring(ItemStack itemStack, EntityPlayer player, DisplayInfoEntry infoEntry)
	{
		FastTags<TagGroup.Global> tags = infoEntry.TagsSet ? infoEntry.Tags : (XUiM_ItemStack.primaryFastTags | XUiM_ItemStack.physicalDamageFastTags);
		MinEventParams.CachedEventParam.ItemValue = itemStack.itemValue;
		MinEventParams.CachedEventParam.Seed = (int)itemStack.itemValue.Seed;
		float num;
		float num2;
		if (infoEntry.CustomName == "")
		{
			MinEventParams.CachedEventParam.ItemValue = itemStack.itemValue;
			MinEventParams.CachedEventParam.Seed = (int)itemStack.itemValue.Seed;
			num = EffectManager.GetValue(infoEntry.StatType, itemStack.itemValue, 0f, player, null, tags, false, false, false, false, true, 1, false, false);
			num2 = EffectManager.GetValue(infoEntry.StatType, itemStack.itemValue, 0f, player, null, tags, false, false, false, false, true, 1, true, false);
		}
		else
		{
			num = XUiM_ItemStack.GetCustomValue(infoEntry, itemStack, false);
			num2 = XUiM_ItemStack.GetCustomValue(infoEntry, itemStack, true);
		}
		if (infoEntry.DisplayType == DisplayInfoEntry.DisplayTypes.Bool)
		{
			if (num != num2)
			{
				bool flag = num2 > num;
				return ((infoEntry.NegativePreferred ? (!flag) : flag) ? "[00FF00]" : "[FF0000]") + XUiM_ItemStack.ShowLocalizedBool(Convert.ToBoolean(num2)) + "%[-]";
			}
			return XUiM_ItemStack.ShowLocalizedBool(Convert.ToBoolean(num));
		}
		else if (infoEntry.DisplayType == DisplayInfoEntry.DisplayTypes.Time)
		{
			if (num != num2)
			{
				if (infoEntry.ShowInverted)
				{
					num -= 1f;
					num2 -= 1f;
				}
				bool flag2 = num2 - num > 0f;
				return ((infoEntry.NegativePreferred ? (!flag2) : flag2) ? "[00FF00]" : "[FF0000]") + XUiM_PlayerBuffs.GetCVarValueAsTimeString(num) + "[-])";
			}
			return XUiM_PlayerBuffs.GetCVarValueAsTimeString(num);
		}
		else if (((infoEntry.DisplayType == DisplayInfoEntry.DisplayTypes.Decimal1 || infoEntry.DisplayType == DisplayInfoEntry.DisplayTypes.Decimal2 || infoEntry.DisplayType == DisplayInfoEntry.DisplayTypes.Percent) && Mathf.Floor(num2 * 100f) != Mathf.Floor(num * 100f)) || (infoEntry.DisplayType == DisplayInfoEntry.DisplayTypes.Integer && Mathf.Floor(num2) != Mathf.Floor(num)))
		{
			if (infoEntry.DisplayType == DisplayInfoEntry.DisplayTypes.Percent)
			{
				num *= 100f;
				num = Mathf.Floor(num);
				num2 *= 100f;
				num2 = Mathf.Floor(num2);
				if (infoEntry.ShowInverted)
				{
					num -= 100f;
					num2 -= 100f;
				}
				bool flag3 = num2 - num > 0f;
				bool flag4 = infoEntry.NegativePreferred ? (!flag3) : flag3;
				string str = (num2 > 0f && infoEntry.DisplayLeadingPlus) ? "+" : "";
				return (flag4 ? "[00FF00]" : "[FF0000]") + str + num2.ToString() + "%[-]";
			}
			if (infoEntry.DisplayType == DisplayInfoEntry.DisplayTypes.Decimal1)
			{
				num2 *= 10f;
				num2 = Mathf.Floor(num2);
				num2 /= 10f;
				num *= 10f;
				num = Mathf.Floor(num);
				num /= 10f;
			}
			else if (infoEntry.DisplayType == DisplayInfoEntry.DisplayTypes.Decimal2)
			{
				num2 *= 100f;
				num2 = Mathf.Floor(num2);
				num2 /= 100f;
				num *= 100f;
				num = Mathf.Floor(num);
				num /= 100f;
			}
			else
			{
				num2 = Mathf.Floor(num2);
				num = Mathf.Floor(num);
			}
			if (infoEntry.DisplayType == DisplayInfoEntry.DisplayTypes.Time)
			{
				bool flag5 = num2 - num > 0f;
				return ((infoEntry.NegativePreferred ? (!flag5) : flag5) ? "[00FF00]" : "[FF0000]") + XUiM_PlayerBuffs.GetCVarValueAsTimeString(num2) + "[-]";
			}
			if (infoEntry.ShowInverted)
			{
				num -= 1f;
				num2 -= 1f;
			}
			bool flag6 = num2 - num > 0f;
			bool flag7 = infoEntry.NegativePreferred ? (!flag6) : flag6;
			string str2 = (num > 0f && infoEntry.DisplayLeadingPlus) ? "+" : "";
			return (flag7 ? "[00FF00]" : "[FF0000]") + str2 + num2.ToString() + "[-]";
		}
		else
		{
			if (infoEntry.DisplayType == DisplayInfoEntry.DisplayTypes.Percent)
			{
				num2 *= 100f;
				num2 = Mathf.Floor(num2);
				if (infoEntry.ShowInverted)
				{
					num2 -= 100f;
				}
				return ((num2 > 0f && infoEntry.DisplayLeadingPlus) ? "+" : "") + num2.ToString("0") + "%";
			}
			if (infoEntry.DisplayType == DisplayInfoEntry.DisplayTypes.Decimal1)
			{
				num2 *= 10f;
				num2 = Mathf.Floor(num2);
				num2 /= 10f;
			}
			else if (infoEntry.DisplayType == DisplayInfoEntry.DisplayTypes.Decimal2)
			{
				num2 *= 100f;
				num2 = Mathf.Floor(num2);
				num2 /= 100f;
			}
			else
			{
				num2 = Mathf.Floor(num2);
			}
			if (infoEntry.ShowInverted)
			{
				num2 -= 1f;
			}
			if (infoEntry.DisplayType == DisplayInfoEntry.DisplayTypes.Time)
			{
				return XUiM_PlayerBuffs.GetCVarValueAsTimeString(num2);
			}
			return ((num2 > 0f && infoEntry.DisplayLeadingPlus) ? "+" : "") + num2.ToString("0.##");
		}
	}

	public static string GetStatItemValueTextWithCompareInfo(ItemStack itemStack, ItemStack compareStack, EntityPlayer player, DisplayInfoEntry infoEntry, bool flipCompare = false, bool useMods = true)
	{
		FastTags<TagGroup.Global> tags = infoEntry.TagsSet ? infoEntry.Tags : (XUiM_ItemStack.primaryFastTags | XUiM_ItemStack.physicalDamageFastTags);
		if (compareStack.IsEmpty() || compareStack == itemStack)
		{
			return XUiM_ItemStack.GetStatItemValueTextWithModColoring(itemStack, player, infoEntry);
		}
		float num;
		float num2;
		if (infoEntry.CustomName == "")
		{
			MinEventParams.CachedEventParam.ItemValue = itemStack.itemValue;
			MinEventParams.CachedEventParam.Seed = (int)itemStack.itemValue.Seed;
			num = EffectManager.GetValue(infoEntry.StatType, itemStack.itemValue, 0f, player, null, tags, false, false, false, false, true, 1, useMods, false);
			num2 = EffectManager.GetValue(infoEntry.StatType, compareStack.itemValue, 0f, player, null, tags, false, false, false, false, true, 1, useMods, false);
		}
		else
		{
			num = XUiM_ItemStack.GetCustomValue(infoEntry, itemStack, useMods);
			num2 = XUiM_ItemStack.GetCustomValue(infoEntry, compareStack, useMods);
		}
		if (infoEntry.DisplayType == DisplayInfoEntry.DisplayTypes.Bool)
		{
			if (!compareStack.IsEmpty() && num != num2)
			{
				bool flag = num2 > num;
				bool flag2 = infoEntry.NegativePreferred ? (!flag) : flag;
				return string.Concat(new string[]
				{
					XUiM_ItemStack.ShowLocalizedBool(Convert.ToBoolean(num)),
					" (",
					flag2 ? "[00FF00]" : "[FF0000]",
					XUiM_ItemStack.ShowLocalizedBool(Convert.ToBoolean(num2)),
					"[-])"
				});
			}
			return XUiM_ItemStack.ShowLocalizedBool(Convert.ToBoolean(num));
		}
		else if (infoEntry.DisplayType == DisplayInfoEntry.DisplayTypes.Time)
		{
			if (!compareStack.IsEmpty() && num != num2)
			{
				if (infoEntry.ShowInverted)
				{
					num -= 1f;
					num2 -= 1f;
				}
				float num3 = num2 - num;
				if (flipCompare)
				{
					num3 = num - num2;
				}
				bool flag3 = num3 > 0f;
				bool flag4 = infoEntry.NegativePreferred ? (!flag3) : flag3;
				return string.Concat(new string[]
				{
					XUiM_PlayerBuffs.GetCVarValueAsTimeString(num),
					" (",
					flag4 ? "[00FF00]" : "[FF0000]",
					flag3 ? "+" : "-",
					XUiM_PlayerBuffs.GetCVarValueAsTimeString(Mathf.Abs(num3)),
					"[-])"
				});
			}
			return XUiM_PlayerBuffs.GetCVarValueAsTimeString(num);
		}
		else if (!compareStack.IsEmpty() && (((infoEntry.DisplayType == DisplayInfoEntry.DisplayTypes.Decimal1 || infoEntry.DisplayType == DisplayInfoEntry.DisplayTypes.Decimal2 || infoEntry.DisplayType == DisplayInfoEntry.DisplayTypes.Percent) && Mathf.Floor(num2 * 100f) != Mathf.Floor(num * 100f)) || (infoEntry.DisplayType == DisplayInfoEntry.DisplayTypes.Integer && Mathf.Floor(num2) != Mathf.Floor(num))))
		{
			if (infoEntry.DisplayType == DisplayInfoEntry.DisplayTypes.Percent)
			{
				num *= 100f;
				num = Mathf.Floor(num);
				num2 *= 100f;
				num2 = Mathf.Floor(num2);
				if (infoEntry.ShowInverted)
				{
					num -= 100f;
					num2 -= 100f;
				}
				float num4 = num2 - num;
				if (flipCompare)
				{
					num4 = num - num2;
				}
				bool flag5 = num4 > 0f;
				bool flag6 = infoEntry.NegativePreferred ? (!flag5) : flag5;
				string text = (num > 0f && infoEntry.DisplayLeadingPlus) ? "+" : "";
				return string.Concat(new string[]
				{
					text,
					num.ToString(),
					"% (",
					flag6 ? "[00FF00]" : "[FF0000]",
					flag5 ? "+" : "",
					num4.ToString(),
					"%[-])"
				});
			}
			if (infoEntry.DisplayType == DisplayInfoEntry.DisplayTypes.Decimal1)
			{
				num2 *= 10f;
				num2 = Mathf.Floor(num2);
				num2 /= 10f;
				num *= 10f;
				num = Mathf.Floor(num);
				num /= 10f;
			}
			else if (infoEntry.DisplayType == DisplayInfoEntry.DisplayTypes.Decimal2)
			{
				num2 *= 100f;
				num2 = Mathf.Floor(num2);
				num2 /= 100f;
				num *= 100f;
				num = Mathf.Floor(num);
				num /= 100f;
			}
			else
			{
				num2 = Mathf.Floor(num2);
				num = Mathf.Floor(num);
			}
			if (infoEntry.ShowInverted)
			{
				num -= 1f;
				num2 -= 1f;
			}
			float num5 = num2 - num;
			if (flipCompare)
			{
				num5 = num - num2;
			}
			bool flag7 = num5 > 0f;
			bool flag8 = infoEntry.NegativePreferred ? (!flag7) : flag7;
			string text2 = (num > 0f && infoEntry.DisplayLeadingPlus) ? "+" : "";
			return string.Concat(new string[]
			{
				text2,
				num.ToString(),
				" (",
				flag8 ? "[00FF00]" : "[FF0000]",
				flag7 ? "+" : "",
				num5.ToString("0.##"),
				"[-])"
			});
		}
		else
		{
			if (infoEntry.DisplayType == DisplayInfoEntry.DisplayTypes.Percent)
			{
				num *= 100f;
				num = Mathf.Floor(num);
				if (infoEntry.ShowInverted)
				{
					num -= 100f;
				}
				return ((num > 0f && infoEntry.DisplayLeadingPlus) ? "+" : "") + num.ToString("0") + "%";
			}
			if (infoEntry.DisplayType == DisplayInfoEntry.DisplayTypes.Decimal1)
			{
				num *= 10f;
				num = Mathf.Floor(num);
				num /= 10f;
			}
			else if (infoEntry.DisplayType == DisplayInfoEntry.DisplayTypes.Decimal2)
			{
				num *= 100f;
				num = Mathf.Floor(num);
				num /= 100f;
			}
			else
			{
				num = Mathf.Floor(num);
			}
			if (infoEntry.ShowInverted)
			{
				num -= 1f;
			}
			return ((num > 0f && infoEntry.DisplayLeadingPlus) ? "+" : "") + num.ToString("0.##");
		}
	}

	public static string ShowLocalizedBool(bool value)
	{
		if (XUiM_ItemStack.localizedTrue == "")
		{
			XUiM_ItemStack.localizedTrue = Localization.Get("statTrue", false);
			XUiM_ItemStack.localizedFalse = Localization.Get("statFalse", false);
		}
		if (!value)
		{
			return XUiM_ItemStack.localizedFalse;
		}
		return XUiM_ItemStack.localizedTrue;
	}

	public static bool CanCompare(ItemClass item1, ItemClass item2)
	{
		if (item1 == null || item2 == null)
		{
			return false;
		}
		string displayType = item1.DisplayType;
		string displayType2 = item2.DisplayType;
		if (item1.IsBlock())
		{
			displayType = Block.list[item1.Id].DisplayType;
		}
		if (item2.IsBlock())
		{
			displayType2 = Block.list[item2.Id].DisplayType;
		}
		ItemDisplayEntry displayStatsForTag = UIDisplayInfoManager.Current.GetDisplayStatsForTag(displayType);
		ItemDisplayEntry displayStatsForTag2 = UIDisplayInfoManager.Current.GetDisplayStatsForTag(displayType2);
		return displayStatsForTag != null && displayStatsForTag2 != null && displayStatsForTag.DisplayGroup == displayStatsForTag2.DisplayGroup;
	}

	public static float GetCustomValue(DisplayInfoEntry entry, ItemStack itemStack, bool useMods)
	{
		string customName = entry.CustomName;
		uint num = <PrivateImplementationDetails>.ComputeStringHash(customName);
		if (num <= 2273019518U)
		{
			if (num <= 1180197617U)
			{
				if (num != 470662067U)
				{
					if (num != 944097337U)
					{
						if (num == 1180197617U)
						{
							if (customName == "Mass")
							{
								Block block = itemStack.itemValue.ToBlockValue().Block;
								if (block != null)
								{
									return (float)block.blockMaterial.Mass.Value;
								}
								goto IL_56C;
							}
						}
					}
					else if (customName == "RequiredPower")
					{
						BlockPowered blockPowered = itemStack.itemValue.ToBlockValue().Block as BlockPowered;
						if (blockPowered != null)
						{
							return (float)blockPowered.RequiredPower;
						}
						goto IL_56C;
					}
				}
				else if (customName == "Explosion.RadiusEntities")
				{
					Block block2 = itemStack.itemValue.ToBlockValue().Block;
					if (block2 == null)
					{
						goto IL_56C;
					}
					BlockMine blockMine = block2 as BlockMine;
					if (blockMine != null)
					{
						return (float)blockMine.Explosion.EntityRadius;
					}
					BlockCarExplode blockCarExplode = block2 as BlockCarExplode;
					if (blockCarExplode != null)
					{
						return (float)blockCarExplode.Explosion.EntityRadius;
					}
					BlockCarExplodeLoot blockCarExplodeLoot = block2 as BlockCarExplodeLoot;
					if (blockCarExplodeLoot != null)
					{
						return (float)blockCarExplodeLoot.Explosion.EntityRadius;
					}
					goto IL_56C;
				}
			}
			else if (num != 1509954053U)
			{
				if (num != 2218964596U)
				{
					if (num == 2273019518U)
					{
						if (customName == "Explosion.RadiusBlocks")
						{
							Block block3 = itemStack.itemValue.ToBlockValue().Block;
							if (block3 == null)
							{
								goto IL_56C;
							}
							BlockMine blockMine2 = block3 as BlockMine;
							if (blockMine2 != null)
							{
								return blockMine2.Explosion.BlockRadius;
							}
							BlockCarExplode blockCarExplode2 = block3 as BlockCarExplode;
							if (blockCarExplode2 != null)
							{
								return blockCarExplode2.Explosion.BlockRadius;
							}
							BlockCarExplodeLoot blockCarExplodeLoot2 = block3 as BlockCarExplodeLoot;
							if (blockCarExplodeLoot2 != null)
							{
								return blockCarExplodeLoot2.Explosion.BlockRadius;
							}
							goto IL_56C;
						}
					}
				}
				else if (customName == "Explosion.EntityDamage")
				{
					Block block4 = itemStack.itemValue.ToBlockValue().Block;
					if (block4 == null)
					{
						goto IL_56C;
					}
					BlockMine blockMine3 = block4 as BlockMine;
					if (blockMine3 != null)
					{
						return blockMine3.Explosion.EntityDamage;
					}
					BlockCarExplode blockCarExplode3 = block4 as BlockCarExplode;
					if (blockCarExplode3 != null)
					{
						return blockCarExplode3.Explosion.EntityDamage;
					}
					BlockCarExplodeLoot blockCarExplodeLoot3 = block4 as BlockCarExplodeLoot;
					if (blockCarExplodeLoot3 != null)
					{
						return blockCarExplodeLoot3.Explosion.EntityDamage;
					}
					goto IL_56C;
				}
			}
			else if (customName == "StabilityGlue")
			{
				Block block5 = itemStack.itemValue.ToBlockValue().Block;
				if (block5 != null)
				{
					return (float)block5.blockMaterial.StabilityGlue;
				}
				goto IL_56C;
			}
		}
		else if (num <= 2523555452U)
		{
			if (num != 2276058132U)
			{
				if (num != 2327184015U)
				{
					if (num == 2523555452U)
					{
						if (customName == "LightOpacity")
						{
							Block block6 = itemStack.itemValue.ToBlockValue().Block;
							if (block6 != null)
							{
								return (float)block6.lightOpacity;
							}
							goto IL_56C;
						}
					}
				}
				else if (customName == "StabilitySupport")
				{
					Block block7 = itemStack.itemValue.ToBlockValue().Block;
					if (block7 == null)
					{
						goto IL_56C;
					}
					if (!block7.StabilitySupport)
					{
						return 0f;
					}
					return 1f;
				}
			}
			else if (customName == "MaxDamage")
			{
				Block block8 = itemStack.itemValue.ToBlockValue().Block;
				if (block8 != null)
				{
					return (float)block8.MaxDamage;
				}
				goto IL_56C;
			}
		}
		else if (num != 2927752580U)
		{
			if (num != 3550496702U)
			{
				if (num == 3706968837U)
				{
					if (customName == "ExplosionResistance")
					{
						Block block9 = itemStack.itemValue.ToBlockValue().Block;
						if (block9 != null)
						{
							return block9.GetExplosionResistance();
						}
						goto IL_56C;
					}
				}
			}
			else if (customName == "Explosion.BlockDamage")
			{
				Block block10 = itemStack.itemValue.ToBlockValue().Block;
				if (block10 == null)
				{
					goto IL_56C;
				}
				BlockMine blockMine4 = block10 as BlockMine;
				if (blockMine4 != null)
				{
					return blockMine4.Explosion.BlockDamage;
				}
				BlockCarExplode blockCarExplode4 = block10 as BlockCarExplode;
				if (blockCarExplode4 != null)
				{
					return blockCarExplode4.Explosion.BlockDamage;
				}
				BlockCarExplodeLoot blockCarExplodeLoot4 = block10 as BlockCarExplodeLoot;
				if (blockCarExplodeLoot4 != null)
				{
					return blockCarExplodeLoot4.Explosion.BlockDamage;
				}
				goto IL_56C;
			}
		}
		else if (customName == "FertileLevel")
		{
			Block block11 = itemStack.itemValue.ToBlockValue().Block;
			if (block11 != null)
			{
				return (float)block11.blockMaterial.FertileLevel;
			}
			goto IL_56C;
		}
		float num2 = 0f;
		if (itemStack.itemValue.ItemClass != null && itemStack.itemValue.ItemClass.Effects != null && itemStack.itemValue.ItemClass.Effects.EffectGroups != null)
		{
			num2 = XUiM_ItemStack.GetCustomDisplayValueForItem(itemStack.itemValue, entry);
			if (useMods)
			{
				for (int i = 0; i < itemStack.itemValue.Modifications.Length; i++)
				{
					if (itemStack.itemValue.Modifications[i] != null && itemStack.itemValue.Modifications[i].ItemClass is ItemClassModifier)
					{
						num2 += XUiM_ItemStack.GetCustomDisplayValueForItem(itemStack.itemValue.Modifications[i], entry);
					}
				}
			}
		}
		return num2;
		IL_56C:
		return 0f;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static float GetCustomDisplayValueForItem(ItemValue itemValue, DisplayInfoEntry entry)
	{
		float num = 0f;
		for (int i = 0; i < itemValue.ItemClass.Effects.EffectGroups.Count; i++)
		{
			MinEffectGroup minEffectGroup = itemValue.ItemClass.Effects.EffectGroups[i];
			MinEventParams.CachedEventParam.ItemValue = itemValue;
			MinEventParams.CachedEventParam.Seed = (int)itemValue.Seed;
			if (minEffectGroup.EffectDisplayValues.ContainsKey(entry.CustomName) && minEffectGroup.EffectDisplayValues[entry.CustomName].IsValid(MinEventParams.CachedEventParam))
			{
				num += minEffectGroup.EffectDisplayValues[entry.CustomName].GetValue((int)itemValue.Quality);
			}
			List<MinEventActionBase> triggeredEffects = minEffectGroup.TriggeredEffects;
			if (triggeredEffects != null)
			{
				for (int j = 0; j < triggeredEffects.Count; j++)
				{
					MinEventActionBase minEventActionBase = triggeredEffects[j];
					if ((minEventActionBase.EventType == MinEventTypes.onSelfPrimaryActionEnd || minEventActionBase.EventType == MinEventTypes.onSelfSecondaryActionEnd) && minEventActionBase is MinEventActionModifyCVar)
					{
						bool flag = true;
						for (int k = 0; k < minEventActionBase.Requirements.Count; k++)
						{
							if (!minEventActionBase.Requirements[k].IsValid(MinEventParams.CachedEventParam))
							{
								flag = false;
								break;
							}
						}
						if (flag)
						{
							MinEventActionModifyCVar minEventActionModifyCVar = minEventActionBase as MinEventActionModifyCVar;
							if (minEventActionModifyCVar.cvarName == entry.CustomName && minEventActionModifyCVar.targetType == MinEventActionTargetedBase.TargetTypes.self)
							{
								num += minEventActionModifyCVar.GetValueForDisplay();
							}
						}
					}
				}
			}
		}
		return num;
	}

	public static bool CheckKnown(EntityPlayerLocal player, ItemClass itemClass, ItemValue itemValue = null)
	{
		string unlocks = itemClass.Unlocks;
		bool flag = false;
		if (unlocks != "")
		{
			if (player.GetCVar(unlocks) == 1f)
			{
				flag = true;
			}
			if (!flag)
			{
				ProgressionValue progressionValue = player.Progression.GetProgressionValue(unlocks);
				if (progressionValue != null)
				{
					if (progressionValue.ProgressionClass.IsCrafting)
					{
						if (progressionValue.Level == progressionValue.ProgressionClass.MaxLevel)
						{
							flag = true;
						}
					}
					else if (progressionValue.Level == 1)
					{
						flag = true;
					}
				}
			}
			if (!flag)
			{
				Recipe recipe = CraftingManager.GetRecipe(unlocks);
				if (recipe != null && !recipe.scrapable && !recipe.wildcardForgeCategory && recipe.IsUnlocked(player))
				{
					flag = true;
				}
			}
		}
		return flag;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static FastTags<TagGroup.Global> primaryFastTags = FastTags<TagGroup.Global>.Parse("primary");

	[PublicizedFrom(EAccessModifier.Private)]
	public static FastTags<TagGroup.Global> physicalDamageFastTags = FastTags<TagGroup.Global>.Parse("piercing,bashing,slashing,crushing,none,corrosive");

	[PublicizedFrom(EAccessModifier.Private)]
	public static string localizedTrue = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public static string localizedFalse = "";
}
