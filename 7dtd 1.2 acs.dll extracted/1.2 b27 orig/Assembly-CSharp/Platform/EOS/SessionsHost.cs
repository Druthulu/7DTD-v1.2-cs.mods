using System;
using System.Collections.Generic;
using System.Text;
using Epic.OnlineServices;
using Epic.OnlineServices.Sessions;
using UnityEngine;

namespace Platform.EOS
{
	public class SessionsHost : IMasterServerAnnouncer
	{
		public void Init(IPlatform _owner)
		{
			this.owner = _owner;
			this.owner.Api.ClientApiInitialized += this.apiInitialized;
		}

		public void Update()
		{
			if (!this.GameServerInitialized)
			{
				if (this.updatesSessionModification != null)
				{
					object lockObject = AntiCheatCommon.LockObject;
					lock (lockObject)
					{
						this.updatesSessionModification.Release();
					}
					this.updatesSessionModification = null;
				}
				return;
			}
			if (this.commitBackendCountdown.HasPassed())
			{
				this.commitBackendCountdown.Reset();
				this.commitBackendCountdown.SetTimeout(30f);
				if (this.updatesSessionModification != null)
				{
					this.commitSessionToBackend(false, this.updatesSessionModification);
					this.updatesSessionModification = null;
				}
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void apiInitialized()
		{
			object lockObject = AntiCheatCommon.LockObject;
			lock (lockObject)
			{
				this.sessionsInterface = ((Api)this.owner.Api).PlatformInterface.GetSessionsInterface();
			}
		}

		public bool GameServerInitialized
		{
			get
			{
				return this.sessionId != null;
			}
		}

		public string GetServerPorts()
		{
			return string.Empty;
		}

		public void AdvertiseServer(Action _onServerRegistered)
		{
			Log.Out("[EOS] Registering server");
			EosHelpers.AssertMainThread("SeHo.Adv");
			this.onServerRegistered = _onServerRegistered;
			IUserClient user = this.owner.User;
			UserIdentifierEos userIdentifierEos = (UserIdentifierEos)((user != null) ? user.PlatformUserId : null);
			if (this.sessionsInterface == null)
			{
				return;
			}
			GameServerInfo localServerInfo = SingletonMonoBehaviour<ConnectionManager>.Instance.LocalServerInfo;
			localServerInfo.SetValue(GameInfoString.CombinedPrimaryId, (userIdentifierEos != null) ? userIdentifierEos.CombinedString : null);
			GameServerInfo gameServerInfo = localServerInfo;
			GameInfoString key = GameInfoString.CombinedNativeId;
			IUserClient user2 = PlatformManager.NativePlatform.User;
			string value;
			if (user2 == null)
			{
				value = null;
			}
			else
			{
				PlatformUserIdentifierAbs platformUserId = user2.PlatformUserId;
				value = ((platformUserId != null) ? platformUserId.CombinedString : null);
			}
			gameServerInfo.SetValue(key, value);
			CreateSessionModificationOptions createSessionModificationOptions = new CreateSessionModificationOptions
			{
				SessionName = "GameHost",
				BucketId = "<WeDontCare>",
				MaxPlayers = (uint)localServerInfo.GetValue(GameInfoInt.MaxPlayers),
				LocalUserId = ((userIdentifierEos != null) ? userIdentifierEos.ProductUserId : null),
				PresenceEnabled = false,
				SanctionsEnabled = this.owner.AntiCheatServer.ServerEacEnabled(),
				AllowedPlatformIds = EPlayGroupExtensions.GetCurrentlyAllowedPlatformIds()
			};
			object lockObject = AntiCheatCommon.LockObject;
			SessionModification sessionModification;
			Result result;
			lock (lockObject)
			{
				result = this.sessionsInterface.CreateSessionModification(ref createSessionModificationOptions, out sessionModification);
			}
			if (result != Result.Success)
			{
				Log.Error("[EOS] Failed creating session modification: " + result.ToStringCached<Result>());
				lockObject = AntiCheatCommon.LockObject;
				lock (lockObject)
				{
					if (sessionModification != null)
					{
						sessionModification.Release();
					}
				}
				return;
			}
			SessionModificationSetPermissionLevelOptions sessionModificationSetPermissionLevelOptions = default(SessionModificationSetPermissionLevelOptions);
			int value2 = localServerInfo.GetValue(GameInfoInt.ServerVisibility);
			OnlineSessionPermissionLevel permissionLevel;
			if (value2 != 1)
			{
				if (value2 == 2)
				{
					permissionLevel = OnlineSessionPermissionLevel.PublicAdvertised;
				}
				else
				{
					permissionLevel = OnlineSessionPermissionLevel.JoinViaPresence;
				}
			}
			else
			{
				permissionLevel = OnlineSessionPermissionLevel.JoinViaPresence;
			}
			sessionModificationSetPermissionLevelOptions.PermissionLevel = permissionLevel;
			SessionModificationSetPermissionLevelOptions sessionModificationSetPermissionLevelOptions2 = sessionModificationSetPermissionLevelOptions;
			lockObject = AntiCheatCommon.LockObject;
			lock (lockObject)
			{
				result = sessionModification.SetPermissionLevel(ref sessionModificationSetPermissionLevelOptions2);
			}
			if (result != Result.Success)
			{
				Log.Error("[EOS] Failed setting permission level: " + result.ToStringCached<Result>());
				lockObject = AntiCheatCommon.LockObject;
				lock (lockObject)
				{
					sessionModification.Release();
				}
				return;
			}
			SessionModificationSetJoinInProgressAllowedOptions sessionModificationSetJoinInProgressAllowedOptions = new SessionModificationSetJoinInProgressAllowedOptions
			{
				AllowJoinInProgress = true
			};
			lockObject = AntiCheatCommon.LockObject;
			lock (lockObject)
			{
				result = sessionModification.SetJoinInProgressAllowed(ref sessionModificationSetJoinInProgressAllowedOptions);
			}
			if (result != Result.Success)
			{
				Log.Error("[EOS] Failed setting join in progress: " + result.ToStringCached<Result>());
				lockObject = AntiCheatCommon.LockObject;
				lock (lockObject)
				{
					sessionModification.Release();
				}
				return;
			}
			SessionModificationSetInvitesAllowedOptions sessionModificationSetInvitesAllowedOptions = new SessionModificationSetInvitesAllowedOptions
			{
				InvitesAllowed = false
			};
			lockObject = AntiCheatCommon.LockObject;
			lock (lockObject)
			{
				result = sessionModification.SetInvitesAllowed(ref sessionModificationSetInvitesAllowedOptions);
			}
			if (result != Result.Success)
			{
				Log.Error("[EOS] Failed setting invites allowed: " + result.ToStringCached<Result>());
				lockObject = AntiCheatCommon.LockObject;
				lock (lockObject)
				{
					sessionModification.Release();
				}
				return;
			}
			if (!this.setBaseAttributes(sessionModification, localServerInfo))
			{
				lockObject = AntiCheatCommon.LockObject;
				lock (lockObject)
				{
					sessionModification.Release();
				}
				return;
			}
			this.commitSessionToBackend(true, sessionModification);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void sessionRegisteredCallback(ref UpdateSessionCallbackInfo _callbackData)
		{
			if (this.onServerRegistered == null)
			{
				return;
			}
			if (_callbackData.ResultCode != Result.Success)
			{
				Log.Error("[EOS] Failed registering session on backend: " + _callbackData.ResultCode.ToStringCached<Result>());
				Log.Warning(string.Format("[EOS] Attribute count: {0}", this.registeredAttributes.Count));
				return;
			}
			this.sessionId = _callbackData.SessionId;
			Log.Out(string.Format("[EOS] Server registered, session: {0}, {1} attributes", this.sessionId, this.registeredAttributes.Count));
			GameServerInfo localServerInfo = SingletonMonoBehaviour<ConnectionManager>.Instance.LocalServerInfo;
			localServerInfo.OnChangedString += this.updateSessionString;
			localServerInfo.OnChangedInt += this.updateSessionInt;
			localServerInfo.OnChangedBool += this.updateSessionBool;
			localServerInfo.SetValue(GameInfoString.IP, this.getPublicIpFromHostedSession());
			localServerInfo.SetValue(GameInfoString.UniqueId, this.sessionId);
			Action action = this.onServerRegistered;
			if (action == null)
			{
				return;
			}
			action();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public string getPublicIpFromHostedSession()
		{
			CopyActiveSessionHandleOptions copyActiveSessionHandleOptions = new CopyActiveSessionHandleOptions
			{
				SessionName = "GameHost"
			};
			object lockObject = AntiCheatCommon.LockObject;
			ActiveSession activeSession;
			Result result;
			lock (lockObject)
			{
				result = this.sessionsInterface.CopyActiveSessionHandle(ref copyActiveSessionHandleOptions, out activeSession);
			}
			if (result != Result.Success)
			{
				Log.Error("[EOS] Failed getting active session: " + result.ToStringCached<Result>());
				return null;
			}
			ActiveSessionCopyInfoOptions activeSessionCopyInfoOptions = default(ActiveSessionCopyInfoOptions);
			lockObject = AntiCheatCommon.LockObject;
			ActiveSessionInfo? activeSessionInfo;
			lock (lockObject)
			{
				result = activeSession.CopyInfo(ref activeSessionCopyInfoOptions, out activeSessionInfo);
			}
			if (result != Result.Success)
			{
				Log.Error("[EOS] Failed getting active session info: " + result.ToStringCached<Result>());
				lockObject = AntiCheatCommon.LockObject;
				lock (lockObject)
				{
					activeSession.Release();
				}
				return null;
			}
			string text = activeSessionInfo.Value.SessionDetails.Value.HostAddress;
			Log.Out("[EOS] Session address: " + Utils.MaskIp(text));
			lockObject = AntiCheatCommon.LockObject;
			lock (lockObject)
			{
				activeSession.Release();
			}
			return text;
		}

		public void StopServer()
		{
			EosHelpers.AssertMainThread("SeHo.Stop");
			this.onServerRegistered = null;
			if (!this.GameServerInitialized)
			{
				return;
			}
			Log.Out("[EOS] Unregistering server");
			if (SingletonMonoBehaviour<ConnectionManager>.Instance != null && SingletonMonoBehaviour<ConnectionManager>.Instance.LocalServerInfo != null)
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.LocalServerInfo.OnChangedString -= this.updateSessionString;
				SingletonMonoBehaviour<ConnectionManager>.Instance.LocalServerInfo.OnChangedInt -= this.updateSessionInt;
				SingletonMonoBehaviour<ConnectionManager>.Instance.LocalServerInfo.OnChangedBool -= this.updateSessionBool;
			}
			this.registeredAttributes.Clear();
			DestroySessionOptions destroySessionOptions = new DestroySessionOptions
			{
				SessionName = "GameHost"
			};
			object lockObject = AntiCheatCommon.LockObject;
			lock (lockObject)
			{
				this.sessionsInterface.DestroySession(ref destroySessionOptions, null, delegate(ref DestroySessionCallbackInfo _callbackData)
				{
					if (_callbackData.ResultCode == Result.Success)
					{
						Log.Out("[EOS] Server unregistered");
						this.sessionId = null;
						return;
					}
					Log.Error("[EOS] Failed unregistering session on backend: " + _callbackData.ResultCode.ToStringCached<Result>());
				});
			}
		}

		public void RegisterUser(ClientInfo _cInfo)
		{
			EosHelpers.AssertMainThread("SeHo.Reg");
			RegisterPlayersOptions registerPlayersOptions = new RegisterPlayersOptions
			{
				SessionName = "GameHost",
				PlayersToRegister = new ProductUserId[]
				{
					((UserIdentifierEos)_cInfo.CrossplatformId).ProductUserId
				}
			};
			object lockObject = AntiCheatCommon.LockObject;
			lock (lockObject)
			{
				this.sessionsInterface.RegisterPlayers(ref registerPlayersOptions, null, delegate(ref RegisterPlayersCallbackInfo _callbackData)
				{
					if (_callbackData.ResultCode != Result.Success)
					{
						Log.Error("[EOS] Failed registering player in session: " + _callbackData.ResultCode.ToStringCached<Result>());
						return;
					}
					if (_callbackData.SanctionedPlayers != null)
					{
						ProductUserId[] sanctionedPlayers = _callbackData.SanctionedPlayers;
						for (int i = 0; i < sanctionedPlayers.Length; i++)
						{
							if (sanctionedPlayers[i] == ((UserIdentifierEos)_cInfo.CrossplatformId).ProductUserId)
							{
								Log.Out("Player " + _cInfo.playerName + " has a sanction and cannot join the session, kicking player");
								GameUtils.KickPlayerForClientInfo(_cInfo, new GameUtils.KickPlayerData(GameUtils.EKickReason.CrossPlatformAuthenticationFailed, 9, default(DateTime), "Sanction"));
							}
						}
					}
				});
			}
		}

		public void UnregisterUser(ClientInfo _cInfo)
		{
			if (((_cInfo != null) ? _cInfo.CrossplatformId : null) == null)
			{
				return;
			}
			EosHelpers.AssertMainThread("SeHo.Free");
			UnregisterPlayersOptions unregisterPlayersOptions = new UnregisterPlayersOptions
			{
				SessionName = "GameHost",
				PlayersToUnregister = new ProductUserId[]
				{
					((UserIdentifierEos)_cInfo.CrossplatformId).ProductUserId
				}
			};
			object lockObject = AntiCheatCommon.LockObject;
			lock (lockObject)
			{
				this.sessionsInterface.UnregisterPlayers(ref unregisterPlayersOptions, null, delegate(ref UnregisterPlayersCallbackInfo _callbackData)
				{
					if (_callbackData.ResultCode != Result.Success)
					{
						Log.Error("[EOS] Failed unregistering player in session: " + _callbackData.ResultCode.ToStringCached<Result>());
						return;
					}
				});
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public SessionModification getSessionModificationHandle()
		{
			UpdateSessionModificationOptions updateSessionModificationOptions = new UpdateSessionModificationOptions
			{
				SessionName = "GameHost"
			};
			object lockObject = AntiCheatCommon.LockObject;
			SessionModification sessionModification;
			Result result;
			lock (lockObject)
			{
				result = this.sessionsInterface.UpdateSessionModification(ref updateSessionModificationOptions, out sessionModification);
			}
			if (result != Result.Success)
			{
				Log.Error("[EOS] Failed getting session modification: " + result.ToStringCached<Result>());
				lockObject = AntiCheatCommon.LockObject;
				lock (lockObject)
				{
					sessionModification.Release();
				}
				sessionModification = null;
			}
			return sessionModification;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public bool addAttribute(SessionModification _sessionModificationHandle, string _key, string _value)
		{
			if (_value == null)
			{
				_value = "";
			}
			_value = _value + "~$#$~" + _value.ToLowerInvariant();
			return this.addAttributeInternal(_sessionModificationHandle, _key, new AttributeDataValue
			{
				AsUtf8 = _value
			}, _value);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public bool addAttribute(SessionModification _sessionModificationHandle, string _key, int _value)
		{
			return this.addAttributeInternal(_sessionModificationHandle, _key, new AttributeDataValue
			{
				AsInt64 = new long?((long)_value)
			}, _value.ToString());
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public bool addAttribute(SessionModification _sessionModificationHandle, string _key, bool _value)
		{
			return this.addAttributeInternal(_sessionModificationHandle, _key, new AttributeDataValue
			{
				AsBool = new bool?(_value)
			}, _value.ToString());
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public bool addBoolsAttribute(SessionModification _sessionModificationHandle, string _values)
		{
			return this.addAttributeInternal(_sessionModificationHandle, "-BoolValues-", new AttributeDataValue
			{
				AsUtf8 = _values
			}, _values);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public bool addAttributeInternal(SessionModification _sessionModificationHandle, string _key, AttributeDataValue _value, string _valueString)
		{
			SessionModificationAddAttributeOptions sessionModificationAddAttributeOptions = new SessionModificationAddAttributeOptions
			{
				AdvertisementType = SessionAttributeAdvertisementType.Advertise,
				SessionAttribute = new AttributeData?(new AttributeData
				{
					Key = _key,
					Value = _value
				})
			};
			object lockObject = AntiCheatCommon.LockObject;
			Result result;
			lock (lockObject)
			{
				result = _sessionModificationHandle.AddAttribute(ref sessionModificationAddAttributeOptions);
			}
			if (result == Result.Success)
			{
				this.registeredAttributes.Add(_key);
				return true;
			}
			Log.Error(string.Format("[EOS] Failed setting {0}th attribute '{1}' to '{2}': {3}", new object[]
			{
				this.registeredAttributes.Count + 1,
				_key,
				_valueString,
				result.ToStringCached<Result>()
			}));
			return false;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public bool setBaseAttributes(SessionModification _sessionModificationHandle, GameServerInfo _gameServerInfo)
		{
			foreach (GameInfoInt gameInfoInt in GameServerInfo.IntInfosInGameTags)
			{
				if (!this.addAttribute(_sessionModificationHandle, gameInfoInt.ToStringCached<GameInfoInt>(), _gameServerInfo.GetValue(gameInfoInt)))
				{
					return false;
				}
			}
			if (!this.addBoolsAttribute(_sessionModificationHandle, this.getBoolsString(_gameServerInfo)))
			{
				return false;
			}
			foreach (GameInfoString gameInfoString in GameServerInfo.SearchableStringInfos)
			{
				if (!this.addAttribute(_sessionModificationHandle, gameInfoString.ToStringCached<GameInfoString>(), _gameServerInfo.GetValue(gameInfoString)))
				{
					return false;
				}
			}
			return true;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public SessionModification getUpdateSessionModification()
		{
			if (this.updatesSessionModification == null)
			{
				this.updatesSessionModification = this.getSessionModificationHandle();
			}
			return this.updatesSessionModification;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void updateSessionString(GameServerInfo _gameServerInfo, GameInfoString _gameInfoKey)
		{
			if (!this.GameServerInitialized)
			{
				return;
			}
			if (!GameServerInfo.IsSearchable(_gameInfoKey))
			{
				return;
			}
			if (!this.commitBackendCountdown.IsRunning)
			{
				this.commitBackendCountdown.ResetAndRestart();
			}
			if (_gameInfoKey.ToStringCached<GameInfoString>().EndsWith("ID", StringComparison.OrdinalIgnoreCase))
			{
				this.commitBackendCountdown.SetTimeout(5f);
			}
			this.addAttribute(this.getUpdateSessionModification(), _gameInfoKey.ToStringCached<GameInfoString>(), _gameServerInfo.GetValue(_gameInfoKey));
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void updateSessionInt(GameServerInfo _gameServerInfo, GameInfoInt _gameInfoKey)
		{
			if (!this.GameServerInitialized)
			{
				return;
			}
			if (!GameServerInfo.IsSearchable(_gameInfoKey))
			{
				return;
			}
			if (!this.commitBackendCountdown.IsRunning)
			{
				this.commitBackendCountdown.ResetAndRestart();
			}
			this.addAttribute(this.getUpdateSessionModification(), _gameInfoKey.ToStringCached<GameInfoInt>(), _gameServerInfo.GetValue(_gameInfoKey));
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void updateSessionBool(GameServerInfo _gameServerInfo, GameInfoBool _gameInfoKey)
		{
			if (!this.GameServerInitialized)
			{
				return;
			}
			if (!GameServerInfo.IsSearchable(_gameInfoKey))
			{
				return;
			}
			if (!this.commitBackendCountdown.IsRunning)
			{
				this.commitBackendCountdown.ResetAndRestart();
			}
			this.addBoolsAttribute(this.getUpdateSessionModification(), this.getBoolsString(_gameServerInfo));
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public string getBoolsString(GameServerInfo _gameServerInfo)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(',');
			foreach (GameInfoBool gameInfoBool in GameServerInfo.BoolInfosInGameTags)
			{
				stringBuilder.Append(gameInfoBool.ToStringCached<GameInfoBool>());
				stringBuilder.Append('=');
				stringBuilder.Append(_gameServerInfo.GetValue(gameInfoBool) ? '1' : '0');
				stringBuilder.Append(',');
			}
			return stringBuilder.ToString();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void commitSessionToBackend(bool _initialRegistration, SessionModification _sessionModification)
		{
			UpdateSessionOptions updateSessionOptions = new UpdateSessionOptions
			{
				SessionModificationHandle = _sessionModification
			};
			object lockObject = AntiCheatCommon.LockObject;
			lock (lockObject)
			{
				this.sessionsInterface.UpdateSession(ref updateSessionOptions, new SessionsHost.SessionModificationCallbackArgs(_sessionModification, _initialRegistration, _initialRegistration ? new OnUpdateSessionCallback(this.sessionRegisteredCallback) : new OnUpdateSessionCallback(this.sessionUpdatedCallback)), new OnUpdateSessionCallback(this.commitSessionCallbackWrapper));
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void commitSessionCallbackWrapper(ref UpdateSessionCallbackInfo _callbackData)
		{
			SessionsHost.SessionModificationCallbackArgs sessionModificationCallbackArgs = (SessionsHost.SessionModificationCallbackArgs)_callbackData.ClientData;
			if (_callbackData.ResultCode == Result.OperationWillRetry)
			{
				Log.Warning("[EOS] Failed updating session on backend, will retry");
				return;
			}
			sessionModificationCallbackArgs.SessionModification.Release();
			sessionModificationCallbackArgs.SessionModification = null;
			if (sessionModificationCallbackArgs.IsInitialRegistration || this.GameServerInitialized)
			{
				sessionModificationCallbackArgs.Callback(ref _callbackData);
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void sessionUpdatedCallback(ref UpdateSessionCallbackInfo _callbackData)
		{
			if (_callbackData.ResultCode != Result.Success)
			{
				Log.Error("[EOS] Failed updating session on backend: " + _callbackData.ResultCode.ToStringCached<Result>() + ". From: " + StackTraceUtility.ExtractStackTrace());
				Log.Warning(string.Format("[EOS] Attribute count: {0}", this.registeredAttributes.Count));
				return;
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public const float sessionUpdateIntervalSecsDefault = 30f;

		[PublicizedFrom(EAccessModifier.Private)]
		public const float sessionUpdateIntervalSecsImportant = 5f;

		[PublicizedFrom(EAccessModifier.Private)]
		public const string sessionName = "GameHost";

		[PublicizedFrom(EAccessModifier.Private)]
		public const string bucketId = "<WeDontCare>";

		public const string EmptyStringAttributeValue = "##EMPTY##";

		public const string LowerCaseAttributeSeparator = "~$#$~";

		public const string BoolsAttributeName = "-BoolValues-";

		[PublicizedFrom(EAccessModifier.Private)]
		public IPlatform owner;

		[PublicizedFrom(EAccessModifier.Private)]
		public SessionsInterface sessionsInterface;

		[PublicizedFrom(EAccessModifier.Private)]
		public string sessionId;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly CountdownTimer commitBackendCountdown = new CountdownTimer(30f, false);

		[PublicizedFrom(EAccessModifier.Private)]
		public SessionModification updatesSessionModification;

		[PublicizedFrom(EAccessModifier.Private)]
		public Action onServerRegistered;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly HashSet<string> registeredAttributes = new HashSet<string>();

		[PublicizedFrom(EAccessModifier.Private)]
		public class SessionModificationCallbackArgs
		{
			public SessionModificationCallbackArgs(SessionModification _sessionModification, bool _isInitialRegistration, OnUpdateSessionCallback _callback)
			{
				this.SessionModification = _sessionModification;
				this.IsInitialRegistration = _isInitialRegistration;
				this.Callback = _callback;
			}

			public SessionModification SessionModification;

			public readonly bool IsInitialRegistration;

			public readonly OnUpdateSessionCallback Callback;
		}
	}
}
