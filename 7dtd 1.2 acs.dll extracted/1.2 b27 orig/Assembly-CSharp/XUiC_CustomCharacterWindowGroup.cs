using System;
using System.Globalization;
using GUI_2;
using Platform;
using UnityEngine.Scripting;

[Preserve]
[PublicizedFrom(EAccessModifier.Internal)]
public class XUiC_CustomCharacterWindowGroup : XUiController
{
	public override void Init()
	{
		base.Init();
		XUiC_CustomCharacterWindowGroup.ID = this.windowGroup.ID;
		this.previewWindow = base.GetChildByType<XUiC_SDCSPreviewWindow>();
		this.btnBack = (XUiC_SimpleButton)base.GetChildById("btnBack");
		this.btnBack.OnPressed += this.BtnBack_OnPress;
		this.btnApply = (XUiC_SimpleButton)base.GetChildById("btnApply");
		this.btnApply.OnPressed += this.BtnApply_OnPress;
		this.RefreshApplyLabel();
		this.btnRandomize = (XUiC_SimpleButton)base.GetChildById("btnRandomize");
		this.btnRandomize.OnPressed += this.BtnRandomize_OnPressed;
		this.races = (XUiC_ComboBoxList<XUiC_CustomCharacterWindowGroup.NameData>)base.GetChildById("cbxRace");
		this.races.OnValueChanged += this.Races_OnValueChanged;
		this.genders = (XUiC_ComboBoxList<XUiC_CustomCharacterWindowGroup.NameData>)base.GetChildById("cbxGender");
		this.genders.OnValueChanged += this.Genders_OnValueChanged;
		this.eyeColors = (XUiC_ComboBoxList<XUiC_CustomCharacterWindowGroup.NameData>)base.GetChildById("cbxEyeColor");
		this.eyeColors.OnValueChanged += this.EyeColors_OnValueChanged;
		this.hairs = (XUiC_ComboBoxList<XUiC_CustomCharacterWindowGroup.NameData>)base.GetChildById("cbxHairStyle");
		this.hairs.OnValueChanged += this.Hairs_OnValueChanged;
		this.hairColors = (XUiC_ComboBoxList<XUiC_CustomCharacterWindowGroup.NameData>)base.GetChildById("cbxHairColor");
		this.hairColors.OnValueChanged += this.HairColors_OnValueChanged;
		this.variants = (XUiC_ComboBoxList<XUiC_CustomCharacterWindowGroup.NameData>)base.GetChildById("cbxFace");
		this.variants.OnValueChanged += this.Variants_OnValueChanged;
		this.mustaches = (XUiC_ComboBoxList<XUiC_CustomCharacterWindowGroup.NameData>)base.GetChildById("cbxMustaches");
		this.mustaches.OnValueChanged += this.Mustaches_OnValueChanged;
		this.chops = (XUiC_ComboBoxList<XUiC_CustomCharacterWindowGroup.NameData>)base.GetChildById("cbxChops");
		this.chops.OnValueChanged += this.Chops_OnValueChanged;
		this.beards = (XUiC_ComboBoxList<XUiC_CustomCharacterWindowGroup.NameData>)base.GetChildById("cbxBeards");
		this.beards.OnValueChanged += this.Beards_OnValueChanged;
		XUiController childById = base.GetChildById("btnLockRace");
		if (childById != null)
		{
			this.btnLockRace = (XUiV_Button)childById.ViewComponent;
			childById.OnPress += this.BtnLockRace_OnPress;
		}
		childById = base.GetChildById("btnLockGender");
		if (childById != null)
		{
			this.btnLockGender = (XUiV_Button)childById.ViewComponent;
			childById.OnPress += this.BtnLockGender_OnPress;
		}
		childById = base.GetChildById("btnLockFace");
		if (childById != null)
		{
			this.btnLockFace = (XUiV_Button)childById.ViewComponent;
			childById.OnPress += this.BtnLockFace_OnPress;
		}
		childById = base.GetChildById("btnLockEyeColor");
		if (childById != null)
		{
			this.btnLockEyeColor = (XUiV_Button)childById.ViewComponent;
			childById.OnPress += this.BtnLockEyeColor_OnPress;
		}
		childById = base.GetChildById("btnLockHairStyle");
		if (childById != null)
		{
			this.btnLockHairStyle = (XUiV_Button)childById.ViewComponent;
			childById.OnPress += this.BtnLockHairStyle_OnPress;
		}
		childById = base.GetChildById("btnLockHairColor");
		if (childById != null)
		{
			this.btnLockHairColor = (XUiV_Button)childById.ViewComponent;
			childById.OnPress += this.BtnLockHairColor_OnPress;
		}
		childById = base.GetChildById("btnLockMustaches");
		if (childById != null)
		{
			this.btnLockMustaches = (XUiV_Button)childById.ViewComponent;
			childById.OnPress += this.BtnLockMustache_OnPress;
		}
		childById = base.GetChildById("btnLockChops");
		if (childById != null)
		{
			this.btnLockChops = (XUiV_Button)childById.ViewComponent;
			childById.OnPress += this.BtnLockChops_OnPress;
		}
		childById = base.GetChildById("btnLockBeards");
		if (childById != null)
		{
			this.btnLockBeards = (XUiV_Button)childById.ViewComponent;
			childById.OnPress += this.BtnLockBeards_OnPress;
		}
		this.CustomCharacterWindow = base.GetChildByType<XUiC_CustomCharacterWindow>();
		this.gr = GameRandomManager.Instance.CreateGameRandom();
		SDCSDataUtils.SetupData();
		base.RegisterForInputStyleChanges();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RefreshApplyLabel()
	{
		InControlExtensions.SetApplyButtonString(this.btnApply, "xuiApply");
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void InputStyleChanged(PlayerInputManager.InputStyle _oldStyle, PlayerInputManager.InputStyle _newStyle)
	{
		base.InputStyleChanged(_oldStyle, _newStyle);
		this.RefreshApplyLabel();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetLockButtonState(XUiV_Button btn, bool isLocked)
	{
		btn.DefaultSpriteName = (isLocked ? this.CustomCharacterWindow.lockedSprite : this.CustomCharacterWindow.unlockedSprite);
		btn.DefaultSpriteColor = (isLocked ? this.CustomCharacterWindow.lockedColor : this.CustomCharacterWindow.unlockedColor);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnLockRace_OnPress(XUiController _sender, int _mouseButton)
	{
		this.lockedRace = !this.lockedRace;
		this.SetLockButtonState(this.btnLockRace, this.lockedRace);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnLockGender_OnPress(XUiController _sender, int _mouseButton)
	{
		this.lockedGenders = !this.lockedGenders;
		this.SetLockButtonState(this.btnLockGender, this.lockedGenders);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnLockFace_OnPress(XUiController _sender, int _mouseButton)
	{
		this.lockedVariants = !this.lockedVariants;
		this.SetLockButtonState(this.btnLockFace, this.lockedVariants);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnLockEyeColor_OnPress(XUiController _sender, int _mouseButton)
	{
		this.lockedEyeColors = !this.lockedEyeColors;
		this.SetLockButtonState(this.btnLockEyeColor, this.lockedEyeColors);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnLockHairStyle_OnPress(XUiController _sender, int _mouseButton)
	{
		this.lockedHairs = !this.lockedHairs;
		this.SetLockButtonState(this.btnLockHairStyle, this.lockedHairs);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnLockHairColor_OnPress(XUiController _sender, int _mouseButton)
	{
		this.lockedHairColors = !this.lockedHairColors;
		this.SetLockButtonState(this.btnLockHairColor, this.lockedHairColors);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnLockMustache_OnPress(XUiController _sender, int _mouseButton)
	{
		this.lockedMustaches = !this.lockedMustaches;
		this.SetLockButtonState(this.btnLockMustaches, this.lockedMustaches);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnLockChops_OnPress(XUiController _sender, int _mouseButton)
	{
		this.lockedChops = !this.lockedChops;
		this.SetLockButtonState(this.btnLockChops, this.lockedChops);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnLockBeards_OnPress(XUiController _sender, int _mouseButton)
	{
		this.lockedBeards = !this.lockedBeards;
		this.SetLockButtonState(this.btnLockBeards, this.lockedBeards);
	}

	public override void OnOpen()
	{
		this.windowGroup.openWindowOnEsc = XUiC_OptionsProfiles.ID;
		base.OnOpen();
		this.playerProfile = null;
		this.archetype = null;
		this.archetype = Archetype.GetArchetype(ProfileSDF.CurrentProfileName());
		if (this.archetype != null)
		{
			this.archetype = this.archetype.Clone();
		}
		else
		{
			string profileName = ProfileSDF.CurrentProfileName();
			this.playerProfile = PlayerProfile.LoadProfile(profileName).Clone();
		}
		base.xui.calloutWindow.AddCallout(UIUtils.ButtonIcon.FaceButtonWest, "lblChangeView", XUiC_GamepadCalloutWindow.CalloutType.CharacterEditor);
		base.xui.calloutWindow.AddCallout(UIUtils.ButtonIcon.LeftTrigger, "igcoRotateLeft", XUiC_GamepadCalloutWindow.CalloutType.CharacterEditor);
		base.xui.calloutWindow.AddCallout(UIUtils.ButtonIcon.RightTrigger, "igcoRotateRight", XUiC_GamepadCalloutWindow.CalloutType.CharacterEditor);
		base.xui.calloutWindow.EnableCallouts(XUiC_GamepadCalloutWindow.CalloutType.CharacterEditor, 0f);
		this.SetInitialOptions();
		base.RefreshBindings(false);
	}

	public override void OnClose()
	{
		base.OnClose();
		base.xui.calloutWindow.ClearCallouts(XUiC_GamepadCalloutWindow.CalloutType.CharacterEditor);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetInitialOptions()
	{
		if (this.playerProfile != null)
		{
			this.SetupGenders();
			this.genders.SelectedIndex = (this.playerProfile.IsMale ? 1 : 0);
			this.SetupRaces(this.playerProfile.IsMale);
			this.SetupVariants(this.playerProfile.IsMale, this.playerProfile.RaceName);
			this.SetupHairStyles(this.playerProfile.IsMale);
			this.SetupMustaches(this.playerProfile.IsMale);
			this.SetupChops(this.playerProfile.IsMale);
			this.SetupBeards(this.playerProfile.IsMale);
			this.SetupEyeColors();
			this.SetupHairColors();
			this.SetSelectedRace(this.playerProfile.RaceName, false);
			this.SetSelectedVariant(this.playerProfile.VariantNumber, false);
			this.SetSelectedEyeColor(this.playerProfile.EyeColor, false);
			this.SetSelectedHair(this.playerProfile.HairName, false);
			this.SetSelectedMustache(this.playerProfile.MustacheName, false);
			this.SetSelectedChops(this.playerProfile.ChopsName, false);
			this.SetSelectedBeard(this.playerProfile.BeardName, false);
			this.SetSelectedHairColor(this.playerProfile.HairColor, false);
		}
		else
		{
			this.genders.SelectedIndex = (this.playerProfile.IsMale ? 1 : 0);
			this.SetupRaces(this.archetype.IsMale);
			this.SetupVariants(this.archetype.IsMale, this.archetype.Race);
			this.SetupHairStyles(this.archetype.IsMale);
			this.SetupMustaches(this.archetype.IsMale);
			this.SetupChops(this.archetype.IsMale);
			this.SetupBeards(this.archetype.IsMale);
			this.SetupEyeColors();
			this.SetupHairColors();
			this.SetSelectedRace(this.archetype.Race, false);
			this.SetSelectedVariant(this.archetype.Variant, false);
			this.SetSelectedEyeColor(this.archetype.EyeColorName, false);
			this.SetSelectedHair(this.archetype.Hair, false);
			this.SetSelectedMustache(this.archetype.MustacheName, false);
			this.SetSelectedChops(this.archetype.ChopsName, false);
			this.SetSelectedBeard(this.archetype.BeardName, false);
			this.SetSelectedHairColor(this.playerProfile.HairColor, false);
		}
		this.ResetLocks();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ResetLocks()
	{
		this.lockedBeards = (this.lockedChops = (this.lockedEyeColors = (this.lockedGenders = (this.lockedHairColors = (this.lockedHairs = (this.lockedMustaches = (this.lockedRace = (this.lockedVariants = false))))))));
		this.SetLockButtonState(this.btnLockGender, false);
		this.SetLockButtonState(this.btnLockRace, false);
		this.SetLockButtonState(this.btnLockFace, false);
		this.SetLockButtonState(this.btnLockEyeColor, false);
		this.SetLockButtonState(this.btnLockHairStyle, false);
		this.SetLockButtonState(this.btnLockHairColor, false);
		this.SetLockButtonState(this.btnLockMustaches, false);
		this.SetLockButtonState(this.btnLockChops, false);
		this.SetLockButtonState(this.btnLockBeards, false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnApply_OnPress(XUiController _sender, int _mouseButton)
	{
		if (this.HasChanges)
		{
			if (this.playerProfile != null)
			{
				ProfileSDF.SaveProfile(ProfileSDF.CurrentProfileName(), this.playerProfile.ProfileArchetype, this.playerProfile.IsMale, this.playerProfile.RaceName, this.playerProfile.VariantNumber, this.playerProfile.EyeColor, this.playerProfile.HairName, this.playerProfile.HairColor, this.playerProfile.MustacheName, this.playerProfile.ChopsName, this.playerProfile.BeardName);
			}
			else if (this.archetype != null)
			{
				Archetype.SetArchetype(this.archetype);
				Archetype.SaveArchetypesToFile();
			}
			this.HasChanges = false;
		}
		this.OpenOptions();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnBack_OnPress(XUiController _sender, int _mouseButton)
	{
		this.OpenOptions();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnRandomize_OnPressed(XUiController _sender, int _mouseButton)
	{
		if (!this.lockedGenders)
		{
			this.genders.SelectedIndex = this.gr.RandomRange(2);
			this.genders.TriggerValueChangedEvent(this.genders.Elements[0]);
		}
		if (!this.lockedRace)
		{
			this.races.SelectedIndex = this.gr.RandomRange(this.races.Elements.Count);
			this.races.TriggerValueChangedEvent(this.races.Elements[0]);
		}
		if (!this.lockedVariants)
		{
			this.variants.SelectedIndex = this.gr.RandomRange(this.variants.Elements.Count);
			this.variants.TriggerValueChangedEvent(this.variants.Elements[0]);
		}
		if (!this.lockedEyeColors)
		{
			this.eyeColors.SelectedIndex = this.gr.RandomRange(this.eyeColors.Elements.Count);
			this.eyeColors.TriggerValueChangedEvent(this.eyeColors.Elements[0]);
		}
		if (!this.lockedHairs)
		{
			this.hairs.SelectedIndex = this.gr.RandomRange(this.hairs.Elements.Count);
			this.hairs.TriggerValueChangedEvent(this.hairs.Elements[0]);
		}
		if (!this.lockedHairColors)
		{
			this.hairColors.SelectedIndex = this.gr.RandomRange(this.hairColors.Elements.Count);
			this.hairColors.TriggerValueChangedEvent(this.hairColors.Elements[0]);
		}
		if (!this.lockedMustaches)
		{
			this.mustaches.SelectedIndex = this.gr.RandomRange(this.mustaches.Elements.Count);
			this.mustaches.TriggerValueChangedEvent(this.mustaches.Elements[0]);
		}
		if (!this.lockedChops)
		{
			this.chops.SelectedIndex = this.gr.RandomRange(this.chops.Elements.Count);
			this.chops.TriggerValueChangedEvent(this.chops.Elements[0]);
		}
		if (!this.lockedBeards)
		{
			this.beards.SelectedIndex = this.gr.RandomRange(this.beards.Elements.Count);
			this.beards.TriggerValueChangedEvent(this.beards.Elements[0]);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OpenOptions()
	{
		base.xui.playerUI.windowManager.Close(this.windowGroup.ID);
		base.xui.playerUI.windowManager.Open(XUiC_OptionsProfiles.ID, true, false, true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetupGenders()
	{
		this.genders.Elements.Clear();
		this.genders.Elements.Add(new XUiC_CustomCharacterWindowGroup.NameData("female", Localization.Get("xuiBoolMaleOff", false)));
		this.genders.Elements.Add(new XUiC_CustomCharacterWindowGroup.NameData("male", Localization.Get("xuiBoolMaleOn", false)));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetupRaces(bool isMale)
	{
		this.races.Elements.Clear();
		int num = 1;
		foreach (string text in SDCSDataUtils.GetRaceList(isMale))
		{
			this.races.Elements.Add(new XUiC_CustomCharacterWindowGroup.NameData(text, Localization.Get("xuiRace" + text, false)));
			num++;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetupVariants(bool isMale, string raceName)
	{
		this.variants.Elements.Clear();
		int num = 1;
		foreach (string internalName in SDCSDataUtils.GetVariantList(isMale, raceName))
		{
			this.variants.Elements.Add(new XUiC_CustomCharacterWindowGroup.NameData(internalName, Localization.Get("lblFace", false) + " " + num.ToString("00")));
			num++;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetupEyeColors()
	{
		this.eyeColors.Elements.Clear();
		int num = 1;
		foreach (string internalName in SDCSDataUtils.GetEyeColorNames())
		{
			this.eyeColors.Elements.Add(new XUiC_CustomCharacterWindowGroup.NameData(internalName, Localization.Get("xuiCharacterColorSlotEyes", false) + " " + num.ToString("00")));
			num++;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetupHairStyles(bool isMale)
	{
		this.hairs.Elements.Clear();
		this.hairs.Elements.Add(new XUiC_CustomCharacterWindowGroup.NameData("", Localization.Get("xuiCharacterHairStyle", false) + " 00"));
		int num = 1;
		foreach (string internalName in SDCSDataUtils.GetHairNames(isMale, SDCSDataUtils.HairTypes.Hair))
		{
			this.hairs.Elements.Add(new XUiC_CustomCharacterWindowGroup.NameData(internalName, Localization.Get("xuiCharacterHairStyle", false) + " " + num.ToString("00")));
			num++;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetupHairColors()
	{
		this.hairColors.Elements.Clear();
		int num = 1;
		foreach (SDCSDataUtils.HairColorData hairColorData in SDCSDataUtils.GetHairColorNames())
		{
			this.hairColors.Elements.Add(new XUiC_CustomCharacterWindowGroup.NameData(hairColorData.PrefabName, Localization.Get("xuiCharacterHairColor", false) + " " + num.ToString("00")));
			num++;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetupMustaches(bool isMale)
	{
		this.mustaches.Elements.Clear();
		this.mustaches.Elements.Add(new XUiC_CustomCharacterWindowGroup.NameData("", Localization.Get("xuiCharacterMustaches", false) + " 00"));
		foreach (string text in SDCSDataUtils.GetHairNames(isMale, SDCSDataUtils.HairTypes.Mustache))
		{
			string text2 = text;
			if (text2.Length == 1)
			{
				text2 = text2.Insert(0, "0");
			}
			this.mustaches.Elements.Add(new XUiC_CustomCharacterWindowGroup.NameData(text, Localization.Get("xuiCharacterMustaches", false) + " " + Localization.Get(text2, false)));
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetupChops(bool isMale)
	{
		this.chops.Elements.Clear();
		this.chops.Elements.Add(new XUiC_CustomCharacterWindowGroup.NameData("", Localization.Get("xuiCharacterChops", false) + " 00"));
		foreach (string text in SDCSDataUtils.GetHairNames(isMale, SDCSDataUtils.HairTypes.Chops))
		{
			string text2 = text;
			if (text2.Length == 1)
			{
				text2 = text2.Insert(0, "0");
			}
			this.chops.Elements.Add(new XUiC_CustomCharacterWindowGroup.NameData(text, Localization.Get("xuiCharacterChops", false) + " " + Localization.Get(text2, false)));
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetupBeards(bool isMale)
	{
		this.beards.Elements.Clear();
		this.beards.Elements.Add(new XUiC_CustomCharacterWindowGroup.NameData("", Localization.Get("xuiCharacterBeards", false) + " 00"));
		foreach (string text in SDCSDataUtils.GetHairNames(isMale, SDCSDataUtils.HairTypes.Beard))
		{
			string text2 = text;
			if (text2.Length == 1)
			{
				text2 = text2.Insert(0, "0");
			}
			this.beards.Elements.Add(new XUiC_CustomCharacterWindowGroup.NameData(text, Localization.Get("xuiCharacterBeards", false) + " " + Localization.Get(text2, false)));
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Genders_OnValueChanged(XUiController _sender, XUiC_CustomCharacterWindowGroup.NameData _oldValue, XUiC_CustomCharacterWindowGroup.NameData _newValue)
	{
		string text = _newValue.InternalName.ToLower();
		this.previewWindow.Archetype.Sex = text;
		bool isMale = text == "male";
		if (this.playerProfile != null)
		{
			this.playerProfile.IsMale = isMale;
		}
		string internalName = this.races.Value.InternalName;
		int variant = StringParsers.ParseSInt32(this.variants.Value.InternalName, 0, -1, NumberStyles.Integer);
		this.SetupRaces(isMale);
		this.SetSelectedRace(internalName, true);
		this.SetupVariants(isMale, this.races.Value.InternalName);
		this.SetSelectedVariant(variant, true);
		this.SetupHairStyles(isMale);
		this.SetupMustaches(isMale);
		this.SetupChops(isMale);
		this.SetupBeards(isMale);
		this.SetSelectedMustache("", true);
		this.SetSelectedChops("", true);
		this.SetSelectedBeard("", true);
		base.RefreshBindings(false);
		this.previewWindow.MakePreview();
		this.HasChanges = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Races_OnValueChanged(XUiController _sender, XUiC_CustomCharacterWindowGroup.NameData _oldValue, XUiC_CustomCharacterWindowGroup.NameData _newValue)
	{
		string text = _newValue.InternalName.ToLower();
		this.previewWindow.Archetype.Race = text;
		if (this.playerProfile != null)
		{
			this.playerProfile.RaceName = text;
		}
		int variant = StringParsers.ParseSInt32(this.variants.Value.InternalName, 0, -1, NumberStyles.Integer);
		this.SetupVariants(this.previewWindow.Archetype.IsMale, this.races.Value.InternalName);
		this.SetSelectedVariant(variant, true);
		this.previewWindow.MakePreview();
		this.HasChanges = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Variants_OnValueChanged(XUiController _sender, XUiC_CustomCharacterWindowGroup.NameData _oldValue, XUiC_CustomCharacterWindowGroup.NameData _newValue)
	{
		int num = StringParsers.ParseSInt32(_newValue.InternalName, 0, -1, NumberStyles.Integer);
		this.previewWindow.Archetype.Variant = num;
		if (this.playerProfile != null)
		{
			this.playerProfile.VariantNumber = num;
		}
		this.previewWindow.MakePreview();
		this.previewWindow.ZoomToHead();
		this.HasChanges = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Hairs_OnValueChanged(XUiController _sender, XUiC_CustomCharacterWindowGroup.NameData _oldValue, XUiC_CustomCharacterWindowGroup.NameData _newValue)
	{
		string text = _newValue.InternalName.ToLower();
		this.previewWindow.Archetype.Hair = text;
		this.previewWindow.Archetype.HairColor = this.hairColors.Value.InternalName;
		if (this.playerProfile != null)
		{
			this.playerProfile.HairName = text;
			this.playerProfile.HairColor = this.hairColors.Value.InternalName;
		}
		this.previewWindow.MakePreview();
		this.previewWindow.ZoomToHead();
		this.HasChanges = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HairColors_OnValueChanged(XUiController _sender, XUiC_CustomCharacterWindowGroup.NameData _oldValue, XUiC_CustomCharacterWindowGroup.NameData _newValue)
	{
		string hairColor = _newValue.InternalName.ToLower();
		this.previewWindow.Archetype.HairColor = hairColor;
		if (this.playerProfile != null)
		{
			this.playerProfile.HairColor = hairColor;
		}
		this.previewWindow.MakePreview();
		this.previewWindow.ZoomToHead();
		this.HasChanges = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void EyeColors_OnValueChanged(XUiController _sender, XUiC_CustomCharacterWindowGroup.NameData _oldValue, XUiC_CustomCharacterWindowGroup.NameData _newValue)
	{
		string text = _newValue.InternalName.ToLower();
		this.previewWindow.Archetype.EyeColorName = text;
		if (this.playerProfile != null)
		{
			this.playerProfile.EyeColor = text;
		}
		this.previewWindow.MakePreview();
		this.previewWindow.ZoomToEye();
		this.HasChanges = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Mustaches_OnValueChanged(XUiController _sender, XUiC_CustomCharacterWindowGroup.NameData _oldValue, XUiC_CustomCharacterWindowGroup.NameData _newValue)
	{
		string mustacheName = _newValue.InternalName.ToLower();
		this.previewWindow.Archetype.MustacheName = mustacheName;
		if (this.playerProfile != null)
		{
			this.playerProfile.MustacheName = mustacheName;
		}
		this.previewWindow.MakePreview();
		this.previewWindow.ZoomToHead();
		this.HasChanges = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Chops_OnValueChanged(XUiController _sender, XUiC_CustomCharacterWindowGroup.NameData _oldValue, XUiC_CustomCharacterWindowGroup.NameData _newValue)
	{
		string chopsName = _newValue.InternalName.ToLower();
		this.previewWindow.Archetype.ChopsName = chopsName;
		if (this.playerProfile != null)
		{
			this.playerProfile.ChopsName = chopsName;
		}
		this.previewWindow.MakePreview();
		this.previewWindow.ZoomToHead();
		this.HasChanges = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Beards_OnValueChanged(XUiController _sender, XUiC_CustomCharacterWindowGroup.NameData _oldValue, XUiC_CustomCharacterWindowGroup.NameData _newValue)
	{
		string beardName = _newValue.InternalName.ToLower();
		this.previewWindow.Archetype.BeardName = beardName;
		if (this.playerProfile != null)
		{
			this.playerProfile.BeardName = beardName;
		}
		this.previewWindow.MakePreview();
		this.previewWindow.ZoomToHead();
		this.HasChanges = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetSelectedRace(string raceName, bool applyToPreview = false)
	{
		if (raceName == "")
		{
			this.races.SelectedIndex = 0;
		}
		else
		{
			int num = -1;
			for (int i = 0; i < this.races.Elements.Count; i++)
			{
				if (this.races.Elements[i].InternalName.EqualsCaseInsensitive(raceName))
				{
					this.races.SelectedIndex = i;
					return;
				}
			}
			if (num == -1)
			{
				this.races.SelectedIndex = 0;
			}
		}
		if (applyToPreview)
		{
			this.previewWindow.Archetype.Race = this.races.Value.InternalName;
			if (this.playerProfile != null)
			{
				this.playerProfile.RaceName = this.previewWindow.Archetype.Race;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetSelectedVariant(int variant, bool applyToPreview = false)
	{
		if (variant == -1)
		{
			this.variants.SelectedIndex = 0;
		}
		else
		{
			int num = -1;
			for (int i = 0; i < this.variants.Elements.Count; i++)
			{
				if (StringParsers.ParseSInt32(this.variants.Elements[i].InternalName, 0, -1, NumberStyles.Integer) == variant)
				{
					this.variants.SelectedIndex = i;
					return;
				}
			}
			if (num == -1)
			{
				this.variants.SelectedIndex = 0;
			}
		}
		if (applyToPreview)
		{
			this.previewWindow.Archetype.Variant = StringParsers.ParseSInt32(this.variants.Value.InternalName, 0, -1, NumberStyles.Integer);
			if (this.playerProfile != null)
			{
				this.playerProfile.VariantNumber = this.previewWindow.Archetype.Variant;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetSelectedEyeColor(string eyeColorName, bool applyToPreview = false)
	{
		if (eyeColorName == "")
		{
			this.eyeColors.SelectedIndex = 0;
		}
		else
		{
			int num = -1;
			for (int i = 0; i < this.eyeColors.Elements.Count; i++)
			{
				if (this.eyeColors.Elements[i].InternalName.EqualsCaseInsensitive(eyeColorName))
				{
					this.eyeColors.SelectedIndex = i;
					return;
				}
			}
			if (num == -1)
			{
				this.eyeColors.SelectedIndex = 0;
			}
		}
		if (applyToPreview)
		{
			this.previewWindow.Archetype.EyeColorName = this.eyeColors.Value.InternalName;
			if (this.playerProfile != null)
			{
				this.playerProfile.EyeColor = this.previewWindow.Archetype.EyeColorName;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetSelectedHair(string hairName, bool applyToPreview = false)
	{
		if (hairName == "")
		{
			this.hairs.SelectedIndex = 0;
		}
		else
		{
			int num = -1;
			for (int i = 0; i < this.hairs.Elements.Count; i++)
			{
				if (this.hairs.Elements[i].InternalName.EqualsCaseInsensitive(hairName))
				{
					this.hairs.SelectedIndex = i;
					return;
				}
			}
			if (num == -1)
			{
				this.hairs.SelectedIndex = 0;
			}
		}
		if (applyToPreview)
		{
			this.previewWindow.Archetype.Hair = this.hairs.Value.InternalName;
			if (this.playerProfile != null)
			{
				this.playerProfile.HairName = this.previewWindow.Archetype.Hair;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetSelectedHairColor(string hairColorName, bool applyToPreview = false)
	{
		if (hairColorName == "")
		{
			this.hairColors.SelectedIndex = 0;
		}
		else
		{
			int num = -1;
			for (int i = 0; i < this.hairColors.Elements.Count; i++)
			{
				if (this.hairColors.Elements[i].InternalName.EqualsCaseInsensitive(hairColorName))
				{
					this.hairColors.SelectedIndex = i;
					return;
				}
			}
			if (num == -1)
			{
				this.hairColors.SelectedIndex = 0;
			}
		}
		if (applyToPreview)
		{
			this.previewWindow.Archetype.HairColor = this.hairColors.Value.InternalName;
			if (this.playerProfile != null)
			{
				this.playerProfile.HairColor = this.previewWindow.Archetype.HairColor;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetSelectedMustache(string mustacheName, bool applyToPreview = false)
	{
		if (mustacheName == "")
		{
			this.mustaches.SelectedIndex = 0;
		}
		else
		{
			int num = -1;
			for (int i = 0; i < this.mustaches.Elements.Count; i++)
			{
				if (this.mustaches.Elements[i].InternalName.EqualsCaseInsensitive(mustacheName))
				{
					this.mustaches.SelectedIndex = i;
					return;
				}
			}
			if (num == -1)
			{
				this.mustaches.SelectedIndex = 0;
			}
		}
		if (applyToPreview)
		{
			this.previewWindow.Archetype.MustacheName = this.mustaches.Value.InternalName;
			if (this.playerProfile != null)
			{
				this.playerProfile.MustacheName = this.previewWindow.Archetype.MustacheName;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetSelectedChops(string chopsName, bool applyToPreview = false)
	{
		if (chopsName == "")
		{
			this.chops.SelectedIndex = 0;
		}
		else
		{
			int num = -1;
			for (int i = 0; i < this.chops.Elements.Count; i++)
			{
				if (this.chops.Elements[i].InternalName.EqualsCaseInsensitive(chopsName))
				{
					this.chops.SelectedIndex = i;
					return;
				}
			}
			if (num == -1)
			{
				this.chops.SelectedIndex = 0;
			}
		}
		if (applyToPreview)
		{
			this.previewWindow.Archetype.ChopsName = this.chops.Value.InternalName;
			if (this.playerProfile != null)
			{
				this.playerProfile.ChopsName = this.previewWindow.Archetype.ChopsName;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetSelectedBeard(string beardName, bool applyToPreview = false)
	{
		if (beardName == "")
		{
			this.beards.SelectedIndex = 0;
		}
		else
		{
			int num = -1;
			for (int i = 0; i < this.beards.Elements.Count; i++)
			{
				if (this.beards.Elements[i].InternalName.EqualsCaseInsensitive(beardName))
				{
					this.beards.SelectedIndex = i;
					return;
				}
			}
			if (num == -1)
			{
				this.beards.SelectedIndex = 0;
			}
		}
		if (applyToPreview)
		{
			this.previewWindow.Archetype.BeardName = this.beards.Value.InternalName;
			if (this.playerProfile != null)
			{
				this.playerProfile.BeardName = this.previewWindow.Archetype.BeardName;
			}
		}
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (this.HasChanges && PlatformManager.NativePlatform.Input.CurrentInputStyle != PlayerInputManager.InputStyle.Keyboard && base.xui.playerUI.playerInput.GUIActions.Apply.WasPressed)
		{
			this.BtnApply_OnPress(null, 0);
		}
	}

	public override bool GetBindingValue(ref string value, string bindingName)
	{
		if (bindingName == "isMale")
		{
			value = (this.genders != null && this.genders.SelectedIndex == 1).ToString();
			return true;
		}
		return false;
	}

	public static string ID = "";

	public bool IsMale = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool HasChanges;

	public PlayerProfile playerProfile;

	public Archetype archetype;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<XUiC_CustomCharacterWindowGroup.NameData> races;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<XUiC_CustomCharacterWindowGroup.NameData> genders;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<XUiC_CustomCharacterWindowGroup.NameData> eyeColors;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<XUiC_CustomCharacterWindowGroup.NameData> hairs;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<XUiC_CustomCharacterWindowGroup.NameData> hairColors;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<XUiC_CustomCharacterWindowGroup.NameData> mustaches;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<XUiC_CustomCharacterWindowGroup.NameData> chops;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<XUiC_CustomCharacterWindowGroup.NameData> beards;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<XUiC_CustomCharacterWindowGroup.NameData> variants;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SDCSPreviewWindow previewWindow;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnBack;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnApply;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton btnRandomize;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool lockedRace = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool lockedGenders = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool lockedHairs;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool lockedHairColors;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool lockedVariants;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool lockedEyeColors;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool lockedMustaches;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool lockedChops;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool lockedBeards;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Button btnLockRace;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Button btnLockGender;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Button btnLockFace;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Button btnLockEyeColor;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Button btnLockHairStyle;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Button btnLockHairColor;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Button btnLockMustaches;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Button btnLockChops;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Button btnLockBeards;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_CustomCharacterWindow CustomCharacterWindow;

	[PublicizedFrom(EAccessModifier.Private)]
	public GameRandom gr;

	public enum Gender
	{
		Male,
		Female
	}

	public enum Race
	{
		White,
		Black,
		Asian,
		Hispanic
	}

	public struct NameData
	{
		public NameData(string _internalName, string _formattedName)
		{
			this.InternalName = _internalName;
			this.FormattedName = _formattedName;
		}

		public override string ToString()
		{
			return this.FormattedName;
		}

		public string InternalName;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly string FormattedName;
	}
}
