using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

public static class Extensions
{
	public static Transform FindInChildren(this Transform _t, string _name)
	{
		int childCount = _t.childCount;
		if (childCount == 0)
		{
			return null;
		}
		Transform transform = _t.Find(_name);
		if (!transform)
		{
			for (int i = 0; i < childCount; i++)
			{
				transform = _t.GetChild(i).FindInChildren(_name);
				if (transform)
				{
					break;
				}
			}
		}
		return transform;
	}

	public static Transform FindInChilds(this Transform target, string name, bool onlyActive = false)
	{
		if (!target || name == null)
		{
			return null;
		}
		if (onlyActive && (!target.gameObject || !target.gameObject.activeSelf))
		{
			return null;
		}
		if (target.name == name)
		{
			return target;
		}
		for (int i = 0; i < target.childCount; i++)
		{
			Transform transform = target.GetChild(i).FindInChilds(name, onlyActive);
			if (transform != null)
			{
				return transform;
			}
		}
		return null;
	}

	public static T GetComponentInChildren<T>(this GameObject o, bool searchInactive, bool avoidGC = false) where T : Component
	{
		return o.transform.GetComponentInChildren(searchInactive, avoidGC);
	}

	public static T GetComponentInChildren<T>(this Component c, bool searchInactive, bool avoidGC = false) where T : Component
	{
		return c.transform.GetComponentInChildren(searchInactive, avoidGC);
	}

	public static T GetComponentInChildren<T>(this Transform t, bool searchInactive, bool avoidGC = false) where T : Component
	{
		if (!searchInactive)
		{
			return t.GetComponentInChildren<T>();
		}
		if (avoidGC)
		{
			T t2 = t.GetComponent<T>();
			if (t2 == null)
			{
				for (int i = 0; i < t.childCount; i++)
				{
					t2 = t.GetChild(i).GetComponentInChildren(searchInactive, avoidGC);
					if (t2 != null)
					{
						break;
					}
				}
			}
			return t2;
		}
		T[] componentsInChildren = t.GetComponentsInChildren<T>(true);
		if (componentsInChildren.Length == 0)
		{
			return default(T);
		}
		return componentsInChildren[0];
	}

	public static T GetOrAddComponent<T>(this GameObject go) where T : Component
	{
		T t = go.GetComponent<T>();
		if (t == null)
		{
			t = go.AddComponent<T>();
		}
		return t;
	}

	public static string GetGameObjectPath(this GameObject _obj)
	{
		string text = "/" + _obj.name;
		while (_obj.transform.parent != null)
		{
			_obj = _obj.transform.parent.gameObject;
			text = "/" + _obj.name + text;
		}
		return text;
	}

	public static bool ContainsWithComparer<T>(this List<T> _list, T _item, IEqualityComparer<T> _comparer)
	{
		if (_list == null)
		{
			throw new ArgumentNullException("_list");
		}
		if (_comparer == null)
		{
			_comparer = EqualityComparer<T>.Default;
		}
		for (int i = 0; i < _list.Count; i++)
		{
			if (_comparer.Equals(_list[i], _item))
			{
				return true;
			}
		}
		return false;
	}

	public static bool ContainsCaseInsensitive(this IList<string> _list, string _item)
	{
		if (_item == null)
		{
			for (int i = 0; i < _list.Count; i++)
			{
				if (_list[i] == null)
				{
					return true;
				}
			}
			return false;
		}
		for (int j = 0; j < _list.Count; j++)
		{
			if (StringComparer.OrdinalIgnoreCase.Equals(_list[j], _item))
			{
				return true;
			}
		}
		return false;
	}

	public static void CopyTo<T>(this IList<T> _srcList, IList<T> _dest)
	{
		foreach (T item in _srcList)
		{
			_dest.Add(item);
		}
	}

	public static bool ColorEquals(this Color32 _a, Color32 _b)
	{
		return _a.r == _b.r && _a.g == _b.g && _a.b == _b.b && _a.a == _b.a;
	}

	public static string ToHexCode(this Color _color, bool _includeAlpha = false)
	{
		return _color.ToHexCode(_includeAlpha);
	}

	public static string ToHexCode(this Color32 _color, bool _includeAlpha = false)
	{
		if (!_includeAlpha)
		{
			return string.Format("{0:X02}{1:X02}{2:X02}", _color.r, _color.g, _color.b);
		}
		return string.Format("{0:X02}{1:X02}{2:X02}{3:X02}", new object[]
		{
			_color.r,
			_color.g,
			_color.b,
			_color.a
		});
	}

	public static bool EqualsCaseInsensitive(this string _a, string _b)
	{
		return string.Equals(_a, _b, StringComparison.OrdinalIgnoreCase);
	}

	public static bool ContainsCaseInsensitive(this string _a, string _b)
	{
		return _a.IndexOf(_b, StringComparison.OrdinalIgnoreCase) >= 0;
	}

	public static string SeparateCamelCase(this string _value)
	{
		return Extensions.StringSeparationRegex.Replace(_value, " $1").Trim();
	}

	public static string ToHexString(this byte[] _bytes, string _separator = "")
	{
		return BitConverter.ToString(_bytes).Replace("-", _separator).ToUpperInvariant();
	}

	public static string RemoveLineBreaks(this string _value)
	{
		return _value.Replace("\r\n", " ").Replace("\r", " ").Replace("\n", " ");
	}

	public static int GetStableHashCode(this string _str)
	{
		return _str.AsSpan().GetStableHashCode();
	}

	public unsafe static int GetStableHashCode(this ReadOnlySpan<char> _str)
	{
		int num = 5381;
		int num2 = num;
		int num3 = 0;
		while (num3 < _str.Length && *_str[num3] != 0)
		{
			num = ((num << 5) + num ^ (int)(*_str[num3]));
			if (num3 == _str.Length - 1 || *_str[num3 + 1] == 0)
			{
				break;
			}
			num2 = ((num2 << 5) + num2 ^ (int)(*_str[num3 + 1]));
			num3 += 2;
		}
		return num + num2 * 1566083941;
	}

	public static string Unindent(this string _indented, bool _trimEmptyLines = true)
	{
		if (_trimEmptyLines)
		{
			_indented = Extensions.unindentEmptyBeginning.Replace(_indented, string.Empty);
			_indented = Extensions.unindentEmptyEnd.Replace(_indented, string.Empty);
		}
		_indented = Extensions.unindentIndentationNoLinebreak.Replace(_indented, " $1");
		_indented = Extensions.unindentIndentationRegularLinebreak.Replace(_indented, string.Empty);
		return _indented;
	}

	public static StringBuilder TrimEnd(this StringBuilder _sb)
	{
		if (_sb == null || _sb.Length == 0)
		{
			return _sb;
		}
		int num = _sb.Length - 1;
		while (num >= 0 && char.IsWhiteSpace(_sb[num]))
		{
			num--;
		}
		if (num < _sb.Length - 1)
		{
			_sb.Length = num + 1;
		}
		return _sb;
	}

	public static StringBuilder TrimStart(this StringBuilder _sb)
	{
		if (_sb == null || _sb.Length == 0)
		{
			return _sb;
		}
		int num = 0;
		while (num < _sb.Length && char.IsWhiteSpace(_sb[num]))
		{
			num++;
		}
		if (num > 0)
		{
			_sb.Remove(0, num);
		}
		return _sb;
	}

	public static StringBuilder Trim(this StringBuilder _sb)
	{
		if (_sb == null || _sb.Length == 0)
		{
			return _sb;
		}
		return _sb.TrimEnd().TrimStart();
	}

	public static string ToCultureInvariantString(this float _value)
	{
		return _value.ToString(Utils.StandardCulture);
	}

	public static string ToCultureInvariantString(this double _value)
	{
		return _value.ToString(Utils.StandardCulture);
	}

	public static string ToCultureInvariantString(this float _value, string _format)
	{
		return _value.ToString(_format, Utils.StandardCulture);
	}

	public static string ToCultureInvariantString(this double _value, string _format)
	{
		return _value.ToString(_format, Utils.StandardCulture);
	}

	public static string ToCultureInvariantString(this decimal _value)
	{
		return _value.ToString(Utils.StandardCulture);
	}

	public static string ToCultureInvariantString(this decimal _value, string _format)
	{
		return _value.ToString(_format, Utils.StandardCulture);
	}

	public static string ToCultureInvariantString(this DateTime _value)
	{
		return _value.ToString(Utils.StandardCulture);
	}

	public static string ToCultureInvariantString(this Vector2 _value)
	{
		return string.Concat(new string[]
		{
			"(",
			_value.x.ToCultureInvariantString("F1"),
			", ",
			_value.y.ToCultureInvariantString("F1"),
			")"
		});
	}

	public static string ToCultureInvariantString(this Vector2 _value, string _format)
	{
		return string.Concat(new string[]
		{
			"(",
			_value.x.ToCultureInvariantString(_format),
			", ",
			_value.y.ToCultureInvariantString(_format),
			")"
		});
	}

	public static string ToCultureInvariantString(this Vector3 _value)
	{
		return string.Concat(new string[]
		{
			"(",
			_value.x.ToCultureInvariantString("F1"),
			", ",
			_value.y.ToCultureInvariantString("F1"),
			", ",
			_value.z.ToCultureInvariantString("F1"),
			")"
		});
	}

	public static string ToCultureInvariantString(this Vector3 _value, string _format)
	{
		return string.Concat(new string[]
		{
			"(",
			_value.x.ToCultureInvariantString(_format),
			", ",
			_value.y.ToCultureInvariantString(_format),
			", ",
			_value.z.ToCultureInvariantString(_format),
			")"
		});
	}

	public static string ToCultureInvariantString(this Vector4 _value)
	{
		return string.Concat(new string[]
		{
			"(",
			_value.x.ToCultureInvariantString("F1"),
			", ",
			_value.y.ToCultureInvariantString("F1"),
			", ",
			_value.z.ToCultureInvariantString("F1"),
			", ",
			_value.w.ToCultureInvariantString("F1"),
			")"
		});
	}

	public static string ToCultureInvariantString(this Vector4 _value, string _format)
	{
		return string.Concat(new string[]
		{
			"(",
			_value.x.ToCultureInvariantString(_format),
			", ",
			_value.y.ToCultureInvariantString(_format),
			", ",
			_value.z.ToCultureInvariantString(_format),
			", ",
			_value.w.ToCultureInvariantString(_format),
			")"
		});
	}

	public static string ToCultureInvariantString(this Bounds _value)
	{
		return "Center: " + _value.center.ToCultureInvariantString() + ", Extents: " + _value.extents.ToCultureInvariantString();
	}

	public static string ToCultureInvariantString(this Rect _value)
	{
		return string.Concat(new string[]
		{
			"(x:",
			_value.x.ToCultureInvariantString("F2"),
			", y:",
			_value.y.ToCultureInvariantString("F2"),
			", width:",
			_value.width.ToCultureInvariantString("F2"),
			", height:",
			_value.height.ToCultureInvariantString("F2"),
			")"
		});
	}

	public static string ToCultureInvariantString(this Quaternion _value)
	{
		return string.Concat(new string[]
		{
			"(",
			_value.x.ToCultureInvariantString("F1"),
			", ",
			_value.y.ToCultureInvariantString("F1"),
			", ",
			_value.z.ToCultureInvariantString("F1"),
			", ",
			_value.w.ToCultureInvariantString("F1"),
			")"
		});
	}

	public static string ToCultureInvariantString(this Matrix4x4 _value)
	{
		return string.Concat(new string[]
		{
			_value.m00.ToCultureInvariantString("F5"),
			"\t",
			_value.m01.ToCultureInvariantString("F5"),
			"\t",
			_value.m02.ToCultureInvariantString("F5"),
			"\t",
			_value.m03.ToCultureInvariantString("F5"),
			"\n",
			_value.m10.ToCultureInvariantString("F5"),
			"\t",
			_value.m11.ToCultureInvariantString("F5"),
			"\t",
			_value.m12.ToCultureInvariantString("F5"),
			"\t",
			_value.m13.ToCultureInvariantString("F5"),
			"\n",
			_value.m20.ToCultureInvariantString("F5"),
			"\t",
			_value.m21.ToCultureInvariantString("F5"),
			"\t",
			_value.m22.ToCultureInvariantString("F5"),
			"\t",
			_value.m23.ToCultureInvariantString("F5"),
			"\n",
			_value.m30.ToCultureInvariantString("F5"),
			"\t",
			_value.m31.ToCultureInvariantString("F5"),
			"\t",
			_value.m32.ToCultureInvariantString("F5"),
			"\t",
			_value.m33.ToCultureInvariantString("F5"),
			"\n"
		});
	}

	public static string ToCultureInvariantString(this Color _value)
	{
		return string.Concat(new string[]
		{
			"RGBA(",
			_value.r.ToCultureInvariantString("F3"),
			", ",
			_value.g.ToCultureInvariantString("F3"),
			", ",
			_value.b.ToCultureInvariantString("F3"),
			", ",
			_value.a.ToCultureInvariantString("F3"),
			")"
		});
	}

	public static string ToCultureInvariantString(this Plane _value)
	{
		return string.Concat(new string[]
		{
			"(normal:(",
			_value.normal.x.ToCultureInvariantString("F1"),
			", ",
			_value.normal.y.ToCultureInvariantString("F1"),
			", ",
			_value.normal.z.ToCultureInvariantString("F1"),
			"), distance:",
			_value.distance.ToCultureInvariantString("F1"),
			")"
		});
	}

	public static string ToCultureInvariantString(this Ray _value)
	{
		return "Origin: " + _value.origin.ToCultureInvariantString() + ", Dir: " + _value.direction.ToCultureInvariantString();
	}

	public static string ToCultureInvariantString(this Ray2D _value)
	{
		return "Origin: " + _value.origin.ToCultureInvariantString() + ", Dir: " + _value.direction.ToCultureInvariantString();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Regex StringSeparationRegex = new Regex("((?<=\\p{Ll})\\p{Lu}|\\p{Lu}(?=\\p{Ll}))", RegexOptions.IgnorePatternWhitespace);

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Regex unindentEmptyBeginning = new Regex("^\\s*\r?\n", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.CultureInvariant);

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Regex unindentEmptyEnd = new Regex("\r?\n\\s*$", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.CultureInvariant);

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Regex unindentIndentationNoLinebreak = new Regex("\\s*\r?\n\\s*([^\\s|])", RegexOptions.Multiline | RegexOptions.Compiled | RegexOptions.CultureInvariant);

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Regex unindentIndentationRegularLinebreak = new Regex("^\\s*\\|", RegexOptions.Multiline | RegexOptions.Compiled | RegexOptions.CultureInvariant);
}
