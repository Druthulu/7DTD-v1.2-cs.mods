using System;

public class ModEvent<T1, T2> : ModEventAbs<Action<T1, T2>>
{
	public void Invoke(T1 _a1, T2 _a2)
	{
		for (int i = 0; i < this.receivers.Count; i++)
		{
			ModEventAbs<Action<T1, T2>>.Receiver receiver = this.receivers[i];
			try
			{
				receiver.DelegateFunc(_a1, _a2);
			}
			catch (Exception e)
			{
				base.LogError(e, receiver);
			}
		}
	}
}
