using System;
using System.Runtime.CompilerServices;
using GUI_2;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_QuestWindowGroup : XUiController
{
	public override void Init()
	{
		base.Init();
		this.objectivesWindow = base.GetChildByType<XUiC_QuestObjectivesWindow>();
		this.rewardsWindow = base.GetChildByType<XUiC_QuestRewardsWindow>();
		this.descriptionWindow = base.GetChildByType<XUiC_QuestDescriptionWindow>();
		XUiC_QuestListWindow childByType = base.GetChildByType<XUiC_QuestListWindow>();
		this.questList = ((childByType != null) ? childByType.GetChildByType<XUiC_QuestList>() : null);
		XUiC_QuestSharedListWindow childByType2 = base.GetChildByType<XUiC_QuestSharedListWindow>();
		this.sharedList = ((childByType2 != null) ? childByType2.GetChildByType<XUiC_QuestSharedList>() : null);
		if (this.questList != null)
		{
			this.questList.SharedList = this.sharedList;
		}
		if (this.sharedList != null)
		{
			this.sharedList.QuestList = this.questList;
		}
	}

	public void SetQuest(XUiC_QuestEntry q)
	{
		this.objectivesWindow.SetQuest(q);
		this.rewardsWindow.SetQuest(q);
		this.descriptionWindow.SetQuest(q);
	}

	public override void OnOpen()
	{
		base.OnOpen();
		base.xui.playerUI.windowManager.OpenIfNotOpen("windowpaging", false, false, true);
		base.xui.calloutWindow.ClearCallouts(XUiC_GamepadCalloutWindow.CalloutType.Menu);
		base.xui.calloutWindow.AddCallout(UIUtils.ButtonIcon.FaceButtonSouth, "igcoSelect", XUiC_GamepadCalloutWindow.CalloutType.Menu);
		base.xui.calloutWindow.AddCallout(UIUtils.ButtonIcon.FaceButtonEast, "igcoExit", XUiC_GamepadCalloutWindow.CalloutType.Menu);
		base.xui.calloutWindow.EnableCallouts(XUiC_GamepadCalloutWindow.CalloutType.Menu, 0f);
		XUiController xuiController = base.xui.FindWindowGroupByName("windowpaging");
		if (xuiController != null)
		{
			XUiC_WindowSelector childByType = xuiController.GetChildByType<XUiC_WindowSelector>();
			if (childByType != null)
			{
				childByType.SetSelected("quests");
			}
		}
		base.RefreshBindings(true);
		this.AsyncUISelectionOnOpen();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void AsyncUISelectionOnOpen()
	{
		XUiC_QuestWindowGroup.<AsyncUISelectionOnOpen>d__8 <AsyncUISelectionOnOpen>d__;
		<AsyncUISelectionOnOpen>d__.<>t__builder = AsyncVoidMethodBuilder.Create();
		<AsyncUISelectionOnOpen>d__.<>4__this = this;
		<AsyncUISelectionOnOpen>d__.<>1__state = -1;
		<AsyncUISelectionOnOpen>d__.<>t__builder.Start<XUiC_QuestWindowGroup.<AsyncUISelectionOnOpen>d__8>(ref <AsyncUISelectionOnOpen>d__);
	}

	public override void OnClose()
	{
		base.OnClose();
		base.xui.calloutWindow.DisableCallouts(XUiC_GamepadCalloutWindow.CalloutType.Menu);
		base.xui.playerUI.windowManager.CloseIfOpen("windowpaging");
	}

	public override bool GetBindingValue(ref string _value, string _bindingName)
	{
		if (_bindingName == "questsautoshare")
		{
			_value = PartyQuests.AutoShare.ToString();
			return true;
		}
		if (_bindingName == "questsautoaccept")
		{
			_value = PartyQuests.AutoAccept.ToString();
			return true;
		}
		if (!(_bindingName == "queststier"))
		{
			return base.GetBindingValue(ref _value, _bindingName);
		}
		EntityPlayerLocal entityPlayer = base.xui.playerUI.entityPlayer;
		if (entityPlayer != null)
		{
			int currentFactionTier = entityPlayer.QuestJournal.GetCurrentFactionTier(1, 0, false);
			_value = string.Format(Localization.Get("xuiQuestTierDescription", false), ValueDisplayFormatters.RomanNumber(entityPlayer.QuestJournal.GetCurrentFactionTier(1, 0, false)), entityPlayer.QuestJournal.GetQuestFactionPoints(1), entityPlayer.QuestJournal.GetQuestFactionMax(1, currentFactionTier));
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_QuestList questList;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_QuestSharedList sharedList;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_QuestObjectivesWindow objectivesWindow;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_QuestRewardsWindow rewardsWindow;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_QuestDescriptionWindow descriptionWindow;
}
