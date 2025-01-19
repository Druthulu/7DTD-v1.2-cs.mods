using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_CustomCharacterWindow : XUiController
{
	public override bool ParseAttribute(string name, string value, XUiController _parent)
	{
		bool flag = base.ParseAttribute(name, value, _parent);
		if (!flag)
		{
			if (!(name == "locked_sprite"))
			{
				if (!(name == "unlocked_sprite"))
				{
					if (!(name == "locked_color"))
					{
						if (!(name == "unlocked_color"))
						{
							return false;
						}
						this.unlockedColor = StringParsers.ParseColor32(value);
					}
					else
					{
						this.lockedColor = StringParsers.ParseColor32(value);
					}
				}
				else
				{
					this.unlockedSprite = value;
				}
			}
			else
			{
				this.lockedSprite = value;
			}
			return true;
		}
		return flag;
	}

	public string lockedSprite = "ui_game_symbol_lock";

	public string unlockedSprite = "ui_game_symbol_lock";

	public Color lockedColor;

	public Color unlockedColor;
}
