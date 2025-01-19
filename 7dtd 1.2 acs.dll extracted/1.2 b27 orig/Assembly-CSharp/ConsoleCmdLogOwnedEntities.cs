using System;
using System.Collections.Generic;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdLogOwnedEntities : ConsoleCmdAbstract
{
	public override bool IsExecuteOnClient
	{
		get
		{
			return true;
		}
	}

	public override DeviceFlag AllowedDeviceTypes
	{
		get
		{
			return DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5;
		}
	}

	public override DeviceFlag AllowedDeviceTypesClient
	{
		get
		{
			return DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[]
		{
			"playerOwnedEntities",
			"poe"
		};
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Lists player owned entities.";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (_params.Count == 0)
		{
			EntityPlayerLocal primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
			Log.Out("Player Owned Entities" + Environment.NewLine + this.logPlayerOwnedEntities(primaryPlayer.entityId));
			return;
		}
		if (_params[0].ContainsCaseInsensitive("log"))
		{
			Log.Out(this.logPlayerOwnedEntities("Player Owned Entities"));
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string logPlayerOwnedEntities(string header)
	{
		string text = header + Environment.NewLine;
		GameManager instance = GameManager.Instance;
		PersistentPlayerList persistentPlayerList = instance.GetPersistentPlayerList();
		World world = instance.World;
		foreach (KeyValuePair<PlatformUserIdentifierAbs, PersistentPlayerData> keyValuePair in persistentPlayerList.Players)
		{
			text += this.logPlayerOwnedEntities(keyValuePair.Value.EntityId);
		}
		return text;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string logPlayerOwnedEntities(int entityId)
	{
		string text = string.Empty;
		GameManager instance = GameManager.Instance;
		instance.GetPersistentPlayerList();
		EntityPlayer entityPlayer = instance.World.GetEntity(entityId) as EntityPlayer;
		if (entityPlayer)
		{
			OwnedEntityData[] ownedEntities = entityPlayer.GetOwnedEntities();
			string text2 = string.Empty;
			foreach (OwnedEntityData ownedEntityData in ownedEntities)
			{
				text2 = text2 + string.Format("entityId: {0}, classId: {1}, lastKnownPosition: {2}", ownedEntityData.Id, EntityClass.GetEntityClassName(ownedEntityData.ClassId), ownedEntityData.hasLastKnownPosition ? ownedEntityData.LastKnownPosition.ToString() : "none") + Environment.NewLine;
			}
			text += string.Format("[{0} - ({1})]" + Environment.NewLine + "{2}", entityPlayer.EntityName, ownedEntities.Length, text2);
		}
		return text;
	}
}
