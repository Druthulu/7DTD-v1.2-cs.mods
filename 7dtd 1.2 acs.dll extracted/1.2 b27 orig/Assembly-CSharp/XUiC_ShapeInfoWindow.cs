using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ShapeInfoWindow : XUiController
{
	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (this.IsDirty && base.ViewComponent.IsVisible)
		{
			this.IsDirty = false;
		}
	}

	public override bool GetBindingValue(ref string _value, string _bindingName)
	{
		if (_bindingName == "blockname")
		{
			_value = this.shapeName;
			return true;
		}
		if (_bindingName == "blockicon")
		{
			_value = ((this.blockData == null) ? "" : this.blockData.GetIconName());
			return true;
		}
		if (!(_bindingName == "blockicontint"))
		{
			return false;
		}
		Color32 v = Color.white;
		if (this.blockData != null)
		{
			v = this.blockData.CustomIconTint;
		}
		_value = this.itemicontintcolorFormatter.Format(v);
		return true;
	}

	public void SetShape(Block _newBlockData)
	{
		this.blockData = _newBlockData;
		if (_newBlockData != null)
		{
			if (_newBlockData.GetAutoShapeType() == EAutoShapeType.None)
			{
				this.shapeName = this.blockData.GetLocalizedBlockName();
			}
			else
			{
				this.shapeName = this.blockData.GetLocalizedAutoShapeShapeName();
			}
		}
		else
		{
			this.shapeName = "";
		}
		base.RefreshBindings(false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Block blockData;

	[PublicizedFrom(EAccessModifier.Private)]
	public string shapeName = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterXuiRgbaColor itemicontintcolorFormatter = new CachedStringFormatterXuiRgbaColor();
}
