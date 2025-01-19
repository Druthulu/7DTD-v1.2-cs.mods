using System;
using System.Collections.Generic;
using Twitch;
using UniLinq;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_TwitchCommandList : XUiController
{
	public XUiC_TwitchWindow Owner { get; set; }

	public float GetHeight()
	{
		if (!this.twitchManager.IsReady || this.twitchManager.VotingManager.VotingIsActive)
		{
			return 0f;
		}
		if (this.commandLists.ContainsKey(this.CurrentKey))
		{
			return (float)(this.commandLists[this.CurrentKey].Count * 30);
		}
		return 0f;
	}

	public override void Init()
	{
		base.Init();
		XUiC_TwitchCommandEntry[] childrenByType = base.GetChildrenByType<XUiC_TwitchCommandEntry>(null);
		for (int i = 0; i < childrenByType.Length; i++)
		{
			if (childrenByType[i] != null)
			{
				this.commandEntries.Add(childrenByType[i]);
			}
		}
		this.twitchManager = TwitchManager.Current;
		this.twitchManager.CommandsChanged -= this.TwitchManager_CommandsChanged;
		this.twitchManager.CommandsChanged += this.TwitchManager_CommandsChanged;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TwitchManager_CommandsChanged()
	{
		this.SetupCommandList();
		this.lastUpdate = 0f;
		this.commandListIndex = -1;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void GetPrevCategory()
	{
		bool flag = false;
		int num = 0;
		while (!flag)
		{
			this.commandListIndex--;
			if (this.commandListIndex < 0)
			{
				this.commandListIndex = this.commandGroupList.Count - 1;
			}
			if (this.commandLists.ContainsKey(this.commandGroupList[this.commandListIndex].groupName))
			{
				flag = true;
			}
			num++;
			if (num > this.commandGroupList.Count)
			{
				break;
			}
		}
		this.ResetKey();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void GetNextCategory()
	{
		bool flag = false;
		int num = 0;
		while (!flag)
		{
			this.commandListIndex++;
			if (this.commandListIndex >= this.commandGroupList.Count)
			{
				this.commandListIndex = 0;
			}
			if (this.commandGroupList.Count == 0 || this.commandLists.ContainsKey(this.commandGroupList[this.commandListIndex].groupName))
			{
				flag = true;
			}
			num++;
			if (num > this.commandGroupList.Count)
			{
				break;
			}
		}
		this.ResetKey();
	}

	public override void Update(float _dt)
	{
		if (!this.twitchManager.IsReady)
		{
			return;
		}
		if (Time.time - this.lastUpdate >= this.secondRotation)
		{
			this.isDirty = true;
			if (this.commandLists.Count > 0)
			{
				this.GetNextCategory();
			}
			this.lastUpdate = Time.time;
		}
		if (this.isDirty)
		{
			if (this.commandLists.Count == 0)
			{
				this.SetupCommandList();
			}
			if (this.commandLists.Count != 0 && this.commandLists.ContainsKey(this.CurrentKey))
			{
				TwitchAction[] array = (from a in this.commandLists[this.CurrentKey]
				orderby a.Command
				orderby a.PointType
				select a).ToArray<TwitchAction>();
				int num = 0;
				int num2 = 0;
				while (num2 < array.Length && num < this.commandEntries.Count)
				{
					if (this.commandEntries[num] != null)
					{
						this.commandEntries[num].Owner = this.Owner;
						this.commandEntries[num].Action = array[num2];
						num++;
					}
					num2++;
				}
				for (int i = num; i < this.commandEntries.Count; i++)
				{
					this.commandEntries[i].Action = null;
				}
				this.isDirty = false;
			}
		}
		base.Update(_dt);
	}

	public void MoveForward()
	{
		this.GetNextCategory();
		this.lastUpdate = Time.time - 2f;
		this.isDirty = true;
	}

	public void MoveBackward()
	{
		this.GetPrevCategory();
		this.lastUpdate = Time.time - 2f;
		this.isDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public TwitchActionGroup AddCommandGroup(string groupName)
	{
		for (int i = 0; i < this.commandGroupList.Count; i++)
		{
			if (this.commandGroupList[i].groupName == groupName)
			{
				return this.commandGroupList[i];
			}
		}
		int categoryIndex = TwitchActionManager.Current.GetCategoryIndex(groupName);
		this.commandGroupList.Add(new TwitchActionGroup
		{
			ActionList = new List<TwitchAction>(),
			groupName = groupName,
			displayName = TwitchActionManager.Current.CategoryList[categoryIndex].DisplayName,
			index = categoryIndex
		});
		return this.commandGroupList[this.commandGroupList.Count - 1];
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RemoveCommandGroup(string groupName)
	{
		for (int i = 0; i < this.commandGroupList.Count; i++)
		{
			if (this.commandGroupList[i].groupName == groupName)
			{
				this.commandGroupList.RemoveAt(i);
				return;
			}
		}
	}

	public void SetupCommandList()
	{
		this.commandLists.Clear();
		string name = TwitchActionManager.Current.CategoryList[0].Name;
		int num = 0;
		foreach (string key in this.twitchManager.AvailableCommands.Keys)
		{
			TwitchAction twitchAction = this.twitchManager.AvailableCommands[key];
			if (!twitchAction.OnCooldown && twitchAction.OnlyUsableByType == TwitchAction.OnlyUsableTypes.Everyone)
			{
				num++;
			}
			if (num > 10)
			{
				break;
			}
		}
		bool flag = num <= 10;
		if (flag)
		{
			this.commandLists.Add(name, new List<TwitchAction>());
		}
		foreach (string key2 in this.twitchManager.AvailableCommands.Keys)
		{
			TwitchAction twitchAction2 = this.twitchManager.AvailableCommands[key2];
			if (twitchAction2.PointType != TwitchAction.PointTypes.Bits || !(this.twitchManager.BroadcasterType == ""))
			{
				if (flag)
				{
					if (!twitchAction2.OnCooldown && twitchAction2.OnlyUsableByType == TwitchAction.OnlyUsableTypes.Everyone)
					{
						twitchAction2.groupIndex = 0;
						this.commandLists[name].Add(twitchAction2);
						this.AddCommandGroup(name).ActionList.Add(twitchAction2);
					}
				}
				else if (twitchAction2.HasExtraConditions())
				{
					string text = twitchAction2.CategoryNames[0];
					if (!this.commandLists.ContainsKey(text))
					{
						this.commandLists.Add(text, new List<TwitchAction>());
					}
					this.AddCommandGroup(text).ActionList.Add(twitchAction2);
					twitchAction2.groupIndex = 0;
					this.commandLists[text].Add(twitchAction2);
				}
			}
		}
		this.commandListIndex = 0;
		this.lastUpdate = Time.time;
		if (!flag)
		{
			bool flag2 = true;
			while (flag2)
			{
				flag2 = false;
				foreach (string text2 in this.commandLists.Keys)
				{
					bool flag3 = false;
					if (this.commandLists[text2].Count > 10)
					{
						List<TwitchAction> list = this.commandLists[text2];
						this.commandLists.Remove(text2);
						this.RemoveCommandGroup(text2);
						for (int i = 0; i < list.Count; i++)
						{
							TwitchAction twitchAction3 = list[i];
							string text3 = twitchAction3.CategoryNames[twitchAction3.groupIndex];
							if (twitchAction3.CategoryNames.Count > twitchAction3.groupIndex + 1)
							{
								twitchAction3.groupIndex++;
								text3 = twitchAction3.CategoryNames[twitchAction3.groupIndex];
								flag3 = true;
							}
							if (!this.commandLists.ContainsKey(text3))
							{
								this.commandLists.Add(text3, new List<TwitchAction>());
							}
							this.AddCommandGroup(text3).ActionList.Add(twitchAction3);
							this.commandLists[text3].Add(twitchAction3);
						}
						if (flag3)
						{
							flag2 = true;
							break;
						}
						this.commandLists.Remove(text2);
						this.RemoveCommandGroup(text2);
						for (int j = 0; j < list.Count; j++)
						{
							TwitchAction twitchAction4 = list[j];
							int num2 = j / 10 + 1;
							string text4 = twitchAction4.CategoryNames[twitchAction4.groupIndex] + num2.ToString();
							if (!this.commandLists.ContainsKey(text4))
							{
								this.commandLists.Add(text4, new List<TwitchAction>());
							}
							this.AddCommandGroup(text4).ActionList.Add(twitchAction4);
							this.commandLists[text4].Add(twitchAction4);
						}
						flag2 = true;
						break;
					}
				}
			}
		}
		this.commandGroupList = (from x in this.commandGroupList
		orderby x.index, x.groupName
		select x).ToList<TwitchActionGroup>();
		this.ResetKey();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ResetKey()
	{
		if (this.commandLists.Count == 1)
		{
			this.CurrentTitle = (this.CurrentKey = Localization.Get("TwitchActionCategory_Commands", false));
			return;
		}
		this.CurrentKey = this.commandGroupList[this.commandListIndex].groupName;
		this.CurrentTitle = this.commandGroupList[this.commandListIndex].displayName;
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.isDirty = true;
		this.twitchManager = TwitchManager.Current;
	}

	public override bool ParseAttribute(string name, string value, XUiController _parent)
	{
		if (name == "complete_icon")
		{
			this.completeIconName = value;
			return true;
		}
		if (name == "incomplete_icon")
		{
			this.incompleteIconName = value;
			return true;
		}
		if (name == "complete_color")
		{
			Color32 color = StringParsers.ParseColor(value);
			this.completeColor = string.Format("{0},{1},{2},{3}", new object[]
			{
				color.r,
				color.g,
				color.b,
				color.a
			});
			this.completeHexColor = Utils.ColorToHex(color);
			return true;
		}
		if (!(name == "incomplete_color"))
		{
			return base.ParseAttribute(name, value, _parent);
		}
		Color32 color2 = StringParsers.ParseColor(value);
		this.incompleteColor = string.Format("{0},{1},{2},{3}", new object[]
		{
			color2.r,
			color2.g,
			color2.b,
			color2.a
		});
		this.incompleteHexColor = Utils.ColorToHex(color2);
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public List<XUiC_TwitchCommandEntry> commandEntries = new List<XUiC_TwitchCommandEntry>();

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isDirty;

	public string completeIconName = "";

	public string incompleteIconName = "";

	public string completeHexColor = "FF00FF00";

	public string incompleteHexColor = "FFB400";

	public string warningHexColor = "FFFF00FF";

	public string inactiveHexColor = "888888FF";

	public string activeHexColor = "FFFFFFFF";

	public string completeColor = "0,255,0,255";

	public string incompleteColor = "255, 180, 0, 255";

	public string warningColor = "255,255,0,255";

	public Dictionary<string, List<TwitchAction>> commandLists = new Dictionary<string, List<TwitchAction>>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<TwitchActionGroup> commandGroupList = new List<TwitchActionGroup>();

	[PublicizedFrom(EAccessModifier.Private)]
	public int commandListIndex = -1;

	public string CurrentKey = "";

	public string CurrentTitle = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public float lastUpdate;

	[PublicizedFrom(EAccessModifier.Private)]
	public float secondRotation = 10f;

	[PublicizedFrom(EAccessModifier.Private)]
	public TwitchManager twitchManager;
}
