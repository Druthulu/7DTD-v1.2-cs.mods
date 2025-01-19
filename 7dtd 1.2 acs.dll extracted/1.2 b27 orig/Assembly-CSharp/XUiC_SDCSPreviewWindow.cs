using System;
using HorizonBasedAmbientOcclusion;
using PI.NGSS;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
[PublicizedFrom(EAccessModifier.Internal)]
public class XUiC_SDCSPreviewWindow : XUiController
{
	public override void Init()
	{
		base.Init();
		this.textPreview = (XUiV_Texture)base.GetChildById("playerPreview").ViewComponent;
		this.textPreview.UpdateData();
		this.RenderTextureSystem = new RenderTextureSystem();
		this.zoomButton = base.GetChildById("zoomButton");
		if (this.zoomButton != null)
		{
			this.zoomButton.OnPress += this.ZoomButton_OnPress;
		}
		base.RegisterForInputStyleChanges();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ZoomButton_OnPress(XUiController _sender, int _mouseButton)
	{
		this.toggleHeadZoom();
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.originalPixelLightCount = QualitySettings.pixelLightCount;
		QualitySettings.pixelLightCount = 4;
		if (this.zoomButton != null)
		{
			this.zoomButton.ViewComponent.IsVisible = (PlatformManager.NativePlatform.Input.CurrentInputStyle == PlayerInputManager.InputStyle.Keyboard);
		}
	}

	public override void OnClose()
	{
		base.OnClose();
		SDCSUtils.DestroyViz(this.previewTransform, false);
		this.RenderTextureSystem.Cleanup();
		this.lastProfile = "";
		this.lastFieldOfView = 54f;
		QualitySettings.pixelLightCount = this.originalPixelLightCount;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		string text = ProfileSDF.CurrentProfileName();
		if (text != this.lastProfile)
		{
			this.Archetype = Archetype.GetArchetype(text);
			if (this.Archetype == null)
			{
				this.Archetype = ProfileSDF.CreateTempArchetype(text);
			}
			this.MakePreview();
			this.lastProfile = text;
			if (this.canZoom)
			{
				this.state = XUiC_SDCSPreviewWindow.ZoomStates.Head;
				this.SetToHeadZoom();
			}
		}
		if (PlatformManager.NativePlatform.Input.CurrentInputStyle != PlayerInputManager.InputStyle.Keyboard)
		{
			this.UpdateController();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void InputStyleChanged(PlayerInputManager.InputStyle _oldStyle, PlayerInputManager.InputStyle _newStyle)
	{
		base.InputStyleChanged(_oldStyle, _newStyle);
		if (this.zoomButton != null)
		{
			this.zoomButton.ViewComponent.IsVisible = (_newStyle == PlayerInputManager.InputStyle.Keyboard);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void UpdateController()
	{
		float value = base.xui.playerUI.playerInput.GUIActions.TriggerAxis.Value;
		if (value != 0f)
		{
			this.CameraRotate(-value);
		}
		if (base.xui.playerUI.playerInput.GUIActions.HalfStack.WasPressed)
		{
			this.toggleHeadZoom();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnDragged(EDragType _dragType, Vector2 _mousePositionDelta)
	{
		base.OnDragged(_dragType, _mousePositionDelta);
		float x = _mousePositionDelta.x;
		if (base.xui.playerUI.CursorController.GetMouseButton(UICamera.MouseButton.RightButton))
		{
			this.CameraRotate(x);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CameraVerticalPan(float _value)
	{
		this.RenderCamera.transform.localPosition -= new Vector3(0f, _value, 0f);
		if (this.RenderCamera.transform.localPosition.y < -1.5f)
		{
			this.RenderCamera.transform.localPosition = new Vector3(this.RenderCamera.transform.localPosition.x, -1.5f, this.RenderCamera.transform.localPosition.z);
			return;
		}
		if (this.RenderCamera.transform.localPosition.y > 0f)
		{
			this.RenderCamera.transform.localPosition = new Vector3(this.RenderCamera.transform.localPosition.x, 0f, this.RenderCamera.transform.localPosition.z);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CameraRotate(float _value)
	{
		this.RenderCamera.transform.RotateAround(this.previewTransform.transform.position, Vector3.up, _value);
		this.RenderTextureSystem.LightGO.transform.RotateAround(this.previewTransform.transform.position, Vector3.up, _value);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnScrolled(float _delta)
	{
		base.OnScrolled(_delta);
	}

	public void MakePreview()
	{
		SDCSUtils.CreateVizUI(this.Archetype, ref this.previewTransform, ref this.uiBoneCatalog);
		this.previewTransform.GetComponentInChildren<Animator>().Update(0f);
		this.init();
		this.previewTransform.transform.parent = this.RenderTextureSystem.TargetGO.transform;
		this.previewTransform.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
		this.previewTransform.transform.localPosition = new Vector3(0f, -0.9f, 0f);
		this.characterGazeController = this.previewTransform.GetComponentInChildren<CharacterGazeController>();
		Utils.SetLayerRecursively(this.RenderTextureSystem.TargetGO, 11, null);
		this.textPreview.Texture = this.RenderTexture;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void toggleHeadZoom()
	{
		switch (this.state)
		{
		case XUiC_SDCSPreviewWindow.ZoomStates.Eyes:
			this.state = XUiC_SDCSPreviewWindow.ZoomStates.Head;
			break;
		case XUiC_SDCSPreviewWindow.ZoomStates.Head:
			this.state = XUiC_SDCSPreviewWindow.ZoomStates.Chest;
			break;
		case XUiC_SDCSPreviewWindow.ZoomStates.Chest:
			this.state = XUiC_SDCSPreviewWindow.ZoomStates.FullBody;
			break;
		case XUiC_SDCSPreviewWindow.ZoomStates.FullBody:
			this.state = XUiC_SDCSPreviewWindow.ZoomStates.Eyes;
			break;
		}
		switch (this.state)
		{
		case XUiC_SDCSPreviewWindow.ZoomStates.Eyes:
			this.SetToEyeZoom();
			return;
		case XUiC_SDCSPreviewWindow.ZoomStates.Head:
			this.SetToHeadZoom();
			return;
		case XUiC_SDCSPreviewWindow.ZoomStates.Chest:
			this.SetToChestZoom();
			return;
		case XUiC_SDCSPreviewWindow.ZoomStates.FullBody:
			this.SetToFullBodyZoom();
			return;
		default:
			return;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetToFullBodyZoom()
	{
		this.RenderCamera.transform.SetLocalPositionAndRotation(this.originalCamPosition, Quaternion.AngleAxis(19f, new Vector3(1f, 0f, 0f)) * this.originalCamRotation);
		this.RenderTextureSystem.LightGO.transform.SetLocalPositionAndRotation(this.originalLightPosition, this.originalLightRotation);
		this.RenderCamera.fieldOfView = 54f;
		this.SetCameraInitialRotationOffset();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetToHeadZoom()
	{
		this.RenderCamera.transform.SetLocalPositionAndRotation(this.originalCamPosition, Quaternion.AngleAxis(1.5f, new Vector3(1f, 0f, 0f)) * this.originalCamRotation);
		this.RenderTextureSystem.LightGO.transform.SetLocalPositionAndRotation(this.originalLightPosition, this.originalLightRotation);
		this.RenderCamera.fieldOfView = 12f;
		this.RenderTextureSystem.TargetGO.transform.localPosition = new Vector3(0.015f, -0.78f, 2.14f);
		this.SetCameraInitialRotationOffset();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetToChestZoom()
	{
		this.RenderCamera.transform.SetLocalPositionAndRotation(this.originalCamPosition, Quaternion.AngleAxis(5f, new Vector3(1f, 0f, 0f)) * this.originalCamRotation);
		this.RenderTextureSystem.LightGO.transform.SetLocalPositionAndRotation(this.originalLightPosition, this.originalLightRotation);
		this.RenderCamera.fieldOfView = 20f;
		this.RenderTextureSystem.TargetGO.transform.localPosition = new Vector3(0.02f, -0.78f, 2.14f);
		this.SetCameraInitialRotationOffset();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetToEyeZoom()
	{
		this.RenderCamera.transform.SetLocalPositionAndRotation(this.originalCamPosition, this.originalCamRotation);
		this.RenderTextureSystem.LightGO.transform.SetLocalPositionAndRotation(this.originalLightPosition, this.originalLightRotation);
		this.RenderCamera.fieldOfView = 6f;
		this.RenderTextureSystem.TargetGO.transform.localPosition = new Vector3(0.015f, -0.78f, 2.14f);
		this.SetCameraInitialRotationOffset();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetCameraInitialRotationOffset()
	{
		this.RenderCamera.transform.RotateAround(this.previewTransform.transform.position, Vector3.up, -30f);
		this.RenderTextureSystem.LightGO.transform.RotateAround(this.previewTransform.transform.position, Vector3.up, -30f);
		if (this.characterGazeController != null)
		{
			this.characterGazeController.SnapNextUpdate();
		}
	}

	public void ZoomToHead()
	{
		if (this.state == XUiC_SDCSPreviewWindow.ZoomStates.Head)
		{
			return;
		}
		if (PlatformManager.NativePlatform.Input.CurrentInputStyle != PlayerInputManager.InputStyle.Keyboard && base.xui.playerUI.playerInput.GUIActions.TriggerAxis.Value != 0f)
		{
			return;
		}
		this.state = XUiC_SDCSPreviewWindow.ZoomStates.Head;
		this.SetToHeadZoom();
	}

	public void ZoomToEye()
	{
		if (PlatformManager.NativePlatform.Input.CurrentInputStyle != PlayerInputManager.InputStyle.Keyboard && base.xui.playerUI.playerInput.GUIActions.TriggerAxis.Value != 0f)
		{
			return;
		}
		this.state = XUiC_SDCSPreviewWindow.ZoomStates.Eyes;
		this.SetToEyeZoom();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void init()
	{
		if (this.RenderTextureSystem.ParentGO == null)
		{
			this.RenderTextureSystem.Create("characterpreview", new GameObject(), new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f), this.textPreview.Size, true, true, 1f);
			this.RenderTextureSystem.TargetGO.transform.localPosition = new Vector3(0f, -0.75f, 2.15f);
			this.RenderTexture = this.RenderTextureSystem.RenderTex;
			this.RenderCamera = this.RenderTextureSystem.CameraGO.GetComponent<Camera>();
			this.RenderCamera.orthographic = false;
			this.RenderCamera.backgroundColor = new Color(0f, 0f, 0f, 0f);
			this.RenderCamera.fieldOfView = 54f;
			this.RenderCamera.renderingPath = RenderingPath.DeferredShading;
			this.RenderCamera.tag = "MainCamera";
			this.RenderCamera.gameObject.AddComponent<StreamingController>();
			this.RenderTextureSystem.CameraGO.AddComponent<NGSS_Local>().NGSS_PCSS_SOFTNESS_NEAR = 0.05f;
			HBAO hbao = this.RenderTextureSystem.CameraGO.AddComponent<HBAO>();
			hbao.SetAoPerPixelNormals(HBAO.PerPixelNormals.Reconstruct);
			hbao.SetAoIntensity(0.5f);
			this.RenderTextureSystem.LightGO.GetComponent<Light>().enabled = false;
			GameObject gameObject = new GameObject("Key Light", new Type[]
			{
				typeof(Light)
			});
			gameObject.transform.SetParent(this.RenderTextureSystem.LightGO.transform, false);
			gameObject.transform.SetPositionAndRotation(new Vector3(0.25f, 0.475f, 0.62f), Quaternion.Euler(33f, -8f, 0f));
			gameObject.AddComponent<NGSS_Directional>().NGSS_PCSS_ENABLED = true;
			Light component = gameObject.GetComponent<Light>();
			component.color = new Color(0.9f, 0.8f, 0.7f, 1f);
			component.type = LightType.Spot;
			component.range = 20f;
			component.spotAngle = 60f;
			component.intensity = 1.5f;
			component.shadows = LightShadows.Hard;
			component.shadowStrength = 0.2f;
			component.shadowBias = 0.005f;
			NGSS_FrustumShadows ngss_FrustumShadows = this.RenderTextureSystem.CameraGO.AddComponent<NGSS_FrustumShadows>();
			ngss_FrustumShadows.mainShadowsLight = component;
			ngss_FrustumShadows.m_fastBlur = false;
			ngss_FrustumShadows.m_shadowsBlur = 1f;
			ngss_FrustumShadows.m_shadowsBlurIterations = 4;
			ngss_FrustumShadows.m_rayThickness = 0.025f;
			GameObject gameObject2 = new GameObject("Fill Light", new Type[]
			{
				typeof(Light)
			});
			gameObject2.transform.SetParent(this.RenderTextureSystem.LightGO.transform, false);
			gameObject2.transform.SetPositionAndRotation(new Vector3(-1.15f, 1.4f, 1f), Quaternion.Euler(50f, 45f, 0f));
			Light component2 = gameObject2.GetComponent<Light>();
			component2.color = new Color(1f, 1f, 1f, 1f);
			component2.type = LightType.Spot;
			component2.range = 20f;
			component2.spotAngle = 60f;
			component2.intensity = 0.5f;
			component2.shadows = LightShadows.Hard;
			component2.shadowStrength = 0.2f;
			component2.shadowBias = 0.005f;
			GameObject gameObject3 = new GameObject("Fill 2 Light", new Type[]
			{
				typeof(Light)
			});
			gameObject3.transform.SetParent(this.RenderTextureSystem.LightGO.transform, false);
			gameObject3.transform.SetPositionAndRotation(new Vector3(0f, -1.5f, -0.5f), Quaternion.Euler(-15f, 0f, 0f));
			Light component3 = gameObject3.GetComponent<Light>();
			component3.color = new Color(1f, 1f, 1f, 1f);
			component3.type = LightType.Spot;
			component3.range = 20f;
			component3.spotAngle = 60f;
			component3.intensity = 0.5f;
			component3.shadows = LightShadows.Hard;
			component3.shadowStrength = 0.2f;
			component3.shadowBias = 0.005f;
			GameObject gameObject4 = new GameObject("Back Light", new Type[]
			{
				typeof(Light)
			});
			gameObject4.transform.SetParent(this.RenderTextureSystem.LightGO.transform, false);
			gameObject4.transform.SetPositionAndRotation(new Vector3(-0.6f, 0.75f, 2.6f), Quaternion.Euler(55f, 133f, 0f));
			Light component4 = gameObject4.GetComponent<Light>();
			component4.color = new Color(0.4f, 0.75f, 1f, 1f);
			component4.type = LightType.Spot;
			component4.spotAngle = 60f;
			component4.range = 20f;
			component4.intensity = 1.5f;
			component4.shadows = LightShadows.Hard;
			component4.shadowStrength = 0.2f;
			component4.shadowBias = 0.005f;
			this.originalCamPosition = this.RenderCamera.transform.localPosition;
			this.originalCamRotation = this.RenderCamera.transform.localRotation;
			this.originalLightPosition = this.RenderTextureSystem.LightGO.transform.localPosition;
			this.originalLightRotation = this.RenderTextureSystem.LightGO.transform.localRotation;
			this.RenderCamera.transform.localPosition = new Vector3(0f, -0.75f, 0f);
		}
	}

	public override bool ParseAttribute(string name, string value, XUiController _parent)
	{
		bool flag = base.ParseAttribute(name, value, _parent);
		if (flag)
		{
			return flag;
		}
		if (name == "can_zoom")
		{
			this.canZoom = StringParsers.ParseBool(value, 0, -1, true);
			return true;
		}
		return false;
	}

	public RenderTextureSystem RenderTextureSystem;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Texture textPreview;

	public RenderTexture RenderTexture;

	public Camera RenderCamera;

	public Transform TargetTransform;

	public GameObject RotateTable;

	[PublicizedFrom(EAccessModifier.Private)]
	public GameObject previewTransform;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lastProfile;

	[PublicizedFrom(EAccessModifier.Private)]
	public float lastFieldOfView;

	public Archetype Archetype;

	[PublicizedFrom(EAccessModifier.Private)]
	public CharacterGazeController characterGazeController;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController zoomButton;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 originalCamPosition;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 originalLightPosition;

	[PublicizedFrom(EAccessModifier.Private)]
	public Quaternion originalCamRotation;

	[PublicizedFrom(EAccessModifier.Private)]
	public Quaternion originalLightRotation;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool canZoom = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public int originalPixelLightCount;

	[PublicizedFrom(EAccessModifier.Private)]
	public SDCSUtils.TransformCatalog uiBoneCatalog;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SDCSPreviewWindow.ZoomStates state = XUiC_SDCSPreviewWindow.ZoomStates.FullBody;

	[PublicizedFrom(EAccessModifier.Private)]
	public float baseOrtho = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public enum ZoomStates
	{
		Eyes,
		Head,
		Chest,
		FullBody
	}
}
