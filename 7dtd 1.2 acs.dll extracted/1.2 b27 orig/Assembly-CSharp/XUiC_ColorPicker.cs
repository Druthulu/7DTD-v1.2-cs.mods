using System;
using System.Collections;
using GUI_2;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ColorPicker : XUiController
{
	public event XUiEvent_SelectedColorChanged OnSelectedColorChanged;

	public Color SelectedColor
	{
		get
		{
			return this.selectedColor;
		}
		set
		{
			this.selectedColor = value;
			this.hue = (float)HSVUtil.ConvertRgbToHsv(this.selectedColor).H;
			this.saturation = (float)HSVUtil.ConvertRgbToHsv(this.selectedColor).S;
			this.vibrance = (float)HSVUtil.ConvertRgbToHsv(this.selectedColor).V;
			this.SetupSaturationVibranceTexture();
			float num = this.saturation * (float)this.svPicker.Size.x;
			float num2 = this.vibrance * (float)this.svPicker.Size.y;
			this.selectedColorSVPointerSprite.UiTransform.gameObject.SetActive(true);
			this.selectedColorSVPointerSprite.Position = new Vector2i((int)num, -(int)num2);
			this.selectedColorSVPointerSprite.UiTransform.localPosition = new Vector3(num, -num2);
			this.selectedColorSprite.Color = this.selectedColor;
		}
	}

	public override void Init()
	{
		base.Init();
		XUiController childById = base.GetChildById("svPicker");
		XUiController childById2 = base.GetChildById("hPicker");
		XUiController childById3 = base.GetChildById("selectedColorSVPointer");
		if (childById != null && childById.ViewComponent is XUiV_Texture)
		{
			this.svPicker = (childById.ViewComponent as XUiV_Texture);
			childById.OnPress += this.SvPickerC_OnPress;
			childById.OnDrag += this.SvPickerC_OnDrag;
		}
		if (childById2 != null && childById2.ViewComponent is XUiV_Texture)
		{
			this.hPicker = (childById2.ViewComponent as XUiV_Texture);
			childById2.OnPress += this.HPickerC_OnPress;
			childById2.OnDrag += this.HPickerC_OnDrag;
		}
		XUiController childById4 = base.GetChildById("selectedColor");
		if (childById4 != null && childById4.ViewComponent is XUiV_Sprite)
		{
			this.selectedColorSprite = (childById4.ViewComponent as XUiV_Sprite);
		}
		if (childById3 != null && childById3.ViewComponent is XUiV_Sprite)
		{
			this.selectedColorSVPointerSprite = (childById3.ViewComponent as XUiV_Sprite);
		}
		this.SetupHueTexture();
		this.SetupSaturationVibranceTexture();
	}

	public override void OnClose()
	{
		base.OnClose();
		this.HideCallouts();
		base.xui.playerUI.CursorController.Locked = false;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (base.xui.playerUI.CursorController.navigationTarget == this.svPicker && PlatformManager.NativePlatform.Input.CurrentInputStyle != PlayerInputManager.InputStyle.Keyboard)
		{
			if (base.xui.playerUI.playerInput.GUIActions.Submit.IsPressed)
			{
				this.ShowCallouts(true);
				base.xui.playerUI.CursorController.Locked = true;
				Vector2 value = base.xui.playerUI.playerInput.GUIActions.Nav.Value;
				if (value != Vector2.zero)
				{
					this.vibrance = Mathf.Clamp01(this.vibrance - value.y * 0.05f);
					this.saturation = Mathf.Clamp01(this.saturation + value.x * 0.05f);
					this.isLocalDirty = true;
				}
				float num = this.hue;
				this.hue = Mathf.Clamp(this.hue + base.xui.playerUI.playerInput.GUIActions.PageUp.Value, 0f, 255f);
				this.hue = Mathf.Clamp(this.hue - base.xui.playerUI.playerInput.GUIActions.PageDown.Value, 0f, 255f);
				if (this.hue != num)
				{
					this.isLocalDirty = true;
				}
			}
			else
			{
				this.ShowCallouts(false);
				base.xui.playerUI.CursorController.Locked = false;
			}
		}
		else
		{
			this.HideCallouts();
			base.xui.playerUI.CursorController.Locked = false;
		}
		if (this.isLocalDirty)
		{
			this.isLocalDirty = false;
			this.selectedColor = HSVUtil.ConvertHsvToRgb((double)this.hue, (double)this.saturation, (double)this.vibrance, 1f);
			this.selectedColorSprite.Color = this.selectedColor;
			if (this.OnSelectedColorChanged != null)
			{
				this.OnSelectedColorChanged(this.selectedColor);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ShowCallouts(bool _held)
	{
		if (this.showingCallouts && this.calloutStateHeld == _held)
		{
			return;
		}
		base.xui.calloutWindow.ClearCallouts(XUiC_GamepadCalloutWindow.CalloutType.ColorPicker);
		if (_held)
		{
			base.xui.calloutWindow.AddCallout(UIUtils.ButtonIcon.LeftStick, "igcoColorPickerSaturationVibrance", XUiC_GamepadCalloutWindow.CalloutType.ColorPicker);
			base.xui.calloutWindow.AddCallout(UIUtils.ButtonIcon.LeftTrigger, "igcoColorPickerHueMinus", XUiC_GamepadCalloutWindow.CalloutType.ColorPicker);
			base.xui.calloutWindow.AddCallout(UIUtils.ButtonIcon.RightTrigger, "igcoColorPickerHuePlus", XUiC_GamepadCalloutWindow.CalloutType.ColorPicker);
		}
		else
		{
			base.xui.calloutWindow.AddCallout(UIUtils.ButtonIcon.FaceButtonSouth, "igcoSelectColorPicker", XUiC_GamepadCalloutWindow.CalloutType.ColorPicker);
		}
		base.xui.calloutWindow.EnableCallouts(XUiC_GamepadCalloutWindow.CalloutType.ColorPicker, 0f);
		this.calloutStateHeld = _held;
		this.showingCallouts = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HideCallouts()
	{
		base.xui.calloutWindow.ClearCallouts(XUiC_GamepadCalloutWindow.CalloutType.ColorPicker);
		this.showingCallouts = false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator setSV()
	{
		Texture2D tex = new Texture2D(1, 1, TextureFormat.ARGB32, false);
		this.selectedColorSVPointerSprite.UiTransform.gameObject.SetActive(false);
		yield return new WaitForEndOfFrame();
		Color colorUnderMouse = this.getColorUnderMouse(tex);
		this.saturation = (float)HSVUtil.ConvertRgbToHsv(colorUnderMouse).S;
		this.vibrance = (float)HSVUtil.ConvertRgbToHsv(colorUnderMouse).V;
		float num = this.saturation * (float)this.svPicker.Size.x;
		float num2 = this.vibrance * (float)this.svPicker.Size.y;
		this.selectedColorSVPointerSprite.UiTransform.gameObject.SetActive(true);
		this.selectedColorSVPointerSprite.Position = new Vector2i((int)num, -(int)num2);
		this.selectedColorSVPointerSprite.UiTransform.localPosition = new Vector3(num, -num2);
		this.isLocalDirty = true;
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator setHue()
	{
		Texture2D tex = new Texture2D(1, 1, TextureFormat.ARGB32, false);
		yield return new WaitForEndOfFrame();
		Color colorUnderMouse = this.getColorUnderMouse(tex);
		this.hue = (float)HSVUtil.ConvertRgbToHsv(colorUnderMouse).H;
		this.SetupSaturationVibranceTexture();
		this.isLocalDirty = true;
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Color getColorUnderMouse(Texture2D _tex)
	{
		_tex.ReadPixels(new Rect(Input.mousePosition, Vector2.one), 0, 0);
		_tex.Apply();
		return _tex.GetPixel(0, 0);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetupSaturationVibranceTexture()
	{
		Texture2D texture2D = new Texture2D(this.svPicker.Size.x, this.svPicker.Size.y);
		for (int i = 0; i < this.svPicker.Size.y; i++)
		{
			float num = 1f - (float)(i + 1) / (float)this.svPicker.Size.y;
			for (int j = 0; j < this.svPicker.Size.x; j++)
			{
				float num2 = (float)(j + 1) / (float)this.svPicker.Size.x;
				texture2D.SetPixel(j, i, HSVUtil.ConvertHsvToRgb((double)this.hue, (double)num2, (double)num, 1f));
			}
		}
		texture2D.Apply();
		this.svPicker.Texture = texture2D;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetupHueTexture()
	{
		Texture2D texture2D = new Texture2D(this.hPicker.Size.x, this.hPicker.Size.y);
		for (int i = 0; i < this.hPicker.Size.y; i++)
		{
			float num = (float)i / ((float)this.hPicker.Size.y - 1f);
			for (int j = 0; j < this.hPicker.Size.x; j++)
			{
				texture2D.SetPixel(j, i, HSVUtil.ConvertHsvToRgb((double)(num * 360f), 1.0, 1.0, 1f));
			}
		}
		texture2D.Apply();
		this.hPicker.Texture = texture2D;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SvPickerC_OnDrag(XUiController _sender, EDragType _dragType, Vector2 _mousePositionDelta)
	{
		GameManager.Instance.StartCoroutine(this.setSV());
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SvPickerC_OnPress(XUiController _sender, int _mouseButton)
	{
		GameManager.Instance.StartCoroutine(this.setSV());
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HPickerC_OnDrag(XUiController _sender, EDragType _dragType, Vector2 _mousePositionDelta)
	{
		GameManager.Instance.StartCoroutine(this.setHue());
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HPickerC_OnPress(XUiController _sender, int _mouseButton)
	{
		GameManager.Instance.StartCoroutine(this.setHue());
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Texture svPicker;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Texture hPicker;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color selectedColor = Color.red;

	[PublicizedFrom(EAccessModifier.Private)]
	public float saturation;

	[PublicizedFrom(EAccessModifier.Private)]
	public float vibrance;

	[PublicizedFrom(EAccessModifier.Private)]
	public float hue;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isLocalDirty;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite selectedColorSprite;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite selectedColorSVPointerSprite;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool showingCallouts;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool calloutStateHeld;
}
