using System;

public class ModEventInterruptible : ModEventInterruptibleAbs<Func<bool>>
{
	public Mod Invoke()
	{
		for (int i = 0; i < this.receivers.Count; i++)
		{
			ModEventAbs<Func<bool>>.Receiver receiver = this.receivers[i];
			try
			{
				if (!receiver.DelegateFunc())
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
