using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using Audio;
using Challenges;
using Platform;
using Twitch.PubSub;
using UniLinq;
using UnityEngine;

namespace Twitch
{
	public class TwitchManager
	{
		public static TwitchManager Current
		{
			get
			{
				if (TwitchManager.instance == null)
				{
					TwitchManager.instance = new TwitchManager();
				}
				return TwitchManager.instance;
			}
		}

		public static bool HasInstance
		{
			get
			{
				return TwitchManager.instance != null;
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public TwitchManager()
		{
			this.ViewerData = new TwitchViewerData(this);
			this.VotingManager = new TwitchVotingManager(this);
			this.HighestGameStage = -1;
			this.UseProgression = true;
		}

		public void Cleanup()
		{
			this.Disconnect();
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && this.InitState == TwitchManager.InitStates.Ready)
			{
				this.SaveViewerData();
			}
			TwitchManager.instance = null;
		}

		public byte CurrentFileVersion { get; set; }

		public byte CurrentMainFileVersion { get; set; }

		public bool OverrideProgession
		{
			get
			{
				return this.overrideProgression;
			}
			set
			{
				if (this.overrideProgression != value)
				{
					this.overrideProgression = value;
					if (this.InitState == TwitchManager.InitStates.Ready)
					{
						this.resetCommandsNeeded = true;
					}
				}
			}
		}

		public bool UseProgression { get; [PublicizedFrom(EAccessModifier.Private)] set; }

		public int HighestGameStage { get; [PublicizedFrom(EAccessModifier.Private)] set; }

		public static bool BossHordeActive
		{
			get
			{
				return TwitchManager.HasInstance && TwitchManager.Current.IsBossHordeActive;
			}
		}

		public TwitchManager.IntegrationSettings IntegrationSetting
		{
			get
			{
				return this.integrationSetting;
			}
			set
			{
				if (this.integrationSetting != value)
				{
					this.integrationSetting = value;
					this.IntegrationTypeChanged();
				}
			}
		}

		public float ActionCooldownModifier
		{
			get
			{
				return this.actionCooldownModifier;
			}
			set
			{
				if (value != this.actionCooldownModifier)
				{
					this.actionCooldownModifier = value;
					this.UpdateActionCooldowns(value);
				}
			}
		}

		public bool AllowActions
		{
			get
			{
				return !this.CurrentActionPreset.IsEmpty;
			}
		}

		public bool AllowEvents
		{
			get
			{
				return !this.CurrentEventPreset.IsEmpty;
			}
		}

		public bool OnCooldown
		{
			get
			{
				return this.CooldownTime > 0f || this.CurrentCooldownPreset.CooldownType == CooldownPreset.CooldownTypes.Always;
			}
		}

		public float BitPriceMultiplier
		{
			get
			{
				return this.bitPriceMultiplier;
			}
			set
			{
				if (this.bitPriceMultiplier != value)
				{
					this.bitPriceMultiplier = value;
					this.ResetPrices();
				}
			}
		}

		public TwitchVotingManager VotingManager { get; [PublicizedFrom(EAccessModifier.Private)] set; }

		public TwitchViewerData ViewerData { get; [PublicizedFrom(EAccessModifier.Private)] set; }

		public string BroadcasterType
		{
			get
			{
				return this.broadcasterType;
			}
			set
			{
				this.broadcasterType = value;
				this.UIDirty = true;
				if (this.CommandsChanged != null)
				{
					this.CommandsChanged();
				}
			}
		}

		public TwitchManager.InitStates InitState
		{
			get
			{
				return this.initState;
			}
			[PublicizedFrom(EAccessModifier.Private)]
			set
			{
				if (this.initState != value)
				{
					TwitchManager.InitStates oldState = this.initState;
					this.initState = value;
					if (this.ConnectionStateChanged != null)
					{
						this.ConnectionStateChanged(oldState, this.initState);
					}
				}
			}
		}

		public string StateText
		{
			get
			{
				switch (this.initState)
				{
				case TwitchManager.InitStates.Setup:
				case TwitchManager.InitStates.WaitingForOAuth:
				case TwitchManager.InitStates.Authenticating:
				case TwitchManager.InitStates.Authenticated:
					return Localization.Get("xuiTwitchStatus_Connecting", false);
				case TwitchManager.InitStates.WaitingForPermission:
					return Localization.Get("xuiTwitchStatus_RequestPermission", false);
				case TwitchManager.InitStates.PermissionDenied:
					return Localization.Get("xuiTwitchStatus_PermissionDenied", false);
				case TwitchManager.InitStates.Ready:
					return string.Format(Localization.Get("xuiTwitchStatus_Connected", false), this.Authentication.userName);
				case TwitchManager.InitStates.ExtensionNotInstalled:
					return Localization.Get("xuiTwitchStatus_ExtensionDenied", false);
				case TwitchManager.InitStates.Failed:
					return Localization.Get("xuiTwitchStatus_ConnectionFailed", false);
				}
				return "";
			}
		}

		public bool IsReady
		{
			get
			{
				return this.initState == TwitchManager.InitStates.Ready;
			}
		}

		public bool IsVoting
		{
			get
			{
				return this.initState == TwitchManager.InitStates.Ready && this.VotingManager.VotingIsActive;
			}
		}

		public bool IsBossHordeActive
		{
			get
			{
				return this.initState == TwitchManager.InitStates.Ready && (this.VotingManager.CurrentVoteState == TwitchVotingManager.VoteStateTypes.EventActive || this.VotingManager.CurrentVoteState == TwitchVotingManager.VoteStateTypes.WaitingForActive);
			}
		}

		public bool ReadyForVote
		{
			get
			{
				return this.actionSpawnLiveList.Count == 0;
			}
		}

		public bool IsSafe { get; [PublicizedFrom(EAccessModifier.Private)] set; }

		public event OnTwitchConnectionStateChange ConnectionStateChanged;

		public event OnCommandsChanged CommandsChanged;

		public event OnHistoryAdded ActionHistoryAdded;

		public event OnHistoryAdded VoteHistoryAdded;

		public event OnHistoryAdded EventHistoryAdded;

		public bool HasCustomEvents
		{
			get
			{
				return this.EventPresets.Count > 0;
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void SetupLocalization()
		{
			this.chatOutput_ActivatedAction = Localization.Get("TwitchChat_ActivatedAction", false);
			this.chatOutput_ActivatedBitAction = Localization.Get("TwitchChat_ActivatedBitAction", false);
			this.chatOutput_BitCredits = Localization.Get("TwitchChat_BitCredits", false);
			this.chatOutput_BitEvent = Localization.Get("TwitchChat_BitEvent", false);
			this.chatOutput_BitPotBalance = Localization.Get("TwitchChat_BitPotBalance", false);
			this.chatOutput_ChannelPointEvent = Localization.Get("TwitchChat_ChannelPointEvent", false);
			this.chatOutput_CharityEvent = Localization.Get("TwitchChat_CharityEvent", false);
			this.chatOutput_Commands = Localization.Get("TwitchChat_Commands", false);
			this.chatOutput_CooldownComplete = Localization.Get("TwitchChat_CooldownComplete", false);
			this.chatOutput_CooldownStarted = Localization.Get("TwitchChat_CooldownStarted", false);
			this.chatOutput_CooldownTime = Localization.Get("TwitchChat_CooldownTime", false);
			this.chatOutput_CreatorGoalEvent = Localization.Get("TwitchChat_CreatorGoalEvent", false);
			this.chatOutput_DonateBits = Localization.Get("TwitchChat_DonateBits", false);
			this.chatOutput_DonateCharity = Localization.Get("TwitchChat_DonateCharity", false);
			this.chatOutput_Gamestage = Localization.Get("TwitchChat_Gamestage", false);
			this.chatOutput_GiftSubEvent = Localization.Get("TwitchChat_GiftedSubEvent", false);
			this.chatOutput_GiftSubs = Localization.Get("TwitchChat_GiftedSubs", false);
			this.chatOutput_HypeTrainEvent = Localization.Get("TwitchChat_HypeTrainEvent", false);
			this.chatOutput_KilledParty = Localization.Get("TwitchChat_KilledParty", false);
			this.chatOutput_KilledStreamer = Localization.Get("TwitchChat_KilledStreamer", false);
			this.chatOutput_KilledByBits = Localization.Get("TwitchChat_KilledByBits", false);
			this.chatOutput_KilledByHypeTrain = Localization.Get("TwitchChat_KilledByHypeTrain", false);
			this.chatOutput_KilledByVote = Localization.Get("TwitchChat_KilledByVote", false);
			this.chatOutput_NewActions = Localization.Get("TwitchChat_NewActions", false);
			this.chatOutput_PimpPotBalance = Localization.Get("TwitchChat_PimpPotBalance", false);
			this.chatOutput_PointsWithSpecial = Localization.Get("TwitchChat_PointsWithSpecial", false);
			this.chatOutput_PointsWithoutSpecial = Localization.Get("TwitchChat_PointsWithoutSpecial", false);
			this.chatOutput_QueuedBitAction = Localization.Get("TwitchChat_QueuedBitAction", false);
			this.chatOutput_RaidEvent = Localization.Get("TwitchChat_RaidEvent", false);
			this.chatOutput_RaidPoints = Localization.Get("TwitchChat_RaidPoints", false);
			this.chatOutput_SubEvent = Localization.Get("TwitchChat_SubEvent", false);
			this.chatOutput_Subscribed = Localization.Get("TwitchChat_Subscribed", false);
			this.ingameOutput_ActivatedAction = Localization.Get("TwitchInGame_ActivatedAction", false);
			this.ingameOutput_BitRespawns = Localization.Get("TwitchInGame_BitRespawns", false);
			this.ingameOutput_DonateBits = Localization.Get("TwitchInGame_DonateBits", false);
			this.ingameOutput_DonateCharity = Localization.Get("TwitchInGame_DonateCharity", false);
			this.ingameOutput_GiftSubs = Localization.Get("TwitchInGame_GiftedSubs", false);
			this.ingameOutput_KilledParty = Localization.Get("TwitchInGame_KilledParty", false);
			this.ingameOutput_KilledStreamer = Localization.Get("TwitchInGame_KilledStreamer", false);
			this.ingameOutput_KilledByBits = Localization.Get("TwitchInGame_KilledByBits", false);
			this.ingameOutput_KilledByHypeTrain = Localization.Get("TwitchInGame_KilledByHypeTrain", false);
			this.ingameOutput_KilledByVote = Localization.Get("TwitchInGame_KilledByVote", false);
			this.ingameOutput_RaidPoints = Localization.Get("TwitchInGame_RaidPoints", false);
			this.ingameOutput_RefundedAction = Localization.Get("TwitchInGame_RefundedAction", false);
			this.ingameOutput_Subscribed = Localization.Get("TwitchInGame_Subscribed", false);
			this.ingameDeathScreen_Message = Localization.Get("TwitchDeathMessage", false);
			this.ingameBitsDeathScreen_Message = Localization.Get("TwitchBitsDeathMessage", false);
			this.ingameHypeTrainDeathScreen_Message = Localization.Get("TwitchHypeTrainDeathMessage", false);
			this.ingameVoteDeathScreen_Message = Localization.Get("TwitchVoteDeathMessage", false);
			this.subPointDisplay = Localization.Get("xuiOptionsTwitchSubPointDisplay", false);
			this.ViewerData.SetupLocalization();
			this.VotingManager.SetupLocalization();
			TwitchManager.LeaderboardStats.SetupLocalization();
		}

		public void SetupClient(string twitchChannel, string password)
		{
			this.ircClient = new TwitchIRCClient("irc.twitch.tv", 6667, twitchChannel, password);
			if (this.PubSub == null)
			{
				this.PubSub = new TwitchPubSub();
			}
		}

		public void IntegrationTypeChanged()
		{
			if (this.IsReady && this.extensionManager == null)
			{
				this.extensionManager = new ExtensionManager();
				this.extensionManager.Init();
			}
		}

		public void CleanupData()
		{
			BaseTwitchCommand.ClearCommandPermissionOverrides();
			this.VotingManager.CleanupData();
			this.CooldownPresets.Clear();
			this.tipTitleList.Clear();
			this.tipDescriptionList.Clear();
			this.ActionPresets.Clear();
			this.VotePresets.Clear();
			this.CurrentActionPreset = null;
			this.CurrentVotePreset = null;
			this.CleanupEventData();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void RemoveChannelPointRedeems()
		{
			if (this.CurrentEventPreset != null)
			{
				this.CurrentEventPreset.RemoveChannelPointRedemptions(null);
			}
		}

		public void CleanupEventData()
		{
			this.RemoveChannelPointRedeems();
			this.CurrentEventPreset = null;
			this.EventPresets.Clear();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void Disconnect()
		{
			this.RemoveChannelPointRedeems();
			if (this.PubSub != null)
			{
				this.PubSub.Disconnect();
				this.PubSub.Cleanup();
				this.PubSub = null;
			}
			if (this.ircClient != null)
			{
				this.ircClient.Disconnect();
				this.ircClient = null;
			}
			if (this.extensionManager != null)
			{
				this.extensionManager.Cleanup();
				this.extensionManager = null;
			}
			this.Authentication = null;
			if (this.LocalPlayer != null && this.LocalPlayer.PlayerUI != null && this.LocalPlayer.PlayerUI.windowManager.IsWindowOpen("twitch"))
			{
				this.LocalPlayer.PlayerUI.windowManager.Close("twitch");
			}
		}

		public void AddRandomGroup(string name, int randomCount)
		{
			if (!this.randomGroups.ContainsKey(name))
			{
				this.randomGroups.Add(name, new TwitchRandomActionGroup
				{
					Name = name,
					RandomCount = randomCount
				});
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void ResetDailyCommands(int currentDay, int lastGameStage = -1)
		{
			this.randomKeys.Clear();
			foreach (string key in this.AvailableCommands.Keys)
			{
				TwitchAction twitchAction = this.AvailableCommands[key];
				if (twitchAction.IsInPreset(this.CurrentActionPreset))
				{
					if (twitchAction.RandomDaily)
					{
						if (!this.randomKeys.ContainsKey(twitchAction.RandomGroup))
						{
							this.randomKeys.Add(twitchAction.RandomGroup, new List<TwitchAction>());
						}
						this.randomKeys[twitchAction.RandomGroup].Add(twitchAction);
					}
					else if (twitchAction.SingleDayUse)
					{
						twitchAction.AllowedDay = currentDay;
					}
				}
			}
			foreach (string key2 in this.randomKeys.Keys)
			{
				int num = 1;
				if (this.randomGroups.ContainsKey(key2))
				{
					num = this.randomGroups[key2].RandomCount;
				}
				List<TwitchAction> list = this.randomKeys[key2];
				if (lastGameStage != -1)
				{
					bool flag = false;
					for (int i = 0; i < list.Count; i++)
					{
						if (list[i].StartGameStage > lastGameStage)
						{
							flag = true;
							break;
						}
					}
					if (!flag)
					{
						continue;
					}
				}
				for (int j = 0; j < list.Count; j++)
				{
					int index = UnityEngine.Random.Range(0, list.Count);
					int index2 = UnityEngine.Random.Range(0, list.Count);
					TwitchAction value = list[index];
					list[index] = list[index2];
					list[index2] = value;
				}
				for (int k = 0; k < list.Count; k++)
				{
					list[k].AllowedDay = ((k < num) ? currentDay : -1);
				}
			}
			if (this.CommandsChanged != null)
			{
				this.CommandsChanged();
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void SetupTwitchCommands()
		{
			this.TwitchCommandList.Clear();
			this.TwitchCommandList.Add(new TwitchCommandAddBitCredit());
			this.TwitchCommandList.Add(new TwitchCommandAddPoints());
			this.TwitchCommandList.Add(new TwitchCommandAddSpecialPoints());
			this.TwitchCommandList.Add(new TwitchCommandCheckCredit());
			this.TwitchCommandList.Add(new TwitchCommandCheckPoints());
			this.TwitchCommandList.Add(new TwitchCommandCommands());
			this.TwitchCommandList.Add(new TwitchCommandDebug());
			this.TwitchCommandList.Add(new TwitchCommandDisableCommand());
			this.TwitchCommandList.Add(new TwitchCommandGamestage());
			this.TwitchCommandList.Add(new TwitchCommandPauseCommand());
			this.TwitchCommandList.Add(new TwitchCommandUnpauseCommand());
			this.TwitchCommandList.Add(new TwitchCommandRemoveViewer());
			this.TwitchCommandList.Add(new TwitchCommandResetCooldowns());
			this.TwitchCommandList.Add(new TwitchCommandSetBitPot());
			this.TwitchCommandList.Add(new TwitchCommandSetCooldown());
			this.TwitchCommandList.Add(new TwitchCommandSetPot());
			this.TwitchCommandList.Add(new TwitchCommandTeleportBackpack());
			if (this.CurrentEventPreset.HasBitEvents && this.AllowBitEvents)
			{
				this.TwitchCommandList.Add(new TwitchCommandRedeemBits());
			}
			if (this.CurrentEventPreset.HasSubEvents && this.AllowSubEvents)
			{
				this.TwitchCommandList.Add(new TwitchCommandRedeemSub());
			}
			if (this.CurrentEventPreset.HasGiftSubEvents && this.AllowGiftSubEvents)
			{
				this.TwitchCommandList.Add(new TwitchCommandRedeemGiftSub());
			}
			if (this.CurrentEventPreset.HasRaidEvents && this.AllowRaidEvents)
			{
				this.TwitchCommandList.Add(new TwitchCommandRedeemRaid());
			}
			if (this.CurrentEventPreset.HasCharityEvents && this.AllowCharityEvents)
			{
				this.TwitchCommandList.Add(new TwitchCommandRedeemCharity());
			}
			if (this.CurrentEventPreset.HasHypeTrainEvents && this.AllowHypeTrainEvents)
			{
				this.TwitchCommandList.Add(new TwitchCommandRedeemHypeTrain());
			}
			if (this.CurrentEventPreset.HasCreatorGoalEvents && this.AllowCreatorGoalEvents)
			{
				this.TwitchCommandList.Add(new TwitchCommandRedeemCreatorGoal());
			}
			this.TwitchCommandList.Add(new TwitchCommandUseProgression());
		}

		public void StartTwitchIntegration()
		{
			this.updateTime = 60f;
			try
			{
				if (this.Authentication == null)
				{
					this.Authentication = new TwitchAuthentication();
				}
				if ((DeviceFlags.Current & (DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX)) == DeviceFlags.Current)
				{
					this.Authentication.StopListener();
				}
				this.Authentication.GetToken();
			}
			catch (Exception ex)
			{
				Log.Out("Twitch integration failed to start with message " + ex.Message);
				this.updateTime = 5f;
			}
			this.InitialCooldownSet = false;
			this.InitState = TwitchManager.InitStates.WaitingForOAuth;
		}

		public void StopTwitchIntegration(TwitchManager.InitStates initState = TwitchManager.InitStates.None)
		{
			this.resetClientAttempts = 0;
			this.Disconnect();
			this.TwitchDisconnectPartyUpdate();
			this.ClearEventHandlers();
			if (this.LocalPlayer != null)
			{
				this.LocalPlayer.TwitchEnabled = false;
				this.LocalPlayer.TwitchActionsEnabled = EntityPlayer.TwitchActionsStates.Enabled;
			}
			if (this.Authentication != null && (DeviceFlags.Current & (DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX)) == DeviceFlags.Current)
			{
				this.Authentication.StopListener();
			}
			this.InitState = initState;
		}

		public void WaitForOAuth()
		{
			this.updateTime = 10f;
			this.InitState = TwitchManager.InitStates.WaitingForOAuth;
		}

		public void WaitForPermission()
		{
			this.updateTime = 10f;
			this.InitState = TwitchManager.InitStates.WaitingForPermission;
		}

		public void DeniedPermission()
		{
			this.InitState = TwitchManager.InitStates.PermissionDenied;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void ClearEventHandlers()
		{
			GameEventManager gameEventManager = GameEventManager.Current;
			gameEventManager.GameEntitySpawned -= this.Current_GameEntitySpawned;
			gameEventManager.GameEntityDespawned -= this.Current_GameEntityDespawned;
			gameEventManager.GameEntityKilled -= this.Current_GameEntityKilled;
			gameEventManager.GameBlocksAdded -= this.Current_GameBlocksAdded;
			gameEventManager.GameBlocksRemoved -= this.Current_GameBlocksRemoved;
			gameEventManager.GameBlockRemoved -= this.Current_GameBlockRemoved;
			gameEventManager.GameEventApproved -= this.Current_GameEventApproved;
			gameEventManager.TwitchPartyGameEventApproved -= this.Current_TwitchPartyGameEventApproved;
			gameEventManager.TwitchRefundNeeded -= this.Current_TwitchRefundNeeded;
			gameEventManager.GameEventDenied -= this.Current_GameEventDenied;
			gameEventManager.GameEventCompleted -= this.Current_GameEventCompleted;
			if (this.LocalPlayer != null)
			{
				this.LocalPlayer.PartyLeave -= this.LocalPlayer_PartyLeave;
				this.LocalPlayer.PartyJoined -= this.LocalPlayer_PartyJoined;
				this.LocalPlayer.PartyChanged -= this.LocalPlayer_PartyChanged;
				if (this.LocalPlayer.Party != null)
				{
					this.LocalPlayer.Party.PartyMemberAdded -= this.Party_PartyMemberAdded;
					this.LocalPlayer.Party.PartyMemberRemoved -= this.Party_PartyMemberRemoved;
				}
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void Current_TwitchPartyGameEventApproved(string gameEventID, int targetEntityID, string extraData, string tag)
		{
			foreach (TwitchAction twitchAction in TwitchActionManager.TwitchActions.Values)
			{
				if (twitchAction.IsInPreset(this.CurrentActionPreset) && twitchAction.EventName == gameEventID)
				{
					this.AddCooldownForAction(twitchAction);
					break;
				}
			}
		}

		public void Update(float deltaTime)
		{
			GameManager gameManager = GameManager.Instance;
			if (gameManager.World == null || gameManager.World.Players == null || gameManager.World.Players.Count == 0)
			{
				return;
			}
			this.CurrentUnityTime = Time.time;
			switch (this.InitState)
			{
			case TwitchManager.InitStates.Setup:
			{
				GameEventManager gameEventManager = GameEventManager.Current;
				gameEventManager.GameEventAccessApproved -= this.Current_GameEventAccessApproved;
				gameEventManager.GameEventAccessApproved += this.Current_GameEventAccessApproved;
				this.SetupLocalization();
				if (!this.isLoaded)
				{
					this.isLoaded = true;
					this.LoadViewerData();
				}
				if (!this.LoadLatestMainViewerData() && !this.LoadMainViewerData())
				{
					this.LoadSpecialViewerData();
				}
				this.InitState = TwitchManager.InitStates.None;
				break;
			}
			case TwitchManager.InitStates.WaitingForPermission:
				this.updateTime -= deltaTime;
				if (this.updateTime <= 0f)
				{
					this.StopTwitchIntegration(TwitchManager.InitStates.None);
					Log.Warning("Twitch: login failed in " + this.InitState.ToString() + " state");
					this.InitState = TwitchManager.InitStates.Failed;
					return;
				}
				break;
			case TwitchManager.InitStates.WaitingForOAuth:
				this.updateTime -= deltaTime;
				if (this.updateTime <= 0f)
				{
					this.StopTwitchIntegration(TwitchManager.InitStates.None);
					Log.Warning("Twitch: Login failed in " + this.InitState.ToString() + " state");
					this.InitState = TwitchManager.InitStates.Failed;
					return;
				}
				if (this.Authentication.oauth != "" && this.Authentication.userName != "" && this.Authentication.userID != "")
				{
					this.SetupClient(this.Authentication.userName, this.Authentication.oauth);
					this.PubSub.OnChannelPointsRedeemed -= this.PubSub_OnChannelPointsRedeemed;
					this.PubSub.OnChannelPointsRedeemed += this.PubSub_OnChannelPointsRedeemed;
					this.PubSub.OnBitsRedeemed -= this.PubSub_OnBitsRedeemed;
					this.PubSub.OnBitsRedeemed += this.PubSub_OnBitsRedeemed;
					this.PubSub.OnSubscriptionRedeemed -= this.PubSub_OnSubscriptionRedeemed;
					this.PubSub.OnSubscriptionRedeemed += this.PubSub_OnSubscriptionRedeemed;
					this.PubSub.OnGoalAchieved -= this.PubSub_OnGoalAchieved;
					this.PubSub.OnGoalAchieved += this.PubSub_OnGoalAchieved;
					this.PubSub.Connect(this.Authentication.userID);
					this.updateTime = 3f;
					Log.Out("retrieved oauth. Waiting for IRC to post auth...");
					this.InitState = TwitchManager.InitStates.Authenticating;
				}
				break;
			case TwitchManager.InitStates.Authenticating:
				this.updateTime -= deltaTime;
				if (this.updateTime <= 0f)
				{
					if (this.Authentication.oauth != "" && this.Authentication.userName != "" && this.Authentication.userID != "")
					{
						int num = this.resetClientAttempts;
						this.resetClientAttempts = num + 1;
						if (num < 5)
						{
							this.SetupClient(this.Authentication.userName, this.Authentication.oauth);
							this.updateTime = 2.5f;
							Log.Out("attempting to reset client...");
							break;
						}
					}
					Log.Warning("Twitch: Login failed in " + this.InitState.ToString() + " state");
					this.StopTwitchIntegration(TwitchManager.InitStates.Failed);
					this.InitState = TwitchManager.InitStates.Failed;
					return;
				}
				break;
			case TwitchManager.InitStates.Authenticated:
			{
				this.ClearEventHandlers();
				GameEventManager gameEventManager2 = GameEventManager.Current;
				gameEventManager2.GameEntitySpawned += this.Current_GameEntitySpawned;
				gameEventManager2.GameEntityDespawned += this.Current_GameEntityDespawned;
				gameEventManager2.GameEntityKilled += this.Current_GameEntityKilled;
				gameEventManager2.GameBlocksAdded += this.Current_GameBlocksAdded;
				gameEventManager2.GameBlockRemoved += this.Current_GameBlockRemoved;
				gameEventManager2.GameBlocksRemoved += this.Current_GameBlocksRemoved;
				gameEventManager2.GameEventApproved += this.Current_GameEventApproved;
				gameEventManager2.TwitchPartyGameEventApproved += this.Current_TwitchPartyGameEventApproved;
				gameEventManager2.TwitchRefundNeeded += this.Current_TwitchRefundNeeded;
				gameEventManager2.GameEventDenied += this.Current_GameEventDenied;
				gameEventManager2.GameEventCompleted += this.Current_GameEventCompleted;
				this.world = gameManager.World;
				if (this.extensionManager == null)
				{
					this.extensionManager = new ExtensionManager();
					this.extensionManager.Init();
				}
				this.InitState = TwitchManager.InitStates.Ready;
				QuestEventManager.Current.TwitchEventReceived(TwitchObjectiveTypes.Enabled, "");
				break;
			}
			case TwitchManager.InitStates.CheckingForExtension:
				if (!this.AllowActions)
				{
					this.InitState = TwitchManager.InitStates.Authenticated;
				}
				else if (!this.checkingExtensionInstalled)
				{
					this.checkingExtensionInstalled = true;
					ExtensionManager.CheckExtensionInstalled(delegate(bool IsInstalled)
					{
						this.checkingExtensionInstalled = false;
						if (!IsInstalled)
						{
							XUiC_MessageBoxWindowGroup.ShowMessageBox(this.LocalPlayerXUi, Localization.Get("xuiTwitchPopup_ExtensionNeededHeader", false), Localization.Get("xuiTwitchPopup_ExtensionNeeded", false), XUiC_MessageBoxWindowGroup.MessageBoxTypes.Ok, null, null, true, true);
							Application.OpenURL("https://dashboard.twitch.tv/extensions/k6ji189bf7i4ge8il4iczzw7kpgmjt");
						}
						this.InitState = TwitchManager.InitStates.Authenticated;
					});
				}
				break;
			case TwitchManager.InitStates.Ready:
				if (this.LocalPlayer == null)
				{
					this.SetupTwitchCommands();
					this.LocalPlayer = (XUiM_Player.GetPlayer() as EntityPlayerLocal);
					this.RefreshPartyInfo();
					this.HighestGameStage = this.LocalPlayer.unModifiedGameStage;
					this.ActionMessages.Clear();
					this.GetCooldownMax();
					this.SetCooldown((float)this.CurrentCooldownPreset.NextCooldownTime, TwitchManager.CooldownTypes.Startup, false, false);
					this.LocalPlayer.PartyLeave += this.LocalPlayer_PartyLeave;
					this.LocalPlayer.PartyJoined += this.LocalPlayer_PartyJoined;
					this.LocalPlayer.PartyChanged += this.LocalPlayer_PartyChanged;
					if (this.LocalPlayer.Party != null)
					{
						this.LocalPlayer.Party.PartyMemberAdded += this.Party_PartyMemberAdded;
						this.LocalPlayer.Party.PartyMemberRemoved += this.Party_PartyMemberRemoved;
					}
					if (!this.InitialCooldownSet)
					{
						if (this.CurrentCooldownPreset.StartCooldownTime > 0)
						{
							this.SetCooldown(100000f, TwitchManager.CooldownTypes.Startup, false, true);
						}
						if (this.CurrentCooldownPreset.CooldownType != CooldownPreset.CooldownTypes.Fill)
						{
							this.SetCooldown(0f, TwitchManager.CooldownTypes.None, false, false);
						}
						this.CurrentActionPreset.HandleCooldowns();
						this.InitialCooldownSet = true;
					}
				}
				this.LocalPlayer.TwitchEnabled = true;
				this.LocalPlayerInLandClaim = GameManager.Instance.World.GetLandClaimOwnerInParty(this.LocalPlayer, this.LocalPlayer.persistentPlayerData);
				if (!this.ircClient.IsConnected)
				{
					Log.Out("Reached 'Ready' but waiting for IRC to post auth message...");
					this.ircClient.Reconnect();
					this.InitState = TwitchManager.InitStates.Authenticating;
					this.updateTime = 30f;
				}
				TwitchManager.LeaderboardStats.UpdateStats(deltaTime);
				if (this.resetCommandsNeeded)
				{
					this.ResetCommands();
				}
				if (!XUi.InGameMenuOpen && this.AllowActions && this.ExtensionCheckTime < 0f)
				{
					this.ExtensionCheckTime = 30f;
					ExtensionManager.CheckExtensionInstalled(delegate(bool IsInstalled)
					{
						if (IsInstalled)
						{
							this.extensionActiveCheckFailures = 0;
							return;
						}
						if (this.extensionActiveCheckFailures < 3)
						{
							this.extensionActiveCheckFailures++;
							return;
						}
						this.extensionActiveCheckFailures = 0;
						LocalPlayerUI uiforPlayer = LocalPlayerUI.GetUIForPlayer(this.LocalPlayer);
						this.ircClient.SendChannelMessage(Localization.Get("TwitchChat_ExtensionNotInstalled", false), false);
						XUiC_ChatOutput.AddMessage(uiforPlayer.xui, EnumGameMessages.PlainTextLocal, EChatType.Global, Localization.Get("TwitchChat_ExtensionNotInstalled", false), -1, EMessageSender.None, GeneratedTextManager.TextFilteringMode.None);
						this.StopTwitchIntegration(TwitchManager.InitStates.None);
						this.InitState = TwitchManager.InitStates.ExtensionNotInstalled;
						Application.OpenURL("https://dashboard.twitch.tv/extensions/k6ji189bf7i4ge8il4iczzw7kpgmjt");
					});
				}
				this.ExtensionCheckTime -= deltaTime;
				if (this.LocalPlayer.Buffs.HasBuff("twitch_extensionneeded"))
				{
					this.updateTime -= deltaTime;
					if (this.updateTime <= 0f)
					{
						ExtensionManager.CheckExtensionInstalled(delegate(bool IsInstalled)
						{
							if (IsInstalled)
							{
								this.LocalPlayer.Buffs.RemoveBuff("twitch_extensionneeded", true);
							}
						});
						this.updateTime = 5f;
					}
				}
				break;
			}
			if (this.extensionManager != null)
			{
				if (this.extensionManager.HasCommand())
				{
					ExtensionAction command = this.extensionManager.GetCommand();
					int userId = int.Parse(command.username);
					string command2 = command.command;
					bool isRerun = false;
					int creditUsed = command.creditUsed;
					ExtensionBitAction extensionBitAction = command as ExtensionBitAction;
					this.HandleExtensionMessage(userId, command2, isRerun, creditUsed, (extensionBitAction != null) ? extensionBitAction.cost : 0);
				}
				this.extensionManager.Update();
			}
			if (this.ircClient != null)
			{
				this.ircClient.Update(deltaTime);
				if (this.ircClient.AvailableMessage())
				{
					this.HandleMessage(this.ircClient.ReadMessage());
				}
				this.ViewerData.Update(deltaTime);
				if (this.LocalPlayer == null)
				{
					return;
				}
				bool flag = false;
				for (int i = this.LiveActionEntries.Count - 1; i >= 0; i--)
				{
					if (this.LiveActionEntries[i].ReadyForRemove)
					{
						this.LiveActionEntries.RemoveAt(i);
					}
					else if (this.LiveActionEntries[i].Action.CooldownBlocked)
					{
						flag = true;
					}
				}
				for (int j = this.actionSpawnLiveList.Count - 1; j >= 0; j--)
				{
					if (this.actionSpawnLiveList[j].SpawnedEntity == null)
					{
						this.actionSpawnLiveList.RemoveAt(j);
					}
				}
				for (int k = this.LiveEvents.Count - 1; k >= 0; k--)
				{
					if (this.LiveEvents[k].ReadyForRemove)
					{
						this.LiveEvents.RemoveAt(k);
					}
				}
				if (this.LocalPlayer.IsAlive() && this.CooldownTime > 0f && this.TwitchActive)
				{
					if (this.CooldownType == TwitchManager.CooldownTypes.MaxReachedWaiting && this.actionSpawnLiveList.Count == 0 && !flag)
					{
						this.SetCooldown(this.CooldownTime, TwitchManager.CooldownTypes.MaxReached, false, true);
					}
					if (this.CooldownType == TwitchManager.CooldownTypes.MaxReached || this.CooldownType == TwitchManager.CooldownTypes.Time || this.CooldownType == TwitchManager.CooldownTypes.Startup || this.CooldownType == TwitchManager.CooldownTypes.SafeCooldownExit)
					{
						float cooldownTime = this.CooldownTime;
						this.CooldownTime -= Time.deltaTime;
						if (cooldownTime >= 15f && this.CooldownTime < 15f && this.CooldownTime > 0f && this.CooldownType != TwitchManager.CooldownTypes.SafeCooldownExit)
						{
							this.LocalPlayer.TwitchActionsEnabled = EntityPlayer.TwitchActionsStates.TempDisabledEnding;
						}
					}
					if (this.CooldownTime <= 0f)
					{
						if (this.CooldownType == TwitchManager.CooldownTypes.SafeCooldownExit)
						{
							this.HandleEndCooldownStateChanging();
						}
						else
						{
							this.HandleEndCooldown();
						}
						this.VotingManager.VoteStartDelayTimeRemaining = 10f;
					}
				}
				for (int l = this.liveList.Count - 1; l >= 0; l--)
				{
					if (this.liveList[l].SpawnedEntity == null)
					{
						this.liveList[l].SpawnedEntity = this.world.GetEntity(this.liveList[l].SpawnedEntityID);
						this.liveList[l].SpawnedEntity == null;
					}
				}
				for (int m = this.recentlyDeadList.Count - 1; m >= 0; m--)
				{
					this.recentlyDeadList[m].TimeRemaining -= deltaTime;
					if (this.recentlyDeadList[m].TimeRemaining <= 0f)
					{
						this.recentlyDeadList.RemoveAt(m);
					}
				}
				for (int n = this.liveBlockList.Count - 1; n >= 0; n--)
				{
					TwitchSpawnedBlocksEntry twitchSpawnedBlocksEntry = this.liveBlockList[n];
					if (twitchSpawnedBlocksEntry.TimeRemaining > 0f)
					{
						twitchSpawnedBlocksEntry.TimeRemaining -= deltaTime;
						if (twitchSpawnedBlocksEntry.TimeRemaining <= 0f)
						{
							this.liveBlockList.RemoveAt(n);
						}
					}
				}
				int num2 = GameUtils.WorldTimeToDays(this.world.worldTime);
				if (num2 != this.lastGameDay)
				{
					this.SetupAvailableCommands();
					this.ResetDailyCommands(num2, -1);
					this.HandleCooldownActionLocking();
					this.lastGameDay = num2;
				}
				if (this.CooldownType != TwitchManager.CooldownTypes.Startup && this.TwitchActive && !gameManager.IsPaused() && this.InitState == TwitchManager.InitStates.Ready)
				{
					this.VotingManager.Update(deltaTime);
				}
				this.HandleEventQueue();
				if (this.LocalPlayer.IsAlive() && this.CooldownType != TwitchManager.CooldownTypes.Time)
				{
					int num3 = 0;
					while (num3 < this.QueuedActionEntries.Count)
					{
						if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsClient)
						{
							if (!this.QueuedActionEntries[num3].IsSent)
							{
								SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageGameEventRequest>().Setup(this.QueuedActionEntries[num3].Action.EventName, this.QueuedActionEntries[num3].Target.entityId, true, Vector3.zero, this.QueuedActionEntries[num3].UserName, "action", this.AllowCrateSharing, true, ""), false);
								this.QueuedActionEntries[num3].IsSent = true;
								break;
							}
							num3++;
						}
						else
						{
							if (GameEventManager.Current.HandleAction(this.QueuedActionEntries[num3].Action.EventName, this.LocalPlayer, this.QueuedActionEntries[num3].Target, true, this.QueuedActionEntries[num3].UserName, "action", this.AllowCrateSharing, true, "", null))
							{
								if (this.LocalPlayer.Party != null)
								{
									for (int num4 = 0; num4 < this.LocalPlayer.Party.MemberList.Count; num4++)
									{
										EntityPlayer entityPlayer = this.LocalPlayer.Party.MemberList[num4];
										if (entityPlayer != this.LocalPlayer && entityPlayer.TwitchEnabled)
										{
											SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageGameEventResponse>().Setup(this.QueuedActionEntries[num3].Action.EventName, this.QueuedActionEntries[num3].Target.entityId, this.QueuedActionEntries[num3].UserName, "action", NetPackageGameEventResponse.ResponseTypes.TwitchPartyActionApproved, -1, -1, false), false, entityPlayer.entityId, -1, -1, null, 192);
										}
									}
								}
								GameEventManager.Current.HandleGameEventApproved(this.QueuedActionEntries[num3].Action.EventName, this.QueuedActionEntries[num3].Target.entityId, this.QueuedActionEntries[num3].UserName, "action");
								break;
							}
							TwitchActionEntry twitchActionEntry = this.QueuedActionEntries[num3];
							ViewerEntry viewerEntry = this.ViewerData.GetViewerEntry(twitchActionEntry.UserName);
							this.AddActionHistory(twitchActionEntry, viewerEntry, TwitchActionHistoryEntry.EntryStates.Reimbursed);
							this.ShowReimburseMessage(twitchActionEntry, viewerEntry);
							this.ViewerData.ReimburseAction(twitchActionEntry);
							this.QueuedActionEntries.RemoveAt(num3);
							break;
						}
					}
				}
				this.saveTime -= Time.deltaTime;
				if (this.saveTime <= 0f && (this.dataSaveThreadInfo == null || this.dataSaveThreadInfo.HasTerminated()))
				{
					this.saveTime = 30f;
					if (this.HasDataChanges)
					{
						this.SaveViewerData();
						this.HasDataChanges = false;
					}
				}
				this.updateTime -= deltaTime;
				if (this.updateTime <= 0f)
				{
					this.updateTime = 2f;
					if (this.UseProgression && !this.OverrideProgession && this.commandsAvailable == -1)
					{
						this.commandsAvailable = this.GetCommandCount();
					}
					this.RefreshCommands(true);
					int @int = GameStats.GetInt(EnumGameStats.BloodMoonDay);
					if (this.nextBMDay != @int)
					{
						this.nextBMDay = @int;
						if (num2 != this.currentBMDayEnd)
						{
							this.currentBMDayEnd = this.nextBMDay + 1;
						}
						this.SetupBloodMoonData();
					}
					this.RefreshVoteLockedLevel();
					this.IsSafe = this.LocalPlayer.TwitchSafe;
					this.isBMActive = false;
					if (this.CooldownType != TwitchManager.CooldownTypes.Time && !this.IsVoting)
					{
						if (this.UseActionsDuringBloodmoon != 1)
						{
							if (this.WithinBloodMoonPeriod())
							{
								this.isBMActive = true;
								if (this.CooldownType != TwitchManager.CooldownTypes.BloodMoonDisabled && this.CooldownType != TwitchManager.CooldownTypes.BloodMoonCooldown)
								{
									this.SetCooldown(5f, (this.UseActionsDuringBloodmoon == 0) ? TwitchManager.CooldownTypes.BloodMoonDisabled : TwitchManager.CooldownTypes.BloodMoonCooldown, false, true);
								}
							}
							else if (this.CooldownType == TwitchManager.CooldownTypes.BloodMoonDisabled || this.CooldownType == TwitchManager.CooldownTypes.BloodMoonCooldown)
							{
								this.SetCooldown(5f, TwitchManager.CooldownTypes.Time, false, true);
								this.currentBMDayEnd = this.nextBMDay + 1;
								this.VotingManager.VoteStartDelayTimeRemaining += 35f;
							}
						}
						else if (this.CooldownType == TwitchManager.CooldownTypes.BloodMoonDisabled || this.CooldownType == TwitchManager.CooldownTypes.BloodMoonCooldown)
						{
							this.SetCooldown(5f, TwitchManager.CooldownTypes.Time, false, true);
						}
						if (this.AllowActions && this.CooldownType != TwitchManager.CooldownTypes.BloodMoonDisabled && this.CooldownType != TwitchManager.CooldownTypes.BloodMoonCooldown)
						{
							if (this.UseActionsDuringQuests != 1 && this.CooldownType != TwitchManager.CooldownTypes.Time)
							{
								if (QuestEventManager.Current.QuestBounds.width != 0f)
								{
									if (this.UseActionsDuringQuests == 0 && this.CooldownType != TwitchManager.CooldownTypes.QuestDisabled)
									{
										this.SetCooldown(5f, TwitchManager.CooldownTypes.QuestDisabled, false, false);
									}
									else if (this.UseActionsDuringQuests == 2 && this.CooldownType != TwitchManager.CooldownTypes.QuestCooldown)
									{
										this.SetCooldown(5f, TwitchManager.CooldownTypes.QuestCooldown, false, false);
									}
								}
								else if (this.CooldownType == TwitchManager.CooldownTypes.QuestCooldown || this.CooldownType == TwitchManager.CooldownTypes.QuestDisabled)
								{
									this.SetCooldown(60f, TwitchManager.CooldownTypes.Time, false, true);
									this.CurrentCooldownFill = 0f;
								}
							}
							else if (this.UseActionsDuringQuests == 1 && (this.CooldownType == TwitchManager.CooldownTypes.QuestCooldown || this.CooldownType == TwitchManager.CooldownTypes.QuestDisabled))
							{
								this.SetCooldown(60f, TwitchManager.CooldownTypes.Time, false, true);
							}
							if (this.CooldownType != TwitchManager.CooldownTypes.Time && this.CooldownType != TwitchManager.CooldownTypes.Startup && this.CooldownType != TwitchManager.CooldownTypes.MaxReached && this.CooldownType != TwitchManager.CooldownTypes.MaxReachedWaiting && this.CooldownType != TwitchManager.CooldownTypes.QuestCooldown && this.CooldownType != TwitchManager.CooldownTypes.QuestDisabled)
							{
								if (this.CooldownType != TwitchManager.CooldownTypes.SafeCooldown && this.LocalPlayer.TwitchSafe)
								{
									this.SetCooldown(5f, TwitchManager.CooldownTypes.SafeCooldown, false, false);
								}
								else if (this.CooldownType == TwitchManager.CooldownTypes.SafeCooldown && !this.LocalPlayer.TwitchSafe)
								{
									this.SetCooldown(5f, TwitchManager.CooldownTypes.SafeCooldownExit, false, true);
								}
							}
						}
						else if (this.CooldownType == TwitchManager.CooldownTypes.QuestCooldown || this.CooldownType == TwitchManager.CooldownTypes.QuestDisabled)
						{
							this.SetCooldown(60f, TwitchManager.CooldownTypes.Time, false, true);
						}
					}
					for (int num5 = this.RespawnEntries.Count - 1; num5 >= 0; num5--)
					{
						TwitchRespawnEntry twitchRespawnEntry = this.RespawnEntries[num5];
						if (twitchRespawnEntry.CanRespawn(this))
						{
							EntityPlayer target = twitchRespawnEntry.Target;
							if (!this.PartyInfo.ContainsKey(target) || this.PartyInfo[target].Cooldown <= 0f)
							{
								if (target.Buffs.HasBuff("twitch_pausedspawns"))
								{
									target.Buffs.RemoveBuff("twitch_pausedspawns", true);
								}
								target.PlayOneShot("twitch_unpause", false, false, false);
								this.QueuedActionEntries.Add(twitchRespawnEntry.RespawnAction());
							}
						}
					}
				}
				if (this.lastAlive && this.LocalPlayer.IsDead())
				{
					this.CurrentCooldownFill = 0f;
					if (this.CurrentCooldownPreset.CooldownType == CooldownPreset.CooldownTypes.Fill)
					{
						this.SetCooldown((float)this.CurrentCooldownPreset.AfterDeathCooldownTime, TwitchManager.CooldownTypes.Time, false, true);
					}
					this.KillAllSpawnsForPlayer(this.LocalPlayer);
				}
				if (this.lastAlive && this.LocalPlayer.IsAlive())
				{
					TwitchManager.DeathText = "";
				}
				if (!this.lastAlive && this.LocalPlayer.IsAlive())
				{
					this.respawnEventNeeded = true;
				}
				if (this.respawnEventNeeded && this.CheckCanRespawnEvent(this.LocalPlayer))
				{
					if (this.OnPlayerRespawnEvent != "")
					{
						GameEventManager.Current.HandleAction(this.OnPlayerRespawnEvent, this.LocalPlayer, this.LocalPlayer, false, "", "", false, true, "", null);
					}
					this.respawnEventNeeded = false;
				}
				this.lastAlive = this.LocalPlayer.IsAlive();
				this.twitchPlayerDeathsThisFrame.Clear();
				this.UpdatePartyInfo(deltaTime);
			}
			this.HandleInGameChatQueue();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void HandleEndCooldown()
		{
			this.CurrentCooldownFill = 0f;
			Manager.BroadcastPlayByLocalPlayer(this.LocalPlayer.position, "twitch_cooldown_ended");
			this.ircClient.SendChannelMessage(this.chatOutput_CooldownComplete, true);
			this.HandleEndCooldownStateChanging();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void HandleEndCooldownStateChanging()
		{
			this.CooldownType = TwitchManager.CooldownTypes.None;
			if (this.LocalPlayer.TwitchActionsEnabled == EntityPlayer.TwitchActionsStates.TempDisabled || this.LocalPlayer.TwitchActionsEnabled == EntityPlayer.TwitchActionsStates.TempDisabledEnding)
			{
				this.LocalPlayer.TwitchActionsEnabled = EntityPlayer.TwitchActionsStates.Enabled;
			}
			if (this.ConnectionStateChanged != null)
			{
				this.ConnectionStateChanged(this.initState, this.initState);
			}
			this.HandleCooldownActionLocking();
		}

		public void AddToInGameChatQueue(string msg, string sound = null)
		{
			this.inGameChatQueue.Add(new TwitchMessageEntry(msg, sound));
		}

		public void HandleInGameChatQueue()
		{
			if (this.inGameChatQueue.Count > 0 && this.LocalPlayer != null && this.LocalPlayer.IsAlive())
			{
				TwitchMessageEntry twitchMessageEntry = this.inGameChatQueue[0];
				XUiC_ChatOutput.AddMessage(this.LocalPlayerXUi, EnumGameMessages.PlainTextLocal, EChatType.Global, twitchMessageEntry.Message, -1, EMessageSender.Server, GeneratedTextManager.TextFilteringMode.None);
				if (twitchMessageEntry.Sound != null)
				{
					this.LocalPlayer.PlayOneShot(twitchMessageEntry.Sound, false, false, false);
				}
				this.inGameChatQueue.RemoveAt(0);
			}
		}

		public void RefreshVoteLockedLevel()
		{
			this.VoteLockedLevel = this.LocalPlayer.HasTwitchVoteLockMember();
		}

		public void SetupBloodMoonData()
		{
			int num = 22;
			int @int = GameStats.GetInt(EnumGameStats.DayLightLength);
			if (@int > 22)
			{
				num = Mathf.Clamp(@int, 0, 23);
			}
			this.BMCooldownStart = num - this.CurrentCooldownPreset.BMStartOffset;
			this.BMCooldownEnd = Mathf.Clamp(num - @int, 0, 23) + this.CurrentCooldownPreset.BMEndOffset;
		}

		public bool WithinBloodMoonPeriod()
		{
			ulong worldTime = this.world.worldTime;
			int num = GameUtils.WorldTimeToDays(worldTime);
			int num2 = GameUtils.WorldTimeToHours(worldTime);
			if (num == this.nextBMDay)
			{
				if (num2 >= this.BMCooldownStart)
				{
					return true;
				}
			}
			else if (num > 1 && num == this.currentBMDayEnd && num2 < this.BMCooldownEnd)
			{
				return true;
			}
			return false;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void Party_PartyMemberAdded(EntityPlayer player)
		{
			this.GetCooldownMax();
			if (!this.PartyInfo.ContainsKey(player))
			{
				this.PartyInfo.Add(player, new TwitchManager.TwitchPartyMemberInfo());
				ExtensionManager extensionManager = this.extensionManager;
				if (extensionManager == null)
				{
					return;
				}
				extensionManager.OnPartyChanged();
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void Party_PartyMemberRemoved(EntityPlayer player)
		{
			this.GetCooldownMax();
			if (this.PartyInfo.ContainsKey(player))
			{
				this.PartyInfo.Remove(player);
				ExtensionManager extensionManager = this.extensionManager;
				if (extensionManager == null)
				{
					return;
				}
				extensionManager.OnPartyChanged();
			}
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public void RefreshPartyInfo()
		{
			if (this.LocalPlayer == null)
			{
				return;
			}
			if (this.LocalPlayer.Party == null)
			{
				this.PartyInfo.Clear();
				return;
			}
			this.PartyInfo.Clear();
			for (int i = 0; i < this.LocalPlayer.Party.MemberList.Count; i++)
			{
				EntityPlayer entityPlayer = this.LocalPlayer.Party.MemberList[i];
				if (!(entityPlayer == this.LocalPlayer) && !this.PartyInfo.ContainsKey(entityPlayer))
				{
					this.PartyInfo.Add(entityPlayer, new TwitchManager.TwitchPartyMemberInfo());
				}
			}
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public void UpdatePartyInfo(float deltaTime)
		{
			bool flag = false;
			foreach (EntityPlayer entityPlayer in this.PartyInfo.Keys)
			{
				TwitchManager.TwitchPartyMemberInfo twitchPartyMemberInfo = this.PartyInfo[entityPlayer];
				if (!twitchPartyMemberInfo.LastAlive && entityPlayer.IsAlive() && this.PartyRespawnEvent != "")
				{
					GameEventManager.Current.HandleAction(this.PartyRespawnEvent, this.LocalPlayer, entityPlayer, false, "", "", false, true, "", null);
				}
				if (twitchPartyMemberInfo.LastOptedOut != (entityPlayer.TwitchActionsEnabled == EntityPlayer.TwitchActionsStates.Disabled))
				{
					twitchPartyMemberInfo.LastOptedOut = (entityPlayer.TwitchActionsEnabled == EntityPlayer.TwitchActionsStates.Disabled);
					flag = true;
				}
				if (twitchPartyMemberInfo.Cooldown > 0f)
				{
					twitchPartyMemberInfo.Cooldown -= deltaTime;
				}
				if (twitchPartyMemberInfo.LastAlive && !entityPlayer.IsAlive())
				{
					this.KillAllSpawnsForPlayer(entityPlayer);
					twitchPartyMemberInfo.Cooldown = 60f;
				}
				if (!twitchPartyMemberInfo.LastAlive && entityPlayer.IsAlive())
				{
					twitchPartyMemberInfo.NeedsRespawnEvent = true;
				}
				if (twitchPartyMemberInfo.NeedsRespawnEvent && this.CheckCanRespawnEvent(entityPlayer) && twitchPartyMemberInfo.Cooldown <= 0f)
				{
					if (this.OnPlayerRespawnEvent != "")
					{
						GameEventManager.Current.HandleAction(this.OnPlayerRespawnEvent, this.LocalPlayer, entityPlayer, false, "", "", false, true, "", null);
					}
					twitchPartyMemberInfo.NeedsRespawnEvent = false;
				}
				twitchPartyMemberInfo.LastAlive = entityPlayer.IsAlive();
			}
			if (flag)
			{
				this.GetCooldownMax();
			}
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public void TwitchDisconnectPartyUpdate()
		{
			this.RespawnEntries.Clear();
			if (this.OnPlayerRespawnEvent != "")
			{
				GameEventManager.Current.HandleAction(this.OnPlayerRespawnEvent, this.LocalPlayer, this.LocalPlayer, false, "", "", false, true, "", null);
			}
			if (this.LocalPlayer != null && this.LocalPlayer.Buffs.HasBuff("twitch_pausedspawns"))
			{
				this.LocalPlayer.Buffs.RemoveBuff("twitch_pausedspawns", true);
			}
			foreach (EntityPlayer entityPlayer in this.PartyInfo.Keys)
			{
				TwitchManager.TwitchPartyMemberInfo twitchPartyMemberInfo = this.PartyInfo[entityPlayer];
				if (twitchPartyMemberInfo.NeedsRespawnEvent)
				{
					if (this.OnPlayerRespawnEvent != "")
					{
						GameEventManager.Current.HandleAction(this.OnPlayerRespawnEvent, this.LocalPlayer, entityPlayer, false, "", "", false, true, "", null);
					}
					twitchPartyMemberInfo.NeedsRespawnEvent = false;
				}
				if (entityPlayer.Buffs.HasBuff("twitch_pausedspawns"))
				{
					entityPlayer.Buffs.RemoveBuff("twitch_pausedspawns", true);
				}
			}
		}

		public bool CheckCanRespawnEvent(EntityPlayer player)
		{
			return player != null && player.IsAlive() && player.TwitchActionsEnabled == EntityPlayer.TwitchActionsStates.Enabled && !player.TwitchSafe;
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public void KillAllSpawnsForPlayer(EntityPlayer player)
		{
			bool flag = false;
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = this.RespawnEntries.Count - 1; i >= 0; i--)
			{
				TwitchRespawnEntry twitchRespawnEntry = this.RespawnEntries[i];
				if (twitchRespawnEntry != null && twitchRespawnEntry.Target == player)
				{
					twitchRespawnEntry.NeedsRespawn = (twitchRespawnEntry.SpawnedEntities.Count > twitchRespawnEntry.Action.RespawnThreshold);
					if (twitchRespawnEntry.NeedsRespawn)
					{
						if (flag)
						{
							stringBuilder.Append(", ");
						}
						flag = true;
						stringBuilder.Append(twitchRespawnEntry.Action.Command);
					}
					else
					{
						Debug.LogWarning(string.Format("Respawn Entry removed '{0}' because count {1} was less than {2}", twitchRespawnEntry.Action.Command, twitchRespawnEntry.SpawnedEntities.Count, twitchRespawnEntry.Action.RespawnThreshold));
						this.RespawnEntries.RemoveAt(i);
					}
				}
			}
			if (flag)
			{
				if (!player.Buffs.HasBuff("twitch_pausedspawns"))
				{
					player.Buffs.AddBuff("twitch_pausedspawns", -1, true, false, -1f);
				}
				string text = stringBuilder.ToString();
				string msg = string.Format(this.ingameOutput_BitRespawns, text);
				this.AddToInGameChatQueue(msg, "twitch_pause");
				Debug.LogWarning(string.Format("Respawns Found for {0}: {1}", player.EntityName, text));
			}
			else
			{
				Debug.LogWarning("No Respawns Found!");
				if (player.Buffs.HasBuff("twitch_pausedspawns"))
				{
					player.Buffs.RemoveBuff("twitch_pausedspawns", true);
				}
			}
			if (this.OnPlayerDeathEvent != "")
			{
				GameEventManager.Current.HandleAction(this.OnPlayerDeathEvent, this.LocalPlayer, player, false, "", "", false, true, "", null);
			}
			for (int j = this.LiveActionEntries.Count - 1; j >= 0; j--)
			{
				if (this.LiveActionEntries[j] != null && this.LiveActionEntries[j].Target == player)
				{
					TwitchActionEntry twitchActionEntry = this.LiveActionEntries[j];
					twitchActionEntry.ReadyForRemove = true;
					if (twitchActionEntry.HistoryEntry != null)
					{
						twitchActionEntry.HistoryEntry.EntryState = TwitchActionHistoryEntry.EntryStates.Despawned;
					}
				}
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void LocalPlayer_PartyChanged(Party _affectedParty, EntityPlayer _player)
		{
			if (this.extensionManager != null)
			{
				this.extensionManager.OnPartyChanged();
			}
			this.GetCooldownMax();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void LocalPlayer_PartyLeave(Party _affectedParty, EntityPlayer _player)
		{
			if (this.extensionManager != null)
			{
				this.extensionManager.OnPartyChanged();
			}
			this.GetCooldownMax();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void LocalPlayer_PartyJoined(Party _affectedParty, EntityPlayer _player)
		{
			if (this.LocalPlayer.Party != null)
			{
				this.LocalPlayer.Party.PartyMemberAdded += this.Party_PartyMemberAdded;
				this.LocalPlayer.Party.PartyMemberRemoved += this.Party_PartyMemberRemoved;
			}
			if (this.extensionManager != null)
			{
				this.extensionManager.OnPartyChanged();
			}
			this.GetCooldownMax();
			this.RefreshPartyInfo();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void Current_GameEntityKilled(int entityID)
		{
			for (int i = this.liveList.Count - 1; i >= 0; i--)
			{
				TwitchSpawnedEntityEntry twitchSpawnedEntityEntry = this.liveList[i];
				if (twitchSpawnedEntityEntry.SpawnedEntityID == entityID)
				{
					if (twitchSpawnedEntityEntry.RespawnEntry != null)
					{
						TwitchRespawnEntry respawnEntry = twitchSpawnedEntityEntry.RespawnEntry;
						if (respawnEntry.RemoveSpawnedEntry(entityID, true) && respawnEntry.ReadyForRemove)
						{
							this.RespawnEntries.Remove(respawnEntry);
						}
					}
					this.actionSpawnLiveList.Remove(twitchSpawnedEntityEntry);
					this.recentlyDeadList.Add(new TwitchRecentlyRemovedEntityEntry(twitchSpawnedEntityEntry));
					this.liveList.RemoveAt(i);
					return;
				}
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void Current_GameEntityDespawned(int entityID)
		{
			for (int i = this.liveList.Count - 1; i >= 0; i--)
			{
				if (this.liveList[i].SpawnedEntityID == entityID)
				{
					TwitchSpawnedEntityEntry twitchSpawnedEntityEntry = this.liveList[i];
					if (twitchSpawnedEntityEntry.Action != null && twitchSpawnedEntityEntry.Action.UserName != null)
					{
						this.ViewerData.AddPoints(twitchSpawnedEntityEntry.Action.UserName, (int)((float)twitchSpawnedEntityEntry.Action.ActionCost * 0.25f), false, false);
						if (twitchSpawnedEntityEntry.Action.HistoryEntry != null)
						{
							twitchSpawnedEntityEntry.Action.HistoryEntry.EntryState = TwitchActionHistoryEntry.EntryStates.Despawned;
						}
						if (twitchSpawnedEntityEntry.RespawnEntry != null)
						{
							TwitchRespawnEntry respawnEntry = twitchSpawnedEntityEntry.RespawnEntry;
							if (respawnEntry.RemoveSpawnedEntry(entityID, false) && respawnEntry.RespawnsLeft == 0)
							{
								this.RespawnEntries.Remove(respawnEntry);
							}
						}
					}
					else if (twitchSpawnedEntityEntry.Event != null && twitchSpawnedEntityEntry.Event.HistoryEntry != null)
					{
						twitchSpawnedEntityEntry.Event.HistoryEntry.EntryState = TwitchActionHistoryEntry.EntryStates.Despawned;
					}
					this.actionSpawnLiveList.Remove(twitchSpawnedEntityEntry);
					this.recentlyDeadList.Add(new TwitchRecentlyRemovedEntityEntry(twitchSpawnedEntityEntry));
					this.liveList.RemoveAt(i);
					return;
				}
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void Current_GameEventAccessApproved()
		{
			if (this.InitState == TwitchManager.InitStates.None || this.InitState == TwitchManager.InitStates.WaitingForPermission)
			{
				this.StartTwitchIntegration();
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void Current_GameEventApproved(string gameEventID, int targetEntityID, string viewerName, string tag)
		{
			if (tag == "action")
			{
				for (int i = 0; i < this.QueuedActionEntries.Count; i++)
				{
					if (this.QueuedActionEntries[i].Action.EventName == gameEventID && this.QueuedActionEntries[i].Target.entityId == targetEntityID && this.QueuedActionEntries[i].UserName == viewerName)
					{
						this.ConfirmAction(this.QueuedActionEntries[i]);
						this.LiveActionEntries.Add(this.QueuedActionEntries[i]);
						this.QueuedActionEntries.RemoveAt(i);
						return;
					}
				}
				return;
			}
			if (tag == "event")
			{
				for (int j = 0; j < this.EventQueue.Count; j++)
				{
					if (this.EventQueue[j].UserName == viewerName && this.EventQueue[j].Event.EventName == gameEventID)
					{
						this.LiveEvents.Add(this.EventQueue[j]);
						this.EventQueue.RemoveAt(j);
						return;
					}
				}
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void Current_GameEventDenied(string gameEventID, int targetEntityID, string viewerName, string tag)
		{
			if (tag == "action")
			{
				for (int i = 0; i < this.QueuedActionEntries.Count; i++)
				{
					if (this.QueuedActionEntries[i].Action.EventName == gameEventID && this.QueuedActionEntries[i].Target.entityId == targetEntityID && this.QueuedActionEntries[i].UserName == viewerName)
					{
						this.ViewerData.ReimburseAction(this.QueuedActionEntries[i]);
						this.QueuedActionEntries.RemoveAt(i);
						return;
					}
				}
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void Current_TwitchRefundNeeded(string gameEventID, int targetEntityID, string viewerName, string tag)
		{
			if (tag == "action")
			{
				for (int i = 0; i < this.LiveActionEntries.Count; i++)
				{
					TwitchActionEntry twitchActionEntry = this.LiveActionEntries[i];
					if (twitchActionEntry.Action.EventName == gameEventID && twitchActionEntry.Target.entityId == targetEntityID && twitchActionEntry.UserName == viewerName)
					{
						this.ViewerData.ReimburseAction(this.LiveActionEntries[i]);
						this.ShowReimburseMessage(twitchActionEntry, null);
						Debug.LogWarning(string.Format("TwitchAction {0} refunded for {1}.", twitchActionEntry.Action.Name, viewerName));
						if (twitchActionEntry.HistoryEntry != null)
						{
							twitchActionEntry.HistoryEntry.EntryState = TwitchActionHistoryEntry.EntryStates.Reimbursed;
						}
						if (twitchActionEntry.Action.tempCooldown > 30f)
						{
							twitchActionEntry.Action.SetCooldown(this.CurrentUnityTime, 30f);
						}
						this.LiveActionEntries.RemoveAt(i);
						return;
					}
				}
				return;
			}
			if (tag == "event")
			{
				int j = 0;
				while (j < this.LiveEvents.Count)
				{
					TwitchEventActionEntry twitchEventActionEntry = this.LiveEvents[j];
					if (twitchEventActionEntry.Event.EventName == gameEventID && twitchEventActionEntry.UserName == viewerName)
					{
						Debug.LogWarning(string.Format("Twitch Debug: Live Event: Refunded {0} for {1}.", twitchEventActionEntry.Event.EventTitle, viewerName));
						if (twitchEventActionEntry.HistoryEntry != null)
						{
							twitchEventActionEntry.HistoryEntry.EntryState = TwitchActionHistoryEntry.EntryStates.Reimbursed;
							return;
						}
						break;
					}
					else
					{
						j++;
					}
				}
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void Current_GameEventCompleted(string gameEventID, int targetEntityID, string viewerName, string tag)
		{
			if (tag == "action")
			{
				for (int i = 0; i < this.LiveActionEntries.Count; i++)
				{
					TwitchActionEntry twitchActionEntry = this.LiveActionEntries[i];
					if (!twitchActionEntry.ReadyForRemove && twitchActionEntry.Action.EventName == gameEventID && twitchActionEntry.Target.entityId == targetEntityID && twitchActionEntry.UserName == viewerName)
					{
						twitchActionEntry.ReadyForRemove = true;
						if (twitchActionEntry.HistoryEntry != null)
						{
							twitchActionEntry.HistoryEntry.EntryState = TwitchActionHistoryEntry.EntryStates.Completed;
						}
						return;
					}
				}
				return;
			}
			if (tag == "event")
			{
				for (int j = 0; j < this.LiveEvents.Count; j++)
				{
					TwitchEventActionEntry twitchEventActionEntry = this.LiveEvents[j];
					if (!twitchEventActionEntry.ReadyForRemove && twitchEventActionEntry.UserName == viewerName && twitchEventActionEntry.Event.EventName == gameEventID)
					{
						if (twitchEventActionEntry.HistoryEntry != null)
						{
							twitchEventActionEntry.HistoryEntry.EntryState = TwitchActionHistoryEntry.EntryStates.Completed;
						}
						twitchEventActionEntry.ReadyForRemove = true;
						return;
					}
				}
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void Current_GameEntitySpawned(string gameEventID, int entityID, string tag)
		{
			if (tag == "action")
			{
				int i = 0;
				while (i < this.LiveActionEntries.Count)
				{
					if (!this.LiveActionEntries[i].ReadyForRemove && this.LiveActionEntries[i].Action.EventName == gameEventID)
					{
						Entity entity = null;
						if (!this.LiveActionEntries[i].Action.AddsToCooldown)
						{
							return;
						}
						TwitchSpawnedEntityEntry twitchSpawnedEntityEntry = new TwitchSpawnedEntityEntry
						{
							Action = this.LiveActionEntries[i],
							SpawnedEntityID = entityID
						};
						this.liveList.Add(twitchSpawnedEntityEntry);
						if (entity == null)
						{
							entity = this.world.GetEntity(entityID);
						}
						twitchSpawnedEntityEntry.SpawnedEntity = entity;
						if (twitchSpawnedEntityEntry.Action.Action.RespawnCountType != TwitchAction.RespawnCountTypes.None)
						{
							TwitchRespawnEntry respawnEntry = this.GetRespawnEntry(twitchSpawnedEntityEntry.Action.UserName, twitchSpawnedEntityEntry.Action.Target, twitchSpawnedEntityEntry.Action.Action);
							respawnEntry.SpawnedEntities.Add(entityID);
							twitchSpawnedEntityEntry.RespawnEntry = respawnEntry;
						}
						this.actionSpawnLiveList.Add(twitchSpawnedEntityEntry);
						return;
					}
					else
					{
						i++;
					}
				}
				return;
			}
			if (tag == "event")
			{
				int j = 0;
				while (j < this.LiveEvents.Count)
				{
					if (!this.LiveEvents[j].ReadyForRemove && this.LiveEvents[j].Event.EventName == gameEventID)
					{
						Entity entity2 = null;
						if (entity2 == null)
						{
							entity2 = this.world.GetEntity(entityID);
						}
						if (!(entity2 is EntityAlive))
						{
							return;
						}
						TwitchSpawnedEntityEntry twitchSpawnedEntityEntry2 = new TwitchSpawnedEntityEntry
						{
							Event = this.LiveEvents[j],
							SpawnedEntityID = entityID
						};
						this.liveList.Add(twitchSpawnedEntityEntry2);
						twitchSpawnedEntityEntry2.SpawnedEntity = entity2;
						this.actionSpawnLiveList.Add(twitchSpawnedEntityEntry2);
						return;
					}
					else
					{
						j++;
					}
				}
				return;
			}
			if (!(tag == "vote") || this.VotingManager.CurrentEvent == null || !(this.VotingManager.CurrentEvent.VoteClass.GameEvent == gameEventID))
			{
				return;
			}
			Entity entity3 = null;
			if (entity3 == null)
			{
				entity3 = this.world.GetEntity(entityID);
			}
			if (!(entity3 is EntityAlive))
			{
				return;
			}
			TwitchSpawnedEntityEntry twitchSpawnedEntityEntry3 = new TwitchSpawnedEntityEntry
			{
				Vote = this.VotingManager.CurrentEvent,
				SpawnedEntityID = entityID
			};
			this.liveList.Add(twitchSpawnedEntityEntry3);
			twitchSpawnedEntityEntry3.SpawnedEntity = entity3;
			this.VotingManager.CurrentEvent.ActiveSpawns.Add(entityID);
			this.actionSpawnLiveList.Add(twitchSpawnedEntityEntry3);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void Current_GameBlocksAdded(string gameEventID, int blockGroupID, List<Vector3i> blockList, string tag)
		{
			if (tag == "action")
			{
				int i = 0;
				while (i < this.LiveActionEntries.Count)
				{
					if (!this.LiveActionEntries[i].ReadyForRemove && this.LiveActionEntries[i].Action.EventName == gameEventID)
					{
						if (!this.LiveActionEntries[i].Action.AddsToCooldown)
						{
							return;
						}
						TwitchSpawnedBlocksEntry twitchSpawnedBlocksEntry = new TwitchSpawnedBlocksEntry
						{
							BlockGroupID = blockGroupID,
							Action = this.LiveActionEntries[i],
							blocks = blockList.ToList<Vector3i>()
						};
						this.liveBlockList.Add(twitchSpawnedBlocksEntry);
						if (twitchSpawnedBlocksEntry.Action.Action.RespawnCountType != TwitchAction.RespawnCountTypes.None)
						{
							TwitchRespawnEntry respawnEntry = this.GetRespawnEntry(twitchSpawnedBlocksEntry.Action.UserName, twitchSpawnedBlocksEntry.Action.Target, twitchSpawnedBlocksEntry.Action.Action);
							respawnEntry.SpawnedBlocks.AddRange(blockList);
							twitchSpawnedBlocksEntry.RespawnEntry = respawnEntry;
						}
						return;
					}
					else
					{
						i++;
					}
				}
				return;
			}
			if (tag == "event")
			{
				for (int j = 0; j < this.LiveEvents.Count; j++)
				{
					if (!this.LiveEvents[j].ReadyForRemove && this.LiveEvents[j].Event.EventName == gameEventID)
					{
						TwitchSpawnedBlocksEntry item = new TwitchSpawnedBlocksEntry
						{
							BlockGroupID = blockGroupID,
							Event = this.LiveEvents[j],
							blocks = blockList.ToList<Vector3i>()
						};
						this.liveBlockList.Add(item);
						return;
					}
				}
				return;
			}
			if (tag == "vote" && this.VotingManager.CurrentEvent != null && this.VotingManager.CurrentEvent.VoteClass.GameEvent == gameEventID)
			{
				TwitchSpawnedBlocksEntry item2 = new TwitchSpawnedBlocksEntry
				{
					BlockGroupID = blockGroupID,
					Vote = this.VotingManager.CurrentEvent,
					blocks = blockList.ToList<Vector3i>()
				};
				this.liveBlockList.Add(item2);
				return;
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void Current_GameBlocksRemoved(int blockGroupID, bool isDespawn)
		{
			for (int i = 0; i < this.liveBlockList.Count; i++)
			{
				if (this.liveBlockList[i].BlockGroupID == blockGroupID)
				{
					if (this.liveBlockList[i].RespawnEntry != null)
					{
						TwitchRespawnEntry respawnEntry = this.liveBlockList[i].RespawnEntry;
						if (respawnEntry.RemoveAllSpawnedBlock(!isDespawn) && respawnEntry.ReadyForRemove)
						{
							this.RespawnEntries.Remove(respawnEntry);
						}
					}
					this.liveBlockList.RemoveAt(i);
					return;
				}
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void Current_GameBlockRemoved(Vector3i blockRemoved)
		{
			for (int i = 0; i < this.liveBlockList.Count; i++)
			{
				if (this.liveBlockList[i].RemoveBlock(blockRemoved))
				{
					this.liveBlockList[i].TimeRemaining = 5f;
					return;
				}
			}
		}

		public void HandleConsoleAction(List<string> consoleParams)
		{
			for (int i = 0; i < this.TwitchCommandList.Count; i++)
			{
				for (int j = 0; j < this.TwitchCommandList[i].CommandText.Length; j++)
				{
					if (consoleParams[0].StartsWith(this.TwitchCommandList[i].CommandText[j]))
					{
						this.TwitchCommandList[i].ExecuteConsole(consoleParams);
						return;
					}
				}
			}
		}

		public bool IsActionAvailable(string actionName)
		{
			if (!this.VotingManager.VotingIsActive)
			{
				if (actionName[0] != '#')
				{
					actionName = "#" + actionName.ToLower();
				}
				if (this.twitchActive && this.VoteLockedLevel != TwitchVoteLockTypes.ActionsLocked && this.AllowActions && this.AvailableCommands.ContainsKey(actionName))
				{
					TwitchAction twitchAction = this.AvailableCommands[actionName];
					if (!twitchAction.IgnoreCooldown)
					{
						if (this.CooldownType == TwitchManager.CooldownTypes.BloodMoonDisabled || this.CooldownType == TwitchManager.CooldownTypes.Time || this.CooldownType == TwitchManager.CooldownTypes.QuestDisabled)
						{
							return false;
						}
						if ((this.CooldownType == TwitchManager.CooldownTypes.MaxReachedWaiting || this.CooldownType == TwitchManager.CooldownTypes.SafeCooldown) && twitchAction.WaitingBlocked)
						{
							return false;
						}
					}
					if (this.UseProgression && !this.OverrideProgession && twitchAction.StartGameStage != -1 && twitchAction.StartGameStage > this.HighestGameStage)
					{
						return false;
					}
					if (!twitchAction.CanUse)
					{
						return false;
					}
					if ((twitchAction.IgnoreCooldown || !twitchAction.CooldownBlocked || !this.OnCooldown) && twitchAction.IsReady(this))
					{
						return true;
					}
				}
			}
			return false;
		}

		public void HandleExtensionMessage(int userId, string message, bool isRerun, int creditUsed, int bitsUsed)
		{
			string text;
			if (!this.ViewerData.IdToUsername.TryGetValue(userId, out text))
			{
				return;
			}
			bool flag = false;
			if (creditUsed < 0)
			{
				creditUsed = 0;
			}
			string[] array = message.Split(' ', StringSplitOptions.None);
			string text2 = array[0];
			TwitchAction twitchAction = null;
			if (this.AvailableCommands.ContainsKey(text2))
			{
				twitchAction = this.AvailableCommands[text2];
			}
			else
			{
				foreach (TwitchAction twitchAction2 in TwitchActionManager.TwitchActions.Values)
				{
					if (twitchAction2.IsInPreset(this.CurrentActionPreset) && twitchAction2.Command == text2)
					{
						twitchAction = twitchAction2;
						break;
					}
				}
			}
			if (twitchAction == null)
			{
				return;
			}
			bool flag2 = twitchAction.PointType == TwitchAction.PointTypes.Bits;
			if (!this.VotingManager.VotingIsActive && this.twitchActive && (flag2 || (this.VoteLockedLevel != TwitchVoteLockTypes.ActionsLocked && this.AllowActions)))
			{
				if (!isRerun && !flag2)
				{
					if (!twitchAction.IgnoreCooldown)
					{
						if (this.CooldownType == TwitchManager.CooldownTypes.BloodMoonDisabled || this.CooldownType == TwitchManager.CooldownTypes.Time || this.CooldownType == TwitchManager.CooldownTypes.QuestDisabled)
						{
							return;
						}
						if ((this.CooldownType == TwitchManager.CooldownTypes.MaxReachedWaiting || this.CooldownType == TwitchManager.CooldownTypes.SafeCooldown) && twitchAction.WaitingBlocked)
						{
							return;
						}
					}
					if (this.UseProgression && !this.OverrideProgession && twitchAction.StartGameStage != -1 && twitchAction.StartGameStage > this.HighestGameStage)
					{
						return;
					}
					if (!twitchAction.CanUse)
					{
						return;
					}
				}
				if (flag2 || isRerun || twitchAction.IgnoreCooldown || !twitchAction.CooldownBlocked || !this.OnCooldown || ((this.CooldownType == TwitchManager.CooldownTypes.MaxReachedWaiting || this.CooldownType == TwitchManager.CooldownTypes.SafeCooldown) && !twitchAction.WaitingBlocked))
				{
					TwitchActionEntry twitchActionEntry = null;
					if ((isRerun || twitchAction.IsReady(this)) && this.ViewerData.HandleInitialActionEntrySetup(text, twitchAction, isRerun, flag2, out twitchActionEntry))
					{
						twitchActionEntry.UserName = text;
						twitchActionEntry.ChannelNotify = (twitchAction.PointType == TwitchAction.PointTypes.Bits);
						twitchActionEntry.IsBitAction = flag2;
						twitchActionEntry.IsReRun = isRerun;
						twitchActionEntry.Action = twitchAction;
						EntityPlayer entityPlayer = this.LocalPlayer;
						if (array.Length > 1 && this.LocalPlayer.Party != null)
						{
							string b = message.Substring(array[0].Length + 1).ToLower();
							for (int i = 0; i < this.LocalPlayer.Party.MemberList.Count; i++)
							{
								if (this.LocalPlayer.Party.MemberList[i].EntityName.ToLower() == b)
								{
									entityPlayer = this.LocalPlayer.Party.MemberList[i];
									break;
								}
							}
							if (entityPlayer != this.LocalPlayer && entityPlayer.TwitchActionsEnabled != EntityPlayer.TwitchActionsStates.Enabled)
							{
								if (flag2)
								{
									ViewerEntry viewerEntry = this.ViewerData.GetViewerEntry(twitchActionEntry.UserName);
									twitchActionEntry.Target = entityPlayer;
									this.AddActionHistory(twitchActionEntry, viewerEntry, TwitchActionHistoryEntry.EntryStates.Reimbursed);
									this.ShowReimburseMessage(twitchActionEntry, viewerEntry);
								}
								this.ViewerData.ReimburseAction(twitchActionEntry);
								return;
							}
							if (this.PartyInfo.ContainsKey(entityPlayer) && this.PartyInfo[entityPlayer].Cooldown > 0f)
							{
								if (flag2)
								{
									ViewerEntry viewerEntry2 = this.ViewerData.GetViewerEntry(twitchActionEntry.UserName);
									twitchActionEntry.Target = entityPlayer;
									this.AddActionHistory(twitchActionEntry, viewerEntry2, TwitchActionHistoryEntry.EntryStates.Reimbursed);
									this.ShowReimburseMessage(twitchActionEntry, viewerEntry2);
								}
								this.ViewerData.ReimburseAction(twitchActionEntry);
								return;
							}
						}
						twitchActionEntry.Target = entityPlayer;
						if (twitchActionEntry.CreditsUsed != creditUsed)
						{
							Debug.LogWarning(string.Format("Twitch Bit Credit usage is invalid: {0} used {1} when their balance was {2}. They were credited the amount they spent in bits.", twitchActionEntry.UserName, creditUsed, twitchActionEntry.CreditsUsed));
							this.ViewerData.AddCredit(text, creditUsed + bitsUsed, false);
							ViewerEntry viewerEntry3 = this.ViewerData.GetViewerEntry(twitchActionEntry.UserName);
							this.AddActionHistory(twitchActionEntry, viewerEntry3, TwitchActionHistoryEntry.EntryStates.Reimbursed);
							this.ShowReimburseMessage(twitchActionEntry, viewerEntry3);
							return;
						}
						if (twitchAction.ModifiedCooldown > 0f)
						{
							twitchAction.tempCooldownSet = this.CurrentUnityTime;
							twitchAction.tempCooldown = 1f;
						}
						if (twitchActionEntry.CreditsUsed > 0)
						{
							ViewerEntry viewerEntry4 = this.ViewerData.GetViewerEntry(twitchActionEntry.UserName);
							this.PushBalanceToExtensionQueue(viewerEntry4.UserID.ToString(), viewerEntry4.BitCredits);
						}
						this.QueuedActionEntries.Add(twitchActionEntry);
						flag = true;
					}
				}
			}
			if (!flag && flag2)
			{
				ViewerEntry viewerEntry5 = this.ViewerData.AddCredit(text, twitchAction.CurrentCost - creditUsed, false);
				if (viewerEntry5 != null)
				{
					this.ShowReimburseMessage(text, creditUsed, twitchAction, viewerEntry5);
				}
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void HandleMessage(TwitchIRCClient.TwitchChatMessage message)
		{
			switch (message.MessageType)
			{
			case TwitchIRCClient.TwitchChatMessage.MessageTypes.Invalid:
				return;
			case TwitchIRCClient.TwitchChatMessage.MessageTypes.Message:
			{
				if (this.InitState != TwitchManager.InitStates.Ready)
				{
					return;
				}
				string text = message.Message.ToLower();
				ViewerEntry entry = this.ViewerData.UpdateViewerEntry(message.UserID, message.UserName, message.UserNameColor, message.isSub);
				if (message.isBroadcaster)
				{
					if (text.StartsWith("#cooldowninfo"))
					{
						this.ircClient.SendChannelMessage(string.Format("[7DTD]: Cooldown is at {0}/{1}.", this.CurrentCooldownFill, this.CurrentCooldownPreset.CooldownFillMax), true);
					}
					else if (text.StartsWith("#reset "))
					{
						this.actionSpawnLiveList.Clear();
						this.LiveActionEntries.Clear();
						this.ircClient.SendChannelMessage("[7DTD]: Action Live list Cleared!", true);
					}
				}
				for (int i = 0; i < this.TwitchCommandList.Count; i++)
				{
					for (int j = 0; j < this.TwitchCommandList[i].CommandTextList.Count; j++)
					{
						if (text.StartsWith(this.TwitchCommandList[i].CommandTextList[j]) && this.TwitchCommandList[i].CheckAllowed(message))
						{
							this.TwitchCommandList[i].Execute(entry, message);
							break;
						}
					}
				}
				this.VotingManager.HandleMessage(message);
				if (!this.VotingManager.VotingIsActive)
				{
					if (this.IntegrationSetting == TwitchManager.IntegrationSettings.ExtensionOnly)
					{
						return;
					}
					string[] array = text.Split(' ', StringSplitOptions.None);
					string key = array[0];
					if (this.AlternateCommands.ContainsKey(key))
					{
						key = this.AlternateCommands[key];
					}
					if (this.twitchActive && this.VoteLockedLevel != TwitchVoteLockTypes.ActionsLocked && this.AllowActions && this.AvailableCommands.ContainsKey(key))
					{
						TwitchAction twitchAction = this.AvailableCommands[key];
						if (twitchAction.PointType == TwitchAction.PointTypes.Bits)
						{
							return;
						}
						if (!twitchAction.IgnoreCooldown)
						{
							if (this.CooldownType == TwitchManager.CooldownTypes.BloodMoonDisabled || this.CooldownType == TwitchManager.CooldownTypes.Time || this.CooldownType == TwitchManager.CooldownTypes.Startup || this.CooldownType == TwitchManager.CooldownTypes.QuestDisabled)
							{
								return;
							}
							if ((this.CooldownType == TwitchManager.CooldownTypes.MaxReachedWaiting || this.CooldownType == TwitchManager.CooldownTypes.SafeCooldown) && twitchAction.WaitingBlocked)
							{
								return;
							}
						}
						if (this.UseProgression && !this.OverrideProgession && twitchAction.StartGameStage != -1 && twitchAction.StartGameStage > this.HighestGameStage)
						{
							return;
						}
						if (!twitchAction.CanUse || !twitchAction.CheckUsable(message))
						{
							return;
						}
						if (twitchAction.IgnoreCooldown || !twitchAction.CooldownBlocked || !this.OnCooldown || ((this.CooldownType == TwitchManager.CooldownTypes.MaxReachedWaiting || this.CooldownType == TwitchManager.CooldownTypes.SafeCooldown) && !twitchAction.WaitingBlocked))
						{
							TwitchActionEntry twitchActionEntry = null;
							if (twitchAction.IsReady(this) && this.ViewerData.HandleInitialActionEntrySetup(message.UserName, twitchAction, false, false, out twitchActionEntry))
							{
								twitchActionEntry.UserName = message.UserName;
								EntityPlayer entityPlayer = this.LocalPlayer;
								if (array.Length > 1 && this.LocalPlayer.Party != null)
								{
									int index = -1;
									if (StringParsers.TryParseSInt32(array[1], out index, 0, -1, NumberStyles.Integer))
									{
										entityPlayer = this.LocalPlayer.Party.GetMemberAtIndex(index, this.LocalPlayer);
										if (entityPlayer == null)
										{
											this.ViewerData.ReimburseAction(twitchActionEntry);
											return;
										}
										if (entityPlayer.TwitchActionsEnabled != EntityPlayer.TwitchActionsStates.Enabled)
										{
											this.ViewerData.ReimburseAction(twitchActionEntry);
											return;
										}
									}
									else
									{
										string b = text.Substring(array[0].Length + 1);
										bool flag = false;
										for (int k = 0; k < this.LocalPlayer.Party.MemberList.Count; k++)
										{
											if (this.LocalPlayer.Party.MemberList[k].EntityName.ToLower() == b)
											{
												entityPlayer = this.LocalPlayer.Party.MemberList[k];
												flag = true;
												break;
											}
										}
										if (!flag)
										{
											this.ViewerData.ReimburseAction(twitchActionEntry);
											return;
										}
										if (entityPlayer != this.LocalPlayer && entityPlayer.TwitchActionsEnabled != EntityPlayer.TwitchActionsStates.Enabled)
										{
											this.ViewerData.ReimburseAction(twitchActionEntry);
											return;
										}
										if (this.PartyInfo.ContainsKey(entityPlayer) && this.PartyInfo[entityPlayer].Cooldown > 0f)
										{
											this.ViewerData.ReimburseAction(twitchActionEntry);
											return;
										}
									}
								}
								if (twitchAction.StreamerOnly && entityPlayer != this.LocalPlayer)
								{
									this.ViewerData.ReimburseAction(twitchActionEntry);
									return;
								}
								twitchActionEntry.Target = entityPlayer;
								twitchActionEntry.Action = twitchAction;
								this.QueuedActionEntries.Add(twitchActionEntry);
								return;
							}
						}
					}
				}
				break;
			}
			case TwitchIRCClient.TwitchChatMessage.MessageTypes.Output:
				break;
			case TwitchIRCClient.TwitchChatMessage.MessageTypes.Authenticated:
				if (this.InitState != TwitchManager.InitStates.Ready)
				{
					List<string> list = new List<string>();
					list.Add("CAP REQ :twitch.tv/membership");
					list.Add("CAP REQ :twitch.tv/tags");
					list.Add("CAP REQ :twitch.tv/commands");
					this.ircClient.SendIrcMessages(list, false);
					this.InitState = TwitchManager.InitStates.CheckingForExtension;
					TwitchAuthentication.bFirstLogin = false;
					return;
				}
				break;
			case TwitchIRCClient.TwitchChatMessage.MessageTypes.Raid:
			{
				int viewerAmount = StringParsers.ParseSInt32(message.Message, 0, -1, NumberStyles.Integer);
				this.HandleRaid(message.UserName.ToLower(), message.UserID, viewerAmount);
				return;
			}
			case TwitchIRCClient.TwitchChatMessage.MessageTypes.Charity:
			{
				int charityAmount = StringParsers.ParseSInt32(message.Message, 0, -1, NumberStyles.Integer);
				this.HandleCharity(message.UserName.ToLower(), message.UserID, charityAmount);
				break;
			}
			default:
				return;
			}
		}

		public void DisplayDebug(string message)
		{
			Debug.LogWarning("Called: " + message);
			Debug.LogWarning(string.Format("[7DTD]: Spawns Alive: {0}  Blocks Alive: {1}  ActionLiveList: {2}.", this.actionSpawnLiveList.Count, this.liveBlockList.Count, this.LiveActionEntries.Count));
			for (int i = 0; i < this.actionSpawnLiveList.Count; i++)
			{
				if (this.actionSpawnLiveList[i].SpawnedEntity != null)
				{
					Debug.LogWarning(string.Format("Spawn Alive: {0}", this.actionSpawnLiveList[i].SpawnedEntity.name));
				}
			}
			for (int j = 0; j < this.LiveActionEntries.Count; j++)
			{
				Debug.LogWarning(string.Format("Action: {0} Target: {1} Viewer: {2}", this.LiveActionEntries[j].Action.Name, this.LiveActionEntries[j].Target.EntityName, this.LiveActionEntries[j].UserName));
			}
			for (int k = 0; k < this.EventQueue.Count; k++)
			{
				Debug.LogWarning(string.Format("Event: {0} User: {1} Sent: {2}", this.EventQueue[k].Event.EventTitle, this.EventQueue[k].UserName, this.EventQueue[k].IsSent));
			}
			this.ircClient.SendChannelMessage("[7DTD]: Debug Complete!", true);
		}

		public void AddToPot(int amount)
		{
			this.RewardPot += amount;
			if (this.RewardPot < 0)
			{
				this.RewardPot = 0;
			}
			if (this.RewardPot > TwitchManager.LeaderboardStats.LargestPimpPot)
			{
				TwitchManager.LeaderboardStats.LargestPimpPot = this.RewardPot;
			}
		}

		public void AddToBitPot(int amount)
		{
			this.BitPot += amount;
			if (this.BitPot < 0)
			{
				this.BitPot = 0;
			}
			if (this.BitPot > TwitchManager.LeaderboardStats.LargestBitPot)
			{
				TwitchManager.LeaderboardStats.LargestBitPot = this.BitPot;
			}
		}

		public void SetPot(int newPot)
		{
			if (newPot < 0)
			{
				newPot = 0;
			}
			this.RewardPot = newPot;
			this.ircClient.SendChannelMessage(string.Format(this.chatOutput_PimpPotBalance, this.RewardPot), true);
			if (this.RewardPot > TwitchManager.LeaderboardStats.LargestPimpPot)
			{
				TwitchManager.LeaderboardStats.LargestPimpPot = this.RewardPot;
			}
		}

		public void SetBitPot(int newPot)
		{
			if (newPot < 0)
			{
				newPot = 0;
			}
			this.BitPot = newPot;
			this.ircClient.SendChannelMessage(string.Format(this.chatOutput_BitPotBalance, this.BitPot), true);
			if (this.BitPot > TwitchManager.LeaderboardStats.LargestBitPot)
			{
				TwitchManager.LeaderboardStats.LargestBitPot = this.BitPot;
			}
		}

		public void SetCooldown(float newCooldownTime, TwitchManager.CooldownTypes newCooldownType, bool displayToChannel = false, bool playCooldownSound = true)
		{
			if (this.LocalPlayer == null)
			{
				return;
			}
			if (this.CooldownType == newCooldownType && this.CooldownTime == newCooldownTime)
			{
				return;
			}
			if (newCooldownType != TwitchManager.CooldownTypes.MaxReachedWaiting && newCooldownType != TwitchManager.CooldownTypes.SafeCooldown && newCooldownType != TwitchManager.CooldownTypes.SafeCooldownExit && newCooldownType != TwitchManager.CooldownTypes.None)
			{
				this.LocalPlayer.HandleTwitchActionsTempEnabled((newCooldownTime > 15f) ? EntityPlayer.TwitchActionsStates.TempDisabled : EntityPlayer.TwitchActionsStates.TempDisabledEnding);
			}
			else if (newCooldownType == TwitchManager.CooldownTypes.None)
			{
				this.LocalPlayer.HandleTwitchActionsTempEnabled(EntityPlayer.TwitchActionsStates.Enabled);
			}
			this.CooldownType = newCooldownType;
			this.CooldownTime = newCooldownTime;
			if (displayToChannel)
			{
				this.ircClient.SendChannelMessage(string.Format(this.chatOutput_CooldownTime, newCooldownTime), true);
			}
			this.HandleCooldownActionLocking();
			if (playCooldownSound && this.LocalPlayer != null && newCooldownType != TwitchManager.CooldownTypes.None)
			{
				Manager.BroadcastPlayByLocalPlayer(this.LocalPlayer.position, "twitch_cooldown_started");
			}
		}

		public bool ForceEndCooldown(bool playEndSound = true)
		{
			if (this.IsReady && (this.CooldownType == TwitchManager.CooldownTypes.MaxReached || this.CooldownType == TwitchManager.CooldownTypes.Time))
			{
				this.SetCooldown(0f, TwitchManager.CooldownTypes.None, false, true);
				this.CurrentCooldownFill = 0f;
				if (playEndSound)
				{
					Manager.BroadcastPlayByLocalPlayer(this.LocalPlayer.position, "twitch_end_cooldown");
				}
				return true;
			}
			return false;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void ConfirmAction(TwitchActionEntry entry)
		{
			TwitchAction action = entry.Action;
			if (!entry.IsRespawn)
			{
				action.SetQueued();
			}
			if (!entry.IsRespawn)
			{
				if (!entry.IsBitAction && this.PimpPotType != TwitchManager.PimpPotSettings.Disabled)
				{
					int num = (int)EffectManager.GetValue(PassiveEffects.TwitchAddPimpPot, null, (float)action.ModifiedCost * this.ActionPotPercentage, this.LocalPlayer, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
					if (num > 0)
					{
						this.RewardPot += num;
						if (this.RewardPot > TwitchManager.LeaderboardStats.LargestPimpPot)
						{
							TwitchManager.LeaderboardStats.LargestPimpPot = this.RewardPot;
						}
					}
				}
				if (entry.IsBitAction && entry.BitsUsed > 0)
				{
					int num2 = (int)((float)(entry.BitsUsed - entry.CreditsUsed) * this.BitPotPercentage);
					if (num2 > 0)
					{
						this.BitPot += num2;
						if (this.BitPot > TwitchManager.LeaderboardStats.LargestBitPot)
						{
							TwitchManager.LeaderboardStats.LargestBitPot = this.BitPot;
						}
					}
				}
			}
			this.AddCooldownForAction(action);
			if (!entry.IsRespawn)
			{
				ViewerEntry viewerEntry = this.ViewerData.GetViewerEntry(entry.UserName);
				if (EffectManager.GetValue(PassiveEffects.DisableGameEventNotify, null, 0f, this.LocalPlayer, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false) == 0f)
				{
					if (action.DelayNotify)
					{
						GameManager.Instance.StartCoroutine(this.onDelayedActionNotify(entry, viewerEntry));
					}
					else
					{
						this.DisplayActionNotification(entry, viewerEntry);
					}
				}
				this.AddActionHistory(entry, viewerEntry, TwitchActionHistoryEntry.EntryStates.Waiting);
			}
		}

		public void ShowReimburseMessage(string userName, int bitsUsed, TwitchAction action, ViewerEntry viewerEntry = null)
		{
			if (bitsUsed > 0)
			{
				if (viewerEntry == null)
				{
					viewerEntry = this.ViewerData.GetViewerEntry(userName);
				}
				if (viewerEntry == null)
				{
					return;
				}
				string msg = string.Format(this.ingameOutput_RefundedAction, new object[]
				{
					viewerEntry.UserColor,
					userName,
					bitsUsed,
					action.Command
				});
				Debug.LogWarning(string.Format("{0} has been refunded {1} bits for {2}", userName, bitsUsed, action.Command));
				this.AddToInGameChatQueue(msg, "twitch_refund");
			}
		}

		public void ShowReimburseMessage(TwitchActionEntry entry, ViewerEntry viewerEntry = null)
		{
			this.ShowReimburseMessage(entry.UserName, entry.BitsUsed, entry.Action, viewerEntry);
		}

		public TwitchActionHistoryEntry AddActionHistory(TwitchActionEntry entry, ViewerEntry viewerEntry, TwitchActionHistoryEntry.EntryStates startState = TwitchActionHistoryEntry.EntryStates.Waiting)
		{
			if (entry.IsReRun)
			{
				return null;
			}
			if (entry.HistoryEntry == null)
			{
				TwitchActionHistoryEntry twitchActionHistoryEntry = entry.SetupHistoryEntry(viewerEntry);
				twitchActionHistoryEntry.EntryState = startState;
				entry.HistoryEntry = twitchActionHistoryEntry;
				this.ActionHistory.Insert(0, twitchActionHistoryEntry);
				if (this.ActionHistory.Count > 500)
				{
					this.ActionHistory.RemoveAt(this.ActionHistory.Count - 1);
				}
				if (this.ActionHistoryAdded != null)
				{
					this.ActionHistoryAdded();
				}
				return twitchActionHistoryEntry;
			}
			entry.HistoryEntry.EntryState = startState;
			return entry.HistoryEntry;
		}

		public void AddVoteHistory(TwitchVote vote)
		{
			TwitchActionHistoryEntry twitchActionHistoryEntry = new TwitchActionHistoryEntry("Vote", "FFFFFF", null, vote, null);
			this.VoteHistory.Insert(0, twitchActionHistoryEntry);
			twitchActionHistoryEntry.EntryState = TwitchActionHistoryEntry.EntryStates.Completed;
			if (this.VoteHistory.Count > 500)
			{
				this.VoteHistory.RemoveAt(this.VoteHistory.Count - 1);
			}
			if (this.VoteHistoryAdded != null)
			{
				this.VoteHistoryAdded();
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void AddCooldownForAction(TwitchAction action)
		{
			if (action.AddsToCooldown)
			{
				int num = (int)EffectManager.GetValue(PassiveEffects.TwitchAddCooldown, null, (float)action.CooldownAddAmount, this.LocalPlayer, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
				if (num > 0)
				{
					this.AddCooldownAmount(num);
				}
			}
		}

		public void AddCooldownAmount(int amount)
		{
			if (this.CurrentCooldownPreset == null)
			{
				this.GetCooldownMax();
			}
			if (this.CooldownType == TwitchManager.CooldownTypes.QuestCooldown || this.CooldownType == TwitchManager.CooldownTypes.QuestDisabled || this.CooldownType == TwitchManager.CooldownTypes.BloodMoonCooldown || this.CooldownType == TwitchManager.CooldownTypes.BloodMoonDisabled)
			{
				return;
			}
			if (this.IsVoting)
			{
				return;
			}
			if (this.CurrentCooldownPreset != null && this.CurrentCooldownPreset.CooldownType == CooldownPreset.CooldownTypes.Fill)
			{
				if (this.CurrentCooldownFill < this.CurrentCooldownPreset.CooldownFillMax)
				{
					this.CurrentCooldownFill += (float)amount;
					if (this.CurrentCooldownFill >= this.CurrentCooldownPreset.CooldownFillMax)
					{
						this.SetCooldown((float)this.CurrentCooldownPreset.NextCooldownTime, TwitchManager.CooldownTypes.MaxReachedWaiting, false, true);
						if (this.ircClient != null)
						{
							this.ircClient.SendChannelMessage(this.chatOutput_CooldownStarted, true);
						}
					}
				}
				else if (this.CooldownType != TwitchManager.CooldownTypes.MaxReachedWaiting && this.CooldownType != TwitchManager.CooldownTypes.MaxReached)
				{
					this.SetCooldown((float)this.CurrentCooldownPreset.NextCooldownTime, TwitchManager.CooldownTypes.MaxReachedWaiting, false, true);
					if (this.ircClient != null)
					{
						this.ircClient.SendChannelMessage(this.chatOutput_CooldownStarted, true);
					}
				}
			}
			this.UIDirty = true;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public IEnumerator onDelayedActionNotify(TwitchActionEntry entry, ViewerEntry viewerEntry)
		{
			yield return new WaitForSeconds(5f);
			if (this.ircClient != null)
			{
				this.DisplayActionNotification(entry, viewerEntry);
			}
			yield break;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void DisplayActionNotification(TwitchActionEntry entry, ViewerEntry viewerEntry)
		{
			if (entry.HistoryEntry != null && entry.HistoryEntry.EntryState == TwitchActionHistoryEntry.EntryStates.Reimbursed)
			{
				return;
			}
			TwitchAction action = entry.Action;
			if (entry.ChannelNotify)
			{
				string message;
				if (action.PointType == TwitchAction.PointTypes.Bits)
				{
					message = string.Format(this.chatOutput_ActivatedBitAction, new object[]
					{
						entry.UserName,
						action.Command,
						viewerEntry.CombinedPoints,
						entry.Target.EntityName,
						entry.Action.CurrentCost
					});
					Manager.PlayInsidePlayerHead(action.IsPositive ? "twitch_donation" : "twitch_donation_bad", this.LocalPlayer.entityId, 0f, false, false);
				}
				else
				{
					message = string.Format(this.chatOutput_ActivatedAction, new object[]
					{
						entry.UserName,
						action.Command,
						viewerEntry.CombinedPoints,
						entry.Target.EntityName
					});
				}
				this.ircClient.SendChannelMessage(message, true);
			}
			string text = string.Format(this.ingameOutput_ActivatedAction, new object[]
			{
				viewerEntry.UserColor,
				entry.UserName,
				action.Command,
				entry.Target.EntityName
			});
			this.SendServerChatMessage(text);
			this.AddToInGameChatQueue(text, null);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void SendServerChatMessage(string serverMsg)
		{
			if (this.LocalPlayer.IsInParty())
			{
				List<int> memberIdList = this.LocalPlayer.Party.GetMemberIdList(this.LocalPlayer);
				if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
				{
					if (memberIdList == null)
					{
						return;
					}
					using (List<int>.Enumerator enumerator = memberIdList.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							int entityId = enumerator.Current;
							ClientInfo clientInfo = SingletonMonoBehaviour<ConnectionManager>.Instance.Clients.ForEntityId(entityId);
							if (clientInfo != null)
							{
								clientInfo.SendPackage(NetPackageManager.GetPackage<NetPackageSimpleChat>().Setup(serverMsg));
							}
						}
						return;
					}
				}
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageSimpleChat>().Setup(serverMsg, memberIdList), false);
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void RefreshCommands(bool displayMessage)
		{
			if (this.LocalPlayer.unModifiedGameStage != this.HighestGameStage)
			{
				int highestGameStage = this.HighestGameStage;
				this.HighestGameStage = this.LocalPlayer.unModifiedGameStage;
				this.GetCooldownMax();
				if (this.UseProgression && !this.OverrideProgession)
				{
					this.ActionMessages.Clear();
					this.SetupAvailableCommandsWithOutput(highestGameStage, displayMessage);
					this.ResetDailyCommands(this.lastGameDay, highestGameStage);
					this.HandleCooldownActionLocking();
					this.commandsAvailable = this.AvailableCommands.Count;
				}
			}
		}

		public void ToggleTwitchActive()
		{
			this.twitchActive = !this.twitchActive;
			this.ActionMessages.Clear();
			this.HandleCooldownActionLocking();
		}

		public void SetTwitchActive(bool newActive)
		{
			if (this.twitchActive != newActive)
			{
				this.twitchActive = newActive;
				this.ActionMessages.Clear();
				this.HandleCooldownActionLocking();
			}
		}

		public void ResetPrices()
		{
			bool flag = false;
			using (Dictionary<string, TwitchAction>.ValueCollection.Enumerator enumerator = TwitchActionManager.TwitchActions.Values.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current.UpdateCost(this.BitPriceMultiplier))
					{
						flag = true;
					}
				}
			}
			if (flag)
			{
				this.resetCommandsNeeded = true;
			}
		}

		public void ResetPricesToDefault()
		{
			bool flag = false;
			foreach (TwitchAction twitchAction in TwitchActionManager.TwitchActions.Values)
			{
				twitchAction.ResetToDefaultCost();
				if (twitchAction.UpdateCost(this.BitPriceMultiplier))
				{
					flag = true;
				}
			}
			if (flag)
			{
				this.resetCommandsNeeded = true;
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void ResetCommands()
		{
			this.ActionMessages.Clear();
			this.SetupAvailableCommands();
			if (this.UseProgression && !this.OverrideProgession)
			{
				this.ResetDailyCommands(this.lastGameDay, -1);
			}
			this.HandleCooldownActionLocking();
			this.resetCommandsNeeded = false;
		}

		public void SetUseProgression(bool useProgression)
		{
			if (this.UseProgression != useProgression)
			{
				this.UseProgression = useProgression;
				if (this.InitState == TwitchManager.InitStates.Ready)
				{
					this.resetCommandsNeeded = true;
				}
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public int GetCommandCount()
		{
			int num = 0;
			if (this.CooldownType == TwitchManager.CooldownTypes.BloodMoonDisabled || this.CooldownType == TwitchManager.CooldownTypes.Time || this.CooldownType == TwitchManager.CooldownTypes.QuestDisabled)
			{
				return 0;
			}
			foreach (string key in TwitchActionManager.TwitchActions.Keys)
			{
				TwitchAction twitchAction = TwitchActionManager.TwitchActions[key];
				if (twitchAction.IsInPreset(this.CurrentActionPreset))
				{
					if (this.CooldownType == TwitchManager.CooldownTypes.MaxReachedWaiting)
					{
						if (twitchAction.WaitingBlocked)
						{
							continue;
						}
					}
					else if (twitchAction.CooldownBlocked && this.CooldownTime > 0f)
					{
						continue;
					}
					int startGameStage = twitchAction.StartGameStage;
					if (startGameStage == -1 || startGameStage <= this.HighestGameStage)
					{
						num++;
					}
				}
			}
			return num;
		}

		public TwitchRespawnEntry GetRespawnEntry(string username, EntityPlayer target, TwitchAction action)
		{
			for (int i = 0; i < this.RespawnEntries.Count; i++)
			{
				if (this.RespawnEntries[i].CheckRespawn(username, target, action))
				{
					return this.RespawnEntries[i];
				}
			}
			TwitchRespawnEntry twitchRespawnEntry = new TwitchRespawnEntry(username, GameEventManager.Current.Random.RandomRange(action.MinRespawnCount, action.MaxRespawnCount), target, action);
			this.RespawnEntries.Add(twitchRespawnEntry);
			return twitchRespawnEntry;
		}

		public void CheckKiller(EntityPlayer player, EntityAlive killer, Vector3i pos)
		{
			if (this.LocalPlayer == null)
			{
				return;
			}
			if (this.twitchPlayerDeathsThisFrame.Contains(player))
			{
				return;
			}
			if (player == this.LocalPlayer && this.VotingManager != null && this.VotingManager.VotingEnabled && this.VotingManager.CurrentEvent != null)
			{
				this.HandleVoteKill(null);
				this.VotingManager.ResetVoteOnDeath();
				this.twitchPlayerDeathsThisFrame.Add(player);
				return;
			}
			bool flag;
			if (player == this.LocalPlayer)
			{
				flag = true;
			}
			else
			{
				if (this.LocalPlayer.Party == null || !this.LocalPlayer.Party.ContainsMember(player))
				{
					return;
				}
				flag = false;
			}
			TwitchActionEntry twitchActionEntry = null;
			TwitchEventActionEntry twitchEventActionEntry = null;
			TwitchVoteEntry twitchVoteEntry = null;
			if (killer == null)
			{
				int i = this.liveBlockList.Count - 1;
				while (i >= 0)
				{
					if (this.liveBlockList[i].CheckPos(pos))
					{
						TwitchSpawnedBlocksEntry twitchSpawnedBlocksEntry = this.liveBlockList[i];
						twitchActionEntry = twitchSpawnedBlocksEntry.Action;
						twitchEventActionEntry = twitchSpawnedBlocksEntry.Event;
						twitchVoteEntry = twitchSpawnedBlocksEntry.Vote;
						if (twitchSpawnedBlocksEntry.RespawnEntry != null && (flag || twitchActionEntry.Target == player))
						{
							this.RespawnEntries.Remove(twitchSpawnedBlocksEntry.RespawnEntry);
							break;
						}
						break;
					}
					else
					{
						i--;
					}
				}
			}
			else
			{
				int j = this.liveList.Count - 1;
				while (j >= 0)
				{
					if (this.liveList[j].SpawnedEntity == killer)
					{
						TwitchSpawnedEntityEntry twitchSpawnedEntityEntry = this.liveList[j];
						twitchActionEntry = twitchSpawnedEntityEntry.Action;
						twitchEventActionEntry = twitchSpawnedEntityEntry.Event;
						twitchVoteEntry = twitchSpawnedEntityEntry.Vote;
						if (twitchSpawnedEntityEntry.RespawnEntry != null && (flag || twitchSpawnedEntityEntry.Action.Target == player))
						{
							this.RespawnEntries.Remove(twitchSpawnedEntityEntry.RespawnEntry);
							break;
						}
						break;
					}
					else
					{
						j--;
					}
				}
			}
			if (twitchActionEntry == null && twitchEventActionEntry == null && twitchVoteEntry == null)
			{
				for (int k = this.recentlyDeadList.Count - 1; k >= 0; k--)
				{
					if (this.recentlyDeadList[k].SpawnedEntity == killer)
					{
						twitchActionEntry = this.recentlyDeadList[k].Action;
						twitchEventActionEntry = this.recentlyDeadList[k].Event;
						twitchVoteEntry = this.recentlyDeadList[k].Vote;
						break;
					}
				}
			}
			if (twitchActionEntry != null || twitchEventActionEntry != null)
			{
				string text = (twitchActionEntry != null) ? twitchActionEntry.UserName : twitchEventActionEntry.UserName;
				string text2 = (twitchActionEntry != null) ? twitchActionEntry.Action.Command : twitchEventActionEntry.Event.EventTitle;
				if (twitchEventActionEntry != null && (twitchEventActionEntry.Event.EventType == BaseTwitchEventEntry.EventTypes.HypeTrain || twitchEventActionEntry.Event.EventType == BaseTwitchEventEntry.EventTypes.CreatorGoal))
				{
					if (flag)
					{
						int rewardAmount;
						bool flag2;
						if (twitchEventActionEntry.Event.EventType == BaseTwitchEventEntry.EventTypes.HypeTrain)
						{
							TwitchHypeTrainEventEntry twitchHypeTrainEventEntry = (TwitchHypeTrainEventEntry)twitchEventActionEntry.Event;
							rewardAmount = twitchHypeTrainEventEntry.RewardAmount;
							flag2 = (twitchHypeTrainEventEntry.RewardType > TwitchAction.PointTypes.PP);
						}
						else
						{
							TwitchCreatorGoalEventEntry twitchCreatorGoalEventEntry = (TwitchCreatorGoalEventEntry)twitchEventActionEntry.Event;
							rewardAmount = twitchCreatorGoalEventEntry.RewardAmount;
							flag2 = (twitchCreatorGoalEventEntry.RewardType > TwitchAction.PointTypes.PP);
						}
						this.ViewerData.AddPointsAll((!flag2) ? rewardAmount : 0, flag2 ? rewardAmount : 0, false);
						string arg = flag2 ? Localization.Get("TwitchPoints_SP", false) : Localization.Get("TwitchPoints_PP", false);
						string text3 = string.Format(this.ingameOutput_KilledByHypeTrain, rewardAmount, arg, this.LocalPlayer.EntityName);
						GameManager.ShowTooltip(this.LocalPlayer, text3, false);
						GameManager.Instance.ChatMessageServer(null, EChatType.Global, -1, Utils.CreateGameMessage(this.Authentication.userName, text3), null, EMessageSender.None);
						this.ircClient.SendChannelMessage(string.Format(this.chatOutput_KilledByHypeTrain, rewardAmount, arg, this.LocalPlayer.EntityName), true);
						TwitchManager.DeathText = string.Format(this.ingameHypeTrainDeathScreen_Message, rewardAmount, arg);
					}
					this.twitchPlayerDeathsThisFrame.Add(player);
					return;
				}
				if (flag)
				{
					ViewerEntry viewerEntry = this.ViewerData.GetViewerEntry(text);
					if ((twitchActionEntry != null && twitchActionEntry.IsBitAction) || (twitchEventActionEntry != null && twitchEventActionEntry.Event.RewardsBitPot))
					{
						if (this.BitPot > 0)
						{
							viewerEntry.BitCredits += this.BitPot;
						}
						string text4 = (this.PimpPotType == TwitchManager.PimpPotSettings.EnabledSP) ? Localization.Get("TwitchPoints_SP", false) : Localization.Get("TwitchPoints_PP", false);
						if (this.PimpPotType != TwitchManager.PimpPotSettings.Disabled)
						{
							if (this.PimpPotType == TwitchManager.PimpPotSettings.EnabledSP)
							{
								viewerEntry.SpecialPoints += (float)this.RewardPot;
							}
							else
							{
								viewerEntry.StandardPoints += (float)this.RewardPot;
							}
						}
						this.ircClient.SendChannelMessage(string.Format(this.chatOutput_KilledByBits, new object[]
						{
							text,
							viewerEntry.BitCredits,
							this.BitPot,
							player.EntityName,
							this.RewardPot,
							text4
						}), true);
						string text5 = string.Format(this.ingameOutput_KilledByBits, new object[]
						{
							viewerEntry.UserColor,
							text,
							this.BitPot,
							player.EntityName,
							this.RewardPot,
							text4
						});
						GameManager.ShowTooltip(this.LocalPlayer, text5, false);
						GameManager.Instance.ChatMessageServer(null, EChatType.Global, -1, Utils.CreateGameMessage(this.Authentication.userName, text5), null, EMessageSender.None);
						TwitchManager.DeathText = string.Format(this.ingameBitsDeathScreen_Message, new object[]
						{
							viewerEntry.UserColor,
							text,
							text2,
							this.BitPot,
							this.RewardPot,
							text4
						});
						QuestEventManager.Current.TwitchEventReceived(TwitchObjectiveTypes.BitPot, "");
						this.BitPot = 0;
						this.RewardPot = TwitchManager.PimpPotDefault;
					}
					else
					{
						string text6 = (this.PimpPotType == TwitchManager.PimpPotSettings.EnabledSP) ? Localization.Get("TwitchPoints_SP", false) : Localization.Get("TwitchPoints_PP", false);
						if (this.PimpPotType != TwitchManager.PimpPotSettings.Disabled)
						{
							if (this.PimpPotType == TwitchManager.PimpPotSettings.EnabledSP)
							{
								viewerEntry.SpecialPoints += (float)this.RewardPot;
							}
							else
							{
								viewerEntry.StandardPoints += (float)this.RewardPot;
							}
							this.ircClient.SendChannelMessage(string.Format(this.chatOutput_KilledStreamer, new object[]
							{
								text,
								viewerEntry.CombinedPoints,
								this.RewardPot,
								text6,
								player.EntityName
							}), true);
							string text7 = string.Format(this.ingameOutput_KilledStreamer, new object[]
							{
								viewerEntry.UserColor,
								text,
								this.RewardPot,
								text6,
								player.EntityName
							});
							GameManager.ShowTooltip(TwitchManager.Current.LocalPlayer, text7, false);
							GameManager.Instance.ChatMessageServer(null, EChatType.Global, -1, Utils.CreateGameMessage(this.Authentication.userName, text7), null, EMessageSender.None);
							QuestEventManager.Current.TwitchEventReceived(TwitchObjectiveTypes.PimpPot, "");
						}
						TwitchManager.DeathText = string.Format(this.ingameDeathScreen_Message, new object[]
						{
							viewerEntry.UserColor,
							text,
							text2,
							this.RewardPot,
							text6
						});
						this.RewardPot = TwitchManager.PimpPotDefault;
					}
					TwitchManager.LeaderboardStats.CheckTopKiller(TwitchManager.LeaderboardStats.AddKill(text, viewerEntry.UserColor));
					this.AddKillToLeaderboard(text, viewerEntry.UserColor);
					this.twitchPlayerDeathsThisFrame.Add(player);
				}
				else
				{
					if (this.PimpPotType != TwitchManager.PimpPotSettings.Disabled)
					{
						ViewerEntry viewerEntry2 = this.ViewerData.GetViewerEntry(text);
						int num = Mathf.Min(this.PartyKillRewardMax, this.RewardPot);
						viewerEntry2.StandardPoints += (float)num;
						this.ircClient.SendChannelMessage(string.Format(this.chatOutput_KilledParty, new object[]
						{
							text,
							viewerEntry2.CombinedPoints,
							num,
							player.EntityName
						}), true);
						string text8 = string.Format(this.ingameOutput_KilledParty, new object[]
						{
							viewerEntry2.UserColor,
							text,
							num,
							player.EntityName
						});
						GameManager.ShowTooltip(TwitchManager.Current.LocalPlayer, text8, false);
						GameManager.Instance.ChatMessageServer(null, EChatType.Global, -1, Utils.CreateGameMessage(this.Authentication.userName, text8), null, EMessageSender.None);
					}
					this.twitchPlayerDeathsThisFrame.Add(player);
				}
			}
			else if (twitchVoteEntry != null && flag && this.VotingManager != null && !twitchVoteEntry.Complete)
			{
				this.HandleVoteKill(twitchVoteEntry);
			}
			if (flag)
			{
				this.VotingManager.ResetVoteOnDeath();
			}
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public void HandleVoteKill(TwitchVoteEntry voteEntry)
		{
			List<string> list = this.VotingManager.HandleKiller(voteEntry);
			if (list != null && list.Count > 0)
			{
				for (int i = 0; i < list.Count; i++)
				{
					this.ViewerData.GetViewerEntry(list[i]).StandardPoints += (float)this.VotingManager.ViewerDefeatReward;
				}
				string text = string.Format(this.ingameOutput_KilledByVote, this.VotingManager.ViewerDefeatReward, this.LocalPlayer.EntityName);
				GameManager.ShowTooltip(TwitchManager.Current.LocalPlayer, text, false);
				GameManager.Instance.ChatMessageServer(null, EChatType.Global, -1, Utils.CreateGameMessage(this.Authentication.userName, text), null, EMessageSender.None);
				this.ircClient.SendChannelMessage(string.Format(this.chatOutput_KilledByVote, this.VotingManager.ViewerDefeatReward, this.LocalPlayer.EntityName), true);
				TwitchManager.DeathText = string.Format(this.ingameVoteDeathScreen_Message, this.VotingManager.ViewerDefeatReward);
				list.Clear();
			}
			if (voteEntry != null)
			{
				voteEntry.Complete = true;
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void AddKillToLeaderboard(string username, string usercolor)
		{
			bool flag = false;
			for (int i = 0; i < this.Leaderboard.Count; i++)
			{
				if (this.Leaderboard[i].UserName == username)
				{
					this.Leaderboard[i].Kills++;
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				this.Leaderboard.Add(new TwitchLeaderboardEntry(username, usercolor, 1));
			}
		}

		public void ClearLeaderboard()
		{
			this.Leaderboard.Clear();
		}

		public void AddCooldownPreset(CooldownPreset preset)
		{
			if (this.CooldownPresets == null)
			{
				this.CooldownPresets = new List<CooldownPreset>();
			}
			if (preset.IsDefault)
			{
				this.CooldownPresetIndex = this.CooldownPresets.Count;
			}
			this.CooldownPresets.Add(preset);
		}

		public void SetCooldownPreset(int index)
		{
			if (this.InitState == TwitchManager.InitStates.Ready)
			{
				bool flag = this.CurrentCooldownPreset.CooldownType != this.CooldownPresets[index].CooldownType;
				this.CooldownPresetIndex = index;
				this.GetCooldownMax();
				if (flag)
				{
					this.resetCommandsNeeded = true;
					return;
				}
			}
			else
			{
				this.CooldownPresetIndex = index;
			}
		}

		public void SetToDefaultCooldown()
		{
			for (int i = 0; i < this.CooldownPresets.Count; i++)
			{
				if (this.CooldownPresets[i].IsDefault)
				{
					this.SetCooldownPreset(i);
					return;
				}
			}
		}

		public void GetCooldownMax()
		{
			this.CurrentCooldownPreset = this.CooldownPresets[this.CooldownPresetIndex];
			this.CurrentCooldownPreset.SetupCooldownInfo(this.HighestGameStage, this.LocalPlayer);
			this.SetupBloodMoonData();
		}

		public void AddTwitchActionPreset(TwitchActionPreset preset)
		{
			if (this.ActionPresets == null)
			{
				this.ActionPresets = new List<TwitchActionPreset>();
			}
			this.ActionPresets.Add(preset);
			if (preset.IsDefault)
			{
				this.ActionPresetIndex = this.ActionPresets.Count - 1;
				this.CurrentActionPreset = this.ActionPresets[this.ActionPresetIndex];
			}
		}

		public void SetTwitchActionPreset(int index)
		{
			if (this.ActionPresetIndex == index)
			{
				return;
			}
			this.ActionPresets[this.ActionPresetIndex].AddedActions.Clear();
			this.ActionPresets[this.ActionPresetIndex].RemovedActions.Clear();
			this.ActionPresetIndex = index;
			this.CurrentActionPreset = this.ActionPresets[this.ActionPresetIndex];
			this.CurrentActionPreset.HandleCooldowns();
			if (this.InitState == TwitchManager.InitStates.Ready)
			{
				this.resetCommandsNeeded = true;
			}
		}

		public void SetToDefaultActionPreset()
		{
			for (int i = 0; i < this.ActionPresets.Count; i++)
			{
				if (this.ActionPresets[i].IsDefault)
				{
					this.SetTwitchActionPreset(i);
					return;
				}
			}
		}

		public void AddTwitchVotePreset(TwitchVotePreset preset)
		{
			if (this.VotePresets == null)
			{
				this.VotePresets = new List<TwitchVotePreset>();
			}
			this.VotePresets.Add(preset);
			if (preset.IsDefault)
			{
				this.VotePresetIndex = this.VotePresets.Count - 1;
				this.CurrentVotePreset = this.VotePresets[this.VotePresetIndex];
			}
		}

		public void SetTwitchVotePreset(int index)
		{
			if (this.VotePresetIndex == index)
			{
				return;
			}
			this.VotePresetIndex = index;
			this.CurrentVotePreset = this.VotePresets[this.VotePresetIndex];
			if (this.CurrentVotePreset.IsEmpty)
			{
				this.VotingManager.ForceEndVote();
			}
			this.SetupAvailableCommands();
		}

		public void SetToDefaultVotePreset()
		{
			for (int i = 0; i < this.VotePresets.Count; i++)
			{
				if (this.VotePresets[i].IsDefault)
				{
					this.SetTwitchVotePreset(i);
					return;
				}
			}
		}

		public void AddTwitchEventPreset(TwitchEventPreset preset)
		{
			if (this.EventPresets == null)
			{
				this.EventPresets = new List<TwitchEventPreset>();
			}
			this.EventPresets.Add(preset);
			if (preset.IsDefault)
			{
				this.EventPresetIndex = this.EventPresets.Count - 1;
				this.CurrentEventPreset = this.EventPresets[this.EventPresetIndex];
			}
		}

		public void SetTwitchEventPreset(int index, bool oldAllowChannelPointRedeems)
		{
			TwitchEventPreset currentEventPreset = this.CurrentEventPreset;
			this.EventPresetIndex = index;
			this.CurrentEventPreset = this.EventPresets[this.EventPresetIndex];
			if (currentEventPreset != null)
			{
				currentEventPreset.RemoveChannelPointRedemptions(this.AllowChannelPointRedemptions ? this.CurrentEventPreset : null);
			}
			this.SetupTwitchCommands();
			if (this.AllowChannelPointRedemptions && this.CurrentEventPreset != null)
			{
				this.CurrentEventPreset.AddChannelPointRedemptions();
			}
		}

		public TwitchEventPreset GetEventPreset(string name)
		{
			for (int i = 0; i < this.EventPresets.Count; i++)
			{
				if (this.EventPresets[i].Name.EqualsCaseInsensitive(name))
				{
					return this.EventPresets[i];
				}
			}
			return null;
		}

		public void SetToDefaultEventPreset()
		{
			for (int i = 0; i < this.EventPresets.Count; i++)
			{
				if (this.EventPresets[i].IsDefault)
				{
					this.SetTwitchEventPreset(i, this.AllowChannelPointRedemptions);
					return;
				}
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void PubSub_OnSubscriptionRedeemed(object sender, PubSubSubscriptionRedemptionMessage e)
		{
			if (e.user_name == null)
			{
				return;
			}
			TwitchSubEventEntry.SubTierTypes subTier = TwitchSubEventEntry.GetSubTier(e.sub_plan);
			if (e.is_gift)
			{
				if (e.user_name != "ananonymousgifter")
				{
					this.ViewerData.AddGiftSubEntry(e.user_name, StringParsers.ParseSInt32(e.user_id, 0, -1, NumberStyles.Integer), subTier);
				}
				return;
			}
			ViewerEntry viewerEntry = this.ViewerData.GetViewerEntry(e.user_name);
			viewerEntry.UserID = StringParsers.ParseSInt32(e.user_id, 0, -1, NumberStyles.Integer);
			int num = this.ViewerData.GetSubTierPoints(subTier) * this.SubPointModifier;
			if (num > 0)
			{
				viewerEntry.SpecialPoints += (float)num;
				this.ircClient.SendChannelMessage(string.Format(this.chatOutput_Subscribed, new object[]
				{
					e.user_name,
					viewerEntry.CombinedPoints,
					this.GetTierName(subTier),
					num
				}), true);
				string msg = string.Format(this.ingameOutput_Subscribed, e.user_name, this.GetTierName(subTier), num);
				this.AddToInGameChatQueue(msg, null);
			}
			this.HandleSubEvent(e.user_name, e.cumulative_months, subTier);
		}

		public string GetTierName(TwitchSubEventEntry.SubTierTypes tier)
		{
			switch (tier)
			{
			case TwitchSubEventEntry.SubTierTypes.Prime:
				return "Prime";
			case TwitchSubEventEntry.SubTierTypes.Tier1:
				return "1";
			case TwitchSubEventEntry.SubTierTypes.Tier2:
				return "2";
			case TwitchSubEventEntry.SubTierTypes.Tier3:
				return "3";
			default:
				return "1";
			}
		}

		public string GetSubTierRewards(int subModifier)
		{
			if (subModifier == 0)
			{
				return Localization.Get("xuiLightPropShadowsNone", false);
			}
			return string.Format(this.subPointDisplay, this.ViewerData.GetSubTierPoints(TwitchSubEventEntry.SubTierTypes.Tier1) * subModifier, this.ViewerData.GetSubTierPoints(TwitchSubEventEntry.SubTierTypes.Tier2) * subModifier, this.ViewerData.GetSubTierPoints(TwitchSubEventEntry.SubTierTypes.Tier3) * subModifier);
		}

		public string GetGiftSubTierRewards(int subModifier)
		{
			if (subModifier == 0)
			{
				return Localization.Get("xuiLightPropShadowsNone", false);
			}
			return string.Format(this.subPointDisplay, this.ViewerData.GetGiftSubTierPoints(TwitchSubEventEntry.SubTierTypes.Tier1) * subModifier, this.ViewerData.GetGiftSubTierPoints(TwitchSubEventEntry.SubTierTypes.Tier2) * subModifier, this.ViewerData.GetGiftSubTierPoints(TwitchSubEventEntry.SubTierTypes.Tier3) * subModifier);
		}

		public void HandleSubEvent(string username, int months, TwitchSubEventEntry.SubTierTypes tier)
		{
			if (!this.AllowEvents)
			{
				return;
			}
			TwitchSubEventEntry twitchSubEventEntry = this.CurrentEventPreset.HandleSubEvent(months, tier);
			if (twitchSubEventEntry != null)
			{
				ViewerEntry viewerEntry = this.ViewerData.GetViewerEntry(username);
				TwitchEventActionEntry twitchEventActionEntry = new TwitchEventActionEntry();
				twitchEventActionEntry.UserName = username;
				twitchEventActionEntry.Event = twitchSubEventEntry;
				this.EventQueue.Add(twitchEventActionEntry);
				twitchEventActionEntry.Event.HandleInstant(username, this);
				this.ircClient.SendChannelMessage(string.Format(this.chatOutput_SubEvent, twitchSubEventEntry.EventTitle, username, viewerEntry.CombinedPoints), true);
			}
		}

		public void HandleGiftSubEvent(string username, int giftCounts, TwitchSubEventEntry.SubTierTypes tier)
		{
			if (!this.AllowEvents)
			{
				return;
			}
			TwitchSubEventEntry twitchSubEventEntry = this.CurrentEventPreset.HandleGiftSubEvent(giftCounts, tier);
			if (twitchSubEventEntry != null)
			{
				ViewerEntry viewerEntry = this.ViewerData.GetViewerEntry(username);
				TwitchEventActionEntry twitchEventActionEntry = new TwitchEventActionEntry();
				twitchEventActionEntry.UserName = username;
				twitchEventActionEntry.Event = twitchSubEventEntry;
				this.EventQueue.Add(twitchEventActionEntry);
				twitchEventActionEntry.Event.HandleInstant(username, this);
				this.ircClient.SendChannelMessage(string.Format(this.chatOutput_GiftSubEvent, twitchSubEventEntry.EventTitle, username, viewerEntry.CombinedPoints), true);
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void PubSub_OnBitsRedeemed(object sender, PubSubBitRedemptionMessage.BitRedemptionData e)
		{
			if (e.user_name == null)
			{
				return;
			}
			ViewerEntry viewerEntry = this.ViewerData.GetViewerEntry(e.user_name);
			int num = e.bits_used * this.BitPointModifier;
			viewerEntry.UserID = StringParsers.ParseSInt32(e.user_id, 0, -1, NumberStyles.Integer);
			viewerEntry.SpecialPoints += (float)num;
			this.ircClient.SendChannelMessage(string.Format(this.chatOutput_DonateBits, new object[]
			{
				e.user_name,
				viewerEntry.CombinedPoints,
				e.bits_used,
				num
			}), true);
			string msg = string.Format(this.ingameOutput_DonateBits, e.user_name, e.bits_used, num);
			this.AddToInGameChatQueue(msg, null);
			this.HandleBitRedeem(e.user_name, e.bits_used, viewerEntry);
		}

		public void HandleBitRedeem(string userName, int bitAmount, ViewerEntry viewerEntry = null)
		{
			if (!this.AllowEvents)
			{
				return;
			}
			TwitchEventEntry twitchEventEntry = this.CurrentEventPreset.HandleBitRedeem(bitAmount);
			if (twitchEventEntry != null)
			{
				if (viewerEntry == null)
				{
					viewerEntry = this.ViewerData.GetViewerEntry(userName);
				}
				TwitchEventActionEntry twitchEventActionEntry = new TwitchEventActionEntry();
				twitchEventActionEntry.UserName = userName;
				twitchEventActionEntry.Event = twitchEventEntry;
				this.EventQueue.Add(twitchEventActionEntry);
				twitchEventActionEntry.Event.HandleInstant(userName, this);
				this.ircClient.SendChannelMessage(string.Format(this.chatOutput_BitEvent, twitchEventEntry.EventTitle, userName, viewerEntry.CombinedPoints), true);
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void PubSub_OnChannelPointsRedeemed(object sender, PubSubChannelPointMessage.ChannelRedemptionData e)
		{
			this.HandleChannelPointsRedeem(e.redemption.reward.title, e.redemption.user.display_name.ToLower());
		}

		public void HandleChannelPointsRedeem(string title, string userName)
		{
			if (!this.AllowEvents)
			{
				return;
			}
			TwitchChannelPointEventEntry twitchChannelPointEventEntry = this.CurrentEventPreset.HandleChannelPointsRedeem(title);
			if (twitchChannelPointEventEntry != null)
			{
				ViewerEntry viewerEntry = this.ViewerData.GetViewerEntry(userName);
				TwitchEventActionEntry twitchEventActionEntry = new TwitchEventActionEntry();
				twitchEventActionEntry.UserName = userName;
				twitchEventActionEntry.Event = twitchChannelPointEventEntry;
				this.EventQueue.Add(twitchEventActionEntry);
				twitchEventActionEntry.Event.HandleInstant(userName, this);
				this.ircClient.SendChannelMessage(string.Format(this.chatOutput_ChannelPointEvent, twitchChannelPointEventEntry.EventTitle, userName, viewerEntry.CombinedPoints), true);
				QuestEventManager.Current.TwitchEventReceived(TwitchObjectiveTypes.ChannelPointRedeems, twitchChannelPointEventEntry.EventName);
			}
		}

		public void HandleRaid(string userName, int userID, int viewerAmount)
		{
			ViewerEntry viewerEntry = this.ViewerData.GetViewerEntry(userName);
			if (viewerAmount >= this.RaidViewerMinimum && this.RaidPointAdd > 0)
			{
				viewerEntry.UserID = userID;
				viewerEntry.SpecialPoints += (float)this.RaidPointAdd;
				this.ircClient.SendChannelMessage(string.Format(this.chatOutput_RaidPoints, new object[]
				{
					userName,
					viewerEntry.CombinedPoints,
					viewerAmount,
					this.RaidPointAdd
				}), true);
				string msg = string.Format(this.ingameOutput_RaidPoints, userName, viewerAmount, this.RaidPointAdd);
				this.AddToInGameChatQueue(msg, null);
			}
			this.HandleRaidRedeem(userName, viewerAmount, viewerEntry);
		}

		public void HandleRaidRedeem(string userName, int viewerAmount, ViewerEntry viewerEntry = null)
		{
			if (!this.AllowEvents)
			{
				return;
			}
			TwitchEventEntry twitchEventEntry = this.CurrentEventPreset.HandleRaid(viewerAmount);
			if (twitchEventEntry != null)
			{
				if (viewerEntry == null)
				{
					viewerEntry = this.ViewerData.GetViewerEntry(userName);
				}
				TwitchEventActionEntry twitchEventActionEntry = new TwitchEventActionEntry();
				twitchEventActionEntry.UserName = userName;
				twitchEventActionEntry.Event = twitchEventEntry;
				this.EventQueue.Add(twitchEventActionEntry);
				twitchEventActionEntry.Event.HandleInstant(userName, this);
				this.ircClient.SendChannelMessage(string.Format(this.chatOutput_RaidEvent, new object[]
				{
					twitchEventEntry.EventTitle,
					userName,
					viewerEntry.CombinedPoints,
					viewerAmount
				}), true);
			}
		}

		public void HandleCharity(string userName, int userID, int charityAmount)
		{
			ViewerEntry viewerEntry = this.ViewerData.GetViewerEntry(userName);
			int num = charityAmount * this.BitPointModifier;
			viewerEntry.UserID = userID;
			viewerEntry.SpecialPoints += (float)num;
			this.ircClient.SendChannelMessage(string.Format(this.chatOutput_DonateCharity, new object[]
			{
				userName,
				viewerEntry.CombinedPoints,
				charityAmount,
				num
			}), true);
			string msg = string.Format(this.ingameOutput_DonateCharity, userName, charityAmount, num);
			this.AddToInGameChatQueue(msg, null);
			this.HandleCharityRedeem(userName, charityAmount, viewerEntry);
		}

		public void HandleCharityRedeem(string userName, int charityAmount, ViewerEntry viewerEntry = null)
		{
			if (!this.AllowEvents)
			{
				return;
			}
			TwitchEventEntry twitchEventEntry = this.CurrentEventPreset.HandleCharityRedeem(charityAmount);
			if (twitchEventEntry != null)
			{
				if (viewerEntry == null)
				{
					viewerEntry = this.ViewerData.GetViewerEntry(userName);
				}
				TwitchEventActionEntry twitchEventActionEntry = new TwitchEventActionEntry();
				twitchEventActionEntry.UserName = userName;
				twitchEventActionEntry.Event = twitchEventEntry;
				this.EventQueue.Add(twitchEventActionEntry);
				twitchEventActionEntry.Event.HandleInstant(userName, this);
				this.ircClient.SendChannelMessage(string.Format(this.chatOutput_CharityEvent, twitchEventEntry.EventTitle, userName, viewerEntry.CombinedPoints), true);
			}
		}

		public void StartHypeTrain()
		{
			this.HypeTrainLevel = 1;
			this.HandleHypeTrainRedeem(this.HypeTrainLevel);
		}

		public void IncrementHypeTrainLevel()
		{
			this.HypeTrainLevel++;
			this.HandleHypeTrainRedeem(this.HypeTrainLevel);
		}

		public void EndHypeTrain()
		{
			this.HypeTrainLevel = 0;
		}

		public void HandleHypeTrainRedeem(int hypeTrainLevel)
		{
			if (!this.AllowEvents)
			{
				return;
			}
			TwitchEventEntry twitchEventEntry = this.CurrentEventPreset.HandleHypeTrainRedeem(hypeTrainLevel);
			if (twitchEventEntry != null)
			{
				TwitchEventActionEntry twitchEventActionEntry = new TwitchEventActionEntry();
				twitchEventActionEntry.UserName = " ";
				twitchEventActionEntry.Event = twitchEventEntry;
				this.EventQueue.Add(twitchEventActionEntry);
				twitchEventActionEntry.Event.HandleInstant(twitchEventActionEntry.UserName, this);
				this.ircClient.SendChannelMessage(string.Format(this.chatOutput_HypeTrainEvent, twitchEventEntry.EventTitle, hypeTrainLevel), true);
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void PubSub_OnGoalAchieved(object sender, PubSubGoalMessage.Goal e)
		{
			this.HandleCreatorGoalRedeem(e.contributionType.ToLower());
		}

		public void HandleCreatorGoalRedeem(string goalType)
		{
			if (!this.AllowEvents)
			{
				return;
			}
			TwitchCreatorGoalEventEntry twitchCreatorGoalEventEntry = this.CurrentEventPreset.HandleCreatorGoalEvent(goalType);
			if (twitchCreatorGoalEventEntry != null)
			{
				TwitchEventActionEntry twitchEventActionEntry = new TwitchEventActionEntry();
				twitchEventActionEntry.UserName = " ";
				twitchEventActionEntry.Event = twitchCreatorGoalEventEntry;
				this.EventQueue.Add(twitchEventActionEntry);
				twitchEventActionEntry.Event.HandleInstant(twitchEventActionEntry.UserName, this);
				this.ircClient.SendChannelMessage(string.Format(this.chatOutput_CreatorGoalEvent, twitchCreatorGoalEventEntry.EventTitle), true);
			}
		}

		public void HandleEventQueue()
		{
			if (!this.twitchActive)
			{
				return;
			}
			for (int i = 0; i < this.EventQueue.Count; i++)
			{
				TwitchEventActionEntry twitchEventActionEntry = this.EventQueue[i];
				if (!twitchEventActionEntry.IsSent && twitchEventActionEntry.HandleEvent(this))
				{
					Manager.BroadcastPlayByLocalPlayer(this.LocalPlayer.position, "twitch_custom_event");
					if (!twitchEventActionEntry.IsRetry)
					{
						TwitchActionHistoryEntry twitchActionHistoryEntry = new TwitchActionHistoryEntry(twitchEventActionEntry.UserName, "FFFFFF", null, null, twitchEventActionEntry);
						twitchActionHistoryEntry.EventEntry = twitchEventActionEntry;
						twitchEventActionEntry.HistoryEntry = twitchActionHistoryEntry;
						this.EventHistory.Insert(0, twitchActionHistoryEntry);
						if (this.EventHistory.Count > 500)
						{
							this.EventHistory.RemoveAt(this.EventHistory.Count - 1);
						}
						if (this.EventHistoryAdded != null)
						{
							this.EventHistoryAdded();
						}
					}
					return;
				}
			}
		}

		public bool TwitchActive
		{
			get
			{
				return this.twitchActive;
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public int saveViewerDataThreaded(ThreadManager.ThreadInfo _threadInfo)
		{
			PooledExpandableMemoryStream[] array = (PooledExpandableMemoryStream[])_threadInfo.parameter;
			string arg = SingletonMonoBehaviour<ConnectionManager>.Instance.IsClient ? GameIO.GetSaveGameLocalDir() : GameIO.GetSaveGameDir();
			string text = string.Format("{0}/{1}", arg, "twitch.dat");
			if (SdFile.Exists(text))
			{
				SdFile.Copy(text, string.Format("{0}/{1}", arg, "twitch.dat.bak"), true);
			}
			array[0].Position = 0L;
			StreamUtils.WriteStreamToFile(array[0], text);
			MemoryPools.poolMemoryStream.FreeSync(array[0]);
			string arg2 = GameIO.GetUserGameDataDir() + "/Twitch/" + TwitchManager.MainFileVersion.ToString();
			text = string.Format("{0}/{1}", arg2, "twitch_main.dat");
			if (SdFile.Exists(text))
			{
				SdFile.Copy(text, string.Format("{0}/{1}", arg2, "twitch_main.dat.bak"), true);
			}
			array[1].Position = 0L;
			StreamUtils.WriteStreamToFile(array[1], text);
			MemoryPools.poolMemoryStream.FreeSync(array[1]);
			return -1;
		}

		public void LoadViewerData()
		{
			string arg = SingletonMonoBehaviour<ConnectionManager>.Instance.IsClient ? GameIO.GetSaveGameLocalDir() : GameIO.GetSaveGameDir();
			string path = string.Format("{0}/{1}", arg, "twitch.dat");
			if (SdFile.Exists(path))
			{
				try
				{
					using (Stream stream = SdFile.OpenRead(path))
					{
						using (PooledBinaryReader pooledBinaryReader = MemoryPools.poolBinaryReader.AllocSync(false))
						{
							pooledBinaryReader.SetBaseStream(stream);
							this.Read(pooledBinaryReader);
						}
					}
				}
				catch (Exception)
				{
					path = string.Format("{0}/{1}", arg, "twitch.dat.bak");
					if (SdFile.Exists(path))
					{
						using (Stream stream2 = SdFile.OpenRead(path))
						{
							using (PooledBinaryReader pooledBinaryReader2 = MemoryPools.poolBinaryReader.AllocSync(false))
							{
								pooledBinaryReader2.SetBaseStream(stream2);
								this.Read(pooledBinaryReader2);
							}
						}
					}
				}
			}
		}

		public void LoadSpecialViewerData()
		{
			string path = string.Format("{0}/{1}", GameIO.GetSaveGameRootDir(), "twitch_special.dat");
			if (SdFile.Exists(path))
			{
				try
				{
					using (Stream stream = SdFile.OpenRead(path))
					{
						using (PooledBinaryReader pooledBinaryReader = MemoryPools.poolBinaryReader.AllocSync(false))
						{
							pooledBinaryReader.SetBaseStream(stream);
							this.ReadSpecial(pooledBinaryReader);
						}
					}
				}
				catch (Exception)
				{
					path = string.Format("{0}/{1}", GameIO.GetSaveGameRootDir(), "twitch_special.dat.bak");
					if (SdFile.Exists(path))
					{
						using (Stream stream2 = SdFile.OpenRead(path))
						{
							using (PooledBinaryReader pooledBinaryReader2 = MemoryPools.poolBinaryReader.AllocSync(false))
							{
								pooledBinaryReader2.SetBaseStream(stream2);
								this.ReadSpecial(pooledBinaryReader2);
							}
						}
					}
				}
			}
		}

		public bool LoadMainViewerData()
		{
			string path = string.Format("{0}/{1}", GameIO.GetSaveGameRootDir(), "twitch_main.dat");
			if (SdFile.Exists(path))
			{
				try
				{
					using (Stream stream = SdFile.OpenRead(path))
					{
						using (PooledBinaryReader pooledBinaryReader = MemoryPools.poolBinaryReader.AllocSync(false))
						{
							pooledBinaryReader.SetBaseStream(stream);
							this.ReadMain(pooledBinaryReader);
						}
					}
				}
				catch (Exception)
				{
					path = string.Format("{0}/{1}", GameIO.GetSaveGameRootDir(), "twitch_main.dat.bak");
					if (SdFile.Exists(path))
					{
						using (Stream stream2 = SdFile.OpenRead(path))
						{
							using (PooledBinaryReader pooledBinaryReader2 = MemoryPools.poolBinaryReader.AllocSync(false))
							{
								pooledBinaryReader2.SetBaseStream(stream2);
								this.ReadMain(pooledBinaryReader2);
							}
						}
					}
				}
				return true;
			}
			return false;
		}

		public bool LoadLatestMainViewerData()
		{
			for (int i = (int)TwitchManager.MainFileVersion; i >= 2; i--)
			{
				if (this.LoadLatestMainViewerData(i))
				{
					return true;
				}
			}
			return false;
		}

		public bool LoadLatestMainViewerData(int version)
		{
			string text = GameIO.GetUserGameDataDir() + "/Twitch/" + version.ToString();
			if (!SdDirectory.Exists(text))
			{
				SdDirectory.CreateDirectory(text);
			}
			string path = string.Format("{0}/{1}", text, "twitch_main.dat");
			if (SdFile.Exists(path))
			{
				try
				{
					using (Stream stream = SdFile.OpenRead(path))
					{
						using (PooledBinaryReader pooledBinaryReader = MemoryPools.poolBinaryReader.AllocSync(false))
						{
							pooledBinaryReader.SetBaseStream(stream);
							this.ReadMain(pooledBinaryReader);
						}
					}
				}
				catch (Exception)
				{
					path = string.Format("{0}/{1}", text, "twitch_main.dat.bak");
					if (SdFile.Exists(path))
					{
						using (Stream stream2 = SdFile.OpenRead(path))
						{
							using (PooledBinaryReader pooledBinaryReader2 = MemoryPools.poolBinaryReader.AllocSync(false))
							{
								pooledBinaryReader2.SetBaseStream(stream2);
								this.ReadMain(pooledBinaryReader2);
							}
						}
					}
				}
				return true;
			}
			return false;
		}

		public void SaveViewerData()
		{
			if (this.dataSaveThreadInfo == null || !ThreadManager.ActiveThreads.ContainsKey("viewerDataSave"))
			{
				PooledExpandableMemoryStream pooledExpandableMemoryStream = MemoryPools.poolMemoryStream.AllocSync(true);
				using (PooledBinaryWriter pooledBinaryWriter = MemoryPools.poolBinaryWriter.AllocSync(false))
				{
					pooledBinaryWriter.SetBaseStream(pooledExpandableMemoryStream);
					this.Write(pooledBinaryWriter);
				}
				PooledExpandableMemoryStream pooledExpandableMemoryStream2 = MemoryPools.poolMemoryStream.AllocSync(true);
				using (PooledBinaryWriter pooledBinaryWriter2 = MemoryPools.poolBinaryWriter.AllocSync(false))
				{
					pooledBinaryWriter2.SetBaseStream(pooledExpandableMemoryStream2);
					this.WriteMain(pooledBinaryWriter2);
				}
				this.dataSaveThreadInfo = ThreadManager.StartThread("viewerDataSave", null, new ThreadManager.ThreadFunctionLoopDelegate(this.saveViewerDataThreaded), null, System.Threading.ThreadPriority.Normal, new PooledExpandableMemoryStream[]
				{
					pooledExpandableMemoryStream,
					pooledExpandableMemoryStream2
				}, null, false, true);
			}
		}

		public void Write(BinaryWriter bw)
		{
			bw.Write(TwitchManager.FileVersion);
			bw.Write(this.UseProgression);
			this.ViewerData.Write(bw);
			bw.Write(this.Leaderboard.Count);
			for (int i = 0; i < this.Leaderboard.Count; i++)
			{
				TwitchLeaderboardEntry twitchLeaderboardEntry = this.Leaderboard[i];
				bw.Write(twitchLeaderboardEntry.UserName);
				bw.Write(twitchLeaderboardEntry.Kills);
				bw.Write(twitchLeaderboardEntry.UserColor);
			}
			bw.Write(this.UseActionsDuringBloodmoon);
			bw.Write(this.RewardPot);
			bw.Write(this.ViewerData.PointRate);
			bw.Write(this.CooldownPresetIndex);
			bw.Write((byte)this.PimpPotType);
			bw.Write(this.AllowCrateSharing);
			bw.Write(this.BitPointModifier);
			bw.Write(this.RaidPointAdd);
			bw.Write(this.RaidViewerMinimum);
			bw.Write(this.SubPointModifier);
			bw.Write(this.GiftSubPointModifier);
			bw.Write((byte)this.VotingManager.MaxDailyVotes);
			bw.Write(this.VotingManager.VoteTime);
			bw.Write((byte)this.VotingManager.CurrentVoteDayTimeRange);
			bw.Write(this.VotingManager.ViewerDefeatReward);
			bw.Write(this.VotingManager.AllowVotesDuringBloodmoon);
			bw.Write(this.ViewerData.ActionSpamDelay);
			bw.Write(this.ViewerData.StartingPoints);
			bw.Write(this.UseActionsDuringQuests);
			bw.Write(this.VotingManager.AllowVotesDuringQuests);
			bw.Write(this.VotingManager.AllowVotesInSafeZone);
			bw.Write(this.changedEnabledVoteList.Count);
			for (int j = 0; j < this.changedEnabledVoteList.Count; j++)
			{
				bw.Write(this.changedEnabledVoteList[j]);
				bw.Write(TwitchActionManager.TwitchVotes[this.changedEnabledVoteList[j]].Enabled);
			}
			bw.Write((byte)this.integrationSetting);
			bw.Write(this.EventPresetIndex);
			bw.Write(this.ActionPresetIndex);
			bw.Write(this.VotePresetIndex);
			bw.Write(this.AllowBitEvents);
			bw.Write(this.AllowSubEvents);
			bw.Write(this.AllowGiftSubEvents);
			bw.Write(this.AllowCharityEvents);
			bw.Write(this.AllowRaidEvents);
			bw.Write(this.AllowHypeTrainEvents);
			bw.Write(this.AllowChannelPointRedemptions);
			bw.Write(TwitchManager.LeaderboardStats.GoodRewardTime);
			bw.Write(TwitchManager.LeaderboardStats.GoodRewardAmount);
			int num = 0;
			for (int k = 0; k < this.ActionPresets.Count; k++)
			{
				TwitchActionPreset twitchActionPreset = this.ActionPresets[k];
				if (twitchActionPreset.AddedActions.Count > 0 || twitchActionPreset.RemovedActions.Count > 0)
				{
					num++;
				}
			}
			bw.Write(num);
			for (int l = 0; l < this.ActionPresets.Count; l++)
			{
				TwitchActionPreset twitchActionPreset2 = this.ActionPresets[l];
				if (twitchActionPreset2.AddedActions.Count > 0 || twitchActionPreset2.RemovedActions.Count > 0)
				{
					bw.Write(twitchActionPreset2.Name);
					bw.Write(twitchActionPreset2.AddedActions.Count);
					for (int m = 0; m < twitchActionPreset2.AddedActions.Count; m++)
					{
						bw.Write(twitchActionPreset2.AddedActions[m]);
					}
					bw.Write(twitchActionPreset2.RemovedActions.Count);
					for (int n = 0; n < twitchActionPreset2.RemovedActions.Count; n++)
					{
						bw.Write(twitchActionPreset2.RemovedActions[n]);
					}
				}
			}
			bw.Write(this.bitPriceMultiplier);
			bw.Write(this.AllowCreatorGoalEvents);
			bw.Write(this.changedActionList.Count);
			for (int num2 = 0; num2 < this.changedActionList.Count; num2++)
			{
				bw.Write(this.changedActionList[num2]);
				bw.Write(TwitchActionManager.TwitchActions[this.changedActionList[num2]].ModifiedCost);
			}
			bw.Write(this.BitPot);
			bw.Write(this.BitPotPercentage);
		}

		public void HandleChangedPropertyList()
		{
			this.changedActionList.Clear();
			this.changedEnabledVoteList.Clear();
			foreach (string text in TwitchActionManager.TwitchActions.Keys)
			{
				TwitchAction twitchAction = TwitchActionManager.TwitchActions[text];
				if (twitchAction.DefaultCost != twitchAction.ModifiedCost)
				{
					this.changedActionList.Add(text);
				}
			}
			foreach (string text2 in TwitchActionManager.TwitchVotes.Keys)
			{
				TwitchVote twitchVote = TwitchActionManager.TwitchVotes[text2];
				if (twitchVote.Enabled != twitchVote.OriginalEnabled)
				{
					this.changedEnabledVoteList.Add(text2);
				}
			}
		}

		public void WriteSpecial(BinaryWriter bw)
		{
			this.ViewerData.WriteSpecial(bw);
		}

		public void WriteMain(BinaryWriter bw)
		{
			bw.Write(TwitchManager.MainFileVersion);
			bw.Write(this.HasViewedSettings);
			this.ViewerData.WriteSpecial(bw);
		}

		public void Read(BinaryReader br)
		{
			this.CurrentFileVersion = br.ReadByte();
			if (this.CurrentFileVersion > 1)
			{
				this.UseProgression = br.ReadBoolean();
			}
			this.ViewerData.Read(br, this.CurrentFileVersion);
			if (this.CurrentFileVersion > 3)
			{
				int num = br.ReadInt32();
				this.Leaderboard.Clear();
				for (int i = 0; i < num; i++)
				{
					string username;
					int kills;
					string usercolor;
					if (this.CurrentFileVersion > 10)
					{
						username = br.ReadString();
						kills = br.ReadInt32();
						usercolor = br.ReadString();
					}
					else
					{
						username = br.ReadString();
						usercolor = br.ReadString();
						kills = br.ReadInt32();
					}
					this.Leaderboard.Add(new TwitchLeaderboardEntry(username, usercolor, kills));
				}
			}
			if (this.CurrentFileVersion > 4)
			{
				this.UseActionsDuringBloodmoon = br.ReadInt32();
			}
			if (this.CurrentFileVersion > 5)
			{
				this.RewardPot = br.ReadInt32();
				if (this.RewardPot <= 0)
				{
					this.RewardPot = 0;
				}
				if (this.RewardPot > TwitchManager.LeaderboardStats.LargestPimpPot)
				{
					TwitchManager.LeaderboardStats.LargestPimpPot = this.RewardPot;
				}
			}
			if (this.CurrentFileVersion > 6)
			{
				this.ViewerData.PointRate = br.ReadSingle();
				this.CooldownPresetIndex = br.ReadInt32();
				if (this.CurrentFileVersion <= 18)
				{
					this.ActionCooldownModifier = br.ReadSingle();
				}
				this.PimpPotType = (TwitchManager.PimpPotSettings)br.ReadByte();
				this.AllowCrateSharing = br.ReadBoolean();
			}
			if (this.CurrentFileVersion > 7)
			{
				if (this.CurrentFileVersion <= 17)
				{
					br.ReadBoolean();
					br.ReadBoolean();
					br.ReadBoolean();
				}
				this.BitPointModifier = br.ReadInt32();
				this.RaidPointAdd = br.ReadInt32();
				this.RaidViewerMinimum = br.ReadInt32();
				this.SubPointModifier = br.ReadInt32();
				this.GiftSubPointModifier = br.ReadInt32();
				if (this.CurrentFileVersion <= 20)
				{
					int num2 = br.ReadInt32();
					this.changedActionList.Clear();
					for (int j = 0; j < num2; j++)
					{
						string text = br.ReadString();
						bool enabled = br.ReadBoolean();
						if (TwitchActionManager.TwitchActions.ContainsKey(text))
						{
							this.changedActionList.Add(text);
							TwitchActionManager.TwitchActions[text].Enabled = enabled;
						}
					}
				}
			}
			if (this.CurrentFileVersion > 8)
			{
				this.VotingManager.MaxDailyVotes = (int)br.ReadByte();
				this.VotingManager.VoteTime = br.ReadSingle();
				this.VotingManager.CurrentVoteDayTimeRange = (int)br.ReadByte();
				this.VotingManager.ViewerDefeatReward = br.ReadInt32();
				this.VotingManager.AllowVotesDuringBloodmoon = br.ReadBoolean();
			}
			if (this.CurrentFileVersion > 9)
			{
				this.ViewerData.ActionSpamDelay = br.ReadSingle();
			}
			if (this.CurrentFileVersion > 10)
			{
				this.ViewerData.StartingPoints = br.ReadInt32();
			}
			if (this.CurrentFileVersion > 11)
			{
				this.UseActionsDuringQuests = br.ReadInt32();
				this.VotingManager.AllowVotesDuringQuests = br.ReadBoolean();
			}
			if (this.CurrentFileVersion > 12)
			{
				this.VotingManager.AllowVotesInSafeZone = br.ReadBoolean();
				if (this.CurrentFileVersion <= 17)
				{
					br.ReadByte();
				}
			}
			if (this.CurrentFileVersion > 13)
			{
				int num3 = br.ReadInt32();
				this.changedEnabledVoteList.Clear();
				for (int k = 0; k < num3; k++)
				{
					string text2 = br.ReadString();
					this.changedEnabledVoteList.Add(text2);
					TwitchActionManager.TwitchVotes[text2].Enabled = br.ReadBoolean();
				}
			}
			if (this.CurrentFileVersion >= 16)
			{
				byte b = br.ReadByte();
				if (b > 1)
				{
					b = 1;
				}
				this.IntegrationSetting = (TwitchManager.IntegrationSettings)b;
			}
			if (this.CurrentFileVersion >= 17)
			{
				this.EventPresetIndex = br.ReadInt32();
				this.CurrentEventPreset = this.EventPresets[this.EventPresetIndex];
			}
			if (this.CurrentFileVersion >= 18)
			{
				this.ActionPresetIndex = br.ReadInt32();
				this.CurrentActionPreset = this.ActionPresets[this.ActionPresetIndex];
				this.VotePresetIndex = br.ReadInt32();
				this.CurrentVotePreset = this.VotePresets[this.VotePresetIndex];
				this.AllowBitEvents = br.ReadBoolean();
				this.AllowSubEvents = br.ReadBoolean();
				this.AllowGiftSubEvents = br.ReadBoolean();
				this.AllowCharityEvents = br.ReadBoolean();
				this.AllowRaidEvents = br.ReadBoolean();
				this.AllowHypeTrainEvents = br.ReadBoolean();
				this.AllowChannelPointRedemptions = br.ReadBoolean();
			}
			if (this.CurrentFileVersion >= 20)
			{
				TwitchManager.LeaderboardStats.GoodRewardTime = br.ReadInt32();
				TwitchManager.LeaderboardStats.GoodRewardAmount = br.ReadInt32();
			}
			if (this.CurrentFileVersion >= 21)
			{
				int num4 = br.ReadInt32();
				for (int l = 0; l < num4; l++)
				{
					string b2 = br.ReadString();
					TwitchActionPreset twitchActionPreset = null;
					for (int m = 0; m < this.ActionPresets.Count; m++)
					{
						if (this.ActionPresets[m].Name == b2)
						{
							twitchActionPreset = this.ActionPresets[m];
							break;
						}
					}
					int num5 = br.ReadInt32();
					if (twitchActionPreset != null)
					{
						twitchActionPreset.AddedActions.Clear();
						twitchActionPreset.RemovedActions.Clear();
					}
					for (int n = 0; n < num5; n++)
					{
						if (twitchActionPreset != null)
						{
							twitchActionPreset.AddedActions.Add(br.ReadString());
						}
					}
					num5 = br.ReadInt32();
					for (int num6 = 0; num6 < num5; num6++)
					{
						if (twitchActionPreset != null)
						{
							twitchActionPreset.RemovedActions.Add(br.ReadString());
						}
					}
				}
			}
			if (this.CurrentFileVersion >= 22)
			{
				this.BitPriceMultiplier = br.ReadSingle();
			}
			if (this.CurrentFileVersion >= 23)
			{
				this.AllowCreatorGoalEvents = br.ReadBoolean();
			}
			if (this.CurrentFileVersion >= 24)
			{
				int num7 = br.ReadInt32();
				this.changedActionList.Clear();
				for (int num8 = 0; num8 < num7; num8++)
				{
					string text3 = br.ReadString();
					int modifiedCost = br.ReadInt32();
					if (TwitchActionManager.TwitchActions.ContainsKey(text3))
					{
						this.changedActionList.Add(text3);
						TwitchActionManager.TwitchActions[text3].ModifiedCost = modifiedCost;
					}
				}
			}
			if (this.CurrentFileVersion >= 25)
			{
				this.BitPot = br.ReadInt32();
				if (this.BitPot <= 0)
				{
					this.BitPot = 0;
				}
				if (this.BitPot > TwitchManager.LeaderboardStats.LargestBitPot)
				{
					TwitchManager.LeaderboardStats.LargestBitPot = this.BitPot;
				}
			}
			if (this.CurrentFileVersion >= 26)
			{
				this.BitPotPercentage = br.ReadSingle();
			}
		}

		public void ReadSpecial(BinaryReader br)
		{
			this.ViewerData.ReadSpecial(br, 1);
		}

		public void ReadMain(BinaryReader br)
		{
			this.CurrentMainFileVersion = br.ReadByte();
			this.HasViewedSettings = br.ReadBoolean();
			this.ViewerData.ReadSpecial(br, this.CurrentMainFileVersion);
		}

		public void SendChannelPointOutputMessage(string name, ViewerEntry entry)
		{
			if (entry.SpecialPoints == 0f)
			{
				this.ircClient.SendChannelMessage(string.Format(this.chatOutput_PointsWithoutSpecial, name, entry.CombinedPoints), true);
				return;
			}
			this.ircClient.SendChannelMessage(string.Format(this.chatOutput_PointsWithSpecial, name, entry.CombinedPoints, entry.SpecialPoints), true);
		}

		public void SendChannelCreditOutputMessage(string name, ViewerEntry entry)
		{
			this.ircClient.SendChannelMessage(string.Format(this.chatOutput_BitCredits, name, entry.BitCredits), true);
		}

		public void SendChannelPointOutputMessage(string name)
		{
			this.SendChannelPointOutputMessage(name, this.ViewerData.GetViewerEntry(name));
		}

		public void SendChannelCreditOutputMessage(string name)
		{
			this.SendChannelCreditOutputMessage(name, this.ViewerData.GetViewerEntry(name));
		}

		public void SendChannelMessage(string message, bool useQueue = true)
		{
			this.ircClient.SendChannelMessage(message, useQueue);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void UpdateActionCooldowns(float modifier)
		{
			foreach (TwitchAction twitchAction in TwitchActionManager.TwitchActions.Values)
			{
				if (twitchAction.IsInPreset(this.CurrentActionPreset))
				{
					twitchAction.UpdateModifiedCooldown(modifier);
				}
			}
		}

		public void SetupAvailableCommands()
		{
			this.AvailableCommands.Clear();
			this.AlternateCommands.Clear();
			TwitchAction[] array = (from a in TwitchActionManager.TwitchActions.Values
			where a.CanUse && a.IsInPreset(this.CurrentActionPreset)
			orderby a.Command
			orderby a.PointType
			select a).ToArray<TwitchAction>();
			List<string> list = null;
			for (int i = 0; i < array.Count<TwitchAction>(); i++)
			{
				TwitchAction twitchAction = array[i];
				if (this.UseProgression && !this.OverrideProgession)
				{
					int startGameStage = twitchAction.StartGameStage;
					if (startGameStage == -1 || startGameStage <= this.HighestGameStage)
					{
						if (twitchAction.Replaces != "")
						{
							string item = twitchAction.Replaces;
							if (this.AlternateCommands.ContainsKey(twitchAction.Replaces))
							{
								item = this.AlternateCommands[twitchAction.Replaces];
							}
							if (list == null)
							{
								list = new List<string>();
							}
							list.Add(item);
						}
						if (this.AvailableCommands.ContainsKey(twitchAction.Command))
						{
							this.AvailableCommands[twitchAction.Command] = twitchAction;
						}
						else
						{
							this.AvailableCommands.Add(twitchAction.Command, twitchAction);
						}
						if (!this.AlternateCommands.ContainsKey(twitchAction.BaseCommand))
						{
							this.AlternateCommands.Add(twitchAction.BaseCommand, twitchAction.Command);
						}
					}
				}
				else
				{
					if (twitchAction.RandomDaily)
					{
						twitchAction.AllowedDay = this.lastGameDay;
					}
					if (twitchAction.Replaces != "")
					{
						string item2 = twitchAction.Replaces;
						if (this.AlternateCommands.ContainsKey(twitchAction.Replaces))
						{
							item2 = this.AlternateCommands[twitchAction.Replaces];
						}
						if (list == null)
						{
							list = new List<string>();
						}
						list.Add(item2);
					}
					if (this.AvailableCommands.ContainsKey(twitchAction.Command))
					{
						this.AvailableCommands[twitchAction.Command] = twitchAction;
					}
					else
					{
						this.AvailableCommands.Add(twitchAction.Command, twitchAction);
					}
					if (!this.AlternateCommands.ContainsKey(twitchAction.BaseCommand))
					{
						this.AlternateCommands.Add(twitchAction.BaseCommand, twitchAction.Command);
					}
				}
			}
			if (list != null)
			{
				for (int j = 0; j < list.Count; j++)
				{
					string key = list[j];
					if (this.AvailableCommands.ContainsKey(key))
					{
						this.AvailableCommands.Remove(key);
					}
				}
			}
		}

		public void SetupAvailableCommandsWithOutput(int lastGameStage, bool displayMessage)
		{
			this.AvailableCommands.Clear();
			this.AlternateCommands.Clear();
			StringBuilder stringBuilder = null;
			TwitchAction[] array = (from a in TwitchActionManager.TwitchActions.Values
			where a.CanUse && a.IsInPreset(this.CurrentActionPreset)
			orderby a.Command
			orderby a.PointType
			select a).ToArray<TwitchAction>();
			List<string> list = null;
			for (int i = 0; i < array.Count<TwitchAction>(); i++)
			{
				TwitchAction twitchAction = array[i];
				if (this.UseProgression && !this.OverrideProgession)
				{
					int startGameStage = twitchAction.StartGameStage;
					if (startGameStage == -1 || startGameStage <= this.HighestGameStage)
					{
						if (twitchAction.Replaces != "")
						{
							string item = twitchAction.Replaces;
							if (this.AlternateCommands.ContainsKey(twitchAction.Replaces))
							{
								item = this.AlternateCommands[twitchAction.Replaces];
							}
							if (list == null)
							{
								list = new List<string>();
							}
							list.Add(item);
						}
						if (this.AvailableCommands.ContainsKey(twitchAction.Command))
						{
							this.AvailableCommands[twitchAction.Command] = twitchAction;
							if (startGameStage > lastGameStage)
							{
								if (stringBuilder == null)
								{
									stringBuilder = new StringBuilder();
									stringBuilder.Append("*" + twitchAction.Command);
								}
								else
								{
									stringBuilder.Append(", " + twitchAction.Command);
								}
							}
						}
						else
						{
							this.AvailableCommands.Add(twitchAction.Command, twitchAction);
							if (startGameStage > lastGameStage)
							{
								if (stringBuilder == null)
								{
									stringBuilder = new StringBuilder();
									stringBuilder.Append(twitchAction.Command);
								}
								else
								{
									stringBuilder.Append(", " + twitchAction.Command);
								}
							}
						}
						if (!this.AlternateCommands.ContainsKey(twitchAction.BaseCommand))
						{
							this.AlternateCommands.Add(twitchAction.BaseCommand, twitchAction.Command);
						}
					}
				}
				else
				{
					if (twitchAction.RandomDaily)
					{
						twitchAction.AllowedDay = this.lastGameDay;
					}
					if (this.AvailableCommands.ContainsKey(twitchAction.Command))
					{
						this.AvailableCommands[twitchAction.Command] = twitchAction;
					}
					else
					{
						this.AvailableCommands.Add(twitchAction.Command, twitchAction);
					}
					if (!this.AlternateCommands.ContainsKey(twitchAction.BaseCommand))
					{
						this.AlternateCommands.Add(twitchAction.BaseCommand, twitchAction.Command);
					}
				}
			}
			if (list != null)
			{
				for (int j = 0; j < list.Count; j++)
				{
					string key = list[j];
					if (this.AvailableCommands.ContainsKey(key))
					{
						this.AvailableCommands.Remove(key);
					}
				}
			}
			if (displayMessage && stringBuilder != null && this.AllowActions)
			{
				this.ircClient.SendChannelMessage(string.Format(this.chatOutput_NewActions, stringBuilder), true);
				Manager.BroadcastPlayByLocalPlayer(this.LocalPlayer.position, "twitch_new_commands");
			}
		}

		public void HandleCooldownActionLocking()
		{
			foreach (string key in this.AvailableCommands.Keys)
			{
				TwitchAction twitchAction = this.AvailableCommands[key];
				if (twitchAction.IsInPreset(this.CurrentActionPreset))
				{
					if (this.OnCooldown)
					{
						if (this.CooldownType == TwitchManager.CooldownTypes.BloodMoonDisabled || this.CooldownType == TwitchManager.CooldownTypes.Time)
						{
							twitchAction.OnCooldown = true;
						}
						else if (this.CooldownType == TwitchManager.CooldownTypes.MaxReachedWaiting || this.CooldownType == TwitchManager.CooldownTypes.SafeCooldown)
						{
							twitchAction.OnCooldown = twitchAction.WaitingBlocked;
						}
						else
						{
							twitchAction.OnCooldown = twitchAction.CooldownBlocked;
						}
					}
					else
					{
						twitchAction.OnCooldown = false;
					}
				}
			}
			if (this.CommandsChanged != null)
			{
				this.CommandsChanged();
			}
		}

		public void PushBalanceToExtensionQueue(string userID, int creditBalance)
		{
			if (this.extensionManager != null)
			{
				this.extensionManager.PushUserBalance(new ValueTuple<string, int>(userID, creditBalance));
			}
		}

		public void DisplayActions()
		{
			int count = this.ActionMessages.Count;
			if (this.CurrentUnityTime > this.nextDisplayCommandsTime)
			{
				this.nextDisplayCommandsTime = this.CurrentUnityTime + 15f;
				this.ircClient.SendChannelMessages(this.ActionMessages, true);
			}
		}

		public void DisplayCommands(bool isBroadcaster, bool isMod, bool isVIP, bool isSub)
		{
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < this.TwitchCommandList.Count; i++)
			{
				if (!(this.TwitchCommandList[i] is TwitchCommandCommands))
				{
					bool flag = false;
					switch (BaseTwitchCommand.GetPermission(this.TwitchCommandList[i]))
					{
					case BaseTwitchCommand.PermissionLevels.Everyone:
						flag = true;
						break;
					case BaseTwitchCommand.PermissionLevels.VIP:
						flag = isVIP;
						break;
					case BaseTwitchCommand.PermissionLevels.Sub:
						flag = isSub;
						break;
					case BaseTwitchCommand.PermissionLevels.Mod:
						flag = isMod;
						break;
					case BaseTwitchCommand.PermissionLevels.Broadcaster:
						flag = isBroadcaster;
						break;
					}
					if (flag)
					{
						for (int j = 0; j < this.TwitchCommandList[i].LocalizedCommandNames.Length; j++)
						{
							if (stringBuilder.Length != 0)
							{
								stringBuilder.Append(", ");
							}
							stringBuilder.Append(this.TwitchCommandList[i].LocalizedCommandNames[j]);
						}
					}
				}
			}
			this.ircClient.SendChannelMessage(string.Format(this.chatOutput_Commands, stringBuilder.ToString()), true);
		}

		public void AddTip(string tipname)
		{
			string text = Localization.Get(tipname, false);
			string item = Localization.Get(tipname + "Desc", false);
			if (text != "")
			{
				this.tipTitleList.Add(text);
				this.tipDescriptionList.Add(item);
			}
		}

		public void DisplayGameStage()
		{
			if (this.LocalPlayer != null)
			{
				this.ircClient.SendChannelMessage(string.Format(this.chatOutput_Gamestage, this.LocalPlayer.unModifiedGameStage), true);
			}
		}

		public bool CheckIfTwitchKill(EntityPlayer player)
		{
			return this.twitchPlayerDeathsThisFrame.Contains(player);
		}

		public bool LiveListContains(int entityID)
		{
			for (int i = 0; i < this.liveList.Count; i++)
			{
				if (this.liveList[i].SpawnedEntityID == entityID)
				{
					return true;
				}
			}
			return false;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static TwitchManager instance = null;

		[PublicizedFrom(EAccessModifier.Private)]
		public const float SAVE_TIME_SEC = 30f;

		public float saveTime;

		[PublicizedFrom(EAccessModifier.Private)]
		public ThreadManager.ThreadInfo dataSaveThreadInfo;

		public static byte FileVersion = 26;

		public static byte MainFileVersion = 3;

		public TwitchIRCClient ircClient;

		public ExtensionManager extensionManager;

		[PublicizedFrom(EAccessModifier.Private)]
		public int resetClientAttempts;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool overrideProgression;

		[PublicizedFrom(EAccessModifier.Private)]
		public int commandsAvailable = -1;

		public Dictionary<string, TwitchAction> AvailableCommands = new Dictionary<string, TwitchAction>();

		public Dictionary<string, string> AlternateCommands = new Dictionary<string, string>();

		public TwitchManager.PimpPotSettings PimpPotType = TwitchManager.PimpPotSettings.EnabledSP;

		public static int PimpPotDefault = 500;

		[PublicizedFrom(EAccessModifier.Private)]
		public TwitchManager.IntegrationSettings integrationSetting = TwitchManager.IntegrationSettings.Both;

		[PublicizedFrom(EAccessModifier.Private)]
		public float actionCooldownModifier = 1f;

		[PublicizedFrom(EAccessModifier.Private)]
		public const int HistoryItemMax = 500;

		public float ActionPotPercentage = 0.15f;

		public float BitPotPercentage = 0.25f;

		public int RewardPot = TwitchManager.PimpPotDefault;

		public int BitPot;

		public int PartyKillRewardMax = 250;

		public EntityPlayerLocal LocalPlayer;

		public bool LocalPlayerInLandClaim;

		public TwitchManager.CooldownTypes CooldownType = TwitchManager.CooldownTypes.Startup;

		public float CooldownTime = 300f;

		public float CurrentCooldownFill;

		public float CooldownFillMax = 50f;

		public int NextCooldownTime = 180;

		public bool AllowCrateSharing;

		public bool AllowBitEvents = true;

		public bool AllowSubEvents = true;

		public bool AllowGiftSubEvents = true;

		public bool AllowCharityEvents = true;

		public bool AllowRaidEvents = true;

		public bool AllowHypeTrainEvents = true;

		public bool AllowCreatorGoalEvents = true;

		public bool AllowChannelPointRedemptions = true;

		public List<CooldownPreset> CooldownPresets = new List<CooldownPreset>();

		public int CooldownPresetIndex;

		public CooldownPreset CurrentCooldownPreset;

		public List<TwitchActionPreset> ActionPresets = new List<TwitchActionPreset>();

		public List<TwitchVotePreset> VotePresets = new List<TwitchVotePreset>();

		public List<TwitchEventPreset> EventPresets = new List<TwitchEventPreset>();

		public int ActionPresetIndex;

		public int VotePresetIndex;

		public int EventPresetIndex;

		public TwitchActionPreset CurrentActionPreset;

		public TwitchVotePreset CurrentVotePreset;

		public TwitchEventPreset CurrentEventPreset;

		public bool UIDirty;

		[PublicizedFrom(EAccessModifier.Private)]
		public float updateTime = 1f;

		public float ExtensionCheckTime;

		[PublicizedFrom(EAccessModifier.Private)]
		public World world;

		public int lastGameDay = -1;

		public int currentBMDayEnd = -1;

		public int nextBMDay = -1;

		public int BMCooldownStart;

		public int BMCooldownEnd;

		public int BitPointModifier = 1;

		public int SubPointModifier = 1;

		public int GiftSubPointModifier = 2;

		public int RaidPointAdd = 1000;

		public int RaidViewerMinimum = 10;

		public int HypeTrainLevel;

		[PublicizedFrom(EAccessModifier.Private)]
		public float bitPriceMultiplier = 1f;

		public static TwitchLeaderboardStats LeaderboardStats = new TwitchLeaderboardStats();

		public bool isBMActive;

		public TwitchVoteLockTypes VoteLockedLevel;

		public List<TwitchActionHistoryEntry> ActionHistory = new List<TwitchActionHistoryEntry>();

		public List<TwitchActionHistoryEntry> VoteHistory = new List<TwitchActionHistoryEntry>();

		public List<TwitchActionHistoryEntry> EventHistory = new List<TwitchActionHistoryEntry>();

		public List<TwitchLeaderboardEntry> Leaderboard = new List<TwitchLeaderboardEntry>();

		public List<TwitchRespawnEntry> RespawnEntries = new List<TwitchRespawnEntry>();

		public int UseActionsDuringBloodmoon = 2;

		public int UseActionsDuringQuests = 2;

		public bool InitialCooldownSet;

		public List<EntityPlayer> twitchPlayerDeathsThisFrame = new List<EntityPlayer>();

		[PublicizedFrom(EAccessModifier.Private)]
		public TwitchManager.InitStates initState;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool isLoaded;

		public XUi LocalPlayerXUi;

		public float CurrentUnityTime;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool resetCommandsNeeded;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool respawnEventNeeded;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool checkingExtensionInstalled;

		[PublicizedFrom(EAccessModifier.Private)]
		public string broadcasterType = "";

		[PublicizedFrom(EAccessModifier.Private)]
		public List<TwitchMessageEntry> inGameChatQueue = new List<TwitchMessageEntry>();

		public TwitchAuthentication Authentication;

		public TwitchPubSub PubSub;

		public bool HasViewedSettings;

		public string DeniedCrateEvent = "";

		public string StealingCrateEvent = "";

		public string PartyRespawnEvent = "";

		public string OnPlayerDeathEvent = "";

		public string OnPlayerRespawnEvent = "";

		public List<string> tipTitleList = new List<string>();

		public List<string> tipDescriptionList = new List<string>();

		[PublicizedFrom(EAccessModifier.Private)]
		public int extensionActiveCheckFailures;

		[PublicizedFrom(EAccessModifier.Private)]
		public string chatOutput_ActivatedAction;

		[PublicizedFrom(EAccessModifier.Private)]
		public string chatOutput_ActivatedBitAction;

		[PublicizedFrom(EAccessModifier.Private)]
		public string chatOutput_BitCredits;

		[PublicizedFrom(EAccessModifier.Private)]
		public string chatOutput_BitEvent;

		[PublicizedFrom(EAccessModifier.Private)]
		public string chatOutput_BitPotBalance;

		[PublicizedFrom(EAccessModifier.Private)]
		public string chatOutput_ChannelPointEvent;

		[PublicizedFrom(EAccessModifier.Private)]
		public string chatOutput_CharityEvent;

		[PublicizedFrom(EAccessModifier.Private)]
		public string chatOutput_CooldownComplete;

		[PublicizedFrom(EAccessModifier.Private)]
		public string chatOutput_CooldownStarted;

		[PublicizedFrom(EAccessModifier.Private)]
		public string chatOutput_CooldownTime;

		[PublicizedFrom(EAccessModifier.Private)]
		public string chatOutput_Commands;

		[PublicizedFrom(EAccessModifier.Private)]
		public string chatOutput_CreatorGoalEvent;

		[PublicizedFrom(EAccessModifier.Private)]
		public string chatOutput_DonateBits;

		[PublicizedFrom(EAccessModifier.Private)]
		public string chatOutput_DonateCharity;

		[PublicizedFrom(EAccessModifier.Private)]
		public string chatOutput_Gamestage;

		[PublicizedFrom(EAccessModifier.Private)]
		public string chatOutput_GiftSubEvent;

		[PublicizedFrom(EAccessModifier.Private)]
		public string chatOutput_GiftSubs;

		[PublicizedFrom(EAccessModifier.Private)]
		public string chatOutput_HypeTrainEvent;

		[PublicizedFrom(EAccessModifier.Private)]
		public string chatOutput_KilledParty;

		[PublicizedFrom(EAccessModifier.Private)]
		public string chatOutput_KilledStreamer;

		[PublicizedFrom(EAccessModifier.Private)]
		public string chatOutput_KilledByBits;

		[PublicizedFrom(EAccessModifier.Private)]
		public string chatOutput_KilledByHypeTrain;

		[PublicizedFrom(EAccessModifier.Private)]
		public string chatOutput_KilledByVote;

		[PublicizedFrom(EAccessModifier.Private)]
		public string chatOutput_NewActions;

		[PublicizedFrom(EAccessModifier.Private)]
		public string chatOutput_PimpPotBalance;

		[PublicizedFrom(EAccessModifier.Private)]
		public string chatOutput_PointsWithSpecial;

		[PublicizedFrom(EAccessModifier.Private)]
		public string chatOutput_PointsWithoutSpecial;

		[PublicizedFrom(EAccessModifier.Private)]
		public string chatOutput_QueuedBitAction;

		[PublicizedFrom(EAccessModifier.Private)]
		public string chatOutput_RaidEvent;

		[PublicizedFrom(EAccessModifier.Private)]
		public string chatOutput_RaidPoints;

		[PublicizedFrom(EAccessModifier.Private)]
		public string chatOutput_SubEvent;

		[PublicizedFrom(EAccessModifier.Private)]
		public string chatOutput_Subscribed;

		[PublicizedFrom(EAccessModifier.Private)]
		public string ingameOutput_ActivatedAction;

		[PublicizedFrom(EAccessModifier.Private)]
		public string ingameOutput_BitRespawns;

		[PublicizedFrom(EAccessModifier.Private)]
		public string ingameOutput_DonateBits;

		[PublicizedFrom(EAccessModifier.Private)]
		public string ingameOutput_DonateCharity;

		[PublicizedFrom(EAccessModifier.Private)]
		public string ingameOutput_GiftSubs;

		[PublicizedFrom(EAccessModifier.Private)]
		public string ingameOutput_KilledParty;

		[PublicizedFrom(EAccessModifier.Private)]
		public string ingameOutput_KilledStreamer;

		[PublicizedFrom(EAccessModifier.Private)]
		public string ingameOutput_KilledByBits;

		[PublicizedFrom(EAccessModifier.Private)]
		public string ingameOutput_KilledByHypeTrain;

		[PublicizedFrom(EAccessModifier.Private)]
		public string ingameOutput_KilledByVote;

		[PublicizedFrom(EAccessModifier.Private)]
		public string ingameOutput_RaidPoints;

		[PublicizedFrom(EAccessModifier.Private)]
		public string ingameOutput_RefundedAction;

		[PublicizedFrom(EAccessModifier.Private)]
		public string ingameOutput_Subscribed;

		[PublicizedFrom(EAccessModifier.Private)]
		public string ingameDeathScreen_Message;

		[PublicizedFrom(EAccessModifier.Private)]
		public string ingameBitsDeathScreen_Message;

		[PublicizedFrom(EAccessModifier.Private)]
		public string ingameHypeTrainDeathScreen_Message;

		[PublicizedFrom(EAccessModifier.Private)]
		public string ingameVoteDeathScreen_Message;

		[PublicizedFrom(EAccessModifier.Private)]
		public string subPointDisplay;

		[PublicizedFrom(EAccessModifier.Private)]
		public Dictionary<string, TwitchRandomActionGroup> randomGroups = new Dictionary<string, TwitchRandomActionGroup>();

		[PublicizedFrom(EAccessModifier.Private)]
		public Dictionary<string, List<TwitchAction>> randomKeys = new Dictionary<string, List<TwitchAction>>();

		public List<BaseTwitchCommand> TwitchCommandList = new List<BaseTwitchCommand>();

		[PublicizedFrom(EAccessModifier.Protected)]
		public Dictionary<EntityPlayer, TwitchManager.TwitchPartyMemberInfo> PartyInfo = new Dictionary<EntityPlayer, TwitchManager.TwitchPartyMemberInfo>();

		[PublicizedFrom(EAccessModifier.Private)]
		public bool lastAlive;

		public static string DeathText = "";

		[PublicizedFrom(EAccessModifier.Private)]
		public bool twitchActive = true;

		[PublicizedFrom(EAccessModifier.Private)]
		public List<TwitchActionEntry> QueuedActionEntries = new List<TwitchActionEntry>();

		[PublicizedFrom(EAccessModifier.Private)]
		public List<TwitchActionEntry> LiveActionEntries = new List<TwitchActionEntry>();

		public List<TwitchEventActionEntry> EventQueue = new List<TwitchEventActionEntry>();

		public List<TwitchEventActionEntry> LiveEvents = new List<TwitchEventActionEntry>();

		[PublicizedFrom(EAccessModifier.Private)]
		public List<TwitchSpawnedEntityEntry> liveList = new List<TwitchSpawnedEntityEntry>();

		[PublicizedFrom(EAccessModifier.Private)]
		public List<TwitchSpawnedBlocksEntry> liveBlockList = new List<TwitchSpawnedBlocksEntry>();

		[PublicizedFrom(EAccessModifier.Private)]
		public List<TwitchRecentlyRemovedEntityEntry> recentlyDeadList = new List<TwitchRecentlyRemovedEntityEntry>();

		public List<TwitchSpawnedEntityEntry> actionSpawnLiveList = new List<TwitchSpawnedEntityEntry>();

		public bool HasDataChanges;

		public List<string> changedActionList = new List<string>();

		public List<string> changedEnabledVoteList = new List<string>();

		[PublicizedFrom(EAccessModifier.Private)]
		public List<string> ActionMessages = new List<string>();

		[PublicizedFrom(EAccessModifier.Private)]
		public float nextDisplayCommandsTime;

		public enum PimpPotSettings
		{
			Disabled,
			EnabledSP,
			EnabledPP
		}

		public enum IntegrationSettings
		{
			ExtensionOnly,
			Both
		}

		public enum CooldownTypes
		{
			None,
			Startup,
			Time,
			MaxReached,
			MaxReachedWaiting,
			BloodMoonDisabled,
			BloodMoonCooldown,
			QuestDisabled,
			QuestCooldown,
			SafeCooldown,
			SafeCooldownExit
		}

		public enum InitStates
		{
			Setup,
			None,
			WaitingForPermission,
			PermissionDenied,
			WaitingForOAuth,
			Authenticating,
			Authenticated,
			CheckingForExtension,
			Ready,
			ExtensionNotInstalled,
			Failed
		}

		public class TwitchPartyMemberInfo
		{
			public bool LastOptedOut;

			public bool LastAlive = true;

			public float Cooldown;

			public bool NeedsRespawnEvent;
		}
	}
}
