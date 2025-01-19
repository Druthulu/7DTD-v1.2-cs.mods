using System;
using UnityEngine.Scripting;

[Preserve]
public class MinEventActionAddPartTPV : MinEventActionAddPart
{
	public override bool CanExecute(MinEventTypes _eventType, MinEventParams _params)
	{
		EntityPlayerLocal entityPlayerLocal = _params.Self as EntityPlayerLocal;
		return entityPlayerLocal != null && !entityPlayerLocal.emodel.IsFPV && base.CanExecute(_eventType, _params);
	}
}
