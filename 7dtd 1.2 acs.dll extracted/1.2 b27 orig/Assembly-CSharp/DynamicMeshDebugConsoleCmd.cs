using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using UniLinq;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class DynamicMeshDebugConsoleCmd : ConsoleCmdAbstract
{
	public override bool IsExecuteOnClient
	{
		get
		{
			return false;
		}
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (_params == null || _params.Count == 0)
		{
			DynamicMeshManager.Instance.AddChunk(new Vector3i(GameManager.Instance.World.GetPrimaryPlayer().GetPosition()), true);
			return;
		}
		string a = _params[0].ToLower();
		if (a == "ss")
		{
			DynamicMeshServer.ShowSender = true;
			return;
		}
		if (a == "dolog")
		{
			DynamicMeshManager.DoLog = !DynamicMeshManager.DoLog;
			Log.Out("DyMesh doLog: " + DynamicMeshManager.DoLog.ToString());
			return;
		}
		if (a == "lognet")
		{
			DynamicMeshManager.DoLogNet = !DynamicMeshManager.DoLogNet;
			Log.Out("DoLogNet: " + DynamicMeshManager.DoLogNet.ToString());
			return;
		}
		if (a == "tars")
		{
			Vector3 position = GameManager.Instance.World.Players.dict[_senderInfo.RemoteClientInfo.entityId].position;
			DynamicMeshManager.ImportVox("tars", position, 502);
			return;
		}
		if (a == "vox")
		{
			string param = this.GetParam(_params, 1);
			Vector3 position2 = GameManager.Instance.World.Players.dict[_senderInfo.RemoteClientInfo.entityId].position;
			int blockId;
			int.TryParse(this.GetParam(_params, 2) ?? "502", out blockId);
			DynamicMeshManager.ImportVox(param, position2, blockId);
			return;
		}
		if (a == "areaaround" || a == "aa")
		{
			int num = (_params.Count < 2) ? 150 : int.Parse(_params[1]);
			EntityPlayerLocal primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
			Vector3 position3;
			if (primaryPlayer == null)
			{
				position3 = GameManager.Instance.World.GetPlayers().FirstOrDefault((EntityPlayer d) => d.entityId == _senderInfo.RemoteClientInfo.entityId).position;
			}
			else
			{
				position3 = primaryPlayer.position;
			}
			int num2 = (int)position3.x - num;
			while ((float)num2 < position3.x + (float)num)
			{
				int num3 = (int)position3.z - num;
				while ((float)num3 < position3.z + (float)num)
				{
					DynamicMeshManager.Instance.AddChunk(new Vector3i(num2, 0, num3), true);
					num3 += 16;
				}
				num2 += 16;
			}
			string str = "Adding chunks around  ";
			Vector3 vector = position3;
			Log.Out(str + vector.ToString() + " rad: " + num.ToString());
			return;
		}
		if (a == "air")
		{
			if (_params.Count < 1)
			{
				DynamicMeshManager.LogMsg("Specify a radius");
				return;
			}
			int num4 = int.Parse(this.GetParam(_params, 1));
			BlockValue air = BlockValue.Air;
			Vector3i vector3i = new Vector3i(GameManager.Instance.World.Players.list.FirstOrDefault((EntityPlayer d) => d.entityId == _senderInfo.RemoteClientInfo.entityId).GetPosition());
			World world = GameManager.Instance.World;
			for (int i = vector3i.x - num4; i < vector3i.x + num4; i++)
			{
				for (int j = vector3i.z - num4; j < vector3i.z + num4; j++)
				{
					Vector3i vector3i2 = new Vector3i(i, vector3i.y, j);
					if (world.GetBlock(vector3i2).type != 0)
					{
						world.SetBlockRPC(vector3i2, air);
					}
				}
			}
			return;
		}
		else
		{
			if (a == "log")
			{
				string text = string.Concat(new string[]
				{
					"Buff : ",
					DynamicMeshManager.Instance.BufferRegionLoadRequests.Count.ToString(),
					"\n Server : ",
					SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer.ToString(),
					" / ",
					SingletonMonoBehaviour<ConnectionManager>.Instance.IsClient.ToString(),
					"\n Thread P/S : ",
					DynamicMeshThread.PrimaryQueue.Count.ToString(),
					" / ",
					DynamicMeshThread.SecondaryQueue.Count.ToString(),
					"\n Packets: ",
					NetPackageDynamicMesh.Count.ToString(),
					"\n ThreadDistance: ",
					DynamicMeshManager.ThreadDistance.ToString(),
					"\n ObserverDistance: ",
					DynamicMeshManager.ObserverDistance.ToString(),
					"\n ItemLoadDistance: ",
					DynamicMeshManager.ItemLoadDistance.ToString(),
					"\n WorldChunks: ",
					GameManager.Instance.World.ChunkCache.chunks.list.Count.ToString(),
					"\n ThreadNext: ",
					DynamicMeshThread.nextChunks.Count.ToString(),
					"\n ThreadQueue: ",
					DynamicMeshThread.Queue,
					"\n RegionUpdates: ",
					DynamicMeshThread.RegionUpdates.Count.ToString(),
					"\n RegionUpdatesDebug: ",
					DynamicMeshThread.RegionUpdatesDebug,
					"\n SyncPackets: ",
					DynamicMeshServer.SyncRequests.Count.ToString(),
					" (",
					DynamicMeshServer.ActiveSyncs.Count.ToString(),
					")"
				});
				foreach (DynamicMeshClientConnection dynamicMeshClientConnection in DynamicMeshServer.ClientData.Values)
				{
					string[] array = new string[5];
					array[0] = text;
					array[1] = "\n ";
					array[2] = dynamicMeshClientConnection.EntityId.ToString();
					array[3] = ": ";
					array[4] = dynamicMeshClientConnection.ItemsToSend.Values.Sum((ConcurrentQueue<DynamicMeshSyncRequest> d) => d.Count).ToString();
					text = string.Concat(array);
				}
				Log.Out("Info: " + text);
				return;
			}
			if (a == "fog")
			{
				float minValue = float.MinValue;
				float minValue2 = float.MinValue;
				if (_params.Count >= 1)
				{
					float density = StringParsers.ParseFloat(_params[1], 0, -1, NumberStyles.Any);
					SkyManager.SetFogDebug(density, minValue, minValue2);
					Log.Out("Fog " + density.ToString());
					return;
				}
			}
			else
			{
				if (a == "checkprefabs" || a == "cp" || a == "forcegen")
				{
					DynamicMeshManager.Instance.CheckPrefabs("Console", true);
					return;
				}
				if (a == "resend")
				{
					DynamicMeshServer.ResendPackages = !DynamicMeshServer.ResendPackages;
					Log.Out("Resending dymesh packages: " + DynamicMeshServer.ResendPackages.ToString());
				}
			}
			return;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string GetParam(List<string> _params, int index)
	{
		if (_params == null)
		{
			return null;
		}
		if (index >= _params.Count)
		{
			return null;
		}
		return _params[index];
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int GetParamAsInt(List<string> _params, int index)
	{
		int result = -9999;
		if (_params == null)
		{
			return result;
		}
		if (index >= _params.Count)
		{
			return result;
		}
		int.TryParse(_params[index], out result);
		return result;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[]
		{
			DynamicMeshDebugConsoleCmd.info,
			"zd"
		};
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return DynamicMeshDebugConsoleCmd.info;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string info = "Dynamic mesh debug";

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool GC_ENABLED = true;
}
