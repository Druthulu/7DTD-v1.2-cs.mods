using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_HUDStatBar : XUiController
{
	public HUDStatGroups StatGroup
	{
		get
		{
			return this.statGroup;
		}
		set
		{
			this.statGroup = value;
		}
	}

	public HUDStatTypes StatType
	{
		get
		{
			return this.statType;
		}
		set
		{
			this.statType = value;
			this.SetStatValues();
		}
	}

	public EntityPlayerLocal LocalPlayer { get; [PublicizedFrom(EAccessModifier.Internal)] set; }

	public EntityVehicle Vehicle { get; [PublicizedFrom(EAccessModifier.Internal)] set; }

	public override void Init()
	{
		base.Init();
		this.IsDirty = true;
		XUiController childById = base.GetChildById("BarContent");
		if (childById != null)
		{
			this.barContent = (XUiV_Sprite)childById.ViewComponent;
		}
		XUiController childById2 = base.GetChildById("TextContent");
		if (childById2 != null)
		{
			this.textContent = (XUiV_Label)childById2.ViewComponent;
		}
		this.activeAmmoItemValue = ItemValue.None.Clone();
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		this.deltaTime = _dt;
		if (this.LocalPlayer == null && XUi.IsGameRunning())
		{
			this.LocalPlayer = base.xui.playerUI.entityPlayer;
		}
		GUIWindowManager windowManager = base.xui.playerUI.windowManager;
		if (windowManager.IsFullHUDDisabled())
		{
			this.viewComponent.IsVisible = false;
			return;
		}
		if (!base.xui.dragAndDrop.InMenu && windowManager.IsHUDPartialHidden())
		{
			this.viewComponent.IsVisible = false;
			return;
		}
		if (this.statGroup == HUDStatGroups.Vehicle && this.LocalPlayer != null)
		{
			if (this.Vehicle == null && this.LocalPlayer.AttachedToEntity != null && this.LocalPlayer.AttachedToEntity is EntityVehicle)
			{
				this.Vehicle = (EntityVehicle)this.LocalPlayer.AttachedToEntity;
				this.IsDirty = true;
				base.xui.CollectedItemList.SetYOffset(100);
			}
			else if (this.Vehicle != null && this.LocalPlayer.AttachedToEntity == null)
			{
				this.Vehicle = null;
				this.IsDirty = true;
			}
		}
		if (this.statType == HUDStatTypes.Stealth && this.LocalPlayer.IsCrouching != this.wasCrouching)
		{
			this.wasCrouching = this.LocalPlayer.IsCrouching;
			base.RefreshBindings(true);
			this.IsDirty = true;
		}
		if (this.statType == HUDStatTypes.ActiveItem)
		{
			if (this.currentSlotIndex != base.xui.PlayerInventory.Toolbelt.GetFocusedItemIdx())
			{
				this.currentSlotIndex = base.xui.PlayerInventory.Toolbelt.GetFocusedItemIdx();
				this.IsDirty = true;
			}
			if (this.HasChanged() || this.IsDirty)
			{
				this.SetupActiveItemEntry();
				this.updateActiveItemAmmo();
				base.RefreshBindings(true);
				this.IsDirty = false;
				return;
			}
		}
		else
		{
			this.RefreshFill();
			if (this.HasChanged() || this.IsDirty)
			{
				if (this.IsDirty)
				{
					this.IsDirty = false;
				}
				base.RefreshBindings(true);
			}
		}
	}

	public bool HasChanged()
	{
		bool result = false;
		switch (this.statType)
		{
		case HUDStatTypes.Health:
			result = true;
			break;
		case HUDStatTypes.Stamina:
			result = true;
			break;
		case HUDStatTypes.Water:
			result = (this.oldValue != this.LocalPlayer.Stats.Water.ValuePercentUI);
			this.oldValue = this.LocalPlayer.Stats.Water.ValuePercentUI;
			break;
		case HUDStatTypes.Food:
			result = (this.oldValue != this.LocalPlayer.Stats.Food.ValuePercentUI);
			this.oldValue = this.LocalPlayer.Stats.Food.ValuePercentUI;
			break;
		case HUDStatTypes.Stealth:
			result = (this.oldValue != this.lastValue);
			this.oldValue = this.lastValue;
			break;
		case HUDStatTypes.ActiveItem:
		{
			ItemAction itemAction = this.LocalPlayer.inventory.holdingItemItemValue.ItemClass.Actions[0];
			if (itemAction != null && itemAction.IsEditingTool())
			{
				result = itemAction.IsStatChanged();
			}
			break;
		}
		case HUDStatTypes.VehicleHealth:
		{
			if (this.Vehicle == null)
			{
				return false;
			}
			int health = this.Vehicle.GetVehicle().GetHealth();
			result = (this.oldValue != (float)health);
			this.oldValue = (float)health;
			break;
		}
		case HUDStatTypes.VehicleFuel:
			if (this.Vehicle == null)
			{
				return false;
			}
			result = (this.oldValue != this.Vehicle.GetVehicle().GetFuelLevel());
			this.oldValue = this.Vehicle.GetVehicle().GetFuelLevel();
			break;
		case HUDStatTypes.VehicleBattery:
			if (this.Vehicle == null)
			{
				return false;
			}
			result = (this.oldValue != this.Vehicle.GetVehicle().GetBatteryLevel());
			this.oldValue = this.Vehicle.GetVehicle().GetBatteryLevel();
			break;
		}
		return result;
	}

	public void RefreshFill()
	{
		if (this.barContent == null || this.LocalPlayer == null || (this.statGroup == HUDStatGroups.Vehicle && this.Vehicle == null))
		{
			return;
		}
		float t = Time.deltaTime * 3f;
		float b = 0f;
		switch (this.statType)
		{
		case HUDStatTypes.Health:
			b = Mathf.Clamp01(this.LocalPlayer.Stats.Health.ValuePercentUI);
			break;
		case HUDStatTypes.Stamina:
			b = Mathf.Clamp01(this.LocalPlayer.Stats.Stamina.ValuePercentUI);
			break;
		case HUDStatTypes.Water:
			b = this.LocalPlayer.Stats.Water.ValuePercentUI;
			break;
		case HUDStatTypes.Food:
			b = this.LocalPlayer.Stats.Food.ValuePercentUI;
			break;
		case HUDStatTypes.Stealth:
			b = this.LocalPlayer.Stealth.ValuePercentUI;
			break;
		case HUDStatTypes.ActiveItem:
			b = (float)this.LocalPlayer.inventory.holdingItemItemValue.Meta / EffectManager.GetValue(PassiveEffects.MagazineSize, this.LocalPlayer.inventory.holdingItemItemValue, (float)this.attackAction.BulletsPerMagazine, this.LocalPlayer, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
			break;
		case HUDStatTypes.VehicleHealth:
			b = this.Vehicle.GetVehicle().GetHealthPercent();
			break;
		case HUDStatTypes.VehicleFuel:
			b = this.Vehicle.GetVehicle().GetFuelPercent();
			break;
		case HUDStatTypes.VehicleBattery:
			b = this.Vehicle.GetVehicle().GetBatteryLevel();
			break;
		}
		float fill = Math.Max(this.lastValue, 0f);
		this.lastValue = Mathf.Lerp(this.lastValue, b, t);
		this.barContent.Fill = fill;
	}

	public override bool GetBindingValue(ref string value, string bindingName)
	{
		uint num = <PrivateImplementationDetails>.ComputeStringHash(bindingName);
		if (num <= 2825508620U)
		{
			if (num <= 1542587592U)
			{
				if (num != 669092238U)
				{
					if (num != 1122103630U)
					{
						if (num == 1542587592U)
						{
							if (bindingName == "statcurrent")
							{
								if (this.LocalPlayer == null || (this.statGroup == HUDStatGroups.Vehicle && this.Vehicle == null))
								{
									value = "";
									return true;
								}
								switch (this.statType)
								{
								case HUDStatTypes.Health:
									value = this.statcurrentFormatterInt.Format(this.LocalPlayer.Health);
									break;
								case HUDStatTypes.Stamina:
									value = this.statcurrentFormatterFloat.Format(this.LocalPlayer.Stamina);
									break;
								case HUDStatTypes.Water:
									value = this.statcurrentFormatterInt.Format((int)(this.LocalPlayer.Stats.Water.ValuePercentUI * 100f));
									break;
								case HUDStatTypes.Food:
									value = this.statcurrentFormatterInt.Format((int)(this.LocalPlayer.Stats.Food.ValuePercentUI * 100f));
									break;
								case HUDStatTypes.Stealth:
									value = this.statcurrentFormatterFloat.Format((float)((int)(this.LocalPlayer.Stealth.ValuePercentUI * 100f)));
									break;
								case HUDStatTypes.ActiveItem:
									if (this.attackAction is ItemActionTextureBlock)
									{
										value = this.currentPaintAmmoFormatter.Format(this.currentAmmoCount);
									}
									else
									{
										value = this.statcurrentFormatterInt.Format(this.LocalPlayer.inventory.holdingItemItemValue.Meta);
									}
									break;
								case HUDStatTypes.VehicleHealth:
									value = this.statcurrentFormatterInt.Format(this.Vehicle.GetVehicle().GetHealth());
									break;
								case HUDStatTypes.VehicleFuel:
									value = this.statcurrentFormatterFloat.Format(this.Vehicle.GetVehicle().GetFuelLevel());
									break;
								case HUDStatTypes.VehicleBattery:
									value = this.statcurrentFormatterFloat.Format(this.Vehicle.GetVehicle().GetBatteryLevel());
									break;
								}
								return true;
							}
						}
					}
					else if (bindingName == "statcurrentwithmax")
					{
						if (this.LocalPlayer == null || (this.statGroup == HUDStatGroups.Vehicle && this.Vehicle == null))
						{
							value = "";
							return true;
						}
						switch (this.statType)
						{
						case HUDStatTypes.Health:
							value = this.statcurrentWMaxFormatterAOfB.Format((int)this.LocalPlayer.Stats.Health.Value, (int)this.LocalPlayer.Stats.Health.Max);
							break;
						case HUDStatTypes.Stamina:
							value = this.statcurrentWMaxFormatterAOfB.Format((int)XUiM_Player.GetStamina(this.LocalPlayer), (int)this.LocalPlayer.Stats.Stamina.Max);
							break;
						case HUDStatTypes.Water:
							value = this.statcurrentWMaxFormatterOf100.Format((int)(this.LocalPlayer.Stats.Water.ValuePercentUI * 100f));
							break;
						case HUDStatTypes.Food:
							value = this.statcurrentWMaxFormatterOf100.Format((int)(this.LocalPlayer.Stats.Food.ValuePercentUI * 100f));
							break;
						case HUDStatTypes.Stealth:
							value = this.statcurrentWMaxFormatterOf100.Format((int)(this.LocalPlayer.Stealth.ValuePercentUI * 100f));
							break;
						case HUDStatTypes.ActiveItem:
							if (this.attackAction is ItemActionTextureBlock)
							{
								value = this.currentPaintAmmoFormatter.Format(this.currentAmmoCount);
							}
							else if (this.attackAction != null && this.attackAction.IsEditingTool())
							{
								ItemActionData itemActionDataInSlot = this.LocalPlayer.inventory.GetItemActionDataInSlot(this.currentSlotIndex, 1);
								value = this.attackAction.GetStat(itemActionDataInSlot);
							}
							else
							{
								value = this.statcurrentWMaxFormatterAOfB.Format(this.LocalPlayer.inventory.GetItem(this.currentSlotIndex).itemValue.Meta, this.currentAmmoCount);
							}
							break;
						case HUDStatTypes.VehicleHealth:
							value = this.statcurrentWMaxFormatterPercent.Format((int)(this.Vehicle.GetVehicle().GetHealthPercent() * 100f));
							break;
						case HUDStatTypes.VehicleFuel:
							value = this.statcurrentWMaxFormatterPercent.Format((int)(this.Vehicle.GetVehicle().GetFuelPercent() * 100f));
							break;
						case HUDStatTypes.VehicleBattery:
							value = this.statcurrentWMaxFormatterPercent.Format((int)(this.Vehicle.GetVehicle().GetBatteryLevel() * 100f));
							break;
						}
						return true;
					}
				}
				else if (bindingName == "statfill")
				{
					if (this.LocalPlayer == null || (this.statGroup == HUDStatGroups.Vehicle && this.Vehicle == null))
					{
						value = "0";
						return true;
					}
					float t = this.deltaTime * 3f;
					float b = 0f;
					switch (this.statType)
					{
					case HUDStatTypes.Health:
						b = this.LocalPlayer.Stats.Health.ValuePercentUI;
						break;
					case HUDStatTypes.Stamina:
						b = this.LocalPlayer.Stats.Stamina.ValuePercentUI;
						break;
					case HUDStatTypes.Water:
						b = this.LocalPlayer.Stats.Water.ValuePercentUI;
						break;
					case HUDStatTypes.Food:
						b = this.LocalPlayer.Stats.Food.ValuePercentUI;
						break;
					case HUDStatTypes.Stealth:
						b = this.LocalPlayer.Stealth.ValuePercentUI;
						break;
					case HUDStatTypes.ActiveItem:
						b = (float)this.LocalPlayer.inventory.holdingItemItemValue.Meta / EffectManager.GetValue(PassiveEffects.MagazineSize, this.LocalPlayer.inventory.holdingItemItemValue, (float)this.attackAction.BulletsPerMagazine, this.LocalPlayer, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
						break;
					case HUDStatTypes.VehicleHealth:
						b = this.Vehicle.GetVehicle().GetHealthPercent();
						break;
					case HUDStatTypes.VehicleFuel:
						b = this.Vehicle.GetVehicle().GetFuelPercent();
						break;
					case HUDStatTypes.VehicleBattery:
						b = this.Vehicle.GetVehicle().GetBatteryLevel();
						break;
					}
					float v = Math.Max(this.lastValue, 0f) * 1.01f;
					value = this.statfillFormatter.Format(v);
					this.lastValue = Mathf.Lerp(this.lastValue, b, t);
					return true;
				}
			}
			else if (num != 1822678806U)
			{
				if (num != 2758588565U)
				{
					if (num == 2825508620U)
					{
						if (bindingName == "statimage")
						{
							value = this.statImage;
							return true;
						}
					}
				}
				else if (bindingName == "staticonatlas")
				{
					value = this.statAtlas;
					return true;
				}
			}
			else if (bindingName == "staticon")
			{
				if (this.statType == HUDStatTypes.ActiveItem)
				{
					value = ((this.itemClass != null) ? this.itemClass.GetIconName() : "");
				}
				else if (this.statType == HUDStatTypes.VehicleHealth)
				{
					value = ((this.Vehicle != null) ? this.Vehicle.GetMapIcon() : "");
				}
				else
				{
					value = this.statIcon;
				}
				return true;
			}
		}
		else if (num <= 3799067675U)
		{
			if (num != 3007315583U)
			{
				if (num != 3150708601U)
				{
					if (num == 3799067675U)
					{
						if (bindingName == "statvisible")
						{
							if (this.LocalPlayer == null)
							{
								value = "true";
								return true;
							}
							value = "true";
							if (this.LocalPlayer.IsDead())
							{
								value = "false";
								return true;
							}
							if (this.statGroup == HUDStatGroups.Vehicle)
							{
								if (this.statType == HUDStatTypes.VehicleFuel)
								{
									value = (this.Vehicle != null && this.Vehicle.GetVehicle().HasEnginePart()).ToString();
								}
								else
								{
									value = (this.Vehicle != null).ToString();
								}
							}
							else if (this.statType == HUDStatTypes.ActiveItem)
							{
								if (this.attackAction != null && (this.attackAction.IsEditingTool() || (int)EffectManager.GetValue(PassiveEffects.MagazineSize, this.LocalPlayer.inventory.holdingItemItemValue, 0f, this.LocalPlayer, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false) > 0))
								{
									value = "true";
								}
								else
								{
									value = "false";
								}
							}
							else if (this.statType == HUDStatTypes.Stealth)
							{
								if (this.LocalPlayer.Crouching)
								{
									base.xui.BuffPopoutList.SetYOffset(52);
									value = "true";
								}
								else
								{
									base.xui.BuffPopoutList.SetYOffset(0);
									value = "false";
								}
							}
							return true;
						}
					}
				}
				else if (bindingName == "staticoncolor")
				{
					Color32 v2 = Color.white;
					if (this.statType == HUDStatTypes.ActiveItem && this.itemClass != null)
					{
						v2 = this.itemClass.GetIconTint(null);
					}
					value = this.staticoncolorFormatter.Format(v2);
					return true;
				}
			}
			else if (bindingName == "sprintactive")
			{
				if (this.LocalPlayer == null)
				{
					value = "false";
				}
				else if (this.LocalPlayer.MovementRunning || this.LocalPlayer.MoveController.RunToggleActive)
				{
					value = "true";
				}
				else
				{
					value = "false";
				}
				return true;
			}
		}
		else if (num != 3888153342U)
		{
			if (num != 3905392387U)
			{
				if (num == 3907838626U)
				{
					if (bindingName == "statmodifiedmax")
					{
						if (this.LocalPlayer == null || (this.statGroup == HUDStatGroups.Vehicle && this.Vehicle == null))
						{
							value = "0";
							return true;
						}
						switch (this.statType)
						{
						case HUDStatTypes.Health:
							value = this.statmodifiedmaxFormatter.Format(this.LocalPlayer.Stats.Health.ModifiedMax, this.LocalPlayer.Stats.Health.Max);
							break;
						case HUDStatTypes.Stamina:
							value = this.statmodifiedmaxFormatter.Format(this.LocalPlayer.Stats.Stamina.ModifiedMax, this.LocalPlayer.Stats.Stamina.Max);
							break;
						case HUDStatTypes.Water:
							value = this.statmodifiedmaxFormatter.Format(this.LocalPlayer.Stats.Water.ModifiedMax, this.LocalPlayer.Stats.Water.Max);
							break;
						case HUDStatTypes.Food:
							value = this.statmodifiedmaxFormatter.Format(this.LocalPlayer.Stats.Food.ModifiedMax, this.LocalPlayer.Stats.Food.Max);
							break;
						}
						return true;
					}
				}
			}
			else if (bindingName == "stealthcolor")
			{
				EntityPlayerLocal localPlayer = this.LocalPlayer;
				value = this.stealthColorFormatter.Format(localPlayer ? localPlayer.Stealth.ValueColorUI : default(Color32));
				return true;
			}
		}
		else if (bindingName == "statregenrate")
		{
			if (this.LocalPlayer == null || (this.statGroup == HUDStatGroups.Vehicle && this.Vehicle == null))
			{
				value = "0";
				return true;
			}
			switch (this.statType)
			{
			case HUDStatTypes.Health:
				value = this.statregenrateFormatter.Format(this.LocalPlayer.Stats.Health.RegenerationAmountUI);
				break;
			case HUDStatTypes.Stamina:
				value = this.statregenrateFormatter.Format(this.LocalPlayer.Stats.Stamina.RegenerationAmountUI);
				break;
			case HUDStatTypes.Water:
				value = this.statregenrateFormatter.Format(this.LocalPlayer.Stats.Water.RegenerationAmountUI);
				break;
			case HUDStatTypes.Food:
				value = this.statregenrateFormatter.Format(this.LocalPlayer.Stats.Food.RegenerationAmountUI);
				break;
			}
			return true;
		}
		return false;
	}

	public override bool ParseAttribute(string name, string value, XUiController _parent)
	{
		bool flag = base.ParseAttribute(name, value, _parent);
		if (flag)
		{
			return flag;
		}
		if (name == "stat_type")
		{
			this.StatType = EnumUtils.Parse<HUDStatTypes>(value, true);
			return true;
		}
		return false;
	}

	public void SetStatValues()
	{
		switch (this.statType)
		{
		case HUDStatTypes.Health:
			this.statImage = "ui_game_stat_bar_health";
			this.statIcon = "ui_game_symbol_add";
			this.statGroup = HUDStatGroups.Player;
			return;
		case HUDStatTypes.Stamina:
			this.statImage = "ui_game_stat_bar_stamina";
			this.statIcon = "ui_game_symbol_run";
			this.statGroup = HUDStatGroups.Player;
			return;
		case HUDStatTypes.Water:
			this.statImage = "ui_game_stat_bar_stamina";
			this.statIcon = "ui_game_symbol_water";
			this.statGroup = HUDStatGroups.Player;
			return;
		case HUDStatTypes.Food:
			this.statImage = "ui_game_stat_bar_health";
			this.statIcon = "ui_game_symbol_hunger";
			this.statGroup = HUDStatGroups.Player;
			return;
		case HUDStatTypes.Stealth:
			this.statImage = "ui_game_stat_bar_health";
			this.statIcon = "ui_game_symbol_stealth";
			this.statGroup = HUDStatGroups.Player;
			return;
		case HUDStatTypes.ActiveItem:
			this.statImage = "ui_game_popup";
			this.statIcon = "ui_game_symbol_battery";
			this.statGroup = HUDStatGroups.Player;
			this.statAtlas = "ItemIconAtlas";
			return;
		case HUDStatTypes.VehicleHealth:
			this.statImage = "ui_game_stat_bar_health";
			this.statIcon = "ui_game_symbol_minibike";
			this.statGroup = HUDStatGroups.Vehicle;
			return;
		case HUDStatTypes.VehicleFuel:
			this.statImage = "ui_game_stat_bar_stamina";
			this.statIcon = "ui_game_symbol_gas";
			this.statGroup = HUDStatGroups.Vehicle;
			return;
		case HUDStatTypes.VehicleBattery:
			this.statImage = "ui_game_popup";
			this.statIcon = "ui_game_symbol_battery";
			this.statGroup = HUDStatGroups.Vehicle;
			return;
		default:
			return;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetupActiveItemEntry()
	{
		this.itemClass = null;
		this.attackAction = null;
		this.activeAmmoItemValue = ItemValue.None.Clone();
		EntityPlayer localPlayer = this.LocalPlayer;
		if (localPlayer)
		{
			Inventory inventory = localPlayer.inventory;
			ItemValue itemValue = inventory.GetItem(this.currentSlotIndex).itemValue;
			this.itemClass = itemValue.ItemClass;
			if (this.itemClass != null)
			{
				ItemActionAttack itemActionAttack = this.itemClass.Actions[0] as ItemActionAttack;
				if (itemActionAttack != null && itemActionAttack.IsEditingTool())
				{
					this.attackAction = itemActionAttack;
					base.xui.CollectedItemList.SetYOffset(46);
					return;
				}
				if (itemActionAttack == null || itemActionAttack is ItemActionMelee || !this.itemClass.IsGun() || (int)EffectManager.GetValue(PassiveEffects.MagazineSize, inventory.holdingItemItemValue, 0f, localPlayer, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false) <= 0)
				{
					this.currentAmmoCount = 0;
					base.xui.CollectedItemList.SetYOffset((localPlayer.AttachedToEntity is EntityVehicle) ? 100 : 0);
					return;
				}
				this.attackAction = itemActionAttack;
				if (itemActionAttack.MagazineItemNames != null && itemActionAttack.MagazineItemNames.Length != 0)
				{
					this.lastAmmoName = itemActionAttack.MagazineItemNames[(int)itemValue.SelectedAmmoTypeIndex];
					this.activeAmmoItemValue = ItemClass.GetItem(this.lastAmmoName, false);
					this.itemClass = ItemClass.GetItemClass(this.lastAmmoName, false);
				}
				base.xui.CollectedItemList.SetYOffset(46);
				return;
			}
			else
			{
				this.currentAmmoCount = 0;
				base.xui.CollectedItemList.SetYOffset((localPlayer.AttachedToEntity is EntityVehicle) ? 100 : 0);
			}
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		if (this.statType == HUDStatTypes.ActiveItem)
		{
			base.xui.PlayerInventory.OnBackpackItemsChanged += this.PlayerInventory_OnBackpackItemsChanged;
			base.xui.PlayerInventory.OnToolbeltItemsChanged += this.PlayerInventory_OnToolbeltItemsChanged;
		}
		this.IsDirty = true;
		base.RefreshBindings(true);
	}

	public override void OnClose()
	{
		base.OnClose();
		base.xui.PlayerInventory.OnBackpackItemsChanged -= this.PlayerInventory_OnBackpackItemsChanged;
		base.xui.PlayerInventory.OnToolbeltItemsChanged -= this.PlayerInventory_OnToolbeltItemsChanged;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PlayerInventory_OnToolbeltItemsChanged()
	{
		this.IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PlayerInventory_OnBackpackItemsChanged()
	{
		this.IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateActiveItemAmmo()
	{
		if (this.activeAmmoItemValue.type == 0)
		{
			return;
		}
		this.currentAmmoCount = this.LocalPlayer.inventory.GetItemCount(this.activeAmmoItemValue, false, -1, -1, true);
		this.currentAmmoCount += this.LocalPlayer.bag.GetItemCount(this.activeAmmoItemValue, -1, -1, true);
		this.IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public float lastValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool flipped;

	[PublicizedFrom(EAccessModifier.Private)]
	public HUDStatGroups statGroup;

	[PublicizedFrom(EAccessModifier.Private)]
	public HUDStatTypes statType;

	[PublicizedFrom(EAccessModifier.Private)]
	public string statImage = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public string statIcon = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public string statAtlas = "UIAtlas";

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite barContent;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label textContent;

	[PublicizedFrom(EAccessModifier.Private)]
	public DateTime updateTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public float deltaTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool wasCrouching;

	[PublicizedFrom(EAccessModifier.Private)]
	public int currentSlotIndex = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lastAmmoName = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public int currentAmmoCount;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemValue activeAmmoItemValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemActionAttack attackAction;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemClass itemClass;

	[PublicizedFrom(EAccessModifier.Private)]
	public float oldValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterInt statcurrentFormatterInt = new CachedStringFormatterInt();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterFloat statcurrentFormatterFloat = new CachedStringFormatterFloat(null);

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterInt currentPaintAmmoFormatter = new CachedStringFormatterInt();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<int, int> statcurrentWMaxFormatterAOfB = new CachedStringFormatter<int, int>((int _i, int _i1) => string.Format("{0}/{1}", _i, _i1));

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<int> statcurrentWMaxFormatterOf100 = new CachedStringFormatter<int>((int _i) => _i.ToString() + "/100");

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<int> statcurrentWMaxFormatterPercent = new CachedStringFormatter<int>((int _i) => _i.ToString() + "%");

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<float, float> statmodifiedmaxFormatter = new CachedStringFormatter<float, float>((float _f1, float _f2) => (_f1 / _f2).ToCultureInvariantString());

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<float> statregenrateFormatter = new CachedStringFormatter<float>((float _f) => ((_f >= 0f) ? "+" : "") + _f.ToCultureInvariantString("0.00"));

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterFloat statfillFormatter = new CachedStringFormatterFloat(null);

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterXuiRgbaColor staticoncolorFormatter = new CachedStringFormatterXuiRgbaColor();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterXuiRgbaColor stealthColorFormatter = new CachedStringFormatterXuiRgbaColor();

	[PublicizedFrom(EAccessModifier.Private)]
	public float lastRegenAmount;
}
