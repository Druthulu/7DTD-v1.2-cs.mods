using System;
using UnityEngine.Scripting;

[Preserve]
public struct EntityActivationCommand
{
	public EntityActivationCommand(string _text, string _icon, bool _enabled)
	{
		this.text = _text;
		this.icon = _icon;
		this.enabled = _enabled;
	}

	public string text;

	public string icon;

	public bool enabled;
}
