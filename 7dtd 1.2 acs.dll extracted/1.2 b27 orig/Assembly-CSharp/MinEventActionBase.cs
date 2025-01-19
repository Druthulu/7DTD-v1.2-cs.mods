using System;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine.Scripting;

[Preserve]
public class MinEventActionBase : IMinEventAction
{
	public MinEventActionBase()
	{
		this.Requirements = new List<IRequirement>();
	}

	public virtual void GetInfoStrings(ref List<string> list)
	{
		list.Add(this.EventType.ToStringCached<MinEventTypes>() + ": " + this.ToString());
		if (this.Requirements != null)
		{
			for (int i = 0; i < this.Requirements.Count; i++)
			{
				this.Requirements[i].GetInfoStrings(ref list);
			}
		}
	}

	public virtual void Execute(MinEventParams _params)
	{
	}

	public virtual bool CanExecute(MinEventTypes _eventType, MinEventParams _params)
	{
		if (this.Requirements.Count > 0)
		{
			bool flag = true;
			if (!this.OrCompare)
			{
				for (int i = 0; i < this.Requirements.Count; i++)
				{
					flag &= this.Requirements[i].IsValid(_params);
					if (!flag)
					{
						return false;
					}
				}
			}
			else
			{
				for (int j = 0; j < this.Requirements.Count; j++)
				{
					flag = this.Requirements[j].IsValid(_params);
					if (flag)
					{
						break;
					}
				}
				if (!flag)
				{
					return false;
				}
			}
			return flag;
		}
		return true;
	}

	public virtual bool ParseXmlAttribute(XAttribute _attribute)
	{
		string localName = _attribute.Name.LocalName;
		if (localName == "trigger")
		{
			this.EventType = EnumUtils.Parse<MinEventTypes>(_attribute.Value, false);
			return true;
		}
		if (localName == "anytrue")
		{
			this.OrCompare = true;
			return true;
		}
		if (localName == "compare_type")
		{
			this.OrCompare = _attribute.Value.EqualsCaseInsensitive("or");
			return true;
		}
		if (!(localName == "delay"))
		{
			return false;
		}
		this.Delay = float.Parse(_attribute.Value);
		return true;
	}

	public static MinEventActionBase ParseAction(XElement _element)
	{
		if (!_element.HasAttribute("action"))
		{
			return null;
		}
		Type typeWithPrefix = ReflectionHelpers.GetTypeWithPrefix("MinEventAction", _element.GetAttribute("action"));
		if (typeWithPrefix == null)
		{
			Log.Out("Unable to find class: MinEventAction{0}", new object[]
			{
				_element.GetAttribute("action")
			});
			return null;
		}
		MinEventActionBase minEventActionBase = (MinEventActionBase)Activator.CreateInstance(typeWithPrefix);
		foreach (XAttribute attribute in _element.Attributes())
		{
			minEventActionBase.ParseXmlAttribute(attribute);
		}
		foreach (XElement element in _element.Elements("requirement"))
		{
			IRequirement requirement = RequirementBase.ParseRequirement(element);
			if (requirement != null)
			{
				minEventActionBase.Requirements.Add(requirement);
			}
		}
		minEventActionBase.ParseXMLPostProcess();
		return minEventActionBase;
	}

	public virtual void ParseXMLPostProcess()
	{
	}

	public MinEventTypes EventType;

	public bool OrCompare;

	public float Delay;

	public List<IRequirement> Requirements;
}
