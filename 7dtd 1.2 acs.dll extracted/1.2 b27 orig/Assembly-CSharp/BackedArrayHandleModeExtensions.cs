using System;

public static class BackedArrayHandleModeExtensions
{
	public static bool CanRead(this BackedArrayHandleMode mode)
	{
		bool result;
		if (mode != BackedArrayHandleMode.ReadOnly)
		{
			if (mode != BackedArrayHandleMode.ReadWrite)
			{
				throw new ArgumentOutOfRangeException("mode", mode, string.Format("Unknown mode: {0}", mode));
			}
			result = true;
		}
		else
		{
			result = true;
		}
		return result;
	}

	public static bool CanWrite(this BackedArrayHandleMode mode)
	{
		bool result;
		if (mode != BackedArrayHandleMode.ReadOnly)
		{
			if (mode != BackedArrayHandleMode.ReadWrite)
			{
				throw new ArgumentOutOfRangeException("mode", mode, string.Format("Unknown mode: {0}", mode));
			}
			result = true;
		}
		else
		{
			result = false;
		}
		return result;
	}
}
