using System;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;
using WorldGenerationEngineFinal;

[Preserve]
public class XUiC_WorldGenerationPreview : XUiController
{
	public static WorldBuilder worldBuilder
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			XUiC_WorldGenerationWindowGroup instance = XUiC_WorldGenerationWindowGroup.Instance;
			if (instance == null)
			{
				return null;
			}
			return instance.worldBuilder;
		}
	}

	public override void Init()
	{
		base.Init();
		XUiC_WorldGenerationPreview.Instance = this;
		this.UIPreviewTexture = (XUiV_Texture)base.ViewComponent;
		this.UIWinGroup = (base.WindowGroup.Controller as XUiC_WorldGenerationWindowGroup);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void worldPreview_OnPress(XUiController _sender, int _button)
	{
		this.ResetCamera();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void worldPreview_OnDrag(XUiController _sender, EDragType _dragType, Vector2 _mousePositionDelta)
	{
		if (this.renderTextureSystem != null && Input.GetMouseButton(1))
		{
			Transform transform = this.renderTextureSystem.CameraGO.transform;
			this.cameraRotX += _mousePositionDelta.y * -0.2f;
			this.cameraRotY += _mousePositionDelta.x * 0.2f;
			transform.rotation = Quaternion.Euler(this.cameraRotX, this.cameraRotY, 0f);
		}
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (this.renderTextureSystem == null)
		{
			return;
		}
		Vector3 vector;
		if (PlatformManager.NativePlatform.Input.CurrentInputStyle == PlayerInputManager.InputStyle.Keyboard)
		{
			vector = this.UpdateMovementKeyboard();
		}
		else
		{
			vector = this.UpdateMovementController(_dt);
		}
		vector *= 150f * Time.deltaTime;
		Transform transform = this.renderTextureSystem.CameraGO.transform;
		Vector3 vector2 = transform.position;
		vector2 += transform.forward * vector.z;
		vector2 += transform.right * vector.x;
		vector2 += transform.up * vector.y;
		vector2.y = Utils.FastMax(40f, vector2.y);
		transform.position = vector2;
	}

	public Vector3 UpdateMovementKeyboard()
	{
		Vector3 vector = Vector3.zero;
		if (Input.GetKey(KeyCode.W))
		{
			vector.z = 1f;
		}
		else if (Input.GetKey(KeyCode.S))
		{
			vector.z = -1f;
		}
		if (Input.GetKey(KeyCode.A))
		{
			vector.x = -1f;
		}
		else if (Input.GetKey(KeyCode.D))
		{
			vector.x = 1f;
		}
		if (Input.GetKey(KeyCode.Space))
		{
			vector.y = 1f;
		}
		else if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.C))
		{
			vector.y = -1f;
		}
		if (InputUtils.ShiftKeyPressed)
		{
			vector *= 10f;
		}
		return vector;
	}

	public Vector3 UpdateMovementController(float _dt)
	{
		Vector3 vector = Vector3.zero;
		if (base.xui.playerUI.playerInput.GUIActions.PageUp.IsPressed)
		{
			base.xui.playerUI.CursorController.Locked = true;
			base.xui.playerUI.CursorController.VirtualCursorHidden = true;
			base.xui.calloutWindow.DisableCallouts(XUiC_GamepadCalloutWindow.CalloutType.Menu);
			base.xui.calloutWindow.DisableCallouts(XUiC_GamepadCalloutWindow.CalloutType.RWGEditor);
			base.xui.calloutWindow.EnableCallouts(XUiC_GamepadCalloutWindow.CalloutType.RWGCamera, 0f);
			vector = new Vector3(base.xui.playerUI.playerInput.GUIActions.Look.X, 0f, base.xui.playerUI.playerInput.GUIActions.Look.Y);
			if (base.xui.playerUI.playerInput.GUIActions.PageDown.IsPressed)
			{
				vector *= 10f;
			}
			Transform transform = this.renderTextureSystem.CameraGO.transform;
			this.cameraRotX += base.xui.playerUI.playerInput.GUIActions.Camera.Y * _dt * -165f;
			this.cameraRotY += base.xui.playerUI.playerInput.GUIActions.Camera.X * _dt * 165f;
			transform.rotation = Quaternion.Euler(this.cameraRotX, this.cameraRotY, 0f);
		}
		else
		{
			base.xui.playerUI.CursorController.Locked = false;
			base.xui.playerUI.CursorController.VirtualCursorHidden = false;
			base.xui.calloutWindow.EnableCallouts(XUiC_GamepadCalloutWindow.CalloutType.Menu, 0f);
			base.xui.calloutWindow.EnableCallouts(XUiC_GamepadCalloutWindow.CalloutType.RWGEditor, 0f);
			base.xui.calloutWindow.DisableCallouts(XUiC_GamepadCalloutWindow.CalloutType.RWGCamera);
		}
		return vector;
	}

	public Vector3 GetCameraPosition()
	{
		return this.renderTextureSystem.CameraGO.transform.position;
	}

	public override void OnOpen()
	{
		this.UIPreviewTexture = (XUiV_Texture)base.ViewComponent;
		this.UIWinGroup = (base.WindowGroup.Controller as XUiC_WorldGenerationWindowGroup);
		this.UIPreviewTexture.Controller.OnPress += this.worldPreview_OnPress;
		this.UIPreviewTexture.Controller.OnDrag += this.worldPreview_OnDrag;
		this.initRenderTextureSystem();
		base.OnOpen();
	}

	public override void OnClose()
	{
		this.renderTextureSystem.SetEnabled(false);
		this.destroyRenderTextureSystem();
		this.CleanupTerrainMesh();
		this.destroyPOIPreviews();
		this.UIPreviewTexture.Controller.OnPress -= this.worldPreview_OnPress;
		this.UIPreviewTexture.Controller.OnDrag -= this.worldPreview_OnDrag;
		this.UIPreviewTexture = null;
		base.xui.playerUI.CursorController.Locked = false;
		base.xui.playerUI.CursorController.VirtualCursorHidden = false;
		base.OnClose();
	}

	public void GeneratePreview()
	{
		this.CleanupTerrainMesh();
		if (this.terrainPreviewRootObj && XUiC_WorldGenerationPreview.worldBuilder != null)
		{
			WorldPreviewTerrain.GenerateTerrain(this.terrainPreviewRootObj.transform);
			this.ResetCamera();
		}
	}

	public void CleanupTerrainMesh()
	{
		if (this.terrainPreviewRootObj)
		{
			WorldPreviewTerrain.Cleanup(this.terrainPreviewRootObj);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void destroyPOIPreviews()
	{
		if (this.UIWinGroup != null && this.UIWinGroup.prefabPreviewManager != null)
		{
			this.UIWinGroup.prefabPreviewManager.Cleanup();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void initRenderTextureSystem()
	{
		if (this.renderTextureSystem == null)
		{
			this.renderTextureSystem = new RenderTextureSystem();
			this.terrainPreviewRootObj = new GameObject("TerrainMesh");
			this.renderTextureSystem.Create("worldpreview", this.terrainPreviewRootObj, new Vector3(0f, 0f, 0f), new Vector3(0f, 4000f, 0f), this.UIPreviewTexture.Size, false, false, 1f);
			Camera component = this.renderTextureSystem.CameraGO.transform.GetComponent<Camera>();
			component.nearClipPlane = 0.1f;
			component.farClipPlane = 20000f;
			this.ResetCamera();
			Transform transform = this.renderTextureSystem.LightGO.transform;
			transform.localPosition = new Vector3(0f, 2000f, 0f);
			transform.localRotation = Quaternion.Euler(30f, 0f, 0f);
			transform.GetComponent<Light>().type = LightType.Directional;
			transform.GetComponent<Light>().intensity = 1.2f;
			this.terrainPreviewRootObj.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
			this.terrainPreviewRootObj.transform.position = new Vector3(-10240f, 0f, -10240f);
			this.UIPreviewTexture.Texture = this.renderTextureSystem.RenderTex;
		}
		this.renderTextureSystem.SetEnabled(true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void destroyRenderTextureSystem()
	{
		UnityEngine.Object.Destroy(this.renderTextureSystem.ParentGO);
		this.renderTextureSystem = null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ResetCamera()
	{
		if (this.renderTextureSystem != null && XUiC_WorldGenerationPreview.worldBuilder != null)
		{
			this.cameraRotX = 90f;
			this.cameraRotY = 0f;
			Transform transform = this.renderTextureSystem.CameraGO.transform;
			transform.localPosition = new Vector3(0f, (float)XUiC_WorldGenerationPreview.worldBuilder.WorldSize * 0.8745f, 0f);
			transform.localRotation = Quaternion.Euler(this.cameraRotX, this.cameraRotY, 0f);
		}
	}

	public static XUiC_WorldGenerationPreview Instance;

	[PublicizedFrom(EAccessModifier.Private)]
	public RenderTextureSystem renderTextureSystem;

	[PublicizedFrom(EAccessModifier.Private)]
	public GameObject terrainPreviewRootObj;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Texture UIPreviewTexture;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_WorldGenerationWindowGroup UIWinGroup;

	[PublicizedFrom(EAccessModifier.Private)]
	public float cameraRotX;

	[PublicizedFrom(EAccessModifier.Private)]
	public float cameraRotY;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float currentFlySpeed = 150f;

	public class PrefabNameHandler : MonoBehaviour
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public void Awake()
		{
			this.textMesh = base.transform.GetComponent<TextMesh>();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void OnMouseOver(bool isOver)
		{
			if (this.textMesh != null)
			{
				if (isOver)
				{
					base.gameObject.layer = 11;
					return;
				}
				base.gameObject.layer = 0;
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		[NonSerialized]
		public TextMesh textMesh;
	}
}
