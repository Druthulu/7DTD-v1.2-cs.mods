using System;
using UnityEngine.Scripting;

[Preserve]
public class VersionAuthorizer : AuthorizerAbs
{
	public override int Order
	{
		get
		{
			return 70;
		}
	}

	public override string AuthorizerName
	{
		get
		{
			return "VersionCheck";
		}
	}

	public override string StateLocalizationKey
	{
		get
		{
			return null;
		}
	}

	public override ValueTuple<EAuthorizerSyncResult, GameUtils.KickPlayerData?> Authorize(ClientInfo _clientInfo)
	{
		if (!string.Equals(Constants.cVersionInformation.LongStringNoBuild, _clientInfo.compatibilityVersion, StringComparison.Ordinal))
		{
			EAuthorizerSyncResult item = EAuthorizerSyncResult.SyncDeny;
			GameUtils.EKickReason kickReason = GameUtils.EKickReason.VersionMismatch;
			int apiResponseEnum = 0;
			string longStringNoBuild = Constants.cVersionInformation.LongStringNoBuild;
			return new ValueTuple<EAuthorizerSyncResult, GameUtils.KickPlayerData?>(item, new GameUtils.KickPlayerData?(new GameUtils.KickPlayerData(kickReason, apiResponseEnum, default(DateTime), longStringNoBuild)));
		}
		return new ValueTuple<EAuthorizerSyncResult, GameUtils.KickPlayerData?>(EAuthorizerSyncResult.SyncAllow, null);
	}
}
