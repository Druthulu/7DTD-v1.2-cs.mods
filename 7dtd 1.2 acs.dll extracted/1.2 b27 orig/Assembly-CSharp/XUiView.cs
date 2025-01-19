using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using Audio;
using InControl;
using Platform;
using UnityEngine;

public class XUiView
{
	public bool RepeatContent { get; set; }

	public virtual int RepeatCount { get; set; }

	public string ID
	{
		get
		{
			return this.id;
		}
	}

	public bool IsNavigatable
	{
		get
		{
			return this._isNavigatable && this.Enabled && this.IsVisible && this.UiTransform.gameObject.activeInHierarchy;
		}
		set
		{
			this._isNavigatable = value;
		}
	}

	public XUiView NavUpTarget
	{
		get
		{
			return this.navUpTarget;
		}
		set
		{
			this.navUpTarget = value;
			foreach (XUiController xuiController in this.Controller.Children)
			{
				xuiController.ViewComponent.NavUpTarget = value;
			}
		}
	}

	public XUiView NavDownTarget
	{
		get
		{
			return this.navDownTarget;
		}
		set
		{
			this.navDownTarget = value;
			foreach (XUiController xuiController in this.Controller.Children)
			{
				xuiController.ViewComponent.navDownTarget = value;
			}
		}
	}

	public XUiView NavLeftTarget
	{
		get
		{
			return this.navLeftTarget;
		}
		set
		{
			this.navLeftTarget = value;
			foreach (XUiController xuiController in this.Controller.Children)
			{
				xuiController.ViewComponent.navLeftTarget = value;
			}
		}
	}

	public XUiView NavRightTarget
	{
		get
		{
			return this.navRightTarget;
		}
		set
		{
			this.navRightTarget = value;
			foreach (XUiController xuiController in this.Controller.Children)
			{
				xuiController.ViewComponent.navRightTarget = value;
			}
		}
	}

	public bool IsDirty
	{
		get
		{
			return this.isDirty;
		}
		set
		{
			this.isDirty = value;
		}
	}

	public XUiController Controller
	{
		get
		{
			return this.controller;
		}
		set
		{
			this.controller = value;
			if (this.controller.ViewComponent != this)
			{
				this.controller.ViewComponent = this;
			}
		}
	}

	public Transform UiTransform
	{
		get
		{
			return this.uiTransform;
		}
	}

	public Vector2i Size
	{
		get
		{
			return this.size;
		}
		set
		{
			if (this.size != value)
			{
				this.size = value;
				this.isDirty = true;
			}
		}
	}

	public Vector2i Position
	{
		get
		{
			return this.position;
		}
		set
		{
			this.position = value;
			this.isDirty = true;
			this.positionDirty = true;
		}
	}

	public float Rotation
	{
		get
		{
			return this.rotation;
		}
		set
		{
			this.rotation = value;
			this.isDirty = true;
		}
	}

	public UIWidget.Pivot Pivot
	{
		get
		{
			return this.pivot;
		}
		set
		{
			this.pivot = value;
			this.isDirty = true;
		}
	}

	public int Depth
	{
		get
		{
			return this.depth;
		}
		set
		{
			this.depth = value;
			this.isDirty = true;
		}
	}

	public virtual bool IsVisible
	{
		get
		{
			return this.isVisible && !this.ForceHide;
		}
		set
		{
			if (this.isVisible != value)
			{
				if (this.ForceHide && value)
				{
					return;
				}
				this.isVisible = value;
				if (this.uiTransform != null && this.isVisible != this.uiTransform.gameObject.activeSelf)
				{
					this.uiTransform.gameObject.SetActive(this.isVisible);
				}
				this.Controller.IsDirty = true;
				this.Controller.OnVisibilityChanged(this.isVisible);
				if (this.xui.playerUI.CursorController.navigationTarget == this)
				{
					this.xui.playerUI.RefreshNavigationTarget();
				}
				this.isDirty = true;
			}
		}
	}

	public bool ForceHide
	{
		get
		{
			return this.forceHide;
		}
		set
		{
			this.forceHide = value;
		}
	}

	public virtual bool Enabled
	{
		get
		{
			return this.enabled;
		}
		set
		{
			if (value != this.enabled)
			{
				this.enabled = value;
				if (!this.enabled && this.xui.playerUI.CursorController.navigationTarget == this)
				{
					this.xui.playerUI.RefreshNavigationTarget();
				}
				this.isDirty = true;
			}
		}
	}

	public string Value
	{
		get
		{
			return this.m_value;
		}
		set
		{
			this.m_value = value;
		}
	}

	public string ToolTip
	{
		get
		{
			return this.toolTip;
		}
		set
		{
			if (this.toolTip != value)
			{
				if (GameManager.Instance.GameIsFocused && this.enabled && this.isOver && this.xui.currentToolTip != null && this.xui.currentToolTip.ToolTip == this.toolTip)
				{
					this.xui.currentToolTip.ToolTip = value;
				}
				this.toolTip = value;
			}
		}
	}

	public string DisabledToolTip
	{
		get
		{
			return this.disabledToolTip;
		}
		set
		{
			if (this.disabledToolTip != value)
			{
				if (GameManager.Instance.GameIsFocused && this.enabled && this.isOver && this.xui.currentToolTip != null && this.xui.currentToolTip.ToolTip == this.disabledToolTip)
				{
					this.xui.currentToolTip.ToolTip = value;
				}
				this.disabledToolTip = value;
			}
		}
	}

	public Vector2 Center
	{
		get
		{
			return this.collider.bounds.center;
		}
	}

	public float heightExtent
	{
		get
		{
			return this.collider.bounds.size.y / 2f;
		}
	}

	public float widthExtent
	{
		get
		{
			return this.collider.bounds.size.x / 2f;
		}
	}

	public Bounds bounds
	{
		get
		{
			return this.collider.bounds;
		}
	}

	public Vector2 ScreenPosition
	{
		get
		{
			return this.xui.playerUI.uiCamera.cachedCamera.WorldToScreenPoint(this.uiTransform.position);
		}
	}

	public bool HasCollider
	{
		get
		{
			return this.collider != null;
		}
	}

	public bool ColliderEnabled
	{
		get
		{
			return this.HasCollider && this.collider.enabled;
		}
	}

	public bool SoundPlayOnClick
	{
		get
		{
			return this.soundPlayOnClick;
		}
		set
		{
			this.soundPlayOnClick = value;
		}
	}

	public bool SoundPlayOnHover
	{
		get
		{
			return this.soundPlayOnHover;
		}
		set
		{
			this.soundPlayOnHover = value;
		}
	}

	public bool HasEvent
	{
		get
		{
			return this.EventOnPress || this.EventOnDoubleClick || this.EventOnHover || this.EventOnHeld || this.EventOnDrag || this.EventOnScroll || this.EventOnSelect || !string.IsNullOrEmpty(this.ToolTip);
		}
	}

	public XUi xui
	{
		get
		{
			return this.mXUi;
		}
		set
		{
			this.mXUi = value;
			if (this.viewIndex < 0)
			{
				this.viewIndex = this.mXUi.RegisterXUiView(this);
			}
		}
	}

	public XUiView(string _id)
	{
		this.id = _id;
	}

	public virtual void UpdateData()
	{
		if (this.positionDirty)
		{
			this.uiTransform.localPosition = new Vector3((float)this.position.x, (float)this.position.y, this.uiTransform.localPosition.z);
			this.positionDirty = false;
		}
		this.parseNavigationTargets();
	}

	public void TryUpdatePosition()
	{
		if (this.positionDirty && this.uiTransform != null)
		{
			this.uiTransform.localPosition = new Vector3((float)this.position.x, (float)this.position.y, this.uiTransform.localPosition.z);
		}
	}

	public virtual void OnOpen()
	{
		if (this.xuiSound != null && this.soundPlayOnOpen)
		{
			Manager.PlayXUiSound(this.xuiSound, this.soundVolume);
		}
		this.isPressed = false;
		this.isHold = false;
	}

	public virtual void OnClose()
	{
		this.isPressed = false;
		this.isHold = false;
		if (!GameManager.Instance.IsQuitting)
		{
			if (this.xui.playerUI.CursorController.navigationTarget == this)
			{
				this.controller.Hovered(false);
				this.xui.playerUI.CursorController.SetNavigationTarget(null);
			}
			if (this.xui.playerUI.CursorController.lockNavigationToView == this)
			{
				this.xui.playerUI.CursorController.SetNavigationLockView(null, null);
			}
		}
	}

	public virtual void Cleanup()
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void CreateComponents(GameObject _go)
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BuildView()
	{
		Type type = base.GetType();
		GameObject original;
		if (XUiView.componentTemplates.TryGetValue(type, out original))
		{
			this.uiTransform = UnityEngine.Object.Instantiate<GameObject>(original).transform;
			return;
		}
		if (XUiView.templatesParent == null)
		{
			Transform transform = this.xui.playerUI.uiCamera.transform;
			XUiView.templatesParent = new GameObject("_ViewTemplates").transform;
			XUiView.templatesParent.parent = transform;
		}
		GameObject gameObject = new GameObject(type.Name);
		gameObject.layer = 12;
		gameObject.transform.parent = XUiView.templatesParent;
		gameObject.AddComponent<BoxCollider>().enabled = false;
		gameObject.AddComponent<UIAnchor>().enabled = false;
		UIEventListener.Get(gameObject);
		this.CreateComponents(gameObject);
		XUiView.componentTemplates[type] = gameObject;
		this.uiTransform = UnityEngine.Object.Instantiate<GameObject>(gameObject).transform;
	}

	public virtual void InitView()
	{
		if (this.uiTransform == null)
		{
			this.BuildView();
		}
		this.uiTransform.name = this.id;
		this.collider = this.uiTransform.gameObject.GetComponent<BoxCollider>();
		this.anchor = this.uiTransform.gameObject.GetComponent<UIAnchor>();
		XUiController parent = this.controller.Parent;
		if (((parent != null) ? parent.ViewComponent : null) != null)
		{
			XUiView viewComponent = this.controller.Parent.ViewComponent;
			this.uiTransform.parent = viewComponent.UiTransform;
			this.uiTransform.localScale = Vector3.one;
			this.uiTransform.localPosition = new Vector3((float)this.position.x, (float)this.position.y, 0f);
			this.uiTransform.localEulerAngles = new Vector3(0f, 0f, this.rotation);
		}
		else
		{
			this.setRootNode();
		}
		if (this.HasEvent)
		{
			this.collider.enabled = true;
			this.RefreshBoxCollider();
		}
		if (this.isAnchored)
		{
			this.anchor.enabled = true;
			if (string.IsNullOrEmpty(this.anchorContainerName))
			{
				this.anchor.container = this.uiTransform.parent.gameObject;
			}
			else if (!this.anchorContainerName.EqualsCaseInsensitive("#none"))
			{
				this.anchor.container = this.Controller.Parent.GetChildById(this.anchorContainerName).ViewComponent.uiTransform.gameObject;
			}
			this.anchor.side = this.anchorSide;
			this.anchor.runOnlyOnce = this.anchorRunOnce;
			this.anchor.pixelOffset = this.anchorOffset;
		}
		if (this.HasEvent)
		{
			UIEventListener uieventListener = UIEventListener.Get(this.uiTransform.gameObject);
			if (this.EventOnPress)
			{
				UIEventListener uieventListener2 = uieventListener;
				uieventListener2.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uieventListener2.onClick, new UIEventListener.VoidDelegate(this.OnClick));
			}
			if (this.EventOnDoubleClick)
			{
				UIEventListener uieventListener3 = uieventListener;
				uieventListener3.onDoubleClick = (UIEventListener.VoidDelegate)Delegate.Combine(uieventListener3.onDoubleClick, new UIEventListener.VoidDelegate(this.OnDoubleClick));
			}
			if (this.EventOnHover || !string.IsNullOrEmpty(this.ToolTip))
			{
				UIEventListener uieventListener4 = uieventListener;
				uieventListener4.onHover = (UIEventListener.BoolDelegate)Delegate.Combine(uieventListener4.onHover, new UIEventListener.BoolDelegate(this.OnHover));
			}
			if (this.EventOnDrag)
			{
				UIEventListener uieventListener5 = uieventListener;
				uieventListener5.onDrag = (UIEventListener.VectorDelegate)Delegate.Combine(uieventListener5.onDrag, new UIEventListener.VectorDelegate(this.OnDrag));
				UIEventListener uieventListener6 = uieventListener;
				uieventListener6.onPress = (UIEventListener.BoolDelegate)Delegate.Combine(uieventListener6.onPress, new UIEventListener.BoolDelegate(this.OnPress));
			}
			if (this.EventOnScroll)
			{
				UIEventListener uieventListener7 = uieventListener;
				uieventListener7.onScroll = (UIEventListener.FloatDelegate)Delegate.Combine(uieventListener7.onScroll, new UIEventListener.FloatDelegate(this.OnScroll));
			}
			if (this.EventOnSelect)
			{
				UIEventListener uieventListener8 = uieventListener;
				uieventListener8.onSelect = (UIEventListener.BoolDelegate)Delegate.Combine(uieventListener8.onSelect, new UIEventListener.BoolDelegate(this.OnSelect));
			}
			if (this.EventOnHeld)
			{
				UIEventListener uieventListener9 = uieventListener;
				uieventListener9.onPress = (UIEventListener.BoolDelegate)Delegate.Combine(uieventListener9.onPress, new UIEventListener.BoolDelegate(this.OnHeldPress));
				UIEventListener uieventListener10 = uieventListener;
				uieventListener10.onDragOut = (UIEventListener.VoidDelegate)Delegate.Combine(uieventListener10.onDragOut, new UIEventListener.VoidDelegate(this.OnHeldDragOut));
			}
		}
		if (this.uiTransform.gameObject.activeSelf != this.isVisible)
		{
			this.uiTransform.gameObject.SetActive(this.isVisible);
		}
		if (!this.gamepadSelectableSetFromAttributes)
		{
			this.IsNavigatable = (this.IsSnappable = this.EventOnPress);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnScroll(GameObject _go, float _delta)
	{
		if (this.EventOnScroll)
		{
			this.controller.Scrolled(_delta);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnSelect(GameObject _go, bool _selected)
	{
		if (this.EventOnSelect && this.enabled)
		{
			this.controller.Selected(_selected);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDrag(GameObject _go, Vector2 _delta)
	{
		if (this.EventOnDrag && this.enabled)
		{
			EDragType dragType = this.wasDragging ? EDragType.Dragging : EDragType.DragStart;
			this.wasDragging = true;
			this.controller.Dragged(_delta, dragType);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnPress(GameObject _go, bool _pressed)
	{
		if (this.EventOnDrag && this.enabled && !_pressed && this.wasDragging)
		{
			this.wasDragging = false;
			this.controller.Dragged(default(Vector2), EDragType.DragEnd);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnHeldPress(GameObject _go, bool _pressed)
	{
		if (this.EventOnHeld)
		{
			if (_pressed && !this.isPressed && this.enabled)
			{
				this.isPressed = true;
				this.isHold = false;
				this.pressStartTime = Time.unscaledTime;
				this.holdStartTime = -1f;
				return;
			}
			if (!_pressed)
			{
				this.isPressed = false;
				bool flag = this.isHold;
				this.isHold = false;
				if (flag)
				{
					this.controller.Held(EHoldType.HoldEnd, Time.unscaledTime - this.holdStartTime, -1f);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnHeldDragOut(GameObject _go)
	{
		if (this.EventOnHeld && this.isPressed)
		{
			this.isPressed = false;
			bool flag = this.isHold;
			this.isHold = false;
			if (flag)
			{
				this.controller.Held(EHoldType.HoldEnd, Time.unscaledTime - this.holdStartTime, -1f);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnClick(GameObject _go)
	{
		if (this.EventOnPress && this.enabled)
		{
			if (this.xuiSound != null && this.soundPlayOnClick && UICamera.currentTouchID == -1)
			{
				Manager.PlayXUiSound(this.xuiSound, this.soundVolume);
			}
			this.controller.Pressed(UICamera.currentTouchID);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDoubleClick(GameObject _go)
	{
		if (this.EventOnDoubleClick && this.enabled)
		{
			if (this.xuiSound != null && this.soundPlayOnClick && UICamera.currentTouchID == -1)
			{
				Manager.PlayXUiSound(this.xuiSound, this.soundVolume);
			}
			this.controller.DoubleClicked(UICamera.currentTouchID);
		}
	}

	public virtual void OnHover(GameObject _go, bool _isOver)
	{
		if (this.xui.playerUI.playerInput.LastDeviceClass == InputDeviceClass.Keyboard && !Cursor.visible)
		{
			_isOver = false;
		}
		bool flag = _isOver && !this.enabled && !string.IsNullOrEmpty(this.DisabledToolTip);
		_isOver &= this.enabled;
		bool flag2 = _isOver && !string.IsNullOrEmpty(this.ToolTip);
		if (this.controllerOnlyTooltip && PlatformManager.NativePlatform.Input.CurrentInputStyle == PlayerInputManager.InputStyle.Keyboard)
		{
			flag = false;
			flag2 = false;
		}
		if (_isOver != this.isOver && _isOver)
		{
			this.PlayHoverSound();
		}
		this.isOver = _isOver;
		if (this.EventOnHover)
		{
			this.controller.Hovered(_isOver);
		}
		if (this.xui.currentToolTip != null)
		{
			if (flag2)
			{
				this.xui.currentToolTip.ToolTip = this.ToolTip;
			}
			else if (flag)
			{
				this.xui.currentToolTip.ToolTip = this.DisabledToolTip;
			}
			else
			{
				this.xui.currentToolTip.ToolTip = "";
			}
		}
		this.xui.playerUI.CursorController.HoverTarget = (_isOver ? this : null);
	}

	public void PlayHoverSound()
	{
		if (PlatformManager.NativePlatform.Input.CurrentInputStyle == PlayerInputManager.InputStyle.Keyboard && this.xuiHoverSound != null && this.soundPlayOnHover && this.enabled && GameManager.Instance.GameIsFocused)
		{
			Manager.PlayXUiSound(this.xuiHoverSound, this.soundVolume);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void setRootNode()
	{
		if (this.rootNode == null)
		{
			this.rootNode = this.xui.transform.Find("CenterTop").transform;
		}
		this.uiTransform.parent = this.rootNode;
		this.uiTransform.gameObject.layer = 12;
		this.uiTransform.localScale = Vector3.one;
		this.uiTransform.localPosition = new Vector3((float)this.position.x, (float)this.position.y, 0f);
		this.uiTransform.localEulerAngles = new Vector3(0f, 0f, this.rotation);
	}

	public virtual void RefreshBoxCollider()
	{
		if (this.collider != null)
		{
			float num = (float)this.size.x * 0.5f;
			float num2 = (float)this.size.y * 0.5f;
			float x;
			float y;
			switch (this.pivot)
			{
			case UIWidget.Pivot.TopLeft:
				x = num;
				y = 0f - num2;
				break;
			case UIWidget.Pivot.Top:
				x = 0f;
				y = 0f - num2;
				break;
			case UIWidget.Pivot.TopRight:
				x = 0f - num;
				y = 0f - num2;
				break;
			case UIWidget.Pivot.Left:
				x = num;
				y = 0f;
				break;
			case UIWidget.Pivot.Center:
				x = 0f;
				y = 0f;
				break;
			case UIWidget.Pivot.Right:
				x = 0f - num;
				y = 0f;
				break;
			case UIWidget.Pivot.BottomLeft:
				x = num;
				y = num2;
				break;
			case UIWidget.Pivot.Bottom:
				x = 0f;
				y = num2;
				break;
			case UIWidget.Pivot.BottomRight:
				x = 0f - num;
				y = num2;
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			this.collider.center = new Vector3(x, y, 0f);
			this.collider.size = new Vector3((float)this.size.x * this.colliderScale, (float)this.size.y * this.colliderScale, 0f);
		}
	}

	public virtual void Update(float _dt)
	{
		if (this.isOver && UICamera.hoveredObject != this.UiTransform.gameObject)
		{
			this.OnHover(this.UiTransform.gameObject, false);
		}
		if (this.isPressed && this.enabled)
		{
			float unscaledTime = Time.unscaledTime;
			if (!this.isHold)
			{
				this.isHold = (unscaledTime - this.pressStartTime >= this.holdDelay);
				if (this.isHold)
				{
					this.holdStartTime = unscaledTime;
					this.controller.Held(EHoldType.HoldStart, 0f, -1f);
					this.holdEventNextTime = 0f;
					this.holdEventLastTime = unscaledTime;
					this.holdEventIntervalChangeSpeed = 0f;
					this.holdEventIntervalCurrent = this.holdEventIntervalInitial;
				}
			}
			else
			{
				this.controller.Held(EHoldType.Hold, unscaledTime - this.holdStartTime, -1f);
			}
			if (this.isHold && unscaledTime >= this.holdEventNextTime)
			{
				this.holdEventIntervalCurrent = Mathf.SmoothDamp(this.holdEventIntervalCurrent, this.holdEventIntervalFinal, ref this.holdEventIntervalChangeSpeed, this.holdEventIntervalAcceleration, float.PositiveInfinity, _dt);
				this.holdEventNextTime = unscaledTime + this.holdEventIntervalCurrent;
				this.controller.Held(EHoldType.HoldTimed, unscaledTime - this.holdStartTime, unscaledTime - this.holdEventLastTime);
				this.holdEventLastTime = unscaledTime;
			}
		}
		if (this.isDirty)
		{
			this.UpdateData();
			this.isDirty = false;
		}
	}

	public Vector2 GetClosestPoint(Vector3 point)
	{
		if (this.collider != null)
		{
			return this.collider.ClosestPointOnBounds(point);
		}
		Log.Warning("XUiView: Attempting to get closest point to a view without a box collider");
		return this.uiTransform.position;
	}

	public void ClearNavigationTargets()
	{
		this.NavUpTarget = (this.NavDownTarget = (this.NavLeftTarget = (this.NavRightTarget = null)));
	}

	public virtual void SetDefaults(XUiController _parent)
	{
		this.Pivot = UIWidget.Pivot.TopLeft;
		this.RepeatContent = false;
		this.RepeatCount = 1;
		Vector2i? vector2i;
		if (_parent == null)
		{
			vector2i = null;
		}
		else
		{
			XUiView viewComponent = _parent.ViewComponent;
			vector2i = ((viewComponent != null) ? new Vector2i?(viewComponent.Size) : null);
		}
		this.Size = (vector2i ?? new Vector2i(0, 0));
		this.Position = Vector2i.zero;
		this.IsVisible = true;
		int? num;
		if (_parent == null)
		{
			num = null;
		}
		else
		{
			XUiView viewComponent2 = _parent.ViewComponent;
			num = ((viewComponent2 != null) ? new int?(viewComponent2.Depth) : null);
		}
		int? num2 = num;
		this.Depth = num2.GetValueOrDefault();
		this.ToolTip = "";
		this.soundLoop = false;
		this.soundPlayOnClick = true;
		this.soundPlayOnOpen = false;
		this.soundPlayOnHover = true;
		this.soundVolume = 1f;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void parseNavigationTargets()
	{
		if (this.navUpSetFromXML)
		{
			this.NavUpTarget = this.<parseNavigationTargets>g__findView|202_0(this.navUpTargetString);
		}
		if (this.navDownSetFromXML)
		{
			this.NavDownTarget = this.<parseNavigationTargets>g__findView|202_0(this.navDownTargetString);
		}
		if (this.navLeftSetFromXML)
		{
			this.NavLeftTarget = this.<parseNavigationTargets>g__findView|202_0(this.navLeftTargetString);
		}
		if (this.navRightSetFromXML)
		{
			this.NavRightTarget = this.<parseNavigationTargets>g__findView|202_0(this.navRightTargetString);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void parseAnchors(UIWidget _target, bool _fixSize = true)
	{
		if (_target == null)
		{
			return;
		}
		if (this.parseAnchorString(this.anchorLeft, ref this.anchorLeftParsed, _target.leftAnchor, _target) | this.parseAnchorString(this.anchorRight, ref this.anchorRightParsed, _target.rightAnchor, _target) | this.parseAnchorString(this.anchorBottom, ref this.anchorBottomParsed, _target.bottomAnchor, _target) | this.parseAnchorString(this.anchorTop, ref this.anchorTopParsed, _target.topAnchor, _target))
		{
			this.isDirty = true;
			_target.ResetAnchors();
		}
		if (_fixSize)
		{
			if ((!_target.leftAnchor.target || !_target.rightAnchor.target) && _target.width != this.size.x)
			{
				_target.width = this.size.x;
			}
			if ((!_target.bottomAnchor.target || !_target.topAnchor.target) && _target.height != this.size.y)
			{
				_target.height = this.size.y;
			}
		}
		ThreadManager.StartCoroutine(XUiView.<parseAnchors>g__markAsChangedLater|203_0(_target));
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool parseAnchorString(string _anchorString, ref string _parsedString, UIRect.AnchorPoint _anchor, UIWidget _target)
	{
		if (_anchorString == null)
		{
			return false;
		}
		if (_anchorString == _parsedString && _anchor.target != null)
		{
			return false;
		}
		_parsedString = _anchorString;
		int num = _anchorString.IndexOf(',');
		if (num < 0)
		{
			throw new ArgumentException("Invalid anchor string '" + _anchorString + "', expected '<target>,<relative>,<absolute>'");
		}
		string text = _anchorString.Substring(0, num);
		int num2 = _anchorString.IndexOf(',', num + 1);
		if (num2 < 0)
		{
			throw new ArgumentException("Invalid anchor string '" + _anchorString + "', expected '<target>,<relative>,<absolute>'");
		}
		float relative = StringParsers.ParseFloat(_anchorString, num + 1, num2 - 1, NumberStyles.Any);
		int absolute = StringParsers.ParseSInt32(_anchorString, num2 + 1, -1, NumberStyles.Integer);
		if (text.Length == 0)
		{
			throw new ArgumentException("Invalid anchor string '" + _anchorString + "', expected '<target>,<relative>,<absolute>'");
		}
		if (text.EqualsCaseInsensitive("#parent"))
		{
			_anchor.target = this.uiTransform.parent;
		}
		else if (text.EqualsCaseInsensitive("#cam"))
		{
			UICamera componentInParent = this.uiTransform.gameObject.GetComponentInParent<UICamera>();
			if (componentInParent == null)
			{
				throw new Exception("UICamera not found");
			}
			_anchor.target = componentInParent.transform;
		}
		else if (text[0] == '#')
		{
			string text2 = text.Substring(1);
			UIAnchor.Side side;
			if (!EnumUtils.TryParse<UIAnchor.Side>(text2, out side, true))
			{
				throw new ArgumentException("Invalid anchor side name '" + text2 + "', expected any of '\tBottomLeft,Left,TopLeft,Top,TopRight,Right,BottomRight,Bottom,Center'");
			}
			_anchor.target = this.xui.GetAnchor(side).transform;
		}
		else
		{
			XUiView xuiView = this.findHierarchyClosestView(text);
			if (xuiView == null)
			{
				throw new ArgumentException(string.Concat(new string[]
				{
					"Invalid anchor string '",
					_anchorString,
					"', view component with name '",
					text,
					"' not found.\nOn: ",
					this.controller.GetXuiHierarchy()
				}));
			}
			_anchor.target = xuiView.UiTransform;
			XUiV_Grid xuiV_Grid = xuiView as XUiV_Grid;
			if (xuiV_Grid != null)
			{
				xuiV_Grid.OnSizeChangedSimple += _target.UpdateAnchors;
			}
		}
		_anchor.relative = relative;
		_anchor.absolute = absolute;
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiView findHierarchyClosestView(string _name)
	{
		XUiController xuiController = this.Controller.WindowGroup.Controller;
		XUiController parent = this.Controller.Parent;
		XUiController childById;
		for (;;)
		{
			childById = parent.GetChildById(_name);
			if (childById != null)
			{
				break;
			}
			parent = parent.Parent;
			if (parent == xuiController)
			{
				goto Block_2;
			}
		}
		return childById.ViewComponent;
		Block_2:
		return null;
	}

	public bool ParseAttributeViewAndController(string _attribute, string _value, XUiController _parent, bool _allowBindingCreation = true)
	{
		if (_value.Contains("{"))
		{
			if (_allowBindingCreation)
			{
				new BindingInfo(this, _attribute, _value);
				return true;
			}
			if (XUiFromXml.DebugXuiLoading == XUiFromXml.DebugLevel.Verbose)
			{
				Log.Warning(string.Concat(new string[]
				{
					"[XUi] Refreshed binding contained '{': ",
					_attribute,
					"='",
					_value,
					"' on ",
					this.id
				}));
			}
		}
		if (this.ParseAttribute(_attribute, _value, _parent))
		{
			return true;
		}
		if (this.Controller != null)
		{
			if (!this.Controller.ParseAttribute(_attribute, _value, _parent))
			{
				this.Controller.CustomAttributes[_attribute] = _value;
			}
			return true;
		}
		return false;
	}

	public virtual bool ParseAttribute(string _attribute, string _value, XUiController _parent)
	{
		uint num = <PrivateImplementationDetails>.ComputeStringHash(_attribute);
		if (num <= 2471448074U)
		{
			if (num > 1113510858U)
			{
				if (num <= 1936866307U)
				{
					if (num <= 1322393624U)
					{
						if (num != 1214160313U)
						{
							if (num != 1259646524U)
							{
								if (num != 1322393624U)
								{
									return false;
								}
								if (!(_attribute == "on_doubleclick"))
								{
									return false;
								}
								this.EventOnDoubleClick = StringParsers.ParseBool(_value, 0, -1, true);
								return true;
							}
							else
							{
								if (!(_attribute == "on_press"))
								{
									return false;
								}
								this.EventOnPress = StringParsers.ParseBool(_value, 0, -1, true);
								return true;
							}
						}
						else
						{
							if (!(_attribute == "aspect_ratio"))
							{
								return false;
							}
							this.aspectRatio = StringParsers.ParseFloat(_value, 0, -1, NumberStyles.Any);
							return true;
						}
					}
					else if (num <= 1425709473U)
					{
						if (num != 1412654217U)
						{
							if (num != 1425709473U)
							{
								return false;
							}
							if (!(_attribute == "visible"))
							{
								return false;
							}
							this.IsVisible = StringParsers.ParseBool(_value, 0, -1, true);
							return true;
						}
						else if (!(_attribute == "pos"))
						{
							return false;
						}
					}
					else if (num != 1577512446U)
					{
						if (num != 1936866307U)
						{
							return false;
						}
						if (!(_attribute == "nav_down"))
						{
							return false;
						}
						this.navDownTargetString = _value;
						this.navDownSetFromXML = true;
						this.isDirty = true;
						return true;
					}
					else
					{
						if (!(_attribute == "anchor_bottom"))
						{
							return false;
						}
						this.anchorBottom = _value;
						this.isDirty = true;
						return true;
					}
				}
				else if (num <= 2237619868U)
				{
					if (num != 2010377433U)
					{
						if (num != 2180766004U)
						{
							if (num != 2237619868U)
							{
								return false;
							}
							if (!(_attribute == "sound_play_on_hover"))
							{
								return false;
							}
							this.xui.LoadData<AudioClip>(_value, delegate(AudioClip _o)
							{
								this.xuiHoverSound = _o;
							});
							return true;
						}
						else
						{
							if (!(_attribute == "tooltip_key"))
							{
								return false;
							}
							this.ToolTip = Localization.Get(_value, false);
							return true;
						}
					}
					else
					{
						if (!(_attribute == "hold_timed_step_divider"))
						{
							return false;
						}
						return true;
					}
				}
				else if (num <= 2369371622U)
				{
					if (num != 2291184263U)
					{
						if (num != 2369371622U)
						{
							return false;
						}
						if (!(_attribute == "name"))
						{
							return false;
						}
						this.id = _value;
						return true;
					}
					else
					{
						if (!(_attribute == "anchor_parent_id"))
						{
							return false;
						}
						this.anchorContainerName = _value;
						return true;
					}
				}
				else if (num != 2427138910U)
				{
					if (num != 2471448074U)
					{
						return false;
					}
					if (!(_attribute == "position"))
					{
						return false;
					}
				}
				else
				{
					if (!(_attribute == "nav_left"))
					{
						return false;
					}
					this.navLeftTargetString = _value;
					this.navLeftSetFromXML = true;
					this.isDirty = true;
					return true;
				}
				this.Position = StringParsers.ParseVector2i(_value, ',');
				return true;
			}
			if (num <= 449234616U)
			{
				if (num <= 273325558U)
				{
					if (num != 49525662U)
					{
						if (num != 235771284U)
						{
							if (num == 273325558U)
							{
								if (_attribute == "anchor_run_once")
								{
									this.anchorRunOnce = StringParsers.ParseBool(_value, 0, -1, true);
									return true;
								}
							}
						}
						else if (_attribute == "sound")
						{
							this.xui.LoadData<AudioClip>(_value, delegate(AudioClip _o)
							{
								this.xuiSound = _o;
							});
							return true;
						}
					}
					else if (_attribute == "enabled")
					{
						this.Enabled = StringParsers.ParseBool(_value, 0, -1, true);
						return true;
					}
				}
				else if (num != 324448160U)
				{
					if (num != 353658589U)
					{
						if (num == 449234616U)
						{
							if (_attribute == "repeat_content")
							{
								this.RepeatContent = StringParsers.ParseBool(_value, 0, -1, true);
								return true;
							}
						}
					}
					else if (_attribute == "gamepad_selectable")
					{
						this.IsNavigatable = StringParsers.ParseBool(_value, 0, -1, true);
						this.gamepadSelectableSetFromAttributes = true;
						return true;
					}
				}
				else if (_attribute == "anchor_top")
				{
					this.anchorTop = _value;
					this.isDirty = true;
					return true;
				}
			}
			else if (num <= 568213883U)
			{
				if (num != 450319007U)
				{
					if (num != 564937055U)
					{
						if (num == 568213883U)
						{
							if (_attribute == "sound_volume")
							{
								this.soundVolume = StringParsers.ParseFloat(_value, 0, -1, NumberStyles.Any);
								return true;
							}
						}
					}
					else if (_attribute == "rotation")
					{
						this.Rotation = (float)int.Parse(_value);
						return true;
					}
				}
				else if (_attribute == "use_selection_box")
				{
					this.UseSelectionBox = StringParsers.ParseBool(_value, 0, -1, true);
					return true;
				}
			}
			else if (num <= 847898140U)
			{
				if (num != 597743964U)
				{
					if (num == 847898140U)
					{
						if (_attribute == "anchor_left")
						{
							this.anchorLeft = _value;
							this.isDirty = true;
							return true;
						}
					}
				}
				else if (_attribute == "size")
				{
					this.Size = StringParsers.ParseVector2i(_value, ',');
					return true;
				}
			}
			else if (num != 920647948U)
			{
				if (num == 1113510858U)
				{
					if (_attribute == "value")
					{
						this.Value = _value;
						return true;
					}
				}
			}
			else if (_attribute == "hold_timed_step_acceleration")
			{
				this.holdEventIntervalAcceleration = StringParsers.ParseFloat(_value, 0, -1, NumberStyles.Any);
				return true;
			}
		}
		else if (num <= 3431509877U)
		{
			if (num <= 2796611931U)
			{
				if (num <= 2641117421U)
				{
					if (num != 2508680735U)
					{
						if (num != 2608083357U)
						{
							if (num == 2641117421U)
							{
								if (_attribute == "hold_timed_initial_interval")
								{
									this.holdEventIntervalInitial = StringParsers.ParseFloat(_value, 0, -1, NumberStyles.Any);
									return true;
								}
							}
						}
						else if (_attribute == "snap")
						{
							this.IsSnappable = StringParsers.ParseBool(_value, 0, -1, true);
							return true;
						}
					}
					else if (_attribute == "width")
					{
						int y = this.Size.y;
						int num2;
						if (_value.Contains("%"))
						{
							_value = _value.Replace("%", "");
							if (int.TryParse(_value, out num2))
							{
								num2 = (int)((float)num2 / 100f) * _parent.ViewComponent.Size.x;
							}
						}
						else
						{
							int.TryParse(_value, out num2);
						}
						this.Size = new Vector2i(num2, y);
						return true;
					}
				}
				else if (num != 2664078777U)
				{
					if (num != 2667237188U)
					{
						if (num == 2796611931U)
						{
							if (_attribute == "on_drag")
							{
								this.EventOnDrag = StringParsers.ParseBool(_value, 0, -1, true);
								return true;
							}
						}
					}
					else if (_attribute == "collider_scale")
					{
						this.colliderScale = StringParsers.ParseFloat(_value, 0, -1, NumberStyles.Any);
						return true;
					}
				}
				else if (_attribute == "hold_timed_final_interval")
				{
					this.holdEventIntervalFinal = StringParsers.ParseFloat(_value, 0, -1, NumberStyles.Any);
					return true;
				}
			}
			else if (num <= 2857717125U)
			{
				if (num != 2830671030U)
				{
					if (num != 2854231290U)
					{
						if (num == 2857717125U)
						{
							if (_attribute == "disabled_tooltip_key")
							{
								this.DisabledToolTip = Localization.Get(_value, false);
								return true;
							}
						}
					}
					else if (_attribute == "anchor_side")
					{
						this.isAnchored = true;
						this.anchorSide = EnumUtils.Parse<UIAnchor.Side>(_value, true);
						return true;
					}
				}
				else if (_attribute == "on_held")
				{
					this.EventOnHeld = StringParsers.ParseBool(_value, 0, -1, true);
					return true;
				}
			}
			else if (num <= 3135069273U)
			{
				if (num != 2966252344U)
				{
					if (num == 3135069273U)
					{
						if (_attribute == "force_hide")
						{
							this.ForceHide = StringParsers.ParseBool(_value, 0, -1, true);
							return true;
						}
					}
				}
				else if (_attribute == "hold_delay")
				{
					this.holdDelay = StringParsers.ParseFloat(_value, 0, -1, NumberStyles.Any);
					return true;
				}
			}
			else if (num != 3136805134U)
			{
				if (num == 3431509877U)
				{
					if (_attribute == "on_hover")
					{
						this.EventOnHover = StringParsers.ParseBool(_value, 0, -1, true);
						return true;
					}
				}
			}
			else if (_attribute == "anchor_offset")
			{
				this.anchorOffset = StringParsers.ParseVector2(_value);
				return true;
			}
		}
		else if (num <= 3741212336U)
		{
			if (num <= 3545960405U)
			{
				if (num != 3460649205U)
				{
					if (num != 3529586537U)
					{
						if (num == 3545960405U)
						{
							if (_attribute == "on_select")
							{
								this.EventOnSelect = StringParsers.ParseBool(_value, 0, -1, true);
								return true;
							}
						}
					}
					else if (_attribute == "anchor_right")
					{
						this.anchorRight = _value;
						this.isDirty = true;
						return true;
					}
				}
				else if (_attribute == "disabled_tooltip")
				{
					this.DisabledToolTip = _value;
					return true;
				}
			}
			else if (num <= 3623547202U)
			{
				if (num != 3585981250U)
				{
					if (num == 3623547202U)
					{
						if (_attribute == "nav_up")
						{
							this.navUpTargetString = _value;
							this.navUpSetFromXML = true;
							this.isDirty = true;
							return true;
						}
					}
				}
				else if (_attribute == "height")
				{
					int x = this.Size.x;
					int num3;
					if (_value.Contains("%"))
					{
						_value = _value.Replace("%", "");
						if (int.TryParse(_value, out num3))
						{
							num3 = (int)((float)num3 / 100f) * _parent.ViewComponent.Size.y;
						}
					}
					else
					{
						int.TryParse(_value, out num3);
					}
					this.Size = new Vector2i(x, num3);
					return true;
				}
			}
			else if (num != 3705907993U)
			{
				if (num == 3741212336U)
				{
					if (_attribute == "tooltip")
					{
						this.ToolTip = _value;
						return true;
					}
				}
			}
			else if (_attribute == "sound_play_on_press")
			{
				this.soundPlayOnClick = StringParsers.ParseBool(_value, 0, -1, true);
				return true;
			}
		}
		else if (num <= 4041470899U)
		{
			if (num != 3950266292U)
			{
				if (num != 3983471730U)
				{
					if (num == 4041470899U)
					{
						if (_attribute == "nav_right")
						{
							this.navRightTargetString = _value;
							this.navRightSetFromXML = true;
							this.isDirty = true;
							return true;
						}
					}
				}
				else if (_attribute == "sound_play_on_open")
				{
					this.soundPlayOnOpen = StringParsers.ParseBool(_value, 0, -1, true);
					return true;
				}
			}
			else if (_attribute == "on_scroll")
			{
				this.EventOnScroll = StringParsers.ParseBool(_value, 0, -1, true);
				return true;
			}
		}
		else if (num <= 4226831639U)
		{
			if (num != 4121269289U)
			{
				if (num == 4226831639U)
				{
					if (_attribute == "pivot")
					{
						this.Pivot = EnumUtils.Parse<UIWidget.Pivot>(_value, true);
						return true;
					}
				}
			}
			else if (_attribute == "keep_aspect_ratio")
			{
				this.keepAspectRatio = EnumUtils.Parse<UIWidget.AspectRatioSource>(_value, false);
				return true;
			}
		}
		else if (num != 4265554070U)
		{
			if (num == 4269121258U)
			{
				if (_attribute == "depth")
				{
					int num4;
					int.TryParse(_value, out num4);
					int num5 = num4;
					XUiView viewComponent = _parent.ViewComponent;
					this.Depth = num5 + ((viewComponent != null) ? viewComponent.Depth : 0);
					return true;
				}
			}
		}
		else if (_attribute == "repeat_count")
		{
			this.RepeatCount = StringParsers.ParseSInt32(_value, 0, -1, NumberStyles.Integer);
			return true;
		}
		return false;
	}

	public virtual void setRepeatContentTemplateParams(Dictionary<string, object> _templateParams, int _curRepeatNum)
	{
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Private)]
	public XUiView <parseNavigationTargets>g__findView|202_0(string _name)
	{
		if (string.IsNullOrEmpty(_name))
		{
			return null;
		}
		XUiView xuiView = this.findHierarchyClosestView(_name);
		if (xuiView == null)
		{
			throw new ArgumentException("Invalid navigation target, view component with name '" + _name + "' not found.\nOn: " + this.controller.GetXuiHierarchy());
		}
		return xuiView;
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Internal)]
	public static IEnumerator <parseAnchors>g__markAsChangedLater|203_0(UIWidget _target)
	{
		yield return new WaitForEndOfFrame();
		_target.MarkAsChanged();
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Dictionary<Type, GameObject> componentTemplates = new Dictionary<Type, GameObject>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static Transform templatesParent;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string id;

	[PublicizedFrom(EAccessModifier.Protected)]
	public Transform uiTransform;

	[PublicizedFrom(EAccessModifier.Protected)]
	public BoxCollider collider;

	[PublicizedFrom(EAccessModifier.Protected)]
	public UIAnchor anchor;

	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector2i size;

	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector2i position;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool positionDirty;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float rotation;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool isVisible;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool forceHide;

	[PublicizedFrom(EAccessModifier.Protected)]
	public UIWidget.Pivot pivot;

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUi.Alignment alignment;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int depth;

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiController controller;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string m_value;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool initialized;

	[PublicizedFrom(EAccessModifier.Protected)]
	public UIWidget.AspectRatioSource keepAspectRatio;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float aspectRatio;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string anchorLeft;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string anchorRight;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string anchorBottom;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string anchorTop;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string anchorLeftParsed;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string anchorRightParsed;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string anchorBottomParsed;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string anchorTopParsed;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool isAnchored;

	[PublicizedFrom(EAccessModifier.Protected)]
	public UIAnchor.Side anchorSide = UIAnchor.Side.Center;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool anchorRunOnce = true;

	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector2 anchorOffset = Vector2.zero;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string anchorContainerName;

	[PublicizedFrom(EAccessModifier.Protected)]
	public Transform rootNode;

	[PublicizedFrom(EAccessModifier.Private)]
	public AudioClip xuiSound;

	[PublicizedFrom(EAccessModifier.Private)]
	public AudioClip xuiHoverSound;

	[PublicizedFrom(EAccessModifier.Private)]
	public string toolTip;

	[PublicizedFrom(EAccessModifier.Private)]
	public string disabledToolTip;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool soundPlayOnClick;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool soundPlayOnHover;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool soundPlayOnOpen;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool soundLoop;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float soundVolume;

	[PublicizedFrom(EAccessModifier.Private)]
	public float holdDelay = 0.5f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float holdEventIntervalInitial = 0.5f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float holdEventIntervalFinal = 0.06f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float holdEventIntervalAcceleration = 0.015f;

	public bool EventOnHover;

	public bool EventOnPress;

	public bool EventOnDoubleClick;

	public bool EventOnHeld;

	public bool EventOnScroll;

	public bool EventOnDrag;

	public bool EventOnSelect;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool enabled = true;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool isDirty;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float colliderScale = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool _isNavigatable = true;

	public bool IsSnappable = true;

	public bool UseSelectionBox = true;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool gamepadSelectableSetFromAttributes;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool navLeftSetFromXML;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool navRightSetFromXML;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool navUpSetFromXML;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool navDownSetFromXML;

	[PublicizedFrom(EAccessModifier.Private)]
	public string navUpTargetString;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiView navUpTarget;

	[PublicizedFrom(EAccessModifier.Private)]
	public string navDownTargetString;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiView navDownTarget;

	[PublicizedFrom(EAccessModifier.Private)]
	public string navLeftTargetString;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiView navLeftTarget;

	[PublicizedFrom(EAccessModifier.Private)]
	public string navRightTargetString;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiView navRightTarget;

	public bool controllerOnlyTooltip;

	[PublicizedFrom(EAccessModifier.Private)]
	public int viewIndex = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUi mXUi;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool wasDragging;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isPressed;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isHold;

	[PublicizedFrom(EAccessModifier.Private)]
	public float pressStartTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public float holdStartTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public float holdEventIntervalCurrent;

	[PublicizedFrom(EAccessModifier.Private)]
	public float holdEventIntervalChangeSpeed;

	[PublicizedFrom(EAccessModifier.Private)]
	public float holdEventNextTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public float holdEventLastTime;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool isOver;
}
