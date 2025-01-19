using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiControllerMissing : XUiController
{
	public override void OnOpen()
	{
		base.OnOpen();
		this.OnClose();
	}
}
