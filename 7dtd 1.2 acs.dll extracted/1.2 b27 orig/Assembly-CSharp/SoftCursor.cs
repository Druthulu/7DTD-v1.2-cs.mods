using System;
using System.Collections;
using Audio;
using InControl;
using Platform;
using UnityEngine;

public class SoftCursor : CursorControllerAbs
{
	public PlayerInputManager.InputStyle LastInputStyle
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return this.m_lastInputStyle;
		}
		[PublicizedFrom(EAccessModifier.Protected)]
		set
		{
			if (value != this.m_lastInputStyle)
			{
				this.m_lastInputStyle = value;
				this.SetVisible(this.SoftcursorAllowed);
			}
		}
	}

	public bool SoftcursorEnabled
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.m_softcursorEnabled;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			if (value != this.m_softcursorEnabled)
			{
				this.m_softcursorEnabled = value;
				this.SetVisible(this.SoftcursorAllowed);
			}
		}
	}

	public override bool CursorModeActive
	{
		get
		{
			return this.cursorModeActive;
		}
	}

	public bool SoftcursorAllowed
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return this.guiActions != null && this.guiActions.Enabled && LocalPlayerUI.AnyModalWindowOpen();
		}
	}

	public Vector3 Position
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return this.cursor.transform.position;
		}
		[PublicizedFrom(EAccessModifier.Protected)]
		set
		{
			this.cursor.transform.position = value;
			this.lastMousePosition = Input.mousePosition;
		}
	}

	public Vector3 LocalPosition
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return this.cursor.transform.localPosition;
		}
		[PublicizedFrom(EAccessModifier.Protected)]
		set
		{
			this.cursor.transform.localPosition = value;
			this.lastMousePosition = Input.mousePosition;
		}
	}

	public static CursorLockMode DefaultCursorLockState
	{
		get
		{
			return CursorLockMode.None;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Awake()
	{
		this.AwakeBase();
		Cursor.lockState = SoftCursor.DefaultCursorLockState;
		foreach (UISprite uisprite in base.GetComponentsInChildren<UISprite>())
		{
			string name = uisprite.gameObject.name;
			if (name == "Cursor")
			{
				this.cursor = uisprite;
			}
			else if (name == "SelectionBox")
			{
				this.selectionBox = uisprite;
			}
		}
		this.cursorPanel = base.GetComponent<UIPanel>();
		GameObject gameObject = new GameObject("cursorMouse");
		gameObject.transform.parent = base.gameObject.transform;
		gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
		if (SoftCursor.defaultMouseCursorAtlas == null)
		{
			foreach (UIAtlas uiatlas in Resources.FindObjectsOfTypeAll<UIAtlas>())
			{
				if (uiatlas.name.EqualsCaseInsensitive(SoftCursor.defaultMouseCursorAtlasName))
				{
					SoftCursor.defaultMouseCursorAtlas = uiatlas;
				}
				if (uiatlas.name.EqualsCaseInsensitive(SoftCursor.emptyCursorAtlasName))
				{
					SoftCursor.emptyCursorAtlas = uiatlas;
				}
				if (uiatlas.name.EqualsCaseInsensitive(SoftCursor.defaultControllerCursorAtlasName))
				{
					SoftCursor.defaultControllerCursorAtlas = uiatlas;
				}
				if (uiatlas.name.EqualsCaseInsensitive(SoftCursor.mapCursorAtlasName))
				{
					SoftCursor.mapCursorAtlas = uiatlas;
				}
			}
		}
		LocalPlayerUI.primaryUI.xui.LoadData<AudioClip>("Sounds/UI/ui_hover", delegate(AudioClip _o)
		{
			this.cursorSelectSound = _o;
		});
		PlatformManager.NativePlatform.Input.OnLastInputStyleChanged += this.OnLastInputStyleChanged;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnLastInputStyleChanged(PlayerInputManager.InputStyle _style)
	{
		if (_style != PlayerInputManager.InputStyle.Keyboard)
		{
			if (this.LastInputStyle == PlayerInputManager.InputStyle.Keyboard)
			{
				this.LocalPosition = this.cursorPanel.transform.worldToLocalMatrix.MultiplyPoint3x4(this.uiCamera.cachedCamera.ScreenToWorldPoint(Input.mousePosition));
			}
			this.RefreshSelection();
		}
		this.SetVisible(this.SoftcursorAllowed);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnEnable()
	{
		this.uiCamera = base.GetComponentInParent<UICamera>();
		this.UpdateMoveSpeed();
		base.InitCursorBounds();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDestroy()
	{
		base.DestroyBase();
	}

	public override void UpdateMoveSpeed()
	{
		float @float = GamePrefs.GetFloat(EnumGamePrefs.OptionsInterfaceSensitivity);
		this.speed = 500f + 2000f * @float;
		this.mouseSpeed = (500f + 2000f * @float) / 1000f * (1f / MouseBindingSource.ScaleX) * 4f;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		if (Application.isPlaying && !GameManager.Instance.IsQuitting && !GameManager.Instance.m_GUIConsole.isShowing)
		{
			this.LastInputStyle = PlatformManager.NativePlatform.Input.CurrentInputStyle;
			if (this.guiActions != null && this.windowManager != null)
			{
				if (this.SoftcursorAllowed != this.SoftcursorEnabled)
				{
					this.SoftcursorEnabled = (this.SoftcursorAllowed & !this.hidden);
				}
				if (this.SoftcursorEnabled)
				{
					this.HandleMovement();
				}
			}
			this.LastFrameTime = Time.realtimeSinceStartup;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void LateUpdate()
	{
		if (this.selectionBox.enabled)
		{
			this.RefreshSelection();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void HandleMovement()
	{
		if (this.guiActions == null)
		{
			return;
		}
		if (base.Locked || this.hidden)
		{
			return;
		}
		this.movingMouse = false;
		Vector2 lhs = Input.mousePosition;
		if (lhs != this.lastMousePosition)
		{
			this.movingMouse = true;
		}
		this.lastMousePosition = lhs;
		if (this.LastInputStyle != PlayerInputManager.InputStyle.Keyboard)
		{
			if (this.movingMouse)
			{
				this.LocalPosition = this.cursorPanel.transform.worldToLocalMatrix.MultiplyPoint3x4(this.uiCamera.cachedCamera.ScreenToWorldPoint(Input.mousePosition));
				this.SetVisible(true);
			}
			else
			{
				if (!CursorControllerAbs.FreeCursorEnabled)
				{
					return;
				}
				Vector2 vector = new Vector2(this.guiActions.Right.RawValue - this.guiActions.Left.RawValue, this.guiActions.Up.RawValue - this.guiActions.Down.RawValue);
				float magnitude = vector.magnitude;
				if (magnitude > 0f)
				{
					this.cursorModeActive = true;
					this.SetVisible(true);
					this.SetNavigationTarget(null);
				}
				Vector3 localPosition = this.LocalPosition;
				Vector3 localPosition2 = this.LocalPosition;
				if (this.bHasHoverTarget && (base.HoverTarget == null || !base.HoverTarget.ColliderEnabled || !base.HoverTarget.UiTransform.gameObject.activeInHierarchy))
				{
					base.HoverTarget = null;
				}
				float b = this.bHasHoverTarget ? CursorControllerAbs.hoverSpeed : 1f;
				this.currentAcceleration = Mathf.Clamp(this.currentAcceleration + magnitude * Time.unscaledDeltaTime, 0f, Mathf.Min(magnitude, b));
				this.speedMultiplier = Mathf.MoveTowards(this.speedMultiplier, this.bHasHoverTarget ? CursorControllerAbs.hoverSpeed : 1f, Time.unscaledDeltaTime * (this.bHasHoverTarget ? 10f : 1f));
				float num = Time.unscaledDeltaTime * this.speed * this.speedMultiplier * this.accelerationCurve.Evaluate(this.currentAcceleration);
				vector.x *= num;
				vector.y *= num;
				localPosition2.x += vector.x;
				localPosition2.y += vector.y;
				this.LocalPosition = localPosition2;
				if (CursorControllerAbs.bSnapCursor && localPosition == localPosition2)
				{
					if (!this.snapped)
					{
						this.Snap();
						this.snapped = true;
					}
				}
				else
				{
					this.snapped = false;
				}
			}
			this.ConstrainCursor();
			return;
		}
		this.LocalPosition = this.cursorPanel.transform.worldToLocalMatrix.MultiplyPoint3x4(this.uiCamera.cachedCamera.ScreenToWorldPoint(Input.mousePosition));
		this.ConstrainCursor();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 ConstrainToBounds(Vector3 _newPosition)
	{
		Vector3 vector = _newPosition;
		if (this.LastInputStyle != PlayerInputManager.InputStyle.Keyboard)
		{
			vector.z = this.currentBounds.center.z;
			vector = this.currentBounds.ClosestPoint(vector);
		}
		return vector;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ConstrainCursor()
	{
		Vector3 newPosition = this.uiCamera.cachedCamera.WorldToScreenPoint(this.Position);
		Vector3 vector = this.ConstrainToBounds(newPosition);
		vector = this.uiCamera.cachedCamera.ScreenToViewportPoint(vector);
		if (vector.x < 0f)
		{
			vector.x = 0f;
		}
		else if (vector.x > 1f)
		{
			vector.x = 1f;
		}
		if (vector.y < 0f)
		{
			vector.y = 0f;
		}
		else if (vector.y > 1f)
		{
			vector.y = 1f;
		}
		this.Position = this.uiCamera.cachedCamera.ViewportToWorldPoint(vector);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Snap()
	{
		if (GamePrefs.GetBool(EnumGamePrefs.OptionsControllerCursorSnap) && this.hoverTarget != null && this.hoverTarget.IsSnappable && this.hoverTarget.ColliderEnabled && this.hoverTarget.UiTransform.gameObject.activeInHierarchy)
		{
			Bounds bounds = this.hoverTarget.bounds;
			if (this.cursorWorldBounds.extents.x > bounds.extents.x - this.OffsetSnapBounds)
			{
				this.Position = bounds.center;
				return;
			}
			Vector3 vector = bounds.ClosestPoint(this.Position);
			Vector3 b = Vector3.right * this.cursorWorldBounds.extents.x;
			Vector3 point = vector - b;
			Vector3 point2 = vector + b;
			if (!bounds.Contains(point))
			{
				vector = bounds.ClosestPoint(point) + b;
			}
			else if (!bounds.Contains(point2))
			{
				vector = bounds.ClosestPoint(point2) - b;
			}
			vector.y = bounds.center.y;
			this.Position = vector;
		}
	}

	public override Vector2 GetScreenPosition()
	{
		Vector2 result = default(Vector2);
		if (this.uiCamera != null)
		{
			result = this.uiCamera.cachedCamera.WorldToScreenPoint(this.Position);
		}
		return result;
	}

	public override Vector2 GetLocalScreenPosition()
	{
		Vector2 vector = default(Vector2);
		if (this.uiCamera != null)
		{
			vector = this.uiCamera.cachedCamera.WorldToViewportPoint(this.Position);
			vector.x *= (float)this.uiCamera.cachedCamera.pixelWidth;
			vector.y *= (float)this.uiCamera.cachedCamera.pixelHeight;
		}
		return vector;
	}

	public override void SetScreenPosition(Vector2 _newPosition)
	{
		this.Position = this.uiCamera.cachedCamera.ScreenToWorldPoint(_newPosition);
	}

	public override void SetScreenPosition(float _x, float _y)
	{
		this.SetScreenPosition(new Vector2(_x, _y));
	}

	public override void ResetToCenter()
	{
		this.LocalPosition = Vector3.zero;
	}

	public override void SetCursorHidden(bool _hidden)
	{
		this.hidden = _hidden;
		this.SoftcursorEnabled &= !this.hidden;
		SoftCursor.SetCursor(_hidden ? CursorControllerAbs.ECursorType.None : CursorControllerAbs.ECursorType.Default);
		Cursor.visible = (PlatformManager.NativePlatform.Input.CurrentInputStyle == PlayerInputManager.InputStyle.Keyboard && !this.hidden);
		if (this.LastInputStyle != PlayerInputManager.InputStyle.Keyboard && !this.hidden)
		{
			this.RefreshSelection();
		}
	}

	public override bool GetCursorHidden()
	{
		return this.hidden;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetCursorSprite(UIAtlas _atlasMouse, string _spriteMouse, UIWidget.Pivot _pivotMouse, UIAtlas _atlasController, string _spriteController, UIWidget.Pivot _pivotController)
	{
		this.cursor.atlas = _atlasController;
		this.cursor.spriteName = _spriteController;
		this.cursor.pivot = _pivotController;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetCursorSpriteNone()
	{
		this.cursor.atlas = null;
		this.cursor.spriteName = "";
		this.SetSelectionBoxEnabled(false);
	}

	public new static void SetCursor(CursorControllerAbs.ECursorType _cursorType)
	{
		switch (_cursorType)
		{
		case CursorControllerAbs.ECursorType.None:
			for (int i = 0; i < CursorControllerAbs.softCursors.Count; i++)
			{
				(CursorControllerAbs.softCursors[i] as SoftCursor).SetCursorSpriteNone();
			}
			return;
		case CursorControllerAbs.ECursorType.Default:
			for (int j = 0; j < CursorControllerAbs.softCursors.Count; j++)
			{
				(CursorControllerAbs.softCursors[j] as SoftCursor).SetCursorSprite(SoftCursor.defaultMouseCursorAtlas, SoftCursor.defaultMouseCursorSprite, SoftCursor.defaultMouseCursorPivot, SoftCursor.defaultControllerCursorAtlas, SoftCursor.defaultControllerCursorSprite, SoftCursor.defaultControllerCursorPivot);
			}
			return;
		case CursorControllerAbs.ECursorType.Map:
			for (int k = 0; k < CursorControllerAbs.softCursors.Count; k++)
			{
				(CursorControllerAbs.softCursors[k] as SoftCursor).SetCursorSprite(SoftCursor.mapCursorAtlas, SoftCursor.mapCursorSprite, SoftCursor.mapCursorPivot, SoftCursor.mapCursorAtlas, SoftCursor.mapCursorSprite, SoftCursor.mapCursorPivot);
			}
			return;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetVisible(bool _visible)
	{
		Cursor.lockState = (_visible ? SoftCursor.DefaultCursorLockState : CursorLockMode.Locked);
		if (this.LastInputStyle == PlayerInputManager.InputStyle.Keyboard || this.movingMouse)
		{
			Cursor.visible = _visible;
			this.cursor.enabled = false;
			this.SetSelectionBoxEnabled(false);
		}
		else
		{
			Cursor.visible = false;
			this.cursor.enabled = (_visible && CursorControllerAbs.FreeCursorEnabled && !base.Locked && this.cursorModeActive && !base.VirtualCursorHidden);
			this.SetSelectionBoxEnabled(_visible && !this.cursorModeActive && !this.hidden && base.navigationTarget != null && base.navigationTarget.UseSelectionBox);
		}
		GameManager.Instance.bCursorVisible = _visible;
		this.lastMousePosition = Input.mousePosition;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnVirtualCursorVisibleChanged()
	{
		if (this.LastInputStyle == PlayerInputManager.InputStyle.Keyboard || this.movingMouse)
		{
			return;
		}
		this.cursor.enabled = (!base.VirtualCursorHidden && !base.Locked && this.cursorModeActive);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetSelectionBoxEnabled(bool _enabled)
	{
		this.selectionBox.enabled = _enabled;
	}

	public static void SetCursorVisible(bool _visible)
	{
		for (int i = 0; i < CursorControllerAbs.softCursors.Count; i++)
		{
			(CursorControllerAbs.softCursors[i] as SoftCursor).SetVisible(_visible);
		}
	}

	public Vector2 GetFlatPosition()
	{
		return new Vector2(this.Position.x, this.Position.y);
	}

	public override void SetNavigationTarget(XUiView _view)
	{
		if (_view != null && !_view.IsNavigatable)
		{
			this.SetNavigationTarget(null);
			return;
		}
		if (_view == base.navigationTarget)
		{
			return;
		}
		if (base.lockNavigationToView != null && (_view == null || !_view.Controller.IsChildOf(base.lockNavigationToView.Controller)))
		{
			return;
		}
		if (base.navigationTarget != null)
		{
			base.navigationTarget.Controller.OnCursorUnSelected();
		}
		if (_view != null && PlatformManager.NativePlatform.Input.CurrentInputStyle != PlayerInputManager.InputStyle.Keyboard)
		{
			_view.Controller.OnCursorSelected();
			Manager.PlayXUiSound(this.cursorSelectSound, 1f);
			this.cursorModeActive = false;
			this.SetSelectionBoxEnabled(_view.UseSelectionBox);
			this.PositionSelectionBox(_view);
		}
		else
		{
			this.SetSelectionBoxEnabled(false);
		}
		base.navigationTarget = _view;
		if (PlatformManager.NativePlatform.Input.CurrentInputStyle != PlayerInputManager.InputStyle.Keyboard)
		{
			this.SetVisible(base.navigationTarget != null);
		}
	}

	public override void RefreshSelection()
	{
		if (base.navigationTarget == null)
		{
			return;
		}
		this.PositionSelectionBox(base.navigationTarget);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PositionSelectionBox(XUiView _view)
	{
		this.Position = _view.Center;
		if (_view.UseSelectionBox)
		{
			this.selectionBox.transform.position = _view.Center;
			if (_view.Controller.GetParentWindow().IsInStackpanel)
			{
				this.selectionBox.width = (int)((float)_view.Size.x * _view.xui.transform.localScale.x * _view.xui.StackPanelTransform.localScale.x + (float)SoftCursor.selectionBoxMargin);
				this.selectionBox.height = (int)((float)_view.Size.y * _view.xui.transform.localScale.y * _view.xui.StackPanelTransform.localScale.y + (float)SoftCursor.selectionBoxMargin);
				return;
			}
			this.selectionBox.width = (int)((float)_view.Size.x * _view.xui.transform.localScale.x + (float)SoftCursor.selectionBoxMargin);
			this.selectionBox.height = (int)((float)_view.Size.y * _view.xui.transform.localScale.y + (float)SoftCursor.selectionBoxMargin);
		}
	}

	public override void SetNavigationLockView(XUiView _view, XUiView _viewToSelect = null)
	{
		if (_view != null && (!_view.IsVisible || !_view.UiTransform.gameObject.activeInHierarchy))
		{
			this.SetNavigationLockView(null, null);
			return;
		}
		base.lockNavigationToView = _view;
		if (_viewToSelect != null)
		{
			_viewToSelect.Controller.SelectCursorElement(true, false);
			return;
		}
		if (_view != null)
		{
			_view.Controller.SelectCursorElement(true, false);
		}
	}

	public override void SetNavigationTargetLater(XUiView _view)
	{
		if (_view == null)
		{
			this.SetNavigationTarget(null);
			return;
		}
		base.StartCoroutine(this.SetNavigationTargetWithDelay(_view));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator SetNavigationTargetWithDelay(XUiView _view)
	{
		base.Locked = true;
		int num;
		for (int i = 0; i < 3; i = num + 1)
		{
			yield return null;
			num = i;
		}
		base.Locked = false;
		if (_view != null && _view.HasCollider)
		{
			this.SetNavigationTarget(_view);
		}
		this.lastMousePosition = Input.mousePosition;
		yield break;
	}

	public override void ResetNavigationTarget()
	{
		if (base.navigationTarget != null)
		{
			if (PlatformManager.NativePlatform.Input.CurrentInputStyle != PlayerInputManager.InputStyle.Keyboard && base.navigationTarget.HasCollider)
			{
				this.Position = base.navigationTarget.Center;
				return;
			}
			this.SetNavigationTarget(null);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float BaseSpeed = 500f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float SpeedModRange = 2000f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static readonly string emptyCursorAtlasName = "UIAtlas";

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static readonly string emptyCursorSprite = "";

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static readonly UIWidget.Pivot emptyCursorPivot = UIWidget.Pivot.Center;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static UIAtlas emptyCursorAtlas;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static readonly string defaultControllerCursorAtlasName = "UIAtlas";

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static readonly string defaultControllerCursorSprite = "soft_cursor";

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static readonly UIWidget.Pivot defaultControllerCursorPivot = UIWidget.Pivot.Center;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static UIAtlas defaultControllerCursorAtlas;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static readonly string defaultMouseCursorAtlasName = "UIAtlas";

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static readonly string defaultMouseCursorSprite = "cursor01";

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static readonly UIWidget.Pivot defaultMouseCursorPivot = UIWidget.Pivot.TopLeft;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static UIAtlas defaultMouseCursorAtlas;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static readonly string mapCursorAtlasName = "UIAtlas";

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static readonly string mapCursorSprite = "map_cursor";

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static readonly UIWidget.Pivot mapCursorPivot = UIWidget.Pivot.Center;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static UIAtlas mapCursorAtlas;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float speed;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float mouseSpeed;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float LastFrameTime;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool hidden;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool snapped;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float OffsetSnapBounds = 0.1f;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public UIPanel cursorPanel;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Vector2 lastMousePosition = Vector2.zero;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float currentAcceleration;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float speedMultiplier = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool movingMouse;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public UISprite selectionBox;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static int selectionBoxMargin = 10;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public AudioClip cursorSelectSound;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public PlayerInputManager.InputStyle m_lastInputStyle = PlayerInputManager.InputStyle.Count;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool m_softcursorEnabled;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool cursorModeActive;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public CursorControllerAbs.ECursorType currentCursorType = CursorControllerAbs.ECursorType.Default;
}
