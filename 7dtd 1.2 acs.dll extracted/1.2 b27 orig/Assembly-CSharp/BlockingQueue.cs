using System;
using System.Collections.Generic;
using System.Threading;

public class BlockingQueue<T>
{
	public void Enqueue(T item)
	{
		Queue<T> obj = this.queue;
		lock (obj)
		{
			this.queue.Enqueue(item);
			Monitor.PulseAll(this.queue);
		}
	}

	public T Dequeue()
	{
		Queue<T> obj = this.queue;
		T result;
		lock (obj)
		{
			while (this.queue.Count == 0)
			{
				if (this.closing)
				{
					result = default(T);
					return result;
				}
				Monitor.Wait(this.queue);
			}
			result = this.queue.Dequeue();
		}
		return result;
	}

	public bool HasData()
	{
		Queue<T> obj = this.queue;
		bool result;
		lock (obj)
		{
			result = (this.queue.Count > 0);
		}
		return result;
	}

	public void Close()
	{
		Queue<T> obj = this.queue;
		lock (obj)
		{
			this.closing = true;
			Monitor.PulseAll(this.queue);
		}
	}

	public void Clear()
	{
		Queue<T> obj = this.queue;
		lock (obj)
		{
			this.queue.Clear();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool closing;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Queue<T> queue = new Queue<T>();
}
