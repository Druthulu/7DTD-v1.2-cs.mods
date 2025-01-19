using System;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class FriendsAuthorizer : AuthorizerAbs
{
	public override int Order
	{
		get
		{
			return 450;
		}
	}

	public override string AuthorizerName
	{
		get
		{
			return "Friends";
		}
	}

	public override string StateLocalizationKey
	{
		get
		{
			return null;
		}
	}

	public override EPlatformIdentifier PlatformRestriction
	{
		get
		{
			return EPlatformIdentifier.Steam;
		}
	}

	public override bool AuthorizerActive
	{
		get
		{
			return !GameManager.IsDedicatedServer && GamePrefs.GetInt(EnumGamePrefs.ServerVisibility) == 1;
		}
	}

	public override ValueTuple<EAuthorizerSyncResult, GameUtils.KickPlayerData?> Authorize(ClientInfo _clientInfo)
	{
		if (!PlatformManager.NativePlatform.User.IsFriend(_clientInfo.PlatformId))
		{
			return new ValueTuple<EAuthorizerSyncResult, GameUtils.KickPlayerData?>(EAuthorizerSyncResult.SyncDeny, new GameUtils.KickPlayerData?(new GameUtils.KickPlayerData(GameUtils.EKickReason.FriendsOnly, 0, default(DateTime), "")));
		}
		return new ValueTuple<EAuthorizerSyncResult, GameUtils.KickPlayerData?>(EAuthorizerSyncResult.SyncAllow, null);
	}
}
