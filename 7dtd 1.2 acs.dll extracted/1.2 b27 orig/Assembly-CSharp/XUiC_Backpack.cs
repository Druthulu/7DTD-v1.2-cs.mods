using System;
using GUI_2;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_Backpack : XUiC_ItemStackGrid
{
	public override XUiC_ItemStack.StackLocationTypes StackLocation
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return XUiC_ItemStack.StackLocationTypes.Backpack;
		}
	}

	public void RefreshBackpackSlots()
	{
		this.SetStacks(this.GetSlots());
		this.IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void PlayerInventory_OnBackpackItemsChanged()
	{
		this.RefreshBackpackSlots();
	}

	public override ItemStack[] GetSlots()
	{
		return base.xui.PlayerInventory.GetBackpackItemStacks();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void UpdateBackend(ItemStack[] stackList)
	{
		base.xui.PlayerInventory.SetBackpackItemStacks(stackList);
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.wasHoldingItem = !base.xui.dragAndDrop.CurrentStack.IsEmpty();
		this.UpdateCallouts(this.wasHoldingItem);
		base.xui.calloutWindow.EnableCallouts(XUiC_GamepadCalloutWindow.CalloutType.Menu, 0f);
		if (EffectManager.GetValue(PassiveEffects.ShuffledBackpack, null, 0f, base.xui.playerUI.entityPlayer, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false) > 0f)
		{
			ItemStack[] slots = this.GetSlots();
			GameRandom rand = base.xui.playerUI.entityPlayer.rand;
			for (int i = 0; i < slots.Length * 2; i++)
			{
				int num = rand.RandomRange(slots.Length);
				int num2 = rand.RandomRange(slots.Length);
				if (!this.itemControllers[num].StackLock && !this.itemControllers[num2].StackLock)
				{
					ItemStack itemStack = slots[num];
					slots[num] = slots[num2];
					slots[num2] = itemStack;
				}
			}
			this.SetStacks(slots);
		}
		else
		{
			this.SetStacks(this.GetSlots());
		}
		base.xui.PlayerInventory.OnBackpackItemsChanged += this.PlayerInventory_OnBackpackItemsChanged;
	}

	public override void OnClose()
	{
		base.OnClose();
		base.xui.calloutWindow.DisableCallouts(XUiC_GamepadCalloutWindow.CalloutType.Menu);
		base.xui.PlayerInventory.OnBackpackItemsChanged -= this.PlayerInventory_OnBackpackItemsChanged;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		base.xui.playerUI.entityPlayer.AimingGun = false;
		if (base.ViewComponent.IsVisible)
		{
			bool flag = !base.xui.dragAndDrop.CurrentStack.IsEmpty();
			if (flag != this.wasHoldingItem)
			{
				this.wasHoldingItem = flag;
				this.UpdateCallouts(flag);
			}
		}
		if (this.maxSlotCount != base.xui.playerUI.entityPlayer.bag.MaxItemCount)
		{
			this.RefreshBackpackSlots();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateCallouts(bool _holdingItem)
	{
		string action = string.Format(Localization.Get("igcoItemActions", false), InControlExtensions.GetBlankDPadSourceString());
		XUiC_GamepadCalloutWindow calloutWindow = base.xui.calloutWindow;
		if (calloutWindow != null)
		{
			calloutWindow.ClearCallouts(XUiC_GamepadCalloutWindow.CalloutType.Menu);
			if (!_holdingItem)
			{
				calloutWindow.AddCallout(UIUtils.ButtonIcon.FaceButtonNorth, "igcoInspect", XUiC_GamepadCalloutWindow.CalloutType.Menu);
				calloutWindow.AddCallout(UIUtils.ButtonIcon.FaceButtonNorth, action, XUiC_GamepadCalloutWindow.CalloutType.Menu);
				calloutWindow.AddCallout(UIUtils.ButtonIcon.FaceButtonSouth, "igcoSelect", XUiC_GamepadCalloutWindow.CalloutType.Menu);
			}
			calloutWindow.AddCallout(UIUtils.ButtonIcon.FaceButtonEast, "igcoExit", XUiC_GamepadCalloutWindow.CalloutType.Menu);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void SetStacks(ItemStack[] stackList)
	{
		if (stackList == null)
		{
			return;
		}
		this.maxSlotCount = base.xui.playerUI.entityPlayer.bag.MaxItemCount;
		XUiC_ItemInfoWindow childByType = base.xui.GetChildByType<XUiC_ItemInfoWindow>();
		int num = 0;
		int num2 = 0;
		while (num2 < stackList.Length && this.itemControllers.Length > num2 && stackList.Length > num2)
		{
			num = num2;
			XUiC_ItemStack xuiC_ItemStack = this.itemControllers[num2];
			xuiC_ItemStack.SlotChangedEvent -= this.handleSlotChangedDelegate;
			xuiC_ItemStack.ItemStack = stackList[num2].Clone();
			xuiC_ItemStack.SlotChangedEvent += this.handleSlotChangedDelegate;
			xuiC_ItemStack.SlotNumber = num2;
			xuiC_ItemStack.InfoWindow = childByType;
			xuiC_ItemStack.StackLocation = this.StackLocation;
			bool flag = num2 >= this.maxSlotCount;
			if (xuiC_ItemStack.AttributeLock != flag)
			{
				xuiC_ItemStack.AttributeLock = flag;
			}
			num2++;
		}
		for (int i = num; i < this.itemControllers.Length; i++)
		{
			XUiC_ItemStack xuiC_ItemStack2 = this.itemControllers[i];
			bool flag2 = i >= this.maxSlotCount;
			if (xuiC_ItemStack2.AttributeLock != flag2)
			{
				xuiC_ItemStack2.AttributeLock = flag2;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool wasHoldingItem;

	[PublicizedFrom(EAccessModifier.Private)]
	public int maxSlotCount;
}
