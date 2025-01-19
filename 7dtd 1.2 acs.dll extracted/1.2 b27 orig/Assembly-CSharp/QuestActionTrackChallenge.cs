using System;
using UnityEngine.Scripting;

[Preserve]
public class QuestActionTrackChallenge : BaseQuestAction
{
	public override void SetupAction()
	{
	}

	public override void PerformAction(Quest ownerQuest)
	{
		EntityPlayerLocal ownerPlayer = ownerQuest.OwnerJournal.OwnerPlayer;
		XUi xui = ownerPlayer.PlayerUI.xui;
		if (xui.QuestTracker.TrackedQuest == null && xui.QuestTracker.TrackedChallenge == null && xui.Recipes.TrackedRecipe == null && ownerPlayer.challengeJournal.ChallengeDictionary.ContainsKey(this.ID))
		{
			xui.QuestTracker.TrackedChallenge = ownerPlayer.challengeJournal.ChallengeDictionary[this.ID];
		}
	}

	public override BaseQuestAction Clone()
	{
		QuestActionTrackChallenge questActionTrackChallenge = new QuestActionTrackChallenge();
		base.CopyValues(questActionTrackChallenge);
		return questActionTrackChallenge;
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		properties.ParseString(QuestActionTrackChallenge.PropChallenge, ref this.ID);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropChallenge = "challenge";
}
