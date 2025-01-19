using System;
using System.Collections.Generic;
using Epic.OnlineServices;
using Epic.OnlineServices.PlayerDataStorage;

namespace Platform.EOS
{
	public class RemotePlayerFileStorage : IRemotePlayerFileStorage
	{
		public bool IsReady
		{
			get
			{
				IPlatform platform = this.owner;
				if (platform == null)
				{
					return false;
				}
				IUserClient user = platform.User;
				EUserStatus? euserStatus = (user != null) ? new EUserStatus?(user.UserStatus) : null;
				EUserStatus euserStatus2 = EUserStatus.LoggedIn;
				return euserStatus.GetValueOrDefault() == euserStatus2 & euserStatus != null;
			}
		}

		public void Init(IPlatform _owner)
		{
			this.owner = _owner;
			this.owner.Api.ClientApiInitialized += this.OnClientApiInitialized;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void OnClientApiInitialized()
		{
			this.playerDataStorage = ((Api)this.owner.Api).PlatformInterface.GetPlayerDataStorageInterface();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void ProcessNextOperation()
		{
			object obj = this.queueLock;
			lock (obj)
			{
				if (!this.queueProcessing)
				{
					RemotePlayerFileStorage.StorageOperation storageOperation;
					if (this.operationQueue.TryDequeue(out storageOperation))
					{
						this.queueProcessing = true;
						if (storageOperation.ToRead)
						{
							this.ProcessReadOperation(storageOperation.Filename, storageOperation.OverwriteCache, storageOperation.ReadCallback);
						}
						else
						{
							this.ProcessWriteOperation(storageOperation.Filename, storageOperation.Data, storageOperation.OverwriteCache, storageOperation.WriteCallback);
						}
					}
				}
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void CompleteActiveOperation()
		{
			object obj = this.queueLock;
			lock (obj)
			{
				this.queueProcessing = false;
				this.ProcessNextOperation();
			}
		}

		public void ReadRemoteData(string _filename, bool _overwriteCache, IRemotePlayerFileStorage.FileReadCompleteCallback _callback)
		{
			object obj = this.queueLock;
			lock (obj)
			{
				this.operationQueue.Enqueue(new RemotePlayerFileStorage.StorageOperation(_filename, _overwriteCache, _callback));
				this.ProcessNextOperation();
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void ProcessReadOperation(string _filename, bool _overwriteCache, IRemotePlayerFileStorage.FileReadCompleteCallback _callback)
		{
			if (_callback == null)
			{
				Log.Warning("[EOS] PlayerDataStorage Read Operation failed as no callback supplied.");
				this.CompleteActiveOperation();
				return;
			}
			if (!this.IsReady)
			{
				Log.Warning("[EOS] Tried to read from PlayerDataStorage user is not logged in.");
				_callback(IRemotePlayerFileStorage.CallbackResult.NoConnection, null);
				this.CompleteActiveOperation();
				return;
			}
			if (this.playerDataStorage == null)
			{
				Log.Warning("[EOS] Tried to read from PlayerDataStorage but it was null.");
				_callback(IRemotePlayerFileStorage.CallbackResult.NoConnection, null);
				this.CompleteActiveOperation();
				return;
			}
			if (string.IsNullOrEmpty(_filename))
			{
				Log.Warning("[EOS] Supplied filename was null or empty.");
				_callback(IRemotePlayerFileStorage.CallbackResult.FileNotFound, null);
				this.CompleteActiveOperation();
				return;
			}
			ProductUserId productUserId = ((UserIdentifierEos)PlatformManager.CrossplatformPlatform.User.PlatformUserId).ProductUserId;
			RemotePlayerFileStorage.ReadRequestDetails clientData = new RemotePlayerFileStorage.ReadRequestDetails(_callback, _overwriteCache);
			ReadFileOptions readFileOptions = new ReadFileOptions
			{
				Filename = _filename,
				LocalUserId = productUserId,
				ReadChunkLengthBytes = 524288U,
				ReadFileDataCallback = new OnReadFileDataCallback(this.ReadChunkCallback)
			};
			object lockObject = AntiCheatCommon.LockObject;
			lock (lockObject)
			{
				this.playerDataStorage.ReadFile(ref readFileOptions, clientData, new OnReadFileCompleteCallback(this.ReadFileCompleteCallback));
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public ReadResult ReadChunkCallback(ref ReadFileDataCallbackInfo _callbackData)
		{
			((RemotePlayerFileStorage.ReadRequestDetails)_callbackData.ClientData).Chunks.Add(_callbackData.DataChunk);
			return ReadResult.ContinueReading;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void ReadFileCompleteCallback(ref ReadFileCallbackInfo _callbackData)
		{
			try
			{
				RemotePlayerFileStorage.ReadRequestDetails readRequestDetails = (RemotePlayerFileStorage.ReadRequestDetails)_callbackData.ClientData;
				if (_callbackData.ResultCode != Result.Success)
				{
					if (_callbackData.ResultCode == Result.NotFound)
					{
						readRequestDetails.Callback(IRemotePlayerFileStorage.CallbackResult.FileNotFound, null);
					}
					else if (_callbackData.ResultCode != Result.OperationWillRetry)
					{
						Log.Warning(string.Format("[EOS] Read from PlayerDataStorage failed ({0}): {1}", _callbackData.Filename, _callbackData.ResultCode.ToStringCached<Result>()));
						readRequestDetails.Callback(IRemotePlayerFileStorage.CallbackResult.Other, null);
					}
				}
				else
				{
					int num = 0;
					foreach (ArraySegment<byte> arraySegment in readRequestDetails.Chunks)
					{
						num += arraySegment.Count;
					}
					byte[] array = new byte[num];
					int num2 = 0;
					for (int i = 0; i < readRequestDetails.Chunks.Count; i++)
					{
						Array.Copy(readRequestDetails.Chunks[i].Array, readRequestDetails.Chunks[i].Offset, array, num2, readRequestDetails.Chunks[i].Count);
						num2 += readRequestDetails.Chunks[i].Count;
					}
					if (readRequestDetails.OverwriteCache)
					{
						IRemotePlayerFileStorage.WriteCachedObject(this.owner.User, _callbackData.Filename, array);
					}
					Log.Out(string.Format("[EOS] Read ({0}) completed: {1}, received {2} bytes", _callbackData.Filename, _callbackData.ResultCode, num));
					readRequestDetails.Callback(IRemotePlayerFileStorage.CallbackResult.Success, array);
				}
			}
			finally
			{
				this.CompleteActiveOperation();
			}
		}

		public void WriteRemoteData(string _filename, byte[] _data, bool _overwriteCache, IRemotePlayerFileStorage.FileWriteCompleteCallback _callback)
		{
			object obj = this.queueLock;
			lock (obj)
			{
				this.operationQueue.Enqueue(new RemotePlayerFileStorage.StorageOperation(_filename, _data, _overwriteCache, _callback));
				this.ProcessNextOperation();
			}
		}

		public void ProcessWriteOperation(string _filename, byte[] _data, bool _overwriteCache, IRemotePlayerFileStorage.FileWriteCompleteCallback _callback)
		{
			if (_callback == null)
			{
				Log.Warning("[EOS] PlayerDataStorage Write Operation failed as no callback supplied.");
				this.CompleteActiveOperation();
				return;
			}
			if (!this.IsReady)
			{
				Log.Warning("[EOS] Tried to write to PlayerDataStorage user is not logged in.");
				this.CompleteActiveOperation();
				_callback(IRemotePlayerFileStorage.CallbackResult.NoConnection);
				return;
			}
			if (this.playerDataStorage == null)
			{
				Log.Warning("[EOS] Tried to write to PlayerDataStorage but it was null.");
				this.CompleteActiveOperation();
				_callback(IRemotePlayerFileStorage.CallbackResult.NoConnection);
				return;
			}
			if (string.IsNullOrEmpty(_filename))
			{
				Log.Warning("[EOS] Supplied filename was null or empty.");
				this.CompleteActiveOperation();
				_callback(IRemotePlayerFileStorage.CallbackResult.FileNotFound);
				return;
			}
			if (_data == null)
			{
				Log.Warning("[EOS] Supplied data to store was null.");
				this.CompleteActiveOperation();
				_callback(IRemotePlayerFileStorage.CallbackResult.MalformedData);
				return;
			}
			RemotePlayerFileStorage.WriteRequestDetails clientData = new RemotePlayerFileStorage.WriteRequestDetails(_overwriteCache, _data, _callback);
			ProductUserId productUserId = ((UserIdentifierEos)PlatformManager.CrossplatformPlatform.User.PlatformUserId).ProductUserId;
			WriteFileOptions writeFileOptions = new WriteFileOptions
			{
				Filename = _filename,
				LocalUserId = productUserId,
				ChunkLengthBytes = 524288U,
				WriteFileDataCallback = new OnWriteFileDataCallback(this.WriteFileChunkCallback)
			};
			object lockObject = AntiCheatCommon.LockObject;
			lock (lockObject)
			{
				this.playerDataStorage.WriteFile(ref writeFileOptions, clientData, new OnWriteFileCompleteCallback(this.WriteFileCompleteCallback));
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public WriteResult WriteFileChunkCallback(ref WriteFileDataCallbackInfo _callbackData, out ArraySegment<byte> _outDataBuffer)
		{
			RemotePlayerFileStorage.WriteRequestDetails writeRequestDetails = (RemotePlayerFileStorage.WriteRequestDetails)_callbackData.ClientData;
			_outDataBuffer = (writeRequestDetails.GetNextChunk() ?? null);
			if (writeRequestDetails.HasNextChunk())
			{
				return WriteResult.ContinueWriting;
			}
			return WriteResult.CompleteRequest;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void WriteFileCompleteCallback(ref WriteFileCallbackInfo _callbackData)
		{
			try
			{
				RemotePlayerFileStorage.WriteRequestDetails writeRequestDetails = (RemotePlayerFileStorage.WriteRequestDetails)_callbackData.ClientData;
				IRemotePlayerFileStorage.CallbackResult result = IRemotePlayerFileStorage.CallbackResult.Success;
				if (_callbackData.ResultCode != Result.Success && _callbackData.ResultCode != Result.OperationWillRetry)
				{
					Log.Warning(string.Format("[EOS] Write to PlayerDataStorage failed ({0}): {1}", _callbackData.Filename, _callbackData.ResultCode.ToStringCached<Result>()));
					result = IRemotePlayerFileStorage.CallbackResult.Other;
				}
				if (writeRequestDetails.WriteToCache && !IRemotePlayerFileStorage.WriteCachedObject(this.owner.User, _callbackData.Filename, writeRequestDetails.Data))
				{
					Log.Warning(string.Format("[EOS] Write to PlayerDataStorage succeeded ({0}), but failed while saving to local cache.", _callbackData.Filename));
				}
				IRemotePlayerFileStorage.FileWriteCompleteCallback callback = writeRequestDetails.Callback;
				if (callback != null)
				{
					callback(result);
				}
			}
			finally
			{
				this.CompleteActiveOperation();
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public const int cReadWriteByteLimit = 524288;

		[PublicizedFrom(EAccessModifier.Private)]
		public IPlatform owner;

		[PublicizedFrom(EAccessModifier.Private)]
		public PlayerDataStorageInterface playerDataStorage;

		[PublicizedFrom(EAccessModifier.Private)]
		public Queue<RemotePlayerFileStorage.StorageOperation> operationQueue = new Queue<RemotePlayerFileStorage.StorageOperation>();

		[PublicizedFrom(EAccessModifier.Private)]
		public object queueLock = new object();

		[PublicizedFrom(EAccessModifier.Private)]
		public bool queueProcessing;

		[PublicizedFrom(EAccessModifier.Private)]
		public struct StorageOperation
		{
			public StorageOperation(string _filename, bool _overwriteCache, IRemotePlayerFileStorage.FileReadCompleteCallback _callback)
			{
				this.ToRead = true;
				this.Filename = _filename;
				this.OverwriteCache = _overwriteCache;
				this.Data = null;
				this.ReadCallback = _callback;
				this.WriteCallback = null;
			}

			public StorageOperation(string _filename, byte[] _data, bool _overwriteCache, IRemotePlayerFileStorage.FileWriteCompleteCallback _callback)
			{
				this.ToRead = false;
				this.Filename = _filename;
				this.Data = _data;
				this.OverwriteCache = _overwriteCache;
				this.ReadCallback = null;
				this.WriteCallback = _callback;
			}

			public bool ToRead;

			public string Filename;

			public bool OverwriteCache;

			public byte[] Data;

			public IRemotePlayerFileStorage.FileReadCompleteCallback ReadCallback;

			public IRemotePlayerFileStorage.FileWriteCompleteCallback WriteCallback;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public class ReadRequestDetails
		{
			public ReadRequestDetails(IRemotePlayerFileStorage.FileReadCompleteCallback _callback, bool _overwriteCache)
			{
				this.Callback = _callback;
				this.OverwriteCache = _overwriteCache;
				this.Chunks = new List<ArraySegment<byte>>();
			}

			public readonly IRemotePlayerFileStorage.FileReadCompleteCallback Callback;

			public List<ArraySegment<byte>> Chunks;

			public bool OverwriteCache;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public class WriteRequestDetails
		{
			public WriteRequestDetails(bool _writeToCache, byte[] _data, IRemotePlayerFileStorage.FileWriteCompleteCallback _callback)
			{
				this.WriteToCache = _writeToCache;
				this.Data = _data;
				this.Callback = _callback;
			}

			public ArraySegment<byte>? GetNextChunk()
			{
				if (this.DataPointer >= this.Data.Length)
				{
					return null;
				}
				int val = this.Data.Length - this.DataPointer;
				int num = Math.Min(524288, val);
				ArraySegment<byte> value = new ArraySegment<byte>(this.Data, this.DataPointer, num);
				this.DataPointer += num;
				return new ArraySegment<byte>?(value);
			}

			public bool HasNextChunk()
			{
				return this.DataPointer < this.Data.Length;
			}

			public bool WriteToCache;

			public byte[] Data;

			public readonly IRemotePlayerFileStorage.FileWriteCompleteCallback Callback;

			[PublicizedFrom(EAccessModifier.Private)]
			public int DataPointer;
		}
	}
}
