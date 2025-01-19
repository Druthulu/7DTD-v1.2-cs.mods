using System;
using System.Threading;

public class VoxelMeshLayer : IMemoryPoolableObject
{
	public VoxelMeshLayer()
	{
		this.idx = 0;
		this.meshes = new VoxelMesh[MeshDescription.meshes.Length];
		for (int i = 0; i < MeshDescription.meshes.Length; i++)
		{
			this.meshes[i] = VoxelMesh.Create(i, MeshDescription.meshes[i].meshType, 0);
		}
		Interlocked.Increment(ref VoxelMeshLayer.InstanceCount);
	}

	public void Reset()
	{
		for (int i = 0; i < this.meshes.Length; i++)
		{
			this.meshes[i].ClearMesh();
		}
	}

	public void Cleanup()
	{
		Interlocked.Decrement(ref VoxelMeshLayer.InstanceCount);
	}

	public static void StaticCleanup()
	{
		VoxelMeshLayer.InstanceCount = 0;
	}

	public void SizeToChunkDefaults()
	{
		for (int i = 0; i < this.meshes.Length; i++)
		{
			this.meshes[i].SizeToChunkDefaults(i);
		}
	}

	public bool HasContent()
	{
		for (int i = 0; i < this.meshes.Length; i++)
		{
			if (this.meshes[i].Vertices.Count > 0)
			{
				return true;
			}
		}
		return false;
	}

	public int GetTris()
	{
		int num = 0;
		for (int i = 0; i < this.meshes.Length; i++)
		{
			num += this.meshes[i].Triangles;
		}
		return num;
	}

	public int GetTrisInMesh(int _idx)
	{
		return this.meshes[_idx].Triangles;
	}

	public int GetSizeOfMesh(int _idx)
	{
		return this.meshes[_idx].Size;
	}

	public int idx;

	public VoxelMesh[] meshes;

	public static int InstanceCount;
}
