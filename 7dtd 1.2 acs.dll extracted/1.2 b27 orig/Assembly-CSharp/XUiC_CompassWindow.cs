using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_CompassWindow : XUiController
{
	public EntityPlayerLocal localPlayer { get; [PublicizedFrom(EAccessModifier.Internal)] set; }

	public override void Init()
	{
		base.Init();
		XUiC_CompassWindow.ID = base.WindowGroup.ID;
		for (int i = 0; i < 50; i++)
		{
			GameObject gameObject = new GameObject();
			gameObject.transform.parent = base.ViewComponent.UiTransform;
			this.waypointSpriteList.Add(gameObject.AddComponent<UISprite>());
			this.waypointSpriteList[this.waypointSpriteList.Count - 1].atlas = base.xui.GetAtlasByName("UIAtlas", "menu_empty");
			this.waypointSpriteList[this.waypointSpriteList.Count - 1].transform.localScale = Vector3.one;
			this.waypointSpriteList[this.waypointSpriteList.Count - 1].spriteName = "menu_empty";
			this.waypointSpriteList[this.waypointSpriteList.Count - 1].SetDimensions(20, 20);
			this.waypointSpriteList[this.waypointSpriteList.Count - 1].color = Color.clear;
			this.waypointSpriteList[this.waypointSpriteList.Count - 1].pivot = UIWidget.Pivot.Center;
			this.waypointSpriteList[this.waypointSpriteList.Count - 1].depth = 12;
			this.waypointSpriteList[this.waypointSpriteList.Count - 1].gameObject.layer = 12;
		}
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (this.localPlayer == null)
		{
			this.localPlayer = base.xui.playerUI.entityPlayer;
		}
		this.showSleeperVolumes = true;
		base.ViewComponent.IsVisible = (!this.localPlayer.IsDead() && base.xui.playerUI.windowManager.IsHUDEnabled());
		if (this.localPlayer != null && this.localPlayer.playerCamera != null)
		{
			int num = 0;
			this.updateNavObjects(this.localPlayer, ref num);
			this.updateMarkers(this.localPlayer, ref num, GameManager.Instance.World.GetObjectOnMapList(EnumMapObjectType.SleepingBag));
			this.updateMarkers(this.localPlayer, ref num, GameManager.Instance.World.GetObjectOnMapList(EnumMapObjectType.LandClaim));
			this.updateMarkers(this.localPlayer, ref num, GameManager.Instance.World.GetObjectOnMapList(EnumMapObjectType.MapMarker));
			this.updateMarkers(this.localPlayer, ref num, GameManager.Instance.World.GetObjectOnMapList(EnumMapObjectType.MapQuickMarker));
			this.updateMarkers(this.localPlayer, ref num, GameManager.Instance.World.GetObjectOnMapList(EnumMapObjectType.Backpack));
			if (this.showSleeperVolumes)
			{
				this.updateMarkers(this.localPlayer, ref num, GameManager.Instance.World.GetObjectOnMapList(EnumMapObjectType.Quest));
			}
			this.updateMarkers(this.localPlayer, ref num, GameManager.Instance.World.GetObjectOnMapList(EnumMapObjectType.TreasureChest));
			this.updateMarkers(this.localPlayer, ref num, GameManager.Instance.World.GetObjectOnMapList(EnumMapObjectType.FetchItem));
			this.updateMarkers(this.localPlayer, ref num, GameManager.Instance.World.GetObjectOnMapList(EnumMapObjectType.HiddenCache));
			this.updateMarkers(this.localPlayer, ref num, GameManager.Instance.World.GetObjectOnMapList(EnumMapObjectType.RestorePower));
			if (this.showSleeperVolumes)
			{
				this.updateMarkers(this.localPlayer, ref num, GameManager.Instance.World.GetObjectOnMapList(EnumMapObjectType.SleeperVolume));
			}
			this.updateMarkers(this.localPlayer, ref num, GameManager.Instance.World.GetObjectOnMapList(EnumMapObjectType.VendingMachine));
			if (GameStats.GetBool(EnumGameStats.AirDropMarker))
			{
				this.updateMarkers(this.localPlayer, ref num, GameManager.Instance.World.GetObjectOnMapList(EnumMapObjectType.SupplyDrop));
			}
			for (int i = num; i < this.waypointSpriteList.Count; i++)
			{
				this.waypointSpriteList[i].color = Color.clear;
			}
		}
		if (GameManager.Instance != null && GameManager.Instance.World != null && XUi.IsGameRunning())
		{
			base.RefreshBindings(false);
		}
	}

	public override bool GetBindingValue(ref string value, string bindingName)
	{
		uint num = <PrivateImplementationDetails>.ComputeStringHash(bindingName);
		if (num <= 1586863888U)
		{
			if (num <= 1021729756U)
			{
				if (num != 85644157U)
				{
					if (num == 1021729756U)
					{
						if (bindingName == "daytime")
						{
							value = "";
							if (XUi.IsGameRunning())
							{
								value = this.daytimeFormatter.Format(GameManager.Instance.World.worldTime);
							}
							return true;
						}
					}
				}
				else if (bindingName == "showtime")
				{
					if (this.localPlayer != null)
					{
						value = (EffectManager.GetValue(PassiveEffects.NoTimeDisplay, null, 0f, this.localPlayer, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false) == 0f).ToString();
					}
					else
					{
						value = "true";
					}
					return true;
				}
			}
			else if (num != 1564253156U)
			{
				if (num == 1586863888U)
				{
					if (bindingName == "daycolor")
					{
						value = "FFFFFF";
						if (XUi.IsGameRunning())
						{
							ulong worldTime = GameManager.Instance.World.worldTime;
							int @int = GameStats.GetInt(EnumGameStats.BloodMoonWarning);
							ValueTuple<int, int, int> valueTuple = GameUtils.WorldTimeToElements(worldTime);
							int item = valueTuple.Item1;
							int item2 = valueTuple.Item2;
							if (@int != -1 && GameStats.GetInt(EnumGameStats.BloodMoonDay) == item && @int <= item2)
							{
								value = "FF0000";
							}
						}
						return true;
					}
				}
			}
			else if (bindingName == "time")
			{
				value = "";
				if (XUi.IsGameRunning())
				{
					ValueTuple<int, int, int> valueTuple2 = GameUtils.WorldTimeToElements(GameManager.Instance.World.worldTime);
					int item3 = valueTuple2.Item2;
					int item4 = valueTuple2.Item3;
					value = this.timeFormatter.Format(item3, item4);
				}
				return true;
			}
		}
		else if (num <= 1978910129U)
		{
			if (num != 1971063990U)
			{
				if (num == 1978910129U)
				{
					if (bindingName == "daytitle")
					{
						value = Localization.Get("xuiDay", false);
						return true;
					}
				}
			}
			else if (bindingName == "compass_rotation")
			{
				if (this.localPlayer != null && this.localPlayer.playerCamera != null)
				{
					value = this.localPlayer.playerCamera.transform.eulerAngles.y.ToString();
				}
				else
				{
					value = "0.0";
				}
				return true;
			}
		}
		else if (num != 2235205906U)
		{
			if (num != 2899617242U)
			{
				if (num == 3830391293U)
				{
					if (bindingName == "day")
					{
						value = "0";
						if (XUi.IsGameRunning())
						{
							int v = GameUtils.WorldTimeToDays(GameManager.Instance.World.worldTime);
							value = this.dayFormatter.Format(v);
						}
						return true;
					}
				}
			}
			else if (bindingName == "compass_language")
			{
				if (GamePrefs.GetBool(EnumGamePrefs.OptionsUiCompassUseEnglishCardinalDirections))
				{
					value = Localization.DefaultLanguage;
				}
				else
				{
					value = Localization.language;
				}
				return true;
			}
		}
		else if (bindingName == "timetitle")
		{
			value = Localization.Get("xuiTime", false);
			return true;
		}
		return base.GetBindingValue(ref value, bindingName);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateMarkers(EntityPlayer localPlayer, ref int waypointSpriteIndex, List<Waypoint> waypoints)
	{
		int num = 256;
		Entity entity = (localPlayer.AttachedToEntity != null) ? localPlayer.AttachedToEntity : localPlayer;
		Vector2 b = new Vector2(entity.GetPosition().x, entity.GetPosition().z);
		Vector2 rhs = new Vector2(entity.transform.forward.x, entity.transform.forward.z);
		rhs.Normalize();
		Vector2 rhs2 = new Vector2(entity.transform.right.x, entity.transform.right.z);
		rhs2.Normalize();
		float a = 0.25f;
		float b2 = 1f;
		float num2 = (float)base.ViewComponent.Size.x * 0.5f;
		float num3 = num2 * 1.1f;
		for (int i = 0; i < waypoints.Count; i++)
		{
			if (i >= waypoints.Count)
			{
				this.waypointSpriteList[i].color = Color.clear;
				return;
			}
			if (i >= this.waypointSpriteList.Count)
			{
				break;
			}
			Vector2 vector = new Vector2((float)waypoints[i].pos.x, (float)waypoints[i].pos.z) - b;
			float magnitude = vector.magnitude;
			if (magnitude > (float)num)
			{
				this.waypointSpriteList[i].color = Color.clear;
			}
			else
			{
				Vector2 normalized = vector.normalized;
				if (Vector2.Dot(normalized, rhs) < 0.75f)
				{
					this.waypointSpriteList[i].color = Color.clear;
				}
				else
				{
					float num4 = Mathf.Lerp(a, b2, 1f - magnitude / (float)num);
					this.waypointSpriteList[i].atlas = base.xui.GetAtlasByName("UIAtlas", waypoints[i].icon);
					this.waypointSpriteList[i].spriteName = waypoints[i].icon;
					if (waypoints[i].bTracked)
					{
						this.waypointSpriteList[i].color = new Color(1f, 1f, 1f, num4);
					}
					else
					{
						this.waypointSpriteList[i].color = new Color(0.5f, 0.5f, 0.5f, num4);
					}
					this.waypointSpriteList[i].SetDimensions((int)(25f * num4), (int)(25f * num4));
					this.waypointSpriteList[i].transform.localPosition = new Vector3(num2 + Vector2.Dot(normalized, rhs2) * num3, -16f);
					waypointSpriteIndex++;
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateMarkers(EntityPlayerLocal localPlayer, ref int waypointSpriteIndex, List<MapObject> _mapObjectList)
	{
		float num = (float)base.ViewComponent.Size.x * 0.5f;
		float num2 = num * 1.15f;
		Transform transform = localPlayer.playerCamera.transform;
		Entity entity = (localPlayer.AttachedToEntity != null) ? localPlayer.AttachedToEntity : localPlayer;
		Vector2 b = new Vector2(entity.GetPosition().x, entity.GetPosition().z);
		Vector2 rhs = new Vector2(transform.forward.x, transform.forward.z);
		rhs.Normalize();
		Vector2 rhs2 = new Vector2(transform.right.x, transform.right.z);
		rhs2.Normalize();
		for (int i = 0; i < _mapObjectList.Count; i++)
		{
			MapObject mapObject = _mapObjectList[i];
			mapObject.RefreshData();
			if (waypointSpriteIndex >= this.waypointSpriteList.Count)
			{
				break;
			}
			if (mapObject.IsOnCompass())
			{
				if (mapObject is MapObjectZombie)
				{
					this.showSleeperVolumes = false;
				}
				Vector2 vector = new Vector2(mapObject.GetPosition().x, mapObject.GetPosition().z) - b;
				float magnitude = vector.magnitude;
				bool flag = true;
				if (_mapObjectList[i].type == EnumMapObjectType.TreasureChest)
				{
					float num3 = (float)(_mapObjectList[i] as MapObjectTreasureChest).DefaultRadius;
					float num4 = EffectManager.GetValue(PassiveEffects.TreasureRadius, null, num3, localPlayer, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
					num4 = Mathf.Clamp(num4, 0f, num3);
					if (magnitude < num4)
					{
						float num5 = Mathf.PingPong(Time.time, 0.25f);
						float num6 = 1.25f + num5;
						this.waypointSpriteList[waypointSpriteIndex].atlas = base.xui.GetAtlasByName("UIAtlas", mapObject.GetMapIcon());
						this.waypointSpriteList[waypointSpriteIndex].spriteName = mapObject.GetMapIcon();
						this.waypointSpriteList[waypointSpriteIndex].SetDimensions((int)(25f * num6), (int)(25f * num6));
						this.waypointSpriteList[waypointSpriteIndex].transform.localPosition = new Vector3(num, -24f);
						Color mapIconColor = mapObject.GetMapIconColor();
						this.waypointSpriteList[waypointSpriteIndex].color = Color.Lerp(mapIconColor, Color.red, num5 * 4f);
						waypointSpriteIndex++;
						flag = false;
					}
				}
				string spriteName = mapObject.GetCompassIcon();
				if (_mapObjectList[i].type == EnumMapObjectType.HiddenCache)
				{
					this.waypointSpriteList[waypointSpriteIndex].flip = UIBasicSprite.Flip.Nothing;
					if (mapObject.GetPosition().y < localPlayer.GetPosition().y - 2f)
					{
						spriteName = _mapObjectList[i].GetCompassDownIcon();
					}
					else if (mapObject.GetPosition().y > localPlayer.GetPosition().y + 2f)
					{
						spriteName = _mapObjectList[i].GetCompassUpIcon();
					}
					this.waypointSpriteList[waypointSpriteIndex].depth = 100;
					this.waypointSpriteList[waypointSpriteIndex].atlas = base.xui.GetAtlasByName("UIAtlas", spriteName);
					this.waypointSpriteList[waypointSpriteIndex].spriteName = spriteName;
					if ((mapObject.GetPosition() - entity.GetPosition()).magnitude < 10f)
					{
						float num7 = Mathf.PingPong(Time.time, 0.25f);
						float num8 = 1.25f + num7;
						this.waypointSpriteList[waypointSpriteIndex].SetDimensions((int)(25f * num8), (int)(25f * num8));
						this.waypointSpriteList[waypointSpriteIndex].transform.localPosition = new Vector3(num, -24f);
						Color mapIconColor2 = mapObject.GetMapIconColor();
						this.waypointSpriteList[waypointSpriteIndex].color = Color.Lerp(mapIconColor2, Color.grey, num7 * 4f);
						waypointSpriteIndex++;
						flag = false;
					}
				}
				if (_mapObjectList[i].UseUpDownCompassIcons())
				{
					this.waypointSpriteList[waypointSpriteIndex].flip = UIBasicSprite.Flip.Nothing;
					if (mapObject.GetPosition().y < localPlayer.GetPosition().y - 2f)
					{
						spriteName = _mapObjectList[i].GetCompassDownIcon();
					}
					else if (mapObject.GetPosition().y > localPlayer.GetPosition().y + 3f)
					{
						spriteName = _mapObjectList[i].GetCompassUpIcon();
					}
					this.waypointSpriteList[waypointSpriteIndex].depth = 100;
					this.waypointSpriteList[waypointSpriteIndex].atlas = base.xui.GetAtlasByName("UIAtlas", spriteName);
					this.waypointSpriteList[waypointSpriteIndex].spriteName = spriteName;
				}
				if (flag)
				{
					Vector2 normalized = vector.normalized;
					if (!mapObject.IsCompassIconClamped() && Vector2.Dot(normalized, rhs) < 0.75f)
					{
						this.waypointSpriteList[waypointSpriteIndex].color = Color.clear;
					}
					else
					{
						float num9 = mapObject.GetCompassIconScale(magnitude);
						this.waypointSpriteList[waypointSpriteIndex].color = mapObject.GetMapIconColor();
						if (mapObject.IsTracked() && _mapObjectList[i].NearbyCompassBlink() && (mapObject.GetPosition() - entity.GetPosition()).magnitude <= 6f)
						{
							Color mapIconColor3 = mapObject.GetMapIconColor();
							float num10 = Mathf.PingPong(Time.time, 0.5f);
							this.waypointSpriteList[waypointSpriteIndex].color = Color.Lerp(Color.grey, mapIconColor3, num10 * 4f);
							if (num10 > 0.25f)
							{
								num9 += num10 - 0.25f;
							}
						}
						this.waypointSpriteList[waypointSpriteIndex].atlas = base.xui.GetAtlasByName("UIAtlas", spriteName);
						this.waypointSpriteList[waypointSpriteIndex].spriteName = spriteName;
						this.waypointSpriteList[waypointSpriteIndex].SetDimensions((int)(25f * num9), (int)(25f * num9));
						if (Vector2.Dot(normalized, rhs) >= 0.75f)
						{
							this.waypointSpriteList[waypointSpriteIndex].transform.localPosition = new Vector3(num + Vector2.Dot(normalized, rhs2) * num2, -16f);
						}
						else
						{
							this.waypointSpriteList[waypointSpriteIndex].transform.localPosition = new Vector3(num + ((Vector2.Dot(normalized, rhs2) < 0f) ? -0.675f : 0.675f) * num2, -16f);
						}
						if (mapObject.type == EnumMapObjectType.Entity)
						{
							this.waypointSpriteList[waypointSpriteIndex].depth = 12 + (int)(num9 * 100f);
						}
						if (!mapObject.IsTracked())
						{
							Color mapIconColor4 = mapObject.GetMapIconColor();
							this.waypointSpriteList[waypointSpriteIndex].color = new Color(mapIconColor4.r * 0.75f, mapIconColor4.g * 0.75f, mapIconColor4.b * 0.75f) * num9;
						}
						waypointSpriteIndex++;
					}
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateNavObjects(EntityPlayerLocal localPlayer, ref int waypointSpriteIndex)
	{
		float num = (float)base.ViewComponent.Size.x * 0.5f;
		float num2 = num * 1.15f;
		Transform transform = localPlayer.playerCamera.transform;
		Entity entity = (localPlayer.AttachedToEntity != null) ? localPlayer.AttachedToEntity : localPlayer;
		Vector2 b = new Vector2(entity.GetPosition().x, entity.GetPosition().z);
		Vector2 rhs = new Vector2(transform.forward.x, transform.forward.z);
		rhs.Normalize();
		Vector2 rhs2 = new Vector2(transform.right.x, transform.right.z);
		rhs2.Normalize();
		List<NavObject> navObjectList = NavObjectManager.Instance.NavObjectList;
		for (int i = 0; i < navObjectList.Count; i++)
		{
			NavObject navObject = navObjectList[i];
			if (navObject.IsValid() && navObject.HasRequirements)
			{
				if (waypointSpriteIndex >= this.waypointSpriteList.Count)
				{
					break;
				}
				if (navObject.NavObjectClass.IsOnCompass(navObject.IsActive) && !navObject.hiddenOnCompass)
				{
					NavObjectCompassSettings currentCompassSettings = navObject.CurrentCompassSettings;
					Vector2 vector = new Vector2(navObject.GetPosition().x + Origin.position.x, navObject.GetPosition().z + Origin.position.z) - b;
					float magnitude = vector.magnitude;
					bool flag = true;
					string spriteName = navObject.GetSpriteName(currentCompassSettings);
					float maxDistance = navObject.GetMaxDistance(currentCompassSettings, localPlayer);
					if ((maxDistance == -1f || magnitude <= maxDistance) && (currentCompassSettings.MinDistance <= 0f || magnitude >= currentCompassSettings.MinDistance))
					{
						this.waypointSpriteList[waypointSpriteIndex].depth = 12 + currentCompassSettings.DepthOffset;
						if (currentCompassSettings.HotZone != null)
						{
							float num3 = 1f;
							if (currentCompassSettings.HotZone.HotZoneType == NavObjectCompassSettings.HotZoneSettings.HotZoneTypes.Treasure)
							{
								float extraData = navObject.ExtraData;
								num3 = EffectManager.GetValue(PassiveEffects.TreasureRadius, null, extraData, localPlayer, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
								num3 = Mathf.Clamp(num3, 0f, extraData);
							}
							else if (currentCompassSettings.HotZone.HotZoneType == NavObjectCompassSettings.HotZoneSettings.HotZoneTypes.Custom)
							{
								num3 = currentCompassSettings.HotZone.CustomDistance;
							}
							if (magnitude < num3)
							{
								float num4 = Mathf.PingPong(Time.time, 0.25f);
								float num5 = 1.25f + num4;
								this.waypointSpriteList[waypointSpriteIndex].atlas = base.xui.GetAtlasByName("UIAtlas", currentCompassSettings.HotZone.SpriteName);
								this.waypointSpriteList[waypointSpriteIndex].spriteName = currentCompassSettings.HotZone.SpriteName;
								this.waypointSpriteList[waypointSpriteIndex].SetDimensions((int)(25f * num5), (int)(25f * num5));
								this.waypointSpriteList[waypointSpriteIndex].transform.localPosition = new Vector3(num, -24f);
								Color color = currentCompassSettings.HotZone.Color;
								this.waypointSpriteList[waypointSpriteIndex].color = Color.Lerp(color, Color.red, num4 * 4f);
								waypointSpriteIndex++;
								flag = false;
							}
						}
						if (currentCompassSettings.ShowVerticalCompassIcons)
						{
							this.waypointSpriteList[waypointSpriteIndex].flip = UIBasicSprite.Flip.Nothing;
							float num6 = localPlayer.GetPosition().y - Origin.position.y;
							if (navObject.GetPosition().y < num6 + currentCompassSettings.ShowDownOffset)
							{
								spriteName = currentCompassSettings.DownSpriteName;
							}
							else if (navObject.GetPosition().y > num6 + currentCompassSettings.ShowUpOffset)
							{
								spriteName = currentCompassSettings.UpSpriteName;
							}
							this.waypointSpriteList[waypointSpriteIndex].depth = 100;
							this.waypointSpriteList[waypointSpriteIndex].atlas = base.xui.GetAtlasByName("UIAtlas", spriteName);
							this.waypointSpriteList[waypointSpriteIndex].spriteName = spriteName;
						}
						if (flag)
						{
							Vector2 normalized = vector.normalized;
							if (!currentCompassSettings.IconClamped && Vector2.Dot(normalized, rhs) < 0.75f)
							{
								this.waypointSpriteList[waypointSpriteIndex].color = Color.clear;
							}
							else
							{
								float num7 = navObject.GetCompassIconScale(magnitude);
								this.waypointSpriteList[waypointSpriteIndex].color = (navObject.UseOverrideColor ? navObject.OverrideColor : currentCompassSettings.Color);
								if (currentCompassSettings.HasPulse && (navObject.GetPosition() - entity.GetPosition()).magnitude <= 6f)
								{
									Color b2 = navObject.UseOverrideColor ? navObject.OverrideColor : currentCompassSettings.Color;
									float num8 = Mathf.PingPong(Time.time, 0.5f);
									this.waypointSpriteList[waypointSpriteIndex].color = Color.Lerp(Color.grey, b2, num8 * 4f);
									if (num8 > 0.25f)
									{
										num7 += num8 - 0.25f;
									}
								}
								this.waypointSpriteList[waypointSpriteIndex].atlas = base.xui.GetAtlasByName("UIAtlas", spriteName);
								this.waypointSpriteList[waypointSpriteIndex].spriteName = spriteName;
								this.waypointSpriteList[waypointSpriteIndex].SetDimensions((int)(25f * num7), (int)(25f * num7));
								if (Vector2.Dot(normalized, rhs) >= 0.75f)
								{
									this.waypointSpriteList[waypointSpriteIndex].transform.localPosition = new Vector3(num + Vector2.Dot(normalized, rhs2) * num2, -16f);
								}
								else
								{
									this.waypointSpriteList[waypointSpriteIndex].transform.localPosition = new Vector3(num + ((Vector2.Dot(normalized, rhs2) < 0f) ? -0.675f : 0.675f) * num2, -16f);
								}
								if (!navObject.IsActive)
								{
									Color color2 = navObject.UseOverrideColor ? navObject.OverrideColor : currentCompassSettings.Color;
									if (currentCompassSettings.MinFadePercent != -1f)
									{
										if (currentCompassSettings.MinFadePercent > num7)
										{
											num7 = currentCompassSettings.MinFadePercent;
										}
										this.waypointSpriteList[waypointSpriteIndex].color = color2 * num7;
									}
									else
									{
										this.waypointSpriteList[waypointSpriteIndex].color = color2;
									}
								}
								waypointSpriteIndex++;
							}
						}
					}
				}
			}
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		base.xui.playerUI.windowManager.CloseIfOpen("windowpaging");
	}

	public static string ID = "";

	public List<UISprite> waypointSpriteList = new List<UISprite>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<ulong> daytimeFormatter = new CachedStringFormatter<ulong>((ulong _worldTime) => ValueDisplayFormatters.WorldTime(_worldTime, Localization.Get("xuiDayTimeLong", false)));

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatterInt dayFormatter = new CachedStringFormatterInt();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CachedStringFormatter<int, int> timeFormatter = new CachedStringFormatter<int, int>((int _hour, int _min) => string.Format("{0:00}:{1:00}", _hour, _min));

	[PublicizedFrom(EAccessModifier.Private)]
	public bool showSleeperVolumes = true;
}
