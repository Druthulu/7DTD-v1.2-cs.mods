using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_DewCollectorModGrid : XUiC_ItemStackGrid
{
	public override XUiC_ItemStack.StackLocationTypes StackLocation
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return XUiC_ItemStack.StackLocationTypes.Workstation;
		}
	}

	public override void Init()
	{
		base.Init();
		string[] array = this.requiredMods.Split(',', StringSplitOptions.None);
		for (int i = 0; i < this.itemControllers.Length; i++)
		{
			XUiC_RequiredItemStack xuiC_RequiredItemStack = this.itemControllers[i] as XUiC_RequiredItemStack;
			if (xuiC_RequiredItemStack != null)
			{
				if (i < array.Length)
				{
					xuiC_RequiredItemStack.RequiredItemClass = ItemClass.GetItemClass(array[i], false);
					xuiC_RequiredItemStack.RequiredItemOnly = this.requiredModsOnly;
				}
				else
				{
					xuiC_RequiredItemStack.RequiredItemClass = null;
					xuiC_RequiredItemStack.RequiredItemOnly = false;
				}
				xuiC_RequiredItemStack.StackLocation = this.StackLocation;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void UpdateBackend(ItemStack[] stackList)
	{
		base.UpdateBackend(stackList);
		this.tileEntity.ModSlots = stackList;
		this.windowGroup.Controller.SetAllChildrenDirty(false);
	}

	public void SetTileEntity(TileEntityDewCollector te)
	{
		this.tileEntity = te;
		this.SetStacks(te.ModSlots);
	}

	public override bool ParseAttribute(string name, string value, XUiController _parent)
	{
		bool flag = base.ParseAttribute(name, value, _parent);
		if (!flag)
		{
			if (!(name == "required_mods"))
			{
				if (!(name == "required_mods_only"))
				{
					return false;
				}
				this.requiredModsOnly = StringParsers.ParseBool(value, 0, -1, true);
			}
			else
			{
				this.requiredMods = value;
			}
			return true;
		}
		return flag;
	}

	public bool TryAddMod(ItemClass newItemClass, ItemStack newItemStack)
	{
		if (!this.requiredModsOnly)
		{
			return false;
		}
		for (int i = 0; i < this.itemControllers.Length; i++)
		{
			XUiC_RequiredItemStack xuiC_RequiredItemStack = (XUiC_RequiredItemStack)this.itemControllers[i];
			if (xuiC_RequiredItemStack.RequiredItemClass == newItemClass && xuiC_RequiredItemStack.ItemStack.IsEmpty())
			{
				xuiC_RequiredItemStack.ItemStack = newItemStack.Clone();
				return true;
			}
		}
		return false;
	}

	public override void OnOpen()
	{
		base.OnOpen();
		base.xui.currentDewCollectorModGrid = this;
	}

	public override void OnClose()
	{
		base.OnClose();
		base.xui.currentDewCollectorModGrid = null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string requiredMods = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public bool requiredModsOnly;

	[PublicizedFrom(EAccessModifier.Private)]
	public TileEntityDewCollector tileEntity;
}
