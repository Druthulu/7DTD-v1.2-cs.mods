﻿using System;
using System.Collections.Generic;
using Audio;
using GUI_2;
using InControl;
using Platform;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Scripting;

[UnityEngine.Scripting.Preserve]
public class XUiC_MapArea : XUiController
{
	public override void Init()
	{
		base.Init();
		if (this.mapTexture == null)
		{
			this.mapTexture = new Texture2D(2048, 2048, TextureFormat.RGBA32, false);
			this.mapTexture.name = "XUiC_MapArea.mapTexture";
		}
		NativeArray<Color32> rawTextureData = this.mapTexture.GetRawTextureData<Color32>();
		Color32 value = new Color32(0, 0, 0, byte.MaxValue);
		for (int i = 0; i < rawTextureData.Length; i += 4)
		{
			rawTextureData[i] = value;
		}
		XUiController childById = base.GetChildById("mapViewTexture");
		this.xuiTexture = (XUiV_Texture)childById.ViewComponent;
		this.transformSpritesParent = base.GetChildById("clippingPanel").ViewComponent.UiTransform;
		this.mapView = base.GetChildById("mapView");
		this.mapView.OnDrag += this.onMapDragged;
		this.mapView.OnScroll += this.onMapScrolled;
		this.mapView.OnPress += this.onMapPressedLeft;
		this.mapView.OnRightPress += this.onMapPressed;
		this.mapView.OnHover += this.onMapHover;
		this.zoomScale = 1f;
		this.targetZoomScale = 1f;
		base.xui.LoadData<GameObject>("Prefabs/MapSpriteEntity", delegate(GameObject o)
		{
			this.prefabMapSprite = o;
		});
		base.xui.LoadData<GameObject>("Prefabs/MapSpriteStartPoint", delegate(GameObject o)
		{
			this.prefabMapSpriteStartPoint = o;
		});
		base.xui.LoadData<GameObject>("Prefabs/MapSpritePrefab", delegate(GameObject o)
		{
			this.prefabMapSpritePrefab = o;
		});
		base.xui.LoadData<GameObject>("Prefabs/MapSpriteEntitySpawner", delegate(GameObject o)
		{
			this.prefabMapSpriteEntitySpawner = o;
		});
		this.initFOWChunkMaskColors();
		base.GetChildById("playerIcon").OnPress += this.onPlayerIconPressed;
		this.uiLblPlayerPos = (XUiV_Label)base.GetChildById("playerPos").ViewComponent;
		this.uiLblCursorPos = (XUiV_Label)base.GetChildById("cursorPos").ViewComponent;
		base.GetChildById("bedrollIcon").OnPress += this.onBedrollIconPressed;
		this.uiLblBedrollPos = (XUiV_Label)base.GetChildById("bedrollPos").ViewComponent;
		base.GetChildById("waypointIcon").OnPress += this.onWaypointIconPressed;
		this.uiLblMapMarkerDistance = (XUiV_Label)base.GetChildById("waypointDistance").ViewComponent;
		this.switchStaticMap = (XUiV_Button)base.GetChildById("switchStaticMap").ViewComponent;
		base.GetChildById("switchStaticMap").OnPress += delegate(XUiController _sender, int _args)
		{
			this.showStaticData = !this.showStaticData;
			this.switchStaticMap.Selected = this.showStaticData;
			this.cbxStaticMapType.ViewComponent.IsVisible = this.showStaticData;
		};
		this.cbxStaticMapType = base.GetChildByType<XUiC_ComboBoxEnum<XUiC_MapArea.EStaticMapOverlay>>();
		this.cbxStaticMapType.OnValueChanged += this.CbxStaticMapType_OnValueChanged;
		this.bShouldRedrawMap = true;
		this.initMap();
		this.kilometers = Localization.Get("xuiKilometers", false);
		this.crosshair = (base.GetChildById("crosshair").ViewComponent as XUiV_Sprite);
		NavObjectManager.Instance.OnNavObjectRemoved += this.Instance_OnNavObjectRemoved;
		if (GameManager.Instance.IsEditMode() && !PrefabEditModeManager.Instance.IsActive())
		{
			this.showStaticData = true;
			this.switchStaticMap.Selected = true;
			this.cbxStaticMapType.Value = XUiC_MapArea.EStaticMapOverlay.Biomes;
		}
		this.mapView.ViewComponent.IsSnappable = false;
		childById.ViewComponent.IsSnappable = false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Instance_OnNavObjectRemoved(NavObject newNavObject)
	{
		UnityEngine.Object.Destroy(this.keyToNavSprite[newNavObject.Key]);
		this.keyToNavObject.Remove(newNavObject.Key);
		this.keyToNavSprite.Remove(newNavObject.Key);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void initFOWChunkMaskColors()
	{
		base.xui.LoadData<Texture2D>("Textures/UI/fow_chunkMask", delegate(Texture2D o)
		{
			Color32[] pixels = o.GetPixels32();
			for (int i = 0; i < 3; i++)
			{
				for (int j = 0; j < 3; j++)
				{
					byte[] array = new byte[256];
					int num = 0;
					for (int k = i * 16; k < (i + 1) * 16; k++)
					{
						for (int l = j * 16; l < (j + 1) * 16; l++)
						{
							array[num++] = pixels[k * o.width + l].r;
						}
					}
					this.fowChunkMaskAlphas[i * 3 + j] = array;
				}
			}
			int num2 = 3;
			for (int m = 0; m < 4; m++)
			{
				byte[] array2 = new byte[256];
				int num3 = 0;
				for (int n = num2 * 16; n < (num2 + 1) * 16; n++)
				{
					for (int num4 = m * 16; num4 < (m + 1) * 16; num4++)
					{
						array2[num3++] = pixels[n * o.width + num4].r;
					}
				}
				this.fowChunkMaskAlphas[num2 * 3 + m] = array2;
			}
		});
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.closeAllPopups();
		base.xui.playerUI.windowManager.OpenIfNotOpen("windowpaging", false, false, true);
		if (this.isOpen)
		{
			return;
		}
		this.isOpen = true;
		this.localPlayer = base.xui.playerUI.entityPlayer;
		this.bFowMaskEnabled = !GameManager.Instance.IsEditMode();
		this.switchStaticMap.IsVisible = (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && (GameManager.Instance.IsEditMode() || GamePrefs.GetBool(EnumGamePrefs.DebugMenuEnabled)));
		this.cbxStaticMapType.ViewComponent.IsVisible = (this.switchStaticMap.IsVisible && this.showStaticData);
		this.initExistingWaypoints(this.localPlayer.Waypoints);
		this.initMap();
		this.positionMap();
		this.PositionMapAt(this.localPlayer.GetPosition());
		base.xui.playerUI.GetComponentInParent<LocalPlayerCamera>().PreRender += this.OnPreRender;
		base.xui.calloutWindow.ClearCallouts(XUiC_GamepadCalloutWindow.CalloutType.Menu);
		base.xui.calloutWindow.AddCallout(UIUtils.ButtonIcon.RightStick, "igcoMapMoveNoHold", XUiC_GamepadCalloutWindow.CalloutType.Menu);
		base.xui.calloutWindow.AddCallout(UIUtils.ButtonIcon.FaceButtonSouth, "igcoMapWaypoint", XUiC_GamepadCalloutWindow.CalloutType.Menu);
		base.xui.calloutWindow.AddCallout(UIUtils.ButtonIcon.RightTrigger, "igcoMapZoomIn", XUiC_GamepadCalloutWindow.CalloutType.Menu);
		base.xui.calloutWindow.AddCallout(UIUtils.ButtonIcon.LeftTrigger, "igcoMapZoomOut", XUiC_GamepadCalloutWindow.CalloutType.Menu);
		base.xui.calloutWindow.AddCallout(UIUtils.ButtonIcon.FaceButtonEast, "igcoExit", XUiC_GamepadCalloutWindow.CalloutType.Menu);
		base.xui.calloutWindow.EnableCallouts(XUiC_GamepadCalloutWindow.CalloutType.Menu, 0f);
		XUiC_WindowSelector childByType = base.xui.FindWindowGroupByName("windowpaging").GetChildByType<XUiC_WindowSelector>();
		if (childByType != null)
		{
			childByType.SetSelected("map");
		}
		this.crosshair.IsVisible = false;
		this.windowGroup.isEscClosable = false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetMapCursor(bool _customCursor)
	{
		this.bMapCursorSet = _customCursor;
		if (_customCursor)
		{
			CursorControllerAbs.SetCursor(CursorControllerAbs.ECursorType.Map);
			return;
		}
		CursorControllerAbs.SetCursor(CursorControllerAbs.ECursorType.Default);
	}

	public override void OnClose()
	{
		base.OnClose();
		base.xui.playerUI.windowManager.CloseIfOpen("windowpaging");
		if (!this.isOpen)
		{
			return;
		}
		this.isOpen = false;
		this.bShouldRedrawMap = false;
		if (this.bMapCursorSet)
		{
			this.SetMapCursor(false);
			base.xui.currentToolTip.ToolTip = string.Empty;
		}
		base.xui.playerUI.GetComponentInParent<LocalPlayerCamera>().PreRender -= this.OnPreRender;
		base.xui.calloutWindow.DisableCallouts(XUiC_GamepadCalloutWindow.CalloutType.Menu);
		base.xui.playerUI.CursorController.Locked = false;
		SoftCursor.SetCursor(CursorControllerAbs.ECursorType.Default);
		this.closestMouseOverNavObject = null;
	}

	public override void OnCursorSelected()
	{
		base.OnCursorSelected();
		this.crosshair.IsVisible = (PlatformManager.NativePlatform.Input.CurrentInputStyle != PlayerInputManager.InputStyle.Keyboard);
	}

	public override void OnCursorUnSelected()
	{
		base.OnCursorUnSelected();
		this.crosshair.IsVisible = false;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (!this.windowGroup.isShowing)
		{
			return;
		}
		if (!XUi.IsGameRunning() || base.xui.playerUI.entityPlayer == null)
		{
			return;
		}
		if (!this.bMapInitialized)
		{
			this.initMap();
		}
		if (base.xui.playerUI.playerInput.GUIActions.LastDeviceClass == InputDeviceClass.Controller && !base.xui.GetWindow("mapAreaSetWaypoint").IsVisible)
		{
			Vector2 a = -base.xui.playerUI.playerInput.GUIActions.Camera.Vector;
			if (a.sqrMagnitude > 0f)
			{
				this.DragMap(a * 500f * _dt);
			}
		}
		this.updateMapOverlay();
		if (this.bShouldRedrawMap)
		{
			this.updateFullMap();
			this.bShouldRedrawMap = false;
		}
		if (this.timeToRedrawMap > 0f)
		{
			this.timeToRedrawMap -= _dt;
			if (this.timeToRedrawMap <= 0f)
			{
				this.bShouldRedrawMap = true;
			}
		}
		if (this.localPlayer.ChunkObserver.mapDatabase.IsNetworkDataAvail())
		{
			this.timeToRedrawMap = 0.5f;
			this.localPlayer.ChunkObserver.mapDatabase.ResetNetworkDataAvail();
		}
		this.uiLblPlayerPos.Text = ValueDisplayFormatters.WorldPos(base.xui.playerUI.entityPlayer.GetPosition(), " ", false);
		Vector3 pos = this.screenPosToWorldPos(base.xui.playerUI.CursorController.GetScreenPosition(), false);
		this.uiLblCursorPos.Text = ValueDisplayFormatters.WorldPos(pos, " ", false);
		this.uiLblCursorPos.UiTransform.gameObject.SetActive(this.bMouseOverMap);
		this.uiLblBedrollPos.Text = ((this.localPlayer.SpawnPoints.Count > 0) ? ValueDisplayFormatters.WorldPos(this.localPlayer.SpawnPoints[0].ToVector3(), " ", false) : string.Empty);
		string text = string.Empty;
		if (this.localPlayer.markerPosition != Vector3i.zero)
		{
			text = string.Format("{0} {1}", ((this.localPlayer.position - this.localPlayer.markerPosition.ToVector3()).magnitude / 1000f).ToCultureInvariantString("0.0"), this.kilometers);
		}
		this.uiLblMapMarkerDistance.Text = text;
		float num = 5f * base.xui.playerUI.playerInput.GUIActions.TriggerAxis.Value;
		if (num != 0f)
		{
			this.targetZoomScale = Utils.FastClamp(this.targetZoomScale + num * _dt, 0.7f, 6.15f);
		}
		this.zoomScale = Mathf.Lerp(this.zoomScale, this.targetZoomScale, 5f * _dt);
		this.positionMap();
		this.updateMapObjects();
		this.UpdateWaypointSelection();
		if (base.xui.playerUI.playerInput.GUIActions.Cancel.WasPressed || base.xui.playerUI.playerInput.PermanentActions.Cancel.WasPressed)
		{
			XUiV_Window window = base.xui.GetWindow("mapAreaSetWaypoint");
			if (window.IsVisible)
			{
				window.IsVisible = false;
				base.xui.GetWindow("mapAreaChooseWaypoint").IsVisible = false;
				base.xui.GetWindow("mapAreaEnterWaypointName").IsVisible = false;
				base.xui.playerUI.CursorController.SetNavigationTargetLater(base.GetChildById("MapView").ViewComponent);
				return;
			}
			base.xui.playerUI.windowManager.CloseAllOpenWindows(null, true);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public unsafe void updateMapOverlay()
	{
		if (this.showStaticData != (this.staticWorldTexture != null))
		{
			this.bShouldRedrawMap = true;
			if (!this.showStaticData)
			{
				this.staticWorldTexture = null;
				return;
			}
			if (this.staticWorldTexture == null)
			{
				World world = GameManager.Instance.World;
				ChunkProviderGenerateWorldFromRaw chunkProviderGenerateWorldFromRaw = world.ChunkCache.ChunkProvider as ChunkProviderGenerateWorldFromRaw;
				if (chunkProviderGenerateWorldFromRaw == null)
				{
					return;
				}
				IBiomeProvider biomeProvider = chunkProviderGenerateWorldFromRaw.GetBiomeProvider();
				WorldDecoratorPOIFromImage poiFromImage = chunkProviderGenerateWorldFromRaw.poiFromImage;
				if (biomeProvider == null || poiFromImage == null)
				{
					return;
				}
				int splat3Width = poiFromImage.splat3Width;
				int splat3Height = poiFromImage.splat3Height;
				int num = splat3Width / 2;
				int num2 = splat3Height / 2;
				this.staticWorldWidth = splat3Width;
				this.staticWorldHeight = splat3Height;
				this.staticMapLeft = -this.staticWorldWidth / 2;
				this.staticMapRight = this.staticWorldWidth / 2 - 1;
				this.staticMapBottom = -this.staticWorldHeight / 2;
				this.staticMapTop = this.staticWorldHeight / 2 - 1;
				this.staticWorldTexture = new Color32[poiFromImage.m_Poi.DimX * poiFromImage.m_Poi.DimY];
				Color32 color = new Color32(0, 0, 0, byte.MaxValue);
				for (int i = 0; i < splat3Height; i++)
				{
					int num3 = i * splat3Width;
					ReadOnlySpan<ushort> readOnlySpan2;
					using (chunkProviderGenerateWorldFromRaw.heightData.GetReadOnlySpan(num3, splat3Width, out readOnlySpan2))
					{
						int j = 0;
						while (j < splat3Width)
						{
							color.r = 0;
							color.g = 0;
							color.b = 0;
							byte value = poiFromImage.m_Poi.colors.GetValue(j, i);
							if (value == 1)
							{
								color.r = (color.b = byte.MaxValue);
								goto IL_444;
							}
							if (value == 2)
							{
								color.r = byte.MaxValue;
								goto IL_444;
							}
							if (value == 3)
							{
								color.g = byte.MaxValue;
								goto IL_444;
							}
							if (value == 4)
							{
								color.b = byte.MaxValue;
								goto IL_444;
							}
							PoiMapElement poiForColor;
							if (value != 0 && (poiForColor = world.Biomes.getPoiForColor((uint)value)) != null && poiForColor.m_BlockValue.Block.blockMaterial.IsLiquid)
							{
								color.b = byte.MaxValue;
								goto IL_444;
							}
							byte b = (byte)((float)(*readOnlySpan2[j]) * 0.00389105058f);
							color.r = (color.g = (color.b = b));
							if (this.cbxStaticMapType.Value == XUiC_MapArea.EStaticMapOverlay.Biomes)
							{
								BiomeDefinition biomeAt = biomeProvider.GetBiomeAt(j - num, i - num2);
								if (biomeAt != null)
								{
									color.r = (byte)Mathf.LerpUnclamped((float)color.r, biomeAt.m_uiColor >> 16 & 255U, 0.25f);
									color.g = (byte)Mathf.LerpUnclamped((float)color.g, biomeAt.m_uiColor >> 8 & 255U, 0.25f);
									color.b = (byte)Mathf.LerpUnclamped((float)color.b, biomeAt.m_uiColor & 255U, 0.25f);
									goto IL_444;
								}
								goto IL_444;
							}
							else
							{
								if (this.cbxStaticMapType.Value != XUiC_MapArea.EStaticMapOverlay.Radiation)
								{
									goto IL_444;
								}
								float radiationAt = biomeProvider.GetRadiationAt(j - num, i - num2);
								if (radiationAt >= 0.5f)
								{
									if (radiationAt < 1.5f)
									{
										color.r = (byte)Mathf.LerpUnclamped((float)color.r, 0f, 0.25f);
										color.g = (byte)Mathf.LerpUnclamped((float)color.g, 255f, 0.25f);
										color.b = (byte)Mathf.LerpUnclamped((float)color.b, 0f, 0.25f);
										goto IL_444;
									}
									if (radiationAt < 2.5f)
									{
										color.r = (byte)Mathf.LerpUnclamped((float)color.r, 0f, 0.25f);
										color.g = (byte)Mathf.LerpUnclamped((float)color.g, 0f, 0.25f);
										color.b = (byte)Mathf.LerpUnclamped((float)color.b, 255f, 0.25f);
										goto IL_444;
									}
									color.r = (byte)Mathf.LerpUnclamped((float)color.r, 255f, 0.25f);
									color.g = (byte)Mathf.LerpUnclamped((float)color.g, 0f, 0.25f);
									color.b = (byte)Mathf.LerpUnclamped((float)color.b, 0f, 0.25f);
									goto IL_444;
								}
							}
							IL_456:
							j++;
							continue;
							IL_444:
							this.staticWorldTexture[num3 + j] = color;
							goto IL_456;
						}
					}
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void initExistingWaypoints(WaypointCollection _waypoints)
	{
		if (!this.existingWaypointsInitialized)
		{
			using (List<Waypoint>.Enumerator enumerator = _waypoints.Collection.list.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					Waypoint w = enumerator.Current;
					GeneratedTextManager.GetDisplayText(w.name, delegate(string _filtered)
					{
						w.navObject.name = _filtered;
					}, true, false, GeneratedTextManager.TextFilteringMode.FilterWithSafeString, GeneratedTextManager.BbCodeSupportMode.SupportedAndAddEscapes);
				}
			}
			this.existingWaypointsInitialized = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void initMap()
	{
		if (base.xui.playerUI.entityPlayer == null)
		{
			return;
		}
		this.localPlayer = base.xui.playerUI.entityPlayer;
		this.bMapInitialized = true;
		this.xuiTexture.Texture = this.mapTexture;
		this.xuiTexture.Size = new Vector2i(712, 712);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateFullMap()
	{
		int mapStartX = (int)this.mapMiddlePosChunks.x - 1024;
		int mapEndX = (int)this.mapMiddlePosChunks.x + 1024;
		int mapStartZ = (int)this.mapMiddlePosChunks.y - 1024;
		int mapEndZ = (int)this.mapMiddlePosChunks.y + 1024;
		this.updateMapSection(mapStartX, mapStartZ, mapEndX, mapEndZ, 0, 0, 2048, 2048);
		this.mapScrollTextureOffset.x = 0f;
		this.mapScrollTextureOffset.y = 0f;
		this.mapScrollTextureChunksOffsetX = 0;
		this.mapScrollTextureChunksOffsetZ = 0;
		this.positionMap();
		this.mapTexture.Apply();
		this.SendMapPositionToServer();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateMapForScroll(int deltaChunksX, int deltaChunksZ)
	{
		if (deltaChunksX != 0)
		{
			int num = Mathf.Abs(deltaChunksX);
			int num2 = this.mapScrollTextureChunksOffsetX * 16;
			int num3 = (this.mapScrollTextureChunksOffsetX + deltaChunksX) * 16;
			num3 = Utils.WrapInt(num3, 0, 2048);
			int num4;
			int num5;
			if (deltaChunksX > 0)
			{
				if (num2 == 2048)
				{
					num2 = 0;
				}
				num4 = (int)this.mapMiddlePosChunks.x + 1024;
				num5 = num4 - num * 16;
			}
			else
			{
				if (num2 == 0)
				{
					num2 = 2048;
				}
				int num6 = num2;
				num2 = num3;
				num3 = num6;
				num5 = (int)this.mapMiddlePosChunks.x - 1024;
				num4 = num5 + num * 16;
			}
			int num7 = (this.mapScrollTextureChunksOffsetZ + deltaChunksZ) * 16;
			num7 = Utils.WrapIndex(num7, 2048);
			int num8 = Utils.WrapIndex(num7 - 1, 2048);
			int num9 = (int)this.mapMiddlePosChunks.y - 1024;
			int num10 = num9 + 2048;
			this.updateMapSection(num5, num9, num4, num10, num2, num7, num3, num8);
		}
		if (deltaChunksZ != 0)
		{
			int num11 = Mathf.Abs(deltaChunksZ);
			int num7 = this.mapScrollTextureChunksOffsetZ * 16;
			int num8 = (this.mapScrollTextureChunksOffsetZ + deltaChunksZ) * 16;
			num8 = Utils.WrapInt(num8, 0, 2048);
			int num9;
			int num10;
			if (deltaChunksZ > 0)
			{
				if (num7 == 2048)
				{
					num7 = 0;
				}
				num10 = (int)this.mapMiddlePosChunks.y + 1024;
				num9 = num10 - num11 * 16;
			}
			else
			{
				if (num7 == 0)
				{
					num7 = 2048;
				}
				int num12 = num7;
				num7 = num8;
				num8 = num12;
				num9 = (int)this.mapMiddlePosChunks.y - 1024;
				num10 = num9 + num11 * 16;
			}
			int num2 = (this.mapScrollTextureChunksOffsetX + deltaChunksX) * 16;
			num2 = Utils.WrapIndex(num2, 2048);
			int num3 = Utils.WrapIndex(num2 - 1, 2048);
			int num5 = (int)this.mapMiddlePosChunks.x - 1024;
			int num4 = num5 + 2048;
			this.updateMapSection(num5, num9, num4, num10, num2, num7, num3, num8);
		}
		this.mapScrollTextureOffset.x = this.mapScrollTextureOffset.x + (float)(deltaChunksX * 16) / (float)this.mapTexture.width;
		this.mapScrollTextureOffset.y = this.mapScrollTextureOffset.y + (float)(deltaChunksZ * 16) / (float)this.mapTexture.width;
		this.mapScrollTextureOffset.x = Utils.WrapFloat(this.mapScrollTextureOffset.x, 0f, 1f);
		this.mapScrollTextureOffset.y = Utils.WrapFloat(this.mapScrollTextureOffset.y, 0f, 1f);
		this.mapScrollTextureChunksOffsetX += deltaChunksX;
		this.mapScrollTextureChunksOffsetZ += deltaChunksZ;
		this.mapScrollTextureChunksOffsetX = Utils.WrapIndex(this.mapScrollTextureChunksOffsetX, 128);
		this.mapScrollTextureChunksOffsetZ = Utils.WrapIndex(this.mapScrollTextureChunksOffsetZ, 128);
		this.positionMap();
		this.mapTexture.Apply();
		this.SendMapPositionToServer();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateMapSection(int mapStartX, int mapStartZ, int mapEndX, int mapEndZ, int drawnMapStartX, int drawnMapStartZ, int drawnMapEndX, int drawnMapEndZ)
	{
		IMapChunkDatabase mapDatabase = this.localPlayer.ChunkObserver.mapDatabase;
		bool flag = this.showStaticData && this.staticWorldTexture != null;
		NativeArray<Color32> rawTextureData = this.mapTexture.GetRawTextureData<Color32>();
		int i = mapStartZ;
		int num = drawnMapStartZ;
		while (i < mapEndZ)
		{
			int j = mapStartX;
			int num2 = drawnMapStartX;
			while (j < mapEndX)
			{
				int num3 = World.toChunkXZ(j);
				int num4 = World.toChunkXZ(i);
				if (flag)
				{
					int num5 = num3 << 4;
					int num6 = num4 << 4;
					for (int k = 0; k < 256; k++)
					{
						int num7 = k / 16;
						int num8 = k % 16;
						int num9 = (num + num7) * 2048;
						int num10 = num2 + num8;
						int index = num9 + num10;
						int num11 = num5 + num8;
						int num12 = num6 + num7;
						if (num11 < this.staticMapLeft || num11 > this.staticMapRight || num12 < this.staticMapBottom || num12 > this.staticMapTop)
						{
							rawTextureData[index] = new Color32(0, 0, 0, 0);
						}
						else
						{
							int num13 = num11 - this.staticMapLeft + (num12 - this.staticMapBottom) * this.staticWorldWidth;
							Color32 color = this.staticWorldTexture[num13];
							if (color.a > 0)
							{
								rawTextureData[index] = new Color32(color.r, color.g, color.b, byte.MaxValue);
							}
							else
							{
								rawTextureData[index] = new Color32(0, 0, 0, 0);
							}
						}
					}
				}
				else
				{
					long chunkKey = WorldChunkCache.MakeChunkKey(num3, num4);
					ushort[] mapColors = mapDatabase.GetMapColors(chunkKey);
					if (mapColors == null)
					{
						for (int l = 0; l < 256; l++)
						{
							int num14 = (num + l / 16) * 2048;
							int index2 = num2 + l % 16 + num14;
							rawTextureData[index2] = new Color32(0, 0, 0, 0);
						}
					}
					else
					{
						bool flag2 = mapDatabase.Contains(WorldChunkCache.MakeChunkKey(num3, num4 + 1));
						bool flag3 = mapDatabase.Contains(WorldChunkCache.MakeChunkKey(num3, num4 - 1));
						bool flag4 = mapDatabase.Contains(WorldChunkCache.MakeChunkKey(num3 - 1, num4));
						bool flag5 = mapDatabase.Contains(WorldChunkCache.MakeChunkKey(num3 + 1, num4));
						int num15 = 0;
						if (flag2 && flag3 && flag4 && flag5)
						{
							bool flag6 = mapDatabase.Contains(WorldChunkCache.MakeChunkKey(num3 - 1, num4 + 1));
							bool flag7 = mapDatabase.Contains(WorldChunkCache.MakeChunkKey(num3 + 1, num4 + 1));
							bool flag8 = mapDatabase.Contains(WorldChunkCache.MakeChunkKey(num3 - 1, num4 - 1));
							bool flag9 = mapDatabase.Contains(WorldChunkCache.MakeChunkKey(num3 + 1, num4 - 1));
							if (!flag6)
							{
								num15 = 9;
							}
							else if (!flag7)
							{
								num15 = 10;
							}
							else if (!flag9)
							{
								num15 = 11;
							}
							else if (!flag8)
							{
								num15 = 12;
							}
							else
							{
								num15 = 4;
							}
						}
						else
						{
							if (flag3 && !flag2)
							{
								num15 += 6;
							}
							else if (flag3 && flag2)
							{
								num15 += 3;
							}
							if (flag5 && flag4)
							{
								num15++;
							}
							else if (flag4)
							{
								num15 += 2;
							}
						}
						byte[] array = this.fowChunkMaskAlphas[num15];
						if (!this.bFowMaskEnabled)
						{
							array = this.fowChunkMaskAlphas[4];
						}
						for (int m = 0; m < 256; m++)
						{
							int num16 = m / 16;
							int num17 = m % 16;
							int num18 = (num + num16) * 2048;
							int num19 = num2 + num17;
							int index3 = num18 + num19;
							int num20 = num16 * 16;
							byte b = array[num20 + num17];
							Color32 color2 = Utils.FromColor5To32(mapColors[m]);
							rawTextureData[index3] = new Color32(color2.r, color2.g, color2.b, (b < byte.MaxValue) ? b : byte.MaxValue);
						}
					}
				}
				j += 16;
				num2 = Utils.WrapIndex(num2 + 16, 2048);
			}
			i += 16;
			num = Utils.WrapIndex(num + 16, 2048);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SendMapPositionToServer()
	{
		if (GameManager.Instance.World.IsRemote() && !this.mapMiddlePosChunksToServer.Equals(this.mapMiddlePosChunks))
		{
			this.mapMiddlePosChunksToServer = this.mapMiddlePosChunks;
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageMapPosition>().Setup(this.localPlayer.entityId, new Vector2i(Utils.Fastfloor(this.mapMiddlePosChunks.x), Utils.Fastfloor(this.mapMiddlePosChunks.y))), false);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void positionMap()
	{
		float num = (2048f - 336f * this.zoomScale) / 2f;
		this.mapScale = 336f * this.zoomScale / 2048f;
		float num2 = (num + (this.mapMiddlePosPixel.x - this.mapMiddlePosChunks.x)) / 2048f;
		float num3 = (num + (this.mapMiddlePosPixel.y - this.mapMiddlePosChunks.y)) / 2048f;
		this.mapPos = new Vector3(num2 + this.mapScrollTextureOffset.x, num3 + this.mapScrollTextureOffset.y, 0f);
		this.mapBGPos.x = (num + this.mapMiddlePosPixel.x) / 2048f;
		this.mapBGPos.y = (num + this.mapMiddlePosPixel.y) / 2048f;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnPreRender(LocalPlayerCamera _localPlayerCamera)
	{
		Shader.SetGlobalVector("_MainMapPosAndScale", new Vector4(this.mapPos.x, this.mapPos.y, this.mapScale, this.mapScale));
		Shader.SetGlobalVector("_MainMapBGPosAndScale", new Vector4(this.mapBGPos.x, this.mapBGPos.y, this.mapScale, this.mapScale));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void onMapDragged(XUiController _sender, EDragType _dragType, Vector2 _mousePositionDelta)
	{
		if (UICamera.currentKey != KeyCode.Mouse0)
		{
			return;
		}
		if (base.xui.playerUI.playerInput.GUIActions.LastDeviceClass != InputDeviceClass.Controller)
		{
			this.DragMap(_mousePositionDelta);
		}
		this.closeAllPopups();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DragMap(Vector2 delta)
	{
		float pixelRatioFactor = base.xui.GetPixelRatioFactor();
		this.mapMiddlePosPixel -= delta * pixelRatioFactor * 0.471910119f * this.zoomScale;
		this.mapMiddlePosPixel = GameManager.Instance.World.ClampToValidWorldPosForMap(this.mapMiddlePosPixel);
		int num = 0;
		int num2 = 0;
		while (this.mapMiddlePosChunks.x - this.mapMiddlePosPixel.x >= 16f)
		{
			this.mapMiddlePosChunks.x = this.mapMiddlePosChunks.x - 16f;
			num--;
		}
		while (this.mapMiddlePosChunks.x - this.mapMiddlePosPixel.x <= -16f)
		{
			this.mapMiddlePosChunks.x = this.mapMiddlePosChunks.x + 16f;
			num++;
		}
		while (this.mapMiddlePosChunks.y - this.mapMiddlePosPixel.y >= 16f)
		{
			this.mapMiddlePosChunks.y = this.mapMiddlePosChunks.y - 16f;
			num2--;
		}
		while (this.mapMiddlePosChunks.y - this.mapMiddlePosPixel.y <= -16f)
		{
			this.mapMiddlePosChunks.y = this.mapMiddlePosChunks.y + 16f;
			num2++;
		}
		if (num != 0 || num2 != 0)
		{
			this.updateMapForScroll(num, num2);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void onMapScrolled(XUiController _sender, float _delta)
	{
		float num = 6f;
		if (InputUtils.ShiftKeyPressed)
		{
			num = 5f * this.zoomScale;
		}
		float min = 0.7f;
		float max = 6.15f;
		this.targetZoomScale = Utils.FastClamp(this.zoomScale - _delta * num, min, max);
		if (_delta < 0f)
		{
			Manager.PlayInsidePlayerHead("map_zoom_in", -1, 0f, false, false);
		}
		else
		{
			Manager.PlayInsidePlayerHead("map_zoom_out", -1, 0f, false, false);
		}
		this.closeAllPopups();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateMapObject(EnumMapObjectType _type, long _key, string _name, Vector3 _position, Vector3 _size, GameObject _prefab)
	{
		GameObject gameObject;
		if (!this.keyToMapSprite.TryGetValue(_key, out gameObject))
		{
			gameObject = this.transformSpritesParent.gameObject.AddChild(_prefab);
			gameObject.GetComponent<UISprite>().depth = 20;
			gameObject.name = _name;
			gameObject.GetComponent<UISprite>().depth = 1;
			this.keyToMapObject[_key] = new MapObject(_type, _position, _key, null, true);
			this.keyToMapSprite[_key] = gameObject;
		}
		if (gameObject)
		{
			float num = this.getSpriteZoomScaleFac() * 4.3f;
			UISprite component = gameObject.GetComponent<UISprite>();
			component.width = (int)(_size.x * num);
			component.height = (int)(_size.z * num);
			Transform transform = gameObject.transform;
			transform.localPosition = this.worldPosToScreenPos(_position);
			transform.localRotation = Quaternion.identity;
			this.mapObjectsOnMapAlive.Add(_key);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public float getSpriteZoomScaleFac()
	{
		return Utils.FastClamp(1f / (this.zoomScale * 2f), 0.02f, 20f);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateMapObjectList(List<MapObject> _mapObjectList, bool _bConsiderInOnMouseOverCursor = false)
	{
		bool flag = false;
		for (int i = 0; i < _mapObjectList.Count; i++)
		{
			MapObject mapObject = _mapObjectList[i];
			mapObject.RefreshData();
			long num = (long)mapObject.type << 32 | mapObject.key;
			if (mapObject.IsMapIconEnabled())
			{
				GameObject gameObject;
				UISprite component;
				if (!this.keyToMapSprite.ContainsKey(num))
				{
					gameObject = this.transformSpritesParent.gameObject.AddChild(this.prefabMapSprite);
					component = gameObject.transform.Find("Sprite").GetComponent<UISprite>();
					string mapIcon = mapObject.GetMapIcon();
					component.atlas = base.xui.GetAtlasByName(((UnityEngine.Object)component.atlas).name, mapIcon);
					component.spriteName = mapIcon;
					component.depth = mapObject.GetLayerForMapIcon();
					component.gameObject.GetComponent<TweenAlpha>().enabled = mapObject.IsMapIconBlinking();
					this.keyToMapObject[num] = mapObject;
					this.keyToMapSprite[num] = gameObject;
				}
				else
				{
					gameObject = this.keyToMapSprite[num];
				}
				string text = mapObject.IsShowName() ? mapObject.GetName() : null;
				if (text != null)
				{
					UILabel component2 = gameObject.transform.Find("Name").GetComponent<UILabel>();
					component2.text = text;
					component2.gameObject.SetActive(true);
					component2.color = mapObject.GetMapIconColor();
				}
				float spriteZoomScaleFac = this.getSpriteZoomScaleFac();
				component = gameObject.transform.Find("Sprite").GetComponent<UISprite>();
				Vector3 vector = mapObject.GetMapIconScale() * spriteZoomScaleFac;
				component.width = (int)((float)this.cSpriteScale * vector.x);
				component.height = (int)((float)this.cSpriteScale * vector.y);
				component.color = mapObject.GetMapIconColor();
				component.gameObject.transform.localEulerAngles = new Vector3(0f, 0f, -mapObject.GetRotation().y);
				gameObject.transform.localPosition = this.worldPosToScreenPos(mapObject.GetPosition());
				if (mapObject.IsCenterOnLeftBottomCorner())
				{
					gameObject.transform.localPosition += new Vector3((float)(component.width / 2), (float)(component.height / 2), 0f);
				}
				if (_bConsiderInOnMouseOverCursor)
				{
					Vector3 vector2 = this.worldPosToScreenPos(mapObject.GetPosition());
					vector2.y = -vector2.y;
					Vector3 b = this.mousePosToWindowPos(base.xui.playerUI.CursorController.GetScreenPosition());
					if (vector2.x > 0f && vector2.x < 712f && vector2.y > 0f && vector2.y < 712f && Utils.FastAbs((vector2 - b).magnitude) < 30f)
					{
						if (!this.bMapCursorSet)
						{
							this.SetMapCursor(true);
						}
						if (base.xui.currentToolTip.ToolTip != mapObject.GetName() && !string.IsNullOrEmpty(mapObject.GetName()))
						{
							base.xui.currentToolTip.ToolTip = mapObject.GetName();
							if (mapObject is MapObjectWaypoint)
							{
								this.selectWaypoint(((MapObjectWaypoint)mapObject).waypoint);
							}
						}
						flag = true;
					}
				}
				this.mapObjectsOnMapAlive.Add(num);
			}
		}
		if (_bConsiderInOnMouseOverCursor && !flag && this.bMapCursorSet)
		{
			this.SetMapCursor(false);
			base.xui.currentToolTip.ToolTip = string.Empty;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateNavObjectList()
	{
		bool flag = true;
		bool flag2 = false;
		List<NavObject> navObjectList = NavObjectManager.Instance.NavObjectList;
		this.navObjectsOnMapAlive.Clear();
		for (int i = 0; i < navObjectList.Count; i++)
		{
			NavObject navObject = navObjectList[i];
			int key = navObject.Key;
			if (navObject.HasRequirements && navObject.NavObjectClass.IsOnMap(navObject.IsActive))
			{
				NavObjectMapSettings currentMapSettings = navObject.CurrentMapSettings;
				GameObject gameObject;
				UISprite component;
				if (!this.keyToNavObject.ContainsKey(key))
				{
					gameObject = this.transformSpritesParent.gameObject.AddChild(this.prefabMapSprite);
					component = gameObject.transform.Find("Sprite").GetComponent<UISprite>();
					string spriteName = navObject.GetSpriteName(currentMapSettings);
					component.atlas = base.xui.GetAtlasByName(((UnityEngine.Object)component.atlas).name, spriteName);
					component.spriteName = spriteName;
					component.depth = currentMapSettings.Layer;
					this.keyToNavObject[key] = navObject;
					this.keyToNavSprite[key] = gameObject;
				}
				else
				{
					gameObject = this.keyToNavSprite[key];
				}
				EntityPlayer entityPlayer = navObject.TrackedEntity as EntityPlayer;
				string text = (entityPlayer != null) ? entityPlayer.PlayerDisplayName : navObject.DisplayName;
				if (!string.IsNullOrEmpty(text))
				{
					UILabel component2 = gameObject.transform.Find("Name").GetComponent<UILabel>();
					component2.text = text;
					component2.font = base.xui.GetUIFontByName("ReferenceFont", true);
					component2.gameObject.SetActive(true);
					component2.color = (navObject.UseOverrideColor ? navObject.OverrideColor : currentMapSettings.Color);
				}
				else
				{
					gameObject.transform.Find("Name").GetComponent<UILabel>().text = "";
				}
				float spriteZoomScaleFac = this.getSpriteZoomScaleFac();
				component = gameObject.transform.Find("Sprite").GetComponent<UISprite>();
				Vector3 vector = currentMapSettings.IconScaleVector * spriteZoomScaleFac;
				component.width = Mathf.Clamp((int)((float)this.cSpriteScale * vector.x), 9, 100);
				component.height = Mathf.Clamp((int)((float)this.cSpriteScale * vector.y), 9, 100);
				component.color = (navObject.hiddenOnCompass ? Color.grey : (navObject.UseOverrideColor ? navObject.OverrideColor : currentMapSettings.Color));
				component.gameObject.transform.localEulerAngles = new Vector3(0f, 0f, -navObject.Rotation.y);
				gameObject.transform.localPosition = this.worldPosToScreenPos(navObject.GetPosition() + Origin.position);
				if (currentMapSettings.AdjustCenter)
				{
					gameObject.transform.localPosition += new Vector3((float)(component.width / 2), (float)(component.height / 2), 0f);
				}
				this.navObjectsOnMapAlive.Add((long)key);
			}
		}
		if (flag && !flag2 && this.bMapCursorSet)
		{
			this.SetMapCursor(false);
			base.xui.currentToolTip.ToolTip = string.Empty;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void updateMapObjects()
	{
		WorldBase world = GameManager.Instance.World;
		this.navObjectsOnMapAlive.Clear();
		this.mapObjectsOnMapAlive.Clear();
		if (world.IsEditor() || this.showStaticData)
		{
			SpawnPointList spawnPointList = GameManager.Instance.GetSpawnPointList();
			for (int i = 0; i < spawnPointList.Count; i++)
			{
				SpawnPoint spawnPoint = spawnPointList[i];
				this.updateMapObject(EnumMapObjectType.StartPoint, (long)spawnPoint.GetHashCode() << 32 | (long)((ulong)-1), "SpawnPoint", spawnPoint.spawnPosition.position, Vector3.one * 30f, this.prefabMapSpriteStartPoint);
			}
			DynamicPrefabDecorator dynamicPrefabDecorator = GameManager.Instance.GetDynamicPrefabDecorator();
			List<PrefabInstance> list = (dynamicPrefabDecorator != null) ? dynamicPrefabDecorator.GetDynamicPrefabs() : null;
			if (list != null)
			{
				for (int j = 0; j < list.Count; j++)
				{
					PrefabInstance prefabInstance = list[j];
					Vector3 position = prefabInstance.boundingBoxPosition.ToVector3() + prefabInstance.boundingBoxSize.ToVector3() * 0.5f;
					this.updateMapObject(EnumMapObjectType.Prefab, (long)prefabInstance.id << 32 | (long)((ulong)-3), prefabInstance.name, position, prefabInstance.boundingBoxSize.ToVector3(), this.prefabMapSpritePrefab);
				}
			}
		}
		this.updateNavObjectList();
		foreach (KeyValuePair<int, NavObject> keyValuePair in this.keyToNavObject.Dict)
		{
			if (!this.navObjectsOnMapAlive.Contains((long)keyValuePair.Key))
			{
				this.keyToNavObject.MarkToRemove(keyValuePair.Key);
				this.keyToNavSprite.MarkToRemove(keyValuePair.Key);
			}
		}
		foreach (KeyValuePair<long, MapObject> keyValuePair2 in this.keyToMapObject.Dict)
		{
			if (!this.mapObjectsOnMapAlive.Contains(keyValuePair2.Key))
			{
				this.keyToMapObject.MarkToRemove(keyValuePair2.Key);
				this.keyToMapSprite.MarkToRemove(keyValuePair2.Key);
			}
		}
		this.keyToNavObject.RemoveAllMarked(delegate(int _key)
		{
			this.keyToNavObject.Remove(_key);
		});
		this.keyToNavSprite.RemoveAllMarked(delegate(int _key)
		{
			UnityEngine.Object.Destroy(this.keyToNavSprite[_key]);
			this.keyToNavSprite.Remove(_key);
		});
		this.keyToMapObject.RemoveAllMarked(delegate(long _key)
		{
			this.keyToMapObject.Remove(_key);
		});
		this.keyToMapSprite.RemoveAllMarked(delegate(long _key)
		{
			UnityEngine.Object.Destroy(this.keyToMapSprite[_key]);
			this.keyToMapSprite.Remove(_key);
		});
		this.localPlayer.selectedSpawnPointKey = -1L;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateWaypointSelection()
	{
		Vector2 screenPosition = base.xui.playerUI.CursorController.GetScreenPosition();
		GameObject y = null;
		float num = float.MaxValue;
		this.closestMouseOverNavObject = null;
		foreach (NavObject navObject in NavObjectManager.Instance.NavObjectList)
		{
			if (navObject != null && navObject.NavObjectClass != null && (!(navObject.TrackedEntity != null) || navObject.TrackedEntity.entityId != GameManager.Instance.World.GetPrimaryPlayerId()))
			{
				GameObject gameObject = this.keyToNavSprite[navObject.Key];
				if (gameObject != null)
				{
					Vector3 b = base.xui.playerUI.uiCamera.cachedCamera.WorldToScreenPoint(gameObject.transform.position);
					float num2 = Vector3.Distance(screenPosition, b);
					if (num2 <= 20f && (this.closestMouseOverNavObject == null || (this.closestMouseOverNavObject != null && num2 < num)))
					{
						this.closestMouseOverNavObject = navObject;
						num = num2;
						y = gameObject;
					}
				}
			}
		}
		if (this.closestMouseOverNavObject != null)
		{
			if (this.selectMapSprite != y)
			{
				if (this.selectMapSprite != null)
				{
					this.selectMapSprite.transform.localScale = Vector3.one;
				}
				this.selectMapSprite = y;
				this.selectMapSprite.transform.localScale = Vector3.one * 1.5f;
				return;
			}
		}
		else if (this.selectMapSprite != null)
		{
			this.selectMapSprite.transform.localScale = Vector3.one;
			this.selectMapSprite = null;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 worldPosToScreenPos(Vector3 _worldPos)
	{
		return new Vector3((_worldPos.x - this.mapMiddlePosPixel.x) * 2.11904764f / this.zoomScale + (float)this.cTexMiddle.x, (_worldPos.z - this.mapMiddlePosPixel.y) * 2.11904764f / this.zoomScale - (float)this.cTexMiddle.y, 0f);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 mousePosToWindowPos(Vector3 _mousePos)
	{
		Vector2i mouseXUIPosition = base.xui.GetMouseXUIPosition();
		Vector3 vector = new Vector3((float)mouseXUIPosition.x, (float)mouseXUIPosition.y, 0f);
		vector.x += 217f;
		vector.y -= 362f;
		vector.y = -vector.y;
		return vector * 0.9493333f;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 screenPosToWorldPos(Vector3 _mousePos, bool needY = false)
	{
		Vector3 vector = _mousePos;
		Vector3 vector2 = base.xui.playerUI.camera.WorldToScreenPoint(this.xuiTexture.UiTransform.position);
		vector.x -= vector2.x;
		vector.y -= vector2.y;
		vector.y *= -1f;
		Bounds xuiwindowScreenBounds = base.xui.GetXUIWindowScreenBounds(this.xuiTexture.UiTransform, false);
		Vector3 vector3 = xuiwindowScreenBounds.max - xuiwindowScreenBounds.min;
		float num = vector3.x / 336f;
		float num2 = (vector.x - vector3.x / 2f) / num * this.zoomScale + this.mapMiddlePosPixel.x;
		float num3 = -(vector.y - vector3.y / 2f) / num * this.zoomScale + this.mapMiddlePosPixel.y;
		float y = 0f;
		if (needY)
		{
			y = GameManager.Instance.World.GetHeightAt(num2, num3);
		}
		return new Vector3(num2, y, num3);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void teleportPlayerOnMap(Vector3 _screenPosition)
	{
		Vector3 vector = this.screenPosToWorldPos(_screenPosition, false);
		this.localPlayer.Teleport(new Vector3(vector.x, 180f, vector.z), float.MinValue);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void onMapPressedLeft(XUiController _sender, int _mouseButton)
	{
		this.closeAllPopups();
		if (this.closestMouseOverNavObject == null)
		{
			if (PlatformManager.NativePlatform.Input.CurrentInputStyle != PlayerInputManager.InputStyle.Keyboard)
			{
				this.OpenWaypointPopup();
			}
			return;
		}
		this.closestMouseOverNavObject.hiddenOnCompass = !this.closestMouseOverNavObject.hiddenOnCompass;
		if (this.closestMouseOverNavObject.hiddenOnCompass)
		{
			GameManager.ShowTooltip(GameManager.Instance.World.GetPrimaryPlayer(), Localization.Get("compassWaypointHiddenTooltip", false), false);
		}
		if (this.closestMouseOverNavObject.NavObjectClass.NavObjectClassName == "quick_waypoint")
		{
			base.xui.playerUI.entityPlayer.navMarkerHidden = this.closestMouseOverNavObject.hiddenOnCompass;
			return;
		}
		Waypoint waypointForNavObject = base.xui.playerUI.entityPlayer.Waypoints.GetWaypointForNavObject(this.closestMouseOverNavObject);
		if (waypointForNavObject != null)
		{
			waypointForNavObject.hiddenOnCompass = this.closestMouseOverNavObject.hiddenOnCompass;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void onMapPressed(XUiController _sender, int _mouseButton)
	{
		if (PlatformManager.NativePlatform.Input.CurrentInputStyle != PlayerInputManager.InputStyle.Keyboard)
		{
			return;
		}
		this.closeAllPopups();
		UICamera uiCamera = base.xui.playerUI.uiCamera;
		if (InputUtils.ControlKeyPressed)
		{
			if (GameStats.GetBool(EnumGameStats.IsTeleportEnabled) || GamePrefs.GetBool(EnumGamePrefs.DebugMenuEnabled))
			{
				this.teleportPlayerOnMap(base.xui.playerUI.CursorController.GetScreenPosition());
				return;
			}
		}
		else
		{
			this.OpenWaypointPopup();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OpenWaypointPopup()
	{
		this.nextMarkerMousePosition = base.xui.playerUI.CursorController.GetScreenPosition();
		base.xui.GetWindow("mapAreaChooseWaypoint").IsVisible = false;
		XUiV_Window window = base.xui.GetWindow("mapAreaSetWaypoint");
		window.Position = base.xui.GetMouseXUIPosition();
		window.IsVisible = true;
		base.xui.playerUI.CursorController.SetNavigationTargetLater(window.Controller.GetChildById("opt1").ViewComponent);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void onMapHover(XUiController _sender, bool _isOver)
	{
		this.bMouseOverMap = _isOver;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void onPlayerIconPressed(XUiController _sender, int _mouseButton)
	{
		this.PositionMapAt(this.localPlayer.GetPosition());
		this.closeAllPopups();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void onBedrollIconPressed(XUiController _sender, int _mouseButton)
	{
		if (this.localPlayer.SpawnPoints.Count == 0)
		{
			return;
		}
		this.PositionMapAt(this.localPlayer.SpawnPoints[0].ToVector3());
		this.closeAllPopups();
	}

	public void OnSetWaypoint()
	{
		this.localPlayer.navMarkerHidden = false;
		this.localPlayer.markerPosition = World.worldToBlockPos(this.screenPosToWorldPos(this.nextMarkerMousePosition, true));
		this.closeAllPopups();
	}

	public void OnWaypointEntryChosen(string _iconName)
	{
		this.currentWaypointIconChosen = _iconName;
	}

	public void OnWaypointCreated(string _name)
	{
		Waypoint w = new Waypoint();
		w.pos = World.worldToBlockPos(this.screenPosToWorldPos(this.nextMarkerMousePosition, true));
		w.icon = this.currentWaypointIconChosen;
		w.name.Update(_name, PlatformManager.MultiPlatform.User.PlatformUserId);
		base.xui.playerUI.entityPlayer.Waypoints.Collection.Add(w);
		this.closeAllPopups();
		((XUiC_MapWaypointList)base.Parent.GetChildById("waypointList")).UpdateWaypointsList(null);
		w.navObject = NavObjectManager.Instance.RegisterNavObject("waypoint", w.pos.ToVector3(), w.icon, false, null);
		w.navObject.IsActive = false;
		GeneratedTextManager.GetDisplayText(w.name, delegate(string _filtered)
		{
			w.navObject.name = _filtered;
		}, true, false, GeneratedTextManager.TextFilteringMode.FilterWithSafeString, GeneratedTextManager.BbCodeSupportMode.SupportedAndAddEscapes);
		w.navObject.usingLocalizationId = w.bUsingLocalizationId;
		this.selectWaypoint(w);
		Manager.PlayInsidePlayerHead("ui_waypoint_add", -1, 0f, false, false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CbxStaticMapType_OnValueChanged(XUiController _sender, XUiC_MapArea.EStaticMapOverlay _oldvalue, XUiC_MapArea.EStaticMapOverlay _newvalue)
	{
		this.staticWorldTexture = null;
	}

	public void CreateVehicleLastKnownWaypoint(EntityVehicle _vehicle)
	{
		Waypoint waypoint = new Waypoint();
		waypoint.pos = World.worldToBlockPos(_vehicle.position);
		waypoint.icon = _vehicle.GetMapIcon();
		waypoint.ownerId = _vehicle.GetVehicle().OwnerId;
		waypoint.name.Update(Localization.Get(_vehicle.EntityName, false), PlatformManager.MultiPlatform.User.PlatformUserId);
		waypoint.entityId = _vehicle.entityId;
		EntityPlayer entityPlayer = base.xui.playerUI.entityPlayer;
		if (!entityPlayer.Waypoints.ContainsWaypoint(waypoint))
		{
			entityPlayer.Waypoints.Collection.Add(waypoint);
			if (waypoint.CanBeViewedBy(PlatformManager.InternalLocalUserIdentifier))
			{
				((XUiC_MapWaypointList)base.Parent.GetChildById("waypointList")).UpdateWaypointsList(null);
				waypoint.navObject = NavObjectManager.Instance.RegisterNavObject("waypoint", waypoint.pos.ToVector3(), waypoint.icon, false, null);
				waypoint.navObject.IsActive = false;
				waypoint.navObject.OverrideSpriteName = _vehicle.GetMapIcon();
				waypoint.navObject.name = waypoint.name.Text;
				waypoint.navObject.usingLocalizationId = waypoint.bUsingLocalizationId;
			}
		}
	}

	public void RemoveVehicleLastKnownWaypoint(EntityVehicle _vehicle)
	{
		EntityPlayer entityPlayer = base.xui.playerUI.entityPlayer;
		Waypoint entityVehicleWaypoint = entityPlayer.Waypoints.GetEntityVehicleWaypoint(_vehicle.entityId);
		if (entityVehicleWaypoint != null)
		{
			entityPlayer.Waypoints.Collection.Remove(entityVehicleWaypoint);
			NavObjectManager.Instance.UnRegisterNavObject(entityVehicleWaypoint.navObject);
			((XUiC_MapWaypointList)base.Parent.GetChildById("waypointList")).UpdateWaypointsList(null);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void onWaypointIconPressed(XUiController _sender, int _mouseButton)
	{
		EntityPlayerLocal entityPlayer = base.xui.playerUI.entityPlayer;
		if (this.localPlayer.markerPosition == Vector3i.zero)
		{
			Manager.PlayInsidePlayerHead("ui_denied", -1, 0f, false, false);
		}
		else
		{
			Manager.PlayInsidePlayerHead("ui_waypoint_delete", -1, 0f, false, false);
		}
		this.localPlayer.markerPosition = Vector3i.zero;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void selectWaypoint(Waypoint _w)
	{
		((XUiC_MapWaypointList)base.Parent.GetChildById("waypointList")).SelectWaypoint(_w);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void selectWaypoint(NavObject _nav)
	{
		((XUiC_MapWaypointList)base.Parent.GetChildById("waypointList")).SelectWaypoint(_nav);
	}

	public void closeAllPopups()
	{
		base.xui.GetWindow("mapAreaSetWaypoint").IsVisible = false;
		base.xui.GetWindow("mapAreaChooseWaypoint").IsVisible = false;
		base.xui.GetWindow("mapAreaEnterWaypointName").IsVisible = false;
		base.xui.GetWindow("mapTrackingPopup").IsVisible = false;
	}

	public void PositionMapAt(Vector3 _worldPos)
	{
		int num = (int)_worldPos.x;
		int num2 = (int)_worldPos.z;
		this.mapMiddlePosChunks = new Vector2((float)(World.toChunkXZ(num - 1024) * 16 + 1024), (float)(World.toChunkXZ(num2 - 1024) * 16 + 1024));
		this.mapMiddlePosPixel = this.mapMiddlePosChunks;
		this.mapMiddlePosPixel = GameManager.Instance.World.ClampToValidWorldPosForMap(this.mapMiddlePosPixel);
		this.updateFullMap();
	}

	public override void Cleanup()
	{
		base.Cleanup();
		UnityEngine.Object.Destroy(this.mapTexture);
	}

	public const int MapDrawnSizeInChunks = 128;

	public const int MapDrawnSize = 2048;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cMinZoomScale = 0.7f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cMaxZoomScale = 6.15f;

	[PublicizedFrom(EAccessModifier.Private)]
	public int cSpriteScale = 50;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int MapOnScreenSize = 712;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2i cTexMiddle = new Vector2i(356, 356);

	[PublicizedFrom(EAccessModifier.Private)]
	public const int MapSizeFull = 2048;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int MapSizeZoom1 = 336;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float factorScreenSizeToDTM = 2.11904764f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float dragFactorSizeOfMap = 0.471910119f;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool showStaticData;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color32[] staticWorldTexture;

	[PublicizedFrom(EAccessModifier.Private)]
	public int staticWorldWidth;

	[PublicizedFrom(EAccessModifier.Private)]
	public int staticWorldHeight;

	[PublicizedFrom(EAccessModifier.Private)]
	public int staticMapLeft;

	[PublicizedFrom(EAccessModifier.Private)]
	public int staticMapRight;

	[PublicizedFrom(EAccessModifier.Private)]
	public int staticMapBottom;

	[PublicizedFrom(EAccessModifier.Private)]
	public int staticMapTop;

	[PublicizedFrom(EAccessModifier.Private)]
	public Texture2D mapTexture;

	[PublicizedFrom(EAccessModifier.Private)]
	public const byte mapMaskTransparency = 255;

	[PublicizedFrom(EAccessModifier.Private)]
	public byte[][] fowChunkMaskAlphas = new byte[13][];

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2 mapScrollTextureOffset;

	[PublicizedFrom(EAccessModifier.Private)]
	public int mapScrollTextureChunksOffsetX;

	[PublicizedFrom(EAccessModifier.Private)]
	public int mapScrollTextureChunksOffsetZ;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool existingWaypointsInitialized;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bMapInitialized;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bShouldRedrawMap;

	[PublicizedFrom(EAccessModifier.Private)]
	public float timeToRedrawMap;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bFowMaskEnabled;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2 mapMiddlePosChunks;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2 mapMiddlePosPixel;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2 mapMiddlePosChunksToServer;

	[PublicizedFrom(EAccessModifier.Private)]
	public float zoomScale;

	[PublicizedFrom(EAccessModifier.Private)]
	public float targetZoomScale;

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityPlayer localPlayer;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Texture xuiTexture;

	[PublicizedFrom(EAccessModifier.Private)]
	public Color32 defaultColor = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);

	[PublicizedFrom(EAccessModifier.Private)]
	public Color32 hoverColor = new Color32(222, 206, 163, byte.MaxValue);

	[PublicizedFrom(EAccessModifier.Private)]
	public Color32 disabledColor = new Color32(96, 96, 96, byte.MaxValue);

	[PublicizedFrom(EAccessModifier.Private)]
	public DictionarySave<long, MapObject> keyToMapObject = new DictionarySave<long, MapObject>();

	[PublicizedFrom(EAccessModifier.Private)]
	public DictionarySave<int, NavObject> keyToNavObject = new DictionarySave<int, NavObject>();

	[PublicizedFrom(EAccessModifier.Private)]
	public DictionarySave<int, GameObject> keyToNavSprite = new DictionarySave<int, GameObject>();

	[PublicizedFrom(EAccessModifier.Private)]
	public DictionarySave<long, GameObject> keyToMapSprite = new DictionarySave<long, GameObject>();

	[PublicizedFrom(EAccessModifier.Private)]
	public HashSetLong navObjectsOnMapAlive = new HashSetLong();

	[PublicizedFrom(EAccessModifier.Private)]
	public HashSetLong mapObjectsOnMapAlive = new HashSetLong();

	[PublicizedFrom(EAccessModifier.Private)]
	public NavObject closestMouseOverNavObject;

	[PublicizedFrom(EAccessModifier.Private)]
	public GameObject prefabMapSprite;

	[PublicizedFrom(EAccessModifier.Private)]
	public GameObject prefabMapSpriteStartPoint;

	[PublicizedFrom(EAccessModifier.Private)]
	public GameObject prefabMapSpritePrefab;

	[PublicizedFrom(EAccessModifier.Private)]
	public GameObject prefabMapSpriteEntitySpawner;

	[PublicizedFrom(EAccessModifier.Private)]
	public Transform transformSpritesParent;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label uiLblPlayerPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label uiLblBedrollPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label uiLblCursorPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label uiLblMapMarkerDistance;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label uiLblMapMarkerInfo;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Button switchStaticMap;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxEnum<XUiC_MapArea.EStaticMapOverlay> cbxStaticMapType;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController mapView;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Sprite crosshair;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bMouseOverMap;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 nextMarkerMousePosition = Vector3.zero;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isOpen;

	[PublicizedFrom(EAccessModifier.Private)]
	public string kilometers;

	[PublicizedFrom(EAccessModifier.Private)]
	public float mapScale = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2 mapPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2 mapBGPos;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float ZOOM_SPEED = 5f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const KeyCode dragKey = KeyCode.Mouse0;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int MaxMapSymbolSize = 100;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int MinMapSymbolSize = 9;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float mapSpriteMouseOverDistance = 20f;

	[PublicizedFrom(EAccessModifier.Private)]
	public GameObject selectMapSprite;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bMapCursorSet;

	[PublicizedFrom(EAccessModifier.Private)]
	public string currentWaypointIconChosen;

	public enum EStaticMapOverlay
	{
		None,
		Radiation,
		Biomes
	}
}
