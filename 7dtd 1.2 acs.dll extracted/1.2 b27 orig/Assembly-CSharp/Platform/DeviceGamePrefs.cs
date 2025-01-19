using System;
using System.IO;

namespace Platform
{
	public static class DeviceGamePrefs
	{
		public static void Apply()
		{
			if ((DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5).IsCurrent())
			{
				int @int = GamePrefs.GetInt(EnumGamePrefs.OptionsGfxQualityPreset);
				if (@int != 6 && @int != 7 && @int != 8 && @int != 9)
				{
					Log.Out(string.Format("[DeviceGamePrefs] Quality preset \"{0}\" is unsupported on this platform; defaulting to ConsolePerformance.", @int));
					GamePrefs.Set(EnumGamePrefs.OptionsGfxQualityPreset, 6);
				}
				GameOptionsManager.SetGraphicsQuality();
			}
			DeviceGamePrefs.ApplyConfigFilePrefs();
		}

		public static string ConfigFilename(string _deviceName)
		{
			return "gameprefs_" + _deviceName;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static void ApplyConfigFilePrefs()
		{
			string text = DeviceGamePrefs.ConfigFilename(DeviceFlags.Current.GetDeviceName());
			string text2 = Path.Combine(GameIO.GetApplicationPath(), text + ".xml");
			if (File.Exists(text2))
			{
				Log.Out("[DeviceGamePrefs] Applying game prefs from {0}", new object[]
				{
					text2
				});
				DynamicProperties dynamicProperties = new DynamicProperties();
				if (dynamicProperties.Load(GameIO.GetApplicationPath(), text, true))
				{
					DeviceGamePrefs.ApplyGamePrefs(dynamicProperties);
				}
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static void ApplyGamePrefs(DynamicProperties properties)
		{
			foreach (string text in properties.Values.Dict.Keys)
			{
				EnumGamePrefs enumGamePrefs;
				if (EnumUtils.TryParse<EnumGamePrefs>(text, out enumGamePrefs, true))
				{
					object obj = GamePrefs.Parse(enumGamePrefs, properties.Values[text]);
					if (obj != null)
					{
						GamePrefs.SetObject(enumGamePrefs, obj);
						Log.Out("[DeviceGamePrefs] {0}={1}", new object[]
						{
							text,
							GamePrefs.GetObject(enumGamePrefs)
						});
					}
					else
					{
						Log.Error("[DeviceGamePrefs] Invalid value for GamePref: {0}", new object[]
						{
							text
						});
					}
				}
				else
				{
					Log.Error("[DeviceGamePrefs] Unknown GamePref: {0}", new object[]
					{
						text
					});
				}
			}
		}
	}
}
