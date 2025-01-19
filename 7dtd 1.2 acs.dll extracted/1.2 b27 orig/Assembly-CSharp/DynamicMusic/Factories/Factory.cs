﻿using System;
using MusicUtils.Enums;
using UnityEngine.Scripting;

namespace DynamicMusic.Factories
{
	public static class Factory
	{
		[Preserve]
		public static Conductor CreateConductor()
		{
			return new Conductor();
		}

		[Preserve]
		public static ISectionSelector CreateSectionSelector()
		{
			return new SectionSelector();
		}

		[Preserve]
		public static IThreatLevel CreateThreatLevel()
		{
			return default(ThreatLevel);
		}

		[Preserve]
		public static IFilter<SectionType> CreatePlayerTracker()
		{
			return new PlayerTracker();
		}

		[Preserve]
		public static DayTimeTracker CreateDayTimeTracker()
		{
			return new DayTimeTracker();
		}

		[Preserve]
		public static IMultiNotifiableFilter CreateMusicTimeTracker()
		{
			return new MusicTimeTracker();
		}

		[Preserve]
		public static ISection CreateSection<T>(SectionType _sectionType) where T : ISection, new()
		{
			T t = Activator.CreateInstance<T>();
			t.Sect = _sectionType;
			t.Init();
			return t;
		}

		[Preserve]
		public static IConfiguration CreateConfiguration()
		{
			return new Configuration();
		}
	}
}
