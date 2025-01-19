using System;

public class BindingItemStandard : BindingItem
{
	public BindingItemStandard(BindingInfo _parent, XUiView _view, string _sourceText) : base(_sourceText)
	{
		string[] array = this.FieldName.Split(BindingItemStandard.bindingTypeSplitChar);
		if (array.Length > 1)
		{
			for (int i = 1; i < array.Length; i++)
			{
				if (array[i].EqualsCaseInsensitive("once"))
				{
					this.BindingType = BindingItem.BindingTypes.Once;
				}
			}
			this.FieldName = array[0];
		}
		for (XUiController xuiController = _view.Controller; xuiController != null; xuiController = xuiController.Parent)
		{
			if (xuiController.GetType() != typeof(XUiController))
			{
				this.DataContext = xuiController;
				string text = "";
				if (this.DataContext.GetBindingValue(ref text, this.FieldName))
				{
					this.DataContext.AddBinding(_parent);
					return;
				}
			}
		}
	}

	public override string GetValue(bool _forceAll = false)
	{
		if (this.BindingType == BindingItem.BindingTypes.Complete && !_forceAll)
		{
			return this.CurrentValue;
		}
		if (!this.DataContext.GetBindingValue(ref this.CurrentValue, this.FieldName))
		{
			return this.CurrentValue;
		}
		if (this.CurrentValue != null && this.CurrentValue.Contains("{cvar("))
		{
			this.CurrentValue = base.ParseCVars(this.CurrentValue);
		}
		if (this.BindingType == BindingItem.BindingTypes.Once)
		{
			this.BindingType = BindingItem.BindingTypes.Complete;
		}
		return this.CurrentValue;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly char[] bindingTypeSplitChar = new char[]
	{
		'|'
	};
}
