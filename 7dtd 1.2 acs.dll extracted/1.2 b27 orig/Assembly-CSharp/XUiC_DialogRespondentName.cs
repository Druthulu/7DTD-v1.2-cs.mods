using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_DialogRespondentName : XUiController
{
	public Dialog CurrentDialog
	{
		get
		{
			return this.currentDialog;
		}
		set
		{
			this.currentDialog = value;
			base.RefreshBindings(true);
			this.IsDirty = true;
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		base.RefreshBindings(false);
	}

	public override void OnClose()
	{
		base.OnClose();
		this.currentDialog = null;
	}

	public override bool GetBindingValue(ref string value, string bindingName)
	{
		if (bindingName == "respondentname")
		{
			value = ((base.xui.Dialog.Respondent != null) ? Localization.Get(base.xui.Dialog.Respondent.EntityName, false) : "");
			return true;
		}
		return false;
	}

	public void Refresh()
	{
		base.RefreshBindings(true);
		this.IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label label;

	[PublicizedFrom(EAccessModifier.Private)]
	public Dialog currentDialog;
}
