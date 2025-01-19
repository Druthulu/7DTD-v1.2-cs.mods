using System;
using System.Globalization;
using System.Xml.Linq;
using UnityEngine.Scripting;

[Preserve]
public class MinEventActionAddHealth : MinEventActionTargetedBase
{
	public override void Execute(MinEventParams _params)
	{
		if (this.targets == null)
		{
			return;
		}
		for (int i = 0; i < this.targets.Count; i++)
		{
			this.targets[i].AddHealth(this.health);
			if (this.health < 0)
			{
				EntityPlayerLocal entityPlayerLocal = this.targets[i] as EntityPlayerLocal;
				if (entityPlayerLocal != null)
				{
					entityPlayerLocal.ForceBloodSplatter();
				}
			}
		}
	}

	public override bool ParseXmlAttribute(XAttribute _attribute)
	{
		bool flag = base.ParseXmlAttribute(_attribute);
		if (!flag && _attribute.Name.LocalName == "health")
		{
			this.health = StringParsers.ParseSInt32(_attribute.Value, 0, -1, NumberStyles.Integer);
		}
		return flag;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int health;
}
