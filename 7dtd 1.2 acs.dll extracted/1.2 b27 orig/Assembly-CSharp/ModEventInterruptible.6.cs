using System;

public class ModEventInterruptible<T1, T2, T3, T4, T5, T6> : ModEventInterruptibleAbs<global::Func<T1, T2, T3, T4, T5, T6, bool>>
{
	public Mod Invoke(T1 _a1, T2 _a2, T3 _a3, T4 _a4, T5 _a5, T6 _a6)
	{
		for (int i = 0; i < this.receivers.Count; i++)
		{
			ModEventAbs<global::Func<T1, T2, T3, T4, T5, T6, bool>>.Receiver receiver = this.receivers[i];
			try
			{
				if (!receiver.DelegateFunc(_a1, _a2, _a3, _a4, _a5, _a6))
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
