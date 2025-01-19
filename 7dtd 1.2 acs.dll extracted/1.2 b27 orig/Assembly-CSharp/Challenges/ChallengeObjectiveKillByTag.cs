using System;
using System.Xml.Linq;
using UnityEngine.Scripting;

namespace Challenges
{
	[Preserve]
	public class ChallengeObjectiveKillByTag : BaseChallengeObjective
	{
		public override ChallengeObjectiveType ObjectiveType
		{
			get
			{
				return ChallengeObjectiveType.KillByTag;
			}
		}

		public override string DescriptionText
		{
			get
			{
				if (this.biome == "")
				{
					return Localization.Get("challengeObjectiveKill", false) + " " + Localization.Get(this.targetName, false) + ":";
				}
				return string.Format(Localization.Get("challengeObjectiveKillIn", false), Localization.Get(this.targetName, false), this.biome);
			}
		}

		public override void Init()
		{
			this.entityTags = FastTags<TagGroup.Global>.Parse(this.entityTag);
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
			if (!this.entityTags.Test_AnySet(killedEntity.EntityClass.Tags))
			{
				return;
			}
			if (this.biome != "" && this.Owner.Owner.Player.biomeStandingOn.m_sBiomeName != this.biome)
			{
				return;
			}
			if (this.isTwitchSpawn > -1)
			{
				if (this.isTwitchSpawn == 0 && killedEntity.spawnById != -1)
				{
					return;
				}
				if (this.isTwitchSpawn == 1 && killedEntity.spawnById == -1)
				{
					return;
				}
			}
			if (!this.killerHasBuffTag.IsEmpty && !killedBy.Buffs.HasBuffByTag(this.killerHasBuffTag))
			{
				return;
			}
			if (!this.killedHasBuffTag.IsEmpty && !killedEntity.Buffs.HasBuffByTag(this.killedHasBuffTag))
			{
				return;
			}
			int num = base.Current;
			base.Current = num + 1;
			this.CheckObjectiveComplete(true);
		}

		public override void ParseElement(XElement e)
		{
			base.ParseElement(e);
			if (e.HasAttribute("entity_tags"))
			{
				this.entityTag = e.GetAttribute("entity_tags");
			}
			if (e.HasAttribute("target_name_key"))
			{
				this.targetName = Localization.Get(e.GetAttribute("target_name_key"), false);
			}
			else if (e.HasAttribute("target_name"))
			{
				this.targetName = e.GetAttribute("target_name");
			}
			if (e.HasAttribute("biome"))
			{
				this.biome = e.GetAttribute("biome");
			}
			if (e.HasAttribute("is_twitch_spawn"))
			{
				this.isTwitchSpawn = (StringParsers.ParseBool(e.GetAttribute("is_twitch_spawn"), 0, -1, true) ? 1 : 0);
			}
			if (e.HasAttribute("killer_has_bufftag"))
			{
				this.killerHasBuffTag = FastTags<TagGroup.Global>.Parse(e.GetAttribute("killer_has_bufftag"));
			}
			if (e.HasAttribute("killed_has_bufftag"))
			{
				this.killedHasBuffTag = FastTags<TagGroup.Global>.Parse(e.GetAttribute("killed_has_bufftag"));
			}
		}

		public override BaseChallengeObjective Clone()
		{
			return new ChallengeObjectiveKillByTag
			{
				entityTag = this.entityTag,
				entityTags = this.entityTags,
				biome = this.biome,
				targetName = this.targetName,
				isTwitchSpawn = this.isTwitchSpawn,
				killerHasBuffTag = this.killerHasBuffTag,
				killedHasBuffTag = this.killedHasBuffTag
			};
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public string entityTag = "";

		[PublicizedFrom(EAccessModifier.Private)]
		public FastTags<TagGroup.Global> entityTags;

		[PublicizedFrom(EAccessModifier.Private)]
		public string targetName = "";

		[PublicizedFrom(EAccessModifier.Private)]
		public string biome = "";

		[PublicizedFrom(EAccessModifier.Private)]
		public int isTwitchSpawn = -1;

		[PublicizedFrom(EAccessModifier.Private)]
		public FastTags<TagGroup.Global> killerHasBuffTag;

		[PublicizedFrom(EAccessModifier.Private)]
		public FastTags<TagGroup.Global> killedHasBuffTag;
	}
}
