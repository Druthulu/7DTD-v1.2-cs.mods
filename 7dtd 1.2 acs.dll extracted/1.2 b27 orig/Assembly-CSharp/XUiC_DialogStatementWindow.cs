using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_DialogStatementWindow : XUiController
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

	public override void Init()
	{
		base.Init();
		XUiController childById = base.GetChildById("statementLabel");
		if (childById != null)
		{
			this.label = (XUiV_Label)childById.ViewComponent;
		}
		childById = base.GetChildById("backgroundSprite");
		if (childById != null)
		{
			this.backgroundSprite = (XUiV_Sprite)childById.ViewComponent;
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

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (this.IsDirty)
		{
			this.IsDirty = false;
		}
	}

	public override bool GetBindingValue(ref string value, string bindingName)
	{
		if (bindingName == "statement")
		{
			value = ((this.currentDialog != null && this.currentDialog.CurrentStatement != null) ? this.currentDialog.CurrentStatement.Text : "");
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
	public XUiV_Sprite backgroundSprite;

	[PublicizedFrom(EAccessModifier.Private)]
	public Dialog currentDialog;
}
