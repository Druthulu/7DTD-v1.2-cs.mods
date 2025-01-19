﻿using System;
using System.Collections.Generic;
using Platform;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions
{
	[Preserve]
	public class ActionRenameSigns : ActionBaseContainersAction
	{
		public override bool CheckValidTileEntity(TileEntity te, out bool isEmpty)
		{
			isEmpty = true;
			ITileEntitySignable tileEntitySignable;
			if (te.TryGetSelfOrFeature(out tileEntitySignable) && tileEntitySignable.EntityId == -1)
			{
				isEmpty = (tileEntitySignable.GetAuthoredText().Text == base.ModifiedName);
				return true;
			}
			return false;
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override bool HandleContainerAction(List<TileEntity> tileEntityList)
		{
			bool result = false;
			for (int i = 0; i < tileEntityList.Count; i++)
			{
				ITileEntitySignable tileEntitySignable;
				if (tileEntityList[i].TryGetSelfOrFeature(out tileEntitySignable) && tileEntitySignable.EntityId == -1)
				{
					tileEntitySignable.SetText(base.ModifiedName, true, PlatformManager.MultiPlatform.User.PlatformUserId);
					result = true;
				}
			}
			return result;
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override BaseAction CloneChildSettings()
		{
			return new ActionRenameSigns
			{
				TargetingType = this.TargetingType,
				maxDistance = this.maxDistance,
				newName = this.newName,
				changeName = this.changeName,
				tileEntityList = this.tileEntityList
			};
		}
	}
}
