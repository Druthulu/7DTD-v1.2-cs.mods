using System;
using System.Collections.Generic;
using GameSparks.Core;
using UnityEngine;

namespace GameSparks.Platforms
{
	public class WebSocketController : MonoBehaviour
	{
		public string GSName { get; set; }

		[PublicizedFrom(EAccessModifier.Private)]
		public void Awake()
		{
			this.GSName = base.name;
		}

		public void AddWebSocket(IControlledWebSocket socket)
		{
			this.webSockets.Add(socket);
			this.websocketCollectionModified = true;
		}

		public void RemoveWebSocket(IControlledWebSocket socket)
		{
			this.webSockets.Remove(socket);
			this.websocketCollectionModified = true;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public IControlledWebSocket GetSocket(int socketId)
		{
			foreach (IControlledWebSocket controlledWebSocket in this.webSockets)
			{
				if (controlledWebSocket.SocketId == socketId)
				{
					return controlledWebSocket;
				}
			}
			return null;
		}

		public void GSSocketOnOpen(string data)
		{
			IDictionary<string, object> dictionary = (IDictionary<string, object>)GSJson.From(data);
			if (dictionary == null)
			{
				throw new FormatException("parsed json was null. ");
			}
			if (!dictionary.ContainsKey("socketId"))
			{
				throw new FormatException();
			}
			int socketId = Convert.ToInt32(dictionary["socketId"]);
			IControlledWebSocket socket = this.GetSocket(socketId);
			if (socket != null)
			{
				socket.TriggerOnOpen();
			}
		}

		public void GSSocketOnClose(string data)
		{
			int socketId = Convert.ToInt32(((IDictionary<string, object>)GSJson.From(data))["socketId"]);
			IControlledWebSocket socket = this.GetSocket(socketId);
			if (socket != null)
			{
				socket.TriggerOnClose();
			}
		}

		public void GSSocketOnMessage(string data)
		{
			IDictionary<string, object> dictionary = (IDictionary<string, object>)GSJson.From(data);
			int socketId = Convert.ToInt32(dictionary["socketId"]);
			IControlledWebSocket socket = this.GetSocket(socketId);
			if (socket != null)
			{
				socket.TriggerOnMessage((string)dictionary["message"]);
			}
		}

		public void GSSocketOnError(string data)
		{
			IDictionary<string, object> dictionary = (IDictionary<string, object>)GSJson.From(data);
			int socketId = Convert.ToInt32(dictionary["socketId"]);
			string message = (string)dictionary["error"];
			IControlledWebSocket socket = this.GetSocket(socketId);
			if (socket != null)
			{
				socket.TriggerOnError(message);
			}
		}

		public void ServerToClient(string jsonData)
		{
			IDictionary<string, object> dictionary = GSJson.From(jsonData) as IDictionary<string, object>;
			int socketId = int.Parse(dictionary["socketId"].ToString());
			IControlledWebSocket socket = this.GetSocket(socketId);
			if (socket == null)
			{
				return;
			}
			string a = dictionary["functionName"].ToString();
			if (a == "onError")
			{
				socket.TriggerOnError(dictionary["data"].ToString());
				return;
			}
			if (a == "onMessage")
			{
				socket.TriggerOnMessage(dictionary["data"].ToString());
				return;
			}
			if (a == "onOpen")
			{
				socket.TriggerOnOpen();
				return;
			}
			if (!(a == "onClose"))
			{
				return;
			}
			socket.TriggerOnClose();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void Update()
		{
			this.websocketCollectionModified = false;
			foreach (IControlledWebSocket controlledWebSocket in this.webSockets)
			{
				controlledWebSocket.Update();
				if (this.websocketCollectionModified)
				{
					break;
				}
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		[NonSerialized]
		public List<IControlledWebSocket> webSockets = new List<IControlledWebSocket>();

		[PublicizedFrom(EAccessModifier.Private)]
		[NonSerialized]
		public bool websocketCollectionModified;
	}
}
