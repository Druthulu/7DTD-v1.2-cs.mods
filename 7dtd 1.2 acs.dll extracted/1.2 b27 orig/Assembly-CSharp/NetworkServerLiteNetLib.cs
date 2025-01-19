using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using LiteNetLib;
using UnityEngine.Networking;

public class NetworkServerLiteNetLib : INetworkServer
{
	public NetworkServerLiteNetLib(IProtocolManagerProtocolInterface _protoManager)
	{
		this.protoManager = _protoManager;
	}

	public void Update()
	{
		ClientInfo clientInfo;
		do
		{
			clientInfo = null;
			List<ClientInfo> obj = this.dropClientsQueue;
			lock (obj)
			{
				int num = this.dropClientsQueue.Count - 1;
				if (num >= 0)
				{
					clientInfo = this.dropClientsQueue[num];
					this.dropClientsQueue.RemoveAt(num);
				}
			}
			if (clientInfo != null)
			{
				this.DropClient(clientInfo, false);
			}
		}
		while (clientInfo != null);
	}

	public void LateUpdate()
	{
	}

	public NetworkConnectionError StartServer(int _basePort, string _password)
	{
		this.serverPassword = (string.IsNullOrEmpty(_password) ? "" : _password);
		EventBasedNetListener eventBasedNetListener = new EventBasedNetListener();
		this.server = new NetManager(eventBasedNetListener, null);
		NetworkCommonLiteNetLib.InitConfig(this.server);
		eventBasedNetListener.ConnectionRequestEvent += this.ConnectionRequestCheck;
		eventBasedNetListener.PeerConnectedEvent += delegate(NetPeer _peer)
		{
			Log.Out(string.Format("NET: LiteNetLib: Connect from: {0} / {1}", _peer.EndPoint, _peer.Id));
			this.OnPlayerConnected(_peer);
		};
		eventBasedNetListener.PeerDisconnectedEvent += delegate(NetPeer _peer, DisconnectInfo _info)
		{
			Log.Out(string.Format("NET: LiteNetLib: Client disconnect from: {0} / {1} ({2})", _peer.EndPoint, _peer.Id, _info.Reason.ToStringCached<DisconnectReason>()));
			if (_info.Reason == DisconnectReason.Timeout)
			{
				Log.Out(string.Format("NET: LiteNetLib: TimeSinceLastPacket: {0}", _peer.TimeSinceLastPacket));
			}
			ThreadManager.AddSingleTaskMainThread("PlayerDisconnectLiteNetLib", delegate(object _taskInfo)
			{
				Log.Out(string.Format("NET: LiteNetLib: MT: Client disconnect from: {0} / {1} ({2})", _peer.EndPoint, _peer.Id, _info.Reason.ToStringCached<DisconnectReason>()));
				this.OnPlayerDisconnected((long)_peer.Id);
			}, null);
		};
		eventBasedNetListener.NetworkReceiveEvent += this.NetworkReceiveEvent;
		eventBasedNetListener.NetworkErrorEvent += delegate(IPEndPoint _endpoint, SocketError _code)
		{
			Log.Error("NET: LiteNetLib: Network error: {0}", new object[]
			{
				_code
			});
		};
		if (this.server.Start(_basePort + 2))
		{
			Log.Out("NET: LiteNetLib server started");
			return NetworkConnectionError.NoError;
		}
		Log.Out("NET: LiteNetLib server could not be started");
		return NetworkConnectionError.CreateSocketOrThreadFailure;
	}

	public void SetServerPassword(string _password)
	{
		this.serverPassword = (_password ?? "");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ConnectionRequestCheck(ConnectionRequest _request)
	{
		string text = _request.RemoteEndPoint.Address.ToString();
		DateTime now = DateTime.Now;
		DateTime d;
		this.lastConnectAttemptTimes.TryGetValue(text, out d);
		TimeSpan timeSpan = now - d;
		this.lastConnectAttemptTimes[text] = now;
		if (timeSpan.TotalMilliseconds < 500.0)
		{
			Log.Out("NET: Rejecting connection request from " + text + ": Limiting connect rate from that IP!");
			_request.Reject(NetworkServerLiteNetLib.rejectRateLimit);
			return;
		}
		foreach (ClientInfo clientInfo in SingletonMonoBehaviour<ConnectionManager>.Instance.Clients.List)
		{
			if (!clientInfo.loginDone && clientInfo.ip == text)
			{
				Log.Out("NET: Rejecting connection request from " + text + ": A connection attempt from that IP is currently being processed!");
				_request.Reject(NetworkServerLiteNetLib.rejectPendingConnection);
				return;
			}
		}
		if (_request.Data.GetString() != this.serverPassword)
		{
			_request.Reject(NetworkServerLiteNetLib.rejectInvalidPassword);
			return;
		}
		_request.Accept();
	}

	public void StopServer()
	{
		NetManager netManager = this.server;
		if (netManager != null && netManager.IsRunning)
		{
			List<NetPeer> list = new List<NetPeer>();
			this.server.GetPeersNonAlloc(list, ConnectionState.Any);
			for (int i = 0; i < list.Count; i++)
			{
				this.server.DisconnectPeer(list[i], NetworkServerLiteNetLib.disconnectServerShutdown);
			}
			this.server.Stop();
		}
		Log.Out("NET: LiteNetLib server stopped");
	}

	public void DropClient(ClientInfo _clientInfo, bool _clientDisconnect)
	{
		this.OnPlayerDisconnected(_clientInfo.litenetPeerConnectId);
		NetPeer peerByConnectId = this.GetPeerByConnectId(_clientInfo.litenetPeerConnectId);
		if (peerByConnectId != null)
		{
			this.server.DisconnectPeer(peerByConnectId, NetworkServerLiteNetLib.disconnectFromClientSide);
		}
	}

	public NetworkError SendData(ClientInfo _cInfo, int _channel, ArrayListMP<byte> _data, bool reliableDelivery = true)
	{
		NetPeer peerByConnectId = this.GetPeerByConnectId(_cInfo.litenetPeerConnectId);
		if (peerByConnectId == null)
		{
			Log.Warning("NET: LiteNetLib: SendData requested for unknown client {0}", new object[]
			{
				_cInfo.ToString()
			});
			List<ClientInfo> obj = this.dropClientsQueue;
			lock (obj)
			{
				if (!this.dropClientsQueue.Contains(_cInfo))
				{
					this.dropClientsQueue.Add(_cInfo);
				}
			}
			return NetworkError.WrongConnection;
		}
		_data[0] = (byte)_channel;
		if (ConnectionManager.VerboseNetLogging)
		{
			Log.Out("Sending data to peer {2}: ch={0}, size={1}", new object[]
			{
				_channel,
				_data.Count,
				_cInfo.InternalId.CombinedString
			});
		}
		peerByConnectId.Send(_data.Items, 0, _data.Count, reliableDelivery ? DeliveryMethod.ReliableOrdered : DeliveryMethod.Unreliable);
		return NetworkError.Ok;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void NetworkReceiveEvent(NetPeer _peer, NetPacketReader _reader, DeliveryMethod _deliveryMethod)
	{
		ClientInfo clientInfo = SingletonMonoBehaviour<ConnectionManager>.Instance.Clients.ForLiteNetPeer((long)_peer.Id);
		if (clientInfo == null)
		{
			string str = "NET: LiteNetLib: Received package from an unknown client: ";
			IPEndPoint endPoint = _peer.EndPoint;
			Log.Out(str + ((endPoint != null) ? endPoint.ToString() : null));
			return;
		}
		if (_reader.AvailableBytes == 0)
		{
			string str2 = "NET: LiteNetLib: Received package with zero size from: ";
			ClientInfo clientInfo2 = clientInfo;
			Log.Out(str2 + ((clientInfo2 != null) ? clientInfo2.ToString() : null));
			return;
		}
		int availableBytes = _reader.AvailableBytes;
		byte[] array = MemoryPools.poolByte.Alloc(availableBytes);
		_reader.GetBytes(array, availableBytes);
		if (ConnectionManager.VerboseNetLogging)
		{
			Log.Out("Received data from peer {2}: ch={0}, size={1}", new object[]
			{
				array[0],
				availableBytes,
				clientInfo.InternalId.CombinedString
			});
		}
		SingletonMonoBehaviour<ConnectionManager>.Instance.Net_DataReceivedServer(clientInfo, (int)array[0], array, availableBytes);
	}

	public string GetIP(ClientInfo _cInfo)
	{
		NetPeer peerByConnectId = this.GetPeerByConnectId(_cInfo.litenetPeerConnectId);
		if (peerByConnectId == null)
		{
			Log.Warning("NET: LiteNetLib: IP requested for unknown client {0}", new object[]
			{
				_cInfo.ToString()
			});
			return string.Empty;
		}
		return peerByConnectId.EndPoint.Address.ToString();
	}

	public int GetPing(ClientInfo _cInfo)
	{
		NetPeer peerByConnectId = this.GetPeerByConnectId(_cInfo.litenetPeerConnectId);
		if (peerByConnectId == null)
		{
			Log.Warning("NET: LiteNetLib: Ping requested for unknown client {0}", new object[]
			{
				_cInfo.ToString()
			});
			return -1;
		}
		return peerByConnectId.Ping;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnPlayerConnected(NetPeer _peer)
	{
		ClientInfo clientInfo = new ClientInfo
		{
			litenetPeerConnectId = (long)_peer.Id,
			network = this,
			netConnection = new INetConnection[2]
		};
		for (int i = 0; i < 2; i++)
		{
			clientInfo.netConnection[i] = new NetConnectionSimple(i, clientInfo, null, _peer.Id.ToString(), 1, 0);
		}
		SingletonMonoBehaviour<ConnectionManager>.Instance.AddClient(clientInfo);
		SingletonMonoBehaviour<ConnectionManager>.Instance.Net_PlayerConnected(clientInfo);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnPlayerDisconnected(long _peerConnectId)
	{
		ClientInfo clientInfo = SingletonMonoBehaviour<ConnectionManager>.Instance.Clients.ForLiteNetPeer(_peerConnectId);
		if (clientInfo != null)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.Net_PlayerDisconnected(clientInfo);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public NetPeer GetPeerByConnectId(long _connectId)
	{
		List<NetPeer> obj = this.getPeersList;
		lock (obj)
		{
			this.server.GetPeersNonAlloc(this.getPeersList, ConnectionState.Any);
			for (int i = 0; i < this.getPeersList.Count; i++)
			{
				if ((long)this.getPeersList[i].Id == _connectId)
				{
					return this.getPeersList[i];
				}
			}
		}
		return null;
	}

	public int GetBadPacketCount(ClientInfo _cInfo)
	{
		NetPeer peerByConnectId = this.GetPeerByConnectId(_cInfo.litenetPeerConnectId);
		if (peerByConnectId != null)
		{
			return peerByConnectId.badPacketCount;
		}
		return 0;
	}

	public string GetServerPorts(int _basePort)
	{
		return (_basePort + 2).ToString() + "/UDP";
	}

	public void SetLatencySimulation(bool _enable, int _minLatency, int _maxLatency)
	{
		if (this.server != null)
		{
			this.server.SimulateLatency = _enable;
			this.server.SimulationMinLatency = _minLatency;
			this.server.SimulationMaxLatency = _maxLatency;
		}
	}

	public void SetPacketLossSimulation(bool _enable, int _chance)
	{
		if (this.server != null)
		{
			this.server.SimulatePacketLoss = _enable;
			this.server.SimulationPacketLossChance = _chance;
		}
	}

	public void EnableStatistics()
	{
		if (this.server != null)
		{
			this.server.EnableStatistics = true;
		}
	}

	public void DisableStatistics()
	{
		if (this.server != null)
		{
			this.server.EnableStatistics = false;
		}
	}

	public string PrintNetworkStatistics()
	{
		if (this.server != null)
		{
			return this.server.Statistics.ToString();
		}
		return "no server!";
	}

	public void ResetNetworkStatistics()
	{
		if (this.server != null)
		{
			this.server.Statistics.Reset();
		}
	}

	public int GetMaximumPacketSize(ClientInfo _cInfo, bool reliable = false)
	{
		int result = -1;
		NetPeer peerByConnectId = this.GetPeerByConnectId(_cInfo.litenetPeerConnectId);
		if (peerByConnectId != null)
		{
			result = peerByConnectId.GetMaxSinglePacketSize(reliable ? DeliveryMethod.ReliableOrdered : DeliveryMethod.Unreliable);
		}
		return result;
	}

	// Note: this type is marked as 'beforefieldinit'.
	[PublicizedFrom(EAccessModifier.Private)]
	static NetworkServerLiteNetLib()
	{
		byte[] array = new byte[2];
		array[0] = 1;
		NetworkServerLiteNetLib.rejectRateLimit = array;
		byte[] array2 = new byte[2];
		array2[0] = 2;
		NetworkServerLiteNetLib.rejectPendingConnection = array2;
		byte[] array3 = new byte[2];
		array3[0] = 3;
		NetworkServerLiteNetLib.disconnectServerShutdown = array3;
		byte[] array4 = new byte[2];
		array4[0] = 4;
		NetworkServerLiteNetLib.disconnectFromClientSide = array4;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const int ConnectionRateLimitMilliseconds = 500;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] rejectInvalidPassword = new byte[2];

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] rejectRateLimit;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] rejectPendingConnection;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] disconnectServerShutdown;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly byte[] disconnectFromClientSide;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly IProtocolManagerProtocolInterface protoManager;

	[PublicizedFrom(EAccessModifier.Private)]
	public string serverPassword;

	[PublicizedFrom(EAccessModifier.Private)]
	public NetManager server;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<string, DateTime> lastConnectAttemptTimes = new Dictionary<string, DateTime>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<ClientInfo> dropClientsQueue = new List<ClientInfo>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<NetPeer> getPeersList = new List<NetPeer>();
}
