using System;
using UnityEngine;

public class XUiM_Trader : XUiModel
{
	public static int GetBuyPrice(XUi _xui, ItemValue itemValue, int count, ItemClass itemClass = null, int index = -1)
	{
		bool flag = false;
		if (itemClass == null)
		{
			itemClass = itemValue.ItemClass;
		}
		float num;
		int economicBundleSize;
		if (itemClass.IsBlock())
		{
			num = Block.list[itemValue.type].EconomicValue;
			economicBundleSize = Block.list[itemValue.type].EconomicBundleSize;
		}
		else
		{
			num = EffectManager.GetValue(PassiveEffects.EconomicValue, itemValue, itemClass.EconomicValue, _xui.playerUI.entityPlayer, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
			economicBundleSize = itemClass.EconomicBundleSize;
		}
		if (num == 0f)
		{
			return 0;
		}
		float num2 = 0f;
		float num3;
		if (_xui.Trader.Trader == null)
		{
			num3 = TraderInfo.BuyMarkup;
		}
		else if (_xui.Trader.Trader.TraderInfo.Rentable || _xui.Trader.Trader.TraderInfo.PlayerOwned)
		{
			num3 = 1f + (float)_xui.Trader.Trader.GetMarkupByIndex(index) * 0.2f;
			flag = true;
		}
		else
		{
			flag = (_xui.Trader.Trader.TraderInfo.OverrideBuyMarkup != -1f);
			num3 = (flag ? _xui.Trader.Trader.TraderInfo.OverrideBuyMarkup : TraderInfo.BuyMarkup);
		}
		if (itemValue.HasQuality)
		{
			num2 = num * num3;
			num2 *= Mathf.Lerp(TraderInfo.QualityMinMod, TraderInfo.QualityMaxMod, ((float)itemValue.Quality - 1f) / 5f);
			float percentUsesLeft = itemValue.PercentUsesLeft;
			num2 *= percentUsesLeft;
		}
		else if (itemClass.HasSubItems)
		{
			for (int i = 0; i < itemValue.Modifications.Length; i++)
			{
				ItemValue itemValue2 = itemValue.Modifications[i];
				if (!itemValue2.IsEmpty())
				{
					num2 += (float)XUiM_Trader.GetBuyPrice(_xui, itemValue2, 1, null, -1);
				}
			}
		}
		else
		{
			num2 = num * num3;
		}
		if (!flag)
		{
			num2 -= num2 * EffectManager.GetValue(PassiveEffects.BarteringBuying, null, 0f, XUiM_Player.GetPlayer(), null, itemClass.ItemTags, true, true, true, true, true, 1, true, false);
		}
		return (int)(num2 * (float)(count / economicBundleSize));
	}

	public static int GetSellPrice(XUi _xui, ItemValue itemValue, int count, ItemClass itemClass = null)
	{
		bool flag = false;
		if (itemClass == null)
		{
			itemClass = itemValue.ItemClass;
		}
		float num;
		int economicBundleSize;
		if (itemClass.IsBlock())
		{
			Block block = Block.list[itemValue.type];
			num = block.EconomicValue;
			num *= block.EconomicSellScale;
			economicBundleSize = block.EconomicBundleSize;
		}
		else
		{
			num = itemClass.EconomicValue;
			num *= itemClass.EconomicSellScale;
			num = EffectManager.GetValue(PassiveEffects.EconomicValue, itemValue, num, _xui.playerUI.entityPlayer, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
			economicBundleSize = itemClass.EconomicBundleSize;
		}
		if (num == 0f)
		{
			return 0;
		}
		float num2 = 0f;
		float num3;
		if (_xui.Trader.Trader == null)
		{
			num3 = TraderInfo.SellMarkdown;
		}
		else
		{
			flag = (_xui.Trader.Trader.TraderInfo.OverrideSellMarkdown != -1f);
			num3 = (flag ? _xui.Trader.Trader.TraderInfo.OverrideSellMarkdown : TraderInfo.SellMarkdown);
		}
		if (itemValue.HasQuality)
		{
			num2 = num * num3;
			num2 *= Mathf.Lerp(TraderInfo.QualityMinMod, TraderInfo.QualityMaxMod, ((float)itemValue.Quality - 1f) / 5f);
			float percentUsesLeft = itemValue.PercentUsesLeft;
			num2 *= percentUsesLeft;
		}
		else if (itemClass.HasSubItems)
		{
			for (int i = 0; i < itemValue.Modifications.Length; i++)
			{
				ItemValue itemValue2 = itemValue.Modifications[i];
				if (!itemValue2.IsEmpty())
				{
					num2 += (float)XUiM_Trader.GetSellPrice(_xui, itemValue2, 1, null);
				}
			}
		}
		else
		{
			num2 = num * num3;
		}
		if (!flag)
		{
			num2 += num2 * EffectManager.GetValue(PassiveEffects.BarteringSelling, null, 0f, XUiM_Player.GetPlayer(), null, itemClass.ItemTags, true, true, true, true, true, 1, true, false);
		}
		return (int)(num2 * (float)(count / economicBundleSize));
	}

	public TraderData Trader;

	public EntityNPC TraderEntity;

	public XUiC_TraderWindowGroup TraderWindowGroup;

	public TileEntityTrader TraderTileEntity;
}
