using System;
using Platform;

public class XUiWindowGroup : GUIWindow
{
	public string ID
	{
		get
		{
			return this.id;
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
			this.playerUI = this.mXUi.playerUI;
			this.windowManager = this.playerUI.windowManager;
			this.nguiWindowManager = this.playerUI.nguiWindowManager;
		}
	}

	public XUiWindowGroup(string _id, XUiWindowGroup.EHasActionSetFor _hasActionSetFor = XUiWindowGroup.EHasActionSetFor.Both, string _defaultSelectedName = "") : base(_id)
	{
		this.hasActionSetFor = _hasActionSetFor;
		this.defaultSelectedView = _defaultSelectedName;
	}

	public bool Initialized
	{
		get
		{
			return this.initialized;
		}
	}

	public void Init()
	{
		if (this.initialized)
		{
			return;
		}
		this.Controller.Init();
		for (int i = 0; i < this.Controller.Children.Count; i++)
		{
			XUiController xuiController = this.Controller.Children[i];
			if (xuiController.ViewComponent != null)
			{
				xuiController.ViewComponent.IsVisible = false;
				string name = xuiController.ViewComponent.UiTransform.parent.name;
				this.UseStackPanelAlignment |= (name.EqualsCaseInsensitive("left") || name.EqualsCaseInsensitive("right"));
			}
		}
		this.windowManager.Add(this.id, this);
		this.initialized = true;
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.Controller.OnOpen();
		if (!string.IsNullOrEmpty(this.defaultSelectedView))
		{
			if (this.defaultSelectedView.StartsWith("bp."))
			{
				XUiC_BackpackWindow.defaultSelectedElement = this.defaultSelectedView.Remove(0, 3);
			}
			else
			{
				XUiController childById = this.Controller.GetChildById(this.defaultSelectedView);
				if (childById != null)
				{
					childById.SelectCursorElement(true, false);
				}
				else
				{
					Log.Warning("Could not find selectable element {0} in WindowGroup {1}", new object[]
					{
						this.defaultSelectedView,
						this.ID
					});
				}
			}
		}
		if (this.closeCompassOnOpen)
		{
			this.windowManager.CloseIfOpen("compass");
		}
		if (this.openBackpackOnOpen && GameManager.Instance != null)
		{
			this.windowManager.OpenIfNotOpen("backpack", false, false, true);
		}
		this.xui.RecenterWindowGroup(this, false);
		switch (this.hasActionSetFor)
		{
		case XUiWindowGroup.EHasActionSetFor.Both:
			this.hasActionSetThisOpen = true;
			return;
		case XUiWindowGroup.EHasActionSetFor.OnlyController:
			this.hasActionSetThisOpen = (PlatformManager.NativePlatform.Input.CurrentInputStyle != PlayerInputManager.InputStyle.Keyboard);
			return;
		case XUiWindowGroup.EHasActionSetFor.OnlyKeyboard:
			this.hasActionSetThisOpen = (PlatformManager.NativePlatform.Input.CurrentInputStyle == PlayerInputManager.InputStyle.Keyboard);
			return;
		case XUiWindowGroup.EHasActionSetFor.None:
			this.hasActionSetThisOpen = false;
			return;
		default:
			return;
		}
	}

	public override void OnClose()
	{
		base.OnClose();
		if (this.xui.dragAndDrop != null)
		{
			this.xui.dragAndDrop.PlaceItemBackInInventory();
		}
		this.Controller.OnClose();
		if (this.openBackpackOnOpen && GameManager.Instance != null)
		{
			this.windowManager.CloseIfOpen("backpack");
		}
		if (this.xui.currentToolTip != null)
		{
			this.xui.currentToolTip.ToolTip = "";
		}
		if (this.xui.currentPopupMenu != null)
		{
			this.xui.currentPopupMenu.ClearItems();
		}
	}

	public override bool HasActionSet()
	{
		return this.hasActionSetThisOpen;
	}

	public bool HasStackPanelWindows()
	{
		if (this.Controller == null)
		{
			return false;
		}
		foreach (XUiController xuiController in this.Controller.Children)
		{
			XUiV_Window xuiV_Window = xuiController.ViewComponent as XUiV_Window;
			if (xuiV_Window != null && xuiV_Window.IsInStackpanel)
			{
				return true;
			}
		}
		return false;
	}

	public XUiController Controller;

	public bool LeftPanelVAlignTop = true;

	public bool RightPanelVAlignTop = true;

	public bool UseStackPanelAlignment;

	public bool BoundsCalculated;

	public int StackPanelYOffset = 457;

	public int StackPanelPadding = 9;

	public bool openBackpackOnOpen;

	public bool closeCompassOnOpen;

	public string defaultSelectedView;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUi mXUi;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiWindowGroup.EHasActionSetFor hasActionSetFor;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool hasActionSetThisOpen = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool initialized;

	public enum EHasActionSetFor
	{
		Both,
		OnlyController,
		OnlyKeyboard,
		None
	}
}
