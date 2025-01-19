using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_MapStats : XUiController
{
	public override bool GetBindingValue(ref string value, string bindingName)
	{
		uint num = <PrivateImplementationDetails>.ComputeStringHash(bindingName);
		if (num <= 871453238U)
		{
			if (num <= 195109979U)
			{
				if (num != 85644157U)
				{
					if (num == 195109979U)
					{
						if (bindingName == "mapwind")
						{
							value = "";
							if (XUi.IsGameRunning() && base.xui.playerUI.entityPlayer != null)
							{
								value = this.mapwindFormatter.Format(Mathf.RoundToInt(WeatherManager.GetWindSpeed()));
							}
							return true;
						}
					}
				}
				else if (bindingName == "showtime")
				{
					value = "true";
					if (XUi.IsGameRunning() && base.xui.playerUI.entityPlayer != null)
					{
						value = (EffectManager.GetValue(PassiveEffects.NoTimeDisplay, null, 0f, base.xui.playerUI.entityPlayer, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false) == 0f).ToString();
					}
					return true;
				}
			}
			else if (num != 281380209U)
			{
				if (num != 304163417U)
				{
					if (num == 871453238U)
					{
						if (bindingName == "mapdaytimetitle")
						{
							value = Localization.Get("xuiDayTime", false);
							return true;
						}
					}
				}
				else if (bindingName == "playercoretemp")
				{
					value = "";
					if (XUi.IsGameRunning() && base.xui.playerUI.entityPlayer != null)
					{
						value = XUiM_Player.GetCoreTemp(base.xui.playerUI.entityPlayer);
					}
					return true;
				}
			}
			else if (bindingName == "maptemperature")
			{
				value = "";
				if (XUi.IsGameRunning() && base.xui.playerUI.entityPlayer != null)
				{
					value = this.tempFormatter.Format(WeatherManager.Instance.GetCurrentTemperatureValue());
				}
				return true;
			}
		}
		else if (num <= 1972321830U)
		{
			if (num != 1600623488U)
			{
				if (num == 1972321830U)
				{
					if (bindingName == "mapelevationtitle")
					{
						value = Localization.Get("xuiElevation", false);
						return true;
					}
				}
			}
			else if (bindingName == "mapdaytime")
			{
				value = "";
				if (XUi.IsGameRunning() && base.xui.playerUI.entityPlayer != null)
				{
					value = this.mapdaytimeFormatter.Format(GameManager.Instance.World.worldTime);
				}
				return true;
			}
		}
		else if (num != 2762345723U)
		{
			if (num != 3729109520U)
			{
				if (num == 4025935093U)
				{
					if (bindingName == "playercoretemptitle")
					{
						value = Localization.Get("xuiFeelsLike", false);
						return true;
					}
				}
			}
			else if (bindingName == "mapelevation")
			{
				value = "";
				if (XUi.IsGameRunning() && base.xui.playerUI.entityPlayer != null)
				{
					int v = Mathf.RoundToInt(base.xui.playerUI.entityPlayer.GetPosition().y - WeatherManager.SeaLevel());
					value = this.levelFormatter.Format(v);
				}
				return true;
			}
		}
		else if (bindingName == "mapwindtitle")
		{
			value = Localization.Get("xuiWind", false);
			return true;
		}
		return false;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		base.RefreshBindings(false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<ulong> mapdaytimeFormatter = new CachedStringFormatter<ulong>((ulong _worldTime) => ValueDisplayFormatters.WorldTime(_worldTime, "{0}/{1:00}:{2:00}"));

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<float> tempFormatter = new CachedStringFormatter<float>((float _f) => ValueDisplayFormatters.Temperature(_f, 0));

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterInt mapwindFormatter = new CachedStringFormatterInt();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<int> levelFormatter = new CachedStringFormatter<int>((int _i) => _i.ToString("+0;-#"));
}
