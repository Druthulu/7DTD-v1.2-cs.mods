using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ItemPartStack : XUiC_BasePartStack
{
	public ItemValue ItemValue
	{
		get
		{
			return this.itemValue;
		}
		set
		{
			this.itemValue = value;
			base.ItemStack = new ItemStack(value, 1);
		}
	}

	public ItemClass ExpectedItemClass
	{
		get
		{
			return this.expectedItemClass;
		}
		set
		{
			this.expectedItemClass = value;
			this.SetEmptySpriteName();
		}
	}

	public override void Init()
	{
		base.Init();
		this.lblStackMissing = Localization.Get("lblPartStackMissing", false);
	}

	public override string GetAtlas()
	{
		if (base.ItemStack.IsEmpty())
		{
			return "ItemIconAtlasGreyscale";
		}
		return "ItemIconAtlas";
	}

	public override string GetPartName()
	{
		if (this.itemClass == null && this.expectedItemClass == null)
		{
			return "";
		}
		if (this.itemClass == null)
		{
			return string.Format(this.lblStackMissing, this.expectedItemClass.GetLocalizedItemName());
		}
		return this.itemClass.GetLocalizedItemName();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void SetEmptySpriteName()
	{
		if (this.expectedItemClass != null && this.expectedItemClass.Id != 0)
		{
			this.emptySpriteName = this.expectedItemClass.GetIconName();
			return;
		}
		this.emptySpriteName = "";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool CanSwap(ItemStack stack)
	{
		ItemClassModifier itemClassModifier = stack.itemValue.ItemClass as ItemClassModifier;
		if (itemClassModifier == null)
		{
			return false;
		}
		ItemValue itemValue = base.xui.AssembleItem.CurrentItem.itemValue;
		ItemClass itemClass = itemValue.ItemClass;
		if (itemClass.HasAnyTags(itemClassModifier.DisallowedTags))
		{
			return false;
		}
		if (!itemClass.HasAnyTags(itemClassModifier.InstallableTags))
		{
			return false;
		}
		if (itemClassModifier.HasAnyTags(EntityDrone.cStorageModifierTags))
		{
			return true;
		}
		if (itemClassModifier.ItemTags.Test_AnySet(ItemClassModifier.CosmeticModTypes) && itemValue.CosmeticMods.Length != 0)
		{
			for (int i = 0; i < itemValue.CosmeticMods.Length; i++)
			{
				if (itemValue.CosmeticMods[i] == null || itemValue.CosmeticMods[i].IsEmpty())
				{
					return true;
				}
			}
			return false;
		}
		bool flag = itemClassModifier.InstallableTags.IsEmpty || itemClass.HasAnyTags(itemClassModifier.InstallableTags);
		for (int j = 0; j < itemValue.Modifications.Length; j++)
		{
			if (itemValue.Modifications[j].ItemClass != null && !itemValue.Modifications[j].ItemClass.ItemTags.IsEmpty && itemValue.Modifications[j] != this.itemValue && itemClassModifier.HasAnyTags(itemValue.Modifications[j].ItemClass.ItemTags))
			{
				return false;
			}
		}
		return flag && (this.itemValue == null || this.itemValue.type == 0 || (this.itemValue.ItemClass as ItemClassModifier).Type == ItemClassModifier.ModifierTypes.Attachment);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool CanRemove()
	{
		return this.itemClass is ItemClassModifier && (this.itemClass as ItemClassModifier).Type == ItemClassModifier.ModifierTypes.Attachment;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void SwapItem()
	{
		ItemClassModifier itemClassModifier = base.xui.dragAndDrop.CurrentStack.itemValue.ItemClass as ItemClassModifier;
		base.SwapItem();
		if (itemClassModifier != null && itemClassModifier.ItemTags.Test_AnySet(ItemClassModifier.CosmeticModTypes))
		{
			base.xui.dragAndDrop.CurrentStack = ItemStack.Empty.Clone();
			base.xui.dragAndDrop.PickUpType = base.StackLocation;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemValue itemValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemClass expectedItemClass;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblStackMissing;
}
