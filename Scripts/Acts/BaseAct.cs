using BepInEx.Logging;
using UnityEngine;

namespace DebugMenu.Scripts.Acts;

public abstract class BaseAct
{
	protected readonly ManualLogSource Logger;

	public BaseAct(ManualLogSource logger)
	{
		Logger = logger;
	}

	public abstract void Update();
	
	public abstract void OnGUI();
	
	public abstract void OnGUIReload();
	public abstract void OnGUIRestart();

	public void Log(string log)
	{
		Logger.LogInfo(log);
	}
	
	public void Warning(string log)
	{
		Logger.LogWarning(log);
	}
	
	public void Error(string log)
	{
		Logger.LogError(log);
	}
}