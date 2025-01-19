using System;
using UnityEngine.Scripting;

[Preserve]
public class BansAndWhitelistAuthorizer : AuthorizerAbs
{
	public override int Order
	{
		get
		{
			return 500;
		}
	}

	public override string AuthorizerName
	{
		get
		{
			return "BansAndWhitelist";
		}
	}

	public override string StateLocalizationKey
	{
		get
		{
			return null;
		}
	}

	public override bool AuthorizerActive
	{
		get
		{
			return GameManager.Instance.adminTools != null;
		}
	}

	public override ValueTuple<EAuthorizerSyncResult, GameUtils.KickPlayerData?> Authorize(ClientInfo _clientInfo)
	{
		AdminTools adminTools = GameManager.Instance.adminTools;
		DateTime banUntil;
		string customReason;
		if (adminTools.Blacklist.IsBanned(_clientInfo.PlatformId, out banUntil, out customReason))
		{
			return new ValueTuple<EAuthorizerSyncResult, GameUtils.KickPlayerData?>(EAuthorizerSyncResult.SyncDeny, new GameUtils.KickPlayerData?(new GameUtils.KickPlayerData(GameUtils.EKickReason.Banned, 0, banUntil, customReason)));
		}
		DateTime banUntil2;
		string customReason2;
		if (_clientInfo.CrossplatformId != null && adminTools.Blacklist.IsBanned(_clientInfo.CrossplatformId, out banUntil2, out customReason2))
		{
			return new ValueTuple<EAuthorizerSyncResult, GameUtils.KickPlayerData?>(EAuthorizerSyncResult.SyncDeny, new GameUtils.KickPlayerData?(new GameUtils.KickPlayerData(GameUtils.EKickReason.Banned, 0, banUntil2, customReason2)));
		}
		if (adminTools.Whitelist.IsWhiteListEnabled() && !adminTools.Whitelist.IsWhitelisted(_clientInfo) && !adminTools.Users.HasEntry(_clientInfo))
		{
			return new ValueTuple<EAuthorizerSyncResult, GameUtils.KickPlayerData?>(EAuthorizerSyncResult.SyncDeny, new GameUtils.KickPlayerData?(new GameUtils.KickPlayerData(GameUtils.EKickReason.NotOnWhitelist, 0, default(DateTime), "")));
		}
		return new ValueTuple<EAuthorizerSyncResult, GameUtils.KickPlayerData?>(EAuthorizerSyncResult.SyncAllow, null);
	}
}
