using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_CharacterFrameWindow : XUiController
{
	public override void Init()
	{
		base.Init();
		this.previewFrame = base.GetChildById("playerPreviewSDCS");
		this.previewFrame.OnPress += this.PreviewFrame_OnPress;
		this.previewFrame.OnHover += this.PreviewFrame_OnHover;
		this.lblLevel = (XUiV_Label)base.GetChildById("levelNumber").ViewComponent;
		this.lblName = (XUiV_Label)base.GetChildById("characterName").ViewComponent;
		this.textPreview = (XUiV_Texture)base.GetChildById("playerPreviewSDCS").ViewComponent;
		this.isDirty = true;
		this.characterButton = base.GetChildById("characterButton");
		if (this.characterButton != null)
		{
			this.characterButton.OnPress += this.CharacterButton_OnPress;
		}
		this.statsButton = base.GetChildById("statButton");
		if (this.statsButton != null)
		{
			this.statsButton.OnPress += this.StatsButton_OnPress;
		}
		this.coreStatsButton = base.GetChildById("coreStatButton");
		if (this.coreStatsButton != null)
		{
			this.coreStatsButton.OnPress += this.CoreStatsButton_OnPress;
		}
		XUiM_PlayerEquipment.HandleRefreshEquipment += this.XUiM_PlayerEquipment_HandleRefreshEquipment;
		this.levelLabel = Localization.Get("lblLevel", false);
		this.WeatherSlot = (base.GetChildById("weatherSlot") as XUiC_EquipmentStack);
		XUiC_EquipmentStackGrid childByType = base.GetChildByType<XUiC_EquipmentStackGrid>();
		if (childByType != null)
		{
			childByType.ExtraSlot = this.WeatherSlot;
		}
		base.xui.playerUI.OnUIShutdown += this.HandleUIShutdown;
		base.xui.OnShutdown += this.HandleUIShutdown;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void StatsButton_OnPress(XUiController _sender, int _mouseButton)
	{
		this.currentTab = XUiC_CharacterFrameWindow.Tabs.Stats;
		if (this.characterButton != null)
		{
			((XUiV_Button)this.characterButton.ViewComponent).Selected = false;
		}
		if (this.statsButton != null)
		{
			((XUiV_Button)this.statsButton.ViewComponent).Selected = true;
		}
		if (this.coreStatsButton != null)
		{
			((XUiV_Button)this.coreStatsButton.ViewComponent).Selected = false;
		}
		this.isDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CharacterButton_OnPress(XUiController _sender, int _mouseButton)
	{
		this.currentTab = XUiC_CharacterFrameWindow.Tabs.Character;
		if (this.characterButton != null)
		{
			((XUiV_Button)this.characterButton.ViewComponent).Selected = true;
		}
		if (this.statsButton != null)
		{
			((XUiV_Button)this.statsButton.ViewComponent).Selected = false;
		}
		if (this.coreStatsButton != null)
		{
			((XUiV_Button)this.coreStatsButton.ViewComponent).Selected = false;
		}
		this.isDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CoreStatsButton_OnPress(XUiController _sender, int _mouseButton)
	{
		this.currentTab = XUiC_CharacterFrameWindow.Tabs.CoreStats;
		if (this.characterButton != null)
		{
			((XUiV_Button)this.characterButton.ViewComponent).Selected = false;
		}
		if (this.statsButton != null)
		{
			((XUiV_Button)this.statsButton.ViewComponent).Selected = false;
		}
		if (this.coreStatsButton != null)
		{
			((XUiV_Button)this.coreStatsButton.ViewComponent).Selected = true;
		}
		this.isDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleUIShutdown()
	{
		base.xui.playerUI.OnUIShutdown -= this.HandleUIShutdown;
		base.xui.OnShutdown -= this.HandleUIShutdown;
		XUiM_PlayerEquipment.HandleRefreshEquipment -= this.XUiM_PlayerEquipment_HandleRefreshEquipment;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PreviewFrame_OnHover(XUiController _sender, bool _isOver)
	{
		this.renderTextureSystem.RotateTarget(Time.deltaTime * 10f);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PreviewFrame_OnPress(XUiController _sender, int _mouseButton)
	{
		if (base.xui.dragAndDrop.CurrentStack != ItemStack.Empty)
		{
			ItemStack itemStack = base.xui.PlayerEquipment.EquipItem(base.xui.dragAndDrop.CurrentStack);
			if (base.xui.dragAndDrop.CurrentStack != itemStack)
			{
				base.xui.dragAndDrop.CurrentStack = itemStack;
				base.xui.dragAndDrop.PickUpType = XUiC_ItemStack.StackLocationTypes.Equipment;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void XUiM_PlayerEquipment_HandleRefreshEquipment(XUiM_PlayerEquipment _playerEquipment)
	{
	}

	public override void Update(float _dt)
	{
		if (GameManager.Instance == null || GameManager.Instance.World == null)
		{
			return;
		}
		if (this.ep == null)
		{
			this.ep = base.xui.playerUI.entityPlayer;
		}
		if (this.currentTab != XUiC_CharacterFrameWindow.Tabs.Character && Time.time > this.updateTime)
		{
			this.updateTime = Time.time + 0.25f;
			base.RefreshBindings(this.isDirty);
		}
		if (this.isDirty)
		{
			if (this.player == null)
			{
				return;
			}
			if (this.WeatherSlot != null)
			{
				this.WeatherSlot.EquipSlot = EquipmentSlots.WeatherKit;
			}
			this.lblLevel.Text = string.Format(this.levelLabel, this.player.Progression.GetLevel());
			this.lblName.Text = this.player.PlayerDisplayName;
			this.isDirty = false;
			base.RefreshBindings(false);
		}
		if (this.isPreviewDirty)
		{
			this.MakePreview();
		}
		this.textPreview.Texture = this.renderTextureSystem.RenderTex;
		if (this.previewSDCSObj != null)
		{
			this.previewSDCSObj.transform.localEulerAngles = new Vector3(0f, 180f, 0f);
		}
		base.Update(_dt);
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.isDirty = true;
		this.isPreviewDirty = true;
		this.player = base.xui.playerUI.entityPlayer;
		if (this.previewFrame != null)
		{
			this.previewFrame.OnPress -= this.PreviewFrame_OnPress;
			this.previewFrame.OnHover -= this.PreviewFrame_OnHover;
		}
		this.previewFrame = base.GetChildById("previewFrameSDCS");
		this.previewFrame.OnPress += this.PreviewFrame_OnPress;
		this.previewFrame.OnHover += this.PreviewFrame_OnHover;
		this.textPreview = (XUiV_Texture)base.GetChildById("playerPreviewSDCS").ViewComponent;
		if (this.renderTextureSystem.ParentGO == null)
		{
			this.renderTextureSystem.Create("playerpreview", new GameObject(), new Vector3(0f, -0.5f, 3f), new Vector3(0f, -0.2f, 7.5f), this.textPreview.Size, true, false, 1f);
		}
		this.displayInfoEntries = UIDisplayInfoManager.Current.GetCharacterDisplayInfo();
		if (this.player as EntityPlayerLocal != null && this.player.emodel as EModelSDCS != null)
		{
			XUiM_PlayerEquipment.HandleRefreshEquipment += this.XUiM_PlayerEquipment_HandleRefreshEquipment1;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void XUiM_PlayerEquipment_HandleRefreshEquipment1(XUiM_PlayerEquipment playerEquipment)
	{
		if (!base.IsOpen)
		{
			return;
		}
		this.MakePreview();
	}

	public override void OnClose()
	{
		base.OnClose();
		XUiM_PlayerEquipment.HandleRefreshEquipment -= this.XUiM_PlayerEquipment_HandleRefreshEquipment1;
		SDCSUtils.DestroyViz(this.previewSDCSObj, false);
		this.renderTextureSystem.Cleanup();
	}

	public void MakePreview()
	{
		if (this.ep == null)
		{
			return;
		}
		if (this.ep.emodel == null)
		{
			return;
		}
		EModelSDCS emodelSDCS = this.ep.emodel as EModelSDCS;
		if (emodelSDCS != null)
		{
			this.isPreviewDirty = false;
			SDCSUtils.CreateVizUI(emodelSDCS.Archetype, ref this.previewSDCSObj, ref this.transformCatalog, this.ep);
			Utils.SetLayerRecursively(this.previewSDCSObj, 11, null);
			Transform transform = this.previewSDCSObj.transform;
			transform.SetParent(this.renderTextureSystem.ParentGO.transform, false);
			transform.localPosition = new Vector3(0.022f, -2.9f, 12f);
			transform.localEulerAngles = new Vector3(0f, 180f, 0f);
			this.renderTextureSystem.SetOrtho(true, 0.95f);
		}
	}

	public override bool GetBindingValue(ref string value, string bindingName)
	{
		uint num = <PrivateImplementationDetails>.ComputeStringHash(bindingName);
		if (num <= 2395478116U)
		{
			if (num <= 732973786U)
			{
				if (num <= 145239280U)
				{
					if (num <= 75296162U)
					{
						if (num != 8937094U)
						{
							if (num == 75296162U)
							{
								if (bindingName == "playerfoodtitle")
								{
									value = Localization.Get("xuiFood", false);
									return true;
								}
							}
						}
						else if (bindingName == "playerhealth")
						{
							value = ((this.player != null) ? this.playerHealthFormatter.Format((int)XUiM_Player.GetHealth(this.player)) : "");
							return true;
						}
					}
					else if (num != 125916223U)
					{
						if (num == 145239280U)
						{
							if (bindingName == "playerarmorratingtitle")
							{
								value = Localization.Get("statPhysicalDamageResist", false);
								return true;
							}
						}
					}
					else if (bindingName == "playercurrentlife")
					{
						value = ((this.player != null) ? XUiM_Player.GetCurrentLife(this.player) : "");
						return true;
					}
				}
				else if (num <= 304163417U)
				{
					if (num != 234495987U)
					{
						if (num == 304163417U)
						{
							if (bindingName == "playercoretemp")
							{
								value = ((this.player != null) ? XUiM_Player.GetCoreTemp(this.player) : "");
								return true;
							}
						}
					}
					else if (bindingName == "playerdeathstitle")
					{
						value = Localization.Get("xuiDeaths", false);
						return true;
					}
				}
				else if (num != 672688503U)
				{
					if (num != 696376978U)
					{
						if (num == 732973786U)
						{
							if (bindingName == "showcharactersdcs")
							{
								value = (this.currentTab == XUiC_CharacterFrameWindow.Tabs.Character && this.player != null && this.player.emodel as EModelSDCS != null).ToString();
								return true;
							}
						}
					}
					else if (bindingName == "playerlootstagetitle")
					{
						value = Localization.Get("xuiLootstage", false);
						return true;
					}
				}
				else if (bindingName == "playercurrentlifetitle")
				{
					value = Localization.Get("xuiCurrentLife", false);
					return true;
				}
			}
			else if (num <= 1477941828U)
			{
				if (num <= 885900949U)
				{
					if (num != 782575427U)
					{
						if (num == 885900949U)
						{
							if (bindingName == "playertravelledtitle")
							{
								value = Localization.Get("xuiKMTravelled", false);
								return true;
							}
						}
					}
					else if (bindingName == "playerdeaths")
					{
						value = ((this.player != null) ? this.playerDeathsFormatter.Format(XUiM_Player.GetDeaths(this.player)) : "");
						return true;
					}
				}
				else if (num != 965025103U)
				{
					if (num != 1009276468U)
					{
						if (num == 1477941828U)
						{
							if (bindingName == "playerlongestlife")
							{
								value = ((this.player != null) ? XUiM_Player.GetLongestLife(this.player) : "");
								return true;
							}
						}
					}
					else if (bindingName == "playerxptonextleveltitle")
					{
						value = Localization.Get("xuiXPToNextLevel", false);
						return true;
					}
				}
				else if (bindingName == "playerwater")
				{
					value = ((this.player != null) ? this.playerWaterFormatter.Format(XUiM_Player.GetWater(this.player)) : "");
					return true;
				}
			}
			else if (num <= 2023588471U)
			{
				if (num != 1811778199U)
				{
					if (num == 2023588471U)
					{
						if (bindingName == "playerzombiekillstitle")
						{
							value = Localization.Get("xuiZombieKills", false);
							return true;
						}
					}
				}
				else if (bindingName == "playeritemscraftedtitle")
				{
					value = Localization.Get("xuiItemsCrafted", false);
					return true;
				}
			}
			else if (num != 2186126559U)
			{
				if (num != 2219475343U)
				{
					if (num == 2395478116U)
					{
						if (bindingName == "playerlootstage")
						{
							value = ((this.player != null) ? this.player.GetHighestPartyLootStage(0f, 0f).ToString() : "");
							return true;
						}
					}
				}
				else if (bindingName == "playermaxstamina")
				{
					value = ((this.player != null) ? this.playerMaxStaminaFormatter.Format((int)XUiM_Player.GetMaxStamina(this.player)) : "");
					return true;
				}
			}
			else if (bindingName == "playeritemscrafted")
			{
				value = ((this.player != null) ? this.playerItemsCraftedFormatter.Format(XUiM_Player.GetItemsCrafted(this.player)) : "");
				return true;
			}
		}
		else if (num <= 3537464933U)
		{
			if (num <= 3249756066U)
			{
				if (num <= 2587631291U)
				{
					if (num != 2532548756U)
					{
						if (num == 2587631291U)
						{
							if (bindingName == "showcore")
							{
								value = (this.currentTab == XUiC_CharacterFrameWindow.Tabs.CoreStats).ToString();
								return true;
							}
						}
					}
					else if (bindingName == "playerfood")
					{
						value = ((this.player != null) ? this.playerFoodFormatter.Format(XUiM_Player.GetFood(this.player)) : "");
						return true;
					}
				}
				else if (num != 2974192615U)
				{
					if (num != 3042900123U)
					{
						if (num == 3249756066U)
						{
							if (bindingName == "playerfoodmax")
							{
								value = ((this.player != null) ? this.playerFoodMaxFormatter.Format(XUiM_Player.GetFoodMax(this.player)) : "");
								return true;
							}
						}
					}
					else if (bindingName == "playerpvpkills")
					{
						value = ((this.player != null) ? this.playerPvpKillsFormatter.Format(XUiM_Player.GetPlayerKills(this.player)) : "");
						return true;
					}
				}
				else if (bindingName == "playerwatertitle")
				{
					value = Localization.Get("xuiWater", false);
					return true;
				}
			}
			else if (num <= 3275992332U)
			{
				if (num != 3257770903U)
				{
					if (num == 3275992332U)
					{
						if (bindingName == "playermaxhealth")
						{
							value = ((this.player != null) ? this.playerMaxHealthFormatter.Format((int)XUiM_Player.GetMaxHealth(this.player)) : "");
							return true;
						}
					}
				}
				else if (bindingName == "showstats")
				{
					value = (this.currentTab == XUiC_CharacterFrameWindow.Tabs.Stats).ToString();
					return true;
				}
			}
			else if (num != 3371877161U)
			{
				if (num != 3484390642U)
				{
					if (num == 3537464933U)
					{
						if (bindingName == "playerstamina")
						{
							value = ((this.player != null) ? this.playerStaminaFormatter.Format((int)XUiM_Player.GetStamina(this.player)) : "");
							return true;
						}
					}
				}
				else if (bindingName == "playerlongestlifetitle")
				{
					value = Localization.Get("xuiLongestLife", false);
					return true;
				}
			}
			else if (bindingName == "playerstaminatitle")
			{
				value = Localization.Get("lblStamina", false);
				return true;
			}
		}
		else if (num <= 3931175545U)
		{
			if (num <= 3712331684U)
			{
				if (num != 3705263762U)
				{
					if (num == 3712331684U)
					{
						if (bindingName == "playermodifiedcurrentfood")
						{
							value = ((this.player != null) ? this.playerFoodFormatter.Format(XUiM_Player.GetModifiedCurrentFood(this.player)) : "");
							return true;
						}
					}
				}
				else if (bindingName == "playerxptonextlevel")
				{
					value = ((this.player != null) ? this.playerXpToNextLevelFormatter.Format(XUiM_Player.GetXPToNextLevel(this.player) + this.player.Progression.ExpDeficit) : "");
					return true;
				}
			}
			else if (num != 3887827771U)
			{
				if (num != 3900606022U)
				{
					if (num == 3931175545U)
					{
						if (bindingName == "playertravelled")
						{
							value = ((this.player != null) ? XUiM_Player.GetKMTraveled(this.player) : "");
							return true;
						}
					}
				}
				else if (bindingName == "playerarmorrating")
				{
					value = ((this.player != null) ? this.playerArmorRatingFormatter.Format((int)EffectManager.GetValue(PassiveEffects.PhysicalDamageResist, null, 0f, this.player, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false)) : "");
					return true;
				}
			}
			else if (bindingName == "playerpvpkillstitle")
			{
				value = Localization.Get("xuiPlayerKills", false);
				return true;
			}
		}
		else if (num <= 4031300656U)
		{
			if (num != 4025935093U)
			{
				if (num == 4031300656U)
				{
					if (bindingName == "playerhealthtitle")
					{
						value = Localization.Get("lblHealth", false);
						return true;
					}
				}
			}
			else if (bindingName == "playercoretemptitle")
			{
				value = Localization.Get("xuiFeelsLike", false);
				return true;
			}
		}
		else if (num != 4077864767U)
		{
			if (num != 4107995367U)
			{
				if (num == 4159374943U)
				{
					if (bindingName == "playermodifiedcurrentwater")
					{
						value = ((this.player != null) ? this.playerWaterFormatter.Format(XUiM_Player.GetModifiedCurrentWater(this.player)) : "");
						return true;
					}
				}
			}
			else if (bindingName == "playerwatermax")
			{
				value = ((this.player != null) ? this.playerWaterMaxFormatter.Format(XUiM_Player.GetWaterMax(this.player)) : "");
				return true;
			}
		}
		else if (bindingName == "playerzombiekills")
		{
			value = ((this.player != null) ? this.playerZombieKillsFormatter.Format(XUiM_Player.GetZombieKills(this.player)) : "");
			return true;
		}
		if (bindingName.StartsWith("playerstattitle"))
		{
			if (this.player != null)
			{
				int index = Convert.ToInt32(bindingName.Replace("playerstattitle", "")) - 1;
				value = this.GetStatTitle(index);
			}
			else
			{
				value = "";
			}
			return true;
		}
		if (bindingName.StartsWith("playerstat"))
		{
			if (this.player != null)
			{
				int index2 = Convert.ToInt32(bindingName.Replace("playerstat", "")) - 1;
				value = this.GetStatValue(index2);
			}
			else
			{
				value = "";
			}
			return true;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string GetStatTitle(int index)
	{
		if (this.displayInfoEntries.Count <= index)
		{
			return "";
		}
		if (this.displayInfoEntries[index].TitleOverride != null)
		{
			return this.displayInfoEntries[index].TitleOverride;
		}
		return UIDisplayInfoManager.Current.GetLocalizedName(this.displayInfoEntries[index].StatType);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string GetStatValue(int index)
	{
		if (this.displayInfoEntries.Count <= index)
		{
			return "";
		}
		DisplayInfoEntry displayInfoEntry = this.displayInfoEntries[index];
		return XUiM_Player.GetStatValue(displayInfoEntry.StatType, base.xui.playerUI.entityPlayer, displayInfoEntry);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label lblLevel;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label lblName;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController previewFrame;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController characterButton;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController statsButton;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController coreStatsButton;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_CharacterFrameWindow.Tabs currentTab;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Texture textPreview;

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityPlayer ep;

	[PublicizedFrom(EAccessModifier.Private)]
	public Camera cam;

	public RuntimeAnimatorController animationController;

	public float atlasResolutionScale;

	[PublicizedFrom(EAccessModifier.Private)]
	public RenderTextureSystem renderTextureSystem = new RenderTextureSystem();

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isDirty;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isPreviewDirty;

	[PublicizedFrom(EAccessModifier.Private)]
	public string levelLabel;

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityPlayer player;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<DisplayInfoEntry> displayInfoEntries;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_EquipmentStack WeatherSlot;

	[PublicizedFrom(EAccessModifier.Private)]
	public float updateTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public GameObject previewSDCSObj;

	[PublicizedFrom(EAccessModifier.Private)]
	public SDCSUtils.TransformCatalog transformCatalog;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterInt playerDeathsFormatter = new CachedStringFormatterInt();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterInt playerHealthFormatter = new CachedStringFormatterInt();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterInt playerStaminaFormatter = new CachedStringFormatterInt();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterInt playerMaxHealthFormatter = new CachedStringFormatterInt();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterInt playerMaxStaminaFormatter = new CachedStringFormatterInt();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterFloat playerFoodFormatter = new CachedStringFormatterFloat("0");

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterFloat playerWaterFormatter = new CachedStringFormatterFloat("0");

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterInt playerFoodMaxFormatter = new CachedStringFormatterInt();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterInt playerWaterMaxFormatter = new CachedStringFormatterInt();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterInt playerItemsCraftedFormatter = new CachedStringFormatterInt();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterInt playerPvpKillsFormatter = new CachedStringFormatterInt();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterInt playerZombieKillsFormatter = new CachedStringFormatterInt();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterInt playerXpToNextLevelFormatter = new CachedStringFormatterInt();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterInt playerArmorRatingFormatter = new CachedStringFormatterInt();

	[PublicizedFrom(EAccessModifier.Private)]
	public enum Tabs
	{
		Character,
		Stats,
		CoreStats
	}
}
