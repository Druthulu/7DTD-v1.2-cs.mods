using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;
using Epic.OnlineServices;
using Epic.OnlineServices.AntiCheatCommon;

namespace Platform.EOS
{
	public static class EosHelpers
	{
		[PublicizedFrom(EAccessModifier.Private)]
		static EosHelpers()
		{
			EnumDictionary<EPlatformIdentifier, ExternalAccountType> enumDictionary = new EnumDictionary<EPlatformIdentifier, ExternalAccountType>();
			foreach (KeyValuePair<ExternalAccountType, EPlatformIdentifier> keyValuePair in EosHelpers.accountTypeMappings)
			{
				enumDictionary.Add(keyValuePair.Value, keyValuePair.Key);
			}
			EosHelpers.PlatformIdentifierMappings = new ReadOnlyDictionary<EPlatformIdentifier, ExternalAccountType>(enumDictionary);
		}

		public static void TestEosConnection(Action<bool> _callback)
		{
			ThreadManager.StartThread("TestEosConnection", new ThreadManager.ThreadFunctionDelegate(EosHelpers.<TestEosConnection>g__workerFunc|9_0), ThreadPriority.BelowNormal, new EosHelpers.EosConnectionTestInfo
			{
				Callback = _callback
			}, null, false, true);
		}

		public static void AssertMainThread(string _id)
		{
			if (!ThreadManager.IsMainThread())
			{
				Log.Warning("[EOSH] Called EOS code from secondary thread: " + _id);
			}
		}

		public static ClientInfo.EDeviceType GetDeviceTypeFromPlatform(string platform)
		{
			if (platform == "other" || platform == "steam")
			{
				return ClientInfo.EDeviceType.Unknown;
			}
			if (platform == "playstation")
			{
				return ClientInfo.EDeviceType.PlayStation;
			}
			if (!(platform == "xbox"))
			{
				Log.Error("[EOS] [Auth] GetDeviceTypeFromPlatform: Unknown platform: " + platform);
				return ClientInfo.EDeviceType.Unknown;
			}
			return ClientInfo.EDeviceType.Xbox;
		}

		public static bool RequiresAntiCheat(this ClientInfo.EDeviceType deviceType)
		{
			return deviceType != ClientInfo.EDeviceType.PlayStation && deviceType != ClientInfo.EDeviceType.Xbox;
		}

		[CompilerGenerated]
		[PublicizedFrom(EAccessModifier.Internal)]
		public static void <TestEosConnection>g__workerFunc|9_0(ThreadManager.ThreadInfo _info)
		{
			try
			{
				HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create("https://api.epicgames.dev/sdk/v1/default");
				httpWebRequest.Timeout = 5000;
				httpWebRequest.KeepAlive = false;
				using ((HttpWebResponse)httpWebRequest.GetResponse())
				{
					((EosHelpers.EosConnectionTestInfo)_info.parameter).Result = true;
				}
			}
			catch (Exception ex)
			{
				Log.Out("[EOS] Connection test failed: " + ex.Message);
				((EosHelpers.EosConnectionTestInfo)_info.parameter).Result = false;
			}
			ThreadManager.AddSingleTaskMainThread("TestEosConnectionResult", new ThreadManager.MainThreadTaskFunctionDelegate(EosHelpers.<TestEosConnection>g__mainThreadSyncFunc|9_1), _info.parameter);
		}

		[CompilerGenerated]
		[PublicizedFrom(EAccessModifier.Internal)]
		public static void <TestEosConnection>g__mainThreadSyncFunc|9_1(object _parameter)
		{
			EosHelpers.EosConnectionTestInfo eosConnectionTestInfo = (EosHelpers.EosConnectionTestInfo)_parameter;
			eosConnectionTestInfo.Callback(eosConnectionTestInfo.Result);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly Dictionary<ExternalAccountType, EPlatformIdentifier> accountTypeMappings = new EnumDictionary<ExternalAccountType, EPlatformIdentifier>
		{
			{
				ExternalAccountType.Epic,
				EPlatformIdentifier.EGS
			},
			{
				ExternalAccountType.Psn,
				EPlatformIdentifier.PSN
			},
			{
				ExternalAccountType.Steam,
				EPlatformIdentifier.Steam
			},
			{
				ExternalAccountType.Xbl,
				EPlatformIdentifier.XBL
			}
		};

		public static readonly ReadOnlyDictionary<ExternalAccountType, EPlatformIdentifier> AccountTypeMappings = new ReadOnlyDictionary<ExternalAccountType, EPlatformIdentifier>(EosHelpers.accountTypeMappings);

		public static readonly ReadOnlyDictionary<EPlatformIdentifier, ExternalAccountType> PlatformIdentifierMappings;

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly Dictionary<ClientInfo.EDeviceType, AntiCheatCommonClientPlatform> deviceTypeToAntiCheatPlatformMappings = new EnumDictionary<ClientInfo.EDeviceType, AntiCheatCommonClientPlatform>
		{
			{
				ClientInfo.EDeviceType.Unknown,
				AntiCheatCommonClientPlatform.Unknown
			},
			{
				ClientInfo.EDeviceType.Linux,
				AntiCheatCommonClientPlatform.Linux
			},
			{
				ClientInfo.EDeviceType.Mac,
				AntiCheatCommonClientPlatform.Mac
			},
			{
				ClientInfo.EDeviceType.Windows,
				AntiCheatCommonClientPlatform.Windows
			},
			{
				ClientInfo.EDeviceType.PlayStation,
				AntiCheatCommonClientPlatform.PlayStation
			},
			{
				ClientInfo.EDeviceType.Xbox,
				AntiCheatCommonClientPlatform.Xbox
			}
		};

		public static readonly ReadOnlyDictionary<ClientInfo.EDeviceType, AntiCheatCommonClientPlatform> DeviceTypeToAntiCheatPlatformMappings = new ReadOnlyDictionary<ClientInfo.EDeviceType, AntiCheatCommonClientPlatform>(EosHelpers.deviceTypeToAntiCheatPlatformMappings);

		[PublicizedFrom(EAccessModifier.Private)]
		public const string eosApiUrl = "https://api.epicgames.dev/sdk/v1/default";

		[PublicizedFrom(EAccessModifier.Private)]
		public const int eosApiTestTimeout = 5000;

		[PublicizedFrom(EAccessModifier.Private)]
		public class EosConnectionTestInfo
		{
			public Action<bool> Callback;

			public bool Result;
		}
	}
}
