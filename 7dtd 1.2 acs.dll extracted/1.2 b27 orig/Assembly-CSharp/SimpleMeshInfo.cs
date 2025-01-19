using System;
using UnityEngine;

public class SimpleMeshInfo
{
	public SimpleMeshInfo(string[] _meshNames, Mesh[] _meshes, float _offsetY, Material _mat)
	{
		this.meshNames = _meshNames;
		this.meshes = _meshes;
		this.offsetY = _offsetY;
		this.mat = _mat;
	}

	public string[] meshNames;

	public Mesh[] meshes;

	public readonly float offsetY;

	public readonly Material mat;
}
