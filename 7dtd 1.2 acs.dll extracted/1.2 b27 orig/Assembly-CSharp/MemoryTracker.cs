using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

public class MemoryTracker
{
	public static MemoryTracker Instance
	{
		get
		{
			if (MemoryTracker.m_Instance == null)
			{
				MemoryTracker.m_Instance = new MemoryTracker();
			}
			return MemoryTracker.m_Instance;
		}
	}

	public void New(object _o)
	{
		Dictionary<object, int> obj = this.refs;
		lock (obj)
		{
			Type type = _o.GetType();
			this.refs[type] = (this.refs.ContainsKey(type) ? (this.refs[type] + 1) : 1);
		}
	}

	public void Delete(object _o)
	{
		Dictionary<object, int> obj = this.refs;
		lock (obj)
		{
			Type type = _o.GetType();
			this.refs[type] = this.refs[type] - 1;
		}
	}

	public void SetEstimationFunction(object _o, MemoryTracker.EstimateOwnedBytes _func)
	{
		if (_o == null)
		{
			return;
		}
		Dictionary<Type, MemoryTracker.AllocationsForType> obj = this.allocTypeDict;
		lock (obj)
		{
			Type type = _o.GetType();
			MemoryTracker.AllocationsForType allocationsForType;
			if (!this.allocTypeDict.TryGetValue(type, out allocationsForType))
			{
				allocationsForType = new MemoryTracker.AllocationsForType();
				this.allocTypeDict.Add(type, allocationsForType);
			}
			allocationsForType.allocations.AddLast(new MemoryTracker.Allocation(_o, _func));
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int EstimateSelfBytes()
	{
		int num = 0;
		Dictionary<object, int> obj = this.refs;
		lock (obj)
		{
			num += MemoryTracker.GetUsedSize<object, int>(this.refs);
			num += MemoryTracker.GetUsedSize<object, int>(this.last);
		}
		Dictionary<Type, MemoryTracker.AllocationsForType> obj2 = this.allocTypeDict;
		lock (obj2)
		{
			num += MemoryTracker.GetUsedSize<Type, MemoryTracker.AllocationsForType>(this.allocTypeDict);
			foreach (MemoryTracker.AllocationsForType allocationsForType in this.allocTypeDict.Values)
			{
				num += allocationsForType.allocations.Count * MemoryTracker.GetSize<MemoryTracker.Allocation>();
			}
		}
		return num;
	}

	public void Dump()
	{
		Dictionary<string, MemoryTracker.AllocationsForType.Summary> dictionary = new Dictionary<string, MemoryTracker.AllocationsForType.Summary>();
		long num = 0L;
		Dictionary<object, int> obj = this.refs;
		lock (obj)
		{
			Log.Out("---Classes----------------------------------------");
			foreach (KeyValuePair<object, int> keyValuePair in this.refs)
			{
				Log.Out(string.Concat(new string[]
				{
					keyValuePair.Key.ToString(),
					" = ",
					keyValuePair.Value.ToString(),
					" last = ",
					(this.last.ContainsKey(keyValuePair.Key) ? this.last[keyValuePair.Key] : 0).ToString()
				}));
				this.last[keyValuePair.Key] = keyValuePair.Value;
			}
		}
		long totalMemory = GC.GetTotalMemory(false);
		Dictionary<Type, MemoryTracker.AllocationsForType> obj2 = this.allocTypeDict;
		lock (obj2)
		{
			foreach (KeyValuePair<Type, MemoryTracker.AllocationsForType> keyValuePair2 in this.allocTypeDict)
			{
				Type key = keyValuePair2.Key;
				MemoryTracker.AllocationsForType value = keyValuePair2.Value;
				MemoryTracker.AllocationsForType.Summary summary = default(MemoryTracker.AllocationsForType.Summary);
				summary.numGC = value.ClearDeadAllocations();
				foreach (MemoryTracker.Allocation allocation in value.allocations)
				{
					summary.totalBytes += (long)allocation.GetOwnedBytes();
					summary.numInstances++;
				}
				dictionary.Add(key.ToString(), summary);
				num += summary.totalBytes;
			}
		}
		int num2 = this.EstimateSelfBytes();
		dictionary.Add(typeof(MemoryTracker).ToString(), new MemoryTracker.AllocationsForType.Summary((long)num2));
		num += (long)num2;
		double num3 = (double)(totalMemory - num) * 9.5367431640625E-07;
		Log.Out("GC.GetTotalMemory (MB): {0:F2}", new object[]
		{
			(double)totalMemory * 9.5367431640625E-07
		});
		Log.Out("Total Tracked (MB): {0:F2}", new object[]
		{
			(double)num * 9.5367431640625E-07
		});
		Log.Out("Untracked (MB): {0:F2}", new object[]
		{
			num3
		});
		if (num > 0L)
		{
			StringBuilder stringBuilder = new StringBuilder();
			StringBuilder stringBuilder2 = new StringBuilder();
			stringBuilder.Append("Untracked,");
			stringBuilder2.AppendFormat("{0:F2},", num3);
			Log.Out("---Tracked----------------------------------------");
			foreach (KeyValuePair<string, MemoryTracker.AllocationsForType.Summary> keyValuePair3 in dictionary)
			{
				string key2 = keyValuePair3.Key;
				MemoryTracker.AllocationsForType.Summary value2 = keyValuePair3.Value;
				double num4 = (double)value2.totalBytes * 9.5367431640625E-07;
				Log.Out("{0}: {1:F2} MB, Count = {2}, GC Count = {3}", new object[]
				{
					key2,
					num4,
					value2.numInstances,
					value2.numGC
				});
				stringBuilder.AppendFormat("{0},", key2);
				stringBuilder2.AppendFormat("{0:F2},", num4);
			}
			if (stringBuilder.Length > 0)
			{
				Log.Out("---CSV----------------------------------------");
				stringBuilder.Remove(stringBuilder.Length - 1, 1);
				stringBuilder2.Remove(stringBuilder2.Length - 1, 1);
				Log.Out(stringBuilder.ToString());
				Log.Out(stringBuilder2.ToString());
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DrawLabel(float _left, float _top, string _text)
	{
		Rect rect = new Rect(_left, _top, (float)(_text.Length * 20), 30f);
		Rect position = new Rect(rect);
		position.x += 1f;
		position.y += 1f;
		GUI.color = Color.black;
		GUI.Label(position, _text);
		GUI.color = Color.white;
		GUI.Label(rect, _text);
	}

	public void DebugOnGui()
	{
		this.DrawLabel(800f, 30f, "Type");
		this.DrawLabel(1000f, 30f, "Count");
		int num = 0;
		Dictionary<object, int> obj = this.refs;
		lock (obj)
		{
			foreach (KeyValuePair<object, int> keyValuePair in this.refs)
			{
				this.DrawLabel(800f, (float)(80 + num * 35), keyValuePair.Key.ToString());
				this.DrawLabel(1000f, (float)(80 + num * 35), keyValuePair.Value.ToString());
				num++;
			}
		}
	}

	public static int GetSize<T>()
	{
		return MemoryTracker.GetSize(typeof(T));
	}

	public static int GetSize(Type _type)
	{
		if (_type.IsEnum)
		{
			return Marshal.SizeOf(Enum.GetUnderlyingType(_type));
		}
		if (_type.IsValueType)
		{
			return UnsafeUtility.SizeOf(_type);
		}
		return IntPtr.Size;
	}

	public static int GetSize<T>(T[] _array)
	{
		int num = IntPtr.Size;
		if (_array != null)
		{
			num += MemoryTracker.GetSize<T>() * _array.Length;
		}
		return num;
	}

	public static int GetSize<T>(T[][] _doubleArray)
	{
		int num = IntPtr.Size;
		if (_doubleArray != null)
		{
			foreach (T[] array in _doubleArray)
			{
				num += MemoryTracker.GetSize<T>(array);
			}
		}
		return num;
	}

	public static int GetSize<T>(T[,] _array)
	{
		int num = IntPtr.Size;
		if (_array != null)
		{
			num += MemoryTracker.GetSize<T>() * _array.GetLength(0) * _array.GetLength(1);
		}
		return num;
	}

	public static int GetSize<T>(List<T> _list)
	{
		int num = IntPtr.Size;
		if (_list != null)
		{
			num += _list.Capacity * MemoryTracker.GetSize<T>();
		}
		return num;
	}

	public static int GetUsedSize<TKey, TValue>(IDictionary<TKey, TValue> _dictionary)
	{
		int num = IntPtr.Size;
		if (_dictionary != null)
		{
			num += (MemoryTracker.GetSize<TKey>() + MemoryTracker.GetSize<TValue>()) * _dictionary.Count;
		}
		return num;
	}

	public static int GetSize(string stringVal)
	{
		if (stringVal == null)
		{
			return IntPtr.Size;
		}
		return stringVal.Length * 2 + IntPtr.Size;
	}

	public static int GetSize(Dictionary<string, string> stringDict)
	{
		int num = 0;
		foreach (KeyValuePair<string, string> keyValuePair in stringDict)
		{
			num += MemoryTracker.GetSize(keyValuePair.Key) + MemoryTracker.GetSize(keyValuePair.Value);
		}
		return num;
	}

	public static int GetSizeAuto(object _obj)
	{
		if (_obj == null)
		{
			return IntPtr.Size;
		}
		Type type = _obj.GetType();
		if (type.IsEnum)
		{
			return Marshal.SizeOf(Enum.GetUnderlyingType(type));
		}
		if (type.IsValueType)
		{
			return UnsafeUtility.SizeOf(type);
		}
		if (type.IsArray)
		{
			Type elementType = type.GetElementType();
			Array array = _obj as Array;
			int num = IntPtr.Size;
			if (array != null)
			{
				for (int i = 0; i < array.Rank; i++)
				{
					num += array.GetLength(i) * MemoryTracker.GetSize(elementType);
				}
			}
			return num;
		}
		if (typeof(string).IsAssignableFrom(type))
		{
			string text = (string)_obj;
			int num2 = IntPtr.Size;
			if (text != null)
			{
				num2 += MemoryTracker.GetSize(text);
			}
			return num2;
		}
		return IntPtr.Size;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static MemoryTracker m_Instance;

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<object, int> refs = new Dictionary<object, int>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<object, int> last = new Dictionary<object, int>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<Type, MemoryTracker.AllocationsForType> allocTypeDict = new Dictionary<Type, MemoryTracker.AllocationsForType>();

	public delegate int EstimateOwnedBytes(object obj);

	public struct Allocation
	{
		public Allocation(object _obj, MemoryTracker.EstimateOwnedBytes _func)
		{
			this.obj = new WeakReference<object>(_obj);
			this.estimateBytesFunc = _func;
		}

		public int GetOwnedBytes()
		{
			object obj;
			if (this.obj.TryGetTarget(out obj))
			{
				return this.estimateBytesFunc(obj);
			}
			return 0;
		}

		public WeakReference<object> obj;

		public MemoryTracker.EstimateOwnedBytes estimateBytesFunc;
	}

	public class AllocationsForType
	{
		public int ClearDeadAllocations()
		{
			int num = 0;
			LinkedListNode<MemoryTracker.Allocation> next;
			for (LinkedListNode<MemoryTracker.Allocation> linkedListNode = this.allocations.First; linkedListNode != null; linkedListNode = next)
			{
				next = linkedListNode.Next;
				object obj;
				if (!linkedListNode.Value.obj.TryGetTarget(out obj) || obj == null)
				{
					this.allocations.Remove(linkedListNode);
					num++;
				}
			}
			return num;
		}

		public LinkedList<MemoryTracker.Allocation> allocations = new LinkedList<MemoryTracker.Allocation>();

		public struct Summary
		{
			public Summary(long _totalBytes)
			{
				this.totalBytes = _totalBytes;
				this.numInstances = 1;
				this.numGC = 0;
			}

			public long totalBytes;

			public int numInstances;

			public int numGC;
		}
	}
}
