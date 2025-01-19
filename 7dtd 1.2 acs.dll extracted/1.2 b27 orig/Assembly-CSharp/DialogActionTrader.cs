using System;
using UnityEngine.Scripting;

[Preserve]
public class DialogActionTrader : BaseDialogAction
{
	public override BaseDialogAction.ActionTypes ActionType
	{
		get
		{
			return BaseDialogAction.ActionTypes.Trader;
		}
	}

	public override void PerformAction(EntityPlayer player)
	{
		EntityNPC respondent = LocalPlayerUI.GetUIForPlayer(player as EntityPlayerLocal).xui.Dialog.Respondent;
		if (respondent != null)
		{
			if (base.ID.EqualsCaseInsensitive("restock"))
			{
				(respondent as EntityTrader).TileEntityTrader.TraderData.lastInventoryUpdate = 0UL;
				return;
			}
			if (base.ID.EqualsCaseInsensitive("trade"))
			{
				(respondent as EntityTrader).StartTrading(player);
				return;
			}
			if (base.ID.EqualsCaseInsensitive("reset_quests"))
			{
				if (respondent is EntityTrader)
				{
					(respondent as EntityTrader).ClearActiveQuests(player.entityId);
					return;
				}
			}
			else
			{
				if (base.ID.EqualsCaseInsensitive("drone_storage"))
				{
					(respondent as EntityDrone).OpenStorage(player);
					return;
				}
				if (base.ID.EqualsCaseInsensitive("drone_follow"))
				{
					(respondent as EntityDrone).FollowMode();
					return;
				}
				if (base.ID.EqualsCaseInsensitive("drone_sentry"))
				{
					(respondent as EntityDrone).SentryMode();
					return;
				}
				if (base.ID.EqualsCaseInsensitive("drone_heal"))
				{
					(respondent as EntityDrone).HealOwner();
					return;
				}
				if (base.ID.EqualsCaseInsensitive("drone_dont_heal_allies") || base.ID.EqualsCaseInsensitive("drone_heal_allies"))
				{
					(respondent as EntityDrone).ToggleHealAllies();
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string name = "";
}
