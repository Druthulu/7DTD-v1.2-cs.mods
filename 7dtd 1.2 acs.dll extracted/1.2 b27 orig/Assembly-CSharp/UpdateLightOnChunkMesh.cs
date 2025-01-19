using System;
using UnityEngine;

public class UpdateLightOnChunkMesh : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public void Awake()
	{
		if (GameManager.IsDedicatedServer)
		{
			base.enabled = false;
			return;
		}
		this.Reset();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void Start()
	{
		if (GameManager.IsDedicatedServer)
		{
			return;
		}
		this.checkLight();
	}

	public void SetChunkMesh(VoxelMesh _chunkMesh)
	{
		this.chunkMesh = _chunkMesh;
	}

	public void Reset()
	{
		this.chunkMesh = null;
		this.lastTimeLightBrightnessChecked = 0f;
		this.lastSunLight = (this.lastBlockLight = byte.MaxValue);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		if (GameManager.IsDedicatedServer)
		{
			return;
		}
		if (Time.time - this.lastTimeLightBrightnessChecked < this.cTimeBrightnessUpdate)
		{
			return;
		}
		this.lastTimeLightBrightnessChecked = Time.time;
		this.checkLight();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void checkLight()
	{
		if (this.chunkMesh == null)
		{
			return;
		}
		GameManager instance = GameManager.Instance;
		if (!instance || !instance.gameStateManager.IsGameStarted())
		{
			return;
		}
		World world = instance.World;
		if (world == null)
		{
			return;
		}
		if (this.meshFilter == null)
		{
			this.meshFilter = base.transform.GetComponent<MeshFilter>();
		}
		if (this.meshRenderer == null)
		{
			this.meshRenderer = base.transform.GetComponent<MeshRenderer>();
		}
		if (this.meshFilter == null || this.meshRenderer == null)
		{
			return;
		}
		byte v;
		byte v2;
		world.GetSunAndBlockColors(World.worldToBlockPos(base.transform.position + Origin.position), out v, out v2);
		byte v3;
		byte v4;
		world.GetSunAndBlockColors(World.worldToBlockPos(base.transform.position + Vector3.up + Origin.position), out v3, out v4);
		byte b = Utils.FastMax(v, v3);
		byte b2 = Utils.FastMax(v2, v4);
		Color value = world.IsDaytime() ? world.m_WorldEnvironment.GetSunLightColor() : world.m_WorldEnvironment.GetMoonLightColor();
		value.a = 1f;
		if (b != this.lastSunLight || b2 != this.lastBlockLight || !value.Equals(this.lastSunMoonLight))
		{
			this.lastSunLight = b;
			this.lastBlockLight = b2;
			this.lastSunMoonLight = value;
			this.meshFilter.mesh.colors = this.chunkMesh.UpdateColors(b, b2);
			this.meshRenderer.material.SetColor("_SunMoonlight", value);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float lastTimeLightBrightnessChecked;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float cTimeBrightnessUpdate = 0.5f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public byte lastSunLight;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public byte lastBlockLight;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Color lastSunMoonLight;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public MeshFilter meshFilter;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public MeshRenderer meshRenderer;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public VoxelMesh chunkMesh;
}
