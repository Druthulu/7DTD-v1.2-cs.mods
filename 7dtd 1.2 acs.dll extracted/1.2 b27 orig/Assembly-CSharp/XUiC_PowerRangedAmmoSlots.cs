using System;
using Audio;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_PowerRangedAmmoSlots : XUiC_ItemStackGrid
{
	public TileEntityPoweredRangedTrap TileEntity
	{
		get
		{
			return this.tileEntity;
		}
		set
		{
			this.tileEntity = value;
			this.SetSlots(this.tileEntity.ItemSlots);
		}
	}

	public override void HandleSlotChangedEvent(int slotNumber, ItemStack stack)
	{
		base.HandleSlotChangedEvent(slotNumber, stack);
		this.tileEntity.SetSendSlots();
		this.tileEntity.SetModified();
	}

	public override void Init()
	{
		base.Init();
		this.btnOn = this.windowGroup.Controller.GetChildById("windowPowerTrapSlots").GetChildById("btnOn");
		this.btnOn_Background = (XUiV_Button)this.btnOn.GetChildById("clickable").ViewComponent;
		this.btnOn_Background.Controller.OnPress += this.btnOn_OnPress;
		XUiController childById = this.btnOn.GetChildById("lblOnOff");
		if (childById != null)
		{
			this.lblOnOff = (XUiV_Label)childById.ViewComponent;
		}
		childById = this.btnOn.GetChildById("sprOnOff");
		if (childById != null)
		{
			this.sprOnOff = (XUiV_Sprite)childById.ViewComponent;
		}
		this.isDirty = true;
		this.turnOff = Localization.Get("xuiUnlockAmmo", false);
		this.turnOn = Localization.Get("xuiLockAmmo", false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void btnOn_OnPress(XUiController _sender, int _mouseButton)
	{
		bool flag = false;
		for (int i = 0; i < this.TileEntity.ItemSlots.Length; i++)
		{
			if (!this.TileEntity.ItemSlots[i].IsEmpty())
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			Manager.PlayInsidePlayerHead("ui_denied", -1, 0f, false, false);
			GameManager.ShowTooltip(base.xui.playerUI.localPlayer.entityPlayerLocal, Localization.Get("ttRequiresAmmo", false), false);
			return;
		}
		this.tileEntity.IsLocked = !this.tileEntity.IsLocked;
		this.tileEntity.SetModified();
		this.RefreshIsLocked(this.tileEntity.IsLocked);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetRequirements()
	{
		for (int i = 0; i < this.itemControllers.Length; i++)
		{
			XUiC_RequiredItemStack xuiC_RequiredItemStack = this.itemControllers[i] as XUiC_RequiredItemStack;
			if (xuiC_RequiredItemStack != null)
			{
				xuiC_RequiredItemStack.RequiredType = XUiC_RequiredItemStack.RequiredTypes.ItemClass;
				xuiC_RequiredItemStack.RequiredItemClass = this.tileEntity.AmmoItem;
			}
		}
	}

	public override XUiC_ItemStack.StackLocationTypes StackLocation
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return XUiC_ItemStack.StackLocationTypes.Workstation;
		}
	}

	public XUiC_PowerRangedTrapWindowGroup Owner { get; set; }

	public virtual void SetSlots(ItemStack[] stacks)
	{
		this.items = stacks;
		base.SetStacks(stacks);
	}

	public virtual bool HasRequirement(Recipe recipe)
	{
		return true;
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.tileEntity.SetUserAccessing(true);
		if (base.ViewComponent != null && !base.ViewComponent.IsVisible)
		{
			base.ViewComponent.OnOpen();
			base.ViewComponent.IsVisible = true;
		}
		this.IsDirty = true;
		this.SetRequirements();
		bool isLocked = this.tileEntity.IsLocked;
		this.RefreshIsLocked(isLocked);
		base.xui.powerAmmoSlots = this;
		XUiC_PowerRangedAmmoSlots.Current = this;
		this.IsDormant = false;
	}

	public override void OnClose()
	{
		base.OnClose();
		GameManager instance = GameManager.Instance;
		if (base.ViewComponent != null && base.ViewComponent.IsVisible)
		{
			base.ViewComponent.OnClose();
			base.ViewComponent.IsVisible = false;
		}
		Vector3i blockPos = this.tileEntity.ToWorldPos();
		if (!XUiC_CameraWindow.hackyIsOpeningMaximizedWindow)
		{
			this.tileEntity.SetUserAccessing(false);
			instance.TEUnlockServer(this.tileEntity.GetClrIdx(), blockPos, this.tileEntity.entityId, true);
			this.tileEntity.SetModified();
		}
		this.IsDirty = true;
		this.tileEntity = null;
		XUiC_PowerRangedAmmoSlots.Current = (base.xui.powerAmmoSlots = null);
		this.IsDormant = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RefreshIsLocked(bool isOn)
	{
		if (isOn)
		{
			this.lblOnOff.Text = this.turnOff;
			if (this.sprOnOff != null)
			{
				this.sprOnOff.Color = this.onColor;
				this.sprOnOff.SpriteName = "ui_game_symbol_lock";
			}
		}
		else
		{
			this.lblOnOff.Text = this.turnOn;
			if (this.sprOnOff != null)
			{
				this.sprOnOff.Color = this.offColor;
				this.sprOnOff.SpriteName = "ui_game_symbol_unlock";
			}
		}
		for (int i = 0; i < this.itemControllers.Length; i++)
		{
			XUiC_RequiredItemStack xuiC_RequiredItemStack = this.itemControllers[i] as XUiC_RequiredItemStack;
			if (xuiC_RequiredItemStack != null)
			{
				xuiC_RequiredItemStack.ToolLock = isOn;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public void SetLocked(bool isOn)
	{
		for (int i = 0; i < this.itemControllers.Length; i++)
		{
			XUiC_RequiredItemStack xuiC_RequiredItemStack = this.itemControllers[i] as XUiC_RequiredItemStack;
			if (xuiC_RequiredItemStack != null)
			{
				xuiC_RequiredItemStack.ToolLock = isOn;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void UpdateBackend(ItemStack[] stackList)
	{
		base.UpdateBackend(stackList);
		this.tileEntity.ItemSlots = stackList;
		this.tileEntity.SetSendSlots();
		this.windowGroup.Controller.SetAllChildrenDirty(false);
	}

	public override void Update(float _dt)
	{
		if (GameManager.Instance == null && GameManager.Instance.World == null)
		{
			return;
		}
		if (this.tileEntity == null)
		{
			return;
		}
		base.Update(_dt);
		if (this.lastLocked != this.tileEntity.IsLocked)
		{
			this.lastLocked = this.tileEntity.IsLocked;
			this.RefreshIsLocked(this.tileEntity.IsLocked);
		}
		if (this.tileEntity.IsLocked)
		{
			this.SetSlots(this.tileEntity.ItemSlots);
		}
		base.RefreshBindings(false);
	}

	public void Refresh()
	{
		this.SetSlots(this.tileEntity.ItemSlots);
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public bool TryAddItemToSlot(ItemClass itemClass, ItemStack itemStack)
	{
		if (itemClass != this.tileEntity.AmmoItem)
		{
			return false;
		}
		this.tileEntity.TryStackItem(itemStack);
		this.SetSlots(this.tileEntity.ItemSlots);
		return itemStack.count == 0;
	}

	public static XUiC_PowerRangedAmmoSlots Current;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController btnOn;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Button btnOn_Background;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label lblOnOff;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite sprOnOff;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color32 onColor = new Color32(250, byte.MaxValue, 163, byte.MaxValue);

	[PublicizedFrom(EAccessModifier.Private)]
	public Color32 offColor = Color.white;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool lastLocked;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isDirty;

	[PublicizedFrom(EAccessModifier.Private)]
	public string turnOff;

	[PublicizedFrom(EAccessModifier.Private)]
	public string turnOn;

	[PublicizedFrom(EAccessModifier.Private)]
	public TileEntityPoweredRangedTrap tileEntity;
}
