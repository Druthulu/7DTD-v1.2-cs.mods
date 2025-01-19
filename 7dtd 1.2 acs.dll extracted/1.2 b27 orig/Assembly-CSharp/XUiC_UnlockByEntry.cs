using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_UnlockByEntry : XUiController
{
	public Recipe Recipe { get; set; }

	public RecipeUnlockData UnlockData
	{
		get
		{
			return this.unlockData;
		}
		set
		{
			this.unlockData = value;
			this.isDirty = true;
		}
	}

	public override void Init()
	{
		base.Init();
		this.isDirty = false;
	}

	public override bool GetBindingValue(ref string value, string bindingName)
	{
		bool flag = this.UnlockData != null;
		if (bindingName == "name")
		{
			if (flag)
			{
				value = this.UnlockData.GetName();
			}
			else
			{
				value = "";
			}
			return true;
		}
		if (bindingName == "itemicon")
		{
			if (flag)
			{
				value = this.UnlockData.GetIcon();
			}
			else
			{
				value = "";
			}
			return true;
		}
		if (bindingName == "itemiconatlas")
		{
			if (flag)
			{
				value = this.UnlockData.GetIconAtlas();
			}
			else
			{
				value = "UIAtlas";
			}
			return true;
		}
		if (bindingName == "itemicontint")
		{
			Color32 v = Color.white;
			if (flag)
			{
				v = this.UnlockData.GetItemTint();
			}
			value = this.itemicontintcolorFormatter.Format(v);
			return true;
		}
		if (!(bindingName == "level"))
		{
			return false;
		}
		if (flag)
		{
			value = this.UnlockData.GetLevel(base.xui.playerUI.entityPlayer, this.Recipe.GetOutputItemClass().Name);
		}
		else
		{
			value = "";
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleOnCountChanged(XUiController _sender, OnCountChangedEventArgs _e)
	{
		this.isDirty = true;
	}

	public override void Update(float _dt)
	{
		if (this.isDirty)
		{
			base.RefreshBindings(false);
			base.ViewComponent.IsVisible = true;
			this.isDirty = false;
		}
		base.Update(_dt);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isDirty;

	[PublicizedFrom(EAccessModifier.Private)]
	public RecipeUnlockData unlockData;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterXuiRgbaColor itemicontintcolorFormatter = new CachedStringFormatterXuiRgbaColor();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterInt havecountFormatter = new CachedStringFormatterInt();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterInt needcountFormatter = new CachedStringFormatterInt();
}
