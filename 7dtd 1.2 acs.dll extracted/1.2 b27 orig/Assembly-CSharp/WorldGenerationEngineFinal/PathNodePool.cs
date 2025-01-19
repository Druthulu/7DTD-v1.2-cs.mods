using System;
using System.Collections.Generic;

namespace WorldGenerationEngineFinal
{
	public class PathNodePool
	{
		public PathNodePool(int _initialSize)
		{
			this.pool = new List<PathNode>(_initialSize);
		}

		public PathNode Alloc()
		{
			PathNode pathNode;
			if (this.used >= this.pool.Count)
			{
				pathNode = new PathNode();
				this.pool.Add(pathNode);
			}
			else
			{
				pathNode = this.pool[this.used];
			}
			this.used++;
			return pathNode;
		}

		public void ReturnAll()
		{
			for (int i = 0; i < this.used; i++)
			{
				this.pool[i].Reset();
			}
			this.used = 0;
		}

		public void Cleanup()
		{
			this.ReturnAll();
			this.pool.Clear();
			this.pool.Capacity = 16;
		}

		public void LogStats()
		{
			Log.Out(string.Format("PathNodePool: Capacity={0}, Allocated={1}, InUse={2}", this.pool.Capacity, this.pool.Count, this.used));
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public List<PathNode> pool;

		[PublicizedFrom(EAccessModifier.Private)]
		public int used;
	}
}
