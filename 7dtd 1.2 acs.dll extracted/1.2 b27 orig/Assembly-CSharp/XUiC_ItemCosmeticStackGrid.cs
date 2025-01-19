using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ItemCosmeticStackGrid : XUiController
{
	public virtual XUiC_ItemStack.StackLocationTypes StackLocation
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return XUiC_ItemStack.StackLocationTypes.Backpack;
		}
	}

	public ItemStack CurrentItem { get; set; }

	public XUiC_AssembleWindow AssembleWindow { get; set; }

	public override void Init()
	{
		base.Init();
		XUiController[] childrenByType = base.GetChildrenByType<XUiC_ItemCosmeticStack>(null);
		this.itemControllers = childrenByType;
		this.IsDirty = false;
	}

	public override void Update(float _dt)
	{
		if (GameManager.Instance == null && GameManager.Instance.World == null)
		{
			return;
		}
		base.Update(_dt);
	}

	public void SetParts(ItemValue[] stackList)
	{
		if (stackList == null)
		{
			return;
		}
		this.currentItemClass = this.CurrentItem.itemValue.ItemClass;
		XUiC_ItemInfoWindow childByType = base.xui.GetChildByType<XUiC_ItemInfoWindow>();
		for (int i = 0; i < this.itemControllers.Length; i++)
		{
			XUiC_ItemCosmeticStack xuiC_ItemCosmeticStack = (XUiC_ItemCosmeticStack)this.itemControllers[i];
			if (i < this.CurrentItem.itemValue.CosmeticMods.Length)
			{
				ItemValue itemValue = this.CurrentItem.itemValue.CosmeticMods[i];
				if (itemValue != null && itemValue.ItemClass is ItemClassModifier)
				{
					xuiC_ItemCosmeticStack.SlotType = (itemValue.ItemClass as ItemClassModifier).Type.ToStringCached<ItemClassModifier.ModifierTypes>().ToLower();
				}
				xuiC_ItemCosmeticStack.SlotChangedEvent -= this.HandleSlotChangedEvent;
				xuiC_ItemCosmeticStack.ItemValue = ((itemValue != null) ? itemValue : ItemValue.None.Clone());
				xuiC_ItemCosmeticStack.SlotChangedEvent += this.HandleSlotChangedEvent;
				xuiC_ItemCosmeticStack.SlotNumber = i;
				xuiC_ItemCosmeticStack.InfoWindow = childByType;
				xuiC_ItemCosmeticStack.StackLocation = this.StackLocation;
				xuiC_ItemCosmeticStack.ViewComponent.IsVisible = true;
			}
			else
			{
				xuiC_ItemCosmeticStack.ViewComponent.IsVisible = false;
			}
		}
	}

	public void HandleSlotChangedEvent(int slotNumber, ItemStack stack)
	{
		XUiC_ItemCosmeticStack xuiC_ItemCosmeticStack = (XUiC_ItemCosmeticStack)this.itemControllers[slotNumber];
		ItemValue itemValue = xuiC_ItemCosmeticStack.ItemStack.IsEmpty() ? ItemValue.None.Clone() : xuiC_ItemCosmeticStack.ItemStack.itemValue;
		if (itemValue.ItemClass != null)
		{
			if (!itemValue.ItemClass.ItemTags.Test_AnySet(ItemClassModifier.CosmeticModTypes) && this.CurrentItem.itemValue.Modifications.Length != 0)
			{
				for (int i = 0; i < this.CurrentItem.itemValue.Modifications.Length; i++)
				{
					ItemValue itemValue2 = this.CurrentItem.itemValue.Modifications[i];
					if (itemValue2 == null || itemValue2.IsEmpty())
					{
						this.CurrentItem.itemValue.Modifications[i] = itemValue;
						break;
					}
				}
			}
			else
			{
				this.CurrentItem.itemValue.CosmeticMods[slotNumber] = itemValue;
			}
		}
		else
		{
			this.CurrentItem.itemValue.CosmeticMods[slotNumber] = itemValue;
		}
		this.AssembleWindow.ItemStack = this.CurrentItem;
		this.AssembleWindow.OnChanged();
		base.xui.AssembleItem.RefreshAssembleItem();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void UpdateBackend(ItemStack[] stackList)
	{
	}

	public override void OnOpen()
	{
		if (base.ViewComponent != null && !base.ViewComponent.IsVisible)
		{
			base.ViewComponent.IsVisible = true;
		}
		this.IsDirty = true;
	}

	public override void OnClose()
	{
		if (base.ViewComponent != null && base.ViewComponent.IsVisible)
		{
			base.ViewComponent.IsVisible = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public int curPageIdx;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int numPages;

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiController[] itemControllers;

	[PublicizedFrom(EAccessModifier.Protected)]
	public ItemStack[] items;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemClass currentItemClass;
}
