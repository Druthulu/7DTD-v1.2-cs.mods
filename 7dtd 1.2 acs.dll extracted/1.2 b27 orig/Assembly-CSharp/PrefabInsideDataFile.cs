using System;
using System.IO;

public class PrefabInsideDataFile
{
	public PrefabInsideDataFile()
	{
	}

	public PrefabInsideDataFile(PrefabInsideDataFile _other)
	{
		this.size = _other.size;
		this.data = _other.data;
	}

	public void Init(Vector3i _size)
	{
		this.size = _size;
		this.data = null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Alloc()
	{
		this.data = new byte[this.size.x * this.size.y * this.size.z + 7 >> 3];
	}

	public void Add(int _offset)
	{
		if (this.data == null)
		{
			this.Alloc();
		}
		byte[] array = this.data;
		int num = _offset >> 3;
		array[num] |= (byte)(1 << (_offset & 7));
	}

	public void Add(int x, int y, int z)
	{
		int offset = x + y * this.size.x + z * this.size.x * this.size.y;
		this.Add(offset);
	}

	public bool Contains(int x, int y, int z)
	{
		if (this.data == null)
		{
			return false;
		}
		int num = x + y * this.size.x + z * this.size.x * this.size.y;
		return ((int)this.data[num >> 3] & 1 << (num & 7)) > 0;
	}

	public PrefabInsideDataFile Clone()
	{
		return new PrefabInsideDataFile(this);
	}

	public void Load(string _filename, Vector3i _size)
	{
		this.Init(_size);
		if (!SdFile.Exists(_filename))
		{
			return;
		}
		try
		{
			using (Stream stream = SdFile.OpenRead(_filename))
			{
				using (PooledBinaryReader pooledBinaryReader = MemoryPools.poolBinaryReader.AllocSync(false))
				{
					pooledBinaryReader.SetBaseStream(stream);
					try
					{
						this.Read(pooledBinaryReader);
					}
					catch (Exception e)
					{
						Log.Error("PrefabInsideDataFile Load {0}, expected data len {1}. Probably outdated ins file, please re-save to fix. Read error:", new object[]
						{
							_filename,
							this.data.Length
						});
						Log.Exception(e);
					}
				}
			}
		}
		catch (Exception e2)
		{
			Log.Exception(e2);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Read(BinaryReader _br)
	{
		int num = (int)_br.ReadByte();
		int num2 = _br.ReadInt32();
		if (num <= 1)
		{
			for (int i = 0; i < num2; i++)
			{
				int x = (int)_br.ReadByte();
				int y = (int)_br.ReadByte();
				int z = (int)_br.ReadByte();
				this.Add(x, y, z);
			}
			return;
		}
		if (num2 > 0)
		{
			this.Alloc();
			_br.Read(this.data, 0, num2);
		}
	}

	public void Save(string _filename)
	{
		try
		{
			using (Stream stream = SdFile.Open(_filename, FileMode.Create))
			{
				using (PooledBinaryWriter pooledBinaryWriter = MemoryPools.poolBinaryWriter.AllocSync(false))
				{
					pooledBinaryWriter.SetBaseStream(stream);
					this.Write(pooledBinaryWriter);
				}
			}
		}
		catch (Exception e)
		{
			Log.Exception(e);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Write(BinaryWriter _bw)
	{
		_bw.Write(2);
		int num = (this.data != null) ? this.data.Length : 0;
		_bw.Write(num);
		if (num > 0)
		{
			_bw.Write(this.data, 0, num);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cSaveVersion = 2;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i size;

	[PublicizedFrom(EAccessModifier.Private)]
	public byte[] data;
}
