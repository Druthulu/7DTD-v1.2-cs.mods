﻿using System;
using System.Collections;
using System.Xml.Linq;
using MusicUtils.Enums;

namespace DynamicMusic
{
	public interface IClipAdapter
	{
		float GetSample(int idx, params float[] _params);

		bool IsLoaded { get; }

		IEnumerator Load();

		void LoadImmediate();

		void Unload();

		void SetPaths(int _num, PlacementType _placement, SectionType _section, LayerType _layer, string stress = "");

		void ParseXml(XElement _xmlNode);
	}
}
