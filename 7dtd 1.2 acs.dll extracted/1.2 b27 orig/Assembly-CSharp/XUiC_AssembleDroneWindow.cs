using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_AssembleDroneWindow : XUiC_AssembleWindow
{
	public override void OnOpen()
	{
		base.OnOpen();
		this.isDirty = true;
	}

	public override ItemStack ItemStack
	{
		set
		{
			this.group.CurrentVehicleEntity.LoadMods();
			base.ItemStack = value;
		}
	}

	public override void OnChanged()
	{
		this.group.OnItemChanged(this.ItemStack);
		this.isDirty = true;
	}

	public XUiC_DroneWindowGroup group;
}
