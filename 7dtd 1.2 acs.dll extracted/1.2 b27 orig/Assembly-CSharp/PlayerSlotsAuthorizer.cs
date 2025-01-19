using System;
using System.Collections.ObjectModel;
using UnityEngine.Scripting;

[Preserve]
public class PlayerSlotsAuthorizer : AuthorizerAbs
{
	public override int Order
	{
		get
		{
			return 80;
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
		AdminTools adminTools = GameManager.Instance.adminTools;
		int num = 0;
		int num2 = 0;
		int @int = GamePrefs.GetInt(EnumGamePrefs.ServerMaxPlayerCount);
		int int2 = GamePrefs.GetInt(EnumGamePrefs.ServerReservedSlots);
		int int3 = GamePrefs.GetInt(EnumGamePrefs.ServerReservedSlotsPermission);
		ReadOnlyCollection<ClientInfo> list = SingletonMonoBehaviour<ConnectionManager>.Instance.Clients.List;
		for (int i = 0; i < list.Count; i++)
		{
			ClientInfo clientInfo = list[i];
			if (clientInfo != _clientInfo)
			{
				if (((adminTools != null) ? adminTools.Users.GetUserPermissionLevel(clientInfo) : 1000) <= int3)
				{
					num++;
				}
				else
				{
					num2++;
				}
			}
		}
		if (!GameManager.IsDedicatedServer)
		{
			num++;
		}
		bool flag = ((adminTools != null) ? adminTools.Users.GetUserPermissionLevel(_clientInfo) : 1000) <= int3;
		bool flag2;
		if (flag)
		{
			flag2 = (num2 + num < @int);
		}
		else
		{
			flag2 = (num2 + num < @int && num2 < @int - int2);
		}
		if (!flag2)
		{
			int int4 = GamePrefs.GetInt(EnumGamePrefs.ServerAdminSlots);
			if (int4 > 0 && num2 + num < @int + int4)
			{
				flag2 = (((adminTools != null) ? adminTools.Users.GetUserPermissionLevel(_clientInfo) : 1000) <= GamePrefs.GetInt(EnumGamePrefs.ServerAdminSlotsPermission));
			}
		}
		if (flag2)
		{
			return new ValueTuple<EAuthorizerSyncResult, GameUtils.KickPlayerData?>(EAuthorizerSyncResult.SyncAllow, null);
		}
		string customReason;
		if (!flag && num2 + num < @int)
		{
			EAuthorizerSyncResult item = EAuthorizerSyncResult.SyncDeny;
			GameUtils.EKickReason kickReason = GameUtils.EKickReason.PlayerLimitExceededNonVIP;
			int apiResponseEnum = 0;
			customReason = (@int - int2).ToString();
			return new ValueTuple<EAuthorizerSyncResult, GameUtils.KickPlayerData?>(item, new GameUtils.KickPlayerData?(new GameUtils.KickPlayerData(kickReason, apiResponseEnum, default(DateTime), customReason)));
		}
		EAuthorizerSyncResult item2 = EAuthorizerSyncResult.SyncDeny;
		GameUtils.EKickReason kickReason2 = GameUtils.EKickReason.PlayerLimitExceeded;
		int apiResponseEnum2 = 0;
		customReason = @int.ToString();
		return new ValueTuple<EAuthorizerSyncResult, GameUtils.KickPlayerData?>(item2, new GameUtils.KickPlayerData?(new GameUtils.KickPlayerData(kickReason2, apiResponseEnum2, default(DateTime), customReason)));
	}
}
