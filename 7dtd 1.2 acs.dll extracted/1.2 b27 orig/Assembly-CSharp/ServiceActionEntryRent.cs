using System;
using UnityEngine.Scripting;

[Preserve]
public class ServiceActionEntryRent : BaseItemActionEntry
{
	public ServiceActionEntryRent(XUiController controller, TileEntityVendingMachine _vending) : base(controller, "lblContextActionRent", "ui_game_symbol_coin", BaseItemActionEntry.GamepadShortCut.None, "crafting/craft_click_craft", "ui/ui_denied")
	{
		this.vending = _vending;
	}

	public override void RefreshEnabled()
	{
		base.Enabled = (this.vending.CanRent() == TileEntityVendingMachine.RentResult.Allowed);
	}

	public override void OnDisabledActivate()
	{
		EntityPlayerLocal entityPlayer = base.ItemController.xui.playerUI.entityPlayer;
		switch (this.vending.CanRent())
		{
		case TileEntityVendingMachine.RentResult.AlreadyRented:
			GameManager.ShowTooltip(entityPlayer, Localization.Get("ttVMAlreadyRented", false), false);
			return;
		case TileEntityVendingMachine.RentResult.AlreadyRentingVM:
			GameManager.ShowTooltip(entityPlayer, Localization.Get("ttAlreadyRentingVM", false), false);
			return;
		case TileEntityVendingMachine.RentResult.NotEnoughMoney:
			if (this.vending.LocalPlayerIsOwner())
			{
				GameManager.ShowTooltip(entityPlayer, Localization.Get("ttVMNotEnoughMoneyAddTime", false), false);
				return;
			}
			GameManager.ShowTooltip(entityPlayer, Localization.Get("ttVMNotEnoughMoneyRent", false), false);
			return;
		default:
			return;
		}
	}

	public override void OnActivated()
	{
		if (this.vending.Rent())
		{
			((XUiC_TraderWindow)base.ItemController).Refresh();
		}
		this.RefreshEnabled();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public TileEntityVendingMachine vending;
}
