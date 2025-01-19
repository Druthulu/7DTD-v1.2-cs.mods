using System;

public class ModEvent : ModEventAbs<Action>
{
	public void Invoke()
	{
		for (int i = 0; i < this.receivers.Count; i++)
		{
			ModEventAbs<Action>.Receiver receiver = this.receivers[i];
			try
			{
				receiver.DelegateFunc();
			}
			catch (Exception e)
			{
				base.LogError(e, receiver);
			}
		}
	}
}
