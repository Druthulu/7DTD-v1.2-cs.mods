using System;
using System.Collections;
using System.Globalization;
using UnityEngine;
using UnityEngine.Networking;

public class XUiV_Texture : XUiView
{
	public UITexture UITexture
	{
		get
		{
			return this.uiTexture;
		}
	}

	public Texture Texture
	{
		get
		{
			return this.texture;
		}
		set
		{
			this.texture = value;
			this.isDirty = true;
		}
	}

	public Material Material
	{
		get
		{
			return this.material;
		}
		set
		{
			this.material = value;
			this.isDirty = true;
		}
	}

	public Rect UVRect
	{
		get
		{
			return this.uvRect;
		}
		set
		{
			this.uvRect = value;
			this.isDirty = true;
		}
	}

	public UIBasicSprite.Type Type
	{
		get
		{
			return this.type;
		}
		set
		{
			this.type = value;
			this.isDirty = true;
		}
	}

	public Vector4 Border
	{
		get
		{
			return this.border;
		}
		set
		{
			this.border = value;
			this.isDirty = true;
		}
	}

	public UIBasicSprite.Flip Flip
	{
		get
		{
			return this.flip;
		}
		set
		{
			this.flip = value;
			this.isDirty = true;
		}
	}

	public Color Color
	{
		get
		{
			return this.color;
		}
		set
		{
			this.color = value;
			this.isDirty = true;
		}
	}

	public UIBasicSprite.FillDirection FillDirection
	{
		get
		{
			return this.fillDirection;
		}
		set
		{
			this.fillDirection = value;
			this.isDirty = true;
		}
	}

	public bool FillCenter
	{
		get
		{
			return this.fillCenter;
		}
		set
		{
			this.fillCenter = value;
			this.isDirty = true;
		}
	}

	public float GlobalOpacityModifier
	{
		get
		{
			return this.globalOpacityModifier;
		}
		set
		{
			this.globalOpacityModifier = value;
			this.isDirty = true;
		}
	}

	public bool OriginalAspectRatio
	{
		get
		{
			return this.originalAspectRatio;
		}
		set
		{
			this.originalAspectRatio = value;
			this.isDirty = true;
		}
	}

	public XUiV_Texture(string _id) : base(_id)
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void CreateComponents(GameObject _go)
	{
		_go.AddComponent<UITexture>();
	}

	public void CreateMaterial()
	{
		this.Material = new Material(Shader.Find("Unlit/Transparent Colored Emissive TextureArray"));
		this.isCreatedMaterial = true;
	}

	public override void InitView()
	{
		base.InitView();
		this.uiTexture = this.uiTransform.gameObject.GetComponent<UITexture>();
	}

	public override void Cleanup()
	{
		base.Cleanup();
		if (this.isCreatedMaterial)
		{
			UnityEngine.Object.Destroy(this.material);
			this.material = null;
			this.isCreatedMaterial = false;
		}
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (base.xui.GlobalOpacityChanged)
		{
			this.isDirty = true;
		}
	}

	public override void UpdateData()
	{
		if (!this.wwwAssigned && !string.IsNullOrEmpty(this.pathName) && this.pathName.Contains("@"))
		{
			if (!this.www.isDone)
			{
				return;
			}
			if (this.www.result == UnityWebRequest.Result.Success)
			{
				Texture2D texture2D = ((DownloadHandlerTexture)this.www.downloadHandler).texture;
				this.Texture = TextureUtils.CloneTexture(texture2D, false, false, true);
				UnityEngine.Object.DestroyImmediate(texture2D);
			}
			else
			{
				Log.Warning("Retrieving XUiV_Texture file from '" + this.pathName + "' failed: " + this.www.error);
			}
			this.wwwAssigned = true;
		}
		if (!this.isDirty)
		{
			return;
		}
		this.uiTexture.enabled = (this.texture != null);
		this.uiTexture.mainTexture = this.texture;
		this.uiTexture.color = this.color;
		this.uiTexture.keepAspectRatio = this.keepAspectRatio;
		this.uiTexture.aspectRatio = this.aspectRatio;
		this.uiTexture.fixedAspect = this.originalAspectRatio;
		this.uiTexture.SetDimensions(this.size.x, this.size.y);
		this.uiTexture.type = this.type;
		this.uiTexture.border = this.border;
		this.uiTexture.uvRect = this.uvRect;
		this.uiTexture.flip = this.flip;
		this.uiTexture.centerType = (this.fillCenter ? UIBasicSprite.AdvancedType.Sliced : UIBasicSprite.AdvancedType.Invisible);
		this.uiTexture.fillDirection = this.fillDirection;
		this.uiTexture.material = this.material;
		if (this.globalOpacityModifier != 0f && base.xui.ForegroundGlobalOpacity < 1f)
		{
			float a = Mathf.Clamp01(this.color.a * (this.globalOpacityModifier * base.xui.ForegroundGlobalOpacity));
			this.uiTexture.color = new Color(this.color.r, this.color.g, this.color.b, a);
		}
		if (!this.initialized)
		{
			this.uiTexture.pivot = this.pivot;
			this.uiTexture.depth = this.depth;
			this.uiTransform.localScale = Vector3.one;
			this.uiTransform.localPosition = new Vector3((float)this.position.x, (float)this.position.y, 0f);
			if (this.EventOnHover || this.EventOnPress || this.EventOnScroll || this.EventOnDrag)
			{
				BoxCollider collider = this.collider;
				collider.center = this.uiTexture.localCenter;
				collider.size = new Vector3(this.uiTexture.localSize.x * this.colliderScale, this.uiTexture.localSize.y * this.colliderScale, 0f);
			}
		}
		base.parseAnchors(this.uiTexture, true);
		base.UpdateData();
	}

	public void UnloadTexture()
	{
		if (this.Texture == null)
		{
			return;
		}
		Texture assetToUnload = this.Texture;
		this.uiTexture.mainTexture = null;
		this.Texture = null;
		this.pathName = null;
		this.wwwAssigned = false;
		if (this.www == null)
		{
			Resources.UnloadAsset(assetToUnload);
		}
		this.www = null;
	}

	public override bool ParseAttribute(string attribute, string value, XUiController _parent)
	{
		uint num = <PrivateImplementationDetails>.ComputeStringHash(attribute);
		if (num <= 2672033115U)
		{
			if (num <= 1031692888U)
			{
				if (num != 1013213428U)
				{
					if (num == 1031692888U)
					{
						if (attribute == "color")
						{
							this.Color = StringParsers.ParseColor32(value);
							return true;
						}
					}
				}
				else if (attribute == "texture")
				{
					if (this.pathName == value)
					{
						return true;
					}
					this.pathName = value;
					try
					{
						this.wwwAssigned = false;
						string text = ModManager.PatchModPathString(this.pathName);
						if (text != null)
						{
							this.fetchWwwTexture("file://" + text);
						}
						else if (this.pathName[0] == '@')
						{
							string text2 = this.pathName.Substring(1);
							if (text2.StartsWith("file:", StringComparison.OrdinalIgnoreCase))
							{
								string text3 = text2.Substring(5);
								if (text3[0] != '/' && text3[0] != '\\')
								{
									text2 = new Uri(((Application.platform == RuntimePlatform.OSXPlayer || Application.platform == RuntimePlatform.OSXServer) ? (Application.dataPath + "/../../") : (Application.dataPath + "/../")) + text3).AbsoluteUri;
								}
							}
							this.fetchWwwTexture(text2);
						}
						else
						{
							base.xui.LoadData<Texture>(this.pathName, delegate(Texture o)
							{
								this.Texture = o;
							});
						}
					}
					catch (Exception e)
					{
						Log.Error("[XUi] Could not load texture: " + this.pathName);
						Log.Exception(e);
					}
					return true;
				}
			}
			else if (num != 1361572173U)
			{
				if (num == 2672033115U)
				{
					if (attribute == "rect_offset")
					{
						Vector2 vector = StringParsers.ParseVector2(value);
						Rect uvrect = this.uvRect;
						uvrect.x = vector.x;
						uvrect.y = vector.y;
						this.UVRect = uvrect;
						return true;
					}
				}
			}
			else if (attribute == "type")
			{
				this.type = EnumUtils.Parse<UIBasicSprite.Type>(value, true);
				return true;
			}
		}
		else if (num <= 3060355671U)
		{
			if (num != 3007493977U)
			{
				if (num == 3060355671U)
				{
					if (attribute == "globalopacity")
					{
						if (!StringParsers.ParseBool(value, 0, -1, true))
						{
							this.GlobalOpacityModifier = 0f;
						}
						return true;
					}
				}
			}
			else if (attribute == "rect_size")
			{
				Vector2 vector2 = StringParsers.ParseVector2(value);
				Rect uvrect2 = this.uvRect;
				uvrect2.width = vector2.x;
				uvrect2.height = vector2.y;
				this.UVRect = uvrect2;
				return true;
			}
		}
		else if (num != 3538210912U)
		{
			if (num != 4072220735U)
			{
				if (num == 4144336821U)
				{
					if (attribute == "globalopacitymod")
					{
						this.GlobalOpacityModifier = StringParsers.ParseFloat(value, 0, -1, NumberStyles.Any);
						return true;
					}
				}
			}
			else if (attribute == "original_aspect_ratio")
			{
				this.OriginalAspectRatio = StringParsers.ParseBool(value, 0, -1, true);
				return true;
			}
		}
		else if (attribute == "material")
		{
			base.xui.LoadData<Material>(value, delegate(Material o)
			{
				this.material = new Material(o);
			});
			return true;
		}
		return base.ParseAttribute(attribute, value, _parent);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void fetchWwwTexture(string _uri)
	{
		_uri = _uri.Replace("#", "%23").Replace("+", "%2B");
		this.www = UnityWebRequestTexture.GetTexture(_uri);
		this.www.SendWebRequest();
		ThreadManager.StartCoroutine(this.waitForWwwData());
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator waitForWwwData()
	{
		while (this.www != null && !this.www.isDone)
		{
			yield return null;
		}
		if (this.www != null)
		{
			this.isDirty = true;
		}
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public UITexture uiTexture;

	[PublicizedFrom(EAccessModifier.Protected)]
	public Texture texture;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string pathName;

	[PublicizedFrom(EAccessModifier.Protected)]
	public Material material;

	[PublicizedFrom(EAccessModifier.Protected)]
	public Rect uvRect = new Rect(0f, 0f, 1f, 1f);

	[PublicizedFrom(EAccessModifier.Protected)]
	public UIBasicSprite.Type type;

	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector4 border = Vector4.zero;

	[PublicizedFrom(EAccessModifier.Protected)]
	public UIBasicSprite.Flip flip;

	[PublicizedFrom(EAccessModifier.Protected)]
	public Color color = Color.white;

	[PublicizedFrom(EAccessModifier.Protected)]
	public UIBasicSprite.FillDirection fillDirection;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool fillCenter = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isCreatedMaterial;

	[PublicizedFrom(EAccessModifier.Private)]
	public float globalOpacityModifier = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool originalAspectRatio;

	[PublicizedFrom(EAccessModifier.Protected)]
	public UnityWebRequest www;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool wwwAssigned;
}
