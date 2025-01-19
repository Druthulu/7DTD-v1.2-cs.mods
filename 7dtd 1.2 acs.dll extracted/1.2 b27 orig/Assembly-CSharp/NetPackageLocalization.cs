using System;
using System.IO;
using Noemax.GZip;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageLocalization : NetPackage
{
	public override bool Compress
	{
		get
		{
			return true;
		}
	}

	public NetPackageLocalization Setup(byte[] _data)
	{
		this.data = _data;
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		int count = _reader.ReadInt32();
		this.data = _reader.ReadBytes(count);
		using (MemoryStream memoryStream = new MemoryStream(this.data))
		{
			using (DeflateInputStream deflateInputStream = new DeflateInputStream(memoryStream))
			{
				using (MemoryStream memoryStream2 = new MemoryStream())
				{
					StreamUtils.StreamCopy(deflateInputStream, memoryStream2, null, true);
					this.data = memoryStream2.ToArray();
				}
			}
		}
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write(this.data.Length);
		_writer.Write(this.data);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		Localization.LoadServerPatchDictionary(this.data);
	}

	public override int GetLength()
	{
		return this.data.Length;
	}

	public override NetPackageDirection PackageDirection
	{
		get
		{
			return NetPackageDirection.ToClient;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public byte[] data;
}
