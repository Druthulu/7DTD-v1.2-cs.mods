using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class AIDirectorMarkerManagementComponent : AIDirectorComponent
{
	public override void Tick(double _dt)
	{
		base.Tick(_dt);
		this.TickMarkers(_dt);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TickMarkers(double _dt)
	{
		for (int i = this.markers.Count - 1; i >= 0; i--)
		{
			IAIDirectorMarker iaidirectorMarker = this.markers[i];
			iaidirectorMarker.Tick(_dt);
			if (iaidirectorMarker.TimeToLive <= 0f || (iaidirectorMarker.Player != null && iaidirectorMarker.Player.IsDead()))
			{
				this.markers.RemoveAt(i);
				iaidirectorMarker.Release();
			}
		}
	}

	public IAIDirectorMarker FindBestMarker(Vector3 _pos, ref double _inOutIntensity)
	{
		IAIDirectorMarker result = null;
		int num = -1;
		for (int i = this.markers.Count - 1; i >= 0; i--)
		{
			IAIDirectorMarker iaidirectorMarker = this.markers[i];
			if (iaidirectorMarker.TimeToLive > 0f)
			{
				double num2 = iaidirectorMarker.IntensityForPosition(_pos);
				if (num2 > 0.0 && iaidirectorMarker.Priority > num)
				{
					num = iaidirectorMarker.Priority;
					result = iaidirectorMarker;
					_inOutIntensity = num2;
				}
			}
		}
		return result;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public List<IAIDirectorMarker> markers = new List<IAIDirectorMarker>(256);
}
