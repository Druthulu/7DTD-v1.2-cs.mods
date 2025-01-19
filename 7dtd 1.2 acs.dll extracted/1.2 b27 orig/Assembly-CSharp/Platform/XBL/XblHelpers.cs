using System;
using System.Collections.Generic;
using Unity.XGamingRuntime;

namespace Platform.XBL
{
	public static class XblHelpers
	{
		public static event XblHelpers.ErrorDelegate OnError;

		public static bool Succeeded(int _hresult, string _operationFriendlyName, bool _logToConsole = true, bool _printSuccess = false)
		{
			if (XblHelpers.Succeeded(_hresult))
			{
				if (_printSuccess && _logToConsole)
				{
					Log.Out("[XBL] Success: " + _operationFriendlyName);
				}
				return true;
			}
			string text;
			if (!XblHelpers.hresultToFriendlyErrorLookup.TryGetValue(_hresult, out text))
			{
				text = _operationFriendlyName + " failed. Error code: " + HR.NameOf(_hresult);
			}
			if (_logToConsole)
			{
				Log.Error(string.Format("[XBL] Error: 0x{0:X8} - {1}", _hresult, text));
			}
			XblHelpers.ErrorDelegate onError = XblHelpers.OnError;
			if (onError != null)
			{
				onError(_hresult, _operationFriendlyName, text);
			}
			return false;
		}

		[PublicizedFrom(EAccessModifier.Internal)]
		public static bool Succeeded(int _hresult)
		{
			return _hresult >= 0;
		}

		[PublicizedFrom(EAccessModifier.Internal)]
		public static bool Failed(int _hresult)
		{
			return _hresult < 0;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		static XblHelpers()
		{
			XblHelpers.hresultToFriendlyErrorLookup[-2143330041] = "IAP_UNEXPECTED: Does the player you are signed in as have a license for the game? You can get one by downloading your game from the store and purchasing it first. If you can't find your game in the store, have you published it in Partner Center?";
			XblHelpers.hresultToFriendlyErrorLookup[-2015035361] = "Missing Game Config";
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly Dictionary<int, string> hresultToFriendlyErrorLookup = new Dictionary<int, string>();

		public delegate void ErrorDelegate(int _hresult, string _operationFriendlyName, string _errorMessage);
	}
}
