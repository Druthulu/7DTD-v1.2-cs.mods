﻿using System;

namespace Platform
{
	public enum EApiStatusReason
	{
		ApiNotLoadable,
		SteamNotRunning,
		NoLicense,
		NoFriendsName,
		NotLoggedOn,
		Other,
		NoLoginTicket,
		NoOnlineStart,
		Unknown,
		NoConnection,
		Ok
	}
}
