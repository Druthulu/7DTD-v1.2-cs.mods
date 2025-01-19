using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_EnteringArea : XUiController
{
	public Prefab Prefab
	{
		get
		{
			return this.prefab;
		}
		set
		{
			this.prefab = value;
			if (this.prefab != null)
			{
				this.showTime = 5f;
			}
		}
	}

	public BiomeDefinition Biome
	{
		get
		{
			return this.biome;
		}
		set
		{
			this.biome = value;
			if (this.biome != null)
			{
				this.showTime = 5f;
			}
		}
	}

	public override void Init()
	{
		base.Init();
		this.IsDirty = true;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (this.LocalPlayer == null && base.xui != null && base.xui.playerUI != null && base.xui.playerUI.entityPlayer != null)
		{
			this.LocalPlayer = base.xui.playerUI.entityPlayer;
		}
		if (this.LocalPlayer == null)
		{
			return;
		}
		if (!this.LocalPlayer.IsAlive())
		{
			base.ViewComponent.IsVisible = false;
			return;
		}
		if (!base.xui.playerUI.windowManager.IsHUDEnabled())
		{
			base.ViewComponent.IsVisible = false;
			return;
		}
		if (this.LocalPlayer.enteredPrefab != null && this.prefab != this.LocalPlayer.enteredPrefab.prefab && this.LocalPlayer.enteredPrefab.prefab != this.lastPrefab)
		{
			this.Prefab = this.LocalPlayer.enteredPrefab.prefab;
			this.LocalPlayer.enteredPrefab = null;
			if (this.Prefab != null)
			{
				if (this.prefab.Tags.Test_AnySet(this.partTag) || this.prefab.Tags.Test_AnySet(this.streetTileTag) || this.prefab.Tags.Test_AnySet(this.navOnlyTileTag) || this.prefab.Tags.Test_AnySet(this.hideUITag))
				{
					this.prefabDiff = 0;
					this.Prefab = null;
				}
				else
				{
					this.prefabDiff = (int)this.Prefab.DifficultyTier;
					this.HandleBiomeDifficulty(this.LocalPlayer.biomeStandingOn);
				}
			}
			else
			{
				this.prefabDiff = 0;
			}
			base.RefreshBindings(true);
			return;
		}
		BiomeDefinition biomeStandingOn = this.LocalPlayer.biomeStandingOn;
		if (this.LocalPlayer.prefab == null && biomeStandingOn != null && this.biome != biomeStandingOn && biomeStandingOn != this.lastBiome)
		{
			if (this.ignoreFirst)
			{
				this.lastBiome = biomeStandingOn;
				this.ignoreFirst = false;
				return;
			}
			this.Biome = biomeStandingOn;
			this.HandleBiomeDifficulty(this.Biome);
			this.Prefab = null;
			this.prefabDiff = 0;
			base.RefreshBindings(true);
			return;
		}
		else
		{
			if (this.prefab == null && this.biome == null)
			{
				return;
			}
			if (this.LocalPlayer.prefab != null && this.prefab != this.LocalPlayer.prefab.prefab)
			{
				this.showTime = 0f;
			}
			this.showTime -= _dt;
			if (this.showTime <= 0f)
			{
				if (this.prefab != null)
				{
					this.lastPrefab = this.prefab;
					this.prefab = null;
				}
				if (this.biome != null)
				{
					this.lastBiome = this.biome;
					this.biome = null;
				}
				base.RefreshBindings(true);
			}
			return;
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.ignoreFirst = true;
		base.RefreshBindings(true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleBiomeDifficulty(BiomeDefinition biome)
	{
		if (biome != null)
		{
			float num = (float)(biome.Difficulty - 1) * 0.5f;
			this.biomeDiff = (int)Mathf.Floor(num);
			this.showBiomeHalf = (num - (float)this.biomeDiff == 0.5f);
			return;
		}
		this.biomeDiff = 0;
		this.showBiomeHalf = false;
	}

	public override bool GetBindingValue(ref string _value, string _bindingName)
	{
		uint num = <PrivateImplementationDetails>.ComputeStringHash(_bindingName);
		if (num <= 1425709473U)
		{
			if (num <= 972353540U)
			{
				if (num <= 938798302U)
				{
					if (num != 922020683U)
					{
						if (num == 938798302U)
						{
							if (_bindingName == "color2")
							{
								_value = this.inactiveColorFormatter.Format(this.inactiveColor);
								if (this.prefabDiff >= 2)
								{
									_value = this.activeColorFormatter.Format(this.difficultyActiveColor);
								}
								else if (this.prefabDiff + this.biomeDiff >= 2)
								{
									_value = this.activeColorFormatter.Format(this.biomeActiveColor);
								}
								return true;
							}
						}
					}
					else if (_bindingName == "color1")
					{
						_value = this.inactiveColorFormatter.Format(this.inactiveColor);
						if (this.prefabDiff >= 1)
						{
							_value = this.activeColorFormatter.Format(this.difficultyActiveColor);
						}
						else if (this.prefabDiff + this.biomeDiff >= 1)
						{
							_value = this.activeColorFormatter.Format(this.biomeActiveColor);
						}
						return true;
					}
				}
				else if (num != 955575921U)
				{
					if (num == 972353540U)
					{
						if (_bindingName == "color4")
						{
							_value = this.inactiveColorFormatter.Format(this.inactiveColor);
							if (this.prefabDiff >= 4)
							{
								_value = this.activeColorFormatter.Format(this.difficultyActiveColor);
							}
							else if (this.prefabDiff + this.biomeDiff >= 4)
							{
								_value = this.activeColorFormatter.Format(this.biomeActiveColor);
							}
							return true;
						}
					}
				}
				else if (_bindingName == "color3")
				{
					_value = this.inactiveColorFormatter.Format(this.inactiveColor);
					if (this.prefabDiff >= 3)
					{
						_value = this.activeColorFormatter.Format(this.difficultyActiveColor);
					}
					else if (this.prefabDiff + this.biomeDiff >= 3)
					{
						_value = this.activeColorFormatter.Format(this.biomeActiveColor);
					}
					return true;
				}
			}
			else if (num <= 1005908778U)
			{
				if (num != 989131159U)
				{
					if (num == 1005908778U)
					{
						if (_bindingName == "color6")
						{
							_value = this.inactiveColorFormatter.Format(this.inactiveColor);
							if (this.prefabDiff >= 6)
							{
								_value = this.activeColorFormatter.Format(this.difficultyActiveColor);
							}
							else if (this.prefabDiff + this.biomeDiff >= 6)
							{
								_value = this.activeColorFormatter.Format(this.biomeActiveColor);
							}
							return true;
						}
					}
				}
				else if (_bindingName == "color5")
				{
					_value = this.inactiveColorFormatter.Format(this.inactiveColor);
					if (this.prefabDiff >= 5)
					{
						_value = this.activeColorFormatter.Format(this.difficultyActiveColor);
					}
					else if (this.prefabDiff + this.biomeDiff >= 5)
					{
						_value = this.activeColorFormatter.Format(this.biomeActiveColor);
					}
					return true;
				}
			}
			else if (num != 1022686397U)
			{
				if (num == 1425709473U)
				{
					if (_bindingName == "visible")
					{
						if (this.LocalPlayer == null)
						{
							_value = "false";
							return true;
						}
						if (!this.LocalPlayer.IsAlive())
						{
							_value = "false";
							return true;
						}
						if (this.Prefab == null && this.Biome == null)
						{
							_value = "false";
							return true;
						}
						_value = "true";
						return true;
					}
				}
			}
			else if (_bindingName == "color7")
			{
				_value = this.inactiveColorFormatter.Format(this.inactiveColor);
				if (this.prefabDiff >= 7)
				{
					_value = this.activeColorFormatter.Format(this.difficultyActiveColor);
				}
				else if (this.prefabDiff + this.biomeDiff >= 7)
				{
					_value = this.activeColorFormatter.Format(this.biomeActiveColor);
				}
				return true;
			}
		}
		else if (num <= 1518322684U)
		{
			if (num <= 1484767446U)
			{
				if (num != 1451212208U)
				{
					if (num == 1484767446U)
					{
						if (_bindingName == "visible3")
						{
							_value = (this.prefabDiff + this.biomeDiff >= 3).ToString();
							return true;
						}
					}
				}
				else if (_bindingName == "visible1")
				{
					_value = (this.prefabDiff + this.biomeDiff >= 1).ToString();
					return true;
				}
			}
			else if (num != 1501545065U)
			{
				if (num == 1518322684U)
				{
					if (_bindingName == "visible5")
					{
						_value = (this.prefabDiff + this.biomeDiff >= 5).ToString();
						return true;
					}
				}
			}
			else if (_bindingName == "visible2")
			{
				_value = (this.prefabDiff + this.biomeDiff >= 2).ToString();
				return true;
			}
		}
		else if (num <= 1551877922U)
		{
			if (num != 1535100303U)
			{
				if (num == 1551877922U)
				{
					if (_bindingName == "visible7")
					{
						_value = (this.prefabDiff + this.biomeDiff >= 7).ToString();
						return true;
					}
				}
			}
			else if (_bindingName == "visible4")
			{
				_value = (this.prefabDiff + this.biomeDiff >= 4).ToString();
				return true;
			}
		}
		else if (num != 1568655541U)
		{
			if (num != 4063875975U)
			{
				if (num == 4157284869U)
				{
					if (_bindingName == "locationname")
					{
						if (this.Prefab == null && this.Biome == null)
						{
							_value = "";
							return true;
						}
						_value = ((this.Prefab != null) ? this.Prefab.LocalizedName : this.Biome.LocalizedName);
						return true;
					}
				}
			}
			else if (_bindingName == "visible_half")
			{
				_value = this.showBiomeHalf.ToString();
				return true;
			}
		}
		else if (_bindingName == "visible6")
		{
			_value = (this.prefabDiff + this.biomeDiff >= 6).ToString();
			return true;
		}
		return false;
	}

	public override bool ParseAttribute(string name, string value, XUiController _parent)
	{
		bool flag = base.ParseAttribute(name, value, _parent);
		if (!flag)
		{
			if (!(name == "active_color"))
			{
				if (!(name == "inactive_color"))
				{
					return false;
				}
				this.inactiveColor = StringParsers.ParseColor32(value);
			}
			else
			{
				this.difficultyActiveColor = StringParsers.ParseColor32(value);
			}
			return true;
		}
		return flag;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public float showTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public Prefab lastPrefab;

	[PublicizedFrom(EAccessModifier.Private)]
	public Prefab prefab;

	[PublicizedFrom(EAccessModifier.Private)]
	public BiomeDefinition lastBiome;

	[PublicizedFrom(EAccessModifier.Private)]
	public BiomeDefinition biome;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool ignoreFirst;

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityPlayerLocal LocalPlayer;

	[PublicizedFrom(EAccessModifier.Private)]
	public int prefabDiff;

	[PublicizedFrom(EAccessModifier.Private)]
	public int biomeDiff;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool showBiomeHalf;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color difficultyActiveColor = Color.red;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color biomeActiveColor = new Color(1f, 0.5f, 0f);

	[PublicizedFrom(EAccessModifier.Private)]
	public Color inactiveColor = Color.grey;

	[PublicizedFrom(EAccessModifier.Private)]
	public FastTags<TagGroup.Poi> partTag = FastTags<TagGroup.Poi>.Parse("part");

	[PublicizedFrom(EAccessModifier.Private)]
	public FastTags<TagGroup.Poi> streetTileTag = FastTags<TagGroup.Poi>.Parse("streettile");

	[PublicizedFrom(EAccessModifier.Private)]
	public FastTags<TagGroup.Poi> navOnlyTileTag = FastTags<TagGroup.Poi>.Parse("navonly");

	[PublicizedFrom(EAccessModifier.Private)]
	public FastTags<TagGroup.Poi> hideUITag = FastTags<TagGroup.Poi>.Parse("hideui");

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterXuiRgbaColor activeColorFormatter = new CachedStringFormatterXuiRgbaColor();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterXuiRgbaColor inactiveColorFormatter = new CachedStringFormatterXuiRgbaColor();
}
