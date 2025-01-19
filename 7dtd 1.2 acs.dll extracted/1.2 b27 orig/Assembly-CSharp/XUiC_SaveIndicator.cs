using System;
using System.Diagnostics;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_SaveIndicator : XUiController
{
	public override void Init()
	{
		base.Init();
		this.m_window = (XUiV_Window)base.ViewComponent;
		this.m_tailTimer = new Stopwatch();
		this.m_saveDataManager = SaveDataUtils.SaveDataManager;
		this.m_saveDataManager.CommitStarted += this.OnCommitStarted;
		this.m_saveDataManager.CommitFinished += this.OnCommitFinished;
		this.ID = base.WindowGroup.ID;
		base.xui.saveIndicator = this;
		this.m_window.TargetAlpha = 0.0015f;
	}

	public override void Cleanup()
	{
		base.Cleanup();
		if (this.m_saveDataManager != null)
		{
			this.m_saveDataManager.CommitStarted -= this.OnCommitStarted;
			this.m_saveDataManager.CommitFinished -= this.OnCommitFinished;
			this.m_saveDataManager = null;
		}
		this.m_window = null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnCommitStarted()
	{
		this.m_commitInProgress = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnCommitFinished()
	{
		this.m_tailTimer.Restart();
		this.m_commitInProgress = false;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (!this.m_commitInProgress && this.m_tailTimer.IsRunning && this.m_tailTimer.Elapsed >= XUiC_SaveIndicator.TailDuration)
		{
			this.m_tailTimer.Stop();
		}
		bool flag = this.m_commitInProgress || this.m_tailTimer.IsRunning;
		this.m_window.TargetAlpha = (flag ? 1f : 0.0015f);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly TimeSpan TailDuration = TimeSpan.FromSeconds(2.0);

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Window m_window;

	[PublicizedFrom(EAccessModifier.Private)]
	public ISaveDataManager m_saveDataManager;

	[PublicizedFrom(EAccessModifier.Private)]
	public Stopwatch m_tailTimer;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool m_commitInProgress;

	public string ID = "";
}
