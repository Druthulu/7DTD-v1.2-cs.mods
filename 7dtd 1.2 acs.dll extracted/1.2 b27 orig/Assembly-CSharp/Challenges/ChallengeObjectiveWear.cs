using System;
using System.Xml.Linq;
using UnityEngine.Scripting;

namespace Challenges
{
	[Preserve]
	public class ChallengeObjectiveWear : BaseChallengeObjective
	{
		public override ChallengeObjectiveType ObjectiveType
		{
			get
			{
				return ChallengeObjectiveType.Wear;
			}
		}

		public override string DescriptionText
		{
			get
			{
				return Localization.Get("challengeObjectiveWear", false) + " " + this.expectedItemClass.GetLocalizedItemName() + ":";
			}
		}

		public override void Init()
		{
			this.expectedItem = ItemClass.GetItem(this.itemClassID, false);
			this.expectedItemClass = ItemClass.GetItemClass(this.itemClassID, false);
		}

		public override void HandleAddHooks()
		{
			QuestEventManager.Current.WearItem -= this.Current_WearItem;
			XUi xui = LocalPlayerUI.GetUIForPlayer(this.Owner.Owner.Player).xui;
			XUiM_PlayerInventory playerInventory = xui.PlayerInventory;
			if (xui.PlayerEquipment.IsWearing(this.expectedItem))
			{
				base.Current = this.MaxCount;
				this.CheckObjectiveComplete(true);
				return;
			}
			QuestEventManager.Current.WearItem += this.Current_WearItem;
		}

		public override void HandleRemoveHooks()
		{
			QuestEventManager.Current.WearItem -= this.Current_WearItem;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void Current_WearItem(ItemValue itemValue)
		{
			if (itemValue.type == this.expectedItem.type)
			{
				base.Current = this.MaxCount;
				this.CheckObjectiveComplete(true);
			}
		}

		public override void ParseElement(XElement e)
		{
			base.ParseElement(e);
			if (e.HasAttribute("item"))
			{
				this.itemClassID = e.GetAttribute("item");
			}
		}

		public override BaseChallengeObjective Clone()
		{
			return new ChallengeObjectiveWear
			{
				itemClassID = this.itemClassID,
				expectedItem = this.expectedItem,
				expectedItemClass = this.expectedItemClass
			};
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public ItemValue expectedItem = ItemValue.None.Clone();

		[PublicizedFrom(EAccessModifier.Private)]
		public ItemClass expectedItemClass;

		public string itemClassID = "";
	}
}
