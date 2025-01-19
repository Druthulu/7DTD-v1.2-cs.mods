using System;
using System.Xml.Linq;
using UnityEngine.Scripting;

[Preserve]
public class MinEventActionSetBigHead : MinEventActionTargetedBase
{
	public override void Execute(MinEventParams _params)
	{
		for (int i = 0; i < this.targets.Count; i++)
		{
			EntityAlive entityAlive = this.targets[i];
			if (this.enabled)
			{
				entityAlive.SetBigHead();
			}
			else
			{
				entityAlive.ResetHead();
			}
		}
	}

	public override bool ParseXmlAttribute(XAttribute _attribute)
	{
		bool flag = base.ParseXmlAttribute(_attribute);
		if (!flag && _attribute.Name.LocalName == "enabled")
		{
			this.enabled = StringParsers.ParseBool(_attribute.Value, 0, -1, true);
		}
		return flag;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool enabled;
}
