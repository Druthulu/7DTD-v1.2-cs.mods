using System;
using UnityEngine.Scripting;

namespace Challenges
{
	[Preserve]
	public class ChallengeObjectiveWindowOpen : BaseChallengeObjective
	{
		public override ChallengeObjectiveType ObjectiveType
		{
			get
			{
				return ChallengeObjectiveType.WindowOpen;
			}
		}

		public override string DescriptionText
		{
			get
			{
				return string.Format(Localization.Get("ObjectiveOpenWindow_keyword", false), string.Format("[DECEA3]{0}[-]", Localization.Get("xui" + this.WindowName, false)));
			}
		}

		public override void Init()
		{
		}

		public override void HandleAddHooks()
		{
			QuestEventManager.Current.WindowChanged -= this.Current_WindowChanged;
			QuestEventManager.Current.WindowChanged += this.Current_WindowChanged;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void Current_WindowChanged(string _windowName)
		{
			if (_windowName == "windowpaging")
			{
				return;
			}
			this.currentOpenWindow = _windowName;
			this.HandleUpdatingCurrent();
			this.CheckObjectiveComplete(true);
			this.Parent.CheckPrerequisites();
		}

		public override void HandleRemoveHooks()
		{
			QuestEventManager.Current.WindowChanged -= this.Current_WindowChanged;
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void HandleUpdatingCurrent()
		{
			base.HandleUpdatingCurrent();
			if (this.Owner != null)
			{
				base.Current = ((this.currentOpenWindow == this.WindowName) ? 1 : 0);
			}
		}

		public override BaseChallengeObjective Clone()
		{
			return new ChallengeObjectiveWindowOpen
			{
				WindowName = this.WindowName
			};
		}

		public string WindowName = "";

		[PublicizedFrom(EAccessModifier.Private)]
		public string currentOpenWindow = "";

		public RequirementObjectiveGroupWindowOpen Parent;
	}
}
