using System;
using System.Collections;
using System.Text.RegularExpressions;
using Unity.XGamingRuntime;
using UnityEngine;

namespace Platform.XBL
{
	[PublicizedFrom(EAccessModifier.Internal)]
	public class JoinSessionGameInviteListener : IJoinSessionGameInviteListener
	{
		public void Init(IPlatform _owner)
		{
			PlatformManager.NativePlatform.User.UserLoggedIn += delegate(IPlatform _platform)
			{
				this.userLoggedIn = (((User)_owner.User).UserStatus == EUserStatus.LoggedIn);
				XGameInviteRegistrationToken xgameInviteRegistrationToken;
				XblHelpers.Succeeded(SDK.XGameInviteRegisterForEvent(new XGameInviteEventCallback(this.inviteReceivedCallback), out xgameInviteRegistrationToken), "Register for invite event", true, true);
			};
			this.FetchSessionDetailsFromCommandLine();
		}

		public void Update()
		{
			if (!XUiC_MainMenu.openedOnce)
			{
				return;
			}
			if (!this.userLoggedIn)
			{
				return;
			}
			if (string.IsNullOrEmpty(this.connectionString))
			{
				return;
			}
			if (XUiC_WorldGenerationWindowGroup.IsGenerating())
			{
				XUiC_WorldGenerationWindowGroup.CancelGeneration();
				return;
			}
			if (this.activeCoroutine != null)
			{
				ThreadManager.StopCoroutine(this.activeCoroutine);
				this.activeCoroutine = null;
			}
			if (this.profileReady)
			{
				this.activeCoroutine = ThreadManager.StartCoroutine(this.JoinSessionCoroutine(this.connectionString));
				this.connectionString = null;
				return;
			}
			if (this.creatingProfile)
			{
				return;
			}
			if (ProfileSDF.CurrentProfileName().Length == 0)
			{
				this.creatingProfile = true;
				XUiC_OptionsProfiles.Open(LocalPlayerUI.primaryUI.xui, delegate
				{
					this.profileReady = true;
					this.creatingProfile = false;
				});
				return;
			}
			this.profileReady = true;
		}

		public void Cancel()
		{
			this.CompleteCoroutine();
		}

		public bool HasPendingIntent()
		{
			return !string.IsNullOrEmpty(this.connectionString) || this.inProgress;
		}

		public bool IsProcessingIntent(out bool _checkRestartAtMainMenu)
		{
			_checkRestartAtMainMenu = false;
			return this.inProgress;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public IEnumerator JoinSessionCoroutine(string _connectionString)
		{
			this.inProgress = true;
			while (!GameManager.Instance.IsSafeToDisconnect())
			{
				yield return null;
			}
			yield return this.RequestPlayerWarning();
			if (!this.warningAccepted || !this.inProgress)
			{
				this.CompleteCoroutine();
				yield break;
			}
			yield return this.RequestSessionDetails(_connectionString);
			if (this.serverInfo == null || !this.inProgress)
			{
				this.CompleteCoroutine();
				yield break;
			}
			yield return this.CheckMultiplayerPrivileges();
			if (!this.hasPrivileges || !this.inProgress)
			{
				this.CompleteCoroutine();
				yield break;
			}
			GameStateManager gameStateManager = GameManager.Instance.gameStateManager;
			if (gameStateManager != null && gameStateManager.IsGameStarted())
			{
				JoinSessionGameInviteListener.<>c__DisplayClass14_0 CS$<>8__locals1 = new JoinSessionGameInviteListener.<>c__DisplayClass14_0();
				CS$<>8__locals1.userCancelled = false;
				string text = Localization.Get("lblJoiningGame", false) + "\n\n[FFFFFF]" + Utils.GetCancellationMessage();
				XUiC_ProgressWindow.Open(LocalPlayerUI.primaryUI, text, delegate
				{
					CS$<>8__locals1.userCancelled = true;
				}, true, true, true, false);
				yield return new WaitForSeconds(2f);
				XUiC_ProgressWindow.Close(LocalPlayerUI.primaryUI);
				if (CS$<>8__locals1.userCancelled || !this.inProgress)
				{
					this.CompleteCoroutine();
					yield break;
				}
				CS$<>8__locals1 = null;
			}
			while (!GameManager.Instance.IsSafeToConnect())
			{
				if (!this.inProgress)
				{
					this.CompleteCoroutine();
					yield break;
				}
				if (GameManager.Instance.IsSafeToDisconnect())
				{
					GameManager.Instance.Disconnect();
				}
				yield return null;
			}
			if (!this.inProgress)
			{
				this.CompleteCoroutine();
				yield break;
			}
			if (this.serverInfo != null)
			{
				Log.Out("[XBL] Got server details, trying to connect");
				SingletonMonoBehaviour<ConnectionManager>.Instance.Connect(this.serverInfo);
			}
			this.CompleteCoroutine();
			yield break;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public IEnumerator RequestPlayerWarning()
		{
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.ClientCount() > 0)
			{
				XUiC_MessageBoxWindowGroup xuiC_MessageBoxWindowGroup = (XUiC_MessageBoxWindowGroup)((XUiWindowGroup)LocalPlayerUI.primaryUI.windowManager.GetWindow(XUiC_MessageBoxWindowGroup.ID)).Controller;
				bool dialogClosed = false;
				xuiC_MessageBoxWindowGroup.ShowMessage(Localization.Get("lblPrivilegesCloseServerHeader", false), string.Format(Localization.Get("lblPrivilegesCloseServer", false), SingletonMonoBehaviour<ConnectionManager>.Instance.ClientCount()), XUiC_MessageBoxWindowGroup.MessageBoxTypes.OkCancel, delegate
				{
					this.warningAccepted = true;
					dialogClosed = true;
				}, delegate
				{
					this.warningAccepted = false;
					dialogClosed = true;
				}, true, true, true);
				yield return new WaitUntil(() => dialogClosed);
			}
			else
			{
				this.warningAccepted = true;
			}
			yield break;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public IEnumerator RequestSessionDetails(string _connectionString)
		{
			GameServerInfo gameServerInfo = new GameServerInfo();
			gameServerInfo.SetValue(GameInfoString.UniqueId, _connectionString);
			bool serverLookupComplete = false;
			Log.Out(string.Format("[GameCore] Looking up {0} session {1}: '{2}'", PlatformManager.CrossplatformPlatform.PlatformIdentifier, 0, _connectionString));
			PlatformManager.CrossplatformPlatform.ServerLookupInterface.GetSingleServerDetails(gameServerInfo, EServerRelationType.Friends, delegate(IPlatform _platform, GameServerInfo _info, EServerRelationType _source)
			{
				serverLookupComplete = true;
				if (_info == null)
				{
					Log.Error("[GameCore] Could not find server details for session connection string: " + _connectionString);
					return;
				}
				this.serverInfo = _info;
			});
			yield return new WaitUntil(() => serverLookupComplete);
			yield break;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public IEnumerator CheckMultiplayerPrivileges()
		{
			if (this.activeCoroutine == null)
			{
				yield break;
			}
			XUiC_MultiplayerPrivilegeNotification window = XUiC_MultiplayerPrivilegeNotification.GetWindow();
			bool isCheckComplete = false;
			EUserPerms permissionsWithPrompt = this.serverInfo.AllowsCrossplay ? (EUserPerms.Multiplayer | EUserPerms.Crossplay) : EUserPerms.Multiplayer;
			if (window.ResolvePrivilegesWithDialog(permissionsWithPrompt, delegate(bool _result)
			{
				this.hasPrivileges = _result;
				isCheckComplete = true;
			}, (EUserPerms)0, 0f, true, delegate
			{
				this.hasPrivileges = false;
				isCheckComplete = true;
			}))
			{
				yield return new WaitUntil(() => isCheckComplete);
			}
			yield break;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void CompleteCoroutine()
		{
			this.activeCoroutine = null;
			this.inProgress = false;
			this.warningAccepted = false;
			this.hasPrivileges = false;
			this.serverInfo = null;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void FetchSessionDetailsFromCommandLine()
		{
			string[] commandLineArgs = GameStartupHelper.GetCommandLineArgs();
			for (int i = 0; i < commandLineArgs.Length; i++)
			{
				this.connectionString = this.parseInviteUri(commandLineArgs[i]);
				if (this.connectionString != null)
				{
					Log.Out("[XBL] Found connection string from command line: " + this.connectionString);
					return;
				}
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void inviteReceivedCallback(IntPtr _, string _inviteuri)
		{
			Log.Out("[XBL] Invite received: '" + _inviteuri + "'");
			this.connectionString = this.parseInviteUri(_inviteuri);
			if (this.connectionString == null)
			{
				Log.Error("[XBL] Received invite but could not extract connect information");
				return;
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public string parseInviteUri(string _inviteUri)
		{
			Match match = JoinSessionGameInviteListener.msInviteUriMatcher.Match(_inviteUri);
			if (!match.Success)
			{
				return null;
			}
			string[] array = match.Groups[3].Value.Split('&', StringSplitOptions.None);
			for (int i = 0; i < array.Length; i++)
			{
				string[] array2 = array[i].Split('=', StringSplitOptions.None);
				if (array2[0].EqualsCaseInsensitive("connectionString"))
				{
					return array2[1];
				}
			}
			return null;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public bool userLoggedIn;

		[PublicizedFrom(EAccessModifier.Private)]
		public string connectionString;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool profileReady;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool creatingProfile;

		[PublicizedFrom(EAccessModifier.Private)]
		public Coroutine activeCoroutine;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool inProgress;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool warningAccepted;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool hasPrivileges;

		[PublicizedFrom(EAccessModifier.Private)]
		public GameServerInfo serverInfo;

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly Regex msInviteUriMatcher = new Regex("^ms-xbl-(\\w+):\\/\\/(\\w+)\\/?\\?(.*)$", RegexOptions.Compiled);
	}
}
