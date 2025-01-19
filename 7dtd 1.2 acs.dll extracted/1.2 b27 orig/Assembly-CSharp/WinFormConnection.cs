using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using UnityEngine;

public partial class WinFormConnection : Form, IConsoleConnection
{
	public WinFormConnection(WinFormInstance _owner)
	{
		this.initialize();
		ModEvents.GameStartDone.RegisterHandler(new Action(this.OnGameStartDone));
		ModEvents.GameAwake.RegisterHandler(new Action(this.OnGameAwake));
	}

	public void CloseTerminal()
	{
		if (base.InvokeRequired)
		{
			base.BeginInvoke(new Action(this.CloseTerminal));
			return;
		}
		this.forceClose = true;
		base.Close();
		System.Windows.Forms.Application.Exit();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnClosing(CancelEventArgs _e)
	{
		base.OnClosing(_e);
		if (this.forceClose)
		{
			return;
		}
		_e.Cancel = true;
		if (this.shutdownRequested)
		{
			return;
		}
		if (MessageBox.Show("Really shut down the 7 Days to Die server?", "Shutdown", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
		{
			return;
		}
		this.shutdownRequested = true;
		Log.Out("Shutdown game from Terminal Window");
		ThreadManager.AddSingleTaskMainThread("Shutdown", delegate(object _)
		{
			UnityEngine.Application.Quit();
		}, null);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnClosed(EventArgs _e)
	{
		base.OnClosed(_e);
		ModEvents.GameStartDone.UnregisterHandler(new Action(this.OnGameStartDone));
		ModEvents.GameAwake.UnregisterHandler(new Action(this.OnGameAwake));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void initialize()
	{
		base.SuspendLayout();
		base.ClientSize = new Size(1000, 600);
		this.Text = "Starting - 7 Days to Die Dedicated Server Console";
		base.Icon = new Icon(this.iconPath);
		this.BackColor = System.Drawing.Color.Black;
		this.ForeColor = WinFormConnection.logColorNormal;
		this.consoleOutputBox = new RichTextBox
		{
			Dock = DockStyle.Fill,
			Multiline = true,
			ScrollBars = RichTextBoxScrollBars.Both,
			Font = new System.Drawing.Font(FontFamily.GenericMonospace, 10f),
			ReadOnly = true,
			BackColor = this.BackColor,
			ForeColor = this.ForeColor,
			BorderStyle = BorderStyle.None
		};
		base.Controls.Add(this.consoleOutputBox);
		IntPtr handle = this.consoleOutputBox.Handle;
		this.commandInputBox = new TextBox
		{
			Dock = DockStyle.Bottom,
			Multiline = false,
			Text = "",
			Font = new System.Drawing.Font(FontFamily.GenericMonospace, 12f),
			Enabled = false,
			AutoCompleteMode = AutoCompleteMode.Append,
			AutoCompleteSource = AutoCompleteSource.CustomSource,
			BackColor = System.Drawing.Color.LightGray,
			ForeColor = System.Drawing.Color.Black,
			BorderStyle = BorderStyle.FixedSingle
		};
		this.commandInputBox.KeyDown += this.CommandInputBoxOnKeyDown;
		this.commandInputBox.AutoCompleteCustomSource = new AutoCompleteStringCollection();
		base.Controls.Add(this.commandInputBox);
		this.CreateHandle();
		base.ResumeLayout();
		base.PerformLayout();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnGameAwake()
	{
		this.Text = string.Format("{0} - Port {1} - Loading - 7 Days to Die Dedicated Server Console", GamePrefs.GetString(EnumGamePrefs.ServerName), GamePrefs.GetInt(EnumGamePrefs.ServerPort));
		foreach (IConsoleCommand consoleCommand in SingletonMonoBehaviour<SdtdConsole>.Instance.GetCommands())
		{
			this.commandInputBox.AutoCompleteCustomSource.AddRange(consoleCommand.GetCommands());
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnGameStartDone()
	{
		this.Text = string.Format("{0} - Port {1} - Running - 7 Days to Die Dedicated Server Console", GamePrefs.GetString(EnumGamePrefs.ServerName), GamePrefs.GetInt(EnumGamePrefs.ServerPort));
		this.commandInputBox.Enabled = true;
		this.commandInputBox.Clear();
		this.commandInputBox.Focus();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CommandInputBoxOnKeyDown(object _sender, KeyEventArgs _keyEventArgs)
	{
		if (_keyEventArgs.KeyCode == Keys.Return)
		{
			this.execCommand();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void execCommand()
	{
		if (this.commandInputBox.Enabled && this.commandInputBox.Text.Length > 0)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.ExecuteAsync(this.commandInputBox.Text, this);
			this.commandInputBox.Clear();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void AddLine(string _text, System.Drawing.Color _color)
	{
		if (this.consoleOutputBox.InvokeRequired)
		{
			if (this.addLineDelegate == null)
			{
				this.addLineDelegate = new Action<string, System.Drawing.Color>(this.AddLine);
			}
			base.BeginInvoke(this.addLineDelegate, new object[]
			{
				_text,
				_color
			});
			return;
		}
		this.consoleOutputBox.SelectionStart = this.consoleOutputBox.TextLength;
		this.consoleOutputBox.SelectionLength = 0;
		this.consoleOutputBox.SelectionColor = _color;
		this.consoleOutputBox.AppendText(_text + "\n");
		this.consoleOutputBox.SelectionColor = this.consoleOutputBox.ForeColor;
		if (this.consoleOutputBox.Lines.Length > 1000)
		{
			int num = this.consoleOutputBox.Lines.Length - 500;
			string[] array = new string[500];
			for (int i = 0; i < 500; i++)
			{
				array[i] = this.consoleOutputBox.Lines[i + num];
			}
			this.consoleOutputBox.Lines = array;
		}
		this.consoleOutputBox.SelectionStart = this.consoleOutputBox.TextLength;
		this.consoleOutputBox.ScrollToCaret();
	}

	public void SendLine(string _line)
	{
		this.AddLine(_line, WinFormConnection.logColorCommandReply);
	}

	public void SendLines(List<string> _output)
	{
		foreach (string line in _output)
		{
			this.SendLine(line);
		}
	}

	public void SendLog(string _formattedMessage, string _plainMessage, string _trace, LogType _type, DateTime _timestamp, long _uptime)
	{
		if (this.IsLogLevelEnabled(_type))
		{
			System.Drawing.Color color = WinFormConnection.logColorNormal;
			switch (_type)
			{
			case LogType.Error:
			case LogType.Assert:
			case LogType.Exception:
				color = WinFormConnection.logColorError;
				break;
			case LogType.Warning:
				color = WinFormConnection.logColorWarning;
				break;
			}
			this.AddLine(_formattedMessage, color);
		}
	}

	public void EnableLogLevel(LogType _type, bool _enable)
	{
		if (_enable)
		{
			this.enabledLogLevels.Add(_type);
			return;
		}
		this.enabledLogLevels.Remove(_type);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool IsLogLevelEnabled(LogType _type)
	{
		return this.enabledLogLevels.Contains(_type);
	}

	public string GetDescription()
	{
		return "Terminal Window";
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly string iconPath = GameIO.GetGameDir("Data") + "/7dtd_icon.ico";

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly HashSet<LogType> enabledLogLevels = new HashSet<LogType>
	{
		LogType.Log,
		LogType.Warning,
		LogType.Error,
		LogType.Exception,
		LogType.Assert
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public RichTextBox consoleOutputBox;

	[PublicizedFrom(EAccessModifier.Private)]
	public TextBox commandInputBox;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool forceClose;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool shutdownRequested;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int lineLimit = 500;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly System.Drawing.Color logColorCommandReply = System.Drawing.Color.LightCyan;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly System.Drawing.Color logColorNormal = System.Drawing.Color.LimeGreen;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly System.Drawing.Color logColorWarning = System.Drawing.Color.Yellow;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly System.Drawing.Color logColorError = System.Drawing.Color.Red;

	[PublicizedFrom(EAccessModifier.Private)]
	public Action<string, System.Drawing.Color> addLineDelegate;
}
