using System;
using System.IO;
using System.Runtime.CompilerServices;
using Epic.OnlineServices;
using Epic.OnlineServices.AntiCheatCommon;
using Epic.OnlineServices.AntiCheatServer;

namespace Platform.EOS
{
	public class AntiCheatServer : IAntiCheatServer, IAntiCheatEncryption
	{
		public void Init(IPlatform _owner)
		{
			this.owner = _owner;
			this.owner.Api.ClientApiInitialized += this.apiInitialized;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void apiInitialized()
		{
			EosHelpers.AssertMainThread("ACS.Init");
			object lockObject = AntiCheatCommon.LockObject;
			lock (lockObject)
			{
				this.antiCheatInterface = ((Api)this.owner.Api).PlatformInterface.GetAntiCheatServerInterface();
			}
			if (this.antiCheatInterface == null)
			{
				Log.Out("[EAC] AntiCheatServer initialized with null interface");
				return;
			}
			AddNotifyMessageToClientOptions addNotifyMessageToClientOptions = default(AddNotifyMessageToClientOptions);
			lockObject = AntiCheatCommon.LockObject;
			lock (lockObject)
			{
				this.antiCheatInterface.AddNotifyMessageToClient(ref addNotifyMessageToClientOptions, null, new OnMessageToClientCallback(this.handleMessageToClient));
			}
			AddNotifyClientActionRequiredOptions addNotifyClientActionRequiredOptions = default(AddNotifyClientActionRequiredOptions);
			lockObject = AntiCheatCommon.LockObject;
			lock (lockObject)
			{
				this.antiCheatInterface.AddNotifyClientActionRequired(ref addNotifyClientActionRequiredOptions, null, new OnClientActionRequiredCallback(this.handleClientAction));
			}
			AddNotifyClientAuthStatusChangedOptions addNotifyClientAuthStatusChangedOptions = default(AddNotifyClientAuthStatusChangedOptions);
			lockObject = AntiCheatCommon.LockObject;
			lock (lockObject)
			{
				this.antiCheatInterface.AddNotifyClientAuthStatusChanged(ref addNotifyClientAuthStatusChangedOptions, null, new OnClientAuthStatusChangedCallback(this.handleClientAuthStateChange));
			}
		}

		public void Update()
		{
		}

		public bool GetHostUserIdAndToken([TupleElementNames(new string[]
		{
			"userId",
			"token"
		})] out ValueTuple<PlatformUserIdentifierAbs, string> _hostUserIdAndToken)
		{
			_hostUserIdAndToken = default(ValueTuple<PlatformUserIdentifierAbs, string>);
			return false;
		}

		public bool StartServer(AuthenticationSuccessfulCallbackDelegate _authSuccessfulDelegate, KickPlayerDelegate _kickPlayerDelegate)
		{
			if (this.ServerEacEnabled())
			{
				Log.Out("[EAC] Starting EAC server");
				this.authSuccessfulDelegate = _authSuccessfulDelegate;
				this.kickPlayerDelegate = _kickPlayerDelegate;
				ProductUserId localUserId = GameManager.IsDedicatedServer ? null : ((UserIdentifierEos)this.owner.User.PlatformUserId).ProductUserId;
				string value = SingletonMonoBehaviour<ConnectionManager>.Instance.LocalServerInfo.GetValue(GameInfoString.GameHost);
				BeginSessionOptions beginSessionOptions = new BeginSessionOptions
				{
					EnableGameplayData = false,
					LocalUserId = localUserId,
					RegisterTimeoutSeconds = 60U,
					ServerName = value
				};
				object lockObject = AntiCheatCommon.LockObject;
				Result result;
				lock (lockObject)
				{
					result = this.antiCheatInterface.BeginSession(ref beginSessionOptions);
				}
				if (result != Result.Success)
				{
					Log.Error("[EOS-ACS] Starting module failed: " + result.ToStringCached<Result>());
				}
				else
				{
					this.serverRunning = true;
				}
				return result == Result.Success;
			}
			return true;
		}

		public bool RegisterUser(ClientInfo _client)
		{
			if (!this.serverRunning)
			{
				return false;
			}
			Log.Out(string.Format("[EOS-ACS] Registering user: {0}", _client));
			EosHelpers.AssertMainThread("ACS.Reg");
			RegisterClientOptions registerClientOptions = new RegisterClientOptions
			{
				UserId = ((UserIdentifierEos)_client.CrossplatformId).ProductUserId,
				ClientHandle = AntiCheatCommon.ClientInfoToIntPtr(_client),
				ClientPlatform = EosHelpers.DeviceTypeToAntiCheatPlatformMappings[_client.device],
				ClientType = (_client.requiresAntiCheat ? AntiCheatCommonClientType.ProtectedClient : AntiCheatCommonClientType.UnprotectedClient),
				IpAddress = _client.ip
			};
			object lockObject = AntiCheatCommon.LockObject;
			Result result;
			lock (lockObject)
			{
				result = this.antiCheatInterface.RegisterClient(ref registerClientOptions);
			}
			if (result != Result.Success)
			{
				Log.Error("[EOS-ACS] Failed registerung user: " + result.ToStringCached<Result>());
				return false;
			}
			return true;
		}

		public void FreeUser(ClientInfo _client)
		{
			if (!this.serverRunning)
			{
				return;
			}
			EosHelpers.AssertMainThread("ACS.Free");
			Log.Out(string.Format("[EOS-ACS] FreeUser: {0}", _client));
			UnregisterClientOptions unregisterClientOptions = new UnregisterClientOptions
			{
				ClientHandle = AntiCheatCommon.ClientInfoToIntPtr(_client)
			};
			object lockObject = AntiCheatCommon.LockObject;
			Result result;
			lock (lockObject)
			{
				result = this.antiCheatInterface.UnregisterClient(ref unregisterClientOptions);
			}
			if (result != Result.Success)
			{
				Log.Error("[EOS-ACS] Failed unregistering user: " + result.ToStringCached<Result>());
			}
		}

		public void HandleMessageFromClient(ClientInfo _cInfo, byte[] _data)
		{
			if (!this.serverRunning)
			{
				Log.Warning("[EOS-ACS] Server: Received EAC package but EAC was not initialized");
				return;
			}
			if (AntiCheatCommon.DebugEacVerbose)
			{
				Log.Out(string.Format("[EOS-ACS] PushNetworkMessage (len={0}, from={1})", _data.Length, _cInfo.InternalId));
			}
			ReceiveMessageFromClientOptions receiveMessageFromClientOptions = new ReceiveMessageFromClientOptions
			{
				Data = new ArraySegment<byte>(_data),
				ClientHandle = AntiCheatCommon.ClientInfoToIntPtr(_cInfo)
			};
			object lockObject = AntiCheatCommon.LockObject;
			Result result;
			lock (lockObject)
			{
				result = this.antiCheatInterface.ReceiveMessageFromClient(ref receiveMessageFromClientOptions);
			}
			if (result != Result.Success)
			{
				Log.Error("[EOS-ACS] Failed handling message: " + result.ToStringCached<Result>());
			}
		}

		public void StopServer()
		{
			if (!this.serverRunning)
			{
				return;
			}
			EndSessionOptions endSessionOptions = default(EndSessionOptions);
			object lockObject = AntiCheatCommon.LockObject;
			Result result;
			lock (lockObject)
			{
				result = this.antiCheatInterface.EndSession(ref endSessionOptions);
			}
			if (result != Result.Success)
			{
				Log.Error("[EOS-ACS] Stopping module failed: " + result.ToStringCached<Result>());
			}
			this.serverRunning = false;
			this.authSuccessfulDelegate = null;
			this.kickPlayerDelegate = null;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void handleMessageToClient(ref OnMessageToClientCallbackInfo _data)
		{
			ClientInfo clientInfo = AntiCheatCommon.IntPtrToClientInfo(_data.ClientHandle, "[EOS-ACS] Got message for unknown client number: {0}");
			if (clientInfo == null)
			{
				Log.Out(string.Format("[EOS-ACS] FreeUser: {0}", _data.ClientHandle));
				UnregisterClientOptions unregisterClientOptions = new UnregisterClientOptions
				{
					ClientHandle = _data.ClientHandle
				};
				object lockObject = AntiCheatCommon.LockObject;
				Result result;
				lock (lockObject)
				{
					result = this.antiCheatInterface.UnregisterClient(ref unregisterClientOptions);
				}
				if (result != Result.Success)
				{
					Log.Error("[EOS-ACS] Failed unregistering user: " + result.ToStringCached<Result>());
				}
			}
			if (clientInfo != null)
			{
				clientInfo.SendPackage(NetPackageManager.GetPackage<NetPackageEAC>().Setup(_data.MessageData.Count, _data.MessageData.Array));
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void handleClientAction(ref OnClientActionRequiredCallbackInfo _data)
		{
			ClientInfo clientInfo = AntiCheatCommon.IntPtrToClientInfo(_data.ClientHandle, "[EOS-ACS] Got action for unknown client number: {0}");
			if (clientInfo == null)
			{
				return;
			}
			AntiCheatCommonClientAction clientAction = _data.ClientAction;
			AntiCheatCommonClientActionReason actionReasonCode = _data.ActionReasonCode;
			string text = _data.ActionReasonDetailsString;
			if (clientAction != AntiCheatCommonClientAction.RemovePlayer)
			{
				Log.Warning(string.Format("[EOS-ACS] Got invalid action ({0}), reason='{1}', details={2}, client={3}", new object[]
				{
					clientAction.ToStringCached<AntiCheatCommonClientAction>(),
					actionReasonCode.ToStringCached<AntiCheatCommonClientActionReason>(),
					text,
					clientInfo
				}));
				return;
			}
			Log.Out(string.Format("[EOS-ACS] Kicking player. Reason={0}, details='{1}', client={2}", actionReasonCode.ToStringCached<AntiCheatCommonClientActionReason>(), text, clientInfo));
			KickPlayerDelegate kickPlayerDelegate = this.kickPlayerDelegate;
			if (kickPlayerDelegate == null)
			{
				return;
			}
			ClientInfo cInfo = clientInfo;
			GameUtils.EKickReason kickReason = GameUtils.EKickReason.EosEacViolation;
			int apiResponseEnum = (int)actionReasonCode;
			string customReason = text;
			kickPlayerDelegate(cInfo, new GameUtils.KickPlayerData(kickReason, apiResponseEnum, default(DateTime), customReason));
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void handleClientAuthStateChange(ref OnClientAuthStatusChangedCallbackInfo _data)
		{
			ClientInfo cInfo = AntiCheatCommon.IntPtrToClientInfo(_data.ClientHandle, "[EOS-ACS] Got auth state change for unknown client number: {0}");
			if (_data.ClientAuthStatus == AntiCheatCommonClientAuthStatus.RemoteAuthComplete)
			{
				AuthenticationSuccessfulCallbackDelegate authenticationSuccessfulCallbackDelegate = this.authSuccessfulDelegate;
				if (authenticationSuccessfulCallbackDelegate == null)
				{
					return;
				}
				authenticationSuccessfulCallbackDelegate(cInfo);
			}
		}

		public void Destroy()
		{
		}

		public bool ServerEacEnabled()
		{
			return this.antiCheatInterface != null && GamePrefs.GetBool(EnumGamePrefs.EACEnabled);
		}

		public bool ServerEacAvailable()
		{
			return this.antiCheatInterface != null;
		}

		public bool EncryptionAvailable()
		{
			return this.ServerEacEnabled();
		}

		public bool EncryptStream(ClientInfo _cInfo, MemoryStream _stream, long _startOffset)
		{
			int num = (int)_stream.Length;
			_stream.SetLength((long)(num + 40));
			ArraySegment<byte> data = new ArraySegment<byte>(_stream.GetBuffer(), 0, num);
			ProtectMessageOptions protectMessageOptions = new ProtectMessageOptions
			{
				ClientHandle = AntiCheatCommon.ClientInfoToIntPtr(_cInfo),
				Data = data,
				OutBufferSizeBytes = (uint)(num + 40)
			};
			byte[] array = MemoryPools.poolByte.Alloc(num + 40);
			ArraySegment<byte> outBuffer = new ArraySegment<byte>(array);
			object lockObject = AntiCheatCommon.LockObject;
			uint num2;
			Result result;
			lock (lockObject)
			{
				result = this.antiCheatInterface.ProtectMessage(ref protectMessageOptions, outBuffer, out num2);
			}
			_stream.SetLength(0L);
			_stream.Write(array, 0, (int)num2);
			_stream.Position = 0L;
			MemoryPools.poolByte.Free(array);
			if (result != Result.Success)
			{
				Log.Error(string.Format("[EOS-ACS] Failed encrypting stream for {0}: {1}", _cInfo.InternalId, result.ToStringCached<Result>()));
				return false;
			}
			if (AntiCheatCommon.DebugEacVerbose)
			{
				Log.Out(string.Format("[EOS-ACS] Encrypted. Orig stream len={0}, result len={1}", num, num2));
			}
			_stream.SetLength((long)((ulong)num2));
			return true;
		}

		public bool DecryptStream(ClientInfo _cInfo, MemoryStream _stream, long _startOffset)
		{
			int num = (int)_stream.Length;
			ArraySegment<byte> data = new ArraySegment<byte>(_stream.GetBuffer(), 0, num);
			UnprotectMessageOptions unprotectMessageOptions = new UnprotectMessageOptions
			{
				ClientHandle = AntiCheatCommon.ClientInfoToIntPtr(_cInfo),
				Data = data,
				OutBufferSizeBytes = (uint)num
			};
			byte[] array = MemoryPools.poolByte.Alloc(num);
			ArraySegment<byte> outBuffer = new ArraySegment<byte>(array);
			object lockObject = AntiCheatCommon.LockObject;
			uint num2;
			Result result;
			lock (lockObject)
			{
				result = this.antiCheatInterface.UnprotectMessage(ref unprotectMessageOptions, outBuffer, out num2);
			}
			_stream.SetLength(0L);
			try
			{
				_stream.Write(array, 0, (int)num2);
			}
			catch (Exception e)
			{
				Log.Exception(e);
			}
			_stream.Position = 0L;
			MemoryPools.poolByte.Free(array);
			if (result != Result.Success)
			{
				Log.Error(string.Format("[EOS-ACS] Failed decrypting stream from {0}: {1}", _cInfo.InternalId, result.ToStringCached<Result>()));
				return false;
			}
			if (AntiCheatCommon.DebugEacVerbose)
			{
				Log.Out(string.Format("[EOS-ACS] Decrypted. Orig stream len={0}, result len={1}", num, num2));
			}
			_stream.SetLength((long)((ulong)num2));
			return true;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public IPlatform owner;

		[PublicizedFrom(EAccessModifier.Private)]
		public AntiCheatServerInterface antiCheatInterface;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool serverRunning;

		[PublicizedFrom(EAccessModifier.Private)]
		public AuthenticationSuccessfulCallbackDelegate authSuccessfulDelegate;

		[PublicizedFrom(EAccessModifier.Private)]
		public KickPlayerDelegate kickPlayerDelegate;
	}
}
