using System;
using System.Globalization;
using Audio;
using InControl;
using UnityEngine;
using UnityEngine.Scripting;

[UnityEngine.Scripting.Preserve]
public class XUiC_EquipmentStack : XUiC_SelectableEntry
{
	public EquipmentSlots EquipSlot
	{
		get
		{
			return this.equipSlot;
		}
		set
		{
			this.equipSlot = value;
			this.SetEmptySpriteNameAndTooltip();
		}
	}

	public int SlotNumber { get; set; }

	public event XUiEvent_SlotChangedEventHandler SlotChangedEvent;

	public float HoverIconGrow { get; [PublicizedFrom(EAccessModifier.Private)] set; }

	public ItemValue ItemValue
	{
		get
		{
			return this.itemValue;
		}
		set
		{
			if (this.itemValue != value)
			{
				this.itemValue = value;
				this.itemStack.itemValue = this.itemValue;
				if (!this.itemStack.itemValue.IsEmpty())
				{
					this.itemStack.count = 1;
				}
				if (value.IsEmpty() && base.Selected)
				{
					base.Selected = false;
					if (this.InfoWindow != null)
					{
						this.InfoWindow.SetItemStack(null, true);
					}
				}
				if (this.SlotChangedEvent != null)
				{
					this.SlotChangedEvent(this.SlotNumber, this.itemStack);
				}
			}
			this.isDirty = true;
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
				this.itemStack = value.Clone();
				this.ItemValue = this.itemStack.itemValue.Clone();
				this.isDirty = true;
			}
		}
	}

	public XUiC_ItemInfoWindow InfoWindow { get; set; }

	public XUiC_CharacterFrameWindow FrameWindow { get; set; }

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void SelectedChanged(bool isSelected)
	{
		this.SetBorderColor(isSelected ? this.selectedBorderColor : this.normalBorderColor);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetBorderColor(Color32 color)
	{
		((XUiV_Sprite)this.background.ViewComponent).Color = color;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetEmptySpriteNameAndTooltip()
	{
		switch (this.equipSlot)
		{
		case EquipmentSlots.Head:
			this.emptySpriteName = "apparelCowboyHat";
			this.emptyTooltipName = this.lblHeadgear;
			break;
		case EquipmentSlots.Chest:
			this.emptySpriteName = "armorSteelChest";
			this.emptyTooltipName = this.lblChestArmor;
			break;
		case EquipmentSlots.Hands:
			this.emptySpriteName = "armorLeatherGloves";
			this.emptyTooltipName = this.lblGloves;
			break;
		case EquipmentSlots.Feet:
			this.emptySpriteName = "apparelWornBoots";
			this.emptyTooltipName = this.lblFootwear;
			break;
		}
		if (this.emptyTooltipName != null)
		{
			this.emptyTooltipName = this.emptyTooltipName.ToUpper();
		}
	}

	public override void Init()
	{
		base.Init();
		this.stackValue = base.GetChildById("stackValue");
		this.background = base.GetChildById("background");
		this.SetBorderColor(this.normalBorderColor);
		this.itemIcon = base.GetChildById("itemIcon");
		this.durabilityBackground = base.GetChildById("durabilityBackground");
		this.durability = base.GetChildById("durability");
		this.tintedOverlay = base.GetChildById("tintedOverlay");
		this.highlightOverlay = base.GetChildById("highlightOverlay");
		this.lockTypeIcon = base.GetChildById("lockTypeIcon");
		this.overlay = base.GetChildById("overlay");
		this.itemStack.count = 1;
		this.tweenScale = this.itemIcon.ViewComponent.UiTransform.gameObject.AddComponent<TweenScale>();
		this.lblHeadgear = Localization.Get("lblHeadgear", false);
		this.lblChestArmor = Localization.Get("lblChestArmor", false);
		this.lblGloves = Localization.Get("lblGloves", false);
		this.lblFootwear = Localization.Get("lblFootwear", false);
		base.ViewComponent.UseSelectionBox = false;
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
					bool wasReleased = guiactions.Submit.WasReleased;
					bool wasReleased2 = guiactions.Inspect.WasReleased;
					bool wasReleased3 = guiactions.RightStick.WasReleased;
					if (base.xui.dragAndDrop.CurrentStack.IsEmpty() && !this.ItemStack.IsEmpty())
					{
						if (wasReleased)
						{
							this.SwapItem();
						}
						else if (wasReleased3)
						{
							this.HandleMoveToPreferredLocation();
							base.xui.PlayerEquipment.RefreshEquipment();
						}
						else if (wasReleased2)
						{
							this.HandleItemInspect();
						}
						if (this.itemStack.IsEmpty())
						{
							((XUiV_Sprite)this.background.ViewComponent).Color = XUiC_EquipmentStack.backgroundColor;
						}
					}
					else if (wasReleased)
					{
						this.HandleStackSwap();
					}
				}
				else if (InputUtils.ShiftKeyPressed)
				{
					if (mouseButtonUp)
					{
						this.HandleMoveToPreferredLocation();
						base.xui.PlayerEquipment.RefreshEquipment();
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
						else if (Mathf.Abs((a - this.startMousePos).magnitude) > (float)this.PickupSnapDistance)
						{
							this.SwapItem();
							base.xui.PlayerEquipment.RefreshEquipment();
						}
						this.SetBorderColor(this.normalBorderColor);
					}
					if (mouseButtonDown)
					{
						this.lastClicked = true;
					}
				}
				else if (mouseButtonUp)
				{
					if (base.xui.dragAndDrop.CurrentStack.IsEmpty())
					{
						this.HandleItemInspect();
					}
					else if (this.lastClicked)
					{
						this.HandleStackSwap();
						base.xui.PlayerEquipment.RefreshEquipment();
					}
				}
				else
				{
					this.lastClicked = false;
				}
			}
			else
			{
				this.lastClicked = false;
				if (this.isOver || this.itemIcon.ViewComponent.UiTransform.localScale != Vector3.one)
				{
					if (this.tweenScale.value != Vector3.one && !this.itemStack.IsEmpty())
					{
						this.tweenScale.from = Vector3.one * 1.5f;
						this.tweenScale.to = Vector3.one;
						this.tweenScale.enabled = true;
						this.tweenScale.duration = 0.5f;
					}
					this.isOver = false;
				}
			}
		}
		if (this.isDirty)
		{
			bool flag = !this.itemValue.IsEmpty();
			ItemClass itemClass = null;
			if (flag)
			{
				itemClass = ItemClass.GetForId(this.itemValue.type);
			}
			if (this.itemIcon != null)
			{
				((XUiV_Sprite)this.itemIcon.ViewComponent).SpriteName = (flag ? this.itemStack.itemValue.GetPropertyOverride("CustomIcon", itemClass.GetIconName()) : this.emptySpriteName);
				((XUiV_Sprite)this.itemIcon.ViewComponent).UIAtlas = (flag ? "ItemIconAtlas" : "ItemIconAtlasGreyscale");
				((XUiV_Sprite)this.itemIcon.ViewComponent).Color = (flag ? Color.white : new Color(1f, 1f, 1f, 0.7f));
				string text = string.Empty;
				if (flag)
				{
					text = itemClass.GetLocalizedItemName();
				}
				base.ViewComponent.ToolTip = (flag ? text : this.emptyTooltipName);
			}
			if (itemClass != null)
			{
				((XUiV_Sprite)this.itemIcon.ViewComponent).Color = this.itemStack.itemValue.ItemClass.GetIconTint(this.itemStack.itemValue);
				if (itemClass.ShowQualityBar)
				{
					if (this.durability != null)
					{
						this.durability.ViewComponent.IsVisible = true;
						this.durabilityBackground.ViewComponent.IsVisible = true;
						XUiV_Sprite xuiV_Sprite = (XUiV_Sprite)this.durability.ViewComponent;
						xuiV_Sprite.Color = QualityInfo.GetQualityColor((int)this.itemValue.Quality);
						xuiV_Sprite.Fill = this.itemValue.PercentUsesLeft;
					}
					if (this.stackValue != null)
					{
						XUiV_Label xuiV_Label = (XUiV_Label)this.stackValue.ViewComponent;
						xuiV_Label.Alignment = NGUIText.Alignment.Center;
						xuiV_Label.Text = ((this.itemStack.itemValue.Quality > 0) ? this.itemStack.itemValue.Quality.ToString() : (this.itemStack.itemValue.IsMod ? "*" : ""));
					}
				}
				else if (this.durability != null)
				{
					this.durability.ViewComponent.IsVisible = false;
					this.durabilityBackground.ViewComponent.IsVisible = false;
				}
				if (this.lockTypeIcon != null)
				{
					if (this.itemStack.itemValue.HasMods())
					{
						(this.lockTypeIcon.ViewComponent as XUiV_Sprite).SpriteName = "ui_game_symbol_modded";
					}
					else
					{
						(this.lockTypeIcon.ViewComponent as XUiV_Sprite).SpriteName = "";
					}
				}
			}
			else
			{
				if (this.durability != null)
				{
					this.durability.ViewComponent.IsVisible = false;
				}
				if (this.durabilityBackground != null)
				{
					this.durabilityBackground.ViewComponent.IsVisible = false;
				}
				if (this.stackValue != null)
				{
					((XUiV_Label)this.stackValue.ViewComponent).Text = "";
				}
				if (this.lockTypeIcon != null)
				{
					(this.lockTypeIcon.ViewComponent as XUiV_Sprite).SpriteName = "";
				}
			}
			this.isDirty = false;
		}
		((XUiV_Label)this.stackValue.ViewComponent).Alignment = ((this.itemStack.itemValue.HasQuality || this.itemStack.itemValue.Modifications.Length != 0) ? NGUIText.Alignment.Center : NGUIText.Alignment.Right);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleItemInspect()
	{
		if (!this.ItemStack.IsEmpty() && this.InfoWindow != null)
		{
			base.Selected = true;
			this.InfoWindow.SetItemStack(this, true);
		}
		this.HandleClickComplete();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleStackSwap()
	{
		ItemClass itemClass = base.xui.dragAndDrop.CurrentStack.itemValue.ItemClass;
		if (itemClass == null)
		{
			return;
		}
		ItemClassArmor itemClassArmor = itemClass as ItemClassArmor;
		if (itemClassArmor != null && itemClassArmor.EquipSlot == this.EquipSlot)
		{
			this.SwapItem();
		}
		base.Selected = false;
		this.HandleClickComplete();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleClickComplete()
	{
		this.lastClicked = false;
		if (this.itemValue.IsEmpty())
		{
			this.SetBorderColor(this.normalBorderColor);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnHovered(bool _isOver)
	{
		this.isOver = _isOver;
		if (_isOver)
		{
			if (!base.Selected)
			{
				this.SetBorderColor(this.hoverBorderColor);
			}
			if (!this.itemStack.IsEmpty())
			{
				this.tweenScale.from = Vector3.one;
				this.tweenScale.to = Vector3.one * 1.5f;
				this.tweenScale.enabled = true;
				this.tweenScale.duration = 0.5f;
			}
		}
		else
		{
			if (!base.Selected)
			{
				this.SetBorderColor(this.normalBorderColor);
			}
			this.tweenScale.from = Vector3.one * 1.5f;
			this.tweenScale.to = Vector3.one;
			this.tweenScale.enabled = true;
			this.tweenScale.duration = 0.5f;
		}
		bool canSwap = false;
		ItemStack currentStack = base.xui.dragAndDrop.CurrentStack;
		ItemClassArmor itemClassArmor = (currentStack.IsEmpty() ? null : currentStack.itemValue.ItemClass) as ItemClassArmor;
		if (itemClassArmor != null)
		{
			canSwap = (this.equipSlot == itemClassArmor.EquipSlot);
		}
		base.xui.calloutWindow.UpdateCalloutsForItemStack(base.ViewComponent.UiTransform.gameObject, this.ItemStack, this.isOver, canSwap, true, true);
		if (!_isOver && this.tweenScale.value != Vector3.one && !this.itemStack.IsEmpty())
		{
			this.tweenScale.from = Vector3.one * 1.5f;
			this.tweenScale.to = Vector3.one;
			this.tweenScale.enabled = true;
			this.tweenScale.duration = 0.5f;
		}
		base.OnHovered(_isOver);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SwapItem()
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
		base.xui.dragAndDrop.CurrentStack = this.itemStack.Clone();
		base.xui.dragAndDrop.PickUpType = XUiC_ItemStack.StackLocationTypes.Equipment;
		this.ItemStack = currentStack.Clone();
		if (this.SlotChangedEvent != null)
		{
			this.SlotChangedEvent(this.SlotNumber, this.itemStack);
		}
	}

	public void HandleMoveToPreferredLocation()
	{
		ItemStack itemStack = this.ItemStack.Clone();
		if (base.xui.PlayerInventory.AddItemToBackpack(itemStack))
		{
			this.ItemValue = ItemStack.Empty.itemValue.Clone();
			if (this.placeSound != null)
			{
				Manager.PlayXUiSound(this.placeSound, 0.75f);
			}
			if (this.itemStack.IsEmpty() && base.Selected)
			{
				base.Selected = false;
				return;
			}
		}
		else if (base.xui.PlayerInventory.AddItemToToolbelt(itemStack))
		{
			this.ItemValue = ItemStack.Empty.itemValue.Clone();
			if (this.placeSound != null)
			{
				Manager.PlayXUiSound(this.placeSound, 0.75f);
			}
			if (this.SlotChangedEvent != null)
			{
				this.SlotChangedEvent(this.SlotNumber, this.itemStack);
			}
			if (this.itemStack.IsEmpty() && base.Selected)
			{
				base.Selected = false;
			}
		}
	}

	public override bool ParseAttribute(string name, string value, XUiController _parent)
	{
		bool flag = base.ParseAttribute(name, value, _parent);
		if (!flag)
		{
			uint num = <PrivateImplementationDetails>.ComputeStringHash(name);
			if (num <= 1352047028U)
			{
				if (num <= 605893325U)
				{
					if (num != 507753414U)
					{
						if (num == 605893325U)
						{
							if (name == "pickup_sound")
							{
								base.xui.LoadData<AudioClip>(value, delegate(AudioClip o)
								{
									this.pickupSound = o;
								});
								return true;
							}
						}
					}
					else if (name == "normal_color")
					{
						this.normalBackgroundColor = StringParsers.ParseColor32(value);
						return true;
					}
				}
				else if (num != 783618599U)
				{
					if (num == 1352047028U)
					{
						if (name == "hover_border_color")
						{
							this.hoverBorderColor = StringParsers.ParseColor32(value);
							return true;
						}
					}
				}
				else if (name == "hover_icon_grow")
				{
					this.HoverIconGrow = StringParsers.ParseFloat(value, 0, -1, NumberStyles.Any);
					return true;
				}
			}
			else if (num <= 1800731603U)
			{
				if (num != 1619874765U)
				{
					if (num == 1800731603U)
					{
						if (name == "normal_background_color")
						{
							XUiC_EquipmentStack.finalPressedColor = StringParsers.ParseColor32(value);
							return true;
						}
					}
				}
				else if (name == "normal_border_color")
				{
					this.normalBorderColor = StringParsers.ParseColor32(value);
					return true;
				}
			}
			else if (num != 3765930259U)
			{
				if (num == 3919060864U)
				{
					if (name == "place_sound")
					{
						base.xui.LoadData<AudioClip>(value, delegate(AudioClip o)
						{
							this.placeSound = o;
						});
						return true;
					}
				}
			}
			else if (name == "selected_border_color")
			{
				this.selectedBorderColor = StringParsers.ParseColor32(value);
				return true;
			}
			return false;
		}
		return flag;
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.isDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemStack itemStack = ItemStack.Empty.Clone();

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemValue itemValue = new ItemValue();

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isDirty = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bHighlightEnabled;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bDropEnabled = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bDisabled;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isOver;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color32 selectedBorderColor = new Color32(222, 206, 163, byte.MaxValue);

	[PublicizedFrom(EAccessModifier.Private)]
	public Color32 hoverBorderColor = new Color32(182, 166, 123, byte.MaxValue);

	[PublicizedFrom(EAccessModifier.Private)]
	public Color32 normalBorderColor = new Color32(96, 96, 96, byte.MaxValue);

	[PublicizedFrom(EAccessModifier.Private)]
	public Color32 normalBackgroundColor = new Color32(96, 96, 96, 96);

	[PublicizedFrom(EAccessModifier.Private)]
	public bool lastClicked;

	[PublicizedFrom(EAccessModifier.Private)]
	public string emptySpriteName = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public string emptyTooltipName = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public AudioClip pickupSound;

	[PublicizedFrom(EAccessModifier.Private)]
	public AudioClip placeSound;

	[PublicizedFrom(EAccessModifier.Private)]
	public EquipmentSlots equipSlot;

	public int PickupSnapDistance = 4;

	public static Color32 finalPressedColor = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);

	public static Color32 backgroundColor = new Color32(96, 96, 96, byte.MaxValue);

	public static Color32 highlightColor = new Color32(222, 206, 163, byte.MaxValue);

	public Color32 holdingColor = new Color32(byte.MaxValue, 128, 0, byte.MaxValue);

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController timer;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController stackValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController itemIcon;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController durability;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController durabilityBackground;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController lockTypeIcon;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController tintedOverlay;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController highlightOverlay;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController overlay;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController background;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblHeadgear;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblChestArmor;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblGloves;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblFootwear;

	[PublicizedFrom(EAccessModifier.Private)]
	public TweenScale tweenScale;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 startMousePos = Vector3.zero;
}
