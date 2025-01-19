using System;
using System.Collections.Generic;
using GameSparks.Api.Messages;
using GameSparks.Api.Responses;
using GameSparks.Core;
using GameSparks.RT;
using UnityEngine;

public class GameSparksRTUnity : MonoBehaviour, IRTSessionListener
{
	public static GameSparksRTUnity Instance
	{
		get
		{
			if (GameSparksRTUnity.instance == null)
			{
				GameSparksRTUnity.instance = new GameObject("GameSparksRTUnity").AddComponent<GameSparksRTUnity>();
				UnityEngine.Object.DontDestroyOnLoad(GameSparksRTUnity.instance.gameObject);
			}
			return GameSparksRTUnity.instance;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			if (GameSparksRTUnity.instance != null && GameSparksRTUnity.instance != value)
			{
				UnityEngine.Object.Destroy(GameSparksRTUnity.instance.gameObject);
			}
			GameSparksRTUnity.instance = value;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Awake()
	{
		GameSparksRTUnity.instance = this;
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
	}

	public void Configure(MatchFoundMessage message, Action<int> OnPlayerConnect, Action<int> OnPlayerDisconnect, Action<bool> OnReady, Action<RTPacket> OnPacket, GSInstance instance = null)
	{
		if (message.Port == null)
		{
			Debug.Log("Response does not contain a port, exiting.");
			return;
		}
		this.Configure(message.Host, message.Port.Value, message.AccessToken, OnPlayerConnect, OnPlayerDisconnect, OnReady, OnPacket, instance);
	}

	public void Configure(FindMatchResponse response, Action<int> OnPlayerConnect, Action<int> OnPlayerDisconnect, Action<bool> OnReady, Action<RTPacket> OnPacket, GSInstance instance = null)
	{
		if (response.Port == null)
		{
			Debug.Log("Response does not contain a port, exiting.");
			return;
		}
		this.Configure(response.Host, response.Port.Value, response.AccessToken, OnPlayerConnect, OnPlayerDisconnect, OnReady, OnPacket, instance);
	}

	public void Configure(string host, int port, string accessToken, Action<int> OnPlayerConnect, Action<int> OnPlayerDisconnect, Action<bool> OnReady, Action<RTPacket> OnPacket, GSInstance instance = null)
	{
		this.m_OnPlayerConnect = OnPlayerConnect;
		this.m_OnPlayerDisconnect = OnPlayerDisconnect;
		this.m_OnReady = OnReady;
		this.m_OnPacket = OnPacket;
		if (this.session != null)
		{
			this.session.Stop();
		}
		this.session = GameSparksRT.SessionBuilder().SetHost(host).SetPort(port).SetConnectToken(accessToken).SetListener(this).SetGSInstance(instance).Build();
	}

	public void Connect()
	{
		if (this.session != null)
		{
			Debug.Log("Starting Session");
			this.session.Start();
			return;
		}
		Debug.Log("Cannot start Session");
	}

	public void Disconnect()
	{
		if (this.session != null)
		{
			this.session.Stop();
		}
	}

	public int? PeerId
	{
		get
		{
			if (this.session != null)
			{
				return this.session.PeerId;
			}
			return null;
		}
	}

	public List<int> ActivePeers
	{
		get
		{
			if (this.session != null)
			{
				return this.session.ActivePeers;
			}
			return null;
		}
	}

	public bool Ready
	{
		get
		{
			return this.session != null && this.session.Ready;
		}
	}

	public int SendData(int opCode, GameSparksRT.DeliveryIntent deliveryIntent, RTData structuredData, params int[] targetPlayers)
	{
		if (this.session != null)
		{
			return this.session.SendRTData(opCode, deliveryIntent, structuredData, targetPlayers);
		}
		return -1;
	}

	public int SendBytes(int opCode, GameSparksRT.DeliveryIntent deliveryIntent, ArraySegment<byte> unstructuredData, params int[] targetPlayers)
	{
		if (this.session != null)
		{
			return this.session.SendBytes(opCode, deliveryIntent, unstructuredData, targetPlayers);
		}
		return -1;
	}

	public int SendRTDataAndBytes(int opCode, GameSparksRT.DeliveryIntent deliveryIntent, ArraySegment<byte> unstructuredData, RTData structuredData, params int[] targetPlayers)
	{
		if (this.session != null)
		{
			return this.session.SendRTDataAndBytes(opCode, deliveryIntent, new ArraySegment<byte>?(unstructuredData), structuredData, targetPlayers);
		}
		return -1;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDestroy()
	{
		this.Disconnect();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		if (this.session != null)
		{
			this.session.Update();
		}
	}

	public void OnPlayerConnect(int peerId)
	{
		if (this.m_OnPlayerConnect != null)
		{
			this.m_OnPlayerConnect(peerId);
		}
	}

	public void OnPlayerDisconnect(int peerId)
	{
		if (this.m_OnPlayerDisconnect != null)
		{
			this.m_OnPlayerDisconnect(peerId);
		}
	}

	public void OnReady(bool ready)
	{
		if (this.m_OnReady != null)
		{
			this.m_OnReady(ready);
		}
	}

	public void OnPacket(RTPacket packet)
	{
		if (this.m_OnPacket != null)
		{
			this.m_OnPacket(packet);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public IRTSession session;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Action<int> m_OnPlayerConnect;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Action<int> m_OnPlayerDisconnect;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Action<bool> m_OnReady;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Action<RTPacket> m_OnPacket;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static GameSparksRTUnity instance;
}
