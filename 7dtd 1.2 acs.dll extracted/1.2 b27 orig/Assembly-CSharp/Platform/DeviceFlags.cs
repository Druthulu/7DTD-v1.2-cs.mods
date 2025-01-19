using System;

namespace Platform
{
	public static class DeviceFlags
	{
		public static DeviceFlag Current
		{
			get
			{
				DeviceFlag deviceFlag = DeviceFlags.m_current.GetValueOrDefault();
				if (DeviceFlags.m_current == null)
				{
					deviceFlag = DeviceFlags.GetCurrentDeviceFlag();
					DeviceFlags.m_current = new DeviceFlag?(deviceFlag);
					return deviceFlag;
				}
				return deviceFlag;
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static DeviceFlag GetCurrentDeviceFlag()
		{
			return DeviceFlag.StandaloneWindows;
		}

		public static bool IsCurrent(this DeviceFlag flags)
		{
			return flags.HasFlag(DeviceFlags.Current);
		}

		public const DeviceFlag Standalone = DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX;

		public const DeviceFlag XBoxSeries = DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX;

		public const DeviceFlag PS5 = DeviceFlag.PS5;

		public const DeviceFlag Console = DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5;

		public const DeviceFlag All = DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5;

		public const DeviceFlag None = DeviceFlag.None;

		[PublicizedFrom(EAccessModifier.Private)]
		public static DeviceFlag? m_current;
	}
}
