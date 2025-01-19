﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdLights : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[]
		{
			"lights"
		};
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Light debugging";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "Light debugging:\nv - toggle viewer enable\noff - viewer lights off\non - viewer lights on\nclearreg - clear registered\ndisableall - \nenableall - \nlist - list all lights\nlistfile - list all lights to Lights.txt\nliste - list lights effecting the player position\nlodviewdistance <d> - set the light LOD view distance (0 uses defaults)";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (_params.Count == 0)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output(this.GetHelp());
			return;
		}
		string a = _params[0].ToLower();
		if (a == "v")
		{
			LightViewer.SetEnabled(!LightViewer.IsEnabled);
			return;
		}
		if (a == "lodviewdistance" || a == "lvd")
		{
			float debugViewDistance = 0f;
			if (_params.Count >= 2)
			{
				float.TryParse(_params[1], out debugViewDistance);
			}
			LightLOD.DebugViewDistance = debugViewDistance;
			return;
		}
		if (_params.Count == 1)
		{
			if (_params[0].EqualsCaseInsensitive("on"))
			{
				if (Camera.main != null)
				{
					LightViewer component = Camera.main.GetComponent<LightViewer>();
					if (component != null)
					{
						LightViewer.IsAllOff = false;
						component.TurnOnAllLights();
					}
				}
				return;
			}
			if (_params[0].EqualsCaseInsensitive("off"))
			{
				if (Camera.main != null)
				{
					LightViewer component2 = Camera.main.GetComponent<LightViewer>();
					if (component2 != null)
					{
						LightViewer.IsAllOff = true;
						component2.TurnOffAllLights();
					}
				}
				return;
			}
			if (_params[0].EqualsCaseInsensitive("showlevel"))
			{
				LightManager.ShowLightLevel(!LightManager.ShowLightLevelOn);
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output(LightManager.ShowLightLevelOn ? "Showing " : "Hiding light level");
				return;
			}
			if (_params[0].EqualsCaseInsensitive("showsearchpattern"))
			{
				LightManager.ShowSearchPattern(!LightManager.ShowSearchPatternOn);
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output(LightManager.ShowSearchPatternOn ? "Showing " : "Hiding search pattern");
				return;
			}
			if (_params[0].EqualsCaseInsensitive("clearreg"))
			{
				LightManager.Clear();
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Cleared.");
				return;
			}
			if (_params[0].EqualsCaseInsensitive("liste"))
			{
				Dictionary<Vector3, Light> lightsEffecting = LightManager.GetLightsEffecting(GameManager.Instance.World.GetPrimaryPlayer().position);
				if (lightsEffecting != null)
				{
					int num = 0;
					using (Dictionary<Vector3, Light>.Enumerator enumerator = lightsEffecting.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							KeyValuePair<Vector3, Light> keyValuePair = enumerator.Current;
							Vector3 position = keyValuePair.Value.transform.position;
							SingletonMonoBehaviour<SdtdConsole>.Instance.Output(string.Format("#{0} {1} {2}", num++, keyValuePair.Value.name, position.ToCultureInvariantString("g")));
						}
						return;
					}
				}
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("No lights");
				return;
			}
			if (_params[0].EqualsCaseInsensitive("list"))
			{
				ConsoleCmdLights.allLights = UnityEngine.Object.FindObjectsOfType<Light>();
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output(ConsoleCmdLights.allLights.Length.ToString() + " lights:");
				for (int i = 0; i < ConsoleCmdLights.allLights.Length; i++)
				{
					Light light = ConsoleCmdLights.allLights[i];
					string line = string.Format("#{0} {1} {2} {3}", new object[]
					{
						i,
						light.name,
						light.transform.position.ToCultureInvariantString("g"),
						light.enabled ? "enabled" : "disabled"
					});
					SingletonMonoBehaviour<SdtdConsole>.Instance.Output(line);
				}
				return;
			}
			if (_params[0].EqualsCaseInsensitive("listfile"))
			{
				StreamWriter streamWriter = SdFile.CreateText("Lights.txt");
				if (streamWriter != null)
				{
					ConsoleCmdLights.allLights = UnityEngine.Object.FindObjectsOfType<Light>();
					streamWriter.WriteLine(ConsoleCmdLights.allLights.Length.ToString() + " lights:");
					for (int j = 0; j < ConsoleCmdLights.allLights.Length; j++)
					{
						Light light2 = ConsoleCmdLights.allLights[j];
						string value = string.Format("#{0} {1} {2} {3}", new object[]
						{
							j,
							light2.name,
							light2.transform.position.ToCultureInvariantString("g"),
							light2.enabled ? "enabled" : "disabled"
						});
						streamWriter.WriteLine(value);
					}
					streamWriter.Close();
				}
				return;
			}
			if (_params[0].EqualsCaseInsensitive("regtest"))
			{
				LightLOD[] array = UnityEngine.Object.FindObjectsOfType<LightLOD>();
				for (int k = 0; k < array.Length; k++)
				{
					array[k].TestRegistration();
				}
				return;
			}
			if (_params[0].EqualsCaseInsensitive("disableall"))
			{
				ConsoleCmdLights.allLights = UnityEngine.Object.FindObjectsOfType<Light>();
				Light[] array2 = ConsoleCmdLights.allLights;
				for (int l = 0; l < array2.Length; l++)
				{
					array2[l].enabled = false;
				}
				return;
			}
			if (_params[0].EqualsCaseInsensitive("enableall"))
			{
				ConsoleCmdLights.allLights = UnityEngine.Object.FindObjectsOfType<Light>();
				Light[] array2 = ConsoleCmdLights.allLights;
				for (int l = 0; l < array2.Length; l++)
				{
					array2[l].enabled = true;
				}
				return;
			}
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Unknown command");
			return;
		}
		else
		{
			float num2 = 1f;
			if (_params.Count > 1)
			{
				num2 = StringParsers.ParseFloat(_params[1], 0, -1, NumberStyles.Any);
				if (_params[0].EqualsCaseInsensitive("level"))
				{
					int entityId = Mathf.FloorToInt(num2);
					EntityAlive entityAlive = GameManager.Instance.World.GetEntity(entityId) as EntityAlive;
					if (entityAlive)
					{
						float num3;
						float stealthLightLevel = LightManager.GetStealthLightLevel(entityAlive, out num3);
						SingletonMonoBehaviour<SdtdConsole>.Instance.Output("LightLevel for player(" + entityId.ToString() + ") = " + (stealthLightLevel + num3).ToCultureInvariantString());
					}
				}
				else
				{
					if (_params[0].EqualsCaseInsensitive("disable"))
					{
						ConsoleCmdLights.allLights = UnityEngine.Object.FindObjectsOfType<Light>();
						if ((int)num2 < ConsoleCmdLights.allLights.Length)
						{
							ConsoleCmdLights.allLights[(int)num2].enabled = false;
						}
						return;
					}
					if (_params[0].EqualsCaseInsensitive("enable"))
					{
						ConsoleCmdLights.allLights = UnityEngine.Object.FindObjectsOfType<Light>();
						if ((int)num2 < ConsoleCmdLights.allLights.Length)
						{
							ConsoleCmdLights.allLights[(int)num2].enabled = true;
						}
						return;
					}
				}
			}
			if (_params[0].EqualsCaseInsensitive("sec"))
			{
				if (Camera.main != null)
				{
					LightViewer component3 = Camera.main.GetComponent<LightViewer>();
					if (component3 != null)
					{
						component3.SetUpdateFrequency(num2);
					}
				}
				return;
			}
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Unknown command");
			return;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static Light[] allLights;
}
