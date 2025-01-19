using System;
using UnityEngine;

public class GUIWindowNGUI : GUIWindow
{
	public GUIWindowNGUI(EnumNGUIWindow _nguiEnum) : base(_nguiEnum.ToStringCached<EnumNGUIWindow>(), default(Rect))
	{
		this.nguiEnum = _nguiEnum;
	}

	public GUIWindowNGUI(EnumNGUIWindow _nguiEnum, bool _bDrawBackground) : base(_nguiEnum.ToStringCached<EnumNGUIWindow>(), default(Rect), _bDrawBackground)
	{
		this.nguiEnum = _nguiEnum;
	}

	public override void OnOpen()
	{
		this.nguiWindowManager.Show(this.nguiEnum, true);
	}

	public override void OnClose()
	{
		base.OnClose();
		this.nguiWindowManager.Show(this.nguiEnum, false);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public EnumNGUIWindow nguiEnum;
}
