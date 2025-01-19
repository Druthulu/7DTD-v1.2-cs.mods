using System;
using InControl;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[UnityEngine.Scripting.Preserve]
public class XUiC_ShapeStack : XUiC_SelectableEntry
{
	public bool IsLocked { get; [PublicizedFrom(EAccessModifier.Private)] set; }

	public Block BlockData
	{
		get
		{
			return this.blockData;
		}
		set
		{
			if (this.blockData != value)
			{
				this.blockData = value;
				this.isDirty = true;
				if (this.blockData == null)
				{
					this.viewComponent.ToolTip = string.Empty;
					this.IsLocked = false;
				}
				else
				{
					this.viewComponent.ToolTip = ((this.blockData.GetAutoShapeType() != EAutoShapeType.None) ? this.blockData.GetLocalizedAutoShapeShapeName() : this.blockData.GetLocalizedBlockName());
				}
			}
			base.ViewComponent.Enabled = (value != null);
			base.RefreshBindings(false);
		}
	}

	public XUiC_ShapeInfoWindow InfoWindow { get; set; }

	public XUiC_ShapeMaterialInfoWindow MaterialInfoWindow { get; set; }

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void SelectedChanged(bool isSelected)
	{
		if (isSelected)
		{
			this.SetColor(this.selectColor);
			if (base.xui.currentSelectedEntry == this)
			{
				XUiC_ShapeInfoWindow infoWindow = this.InfoWindow;
				if (infoWindow != null)
				{
					infoWindow.SetShape(this.blockData);
				}
				XUiC_ShapeMaterialInfoWindow materialInfoWindow = this.MaterialInfoWindow;
				if (materialInfoWindow == null)
				{
					return;
				}
				materialInfoWindow.SetShape(this.blockData);
				return;
			}
		}
		else
		{
			this.SetColor(XUiC_ShapeStack.backgroundColor);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetColor(Color32 color)
	{
		this.background.Color = color;
	}

	public override void Init()
	{
		base.Init();
		this.tintedOverlay = base.GetChildById("tintedOverlay");
		this.highlightOverlay = (base.GetChildById("highlightOverlay").ViewComponent as XUiV_Sprite);
		this.background = (base.GetChildById("background").ViewComponent as XUiV_Sprite);
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		PlayerActionsGUI guiactions = base.xui.playerUI.playerInput.GUIActions;
		if (base.WindowGroup.isShowing)
		{
			CursorControllerAbs cursorController = base.xui.playerUI.CursorController;
			cursorController.GetScreenPosition();
			bool mouseButtonUp = cursorController.GetMouseButtonUp(UICamera.MouseButton.LeftButton);
			cursorController.GetMouseButtonDown(UICamera.MouseButton.LeftButton);
			cursorController.GetMouseButton(UICamera.MouseButton.LeftButton);
			cursorController.GetMouseButtonUp(UICamera.MouseButton.RightButton);
			cursorController.GetMouseButtonDown(UICamera.MouseButton.RightButton);
			cursorController.GetMouseButton(UICamera.MouseButton.RightButton);
			if (this.isOver && UICamera.hoveredObject == base.ViewComponent.UiTransform.gameObject && base.ViewComponent.EventOnPress)
			{
				if (guiactions.LastInputType == BindingSourceType.DeviceBindingSource)
				{
					bool wasReleased = guiactions.Submit.WasReleased;
					bool wasReleased2 = guiactions.HalfStack.WasReleased;
					bool wasReleased3 = guiactions.Inspect.WasReleased;
					bool wasReleased4 = guiactions.RightStick.WasReleased;
					if (wasReleased && this.blockData != null)
					{
						this.SetSelectedShapeForItem();
						if (wasReleased4)
						{
							base.xui.playerUI.windowManager.Close("shapes");
						}
					}
				}
				else if (mouseButtonUp && this.blockData != null)
				{
					this.SetSelectedShapeForItem();
					if (InputUtils.ShiftKeyPressed)
					{
						base.xui.playerUI.windowManager.Close("shapes");
					}
				}
			}
			else
			{
				this.currentColor = XUiC_ShapeStack.backgroundColor;
				if (this.highlightOverlay != null)
				{
					this.highlightOverlay.Color = XUiC_ShapeStack.backgroundColor;
				}
				if (!base.Selected)
				{
					this.background.Color = this.currentColor;
				}
				this.lastClicked = false;
				if (this.isOver)
				{
					this.isOver = false;
				}
			}
		}
		if (this.isDirty)
		{
			this.isDirty = false;
		}
	}

	public static string GetFavoritesEntryName(Block _block)
	{
		if (_block == null)
		{
			return null;
		}
		if (_block.GetAutoShapeType() == EAutoShapeType.None)
		{
			return _block.GetBlockName();
		}
		return _block.GetAutoShapeShapeName();
	}

	public override void UpdateInput()
	{
		base.UpdateInput();
		if (!base.Selected || this.BlockData == null)
		{
			return;
		}
		if (base.xui.playerUI.playerInput != null)
		{
			PlayerActionsGUI guiactions = base.xui.playerUI.playerInput.GUIActions;
			if ((PlatformManager.NativePlatform.Input.CurrentInputStyle == PlayerInputManager.InputStyle.Keyboard && guiactions.DPad_Right.WasReleased) || (PlatformManager.NativePlatform.Input.CurrentInputStyle != PlayerInputManager.InputStyle.Keyboard && guiactions.Inspect.WasReleased))
			{
				EntityPlayer entityPlayer = base.xui.playerUI.entityPlayer;
				string favoritesEntryName = XUiC_ShapeStack.GetFavoritesEntryName(this.BlockData);
				if (!entityPlayer.favoriteShapes.Remove(favoritesEntryName))
				{
					entityPlayer.favoriteShapes.Add(favoritesEntryName);
				}
				this.Owner.Owner.UpdateAll();
			}
		}
	}

	public void SetSelectedShapeForItem()
	{
		if (!this.IsLocked)
		{
			this.Owner.Owner.ItemValue.Meta = this.ShapeIndex;
			this.Owner.Owner.RefreshItemStack();
		}
		base.Selected = true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnHovered(bool _isOver)
	{
		this.isOver = _isOver;
		if (!base.Selected)
		{
			if (_isOver)
			{
				this.background.Color = XUiC_ShapeStack.highlightColor;
			}
			else
			{
				this.background.Color = XUiC_ShapeStack.backgroundColor;
			}
		}
		base.OnHovered(_isOver);
	}

	public override void OnOpen()
	{
		base.OnOpen();
		base.RefreshBindings(false);
	}

	public override bool GetBindingValue(ref string value, string bindingName)
	{
		if (bindingName == "islocked")
		{
			value = this.IsLocked.ToString();
			return true;
		}
		if (bindingName == "itemicon")
		{
			value = ((this.BlockData == null) ? "" : this.BlockData.GetIconName());
			return true;
		}
		if (bindingName == "itemicontint")
		{
			Color32 v = Color.white;
			if (this.BlockData != null)
			{
				v = this.BlockData.CustomIconTint;
			}
			value = this.itemicontintcolorFormatter.Format(v);
			return true;
		}
		if (!(bindingName == "isfavorite"))
		{
			return base.GetBindingValue(ref value, bindingName);
		}
		string favoritesEntryName = XUiC_ShapeStack.GetFavoritesEntryName(this.BlockData);
		value = (favoritesEntryName != null && base.xui.playerUI.entityPlayer.favoriteShapes.Contains(favoritesEntryName)).ToString();
		return true;
	}

	public override bool ParseAttribute(string name, string value, XUiController _parent)
	{
		bool flag = base.ParseAttribute(name, value, _parent);
		if (!flag)
		{
			if (!(name == "select_color"))
			{
				if (!(name == "press_color"))
				{
					if (!(name == "background_color"))
					{
						if (!(name == "highlight_color"))
						{
							if (!(name == "select_sound"))
							{
								return false;
							}
							base.xui.LoadData<AudioClip>(value, delegate(AudioClip o)
							{
								this.selectSound = o;
							});
						}
						else
						{
							XUiC_ShapeStack.highlightColor = StringParsers.ParseColor32(value);
						}
					}
					else
					{
						XUiC_ShapeStack.backgroundColor = StringParsers.ParseColor32(value);
					}
				}
				else
				{
					this.pressColor = StringParsers.ParseColor32(value);
				}
			}
			else
			{
				this.selectColor = StringParsers.ParseColor32(value);
			}
			return true;
		}
		return flag;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isDirty = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bHighlightEnabled;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bDropEnabled = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public AudioClip selectSound;

	[PublicizedFrom(EAccessModifier.Private)]
	public AudioClip placeSound;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isOver;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color32 currentColor;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color32 selectColor = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);

	[PublicizedFrom(EAccessModifier.Private)]
	public Color32 pressColor = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);

	[PublicizedFrom(EAccessModifier.Private)]
	public bool lastClicked;

	public static Color32 backgroundColor = new Color32(96, 96, 96, byte.MaxValue);

	public static Color32 highlightColor = new Color32(222, 206, 163, byte.MaxValue);

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController tintedOverlay;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label stackValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite highlightOverlay;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite background;

	public int ShapeIndex = -1;

	public XUiC_ShapeStackGrid Owner;

	[PublicizedFrom(EAccessModifier.Private)]
	public Block blockData;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 startMousePos = Vector3.zero;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterXuiRgbaColor itemicontintcolorFormatter = new CachedStringFormatterXuiRgbaColor();
}
