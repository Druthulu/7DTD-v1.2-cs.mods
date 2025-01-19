using System;
using System.IO;

namespace SDF
{
	public class SdfFile
	{
		public void Open(string path)
		{
			try
			{
				this.data = new SdfData();
				this.filePath = path;
				this.valuesChanged = false;
				if (!SdDirectory.Exists(Path.GetDirectoryName(path)))
				{
					SdDirectory.CreateDirectory(Path.GetDirectoryName(path));
				}
				using (Stream stream = SdFile.Open(this.filePath, FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read))
				{
					this.data.Nodes = SdfReader.Read(stream);
				}
			}
			catch (Exception ex)
			{
				Log.Error("Error opening SDF file: " + ex.Message);
			}
		}

		public void Close()
		{
			try
			{
				if (this.valuesChanged)
				{
					using (Stream stream = SdFile.Open(this.filePath, FileMode.Create, FileAccess.Write, FileShare.Read))
					{
						SdfWriter.Write(stream, this.data.Nodes);
					}
				}
			}
			catch (Exception e)
			{
				Log.Error("Error opening SDF file:");
				Log.Exception(e);
			}
		}

		public void SaveAndKeepOpen()
		{
			this.Close();
			this.Open(this.filePath);
		}

		public void Set(string name, int val)
		{
			this.data.Add(new SdfInt(name, val));
			this.valuesChanged = true;
		}

		public void Set(string name, float val)
		{
			this.data.Add(new SdfFloat(name, val));
			this.valuesChanged = true;
		}

		public void Set(string name, string val)
		{
			this.Set(name, val, false);
		}

		public void Set(string name, string val, bool isBinary)
		{
			if (!isBinary)
			{
				this.data.Add(new SdfString(name, val));
			}
			else
			{
				this.data.Add(new SdfBinary(name, val));
			}
			this.valuesChanged = true;
		}

		public void Set(string name, byte[] byteArray)
		{
			this.data.Add(new SdfByteArray(name, byteArray));
		}

		public void Set(string name, bool val)
		{
			this.data.Add(new SdfBool(name, val));
			this.valuesChanged = true;
		}

		public float? GetFloat(string name)
		{
			return this.data.GetFloat(name);
		}

		public int? GetInt(string name)
		{
			return this.data.GetInt(name);
		}

		public string GetString(string name)
		{
			return this.GetString(name, false);
		}

		public string GetString(string name, bool isBinary)
		{
			if (!isBinary)
			{
				return this.data.GetString(name);
			}
			return Utils.FromBase64(this.data.GetString(name));
		}

		public byte[] GetByteArray(string name)
		{
			return this.data.GetByteArray(name);
		}

		public bool? GetBool(string name)
		{
			return this.data.GetBool(name);
		}

		public void Remove(string name)
		{
			this.data.Remove(name);
			this.valuesChanged = true;
		}

		public string[] GetKeys()
		{
			string[] array = new string[this.data.Nodes.Count];
			this.data.Nodes.CopyKeysTo(array);
			return array;
		}

		public string[] GetStoredGamePrefs()
		{
			string[] array = new string[this.data.Nodes.Count];
			this.data.Nodes.CopyKeysTo(array);
			return array;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public SdfData data;

		[PublicizedFrom(EAccessModifier.Private)]
		public string filePath;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool valuesChanged;
	}
}
