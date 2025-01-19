﻿using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class EModelPlayer : EModelBase
{
	public override void SetSkinTexture(string _textureName)
	{
		base.SetSkinTexture(_textureName);
		Transform transform = this.modelTransformParent.Find(this.modelTransformParent.GetChild(0).name);
		Transform transform2;
		if (transform != null && (transform2 = transform.Find("body")) != null)
		{
			Texture2D mainTexture = DataLoader.LoadAsset<Texture2D>(DataLoader.IsInResources(_textureName) ? ("Entities/" + _textureName) : _textureName);
			transform2.GetComponent<Renderer>().material.mainTexture = mainTexture;
		}
	}
}
