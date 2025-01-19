﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class DroneManager
{
	public static DroneManager Instance
	{
		get
		{
			return DroneManager.instance;
		}
	}

	public static void Init()
	{
		DroneManager.instance = new DroneManager();
		DroneManager.instance.Load();
	}

	public void AddTrackedDrone(EntityDrone _drone)
	{
		if (!_drone)
		{
			Log.Error("{0} AddTrackedDrone null", new object[]
			{
				base.GetType()
			});
			return;
		}
		_drone.OnWakeUp();
		if (!this.dronesActive.Contains(_drone))
		{
			this.dronesActive.Add(_drone);
			this.TriggerSave();
		}
	}

	public void RemoveTrackedDrone(EntityDrone _drone, EnumRemoveEntityReason _reason)
	{
		this.dronesActive.Remove(_drone);
		if (_reason == EnumRemoveEntityReason.Unloaded)
		{
			EntityAlive owner = _drone.Owner;
			if (owner)
			{
				OwnedEntityData ownedEntity = owner.GetOwnedEntity(_drone.entityId);
				if (ownedEntity != null)
				{
					ownedEntity.SetLastKnownPosition(_drone.position);
					EntityClass entityClass = EntityClass.list[ownedEntity.ClassId];
					NavObjectManager.Instance.RegisterNavObject(entityClass.NavObject, ownedEntity.LastKnownPosition, "", false, null);
				}
			}
			this.dronesUnloaded.Add(new EntityCreationData(_drone, true));
		}
		this.TriggerSave();
		SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageVehicleCount>().Setup(), false, -1, -1, -1, null, 192);
	}

	public void TriggerSave()
	{
		this.saveTime = Mathf.Min(this.saveTime, 10f);
	}

	public void Update()
	{
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			return;
		}
		World world = GameManager.Instance.World;
		if (world == null || world.Players == null || world.Players.Count == 0)
		{
			return;
		}
		if (!GameManager.Instance.gameStateManager.IsGameStarted())
		{
			return;
		}
		int num = 0;
		for (int i = this.dronesUnloaded.Count - 1; i >= 0; i--)
		{
			EntityCreationData entityCreationData = this.dronesUnloaded[i];
			EntityDrone entityDrone = world.GetEntity(entityCreationData.id) as EntityDrone;
			if (entityDrone)
			{
				Log.Warning("{0} already loaded #{1}, id {2}, {3}, {4}", new object[]
				{
					base.GetType(),
					i,
					entityCreationData.id,
					entityDrone,
					entityDrone.position.ToCultureInvariantString()
				});
				this.dronesUnloaded.RemoveAt(i);
			}
			else if (world.IsChunkAreaCollidersLoaded(entityCreationData.pos))
			{
				if (!this.isValidDronePos(entityCreationData.pos))
				{
					bool flag = false;
					IDictionary<PlatformUserIdentifierAbs, PersistentPlayerData> players = GameManager.Instance.GetPersistentPlayerList().Players;
					if (players != null)
					{
						foreach (KeyValuePair<PlatformUserIdentifierAbs, PersistentPlayerData> keyValuePair in players)
						{
							EntityPlayer entityPlayer = GameManager.Instance.World.GetEntity(keyValuePair.Value.EntityId) as EntityPlayer;
							if (entityPlayer)
							{
								OwnedEntityData[] ownedEntities = entityPlayer.GetOwnedEntities();
								for (int j = 0; j < ownedEntities.Length; j++)
								{
									if (entityCreationData.id == ownedEntities[j].Id)
									{
										Log.Warning("recovering {0} owned entity for {1}", new object[]
										{
											entityCreationData.id,
											entityPlayer.entityId
										});
										entityCreationData.pos = entityPlayer.getHeadPosition();
										entityCreationData.belongsPlayerId = entityPlayer.entityId;
										flag = true;
										break;
									}
								}
							}
						}
					}
					if (!flag)
					{
						entityCreationData.pos = Vector3.zero;
						if (entityCreationData.belongsPlayerId == -1)
						{
							this.dronesWithoutOwner.Add(entityCreationData);
							this.dronesUnloaded.RemoveAt(i);
							goto IL_290;
						}
						goto IL_290;
					}
				}
				entityDrone = (EntityFactory.CreateEntity(entityCreationData) as EntityDrone);
				if (entityDrone)
				{
					this.dronesActive.Add(entityDrone);
					world.SpawnEntityInWorld(entityDrone);
					num++;
				}
				else
				{
					Log.Error("DroneManager load failed #{0}, id {1}, {2}", new object[]
					{
						i,
						entityCreationData.id,
						EntityClass.GetEntityClassName(entityCreationData.entityClass)
					});
				}
				this.dronesUnloaded.RemoveAt(i);
			}
			IL_290:;
		}
		this.saveTime -= Time.deltaTime;
		if (this.saveTime <= 0f && (this.saveThread == null || this.saveThread.HasTerminated()))
		{
			this.saveTime = 120f;
			this.Save();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isValidDronePos(Vector3 pos)
	{
		return !float.IsNaN(pos.x) && !float.IsNaN(pos.y) && !float.IsNaN(pos.z);
	}

	public void RemoveAllDronesFromMap()
	{
		GameManager gameManager = GameManager.Instance;
		PersistentPlayerList persistentPlayerList = gameManager.GetPersistentPlayerList();
		World world = gameManager.World;
		foreach (KeyValuePair<PlatformUserIdentifierAbs, PersistentPlayerData> keyValuePair in persistentPlayerList.Players)
		{
			EntityPlayer entityPlayer = world.GetEntity(keyValuePair.Value.EntityId) as EntityPlayer;
			if (entityPlayer)
			{
				OwnedEntityData[] ownedEntities = entityPlayer.GetOwnedEntities();
				int j;
				int i;
				Predicate<EntityDrone> <>9__0;
				for (i = ownedEntities.Length - 1; i >= 0; i = j)
				{
					List<EntityDrone> list = this.dronesActive;
					Predicate<EntityDrone> match;
					if ((match = <>9__0) == null)
					{
						match = (<>9__0 = ((EntityDrone v) => v.entityId == ownedEntities[i].Id));
					}
					EntityDrone entityDrone = list.Find(match);
					if (entityDrone && entityPlayer.HasOwnedEntity(entityDrone.entityId))
					{
						GameManager.Instance.World.RemoveEntityFromMap(entityDrone, EnumRemoveEntityReason.Unloaded);
					}
					j = i - 1;
				}
			}
		}
	}

	public void ClearAllDronesForPlayer(EntityPlayer player)
	{
		this.ClearAllDronesForPlayer(player.entityId);
	}

	public void ClearAllDronesForPlayer(int entityId)
	{
		this.ClearUnloadedDrones(entityId);
		this.ClearActiveDrones(entityId);
		this.TriggerSave();
	}

	public void ClearUnloadedDrones(EntityPlayer player)
	{
		this.ClearUnloadedDrones(player.entityId);
	}

	public void ClearUnloadedDrones(int entityId)
	{
		for (int i = this.dronesUnloaded.Count - 1; i >= 0; i--)
		{
			if (this.dronesUnloaded[i].belongsPlayerId == entityId)
			{
				this.dronesUnloaded.RemoveAt(i);
			}
		}
	}

	public void ClearActiveDrones(int entityId)
	{
		for (int i = this.dronesActive.Count - 1; i >= 0; i--)
		{
			EntityDrone entityDrone = this.dronesActive[i];
			if (entityDrone.belongsPlayerId == entityId)
			{
				this.dronesActive.RemoveAt(i);
				GameManager.Instance.World.RemoveEntity(entityDrone.entityId, EnumRemoveEntityReason.Killed);
			}
		}
	}

	public string LogUnloadedDronesForPlayer(EntityPlayer player)
	{
		string text = string.Empty;
		for (int i = 0; i < this.dronesUnloaded.Count; i++)
		{
			if (this.dronesUnloaded[i].belongsPlayerId == player.entityId)
			{
				text = text + this.dronesUnloaded[i].clientEntityId.ToString() + Environment.NewLine;
			}
		}
		return text;
	}

	public bool AssignUnloadedDrone(EntityPlayer player, int entityId)
	{
		for (int i = 0; i < this.dronesUnloaded.Count; i++)
		{
			EntityCreationData entityCreationData = this.dronesUnloaded[i];
			if (entityCreationData.id == entityId)
			{
				entityCreationData.pos = player.getHeadPosition();
				entityCreationData.belongsPlayerId = player.entityId;
				Log.Warning(entityCreationData.belongsPlayerId.ToString());
				this.debugDronePlayerAssignment.Add(entityCreationData.belongsPlayerId);
				return true;
			}
		}
		return false;
	}

	public static void Cleanup()
	{
		if (DroneManager.instance != null)
		{
			DroneManager.instance.SaveAndClear();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SaveAndClear()
	{
		this.WaitOnSave();
		this.Save();
		this.WaitOnSave();
		this.dronesActive.Clear();
		this.dronesUnloaded.Clear();
		DroneManager.instance = null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void WaitOnSave()
	{
		if (this.saveThread != null)
		{
			this.saveThread.WaitForEnd();
			this.saveThread = null;
		}
	}

	public void Load()
	{
		string text = string.Format("{0}/{1}", GameIO.GetSaveGameDir(), "drones.dat");
		if (SdFile.Exists(text))
		{
			try
			{
				using (Stream stream = SdFile.OpenRead(text))
				{
					using (PooledBinaryReader pooledBinaryReader = MemoryPools.poolBinaryReader.AllocSync(false))
					{
						pooledBinaryReader.SetBaseStream(stream);
						this.read(pooledBinaryReader);
					}
				}
			}
			catch (Exception)
			{
				text = string.Format("{0}/{1}", GameIO.GetSaveGameDir(), "drones.dat.bak");
				if (SdFile.Exists(text))
				{
					using (Stream stream2 = SdFile.OpenRead(text))
					{
						using (PooledBinaryReader pooledBinaryReader2 = MemoryPools.poolBinaryReader.AllocSync(false))
						{
							pooledBinaryReader2.SetBaseStream(stream2);
							this.read(pooledBinaryReader2);
						}
					}
				}
			}
			Log.Out("{0} {1}, loaded {2}", new object[]
			{
				base.GetType(),
				text,
				this.dronesUnloaded.Count
			});
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Save()
	{
		if (this.saveThread == null || !ThreadManager.ActiveThreads.ContainsKey("droneDataSave"))
		{
			Log.Out("{0} saving {1} ({2} + {3})", new object[]
			{
				base.GetType(),
				this.dronesActive.Count + this.dronesUnloaded.Count,
				this.dronesActive.Count,
				this.dronesUnloaded.Count
			});
			PooledExpandableMemoryStream pooledExpandableMemoryStream = MemoryPools.poolMemoryStream.AllocSync(true);
			using (PooledBinaryWriter pooledBinaryWriter = MemoryPools.poolBinaryWriter.AllocSync(false))
			{
				pooledBinaryWriter.SetBaseStream(pooledExpandableMemoryStream);
				this.write(pooledBinaryWriter);
			}
			this.saveThread = ThreadManager.StartThread("droneDataSave", null, new ThreadManager.ThreadFunctionLoopDelegate(this.SaveThread), null, System.Threading.ThreadPriority.Normal, pooledExpandableMemoryStream, null, false, true);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int SaveThread(ThreadManager.ThreadInfo _threadInfo)
	{
		PooledExpandableMemoryStream pooledExpandableMemoryStream = (PooledExpandableMemoryStream)_threadInfo.parameter;
		string text = string.Format("{0}/{1}", GameIO.GetSaveGameDir(), "drones.dat");
		if (SdFile.Exists(text))
		{
			SdFile.Copy(text, string.Format("{0}/{1}", GameIO.GetSaveGameDir(), "drones.dat.bak"), true);
		}
		pooledExpandableMemoryStream.Position = 0L;
		StreamUtils.WriteStreamToFile(pooledExpandableMemoryStream, text);
		Log.Out("{0} saved {1} bytes", new object[]
		{
			base.GetType(),
			pooledExpandableMemoryStream.Length
		});
		MemoryPools.poolMemoryStream.FreeSync(pooledExpandableMemoryStream);
		return -1;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void read(PooledBinaryReader _br)
	{
		if (_br.ReadChar() != 'v' || _br.ReadChar() != 'd' || _br.ReadChar() != 'a' || _br.ReadChar() != '\0')
		{
			Log.Error("{0} file bad signature", new object[]
			{
				base.GetType()
			});
			return;
		}
		if (_br.ReadByte() != 1)
		{
			Log.Error("{0} file bad version", new object[]
			{
				base.GetType()
			});
			return;
		}
		this.dronesUnloaded.Clear();
		int num = _br.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			EntityCreationData entityCreationData = new EntityCreationData();
			entityCreationData.read(_br, false);
			this.dronesUnloaded.Add(entityCreationData);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void write(PooledBinaryWriter _bw)
	{
		_bw.Write('v');
		_bw.Write('d');
		_bw.Write('a');
		_bw.Write(0);
		_bw.Write(1);
		List<EntityCreationData> list = new List<EntityCreationData>();
		this.GetDrones(list);
		_bw.Write(list.Count);
		for (int i = 0; i < list.Count; i++)
		{
			EntityCreationData entityCreationData = list[i];
			if (!this.isValidDronePos(entityCreationData.pos))
			{
				EntityPlayer entityPlayer = GameManager.Instance.World.GetEntity(entityCreationData.belongsPlayerId) as EntityPlayer;
				if (entityPlayer)
				{
					Log.Warning("corrupted data using the player position");
					entityCreationData.pos = entityPlayer.getHeadPosition();
				}
				else
				{
					Log.Warning("corrupted data clearing the drone position");
					entityCreationData.pos = Vector3.zero;
				}
			}
			entityCreationData.write(_bw, false);
		}
	}

	public List<EntityCreationData> GetDronesList()
	{
		List<EntityCreationData> list = new List<EntityCreationData>();
		this.GetDrones(list);
		return list;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void GetDrones(List<EntityCreationData> _list)
	{
		for (int i = 0; i < this.dronesActive.Count; i++)
		{
			_list.Add(new EntityCreationData(this.dronesActive[i], true));
		}
		for (int j = 0; j < this.dronesUnloaded.Count; j++)
		{
			_list.Add(this.dronesUnloaded[j]);
		}
		for (int k = 0; k < this.dronesWithoutOwner.Count; k++)
		{
			_list.Add(this.dronesWithoutOwner[k]);
		}
	}

	public static int GetServerDroneCount()
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			return DroneManager.Instance.dronesActive.Count + DroneManager.Instance.dronesUnloaded.Count;
		}
		return DroneManager.serverDroneCount;
	}

	public static void SetServerDroneCount(int count)
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			return;
		}
		DroneManager.serverDroneCount = count;
	}

	public static bool CanAddMoreDrones()
	{
		return !(DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5).IsCurrent() || DroneManager.GetServerDroneCount() < 500;
	}

	[Conditional("DEBUG_DRONEMAN")]
	public static void VMLog(string _format = "", params object[] _args)
	{
		int frameCount = GameManager.frameCount;
		_format = string.Format("{0} {1} {2}", frameCount, "DroneManager", _format);
		Log.Out(_format, _args);
	}

	public static bool Debug_LocalControl;

	public static bool DebugLogEnabled;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cVersion = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cSaveTime = 120f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cChangeSaveDelay = 10f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cMaxDrones = 500;

	[PublicizedFrom(EAccessModifier.Private)]
	public static int serverDroneCount;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<EntityDrone> dronesActive = new List<EntityDrone>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<EntityCreationData> dronesUnloaded = new List<EntityCreationData>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<EntityCreationData> dronesWithoutOwner = new List<EntityCreationData>();

	[PublicizedFrom(EAccessModifier.Private)]
	public float saveTime = 120f;

	[PublicizedFrom(EAccessModifier.Private)]
	public static DroneManager instance;

	[PublicizedFrom(EAccessModifier.Private)]
	public ThreadManager.ThreadInfo saveThread;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<int> debugDronePlayerAssignment = new List<int>();

	[PublicizedFrom(EAccessModifier.Private)]
	public const string cNameKey = "drones";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string cThreadKey = "droneDataSave";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string cStringName = "DroneManager";
}
