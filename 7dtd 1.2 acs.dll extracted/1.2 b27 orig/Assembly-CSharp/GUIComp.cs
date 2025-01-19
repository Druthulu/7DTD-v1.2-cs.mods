using System;
using UnityEngine;

public abstract class GUIComp
{
	public abstract void OnGUI();

	public virtual void OnGUILayout()
	{
	}

	public void SetPosition(int _x, int _y)
	{
		this.rect.x = (float)_x;
		this.rect.y = (float)_y;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public GUIComp()
	{
	}

	public Rect rect;
}
