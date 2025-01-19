using System;
using System.Globalization;
using XMLData.Exceptions;

namespace XMLData.Parsers
{
	public static class doubleParser
	{
		public static double Parse(string _value)
		{
			double result;
			if (StringParsers.TryParseDouble(_value, out result, 0, -1, NumberStyles.Any))
			{
				return result;
			}
			throw new InvalidValueException("Expected double value, found \"" + _value + "\"", -1);
		}

		public static string Unparse(double _value)
		{
			return _value.ToCultureInvariantString();
		}
	}
}
