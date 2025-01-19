using System;
using System.Collections.Generic;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiController
{
	public PlayerInputManager.InputStyle CurrentInputStyle
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return this.lastInputStyle;
		}
	}

	public event XUiEvent_OnPressEventHandler OnPress;

	public event XUiEvent_OnPressEventHandler OnDoubleClick;

	public event XUiEvent_OnPressEventHandler OnRightPress;

	public event XUiEvent_OnHoverEventHandler OnHover;

	public event XUiEvent_OnDragEventHandler OnDrag;

	public event XUiEvent_OnHeldHandler OnHold;

	public event XUiEvent_OnScrollEventHandler OnScroll;

	public event XUiEvent_OnSelectEventHandler OnSelect;

	public event XUiEvent_OnVisibilityChanged OnVisiblity;

	public XUiView ViewComponent
	{
		get
		{
			return this.viewComponent;
		}
		set
		{
			this.viewComponent = value;
		}
	}

	public XUiController Parent
	{
		get
		{
			return this.parent;
		}
		set
		{
			this.parent = value;
		}
	}

	public List<XUiController> Children
	{
		get
		{
			return this.children;
		}
	}

	public XUiWindowGroup WindowGroup
	{
		get
		{
			return this.windowGroup;
		}
		set
		{
			this.windowGroup = value;
		}
	}

	public XUi xui { get; set; }

	public bool IsOpen { get; [PublicizedFrom(EAccessModifier.Protected)] set; }

	public XUiController()
	{
		this.parent = null;
	}

	public XUiController(XUiController _parent)
	{
		this.parent = _parent;
		XUiController xuiController = this.parent;
		if (xuiController == null)
		{
			return;
		}
		xuiController.AddChild(this);
	}

	public virtual void Init()
	{
		if (this.viewComponent != null)
		{
			this.viewComponent.InitView();
		}
		for (int i = 0; i < this.children.Count; i++)
		{
			this.children[i].Init();
		}
		this.curInputStyle = PlatformManager.NativePlatform.Input.CurrentInputStyle;
	}

	public virtual void UpdateInput()
	{
		for (int i = 0; i < this.children.Count; i++)
		{
			if (!this.children[i].IsDormant)
			{
				this.children[i].UpdateInput();
			}
		}
	}

	public virtual void Update(float _dt)
	{
		if (this.viewComponent != null && this.windowGroup != null && this.windowGroup.isShowing && this.viewComponent.IsVisible)
		{
			this.viewComponent.Update(_dt);
		}
		if (this.curInputStyle != this.lastInputStyle)
		{
			PlayerInputManager.InputStyle oldStyle = this.lastInputStyle;
			this.lastInputStyle = this.curInputStyle;
			this.RefreshBindings(false);
			this.InputStyleChanged(oldStyle, this.lastInputStyle);
		}
		for (int i = 0; i < this.children.Count; i++)
		{
			XUiController xuiController = this.children[i];
			if (!xuiController.IsDormant)
			{
				xuiController.Update(_dt);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void InputStyleChanged(PlayerInputManager.InputStyle _oldStyle, PlayerInputManager.InputStyle _newStyle)
	{
	}

	public void ForceInputStyleChange(PlayerInputManager.InputStyle _oldStyle, PlayerInputManager.InputStyle _newStyle)
	{
		if (this.registeredForInputStyleChanges)
		{
			this.InputStyleChanged(_oldStyle, _newStyle);
		}
		foreach (XUiController xuiController in this.children)
		{
			this.ForceInputStyleChange(_oldStyle, _newStyle);
		}
	}

	public XUiController GetChildById(string _id)
	{
		XUiController xuiController = null;
		if (this.viewComponent != null && string.Equals(this.viewComponent.ID, _id, StringComparison.OrdinalIgnoreCase))
		{
			xuiController = this;
		}
		else
		{
			for (int i = 0; i < this.children.Count; i++)
			{
				xuiController = this.children[i].GetChildById(_id);
				if (xuiController != null)
				{
					break;
				}
			}
		}
		return xuiController;
	}

	public XUiController[] GetChildrenById(string _id, List<XUiController> _list = null)
	{
		List<XUiController> list;
		if (_list == null)
		{
			list = new List<XUiController>();
		}
		else
		{
			list = _list;
		}
		if (this.viewComponent != null && string.Equals(this.viewComponent.ID, _id, StringComparison.OrdinalIgnoreCase))
		{
			list.Add(this);
		}
		else
		{
			for (int i = 0; i < this.children.Count; i++)
			{
				this.children[i].GetChildrenById(_id, list);
			}
		}
		if (_list == null)
		{
			return list.ToArray();
		}
		return null;
	}

	public T GetChildByType<T>() where T : XUiController
	{
		T t = this as T;
		if (t == null)
		{
			foreach (XUiController xuiController in this.children)
			{
				t = xuiController.GetChildByType<T>();
				if (t != null)
				{
					break;
				}
			}
		}
		return t;
	}

	public T[] GetChildrenByType<T>(List<T> _list = null) where T : XUiController
	{
		List<T> list;
		if (_list == null)
		{
			list = new List<T>();
		}
		else
		{
			list = _list;
		}
		T t = this as T;
		if (t != null)
		{
			list.Add(t);
		}
		else
		{
			foreach (XUiController xuiController in this.children)
			{
				xuiController.GetChildrenByType<T>(list);
			}
		}
		if (_list == null)
		{
			return list.ToArray();
		}
		return null;
	}

	public T GetParentByType<T>() where T : XUiController
	{
		if (this is T)
		{
			return this as T;
		}
		if (this.Parent != null)
		{
			return this.Parent.GetParentByType<T>();
		}
		return default(T);
	}

	public bool IsChildOf(XUiController _controller)
	{
		return this.Parent != null && (this.Parent == _controller || this.Parent.IsChildOf(_controller));
	}

	public XUiV_Window GetParentWindow()
	{
		XUiV_Window xuiV_Window = this.ViewComponent as XUiV_Window;
		if (xuiV_Window != null)
		{
			return xuiV_Window;
		}
		XUiController xuiController = this.Parent;
		if (xuiController == null)
		{
			return null;
		}
		return xuiController.GetParentWindow();
	}

	public void AddChild(XUiController _child)
	{
		this.children.Add(_child);
	}

	public void Pressed(int _mouseButton)
	{
		this.OnPressed(_mouseButton);
	}

	public void DoubleClicked(int _mouseButton)
	{
		this.OnDoubleClicked(_mouseButton);
	}

	public void Hovered(bool _isOver)
	{
		this.OnHovered(_isOver);
	}

	public void Scrolled(float _delta)
	{
		this.OnScrolled(_delta);
	}

	public void Selected(bool _selected)
	{
		this.OnSelected(_selected);
	}

	public void Dragged(Vector2 _mouseDelta, EDragType _dragType)
	{
		this.OnDragged(_dragType, _mouseDelta);
	}

	public void Held(EHoldType _event, float _holdDuration, float _deltaSinceLastTimedEvent = -1f)
	{
		this.OnHeld(_event, _holdDuration, _deltaSinceLastTimedEvent);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnPressed(int _mouseButton)
	{
		if (_mouseButton != -1)
		{
			if (_mouseButton == -2)
			{
				XUiEvent_OnPressEventHandler onRightPress = this.OnRightPress;
				if (onRightPress == null)
				{
					return;
				}
				onRightPress(this, _mouseButton);
			}
			return;
		}
		XUiEvent_OnPressEventHandler onPress = this.OnPress;
		if (onPress == null)
		{
			return;
		}
		onPress(this, _mouseButton);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnDoubleClicked(int _mouseButton)
	{
		XUiEvent_OnPressEventHandler onDoubleClick = this.OnDoubleClick;
		if (onDoubleClick == null)
		{
			return;
		}
		onDoubleClick(this, _mouseButton);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnHovered(bool _isOver)
	{
		XUiEvent_OnHoverEventHandler onHover = this.OnHover;
		if (onHover == null)
		{
			return;
		}
		onHover(this, _isOver);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnDragged(EDragType _dragType, Vector2 _mousePositionDelta)
	{
		XUiEvent_OnDragEventHandler onDrag = this.OnDrag;
		if (onDrag == null)
		{
			return;
		}
		onDrag(this, _dragType, _mousePositionDelta);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnHeld(EHoldType _event, float _holdDuration, float _deltaSinceLastTimedEvent)
	{
		XUiEvent_OnHeldHandler onHold = this.OnHold;
		if (onHold == null)
		{
			return;
		}
		onHold(this, _event, _holdDuration, _deltaSinceLastTimedEvent);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnScrolled(float _delta)
	{
		XUiEvent_OnScrollEventHandler onScroll = this.OnScroll;
		if (onScroll == null)
		{
			return;
		}
		onScroll(this, _delta);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnSelected(bool _selected)
	{
		XUiEvent_OnSelectEventHandler onSelect = this.OnSelect;
		if (onSelect == null)
		{
			return;
		}
		onSelect(this, _selected);
	}

	public virtual void OnOpen()
	{
		this.IsOpen = true;
		for (int i = 0; i < this.children.Count; i++)
		{
			this.children[i].OnOpen();
		}
		if (this.ViewComponent != null)
		{
			if (this.ViewComponent.ForceHide)
			{
				this.ViewComponent.IsVisible = false;
				return;
			}
			if (!this.ViewComponent.IsVisible)
			{
				this.ViewComponent.OnOpen();
				this.ViewComponent.IsVisible = true;
			}
		}
	}

	public virtual void OnClose()
	{
		this.IsOpen = false;
		for (int i = 0; i < this.children.Count; i++)
		{
			this.children[i].OnClose();
		}
		if (this.ViewComponent != null && this.ViewComponent.IsVisible)
		{
			this.ViewComponent.OnClose();
			this.ViewComponent.IsVisible = false;
		}
	}

	public virtual void OnVisibilityChanged(bool _isVisible)
	{
		XUiEvent_OnVisibilityChanged onVisiblity = this.OnVisiblity;
		if (onVisiblity == null)
		{
			return;
		}
		onVisiblity(this, _isVisible);
	}

	public virtual bool ParseAttribute(string _name, string _value, XUiController _parent)
	{
		return false;
	}

	public virtual bool GetBindingValue(ref string _value, string _bindingName)
	{
		uint num = <PrivateImplementationDetails>.ComputeStringHash(_bindingName);
		if (num <= 2577987401U)
		{
			if (num <= 1573359332U)
			{
				if (num != 820250566U)
				{
					if (num == 1573359332U)
					{
						if (_bindingName == "is_playtesting")
						{
							_value = GameUtils.IsPlaytesting().ToString();
							return true;
						}
					}
				}
				else if (_bindingName == "is_unityeditor")
				{
					_value = "false";
					return true;
				}
			}
			else if (num != 1899437583U)
			{
				if (num != 2161620009U)
				{
					if (num == 2577987401U)
					{
						if (_bindingName == "is_editmode")
						{
							_value = GameManager.Instance.IsEditMode().ToString();
							return true;
						}
					}
				}
				else if (_bindingName == "gamelanguage")
				{
					_value = Localization.language;
					return true;
				}
			}
			else if (_bindingName == "is_modal")
			{
				_value = this.WindowGroup.isModal.ToString();
				return true;
			}
		}
		else if (num <= 2759425756U)
		{
			if (num != 2745931225U)
			{
				if (num == 2759425756U)
				{
					if (_bindingName == "is_prefab_editor")
					{
						_value = PrefabEditModeManager.Instance.IsActive().ToString();
						return true;
					}
				}
			}
			else if (_bindingName == "is_server")
			{
				_value = SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer.ToString();
				return true;
			}
		}
		else if (num != 3445481009U)
		{
			if (num != 3556752319U)
			{
				if (num == 4180283068U)
				{
					if (_bindingName == "inputstyle")
					{
						this.RegisterForInputStyleChanges();
						_value = this.lastInputStyle.ToStringCached<PlayerInputManager.InputStyle>();
						return true;
					}
				}
			}
			else if (_bindingName == "is_creative")
			{
				_value = (GameStats.GetBool(EnumGameStats.IsCreativeMenuEnabled) || GamePrefs.GetBool(EnumGamePrefs.CreativeMenuEnabled)).ToString();
				return true;
			}
		}
		else if (_bindingName == "is_controller_input")
		{
			this.RegisterForInputStyleChanges();
			_value = (this.lastInputStyle != PlayerInputManager.InputStyle.Keyboard).ToString();
			return true;
		}
		return false;
	}

	public virtual void SetAllChildrenDirty(bool _includeViewComponents = false)
	{
		for (int i = 0; i < this.children.Count; i++)
		{
			this.children[i].SetAllChildrenDirty(false);
		}
		if (this.viewComponent != null)
		{
			this.viewComponent.IsDirty = true;
		}
		this.IsDirty = true;
	}

	public virtual void RefreshBindingsSelfAndChildren()
	{
		for (int i = 0; i < this.children.Count; i++)
		{
			this.children[i].RefreshBindingsSelfAndChildren();
		}
		this.RefreshBindings(true);
	}

	public void RefreshBindings(bool _forceAll = false)
	{
		for (int i = 0; i < this.BindingList.Count; i++)
		{
			this.BindingList[i].RefreshValue(_forceAll);
		}
	}

	public void AddBinding(BindingInfo _info)
	{
		if (!this.BindingList.Contains(_info))
		{
			this.BindingList.Add(_info);
		}
	}

	public virtual bool AlwaysUpdate()
	{
		return false;
	}

	public virtual void Cleanup()
	{
		foreach (XUiController xuiController in this.children)
		{
			xuiController.Cleanup();
		}
		if (this.registeredForInputStyleChanges)
		{
			IPlatform nativePlatform = PlatformManager.NativePlatform;
			if (((nativePlatform != null) ? nativePlatform.Input : null) != null)
			{
				PlatformManager.NativePlatform.Input.OnLastInputStyleChanged -= this.OnLastInputStyleChanged;
			}
		}
		XUiView xuiView = this.ViewComponent;
		if (xuiView == null)
		{
			return;
		}
		xuiView.Cleanup();
	}

	public void FindNavigatableChildren(List<XUiView> views)
	{
		foreach (XUiController xuiController in this.children)
		{
			if (xuiController.viewComponent.IsNavigatable)
			{
				views.Add(xuiController.viewComponent);
			}
			xuiController.FindNavigatableChildren(views);
		}
	}

	public bool TryFindFirstNavigableChild(out XUiView foundView)
	{
		foundView = null;
		foreach (XUiController xuiController in this.children)
		{
			if (xuiController.ViewComponent.IsNavigatable && xuiController.viewComponent.IsVisible && xuiController.viewComponent.UiTransform.gameObject.activeInHierarchy)
			{
				foundView = xuiController.viewComponent;
				return true;
			}
			if (xuiController.TryFindFirstNavigableChild(out foundView))
			{
				return true;
			}
		}
		return false;
	}

	public bool SelectCursorElement(bool _withDelay = false, bool _overrideCursorMode = false)
	{
		if (this.ViewComponent == null)
		{
			return false;
		}
		if (this.xui.playerUI.CursorController.CursorModeActive && !_overrideCursorMode)
		{
			return false;
		}
		XUiView xuiView = this.ViewComponent;
		if (xuiView.IsNavigatable && xuiView.IsVisible)
		{
			if (_withDelay)
			{
				this.xui.playerUI.CursorController.SetNavigationTargetLater(xuiView);
			}
			else
			{
				this.xui.playerUI.CursorController.SetNavigationTarget(xuiView);
			}
			return true;
		}
		this.TryFindFirstNavigableChild(out xuiView);
		if (xuiView != null)
		{
			if (_withDelay)
			{
				this.xui.playerUI.CursorController.SetNavigationTargetLater(xuiView);
			}
			else
			{
				this.xui.playerUI.CursorController.SetNavigationTarget(xuiView);
			}
			return true;
		}
		return false;
	}

	public virtual void OnCursorSelected()
	{
		if (this.parent != null)
		{
			this.parent.OnCursorSelected();
		}
	}

	public virtual void OnCursorUnSelected()
	{
		if (this.parent != null)
		{
			this.parent.OnCursorUnSelected();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void RegisterForInputStyleChanges()
	{
		if (!this.registeredForInputStyleChanges)
		{
			this.registeredForInputStyleChanges = true;
			PlatformManager.NativePlatform.Input.OnLastInputStyleChanged += this.OnLastInputStyleChanged;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnLastInputStyleChanged(PlayerInputManager.InputStyle _style)
	{
		this.curInputStyle = _style;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiView viewComponent;

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiController parent;

	[PublicizedFrom(EAccessModifier.Protected)]
	public readonly List<XUiController> children = new List<XUiController>();

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiWindowGroup windowGroup;

	[PublicizedFrom(EAccessModifier.Private)]
	public PlayerInputManager.InputStyle lastInputStyle = PlayerInputManager.InputStyle.Count;

	[PublicizedFrom(EAccessModifier.Private)]
	public PlayerInputManager.InputStyle curInputStyle;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool registeredForInputStyleChanges;

	public bool IsDirty;

	public bool IsDormant;

	public object CustomData;

	public readonly Dictionary<string, string> CustomAttributes = new Dictionary<string, string>();

	public readonly List<BindingInfo> BindingList = new List<BindingInfo>();
}
