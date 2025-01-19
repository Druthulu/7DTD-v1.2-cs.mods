using System;
using System.Globalization;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ServerBrowserGameOptionInputRange : XUiController
{
	public override void Init()
	{
		base.Init();
		this.gameInfoInt = EnumUtils.Parse<GameInfoInt>(this.viewComponent.ID, false);
		this.valuemin = base.GetChildById("valuemin").GetChildByType<XUiC_TextInput>();
		this.valuemin.OnChangeHandler += this.ControlText_OnChangeHandler;
		this.valuemax = base.GetChildById("valuemax").GetChildByType<XUiC_TextInput>();
		this.valuemax.OnChangeHandler += this.ControlText_OnChangeHandler;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ControlText_OnChangeHandler(XUiController _sender, string _text, bool _changeFromCode)
	{
		if (this.OnValueChanged != null)
		{
			this.OnValueChanged(this);
		}
	}

	public XUiC_ServersList.UiServerFilter GetFilter()
	{
		string name = this.gameInfoInt.ToStringCached<GameInfoInt>();
		bool flag = this.valuemin.Text.Length > 0 && this.valuemin.Text != "-";
		bool flag2 = this.valuemax.Text.Length > 0 && this.valuemax.Text != "-";
		Func<XUiC_ServersList.ListEntry, bool> func = null;
		IServerListInterface.ServerFilter.EServerFilterType type = IServerListInterface.ServerFilter.EServerFilterType.Any;
		int filterMin = 0;
		int filterMax = 0;
		if (flag && !flag2)
		{
			filterMin = StringParsers.ParseSInt32(this.valuemin.Text, 0, -1, NumberStyles.Integer);
			func = ((XUiC_ServersList.ListEntry _entry) => _entry.gameServerInfo.GetValue(this.gameInfoInt) >= filterMin);
			type = IServerListInterface.ServerFilter.EServerFilterType.IntMin;
		}
		else if (flag && flag2)
		{
			filterMin = StringParsers.ParseSInt32(this.valuemin.Text, 0, -1, NumberStyles.Integer);
			filterMax = StringParsers.ParseSInt32(this.valuemax.Text, 0, -1, NumberStyles.Integer);
			func = delegate(XUiC_ServersList.ListEntry _entry)
			{
				int value = _entry.gameServerInfo.GetValue(this.gameInfoInt);
				return value >= filterMin && value <= filterMax;
			};
			type = IServerListInterface.ServerFilter.EServerFilterType.IntRange;
		}
		else if (!flag && flag2)
		{
			filterMax = StringParsers.ParseSInt32(this.valuemax.Text, 0, -1, NumberStyles.Integer);
			func = ((XUiC_ServersList.ListEntry _entry) => _entry.gameServerInfo.GetValue(this.gameInfoInt) <= filterMax);
			type = IServerListInterface.ServerFilter.EServerFilterType.IntMax;
		}
		return new XUiC_ServersList.UiServerFilter(name, XUiC_ServersList.EnumServerLists.Regular, func, type, filterMin, filterMax, false, null);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput valuemin;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput valuemax;

	[PublicizedFrom(EAccessModifier.Private)]
	public GameInfoInt gameInfoInt;

	public Action<XUiC_ServerBrowserGameOptionInputRange> OnValueChanged;
}
