using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Twitch;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class AIDirector
{
	public AIDirector(World _world)
	{
		this.World = _world;
		this.CreateComponents();
		this.Init();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Init()
	{
		this.random = GameRandomManager.Instance.CreateGameRandom();
		this.ComponentsInitNewGame();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CreateComponents()
	{
		this.CreateComponent<AIDirectorMarkerManagementComponent>();
		this.CreateComponent<AIDirectorPlayerManagementComponent>();
		this.CreateComponent<AIDirectorWanderingHordeComponent>();
		this.CreateComponent<AIDirectorAirDropComponent>();
		this.CreateComponent<AIDirectorChunkEventComponent>();
		this.CreateComponent<AIDirectorBloodMoonComponent>();
		this.playerManagementComponent = this.GetComponent<AIDirectorPlayerManagementComponent>();
		this.chunkEventComponent = this.GetComponent<AIDirectorChunkEventComponent>();
		this.bloodMoonComponent = this.GetComponent<AIDirectorBloodMoonComponent>();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public T CreateComponent<T>() where T : AIDirectorComponent, new()
	{
		string fullName = typeof(T).FullName;
		if (this.components.dict.ContainsKey(fullName))
		{
			throw new Exception("Multiple instances of the same component type are not allowed!");
		}
		T t = Activator.CreateInstance<T>();
		t.Director = this;
		this.components.Add(fullName, t);
		return t;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ComponentsInitNewGame()
	{
		for (int i = 0; i < this.components.list.Count; i++)
		{
			this.components.list[i].InitNewGame();
		}
	}

	public T GetComponent<T>() where T : AIDirectorComponent
	{
		string fullName = typeof(T).FullName;
		AIDirectorComponent aidirectorComponent;
		if (this.components.dict.TryGetValue(fullName, out aidirectorComponent))
		{
			return aidirectorComponent as T;
		}
		return default(T);
	}

	public AIDirectorBloodMoonComponent BloodMoonComponent
	{
		get
		{
			return this.bloodMoonComponent;
		}
	}

	public void Load(BinaryReader stream)
	{
		int version = stream.ReadInt32();
		this.ComponentsLoad(stream, version);
		if (this.World.worldTime == 0UL)
		{
			this.Init();
		}
	}

	public void Save(BinaryWriter stream)
	{
		stream.Write(9);
		this.ComponentsSave(stream);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ComponentsLoad(BinaryReader reader, int version)
	{
		for (int i = 0; i < this.components.list.Count; i++)
		{
			this.components.list[i].Read(reader, version);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ComponentsSave(BinaryWriter writer)
	{
		for (int i = 0; i < this.components.list.Count; i++)
		{
			this.components.list[i].Write(writer);
		}
	}

	public void Tick(double dt)
	{
		this.ComponentsTick(dt);
		this.DebugTick();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ComponentsTick(double _dt)
	{
		for (int i = 0; i < this.components.list.Count; i++)
		{
			this.components.list[i].Tick(_dt);
		}
	}

	public static bool CanSpawn(float _priority = 1f)
	{
		return (float)GameStats.GetInt(EnumGameStats.EnemyCount) < (float)GamePrefs.GetInt(EnumGamePrefs.MaxSpawnedZombies) * _priority;
	}

	public static ulong GetActivityWorldTimeDelay()
	{
		float num = (float)GameStats.GetInt(EnumGameStats.TimeOfDayIncPerSec) / 6f;
		num = Utils.FastClamp(num, 0.2f, 5f);
		return (ulong)(1000f * num);
	}

	public void NotifyActivity(EnumAIDirectorChunkEvent type, Vector3i position, float value, float _duration = 720f)
	{
		if (value > 0f && GameStats.GetBool(EnumGameStats.ZombieHordeMeter) && GameStats.GetBool(EnumGameStats.IsSpawnEnemies) && !this.BloodMoonComponent.BloodMoonActive && !TwitchManager.BossHordeActive)
		{
			AIDirectorChunkEvent chunkEvent = new AIDirectorChunkEvent(type, position, value, _duration);
			this.chunkEventComponent.NotifyEvent(chunkEvent);
		}
	}

	public void NotifyNoise(Entity instigator, Vector3 position, string clipName, float volumeScale)
	{
		AIDirectorData.Noise noise;
		if (!AIDirectorData.FindNoise(clipName, out noise))
		{
			return;
		}
		if (instigator is EntityEnemy)
		{
			return;
		}
		AIDirectorPlayerState aidirectorPlayerState = null;
		if (instigator)
		{
			if (instigator.IsIgnoredByAI())
			{
				return;
			}
			this.playerManagementComponent.trackedPlayers.dict.TryGetValue(instigator.entityId, out aidirectorPlayerState);
		}
		EntityItem entityItem = instigator as EntityItem;
		if (entityItem != null && ItemClass.GetForId(entityItem.itemStack.itemValue.type).ThrowableDecoy.Value)
		{
			return;
		}
		if (aidirectorPlayerState != null)
		{
			if (aidirectorPlayerState.Player.IsCrouching)
			{
				volumeScale *= noise.muffledWhenCrouched;
			}
			float volume = noise.volume * volumeScale;
			if (aidirectorPlayerState.Player.Stealth.NotifyNoise(volume, noise.duration))
			{
				instigator.world.CheckSleeperVolumeNoise(position);
			}
		}
		if (noise.heatMapStrength > 0f)
		{
			this.NotifyActivity(EnumAIDirectorChunkEvent.Sound, World.worldToBlockPos(position), noise.heatMapStrength * volumeScale, 240f);
		}
	}

	public void NotifyIntentToAttack(EntityAlive zombie, EntityAlive player)
	{
	}

	public void UpdatePlayerInventory(EntityPlayerLocal player)
	{
		this.playerManagementComponent.UpdatePlayerInventory(player);
	}

	public void UpdatePlayerInventory(int entityId, AIDirectorPlayerInventory inventory)
	{
		this.playerManagementComponent.UpdatePlayerInventory(entityId, inventory);
	}

	public void OnSoundPlayedAtPosition(int _entityThatCausedSound, Vector3 _position, string clipName, float volumeScale)
	{
		Entity instigator = null;
		if (_entityThatCausedSound != -1)
		{
			instigator = this.World.GetEntity(_entityThatCausedSound);
		}
		this.NotifyNoise(instigator, _position, clipName, volumeScale);
	}

	public void AddEntity(Entity entity)
	{
		EntityPlayer player;
		if (player = (entity as EntityPlayer))
		{
			this.AddPlayer(player);
		}
	}

	public void RemoveEntity(Entity entity)
	{
		EntityPlayer player;
		if (player = (entity as EntityPlayer))
		{
			this.RemovePlayer(player);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void AddPlayer(EntityPlayer player)
	{
		this.playerManagementComponent.AddPlayer(player);
		this.BloodMoonComponent.AddPlayer(player);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RemovePlayer(EntityPlayer player)
	{
		this.playerManagementComponent.RemovePlayer(player);
		this.BloodMoonComponent.RemovePlayer(player);
	}

	public static void LogAI(string _format, params object[] _args)
	{
		_format = string.Format("AIDirector: {0}", _format);
		Log.Out(_format, _args);
	}

	public static void LogAIExtra(string _format, params object[] _args)
	{
		if (AIDirectorConstants.DebugOutput)
		{
			AIDirector.LogAI(_format, _args);
		}
	}

	public void DebugFrameLateUpdate()
	{
		if (AIDirector.debugSendLatencyToPlayerIds.Count > 0)
		{
			this.DebugSendLatency();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DebugTick()
	{
		if (AIDirector.debugSendNameInfoToPlayerIds.Count > 0)
		{
			this.DebugSendNameInfo();
		}
	}

	public static void DebugToggleSendNameInfo(int playerId)
	{
		if (AIDirector.debugSendNameInfoToPlayerIds.Remove(playerId))
		{
			Log.Out("DebugToggleSendNames {0} off", new object[]
			{
				playerId
			});
			NetPackageDebug package = NetPackageManager.GetPackage<NetPackageDebug>().Setup(NetPackageDebug.Type.AINameInfoClientOff, -1, null);
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(package, false, playerId, -1, -1, null, 192);
			return;
		}
		Log.Out("DebugToggleSendNames {0} on", new object[]
		{
			playerId
		});
		AIDirector.debugSendNameInfoToPlayerIds.Add(playerId);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DebugSendNameInfo()
	{
		int num = this.debugNameInfoTicks - 1;
		this.debugNameInfoTicks = num;
		if (num > 0)
		{
			return;
		}
		this.debugNameInfoTicks = 5;
		World world = GameManager.Instance.World;
		for (int i = 0; i < AIDirector.debugSendNameInfoToPlayerIds.Count; i++)
		{
			int num2 = AIDirector.debugSendNameInfoToPlayerIds[i];
			EntityPlayer entityPlayer;
			world.Players.dict.TryGetValue(num2, out entityPlayer);
			if (entityPlayer)
			{
				ClientInfo clientInfo = SingletonMonoBehaviour<ConnectionManager>.Instance.Clients.ForEntityId(num2);
				if (clientInfo != null)
				{
					Bounds bb = new Bounds(entityPlayer.position, new Vector3(50f, 50f, 50f));
					world.GetEntitiesInBounds(typeof(EntityAlive), bb, this.debugEntities);
					for (int j = this.debugEntities.Count - 1; j >= 0; j--)
					{
						EntityAlive entityAlive = (EntityAlive)this.debugEntities[j];
						if (entityAlive.aiManager != null)
						{
							string s = entityAlive.aiManager.MakeDebugName(entityPlayer);
							NetPackageDebug package = NetPackageManager.GetPackage<NetPackageDebug>().Setup(NetPackageDebug.Type.AINameInfo, entityAlive.entityId, Encoding.UTF8.GetBytes(s));
							clientInfo.SendPackage(package);
						}
					}
					this.debugEntities.Clear();
				}
			}
		}
	}

	public static void DebugReceiveNameInfo(int entityId, byte[] _data)
	{
		World world = GameManager.Instance.World;
		if (world == null)
		{
			return;
		}
		EntityAlive entityAlive = world.GetEntity(entityId) as EntityAlive;
		if (entityAlive)
		{
			entityAlive.SetupDebugNameHUD(true);
			string @string = Encoding.UTF8.GetString(_data);
			entityAlive.DebugNameInfo = @string;
		}
	}

	public static void DebugToggleSendLatency(int playerId)
	{
		if (!AIDirector.debugSendLatencyToPlayerIds.Remove(playerId))
		{
			Log.Out("DebugToggleSendLatency {0} on", new object[]
			{
				playerId
			});
			AIDirector.debugSendLatencyToPlayerIds.Add(playerId);
			return;
		}
		Log.Out("DebugToggleSendLatency {0} off", new object[]
		{
			playerId
		});
		if (GameManager.Instance.World.GetPrimaryPlayerId() != playerId)
		{
			NetPackageDebug package = NetPackageManager.GetPackage<NetPackageDebug>().Setup(NetPackageDebug.Type.AILatencyClientOff, -1, null);
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(package, false, playerId, -1, -1, null, 192);
			return;
		}
		AIDirector.DebugLatencyOff();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DebugSendLatency()
	{
		World world = GameManager.Instance.World;
		for (int i = 0; i < AIDirector.debugSendLatencyToPlayerIds.Count; i++)
		{
			int num = AIDirector.debugSendLatencyToPlayerIds[i];
			EntityPlayer entityPlayer;
			world.Players.dict.TryGetValue(num, out entityPlayer);
			if (entityPlayer)
			{
				ClientInfo clientInfo = SingletonMonoBehaviour<ConnectionManager>.Instance.Clients.ForEntityId(num);
				Bounds bb = new Bounds(entityPlayer.position, new Vector3(50f, 50f, 50f));
				world.GetEntitiesInBounds(typeof(EntityAlive), bb, this.debugEntities);
				for (int j = this.debugEntities.Count - 1; j >= 0; j--)
				{
					EntityAlive entityAlive = (EntityAlive)this.debugEntities[j];
					if (entityAlive.aiManager != null)
					{
						using (PooledBinaryWriter pooledBinaryWriter = MemoryPools.poolBinaryWriter.AllocSync(false))
						{
							pooledBinaryWriter.SetBaseStream(AIDirector.latencyStream);
							AIDirector.latencyStream.Position = 0L;
							pooledBinaryWriter.Write(entityAlive.position.x);
							pooledBinaryWriter.Write(entityAlive.position.y);
							pooledBinaryWriter.Write(entityAlive.position.z);
							Vector3 vector = entityAlive.GetVelocityPerSecond();
							Vector3 vector2 = entityAlive.motion * 20f;
							if (vector.sqrMagnitude < vector2.sqrMagnitude)
							{
								vector = vector2;
							}
							pooledBinaryWriter.Write(vector.x);
							pooledBinaryWriter.Write(vector.y);
							pooledBinaryWriter.Write(vector.z);
							Quaternion rotation = entityAlive.transform.rotation;
							pooledBinaryWriter.Write(rotation.x);
							pooledBinaryWriter.Write(rotation.y);
							pooledBinaryWriter.Write(rotation.z);
							pooledBinaryWriter.Write(rotation.w);
							byte[] data = AIDirector.latencyStream.ToArray();
							if (clientInfo != null)
							{
								NetPackageDebug package = NetPackageManager.GetPackage<NetPackageDebug>().Setup(NetPackageDebug.Type.AILatency, entityAlive.entityId, data);
								clientInfo.SendPackage(package);
							}
							else
							{
								AIDirector.DebugReceiveLatency(entityAlive.entityId, data);
							}
						}
					}
				}
				this.debugEntities.Clear();
			}
		}
	}

	public static void DebugReceiveLatency(int entityId, byte[] _data)
	{
		World world = GameManager.Instance.World;
		if (world == null)
		{
			return;
		}
		EntityAlive entityAlive = world.GetEntity(entityId) as EntityAlive;
		if (entityAlive)
		{
			using (PooledBinaryReader pooledBinaryReader = MemoryPools.poolBinaryReader.AllocSync(false))
			{
				MemoryStream baseStream = new MemoryStream(_data);
				pooledBinaryReader.SetBaseStream(baseStream);
				Vector3 a;
				a.x = pooledBinaryReader.ReadSingle();
				a.y = pooledBinaryReader.ReadSingle();
				a.z = pooledBinaryReader.ReadSingle();
				Vector3 vector;
				vector.x = pooledBinaryReader.ReadSingle();
				vector.y = pooledBinaryReader.ReadSingle();
				vector.z = pooledBinaryReader.ReadSingle();
				Quaternion rotation;
				rotation.x = pooledBinaryReader.ReadSingle();
				rotation.y = pooledBinaryReader.ReadSingle();
				rotation.z = pooledBinaryReader.ReadSingle();
				rotation.w = pooledBinaryReader.ReadSingle();
				Transform transform = entityAlive.transform;
				Transform parent = transform.parent;
				Transform transform2 = parent.Find("DebugLatency");
				if (!transform2)
				{
					GameObject gameObject = (GameObject)UnityEngine.Object.Instantiate(Resources.Load("Prefabs/Debug/DebugLatency"), parent);
					gameObject.name = "DebugLatency";
					transform2 = gameObject.transform;
				}
				Vector3 vector2 = a - Origin.position;
				transform2.position = vector2;
				transform2.rotation = rotation;
				LineRenderer component = transform2.GetComponent<LineRenderer>();
				component.SetPosition(0, Quaternion.Inverse(rotation) * (transform.position - vector2));
				float num = (float)world.GetPrimaryPlayer().pingToServer * 0.001f;
				if (num < 0f)
				{
					num = 0f;
				}
				num *= 2f;
				if (vector.y < 0f)
				{
					vector.y = 0f;
				}
				component.SetPosition(2, Quaternion.Inverse(rotation) * (vector * num));
			}
		}
	}

	public static void DebugLatencyOff()
	{
		World world = GameManager.Instance.World;
		if (world == null)
		{
			return;
		}
		for (int i = 0; i < world.Entities.list.Count; i++)
		{
			EntityAlive entityAlive = world.Entities.list[i] as EntityAlive;
			if (entityAlive)
			{
				Transform transform = entityAlive.transform.parent.Find("DebugLatency");
				if (transform)
				{
					UnityEngine.Object.Destroy(transform.gameObject);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cActivityDuration = 720f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cActivityNoiseDuration = 240f;

	public readonly World World;

	public GameRandom random;

	[PublicizedFrom(EAccessModifier.Private)]
	public DictionaryList<string, AIDirectorComponent> components = new DictionaryList<string, AIDirectorComponent>();

	[PublicizedFrom(EAccessModifier.Private)]
	public AIDirectorPlayerManagementComponent playerManagementComponent;

	[PublicizedFrom(EAccessModifier.Private)]
	public AIDirectorChunkEventComponent chunkEventComponent;

	[PublicizedFrom(EAccessModifier.Private)]
	public AIDirectorBloodMoonComponent bloodMoonComponent;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<Entity> debugEntities = new List<Entity>();

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cDebugSendNameInfoTickRate = 5;

	[PublicizedFrom(EAccessModifier.Private)]
	public static List<int> debugSendNameInfoToPlayerIds = new List<int>();

	[PublicizedFrom(EAccessModifier.Private)]
	public int debugNameInfoTicks;

	[PublicizedFrom(EAccessModifier.Private)]
	public const string cLatencyName = "DebugLatency";

	[PublicizedFrom(EAccessModifier.Private)]
	public static MemoryStream latencyStream = new MemoryStream();

	[PublicizedFrom(EAccessModifier.Private)]
	public static List<int> debugSendLatencyToPlayerIds = new List<int>();

	public enum HordeEvent
	{
		None,
		Warn1,
		Warn2,
		Spawn
	}
}
