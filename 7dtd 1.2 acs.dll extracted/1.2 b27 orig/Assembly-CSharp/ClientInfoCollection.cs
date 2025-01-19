using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

public class ClientInfoCollection
{
	public ClientInfoCollection()
	{
		this.List = new ReadOnlyCollection<ClientInfo>(this.list);
	}

	public void Add(ClientInfo _cInfo)
	{
		this.list.Add(_cInfo);
		this.clientNumberMap.Add(_cInfo.ClientNumber, _cInfo);
	}

	public void Clear()
	{
		this.list.Clear();
		this.clientNumberMap.Clear();
		this.entityIdMap.Clear();
		this.litenetPeerMap.Clear();
		this.userIdMap.Clear();
	}

	public bool Contains(ClientInfo _cInfo)
	{
		return this.list.Contains(_cInfo);
	}

	public void Remove(ClientInfo _cInfo)
	{
		this.list.Remove(_cInfo);
		this.clientNumberMap.Remove(_cInfo.ClientNumber);
		this.entityIdMap.Remove(_cInfo.entityId);
		if (_cInfo.litenetPeerConnectId >= 0L)
		{
			this.litenetPeerMap.Remove(_cInfo.litenetPeerConnectId);
		}
		if (_cInfo.PlatformId != null)
		{
			this.userIdMap.Remove(_cInfo.PlatformId);
		}
		if (_cInfo.CrossplatformId != null)
		{
			this.userIdMap.Remove(_cInfo.CrossplatformId);
		}
	}

	public int Count
	{
		get
		{
			return this.list.Count;
		}
	}

	public ClientInfo ForClientNumber(int _clientNumber)
	{
		ClientInfo result;
		if (this.clientNumberMap.TryGetValue(_clientNumber, out result))
		{
			return result;
		}
		return null;
	}

	public ClientInfo ForEntityId(int _entityId)
	{
		ClientInfo result;
		if (this.entityIdMap.TryGetValue(_entityId, out result))
		{
			return result;
		}
		for (int i = 0; i < this.list.Count; i++)
		{
			ClientInfo clientInfo = this.list[i];
			if (clientInfo.entityId == _entityId)
			{
				this.entityIdMap.Add(_entityId, clientInfo);
				return clientInfo;
			}
		}
		return null;
	}

	public ClientInfo ForLiteNetPeer(long _peerConnectId)
	{
		ClientInfo result;
		if (this.litenetPeerMap.TryGetValue(_peerConnectId, out result))
		{
			return result;
		}
		for (int i = 0; i < this.list.Count; i++)
		{
			ClientInfo clientInfo = this.list[i];
			if (clientInfo.litenetPeerConnectId == _peerConnectId)
			{
				this.litenetPeerMap.Add(_peerConnectId, clientInfo);
				return clientInfo;
			}
		}
		return null;
	}

	public ClientInfo ForUserId(PlatformUserIdentifierAbs _userIdentifier)
	{
		ClientInfo result;
		if (this.userIdMap.TryGetValue(_userIdentifier, out result))
		{
			return result;
		}
		for (int i = 0; i < this.list.Count; i++)
		{
			ClientInfo clientInfo = this.list[i];
			if (_userIdentifier.Equals(clientInfo.PlatformId))
			{
				this.userIdMap[_userIdentifier] = clientInfo;
				return clientInfo;
			}
			if (_userIdentifier.Equals(clientInfo.CrossplatformId))
			{
				this.userIdMap[_userIdentifier] = clientInfo;
				return clientInfo;
			}
		}
		return null;
	}

	public ClientInfo GetForPlayerName(string _playerName, bool _ignoreCase = true, bool _ignoreBlanks = false)
	{
		if (_ignoreBlanks)
		{
			_playerName = _playerName.Replace(" ", "");
		}
		for (int i = 0; i < this.list.Count; i++)
		{
			ClientInfo clientInfo = this.list[i];
			string text = clientInfo.playerName ?? string.Empty;
			if (_ignoreBlanks)
			{
				text = text.Replace(" ", "");
			}
			if (string.Equals(text, _playerName, _ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
			{
				return clientInfo;
			}
		}
		return null;
	}

	public ClientInfo GetForNameOrId(string _nameOrId, bool _ignoreCase = true, bool _ignoreBlanks = false)
	{
		int entityId;
		if (int.TryParse(_nameOrId, out entityId))
		{
			ClientInfo clientInfo = this.ForEntityId(entityId);
			if (clientInfo != null)
			{
				return clientInfo;
			}
		}
		PlatformUserIdentifierAbs userIdentifier;
		if (PlatformUserIdentifierAbs.TryFromCombinedString(_nameOrId, out userIdentifier))
		{
			ClientInfo clientInfo2 = this.ForUserId(userIdentifier);
			if (clientInfo2 != null)
			{
				return clientInfo2;
			}
		}
		return this.GetForPlayerName(_nameOrId, _ignoreCase, _ignoreBlanks);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<ClientInfo> list = new List<ClientInfo>();

	public readonly ReadOnlyCollection<ClientInfo> List;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<int, ClientInfo> clientNumberMap = new Dictionary<int, ClientInfo>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<int, ClientInfo> entityIdMap = new Dictionary<int, ClientInfo>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<long, ClientInfo> litenetPeerMap = new Dictionary<long, ClientInfo>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<PlatformUserIdentifierAbs, ClientInfo> userIdMap = new Dictionary<PlatformUserIdentifierAbs, ClientInfo>();
}
