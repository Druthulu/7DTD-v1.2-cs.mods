using System;
using System.IO;
using System.Xml.Linq;

public static class SdXDocument
{
	public static XDocument Load(string filename)
	{
		XDocument result;
		using (Stream stream = SdFile.OpenRead(filename))
		{
			result = XDocument.Load(stream);
		}
		return result;
	}
}
