using System;

public class ModEventInterruptible<T1, T2, T3> : ModEventInterruptibleAbs<Func<T1, T2, T3, bool>>
{
	public Mod Invoke(T1 _a1, T2 _a2, T3 _a3)
	{
		for (int i = 0; i < this.receivers.Count; i++)
		{
			ModEventAbs<Func<T1, T2, T3, bool>>.Receiver receiver = this.receivers[i];
			try
			{
				if (!receiver.DelegateFunc(_a1, _a2, _a3))
				{
					return receiver.Mod;
				}
			}
			catch (Exception e)
			{
				base.LogError(e, receiver);
			}
		}
		return null;
	}
}
