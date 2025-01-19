using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Epic.OnlineServices;
using Epic.OnlineServices.Connect;
using UnityEngine;

namespace Platform.EOS
{
	public class User : IUserClient
	{
		public ConnectInterface connectInterface
		{
			[PublicizedFrom(EAccessModifier.Private)]
			get
			{
				return ((Api)this.owner.Api).ConnectInterface;
			}
		}

		public void Init(IPlatform _owner)
		{
			this.owner = _owner;
			this.owner.Api.ClientApiInitialized += this.apiInitialized;
			EPlatformIdentifier platformIdentifier = PlatformManager.NativePlatform.PlatformIdentifier;
			ExternalCredentialType externalCredentialType;
			switch (platformIdentifier)
			{
			case EPlatformIdentifier.Steam:
				externalCredentialType = ExternalCredentialType.SteamAppTicket;
				break;
			case EPlatformIdentifier.XBL:
				externalCredentialType = ExternalCredentialType.XblXstsToken;
				break;
			case EPlatformIdentifier.PSN:
				externalCredentialType = ExternalCredentialType.PsnIdToken;
				break;
			default:
				throw new Exception("[EOS] Can not run EOS with the " + platformIdentifier.ToStringCached<EPlatformIdentifier>() + " platform");
			}
			this.externalCredentialType = externalCredentialType;
			this.nativeApplicationStateController = PlatformManager.NativePlatform.ApplicationState;
			if (this.nativeApplicationStateController != null)
			{
				this.nativeApplicationStateController.OnApplicationStateChanged += this.OnApplicationStateChanged;
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

		public event UserBlocksChangedCallback UserBlocksChanged
		{
			add
			{
			}
			remove
			{
			}
		}

		public PlatformUserIdentifierAbs PlatformUserId
		{
			get
			{
				return this.platformUserId;
			}
		}

		public PlatformUserIdentifierAbs NativePlatformUserId { get; [PublicizedFrom(EAccessModifier.Private)] set; }

		public void Login(LoginUserCallback _delegate)
		{
			if (this.UserStatus == EUserStatus.LoggedIn)
			{
				Log.Out("[EOS] Login already done.");
				this.eosLoginDone(_delegate);
				return;
			}
			Log.Out("[EOS] Login");
			EosHelpers.TestEosConnection(delegate(bool _success)
			{
				if (!_success)
				{
					this.UserStatus = EUserStatus.OfflineMode;
					_delegate(this.owner, EApiStatusReason.Other, "No connection to EOS backend");
					return;
				}
				if (PlatformManager.NativePlatform.User.UserStatus == EUserStatus.LoggedIn)
				{
					this.FetchTicket(_delegate, false);
					return;
				}
				this.UserStatus = EUserStatus.OfflineMode;
				_delegate(this.owner, EApiStatusReason.Other, "User offline");
			});
		}

		public void PlayOffline(LoginUserCallback _delegate)
		{
			this.UserStatus = EUserStatus.NotAttempted;
			Dictionary<PlatformUserIdentifierAbs, UserIdentifierEos> dictionary = this.loadUserMappings();
			if (dictionary == null)
			{
				_delegate(this.owner, EApiStatusReason.NoOnlineStart, null);
				return;
			}
			PlatformUserIdentifierAbs platformUserIdentifierAbs = PlatformManager.NativePlatform.User.PlatformUserId;
			if (platformUserIdentifierAbs == null)
			{
				Log.Warning("[EOS] No native platform user logged in, can not proceed in offline mode");
				_delegate(this.owner, EApiStatusReason.Other, "Not logged in to native platform");
				return;
			}
			UserIdentifierEos userIdentifierEos;
			if (!dictionary.TryGetValue(platformUserIdentifierAbs, out userIdentifierEos))
			{
				Log.Warning("[EOS] No mapping for the logged in user: " + platformUserIdentifierAbs.CombinedString);
				_delegate(this.owner, EApiStatusReason.NoOnlineStart, null);
				return;
			}
			this.platformUserId = userIdentifierEos;
			this.UserStatus = EUserStatus.OfflineMode;
			Action<IPlatform> action = this.userLoggedIn;
			if (action != null)
			{
				action(this.owner);
			}
			_delegate(this.owner, EApiStatusReason.NotLoggedOn, null);
		}

		public void StartAdvertisePlaying(GameServerInfo _serverInfo)
		{
		}

		public void StopAdvertisePlaying()
		{
		}

		public void GetLoginTicket(Action<bool, byte[], string> _callback)
		{
			throw new NotImplementedException();
		}

		public string GetFriendName(PlatformUserIdentifierAbs _playerId)
		{
			throw new NotImplementedException();
		}

		public bool IsFriend(PlatformUserIdentifierAbs _playerId)
		{
			throw new NotImplementedException();
		}

		public EUserPerms Permissions
		{
			get
			{
				if (this.playerHasSanctions)
				{
					return EUserPerms.Multiplayer | EUserPerms.Communication | EUserPerms.Crossplay;
				}
				return EUserPerms.All;
			}
		}

		public string GetPermissionDenyReason(EUserPerms _perms)
		{
			EUserPerms euserPerms = ~this.Permissions & _perms;
			if (euserPerms.HasFlag(EUserPerms.HostMultiplayer))
			{
				return this.reasonForPermissions;
			}
			return null;
		}

		public IEnumerator ResolvePermissions(EUserPerms _perms, bool _canPrompt, CoroutineCancellationToken _cancellationToken = null)
		{
			Log.Out(string.Format("[EOS] {0}({1}: [{2}], {3}: {4})", new object[]
			{
				"ResolvePermissions",
				"_perms",
				_perms,
				"_canPrompt",
				_canPrompt
			}));
			if (this.UserStatus == EUserStatus.LoggedIn)
			{
				if (((Api)this.owner.Api).SanctionsInterface == null || ((Api)this.owner.Api).eosSanctionsCheck == null)
				{
					Log.Out(string.Format("[EOS] ResolvePermissions not possible: eosSanctionsCheck: {0}, SanctionsInterface: {1}", ((Api)this.owner.Api).eosSanctionsCheck != null, ((Api)this.owner.Api).SanctionsInterface != null));
					this.playerHasSanctions = true;
					yield break;
				}
				if (_perms.HasHostMultiplayer())
				{
					User.<>c__DisplayClass41_0 CS$<>8__locals1 = new User.<>c__DisplayClass41_0();
					CS$<>8__locals1.connectionTestComplete = false;
					CS$<>8__locals1.connectionTestSuccess = false;
					EosHelpers.TestEosConnection(delegate(bool isConnected)
					{
						CS$<>8__locals1.connectionTestComplete = true;
						CS$<>8__locals1.connectionTestSuccess = isConnected;
					});
					while (!CS$<>8__locals1.connectionTestComplete)
					{
						yield return null;
						if (_cancellationToken != null && _cancellationToken.IsCancelled())
						{
							yield break;
						}
					}
					if (!CS$<>8__locals1.connectionTestSuccess)
					{
						Log.Out("[EOS] Could not check sanctions as the connection test failed");
						this.playerHasSanctions = true;
						this.reasonForPermissions = Localization.Get("permissionsSanction_error", false);
						yield break;
					}
					yield return (this.owner.Api as Api).eosSanctionsCheck.CheckSanctionsEnumerator((this.owner.Api as Api).SanctionsInterface, this.platformUserId.ProductUserId, this.platformUserId.ProductUserId, delegate(SanctionsCheckResult checkResult)
					{
						if (!checkResult.Success)
						{
							this.playerHasSanctions = true;
							this.reasonForPermissions = Localization.Get("permissionsSanction_error", false);
							return;
						}
						Log.Out(string.Format("[EOS] CheckSanctionsEnumerator: hasSanctions {0}", checkResult.HasActiveSanctions));
						if (checkResult.HasActiveSanctions)
						{
							this.playerHasSanctions = true;
							this.reasonForPermissions = checkResult.ReasonForSanction;
							return;
						}
						this.playerHasSanctions = false;
						this.reasonForPermissions = null;
					}, _cancellationToken);
					CS$<>8__locals1 = null;
				}
			}
			yield break;
		}

		public void UserAdded(PlatformUserIdentifierAbs _userId, bool _isPrimary)
		{
		}

		public IEnumerator ResolveUserBlocks(IReadOnlyList<IPlatformUserBlockedResults> _results)
		{
			return Enumerable.Empty<object>().GetEnumerator();
		}

		public void Destroy()
		{
			EosHelpers.AssertMainThread("Usr.Destroy");
			this.RemoveNotifications();
			if (this.nativeApplicationStateController != null)
			{
				this.nativeApplicationStateController.OnApplicationStateChanged -= this.OnApplicationStateChanged;
				this.nativeApplicationStateController = null;
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void apiInitialized()
		{
			EosHelpers.AssertMainThread("Usr.Init");
			this.AddNotifications();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void OnApplicationStateChanged(ApplicationState newState)
		{
			bool flag = newState == ApplicationState.Suspended;
			if (this.wasSuspended == flag)
			{
				return;
			}
			this.wasSuspended = flag;
			if (flag)
			{
				this.OnSuspend();
				return;
			}
			this.resumeCount++;
			this.OnResume();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void OnSuspend()
		{
			this.shouldRefreshLoginOnResume = (this.shouldRefreshLoginOnResume || this.UserStatus == EUserStatus.LoggedIn);
			Log.Out(string.Format("[EOS] User.OnSuspend() shouldRefreshLoginOnResume: {0}", this.shouldRefreshLoginOnResume));
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void OnResume()
		{
			Log.Out(string.Format("[EOS] User.OnResume() shouldRefreshLoginOnResume: {0}", this.shouldRefreshLoginOnResume));
			if (this.shouldRefreshLoginOnResume)
			{
				ThreadManager.StartCoroutine(this.OnResumeRefreshLoginCoroutine());
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public IEnumerator OnResumeRefreshLoginCoroutine()
		{
			User.<>c__DisplayClass49_0 CS$<>8__locals1;
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.initialResumeCount = this.resumeCount;
			yield return this.RefreshLoginCoroutine();
			if (this.<OnResumeRefreshLoginCoroutine>g__ShouldExitCoroutine|49_0(true, ref CS$<>8__locals1))
			{
				yield break;
			}
			if (this.nativeApplicationStateController != null)
			{
				Log.Out("[EOS] Waiting for network to be ready...");
				for (;;)
				{
					yield return new WaitForSecondsRealtime(0.25f);
					if (this.<OnResumeRefreshLoginCoroutine>g__ShouldExitCoroutine|49_0(false, ref CS$<>8__locals1))
					{
						break;
					}
					if (this.nativeApplicationStateController.NetworkConnectionState)
					{
						goto Block_4;
					}
				}
				yield break;
				Block_4:
				Log.Out("[EOS] Network is ready. Trying to refresh login...");
				yield return this.RefreshLoginCoroutine();
				if (this.<OnResumeRefreshLoginCoroutine>g__ShouldExitCoroutine|49_0(true, ref CS$<>8__locals1))
				{
					yield break;
				}
			}
			User.<>c__DisplayClass49_1 CS$<>8__locals2 = new User.<>c__DisplayClass49_1();
			Log.Out("[EOS] Waiting for EOS to be reachable...");
			CS$<>8__locals2.eosReachable = false;
			bool eosReachableFirstCheck = true;
			float waitTime = 2f;
			while (!CS$<>8__locals2.eosReachable)
			{
				User.<>c__DisplayClass49_2 CS$<>8__locals3 = new User.<>c__DisplayClass49_2();
				CS$<>8__locals3.CS$<>8__locals1 = CS$<>8__locals2;
				if (eosReachableFirstCheck)
				{
					eosReachableFirstCheck = false;
				}
				else
				{
					Log.Out(string.Format("[EOS] No connection to EOS. Will retry in {0} s", waitTime));
					yield return new WaitForSecondsRealtime(waitTime);
					waitTime = Math.Min(waitTime * 2f, 60f);
				}
				if (this.<OnResumeRefreshLoginCoroutine>g__ShouldExitCoroutine|49_0(false, ref CS$<>8__locals1))
				{
					yield break;
				}
				Log.Out("[EOS] Testing connecting to EOS...");
				CS$<>8__locals3.eosTestComplete = false;
				EosHelpers.TestEosConnection(delegate(bool success)
				{
					CS$<>8__locals3.CS$<>8__locals1.eosReachable = success;
					CS$<>8__locals3.eosTestComplete = true;
				});
				while (!CS$<>8__locals3.eosTestComplete)
				{
					yield return new WaitForSecondsRealtime(0.25f);
					if (this.<OnResumeRefreshLoginCoroutine>g__ShouldExitCoroutine|49_0(false, ref CS$<>8__locals1))
					{
						yield break;
					}
				}
				CS$<>8__locals3 = null;
			}
			Log.Out("[EOS] EOS is reachable so we can try refresh the login now.");
			CS$<>8__locals2 = null;
			yield return this.RefreshLoginCoroutine();
			if (this.UserStatus == EUserStatus.LoggedIn)
			{
				Log.Warning("[EOS] Refresh login on resume has failed. User will have to trigger a login through other means.");
			}
			yield break;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void AddNotifications()
		{
			if (this.connectInterface == null)
			{
				return;
			}
			EosHelpers.AssertMainThread("Usr.AddNtfs");
			AddNotifyAuthExpirationOptions addNotifyAuthExpirationOptions = default(AddNotifyAuthExpirationOptions);
			object lockObject = AntiCheatCommon.LockObject;
			lock (lockObject)
			{
				this.notifyAuthExpirationHandle = this.connectInterface.AddNotifyAuthExpiration(ref addNotifyAuthExpirationOptions, null, new OnAuthExpirationCallback(this.OnAuthExpiration));
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void RemoveNotifications()
		{
			if (this.connectInterface == null)
			{
				return;
			}
			EosHelpers.AssertMainThread("Usr.RemNtfs");
			if (this.notifyAuthExpirationHandle != 0UL)
			{
				object lockObject = AntiCheatCommon.LockObject;
				lock (lockObject)
				{
					this.connectInterface.RemoveNotifyAuthExpiration(this.notifyAuthExpirationHandle);
				}
				this.notifyAuthExpirationHandle = 0UL;
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void OnAuthExpiration(ref AuthExpirationCallbackInfo _data)
		{
			this.RefreshLogin();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void RefreshLogin()
		{
			Log.Out("[EOS] Refreshing Login");
			this.FetchTicket(null, true);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public IEnumerator RefreshLoginCoroutine()
		{
			bool done = false;
			Log.Out("[EOS] Refreshing Login");
			this.FetchTicket(delegate(IPlatform _, EApiStatusReason _, string _)
			{
				done = true;
			}, true);
			while (!done)
			{
				yield return new WaitForSecondsRealtime(0.25f);
			}
			yield break;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void FetchTicket(LoginUserCallback _delegate, bool _refreshing = false)
		{
			PlatformManager.NativePlatform.User.GetLoginTicket(delegate(bool _success, byte[] _byteTicket, string _stringTicket)
			{
				if (_success)
				{
					this.ConnectLogin(_byteTicket, _stringTicket, this.externalCredentialType, _delegate, _refreshing);
					return;
				}
				Log.Error("[EOS] Failed fetching login ticket from native platform");
				this.UserStatus = EUserStatus.TemporaryError;
				LoginUserCallback @delegate = _delegate;
				if (@delegate == null)
				{
					return;
				}
				@delegate(this.owner, EApiStatusReason.NoLoginTicket, null);
			});
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void ConnectLogin(byte[] _byteTicket, string _stringTicket, ExternalCredentialType _externalType, LoginUserCallback _callback, bool _refreshing)
		{
			EosHelpers.AssertMainThread("Usr.Log");
			Utf8String token = (_byteTicket != null) ? Common.ToString(new ArraySegment<byte>(_byteTicket)) : new Utf8String(_stringTicket);
			LoginOptions loginOptions = new LoginOptions
			{
				Credentials = new Credentials?(new Credentials
				{
					Token = token,
					Type = _externalType
				}),
				UserLoginInfo = null
			};
			object lockObject = AntiCheatCommon.LockObject;
			lock (lockObject)
			{
				this.connectInterface.Login(ref loginOptions, null, delegate(ref LoginCallbackInfo _callbackData)
				{
					if (_callbackData.ResultCode == Result.Success)
					{
						if (!_refreshing)
						{
							string str = "[EOS] Login succeeded, PUID: ";
							ProductUserId localUserId = _callbackData.LocalUserId;
							Log.Out(str + ((localUserId != null) ? localUserId.ToString() : null));
							this.eosLoggedIn(_callbackData.LocalUserId, _callback);
							return;
						}
						Log.Out("[EOS] Login refreshed");
						return;
					}
					else if (Common.IsOperationComplete(_callbackData.ResultCode))
					{
						if (_callbackData.ResultCode == Result.InvalidUser)
						{
							if (!_refreshing)
							{
								this.ConnectCreateUser(_callbackData.ContinuanceToken, _callback);
								return;
							}
							Log.Error("[EOS] Login refresh failed, invalid user");
							return;
						}
						else
						{
							Log.Warning(string.Format("[EOS] Login {0}failed: {1}", _refreshing ? "refresh " : "", _callbackData.ResultCode));
							this.UserStatus = ((_callbackData.ResultCode == Result.UnexpectedError) ? EUserStatus.OfflineMode : EUserStatus.TemporaryError);
							LoginUserCallback callback = _callback;
							if (callback == null)
							{
								return;
							}
							callback(this.owner, EApiStatusReason.Unknown, _callbackData.ResultCode.ToStringCached<Result>());
							return;
						}
					}
					else
					{
						Log.Error("[EOS] Login " + (_refreshing ? "refresh " : "") + "error: " + _callbackData.ResultCode.ToString());
						this.UserStatus = EUserStatus.PermanentError;
						LoginUserCallback callback2 = _callback;
						if (callback2 == null)
						{
							return;
						}
						callback2(this.owner, EApiStatusReason.Unknown, _callbackData.ResultCode.ToStringCached<Result>());
						return;
					}
				});
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void ConnectCreateUser(ContinuanceToken _continuanceToken, LoginUserCallback _callback)
		{
			EosHelpers.AssertMainThread("Usr.Create");
			Log.Out("[EOS] Creating account");
			CreateUserOptions createUserOptions = new CreateUserOptions
			{
				ContinuanceToken = _continuanceToken
			};
			object lockObject = AntiCheatCommon.LockObject;
			lock (lockObject)
			{
				this.connectInterface.CreateUser(ref createUserOptions, null, delegate(ref CreateUserCallbackInfo _callbackData)
				{
					if (_callbackData.ResultCode == Result.Success)
					{
						string str = "[EOS] CreateUser succeeded, PUID: ";
						ProductUserId localUserId = _callbackData.LocalUserId;
						Log.Out(str + ((localUserId != null) ? localUserId.ToString() : null));
						this.SyncExternalAccountInfo(_callbackData.LocalUserId, _callback);
						return;
					}
					if (Common.IsOperationComplete(_callbackData.ResultCode))
					{
						Log.Warning("[EOS] CreateUser failed: " + _callbackData.ResultCode.ToString());
						this.UserStatus = EUserStatus.TemporaryError;
						LoginUserCallback callback = _callback;
						if (callback == null)
						{
							return;
						}
						callback(this.owner, EApiStatusReason.Unknown, _callbackData.ResultCode.ToStringCached<Result>());
						return;
					}
					else
					{
						Log.Error("[EOS] CreateUser error: " + _callbackData.ResultCode.ToString());
						this.UserStatus = EUserStatus.PermanentError;
						LoginUserCallback callback2 = _callback;
						if (callback2 == null)
						{
							return;
						}
						callback2(this.owner, EApiStatusReason.Unknown, _callbackData.ResultCode.ToStringCached<Result>());
						return;
					}
				});
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void SyncExternalAccountInfo(ProductUserId _puid, LoginUserCallback _callback)
		{
			if (PlatformManager.NativePlatform.PlatformIdentifier == EPlatformIdentifier.XBL)
			{
				Log.Out("[EOS] EnsureAccountInfo required for this platform, starting additional login");
				OnLoginCallback <>9__1;
				PlatformManager.NativePlatform.User.GetLoginTicket(delegate(bool _success, byte[] _byteTicket, string _stringTicket)
				{
					Utf8String token = (_byteTicket != null) ? Common.ToString(new ArraySegment<byte>(_byteTicket)) : new Utf8String(_stringTicket);
					LoginOptions loginOptions = new LoginOptions
					{
						Credentials = new Credentials?(new Credentials
						{
							Token = token,
							Type = this.externalCredentialType
						}),
						UserLoginInfo = null
					};
					object lockObject = AntiCheatCommon.LockObject;
					lock (lockObject)
					{
						ConnectInterface connectInterface = this.connectInterface;
						object clientData = null;
						OnLoginCallback completionDelegate;
						if ((completionDelegate = <>9__1) == null)
						{
							completionDelegate = (<>9__1 = delegate(ref LoginCallbackInfo _callbackData)
							{
								if (_callbackData.ResultCode == Result.Success)
								{
									string str = "[EOS] ensure account info succeeded, PUID: ";
									ProductUserId localUserId = _callbackData.LocalUserId;
									Log.Out(str + ((localUserId != null) ? localUserId.ToString() : null));
									this.eosLoggedIn(_callbackData.LocalUserId, _callback);
									return;
								}
								if (Common.IsOperationComplete(_callbackData.ResultCode))
								{
									Log.Warning("[EOS] ensure account info failed: " + _callbackData.ResultCode.ToString());
									this.UserStatus = EUserStatus.TemporaryError;
									LoginUserCallback callback = _callback;
									if (callback == null)
									{
										return;
									}
									callback(this.owner, EApiStatusReason.Unknown, _callbackData.ResultCode.ToStringCached<Result>());
									return;
								}
								else
								{
									Log.Error("[EOS] ensure account info error: " + _callbackData.ResultCode.ToString());
									this.UserStatus = EUserStatus.PermanentError;
									LoginUserCallback callback2 = _callback;
									if (callback2 == null)
									{
										return;
									}
									callback2(this.owner, EApiStatusReason.Unknown, _callbackData.ResultCode.ToStringCached<Result>());
									return;
								}
							});
						}
						connectInterface.Login(ref loginOptions, clientData, completionDelegate);
					}
				});
				return;
			}
			this.eosLoggedIn(_puid, _callback);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void eosLoggedIn(ProductUserId _puid, LoginUserCallback _callback)
		{
			this.platformUserId = new UserIdentifierEos(_puid);
			this.GetNativePlatformUserIdentifier(_callback);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void GetNativePlatformUserIdentifier(LoginUserCallback _callback)
		{
			Log.Out("[EOS] Getting native user for " + this.platformUserId.ReadablePlatformUserIdentifier);
			IdToken value = new IdToken
			{
				JsonWebToken = this.owner.AuthenticationClient.GetAuthTicket(),
				ProductUserId = this.platformUserId.ProductUserId
			};
			VerifyIdTokenOptions verifyIdTokenOptions = new VerifyIdTokenOptions
			{
				IdToken = new IdToken?(value)
			};
			object lockObject = AntiCheatCommon.LockObject;
			lock (lockObject)
			{
				this.connectInterface.VerifyIdToken(ref verifyIdTokenOptions, null, delegate(ref VerifyIdTokenCallbackInfo _callbackData)
				{
					if (_callbackData.ResultCode != Result.Success)
					{
						Log.Error("[EOS] VerifyIdToken failed: " + _callbackData.ResultCode.ToStringCached<Result>());
						this.UserStatus = EUserStatus.TemporaryError;
						_callback(this.owner, EApiStatusReason.Unknown, _callbackData.ResultCode.ToStringCached<Result>());
						return;
					}
					if (!_callbackData.IsAccountInfoPresent)
					{
						Log.Error("[EOS] VerifyIdToken failed: No account info");
						this.UserStatus = EUserStatus.TemporaryError;
						_callback(this.owner, EApiStatusReason.Unknown, "NoAccountInfo");
						return;
					}
					_callbackData.Platform;
					string text = _callbackData.AccountId;
					_callbackData.DeviceType;
					ExternalAccountType accountIdType = _callbackData.AccountIdType;
					ProductUserId productUserId = _callbackData.ProductUserId;
					EPlatformIdentifier enumValue;
					if (!EosHelpers.AccountTypeMappings.TryGetValue(accountIdType, out enumValue))
					{
						Log.Error("[EOS] VerifyIdToken failed: Unsupported account type: " + accountIdType.ToString());
						this.UserStatus = EUserStatus.TemporaryError;
						_callback(this.owner, EApiStatusReason.Unknown, "UnsupportedAccountType");
						return;
					}
					string text2 = productUserId.ToString();
					if (text2 != this.platformUserId.ProductUserIdString)
					{
						Log.Error("[EOS] VerifyIdToken failed: PUID mismatch: " + text2);
						this.UserStatus = EUserStatus.TemporaryError;
						_callback(this.owner, EApiStatusReason.Unknown, "PUID mismatch");
						return;
					}
					PlatformUserIdentifierAbs platformUserIdentifierAbs = PlatformUserIdentifierAbs.FromPlatformAndId(enumValue.ToStringCached<EPlatformIdentifier>(), text, true);
					if (platformUserIdentifierAbs == null)
					{
						Log.Error("[EOS] VerifyIdToken failed: Could not create user identifier from platform/accountid: " + enumValue.ToStringCached<EPlatformIdentifier>() + "/" + text);
						this.UserStatus = EUserStatus.TemporaryError;
						_callback(this.owner, EApiStatusReason.Unknown, "NoUserId");
						return;
					}
					this.NativePlatformUserId = platformUserIdentifierAbs;
					this.eosLoginDone(_callback);
				});
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void eosLoginDone(LoginUserCallback _callback)
		{
			this.UserStatus = EUserStatus.LoggedIn;
			Action<IPlatform> action = this.userLoggedIn;
			if (action != null)
			{
				action(this.owner);
			}
			if (_callback != null)
			{
				_callback(this.owner, EApiStatusReason.Ok, null);
			}
			this.saveUserMapping();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public Dictionary<PlatformUserIdentifierAbs, UserIdentifierEos> loadUserMappings()
		{
			if (!SdPlayerPrefs.HasKey("EosMappings"))
			{
				Log.Warning("[EOS] No platform -> EOS mappings found");
				return null;
			}
			Dictionary<PlatformUserIdentifierAbs, UserIdentifierEos> dictionary = new Dictionary<PlatformUserIdentifierAbs, UserIdentifierEos>();
			string[] array = SdPlayerPrefs.GetString("EosMappings").Split(';', StringSplitOptions.None);
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i].Length != 0)
				{
					string[] array2 = array[i].Split('=', StringSplitOptions.None);
					if (array2.Length != 2)
					{
						Log.Warning("[EOS] Malformed user mapping entry: '" + array[i] + "'");
					}
					else
					{
						PlatformUserIdentifierAbs platformUserIdentifierAbs = PlatformUserIdentifierAbs.FromCombinedString(array2[0], true);
						if (platformUserIdentifierAbs == null)
						{
							Log.Warning("[EOS] Malformed user identifier entry: '" + array2[0] + "'");
						}
						else
						{
							PlatformUserIdentifierAbs platformUserIdentifierAbs2 = PlatformUserIdentifierAbs.FromCombinedString(array2[1], true);
							if (platformUserIdentifierAbs2 == null)
							{
								Log.Warning("[EOS] Malformed user identifier EOS mapping entry: '" + array2[1] + "'");
							}
							else if (platformUserIdentifierAbs2.PlatformIdentifier != EPlatformIdentifier.EOS)
							{
								Log.Warning("[EOS] Stored user identifier EOS mapping not an EOS identifier: '" + array2[1] + "'");
							}
							else
							{
								if (dictionary.ContainsKey(platformUserIdentifierAbs))
								{
									Log.Warning("[EOS] User identifier found multiple times: " + array2[0]);
								}
								dictionary[platformUserIdentifierAbs] = (UserIdentifierEos)platformUserIdentifierAbs2;
							}
						}
					}
				}
			}
			return dictionary;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void saveUserMapping()
		{
			Dictionary<PlatformUserIdentifierAbs, UserIdentifierEos> dictionary = this.loadUserMappings() ?? new Dictionary<PlatformUserIdentifierAbs, UserIdentifierEos>();
			dictionary[PlatformManager.NativePlatform.User.PlatformUserId] = this.platformUserId;
			StringBuilder stringBuilder = new StringBuilder();
			foreach (KeyValuePair<PlatformUserIdentifierAbs, UserIdentifierEos> keyValuePair in dictionary)
			{
				stringBuilder.Append(keyValuePair.Key.CombinedString + "=" + keyValuePair.Value.CombinedString + ";");
			}
			SdPlayerPrefs.SetString("EosMappings", stringBuilder.ToString());
			SdPlayerPrefs.Save();
		}

		[CompilerGenerated]
		[PublicizedFrom(EAccessModifier.Private)]
		public bool <OnResumeRefreshLoginCoroutine>g__ShouldExitCoroutine|49_0(bool attemptedLogin, ref User.<>c__DisplayClass49_0 A_2)
		{
			if (A_2.initialResumeCount != this.resumeCount)
			{
				Log.Out("[EOS] Another resume is in progress. Exiting.");
				return true;
			}
			if (!this.shouldRefreshLoginOnResume)
			{
				Log.Out("[EOS] Refresh login on resume is no longer needed. Exiting.");
				return true;
			}
			if (this.UserStatus == EUserStatus.LoggedIn)
			{
				this.shouldRefreshLoginOnResume = false;
				if (!attemptedLogin)
				{
					Log.Out("[EOS] User logged in through other means. Exiting.");
				}
				else
				{
					Log.Out("[EOS] User successfully logged in on resume.");
				}
				return true;
			}
			return false;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public const string eosMappingsPrefName = "EosMappings";

		[PublicizedFrom(EAccessModifier.Private)]
		public IPlatform owner;

		[PublicizedFrom(EAccessModifier.Private)]
		public IApplicationStateController nativeApplicationStateController;

		[PublicizedFrom(EAccessModifier.Private)]
		public ExternalCredentialType externalCredentialType;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool wasSuspended;

		[PublicizedFrom(EAccessModifier.Private)]
		public int resumeCount;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool shouldRefreshLoginOnResume;

		[PublicizedFrom(EAccessModifier.Private)]
		public ulong notifyAuthExpirationHandle;

		[PublicizedFrom(EAccessModifier.Private)]
		public Action<IPlatform> userLoggedIn;

		[PublicizedFrom(EAccessModifier.Private)]
		public UserIdentifierEos platformUserId;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool playerHasSanctions;

		[PublicizedFrom(EAccessModifier.Private)]
		public string reasonForPermissions;
	}
}
