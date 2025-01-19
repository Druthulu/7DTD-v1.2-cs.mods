using System;
using System.Text;
using UnityEngine.Scripting;

[Preserve]
public class LegacyModAuthorizer : AuthorizerAbs
{
	public override int Order
	{
		get
		{
			return 150;
		}
	}

	public override string AuthorizerName
	{
		get
		{
			return "LegacyModAuthorizations";
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
		StringBuilder stringBuilder = new StringBuilder();
		Mod mod = ModEvents.PlayerLogin.Invoke(_clientInfo, _clientInfo.compatibilityVersion, stringBuilder);
		if (mod != null)
		{
			Log.Out("Denying login from mod: " + mod.Name);
			EAuthorizerSyncResult item = EAuthorizerSyncResult.SyncDeny;
			GameUtils.EKickReason kickReason = GameUtils.EKickReason.ModDecision;
			int apiResponseEnum = 0;
			string customReason = stringBuilder.ToString();
			return new ValueTuple<EAuthorizerSyncResult, GameUtils.KickPlayerData?>(item, new GameUtils.KickPlayerData?(new GameUtils.KickPlayerData(kickReason, apiResponseEnum, default(DateTime), customReason)));
		}
		return new ValueTuple<EAuthorizerSyncResult, GameUtils.KickPlayerData?>(EAuthorizerSyncResult.SyncAllow, null);
	}
}
