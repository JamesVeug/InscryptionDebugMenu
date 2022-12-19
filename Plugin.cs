using BepInEx;
using BepInEx.Logging;
using DebugMenu.Scripts;
using DebugMenu.Scripts.Act1;
using DebugMenu.Scripts.Act2;
using DebugMenu.Scripts.Act3;
using DebugMenu.Scripts.Acts;
using DebugMenu.Scripts.All;
using DebugMenu.Scripts.Grimora;
using DebugMenu.Scripts.Magnificus;
using DebugMenu.Scripts.Popups;
using HarmonyLib;
using UnityEngine;

namespace DebugMenu
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency("cyantist.inscryption.api", BepInDependency.DependencyFlags.HardDependency)]
    public class Plugin : BaseUnityPlugin
    {
	    public const string PluginGuid = "jamesgames.inscryption.debugmenu";
	    public const string PluginName = "Debug Menu";
	    public const string PluginVersion = "0.1.0.0";

	    public static Plugin Instance;
	    public static ManualLogSource Log;
	    
	    public static string PluginDirectory;
	    public static float StartingFixedDeltaTime;

	    public static List<BaseWindow> AllWindows = new List<BaseWindow>();
	    

        private void Awake()
        {
	        Logger.LogInfo($"Loading {PluginName}...");
	        Instance = this;
	        Log = Logger;
	        StartingFixedDeltaTime = Time.fixedDeltaTime;
	        
            PluginDirectory = this.Info.Location.Replace("DebugMenu.dll", "");

            new Harmony(PluginGuid).PatchAll();

            if (AllWindows.Count == 0)
            {
	            ToggleWindow<DebugWindow>();
            }

            Logger.LogInfo($"Loaded {PluginName}!");	        
        }

        private void Update()
        {
	        for (int i = 0; i < AllWindows.Count; i++)
	        {
		        AllWindows[i].Update();
	        }
        }

        private void OnGUI()
        {
	        for (int i = 0; i < AllWindows.Count; i++)
	        {
		        AllWindows[i].OnWindowGUI();
	        }
        }

        public void ToggleWindow<T>() where T : BaseWindow, new()
        {
	        for (int i = 0; i < AllWindows.Count; i++)
	        {
		        BaseWindow window = AllWindows[i];
		        if (window.GetType() == typeof(T))
		        {
			        AllWindows.RemoveAt(i);
			        return;
		        }
	        }

	        T t = new T();
	        AllWindows.Add(t);
        }
    }
}
