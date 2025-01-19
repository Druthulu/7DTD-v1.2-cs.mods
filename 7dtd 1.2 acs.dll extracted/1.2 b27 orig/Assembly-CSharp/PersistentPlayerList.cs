using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class PersistentPlayerList
{
	public void HandlePlayerDetailsUpdate(IPlatformUserData platformUserData, string name)
	{
		PersistentPlayerData persistentPlayerData;
		if (this.Players.TryGetValue(platformUserData.PrimaryId, out persistentPlayerData))
		{
			persistentPlayerData.PlayerName.Update(name, platformUserData.PrimaryId);
		}
	}

	public void PlaceLandProtectionBlock(Vector3i pos, PersistentPlayerData owner)
	{
		PersistentPlayerData persistentPlayerData;
		if (this.m_lpBlockMap.TryGetValue(pos, out persistentPlayerData))
		{
			persistentPlayerData.RemoveLandProtectionBlock(pos);
		}
		owner.AddLandProtectionBlock(pos);
		this.RemoveExtraLandClaims(owner);
		this.m_lpBlockMap[pos] = owner;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RemoveExtraLandClaims(PersistentPlayerData owner)
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			int @int = GameStats.GetInt(EnumGameStats.LandClaimCount);
			int num = owner.LPBlocks.Count - @int;
			for (int i = 0; i < num; i++)
			{
				Vector3i blockPos = owner.LPBlocks[0];
				BlockLandClaim.HandleDeactivateLandClaim(blockPos);
				owner.LPBlocks.RemoveAt(0);
				if (GameManager.Instance.World != null)
				{
					NavObjectManager.Instance.UnRegisterNavObjectByPosition(blockPos.ToVector3(), "land_claim");
					SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageEntityMapMarkerRemove>().Setup(EnumMapObjectType.LandClaim, blockPos.ToVector3()), false, -1, -1, -1, null, 192);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RemoveLandClaimsFromDictionaryForPlayer(PersistentPlayerData owner)
	{
		List<Vector3i> landProtectionBlocks = owner.GetLandProtectionBlocks();
		if (landProtectionBlocks != null)
		{
			for (int i = 0; i < landProtectionBlocks.Count; i++)
			{
				this.m_lpBlockMap.Remove(landProtectionBlocks[i]);
			}
		}
	}

	public void RemoveLandProtectionBlock(Vector3i pos)
	{
		PersistentPlayerData persistentPlayerData;
		if (!this.m_lpBlockMap.TryGetValue(pos, out persistentPlayerData))
		{
			return;
		}
		this.m_lpBlockMap.Remove(pos);
		persistentPlayerData.RemoveLandProtectionBlock(pos);
		if (GameManager.Instance.World != null)
		{
			NavObjectManager.Instance.UnRegisterNavObjectByPosition(pos.ToVector3(), "land_claim");
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageEntityMapMarkerRemove>().Setup(EnumMapObjectType.LandClaim, pos.ToVector3()), false, -1, -1, -1, null, 192);
			}
		}
	}

	public PersistentPlayerData GetLandProtectionBlockOwner(Vector3i pos)
	{
		PersistentPlayerData result;
		this.m_lpBlockMap.TryGetValue(pos, out result);
		return result;
	}

	public void AddPlayerEventHandler(PersistentPlayerData.PlayerEventHandler handler)
	{
		if (this.m_dispatch == null)
		{
			this.m_dispatch = new List<PersistentPlayerData.PlayerEventHandler>();
		}
		this.m_dispatch.Add(handler);
	}

	public void RemovePlayerEventHandler(PersistentPlayerData.PlayerEventHandler handler)
	{
		this.m_dispatch.Remove(handler);
	}

	public void DispatchPlayerEvent(PersistentPlayerData player, PersistentPlayerData otherPlayer, EnumPersistentPlayerDataReason reason)
	{
		if (this.m_dispatch != null)
		{
			for (int i = 0; i < this.m_dispatch.Count; i++)
			{
				this.m_dispatch[i](player, otherPlayer, reason);
			}
		}
	}

	public PersistentPlayerData GetPlayerDataFromEntityID(int _entityId)
	{
		PersistentPlayerData result;
		if (!this.EntityToPlayerMap.TryGetValue(_entityId, out result))
		{
			return null;
		}
		return result;
	}

	public PersistentPlayerData GetPlayerData(PlatformUserIdentifierAbs _userIdentifier)
	{
		if (_userIdentifier == null)
		{
			return null;
		}
		PersistentPlayerData result;
		if (!this.Players.TryGetValue(_userIdentifier, out result))
		{
			return null;
		}
		return result;
	}

	public PersistentPlayerData CreatePlayerData(PlatformUserIdentifierAbs _primaryId, PlatformUserIdentifierAbs _nativeId, string _playerName, EPlayGroup _playGroup)
	{
		PersistentPlayerData persistentPlayerData = new PersistentPlayerData(_primaryId, _nativeId, new AuthoredText(_playerName, _primaryId), _playGroup);
		persistentPlayerData.EntityId = -1;
		persistentPlayerData.LastLogin = DateTime.Now;
		this.Players[_primaryId] = persistentPlayerData;
		return persistentPlayerData;
	}

	public void UnmapPlayer(PlatformUserIdentifierAbs _userIdentifier)
	{
		PersistentPlayerData playerData = this.GetPlayerData(_userIdentifier);
		if (playerData != null && playerData.EntityId != -1)
		{
			this.EntityToPlayerMap.Remove(playerData.EntityId);
			this.PlayerToEntityMap.Remove(_userIdentifier);
			playerData.EntityId = -1;
		}
	}

	public void MapPlayer(PersistentPlayerData ppd)
	{
		if (ppd.EntityId != -1)
		{
			this.EntityToPlayerMap[ppd.EntityId] = ppd;
			this.PlayerToEntityMap[ppd.PrimaryId] = ppd.EntityId;
		}
	}

	public void AutoFixNameCollisions()
	{
		if (this.EntityToPlayerMap.Count == 0)
		{
			return;
		}
		HashSet<string> hashSet = new HashSet<string>();
		foreach (KeyValuePair<PlatformUserIdentifierAbs, PersistentPlayerData> keyValuePair in this.Players)
		{
			hashSet.Add(keyValuePair.Value.PlayerName.AuthoredName.Text);
		}
		foreach (string name in hashSet)
		{
			this.FixNameCollisions(name);
		}
		GameManager.Instance.persistentPlayers.Players.EntryModified += this.NameCollisionEvent;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void NameCollisionEvent(object _sender, DictionaryChangedEventArgs<PlatformUserIdentifierAbs, PersistentPlayerData> _entry)
	{
		GameManager.Instance.persistentPlayers.FixNameCollisions(_entry.Value.PlayerName.AuthoredName.Text);
	}

	public void FixNameCollisions(string _name)
	{
		if (this.EntityToPlayerMap.Count == 0 || _name == null)
		{
			return;
		}
		int num = 0;
		IUserClient user = PlatformManager.MultiPlatform.User;
		string b = (((user != null) ? user.PlatformUserId : null) != null && this.Players.ContainsKey(PlatformManager.MultiPlatform.User.PlatformUserId)) ? this.Players[PlatformManager.MultiPlatform.User.PlatformUserId].PlayerName.AuthoredName.Text : null;
		EPlatformIdentifier platformIdentifier = PlatformManager.NativePlatform.PlatformIdentifier;
		PlatformUserIdentifierAbs platformUserIdentifierAbs = null;
		if (_name == b)
		{
			platformUserIdentifierAbs = PlatformManager.MultiPlatform.User.PlatformUserId;
			num++;
		}
		else
		{
			foreach (KeyValuePair<PlatformUserIdentifierAbs, PersistentPlayerData> keyValuePair in this.Players)
			{
				if (platformIdentifier == keyValuePair.Value.PlatformData.NativeId.PlatformIdentifier && _name == keyValuePair.Value.PlayerName.AuthoredName.Text)
				{
					platformUserIdentifierAbs = keyValuePair.Value.PlatformData.PrimaryId;
					num++;
					break;
				}
			}
		}
		if (platformUserIdentifierAbs != null)
		{
			this.Players[platformUserIdentifierAbs].PlayerName.SetCollisionSuffix(0);
		}
		foreach (KeyValuePair<PlatformUserIdentifierAbs, PersistentPlayerData> keyValuePair2 in this.Players)
		{
			if (!keyValuePair2.Key.Equals(platformUserIdentifierAbs) && this.PlayerToEntityMap.ContainsKey(keyValuePair2.Key) && keyValuePair2.Value.PlayerName.AuthoredName.Text == _name)
			{
				keyValuePair2.Value.PlayerName.SetCollisionSuffix(num++);
			}
		}
		foreach (KeyValuePair<PlatformUserIdentifierAbs, PersistentPlayerData> keyValuePair3 in this.Players)
		{
			if (!keyValuePair3.Key.Equals(platformUserIdentifierAbs) && !this.PlayerToEntityMap.ContainsKey(keyValuePair3.Key) && keyValuePair3.Value.PlayerName.AuthoredName.Text == _name)
			{
				keyValuePair3.Value.PlayerName.SetCollisionSuffix(num++);
			}
		}
	}

	public void SetPlayerData(PersistentPlayerData ppData)
	{
		if (ppData.EntityId == -1)
		{
			this.UnmapPlayer(ppData.PrimaryId);
		}
		this.Players[ppData.PrimaryId] = ppData;
		if (ppData.LPBlocks != null)
		{
			for (int i = 0; i < ppData.LPBlocks.Count; i++)
			{
				Vector3i key = ppData.LPBlocks[i];
				this.m_lpBlockMap[key] = ppData;
			}
		}
		this.MapPlayer(ppData);
	}

	public PersistentPlayerList NetworkCloneRelevantForPlayer()
	{
		PersistentPlayerList persistentPlayerList = new PersistentPlayerList();
		foreach (KeyValuePair<PlatformUserIdentifierAbs, PersistentPlayerData> keyValuePair in this.Players)
		{
			persistentPlayerList.Players[keyValuePair.Value.PrimaryId] = keyValuePair.Value;
		}
		foreach (KeyValuePair<PlatformUserIdentifierAbs, PersistentPlayerData> keyValuePair2 in persistentPlayerList.Players)
		{
			if (keyValuePair2.Value.LPBlocks != null)
			{
				for (int i = 0; i < keyValuePair2.Value.LPBlocks.Count; i++)
				{
					Vector3i key = keyValuePair2.Value.LPBlocks[i];
					persistentPlayerList.m_lpBlockMap[key] = keyValuePair2.Value;
				}
			}
		}
		return persistentPlayerList;
	}

	public bool CleanupPlayers()
	{
		List<PersistentPlayerData> list = null;
		double num = (double)GameStats.GetInt(EnumGameStats.LandClaimExpiryTime) * 24.0;
		DateTime now = DateTime.Now;
		foreach (KeyValuePair<PlatformUserIdentifierAbs, PersistentPlayerData> keyValuePair in this.Players)
		{
			if ((keyValuePair.Value.ACL == null || keyValuePair.Value.ACL.Count == 0) && !keyValuePair.Value.HasBedrollPos && keyValuePair.Value.EntityId == -1 && (now - keyValuePair.Value.LastLogin).TotalHours > num)
			{
				if (list == null)
				{
					list = new List<PersistentPlayerData>();
				}
				list.Add(keyValuePair.Value);
			}
		}
		if (list != null)
		{
			for (int i = 0; i < list.Count; i++)
			{
				PersistentPlayerData persistentPlayerData = list[i];
				if (persistentPlayerData.LPBlocks != null)
				{
					for (int j = 0; j < persistentPlayerData.LPBlocks.Count; j++)
					{
						Vector3i key = persistentPlayerData.LPBlocks[j];
						this.m_lpBlockMap.Remove(key);
					}
				}
				this.Players.Remove(persistentPlayerData.PrimaryId);
			}
		}
		return list != null;
	}

	public void SpawnPointRemoved(Vector3i _pos)
	{
		foreach (KeyValuePair<PlatformUserIdentifierAbs, PersistentPlayerData> keyValuePair in this.Players)
		{
			if (keyValuePair.Value.BedrollPos.Equals(_pos))
			{
				keyValuePair.Value.ClearBedroll();
				break;
			}
		}
	}

	public void Write(BinaryWriter stream)
	{
		stream.Write(this.Players.Count);
		foreach (KeyValuePair<PlatformUserIdentifierAbs, PersistentPlayerData> keyValuePair in this.Players)
		{
			keyValuePair.Value.Write(stream);
		}
		stream.Write(this.m_lpBlockMap.Count);
		foreach (KeyValuePair<Vector3i, PersistentPlayerData> keyValuePair2 in this.m_lpBlockMap)
		{
			stream.Write(keyValuePair2.Key.x);
			stream.Write(keyValuePair2.Key.y);
			stream.Write(keyValuePair2.Key.z);
			keyValuePair2.Value.PrimaryId.ToStream(stream, false);
		}
	}

	public static PersistentPlayerList Read(BinaryReader stream)
	{
		PersistentPlayerList persistentPlayerList = new PersistentPlayerList();
		int num = stream.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			PersistentPlayerData persistentPlayerData = PersistentPlayerData.Read(stream);
			persistentPlayerList.Players.Add(persistentPlayerData.PrimaryId, persistentPlayerData);
			persistentPlayerList.MapPlayer(persistentPlayerData);
		}
		int num2 = stream.ReadInt32();
		for (int j = 0; j < num2; j++)
		{
			Vector3i key = new Vector3i(stream.ReadInt32(), stream.ReadInt32(), stream.ReadInt32());
			PlatformUserIdentifierAbs key2 = PlatformUserIdentifierAbs.FromStream(stream, false, false);
			PersistentPlayerData persistentPlayerData2;
			if (persistentPlayerList.Players.TryGetValue(key2, out persistentPlayerData2) && persistentPlayerData2 != null)
			{
				persistentPlayerList.m_lpBlockMap[key] = persistentPlayerData2;
			}
		}
		return persistentPlayerList;
	}

	public static PersistentPlayerList ReadXML(string filePath)
	{
		Log.Out("Loading players.xml");
		PersistentPlayerList persistentPlayerList = new PersistentPlayerList();
		if (!SdFile.Exists(filePath))
		{
			return persistentPlayerList;
		}
		XmlDocument xmlDocument = new XmlDocument();
		try
		{
			xmlDocument.SdLoad(filePath);
		}
		catch (XmlException ex)
		{
			Log.Error(string.Format("Failed loading players.xml: {0}", ex.Message));
			return persistentPlayerList;
		}
		if (xmlDocument.DocumentElement == null)
		{
			throw new Exception("malformed persistent player data xml file!");
		}
		foreach (object obj in xmlDocument.DocumentElement.ChildNodes)
		{
			XmlNode xmlNode = (XmlNode)obj;
			if (xmlNode.NodeType == XmlNodeType.Element && xmlNode.Name == "player")
			{
				PersistentPlayerData persistentPlayerData = PersistentPlayerData.ReadXML(xmlNode as XmlElement);
				if (persistentPlayerData == null)
				{
					return null;
				}
				persistentPlayerList.Players.Add(persistentPlayerData.PrimaryId, persistentPlayerData);
				if (persistentPlayerData.LPBlocks != null)
				{
					for (int i = 0; i < persistentPlayerData.LPBlocks.Count; i++)
					{
						Vector3i key = persistentPlayerData.LPBlocks[i];
						persistentPlayerList.m_lpBlockMap[key] = persistentPlayerData;
					}
				}
			}
		}
		return persistentPlayerList;
	}

	public void Write(string filePath)
	{
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.CreateXmlDeclaration();
		XmlElement root = xmlDocument.AddXmlElement("persistentplayerdata");
		foreach (KeyValuePair<PlatformUserIdentifierAbs, PersistentPlayerData> keyValuePair in this.Players)
		{
			keyValuePair.Value.Write(root);
		}
		xmlDocument.SdSave(filePath);
	}

	public void Destroy()
	{
		PlatformUserManager.DetailsUpdated -= this.HandlePlayerDetailsUpdate;
	}

	public ObservableDictionary<PlatformUserIdentifierAbs, PersistentPlayerData> Players = new ObservableDictionary<PlatformUserIdentifierAbs, PersistentPlayerData>();

	public Dictionary<int, PersistentPlayerData> EntityToPlayerMap = new Dictionary<int, PersistentPlayerData>();

	public Dictionary<PlatformUserIdentifierAbs, int> PlayerToEntityMap = new Dictionary<PlatformUserIdentifierAbs, int>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<PersistentPlayerData.PlayerEventHandler> m_dispatch;

	public Dictionary<Vector3i, PersistentPlayerData> m_lpBlockMap = new Dictionary<Vector3i, PersistentPlayerData>();
}
