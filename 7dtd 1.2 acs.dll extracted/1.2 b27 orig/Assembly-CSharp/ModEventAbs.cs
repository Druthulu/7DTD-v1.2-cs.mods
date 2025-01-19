using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;

public abstract class ModEventAbs<TDelegate>
{
	[MethodImpl(MethodImplOptions.NoInlining)]
	public void RegisterHandler(TDelegate _handlerFunc)
	{
		Assembly callingAssembly = Assembly.GetCallingAssembly();
		Assembly assembly = typeof(ModEvents).Assembly;
		bool coreGame = false;
		Mod mod = null;
		if (callingAssembly.Equals(assembly))
		{
			coreGame = true;
		}
		else
		{
			mod = ModManager.GetModForAssembly(callingAssembly);
			if (mod == null)
			{
				Log.Warning("[MODS] Could not find mod that tries to register a handler for event " + this.eventName);
			}
		}
		this.receivers.Add(new ModEventAbs<TDelegate>.Receiver(mod, _handlerFunc, coreGame));
	}

	public void UnregisterHandler(TDelegate _handlerFunc)
	{
		for (int i = 0; i < this.receivers.Count; i++)
		{
			TDelegate delegateFunc = this.receivers[i].DelegateFunc;
			if (delegateFunc.Equals(_handlerFunc))
			{
				this.receivers.RemoveAt(i);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void LogError(Exception _e, ModEventAbs<TDelegate>.Receiver _currentMod)
	{
		Log.Error(string.Concat(new string[]
		{
			"[MODS] Error while executing ",
			this.eventName,
			" on mod \"",
			_currentMod.ModName,
			"\""
		}));
		Log.Exception(_e);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public ModEventAbs()
	{
	}

	public string eventName;

	[PublicizedFrom(EAccessModifier.Protected)]
	public readonly List<ModEventAbs<TDelegate>.Receiver> receivers = new List<ModEventAbs<TDelegate>.Receiver>();

	[PublicizedFrom(EAccessModifier.Protected)]
	public class Receiver
	{
		public string ModName
		{
			get
			{
				if (this.Mod != null)
				{
					return this.Mod.Name;
				}
				if (this.coreGame)
				{
					return "-GameCore-";
				}
				return "-UnknownMod-";
			}
		}

		public Receiver(Mod _mod, TDelegate _handler, bool _coreGame = false)
		{
			this.Mod = _mod;
			this.DelegateFunc = _handler;
			this.coreGame = _coreGame;
		}

		public readonly Mod Mod;

		public readonly TDelegate DelegateFunc;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly bool coreGame;
	}
}
