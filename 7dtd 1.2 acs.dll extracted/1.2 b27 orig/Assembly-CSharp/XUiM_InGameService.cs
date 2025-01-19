using System;
using System.Text;

public class XUiM_InGameService : XUiModel
{
	public static string GetServiceStats(XUi _xui, InGameService service)
	{
		if (service == null)
		{
			return "";
		}
		StringBuilder stringBuilder = new StringBuilder();
		if (service.ServiceType == InGameService.InGameServiceTypes.VendingRent)
		{
			TileEntityVendingMachine tileEntityVendingMachine = _xui.Trader.TraderTileEntity as TileEntityVendingMachine;
			stringBuilder.Append(XUiM_InGameService.StringFormatHandler(Localization.Get("xuiCost", false), tileEntityVendingMachine.TraderData.TraderInfo.RentCost));
			stringBuilder.Append(XUiM_InGameService.StringFormatHandler(Localization.Get("xuiGameTime", false), tileEntityVendingMachine.TraderData.TraderInfo.RentTimeInDays, Localization.Get("xuiGameDays", false)));
		}
		return stringBuilder.ToString();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string StringFormatHandler(string title, object value)
	{
		return string.Format("{0}: [REPLACE_COLOR]{1}[-]\n", title, value);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string StringFormatHandler(string title, object value, string units)
	{
		return string.Format("{0}: [REPLACE_COLOR]{1} {2}[-]\n", title, value, units);
	}
}
