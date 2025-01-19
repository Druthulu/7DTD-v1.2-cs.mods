using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class EntityStats
{
	public float AmountEnclosed
	{
		get
		{
			return this.m_amountEnclosed;
		}
		set
		{
			this.m_amountEnclosed = value;
		}
	}

	public float HeightTemperatureOffset
	{
		get
		{
			return this.m_heightTemperatureOffset;
		}
		set
		{
			this.m_heightTemperatureOffset = value;
		}
	}

	public bool Shaded
	{
		get
		{
			return this.m_isInShade;
		}
		set
		{
			this.m_isInShade = value;
		}
	}

	public EntityStats(EntityAlive entity)
	{
		this.m_entity = entity;
		this.m_isEntityPlayer = (entity as EntityPlayer != null || entity as EntityPlayerLocal != null);
		int num = (int)EffectManager.GetValue(PassiveEffects.HealthMax, null, 100f, entity, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
		int num2 = (int)EffectManager.GetValue(PassiveEffects.StaminaMax, null, 100f, entity, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
		int num3 = (int)EffectManager.GetValue(PassiveEffects.FoodMax, null, 100f, entity, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
		int num4 = (int)EffectManager.GetValue(PassiveEffects.WaterMax, null, 100f, entity, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
		this.Health = new Stat(entity, (float)num, (float)num)
		{
			StatType = Stat.StatTypes.Health,
			GainPassive = PassiveEffects.HealthGain,
			ChangeOTPassive = PassiveEffects.HealthChangeOT,
			MaxPassive = PassiveEffects.HealthMax,
			LossPassive = PassiveEffects.HealthLoss
		};
		this.Stamina = new Stat(entity, (float)num2, (float)num2)
		{
			StatType = Stat.StatTypes.Stamina,
			GainPassive = PassiveEffects.StaminaGain,
			ChangeOTPassive = PassiveEffects.StaminaChangeOT,
			MaxPassive = PassiveEffects.StaminaMax,
			LossPassive = PassiveEffects.StaminaLoss
		};
		this.Water = new Stat(entity, (float)num4, (float)num4)
		{
			StatType = Stat.StatTypes.Water,
			GainPassive = PassiveEffects.WaterGain,
			ChangeOTPassive = PassiveEffects.WaterChangeOT,
			MaxPassive = PassiveEffects.WaterMax,
			LossPassive = PassiveEffects.WaterLoss
		};
		this.Food = new Stat(entity, (float)num3, (float)num3)
		{
			StatType = Stat.StatTypes.Food,
			GainPassive = PassiveEffects.FoodGain,
			ChangeOTPassive = PassiveEffects.FoodChangeOT,
			MaxPassive = PassiveEffects.FoodMax,
			LossPassive = PassiveEffects.FoodLoss
		};
		this.CoreTemp = new Stat(entity, -200f, 200f)
		{
			StatType = Stat.StatTypes.CoreTemp
		};
		this.buffChangedDelegates = new List<IEntityBuffsChanged>();
		this.notificationChangedDelegates = new List<IEntityUINotificationChanged>();
		this.m_notifications = new List<EntityUINotification>();
		this.CreatePlayerNotifications();
	}

	public void AddUINotificationChangedDelegate(IEntityUINotificationChanged _uiChangedDelegate)
	{
		if (!this.notificationChangedDelegates.Contains(_uiChangedDelegate))
		{
			this.notificationChangedDelegates.Add(_uiChangedDelegate);
		}
	}

	public void RemoveUINotificationChangedDelegate(IEntityUINotificationChanged _uiChangedDelegate)
	{
		this.notificationChangedDelegates.Remove(_uiChangedDelegate);
	}

	public void CopyBuffChangedDelegates(EntityStats _from)
	{
		if (_from == null)
		{
			return;
		}
		foreach (IEntityBuffsChanged buffChangedDelegate in _from.buffChangedDelegates)
		{
			this.AddBuffChangedDelegate(buffChangedDelegate);
		}
	}

	public void AddBuffChangedDelegate(IEntityBuffsChanged _buffChangedDelegate)
	{
		if (!this.buffChangedDelegates.Contains(_buffChangedDelegate))
		{
			this.buffChangedDelegates.Add(_buffChangedDelegate);
		}
	}

	public void RemoveBuffChangedDelegate(IEntityBuffsChanged _buffChangedDelegate)
	{
		this.buffChangedDelegates.Remove(_buffChangedDelegate);
	}

	public void EntityBuffAdded(BuffValue _buff)
	{
		for (int i = 0; i < this.buffChangedDelegates.Count; i++)
		{
			this.buffChangedDelegates[i].EntityBuffAdded(_buff);
		}
		if (!_buff.BuffClass.Hidden && _buff.BuffClass.Icon != null)
		{
			BuffEntityUINotification buffEntityUINotification = new BuffEntityUINotification();
			buffEntityUINotification.SetBuff(_buff);
			buffEntityUINotification.SetStats(this);
			this.NotificationAdded(buffEntityUINotification);
		}
	}

	public void EntityBuffRemoved(BuffValue _buff)
	{
		for (int i = 0; i < this.buffChangedDelegates.Count; i++)
		{
			this.buffChangedDelegates[i].EntityBuffRemoved(_buff);
		}
		for (int j = 0; j < this.m_notifications.Count; j++)
		{
			if (this.m_notifications[j].Buff == _buff)
			{
				this.m_notifications[j].NotifyBuffRemoved();
				if (this.m_notifications[j].Expired)
				{
					this.NotificationRemoved(this.m_notifications[j]);
					continue;
				}
			}
		}
	}

	public void NotificationAdded(EntityUINotification notification)
	{
		this.m_notifications.Add(notification);
		for (int i = 0; i < this.notificationChangedDelegates.Count; i++)
		{
			this.notificationChangedDelegates[i].EntityUINotificationAdded(notification);
		}
	}

	public void NotificationRemoved(EntityUINotification notification)
	{
		this.m_notifications.Remove(notification);
		for (int i = 0; i < this.notificationChangedDelegates.Count; i++)
		{
			this.notificationChangedDelegates[i].EntityUINotificationRemoved(notification);
		}
	}

	public List<EntityUINotification> Notifications
	{
		get
		{
			return this.m_notifications;
		}
	}

	public void Update(float dt, ulong worldTime)
	{
		if (this.m_entity.isEntityRemote)
		{
			return;
		}
		if (this.m_entity.IsDead())
		{
			return;
		}
		int num = this.waitTicks + 1;
		this.waitTicks = num;
		if (num >= 10)
		{
			this.waitTicks = 0;
		}
		this.m_isEntityPlayer = (this.m_entity as EntityPlayer != null || this.m_entity as EntityPlayerLocal != null);
		dt = 0.5f;
		if (this.m_isEntityPlayer)
		{
			if (this.waitTicks == 1)
			{
				this.UpdateWeatherStats(dt, worldTime, this.m_entity.IsGodMode.Value);
			}
			if (this.waitTicks == 2)
			{
				this.UpdatePlayerFoodOT(dt);
				this.UpdatePlayerWaterOT(dt);
			}
			if (this.waitTicks == 3)
			{
				this.UpdatePlayerHealthOT(dt);
			}
			if (this.waitTicks == 4)
			{
				this.UpdatePlayerStaminaOT(dt);
			}
		}
		else if (this.waitTicks == 1)
		{
			this.UpdateNPCStatsOverTime(dt);
			this.Health.Tick(dt, 0UL, false);
			this.Stamina.Tick(dt, 0UL, false);
		}
		if (this.waitTicks == 5)
		{
			if (this.Health.Changed)
			{
				this.SendStatChangePacket(NetPackageEntityStatChanged.EnumStat.Health);
				this.Health.Changed = false;
			}
			if (this.Stamina.Changed)
			{
				this.SendStatChangePacket(NetPackageEntityStatChanged.EnumStat.Stamina);
				this.Stamina.Changed = false;
			}
			if (this.Water.Changed)
			{
				this.SendStatChangePacket(NetPackageEntityStatChanged.EnumStat.Water);
				this.Water.Changed = false;
			}
			if (this.Food.Changed)
			{
				this.SendStatChangePacket(NetPackageEntityStatChanged.EnumStat.Food);
				this.Food.Changed = false;
			}
		}
		if (this.waitTicks == 6)
		{
			if (this.m_isEntityPlayer)
			{
				int i = 0;
				while (i < this.m_notifications.Count)
				{
					this.m_notifications[i].Tick(dt);
					if (this.m_notifications[i].Expired)
					{
						this.tmp_uiNotification = this.m_notifications[i];
						this.m_notifications.RemoveAt(i);
						for (int j = 0; j < this.notificationChangedDelegates.Count; j++)
						{
							this.notificationChangedDelegates[j].EntityUINotificationRemoved(this.tmp_uiNotification);
						}
					}
					else
					{
						i++;
					}
				}
			}
			if (this.netSyncWaitTicks > 0)
			{
				this.netSyncWaitTicks--;
				return;
			}
			this.netSyncWaitTicks = 10;
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageEntityStatsBuff>().Setup(this.m_entity, null), false, -1, -1, -1, null, 192);
				return;
			}
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageEntityStatsBuff>().Setup(this.m_entity, null), false);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public float AdjustTemperatureFromEnclosure(float _temperature)
	{
		if (_temperature < 70f)
		{
			if (_temperature + 30f < 70f)
			{
				_temperature = (_temperature + 30f) * this.m_amountEnclosed + _temperature * (1f - this.m_amountEnclosed);
			}
			else
			{
				_temperature = 70f * this.m_amountEnclosed + _temperature * (1f - this.m_amountEnclosed);
			}
		}
		else if (_temperature - 30f > 70f)
		{
			_temperature = (_temperature - 30f) * this.m_amountEnclosed + _temperature * (1f - this.m_amountEnclosed);
		}
		else
		{
			_temperature = 70f * this.m_amountEnclosed + _temperature * (1f - this.m_amountEnclosed);
		}
		return _temperature;
	}

	public float GetOutsideTemperature()
	{
		float num = WeatherManager.Instance.GetCurrentTemperatureValue();
		if (!this.m_isInShade)
		{
			if (WeatherManager.Instance.GetCurrentRainfallValue() > 0.25f && num > 70f)
			{
				num -= 10f;
			}
			float currentCloudThicknessPercent = WeatherManager.Instance.GetCurrentCloudThicknessPercent();
			float num2 = Mathf.Lerp(WeatherParams.OutsideTempChangeWhenInSun, WeatherParams.OutsideTempChangeWhenInSun * WeatherParams.OutsideTempChangeWhenInSunCloudScale, currentCloudThicknessPercent);
			num += num2;
		}
		else if (num > 70f)
		{
			num -= 8f;
		}
		else
		{
			num += 8f;
		}
		return this.AdjustTemperatureFromEnclosure(num);
	}

	public void UpdateNPCStatsOverTime(float dt)
	{
		List<EffectManager.ModifierValuesAndSources> valuesAndSources = EffectManager.GetValuesAndSources(PassiveEffects.HealthChangeOT, null, 0f, this.m_entity, null, default(FastTags<TagGroup.Global>), true, true);
		for (int i = 0; i < valuesAndSources.Count; i++)
		{
			EffectManager.ModifierValuesAndSources modifierValuesAndSources = valuesAndSources[i];
			if (modifierValuesAndSources.ParentType == MinEffectController.SourceParentType.BuffClass)
			{
				BuffValue buff = this.Entity.Buffs.GetBuff((string)modifierValuesAndSources.Source);
				if (buff != null && buff.BuffClass != null)
				{
					BuffClass buffClass = buff.BuffClass;
					float num = 0f;
					float num2 = 1f;
					buffClass.ModifyValue(this.Entity, PassiveEffects.HealthChangeOT, buff, ref num, ref num2, FastTags<TagGroup.Global>.none);
					float num3 = num * num2 * dt;
					if (num3 < 0f)
					{
						float num4 = -num3 + this.buffDamageRemainder;
						int num5 = (int)num4;
						this.buffDamageRemainder = num4 - (float)num5;
						if (num5 > 0)
						{
							DamageSource damageSource = new DamageSource(buffClass.DamageSource, buffClass.DamageType);
							damageSource.BuffClass = buffClass;
							this.Entity.DamageEntity(damageSource, num5, false, 0f);
						}
					}
					else if (num3 > 0f)
					{
						this.Health.Value += num3;
					}
				}
			}
			else
			{
				this.Health.Value += modifierValuesAndSources.Value * dt;
			}
		}
		this.Stamina.Value += EffectManager.GetValue(PassiveEffects.StaminaChangeOT, null, 0f, this.m_entity, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false) * dt;
	}

	public void UpdatePlayerFoodOT(float dt)
	{
		this.Food.RegenerationAmount += EffectManager.GetValue(PassiveEffects.FoodChangeOT, null, 0f, this.m_entity, null, this.Entity.CurrentMovementTag, true, true, true, true, true, 1, true, false) * dt;
		this.Food.MaxModifier = -EffectManager.GetValue(PassiveEffects.FoodMaxBlockage, null, 0f, this.m_entity, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
		this.Food.Tick(dt, 0UL, false);
		this.Food.RegenerationAmount = 0f;
	}

	public void UpdatePlayerWaterOT(float dt)
	{
		this.Water.RegenerationAmount += EffectManager.GetValue(PassiveEffects.WaterChangeOT, null, 0f, this.m_entity, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false) * dt;
		this.Water.MaxModifier = -EffectManager.GetValue(PassiveEffects.WaterMaxBlockage, null, 0f, this.m_entity, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
		this.Water.Tick(dt, 0UL, false);
		this.Water.RegenerationAmount = 0f;
	}

	public void UpdatePlayerHealthOT(float dt)
	{
		float num = EffectManager.GetValue(PassiveEffects.HealthChangeOT, null, 0f, this.m_entity, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
		if (this.Health.ValuePercent < 1f && num > 0f)
		{
			float waterPercent = this.GetWaterPercent();
			this.Health.RegenerationAmount = num * waterPercent * dt;
		}
		else if (num < 0f)
		{
			List<EffectManager.ModifierValuesAndSources> valuesAndSources = EffectManager.GetValuesAndSources(PassiveEffects.HealthChangeOT, null, 0f, this.m_entity, null, default(FastTags<TagGroup.Global>), true, true);
			float num2 = 0f;
			float num3 = 1f;
			for (int i = 0; i < valuesAndSources.Count; i++)
			{
				if (valuesAndSources[i].ParentType == MinEffectController.SourceParentType.BuffClass)
				{
					BuffValue buff = this.Entity.Buffs.GetBuff((string)valuesAndSources[i].Source);
					if (buff != null && buff.BuffClass != null && !buff.Remove)
					{
						BuffClass buffClass = buff.BuffClass;
						num2 = 0f;
						num3 = 1f;
						buffClass.ModifyValue(this.Entity, PassiveEffects.HealthChangeOT, buff, ref num2, ref num3, FastTags<TagGroup.Global>.none);
						num = num2 * num3 * dt;
						if (num < 0f)
						{
							DamageSourceEntity damageSourceEntity = new DamageSourceEntity(buffClass.DamageSource, buffClass.DamageType, buff.InstigatorId);
							damageSourceEntity.BuffClass = buffClass;
							this.Entity.DamageEntity(damageSourceEntity, (int)(-num + 0.5f), false, 0f);
						}
					}
				}
				else
				{
					this.Health.RegenerationAmount = valuesAndSources[i].Value * dt;
				}
			}
		}
		this.Health.MaxModifier = -EffectManager.GetValue(PassiveEffects.HealthMaxBlockage, null, 0f, this.m_entity, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
		this.Health.Tick(dt, 0UL, false);
	}

	public void UpdatePlayerStaminaOT(float dt)
	{
		float value = EffectManager.GetValue(PassiveEffects.StaminaChangeOT, null, 0f, this.m_entity, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
		if (this.Stamina.ValuePercent < 1f && value > 0f)
		{
			this.Stamina.RegenerationAmount = value * dt;
		}
		else if (value < 0f)
		{
			this.Stamina.RegenerationAmount = value * dt;
		}
		this.Stamina.RegenerationAmount = EffectManager.GetValue(PassiveEffects.StaminaChangeOT, null, this.Stamina.RegenerationAmount, this.Entity, null, this.Entity.CurrentMovementTag | this.Entity.CurrentStanceTag, true, true, true, true, true, 1, true, false) * dt;
		if (this.Stamina.RegenerationAmount > 0f)
		{
			float waterPercent = this.GetWaterPercent();
			this.Stamina.RegenerationAmount *= waterPercent;
		}
		this.Stamina.MaxModifier = -EffectManager.GetValue(PassiveEffects.StaminaMaxBlockage, null, 0f, this.m_entity, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
		this.Stamina.Tick(dt, 0UL, false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public float GetWaterPercent()
	{
		float num = this.Water.ValuePercentUI * (this.Water.Max * 0.01f);
		if (num != 0f)
		{
			if (num < 0.25f)
			{
				num = 0.25f;
			}
			else if (num < 0.5f)
			{
				num = 0.5f;
			}
			else
			{
				num = 1f;
			}
		}
		return num;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateWeatherStats(float dt, ulong worldTime, bool godMode)
	{
		this.m_amountEnclosed = this.GetAmountEnclosure();
		this.LightInsidePer = this.m_amountEnclosed;
		EntityAlive entity = this.Entity;
		EntityBuffs buffs = entity.Buffs;
		float wetnessPercentage = entity.GetWetnessPercentage();
		if (!EntityStats.WeatherSurvivalEnabled || WeatherManager.inWeatherGracePeriod || entity.IsGodMode.Value || entity.biomeStandingOn == null || buffs.HasBuff("god"))
		{
			buffs.SetCustomVar("_sheltered", this.m_amountEnclosed, true);
			buffs.SetCustomVar("_shaded", (float)(this.IsShaded() ? 1 : 0), true);
			buffs.SetCustomVar("_degreesabsorbed", 0f, true);
			buffs.SetCustomVar("_coretemp", 0f, true);
			buffs.SetCustomVar("_wetness", wetnessPercentage, true);
			buffs.SetCustomVar(".bodytemp", 70f, true);
			return;
		}
		this.m_isInShade = this.IsShaded();
		float num = this.GetOutsideTemperature();
		num -= 10f * wetnessPercentage;
		float value;
		if (num < 70f)
		{
			value = EffectManager.GetValue(PassiveEffects.HypothermalResist, null, 0f, entity, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
			num = Mathf.Min(70f, num + value);
		}
		else
		{
			value = EffectManager.GetValue(PassiveEffects.HyperthermalResist, null, 0f, entity, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
			num = Mathf.Max(70f, num - value);
		}
		if ((int)this.lastCoreTemp < (int)num)
		{
			this.lastCoreTemp += 1f;
		}
		else if ((int)this.lastCoreTemp > (int)num)
		{
			this.lastCoreTemp -= 1f;
		}
		buffs.SetCustomVar("_degreesabsorbed", value, true);
		buffs.SetCustomVar("_coretemp", this.lastCoreTemp - 70f, true);
		buffs.SetCustomVar("_sheltered", this.m_amountEnclosed, true);
		buffs.SetCustomVar("_shaded", (float)(this.m_isInShade ? 1 : 0), true);
		buffs.SetCustomVar("_wetness", wetnessPercentage, true);
		buffs.SetCustomVar(".bodytemp", this.lastCoreTemp, true);
	}

	public float WaterLevel
	{
		get
		{
			return this.m_seekWaterLevel;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public float GetAmountEnclosure()
	{
		float num = 1f;
		Vector3i vector3i = World.worldToBlockPos(this.m_entity.GetPosition());
		IChunk chunkFromWorldPos = this.m_entity.world.GetChunkFromWorldPos(vector3i);
		if (chunkFromWorldPos != null && vector3i.y >= 0 && vector3i.y < 255)
		{
			num = (float)Mathf.Max((int)chunkFromWorldPos.GetLight(vector3i.x, vector3i.y, vector3i.z, Chunk.LIGHT_TYPE.SUN), (int)chunkFromWorldPos.GetLight(vector3i.x, vector3i.y + 1, vector3i.z, Chunk.LIGHT_TYPE.SUN));
			num /= 15f;
		}
		return 1f - num;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool IsShaded()
	{
		Vector3 sunLightDirection = SkyManager.GetSunLightDirection();
		if (sunLightDirection.y > -0.25f)
		{
			return true;
		}
		Ray ray = new Ray(this.m_entity.getHeadPosition() - Origin.position + sunLightDirection * 0.5f, -sunLightDirection);
		bool result = false;
		RaycastHit raycastHit;
		if (Physics.SphereCast(ray, 0.5f, out raycastHit, float.PositiveInfinity, 65809))
		{
			result = (raycastHit.distance < float.PositiveInfinity);
		}
		return result;
	}

	public void ResetStats()
	{
		this.m_seekWaterLevel = 0f;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RemoveAllNotifications()
	{
		while (this.m_notifications.Count > 0)
		{
			for (int i = 0; i < this.notificationChangedDelegates.Count; i++)
			{
				this.notificationChangedDelegates[i].EntityUINotificationRemoved(this.m_notifications[0]);
			}
			this.m_notifications.RemoveAt(0);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SendStatChangePacket(NetPackageEntityStatChanged.EnumStat enumStat)
	{
		if (this.m_entity.world.IsRemote())
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageEntityStatChanged>().Setup(this.m_entity, this.LocalPlayerId, enumStat), false);
			return;
		}
		this.m_entity.world.entityDistributer.SendPacketToTrackedPlayersAndTrackedEntity(this.m_entity.entityId, -1, NetPackageManager.GetPackage<NetPackageEntityStatChanged>().Setup(this.m_entity, this.LocalPlayerId, enumStat), enumStat > NetPackageEntityStatChanged.EnumStat.Health);
	}

	public int LocalPlayerId
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (this.m_localPlayer == null)
			{
				this.m_localPlayer = this.m_entity.world.GetPrimaryPlayer();
				if (this.m_localPlayer != null)
				{
					this.m_localPlayerId = this.m_localPlayer.entityId;
				}
			}
			return this.m_localPlayerId;
		}
	}

	public EntityAlive Entity
	{
		get
		{
			return this.m_entity;
		}
		set
		{
			this.m_entity = value;
			this.Health.Entity = value;
			this.Stamina.Entity = value;
			this.CoreTemp.Entity = value;
			this.Water.Entity = value;
			this.Food.Entity = value;
			this.CreatePlayerNotifications();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CreatePlayerNotifications()
	{
	}

	public void Write(BinaryWriter stream)
	{
		stream.Write(9);
		stream.Write(this.m_immunity.Length);
		for (int i = 0; i < this.m_immunity.Length; i++)
		{
			stream.Write(this.m_immunity[i]);
		}
		ushort num = 0;
		this.Health.Write(stream, ref num);
		this.Stamina.Write(stream, ref num);
		this.CoreTemp.Write(stream, ref num);
		this.Water.Write(stream, ref num);
		this.Food.Write(stream, ref num);
		stream.Write(this.m_seekWaterLevel);
	}

	public void Read(BinaryReader stream)
	{
		int num = stream.ReadInt32();
		if (num > 3)
		{
			int num2 = stream.ReadInt32();
			for (int i = 0; i < num2; i++)
			{
				int num3 = stream.ReadInt32();
				if (i < this.m_immunity.Length)
				{
					this.m_immunity[i] = num3;
				}
			}
		}
		this.Health.Read(stream);
		this.Stamina.Read(stream);
		if (num < 8)
		{
			new Stat(this.Entity, 0f, 0f).Read(stream);
		}
		if (num > 4)
		{
			this.CoreTemp.Read(stream);
			this.Water.Read(stream);
			if (num > 8)
			{
				this.Food.Read(stream);
			}
		}
		else
		{
			this.CoreTemp.ResetAll();
			this.Water.ResetAll();
			this.Food.ResetAll();
			this.CoreTemp.Changed = false;
		}
		if (num > 5)
		{
			this.m_seekWaterLevel = stream.ReadSingle();
		}
	}

	public void ReadBeforeEmbeddedVersion(BinaryReader stream)
	{
		this.Health.Read(stream);
		this.Stamina.Read(stream);
		this.CoreTemp.ResetAll();
		this.CoreTemp.Changed = false;
		this.Water.ResetAll();
		this.Water.Changed = true;
		this.Food.ResetAll();
		this.Food.Changed = true;
	}

	public void InitWithOldFormatData(int health, int stamina, int sickness, int gassiness)
	{
		if (health != -2147483648)
		{
			this.Health.Value = (float)health;
		}
		if (stamina != -2147483648)
		{
			this.Stamina.Value = (float)stamina;
		}
	}

	public EntityStats SimpleClone()
	{
		EntityStats entityStats = new EntityStats(null);
		entityStats.Health.SimpleAssignFrom(this.Health);
		entityStats.Stamina.SimpleAssignFrom(this.Stamina);
		entityStats.CoreTemp.SimpleAssignFrom(this.CoreTemp);
		entityStats.Water.SimpleAssignFrom(this.Water);
		entityStats.Food.SimpleAssignFrom(this.Food);
		return entityStats;
	}

	public const int kBinaryVersion = 9;

	public static bool WeatherSurvivalEnabled = true;

	public static bool NewWeatherSurvivalEnabled = false;

	public Stat Health;

	public Stat Stamina;

	public Stat CoreTemp;

	public Stat Water;

	public Stat Food;

	public float LightInsidePer;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<IEntityBuffsChanged> buffChangedDelegates;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<IEntityUINotificationChanged> notificationChangedDelegates;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<EntityUINotification> m_notifications;

	[PublicizedFrom(EAccessModifier.Private)]
	public PlayerTemperatureUINotification m_tempNotification;

	[PublicizedFrom(EAccessModifier.Private)]
	public int[] m_immunity = new int[0];

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityPlayer m_localPlayer;

	[PublicizedFrom(EAccessModifier.Private)]
	public int m_localPlayerId = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public float m_seekWaterLevel;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool m_isInShade;

	[PublicizedFrom(EAccessModifier.Private)]
	public float m_amountEnclosed;

	[PublicizedFrom(EAccessModifier.Private)]
	public float m_heightTemperatureOffset;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool m_isEntityPlayer;

	[PublicizedFrom(EAccessModifier.Private)]
	public float buffDamageRemainder;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int raycastMask = 65809;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cMaxWaitTicks = 10;

	[PublicizedFrom(EAccessModifier.Private)]
	public int waitTicks;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int MaxNetSyncWaitTicks = 10;

	[PublicizedFrom(EAccessModifier.Private)]
	public int netSyncWaitTicks = 10;

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityUINotification tmp_uiNotification;

	[PublicizedFrom(EAccessModifier.Private)]
	public float lastCoreTemp = 70f;

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityAlive m_entity;
}
