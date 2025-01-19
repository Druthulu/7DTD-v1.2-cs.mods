using System;
using System.Threading;
using System.Windows.Forms;
using UnityEngine;
using UnityEngine.Profiling;

public class WinFormInstance : IConsoleServer
{
	public WinFormInstance()
	{
		try
		{
			this.windowThread = new Thread(new ThreadStart(this.windowThreadMain))
			{
				Name = "WinFormInstance"
			};
			this.windowThread.SetApartmentState(ApartmentState.STA);
			this.windowThread.Start();
			Thread.Sleep(250);
			Log.Out("Started Terminal Window");
		}
		catch (Exception ex)
		{
			string str = "Error in WinFormInstance.ctor: ";
			Exception ex2 = ex;
			Log.Out(str + ((ex2 != null) ? ex2.ToString() : null));
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void windowThreadMain()
	{
		this.form = new WinFormConnection(this);
		Log.Out("WinThread started");
		System.Windows.Forms.Application.ThreadException += this.ApplicationOnThreadException;
		System.Windows.Forms.Application.Run(this.form);
		Profiler.EndThreadProfiling();
		this.form = null;
		Log.Out("WinThread ended");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ApplicationOnThreadException(object _sender, ThreadExceptionEventArgs _threadExceptionEventArgs)
	{
		Log.Error("TerminalWindow Exeption:");
		Log.Exception(_threadExceptionEventArgs.Exception);
	}

	public void Disconnect()
	{
		if (this.form == null)
		{
			return;
		}
		Log.Out("Closing Terminal Window");
		WinFormConnection winFormConnection = this.form;
		this.form = null;
		winFormConnection.CloseTerminal();
		this.windowThread.Join();
		Log.Out("Ended Terminal Window");
	}

	public void SendLine(string _line)
	{
		if (_line != null && this.form != null)
		{
			this.form.SendLine(_line);
		}
	}

	public void SendLog(string _formattedMessage, string _plainMessage, string _trace, LogType _type, DateTime _timestamp, long _uptime)
	{
		if (_formattedMessage != null && this.form != null)
		{
			this.form.SendLog(_formattedMessage, _plainMessage, _trace, _type, _timestamp, _uptime);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Thread windowThread;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool stopThread;

	[PublicizedFrom(EAccessModifier.Private)]
	public WinFormConnection form;
}
