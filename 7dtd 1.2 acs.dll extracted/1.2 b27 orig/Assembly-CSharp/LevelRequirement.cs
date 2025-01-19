using System;
using System.Collections.Generic;

public struct LevelRequirement
{
	public LevelRequirement(int _level)
	{
		this.Level = _level;
		this.Requirements = null;
	}

	public void AddRequirement(IRequirement _req)
	{
		if (this.Requirements == null)
		{
			this.Requirements = new List<IRequirement>();
		}
		this.Requirements.Add(_req);
	}

	public readonly int Level;

	public List<IRequirement> Requirements;
}
