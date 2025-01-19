using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_BugReportSaveSelect : XUiController
{
	public override void Init()
	{
		base.Init();
		this.list = base.GetChildByType<XUiC_BugReportSavesList>();
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.RebuildList();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RebuildList()
	{
		this.list.RebuildList(SaveInfoProvider.Instance.SaveEntryInfos, false);
	}

	public XUiC_BugReportSavesList list;
}
