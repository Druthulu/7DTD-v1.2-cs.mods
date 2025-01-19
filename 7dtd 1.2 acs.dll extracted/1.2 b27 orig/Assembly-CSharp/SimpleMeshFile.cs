using System;
using System.IO;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public static class SimpleMeshFile
{
	public static void WriteGameObject(BinaryWriter _bw, GameObject _go)
	{
		try
		{
			_bw.Write(1835365224);
			_bw.Write(6);
			MeshFilter[] componentsInChildren = _go.GetComponentsInChildren<MeshFilter>();
			_bw.Write((short)componentsInChildren.Length);
			int num = 0;
			foreach (MeshFilter meshFilter in componentsInChildren)
			{
				_bw.Write(meshFilter.transform.name);
				SimpleMeshFile.writeMesh(_bw, meshFilter.mesh, MeshDescription.meshes[0].textureAtlas.uvMapping);
				num += meshFilter.mesh.vertexCount;
			}
			Log.Out("Saved. Meshes: " + componentsInChildren.Length.ToString() + " Vertices: " + num.ToString());
		}
		finally
		{
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void writeMesh(BinaryWriter _bw, Mesh _mesh, UVRectTiling[] _uvMapping)
	{
		try
		{
			Vector3[] vertices = _mesh.vertices;
			_bw.Write((uint)vertices.Length);
			for (int i = 0; i < vertices.Length; i++)
			{
				_bw.Write(vertices[i].x);
				_bw.Write(vertices[i].y);
				_bw.Write(vertices[i].z);
			}
			int[] indices = _mesh.GetIndices(0);
			Vector2[] uv = _mesh.uv;
			Vector2[] uv2 = _mesh.uv2;
			_bw.Write((uint)uv.Length);
			for (int j = 0; j < uv.Length; j++)
			{
				int num = (int)uv2[j].x;
				int num2 = -1;
				for (int k = 0; k < _uvMapping.Length; k++)
				{
					if (_uvMapping[k].index == num || k + 1 >= _uvMapping.Length || (float)_uvMapping[k].index + _uvMapping[k].uv.width * _uvMapping[k].uv.height > (float)num)
					{
						num2 = k;
						break;
					}
				}
				if (num2 == -1)
				{
					num2 = 0;
				}
				_bw.Write((short)num2);
				_bw.Write((byte)(num - _uvMapping[num2].index));
				bool value = (double)uv2[j].y > 0.5;
				_bw.Write(value);
				_bw.Write((ushort)(uv[j].x * 10000f));
				_bw.Write((ushort)(uv[j].y * 10000f));
			}
			_bw.Write((uint)indices.Length);
			for (int l = 0; l < indices.Length; l++)
			{
				_bw.Write((ushort)indices[l]);
			}
		}
		finally
		{
		}
	}

	public static GameObject ReadGameObject(PathAbstractions.AbstractedLocation _meshLocation, float _offsetY = 0f, Material _mat = null, bool _bTextureArray = true, bool _markMeshesNoLongerReadable = false, object _userCallbackData = null, SimpleMeshFile.GameObjectLoadedCallback _asyncCallback = null)
	{
		return SimpleMeshFile.ReadGameObject(_meshLocation.FullPath, _offsetY, _mat, _bTextureArray, _markMeshesNoLongerReadable, _userCallbackData, _asyncCallback);
	}

	public static GameObject ReadGameObject(string _filename, float _offsetY = 0f, Material _mat = null, bool _bTextureArray = true, bool _markMeshesNoLongerReadable = false, object _userCallbackData = null, SimpleMeshFile.GameObjectLoadedCallback _asyncCallback = null)
	{
		try
		{
			return SimpleMeshFile.ReadGameObject(SdFile.OpenRead(_filename), _offsetY, _mat, _bTextureArray, _markMeshesNoLongerReadable, _userCallbackData, _asyncCallback, _filename);
		}
		catch (Exception e)
		{
			Log.Error("Reading mesh " + _filename + " failed:");
			Log.Exception(e);
		}
		return null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static GameObject ReadGameObject(Stream _inputStream, float _offsetY = 0f, Material _mat = null, bool _bTextureArray = true, bool _markMeshesNoLongerReadable = false, object _userCallbackData = null, SimpleMeshFile.GameObjectLoadedCallback _asyncCallback = null, string _identifier = null)
	{
		GameObject result;
		try
		{
			if (_asyncCallback == null)
			{
				try
				{
					using (SimpleMeshFile.SimpleMeshDataArray simpleMeshDataArray = new SimpleMeshFile.SimpleMeshDataArray(Mesh.AllocateWritableMeshData(SimpleMeshFile.readLengthFromHeaderAndReset(_inputStream))))
					{
						SimpleMeshFile.readData(simpleMeshDataArray, _inputStream, _bTextureArray);
						return SimpleMeshFile.CreateUnityObjects(SimpleMeshFile.createMeshInfo(simpleMeshDataArray, _markMeshesNoLongerReadable, _offsetY, _mat));
					}
				}
				finally
				{
					if (_inputStream != null)
					{
						((IDisposable)_inputStream).Dispose();
					}
				}
			}
			SimpleMeshFile.SimpleMeshDataArray meshDatas = new SimpleMeshFile.SimpleMeshDataArray(Mesh.AllocateWritableMeshData(SimpleMeshFile.readLengthFromHeaderAndReset(_inputStream)));
			ThreadManager.AddSingleTask(new ThreadManager.TaskFunctionDelegate(SimpleMeshFile.asyncReadData), new SimpleMeshFile.AsyncReadInfo(_identifier, _inputStream, meshDatas, _offsetY, _mat, _bTextureArray, _markMeshesNoLongerReadable, _asyncCallback, _userCallbackData), null, true);
			result = null;
		}
		finally
		{
		}
		return result;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void asyncReadData(ThreadManager.TaskInfo _taskInfo)
	{
		try
		{
			SimpleMeshFile.AsyncReadInfo asyncReadInfo = (SimpleMeshFile.AsyncReadInfo)_taskInfo.parameter;
			try
			{
				using (asyncReadInfo.inputStream)
				{
					SimpleMeshFile.readData(asyncReadInfo.meshDatas, asyncReadInfo.inputStream, asyncReadInfo.bTextureArray);
					asyncReadInfo.success = true;
				}
			}
			catch (Exception e)
			{
				Log.Error((asyncReadInfo.identifier != null) ? ("Reading mesh " + asyncReadInfo.identifier + " failed:") : "Reading mesh failed:");
				Log.Exception(e);
			}
			ThreadManager.AddSingleTaskMainThread("SimpleMeshFile.CreateGameObjects", new ThreadManager.MainThreadTaskFunctionDelegate(SimpleMeshFile.mainThreadGoCallback), asyncReadInfo);
		}
		finally
		{
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void mainThreadGoCallback(object _parameter)
	{
		try
		{
			SimpleMeshFile.AsyncReadInfo asyncReadInfo = (SimpleMeshFile.AsyncReadInfo)_parameter;
			using (asyncReadInfo.meshDatas)
			{
				GameObject go = null;
				if (asyncReadInfo.success)
				{
					go = SimpleMeshFile.CreateUnityObjects(SimpleMeshFile.createMeshInfo(asyncReadInfo.meshDatas, asyncReadInfo.markMeshesNoLongerReadable, asyncReadInfo.offsetY, asyncReadInfo.mat));
				}
				asyncReadInfo.goCallback(go, asyncReadInfo.userCallbackData);
			}
		}
		finally
		{
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static int readLengthFromHeaderAndReset(Stream _inputStream)
	{
		long position = _inputStream.Position;
		int result;
		using (PooledBinaryReader pooledBinaryReader = MemoryPools.poolBinaryReader.AllocSync(true))
		{
			pooledBinaryReader.SetBaseStream(_inputStream);
			pooledBinaryReader.ReadInt32();
			pooledBinaryReader.ReadByte();
			int num = (int)pooledBinaryReader.ReadInt16();
			_inputStream.Seek(position, SeekOrigin.Begin);
			result = num;
		}
		return result;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void readData(SimpleMeshFile.SimpleMeshDataArray _meshDatas, Stream _inputStream, bool _bTextureArray)
	{
		try
		{
			using (PooledBinaryReader pooledBinaryReader = MemoryPools.poolBinaryReader.AllocSync(true))
			{
				pooledBinaryReader.SetBaseStream(_inputStream);
				pooledBinaryReader.ReadInt32();
				int version = (int)pooledBinaryReader.ReadByte();
				int num = (int)pooledBinaryReader.ReadInt16();
				new Mesh.MeshData[num];
				UVRectTiling[] uvMapping = (MeshDescription.meshes.Length != 0) ? MeshDescription.meshes[0].textureAtlas.uvMapping : new UVRectTiling[0];
				for (int i = 0; i < num; i++)
				{
					SimpleMeshFile.SimpleMeshDataArray.ReadFromReader(_meshDatas[i], version, pooledBinaryReader, uvMapping, _bTextureArray);
				}
			}
		}
		finally
		{
		}
	}

	public static GameObject CreateUnityObjects(SimpleMeshInfo _meshInfo)
	{
		GameObject result;
		try
		{
			Mesh[] meshes = _meshInfo.meshes;
			string[] meshNames = _meshInfo.meshNames;
			float offsetY = _meshInfo.offsetY;
			Material material = _meshInfo.mat;
			if (!material)
			{
				material = MeshDescription.GetOpaqueMaterial();
			}
			GameObject gameObject = new GameObject();
			Transform transform = gameObject.transform;
			Vector3 localPosition = new Vector3(0f, offsetY, 0f);
			for (int i = 0; i < meshes.Length; i++)
			{
				GameObject gameObject2 = new GameObject(meshNames[i]);
				gameObject2.AddComponent<MeshFilter>().mesh = meshes[i];
				gameObject2.AddComponent<MeshRenderer>().material = material;
				Transform transform2 = gameObject2.transform;
				transform2.SetParent(transform, false);
				transform2.localPosition = localPosition;
			}
			result = gameObject;
		}
		finally
		{
		}
		return result;
	}

	public static Mesh[] ReadMesh(string _filename, float _offsetY = 0f, Material _mat = null, bool _bTextureArray = true, bool _markMeshesNoLongerReadable = false, object _userCallbackData = null, SimpleMeshFile.GameObjectMeshesReadCallback _asyncCallback = null)
	{
		Mesh[] result;
		try
		{
			result = SimpleMeshFile.ReadMesh(SdFile.OpenRead(_filename), _offsetY, _mat, _bTextureArray, _markMeshesNoLongerReadable, _userCallbackData, _asyncCallback, _filename);
		}
		finally
		{
		}
		return result;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static Mesh[] ReadMesh(Stream _inputStream, float _offsetY = 0f, Material _mat = null, bool _bTextureArray = true, bool _markMeshesNoLongerReadable = false, object _userCallbackData = null, SimpleMeshFile.GameObjectMeshesReadCallback _asyncCallback = null, string _identifier = null)
	{
		if (_asyncCallback == null)
		{
			try
			{
				using (SimpleMeshFile.SimpleMeshDataArray simpleMeshDataArray = new SimpleMeshFile.SimpleMeshDataArray(Mesh.AllocateWritableMeshData(SimpleMeshFile.readLengthFromHeaderAndReset(_inputStream))))
				{
					SimpleMeshFile.readData(simpleMeshDataArray, _inputStream, _bTextureArray);
					return SimpleMeshFile.SimpleMeshDataArray.ToMeshes(simpleMeshDataArray, _markMeshesNoLongerReadable);
				}
			}
			finally
			{
				if (_inputStream != null)
				{
					((IDisposable)_inputStream).Dispose();
				}
			}
		}
		SimpleMeshFile.SimpleMeshDataArray meshDatas = new SimpleMeshFile.SimpleMeshDataArray(Mesh.AllocateWritableMeshData(SimpleMeshFile.readLengthFromHeaderAndReset(_inputStream)));
		ThreadManager.AddSingleTask(new ThreadManager.TaskFunctionDelegate(SimpleMeshFile.asyncReadMesh), new SimpleMeshFile.AsyncReadInfo(_identifier, _inputStream, meshDatas, _offsetY, _mat, _bTextureArray, _markMeshesNoLongerReadable, _asyncCallback, _userCallbackData), null, true);
		return null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void asyncReadMesh(ThreadManager.TaskInfo _taskInfo)
	{
		SimpleMeshFile.AsyncReadInfo asyncReadInfo = (SimpleMeshFile.AsyncReadInfo)_taskInfo.parameter;
		try
		{
			using (asyncReadInfo.inputStream)
			{
				SimpleMeshFile.readData(asyncReadInfo.meshDatas, asyncReadInfo.inputStream, asyncReadInfo.bTextureArray);
				asyncReadInfo.success = true;
			}
		}
		catch (Exception e)
		{
			Log.Error((asyncReadInfo.identifier != null) ? ("Reading mesh " + asyncReadInfo.identifier + " failed:") : "Reading mesh failed:");
			Log.Exception(e);
		}
		ThreadManager.AddSingleTaskMainThread("SimpleMeshFile.ReadMesh", new ThreadManager.MainThreadTaskFunctionDelegate(SimpleMeshFile.mainThreadMeshCallback), asyncReadInfo);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void mainThreadMeshCallback(object _parameter)
	{
		SimpleMeshFile.AsyncReadInfo asyncReadInfo = (SimpleMeshFile.AsyncReadInfo)_parameter;
		using (SimpleMeshFile.SimpleMeshDataArray meshDatas = asyncReadInfo.meshDatas)
		{
			SimpleMeshInfo meshInfo = SimpleMeshFile.createMeshInfo(meshDatas, asyncReadInfo.markMeshesNoLongerReadable, asyncReadInfo.offsetY, asyncReadInfo.mat);
			asyncReadInfo.meshCallback(meshInfo, asyncReadInfo.userCallbackData);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static SimpleMeshInfo createMeshInfo(SimpleMeshFile.SimpleMeshDataArray _meshDatas, bool _markMeshesNoLongerReadable, float _offsetY, Material _mat)
	{
		string[] array = new string[_meshDatas.Length];
		Mesh[] meshes = SimpleMeshFile.SimpleMeshDataArray.ToMeshes(_meshDatas, _markMeshesNoLongerReadable);
		for (int i = 0; i < _meshDatas.Length; i++)
		{
			array[i] = _meshDatas[i].Name;
		}
		return new SimpleMeshInfo(array, meshes, _offsetY, _mat);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cSaveFileVersion = 6;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cHeader = 1835365224;

	public delegate void GameObjectLoadedCallback(GameObject _go, object _userCallbackData);

	[PublicizedFrom(EAccessModifier.Private)]
	public class AsyncReadInfo
	{
		public AsyncReadInfo(string _identifier, Stream _inputStream, SimpleMeshFile.SimpleMeshDataArray _meshDatas, float _offsetY, Material _mat, bool _bTextureArray, bool _markMeshesNoLongerReadable, SimpleMeshFile.GameObjectMeshesReadCallback _callback, object _userCallbackData) : this(_identifier, _inputStream, _meshDatas, _offsetY, _mat, _bTextureArray, _markMeshesNoLongerReadable)
		{
			this.meshCallback = _callback;
			this.userCallbackData = _userCallbackData;
		}

		public AsyncReadInfo(string _identifier, Stream _inputStream, SimpleMeshFile.SimpleMeshDataArray _meshDatas, float _offsetY, Material _mat, bool _bTextureArray, bool _markMeshesNoLongerReadable, SimpleMeshFile.GameObjectLoadedCallback _callback, object _userCallbackData) : this(_identifier, _inputStream, _meshDatas, _offsetY, _mat, _bTextureArray, _markMeshesNoLongerReadable)
		{
			this.goCallback = _callback;
			this.userCallbackData = _userCallbackData;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public AsyncReadInfo(string _identifier, Stream _inputStream, SimpleMeshFile.SimpleMeshDataArray _meshDatas, float _offsetY, Material _mat, bool _bTextureArray, bool _markMeshesNoLongerReadable)
		{
			this.identifier = _identifier;
			this.inputStream = _inputStream;
			this.meshDatas = _meshDatas;
			this.offsetY = _offsetY;
			this.mat = _mat;
			this.bTextureArray = _bTextureArray;
			this.markMeshesNoLongerReadable = _markMeshesNoLongerReadable;
			this.success = false;
		}

		public readonly string identifier;

		public readonly Stream inputStream;

		public readonly SimpleMeshFile.SimpleMeshDataArray meshDatas;

		public readonly float offsetY;

		public readonly Material mat;

		public readonly bool bTextureArray;

		public readonly bool markMeshesNoLongerReadable;

		public readonly SimpleMeshFile.GameObjectLoadedCallback goCallback;

		public readonly SimpleMeshFile.GameObjectMeshesReadCallback meshCallback;

		public readonly object userCallbackData;

		public bool success;
	}

	public delegate void GameObjectMeshesReadCallback(SimpleMeshInfo _meshInfo, object _userCallbackData);

	[PublicizedFrom(EAccessModifier.Private)]
	public sealed class SimpleMeshDataArray : IDisposable
	{
		public SimpleMeshDataArray(Mesh.MeshDataArray _array)
		{
			this.meshData = _array;
			this.names = new string[this.meshData.Length];
			this.wrappers = new SimpleMeshFile.SimpleMeshDataArray.SimpleMeshDataWrapper[this.meshData.Length];
			for (int i = 0; i < this.wrappers.Length; i++)
			{
				this.wrappers[i] = new SimpleMeshFile.SimpleMeshDataArray.SimpleMeshDataWrapper(this, i);
			}
		}

		public void Dispose()
		{
			if (this.disposed)
			{
				return;
			}
			this.disposed = true;
			this.DisposeMeshData();
			this.names = null;
			this.wrappers = null;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void DisposeMeshData()
		{
			if (this.meshDataDisposed)
			{
				return;
			}
			this.meshDataDisposed = true;
			this.meshData.Dispose();
			this.meshData = default(Mesh.MeshDataArray);
		}

		public int Length
		{
			get
			{
				return this.meshData.Length;
			}
		}

		public SimpleMeshFile.SimpleMeshDataArray.SimpleMeshDataWrapper this[int i]
		{
			get
			{
				return this.wrappers[i];
			}
		}

		public void ApplyAndDisposeWritableMeshData(Mesh[] meshes, MeshUpdateFlags flags = MeshUpdateFlags.Default)
		{
			Mesh.ApplyAndDisposeWritableMeshData(this.meshData, meshes, flags);
			this.meshDataDisposed = true;
		}

		public static void ReadFromReader(SimpleMeshFile.SimpleMeshDataArray.SimpleMeshDataWrapper _meshDataWrapper, int _version, BinaryReader _br, UVRectTiling[] _uvMapping, bool _bTextureArray)
		{
			try
			{
				Mesh.MeshData meshData = _meshDataWrapper.MeshData;
				_meshDataWrapper.Name = ((_version > 1) ? _br.ReadString() : "mesh");
				long position = _br.BaseStream.Position;
				int num = (int)_br.ReadUInt32();
				int num2;
				if (_version < 6)
				{
					num2 = 6;
				}
				else
				{
					num2 = 12;
				}
				_br.BaseStream.Seek((long)(num * num2), SeekOrigin.Current);
				int num3 = (int)_br.ReadUInt32();
				int num4 = 0;
				if (_version > 2)
				{
					num4 += 2;
				}
				if (_version > 4)
				{
					num4++;
				}
				if (_version > 3)
				{
					num4++;
				}
				num4 += 4;
				_br.BaseStream.Seek((long)(num3 * num4), SeekOrigin.Current);
				int num5 = (int)_br.ReadUInt32();
				int num6 = 2;
				_br.BaseStream.Seek((long)(num5 * num6), SeekOrigin.Current);
				_br.BaseStream.Seek(position, SeekOrigin.Begin);
				VertexAttributeDescriptor[] attributes = new VertexAttributeDescriptor[]
				{
					new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3, 0),
					new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float32, 3, 1),
					new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2, 2),
					new VertexAttributeDescriptor(VertexAttribute.Color, VertexAttributeFormat.Float32, 4, 3)
				};
				meshData.SetVertexBufferParams(num, attributes);
				meshData.SetIndexBufferParams(num5, IndexFormat.UInt16);
				num = (int)_br.ReadUInt32();
				NativeArray<Vector3> vertexData = meshData.GetVertexData<Vector3>(0);
				for (int i = 0; i < num; i++)
				{
					if (_version < 6)
					{
						vertexData[i] = new Vector3((float)_br.ReadInt16() / 100f, (float)_br.ReadInt16() / 100f, (float)_br.ReadInt16() / 100f);
					}
					else
					{
						vertexData[i] = new Vector3(_br.ReadSingle(), _br.ReadSingle(), _br.ReadSingle());
					}
				}
				num3 = (int)_br.ReadUInt32();
				NativeArray<Vector2> vertexData2 = meshData.GetVertexData<Vector2>(2);
				NativeArray<Color> vertexData3 = meshData.GetVertexData<Color>(3);
				for (int j = 0; j < num3; j++)
				{
					int num7 = 0;
					int num8 = 0;
					if (_version > 2)
					{
						num7 = (int)_br.ReadInt16();
					}
					if (_version > 4)
					{
						num8 = (int)_br.ReadByte();
					}
					int num9 = (num7 >= 0 && num7 < _uvMapping.Length) ? (_uvMapping[num7].index + num8) : 0;
					bool flag = false;
					if (_version > 3)
					{
						flag = _br.ReadBoolean();
					}
					vertexData2[j] = new Vector2((float)_br.ReadUInt16() / 10000f, (float)_br.ReadUInt16() / 10000f);
					if (!_bTextureArray && num9 >= 0 && num9 < _uvMapping.Length)
					{
						ref NativeArray<Vector2> ptr = ref vertexData2;
						int index = j;
						ptr[index] += new Vector2(_uvMapping[num9].uv.x, _uvMapping[num9].uv.y);
					}
					vertexData3[j] = new Color(0f, (float)num9, 0f, (float)(flag ? 1 : 0));
				}
				num5 = (int)_br.ReadUInt32();
				NativeArray<ushort> indexData = meshData.GetIndexData<ushort>();
				for (int k = 0; k < num5; k++)
				{
					indexData[k] = _br.ReadUInt16();
				}
				meshData.subMeshCount = 1;
				meshData.SetSubMesh(0, new SubMeshDescriptor(0, num5, MeshTopology.Triangles), MeshUpdateFlags.Default);
			}
			finally
			{
			}
		}

		public static Mesh[] ToMeshes(SimpleMeshFile.SimpleMeshDataArray _meshDataArray, bool _markMeshesNoLongerReadable)
		{
			Mesh[] array = new Mesh[_meshDataArray.Length];
			for (int i = 0; i < array.Length; i++)
			{
				Mesh mesh = new Mesh();
				array[i] = mesh;
				mesh.name = "Simple";
			}
			_meshDataArray.ApplyAndDisposeWritableMeshData(array, MeshUpdateFlags.Default);
			foreach (Mesh mesh2 in array)
			{
				mesh2.RecalculateNormals();
				mesh2.RecalculateBounds();
				GameUtils.SetMeshVertexAttributes(mesh2, true);
				mesh2.UploadMeshData(_markMeshesNoLongerReadable);
			}
			return array;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public bool disposed;

		[PublicizedFrom(EAccessModifier.Private)]
		public Mesh.MeshDataArray meshData;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool meshDataDisposed;

		[PublicizedFrom(EAccessModifier.Private)]
		public string[] names;

		[PublicizedFrom(EAccessModifier.Private)]
		public SimpleMeshFile.SimpleMeshDataArray.SimpleMeshDataWrapper[] wrappers;

		public readonly struct SimpleMeshDataWrapper
		{
			public SimpleMeshDataWrapper(SimpleMeshFile.SimpleMeshDataArray _array, int _offset)
			{
				this.array = _array;
				this.offset = _offset;
			}

			public Mesh.MeshData MeshData
			{
				get
				{
					return this.array.meshData[this.offset];
				}
			}

			public string Name
			{
				get
				{
					return this.array.names[this.offset];
				}
				set
				{
					this.array.names[this.offset] = value;
				}
			}

			[PublicizedFrom(EAccessModifier.Private)]
			public readonly SimpleMeshFile.SimpleMeshDataArray array;

			[PublicizedFrom(EAccessModifier.Private)]
			public readonly int offset;
		}
	}
}
