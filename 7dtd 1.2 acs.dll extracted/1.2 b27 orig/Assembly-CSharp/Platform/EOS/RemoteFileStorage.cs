﻿using System;
using System.Collections.Generic;
using System.IO;
using Epic.OnlineServices;
using Epic.OnlineServices.TitleStorage;

namespace Platform.EOS
{
	public class RemoteFileStorage : IRemoteFileStorage
	{
		public void Init(IPlatform _owner)
		{
			this.owner = _owner;
			this.owner.Api.ClientApiInitialized += this.OnClientApiInitialized;
			this.owner.User.UserLoggedIn += this.OnUserLoggedIn;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void OnUserLoggedIn(IPlatform _obj)
		{
			if (this.IsReady)
			{
				this.clearCache();
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void OnClientApiInitialized()
		{
			this.titleStorageInterface = ((Api)this.owner.Api).PlatformInterface.GetTitleStorageInterface();
		}

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

		public bool Unavailable
		{
			get
			{
				IPlatform platform = this.owner;
				EUserStatus? euserStatus;
				if (platform == null)
				{
					euserStatus = null;
				}
				else
				{
					IUserClient user = platform.User;
					euserStatus = ((user != null) ? new EUserStatus?(user.UserStatus) : null);
				}
				EUserStatus? euserStatus2 = euserStatus;
				if (euserStatus2 != null)
				{
					EUserStatus valueOrDefault = euserStatus2.GetValueOrDefault();
					if (valueOrDefault == EUserStatus.OfflineMode || valueOrDefault - EUserStatus.PermanentError <= 1)
					{
						return true;
					}
				}
				return false;
			}
		}

		public void GetFile(string _filename, IRemoteFileStorage.FileDownloadCompleteCallback _callback)
		{
			if (_callback == null)
			{
				return;
			}
			if (string.IsNullOrEmpty(_filename))
			{
				_callback(IRemoteFileStorage.EFileDownloadResult.EmptyFilename, null, null);
				return;
			}
			ProductUserId productUserId = ((UserIdentifierEos)PlatformManager.CrossplatformPlatform.User.PlatformUserId).ProductUserId;
			object obj = this.requestsLock;
			RemoteFileStorage.RequestDetails requestDetails;
			bool flag2;
			lock (obj)
			{
				flag2 = !this.requests.TryGetValue(_filename, out requestDetails);
				if (flag2)
				{
					Log.Out("[EOS] Created RFS Request: " + _filename);
					requestDetails = new RemoteFileStorage.RequestDetails(_filename, productUserId, _callback);
					this.requests.Add(_filename, requestDetails);
				}
				else
				{
					Log.Out("[EOS] Adding callback to existing RFS Request: " + _filename);
					requestDetails.Callback += _callback;
				}
			}
			if (!flag2)
			{
				return;
			}
			EosHelpers.AssertMainThread("RFS.Get");
			this.getMetadata(requestDetails);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void getMetadata(RemoteFileStorage.RequestDetails _details)
		{
			QueryFileOptions queryFileOptions = new QueryFileOptions
			{
				Filename = _details.Filename,
				LocalUserId = _details.LocalUserId
			};
			object lockObject = AntiCheatCommon.LockObject;
			lock (lockObject)
			{
				this.titleStorageInterface.QueryFile(ref queryFileOptions, _details, new OnQueryFileCompleteCallback(this.queryFileCallback));
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void queryFileCallback(ref QueryFileCallbackInfo _callbackData)
		{
			RemoteFileStorage.RequestDetails requestDetails = (RemoteFileStorage.RequestDetails)_callbackData.ClientData;
			if (_callbackData.ResultCode != Result.Success)
			{
				Log.Error("[EOS] QueryFile (" + requestDetails.Filename + ") failed: " + _callbackData.ResultCode.ToStringCached<Result>());
				this.CompleteRequest(requestDetails, IRemoteFileStorage.EFileDownloadResult.FileNotFound, _callbackData.ResultCode.ToStringCached<Result>(), null);
				return;
			}
			CopyFileMetadataByFilenameOptions copyFileMetadataByFilenameOptions = new CopyFileMetadataByFilenameOptions
			{
				Filename = requestDetails.Filename,
				LocalUserId = requestDetails.LocalUserId
			};
			object lockObject = AntiCheatCommon.LockObject;
			Result result;
			lock (lockObject)
			{
				FileMetadata? fileMetadata;
				result = this.titleStorageInterface.CopyFileMetadataByFilename(ref copyFileMetadataByFilenameOptions, out fileMetadata);
			}
			if (result != Result.Success)
			{
				Log.Error("[EOS] CopyFileMetadataByFilename (" + requestDetails.Filename + ") failed: " + result.ToStringCached<Result>());
				this.CompleteRequest(requestDetails, IRemoteFileStorage.EFileDownloadResult.Other, _callbackData.ResultCode.ToStringCached<Result>(), null);
				return;
			}
			this.readFile(requestDetails);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void readFile(RemoteFileStorage.RequestDetails _details)
		{
			ReadFileOptions readFileOptions = new ReadFileOptions
			{
				Filename = _details.Filename,
				LocalUserId = _details.LocalUserId,
				ReadFileDataCallback = new OnReadFileDataCallback(this.readFileDataCallback),
				ReadChunkLengthBytes = 524288U
			};
			object lockObject = AntiCheatCommon.LockObject;
			lock (lockObject)
			{
				this.titleStorageInterface.ReadFile(ref readFileOptions, _details, new OnReadFileCompleteCallback(this.readCompletedCallback));
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void fileTransferProgressCallback(ref FileTransferProgressCallbackInfo _callbackData)
		{
			RemoteFileStorage.RequestDetails requestDetails = (RemoteFileStorage.RequestDetails)_callbackData.ClientData;
			Log.Out(string.Format("[EOS] TransferProgress: {0}, {1} / {2}", _callbackData.Filename, _callbackData.BytesTransferred, _callbackData.TotalFileSizeBytes));
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public ReadResult readFileDataCallback(ref ReadFileDataCallbackInfo _callbackData)
		{
			((RemoteFileStorage.RequestDetails)_callbackData.ClientData).Chunks.Add(_callbackData.DataChunk);
			return ReadResult.RrContinuereading;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void readCompletedCallback(ref ReadFileCallbackInfo _callbackData)
		{
			RemoteFileStorage.RequestDetails requestDetails = (RemoteFileStorage.RequestDetails)_callbackData.ClientData;
			if (_callbackData.ResultCode != Result.Success)
			{
				Log.Error("[EOS] Read (" + requestDetails.Filename + ") failed: " + _callbackData.ResultCode.ToStringCached<Result>());
				if (_callbackData.ResultCode != Result.OperationWillRetry)
				{
					this.CompleteRequest(requestDetails, IRemoteFileStorage.EFileDownloadResult.Other, _callbackData.ResultCode.ToStringCached<Result>(), null);
				}
				return;
			}
			int num = 0;
			foreach (ArraySegment<byte> arraySegment in requestDetails.Chunks)
			{
				num += arraySegment.Count;
			}
			byte[] array = new byte[num];
			int num2 = 0;
			for (int i = 0; i < requestDetails.Chunks.Count; i++)
			{
				Array.Copy(requestDetails.Chunks[i].Array, requestDetails.Chunks[i].Offset, array, num2, requestDetails.Chunks[i].Count);
				num2 += requestDetails.Chunks[i].Count;
			}
			Log.Out(string.Format("[EOS] Read ({0}) completed: {1}, received {2} bytes", _callbackData.Filename, _callbackData.ResultCode, num));
			this.cacheFile(requestDetails.Filename, array);
			this.CompleteRequest(requestDetails, IRemoteFileStorage.EFileDownloadResult.Ok, null, array);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void CompleteRequest(RemoteFileStorage.RequestDetails _details, IRemoteFileStorage.EFileDownloadResult _result, string _errorName, byte[] _data)
		{
			object obj = this.requestsLock;
			lock (obj)
			{
				if (!this.requests.Remove(_details.Filename))
				{
					Log.Warning("[EOS] Unexpected RFS request being completed: " + _details.Filename);
				}
			}
			_details.ExecuteCallback(_result, _errorName, _data);
		}

		public string CacheFilePrefix
		{
			[PublicizedFrom(EAccessModifier.Private)]
			get
			{
				return "Rfs_";
			}
		}

		public string CacheFolder
		{
			[PublicizedFrom(EAccessModifier.Private)]
			get
			{
				return GameIO.GetUserGameDataDir() + "/RfsCache";
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void clearCache()
		{
			if (!SdDirectory.Exists(this.CacheFolder))
			{
				return;
			}
			foreach (string path in SdDirectory.GetFiles(this.CacheFolder))
			{
				if (Path.GetFileName(path).StartsWith(this.CacheFilePrefix))
				{
					SdFile.Delete(path);
				}
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void cacheFile(string _filename, byte[] _data)
		{
			SdDirectory.CreateDirectory(this.CacheFolder);
			SdFile.WriteAllBytes(this.CacheFolder + "/" + this.CacheFilePrefix + _filename, _data);
		}

		public void GetCachedFile(string _filename, IRemoteFileStorage.FileDownloadCompleteCallback _callback)
		{
			if (_callback == null)
			{
				return;
			}
			if (string.IsNullOrEmpty(_filename))
			{
				_callback(IRemoteFileStorage.EFileDownloadResult.EmptyFilename, null, null);
				return;
			}
			string path = this.CacheFolder + "/" + this.CacheFilePrefix + _filename;
			if (!SdFile.Exists(path))
			{
				_callback(IRemoteFileStorage.EFileDownloadResult.FileNotFound, "File not found", null);
				return;
			}
			byte[] array = SdFile.ReadAllBytes(path);
			Log.Out(string.Format("[EOS] Read cached ({0}) completed: {1} bytes", _filename, array.Length));
			_callback(IRemoteFileStorage.EFileDownloadResult.Ok, null, array);
		}

		public const string CacheFolderName = "RfsCache";

		[PublicizedFrom(EAccessModifier.Private)]
		public IPlatform owner;

		[PublicizedFrom(EAccessModifier.Private)]
		public TitleStorageInterface titleStorageInterface;

		[PublicizedFrom(EAccessModifier.Private)]
		public object requestsLock = new object();

		[PublicizedFrom(EAccessModifier.Private)]
		public Dictionary<string, RemoteFileStorage.RequestDetails> requests = new Dictionary<string, RemoteFileStorage.RequestDetails>();

		[PublicizedFrom(EAccessModifier.Private)]
		public class RequestDetails
		{
			public event IRemoteFileStorage.FileDownloadCompleteCallback Callback;

			public RequestDetails(string _filename, ProductUserId _localUserId, IRemoteFileStorage.FileDownloadCompleteCallback _callback)
			{
				this.Filename = _filename;
				this.LocalUserId = _localUserId;
				this.Callback = _callback;
			}

			public void ExecuteCallback(IRemoteFileStorage.EFileDownloadResult _result, string _errorName, byte[] _data)
			{
				IRemoteFileStorage.FileDownloadCompleteCallback callback = this.Callback;
				if (callback == null)
				{
					return;
				}
				callback(_result, _errorName, _data);
			}

			public readonly string Filename;

			public readonly ProductUserId LocalUserId;

			public readonly List<ArraySegment<byte>> Chunks = new List<ArraySegment<byte>>();
		}
	}
}
