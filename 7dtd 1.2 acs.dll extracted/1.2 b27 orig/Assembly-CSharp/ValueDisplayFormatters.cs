﻿using System;
using System.Text;
using UnityEngine;

public static class ValueDisplayFormatters
{
	public static double RoundToSignificantDigits(this double _value, int _digits)
	{
		if (_value == 0.0)
		{
			return 0.0;
		}
		double num = Math.Pow(10.0, Math.Floor(Math.Log10(Math.Abs(_value))) + 1.0);
		return num * Math.Round(_value / num, _digits);
	}

	public static string FormatNumberWithMetricPrefix(this double _value, bool _allowDecimals = true, int _significantDigits = 3)
	{
		int num = 5;
		double num2 = (double)Math.Sign(_value);
		double num3 = Math.Abs(_value);
		if (_allowDecimals)
		{
			num3 = num3.RoundToSignificantDigits(_significantDigits);
			while (num3 > 1000.0)
			{
				num3 /= 1000.0;
				num++;
			}
			while (num3 != 0.0 && num3 < 1.0)
			{
				num3 *= 1000.0;
				num--;
			}
			if (num >= 0 && num < ValueDisplayFormatters.metricPrefixes.Length)
			{
				return (num2 * num3).ToCultureInvariantString("G" + _significantDigits.ToString()) + ((num != 5) ? ValueDisplayFormatters.metricPrefixes[num].ToString() : "");
			}
		}
		else
		{
			while (num3 > 10000.0)
			{
				num3 /= 1000.0;
				num++;
			}
			while (num3 != 0.0 && num3 < 10.0)
			{
				num3 *= 1000.0;
				num--;
			}
			if (num >= 0 && num < ValueDisplayFormatters.metricPrefixes.Length)
			{
				return (num2 * num3).ToCultureInvariantString("0") + ((num != 5) ? ValueDisplayFormatters.metricPrefixes[num].ToString() : "");
			}
		}
		return _value.ToCultureInvariantString("g");
	}

	public static bool UseEnglishCardinalDirections
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return GamePrefs.GetBool(EnumGamePrefs.OptionsUiCompassUseEnglishCardinalDirections);
		}
	}

	public static string CardinalDirectionsLanguage
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (!ValueDisplayFormatters.UseEnglishCardinalDirections)
			{
				return null;
			}
			return Localization.DefaultLanguage;
		}
	}

	public static string WorldPos(Vector3 _pos, string _delim = " ", bool _useLongGeoDirs = false)
	{
		return ValueDisplayFormatters.WorldPosLongitude(_pos, _useLongGeoDirs) + _delim + ValueDisplayFormatters.WorldPosLatitude(_pos, _useLongGeoDirs);
	}

	public static string WorldPosLatitude(Vector3 _pos, bool _useLongGeoDirs = false)
	{
		string cardinalDirectionsLanguage = ValueDisplayFormatters.CardinalDirectionsLanguage;
		string arg = Localization.Get("geoDirection" + ((_pos.z >= 0f) ? "N" : "S") + (_useLongGeoDirs ? "Long" : ""), cardinalDirectionsLanguage, false);
		string arg2 = Utils.FastAbs(_pos.z).ToCultureInvariantString("0");
		return string.Format(Localization.Get("geoLocationSingleAxis", cardinalDirectionsLanguage, false), arg2, arg);
	}

	public static string WorldPosLongitude(Vector3 _pos, bool _useLongGeoDirs = false)
	{
		string cardinalDirectionsLanguage = ValueDisplayFormatters.CardinalDirectionsLanguage;
		string arg = Localization.Get("geoDirection" + ((_pos.x >= 0f) ? "E" : "W") + (_useLongGeoDirs ? "Long" : ""), cardinalDirectionsLanguage, false);
		string arg2 = Utils.FastAbs(_pos.x).ToCultureInvariantString("0");
		return string.Format(Localization.Get("geoLocationSingleAxis", cardinalDirectionsLanguage, false), arg2, arg);
	}

	public static string Direction(GameUtils.DirEightWay _direction, bool _useLongGeoDirs = false)
	{
		string cardinalDirectionsLanguage = ValueDisplayFormatters.CardinalDirectionsLanguage;
		return Localization.Get("geoDirection" + _direction.ToStringCached<GameUtils.DirEightWay>() + (_useLongGeoDirs ? "Long" : ""), cardinalDirectionsLanguage, false);
	}

	public static string WorldTime(ulong _worldTime, string _format)
	{
		ValueTuple<int, int, int> valueTuple = GameUtils.WorldTimeToElements(_worldTime);
		int item = valueTuple.Item1;
		int item2 = valueTuple.Item2;
		int item3 = valueTuple.Item3;
		return string.Format(_format, item, item2, item3);
	}

	public static string Distance(float _distance, bool _useLongDistanceName)
	{
		string format = Localization.Get("geoDistanceTemplate", false);
		string arg;
		if (_distance > 1000f)
		{
			arg = Localization.Get(_useLongDistanceName ? "geoDistanceKmLong" : "geoDistanceKm", false);
			return string.Format(format, (_distance / 1000f).ToCultureInvariantString("0.0"), arg);
		}
		arg = Localization.Get(_useLongDistanceName ? "geoDistanceMLong" : "geoDistanceM", false);
		if (_distance > 100f)
		{
			return string.Format(format, _distance.ToCultureInvariantString("0"), arg);
		}
		return string.Format(format, _distance.ToCultureInvariantString("0.0"), arg);
	}

	public static string Distance(float _distance)
	{
		return ValueDisplayFormatters.Distance(_distance, false);
	}

	public static string DateAge(DateTime _dateTime)
	{
		TimeSpan timeSpan = DateTime.Now - _dateTime;
		if (timeSpan.TotalHours < 24.0)
		{
			return string.Format(Localization.Get("timeAgeHours", false), Mathf.RoundToInt((float)timeSpan.TotalHours));
		}
		if (timeSpan.TotalDays < 7.0)
		{
			return string.Format(Localization.Get("timeAgeDays", false), Mathf.RoundToInt((float)timeSpan.TotalDays));
		}
		if (timeSpan.TotalDays < 31.0)
		{
			return string.Format(Localization.Get("timeAgeWeeks", false), Mathf.RoundToInt((float)(timeSpan.TotalDays / 7.0)));
		}
		if (timeSpan.TotalDays < 365.0)
		{
			return string.Format(Localization.Get("timeAgeMonths", false), Mathf.RoundToInt((float)(timeSpan.TotalDays / 31.0)));
		}
		return string.Format(Localization.Get("timeAgeYears", false), Mathf.RoundToInt((float)(timeSpan.TotalDays / 365.0)));
	}

	public static string Temperature(float _fahrenheit, int _decimals)
	{
		string str = "°F";
		if (GamePrefs.GetBool(EnumGamePrefs.OptionsTempCelsius))
		{
			_fahrenheit = Utils.ToCelsius(_fahrenheit);
			str = "°C";
		}
		return _fahrenheit.ToString(string.Format("F{0}", _decimals)) + str;
	}

	public static string TemperatureRelative(float _fahrenheit, int _decimals)
	{
		string str = "°F";
		if (GamePrefs.GetBool(EnumGamePrefs.OptionsTempCelsius))
		{
			_fahrenheit = Utils.ToRelativeCelsius(_fahrenheit);
			str = "°C";
		}
		return ((_fahrenheit >= 0f) ? "+" : "") + _fahrenheit.ToString(string.Format("F{0}", _decimals)) + str;
	}

	public static string RomanNumber(int _value)
	{
		if (_value <= 0)
		{
			return _value.ToString();
		}
		if (_value <= 10)
		{
			return ValueDisplayFormatters.romanNumbers[_value];
		}
		StringBuilder stringBuilder = new StringBuilder();
		while (_value > 0)
		{
			if (_value >= 1000)
			{
				stringBuilder.Append("M");
				_value -= 1000;
			}
			else if (_value >= 900)
			{
				stringBuilder.Append("CM");
				_value -= 900;
			}
			else if (_value >= 500)
			{
				stringBuilder.Append("D");
				_value -= 500;
			}
			else if (_value >= 400)
			{
				stringBuilder.Append("CD");
				_value -= 400;
			}
			else if (_value >= 100)
			{
				stringBuilder.Append("C");
				_value -= 100;
			}
			else if (_value >= 90)
			{
				stringBuilder.Append("XC");
				_value -= 90;
			}
			else if (_value >= 50)
			{
				stringBuilder.Append("L");
				_value -= 50;
			}
			else if (_value >= 40)
			{
				stringBuilder.Append("XL");
				_value -= 40;
			}
			else if (_value >= 10)
			{
				stringBuilder.Append("X");
				_value -= 10;
			}
			else
			{
				stringBuilder.Append(ValueDisplayFormatters.romanNumbers[_value]);
			}
		}
		return stringBuilder.ToString();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly char[] metricPrefixes = new char[]
	{
		'f',
		'p',
		'n',
		'µ',
		'm',
		' ',
		'k',
		'M',
		'G',
		'T',
		'P'
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public const int BaseMetricPrefixIndex = 5;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly string[] romanNumbers = new string[]
	{
		"",
		"I",
		"II",
		"III",
		"IV",
		"V",
		"VI",
		"VII",
		"VIII",
		"IX",
		"X"
	};
}
