using System;
using InControl;
using UnityEngine.Scripting;

[UnityEngine.Scripting.Preserve]
public class XUiC_KeyboardBindingEntry : XUiController
{
	public override void Init()
	{
		base.Init();
		this.label = (base.GetChildById("label").ViewComponent as XUiV_Label);
		this.value = (base.GetChildById("value").ViewComponent as XUiV_Label);
		this.unbind = (base.GetChildById("unbind").ViewComponent as XUiV_Button);
		this.button = (base.GetChildById("background").ViewComponent as XUiV_Button);
	}

	public void SetAction(PlayerAction _action)
	{
		this.action = _action;
		PlayerActionData.ActionUserData actionUserData = (PlayerActionData.ActionUserData)_action.UserData;
		base.ViewComponent.UiTransform.gameObject.name = "Entry_" + actionUserData.LocalizedName;
		this.label.Text = actionUserData.LocalizedName;
		this.button.ToolTip = actionUserData.LocalizedDescription;
		if (actionUserData.allowRebind)
		{
			this.unbind.ToolTip = Localization.Get("xuiRemoveBinding", false);
			return;
		}
		this.unbind.ForceHide = true;
		this.unbind.IsNavigatable = (this.unbind.IsSnappable = (this.unbind.IsVisible = false));
		this.unbind.UiTransform.gameObject.SetActive(false);
		this.button.ForceHide = true;
		this.button.IsNavigatable = (this.button.IsSnappable = (this.button.IsVisible = false));
		this.button.UiTransform.gameObject.SetActive(false);
	}

	public void Hide()
	{
		base.ViewComponent.UiTransform.gameObject.name = "Hidden Entry";
		this.unbind.ForceHide = true;
		this.unbind.IsNavigatable = (this.unbind.IsSnappable = (this.unbind.IsVisible = false));
		this.button.ForceHide = true;
		this.button.IsNavigatable = (this.button.IsSnappable = (this.button.IsVisible = false));
		this.button.UiTransform.gameObject.SetActive(false);
		this.unbind.UiTransform.gameObject.SetActive(false);
	}

	public PlayerAction action;

	public XUiV_Label label;

	public XUiV_Label value;

	public XUiV_Button unbind;

	public XUiV_Button button;
}
