using System;
using System.Collections.Generic;
using System.Xml.Linq;
using MusicUtils.Enums;
using UnityEngine.Scripting;

namespace DynamicMusic
{
	[Preserve]
	public class BloodmoonConfiguration : AbstractConfiguration, IConfiguration<LayerState>, IConfiguration
	{
		public Dictionary<LayerType, LayerState> Layers { get; [PublicizedFrom(EAccessModifier.Private)] set; }

		public BloodmoonConfiguration()
		{
			this.Layers = new Dictionary<LayerType, LayerState>();
		}

		public override int CountFor(LayerType _layer)
		{
			if (!this.Layers.ContainsKey(_layer))
			{
				return 0;
			}
			return 1;
		}

		public override void ParseFromXml(XElement _xmlNode)
		{
			base.ParseFromXml(_xmlNode);
			foreach (XElement element in _xmlNode.Elements("layer"))
			{
				LayerType key = EnumUtils.Parse<LayerType>(element.GetAttribute("key"), false);
				float lo = float.Parse(element.GetAttribute("lo"));
				float hi = float.Parse(element.GetAttribute("hi"));
				this.Layers.Add(key, new LayerState((float tl) => BloodmoonConfiguration.getState(tl, lo, hi)));
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static LayerStateType getState(float _threatLevel, float _enabledThreshold, float _hiThreshold)
		{
			if (_threatLevel < _enabledThreshold)
			{
				return LayerStateType.disabled;
			}
			if (_threatLevel >= _hiThreshold)
			{
				return LayerStateType.hi;
			}
			return LayerStateType.lo;
		}
	}
}
