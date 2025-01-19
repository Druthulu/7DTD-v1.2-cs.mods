using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Platform;
using UnityEngine.Scripting;

namespace Challenges
{
	[Preserve]
	public class ChallengeClass
	{
		public int OrderIndex { get; [PublicizedFrom(EAccessModifier.Private)] set; }

		public bool HasNavType
		{
			get
			{
				return this.GetNavType() > ChallengeClass.UINavTypes.None;
			}
		}

		public ChallengeClass(string name)
		{
			this.Name = name;
			this.OrderIndex = ChallengeClass.nextIndex++;
		}

		public static ChallengeClass NewClass(string id)
		{
			if (ChallengeClass.s_Challenges.ContainsKey(id))
			{
				return null;
			}
			ChallengeClass challengeClass = new ChallengeClass(id.ToLower());
			ChallengeClass.s_Challenges[id] = challengeClass;
			return challengeClass;
		}

		public static void Cleanup()
		{
			ChallengeClass.s_Challenges.Clear();
		}

		public static void InitChallenges()
		{
			foreach (string key in ChallengeClass.s_Challenges.Keys)
			{
				ChallengeClass.s_Challenges[key].Init();
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void Init()
		{
			for (int i = 0; i < this.ObjectiveList.Count; i++)
			{
				this.ObjectiveList[i].BaseInit();
			}
		}

		public bool HasEventsOrPassives()
		{
			return this.Effects != null;
		}

		public void ModifyValue(EntityAlive _ea, PassiveEffects _effect, ref float _base_value, ref float _perc_value, FastTags<TagGroup.Global> _tags = default(FastTags<TagGroup.Global>))
		{
			if (this.Effects == null)
			{
				return;
			}
			this.Effects.ModifyValue(_ea, _effect, ref _base_value, ref _perc_value, 0f, _tags, 1);
		}

		public static ChallengeClass GetChallenge(string name)
		{
			if (ChallengeClass.s_Challenges.ContainsKey(name))
			{
				return ChallengeClass.s_Challenges[name];
			}
			return null;
		}

		public Challenge CreateChallenge(ChallengeJournal ownerJournal)
		{
			Challenge challenge = new Challenge();
			challenge.ChallengeClass = this;
			challenge.Owner = ownerJournal;
			for (int i = 0; i < this.ObjectiveList.Count; i++)
			{
				BaseChallengeObjective baseChallengeObjective = this.ObjectiveList[i];
				BaseChallengeObjective baseChallengeObjective2 = baseChallengeObjective.Clone();
				baseChallengeObjective2.Owner = challenge;
				baseChallengeObjective2.IsRequirement = baseChallengeObjective.IsRequirement;
				baseChallengeObjective2.MaxCount = baseChallengeObjective.MaxCount;
				baseChallengeObjective2.ShowRequirements = baseChallengeObjective.ShowRequirements;
				baseChallengeObjective2.HandleOnCreated();
				challenge.ObjectiveList.Add(baseChallengeObjective2);
			}
			return challenge;
		}

		public string GetNextChallengeName()
		{
			if (this.NextChallenge != null)
			{
				return this.NextChallenge.Name;
			}
			return "";
		}

		public void AddObjective(BaseChallengeObjective objective)
		{
			this.ObjectiveList.Add(objective);
			objective.OwnerClass = this;
		}

		public bool ResetObjectives(Challenge challenge)
		{
			if (challenge.ObjectiveList.Count != this.ObjectiveList.Count)
			{
				return false;
			}
			for (int i = 0; i < this.ObjectiveList.Count; i++)
			{
				BaseChallengeObjective baseChallengeObjective = this.ObjectiveList[i];
				BaseChallengeObjective obj = challenge.ObjectiveList[i];
				if (baseChallengeObjective.GetType() != challenge.ObjectiveList[i].GetType())
				{
					return false;
				}
				BaseChallengeObjective baseChallengeObjective2 = baseChallengeObjective.Clone();
				baseChallengeObjective2.Owner = challenge;
				baseChallengeObjective2.IsRequirement = baseChallengeObjective.IsRequirement;
				baseChallengeObjective2.MaxCount = baseChallengeObjective.MaxCount;
				baseChallengeObjective2.ShowRequirements = baseChallengeObjective.ShowRequirements;
				baseChallengeObjective2.HandleOnCreated();
				baseChallengeObjective2.CopyValues(obj, baseChallengeObjective);
				challenge.ObjectiveList[i] = baseChallengeObjective2;
			}
			return true;
		}

		public string GetHint(bool isPreReq)
		{
			if (this.ChallengeHint == null)
			{
				return "";
			}
			if (isPreReq)
			{
				if (PlatformManager.NativePlatform.Input.CurrentInputStyle != PlayerInputManager.InputStyle.Keyboard)
				{
					string key = this.PreReqChallengeHint + "_alt";
					if (Localization.Exists(key, false))
					{
						return Localization.Get(key, false);
					}
				}
				return Localization.Get(this.PreReqChallengeHint, false);
			}
			if (PlatformManager.NativePlatform.Input.CurrentInputStyle != PlayerInputManager.InputStyle.Keyboard)
			{
				string key2 = this.ChallengeHint + "_alt";
				if (Localization.Exists(key2, false))
				{
					return Localization.Get(key2, false);
				}
			}
			return Localization.Get(this.ChallengeHint, false);
		}

		public string GetDescription()
		{
			if (PlatformManager.NativePlatform.Input.CurrentInputStyle != PlayerInputManager.InputStyle.Keyboard)
			{
				string key = this.Description + "_alt";
				if (Localization.Exists(key, false))
				{
					return Localization.Get(key, false);
				}
			}
			return Localization.Get(this.Description, false);
		}

		public void FireEvent(MinEventTypes _eventType, MinEventParams _params)
		{
			if (this.Effects != null)
			{
				this.Effects.FireEvent(_eventType, _params);
			}
		}

		public void ParseElement(XElement e)
		{
			if (e.HasAttribute("icon"))
			{
				this.Icon = e.GetAttribute("icon");
			}
			if (e.HasAttribute("title_key"))
			{
				this.Title = Localization.Get(e.GetAttribute("title_key"), false);
			}
			else if (e.HasAttribute("title"))
			{
				this.Title = e.GetAttribute("title");
			}
			else
			{
				this.Title = this.Name;
			}
			if (e.HasAttribute("group"))
			{
				ChallengeGroup challengeGroup = ChallengeGroup.s_ChallengeGroups[e.GetAttribute("group")];
				this.ChallengeGroup = challengeGroup;
				challengeGroup.AddChallenge(this);
			}
			if (e.HasAttribute("prerequisite_hint"))
			{
				this.PreReqChallengeHint = e.GetAttribute("prerequisite_hint");
			}
			if (e.HasAttribute("hint"))
			{
				this.ChallengeHint = e.GetAttribute("hint");
			}
			if (e.HasAttribute("short_description_key"))
			{
				this.ShortDescription = Localization.Get(e.GetAttribute("short_description_key"), false);
			}
			else if (e.HasAttribute("short_description"))
			{
				this.ShortDescription = e.GetAttribute("short_description");
			}
			if (e.HasAttribute("description_key"))
			{
				this.Description = e.GetAttribute("description_key");
			}
			else if (e.HasAttribute("description"))
			{
				this.Description = e.GetAttribute("description");
			}
			if (e.HasAttribute("reward_event"))
			{
				this.RewardEvent = e.GetAttribute("reward_event");
			}
			else
			{
				this.RewardEvent = ChallengesFromXml.DefaultRewardEvent;
			}
			if (e.HasAttribute("reward_text_key"))
			{
				this.RewardText = Localization.Get(e.GetAttribute("reward_text_key"), false);
			}
			else if (e.HasAttribute("reward_text"))
			{
				this.RewardText = e.GetAttribute("reward_text");
			}
			else
			{
				this.RewardText = ChallengesFromXml.DefaultRewardText;
			}
			if (e.HasAttribute("tags"))
			{
				this.TagName = e.GetAttribute("tags");
				this.Tags = FastTags<TagGroup.Global>.Parse(this.TagName);
			}
			if (e.HasAttribute("redeem_always"))
			{
				this.RedeemAlways = StringParsers.ParseBool(e.GetAttribute("redeem_always"), 0, -1, true);
			}
		}

		public ChallengeClass.UINavTypes GetNavType()
		{
			for (int i = 0; i < this.ObjectiveList.Count; i++)
			{
				BaseChallengeObjective baseChallengeObjective = this.ObjectiveList[i];
				if (baseChallengeObjective.NavType != ChallengeClass.UINavTypes.None)
				{
					return baseChallengeObjective.NavType;
				}
			}
			return ChallengeClass.UINavTypes.None;
		}

		public static Dictionary<string, ChallengeClass> s_Challenges = new CaseInsensitiveStringDictionary<ChallengeClass>();

		public string Name;

		public string Title;

		public string Icon;

		public ChallengeGroup ChallengeGroup;

		public string ShortDescription;

		public string Description;

		public string PreReqChallengeHint;

		public string ChallengeHint;

		public string RewardEvent;

		public string RewardText = "";

		public string TagName = string.Empty;

		public FastTags<TagGroup.Global> Tags = FastTags<TagGroup.Global>.none;

		public List<BaseChallengeObjective> ObjectiveList = new List<BaseChallengeObjective>();

		public ChallengeClass NextChallenge;

		[PublicizedFrom(EAccessModifier.Private)]
		public static int nextIndex = 0;

		public bool RedeemAlways;

		public MinEffectController Effects;

		public enum UINavTypes
		{
			None,
			Crafting,
			TwitchActions
		}
	}
}
