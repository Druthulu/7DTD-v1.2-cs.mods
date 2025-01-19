using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;

public class RequirementBase : IRequirement
{
	public virtual bool IsValid(MinEventParams _params)
	{
		return this.ParamsValid(_params);
	}

	public virtual bool ParamsValid(MinEventParams _params)
	{
		if (this.useCVar)
		{
			this.value = _params.Self.Buffs.GetCustomVar(this.refCvarName, 0f);
		}
		return true;
	}

	public void SetDescription(string desc)
	{
		this.Description = desc;
	}

	public virtual void GetInfoStrings(ref List<string> list)
	{
	}

	public virtual bool ParseXAttribute(XAttribute _attribute)
	{
		string localName = _attribute.Name.LocalName;
		if (localName == "operation")
		{
			this.operation = EnumUtils.Parse<RequirementBase.OperationTypes>(_attribute.Value, true);
			return true;
		}
		if (localName == "value")
		{
			if (_attribute.Value.StartsWith("@"))
			{
				this.useCVar = true;
				this.refCvarName = _attribute.Value.Substring(1);
			}
			else
			{
				this.value = StringParsers.ParseFloat(_attribute.Value, 0, -1, NumberStyles.Any);
			}
			return true;
		}
		if (!(localName == "invert"))
		{
			return false;
		}
		this.invert = StringParsers.ParseBool(_attribute.Value, 0, -1, true);
		return true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public static bool compareValues(float _valueA, RequirementBase.OperationTypes _operation, float _valueB)
	{
		switch (_operation)
		{
		case RequirementBase.OperationTypes.Equals:
		case RequirementBase.OperationTypes.EQ:
		case RequirementBase.OperationTypes.E:
			return _valueA == _valueB;
		case RequirementBase.OperationTypes.NotEquals:
		case RequirementBase.OperationTypes.NEQ:
		case RequirementBase.OperationTypes.NE:
			return _valueA != _valueB;
		case RequirementBase.OperationTypes.Less:
		case RequirementBase.OperationTypes.LessThan:
		case RequirementBase.OperationTypes.LT:
			return _valueA < _valueB;
		case RequirementBase.OperationTypes.Greater:
		case RequirementBase.OperationTypes.GreaterThan:
		case RequirementBase.OperationTypes.GT:
			return _valueA > _valueB;
		case RequirementBase.OperationTypes.LessOrEqual:
		case RequirementBase.OperationTypes.LessThanOrEqualTo:
		case RequirementBase.OperationTypes.LTE:
			return _valueA <= _valueB;
		case RequirementBase.OperationTypes.GreaterOrEqual:
		case RequirementBase.OperationTypes.GreaterThanOrEqualTo:
		case RequirementBase.OperationTypes.GTE:
			return _valueA >= _valueB;
		default:
			return false;
		}
	}

	public static IRequirement ParseRequirement(XElement _element)
	{
		if (!_element.HasAttribute("name"))
		{
			return null;
		}
		string text = _element.GetAttribute("name");
		bool flag = text.StartsWith("!");
		if (flag)
		{
			text = text.Substring(1);
		}
		Type type = Type.GetType(text);
		if (type == null)
		{
			return null;
		}
		IRequirement requirement = (IRequirement)Activator.CreateInstance(type);
		if (_element.HasAttribute("desc_key"))
		{
			requirement.SetDescription(Localization.Get(_element.GetAttribute("desc_key"), false));
		}
		foreach (XAttribute attribute in _element.Attributes())
		{
			requirement.ParseXAttribute(attribute);
		}
		if (flag && requirement is RequirementBase)
		{
			(requirement as RequirementBase).invert = true;
		}
		return requirement;
	}

	public static List<IRequirement> ParseRequirements(XElement _element)
	{
		List<IRequirement> list = new List<IRequirement>();
		foreach (XElement element in _element.Elements("requirement"))
		{
			list.Add(RequirementBase.ParseRequirement(element));
		}
		return list;
	}

	public string GetInfoString()
	{
		if (this.Description != null)
		{
			return this.Description;
		}
		List<string> list = new List<string>();
		this.GetInfoStrings(ref list);
		if (list.Count > 0)
		{
			return list[0];
		}
		return null;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool useCVar;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string refCvarName;

	public bool invert;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float value;

	[PublicizedFrom(EAccessModifier.Protected)]
	public RequirementBase.OperationTypes operation;

	public string Description;

	[PublicizedFrom(EAccessModifier.Protected)]
	public enum OperationTypes
	{
		None,
		Equals,
		EQ,
		E,
		NotEquals,
		NEQ,
		NE,
		Less,
		LessThan,
		LT,
		Greater,
		GreaterThan,
		GT,
		LessOrEqual,
		LessThanOrEqualTo,
		LTE,
		GreaterOrEqual,
		GreaterThanOrEqualTo,
		GTE
	}
}
