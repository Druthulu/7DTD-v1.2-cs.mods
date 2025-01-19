using System;
using System.Globalization;
using Audio;
using InControl;
using UnityEngine;
using UnityEngine.Scripting;

[UnityEngine.Scripting.Preserve]
public class XUiC_BasePartStack : XUiC_SelectableEntry
{
	public int SlotNumber { get; set; }

	public XUiC_ItemStack.StackLocationTypes StackLocation { get; set; }

	public event XUiEvent_SlotChangedEventHandler SlotChangingEvent;

	public event XUiEvent_SlotChangedEventHandler SlotChangedEvent;

	public float HoverIconGrow { get; [PublicizedFrom(EAccessModifier.Protected)] set; }

	public string SlotType
	{
		get
		{
			return this.slotType;
		}
		set
		{
			this.slotType = value;
			this.SetEmptySpriteName();
		}
	}

	public ItemStack ItemStack
	{
		get
		{
			return this.itemStack;
		}
		set
		{
			if (this.itemStack != value)
			{
				this.itemStack = value;
				this.isDirty = true;
				if (this.itemStack.IsEmpty())
				{
					base.Selected = false;
				}
				if (base.Selected)
				{
					this.InfoWindow.SetItemStack(this, true);
				}
				if (this.SlotChangedEvent != null)
				{
					this.SlotChangedEvent(this.SlotNumber, this.itemStack);
				}
				this.itemClass = this.itemStack.itemValue.ItemClass;
				base.RefreshBindings(false);
			}
		}
	}

	public ItemClass ItemClass
	{
		get
		{
			return this.itemClass;
		}
	}

	public XUiC_ItemInfoWindow InfoWindow { get; set; }

	public virtual string GetAtlas()
	{
		if (this.itemClass == null)
		{
			return "ItemIconAtlasGreyscale";
		}
		return "ItemIconAtlas";
	}

	public virtual string GetPartName()
	{
		if (this.itemClass == null)
		{
			return string.Format("[MISSING {0}]", this.SlotType);
		}
		return this.itemClass.GetLocalizedItemName();
	}

	public override bool GetBindingValue(ref string value, string bindingName)
	{
		uint num = <PrivateImplementationDetails>.ComputeStringHash(bindingName);
		if (num <= 2285454806U)
		{
			if (num <= 776984821U)
			{
				if (num != 289581667U)
				{
					if (num == 776984821U)
					{
						if (bindingName == "partquality")
						{
							value = ((this.itemClass != null && this.itemStack != null) ? this.qualityFormatter.Format((int)this.itemStack.itemValue.Quality) : "");
							return true;
						}
					}
				}
				else if (bindingName == "partatlas")
				{
					value = this.GetAtlas();
					return true;
				}
			}
			else if (num != 1419324424U)
			{
				if (num == 2285454806U)
				{
					if (bindingName == "partvisible")
					{
						value = ((this.itemClass != null) ? "true" : "false");
						return true;
					}
				}
			}
			else if (bindingName == "particoncolor")
			{
				if (this.itemClass == null)
				{
					value = "255, 255, 255, 178";
				}
				else
				{
					Color32 color = this.itemStack.itemValue.ItemClass.GetIconTint(this.itemStack.itemValue);
					value = string.Format("{0},{1},{2},{3}", new object[]
					{
						color.r,
						color.g,
						color.b,
						color.a
					});
				}
				return true;
			}
		}
		else if (num <= 2552573645U)
		{
			if (num != 2498838587U)
			{
				if (num == 2552573645U)
				{
					if (bindingName == "partfill")
					{
						value = ((this.itemStack.itemValue.MaxUseTimes == 0) ? "1" : this.partfillFormatter.Format(((float)this.itemStack.itemValue.MaxUseTimes - this.itemStack.itemValue.UseTimes) / (float)this.itemStack.itemValue.MaxUseTimes));
						return true;
					}
				}
			}
			else if (bindingName == "partcolor")
			{
				if (this.itemClass != null)
				{
					Color32 v = QualityInfo.GetQualityColor((int)this.itemStack.itemValue.Quality);
					value = this.partcolorFormatter.Format(v);
				}
				else
				{
					value = "255, 255, 255, 0";
				}
				return true;
			}
		}
		else if (num != 2733906447U)
		{
			if (num != 3045999413U)
			{
				if (num == 3130438080U)
				{
					if (bindingName == "emptyvisible")
					{
						value = ((this.itemClass == null) ? "true" : "false");
						return true;
					}
				}
			}
			else if (bindingName == "particon")
			{
				if (this.itemClass == null)
				{
					value = this.emptySpriteName;
				}
				else
				{
					value = this.itemStack.itemValue.GetPropertyOverride("CustomIcon", (this.itemClass.CustomIcon != null) ? this.itemClass.CustomIcon.Value : this.itemClass.GetIconName());
				}
				return true;
			}
		}
		else if (bindingName == "partname")
		{
			value = this.GetPartName();
			return true;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void SelectedChanged(bool isSelected)
	{
		this.SetColor(isSelected ? this.selectColor : XUiC_BasePartStack.backgroundColor);
		((XUiV_Sprite)this.background.ViewComponent).SpriteName = (isSelected ? "ui_game_select_row" : "menu_empty");
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void SetColor(Color32 color)
	{
		((XUiV_Sprite)this.background.ViewComponent).Color = color;
	}

	public override void Init()
	{
		base.Init();
		this.itemIcon = base.GetChildById("itemIcon");
		this.background = base.GetChildById("background");
		base.RefreshBindings(false);
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (base.WindowGroup.isShowing)
		{
			PlayerActionsGUI guiactions = base.xui.playerUI.playerInput.GUIActions;
			CursorControllerAbs cursorController = base.xui.playerUI.CursorController;
			Vector3 a = cursorController.GetScreenPosition();
			bool mouseButtonUp = cursorController.GetMouseButtonUp(UICamera.MouseButton.LeftButton);
			bool mouseButtonDown = cursorController.GetMouseButtonDown(UICamera.MouseButton.LeftButton);
			bool mouseButton = cursorController.GetMouseButton(UICamera.MouseButton.LeftButton);
			if (this.isOver && UICamera.hoveredObject == base.ViewComponent.UiTransform.gameObject && base.ViewComponent.EventOnPress)
			{
				if (guiactions.LastInputType == BindingSourceType.DeviceBindingSource)
				{
					if (base.xui.dragAndDrop.CurrentStack.IsEmpty())
					{
						if (!this.ItemStack.IsEmpty())
						{
							if (guiactions.Submit.WasReleased && this.CanRemove())
							{
								this.SwapItem();
								this.currentColor = XUiC_BasePartStack.backgroundColor;
								if (this.itemStack.IsEmpty())
								{
									((XUiV_Sprite)this.background.ViewComponent).Color = XUiC_BasePartStack.backgroundColor;
									return;
								}
							}
							else
							{
								if (guiactions.RightStick.WasReleased)
								{
									this.HandleMoveToPreferredLocation();
									return;
								}
								if (guiactions.Inspect.WasReleased)
								{
									this.HandleItemInspect();
									return;
								}
							}
						}
					}
					else if (guiactions.Submit.WasReleased)
					{
						this.HandleStackSwap();
						return;
					}
				}
				else if (InputUtils.ShiftKeyPressed)
				{
					if (mouseButtonUp)
					{
						this.HandleMoveToPreferredLocation();
						return;
					}
				}
				else if (mouseButton)
				{
					if (base.xui.dragAndDrop.CurrentStack.IsEmpty() && !this.ItemStack.IsEmpty())
					{
						if (!this.lastClicked)
						{
							this.startMousePos = a;
						}
						else if (this.CanRemove() && Mathf.Abs((a - this.startMousePos).magnitude) > (float)this.PickupSnapDistance)
						{
							this.SwapItem();
							this.currentColor = XUiC_BasePartStack.backgroundColor;
							if (this.itemStack.IsEmpty())
							{
								((XUiV_Sprite)this.background.ViewComponent).Color = XUiC_BasePartStack.backgroundColor;
							}
						}
					}
					if (mouseButtonDown)
					{
						this.lastClicked = true;
						return;
					}
				}
				else
				{
					if (!mouseButtonUp)
					{
						this.lastClicked = false;
						return;
					}
					if (base.xui.dragAndDrop.CurrentStack.IsEmpty())
					{
						this.HandleItemInspect();
						return;
					}
					if (this.lastClicked)
					{
						this.HandleStackSwap();
						return;
					}
				}
			}
			else
			{
				this.currentColor = XUiC_BasePartStack.backgroundColor;
				if (!base.Selected)
				{
					((XUiV_Sprite)this.background.ViewComponent).Color = this.currentColor;
				}
				this.lastClicked = false;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void SetEmptySpriteName()
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void HandleItemInspect()
	{
		if (!this.ItemStack.IsEmpty() && this.InfoWindow != null)
		{
			base.Selected = true;
			this.InfoWindow.SetItemStack(this, true);
		}
		this.HandleClickComplete();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual bool CanSwap(ItemStack stack)
	{
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual bool CanRemove()
	{
		return true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void HandleStackSwap()
	{
		ItemStack currentStack = base.xui.dragAndDrop.CurrentStack;
		if (!currentStack.IsEmpty())
		{
			if (this.CanSwap(currentStack))
			{
				this.SwapItem();
			}
			else
			{
				Manager.PlayInsidePlayerHead("ui_denied", -1, 0f, false, false);
			}
		}
		else if (this.CanRemove())
		{
			this.SwapItem();
		}
		else
		{
			Manager.PlayInsidePlayerHead("ui_denied", -1, 0f, false, false);
		}
		base.Selected = false;
		this.HandleClickComplete();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void HandleClickComplete()
	{
		this.lastClicked = false;
		this.currentColor = XUiC_BasePartStack.backgroundColor;
		if (this.itemStack.IsEmpty())
		{
			((XUiV_Sprite)this.background.ViewComponent).Color = XUiC_BasePartStack.backgroundColor;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnHovered(bool _isOver)
	{
		this.isOver = _isOver;
		if (!base.Selected)
		{
			if (_isOver)
			{
				((XUiV_Sprite)this.background.ViewComponent).Color = XUiC_BasePartStack.highlightColor;
			}
			else
			{
				((XUiV_Sprite)this.background.ViewComponent).Color = XUiC_BasePartStack.backgroundColor;
			}
		}
		ItemStack currentStack = base.xui.dragAndDrop.CurrentStack;
		bool canSwap = !currentStack.IsEmpty() && this.CanSwap(currentStack);
		base.xui.calloutWindow.UpdateCalloutsForItemStack(base.ViewComponent.UiTransform.gameObject, this.ItemStack, this.isOver, canSwap, this.CanRemove(), true);
		base.OnHovered(_isOver);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void SwapItem()
	{
		ItemStack currentStack = base.xui.dragAndDrop.CurrentStack;
		if (this.itemStack.IsEmpty())
		{
			if (this.placeSound != null)
			{
				Manager.PlayXUiSound(this.placeSound, 0.75f);
			}
		}
		else if (this.pickupSound != null)
		{
			Manager.PlayXUiSound(this.pickupSound, 0.75f);
		}
		if (this.SlotChangingEvent != null)
		{
			this.SlotChangingEvent(this.SlotNumber, this.itemStack);
		}
		base.xui.dragAndDrop.CurrentStack = this.itemStack.Clone();
		base.xui.dragAndDrop.PickUpType = this.StackLocation;
		this.ItemStack = currentStack.Clone();
	}

	public void HandleMoveToPreferredLocation()
	{
		if (this.ItemStack.IsEmpty() || !this.CanRemove())
		{
			return;
		}
		if (this.SlotChangingEvent != null)
		{
			this.SlotChangingEvent(this.SlotNumber, this.ItemStack);
		}
		if (base.xui.PlayerInventory.AddItemToBackpack(this.ItemStack))
		{
			if (base.xui.currentSelectedEntry == this)
			{
				base.xui.currentSelectedEntry.Selected = false;
				this.InfoWindow.SetItemStack(null, false);
			}
			this.ItemStack = ItemStack.Empty.Clone();
			if (this.placeSound != null)
			{
				Manager.PlayXUiSound(this.placeSound, 0.75f);
			}
			if (this.SlotChangedEvent != null)
			{
				this.SlotChangedEvent(this.SlotNumber, this.itemStack);
				return;
			}
		}
		else if (base.xui.PlayerInventory.AddItemToToolbelt(this.ItemStack))
		{
			if (base.xui.currentSelectedEntry == this)
			{
				base.xui.currentSelectedEntry.Selected = false;
				this.InfoWindow.SetItemStack(null, false);
			}
			this.ItemStack = ItemStack.Empty.Clone();
			if (this.placeSound != null)
			{
				Manager.PlayXUiSound(this.placeSound, 0.75f);
			}
			if (this.SlotChangedEvent != null)
			{
				this.SlotChangedEvent(this.SlotNumber, this.itemStack);
			}
		}
	}

	public void ClearSelectedInfoWindow()
	{
		if (base.Selected)
		{
			this.InfoWindow.SetItemStack(null, true);
		}
	}

	public override bool ParseAttribute(string name, string value, XUiController _parent)
	{
		bool flag = base.ParseAttribute(name, value, _parent);
		if (!flag)
		{
			if (!(name == "background_color"))
			{
				if (!(name == "highlight_color"))
				{
					if (!(name == "pickup_snap_distance"))
					{
						if (!(name == "hover_icon_grow"))
						{
							if (!(name == "pickup_sound"))
							{
								if (!(name == "place_sound"))
								{
									return false;
								}
								base.xui.LoadData<AudioClip>(value, delegate(AudioClip o)
								{
									this.placeSound = o;
								});
							}
							else
							{
								base.xui.LoadData<AudioClip>(value, delegate(AudioClip o)
								{
									this.pickupSound = o;
								});
							}
						}
						else
						{
							this.HoverIconGrow = StringParsers.ParseFloat(value, 0, -1, NumberStyles.Any);
						}
					}
					else
					{
						this.PickupSnapDistance = int.Parse(value);
					}
				}
				else
				{
					XUiC_BasePartStack.highlightColor = StringParsers.ParseColor32(value);
				}
			}
			else
			{
				XUiC_BasePartStack.backgroundColor = StringParsers.ParseColor32(value);
			}
			return true;
		}
		return flag;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public ItemStack itemStack = ItemStack.Empty.Clone();

	[PublicizedFrom(EAccessModifier.Protected)]
	public ItemClass itemClass;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool isDirty = true;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool bHighlightEnabled;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool bDropEnabled = true;

	[PublicizedFrom(EAccessModifier.Protected)]
	public AudioClip pickupSound;

	[PublicizedFrom(EAccessModifier.Protected)]
	public AudioClip placeSound;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool isOver;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string slotType;

	[PublicizedFrom(EAccessModifier.Protected)]
	public Color32 currentColor;

	[PublicizedFrom(EAccessModifier.Protected)]
	public Color32 selectColor = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);

	[PublicizedFrom(EAccessModifier.Protected)]
	public Color32 pressColor = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool lastClicked;

	public int PickupSnapDistance = 4;

	public static Color32 finalPressedColor = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);

	public static Color32 backgroundColor = new Color32(96, 96, 96, byte.MaxValue);

	public static Color32 highlightColor = new Color32(128, 128, 128, byte.MaxValue);

	public Color32 holdingColor = new Color32(byte.MaxValue, 128, 0, byte.MaxValue);

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiController stackValue;

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiController itemIcon;

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiController durability;

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiController durabilityBackground;

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiController tintedOverlay;

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiController highlightOverlay;

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiController background;

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiController lblItemName;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterInt qualityFormatter = new CachedStringFormatterInt();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterXuiRgbaColor partcolorFormatter = new CachedStringFormatterXuiRgbaColor();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterFloat partfillFormatter = new CachedStringFormatterFloat(null);

	[PublicizedFrom(EAccessModifier.Protected)]
	public string emptySpriteName = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 startMousePos = Vector3.zero;
}
