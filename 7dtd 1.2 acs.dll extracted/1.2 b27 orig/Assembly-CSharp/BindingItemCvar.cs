using System;

public class BindingItemCvar : BindingItem
{
	public BindingItemCvar(BindingInfo _parent, XUiView _view, string _sourceText) : base(_sourceText)
	{
		this.FieldName = this.FieldName.Replace("cvar(", "").Replace(")", "");
		if (this.FieldName.IndexOf(BindingItem.cvarFormatSplitChar) >= 0)
		{
			string[] array = this.FieldName.Split(BindingItem.cvarFormatSplitCharArray);
			this.FieldName = array[0];
			this.format = array[1];
		}
		for (XUiController xuiController = _view.Controller; xuiController != null; xuiController = xuiController.Parent)
		{
			if (xuiController.GetType() != typeof(XUiController))
			{
				this.DataContext = xuiController;
				this.DataContext.AddBinding(_parent);
				return;
			}
		}
	}

	public override string GetValue(bool _forceAll = false)
	{
		if (this.BindingType == BindingItem.BindingTypes.Complete && !_forceAll)
		{
			return this.CurrentValue;
		}
		this.CurrentValue = XUiM_Player.GetPlayer().GetCVar(this.FieldName).ToString(this.format);
		if (this.BindingType == BindingItem.BindingTypes.Once)
		{
			this.BindingType = BindingItem.BindingTypes.Complete;
		}
		return this.CurrentValue;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly string format;
}
