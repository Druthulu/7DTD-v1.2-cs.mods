using System;
using Audio;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class ItemActionEntryResetPrice : BaseItemActionEntry
{
	public ItemActionEntryResetPrice(XUiController controller) : base(controller, "lblContextActionReset", "ui_game_symbol_coin", BaseItemActionEntry.GamepadShortCut.None, "", "ui/ui_denied")
	{
		TileEntityVendingMachine tileEntityVendingMachine = controller.xui.Trader.TraderTileEntity as TileEntityVendingMachine;
		if (tileEntityVendingMachine != null)
		{
			bool playerOwned = controller.xui.Trader.Trader.TraderInfo.PlayerOwned;
			bool rentable = controller.xui.Trader.Trader.TraderInfo.Rentable;
			this.isOwner = ((playerOwned || rentable) && tileEntityVendingMachine.IsUserAllowed(PlatformManager.InternalLocalUserIdentifier));
			return;
		}
		this.isOwner = false;
	}

	public override void OnActivated()
	{
		XUiC_TraderItemEntry xuiC_TraderItemEntry = (XUiC_TraderItemEntry)base.ItemController;
		base.ItemController.xui.Trader.Trader.ResetMarkup(xuiC_TraderItemEntry.SlotIndex);
		xuiC_TraderItemEntry.InfoWindow.RefreshBindings(false);
		base.ItemController.xui.Trader.TraderWindowGroup.RefreshTraderItems();
		Manager.PlayInsidePlayerHead("ui_tab", -1, 0f, false, false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isOwner;
}
