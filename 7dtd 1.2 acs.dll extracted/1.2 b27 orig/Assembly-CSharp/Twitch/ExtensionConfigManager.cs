using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UniLinq;
using UnityEngine;
using UnityEngine.Networking;

namespace Twitch
{
	public class ExtensionConfigManager
	{
		public void Init()
		{
			TwitchManager.Current.CommandsChanged += this.pushConfig;
			TwitchVotingManager votingManager = TwitchManager.Current.VotingManager;
			votingManager.VoteEventEnded = (OnGameEventVoteAction)Delegate.Combine(votingManager.VoteEventEnded, new OnGameEventVoteAction(this.pushConfig));
			TwitchVotingManager votingManager2 = TwitchManager.Current.VotingManager;
			votingManager2.VoteStarted = (OnGameEventVoteAction)Delegate.Combine(votingManager2.VoteStarted, new OnGameEventVoteAction(this.pushConfig));
			this.pushConfig();
		}

		public bool UpdatedConfig()
		{
			if (this.hasUpdated)
			{
				this.hasUpdated = false;
				return true;
			}
			return false;
		}

		public void Cleanup()
		{
			TwitchManager.Current.CommandsChanged -= this.pushConfig;
			TwitchVotingManager votingManager = TwitchManager.Current.VotingManager;
			votingManager.VoteEventEnded = (OnGameEventVoteAction)Delegate.Remove(votingManager.VoteEventEnded, new OnGameEventVoteAction(this.pushConfig));
			TwitchVotingManager votingManager2 = TwitchManager.Current.VotingManager;
			votingManager2.VoteStarted = (OnGameEventVoteAction)Delegate.Remove(votingManager2.VoteStarted, new OnGameEventVoteAction(this.pushConfig));
		}

		public void OnPartyChanged()
		{
			this.pushConfig();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void pushConfig()
		{
			this.lastPushTime = Time.time;
			if (!this.waitingToPush)
			{
				this.waitingToPush = true;
				GameManager.Instance.StartCoroutine(this.pushConfigAfterTimeout());
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public IEnumerator pushConfigAfterTimeout()
		{
			while (Time.time - this.lastPushTime < 1f)
			{
				yield return null;
			}
			this.waitingToPush = false;
			yield return this.UpdateConfig();
			yield break;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public IEnumerator UpdateConfig()
		{
			if (this.displayName == string.Empty)
			{
				yield return this.GetDisplayName();
			}
			if (TwitchManager.Current == null || TwitchManager.Current.Authentication == null)
			{
				Log.Warning("attempted to updated config with no Auth object");
				yield break;
			}
			Dictionary<string, List<ExtensionConfigManager.CommandModel>> commands = this.GetCommands();
			List<string> activeCategories = this.GetActiveCategories(commands.Keys.ToList<string>());
			string bodyData = JsonConvert.SerializeObject(new ExtensionConfigManager.ConfigModel
			{
				displayName = TwitchManager.Current.Authentication.userName,
				party = this.GetPlayers(),
				categories = activeCategories,
				commands = commands
			});
			using (UnityWebRequest req = UnityWebRequest.Put("https://2v3d0ewjcg.execute-api.us-east-1.amazonaws.com/prod/broadcaster/config", bodyData))
			{
				req.SetRequestHeader("Authorization", TwitchManager.Current.Authentication.userID + " " + TwitchManager.Current.Authentication.oauth.Substring(6));
				req.SetRequestHeader("Content-Type", "application/json");
				yield return req.SendWebRequest();
				if (req.result != UnityWebRequest.Result.Success)
				{
					Log.Warning(string.Format("Could not update config on backend: {0}", req.result));
				}
				else
				{
					Log.Out("Successfully updated broadcaster config");
					this.hasUpdated = true;
				}
			}
			UnityWebRequest req = null;
			yield break;
			yield break;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public IEnumerator GetDisplayName()
		{
			using (UnityWebRequest req = UnityWebRequest.Get("https://api.twitch.tv/helix/users"))
			{
				req.SetRequestHeader("Content-Type", "application/json");
				req.SetRequestHeader("Client-Id", TwitchAuthentication.client_id);
				req.SetRequestHeader("Authorization", "Bearer " + TwitchManager.Current.Authentication.oauth.Substring(6));
				yield return req.SendWebRequest();
				if (req.result != UnityWebRequest.Result.Success)
				{
					Log.Warning(string.Format("Could not get user data from Twitch: {0}", req.result));
				}
				else
				{
					Log.Out("Successfully retrieved user data from Twitch");
					JObject jobject = JObject.Parse(req.downloadHandler.text);
					this.displayName = jobject["data"][0]["display_name"].ToString();
					this.broadcasterType = jobject["data"][0]["broadcaster_type"].ToString();
				}
			}
			UnityWebRequest req = null;
			yield break;
			yield break;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public List<string> GetActiveCategories(List<string> categories)
		{
			return (from c in TwitchActionManager.Current.CategoryList
			select c.Name into name
			where categories.Contains(name)
			select name).ToList<string>();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public List<string> GetPlayers()
		{
			if (TwitchManager.Current.LocalPlayer != null && TwitchManager.Current.LocalPlayer.Party != null && TwitchManager.Current.LocalPlayer.Party.MemberList != null)
			{
				return (from m in TwitchManager.Current.LocalPlayer.Party.MemberList
				where !(m is EntityPlayerLocal) && !m.TwitchEnabled
				select m into e
				select e.EntityName).ToList<string>();
			}
			return ExtensionConfigManager.emptyParty;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public Dictionary<string, List<ExtensionConfigManager.CommandModel>> GetCommands()
		{
			Dictionary<string, List<ExtensionConfigManager.CommandModel>> dictionary = new Dictionary<string, List<ExtensionConfigManager.CommandModel>>();
			TwitchAction[] array = (from a in TwitchManager.Current.AvailableCommands.Values
			where a.HasExtraConditions() && (this.CanUseBitCommands() || a.PointType != TwitchAction.PointTypes.Bits)
			select a).ToArray<TwitchAction>();
			int num = 0;
			foreach (TwitchAction twitchAction in array)
			{
				ExtensionConfigManager.CommandModel item = new ExtensionConfigManager.CommandModel
				{
					name = twitchAction.Command.Replace("#", string.Empty).Replace("_", " ").ToUpper(),
					baseCommand = twitchAction.BaseCommand,
					command = twitchAction.Command,
					isPositive = twitchAction.IsPositive,
					spends = twitchAction.PointType.ToString(),
					cost = twitchAction.CurrentCost,
					cooldownType = (twitchAction.WaitingBlocked ? "wait" : (twitchAction.CooldownBlocked ? "regular" : "full")),
					cooldownIndex = num / 32,
					bitPosition = (byte)(num % 32),
					streamerOnly = twitchAction.StreamerOnly
				};
				List<ExtensionConfigManager.CommandModel> list;
				if (!dictionary.TryGetValue(twitchAction.MainCategory.Name, out list))
				{
					dictionary.Add(twitchAction.MainCategory.Name, list = new List<ExtensionConfigManager.CommandModel>());
				}
				list.Add(item);
				num++;
			}
			return dictionary;
		}

		public bool CanUseBitCommands()
		{
			return this.broadcasterType == "affiliate" || this.broadcasterType == "partner";
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static List<string> emptyParty = new List<string>(0);

		[PublicizedFrom(EAccessModifier.Private)]
		public string displayName = string.Empty;

		[PublicizedFrom(EAccessModifier.Private)]
		public string broadcasterType = string.Empty;

		[PublicizedFrom(EAccessModifier.Private)]
		public string jwt = string.Empty;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool hasUpdated;

		[PublicizedFrom(EAccessModifier.Private)]
		public const float pushConfigTimeout = 1f;

		[PublicizedFrom(EAccessModifier.Private)]
		public float lastPushTime = float.NegativeInfinity;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool waitingToPush;

		public class ConfigModel
		{
			public string displayName;

			public List<string> party;

			public List<string> categories;

			public string IdentityGrantHeader = Localization.Get("TwitchInfo_IdentityGrantHeader", false);

			public string IdentityGrantSubtext = Localization.Get("TwitchInfo_IdentityGrantSubtext", false);

			public string LoadingText = Localization.Get("loadActionLoading", false);

			public string OfflineHeader = Localization.Get("TwitchInfo_OfflineHeader", false);

			public string OfflineSubtext1 = Localization.Get("TwitchInfo_OfflineSubtext1", false);

			public string OfflineSubtext2 = Localization.Get("TwitchInfo_OfflineSubtext2", false);

			public string CommandDescriptionsText = Localization.Get("TwitchInfo_CommandDescriptionsText", false);

			public string ChatPromptHeader = Localization.Get("TwitchInfo_ChatPromptHeader", false);

			public string ChatPromptSubtext = Localization.Get("TwitchInfo_ChatPromptSubtext", false);

			public string ActionsOffText = Localization.Get("TwitchInfo_ActionsOffText", false);

			public string CooldownText = Localization.Get("TwitchInfo_CooldownText", false);

			public string PausedText = Localization.Get("TwitchCooldownStatus_Paused", false);

			public string ActionPresetLabel = Localization.Get("xuiOptionsTwitchActionPreset", false);

			public string VotePresetLabel = Localization.Get("xuiOptionsTwitchVotePreset", false);

			public string EventPresetLabel = Localization.Get("xuiOptionsTwitchCustomEvents", false);

			public string TopKillerLabel = Localization.Get("TwitchInfo_TopKiller", false);

			public string TopGoodLabel = Localization.Get("TwitchInfo_TopGood", false);

			public string TopBadLabel = Localization.Get("TwitchInfo_TopEvil", false);

			public string BestHelperLabel = string.Format(Localization.Get("TwitchInfo_CurrentGood", false), TwitchManager.LeaderboardStats.GoodRewardTime);

			public string TotalGoodActionsLabel = Localization.Get("TwitchInfo_TotalGood", false);

			public string TotalBadActionsLabel = Localization.Get("TwitchInfo_TotalBad", false);

			public string LargestPimpPotLabel = Localization.Get("TwitchInfo_LargestPimpPot", false);

			public string DifficultyLabel = Localization.Get("goDifficultyShort", false);

			public string DayCycleLabel = Localization.Get("goDayLength", false);

			public string ModdedLabel = Localization.Get("goModded", false);

			public string PPRateLabel = Localization.Get("TwitchInfo_PPRate", false);

			public Dictionary<string, List<ExtensionConfigManager.CommandModel>> commands;
		}

		public class CommandModel
		{
			public string name;

			public string baseCommand;

			public string command;

			public bool isPositive;

			public string spends;

			public int cost;

			public string cooldownType;

			public int cooldownIndex;

			public byte bitPosition;

			public bool streamerOnly;
		}
	}
}
