using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_EditorPanelSelector : XUiController
{
	public new XUiV_Button Selected
	{
		get
		{
			return this.selected;
		}
		set
		{
			if (this.selected != value)
			{
				if (this.selected != null)
				{
					this.selected.Selected = false;
				}
				this.selected = value;
				if (this.selected != null)
				{
					this.selected.Selected = true;
				}
				this.IsDirty = true;
			}
		}
	}

	public override void Init()
	{
		base.Init();
		XUiC_EditorPanelSelector.ID = base.WindowGroup.ID;
		XUiController childById = base.GetChildById("buttons");
		if (childById != null)
		{
			this.windowNames.Clear();
			for (int i = 0; i < childById.Children.Count; i++)
			{
				XUiController xuiController = childById.Children[i];
				if (xuiController.ViewComponent.EventOnPress)
				{
					xuiController.OnPress += this.HandleOnPress;
					XUiV_Button xuiV_Button = xuiController.ViewComponent as XUiV_Button;
					this.windowNames.Add(xuiV_Button.ID);
					if (i == 0)
					{
						this.SetSelected(xuiV_Button.ID);
					}
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleOnPress(XUiController _sender, int _mouseButton)
	{
		this.Selected = (XUiV_Button)_sender.ViewComponent;
		this.OpenSelectedWindow();
	}

	public void OpenSelectedWindow()
	{
		string text = (this.Selected != null) ? this.Selected.ID : null;
		for (int i = 0; i < this.windowNames.Count; i++)
		{
			if (text == null || text != this.windowNames[i])
			{
				base.xui.playerUI.windowManager.Close(this.windowNames[i]);
			}
		}
		if (text != null)
		{
			base.xui.playerUI.windowManager.OpenIfNotOpen(text, false, false, true);
		}
	}

	public void SetSelected(string name)
	{
		XUiController childById = base.GetChildById(name);
		if (childById != null && childById.ViewComponent is XUiV_Button)
		{
			this.Selected = (XUiV_Button)childById.ViewComponent;
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		if (PrefabEditModeManager.Instance.IsActive() && PrefabEditModeManager.Instance.VoxelPrefab != null)
		{
			PrefabEditModeManager.Instance.VoxelPrefab.RenderingCostStats = WorldStats.CaptureWorldStats();
		}
		this.OpenSelectedWindow();
	}

	public override void OnClose()
	{
		base.OnClose();
		for (int i = 0; i < this.windowNames.Count; i++)
		{
			base.xui.playerUI.windowManager.Close(this.windowNames[i]);
		}
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

	public override bool GetBindingValue(ref string value, string bindingName)
	{
		if (bindingName == "panelname")
		{
			value = ((this.selected != null) ? this.Selected.ToolTip : "");
			return true;
		}
		return base.GetBindingValue(ref value, bindingName);
	}

	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public List<string> windowNames = new List<string>();

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Button selected;
}
