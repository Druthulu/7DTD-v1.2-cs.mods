using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

public abstract class AIDirectorData
{
	public static void InitStatic()
	{
		AIDirectorData.noisySounds = new CaseInsensitiveStringDictionary<AIDirectorData.Noise>();
		AIDirectorData.smells = new CaseInsensitiveStringDictionary<AIDirectorData.Smell>();
	}

	public static void Cleanup()
	{
		if (AIDirectorData.noisySounds != null)
		{
			AIDirectorData.noisySounds.Clear();
		}
	}

	public static void AddNoisySound(string _name, AIDirectorData.Noise _noise)
	{
		AIDirectorData.noisySounds.Add(_name, _noise);
	}

	public static void AddSmell(string name, AIDirectorData.Smell smell)
	{
		AIDirectorData.smells.Add(name, smell);
	}

	public static bool FindNoise(string name, out AIDirectorData.Noise noise)
	{
		if (name == null)
		{
			noise = default(AIDirectorData.Noise);
			return false;
		}
		return AIDirectorData.noisySounds.TryGetValue(name, out noise);
	}

	public static bool FindSmell(string name, out AIDirectorData.Smell smell)
	{
		return AIDirectorData.smells.TryGetValue(name, out smell);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public AIDirectorData()
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static Dictionary<string, AIDirectorData.Noise> noisySounds;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Dictionary<string, AIDirectorData.Smell> smells;

	public struct Noise
	{
		public Noise(string _source, float _volume, float _duration, float _muffledWhenCrouched, float _heatMapStrength, ulong _heatMapWorldTimeToLive)
		{
			this.volume = _volume;
			this.duration = _duration;
			this.muffledWhenCrouched = _muffledWhenCrouched;
			this.heatMapStrength = _heatMapStrength;
			this.heatMapWorldTimeToLive = _heatMapWorldTimeToLive;
		}

		public float volume;

		public float duration;

		public float muffledWhenCrouched;

		public float heatMapStrength;

		public ulong heatMapWorldTimeToLive;
	}

	[Preserve]
	public class Smell
	{
		public Smell(string _name, float _range, float _beltRange, float _heatMapStrength, ulong _heatMapWorldTimeToLive)
		{
			this.name = _name;
			this.range = _range;
			this.beltRange = _beltRange;
			this.heatMapStrength = _heatMapStrength;
			this.heatMapWorldTimeToLive = _heatMapWorldTimeToLive;
		}

		public string name;

		public float range;

		public float beltRange;

		public float heatMapStrength;

		public ulong heatMapWorldTimeToLive;
	}
}
