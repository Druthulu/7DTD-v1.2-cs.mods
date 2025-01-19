﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdMemoryProfiler : ConsoleCmdAbstract
{
	public override bool AllowedInMainMenu
	{
		get
		{
			return true;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[]
		{
			"memprofile",
			"mprof"
		};
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Toggles screen Memory Profiler UI";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		UnityMemoryProfilerLabel[] array2;
		if (!this.enabled)
		{
			this.enabled = true;
			UnityMemoryProfilerLabel[] array = UnityEngine.Object.FindObjectsOfType<UnityMemoryProfilerLabel>();
			if (array == null || array.Length == 0)
			{
				UnityEngine.Object original = Resources.Load("GUI/Prefabs/Debug_ProfilerLabel");
				using (List<UIRoot>.Enumerator enumerator = UIRoot.list.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						UIRoot uiroot = enumerator.Current;
						Transform transform = uiroot.gameObject.transform;
						if (uiroot.gameObject.GetComponentInChildren<UIAnchor>() != null)
						{
							transform = uiroot.gameObject.GetComponentInChildren<UIAnchor>().transform;
						}
						UnityEngine.Object.Instantiate(original, transform);
					}
					return;
				}
			}
			array2 = UnityEngine.Object.FindObjectsOfType<UnityMemoryProfilerLabel>();
			for (int i = 0; i < array2.Length; i++)
			{
				array2[i].gameObject.SetActive(true);
			}
			return;
		}
		this.enabled = false;
		array2 = UnityEngine.Object.FindObjectsOfType<UnityMemoryProfilerLabel>();
		for (int i = 0; i < array2.Length; i++)
		{
			array2[i].gameObject.SetActive(false);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool enabled;
}
