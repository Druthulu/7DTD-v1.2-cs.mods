using System;
using UnityEngine.Scripting;

[Preserve]
public class QuestEntry
{
	public QuestEntry(string questID, float prob, int startStage, int endStage)
	{
		this.QuestID = questID;
		this.Prob = prob;
		this.StartStage = startStage;
		this.EndStage = endStage;
		this.QuestClass = QuestClass.GetQuest(this.QuestID);
	}

	public float Prob = 1f;

	public int StartStage = -1;

	public int EndStage = -1;

	public string QuestID;

	public QuestClass QuestClass;
}
