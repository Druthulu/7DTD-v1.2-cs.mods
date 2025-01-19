using System;
using UnityEngine;

public class XUiV_Rect : XUiView
{
	public XUiV_Rect(string _id) : base(_id)
	{
	}

	public bool DisableFallthrough
	{
		get
		{
			return this.disableFallthrough;
		}
		set
		{
			this.disableFallthrough = value;
		}
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
		if (this.createUiWidget)
		{
			this.widget.enabled = true;
			UIWidget uiwidget = this.widget;
			uiwidget.onChange = (UIWidget.OnDimensionsChanged)Delegate.Combine(uiwidget.onChange, new UIWidget.OnDimensionsChanged(delegate()
			{
				this.isDirty = true;
			}));
		}
		else
		{
			UnityEngine.Object.Destroy(this.widget);
			this.widget = null;
		}
		this.UpdateData();
	}

	public override void UpdateData()
	{
		if (!this.initialized)
		{
			this.initialized = true;
			if (this.widget != null)
			{
				this.uiTransform.localScale = Vector3.one;
				this.uiTransform.localPosition = new Vector3((float)this.position.x, (float)this.position.y, 0f);
			}
		}
		if (this.widget != null)
		{
			this.widget.pivot = this.pivot;
			this.widget.depth = this.depth;
			this.widget.keepAspectRatio = this.keepAspectRatio;
			this.widget.aspectRatio = this.aspectRatio;
			this.widget.autoResizeBoxCollider = true;
			base.parseAnchors(this.widget, true);
		}
		base.UpdateData();
	}

	public override void RefreshBoxCollider()
	{
		base.RefreshBoxCollider();
		if (this.disableFallthrough)
		{
			BoxCollider collider = this.collider;
			if (collider != null)
			{
				int num = 100;
				Vector3 center = collider.center;
				center.z = (float)num;
				collider.center = center;
			}
		}
	}

	public override bool ParseAttribute(string attribute, string value, XUiController _parent)
	{
		bool flag = base.ParseAttribute(attribute, value, _parent);
		if (!flag)
		{
			if (!(attribute == "disablefallthrough"))
			{
				if (!(attribute == "createuiwidget"))
				{
					return false;
				}
				this.createUiWidget = StringParsers.ParseBool(value, 0, -1, true);
			}
			else
			{
				this.DisableFallthrough = StringParsers.ParseBool(value, 0, -1, true);
			}
			return true;
		}
		return flag;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool createUiWidget;

	[PublicizedFrom(EAccessModifier.Protected)]
	public UIWidget widget;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool disableFallthrough;
}
