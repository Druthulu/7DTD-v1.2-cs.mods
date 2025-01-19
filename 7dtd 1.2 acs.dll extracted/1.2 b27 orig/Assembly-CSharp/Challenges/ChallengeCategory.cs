using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Challenges
{
	public class ChallengeCategory
	{
		public ChallengeCategory(string name)
		{
			this.Name = name;
		}

		public bool CanShow(EntityPlayer player)
		{
			return this.showType != ChallengeCategory.ShowTypes.Twitch || player.TwitchEnabled;
		}

		public void ParseElement(XElement e)
		{
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
			if (e.HasAttribute("icon"))
			{
				this.Icon = e.GetAttribute("icon");
			}
			if (e.HasAttribute("show_type"))
			{
				this.showType = (ChallengeCategory.ShowTypes)Enum.Parse(typeof(ChallengeCategory.ShowTypes), e.GetAttribute("show_type"), true);
			}
		}

		public static Dictionary<string, ChallengeCategory> s_ChallengeCategories = new CaseInsensitiveStringDictionary<ChallengeCategory>();

		public string Name;

		public string Icon;

		public string Title;

		[PublicizedFrom(EAccessModifier.Private)]
		public ChallengeCategory.ShowTypes showType;

		public enum ShowTypes
		{
			Normal,
			Twitch
		}
	}
}
