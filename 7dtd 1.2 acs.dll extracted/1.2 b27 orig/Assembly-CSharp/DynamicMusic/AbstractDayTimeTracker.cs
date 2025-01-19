﻿using System;
using MusicUtils.Enums;

namespace DynamicMusic
{
	public abstract class AbstractDayTimeTracker : AbstractFilter, IFilter<SectionType>
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		public abstract int GetCurrentDay();

		[PublicizedFrom(EAccessModifier.Protected)]
		public abstract float GetCurrentTime();

		[PublicizedFrom(EAccessModifier.Protected)]
		public abstract void SetDawnTime();

		[PublicizedFrom(EAccessModifier.Protected)]
		public abstract void SetDuskTime();

		[PublicizedFrom(EAccessModifier.Protected)]
		public AbstractDayTimeTracker()
		{
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public AbstractDayTimeTracker.DayPeriodType dayPeriod;

		[PublicizedFrom(EAccessModifier.Protected)]
		public int currentDay;

		[PublicizedFrom(EAccessModifier.Protected)]
		public float currentTime;

		[PublicizedFrom(EAccessModifier.Protected)]
		public float dawnTime;

		[PublicizedFrom(EAccessModifier.Protected)]
		public float duskTime;

		[PublicizedFrom(EAccessModifier.Protected)]
		public enum DayPeriodType : byte
		{
			Morning,
			Day,
			Night,
			Dusk,
			Dawn
		}
	}
}
