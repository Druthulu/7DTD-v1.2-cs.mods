using System;
using System.IO;
using Epic.OnlineServices;
using Epic.OnlineServices.AntiCheatClient;

namespace Platform.EOS
{
	public class AntiCheatClientCS
	{
		public AntiCheatClientCS(IPlatform _owner, AntiCheatClientInterface _antiCheatInterface)
		{
			this.owner = _owner;
			this.antiCheatInterface = _antiCheatInterface;
		}

		public void Activate()
		{
			AddNotifyMessageToServerOptions addNotifyMessageToServerOptions = default(AddNotifyMessageToServerOptions);
			object lockObject = AntiCheatCommon.LockObject;
			lock (lockObject)
			{
				this.handleMessageToServerID = this.antiCheatInterface.AddNotifyMessageToServer(ref addNotifyMessageToServerOptions, null, new OnMessageToServerCallback(this.handleMessageToServer));
			}
		}

		public void Deactivate()
		{
			if (this.handleMessageToServerID != 0UL)
			{
				this.antiCheatInterface.RemoveNotifyMessageToServer(this.handleMessageToServerID);
				this.handleMessageToServerID = 0UL;
			}
		}

		public void ConnectToServer(Action _onNoAntiCheatOrConnectionComplete, Action<string> _onConnectionFailed)
		{
			ProductUserId productUserId = ((UserIdentifierEos)this.owner.User.PlatformUserId).ProductUserId;
			BeginSessionOptions beginSessionOptions = new BeginSessionOptions
			{
				LocalUserId = productUserId,
				Mode = AntiCheatClientMode.ClientServer
			};
			object lockObject = AntiCheatCommon.LockObject;
			Result result;
			lock (lockObject)
			{
				result = this.antiCheatInterface.BeginSession(ref beginSessionOptions);
			}
			if (result != Result.Success)
			{
				Log.Error("[EOS-ACC] Begin session failed: " + result.ToStringCached<Result>());
				if (_onConnectionFailed != null)
				{
					_onConnectionFailed(result.ToStringCached<Result>());
				}
			}
			Log.Out("[EOS-ACC] Connected to game server");
			if (_onNoAntiCheatOrConnectionComplete != null)
			{
				_onNoAntiCheatOrConnectionComplete();
			}
		}

		public void HandleMessageFromServer(byte[] _data)
		{
			if (AntiCheatCommon.DebugEacVerbose)
			{
				Log.Out(string.Format("[EOS-ACC] PushNetworkMessage (len={0})", _data.Length));
			}
			EosHelpers.AssertMainThread("ACC.HMFS");
			ReceiveMessageFromServerOptions receiveMessageFromServerOptions = new ReceiveMessageFromServerOptions
			{
				Data = new ArraySegment<byte>(_data)
			};
			object lockObject = AntiCheatCommon.LockObject;
			Result result;
			lock (lockObject)
			{
				result = this.antiCheatInterface.ReceiveMessageFromServer(ref receiveMessageFromServerOptions);
			}
			if (result != Result.Success)
			{
				Log.Error("[EOS-ACC] Failed handling message: " + result.ToStringCached<Result>());
			}
		}

		public void DisconnectFromServer()
		{
			Log.Out("[EOS-ACC] Disconnected from game server");
			EosHelpers.AssertMainThread("ACC.Disc");
			EndSessionOptions endSessionOptions = default(EndSessionOptions);
			object lockObject = AntiCheatCommon.LockObject;
			Result result;
			lock (lockObject)
			{
				result = this.antiCheatInterface.EndSession(ref endSessionOptions);
			}
			if (result != Result.Success)
			{
				Log.Error("[EOS-ACC] Stopping module failed: " + result.ToStringCached<Result>());
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void handleMessageToServer(ref OnMessageToServerCallbackInfo _data)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageEAC>().Setup(_data.MessageData.Count, _data.MessageData.Array), false);
		}

		public bool EncryptStream(ClientInfo _cInfo, MemoryStream _stream, long _startOffset)
		{
			int num = (int)_stream.Length;
			_stream.SetLength((long)(num + 40));
			ArraySegment<byte> data = new ArraySegment<byte>(_stream.GetBuffer(), 0, num);
			ProtectMessageOptions protectMessageOptions = new ProtectMessageOptions
			{
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
				Log.Error("[EOS-ACC] Failed encrypting stream: " + result.ToStringCached<Result>());
				return false;
			}
			if (AntiCheatCommon.DebugEacVerbose)
			{
				Log.Out(string.Format("[EOS-ACC] Encrypted. Orig stream len={0}, result len={1}", num, num2));
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
				Log.Error("[EOS-ACC] Failed decrypting stream: " + result.ToStringCached<Result>());
				return false;
			}
			if (AntiCheatCommon.DebugEacVerbose)
			{
				Log.Out(string.Format("[EOS-ACC] Decrypted. Orig stream len={0}, result len={1}", num, num2));
			}
			_stream.SetLength((long)((ulong)num2));
			return true;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public IPlatform owner;

		[PublicizedFrom(EAccessModifier.Private)]
		public AntiCheatClientInterface antiCheatInterface;

		[PublicizedFrom(EAccessModifier.Private)]
		public ulong handleMessageToServerID;
	}
}
