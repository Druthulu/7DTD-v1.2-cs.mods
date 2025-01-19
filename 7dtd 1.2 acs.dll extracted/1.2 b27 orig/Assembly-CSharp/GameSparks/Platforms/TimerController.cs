using System;
using System.Collections.Generic;

namespace GameSparks.Platforms
{
	public class TimerController
	{
		public void Initialize()
		{
			this.timeOfLastUpdate = DateTime.UtcNow.Ticks;
		}

		public void Update()
		{
			long num = DateTime.UtcNow.Ticks - this.timeOfLastUpdate;
			this.timeOfLastUpdate += num;
			foreach (IControlledTimer controlledTimer in this.timers)
			{
				controlledTimer.Update(num);
			}
		}

		public void AddTimer(IControlledTimer timer)
		{
			this.timers.Add(timer);
		}

		public void RemoveTimer(IControlledTimer timer)
		{
			this.timers.Remove(timer);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public long timeOfLastUpdate;

		[PublicizedFrom(EAccessModifier.Private)]
		public List<IControlledTimer> timers = new List<IControlledTimer>();
	}
}
