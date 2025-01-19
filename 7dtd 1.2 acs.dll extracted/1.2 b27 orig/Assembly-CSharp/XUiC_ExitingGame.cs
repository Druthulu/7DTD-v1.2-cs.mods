using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ExitingGame : XUiController
{
	public override void Init()
	{
		base.Init();
		XUiC_ExitingGame.ID = base.WindowGroup.ID;
	}

	public override void OnOpen()
	{
		base.OnOpen();
		base.ViewComponent.IsVisible = true;
		((XUiV_Window)this.viewComponent).ForceVisible(1f);
	}

	public override void Cleanup()
	{
		base.Cleanup();
	}

	public static string ID = "";
}
