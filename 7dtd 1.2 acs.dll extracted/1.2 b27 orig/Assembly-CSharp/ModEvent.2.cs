using System;

public class ModEvent<T1> : ModEventAbs<Action<T1>>
{
	public void Invoke(T1 _a1)
	{
		for (int i = 0; i < this.receivers.Count; i++)
		{
			ModEventAbs<Action<T1>>.Receiver receiver = this.receivers[i];
			try
			{
				receiver.DelegateFunc(_a1);
			}
			catch (Exception e)
			{
				base.LogError(e, receiver);
			}
		}
	}
}
