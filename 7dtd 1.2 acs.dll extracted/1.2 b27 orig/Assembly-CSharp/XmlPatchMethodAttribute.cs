using System;

[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public class XmlPatchMethodAttribute : Attribute
{
	public XmlPatchMethodAttribute(string _patchName)
	{
		this.PatchName = _patchName;
	}

	public XmlPatchMethodAttribute(string _patchName, bool _requiresXpath)
	{
		this.PatchName = _patchName;
		this.RequiresXpath = _requiresXpath;
	}

	public readonly string PatchName;

	public readonly bool RequiresXpath = true;
}
