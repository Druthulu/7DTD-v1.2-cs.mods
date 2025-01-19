using System;
using System.Collections.Generic;
using Audio;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_WindowSelector : XUiController
{
	public new XUiV_Button Selected
	{
		get
		{
			return this.selected;
		}
		set
		{
			if (this.selected != null)
			{
				this.selected.Selected = false;
			}
			this.selected = value;
			if (this.selected != null)
			{
				this.selected.Selected = true;
				this.HandleSelectedChange();
			}
		}
	}

	public string SelectedName
	{
		get
		{
			if (this.selected == null)
			{
				return "";
			}
			return this.selected.ID;
		}
	}

	public override void Init()
	{
		base.Init();
		XUiC_WindowSelector.ID = base.WindowGroup.ID;
		XUiController childById = base.GetChildById("lblWindowName");
		if (childById != null)
		{
			this.lblWindowName = (XUiV_Label)childById.ViewComponent;
		}
		this.categories.Clear();
		for (int i = 0; i < this.children.Count; i++)
		{
			XUiController xuiController = this.children[i];
			if (xuiController.ViewComponent.EventOnPress)
			{
				xuiController.OnPress += this.HandleOnPress;
				this.categories.Add(xuiController.ViewComponent.ID.ToLower());
			}
			xuiController.ViewComponent.IsNavigatable = (xuiController.ViewComponent.IsSnappable = false);
		}
		this.SetSelected("crafting");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleOnPress(XUiController _sender, int _mouseButton)
	{
		this.Selected = (XUiV_Button)_sender.ViewComponent;
		this.OpenSelectedWindow();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleSelectedChange()
	{
		this.updateWindowTitle();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateWindowTitle()
	{
		this.lblWindowName.Text = ((this.selected != null) ? Localization.Get("xui" + this.Selected.ID, false) : "");
	}

	public void OpenSelectedWindow()
	{
		if (this.Selected != null)
		{
			this.updateWindowTitle();
			string id = this.Selected.ID;
			if (!base.xui.playerUI.windowManager.IsWindowOpen(id))
			{
				base.xui.playerUI.windowManager.CloseAllOpenWindows("windowpaging");
				base.xui.playerUI.windowManager.Open(id, true, false, true);
			}
		}
	}

	public void SetSelected(string name)
	{
		XUiController childById = base.GetChildById(name.ToLower());
		if (((childById != null) ? childById.ViewComponent : null) is XUiV_Button)
		{
			this.Selected = (XUiV_Button)childById.ViewComponent;
			this.currentCategoryIndex = this.categories.IndexOf(this.Selected.ID.ToLower());
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.OpenSelectedWindow();
		base.xui.dragAndDrop.InMenu = true;
		Manager.PlayInsidePlayerHead("open_inventory", -1, 0f, false, false);
		base.RefreshBindings(false);
	}

	public override void OnClose()
	{
		base.OnClose();
		GameManager.Instance.SetPauseWindowEffects(false);
		base.xui.dragAndDrop.InMenu = false;
		Manager.PlayInsidePlayerHead("close_inventory", -1, 0f, false, false);
		if (base.xui.currentSelectedEntry != null)
		{
			base.xui.currentSelectedEntry.Selected = false;
		}
		XUiWindowGroup xuiWindowGroup = base.xui.playerUI.windowManager.GetWindow("toolbelt") as XUiWindowGroup;
		if (xuiWindowGroup == null)
		{
			return;
		}
		XUiC_Toolbelt childByType = xuiWindowGroup.Controller.GetChildByType<XUiC_Toolbelt>();
		if (childByType == null)
		{
			return;
		}
		childByType.ClearHoveredItems();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void openSelectorAndWindow(string _selectedPage)
	{
		_selectedPage = _selectedPage.ToLower();
		XUiC_FocusedBlockHealth.SetData(base.xui.playerUI, null, 0f);
		if (base.xui.playerUI.windowManager.IsWindowOpen("windowpaging") && this.SelectedName.EqualsCaseInsensitive(_selectedPage) && !this.OverrideClose)
		{
			base.xui.playerUI.windowManager.CloseAllOpenWindows(null, false);
			if (base.xui.playerUI.windowManager.IsWindowOpen("windowpaging"))
			{
				base.xui.playerUI.windowManager.Close("windowpaging");
				return;
			}
		}
		else
		{
			this.SetSelected(_selectedPage);
			if (base.xui.playerUI.windowManager.IsWindowOpen("windowpaging"))
			{
				this.OpenSelectedWindow();
				return;
			}
			base.xui.playerUI.windowManager.CloseAllOpenWindows(null, false);
			base.xui.playerUI.windowManager.Open("windowpaging", false, false, true);
		}
	}

	public static void OpenSelectorAndWindow(EntityPlayerLocal _localPlayer, string selectedPage)
	{
		if (_localPlayer.IsDead())
		{
			return;
		}
		LocalPlayerUI.GetUIForPlayer(_localPlayer).xui.FindWindowGroupByName("windowpaging").GetChildByType<XUiC_WindowSelector>().openSelectorAndWindow(selectedPage);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void toggleCategory(int _dir)
	{
		int index = NGUIMath.RepeatIndex(this.currentCategoryIndex + _dir, this.categories.Count);
		XUiController childById = base.GetChildById(this.categories[index]);
		if (((childById != null) ? childById.ViewComponent : null) is XUiV_Button)
		{
			if (childById.ViewComponent.IsVisible)
			{
				this.SetSelected(this.categories[index]);
				this.OpenSelectedWindow();
				return;
			}
			this.currentCategoryIndex = index;
			this.toggleCategory(_dir);
		}
	}

	public static void ToggleCategory(EntityPlayerLocal _localPlayer, int _dir)
	{
		LocalPlayerUI.GetUIForPlayer(_localPlayer).xui.FindWindowGroupByName("windowpaging").GetChildByType<XUiC_WindowSelector>().toggleCategory(_dir);
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		GUIWindowManager windowManager = base.xui.playerUI.windowManager;
		if (base.xui.playerUI.windowManager.IsKeyShortcutsAllowed())
		{
			if (base.xui.playerUI.playerInput.GUIActions.WindowPagingLeft.WasReleased && windowManager.IsWindowOpen(XUiC_WindowSelector.ID))
			{
				XUiC_WindowSelector.ToggleCategory(base.xui.playerUI.entityPlayer, -1);
			}
			if (base.xui.playerUI.playerInput.GUIActions.WindowPagingRight.WasReleased && windowManager.IsWindowOpen(XUiC_WindowSelector.ID))
			{
				XUiC_WindowSelector.ToggleCategory(base.xui.playerUI.entityPlayer, 1);
			}
		}
		this.OverrideClose = false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label lblWindowName;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Button selected;

	public static string ID = "";

	public bool OverrideClose;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<string> categories = new List<string>();

	[PublicizedFrom(EAccessModifier.Private)]
	public int currentCategoryIndex;
}
