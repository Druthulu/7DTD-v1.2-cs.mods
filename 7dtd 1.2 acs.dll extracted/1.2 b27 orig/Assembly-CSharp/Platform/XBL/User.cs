using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Platform.EOS;
using Unity.XGamingRuntime;

namespace Platform.XBL
{
	public class User : IUserClient
	{
		[PublicizedFrom(EAccessModifier.Private)]
		static User()
		{
			Dictionary<EBlockType, XblPermission> dictionary = EnumUtils.Values<EBlockType>().ToDictionary((EBlockType blockType) => blockType, delegate(EBlockType blockType)
			{
				XblPermission result;
				switch (blockType)
				{
				case EBlockType.TextChat:
					result = XblPermission.CommunicateUsingText;
					break;
				case EBlockType.VoiceChat:
					result = XblPermission.CommunicateUsingVoice;
					break;
				case EBlockType.Play:
					result = XblPermission.PlayMultiplayer;
					break;
				default:
					throw new NotImplementedException(string.Format("Mapping from {0}.{1} to {2} not implemented!", "EBlockType", blockType, "XblPermission"));
				}
				return result;
			});
			User.userBlockedPermissions = dictionary.Values.ToArray<XblPermission>();
			User.xblPermissionToBlockType = new EnumDictionary<XblPermission, EBlockType>();
			foreach (KeyValuePair<EBlockType, XblPermission> keyValuePair in dictionary)
			{
				EBlockType eblockType;
				XblPermission xblPermission;
				keyValuePair.Deconstruct(out eblockType, out xblPermission);
				EBlockType value = eblockType;
				XblPermission key = xblPermission;
				User.xblPermissionToBlockType.Add(key, value);
			}
			User.userBlockedAnonymousTypes = new XblAnonymousUserType[]
			{
				XblAnonymousUserType.CrossNetworkFriend,
				XblAnonymousUserType.CrossNetworkUser
			};
		}

		public void Init(IPlatform _owner)
		{
			this.owner = _owner;
			this.owner.Api.ClientApiInitialized += this.apiInitialized;
			if (!GameManager.IsDedicatedServer)
			{
				PlatformManager.CrossplatformPlatform.User.UserLoggedIn += this.CrossLoginDone;
				XblXuidMapper.XuidMapped += this.OnXuidMapped;
			}
		}

		public EUserStatus UserStatus { get; [PublicizedFrom(EAccessModifier.Private)] set; } = EUserStatus.NotAttempted;

		public event Action<IPlatform> UserLoggedIn
		{
			add
			{
				lock (this)
				{
					this.userLoggedIn = (Action<IPlatform>)Delegate.Combine(this.userLoggedIn, value);
					if (this.UserStatus == EUserStatus.LoggedIn)
					{
						value(this.owner);
					}
				}
			}
			remove
			{
				lock (this)
				{
					this.userLoggedIn = (Action<IPlatform>)Delegate.Remove(this.userLoggedIn, value);
				}
			}
		}

		public event UserBlocksChangedCallback UserBlocksChanged;

		public XUserHandle GdkUserHandle { get; [PublicizedFrom(EAccessModifier.Private)] set; }

		public XblContextHandle XblContextHandle { get; [PublicizedFrom(EAccessModifier.Private)] set; }

		public PlatformUserIdentifierAbs PlatformUserId
		{
			get
			{
				return this.userIdentifier;
			}
		}

		public void Login(LoginUserCallback _delegate)
		{
			if (this.loginActualUserStatus == EUserStatus.LoggedIn)
			{
				Log.Out("[XBL] Already logged in.");
				this.UserStatus = EUserStatus.LoggedIn;
				Action<IPlatform> action = this.userLoggedIn;
				if (action != null)
				{
					action(this.owner);
				}
				if (_delegate != null)
				{
					_delegate(this.owner, EApiStatusReason.Ok, null);
				}
				return;
			}
			Log.Out("[XBL] Login");
			this.loginUserCallback = _delegate;
			SDK.XUserAddAsync(XUserAddOptions.AddDefaultUserAllowingUI, new XUserAddCompleted(this.AddUserComplete));
		}

		public void PlayOffline(LoginUserCallback _delegate)
		{
			if (this.UserStatus != EUserStatus.LoggedIn)
			{
				throw new Exception("Can not explicitly set XBL to offline mode");
			}
			this.UserStatus = EUserStatus.OfflineMode;
			Action<IPlatform> action = this.userLoggedIn;
			if (action != null)
			{
				action(this.owner);
			}
			_delegate(this.owner, EApiStatusReason.Ok, null);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void updateAdvertisment(GameServerInfo _serverInfo)
		{
			string value = _serverInfo.GetValue(GameInfoString.UniqueId);
			uint value2 = (uint)_serverInfo.GetValue(GameInfoInt.CurrentPlayers);
			int value3 = _serverInfo.GetValue(GameInfoInt.ServerVisibility);
			uint value4 = (uint)_serverInfo.GetValue(GameInfoInt.MaxPlayers);
			if (value4 < 2U)
			{
				return;
			}
			if (string.IsNullOrEmpty(value))
			{
				return;
			}
			XblContextHandle xblContextHandle = this.XblContextHandle;
			XblMultiplayerActivityInfo xblMultiplayerActivityInfo = new XblMultiplayerActivityInfo();
			xblMultiplayerActivityInfo.ConnectionString = value;
			xblMultiplayerActivityInfo.CurrentPlayers = value2;
			xblMultiplayerActivityInfo.GroupId = "Dummy";
			XblMultiplayerActivityInfo xblMultiplayerActivityInfo2 = xblMultiplayerActivityInfo;
			XblMultiplayerActivityJoinRestriction joinRestriction;
			if (value3 != 1)
			{
				if (value3 == 2)
				{
					joinRestriction = XblMultiplayerActivityJoinRestriction.Public;
				}
				else
				{
					joinRestriction = XblMultiplayerActivityJoinRestriction.InviteOnly;
				}
			}
			else
			{
				joinRestriction = XblMultiplayerActivityJoinRestriction.Followed;
			}
			xblMultiplayerActivityInfo2.JoinRestriction = joinRestriction;
			xblMultiplayerActivityInfo.MaxPlayers = value4;
			xblMultiplayerActivityInfo.Platform = XblMultiplayerActivityPlatform.All;
			xblMultiplayerActivityInfo.Xuid = this.userXuid;
			SDK.XBL.XblMultiplayerActivitySetActivityAsync(xblContextHandle, xblMultiplayerActivityInfo, true, delegate(int _hresult)
			{
				XblHelpers.Succeeded(_hresult, "Set Activity", true, false);
			});
		}

		public void StartAdvertisePlaying(GameServerInfo _serverInfo)
		{
			_serverInfo.OnChangedString += delegate(GameServerInfo _info, GameInfoString _key)
			{
				if (_key == GameInfoString.UniqueId)
				{
					this.updateAdvertisment(_serverInfo);
				}
			};
			_serverInfo.OnChangedInt += delegate(GameServerInfo _info, GameInfoInt _key)
			{
				if (_key == GameInfoInt.CurrentPlayers || _key == GameInfoInt.ServerVisibility || _key == GameInfoInt.MaxPlayers)
				{
					this.updateAdvertisment(_serverInfo);
				}
			};
		}

		public void StopAdvertisePlaying()
		{
			SDK.XBL.XblMultiplayerActivityDeleteActivityAsync(this.XblContextHandle, delegate(int _hresult)
			{
				if (XblHelpers.Succeeded(_hresult, "Delete Activity", true, false))
				{
					Log.Out("[XBL] Activity cleared");
				}
			});
		}

		public void GetLoginTicket(Action<bool, byte[], string> _callback)
		{
			User.<>c__DisplayClass42_0 CS$<>8__locals1 = new User.<>c__DisplayClass42_0();
			CS$<>8__locals1._callback = _callback;
			SDK.XUserGetTokenAndSignatureUtf16Async(this.GdkUserHandle, XUserGetTokenAndSignatureOptions.None, "GET", "https://eos.epicgames.com/", User.eosRelyingPartyRequestHeaders, null, new XUserGetTokenAndSignatureUtf16Result(CS$<>8__locals1.<GetLoginTicket>g__CompletionRoutine|0));
		}

		public string GetFriendName(PlatformUserIdentifierAbs _playerId)
		{
			throw new NotImplementedException();
		}

		public bool IsFriend(PlatformUserIdentifierAbs _playerId)
		{
			UserIdentifierXbl userIdentifierXbl = _playerId as UserIdentifierXbl;
			if (userIdentifierXbl == null)
			{
				return false;
			}
			ulong xuid = userIdentifierXbl.Xuid;
			Log.Out(string.Format("[XBL] User logged in: {0}", xuid));
			return true;
		}

		public bool CanShowProfile(PlatformUserIdentifierAbs _playerId)
		{
			return XblXuidMapper.GetXuid(_playerId) > 0UL;
		}

		public void ShowProfile(PlatformUserIdentifierAbs _playerId)
		{
			ulong xuid = XblXuidMapper.GetXuid(_playerId);
			if (xuid != 0UL)
			{
				SDK.XGameUiShowPlayerProfileCardAsync(this.GdkUserHandle, xuid, delegate(int hr)
				{
					if (!XblHelpers.Succeeded(hr, "XGameUiShowPlayerProfileCardAsync", true, false))
					{
						Log.Error("[XBL] Showing Player Profile Failed.");
						return;
					}
					Log.Out("[XBL] Showing Player Profile Succeeded.");
				});
			}
		}

		public bool MultiplayerAllowed
		{
			[PublicizedFrom(EAccessModifier.Private)]
			get
			{
				if (this.mpAllowed != null)
				{
					return this.mpAllowed.Value;
				}
				if (this.UserStatus == EUserStatus.NotAttempted)
				{
					return true;
				}
				bool flag;
				XUserPrivilegeDenyReason enumValue;
				if (XblHelpers.Succeeded(SDK.XUserCheckPrivilege(this.GdkUserHandle, XUserPrivilegeOptions.None, XUserPrivilege.Multiplayer, out flag, out enumValue), "Check MP privilege", true, false))
				{
					Log.Out(string.Format("[XBL] MP privilege: allowed={0}, denyReason={1}", flag, enumValue.ToStringCached<XUserPrivilegeDenyReason>()));
					this.mpAllowed = new bool?(flag);
					return flag;
				}
				return false;
			}
		}

		public bool CommunicationAllowed
		{
			[PublicizedFrom(EAccessModifier.Private)]
			get
			{
				if (this.commsAllowed != null)
				{
					return this.commsAllowed.Value;
				}
				if (this.UserStatus == EUserStatus.NotAttempted)
				{
					return true;
				}
				bool flag;
				XUserPrivilegeDenyReason enumValue;
				if (XblHelpers.Succeeded(SDK.XUserCheckPrivilege(this.GdkUserHandle, XUserPrivilegeOptions.None, XUserPrivilege.Communications, out flag, out enumValue), "Check Communication privilege", true, false))
				{
					Log.Out(string.Format("[XBL] Communication privilege: allowed={0}, denyReason={1}", flag, enumValue.ToStringCached<XUserPrivilegeDenyReason>()));
					this.commsAllowed = new bool?(flag);
					return flag;
				}
				return false;
			}
		}

		public bool CrossplayAllowed
		{
			[PublicizedFrom(EAccessModifier.Private)]
			get
			{
				if (this.crossplayAllowed != null)
				{
					return this.crossplayAllowed.Value;
				}
				if (this.UserStatus == EUserStatus.NotAttempted)
				{
					return true;
				}
				bool flag;
				XUserPrivilegeDenyReason enumValue;
				if (XblHelpers.Succeeded(SDK.XUserCheckPrivilege(this.GdkUserHandle, XUserPrivilegeOptions.None, XUserPrivilege.CrossPlay, out flag, out enumValue), "Check Crossplay privilege", true, false))
				{
					Log.Out(string.Format("[XBL] Crossplay privilege: allowed={0}, denyReason={1}", flag, enumValue.ToStringCached<XUserPrivilegeDenyReason>()));
					this.crossplayAllowed = new bool?(flag);
					return flag;
				}
				return false;
			}
		}

		public EUserPerms Permissions
		{
			get
			{
				EUserPerms euserPerms = (EUserPerms)0;
				if (this.MultiplayerAllowed)
				{
					euserPerms |= (EUserPerms.Multiplayer | EUserPerms.HostMultiplayer);
				}
				if (this.CommunicationAllowed)
				{
					euserPerms |= EUserPerms.Communication;
				}
				if (this.CrossplayAllowed)
				{
					euserPerms |= EUserPerms.Crossplay;
				}
				return euserPerms;
			}
		}

		public string GetPermissionDenyReason(EUserPerms _perms)
		{
			return null;
		}

		public IEnumerator ResolvePermissions(EUserPerms _perms, bool _canPrompt, CoroutineCancellationToken _cancellationToken = null)
		{
			return Enumerable.Empty<object>().GetEnumerator();
		}

		public void UserAdded(PlatformUserIdentifierAbs _userId, bool _isPrimary)
		{
			if (!_isPrimary)
			{
				XblXuidMapper.GetXuid(_userId);
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void OnXuidMapped(IReadOnlyCollection<PlatformUserIdentifierAbs> userIds, ulong xuid)
		{
			UserBlocksChangedCallback userBlocksChanged = this.UserBlocksChanged;
			if (userBlocksChanged == null)
			{
				return;
			}
			userBlocksChanged(userIds);
		}

		public IEnumerator ResolveUserBlocks(IReadOnlyList<IPlatformUserBlockedResults> _results)
		{
			User.<>c__DisplayClass62_0 CS$<>8__locals1 = new User.<>c__DisplayClass62_0();
			CS$<>8__locals1._results = _results;
			CS$<>8__locals1.<>4__this = this;
			this.userBlockedXuidToResultsTemp.Clear();
			this.userBlockedAnonymousResultsTemp.Clear();
			PlatformUserIdentifierAbs platformUserId = this.PlatformUserId;
			foreach (IPlatformUserBlockedResults platformUserBlockedResults in CS$<>8__locals1._results)
			{
				PlatformUserIdentifierAbs nativeId = platformUserBlockedResults.User.NativeId;
				if (!object.Equals(platformUserId, nativeId))
				{
					UserIdentifierXbl userIdentifierXbl = nativeId as UserIdentifierXbl;
					ulong xuid;
					if (userIdentifierXbl == null || (xuid = userIdentifierXbl.Xuid) == 0UL)
					{
						this.userBlockedAnonymousResultsTemp.Add(platformUserBlockedResults);
					}
					else
					{
						this.userBlockedXuidToResultsTemp[xuid] = platformUserBlockedResults;
					}
				}
			}
			CS$<>8__locals1.running = true;
			SDK.XBL.XblPrivacyBatchCheckPermissionAsync(this.XblContextHandle, User.userBlockedPermissions, this.userBlockedXuidToResultsTemp.Keys.ToArray<ulong>(), (this.userBlockedAnonymousResultsTemp.Count > 0) ? User.userBlockedAnonymousTypes : Array.Empty<XblAnonymousUserType>(), new XblPrivacyBatchCheckPermissionCompleted(CS$<>8__locals1.<ResolveUserBlocks>g__CompletionRoutine|0));
			while (CS$<>8__locals1.running)
			{
				yield return null;
			}
			yield break;
		}

		public void Destroy()
		{
			SDK.XBL.XblMultiplayerActivityDeleteActivityAsync(this.XblContextHandle, delegate(int _hresult)
			{
				if (XblHelpers.Succeeded(_hresult, "Delete Activity", true, false))
				{
					Log.Out("[XBL] Activity deleted");
				}
			});
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void apiInitialized()
		{
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void AddUserComplete(int _hresult, XUserHandle _userHandle)
		{
			if (!XblHelpers.Succeeded(_hresult, "Sign in", true, false))
			{
				this.DoLoginUserCallback(EUserStatus.TemporaryError, EApiStatusReason.Unknown, string.Format("Error code: 0x{0:X8}", _hresult));
				return;
			}
			this.GdkUserHandle = _userHandle;
			string text;
			if (!XblHelpers.Succeeded(SDK.XUserGetGamertag(this.GdkUserHandle, XUserGamertagComponent.Classic, out text), "Get gamertag", true, false))
			{
				this.DoLoginUserCallback(EUserStatus.TemporaryError, EApiStatusReason.NoFriendsName, string.Format("Error code: 0x{0:X8}", _hresult));
				return;
			}
			ulong num;
			if (!XblHelpers.Succeeded(SDK.XUserGetId(this.GdkUserHandle, out num), "Get user id", true, false))
			{
				this.DoLoginUserCallback(EUserStatus.TemporaryError, EApiStatusReason.Unknown, string.Format("Error code: 0x{0:X8}", _hresult));
				return;
			}
			Log.Out(string.Format("[XBL] Signed in, id: {0} gamertag: {1}", num, text));
			XblContextHandle xblContextHandle;
			if (!XblHelpers.Succeeded(SDK.XBL.XblContextCreateHandle(this.GdkUserHandle, out xblContextHandle), "Create Xbox Live context", true, false))
			{
				this.DoLoginUserCallback(EUserStatus.TemporaryError, EApiStatusReason.Unknown, string.Format("Error code: 0x{0:X8}", _hresult));
				return;
			}
			this.XblContextHandle = xblContextHandle;
			GamePrefs.Set(EnumGamePrefs.PlayerName, text);
			this.userXuid = num;
			Dictionary<ulong, UserIdentifierXbl> dictionary = this.loadUserMappings();
			UserIdentifierXbl userId;
			if (dictionary != null && dictionary.TryGetValue(num, out userId))
			{
				this.userIdentifier = userId;
				XblXuidMapper.SetXuid(userId, this.userXuid);
			}
			this.DoLoginUserCallback(EUserStatus.LoggedIn, EApiStatusReason.Ok, null);
			this.testSocial();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void DoLoginUserCallback(EUserStatus userStatus, EApiStatusReason reason, string reasonAdditional)
		{
			this.loginActualUserStatus = userStatus;
			this.UserStatus = userStatus;
			if (userStatus == EUserStatus.LoggedIn)
			{
				Action<IPlatform> action = this.userLoggedIn;
				if (action != null)
				{
					action(this.owner);
				}
			}
			LoginUserCallback loginUserCallback = this.loginUserCallback;
			if (loginUserCallback == null)
			{
				return;
			}
			loginUserCallback(this.owner, reason, reasonAdditional);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void testSocial()
		{
			SDK.XBL.XblSocialGetSocialRelationshipsAsync(this.XblContextHandle, this.userXuid, XblSocialRelationshipFilter.All, 0U, 0U, new XblSocialGetSocialRelationshipsResult(this.<testSocial>g__GetRelationshipsCallback|67_0));
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void CrossLoginDone(IPlatform _sender)
		{
			if (this.userIdentifier != null)
			{
				return;
			}
			PlatformUserIdentifierAbs nativePlatformUserId = ((User)_sender.User).NativePlatformUserId;
			if (nativePlatformUserId.PlatformIdentifier != EPlatformIdentifier.XBL)
			{
				Log.Error("[XBL] EOS detected different native platform: " + nativePlatformUserId.PlatformIdentifierString);
				return;
			}
			this.userIdentifier = (UserIdentifierXbl)nativePlatformUserId;
			XblXuidMapper.SetXuid(this.userIdentifier, this.userXuid);
			this.saveUserMapping();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public Dictionary<ulong, UserIdentifierXbl> loadUserMappings()
		{
			if (!SdPlayerPrefs.HasKey("XblMappings"))
			{
				Log.Warning("[XBL] No XUID -> PXUID mappings found");
				return null;
			}
			Dictionary<ulong, UserIdentifierXbl> dictionary = new Dictionary<ulong, UserIdentifierXbl>();
			string[] array = SdPlayerPrefs.GetString("XblMappings").Split(';', StringSplitOptions.None);
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i].Length != 0)
				{
					string[] array2 = array[i].Split('=', StringSplitOptions.None);
					ulong key;
					if (array2.Length != 2)
					{
						Log.Warning("[XBL] Malformed user mapping entry: '" + array[i] + "'");
					}
					else if (!ulong.TryParse(array2[0], out key))
					{
						Log.Warning("[XBL] Malformed user identifier entry: '" + array2[0] + "'");
					}
					else
					{
						PlatformUserIdentifierAbs platformUserIdentifierAbs = PlatformUserIdentifierAbs.FromCombinedString(array2[1], true);
						if (platformUserIdentifierAbs == null)
						{
							Log.Warning("[XBL] Malformed user identifier XBL mapping entry: '" + array2[1] + "'");
						}
						else if (platformUserIdentifierAbs.PlatformIdentifier != EPlatformIdentifier.XBL)
						{
							Log.Warning("[XBL] Stored user identifier XBL mapping not an XBL identifier: '" + array2[1] + "'");
						}
						else
						{
							dictionary.Add(key, (UserIdentifierXbl)platformUserIdentifierAbs);
						}
					}
				}
			}
			return dictionary;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void saveUserMapping()
		{
			Dictionary<ulong, UserIdentifierXbl> dictionary = this.loadUserMappings() ?? new Dictionary<ulong, UserIdentifierXbl>();
			dictionary[this.userXuid] = this.userIdentifier;
			StringBuilder stringBuilder = new StringBuilder();
			foreach (KeyValuePair<ulong, UserIdentifierXbl> keyValuePair in dictionary)
			{
				stringBuilder.Append(keyValuePair.Key.ToString() + "=" + keyValuePair.Value.CombinedString + ";");
			}
			SdPlayerPrefs.SetString("XblMappings", stringBuilder.ToString());
			SdPlayerPrefs.Save();
		}

		[CompilerGenerated]
		[PublicizedFrom(EAccessModifier.Private)]
		public void <testSocial>g__GetRelationshipsCallback|67_0(int _hresult, XblSocialRelationshipResult _handle)
		{
			if (!XblHelpers.Succeeded(_hresult, "Get social relationships cb", true, false))
			{
				return;
			}
			XblSocialRelationship[] array;
			if (!XblHelpers.Succeeded(SDK.XBL.XblSocialRelationshipResultGetRelationships(_handle, out array), "Copy relationships", true, false))
			{
				SDK.XBL.XblSocialRelationshipResultCloseHandle(_handle);
				return;
			}
			Log.Out(string.Format("[XBL] Social relationships received: {0}", array.Length));
			for (int i = 0; i < array.Length; i++)
			{
				Log.Out(string.Format("[XBL] Social relationship {0}: {1}xxx, fav={2}, isBiDi={3}", new object[]
				{
					i,
					array[i].XboxUserId.ToString().Substring(0, 13),
					array[i].IsFavourite,
					array[i].IsFollowingCaller
				}));
			}
			bool flag = false;
			if (!XblHelpers.Succeeded(SDK.XBL.XblSocialRelationshipResultHasNext(_handle, out flag), "Get hasNext relationships", true, false))
			{
				SDK.XBL.XblSocialRelationshipResultCloseHandle(_handle);
				return;
			}
			if (flag)
			{
				SDK.XBL.XblSocialRelationshipResultGetNextAsync(this.XblContextHandle, _handle, 0U, new XblSocialRelationshipResultGetNextResult(this.<testSocial>g__GetRelationshipsCallback|67_0));
			}
			SDK.XBL.XblSocialRelationshipResultCloseHandle(_handle);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public const string xblMappingsPrefName = "XblMappings";

		[PublicizedFrom(EAccessModifier.Private)]
		public const string eosRelyingPartyUrl = "https://eos.epicgames.com/";

		[PublicizedFrom(EAccessModifier.Private)]
		public const string eosRelyingPartyHttpMethod = "GET";

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly XUserGetTokenAndSignatureUtf16HttpHeader[] eosRelyingPartyRequestHeaders = new XUserGetTokenAndSignatureUtf16HttpHeader[]
		{
			new XUserGetTokenAndSignatureUtf16HttpHeader
			{
				Name = "X-XBL-Contract-Version",
				Value = "2"
			}
		};

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly EnumDictionary<XblPermission, EBlockType> xblPermissionToBlockType;

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly XblPermission[] userBlockedPermissions;

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly XblAnonymousUserType[] userBlockedAnonymousTypes;

		[PublicizedFrom(EAccessModifier.Private)]
		public IPlatform owner;

		[PublicizedFrom(EAccessModifier.Private)]
		public EUserStatus loginActualUserStatus = EUserStatus.NotAttempted;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly Dictionary<ulong, IPlatformUserBlockedResults> userBlockedXuidToResultsTemp = new Dictionary<ulong, IPlatformUserBlockedResults>();

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly List<IPlatformUserBlockedResults> userBlockedAnonymousResultsTemp = new List<IPlatformUserBlockedResults>();

		[PublicizedFrom(EAccessModifier.Private)]
		public Action<IPlatform> userLoggedIn;

		[PublicizedFrom(EAccessModifier.Private)]
		public ulong userXuid;

		[PublicizedFrom(EAccessModifier.Private)]
		public UserIdentifierXbl userIdentifier;

		[PublicizedFrom(EAccessModifier.Private)]
		public LoginUserCallback loginUserCallback;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool? mpAllowed;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool? commsAllowed;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool? crossplayAllowed;
	}
}
