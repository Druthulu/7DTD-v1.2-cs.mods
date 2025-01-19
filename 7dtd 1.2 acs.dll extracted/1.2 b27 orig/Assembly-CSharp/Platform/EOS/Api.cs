using System;
using System.IO;
using Epic.OnlineServices;
using Epic.OnlineServices.Connect;
using Epic.OnlineServices.Logging;
using Epic.OnlineServices.Platform;
using Epic.OnlineServices.Sanctions;
using UnityEngine;

namespace Platform.EOS
{
	public class Api : IPlatformApi
	{
		[PublicizedFrom(EAccessModifier.Private)]
		static Api()
		{
			string launchArgument = GameUtils.GetLaunchArgument("debugeos");
			if (launchArgument != null)
			{
				if (launchArgument == "verbose")
				{
					Api.DebugLevel = Api.EDebugLevel.Verbose;
					return;
				}
				Api.DebugLevel = Api.EDebugLevel.Normal;
			}
		}

		public void Init(IPlatform _owner)
		{
			this.owner = _owner;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void OnApplicationStateChanged(ApplicationState _applicationState)
		{
			if (this.PlatformInterface == null)
			{
				return;
			}
			ApplicationStatus applicationStatus;
			if (_applicationState != ApplicationState.Foreground)
			{
				if (_applicationState != ApplicationState.Suspended)
				{
					throw new ArgumentOutOfRangeException("_applicationState", _applicationState, "[EOS] OnApplicationStateChanged: ApplicationState is missing a conversion to a EOS.ApplicationStatus");
				}
				applicationStatus = ApplicationStatus.BackgroundSuspended;
			}
			else
			{
				applicationStatus = ApplicationStatus.Foreground;
			}
			object lockObject = AntiCheatCommon.LockObject;
			lock (lockObject)
			{
				this.PlatformInterface.SetApplicationStatus(applicationStatus);
			}
		}

		public EApiStatus ClientApiStatus { get; [PublicizedFrom(EAccessModifier.Private)] set; } = EApiStatus.Uninitialized;

		public event Action ClientApiInitialized
		{
			add
			{
				lock (this)
				{
					this.clientApiInitialized = (Action)Delegate.Combine(this.clientApiInitialized, value);
					if (this.ClientApiStatus == EApiStatus.Ok)
					{
						value();
					}
				}
			}
			remove
			{
				lock (this)
				{
					this.clientApiInitialized = (Action)Delegate.Remove(this.clientApiInitialized, value);
				}
			}
		}

		public bool InitClientApis()
		{
			if (this.ClientApiStatus == EApiStatus.Ok)
			{
				return true;
			}
			EosCreds eosCreds = GameManager.IsDedicatedServer ? EosCreds.ServerCredentials : EosCreds.ClientCredentials;
			this.initPlatform(eosCreds, eosCreds.ServerMode);
			return this.ClientApiStatus == EApiStatus.Ok;
		}

		public bool InitServerApis()
		{
			return this.InitClientApis();
		}

		public void ServerApiLoaded()
		{
		}

		public void Update()
		{
			if (this.ClientApiStatus != EApiStatus.Ok)
			{
				return;
			}
			this.platformTickTimer += Time.unscaledDeltaTime;
			if (this.platformTickTimer >= 0.1f)
			{
				this.platformTickTimer = 0f;
				this.tickDurationStopwatch.Restart();
				object lockObject = AntiCheatCommon.LockObject;
				lock (lockObject)
				{
					this.PlatformInterface.Tick();
				}
				long num = this.tickDurationStopwatch.ElapsedMicroseconds / 1000L;
				if (Api.DebugLevel != Api.EDebugLevel.Off && num > 20L)
				{
					Log.Warning(string.Format("[EOS] Tick took exceptionally long: {0} ms", num));
				}
			}
		}

		public void Destroy()
		{
			if (this.ClientApiStatus != EApiStatus.Ok)
			{
				return;
			}
			this.ConnectInterface = null;
			object lockObject = AntiCheatCommon.LockObject;
			lock (lockObject)
			{
				this.PlatformInterface.Release();
			}
			this.PlatformInterface = null;
			lockObject = AntiCheatCommon.LockObject;
			lock (lockObject)
			{
				PlatformInterface.Shutdown();
			}
		}

		public float GetScreenBoundsValueFromSystem()
		{
			return 1f;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void initPlatform(EosCreds _creds, bool _serverMode)
		{
			InitializeOptions initializeOptions = new InitializeOptions
			{
				ProductName = "7 Days To Die",
				ProductVersion = Constants.cVersionInformation.SerializableString
			};
			Result result = Result.NotFound;
			object lockObject;
			try
			{
				lockObject = AntiCheatCommon.LockObject;
				lock (lockObject)
				{
					result = PlatformInterface.Initialize(ref initializeOptions);
				}
			}
			catch (DllNotFoundException e)
			{
				this.ClientApiStatus = EApiStatus.PermanentError;
				Log.Error("[EOS] Native library or one of its dependencies not found (e.g. no Microsoft Visual C Redistributables 2022)");
				Log.Exception(e);
				Application.Quit(1);
			}
			Log.Out(string.Format("[EOS] Initialize: {0}", result));
			LogLevel logLevel;
			switch (Api.DebugLevel)
			{
			case Api.EDebugLevel.Off:
				logLevel = LogLevel.Warning;
				break;
			case Api.EDebugLevel.Normal:
				logLevel = LogLevel.Info;
				break;
			case Api.EDebugLevel.Verbose:
				logLevel = LogLevel.VeryVerbose;
				break;
			default:
				throw new ArgumentOutOfRangeException("DebugLevel");
			}
			LogLevel logLevel2 = logLevel;
			lockObject = AntiCheatCommon.LockObject;
			lock (lockObject)
			{
				LoggingInterface.SetLogLevel(LogCategory.AllCategories, logLevel2);
				string launchArgument = GameUtils.GetLaunchArgument("debugeac");
				if (launchArgument != null)
				{
					LoggingInterface.SetLogLevel(LogCategory.AntiCheat, (launchArgument == "verbose") ? LogLevel.Verbose : LogLevel.Info);
				}
				else
				{
					LoggingInterface.SetLogLevel(LogCategory.AntiCheat, LogLevel.Warning);
				}
				LoggingInterface.SetLogLevel(LogCategory.Analytics, LogLevel.Error);
				LoggingInterface.SetLogLevel(LogCategory.Messaging, LogLevel.Warning);
				LoggingInterface.SetLogLevel(LogCategory.Ecom, LogLevel.Error);
				LoggingInterface.SetLogLevel(LogCategory.Auth, LogLevel.Error);
				LoggingInterface.SetLogLevel(LogCategory.Presence, LogLevel.Warning);
				LoggingInterface.SetLogLevel(LogCategory.Overlay, LogLevel.Warning);
				LoggingInterface.SetLogLevel(LogCategory.Ui, LogLevel.Warning);
				LoggingInterface.SetCallback(new LogMessageFunc(this.logCallback));
			}
			this.PlatformInterface = this.createPlatformInterface(_creds, _serverMode);
			if (this.PlatformInterface == null)
			{
				this.ClientApiStatus = EApiStatus.PermanentError;
				Log.Error("[EOS] Failed to create platform");
				return;
			}
			IPlatform nativePlatform = PlatformManager.NativePlatform;
			if (((nativePlatform != null) ? nativePlatform.ApplicationState : null) != null)
			{
				PlatformManager.NativePlatform.ApplicationState.OnApplicationStateChanged += this.OnApplicationStateChanged;
			}
			lockObject = AntiCheatCommon.LockObject;
			lock (lockObject)
			{
				this.ConnectInterface = this.PlatformInterface.GetConnectInterface();
			}
			if (this.ConnectInterface == null)
			{
				this.ClientApiStatus = EApiStatus.PermanentError;
				Log.Error("[EOS] Failed to get connect interface");
				return;
			}
			lockObject = AntiCheatCommon.LockObject;
			lock (lockObject)
			{
				this.SanctionsInterface = this.PlatformInterface.GetSanctionsInterface();
			}
			if (this.SanctionsInterface == null)
			{
				this.ClientApiStatus = EApiStatus.PermanentError;
				Log.Error("[EOS] Failed to get sanctions interface");
				return;
			}
			this.ClientApiStatus = EApiStatus.Ok;
			Action action = this.clientApiInitialized;
			if (action == null)
			{
				return;
			}
			action();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public PlatformInterface createPlatformInterface(EosCreds _creds, bool _serverMode)
		{
			WindowsOptions windowsOptions = default(WindowsOptions);
			windowsOptions.ProductId = _creds.ProductId;
			windowsOptions.SandboxId = _creds.SandboxId;
			windowsOptions.ClientCredentials = new ClientCredentials
			{
				ClientId = _creds.ClientId,
				ClientSecret = _creds.ClientSecret
			};
			windowsOptions.DeploymentId = _creds.DeploymentId;
			windowsOptions.EncryptionKey = "0000000000000000000000000000000000000000000000000000000000000000";
			windowsOptions.IsServer = _serverMode;
			windowsOptions.Flags = PlatformFlags.DisableOverlay;
			windowsOptions.Flags |= PlatformFlags.DisableSocialOverlay;
			windowsOptions.RTCOptions = null;
			windowsOptions.RTCOptions = new WindowsRTCOptions?(new WindowsRTCOptions
			{
				PlatformSpecificOptions = new WindowsRTCOptionsPlatformSpecificOptions?(new WindowsRTCOptionsPlatformSpecificOptions
				{
					XAudio29DllPath = GameIO.GetGameDir("7DaysToDie_Data/Plugins/x86_64/xaudio2_9redist.dll")
				})
			});
			windowsOptions.CacheDirectory = GameIO.GetUserGameDataDir();
			if (!Directory.Exists(windowsOptions.CacheDirectory))
			{
				Directory.CreateDirectory(windowsOptions.CacheDirectory);
			}
			object lockObject = AntiCheatCommon.LockObject;
			PlatformInterface result;
			lock (lockObject)
			{
				result = PlatformInterface.Create(ref windowsOptions);
			}
			return result;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void logCallback(ref LogMessage _message)
		{
			if (_message.Level == LogLevel.Warning && _message.Category == "LogHttp")
			{
				this.httpWarningCount++;
				if (this.httpWarningCount == 50)
				{
					this.httpNextTime = Time.unscaledTime + 600f;
					return;
				}
				if (this.httpWarningCount > 50)
				{
					if (Time.unscaledTime < this.httpNextTime)
					{
						return;
					}
					Log.Out(string.Format("[EOS] [LogHttp - Warning] Skipped {0} warnings within the last {1} seconds!", this.httpWarningCount - 50, 600f));
					this.httpWarningCount = 0;
				}
			}
			string txt = string.Format("[EOS] [{0} - {1}] {2}", _message.Category, _message.Level.ToStringCached<LogLevel>(), _message.Message);
			LogLevel level = _message.Level;
			if (level > LogLevel.Error)
			{
				if (level <= LogLevel.Info)
				{
					if (level == LogLevel.Warning)
					{
						Log.Warning(txt);
						return;
					}
					if (level != LogLevel.Info)
					{
						goto IL_129;
					}
				}
				else if (level != LogLevel.Verbose && level != LogLevel.VeryVerbose)
				{
					goto IL_129;
				}
				Log.Out(txt);
				return;
			}
			if (level == LogLevel.Off)
			{
				Log.Error(txt);
				throw new ArgumentOutOfRangeException();
			}
			if (level == LogLevel.Fatal || level == LogLevel.Error)
			{
				Log.Error(txt);
				return;
			}
			IL_129:
			throw new ArgumentOutOfRangeException();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public const float platformTickInterval = 0.1f;

		public static readonly Api.EDebugLevel DebugLevel = Api.EDebugLevel.Off;

		[PublicizedFrom(EAccessModifier.Private)]
		public IPlatform owner;

		public PlatformInterface PlatformInterface;

		public ConnectInterface ConnectInterface;

		public SanctionsInterface SanctionsInterface;

		[PublicizedFrom(EAccessModifier.Private)]
		public float platformTickTimer;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly MicroStopwatch tickDurationStopwatch = new MicroStopwatch(false);

		[PublicizedFrom(EAccessModifier.Internal)]
		public readonly SanctionsCheck eosSanctionsCheck = new SanctionsCheck();

		[PublicizedFrom(EAccessModifier.Private)]
		public Action clientApiInitialized;

		[PublicizedFrom(EAccessModifier.Private)]
		public const int httpWarningLimit = 50;

		[PublicizedFrom(EAccessModifier.Private)]
		public const float httpWarningTimeout = 600f;

		[PublicizedFrom(EAccessModifier.Private)]
		public int httpWarningCount;

		[PublicizedFrom(EAccessModifier.Private)]
		public float httpNextTime;

		public enum EDebugLevel
		{
			Off,
			Normal,
			Verbose
		}
	}
}
