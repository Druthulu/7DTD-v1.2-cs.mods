using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_DialogResponseList : XUiController
{
	public Dialog CurrentDialog
	{
		get
		{
			return this.currentDialog;
		}
		set
		{
			this.currentDialog = value;
			base.RefreshBindings(true);
			this.IsDirty = true;
		}
	}

	public override void Init()
	{
		base.Init();
		this.xuiQuestDescriptionLabel = Localization.Get("xuiDescriptionLabel", false);
		for (int i = 0; i < this.children.Count; i++)
		{
			if (this.children[i] is XUiC_DialogResponseEntry)
			{
				XUiC_DialogResponseEntry item = (XUiC_DialogResponseEntry)this.children[i];
				this.entryList.Add(item);
				this.length++;
			}
		}
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (this.IsDirty)
		{
			List<BaseResponseEntry> list = new List<BaseResponseEntry>();
			this.uniqueResponseIDs.Clear();
			if (this.currentDialog != null)
			{
				list = this.currentDialog.CurrentStatement.GetResponses();
			}
			int num = 0;
			for (int i = 0; i < this.entryList.Count; i++)
			{
				XUiC_DialogResponseEntry xuiC_DialogResponseEntry = this.entryList[i];
				if (xuiC_DialogResponseEntry != null)
				{
					xuiC_DialogResponseEntry.OnPress -= this.OnPressResponse;
					if (num < list.Count)
					{
						xuiC_DialogResponseEntry.ViewComponent.SoundPlayOnClick = true;
						if (list[num].UniqueID == "" || !this.uniqueResponseIDs.Contains(list[num].UniqueID))
						{
							xuiC_DialogResponseEntry.CurrentResponse = list[num].Response;
							xuiC_DialogResponseEntry.OnPress += this.OnPressResponse;
						}
						else
						{
							xuiC_DialogResponseEntry.CurrentResponse = null;
						}
						if (xuiC_DialogResponseEntry.CurrentResponse == null)
						{
							i--;
						}
						else if (list[num].UniqueID != "")
						{
							this.uniqueResponseIDs.Add(list[num].UniqueID);
						}
						num++;
					}
					else
					{
						xuiC_DialogResponseEntry.ViewComponent.SoundPlayOnClick = false;
						xuiC_DialogResponseEntry.CurrentResponse = null;
					}
				}
			}
			if (list.Count > 0)
			{
				this.entryList[0].SelectCursorElement(true, false);
			}
			this.IsDirty = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnPressResponse(XUiController _sender, int _mouseButton)
	{
		if (((XUiC_DialogResponseEntry)_sender).HasRequirement)
		{
			DialogResponse currentResponse = ((XUiC_DialogResponseEntry)_sender).CurrentResponse;
			this.currentDialog.SelectResponse(currentResponse, base.xui.playerUI.entityPlayer);
			((XUiC_DialogWindowGroup)this.windowGroup.Controller).RefreshDialog();
		}
	}

	public void Refresh()
	{
		this.IsDirty = true;
		base.RefreshBindings(true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Dialog conversation;

	[PublicizedFrom(EAccessModifier.Private)]
	public string xuiQuestDescriptionLabel;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<XUiC_DialogResponseEntry> entryList = new List<XUiC_DialogResponseEntry>();

	[PublicizedFrom(EAccessModifier.Private)]
	public int length;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<string> uniqueResponseIDs = new List<string>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Dialog currentDialog;
}
