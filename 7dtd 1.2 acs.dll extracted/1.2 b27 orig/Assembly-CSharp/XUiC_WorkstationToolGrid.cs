using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_WorkstationToolGrid : XUiC_WorkstationGrid
{
	public event XuiEvent_WorkstationItemsChanged OnWorkstationToolsChanged;

	public override void Init()
	{
		base.Init();
		string[] array = this.requiredTools.Split(',', StringSplitOptions.None);
		for (int i = 0; i < this.itemControllers.Length; i++)
		{
			if (i < array.Length)
			{
				((XUiC_RequiredItemStack)this.itemControllers[i]).RequiredItemClass = ItemClass.GetItemClass(array[i], false);
				((XUiC_RequiredItemStack)this.itemControllers[i]).RequiredItemOnly = this.requiredToolsOnly;
			}
			else
			{
				((XUiC_RequiredItemStack)this.itemControllers[i]).RequiredItemClass = null;
				((XUiC_RequiredItemStack)this.itemControllers[i]).RequiredItemOnly = false;
			}
		}
	}

	public override bool HasRequirement(Recipe recipe)
	{
		if (recipe == null)
		{
			return false;
		}
		if (recipe.craftingToolType == 0)
		{
			return true;
		}
		ItemStack[] slots = this.GetSlots();
		for (int i = 0; i < slots.Length; i++)
		{
			if (slots[i].itemValue.type == recipe.craftingToolType)
			{
				return true;
			}
		}
		return false;
	}

	public void SetToolLocks(bool locked)
	{
		if (locked != this.isLocked)
		{
			this.isLocked = locked;
			for (int i = 0; i < this.itemControllers.Length; i++)
			{
				this.itemControllers[i].ToolLock = locked;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void UpdateBackend(ItemStack[] stackList)
	{
		base.UpdateBackend(stackList);
		this.workstationData.SetToolStacks(stackList);
		this.windowGroup.Controller.SetAllChildrenDirty(false);
	}

	public override bool ParseAttribute(string name, string value, XUiController _parent)
	{
		bool flag = base.ParseAttribute(name, value, _parent);
		if (!flag)
		{
			if (!(name == "required_tools"))
			{
				if (!(name == "required_tools_only"))
				{
					return false;
				}
				this.requiredToolsOnly = StringParsers.ParseBool(value, 0, -1, true);
			}
			else
			{
				this.requiredTools = value;
			}
			return true;
		}
		return flag;
	}

	public bool TryAddTool(ItemClass newItemClass, ItemStack newItemStack)
	{
		if (!this.requiredToolsOnly || this.isLocked)
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

	public override void HandleSlotChangedEvent(int slotNumber, ItemStack stack)
	{
		base.HandleSlotChangedEvent(slotNumber, stack);
		if (this.OnWorkstationToolsChanged != null)
		{
			this.OnWorkstationToolsChanged();
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		base.xui.currentWorkstationToolGrid = this;
	}

	public override void OnClose()
	{
		base.OnClose();
		base.xui.currentWorkstationToolGrid = null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isLocked;

	[PublicizedFrom(EAccessModifier.Private)]
	public string requiredTools = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public bool requiredToolsOnly;
}
