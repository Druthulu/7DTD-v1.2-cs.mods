using System;
using System.Globalization;
using Audio;
using InControl;
using UnityEngine;
using UnityEngine.Scripting;

[UnityEngine.Scripting.Preserve]
public class XUiC_ItemStack : XUiC_SelectableEntry
{
	public bool isQuickSwap
	{
		get
		{
			return this._isQuickSwap;
		}
		set
		{
			if (this.swapIcon != null)
			{
				this.swapIcon.IsVisible = value;
			}
			this._isQuickSwap = value;
		}
	}

	public Color32 SelectionBorderColor
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return this.selectionBorderColor;
		}
		[PublicizedFrom(EAccessModifier.Protected)]
		set
		{
			if (!this.selectionBorderColor.ColorEquals(value))
			{
				this.selectionBorderColor = value;
				this.IsDirty = true;
			}
		}
	}

	public int SlotNumber { get; set; }

	public XUiC_ItemStack.StackLocationTypes StackLocation { get; set; }

	public event XUiEvent_SlotChangedEventHandler SlotChangedEvent;

	public event XUiEvent_ToolLockChangeEventHandler ToolLockChangedEvent;

	public event XUiEvent_LockChangeEventHandler LockChangedEvent;

	public event XUiEvent_TimeIntervalElapsedEventHandler TimeIntervalElapsedEvent;

	public ItemClass itemClass
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			ItemStack itemStack = this.itemStack;
			if (itemStack == null)
			{
				return null;
			}
			ItemValue itemValue = itemStack.itemValue;
			if (itemValue == null)
			{
				return null;
			}
			return itemValue.ItemClass;
		}
	}

	public ItemClass itemClassOrMissing
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			ItemStack itemStack = this.itemStack;
			if (itemStack == null)
			{
				return null;
			}
			ItemValue itemValue = itemStack.itemValue;
			if (itemValue == null)
			{
				return null;
			}
			return itemValue.ItemClassOrMissing;
		}
	}

	public float HoverIconGrow { get; [PublicizedFrom(EAccessModifier.Private)] set; }

	public bool AssembleLock
	{
		get
		{
			return this.stackLockType == XUiC_ItemStack.StackLockTypes.Assemble;
		}
		set
		{
			this.stackLockType = (value ? XUiC_ItemStack.StackLockTypes.Assemble : XUiC_ItemStack.StackLockTypes.None);
			base.RefreshBindings(false);
		}
	}

	public bool QuestLock
	{
		get
		{
			return this.stackLockType == XUiC_ItemStack.StackLockTypes.Quest;
		}
		set
		{
			this.stackLockType = (value ? XUiC_ItemStack.StackLockTypes.Quest : XUiC_ItemStack.StackLockTypes.None);
			base.RefreshBindings(false);
		}
	}

	public bool ToolLock
	{
		get
		{
			return this.stackLockType == XUiC_ItemStack.StackLockTypes.Tool;
		}
		set
		{
			this.stackLockType = (value ? XUiC_ItemStack.StackLockTypes.Tool : XUiC_ItemStack.StackLockTypes.None);
			XUiEvent_ToolLockChangeEventHandler toolLockChangedEvent = this.ToolLockChangedEvent;
			if (toolLockChangedEvent != null)
			{
				toolLockChangedEvent(this.SlotNumber, this.itemStack, value);
			}
			base.RefreshBindings(false);
		}
	}

	public bool HiddenLock
	{
		get
		{
			return this.stackLockType == XUiC_ItemStack.StackLockTypes.Hidden;
		}
		set
		{
			this.stackLockType = (value ? XUiC_ItemStack.StackLockTypes.Hidden : XUiC_ItemStack.StackLockTypes.None);
			base.RefreshBindings(false);
		}
	}

	public bool AttributeLock
	{
		get
		{
			return this.attributeLock;
		}
		set
		{
			this.attributeLock = value;
			base.RefreshBindings(false);
		}
	}

	public bool StackLock
	{
		get
		{
			return this.stackLockType > XUiC_ItemStack.StackLockTypes.None;
		}
	}

	public bool IsDragAndDrop
	{
		get
		{
			return this.isDragAndDrop;
		}
		set
		{
			this.isDragAndDrop = value;
			if (!value)
			{
				return;
			}
			base.ViewComponent.EventOnPress = false;
			base.ViewComponent.EventOnHover = false;
		}
	}

	public bool IsHolding { get; set; }

	public bool IsLocked { get; [PublicizedFrom(EAccessModifier.Private)] set; }

	public int RepairAmount { get; [PublicizedFrom(EAccessModifier.Private)] set; }

	public bool AllowIconGrow
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return this.itemClass != null;
		}
	}

	public XUiC_ItemInfoWindow InfoWindow { get; set; }

	public bool SimpleClick { get; [PublicizedFrom(EAccessModifier.Internal)] set; }

	public bool AllowDropping { get; [PublicizedFrom(EAccessModifier.Protected)] set; } = true;

	public bool PrefixId { get; [PublicizedFrom(EAccessModifier.Protected)] set; }

	public bool ShowFavorites { get; [PublicizedFrom(EAccessModifier.Private)] set; }

	public float LockTime
	{
		get
		{
			return this.lockTime;
		}
		set
		{
			this.lockTime = value;
			if (value == 0f)
			{
				this.timer.Text = "";
				this.timer.IsVisible = false;
				return;
			}
			this.timer.Text = string.Format("{0:00}:{1:00}", Mathf.Floor(this.lockTime / 60f), Mathf.Floor(this.lockTime % 60f));
			this.timer.IsVisible = true;
		}
	}

	public bool UserLockedSlot
	{
		get
		{
			return this.userLockedSlot;
		}
		set
		{
			if (value == this.userLockedSlot)
			{
				return;
			}
			this.userLockedSlot = value;
			base.RefreshBindings(false);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void setItemStack(ItemStack _stack)
	{
		this.itemStack = _stack.Clone();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void updateItemInfoWindow(XUiC_ItemStack _itemStack)
	{
		this.InfoWindow.SetItemStack(_itemStack, true);
	}

	public ItemStack ItemStack
	{
		get
		{
			return this.itemStack;
		}
		set
		{
			if (!this.itemStack.Equals(value))
			{
				this.setItemStack(value);
				if (this.itemStack.IsEmpty())
				{
					this.itemStack.Clear();
				}
				if (base.Selected)
				{
					this.updateItemInfoWindow(this);
				}
				this.HandleSlotChangeEvent();
				ItemClass itemClass = this.itemStack.itemValue.ItemClass;
				if (itemClass != null && (this.StackLocation == XUiC_ItemStack.StackLocationTypes.Backpack || this.StackLocation == XUiC_ItemStack.StackLocationTypes.ToolBelt))
				{
					this.QuestLock = itemClass.IsQuestItem;
				}
				if (value.IsEmpty())
				{
					this.stackLockType = XUiC_ItemStack.StackLockTypes.None;
				}
				if (this.backgroundTexture != null)
				{
					this.backgroundTexture.IsVisible = false;
					ItemClassBlock itemClassBlock = itemClass as ItemClassBlock;
					if (itemClassBlock != null)
					{
						Block block = itemClassBlock.GetBlock();
						if (block.GetAutoShapeType() != EAutoShapeType.None)
						{
							int uiBackgroundTextureId = block.GetUiBackgroundTextureId(this.itemStack.itemValue.ToBlockValue(), BlockFace.Top);
							if (uiBackgroundTextureId != 0)
							{
								this.backgroundTexture.IsVisible = true;
								MeshDescription meshDescription = MeshDescription.meshes[0];
								UVRectTiling uvrectTiling = meshDescription.textureAtlas.uvMapping[uiBackgroundTextureId];
								Rect uv = uvrectTiling.uv;
								this.backgroundTexture.Texture = meshDescription.textureAtlas.diffuseTexture;
								if (meshDescription.bTextureArray)
								{
									this.backgroundTexture.Material.SetTexture("_BumpMap", meshDescription.textureAtlas.normalTexture);
									this.backgroundTexture.Material.SetFloat("_Index", (float)uvrectTiling.index);
									this.backgroundTexture.Material.SetFloat("_Size", (float)uvrectTiling.blockW);
								}
								else
								{
									this.backgroundTexture.UVRect = uv;
								}
							}
						}
					}
					ItemActionTextureBlock itemActionTextureBlock = ((itemClass != null) ? itemClass.Actions[0] : null) as ItemActionTextureBlock;
					if (itemActionTextureBlock != null)
					{
						if (this.itemStack.itemValue.Meta == 0)
						{
							this.itemStack.itemValue.Meta = itemActionTextureBlock.DefaultTextureID;
						}
						this.backgroundTexture.IsVisible = true;
						MeshDescription meshDescription2 = MeshDescription.meshes[0];
						int textureID = (int)BlockTextureData.list[this.itemStack.itemValue.Meta].TextureID;
						Rect uvrect = (textureID == 0) ? WorldConstants.uvRectZero : meshDescription2.textureAtlas.uvMapping[textureID].uv;
						this.backgroundTexture.Texture = meshDescription2.textureAtlas.diffuseTexture;
						if (meshDescription2.bTextureArray)
						{
							this.backgroundTexture.Material.SetTexture("_BumpMap", meshDescription2.textureAtlas.normalTexture);
							this.backgroundTexture.Material.SetFloat("_Index", (float)meshDescription2.textureAtlas.uvMapping[textureID].index);
							this.backgroundTexture.Material.SetFloat("_Size", (float)meshDescription2.textureAtlas.uvMapping[textureID].blockW);
						}
						else
						{
							this.backgroundTexture.UVRect = uvrect;
						}
					}
				}
				base.RefreshBindings(false);
				this.ResetTweenScale();
			}
			else
			{
				if (this.ItemStack.IsEmpty() && this.backgroundTexture != null)
				{
					this.backgroundTexture.Texture = null;
				}
				if (base.Selected)
				{
					this.updateItemInfoWindow(this);
				}
				base.xui.playerUI.CursorController.HoverTarget = null;
			}
			this.viewComponent.IsSnappable = !this.itemStack.IsEmpty();
			this.IsDirty = true;
		}
	}

	public void ResetTweenScale()
	{
		if (this.tweenScale != null && this.tweenScale.value != Vector3.one)
		{
			this.tweenScale.from = Vector3.one * 1.5f;
			this.tweenScale.to = Vector3.one;
			this.tweenScale.enabled = true;
			this.tweenScale.duration = 0.1f;
		}
	}

	public void ForceSetItemStack(ItemStack _stack)
	{
		bool selected = base.Selected;
		this.itemStack = ItemStack.Empty.Clone();
		XUiEvent_SlotChangedEventHandler slotChangedEvent = this.SlotChangedEvent;
		if (slotChangedEvent != null)
		{
			slotChangedEvent(this.SlotNumber, this.itemStack);
		}
		if (!_stack.IsEmpty())
		{
			base.Selected = selected;
		}
		this.ItemStack = _stack;
		XUiEvent_SlotChangedEventHandler slotChangedEvent2 = this.SlotChangedEvent;
		if (slotChangedEvent2 == null)
		{
			return;
		}
		slotChangedEvent2(this.SlotNumber, this.itemStack);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void HandleSlotChangeEvent()
	{
		if (this.itemStack.IsEmpty() && base.Selected)
		{
			base.Selected = false;
		}
		XUiEvent_SlotChangedEventHandler slotChangedEvent = this.SlotChangedEvent;
		if (slotChangedEvent == null)
		{
			return;
		}
		slotChangedEvent(this.SlotNumber, this.itemStack);
	}

	public override void Init()
	{
		base.Init();
		XUiController childById = base.GetChildById("timer");
		if (childById != null)
		{
			this.timer = (childById.ViewComponent as XUiV_Label);
		}
		XUiController childById2 = base.GetChildById("itemIcon");
		if (childById2 != null)
		{
			this.itemIconSprite = (childById2.ViewComponent as XUiV_Sprite);
		}
		XUiController childById3 = base.GetChildById("lockTypeIcon");
		if (childById3 != null)
		{
			this.lockTypeIcon = (childById3.ViewComponent as XUiV_Sprite);
		}
		XUiController childById4 = base.GetChildById("backgroundTexture");
		if (childById4 != null)
		{
			this.backgroundTexture = (childById4.ViewComponent as XUiV_Texture);
			if (this.backgroundTexture != null)
			{
				this.backgroundTexture.CreateMaterial();
			}
		}
		XUiController childById5 = base.GetChildById("cancel");
		if (childById5 != null)
		{
			this.cancelIcon = (childById5.ViewComponent as XUiV_Sprite);
		}
		XUiController childById6 = base.GetChildById("quickswapIcon");
		if (childById6 != null)
		{
			this.swapIcon = (childById6.ViewComponent as XUiV_Sprite);
			if (this.swapIcon != null)
			{
				this.swapIcon.IsVisible = this.isQuickSwap;
			}
			else
			{
				Log.Warning("[XUI] Failed to convert \"quickswapIcon\" to a XUiV_Sprite");
			}
		}
		XUiController childById7 = base.GetChildById("rectSlotLock");
		if (childById7 != null)
		{
			childById7.OnHover += delegate(XUiController _, bool _over)
			{
			};
			childById7.OnPress += delegate(XUiController _, int _)
			{
				this.UserLockedSlot = !this.UserLockedSlot;
				base.RefreshBindings(false);
			};
		}
		this.tweenScale = this.itemIconSprite.UiTransform.gameObject.AddComponent<TweenScale>();
		base.ViewComponent.UseSelectionBox = false;
	}

	public void UpdateTimer(float _dt)
	{
		if (!this.IsLocked)
		{
			return;
		}
		if (this.lockType == XUiC_ItemStack.LockTypes.Shell || this.lockType == XUiC_ItemStack.LockTypes.Burning)
		{
			return;
		}
		float num = this.lockTime;
		if (this.lockTime > 0f)
		{
			this.lockTime -= _dt;
			if (this.currentInterval == -1)
			{
				this.currentInterval = (int)this.lockTime / this.TimeInterval;
			}
			if (this.TimeIntervalElapsedEvent != null && this.TimeInterval != 0)
			{
				int num2 = (int)this.lockTime / this.TimeInterval;
				if (num2 != this.currentInterval)
				{
					this.TimeIntervalElapsedEvent(this.lockTime, this);
					this.currentInterval = num2;
				}
			}
		}
		if (this.lockTime <= 0f && num != 0f)
		{
			XUiEvent_TimeIntervalElapsedEventHandler timeIntervalElapsedEvent = this.TimeIntervalElapsedEvent;
			if (timeIntervalElapsedEvent != null)
			{
				timeIntervalElapsedEvent(this.lockTime, this);
			}
			if (this.LockChangedEvent != null)
			{
				this.LockChangedEvent(this.lockType, this);
			}
			else
			{
				this.IsLocked = false;
			}
		}
		if (this.lockTime <= 0f)
		{
			this.timer.IsVisible = false;
			this.timer.Text = "";
			return;
		}
		this.timer.IsVisible = true;
		this.timer.Text = string.Format("{0:00}:{1:00}", Mathf.Floor(this.lockTime / 60f), Mathf.Floor(this.lockTime % 60f));
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
			bool mouseButtonUp2 = cursorController.GetMouseButtonUp(UICamera.MouseButton.RightButton);
			bool mouseButtonDown2 = cursorController.GetMouseButtonDown(UICamera.MouseButton.RightButton);
			bool mouseButton2 = cursorController.GetMouseButton(UICamera.MouseButton.RightButton);
			if (!this.IsLocked && !this.isDragAndDrop)
			{
				if (this.isOver && UICamera.hoveredObject == base.ViewComponent.UiTransform.gameObject && base.ViewComponent.EventOnPress)
				{
					if (guiactions.LastInputType == BindingSourceType.DeviceBindingSource)
					{
						bool wasReleased = guiactions.Submit.WasReleased;
						bool wasReleased2 = guiactions.HalfStack.WasReleased;
						bool wasPressed = guiactions.Inspect.WasPressed;
						bool wasReleased3 = guiactions.RightStick.WasReleased;
						if (this.SimpleClick && !this.StackLock)
						{
							if (wasReleased)
							{
								this.HandleMoveToPreferredLocation();
							}
							else if (wasPressed)
							{
								this.HandleItemInspect();
							}
						}
						else if (base.xui.dragAndDrop.CurrentStack.IsEmpty() && !this.ItemStack.IsEmpty())
						{
							if (!this.StackLock)
							{
								if (wasReleased)
								{
									this.SwapItem();
								}
								else if (wasReleased2)
								{
									this.HandlePartialStackPickup();
								}
								else if (wasReleased3)
								{
									this.HandleMoveToPreferredLocation();
								}
								else if (wasPressed)
								{
									this.HandleItemInspect();
								}
							}
						}
						else if (!this.StackLock)
						{
							if (wasReleased)
							{
								this.HandleStackSwap();
							}
							else if (wasReleased2 && this.AllowDropping)
							{
								this.HandleDropOne();
							}
						}
					}
					else if (this.SimpleClick && !this.StackLock)
					{
						if (mouseButtonUp)
						{
							this.HandleMoveToPreferredLocation();
						}
					}
					else if (InputUtils.ShiftKeyPressed)
					{
						if (!this.StackLock && mouseButtonUp)
						{
							this.HandleMoveToPreferredLocation();
						}
					}
					else if (mouseButton || mouseButton2)
					{
						if (base.xui.dragAndDrop.CurrentStack.IsEmpty() && !this.ItemStack.IsEmpty())
						{
							if (!this.lastClicked)
							{
								this.startMousePos = a;
							}
							else if (Mathf.Abs((a - this.startMousePos).magnitude) > (float)this.PickupSnapDistance && !this.StackLock)
							{
								if (mouseButton)
								{
									this.SwapItem();
								}
								else
								{
									this.HandlePartialStackPickup();
								}
							}
						}
						if (mouseButtonDown || mouseButtonDown2)
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
						else if (this.lastClicked && !this.StackLock)
						{
							this.HandleStackSwap();
						}
					}
					else if (mouseButtonUp2)
					{
						if (this.lastClicked && !this.StackLock && this.AllowDropping)
						{
							this.HandleDropOne();
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
					if ((this.isOver || this.itemIconSprite.UiTransform.localScale != Vector3.one) && this.tweenScale.value != Vector3.one && !this.itemStack.IsEmpty())
					{
						this.tweenScale.from = Vector3.one * 1.5f;
						this.tweenScale.to = Vector3.one;
						this.tweenScale.enabled = true;
						this.tweenScale.duration = 0.5f;
					}
				}
			}
			else if (this.IsLocked && ((guiactions.LastInputType == BindingSourceType.DeviceBindingSource && guiactions.Submit.WasReleased) || (guiactions.LastInputType != BindingSourceType.DeviceBindingSource && guiactions.LeftClick.WasPressed)) && this.isOver)
			{
				XUiEvent_LockChangeEventHandler lockChangedEvent = this.LockChangedEvent;
				if (lockChangedEvent != null)
				{
					lockChangedEvent(XUiC_ItemStack.LockTypes.None, this);
				}
			}
		}
		this.updateBorderColor();
		if (this.flashLockTypeIcon)
		{
			Color green = Color.green;
			float num = Mathf.PingPong(Time.time, 0.5f);
			this.setLockTypeIconColor(Color.Lerp(Color.grey, green, num * 4f));
		}
		if (this.IsDirty)
		{
			this.IsDirty = false;
			this.updateLockTypeIcon();
			base.RefreshBindings(false);
		}
		if (this.IsLocked && this.lockType != XUiC_ItemStack.LockTypes.None)
		{
			this.UpdateTimer(_dt);
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateBorderColor()
	{
		if (this.IsDragAndDrop)
		{
			this.SelectionBorderColor = Color.clear;
			return;
		}
		if (base.Selected)
		{
			this.SelectionBorderColor = this.selectColor;
			return;
		}
		if (this.isOver)
		{
			this.SelectionBorderColor = this.highlightColor;
			return;
		}
		if (this.IsHolding)
		{
			this.SelectionBorderColor = this.holdingColor;
			return;
		}
		this.SelectionBorderColor = this.backgroundColor;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual bool CanSwap(ItemStack _stack)
	{
		return true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void HandleItemInspect()
	{
		if (!this.ItemStack.IsEmpty() && this.InfoWindow != null)
		{
			base.Selected = true;
			this.InfoWindow.SetMaxCountOnDirty = true;
			this.updateItemInfoWindow(this);
		}
		this.HandleClickComplete();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void HandleStackSwap()
	{
		base.xui.currentPopupMenu.ClearItems();
		if (!this.AllowDropping)
		{
			base.xui.dragAndDrop.CurrentStack = ItemStack.Empty.Clone();
			base.xui.dragAndDrop.PickUpType = this.StackLocation;
		}
		bool flag = false;
		ItemStack currentStack = base.xui.dragAndDrop.CurrentStack;
		ItemClass itemClassOrMissing = currentStack.itemValue.ItemClassOrMissing;
		int num = 0;
		if (itemClassOrMissing != null)
		{
			num = ((this.OverrideStackCount == -1) ? itemClassOrMissing.Stacknumber.Value : Mathf.Min(itemClassOrMissing.Stacknumber.Value, this.OverrideStackCount));
			if (!currentStack.IsEmpty() && this.itemStack.IsEmpty() && num < currentStack.count)
			{
				flag = true;
			}
		}
		if (!flag && (this.itemStack.IsEmpty() || currentStack.IsEmpty()))
		{
			this.SwapItem();
			base.Selected = false;
		}
		else if (!flag && (!this.itemStack.itemValue.ItemClassOrMissing.CanStack() || !itemClassOrMissing.CanStack()))
		{
			this.SwapItem();
			base.Selected = false;
		}
		else if (currentStack.itemValue.type == this.itemStack.itemValue.type && !currentStack.itemValue.HasQuality && !this.itemStack.itemValue.HasQuality)
		{
			if (currentStack.count + this.itemStack.count > num)
			{
				int count = currentStack.count + this.itemStack.count - num;
				ItemStack itemStack = this.itemStack.Clone();
				itemStack.count = num;
				currentStack.count = count;
				base.xui.dragAndDrop.CurrentStack = currentStack;
				base.xui.dragAndDrop.PickUpType = this.StackLocation;
				this.ItemStack = itemStack;
				this.PlayPickupSound(null);
			}
			else
			{
				ItemStack itemStack2 = this.itemStack.Clone();
				itemStack2.count += currentStack.count;
				this.ItemStack = itemStack2;
				base.xui.dragAndDrop.CurrentStack = ItemStack.Empty.Clone();
				this.PlayPlaceSound(null);
			}
			if (base.Selected)
			{
				this.updateItemInfoWindow(this);
			}
		}
		else if (flag)
		{
			int count2 = currentStack.count - num;
			ItemStack itemStack3 = currentStack.Clone();
			itemStack3.count = num;
			currentStack.count = count2;
			base.xui.dragAndDrop.CurrentStack = currentStack;
			base.xui.dragAndDrop.PickUpType = this.StackLocation;
			this.ItemStack = itemStack3;
			this.PlayPickupSound(null);
		}
		else
		{
			this.SwapItem();
			base.Selected = false;
		}
		this.HandleClickComplete();
		base.xui.calloutWindow.UpdateCalloutsForItemStack(base.ViewComponent.UiTransform.gameObject, this.ItemStack, this.isOver, true, true, true);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void HandlePartialStackPickup()
	{
		ItemStack itemStack = base.xui.dragAndDrop.CurrentStack;
		if (itemStack.IsEmpty() && !this.itemStack.IsEmpty())
		{
			int num = this.itemStack.count / 2;
			if (num > 0)
			{
				itemStack = this.itemStack.Clone();
				itemStack.count = num;
				if (this.AllowDropping)
				{
					ItemStack itemStack2 = this.itemStack.Clone();
					itemStack2.count -= num;
					this.ItemStack = itemStack2;
				}
				base.xui.dragAndDrop.CurrentStack = itemStack;
				base.xui.dragAndDrop.PickUpType = this.StackLocation;
				this.PlayPickupSound(null);
			}
		}
		if (base.Selected)
		{
			base.Selected = false;
		}
		this.HandleClickComplete();
		base.xui.calloutWindow.UpdateCalloutsForItemStack(base.ViewComponent.UiTransform.gameObject, this.ItemStack, this.isOver, true, true, true);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void HandleDropOne()
	{
		ItemStack currentStack = base.xui.dragAndDrop.CurrentStack;
		if (!currentStack.IsEmpty())
		{
			int num = 1;
			if (this.itemStack.IsEmpty())
			{
				ItemStack itemStack = currentStack.Clone();
				itemStack.count = num;
				currentStack.count -= num;
				base.xui.dragAndDrop.CurrentStack = currentStack;
				this.ItemStack = itemStack;
				this.PlayPlaceSound(null);
			}
			else if (currentStack.itemValue.type == this.itemStack.itemValue.type)
			{
				ItemClass itemClassOrMissing = currentStack.itemValue.ItemClassOrMissing;
				int num2 = (this.OverrideStackCount == -1) ? itemClassOrMissing.Stacknumber.Value : Mathf.Min(itemClassOrMissing.Stacknumber.Value, this.OverrideStackCount);
				if (this.itemStack.count + 1 <= num2)
				{
					ItemStack itemStack2 = this.itemStack.Clone();
					itemStack2.count++;
					currentStack.count--;
					this.ItemStack = itemStack2.Clone();
					base.xui.dragAndDrop.CurrentStack = currentStack;
					XUiEvent_SlotChangedEventHandler slotChangedEvent = this.SlotChangedEvent;
					if (slotChangedEvent != null)
					{
						slotChangedEvent(this.SlotNumber, this.itemStack);
					}
					this.IsDirty = true;
				}
				this.PlayPlaceSound(null);
			}
			if (currentStack.count == 0)
			{
				base.xui.dragAndDrop.CurrentStack = ItemStack.Empty.Clone();
			}
		}
		base.Selected = false;
		this.HandleClickComplete();
		base.xui.calloutWindow.UpdateCalloutsForItemStack(base.ViewComponent.UiTransform.gameObject, this.ItemStack, this.isOver, true, true, true);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void HandleClickComplete()
	{
		this.lastClicked = false;
		if (this.itemIconSprite.UiTransform.localScale.x <= 1f)
		{
			return;
		}
		if (this.itemStack.IsEmpty())
		{
			return;
		}
		this.tweenScale.from = Vector3.one * 1.5f;
		this.tweenScale.to = Vector3.one;
		this.tweenScale.enabled = true;
		this.tweenScale.duration = 0.5f;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnHovered(bool _isOver)
	{
		this.isOver = _isOver;
		if (!this.IsLocked)
		{
			if (_isOver)
			{
				if (this.InfoWindow != null && this.InfoWindow.ViewComponent.IsVisible)
				{
					this.InfoWindow.HoverEntry = this;
				}
				if (this.AllowIconGrow)
				{
					this.tweenScale.from = Vector3.one;
					this.tweenScale.to = Vector3.one * 1.5f;
					this.tweenScale.enabled = true;
					this.tweenScale.duration = 0.5f;
				}
			}
			else
			{
				if (this.InfoWindow != null && this.InfoWindow.ViewComponent.IsVisible)
				{
					this.InfoWindow.HoverEntry = null;
				}
				if (this.AllowIconGrow)
				{
					this.tweenScale.from = Vector3.one * 1.5f;
					this.tweenScale.to = Vector3.one;
					this.tweenScale.enabled = true;
					this.tweenScale.duration = 0.5f;
				}
			}
		}
		else if (this.IsLocked && this.cancelIcon != null)
		{
			this.cancelIcon.IsVisible = _isOver;
		}
		base.xui.calloutWindow.UpdateCalloutsForItemStack(base.ViewComponent.UiTransform.gameObject, this.ItemStack, this.isOver, true, !this.StackLock, true);
		base.OnHovered(_isOver);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void SwapItem()
	{
		base.xui.currentPopupMenu.ClearItems();
		ItemStack currentStack = base.xui.dragAndDrop.CurrentStack;
		if (!currentStack.IsEmpty() && !this.CanSwap(currentStack))
		{
			return;
		}
		if (this.StackLocation == XUiC_ItemStack.StackLocationTypes.LootContainer && base.xui.dragAndDrop.PickUpType != XUiC_ItemStack.StackLocationTypes.LootContainer && !currentStack.IsEmpty() && !currentStack.itemValue.ItemClassOrMissing.CanPlaceInContainer())
		{
			Manager.PlayInsidePlayerHead("ui_denied", -1, 0f, false, false);
			GameManager.ShowTooltip(base.xui.playerUI.entityPlayer, "Quest Items cannot be placed in containers.", false);
			return;
		}
		if (this.itemStack.IsEmpty())
		{
			this.PlayPlaceSound(currentStack);
		}
		else
		{
			this.PlayPickupSound(null);
		}
		base.xui.dragAndDrop.CurrentStack = this.itemStack.Clone();
		base.xui.dragAndDrop.PickUpType = this.StackLocation;
		if (this.StackLocation == XUiC_ItemStack.StackLocationTypes.ToolBelt)
		{
			base.xui.dragAndDrop.CurrentStack.Deactivate();
		}
		this.ForceSetItemStack(currentStack.Clone());
		base.xui.calloutWindow.UpdateCalloutsForItemStack(base.ViewComponent.UiTransform.gameObject, this.ItemStack, this.isOver, true, true, true);
	}

	public void HandleMoveToPreferredLocation()
	{
		base.xui.currentPopupMenu.ClearItems();
		if (this.ItemStack.IsEmpty())
		{
			return;
		}
		if (this.StackLock)
		{
			return;
		}
		if (this.StackLocation == XUiC_ItemStack.StackLocationTypes.ToolBelt)
		{
			this.ItemStack.Deactivate();
		}
		int count = this.ItemStack.count;
		switch (this.StackLocation)
		{
		case XUiC_ItemStack.StackLocationTypes.Backpack:
		case XUiC_ItemStack.StackLocationTypes.ToolBelt:
		{
			XUiM_AssembleItem assembleItem = base.xui.AssembleItem;
			bool flag = ((assembleItem != null) ? assembleItem.CurrentItem : null) != null;
			if (base.xui.vehicle != null && !flag)
			{
				string vehicleSlotType = this.ItemStack.itemValue.ItemClass.VehicleSlotType;
				ItemStack itemStack;
				if (vehicleSlotType != "" && base.xui.Vehicle.SetPart(base.xui, vehicleSlotType, this.ItemStack, out itemStack))
				{
					this.PlayPlaceSound(null);
					this.ItemStack = itemStack;
					this.HandleSlotChangeEvent();
					return;
				}
				if (base.xui.vehicle.GetVehicle().HasStorage())
				{
					XUiC_VehicleContainer childByType = base.xui.FindWindowGroupByName(XUiC_VehicleStorageWindowGroup.ID).GetChildByType<XUiC_VehicleContainer>();
					if (childByType != null)
					{
						if (childByType.AddItem(this.ItemStack))
						{
							this.PlayPlaceSound(null);
							this.ItemStack = ItemStack.Empty.Clone();
							this.HandleSlotChangeEvent();
							return;
						}
						if (count != this.ItemStack.count)
						{
							this.PlayPlaceSound(null);
							if (this.ItemStack.count == 0)
							{
								this.ItemStack = ItemStack.Empty.Clone();
							}
							this.HandleSlotChangeEvent();
							return;
						}
					}
				}
			}
			if (flag && this.ItemStack.itemValue.ItemClass is ItemClassModifier)
			{
				ItemStack itemStack2;
				if (base.xui.AssembleItem.AddPartToItem(this.ItemStack, out itemStack2))
				{
					this.PlayPlaceSound(null);
					this.ItemStack = itemStack2;
					this.HandleSlotChangeEvent();
					return;
				}
				Manager.PlayInsidePlayerHead("ui_denied", -1, 0f, false, false);
				return;
			}
			else
			{
				if (base.xui.PlayerEquipment != null && base.xui.PlayerEquipment.IsOpen && this.itemStack.itemValue.ItemClass.IsEquipment)
				{
					this.PlayPlaceSound(null);
					this.ItemStack = base.xui.PlayerEquipment.EquipItem(this.ItemStack);
					this.HandleSlotChangeEvent();
					return;
				}
				if (base.xui.lootContainer != null)
				{
					XUiC_LootContainer childByType2 = base.xui.FindWindowGroupByName(XUiC_LootWindowGroup.ID).GetChildByType<XUiC_LootContainer>();
					if (XUiM_LootContainer.AddItem(this.ItemStack, base.xui))
					{
						this.PlayPlaceSound(null);
						this.ItemStack = ItemStack.Empty.Clone();
						this.HandleSlotChangeEvent();
						if (childByType2 != null)
						{
							childByType2.SetSlots(base.xui.lootContainer, base.xui.lootContainer.items);
						}
						return;
					}
					if (count != this.ItemStack.count)
					{
						this.PlayPlaceSound(null);
						if (this.ItemStack.count == 0)
						{
							this.ItemStack = ItemStack.Empty.Clone();
						}
						this.HandleSlotChangeEvent();
						if (childByType2 != null)
						{
							childByType2.SetSlots(base.xui.lootContainer, base.xui.lootContainer.items);
						}
						return;
					}
				}
				if (base.xui.currentWorkstationToolGrid != null && base.xui.currentWorkstationToolGrid.TryAddTool(this.itemClass, this.ItemStack))
				{
					this.PlayPlaceSound(null);
					this.ItemStack = ItemStack.Empty.Clone();
					this.HandleSlotChangeEvent();
					return;
				}
				if (base.xui.currentWorkstationFuelGrid != null && this.itemClass.FuelValue != null && this.itemClass.FuelValue.Value > 0)
				{
					if (base.xui.currentWorkstationFuelGrid.AddItem(this.itemClass, this.ItemStack))
					{
						this.PlayPlaceSound(null);
						this.ItemStack = ItemStack.Empty.Clone();
						this.HandleSlotChangeEvent();
						return;
					}
					if (count != this.ItemStack.count)
					{
						this.PlayPlaceSound(null);
						if (this.ItemStack.count == 0)
						{
							this.ItemStack = ItemStack.Empty.Clone();
						}
						this.HandleSlotChangeEvent();
						return;
					}
				}
				if (base.xui.currentDewCollectorModGrid != null && base.xui.currentDewCollectorModGrid.TryAddMod(this.itemClass, this.ItemStack))
				{
					this.PlayPlaceSound(null);
					this.ItemStack = ItemStack.Empty.Clone();
					this.HandleSlotChangeEvent();
					return;
				}
				if (base.xui.currentCombineGrid != null && base.xui.currentCombineGrid.TryAddItemToSlot(this.itemClass, this.ItemStack))
				{
					this.PlayPlaceSound(null);
					this.ItemStack = ItemStack.Empty.Clone();
					this.HandleSlotChangeEvent();
					return;
				}
				if (base.xui.powerSourceSlots != null && base.xui.powerSourceSlots.TryAddItemToSlot(this.itemClass, this.ItemStack))
				{
					this.PlayPlaceSound(null);
					this.ItemStack = ItemStack.Empty.Clone();
					this.HandleSlotChangeEvent();
					return;
				}
				if (base.xui.powerAmmoSlots != null)
				{
					if (base.xui.powerAmmoSlots.TryAddItemToSlot(this.itemClass, this.ItemStack))
					{
						this.PlayPlaceSound(null);
						this.ItemStack = ItemStack.Empty.Clone();
						this.HandleSlotChangeEvent();
						return;
					}
					if (count != this.ItemStack.count)
					{
						this.PlayPlaceSound(null);
						if (this.ItemStack.count == 0)
						{
							this.ItemStack = ItemStack.Empty.Clone();
						}
						this.HandleSlotChangeEvent();
						return;
					}
				}
				if (base.xui.Trader.Trader != null && (this.StackLocation == XUiC_ItemStack.StackLocationTypes.Backpack || this.StackLocation == XUiC_ItemStack.StackLocationTypes.ToolBelt))
				{
					this.HandleItemInspect();
					this.InfoWindow.SetMaxCountOnDirty = true;
					return;
				}
				if (this.StackLocation == XUiC_ItemStack.StackLocationTypes.Backpack)
				{
					if (base.xui.PlayerInventory.AddItemToToolbelt(this.ItemStack))
					{
						this.PlayPlaceSound(null);
						this.ItemStack = ItemStack.Empty.Clone();
						this.HandleSlotChangeEvent();
						return;
					}
					if (count != this.ItemStack.count)
					{
						this.PlayPlaceSound(null);
						if (this.ItemStack.count == 0)
						{
							this.ItemStack = ItemStack.Empty.Clone();
						}
						this.HandleSlotChangeEvent();
						return;
					}
				}
				else if (this.StackLocation == XUiC_ItemStack.StackLocationTypes.ToolBelt)
				{
					if (base.xui.PlayerInventory.AddItemToBackpack(this.ItemStack))
					{
						this.PlayPlaceSound(null);
						this.ItemStack = ItemStack.Empty.Clone();
						this.HandleSlotChangeEvent();
						return;
					}
					if (count != this.ItemStack.count)
					{
						this.PlayPlaceSound(null);
						if (this.ItemStack.count == 0)
						{
							this.ItemStack = ItemStack.Empty.Clone();
						}
						this.HandleSlotChangeEvent();
						return;
					}
				}
			}
			break;
		}
		case XUiC_ItemStack.StackLocationTypes.LootContainer:
		case XUiC_ItemStack.StackLocationTypes.Workstation:
		case XUiC_ItemStack.StackLocationTypes.Merge:
			if (base.xui.PlayerInventory.AddItem(this.ItemStack))
			{
				this.PlayPlaceSound(null);
				this.ItemStack = ItemStack.Empty.Clone();
				this.HandleSlotChangeEvent();
			}
			else if (count != this.ItemStack.count)
			{
				this.PlayPlaceSound(null);
				if (this.ItemStack.count == 0)
				{
					this.ItemStack = ItemStack.Empty.Clone();
				}
				this.HandleSlotChangeEvent();
				return;
			}
			break;
		case XUiC_ItemStack.StackLocationTypes.Creative:
		{
			ItemStack itemStack3 = this.itemStack.Clone();
			if (!base.xui.PlayerInventory.AddItem(itemStack3))
			{
				return;
			}
			this.PlayPlaceSound(null);
			break;
		}
		}
		base.xui.calloutWindow.UpdateCalloutsForItemStack(base.ViewComponent.UiTransform.gameObject, this.ItemStack, this.isOver, true, true, true);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void PlayPlaceSound(ItemStack newStack = null)
	{
		string text;
		if (newStack != null)
		{
			text = ((newStack.itemValue.ItemClass == null) ? "" : newStack.itemValue.ItemClass.SoundPlace);
		}
		else
		{
			text = ((this.itemStack.itemValue.ItemClass == null) ? "" : this.itemStack.itemValue.ItemClass.SoundPlace);
		}
		if (text != "")
		{
			if (text != null)
			{
				Manager.PlayInsidePlayerHead(text, -1, 0f, false, false);
				return;
			}
		}
		else if (this.placeSound != null)
		{
			Manager.PlayXUiSound(this.placeSound, 0.75f);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void PlayPickupSound(ItemStack newStack = null)
	{
		ItemStack itemStack = (newStack != null) ? newStack : this.itemStack;
		string text = (itemStack.IsEmpty() || itemStack.itemValue.ItemClass == null) ? "" : itemStack.itemValue.ItemClass.SoundPickup;
		if (text != "")
		{
			if (text != null)
			{
				Manager.PlayInsidePlayerHead(text, -1, 0f, false, false);
				return;
			}
		}
		else if (this.pickupSound != null)
		{
			Manager.PlayXUiSound(this.pickupSound, 0.75f);
		}
	}

	public void UnlockStack()
	{
		this.lockType = XUiC_ItemStack.LockTypes.None;
		this.IsLocked = false;
		this.lockTime = 0f;
		this.lockSprite = "";
		this.setLockTypeIconColor(Color.white);
		this.RepairAmount = 0;
		this.timer.IsVisible = false;
		if (this.cancelIcon != null)
		{
			this.cancelIcon.IsVisible = false;
		}
		this.IsDirty = true;
	}

	public void LockStack(XUiC_ItemStack.LockTypes _lockType, float _time, int _count, BaseItemActionEntry _itemActionEntry)
	{
		if (_lockType == XUiC_ItemStack.LockTypes.Crafting)
		{
			this.lockSprite = _itemActionEntry.IconName;
		}
		else if (_lockType == XUiC_ItemStack.LockTypes.Scrapping)
		{
			this.lockSprite = "ui_game_symbol_scrap";
		}
		else if (_lockType == XUiC_ItemStack.LockTypes.Burning)
		{
			this.lockSprite = "ui_game_symbol_campfire";
		}
		else if (_lockType == XUiC_ItemStack.LockTypes.Repairing)
		{
			this.lockSprite = "ui_game_symbol_hammer";
		}
		this.IsLocked = true;
		this.lockType = _lockType;
		if (_lockType == XUiC_ItemStack.LockTypes.Repairing)
		{
			this.RepairAmount = _count;
		}
		this.LockTime = _time;
		this.IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateLockTypeIcon()
	{
		if (this.IsLocked && this.lockType != XUiC_ItemStack.LockTypes.None)
		{
			return;
		}
		this.lockSprite = "";
		if (this.itemClass != null)
		{
			ItemClassBlock itemClassBlock = this.itemClass as ItemClassBlock;
			if (itemClassBlock != null && ((!itemClassBlock.GetBlock().SelectAlternates && this.itemStack.itemValue.Texture > 0L) || this.itemStack.itemValue.TextureAllSides > 0))
			{
				this.lockSprite = "ui_game_symbol_paint_brush";
			}
			ItemClassModifier itemClassModifier = this.itemClass as ItemClassModifier;
			if (itemClassModifier != null)
			{
				this.lockSprite = "ui_game_symbol_assemble";
				if (itemClassModifier.HasAnyTags(ItemClassModifier.CosmeticModTypes))
				{
					this.lockSprite = "ui_game_symbol_paint_bucket";
				}
				if (base.xui.AssembleItem.CurrentItem != null)
				{
					if ((itemClassModifier.InstallableTags.IsEmpty || base.xui.AssembleItem.CurrentItem.itemValue.ItemClass.HasAnyTags(itemClassModifier.InstallableTags)) && !base.xui.AssembleItem.CurrentItem.itemValue.ItemClass.HasAnyTags(itemClassModifier.DisallowedTags))
					{
						this.flashLockTypeIcon = true;
					}
					else
					{
						this.setLockTypeIconColor(Color.grey);
						this.flashLockTypeIcon = false;
					}
				}
				else
				{
					this.setLockTypeIconColor(Color.white);
					this.flashLockTypeIcon = false;
				}
			}
			if (this.itemStack.itemValue.HasMods())
			{
				this.lockSprite = "ui_game_symbol_modded";
				this.setLockTypeIconColor(Color.white);
				this.flashLockTypeIcon = false;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setLockTypeIconColor(Color _color)
	{
		if (this.lockTypeIcon == null)
		{
			return;
		}
		this.lockTypeIcon.Color = _color;
	}

	public void ForceRefreshItemStack()
	{
		XUiEvent_SlotChangedEventHandler slotChangedEvent = this.SlotChangedEvent;
		if (slotChangedEvent == null)
		{
			return;
		}
		slotChangedEvent(this.SlotNumber, this.itemStack);
	}

	public bool IsFavorite
	{
		get
		{
			return !this.itemStack.IsEmpty() && base.xui.playerUI.entityPlayer.favoriteCreativeStacks.Contains((ushort)this.itemStack.itemValue.type);
		}
	}

	public virtual string ItemIcon
	{
		get
		{
			if (this.itemStack.IsEmpty())
			{
				return "";
			}
			ItemClass itemClassOrMissing = this.itemClassOrMissing;
			ItemClassBlock itemClassBlock = itemClassOrMissing as ItemClassBlock;
			Block block = (itemClassBlock != null) ? itemClassBlock.GetBlock() : null;
			if (block == null)
			{
				return this.itemStack.itemValue.GetPropertyOverride(ItemClass.PropCustomIcon, itemClassOrMissing.GetIconName());
			}
			if (!block.SelectAlternates)
			{
				return this.itemStack.itemValue.GetPropertyOverride(ItemClass.PropCustomIcon, itemClassOrMissing.GetIconName());
			}
			return block.GetAltBlockValue(this.itemStack.itemValue.Meta).Block.GetIconName();
		}
	}

	public virtual string ItemIconColor
	{
		get
		{
			ItemClass itemClassOrMissing = this.itemClassOrMissing;
			if (itemClassOrMissing == null)
			{
				return "255,255,255,0";
			}
			Color32 v = itemClassOrMissing.GetIconTint(this.itemStack.itemValue);
			return this.itemiconcolorFormatter.Format(v);
		}
	}

	public bool GreyedOut
	{
		get
		{
			return this.itemIconSprite.UIAtlas == "ItemIconAtlasGreyscale";
		}
		set
		{
			if (this.itemIconSprite != null)
			{
				this.itemIconSprite.UIAtlas = (value ? "ItemIconAtlasGreyscale" : "ItemIconAtlas");
			}
		}
	}

	public string ItemNameText
	{
		get
		{
			if (this.itemStack.IsEmpty())
			{
				return "";
			}
			ItemClass itemClassOrMissing = this.itemClassOrMissing;
			string text = itemClassOrMissing.GetLocalizedItemName();
			if (itemClassOrMissing.IsBlock())
			{
				text = Block.list[this.itemStack.itemValue.type].GetLocalizedBlockName(this.itemStack.itemValue);
			}
			if (!this.PrefixId)
			{
				return text;
			}
			int itemOrBlockId = this.itemStack.itemValue.GetItemOrBlockId();
			return string.Format("{0}\n({1}) {2}", text, itemOrBlockId, itemClassOrMissing.Name);
		}
	}

	public bool ShowDurability
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			if (!this.IsLocked || this.lockType == XUiC_ItemStack.LockTypes.None)
			{
				ItemClass itemClass = this.itemClass;
				return itemClass != null && itemClass.ShowQualityBar;
			}
			return false;
		}
	}

	public override bool GetBindingValue(ref string _value, string _bindingName)
	{
		uint num = <PrivateImplementationDetails>.ComputeStringHash(_bindingName);
		if (num <= 1768227542U)
		{
			if (num <= 770575247U)
			{
				if (num <= 684314633U)
				{
					if (num != 402247192U)
					{
						if (num == 684314633U)
						{
							if (_bindingName == "stacklockicon")
							{
								if (this.stackLockType == XUiC_ItemStack.StackLockTypes.Quest)
								{
									_value = "ui_game_symbol_quest";
								}
								else if (this.attributeLock && this.itemStack.IsEmpty())
								{
									_value = "ui_game_symbol_pack_mule";
								}
								else
								{
									_value = "ui_game_symbol_lock";
								}
								return true;
							}
						}
					}
					else if (_bindingName == "userlockedslot")
					{
						_value = this.UserLockedSlot.ToString();
						return true;
					}
				}
				else if (num != 727013168U)
				{
					if (num == 770575247U)
					{
						if (_bindingName == "stacklockcolor")
						{
							if (this.attributeLock && this.itemStack.IsEmpty())
							{
								_value = "200,200,200,64";
							}
							else
							{
								_value = "255,255,255,255";
							}
							return true;
						}
					}
				}
				else if (_bindingName == "backgroundcolor")
				{
					_value = this.backgroundcolorFormatter.Format(this.AttributeLock ? this.attributeLockColor : this.backgroundColor);
					return true;
				}
			}
			else if (num <= 1062608009U)
			{
				if (num != 847165955U)
				{
					if (num == 1062608009U)
					{
						if (_bindingName == "durabilitycolor")
						{
							CachedStringFormatter<Color32> cachedStringFormatter = this.durabilitycolorFormatter;
							ItemStack itemStack = this.itemStack;
							_value = cachedStringFormatter.Format(QualityInfo.GetQualityColor((int)((itemStack != null) ? itemStack.itemValue.Quality : 0)));
							return true;
						}
					}
				}
				else if (_bindingName == "itemtypeicon")
				{
					_value = "";
					if (!this.itemStack.IsEmpty())
					{
						ItemClass itemClassOrMissing = this.itemClassOrMissing;
						if (itemClassOrMissing != null)
						{
							if (itemClassOrMissing.IsBlock() && this.itemStack.itemValue.TextureAllSides == 0)
							{
								_value = Block.list[this.itemStack.itemValue.type].ItemTypeIcon;
							}
							else
							{
								if (itemClassOrMissing.AltItemTypeIcon != null && itemClassOrMissing.Unlocks != "" && XUiM_ItemStack.CheckKnown(base.xui.playerUI.entityPlayer, itemClassOrMissing, this.itemStack.itemValue))
								{
									_value = itemClassOrMissing.AltItemTypeIcon;
									return true;
								}
								_value = itemClassOrMissing.ItemTypeIcon;
							}
						}
					}
					return true;
				}
			}
			else if (num != 1129104269U)
			{
				if (num != 1388578781U)
				{
					if (num == 1768227542U)
					{
						if (_bindingName == "selectionbordercolor")
						{
							_value = this.selectionbordercolorFormatter.Format(this.SelectionBorderColor);
							return true;
						}
					}
				}
				else if (_bindingName == "hasitemtypeicon")
				{
					if (this.itemStack.IsEmpty() || !string.IsNullOrEmpty(this.lockSprite))
					{
						_value = "false";
					}
					else
					{
						ItemClass itemClassOrMissing2 = this.itemClassOrMissing;
						if (itemClassOrMissing2 == null)
						{
							_value = "false";
						}
						else
						{
							_value = (itemClassOrMissing2.IsBlock() ? (Block.list[this.itemStack.itemValue.type].ItemTypeIcon != "").ToString() : (itemClassOrMissing2.ItemTypeIcon != "").ToString());
						}
					}
					return true;
				}
			}
			else if (_bindingName == "showicon")
			{
				_value = (this.ItemIcon != "").ToString();
				return true;
			}
		}
		else if (num <= 2944858628U)
		{
			if (num <= 2412344255U)
			{
				if (num != 1820482109U)
				{
					if (num == 2412344255U)
					{
						if (_bindingName == "isassemblelocked")
						{
							_value = ((this.stackLockType != XUiC_ItemStack.StackLockTypes.None && this.stackLockType != XUiC_ItemStack.StackLockTypes.Hidden) || (this.attributeLock && this.itemStack.IsEmpty())).ToString();
							return true;
						}
					}
				}
				else if (_bindingName == "isfavorite")
				{
					_value = (this.ShowFavorites && this.IsFavorite).ToString();
					return true;
				}
			}
			else if (num != 2705680661U)
			{
				if (num != 2733860383U)
				{
					if (num == 2944858628U)
					{
						if (_bindingName == "hasdurability")
						{
							_value = this.ShowDurability.ToString();
							return true;
						}
					}
				}
				else if (_bindingName == "locktypeicon")
				{
					_value = (this.lockSprite ?? "");
					return true;
				}
			}
			else if (_bindingName == "itemcount")
			{
				_value = "";
				if (!this.itemStack.IsEmpty())
				{
					if (this.ShowDurability)
					{
						_value = ((this.itemStack.itemValue.Quality > 0) ? this.itemcountFormatter.Format((int)this.itemStack.itemValue.Quality) : (this.itemStack.itemValue.IsMod ? "*" : ""));
					}
					else
					{
						_value = ((this.itemClassOrMissing.Stacknumber == 1) ? "" : this.itemcountFormatter.Format(this.itemStack.count));
					}
				}
				return true;
			}
		}
		else if (num <= 3708628627U)
		{
			if (num != 3106195591U)
			{
				if (num == 3708628627U)
				{
					if (_bindingName == "itemicon")
					{
						_value = this.ItemIcon;
						return true;
					}
				}
			}
			else if (_bindingName == "iconcolor")
			{
				_value = this.ItemIconColor;
				return true;
			}
		}
		else if (num != 3741212336U)
		{
			if (num != 4049247086U)
			{
				if (num == 4172540779U)
				{
					if (_bindingName == "durabilityfill")
					{
						ItemStack itemStack2 = this.itemStack;
						_value = ((((itemStack2 != null) ? itemStack2.itemValue : null) == null) ? "0.0" : this.durabilityFillFormatter.Format(this.itemStack.itemValue.PercentUsesLeft));
						return true;
					}
				}
			}
			else if (_bindingName == "itemtypeicontint")
			{
				_value = "255,255,255,255";
				if (!this.itemStack.IsEmpty())
				{
					ItemClass itemClassOrMissing3 = this.itemClassOrMissing;
					if (itemClassOrMissing3 != null && itemClassOrMissing3.Unlocks != "" && XUiM_ItemStack.CheckKnown(base.xui.playerUI.entityPlayer, itemClassOrMissing3, this.itemStack.itemValue))
					{
						_value = this.altitemtypeiconcolorFormatter.Format(itemClassOrMissing3.AltItemTypeIconColor);
					}
				}
				return true;
			}
		}
		else if (_bindingName == "tooltip")
		{
			_value = this.ItemNameText;
			return true;
		}
		return false;
	}

	public override bool ParseAttribute(string _name, string _value, XUiController _parent)
	{
		uint num = <PrivateImplementationDetails>.ComputeStringHash(_name);
		if (num <= 783618599U)
		{
			if (num <= 551907657U)
			{
				if (num != 310027284U)
				{
					if (num != 505492686U)
					{
						if (num == 551907657U)
						{
							if (_name == "background_color")
							{
								this.backgroundColor = StringParsers.ParseColor32(_value);
								return true;
							}
						}
					}
					else if (_name == "show_favorites")
					{
						this.ShowFavorites = StringParsers.ParseBool(_value, 0, -1, true);
						return true;
					}
				}
				else if (_name == "press_color")
				{
					this.pressColor = StringParsers.ParseColor32(_value);
					return true;
				}
			}
			else if (num <= 589168786U)
			{
				if (num != 587922239U)
				{
					if (num == 589168786U)
					{
						if (_name == "final_pressed_color")
						{
							this.finalPressedColor = StringParsers.ParseColor32(_value);
							return true;
						}
					}
				}
				else if (_name == "highlight_color")
				{
					this.highlightColor = StringParsers.ParseColor32(_value);
					return true;
				}
			}
			else if (num != 605893325U)
			{
				if (num == 783618599U)
				{
					if (_name == "hover_icon_grow")
					{
						this.HoverIconGrow = StringParsers.ParseFloat(_value, 0, -1, NumberStyles.Any);
						return true;
					}
				}
			}
			else if (_name == "pickup_sound")
			{
				base.xui.LoadData<AudioClip>(_value, delegate(AudioClip _o)
				{
					this.pickupSound = _o;
				});
				return true;
			}
		}
		else if (num <= 2540527193U)
		{
			if (num <= 933221862U)
			{
				if (num != 785383727U)
				{
					if (num == 933221862U)
					{
						if (_name == "allow_dropping")
						{
							this.AllowDropping = StringParsers.ParseBool(_value, 0, -1, true);
							return true;
						}
					}
				}
				else if (_name == "prefix_id")
				{
					this.PrefixId = StringParsers.ParseBool(_value, 0, -1, true);
					return true;
				}
			}
			else if (num != 1808090607U)
			{
				if (num == 2540527193U)
				{
					if (_name == "select_color")
					{
						this.selectColor = StringParsers.ParseColor32(_value);
						return true;
					}
				}
			}
			else if (_name == "attribute_lock_color")
			{
				this.attributeLockColor = StringParsers.ParseColor32(_value);
				return true;
			}
		}
		else if (num <= 3396886192U)
		{
			if (num != 3096841498U)
			{
				if (num == 3396886192U)
				{
					if (_name == "holding_color")
					{
						this.holdingColor = StringParsers.ParseColor32(_value);
						return true;
					}
				}
			}
			else if (_name == "override_stack_count")
			{
				this.OverrideStackCount = StringParsers.ParseSInt32(_value, 0, -1, NumberStyles.Integer);
				return true;
			}
		}
		else if (num != 3919060864U)
		{
			if (num == 3936377800U)
			{
				if (_name == "pickup_snap_distance")
				{
					this.PickupSnapDistance = int.Parse(_value);
					return true;
				}
			}
		}
		else if (_name == "place_sound")
		{
			base.xui.LoadData<AudioClip>(_value, delegate(AudioClip _o)
			{
				this.placeSound = _o;
			});
			return true;
		}
		return base.ParseAttribute(_name, _value, _parent);
	}

	public static bool IsStackLocationFromPlayer(XUiC_ItemStack.StackLocationTypes? location)
	{
		if (location != null)
		{
			XUiC_ItemStack.StackLocationTypes? stackLocationTypes = location;
			XUiC_ItemStack.StackLocationTypes stackLocationTypes2 = XUiC_ItemStack.StackLocationTypes.Backpack;
			if (!(stackLocationTypes.GetValueOrDefault() == stackLocationTypes2 & stackLocationTypes != null))
			{
				stackLocationTypes = location;
				stackLocationTypes2 = XUiC_ItemStack.StackLocationTypes.ToolBelt;
				if (!(stackLocationTypes.GetValueOrDefault() == stackLocationTypes2 & stackLocationTypes != null))
				{
					stackLocationTypes = location;
					stackLocationTypes2 = XUiC_ItemStack.StackLocationTypes.Equipment;
					return stackLocationTypes.GetValueOrDefault() == stackLocationTypes2 & stackLocationTypes != null;
				}
			}
			return true;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public ItemStack itemStack = ItemStack.Empty.Clone();

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isOver;

	[PublicizedFrom(EAccessModifier.Protected)]
	public AudioClip pickupSound;

	[PublicizedFrom(EAccessModifier.Protected)]
	public AudioClip placeSound;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color32 selectColor = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);

	[PublicizedFrom(EAccessModifier.Private)]
	public Color32 pressColor = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool lastClicked;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ItemStack.LockTypes lockType;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lockSprite;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float lockTime;

	public int TimeInterval = 5;

	public int OverrideStackCount = -1;

	public bool _isQuickSwap;

	[PublicizedFrom(EAccessModifier.Protected)]
	public Color32 selectionBorderColor;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int PickupSnapDistance = 4;

	[PublicizedFrom(EAccessModifier.Protected)]
	public Color32 finalPressedColor = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);

	[PublicizedFrom(EAccessModifier.Protected)]
	public Color32 backgroundColor = new Color32(96, 96, 96, byte.MaxValue);

	[PublicizedFrom(EAccessModifier.Protected)]
	public Color32 highlightColor = new Color32(222, 206, 163, byte.MaxValue);

	[PublicizedFrom(EAccessModifier.Protected)]
	public Color32 holdingColor = new Color32(byte.MaxValue, 128, 0, byte.MaxValue);

	[PublicizedFrom(EAccessModifier.Protected)]
	public Color32 attributeLockColor = new Color32(48, 48, 48, byte.MaxValue);

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiV_Sprite lockTypeIcon;

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiV_Sprite itemIconSprite;

	public XUiV_Label timer;

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiV_Texture backgroundTexture;

	public XUiV_Sprite cancelIcon;

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiV_Sprite swapIcon;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool flashLockTypeIcon;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ItemStack.StackLockTypes stackLockType;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool attributeLock;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isDragAndDrop;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool userLockedSlot;

	[PublicizedFrom(EAccessModifier.Private)]
	public int currentInterval = -1;

	[PublicizedFrom(EAccessModifier.Protected)]
	public TweenScale tweenScale;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 startMousePos = Vector3.zero;

	[PublicizedFrom(EAccessModifier.Protected)]
	public readonly CachedStringFormatterXuiRgbaColor itemiconcolorFormatter = new CachedStringFormatterXuiRgbaColor();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterInt itemcountFormatter = new CachedStringFormatterInt();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterFloat durabilityFillFormatter = new CachedStringFormatterFloat(null);

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterXuiRgbaColor altitemtypeiconcolorFormatter = new CachedStringFormatterXuiRgbaColor();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterXuiRgbaColor durabilitycolorFormatter = new CachedStringFormatterXuiRgbaColor();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterXuiRgbaColor backgroundcolorFormatter = new CachedStringFormatterXuiRgbaColor();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterXuiRgbaColor selectionbordercolorFormatter = new CachedStringFormatterXuiRgbaColor();

	public enum LockTypes
	{
		None,
		Shell,
		Crafting,
		Repairing,
		Scrapping,
		Burning
	}

	public enum StackLockTypes
	{
		None,
		Assemble,
		Quest,
		Tool,
		Hidden
	}

	public enum StackLocationTypes
	{
		Backpack,
		ToolBelt,
		LootContainer,
		Equipment,
		Creative,
		Vehicle,
		Workstation,
		Merge,
		DewCollector
	}
}
