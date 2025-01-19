using System;

public class MenuItemEntry
{
	public string Text { get; set; }

	public string IconName { get; set; }

	public bool IsEnabled { get; set; }

	public object Tag { get; set; }

	public event XUiEvent_MenuItemClicked ItemClicked;

	public void HandleItemClicked()
	{
		if (this.ItemClicked != null)
		{
			this.ItemClicked(this);
		}
	}
}
