using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_Selector : XUiController
{
	public event XUiEvent_SelectedIndexChanged OnSelectedIndexChanged;

	public override void OnOpen()
	{
		base.OnOpen();
		this.currentValue.Text = this.selectedIndex.ToString();
	}

	public int SelectedIndex
	{
		get
		{
			return this.selectedIndex;
		}
		set
		{
			this.selectedIndex = value;
		}
	}

	public override bool ParseAttribute(string name, string value, XUiController _parent)
	{
		if (name == "min")
		{
			this.Min = int.Parse(value);
			return true;
		}
		if (!(name == "max"))
		{
			return base.ParseAttribute(name, value, _parent);
		}
		this.Max = int.Parse(value);
		return true;
	}

	public override void Init()
	{
		base.Init();
		XUiController childById = base.GetChildById("forward");
		XUiController childById2 = base.GetChildById("back");
		XUiController childById3 = base.GetChildById("currentValue");
		if (childById != null)
		{
			childById.OnPress += this.ForwardButton_OnPress;
		}
		if (childById2 != null)
		{
			childById2.OnPress += this.BackButton_OnPress;
		}
		if (childById3 != null && childById3.ViewComponent is XUiV_Label)
		{
			this.currentValue = (childById3.ViewComponent as XUiV_Label);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BackButton_OnPress(XUiController _sender, int _mouseButton)
	{
		this.selectedIndex--;
		this.BackPressed();
		this.IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ForwardButton_OnPress(XUiController _sender, int _mouseButton)
	{
		this.selectedIndex++;
		this.ForwardPressed();
		this.IsDirty = true;
	}

	public virtual void BackPressed()
	{
		if (this.selectedIndex < this.Min)
		{
			this.selectedIndex = this.Max;
		}
		this.currentValue.Text = this.selectedIndex.ToString();
		if (this.OnSelectedIndexChanged != null)
		{
			this.OnSelectedIndexChanged(this.selectedIndex);
		}
	}

	public virtual void ForwardPressed()
	{
		if (this.selectedIndex > this.Max)
		{
			this.selectedIndex = this.Min;
		}
		this.currentValue.Text = this.selectedIndex.ToString();
		if (this.OnSelectedIndexChanged != null)
		{
			this.OnSelectedIndexChanged(this.selectedIndex);
		}
	}

	public int Min;

	public int Max = int.MaxValue;

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiV_Label currentValue;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int selectedIndex;
}
