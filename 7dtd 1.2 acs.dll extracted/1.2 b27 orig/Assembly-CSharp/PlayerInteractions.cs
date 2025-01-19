using System;
using System.Collections.Generic;
using System.Linq;
using Platform;

public class PlayerInteractions
{
	public static event PlayerIteractionEvent OnNewPlayerInteraction;

	public void JoinedMultiplayerServer(PersistentPlayerList ppl)
	{
		if (PlatformManager.MultiPlatform.PlayerInteractionsRecorder == null)
		{
			return;
		}
		Log.Out("[PlayerInteractions] JoinedMultplayerServer");
		if (this.playerList != null && ppl != this.playerList)
		{
			this.playerList.RemovePlayerEventHandler(new PersistentPlayerData.PlayerEventHandler(this.OnPersistentPlayerEvent));
			this.playerList = null;
		}
		this.playerList = ppl;
		PlayerInteractions.RecordInteractionForActivePersistentPlayers(this.playerList, PlayerInteractionType.Login);
		this.playerList.AddPlayerEventHandler(new PersistentPlayerData.PlayerEventHandler(this.OnPersistentPlayerEvent));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnPersistentPlayerEvent(PersistentPlayerData ppData, PersistentPlayerData otherPlayer, EnumPersistentPlayerDataReason reason)
	{
		PlayerInteraction playerInteraction = default(PlayerInteraction);
		if (reason != EnumPersistentPlayerDataReason.Login)
		{
			if (reason == EnumPersistentPlayerDataReason.Disconnected)
			{
				Log.Out("[PlayerInteractions] persistent player disconnect");
				playerInteraction = PlayerInteractions.CreateInteraction(ppData, PlayerInteractionType.Disconnect);
				PlatformManager.MultiPlatform.PlayerInteractionsRecorder.RecordPlayerInteraction(playerInteraction);
			}
		}
		else
		{
			Log.Out("[PlayerInteractions] persistent player login");
			playerInteraction = PlayerInteractions.CreateInteraction(ppData, PlayerInteractionType.Login);
			PlatformManager.MultiPlatform.PlayerInteractionsRecorder.RecordPlayerInteraction(playerInteraction);
		}
		PlayerIteractionEvent onNewPlayerInteraction = PlayerInteractions.OnNewPlayerInteraction;
		if (onNewPlayerInteraction == null)
		{
			return;
		}
		onNewPlayerInteraction(playerInteraction);
	}

	public void Shutdown()
	{
		if (this.playerList != null)
		{
			Log.Out("[PlayerInteractions] Shutdown, record disconnect for all currently connected players");
			PlayerInteractions.RecordInteractionForActivePersistentPlayers(this.playerList, PlayerInteractionType.Disconnect);
			PersistentPlayerList persistentPlayerList = this.playerList;
			if (persistentPlayerList != null)
			{
				persistentPlayerList.RemovePlayerEventHandler(new PersistentPlayerData.PlayerEventHandler(this.OnPersistentPlayerEvent));
			}
			this.playerList = null;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void RecordInteractionForActivePersistentPlayers(PersistentPlayerList ppl, PlayerInteractionType interactionType)
	{
		IEnumerable<PlayerInteraction> enumerable = from ppd in ppl.Players.Values.ToList<PersistentPlayerData>()
		where ppd.EntityId != -1
		select PlayerInteractions.CreateInteraction(ppd, interactionType);
		PlatformManager.MultiPlatform.PlayerInteractionsRecorder.RecordPlayerInteractions(enumerable);
		foreach (PlayerInteraction playerInteraction in enumerable)
		{
			PlayerIteractionEvent onNewPlayerInteraction = PlayerInteractions.OnNewPlayerInteraction;
			if (onNewPlayerInteraction != null)
			{
				onNewPlayerInteraction(playerInteraction);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static PlayerInteraction CreateInteraction(PersistentPlayerData ppd, PlayerInteractionType type)
	{
		return new PlayerInteraction(ppd.PlayerData, type);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public PersistentPlayerList playerList;
}
