﻿using System;
using System.Collections.Generic;
using Audio;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_BuffPopoutList : XUiController, IEntityUINotificationChanged
{
	public EntityPlayer LocalPlayer { get; [PublicizedFrom(EAccessModifier.Internal)] set; }

	public override void Init()
	{
		base.Init();
		XUiController childById = base.GetChildById("item");
		this.PrefabItems = childById.ViewComponent.UiTransform;
		this.height = (float)(childById.ViewComponent.Size.y + 2);
		childById.xui.BuffPopoutList = this;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (this.LocalPlayer == null && XUi.IsGameRunning())
		{
			this.LocalPlayer = base.xui.playerUI.entityPlayer;
		}
		GUIWindowManager windowManager = base.xui.playerUI.windowManager;
		if (windowManager.IsHUDEnabled() || (base.xui.dragAndDrop.InMenu && windowManager.IsHUDPartialHidden()))
		{
			if (base.ViewComponent.IsVisible && this.LocalPlayer.IsDead())
			{
				base.ViewComponent.IsVisible = false;
			}
			else if (!base.ViewComponent.IsVisible && !this.LocalPlayer.IsDead())
			{
				base.ViewComponent.IsVisible = true;
			}
		}
		else
		{
			base.ViewComponent.IsVisible = false;
		}
		for (int i = 0; i < this.items.Count; i++)
		{
			XUiC_BuffPopoutList.Data data = this.items[i];
			if (data.Notification.Buff != null && data.Notification.Buff.Paused)
			{
				this.removeEntry(data.Notification, i);
				this.disabledItems.Add(data.Notification);
			}
			else
			{
				if (data.Notification.DisplayMode == EnumEntityUINotificationDisplayMode.IconPlusCurrentValue)
				{
					UILabel component = data.Item.transform.Find("TextContent").GetComponent<UILabel>();
					string units = data.Notification.Units;
					if (!(units == "%"))
					{
						if (!(units == "°"))
						{
							if (!(units == "cvar"))
							{
								if (!(units == "duration"))
								{
									if (data.Notification.Buff.BuffClass.DisplayValueKey != null)
									{
										if (data.Notification.Buff.BuffClass.DisplayValueFormat == BuffClass.CVarDisplayFormat.Time)
										{
											component.text = string.Format(Localization.Get(data.Notification.Buff.BuffClass.DisplayValueKey, false), XUiC_BuffPopoutList.GetCVarValueAsTimeString(data.Notification.CurrentValue));
										}
										else
										{
											component.text = string.Format(Localization.Get(data.Notification.Buff.BuffClass.DisplayValueKey, false), data.Notification.CurrentValue);
										}
									}
									else
									{
										component.text = ((int)data.Notification.CurrentValue).ToString();
									}
								}
								else
								{
									component.text = XUiC_BuffPopoutList.GetCVarValueAsTimeString(data.Notification.Buff.BuffClass.DurationMax - data.Notification.Buff.DurationInSeconds);
								}
							}
							else if (data.Notification.Buff.BuffClass.DisplayValueKey != null)
							{
								if (data.Notification.Buff.BuffClass.DisplayValueFormat == BuffClass.CVarDisplayFormat.Time)
								{
									component.text = string.Format(Localization.Get(data.Notification.Buff.BuffClass.DisplayValueKey, false), XUiC_BuffPopoutList.GetCVarValueAsTimeString(data.Notification.CurrentValue));
								}
								else
								{
									component.text = string.Format(Localization.Get(data.Notification.Buff.BuffClass.DisplayValueKey, false), data.Notification.CurrentValue);
								}
							}
							else if (data.Notification.Buff.BuffClass.DisplayValueFormat == BuffClass.CVarDisplayFormat.Time)
							{
								component.text = XUiC_BuffPopoutList.GetCVarValueAsTimeString(data.Notification.CurrentValue);
							}
							else
							{
								component.text = ((int)data.Notification.CurrentValue).ToString();
							}
						}
						else if (component != null)
						{
							component.text = ValueDisplayFormatters.Temperature(data.Notification.CurrentValue, 0);
						}
					}
					else if (component != null)
					{
						component.text = ((int)(data.Notification.CurrentValue * 100f)).ToString() + "%";
					}
				}
				else
				{
					EnumEntityUINotificationDisplayMode displayMode = data.Notification.DisplayMode;
				}
				bool flag = false;
				if (data.Notification.Buff != null)
				{
					flag = (EffectManager.GetValue(PassiveEffects.BuffBlink, null, 0f, this.LocalPlayer, null, data.Notification.Buff.BuffClass.NameTag, false, false, false, true, true, 1, true, false) >= 1f);
				}
				if (data.Notification.Buff != null && (data.Notification.Buff.BuffClass.IconBlink || flag))
				{
					Color color = data.Notification.GetColor();
					float num = Mathf.PingPong(Time.time, 0.5f);
					data.Sprite.color = Color.Lerp(Color.grey, color, num * 4f);
					float num2 = 1f;
					if (num > 0.25f)
					{
						num2 = 1f + num - 0.25f;
					}
					data.Sprite.SetDimensions((int)(this.spriteSize.x * num2), (int)(this.spriteSize.y * num2));
				}
				else
				{
					data.Sprite.color = data.Notification.GetColor();
					data.Sprite.SetDimensions((int)this.spriteSize.x, (int)this.spriteSize.y);
				}
			}
		}
		if (this.disabledItems.Count > 0)
		{
			for (int j = this.disabledItems.Count - 1; j >= 0; j--)
			{
				EntityUINotification entityUINotification = this.disabledItems[j];
				if (!entityUINotification.Buff.Paused)
				{
					this.AddNotification(entityUINotification);
					this.disabledItems.RemoveAt(j);
				}
			}
			this.updateEntries();
		}
	}

	public static string GetCVarValueAsTimeString(float cvarValue)
	{
		return XUiM_PlayerBuffs.GetCVarValueAsTimeString(cvarValue);
	}

	public override void OnOpen()
	{
		base.OnOpen();
		if (!this.initialized)
		{
			this.PrefabItems.gameObject.SetActive(false);
			EntityPlayer entityPlayer = base.xui.playerUI.entityPlayer;
			List<EntityUINotification> notifications = entityPlayer.Stats.Notifications;
			for (int i = 0; i < notifications.Count; i++)
			{
				this.AddNotification(notifications[i]);
			}
			entityPlayer.Stats.AddUINotificationChangedDelegate(this);
			this.initialized = true;
		}
	}

	public override void OnClose()
	{
		base.OnClose();
		EntityPlayer entityPlayer = base.xui.playerUI.entityPlayer;
		if (entityPlayer != null)
		{
			entityPlayer.Stats.RemoveUINotificationChangedDelegate(this);
		}
		this.initialized = false;
		for (int i = 0; i < this.items.Count; i++)
		{
			UnityEngine.Object.Destroy(this.items[i].Item.gameObject);
		}
		this.items.Clear();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void removeEntry(EntityUINotification notification, int currentIndex = -1)
	{
		int num = (currentIndex == -1) ? this.GetNotificationIndex(notification) : currentIndex;
		if (num == -1)
		{
			return;
		}
		TemporaryObject temporaryObject = this.items[num].Item.transform.GetComponent<TemporaryObject>();
		if (temporaryObject == null)
		{
			temporaryObject = this.items[num].Item.transform.gameObject.AddComponent<TemporaryObject>();
		}
		temporaryObject.enabled = true;
		TweenColor tweenColor = this.items[num].Item.transform.GetComponent<TweenColor>();
		if (tweenColor == null)
		{
			tweenColor = this.items[num].Item.transform.gameObject.AddComponent<TweenColor>();
		}
		tweenColor.from = Color.white;
		tweenColor.to = new Color(1f, 1f, 1f, 0f);
		tweenColor.enabled = true;
		tweenColor.duration = 0.4f;
		TweenScale tweenScale = this.items[num].Item.gameObject.AddComponent<TweenScale>();
		tweenScale.from = Vector3.one;
		tweenScale.to = Vector3.zero;
		tweenScale.enabled = true;
		tweenScale.duration = 0.5f;
		this.items.RemoveAt(num);
		this.updateEntries();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int GetNotificationIndex(EntityUINotification notification)
	{
		for (int i = 0; i < this.items.Count; i++)
		{
			if (this.items[i].Notification.Subject == notification.Subject)
			{
				if (notification.Subject != EnumEntityUINotificationSubject.Buff)
				{
					return i;
				}
				if (this.items[i].Notification.Buff.BuffClass.ShowOnHUD && this.items[i].Notification.Buff.BuffClass.Name == notification.Buff.BuffClass.Name)
				{
					return i;
				}
			}
		}
		return -1;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateEntries()
	{
		int num = 0;
		for (int i = 0; i < this.items.Count; i++)
		{
			if (!this.items[i].Notification.Buff.Paused)
			{
				TweenPosition tweenPosition = this.items[i].Item.GetComponent<TweenPosition>();
				if (tweenPosition)
				{
					UnityEngine.Object.Destroy(tweenPosition);
				}
				tweenPosition = this.items[i].Item.AddComponent<TweenPosition>();
				tweenPosition.from = this.items[i].Item.transform.localPosition;
				tweenPosition.to = new Vector3(this.items[i].Item.transform.localPosition.x, (float)num * this.height + (float)this.yOffset, this.items[i].Item.transform.localPosition.z);
				tweenPosition.enabled = true;
				num++;
			}
		}
	}

	public void EntityUINotificationAdded(EntityUINotification _notification)
	{
		this.AddNotification(_notification);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void AddNotification(EntityUINotification _notification)
	{
		int notificationIndex = this.GetNotificationIndex(_notification);
		if (notificationIndex == -1)
		{
			if (_notification.Icon != "")
			{
				if (_notification.Buff != null)
				{
					if (!_notification.Buff.BuffClass.ShowOnHUD)
					{
						return;
					}
					if (_notification.Buff.Paused)
					{
						this.disabledItems.Add(_notification);
					}
				}
				GameObject gameObject = base.ViewComponent.UiTransform.gameObject.AddChild(this.PrefabItems.gameObject);
				gameObject.SetActive(true);
				gameObject.GetComponent<BoxCollider>().center = Vector3.zero;
				gameObject.GetComponent<UIPanel>();
				UIEventListener uieventListener = UIEventListener.Get(gameObject.gameObject);
				uieventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uieventListener.onClick, new UIEventListener.VoidDelegate(this.OnNotificationClicked));
				gameObject.transform.Find("Background").GetComponent<UISprite>().color = Color.white;
				UISprite component = gameObject.transform.Find("Icon").GetComponent<UISprite>();
				component.atlas = base.xui.GetAtlasByName(((UnityEngine.Object)component.atlas).name, _notification.Icon);
				component.spriteName = _notification.Icon;
				component.color = _notification.GetColor();
				if (this.spriteSize == Vector2.zero)
				{
					this.spriteSize = new Vector2((float)component.width, (float)component.height);
				}
				UILabel component2 = gameObject.transform.Find("TextContent").GetComponent<UILabel>();
				if (_notification.DisplayMode == EnumEntityUINotificationDisplayMode.IconPlusCurrentValue)
				{
					string units = _notification.Units;
					if (!(units == "%"))
					{
						if (!(units == "°"))
						{
							if (component2 != null)
							{
								component2.text = _notification.CurrentValue.ToCultureInvariantString("0");
							}
						}
						else if (component2 != null)
						{
							component2.text = _notification.CurrentValue.ToCultureInvariantString("0") + "°";
						}
					}
					else if (component2 != null)
					{
						component2.text = (_notification.CurrentValue * 100f).ToCultureInvariantString("0") + "%";
					}
				}
				gameObject.transform.localPosition = new Vector3(gameObject.transform.localPosition.x, (float)this.items.Count * this.height + (float)this.yOffset, gameObject.transform.localPosition.z);
				XUiC_BuffPopoutList.Data data = new XUiC_BuffPopoutList.Data();
				data.Item = gameObject;
				data.TimeAdded = Time.time;
				data.Notification = _notification;
				data.Sprite = component;
				if (_notification.Buff != null && _notification.Buff.BuffClass.TooltipKey != null)
				{
					GameManager.ShowTooltip(base.xui.playerUI.entityPlayer, _notification.Buff.BuffClass.TooltipKey, false);
				}
				this.items.Add(data);
				return;
			}
		}
		else
		{
			this.items[notificationIndex].Notification = _notification;
			this.items[notificationIndex].TimeAdded = Time.time;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnNotificationClicked(GameObject go)
	{
		this.HandleClickForItem(go);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleClickForItem(GameObject go)
	{
		for (int i = 0; i < this.items.Count; i++)
		{
			if (this.items[i].Item == go)
			{
				Manager.PlayInsidePlayerHead("craft_click_craft", -1, 0f, false, false);
				if (!base.xui.playerUI.windowManager.IsWindowOpen("character"))
				{
					XUiC_WindowSelector.OpenSelectorAndWindow(base.xui.playerUI.entityPlayer, "character");
				}
				this.SelectedNotification = this.items[i].Notification;
				return;
			}
		}
	}

	public void EntityUINotificationRemoved(EntityUINotification _notification)
	{
		this.removeEntry(_notification, -1);
	}

	public void SetYOffset(int _yOffset)
	{
		if (_yOffset != this.yOffset)
		{
			this.yOffset = _yOffset;
			this.updateEntries();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool initialized;

	[PublicizedFrom(EAccessModifier.Private)]
	public float height;

	[PublicizedFrom(EAccessModifier.Private)]
	public int yOffset;

	public Transform PrefabItems;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2 spriteSize;

	public EntityUINotification SelectedNotification;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<XUiC_BuffPopoutList.Data> items = new List<XUiC_BuffPopoutList.Data>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<EntityUINotification> disabledItems = new List<EntityUINotification>();

	[PublicizedFrom(EAccessModifier.Private)]
	public class Data
	{
		public GameObject Item;

		public float TimeAdded;

		public EntityUINotification Notification;

		public UISprite Sprite;
	}
}
