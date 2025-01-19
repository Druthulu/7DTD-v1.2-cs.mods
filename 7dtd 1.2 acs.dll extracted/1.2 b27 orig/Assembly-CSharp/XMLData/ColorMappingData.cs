using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

namespace XMLData
{
	[Preserve]
	public class ColorMappingData : IXMLData
	{
		public static ColorMappingData Instance
		{
			get
			{
				ColorMappingData result;
				if ((result = ColorMappingData.instance) == null)
				{
					result = (ColorMappingData.instance = new ColorMappingData());
				}
				return result;
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public ColorMappingData()
		{
			this.IDFromName = new Dictionary<string, int>();
			this.NameFromID = new Dictionary<int, string>();
			this.ColorFromID = new Dictionary<int, Color>();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static ColorMappingData instance;

		public Dictionary<string, int> IDFromName;

		public Dictionary<int, string> NameFromID;

		public Dictionary<int, Color> ColorFromID;
	}
}
