using System;
using System.Collections.Generic;
using System.IO;
using Audio;
using UnityEngine;

public class Equipment
{
	public event Action OnChanged;

	public Equipment()
	{
		int num = 5;
		this.m_slots = new ItemValue[num];
		this.preferredItemSlots = new int[num];
	}

	public Equipment(EntityAlive _entity) : this()
	{
		this.m_entity = _entity;
	}

	public void ModifyValue(ItemValue _originalItemValue, PassiveEffects _passiveEffect, ref float _base_val, ref float _perc_val, FastTags<TagGroup.Global> tags, bool _useDurability = false)
	{
		for (int i = 0; i < this.m_slots.Length; i++)
		{
			ItemValue itemValue = this.m_slots[i];
			if (itemValue != null && !itemValue.Equals(_originalItemValue) && itemValue.ItemClass != null)
			{
				itemValue.ModifyValue(this.m_entity, _originalItemValue, _passiveEffect, ref _base_val, ref _perc_val, tags, true, _useDurability);
			}
		}
	}

	public void GetModifiedValueData(List<EffectManager.ModifierValuesAndSources> _modValueSources, EffectManager.ModifierValuesAndSources.ValueSourceType _sourceType, ItemValue _originalItemValue, PassiveEffects _passiveEffect, ref float _base_val, ref float _perc_val, FastTags<TagGroup.Global> tags)
	{
		for (int i = 0; i < this.m_slots.Length; i++)
		{
			ItemValue itemValue = this.m_slots[i];
			if (itemValue != null && !itemValue.Equals(_originalItemValue) && itemValue.ItemClass != null)
			{
				itemValue.GetModifiedValueData(_modValueSources, _sourceType, this.m_entity, _originalItemValue, _passiveEffect, ref _base_val, ref _perc_val, tags);
			}
		}
	}

	public void FireEvent(MinEventTypes _eventType, MinEventParams _params)
	{
		for (int i = 0; i < this.m_slots.Length; i++)
		{
			ItemValue itemValue = this.m_slots[i];
			if (itemValue != null && itemValue.ItemClass != null)
			{
				itemValue.FireEvent(_eventType, _params);
			}
		}
	}

	public void DropItems()
	{
		for (int i = 0; i < this.m_slots.Length; i++)
		{
			ItemValue itemValue = this.m_slots[i];
			if (itemValue != null)
			{
				this.DropItemOnGround(itemValue);
				this.SetSlotItem(i, null, true);
			}
		}
		this.updateInsulation();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateInsulation()
	{
		this.waterProof = 0f;
		for (int i = 0; i < this.m_slots.Length; i++)
		{
			ItemValue itemValue = this.m_slots[i];
			if (itemValue != null)
			{
				this.waterProof += itemValue.ItemClass.WaterProof;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void DropItemOnGround(ItemValue _itemValue)
	{
		this.m_entity.world.GetGameManager().ItemDropServer(new ItemStack(_itemValue, 1), this.m_entity.GetPosition(), new Vector3(0.5f, 0f, 0.5f), this.m_entity.belongsPlayerId, 60f, false);
	}

	public float GetTotalInsulation()
	{
		return this.insulation;
	}

	public float GetTotalWaterproof()
	{
		return this.waterProof;
	}

	public int GetSlotCount()
	{
		return this.m_slots.Length;
	}

	public ItemValue[] GetItems()
	{
		return this.m_slots;
	}

	public ItemValue GetSlotItem(int index)
	{
		return this.m_slots[index];
	}

	public ItemValue GetSlotItemOrNone(int index)
	{
		ItemValue itemValue = this.m_slots[index];
		if (itemValue == null)
		{
			return ItemValue.None;
		}
		return itemValue;
	}

	public bool HasAnyItems()
	{
		for (int i = 0; i < this.m_slots.Length; i++)
		{
			if (this.m_slots[i] != null)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsNaked()
	{
		return !this.HasAnyItems();
	}

	public void CalcDamage(ref int entityDamageTaken, ref int armorDamageTaken, FastTags<TagGroup.Global> damageTypeTag, EntityAlive attacker, ItemValue attackingItem)
	{
		armorDamageTaken = entityDamageTaken;
		if (damageTypeTag.Test_AnySet(Equipment.physicalDamageTypes))
		{
			if (entityDamageTaken > 0)
			{
				float totalPhysicalArmorResistPercent = this.GetTotalPhysicalArmorResistPercent(attacker, attackingItem);
				armorDamageTaken = Utils.FastMax((totalPhysicalArmorResistPercent > 0f) ? 1 : 0, Mathf.RoundToInt((float)entityDamageTaken * totalPhysicalArmorResistPercent));
				entityDamageTaken -= armorDamageTaken;
				return;
			}
		}
		else
		{
			entityDamageTaken = Mathf.RoundToInt(Utils.FastMax(0f, (float)entityDamageTaken * (1f - EffectManager.GetValue(PassiveEffects.ElementalDamageResist, null, 0f, this.m_entity, null, damageTypeTag, true, true, true, true, true, 1, true, false) / 100f)));
			armorDamageTaken = Mathf.RoundToInt((float)Utils.FastMax(0, armorDamageTaken - entityDamageTaken));
		}
	}

	public float GetTotalPhysicalArmorResistPercent(EntityAlive attacker, ItemValue attackingItem)
	{
		return this.GetTotalPhysicalArmorRating(attacker, attackingItem) / 100f;
	}

	public float GetTotalPhysicalArmorRating(EntityAlive attacker, ItemValue attackingItem)
	{
		float value = EffectManager.GetValue(PassiveEffects.PhysicalDamageResist, null, 0f, this.m_entity, null, Equipment.coreDamageResist, true, true, true, true, true, 1, true, true);
		return EffectManager.GetValue(PassiveEffects.TargetArmor, attackingItem, value, attacker, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
	}

	public List<ItemValue> GetArmor()
	{
		List<ItemValue> list = new List<ItemValue>();
		for (int i = 0; i < this.m_slots.Length; i++)
		{
			ItemValue itemValue = this.m_slots[i];
			if (itemValue != null)
			{
				float num = 0f;
				float num2 = 1f;
				itemValue.ModifyValue(this.m_entity, null, PassiveEffects.PhysicalDamageResist, ref num, ref num2, FastTags<TagGroup.Global>.all, true, false);
				if (num != 0f)
				{
					list.Add(itemValue);
				}
			}
		}
		return list;
	}

	public bool CheckBreakUseItems()
	{
		bool result = false;
		this.CurrentLowestDurability = 1f;
		for (int i = 0; i < this.m_slots.Length; i++)
		{
			ItemValue itemValue = this.m_slots[i];
			if (itemValue != null)
			{
				ItemClass forId = ItemClass.GetForId(itemValue.type);
				float percentUsesLeft = itemValue.PercentUsesLeft;
				if (percentUsesLeft < this.CurrentLowestDurability)
				{
					this.CurrentLowestDurability = percentUsesLeft;
				}
				if (forId != null && itemValue.MaxUseTimes > 0 && forId.MaxUseTimesBreaksAfter.Value && itemValue.UseTimes > (float)itemValue.MaxUseTimes)
				{
					this.SetSlotItem(i, null, true);
					if (this.m_entity != null && forId.Properties.Values.ContainsKey(ItemClass.PropSoundDestroy))
					{
						Manager.BroadcastPlay(this.m_entity, forId.Properties.Values[ItemClass.PropSoundDestroy], false);
					}
					result = true;
				}
			}
		}
		return result;
	}

	public void SetSlotItem(int index, ItemValue value, bool isLocal = true)
	{
		if (value != null && value.IsEmpty())
		{
			value = null;
		}
		ItemValue itemValue = this.m_slots[index];
		if (value == null && itemValue == null)
		{
			return;
		}
		this.m_entity.IsEquipping = true;
		bool flag = false;
		if (itemValue != null && itemValue.Equals(value))
		{
			this.m_slots[index] = value;
			this.m_entity.MinEventContext.ItemValue = value;
			value.FireEvent(MinEventTypes.onSelfEquipStart, this.m_entity.MinEventContext);
		}
		else
		{
			flag = true;
			if (itemValue != null)
			{
				if (itemValue.ItemClass.HasTrigger(MinEventTypes.onSelfItemActivate) && itemValue.Activated != 0)
				{
					this.m_entity.MinEventContext.ItemValue = itemValue;
					itemValue.FireEvent(MinEventTypes.onSelfItemDeactivate, this.m_entity.MinEventContext);
					itemValue.Activated = 0;
				}
				for (int i = 0; i < itemValue.Modifications.Length; i++)
				{
					ItemValue itemValue2 = itemValue.Modifications[i];
					if (itemValue2 != null)
					{
						ItemClass itemClass = itemValue2.ItemClass;
						if (itemClass != null && itemClass.HasTrigger(MinEventTypes.onSelfItemActivate) && itemValue2.Activated != 0)
						{
							this.m_entity.MinEventContext.ItemValue = itemValue2;
							itemValue2.FireEvent(MinEventTypes.onSelfItemDeactivate, this.m_entity.MinEventContext);
							itemValue2.Activated = 0;
						}
					}
				}
				this.m_entity.MinEventContext.ItemValue = itemValue;
				itemValue.FireEvent(MinEventTypes.onSelfEquipStop, this.m_entity.MinEventContext);
			}
			this.preferredItemSlots[index] = ((value != null) ? value.type : itemValue.type);
			this.m_slots[index] = value;
			this.slotsSetFlags |= 1 << index;
			this.slotsChangedFlags |= 1 << index;
		}
		if (flag)
		{
			if (this.m_entity && !this.m_entity.isEntityRemote)
			{
				this.m_entity.bPlayerStatsChanged = true;
			}
			this.ResetArmorGroups();
			if (this.OnChanged != null)
			{
				this.OnChanged();
			}
		}
		this.m_entity.IsEquipping = false;
	}

	public void SetSlotItemRaw(int index, ItemValue _iv)
	{
		if (_iv != null && _iv.IsEmpty())
		{
			_iv = null;
		}
		this.m_slots[index] = _iv;
	}

	public void FireEventsForSlots(MinEventTypes _event, int _flags = -1)
	{
		this.m_entity.MinEventContext.Self = this.m_entity;
		for (int i = 0; i < this.m_slots.Length; i++)
		{
			if ((_flags & 1 << i) > 0)
			{
				ItemValue itemValue = this.m_slots[i];
				if (itemValue != null && itemValue.ItemClass != null)
				{
					this.m_entity.MinEventContext.ItemValue = itemValue;
					itemValue.FireEvent(_event, this.m_entity.MinEventContext);
					for (int j = 0; j < itemValue.Modifications.Length; j++)
					{
						ItemValue itemValue2 = itemValue.Modifications[j];
						if (itemValue2 != null && itemValue2.ItemClass != null)
						{
							itemValue2.FireEvent(_event, this.m_entity.MinEventContext);
						}
					}
				}
			}
		}
	}

	public void FireEventsForSetSlots()
	{
		this.FireEventsForSlots(MinEventTypes.onSelfEquipStart, this.slotsSetFlags);
		this.slotsSetFlags = 0;
	}

	public void FireEventsForChangedSlots()
	{
		if (this.slotsChangedFlags == 0)
		{
			return;
		}
		this.m_entity.IsEquipping = true;
		for (int i = 0; i < this.m_slots.Length; i++)
		{
			if ((this.slotsChangedFlags & 1 << i) > 0)
			{
				ItemValue itemValue = this.m_slots[i];
				if (itemValue != null)
				{
					ItemClass itemClass = itemValue.ItemClass;
					if (itemClass != null)
					{
						this.m_entity.MinEventContext.Self = this.m_entity;
						this.m_entity.MinEventContext.ItemValue = itemValue;
						itemValue.FireEvent(MinEventTypes.onSelfEquipChanged, this.m_entity.MinEventContext);
						if (itemClass.HasTrigger(MinEventTypes.onSelfItemActivate))
						{
							if (itemValue.Activated == 0)
							{
								itemValue.FireEvent(MinEventTypes.onSelfItemDeactivate, this.m_entity.MinEventContext);
							}
							else
							{
								itemValue.FireEvent(MinEventTypes.onSelfItemActivate, this.m_entity.MinEventContext);
							}
						}
						for (int j = 0; j < itemValue.Modifications.Length; j++)
						{
							ItemValue itemValue2 = itemValue.Modifications[j];
							if (itemValue2 != null && itemValue2.ItemClass != null && itemValue2.ItemClass.HasTrigger(MinEventTypes.onSelfItemActivate))
							{
								this.m_entity.MinEventContext.ItemValue = itemValue2;
								if (itemValue2.Activated == 0)
								{
									itemValue2.FireEvent(MinEventTypes.onSelfItemDeactivate, this.m_entity.MinEventContext);
								}
								else
								{
									itemValue2.FireEvent(MinEventTypes.onSelfItemActivate, this.m_entity.MinEventContext);
								}
							}
						}
					}
				}
			}
		}
		this.slotsChangedFlags = 0;
		this.m_entity.IsEquipping = false;
	}

	public void Update()
	{
		if (Time.time - this.lastUpdateTime >= 1f)
		{
			for (int i = 0; i < this.m_slots.Length; i++)
			{
				ItemValue itemValue = this.m_slots[i];
				if (itemValue != null)
				{
					this.m_entity.MinEventContext.ItemValue = itemValue;
					itemValue.FireEvent(MinEventTypes.onSelfEquipUpdate, this.m_entity.MinEventContext);
				}
			}
			this.lastUpdateTime = Time.time;
		}
	}

	public int PreferredItemSlot(ItemValue _itemValue)
	{
		for (int i = 0; i < this.preferredItemSlots.Length; i++)
		{
			if (_itemValue.type == this.preferredItemSlots[i])
			{
				return i;
			}
		}
		return -1;
	}

	public int PreferredItemSlot(ItemStack _itemStack)
	{
		for (int i = 0; i < this.preferredItemSlots.Length; i++)
		{
			if (_itemStack.itemValue.type == this.preferredItemSlots[i])
			{
				return i;
			}
		}
		return -1;
	}

	public void Write(BinaryWriter writer)
	{
		writer.Write(1);
		for (int i = 0; i < this.m_slots.Length; i++)
		{
			ItemValue.Write(this.m_slots[i], writer);
		}
	}

	public static Equipment Read(BinaryReader reader)
	{
		reader.ReadByte();
		Equipment equipment = new Equipment();
		for (int i = 0; i < equipment.m_slots.Length; i++)
		{
			equipment.m_slots[i] = ItemValue.ReadOrNull(reader);
		}
		return equipment;
	}

	public virtual bool ReturnItem(ItemStack _itemStack, bool isLocal = true)
	{
		int num = this.PreferredItemSlot(_itemStack);
		if (num < 0 || num >= this.m_slots.Length)
		{
			return false;
		}
		if (this.m_slots[num] == null)
		{
			this.SetSlotItem(num, _itemStack.itemValue, isLocal);
			return true;
		}
		return false;
	}

	public void Apply(Equipment eq, bool isLocal = true)
	{
		for (int i = 0; i < this.m_slots.Length; i++)
		{
			this.SetSlotItem(i, eq.m_slots[i], isLocal);
		}
		this.FireEventsForSetSlots();
		EModelSDCS emodelSDCS = this.m_entity.emodel as EModelSDCS;
		if (emodelSDCS != null)
		{
			emodelSDCS.UpdateEquipment();
		}
		this.FireEventsForChangedSlots();
	}

	public void InitializeEquipmentTransforms()
	{
		this.FireEventsForSlots(MinEventTypes.onSelfEquipStop, -1);
		this.FireEventsForSlots(MinEventTypes.onSelfEquipStart, -1);
		this.slotsSetFlags = 0;
		this.slotsChangedFlags = -1;
		this.FireEventsForChangedSlots();
	}

	public Equipment Clone()
	{
		Equipment equipment = new Equipment();
		for (int i = 0; i < this.m_slots.Length; i++)
		{
			if (this.m_slots[i] != null)
			{
				equipment.m_slots[i] = this.m_slots[i].Clone();
			}
		}
		return equipment;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void AddArmorGroup(string armorGroup, int quality)
	{
		if (this.ArmorGroupEquipped.ContainsKey(armorGroup))
		{
			Equipment.ArmorGroupInfo armorGroupInfo = this.ArmorGroupEquipped[armorGroup];
			armorGroupInfo.Count++;
			if (armorGroupInfo.LowestQuality > quality)
			{
				armorGroupInfo.LowestQuality = quality;
				return;
			}
		}
		else
		{
			this.ArmorGroupEquipped[armorGroup] = new Equipment.ArmorGroupInfo
			{
				Count = 1,
				LowestQuality = quality
			};
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ResetArmorGroups()
	{
		this.ArmorGroupEquipped.Clear();
		for (int i = 0; i < this.m_slots.Length; i++)
		{
			ItemValue itemValue = this.m_slots[i];
			if (itemValue != null)
			{
				ItemClassArmor itemClassArmor = itemValue.ItemClass as ItemClassArmor;
				if (itemClassArmor != null)
				{
					for (int j = 0; j < itemClassArmor.ArmorGroup.Length; j++)
					{
						this.AddArmorGroup(itemClassArmor.ArmorGroup[j], (int)itemValue.Quality);
					}
				}
			}
		}
	}

	public int GetArmorGroupCount(string armorGroup)
	{
		if (this.ArmorGroupEquipped.ContainsKey(armorGroup))
		{
			return this.ArmorGroupEquipped[armorGroup].Count;
		}
		return 0;
	}

	public int GetArmorGroupLowestQuality(string armorGroup)
	{
		if (this.ArmorGroupEquipped.ContainsKey(armorGroup))
		{
			return this.ArmorGroupEquipped[armorGroup].LowestQuality;
		}
		return 0;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const int CurrentSaveVersion = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemValue[] m_slots;

	[PublicizedFrom(EAccessModifier.Private)]
	public int slotsSetFlags;

	[PublicizedFrom(EAccessModifier.Private)]
	public int slotsChangedFlags;

	[PublicizedFrom(EAccessModifier.Private)]
	public int[] preferredItemSlots;

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityAlive m_entity;

	[PublicizedFrom(EAccessModifier.Private)]
	public float insulation;

	[PublicizedFrom(EAccessModifier.Private)]
	public float waterProof;

	public float CurrentLowestDurability = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<string, Equipment.ArmorGroupInfo> ArmorGroupEquipped = new Dictionary<string, Equipment.ArmorGroupInfo>();

	[PublicizedFrom(EAccessModifier.Private)]
	public float lastUpdateTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public static FastTags<TagGroup.Global> physicalDamageTypes = FastTags<TagGroup.Global>.Parse("piercing,bashing,slashing,crushing,none,corrosive");

	[PublicizedFrom(EAccessModifier.Private)]
	public static FastTags<TagGroup.Global> piercingDamage = FastTags<TagGroup.Global>.Parse("piercing");

	[PublicizedFrom(EAccessModifier.Private)]
	public static FastTags<TagGroup.Global> slashingDamage = FastTags<TagGroup.Global>.Parse("slashing");

	[PublicizedFrom(EAccessModifier.Private)]
	public static FastTags<TagGroup.Global> corrosiveDamage = FastTags<TagGroup.Global>.Parse("corrosive");

	[PublicizedFrom(EAccessModifier.Private)]
	public static FastTags<TagGroup.Global> coreDamageResist = FastTags<TagGroup.Global>.Parse("coredamageresist");

	[PublicizedFrom(EAccessModifier.Private)]
	public class ArmorGroupInfo
	{
		public int Count;

		public int LowestQuality;
	}
}
