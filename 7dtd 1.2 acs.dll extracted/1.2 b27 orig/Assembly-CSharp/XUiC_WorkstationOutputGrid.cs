using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_WorkstationOutputGrid : XUiC_WorkstationGrid
{
	public void UpdateData(ItemStack[] stackList)
	{
		this.UpdateBackend(stackList);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void UpdateBackend(ItemStack[] stackList)
	{
		base.UpdateBackend(stackList);
		this.workstationData.SetOutputStacks(stackList);
		this.windowGroup.Controller.SetAllChildrenDirty(false);
	}
}
