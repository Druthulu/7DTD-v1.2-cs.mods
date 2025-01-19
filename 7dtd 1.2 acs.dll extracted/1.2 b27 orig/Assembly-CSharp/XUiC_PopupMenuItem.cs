using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_PopupMenuItem : XUiController
{
	public MenuItemEntry ItemEntry
	{
		get
		{
			return this.itemEntry;
		}
		set
		{
			this.itemEntry = value;
			base.RefreshBindings(false);
			this.label.Label.text = this.label.Text;
			base.xui.currentPopupMenu.SetWidth((int)this.label.Label.printedSize.x);
			this.label.Size = new Vector2i((int)this.label.Label.printedSize.x, this.label.Label.height);
			this.label.Position = new Vector2i(50, -8);
			this.background.SpriteName = "menu_empty";
			this.background.Color = new Color32(64, 64, 64, byte.MaxValue);
		}
	}

	public override void Init()
	{
		base.Init();
		base.OnPress += this.onPressed;
		base.OnHover += this.OnHovered;
		this.label = (XUiV_Label)base.GetChildById("lblText").ViewComponent;
		this.label.Overflow = UILabel.Overflow.ResizeFreely;
		this.background = (XUiV_Sprite)base.GetChildById("background").ViewComponent;
		this.collider = this.viewComponent.UiTransform.GetComponent<BoxCollider>();
		this.viewComponent.UseSelectionBox = false;
		this.background.UseSelectionBox = false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnHovered(XUiController _sender, bool _isOver)
	{
		if (this.background != null)
		{
			if (_isOver)
			{
				this.background.SpriteName = "ui_game_select_row";
				this.background.Color = Color.white;
			}
			else
			{
				this.background.SpriteName = "menu_empty";
				this.background.Color = new Color32(64, 64, 64, byte.MaxValue);
			}
		}
		base.xui.currentPopupMenu.IsOver = _isOver;
	}

	public override bool GetBindingValue(ref string value, string bindingName)
	{
		if (bindingName == "menuicon")
		{
			value = ((this.ItemEntry != null) ? this.ItemEntry.IconName : "");
			return true;
		}
		if (bindingName == "menutext")
		{
			value = ((this.ItemEntry != null) ? this.ItemEntry.Text : "");
			return true;
		}
		if (!(bindingName == "statuscolor"))
		{
			return false;
		}
		value = "255,255,255,255";
		if (this.ItemEntry != null)
		{
			Color32 v = this.ItemEntry.IsEnabled ? this.defaultFontColor : this.disabledFontColor;
			value = this.statuscolorFormatter.Format(v);
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void onPressed(XUiController _sender, int _mouseButton)
	{
		if (this.ItemEntry != null)
		{
			if (this.ItemEntry != null && this.ItemEntry.IsEnabled)
			{
				this.ItemEntry.HandleItemClicked();
			}
			base.xui.currentPopupMenu.ClearItems();
		}
	}

	public void SetWidth(int width)
	{
		if (this.background != null)
		{
			this.background.Size = new Vector2i(width, this.background.Size.y);
			if (this.collider != null)
			{
				this.collider.size = new Vector3((float)width, this.collider.size.y, this.collider.size.z);
				this.collider.center = new Vector3((float)(width / 2), this.collider.center.y, this.collider.center.z);
			}
		}
	}

	public override bool ParseAttribute(string name, string value, XUiController _parent)
	{
		bool flag = base.ParseAttribute(name, value, _parent);
		if (!flag)
		{
			if (!(name == "default_font_color"))
			{
				if (!(name == "disabled_font_color"))
				{
					return false;
				}
				this.disabledFontColor = StringParsers.ParseColor32(value);
			}
			else
			{
				this.defaultFontColor = StringParsers.ParseColor32(value);
			}
			return true;
		}
		return flag;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isOver;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label label;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite background;

	[PublicizedFrom(EAccessModifier.Protected)]
	public Color32 disabledFontColor = Color.gray;

	[PublicizedFrom(EAccessModifier.Protected)]
	public Color32 defaultFontColor = Color.white;

	[PublicizedFrom(EAccessModifier.Private)]
	public BoxCollider collider;

	[PublicizedFrom(EAccessModifier.Private)]
	public MenuItemEntry itemEntry;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterXuiRgbaColor statuscolorFormatter = new CachedStringFormatterXuiRgbaColor();
}
