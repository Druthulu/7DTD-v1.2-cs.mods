using System;
using System.Globalization;
using UnityEngine.Scripting;

namespace Platform.PSN
{
	[Preserve]
	[UserIdentifierFactory(EPlatformIdentifier.PSN)]
	public class UserIdentifierFactory : AbsUserIdentifierFactory
	{
		public override PlatformUserIdentifierAbs FromId(string _idString)
		{
			Log.Out("[PSN] Creating PSN user identifier from: {0}", new object[]
			{
				_idString
			});
			ulong accountId;
			if (StringParsers.TryParseUInt64(_idString, out accountId, 0, -1, NumberStyles.Integer))
			{
				return new UserIdentifierPSN(accountId);
			}
			Log.Warning("[PSN] Could not parse PSN user from " + _idString);
			return null;
		}
	}
}
