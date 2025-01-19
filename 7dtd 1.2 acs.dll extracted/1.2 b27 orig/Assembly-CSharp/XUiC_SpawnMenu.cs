using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_SpawnMenu : XUiController
{
	public override void Init()
	{
		base.Init();
		XUiC_SpawnMenu.ID = base.WindowGroup.ID;
		this.toggleLookAtYou = base.GetChildById("toggleLookAtYou").GetChildByType<XUiC_ToggleButton>();
		this.toggleLookAtYou.OnValueChanged += this.ToggleLookAtYou_OnValueChanged;
		this.toggleSpawn25 = base.GetChildById("toggleSpawn25").GetChildByType<XUiC_ToggleButton>();
		this.toggleSpawn25.OnValueChanged += this.ToggleSpawn25_OnValueChanged;
		this.toggleFromDynamic = base.GetChildById("toggleFromDynamic").GetChildByType<XUiC_ToggleButton>();
		this.toggleFromDynamic.OnValueChanged += this.ToggleFromDynamic_OnValueChanged;
		this.toggleFromStatic = base.GetChildById("toggleFromStatic").GetChildByType<XUiC_ToggleButton>();
		this.toggleFromStatic.OnValueChanged += this.ToggleFromStatic_OnValueChanged;
		this.toggleFromBiome = base.GetChildById("toggleFromBiome").GetChildByType<XUiC_ToggleButton>();
		this.toggleFromBiome.OnValueChanged += this.ToggleFromBiome_OnValueChanged;
		this.entitiesList = (XUiC_SpawnEntitiesList)base.GetChildById("entities");
		this.entitiesList.SelectionChanged += this.EntitiesList_SelectionChanged;
		this.toggleFromDynamic.Value = true;
	}

	public override void OnOpen()
	{
		base.OnOpen();
		XUiC_FocusedBlockHealth.SetData(base.xui.playerUI, null, 0f);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void EntitiesList_SelectionChanged(XUiC_ListEntry<XUiC_SpawnEntitiesList.SpawnEntityEntry> _previousEntry, XUiC_ListEntry<XUiC_SpawnEntitiesList.SpawnEntityEntry> _newEntry)
	{
		if (_newEntry != null)
		{
			this.entitiesList.ClearSelection();
			if (_newEntry.GetEntry() != null)
			{
				XUiC_SpawnEntitiesList.SpawnEntityEntry entry = _newEntry.GetEntry();
				this.BtnSpawns_OnPress(entry.key);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ToggleLookAtYou_OnValueChanged(XUiC_ToggleButton _sender, bool _newValue)
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ToggleSpawn25_OnValueChanged(XUiC_ToggleButton _sender, bool _newValue)
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ToggleFromDynamic_OnValueChanged(XUiC_ToggleButton _sender, bool _newValue)
	{
		if (_newValue)
		{
			this.toggleFromStatic.Value = false;
			this.toggleFromBiome.Value = false;
			return;
		}
		this.toggleFromDynamic.Value = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ToggleFromStatic_OnValueChanged(XUiC_ToggleButton _sender, bool _newValue)
	{
		if (_newValue)
		{
			this.toggleFromDynamic.Value = false;
			this.toggleFromBiome.Value = false;
			return;
		}
		this.toggleFromStatic.Value = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ToggleFromBiome_OnValueChanged(XUiC_ToggleButton _sender, bool _newValue)
	{
		if (_newValue)
		{
			this.toggleFromDynamic.Value = false;
			this.toggleFromStatic.Value = false;
			return;
		}
		this.toggleFromBiome.Value = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnSpawns_OnPress(int _key)
	{
		EntityPlayerLocal primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
		Camera finalCamera = primaryPlayer.finalCamera;
		float offsetUp = (float)(primaryPlayer.AttachedToEntity ? 2 : 0);
		Vector3 vector = XUiC_LevelTools3Window.getRaycastHitPoint(100f, offsetUp);
		if (vector.Equals(Vector3.zero))
		{
			Ray ray = finalCamera.ScreenPointToRay(new Vector3((float)Screen.width * 0.5f, (float)Screen.height * 0.5f, 0f));
			vector = ray.origin + ray.direction * 10f + Origin.position;
		}
		vector.y += 0.25f;
		Vector3 vector2 = new Vector3(0f, this.toggleLookAtYou.Value ? (finalCamera.transform.eulerAngles.y + 180f) : finalCamera.transform.eulerAngles.y, 0f);
		EnumSpawnerSource enumSpawnerSource = EnumSpawnerSource.Unknown;
		if (this.toggleFromDynamic.Value)
		{
			enumSpawnerSource = EnumSpawnerSource.Dynamic;
		}
		if (this.toggleFromStatic.Value)
		{
			enumSpawnerSource = EnumSpawnerSource.StaticSpawner;
		}
		if (this.toggleFromBiome.Value)
		{
			enumSpawnerSource = EnumSpawnerSource.Biome;
		}
		int num = this.toggleSpawn25.Value ? 25 : 1;
		if (InputUtils.ShiftKeyPressed)
		{
			num = 5;
		}
		Vector3 vector3 = finalCamera.transform.right;
		if (!InputUtils.AltKeyPressed)
		{
			vector3 *= 0.01f;
		}
		if (EntityClass.list[_key].entityClassName == "entityJunkDrone")
		{
			if (!EntityDrone.IsValidForLocalPlayer())
			{
				return;
			}
			GameManager.Instance.World.EntityLoadedDelegates += EntityDrone.OnClientSpawnRemote;
			num = 1;
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			vector -= vector3 * ((float)(num - 1) * 0.5f);
			for (int i = 0; i < num; i++)
			{
				Entity entity = EntityFactory.CreateEntity(_key, vector, vector2);
				entity.SetSpawnerSource(enumSpawnerSource);
				this.setUpEntity(entity);
				GameManager.Instance.World.SpawnEntityInWorld(entity);
				vector += vector3;
			}
			return;
		}
		SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageConsoleCmdServer>().Setup(string.Concat(new string[]
		{
			"spawnentityat \"",
			EntityClass.list[_key].entityClassName,
			"\" ",
			vector.x.ToCultureInvariantString(),
			" ",
			vector.y.ToCultureInvariantString(),
			" ",
			vector.z.ToCultureInvariantString(),
			" ",
			num.ToString(),
			" ",
			vector2.x.ToCultureInvariantString(),
			" ",
			vector2.y.ToCultureInvariantString(),
			" ",
			vector2.z.ToCultureInvariantString(),
			" ",
			vector3.x.ToCultureInvariantString(),
			" ",
			vector3.y.ToCultureInvariantString(),
			" ",
			vector3.z.ToCultureInvariantString(),
			" ",
			enumSpawnerSource.ToStringCached<EnumSpawnerSource>()
		})), false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setUpEntity(Entity _entity)
	{
	}

	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SpawnEntitiesList entitiesList;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ToggleButton toggleLookAtYou;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ToggleButton toggleSpawn25;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ToggleButton toggleFromDynamic;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ToggleButton toggleFromStatic;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ToggleButton toggleFromBiome;
}
