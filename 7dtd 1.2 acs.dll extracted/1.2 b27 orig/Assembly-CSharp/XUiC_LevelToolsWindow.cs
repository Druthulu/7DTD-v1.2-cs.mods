using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_LevelToolsWindow : XUiController
{
	public override void Init()
	{
		base.Init();
		XUiC_LevelToolsWindow.ID = base.WindowGroup.ID;
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
			foreach (KeyValuePair<string, SelectionCategory> keyValuePair in SelectionBoxManager.Instance.GetCategories())
			{
				string name = keyValuePair.Value.name;
				NGuiAction action = new NGuiAction(Localization.Get("selectionCategory" + name, false), null, true);
				action.SetDescription(name);
				action.SetClickActionDelegate(delegate
				{
					SelectionCategory category = SelectionBoxManager.Instance.GetCategory(action.GetDescription());
					category.SetVisible(!category.IsVisible());
				});
				action.SetIsCheckedDelegate(() => SelectionBoxManager.Instance.GetCategory(action.GetDescription()).IsVisible());
				this.SetButton(ref this.buttonsCount, action);
				if (name == "SleeperVolume")
				{
					NGuiAction nguiAction = new NGuiAction(Localization.Get("leveltoolsSleeperXRay", false), null, true);
					nguiAction.SetClickActionDelegate(delegate
					{
						SleeperVolumeToolManager.SetXRay(!SleeperVolumeToolManager.GetXRay());
					});
					nguiAction.SetIsCheckedDelegate(() => SleeperVolumeToolManager.GetXRay());
					this.SetButton(ref this.buttonsCount, nguiAction);
				}
			}
			this.buttonsCount++;
			NGuiAction nguiAction2 = new NGuiAction(Localization.Get("leveltoolsShowUnpaintable", false), null, true);
			nguiAction2.SetClickActionDelegate(delegate
			{
				GameObject gameObject = GameObject.Find("/Chunks");
				if (gameObject == null)
				{
					return;
				}
				GameManager.bShowUnpaintables = !GameManager.bShowUnpaintables;
				this.enableUnpaintables(GameManager.bShowUnpaintables, gameObject.transform);
			});
			nguiAction2.SetIsCheckedDelegate(() => GameManager.bShowUnpaintables);
			this.SetButton(ref this.buttonsCount, nguiAction2);
			NGuiAction nguiAction3 = new NGuiAction(Localization.Get("leveltoolsShowPaintable", false), null, true);
			nguiAction3.SetClickActionDelegate(delegate
			{
				GameObject gameObject = GameObject.Find("/Chunks");
				if (gameObject == null)
				{
					return;
				}
				GameManager.bShowPaintables = !GameManager.bShowPaintables;
				this.enablePaintables(GameManager.bShowPaintables, gameObject.transform);
			});
			nguiAction3.SetIsCheckedDelegate(() => GameManager.bShowPaintables);
			this.SetButton(ref this.buttonsCount, nguiAction3);
			NGuiAction nguiAction4 = new NGuiAction(Localization.Get("leveltoolsShowTerrain", false), null, true);
			nguiAction4.SetClickActionDelegate(delegate
			{
				GameObject gameObject = GameObject.Find("/Chunks");
				if (gameObject == null)
				{
					return;
				}
				GameManager.bShowTerrain = !GameManager.bShowTerrain;
				this.enableTerrain(GameManager.bShowTerrain, gameObject.transform);
			});
			nguiAction4.SetIsCheckedDelegate(() => GameManager.bShowTerrain);
			this.SetButton(ref this.buttonsCount, nguiAction4);
			NGuiAction nguiAction5 = new NGuiAction(Localization.Get("leveltoolsShowDecor", false), null, true);
			nguiAction5.SetClickActionDelegate(delegate
			{
				GameManager.bShowDecorBlocks = !GameManager.bShowDecorBlocks;
				foreach (Chunk chunk in GameManager.Instance.World.ChunkCache.GetChunkArrayCopySync())
				{
					chunk.NeedsRegeneration = true;
				}
			});
			nguiAction5.SetIsCheckedDelegate(() => GameManager.bShowDecorBlocks);
			this.SetButton(ref this.buttonsCount, nguiAction5);
			NGuiAction nguiAction6 = new NGuiAction(Localization.Get("leveltoolsShowLoot", false), null, true);
			nguiAction6.SetClickActionDelegate(delegate
			{
				GameManager.bShowLootBlocks = !GameManager.bShowLootBlocks;
				foreach (Chunk chunk in GameManager.Instance.World.ChunkCache.GetChunkArrayCopySync())
				{
					chunk.NeedsRegeneration = true;
				}
			});
			nguiAction6.SetIsCheckedDelegate(() => GameManager.bShowLootBlocks);
			this.SetButton(ref this.buttonsCount, nguiAction6);
			NGuiAction nguiAction7 = new NGuiAction(Localization.Get("leveltoolsShowQuestLoot", false), null, true);
			nguiAction7.SetClickActionDelegate(delegate
			{
				PrefabEditModeManager.Instance.HighlightQuestLoot = !PrefabEditModeManager.Instance.HighlightQuestLoot;
			});
			nguiAction7.SetIsCheckedDelegate(() => PrefabEditModeManager.Instance.HighlightQuestLoot);
			this.SetButton(ref this.buttonsCount, nguiAction7);
			NGuiAction nguiAction8 = new NGuiAction(Localization.Get("leveltoolsShowBlockTriggers", false), null, true);
			nguiAction8.SetClickActionDelegate(delegate
			{
				PrefabEditModeManager.Instance.HighlightBlockTriggers = !PrefabEditModeManager.Instance.HighlightBlockTriggers;
			});
			nguiAction8.SetIsCheckedDelegate(() => PrefabEditModeManager.Instance.HighlightBlockTriggers);
			this.SetButton(ref this.buttonsCount, nguiAction8);
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

	[PublicizedFrom(EAccessModifier.Private)]
	public void enableUnpaintables(bool _bEnable, Transform _t)
	{
		for (int i = 0; i < _t.childCount; i++)
		{
			Transform child = _t.GetChild(i);
			if (child.name == "_BlockEntities")
			{
				child.gameObject.SetActive(_bEnable);
			}
			else if (child.name == "models" || child.name == "modelsCollider" || child.name == "cutout" || child.name == "cutoutCollider")
			{
				child.gameObject.SetActive(_bEnable);
			}
			else if (child.childCount > 0)
			{
				this.enableUnpaintables(_bEnable, child);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void enablePaintables(bool _bEnable, Transform _t)
	{
		for (int i = 0; i < _t.childCount; i++)
		{
			Transform child = _t.GetChild(i);
			if (child.name == "opaque" || child.name == "opaqueCollider")
			{
				child.gameObject.SetActive(_bEnable);
			}
			else if (child.childCount > 0)
			{
				this.enablePaintables(_bEnable, child);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void enableTerrain(bool _bEnable, Transform _t)
	{
		for (int i = 0; i < _t.childCount; i++)
		{
			Transform child = _t.GetChild(i);
			if (child.name == "terrain" || child.name == "terrainCollider")
			{
				child.gameObject.SetActive(_bEnable);
			}
			else if (child.childCount > 0)
			{
				this.enableTerrain(_bEnable, child);
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
}
