using System;
using UnityEngine.Scripting;

[Preserve]
public class ItemClassArmor : ItemClass
{
	public override bool IsEquipment
	{
		get
		{
			return true;
		}
	}

	public override void Init()
	{
		base.Init();
		string text = "";
		this.Properties.ParseString("ArmorGroup", ref text);
		this.ArmorGroup = text.Split(',', StringSplitOptions.None);
		if (this.Properties.Values.ContainsKey("EquipSlot"))
		{
			this.EquipSlot = EnumUtils.Parse<EquipmentSlots>(this.Properties.Values["EquipSlot"], false);
		}
	}

	public const string PropEquipSlot = "EquipSlot";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string PropArmorGroup = "ArmorGroup";

	public EquipmentSlots EquipSlot = EquipmentSlots.Count;

	public string[] ArmorGroup;
}
