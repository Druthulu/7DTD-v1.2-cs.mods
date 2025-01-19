﻿using System;
using Twitch;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_TwitchActionHistoryEntry : XUiController
{
	public TwitchActionHistoryEntry HistoryItem
	{
		get
		{
			return this.historyItem;
		}
		set
		{
			base.ViewComponent.Enabled = (value != null);
			this.historyItem = value;
			this.IsDirty = true;
		}
	}

	public XUiC_TwitchInfoWindowGroup TwitchInfoUIHandler { get; set; }

	public override bool GetBindingValue(ref string value, string bindingName)
	{
		bool flag = this.historyItem != null;
		uint num = <PrivateImplementationDetails>.ComputeStringHash(bindingName);
		if (num <= 2472102578U)
		{
			if (num <= 1320097209U)
			{
				if (num != 765459171U)
				{
					if (num == 1320097209U)
					{
						if (bindingName == "username")
						{
							if (flag)
							{
								if (this.historyItem.IsRefunded)
								{
									value = this.historyItem.UserName;
								}
								else
								{
									value = string.Format("[{0}]{1}[-]", this.historyItem.UserColor, this.historyItem.UserName);
								}
							}
							else
							{
								value = "";
							}
							return true;
						}
					}
				}
				else if (bindingName == "rowstatecolor")
				{
					value = (this.Selected ? "255,255,255,255" : (this.IsHovered ? this.hoverColor : this.rowColor));
					return true;
				}
			}
			else if (num != 1656712805U)
			{
				if (num == 2472102578U)
				{
					if (bindingName == "command")
					{
						value = (flag ? this.historyItem.Command : "");
						return true;
					}
				}
			}
			else if (bindingName == "rowstatesprite")
			{
				value = (this.Selected ? "ui_game_select_row" : "menu_empty");
				return true;
			}
		}
		else if (num <= 3284901973U)
		{
			if (num != 2511012101U)
			{
				if (num == 3284901973U)
				{
					if (bindingName == "command_with_cost")
					{
						value = (flag ? this.historyItem.Command : "");
						return true;
					}
				}
			}
			else if (bindingName == "commandcolor")
			{
				if (flag)
				{
					if (this.historyItem.Action != null)
					{
						if (this.historyItem.IsRefunded)
						{
							value = this.disabledColor;
						}
						else if (this.historyItem.Action.IsPositive)
						{
							value = this.positiveColor;
						}
						else
						{
							value = this.negativeColor;
						}
					}
					else if (this.historyItem.Vote != null)
					{
						value = this.historyItem.Vote.TitleColor;
					}
					else if (this.historyItem.EventEntry != null)
					{
						value = "255,255,255,255";
					}
				}
				return true;
			}
		}
		else if (num != 3644377122U)
		{
			if (num == 3898356536U)
			{
				if (bindingName == "cost")
				{
					value = (flag ? this.historyItem.Action.CurrentCost.ToString() : "");
					return true;
				}
			}
		}
		else if (bindingName == "textstatecolor")
		{
			value = "255,255,255,255";
			if (flag)
			{
				value = (this.historyItem.IsRefunded ? this.disabledColor : this.enabledColor);
			}
			return true;
		}
		return false;
	}

	public override void Init()
	{
		base.Init();
		this.IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnHovered(bool _isOver)
	{
		base.OnHovered(_isOver);
		if (this.historyItem == null)
		{
			this.IsHovered = false;
			return;
		}
		if (this.IsHovered != _isOver)
		{
			this.IsHovered = _isOver;
			base.RefreshBindings(false);
		}
	}

	public override void OnCursorSelected()
	{
		base.OnCursorSelected();
		base.GetParentByType<XUiC_TwitchActionHistoryEntryList>().SelectedEntry = this;
		this.TwitchInfoUIHandler.SetEntry(this);
	}

	public override void Update(float _dt)
	{
		base.RefreshBindings(this.IsDirty);
		this.IsDirty = false;
		base.Update(_dt);
	}

	public void Refresh()
	{
		this.IsDirty = true;
	}

	public override bool ParseAttribute(string name, string value, XUiController _parent)
	{
		if (name == "enabled_color")
		{
			this.enabledColor = value;
			return true;
		}
		if (name == "disabled_color")
		{
			this.disabledColor = value;
			return true;
		}
		if (name == "positive_color")
		{
			this.positiveColor = value;
			return true;
		}
		if (name == "negative_color")
		{
			this.negativeColor = value;
			return true;
		}
		if (name == "row_color")
		{
			this.rowColor = value;
			return true;
		}
		if (!(name == "hover_color"))
		{
			return base.ParseAttribute(name, value, _parent);
		}
		this.hoverColor = value;
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string enabledColor;

	[PublicizedFrom(EAccessModifier.Private)]
	public string disabledColor;

	[PublicizedFrom(EAccessModifier.Private)]
	public string rowColor;

	[PublicizedFrom(EAccessModifier.Private)]
	public string hoverColor;

	[PublicizedFrom(EAccessModifier.Private)]
	public string positiveColor = "0,0,255";

	[PublicizedFrom(EAccessModifier.Private)]
	public string negativeColor = "255,0,0";

	public new bool Selected;

	public bool IsHovered;

	[PublicizedFrom(EAccessModifier.Private)]
	public TwitchActionHistoryEntry historyItem;

	public XUiC_TwitchActionHistoryEntryList Owner;
}
