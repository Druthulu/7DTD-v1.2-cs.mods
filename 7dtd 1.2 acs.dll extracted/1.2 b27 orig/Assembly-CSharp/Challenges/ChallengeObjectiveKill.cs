using System;
using System.Xml.Linq;
using UnityEngine.Scripting;

namespace Challenges
{
	[Preserve]
	public class ChallengeObjectiveKill : BaseChallengeObjective
	{
		public override ChallengeObjectiveType ObjectiveType
		{
			get
			{
				return ChallengeObjectiveType.Kill;
			}
		}

		public override string DescriptionText
		{
			get
			{
				if (this.biome == "")
				{
					return Localization.Get("challengeObjectiveKill", false) + " " + Localization.Get(this.entityIDs, false) + ":";
				}
				return string.Format(Localization.Get("challengeObjectiveKillIn", false), Localization.Get(this.entityIDs, false), this.biome);
			}
		}

		public override void Init()
		{
			if (this.entityIDs != null)
			{
				string[] array = this.entityIDs.Split(',', StringSplitOptions.None);
				if (array.Length > 1)
				{
					this.entityIDs = array[0];
					this.entityNames = new string[array.Length - 1];
					for (int i = 1; i < array.Length; i++)
					{
						this.entityNames[i - 1] = array[i];
					}
				}
			}
		}

		public override void HandleAddHooks()
		{
			QuestEventManager.Current.EntityKill -= this.Current_EntityKill;
			QuestEventManager.Current.EntityKill += this.Current_EntityKill;
		}

		public override void HandleRemoveHooks()
		{
			QuestEventManager.Current.EntityKill -= this.Current_EntityKill;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void Current_EntityKill(EntityAlive killedBy, EntityAlive killedEntity)
		{
			string entityClassName = killedEntity.EntityClass.entityClassName;
			bool flag = false;
			if (this.biome != "" && this.Owner.Owner.Player.biomeStandingOn.m_sBiomeName != this.biome)
			{
				return;
			}
			if (this.entityIDs == null || entityClassName.EqualsCaseInsensitive(this.entityIDs))
			{
				flag = true;
			}
			if (!flag && this.entityNames != null)
			{
				for (int i = 0; i < this.entityNames.Length; i++)
				{
					if (this.entityNames[i].EqualsCaseInsensitive(entityClassName))
					{
						flag = true;
						break;
					}
				}
			}
			if (flag)
			{
				int num = base.Current;
				base.Current = num + 1;
				this.CheckObjectiveComplete(true);
			}
		}

		public override void ParseElement(XElement e)
		{
			base.ParseElement(e);
			if (e.HasAttribute("entity_names"))
			{
				this.entityIDs = e.GetAttribute("entity_names");
			}
			if (e.HasAttribute("biome"))
			{
				this.biome = e.GetAttribute("biome");
			}
		}

		public override BaseChallengeObjective Clone()
		{
			return new ChallengeObjectiveKill
			{
				entityIDs = this.entityIDs,
				entityNames = this.entityNames,
				biome = this.biome
			};
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public string entityIDs = "";

		[PublicizedFrom(EAccessModifier.Private)]
		public string[] entityNames;

		[PublicizedFrom(EAccessModifier.Private)]
		public string biome = "";
	}
}
