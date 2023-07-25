using System.Reflection;
using BepInEx;
using BepInEx.Bootstrap;
using DiskCardGame;

namespace DebugMenu.Scripts.Utils;

public static class GrimoraModHelpers
{
	private static Assembly GrimoraModAssembly = null;
	private static PropertyInfo IsGrimoraModRunProperty = null;
	
	public static RunState GetRunState()
	{
		return RunState.Run;
	}

	private static void Initialize(Assembly assembly)
	{
		GrimoraModAssembly = assembly;

		IsGrimoraModRunProperty = GrimoraModAssembly.GetType("GrimoraMod.GrimoraSaveUtil")?.GetProperty("IsGrimoraModRun");
	}
	
	public static bool GrimoraModIsActive()
	{
		if (GrimoraModAssembly == null)
		{
			foreach (KeyValuePair<string,PluginInfo> pair in Chainloader.PluginInfos)
			{
				if (pair.Value.Metadata.GUID == "arackulele.inscryption.grimoramod")
				{
					Plugin.Log.LogInfo($"[GrimoraModHelpers] GrimoraMod found!");
					Initialize(pair.Value.Instance.GetType().Assembly);
				}
			}

			if (GrimoraModAssembly == null)
			{
				return false;
			}
		}
		
		return (bool) IsGrimoraModRunProperty.GetValue(null);
	}
	
}