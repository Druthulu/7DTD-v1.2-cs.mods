﻿using System;
using UnityEngine;

namespace XMLData.Parsers
{
	public static class Vector2Parser
	{
		public static Vector2 Parse(string _value)
		{
			return StringParsers.ParseVector2(_value);
		}

		public static string Unparse(Vector2 _value)
		{
			return string.Format("{0},{1}", _value.x.ToCultureInvariantString(), _value.y.ToCultureInvariantString());
		}
	}
}
