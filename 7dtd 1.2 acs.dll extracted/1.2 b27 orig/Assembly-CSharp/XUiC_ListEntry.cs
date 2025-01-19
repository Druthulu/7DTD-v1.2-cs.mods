using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ListEntry<T> : XUiController where T : XUiListEntry
{
	[PublicizedFrom(EAccessModifier.Private)]
	static XUiC_ListEntry()
	{
		MethodInfo method = typeof(T).GetMethod("GetNullBindingValues", BindingFlags.Static | BindingFlags.Public, null, new Type[]
		{
			typeof(string).MakeByRefType(),
			typeof(string)
		}, null);
		if (method != null)
		{
			XUiC_ListEntry<T>.nullBindings = (Delegate.CreateDelegate(typeof(XUiC_ListEntry<T>.NullBindingDelegate), method) as XUiC_ListEntry<T>.NullBindingDelegate);
			return;
		}
		Log.Warning("[XUi] List entry type \"" + typeof(T).FullName + "\" does not have a static GetNullBindingValues method");
	}

	public bool HasEntry
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return this.entryData != null;
		}
	}

	public new bool Selected
	{
		get
		{
			return this.selected;
		}
		set
		{
			if (value)
			{
				if (this.List.SelectedEntry != null)
				{
					this.List.SelectedEntry.SelectedChanged(false);
					this.List.SelectedEntry.selected = false;
				}
			}
			else if (this.List.SelectedEntry == this)
			{
				this.SelectedChanged(false);
				this.selected = false;
				this.List.ClearSelection();
			}
			this.selected = value;
			if (this.selected)
			{
				this.List.SelectedEntry = this;
			}
			this.SelectedChanged(this.selected);
		}
	}

	public bool ForceHovered
	{
		get
		{
			return this.forceHovered;
		}
		set
		{
			if (value != this.forceHovered)
			{
				this.forceHovered = value;
				this.updateHoveredEffect();
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void SelectedChanged(bool isSelected)
	{
		if (this.background != null)
		{
			this.background.Color = (isSelected ? this.bgColorSelected : this.bgColorUnselected);
			this.background.SpriteName = (isSelected ? this.bgSpriteNameSelected : this.bgSpriteNameUnselected);
		}
	}

	public override void Init()
	{
		base.Init();
		for (int i = 0; i < this.children.Count; i++)
		{
			XUiView viewComponent = this.children[i].ViewComponent;
			if (viewComponent.ID.EqualsCaseInsensitive("background"))
			{
				this.background = (viewComponent as XUiV_Sprite);
			}
		}
		base.OnPress += this.XUiC_ListEntry_OnPress;
		base.ViewComponent.Enabled = this.HasEntry;
		this.IsDirty = true;
	}

	public void XUiC_ListEntry_OnPress(XUiController _sender, int _mouseButton)
	{
		if (!base.ViewComponent.Enabled)
		{
			return;
		}
		if (!this.Selected)
		{
			this.Selected = true;
		}
		this.List.OnListEntryClicked(this);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateHoveredEffect()
	{
		if (this.background != null && this.HasEntry && !this.Selected)
		{
			if (this.forceHovered || this.isHovered)
			{
				this.background.Color = this.bgColorHovered;
				return;
			}
			this.background.Color = this.bgColorUnselected;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnHovered(bool _isOver)
	{
		this.isHovered = _isOver;
		this.updateHoveredEffect();
		base.OnHovered(_isOver);
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (this.IsDirty)
		{
			base.RefreshBindings(false);
			this.IsDirty = false;
		}
	}

	public override bool ParseAttribute(string name, string value, XUiController _parent)
	{
		if (base.ParseAttribute(name, value, _parent))
		{
			return true;
		}
		if (!(name == "background_color_unselected"))
		{
			if (!(name == "background_color_hovered"))
			{
				if (!(name == "background_color_selected"))
				{
					if (!(name == "background_sprite_unselected"))
					{
						if (!(name == "background_sprite_selected"))
						{
							return false;
						}
						this.bgSpriteNameSelected = value;
					}
					else
					{
						this.bgSpriteNameUnselected = value;
					}
				}
				else
				{
					this.bgColorSelected = StringParsers.ParseColor32(value);
				}
			}
			else
			{
				this.bgColorHovered = StringParsers.ParseColor32(value);
			}
		}
		else
		{
			this.bgColorUnselected = StringParsers.ParseColor32(value);
		}
		return true;
	}

	public override bool GetBindingValue(ref string _value, string _bindingName)
	{
		if (this.entryData != null)
		{
			return this.entryData.GetBindingValue(ref _value, _bindingName);
		}
		return XUiC_ListEntry<T>.nullBindings != null && XUiC_ListEntry<T>.nullBindings(ref _value, _bindingName);
	}

	public virtual void SetEntry(T _data)
	{
		if (_data != this.entryData)
		{
			this.entryData = _data;
			base.ViewComponent.Enabled = this.HasEntry;
			if (!this.Selected || !this.HasEntry)
			{
				this.background.Color = this.bgColorUnselected;
			}
		}
		base.ViewComponent.IsNavigatable = (base.ViewComponent.IsSnappable = this.HasEntry);
		this.IsDirty = true;
	}

	public T GetEntry()
	{
		return this.entryData;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool isHovered;

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiV_Sprite background;

	public XUiC_List<T> List;

	[PublicizedFrom(EAccessModifier.Protected)]
	public Color32 bgColorUnselected = new Color32(64, 64, 64, byte.MaxValue);

	[PublicizedFrom(EAccessModifier.Protected)]
	public Color32 bgColorHovered = new Color32(96, 96, 96, byte.MaxValue);

	[PublicizedFrom(EAccessModifier.Protected)]
	public Color32 bgColorSelected = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);

	[PublicizedFrom(EAccessModifier.Protected)]
	public string bgSpriteNameUnselected = "menu_empty";

	[PublicizedFrom(EAccessModifier.Protected)]
	public string bgSpriteNameSelected = "ui_game_select_row";

	[PublicizedFrom(EAccessModifier.Private)]
	public static XUiC_ListEntry<T>.NullBindingDelegate nullBindings;

	[PublicizedFrom(EAccessModifier.Private)]
	public T entryData;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool selected;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool forceHovered;

	[PublicizedFrom(EAccessModifier.Private)]
	public delegate bool NullBindingDelegate(ref string _value, string _bindingName);
}
