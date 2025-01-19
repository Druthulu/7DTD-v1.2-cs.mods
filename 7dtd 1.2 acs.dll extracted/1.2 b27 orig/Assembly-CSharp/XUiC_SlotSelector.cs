using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_SlotSelector : XUiC_Selector
{
	public event XUiEvent_SelectedSlotChanged OnSelectedSlotChanged;

	public override void OnOpen()
	{
		base.OnOpen();
		this.currentValue.Text = ((EquipmentSlots)this.selectedIndex).ToStringCached<EquipmentSlots>();
	}

	public override void BackPressed()
	{
		if (this.selectedIndex < 0)
		{
			this.selectedIndex = 4;
		}
		this.currentValue.Text = ((EquipmentSlots)this.selectedIndex).ToStringCached<EquipmentSlots>();
		if (this.OnSelectedSlotChanged != null)
		{
			this.OnSelectedSlotChanged(this.selectedIndex);
		}
	}

	public override void ForwardPressed()
	{
		if (this.selectedIndex >= 5)
		{
			this.selectedIndex = 0;
		}
		this.currentValue.Text = ((EquipmentSlots)this.selectedIndex).ToStringCached<EquipmentSlots>();
		if (this.OnSelectedSlotChanged != null)
		{
			this.OnSelectedSlotChanged(this.selectedIndex);
		}
	}
}
