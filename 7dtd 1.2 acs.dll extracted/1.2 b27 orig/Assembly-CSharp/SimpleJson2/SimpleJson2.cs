using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.Serialization;
using System.Text;
using SimpleJson2.Reflection;

namespace SimpleJson2
{
	[GeneratedCode("simple-json", "1.0.0")]
	public static class SimpleJson2
	{
		[PublicizedFrom(EAccessModifier.Private)]
		static SimpleJson2()
		{
			SimpleJson2.EscapeTable = new char[93];
			SimpleJson2.EscapeTable[34] = '"';
			SimpleJson2.EscapeTable[92] = '\\';
			SimpleJson2.EscapeTable[8] = 'b';
			SimpleJson2.EscapeTable[12] = 'f';
			SimpleJson2.EscapeTable[10] = 'n';
			SimpleJson2.EscapeTable[13] = 'r';
			SimpleJson2.EscapeTable[9] = 't';
		}

		public static object DeserializeObject(string json)
		{
			object result;
			if (SimpleJson2.TryDeserializeObject(json, out result))
			{
				return result;
			}
			throw new SerializationException("Invalid JSON string");
		}

		public static bool TryDeserializeObject(string json, out object obj)
		{
			bool result = true;
			if (json != null)
			{
				char[] json2 = json.ToCharArray();
				int num = 0;
				obj = SimpleJson2.ParseValue(json2, ref num, ref result);
			}
			else
			{
				obj = null;
			}
			return result;
		}

		public static object DeserializeObject(string json, Type type, IJsonSerializerStrategy jsonSerializerStrategy)
		{
			object obj = SimpleJson2.DeserializeObject(json);
			if (!(type == null) && (obj == null || !ReflectionUtils.IsAssignableFrom(obj.GetType(), type)))
			{
				return (jsonSerializerStrategy ?? SimpleJson2.CurrentJsonSerializerStrategy).DeserializeObject(obj, type);
			}
			return obj;
		}

		public static object DeserializeObject(string json, Type type)
		{
			return SimpleJson2.DeserializeObject(json, type, null);
		}

		public static T DeserializeObject<T>(string json, IJsonSerializerStrategy jsonSerializerStrategy)
		{
			return (T)((object)SimpleJson2.DeserializeObject(json, typeof(T), jsonSerializerStrategy));
		}

		public static T DeserializeObject<T>(string json)
		{
			return (T)((object)SimpleJson2.DeserializeObject(json, typeof(T), null));
		}

		public static string SerializeObject(object json, IJsonSerializerStrategy jsonSerializerStrategy)
		{
			StringBuilder stringBuilder = new StringBuilder(2000);
			if (!SimpleJson2.SerializeValue(jsonSerializerStrategy, json, stringBuilder))
			{
				return null;
			}
			return stringBuilder.ToString();
		}

		public static string SerializeObject(object json)
		{
			return SimpleJson2.SerializeObject(json, SimpleJson2.CurrentJsonSerializerStrategy);
		}

		public static string EscapeToJavascriptString(string jsonString)
		{
			if (string.IsNullOrEmpty(jsonString))
			{
				return jsonString;
			}
			StringBuilder stringBuilder = new StringBuilder();
			int i = 0;
			while (i < jsonString.Length)
			{
				char c = jsonString[i++];
				if (c == '\\')
				{
					if (jsonString.Length - i >= 2)
					{
						char c2 = jsonString[i];
						if (c2 == '\\')
						{
							stringBuilder.Append('\\');
							i++;
						}
						else if (c2 == '"')
						{
							stringBuilder.Append("\"");
							i++;
						}
						else if (c2 == 't')
						{
							stringBuilder.Append('\t');
							i++;
						}
						else if (c2 == 'b')
						{
							stringBuilder.Append('\b');
							i++;
						}
						else if (c2 == 'n')
						{
							stringBuilder.Append('\n');
							i++;
						}
						else if (c2 == 'r')
						{
							stringBuilder.Append('\r');
							i++;
						}
					}
				}
				else
				{
					stringBuilder.Append(c);
				}
			}
			return stringBuilder.ToString();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static IDictionary<string, object> ParseObject(char[] json, ref int index, ref bool success)
		{
			IDictionary<string, object> dictionary = new JsonObject();
			SimpleJson2.NextToken(json, ref index);
			bool flag = false;
			while (!flag)
			{
				int num = SimpleJson2.LookAhead(json, index);
				if (num == 0)
				{
					success = false;
					return null;
				}
				if (num == 6)
				{
					SimpleJson2.NextToken(json, ref index);
				}
				else
				{
					if (num == 2)
					{
						SimpleJson2.NextToken(json, ref index);
						return dictionary;
					}
					string key = SimpleJson2.ParseString(json, ref index, ref success);
					if (!success)
					{
						success = false;
						return null;
					}
					num = SimpleJson2.NextToken(json, ref index);
					if (num != 5)
					{
						success = false;
						return null;
					}
					object value = SimpleJson2.ParseValue(json, ref index, ref success);
					if (!success)
					{
						success = false;
						return null;
					}
					dictionary[key] = value;
				}
			}
			return dictionary;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static JsonArray ParseArray(char[] json, ref int index, ref bool success)
		{
			JsonArray jsonArray = new JsonArray();
			SimpleJson2.NextToken(json, ref index);
			bool flag = false;
			while (!flag)
			{
				int num = SimpleJson2.LookAhead(json, index);
				if (num == 0)
				{
					success = false;
					return null;
				}
				if (num == 6)
				{
					SimpleJson2.NextToken(json, ref index);
				}
				else
				{
					if (num == 4)
					{
						SimpleJson2.NextToken(json, ref index);
						break;
					}
					object item = SimpleJson2.ParseValue(json, ref index, ref success);
					if (!success)
					{
						return null;
					}
					jsonArray.Add(item);
				}
			}
			return jsonArray;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static object ParseValue(char[] json, ref int index, ref bool success)
		{
			switch (SimpleJson2.LookAhead(json, index))
			{
			case 1:
				return SimpleJson2.ParseObject(json, ref index, ref success);
			case 3:
				return SimpleJson2.ParseArray(json, ref index, ref success);
			case 7:
				return SimpleJson2.ParseString(json, ref index, ref success);
			case 8:
				return SimpleJson2.ParseNumber(json, ref index, ref success);
			case 9:
				SimpleJson2.NextToken(json, ref index);
				return true;
			case 10:
				SimpleJson2.NextToken(json, ref index);
				return false;
			case 11:
				SimpleJson2.NextToken(json, ref index);
				return null;
			}
			success = false;
			return null;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static string ParseString(char[] json, ref int index, ref bool success)
		{
			StringBuilder stringBuilder = new StringBuilder(2000);
			SimpleJson2.EatWhitespace(json, ref index);
			int num = index;
			index = num + 1;
			char c = json[num];
			bool flag = false;
			while (!flag && index != json.Length)
			{
				num = index;
				index = num + 1;
				c = json[num];
				if (c == '"')
				{
					flag = true;
					break;
				}
				if (c == '\\')
				{
					if (index == json.Length)
					{
						break;
					}
					num = index;
					index = num + 1;
					c = json[num];
					if (c == '"')
					{
						stringBuilder.Append('"');
					}
					else if (c == '\\')
					{
						stringBuilder.Append('\\');
					}
					else if (c == '/')
					{
						stringBuilder.Append('/');
					}
					else if (c == 'b')
					{
						stringBuilder.Append('\b');
					}
					else if (c == 'f')
					{
						stringBuilder.Append('\f');
					}
					else if (c == 'n')
					{
						stringBuilder.Append('\n');
					}
					else if (c == 'r')
					{
						stringBuilder.Append('\r');
					}
					else if (c == 't')
					{
						stringBuilder.Append('\t');
					}
					else if (c == 'u')
					{
						if (json.Length - index < 4)
						{
							break;
						}
						uint num2;
						if (!(success = uint.TryParse(new string(json, index, 4), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out num2)))
						{
							return "";
						}
						if (55296U <= num2 && num2 <= 56319U)
						{
							index += 4;
							uint num3;
							if (json.Length - index < 6 || !(new string(json, index, 2) == "\\u") || !uint.TryParse(new string(json, index + 2, 4), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out num3) || 56320U > num3 || num3 > 57343U)
							{
								success = false;
								return "";
							}
							stringBuilder.Append((char)num2);
							stringBuilder.Append((char)num3);
							index += 6;
						}
						else
						{
							stringBuilder.Append(SimpleJson2.ConvertFromUtf32((int)num2));
							index += 4;
						}
					}
				}
				else
				{
					stringBuilder.Append(c);
				}
			}
			if (!flag)
			{
				success = false;
				return null;
			}
			return stringBuilder.ToString();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static string ConvertFromUtf32(int utf32)
		{
			if (utf32 < 0 || utf32 > 1114111)
			{
				throw new ArgumentOutOfRangeException("utf32", "The argument must be from 0 to 0x10FFFF.");
			}
			if (55296 <= utf32 && utf32 <= 57343)
			{
				throw new ArgumentOutOfRangeException("utf32", "The argument must not be in surrogate pair range.");
			}
			if (utf32 < 65536)
			{
				return new string((char)utf32, 1);
			}
			utf32 -= 65536;
			return new string(new char[]
			{
				(char)((utf32 >> 10) + 55296),
				(char)(utf32 % 1024 + 56320)
			});
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static object ParseNumber(char[] json, ref int index, ref bool success)
		{
			SimpleJson2.EatWhitespace(json, ref index);
			int lastIndexOfNumber = SimpleJson2.GetLastIndexOfNumber(json, index);
			int length = lastIndexOfNumber - index + 1;
			string text = new string(json, index, length);
			object result;
			if (text.IndexOf(".", StringComparison.OrdinalIgnoreCase) != -1 || text.IndexOf("e", StringComparison.OrdinalIgnoreCase) != -1)
			{
				double num;
				success = double.TryParse(new string(json, index, length), NumberStyles.Any, CultureInfo.InvariantCulture, out num);
				result = num;
			}
			else
			{
				long num2;
				success = long.TryParse(new string(json, index, length), NumberStyles.Any, CultureInfo.InvariantCulture, out num2);
				result = num2;
			}
			index = lastIndexOfNumber + 1;
			return result;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static int GetLastIndexOfNumber(char[] json, int index)
		{
			int num = index;
			while (num < json.Length && "0123456789+-.eE".IndexOf(json[num]) != -1)
			{
				num++;
			}
			return num - 1;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static void EatWhitespace(char[] json, ref int index)
		{
			while (index < json.Length && " \t\n\r\b\f".IndexOf(json[index]) != -1)
			{
				index++;
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static int LookAhead(char[] json, int index)
		{
			int num = index;
			return SimpleJson2.NextToken(json, ref num);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static int NextToken(char[] json, ref int index)
		{
			SimpleJson2.EatWhitespace(json, ref index);
			if (index == json.Length)
			{
				return 0;
			}
			char c = json[index];
			index++;
			if (c <= '[')
			{
				switch (c)
				{
				case '"':
					return 7;
				case '#':
				case '$':
				case '%':
				case '&':
				case '\'':
				case '(':
				case ')':
				case '*':
				case '+':
				case '.':
				case '/':
					break;
				case ',':
					return 6;
				case '-':
				case '0':
				case '1':
				case '2':
				case '3':
				case '4':
				case '5':
				case '6':
				case '7':
				case '8':
				case '9':
					return 8;
				case ':':
					return 5;
				default:
					if (c == '[')
					{
						return 3;
					}
					break;
				}
			}
			else
			{
				if (c == ']')
				{
					return 4;
				}
				if (c == '{')
				{
					return 1;
				}
				if (c == '}')
				{
					return 2;
				}
			}
			index--;
			int num = json.Length - index;
			if (num >= 5 && json[index] == 'f' && json[index + 1] == 'a' && json[index + 2] == 'l' && json[index + 3] == 's' && json[index + 4] == 'e')
			{
				index += 5;
				return 10;
			}
			if (num >= 4 && json[index] == 't' && json[index + 1] == 'r' && json[index + 2] == 'u' && json[index + 3] == 'e')
			{
				index += 4;
				return 9;
			}
			if (num >= 4 && json[index] == 'n' && json[index + 1] == 'u' && json[index + 2] == 'l' && json[index + 3] == 'l')
			{
				index += 4;
				return 11;
			}
			return 0;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static bool SerializeValue(IJsonSerializerStrategy jsonSerializerStrategy, object value, StringBuilder builder)
		{
			bool flag = true;
			string text = value as string;
			if (text != null)
			{
				flag = SimpleJson2.SerializeString(text, builder);
			}
			else
			{
				IDictionary<string, object> dictionary = value as IDictionary<string, object>;
				if (dictionary != null)
				{
					flag = SimpleJson2.SerializeObject(jsonSerializerStrategy, dictionary.Keys, dictionary.Values, builder);
				}
				else
				{
					IDictionary<string, string> dictionary2 = value as IDictionary<string, string>;
					if (dictionary2 != null)
					{
						flag = SimpleJson2.SerializeObject(jsonSerializerStrategy, dictionary2.Keys, dictionary2.Values, builder);
					}
					else
					{
						IEnumerable enumerable = value as IEnumerable;
						if (enumerable != null)
						{
							flag = SimpleJson2.SerializeArray(jsonSerializerStrategy, enumerable, builder);
						}
						else if (SimpleJson2.IsNumeric(value))
						{
							flag = SimpleJson2.SerializeNumber(value, builder);
						}
						else if (value is bool)
						{
							builder.Append(((bool)value) ? "true" : "false");
						}
						else if (value == null)
						{
							builder.Append("null");
						}
						else
						{
							object value2;
							flag = jsonSerializerStrategy.TrySerializeNonPrimitiveObject(value, out value2);
							if (flag)
							{
								SimpleJson2.SerializeValue(jsonSerializerStrategy, value2, builder);
							}
						}
					}
				}
			}
			return flag;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static bool SerializeObject(IJsonSerializerStrategy jsonSerializerStrategy, IEnumerable keys, IEnumerable values, StringBuilder builder)
		{
			builder.Append("{");
			IEnumerator enumerator = keys.GetEnumerator();
			IEnumerator enumerator2 = values.GetEnumerator();
			bool flag = true;
			while (enumerator.MoveNext() && enumerator2.MoveNext())
			{
				object obj = enumerator.Current;
				object value = enumerator2.Current;
				if (!flag)
				{
					builder.Append(",");
				}
				string text = obj as string;
				if (text != null)
				{
					SimpleJson2.SerializeString(text, builder);
				}
				else if (!SimpleJson2.SerializeValue(jsonSerializerStrategy, value, builder))
				{
					return false;
				}
				builder.Append(":");
				if (!SimpleJson2.SerializeValue(jsonSerializerStrategy, value, builder))
				{
					return false;
				}
				flag = false;
			}
			builder.Append("}");
			return true;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static bool SerializeArray(IJsonSerializerStrategy jsonSerializerStrategy, IEnumerable anArray, StringBuilder builder)
		{
			builder.Append("[");
			bool flag = true;
			foreach (object value in anArray)
			{
				if (!flag)
				{
					builder.Append(",");
				}
				if (!SimpleJson2.SerializeValue(jsonSerializerStrategy, value, builder))
				{
					return false;
				}
				flag = false;
			}
			builder.Append("]");
			return true;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static bool SerializeString(string aString, StringBuilder builder)
		{
			if (aString.IndexOfAny(SimpleJson2.EscapeCharacters) == -1)
			{
				builder.Append('"');
				builder.Append(aString);
				builder.Append('"');
				return true;
			}
			builder.Append('"');
			int num = 0;
			char[] array = aString.ToCharArray();
			for (int i = 0; i < array.Length; i++)
			{
				char c = array[i];
				if ((int)c >= SimpleJson2.EscapeTable.Length || SimpleJson2.EscapeTable[(int)c] == '\0')
				{
					num++;
				}
				else
				{
					if (num > 0)
					{
						builder.Append(array, i - num, num);
						num = 0;
					}
					builder.Append('\\');
					builder.Append(SimpleJson2.EscapeTable[(int)c]);
				}
			}
			if (num > 0)
			{
				builder.Append(array, array.Length - num, num);
			}
			builder.Append('"');
			return true;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static bool SerializeNumber(object number, StringBuilder builder)
		{
			if (number is long)
			{
				builder.Append(((long)number).ToString(CultureInfo.InvariantCulture));
			}
			else if (number is ulong)
			{
				builder.Append(((ulong)number).ToString(CultureInfo.InvariantCulture));
			}
			else if (number is int)
			{
				builder.Append(((int)number).ToString(CultureInfo.InvariantCulture));
			}
			else if (number is uint)
			{
				builder.Append(((uint)number).ToString(CultureInfo.InvariantCulture));
			}
			else if (number is decimal)
			{
				builder.Append(((decimal)number).ToString(CultureInfo.InvariantCulture));
			}
			else if (number is float)
			{
				builder.Append(((float)number).ToString(CultureInfo.InvariantCulture));
			}
			else
			{
				builder.Append(Convert.ToDouble(number, CultureInfo.InvariantCulture).ToString("r", CultureInfo.InvariantCulture));
			}
			return true;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static bool IsNumeric(object value)
		{
			return value is sbyte || value is byte || value is short || value is ushort || value is int || value is uint || value is long || value is ulong || value is float || value is double || value is decimal;
		}

		public static IJsonSerializerStrategy CurrentJsonSerializerStrategy
		{
			get
			{
				IJsonSerializerStrategy result;
				if ((result = SimpleJson2._currentJsonSerializerStrategy) == null)
				{
					result = (SimpleJson2._currentJsonSerializerStrategy = SimpleJson2.PocoJsonSerializerStrategy);
				}
				return result;
			}
			set
			{
				SimpleJson2._currentJsonSerializerStrategy = value;
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public static PocoJsonSerializerStrategy PocoJsonSerializerStrategy
		{
			get
			{
				PocoJsonSerializerStrategy result;
				if ((result = SimpleJson2._pocoJsonSerializerStrategy) == null)
				{
					result = (SimpleJson2._pocoJsonSerializerStrategy = new PocoJsonSerializerStrategy());
				}
				return result;
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public const int TOKEN_NONE = 0;

		[PublicizedFrom(EAccessModifier.Private)]
		public const int TOKEN_CURLY_OPEN = 1;

		[PublicizedFrom(EAccessModifier.Private)]
		public const int TOKEN_CURLY_CLOSE = 2;

		[PublicizedFrom(EAccessModifier.Private)]
		public const int TOKEN_SQUARED_OPEN = 3;

		[PublicizedFrom(EAccessModifier.Private)]
		public const int TOKEN_SQUARED_CLOSE = 4;

		[PublicizedFrom(EAccessModifier.Private)]
		public const int TOKEN_COLON = 5;

		[PublicizedFrom(EAccessModifier.Private)]
		public const int TOKEN_COMMA = 6;

		[PublicizedFrom(EAccessModifier.Private)]
		public const int TOKEN_STRING = 7;

		[PublicizedFrom(EAccessModifier.Private)]
		public const int TOKEN_NUMBER = 8;

		[PublicizedFrom(EAccessModifier.Private)]
		public const int TOKEN_TRUE = 9;

		[PublicizedFrom(EAccessModifier.Private)]
		public const int TOKEN_FALSE = 10;

		[PublicizedFrom(EAccessModifier.Private)]
		public const int TOKEN_NULL = 11;

		[PublicizedFrom(EAccessModifier.Private)]
		public const int BUILDER_CAPACITY = 2000;

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly char[] EscapeTable;

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly char[] EscapeCharacters = new char[]
		{
			'"',
			'\\',
			'\b',
			'\f',
			'\n',
			'\r',
			'\t'
		};

		[PublicizedFrom(EAccessModifier.Private)]
		public static IJsonSerializerStrategy _currentJsonSerializerStrategy;

		[PublicizedFrom(EAccessModifier.Private)]
		public static PocoJsonSerializerStrategy _pocoJsonSerializerStrategy;
	}
}
