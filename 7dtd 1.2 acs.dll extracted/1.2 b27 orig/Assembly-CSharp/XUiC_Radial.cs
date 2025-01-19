using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using GUI_2;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_Radial : XUiController
{
	public XUiC_RadialEntry mCurrentlySelectedEntry
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (this.selectedIndex >= 0 && this.selectedIndex < this.menuItem.Length)
			{
				return this.menuItem[this.selectedIndex];
			}
			return null;
		}
	}

	public override void Init()
	{
		base.Init();
		this.menuItem = base.GetChildrenByType<XUiC_RadialEntry>(null);
		this.menuItemState = new bool[this.menuItem.Length];
		for (int i = 0; i < this.menuItem.Length; i++)
		{
			this.menuItem[i].OnHover += this.XUiC_Radial_OnHover;
			this.menuItem[i].ViewComponent.IsVisible = false;
			this.menuItem[i].MenuItemIndex = i;
			this.menuItemState[i] = false;
		}
		this.selectionText = (base.GetChildById("selection").ViewComponent as XUiV_Label);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void XUiC_Radial_OnPress(XUiC_RadialEntry _sender)
	{
		this.selectedIndex = _sender.MenuItemIndex;
		this.CallContextAction();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void XUiC_Radial_OnHover(XUiController _sender, bool _isOver)
	{
		XUiC_RadialEntry xuiC_RadialEntry = (XUiC_RadialEntry)_sender;
		if (_isOver)
		{
			this.SelectionEffect(xuiC_RadialEntry, true);
			this.selectedIndex = xuiC_RadialEntry.MenuItemIndex;
			return;
		}
		this.SelectionEffect(xuiC_RadialEntry, false);
		this.selectedIndex = int.MinValue;
	}

	public void Open()
	{
		this.openTime = Time.time;
		this.isOpenRequested = true;
	}

	public override void OnOpen()
	{
		base.OnOpen();
		for (int i = 0; i < this.menuItem.Length; i++)
		{
			this.menuItem[i].ViewComponent.IsVisible = this.menuItemState[i];
		}
		if (!this.showingCallouts)
		{
			this.showingCallouts = true;
			base.xui.calloutWindow.ClearCallouts(XUiC_GamepadCalloutWindow.CalloutType.Menu);
			base.xui.calloutWindow.AddCallout(this.radialButtonIsLeftSide(base.xui.playerUI.playerInput) ? UIUtils.ButtonIcon.RightStick : UIUtils.ButtonIcon.LeftStick, "igcoRadialHighlight", XUiC_GamepadCalloutWindow.CalloutType.Menu);
			base.xui.calloutWindow.AddCallout(this.selectionControllerButton, "igcoRadialSelect", XUiC_GamepadCalloutWindow.CalloutType.Menu);
			base.xui.calloutWindow.EnableCallouts(XUiC_GamepadCalloutWindow.CalloutType.Menu, 0f);
		}
		if (PlatformManager.NativePlatform.Input.CurrentInputStyle != PlayerInputManager.InputStyle.Keyboard)
		{
			base.xui.playerUI.entityPlayer.SetControllable(false);
			base.xui.playerUI.CursorController.SetCursorHidden(true);
		}
	}

	public override void OnClose()
	{
		base.OnClose();
		for (int i = 0; i < this.menuItem.Length; i++)
		{
			this.SelectionEffect(this.menuItem[i], false);
		}
		this.context = null;
		this.commandHandler = null;
		this.validCheckDelegate = null;
		base.xui.calloutWindow.DisableCallouts(XUiC_GamepadCalloutWindow.CalloutType.Menu);
		this.showingCallouts = false;
		this.isOpenRequested = false;
		base.xui.playerUI.entityPlayer.SetControllable(true);
		base.xui.playerUI.CursorController.SetCursorHidden(false);
	}

	public override bool AlwaysUpdate()
	{
		return true;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (!this.isOpenRequested)
		{
			return;
		}
		bool flag = Time.time - this.openTime >= this.displayDelay;
		if (flag && !base.IsOpen)
		{
			if (this.validCheckDelegate != null && !this.validCheckDelegate(this, this.context))
			{
				this.Close();
				return;
			}
			base.xui.playerUI.windowManager.Open("radial", true, true, true);
		}
		PlayerActionsLocal playerInput = base.xui.playerUI.playerInput;
		if (!this.radialButtonPressed(playerInput))
		{
			if (this.mCurrentlySelectedEntry != null)
			{
				this.XUiC_Radial_OnPress(this.mCurrentlySelectedEntry);
			}
			else
			{
				if (!flag)
				{
					if (this.hasSpecialActionPriorToRadialVisibility)
					{
						if (InputUtils.ShiftKeyPressed)
						{
							this.selectedIndex = -2;
						}
						else
						{
							this.selectedIndex = -1;
						}
					}
					else if (this.selectedIndex == -2147483648)
					{
						for (int i = 0; i < this.menuItemState.Length; i++)
						{
							if (this.menuItemState[i])
							{
								this.selectedIndex = i;
								break;
							}
						}
					}
				}
				if (this.selectedIndex != -2147483648 && !GameManager.Instance.IsPaused())
				{
					this.CallContextAction();
				}
				else
				{
					this.Close();
				}
			}
		}
		if (base.IsOpen && this.radialButtonPressed(playerInput))
		{
			this.CalculateSelectionFromController(playerInput);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool radialButtonPressed(PlayerActionsLocal _actionSet)
	{
		return _actionSet.Activate.IsPressed || _actionSet.PermanentActions.Activate.IsPressed || _actionSet.Reload.IsPressed || _actionSet.PermanentActions.Reload.IsPressed || _actionSet.ToggleFlashlight.IsPressed || _actionSet.PermanentActions.ToggleFlashlight.IsPressed || _actionSet.Inventory.IsPressed || _actionSet.VehicleActions.Inventory.IsPressed || _actionSet.PermanentActions.Inventory.IsPressed || _actionSet.Swap.IsPressed || _actionSet.PermanentActions.Swap.IsPressed || _actionSet.InventorySlotLeft.IsPressed || _actionSet.InventorySlotRight.IsPressed;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool radialButtonIsLeftSide(PlayerActionsLocal _actionSet)
	{
		return _actionSet.ToggleFlashlight.IsPressed || _actionSet.PermanentActions.ToggleFlashlight.IsPressed;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CallContextAction()
	{
		if (this.selectedIndex == -2147483648)
		{
			this.Close();
			return;
		}
		if (GameManager.Instance == null || GameManager.Instance.World == null)
		{
			this.Close();
			return;
		}
		if (this.commandHandler == null)
		{
			this.Close();
			return;
		}
		if (this.validCheckDelegate != null && !this.validCheckDelegate(this, this.context))
		{
			this.Close();
			return;
		}
		int commandIndex;
		if (this.selectedIndex < 0)
		{
			commandIndex = this.selectedIndex;
		}
		else
		{
			if (!this.menuItemState[this.selectedIndex])
			{
				this.Close();
				return;
			}
			commandIndex = this.menuItem[this.selectedIndex].CommandIndex;
		}
		XUiC_Radial.RadialContextAbs radialContextAbs = this.context;
		XUiC_Radial.CommandHandlerDelegate commandHandlerDelegate = this.commandHandler;
		this.Close();
		commandHandlerDelegate(this, commandIndex, radialContextAbs);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Close()
	{
		if (GameManager.Instance == null)
		{
			Log.Out("GetGameManager is null");
		}
		if (base.xui.playerUI.windowManager == null)
		{
			Log.Out("GetWindowManager is null");
		}
		this.isOpenRequested = false;
		base.xui.playerUI.windowManager.Close("radial");
	}

	public void ResetRadialEntries()
	{
		for (int i = 0; i < this.menuItemState.Length; i++)
		{
			this.menuItemState[i] = false;
			this.menuItem[i].SetHighlighted(false);
		}
	}

	public void SetCommonData(UIUtils.ButtonIcon _controllerButtonForSelect, XUiC_Radial.CommandHandlerDelegate _commandHandlerFunc, XUiC_Radial.RadialContextAbs _context = null, int _preSelectedCommandIndex = -1, bool _hasSpecialActionPriorToRadialVisibility = false, XUiC_Radial.RadialStillValidDelegate _radialValidityCallback = null)
	{
		this.updateRadialButtonPositions();
		int num = this.currentEnabledEntriesCount();
		this.context = _context;
		this.selectionControllerButton = _controllerButtonForSelect;
		this.commandHandler = _commandHandlerFunc;
		this.validCheckDelegate = _radialValidityCallback;
		this.hasSpecialActionPriorToRadialVisibility = _hasSpecialActionPriorToRadialVisibility;
		this.DefaultSelect();
		this.selectedIndex = int.MinValue;
		if (_preSelectedCommandIndex >= 0)
		{
			for (int i = 0; i < this.menuItem.Length; i++)
			{
				if (this.menuItemState[i] && this.menuItem[i].CommandIndex == _preSelectedCommandIndex)
				{
					this.selectedIndex = i;
				}
			}
		}
		if (num == 0 || num == 1)
		{
			this.selectedIndex = 0;
			if (this.hasSpecialActionPriorToRadialVisibility)
			{
				if (InputUtils.ShiftKeyPressed)
				{
					this.selectedIndex = -2;
				}
				else
				{
					this.selectedIndex = -1;
				}
			}
			if (num == 1 || this.selectedIndex < 0)
			{
				this.CallContextAction();
				return;
			}
			this.Close();
		}
		for (int j = 0; j < this.menuItem.Length; j++)
		{
			this.menuItem[j].ViewComponent.IsVisible = (j < num);
			this.menuItem[j].ViewComponent.Enabled = (j < num);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DefaultSelect()
	{
		this.SetHovered(null);
		this.ResetIconsScale();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CalculateSelectionFromController(PlayerActionsLocal _actionSet)
	{
		bool flag = base.xui.playerUI.entityPlayer != null && base.xui.playerUI.entityPlayer.AttachedToEntity is EntityVehicle;
		Vector2 vector = flag ? _actionSet.VehicleActions.Look.Value : _actionSet.Look.Value;
		if (vector == Vector2.zero)
		{
			vector = (flag ? _actionSet.VehicleActions.LeftStick.Value : _actionSet.Move.Value);
		}
		if (vector.magnitude < 0.75f)
		{
			return;
		}
		float num = 361f;
		XUiC_RadialEntry xuiC_RadialEntry = null;
		for (int i = 0; i < this.menuItem.Length; i++)
		{
			if (this.menuItemState[i])
			{
				XUiC_RadialEntry xuiC_RadialEntry2 = this.menuItem[i];
				Vector3 localPosition = xuiC_RadialEntry2.ViewComponent.UiTransform.localPosition;
				Vector2 to = new Vector2(localPosition.x, localPosition.y);
				float num2 = Vector2.Angle(vector, to);
				if (num2 < num)
				{
					num = num2;
					xuiC_RadialEntry = xuiC_RadialEntry2;
				}
			}
		}
		if (xuiC_RadialEntry != null)
		{
			this.SetHovered(xuiC_RadialEntry);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ResetIconsScale()
	{
		for (int i = 0; i < this.menuItem.Length; i++)
		{
			this.menuItem[i].ResetScale();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SelectionEffect(XUiC_RadialEntry _entry, bool _selected)
	{
		if (!_selected)
		{
			if (_entry != null)
			{
				_entry.SetScale(1f, false);
			}
			this.selectionText.Text = "";
			return;
		}
		if (_entry != null)
		{
			if (PlatformManager.NativePlatform.Input.CurrentInputStyle != PlayerInputManager.InputStyle.Keyboard)
			{
				_entry.ViewComponent.PlayHoverSound();
			}
			_entry.SetScale(1.5f, false);
			this.selectionText.Text = _entry.SelectionText;
			return;
		}
		this.selectionText.Text = "";
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetHovered(XUiC_RadialEntry _newSelected)
	{
		if (this.mCurrentlySelectedEntry != _newSelected)
		{
			this.SelectionEffect(this.mCurrentlySelectedEntry, false);
			this.selectedIndex = Array.IndexOf<XUiC_RadialEntry>(this.menuItem, _newSelected);
			this.SelectionEffect(this.mCurrentlySelectedEntry, true);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int currentEnabledEntriesCount()
	{
		int num = 0;
		for (int i = 0; i < this.menuItem.Length; i++)
		{
			if (this.menuItemState[i])
			{
				num++;
			}
		}
		return num;
	}

	public void CreateRadialEntry(int _commandIdx, string _icon, string _atlas = "UIAtlas", string _text = "", string _selectionText = "", bool _highlighted = false)
	{
		this.CreateRadialEntry(_commandIdx, _icon, Color.white, _atlas, _text, _selectionText, _highlighted);
	}

	public void CreateRadialEntry(int _commandIdx, string _icon, Color _iconColor, string _atlas = "UIAtlas", string _text = "", string _selectionText = "", bool _highlighted = false)
	{
		int num = this.currentEnabledEntriesCount();
		this.menuItemState[num] = true;
		XUiC_RadialEntry xuiC_RadialEntry = this.menuItem[num];
		xuiC_RadialEntry.CommandIndex = _commandIdx;
		xuiC_RadialEntry.SetAtlas(_atlas);
		xuiC_RadialEntry.SetSprite(_icon, _iconColor);
		xuiC_RadialEntry.SetText(_text);
		xuiC_RadialEntry.SetHighlighted(_highlighted);
		xuiC_RadialEntry.SelectionText = _selectionText;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void updateRadialButtonPositions()
	{
		int num = this.currentEnabledEntriesCount();
		int num2 = (num > 1) ? 50 : 0;
		float num3 = (float)Utils.FastMax(1, num);
		int num4 = 0;
		for (int i = 0; i < this.menuItem.Length; i++)
		{
			float f = -1.57079637f - 2f / num3 * (float)num4 * 3.14159274f;
			float x = ((float)(-(float)num) * 12.5f - (float)num2) * Mathf.Cos(f);
			float y = ((float)(-(float)num) * 12.5f - (float)num2) * Mathf.Sin(f);
			this.menuItem[i].ViewComponent.UiTransform.localPosition = new Vector3(x, y, 0f);
			num4++;
		}
	}

	public static bool RadialValidSameHoldingSlotIndex(XUiC_Radial _sender, XUiC_Radial.RadialContextAbs _context)
	{
		XUiC_Radial.RadialContextHoldingSlotIndex radialContextHoldingSlotIndex = _context as XUiC_Radial.RadialContextHoldingSlotIndex;
		return radialContextHoldingSlotIndex != null && _sender.xui.playerUI.entityPlayer.inventory.holdingItemIdx == radialContextHoldingSlotIndex.ItemSlotIndex;
	}

	public void SetCurrentBlockData(WorldBase _world, Vector3i _blockPos, int _cIdx, BlockValue _blockValue, EntityPlayerLocal _entityFocusing)
	{
		this.ResetRadialEntries();
		BlockActivationCommand[] blockActivationCommands = _blockValue.Block.GetBlockActivationCommands(_world, _blockValue, _cIdx, _blockPos, _entityFocusing);
		for (int i = 0; i < blockActivationCommands.Length; i++)
		{
			if (blockActivationCommands[i].enabled)
			{
				this.CreateRadialEntry(i, string.Format("ui_game_symbol_{0}", blockActivationCommands[i].icon), "UIAtlas", "", Localization.Get("blockcommand_" + blockActivationCommands[i].text, false), blockActivationCommands[i].highlighted);
			}
		}
		this.SetCommonData(UIUtils.GetButtonIconForAction(base.xui.playerUI.playerInput.Activate), new XUiC_Radial.CommandHandlerDelegate(this.handleBlockCommand), new XUiC_Radial.RadialContextBlock(_blockPos, _cIdx, _blockValue, _entityFocusing, blockActivationCommands), -1, false, null);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void handleBlockCommand(XUiC_Radial _sender, int _commandIndex, XUiC_Radial.RadialContextAbs _context)
	{
		XUiC_Radial.RadialContextBlock radialContextBlock = _context as XUiC_Radial.RadialContextBlock;
		if (radialContextBlock == null)
		{
			return;
		}
		radialContextBlock.BlockValue.Block.OnBlockActivated(radialContextBlock.Commands[_commandIndex].text, GameManager.Instance.World, radialContextBlock.ClusterIdx, radialContextBlock.BlockPos, radialContextBlock.BlockValue, radialContextBlock.EntityFocusing);
	}

	public void SetCurrentEntityData(WorldBase _world, Entity _entity, ITileEntity _te, EntityAlive _entityFocusing)
	{
		this.ResetRadialEntries();
		Vector3i vector3i = _te.ToWorldPos();
		EntityActivationCommand[] activationCommands = _entity.GetActivationCommands(vector3i, _entityFocusing);
		for (int i = 0; i < activationCommands.Length; i++)
		{
			if (activationCommands[i].enabled)
			{
				this.CreateRadialEntry(i, string.Format("ui_game_symbol_{0}", activationCommands[i].icon), "UIAtlas", "", Localization.Get("entitycommand_" + activationCommands[i].text, false), false);
			}
		}
		this.SetCommonData(UIUtils.GetButtonIconForAction(base.xui.playerUI.playerInput.Activate), new XUiC_Radial.CommandHandlerDelegate(this.handleEntityCommand), new XUiC_Radial.RadialContextEntity(vector3i, _entityFocusing, _entity), -1, false, null);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void handleEntityCommand(XUiC_Radial _sender, int _commandIndex, XUiC_Radial.RadialContextAbs _context)
	{
		XUiC_Radial.RadialContextEntity radialContextEntity = _context as XUiC_Radial.RadialContextEntity;
		if (radialContextEntity == null)
		{
			return;
		}
		radialContextEntity.EntityFocused.OnEntityActivated(_commandIndex, radialContextEntity.BlockPos, radialContextEntity.EntityFocusing);
	}

	public void SetActivatableItemData(EntityPlayerLocal _epl)
	{
		this.ResetRadialEntries();
		this.activatableItemPool.Clear();
		_epl.CollectActivatableItems(this.activatableItemPool);
		for (int i = 0; i < this.activatableItemPool.Count; i++)
		{
			this.CreateRadialEntry(i, this.activatableItemPool[i].ItemClass.GetIconName(), (this.activatableItemPool[i].Activated > 0) ? "ItemIconAtlas" : "ItemIconAtlasGreyscale", "", this.activatableItemPool[i].ItemClass.GetLocalizedItemName(), false);
		}
		this.SetCommonData(UIUtils.GetButtonIconForAction(_epl.playerInput.Activate), new XUiC_Radial.CommandHandlerDelegate(this.handleActivatableItemCommand), new XUiC_Radial.RadialContextHoldingSlotIndex(_epl.inventory.holdingItemIdx), -1, false, new XUiC_Radial.RadialStillValidDelegate(XUiC_Radial.RadialValidSameHoldingSlotIndex));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void handleActivatableItemCommand(XUiC_Radial _sender, int _commandIndex, XUiC_Radial.RadialContextAbs _context)
	{
		EntityPlayerLocal entityPlayer = _sender.xui.playerUI.entityPlayer;
		MinEventParams.CopyTo(entityPlayer.MinEventContext, MinEventParams.CachedEventParam);
		this.activatableItemPool.Clear();
		entityPlayer.CollectActivatableItems(this.activatableItemPool);
		if (this.activatableItemPool[_commandIndex].Activated == 0)
		{
			MinEventParams.CachedEventParam.ItemValue = this.activatableItemPool[_commandIndex];
			this.activatableItemPool[_commandIndex].FireEvent(MinEventTypes.onSelfItemActivate, MinEventParams.CachedEventParam);
			this.activatableItemPool[_commandIndex].Activated = 1;
			entityPlayer.bPlayerStatsChanged = true;
		}
		else
		{
			MinEventParams.CachedEventParam.ItemValue = this.activatableItemPool[_commandIndex];
			this.activatableItemPool[_commandIndex].FireEvent(MinEventTypes.onSelfItemDeactivate, MinEventParams.CachedEventParam);
			this.activatableItemPool[_commandIndex].Activated = 0;
			entityPlayer.bPlayerStatsChanged = true;
		}
		entityPlayer.inventory.CallOnToolbeltChangedInternal();
	}

	public void SetupMenuData()
	{
		this.ResetRadialEntries();
		bool flag = GameStats.GetBool(EnumGameStats.IsCreativeMenuEnabled) || GamePrefs.GetBool(EnumGamePrefs.CreativeMenuEnabled);
		this.CreateRadialEntry(0, "ui_game_symbol_hammer", "UIAtlas", "", Localization.Get("xuiWPcrafting", false), false);
		this.CreateRadialEntry(1, "ui_game_symbol_character", "UIAtlas", "", Localization.Get("xuiWPcharacter", false), false);
		this.CreateRadialEntry(2, "ui_game_symbol_map", "UIAtlas", "", Localization.Get("xuiWPmap", false), false);
		this.CreateRadialEntry(3, "ui_game_symbol_skills", "UIAtlas", "", Localization.Get("xuiWPskills", false), false);
		this.CreateRadialEntry(4, "ui_game_symbol_quest", "UIAtlas", "", Localization.Get("xuiWPquests", false), false);
		this.CreateRadialEntry(5, "ui_game_symbol_challenge", "UIAtlas", "", Localization.Get("xuiChallenges", false), false);
		this.CreateRadialEntry(6, "ui_game_symbol_players", "UIAtlas", "", Localization.Get("xuiWPplayers", false), false);
		if (flag)
		{
			this.CreateRadialEntry(7, "ui_game_symbol_lightbulb", "UIAtlas", "", Localization.Get("xuiWPcreative", false), false);
		}
		if (EntityDrone.DebugModeEnabled)
		{
			this.CreateRadialEntry(8, "ui_game_symbol_drone", Localization.Get("entityJunkDrone", false), "", "", false);
		}
		this.CreateRadialEntry(9, "ui_game_symbol_chat", "UIAtlas", "", Localization.Get("inpActChatName", false), false);
		this.SetCommonData(UIUtils.GetButtonIconForAction(base.xui.playerUI.playerInput.Inventory), new XUiC_Radial.CommandHandlerDelegate(this.handleMenuCommand), null, -1, false, null);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void handleMenuCommand(XUiC_Radial _sender, int _commandIndex, XUiC_Radial.RadialContextAbs _context)
	{
		EntityPlayerLocal entityPlayer = _sender.xui.playerUI.entityPlayer;
		switch (_commandIndex)
		{
		case 0:
			XUiC_WindowSelector.OpenSelectorAndWindow(entityPlayer, "crafting");
			return;
		case 1:
			XUiC_WindowSelector.OpenSelectorAndWindow(entityPlayer, "character");
			return;
		case 2:
			XUiC_WindowSelector.OpenSelectorAndWindow(entityPlayer, "map");
			return;
		case 3:
			XUiC_WindowSelector.OpenSelectorAndWindow(entityPlayer, "skills");
			return;
		case 4:
			XUiC_WindowSelector.OpenSelectorAndWindow(entityPlayer, "quests");
			return;
		case 5:
			XUiC_WindowSelector.OpenSelectorAndWindow(entityPlayer, "challenges");
			return;
		case 6:
			XUiC_WindowSelector.OpenSelectorAndWindow(entityPlayer, "players");
			return;
		case 7:
			XUiC_WindowSelector.OpenSelectorAndWindow(entityPlayer, "creative");
			return;
		case 8:
			if (entityPlayer.OwnedEntityCount > 0)
			{
				OwnedEntityData ownedEntityData = entityPlayer.GetOwnedEntities()[0];
				EntityDrone entityDrone = GameManager.Instance.World.GetEntity(ownedEntityData.Id) as EntityDrone;
				if (entityDrone)
				{
					entityDrone.startDialog(entityPlayer);
					return;
				}
			}
			break;
		case 9:
			base.xui.playerUI.windowManager.Open(XUiC_Chat.ID, true, false, true);
			return;
		default:
			XUiC_WindowSelector.OpenSelectorAndWindow(entityPlayer, "crafting");
			break;
		}
	}

	public void SetupToolbeltMenu(int _direction)
	{
		this.toolbeltSwapDirection = _direction;
		EntityPlayerLocal entityPlayer = base.xui.playerUI.entityPlayer;
		this.ResetRadialEntries();
		ItemStack[] slots = entityPlayer.inventory.GetSlots();
		for (int i = 0; i < Mathf.Min(slots.Length, this.menuItem.Length); i++)
		{
			if (i != entityPlayer.inventory.DUMMY_SLOT_IDX)
			{
				if (slots[i].IsEmpty())
				{
					this.CreateRadialEntry(i, "", "UIAtlas", "", "", false);
				}
				else
				{
					this.CreateRadialEntry(i, slots[i].itemValue.ItemClass.GetIconName(), slots[i].itemValue.ItemClass.GetIconTint(slots[i].itemValue), "ItemIconAtlas", slots[i].itemValue.ItemClass.CanStack() ? slots[i].count.ToString() : "", slots[i].itemValue.ItemClass.GetLocalizedItemName(), true);
				}
			}
		}
		this.SetCommonData(UIUtils.GetButtonIconForAction(base.xui.playerUI.playerInput.Swap), new XUiC_Radial.CommandHandlerDelegate(this.HandleToolbeltCommand), null, -1, false, null);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleToolbeltCommand(XUiC_Radial _sender, int _commandIndex, XUiC_Radial.RadialContextAbs _context)
	{
		if (_sender.mCurrentlySelectedEntry != null && Time.time - this.openTime >= 0.4f)
		{
			if (_commandIndex >= 0 && base.xui.playerUI.entityPlayer.inventory.holdingItemIdx != _commandIndex)
			{
				base.xui.playerUI.entityPlayer.inventory.SetHoldingItemIdx(_commandIndex);
			}
			return;
		}
		EntityPlayerLocal entityPlayer = base.xui.playerUI.entityPlayer;
		if (this.toolbeltSwapDirection == 0)
		{
			int bestQuickSwapSlot = entityPlayer.inventory.GetBestQuickSwapSlot();
			entityPlayer.MoveController.SetInventoryIdxFromScroll(bestQuickSwapSlot);
			return;
		}
		int num = entityPlayer.inventory.GetFocusedItemIdx();
		if (this.toolbeltSwapDirection < 0)
		{
			num--;
			if (num < 0)
			{
				num = entityPlayer.inventory.PUBLIC_SLOTS - 1;
			}
		}
		else
		{
			num++;
			if (num >= entityPlayer.inventory.PUBLIC_SLOTS)
			{
				num = 0;
			}
		}
		entityPlayer.MoveController.SetInventoryIdxFromScroll(num);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool getBasicBlockInfo(out EntityPlayerLocal _epl, out ItemClassBlock.ItemBlockInventoryData _ibid, out Block _blockHolding, out Block _blockSelectedShape, out bool _hasAutoRotation, out bool _onlySimpleRotations, out bool _hasCopyRotation, out bool _allowShapes, out bool _hasCopyAutoShape, out bool _hasCopyShapeLegacy, out bool _allowPainting)
	{
		_hasAutoRotation = false;
		_onlySimpleRotations = false;
		_hasCopyRotation = false;
		_hasCopyAutoShape = false;
		_hasCopyShapeLegacy = false;
		_allowPainting = false;
		_epl = base.xui.playerUI.entityPlayer;
		Inventory inventory = _epl.inventory;
		_blockHolding = inventory.GetHoldingBlock().GetBlock();
		_ibid = (inventory.holdingItemData as ItemClassBlock.ItemBlockInventoryData);
		_allowShapes = _blockHolding.SelectAlternates;
		_blockSelectedShape = null;
		if (_blockHolding == null || _ibid == null)
		{
			return false;
		}
		_blockSelectedShape = (_allowShapes ? _blockHolding.GetAltBlock(_ibid.itemValue.Meta) : _blockHolding);
		_hasAutoRotation = (_blockSelectedShape.BlockPlacementHelper != BlockPlacement.None);
		_onlySimpleRotations = ((_blockSelectedShape.AllowedRotations & EBlockRotationClasses.Advanced) == EBlockRotationClasses.None);
		_hasCopyRotation = (_epl.HitInfo.bHitValid && !_epl.HitInfo.hit.blockValue.isair && _blockSelectedShape.SupportsRotation(_epl.HitInfo.hit.blockValue.rotation));
		if (_allowShapes && _epl.HitInfo.bHitValid)
		{
			Block block = _epl.HitInfo.hit.blockValue.Block;
			if (block.GetAutoShapeType() != EAutoShapeType.None && _blockHolding.AutoShapeSupportsShapeName(block.GetAutoShapeShapeName()))
			{
				_hasCopyAutoShape = true;
			}
			else if (_blockHolding.ContainsAlternateBlock(block.GetBlockName()))
			{
				_hasCopyShapeLegacy = true;
			}
		}
		_allowPainting = (_blockSelectedShape.shape is BlockShapeNew && _blockSelectedShape.MeshIndex == 0);
		return true;
	}

	public void SetupBlockShapeData()
	{
		this.ResetRadialEntries();
		EntityPlayerLocal entityPlayerLocal;
		ItemClassBlock.ItemBlockInventoryData itemBlockInventoryData;
		Block block;
		Block block2;
		bool flag;
		bool flag2;
		bool flag3;
		bool flag4;
		bool flag5;
		bool flag6;
		bool flag7;
		if (!this.getBasicBlockInfo(out entityPlayerLocal, out itemBlockInventoryData, out block, out block2, out flag, out flag2, out flag3, out flag4, out flag5, out flag6, out flag7))
		{
			this.SetCommonData(UIUtils.GetButtonIconForAction(base.xui.playerUI.playerInput.Activate), new XUiC_Radial.CommandHandlerDelegate(this.handleBlockShapeCommand), null, -1, false, null);
			return;
		}
		if (flag4)
		{
			this.CreateRadialEntry(0, "ui_game_symbol_all_blocks", "UIAtlas", "", Localization.Get("xuiShape", false), false);
			if (flag5 || flag6)
			{
				this.CreateRadialEntry(1, "ui_game_symbol_copy_shape", "UIAtlas", "", Localization.Get("xuiCopyShape", false), false);
				if (!flag2 && flag3)
				{
					this.CreateRadialEntry(8, "ui_game_symbol_copy_shape_and_rotation", "UIAtlas", "", Localization.Get("xuiCopyShapeAndRotation", false), false);
				}
			}
		}
		this.CreateRadialEntry(2, "ui_game_symbol_rotate_simple", "UIAtlas", "", Localization.Get("xuiSimpleRotation", false), false);
		if (!flag2)
		{
			this.CreateRadialEntry(3, "ui_game_symbol_rotate_advanced", "UIAtlas", "", Localization.Get("xuiAdvancedRotation", false), false);
			this.CreateRadialEntry(4, "ui_game_symbol_rotate_on_face", "UIAtlas", "", Localization.Get("xuiOnFaceRotation", false), false);
			if (flag)
			{
				this.CreateRadialEntry(5, "ui_game_symbol_rotate_auto", "UIAtlas", "", Localization.Get("xuiAutoRotation", false), false);
			}
			if (flag3)
			{
				this.CreateRadialEntry(6, "ui_game_symbol_paint_copy_block", "UIAtlas", "", Localization.Get("xuiCopyRotation", false), false);
			}
		}
		if (flag7 && (GameStats.GetBool(EnumGameStats.IsCreativeMenuEnabled) || GamePrefs.GetBool(EnumGamePrefs.CreativeMenuEnabled)))
		{
			this.CreateRadialEntry(7, "ui_game_symbol_paint_bucket", "UIAtlas", "", Localization.Get("xuiMaterials", false), false);
		}
		this.SetCommonData(UIUtils.GetButtonIconForAction(base.xui.playerUI.playerInput.Activate), new XUiC_Radial.CommandHandlerDelegate(this.handleBlockShapeCommand), new XUiC_Radial.RadialContextHoldingSlotIndex(entityPlayerLocal.inventory.holdingItemIdx), -1, true, new XUiC_Radial.RadialStillValidDelegate(XUiC_Radial.RadialValidSameHoldingSlotIndex));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void handleBlockShapeCommand(XUiC_Radial _sender, int _commandIndex, XUiC_Radial.RadialContextAbs _context)
	{
		EntityPlayerLocal entityPlayerLocal;
		ItemClassBlock.ItemBlockInventoryData itemBlockInventoryData;
		Block block;
		Block block2;
		bool flag;
		bool flag2;
		bool flag3;
		bool flag4;
		bool flag5;
		bool flag6;
		bool flag7;
		if (!this.getBasicBlockInfo(out entityPlayerLocal, out itemBlockInventoryData, out block, out block2, out flag, out flag2, out flag3, out flag4, out flag5, out flag6, out flag7))
		{
			return;
		}
		switch (_commandIndex)
		{
		case -2:
		case 0:
			if (flag4)
			{
				base.xui.GetChildByType<XUiC_ShapesWindow>().ItemValue = entityPlayerLocal.inventory.holdingItemItemValue.Clone();
				base.xui.playerUI.windowManager.Open("shapes", true, false, true);
				return;
			}
			break;
		case -1:
			block2.RotateHoldingBlock(itemBlockInventoryData, false, true);
			return;
		case 1:
			if (flag5 || flag6)
			{
				Block block3 = entityPlayerLocal.HitInfo.hit.blockValue.Block;
				this.<handleBlockShapeCommand>g__copyShape|60_0(entityPlayerLocal, flag5, block, block3, itemBlockInventoryData);
				return;
			}
			break;
		case 2:
			itemBlockInventoryData.mode = BlockPlacement.EnumRotationMode.Simple;
			return;
		case 3:
			if (!flag2)
			{
				itemBlockInventoryData.mode = BlockPlacement.EnumRotationMode.Advanced;
				return;
			}
			break;
		case 4:
			if (!flag2)
			{
				itemBlockInventoryData.mode = BlockPlacement.EnumRotationMode.ToFace;
				return;
			}
			break;
		case 5:
			if (!flag2 && flag)
			{
				itemBlockInventoryData.mode = BlockPlacement.EnumRotationMode.Auto;
				return;
			}
			break;
		case 6:
			if (!flag2 && flag3)
			{
				BlockValue blockValue = entityPlayerLocal.HitInfo.hit.blockValue;
				if (blockValue.ischild)
				{
					blockValue = entityPlayerLocal.world.GetBlock(blockValue.Block.multiBlockPos.GetParentPos(entityPlayerLocal.HitInfo.hit.blockPos, blockValue));
				}
				XUiC_Radial.<handleBlockShapeCommand>g__copyRotation|60_1(itemBlockInventoryData, blockValue);
				return;
			}
			break;
		case 7:
			base.xui.playerUI.windowManager.Open("materials", true, false, true);
			return;
		case 8:
			if (!flag2 && flag3 && (flag5 || flag6))
			{
				BlockValue blockValue2 = entityPlayerLocal.HitInfo.hit.blockValue;
				if (blockValue2.ischild)
				{
					blockValue2 = entityPlayerLocal.world.GetBlock(blockValue2.Block.multiBlockPos.GetParentPos(entityPlayerLocal.HitInfo.hit.blockPos, blockValue2));
				}
				this.<handleBlockShapeCommand>g__copyShape|60_0(entityPlayerLocal, flag5, block, blockValue2.Block, itemBlockInventoryData);
				XUiC_Radial.<handleBlockShapeCommand>g__copyRotation|60_1(itemBlockInventoryData, blockValue2);
			}
			break;
		default:
			return;
		}
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Private)]
	public void <handleBlockShapeCommand>g__copyShape|60_0(EntityPlayerLocal _epl, bool _hasCopyAutoShape, Block _block, Block _targetedBlock, ItemClassBlock.ItemBlockInventoryData _ibid)
	{
		int num = _hasCopyAutoShape ? _block.AutoShapeAlternateShapeNameIndex(_targetedBlock.GetAutoShapeShapeName()) : _block.GetAlternateBlockIndex(_targetedBlock.GetBlockName());
		if (num >= 0)
		{
			_ibid.itemValue.Meta = num;
			XUiC_Toolbelt childByType = ((XUiWindowGroup)base.xui.playerUI.windowManager.GetWindow("toolbelt")).Controller.GetChildByType<XUiC_Toolbelt>();
			if (childByType != null)
			{
				int holdingItemIdx = _epl.inventory.holdingItemIdx;
				XUiC_ItemStack slotControl = childByType.GetSlotControl(holdingItemIdx);
				slotControl.ItemStack = new ItemStack(_ibid.itemValue, _ibid.itemStack.count);
				slotControl.ForceRefreshItemStack();
			}
		}
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Internal)]
	public static void <handleBlockShapeCommand>g__copyRotation|60_1(ItemClassBlock.ItemBlockInventoryData _ibid, BlockValue _bvLookingAt)
	{
		_ibid.rotation = _bvLookingAt.rotation;
		_ibid.mode = BlockPlacement.EnumRotationMode.Advanced;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_RadialEntry[] menuItem;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool[] menuItemState;

	[PublicizedFrom(EAccessModifier.Private)]
	public int selectedIndex;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool hasSpecialActionPriorToRadialVisibility;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_Radial.RadialContextAbs context;

	[PublicizedFrom(EAccessModifier.Private)]
	public UIUtils.ButtonIcon selectionControllerButton;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_Radial.CommandHandlerDelegate commandHandler;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_Radial.RadialStillValidDelegate validCheckDelegate;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isOpenRequested;

	[PublicizedFrom(EAccessModifier.Private)]
	public float displayDelay = 0.25f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float openTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cRadialSelectedScale = 1.5f;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool showingCallouts;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label selectionText;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<ItemValue> activatableItemPool = new List<ItemValue>();

	[PublicizedFrom(EAccessModifier.Private)]
	public int toolbeltSwapDirection;

	public delegate void CommandHandlerDelegate(XUiC_Radial _xuiRadial, int _commandIndex, XUiC_Radial.RadialContextAbs _context);

	public delegate bool RadialStillValidDelegate(XUiC_Radial _xuiRadial, XUiC_Radial.RadialContextAbs _context);

	public class RadialContextAbs
	{
	}

	public class RadialContextHoldingSlotIndex : XUiC_Radial.RadialContextAbs
	{
		public RadialContextHoldingSlotIndex(int _itemSlotIndex)
		{
			this.ItemSlotIndex = _itemSlotIndex;
		}

		public readonly int ItemSlotIndex;
	}

	public class RadialContextBlock : XUiC_Radial.RadialContextAbs
	{
		public RadialContextBlock(Vector3i _blockPos, int _clusterIdx, BlockValue _blockValue, EntityPlayerLocal _entityFocusing, BlockActivationCommand[] _commands)
		{
			this.BlockPos = _blockPos;
			this.ClusterIdx = _clusterIdx;
			this.BlockValue = _blockValue;
			this.EntityFocusing = _entityFocusing;
			this.Commands = _commands;
		}

		public readonly Vector3i BlockPos;

		public readonly int ClusterIdx;

		public readonly BlockValue BlockValue;

		public readonly EntityPlayerLocal EntityFocusing;

		public readonly BlockActivationCommand[] Commands;
	}

	public class RadialContextEntity : XUiC_Radial.RadialContextAbs
	{
		public RadialContextEntity(Vector3i _blockPos, EntityAlive _entityFocusing, Entity _entityFocused)
		{
			this.BlockPos = _blockPos;
			this.EntityFocusing = _entityFocusing;
			this.EntityFocused = _entityFocused;
		}

		public readonly Vector3i BlockPos;

		public readonly EntityAlive EntityFocusing;

		public readonly Entity EntityFocused;
	}
}
