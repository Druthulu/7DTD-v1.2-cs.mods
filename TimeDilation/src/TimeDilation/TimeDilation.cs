using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using UnityEngine.PlayerLoop;
using Webserver.WebAPI.APIs.WorldState;

public class TimeDilation : IModApi
{

	// init
	public void InitMod(Mod _modInstance)
	{
		string str = "[TimeDilation] Loading Patch: ";
		Type type = base.GetType();
		Log.Out(str + ((type != null) ? type.ToString() : null));
		ModEvents.ChatMessage.RegisterHandler(new global::Func<ClientInfo, EChatType, int, string, string, List<int>, bool>(this.ChatMessage));
		ModEvents.PlayerSpawnedInWorld.RegisterHandler(new Action<ClientInfo, RespawnType, Vector3i>(this.PlayerSpawnedInWorld));
		ModEvents.PlayerDisconnected.RegisterHandler(new Action<ClientInfo, bool>(this.PlayerDisconnected));
		//new events to track bloodmoon
		ModEvents.GameUpdate.RegisterHandler(GameUpdate);
		ModEvents.GameStartDone.RegisterHandler(GameStart);

        string modsFolderPath = _modInstance.Path;
		//string text = Path.Combine(_modInstance.Path, "\\Config\\settings.xml");
		//text.Replace('/', Path.DirectorySeparatorChar);
		//text.Replace('\\', Path.DirectorySeparatorChar);
		Log.Out("[TimeDilation] Attempting to load configuration file from " + _modInstance.Path, "\\Config\\settings.xml");
		XElement xelement = XElement.Load(_modInstance.Path + "\\Config\\settings.xml");
		bool flag = false;
        // enable regular time during bloodmoon
        Log.Out("[TimeDilation] Loading bloodmoon preference.");
        if (bool.TryParse(xelement.Element("timedilation").Elements("regTimeBloodMoon").Single<XElement>().Value, out flag))
        {
            this.regTimeBloodMoon = flag;
            Log.Out("[TimeDilation] Load successful.  regTimeBloodMoon = " + this.silent.ToString());
        }
        else
        {
            this.regTimeBloodMoon = true;
            Log.Out("[TimeDilation] Load unsuccessful, loading default  regTimeBloodMoon = true");
        }
        //load silence settings
        Log.Out("[TimeDilation] Loading silence preference.");
        if (bool.TryParse(xelement.Element("timedilation").Elements("silent").Single<XElement>().Value, out flag))
		{
			this.silent = flag;
			Log.Out("[TimeDilation] Load successful.  silent = " + this.silent.ToString());
		}
		else
		{
			this.silent = false;
			Log.Out("[TimeDilation] Load unsuccessful.  silent = false");
		}
		Log.Out("[TimeDilation] Loading proposed half-time threshold.");
		int num;
		//load half threshold players
		if (int.TryParse(xelement.Element("timedilation").Elements("thresholdHalfTime").Single<XElement>().Value, out num))
		{
			this.thresholdHalfTime = num;
			Log.Out("[TimeDilation] Load successful.  thresholdHalfTime = " + this.thresholdHalfTime.ToString());
		}
		else
		{
			this.thresholdHalfTime = 2;
			Log.Out("[TimeDilation] Load unsuccessful.  thresholdHalfTime = 2");
		}
		Log.Out("[TimeDilation] Loading proposed normal-time threshold.");
		int num2;
		//set normal time
		if (int.TryParse(xelement.Element("timedilation").Elements("thresholdNormalTime").Single<XElement>().Value, out num2))
		{
			this.thresholdNormalTime = num2;
			Log.Out("[TimeDilation] Load successful.  thresholdNormalTime = " + this.thresholdNormalTime.ToString());
		}
		else
		{
			this.thresholdNormalTime = 3;
			Log.Out("[TimeDilation] Load unsuccessful.  thresholdNormalTime = 3");
		}
		//invalid values revert to default
		if (this.thresholdNormalTime <= this.thresholdHalfTime || this.thresholdNormalTime == 0 || this.thresholdHalfTime == 0)
		{
			Log.Out("[TimeDilation] Invalid configuration of thresholds detected.  thresholdNormalTime = " + this.thresholdNormalTime.ToString() + "; thresholdHalfTime = " + this.thresholdHalfTime.ToString());
			this.thresholdHalfTime = 2;
			this.thresholdNormalTime = 3;
			Log.Out("[TimeDilation] Returning threshold values to default.");
		}
	}

	public void GameStart()
	{
        this.isBloodMoon = GameManager.Instance.World.isEventBloodMoon;
        this.sayToServer("[TimeDilation] Game start, checking for bloodmoon, bloodmoon = " + this.isBloodMoon.ToString());
    }
    // New func to track bloodmoons
    public void GameUpdate()
    {
		// if config is chosen
		if (this.regTimeBloodMoon)
		{
			if (!this.isBloodMoon)
			{
				if (GameManager.Instance.World.isEventBloodMoon)
				{
                    // un dialate time during bloodmoons
                    
                    this.sayToServer("[TimeDilation] Blood Moon starting. Restoring normal flow of time.");
					this.isBloodMoon = true;
                    GameStats.Set(EnumGameStats.TimeOfDayIncPerSec, 6);
                }
			}
			else
			{
                if (!GameManager.Instance.World.isEventBloodMoon)
                {
                    // dialate time after bloodmoon is over
                    this.sayToServer("[TimeDilation] Blood Moon ending. Dialated time is in effect.");
                    this.isBloodMoon = false;
                    this.dilateTime(this.numPlayers);
                }
            }
        }  
    }


    // main func to change time
    private void dilateTime(int playerCount)
	{
		//bool flag = playerCount > 0;
		if (playerCount > 0)
		{
			//bool flag2 = playerCount >= this.thresholdNormalTime;
			if (playerCount >= this.thresholdNormalTime)
			{
				GameStats.Set(EnumGameStats.TimeOfDayIncPerSec, 6);
				this.sayToServer("[TimeDilation] " + this.thresholdNormalTime.ToString() + "+ players are connected.  Normal time is in effect.");
			}
			else
			{
				//bool flag3 = playerCount >= this.thresholdHalfTime;
				if (playerCount >= this.thresholdHalfTime)
				{
					GameStats.Set(EnumGameStats.TimeOfDayIncPerSec, 3);
					this.sayToServer("[TimeDilation] " + this.thresholdHalfTime.ToString() + "+ players are connected.  Time dilated by 2x.");
				}
				else
				{
					GameStats.Set(EnumGameStats.TimeOfDayIncPerSec, 2);
					this.sayToServer("[TimeDilation] Fewer than " + this.thresholdHalfTime.ToString() + " players are connected.  Time dilated by 3x.");
				}
			}
		}
		else
		{
			//bool flag4 = playerCount == 0;
			if (playerCount == 0)
			{
				GameStats.Set(EnumGameStats.TimeOfDayIncPerSec, 2);
				this.sayToServer("[TimeDilation] No players are connected.  Time dilated by 3x.");
			}
		}
	}

	private void sayToServer(string msg)
	{
		//bool flag = !this.silent;
		if (!this.silent)
		{
			GameManager.Instance.ChatMessageServer(null, EChatType.Global, -1, msg, null, EMessageSender.Server);
		}
	}

	// check messages to see if someone ran the timedia command to change time
	public bool ChatMessage(ClientInfo cInfo, EChatType type, int senderId, string msg, string mainName, List<int> recipientEntityIds)
	{
		bool flag = !string.IsNullOrEmpty(msg) && cInfo != null && mainName != this.serverChatName;
		if (flag)
		{
			bool flag2 = msg.StartsWith("/");
			if (flag2)
			{
				msg = msg.Replace("/", "");
				bool flag3 = msg == "timedilationmanualupdate";
				if (flag3)
				{
					this.dilateTime(GameManager.Instance.World.Players.Count);
					return false;
				}
				bool flag4 = msg.StartsWith("timedilationmanualupdate ");
				if (flag4)
				{
					msg = msg.Replace("timedilationmanualupdate ", "");
					int num = 0;
					bool flag5 = int.TryParse(msg, out num);
					bool flag6 = flag5 && num >= 0;
					if (flag6)
					{
						this.dilateTime(num);
					}
					else
					{
						this.sayToServer("[TimeDilation] An invalid number of players was specified.  No time dilation update was performed.");
					}
					return false;
				}
				return true;
			}
		}
		return true;
	}

	// change time on player spawn
	public void PlayerSpawnedInWorld(ClientInfo cInfo, RespawnType respawnReason, Vector3i pos)
	{
		//bool flag = respawnReason == RespawnType.EnterMultiplayer || respawnReason == RespawnType.JoinMultiplayer;
		if (respawnReason == RespawnType.EnterMultiplayer || respawnReason == RespawnType.JoinMultiplayer)
		{
			this.numPlayers++;
			if (!this.isBloodMoon)
			{
                this.dilateTime(this.numPlayers);
            }
        }
	}

	// change time on player leave
	public void PlayerDisconnected(ClientInfo cInfo, bool bShutDown)
	{
		//bool flag = this.numPlayers > 0;
		if (this.numPlayers > 0)
		{
			this.numPlayers--;
			if (!this.isBloodMoon)
			{
				this.dilateTime(this.numPlayers);
			}
        }
	}


    private bool isBloodMoon;

    private bool regTimeBloodMoon = false;

	private string serverChatName = "Server";

	private bool silent;

	private const int defaultTimeOfDayIncPerSec = 6;

	private int thresholdHalfTime;

	private int thresholdNormalTime;

	private int numPlayers = 0;
}
