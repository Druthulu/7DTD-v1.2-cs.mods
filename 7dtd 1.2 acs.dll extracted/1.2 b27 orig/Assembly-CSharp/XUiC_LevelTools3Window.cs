using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_LevelTools3Window : XUiController
{
	public override void Init()
	{
		base.Init();
		XUiC_LevelTools3Window.ID = base.WindowGroup.ID;
		this.btnEntitySpawner = base.GetChildById("btnEntitySpawner");
		this.btnEntitySpawner.GetChildById("clickable").OnPress += this.BtnEntitySpawner_Controller_OnPress;
		this.btnSleeperVolume = base.GetChildById("btnSleeperVolume");
		this.btnSleeperVolume.GetChildById("clickable").OnPress += this.BtnSleeperVolume_Controller_OnPress;
		this.btnTeleportVolume = base.GetChildById("btnTeleportVolume");
		this.btnTeleportVolume.GetChildById("clickable").OnPress += this.BtnTeleportVolume_Controller_OnPress;
		this.btnTriggerVolume = base.GetChildById("btnTriggerVolume");
		this.btnTriggerVolume.GetChildById("clickable").OnPress += this.BtnTriggerVolume_Controller_OnPress;
		this.btnInfoVolume = base.GetChildById("btnInfoVolume");
		this.btnInfoVolume.GetChildById("clickable").OnPress += this.BtnInfoVolume_Controller_OnPress;
		this.btnWallVolume = base.GetChildById("btnWallVolume");
		this.btnWallVolume.GetChildById("clickable").OnPress += this.BtnWallVolume_Controller_OnPress;
		XUiController childById = base.GetChildById("buttons");
		this.buttons = new XUiC_SimpleButton[childById.Children.Count];
		this.toggles = new XUiC_ToggleButton[childById.Children.Count];
		this.actions = new NGuiAction[childById.Children.Count];
		for (int i = 0; i < childById.Children.Count; i++)
		{
			this.buttons[i] = childById.Children[i].GetChildById("button").GetChildByType<XUiC_SimpleButton>();
			this.toggles[i] = childById.Children[i].GetChildById("toggle").GetChildByType<XUiC_ToggleButton>();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnEntitySpawner_Controller_OnPress(XUiController _sender, int _mouseButton)
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnSleeperVolume_Controller_OnPress(XUiController _sender, int _mouseButton)
	{
		Vector3 raycastHitPoint = XUiC_LevelTools3Window.getRaycastHitPoint(100f, 0f);
		if (raycastHitPoint.Equals(Vector3.zero))
		{
			return;
		}
		Vector3i hitPointBlockPos = World.worldToBlockPos(raycastHitPoint);
		PrefabSleeperVolumeManager.Instance.AddSleeperVolumeServer(hitPointBlockPos);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnTeleportVolume_Controller_OnPress(XUiController _sender, int _mouseButton)
	{
		Vector3 raycastHitPoint = XUiC_LevelTools3Window.getRaycastHitPoint(100f, 0f);
		if (raycastHitPoint.Equals(Vector3.zero))
		{
			return;
		}
		Vector3i hitPointBlockPos = World.worldToBlockPos(raycastHitPoint);
		PrefabVolumeManager.Instance.AddTeleportVolumeServer(hitPointBlockPos);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnTriggerVolume_Controller_OnPress(XUiController _sender, int _mouseButton)
	{
		Vector3 raycastHitPoint = XUiC_LevelTools3Window.getRaycastHitPoint(100f, 0f);
		if (raycastHitPoint.Equals(Vector3.zero))
		{
			return;
		}
		Vector3i hitPointBlockPos = World.worldToBlockPos(raycastHitPoint);
		PrefabTriggerVolumeManager.Instance.AddTriggerVolumeServer(hitPointBlockPos);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnInfoVolume_Controller_OnPress(XUiController _sender, int _mouseButton)
	{
		Vector3 raycastHitPoint = XUiC_LevelTools3Window.getRaycastHitPoint(100f, 0f);
		if (raycastHitPoint.Equals(Vector3.zero))
		{
			return;
		}
		Vector3i hitPointBlockPos = World.worldToBlockPos(raycastHitPoint);
		PrefabVolumeManager.Instance.AddInfoVolumeServer(hitPointBlockPos);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnWallVolume_Controller_OnPress(XUiController _sender, int _mouseButton)
	{
		Vector3 raycastHitPoint = XUiC_LevelTools3Window.getRaycastHitPoint(100f, 0f);
		if (raycastHitPoint.Equals(Vector3.zero))
		{
			return;
		}
		Vector3i hitPointBlockPos = World.worldToBlockPos(raycastHitPoint);
		PrefabVolumeManager.Instance.AddWallVolumeServer(hitPointBlockPos);
	}

	public static Vector3 getRaycastHitPoint(float _maxDistance = 100f, float _offsetUp = 0f)
	{
		Camera finalCamera = GameManager.Instance.World.GetPrimaryPlayer().finalCamera;
		Ray ray = finalCamera.ScreenPointToRay(new Vector3((float)Screen.width * 0.5f, (float)Screen.height * 0.5f, 0f));
		ray.origin += Origin.position;
		Transform transform = finalCamera.transform;
		ray.origin += transform.forward * 0.1f;
		ray.origin += transform.up * _offsetUp;
		if (Voxel.Raycast(GameManager.Instance.World, ray, _maxDistance, 4095, 0f))
		{
			return Voxel.voxelRayHitInfo.hit.pos - ray.direction * 0.05f;
		}
		return Vector3.zero;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ToggleButton_OnPress(NGuiAction _action)
	{
		_action.OnClick();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SimpleButton_OnPress(NGuiAction _action)
	{
		_action.OnClick();
	}

	public override void OnOpen()
	{
		base.OnOpen();
		if (!this.btnsInitialized)
		{
			this.btnsInitialized = true;
			this.buttonsCount = 0;
			BlockToolSelection blockToolSelection = (BlockToolSelection)((GameManager.Instance.GetActiveBlockTool() is BlockToolSelection) ? GameManager.Instance.GetActiveBlockTool() : null);
			if (blockToolSelection != null)
			{
				this.buttonsCount++;
				foreach (NGuiAction nguiAction in blockToolSelection.GetActions())
				{
					if (nguiAction == NGuiAction.Separator)
					{
						this.buttonsCount++;
					}
					else
					{
						this.SetButton(ref this.buttonsCount, nguiAction);
					}
				}
			}
		}
		int num = 0;
		while (num < this.buttonsCount && num < this.buttons.Length)
		{
			if (this.actions[num] == null)
			{
				this.buttons[num].ViewComponent.IsVisible = false;
				this.toggles[num].ViewComponent.IsVisible = false;
			}
			else if (this.actions[num].IsToggle())
			{
				this.buttons[num].ViewComponent.IsVisible = false;
			}
			else
			{
				this.toggles[num].ViewComponent.IsVisible = false;
			}
			num++;
		}
		if (this.buttonsCount < this.buttons.Length)
		{
			for (int i = this.buttonsCount; i < this.buttons.Length; i++)
			{
				this.buttons[i].ViewComponent.IsVisible = false;
				this.toggles[i].ViewComponent.IsVisible = false;
			}
		}
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			this.btnEntitySpawner.ViewComponent.IsVisible = false;
			this.btnLevelStartPoint.ViewComponent.IsVisible = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetButton(ref int _buttonIndex, NGuiAction _action)
	{
		if (_buttonIndex < this.actions.Length)
		{
			this.actions[_buttonIndex] = _action;
			if (_action != null)
			{
				string text = _action.GetText() + " " + _action.GetHotkey().GetBindingXuiMarkupString(XUiUtils.EmptyBindingStyle.EmptyString, XUiUtils.DisplayStyle.KeyboardWithParentheses, null);
				string tooltip = _action.GetTooltip();
				if (_action.IsToggle())
				{
					this.toggles[_buttonIndex].Label = text;
					this.toggles[_buttonIndex].OnValueChanged += delegate(XUiC_ToggleButton _sender, bool _newValue)
					{
						this.ToggleButton_OnPress(_action);
					};
					this.toggles[_buttonIndex].Tooltip = tooltip;
				}
				else
				{
					this.buttons[_buttonIndex].Text = text;
					this.buttons[_buttonIndex].OnPressed += delegate(XUiController _sender, int _mouseButton)
					{
						this.SimpleButton_OnPress(_action);
					};
					this.buttons[_buttonIndex].Tooltip = tooltip;
				}
			}
		}
		else
		{
			Log.Warning("[XUi] Could not add further buttons to XUiC_LevelToolsWindow");
		}
		_buttonIndex++;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		for (int i = 0; i < this.buttonsCount; i++)
		{
			if (this.actions[i] != null)
			{
				if (this.actions[i].IsToggle())
				{
					this.toggles[i].Value = this.actions[i].IsChecked();
				}
				else
				{
					this.buttons[i].Enabled = this.actions[i].IsEnabled();
				}
			}
		}
	}

	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton[] buttons;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ToggleButton[] toggles;

	[PublicizedFrom(EAccessModifier.Private)]
	public NGuiAction[] actions;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool btnsInitialized;

	[PublicizedFrom(EAccessModifier.Private)]
	public int buttonsCount;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController btnLevelStartPoint;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController btnEntitySpawner;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController btnSleeperVolume;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController btnTeleportVolume;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController btnTriggerVolume;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController btnInfoVolume;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController btnWallVolume;
}
