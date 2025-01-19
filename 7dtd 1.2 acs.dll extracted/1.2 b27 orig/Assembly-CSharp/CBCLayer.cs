using System;

public class CBCLayer : IMemoryPoolableObject
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public ~CBCLayer()
	{
	}

	public void Reset()
	{
	}

	public void Cleanup()
	{
	}

	public void InitData(int size)
	{
		if (this.data == null)
		{
			this.data = new byte[size];
		}
	}

	public void CopyFrom(CBCLayer _other)
	{
		Array.Copy(_other.data, this.data, this.data.Length);
	}

	public byte[] data;

	public static int InstanceCount;
}
