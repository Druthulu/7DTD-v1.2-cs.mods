using System;
using System.Collections.Generic;
using System.Globalization;
using Audio;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class DroneWeapons
{
	public const string cSHOCK_BUFF_NAME = "buffShocked";

	public const string cBuffHealCooldown = "buffJunkDroneHealCooldownEffect";

	[PublicizedFrom(EAccessModifier.Private)]
	public static FastTags<TagGroup.Global> healingItemTags = FastTags<TagGroup.Global>.Parse("medical");

	public const string cHealWeaponJoint = "WristLeft";

	[Preserve]
	public class Weapon
	{
		public Weapon(EntityAlive _entity)
		{
			this.entity = _entity;
			this.properties = _entity.EntityClass.Properties;
			this.belongsPlayerId = this.entity.belongsPlayerId;
		}

		public virtual void Init()
		{
		}

		public virtual void Update()
		{
			if (this.cooldownTimer > 0f)
			{
				this.cooldownTimer -= 0.05f;
				if (this.cooldownTimer <= 0f || (this.target && this.target.IsDead()))
				{
					this.InvokeFireComplete();
				}
			}
		}

		public float TimeRemaning
		{
			get
			{
				return this.cooldownTimer;
			}
		}

		public float TimeLength
		{
			get
			{
				return this.actionTime + this.cooldown;
			}
		}

		public float Range
		{
			get
			{
				return this.range;
			}
		}

		public virtual bool canFire()
		{
			return this.cooldownTimer <= 0f;
		}

		public virtual void Fire(EntityAlive _target)
		{
			this.target = _target;
			this.cooldownTimer = this.actionTime + this.cooldown;
		}

		public virtual bool hasActionCompleted()
		{
			return this.cooldownTimer < this.cooldown;
		}

		public void RegisterOnFireComplete(Action _onFireComplete)
		{
			this.onFireComplete = _onFireComplete;
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public virtual void OnFireComplete()
		{
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public void InvokeFireComplete()
		{
			this.OnFireComplete();
			Action action = this.onFireComplete;
			if (action != null)
			{
				action();
			}
			this.onFireComplete = null;
			this.target = null;
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public void TargetApplyBuff(string _buff)
		{
			this.target.Buffs.AddBuff(_buff, (this.belongsPlayerId != -1) ? this.belongsPlayerId : this.entity.entityId, true, false, -1f);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public void SpawnParticleEffect(ParticleEffect _pe, int _entityId)
		{
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				if (!GameManager.IsDedicatedServer)
				{
					GameManager.Instance.SpawnParticleEffectClient(_pe, _entityId, false, false);
				}
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageParticleEffect>().Setup(_pe, _entityId, false, false), false, -1, _entityId, -1, null, 192);
				return;
			}
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageParticleEffect>().Setup(_pe, _entityId, false, false), false);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public Transform SpawnDroneParticleEffect(ParticleEffect _pe, int _entityId, DroneWeapons.NetPackageDroneParticleEffect.cActionType actionType)
		{
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<DroneWeapons.NetPackageDroneParticleEffect>().Setup(_pe, _entityId, actionType), false, -1, -1, -1, null, 192);
				if (!GameManager.IsDedicatedServer)
				{
					return GameManager.Instance.SpawnParticleEffectClientForceCreation(_pe, _entityId, false);
				}
			}
			else
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<DroneWeapons.NetPackageDroneParticleEffect>().Setup(_pe, _entityId, actionType), false);
			}
			return null;
		}

		public Transform WeaponJoint;

		[PublicizedFrom(EAccessModifier.Protected)]
		public EntityAlive entity;

		[PublicizedFrom(EAccessModifier.Protected)]
		public DynamicProperties properties;

		[PublicizedFrom(EAccessModifier.Protected)]
		public int belongsPlayerId;

		[PublicizedFrom(EAccessModifier.Protected)]
		public float actionTime;

		[PublicizedFrom(EAccessModifier.Protected)]
		public float cooldown = 1f;

		[PublicizedFrom(EAccessModifier.Protected)]
		public float range = 10f;

		[PublicizedFrom(EAccessModifier.Protected)]
		public EntityAlive target;

		[PublicizedFrom(EAccessModifier.Private)]
		public Action onFireComplete;

		[PublicizedFrom(EAccessModifier.Private)]
		public float cooldownTimer;
	}

	[Preserve]
	public class HealBeamWeapon : DroneWeapons.Weapon
	{
		public HealBeamWeapon(EntityAlive _entity) : base(_entity)
		{
		}

		public override void Init()
		{
			this.WeaponJoint = this.entity.transform.FindInChilds("WristLeft", false);
			if (this.properties.Values.ContainsKey("HealCooldown"))
			{
				float.TryParse(this.properties.Values["HealCooldown"], out this.cooldown);
			}
			if (this.properties.Values.ContainsKey("HealActionTime"))
			{
				float.TryParse(this.properties.Values["HealActionTime"], out this.actionTime);
			}
			if (this.properties.Values.ContainsKey("HealDamageThreshold"))
			{
				float.TryParse(this.properties.Values["HealDamageThreshold"], out this.HealDamageThreshold);
			}
		}

		public override void Fire(EntityAlive _target)
		{
			base.Fire(_target);
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				ItemStack healingItemStack = this.getHealingItemStack();
				if (healingItemStack == null)
				{
					base.InvokeFireComplete();
					return;
				}
				EntityDrone entityDrone = this.entity as EntityDrone;
				entityDrone.SendSyncData(8);
				this.entity.inventory.SetItem(0, healingItemStack);
				this.entity.inventory.SetHoldingItemIdx(0);
				ItemAction itemAction = this.entity.inventory.holdingItem.Actions[1];
				ItemActionData actionData = this.entity.inventory.holdingItemData.actionData[1];
				if (itemAction != null)
				{
					itemAction.ExecuteAction(actionData, true);
				}
				EntityAlive owner = entityDrone.Owner;
				if (owner)
				{
					owner.Buffs.AddBuff("buffJunkDroneHealCooldownEffect", -1, true, false, -1f);
				}
				ParticleEffect pe = new ParticleEffect("drone_heal_beam", Vector3.zero, Quaternion.LookRotation(_target.getHeadPosition() - this.entity.position), 1f, Color.clear, null, this.entity.transform);
				Transform transform = base.SpawnDroneParticleEffect(pe, this.entity.entityId, DroneWeapons.NetPackageDroneParticleEffect.cActionType.Heal);
				if (transform && !GameManager.IsDedicatedServer)
				{
					transform.GetComponent<DroneBeamParticle>().SetDisplayTime(this.actionTime);
				}
				ParticleEffect pe2 = new ParticleEffect("drone_heal_player", Vector3.zero, Quaternion.identity, 1f, Color.clear, null, _target.transform);
				if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
				{
					SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageParticleEffect>().Setup(pe2, _target.entityId, false, false), false, -1, -1, -1, null, 192);
					if (!GameManager.IsDedicatedServer)
					{
						GameManager.Instance.SpawnParticleEffectClient(pe2, _target.entityId, false, false);
					}
				}
			}
		}

		public override bool canFire()
		{
			return base.canFire() && this.hasHealingItem();
		}

		public bool hasHealingItem()
		{
			return this._hasItem(this._healTypeToString(DroneWeapons.HealBeamWeapon.HealItemType.Bandage)) || this._hasItem(this._healTypeToString(DroneWeapons.HealBeamWeapon.HealItemType.FirstAidKit));
		}

		public bool targetCanBeHealed(EntityAlive _target)
		{
			return _target.IsAlive() && !_target.Buffs.HasBuff("buffHealHealth") && _target.Health < _target.GetMaxHealth();
		}

		public bool targetNeedsHealing(EntityAlive _target)
		{
			float num = (float)_target.GetMaxHealth();
			float modifiedMax = _target.Stats.Health.ModifiedMax;
			return this.targetCanBeHealed(_target) && ((num == modifiedMax && (float)_target.Health < num - this.HealDamageThreshold) || (float)_target.Health < modifiedMax * 0.67f);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public string _healTypeToString(DroneWeapons.HealBeamWeapon.HealItemType healType)
		{
			return this._supportedHealingItems[(int)healType];
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public ItemStack getHealingItemStack()
		{
			DroneWeapons.HealBeamWeapon.HealItemType healType = DroneWeapons.HealBeamWeapon.HealItemType.Bandage;
			int num = this._hasItem(this._healTypeToString(DroneWeapons.HealBeamWeapon.HealItemType.Bandage)) ? 1 : 0;
			bool flag = this._hasItem(this._healTypeToString(DroneWeapons.HealBeamWeapon.HealItemType.FirstAidKit));
			if (num == 0 && flag)
			{
				healType = DroneWeapons.HealBeamWeapon.HealItemType.FirstAidKit;
			}
			ItemStack[] array = this.entity.bag.GetSlots();
			if (this.entity.lootContainer != null)
			{
				array = this.entity.lootContainer.GetItems();
			}
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i] != null && array[i].itemValue != null && array[i].itemValue.ItemClass != null && array[i].itemValue.ItemClass.HasAnyTags(DroneWeapons.healingItemTags) && array[i].count > 0 && this._isItem(array[i].itemValue, this._healTypeToString(healType)))
				{
					ItemValue itemValue = array[i].itemValue.Clone();
					array[i].count--;
					if (array[i].count == 0)
					{
						array[i] = ItemStack.Empty.Clone();
					}
					this.entity.bag.SetSlots(array);
					this.entity.bag.OnUpdate();
					this.entity.lootContainer.UpdateSlot(i, array[i]);
					return new ItemStack(itemValue, 1);
				}
			}
			return null;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public bool _hasItem(string itemGroupOrName)
		{
			ItemStack[] array = this.entity.bag.GetSlots();
			if (this.entity.lootContainer != null)
			{
				array = this.entity.lootContainer.GetItems();
			}
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i] != null && array[i].itemValue != null && array[i].itemValue.ItemClass != null && array[i].itemValue.ItemClass.HasAnyTags(DroneWeapons.healingItemTags) && array[i].itemValue.ItemClass.Name.ContainsCaseInsensitive(itemGroupOrName))
				{
					return true;
				}
			}
			return false;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public bool _isItem(ItemValue iv, string itemGroupOrName)
		{
			return iv.ItemClass.Name.ContainsCaseInsensitive(itemGroupOrName);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public const string cHealBeam = "drone_heal_beam";

		[PublicizedFrom(EAccessModifier.Private)]
		public const string cHealPlayer = "drone_heal_player";

		[PublicizedFrom(EAccessModifier.Private)]
		public const int cIdxHealing = 0;

		public float HealDamageThreshold = 35f;

		[PublicizedFrom(EAccessModifier.Private)]
		public const string cAbrasionInjury = "buffInjuryAbrasion";

		[PublicizedFrom(EAccessModifier.Private)]
		public const string cHealingBuff = "buffHealHealth";

		[PublicizedFrom(EAccessModifier.Private)]
		public const float cModifiedHealthCutoff = 0.67f;

		[PublicizedFrom(EAccessModifier.Private)]
		public string[] _supportedHealingItems = new string[]
		{
			"bandage",
			"medicalFirstAidKit"
		};

		[PublicizedFrom(EAccessModifier.Private)]
		public enum HealItemType
		{
			Bandage,
			FirstAidKit
		}
	}

	[Preserve]
	public class StunBeamWeapon : DroneWeapons.Weapon
	{
		public StunBeamWeapon(EntityAlive _entity) : base(_entity)
		{
		}

		public override void Init()
		{
			this.WeaponJoint = this.entity.transform.FindInChilds("WristRight", false);
			if (this.properties.Values.ContainsKey("StunCooldown"))
			{
				float.TryParse(this.properties.Values["StunCooldown"], out this.cooldown);
			}
			if (this.properties.Values.ContainsKey("StunActionTime"))
			{
				float.TryParse(this.properties.Values["StunActionTime"], out this.actionTime);
			}
		}

		public override void Fire(EntityAlive _target)
		{
			base.Fire(_target);
			base.TargetApplyBuff("buffShocked");
			Manager.Play(this.entity, "drone_attackeffect", 1f, false);
			ParticleEffect pe = new ParticleEffect("nozzleflashuzi", this.WeaponJoint.position + Origin.position, Quaternion.Euler(0f, 180f, 0f), 1f, Color.white, "Electricity/Turret/turret_fire", this.WeaponJoint);
			base.SpawnParticleEffect(pe, -1);
			float lightValue = GameManager.Instance.World.GetLightBrightness(World.worldToBlockPos(this.entity.position)) / 2f;
			ParticleEffect pe2 = new ParticleEffect("nozzlesmokeuzi", this.WeaponJoint.position + Origin.position, lightValue, new Color(1f, 1f, 1f, 0.3f), null, this.WeaponJoint, false);
			base.SpawnParticleEffect(pe2, -1);
		}
	}

	[Preserve]
	public class MachineGunWeapon : DroneWeapons.Weapon
	{
		public MachineGunWeapon(EntityAlive _entity) : base(_entity)
		{
		}

		public override void Init()
		{
			this.damageMultiplier = new DamageMultiplier(this.properties, null);
			this.WeaponJoint = this.entity.transform.FindInChilds("WristRight", false);
			if (this.properties.Values.ContainsKey("MaxDistance"))
			{
				this.range = StringParsers.ParseFloat(this.properties.Values["MaxDistance"], 0, -1, NumberStyles.Any);
			}
			this.spreadHorizontal = new Vector2(-1f, 1f);
			this.spreadVertical = new Vector2(-1f, 1f);
			if (this.properties.Values.ContainsKey("RaySpread"))
			{
				float num = StringParsers.ParseFloat(this.properties.Values["RaySpread"], 0, -1, NumberStyles.Any);
				num *= 0.5f;
				this.spreadHorizontal = new Vector2(-num, num);
				this.spreadVertical = new Vector2(-num, num);
			}
			if (this.properties.Values.ContainsKey("RayCount"))
			{
				this.RayCount = (float)int.Parse(this.properties.Values["RayCount"]);
			}
			if (this.properties.Values.ContainsKey("BurstRoundCount"))
			{
				this.burstRoundCountMax = int.Parse(this.properties.Values["BurstRoundCount"]);
			}
			if (this.properties.Values.ContainsKey("BurstFireRate"))
			{
				this.burstFireRate = Mathf.Max(StringParsers.ParseFloat(this.properties.Values["BurstFireRate"], 0, -1, NumberStyles.Any), 0.1f);
			}
			this.actionTime = this.burstFireRate * (float)this.burstRoundCountMax;
			if (this.properties.Values.ContainsKey("CooldownTime"))
			{
				this.cooldown = StringParsers.ParseFloat(this.properties.Values["CooldownTime"], 0, -1, NumberStyles.Any);
			}
			if (this.properties.Values.ContainsKey("EntityDamage"))
			{
				this.entityDamage = int.Parse(this.properties.Values["EntityDamage"]);
			}
			this.buffActions = new List<string>();
			if (this.properties.Values.ContainsKey("Buff"))
			{
				string[] collection = this.properties.Values["Buff"].Replace(" ", "").Split(',', StringSplitOptions.None);
				this.buffActions.AddRange(collection);
			}
		}

		public override void Update()
		{
			base.Update();
			if (this.target != null && !this.target.IsDead() && this.burstRoundCount < this.burstRoundCountMax && base.TimeRemaning > 0f && base.TimeRemaning < base.TimeLength - this.burstFireRate * (float)this.burstRoundCount)
			{
				this._fireWeapon();
			}
		}

		public override void Fire(EntityAlive _target)
		{
			if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				return;
			}
			base.Fire(_target);
			this._fireWeapon();
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void OnFireComplete()
		{
			base.OnFireComplete();
			this.burstRoundCount = 0;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void _fireWeapon()
		{
			EntityDrone entityDrone = this.entity as EntityDrone;
			Vector3 position = this.WeaponJoint.transform.position;
			Vector3 a = this.target.getChestPosition() - Origin.position;
			EntityAlive entity = GameManager.Instance.World.GetEntity(entityDrone.belongsPlayerId) as EntityAlive;
			GameRandom gameRandom = GameManager.Instance.World.GetGameRandom();
			FastTags<TagGroup.Global> itemTags = entityDrone.OriginalItemValue.ItemClass.ItemTags;
			int num = (int)EffectManager.GetValue(PassiveEffects.RoundRayCount, entityDrone.OriginalItemValue, this.RayCount, entity, null, entityDrone.OriginalItemValue.ItemClass.ItemTags, true, false, true, true, true, 1, true, false);
			float value = EffectManager.GetValue(PassiveEffects.MaxRange, entityDrone.OriginalItemValue, this.range, entity, null, entityDrone.OriginalItemValue.ItemClass.ItemTags, true, false, true, true, true, 1, true, false);
			for (int i = 0; i < num; i++)
			{
				Vector3 vector = (a - position).normalized;
				vector = Quaternion.Euler(gameRandom.RandomRange(this.spreadHorizontal.x, this.spreadHorizontal.y), gameRandom.RandomRange(this.spreadVertical.x, this.spreadVertical.y), 0f) * vector;
				Ray ray = new Ray(position + Origin.position, vector);
				int num2 = Mathf.FloorToInt(EffectManager.GetValue(PassiveEffects.EntityPenetrationCount, entityDrone.OriginalItemValue, 0f, entity, null, entityDrone.OriginalItemValue.ItemClass.ItemTags, true, false, true, true, true, 1, true, false));
				num2++;
				int num3 = Mathf.FloorToInt(EffectManager.GetValue(PassiveEffects.BlockPenetrationFactor, entityDrone.OriginalItemValue, 1f, entity, null, entityDrone.OriginalItemValue.ItemClass.ItemTags, true, false, true, true, true, 1, true, false));
				EntityAlive x = null;
				for (int j = 0; j < num2; j++)
				{
					if (Voxel.Raycast(GameManager.Instance.World, ray, value, -538750997, 8, 0f))
					{
						WorldRayHitInfo worldRayHitInfo = Voxel.voxelRayHitInfo.Clone();
						if (worldRayHitInfo.tag.StartsWith("E_"))
						{
							string text;
							EntityAlive entityAlive = ItemActionAttack.FindHitEntityNoTagCheck(worldRayHitInfo, out text) as EntityAlive;
							if (x == entityAlive)
							{
								ray.origin = worldRayHitInfo.hit.pos + ray.direction * 0.1f;
								j--;
								goto IL_334;
							}
							x = entityAlive;
						}
						else
						{
							j += Mathf.FloorToInt((float)ItemActionAttack.GetBlockHit(GameManager.Instance.World, worldRayHitInfo).Block.MaxDamage / (float)num3);
						}
						ItemActionAttack.Hit(worldRayHitInfo, entityDrone.belongsPlayerId, EnumDamageTypes.Piercing, this.GetDamageBlock(entityDrone.OriginalItemValue, BlockValue.Air, GameManager.Instance.World.GetEntity(entityDrone.belongsPlayerId) as EntityAlive, 1), this.GetDamageEntity(entityDrone.OriginalItemValue, GameManager.Instance.World.GetEntity(entityDrone.belongsPlayerId) as EntityAlive, 1), 1f, entityDrone.OriginalItemValue.PercentUsesLeft, 0f, 0f, "bullet", this.damageMultiplier, this.buffActions, new ItemActionAttack.AttackHitInfo(), 1, 0, 0f, null, null, ItemActionAttack.EnumAttackMode.RealNoHarvesting, null, entityDrone.entityId, entityDrone.OriginalItemValue);
					}
					IL_334:;
				}
			}
			ParticleEffect pe = new ParticleEffect("nozzleflashuzi", this.WeaponJoint.position + Origin.position, Quaternion.Euler(0f, 180f, 0f), 1f, Color.white, "Electricity/Turret/turret_fire", this.WeaponJoint);
			base.SpawnParticleEffect(pe, -1);
			float lightValue = GameManager.Instance.World.GetLightBrightness(World.worldToBlockPos(this.entity.position)) / 2f;
			ParticleEffect pe2 = new ParticleEffect("nozzlesmokeuzi", this.WeaponJoint.position + Origin.position, lightValue, new Color(1f, 1f, 1f, 0.3f), null, this.WeaponJoint, false);
			base.SpawnParticleEffect(pe2, -1);
			this.burstRoundCount++;
			if ((int)EffectManager.GetValue(PassiveEffects.MagazineSize, entityDrone.OriginalItemValue, 0f, null, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false) > 0)
			{
				EntityDrone entityDrone2 = entityDrone;
				int ammoCount = entityDrone2.AmmoCount;
				entityDrone2.AmmoCount = ammoCount - 1;
			}
			entityDrone.OriginalItemValue.UseTimes += EffectManager.GetValue(PassiveEffects.DegradationPerUse, entityDrone.OriginalItemValue, 1f, entity, null, entityDrone.OriginalItemValue.ItemClass.ItemTags, true, false, true, true, true, 1, true, false);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public float GetDamageEntity(ItemValue _itemValue, EntityAlive _holdingEntity = null, int actionIndex = 0)
		{
			return EffectManager.GetValue(PassiveEffects.EntityDamage, _itemValue, (float)this.entityDamage, _holdingEntity, null, _itemValue.ItemClass.ItemTags, true, false, true, true, true, 1, true, false);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public float GetDamageBlock(ItemValue _itemValue, BlockValue _blockValue, EntityAlive _holdingEntity = null, int actionIndex = 0)
		{
			this.tmpTag = _itemValue.ItemClass.ItemTags;
			this.tmpTag |= _blockValue.Block.Tags;
			float value = EffectManager.GetValue(PassiveEffects.BlockDamage, _itemValue, (float)this.blockDamage, _holdingEntity, null, this.tmpTag, true, false, true, true, true, 1, true, false);
			return Utils.FastMin((float)_blockValue.Block.blockMaterial.MaxIncomingDamage, value);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public float RayCount = 1f;

		[PublicizedFrom(EAccessModifier.Private)]
		public Vector2 spreadHorizontal;

		[PublicizedFrom(EAccessModifier.Private)]
		public Vector2 spreadVertical;

		[PublicizedFrom(EAccessModifier.Private)]
		public int burstRoundCountMax = 1;

		[PublicizedFrom(EAccessModifier.Private)]
		public int burstRoundCount;

		[PublicizedFrom(EAccessModifier.Private)]
		public const float burstFireRateMax = 0.1f;

		[PublicizedFrom(EAccessModifier.Private)]
		public float burstFireRate = 1f;

		[PublicizedFrom(EAccessModifier.Private)]
		public int entityDamage;

		[PublicizedFrom(EAccessModifier.Private)]
		public int blockDamage;

		[PublicizedFrom(EAccessModifier.Private)]
		public DamageMultiplier damageMultiplier;

		[PublicizedFrom(EAccessModifier.Private)]
		public List<string> buffActions;

		[PublicizedFrom(EAccessModifier.Private)]
		public FastTags<TagGroup.Global> tmpTag;
	}

	[Preserve]
	public class NetPackageDroneParticleEffect : NetPackage
	{
		public DroneWeapons.NetPackageDroneParticleEffect Setup(ParticleEffect _pe, int _entityThatCausedIt, DroneWeapons.NetPackageDroneParticleEffect.cActionType _actionType)
		{
			this.pe = _pe;
			this.entityThatCausedIt = _entityThatCausedIt;
			this.actionType = _actionType;
			return this;
		}

		public override void read(PooledBinaryReader _br)
		{
			this.pe = new ParticleEffect();
			this.pe.Read(_br);
			this.entityThatCausedIt = _br.ReadInt32();
		}

		public override void write(PooledBinaryWriter _bw)
		{
			base.write(_bw);
			this.pe.Write(_bw);
			_bw.Write(this.entityThatCausedIt);
		}

		public override void ProcessPackage(World _world, GameManager _callbacks)
		{
			if (_world == null)
			{
				return;
			}
			if (!_world.IsRemote())
			{
				_world.GetGameManager().SpawnParticleEffectServer(this.pe, this.entityThatCausedIt, false, false);
				return;
			}
			Transform transform = _world.GetGameManager().SpawnParticleEffectClientForceCreation(this.pe, this.entityThatCausedIt, false);
			if (transform != null)
			{
				EntityDrone entityDrone = _world.GetEntity(this.entityThatCausedIt) as EntityDrone;
				if (entityDrone != null)
				{
					DroneBeamParticle component = transform.GetComponent<DroneBeamParticle>();
					if (this.actionType == DroneWeapons.NetPackageDroneParticleEffect.cActionType.Attack)
					{
						transform.parent = entityDrone.stunWeapon.WeaponJoint;
						component.SetDisplayTime(entityDrone.AttackActionTime);
					}
					else if (this.actionType == DroneWeapons.NetPackageDroneParticleEffect.cActionType.Heal)
					{
						transform.parent = entityDrone.healWeapon.WeaponJoint;
						component.SetDisplayTime(entityDrone.HealActionTime);
					}
					transform.localPosition = Vector3.zero;
					transform.localRotation = Quaternion.identity;
				}
			}
		}

		public override int GetLength()
		{
			return 20;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public ParticleEffect pe;

		[PublicizedFrom(EAccessModifier.Private)]
		public int entityThatCausedIt;

		[PublicizedFrom(EAccessModifier.Private)]
		public DroneWeapons.NetPackageDroneParticleEffect.cActionType actionType;

		public enum cActionType
		{
			Attack,
			Heal
		}
	}
}
