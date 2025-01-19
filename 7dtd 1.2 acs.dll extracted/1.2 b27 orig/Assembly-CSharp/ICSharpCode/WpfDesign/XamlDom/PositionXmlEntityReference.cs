﻿using System;
using System.Xml;

namespace ICSharpCode.WpfDesign.XamlDom
{
	public class PositionXmlEntityReference : XmlEntityReference, IXmlLineInfo
	{
		[PublicizedFrom(EAccessModifier.Internal)]
		public PositionXmlEntityReference(string name, XmlDocument doc, IXmlLineInfo lineInfo) : base(name, doc)
		{
			if (lineInfo != null)
			{
				this.lineNumber = lineInfo.LineNumber;
				this.linePosition = lineInfo.LinePosition;
				this.hasLineInfo = true;
			}
		}

		public bool HasLineInfo()
		{
			return this.hasLineInfo;
		}

		public int LineNumber
		{
			get
			{
				return this.lineNumber;
			}
		}

		public int LinePosition
		{
			get
			{
				return this.linePosition;
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public int lineNumber;

		[PublicizedFrom(EAccessModifier.Private)]
		public int linePosition;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool hasLineInfo;
	}
}
