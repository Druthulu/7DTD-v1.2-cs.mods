using System;
using System.Collections;
using Steamworks;
using UnityEngine;

namespace Platform.Steam
{
	[PublicizedFrom(EAccessModifier.Internal)]
	public class JoinSessionGameInviteListener : IJoinSessionGameInviteListener
	{
		public void Init(IPlatform _owner)
		{
			if (this.m_friends_serverchange == null)
			{
				this.m_friends_serverchange = Callback<GameServerChangeRequested_t>.Create(new Callback<GameServerChangeRequested_t>.DispatchDelegate(this.Friends_GameServerChangeRequested));
			}
			PlatformManager.NativePlatform.User.UserLoggedIn += delegate(IPlatform _platform)
			{
				this.userLoggedIn = (((User)_owner.User).UserStatus == EUserStatus.LoggedIn);
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
			if (string.IsNullOrEmpty(this.lobbyId) && string.IsNullOrEmpty(this.sessionDetails))
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
				this.activeCoroutine = ThreadManager.StartCoroutine(this.JoinSessionCoroutine(this.sessionDetails, this.sessionPassword, this.lobbyId));
				this.sessionDetails = null;
				this.sessionPassword = null;
				this.lobbyId = null;
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
			return !string.IsNullOrEmpty(this.lobbyId) || !string.IsNullOrEmpty(this.sessionDetails) || this.inProgress;
		}

		public bool IsProcessingIntent(out bool _checkRestartAtMainMenu)
		{
			_checkRestartAtMainMenu = false;
			return this.inProgress;
		}

		public void SetLobby(GameLobbyJoinRequested_t _value)
		{
			this.CompleteCoroutine();
			this.lobbyId = (_value.m_steamIDLobby.m_SteamID.ToString() ?? "");
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public IEnumerator JoinSessionCoroutine(string _session, string _password, string _lobbyId)
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
			yield return this.CheckMultiplayerPrivileges();
			if (!this.hasPrivileges || !this.inProgress)
			{
				this.CompleteCoroutine();
				yield break;
			}
			GameStateManager gameStateManager = GameManager.Instance.gameStateManager;
			if (gameStateManager != null && gameStateManager.IsGameStarted())
			{
				JoinSessionGameInviteListener.<>c__DisplayClass17_0 CS$<>8__locals1 = new JoinSessionGameInviteListener.<>c__DisplayClass17_0();
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
			if (!string.IsNullOrEmpty(_lobbyId))
			{
				ILobbyHost lobbyHost = PlatformManager.NativePlatform.LobbyHost;
				if (lobbyHost != null)
				{
					lobbyHost.JoinLobby(_lobbyId, null);
				}
			}
			else
			{
				string[] array = _session.Split(':', StringSplitOptions.None);
				string value = "";
				int num = 0;
				if (array.Length == 2)
				{
					value = array[0];
					num = Convert.ToInt32(array[1]);
				}
				GameServerInfo gameServerInfo = new GameServerInfo();
				gameServerInfo.SetValue(GameInfoString.IP, value);
				gameServerInfo.SetValue(GameInfoInt.Port, num);
				if (!string.IsNullOrEmpty(_password))
				{
					ServerInfoCache.Instance.SavePassword(gameServerInfo, _password);
				}
				if (num != 0)
				{
					SingletonMonoBehaviour<ConnectionManager>.Instance.Connect(gameServerInfo);
				}
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
		public IEnumerator CheckMultiplayerPrivileges()
		{
			if (this.activeCoroutine == null)
			{
				yield break;
			}
			XUiC_MultiplayerPrivilegeNotification window = XUiC_MultiplayerPrivilegeNotification.GetWindow();
			bool isCheckComplete = false;
			if (window.ResolvePrivilegesWithDialog(EUserPerms.Multiplayer, delegate(bool _result)
			{
				this.hasPrivileges = _result;
				isCheckComplete = true;
			}, EUserPerms.Crossplay, 0f, true, delegate
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
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void FetchSessionDetailsFromCommandLine()
		{
			string[] commandLineArgs = GameStartupHelper.GetCommandLineArgs();
			for (int i = 0; i < commandLineArgs.Length; i++)
			{
				if (commandLineArgs[i].StartsWith("+connect_lobby") && commandLineArgs.Length > i + 1)
				{
					Log.Out("Found lobby " + commandLineArgs[i + 1]);
					if (commandLineArgs[i + 1].Length > 1)
					{
						this.lobbyId = commandLineArgs[i + 1];
					}
				}
				else if ((commandLineArgs[i].StartsWith("+connect") || commandLineArgs[i].StartsWith("-connect")) && commandLineArgs.Length > i + 1)
				{
					Log.Out("Found ip " + commandLineArgs[i + 1]);
					if (commandLineArgs[i + 1].Length > 1 && commandLineArgs[i + 1].Split(':', StringSplitOptions.None).Length == 2)
					{
						this.sessionDetails = commandLineArgs[i + 1];
					}
				}
				else if (commandLineArgs[i].StartsWith("+password") && commandLineArgs.Length > i + 1)
				{
					Log.Out("Found password");
					if (commandLineArgs[i + 1].Length > 1)
					{
						this.sessionPassword = commandLineArgs[i + 1];
					}
				}
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void Friends_GameServerChangeRequested(GameServerChangeRequested_t _value)
		{
			Log.Out("[Steamworks.NET] Friends_GameServerChangeRequested");
			this.CompleteCoroutine();
			this.sessionDetails = _value.m_rgchServer;
			this.sessionPassword = _value.m_rgchPassword;
			string[] array = _value.m_rgchServer.Split(':', StringSplitOptions.None);
			string value = "";
			int num = 0;
			if (array.Length == 2)
			{
				value = array[0];
				num = Convert.ToInt32(array[1]);
			}
			GameServerInfo gameServerInfo = new GameServerInfo();
			gameServerInfo.SetValue(GameInfoString.IP, value);
			gameServerInfo.SetValue(GameInfoInt.Port, num);
			if (!string.IsNullOrEmpty(_value.m_rgchPassword))
			{
				ServerInfoCache.Instance.SavePassword(gameServerInfo, _value.m_rgchPassword);
			}
			if (num != 0)
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.Connect(gameServerInfo);
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public Callback<GameServerChangeRequested_t> m_friends_serverchange;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool userLoggedIn;

		[PublicizedFrom(EAccessModifier.Private)]
		public string lobbyId;

		[PublicizedFrom(EAccessModifier.Private)]
		public string sessionDetails;

		[PublicizedFrom(EAccessModifier.Private)]
		public string sessionPassword;

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
	}
}
