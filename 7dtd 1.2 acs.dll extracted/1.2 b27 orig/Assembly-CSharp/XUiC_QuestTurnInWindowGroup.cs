using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_QuestTurnInWindowGroup : XUiController
{
	public override void Init()
	{
		base.Init();
		this.nonPagingHeaderWindow = base.GetChildByType<XUiC_WindowNonPagingHeader>();
		this.detailsWindow = base.GetChildByType<XUiC_QuestTurnInDetailsWindow>();
		this.rewardsWindow = base.GetChildByType<XUiC_QuestTurnInRewardsWindow>();
	}

	public override void OnOpen()
	{
		if (base.xui.Dialog.Respondent != null)
		{
			this.NPC = base.xui.Dialog.Respondent;
		}
		else
		{
			this.NPC = base.xui.Trader.TraderEntity;
		}
		this.detailsWindow.NPC = (this.rewardsWindow.NPC = this.NPC);
		XUiC_ItemInfoWindow childByType = base.xui.GetChildByType<XUiC_ItemInfoWindow>();
		this.rewardsWindow.InfoWindow = childByType;
		base.OnOpen();
		base.xui.playerUI.entityPlayer.OverrideFOV = 30f;
		base.xui.playerUI.entityPlayer.OverrideLookAt = this.NPC.getHeadPosition();
		GUIWindowManager windowManager = base.xui.playerUI.windowManager;
		if (this.nonPagingHeaderWindow != null)
		{
			this.nonPagingHeaderWindow.SetHeader("QUEST COMPLETE");
		}
		windowManager.CloseIfOpen("windowpaging");
		base.xui.dragAndDrop.InMenu = true;
	}

	public override void OnClose()
	{
		base.OnClose();
		EntityPlayerLocal entityPlayer = base.xui.playerUI.entityPlayer;
		if (GameManager.Instance.World == null)
		{
			return;
		}
		EntityTrader entityTrader = base.xui.Trader.TraderEntity as EntityTrader;
		if (entityTrader != null)
		{
			GameManager.Instance.StartCoroutine(this.startTrading(entityTrader, entityPlayer));
			return;
		}
		if (Vector3.Distance(base.xui.Dialog.Respondent.position, entityPlayer.position) > 5f)
		{
			base.xui.Dialog.Respondent = null;
			base.xui.playerUI.entityPlayer.OverrideFOV = -1f;
			return;
		}
		base.xui.playerUI.windowManager.Open("dialog", true, false, true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator startTrading(EntityTrader trader, EntityPlayer player)
	{
		yield return null;
		trader.StartTrading(player);
		yield break;
	}

	public void TryNextComplete()
	{
		Quest nextCompletedQuest = base.xui.playerUI.entityPlayer.QuestJournal.GetNextCompletedQuest(base.xui.Dialog.QuestTurnIn, this.NPC.entityId);
		if (nextCompletedQuest == null)
		{
			base.xui.playerUI.windowManager.CloseAllOpenWindows(null, false);
			return;
		}
		base.xui.Dialog.QuestTurnIn = nextCompletedQuest;
		this.detailsWindow.CurrentQuest = (this.rewardsWindow.CurrentQuest = nextCompletedQuest);
		base.RefreshBindings(false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_QuestTurnInDetailsWindow detailsWindow;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_QuestTurnInRewardsWindow rewardsWindow;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_WindowNonPagingHeader nonPagingHeaderWindow;

	public static string ID = "questTurnIn";

	public EntityNPC NPC;
}
