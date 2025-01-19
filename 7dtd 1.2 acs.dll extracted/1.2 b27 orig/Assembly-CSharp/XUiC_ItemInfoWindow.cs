﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ItemInfoWindow : XUiC_InfoWindow
{
	public bool isOpenAsTrader
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return base.xui.Trader != null && base.xui.Trader.Trader != null;
		}
	}

	public XUiC_SelectableEntry HoverEntry
	{
		get
		{
			return this.hoverEntry;
		}
		set
		{
			if (this.hoverEntry != value)
			{
				this.hoverEntry = value;
				if (this.hoverEntry != null && !this.hoverEntry.Selected && !this.itemStack.IsEmpty())
				{
					ItemStack hoverControllerItemStack = this.GetHoverControllerItemStack();
					if (!hoverControllerItemStack.IsEmpty() && XUiM_ItemStack.CanCompare(hoverControllerItemStack.itemValue.ItemClass, this.itemClass))
					{
						this.CompareStack = hoverControllerItemStack;
						return;
					}
					this.CompareStack = ItemStack.Empty;
					return;
				}
				else
				{
					this.CompareStack = ItemStack.Empty;
				}
			}
		}
	}

	public ItemStack CompareStack
	{
		get
		{
			return this.compareStack;
		}
		set
		{
			if (this.compareStack != value)
			{
				this.compareStack = value;
				base.RefreshBindings(false);
			}
		}
	}

	public ItemStack EquippedStack
	{
		get
		{
			if (this.compareStack.IsEmpty())
			{
				ItemClassArmor itemClassArmor = this.itemClass as ItemClassArmor;
				if (itemClassArmor != null)
				{
					return base.xui.PlayerEquipment.GetStackFromSlot(itemClassArmor.EquipSlot);
				}
			}
			return this.compareStack;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemStack GetHoverControllerItemStack()
	{
		XUiC_ItemStack xuiC_ItemStack = this.hoverEntry as XUiC_ItemStack;
		if (xuiC_ItemStack != null)
		{
			return xuiC_ItemStack.ItemStack;
		}
		XUiC_EquipmentStack xuiC_EquipmentStack = this.hoverEntry as XUiC_EquipmentStack;
		if (xuiC_EquipmentStack != null)
		{
			return xuiC_EquipmentStack.ItemStack;
		}
		XUiC_BasePartStack xuiC_BasePartStack = this.hoverEntry as XUiC_BasePartStack;
		if (xuiC_BasePartStack != null)
		{
			return xuiC_BasePartStack.ItemStack;
		}
		XUiC_TraderItemEntry xuiC_TraderItemEntry = this.hoverEntry as XUiC_TraderItemEntry;
		if (xuiC_TraderItemEntry != null)
		{
			return xuiC_TraderItemEntry.Item;
		}
		XUiC_QuestTurnInEntry xuiC_QuestTurnInEntry = this.hoverEntry as XUiC_QuestTurnInEntry;
		if (xuiC_QuestTurnInEntry != null)
		{
			return xuiC_QuestTurnInEntry.Item;
		}
		return null;
	}

	public override void Init()
	{
		base.Init();
		this.itemPreview = base.GetChildById("itemPreview");
		this.mainActionItemList = (XUiC_ItemActionList)base.GetChildById("itemActions");
		this.traderActionItemList = (XUiC_ItemActionList)base.GetChildById("vendorItemActions");
		this.partList = (XUiC_PartList)base.GetChildById("parts");
		this.BuySellCounter = base.GetChildByType<XUiC_Counter>();
		if (this.BuySellCounter != null)
		{
			this.BuySellCounter.OnCountChanged += this.Counter_OnCountChanged;
			this.BuySellCounter.Count = 1;
		}
		this.statButton = base.GetChildById("statButton");
		this.statButton.OnPress += this.StatButton_OnPress;
		this.descriptionButton = base.GetChildById("descriptionButton");
		this.descriptionButton.OnPress += this.DescriptionButton_OnPress;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DescriptionButton_OnPress(XUiController _sender, int _mouseButton)
	{
		((XUiV_Button)this.statButton.ViewComponent).Selected = false;
		((XUiV_Button)this.descriptionButton.ViewComponent).Selected = true;
		this.showStats = false;
		this.IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void StatButton_OnPress(XUiController _sender, int _mouseButton)
	{
		((XUiV_Button)this.statButton.ViewComponent).Selected = true;
		((XUiV_Button)this.descriptionButton.ViewComponent).Selected = false;
		this.showStats = true;
		this.IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Counter_OnCountChanged(XUiController _sender, OnCountChangedEventArgs _e)
	{
		base.RefreshBindings(false);
		this.traderActionItemList.RefreshActionList();
	}

	public override void Deselect()
	{
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (this.IsDirty && base.ViewComponent.IsVisible)
		{
			if (this.emptyInfoWindow == null)
			{
				this.emptyInfoWindow = (XUiC_InfoWindow)base.xui.FindWindowGroupByName("backpack").GetChildById("emptyInfoPanel");
			}
			if (this.selectedItemStack != null)
			{
				this.SetItemStack(this.selectedItemStack, false);
			}
			else if (this.selectedEquipmentStack != null)
			{
				this.SetItemStack(this.selectedEquipmentStack, false);
			}
			else if (this.selectedPartStack != null)
			{
				this.SetItemStack(this.selectedPartStack, false);
			}
			else if (this.selectedTraderItemStack != null)
			{
				this.SetItemStack(this.selectedTraderItemStack, false);
			}
			else if (this.selectedTurnInItemStack != null)
			{
				this.SetItemStack(this.selectedTurnInItemStack, false);
			}
			this.IsDirty = false;
		}
	}

	public override bool GetBindingValue(ref string value, string bindingName)
	{
		uint num = <PrivateImplementationDetails>.ComputeStringHash(bindingName);
		if (num <= 2418997840U)
		{
			if (num <= 1585275412U)
			{
				if (num <= 1062608009U)
				{
					if (num <= 549600571U)
					{
						if (num != 297758672U)
						{
							if (num == 549600571U)
							{
								if (bindingName == "shownormaloptions")
								{
									value = (!this.isOpenAsTrader).ToString();
									return true;
								}
							}
						}
						else if (bindingName == "showstatoptions")
						{
							value = "false";
							return true;
						}
					}
					else if (num != 847165955U)
					{
						if (num != 1022877350U)
						{
							if (num == 1062608009U)
							{
								if (bindingName == "durabilitycolor")
								{
									Color32 v = Color.white;
									if (!this.itemStack.IsEmpty())
									{
										v = QualityInfo.GetTierColor((int)this.itemStack.itemValue.Quality);
									}
									value = this.durabilitycolorFormatter.Format(v);
									return true;
								}
							}
						}
						else if (bindingName == "durabilityjustify")
						{
							value = "center";
							if (!this.itemStack.IsEmpty() && this.itemClass != null && !this.itemClass.ShowQualityBar)
							{
								value = "right";
							}
							return true;
						}
					}
					else if (bindingName == "itemtypeicon")
					{
						value = "";
						if (!this.itemStack.IsEmpty() && this.itemClass != null)
						{
							if (this.itemClass.IsBlock())
							{
								value = Block.list[this.itemStack.itemValue.type].ItemTypeIcon;
							}
							else
							{
								if (this.itemClass.AltItemTypeIcon != null && this.itemClass.Unlocks != "" && XUiM_ItemStack.CheckKnown(base.xui.playerUI.entityPlayer, this.itemClass, this.itemStack.itemValue))
								{
									value = this.itemClass.AltItemTypeIcon;
									return true;
								}
								value = this.itemClass.ItemTypeIcon;
							}
						}
						return true;
					}
				}
				else if (num <= 1388578781U)
				{
					if (num != 1116089576U)
					{
						if (num == 1388578781U)
						{
							if (bindingName == "hasitemtypeicon")
							{
								value = "false";
								if (!this.itemStack.IsEmpty() && this.itemClass != null)
								{
									if (this.itemClass.IsBlock())
									{
										value = (Block.list[this.itemStack.itemValue.type].ItemTypeIcon != "").ToString();
									}
									else
									{
										value = (this.itemClass.ItemTypeIcon != "").ToString();
									}
								}
								return true;
							}
						}
					}
					else if (bindingName == "showonlydescription")
					{
						value = (!XUiM_ItemStack.HasItemStats(this.itemStack)).ToString();
						return true;
					}
				}
				else if (num != 1556795416U)
				{
					if (num != 1573573035U)
					{
						if (num == 1585275412U)
						{
							if (bindingName == "itemgroupicon")
							{
								value = "";
								if (this.itemClass != null && this.itemClass.Groups.Length != 0)
								{
									string key = this.itemClass.Groups[0];
									if (!XUiC_ItemInfoWindow.itemGroupToIcon.TryGetValue(key, out value))
									{
										value = XUiC_ItemInfoWindow.defaultItemGroupIcon;
									}
								}
								return true;
							}
						}
					}
					else if (bindingName == "itemstattitle3")
					{
						value = ((this.itemClass != null) ? this.GetStatTitle(2) : "");
						return true;
					}
				}
				else if (bindingName == "itemstattitle2")
				{
					value = ((this.itemClass != null) ? this.GetStatTitle(1) : "");
					return true;
				}
			}
			else if (num <= 1674238749U)
			{
				if (num <= 1623905892U)
				{
					if (num != 1607128273U)
					{
						if (num == 1623905892U)
						{
							if (bindingName == "itemstattitle6")
							{
								value = ((this.itemClass != null) ? this.GetStatTitle(5) : "");
								return true;
							}
						}
					}
					else if (bindingName == "itemstattitle1")
					{
						value = ((this.itemClass != null) ? this.GetStatTitle(0) : "");
						return true;
					}
				}
				else if (num != 1640683511U)
				{
					if (num != 1657461130U)
					{
						if (num == 1674238749U)
						{
							if (bindingName == "itemstattitle5")
							{
								value = ((this.itemClass != null) ? this.GetStatTitle(4) : "");
								return true;
							}
						}
					}
					else if (bindingName == "itemstattitle4")
					{
						value = ((this.itemClass != null) ? this.GetStatTitle(3) : "");
						return true;
					}
				}
				else if (bindingName == "itemstattitle7")
				{
					value = ((this.itemClass != null) ? this.GetStatTitle(6) : "");
					return true;
				}
			}
			else if (num <= 1953932597U)
			{
				if (num != 1823975127U)
				{
					if (num == 1953932597U)
					{
						if (bindingName == "durabilitytext")
						{
							value = "";
							if (!this.itemStack.IsEmpty() && this.itemClass != null)
							{
								if (this.itemClass.ShowQualityBar)
								{
									value = ((this.itemStack.itemValue.Quality > 0) ? this.durabilitytextFormatter.Format((int)this.itemStack.itemValue.Quality) : "-");
								}
								else
								{
									value = ((this.itemClass.Stacknumber == 1) ? "" : this.durabilitytextFormatter.Format(this.itemStack.count));
								}
							}
							return true;
						}
					}
				}
				else if (bindingName == "itemcost")
				{
					value = "";
					if (this.itemClass != null)
					{
						bool sellableToTrader;
						if (this.itemClass.IsBlock())
						{
							sellableToTrader = Block.list[this.itemStack.itemValue.type].SellableToTrader;
						}
						else
						{
							sellableToTrader = this.itemClass.SellableToTrader;
						}
						if (!sellableToTrader && !this.isBuying)
						{
							value = Localization.Get("xuiNoSellPrice", false);
							return true;
						}
						int count = this.itemStack.count;
						if (this.isOpenAsTrader)
						{
							count = this.BuySellCounter.Count;
						}
						if (this.isBuying)
						{
							if (this.useCustomMarkup)
							{
								value = this.itemcostFormatter.Format(XUiM_Trader.GetBuyPrice(base.xui, this.itemStack.itemValue, count, this.itemClass, this.selectedTraderItemStack.SlotIndex));
								return true;
							}
							value = this.itemcostFormatter.Format(XUiM_Trader.GetBuyPrice(base.xui, this.itemStack.itemValue, count, this.itemClass, -1));
						}
						else
						{
							int sellPrice = XUiM_Trader.GetSellPrice(base.xui, this.itemStack.itemValue, count, this.itemClass);
							value = ((sellPrice > 0) ? this.itemcostFormatter.Format(sellPrice) : Localization.Get("xuiNoSellPrice", false));
						}
					}
					return true;
				}
			}
			else if (num != 2198780406U)
			{
				if (num != 2269953035U)
				{
					if (num == 2418997840U)
					{
						if (bindingName == "pricelabel")
						{
							value = "";
							if (this.itemClass != null)
							{
								bool sellableToTrader2;
								if (this.itemClass.IsBlock())
								{
									sellableToTrader2 = Block.list[this.itemStack.itemValue.type].SellableToTrader;
								}
								else
								{
									sellableToTrader2 = this.itemClass.SellableToTrader;
								}
								if (!sellableToTrader2)
								{
									return true;
								}
								int count2 = this.itemStack.count;
								if (this.isOpenAsTrader)
								{
									count2 = this.BuySellCounter.Count;
								}
								if (this.isBuying)
								{
									value = ((XUiM_Trader.GetBuyPrice(base.xui, this.itemStack.itemValue, count2, this.itemClass, -1) > 0) ? Localization.Get("xuiBuyPrice", false) : "");
								}
								else
								{
									value = ((XUiM_Trader.GetSellPrice(base.xui, this.itemStack.itemValue, count2, this.itemClass) > 0) ? Localization.Get("xuiSellPrice", false) : "");
								}
							}
							return true;
						}
					}
				}
				else if (bindingName == "showstatanddescription")
				{
					value = XUiM_ItemStack.HasItemStats(this.itemStack).ToString();
					return true;
				}
			}
			else if (bindingName == "showtraderoptions")
			{
				value = this.isOpenAsTrader.ToString();
				return true;
			}
		}
		else if (num <= 3926148983U)
		{
			if (num <= 3154262838U)
			{
				if (num <= 2985969251U)
				{
					if (num != 2944858628U)
					{
						if (num == 2985969251U)
						{
							if (bindingName == "itemammoname")
							{
								value = "";
								if (this.itemClass != null)
								{
									ItemActionRanged itemActionRanged = this.itemClass.Actions[0] as ItemActionRanged;
									if (itemActionRanged != null)
									{
										if (itemActionRanged.MagazineItemNames.Length > 1)
										{
											ItemClass itemClass = ItemClass.GetItemClass(itemActionRanged.MagazineItemNames[(int)this.itemStack.itemValue.SelectedAmmoTypeIndex], false);
											value = itemClass.GetLocalizedItemName();
										}
									}
									else
									{
										ItemActionLauncher itemActionLauncher = this.itemClass.Actions[0] as ItemActionLauncher;
										if (itemActionLauncher != null && itemActionLauncher.MagazineItemNames.Length > 1)
										{
											ItemClass itemClass2 = ItemClass.GetItemClass(itemActionLauncher.MagazineItemNames[(int)this.itemStack.itemValue.SelectedAmmoTypeIndex], false);
											value = itemClass2.GetLocalizedItemName();
										}
									}
								}
								return true;
							}
						}
					}
					else if (bindingName == "hasdurability")
					{
						value = (!this.itemStack.IsEmpty() && this.itemClass != null && this.itemClass.ShowQualityBar).ToString();
						return true;
					}
				}
				else if (num != 3077933458U)
				{
					if (num != 3122611245U)
					{
						if (num == 3154262838U)
						{
							if (bindingName == "showdescription")
							{
								value = (!this.showStats).ToString();
								return true;
							}
						}
					}
					else if (bindingName == "markup")
					{
						value = "";
						if (this.useCustomMarkup)
						{
							int v2 = base.xui.Trader.Trader.GetMarkupByIndex(this.selectedTraderItemStack.SlotIndex) * 20;
							value = this.markupFormatter.Format(v2);
						}
						return true;
					}
				}
				else if (bindingName == "isnotcomparing")
				{
					value = this.CompareStack.IsEmpty().ToString();
					return true;
				}
			}
			else if (num <= 3257770903U)
			{
				if (num != 3191456325U)
				{
					if (num == 3257770903U)
					{
						if (bindingName == "showstats")
						{
							value = this.showStats.ToString();
							return true;
						}
					}
				}
				else if (bindingName == "itemname")
				{
					value = ((this.itemClass != null) ? this.itemClass.GetLocalizedItemName() : "");
					return true;
				}
			}
			else if (num != 3262997624U)
			{
				if (num != 3708628627U)
				{
					if (num == 3926148983U)
					{
						if (bindingName == "iscomparing")
						{
							value = (!this.CompareStack.IsEmpty()).ToString();
							return true;
						}
					}
				}
				else if (bindingName == "itemicon")
				{
					if (this.itemStack != null)
					{
						value = this.itemStack.itemValue.GetPropertyOverride("CustomIcon", (this.itemClass != null) ? this.itemClass.GetIconName() : "");
					}
					else
					{
						value = "";
					}
					return true;
				}
			}
			else if (bindingName == "itemdescription")
			{
				value = "";
				if (this.itemClass != null)
				{
					if (this.itemClass.IsBlock())
					{
						string descriptionKey = Block.list[this.itemClass.Id].DescriptionKey;
						if (Localization.Exists(descriptionKey, false))
						{
							value = Localization.Get(descriptionKey, false);
						}
					}
					else
					{
						string descriptionKey2 = this.itemClass.DescriptionKey;
						if (Localization.Exists(descriptionKey2, false))
						{
							value = Localization.Get(descriptionKey2, false);
						}
						if (this.itemClass.Unlocks != "")
						{
							ItemClass itemClass3 = ItemClass.GetItemClass(this.itemClass.Unlocks, false);
							if (itemClass3 != null)
							{
								value = value + "\n\n" + Localization.Get(itemClass3.DescriptionKey, false);
							}
						}
					}
				}
				return true;
			}
		}
		else if (num <= 4172540779U)
		{
			if (num <= 4053908414U)
			{
				if (num != 4049247086U)
				{
					if (num == 4053908414U)
					{
						if (bindingName == "itemicontint")
						{
							Color32 v3 = Color.white;
							if (this.itemClass != null)
							{
								v3 = this.itemClass.GetIconTint(this.itemStack.itemValue);
							}
							value = this.itemicontintcolorFormatter.Format(v3);
							return true;
						}
					}
				}
				else if (bindingName == "itemtypeicontint")
				{
					value = "255,255,255,255";
					if (!this.itemStack.IsEmpty() && this.itemClass != null && this.itemClass.Unlocks != "" && XUiM_ItemStack.CheckKnown(base.xui.playerUI.entityPlayer, this.itemClass, this.itemStack.itemValue))
					{
						value = this.altitemtypeiconcolorFormatter.Format(this.itemClass.AltItemTypeIconColor);
					}
					return true;
				}
			}
			else if (num != 4140647608U)
			{
				if (num != 4157425227U)
				{
					if (num == 4172540779U)
					{
						if (bindingName == "durabilityfill")
						{
							value = ((!this.itemStack.IsEmpty()) ? ((this.itemStack.itemValue.MaxUseTimes == 0) ? "1" : this.durabilityfillFormatter.Format(((float)this.itemStack.itemValue.MaxUseTimes - this.itemStack.itemValue.UseTimes) / (float)this.itemStack.itemValue.MaxUseTimes)) : "0");
							return true;
						}
					}
				}
				else if (bindingName == "itemstat5")
				{
					value = ((this.itemClass != null) ? this.GetStatValue(4) : "");
					return true;
				}
			}
			else if (bindingName == "itemstat4")
			{
				value = ((this.itemClass != null) ? this.GetStatValue(3) : "");
				return true;
			}
		}
		else if (num <= 4190980465U)
		{
			if (num != 4174202846U)
			{
				if (num == 4190980465U)
				{
					if (bindingName == "itemstat7")
					{
						value = ((this.itemClass != null) ? this.GetStatValue(6) : "");
						return true;
					}
				}
			}
			else if (bindingName == "itemstat6")
			{
				value = ((this.itemClass != null) ? this.GetStatValue(5) : "");
				return true;
			}
		}
		else if (num != 4224535703U)
		{
			if (num != 4241313322U)
			{
				if (num == 4258090941U)
				{
					if (bindingName == "itemstat3")
					{
						value = ((this.itemClass != null) ? this.GetStatValue(2) : "");
						return true;
					}
				}
			}
			else if (bindingName == "itemstat2")
			{
				value = ((this.itemClass != null) ? this.GetStatValue(1) : "");
				return true;
			}
		}
		else if (bindingName == "itemstat1")
		{
			value = ((this.itemClass != null) ? this.GetStatValue(0) : "");
			return true;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string GetStatTitle(int index)
	{
		if (this.itemDisplayEntry == null || this.itemDisplayEntry.DisplayStats.Count <= index)
		{
			return "";
		}
		if (this.itemDisplayEntry.DisplayStats[index].TitleOverride != null)
		{
			return this.itemDisplayEntry.DisplayStats[index].TitleOverride;
		}
		return UIDisplayInfoManager.Current.GetLocalizedName(this.itemDisplayEntry.DisplayStats[index].StatType);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string GetStatValue(int index)
	{
		if (this.itemDisplayEntry == null || this.itemDisplayEntry.DisplayStats.Count <= index)
		{
			return "";
		}
		DisplayInfoEntry infoEntry = this.itemDisplayEntry.DisplayStats[index];
		if (!this.CompareStack.IsEmpty())
		{
			return XUiM_ItemStack.GetStatItemValueTextWithCompareInfo(this.itemStack, this.CompareStack, base.xui.playerUI.entityPlayer, infoEntry, false, false);
		}
		if (!this.EquippedStack.IsEmpty())
		{
			return XUiM_ItemStack.GetStatItemValueTextWithCompareInfo(this.itemStack, this.EquippedStack, base.xui.playerUI.entityPlayer, infoEntry, true, false);
		}
		return XUiM_ItemStack.GetStatItemValueTextWithCompareInfo(this.itemStack, this.CompareStack, base.xui.playerUI.entityPlayer, infoEntry, false, true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void makeVisible(bool _makeVisible)
	{
		if (!_makeVisible)
		{
			return;
		}
		if (!this.windowGroup.isShowing)
		{
			return;
		}
		base.ViewComponent.IsVisible = true;
		((XUiV_Window)this.viewComponent).ForceVisible(1f);
	}

	public void SetItemStack(XUiC_ItemStack stack, bool _makeVisible = false)
	{
		if (stack == null || stack.ItemStack.IsEmpty())
		{
			this.ShowEmptyInfo();
			return;
		}
		this.makeVisible(_makeVisible);
		this.selectedEquipmentStack = null;
		this.selectedItemStack = stack;
		this.selectedPartStack = null;
		this.selectedTraderItemStack = null;
		this.selectedTurnInItemStack = null;
		this.SetInfo(stack.ItemStack, stack, XUiC_ItemActionList.ItemActionListTypes.Item);
	}

	public void SetItemStack(XUiC_EquipmentStack stack, bool _makeVisible = false)
	{
		if (stack == null || stack.ItemStack.IsEmpty())
		{
			this.ShowEmptyInfo();
			return;
		}
		this.makeVisible(_makeVisible);
		this.selectedItemStack = null;
		this.selectedEquipmentStack = stack;
		this.selectedPartStack = null;
		this.selectedTraderItemStack = null;
		this.selectedTurnInItemStack = null;
		this.SetInfo(stack.ItemStack, stack, XUiC_ItemActionList.ItemActionListTypes.Equipment);
	}

	public void SetItemStack(XUiC_BasePartStack stack, bool _makeVisible = false)
	{
		if (stack == null || stack.ItemStack.IsEmpty())
		{
			this.ShowEmptyInfo();
			return;
		}
		this.makeVisible(_makeVisible);
		this.selectedItemStack = null;
		this.selectedEquipmentStack = null;
		this.selectedPartStack = stack;
		this.selectedTraderItemStack = null;
		this.selectedTurnInItemStack = null;
		this.SetInfo(stack.ItemStack, stack, XUiC_ItemActionList.ItemActionListTypes.Part);
	}

	public void SetItemStack(XUiC_TraderItemEntry stack, bool _makeVisible = false)
	{
		if (stack == null || stack.Item == null || stack.Item.IsEmpty())
		{
			this.ShowEmptyInfo();
			return;
		}
		this.makeVisible(_makeVisible);
		this.selectedItemStack = null;
		this.selectedEquipmentStack = null;
		this.selectedPartStack = null;
		this.selectedTraderItemStack = stack;
		this.selectedTurnInItemStack = null;
		this.SetInfo(stack.Item, stack, XUiC_ItemActionList.ItemActionListTypes.Trader);
	}

	public void SetItemStack(XUiC_QuestTurnInEntry stack, bool _makeVisible = false)
	{
		if (stack == null || stack.Item == null || stack.Item.IsEmpty())
		{
			this.ShowEmptyInfo();
			return;
		}
		this.makeVisible(_makeVisible);
		this.selectedItemStack = null;
		this.selectedEquipmentStack = null;
		this.selectedPartStack = null;
		this.selectedTraderItemStack = null;
		this.selectedTurnInItemStack = stack;
		this.SetInfo(stack.Item, stack, XUiC_ItemActionList.ItemActionListTypes.QuestReward);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ShowEmptyInfo()
	{
		if (this.emptyInfoWindow == null)
		{
			this.emptyInfoWindow = (XUiC_InfoWindow)base.xui.FindWindowGroupByName("backpack").GetChildById("emptyInfoPanel");
		}
		this.emptyInfoWindow.ViewComponent.IsVisible = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetInfo(ItemStack stack, XUiController controller, XUiC_ItemActionList.ItemActionListTypes actionListType)
	{
		bool flag = stack.itemValue.type == this.itemStack.itemValue.type && stack.count == this.itemStack.count;
		this.itemStack = stack.Clone();
		bool flag2 = this.itemStack != null && !this.itemStack.IsEmpty();
		if (this.itemPreview == null)
		{
			return;
		}
		if (!flag || !stack.itemValue.Equals(this.itemStack.itemValue))
		{
			this.compareStack = ItemStack.Empty.Clone();
		}
		this.itemClass = null;
		int num = 1;
		if (flag2)
		{
			this.itemClass = this.itemStack.itemValue.ItemClassOrMissing;
			if (this.itemClass != null)
			{
				if (this.itemClass is ItemClassQuest)
				{
					this.itemClass = ItemClassQuest.GetItemQuestById(this.itemStack.itemValue.Seed);
				}
				num = (this.itemClass.IsBlock() ? Block.list[this.itemStack.itemValue.type].EconomicBundleSize : this.itemClass.EconomicBundleSize);
			}
		}
		if (this.itemClass != null)
		{
			this.itemDisplayEntry = UIDisplayInfoManager.Current.GetDisplayStatsForTag(this.itemClass.IsBlock() ? Block.list[this.itemStack.itemValue.type].DisplayType : this.itemClass.DisplayType);
		}
		if (this.isOpenAsTrader)
		{
			this.isBuying = (actionListType == XUiC_ItemActionList.ItemActionListTypes.Trader);
			this.useCustomMarkup = (this.selectedTraderItemStack != null && base.xui.Trader.TraderTileEntity is TileEntityVendingMachine && (base.xui.Trader.Trader.TraderInfo.PlayerOwned || base.xui.Trader.Trader.TraderInfo.Rentable));
			this.traderActionItemList.SetCraftingActionList(actionListType, controller);
			int count = this.BuySellCounter.Count;
			if (!flag)
			{
				this.BuySellCounter.Count = ((this.itemStack.count < num) ? 0 : num);
			}
			else if (count > this.itemStack.count)
			{
				this.BuySellCounter.Count = ((this.itemStack.count < num) ? 0 : this.itemStack.count);
			}
			int num2 = this.isBuying ? Math.Min(this.itemStack.count, base.xui.PlayerInventory.CountAvailabileSpaceForItem(this.itemStack.itemValue)) : this.itemStack.count;
			this.BuySellCounter.MaxCount = num2 / num * num;
			this.BuySellCounter.Step = num;
			if (this.BuySellCounter.Count == 0 && this.itemStack.count >= num)
			{
				this.BuySellCounter.Count = num;
			}
			if (this.SetMaxCountOnDirty)
			{
				this.BuySellCounter.Count = this.BuySellCounter.MaxCount;
				this.SetMaxCountOnDirty = false;
			}
			this.BuySellCounter.ForceTextRefresh();
		}
		else
		{
			this.mainActionItemList.SetCraftingActionList(actionListType, controller);
			this.isBuying = false;
			this.useCustomMarkup = false;
		}
		if (flag2 && this.itemStack.itemValue.Modifications != null)
		{
			this.partList.SetMainItem(this.itemStack);
			if (this.itemStack.itemValue.CosmeticMods != null && this.itemStack.itemValue.CosmeticMods.Length != 0 && this.itemStack.itemValue.CosmeticMods[0] != null && !this.itemStack.itemValue.CosmeticMods[0].IsEmpty())
			{
				this.partList.SetSlot(this.itemStack.itemValue.CosmeticMods[0], 0);
				this.partList.SetSlots(this.itemStack.itemValue.Modifications, 1);
			}
			else
			{
				this.partList.SetSlots(this.itemStack.itemValue.Modifications, 0);
			}
			this.partList.ViewComponent.IsVisible = true;
		}
		else
		{
			this.partList.ViewComponent.IsVisible = false;
		}
		base.RefreshBindings(false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemStack itemStack = ItemStack.Empty.Clone();

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemClass itemClass;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ItemStack selectedItemStack;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_EquipmentStack selectedEquipmentStack;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_BasePartStack selectedPartStack;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TraderItemEntry selectedTraderItemStack;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_QuestTurnInEntry selectedTurnInItemStack;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController itemPreview;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ItemActionList mainActionItemList;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ItemActionList traderActionItemList;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_PartList partList;

	public XUiC_Counter BuySellCounter;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController statButton;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController descriptionButton;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_InfoWindow emptyInfoWindow;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isBuying;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool useCustomMarkup;

	public bool SetMaxCountOnDirty;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemDisplayEntry itemDisplayEntry;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SelectableEntry hoverEntry;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemStack compareStack = ItemStack.Empty;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool showStats = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterInt itemcostFormatter = new CachedStringFormatterInt();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<int> markupFormatter = new CachedStringFormatter<int>(delegate(int _i)
	{
		if (_i > 0)
		{
			return string.Format(" (+{0}%)", _i);
		}
		if (_i >= 0)
		{
			return "";
		}
		return string.Format(" ({0}%)", _i);
	});

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterXuiRgbaColor itemicontintcolorFormatter = new CachedStringFormatterXuiRgbaColor();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterXuiRgbaColor durabilitycolorFormatter = new CachedStringFormatterXuiRgbaColor();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterFloat durabilityfillFormatter = new CachedStringFormatterFloat(null);

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterInt durabilitytextFormatter = new CachedStringFormatterInt();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterXuiRgbaColor altitemtypeiconcolorFormatter = new CachedStringFormatterXuiRgbaColor();

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Dictionary<string, string> itemGroupToIcon = new CaseInsensitiveStringDictionary<string>
	{
		{
			"basics",
			"ui_game_symbol_campfire"
		},
		{
			"building",
			"ui_game_symbol_map_house"
		},
		{
			"resources",
			"ui_game_symbol_resource"
		},
		{
			"ammo/weapons",
			"ui_game_symbol_knife"
		},
		{
			"tools/traps",
			"ui_game_symbol_tool"
		},
		{
			"food/cooking",
			"ui_game_symbol_fork"
		},
		{
			"medicine",
			"ui_game_symbol_medical"
		},
		{
			"clothing",
			"ui_game_symbol_shirt"
		},
		{
			"decor/miscellaneous",
			"ui_game_symbol_chair"
		},
		{
			"books",
			"ui_game_symbol_book"
		},
		{
			"chemicals",
			"ui_game_symbol_water"
		},
		{
			"mods",
			"ui_game_symbol_assemble"
		}
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly string defaultItemGroupIcon = "ui_game_symbol_campfire";
}
