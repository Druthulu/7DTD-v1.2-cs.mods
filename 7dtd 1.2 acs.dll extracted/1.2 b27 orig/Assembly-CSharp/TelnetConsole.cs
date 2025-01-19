using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class TelnetConsole : IConsoleServer
{
	public TelnetConsole()
	{
		try
		{
			int @int = GamePrefs.GetInt(EnumGamePrefs.TelnetPort);
			this.authEnabled = (GamePrefs.GetString(EnumGamePrefs.TelnetPassword).Length != 0);
			this.listener = new TcpListener(this.authEnabled ? IPAddress.Any : IPAddress.Loopback, @int);
			TelnetConsole.maxLoginAttempts = GamePrefs.GetInt(EnumGamePrefs.TelnetFailedLoginLimit);
			TelnetConsole.blockTimeSeconds = GamePrefs.GetInt(EnumGamePrefs.TelnetFailedLoginsBlocktime);
			this.listener.Start();
			this.listener.BeginAcceptTcpClient(new AsyncCallback(this.AcceptClient), null);
			Log.Out("Started Telnet on " + @int.ToString());
		}
		catch (Exception ex)
		{
			string str = "Error in Telnet.ctor: ";
			Exception ex2 = ex;
			Log.Out(str + ((ex2 != null) ? ex2.ToString() : null));
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void AcceptClient(IAsyncResult _asyncResult)
	{
		TcpListener tcpListener = this.listener;
		if (((tcpListener != null) ? tcpListener.Server : null) == null || !this.listener.Server.IsBound)
		{
			return;
		}
		TcpClient tcpClient = this.listener.EndAcceptTcpClient(_asyncResult);
		EndPoint remoteEndPoint = tcpClient.Client.RemoteEndPoint;
		IPEndPoint ipendPoint = remoteEndPoint as IPEndPoint;
		int hashCode;
		if (ipendPoint != null)
		{
			hashCode = ipendPoint.Address.GetHashCode();
		}
		else
		{
			hashCode = remoteEndPoint.GetHashCode();
			string str = "EndPoint is not an IPEndPoint but: ";
			Type type = remoteEndPoint.GetType();
			Log.Out(str + ((type != null) ? type.ToString() : null));
		}
		Dictionary<int, TelnetConsole.LoginAttempts> obj = this.loginAttemptsPerIP;
		lock (obj)
		{
			TelnetConsole.LoginAttempts loginAttempts;
			if (!this.loginAttemptsPerIP.TryGetValue(hashCode, out loginAttempts))
			{
				loginAttempts = new TelnetConsole.LoginAttempts();
				this.loginAttemptsPerIP[hashCode] = loginAttempts;
			}
			if (!loginAttempts.IsBanned())
			{
				TelnetConnection item = new TelnetConnection(this, tcpClient, hashCode, this.authEnabled);
				List<TelnetConnection> obj2 = this.connections;
				lock (obj2)
				{
					this.connections.Add(item);
					goto IL_131;
				}
			}
			tcpClient.Close();
			string str2 = "Telnet connection not accepted for too many login attempts: ";
			EndPoint endPoint = remoteEndPoint;
			Log.Out(str2 + ((endPoint != null) ? endPoint.ToString() : null));
		}
		IL_131:
		this.listener.BeginAcceptTcpClient(new AsyncCallback(this.AcceptClient), null);
	}

	public bool RegisterFailedLogin(TelnetConnection _con)
	{
		Dictionary<int, TelnetConsole.LoginAttempts> obj = this.loginAttemptsPerIP;
		bool result;
		lock (obj)
		{
			result = this.loginAttemptsPerIP[_con.EndPointHash].LogAttempt();
		}
		return result;
	}

	public void ConnectionClosed(TelnetConnection _con)
	{
		List<TelnetConnection> obj = this.connections;
		lock (obj)
		{
			this.connections.Remove(_con);
		}
	}

	public void Disconnect()
	{
		try
		{
			if (this.listener != null)
			{
				this.listener.Stop();
				this.listener = null;
			}
			List<TelnetConnection> obj = this.connections;
			List<TelnetConnection> list;
			lock (obj)
			{
				list = new List<TelnetConnection>(this.connections);
			}
			foreach (TelnetConnection telnetConnection in list)
			{
				telnetConnection.Close(false);
			}
		}
		catch (Exception ex)
		{
			string str = "Error in Telnet.Disconnect: ";
			Exception ex2 = ex;
			Log.Out(str + ((ex2 != null) ? ex2.ToString() : null));
		}
	}

	public void SendLine(string _line)
	{
		if (_line == null)
		{
			return;
		}
		List<TelnetConnection> obj = this.connections;
		lock (obj)
		{
			foreach (TelnetConnection telnetConnection in this.connections)
			{
				telnetConnection.SendLine(_line);
			}
		}
	}

	public void SendLog(string _formattedMessage, string _plainMessage, string _trace, LogType _type, DateTime _timestamp, long _uptime)
	{
		List<TelnetConnection> obj = this.connections;
		lock (obj)
		{
			foreach (TelnetConnection telnetConnection in this.connections)
			{
				telnetConnection.SendLog(_formattedMessage, _plainMessage, _trace, _type, _timestamp, _uptime);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static int maxLoginAttempts;

	[PublicizedFrom(EAccessModifier.Private)]
	public static int blockTimeSeconds;

	[PublicizedFrom(EAccessModifier.Private)]
	public TcpListener listener;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly bool authEnabled;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<TelnetConnection> connections = new List<TelnetConnection>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<int, TelnetConsole.LoginAttempts> loginAttemptsPerIP = new Dictionary<int, TelnetConsole.LoginAttempts>();

	[PublicizedFrom(EAccessModifier.Private)]
	public class LoginAttempts
	{
		public bool LogAttempt()
		{
			this.lastAttempt = DateTime.Now;
			this.count++;
			return this.count < TelnetConsole.maxLoginAttempts;
		}

		public bool IsBanned()
		{
			if ((DateTime.Now - this.lastAttempt).TotalSeconds > (double)TelnetConsole.blockTimeSeconds)
			{
				this.count = 0;
			}
			return this.count >= TelnetConsole.maxLoginAttempts;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public int count;

		[PublicizedFrom(EAccessModifier.Private)]
		public DateTime lastAttempt = new DateTime(0L);
	}
}
