using System;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class TooManyPlayerSlotsAuthorizer : AuthorizerAbs
{
	public override int Order
	{
		get
		{
			return 81;
		}
	}

	public override string AuthorizerName
	{
		get
		{
			return "PlayerSlots";
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
		int @int = GamePrefs.GetInt(EnumGamePrefs.ServerMaxPlayerCount);
		bool flag = true;
		EPlayGroup eplayGroup = _clientInfo.device.ToPlayGroup();
		if (eplayGroup == EPlayGroup.XBS || eplayGroup == EPlayGroup.PS5)
		{
			flag = (@int <= 8);
		}
		if (!flag)
		{
			EAuthorizerSyncResult item = EAuthorizerSyncResult.SyncDeny;
			GameUtils.EKickReason kickReason = GameUtils.EKickReason.PlatformPlayerLimitExceeded;
			int apiResponseEnum = 0;
			string customReason = 8.ToString();
			return new ValueTuple<EAuthorizerSyncResult, GameUtils.KickPlayerData?>(item, new GameUtils.KickPlayerData?(new GameUtils.KickPlayerData(kickReason, apiResponseEnum, default(DateTime), customReason)));
		}
		return new ValueTuple<EAuthorizerSyncResult, GameUtils.KickPlayerData?>(EAuthorizerSyncResult.SyncAllow, null);
	}
}
