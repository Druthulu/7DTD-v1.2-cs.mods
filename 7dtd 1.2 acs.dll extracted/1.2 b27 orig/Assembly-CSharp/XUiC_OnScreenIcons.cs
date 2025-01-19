using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_OnScreenIcons : XUiController
{
	public override void Init()
	{
		base.Init();
		NavObjectManager.Instance.OnNavObjectAdded += this.Instance_OnNavObjectAdded;
		NavObjectManager.Instance.OnNavObjectRemoved += this.Instance_OnNavObjectRemoved;
	}

	public override void Cleanup()
	{
		base.Cleanup();
		this.screenIconList.Clear();
		this.disabledIcons.Clear();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Instance_OnNavObjectRemoved(NavObject newNavObject)
	{
		if (newNavObject.HasOnScreen)
		{
			this.UnRegisterIcon(newNavObject);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Instance_OnNavObjectAdded(NavObject newNavObject)
	{
		if (newNavObject.HasOnScreen)
		{
			this.RegisterIcon(newNavObject);
		}
	}

	public override void Update(float _dt)
	{
		if (this.screenIconList.Count == 0)
		{
			return;
		}
		Vector3 offset = new Vector3((float)(-(float)base.ViewComponent.Size.x) * 0.5f, (float)(-(float)base.ViewComponent.Size.y) * 0.5f, 0f);
		Transform transform = base.xui.playerUI.entityPlayer.playerCamera.transform;
		Vector3 position = base.xui.playerUI.entityPlayer.GetPosition();
		int num = 300;
		for (int i = this.screenIconList.Count - 1; i >= 0; i--)
		{
			this.screenIconList[i].Update(offset, position, transform.forward, base.xui, ref num);
			if (this.screenIconList[i].ReadyForUnload)
			{
				this.disabledIcons.Add(this.screenIconList[i]);
				this.screenIconList[i].Transform.gameObject.SetActive(false);
				this.screenIconList.RemoveAt(i);
			}
		}
		base.Update(_dt);
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.Camera = base.xui.playerUI.entityPlayer.playerCamera;
	}

	public override void OnClose()
	{
		base.OnClose();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RegisterIcon(NavObject newNavObject)
	{
		for (int i = 0; i < this.screenIconList.Count; i++)
		{
			if (this.screenIconList[i].NavObject == newNavObject)
			{
				return;
			}
		}
		XUiC_OnScreenIcons.OnScreenIcon onScreenIcon = this.CreateIcon();
		onScreenIcon.Owner = this;
		onScreenIcon.NavObject = newNavObject;
		onScreenIcon.Init();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_OnScreenIcons.OnScreenIcon CreateIcon()
	{
		if (this.disabledIcons.Count > 0)
		{
			XUiC_OnScreenIcons.OnScreenIcon onScreenIcon = this.disabledIcons[0];
			this.disabledIcons.RemoveAt(0);
			this.screenIconList.Add(onScreenIcon);
			onScreenIcon.ReadyForUnload = false;
			onScreenIcon.Transform.gameObject.SetActive(true);
			onScreenIcon.Sprite.color = Color.clear;
			onScreenIcon.Sprite.spriteName = "";
			return onScreenIcon;
		}
		GameObject gameObject = new GameObject("ScreenIcon");
		gameObject.transform.parent = base.ViewComponent.UiTransform;
		gameObject.transform.localScale = Vector3.one;
		gameObject.layer = 12;
		this.screenIconList.Add(new XUiC_OnScreenIcons.OnScreenIcon());
		this.screenIconList[this.screenIconList.Count - 1].Transform = gameObject.transform;
		UISprite uisprite = new GameObject("Sprite")
		{
			transform = 
			{
				parent = gameObject.transform
			}
		}.AddComponent<UISprite>();
		uisprite.atlas = base.xui.GetAtlasByName("UIAtlas", "menu_empty");
		uisprite.transform.localScale = Vector3.one;
		uisprite.spriteName = "menu_empty";
		uisprite.SetDimensions(50, 50);
		uisprite.color = Color.clear;
		uisprite.pivot = UIWidget.Pivot.Center;
		uisprite.depth = 300;
		uisprite.gameObject.layer = 12;
		this.screenIconList[this.screenIconList.Count - 1].Sprite = uisprite;
		UILabel uilabel = new GameObject("Label")
		{
			transform = 
			{
				parent = gameObject.transform
			}
		}.AddComponent<UILabel>();
		uilabel.transform.localScale = Vector3.one;
		uilabel.font = base.xui.GetUIFontByName("ReferenceFont", true);
		uilabel.fontSize = 24;
		uilabel.pivot = UIWidget.Pivot.Center;
		uilabel.overflowMethod = UILabel.Overflow.ResizeFreely;
		uilabel.alignment = NGUIText.Alignment.Center;
		uilabel.transform.localPosition = new Vector2(-50f, -30f);
		uilabel.effectStyle = UILabel.Effect.Outline;
		uilabel.effectColor = new Color32(0, 0, 0, byte.MaxValue);
		uilabel.effectDistance = new Vector2(2f, 2f);
		uilabel.color = Color.white;
		uilabel.text = "";
		uilabel.gameObject.layer = 12;
		uilabel.depth = 300;
		uilabel.width = 200;
		this.screenIconList[this.screenIconList.Count - 1].Label = uilabel;
		return this.screenIconList[this.screenIconList.Count - 1];
	}

	public void UnRegisterIcon(NavObject navObject)
	{
		for (int i = this.screenIconList.Count - 1; i >= 0; i--)
		{
			if (this.screenIconList[i].NavObject == navObject)
			{
				if (!this.disabledIcons.Contains(this.screenIconList[i]))
				{
					this.disabledIcons.Add(this.screenIconList[i]);
					this.screenIconList[i].Transform.gameObject.SetActive(false);
				}
				this.screenIconList.RemoveAt(i);
			}
		}
	}

	public List<XUiC_OnScreenIcons.OnScreenIcon> screenIconList = new List<XUiC_OnScreenIcons.OnScreenIcon>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<XUiC_OnScreenIcons.OnScreenIcon> disabledIcons = new List<XUiC_OnScreenIcons.OnScreenIcon>();

	public Camera Camera;

	public class OnScreenIcon
	{
		public void Init()
		{
			if (this.Label != null)
			{
				this.ScreenSettings = this.NavObject.CurrentScreenSettings;
				this.Label.transform.localPosition = new Vector2(0f, -(this.ScreenSettings.SpriteSize * 0.5f + 8f));
			}
		}

		public void Update(Vector3 offset, Vector3 playerPosition, Vector3 cameraForward, XUi xui, ref int depth)
		{
			if (this.NavObject.IsValid())
			{
				this.Sprite.enabled = (this.Label.enabled = !this.NavObject.hiddenOnCompass);
				depth = this.UpdateDepth(depth) + 1;
				EntityPlayerLocal entityPlayer = xui.playerUI.entityPlayer;
				if (!this.NavObject.HasRequirements)
				{
					this.Transform.localPosition = NavObject.InvalidPos;
					return;
				}
				if (entityPlayer.IsDead())
				{
					this.Transform.localPosition = NavObject.InvalidPos;
					return;
				}
				this.ScreenSettings = this.NavObject.CurrentScreenSettings;
				if (this.ScreenSettings == null)
				{
					this.Transform.localPosition = NavObject.InvalidPos;
					return;
				}
				Vector3 vector = this.NavObject.GetPosition() + this.ScreenSettings.Offset;
				if (this.ScreenSettings.UseHeadOffset && this.NavObject.TrackType == NavObject.TrackTypes.Entity)
				{
					vector += new Vector3(0f, this.NavObject.TrackedEntity.GetEyeHeight(), 0f) + this.NavObject.TrackedEntity.EntityClass.NavObjectHeadOffset;
				}
				string spriteName = this.NavObject.GetSpriteName(this.ScreenSettings);
				Color color = this.NavObject.UseOverrideColor ? this.NavObject.OverrideColor : this.ScreenSettings.Color;
				float num = 1f;
				if (this.Sprite.spriteName != spriteName)
				{
					this.Sprite.atlas = xui.GetAtlasByName("UIAtlas", spriteName);
					this.Sprite.spriteName = spriteName;
				}
				this.Sprite.color = color;
				float num2 = Vector3.Distance(this.NavObject.GetPosition(), entityPlayer.position - Origin.position);
				float maxDistance = this.NavObject.GetMaxDistance(this.ScreenSettings, entityPlayer);
				if (maxDistance != -1f && num2 > maxDistance)
				{
					this.Transform.localPosition = NavObject.InvalidPos;
					return;
				}
				if (this.ScreenSettings.MinDistance > 0f && num2 < this.ScreenSettings.MinDistance)
				{
					this.Transform.localPosition = NavObject.InvalidPos;
					return;
				}
				Vector3 lhs = vector - this.Owner.Camera.transform.position;
				lhs.Normalize();
				Vector3 vector2 = entityPlayer.finalCamera.WorldToScreenPoint(vector);
				if (this.ScreenSettings.ShowOffScreen)
				{
					if (vector2.x < 30f)
					{
						vector2.x = 30f;
					}
					if (vector2.y < 30f + ((this.ScreenSettings.ShowTextType != NavObjectScreenSettings.ShowTextTypes.None) ? 30f : 0f))
					{
						vector2.y = 30f + ((this.ScreenSettings.ShowTextType != NavObjectScreenSettings.ShowTextTypes.None) ? 30f : 0f);
					}
					if (vector2.x > (float)(Screen.width - 30))
					{
						vector2.x = (float)(Screen.width - 30);
					}
					if (vector2.y > (float)(Screen.height - 30))
					{
						vector2.y = (float)(Screen.height - 30);
					}
					if (Vector3.Dot(lhs, cameraForward) < 0f)
					{
						if (vector2.y < (float)(Screen.height / 2))
						{
							vector2.y = (float)(Screen.height - 30);
						}
						else
						{
							vector2.y = 30f + ((this.ScreenSettings.ShowTextType != NavObjectScreenSettings.ShowTextTypes.None) ? 30f : 0f);
						}
						vector2.x = (float)Screen.width - vector2.x - 30f;
					}
				}
				else if (Vector3.Dot(lhs, cameraForward) < 0f)
				{
					this.Transform.localPosition = NavObject.InvalidPos;
					return;
				}
				Vector3 localPosition = xui.TranslateScreenVectorToXuiVector(vector2);
				localPosition.z = 0f;
				this.Transform.localPosition = localPosition;
				if (maxDistance != -1f)
				{
					float num3 = 1f;
					if (num2 >= maxDistance)
					{
						num3 = 0f;
					}
					else if (num2 >= this.ScreenSettings.FadeEndDistance)
					{
						num3 = 1f - Mathf.Clamp01((num2 - this.ScreenSettings.FadeEndDistance) / (maxDistance - this.ScreenSettings.FadeEndDistance));
						if (num3 < 1f)
						{
							this.Sprite.color = color;
						}
					}
					this.Sprite.alpha = num3;
				}
				else
				{
					this.Sprite.alpha = 1f;
				}
				if (num2 <= maxDistance - this.ScreenSettings.FadeEndDistance && this.ScreenSettings.HasPulse)
				{
					float num4 = Mathf.PingPong(Time.time, 0.5f);
					this.Sprite.color = Color.Lerp(Color.grey, color, num4 * 4f);
					if (num4 > 0.25f)
					{
						num += num4 - 0.25f;
					}
				}
				this.Sprite.SetDimensions((int)(num * this.ScreenSettings.SpriteSize), (int)(num * this.ScreenSettings.SpriteSize));
				if (this.ScreenSettings.SpriteFillType != NavObjectScreenSettings.SpriteFillTypes.None && this.NavObject.TrackedEntity != null)
				{
					if (this.FillSprite == null)
					{
						this.SetupFillSprite();
					}
					this.FillSprite.color = this.ScreenSettings.SpriteFillColor;
					this.FillSprite.alpha = this.Sprite.alpha;
					this.FillSprite.spriteName = this.ScreenSettings.SpriteFillName;
					this.FillSprite.SetDimensions((int)(num * this.ScreenSettings.SpriteSize), (int)(num * this.ScreenSettings.SpriteSize));
					if (this.ScreenSettings.SpriteFillType == NavObjectScreenSettings.SpriteFillTypes.Health)
					{
						this.FillSprite.fillAmount = ((EntityAlive)this.NavObject.TrackedEntity).Stats.Health.ValuePercent;
					}
				}
				else
				{
					this.RemoveFillSprite();
				}
				if (this.ScreenSettings.SubSpriteName != "")
				{
					if (this.SubSprite == null)
					{
						this.SetupSubSprite();
					}
					int num5 = (int)this.ScreenSettings.SubSpriteSize;
					this.SubSprite.transform.localPosition = this.ScreenSettings.SubSpriteOffset;
					this.SubSprite.SetDimensions(num5, num5);
					this.SubSprite.spriteName = this.ScreenSettings.SubSpriteName;
				}
				else
				{
					this.RemoveSubSprite();
				}
				if (this.Label != null)
				{
					if (this.ScreenSettings.ShowTextType != NavObjectScreenSettings.ShowTextTypes.None)
					{
						this.Label.alpha = this.Sprite.alpha;
						this.Label.fontSize = this.ScreenSettings.FontSize;
						this.Label.color = (this.NavObject.UseOverrideFontColor ? this.NavObject.OverrideColor : this.ScreenSettings.FontColor);
						if (this.ScreenSettings.ShowTextType == NavObjectScreenSettings.ShowTextTypes.Distance)
						{
							string arg = "m";
							if (num2 >= 1000f)
							{
								num2 /= 1000f;
								arg = "km";
							}
							this.Label.text = string.Format("{0} {1}", num2.ToCultureInvariantString("0.0"), arg);
							return;
						}
						if (this.ScreenSettings.ShowTextType == NavObjectScreenSettings.ShowTextTypes.Name)
						{
							this.Label.text = this.NavObject.DisplayName;
							return;
						}
						this.Label.text = ((this.NavObject.TrackedEntity != null) ? this.NavObject.TrackedEntity.spawnByName : "");
						return;
					}
					else
					{
						this.Label.text = "";
					}
				}
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void SetupFillSprite()
		{
			GameObject gameObject = new GameObject("FilledSprite");
			gameObject.transform.SetParent(this.Transform);
			gameObject.transform.localPosition = Vector3.zero;
			UISprite uisprite = gameObject.AddComponent<UISprite>();
			uisprite.atlas = this.Owner.xui.GetAtlasByName("UIAtlas", "menu_empty");
			uisprite.transform.localScale = Vector3.one;
			uisprite.spriteName = "menu_empty";
			uisprite.SetDimensions(50, 50);
			uisprite.color = Color.clear;
			uisprite.pivot = UIWidget.Pivot.Center;
			uisprite.fillDirection = UIBasicSprite.FillDirection.Radial360;
			uisprite.type = UIBasicSprite.Type.Filled;
			uisprite.depth = 300;
			uisprite.gameObject.layer = 12;
			uisprite.color = new Color(1f, 1f, 1f, 1f);
			this.FillSprite = uisprite;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void SetupSubSprite()
		{
			GameObject gameObject = new GameObject("SubSprite");
			gameObject.transform.SetParent(this.Transform);
			int num = (int)this.ScreenSettings.SubSpriteSize;
			gameObject.transform.localPosition = this.ScreenSettings.SubSpriteOffset;
			UISprite uisprite = gameObject.AddComponent<UISprite>();
			uisprite.atlas = this.Owner.xui.GetAtlasByName("UIAtlas", "menu_empty");
			uisprite.transform.localScale = Vector3.one;
			uisprite.spriteName = "menu_empty";
			uisprite.SetDimensions(num, num);
			uisprite.color = Color.clear;
			uisprite.pivot = UIWidget.Pivot.Center;
			uisprite.depth = 300;
			uisprite.gameObject.layer = 12;
			uisprite.color = new Color(1f, 1f, 1f, 1f);
			this.SubSprite = uisprite;
		}

		public void RemoveFillSprite()
		{
			if (this.FillSprite != null)
			{
				UnityEngine.Object.Destroy(this.FillSprite.gameObject);
				this.FillSprite = null;
			}
		}

		public void RemoveSubSprite()
		{
			if (this.SubSprite != null)
			{
				UnityEngine.Object.Destroy(this.SubSprite.gameObject);
				this.SubSprite = null;
			}
		}

		public int UpdateDepth(int depth)
		{
			this.Sprite.depth = depth;
			this.Label.depth = depth;
			if (this.FillSprite != null)
			{
				depth = (this.FillSprite.depth = depth + 1);
			}
			if (this.SubSprite != null)
			{
				depth = (this.SubSprite.depth = depth + 1);
			}
			return depth;
		}

		public XUiC_OnScreenIcons Owner;

		public NavObject NavObject;

		public UISprite Sprite;

		public UISprite FillSprite;

		public UISprite SubSprite;

		public UILabel Label;

		public Transform Transform;

		public bool ReadyForUnload;

		public NavObjectScreenSettings ScreenSettings;
	}
}
