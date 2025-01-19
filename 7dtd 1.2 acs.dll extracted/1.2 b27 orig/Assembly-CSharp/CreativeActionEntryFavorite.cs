using System;
using UnityEngine.Scripting;

[Preserve]
public class CreativeActionEntryFavorite : BaseItemActionEntry
{
	public CreativeActionEntryFavorite(XUiController controller, int stackID) : base(controller, "lblContextActionFavorite", "server_favorite", BaseItemActionEntry.GamepadShortCut.DPadRight, "crafting/craft_click_craft", "ui/ui_denied")
	{
		this.StackID = (ushort)stackID;
	}

	public override void OnActivated()
	{
		EntityPlayer entityPlayer = base.ItemController.xui.playerUI.entityPlayer;
		if (entityPlayer.favoriteCreativeStacks.Contains(this.StackID))
		{
			entityPlayer.favoriteCreativeStacks.Remove(this.StackID);
		}
		else
		{
			entityPlayer.favoriteCreativeStacks.Add(this.StackID);
		}
		XUiC_Creative2Window childByType = base.ItemController.WindowGroup.Controller.GetChildByType<XUiC_Creative2Window>();
		if (childByType == null)
		{
			return;
		}
		childByType.RefreshView();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public ushort StackID;
}
