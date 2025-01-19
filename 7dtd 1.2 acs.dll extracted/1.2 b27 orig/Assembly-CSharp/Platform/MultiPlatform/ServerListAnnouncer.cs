using System;
using System.Collections.Generic;

namespace Platform.MultiPlatform
{
	public class ServerListAnnouncer : IMasterServerAnnouncer
	{
		public void Init(IPlatform _owner)
		{
		}

		public void Update()
		{
		}

		public bool GameServerInitialized
		{
			get
			{
				IMasterServerAnnouncer serverListAnnouncer = PlatformManager.NativePlatform.ServerListAnnouncer;
				if (serverListAnnouncer == null || serverListAnnouncer.GameServerInitialized)
				{
					IPlatform crossplatformPlatform = PlatformManager.CrossplatformPlatform;
					bool? flag;
					if (crossplatformPlatform == null)
					{
						flag = null;
					}
					else
					{
						IMasterServerAnnouncer serverListAnnouncer2 = crossplatformPlatform.ServerListAnnouncer;
						flag = ((serverListAnnouncer2 != null) ? new bool?(serverListAnnouncer2.GameServerInitialized) : null);
					}
					return flag ?? true;
				}
				return false;
			}
		}

		public string GetServerPorts()
		{
			string text = "";
			IMasterServerAnnouncer serverListAnnouncer = PlatformManager.NativePlatform.ServerListAnnouncer;
			string text2 = (serverListAnnouncer != null) ? serverListAnnouncer.GetServerPorts() : null;
			if (!string.IsNullOrEmpty(text2))
			{
				if (!string.IsNullOrEmpty(text))
				{
					text += ", ";
				}
				text += text2;
			}
			IPlatform crossplatformPlatform = PlatformManager.CrossplatformPlatform;
			string text3;
			if (crossplatformPlatform == null)
			{
				text3 = null;
			}
			else
			{
				IMasterServerAnnouncer serverListAnnouncer2 = crossplatformPlatform.ServerListAnnouncer;
				text3 = ((serverListAnnouncer2 != null) ? serverListAnnouncer2.GetServerPorts() : null);
			}
			string text4 = text3;
			if (!string.IsNullOrEmpty(text4))
			{
				if (!string.IsNullOrEmpty(text))
				{
					text += ", ";
				}
				text += text4;
			}
			foreach (KeyValuePair<EPlatformIdentifier, IPlatform> keyValuePair in PlatformManager.ServerPlatforms)
			{
				if (keyValuePair.Value.AsServerOnly)
				{
					IMasterServerAnnouncer serverListAnnouncer3 = keyValuePair.Value.ServerListAnnouncer;
					string text5 = (serverListAnnouncer3 != null) ? serverListAnnouncer3.GetServerPorts() : null;
					if (!string.IsNullOrEmpty(text5))
					{
						if (!string.IsNullOrEmpty(text))
						{
							text += ", ";
						}
						text += text5;
					}
				}
			}
			return text;
		}

		public void AdvertiseServer(Action _onServerRegistered)
		{
			IMasterServerAnnouncer serverListAnnouncer = PlatformManager.NativePlatform.ServerListAnnouncer;
			if (serverListAnnouncer != null)
			{
				serverListAnnouncer.AdvertiseServer(_onServerRegistered);
			}
			IPlatform crossplatformPlatform = PlatformManager.CrossplatformPlatform;
			if (crossplatformPlatform != null)
			{
				IMasterServerAnnouncer serverListAnnouncer2 = crossplatformPlatform.ServerListAnnouncer;
				if (serverListAnnouncer2 != null)
				{
					serverListAnnouncer2.AdvertiseServer(_onServerRegistered);
				}
			}
			foreach (KeyValuePair<EPlatformIdentifier, IPlatform> keyValuePair in PlatformManager.ServerPlatforms)
			{
				if (keyValuePair.Value.AsServerOnly)
				{
					IMasterServerAnnouncer serverListAnnouncer3 = keyValuePair.Value.ServerListAnnouncer;
					if (serverListAnnouncer3 != null)
					{
						serverListAnnouncer3.AdvertiseServer(_onServerRegistered);
					}
				}
			}
		}

		public void StopServer()
		{
			foreach (KeyValuePair<EPlatformIdentifier, IPlatform> keyValuePair in PlatformManager.ServerPlatforms)
			{
				if (keyValuePair.Value.AsServerOnly)
				{
					IMasterServerAnnouncer serverListAnnouncer = keyValuePair.Value.ServerListAnnouncer;
					if (serverListAnnouncer != null)
					{
						serverListAnnouncer.StopServer();
					}
				}
			}
			IPlatform crossplatformPlatform = PlatformManager.CrossplatformPlatform;
			if (crossplatformPlatform != null)
			{
				IMasterServerAnnouncer serverListAnnouncer2 = crossplatformPlatform.ServerListAnnouncer;
				if (serverListAnnouncer2 != null)
				{
					serverListAnnouncer2.StopServer();
				}
			}
			IMasterServerAnnouncer serverListAnnouncer3 = PlatformManager.NativePlatform.ServerListAnnouncer;
			if (serverListAnnouncer3 == null)
			{
				return;
			}
			serverListAnnouncer3.StopServer();
		}
	}
}
