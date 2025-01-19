using System;
using System.Collections.Generic;
using System.Xml.Linq;

public class MinEffectGroup
{
	public MinEffectGroup()
	{
		this.Requirements = null;
		this.PassiveEffects = new List<PassiveEffect>();
		this.TriggeredEffects = null;
		this.EffectDescriptions = new List<EffectGroupDescription>();
		this.OwnerTiered = true;
		this.PassivesIndices = new List<PassiveEffects>();
		this.EffectDisplayValues = new CaseInsensitiveStringDictionary<EffectDisplayValue>();
	}

	public void ModifyValue(MinEventParams _params, EntityAlive _self, PassiveEffects _effect, ref float _base_value, ref float _perc_value, float level, FastTags<TagGroup.Global> _tags, int _multiplier = 1)
	{
		if (!this.canRun(_params))
		{
			return;
		}
		int count = this.PassiveEffects.Count;
		for (int i = 0; i < count; i++)
		{
			PassiveEffect passiveEffect = this.PassiveEffects[i];
			if (passiveEffect.Type == _effect && passiveEffect.RequirementsMet(_params))
			{
				passiveEffect.ModifyValue(_self, level, ref _base_value, ref _perc_value, _tags, _multiplier);
			}
		}
	}

	public void GetModifiedValueData(List<EffectManager.ModifierValuesAndSources> _modValueSource, EffectManager.ModifierValuesAndSources.ValueSourceType _sourceType, MinEffectController.SourceParentType _parentType, EntityAlive _self, PassiveEffects _effect, ref float _base_value, ref float _perc_value, float level, FastTags<TagGroup.Global> _tags, int _multiplier = 1, object _parentPointer = null)
	{
		MinEventParams minEventParams;
		if (_self == null)
		{
			minEventParams = MinEventParams.CachedEventParam;
			minEventParams.Self = null;
		}
		else
		{
			minEventParams = _self.MinEventContext;
		}
		minEventParams.Tags = _tags;
		if (!this.canRun(minEventParams))
		{
			return;
		}
		for (int i = 0; i < this.PassiveEffects.Count; i++)
		{
			PassiveEffect passiveEffect = this.PassiveEffects[i];
			if (passiveEffect.Type == _effect && passiveEffect.RequirementsMet(minEventParams))
			{
				passiveEffect.GetModifiedValueData(_modValueSource, _sourceType, _parentType, _self, level, ref _base_value, ref _perc_value, _tags, _multiplier, _parentPointer);
			}
		}
	}

	public bool HasEvents()
	{
		return this.TriggeredEffects != null;
	}

	public void FireEvent(MinEventTypes _eventType, MinEventParams _eventParms)
	{
		if (this.TriggeredEffects == null)
		{
			return;
		}
		if (!this.canRun(_eventParms))
		{
			return;
		}
		for (int i = 0; i < this.TriggeredEffects.Count; i++)
		{
			MinEventActionBase minEventActionBase = this.TriggeredEffects[i];
			if (minEventActionBase.EventType == _eventType && minEventActionBase.CanExecute(_eventType, _eventParms))
			{
				minEventActionBase.Execute(_eventParms);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool canRun(MinEventParams _params)
	{
		if (this.Requirements == null)
		{
			return true;
		}
		if (this.OrCompareRequirements)
		{
			for (int i = 0; i < this.Requirements.Count; i++)
			{
				if (this.Requirements[i].IsValid(_params))
				{
					return true;
				}
			}
			return false;
		}
		for (int j = 0; j < this.Requirements.Count; j++)
		{
			if (!this.Requirements[j].IsValid(_params))
			{
				return false;
			}
		}
		return true;
	}

	public bool HasTrigger(MinEventTypes _eventType)
	{
		if (this.TriggeredEffects != null)
		{
			for (int i = 0; i < this.TriggeredEffects.Count; i++)
			{
				if (this.TriggeredEffects[i].EventType == _eventType)
				{
					return true;
				}
			}
		}
		return false;
	}

	public static MinEffectGroup ParseXml(XElement _element)
	{
		MinEffectGroup minEffectGroup = new MinEffectGroup();
		if (_element.HasAttribute("compare_type"))
		{
			minEffectGroup.OrCompareRequirements = _element.GetAttribute("compare_type").EqualsCaseInsensitive("or");
		}
		if (_element.HasAttribute("tiered"))
		{
			minEffectGroup.OwnerTiered = StringParsers.ParseBool(_element.GetAttribute("tiered"), 0, -1, true);
		}
		foreach (XElement xelement in _element.Elements())
		{
			if (xelement.Name == "requirements")
			{
				if (xelement.HasAttribute("compare_type"))
				{
					minEffectGroup.OrCompareRequirements = xelement.GetAttribute("compare_type").EqualsCaseInsensitive("or");
				}
				List<IRequirement> list = RequirementBase.ParseRequirements(xelement);
				if (list.Count > 0)
				{
					if (minEffectGroup.Requirements == null)
					{
						minEffectGroup.Requirements = new List<IRequirement>();
					}
					minEffectGroup.Requirements.AddRange(list);
				}
			}
			else if (xelement.Name == "requirement")
			{
				IRequirement requirement = RequirementBase.ParseRequirement(xelement);
				if (requirement != null)
				{
					if (minEffectGroup.Requirements == null)
					{
						minEffectGroup.Requirements = new List<IRequirement>();
					}
					minEffectGroup.Requirements.Add(requirement);
				}
			}
			else if (xelement.Name == "passive_effect")
			{
				PassiveEffect passiveEffect = PassiveEffect.ParsePassiveEffect(xelement);
				if (passiveEffect != null)
				{
					MinEffectGroup.AddPassiveEffectToGroup(minEffectGroup, passiveEffect);
				}
			}
			else if (xelement.Name == "triggered_effect")
			{
				MinEventActionBase minEventActionBase = MinEventActionBase.ParseAction(xelement);
				if (minEventActionBase != null)
				{
					if (minEffectGroup.TriggeredEffects == null)
					{
						minEffectGroup.TriggeredEffects = new List<MinEventActionBase>();
					}
					minEffectGroup.TriggeredEffects.Add(minEventActionBase);
				}
			}
			else if (xelement.Name == "effect_description")
			{
				EffectGroupDescription effectGroupDescription = EffectGroupDescription.ParseDescription(xelement);
				if (effectGroupDescription != null)
				{
					minEffectGroup.EffectDescriptions.Add(effectGroupDescription);
				}
			}
			else if (xelement.Name == "display_value")
			{
				EffectDisplayValue effectDisplayValue = EffectDisplayValue.ParseDisplayValue(xelement);
				if (effectDisplayValue != null)
				{
					minEffectGroup.EffectDisplayValues.Add(effectDisplayValue.Name, effectDisplayValue);
				}
			}
		}
		return minEffectGroup;
	}

	public static void AddPassiveEffectToGroup(MinEffectGroup _effectGroup, PassiveEffect _pe)
	{
		_effectGroup.PassivesIndices.Add(_pe.Type);
		_effectGroup.PassiveEffects.Add(_pe);
	}

	public bool OrCompareRequirements;

	public List<IRequirement> Requirements;

	public List<PassiveEffect> PassiveEffects;

	public List<MinEventActionBase> TriggeredEffects;

	public List<EffectGroupDescription> EffectDescriptions;

	public CaseInsensitiveStringDictionary<EffectDisplayValue> EffectDisplayValues;

	public bool OwnerTiered;

	public List<PassiveEffects> PassivesIndices;
}
