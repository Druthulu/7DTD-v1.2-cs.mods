using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_SliderThumb : XUiController
{
	public float ThumbPosition
	{
		get
		{
			return (base.ViewComponent.UiTransform.localPosition.x - this.left) / this.width;
		}
		set
		{
			base.ViewComponent.Position = new Vector2i((int)(value * this.width + this.left), base.ViewComponent.Position.y);
			base.ViewComponent.UiTransform.localPosition = new Vector3((float)((int)(value * this.width + this.left)), (float)base.ViewComponent.Position.y, 0f);
		}
	}

	public bool IsDragging
	{
		get
		{
			return this.isDragging;
		}
	}

	public override void Init()
	{
		base.Init();
		base.ViewComponent.EventOnHover = true;
		this.sliderController = base.GetParentByType<XUiC_Slider>();
		this.sliderBarController = this.sliderController.GetChildByType<XUiC_SliderBar>();
	}

	public void SetDimensions(float _left, float _width)
	{
		this.left = _left;
		this.width = _width;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnHovered(bool _isOver)
	{
		base.OnHovered(_isOver);
		this.isOver = _isOver;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnDragged(EDragType _dragType, Vector2 _mousePositionDelta)
	{
		base.OnDragged(_dragType, _mousePositionDelta);
		if (!this.isDragging && !this.isOver)
		{
			return;
		}
		Vector2i mouseXUIPosition = base.xui.GetMouseXUIPosition();
		if (_dragType == EDragType.DragStart)
		{
			this.lastMousePos = mouseXUIPosition;
			this.isDragging = true;
		}
		else if (_dragType == EDragType.DragEnd)
		{
			this.isDragging = false;
		}
		if (mouseXUIPosition.x - this.lastMousePos.x != 0)
		{
			float num = base.ViewComponent.UiTransform.localPosition.x + (float)(mouseXUIPosition.x - this.lastMousePos.x);
			num = Mathf.Clamp(num, this.left, this.left + this.width);
			this.lastMousePos = mouseXUIPosition;
			base.ViewComponent.UiTransform.localPosition = new Vector3(num, base.ViewComponent.UiTransform.localPosition.y, base.ViewComponent.UiTransform.localPosition.z);
			base.ViewComponent.Position = new Vector2i((int)num, base.ViewComponent.Position.y);
			this.sliderController.ValueChanged(this.ThumbPosition);
			this.sliderController.IsDirty = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnScrolled(float _delta)
	{
		base.OnScrolled(_delta);
		if (this.sliderBarController != null)
		{
			this.sliderBarController.Scrolled(_delta);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2i lastMousePos = new Vector2i(-100000, -100000);

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isOver;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isDragging;

	[PublicizedFrom(EAccessModifier.Private)]
	public float left;

	[PublicizedFrom(EAccessModifier.Private)]
	public float width;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_Slider sliderController;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SliderBar sliderBarController;
}
