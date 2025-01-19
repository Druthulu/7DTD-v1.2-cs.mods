﻿using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using LiteNetLib;
using UnityEngine.Networking;

public class NetworkClientLiteNetLib : INetworkClient
{
	public NetworkClientLiteNetLib(IProtocolManagerProtocolInterface _protoManager)
	{
		this.protoManager = _protoManager;
	}

	public void Update()
	{
	}

	public void LateUpdate()
	{
	}

	public void Connect(GameServerInfo _gsi)
	{
		string value = _gsi.GetValue(GameInfoString.IP);
		int value2 = _gsi.GetValue(GameInfoInt.Port);
		string text = ServerInfoCache.Instance.GetPassword(_gsi);
		if (text == null)
		{
			text = "";
		}
		if (string.IsNullOrEmpty(value))
		{
			Log.Out("NET: Skipping LiteNetLib connection attempt, no IP given");
			this.protoManager.ConnectionFailedEv(Localization.Get("netConnectionFailedNoIp", false));
			return;
		}
		if (_gsi.AllowsCrossplay && !PermissionsManager.IsCrossplayAllowed())
		{
			this.Disconnect();
			this.protoManager.ConnectionFailedEv(Localization.Get("auth_noCrossplay", false));
			return;
		}
		Log.Out("NET: LiteNetLib trying to connect to: " + value + ":" + value2.ToString());
		if (this.client != null)
		{
			this.Disconnect();
		}
		EventBasedNetListener eventBasedNetListener = new EventBasedNetListener();
		this.client = new NetManager(eventBasedNetListener, null);
		NetworkCommonLiteNetLib.InitConfig(this.client);
		eventBasedNetListener.PeerConnectedEvent += delegate(NetPeer _peer)
		{
			Log.Out("NET: LiteNetLib: Connected to server");
			this.serverPeer = _peer;
			this.connected = true;
			this.OnConnectedToServer();
		};
		eventBasedNetListener.PeerDisconnectedEvent += this.OnDisconnectedFromServer;
		eventBasedNetListener.NetworkReceiveEvent += this.NetworkReceiveEvent;
		eventBasedNetListener.NetworkErrorEvent += delegate(IPEndPoint _endpoint, SocketError _code)
		{
			Log.Error("NET: LiteNetLib: Network error: {0}", new object[]
			{
				_code
			});
		};
		this.client.Start();
		this.client.Connect(value, value2 + 2, text);
	}

	public void Disconnect()
	{
		this.connected = false;
		if (this.client != null && this.client.IsRunning)
		{
			this.client.Stop();
		}
		this.client = null;
		this.serverPeer = null;
	}

	public NetworkError SendData(int _channel, ArrayListMP<byte> _data)
	{
		if (this.serverPeer == null)
		{
			Log.Warning("NET: LiteNetLib: SendData requested without active connection");
			return NetworkError.WrongOperation;
		}
		_data[0] = (byte)_channel;
		if (ConnectionManager.VerboseNetLogging)
		{
			Log.Out("Sending data to server: ch={0}, size={1}", new object[]
			{
				_channel,
				_data.Count
			});
		}
		this.serverPeer.Send(_data.Items, 0, _data.Count, DeliveryMethod.ReliableOrdered);
		return NetworkError.Ok;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void NetworkReceiveEvent(NetPeer _peer, NetPacketReader _reader, DeliveryMethod _deliveryMethod)
	{
		if (_reader.AvailableBytes == 0)
		{
			Log.Out("NET: LiteNetLib: Received package with zero size from");
			return;
		}
		int availableBytes = _reader.AvailableBytes;
		byte[] array = MemoryPools.poolByte.Alloc(availableBytes);
		_reader.GetBytes(array, availableBytes);
		if (ConnectionManager.VerboseNetLogging)
		{
			Log.Out("Received data from server: ch={0}, size={1}", new object[]
			{
				array[0],
				availableBytes
			});
		}
		SingletonMonoBehaviour<ConnectionManager>.Instance.Net_DataReceivedClient((int)array[0], array, availableBytes);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void OnConnectedToServer()
	{
		INetConnection[] array = new INetConnection[2];
		for (int i = 0; i < 2; i++)
		{
			array[i] = new NetConnectionSimple(i, null, this, null, 1, 0);
		}
		SingletonMonoBehaviour<ConnectionManager>.Instance.SetConnectionToServer(array);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDisconnectedFromServer(NetPeer _peer, DisconnectInfo _info)
	{
		NetworkCommonLiteNetLib.EAdditionalDisconnectCause additionalDisconnectCause = NetworkCommonLiteNetLib.EAdditionalDisconnectCause.InvalidPassword;
		string arg = null;
		bool hasDisconnectInfo = !_info.AdditionalData.IsNull && _info.AdditionalData.AvailableBytes != 0;
		if (hasDisconnectInfo)
		{
			int availableBytes = _info.AdditionalData.AvailableBytes;
			byte[] array = MemoryPools.poolByte.Alloc(availableBytes);
			_info.AdditionalData.GetBytes(array, availableBytes);
			additionalDisconnectCause = (NetworkCommonLiteNetLib.EAdditionalDisconnectCause)array[0];
			if (((availableBytes >= 2) ? array[1] : 0) > 0)
			{
				arg = Encoding.UTF8.GetString(array, 2, (int)array[1]);
			}
			MemoryPools.poolByte.Free(array);
		}
		DisconnectReason reason = _info.Reason;
		string displayMessage = hasDisconnectInfo ? string.Format(Localization.Get("netLiteNetLibDisconnectReason_" + additionalDisconnectCause.ToStringCached<NetworkCommonLiteNetLib.EAdditionalDisconnectCause>(), false), arg) : Localization.Get("netLiteNetLibDisconnectReason_" + reason.ToStringCached<DisconnectReason>(), false);
		ThreadManager.AddSingleTaskMainThread("DisconnectLiteNetLib", delegate(object _taskInfo)
		{
			if (!this.connected)
			{
				Log.Out("NET: LiteNetLib: Connection failed: {0}", new object[]
				{
					reason.ToStringCached<DisconnectReason>()
				});
				if (reason == DisconnectReason.ConnectionRejected)
				{
					if (additionalDisconnectCause == NetworkCommonLiteNetLib.EAdditionalDisconnectCause.InvalidPassword)
					{
						this.protoManager.InvalidPasswordEv();
					}
					else
					{
						Log.Out("NET: LiteNetLib: Reject cause: {0}", new object[]
						{
							additionalDisconnectCause.ToStringCached<NetworkCommonLiteNetLib.EAdditionalDisconnectCause>()
						});
						this.protoManager.ConnectionFailedEv(displayMessage);
					}
				}
				else
				{
					this.protoManager.ConnectionFailedEv(displayMessage);
				}
			}
			else
			{
				Log.Out("NET: LiteNetLib: Connection closed: " + reason.ToStringCached<DisconnectReason>() + ", add: " + additionalDisconnectCause.ToStringCached<NetworkCommonLiteNetLib.EAdditionalDisconnectCause>());
				if (hasDisconnectInfo && additionalDisconnectCause != NetworkCommonLiteNetLib.EAdditionalDisconnectCause.ClientSideDisconnect)
				{
					Log.Out("NET: LiteNetLib: Cause: {0}", new object[]
					{
						additionalDisconnectCause.ToStringCached<NetworkCommonLiteNetLib.EAdditionalDisconnectCause>()
					});
				}
				if (additionalDisconnectCause != NetworkCommonLiteNetLib.EAdditionalDisconnectCause.ClientSideDisconnect)
				{
					this.protoManager.DisconnectedFromServerEv(displayMessage);
				}
			}
			this.Disconnect();
		}, null);
	}

	public void SetLatencySimulation(bool _enable, int _minLatency, int _maxLatency)
	{
		if (this.client != null)
		{
			this.client.SimulateLatency = _enable;
			this.client.SimulationMinLatency = _minLatency;
			this.client.SimulationMaxLatency = _maxLatency;
		}
	}

	public void SetPacketLossSimulation(bool _enable, int _chance)
	{
		if (this.client != null)
		{
			this.client.SimulatePacketLoss = _enable;
			this.client.SimulationPacketLossChance = _chance;
		}
	}

	public void EnableStatistics()
	{
		if (this.client != null)
		{
			this.client.EnableStatistics = true;
		}
	}

	public void DisableStatistics()
	{
		if (this.client != null)
		{
			this.client.EnableStatistics = false;
		}
	}

	public string PrintNetworkStatistics()
	{
		if (this.client != null)
		{
			return this.client.Statistics.ToString();
		}
		return "No client!";
	}

	public void ResetNetworkStatistics()
	{
		if (this.client != null)
		{
			this.client.Statistics.Reset();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly IProtocolManagerProtocolInterface protoManager;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool connected;

	[PublicizedFrom(EAccessModifier.Private)]
	public NetManager client;

	[PublicizedFrom(EAccessModifier.Private)]
	public NetPeer serverPeer;
}
