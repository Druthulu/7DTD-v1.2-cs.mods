using System;
using System.Collections.Generic;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdAutoMove : ConsoleCmdAbstract
{
	public override bool IsExecuteOnClient
	{
		get
		{
			return true;
		}
	}

	public override DeviceFlag AllowedDeviceTypesClient
	{
		get
		{
			return DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[]
		{
			"automove"
		};
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Player auto movement";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "Parameters:\noff - disable\ngototarget - goto the target position\nsettarget - set target to current player position\nclearlookat - disable look at\nsetlookat - set look at to current player position\nline duration loops <x> <y> <z> - move to x y z (or target) over duration with loops (-loops will ping pong)\norbit duration loops <x> <y> <z> - circle around x y z (or target) over duration with loops (-loops will ping pong)\nrelative x z angle - move by x (left/right) and z (forward) and turn by angle per second";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (_params.Count < 1)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output(this.GetHelp());
			return;
		}
		EntityPlayerLocal primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
		if (!primaryPlayer)
		{
			return;
		}
		string text = _params[0].ToLower();
		uint num = <PrivateImplementationDetails>.ComputeStringHash(text);
		if (num <= 2872740362U)
		{
			if (num <= 1260422518U)
			{
				if (num <= 400234023U)
				{
					if (num != 361777697U)
					{
						if (num != 400234023U)
						{
							goto IL_515;
						}
						if (!(text == "line"))
						{
							goto IL_515;
						}
						goto IL_2E7;
					}
					else
					{
						if (!(text == "cla"))
						{
							goto IL_515;
						}
						goto IL_2CE;
					}
				}
				else if (num != 890022063U)
				{
					if (num != 1260422518U)
					{
						goto IL_515;
					}
					if (!(text == "gt"))
					{
						goto IL_515;
					}
					goto IL_2B3;
				}
				else if (!(text == "0"))
				{
					goto IL_515;
				}
			}
			else
			{
				if (num <= 1918877446U)
				{
					if (num != 1263673922U)
					{
						if (num != 1918877446U)
						{
							goto IL_515;
						}
						if (!(text == "settarget"))
						{
							goto IL_515;
						}
					}
					else if (!(text == "st"))
					{
						goto IL_515;
					}
					this.targetPos = primaryPlayer.GetPosition();
					return;
				}
				if (num != 2142508988U)
				{
					if (num != 2872740362U)
					{
						goto IL_515;
					}
					if (!(text == "off"))
					{
						goto IL_515;
					}
				}
				else
				{
					if (!(text == "clearlookat"))
					{
						goto IL_515;
					}
					goto IL_2CE;
				}
			}
			primaryPlayer.EnableAutoMove(false);
			return;
			IL_2CE:
			this.lookAtPos = Vector3.zero;
			return;
		}
		if (num > 3926667934U)
		{
			if (num <= 4144776981U)
			{
				if (num != 3946618703U)
				{
					if (num != 4144776981U)
					{
						goto IL_515;
					}
					if (!(text == "r"))
					{
						goto IL_515;
					}
				}
				else
				{
					if (!(text == "orbit"))
					{
						goto IL_515;
					}
					goto IL_3BF;
				}
			}
			else if (num != 4236415261U)
			{
				if (num != 4239927381U)
				{
					goto IL_515;
				}
				if (!(text == "setlookat"))
				{
					goto IL_515;
				}
				goto IL_2DA;
			}
			else if (!(text == "relative"))
			{
				goto IL_515;
			}
			float velX = 0f;
			if (_params.Count >= 2)
			{
				velX = this.FloatParse(_params[1]);
			}
			float velZ = 0f;
			if (_params.Count >= 3)
			{
				velZ = this.FloatParse(_params[2]);
			}
			float rotVel = 0f;
			if (_params.Count >= 4)
			{
				rotVel = this.FloatParse(_params[3]);
			}
			EntityPlayerLocal.AutoMove autoMove = primaryPlayer.EnableAutoMove(true);
			autoMove.SetLookAt(this.lookAtPos);
			autoMove.StartRelative(velX, velZ, rotVel);
			return;
		}
		if (num <= 3308801681U)
		{
			if (num != 3291586103U)
			{
				if (num != 3308801681U)
				{
					goto IL_515;
				}
				if (!(text == "sla"))
				{
					goto IL_515;
				}
			}
			else
			{
				if (!(text == "gototarget"))
				{
					goto IL_515;
				}
				goto IL_2B3;
			}
		}
		else if (num != 3909890315U)
		{
			if (num != 3926667934U)
			{
				goto IL_515;
			}
			if (!(text == "o"))
			{
				goto IL_515;
			}
			goto IL_3BF;
		}
		else
		{
			if (!(text == "l"))
			{
				goto IL_515;
			}
			goto IL_2E7;
		}
		IL_2DA:
		this.lookAtPos = primaryPlayer.GetPosition();
		return;
		IL_3BF:
		if (this.targetPos.sqrMagnitude == 0f)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("orbit target is not set");
			return;
		}
		Vector3 orbitPos = this.targetPos;
		float duration = 5f;
		if (_params.Count >= 2)
		{
			duration = this.FloatParse(_params[1]);
		}
		int loopCount = 0;
		if (_params.Count >= 3)
		{
			loopCount = this.IntParse(_params[2]);
		}
		if (_params.Count >= 4)
		{
			orbitPos.x = this.FloatParse(_params[3]);
		}
		if (_params.Count >= 5)
		{
			orbitPos.y = this.FloatParse(_params[4]);
		}
		if (_params.Count >= 6)
		{
			orbitPos.z = this.FloatParse(_params[5]);
		}
		EntityPlayerLocal.AutoMove autoMove2 = primaryPlayer.EnableAutoMove(true);
		autoMove2.SetLookAt(this.lookAtPos);
		autoMove2.StartOrbit(duration, loopCount, orbitPos);
		return;
		IL_2B3:
		primaryPlayer.SetPosition(this.targetPos, true);
		return;
		IL_2E7:
		if (this.targetPos.sqrMagnitude == 0f)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("line target is not set");
			return;
		}
		Vector3 endPos = this.targetPos;
		float duration2 = 5f;
		if (_params.Count >= 2)
		{
			duration2 = this.FloatParse(_params[1]);
		}
		int loopCount2 = 0;
		if (_params.Count >= 3)
		{
			loopCount2 = this.IntParse(_params[2]);
		}
		if (_params.Count >= 4)
		{
			endPos.x = this.FloatParse(_params[3]);
		}
		if (_params.Count >= 5)
		{
			endPos.y = this.FloatParse(_params[4]);
		}
		if (_params.Count >= 6)
		{
			endPos.z = this.FloatParse(_params[5]);
		}
		EntityPlayerLocal.AutoMove autoMove3 = primaryPlayer.EnableAutoMove(true);
		autoMove3.SetLookAt(this.lookAtPos);
		autoMove3.StartLine(duration2, loopCount2, endPos);
		return;
		IL_515:
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("unknown command " + _params[0]);
	}

	public int IntParse(string s)
	{
		int result;
		int.TryParse(s, out result);
		return result;
	}

	public float FloatParse(string s)
	{
		float result;
		float.TryParse(s, out result);
		return result;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 targetPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 lookAtPos;
}
