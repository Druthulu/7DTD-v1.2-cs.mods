using System;
using System.Collections.Generic;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdJunkDrone : ConsoleCmdAbstract
{
	public override bool IsExecuteOnClient
	{
		get
		{
			return true;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[]
		{
			"junkDrone",
			"jd"
		};
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Local player junk drone queries.";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return base.getHelp();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string buildHelpLog()
	{
		return "JunkDrone help:" + Environment.NewLine + "clear - clears data" + Environment.NewLine + "friendlyfire, ff - toggles friendly fire" + Environment.NewLine + "debugcam, dcam - toggles debug camera" + Environment.NewLine + "players - dumps the player owned entity data to the console log" + Environment.NewLine + "man - dumps the manager data to the console log" + Environment.NewLine + "unstuck - triggers teleport to player" + Environment.NewLine + "log - logs out owned entity and manager data" + Environment.NewLine + "assign - spawns unloaded drone from the manager to a player (ex. assign [PlayerName] [DroneEntityId])" + Environment.NewLine;
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (_params.Count > 0 && _params[0].ContainsCaseInsensitive("debuglog"))
		{
			DroneManager.DebugLogEnabled = !DroneManager.DebugLogEnabled;
			Log.Out("Drone debug log enabled: " + DroneManager.DebugLogEnabled.ToString());
		}
		if (_params.Count == 0)
		{
			EntityPlayerLocal primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
			Log.Out("Player JunkDrones" + Environment.NewLine + this.logPlayerOwnedDrones(primaryPlayer.entityId));
			return;
		}
		if (_params[0].ContainsCaseInsensitive("help"))
		{
			Log.Out(this.buildHelpLog());
			return;
		}
		if (_params[0].ContainsCaseInsensitive("log"))
		{
			Log.Out(this.logStoredData("StoredData"));
		}
		if (_params[0].ContainsCaseInsensitive("players"))
		{
			Log.Out(this.logPlayerOwnedDrones("DronePlayer Log"));
		}
		if (_params[0].ContainsCaseInsensitive("man"))
		{
			Log.Out(this.logDroneManager("DroneManager Log"));
		}
		if (_params[0].ContainsCaseInsensitive("uman"))
		{
			EntityPlayerLocal primaryPlayer2 = GameManager.Instance.World.GetPrimaryPlayer();
			if (primaryPlayer2)
			{
				Log.Out(DroneManager.Instance.LogUnloadedDronesForPlayer(primaryPlayer2));
			}
		}
		if (_params[0].ContainsCaseInsensitive("save"))
		{
			DroneManager.Instance.TriggerSave();
		}
		if (_params[0].ContainsCaseInsensitive("assign"))
		{
			string playerName = _params[1];
			int num = -1;
			if (int.TryParse(_params[2], out num))
			{
				EntityPlayer entityPlayer = GameManager.Instance.World.Players.list.Find((EntityPlayer p) => p.EntityName.ContainsCaseInsensitive(playerName));
				if (entityPlayer)
				{
					if (!EntityDrone.IsValidForLocalPlayer())
					{
						Log.Out("please pick up, or clear the currently deployed drone");
						return;
					}
					if (DroneManager.Instance.AssignUnloadedDrone(entityPlayer, num))
					{
						Log.Out("assigned unloaded drone {0} to player {1}", new object[]
						{
							num,
							entityPlayer.EntityName
						});
					}
					else if (EntityDrone.IsValidForLocalPlayer())
					{
						EntityDrone entityDrone = GameManager.Instance.World.GetEntity(num) as EntityDrone;
						if (entityDrone)
						{
							entityDrone.belongsPlayerId = entityPlayer.entityId;
							entityDrone.Owner = entityPlayer;
							entityPlayer.AddOwnedEntity(entityDrone);
							entityDrone.OwnerID = PlatformManager.InternalLocalUserIdentifier;
							Log.Warning(entityDrone.OwnerID.ReadablePlatformUserIdentifier.ToString());
							Log.Out("assigned drone {0} to player {1}", new object[]
							{
								num,
								entityPlayer.EntityName
							});
						}
						else
						{
							Log.Out("assign drone failed, id is not a drone");
						}
					}
					else
					{
						Log.Out("assign drone failed");
					}
				}
				else
				{
					Log.Out("unknown player name: " + playerName);
				}
			}
			else
			{
				Log.Out("assign drone parse failed");
			}
		}
		if (_params[0].ContainsCaseInsensitive("debug"))
		{
			EntityDrone.DebugModeEnabled = !EntityDrone.DebugModeEnabled;
			OwnedEntityData[] ownedEntityClass = GameManager.Instance.World.GetPrimaryPlayer().GetOwnedEntityClass("entityJunkDrone");
			if (ownedEntityClass.Length != 0)
			{
				(GameManager.Instance.World.GetEntity(ownedEntityClass[0].Id) as EntityDrone).SetDebugCameraEnabled(EntityDrone.DebugModeEnabled);
			}
			Log.Out("drone debug mode enabled: " + EntityDrone.DebugModeEnabled.ToString());
		}
		if (_params[0].ContainsCaseInsensitive("unstuck"))
		{
			OwnedEntityData[] ownedEntityClass2 = GameManager.Instance.World.GetPrimaryPlayer().GetOwnedEntityClass("entityJunkDrone");
			if (ownedEntityClass2.Length != 0)
			{
				EntityDrone entityDrone2 = GameManager.Instance.World.GetEntity(ownedEntityClass2[0].Id) as EntityDrone;
				if (entityDrone2 != null)
				{
					entityDrone2.DebugUnstuck();
					Log.Out("drone unstuck complete");
				}
				else
				{
					Log.Out("drone unstuck failed: " + Environment.NewLine + this.logStoredData("StoredData"));
				}
			}
		}
		if (_params[0].ContainsCaseInsensitive("clear"))
		{
			if (_params.Count > 1)
			{
				List<EntityPlayer> list = GameManager.Instance.World.Players.list;
				EntityPlayer entityPlayer2 = null;
				for (int i = 0; i < list.Count; i++)
				{
					if (list[i].EntityName.ContainsCaseInsensitive(_params[1]))
					{
						entityPlayer2 = list[i];
					}
				}
				if (entityPlayer2)
				{
					OwnedEntityData[] ownedEntityClass3 = entityPlayer2.GetOwnedEntityClass("entityJunkDrone");
					for (int j = 0; j < ownedEntityClass3.Length; j++)
					{
						entityPlayer2.RemoveOwnedEntity(ownedEntityClass3[j]);
					}
					DroneManager.Instance.ClearAllDronesForPlayer(entityPlayer2);
					Log.Out("JunkDrone data cleared for {0}.", new object[]
					{
						entityPlayer2.EntityName
					});
					return;
				}
			}
			else
			{
				EntityPlayerLocal primaryPlayer3 = GameManager.Instance.World.GetPrimaryPlayer();
				DroneManager.Instance.ClearAllDronesForPlayer(primaryPlayer3);
				OwnedEntityData[] ownedEntityClass4 = primaryPlayer3.GetOwnedEntityClass("entityJunkDrone");
				for (int k = 0; k < ownedEntityClass4.Length; k++)
				{
					primaryPlayer3.RemoveOwnedEntity(ownedEntityClass4[k]);
				}
				Log.Out("JunkDrone data cleared.");
			}
			return;
		}
		if (_params[0].ContainsCaseInsensitive("cmu"))
		{
			DroneManager.Instance.ClearAllDronesForPlayer(-1);
		}
		if (_params[0].ContainsCaseInsensitive("friendlyfire") || _params[0] == "ff")
		{
			OwnedEntityData[] ownedEntityClass5 = GameManager.Instance.World.GetPrimaryPlayer().GetOwnedEntityClass("entityJunkDrone");
			if (ownedEntityClass5.Length != 0)
			{
				EntityDrone entityDrone3 = GameManager.Instance.World.GetEntity(ownedEntityClass5[0].Id) as EntityDrone;
				entityDrone3.DebugToggleFriendlyFire();
				Log.Out("JunkDrone friendlyfire {0}", new object[]
				{
					entityDrone3.IsFrendlyFireEnabled
				});
			}
		}
		if (_params[0].ContainsCaseInsensitive("debugcam") || _params[0] == "dcam")
		{
			OwnedEntityData[] ownedEntityClass6 = GameManager.Instance.World.GetPrimaryPlayer().GetOwnedEntityClass("entityJunkDrone");
			if (ownedEntityClass6.Length != 0)
			{
				EntityDrone entityDrone4 = GameManager.Instance.World.GetEntity(ownedEntityClass6[0].Id) as EntityDrone;
				entityDrone4.DebugToggleDebugCamera();
				Log.Out("JunkDrone debugcam {0}", new object[]
				{
					entityDrone4.IsDebugCameraEnabled
				});
			}
		}
		if (_params[0].ContainsCaseInsensitive("debugrecon") || _params[0] == "drc")
		{
			OwnedEntityData[] ownedEntityClass7 = GameManager.Instance.World.GetPrimaryPlayer().GetOwnedEntityClass("entityJunkDrone");
			if (ownedEntityClass7.Length != 0)
			{
				(GameManager.Instance.World.GetEntity(ownedEntityClass7[0].Id) as EntityDrone).Debug_ToggleReconMode();
				Log.Out("JunkDrone debugrecon {0}", new object[]
				{
					DroneManager.Debug_LocalControl
				});
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string logPlayerOwnedDrones(string header)
	{
		string text = header + Environment.NewLine;
		foreach (KeyValuePair<PlatformUserIdentifierAbs, PersistentPlayerData> keyValuePair in GameManager.Instance.GetPersistentPlayerList().Players)
		{
			text += this.logPlayerOwnedDrones(keyValuePair.Value.EntityId);
		}
		return text;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string logPlayerOwnedDrones(int entityId)
	{
		string text = string.Empty;
		int num = 0;
		EntityPlayer entityPlayer = GameManager.Instance.World.GetEntity(entityId) as EntityPlayer;
		if (entityPlayer)
		{
			OwnedEntityData[] ownedEntities = entityPlayer.GetOwnedEntities();
			string text2 = string.Empty;
			foreach (OwnedEntityData ownedEntityData in ownedEntities)
			{
				if (ownedEntityData != null && EntityClass.list[ownedEntityData.ClassId].entityClassName.ContainsCaseInsensitive("entityJunkDrone"))
				{
					text2 = text2 + string.Format("entityId: {0}, classId: {1}, lastKnownPosition: {2}", ownedEntityData.Id, EntityClass.GetEntityClassName(ownedEntityData.ClassId), ownedEntityData.hasLastKnownPosition ? ownedEntityData.LastKnownPosition.ToString() : "none") + Environment.NewLine;
					num++;
				}
			}
			text += string.Format("[{0} - count({1})]" + Environment.NewLine + "{2}", entityPlayer.EntityName, num, text2);
		}
		return text;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string logDroneManager(string header)
	{
		string text = header + Environment.NewLine;
		List<EntityCreationData> dronesList = DroneManager.Instance.GetDronesList();
		for (int i = 0; i < dronesList.Count; i++)
		{
			EntityCreationData entityCreationData = dronesList[i];
			text = text + string.Format("#{0}, id {1}, {2}, {3}, chunk {4}", new object[]
			{
				i,
				entityCreationData.id,
				EntityClass.GetEntityClassName(entityCreationData.entityClass),
				entityCreationData.pos.ToCultureInvariantString(),
				World.toChunkXZ(entityCreationData.pos)
			}) + Environment.NewLine;
		}
		return text;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string logStoredData(string header)
	{
		return (header += Environment.NewLine) + this.logPlayerOwnedDrones("DronePlayer") + Environment.NewLine + this.logDroneManager("DroneManager");
	}
}
