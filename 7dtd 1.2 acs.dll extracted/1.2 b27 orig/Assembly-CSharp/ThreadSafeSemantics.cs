using System;
using System.Threading;

public class ThreadSafeSemantics : IThreadingSemantics
{
	public ThreadSafeSemantics()
	{
		this.monitor = this;
	}

	public ThreadSafeSemantics(object _monitor)
	{
		this.monitor = _monitor;
	}

	public void Synchronize(AtomicActionDelegate _delegate)
	{
		object obj = this.monitor;
		lock (obj)
		{
			_delegate();
		}
	}

	public int InterlockedAdd(ref int _number, int _add)
	{
		return Interlocked.Add(ref _number, _add);
	}

	public T Synchronize<T>(Func<T> _delegate)
	{
		object obj = this.monitor;
		T result;
		lock (obj)
		{
			result = _delegate();
		}
		return result;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public object monitor;
}
