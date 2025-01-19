using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using Audio;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;
using XMLData.Item;

[Preserve]
public class ItemClass : ItemData
{
	public string[] Attachments
	{
		get
		{
			return this.attachments;
		}
	}

	public bool HasQuality
	{
		get
		{
			return this.Effects != null && this.Effects.IsOwnerTiered();
		}
	}

	public virtual bool IsEquipment
	{
		get
		{
			return false;
		}
	}

	public RecipeUnlockData[] UnlockedBy
	{
		get
		{
			if (this.unlockedBy == null)
			{
				if (this.Properties.Values.ContainsKey(ItemClass.PropUnlockedBy))
				{
					string[] array = this.Properties.Values[ItemClass.PropUnlockedBy].Split(',', StringSplitOptions.None);
					if (array.Length != 0)
					{
						this.unlockedBy = new RecipeUnlockData[array.Length];
						for (int i = 0; i < array.Length; i++)
						{
							this.unlockedBy[i] = new RecipeUnlockData(array[i]);
						}
					}
				}
				else
				{
					this.unlockedBy = new RecipeUnlockData[0];
				}
			}
			return this.unlockedBy;
		}
	}

	public ItemClass()
	{
		this.bCanHold = true;
		this.bCanDrop = true;
		this.bCraftingTool = false;
		this.Properties = new DynamicProperties();
		this.attachments = null;
		this.Actions = new ItemAction[5];
		for (int i = 0; i < this.Actions.Length; i++)
		{
			this.Actions[i] = null;
		}
	}

	public void SetId(int _id)
	{
		this.pId = _id;
		if (this.Effects != null)
		{
			this.Effects.ParentPointer = _id;
		}
	}

	public virtual void Init()
	{
		string itemName = this.GetItemName();
		ItemClass.nameToItem[itemName] = this;
		ItemClass.nameToItemCaseInsensitive[itemName] = this;
		ItemClass.itemNames.Add(itemName);
		if (this.Properties.Values.ContainsKey(ItemClass.PropTags))
		{
			this.ItemTags = FastTags<TagGroup.Global>.Parse(this.Properties.Values[ItemClass.PropTags]);
		}
		if (this.Properties.Values.ContainsKey(ItemClass.PropDistractionTags))
		{
			this.DistractionTags = FastTags<TagGroup.Global>.Parse(this.Properties.Values[ItemClass.PropDistractionTags]);
		}
		if (this.Properties.Values.ContainsKey(ItemClass.PropFuelValue))
		{
			int startValue = 0;
			int.TryParse(this.Properties.Values[ItemClass.PropFuelValue], out startValue);
			base.FuelValue = new DataItem<int>(startValue);
		}
		if (this.Properties.Values.ContainsKey(ItemClass.PropWeight))
		{
			int startValue2;
			int.TryParse(this.Properties.Values[ItemClass.PropWeight], out startValue2);
			base.Weight = new DataItem<int>(startValue2);
		}
		if (this.Properties.Values.ContainsKey(ItemClass.PropImageEffectOnActive))
		{
			string startValue3 = this.Properties.Values[ItemClass.PropImageEffectOnActive].ToString();
			base.ImageEffectOnActive = new DataItem<string>(startValue3);
		}
		if (this.Properties.Values.ContainsKey(ItemClass.PropActive))
		{
			bool startValue4 = false;
			StringParsers.TryParseBool(this.Properties.Values[ItemClass.PropActive], out startValue4, 0, -1, true);
			base.Active = new DataItem<bool>(startValue4);
		}
		if (this.Properties.Values.ContainsKey(ItemClass.PropAlwaysActive))
		{
			bool startValue5 = false;
			StringParsers.TryParseBool(this.Properties.Values[ItemClass.PropAlwaysActive], out startValue5, 0, -1, true);
			base.AlwaysActive = new DataItem<bool>(startValue5);
		}
		if (this.Properties.Values.ContainsKey(ItemClass.PropPlaySoundOnActive))
		{
			string startValue6 = this.Properties.Values[ItemClass.PropPlaySoundOnActive].ToString();
			base.PlaySoundOnActive = new DataItem<string>(startValue6);
		}
		if (this.Properties.Values.ContainsKey(ItemClass.PropLightSource))
		{
			this.LightSource = new DataItem<string>(this.Properties.Values[ItemClass.PropLightSource]);
		}
		if (this.Properties.Values.ContainsKey(ItemClass.PropLightValue))
		{
			this.lightValue = StringParsers.ParseFloat(this.Properties.Values[ItemClass.PropLightValue], 0, -1, NumberStyles.Any);
		}
		if (this.Properties.Values.ContainsKey(ItemClass.PropSoundSightIn))
		{
			this.soundSightIn = this.Properties.Values[ItemClass.PropSoundSightIn].ToString();
		}
		if (this.Properties.Values.ContainsKey(ItemClass.PropSoundSightOut))
		{
			this.soundSightOut = this.Properties.Values[ItemClass.PropSoundSightOut].ToString();
		}
		this.Properties.ParseBool(ItemClass.PropIgnoreKeystoneSound, ref this.ignoreKeystoneSound);
		if (this.Properties.Values.ContainsKey(ItemClass.PropActivateObject))
		{
			this.ActivateObject = new DataItem<string>(this.Properties.Values[ItemClass.PropActivateObject]);
		}
		if (this.Properties.Values.ContainsKey(ItemClass.PropThrowableDecoy))
		{
			bool startValue7;
			StringParsers.TryParseBool(this.Properties.Values[ItemClass.PropThrowableDecoy], out startValue7, 0, -1, true);
			this.ThrowableDecoy = new DataItem<bool>(startValue7);
		}
		else
		{
			this.ThrowableDecoy = new DataItem<bool>(false);
		}
		if (this.Properties.Values.ContainsKey(ItemClass.PropCustomIcon))
		{
			this.CustomIcon = new DataItem<string>(this.Properties.Values[ItemClass.PropCustomIcon]);
		}
		if (this.Properties.Values.ContainsKey(ItemClass.PropCustomIconTint))
		{
			this.CustomIconTint = StringParsers.ParseHexColor(this.Properties.Values[ItemClass.PropCustomIconTint]);
		}
		else
		{
			this.CustomIconTint = Color.white;
		}
		if (this.Properties.Values.ContainsKey(ItemClass.PropGroupName))
		{
			string[] array = this.Properties.Values[ItemClass.PropGroupName].Split(',', StringSplitOptions.None);
			if (array.Length != 0)
			{
				this.Groups = new string[array.Length];
				for (int i = 0; i < array.Length; i++)
				{
					this.Groups[i] = array[i].Trim();
				}
			}
		}
		if (this.Properties.Values.ContainsKey(ItemClass.PropCritChance))
		{
			this.CritChance = new DataItem<float>(StringParsers.ParseFloat(this.Properties.Values[ItemClass.PropCritChance], 0, -1, NumberStyles.Any));
		}
		else
		{
			this.CritChance = new DataItem<float>(0f);
		}
		if (this.Properties.Values.ContainsKey(ItemClass.PropVehicleSlotType))
		{
			this.VehicleSlotType = this.Properties.Values[ItemClass.PropVehicleSlotType];
		}
		else
		{
			this.VehicleSlotType = string.Empty;
		}
		if (this.Properties.Values.ContainsKey(ItemClass.PropHoldingItemHidden))
		{
			this.HoldingItemHidden = StringParsers.ParseBool(this.Properties.Values[ItemClass.PropHoldingItemHidden], 0, -1, true);
		}
		else
		{
			this.HoldingItemHidden = false;
		}
		if (this.Properties.Values.ContainsKey(ItemClass.PropCraftExpValue))
		{
			StringParsers.TryParseFloat(this.Properties.Values[ItemClass.PropCraftExpValue], out this.CraftComponentExp, 0, -1, NumberStyles.Any);
		}
		if (this.Properties.Values.ContainsKey(ItemClass.PropRepairExpMultiplier))
		{
			StringParsers.TryParseFloat(this.Properties.Values[ItemClass.PropRepairExpMultiplier], out this.RepairExpMultiplier, 0, -1, NumberStyles.Any);
		}
		if (this.Properties.Values.ContainsKey(ItemClass.PropCraftTimeValue))
		{
			StringParsers.TryParseFloat(this.Properties.Values[ItemClass.PropCraftTimeValue], out this.CraftComponentTime, 0, -1, NumberStyles.Any);
		}
		if (this.Properties.Values.ContainsKey(ItemClass.PropLootExpValue))
		{
			StringParsers.TryParseFloat(this.Properties.Values[ItemClass.PropLootExpValue], out this.LootExp, 0, -1, NumberStyles.Any);
		}
		this.Properties.ParseFloat(ItemClass.PropEconomicValue, ref this.EconomicValue);
		this.Properties.ParseFloat(ItemClass.PropEconomicSellScale, ref this.EconomicSellScale);
		this.Properties.ParseInt(ItemClass.PropEconomicBundleSize, ref this.EconomicBundleSize);
		this.Properties.ParseBool(ItemClass.PropSellableToTrader, ref this.SellableToTrader);
		if (this.Properties.Values.ContainsKey(ItemClass.PropCreativeMode))
		{
			this.CreativeMode = EnumUtils.Parse<EnumCreativeMode>(this.Properties.Values[ItemClass.PropCreativeMode], false);
		}
		this.SortOrder = this.Properties.GetString(ItemClass.PropCreativeSort1);
		this.SortOrder += this.Properties.GetString(ItemClass.PropCreativeSort2);
		if (this.Properties.Values.ContainsKey(ItemClass.PropCraftingSkillExp) && !int.TryParse(this.Properties.Values[ItemClass.PropCraftingSkillExp], out this.CraftingSkillExp))
		{
			this.CraftingSkillExp = 10;
		}
		if (this.Properties.Values.ContainsKey(ItemClass.PropActionSkillExp) && !int.TryParse(this.Properties.Values[ItemClass.PropActionSkillExp], out this.ActionSkillExp))
		{
			this.ActionSkillExp = 10;
		}
		if (this.Properties.Values.ContainsKey(ItemClass.PropInsulation) && !StringParsers.TryParseFloat(this.Properties.Values[ItemClass.PropInsulation], out this.Insulation, 0, -1, NumberStyles.Any))
		{
			this.Insulation = 0f;
		}
		if (this.Properties.Values.ContainsKey(ItemClass.PropWaterproof) && !StringParsers.TryParseFloat(this.Properties.Values[ItemClass.PropWaterproof], out this.WaterProof, 0, -1, NumberStyles.Any))
		{
			this.WaterProof = 0f;
		}
		if (this.Properties.Values.ContainsKey(ItemClass.PropEncumbrance) && !StringParsers.TryParseFloat(this.Properties.Values[ItemClass.PropEncumbrance], out this.Encumbrance, 0, -1, NumberStyles.Any))
		{
			this.Encumbrance = 0f;
		}
		this.Properties.ParseString(ItemClass.PropSoundPickup, ref this.SoundPickup);
		this.Properties.ParseString(ItemClass.PropSoundPlace, ref this.SoundPlace);
		this.Properties.ParseString(ItemClass.PropSoundHolster, ref this.SoundHolster);
		this.Properties.ParseString(ItemClass.PropSoundUnholster, ref this.SoundUnholster);
		this.Properties.ParseString(ItemClass.PropSoundStick, ref this.SoundStick);
		this.Properties.ParseString(ItemClass.PropSoundTick, ref this.SoundTick);
		if (this.SoundTick != null)
		{
			string[] array2 = this.SoundTick.Split(',', StringSplitOptions.None);
			this.SoundTick = array2[0];
			if (array2.Length >= 2)
			{
				this.SoundTickDelay = StringParsers.ParseFloat(array2[1], 0, -1, NumberStyles.Any);
			}
		}
		if (this.Properties.Values.ContainsKey(ItemClass.PropDescriptionKey))
		{
			this.DescriptionKey = this.Properties.Values[ItemClass.PropDescriptionKey];
		}
		else
		{
			this.DescriptionKey = base.Name + "Desc";
		}
		if (this.Properties.Values.ContainsKey(ItemClass.PropResourceUnit))
		{
			this.IsResourceUnit = StringParsers.ParseBool(this.Properties.Values[ItemClass.PropResourceUnit], 0, -1, true);
		}
		if (this.Properties.Values.ContainsKey(ItemClass.PropMeltTimePerUnit))
		{
			this.MeltTimePerUnit = StringParsers.ParseFloat(this.Properties.Values[ItemClass.PropMeltTimePerUnit], 0, -1, NumberStyles.Any);
		}
		if (this.Properties.Values.ContainsKey(ItemClass.PropActionSkillGroup))
		{
			this.ActionSkillGroup = this.Properties.Values[ItemClass.PropActionSkillGroup];
		}
		else
		{
			this.ActionSkillGroup = "";
		}
		if (this.Properties.Values.ContainsKey(ItemClass.PropCraftingSkillGroup))
		{
			this.CraftingSkillGroup = this.Properties.Values[ItemClass.PropCraftingSkillGroup];
		}
		else
		{
			this.CraftingSkillGroup = "";
		}
		if (this.Properties.Values.ContainsKey(ItemClass.PropCrosshairOnAim))
		{
			this.bShowCrosshairOnAiming = StringParsers.ParseBool(this.Properties.Values[ItemClass.PropCrosshairOnAim], 0, -1, true);
		}
		if (this.Properties.Values.ContainsKey(ItemClass.PropCrosshairUpAfterShot))
		{
			this.bCrosshairUpAfterShot = StringParsers.ParseBool(this.Properties.Values[ItemClass.PropCrosshairUpAfterShot], 0, -1, true);
		}
		else
		{
			this.bCrosshairUpAfterShot = true;
		}
		if (this.Properties.Values.ContainsKey(ItemClass.PropUsableUnderwater))
		{
			this.UsableUnderwater = StringParsers.ParseBool(this.Properties.Values[ItemClass.PropUsableUnderwater], 0, -1, true);
		}
		if (this.Properties.Values.ContainsKey(ItemClass.PropItemTypeIcon))
		{
			this.ItemTypeIcon = this.Properties.Values[ItemClass.PropItemTypeIcon];
		}
		if (this.Properties.Values.ContainsKey(ItemClass.PropAltItemTypeIcon))
		{
			this.AltItemTypeIcon = this.Properties.Values[ItemClass.PropAltItemTypeIcon];
		}
		if (this.Properties.Values.ContainsKey(ItemClass.PropAltItemTypeIconColor))
		{
			this.AltItemTypeIconColor = StringParsers.ParseHexColor(this.Properties.Values[ItemClass.PropAltItemTypeIconColor]);
		}
		else
		{
			this.AltItemTypeIconColor = Color.white;
		}
		if (this.Properties.Values.ContainsKey(ItemClass.PropUnlocks))
		{
			this.Unlocks = this.Properties.Values[ItemClass.PropUnlocks];
		}
		if (this.Properties.Values.ContainsKey(ItemClass.PropNavObject))
		{
			this.NavObject = this.Properties.Values[ItemClass.PropNavObject];
		}
		if (this.Properties.Values.ContainsKey(ItemClass.PropQuestItem))
		{
			this.IsQuestItem = StringParsers.ParseBool(this.Properties.Values[ItemClass.PropQuestItem], 0, -1, true);
		}
		if (this.Properties.Values.ContainsKey(ItemClass.PropShowQuality))
		{
			this.ShowQualityBar = StringParsers.ParseBool(this.Properties.Values[ItemClass.PropShowQuality], 0, -1, true);
		}
		if (this.Properties.Values.ContainsKey(ItemClass.PropNoScrapping))
		{
			this.NoScrapping = StringParsers.ParseBool(this.Properties.Values[ItemClass.PropNoScrapping], 0, -1, true);
		}
		if (this.Properties.Values.ContainsKey(ItemClass.PropScrapTimeOverride))
		{
			this.ScrapTimeOverride = StringParsers.ParseFloat(this.Properties.Values[ItemClass.PropScrapTimeOverride], 0, -1, NumberStyles.Any);
		}
		if (this.Properties.Classes.ContainsKey("SDCS"))
		{
			this.SDCSData = new SDCSUtils.SlotData();
			if (this.Properties.Values.ContainsKey("SDCS.Prefab"))
			{
				this.SDCSData.PrefabName = this.Properties.Values["SDCS.Prefab"];
				if (this.SDCSData.PrefabName.Contains("*"))
				{
					DataLoader.PreloadBundle(this.SDCSData.PrefabName.Replace("*", "female"));
					DataLoader.PreloadBundle(this.SDCSData.PrefabName.Replace("*", "male"));
				}
				else
				{
					DataLoader.PreloadBundle(this.SDCSData.PrefabName);
				}
			}
			if (this.Properties.Values.ContainsKey("SDCS.TransformName"))
			{
				this.SDCSData.PartName = this.Properties.Values["SDCS.TransformName"];
			}
			if (this.Properties.Values.ContainsKey("SDCS.Excludes"))
			{
				this.SDCSData.BaseToTurnOff = this.Properties.Values["SDCS.Excludes"];
			}
			if (this.Properties.Values.ContainsKey("SDCS.CullDistFPV"))
			{
				this.SDCSData.CullDistance = StringParsers.ParseFloat(this.Properties.Values["SDCS.CullDistFPV"], 0, -1, NumberStyles.Any);
			}
			if (this.Properties.Values.ContainsKey("SDCS.HairMaskType"))
			{
				this.SDCSData.HairMaskType = (SDCSUtils.SlotData.HairMaskTypes)Enum.Parse(typeof(SDCSUtils.SlotData.HairMaskTypes), this.Properties.Values["SDCS.HairMaskType"]);
			}
			if (this.Properties.Values.ContainsKey("SDCS.FacialHairMaskType"))
			{
				this.SDCSData.FacialHairMaskType = (SDCSUtils.SlotData.HairMaskTypes)Enum.Parse(typeof(SDCSUtils.SlotData.HairMaskTypes), this.Properties.Values["SDCS.FacialHairMaskType"]);
			}
			if (this.Properties.Values.ContainsKey("SDCS.HeadGearName"))
			{
				this.SDCSData.HeadGearName = this.Properties.Values["SDCS.HeadGearName"];
			}
		}
		if (this.Properties.Values.ContainsKey(ItemClass.PropDisplayType))
		{
			this.DisplayType = this.Properties.Values[ItemClass.PropDisplayType];
		}
		else
		{
			this.DisplayType = "";
		}
		this.Properties.ParseString(ItemClass.PropTraderStageTemplate, ref this.TraderStageTemplate);
		this.Properties.ParseString(ItemClass.PropTrackerIndexName, ref this.TrackerIndexName);
		this.Properties.ParseString(ItemClass.PropTrackerNavObject, ref this.TrackerNavObject);
	}

	public void LateInit()
	{
		if (this.Properties.Values.ContainsKey(ItemClass.PropSmell))
		{
			AIDirectorData.FindSmell(this.Properties.Values[ItemClass.PropSmell], out this.Smell);
		}
		if (this.HasQuality)
		{
			this.Stacknumber.Value = 1;
		}
	}

	public static void InitStatic()
	{
		ItemClass.list = new ItemClass[ItemClass.MAX_ITEMS];
		ItemClass.itemActionNames = new string[5];
		for (int i = 0; i < 5; i++)
		{
			ItemClass.itemActionNames[i] = "Action" + i.ToString();
		}
	}

	public static void LateInitAll()
	{
		for (int i = 0; i < ItemClass.MAX_ITEMS; i++)
		{
			if (ItemClass.list[i] != null)
			{
				ItemClass.list[i].LateInit();
			}
		}
	}

	public static void Cleanup()
	{
		ItemClass.list = null;
		ItemClass.nameToItem.Clear();
		ItemClass.nameToItemCaseInsensitive.Clear();
		ItemClass.itemNames.Clear();
		ItemClass.itemActionNames = null;
	}

	public virtual int GetInitialMetadata(ItemValue _itemValue)
	{
		if (this.Actions[0] == null)
		{
			return 0;
		}
		return this.Actions[0].GetInitialMeta(_itemValue);
	}

	public static ItemClass GetForId(int _id)
	{
		if (ItemClass.list == null || (ulong)_id >= (ulong)((long)ItemClass.list.Length))
		{
			return null;
		}
		return ItemClass.list[_id];
	}

	public static ItemValue GetItem(string _itemName, bool _caseInsensitive = false)
	{
		ItemClass itemClass = ItemClass.GetItemClass(_itemName, _caseInsensitive);
		if (itemClass != null)
		{
			return new ItemValue(itemClass.Id, false);
		}
		return ItemValue.None.Clone();
	}

	public static ItemClass GetItemClass(string _itemName, bool _caseInsensitive = false)
	{
		ItemClass result;
		if (_caseInsensitive)
		{
			ItemClass.nameToItemCaseInsensitive.TryGetValue(_itemName, out result);
		}
		else
		{
			ItemClass.nameToItem.TryGetValue(_itemName, out result);
		}
		return result;
	}

	public static void GetItemsAndBlocks(List<ItemClass> _targetList, int _idStart = -1, int _idEndExcl = -1, ItemClass.FilterItem[] _filterExprs = null, string _nameFilter = null, bool _bShowUserHidden = false, EnumCreativeMode _currentCreativeMode = EnumCreativeMode.Player, bool _showFavorites = false, bool _sortBySortOrder = false, XUi _xui = null)
	{
		_targetList.Clear();
		if (_idStart < 0)
		{
			_idStart = 0;
		}
		if (_idEndExcl < 0)
		{
			_idEndExcl = ItemClass.list.Length;
		}
		if (string.IsNullOrEmpty(_nameFilter))
		{
			_nameFilter = null;
		}
		int num = -1;
		if (_nameFilter != null)
		{
			int.TryParse(_nameFilter, out num);
		}
		int i = _idStart;
		while (i < _idEndExcl)
		{
			Block block = null;
			if (i >= Block.ItemsStartHere)
			{
				goto IL_55;
			}
			block = Block.list[i];
			if (block != null)
			{
				goto IL_55;
			}
			IL_14C:
			i++;
			continue;
			IL_55:
			ItemClass forId = ItemClass.GetForId(i);
			if (forId == null)
			{
				goto IL_14C;
			}
			EnumCreativeMode creativeMode = forId.CreativeMode;
			if (creativeMode != EnumCreativeMode.None && creativeMode != EnumCreativeMode.Test && (creativeMode == EnumCreativeMode.All || _currentCreativeMode == creativeMode || _bShowUserHidden) && (creativeMode != EnumCreativeMode.Console || (DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5).IsCurrent()) && (!_showFavorites || !(_xui != null) || _xui.playerUI.entityPlayer.favoriteCreativeStacks.Contains((ushort)i)))
			{
				if (_filterExprs != null)
				{
					bool flag = false;
					for (int j = 0; j < _filterExprs.Length; j++)
					{
						if (_filterExprs[j] != null)
						{
							flag = _filterExprs[j](forId, block);
							if (flag)
							{
								flag = true;
								break;
							}
						}
					}
					if (flag)
					{
						goto IL_14C;
					}
				}
				if (_nameFilter != null)
				{
					string a = forId.GetLocalizedItemName() ?? Localization.Get(forId.Name, false);
					if ((num < 0 || forId.Id != num) && !forId.Name.ContainsCaseInsensitive(_nameFilter) && !a.ContainsCaseInsensitive(_nameFilter))
					{
						goto IL_14C;
					}
				}
				_targetList.Add(forId);
				goto IL_14C;
			}
			goto IL_14C;
		}
		if (_sortBySortOrder)
		{
			_targetList.Sort(delegate(ItemClass _icA, ItemClass _icB)
			{
				int num2 = string.CompareOrdinal(_icA.SortOrder, _icB.SortOrder);
				if (num2 != 0)
				{
					return num2;
				}
				return _icA.Id.CompareTo(_icB.Id);
			});
		}
	}

	public static void CreateItemStacks(IEnumerable<ItemClass> _itemClassList, List<ItemStack> _targetList)
	{
		_targetList.Clear();
		foreach (ItemClass itemClass in _itemClassList)
		{
			ItemValue itemValue = new ItemValue(itemClass.Id, true);
			itemValue.Meta = itemValue.ItemClass.GetInitialMetadata(itemValue);
			ItemStack item = new ItemStack(itemValue, itemClass.Stacknumber.Value);
			_targetList.Add(item);
		}
	}

	public virtual bool IsHUDDisabled(ItemInventoryData _data)
	{
		return (this.Actions[0] != null && this.Actions[0].IsHUDDisabled((_data != null) ? _data.actionData[0] : null)) || (this.Actions[1] != null && this.Actions[1].IsHUDDisabled((_data != null) ? _data.actionData[1] : null));
	}

	public virtual bool IsLightSource()
	{
		return this.LightSource != null;
	}

	public virtual Transform CloneModel(GameObject _reuseThisGO, World _world, BlockValue _blockValue, Vector3[] _vertices, Vector3 _position, Transform _parent, BlockShape.MeshPurpose _purpose = BlockShape.MeshPurpose.World, long _textureFull = 0L)
	{
		return this.CloneModel(_world, _blockValue.ToItemValue(), _position, _parent, _purpose, _textureFull);
	}

	public virtual Transform CloneModel(World _world, ItemValue _itemValue, Vector3 _position, Transform _parent, BlockShape.MeshPurpose _purpose = BlockShape.MeshPurpose.World, long _textureFull = 0L)
	{
		GameObject gameObject = null;
		if (this.CanHold())
		{
			string text = null;
			if (_purpose == BlockShape.MeshPurpose.Drop)
			{
				text = _itemValue.GetPropertyOverride("DropMeshFile", this.DropMeshFile);
			}
			if (_purpose == BlockShape.MeshPurpose.Hold)
			{
				text = _itemValue.GetPropertyOverride("HandMeshfile", this.HandMeshFile);
			}
			if (text == null)
			{
				text = _itemValue.GetPropertyOverride("Meshfile", this.MeshFile);
			}
			string text2 = (text != null) ? GameIO.GetFilenameFromPathWithoutExtension(text) : null;
			if (this.renderGameObject == null || (text != null && !text2.Equals(this.renderGameObject.name)))
			{
				this.renderGameObject = DataLoader.LoadAsset<GameObject>(text);
			}
			gameObject = this.renderGameObject;
			if (gameObject == null)
			{
				gameObject = LoadManager.LoadAsset<GameObject>("@:Other/Items/Crafting/leather.fbx", null, null, false, true).Asset;
			}
		}
		if (gameObject == null)
		{
			return null;
		}
		try
		{
			GameObject gameObject2 = UnityEngine.Object.Instantiate<GameObject>(gameObject);
			Transform transform = gameObject2.transform;
			transform.SetParent(_parent, false);
			transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
			gameObject2.SetActive(false);
			if (_purpose == BlockShape.MeshPurpose.Hold)
			{
				Collider[] componentsInChildren = gameObject2.GetComponentsInChildren<Collider>();
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					componentsInChildren[i].enabled = false;
				}
			}
			UpdateLightOnAllMaterials updateLightOnAllMaterials = gameObject2.GetComponent<UpdateLightOnAllMaterials>();
			if (updateLightOnAllMaterials == null)
			{
				updateLightOnAllMaterials = gameObject2.AddComponent<UpdateLightOnAllMaterials>();
			}
			string originalValue = "255,255,255";
			this.Properties.ParseString(Block.PropTintColor, ref originalValue);
			Vector3 tintColorForItem = Block.StringToVector3(_itemValue.GetPropertyOverride(Block.PropTintColor, originalValue));
			updateLightOnAllMaterials.SetTintColorForItem(tintColorForItem);
			return transform;
		}
		catch (Exception ex)
		{
			Log.Error("Instantiate of '" + this.MeshFile + "' led to error: " + ex.Message);
			Log.Error(ex.StackTrace);
		}
		return null;
	}

	public void setLocalizedItemName(string _localizedName)
	{
		this.localizedName = _localizedName;
	}

	public virtual string GetLocalizedItemName()
	{
		return this.localizedName;
	}

	public void SetName(string _name)
	{
		this.pName = _name;
	}

	public virtual string GetItemName()
	{
		return base.Name;
	}

	public virtual string GetItemDescriptionKey()
	{
		return this.DescriptionKey;
	}

	public virtual string GetIconName()
	{
		if (this.CustomIcon != null && this.CustomIcon.Value.Length > 0)
		{
			return this.CustomIcon.Value;
		}
		return base.Name;
	}

	public virtual Color GetIconTint(ItemValue _instance = null)
	{
		if (_instance != null)
		{
			string text = "NONE";
			string propertyOverride = _instance.GetPropertyOverride("CustomIconTint", text);
			if (!propertyOverride.Equals(text))
			{
				return StringParsers.ParseHexColor(propertyOverride);
			}
		}
		return this.CustomIconTint;
	}

	public virtual bool IsGun()
	{
		return this.Actions[0] is ItemActionAttack;
	}

	public virtual bool IsDynamicMelee()
	{
		return this.Actions[0] is ItemActionDynamic;
	}

	public virtual bool CanStack()
	{
		return this.Stacknumber.Value > 1;
	}

	public void SetCanHold(bool _b)
	{
		this.bCanHold = _b;
	}

	public virtual bool CanHold()
	{
		return this.bCanHold;
	}

	public void SetCanDrop(bool _b)
	{
		this.bCanDrop = _b;
	}

	public virtual bool CanDrop(ItemValue _iv = null)
	{
		return this.bCanDrop;
	}

	public virtual void Deactivate(ItemValue _iv)
	{
	}

	public virtual bool KeepOnDeath()
	{
		return false;
	}

	public virtual bool CanPlaceInContainer()
	{
		return true;
	}

	public virtual string CanInteract(ItemInventoryData _data)
	{
		ItemAction itemAction = this.Actions[2];
		if (itemAction == null)
		{
			return null;
		}
		return itemAction.CanInteract(_data.actionData[2]);
	}

	public void Interact(ItemInventoryData _data)
	{
		this.ExecuteAction(2, _data, false, null);
		this.ExecuteAction(2, _data, true, null);
	}

	public bool CanExecuteAction(int actionIdx, EntityAlive holdingEntity, ItemValue itemValue)
	{
		bool flag = true;
		ItemAction itemAction = this.Actions[actionIdx];
		if (itemAction != null)
		{
			List<IRequirement> executionRequirements = itemAction.ExecutionRequirements;
			if (executionRequirements != null)
			{
				holdingEntity.MinEventContext.ItemValue = itemValue;
				for (int i = 0; i < executionRequirements.Count; i++)
				{
					flag &= executionRequirements[i].IsValid(holdingEntity.MinEventContext);
					if (!flag)
					{
						break;
					}
				}
			}
		}
		return flag;
	}

	public virtual void ExecuteAction(int _actionIdx, ItemInventoryData _data, bool _bReleased, PlayerActionsLocal _playerActions)
	{
		ItemAction curAction = this.Actions[_actionIdx];
		if (curAction == null)
		{
			return;
		}
		if (curAction is ItemActionDynamicMelee)
		{
			bool flag = _bReleased;
			if (this.Actions.Length >= 2 && this.Actions[0] != null && this.Actions[1] != null)
			{
				flag = (!this.Actions[1].IsActionRunning(_data.actionData[1]) && !this.Actions[0].IsActionRunning(_data.actionData[0]));
			}
			if (!flag)
			{
				return;
			}
			List<IRequirement> executionRequirements = curAction.ExecutionRequirements;
			if (executionRequirements != null && !_bReleased)
			{
				_data.holdingEntity.MinEventContext.ItemValue = _data.itemValue;
				for (int i = 0; i < executionRequirements.Count; i++)
				{
					flag &= executionRequirements[i].IsValid(_data.holdingEntity.MinEventContext);
					if (!flag)
					{
						break;
					}
				}
			}
			if (!flag)
			{
				return;
			}
			if (_data != null && _data.holdingEntity.emodel != null && _data.holdingEntity.emodel.avatarController != null)
			{
				_data.holdingEntity.emodel.avatarController.UpdateInt(AvatarController.itemActionIndexHash, _actionIdx, true);
			}
			curAction.ExecuteAction((_data != null) ? _data.actionData[_actionIdx] : null, _bReleased);
			return;
		}
		else
		{
			global::ItemActionData actionData = _data.actionData[_actionIdx];
			bool flag2 = _bReleased || !curAction.IsActionRunning(actionData);
			if (!flag2)
			{
				return;
			}
			List<IRequirement> executionRequirements2 = curAction.ExecutionRequirements;
			if (executionRequirements2 != null && !_bReleased)
			{
				_data.holdingEntity.MinEventContext.ItemValue = _data.itemValue;
				for (int j = 0; j < executionRequirements2.Count; j++)
				{
					flag2 &= executionRequirements2[j].IsValid(_data.holdingEntity.MinEventContext);
					if (!flag2)
					{
						break;
					}
				}
			}
			if (!flag2)
			{
				GameManager.ShowTooltip(_data.holdingEntity as EntityPlayerLocal, Localization.Get("ttCannotUseAtThisTime", false), string.Empty, "ui_denied", null, false);
				return;
			}
			if (_data != null && _data.holdingEntity.emodel != null && _data.holdingEntity.emodel.avatarController != null)
			{
				_data.holdingEntity.emodel.avatarController.UpdateInt(AvatarController.itemActionIndexHash, _actionIdx, true);
			}
			if (!_bReleased)
			{
				if (!actionData.HasExecuted)
				{
					_data.holdingEntity.MinEventContext.ItemValue = _data.itemValue;
					_data.holdingEntity.FireEvent(MinEvent.Start[_actionIdx], true);
				}
				if (curAction is ItemActionRanged && !(curAction is ItemActionLauncher) && !(curAction is ItemActionCatapult))
				{
					actionData.HasExecuted = true;
					curAction.ExecuteAction(actionData, _bReleased);
					return;
				}
				if (!actionData.HasExecuted)
				{
					ItemActionEat itemActionEat = curAction as ItemActionEat;
					if (itemActionEat != null)
					{
						LocalPlayerUI uiforPlayer = LocalPlayerUI.GetUIForPlayer(_data.holdingEntity as EntityPlayerLocal);
						XUi xui = (uiforPlayer != null) ? uiforPlayer.xui : null;
						if (itemActionEat.UsePrompt && !xui.isUsingItemActionEntryPromptComplete)
						{
							XUiC_MessageBoxWindowGroup.ShowMessageBox(xui, Localization.Get(itemActionEat.PromptTitle, false), Localization.Get(itemActionEat.PromptDescription, false), XUiC_MessageBoxWindowGroup.MessageBoxTypes.OkCancel, delegate()
							{
								_data.holdingEntity.MinEventContext.ItemValue = _data.holdingEntity.inventory.holdingItemItemValue;
								actionData.HasExecuted = false;
								curAction.ExecuteAction(actionData, true);
								xui.isUsingItemActionEntryPromptComplete = false;
							}, null, true, true);
							return;
						}
						xui.isUsingItemActionEntryPromptComplete = false;
					}
					_data.holdingEntity.MinEventContext.ItemValue = _data.holdingEntity.inventory.holdingItemItemValue;
					actionData.HasExecuted = true;
					curAction.ExecuteAction(actionData, _bReleased);
					return;
				}
			}
			else if (actionData.HasExecuted)
			{
				if (!(curAction is ItemActionUseOther))
				{
					ItemValue itemValue = _data.itemValue;
					if (!curAction.IsEndDelayed() || !curAction.UseAnimation)
					{
						_data.holdingEntity.MinEventContext.ItemValue = itemValue;
						_data.holdingEntity.FireEvent(MinEvent.End[_actionIdx], true);
					}
					if (_data.holdingEntity as EntityPlayerLocal != null)
					{
						ItemClass itemClass = itemValue.ItemClass;
						if (itemClass != null && itemClass.HasAnyTags(this.stopBleed) && _data.holdingEntity.Buffs.HasBuff("buffInjuryBleeding"))
						{
							IAchievementManager achievementManager = PlatformManager.NativePlatform.AchievementManager;
							if (achievementManager != null)
							{
								achievementManager.SetAchievementStat(EnumAchievementDataStat.BleedOutStopped, 1);
							}
						}
					}
				}
				if (curAction is ItemActionActivate && _bReleased)
				{
					executionRequirements2 = curAction.ExecutionRequirements;
					if (executionRequirements2 != null)
					{
						_data.holdingEntity.MinEventContext.ItemValue = _data.itemValue;
						for (int k = 0; k < executionRequirements2.Count; k++)
						{
							flag2 &= executionRequirements2[k].IsValid(_data.holdingEntity.MinEventContext);
							if (!flag2)
							{
								GameManager.ShowTooltip(_data.holdingEntity as EntityPlayerLocal, Localization.Get("ttCannotUseAtThisTime", false), string.Empty, "ui_denied", null, false);
								actionData.HasExecuted = true;
								return;
							}
						}
					}
				}
				_data.holdingEntity.MinEventContext.ItemValue = _data.holdingEntity.inventory.holdingItemItemValue;
				actionData.HasExecuted = false;
				curAction.ExecuteAction(actionData, _bReleased);
			}
			return;
		}
	}

	public virtual bool IsActionRunning(ItemInventoryData _data)
	{
		for (int i = 0; i < 3; i++)
		{
			ItemAction itemAction = this.Actions[i];
			if (itemAction != null && itemAction.IsActionRunning(_data.actionData[i]))
			{
				return true;
			}
		}
		return false;
	}

	public virtual void OnHoldingItemActivated(ItemInventoryData _data)
	{
	}

	public virtual void StartHolding(ItemInventoryData _data, Transform _modelTransform)
	{
		for (int i = 0; i < 3; i++)
		{
			ItemAction itemAction = this.Actions[i];
			if (itemAction != null)
			{
				itemAction.StartHolding(_data.actionData[i]);
			}
		}
		if (this.Actions[0] != null || this.Actions[1] != null)
		{
			_data.holdingEntitySoundID = -1;
		}
	}

	public virtual void CleanupHoldingActions(ItemInventoryData _data)
	{
		if (_data == null)
		{
			return;
		}
		for (int i = 0; i < 3; i++)
		{
			ItemAction itemAction = this.Actions[i];
			if (itemAction != null)
			{
				itemAction.Cleanup(_data.actionData[i]);
			}
		}
	}

	public virtual void OnHoldingUpdate(ItemInventoryData _data)
	{
		EntityAlive holdingEntity = _data.holdingEntity;
		for (int i = 0; i < 3; i++)
		{
			ItemAction itemAction = this.Actions[i];
			if (itemAction != null)
			{
				holdingEntity.MinEventContext.ItemValue = holdingEntity.inventory.holdingItemItemValue;
				holdingEntity.MinEventContext.ItemActionData = _data.actionData[i];
				holdingEntity.FireEvent(MinEvent.Update[i], true);
				itemAction.OnHoldingUpdate((_data != null) ? _data.actionData[i] : null);
			}
		}
		if (this.Properties.Values.ContainsKey(ItemClass.PropSoundIdle) && !_data.holdingEntity.isEntityRemote)
		{
			if (_data.holdingEntitySoundID == 0 && _data.itemValue.Meta == 0)
			{
				Manager.BroadcastStop(_data.holdingEntity.entityId, this.Properties.Values[ItemClass.PropSoundIdle]);
				return;
			}
			if (_data.holdingEntitySoundID == -1 && _data.itemValue.Meta > 0)
			{
				Manager.BroadcastPlay(_data.holdingEntity, this.Properties.Values[ItemClass.PropSoundIdle], false);
				_data.holdingEntitySoundID = 0;
			}
		}
	}

	public virtual void OnHoldingReset(ItemInventoryData _data)
	{
	}

	public void StopHoldingAudio(ItemInventoryData _data)
	{
		if (this.Properties.Values[ItemClass.PropSoundIdle] != null && _data.holdingEntitySoundID == 0)
		{
			Manager.BroadcastStop(_data.holdingEntity.entityId, this.Properties.Values[ItemClass.PropSoundIdle]);
		}
		_data.holdingEntitySoundID = -2;
	}

	public virtual void StopHolding(ItemInventoryData _data, Transform _modelTransform)
	{
		this.StopHoldingAudio(_data);
		if (_data.holdingEntity is EntityPlayer && !_data.holdingEntity.isEntityRemote && _data.holdingEntity.AimingGun)
		{
			_data.holdingEntity.AimingGun = false;
		}
		for (int i = 0; i < 3; i++)
		{
			ItemAction itemAction = this.Actions[i];
			if (itemAction != null)
			{
				itemAction.StopHolding(_data.actionData[i]);
			}
		}
	}

	public virtual void OnMeshCreated(ItemWorldData _data)
	{
	}

	public virtual void OnDroppedUpdate(ItemWorldData _data)
	{
	}

	public virtual BlockValue OnConvertToBlockValue(ItemValue _itemValue, BlockValue _blueprintBlockValue)
	{
		return _blueprintBlockValue;
	}

	public ItemInventoryData CreateInventoryData(ItemStack _itemStack, IGameManager _gameManager, EntityAlive _holdingEntity, int _slotIdxInInventory)
	{
		ItemInventoryData itemInventoryData = this.createItemInventoryData(_itemStack, _gameManager, _holdingEntity, _slotIdxInInventory);
		itemInventoryData.actionData[0] = ((this.Actions[0] != null) ? this.Actions[0].CreateModifierData(itemInventoryData, 0) : null);
		itemInventoryData.actionData[1] = ((this.Actions[1] != null) ? this.Actions[1].CreateModifierData(itemInventoryData, 1) : null);
		if (this.Actions[2] != null)
		{
			itemInventoryData.actionData.Add(this.Actions[2].CreateModifierData(itemInventoryData, 2));
		}
		return itemInventoryData;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual ItemInventoryData createItemInventoryData(ItemStack _itemStack, IGameManager _gameManager, EntityAlive _holdingEntity, int _slotIdxInInventory)
	{
		return new ItemInventoryData(this, _itemStack, _gameManager, _holdingEntity, _slotIdxInInventory);
	}

	public virtual ItemWorldData CreateWorldData(IGameManager _gm, EntityItem _entityItem, ItemValue _itemValue, int _belongsEntityId)
	{
		return new ItemWorldData(_gm, _itemValue, _entityItem, _belongsEntityId);
	}

	public virtual void OnHUD(ItemInventoryData _data, int _x, int _y)
	{
		if (this.Actions[0] != null)
		{
			this.Actions[0].OnHUD(_data.actionData[0], _x, _y);
		}
		if (this.Actions[1] != null)
		{
			this.Actions[1].OnHUD(_data.actionData[1], _x, _y);
		}
	}

	public virtual void OnScreenOverlay(ItemInventoryData _data)
	{
		if (this.Actions[0] != null)
		{
			this.Actions[0].OnScreenOverlay(_data.actionData[0]);
		}
		if (this.Actions[1] != null)
		{
			this.Actions[1].OnScreenOverlay(_data.actionData[1]);
		}
	}

	public virtual RenderCubeType GetFocusType(ItemInventoryData _data)
	{
		if (!this.CanHold())
		{
			return RenderCubeType.None;
		}
		RenderCubeType renderCubeType = RenderCubeType.None;
		if (this.Actions[0] != null)
		{
			renderCubeType = this.Actions[0].GetFocusType((_data != null) ? _data.actionData[0] : null);
		}
		RenderCubeType renderCubeType2 = RenderCubeType.None;
		if (this.Actions[1] != null)
		{
			renderCubeType2 = this.Actions[1].GetFocusType((_data != null) ? _data.actionData[1] : null);
		}
		if (renderCubeType <= renderCubeType2)
		{
			return renderCubeType2;
		}
		return renderCubeType;
	}

	public virtual float GetFocusRange()
	{
		if (this.Actions[0] != null && this.Actions[0] is ItemActionAttack)
		{
			return ((ItemActionAttack)this.Actions[0]).Range;
		}
		return 0f;
	}

	public virtual bool IsFocusBlockInside()
	{
		bool flag = false;
		if (this.Actions[0] != null)
		{
			flag = this.Actions[0].IsFocusBlockInside();
		}
		bool flag2 = false;
		if (this.Actions[1] != null)
		{
			flag2 = this.Actions[1].IsFocusBlockInside();
		}
		return flag2 && flag;
	}

	public virtual bool ConsumeScrollWheel(ItemInventoryData _data, float _scrollWheelInput, PlayerActionsLocal _playerInput)
	{
		bool flag = false;
		if (this.Actions[0] != null)
		{
			flag = this.Actions[0].ConsumeScrollWheel(_data.actionData[0], _scrollWheelInput, _playerInput);
		}
		if (!flag && this.Actions[1] != null)
		{
			flag = this.Actions[1].ConsumeScrollWheel(_data.actionData[1], _scrollWheelInput, _playerInput);
		}
		return flag;
	}

	public virtual void CheckKeys(ItemInventoryData _data, WorldRayHitInfo _hitInfo)
	{
	}

	public virtual float GetLifetimeOnDrop()
	{
		return 60f;
	}

	public virtual Block GetBlock()
	{
		return null;
	}

	public virtual bool IsBlock()
	{
		return false;
	}

	public virtual ItemClass.EnumCrosshairType GetCrosshairType(ItemInventoryData _holdingData)
	{
		ItemClass.EnumCrosshairType enumCrosshairType = ItemClass.EnumCrosshairType.Plus;
		ItemClass.EnumCrosshairType enumCrosshairType2 = ItemClass.EnumCrosshairType.Plus;
		if (this.Actions[0] != null)
		{
			enumCrosshairType = this.Actions[0].GetCrosshairType(_holdingData.actionData[0]);
		}
		if (this.Actions[1] != null)
		{
			enumCrosshairType2 = this.Actions[1].GetCrosshairType(_holdingData.actionData[1]);
		}
		ItemClass.EnumCrosshairType enumCrosshairType3 = (enumCrosshairType > enumCrosshairType2) ? enumCrosshairType : enumCrosshairType2;
		string propertyOverride = _holdingData.itemValue.GetPropertyOverride(ItemClass.PropCrosshairOnAim, string.Empty);
		if (propertyOverride.Length > 0)
		{
			this.bShowCrosshairOnAiming = StringParsers.ParseBool(propertyOverride, 0, -1, true);
		}
		if (enumCrosshairType3 == ItemClass.EnumCrosshairType.Crosshair && this.bShowCrosshairOnAiming)
		{
			enumCrosshairType3 = ItemClass.EnumCrosshairType.CrosshairOnAiming;
		}
		return enumCrosshairType3;
	}

	public virtual void GetIronSights(ItemInventoryData _invData, out float _fov)
	{
		if (this.Actions[0] != null)
		{
			this.Actions[0].GetIronSights(_invData.actionData[0], out _fov);
			if (_fov != 0f)
			{
				return;
			}
		}
		if (this.Actions[1] != null)
		{
			this.Actions[1].GetIronSights(_invData.actionData[1], out _fov);
			if (_fov != 0f)
			{
				return;
			}
		}
		_fov = (float)GamePrefs.GetInt(EnumGamePrefs.OptionsGfxFOV);
	}

	public virtual EnumCameraShake GetCameraShakeType(ItemInventoryData _invData)
	{
		EnumCameraShake enumCameraShake = EnumCameraShake.None;
		EnumCameraShake enumCameraShake2 = EnumCameraShake.None;
		if (this.Actions[0] != null)
		{
			enumCameraShake = this.Actions[0].GetCameraShakeType(_invData.actionData[0]);
		}
		if (this.Actions[1] != null)
		{
			enumCameraShake2 = this.Actions[1].GetCameraShakeType(_invData.actionData[1]);
		}
		if (enumCameraShake > enumCameraShake2)
		{
			return enumCameraShake;
		}
		return enumCameraShake2;
	}

	public virtual TriggerEffectManager.ControllerTriggerEffect GetControllerTriggerEffectPull()
	{
		if (this.Actions[0] != null)
		{
			return this.Actions[0].GetControllerTriggerEffectPull();
		}
		if (this.Actions[1] != null)
		{
			return this.Actions[1].GetControllerTriggerEffectPull();
		}
		return TriggerEffectManager.NoneEffect;
	}

	public virtual TriggerEffectManager.ControllerTriggerEffect GetControllerTriggerEffectShoot()
	{
		if (this.Actions[0] != null)
		{
			return this.Actions[0].GetControllerTriggerEffectShoot();
		}
		if (this.Actions[1] != null)
		{
			return this.Actions[1].GetControllerTriggerEffectShoot();
		}
		return TriggerEffectManager.NoneEffect;
	}

	public virtual bool IsActivated(ItemValue _value)
	{
		return _value.Activated > 0;
	}

	public virtual void SetActivated(ref ItemValue _value, bool _activated)
	{
		_value.Activated = (_activated ? 1 : 0);
	}

	public virtual Vector3 GetDroppedCorrectionRotation()
	{
		return new Vector3(-90f, 0f, 0f);
	}

	public virtual Vector3 GetCorrectionRotation()
	{
		return Vector3.zero;
	}

	public virtual Vector3 GetCorrectionPosition()
	{
		return Vector3.zero;
	}

	public virtual Vector3 GetCorrectionScale()
	{
		return Vector3.zero;
	}

	public virtual void OnDamagedByExplosion(ItemWorldData _data)
	{
	}

	public static int GetFuelValue(ItemValue _itemValue)
	{
		ItemClass itemClass = ItemClass.list[_itemValue.type];
		if (itemClass == null)
		{
			return 0;
		}
		if (itemClass.IsBlock())
		{
			return Block.list[_itemValue.type].FuelValue;
		}
		if (itemClass.FuelValue == null)
		{
			return 0;
		}
		return itemClass.FuelValue.Value;
	}

	public int GetWeight()
	{
		if (this.IsBlock())
		{
			return this.GetBlock().GetWeight();
		}
		if (base.Weight != null)
		{
			return base.Weight.Value;
		}
		return 0;
	}

	public string GetImageEffect()
	{
		if (base.ImageEffectOnActive != null)
		{
			return base.ImageEffectOnActive.Value;
		}
		return "";
	}

	public bool GetActive()
	{
		return base.Active != null && base.Active.Value;
	}

	public string GetSoundOnActive()
	{
		if (base.PlaySoundOnActive != null)
		{
			return base.PlaySoundOnActive.Value;
		}
		return "";
	}

	public void SetWeight(int _w)
	{
		if (this.IsBlock())
		{
			this.GetBlock().Weight = new DataItem<int>(_w);
			return;
		}
		base.Weight = new DataItem<int>(_w);
	}

	public void SetImageEffect(string _str)
	{
		base.ImageEffectOnActive = new DataItem<string>(_str);
	}

	public void SetActive(bool _bOn)
	{
		base.Active = new DataItem<bool>(_bOn);
	}

	public void SetSoundOnActive(string _str)
	{
		base.PlaySoundOnActive = new DataItem<string>(_str);
	}

	public int AutoCalcWeight(Dictionary<string, List<Recipe>> _recipesByName)
	{
		Block block = this.IsBlock() ? this.GetBlock() : null;
		if (block != null)
		{
			if (block.Weight != null)
			{
				if (block.Weight.Value != -1)
				{
					return block.Weight.Value;
				}
				return 0;
			}
			else
			{
				block.Weight = new DataItem<int>(-1);
			}
		}
		else if (base.Weight != null)
		{
			if (base.Weight.Value != -1)
			{
				return base.Weight.Value;
			}
			return 0;
		}
		else
		{
			base.Weight = new DataItem<int>(-1);
		}
		int num = 0;
		int num2 = 0;
		List<Recipe> list;
		if (_recipesByName.TryGetValue(this.GetItemName(), out list))
		{
			Recipe recipe = list[0];
			for (int i = 0; i < recipe.ingredients.Count; i++)
			{
				ItemStack itemStack = recipe.ingredients[i];
				ItemClass forId = ItemClass.GetForId(itemStack.itemValue.type);
				ItemClass forId2 = ItemClass.GetForId(recipe.itemValueType);
				if (recipe.materialBasedRecipe)
				{
					if (forId2.MadeOfMaterial.ForgeCategory != null && forId.MadeOfMaterial.ForgeCategory != null && forId2.MadeOfMaterial.ForgeCategory.EqualsCaseInsensitive(forId.MadeOfMaterial.ForgeCategory))
					{
						num += forId.AutoCalcWeight(_recipesByName) * itemStack.count;
						num2++;
						break;
					}
				}
				else if (forId2.MadeOfMaterial.ForgeCategory != null && forId.MadeOfMaterial.ForgeCategory != null && forId2.MadeOfMaterial.ForgeCategory.EqualsCaseInsensitive(forId.MadeOfMaterial.ForgeCategory))
				{
					if (ItemClass.GetForId(itemStack.itemValue.type).GetWeight() > 0)
					{
						num += ItemClass.GetForId(itemStack.itemValue.type).GetWeight() * itemStack.count;
						num2++;
					}
					else
					{
						num += forId.AutoCalcWeight(_recipesByName) * itemStack.count;
						num2++;
					}
				}
			}
			num /= ((num2 > 1) ? recipe.count : 1);
		}
		if (block != null)
		{
			block.Weight = new DataItem<int>(num);
		}
		else
		{
			base.Weight = new DataItem<int>(num);
		}
		return num;
	}

	public virtual bool HasAnyTags(FastTags<TagGroup.Global> _tags)
	{
		return this.ItemTags.Test_AnySet(_tags);
	}

	public virtual bool HasAllTags(FastTags<TagGroup.Global> _tags)
	{
		return this.ItemTags.Test_AllSet(_tags);
	}

	public static ItemClass GetItemWithTag(FastTags<TagGroup.Global> _tags)
	{
		if (ItemClass.list != null)
		{
			for (int i = 0; i < ItemClass.list.Length; i++)
			{
				if (ItemClass.list[i] != null && ItemClass.list[i].HasAllTags(_tags))
				{
					return ItemClass.list[i];
				}
			}
		}
		return null;
	}

	public static List<ItemClass> GetItemsWithTag(FastTags<TagGroup.Global> _tags)
	{
		List<ItemClass> list = new List<ItemClass>();
		if (ItemClass.list != null)
		{
			for (int i = 0; i < ItemClass.list.Length; i++)
			{
				if (ItemClass.list[i] != null && ItemClass.list[i].HasAllTags(_tags))
				{
					list.Add(ItemClass.list[i]);
				}
			}
		}
		return list;
	}

	public virtual bool CanCollect(ItemValue _iv)
	{
		return true;
	}

	public float AutoCalcEcoVal(Dictionary<string, List<Recipe>> _recipesByName, List<string> _recipeCalcStack)
	{
		string itemName = this.GetItemName();
		if (_recipeCalcStack.ContainsWithComparer(itemName, StringComparer.Ordinal))
		{
			return -1f;
		}
		Block block = this.IsBlock() ? this.GetBlock() : null;
		float num = (block != null) ? block.EconomicValue : this.EconomicValue;
		if (num > 0f)
		{
			return num;
		}
		if ((double)num < -0.1)
		{
			return 0f;
		}
		_recipeCalcStack.Add(itemName);
		float num2 = 0f;
		int num3 = 0;
		List<Recipe> list;
		if (_recipesByName.TryGetValue(itemName, out list))
		{
			foreach (Recipe recipe in list)
			{
				for (int i = 0; i < recipe.ingredients.Count; i++)
				{
					ItemStack itemStack = recipe.ingredients[i];
					float num4 = ItemClass.GetForId(itemStack.itemValue.type).AutoCalcEcoVal(_recipesByName, _recipeCalcStack);
					if (num4 < 0f)
					{
						num2 = -1f;
						break;
					}
					num2 += (float)itemStack.count * num4;
					num3++;
				}
				if (num2 >= 0f)
				{
					num2 /= (float)((num3 > 1) ? recipe.count : 1);
					break;
				}
			}
		}
		_recipeCalcStack.RemoveAt(_recipeCalcStack.Count - 1);
		if (num2 < 0f)
		{
			return -1f;
		}
		if (num2 == 0f)
		{
			num2 = 1f;
		}
		if (block != null)
		{
			block.EconomicValue = num2;
		}
		this.EconomicValue = num2;
		return num2;
	}

	public void FireEvent(MinEventTypes _eventType, MinEventParams _eventParms)
	{
		if (this.Effects != null)
		{
			this.Effects.FireEvent(_eventType, _eventParms);
		}
	}

	public bool IsEatDistraction
	{
		get
		{
			return this.DistractionTags.Test_AnySet(ItemClass.EatDistractionTag);
		}
	}

	public bool IsRequireContactDistraction
	{
		get
		{
			return this.DistractionTags.Test_AnySet(ItemClass.RequiresContactDistractionTag);
		}
	}

	public bool HasTrigger(MinEventTypes _eventType)
	{
		return this.Effects != null && this.Effects.HasTrigger(_eventType);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void assignIdsFromXml()
	{
		Log.Out("ItemIDs from XML");
		foreach (KeyValuePair<string, ItemClass> keyValuePair in ItemClass.nameToItem)
		{
			if (!keyValuePair.Value.IsBlock())
			{
				ItemClass.assignId(keyValuePair.Value, keyValuePair.Value.Id, null);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void assignIdsLinear()
	{
		Log.Out("ItemIDs linear");
		bool[] usedIds = new bool[ItemClass.MAX_ITEMS];
		List<ItemClass> list = new List<ItemClass>(ItemClass.nameToItem.Count);
		ItemClass.nameToItem.CopyValuesTo(list);
		ItemClass.assignLeftOverItems(usedIds, list);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void assignId(ItemClass _b, int _id, bool[] _usedIds)
	{
		ItemClass.list[_id] = _b;
		_b.SetId(_id);
		if (_usedIds != null)
		{
			_usedIds[_id] = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void assignLeftOverItems(bool[] _usedIds, List<ItemClass> _unassignedItems)
	{
		foreach (KeyValuePair<string, int> keyValuePair in ItemClass.fixedItemIds)
		{
			if (ItemClass.nameToItem.ContainsKey(keyValuePair.Key))
			{
				ItemClass itemClass = ItemClass.nameToItem[keyValuePair.Key];
				if (_unassignedItems.Contains(itemClass))
				{
					_unassignedItems.Remove(itemClass);
					ItemClass.assignId(itemClass, keyValuePair.Value + Block.ItemsStartHere, _usedIds);
				}
			}
		}
		int num = Block.ItemsStartHere;
		foreach (ItemClass itemClass2 in _unassignedItems)
		{
			if (!itemClass2.IsBlock())
			{
				while (_usedIds[++num])
				{
				}
				ItemClass.assignId(itemClass2, num, _usedIds);
			}
		}
		Log.Out("ItemClass assignLeftOverItems {0} of {1}", new object[]
		{
			num,
			ItemClass.MAX_ITEMS
		});
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void assignIdsFromMapping()
	{
		Log.Out("ItemIDs from Mapping");
		List<ItemClass> list = new List<ItemClass>();
		bool[] usedIds = new bool[ItemClass.MAX_ITEMS];
		foreach (KeyValuePair<string, ItemClass> keyValuePair in ItemClass.nameToItem)
		{
			if (!keyValuePair.Value.IsBlock())
			{
				int idForName = ItemClass.nameIdMapping.GetIdForName(keyValuePair.Key);
				if (idForName >= 0)
				{
					ItemClass.assignId(keyValuePair.Value, idForName, usedIds);
				}
				else
				{
					list.Add(keyValuePair.Value);
				}
			}
		}
		ItemClass.assignLeftOverItems(usedIds, list);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void createFullMappingForClients()
	{
		NameIdMapping nameIdMapping = new NameIdMapping(null, ItemClass.MAX_ITEMS);
		foreach (KeyValuePair<string, ItemClass> keyValuePair in ItemClass.nameToItem)
		{
			nameIdMapping.AddMapping(keyValuePair.Value.Id, keyValuePair.Key, false);
		}
		ItemClass.fullMappingDataForClients = nameIdMapping.SaveToArray();
	}

	public static void AssignIds()
	{
		if (ItemClass.nameIdMapping != null)
		{
			Log.Out("Item IDs with mapping");
			ItemClass.assignIdsFromMapping();
		}
		else
		{
			Log.Out("Item IDs withOUT mapping");
			ItemClass.assignIdsLinear();
		}
		ItemClass.createFullMappingForClients();
	}

	public static readonly FastTags<TagGroup.Global> EatDistractionTag = FastTags<TagGroup.Global>.GetTag("eat");

	public static readonly FastTags<TagGroup.Global> RequiresContactDistractionTag = FastTags<TagGroup.Global>.GetTag("requires_contact");

	public static string PropSoundIdle = "SoundIdle";

	public static string PropSoundDestroy = "SoundDestroy";

	public static string PropSoundJammed = "SoundJammed";

	public static string PropSoundHolster = "SoundHolster";

	public static string PropSoundUnholster = "SoundUnholster";

	public static string PropSoundStick = "SoundStick";

	public static string PropSoundTick = "SoundTick";

	public static string PropSoundPickup = "SoundPickup";

	public static string PropSoundPlace = "SoundPlace";

	public static string PropFuelValue = "FuelValue";

	public static string PropWeight = "Weight";

	public static string PropMoldTarget = "MoldTarget";

	public static string PropSmell = "Smell";

	public static string PropLightSource = "LightSource";

	public static string PropLightValue = "LightValue";

	public static string PropMatEmission = "MatEmission";

	public static string PropActivateObject = "ActivateObject";

	public static string PropThrowableDecoy = "ThrowableDecoy";

	public static string PropGroupName = "Group";

	public static string PropCritChance = "CritChance";

	public static string PropCustomIcon = "CustomIcon";

	public static string PropCustomIconTint = "CustomIconTint";

	public static string PropPartType = "PartType";

	public static string PropImageEffectOnActive = "ImageEffectOnActive";

	public static string PropPlaySoundOnActive = "PlaySoundOnActive";

	public static string PropActive = "Active";

	public static string PropAlwaysActive = "AlwaysActive";

	public static string PropHoldingItemHidden = "HoldingItemHidden";

	public static string PropVehicleSlotType = "VehicleSlotType";

	public static string PropGetQualityFromWeapon = "GetQualityFromWeapon";

	public static string PropAttributes = "Attributes";

	public static string PropCraftExpValue = "CraftingIngredientExp";

	public static string PropCraftTimeValue = "CraftingIngredientTime";

	public static string PropLootExpValue = "LootExpValue";

	public static string PropEconomicValue = "EconomicValue";

	public static string PropEconomicSellScale = "EconomicSellScale";

	public static string PropEconomicBundleSize = "EconomicBundleSize";

	public static string PropSellableToTrader = "SellableToTrader";

	public static string PropCraftingSkillExp = "CraftingSkillExp";

	public static string PropActionSkillExp = "ActionSkillExp";

	public static string PropInsulation = "Insulation";

	public static string PropWaterproof = "Waterproof";

	public static string PropEncumbrance = "Encumbrance";

	public static string PropDescriptionKey = "DescriptionKey";

	public static string PropResourceUnit = "ResourceUnit";

	public static string PropMeltTimePerUnit = "MeltTimePerUnit";

	public static string PropActionSkillGroup = "ActionSkillGroup";

	public static string PropCraftingSkillGroup = "CraftingSkillGroup";

	public static string PropCrosshairOnAim = "CrosshairOnAim";

	public static string PropCrosshairUpAfterShot = "CrosshairUpAfterShot";

	public static string PropUsableUnderwater = "UsableUnderwater";

	public static string PropRepairExpMultiplier = "RepairExpMultiplier";

	public static string PropTags = "Tags";

	public static string PropShowQuality = "ShowQuality";

	public static string PropSoundSightIn = "Sound_Sight_In";

	public static string PropSoundSightOut = "Sound_Sight_Out";

	public static string PropIgnoreKeystoneSound = "IgnoreKeystoneSound";

	public static string PropCreativeMode = "CreativeMode";

	public static string PropCreativeSort1 = "SortOrder1";

	public static string PropCreativeSort2 = "SortOrder2";

	public static string PropDistractionTags = "DistractionTags";

	public static string PropIsSticky = "IsSticky";

	public static string PropDisplayType = "DisplayType";

	public static string PropItemTypeIcon = "ItemTypeIcon";

	public static string PropAltItemTypeIcon = "AltItemTypeIcon";

	public static string PropAltItemTypeIconColor = "AltItemTypeIconColor";

	public static string PropUnlockedBy = "UnlockedBy";

	public static string PropUnlocks = "Unlocks";

	public static string PropNoScrapping = "NoScrapping";

	public static string PropScrapTimeOverride = "ScrapTimeOverride";

	public static string PropNavObject = "NavObject";

	public static string PropQuestItem = "IsQuestItem";

	public static string PropTrackerIndexName = "TrackerIndexName";

	public static string PropTrackerNavObject = "TrackerNavObject";

	public static string PropTraderStageTemplate = "TraderStageTemplate";

	public static int MAX_ITEMS = Block.MAX_BLOCKS + 16384;

	public static NameIdMapping nameIdMapping;

	public static byte[] fullMappingDataForClients;

	public static ItemClass[] list;

	public static string[] itemActionNames;

	public const int cMaxActionNames = 5;

	[PublicizedFrom(EAccessModifier.Protected)]
	public static Dictionary<string, ItemClass> nameToItem = new Dictionary<string, ItemClass>();

	[PublicizedFrom(EAccessModifier.Protected)]
	public static Dictionary<string, ItemClass> nameToItemCaseInsensitive = new CaseInsensitiveStringDictionary<ItemClass>();

	[PublicizedFrom(EAccessModifier.Protected)]
	public static List<string> itemNames = new List<string>();

	public static readonly ReadOnlyCollection<string> ItemNames = new ReadOnlyCollection<string>(ItemClass.itemNames);

	public DynamicProperties Properties;

	[PublicizedFrom(EAccessModifier.Private)]
	public string localizedName;

	public AIDirectorData.Smell Smell;

	public string MeshFile;

	public string DropMeshFile;

	public string HandMeshFile;

	public string StickyMaterial;

	public float StickyOffset = 0.15f;

	public int StickyColliderUp = -1;

	public float StickyColliderRadius = 0.2f;

	public float StickyColliderLength = -1f;

	public bool IsSticky;

	public const int cActionUpdateCount = 3;

	public ItemAction[] Actions;

	public MaterialBlock MadeOfMaterial;

	public DataItem<int> HoldType = new DataItem<int>(0);

	public DataItem<int> Stacknumber = new DataItem<int>(500);

	public DataItem<int> MaxUseTimes = new DataItem<int>(0);

	public DataItem<bool> MaxUseTimesBreaksAfter = new DataItem<bool>(false);

	public ItemClass MoldTarget;

	public DataItem<string> LightSource;

	public DataItem<string> ActivateObject;

	public DataItem<bool> ThrowableDecoy;

	public ItemData.DataItemArrayRepairTools RepairTools;

	public DataItem<int> RepairAmount;

	public DataItem<float> RepairTime;

	public DataItem<float> CritChance;

	public string[] Groups = new string[]
	{
		"Decor/Miscellaneous"
	};

	public DataItem<string> CustomIcon;

	public Color CustomIconTint;

	public DataItem<bool> UserHidden = new DataItem<bool>(false);

	public string VehicleSlotType;

	public bool GetQualityFromWeapon;

	public string DescriptionKey;

	public bool IsResourceUnit;

	public float MeltTimePerUnit;

	public string ActionSkillGroup = "";

	public string CraftingSkillGroup = "";

	public bool UsableUnderwater = true;

	public float lightValue;

	public string soundSightIn = "";

	public string soundSightOut = "";

	public bool ignoreKeystoneSound;

	public bool HoldingItemHidden;

	public List<int> PartParentId;

	public PreviewData Preview;

	[PublicizedFrom(EAccessModifier.Protected)]
	public GameObject renderGameObject;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool bCanHold;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool bCanDrop;

	public bool HasSubItems;

	[PublicizedFrom(EAccessModifier.Private)]
	public string[] attachments;

	public bool bCraftingTool;

	public float CraftComponentExp = 3f;

	public float CraftComponentTime = 1f;

	public float LootExp = 1f;

	public float EconomicValue;

	public float EconomicSellScale = 1f;

	public int EconomicBundleSize = 1;

	public bool SellableToTrader = true;

	public string TraderStageTemplate;

	public int CraftingSkillExp = 10;

	public int ActionSkillExp = 10;

	public float RepairExpMultiplier = 10f;

	public float Insulation;

	public float WaterProof;

	public float Encumbrance;

	public string SoundUnholster = "generic_unholster";

	public string SoundHolster = "generic_holster";

	public string SoundStick;

	public string SoundTick;

	public float SoundTickDelay = 1f;

	public string SoundPickup = "craft_take_item";

	public string SoundPlace = "craft_place_item";

	public bool bShowCrosshairOnAiming;

	public bool bCrosshairUpAfterShot;

	public EnumCreativeMode CreativeMode;

	public string SortOrder;

	public FastTags<TagGroup.Global> ItemTags;

	public MinEffectController Effects;

	public FastTags<TagGroup.Global> DistractionTags;

	public SDCSUtils.SlotData SDCSData;

	public string DisplayType;

	public string ItemTypeIcon = "";

	public string AltItemTypeIcon;

	public Color AltItemTypeIconColor;

	public bool ShowQualityBar;

	public bool NoScrapping;

	public float ScrapTimeOverride;

	public string Unlocks = "";

	public string NavObject = "";

	public bool IsQuestItem;

	[PublicizedFrom(EAccessModifier.Private)]
	public FastTags<TagGroup.Global> stopBleed = FastTags<TagGroup.Global>.Parse("stopsBleeding");

	public string TrackerIndexName;

	public string TrackerNavObject;

	[PublicizedFrom(EAccessModifier.Private)]
	public RecipeUnlockData[] unlockedBy;

	[PublicizedFrom(EAccessModifier.Private)]
	public static string[] ActionProfilerNames = new string[]
	{
		"action0",
		"action1",
		"action2"
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Dictionary<string, int> fixedItemIds = new Dictionary<string, int>();

	public enum EnumCrosshairType
	{
		None,
		Plus,
		Crosshair,
		CrosshairOnAiming,
		Damage,
		Upgrade,
		Repair,
		PowerSource,
		Heal,
		PowerItem
	}

	public delegate bool FilterItem(ItemClass _class, Block _block);
}
