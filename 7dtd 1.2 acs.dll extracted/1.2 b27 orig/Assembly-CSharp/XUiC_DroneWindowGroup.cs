﻿using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_DroneWindowGroup : XUiController
{
	public EntityDrone CurrentVehicleEntity
	{
		get
		{
			return this.currentVehicleEntity;
		}
		set
		{
			this.currentVehicleEntity = value;
			this.assembleWindow.group = this;
			ItemValue updatedItemValue = this.currentVehicleEntity.GetUpdatedItemValue();
			ItemStack itemStack = new ItemStack(updatedItemValue, 1);
			this.assembleWindow.ItemStack = itemStack;
			this.grid.AssembleWindow = this.assembleWindow;
			this.grid.CurrentItem = itemStack;
			this.grid.SetParts(updatedItemValue.Modifications);
			this.cosmeticGrid.AssembleWindow = this.assembleWindow;
			this.cosmeticGrid.CurrentItem = itemStack;
			this.cosmeticGrid.SetParts(updatedItemValue.CosmeticMods);
			XUiM_AssembleItem assembleItem = base.xui.AssembleItem;
			assembleItem.AssembleWindow = this.assembleWindow;
			assembleItem.CurrentItem = itemStack;
			assembleItem.CurrentItemStackController = null;
		}
	}

	public override void Init()
	{
		base.Init();
		this.nonPagingHeaderWindow = base.GetChildByType<XUiC_WindowNonPagingHeader>();
		this.assembleWindow = base.GetChildByType<XUiC_AssembleDroneWindow>();
		this.grid = base.GetChildByType<XUiC_ItemDronePartStackGrid>();
		this.cosmeticGrid = base.GetChildByType<XUiC_ItemCosmeticStackGrid>();
	}

	public override void OnOpen()
	{
		base.OnOpen();
		GUIWindowManager windowManager = base.xui.playerUI.windowManager;
		if (this.nonPagingHeaderWindow != null)
		{
			this.nonPagingHeaderWindow.SetHeader(Localization.Get("lblContextActionModify", false));
		}
		windowManager.CloseIfOpen("windowpaging");
		XUiC_ItemInfoWindow childByType = base.xui.GetChildByType<XUiC_ItemInfoWindow>();
		if (base.xui.AssembleItem.CurrentItemStackController != null)
		{
			base.xui.AssembleItem.CurrentItemStackController.Selected = true;
			childByType.SetItemStack(base.xui.AssembleItem.CurrentItemStackController, true);
			return;
		}
		if (base.xui.AssembleItem.CurrentEquipmentStackController != null)
		{
			base.xui.AssembleItem.CurrentEquipmentStackController.Selected = false;
			childByType.SetItemStack(base.xui.AssembleItem.CurrentEquipmentStackController, true);
		}
	}

	public override void OnClose()
	{
		base.OnClose();
		XUiM_AssembleItem assembleItem = base.xui.AssembleItem;
		assembleItem.AssembleWindow = null;
		if (assembleItem.CurrentItem.itemValue == this.CurrentVehicleEntity.GetUpdatedItemValue())
		{
			assembleItem.CurrentItem = null;
			assembleItem.CurrentItemStackController = null;
		}
		this.CurrentVehicleEntity.StopUIInteraction();
	}

	public void OnItemChanged(ItemStack itemStack)
	{
		this.grid.CurrentItem = itemStack;
		this.grid.SetParts(itemStack.itemValue.Modifications);
		this.cosmeticGrid.CurrentItem = itemStack;
		this.cosmeticGrid.SetParts(itemStack.itemValue.CosmeticMods);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_AssembleDroneWindow assembleWindow;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_WindowNonPagingHeader nonPagingHeaderWindow;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ItemDronePartStackGrid grid;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ItemCosmeticStackGrid cosmeticGrid;

	public static string ID = "junkDrone";

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityDrone currentVehicleEntity;
}
