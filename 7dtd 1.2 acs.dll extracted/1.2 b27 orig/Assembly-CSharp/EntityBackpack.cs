using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class EntityBackpack : EntityItem
{
	public override void CopyPropertiesFromEntityClass()
	{
		base.CopyPropertiesFromEntityClass();
		EntityClass entityClass = EntityClass.list[this.entityClass];
		float num = 5f;
		entityClass.Properties.ParseFloat(EntityClass.PropTimeStayAfterDeath, ref num);
		this.ticksStayAfterDeath = (int)(num * 20f);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Start()
	{
		base.Start();
		this.LogBackpack("Start", Array.Empty<object>());
		foreach (Collider collider in base.transform.GetComponentsInChildren<Collider>())
		{
			collider.gameObject.tag = "E_BP_Body";
			collider.gameObject.layer = 13;
			collider.enabled = true;
			collider.gameObject.AddMissingComponent<RootTransformRefEntity>().RootTransform = base.transform;
		}
		this.SetDead();
		if (this.lootContainer != null)
		{
			this.lootContainer.entityId = this.entityId;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Update()
	{
		base.Update();
		Vector3 position = this.position;
		if (this.world.AdjustBoundsForPlayers(ref position, 0.06f))
		{
			this.itemRB.velocity *= 0.5f;
			position.y = this.itemRB.position.y + Origin.position.y;
			this.itemRB.position = position - Origin.position;
			this.SetPosition(position, true);
		}
	}

	public override void OnUpdateEntity()
	{
		base.OnUpdateEntity();
		if (this.deathUpdateTicks > 0)
		{
			bool flag = GameManager.Instance.GetEntityIDForLockedTileEntity(this.lootContainer) != -1;
			if (!this.bRemoved && this.lootContainer != null && !this.lootContainer.IsUserAccessing() && !flag)
			{
				if (this.lootContainer.bTouched && this.lootContainer.IsEmpty())
				{
					this.RemoveBackpack("empty");
				}
				if (this.deathUpdateTicks >= this.ticksStayAfterDeath - 1)
				{
					this.RemoveBackpack("old");
				}
			}
		}
		this.deathUpdateTicks++;
		if (!this.bRemoved && !this.isEntityRemote && base.transform.position.y + Origin.position.y < 1f)
		{
			Vector3 vector = new Vector3(this.position.x, (float)(this.world.GetHeight(Utils.Fastfloor(this.position.x), Utils.Fastfloor(this.position.z)) + 5) + this.rand.RandomFloat * 20f, this.position.z);
			Log.Warning("EntityBackpack below world {0}, moving to {1}", new object[]
			{
				this.position.ToCultureInvariantString(),
				vector.ToCultureInvariantString()
			});
			this.SetPosition(vector, true);
			base.transform.position = vector - Origin.position;
			int num = this.safetyCounter + 1;
			this.safetyCounter = num;
			if (num > 500)
			{
				this.RemoveBackpack("retries");
			}
		}
		if (!this.bRemoved && !this.isEntityRemote && this.RefPlayerId != -1)
		{
			using (IEnumerator<PersistentPlayerData> enumerator = GameManager.Instance.persistentPlayers.Players.Values.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current.TryUpdateBackpackPosition(this.entityId, new Vector3i(this.position)))
					{
						break;
					}
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RemoveBackpack(string reason)
	{
		this.LogBackpack("RemoveBackpack empty {0}, reason {1}", new object[]
		{
			this.lootContainer == null || this.lootContainer.IsEmpty(),
			reason
		});
		this.deathUpdateTicks = this.ticksStayAfterDeath;
		Vector3i zero = Vector3i.zero;
		if (!this.isEntityRemote && this.RefPlayerId != -1)
		{
			foreach (PersistentPlayerData persistentPlayerData in GameManager.Instance.persistentPlayers.Players.Values)
			{
				if (persistentPlayerData.TryRemoveDroppedBackpack(this.entityId))
				{
					Vector3i mostRecentBackpackPosition = persistentPlayerData.MostRecentBackpackPosition;
					break;
				}
			}
		}
		EntityPlayer entityPlayer = this.world.GetEntity(this.RefPlayerId) as EntityPlayer;
		if (entityPlayer != null)
		{
			if (!entityPlayer.isEntityRemote && SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				entityPlayer.SetDroppedBackpackPositions(GameManager.Instance.persistentLocalPlayer.GetDroppedBackpackPositions());
			}
			else if (!this.world.IsRemote())
			{
				PersistentPlayerData persistentPlayerData2 = GameManager.Instance.persistentPlayers.EntityToPlayerMap[entityPlayer.entityId];
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackagePlayerSetBackpackPosition>().Setup(entityPlayer.entityId, persistentPlayerData2.GetDroppedBackpackPositions()), false, entityPlayer.entityId, -1, -1, null, 192);
			}
		}
		this.bRemoved = true;
	}

	public override void OnEntityUnload()
	{
		this.LogBackpack("OnEntityUnload markedForUnload {0}, IsDead {1}, IsDespawned {2}", new object[]
		{
			this.markedForUnload,
			this.IsDead(),
			this.IsDespawned
		});
		base.OnEntityUnload();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void createMesh()
	{
	}

	public override void Write(BinaryWriter _bw, bool _bNetworkWrite)
	{
		this.LogBackpack("Write", Array.Empty<object>());
		base.Write(_bw, _bNetworkWrite);
		_bw.Write(this.RefPlayerId);
	}

	public override void Read(byte _version, BinaryReader _br)
	{
		base.Read(_version, _br);
		this.RefPlayerId = _br.ReadInt32();
		this.LogBackpack("Read", Array.Empty<object>());
	}

	public override bool IsMarkedForUnload()
	{
		return base.IsMarkedForUnload() && this.bRemoved;
	}

	public override string GetLootList()
	{
		return this.lootListOnDeath;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void HandleNavObject()
	{
		if (this.RefPlayerId != -1)
		{
			EntityPlayerLocal entityPlayerLocal = this.world.GetEntity(this.RefPlayerId) as EntityPlayerLocal;
			if (entityPlayerLocal != null && this.RefPlayerId == entityPlayerLocal.entityId)
			{
				if (GamePrefs.GetInt(EnumGamePrefs.DeathPenalty) == 3 && entityPlayerLocal.GetDroppedBackpackPositions().Count == 0)
				{
					this.RefPlayerId = -1;
					return;
				}
				if (EntityClass.list[this.entityClass].NavObject != "")
				{
					this.NavObject = NavObjectManager.Instance.RegisterNavObject(EntityClass.list[this.entityClass].NavObject, base.transform, "", false);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void LogBackpack(string format, params object[] args)
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer || this.world.IsLocalPlayer(this.RefPlayerId))
		{
			string text = "?";
			if (this.lootContainer != null)
			{
				text = string.Empty;
				int num = 0;
				foreach (ItemStack itemStack in this.lootContainer.GetItems())
				{
					if (!itemStack.IsEmpty())
					{
						num++;
						if (num == 1)
						{
							text = itemStack.itemValue.ItemClass.Name;
						}
						if (num == 2)
						{
							text = text + ", " + itemStack.itemValue.ItemClass.Name;
						}
					}
				}
				Vector3i vector3i = this.lootContainer.ToWorldPos();
				text = string.Format("{0} {1} at ({2}, xz {3} {4})", new object[]
				{
					num,
					text,
					vector3i,
					World.toBlockXZ(vector3i.x),
					World.toBlockXZ(vector3i.z)
				});
			}
			int num2 = 0;
			if (ThreadManager.IsMainThread())
			{
				num2 = Time.frameCount;
			}
			format = string.Format("{0} EntityBackpack id {1}, plyrId {2}, {3} ({4}), chunk {5} ({6}), items {7} : {8}", new object[]
			{
				num2,
				this.entityId,
				this.RefPlayerId,
				this.position.ToCultureInvariantString(),
				World.toChunkXZ(this.position),
				this.addedToChunk,
				this.chunkPosAddedEntityTo,
				text,
				format
			});
			Log.Out(format, args);
		}
	}

	public int RefPlayerId = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int deathUpdateTicks;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int ticksStayAfterDeath;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool bRemoved;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int safetyCounter;
}
