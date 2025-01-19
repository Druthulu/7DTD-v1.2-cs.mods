using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_UiLimitsWindow : XUiController
{
	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (this.IsDirty)
		{
			base.RefreshBindings(true);
			this.IsDirty = false;
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		int manualHeight = UnityEngine.Object.FindObjectOfType<UIRoot>().manualHeight;
		float scale = base.xui.GetScale();
		this.availableXuiHeight = (float)manualHeight / scale;
		base.RefreshBindings(true);
	}

	public override bool GetBindingValue(ref string _value, string _bindingName)
	{
		if (_bindingName.StartsWith("width_", StringComparison.Ordinal))
		{
			return this.handleArBinding(ref _value, _bindingName);
		}
		if (_bindingName == "height")
		{
			_value = Mathf.FloorToInt(this.availableXuiHeight).ToString();
			return true;
		}
		return base.GetBindingValue(ref _value, _bindingName);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool handleArBinding(ref string _value, string _bindingName)
	{
		int num = _bindingName.IndexOf('_', "width_".Length);
		if (num < 0)
		{
			return false;
		}
		ReadOnlySpan<char> s = _bindingName.AsSpan("width_".Length, num - "width_".Length);
		ReadOnlySpan<char> s2 = _bindingName.AsSpan(num + 1);
		int num2;
		if (!int.TryParse(s, out num2))
		{
			return false;
		}
		int num3;
		if (!int.TryParse(s2, out num3))
		{
			return false;
		}
		double uiSizeLimit = GameOptionsManager.GetUiSizeLimit((double)num2 / (double)num3);
		int num4 = Mathf.RoundToInt((float)((double)(this.availableXuiHeight / (float)num3 * (float)num2) / uiSizeLimit));
		if (num4 % 2 > 0)
		{
			num4--;
		}
		_value = num4.ToString();
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public float availableXuiHeight = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const string bindingWidthPrefix = "width_";
}
