using System;
using System.Collections.Generic;
using System.IO;

namespace SDF
{
	public static class SdfReader
	{
		public static Dictionary<string, SdfTag> Read(Stream fs)
		{
			Dictionary<string, SdfTag> dictionary = new Dictionary<string, SdfTag>();
			using (PooledBinaryReader pooledBinaryReader = MemoryPools.poolBinaryReader.AllocSync(false))
			{
				pooledBinaryReader.SetBaseStream(fs);
				try
				{
					while (pooledBinaryReader.BaseStream.Position < pooledBinaryReader.BaseStream.Length)
					{
						SdfTagType sdfTagType = (SdfTagType)pooledBinaryReader.ReadByte();
						pooledBinaryReader.ReadInt16();
						string text = pooledBinaryReader.ReadString();
						if (sdfTagType == SdfTagType.Float)
						{
							SdfFloat value = new SdfFloat(text, pooledBinaryReader.ReadSingle());
							dictionary.Add(text, value);
						}
						else if (sdfTagType == SdfTagType.Int)
						{
							SdfInt value2 = new SdfInt(text, pooledBinaryReader.ReadInt32());
							dictionary.Add(text, value2);
						}
						else if (sdfTagType == SdfTagType.String)
						{
							pooledBinaryReader.ReadInt16();
							SdfString value3 = new SdfString(text, Utils.FromBase64(pooledBinaryReader.ReadString()));
							dictionary.Add(text, value3);
						}
						else if (sdfTagType == SdfTagType.Binary)
						{
							pooledBinaryReader.ReadInt16();
							SdfString value4 = new SdfString(text, pooledBinaryReader.ReadString());
							dictionary.Add(text, value4);
						}
						else if (sdfTagType == SdfTagType.Bool)
						{
							SdfBool value5 = new SdfBool(text, pooledBinaryReader.ReadBoolean());
							dictionary.Add(text, value5);
						}
						else if (sdfTagType == SdfTagType.ByteArray)
						{
							short count = pooledBinaryReader.ReadInt16();
							SdfByteArray value6 = new SdfByteArray(text, pooledBinaryReader.ReadBytes((int)count));
							dictionary.Add(text, value6);
						}
					}
				}
				catch
				{
				}
			}
			return dictionary;
		}
	}
}
