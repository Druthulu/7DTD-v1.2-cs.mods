using System;

namespace Platform
{
	public interface IRemotePlayerFileStorage
	{
		void Init(IPlatform _owner);

		void ReadRemoteData(string _filename, bool _overwriteCache, IRemotePlayerFileStorage.FileReadCompleteCallback _callback);

		void WriteRemoteData(string _filename, byte[] _data, bool _overwriteCache, IRemotePlayerFileStorage.FileWriteCompleteCallback _callback);

		void ReadRemoteObject<T>(string _filename, bool _overwriteCache, IRemotePlayerFileStorage.FileReadObjectCompleteCallback<T> _callback) where T : IRemotePlayerStorageObject, new()
		{
			if (_callback == null)
			{
				Log.Error("[RPFS] Read failed as no callback was supplied");
				return;
			}
			this.ReadRemoteData(_filename, _overwriteCache, delegate(IRemotePlayerFileStorage.CallbackResult result, byte[] data)
			{
				if (data == null)
				{
					_callback(result, default(T));
					return;
				}
				T t = IRemotePlayerFileStorage.BytesToObject<T>(data);
				if (t == null)
				{
					Log.Error(string.Format("[RPFS] Reading data into type {0} yields malformed result.", typeof(T)));
					result = IRemotePlayerFileStorage.CallbackResult.MalformedData;
					_callback(result, default(T));
					return;
				}
				_callback(result, t);
			});
		}

		void WriteRemoteObject(string _filename, IRemotePlayerStorageObject _object, bool _overwriteCache, IRemotePlayerFileStorage.FileWriteCompleteCallback _callback)
		{
			if (_callback == null)
			{
				Log.Error("[RPFS] Write failed as no callback was supplied");
				return;
			}
			this.WriteRemoteData(_filename, IRemotePlayerFileStorage.ObjectToBytes(_object), _overwriteCache, _callback);
		}

		public static T BytesToObject<T>(byte[] _data) where T : IRemotePlayerStorageObject, new()
		{
			T result;
			if (_data == null || _data.Length == 0)
			{
				Log.Warning("[RPFS] Byte data was empty or null.");
				result = default(T);
				return result;
			}
			try
			{
				using (PooledExpandableMemoryStream pooledExpandableMemoryStream = MemoryPools.poolMemoryStream.AllocSync(true))
				{
					pooledExpandableMemoryStream.Write(_data, 0, _data.Length);
					pooledExpandableMemoryStream.Position = 0L;
					using (PooledBinaryReader pooledBinaryReader = MemoryPools.poolBinaryReader.AllocSync(true))
					{
						pooledBinaryReader.SetBaseStream(pooledExpandableMemoryStream);
						T t = Activator.CreateInstance<T>();
						t.ReadInto(pooledBinaryReader);
						result = t;
					}
				}
			}
			catch (Exception arg)
			{
				Log.Error(string.Format("[RPFS] Error while reading object from byte data. Error: {0}.", arg));
				result = default(T);
			}
			return result;
		}

		public static byte[] ObjectToBytes(IRemotePlayerStorageObject _obj)
		{
			if (_obj == null)
			{
				Log.Warning("[RPFS] Object was null.");
				return null;
			}
			byte[] result;
			try
			{
				using (PooledExpandableMemoryStream pooledExpandableMemoryStream = MemoryPools.poolMemoryStream.AllocSync(true))
				{
					using (PooledBinaryWriter pooledBinaryWriter = MemoryPools.poolBinaryWriter.AllocSync(true))
					{
						pooledBinaryWriter.SetBaseStream(pooledExpandableMemoryStream);
						_obj.WriteFrom(pooledBinaryWriter);
						pooledBinaryWriter.Flush();
					}
					result = pooledExpandableMemoryStream.ToArray();
				}
			}
			catch (Exception arg)
			{
				Log.Error(string.Format("[RPFS] Error while writing object into byte data. Error: {0}.", arg));
				result = null;
			}
			return result;
		}

		public static string GetCacheFolder(IUserClient _user)
		{
			if (_user == null || _user.UserStatus > EUserStatus.LoggedIn)
			{
				return null;
			}
			return GameIO.GetUserGameDataDir() + "/RfsPlayerCache/" + _user.PlatformUserId.ReadablePlatformUserIdentifier;
		}

		public static T ReadCachedObject<T>(IUserClient _user, string _filename) where T : IRemotePlayerStorageObject, new()
		{
			return IRemotePlayerFileStorage.BytesToObject<T>(IRemotePlayerFileStorage.ReadCachedData(_user, _filename));
		}

		public static byte[] ReadCachedData(IUserClient _user, string _filename)
		{
			object localCacheLock = IRemotePlayerFileStorage.LocalCacheLock;
			byte[] result;
			lock (localCacheLock)
			{
				string path = IRemotePlayerFileStorage.GetCacheFolder(_user) + "/" + _filename;
				if (string.IsNullOrEmpty(_filename) || !SdFile.Exists(path))
				{
					Log.Warning("[RPFS] File path was not found.");
					result = null;
				}
				else
				{
					result = SdFile.ReadAllBytes(path);
				}
			}
			return result;
		}

		public static bool WriteCachedObject(IUserClient _user, string _filename, IRemotePlayerStorageObject _object)
		{
			return IRemotePlayerFileStorage.WriteCachedObject(_user, _filename, IRemotePlayerFileStorage.ObjectToBytes(_object));
		}

		public static bool WriteCachedObject(IUserClient _user, string _filename, byte[] _data)
		{
			object localCacheLock = IRemotePlayerFileStorage.LocalCacheLock;
			bool result;
			lock (localCacheLock)
			{
				try
				{
					if (_data == null)
					{
						Log.Warning("[RPFS] Error while converting object to bytes.");
						result = false;
					}
					else
					{
						SdDirectory.CreateDirectory(IRemotePlayerFileStorage.GetCacheFolder(_user));
						SdFile.WriteAllBytes(IRemotePlayerFileStorage.GetCacheFolder(_user) + "/" + _filename, _data);
						result = true;
					}
				}
				catch (Exception arg)
				{
					Log.Warning(string.Format("[RPFS] Error while writing object to cache. Error: {0}.", arg));
					result = false;
				}
			}
			return result;
		}

		public static void ClearCache(IUserClient _user)
		{
			object localCacheLock = IRemotePlayerFileStorage.LocalCacheLock;
			lock (localCacheLock)
			{
				if (SdDirectory.Exists(IRemotePlayerFileStorage.GetCacheFolder(_user)))
				{
					string[] files = SdDirectory.GetFiles(IRemotePlayerFileStorage.GetCacheFolder(_user));
					for (int i = 0; i < files.Length; i++)
					{
						SdFile.Delete(files[i]);
					}
				}
			}
		}

		public static object LocalCacheLock = new object();

		public enum CallbackResult
		{
			Success,
			FileNotFound,
			NoConnection,
			MalformedData,
			Other
		}

		public delegate void FileReadCompleteCallback(IRemotePlayerFileStorage.CallbackResult _result, byte[] _data);

		public delegate void FileReadObjectCompleteCallback<T>(IRemotePlayerFileStorage.CallbackResult _result, T _object) where T : IRemotePlayerStorageObject;

		public delegate void FileWriteCompleteCallback(IRemotePlayerFileStorage.CallbackResult _result);
	}
}
