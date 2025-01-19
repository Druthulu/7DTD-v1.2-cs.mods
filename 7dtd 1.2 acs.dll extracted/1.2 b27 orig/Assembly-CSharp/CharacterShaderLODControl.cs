using System;
using System.Collections.Generic;
using UnityEngine;

public class CharacterShaderLODControl : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Private)]
	public void Start()
	{
		Renderer[] componentsInChildren = base.GetComponentsInChildren<Renderer>();
		this.materials = new List<Material>();
		Renderer[] array = componentsInChildren;
		for (int i = 0; i < array.Length; i++)
		{
			foreach (Material material in array[i].materials)
			{
				if (material.shader.name.Contains("Game/SDCS/"))
				{
					this.materials.Add(material);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		if (Camera.main == null)
		{
			return;
		}
		int maximumLOD;
		if (Vector3.Distance(Camera.main.transform.position, base.transform.position) <= this.transitionDistance)
		{
			maximumLOD = 200;
		}
		else
		{
			maximumLOD = 100;
		}
		foreach (Material material in this.materials)
		{
			material.shader.maximumLOD = maximumLOD;
		}
	}

	public float transitionDistance = 5f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public List<Material> materials;
}
