using System;
using UnityEngine;

public static class vp_TargetEventReturn<T, U, R>
{
	public static void Register(object target, string eventName, Func<T, U, R> callback)
	{
		vp_TargetEventHandler.Register(target, eventName, callback, 6);
	}

	public static void Unregister(object target, string eventName, Func<T, U, R> callback)
	{
		vp_TargetEventHandler.Unregister(target, eventName, callback);
	}

	public static void Unregister(object target)
	{
		vp_TargetEventHandler.Unregister(target, null, null);
	}

	public static R Send(object target, string eventName, T arg1, U arg2, vp_TargetEventOptions options = vp_TargetEventOptions.DontRequireReceiver)
	{
		R result;
		for (;;)
		{
			Delegate callback = vp_TargetEventHandler.GetCallback(target, eventName, false, 6, options);
			if (callback == null)
			{
				break;
			}
			try
			{
				result = ((Func<T, U, R>)callback)(arg1, arg2);
			}
			catch
			{
				eventName += "_";
				continue;
			}
			return result;
		}
		vp_TargetEventHandler.OnNoReceiver(eventName, options);
		result = default(R);
		return result;
	}

	public static R SendUpwards(Component target, string eventName, T arg1, U arg2, vp_TargetEventOptions options = vp_TargetEventOptions.DontRequireReceiver)
	{
		R result;
		for (;;)
		{
			Delegate callback = vp_TargetEventHandler.GetCallback(target, eventName, true, 6, options);
			if (callback == null)
			{
				break;
			}
			try
			{
				result = ((Func<T, U, R>)callback)(arg1, arg2);
			}
			catch
			{
				eventName += "_";
				continue;
			}
			return result;
		}
		vp_TargetEventHandler.OnNoReceiver(eventName, options);
		result = default(R);
		return result;
	}
}
