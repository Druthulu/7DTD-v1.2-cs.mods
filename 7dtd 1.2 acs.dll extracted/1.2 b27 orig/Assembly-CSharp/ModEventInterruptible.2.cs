using System;

public class ModEventInterruptible<T1> : ModEventInterruptibleAbs<Func<T1, bool>>
{
	public Mod Invoke(T1 _a1)
	{
		for (int i = 0; i < this.receivers.Count; i++)
		{
			ModEventAbs<Func<T1, bool>>.Receiver receiver = this.receivers[i];
			try
			{
				if (!receiver.DelegateFunc(_a1))
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
