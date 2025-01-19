using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ItemValue : IEquatable<ItemValue>
{
	public int MaxUseTimes
	{
		get
		{
			return (int)EffectManager.GetValue(PassiveEffects.DegradationMax, this, 0f, null, null, (this.ItemClass != null) ? this.ItemClass.ItemTags : FastTags<TagGroup.Global>.none, true, true, true, true, true, 1, true, false);
		}
	}

	public float PercentUsesLeft
	{
		get
		{
			int maxUseTimes = this.MaxUseTimes;
			if (maxUseTimes > 0)
			{
				return 1f - Mathf.Clamp01(this.UseTimes / (float)maxUseTimes);
			}
			return 1f;
		}
	}

	public bool HasMetadata(string key, TypedMetadataValue.TypeTag typeTag = TypedMetadataValue.TypeTag.None)
	{
		if (this.Metadata == null)
		{
			return false;
		}
		if (typeTag == TypedMetadataValue.TypeTag.None)
		{
			return this.Metadata.ContainsKey(key);
		}
		TypedMetadataValue typedMetadataValue = this.Metadata[key];
		return typedMetadataValue.ValueMatchesTag(typedMetadataValue.GetValue(), typeTag);
	}

	public object GetMetadata(string key)
	{
		if (this.Metadata == null)
		{
			return false;
		}
		TypedMetadataValue typedMetadataValue;
		if (this.Metadata.TryGetValue(key, out typedMetadataValue))
		{
			return typedMetadataValue.GetValue();
		}
		return null;
	}

	public void SetMetadata(string key, object value, string typeTag)
	{
		TypedMetadataValue.TypeTag typeTag2 = TypedMetadataValue.StringToTag(typeTag);
		this.SetMetadata(key, value, typeTag2);
	}

	public void SetMetadata(string key, object value, TypedMetadataValue.TypeTag typeTag)
	{
		if (this.Metadata == null)
		{
			this.Metadata = new Dictionary<string, TypedMetadataValue>();
		}
		TypedMetadataValue typedMetadataValue;
		if (this.Metadata.TryGetValue(key, out typedMetadataValue))
		{
			typedMetadataValue.SetValue(value);
			return;
		}
		this.Metadata.Add(key, new TypedMetadataValue(value, typeTag));
	}

	public void SetMetadata(string key, TypedMetadataValue tmv)
	{
		if (this.Metadata == null)
		{
			this.Metadata = new Dictionary<string, TypedMetadataValue>();
		}
		TypedMetadataValue typedMetadataValue;
		if (this.Metadata.TryGetValue(key, out typedMetadataValue))
		{
			typedMetadataValue.SetValue(tmv.GetValue());
			return;
		}
		this.Metadata.Add(key, tmv);
	}

	public bool HasQuality
	{
		get
		{
			ItemClass itemClass = this.ItemClass;
			return itemClass != null && (itemClass.HasQuality || itemClass is ItemClassModifier);
		}
	}

	public bool HasModSlots
	{
		get
		{
			return this.Modifications.Length != 0;
		}
	}

	public bool IsMod
	{
		get
		{
			ItemClass itemClass = this.ItemClass;
			return itemClass != null && itemClass is ItemClassModifier;
		}
	}

	public bool IsShapeHelperBlock
	{
		get
		{
			ItemClassBlock itemClassBlock = this.ItemClass as ItemClassBlock;
			return itemClassBlock != null && itemClassBlock.GetBlock().SelectAlternates;
		}
	}

	public long Texture
	{
		get
		{
			return (long)(((ulong)this.Quality & 65535UL) << 48 | (ulong)((ulong)((long)this.Meta & 65535L) << 32) | (ulong)((ulong)((long)this.UseTimes & 65535L) << 16) | ((ulong)this.Activated & 255UL) << 8 | ((ulong)this.SelectedAmmoTypeIndex & 255UL));
		}
		set
		{
			this.Quality = (ushort)(value >> 48 & 65535L);
			this.Meta = (int)(value >> 32 & 65535L);
			this.UseTimes = (float)((int)(value >> 16 & 65535L));
			this.Activated = (byte)(value >> 8 & 255L);
			this.SelectedAmmoTypeIndex = (byte)(value & 255L);
		}
	}

	public int TextureAllSides
	{
		get
		{
			return (int)(this.Quality & ushort.MaxValue);
		}
		set
		{
			this.Quality = (ushort)(value & 65535);
		}
	}

	public ItemClass ItemClass
	{
		get
		{
			if (this.type < 0 || ItemClass.list == null || this.type >= ItemClass.list.Length)
			{
				return null;
			}
			ItemClass itemClass = ItemClass.list[this.type];
			if (itemClass is ItemClassQuest)
			{
				return ItemClassQuest.GetItemQuestById(this.Seed);
			}
			return itemClass;
		}
	}

	public ItemClass ItemClassOrMissing
	{
		get
		{
			ItemClass itemClass = this.ItemClass;
			if (itemClass != null)
			{
				return itemClass;
			}
			return ItemClass.GetItemClass("missingItem", false);
		}
	}

	public ItemValue()
	{
		this.Modifications = ItemValue.emptyItemValueArray;
		this.CosmeticMods = ItemValue.emptyItemValueArray;
	}

	public ItemValue(int _type, bool _bCreateDefaultParts = false) : this(_type, 1, 6, _bCreateDefaultParts, null, 1f)
	{
	}

	public ItemValue(int _type, int minQuality, int maxQuality, bool _bCreateDefaultModItems = false, string[] modsToInstall = null, float modInstallDescendingChance = 1f)
	{
		this.type = _type;
		this.Modifications = ItemValue.emptyItemValueArray;
		this.CosmeticMods = ItemValue.emptyItemValueArray;
		if (this.type == 0)
		{
			return;
		}
		DateTime utcNow = DateTime.UtcNow;
		this.Seed = (ushort)((utcNow - ItemValue.baseDate).Seconds + utcNow.Millisecond + this.type);
		if (!ThreadManager.IsMainThread())
		{
			return;
		}
		ItemClass itemClass = this.ItemClass;
		if (itemClass == null)
		{
			return;
		}
		GameRandom gameRandom = null;
		if (itemClass.HasQuality)
		{
			gameRandom = GameRandomManager.Instance.CreateGameRandom((int)this.Seed);
			this.Quality = (ushort)Math.Min(65535, gameRandom.RandomRange(minQuality, maxQuality + 1));
		}
		if (itemClass is ItemClassModifier)
		{
			GameRandomManager.Instance.FreeGameRandom(gameRandom);
			return;
		}
		if (itemClass.Stacknumber.Value > 1)
		{
			GameRandomManager.Instance.FreeGameRandom(gameRandom);
			return;
		}
		this.Modifications = new ItemValue[Math.Min(255, (int)EffectManager.GetValue(PassiveEffects.ModSlots, this, (float)Utils.FastMax(0, (int)(this.Quality - 1)), null, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false))];
		this.CosmeticMods = new ItemValue[itemClass.HasAnyTags(ItemClassModifier.CosmeticItemTags) ? 1 : 0];
		if (_bCreateDefaultModItems)
		{
			if (gameRandom == null)
			{
				gameRandom = GameRandomManager.Instance.CreateGameRandom((int)this.Seed);
			}
			this.createDefaultModItems(itemClass, gameRandom, modsToInstall, modInstallDescendingChance);
		}
		GameRandomManager.Instance.FreeGameRandom(gameRandom);
	}

	public void Clear()
	{
		this.type = 0;
		this.UseTimes = 0f;
		this.Quality = 0;
		this.Meta = 0;
		this.Seed = 0;
		this.Modifications = new ItemValue[0];
		this.CosmeticMods = new ItemValue[0];
		this.Metadata = null;
		this.SelectedAmmoTypeIndex = 0;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void createDefaultModItems(ItemClass ic, GameRandom random, string[] modsToInstall, float modInstallDescendingChance)
	{
		FastTags<TagGroup.Global> fastTags = FastTags<TagGroup.Global>.none;
		bool flag = false;
		bool flag2 = false;
		if (modsToInstall != null && modsToInstall.Length != 0)
		{
			float num = modInstallDescendingChance;
			if (!ic.ItemTags.IsEmpty)
			{
				int num2 = 0;
				for (int i = 0; i < modsToInstall.Length; i++)
				{
					ItemClassModifier itemClassModifier = ItemClass.GetItemClass(modsToInstall[i], true) as ItemClassModifier;
					if (itemClassModifier == null)
					{
						itemClassModifier = ItemClassModifier.GetDesiredItemModWithAnyTags(ic.ItemTags, fastTags, FastTags<TagGroup.Global>.Parse(modsToInstall[i]), random);
					}
					if (itemClassModifier != null)
					{
						if (itemClassModifier.HasAnyTags(ItemClassModifier.CosmeticModTypes))
						{
							flag = true;
							if (!flag2 && random.RandomFloat <= modInstallDescendingChance)
							{
								this.CosmeticMods[0] = new ItemValue(itemClassModifier.Id, false);
								fastTags |= itemClassModifier.ItemTags;
								flag2 = true;
								Log.Warning("ItemValue createDefaultModItems cosmetic {0}", new object[]
								{
									this.CosmeticMods[0]
								});
							}
						}
						else if (num2 < this.Modifications.Length && random.RandomFloat <= num)
						{
							this.Modifications[num2] = new ItemValue(itemClassModifier.Id, false);
							fastTags |= itemClassModifier.ItemTags;
							num2++;
							num *= 0.5f;
						}
					}
				}
				for (int j = num2; j < this.Modifications.Length; j++)
				{
					this.Modifications[j] = ItemValue.None.Clone();
				}
			}
		}
		if (!flag && !ic.HasAnyTags(ItemValue.noPreinstallCosmeticItemTags))
		{
			for (int k = 0; k < this.CosmeticMods.Length; k++)
			{
				ItemClassModifier cosmeticItemMod = ItemClassModifier.GetCosmeticItemMod(ic.ItemTags, fastTags, random);
				if (cosmeticItemMod != null)
				{
					this.CosmeticMods[k] = new ItemValue(cosmeticItemMod.Id, false);
					fastTags |= cosmeticItemMod.ItemTags;
				}
				else
				{
					this.CosmeticMods[k] = ItemValue.None.Clone();
				}
			}
		}
	}

	public float GetValue(EntityAlive _entity, PassiveEffects _passiveEffect, FastTags<TagGroup.Global> _tags)
	{
		float num = 0f;
		float num2 = 1f;
		ItemClass itemClass = this.ItemClass;
		if (itemClass != null)
		{
			if (_entity != null)
			{
				MinEventParams.CopyTo(_entity.MinEventContext, MinEventParams.CachedEventParam);
			}
			if (itemClass.Actions != null && itemClass.Actions.Length != 0 && itemClass.Actions[0] is ItemActionRanged)
			{
				string[] magazineItemNames = (itemClass.Actions[0] as ItemActionRanged).MagazineItemNames;
				if (magazineItemNames != null)
				{
					ItemClass.GetItem(magazineItemNames[(int)this.SelectedAmmoTypeIndex], false).ModifyValue(_entity, this, _passiveEffect, ref num, ref num2, _tags, true, false);
				}
			}
			if (itemClass.Effects != null)
			{
				int seed = MinEventParams.CachedEventParam.Seed;
				if (_entity != null)
				{
					seed = _entity.MinEventContext.Seed;
				}
				MinEventParams.CachedEventParam.Seed = (int)((byte)this.Seed + ((this.Seed != 0) ? _passiveEffect : PassiveEffects.None));
				if (_entity != null)
				{
					_entity.MinEventContext.Seed = MinEventParams.CachedEventParam.Seed;
				}
				itemClass.Effects.ModifyValue(_entity, _passiveEffect, ref num, ref num2, (float)this.Quality, _tags, 1);
				MinEventParams.CachedEventParam.Seed = seed;
				if (_entity != null)
				{
					_entity.MinEventContext.Seed = seed;
				}
			}
		}
		return num * num2;
	}

	public void ModifyValue(EntityAlive _entity, ItemValue _originalItemValue, PassiveEffects _passiveEffect, ref float _originalValue, ref float _perc_value, FastTags<TagGroup.Global> _tags, bool _useMods = true, bool _useDurability = false)
	{
		if (_originalItemValue == null || !_originalItemValue.Equals(this))
		{
			int seed = MinEventParams.CachedEventParam.Seed;
			if (_entity != null)
			{
				seed = _entity.MinEventContext.Seed;
			}
			ItemClass itemClass = this.ItemClass;
			if (itemClass != null)
			{
				if (itemClass.Actions != null && itemClass.Actions.Length != 0 && itemClass.Actions[0] is ItemActionRanged)
				{
					string[] magazineItemNames = (itemClass.Actions[0] as ItemActionRanged).MagazineItemNames;
					if (magazineItemNames != null)
					{
						ItemClass itemClass2 = ItemClass.GetItemClass(magazineItemNames[(int)this.SelectedAmmoTypeIndex], false);
						if (itemClass2 != null && itemClass2.Effects != null)
						{
							itemClass2.Effects.ModifyValue(_entity, _passiveEffect, ref _originalValue, ref _perc_value, 0f, _tags, 1);
						}
					}
				}
				if (itemClass.Effects != null)
				{
					ItemValue itemValue = MinEventParams.CachedEventParam.ItemValue;
					ItemValue itemValue2 = (_entity != null) ? _entity.MinEventContext.ItemValue : null;
					MinEventParams.CachedEventParam.Seed = (int)((byte)this.Seed + ((this.Seed != 0) ? _passiveEffect : PassiveEffects.None));
					MinEventParams.CachedEventParam.ItemValue = this;
					if (_entity != null)
					{
						_entity.MinEventContext.Seed = MinEventParams.CachedEventParam.Seed;
						_entity.MinEventContext.ItemValue = this;
					}
					float num = _originalValue;
					itemClass.Effects.ModifyValue(_entity, _passiveEffect, ref _originalValue, ref _perc_value, (float)this.Quality, _tags, 1);
					if (_useDurability)
					{
						if (_passiveEffect != PassiveEffects.PhysicalDamageResist)
						{
							if (_passiveEffect != PassiveEffects.ElementalDamageResist)
							{
								if (_passiveEffect == PassiveEffects.BuffResistance)
								{
									if (this.PercentUsesLeft < 0.5f)
									{
										float num2 = _originalValue - num;
										_originalValue = num + num2 * this.PercentUsesLeft * 2f;
									}
								}
							}
							else if (this.PercentUsesLeft < 0.5f)
							{
								float num3 = _originalValue - num;
								_originalValue = num + num3 * this.PercentUsesLeft * 2f;
							}
						}
						else if (this.PercentUsesLeft < 0.5f)
						{
							float num4 = _originalValue - num;
							_originalValue = num + num4 * this.PercentUsesLeft * 2f;
						}
					}
					MinEventParams.CachedEventParam.ItemValue = itemValue;
					if (_entity != null)
					{
						_entity.MinEventContext.ItemValue = itemValue2;
					}
				}
			}
			if (_useMods)
			{
				for (int i = 0; i < this.CosmeticMods.Length; i++)
				{
					if (this.CosmeticMods[i] != null && this.CosmeticMods[i].ItemClass is ItemClassModifier)
					{
						this.CosmeticMods[i].ModifyValue(_entity, _originalItemValue, _passiveEffect, ref _originalValue, ref _perc_value, _tags, true, false);
					}
				}
				for (int j = 0; j < this.Modifications.Length; j++)
				{
					if (this.Modifications[j] != null && this.Modifications[j].ItemClass is ItemClassModifier)
					{
						this.Modifications[j].ModifyValue(_entity, _originalItemValue, _passiveEffect, ref _originalValue, ref _perc_value, _tags, true, false);
					}
				}
			}
			MinEventParams.CachedEventParam.Seed = seed;
			if (_entity != null)
			{
				_entity.MinEventContext.Seed = seed;
			}
		}
	}

	public void GetModifiedValueData(List<EffectManager.ModifierValuesAndSources> _modValueSources, EffectManager.ModifierValuesAndSources.ValueSourceType _sourceType, EntityAlive _entity, ItemValue _originalItemValue, PassiveEffects _passiveEffect, ref float _originalValue, ref float _perc_value, FastTags<TagGroup.Global> _tags)
	{
		if (_originalItemValue == null || !_originalItemValue.Equals(this))
		{
			ItemClass itemClass = this.ItemClass;
			if (itemClass != null)
			{
				if (itemClass.Actions != null && itemClass.Actions.Length != 0 && itemClass.Actions[0] is ItemActionRanged)
				{
					string[] magazineItemNames = (itemClass.Actions[0] as ItemActionRanged).MagazineItemNames;
					if (magazineItemNames != null)
					{
						ItemClass.GetItem(magazineItemNames[(int)this.SelectedAmmoTypeIndex], false).GetModifiedValueData(_modValueSources, EffectManager.ModifierValuesAndSources.ValueSourceType.Ammo, _entity, _originalItemValue, _passiveEffect, ref _originalValue, ref _perc_value, _tags);
					}
				}
				if (itemClass.Effects != null)
				{
					itemClass.Effects.GetModifiedValueData(_modValueSources, _sourceType, _entity, _passiveEffect, ref _originalValue, ref _perc_value, (float)this.Quality, _tags, 1);
				}
			}
			for (int i = 0; i < this.CosmeticMods.Length; i++)
			{
				if (this.CosmeticMods[i] != null && this.CosmeticMods[i].ItemClass is ItemClassModifier)
				{
					this.CosmeticMods[i].GetModifiedValueData(_modValueSources, EffectManager.ModifierValuesAndSources.ValueSourceType.CosmeticMod, _entity, _originalItemValue, _passiveEffect, ref _originalValue, ref _perc_value, _tags);
				}
			}
			for (int j = 0; j < this.Modifications.Length; j++)
			{
				if (this.Modifications[j] != null && this.Modifications[j].ItemClass is ItemClassModifier)
				{
					this.Modifications[j].GetModifiedValueData(_modValueSources, EffectManager.ModifierValuesAndSources.ValueSourceType.Mod, _entity, _originalItemValue, _passiveEffect, ref _originalValue, ref _perc_value, _tags);
				}
			}
		}
	}

	public void FireEvent(MinEventTypes _eventType, MinEventParams _eventParms)
	{
		ItemClass itemClass = this.ItemClass;
		if (itemClass != null)
		{
			if (itemClass is ItemClassModifier && itemClass.Effects != null)
			{
				itemClass.Effects.FireEvent(_eventType, _eventParms);
				return;
			}
			if (itemClass.Actions != null && itemClass.Actions.Length != 0 && itemClass.Actions[0] is ItemActionRanged)
			{
				string[] magazineItemNames = (itemClass.Actions[0] as ItemActionRanged).MagazineItemNames;
				if (magazineItemNames != null)
				{
					ItemClass.GetItem(magazineItemNames[(int)this.SelectedAmmoTypeIndex], false).FireEvent(_eventType, _eventParms);
				}
			}
			itemClass.FireEvent(_eventType, _eventParms);
		}
		if (!this.HasQuality)
		{
			return;
		}
		for (int i = 0; i < this.Modifications.Length; i++)
		{
			if (this.Modifications[i] != null)
			{
				this.Modifications[i].FireEvent(_eventType, _eventParms);
			}
		}
		for (int j = 0; j < this.CosmeticMods.Length; j++)
		{
			if (this.CosmeticMods[j] != null)
			{
				this.CosmeticMods[j].FireEvent(_eventType, _eventParms);
			}
		}
	}

	public ItemValue Clone()
	{
		ItemValue itemValue = new ItemValue(this.type, false);
		itemValue.Meta = this.Meta;
		itemValue.UseTimes = this.UseTimes;
		itemValue.Quality = this.Quality;
		itemValue.SelectedAmmoTypeIndex = this.SelectedAmmoTypeIndex;
		itemValue.Modifications = new ItemValue[this.Modifications.Length];
		for (int i = 0; i < this.Modifications.Length; i++)
		{
			itemValue.Modifications[i] = ((this.Modifications[i] != null) ? this.Modifications[i].Clone() : null);
		}
		if (this.Metadata != null)
		{
			itemValue.Metadata = new Dictionary<string, TypedMetadataValue>();
			foreach (KeyValuePair<string, TypedMetadataValue> keyValuePair in this.Metadata)
			{
				Dictionary<string, TypedMetadataValue> metadata = itemValue.Metadata;
				string key = keyValuePair.Key;
				TypedMetadataValue value = keyValuePair.Value;
				metadata.Add(key, (value != null) ? value.Clone() : null);
			}
		}
		itemValue.CosmeticMods = new ItemValue[this.CosmeticMods.Length];
		for (int j = 0; j < this.CosmeticMods.Length; j++)
		{
			itemValue.CosmeticMods[j] = ((this.CosmeticMods[j] != null) ? this.CosmeticMods[j].Clone() : null);
		}
		itemValue.Activated = this.Activated;
		if (itemValue.type == 0)
		{
			this.Seed = 0;
		}
		itemValue.Seed = this.Seed;
		return itemValue;
	}

	public bool IsEmpty()
	{
		return this.type == 0;
	}

	public BlockValue ToBlockValue()
	{
		if (this.type < Block.ItemsStartHere)
		{
			return new BlockValue
			{
				type = this.type
			};
		}
		return BlockValue.Air;
	}

	public void ReadOld(BinaryReader _br)
	{
	}

	public static ItemValue ReadOrNull(BinaryReader _br)
	{
		byte b = _br.ReadByte();
		if (b == 0)
		{
			return null;
		}
		ItemValue itemValue = new ItemValue();
		itemValue.ReadData(_br, (int)b);
		return itemValue;
	}

	public void Read(BinaryReader _br)
	{
		byte b = _br.ReadByte();
		if (b == 0)
		{
			this.type = 0;
			return;
		}
		this.ReadData(_br, (int)b);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ReadData(BinaryReader _br, int version)
	{
		int num = 0;
		if (version >= 8)
		{
			num = (int)_br.ReadByte();
		}
		this.type = (int)_br.ReadUInt16();
		if ((num & 1) > 0)
		{
			this.type += Block.ItemsStartHere;
		}
		if (version < 8 && this.type >= 32768)
		{
			this.type += 32768;
		}
		if (version > 5)
		{
			this.UseTimes = _br.ReadSingle();
		}
		else
		{
			this.UseTimes = (float)_br.ReadUInt16();
		}
		this.Quality = _br.ReadUInt16();
		this.Meta = (int)_br.ReadUInt16();
		if (this.Meta >= 65535)
		{
			this.Meta = -1;
		}
		if (version > 6)
		{
			int num2 = (int)_br.ReadByte();
			for (int i = 0; i < num2; i++)
			{
				string key = _br.ReadString();
				TypedMetadataValue tmv = TypedMetadataValue.Read(_br);
				this.SetMetadata(key, tmv);
			}
		}
		if ((version > 4 || this.HasQuality) && !(this.ItemClass is ItemClassModifier))
		{
			byte b = _br.ReadByte();
			this.Modifications = new ItemValue[(int)b];
			if (b != 0)
			{
				for (int j = 0; j < (int)b; j++)
				{
					if (_br.ReadBoolean())
					{
						this.Modifications[j] = new ItemValue();
						this.Modifications[j].Read(_br);
					}
					else
					{
						this.Modifications[j] = ItemValue.None.Clone();
					}
				}
			}
			b = _br.ReadByte();
			this.CosmeticMods = new ItemValue[(int)b];
			if (b != 0)
			{
				for (int k = 0; k < (int)b; k++)
				{
					if (_br.ReadBoolean())
					{
						this.CosmeticMods[k] = new ItemValue();
						this.CosmeticMods[k].Read(_br);
					}
					else
					{
						this.CosmeticMods[k] = ItemValue.None.Clone();
					}
				}
			}
		}
		if (version > 1)
		{
			this.Activated = _br.ReadByte();
		}
		if (version > 2)
		{
			this.SelectedAmmoTypeIndex = _br.ReadByte();
		}
		if (version > 3)
		{
			this.Seed = _br.ReadUInt16();
			if (this.type == 0)
			{
				this.Seed = 0;
			}
		}
	}

	public static void Write(ItemValue _iv, BinaryWriter _bw)
	{
		if (_iv == null)
		{
			_bw.Write(0);
			return;
		}
		_iv.Write(_bw);
	}

	public void Write(BinaryWriter _bw)
	{
		if (this.IsEmpty())
		{
			_bw.Write(0);
			return;
		}
		_bw.Write(8);
		int num = this.type;
		byte value = 0;
		if (this.type >= Block.ItemsStartHere)
		{
			value = 1;
			num -= Block.ItemsStartHere;
		}
		_bw.Write(value);
		_bw.Write((ushort)num);
		_bw.Write(this.UseTimes);
		_bw.Write(this.Quality);
		_bw.Write((ushort)this.Meta);
		int num2 = (this.Metadata != null) ? this.Metadata.Count : 0;
		_bw.Write((byte)num2);
		if (this.Metadata != null)
		{
			foreach (string text in this.Metadata.Keys)
			{
				TypedMetadataValue typedMetadataValue = this.Metadata[text];
				if (((typedMetadataValue != null) ? typedMetadataValue.GetValue() : null) != null)
				{
					_bw.Write(text);
					TypedMetadataValue.Write(this.Metadata[text], _bw);
				}
			}
		}
		if (!(this.ItemClass is ItemClassModifier))
		{
			_bw.Write((byte)this.Modifications.Length);
			for (int i = 0; i < this.Modifications.Length; i++)
			{
				bool flag = this.Modifications[i] != null && !this.Modifications[i].IsEmpty();
				_bw.Write(flag);
				if (flag)
				{
					this.Modifications[i].Write(_bw);
				}
			}
			_bw.Write((byte)this.CosmeticMods.Length);
			for (int j = 0; j < this.CosmeticMods.Length; j++)
			{
				bool flag2 = this.CosmeticMods[j] != null && !this.CosmeticMods[j].IsEmpty();
				_bw.Write(flag2);
				if (flag2)
				{
					this.CosmeticMods[j].Write(_bw);
				}
			}
		}
		_bw.Write(this.Activated);
		_bw.Write(this.SelectedAmmoTypeIndex);
		if (this.type == 0)
		{
			this.Seed = 0;
		}
		_bw.Write(this.Seed);
		ItemClass itemClass = ItemClass.list[this.type];
		if (itemClass == null)
		{
			if (this.type != 0)
			{
				Log.Error("No ItemClass entry for type " + this.type.ToString());
				return;
			}
		}
		else
		{
			NameIdMapping nameIdMapping;
			if (itemClass.IsBlock())
			{
				nameIdMapping = Block.nameIdMapping;
			}
			else
			{
				nameIdMapping = ItemClass.nameIdMapping;
			}
			if (nameIdMapping != null)
			{
				nameIdMapping.AddMapping(this.type, itemClass.Name, false);
			}
		}
	}

	public bool Equals(ItemValue _other)
	{
		return _other != null && (_other.type == this.type && _other.UseTimes == this.UseTimes && _other.Meta == this.Meta && _other.Seed == this.Seed && _other.Quality == this.Quality && _other.SelectedAmmoTypeIndex == this.SelectedAmmoTypeIndex && _other.Activated == this.Activated && ItemValue.Equals(_other.Metadata, this.Metadata) && ItemValue.Equals(_other.CosmeticMods, this.CosmeticMods)) && ItemValue.Equals(_other.Modifications, this.Modifications);
	}

	public override bool Equals(object _other)
	{
		return _other is ItemValue && this.Equals((ItemValue)_other);
	}

	public static bool Equals(ItemValue[] _a, ItemValue[] _b)
	{
		if (_a == null && _b == null)
		{
			return true;
		}
		if (_a == null || _b == null)
		{
			return false;
		}
		if (_a.Length != _b.Length)
		{
			return false;
		}
		if (_a.Length == 0)
		{
			return true;
		}
		for (int i = 0; i < _a.Length; i++)
		{
			if (_a[i] != null || _b[i] != null)
			{
				if (_a[i] == null || _b[i] == null)
				{
					return false;
				}
				if (!_a[i].Equals(_b[i]))
				{
					return false;
				}
			}
		}
		return true;
	}

	public static bool Equals(object[] _a, object[] _b)
	{
		return _a is ItemValue[] && _b is ItemValue[] && ItemValue.Equals((ItemValue[])_a, (ItemValue[])_b);
	}

	public static bool Equals(Dictionary<string, TypedMetadataValue> _a, Dictionary<string, TypedMetadataValue> _b)
	{
		if (_a == null && _b == null)
		{
			return true;
		}
		if (_a == null || _b == null)
		{
			return false;
		}
		if (_a.Count != _b.Count)
		{
			return false;
		}
		if (_a.Count == 0)
		{
			return true;
		}
		foreach (string key in _a.Keys)
		{
			if (!_b.ContainsKey(key))
			{
				return false;
			}
			if (!_a[key].Equals(_b[key]))
			{
				return false;
			}
		}
		return true;
	}

	public override int GetHashCode()
	{
		return this.type;
	}

	public int GetItemId()
	{
		return this.type - Block.ItemsStartHere;
	}

	public int GetItemOrBlockId()
	{
		if (this.type < Block.ItemsStartHere)
		{
			return this.type;
		}
		return this.type - Block.ItemsStartHere;
	}

	public override string ToString()
	{
		return string.Concat(new string[]
		{
			(this.type >= Block.ItemsStartHere) ? ("item=" + (this.type - Block.ItemsStartHere).ToString()) : ("block=" + this.type.ToString()),
			" m=",
			this.Meta.ToString(),
			" ut=",
			this.UseTimes.ToString()
		});
	}

	public string GetPropertyOverride(string _propertyName, string _originalValue)
	{
		if (this.Modifications.Length == 0 && this.CosmeticMods.Length == 0)
		{
			return _originalValue;
		}
		string result = "";
		string itemName = this.ItemClass.GetItemName();
		for (int i = 0; i < this.Modifications.Length; i++)
		{
			ItemValue itemValue = this.Modifications[i];
			if (itemValue != null)
			{
				ItemClassModifier itemClassModifier = itemValue.ItemClass as ItemClassModifier;
				if (itemClassModifier != null && itemClassModifier.GetPropertyOverride(_propertyName, itemName, ref result))
				{
					return result;
				}
			}
		}
		result = "";
		for (int j = 0; j < this.CosmeticMods.Length; j++)
		{
			ItemValue itemValue2 = this.CosmeticMods[j];
			if (itemValue2 != null)
			{
				ItemClassModifier itemClassModifier2 = itemValue2.ItemClass as ItemClassModifier;
				if (itemClassModifier2 != null && itemClassModifier2.GetPropertyOverride(_propertyName, itemName, ref result))
				{
					return result;
				}
			}
		}
		return _originalValue;
	}

	public bool HasMods()
	{
		for (int i = 0; i < this.Modifications.Length; i++)
		{
			ItemValue itemValue = this.Modifications[i];
			if (itemValue != null && !itemValue.IsEmpty())
			{
				return true;
			}
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const int EmptySaveVersion = 0;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int CurrentSaveVersion = 8;

	[PublicizedFrom(EAccessModifier.Private)]
	public const byte MaxModifications = 255;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ItemValue[] emptyItemValueArray = new ItemValue[0];

	public static ItemValue None = new ItemValue(0, false);

	[PublicizedFrom(EAccessModifier.Private)]
	public static FastTags<TagGroup.Global> noPreinstallCosmeticItemTags = FastTags<TagGroup.Global>.Parse("weapon,tool,armor");

	public int type;

	public byte Activated;

	public byte SelectedAmmoTypeIndex;

	public float UseTimes;

	public int Meta;

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<string, TypedMetadataValue> Metadata;

	public ushort Quality;

	public ItemValue[] Modifications;

	public ItemValue[] CosmeticMods;

	public ushort Seed;

	[PublicizedFrom(EAccessModifier.Private)]
	public static DateTime baseDate = new DateTime(2013, 10, 1);

	[PublicizedFrom(EAccessModifier.Private)]
	public const byte cFlagsItem = 1;
}
