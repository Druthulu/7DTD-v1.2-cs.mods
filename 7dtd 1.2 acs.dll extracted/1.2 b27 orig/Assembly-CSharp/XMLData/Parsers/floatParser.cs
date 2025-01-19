using System;
using System.Globalization;
using XMLData.Exceptions;

namespace XMLData.Parsers
{
	public static class floatParser
	{
		public static float Parse(string _value)
		{
			float result;
			if (StringParsers.TryParseFloat(_value, out result, 0, -1, NumberStyles.Any))
			{
				return result;
			}
			throw new InvalidValueException("Expected float value, found \"" + _value + "\"", -1);
		}

		public static string Unparse(float _value)
		{
			return _value.ToCultureInvariantString();
		}
	}
}
