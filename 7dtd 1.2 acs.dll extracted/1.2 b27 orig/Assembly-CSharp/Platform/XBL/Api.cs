using System;
using Unity.XGamingRuntime;

namespace Platform.XBL
{
	public class Api : IPlatformApi
	{
		public void Init(IPlatform _owner)
		{
			this._owner = _owner;
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
			if (!XblHelpers.Succeeded(SDK.XGameRuntimeInitialize(), "Initialize gaming runtime", true, false))
			{
				this.ClientApiStatus = EApiStatus.PermanentError;
				Log.Error("[XBL] Failed to initialize GDK");
				return false;
			}
			if (!XblHelpers.Succeeded(SDK.CreateDefaultTaskQueue()))
			{
				Log.Error("[XBL] Failed to create task queue");
				this.ClientApiStatus = EApiStatus.PermanentError;
				return false;
			}
			if (!XblHelpers.Succeeded(SDK.XBL.XblInitialize("00000000-0000-0000-0000-0000680ee616"), "Initialize Xbox Live", true, false))
			{
				this.ClientApiStatus = EApiStatus.PermanentError;
				Log.Error("[XBL] Failed to initialize Xbox Live");
				return false;
			}
			Log.Out("[XBL] API loaded");
			this.ClientApiStatus = EApiStatus.Ok;
			Action action = this.clientApiInitialized;
			if (action != null)
			{
				action();
			}
			return true;
		}

		public bool InitServerApis()
		{
			return true;
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
			SDK.XTaskQueueDispatch(0U);
		}

		public void Destroy()
		{
			SDK.CloseDefaultXTaskQueue();
			SDK.XBL.XblCleanup(null);
			SDK.XGameRuntimeUninitialize();
			this.ClientApiStatus = EApiStatus.Uninitialized;
		}

		public float GetScreenBoundsValueFromSystem()
		{
			return 1f;
		}

		public const string SCID = "00000000-0000-0000-0000-0000680ee616";

		public const int TitleId = 1745806870;

		[PublicizedFrom(EAccessModifier.Private)]
		public IPlatform _owner;

		[PublicizedFrom(EAccessModifier.Private)]
		public Action clientApiInitialized;
	}
}
