using System;
using UnityEngine;

public class XUiV_Widget : XUiView
{
	public XUiV_Widget(string _id) : base(_id)
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void CreateComponents(GameObject _go)
	{
		_go.AddComponent<UIWidget>();
	}

	public override void InitView()
	{
		base.InitView();
		this.widget = this.uiTransform.gameObject.GetComponent<UIWidget>();
		this.widget.depth = this.depth;
		this.widget.pivot = this.pivot;
		base.parseAnchors(this.widget, true);
		this.RefreshBoxCollider();
	}

	public override void UpdateData()
	{
		if (this.isDirty)
		{
			this.widget.pivot = this.pivot;
			base.parseAnchors(this.widget, true);
			this.RefreshBoxCollider();
		}
		base.UpdateData();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public UIWidget widget;
}
