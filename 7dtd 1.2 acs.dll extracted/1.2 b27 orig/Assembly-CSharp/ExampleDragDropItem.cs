﻿using System;
using UnityEngine;

[AddComponentMenu("NGUI/Examples/Drag and Drop Item (Example)")]
public class ExampleDragDropItem : UIDragDropItem
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnDragDropRelease(GameObject surface)
	{
		if (surface != null)
		{
			ExampleDragDropSurface component = surface.GetComponent<ExampleDragDropSurface>();
			if (component != null)
			{
				GameObject gameObject = component.gameObject.AddChild(this.prefab);
				gameObject.transform.localScale = component.transform.localScale;
				Transform transform = gameObject.transform;
				transform.position = UICamera.lastWorldPosition;
				if (component.rotatePlacedObject)
				{
					transform.rotation = Quaternion.LookRotation(UICamera.lastHit.normal) * Quaternion.Euler(90f, 0f, 0f);
				}
				NGUITools.Destroy(base.gameObject);
				return;
			}
		}
		base.OnDragDropRelease(surface);
	}

	public GameObject prefab;
}
