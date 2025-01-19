using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_CombineGrid : XUiC_ItemStackGrid
{
	public override void Init()
	{
		base.Init();
		XUiController[] childrenByType = base.GetChildrenByType<XUiC_ItemStack>(null);
		XUiController[] array = childrenByType;
		if (array.Length == 3)
		{
			this.merge1 = (XUiC_RequiredItemStack)array[0];
			this.merge2 = (XUiC_RequiredItemStack)array[1];
			this.result1 = (XUiC_RequiredItemStack)array[2];
			this.merge1.StackLocation = XUiC_ItemStack.StackLocationTypes.Merge;
			this.merge2.StackLocation = XUiC_ItemStack.StackLocationTypes.Merge;
			this.result1.StackLocation = XUiC_ItemStack.StackLocationTypes.Merge;
			this.merge1.RequiredType = (this.merge2.RequiredType = (this.result1.RequiredType = XUiC_RequiredItemStack.RequiredTypes.HasQualityNoParts));
			this.merge1.SlotChangedEvent += this.Merge_SlotChangedEvent;
			this.merge2.SlotChangedEvent += this.Merge_SlotChangedEvent;
			this.result1.HiddenLock = true;
			this.result1.SlotChangedEvent += this.Result1_SlotChangedEvent;
			this.result1.TakeOnly = true;
			this.merge1.FailedSwap += this.Merge_FailedSwap;
			this.merge2.FailedSwap += this.Merge_FailedSwap;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Merge_FailedSwap(ItemStack stack)
	{
		ItemClass itemClass = stack.itemValue.ItemClass;
		if (!stack.itemValue.HasQuality || itemClass.HasSubItems)
		{
			GameManager.ShowTooltip(base.xui.playerUI.entityPlayer, Localization.Get("ttCombineInvalidItem", false), false);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Result1_SlotChangedEvent(int slotNumber, ItemStack stack)
	{
		if (stack.IsEmpty())
		{
			this.merge1.ItemStack = ItemStack.Empty.Clone();
			this.merge2.ItemStack = ItemStack.Empty.Clone();
			if (this.lastResult != null)
			{
				base.xui.playerUI.entityPlayer.Progression.AddLevelExp((int)this.experienceFromLastResult, "_xpOther", Progression.XPTypes.Other, true, true);
				this.lastResult = null;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Merge_SlotChangedEvent(int slotNumber, ItemStack stack)
	{
		if (this.merge1.ItemStack.IsEmpty() || this.merge2.ItemStack.IsEmpty() || this.merge1.ItemStack.itemValue.type != this.merge2.ItemStack.itemValue.type)
		{
			this.result1.SlotChangedEvent -= this.Result1_SlotChangedEvent;
			this.result1.ItemStack = ItemStack.Empty.Clone();
			this.result1.HiddenLock = true;
			this.result1.SlotChangedEvent += this.Result1_SlotChangedEvent;
			return;
		}
		if (this.merge1.ItemStack.itemValue.type == this.merge2.ItemStack.itemValue.type)
		{
			bool flag = false;
			bool flag2 = false;
			ItemStack itemStack;
			ItemStack itemStack2;
			if (this.merge1.ItemStack.itemValue.Quality > this.merge2.ItemStack.itemValue.Quality)
			{
				itemStack = this.merge1.ItemStack;
				itemStack2 = this.merge2.ItemStack;
			}
			else if (this.merge2.ItemStack.itemValue.Quality > this.merge1.ItemStack.itemValue.Quality)
			{
				itemStack = this.merge2.ItemStack;
				itemStack2 = this.merge1.ItemStack;
			}
			else if (this.merge1.ItemStack.itemValue.UseTimes < this.merge2.ItemStack.itemValue.UseTimes)
			{
				itemStack = this.merge1.ItemStack;
				itemStack2 = this.merge2.ItemStack;
			}
			else
			{
				itemStack = this.merge2.ItemStack;
				itemStack2 = this.merge1.ItemStack;
			}
			ItemStack itemStack3 = itemStack.Clone();
			int num = (int)EffectManager.GetValue(PassiveEffects.CraftingTier, null, 1f, XUiM_Player.GetPlayer(), null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false) + 1;
			this.experienceFromLastResult = 0f;
			int num2 = 0;
			if (itemStack.itemValue.UseTimes != 0f)
			{
				num2 = Mathf.Min(itemStack2.itemValue.MaxUseTimes - (int)itemStack2.itemValue.UseTimes, (int)itemStack3.itemValue.UseTimes);
				itemStack3.itemValue.UseTimes -= (float)num2;
				this.experienceFromLastResult += (float)itemStack3.itemValue.MaxUseTimes / (float)(itemStack3.itemValue.MaxUseTimes - num2);
				flag = true;
			}
			if (itemStack2.itemValue.UseTimes + (float)num2 < (float)itemStack2.itemValue.MaxUseTimes && itemStack.itemValue.Quality < 6 && (int)itemStack.itemValue.Quality < num)
			{
				float num3 = ((float)itemStack2.itemValue.MaxUseTimes - itemStack2.itemValue.UseTimes) / (float)itemStack2.itemValue.MaxUseTimes;
				int num4 = (int)Math.Max(1f, (float)itemStack2.itemValue.Quality * num3 * 0.1f);
				if ((int)itemStack3.itemValue.Quality + num4 > num)
				{
					num4 = num - (int)itemStack3.itemValue.Quality;
				}
				if ((int)itemStack3.itemValue.Quality + num4 > 6)
				{
					itemStack3.itemValue.Quality = 6;
				}
				else
				{
					ItemValue itemValue = itemStack3.itemValue;
					itemValue.Quality += (ushort)num4;
				}
				itemStack3.itemValue = new ItemValue(itemStack3.itemValue.type, (int)itemStack3.itemValue.Quality, (int)itemStack3.itemValue.Quality, false, null, 1f);
				this.experienceFromLastResult *= (float)num4;
				flag2 = true;
			}
			if (flag || flag2)
			{
				this.result1.ItemStack = itemStack3;
				this.result1.HiddenLock = false;
				this.lastResult = itemStack3;
				return;
			}
			GameManager.ShowTooltip(base.xui.playerUI.entityPlayer, string.Format(Localization.Get("ttCombineLimitExceeded", false), num), string.Empty, "ui_denied", null, false);
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		if (base.ViewComponent != null && !base.ViewComponent.IsVisible)
		{
			base.ViewComponent.OnOpen();
			base.ViewComponent.IsVisible = true;
		}
		XUiC_ItemInfoWindow childByType = base.xui.GetChildByType<XUiC_ItemInfoWindow>();
		this.merge1.InfoWindow = childByType;
		this.merge2.InfoWindow = childByType;
		this.result1.InfoWindow = childByType;
		this.IsDirty = true;
		this.merge1.ItemStack = ItemStack.Empty.Clone();
		this.merge2.ItemStack = ItemStack.Empty.Clone();
		this.result1.ItemStack = ItemStack.Empty.Clone();
		base.xui.currentCombineGrid = this;
	}

	public override void OnClose()
	{
		base.OnClose();
		if (base.ViewComponent != null && base.ViewComponent.IsVisible)
		{
			base.ViewComponent.OnClose();
			base.ViewComponent.IsVisible = false;
		}
		this.IsDirty = true;
		XUiM_PlayerInventory playerInventory = base.xui.PlayerInventory;
		if (!this.merge1.ItemStack.IsEmpty())
		{
			playerInventory.AddItem(this.merge1.ItemStack);
		}
		if (!this.merge2.ItemStack.IsEmpty())
		{
			playerInventory.AddItem(this.merge2.ItemStack);
		}
		base.xui.currentCombineGrid = null;
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public bool TryAddItemToSlot(ItemClass itemClass, ItemStack itemStack)
	{
		if (this.merge1.ItemStack.IsEmpty())
		{
			if (itemClass.HasQuality && !itemClass.HasSubItems)
			{
				this.merge1.ItemStack = itemStack;
				return true;
			}
		}
		else if (this.merge2.ItemStack.IsEmpty() && itemClass.HasQuality && !itemClass.HasSubItems)
		{
			this.merge2.ItemStack = itemStack;
			return true;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_RequiredItemStack merge1;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_RequiredItemStack merge2;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_RequiredItemStack result1;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemStack lastResult;

	[PublicizedFrom(EAccessModifier.Private)]
	public float experienceFromLastResult;
}
