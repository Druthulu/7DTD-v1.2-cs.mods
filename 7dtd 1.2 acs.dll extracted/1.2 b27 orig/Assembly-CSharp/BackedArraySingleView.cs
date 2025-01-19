using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public sealed class BackedArraySingleView<[IsUnmanaged] T> : IBackedArrayView<T>, IDisposable where T : struct, ValueType
{
	public BackedArraySingleView(IBackedArray<T> array, BackedArrayHandleMode mode, int viewLength = 0, int startingOffset = 0)
	{
		this.m_array = array;
		this.m_length = array.Length;
		this.m_mode = mode;
		if (!mode.CanRead())
		{
			throw new ArgumentException("Expected a readable mode.", "mode");
		}
		if (viewLength <= 0)
		{
			viewLength = BackedArraySingleView<T>.GetDefaultViewLength(array.Length);
		}
		this.m_viewLength = Math.Min(this.m_length, viewLength);
		this.m_viewStart = startingOffset;
		if (this.m_viewStart < 0 || this.m_viewStart >= this.m_length)
		{
			throw new IndexOutOfRangeException(string.Format("{0} is not within length {1}.", this.m_viewStart, this.m_length));
		}
		if (this.m_viewStart + this.m_viewLength > this.m_length)
		{
			this.m_viewStart = this.m_length - this.m_viewLength;
		}
		this.m_viewEnd = this.m_viewStart + this.m_viewLength;
		IBackedArrayHandle viewHandle;
		if (mode != BackedArrayHandleMode.ReadOnly)
		{
			if (mode != BackedArrayHandleMode.ReadWrite)
			{
				throw new ArgumentOutOfRangeException("mode", mode, string.Format("Unknown mode: {0}", mode));
			}
			viewHandle = this.m_array.GetMemoryUnsafe(this.m_viewStart, this.m_viewLength, out this.m_viewPtr);
		}
		else
		{
			viewHandle = this.m_array.GetReadOnlyMemoryUnsafe(this.m_viewStart, this.m_viewLength, out this.m_viewPtr);
		}
		this.m_viewHandle = viewHandle;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Dispose(bool disposing)
	{
		if (!disposing)
		{
			Log.Error("BackedArraySingleView<T> is being finalized, it should be disposed properly.");
			return;
		}
		if (this.IsDisposed())
		{
			return;
		}
		this.m_viewHandle.Dispose();
		this.m_viewHandle = null;
		this.m_viewLength = 0;
		this.m_viewStart = 0;
		this.m_viewEnd = 0;
		this.m_viewPtr = null;
		this.m_array = null;
	}

	public void Dispose()
	{
		this.Dispose(true);
		GC.SuppressFinalize(this);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public ~BackedArraySingleView()
	{
		this.Dispose(false);
	}

	[Conditional("DEVELOPMENT_BUILD")]
	[Conditional("UNITY_EDITOR")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void ThrowIfDisposed()
	{
		if (this.IsDisposed())
		{
			throw new ObjectDisposedException("BackedArraySingleView has already been disposed.");
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool IsDisposed()
	{
		return this.m_viewHandle == null;
	}

	[Conditional("DEVELOPMENT_BUILD")]
	[Conditional("UNITY_EDITOR")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void ThrowIfCannotWrite()
	{
		if (!this.m_mode.CanWrite())
		{
			throw new NotSupportedException("This BackedArraySingleView is not writable.");
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Cache(int i)
	{
		if (this.m_viewStart <= i && i < this.m_viewEnd)
		{
			return;
		}
		if (i < 0 || i >= this.m_length)
		{
			throw new IndexOutOfRangeException(string.Format("Expected index {0} to be in the range [0, {1}).", i, this.m_length));
		}
		if (i + this.m_viewLength > this.m_length)
		{
			i = this.m_length - this.m_viewLength;
		}
		this.m_viewHandle.Dispose();
		BackedArrayHandleMode mode = this.m_mode;
		IBackedArrayHandle viewHandle;
		if (mode != BackedArrayHandleMode.ReadOnly)
		{
			if (mode != BackedArrayHandleMode.ReadWrite)
			{
				throw new ArgumentOutOfRangeException("m_mode", this.m_mode, string.Format("Unknown mode: {0}", this.m_mode));
			}
			viewHandle = this.m_array.GetMemoryUnsafe(i, this.m_viewLength, out this.m_viewPtr);
		}
		else
		{
			viewHandle = this.m_array.GetReadOnlyMemoryUnsafe(i, this.m_viewLength, out this.m_viewPtr);
		}
		this.m_viewHandle = viewHandle;
		this.m_viewStart = i;
		this.m_viewEnd = i + this.m_viewLength;
	}

	public int Length
	{
		get
		{
			return this.m_length;
		}
	}

	public BackedArrayHandleMode Mode
	{
		get
		{
			return this.m_mode;
		}
	}

	public unsafe T this[int i]
	{
		get
		{
			this.Cache(i);
			return this.m_viewPtr[(IntPtr)(i - this.m_viewStart) * (IntPtr)sizeof(T) / (IntPtr)sizeof(T)];
		}
		set
		{
			this.Cache(i);
			this.m_viewPtr[(IntPtr)(i - this.m_viewStart) * (IntPtr)sizeof(T) / (IntPtr)sizeof(T)] = value;
		}
	}

	public void Flush()
	{
		this.m_viewHandle.Flush();
	}

	public static int GetDefaultViewLength(int length)
	{
		return Mathf.NextPowerOfTwo(4 * (int)Math.Sqrt((double)length));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IBackedArray<T> m_array;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly int m_length;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly BackedArrayHandleMode m_mode;

	[PublicizedFrom(EAccessModifier.Private)]
	public int m_viewLength;

	[PublicizedFrom(EAccessModifier.Private)]
	public int m_viewStart;

	[PublicizedFrom(EAccessModifier.Private)]
	public int m_viewEnd;

	[PublicizedFrom(EAccessModifier.Private)]
	public unsafe T* m_viewPtr;

	[PublicizedFrom(EAccessModifier.Private)]
	public IBackedArrayHandle m_viewHandle;
}
