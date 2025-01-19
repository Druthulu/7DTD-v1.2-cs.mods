using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_AssembleWindowGroup : XUiController
{
	public ItemStack ItemStack
	{
		get
		{
			return this.itemStack;
		}
		set
		{
			this.itemStack = value;
			this.assembleWindow.ItemStack = value;
			this.grid.CurrentItem = value;
			this.grid.SetParts(this.itemStack.itemValue.Modifications);
			this.grid.AssembleWindow = this.assembleWindow;
			this.cosmeticGrid.CurrentItem = value;
			this.cosmeticGrid.SetParts(this.itemStack.itemValue.CosmeticMods);
			this.cosmeticGrid.AssembleWindow = this.assembleWindow;
		}
	}

	public override void Init()
	{
		base.Init();
		this.assembleWindow = base.GetChildByType<XUiC_AssembleWindow>();
		this.grid = base.GetChildByType<XUiC_ItemPartStackGrid>();
		this.cosmeticGrid = base.GetChildByType<XUiC_ItemCosmeticStackGrid>();
		this.nonPagingHeaderWindow = base.GetChildByType<XUiC_WindowNonPagingHeader>();
	}

	public override void OnOpen()
	{
		this.ItemStack = base.xui.AssembleItem.CurrentItem;
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
			this.openEquipmentOnClose = false;
			return;
		}
		if (base.xui.AssembleItem.CurrentEquipmentStackController != null)
		{
			base.xui.AssembleItem.CurrentEquipmentStackController.Selected = false;
			childByType.SetItemStack(base.xui.AssembleItem.CurrentEquipmentStackController, true);
			this.openEquipmentOnClose = true;
		}
	}

	public override void OnClose()
	{
		base.OnClose();
		XUiM_AssembleItem assembleItem = base.xui.AssembleItem;
		assembleItem.CurrentItem = null;
		assembleItem.CurrentItemStackController = null;
		assembleItem.CurrentEquipmentStackController = null;
		GameManager.Instance.StartCoroutine(this.showCraftingLater());
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public IEnumerator showCraftingLater()
	{
		yield return new WaitForEndOfFrame();
		if (base.xui != null && base.xui.playerUI != null && base.xui.playerUI.entityPlayer != null)
		{
			XUiC_WindowSelector.OpenSelectorAndWindow(base.xui.playerUI.entityPlayer, this.openEquipmentOnClose ? "character" : "crafting");
			base.xui.GetChildByType<XUiC_WindowSelector>().OverrideClose = true;
		}
		yield break;
	}

	public static XUiC_AssembleWindowGroup GetWindowGroup(XUi _xuiInstance)
	{
		return _xuiInstance.FindWindowGroupByName(XUiC_AssembleWindowGroup.ID) as XUiC_AssembleWindowGroup;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_AssembleWindow assembleWindow;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ItemPartStackGrid grid;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ItemCosmeticStackGrid cosmeticGrid;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_WindowNonPagingHeader nonPagingHeaderWindow;

	public static string ID = "assemble";

	[PublicizedFrom(EAccessModifier.Private)]
	public bool openEquipmentOnClose;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemStack itemStack;
}
