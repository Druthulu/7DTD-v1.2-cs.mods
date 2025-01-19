﻿using System;
using Audio;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_QuestOfferWindow : XUiController
{
	public XUiC_QuestOfferWindow.OfferTypes OfferType { get; [PublicizedFrom(EAccessModifier.Private)] set; }

	public Quest Quest
	{
		get
		{
			return this.quest;
		}
		set
		{
			this.quest = value;
			this.IsDirty = true;
		}
	}

	public int Variation
	{
		get
		{
			return this.variation;
		}
		set
		{
			this.variation = value;
			this.IsDirty = true;
		}
	}

	public XUiC_ItemStack ItemStackController { get; set; }

	public int QuestGiverID { get; [PublicizedFrom(EAccessModifier.Private)] set; }

	public override bool GetBindingValue(ref string value, string bindingName)
	{
		if (bindingName == "questname")
		{
			value = ((this.Quest != null) ? this.Quest.GetParsedText(this.Quest.QuestClass.Name) : "");
			return true;
		}
		if (bindingName == "questicon")
		{
			value = ((this.Quest != null) ? this.Quest.QuestClass.Icon : "");
			return true;
		}
		if (bindingName == "questoffer")
		{
			value = ((this.Quest != null) ? this.Quest.GetParsedText(this.Quest.QuestClass.Offer) : "");
			return true;
		}
		if (bindingName == "questdifficulty")
		{
			value = ((this.Quest != null) ? this.Quest.QuestClass.Difficulty : "");
			return true;
		}
		if (bindingName == "tieradd")
		{
			if (this.Quest != null && this.Quest.QuestClass.AddsToTierComplete)
			{
				if (!base.xui.playerUI.entityPlayer.QuestJournal.CanAddProgression)
				{
					value = "";
				}
				else
				{
					string arg = ((this.Quest.QuestClass.DifficultyTier > 0) ? "+" : "-") + this.Quest.QuestClass.DifficultyTier.ToString();
					value = string.Format(Localization.Get("xuiQuestTierAdd", false), arg);
				}
			}
			else
			{
				value = "";
			}
			return true;
		}
		if (!(bindingName == "tieraddlimited"))
		{
			return false;
		}
		if (this.Quest != null && this.Quest.QuestClass.AddsToTierComplete && !base.xui.playerUI.entityPlayer.QuestJournal.CanAddProgression)
		{
			value = "true";
		}
		else
		{
			value = "false";
		}
		return true;
	}

	public override void Init()
	{
		base.Init();
		this.btnAccept = base.GetChildById("btnAccept");
		this.btnAccept_Background = (XUiV_Button)this.btnAccept.GetChildById("clickable").ViewComponent;
		this.btnAccept_Background.Controller.OnPress += this.btnAccept_OnPress;
		this.btnAccept_Background.Controller.OnHover += this.btnAccept_OnHover;
		this.btnDecline = base.GetChildById("btnDecline");
		this.btnDecline_Background = (XUiV_Button)this.btnDecline.GetChildById("clickable").ViewComponent;
		this.btnDecline_Background.Controller.OnPress += this.btnDecline_OnPress;
		this.btnDecline_Background.Controller.OnHover += this.btnDecline_OnHover;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void btnAccept_OnHover(XUiController _sender, bool _isOver)
	{
		this.btnAcceptHovered = _isOver;
		base.RefreshBindings(false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void btnDecline_OnHover(XUiController _sender, bool _isOver)
	{
		this.btnDeclineHovered = _isOver;
		base.RefreshBindings(false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void btnAccept_OnPress(XUiController _sender, int _mouseButton)
	{
		Quest quest = this.Quest;
		quest.QuestGiverID = this.QuestGiverID;
		if (this.OfferType == XUiC_QuestOfferWindow.OfferTypes.Item)
		{
			ItemStack itemStack = this.ItemStackController.ItemStack;
			if (itemStack.count > 1)
			{
				itemStack.count--;
				this.ItemStackController.ForceSetItemStack(itemStack.Clone());
				this.ItemStackController.WindowGroup.Controller.SetAllChildrenDirty(false);
			}
			else
			{
				this.ItemStackController.ItemStack = ItemStack.Empty.Clone();
				this.ItemStackController.WindowGroup.Controller.SetAllChildrenDirty(false);
			}
		}
		EntityPlayerLocal entityPlayer = base.xui.playerUI.entityPlayer;
		if (this.QuestGiverID != -1)
		{
			base.xui.Dialog.Respondent.PlayVoiceSetEntry("quest_accepted", entityPlayer, true, true);
		}
		this.questAccepted = true;
		base.xui.playerUI.windowManager.Close(base.WindowGroup.ID);
		entityPlayer.QuestJournal.AddQuest(quest, true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void btnDecline_OnPress(XUiController _sender, int _mouseButton)
	{
		EntityNPC respondent = base.xui.Dialog.Respondent;
		if (this.QuestGiverID != -1)
		{
			base.xui.Dialog.Respondent.PlayVoiceSetEntry("quest_declined", base.xui.playerUI.entityPlayer, true, true);
		}
		base.xui.playerUI.windowManager.Close(base.WindowGroup.ID);
		if (this.OnCancel != null)
		{
			this.OnCancel(respondent);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void closeButton_OnPress(XUiController _sender, int _mouseButton)
	{
		base.xui.playerUI.windowManager.Close(base.WindowGroup.ID);
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (this.IsDirty)
		{
			base.RefreshBindings(true);
			this.IsDirty = false;
		}
	}

	public static XUiC_QuestOfferWindow OpenQuestOfferWindow(XUi xui, Quest q, int listIndex = -1, XUiC_QuestOfferWindow.OfferTypes offerType = XUiC_QuestOfferWindow.OfferTypes.Item, int questGiverID = -1, Action<EntityNPC> onCancel = null)
	{
		bool flag = offerType == XUiC_QuestOfferWindow.OfferTypes.Item;
		XUiC_QuestOfferWindow childByType = xui.FindWindowGroupByName("questOffer").GetChildByType<XUiC_QuestOfferWindow>();
		childByType.Quest = q;
		childByType.Variation = -1;
		childByType.listIndex = listIndex;
		childByType.QuestGiverID = questGiverID;
		childByType.OfferType = offerType;
		childByType.OnCancel = onCancel;
		xui.playerUI.windowManager.Open("questOffer", flag, false, flag);
		return childByType;
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.questAccepted = false;
		Manager.PlayInsidePlayerHead("quest_note_offer", -1, 0f, false, false);
		base.xui.playerUI.windowManager.CloseIfOpen("windowpaging");
		base.xui.playerUI.windowManager.CloseIfOpen("toolbelt");
		if (this.OfferType == XUiC_QuestOfferWindow.OfferTypes.Dialog)
		{
			Dialog currentDialog = base.xui.Dialog.DialogWindowGroup.CurrentDialog;
			base.xui.Dialog.DialogWindowGroup.RefreshDialog();
			base.xui.Dialog.DialogWindowGroup.ShowResponseWindow(false);
			if (this.QuestGiverID != -1)
			{
				base.xui.Dialog.Respondent.PlayVoiceSetEntry("quest_offer", base.xui.playerUI.entityPlayer, true, true);
			}
		}
	}

	public override void OnClose()
	{
		base.OnClose();
		if (!this.questAccepted)
		{
			Manager.PlayInsidePlayerHead("quest_note_decline", -1, 0f, false, false);
		}
		if (this.ItemStackController != null)
		{
			this.ItemStackController.QuestLock = false;
			this.ItemStackController = null;
		}
		if (this.OfferType == XUiC_QuestOfferWindow.OfferTypes.Dialog)
		{
			if (this.questAccepted)
			{
				EntityTrader entityTrader = base.xui.Dialog.Respondent as EntityTrader;
				if (entityTrader != null && entityTrader.activeQuests != null)
				{
					entityTrader.activeQuests.Remove(this.Quest);
				}
				if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
				{
					SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageNPCQuestList>().Setup(this.QuestGiverID, base.xui.playerUI.entityPlayer.entityId, (int)this.Quest.QuestClass.DifficultyTier, (byte)this.listIndex), false);
				}
				if (this.Quest.QuestTags.Test_AnySet(QuestEventManager.treasureTag) && GameSparksCollector.CollectGamePlayData)
				{
					GameSparksCollector.IncrementCounter(GameSparksCollector.GSDataKey.QuestAcceptedDistance, ((int)Vector3.Distance(this.Quest.Position, base.xui.Dialog.Respondent.position) / 50 * 50).ToString(), 1, true, GameSparksCollector.GSDataCollection.SessionUpdates);
				}
			}
			Dialog currentDialog = base.xui.Dialog.DialogWindowGroup.CurrentDialog;
			if (currentDialog.CurrentStatement == null || currentDialog.CurrentStatement.NextStatementID == "")
			{
				base.xui.playerUI.windowManager.Close("dialog");
				return;
			}
			currentDialog.CurrentStatement = currentDialog.GetStatement(currentDialog.CurrentStatement.NextStatementID);
			base.xui.Dialog.DialogWindowGroup.RefreshDialog();
			base.xui.Dialog.DialogWindowGroup.ShowResponseWindow(true);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool btnAcceptHovered;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool btnDeclineHovered;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController btnAccept;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController btnDecline;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Button btnAccept_Background;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Button btnDecline_Background;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool questAccepted;

	[PublicizedFrom(EAccessModifier.Private)]
	public Action<EntityNPC> OnCancel;

	[PublicizedFrom(EAccessModifier.Private)]
	public Quest quest;

	[PublicizedFrom(EAccessModifier.Private)]
	public int variation = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public int listIndex = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool lastAnyKey = true;

	public enum OfferTypes
	{
		Item,
		Dialog
	}
}
