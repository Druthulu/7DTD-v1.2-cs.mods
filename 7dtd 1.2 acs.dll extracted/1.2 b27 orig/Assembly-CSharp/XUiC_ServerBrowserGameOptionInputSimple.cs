using System;
using System.Globalization;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ServerBrowserGameOptionInputSimple : XUiController
{
	public override void Init()
	{
		base.Init();
		this.gameInfoInt = EnumUtils.Parse<GameInfoInt>(this.viewComponent.ID, false);
		this.value = base.GetChildById("value").GetChildByType<XUiC_TextInput>();
		this.value.OnChangeHandler += this.ControlText_OnChangeHandler;
		XUiController childById = base.GetChildById("comparison");
		childById.OnPress += this.ComparisonLabel_OnPress;
		this.comparisonLabel = (XUiV_Label)childById.ViewComponent;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ComparisonLabel_OnPress(XUiController _sender, int _mouseButton)
	{
		this.currentComparison = this.currentComparison.CycleEnum(XUiC_ServerBrowserGameOptionInputSimple.EComparisonType.Smaller, XUiC_ServerBrowserGameOptionInputSimple.EComparisonType.Larger, false, true);
		switch (this.currentComparison)
		{
		case XUiC_ServerBrowserGameOptionInputSimple.EComparisonType.Smaller:
			this.comparisonLabel.Text = "<";
			break;
		case XUiC_ServerBrowserGameOptionInputSimple.EComparisonType.SmallerEquals:
			this.comparisonLabel.Text = "<=";
			break;
		case XUiC_ServerBrowserGameOptionInputSimple.EComparisonType.Equals:
			this.comparisonLabel.Text = "=";
			break;
		case XUiC_ServerBrowserGameOptionInputSimple.EComparisonType.LargerEquals:
			this.comparisonLabel.Text = ">=";
			break;
		case XUiC_ServerBrowserGameOptionInputSimple.EComparisonType.Larger:
			this.comparisonLabel.Text = ">";
			break;
		}
		if (this.OnValueChanged != null)
		{
			this.OnValueChanged(this);
		}
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
		if (this.value.Text.Length == 0 || this.value.Text == "-")
		{
			return new XUiC_ServersList.UiServerFilter(name, XUiC_ServersList.EnumServerLists.Regular, null, IServerListInterface.ServerFilter.EServerFilterType.Any, 0, 0, false, null);
		}
		int filterVal = StringParsers.ParseSInt32(this.value.Text, 0, -1, NumberStyles.Integer);
		int intMinValue = 0;
		int intMaxValue = 0;
		Func<XUiC_ServersList.ListEntry, bool> func;
		IServerListInterface.ServerFilter.EServerFilterType type;
		switch (this.currentComparison)
		{
		case XUiC_ServerBrowserGameOptionInputSimple.EComparisonType.Smaller:
			func = ((XUiC_ServersList.ListEntry _entry) => _entry.gameServerInfo.GetValue(this.gameInfoInt) < filterVal);
			type = IServerListInterface.ServerFilter.EServerFilterType.IntMax;
			intMaxValue = filterVal - 1;
			break;
		case XUiC_ServerBrowserGameOptionInputSimple.EComparisonType.SmallerEquals:
			func = ((XUiC_ServersList.ListEntry _entry) => _entry.gameServerInfo.GetValue(this.gameInfoInt) <= filterVal);
			type = IServerListInterface.ServerFilter.EServerFilterType.IntMax;
			intMaxValue = filterVal;
			break;
		case XUiC_ServerBrowserGameOptionInputSimple.EComparisonType.Equals:
			func = ((XUiC_ServersList.ListEntry _entry) => _entry.gameServerInfo.GetValue(this.gameInfoInt) == filterVal);
			type = IServerListInterface.ServerFilter.EServerFilterType.IntValue;
			intMinValue = filterVal;
			break;
		case XUiC_ServerBrowserGameOptionInputSimple.EComparisonType.LargerEquals:
			func = ((XUiC_ServersList.ListEntry _entry) => _entry.gameServerInfo.GetValue(this.gameInfoInt) >= filterVal);
			type = IServerListInterface.ServerFilter.EServerFilterType.IntMin;
			intMinValue = filterVal;
			break;
		case XUiC_ServerBrowserGameOptionInputSimple.EComparisonType.Larger:
			func = ((XUiC_ServersList.ListEntry _entry) => _entry.gameServerInfo.GetValue(this.gameInfoInt) > filterVal);
			type = IServerListInterface.ServerFilter.EServerFilterType.IntMin;
			intMinValue = filterVal + 1;
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		return new XUiC_ServersList.UiServerFilter(name, XUiC_ServersList.EnumServerLists.Regular, func, type, intMinValue, intMaxValue, false, null);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label comparisonLabel;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput value;

	[PublicizedFrom(EAccessModifier.Private)]
	public GameInfoInt gameInfoInt;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ServerBrowserGameOptionInputSimple.EComparisonType currentComparison = XUiC_ServerBrowserGameOptionInputSimple.EComparisonType.Equals;

	public Action<XUiC_ServerBrowserGameOptionInputSimple> OnValueChanged;

	[PublicizedFrom(EAccessModifier.Private)]
	public enum EComparisonType
	{
		Smaller,
		SmallerEquals,
		Equals,
		LargerEquals,
		Larger
	}
}
