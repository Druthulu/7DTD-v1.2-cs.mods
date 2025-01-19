using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

namespace Platform.MultiPlatform
{
	[Preserve]
	public class Factory : IPlatform
	{
		public bool AsServerOnly { get; set; }

		public bool IsCrossplatform { get; set; }

		public EPlatformIdentifier PlatformIdentifier
		{
			get
			{
				return EPlatformIdentifier.None;
			}
		}

		public string PlatformDisplayName
		{
			get
			{
				return null;
			}
		}

		public void Init()
		{
			this.User.Init(this);
			this.ServerListAnnouncer.Init(this);
			this.RichPresence.Init(this);
		}

		public bool HasNetworkingEnabled(IList<string> _disabledProtocolNames)
		{
			return false;
		}

		public INetworkServer GetNetworkingServer(ProtocolManager _protocolManager)
		{
			throw new NotImplementedException();
		}

		public INetworkClient GetNetworkingClient(ProtocolManager _protocolManager)
		{
			throw new NotImplementedException();
		}

		public void UserAdded(PlatformUserIdentifierAbs _id, bool _isPrimary)
		{
			PlatformManager.NativePlatform.UserAdded(_id, _isPrimary);
			IPlatform crossplatformPlatform = PlatformManager.CrossplatformPlatform;
			if (crossplatformPlatform == null)
			{
				return;
			}
			crossplatformPlatform.UserAdded(_id, _isPrimary);
		}

		public string[] GetArgumentsForRelaunch()
		{
			return new string[0];
		}

		public void CreateInstances()
		{
			this.User = new User();
			this.ServerListAnnouncer = new ServerListAnnouncer();
			this.RichPresence = new RichPresence();
			this.PlayerInteractionsRecorder = new PlayerInteractionsRecorderMulti();
		}

		public void Update()
		{
		}

		public void LateUpdate()
		{
		}

		public void Destroy()
		{
			this.ServerListAnnouncer = null;
			IUserClient user = this.User;
			if (user != null)
			{
				user.Destroy();
			}
			this.User = null;
		}

		public IPlatformApi Api
		{
			get
			{
				return null;
			}
		}

		public IUserClient User { get; [PublicizedFrom(EAccessModifier.Private)] set; }

		public IAuthenticationClient AuthenticationClient
		{
			get
			{
				return null;
			}
		}

		public IAuthenticationServer AuthenticationServer
		{
			get
			{
				return null;
			}
		}

		public IList<IServerListInterface> ServerListInterfaces
		{
			get
			{
				if (this.serverListInterfaces != null)
				{
					return this.serverListInterfaces;
				}
				this.serverListInterfaces = new List<IServerListInterface>();
				IPlatform crossplatformPlatform = PlatformManager.CrossplatformPlatform;
				IList<IServerListInterface> list = (crossplatformPlatform != null) ? crossplatformPlatform.ServerListInterfaces : null;
				if (list != null)
				{
					this.serverListInterfaces.AddRange(list);
				}
				list = PlatformManager.NativePlatform.ServerListInterfaces;
				if (list != null)
				{
					this.serverListInterfaces.AddRange(list);
				}
				return this.serverListInterfaces;
			}
		}

		public IServerListInterface ServerLookupInterface
		{
			get
			{
				return null;
			}
		}

		public IMasterServerAnnouncer ServerListAnnouncer { get; [PublicizedFrom(EAccessModifier.Private)] set; }

		public ILobbyHost LobbyHost
		{
			get
			{
				return null;
			}
		}

		public IPlayerInteractionsRecorder PlayerInteractionsRecorder { get; [PublicizedFrom(EAccessModifier.Private)] set; }

		public IGameplayNotifier GameplayNotifier { get; [PublicizedFrom(EAccessModifier.Private)] set; }

		public IJoinSessionGameInviteListener JoinSessionGameInviteListener
		{
			get
			{
				return PlatformManager.NativePlatform.JoinSessionGameInviteListener;
			}
		}

		public IMultiplayerInvitationDialog MultiplayerInvitationDialog
		{
			get
			{
				return PlatformManager.NativePlatform.MultiplayerInvitationDialog;
			}
		}

		public IPartyVoice PartyVoice
		{
			get
			{
				IPlatform crossplatformPlatform = PlatformManager.CrossplatformPlatform;
				return ((crossplatformPlatform != null) ? crossplatformPlatform.PartyVoice : null) ?? PlatformManager.NativePlatform.PartyVoice;
			}
		}

		public IUtils Utils
		{
			get
			{
				return null;
			}
		}

		public IPlatformMemory Memory
		{
			get
			{
				return PlatformManager.NativePlatform.Memory;
			}
		}

		public IAntiCheatClient AntiCheatClient
		{
			get
			{
				IPlatform crossplatformPlatform = PlatformManager.CrossplatformPlatform;
				return ((crossplatformPlatform != null) ? crossplatformPlatform.AntiCheatClient : null) ?? PlatformManager.NativePlatform.AntiCheatClient;
			}
		}

		public IAntiCheatServer AntiCheatServer
		{
			get
			{
				IPlatform crossplatformPlatform = PlatformManager.CrossplatformPlatform;
				return ((crossplatformPlatform != null) ? crossplatformPlatform.AntiCheatServer : null) ?? PlatformManager.NativePlatform.AntiCheatServer;
			}
		}

		public IUserIdentifierMappingService IdMappingService
		{
			get
			{
				return null;
			}
		}

		public IUserDetailsService UserDetailsService
		{
			get
			{
				return null;
			}
		}

		public IPlayerReporting PlayerReporting
		{
			get
			{
				IPlatform crossplatformPlatform = PlatformManager.CrossplatformPlatform;
				return ((crossplatformPlatform != null) ? crossplatformPlatform.PlayerReporting : null) ?? PlatformManager.NativePlatform.PlayerReporting;
			}
		}

		public ITextCensor TextCensor
		{
			get
			{
				IPlatform crossplatformPlatform = PlatformManager.CrossplatformPlatform;
				ITextCensor result;
				if ((result = ((crossplatformPlatform != null) ? crossplatformPlatform.TextCensor : null)) == null)
				{
					IPlatform nativePlatform = PlatformManager.NativePlatform;
					if (nativePlatform == null)
					{
						return null;
					}
					result = nativePlatform.TextCensor;
				}
				return result;
			}
		}

		public IRemoteFileStorage RemoteFileStorage
		{
			get
			{
				IPlatform crossplatformPlatform = PlatformManager.CrossplatformPlatform;
				return ((crossplatformPlatform != null) ? crossplatformPlatform.RemoteFileStorage : null) ?? PlatformManager.NativePlatform.RemoteFileStorage;
			}
		}

		public IRemotePlayerFileStorage RemotePlayerFileStorage
		{
			get
			{
				IPlatform crossplatformPlatform = PlatformManager.CrossplatformPlatform;
				return ((crossplatformPlatform != null) ? crossplatformPlatform.RemotePlayerFileStorage : null) ?? PlatformManager.NativePlatform.RemotePlayerFileStorage;
			}
		}

		public IPlatformNetworkServer NetworkServer
		{
			get
			{
				return null;
			}
		}

		public PlayerInputManager Input
		{
			get
			{
				return null;
			}
		}

		public IVirtualKeyboard VirtualKeyboard
		{
			get
			{
				return null;
			}
		}

		public IAchievementManager AchievementManager
		{
			get
			{
				return null;
			}
		}

		public IRichPresence RichPresence { get; [PublicizedFrom(EAccessModifier.Private)] set; }

		public IApplicationStateController ApplicationState
		{
			get
			{
				return PlatformManager.NativePlatform.ApplicationState;
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public List<IServerListInterface> serverListInterfaces;
	}
}
