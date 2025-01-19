using System;
using System.Collections.Generic;
using InControl;
using Platform;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class GUIWindowConsole : GUIWindowUGUI
{
	public override string UIPrefabPath
	{
		get
		{
			return "GUI/Prefabs/ConsoleWindow";
		}
	}

	public GUIWindowConsole() : base(GUIWindowConsole.ID)
	{
		Log.LogCallbacks += this.LogCallback;
		this.alwaysUsesMouseCursor = true;
		this.components = this.canvas.GetComponent<GUIWindowConsoleComponents>();
		this.scrollRect = this.components.scrollRect;
		this.contentRect = this.components.contentRect;
		this.commandField = this.components.commandField;
		this.commandField.onSubmit.AddListener(new UnityAction<string>(this.EnterCommand));
		this.commandField.shouldActivateOnSelect = !TouchScreenKeyboard.isSupported;
		this.components.closeButton.onClick.AddListener(new UnityAction(this.CloseConsole));
		this.components.openLogsButton.onClick.AddListener(delegate()
		{
			GameIO.OpenExplorer(Application.consoleLogPath);
		});
		if ((DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5).IsCurrent())
		{
			this.components.openLogsButton.gameObject.SetActive(false);
		}
		for (int i = 0; i < 5; i++)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.components.consoleLinePrefab);
			gameObject.SetActive(false);
			gameObject.transform.SetParent(this.contentRect, false);
			this.linePool.Push(gameObject.GetComponent<Text>());
		}
		PlatformManager.NativePlatform.Input.OnLastInputStyleChanged += this.Input_OnLastInputStyleChanged;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Input_OnLastInputStyleChanged(PlayerInputManager.InputStyle _inputStyle)
	{
		if (_inputStyle == PlayerInputManager.InputStyle.Keyboard)
		{
			this.components.controllerPrompts.SetActive(false);
			return;
		}
		this.components.controllerPrompts.SetActive(true);
		this.components.RefreshButtonPrompts();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Text AllocText()
	{
		Text text;
		if (this.linePool.TryPop(out text))
		{
			text.gameObject.SetActive(true);
			return text;
		}
		GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.components.consoleLinePrefab);
		gameObject.transform.SetParent(this.contentRect, false);
		return gameObject.GetComponent<Text>();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void FreeText(Text _text)
	{
		_text.gameObject.SetActive(false);
		this.linePool.Push(_text);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void AddDisplayedLine(GUIWindowConsole.ConsoleLine _line)
	{
		foreach (StringSpan stringSpan in _line.text.GetSplitAnyEnumerator(GUIWindowConsole.lineSeparators, StringSplitOptions.RemoveEmptyEntries))
		{
			Text text;
			if (this.displayedLines.Count == 300)
			{
				text = this.displayedLines.Dequeue();
			}
			else
			{
				text = this.AllocText();
			}
			if (stringSpan.Length > 500)
			{
				text.text = SpanUtils.Concat(stringSpan.Slice(0, 500), "...");
			}
			else
			{
				text.text = stringSpan.ToString();
			}
			text.color = _line.GetLogColor();
			text.transform.SetAsLastSibling();
			this.displayedLines.Enqueue(text);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ClearDisplayedLines()
	{
		Text text;
		while (this.displayedLines.TryDequeue(out text))
		{
			this.FreeText(text);
		}
	}

	public void Shutdown()
	{
		Log.LogCallbacks -= this.LogCallback;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void LogCallback(string _msg, string _trace, LogType _type)
	{
		switch (_type)
		{
		case LogType.Assert:
			this.openConsole(_msg);
			break;
		case LogType.Exception:
			this.openConsole(_msg);
			break;
		}
		this.internalAddLine(new GUIWindowConsole.ConsoleLine(_msg, _trace, _type));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void openConsole(string _logString)
	{
		if (Submission.Enabled)
		{
			return;
		}
		if (_logString.StartsWith("Can't send RPC"))
		{
			return;
		}
		if (_logString.StartsWith("You are trying to load data from"))
		{
			return;
		}
		this.windowManager.OpenIfNotOpen(GUIWindowConsole.ID, false, false, true);
	}

	public void AddLines(string[] _lines)
	{
		for (int i = 0; i < _lines.Length; i++)
		{
			this.AddLine(_lines[i]);
		}
	}

	public void AddLines(List<string> _lines)
	{
		for (int i = 0; i < _lines.Count; i++)
		{
			this.AddLine(_lines[i]);
		}
	}

	public void AddLine(string _line)
	{
		this.internalAddLine(new GUIWindowConsole.ConsoleLine(_line, string.Empty, LogType.Log));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void internalAddLine(GUIWindowConsole.ConsoleLine consoleLine)
	{
		Queue<GUIWindowConsole.ConsoleLine> obj = this.linesToAdd;
		lock (obj)
		{
			this.linesToAdd.Enqueue(consoleLine);
			while (this.linesToAdd.Count > 300)
			{
				this.linesToAdd.Dequeue();
			}
		}
	}

	public override void Update()
	{
		base.Update();
		this.scrolledToBottom = (this.scrollRect.verticalNormalizedPosition < 0.1f);
		bool flag = false;
		Queue<GUIWindowConsole.ConsoleLine> obj = this.linesToAdd;
		lock (obj)
		{
			if (this.linesToAdd.Count > 0)
			{
				flag = true;
				foreach (GUIWindowConsole.ConsoleLine line in this.linesToAdd)
				{
					this.AddDisplayedLine(line);
				}
				this.linesToAdd.Clear();
			}
		}
		if (flag && this.scrolledToBottom)
		{
			Canvas.ForceUpdateCanvases();
			this.scrollRect.verticalNormalizedPosition = 0f;
		}
		if (this.bFirstTime)
		{
			if (!TouchScreenKeyboard.isSupported)
			{
				this.commandField.Select();
				this.commandField.ActivateInputField();
			}
			this.scrollRect.verticalNormalizedPosition = 0f;
			this.bFirstTime = false;
		}
		if (this.bUpdateCursor)
		{
			this.commandField.MoveTextEnd(false);
			this.bUpdateCursor = false;
		}
		if (Input.GetKeyDown(KeyCode.UpArrow))
		{
			this.PreviousCommand();
		}
		else if (Input.GetKeyDown(KeyCode.DownArrow))
		{
			this.NextCommand();
		}
		else if (Input.GetKeyDown(KeyCode.PageUp))
		{
			float num = this.CalculateNormalizedPageSize();
			this.scrollRect.verticalNormalizedPosition = Math.Min(this.scrollRect.verticalNormalizedPosition + num, 1f);
		}
		else if (Input.GetKeyDown(KeyCode.PageDown))
		{
			float num2 = this.CalculateNormalizedPageSize();
			this.scrollRect.verticalNormalizedPosition = Math.Max(this.scrollRect.verticalNormalizedPosition - num2, 0f);
		}
		PlayerActionsLocal playerInput = this.playerUI.playerInput;
		PlayerActionsGUI playerActionsGUI = (playerInput != null) ? playerInput.GUIActions : null;
		if (playerActionsGUI == null)
		{
			return;
		}
		if (playerActionsGUI.Submit.WasPressed)
		{
			this.EnterCommand(this.commandField.text);
		}
		else if (playerActionsGUI.DPad_Up.WasPressed && playerActionsGUI.DPad_Up.LastDeviceClass != InputDeviceClass.Keyboard)
		{
			this.PreviousCommand();
		}
		else if (playerActionsGUI.DPad_Down.WasPressed && playerActionsGUI.DPad_Down.LastDeviceClass != InputDeviceClass.Keyboard)
		{
			this.NextCommand();
		}
		else if (playerActionsGUI.DPad_Left.WasPressed && playerActionsGUI.DPad_Down.LastDeviceClass != InputDeviceClass.Keyboard)
		{
			IVirtualKeyboard virtualKeyboard = PlatformManager.NativePlatform.VirtualKeyboard;
			if (virtualKeyboard != null)
			{
				virtualKeyboard.Open("Enter Command", this.commandField.text, new Action<bool, string>(this.OnTextReceived), UIInput.InputType.Standard, false);
			}
		}
		else if (this.playerUI.playerInput.PermanentActions.Cancel.WasReleased || PlayerActionsGlobal.Instance.Console.WasPressed)
		{
			this.CloseConsole();
		}
		float y = playerActionsGUI.Camera.Vector.y;
		if (y != 0f)
		{
			float num3 = this.CalculateNormalizedPageSize();
			this.scrollRect.verticalNormalizedPosition = Math.Max(this.scrollRect.verticalNormalizedPosition + num3 * y * 0.05f, 0f);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public float CalculateNormalizedPageSize()
	{
		float height = this.scrollRect.viewport.rect.height;
		float num = this.scrollRect.content.rect.height - height;
		if (num > height)
		{
			return Math.Max(height / num, 0.01f);
		}
		return 1f;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnTextReceived(bool _success, string _text)
	{
		if (_success)
		{
			this.commandField.text = _text;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CloseConsole()
	{
		this.windowManager.Close(this, false);
		this.commandField.text = string.Empty;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void EnterCommand(string _command)
	{
		if (_command.Length > 0)
		{
			if (_command == "clear")
			{
				this.Clear();
			}
			else
			{
				this.scrollRect.verticalNormalizedPosition = 0f;
				this.internalAddLine(new GUIWindowConsole.ConsoleLine("> " + _command, string.Empty, LogType.Log));
				if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsClient)
				{
					this.AddLines(SingletonMonoBehaviour<SdtdConsole>.Instance.ExecuteSync(_command, null));
				}
				else
				{
					SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageConsoleCmdServer>().Setup(_command), false);
				}
			}
			if (this.lastCommands.Count == 0 || !this.lastCommands[this.lastCommands.Count - 1].Equals(_command))
			{
				if (this.lastCommands.Contains(_command))
				{
					this.lastCommands.Remove(_command);
				}
				this.lastCommands.Add(_command);
			}
			this.lastCommandsIdx = this.lastCommands.Count;
			this.commandField.text = "";
			if (!TouchScreenKeyboard.isSupported)
			{
				this.commandField.Select();
				this.commandField.ActivateInputField();
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PreviousCommand()
	{
		if (this.lastCommands.Count > 0)
		{
			this.lastCommandsIdx = Mathf.Max(0, this.lastCommandsIdx - 1);
			this.commandField.text = this.lastCommands[this.lastCommandsIdx];
			if (!TouchScreenKeyboard.isSupported)
			{
				this.commandField.Select();
				this.commandField.ActivateInputField();
			}
			this.bUpdateCursor = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void NextCommand()
	{
		if (this.lastCommands.Count > 0)
		{
			this.lastCommandsIdx = Mathf.Min(this.lastCommands.Count, this.lastCommandsIdx + 1);
			if (this.lastCommandsIdx < this.lastCommands.Count)
			{
				this.commandField.text = this.lastCommands[this.lastCommandsIdx];
				this.bUpdateCursor = true;
				if (!TouchScreenKeyboard.isSupported)
				{
					this.commandField.Select();
					this.commandField.ActivateInputField();
					return;
				}
			}
			else
			{
				this.commandField.text = string.Empty;
			}
		}
	}

	public void Clear()
	{
		this.ClearDisplayedLines();
	}

	public override void OnOpen()
	{
		base.OnOpen();
		if (GamePrefs.GetBool(EnumGamePrefs.DebugMenuEnabled) && this.windowManager.IsWindowOpen(XUiC_InGameDebugMenu.ID))
		{
			this.bShouldReopenGebugMenu = true;
			this.windowManager.Close(XUiC_InGameDebugMenu.ID);
		}
		else
		{
			this.bShouldReopenGebugMenu = false;
		}
		this.commandField.text = string.Empty;
		this.bFirstTime = true;
		this.isInputActive = true;
		if (UIInput.selection != null)
		{
			UIInput.selection.isSelected = false;
		}
	}

	public override void OnClose()
	{
		this.scrollRect.verticalNormalizedPosition = 0f;
		base.OnClose();
		if (GamePrefs.GetBool(EnumGamePrefs.DebugMenuEnabled) && this.bShouldReopenGebugMenu)
		{
			this.windowManager.Open(XUiC_InGameDebugMenu.ID, false, false, true);
		}
		this.bShouldReopenGebugMenu = false;
		this.isInputActive = false;
	}

	public static string ID = typeof(GUIWindowConsole).Name;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool scrolledToBottom = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public Queue<GUIWindowConsole.ConsoleLine> linesToAdd = new Queue<GUIWindowConsole.ConsoleLine>(301);

	[PublicizedFrom(EAccessModifier.Private)]
	public Queue<Text> displayedLines = new Queue<Text>();

	[PublicizedFrom(EAccessModifier.Private)]
	public const int maxConsoleLines = 300;

	[PublicizedFrom(EAccessModifier.Private)]
	public Stack<Text> linePool = new Stack<Text>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<string> lastCommands = new List<string>();

	[PublicizedFrom(EAccessModifier.Private)]
	public int lastCommandsIdx;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bFirstTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bUpdateCursor;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bShouldReopenGebugMenu;

	[PublicizedFrom(EAccessModifier.Private)]
	public GUIWindowConsoleComponents components;

	[PublicizedFrom(EAccessModifier.Private)]
	public ScrollRect scrollRect;

	[PublicizedFrom(EAccessModifier.Private)]
	public Transform contentRect;

	[PublicizedFrom(EAccessModifier.Private)]
	public InputField commandField;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly string[] lineSeparators = new string[]
	{
		"\r\n",
		"\r",
		"\n"
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public struct ConsoleLine
	{
		public ConsoleLine(string _text, string _stackTrace, LogType _type)
		{
			this.text = _text;
			this.stackTrace = _stackTrace;
			this.type = _type;
		}

		public Color GetLogColor()
		{
			switch (this.type)
			{
			case LogType.Error:
			case LogType.Assert:
			case LogType.Exception:
				return Color.red;
			case LogType.Warning:
				return Color.yellow;
			case LogType.Log:
				return Color.white;
			default:
				return Color.white;
			}
		}

		public string text;

		public LogType type;

		public string stackTrace;
	}
}
