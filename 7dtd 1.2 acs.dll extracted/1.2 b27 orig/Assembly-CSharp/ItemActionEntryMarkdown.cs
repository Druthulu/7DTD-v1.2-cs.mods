using System;
using Audio;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class ItemActionEntryMarkdown : BaseItemActionEntry
{
	public ItemActionEntryMarkdown(XUiController controller) : base(controller, "", "ui_game_symbol_subtract", BaseItemActionEntry.GamepadShortCut.DPadLeft, "", "ui/ui_denied")
	{
		base.ActionName = string.Format(Localization.Get("lblContextActionPrice", false), 20);
		if (controller.xui.Trader.TraderTileEntity is TileEntityVendingMachine)
		{
			bool playerOwned = controller.xui.Trader.Trader.TraderInfo.PlayerOwned;
			bool rentable = controller.xui.Trader.Trader.TraderInfo.Rentable;
			TileEntityVendingMachine tileEntityVendingMachine = controller.xui.Trader.TraderTileEntity as TileEntityVendingMachine;
			this.isOwner = ((playerOwned || rentable) && tileEntityVendingMachine.IsUserAllowed(PlatformManager.InternalLocalUserIdentifier));
			return;
		}
		this.isOwner = false;
	}

	public override void OnActivated()
	{
		XUiC_TraderItemEntry xuiC_TraderItemEntry = (XUiC_TraderItemEntry)base.ItemController;
		base.ItemController.xui.Trader.Trader.DecreaseMarkup(xuiC_TraderItemEntry.SlotIndex);
		xuiC_TraderItemEntry.InfoWindow.RefreshBindings(false);
		base.ItemController.xui.Trader.TraderWindowGroup.RefreshTraderItems();
		Manager.PlayInsidePlayerHead("ui_tab", -1, 0f, false, false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isOwner;
}
