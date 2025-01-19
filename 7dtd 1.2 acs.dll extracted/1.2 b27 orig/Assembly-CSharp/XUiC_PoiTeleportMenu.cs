﻿using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_PoiTeleportMenu : XUiController
{
	public override void Init()
	{
		base.Init();
		XUiC_PoiTeleportMenu.ID = base.WindowGroup.ID;
		this.list = base.GetChildByType<XUiC_PoiList>();
		this.list.SelectionChanged += this.ListSelectionChanged;
		XUiController childById = base.GetChildById("filterSmall");
		XUiC_ToggleButton xuiC_ToggleButton = (childById != null) ? childById.GetChildByType<XUiC_ToggleButton>() : null;
		xuiC_ToggleButton.Value = this.list.FilterSmallPois;
		xuiC_ToggleButton.OnValueChanged += this.FilterSmall_Changed;
		XUiController childById2 = base.GetChildById("cbxFilterTier");
		this.cbxFilterTier = ((childById2 != null) ? childById2.GetChildByType<XUiC_ComboBoxInt>() : null);
		this.cbxFilterTier.Value = (long)this.list.FilterTier;
		this.cbxFilterTier.OnValueChanged += this.CbxFilterTier_OnValueChanged;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CbxFilterTier_OnValueChanged(XUiController _sender, long _oldValue, long _newValue)
	{
		this.list.FilterTier = (int)_newValue;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void FilterSmall_Changed(XUiC_ToggleButton _sender, bool _newvalue)
	{
		this.list.FilterSmallPois = _newvalue;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ListSelectionChanged(XUiC_ListEntry<XUiC_PoiList.PoiListEntry> _previousEntry, XUiC_ListEntry<XUiC_PoiList.PoiListEntry> _newEntry)
	{
		if (_newEntry != null && _newEntry.GetEntry() != null)
		{
			XUiC_PoiList.PoiListEntry entry = _newEntry.GetEntry();
			this.EntryPressed(entry);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void EntryPressed(XUiC_PoiList.PoiListEntry _key)
	{
		base.xui.playerUI.entityPlayer.Teleport(_key.prefabInstance.boundingBoxPosition.ToVector3(), 45f);
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.cbxFilterTier.Max = (long)this.list.MaxTier;
	}

	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_PoiList list;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxInt cbxFilterTier;
}
