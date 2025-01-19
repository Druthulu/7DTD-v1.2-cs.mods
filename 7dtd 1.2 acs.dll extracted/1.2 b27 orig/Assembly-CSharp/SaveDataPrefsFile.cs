using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

public sealed class SaveDataPrefsFile : ISaveDataPrefs
{
	public static SaveDataPrefsFile INSTANCE
	{
		get
		{
			SaveDataPrefsFile result;
			if ((result = SaveDataPrefsFile.s_instance) == null)
			{
				result = (SaveDataPrefsFile.s_instance = new SaveDataPrefsFile());
			}
			return result;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public SaveDataPrefsFile()
	{
		foreach (SaveDataPrefsFile.PrefType prefType in EnumUtils.Values<SaveDataPrefsFile.PrefType>())
		{
			if (!SaveDataPrefsFile.PrefTypeMapping.ContainsKey(prefType))
			{
				throw new KeyNotFoundException(string.Format("Expected {0} to have key '{1}'.", "PrefTypeMapping", prefType));
			}
		}
		this.m_storageFilePath = GameIO.GetNormalizedPath(Path.Join(GameIO.GetUserGameDataDir(), "prefs.cfg"));
		this.m_storage = new Dictionary<string, SaveDataPrefsFile.Pref>();
		this.Load();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public unsafe static void Escape(TextWriter writer, ReadOnlySpan<char> raw, bool ignoreSeparator)
	{
		bool flag = false;
		ReadOnlySpan<char> readOnlySpan = raw;
		for (int i = 0; i < readOnlySpan.Length; i++)
		{
			char c = (char)(*readOnlySpan[i]);
			if ((!ignoreSeparator || c != '=') && SaveDataPrefsFile.EscapeMapping.ContainsKey(c))
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			writer.Write(raw);
			return;
		}
		readOnlySpan = raw;
		for (int i = 0; i < readOnlySpan.Length; i++)
		{
			char c2 = (char)(*readOnlySpan[i]);
			char value;
			if ((ignoreSeparator && c2 == '=') || !SaveDataPrefsFile.EscapeMapping.TryGetValue(c2, out value))
			{
				writer.Write(c2);
			}
			else
			{
				writer.Write('\\');
				writer.Write(value);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public unsafe static void Unescape(StringBuilder builder, ReadOnlySpan<char> escaped)
	{
		if (escaped.IndexOf('\\') < 0)
		{
			builder.Append(escaped);
			return;
		}
		bool flag = false;
		int num = -1;
		ReadOnlySpan<char> readOnlySpan = escaped;
		for (int i = 0; i < readOnlySpan.Length; i++)
		{
			char c = (char)(*readOnlySpan[i]);
			num++;
			if (flag)
			{
				flag = false;
				char value;
				if (!SaveDataPrefsFile.UnescapeMapping.TryGetValue(c, out value))
				{
					Log.Warning(string.Format("Unexpected character after escape prefix at offset {0} (will be taken as-is): {1}", num, c));
					builder.Append(c);
				}
				builder.Append(value);
			}
			else if (c == '\\')
			{
				flag = true;
			}
			else
			{
				builder.Append(c);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public unsafe static int IndexOfFirstUnescapedSeparator(ReadOnlySpan<char> search)
	{
		bool flag = false;
		int num = -1;
		ReadOnlySpan<char> readOnlySpan = search;
		for (int i = 0; i < readOnlySpan.Length; i++)
		{
			char c = (char)(*readOnlySpan[i]);
			num++;
			if (flag)
			{
				flag = false;
			}
			else if (c == '\\')
			{
				flag = true;
			}
			else if (c == '=')
			{
				return num;
			}
		}
		return -1;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SaveInternal()
	{
		object storageLock = this.m_storageLock;
		lock (storageLock)
		{
			if (this.m_dirty)
			{
				try
				{
					using (StreamWriter streamWriter = SdFile.CreateText(this.m_storageFilePath))
					{
						int num = 0;
						foreach (KeyValuePair<string, SaveDataPrefsFile.Pref> keyValuePair in this.m_storage)
						{
							string text;
							SaveDataPrefsFile.Pref pref;
							keyValuePair.Deconstruct(out text, out pref);
							string text2 = text;
							SaveDataPrefsFile.Pref pref2 = pref;
							string value;
							if (!pref2.TryToString(out value))
							{
								Log.Out(string.Format("[{0}] Failed to convert pref '{1}' of type {2} to a string representation.", "SaveDataPrefsFile", text2, pref2.Type));
							}
							else
							{
								SaveDataPrefsFile.Escape(streamWriter, text2, false);
								streamWriter.Write('=');
								SaveDataPrefsFile.Escape(streamWriter, value, true);
								streamWriter.WriteLine();
								num++;
							}
						}
						this.m_dirty = false;
						Log.Out(string.Format("[{0}] Saved {1} player pref(s) to: {2}", "SaveDataPrefsFile", num, this.m_storageFilePath));
					}
				}
				catch (IOException ex)
				{
					Log.Error("[SaveDataPrefsFile] Failed to Save: " + ex.Message);
					Log.Exception(ex);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void LoadInternal()
	{
		object storageLock = this.m_storageLock;
		lock (storageLock)
		{
			this.m_storage.Clear();
			if (SdFile.Exists(this.m_storageFilePath))
			{
				try
				{
					using (StreamReader streamReader = SdFile.OpenText(this.m_storageFilePath))
					{
						StringBuilder stringBuilder = new StringBuilder();
						int num = 0;
						int num2 = 0;
						for (;;)
						{
							string text = streamReader.ReadLine();
							if (text == null)
							{
								break;
							}
							num2++;
							ReadOnlySpan<char> readOnlySpan = text;
							int num3 = SaveDataPrefsFile.IndexOfFirstUnescapedSeparator(readOnlySpan);
							if (num3 < 0)
							{
								Log.Error(string.Format("[{0}] Skipping line {1} since is missing unescaped separator '{2}'. Contents: {3}", new object[]
								{
									"SaveDataPrefsFile",
									num2,
									'=',
									text
								}));
							}
							else
							{
								StringBuilder builder = stringBuilder;
								ReadOnlySpan<char> readOnlySpan2 = readOnlySpan;
								SaveDataPrefsFile.Unescape(builder, readOnlySpan2.Slice(0, num3));
								string text2 = stringBuilder.ToString();
								stringBuilder.Clear();
								StringBuilder builder2 = stringBuilder;
								readOnlySpan2 = readOnlySpan;
								int num4 = num3 + 1;
								SaveDataPrefsFile.Unescape(builder2, readOnlySpan2.Slice(num4, readOnlySpan2.Length - num4));
								string text3 = stringBuilder.ToString();
								stringBuilder.Clear();
								SaveDataPrefsFile.Pref value;
								if (!SaveDataPrefsFile.Pref.TryParse(text3, out value))
								{
									Log.Error("[SaveDataPrefsFile] Failed to parse pref '" + text2 + "' with string representation: " + text3);
								}
								else
								{
									this.m_storage[text2] = value;
									num++;
								}
							}
						}
						Log.Out(string.Format("[{0}] Loaded {1} player pref(s) from: {2}", "SaveDataPrefsFile", num, this.m_storageFilePath));
					}
					return;
				}
				catch (IOException ex)
				{
					Log.Error("[SaveDataPrefsFile] Failed to Load: " + ex.Message);
					Log.Exception(ex);
					return;
				}
			}
			Log.Out("[SaveDataPrefsFile] Using empty player prefs, as none exists at: " + this.m_storageFilePath);
		}
	}

	public float GetFloat(string key, float defaultValue)
	{
		object storageLock = this.m_storageLock;
		float result;
		lock (storageLock)
		{
			SaveDataPrefsFile.Pref pref;
			float num;
			result = ((this.m_storage.TryGetValue(key, out pref) && pref.TryGet(out num)) ? num : defaultValue);
		}
		return result;
	}

	public void SetFloat(string key, float value)
	{
		object storageLock = this.m_storageLock;
		lock (storageLock)
		{
			SaveDataPrefsFile.Pref pref;
			if (this.m_storage.TryGetValue(key, out pref))
			{
				float num;
				if (!pref.TryGet(out num) || num != value)
				{
					pref.Set(value);
					this.m_dirty = true;
				}
			}
			else
			{
				pref = new SaveDataPrefsFile.Pref(value);
				this.m_storage[key] = pref;
				this.m_dirty = true;
			}
		}
	}

	public int GetInt(string key, int defaultValue)
	{
		object storageLock = this.m_storageLock;
		int result;
		lock (storageLock)
		{
			SaveDataPrefsFile.Pref pref;
			int num;
			result = ((this.m_storage.TryGetValue(key, out pref) && pref.TryGet(out num)) ? num : defaultValue);
		}
		return result;
	}

	public void SetInt(string key, int value)
	{
		object storageLock = this.m_storageLock;
		lock (storageLock)
		{
			SaveDataPrefsFile.Pref pref;
			if (this.m_storage.TryGetValue(key, out pref))
			{
				int num;
				if (!pref.TryGet(out num) || num != value)
				{
					pref.Set(value);
					this.m_dirty = true;
				}
			}
			else
			{
				pref = new SaveDataPrefsFile.Pref(value);
				this.m_storage[key] = pref;
				this.m_dirty = true;
			}
		}
	}

	public string GetString(string key, string defaultValue)
	{
		object storageLock = this.m_storageLock;
		string result;
		lock (storageLock)
		{
			SaveDataPrefsFile.Pref pref;
			string text;
			result = ((this.m_storage.TryGetValue(key, out pref) && pref.TryGet(out text)) ? text : defaultValue);
		}
		return result;
	}

	public void SetString(string key, string value)
	{
		object storageLock = this.m_storageLock;
		lock (storageLock)
		{
			SaveDataPrefsFile.Pref pref;
			if (this.m_storage.TryGetValue(key, out pref))
			{
				string a;
				if (!pref.TryGet(out a) || !(a == value))
				{
					pref.Set(value);
					this.m_dirty = true;
				}
			}
			else
			{
				pref = new SaveDataPrefsFile.Pref(value);
				this.m_storage[key] = pref;
				this.m_dirty = true;
			}
		}
	}

	public bool HasKey(string key)
	{
		object storageLock = this.m_storageLock;
		bool result;
		lock (storageLock)
		{
			result = this.m_storage.ContainsKey(key);
		}
		return result;
	}

	public void DeleteKey(string key)
	{
		object storageLock = this.m_storageLock;
		lock (storageLock)
		{
			this.m_storage.Remove(key);
		}
	}

	public void DeleteAll()
	{
		object storageLock = this.m_storageLock;
		lock (storageLock)
		{
			this.m_storage.Clear();
		}
	}

	public void Save()
	{
		this.SaveInternal();
	}

	public void Load()
	{
		this.LoadInternal();
	}

	public bool CanLoad
	{
		get
		{
			return true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static SaveDataPrefsFile s_instance;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Dictionary<char, char> EscapeMapping = new Dictionary<char, char>
	{
		{
			'\0',
			'0'
		},
		{
			'\r',
			'r'
		},
		{
			'\n',
			'n'
		},
		{
			'=',
			'='
		},
		{
			'\\',
			'\\'
		}
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Dictionary<char, char> UnescapeMapping = SaveDataPrefsFile.EscapeMapping.ToDictionary((KeyValuePair<char, char> pair) => pair.Value, (KeyValuePair<char, char> pair) => pair.Key);

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Dictionary<SaveDataPrefsFile.PrefType, char> PrefTypeMapping = new Dictionary<SaveDataPrefsFile.PrefType, char>
	{
		{
			SaveDataPrefsFile.PrefType.Float,
			'F'
		},
		{
			SaveDataPrefsFile.PrefType.Int,
			'I'
		},
		{
			SaveDataPrefsFile.PrefType.String,
			'S'
		}
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Dictionary<char, SaveDataPrefsFile.PrefType> PrefTypeUnmapping = SaveDataPrefsFile.PrefTypeMapping.ToDictionary((KeyValuePair<SaveDataPrefsFile.PrefType, char> kv) => kv.Value, (KeyValuePair<SaveDataPrefsFile.PrefType, char> kv) => kv.Key);

	[PublicizedFrom(EAccessModifier.Private)]
	public const string StorageFileName = "prefs.cfg";

	[PublicizedFrom(EAccessModifier.Private)]
	public const char KeyValueSeparator = '=';

	[PublicizedFrom(EAccessModifier.Private)]
	public const char EscapePrefix = '\\';

	[PublicizedFrom(EAccessModifier.Private)]
	public const char PrefTypeSeparator = ':';

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly string m_storageFilePath;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<string, SaveDataPrefsFile.Pref> m_storage;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly object m_storageLock = new object();

	[PublicizedFrom(EAccessModifier.Private)]
	public bool m_dirty;

	[PublicizedFrom(EAccessModifier.Private)]
	public enum PrefType
	{
		Float,
		Int,
		String
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public class Pref
	{
		public Pref(float value)
		{
			this.Set(value);
		}

		public Pref(int value)
		{
			this.Set(value);
		}

		public Pref(string value)
		{
			this.Set(value);
		}

		public SaveDataPrefsFile.PrefType Type
		{
			get
			{
				return this.m_type;
			}
		}

		public bool TryGet(out float value)
		{
			if (this.m_type != SaveDataPrefsFile.PrefType.Float)
			{
				value = 0f;
				return false;
			}
			value = this.m_values.Float;
			return true;
		}

		public bool TryGet(out int value)
		{
			if (this.m_type != SaveDataPrefsFile.PrefType.Int)
			{
				value = 0;
				return false;
			}
			value = this.m_values.Int;
			return true;
		}

		public bool TryGet(out string value)
		{
			if (this.m_type != SaveDataPrefsFile.PrefType.String)
			{
				value = null;
				return false;
			}
			value = this.m_refs.String;
			return true;
		}

		public void Set(float value)
		{
			this.m_type = SaveDataPrefsFile.PrefType.Float;
			this.m_values.Float = value;
			this.m_refs.String = null;
		}

		public void Set(int value)
		{
			this.m_type = SaveDataPrefsFile.PrefType.Int;
			this.m_values.Int = value;
			this.m_refs.String = null;
		}

		public void Set(string value)
		{
			this.m_type = SaveDataPrefsFile.PrefType.String;
			this.m_refs.String = value;
			this.m_values.Int = 0;
		}

		public bool TryToString(out string stringRepresentation)
		{
			char c;
			if (!SaveDataPrefsFile.PrefTypeMapping.TryGetValue(this.m_type, out c))
			{
				Log.Warning(string.Format("[{0}] No char mapping for pref type '{1}'.", "SaveDataPrefsFile", this.m_type));
				stringRepresentation = null;
				return false;
			}
			switch (this.m_type)
			{
			case SaveDataPrefsFile.PrefType.Float:
				stringRepresentation = string.Format("{0}{1}{2:R}", c, ':', this.m_values.Float);
				return true;
			case SaveDataPrefsFile.PrefType.Int:
				stringRepresentation = string.Format("{0}{1}{2}", c, ':', this.m_values.Int);
				return true;
			case SaveDataPrefsFile.PrefType.String:
				stringRepresentation = string.Format("{0}{1}{2}", c, ':', this.m_refs.String);
				return true;
			default:
				Log.Error(string.Format("[{0}] Missing to string implementation for '{1}'.", "SaveDataPrefsFile", this.m_type));
				stringRepresentation = null;
				return false;
			}
		}

		public unsafe static bool TryParse(ReadOnlySpan<char> stringRepresentation, out SaveDataPrefsFile.Pref pref)
		{
			if (stringRepresentation.Length < 2 || *stringRepresentation[1] != 58)
			{
				pref = null;
				return false;
			}
			SaveDataPrefsFile.PrefType prefType;
			if (!SaveDataPrefsFile.PrefTypeUnmapping.TryGetValue((char)(*stringRepresentation[0]), out prefType))
			{
				pref = null;
				return false;
			}
			ReadOnlySpan<char> readOnlySpan = stringRepresentation;
			ReadOnlySpan<char> readOnlySpan2 = readOnlySpan.Slice(2, readOnlySpan.Length - 2);
			switch (prefType)
			{
			case SaveDataPrefsFile.PrefType.Float:
			{
				float value;
				if (!float.TryParse(readOnlySpan2, out value))
				{
					pref = null;
					return false;
				}
				pref = new SaveDataPrefsFile.Pref(value);
				return true;
			}
			case SaveDataPrefsFile.PrefType.Int:
			{
				int value2;
				if (!int.TryParse(readOnlySpan2, out value2))
				{
					pref = null;
					return false;
				}
				pref = new SaveDataPrefsFile.Pref(value2);
				return true;
			}
			case SaveDataPrefsFile.PrefType.String:
				pref = new SaveDataPrefsFile.Pref(new string(readOnlySpan2));
				return true;
			default:
				Log.Error(string.Format("[{0}] Missing parse implementation for '{1}'.", "SaveDataPrefsFile", prefType));
				pref = null;
				return false;
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public SaveDataPrefsFile.PrefType m_type;

		[PublicizedFrom(EAccessModifier.Private)]
		public SaveDataPrefsFile.Pref.PrefValues m_values;

		[PublicizedFrom(EAccessModifier.Private)]
		public SaveDataPrefsFile.Pref.PrefRefs m_refs;

		[PublicizedFrom(EAccessModifier.Private)]
		[StructLayout(LayoutKind.Explicit)]
		public struct PrefValues
		{
			[FieldOffset(0)]
			public float Float;

			[FieldOffset(0)]
			public int Int;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		[StructLayout(LayoutKind.Explicit)]
		public struct PrefRefs
		{
			[FieldOffset(0)]
			public string String;
		}
	}
}
