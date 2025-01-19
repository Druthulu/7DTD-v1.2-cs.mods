using System;
using System.Collections.Generic;
using System.Globalization;
using Challenges;
using Platform;
using Twitch;
using UniLinq;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_OptionsTwitch : XUiController
{
	public static event Action OnSettingsChanged;

	public override void Init()
	{
		base.Init();
		XUiC_OptionsTwitch.ID = base.WindowGroup.ID;
		this.tabs = base.GetChildByType<XUiC_TabSelector>();
		this.tabs.OnTabChanged += this.Tabs_OnTabChanged;
		this.comboAllowActionsDuringBloodmoon = base.GetChildById("AllowActionsDuringBloodmoon").GetChildByType<XUiC_ComboBoxEnum<XUiC_OptionsTwitch.TwitchBloodMoonOptions>>();
		this.comboAllowActionsDuringQuests = base.GetChildById("AllowActionsDuringQuests").GetChildByType<XUiC_ComboBoxEnum<XUiC_OptionsTwitch.TwitchBloodMoonOptions>>();
		this.comboPointsGeneration = base.GetChildById("PointGeneration").GetChildByType<XUiC_ComboBoxEnum<XUiC_OptionsTwitch.PointsGenerationOptions>>();
		this.comboCooldownPreset = base.GetChildById("CooldownPreset").GetChildByType<XUiC_ComboBoxList<string>>();
		this.comboUseProgression = base.GetChildById("UseProgression").GetChildByType<XUiC_ComboBoxBool>();
		this.comboOptOutTwitch = base.GetChildById("OptOutTwitch").GetChildByType<XUiC_ComboBoxBool>();
		this.comboAllowVisionEffects = base.GetChildById("AllowVisual").GetChildByType<XUiC_ComboBoxBool>();
		this.comboAllowCrateSharing = base.GetChildById("AllowCrateSharing").GetChildByType<XUiC_ComboBoxBool>();
		this.comboActionSpamDelay = base.GetChildById("ActionSpamDelay").GetChildByType<XUiC_ComboBoxList<string>>();
		this.comboActionTwitchIntegrationType = base.GetChildById("ActionTwitchIntegrationType").GetChildByType<XUiC_ComboBoxEnum<TwitchManager.IntegrationSettings>>();
		this.comboAllowActions = base.GetChildById("AllowActions").GetChildByType<XUiC_ComboBoxList<string>>();
		this.comboAllowVotes = base.GetChildById("AllowVotes").GetChildByType<XUiC_ComboBoxList<string>>();
		this.comboAllowEvents = base.GetChildById("AllowEvents").GetChildByType<XUiC_ComboBoxList<string>>();
		this.comboAllowVotesDuringBloodmoon = base.GetChildById("AllowVotesDuringBloodmoon").GetChildByType<XUiC_ComboBoxBool>();
		this.comboAllowVotesDuringQuests = base.GetChildById("AllowVotesDuringQuests").GetChildByType<XUiC_ComboBoxBool>();
		this.comboAllowVotesInSafeZone = base.GetChildById("AllowVotesInSafeZone").GetChildByType<XUiC_ComboBoxBool>();
		this.comboBitPrices = base.GetChildById("BitPrices").GetChildByType<XUiC_ComboBoxList<string>>();
		this.comboBitPotPercent = base.GetChildById("BitPotPercent").GetChildByType<XUiC_ComboBoxList<string>>();
		this.comboViewerStartingPoints = base.GetChildById("ViewerStartingPoints").GetChildByType<XUiC_ComboBoxList<string>>();
		this.comboBitPointsAdd = base.GetChildById("BitPointsAdd").GetChildByType<XUiC_ComboBoxEnum<XUiC_OptionsTwitch.BitAddOptions>>();
		this.comboSubPointsAdd = base.GetChildById("SubPointsAdd").GetChildByType<XUiC_ComboBoxEnum<XUiC_OptionsTwitch.PointsGenerationOptions>>();
		this.comboGiftSubPointsAdd = base.GetChildById("GiftSubPointsAdd").GetChildByType<XUiC_ComboBoxEnum<XUiC_OptionsTwitch.PointsGenerationOptions>>();
		this.comboRaidPointsAdd = base.GetChildById("RaidPointsAdd").GetChildByType<XUiC_ComboBoxList<string>>();
		this.comboRaidViewerMinimum = base.GetChildById("RaidViewerMinimum").GetChildByType<XUiC_ComboBoxList<string>>();
		this.comboHelperRewardAmount = base.GetChildById("HelperRewardAmount").GetChildByType<XUiC_ComboBoxList<string>>();
		this.comboHelperRewardTime = base.GetChildById("HelperRewardTime").GetChildByType<XUiC_ComboBoxList<string>>();
		this.comboMaxDailyVotes = base.GetChildById("MaxDailyVotes").GetChildByType<XUiC_ComboBoxList<string>>();
		this.comboVoteTime = base.GetChildById("VoteTime").GetChildByType<XUiC_ComboBoxList<string>>();
		this.comboVoteDayTimeRange = base.GetChildById("VoteDayTimeRange").GetChildByType<XUiC_ComboBoxList<string>>();
		this.comboVoteViewerDefeatReward = base.GetChildById("ViewerDefeatReward").GetChildByType<XUiC_ComboBoxList<string>>();
		this.comboAllowBitEvents = base.GetChildById("AllowBitEvents").GetChildByType<XUiC_ComboBoxBool>();
		this.comboAllowSubEvents = base.GetChildById("AllowSubEvents").GetChildByType<XUiC_ComboBoxBool>();
		this.comboAllowGiftSubEvents = base.GetChildById("AllowGiftSubEvents").GetChildByType<XUiC_ComboBoxBool>();
		this.comboAllowCharityEvents = base.GetChildById("AllowCharityEvents").GetChildByType<XUiC_ComboBoxBool>();
		this.comboAllowRaidEvents = base.GetChildById("AllowRaidEvents").GetChildByType<XUiC_ComboBoxBool>();
		this.comboAllowHypeTrainEvents = base.GetChildById("AllowHypeTrainEvents").GetChildByType<XUiC_ComboBoxBool>();
		this.comboAllowCreatorGoalEvents = base.GetChildById("AllowCreatorGoalEvents").GetChildByType<XUiC_ComboBoxBool>();
		this.comboAllowChannelPointRedeemEvents = base.GetChildById("AllowChannelPointRedeemEvents").GetChildByType<XUiC_ComboBoxBool>();
		this.comboAllowActionsDuringBloodmoon.OnValueChangedGeneric += this.Combo_OnValueChangedGeneric;
		this.comboAllowActionsDuringQuests.OnValueChangedGeneric += this.Combo_OnValueChangedGeneric;
		this.comboPointsGeneration.OnValueChangedGeneric += this.Combo_OnValueChangedGeneric;
		this.comboUseProgression.OnValueChangedGeneric += this.Combo_OnValueChangedGeneric;
		this.comboOptOutTwitch.OnValueChangedGeneric += this.Combo_OnValueChangedGeneric;
		this.comboAllowVisionEffects.OnValueChangedGeneric += this.Combo_OnValueChangedGeneric;
		this.comboAllowCrateSharing.OnValueChangedGeneric += this.Combo_OnValueChangedGeneric;
		this.comboActionTwitchIntegrationType.OnValueChangedGeneric += this.Combo_OnValueChangedGeneric;
		this.comboAllowActions.OnValueChanged += this.ComboPreset_OnValueChanged;
		this.comboAllowVotes.OnValueChanged += this.ComboPreset_OnValueChanged;
		this.comboAllowEvents.OnValueChanged += this.ComboPreset_OnValueChanged;
		this.comboAllowVotesDuringBloodmoon.OnValueChangedGeneric += this.Combo_OnValueChangedGeneric;
		this.comboAllowVotesDuringQuests.OnValueChangedGeneric += this.Combo_OnValueChangedGeneric;
		this.comboAllowVotesInSafeZone.OnValueChangedGeneric += this.Combo_OnValueChangedGeneric;
		this.comboBitPointsAdd.OnValueChangedGeneric += this.Combo_OnValueChangedGeneric;
		this.comboSubPointsAdd.OnValueChanged += this.SubPoints_OnValueChanged;
		this.comboGiftSubPointsAdd.OnValueChanged += this.GiftSubPoints_OnValueChanged;
		this.comboAllowBitEvents.OnValueChangedGeneric += this.Combo_OnValueChangedGeneric;
		this.comboAllowSubEvents.OnValueChangedGeneric += this.Combo_OnValueChangedGeneric;
		this.comboAllowGiftSubEvents.OnValueChangedGeneric += this.Combo_OnValueChangedGeneric;
		this.comboAllowCharityEvents.OnValueChangedGeneric += this.Combo_OnValueChangedGeneric;
		this.comboAllowRaidEvents.OnValueChangedGeneric += this.Combo_OnValueChangedGeneric;
		this.comboAllowHypeTrainEvents.OnValueChangedGeneric += this.Combo_OnValueChangedGeneric;
		this.comboAllowCreatorGoalEvents.OnValueChangedGeneric += this.Combo_OnValueChangedGeneric;
		this.comboAllowChannelPointRedeemEvents.OnValueChangedGeneric += this.Combo_OnValueChangedGeneric;
		this.comboBitPrices.OnValueChangedGeneric += this.Combo_OnValueChangedGeneric;
		this.comboBitPotPercent.OnValueChangedGeneric += this.Combo_OnValueChangedGeneric;
		this.btnLoginTwitch = (base.GetChildById("btnLoginTwitch") as XUiC_SimpleButton);
		this.btnLoginTwitch.OnPressed += this.BtnLoginTwitch_OnPressed;
		this.btnShowExtras = (base.GetChildById("btnShowExtras") as XUiC_SimpleButton);
		this.btnShowExtras.OnPressed += this.BtnShowExtras_OnPressed;
		this.btnEnableAllExtras = (base.GetChildById("btnEnableAllExtras") as XUiC_SimpleButton);
		this.btnEnableAllExtras.OnPressed += this.BtnEnableAllExtras_OnPressed;
		this.btnDisableAllExtras = (base.GetChildById("btnDisableAllExtras") as XUiC_SimpleButton);
		this.btnDisableAllExtras.OnPressed += this.BtnDisableAllExtras_OnPressed;
		this.btnResetPrices = (base.GetChildById("btnResetPrices") as XUiC_SimpleButton);
		this.btnResetPrices.OnPressed += this.BtnResetPrices_OnPressed;
		this.btnBack = (base.GetChildById("btnBack") as XUiC_SimpleButton);
		this.btnDefaults = (base.GetChildById("btnDefaults") as XUiC_SimpleButton);
		this.btnApply = (base.GetChildById("btnApply") as XUiC_SimpleButton);
		this.btnBack.OnPressed += this.BtnBack_OnPressed;
		this.btnDefaults.OnPressed += this.BtnDefaults_OnOnPressed;
		this.btnApply.OnPressed += this.BtnApply_OnPressed;
		this.RefreshApplyLabel();
		base.RegisterForInputStyleChanges();
		this.qrCodeTexControl = (base.GetChildById("qrCodeTex").ViewComponent as XUiV_Texture);
		TwitchAuthentication.QRCodeGenerated += this.TwitchAuthentication_QRCodeGenerated;
		(base.GetChildById("btnShowDeviceCode") as XUiC_SimpleButton).OnPressed += this.BtnShowDeviceCode_OnPressed;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RefreshApplyLabel()
	{
		InControlExtensions.SetApplyButtonString(this.btnApply, "xuiApply");
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void InputStyleChanged(PlayerInputManager.InputStyle _oldStyle, PlayerInputManager.InputStyle _newStyle)
	{
		base.InputStyleChanged(_oldStyle, _newStyle);
		this.RefreshApplyLabel();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnShowDeviceCode_OnPressed(XUiController _sender, int _mouseButton)
	{
		this.showDeviceCode = !this.showDeviceCode;
		base.RefreshBindings(false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Combo_OnValueChangedGeneric(XUiController _sender)
	{
		this.btnApply.Enabled = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Tabs_OnTabChanged(int _i, string _s)
	{
		if (_i != 0 && this.twitchManager.InitState != TwitchManager.InitStates.Ready)
		{
			this.tabs.SelectedTabIndex = 0;
		}
		this.IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ComboBoxListString_OnValueChanged(XUiController _sender, string _oldValue, string _newValue)
	{
		this.btnApply.Enabled = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ComboPreset_OnValueChanged(XUiController _sender, string _oldValue, string _newValue)
	{
		this.btnApply.Enabled = true;
		this.IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ComboVoteDayTimeRange_OnValueChanged(XUiController _sender, string _oldValue, string _newValue)
	{
		this.btnApply.Enabled = true;
		this.IsDirty = true;
		this.tempVoteDayTimeRange = this.comboVoteDayTimeRange.SelectedIndex;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SubPoints_OnValueChanged(XUiController _sender, XUiC_OptionsTwitch.PointsGenerationOptions _oldValue, XUiC_OptionsTwitch.PointsGenerationOptions _newValue)
	{
		this.btnApply.Enabled = true;
		this.IsDirty = true;
		this.tempSubModifier = (int)_newValue;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void GiftSubPoints_OnValueChanged(XUiController _sender, XUiC_OptionsTwitch.PointsGenerationOptions _oldValue, XUiC_OptionsTwitch.PointsGenerationOptions _newValue)
	{
		this.btnApply.Enabled = true;
		this.IsDirty = true;
		this.tempGiftSubModifier = (int)_newValue;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnLoginTwitch_OnPressed(XUiController _sender, int _mouseButton)
	{
		switch (this.twitchManager.InitState)
		{
		case TwitchManager.InitStates.None:
		case TwitchManager.InitStates.PermissionDenied:
		case TwitchManager.InitStates.WaitingForOAuth:
		case TwitchManager.InitStates.ExtensionNotInstalled:
		case TwitchManager.InitStates.Failed:
			this.startedSinceOpened = true;
			this.showDeviceCode = false;
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				this.twitchManager.StartTwitchIntegration();
			}
			else
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageTwitchAccess>().Setup(), false);
				this.twitchManager.WaitForPermission();
			}
			if (!this.localPlayer.PlayerUI.windowManager.IsWindowOpen("twitch"))
			{
				this.localPlayer.PlayerUI.windowManager.Open("twitch", false, false, true);
			}
			this.origAllowTwitchOptions = true;
			this.localPlayer.TwitchActionsEnabled = EntityPlayer.TwitchActionsStates.Enabled;
			return;
		case TwitchManager.InitStates.WaitingForPermission:
		case TwitchManager.InitStates.Authenticating:
		case TwitchManager.InitStates.Authenticated:
		case TwitchManager.InitStates.CheckingForExtension:
			break;
		case TwitchManager.InitStates.Ready:
			this.twitchManager.StopTwitchIntegration(TwitchManager.InitStates.None);
			break;
		default:
			return;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnShowExtras_OnPressed(XUiController _sender, int _mouseButton)
	{
		this.applyChanges();
		base.xui.playerUI.windowManager.Close(this.windowGroup.ID);
		if (GameManager.Instance.IsPaused() && GameStats.GetInt(EnumGameStats.GameState) == 2)
		{
			GameManager.Instance.Pause(false);
		}
		XUiC_TwitchWindowSelector.OpenSelectorAndWindow(GameManager.Instance.World.GetPrimaryPlayer(), "Actions", true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnEnableAllExtras_OnPressed(XUiController _sender, int _mouseButton)
	{
		List<TwitchAction> list = (from entry in TwitchActionManager.TwitchActions.Values.ToList<TwitchAction>()
		where entry.DisplayCategory.Name == "Extras"
		orderby entry.Title
		select entry).ToList<TwitchAction>();
		TwitchActionPreset currentActionPreset = TwitchManager.Current.CurrentActionPreset;
		foreach (TwitchAction twitchAction in list)
		{
			if (!currentActionPreset.AddedActions.Contains(twitchAction.Name))
			{
				currentActionPreset.AddedActions.Add(twitchAction.Name);
				QuestEventManager.Current.TwitchEventReceived(TwitchObjectiveTypes.EnableExtras, twitchAction.DisplayCategory.Name);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnDisableAllExtras_OnPressed(XUiController _sender, int _mouseButton)
	{
		List<TwitchAction> list = (from entry in TwitchActionManager.TwitchActions.Values.ToList<TwitchAction>()
		where entry.DisplayCategory.Name == "Extras"
		orderby entry.Title
		select entry).ToList<TwitchAction>();
		TwitchActionPreset currentActionPreset = TwitchManager.Current.CurrentActionPreset;
		foreach (TwitchAction twitchAction in list)
		{
			if (currentActionPreset.AddedActions.Contains(twitchAction.Name))
			{
				currentActionPreset.AddedActions.Remove(twitchAction.Name);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnResetPrices_OnPressed(XUiController _sender, int _mouseButton)
	{
		this.twitchManager.ResetPricesToDefault();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnApply_OnPressed(XUiController _sender, int _mouseButton)
	{
		this.applyChanges();
		this.btnApply.Enabled = false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnDefaults_OnOnPressed(XUiController _sender, int _mouseButton)
	{
		this.twitchManager.UseActionsDuringBloodmoon = 2;
		this.twitchManager.UseActionsDuringQuests = 2;
		this.twitchManager.ViewerData.PointRate = 1f;
		this.twitchManager.SetToDefaultCooldown();
		this.twitchManager.SetUseProgression(true);
		this.twitchManager.AllowCrateSharing = false;
		this.twitchManager.ViewerData.ActionSpamDelay = 3f;
		TwitchVotingManager votingManager = this.twitchManager.VotingManager;
		this.twitchManager.SetToDefaultActionPreset();
		this.twitchManager.SetToDefaultVotePreset();
		this.twitchManager.SetToDefaultEventPreset();
		this.twitchManager.ViewerData.StartingPoints = 100;
		this.twitchManager.BitPointModifier = 1;
		this.twitchManager.SubPointModifier = 1;
		this.twitchManager.GiftSubPointModifier = 2;
		this.twitchManager.RaidPointAdd = 1000;
		this.twitchManager.RaidViewerMinimum = 10;
		this.twitchManager.BitPotPercentage = 0.25f;
		TwitchManager.LeaderboardStats.GoodRewardAmount = 1000;
		TwitchManager.LeaderboardStats.GoodRewardTime = 15;
		this.localPlayer.TwitchActionsEnabled = EntityPlayer.TwitchActionsStates.Enabled;
		votingManager.MaxDailyVotes = 4;
		votingManager.VoteTime = 60f;
		votingManager.CurrentVoteDayTimeRange = 2;
		votingManager.ViewerDefeatReward = 250;
		votingManager.AllowVotesDuringBloodmoon = false;
		votingManager.AllowVotesDuringQuests = false;
		votingManager.AllowVotesInSafeZone = false;
		this.updateOptions();
		this.applyChanges();
		this.btnApply.Enabled = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnBack_OnPressed(XUiController _sender, int _mouseButton)
	{
		base.xui.playerUI.windowManager.Close(this.windowGroup.ID);
		base.xui.playerUI.windowManager.Open(XUiC_OptionsMenu.ID, true, false, true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TwitchAuthentication_QRCodeGenerated(Texture2D qrCodeTex, string userCode, string verificationUrl)
	{
		this.qrCodeTexControl.Texture = qrCodeTex;
		this.Auth_Code = userCode;
		this.Auth_VerificationUrl = verificationUrl;
		base.RefreshBindings(false);
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (this.IsDirty)
		{
			base.RefreshBindings(false);
			this.RefreshApplyLabel();
			this.IsDirty = false;
		}
		if (this.btnApply.Enabled && base.xui.playerUI.playerInput.GUIActions.Inspect.WasPressed)
		{
			this.BtnApply_OnPressed(null, 0);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateOptions()
	{
		if (GameStats.GetBool(EnumGameStats.TwitchBloodMoonAllowed))
		{
			this.comboAllowActionsDuringBloodmoon.Value = (this.origAllowActionsDuringBloodmoon = (XUiC_OptionsTwitch.TwitchBloodMoonOptions)TwitchManager.Current.UseActionsDuringBloodmoon);
			this.comboAllowActionsDuringBloodmoon.Enabled = true;
		}
		else
		{
			this.comboAllowActionsDuringBloodmoon.Value = XUiC_OptionsTwitch.TwitchBloodMoonOptions.Disabled;
			this.comboAllowActionsDuringBloodmoon.Enabled = false;
			this.twitchManager.UseActionsDuringBloodmoon = 0;
		}
		this.comboAllowActionsDuringQuests.Value = (this.origAllowActionsDuringQuests = (XUiC_OptionsTwitch.TwitchBloodMoonOptions)TwitchManager.Current.UseActionsDuringQuests);
		this.comboPointsGeneration.Value = (this.origPointsGeneration = (XUiC_OptionsTwitch.PointsGenerationOptions)this.twitchManager.ViewerData.PointRate);
		this.comboCooldownPreset.SelectedIndex = (this.origCooldownPresetIndex = this.twitchManager.CooldownPresetIndex);
		this.comboUseProgression.Value = (this.origUseProgression = this.twitchManager.UseProgression);
		this.comboAllowVisionEffects.Value = (this.origAllowVisionEffects = !this.localPlayer.TwitchVisionDisabled);
		this.comboOptOutTwitch.Value = (this.origAllowTwitchOptions = (this.localPlayer.TwitchActionsEnabled > EntityPlayer.TwitchActionsStates.Disabled));
		this.comboAllowCrateSharing.Value = (this.origAllowCrateSharing = this.twitchManager.AllowCrateSharing);
		this.comboActionTwitchIntegrationType.Value = (this.origIntegrationSetting = this.twitchManager.IntegrationSetting);
		TwitchVotingManager votingManager = this.twitchManager.VotingManager;
		this.comboAllowActions.SelectedIndex = (this.origActionPresetIndex = this.twitchManager.ActionPresetIndex);
		this.comboAllowVotes.SelectedIndex = (this.origVotePresetIndex = this.twitchManager.VotePresetIndex);
		this.comboAllowEvents.SelectedIndex = (this.origEventPresetIndex = this.twitchManager.EventPresetIndex);
		this.comboBitPrices.SelectedIndex = (this.origBitPricesIndex = this.GetBitPriceModifier(this.twitchManager.BitPriceMultiplier));
		this.comboBitPotPercent.SelectedIndex = (this.origBitPotPercentIndex = Mathf.FloorToInt((1f - this.twitchManager.BitPotPercentage) / 0.05f));
		this.comboViewerStartingPoints.SelectedIndex = (this.origViewerStartingPoints = this.GetIndexFromCombobox(this.twitchManager.ViewerData.StartingPoints, this.comboViewerStartingPoints));
		this.comboBitPointsAdd.Value = (this.origBitPointModifier = (XUiC_OptionsTwitch.BitAddOptions)this.twitchManager.BitPointModifier);
		this.comboSubPointsAdd.Value = (this.origSubPointModifier = (XUiC_OptionsTwitch.PointsGenerationOptions)this.twitchManager.SubPointModifier);
		this.comboGiftSubPointsAdd.Value = (this.origGiftSubPointModifier = (XUiC_OptionsTwitch.PointsGenerationOptions)this.twitchManager.GiftSubPointModifier);
		this.comboRaidPointsAdd.SelectedIndex = (this.origRaidPointAmountIndex = this.GetIndexFromCombobox(this.twitchManager.RaidPointAdd, this.comboRaidPointsAdd));
		this.comboRaidViewerMinimum.SelectedIndex = (this.origRaidViewerMinimumIndex = this.GetIndexFromCombobox(this.twitchManager.RaidViewerMinimum, this.comboRaidViewerMinimum));
		this.comboHelperRewardAmount.SelectedIndex = (this.origHelperRewardAmountIndex = this.GetIndexFromCombobox(TwitchManager.LeaderboardStats.GoodRewardAmount, this.comboHelperRewardAmount));
		this.comboHelperRewardTime.SelectedIndex = (this.origHelperRewardTimeIndex = this.GetIndexFromCombobox(TwitchManager.LeaderboardStats.GoodRewardTime, this.comboHelperRewardTime));
		this.comboMaxDailyVotes.SelectedIndex = (this.origMaxDailyVotes = this.GetIndexFromCombobox(votingManager.MaxDailyVotes, this.comboMaxDailyVotes));
		this.comboVoteTime.SelectedIndex = (this.origVoteTime = this.GetIndexFromCombobox((int)votingManager.VoteTime, this.comboVoteTime));
		this.comboVoteDayTimeRange.SelectedIndex = (this.origVoteDayTimeRange = votingManager.CurrentVoteDayTimeRange);
		this.comboVoteViewerDefeatReward.SelectedIndex = (this.origVoteViewerDefeatReward = this.GetIndexFromCombobox(votingManager.ViewerDefeatReward, this.comboVoteViewerDefeatReward));
		this.comboActionSpamDelay.SelectedIndex = (this.origActionSpamDelay = this.GetActionSpamDelay(this.twitchManager.ViewerData.ActionSpamDelay));
		this.comboAllowVotesDuringBloodmoon.Value = (this.origAllowVotesDuringBloodmoon = votingManager.AllowVotesDuringBloodmoon);
		this.comboAllowVotesDuringQuests.Value = (this.origAllowVotesDuringQuests = votingManager.AllowVotesDuringQuests);
		this.comboAllowVotesInSafeZone.Value = (this.origAllowVotesInSafeZone = votingManager.AllowVotesInSafeZone);
		this.comboAllowBitEvents.Value = (this.origAllowBitEvents = this.twitchManager.AllowBitEvents);
		this.comboAllowSubEvents.Value = (this.origAllowSubEvents = this.twitchManager.AllowSubEvents);
		this.comboAllowGiftSubEvents.Value = (this.origAllowGiftSubEvents = this.twitchManager.AllowGiftSubEvents);
		this.comboAllowCharityEvents.Value = (this.origAllowCharityEvents = this.twitchManager.AllowCharityEvents);
		this.comboAllowRaidEvents.Value = (this.origAllowRaidEvents = this.twitchManager.AllowRaidEvents);
		this.comboAllowHypeTrainEvents.Value = (this.origAllowHypeTrainEvents = this.twitchManager.AllowHypeTrainEvents);
		this.comboAllowCreatorGoalEvents.Value = (this.origAllowCreatorGoalEvents = this.twitchManager.AllowCreatorGoalEvents);
		this.comboAllowChannelPointRedeemEvents.Value = (this.origAllowChannelPointRedemptions = this.twitchManager.AllowChannelPointRedemptions);
		this.tempSubModifier = (int)this.origSubPointModifier;
		this.tempGiftSubModifier = (int)this.origGiftSubPointModifier;
		this.tempVoteDayTimeRange = this.origVoteDayTimeRange;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int GetIndexFromCombobox(int value, XUiC_ComboBoxList<string> cboList)
	{
		string b = value.ToString();
		for (int i = 0; i < cboList.Elements.Count; i++)
		{
			if (cboList.Elements[i] == b)
			{
				return i;
			}
		}
		return -1;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int GetActionSpamDelay(float actionSpamDelay)
	{
		string text = ((int)actionSpamDelay).ToString();
		for (int i = 0; i < this.comboActionSpamDelay.Elements.Count; i++)
		{
			if (i == 0)
			{
				if (text == "0")
				{
					return i;
				}
			}
			else if (this.comboActionSpamDelay.Elements[i] == text)
			{
				return i;
			}
		}
		return -1;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int GetBitPriceModifier(float bitPriceModifier)
	{
		if (bitPriceModifier <= 0.5f)
		{
			if (bitPriceModifier == 0.25f)
			{
				return 3;
			}
			if (bitPriceModifier != 0.5f)
			{
				return 0;
			}
			return 2;
		}
		else
		{
			if (bitPriceModifier != 0.75f)
			{
				return 0;
			}
			return 1;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public float SetBitPriceModifier(int index)
	{
		switch (index)
		{
		case 0:
			return 1f;
		case 1:
			return 0.75f;
		case 2:
			return 0.5f;
		case 3:
			return 0.25f;
		default:
			return 1f;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void applyChanges()
	{
		this.origAllowActionsDuringBloodmoon = this.comboAllowActionsDuringBloodmoon.Value;
		this.origAllowActionsDuringQuests = this.comboAllowActionsDuringQuests.Value;
		this.origPointsGeneration = this.comboPointsGeneration.Value;
		this.origCooldownPresetIndex = this.comboCooldownPreset.SelectedIndex;
		this.origUseProgression = this.comboUseProgression.Value;
		this.origAllowVisionEffects = this.comboAllowVisionEffects.Value;
		this.origAllowTwitchOptions = this.comboOptOutTwitch.Value;
		this.origAllowCrateSharing = this.comboAllowCrateSharing.Value;
		this.origActionSpamDelay = this.comboActionSpamDelay.SelectedIndex;
		this.origIntegrationSetting = this.comboActionTwitchIntegrationType.Value;
		this.origActionPresetIndex = this.comboAllowActions.SelectedIndex;
		this.origVotePresetIndex = this.comboAllowVotes.SelectedIndex;
		this.origEventPresetIndex = this.comboAllowEvents.SelectedIndex;
		this.origViewerStartingPoints = this.comboViewerStartingPoints.SelectedIndex;
		this.origBitPointModifier = this.comboBitPointsAdd.Value;
		this.origSubPointModifier = this.comboSubPointsAdd.Value;
		this.origGiftSubPointModifier = this.comboGiftSubPointsAdd.Value;
		this.origRaidPointAmountIndex = this.comboRaidPointsAdd.SelectedIndex;
		this.origRaidViewerMinimumIndex = this.comboRaidViewerMinimum.SelectedIndex;
		this.origHelperRewardAmountIndex = this.comboHelperRewardAmount.SelectedIndex;
		this.origHelperRewardTimeIndex = this.comboHelperRewardTime.SelectedIndex;
		this.origBitPricesIndex = this.comboBitPrices.SelectedIndex;
		this.origBitPotPercentIndex = this.comboBitPotPercent.SelectedIndex;
		this.origMaxDailyVotes = this.comboMaxDailyVotes.SelectedIndex;
		this.origVoteTime = this.comboVoteTime.SelectedIndex;
		this.origVoteDayTimeRange = this.comboVoteDayTimeRange.SelectedIndex;
		this.origVoteViewerDefeatReward = this.comboVoteViewerDefeatReward.SelectedIndex;
		this.origAllowVotesDuringBloodmoon = this.comboAllowVotesDuringBloodmoon.Value;
		this.origAllowVotesDuringQuests = this.comboAllowVotesDuringQuests.Value;
		this.origAllowVotesInSafeZone = this.comboAllowVotesInSafeZone.Value;
		this.origAllowBitEvents = this.comboAllowBitEvents.Value;
		this.origAllowSubEvents = this.comboAllowSubEvents.Value;
		this.origAllowGiftSubEvents = this.comboAllowGiftSubEvents.Value;
		this.origAllowCharityEvents = this.comboAllowCharityEvents.Value;
		this.origAllowRaidEvents = this.comboAllowRaidEvents.Value;
		this.origAllowHypeTrainEvents = this.comboAllowHypeTrainEvents.Value;
		this.origAllowCreatorGoalEvents = this.comboAllowCreatorGoalEvents.Value;
		this.origAllowChannelPointRedemptions = this.comboAllowChannelPointRedeemEvents.Value;
		if (XUiC_OptionsTwitch.OnSettingsChanged != null)
		{
			XUiC_OptionsTwitch.OnSettingsChanged();
		}
	}

	public override void OnOpen()
	{
		base.WindowGroup.openWindowOnEsc = XUiC_OptionsMenu.ID;
		this.twitchManager = TwitchManager.Current;
		this.twitchManager.ConnectionStateChanged -= this.TwitchManager_ConnectionStateChanged;
		this.twitchManager.ConnectionStateChanged += this.TwitchManager_ConnectionStateChanged;
		this.comboCooldownPreset.OnValueChanged -= this.ComboBoxListString_OnValueChanged;
		this.comboCooldownPreset.Elements.Clear();
		for (int i = 0; i < this.twitchManager.CooldownPresets.Count; i++)
		{
			this.comboCooldownPreset.Elements.Add(this.twitchManager.CooldownPresets[i].Title);
		}
		if (this.twitchManager.CooldownPresetIndex >= this.comboCooldownPreset.Elements.Count)
		{
			this.twitchManager.SetToDefaultCooldown();
		}
		this.comboCooldownPreset.Value = this.comboCooldownPreset.Elements[this.twitchManager.CooldownPresetIndex];
		this.comboCooldownPreset.OnValueChanged += this.ComboBoxListString_OnValueChanged;
		this.localPlayer = GameManager.Instance.World.GetPrimaryPlayer();
		this.comboAllowActions.OnValueChanged -= this.ComboPreset_OnValueChanged;
		this.comboAllowActions.Elements.Clear();
		for (int j = 0; j < this.twitchManager.ActionPresets.Count; j++)
		{
			this.comboAllowActions.Elements.Add(this.twitchManager.ActionPresets[j].Title);
		}
		if (this.twitchManager.ActionPresetIndex >= this.comboAllowActions.Elements.Count)
		{
			this.twitchManager.SetToDefaultActionPreset();
		}
		this.comboAllowActions.Value = this.comboAllowActions.Elements[this.twitchManager.ActionPresetIndex];
		this.comboAllowActions.OnValueChanged += this.ComboPreset_OnValueChanged;
		this.comboAllowVotes.OnValueChanged -= this.ComboPreset_OnValueChanged;
		this.comboAllowVotes.Elements.Clear();
		for (int k = 0; k < this.twitchManager.VotePresets.Count; k++)
		{
			this.comboAllowVotes.Elements.Add(this.twitchManager.VotePresets[k].Title);
		}
		if (this.twitchManager.VotePresetIndex >= this.comboAllowVotes.Elements.Count)
		{
			this.twitchManager.SetToDefaultVotePreset();
		}
		this.comboAllowVotes.Value = this.comboAllowVotes.Elements[this.twitchManager.VotePresetIndex];
		this.comboAllowVotes.OnValueChanged += this.ComboPreset_OnValueChanged;
		this.comboAllowEvents.OnValueChanged -= this.ComboPreset_OnValueChanged;
		this.comboAllowEvents.Elements.Clear();
		for (int l = 0; l < this.twitchManager.EventPresets.Count; l++)
		{
			this.comboAllowEvents.Elements.Add(this.twitchManager.EventPresets[l].Title);
		}
		if (this.twitchManager.EventPresetIndex >= this.comboAllowEvents.Elements.Count)
		{
			this.twitchManager.SetToDefaultEventPreset();
		}
		this.comboAllowEvents.Value = this.comboAllowEvents.Elements[this.twitchManager.EventPresetIndex];
		this.comboAllowEvents.OnValueChanged += this.ComboPreset_OnValueChanged;
		this.SetupComboBoxListString(this.comboViewerStartingPoints, new string[]
		{
			"0",
			"50",
			"100",
			"150",
			"200",
			"250",
			"500"
		}, this.twitchManager.ViewerData.StartingPoints, null);
		this.SetupComboBoxListString(this.comboRaidPointsAdd, new string[]
		{
			"0",
			"500",
			"1000",
			"1500",
			"2000",
			"2500"
		}, this.twitchManager.RaidPointAdd, null);
		this.SetupComboBoxListString(this.comboRaidViewerMinimum, new string[]
		{
			"1",
			"3",
			"5",
			"10",
			"15",
			"20",
			"25",
			"50",
			"100"
		}, this.twitchManager.RaidViewerMinimum, null);
		this.SetupComboBoxListString(this.comboMaxDailyVotes, new string[]
		{
			"1",
			"2",
			"3",
			"4",
			"5",
			"6",
			"7",
			"8",
			"9",
			"10",
			"11",
			"12"
		}, this.twitchManager.VotingManager.MaxDailyVotes, null);
		this.SetupComboBoxListString(this.comboVoteTime, new string[]
		{
			"30",
			"60",
			"90",
			"120"
		}, (int)this.twitchManager.VotingManager.VoteTime, null);
		this.SetupComboBoxListString(this.comboHelperRewardAmount, new string[]
		{
			"100",
			"250",
			"500",
			"1000",
			"1250",
			"1500",
			"2000"
		}, TwitchManager.LeaderboardStats.GoodRewardAmount, null);
		this.SetupComboBoxListString(this.comboHelperRewardTime, new string[]
		{
			"15",
			"30",
			"45",
			"60"
		}, TwitchManager.LeaderboardStats.GoodRewardTime, null);
		this.comboVoteDayTimeRange.OnValueChanged -= this.ComboVoteDayTimeRange_OnValueChanged;
		this.comboVoteDayTimeRange.Elements.Clear();
		for (int m = 0; m < this.twitchManager.VotingManager.VoteDayTimeRanges.Count; m++)
		{
			this.comboVoteDayTimeRange.Elements.Add(this.twitchManager.VotingManager.VoteDayTimeRanges[m].Name);
		}
		this.comboVoteDayTimeRange.SelectedIndex = this.twitchManager.VotingManager.CurrentVoteDayTimeRange;
		this.comboVoteDayTimeRange.OnValueChanged += this.ComboVoteDayTimeRange_OnValueChanged;
		this.SetupComboBoxListString(this.comboVoteViewerDefeatReward, new string[]
		{
			"0",
			"100",
			"200",
			"250",
			"500",
			"1000",
			"2500",
			"5000"
		}, this.twitchManager.VotingManager.ViewerDefeatReward, null);
		this.comboActionSpamDelay.OnValueChanged -= this.ComboBoxListString_OnValueChanged;
		this.comboActionSpamDelay.Elements.Clear();
		this.comboActionSpamDelay.Elements.AddRange(new string[]
		{
			Localization.Get("xuiLightPropShadowsNone", false),
			"1",
			"2",
			"3",
			"4",
			"5"
		});
		this.comboActionSpamDelay.Value = this.comboActionSpamDelay.Elements[this.GetActionSpamDelay(this.twitchManager.ViewerData.ActionSpamDelay)];
		this.comboActionSpamDelay.OnValueChanged += this.ComboBoxListString_OnValueChanged;
		this.comboBitPrices.OnValueChanged -= this.ComboBoxListString_OnValueChanged;
		this.comboBitPrices.Elements.Clear();
		this.comboBitPrices.Elements.AddRange(new string[]
		{
			Localization.Get("xuiDefault", false),
			"75%",
			"50%",
			"25%"
		});
		this.comboBitPrices.Value = this.comboBitPrices.Elements[this.GetBitPriceModifier(this.twitchManager.BitPriceMultiplier)];
		this.comboBitPrices.OnValueChanged += this.ComboBoxListString_OnValueChanged;
		this.comboBitPotPercent.OnValueChanged -= this.ComboBoxListString_OnValueChanged;
		this.comboBitPotPercent.Elements.Clear();
		this.comboBitPotPercent.Elements.AddRange(new string[]
		{
			"100%",
			"95%",
			"90%",
			"85%",
			"80%",
			"75%",
			"70%",
			"65%",
			"60%",
			"55%",
			"50%",
			"45%",
			"40%",
			"35%",
			"30%",
			"25%",
			"20%",
			"15%",
			"10%",
			"0%"
		});
		this.comboBitPotPercent.Value = this.comboBitPotPercent.Elements[Mathf.FloorToInt((1f - this.twitchManager.BitPotPercentage) / 0.05f)];
		this.comboBitPotPercent.OnValueChanged += this.ComboBoxListString_OnValueChanged;
		if (this.tabs != null)
		{
			this.tabs.SelectedTabIndex = 0;
		}
		this.updateOptions();
		base.OnOpen();
		this.btnApply.Enabled = false;
		base.RefreshBindings(false);
		this.startedSinceOpened = false;
		XUi.InGameMenuOpen = true;
		this.IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetupComboBoxListString(XUiC_ComboBoxList<string> comboBox, string[] values, int currentValue, XUiC_ComboBox<string>.XUiEvent_ValueChanged valueChanged = null)
	{
		if (valueChanged == null)
		{
			valueChanged = new XUiC_ComboBox<string>.XUiEvent_ValueChanged(this.ComboBoxListString_OnValueChanged);
		}
		comboBox.OnValueChanged -= valueChanged;
		comboBox.Elements.Clear();
		comboBox.Elements.AddRange(values);
		comboBox.Value = comboBox.Elements[this.GetIndexFromCombobox(currentValue, comboBox)];
		comboBox.OnValueChanged += valueChanged;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TwitchManager_ConnectionStateChanged(TwitchManager.InitStates oldState, TwitchManager.InitStates newState)
	{
		base.RefreshBindings(false);
		if (oldState != TwitchManager.InitStates.Ready && newState == TwitchManager.InitStates.Ready)
		{
			this.localPlayer.TwitchActionsEnabled = EntityPlayer.TwitchActionsStates.Enabled;
			this.comboOptOutTwitch.Value = true;
			this.origAllowTwitchOptions = true;
		}
	}

	public override void OnClose()
	{
		base.OnClose();
		this.twitchManager.UseActionsDuringBloodmoon = (int)this.origAllowActionsDuringBloodmoon;
		this.twitchManager.UseActionsDuringQuests = (int)this.origAllowActionsDuringQuests;
		this.twitchManager.ViewerData.PointRate = (float)this.origPointsGeneration;
		this.twitchManager.BitPriceMultiplier = this.SetBitPriceModifier(this.origBitPricesIndex);
		this.twitchManager.BitPotPercentage = 1f - (float)this.origBitPotPercentIndex * 0.05f;
		this.twitchManager.SetCooldownPreset(this.origCooldownPresetIndex);
		this.twitchManager.SetUseProgression(this.origUseProgression);
		this.twitchManager.AllowCrateSharing = this.origAllowCrateSharing;
		this.twitchManager.ViewerData.ActionSpamDelay = (float)this.origActionSpamDelay;
		this.twitchManager.IntegrationSetting = this.origIntegrationSetting;
		TwitchVotingManager votingManager = this.twitchManager.VotingManager;
		bool allowChannelPointRedemptions = this.twitchManager.AllowChannelPointRedemptions;
		this.twitchManager.AllowBitEvents = this.origAllowBitEvents;
		this.twitchManager.AllowSubEvents = this.origAllowSubEvents;
		this.twitchManager.AllowGiftSubEvents = this.origAllowGiftSubEvents;
		this.twitchManager.AllowCharityEvents = this.origAllowCharityEvents;
		this.twitchManager.AllowRaidEvents = this.origAllowRaidEvents;
		this.twitchManager.AllowHypeTrainEvents = this.origAllowHypeTrainEvents;
		this.twitchManager.AllowCreatorGoalEvents = this.origAllowCreatorGoalEvents;
		this.twitchManager.AllowChannelPointRedemptions = this.origAllowChannelPointRedemptions;
		this.twitchManager.SetTwitchActionPreset(this.origActionPresetIndex);
		this.twitchManager.SetTwitchVotePreset(this.origVotePresetIndex);
		this.twitchManager.SetTwitchEventPreset(this.origEventPresetIndex, allowChannelPointRedemptions);
		this.twitchManager.ViewerData.StartingPoints = StringParsers.ParseSInt32(this.comboViewerStartingPoints.Elements[this.origViewerStartingPoints], 0, -1, NumberStyles.Integer);
		this.twitchManager.BitPointModifier = (int)this.origBitPointModifier;
		this.twitchManager.SubPointModifier = (int)this.origSubPointModifier;
		this.twitchManager.GiftSubPointModifier = (int)this.origGiftSubPointModifier;
		this.twitchManager.RaidPointAdd = StringParsers.ParseSInt32(this.comboRaidPointsAdd.Elements[this.origRaidPointAmountIndex], 0, -1, NumberStyles.Integer);
		this.twitchManager.RaidViewerMinimum = StringParsers.ParseSInt32(this.comboRaidViewerMinimum.Elements[this.origRaidViewerMinimumIndex], 0, -1, NumberStyles.Integer);
		TwitchManager.LeaderboardStats.GoodRewardAmount = StringParsers.ParseSInt32(this.comboHelperRewardAmount.Elements[this.origHelperRewardAmountIndex], 0, -1, NumberStyles.Integer);
		TwitchManager.LeaderboardStats.GoodRewardTime = StringParsers.ParseSInt32(this.comboHelperRewardTime.Elements[this.origHelperRewardTimeIndex], 0, -1, NumberStyles.Integer);
		votingManager.MaxDailyVotes = StringParsers.ParseSInt32(this.comboMaxDailyVotes.Elements[this.origMaxDailyVotes], 0, -1, NumberStyles.Integer);
		votingManager.VoteTime = StringParsers.ParseFloat(this.comboVoteTime.Elements[this.origVoteTime], 0, -1, NumberStyles.Any);
		votingManager.CurrentVoteDayTimeRange = this.origVoteDayTimeRange;
		votingManager.ViewerDefeatReward = StringParsers.ParseSInt32(this.comboVoteViewerDefeatReward.Elements[this.origVoteViewerDefeatReward], 0, -1, NumberStyles.Integer);
		votingManager.AllowVotesDuringBloodmoon = this.origAllowVotesDuringBloodmoon;
		votingManager.AllowVotesDuringQuests = this.origAllowVotesDuringQuests;
		votingManager.AllowVotesInSafeZone = this.origAllowVotesInSafeZone;
		this.twitchManager.HasDataChanges = true;
		if (this.origAllowTwitchOptions)
		{
			if (this.localPlayer.TwitchActionsEnabled == EntityPlayer.TwitchActionsStates.Disabled)
			{
				this.localPlayer.TwitchActionsEnabled = EntityPlayer.TwitchActionsStates.Enabled;
			}
		}
		else if (this.localPlayer.TwitchActionsEnabled != EntityPlayer.TwitchActionsStates.Disabled)
		{
			this.localPlayer.TwitchActionsEnabled = EntityPlayer.TwitchActionsStates.Disabled;
		}
		this.localPlayer.TwitchVisionDisabled = !this.origAllowVisionEffects;
		this.twitchManager.ConnectionStateChanged -= this.TwitchManager_ConnectionStateChanged;
		if (this.twitchManager.CurrentCooldownPreset == null)
		{
			this.twitchManager.GetCooldownMax();
		}
		if (this.startedSinceOpened)
		{
			if (this.twitchManager.CooldownType == TwitchManager.CooldownTypes.Startup && this.twitchManager.CurrentCooldownPreset.StartCooldownTime > 0)
			{
				this.twitchManager.SetCooldown((float)this.twitchManager.CurrentCooldownPreset.StartCooldownTime, TwitchManager.CooldownTypes.Startup, false, false);
			}
			this.twitchManager.InitialCooldownSet = true;
		}
		if (this.twitchManager.InitState == TwitchManager.InitStates.Ready && this.twitchManager.CurrentCooldownPreset.CooldownType != CooldownPreset.CooldownTypes.Fill)
		{
			this.twitchManager.SetCooldown(0f, TwitchManager.CooldownTypes.None, false, false);
		}
		this.twitchManager.ExtensionCheckTime = 0f;
		this.twitchManager = null;
		this.localPlayer = null;
		XUi.InGameMenuOpen = false;
	}

	public override bool GetBindingValue(ref string _value, string _bindingName)
	{
		uint num = <PrivateImplementationDetails>.ComputeStringHash(_bindingName);
		if (num <= 2599571141U)
		{
			if (num <= 1430903321U)
			{
				if (num <= 731463935U)
				{
					if (num != 153500012U)
					{
						if (num != 234580778U)
						{
							if (num == 731463935U)
							{
								if (_bindingName == "connecting_standalone")
								{
									if (this.twitchManager != null)
									{
										if (this.twitchManager.InitState == TwitchManager.InitStates.WaitingForOAuth && (DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX).IsCurrent())
										{
											_value = "true";
										}
										else
										{
											_value = "false";
										}
									}
									else
									{
										_value = "false";
									}
									return true;
								}
							}
						}
						else if (_bindingName == "event_description")
						{
							_value = "";
							if (this.twitchManager != null)
							{
								TwitchEventPreset twitchEventPreset = this.twitchManager.EventPresets[this.comboAllowEvents.SelectedIndex];
								_value = string.Format("[DECEA3]{0}[-]\n{1}", twitchEventPreset.Title, twitchEventPreset.Description);
							}
							return true;
						}
					}
					else if (_bindingName == "hasgiftsubevents")
					{
						if (this.twitchManager != null)
						{
							if (this.twitchManager.InitState == TwitchManager.InitStates.Ready)
							{
								_value = this.twitchManager.EventPresets[this.comboAllowEvents.SelectedIndex].HasGiftSubEvents.ToString();
							}
							else
							{
								_value = "false";
							}
						}
						else
						{
							_value = "false";
						}
						return true;
					}
				}
				else if (num <= 1335533228U)
				{
					if (num != 1004462666U)
					{
						if (num == 1335533228U)
						{
							if (_bindingName == "hascharityevents")
							{
								if (this.twitchManager != null)
								{
									if (this.twitchManager.InitState == TwitchManager.InitStates.Ready)
									{
										_value = this.twitchManager.EventPresets[this.comboAllowEvents.SelectedIndex].HasCharityEvents.ToString();
									}
									else
									{
										_value = "false";
									}
								}
								else
								{
									_value = "false";
								}
								return true;
							}
						}
					}
					else if (_bindingName == "action_description")
					{
						_value = "";
						if (this.twitchManager != null)
						{
							TwitchActionPreset twitchActionPreset = this.twitchManager.ActionPresets[this.comboAllowActions.SelectedIndex];
							_value = string.Format("[DECEA3]{0}[-]\n{1}", twitchActionPreset.Title, twitchActionPreset.Description);
						}
						return true;
					}
				}
				else if (num != 1411810637U)
				{
					if (num == 1430903321U)
					{
						if (_bindingName == "twitchbuttontext")
						{
							if (this.twitchManager != null)
							{
								if (this.twitchManager.InitState == TwitchManager.InitStates.Ready)
								{
									_value = Localization.Get("xuiOptionsTwitchDisconnect", false);
								}
								else
								{
									_value = Localization.Get("xuiOptionsTwitchLoginTwitch", false);
								}
							}
							else
							{
								_value = "";
							}
							return true;
						}
					}
				}
				else if (_bindingName == "notinlinux")
				{
					_value = "true";
					return true;
				}
			}
			else if (num <= 1964364183U)
			{
				if (num <= 1463402542U)
				{
					if (num != 1458362631U)
					{
						if (num == 1463402542U)
						{
							if (_bindingName == "hasraidevents")
							{
								if (this.twitchManager != null)
								{
									if (this.twitchManager.InitState == TwitchManager.InitStates.Ready)
									{
										_value = this.twitchManager.EventPresets[this.comboAllowEvents.SelectedIndex].HasRaidEvents.ToString();
									}
									else
									{
										_value = "false";
									}
								}
								else
								{
									_value = "false";
								}
								return true;
							}
						}
					}
					else if (_bindingName == "hascreatorgoalevents")
					{
						if (this.twitchManager != null)
						{
							if (this.twitchManager.InitState == TwitchManager.InitStates.Ready)
							{
								_value = this.twitchManager.EventPresets[this.comboAllowEvents.SelectedIndex].HasCreatorGoalEvents.ToString();
							}
							else
							{
								_value = "false";
							}
						}
						else
						{
							_value = "false";
						}
						return true;
					}
				}
				else if (num != 1501411403U)
				{
					if (num == 1964364183U)
					{
						if (_bindingName == "notingame")
						{
							_value = (GameStats.GetInt(EnumGameStats.GameState) == 0).ToString();
							return true;
						}
					}
				}
				else if (_bindingName == "hasbitevents")
				{
					if (this.twitchManager != null)
					{
						if (this.twitchManager.InitState == TwitchManager.InitStates.Ready)
						{
							_value = this.twitchManager.EventPresets[this.comboAllowEvents.SelectedIndex].HasBitEvents.ToString();
						}
						else
						{
							_value = "false";
						}
					}
					else
					{
						_value = "false";
					}
					return true;
				}
			}
			else if (num <= 2385092692U)
			{
				if (num != 2360587702U)
				{
					if (num == 2385092692U)
					{
						if (_bindingName == "twitchstatus")
						{
							if (this.twitchManager != null)
							{
								_value = this.twitchManager.StateText;
							}
							else
							{
								_value = "";
							}
							return true;
						}
					}
				}
				else if (_bindingName == "show_devicecode")
				{
					_value = (this.showDeviceCode ? this.Auth_Code : Localization.Get("xuiOptionsTwitchDeviceCodeShow", false));
					return true;
				}
			}
			else if (num != 2575209698U)
			{
				if (num == 2599571141U)
				{
					if (_bindingName == "haschannelpointevents")
					{
						if (this.twitchManager != null)
						{
							if (this.twitchManager.InitState == TwitchManager.InitStates.Ready)
							{
								_value = this.twitchManager.EventPresets[this.comboAllowEvents.SelectedIndex].HasChannelPointEvents.ToString();
							}
							else
							{
								_value = "false";
							}
						}
						else
						{
							_value = "false";
						}
						return true;
					}
				}
			}
			else if (_bindingName == "notconnecting_console")
			{
				if (this.twitchManager != null)
				{
					if (this.twitchManager.InitState == TwitchManager.InitStates.WaitingForOAuth && !(DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX).IsCurrent())
					{
						_value = "false";
					}
					else
					{
						_value = "true";
					}
				}
				else
				{
					_value = "true";
				}
				return true;
			}
		}
		else if (num <= 3009481759U)
		{
			if (num <= 2691102394U)
			{
				if (num != 2603223281U)
				{
					if (num != 2682340352U)
					{
						if (num == 2691102394U)
						{
							if (_bindingName == "onlyconnected")
							{
								if (this.twitchManager != null)
								{
									if (this.twitchManager.InitState == TwitchManager.InitStates.Ready)
									{
										_value = "true";
									}
									else
									{
										_value = "false";
									}
								}
								else
								{
									_value = "false";
								}
								return true;
							}
						}
					}
					else if (_bindingName == "vote_description")
					{
						_value = "";
						if (this.twitchManager != null)
						{
							TwitchVotePreset twitchVotePreset = this.twitchManager.VotePresets[this.comboAllowVotes.SelectedIndex];
							_value = string.Format("[DECEA3]{0}[-]\n{1}", twitchVotePreset.Title, twitchVotePreset.Description);
						}
						return true;
					}
				}
				else if (_bindingName == "connecting_console")
				{
					if (this.twitchManager != null)
					{
						if (this.twitchManager.InitState == TwitchManager.InitStates.WaitingForOAuth && !(DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX).IsCurrent())
						{
							_value = "true";
						}
						else
						{
							_value = "false";
						}
					}
					else
					{
						_value = "false";
					}
					return true;
				}
			}
			else if (num <= 2988540215U)
			{
				if (num != 2790424841U)
				{
					if (num == 2988540215U)
					{
						if (_bindingName == "auth_devicecode")
						{
							_value = this.Auth_Code;
							return true;
						}
					}
				}
				else if (_bindingName == "notconnected")
				{
					if (this.twitchManager != null)
					{
						if (this.twitchManager.InitState == TwitchManager.InitStates.Ready)
						{
							_value = "false";
						}
						else
						{
							_value = "true";
						}
					}
					else
					{
						_value = "true";
					}
					return true;
				}
			}
			else if (num != 2996803900U)
			{
				if (num == 3009481759U)
				{
					if (_bindingName == "allowvotes")
					{
						if (this.twitchManager != null)
						{
							if (this.twitchManager.InitState == TwitchManager.InitStates.Ready)
							{
								_value = (!this.twitchManager.VotePresets[this.comboAllowVotes.SelectedIndex].IsEmpty).ToString();
							}
							else
							{
								_value = "false";
							}
						}
						else
						{
							_value = "false";
						}
						return true;
					}
				}
			}
			else if (_bindingName == "auth_verificationUrl")
			{
				_value = this.Auth_VerificationUrl;
				return true;
			}
		}
		else if (num <= 3519348394U)
		{
			if (num <= 3216530914U)
			{
				if (num != 3177482545U)
				{
					if (num == 3216530914U)
					{
						if (_bindingName == "hassubevents")
						{
							if (this.twitchManager != null)
							{
								if (this.twitchManager.InitState == TwitchManager.InitStates.Ready)
								{
									_value = this.twitchManager.EventPresets[this.comboAllowEvents.SelectedIndex].HasSubEvents.ToString();
								}
								else
								{
									_value = "false";
								}
							}
							else
							{
								_value = "false";
							}
							return true;
						}
					}
				}
				else if (_bindingName == "votedaytimerange")
				{
					_value = "";
					if (this.twitchManager != null)
					{
						_value = this.twitchManager.VotingManager.GetDayTimeRange(this.tempVoteDayTimeRange);
					}
					return true;
				}
			}
			else if (num != 3300710673U)
			{
				if (num == 3519348394U)
				{
					if (_bindingName == "hashypetrainevents")
					{
						if (this.twitchManager != null)
						{
							if (this.twitchManager.InitState == TwitchManager.InitStates.Ready)
							{
								_value = this.twitchManager.EventPresets[this.comboAllowEvents.SelectedIndex].HasHypeTrainEvents.ToString();
							}
							else
							{
								_value = "false";
							}
						}
						else
						{
							_value = "false";
						}
						return true;
					}
				}
			}
			else if (_bindingName == "hascustomevents")
			{
				if (this.twitchManager != null)
				{
					_value = (this.twitchManager.InitState == TwitchManager.InitStates.Ready && this.twitchManager.HasCustomEvents).ToString();
				}
				else
				{
					_value = "false";
				}
				return true;
			}
		}
		else if (num <= 4035104137U)
		{
			if (num != 4027217799U)
			{
				if (num == 4035104137U)
				{
					if (_bindingName == "giftsubvalues")
					{
						_value = "";
						if (this.twitchManager != null)
						{
							_value = this.twitchManager.GetGiftSubTierRewards(this.tempGiftSubModifier);
						}
						return true;
					}
				}
			}
			else if (_bindingName == "allowevents")
			{
				if (this.twitchManager != null)
				{
					if (this.twitchManager.InitState == TwitchManager.InitStates.Ready)
					{
						_value = (!this.twitchManager.EventPresets[this.comboAllowEvents.SelectedIndex].IsEmpty).ToString();
					}
					else
					{
						_value = "false";
					}
				}
				else
				{
					_value = "false";
				}
				return true;
			}
		}
		else if (num != 4109234795U)
		{
			if (num == 4190492937U)
			{
				if (_bindingName == "allowactions")
				{
					if (this.twitchManager != null)
					{
						if (this.twitchManager.InitState == TwitchManager.InitStates.Ready)
						{
							_value = (!this.twitchManager.ActionPresets[this.comboAllowActions.SelectedIndex].IsEmpty).ToString();
						}
						else
						{
							_value = "false";
						}
					}
					else
					{
						_value = "false";
					}
					return true;
				}
			}
		}
		else if (_bindingName == "subvalues")
		{
			_value = "";
			if (this.twitchManager != null)
			{
				_value = this.twitchManager.GetSubTierRewards(this.tempSubModifier);
			}
			return true;
		}
		return base.GetBindingValue(ref _value, _bindingName);
	}

	public static string ID = "";

	public EntityPlayerLocal localPlayer;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TabSelector tabs;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnLoginTwitch;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnShowExtras;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnResetPrices;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnEnableAllExtras;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnDisableAllExtras;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxBool comboOptOutTwitch;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxBool comboUseProgression;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxBool comboAllowCrateSharing;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxEnum<XUiC_OptionsTwitch.TwitchBloodMoonOptions> comboAllowActionsDuringBloodmoon;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxEnum<XUiC_OptionsTwitch.TwitchBloodMoonOptions> comboAllowActionsDuringQuests;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxBool comboAllowVotesDuringBloodmoon;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxBool comboAllowVotesDuringQuests;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxBool comboAllowVotesInSafeZone;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxEnum<XUiC_OptionsTwitch.PointsGenerationOptions> comboPointsGeneration;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<string> comboCooldownPreset;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxBool comboAllowVisionEffects;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<string> comboActionSpamDelay;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxEnum<TwitchManager.IntegrationSettings> comboActionTwitchIntegrationType;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<string> comboAllowActions;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<string> comboAllowVotes;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<string> comboAllowEvents;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<string> comboViewerStartingPoints;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxEnum<XUiC_OptionsTwitch.BitAddOptions> comboBitPointsAdd;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxEnum<XUiC_OptionsTwitch.PointsGenerationOptions> comboSubPointsAdd;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxEnum<XUiC_OptionsTwitch.PointsGenerationOptions> comboGiftSubPointsAdd;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<string> comboRaidPointsAdd;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<string> comboRaidViewerMinimum;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<string> comboMaxDailyVotes;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<string> comboVoteTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<string> comboVoteViewerDefeatReward;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<string> comboVoteDayTimeRange;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<string> comboHelperRewardAmount;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<string> comboHelperRewardTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxBool comboAllowBitEvents;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxBool comboAllowSubEvents;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxBool comboAllowGiftSubEvents;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxBool comboAllowCharityEvents;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxBool comboAllowRaidEvents;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxBool comboAllowHypeTrainEvents;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxBool comboAllowCreatorGoalEvents;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxBool comboAllowChannelPointRedeemEvents;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<string> comboBitPrices;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<string> comboBitPotPercent;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_OptionsTwitch.TwitchBloodMoonOptions origAllowActionsDuringBloodmoon;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_OptionsTwitch.TwitchBloodMoonOptions origAllowActionsDuringQuests;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_OptionsTwitch.PointsGenerationOptions origPointsGeneration;

	[PublicizedFrom(EAccessModifier.Private)]
	public int origCooldownPresetIndex;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool origUseProgression;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool origAllowTwitchOptions;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool origAllowVisionEffects = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool origAllowCrateSharing;

	[PublicizedFrom(EAccessModifier.Private)]
	public int origActionSpamDelay = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	public TwitchManager.IntegrationSettings origIntegrationSetting;

	[PublicizedFrom(EAccessModifier.Private)]
	public int origActionPresetIndex;

	[PublicizedFrom(EAccessModifier.Private)]
	public int origVotePresetIndex;

	[PublicizedFrom(EAccessModifier.Private)]
	public int origEventPresetIndex;

	[PublicizedFrom(EAccessModifier.Private)]
	public int origBitPricesIndex;

	[PublicizedFrom(EAccessModifier.Private)]
	public int origBitPotPercentIndex;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_OptionsTwitch.BitAddOptions origBitPointModifier;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_OptionsTwitch.PointsGenerationOptions origSubPointModifier;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_OptionsTwitch.PointsGenerationOptions origGiftSubPointModifier;

	[PublicizedFrom(EAccessModifier.Private)]
	public int origRaidPointAmountIndex;

	[PublicizedFrom(EAccessModifier.Private)]
	public int origRaidViewerMinimumIndex;

	[PublicizedFrom(EAccessModifier.Private)]
	public int origHelperRewardAmountIndex;

	[PublicizedFrom(EAccessModifier.Private)]
	public int origHelperRewardTimeIndex;

	[PublicizedFrom(EAccessModifier.Private)]
	public int origViewerStartingPoints;

	[PublicizedFrom(EAccessModifier.Private)]
	public int origMaxDailyVotes;

	[PublicizedFrom(EAccessModifier.Private)]
	public int origVoteTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public int origVoteDayTimeRange;

	[PublicizedFrom(EAccessModifier.Private)]
	public int origVoteViewerDefeatReward;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool origAllowVotesDuringBloodmoon;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool origAllowVotesDuringQuests;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool origAllowVotesInSafeZone;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool startedSinceOpened;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnBack;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnDefaults;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnApply;

	public TwitchManager twitchManager;

	[PublicizedFrom(EAccessModifier.Private)]
	public int tempSubModifier = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	public int tempGiftSubModifier = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	public int tempVoteDayTimeRange = 2;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool origAllowBitEvents = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool origAllowSubEvents = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool origAllowGiftSubEvents = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool origAllowCharityEvents = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool origAllowRaidEvents = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool origAllowHypeTrainEvents = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool origAllowCreatorGoalEvents = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool origAllowChannelPointRedemptions = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool showDeviceCode;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Texture qrCodeTexControl;

	[PublicizedFrom(EAccessModifier.Private)]
	public string Auth_Code = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public string Auth_VerificationUrl = "";

	public enum TwitchBloodMoonOptions
	{
		Disabled,
		Standard,
		CooldownOnly
	}

	public enum PointsGenerationOptions
	{
		Disabled,
		Standard,
		Double,
		Triple
	}

	public enum BitAddOptions
	{
		Standard = 1,
		Double,
		Triple
	}
}
