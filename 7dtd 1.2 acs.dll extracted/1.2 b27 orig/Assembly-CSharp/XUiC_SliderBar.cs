using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_SliderBar : XUiController
{
	public override void Init()
	{
		base.Init();
		this.sliderController = base.GetParentByType<XUiC_Slider>();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnPressed(int _mouseButton)
	{
		Vector2i mouseXUIPosition = base.xui.GetMouseXUIPosition();
		XUiController xuiController = this;
		Vector2i vector2i = xuiController.ViewComponent.Position;
		while (xuiController.Parent != null && xuiController.Parent.ViewComponent != null)
		{
			xuiController = xuiController.Parent;
			vector2i += xuiController.ViewComponent.Position;
		}
		vector2i += new Vector2i((int)xuiController.ViewComponent.UiTransform.parent.localPosition.x, (int)xuiController.ViewComponent.UiTransform.parent.localPosition.y);
		int num = (vector2i + base.ViewComponent.Size).x - vector2i.x;
		float newVal = (float)(mouseXUIPosition.x - vector2i.x) / (float)num;
		this.sliderController.ValueChanged(newVal);
		this.sliderController.updateThumb();
		this.sliderController.IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnScrolled(float _delta)
	{
		base.OnScrolled(_delta);
		float num = this.sliderController.Value;
		num += Mathf.Clamp(_delta, -this.sliderController.Step, this.sliderController.Step);
		this.sliderController.ValueChanged(num);
		this.sliderController.updateThumb();
		this.sliderController.IsDirty = true;
	}

	public void PageUpAction()
	{
		this.OnScrolled(this.sliderController.Step);
	}

	public void PageDownAction()
	{
		this.OnScrolled(-this.sliderController.Step);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_Slider sliderController;
}
