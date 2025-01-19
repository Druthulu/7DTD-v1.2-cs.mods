using System;

public struct BlockActivationCommand
{
	public BlockActivationCommand(string _text, string _icon, bool _enabled, bool _highlighted = false)
	{
		this.text = _text;
		this.icon = _icon;
		this.enabled = _enabled;
		this.highlighted = _highlighted;
	}

	public readonly string text;

	public string icon;

	public bool enabled;

	public bool highlighted;

	public static readonly BlockActivationCommand[] Empty = Array.Empty<BlockActivationCommand>();
}
