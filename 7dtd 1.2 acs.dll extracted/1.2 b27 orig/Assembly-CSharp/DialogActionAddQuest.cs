using System;
using UnityEngine.Scripting;

[Preserve]
public class DialogActionAddQuest : BaseDialogAction
{
	public override BaseDialogAction.ActionTypes ActionType
	{
		get
		{
			return BaseDialogAction.ActionTypes.AddQuest;
		}
	}

	public override void PerformAction(EntityPlayer player)
	{
		if (this.Quest != null)
		{
			QuestClass questClass = this.Quest.QuestClass;
			if (questClass != null)
			{
				Quest quest = player.QuestJournal.FindNonSharedQuest(this.Quest.QuestCode);
				if (quest == null || (questClass.Repeatable && !quest.Active))
				{
					LocalPlayerUI playerUI = LocalPlayerUI.GetUIForPlayer(player as EntityPlayerLocal);
					XUiC_QuestOfferWindow.OpenQuestOfferWindow(playerUI.xui, this.Quest, this.ListIndex, XUiC_QuestOfferWindow.OfferTypes.Dialog, playerUI.xui.Dialog.Respondent.entityId, delegate(EntityNPC npc)
					{
						playerUI.xui.Dialog.Respondent = npc;
						playerUI.xui.Dialog.ReturnStatement = ((DialogResponseQuest)this.Owner).LastStatementID;
						playerUI.windowManager.Open("dialog", true, false, true);
					});
					return;
				}
				GameManager.ShowTooltip((EntityPlayerLocal)player, Localization.Get("questunavailable", false), false);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string name = "";

	public Quest Quest;

	public int ListIndex;
}
