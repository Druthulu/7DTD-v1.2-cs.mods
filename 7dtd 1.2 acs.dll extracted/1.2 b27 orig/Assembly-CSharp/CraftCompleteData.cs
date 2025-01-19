using System;
using System.IO;

public class CraftCompleteData
{
	public CraftCompleteData()
	{
	}

	public CraftCompleteData(int crafterEntityID, ItemStack craftedItemStack, string recipeName, int craftExpGain, ushort recipeUsedCount)
	{
		this.CrafterEntityID = crafterEntityID;
		this.CraftedItemStack = craftedItemStack;
		this.RecipeName = recipeName;
		this.RecipeUsedCount = recipeUsedCount;
		this.CraftExpGain = craftExpGain;
	}

	public void Write(BinaryWriter _bw, int version)
	{
		_bw.Write(this.CrafterEntityID);
		this.CraftedItemStack.Write(_bw);
		_bw.Write(this.RecipeName);
		_bw.Write(this.CraftExpGain);
		_bw.Write(this.RecipeUsedCount);
	}

	public void Read(BinaryReader _br, int version)
	{
		this.CrafterEntityID = _br.ReadInt32();
		this.CraftedItemStack = new ItemStack().Read(_br);
		this.RecipeName = _br.ReadString();
		this.CraftExpGain = _br.ReadInt32();
		if (version >= 48)
		{
			this.RecipeUsedCount = _br.ReadUInt16();
			return;
		}
		this.RecipeUsedCount = (ushort)this.CraftedItemStack.count;
	}

	public int CrafterEntityID;

	public ItemStack CraftedItemStack;

	public string RecipeName = "";

	public ushort RecipeUsedCount;

	public int CraftExpGain;
}
