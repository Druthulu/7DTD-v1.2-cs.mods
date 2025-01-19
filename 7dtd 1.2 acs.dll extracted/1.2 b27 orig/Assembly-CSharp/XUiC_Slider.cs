using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_Slider : XUiController
{
	public event XUiEvent_SliderValueChanged OnValueChanged;

	public Func<float, string> ValueFormatter
	{
		get
		{
			return this.valueFormatter;
		}
		set
		{
			this.valueFormatter = value;
			this.IsDirty = true;
		}
	}

	public string Label
	{
		get
		{
			return this.name;
		}
		set
		{
			this.name = value;
			this.IsDirty = true;
		}
	}

	public float Value
	{
		get
		{
			return this.val;
		}
		set
		{
			if (!this.thumbController.IsDragging && value != this.val)
			{
				this.val = Mathf.Clamp01(value);
				this.updateThumb();
				this.IsDirty = true;
			}
		}
	}

	public override void Init()
	{
		base.Init();
		this.thumbController = (base.GetChildById("thumb") as XUiC_SliderThumb);
		if (this.thumbController == null)
		{
			Log.Error("Thumb slider not found!");
			return;
		}
		this.thumbController.ViewComponent.IsNavigatable = (this.thumbController.ViewComponent.IsSnappable = false);
		this.barController = (base.GetChildById("bar") as XUiC_SliderBar);
		if (this.barController == null)
		{
			Log.Error("Thumb bar not found!");
			return;
		}
		this.left = (float)this.barController.ViewComponent.Position.x;
		this.width = (float)this.barController.ViewComponent.Size.x;
		this.thumbController.SetDimensions(this.left, this.width);
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (this.thumbController == null || this.thumbController.ViewComponent == null || float.IsNaN(this.left))
		{
			return;
		}
		if (this.IsDirty)
		{
			base.RefreshBindings(false);
			this.IsDirty = false;
		}
		if (base.xui.playerUI.CursorController.navigationTarget == this.barController.ViewComponent)
		{
			XUi.HandlePaging(base.xui, new Action(this.barController.PageUpAction), new Action(this.barController.PageDownAction), false);
		}
	}

	public void Reset()
	{
		this.initialized = false;
	}

	public override bool GetBindingValue(ref string value, string bindingName)
	{
		if (bindingName == "name")
		{
			value = this.name;
			return true;
		}
		if (!(bindingName == "value"))
		{
			return false;
		}
		if (this.valueFormatter != null)
		{
			value = this.valueFormatter(this.val);
		}
		else
		{
			value = this.internalValueFormatter.Format(this.val);
		}
		return true;
	}

	public void ValueChanged(float _newVal)
	{
		this.val = Mathf.Clamp01(_newVal);
		if (this.OnValueChanged != null)
		{
			this.OnValueChanged(this);
		}
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public void updateThumb()
	{
		this.thumbController.ThumbPosition = this.val;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SliderThumb thumbController;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SliderBar barController;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string name;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float val;

	[PublicizedFrom(EAccessModifier.Protected)]
	public Func<float, string> valueFormatter;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool initialized;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float left = float.NaN;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float width;

	public string Tag;

	public float Step = 0.1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterFloat internalValueFormatter = new CachedStringFormatterFloat("0.00");
}
