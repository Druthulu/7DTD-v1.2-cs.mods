﻿using System;

public interface IPackageDestinationFilter
{
	bool Exclude(ClientInfo _cInfo);
}
