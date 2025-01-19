using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using InControl;
using UnityEngine;

public class GUIWindowScreenshotText : GUIWindow
{
	public GUIWindowScreenshotText() : base(GUIWindowScreenshotText.ID, new Rect(20f, 70f, (float)(Screen.width / 3), 1f))
	{
		this.alwaysUsesMouseCursor = true;
	}

	public override void OnGUI(bool _inputActive)
	{
		base.OnGUI(_inputActive);
		Vector2i vector2i = new Vector2i(Screen.width, Screen.height);
		if (this.lastResolution != vector2i)
		{
			this.lastResolution = vector2i;
			this.labelStyle = new GUIStyle(GUI.skin.label);
			this.checkboxStyle = new GUIStyle(GUI.skin.toggle);
			this.textfieldStyle = new GUIStyle(GUI.skin.textField);
			this.buttonStyle = new GUIStyle(GUI.skin.button);
			this.labelStyle.wordWrap = true;
			this.labelStyle.fontStyle = FontStyle.Bold;
			this.fontSize = vector2i.y / 54;
			this.lineHeight = this.fontSize + 3;
			this.inputAreaHeight = this.fontSize + 10;
			this.labelStyle.fontSize = this.fontSize;
			this.checkboxStyle.fontSize = this.fontSize;
			this.textfieldStyle.fontSize = this.fontSize;
			this.buttonStyle.fontSize = this.fontSize;
		}
		if (Event.current.type == EventType.KeyDown)
		{
			if (Event.current.keyCode == KeyCode.Escape)
			{
				this.CloseWindow();
			}
			else if (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter)
			{
				this.DoScreenshot();
			}
		}
		float xMin = base.windowRect.xMin;
		float yMin = base.windowRect.yMin;
		float width = base.windowRect.width;
		float num = 0f;
		for (int i = 0; i < 2; i++)
		{
			float num2 = yMin;
			if (i == 1)
			{
				GUI.Box(new Rect(xMin, num2, width, num - yMin + 5f), "");
			}
			if (GameManager.Instance.World != null)
			{
				World world = GameManager.Instance.World;
				GameUtils.WorldInfo worldInfo;
				if (world == null)
				{
					worldInfo = null;
				}
				else
				{
					ChunkCluster chunkCache = world.ChunkCache;
					if (chunkCache == null)
					{
						worldInfo = null;
					}
					else
					{
						IChunkProvider chunkProvider = chunkCache.ChunkProvider;
						worldInfo = ((chunkProvider != null) ? chunkProvider.WorldInfo : null);
					}
				}
				GameUtils.WorldInfo worldInfo2 = worldInfo;
				if (i == 1)
				{
					string text = "World: " + GamePrefs.GetString(EnumGamePrefs.GameWorld);
					Utils.DrawOutline(new Rect(xMin + 5f, num2, width - 10f, (float)this.inputAreaHeight), text, this.labelStyle, Color.black, Color.white);
				}
				num2 += (float)this.lineHeight;
				if (!PrefabEditModeManager.Instance.IsActive() && worldInfo2 != null && worldInfo2.RandomGeneratedWorld && worldInfo2.DynamicProperties.Contains("Generation.Seed"))
				{
					if (i == 1)
					{
						Utils.DrawOutline(new Rect(xMin + 5f, num2, width - 10f, (float)this.inputAreaHeight), "World gen seed: " + worldInfo2.DynamicProperties.GetStringValue("Generation.Seed"), this.labelStyle, Color.black, Color.white);
					}
					num2 += (float)this.lineHeight;
				}
				if (i == 1)
				{
					Utils.DrawOutline(new Rect(xMin + 5f, num2, width - 10f, (float)this.inputAreaHeight), "Save name / deco seed: " + (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer ? GamePrefs.GetString(EnumGamePrefs.GameName) : GamePrefs.GetString(EnumGamePrefs.GameNameClient)), this.labelStyle, Color.black, Color.white);
				}
				num2 += (float)this.lineHeight;
				if (LocalPlayerUI.GetUIForPrimaryPlayer() != null)
				{
					EntityPlayer entityPlayer = LocalPlayerUI.GetUIForPrimaryPlayer().entityPlayer;
					if (entityPlayer != null)
					{
						PrefabInstance prefab = entityPlayer.prefab;
						if (i == 1)
						{
							string text2 = string.Format("Coordinates: {0:F0} {1:F0} {2:F0}", entityPlayer.position.x, entityPlayer.position.y, entityPlayer.position.z);
							if (prefab != null)
							{
								text2 += string.Format(" / relative to POI: {0}", prefab.GetPositionRelativeToPoi(Vector3i.Floor(entityPlayer.position)));
							}
							Utils.DrawOutline(new Rect(xMin + 5f, num2, width - 10f, (float)this.inputAreaHeight), text2, this.labelStyle, Color.black, Color.white);
						}
						num2 += (float)this.lineHeight;
						if (prefab != null)
						{
							Prefab prefab2 = prefab.prefab;
							string text3 = ((prefab2 != null) ? prefab2.PrefabName : null) ?? prefab.name;
							Prefab prefab3 = prefab.prefab;
							string text4 = ((prefab3 != null) ? prefab3.LocalizedEnglishName : null) ?? "";
							if (i == 1)
							{
								Utils.DrawOutline(new Rect(xMin + 5f, num2, width - 10f, (float)this.inputAreaHeight), string.Concat(new string[]
								{
									"POI: ",
									text3,
									" (",
									text4,
									")"
								}), this.labelStyle, Color.black, Color.white);
							}
							num2 += (float)this.lineHeight;
						}
						if (!this.confirmed)
						{
							if (i == 1)
							{
								this.savePerks = GUI.Toggle(new Rect(xMin + 5f, num2, width - 10f, (float)this.lineHeight), this.savePerks, "Save Perks", this.checkboxStyle);
							}
							num2 += (float)this.lineHeight;
						}
					}
				}
				num2 += (float)this.lineHeight;
			}
			if (!this.confirmed)
			{
				if (i == 1)
				{
					GUI.SetNextControlName("InputField");
					this.noteInput = GUI.TextField(new Rect(xMin + 5f, num2, width - 60f, (float)this.inputAreaHeight), this.noteInput, 300, this.textfieldStyle);
					if (this.bFirstTime)
					{
						this.bFirstTime = false;
						GUI.FocusControl("InputField");
					}
					if (GUI.Button(new Rect(xMin + width - 50f, num2, 50f, (float)this.inputAreaHeight), "Ok", this.buttonStyle))
					{
						this.DoScreenshot();
						return;
					}
				}
				num2 += (float)this.inputAreaHeight;
			}
			else
			{
				float num3 = this.labelStyle.CalcHeight(new GUIContent("Note: " + this.noteInput), width - 10f);
				if (i == 1)
				{
					Utils.DrawOutline(new Rect(xMin + 5f, num2, width - 10f, num3 + 4f), "Note: " + this.noteInput, this.labelStyle, Color.black, Color.white);
				}
				num2 += num3;
			}
			num = num2;
		}
		if (this.nGuiWdwDebugPanels == null)
		{
			this.nGuiWdwDebugPanels = UnityEngine.Object.FindObjectOfType<NGuiWdwDebugPanels>();
		}
		if (this.nGuiWdwDebugPanels != null)
		{
			this.nGuiWdwDebugPanels.showDebugPanel_FocusedBlock((int)xMin, (int)num + 10, true);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CloseWindow()
	{
		this.windowManager.Close(this, false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DoScreenshot()
	{
		this.confirmed = true;
		ThreadManager.StartCoroutine(this.screenshotCo(null));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator screenshotCo(string _filename)
	{
		yield return null;
		bool saved = true;
		yield return ThreadManager.CoroutineWrapperWithExceptionCallback(GameUtils.TakeScreenshotEnum(GameUtils.EScreenshotMode.Both, _filename, 0f, false, 0, 0, false), delegate(Exception _exception)
		{
			saved = false;
			Log.Exception(_exception);
		});
		if (saved && this.savePerks)
		{
			this.StoreAdditionalStats();
		}
		yield return null;
		this.CloseWindow();
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void StoreAdditionalStats()
	{
		string text = GameUtils.lastSavedScreenshotFilename;
		text = text.Substring(0, text.LastIndexOf('.'));
		if (GameManager.Instance.World != null && GameManager.Instance.World.GetPrimaryPlayer() != null)
		{
			this.StorePlayerStats(text);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void StorePlayerStats(string _filenameBase)
	{
		EntityPlayerLocal primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
		_filenameBase += "_playerstats.csv";
		StringBuilder stringBuilder = new StringBuilder();
		List<ProgressionValue> list = new List<ProgressionValue>();
		foreach (KeyValuePair<int, ProgressionValue> keyValuePair in primaryPlayer.Progression.GetDict())
		{
			ProgressionValue value = keyValuePair.Value;
			bool flag;
			if (value == null)
			{
				flag = (null != null);
			}
			else
			{
				ProgressionClass progressionClass = value.ProgressionClass;
				flag = (((progressionClass != null) ? progressionClass.Name : null) != null);
			}
			if (flag)
			{
				list.Add(keyValuePair.Value);
			}
		}
		list.Sort(ProgressionClass.ListSortOrderComparer.Instance);
		stringBuilder.AppendLine(string.Format("Level,{0}", primaryPlayer.Progression.GetLevel()));
		stringBuilder.AppendLine();
		stringBuilder.AppendLine("Skills");
		foreach (ProgressionValue progressionValue in list)
		{
			ProgressionClass progressionClass2 = progressionValue.ProgressionClass;
			if (progressionClass2.IsAttribute && progressionClass2.MaxLevel != 0)
			{
				stringBuilder.AppendLine();
				stringBuilder.AppendLine(string.Format("{0},{1},{2}", progressionClass2.Name, progressionValue.Level, progressionValue.CalculatedLevel(primaryPlayer)));
			}
			else if (progressionClass2.IsPerk)
			{
				stringBuilder.AppendLine(string.Format(" - {0},{1},{2}", progressionClass2.Name, progressionValue.Level, progressionValue.CalculatedLevel(primaryPlayer)));
			}
		}
		stringBuilder.AppendLine();
		stringBuilder.AppendLine("Books");
		foreach (ProgressionValue progressionValue2 in list)
		{
			ProgressionClass progressionClass3 = progressionValue2.ProgressionClass;
			if (progressionClass3.IsBook)
			{
				stringBuilder.AppendLine(string.Format(" - {0},{1},{2}", progressionClass3.Name, progressionValue2.Level, progressionValue2.CalculatedLevel(primaryPlayer)));
			}
		}
		stringBuilder.AppendLine();
		stringBuilder.AppendLine("Crafting Skills");
		foreach (ProgressionValue progressionValue3 in list)
		{
			ProgressionClass progressionClass4 = progressionValue3.ProgressionClass;
			if (progressionClass4.IsCrafting)
			{
				stringBuilder.AppendLine(string.Format(" - {0},{1},{2}", progressionClass4.Name, progressionValue3.Level, progressionValue3.CalculatedLevel(primaryPlayer)));
			}
		}
		stringBuilder.AppendLine();
		SdFile.WriteAllText(_filenameBase, stringBuilder.ToString());
	}

	public override void OnOpen()
	{
		this.confirmed = false;
		this.bFirstTime = true;
		this.noteInput = "";
		this.isInputActive = true;
		if (UIInput.selection != null)
		{
			UIInput.selection.isSelected = false;
		}
		InputManager.Enabled = false;
	}

	public override void OnClose()
	{
		base.OnClose();
		this.isInputActive = false;
		InputManager.Enabled = true;
	}

	public static void Open(LocalPlayerUI _playerUi, bool _savePerks)
	{
		GUIWindowScreenshotText window = _playerUi.windowManager.GetWindow<GUIWindowScreenshotText>(GUIWindowScreenshotText.ID);
		if (window != null)
		{
			window.savePerks = _savePerks;
		}
		_playerUi.windowManager.Open(GUIWindowScreenshotText.ID, false, false, true);
	}

	public static readonly string ID = typeof(GUIWindowScreenshotText).Name;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bFirstTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public string noteInput;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool savePerks;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool confirmed;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2i lastResolution;

	[PublicizedFrom(EAccessModifier.Private)]
	public GUIStyle labelStyle;

	[PublicizedFrom(EAccessModifier.Private)]
	public GUIStyle checkboxStyle;

	[PublicizedFrom(EAccessModifier.Private)]
	public GUIStyle textfieldStyle;

	[PublicizedFrom(EAccessModifier.Private)]
	public GUIStyle buttonStyle;

	[PublicizedFrom(EAccessModifier.Private)]
	public int fontSize;

	[PublicizedFrom(EAccessModifier.Private)]
	public int lineHeight;

	[PublicizedFrom(EAccessModifier.Private)]
	public int inputAreaHeight;

	[PublicizedFrom(EAccessModifier.Private)]
	public NGuiWdwDebugPanels nGuiWdwDebugPanels;
}
