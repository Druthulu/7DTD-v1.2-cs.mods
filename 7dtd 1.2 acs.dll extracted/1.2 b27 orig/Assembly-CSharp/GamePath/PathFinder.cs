using System;
using UnityEngine;

namespace GamePath
{
	public class PathFinder
	{
		public PathFinder(PathInfo _pathInfo, bool _bDrn, bool _canClimbLadders, bool _bCanClimbWalls)
		{
			this.pathInfo = _pathInfo;
			this.canEntityDrown = _bDrn;
			this.canClimbWalls = _bCanClimbWalls;
			this.canClimbLadders = _canClimbLadders;
		}

		public virtual void Calculate(Vector3 _fromPos, Vector3 _toPos)
		{
		}

		public virtual void Destruct()
		{
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public PathInfo pathInfo;

		[PublicizedFrom(EAccessModifier.Protected)]
		public bool canClimbWalls;

		[PublicizedFrom(EAccessModifier.Protected)]
		public bool canClimbLadders;

		[PublicizedFrom(EAccessModifier.Protected)]
		public bool canEntityDrown;
	}
}
