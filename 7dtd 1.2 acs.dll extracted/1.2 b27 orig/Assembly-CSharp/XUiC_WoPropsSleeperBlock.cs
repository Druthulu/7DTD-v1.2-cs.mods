using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_WoPropsSleeperBlock : XUiController
{
	public static void Open(LocalPlayerUI _playerUi, TileEntitySleeper _te)
	{
		_playerUi.xui.FindWindowGroupByName(XUiC_WoPropsSleeperBlock.ID).GetChildByType<XUiC_WoPropsSleeperBlock>().tileEntitySleeper = _te;
		_playerUi.windowManager.Open(XUiC_WoPropsSleeperBlock.ID, true, false, true);
	}

	public override void Init()
	{
		base.Init();
		XUiC_WoPropsSleeperBlock.ID = base.WindowGroup.ID;
		this.cbxPriority = (XUiC_ComboBoxList<XUiC_WoPropsSleeperBlock.PriorityMultiplier>)base.GetChildById("cbxPriority");
		for (float num = 0.5f; num < 5.1f; num += 0.5f)
		{
			this.cbxPriority.Elements.Add(new XUiC_WoPropsSleeperBlock.PriorityMultiplier(num));
		}
		this.txtSightRange = (XUiC_TextInput)base.GetChildById("txtSightRange");
		this.txtHearingPercent = (XUiC_TextInput)base.GetChildById("txtHearingPercent");
		this.txtSightAngle = (XUiC_TextInput)base.GetChildById("txtSightAngle");
		((XUiC_SimpleButton)base.GetChildById("btnMonsterCloset")).OnPressed += this.BtnMonsterCloset_OnPressed;
		((XUiC_SimpleButton)base.GetChildById("btnDefaults")).OnPressed += this.BtnDefaults_OnPressed;
		((XUiC_SimpleButton)base.GetChildById("btnSave")).OnPressed += this.BtnSave_OnPressed;
		((XUiC_SimpleButton)base.GetChildById("btnCancel")).OnPressed += this.BtnCancel_OnPressed;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnMonsterCloset_OnPressed(XUiController _sender, int _mouseButton)
	{
		this.updateCbxPriority(1.5f);
		this.txtSightRange.Text = "4";
		this.txtHearingPercent.Text = "10";
		this.txtSightAngle.Text = "60";
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnDefaults_OnPressed(XUiController _sender, int _mouseButton)
	{
		this.updateCbxPriority(1f);
		this.txtSightRange.Text = "-1";
		this.txtHearingPercent.Text = "100";
		this.txtSightAngle.Text = "-1";
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnCancel_OnPressed(XUiController _sender, int _mouseButton)
	{
		base.xui.playerUI.windowManager.Close(this.windowGroup.ID);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnSave_OnPressed(XUiController _sender, int _mouseButton)
	{
		this.tileEntitySleeper.SetPriorityMultiplier(this.cbxPriority.Value.value);
		this.tileEntitySleeper.SetSightRange(StringParsers.ParseSInt32(this.txtSightRange.Text, 0, -1, NumberStyles.Integer));
		this.tileEntitySleeper.SetHearingPercent((float)StringParsers.ParseSInt32(this.txtHearingPercent.Text, 0, -1, NumberStyles.Integer) / 100f);
		this.tileEntitySleeper.SetSightAngle(StringParsers.ParseSInt32(this.txtSightAngle.Text, 0, -1, NumberStyles.Integer));
		base.xui.playerUI.windowManager.Close(this.windowGroup.ID);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateCbxPriority(float _priorityMultiplier)
	{
		for (int i = 0; i < this.cbxPriority.Elements.Count; i++)
		{
			if ((double)Mathf.Abs(_priorityMultiplier - this.cbxPriority.Elements[i].value) < 0.01)
			{
				this.cbxPriority.SelectedIndex = i;
				return;
			}
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		if (this.tileEntitySleeper != null)
		{
			this.updateCbxPriority(this.tileEntitySleeper.GetPriorityMultiplier());
			this.txtSightRange.Text = this.tileEntitySleeper.GetSightRange().ToString();
			this.txtHearingPercent.Text = ((int)(this.tileEntitySleeper.GetHearingPercent() * 100f)).ToString();
			this.txtSightAngle.Text = this.tileEntitySleeper.GetSightAngle().ToString();
		}
	}

	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<XUiC_WoPropsSleeperBlock.PriorityMultiplier> cbxPriority;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput txtSightRange;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput txtHearingPercent;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput txtSightAngle;

	[PublicizedFrom(EAccessModifier.Private)]
	public TileEntitySleeper tileEntitySleeper;

	public struct PriorityMultiplier
	{
		public PriorityMultiplier(float _value)
		{
			this.value = _value;
		}

		public override string ToString()
		{
			return "x" + this.value.ToCultureInvariantString("0.0");
		}

		public readonly float value;
	}
}
