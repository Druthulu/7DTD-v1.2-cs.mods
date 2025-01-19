using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ItemCosmeticStack : XUiC_BasePartStack
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
		if (base.xui.AssembleItem.CurrentItem.itemValue.ItemClass.HasAnyTags(itemClassModifier.DisallowedTags))
		{
			return false;
		}
		if (!base.xui.AssembleItem.CurrentItem.itemValue.ItemClass.HasAnyTags(itemClassModifier.InstallableTags))
		{
			return false;
		}
		if (itemClassModifier != null && !itemClassModifier.ItemTags.Test_AnySet(ItemClassModifier.CosmeticModTypes) && base.xui.AssembleItem.CurrentItem.itemValue.Modifications.Length != 0)
		{
			bool result = false;
			for (int i = 0; i < base.xui.AssembleItem.CurrentItem.itemValue.Modifications.Length; i++)
			{
				if (base.xui.AssembleItem.CurrentItem.itemValue.Modifications[i] == null || base.xui.AssembleItem.CurrentItem.itemValue.Modifications[i].IsEmpty())
				{
					result = true;
				}
				else if (itemClassModifier.HasAnyTags(base.xui.AssembleItem.CurrentItem.itemValue.Modifications[i].ItemClass.ItemTags))
				{
					result = false;
					break;
				}
			}
			return result;
		}
		bool flag = itemClassModifier.InstallableTags.IsEmpty || base.xui.AssembleItem.CurrentItem.itemValue.ItemClass.HasAnyTags(itemClassModifier.InstallableTags);
		for (int j = 0; j < base.xui.AssembleItem.CurrentItem.itemValue.CosmeticMods.Length; j++)
		{
			if (base.xui.AssembleItem.CurrentItem.itemValue.CosmeticMods[j] != null && base.xui.AssembleItem.CurrentItem.itemValue.CosmeticMods[j].ItemClass != null && !base.xui.AssembleItem.CurrentItem.itemValue.CosmeticMods[j].ItemClass.ItemTags.IsEmpty && base.xui.AssembleItem.CurrentItem.itemValue.CosmeticMods[j] != this.itemValue && itemClassModifier.HasAnyTags(base.xui.AssembleItem.CurrentItem.itemValue.CosmeticMods[j].ItemClass.ItemTags))
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
		if (itemClassModifier != null && !itemClassModifier.ItemTags.Test_AnySet(ItemClassModifier.CosmeticModTypes) && itemClassModifier != null && !itemClassModifier.ItemTags.Test_AnySet(ItemClassModifier.CosmeticModTypes) && base.xui.AssembleItem.CurrentItem.itemValue.Modifications.Length != 0)
		{
			bool flag = false;
			for (int i = 0; i < base.xui.AssembleItem.CurrentItem.itemValue.Modifications.Length; i++)
			{
				if (itemClassModifier.HasAnyTags(base.xui.AssembleItem.CurrentItem.itemValue.Modifications[i].ItemClass.ItemTags))
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				base.xui.dragAndDrop.CurrentStack = ItemStack.Empty.Clone();
				base.xui.dragAndDrop.PickUpType = base.StackLocation;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemValue itemValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemClass expectedItemClass;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lblStackMissing;
}
