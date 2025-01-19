using System;
using System.Collections.Generic;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class CrossplatformAuthorizer : AuthorizerAbs
{
	public override int Order
	{
		get
		{
			return 490;
		}
	}

	public override string AuthorizerName
	{
		get
		{
			return "CrossplatformAuth";
		}
	}

	public override string StateLocalizationKey
	{
		get
		{
			return "authstate_crossplatform";
		}
	}

	public override bool AuthorizerActive
	{
		get
		{
			IPlatform crossplatformPlatform = PlatformManager.CrossplatformPlatform;
			return ((crossplatformPlatform != null) ? crossplatformPlatform.AuthenticationServer : null) != null;
		}
	}

	public override void ServerStart()
	{
		base.ServerStart();
		foreach (KeyValuePair<EPlatformIdentifier, IPlatform> keyValuePair in PlatformManager.ServerPlatforms)
		{
			if (keyValuePair.Value.IsCrossplatform)
			{
				IAuthenticationServer authenticationServer = keyValuePair.Value.AuthenticationServer;
				if (authenticationServer != null)
				{
					authenticationServer.StartServer(new AuthenticationSuccessfulCallbackDelegate(this.authPlayerSteamSuccessfulCallback), new KickPlayerDelegate(this.kickPlayerCallback));
				}
			}
		}
	}

	public override void ServerStop()
	{
		base.ServerStop();
		foreach (KeyValuePair<EPlatformIdentifier, IPlatform> keyValuePair in PlatformManager.ServerPlatforms)
		{
			if (keyValuePair.Value.IsCrossplatform)
			{
				IAuthenticationServer authenticationServer = keyValuePair.Value.AuthenticationServer;
				if (authenticationServer != null)
				{
					authenticationServer.StopServer();
				}
			}
		}
	}

	public override ValueTuple<EAuthorizerSyncResult, GameUtils.KickPlayerData?> Authorize(ClientInfo _clientInfo)
	{
		if (_clientInfo.CrossplatformId == null)
		{
			EAuthorizerSyncResult item = EAuthorizerSyncResult.SyncDeny;
			GameUtils.EKickReason kickReason = GameUtils.EKickReason.WrongCrossPlatform;
			int apiResponseEnum = 0;
			string customReason = EPlatformIdentifier.None.ToStringCached<EPlatformIdentifier>();
			return new ValueTuple<EAuthorizerSyncResult, GameUtils.KickPlayerData?>(item, new GameUtils.KickPlayerData?(new GameUtils.KickPlayerData(kickReason, apiResponseEnum, default(DateTime), customReason)));
		}
		EPlatformIdentifier platformIdentifier = _clientInfo.CrossplatformId.PlatformIdentifier;
		IPlatform platform = PlatformManager.InstanceForPlatformIdentifier(platformIdentifier);
		if (platform == null)
		{
			EAuthorizerSyncResult item2 = EAuthorizerSyncResult.SyncDeny;
			GameUtils.EKickReason kickReason2 = GameUtils.EKickReason.UnsupportedPlatform;
			int apiResponseEnum2 = 0;
			string customReason = platformIdentifier.ToStringCached<EPlatformIdentifier>();
			return new ValueTuple<EAuthorizerSyncResult, GameUtils.KickPlayerData?>(item2, new GameUtils.KickPlayerData?(new GameUtils.KickPlayerData(kickReason2, apiResponseEnum2, default(DateTime), customReason)));
		}
		if (platform.PlatformIdentifier != PlatformManager.CrossplatformPlatform.PlatformIdentifier)
		{
			EAuthorizerSyncResult item3 = EAuthorizerSyncResult.SyncDeny;
			GameUtils.EKickReason kickReason3 = GameUtils.EKickReason.WrongCrossPlatform;
			int apiResponseEnum3 = 0;
			string customReason = platformIdentifier.ToStringCached<EPlatformIdentifier>();
			return new ValueTuple<EAuthorizerSyncResult, GameUtils.KickPlayerData?>(item3, new GameUtils.KickPlayerData?(new GameUtils.KickPlayerData(kickReason3, apiResponseEnum3, default(DateTime), customReason)));
		}
		if (platform.AuthenticationServer == null)
		{
			return new ValueTuple<EAuthorizerSyncResult, GameUtils.KickPlayerData?>(EAuthorizerSyncResult.SyncAllow, null);
		}
		EBeginUserAuthenticationResult ebeginUserAuthenticationResult = platform.AuthenticationServer.AuthenticateUser(_clientInfo);
		if (ebeginUserAuthenticationResult != EBeginUserAuthenticationResult.Ok)
		{
			return new ValueTuple<EAuthorizerSyncResult, GameUtils.KickPlayerData?>(EAuthorizerSyncResult.SyncDeny, new GameUtils.KickPlayerData?(new GameUtils.KickPlayerData(GameUtils.EKickReason.CrossPlatformAuthenticationBeginFailed, (int)ebeginUserAuthenticationResult, default(DateTime), "")));
		}
		return new ValueTuple<EAuthorizerSyncResult, GameUtils.KickPlayerData?>(EAuthorizerSyncResult.WaitAsync, null);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void authPlayerSteamSuccessfulCallback(ClientInfo _clientInfo)
	{
		this.authResponsesHandler.AuthorizationAccepted(this, _clientInfo);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void kickPlayerCallback(ClientInfo _cInfo, GameUtils.KickPlayerData _kickData)
	{
		this.authResponsesHandler.AuthorizationDenied(this, _cInfo, _kickData);
	}

	public override void Disconnect(ClientInfo _clientInfo)
	{
		if (_clientInfo != null)
		{
			PlatformUserIdentifierAbs crossplatformId = _clientInfo.CrossplatformId;
			if (((crossplatformId != null) ? crossplatformId.ReadablePlatformUserIdentifier : null) != null)
			{
				IPlatform platform = PlatformManager.InstanceForPlatformIdentifier(_clientInfo.CrossplatformId.PlatformIdentifier);
				if (platform == null)
				{
					return;
				}
				IAuthenticationServer authenticationServer = platform.AuthenticationServer;
				if (authenticationServer == null)
				{
					return;
				}
				authenticationServer.RemoveUser(_clientInfo);
			}
		}
	}
}
