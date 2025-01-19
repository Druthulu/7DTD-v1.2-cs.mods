using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdSDCS : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "Change a player's sex, race, and variant for SDCS testing\nUsage:\n   sdcs sex male\n   sdcs race white\n   sdcs variant 4\n";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[]
		{
			"sdcs"
		};
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Control entity sex, race, and variant";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (_params.Count < 2)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("sdcs requires a control type (sex, race, variant) and a value");
			return;
		}
		if (GameManager.Instance.World.GetLocalPlayers().Count == 0)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("No local players found");
			return;
		}
		EntityPlayer entityPlayer = GameManager.Instance.World.GetLocalPlayers()[0];
		ConsoleCmdSDCS.cTypes cTypes;
		if (!Enum.TryParse<ConsoleCmdSDCS.cTypes>(_params[0], true, out cTypes))
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Invalid control type");
			return;
		}
		string text = _params[1];
		EModelSDCS component = entityPlayer.GetComponent<EModelSDCS>();
		if (component == null)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("No SDCS model found");
			return;
		}
		switch (cTypes)
		{
		case ConsoleCmdSDCS.cTypes.Sex:
		{
			ConsoleCmdSDCS.sTypes sTypes;
			if (!Enum.TryParse<ConsoleCmdSDCS.sTypes>(_params[1], true, out sTypes))
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Invalid race '" + _params[1] + "'");
				return;
			}
			bool sex = false;
			if (sTypes == ConsoleCmdSDCS.sTypes.male)
			{
				sex = true;
			}
			component.SetSex(sex);
			component.SetRace("white");
			component.SetVariant(1);
			return;
		}
		case ConsoleCmdSDCS.cTypes.Race:
		{
			ConsoleCmdSDCS.rTypes rTypes;
			if (!Enum.TryParse<ConsoleCmdSDCS.rTypes>(_params[1], true, out rTypes))
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Invalid race '" + _params[1] + "'");
				return;
			}
			component.SetRace(_params[1]);
			component.SetVariant(1);
			return;
		}
		case ConsoleCmdSDCS.cTypes.Variant:
		{
			int num;
			if (!StringParsers.TryParseSInt32(_params[1], out num, 0, -1, NumberStyles.Integer))
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Invalid variant number " + _params[1]);
			}
			if (num > 4)
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("Invalid variant number {0}", num));
			}
			component.SetVariant(num);
			return;
		}
		default:
			return;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public enum cTypes
	{
		Sex,
		Race,
		Variant
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public enum rTypes
	{
		White,
		Black,
		Asian,
		Hispanic,
		MiddleEastern
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public enum sTypes
	{
		male,
		female
	}
}
