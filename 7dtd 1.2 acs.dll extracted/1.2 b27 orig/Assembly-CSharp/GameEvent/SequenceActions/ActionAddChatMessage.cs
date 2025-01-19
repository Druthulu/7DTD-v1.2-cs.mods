using System;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions
{
	[Preserve]
	public class ActionAddChatMessage : ActionBaseClientAction
	{
		public override void OnClientPerform(Entity target)
		{
			EntityPlayerLocal entityPlayerLocal = target as EntityPlayerLocal;
			if (entityPlayerLocal != null)
			{
				XUiC_ChatOutput.AddMessage(LocalPlayerUI.GetUIForPlayer(entityPlayerLocal).xui, EnumGameMessages.PlainTextLocal, EChatType.Global, this.text, -1, EMessageSender.Server, GeneratedTextManager.TextFilteringMode.None);
			}
		}

		public override void ParseProperties(DynamicProperties properties)
		{
			base.ParseProperties(properties);
			if (properties.Values.ContainsKey(ActionAddChatMessage.PropText))
			{
				this.text = properties.Values[ActionAddChatMessage.PropText];
			}
			if (properties.Values.ContainsKey(ActionAddChatMessage.PropTextKey))
			{
				this.textKey = properties.Values[ActionAddChatMessage.PropTextKey];
				this.text = Localization.Get(this.textKey, false);
			}
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override BaseAction CloneChildSettings()
		{
			return new ActionAddChatMessage
			{
				targetGroup = this.targetGroup,
				textKey = this.textKey,
				text = this.text
			};
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public string textKey = "";

		[PublicizedFrom(EAccessModifier.Protected)]
		public string text = "";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropText = "text";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropTextKey = "text_key";
	}
}
