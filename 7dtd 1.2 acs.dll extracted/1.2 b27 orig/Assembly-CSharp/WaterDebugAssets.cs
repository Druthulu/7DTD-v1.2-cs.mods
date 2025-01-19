﻿using System;
using UnityEngine;

public static class WaterDebugAssets
{
	public static Mesh CubeMesh
	{
		get
		{
			return WaterDebugAssets.cubeMesh.Value;
		}
	}

	public static Mesh GenerateCubeMesh()
	{
		Mesh mesh = new Mesh();
		mesh.vertices = new Vector3[]
		{
			new Vector3(-0.5f, -0.5f, -0.5f),
			new Vector3(0.5f, -0.5f, -0.5f),
			new Vector3(0.5f, 0.5f, -0.5f),
			new Vector3(-0.5f, 0.5f, -0.5f),
			new Vector3(-0.5f, 0.5f, 0.5f),
			new Vector3(0.5f, 0.5f, 0.5f),
			new Vector3(0.5f, -0.5f, 0.5f),
			new Vector3(-0.5f, -0.5f, 0.5f)
		};
		mesh.triangles = new int[]
		{
			0,
			2,
			1,
			0,
			3,
			2,
			2,
			3,
			4,
			2,
			4,
			5,
			1,
			2,
			5,
			1,
			5,
			6,
			0,
			7,
			4,
			0,
			4,
			3,
			5,
			4,
			7,
			5,
			7,
			6,
			0,
			6,
			7,
			0,
			1,
			6
		};
		mesh.Optimize();
		mesh.RecalculateNormals();
		return mesh;
	}

	public static Material DebugMaterial
	{
		get
		{
			return WaterDebugAssets.sharedMaterial.Value;
		}
	}

	public static Material CreateDebugMaterial()
	{
		return new Material(Shader.Find("Debug/DebugInstancedProcedural"))
		{
			enableInstancing = true
		};
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static Lazy<Mesh> cubeMesh = new Lazy<Mesh>(new Func<Mesh>(WaterDebugAssets.GenerateCubeMesh));

	[PublicizedFrom(EAccessModifier.Private)]
	public static Lazy<Material> sharedMaterial = new Lazy<Material>(new Func<Material>(WaterDebugAssets.CreateDebugMaterial));
}
