using System;
using Audio;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_PowerSourceStats : XUiController
{
	public PowerSource PowerSource
	{
		get
		{
			return this.powerSource;
		}
		set
		{
			this.powerSource = value;
			base.RefreshBindings(false);
		}
	}

	public TileEntityPowerSource TileEntity
	{
		get
		{
			return this.tileEntity;
		}
		set
		{
			this.tileEntity = value;
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				this.PowerSource = (this.tileEntity.GetPowerItem() as PowerSource);
			}
		}
	}

	public XUiC_PowerSourceWindowGroup Owner { get; [PublicizedFrom(EAccessModifier.Internal)] set; }

	public override void Init()
	{
		base.Init();
		this.sprFuelFill = (XUiV_Sprite)base.GetChildById("sprFuelFill").ViewComponent;
		this.sprFillPotential = (XUiV_Sprite)base.GetChildById("sprFillPotential").ViewComponent;
		this.sprFillPotential.Fill = 0f;
		this.windowIcon = base.GetChildById("windowIcon");
		this.btnRefuel = base.GetChildById("btnRefuel");
		this.btnRefuel_Background = (XUiV_Button)this.btnRefuel.GetChildById("clickable").ViewComponent;
		this.btnRefuel_Background.Controller.OnPress += this.BtnRefuel_OnPress;
		this.btnRefuel_Background.Controller.OnHover += this.btnRefuel_OnHover;
		this.btnOn = base.GetChildById("btnOn");
		this.btnOn_Background = (XUiV_Button)this.btnOn.GetChildById("clickable").ViewComponent;
		this.btnOn_Background.Controller.OnPress += this.btnOn_OnPress;
		XUiController childById = base.GetChildById("lblOnOff");
		if (childById != null)
		{
			this.lblOnOff = (XUiV_Label)childById.ViewComponent;
		}
		childById = base.GetChildById("sprOnOff");
		if (childById != null)
		{
			this.sprOnOff = (XUiV_Sprite)childById.ViewComponent;
		}
		this.isDirty = true;
		this.turnOff = Localization.Get("xuiTurnOff", false);
		this.turnOn = Localization.Get("xuiTurnOn", false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void btnRefuel_OnHover(XUiController _sender, bool _isOver)
	{
		this.RefuelButtonHovered = _isOver;
		base.RefreshBindings(false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnRefuel_OnPress(XUiController _sender, int _mouseButton)
	{
		float num = (float)this.TileEntity.MaxFuel;
		float num2 = (float)this.TileEntity.CurrentFuel;
		if (num2 >= num)
		{
			return;
		}
		float num3 = Mathf.Min(250f, num - num2);
		EntityPlayer entityPlayer = base.xui.playerUI.entityPlayer;
		ItemValue itemValue = new ItemValue(ItemClass.GetItemWithTag(XUiC_PowerSourceStats.tag).Id, false);
		int num4 = entityPlayer.inventory.DecItem(itemValue, (int)num3, false, null);
		if (num4 == 0)
		{
			num4 = entityPlayer.bag.DecItem(itemValue, (int)num3, false, null);
		}
		if (num4 == 0)
		{
			Manager.PlayInsidePlayerHead("ui_denied", -1, 0f, false, false);
			GameManager.ShowTooltip(base.xui.playerUI.localPlayer.entityPlayerLocal, Localization.Get("ttNotEnoughFuel", false), false);
			return;
		}
		ItemStack @is = new ItemStack(itemValue, num4);
		base.xui.CollectedItemList.RemoveItemStack(@is);
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			PowerGenerator powerGenerator = this.TileEntity.GetPowerItem() as PowerGenerator;
			PowerGenerator powerGenerator2 = powerGenerator;
			powerGenerator2.CurrentFuel += (ushort)num4;
			if (powerGenerator.CurrentFuel > powerGenerator.MaxFuel)
			{
				powerGenerator.CurrentFuel = powerGenerator.MaxFuel;
			}
			powerGenerator.CurrentPower = powerGenerator.MaxPower;
		}
		else
		{
			this.tileEntity.ClientData.AddedFuel = (ushort)num4;
			this.tileEntity.SetModified();
		}
		entityPlayer.PlayOneShot("useactions/gas_refill", false, false, false);
		base.RefreshBindings(false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void btnOn_OnPress(XUiController _sender, int _mouseButton)
	{
		BlockValue block = this.TileEntity.GetChunk().GetBlock(this.TileEntity.localChunkPos);
		if (this.TileEntity.MaxOutput == 0)
		{
			Manager.PlayInsidePlayerHead("ui_denied", -1, 0f, false, false);
			GameManager.ShowTooltip(base.xui.playerUI.localPlayer.entityPlayerLocal, Localization.Get("ttRequiresOneComponent", false), false);
			return;
		}
		bool flag = (block.meta & 2) > 0;
		if (this.TileEntity.PowerItemType == PowerItem.PowerItemTypes.Generator && !flag && this.TileEntity.CurrentFuel == 0)
		{
			Manager.PlayInsidePlayerHead("ui_denied", -1, 0f, false, false);
			GameManager.ShowTooltip(base.xui.playerUI.localPlayer.entityPlayerLocal, Localization.Get("ttGeneratorRequiresFuel", false), false);
			return;
		}
		if (!flag)
		{
			bool flag2 = false;
			EntityPlayerLocal entityPlayer = _sender.xui.playerUI.entityPlayer;
			World world = entityPlayer.world;
			Vector3i vector3i = this.TileEntity.ToWorldPos();
			if (flag2 | world.IsWater(vector3i.x, vector3i.y + 1, vector3i.z) | world.IsWater(vector3i.x + 1, vector3i.y, vector3i.z) | world.IsWater(vector3i.x - 1, vector3i.y, vector3i.z) | world.IsWater(vector3i.x, vector3i.y, vector3i.z + 1) | world.IsWater(vector3i.x, vector3i.y, vector3i.z - 1))
			{
				Manager.PlayInsidePlayerHead("ui_denied", -1, 0f, false, false);
				GameManager.ShowTooltip(entityPlayer, Localization.Get("ttPowerSourceUnderwater", false), false);
				return;
			}
		}
		flag = !flag;
		block.meta = (byte)(((int)block.meta & -3) | (flag ? 2 : 0));
		GameManager.Instance.World.SetBlockRPC(this.TileEntity.GetClrIdx(), this.TileEntity.ToWorldPos(), block);
		this.RefreshIsOn(flag);
		this.Owner.SetOn(flag);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RefreshIsOn(bool isOn)
	{
		if (isOn)
		{
			this.lblOnOff.Text = this.turnOff;
			if (this.sprOnOff != null)
			{
				this.sprOnOff.Color = this.onColor;
				return;
			}
		}
		else
		{
			this.lblOnOff.Text = this.turnOn;
			if (this.sprOnOff != null)
			{
				this.sprOnOff.Color = this.offColor;
			}
		}
	}

	public override bool GetBindingValue(ref string value, string bindingName)
	{
		uint num = <PrivateImplementationDetails>.ComputeStringHash(bindingName);
		if (num <= 1246567232U)
		{
			if (num <= 804398480U)
			{
				if (num != 90731405U)
				{
					if (num != 303743984U)
					{
						if (num == 804398480U)
						{
							if (bindingName == "potentialfuelfill")
							{
								if (!this.RefuelButtonHovered)
								{
									value = "0";
								}
								else if (this.tileEntity == null || this.tileEntity.PowerItemType != PowerItem.PowerItemTypes.Generator)
								{
									value = "0";
								}
								else
								{
									value = this.potentialFuelFillFormatter.Format((float)(this.tileEntity.CurrentFuel + 250) / (float)this.tileEntity.MaxFuel);
								}
								return true;
							}
						}
					}
					else if (bindingName == "powertitle")
					{
						value = Localization.Get("xuiPower", false);
						return true;
					}
				}
				else if (bindingName == "fuel")
				{
					if (this.tileEntity == null || this.tileEntity.PowerItemType != PowerItem.PowerItemTypes.Generator)
					{
						value = "";
					}
					else
					{
						value = this.fuelFormatter.Format(this.tileEntity.CurrentFuel);
					}
					return true;
				}
			}
			else if (num != 818200417U)
			{
				if (num != 912419069U)
				{
					if (num == 1246567232U)
					{
						if (bindingName == "fuelfill")
						{
							if (this.tileEntity == null || this.tileEntity.PowerItemType != PowerItem.PowerItemTypes.Generator)
							{
								value = "0";
							}
							else
							{
								value = this.fuelFillFormatter.Format((float)this.tileEntity.CurrentFuel / (float)this.tileEntity.MaxFuel);
							}
							return true;
						}
					}
				}
				else if (bindingName == "showsolar")
				{
					value = ((this.tileEntity == null) ? "false" : (this.tileEntity.PowerItemType == PowerItem.PowerItemTypes.SolarPanel).ToString());
					return true;
				}
			}
			else if (bindingName == "maxfuel")
			{
				if (this.tileEntity == null || this.tileEntity.PowerItemType != PowerItem.PowerItemTypes.Generator)
				{
					value = "";
				}
				else
				{
					value = this.maxfuelFormatter.Format(this.tileEntity.MaxFuel);
				}
				return true;
			}
		}
		else if (num <= 3294842160U)
		{
			if (num != 2034531851U)
			{
				if (num != 2349985850U)
				{
					if (num == 3294842160U)
					{
						if (bindingName == "maxoutput")
						{
							value = ((this.tileEntity == null) ? "" : this.maxoutputFormatter.Format(this.tileEntity.MaxOutput));
							return true;
						}
					}
				}
				else if (bindingName == "powersourceicon")
				{
					if (this.tileEntity == null)
					{
						value = "";
					}
					else
					{
						switch (this.tileEntity.PowerItemType)
						{
						case PowerItem.PowerItemTypes.Generator:
							value = "ui_game_symbol_electric_generator";
							break;
						case PowerItem.PowerItemTypes.SolarPanel:
							value = "ui_game_symbol_electric_solar";
							break;
						case PowerItem.PowerItemTypes.BatteryBank:
							value = "ui_game_symbol_battery";
							break;
						}
					}
					return true;
				}
			}
			else if (bindingName == "powerfill")
			{
				value = ((this.tileEntity == null) ? "0" : this.powerFillFormatter.Format((float)this.tileEntity.LastOutput / (float)this.tileEntity.MaxOutput));
				return true;
			}
		}
		else if (num <= 3776886369U)
		{
			if (num != 3392664512U)
			{
				if (num == 3776886369U)
				{
					if (bindingName == "fueltitle")
					{
						value = Localization.Get("xuiGas", false);
						return true;
					}
				}
			}
			else if (bindingName == "showfuel")
			{
				value = ((this.tileEntity == null) ? "false" : (this.tileEntity.PowerItemType == PowerItem.PowerItemTypes.Generator).ToString());
				return true;
			}
		}
		else if (num != 3809830086U)
		{
			if (num == 4115604294U)
			{
				if (bindingName == "power")
				{
					value = ((this.tileEntity == null) ? "" : this.powerFormatter.Format(this.tileEntity.LastOutput));
					return true;
				}
			}
		}
		else if (bindingName == "maxoutputtitle")
		{
			value = Localization.Get("xuiMaxOutput", false);
			return true;
		}
		return false;
	}

	public override void Update(float _dt)
	{
		if (GameManager.Instance == null && GameManager.Instance.World == null)
		{
			return;
		}
		if (this.tileEntity == null)
		{
			return;
		}
		base.Update(_dt);
		if (this.lastOn != this.tileEntity.IsOn)
		{
			this.lastOn = this.tileEntity.IsOn;
			this.Owner.SetOn(this.tileEntity.IsOn);
			this.RefreshIsOn(this.tileEntity.IsOn);
		}
		base.RefreshBindings(false);
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.tileEntity.SetUserAccessing(true);
		bool isOn = this.tileEntity.IsOn;
		this.RefreshIsOn(isOn);
		this.Owner.SetOn(isOn);
		base.RefreshBindings(false);
		this.tileEntity.SetModified();
	}

	public override void OnClose()
	{
		GameManager instance = GameManager.Instance;
		Vector3i blockPos = this.tileEntity.ToWorldPos();
		if (!XUiC_CameraWindow.hackyIsOpeningMaximizedWindow)
		{
			this.tileEntity.SetUserAccessing(false);
			instance.TEUnlockServer(this.tileEntity.GetClrIdx(), blockPos, this.tileEntity.entityId, true);
			this.tileEntity.SetModified();
			this.powerSource = null;
		}
		base.OnClose();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool RefuelButtonHovered;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite sprFuelFill;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite sprFillPotential;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController windowIcon;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController btnRefuel;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Button btnRefuel_Background;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController btnOn;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Button btnOn_Background;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label lblOnOff;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite sprOnOff;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color32 onColor = new Color32(250, byte.MaxValue, 163, byte.MaxValue);

	[PublicizedFrom(EAccessModifier.Private)]
	public Color32 offColor = Color.white;

	[PublicizedFrom(EAccessModifier.Private)]
	public string turnOff;

	[PublicizedFrom(EAccessModifier.Private)]
	public string turnOn;

	public static FastTags<TagGroup.Global> tag = FastTags<TagGroup.Global>.Parse("gasoline");

	[PublicizedFrom(EAccessModifier.Private)]
	public PowerSource powerSource;

	[PublicizedFrom(EAccessModifier.Private)]
	public TileEntityPowerSource tileEntity;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool lastOn;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isDirty;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<ushort> fuelFormatter = new CachedStringFormatter<ushort>((ushort _i) => _i.ToString());

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<ushort> maxfuelFormatter = new CachedStringFormatter<ushort>((ushort _i) => _i.ToString());

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<ushort> maxoutputFormatter = new CachedStringFormatter<ushort>((ushort _i) => _i.ToString());

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<ushort> powerFormatter = new CachedStringFormatter<ushort>((ushort _i) => _i.ToString());

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterFloat potentialFuelFillFormatter = new CachedStringFormatterFloat(null);

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterFloat powerFillFormatter = new CachedStringFormatterFloat(null);

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterFloat fuelFillFormatter = new CachedStringFormatterFloat(null);

	[PublicizedFrom(EAccessModifier.Private)]
	public float updateTime;
}
