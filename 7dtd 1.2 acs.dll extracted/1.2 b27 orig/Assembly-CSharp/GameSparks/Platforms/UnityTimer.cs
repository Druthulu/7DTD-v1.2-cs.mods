using System;

namespace GameSparks.Platforms
{
	public class UnityTimer : IControlledTimer, IGameSparksTimer
	{
		public void SetController(TimerController controller)
		{
			this.controller = controller;
			this.controller.AddTimer(this);
		}

		public void Initialize(int interval, Action callback)
		{
			this.callback = callback;
			this.interval = interval;
			this.running = true;
		}

		public void Trigger()
		{
		}

		public void Stop()
		{
			this.running = false;
			this.callback = null;
			this.controller.RemoveTimer(this);
		}

		public void Update(long ticks)
		{
			if (this.running)
			{
				this.elapsedTicks += ticks;
				if (this.elapsedTicks > (long)this.interval)
				{
					this.elapsedTicks -= (long)this.interval;
					if (this.callback != null)
					{
						this.callback();
					}
				}
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public Action callback;

		[PublicizedFrom(EAccessModifier.Private)]
		public int interval;

		[PublicizedFrom(EAccessModifier.Private)]
		public long elapsedTicks;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool running;

		[PublicizedFrom(EAccessModifier.Private)]
		public TimerController controller;
	}
}
