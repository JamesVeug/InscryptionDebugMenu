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
	    
	    private bool modUIOpen = true;

	    private BaseAct CurrentAct = null;

	    private AllActs allActs;
	    private Act1 act1;
	    private Act2 act2;
	    private Act3 act3;
	    private ActGrimora actGrimora;
	    private ActMagnificus actMagnificus;

        private void Awake()
        {
	        Logger.LogInfo($"Loading {PluginName}...");
	        Instance = this;
	        Log = Logger;
	        StartingFixedDeltaTime = Time.fixedDeltaTime;
	        
	        allActs = new AllActs(Logger);
	        act1 = new Act1(Logger);
	        act2 = new Act2(Logger);
	        act3 = new Act3(Logger);
	        actGrimora = new ActGrimora(Logger);
	        actMagnificus = new ActMagnificus(Logger);
            PluginDirectory = this.Info.Location.Replace("DebugMenu.dll", "");

            new Harmony(PluginGuid).PatchAll();

            Logger.LogInfo($"Loaded {PluginName}!");	        
        }

        private void Update()
        {
	        if (SaveManager.SaveFile.IsPart1)
	        {
		        // Leshy
		        CurrentAct = act1;
	        }
	        else if (SaveManager.SaveFile.IsPart2)
	        {
		        // GDC
		        CurrentAct = act2;
	        }
	        else if (SaveManager.SaveFile.IsPart3)
	        {
		        // PO3
		        CurrentAct = act3;
	        }
	        else if (SaveManager.SaveFile.IsGrimora)
	        {
		        // Grimora
		        CurrentAct = actGrimora;
	        }
	        else if (SaveManager.SaveFile.IsMagnificus)
	        {
		        // Magnificus
		        CurrentAct = actMagnificus;
	        }
	        else
	        {
		        // In main menu maybe???
		        CurrentAct = null;
	        }

	        if (CurrentAct != null)
	        {
		        CurrentAct.Update();
	        }
        }

        private void OnGUI()
        {
	        GUIHelper.Reset();
	        
	        GUIHelper.Toggle("Show Debug Menu", ref modUIOpen);
	        if (!modUIOpen) 
		        return;
	        
	        allActs.OnGUI();
	        if (CurrentAct != null)
	        {
		        CurrentAct.OnGUIReload();
		        CurrentAct.OnGUIRestart();
	        }
	        
	        GUIHelper.StartNewColumn();
	        if (CurrentAct != null)
	        {
		        CurrentAct.OnGUI();
	        }
        }
    }
}
