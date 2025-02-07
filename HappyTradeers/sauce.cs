so its not the animaation recation we need to change, its the 

base.xui.Dialog.Respondent.PlayVoiceSetEntry("quest_offer", base.xui.playerUI.entityPlayer, true, true);

and that is all over the place, with no enum list to choose from.

giving up on this for now.
the animation recation override did work, but not sure what it did








happy traders
    public void PlayAnimReaction(EntityTrader.AnimReaction reaction)
    {
        AvatarController avatarController = this.emodel.avatarController;
        if (avatarController)
        {
            avatarController.TriggerReaction((int)reaction);
        }
    }

EntityTrader
using System;
using System.Collections.Generic;
using Audio;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class EntityTrader : EntityNPC
{

maybe a prefix and patch the return avatarController.TriggerReaction((int)"Happy");

