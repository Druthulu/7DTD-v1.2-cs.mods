using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_StartPointEditor : XUiController
{
	public override void Init()
	{
		base.Init();
		XUiC_StartPointEditor.ID = base.WindowGroup.ID;
		this.cbxHeading = base.GetChildById("cbxHeading").GetChildByType<XUiC_ComboBoxInt>();
		this.cbxHeading.OnValueChanged += this.CbxHeading_OnValueChanged;
		((XUiC_SimpleButton)base.GetChildById("btnCancel")).OnPressed += this.BtnCancel_OnPressed;
		((XUiC_SimpleButton)base.GetChildById("btnOk")).OnPressed += this.BtnOk_OnPressed;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnCancel_OnPressed(XUiController _sender, int _mouseButton)
	{
		base.xui.playerUI.windowManager.Close(base.WindowGroup, false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnOk_OnPressed(XUiController _sender, int _mouseButton)
	{
		this.headingOnOpen = this.cbxHeading.Value;
		base.xui.playerUI.windowManager.Close(base.WindowGroup, false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CbxHeading_OnValueChanged(XUiController _sender, long _oldvalue, long _newvalue)
	{
		this.spawnPoint.spawnPosition.heading = (this.selectionBox.facingDirection = (float)_newvalue);
		base.RefreshBindings(false);
	}

	public override void OnOpen()
	{
		base.OnOpen();
		ValueTuple<SelectionCategory, SelectionBox>? valueTuple;
		this.selectionBox = ((SelectionBoxManager.Instance.Selection != null) ? valueTuple.GetValueOrDefault().Item2 : null);
		if (this.selectionBox == null)
		{
			return;
		}
		this.spawnPoint = GameManager.Instance.GetSpawnPointList().Find(Vector3i.Parse(this.selectionBox.name));
		this.cbxHeading.Value = (this.headingOnOpen = (long)this.spawnPoint.spawnPosition.heading);
		base.RefreshBindings(false);
	}

	public override void OnClose()
	{
		base.OnClose();
		this.spawnPoint.spawnPosition.heading = (this.selectionBox.facingDirection = (float)this.headingOnOpen);
		this.spawnPoint = null;
		this.selectionBox = null;
	}

	public override bool GetBindingValue(ref string _value, string _bindingName)
	{
		if (_bindingName == "position")
		{
			SpawnPoint spawnPoint = this.spawnPoint;
			_value = (((spawnPoint != null) ? spawnPoint.spawnPosition.position.ToCultureInvariantString() : null) ?? "");
			return true;
		}
		if (!(_bindingName == "cardinal"))
		{
			return base.GetBindingValue(ref _value, _bindingName);
		}
		SpawnPoint spawnPoint2 = this.spawnPoint;
		_value = GameUtils.GetClosestDirection((spawnPoint2 != null) ? spawnPoint2.spawnPosition.heading : 0f, false).ToStringCached<GameUtils.DirEightWay>();
		return true;
	}

	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxInt cbxHeading;

	[PublicizedFrom(EAccessModifier.Private)]
	public SpawnPoint spawnPoint;

	[PublicizedFrom(EAccessModifier.Private)]
	public SelectionBox selectionBox;

	[PublicizedFrom(EAccessModifier.Private)]
	public long headingOnOpen;
}
