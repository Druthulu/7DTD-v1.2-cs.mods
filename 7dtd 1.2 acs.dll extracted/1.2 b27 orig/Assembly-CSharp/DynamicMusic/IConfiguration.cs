﻿using System;
using System.Collections.Generic;
using System.Xml.Linq;
using MusicUtils.Enums;

namespace DynamicMusic
{
	public interface IConfiguration
	{
		IList<SectionType> Sections { get; }

		int CountFor(LayerType _layer);

		void ParseFromXml(XElement _xmlNode);
	}
}
