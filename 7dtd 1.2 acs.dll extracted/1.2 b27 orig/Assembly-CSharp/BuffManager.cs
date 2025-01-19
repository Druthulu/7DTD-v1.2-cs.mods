using System;

public class BuffManager
{
	public static void UpdateBuffTimers(BuffValue _ev, float _deltaTime)
	{
		if (_ev.BuffClass != null)
		{
			_ev.BuffClass.UpdateTimer(_ev, _deltaTime);
			return;
		}
		_ev.Remove = true;
	}

	public static void Cleanup()
	{
		if (BuffManager.Buffs != null)
		{
			BuffManager.Buffs.Clear();
			BuffManager.Buffs = null;
		}
	}

	public static void AddBuff(BuffClass _buffClass)
	{
		BuffManager.Buffs[_buffClass.Name] = _buffClass;
	}

	public static BuffClass GetBuff(string _name)
	{
		BuffClass result;
		BuffManager.Buffs.TryGetValue(_name, out result);
		return result;
	}

	public static CaseInsensitiveStringDictionary<BuffClass> Buffs;
}
