using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_SelectableEntry : XUiController
{
	public new bool Selected
	{
		get
		{
			return this.selected;
		}
		set
		{
			if (value)
			{
				if (base.xui.currentSelectedEntry != null)
				{
					base.xui.currentSelectedEntry.SelectedChanged(false);
					base.xui.currentSelectedEntry.selected = false;
				}
			}
			else if (base.xui.currentSelectedEntry == this)
			{
				base.xui.currentSelectedEntry.SelectedChanged(false);
				base.xui.currentSelectedEntry.selected = false;
				base.xui.currentSelectedEntry = null;
			}
			this.selected = value;
			if (this.selected)
			{
				base.xui.currentSelectedEntry = this;
			}
			this.SelectedChanged(this.selected);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void SelectedChanged(bool isSelected)
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool selected;
}
