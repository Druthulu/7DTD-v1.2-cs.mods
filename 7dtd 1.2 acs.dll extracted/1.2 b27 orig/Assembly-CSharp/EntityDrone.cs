using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Audio;
using GamePath;
using Platform;
using RaycastPathing;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class EntityDrone : EntityNPC, ILockable
{
	public override bool IsValidAimAssistSlowdownTarget
	{
		get
		{
			return false;
		}
	}

	public override bool IsValidAimAssistSnapTarget
	{
		get
		{
			return false;
		}
	}

	public DroneLightManager lightManager
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (!this._lm)
			{
				this._lm = base.transform.GetComponentInChildren<DroneLightManager>();
			}
			return this._lm;
		}
	}

	public EntityDrone.Orders OrderState
	{
		get
		{
			return this.orderState;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setOrders(EntityDrone.Orders orders)
	{
		this.orderState = orders;
	}

	public static bool IsValidForLocalPlayer()
	{
		PersistentPlayerData playerData = GameManager.Instance.GetPersistentPlayerList().GetPlayerData(PlatformManager.InternalLocalUserIdentifier);
		return playerData != null && EntityDrone.IsValidForPlayer(GameManager.Instance.World.GetEntity(playerData.EntityId) as EntityPlayerLocal);
	}

	public static bool IsValidForPlayer(EntityPlayerLocal localPlayer)
	{
		foreach (OwnedEntityData ownedEntityData in localPlayer.GetOwnedEntities())
		{
			if (ownedEntityData.ClassId != -1 && EntityClass.list[ownedEntityData.ClassId].entityClassName == "entityJunkDrone")
			{
				GameManager.ShowTooltip(localPlayer, Localization.Get("xuiMaxDeployedDronesReached", false), string.Empty, "ui_denied", null, false);
				return false;
			}
		}
		return true;
	}

	public static void OnClientSpawnRemote(Entity _entity)
	{
		GameManager instance = GameManager.Instance;
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			EntityDrone entityDrone = _entity as EntityDrone;
			if (entityDrone)
			{
				int i = 1;
				while (i < ItemClass.list.Length - 1)
				{
					ItemClass itemClass = ItemClass.list[i];
					if (itemClass != null && itemClass.Name == "gunBotT3JunkDrone")
					{
						entityDrone.OwnerID = PlatformManager.InternalLocalUserIdentifier;
						PersistentPlayerData playerData = instance.GetPersistentPlayerList().GetPlayerData(entityDrone.OwnerID);
						if (playerData != null)
						{
							entityDrone.belongsPlayerId = playerData.EntityId;
							(instance.World.GetEntity(playerData.EntityId) as EntityAlive).AddOwnedEntity(_entity);
							break;
						}
						break;
					}
					else
					{
						i++;
					}
				}
			}
		}
		instance.World.EntityLoadedDelegates -= EntityDrone.OnClientSpawnRemote;
	}

	public void InitDynamicSpawn()
	{
		int i = 1;
		while (i < ItemClass.list.Length - 1)
		{
			ItemClass itemClass = ItemClass.list[i];
			if (itemClass != null && itemClass.Name == "gunBotT3JunkDrone")
			{
				this.OriginalItemValue = new ItemValue(itemClass.Id, false);
				this.OwnerID = PlatformManager.InternalLocalUserIdentifier;
				PersistentPlayerData playerData = GameManager.Instance.GetPersistentPlayerList().GetPlayerData(this.OwnerID);
				if (playerData != null)
				{
					this.belongsPlayerId = playerData.EntityId;
					(GameManager.Instance.World.GetEntity(playerData.EntityId) as EntityAlive).AddOwnedEntity(this);
					return;
				}
				break;
			}
			else
			{
				i++;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Awake()
	{
		base.Awake();
		this.steering = new EntityDrone.EntitySteering(this);
		this.isLocked = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void LateUpdate()
	{
		if (DroneManager.Debug_LocalControl)
		{
			this.debugInputRotX += Input.GetAxis("Mouse X") * 30f * 0.05f;
			this.debugInputRotY += Input.GetAxis("Mouse Y") * 30f * 0.05f;
			this.debugInputRotY = Mathf.Clamp(this.debugInputRotY, -90f, 90f);
			this.reconCam.transform.localRotation = Quaternion.AngleAxis(this.debugInputRotX, Vector3.up);
			this.reconCam.transform.localRotation *= Quaternion.AngleAxis(this.debugInputRotY, Vector3.left);
			RaycastHit raycastHit;
			if (Input.GetMouseButtonDown(0) && RaycastPathUtils.IsPositionBlocked(this.reconCam.ScreenPointToRay(Input.mousePosition), out raycastHit, 65536, true, 100f))
			{
				RaycastPathUtils.DrawBounds(World.worldToBlockPos(raycastHit.point + Origin.position), Color.yellow, 1f, 1f);
				this.pathMan.CreatePath(this.Owner.position, raycastHit.point + Origin.position, this.currentSpeedFlying, false, this.FollowHoverHeight);
			}
		}
	}

	public override void Init(int _entityClass)
	{
		base.Init(_entityClass);
	}

	public override void InitInventory()
	{
		this.inventory = new EntityDrone.DroneInventory(GameManager.Instance, this);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void LogDrone(string format, params object[] args)
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer || base.GetAttachedPlayerLocal())
		{
			format = string.Format("{0} Drone {1}", GameManager.frameCount, format);
			Log.Out(format, args);
		}
	}

	public override void PostInit()
	{
		this.LogDrone("PostInit {0}, {1} (chunk {2}), rbPos {3}", new object[]
		{
			this,
			this.position,
			World.toChunkXZ(this.position),
			this.PhysicsTransform.position + Origin.position
		});
		float num = 1f / base.transform.localScale.x;
		this.interactionCollider = base.gameObject.GetComponent<BoxCollider>();
		if (this.interactionCollider)
		{
			this.interactionCollider.center = new Vector3(0f, 0.05f * num, 0.05f * num);
			this.interactionCollider.size = new Vector3(2.5f, 2f, 2f);
		}
		this.sensors = new EntityDrone.DroneSensors(this);
		this.sensors.Init();
		this.initWorldValues(true);
		this.IsFlyMode.Value = true;
		this.bCanClimbLadders = true;
		this.bCanClimbVertical = true;
		this.prefabColor = this.GetPaintColor();
	}

	public override void OnAddedToWorld()
	{
		if (this.itemvalueToLoad != null)
		{
			this.OriginalItemValue = this.itemvalueToLoad;
		}
		this.isOwnerSyncPending = true;
		this.InitWeapons();
		this.LoadMods();
		if (this.nativeCollider)
		{
			this.nativeCollider.enabled = true;
		}
		this.Health = Mathf.RoundToInt(base.Stats.Health.Max * (1f - this.OriginalItemValue.UseTimes / (float)this.OriginalItemValue.MaxUseTimes));
		Animator componentInChildren = base.GetComponentInChildren<Animator>();
		if (!componentInChildren.enabled)
		{
			componentInChildren.enabled = true;
		}
		componentInChildren.Play("Base Layer.Idle", 0, 0f);
		componentInChildren.Update(0f);
		componentInChildren.StopPlayback();
		this.pathMan = new FloodFillEntityPathGenerator(this.world, this);
		Origin.OriginChanged = (Action<Vector3>)Delegate.Combine(Origin.OriginChanged, new Action<Vector3>(this.OnOriginChanged));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnOriginChanged(Vector3 _origin)
	{
		string str = "EntityDrone - OnOriginChanged: ";
		Vector3 vector = _origin;
		Log.Out(str + vector.ToString());
	}

	public override void OnEntityUnload()
	{
		Origin.OriginChanged = (Action<Vector3>)Delegate.Remove(Origin.OriginChanged, new Action<Vector3>(this.OnOriginChanged));
		if (!this.isBeingPickedUp)
		{
			EntityClass entityClass = EntityClass.list[this.entityClass];
			if (GameManager.Instance.World.IsLocalPlayer(this.belongsPlayerId))
			{
				NavObjectManager.Instance.RegisterNavObject(entityClass.NavObject, this.position, "", false, null);
			}
		}
		this.UnRegsiterMovingLights();
		base.OnEntityUnload();
	}

	public override bool CanUpdateEntity()
	{
		return this.Owner || base.CanUpdateEntity();
	}

	public override bool CanNavigatePath()
	{
		return true;
	}

	public override float GetEyeHeight()
	{
		if (this.head == null)
		{
			this.head = base.transform.FindInChilds("Head", false);
		}
		return this.head.position.y - base.transform.position.y;
	}

	public override Ray GetLookRay()
	{
		return new Ray(this.position + new Vector3(0f, this.GetEyeHeight(), 0f), (this.currentTarget == null) ? this.GetLookVector() : (this.currentTarget.getHeadPosition() - this.position).normalized);
	}

	public override bool CanBePushed()
	{
		return true;
	}

	public override float GetWeight()
	{
		return base.GetWeight();
	}

	public override bool IsDead()
	{
		return false;
	}

	public override bool IsAttackValid()
	{
		return this.stunWeapon.canFire() || this.machineGunWeapon.canFire();
	}

	public override int Health
	{
		get
		{
			return (int)base.Stats.Health.Value;
		}
		set
		{
			float num = (float)Mathf.Max(value, 1);
			if (num == 1f && this.state != EntityDrone.State.Shutdown)
			{
				this.isShutdownPending = true;
			}
			base.Stats.Health.Value = num;
		}
	}

	public override int DamageEntity(DamageSource _damageSource, int _strength, bool _criticalHit, float _impulseScale)
	{
		int strength = Mathf.RoundToInt((float)_strength * this.armorDamageReduction);
		if (_damageSource.damageType == EnumDamageTypes.BloodLoss)
		{
			this.Buffs.RemoveBuff("buffInjuryBleeding", true);
			strength = 0;
		}
		EntityAlive entityAlive = (EntityAlive)this.world.GetEntity(_damageSource.getEntityId());
		if (this.Owner && entityAlive)
		{
			if (!this.debugFriendlyFire && entityAlive && entityAlive.factionId == this.Owner.factionId)
			{
				strength = 0;
			}
		}
		else
		{
			strength = 0;
		}
		return base.DamageEntity(_damageSource, strength, _criticalHit, _impulseScale);
	}

	public override void PlayStepSound()
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void HandleNavObject()
	{
		NavObjectManager instance = NavObjectManager.Instance;
		EntityClass eClass = EntityClass.list[this.entityClass];
		if (eClass.NavObject != "")
		{
			NavObject navObject = instance.NavObjectList.Find(delegate(NavObject n)
			{
				NavObjectClass navObjectClass = n.NavObjectClass;
				return ((navObjectClass != null) ? navObjectClass.NavObjectClassName : null) == eClass.NavObject;
			});
			if (navObject != null)
			{
				instance.UnRegisterNavObject(navObject);
			}
			this.NavObject = instance.RegisterNavObject(eClass.NavObject, this, "", false);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void AddCharacterController()
	{
		base.AddCharacterController();
		if (this.PhysicsTransform == null)
		{
			return;
		}
		if (this.m_characterController == null)
		{
			return;
		}
		this.RootMotion = false;
		this.m_characterController.SetSize(Vector3.zero, this.physColHeight, this.physColHeight * 0.5f);
		this.setNoClip(true);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override float GetPushBoundsVertical()
	{
		return 1f;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void updateStepSound(float _distX, float _distZ)
	{
	}

	public override void Write(BinaryWriter _bw, bool _bNetworkWrite)
	{
		base.Write(_bw, _bNetworkWrite);
		_bw.Write(1);
		this.OwnerID.ToStream(_bw, false);
		this.OriginalItemValue = this.GetUpdatedItemValue();
		this.OriginalItemValue.Write(_bw);
		ushort num = 49251;
		_bw.Write(num);
		this.WriteSyncData(_bw, num);
	}

	public override void Read(byte _version, BinaryReader _br)
	{
		base.Read(_version, _br);
		_br.ReadInt32();
		this.OwnerID = PlatformUserIdentifierAbs.FromStream(_br, false, false);
		this.OriginalItemValue = ItemValue.None.Clone();
		this.OriginalItemValue.Read(_br);
		ushort syncFlags = _br.ReadUInt16();
		this.ReadSyncData(_br, syncFlags, 0);
	}

	public override EntityActivationCommand[] GetActivationCommands(Vector3i _tePos, EntityAlive _entityFocusing)
	{
		bool flag = !this.IsDead();
		if (this.IsDead())
		{
			return new EntityActivationCommand[0];
		}
		bool flag2 = false;
		if (this.belongsToPlayerId(_entityFocusing.entityId))
		{
			flag2 = ((_entityFocusing as EntityPlayerLocal).IsGodMode.Value && Debug.isDebugBuild);
			return new EntityActivationCommand[]
			{
				new EntityActivationCommand("talk", "talk", flag && this.state != EntityDrone.State.Shutdown),
				new EntityActivationCommand("service", "service", flag2),
				new EntityActivationCommand("repair", "wrench", (float)this.Health < base.Stats.Health.Max),
				new EntityActivationCommand("lock", "lock", !this.isLocked),
				new EntityActivationCommand("unlock", "unlock", this.isLocked),
				new EntityActivationCommand("keypad", "keypad", true),
				new EntityActivationCommand("take", "hand", true),
				new EntityActivationCommand("stay", "run_and_gun", flag && this.OrderState != EntityDrone.Orders.Stay && this.state != EntityDrone.State.Shutdown),
				new EntityActivationCommand("follow", "run", flag && this.OrderState != EntityDrone.Orders.Follow && this.state != EntityDrone.State.Shutdown),
				new EntityActivationCommand("heal", "cardio", flag && this.state != EntityDrone.State.Shutdown && this.TargetCanBeHealed(_entityFocusing)),
				new EntityActivationCommand("storage", "loot_sack", true),
				new EntityActivationCommand("drone_silent", this.isQuietMode ? "sight" : "stealth", true),
				new EntityActivationCommand("drone_light", this.isFlashlightOn ? "electric_switch" : "lightbulb", this.isFlashlightAttached),
				new EntityActivationCommand("force_pickup", "store_all_up", flag2)
			};
		}
		bool flag3 = !this.isLocked || this.belongsToPlayerId(_entityFocusing.entityId) || this.IsUserAllowed(PlatformManager.InternalLocalUserIdentifier);
		bool flag4 = this.isLocked && !this.IsUserAllowed(PlatformManager.InternalLocalUserIdentifier);
		bool flag5 = (float)this.Health < base.Stats.Health.Max;
		if (!flag3 && !flag4 && !flag5 && !flag2)
		{
			this.PlaySound("ui_denied", 1f);
			return new EntityActivationCommand[0];
		}
		return new EntityActivationCommand[]
		{
			new EntityActivationCommand("storage", "loot_sack", flag3),
			new EntityActivationCommand("keypad", "keypad", flag4),
			new EntityActivationCommand("repair", "wrench", flag5),
			new EntityActivationCommand("force_pickup", "store_all_up", flag2)
		};
	}

	public override bool OnEntityActivated(int _indexInBlockActivationCommands, Vector3i _tePos, EntityAlive _entityFocusing)
	{
		EntityPlayerLocal entityPlayer = _entityFocusing as EntityPlayerLocal;
		LocalPlayerUI uiforPlayer = LocalPlayerUI.GetUIForPlayer(entityPlayer);
		int requestType = -1;
		if (this.belongsToPlayerId(_entityFocusing.entityId))
		{
			switch (_indexInBlockActivationCommands)
			{
			case 0:
				this.startDialog(_entityFocusing);
				break;
			case 1:
				requestType = _indexInBlockActivationCommands;
				break;
			case 2:
				this.doRepairAction(entityPlayer, uiforPlayer);
				break;
			case 3:
				this.PlaySound("misc/locking", 1f);
				this.isLocked = !this.isLocked;
				this.SendSyncData(2);
				break;
			case 4:
				this.PlaySound("misc/unlocking", 1f);
				this.isLocked = !this.isLocked;
				this.SendSyncData(2);
				break;
			case 5:
				this.doKeypadAction(uiforPlayer);
				break;
			case 6:
				this.pickup(_entityFocusing);
				break;
			case 7:
				this.SentryMode();
				break;
			case 8:
				this.FollowMode();
				break;
			case 9:
				if (!this.healWeapon.hasHealingItem())
				{
					GameManager.ShowTooltip(this.Owner as EntityPlayerLocal, Localization.Get("xuiDroneNeedsHealItemsStored", false), string.Empty, "ui_denied", null, false);
					this.PlaySound("drone_empty", 1f);
				}
				else
				{
					this.HealOwner();
				}
				break;
			case 10:
				requestType = _indexInBlockActivationCommands;
				break;
			case 11:
			{
				this.isQuietMode = !this.isQuietMode;
				Handle handle = this.idleLoop;
				if (handle != null)
				{
					handle.Stop(this.entityId);
				}
				this.idleLoop = null;
				this.SendSyncData(32);
				break;
			}
			case 12:
				this.doToggleLightAction();
				break;
			case 13:
				this.pickup(_entityFocusing);
				break;
			}
		}
		else
		{
			switch (_indexInBlockActivationCommands)
			{
			case 0:
				requestType = 10;
				break;
			case 1:
				this.doKeypadAction(uiforPlayer);
				break;
			case 2:
				this.doRepairAction(entityPlayer, uiforPlayer);
				break;
			case 3:
				this.pickup(_entityFocusing);
				break;
			}
		}
		this.processRequest(entityPlayer, requestType);
		return false;
	}

	public override void OnUpdateEntity()
	{
		base.OnUpdateEntity();
		if (DroneManager.Debug_LocalControl)
		{
			return;
		}
		this.SyncOwnerData();
		this.UpdateTransitionState();
		this.UpdateAnimStates();
		this.UpdateShutdownState();
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && this.Owner && this.isOutOfRange(this.Owner.position, this.MaxDistFromOwner) && this.state != EntityDrone.State.Shutdown && this.state != EntityDrone.State.Sentry)
		{
			this.teleportState();
		}
		if (!this.isQuietMode && this.idleLoop == null && this.state == EntityDrone.State.Idle && !GameManager.IsDedicatedServer)
		{
			this.idleLoop = this.PlaySoundLoop("drone_idle_hover", 0.2f);
		}
		if (!this.isQuietMode && this.idleLoop != null && this.state == EntityDrone.State.Idle && !GameManager.IsDedicatedServer)
		{
			this.notifySoundNoise(0.2f, 5f);
		}
		if ((this.state == EntityDrone.State.Idle || this.state == EntityDrone.State.Sentry || this.state == EntityDrone.State.Follow) && this.areaScanTimer > 0f)
		{
			this.areaScanTimer -= Time.deltaTime;
			if (this.areaScanTimer <= 0f)
			{
				this.isInConfinedSpace = this.pathMan.IsConfinedSpace(this.position, 3f, false);
				this.areaScanTimer = this.areaScanTime;
			}
		}
		this.UpdatePartyBuffs();
		if (this.Owner)
		{
			if (this.currentTarget)
			{
				if (!this.steering.IsInRange(this.currentTarget.position, this.FollowDistance * 2f))
				{
					if (this.decelerationTime > 0f)
					{
						this.decelerationTime = 0f;
					}
					this.accelerationTime += 0.05f;
					this.currentSpeedFlying = Mathf.Lerp(this.currentSpeedFlying, 15f, Mathf.Clamp01(this.accelerationTime / this.SpeedFlying));
				}
				else
				{
					if (this.accelerationTime > 0f)
					{
						this.accelerationTime = 0f;
					}
					this.decelerationTime += 0.05f;
					this.currentSpeedFlying = Mathf.Lerp(this.currentSpeedFlying, this.SpeedFlying, Mathf.Clamp01(this.decelerationTime / (this.SpeedFlying * 0.5f)));
				}
			}
			this.UpdateDroneSystems();
			EntityPlayerLocal entityPlayerLocal = this.Owner as EntityPlayerLocal;
			if (entityPlayerLocal && this.state == EntityDrone.State.Idle)
			{
				if (this.focusBoxNode == null)
				{
					if (entityPlayerLocal.MoveController.FocusBoxPosition == World.worldToBlockPos(this.position))
					{
						RaycastNode raycastNode = RaycastPathWorldUtils.FindNodeType(RaycastPathWorldUtils.ScanVolume(this.world, this.position, false, false, false, 0f), cPathNodeType.Air);
						if (raycastNode != null)
						{
							this.focusBoxNode = raycastNode;
						}
					}
				}
				else
				{
					Vector3 vector = this.focusBoxNode.Center - this.position;
					RaycastPathUtils.DrawLine(this.position, this.focusBoxNode.Center, Color.yellow, 1f);
					if (this.isOutOfRange(this.focusBoxNode.Center, 0.25f))
					{
						this.move(vector.normalized);
					}
					else
					{
						this.focusBoxNode = null;
					}
				}
			}
			else if (this.state != EntityDrone.State.Idle && this.focusBoxNode != null)
			{
				this.focusBoxNode = null;
			}
		}
		this.UpdateDroneServiceMenu();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateTransitionState()
	{
		if (this.transitionState != EntityDrone.State.None)
		{
			if (this.state == this.transitionState)
			{
				this.transitionState = EntityDrone.State.None;
				return;
			}
			EntityDrone.State state = this.transitionState;
			if (state != EntityDrone.State.Idle)
			{
				if (state != EntityDrone.State.Heal)
				{
					if (state == EntityDrone.State.Shutdown)
					{
						this.isShutdownPending = true;
					}
					else
					{
						this.setState(this.transitionState);
					}
				}
				else if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
				{
					this.HealOwnerServer();
				}
				else
				{
					this.healWeapon.RegisterOnFireComplete(new Action(this.onHealDone));
					this.healWeapon.Fire(this.currentTarget);
					Manager.Play(this, "drone_healeffect", 1f, false);
					this.setState(this.transitionState);
				}
			}
			else if (this.state == EntityDrone.State.Shutdown)
			{
				this.setShutdown(false);
			}
			else
			{
				this.setState(this.transitionState);
			}
			this.transitionState = EntityDrone.State.None;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateAnimStates()
	{
		if (this.stateTime < 1f)
		{
			Animator componentInChildren = base.GetComponentInChildren<Animator>();
			if (!componentInChildren.enabled && !componentInChildren.GetAnimatorTransitionInfo(0).IsName("Base Layer.Idle"))
			{
				this.isAnimationStateSet = false;
			}
		}
		if (!this.isAnimationStateSet)
		{
			Animator componentInChildren2 = base.GetComponentInChildren<Animator>();
			if (!componentInChildren2.enabled)
			{
				componentInChildren2.enabled = true;
			}
			if (this.Health > 1 && this.Owner)
			{
				if (this.PlayWakeupAnim)
				{
					this.playWakeupAnim();
					this.PlayWakeupAnim = false;
				}
				else
				{
					this.playIdleAnim();
				}
			}
			else
			{
				componentInChildren2.Play("Base Layer.Idle", 0, 0f);
				componentInChildren2.Update(0f);
				componentInChildren2.StopPlayback();
				componentInChildren2.enabled = false;
			}
			this.isAnimationStateSet = true;
		}
		if (this.wakeupAnimTime > 0f)
		{
			this.wakeupAnimTime -= 0.05f;
			if (this.wakeupAnimTime <= 0f && !GameManager.IsDedicatedServer)
			{
				if (this.Owner)
				{
					Manager.Stop(this.Owner.entityId, "drone_take");
				}
				if (GameManager.Instance.World.IsLocalPlayer(this.belongsPlayerId))
				{
					this.PlayVO("drone_wakeup", false, 1f);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateShutdownState()
	{
		if (this.isShutdownPending)
		{
			this.performShutdown();
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				this.SendSyncData(32768);
			}
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			if (!this.Owner && this.state != EntityDrone.State.Shutdown)
			{
				this.performShutdown();
				this.SendSyncData(32768);
			}
			if (this.Owner && this.Owner.Health <= 0 && this.state != EntityDrone.State.Shutdown && this.state != EntityDrone.State.Sentry)
			{
				this.performShutdown();
				if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
				{
					this.SendSyncData(32768);
				}
			}
			if (this.Health > 1 && this.Owner && this.Owner.Health > 1 && Vector3.Distance(this.position, this.Owner.position) < 10f && this.state == EntityDrone.State.Shutdown)
			{
				this.setShutdown(false);
				if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
				{
					this.SendSyncData(32768);
				}
			}
			if (this.state == EntityDrone.State.Shutdown)
			{
				this.processShutdown();
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdatePartyBuffs()
	{
		if (this.Owner && this.isSupportModAttached && !this.isEntityRemote)
		{
			this.BuffAllies();
			EntityPlayer entityPlayer = this.Owner as EntityPlayer;
			if (!this.partyEventsSet && entityPlayer && entityPlayer.Party != null)
			{
				this.registerPartyEvents(entityPlayer, true);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void updateTasks()
	{
		if (DroneManager.Debug_LocalControl)
		{
			float num = this.debugInputSpeed;
			if (InputUtils.ShiftKeyPressed)
			{
				num *= 10f;
			}
			this.debugInputFwd = this.reconCam.transform.forward;
			this.debugInputFwd.y = 0f;
			if (Input.GetKey(KeyCode.W))
			{
				this.move(this.debugInputFwd, num);
			}
			if (Input.GetKey(KeyCode.S))
			{
				this.move(-this.debugInputFwd, num);
			}
			this.debugInputRgt = this.reconCam.transform.right;
			this.debugInputRgt.y = 0f;
			if (Input.GetKey(KeyCode.A))
			{
				this.move(-this.debugInputRgt, num);
			}
			if (Input.GetKey(KeyCode.D))
			{
				this.move(this.debugInputRgt, num);
			}
			this.debugInputUp = this.reconCam.transform.up;
			this.debugInputUp.x = 0f;
			this.debugInputUp.z = 0f;
			if (Input.GetKey(KeyCode.Space))
			{
				this.move(this.debugInputUp, num * 0.5f);
			}
			if (Input.GetKey(KeyCode.C))
			{
				this.move(-this.debugInputUp, num * 0.5f);
			}
			RaycastPathUtils.DrawBounds(this.Owner.GetBlockPosition().ToVector3CenterXZ() - new Vector3(0.5f, 0f, 0.5f), Color.cyan, 1f, 1f);
			return;
		}
		base.GetEntitySenses().ClearIfExpired();
		if (this.Owner != null)
		{
			this.updateState();
			this.debugUpdate();
		}
	}

	public void SetItemValueToLoad(ItemValue itemValue)
	{
		this.itemvalueToLoad = itemValue.Clone();
	}

	public void LoadMods()
	{
		LootContainer lootContainer = LootContainer.GetLootContainer("roboticDrone", true);
		Vector2i size = lootContainer.size;
		if (this.lootContainer == null)
		{
			this.lootContainer = new TileEntityLootContainer(null);
			this.lootContainer.entityId = this.entityId;
			this.lootContainer.lootListName = lootContainer.Name;
			this.lootContainer.SetContainerSize(size, true);
			this.bag.SetupSlots(ItemStack.CreateArray(size.x * size.y));
		}
		this.lootContainer.bWasTouched = true;
		this.lightManager.DisableMaterials("junkDroneLamp");
		GameObject gameObject = base.transform.FindInChilds("freightBox", false).gameObject;
		GameObject gameObject2 = base.transform.FindInChilds("armor", false).gameObject;
		GameObject gameObject3 = base.transform.FindInChilds("machineGun", false).gameObject;
		GameObject gameObject4 = base.transform.FindInChilds("teddyBear", false).gameObject;
		if (gameObject != null)
		{
			gameObject.SetActive(false);
		}
		if (gameObject2 != null)
		{
			gameObject2.SetActive(false);
		}
		if (gameObject3 != null)
		{
			gameObject3.SetActive(false);
		}
		if (gameObject4 != null)
		{
			gameObject4.SetActive(false);
		}
		int num = size.x * size.y;
		if (this.OriginalItemValue.HasMods())
		{
			for (int i = 0; i < this.OriginalItemValue.Modifications.Length; i++)
			{
				ItemValue itemValue = this.OriginalItemValue.Modifications[i];
				if (itemValue.ItemClass != null)
				{
					string name = itemValue.ItemClass.Name;
					uint num2 = <PrivateImplementationDetails>.ComputeStringHash(name);
					if (num2 <= 2400030839U)
					{
						if (num2 != 1912183181U)
						{
							if (num2 != 2266484491U)
							{
								if (num2 == 2400030839U)
								{
									if (name == "modRoboticDroneWeaponMod")
									{
										this.isGunModAttached = true;
										if (gameObject3)
										{
											gameObject3.SetActive(true);
										}
									}
								}
							}
							else if (name == "modRoboticDroneMoraleBoosterMod")
							{
								this.isSupportModAttached = true;
								if (gameObject4)
								{
									gameObject4.SetActive(true);
								}
							}
						}
						else if (name == "modRoboticDroneCargoMod")
						{
							num += 8;
							if (gameObject)
							{
								gameObject.SetActive(true);
							}
						}
					}
					else if (num2 <= 3474526689U)
					{
						if (num2 != 2404831999U)
						{
							if (num2 == 3474526689U)
							{
								if (name == "modRoboticDroneArmorPlatingMod")
								{
									this.armorDamageReduction = 0.5f;
									if (gameObject2)
									{
										gameObject2.SetActive(true);
									}
								}
							}
						}
						else if (name == "modRoboticDroneMedicMod")
						{
							this.isHealModAttached = true;
						}
					}
					else if (num2 != 3914512375U)
					{
						if (num2 == 4027736419U)
						{
							if (name == "modRoboticDroneStunWeaponMod")
							{
								this.isStunModAttached = true;
							}
						}
					}
					else if (name == "modRoboticDroneHeadlampMod")
					{
						this.isFlashlightAttached = true;
						DroneLightManager.LightEffect[] lightEffects = this.lightManager.LightEffects;
						if (lightEffects.Length != 0)
						{
							LightManager.RegisterMovingLight(this, lightEffects[0].linkedObjects[0].GetComponent<Light>());
						}
						if (this.isFlashlightOn)
						{
							this.lightManager.InitMaterials("junkDroneLamp");
						}
					}
				}
			}
		}
		ItemStack[] array = ItemStack.CreateArray(num);
		Array.Copy(this.lootContainer.items, 0, array, 0, (this.lootContainer.items.Length < num) ? this.lootContainer.items.Length : num);
		this.bag.SetSlots(array);
		this.lootContainer.SetContainerSize(new Vector2i(8, Mathf.RoundToInt((float)(num / 8))), false);
		this.lootContainer.items = this.bag.GetSlots();
		Color color = this.prefabColor;
		ItemValue itemValue2 = this.OriginalItemValue.CosmeticMods[0];
		if (this.OriginalItemValue.CosmeticMods.Length != 0 && itemValue2 != null && !itemValue2.IsEmpty())
		{
			Vector3 vector = Block.StringToVector3(this.OriginalItemValue.GetPropertyOverride(Block.PropTintColor, "255,255,255"));
			color.r = vector.x;
			color.g = vector.y;
			color.b = vector.z;
		}
		for (int j = 0; j < this.paintableParts.Length; j++)
		{
			this.SetPaint(this.paintableParts[j], color);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void initWorldValues(bool value)
	{
		this.IsEntityUpdatedInUnloadedChunk = value;
		this.bWillRespawn = value;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void InitWeapons()
	{
		this.stunWeapon = new DroneWeapons.StunBeamWeapon(this);
		this.stunWeapon.Init();
		this.machineGunWeapon = new DroneWeapons.MachineGunWeapon(this);
		this.machineGunWeapon.Init();
		this.healWeapon = new DroneWeapons.HealBeamWeapon(this);
		this.healWeapon.Init();
	}

	public Color GetPaintColor()
	{
		return base.transform.FindRecursive("BaseMesh").GetComponentInChildren<Renderer>().sharedMaterial.color;
	}

	public void SetPaint(string childName, Color color)
	{
		Transform transform = base.transform.FindRecursive(childName);
		if (transform && transform.gameObject.activeSelf)
		{
			Renderer[] componentsInChildren = transform.GetComponentsInChildren<Renderer>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].material.color = color;
			}
		}
	}

	public bool PlayWakeupAnim { get; set; }

	[PublicizedFrom(EAccessModifier.Private)]
	public void playWakeupAnim()
	{
		base.GetComponentInChildren<Animator>().Play("Base Layer.SpawnIn");
		this.wakeupAnimTime = 2.5f;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void playIdleAnim()
	{
		Animator componentInChildren = base.GetComponentInChildren<Animator>();
		componentInChildren.Play("Base Layer.Idle", 0, 0f);
		componentInChildren.Update(0f);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UnRegsiterMovingLights()
	{
		DroneLightManager.LightEffect[] lightEffects = this.lightManager.LightEffects;
		if (lightEffects.Length != 0)
		{
			LightManager.UnRegisterMovingLight(this, lightEffects[0].linkedObjects[0].GetComponent<Light>());
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void doToggleLightAction()
	{
		this.isFlashlightOn = !this.isFlashlightOn;
		this.setFlashlightOn(this.isFlashlightOn);
		this.SendSyncData(64);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setFlashlightOn(bool value)
	{
		if (value)
		{
			this.lightManager.InitMaterials("junkDroneLamp");
			return;
		}
		this.lightManager.DisableMaterials("junkDroneLamp");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Handle PlaySoundLoop(string sound_path, float _vol = 1f)
	{
		return Manager.Play(this, sound_path, _vol, true);
	}

	public void PlaySound(string sound_path, float _vol = 1f)
	{
		this.PlaySound(this, sound_path, false, false, _vol);
	}

	public void PlayVO(string sound_path, bool _hasPriority = false, float _vol = 1f)
	{
		this.PlaySound(this, sound_path, true, _hasPriority, _vol);
	}

	public void PlaySound(Entity entity, string sound_path, bool _isVO = false, bool _hasPriority = false, float _vol = 1f)
	{
		if (!this.isQuietMode)
		{
			if (_isVO)
			{
				if (_hasPriority)
				{
					Handle handle = this.voHandle;
					if (handle != null)
					{
						handle.Stop(this.entityId);
					}
				}
				this.voHandle = Manager.Play(entity, sound_path, _vol, true);
			}
			else
			{
				Manager.Play(entity, sound_path, _vol, false);
			}
			this.notifySoundNoise(_vol * 2.5f, 5f);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void notifySoundNoise(float vol, float duration)
	{
		EntityPlayer entityPlayer = this.Owner as EntityPlayer;
		if (entityPlayer)
		{
			entityPlayer.Stealth.NotifyNoise(vol, duration);
		}
	}

	public void NotifySyncOwner()
	{
		PersistentPlayerData playerData = GameManager.Instance.GetPersistentPlayerList().GetPlayerData(this.OwnerID);
		if (playerData != null)
		{
			this.belongsPlayerId = playerData.EntityId;
			this.Owner = (GameManager.Instance.World.GetEntity(this.belongsPlayerId) as EntityAlive);
		}
		if (this.Owner)
		{
			this.currentTarget = this.Owner;
			this.rotation = Quaternion.LookRotation(this.Owner.position - this.position).eulerAngles;
			if (GameManager.Instance.World.IsLocalPlayer(this.belongsPlayerId))
			{
				this.HandleNavObject();
				this.hasNavObjectsEnabled = true;
				this.SetOwner(this.OwnerID);
				this.SendSyncData(3);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SyncOwnerData()
	{
		if (this.isOwnerSyncPending)
		{
			this.NotifySyncOwner();
			this.isOwnerSyncPending = false;
		}
		if (this.belongsPlayerId == -1)
		{
			PersistentPlayerList persistentPlayerList = GameManager.Instance.GetPersistentPlayerList();
			if (persistentPlayerList != null)
			{
				PersistentPlayerData playerData = persistentPlayerList.GetPlayerData(this.OwnerID);
				if (playerData != null)
				{
					this.belongsPlayerId = playerData.EntityId;
				}
			}
		}
		if (!this.Owner)
		{
			this.Owner = (EntityAlive)GameManager.Instance.World.GetEntity(this.belongsPlayerId);
			if (this.Owner)
			{
				this.Owner.AddOwnedEntity(this);
				this.currentTarget = this.Owner;
				if (!this.hasNavObjectsEnabled && GameManager.Instance.World.IsLocalPlayer(this.belongsPlayerId))
				{
					this.HandleNavObject();
					this.hasNavObjectsEnabled = true;
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool belongsToPlayerId(int id)
	{
		return this.belongsPlayerId == id;
	}

	public bool isAlly(EntityAlive _target)
	{
		if (this.debugFriendlyFire)
		{
			return false;
		}
		if (this.Owner && this.Owner == _target)
		{
			return true;
		}
		PersistentPlayerList persistentPlayerList = GameManager.Instance.GetPersistentPlayerList();
		PersistentPlayerData playerData = persistentPlayerList.GetPlayerData(this.OwnerID);
		if (playerData != null && persistentPlayerList.EntityToPlayerMap.ContainsKey(_target.entityId))
		{
			PersistentPlayerData persistentPlayerData = persistentPlayerList.EntityToPlayerMap[_target.entityId];
			if (playerData.ACL != null && persistentPlayerData != null && playerData.ACL.Contains(persistentPlayerData.PrimaryId))
			{
				return true;
			}
			EntityPlayer entityPlayer = this.Owner as EntityPlayer;
			EntityPlayer entityPlayer2 = _target as EntityPlayer;
			if (entityPlayer && entityPlayer2 && entityPlayer.Party != null && entityPlayer.Party.ContainsMember(entityPlayer2))
			{
				return true;
			}
		}
		return false;
	}

	public void BuffAllies()
	{
		EntityPlayer entityPlayer = this.Owner as EntityPlayer;
		if (entityPlayer)
		{
			if (entityPlayer.Party != null)
			{
				this.knownPartyMembers = entityPlayer.Party.GetMemberIdArray();
				for (int i = 0; i < this.knownPartyMembers.Length; i++)
				{
					EntityAlive entity = this.world.GetEntity(this.knownPartyMembers[i]) as EntityAlive;
					this.ProcBuffRange(entity);
				}
				return;
			}
			if (this.knownPartyMembers != null && this.knownPartyMembers.Length != 0)
			{
				for (int j = 0; j < this.knownPartyMembers.Length; j++)
				{
					EntityAlive entity2 = this.world.GetEntity(this.knownPartyMembers[j]) as EntityAlive;
					this.removeSupportBuff(entity2);
				}
				this.knownPartyMembers = null;
			}
			this.ProcBuffRange(entityPlayer);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void registerPartyEvents(EntityPlayer player, bool value)
	{
		if (value)
		{
			player.Party.PartyMemberRemoved += this.OnPartyMemberRemoved;
		}
		else
		{
			player.Party.PartyMemberRemoved -= this.OnPartyMemberRemoved;
		}
		this.partyEventsSet = value;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnPartyMemberRemoved(EntityPlayer player)
	{
		this.removeSupportBuff(player);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RemovePartyBuffs(EntityPlayer owner)
	{
		if (owner.Party != null)
		{
			for (int i = 0; i < owner.Party.MemberList.Count; i++)
			{
				EntityAlive entity = owner.Party.MemberList[i];
				this.removeSupportBuff(entity);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ProcBuffRange(EntityAlive entity)
	{
		if (entity)
		{
			if ((this.position - entity.position).magnitude < 32f)
			{
				this.addSupportBuff(entity);
				return;
			}
			this.removeSupportBuff(entity);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void addSupportBuff(EntityAlive entity)
	{
		if (this.state != EntityDrone.State.Shutdown && !entity.Buffs.HasBuff("buffJunkDroneSupportEffect"))
		{
			entity.Buffs.AddBuff("buffJunkDroneSupportEffect", -1, true, false, -1f);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void removeSupportBuff(EntityAlive entity)
	{
		if (entity && entity.Buffs.HasBuff("buffJunkDroneSupportEffect") && !this.doesEntityHaveSupport(entity))
		{
			entity.Buffs.RemoveBuff("buffJunkDroneSupportEffect", true);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool doesEntityHaveSupport(EntityAlive entity)
	{
		OwnedEntityData[] ownedEntities = entity.GetOwnedEntities();
		for (int i = 0; i < ownedEntities.Length; i++)
		{
			EntityDrone entityDrone = this.world.GetEntity(ownedEntities[i].Id) as EntityDrone;
			if (entityDrone && entityDrone.isSupportModAttached)
			{
				return true;
			}
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void doKeypadAction(LocalPlayerUI playerUI)
	{
		this.PlaySound("misc/password_type", 1f);
		GUIWindow window = playerUI.windowManager.GetWindow(XUiC_KeypadWindow.ID);
		window.OnWindowClose = (Action)Delegate.Combine(window.OnWindowClose, new Action(this.StopUIInsteractionSecurity));
		XUiC_KeypadWindow.Open(playerUI, this);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void processRequest(EntityPlayer entityPlayer, int requestType)
	{
		if (requestType >= 0)
		{
			this.interactionRequestType = requestType;
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				this.ValidateInteractingPlayer();
				int entityId = this.interactingPlayerId;
				if (entityId == -1)
				{
					entityId = entityPlayer.entityId;
				}
				this.StartInteraction(entityPlayer.entityId, entityId);
				return;
			}
			this.interactingPlayerId = entityPlayer.entityId;
			this.SendSyncData(4096);
			this.interactingPlayerId = -1;
		}
	}

	public void startDialog(Entity _entityFocusing)
	{
		LocalPlayerUI uiforPlayer = LocalPlayerUI.GetUIForPlayer(_entityFocusing as EntityPlayerLocal);
		uiforPlayer.xui.Dialog.Respondent = this;
		uiforPlayer.windowManager.CloseAllOpenWindows(null, false);
		uiforPlayer.windowManager.Open("dialog", true, false, true);
		this.PlayVO("drone_greeting", false, 1f);
	}

	public bool HasStoredItem(EntityAlive entity, string itemGroupOrName, FastTags<TagGroup.Global> fastTags)
	{
		ItemValue item = ItemClass.GetItem(itemGroupOrName, false);
		bool itemClass = item.ItemClass != null;
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		if (itemClass)
		{
			num = entity.bag.GetItemCount(item, -1, -1, true);
			num2 = entity.inventory.GetItemCount(item, false, -1, -1, true);
			num3 = ((entity.lootContainer != null && entity.lootContainer.HasItem(item)) ? 1 : 0);
		}
		return num + num2 + num3 > 0;
	}

	public ItemStack TakeStoredItem(EntityAlive entity, string itemGroupOrName, FastTags<TagGroup.Global> fastTags)
	{
		ItemValue item = ItemClass.GetItem(itemGroupOrName, false);
		if (item.ItemClass != null)
		{
			entity.bag.GetItemCount(item, -1, -1, true);
			int itemCount = entity.inventory.GetItemCount(item, false, -1, -1, true);
			if (entity.lootContainer != null)
			{
				entity.lootContainer.HasItem(item);
			}
			if (itemCount > 0)
			{
				entity.inventory.DecItem(item, 1, false, null);
			}
			else if (entity.lootContainer != null)
			{
				this.takeFromEntityContainer(entity, itemGroupOrName, fastTags);
			}
			else
			{
				entity.bag.DecItem(item, 1, false, null);
			}
			return new ItemStack(item.Clone(), 1);
		}
		return null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void takeFromEntityContainer(EntityAlive entity, string itemGroupOrName, FastTags<TagGroup.Global> fastTags)
	{
		ItemStack[] array = entity.bag.GetSlots();
		if (entity.lootContainer != null)
		{
			array = entity.lootContainer.GetItems();
		}
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] != null && array[i].itemValue != null && array[i].itemValue.ItemClass != null && array[i].itemValue.ItemClass.HasAnyTags(fastTags) && array[i].count > 0 && array[i].itemValue.ItemClass.Name.ContainsCaseInsensitive(itemGroupOrName))
			{
				array[i].count--;
				if (array[i].count == 0)
				{
					array[i] = ItemStack.Empty.Clone();
				}
				entity.bag.SetSlots(array);
				entity.bag.OnUpdate();
				if (entity.lootContainer != null)
				{
					entity.lootContainer.UpdateSlot(i, array[i]);
				}
			}
		}
	}

	public void OpenStorage(Entity _entityFocusing)
	{
		this.processRequest(_entityFocusing as EntityPlayerLocal, 10);
	}

	public ItemValue GetUpdatedItemValue()
	{
		this.OriginalItemValue.UseTimes = (float)this.OriginalItemValue.MaxUseTimes * (1f - (float)this.Health / base.Stats.Health.BaseMax);
		return this.OriginalItemValue;
	}

	public void Collect(int _playerId)
	{
		EntityPlayerLocal entityPlayerLocal = this.world.GetEntity(_playerId) as EntityPlayerLocal;
		LocalPlayerUI uiforPlayer = LocalPlayerUI.GetUIForPlayer(entityPlayerLocal);
		ItemStack itemStack = new ItemStack(this.GetUpdatedItemValue(), 1);
		if (!uiforPlayer.xui.PlayerInventory.Toolbelt.AddItem(itemStack) && !uiforPlayer.xui.PlayerInventory.AddItem(itemStack))
		{
			GameManager.Instance.ItemDropServer(itemStack, entityPlayerLocal.GetPosition(), Vector3.zero, _playerId, 60f, false);
		}
		this.OriginalItemValue = this.GetUpdatedItemValue();
		base.transform.gameObject.SetActive(false);
		if (this.Owner)
		{
			this.Owner.RemoveOwnedEntity(this.entityId);
			if (DroneManager.Instance != null)
			{
				DroneManager.Instance.RemoveTrackedDrone(this, EnumRemoveEntityReason.Despawned);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void accessInventory(Entity _entityFocusing)
	{
		LocalPlayerUI uiforPlayer = LocalPlayerUI.GetUIForPlayer(_entityFocusing as EntityPlayerLocal);
		uiforPlayer.xui.Dialog.Respondent = this;
		uiforPlayer.windowManager.CloseAllOpenWindows(null, false);
		string lootContainerName = Localization.Get(EntityClass.list[this.entityClass].entityClassName, false);
		GUIWindow window = uiforPlayer.windowManager.GetWindow("looting");
		((XUiC_LootWindowGroup)((XUiWindowGroup)window).Controller).SetTileEntityChest(lootContainerName, this.lootContainer);
		window.OnWindowClose = (Action)Delegate.Combine(window.OnWindowClose, new Action(this.StopUIInteraction));
		uiforPlayer.windowManager.Open("looting", true, false, true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void pickup(Entity _entityFocusing)
	{
		if (!this.lootContainer.IsEmpty())
		{
			this.PlayVO("drone_takefail", true, 1f);
			GameManager.ShowTooltip(this.Owner as EntityPlayerLocal, Localization.Get("ttEmptyVehicleBeforePickup", false), string.Empty, "ui_denied", null, false);
			return;
		}
		ItemStack itemStack = new ItemStack(this.GetUpdatedItemValue(), 1);
		EntityPlayer entityPlayer = _entityFocusing as EntityPlayer;
		if (entityPlayer.inventory.CanTakeItem(itemStack) || entityPlayer.bag.CanTakeItem(itemStack))
		{
			this.isBeingPickedUp = true;
			this.PlaySound(entityPlayer, "drone_take", true, true, 1f);
			this.initWorldValues(false);
			this.nativeCollider.enabled = false;
			GameManager.Instance.CollectEntityServer(this.entityId, entityPlayer.entityId);
			if (entityPlayer.Buffs.HasBuff("buffJunkDroneSupportEffect"))
			{
				entityPlayer.Buffs.RemoveBuff("buffJunkDroneSupportEffect", true);
			}
			this.RemovePartyBuffs(entityPlayer);
			if (entityPlayer.Party != null)
			{
				this.registerPartyEvents(entityPlayer, false);
			}
			this.UnRegsiterMovingLights();
			return;
		}
		GameManager.ShowTooltip(entityPlayer as EntityPlayerLocal, Localization.Get("xuiInventoryFullForPickup", false), string.Empty, "ui_denied", null, false);
	}

	public int StorageCapacity
	{
		get
		{
			return this.lootContainer.items.Length;
		}
	}

	public int GetStoredItemCount()
	{
		int num = 0;
		for (int i = 0; i < this.lootContainer.items.Length; i++)
		{
			if (!this.lootContainer.items[i].IsEmpty())
			{
				num++;
			}
		}
		return num;
	}

	public bool CanRemoveExtraStorage()
	{
		return this.GetStoredItemCount() < this.StorageCapacity - 8;
	}

	public void NotifyToManyStoredItems()
	{
		if (this.overItemLimitCooldown > 0f)
		{
			return;
		}
		this.overItemLimitCooldown = 5f;
		if (!this.CanRemoveExtraStorage())
		{
			GameManager.ShowTooltip(this.Owner as EntityPlayerLocal, Localization.Get("ttJunkDroneEmptySomeStorage", false), string.Empty, "ui_denied", null, false);
			return;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateDroneServiceMenu()
	{
		if (this.overItemLimitCooldown > 0f)
		{
			this.overItemLimitCooldown -= 0.05f;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void move(Vector3 dir)
	{
		this.move(dir, this.currentSpeedFlying);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void move(Vector3 dir, float speedFlying)
	{
		Vector3 end = this.position + dir.normalized * this.physColHeight;
		if (!RaycastPathUtils.IsPositionBlocked(this.position, end, 1073807360, false))
		{
			this.motion += dir * speedFlying * 0.05f;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void moveAlongPath(Vector3 dir)
	{
		this.position + dir.normalized * this.physColHeight;
		this.motion += dir * this.SpeedFlying * 0.05f;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool canMove(Vector3 dir)
	{
		Vector3 end = this.position + dir.normalized * this.physColHeight;
		return !RaycastPathUtils.IsPositionBlocked(this.position, end, 1073807360, false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 rotateToDir(Vector3 dir)
	{
		return Quaternion.Lerp(base.transform.rotation, Quaternion.LookRotation(dir), (1f - Vector3.Angle(base.transform.forward, dir) / 180f) * this.RotationSpeed * 0.05f).eulerAngles;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 rotateToEuler(Vector3 rot)
	{
		return Quaternion.Lerp(base.transform.rotation, Quaternion.Euler(rot), (1f - Vector3.Angle(base.transform.forward, (rot - base.transform.eulerAngles).normalized) / 180f) * this.RotationSpeed * 0.05f).eulerAngles;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void rotateTo(Vector3 dir)
	{
		if (dir != Vector3.zero)
		{
			this.rotation = this.rotateToDir(dir);
		}
	}

	public int GetRepairAmountNeeded()
	{
		return this.GetMaxHealth() - this.Health;
	}

	public void RepairParts(int _amount)
	{
		this.Health += _amount;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void doRepairAction(EntityPlayer entityPlayer, LocalPlayerUI playerUI)
	{
		string text = "resourceRepairKit";
		if (this.HasStoredItem(entityPlayer, text, EntityDrone.repairKitTags))
		{
			playerUI.xui.CollectedItemList.RemoveItemStack(new ItemStack(ItemClass.GetItem(text, false), 1));
			this.PlaySound("crafting/craft_repair_item", 1f);
			this.TakeStoredItem(entityPlayer, text, EntityDrone.repairKitTags);
			this.performRepair();
			this.SendSyncData(16);
			return;
		}
		Manager.PlayInsidePlayerHead("misc/missingitemtorepair", -1, 0f, false, false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void performRepair()
	{
		this.Health = (int)base.Stats.Health.Max;
		this.OriginalItemValue.UseTimes = 0f;
		this.setShutdown(false);
		this.PlayWakeupAnim = true;
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			this.SendSyncData(16);
		}
	}

	public Vector3 HealArmPosition
	{
		get
		{
			return this.healWeapon.WeaponJoint.position + Origin.position;
		}
	}

	public bool TargetCanBeHealed(EntityAlive entity)
	{
		return this.isHealModAttached && this.healWeapon.targetCanBeHealed(entity) && this.HasHealingItem();
	}

	public bool HasHealingItem()
	{
		return this.healWeapon.hasHealingItem();
	}

	public EntityDrone.AllyHealMode HealAllyMode
	{
		get
		{
			return this.allyHealMode;
		}
	}

	public bool IsHealingAllies { get; [PublicizedFrom(EAccessModifier.Private)] set; }

	public void ToggleHealAllies()
	{
		this.IsHealingAllies = !this.IsHealingAllies;
		this.allyHealMode = (this.IsHealingAllies ? EntityDrone.AllyHealMode.HealAllies : EntityDrone.AllyHealMode.DoNotHeal);
	}

	public bool IsHealModAttached
	{
		get
		{
			return this.isHealModAttached;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateNeedsHealItemCheck()
	{
		if (this.needsHealItemTimer > 0f)
		{
			this.needsHealItemTimer -= 0.05f;
		}
		if (this.needsHealItemTimer <= 0f && this.needsHealNotifyCount < 2 && this.checkNotifityNeedsHealItem())
		{
			this.needsHealItemTimer = 30f;
			this.needsHealNotifyCount++;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ClearNeedsHealItemCheck()
	{
		this.needsHealItemTimer = 0f;
		this.needsHealNotifyCount = 0;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool checkNotifityNeedsHealItem()
	{
		if (!this.healWeapon.hasHealingItem())
		{
			GameManager.ShowTooltip(this.Owner as EntityPlayerLocal, Localization.Get("xuiDroneNeedsHealItemsStored", false), string.Empty, "ui_denied", null, false);
			this.PlaySound("drone_empty", 1f);
			return true;
		}
		return false;
	}

	public void Heal()
	{
		this.UpdateNeedsHealItemCheck();
		if (!this.IsHealingAllies)
		{
			if (this.state != EntityDrone.State.Heal && this.healWeapon.canFire() && this.healWeapon.targetNeedsHealing(this.Owner))
			{
				this.HealOwnerServer();
			}
			return;
		}
	}

	public void HealOwner()
	{
		if (this.state != EntityDrone.State.Heal && this.healWeapon.canFire())
		{
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				this.currentTarget = this.Owner;
				this.healTarget(this.Owner);
				return;
			}
			this.setState(EntityDrone.State.Heal);
			this.SendSyncData(32768);
			this.setState(EntityDrone.State.Idle);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HealOwnerServer()
	{
		if (this.state != EntityDrone.State.Heal && this.healWeapon.canFire() && SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			this.currentTarget = this.Owner;
			this.healTarget(this.Owner);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void healTarget(EntityAlive target)
	{
		this.PlayVO("drone_heal", true, 1f);
		base.SetAttackTarget(target, 1200);
		if (this.attackTarget)
		{
			this.setState(EntityDrone.State.Heal);
			this.SendSyncData(32768);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void onHealDone()
	{
		base.SetRevengeTarget(null);
		base.SetAttackTarget(null, 0);
		if (this.Owner)
		{
			this.Owner.Buffs.RemoveBuff("buffJunkDroneHealCooldownEffect", true);
		}
	}

	public EntityDrone.State GetState()
	{
		return this.state;
	}

	public void FollowMode()
	{
		this.PlayVO("drone_command", true, 1f);
		this.setOrders(EntityDrone.Orders.Follow);
		this.setState(EntityDrone.State.Follow);
		this.SendSyncData(49152);
	}

	public void SentryMode()
	{
		this.PlayVO("drone_command", true, 1f);
		this.sentryPos = this.position;
		this.setOrders(EntityDrone.Orders.Stay);
		this.setState(EntityDrone.State.Sentry);
		this.SendSyncData(49152);
		if (this.Owner && this.Owner.HasOwnedEntity(this.entityId))
		{
			this.Owner.GetOwnedEntity(this.entityId).SetLastKnownPosition(this.position);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setState(EntityDrone.State next)
	{
		this.logDrone(string.Format("State: {0} > {1}", this.state, next));
		this.lastState = this.state;
		this.state = next;
		this.stateTime = 0f;
		if (this.lastState == EntityDrone.State.Shutdown)
		{
			Animator componentInChildren = base.GetComponentInChildren<Animator>();
			if (!componentInChildren.enabled)
			{
				componentInChildren.enabled = true;
			}
		}
		switch (this.state)
		{
		case EntityDrone.State.Idle:
		case EntityDrone.State.Sentry:
			break;
		case EntityDrone.State.Follow:
			if (this.lastState == EntityDrone.State.Sentry && this.Owner && this.Owner.HasOwnedEntity(this.entityId))
			{
				this.Owner.GetOwnedEntity(this.entityId).ClearLastKnownPostition();
				return;
			}
			break;
		case EntityDrone.State.Heal:
			this.ClearNeedsHealItemCheck();
			break;
		default:
			return;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void idleState()
	{
		if (this.currentTarget && !this.isEntityAboveOrBelow(this.currentTarget))
		{
			Vector3 headPosition = this.currentTarget.getHeadPosition();
			this.rotateTo(this.steering.GetDir2D(this.position, headPosition));
			float num = 0f;
			if (this.position.y - this.currentTarget.getHeadPosition().y > num || this.position.y - this.currentTarget.getHeadPosition().y < num)
			{
				Vector3 position = this.position;
				position.y = this.currentTarget.getHeadPosition().y;
				this.move(this.steering.Seek(this.position, position, this.SpeedFlying));
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void sentryState()
	{
		Vector3 vector = this.sentryPos;
		if (this.world.IsChunkAreaLoaded(vector) && !this.steering.IsInRange(vector, 0.25f))
		{
			Vector3 dir = this.steering.Seek(this.position, vector, 0.25f);
			this.move(dir);
			return;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 findOpenFollowPoints(bool debugDraw = false)
	{
		this.currentTarget.GetLookVector().y = 0f;
		Vector3[] array = this.getGroupPositions(this.currentTarget, this.FollowDistance + 1f, false);
		Array.Sort<Vector3>(array, (Vector3 x, Vector3 y) => Vector3.Distance(this.position, x).CompareTo(Vector3.Distance(this.position, y)));
		this.hasOpenGroupPos = false;
		Vector3 vector = this.currentTarget.getHeadPosition();
		for (int i = 0; i < array.Length; i++)
		{
			Vector3i vector3i = World.worldToBlockPos(array[i]);
			if (!RaycastPathUtils.IsPointBlocked(array[i], this.currentTarget.getHeadPosition(), 1073807360, true, 0f) && !RaycastPathUtils.IsPointBlocked(this.currentTarget.getHeadPosition(), array[i], 1073807360, true, 0f))
			{
				RaycastNode raycastNode = RaycastPathWorldUtils.FindNodeType(RaycastPathWorldUtils.ScanVolume(this.world, vector3i.ToVector3Center(), true, false, false, 0f), cPathNodeType.Air);
				if (raycastNode != null)
				{
					if (!this.hasOpenGroupPos)
					{
						vector = raycastNode.Center;
						this.hasOpenGroupPos = true;
					}
					if (debugDraw)
					{
						RaycastPathUtils.DrawNode(new RaycastNode(vector3i.ToVector3Center(), 1f, 0), Color.yellow, 0f);
					}
				}
			}
			else if (debugDraw)
			{
				RaycastPathUtils.DrawNode(new RaycastNode(vector3i.ToVector3Center(), 1f, 0), Color.red, 0f);
			}
		}
		Vector3[] groupFallbackPositions = this.getGroupFallbackPositions(this.currentTarget, this.FollowDistance + 1f, false);
		Array.Sort<Vector3>(groupFallbackPositions, (Vector3 x, Vector3 y) => Vector3.Distance(this.position, x).CompareTo(Vector3.Distance(this.position, y)));
		for (int j = 0; j < groupFallbackPositions.Length; j++)
		{
			Vector3i vector3i2 = World.worldToBlockPos(groupFallbackPositions[j]);
			if (!RaycastPathUtils.IsPointBlocked(groupFallbackPositions[j], this.currentTarget.getHeadPosition(), 1073807360, true, 0f) && !RaycastPathUtils.IsPointBlocked(this.currentTarget.getHeadPosition(), groupFallbackPositions[j], 1073807360, true, 0f))
			{
				RaycastNode raycastNode2 = RaycastPathWorldUtils.FindNodeType(RaycastPathWorldUtils.ScanVolume(this.world, vector3i2.ToVector3Center(), true, false, false, 0f), cPathNodeType.Air);
				if (raycastNode2 != null)
				{
					if (!this.hasOpenGroupPos)
					{
						vector = raycastNode2.Center;
						this.hasOpenGroupPos = true;
					}
					if (debugDraw)
					{
						RaycastPathUtils.DrawNode(new RaycastNode(vector3i2.ToVector3Center(), 1f, 0), Color.yellow, 0f);
					}
				}
			}
			else if (debugDraw)
			{
				RaycastPathUtils.DrawNode(new RaycastNode(vector3i2.ToVector3Center(), 1f, 0), Color.red, 0f);
			}
		}
		if (!this.hasOpenGroupPos)
		{
			Vector3i vector3i3 = World.worldToBlockPos(this.currentTarget.getHeadPosition());
			RaycastNode raycastNode3 = RaycastPathWorldUtils.FindNodeType(RaycastPathWorldUtils.ScanVolume(this.world, vector3i3.ToVector3Center(), false, false, false, 0f), cPathNodeType.Air);
			if (raycastNode3 != null)
			{
				vector = raycastNode3.Center;
			}
		}
		this.followTargetPos = vector;
		if (debugDraw)
		{
			RaycastPathUtils.DrawBounds(World.worldToBlockPos(this.followTargetPos), Color.green, 0f, 1f);
		}
		return this.followTargetPos;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void makePath()
	{
		if (this.pathMan.Path == null)
		{
			this.pathMan.CreatePath(this.position + Vector3.up, this.currentTarget.getHeadPosition() - this.currentTarget.GetForwardVector() * 2f + Vector3.up, this.SpeedFlying, false, 0f);
		}
		if (this.pathMan.isBuildingPath)
		{
			return;
		}
		if (this.pathMan.Path != null && this.pathMan.Path.Nodes.Count > 0 && this.nodePath.Count == 0)
		{
			this.nodePath.AddRange(this.pathMan.Path.Nodes);
			this.nodePath.Reverse();
		}
		if (this.nodePath.Count > 0)
		{
			if ((this.pathMan.Path.Target - this.currentTarget.getHeadPosition()).magnitude > this.FollowDistance)
			{
				this.pathMan.Clear();
				this.nodePath.Clear();
				return;
			}
			Vector3 dir = this.steering.Seek(this.position, this.nodePath[0].Center, 0f);
			this.rotateTo(dir);
			this.move(dir, this.SpeedPathing);
			if (this.steering.IsInRange(this.nodePath[0].Center, 0.5f))
			{
				this.nodePath.RemoveAt(0);
				if (this.nodePath.Count == 0)
				{
					this.transitionToIdle = true;
				}
			}
		}
		if (this.transitionToIdle)
		{
			this.transitionToIdleTime -= 0.05f;
			if (this.transitionToIdleTime <= 0f)
			{
				this.transitionToIdleTime = 0.5f;
				this.pathMan.Clear();
				this.setState(EntityDrone.State.Idle);
				this.transitionToIdle = false;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void clearCurrentPath()
	{
		this.currentPath.Clear();
		this.debugPathDelay = 2f;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void followState()
	{
		if (!this.currentTarget)
		{
			this.currentTarget = this.Owner;
		}
		if (this.isTargetUnderWater(this.currentTarget.getHeadPosition()))
		{
			if (this.currentPath.Count > 0)
			{
				this.clearCurrentPath();
			}
			Vector3 dir = this.steering.Seek(this.position, this.findOpenBlockAbove(this.currentTarget.getHeadPosition(), 256), 0.2f);
			this.rotateTo(dir);
			this.move(dir);
			return;
		}
		float magnitude = (this.currentTarget.getHeadPosition() - this.position).magnitude;
		this.findOpenFollowPoints(true);
		bool flag = !RaycastPathUtils.IsPointBlocked(this.position, this.followTargetPos, 1073807360, true, 0f);
		if (this.debugPathTiming && this.debugPathDelay > 0f)
		{
			this.debugPathDelay -= Time.deltaTime;
			return;
		}
		if (!flag && this.currentPath.Count == 0)
		{
			this.getPath(this.currentTarget.position);
			return;
		}
		if (this.currentPath.Count <= 0)
		{
			Vector3 a = this.steering.Seek(this.position, this.followTargetPos, this.SpeedFlying);
			if (!this.steering.IsInRange(this.followTargetPos, 0.1f))
			{
				if (magnitude > this.FollowDistance && magnitude < 24f && !RaycastPathUtils.IsPointBlocked(this.position, this.currentTarget.position, 1073807360, false, 0f) && Vector3.Angle(this.currentTarget.GetLookVector(), this.position - this.currentTarget.getHeadPosition()) < 45f)
				{
					float d = 0.5f;
					Vector3 vector = this.steering.Flee(this.position, this.currentTarget.getHeadPosition(), this.SpeedFlying);
					if (!RaycastPathUtils.IsPositionBlocked(this.position, this.position + (a + vector), 1073807360, false))
					{
						a += vector * d;
					}
				}
				if (this.steering.GetAltitude(this.position) < magnitude * 0.33f && !RaycastPathUtils.IsPositionBlocked(this.position, this.position + Vector3.up, 1073807360, false))
				{
					float d2 = 0.75f;
					Vector3 a2 = this.steering.Seek(this.position, this.position + Vector3.up, this.SpeedFlying);
					a += a2 * d2;
				}
				this.rotateTo((this.currentTarget.getHeadPosition() - this.position).normalized);
				this.move(a.normalized);
			}
			if (this.state == EntityDrone.State.Follow && this.steering.IsInRange(this.followTargetPos, 0.5f))
			{
				this.debugPathDelay = 2f;
				this.setState(EntityDrone.State.Idle);
			}
			return;
		}
		this.currentPathTarget = this.currentPath[0];
		Vector3 dir2 = this.steering.Seek(this.position, this.currentPathTarget, 1f);
		this.rotateTo((this.currentPathTarget - this.position).normalized);
		this.move(dir2);
		if (this.steering.IsInRange(this.currentPathTarget, 0.5f))
		{
			this.currentPath.RemoveAt(0);
			return;
		}
		if (this.currentPath.Count > 1)
		{
			RaycastPathUtils.DrawLine(this.currentPath[0], this.currentPath[1], Color.green, 1f);
		}
		if (!this.IsStuckInBlock() && !this.IsNotAbleToReachTarget(this.currentPath[0]))
		{
			if (!RaycastPathUtils.IsPointBlocked(this.position, this.followTargetPos, 1073807360, true, 0f))
			{
				this.clearCurrentPath();
			}
			return;
		}
		if (this.currentPath.Count > 1)
		{
			this.teleportToPosition(this.currentPath[1]);
			this.currentPath.RemoveRange(0, 2);
			return;
		}
		this.teleportToPosition(this.currentPath[0]);
		this.currentPath.RemoveAt(0);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void getPath(Vector3 target)
	{
		if (this.findPath(this.position, target, false))
		{
			this.clearCurrentPath();
			this.currentPath.AddRange(this.projectedPath);
			this.currentPath.RemoveAt(0);
			for (int i = 0; i < this.currentPath.Count; i++)
			{
				Vector3 value = this.currentPath[i];
				value.y += 1.5f;
				this.currentPath[i] = value;
			}
			List<RaycastNode> list = RaycastPathWorldUtils.ScanPath(this.world, this.position, this.currentPath, false, false, 0f);
			for (int j = 0; j < this.currentPath.Count; j++)
			{
				Vector3 a = this.currentPath[j];
				RaycastNode raycastNode = list[j];
				if (raycastNode.FlowToWaypoint)
				{
					this.currentPath[j] = (a + raycastNode.Waypoint.Center) * 0.5f;
				}
			}
			for (int k = 0; k < this.currentPath.Count - 1; k++)
			{
				Color endColor = Color.cyan;
				if (RaycastPathUtils.IsPointBlocked(this.currentPath[k], this.currentPath[k + 1], 1073807360, false, 0f))
				{
					endColor = Color.magenta;
				}
				Utils.DrawLine(this.currentPath[k] - Origin.position, this.currentPath[k + 1] - Origin.position, Color.white, endColor, 100, 10f);
			}
			this.projectedPath.Clear();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool findPath(Vector3 start, Vector3 end, bool debugDraw = false)
	{
		PathFinderThread instance = PathFinderThread.Instance;
		if (instance == null)
		{
			return false;
		}
		PathInfo path = instance.GetPath(this.entityId);
		if (path.path == null && !PathFinderThread.Instance.IsCalculatingPath(this.entityId))
		{
			PathFinderThread.Instance.FindPath(this, start, end, this.SpeedFlying, false, null);
			return false;
		}
		if (path.path == null)
		{
			return false;
		}
		for (int i = 0; i < path.path.points.Length; i++)
		{
			Vector3 projectedLocation = path.path.points[i].projectedLocation;
			this.projectedPath.Add(projectedLocation);
		}
		if (debugDraw)
		{
			for (int j = 0; j < this.projectedPath.Count - 1; j++)
			{
				Utils.DrawLine(this.projectedPath[j] - Origin.position, this.projectedPath[j + 1] - Origin.position, Color.white, Color.cyan, 100, 10f);
			}
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void healState()
	{
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			return;
		}
		if (!this.currentTarget)
		{
			this.onHealDone();
			return;
		}
		this.rotateTo(this.currentTarget.position - this.position);
		float magnitude = (this.position - this.currentTarget.getHeadPosition()).magnitude;
		if (magnitude > this.FollowDistance)
		{
			this.followState();
		}
		if (magnitude <= this.FollowDistance && this.healWeapon.canFire())
		{
			this.healWeapon.RegisterOnFireComplete(new Action(this.onHealDone));
			this.healWeapon.Fire(this.currentTarget);
			return;
		}
		if (this.healWeapon.hasActionCompleted())
		{
			this.setState(EntityDrone.State.Idle);
			this.SendSyncData(32768);
		}
	}

	public void DebugUnstuck()
	{
		this.teleportState();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void teleportState()
	{
		if (!this.isTeleporting)
		{
			Log.Out("Drone.teleportState() - {0}", new object[]
			{
				this.entityId
			});
			this.setState(EntityDrone.State.Teleport);
			Vector3 vector = this.Owner.getHeadPosition() - new Vector3(this.Owner.GetLookVector().x, 0f, this.Owner.GetLookVector().z) * this.FollowDistance;
			this.isTeleporting = true;
			this.clearCurrentPath();
			this.motion = Vector3.zero;
			this.SetPosition(vector, true);
			this.orderState = EntityDrone.Orders.Follow;
			base.StartCoroutine(this.validateTeleport(vector));
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator validateTeleport(Vector3 target)
	{
		yield return new WaitForSeconds(1f);
		if (this.Owner)
		{
			if (this.isOutOfRange(this.Owner.position, this.MaxDistFromOwner))
			{
				Log.Out("teleport failed");
			}
			else if (!this.isOutOfRange(target, this.FollowDistance * 1.5f))
			{
				Log.Out("teleport success!");
			}
		}
		this.isTeleporting = false;
		this.setState(EntityDrone.State.Idle);
		yield return null;
		yield break;
	}

	public override void SetPosition(Vector3 _pos, bool _bUpdatePhysics = true)
	{
		base.SetPosition(_pos, _bUpdatePhysics);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void teleportToPosition(Vector3 telePos)
	{
		this.motion = Vector3.zero;
		Utils.DrawLine(telePos, this.position, Color.yellow, Color.green, 100, 20f);
		this.SetPosition(telePos, true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void performShutdown()
	{
		if (this.Owner)
		{
			Manager.Stop(this.Owner.entityId, "drone_take");
		}
		this.PlayVO("drone_shutdown", true, 1f);
		if (this.Owner && this.Owner.HasOwnedEntity(this.entityId))
		{
			this.Owner.GetOwnedEntity(this.entityId).SetLastKnownPosition(this.position);
		}
		this.setShutdown(true);
		this.isShutdownPending = false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setShutdown(bool value)
	{
		base.GetComponentInChildren<Animator>().enabled = !value;
		this.PhysicsTransform.gameObject.SetActive(!value);
		this.IsNoCollisionMode.Value = value;
		if (value)
		{
			base.SetRevengeTarget(null);
			base.SetAttackTarget(null, 0);
			this.setState(EntityDrone.State.Shutdown);
			Handle handle = this.idleLoop;
			if (handle != null)
			{
				handle.Stop(this.entityId);
			}
			this.idleLoop = null;
			return;
		}
		this.isShutdown = value;
		this.isGrounded = value;
		if (this.orderState == EntityDrone.Orders.Stay)
		{
			this.setState(EntityDrone.State.Sentry);
		}
		else
		{
			this.setState(EntityDrone.State.Idle);
		}
		if (this.Owner && this.Owner.HasOwnedEntity(this.entityId))
		{
			this.Owner.GetOwnedEntity(this.entityId).ClearLastKnownPostition();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setShutdownDestruction(bool value)
	{
		base.transform.FindInChilds("p_smokeLeft", false).gameObject.SetActive(value);
		base.transform.FindInChilds("p_smokeRight", false).gameObject.SetActive(value);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void processShutdown()
	{
		this.fallBlockPos.RoundToInt(this.position - this.blockHeightOffset);
		RaycastHit raycastHit;
		if ((!this.hasFallPoint || this.world.GetBlock(this.fallBlockPos).isair) && Physics.Raycast(this.position - Origin.position + this.blockHeightOffset, Vector3.down, out raycastHit, 999f, 268500992))
		{
			this.fallPoint = raycastHit.point;
			this.isShutdown = true;
			this.isGrounded = false;
			this.hasFallPoint = true;
		}
		if (this.isGrounded)
		{
			return;
		}
		if (this.isShutdown)
		{
			Vector3 position = this.position;
			float num = Mathf.Min(1f + Vector3.Distance(this.position, this.fallPoint + Origin.position), 5f);
			if (num < 1.2f)
			{
				this.isGrounded = true;
				return;
			}
			position.y -= num * 0.05f;
			position.y = Mathf.Max(position.y, this.fallPoint.y);
			this.SetPosition(position, true);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateDroneSystems()
	{
		if (this.sensors != null)
		{
			this.sensors.TargetInRange();
			this.sensors.Update();
		}
		if (this.healWeapon != null && this.isHealModAttached)
		{
			if (this.state != EntityDrone.State.Shutdown && this.state != EntityDrone.State.Sentry && this.state != EntityDrone.State.Heal && this.healWeapon.targetNeedsHealing(this.Owner))
			{
				this.Heal();
			}
			this.healWeapon.Update();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateState()
	{
		this.stateTime += 0.05f;
		switch (this.state)
		{
		case EntityDrone.State.Idle:
			if (this.isTargetUnderWater(this.Owner.getHeadPosition()))
			{
				this.currentTarget = this.Owner;
				this.setState(EntityDrone.State.Follow);
				return;
			}
			if (!this.steering.IsInRange(this.Owner.getHeadPosition(), this.FollowDistance + 1f))
			{
				this.currentTarget = this.Owner;
				this.setState(EntityDrone.State.Follow);
				return;
			}
			this.idleState();
			return;
		case EntityDrone.State.Sentry:
			this.sentryState();
			break;
		case EntityDrone.State.Follow:
			this.followState();
			return;
		case EntityDrone.State.Heal:
			this.healState();
			return;
		case EntityDrone.State.Attack:
		case EntityDrone.State.Shutdown:
		case EntityDrone.State.NoClip:
			break;
		default:
			return;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void logDrone(string _log)
	{
		if (DroneManager.DebugLogEnabled)
		{
			Type type = base.GetType();
			Log.Out(((type != null) ? type.ToString() : null) + " " + _log);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void debugUpdate()
	{
		this.updateDebugName();
		if (this.debugCamera)
		{
			if (this.currentTarget && this.currentPath.Count == 0)
			{
				this.debugCamera.transform.LookAt(this.currentTarget.emodel.GetHeadTransform());
				return;
			}
			this.debugCamera.transform.forward = base.transform.forward;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateDebugName()
	{
		this.aiManager.UpdateDebugName();
	}

	public bool IsVisible
	{
		get
		{
			return this.isVisible;
		}
		set
		{
			if (value != this.isVisible)
			{
				Renderer[] componentsInChildren = this.emodel.gameObject.GetComponentsInChildren<Renderer>(true);
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					componentsInChildren[i].enabled = value;
				}
				this.isVisible = value;
			}
		}
	}

	public int AmmoCount
	{
		get
		{
			return this.OriginalItemValue.Meta;
		}
		set
		{
			this.OriginalItemValue.Meta = value;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setNoClip(bool value)
	{
		this.IsNoCollisionMode.Value = value;
		this.PhysicsTransform.gameObject.layer = (value ? 14 : 15);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public List<EntityAlive> getAvoidEntities(float distance)
	{
		List<EntityAlive> list = new List<EntityAlive>();
		List<Entity> entitiesInBounds = GameManager.Instance.World.GetEntitiesInBounds(this, new Bounds(this.position, Vector3.one * distance));
		for (int i = 0; i < entitiesInBounds.Count; i++)
		{
			EntityAlive entityAlive = entitiesInBounds[i] as EntityAlive;
			if (entityAlive != null && entitiesInBounds[i].EntityClass != null && !(entityAlive is EntityNPC) && (!entityAlive.IsSleeper || !entityAlive.IsSleeping))
			{
				list.Add(entitiesInBounds[i] as EntityAlive);
			}
		}
		return list;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isOutOfRange(Vector3 _target, float _distance)
	{
		return (this.position - _target).sqrMagnitude > _distance * _distance;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool IsAnyPlayerWithingDist(float dist)
	{
		PersistentPlayerList persistentPlayerList = GameManager.Instance.GetPersistentPlayerList();
		if (((persistentPlayerList != null) ? persistentPlayerList.Players : null) != null)
		{
			foreach (KeyValuePair<PlatformUserIdentifierAbs, PersistentPlayerData> keyValuePair in persistentPlayerList.Players)
			{
				EntityPlayer entityPlayer = this.world.GetEntity(keyValuePair.Value.EntityId) as EntityPlayer;
				if (entityPlayer && (entityPlayer.getChestPosition() - this.position).magnitude <= dist)
				{
					return true;
				}
			}
			return false;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i getBlockPosition(Vector3 worldPos)
	{
		Vector3i one = new Vector3i(worldPos);
		Vector3 v = worldPos - one.ToVector3Center();
		return one + Vector3i.FromVector3Rounded(v);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 findOpenBlockAbove(Vector3 targetPosition, int maxHeight = 256)
	{
		Vector3i vector3i = this.getBlockPosition(targetPosition);
		vector3i += Vector3i.up;
		int num = 1;
		BlockValue block = this.world.GetBlock(vector3i);
		while (!block.isair && num < maxHeight)
		{
			num++;
			vector3i += Vector3i.up;
			block = this.world.GetBlock(vector3i);
		}
		return vector3i.ToVector3Center();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool IsStuckInBlock()
	{
		this.currentBlockPosition.RoundToInt(this.position);
		this.timeInBlock += Time.deltaTime;
		if (this.currentBlockPosition != this.lastBlockPosition)
		{
			this.lastBlockPosition = this.currentBlockPosition;
			this.timeInBlock = 0f;
		}
		return this.timeInBlock > 0.5f;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool IsNotAbleToReachTarget(Vector3 currentTarget)
	{
		this.timeSpentToNextTarget += Time.deltaTime;
		if (this.targetDestination != currentTarget)
		{
			this.targetDestination = currentTarget;
			this.timeSpentToNextTarget = 0f;
		}
		if (this.timeSpentToNextTarget > 1f)
		{
			this.timeSpentToNextTarget = 0f;
			return true;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isEntityAboveOrBelow(Entity entity)
	{
		bool result = false;
		Vector3 normalized = (this.position - entity.getHeadPosition()).normalized;
		float num = this.position.x - entity.position.x;
		float num2 = this.position.z - entity.position.z;
		float num3 = this.position.y - entity.getHeadPosition().y;
		if (num > -0.85f && num < 0.85f && num2 > -0.85f && num2 < 0.85f && (num3 < -1.2f || num3 > 1.2f))
		{
			result = true;
		}
		return result;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isTargetUnderWater(Vector3 targetPosition)
	{
		Vector3i blockPosition = this.getBlockPosition(targetPosition);
		return this.world.GetBlock(blockPosition).type == 240;
	}

	public Vector3 getPositionOnGround()
	{
		return this.getPositionOnGround(this.position);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 getPositionOnGround(Vector3 pos)
	{
		RaycastHit raycastHit;
		if (Physics.Raycast(pos - Origin.position, Vector3.down, out raycastHit, 255f, 65536))
		{
			return raycastHit.point + Origin.position;
		}
		return this.position;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3[] getGroupPositions(EntityAlive _entity, float followDist, bool debugDraw = false)
	{
		float d = 0.67f;
		Vector3 headPosition = _entity.getHeadPosition();
		Vector3 lookVector = _entity.GetLookVector();
		lookVector.y = 0f;
		this.groupPositions[0] = headPosition - lookVector * followDist;
		Vector3 normalized = (_entity.transform.right - lookVector).normalized;
		normalized.y = 0f;
		this.groupPositions[1] = headPosition + normalized * followDist * d;
		Vector3 normalized2 = (_entity.transform.right + lookVector).normalized;
		normalized2.y = 0f;
		this.groupPositions[2] = headPosition - normalized2 * followDist * d;
		return this.groupPositions;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3[] getGroupFallbackPositions(EntityAlive _entity, float followDist, bool debugDraw = false)
	{
		float d = 0.67f;
		Vector3 headPosition = _entity.getHeadPosition();
		Vector3 vector = -_entity.GetLookVector();
		vector.y = 0f;
		this.fallbackGroupPos[0] = headPosition - vector * followDist;
		Vector3 normalized = (_entity.transform.right - vector).normalized;
		normalized.y = 0f;
		this.fallbackGroupPos[1] = headPosition + normalized * followDist * d;
		Vector3 normalized2 = (_entity.transform.right + vector).normalized;
		normalized2.y = 0f;
		this.fallbackGroupPos[2] = headPosition - normalized2 * followDist * d;
		return this.fallbackGroupPos;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public float getTargetView(EntityAlive target, float degrees, float weight)
	{
		Vector3 lookVector = target.GetLookVector();
		Vector3 to = this.position - target.position;
		float num = Vector3.Angle(lookVector, to);
		if (num < degrees * 0.5f)
		{
			return (1f - num / degrees * 0.5f) * weight;
		}
		return 0f;
	}

	public void OnWakeUp()
	{
	}

	public void NotifyOffTheWorld()
	{
	}

	public override string MakeDebugNameInfo()
	{
		return string.Format("\nState: {0}", this.state.ToStringCached<EntityDrone.State>());
	}

	public bool IsFrendlyFireEnabled
	{
		get
		{
			return this.debugFriendlyFire;
		}
	}

	public void DebugToggleFriendlyFire()
	{
		this.debugFriendlyFire = !this.debugFriendlyFire;
	}

	public bool IsDebugCameraEnabled
	{
		get
		{
			return this.debugShowCamera;
		}
	}

	public void DebugToggleDebugCamera()
	{
		this._prepareDebugCamera();
		this.debugShowCamera = !this.debugShowCamera;
	}

	public void SetDebugCameraEnabled(bool value)
	{
		this._prepareDebugCamera();
		this.debugShowCamera = value;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void _prepareDebugCamera()
	{
		if (this.debugShowCamera && this.debugCamera)
		{
			UnityEngine.Object.Destroy(this.debugCamera);
			return;
		}
		this.debugCamera = new GameObject("Camera");
		this.debugCamera.transform.SetParent(base.transform);
		this.debugCamera.transform.localPosition = Vector3.zero;
		this.debugCamera.transform.localRotation = Quaternion.identity;
		Camera camera = this.debugCamera.AddComponent<Camera>();
		Rect rect = camera.rect;
		float num = 0.35f;
		rect.width = num;
		rect.height = num;
		float num2 = 1f - num;
		rect.x = num2;
		rect.y = num2;
		camera.rect = rect;
		camera.farClipPlane = 32f;
	}

	public void Debug_ToggleReconMode()
	{
		this._prepareReconCam();
		DroneManager.Debug_LocalControl = !DroneManager.Debug_LocalControl;
		EntityPlayerLocal entityPlayerLocal = this.Owner as EntityPlayerLocal;
		entityPlayerLocal.PlayerUI.windowManager.SetHUDEnabled(DroneManager.Debug_LocalControl ? GUIWindowManager.HudEnabledStates.FullHide : GUIWindowManager.HudEnabledStates.Enabled);
		entityPlayerLocal.bEntityAliveFlagsChanged = true;
		entityPlayerLocal.IsGodMode.Value = DroneManager.Debug_LocalControl;
		entityPlayerLocal.IsNoCollisionMode.Value = DroneManager.Debug_LocalControl;
		entityPlayerLocal.IsFlyMode.Value = DroneManager.Debug_LocalControl;
		if (entityPlayerLocal.IsGodMode.Value)
		{
			entityPlayerLocal.Buffs.AddBuff("god", -1, true, false, -1f);
		}
		else if (!GameManager.Instance.World.IsEditor() && !GameModeCreative.TypeName.Equals(GamePrefs.GetString(EnumGamePrefs.GameMode)))
		{
			entityPlayerLocal.Buffs.RemoveBuff("god", true);
		}
		entityPlayerLocal.IsSpectator = DroneManager.Debug_LocalControl;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void _prepareReconCam()
	{
		if (DroneManager.Debug_LocalControl && this.reconCam)
		{
			UnityEngine.Object.Destroy(this.reconCam.gameObject);
			return;
		}
		GameObject gameObject = new GameObject(this.Owner.EntityName + "-Drone|Recon");
		gameObject.transform.SetParent(base.transform);
		gameObject.transform.localPosition = Vector3.zero;
		gameObject.transform.localRotation = Quaternion.identity;
		this.reconCam = gameObject.AddComponent<Camera>();
	}

	public ushort GetSyncFlagsReplicated(ushort syncFlags)
	{
		return syncFlags & 2;
	}

	public void SendSyncData(ushort syncFlags)
	{
		int primaryPlayerId = GameManager.Instance.World.GetPrimaryPlayerId();
		this.SendSyncData(syncFlags, primaryPlayerId);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SendSyncData(ushort syncFlags, int playerId)
	{
		EntityDrone.NetPackageDroneDataSync package = NetPackageManager.GetPackage<EntityDrone.NetPackageDroneDataSync>().Setup(this, playerId, syncFlags);
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(package, false);
			return;
		}
		SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(package, false, -1, -1, -1, null, 192);
	}

	public void WriteSyncData(BinaryWriter _bw, ushort syncFlags)
	{
		_bw.Write(0);
		if ((syncFlags & 1) > 0)
		{
			this.OwnerID.ToStream(_bw, false);
			_bw.Write(this.Health);
		}
		if ((syncFlags & 16384) > 0)
		{
			_bw.Write((byte)this.OrderState);
			if (this.OrderState == EntityDrone.Orders.Stay)
			{
				float[] array = new float[]
				{
					this.sentryPos.x,
					this.sentryPos.y,
					this.sentryPos.z
				};
				for (int i = 0; i < array.Length; i++)
				{
					_bw.Write(array[i]);
				}
			}
		}
		if ((syncFlags & 32768) > 0)
		{
			_bw.Write((byte)this.state);
		}
		if ((syncFlags & 2) > 0)
		{
			byte b = 0;
			if (this.isInteractionLocked)
			{
				b |= 1;
			}
			if (this.isLocked)
			{
				b |= 2;
			}
			_bw.Write(b);
			this.ownerSteamId.ToStream(_bw, false);
			_bw.Write(this.passwordHash);
			_bw.Write((byte)this.allowedUsers.Count);
			for (int j = 0; j < this.allowedUsers.Count; j++)
			{
				this.allowedUsers[j].ToStream(_bw, false);
			}
		}
		if ((syncFlags & 8) > 0)
		{
			ItemStack[] slots = this.bag.GetSlots();
			_bw.Write((byte)slots.Length);
			for (int k = 0; k < slots.Length; k++)
			{
				slots[k].Write(_bw);
			}
		}
		if ((syncFlags & 4096) > 0)
		{
			_bw.Write(this.interactingPlayerId);
		}
		if ((syncFlags & 32) > 0)
		{
			_bw.Write(this.isQuietMode);
		}
		if ((syncFlags & 64) > 0)
		{
			_bw.Write(this.isFlashlightOn);
		}
		if ((syncFlags & 128) > 0)
		{
			this.OriginalItemValue.Write(_bw);
		}
	}

	public void ReadSyncData(BinaryReader _br, ushort syncFlags, int senderId)
	{
		byte b = _br.ReadByte();
		if ((syncFlags & 4) > 0 && (b & 8) > 0)
		{
			this.OriginalItemValue.Read(_br);
			this.LoadMods();
		}
		if ((syncFlags & 1) > 0)
		{
			this.OwnerID = PlatformUserIdentifierAbs.FromStream(_br, false, false);
			this.Health = _br.ReadInt32();
		}
		if ((syncFlags & 16384) > 0)
		{
			EntityDrone.Orders orders = (EntityDrone.Orders)_br.ReadByte();
			if (orders == EntityDrone.Orders.Stay)
			{
				this.sentryPos.x = _br.ReadSingle();
				this.sentryPos.y = _br.ReadSingle();
				this.sentryPos.z = _br.ReadSingle();
			}
			this.setOrders(orders);
			if (GameManager.IsDedicatedServer)
			{
				this.SendSyncData(16384, senderId);
			}
		}
		if ((syncFlags & 32768) > 0)
		{
			byte b2 = _br.ReadByte();
			this.transitionState = (EntityDrone.State)b2;
			this.logDrone("Read Transition State: " + this.transitionState.ToString());
		}
		if ((syncFlags & 2) > 0)
		{
			byte b3 = _br.ReadByte();
			this.isInteractionLocked = ((b3 & 1) > 0);
			this.isLocked = ((b3 & 2) > 0);
			this.ownerSteamId = PlatformUserIdentifierAbs.FromStream(_br, false, false);
			this.passwordHash = _br.ReadInt32();
			this.allowedUsers.Clear();
			int num = (int)_br.ReadByte();
			for (int i = 0; i < num; i++)
			{
				this.allowedUsers.Add(PlatformUserIdentifierAbs.FromStream(_br, true, false));
			}
		}
		if ((syncFlags & 8) > 0)
		{
			int num2 = (int)_br.ReadByte();
			ItemStack[] array = new ItemStack[num2];
			for (int j = 0; j < num2; j++)
			{
				ItemStack itemStack = new ItemStack();
				array[j] = itemStack.Read(_br);
				this.lootContainer.UpdateSlot(j, array[j]);
			}
			this.bag.SetSlots(array);
			this.bag.OnUpdate();
		}
		if ((syncFlags & 4096) > 0)
		{
			int requestId = _br.ReadInt32();
			this.CheckInteractionRequest(senderId, requestId);
		}
		if ((syncFlags & 16) > 0)
		{
			this.performRepair();
			Log.Warning("Read Repair Action: " + 16.ToString());
		}
		if ((syncFlags & 32) > 0)
		{
			this.isQuietMode = _br.ReadBoolean();
			if (this.isQuietMode)
			{
				Handle handle = this.idleLoop;
				if (handle != null)
				{
					handle.Stop(this.entityId);
				}
				this.idleLoop = null;
			}
		}
		if ((syncFlags & 64) > 0)
		{
			this.isFlashlightOn = _br.ReadBoolean();
			this.setFlashlightOn(this.isFlashlightOn);
		}
		if ((syncFlags & 128) > 0)
		{
			this.OriginalItemValue.Read(_br);
			this.LoadMods();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CheckInteractionRequest(int _playerId, int _requestId)
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			if (_requestId != -1)
			{
				this.ValidateInteractingPlayer();
				ushort num = 4096;
				if (this.interactingPlayerId == -1)
				{
					this.interactingPlayerId = _playerId;
					num |= 2;
				}
				this.SendSyncData(num, _playerId);
				return;
			}
			if (this.interactingPlayerId == _playerId)
			{
				this.interactingPlayerId = -1;
				return;
			}
		}
		else
		{
			this.StartInteraction(_playerId, _requestId);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void StartInteraction(int _playerId, int _requestId)
	{
		EntityPlayerLocal localPlayerFromID = GameManager.Instance.World.GetLocalPlayerFromID(_playerId);
		if (!localPlayerFromID)
		{
			return;
		}
		if (_requestId != _playerId)
		{
			GameManager.ShowTooltip(localPlayerFromID, Localization.Get("ttVehicleInUse", false), string.Empty, "ui_denied", null, false);
			return;
		}
		this.interactingPlayerId = _playerId;
		int num = this.interactionRequestType;
		if (num == 1)
		{
			GUIWindowManager windowManager = LocalPlayerUI.GetUIForPlayer(localPlayerFromID).windowManager;
			((XUiC_DroneWindowGroup)((XUiWindowGroup)windowManager.GetWindow(XUiC_DroneWindowGroup.ID)).Controller).CurrentVehicleEntity = this;
			windowManager.Open(XUiC_DroneWindowGroup.ID, true, false, true);
			Manager.BroadcastPlayByLocalPlayer(this.position, "UseActions/service_vehicle");
			return;
		}
		if (num != 10)
		{
			return;
		}
		this.accessInventory(localPlayerFromID);
	}

	public void StopUIInteraction()
	{
		this.StopInteraction(234);
	}

	public void StopUIInsteractionSecurity()
	{
		this.StopInteraction(2);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void StopInteraction(ushort syncFlags = 0)
	{
		this.interactingPlayerId = -1;
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			syncFlags |= 4096;
		}
		if (syncFlags != 0)
		{
			this.SendSyncData(syncFlags);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ValidateInteractingPlayer()
	{
		if (!GameManager.Instance.World.GetEntity(this.interactingPlayerId))
		{
			this.interactingPlayerId = -1;
		}
	}

	public int EntityId
	{
		get
		{
			return this.entityId;
		}
		set
		{
			this.entityId = value;
		}
	}

	public bool IsLocked()
	{
		return this.isLocked;
	}

	public void SetLocked(bool _isLocked)
	{
		this.isLocked = _isLocked;
	}

	public PlatformUserIdentifierAbs GetOwner()
	{
		return this.ownerSteamId;
	}

	public void SetOwner(PlatformUserIdentifierAbs _userIdentifier)
	{
		this.ownerSteamId = _userIdentifier;
	}

	public bool IsUserAllowed(PlatformUserIdentifierAbs _userIdentifier)
	{
		return (_userIdentifier != null && _userIdentifier.Equals(this.ownerSteamId)) || this.allowedUsers.Contains(_userIdentifier);
	}

	public List<PlatformUserIdentifierAbs> GetUsers()
	{
		return new List<PlatformUserIdentifierAbs>();
	}

	public bool LocalPlayerIsOwner()
	{
		return this.IsOwner(PlatformManager.InternalLocalUserIdentifier);
	}

	public bool IsOwner(PlatformUserIdentifierAbs _userIdentifier)
	{
		if (this.ownerSteamId == null && this.OwnerID != null)
		{
			return this.OwnerID.Equals(_userIdentifier);
		}
		return this.ownerSteamId.Equals(_userIdentifier);
	}

	public bool HasPassword()
	{
		return this.passwordHash != 0;
	}

	public bool CheckPassword(string _password, PlatformUserIdentifierAbs _userIdentifier, out bool changed)
	{
		changed = false;
		bool flag = Utils.HashString(_password) == this.passwordHash.ToString();
		if (this.LocalPlayerIsOwner())
		{
			if (!flag)
			{
				changed = true;
				this.passwordHash = _password.GetHashCode();
				this.allowedUsers.Clear();
				this.isLocked = true;
				if (this.ownerSteamId == null)
				{
					this.SetOwner(_userIdentifier);
				}
				this.SendSyncData(2);
			}
			return true;
		}
		if (flag)
		{
			this.allowedUsers.Add(_userIdentifier);
			this.SendSyncData(2);
			return true;
		}
		return false;
	}

	public string GetPassword()
	{
		return this.passwordHash.ToString();
	}

	public const string ClassName = "entityJunkDrone";

	public const string ItemName = "gunBotT3JunkDrone";

	public const int SaveVersion = 1;

	public const string cSupportModBuff = "buffJunkDroneSupportEffect";

	public static FastTags<TagGroup.Global> cStorageModifierTags = FastTags<TagGroup.Global>.Parse("droneStorage");

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const string cIdleAnimName = "Base Layer.Idle";

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const string cSpawnAnimName = "Base Layer.SpawnIn";

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static FastTags<TagGroup.Global> repairKitTags = FastTags<TagGroup.Global>.Parse("junk");

	public static bool DebugModeEnabled;

	public ItemValue OriginalItemValue;

	public PlatformUserIdentifierAbs OwnerID;

	public float FollowDistance = 3f;

	public float MaxDistFromOwner = 32f;

	public float IdleHoverHeight = 2f;

	public float FollowHoverHeight = 1.5f;

	public float StayHoverHeight = 2f;

	public float SpeedPathing = 2f;

	public float SpeedFlying = 3f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float cMaxSpeedFlying = 15f;

	public float RotationSpeed = 12f;

	public float AttackActionTime = 3f;

	public float HealActionTime = 7f;

	public EntityAlive Owner;

	public DroneWeapons.HealBeamWeapon healWeapon;

	public DroneWeapons.StunBeamWeapon stunWeapon;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float accelerationTime;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float decelerationTime;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float armorDamageReduction = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float currentSpeedFlying;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool isStunModAttached;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool isHealModAttached;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool isGunModAttached;

	[SerializeField]
	[PublicizedFrom(EAccessModifier.Private)]
	public EntityDrone.State state;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public EntityDrone.State lastState;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public EntityDrone.State transitionState;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float stateTime;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float stateMaxTime;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 lastPosition;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float timeSpentAtLocation;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool isVisible = true;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public EntityAlive currentTarget;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public List<Vector3> currentPath = new List<Vector3>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public EntityDrone.EntitySteering steering;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public EntityDrone.DroneSensors sensors;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public DroneWeapons.MachineGunWeapon machineGunWeapon;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public DroneWeapons.Weapon activeWeapon;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public FloodFillEntityPathGenerator pathMan;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 originalGFXOffset = Vector3.zero;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Transform head;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float wakeupAnimTime;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Color prefabColor;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public DroneLightManager _lm;

	[SerializeField]
	[PublicizedFrom(EAccessModifier.Private)]
	public EntityDrone.Orders orderState;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool hasNavObjectsEnabled;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool isOwnerSyncPending;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool isAnimationStateSet;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public ItemValue itemvalueToLoad;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool isBeingPickedUp;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public BoxCollider interactionCollider;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public string[] paintableParts = new string[]
	{
		"BaseMesh",
		"junkDroneArmRight",
		"armor"
	};

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool debugFriendlyFire;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool debugShowCamera;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public GameObject debugCamera;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Camera reconCam;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool isQuietMode;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool isFlashlightAttached;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool isFlashlightOn;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool isSupportModAttached;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Handle voHandle;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Handle idleLoop;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool partyEventsSet;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int[] knownPartyMembers;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float areaScanTime = 0.5f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float areaScanTimer = 0.5f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool isInConfinedSpace;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float debugInputRotX;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float debugInputRotY;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 debugInputFwd;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 debugInputRgt;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 debugInputUp;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float debugInputSpeed = 3f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3i debugOwnerPos;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float overItemLimitCooldown;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool isTeleporting;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float retryPathTime = 0.5f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool isTryingToFindPath;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public BlockValue currentBlock;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3i currentBlockPosition;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3i lastBlockPosition;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float timeInBlock;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float physColHeight = 0.6f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const int cTalkCommand = 0;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const int cServiceCommand = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const int cRepairCommand = 2;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const int cLockCommand = 3;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const int cUnlockCommand = 4;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const int cKeypadCommand = 5;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const int cTakeCommand = 6;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const int cStayCommand = 7;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const int cFollowCommand = 8;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const int cHealCommand = 9;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const int cStorageCommand = 10;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const int cQuiteCommand = 11;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const int cToggleLightCommand = 12;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const int cCommandCount = 12;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const int cDebugPickup = 13;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const int cDebugFriendlyFire = 13;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const int cDebugDroneCamera = 14;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public RaycastNode focusBoxNode;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public EntityDrone.AllyHealMode allyHealMode;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float cNotifyNeedsHealItemCooldown = 30f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const int cNotifyNeedsHealMaxNotifyCount = 2;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float needsHealItemTimer;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int needsHealNotifyCount;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 sentryPos;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 followTargetPos;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool hasOpenGroupPos;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public List<RaycastNode> nodePath = new List<RaycastNode>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool transitionToIdle;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float transitionToIdleTime = 0.5f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const int cPathLayer = 1073807360;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 currentPathTarget;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool IsTargetPlayer;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float cDebugExtraPathTime = 2f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float debugPathDelay = 2f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool debugPathTiming;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool triedFollowTeleport;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public List<Vector3> projectedPath = new List<Vector3>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool isShutdown;

	[SerializeField]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool isGrounded;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 fallPoint;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool hasFallPoint;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3i fallBlockPos;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 blockHeightOffset = new Vector3(0f, 0.5f, 0f);

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float timeSpentToNextTarget;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 targetDestination;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3[] groupPositions = new Vector3[3];

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3[] fallbackGroupPos = new Vector3[3];

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const ushort cSyncReplicate = 2;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const byte cSyncVersion = 0;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const ushort cSyncOwnerKey = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const ushort cSyncInteractAndSecurity = 2;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const ushort cSyncAction = 4;

	public const ushort cSyncStorage = 8;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const ushort cSyncInteractRequest = 4096;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const ushort cSyncOrderState = 16384;

	public const ushort cSyncState = 32768;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const byte cSyncInteractAndSecurityFInteracting = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const byte cSyncInteractAndSecurityFLocked = 2;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const ushort cSyncRepairAction = 16;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const ushort cSyncQuiteMode = 32;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const ushort cSyncLightMod = 64;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const ushort cSyncService = 128;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool isShutdownPending;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool isLocked;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int passwordHash;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public List<PlatformUserIdentifierAbs> allowedUsers = new List<PlatformUserIdentifierAbs>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool isInteractionLocked;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int interactingPlayerId = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int interactionRequestType;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public PlatformUserIdentifierAbs ownerSteamId;

	public class SoundKeys
	{
		public const string cIdleHover = "drone_idle_hover";

		public const string cFly = "drone_fly";

		public const string cStorageOpen = "vehicle_storage_open";

		public const string cStorageClose = "vehicle_storage_close";

		public const string cHealEffect = "drone_healeffect";

		public const string cAttackEffect = "drone_attackeffect";

		public const string cCommand = "drone_command";

		public const string cEmpty = "drone_empty";

		public const string cEnemySense = "drone_enemy_sense";

		public const string cEnemyEngauge = "drone_enemy_engauge";

		public const string cDroneHeal = "drone_heal";

		public const string cDroneOther = "drone_other";

		public const string cShutDown = "drone_shutdown";

		public const string cTake = "drone_take";

		public const string cTakeFail = "drone_takefail";

		public const string cWakeUp = "drone_wakeup";

		public const string cGreeting = "drone_greeting";
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public class ModKeys
	{
		public const string cStorageMod = "modRoboticDroneCargoMod";

		public const string cArmorMod = "modRoboticDroneArmorPlatingMod";

		public const string cHealMod = "modRoboticDroneMedicMod";

		public const string cStunMod = "modRoboticDroneStunWeaponMod";

		public const string cGunMod = "modRoboticDroneWeaponMod";

		public const string cMoraleMod = "modRoboticDroneMoraleBoosterMod";

		public const string cHeadlampMod = "modRoboticDroneHeadlampMod";

		public const string cHeadlampLightName = "junkDroneLamp";
	}

	public enum State
	{
		Idle,
		Sentry,
		Follow,
		Heal,
		Attack,
		Shutdown,
		NoClip,
		Teleport,
		None
	}

	public enum Orders
	{
		Follow,
		Stay
	}

	public enum Stance
	{
		Defensive,
		Passive,
		Aggressive
	}

	public enum AllyHealMode
	{
		DoNotHeal,
		HealAllies
	}

	[Preserve]
	public class NetPackageDroneDataSync : NetPackage
	{
		public EntityDrone.NetPackageDroneDataSync Setup(EntityDrone _ev, int _senderId, ushort _syncFlags)
		{
			this.senderId = _senderId;
			this.vehicleId = _ev.entityId;
			this.syncFlags = _syncFlags;
			using (PooledBinaryWriter pooledBinaryWriter = MemoryPools.poolBinaryWriter.AllocSync(false))
			{
				pooledBinaryWriter.SetBaseStream(this.entityData);
				_ev.WriteSyncData(pooledBinaryWriter, _syncFlags);
			}
			return this;
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public ~NetPackageDroneDataSync()
		{
			MemoryPools.poolMemoryStream.FreeSync(this.entityData);
		}

		public override void read(PooledBinaryReader _br)
		{
			this.senderId = _br.ReadInt32();
			this.vehicleId = _br.ReadInt32();
			this.syncFlags = _br.ReadUInt16();
			int length = (int)_br.ReadUInt16();
			StreamUtils.StreamCopy(_br.BaseStream, this.entityData, length, null, true);
		}

		public override void write(PooledBinaryWriter _bw)
		{
			base.write(_bw);
			_bw.Write(this.senderId);
			_bw.Write(this.vehicleId);
			_bw.Write(this.syncFlags);
			_bw.Write((ushort)this.entityData.Length);
			this.entityData.WriteTo(_bw.BaseStream);
		}

		public override void ProcessPackage(World _world, GameManager _callbacks)
		{
			if (_world == null)
			{
				return;
			}
			EntityDrone entityDrone = GameManager.Instance.World.GetEntity(this.vehicleId) as EntityDrone;
			if (entityDrone == null)
			{
				return;
			}
			if (this.entityData.Length > 0L)
			{
				PooledExpandableMemoryStream obj = this.entityData;
				lock (obj)
				{
					this.entityData.Position = 0L;
					try
					{
						using (PooledBinaryReader pooledBinaryReader = MemoryPools.poolBinaryReader.AllocSync(false))
						{
							pooledBinaryReader.SetBaseStream(this.entityData);
							entityDrone.ReadSyncData(pooledBinaryReader, this.syncFlags, this.senderId);
						}
					}
					catch (Exception e)
					{
						Log.Exception(e);
						string str = "Error syncing data for entity ";
						EntityDrone entityDrone2 = entityDrone;
						Log.Error(str + ((entityDrone2 != null) ? entityDrone2.ToString() : null) + "; Sender id = " + this.senderId.ToString());
					}
				}
			}
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				ushort syncFlagsReplicated = entityDrone.GetSyncFlagsReplicated(this.syncFlags);
				if (syncFlagsReplicated != 0)
				{
					SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<EntityDrone.NetPackageDroneDataSync>().Setup(entityDrone, this.senderId, syncFlagsReplicated), false, -1, this.senderId, -1, null, 192);
				}
			}
		}

		public override int GetLength()
		{
			return (int)(12L + this.entityData.Length);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public int senderId;

		[PublicizedFrom(EAccessModifier.Private)]
		public int vehicleId;

		[PublicizedFrom(EAccessModifier.Private)]
		public ushort syncFlags;

		[PublicizedFrom(EAccessModifier.Private)]
		public PooledExpandableMemoryStream entityData = MemoryPools.poolMemoryStream.AllocSync(true);
	}

	public class DroneInventory : Inventory
	{
		public DroneInventory(IGameManager _gameManager, EntityAlive _entity) : base(_gameManager, _entity)
		{
			this.SetupSlots();
		}

		public void SetupSlots()
		{
			int num = base.PUBLIC_SLOTS + 1;
			this.slots = new ItemInventoryData[num];
			this.models = new Transform[num];
			this.m_HoldingItemIdx = 0;
			base.Clear();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public class SteeringMan
	{
		public Vector3 Seek(Vector3 pos, Vector3 target, float slowingRadius)
		{
			return this.doSeek(pos, target, slowingRadius);
		}

		public Vector3 Seek2D(Vector3 pos, Vector3 target, float slowingRadius)
		{
			return this.doSeek2D(pos, target, slowingRadius);
		}

		public Vector3 Flee(Vector3 pos, Vector3 target, float avoidRadius)
		{
			return this.doFlee(pos, target, avoidRadius);
		}

		public Vector3 Flee2D(Vector3 pos, Vector3 target, float avoidRadius)
		{
			return this.doFlee2D(pos, target, avoidRadius);
		}

		public Vector3 GetDir(Vector3 from, Vector3 to)
		{
			return this.getDirVector(from, to);
		}

		public Vector3 GetDir2D(Vector3 from, Vector3 to)
		{
			Vector3 pos = new Vector3(from.x, 0f, from.z);
			Vector3 target = new Vector3(to.x, 0f, to.z);
			return this.getDirVector(pos, target);
		}

		public bool IsInRange(Vector3 from, Vector3 to, float dist)
		{
			return this.isInRange(from, to, dist);
		}

		public bool IsInRange2D(Vector3 from, Vector3 to, float dist)
		{
			return this.isInRange2D(from, to, dist);
		}

		public Vector3 GetPointAround(Vector3 lhs, Vector3 rhs, float radius)
		{
			return this.getPointAround(lhs, rhs, radius);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public Vector3 getVec(Vector3 pos, Vector3 target)
		{
			return target - pos;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public Vector3 getDirVector(Vector3 pos, Vector3 target)
		{
			return this.getVec(pos, target).normalized;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public float getDist(Vector3 pos, Vector3 target)
		{
			return this.getVec(pos, target).magnitude;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public bool isInRange(Vector3 from, Vector3 to, float dist)
		{
			return (from - to).sqrMagnitude < dist * dist;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public bool isInRange2D(Vector3 from, Vector3 to, float dist)
		{
			Vector3 from2 = new Vector3(from.x, 0f, from.z);
			Vector3 to2 = new Vector3(to.x, 0f, to.z);
			return this.isInRange(from2, to2, dist);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public Vector3 getPointAround(Vector3 lhs, Vector3 rhs, float radius)
		{
			return Vector3.Cross(lhs, rhs) * radius * 0.5f;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public Vector3 doSeek(Vector3 pos, Vector3 target, float radius)
		{
			float dist = this.getDist(pos, target);
			if (dist < radius)
			{
				return this.getDirVector(pos, target) * (dist / radius);
			}
			return this.getDirVector(pos, target);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public Vector3 doSeek2D(Vector3 pos, Vector3 target, float radius)
		{
			Vector3 result = this.doSeek(pos, target, radius);
			result.y = 0f;
			return result;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public Vector3 doFlee(Vector3 pos, Vector3 target, float radius)
		{
			return -this.doSeek(pos, target, radius);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public Vector3 doFlee2D(Vector3 pos, Vector3 target, float radius)
		{
			Vector3 result = this.doFlee(pos, target, radius);
			result.y = 0f;
			return result;
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public const int kMaxDistance = 1000;
	}

	[Preserve]
	[PublicizedFrom(EAccessModifier.Private)]
	public class EntitySteering : EntityDrone.SteeringMan
	{
		public EntitySteering(EntityAlive _entity)
		{
			this.entity = _entity;
		}

		public Vector3 Hover(float height, float slowingRadius = 1f)
		{
			return this.doHover(this.entity.position, height, slowingRadius);
		}

		public Vector3 FollowPlayer(Vector3 playerPos, Vector3 playerLookDir, float followDist, float degrees = 90f, float maxDist = 15f)
		{
			return this.followTarget(this.entity.position, playerPos, playerLookDir, followDist, degrees, maxDist);
		}

		public Vector3 AvoidArc(Vector3 fromPos, Vector3 toPos, Vector3 dir, Vector3 up, bool subtract, float degrees, float maxDist = 1000f)
		{
			return this.doAvoidArc(fromPos, toPos, dir, up, subtract, degrees, maxDist);
		}

		public Vector3 AvoidArc2D(Vector3 fromPos, Vector3 toPos, Vector3 dir, bool subtract, float degrees, float maxDist = 1000f)
		{
			return this.doAvoidArc2D(fromPos, toPos, dir, subtract, degrees, maxDist);
		}

		public Vector3 AvoidTargetView(EntityAlive target, float followDist, bool subtract, float degrees = 90f, float maxDist = 15f)
		{
			return this.avoidTargetView(this.entity.position, target.getHeadPosition(), target.GetLookVector(), followDist, subtract, degrees, maxDist);
		}

		public Vector3 FollowTarget(EntityAlive target, Vector3 viewDir, float followDist, bool subtract, float degrees = 90f, float maxDist = 15f)
		{
			return this.pursueAvoidOwnerView(this.entity.position, target.getHeadPosition(), viewDir, Vector3.zero, followDist, subtract, degrees, maxDist);
		}

		public bool IsInRange(Vector3 target, float dist)
		{
			return base.IsInRange(this.entity.position, target, dist);
		}

		public bool IsInRange2D(Vector3 target, float dist)
		{
			return base.IsInRange2D(this.entity.position, target, dist);
		}

		public bool IsInRange2D(EntityAlive target, float dist)
		{
			return base.IsInRange2D(this.entity.position, target.position, dist);
		}

		public float GetYPos(float height)
		{
			return this.getYPos(this.entity.position, height);
		}

		public float GetAltitude(Vector3 pos)
		{
			return this.getAltitude(pos);
		}

		public bool IsAboveGround(Vector3 pos)
		{
			return this.getAltitude(pos) > -1f;
		}

		public float GetCeiling(Vector3 pos)
		{
			return this.getCeiling(pos);
		}

		public bool IsBelowCeiling(Vector3 pos)
		{
			return this.getCeiling(pos) > -1f;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public float getAltitude(Vector3 pos)
		{
			RaycastHit raycastHit;
			if (Physics.Raycast(pos - Origin.position, Vector3.down, out raycastHit, 1000f, 65536))
			{
				return raycastHit.distance;
			}
			return -1f;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public float getCeiling(Vector3 pos)
		{
			RaycastHit raycastHit;
			if (Physics.Raycast(pos - Origin.position, Vector3.up, out raycastHit, 1000f, 65536))
			{
				return raycastHit.distance;
			}
			return -1f;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public float getYPos(Vector3 pos, float height)
		{
			float altitude = this.getAltitude(pos);
			if (altitude >= 0f)
			{
				return pos.y - altitude + height;
			}
			return -1f;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public Vector3 doHover(Vector3 pos, float height, float radius)
		{
			float altitude = this.getAltitude(pos);
			if (altitude <= 0f)
			{
				return Vector3.zero;
			}
			Vector3 vector = (altitude < height) ? Vector3.up : Vector3.down;
			float num = Mathf.Abs(height - altitude);
			if (num < radius)
			{
				return vector * (num / radius);
			}
			return vector;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public Vector3 followTarget(Vector3 pos, Vector3 target, Vector3 lookDir, float followDist, float degrees, float maxDist)
		{
			return base.Seek(pos, target, followDist).normalized;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public Vector3 avoidTargetView(Vector3 pos, Vector3 target, Vector3 lookDir, float followDist, bool subtract, float degrees, float maxDist)
		{
			return this.AvoidArc2D(pos, target, lookDir, subtract, degrees, maxDist).normalized;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public Vector3 pursueAvoidOwnerView(Vector3 pos, Vector3 target, Vector3 lookDir, Vector3 offSet, float followDist, bool subtract, float degrees, float maxDist)
		{
			Vector3 a = base.Seek(pos, target, followDist);
			Vector3 b = this.AvoidArc2D(pos, target, lookDir, subtract, degrees, maxDist);
			return (a + b).normalized;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public Vector3 doAvoidArc(Vector3 from, Vector3 to, Vector3 dir, Vector3 up, bool subtract, float degrees, float maxDist)
		{
			Vector3 to2 = from - to;
			if (Vector3.Angle(dir, to2) < degrees * 0.5f)
			{
				Vector3 vector = base.GetPointAround((to - from).normalized, up, maxDist);
				vector = (subtract ? (to - vector) : (to + vector));
				if (base.IsInRange(from, vector, maxDist))
				{
					return base.Flee(from, vector + dir * to2.magnitude, 0f);
				}
			}
			return Vector3.zero;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public Vector3 doAvoidArc2D(Vector3 from, Vector3 to, Vector3 dir, bool subtract, float degrees, float maxDist)
		{
			Vector3 result = this.doAvoidArc(from, to, dir, Vector3.up, subtract, degrees, maxDist);
			result.y = 0f;
			return result;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public EntityAlive entity;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public class DroneSensors
	{
		public DroneSensors(EntityAlive _entity)
		{
			this.entity = _entity;
		}

		public void Init()
		{
			this.canBarkEnemyDetected = true;
		}

		public void Update()
		{
			if (this.enemyDetectedBarkTimer > 0f)
			{
				this.enemyDetectedBarkTimer -= 0.05f;
				if (this.enemyDetectedBarkTimer <= 0f)
				{
					this.canBarkEnemyDetected = true;
				}
			}
		}

		public EntityAlive TargetInRange()
		{
			EntityAlive entityAlive = this.targetCheck();
			if (entityAlive && this.canBarkEnemyDetected)
			{
				this.barkEnemyDetected();
			}
			return entityAlive;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public EntityAlive targetCheck()
		{
			if (this.entity.GetRevengeTarget() && !this.entity.GetRevengeTarget().Buffs.HasBuff("buffShocked"))
			{
				return this.entity.GetRevengeTarget();
			}
			List<Entity> entitiesInBounds = GameManager.Instance.World.GetEntitiesInBounds(this.entity, new Bounds(this.entity.position, Vector3.one * this.EnemyDetectionRadius));
			for (int i = 0; i < entitiesInBounds.Count; i++)
			{
				EntityAlive entityAlive = entitiesInBounds[i] as EntityAlive;
				if (entityAlive != null && entitiesInBounds[i].EntityClass != null && entitiesInBounds[i].EntityClass.bIsEnemyEntity && !(entityAlive is EntityNPC) && (!entityAlive.IsSleeper || !entityAlive.IsSleeping) && !(entitiesInBounds[i] as EntityAlive).Buffs.HasBuff("buffShocked"))
				{
					return entitiesInBounds[i] as EntityAlive;
				}
			}
			return null;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void barkEnemyDetected()
		{
			EntityDrone entityDrone = this.entity as EntityDrone;
			if (entityDrone)
			{
				if (entityDrone.Owner)
				{
					Manager.Stop(entityDrone.Owner.entityId, "drone_take");
				}
				if (entityDrone.state == EntityDrone.State.Shutdown)
				{
					return;
				}
				entityDrone.PlayVO("drone_enemy_sense", true, 1f);
				this.enemyDetectedBarkTimer = this.EnemyDetectedBarkCooldown;
				this.canBarkEnemyDetected = false;
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public EntityAlive entity;

		public float EnemyDetectionRadius = 20f;

		public float EnemyDetectedBarkCooldown = 90f;

		[PublicizedFrom(EAccessModifier.Private)]
		public float enemyDetectedBarkTimer = 10f;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool canBarkEnemyDetected;
	}
}
