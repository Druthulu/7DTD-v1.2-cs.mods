using System;

public class XUiEventManager
{
	[PublicizedFrom(EAccessModifier.Private)]
	public XUiEventManager()
	{
	}

	public static XUiEventManager Instance
	{
		get
		{
			if (XUiEventManager.instance == null)
			{
				XUiEventManager.instance = new XUiEventManager();
			}
			return XUiEventManager.instance;
		}
	}

	public event XUiEventManager.XUiEvent_SkillExperienceAdded OnSkillExperienceAdded;

	public void SkillExperienceAdded(ProgressionValue skill, int newXP)
	{
		if (this.OnSkillExperienceAdded != null)
		{
			this.OnSkillExperienceAdded(skill, newXP);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static XUiEventManager instance;

	public delegate void XUiEvent_SkillExperienceAdded(ProgressionValue skill, int newXP);
}
