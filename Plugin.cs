using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using DebugMenu.Scripts.Hotkeys;
using DebugMenu.Scripts.Popups;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace DebugMenu
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency("cyantist.inscryption.api", BepInDependency.DependencyFlags.HardDependency)]
    public class Plugin : BaseUnityPlugin
    {
	    public const string PluginGuid = "jamesgames.inscryption.debugmenu";
	    public const string PluginName = "Debug Menu";
	    public const string PluginVersion = "1.3.0";

	    public static Plugin Instance;
	    public static ManualLogSource Log;
	    public static HotkeyController Hotkeys;
	    
	    public static string PluginDirectory;
	    public static float StartingFixedDeltaTime;

	    public static List<BaseWindow> AllWindows = new();
	    
	    private GameObject blockerParent = null;
	    private Canvas blockerParentCanvas = null;
	    private List<WindowBlocker> activeRectTransforms = new List<WindowBlocker>();
	    private List<WindowBlocker> rectTransformPool = new List<WindowBlocker>();

        private void Awake()
        {
	        Instance = this;
	        Log = Logger;
	        StartingFixedDeltaTime = Time.fixedDeltaTime;
	        Hotkeys = new HotkeyController();
	        
            PluginDirectory = this.Info.Location.Replace("DebugMenu.dll", "");

            blockerParent = new GameObject("DebugMenuBlocker");
            blockerParent.layer = LayerMask.NameToLayer("UI");
            blockerParentCanvas = blockerParent.AddComponent<Canvas>();
            blockerParentCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            blockerParentCanvas.sortingOrder = 32767;
            blockerParent.AddComponent<CanvasScaler>();
            blockerParent.AddComponent<GraphicRaycaster>();
            DontDestroyOnLoad(blockerParent);
            
            new Harmony(PluginGuid).PatchAll();

            // Get all types of BaseWindow, instntiate them and add them to allwindows
            Type[] types = Assembly.GetExecutingAssembly().GetTypes();
            for (int i = 0; i < types.Length; i++)
			{
	            Type type = types[i];
	            if (type.IsSubclassOf(typeof(BaseWindow)))
	            {
		            Logger.LogDebug($"Made {type}!");	   
		            AllWindows.Add((BaseWindow)Activator.CreateInstance(type));
	            }
			}

            Logger.LogInfo($"Loaded {PluginName}!");	        
        }

        private void Update()
        {
	        if (Configs.ShowDebugMenu)
	        {
		        for (int i = 0; i < AllWindows.Count; i++)
		        {
			        if(AllWindows[i].IsActive)
						AllWindows[i].Update();
		        }
	        }

	        Hotkeys.Update();
        }

        private void OnGUI()
        {
	        if (!Configs.ShowDebugMenu)
		        return;
	        
	        for (int i = 0; i < AllWindows.Count; i++)
	        {
		        if(AllWindows[i].IsActive)
					AllWindows[i].OnWindowGUI();
	        }
        }

        public T ToggleWindow<T>() where T : BaseWindow, new()
        {
	        return (T)ToggleWindow(typeof(T));
        }

        public BaseWindow ToggleWindow(Type t)
        {
	        for (int i = 0; i < AllWindows.Count; i++)
	        {
		        BaseWindow window = AllWindows[i];
		        if (window.GetType() == t)
		        {
			        window.IsActive = !window.IsActive;
			        return window;
		        }
	        }

	        return null;
        }
        
        public T GetWindow<T>() where T : BaseWindow, new()
		{
			return (T)GetWindow(typeof(T));
		}

        public BaseWindow GetWindow(Type t)
        {
            for (int i = 0; i < AllWindows.Count; i++)
            {
                BaseWindow window = AllWindows[i];
                if (window.GetType() == t)
                    return window;
            }

            return null;
        }

        public WindowBlocker CreateWindowBlocker()
        {
	        GameObject myGO = new GameObject("WindowBlocker", typeof(RectTransform), typeof(WindowBlocker));
	        myGO.transform.SetParent(blockerParent.transform);
	        myGO.layer = LayerMask.NameToLayer("UI");
		        
	        Image image = myGO.AddComponent<Image>();
	        Color color = Color.magenta;
	        color.a = 0; // Uncomment to hide the visuals of this image
	        image.color = color;
		        
	        RectTransform blocker = myGO.GetComponent<RectTransform>();
	        blocker.sizeDelta = new Vector2(Screen.width / 4, Screen.height / 4);
	        blocker.anchoredPosition = Vector2.zero;
	        blocker.pivot = new Vector2(0.0f, 1.0f);
	        blocker.anchorMin = Vector2.zero;
	        blocker.anchorMax = Vector2.zero;

		        
	        WindowBlocker windowBlocker = myGO.GetComponent<WindowBlocker>();
	        activeRectTransforms.Add(windowBlocker);
	        return windowBlocker;
        }

        public bool IsInputBlocked()
        {
	        if (!Configs.ShowDebugMenu)
		        return false;
	        
	        foreach (WindowBlocker rectTransform in activeRectTransforms)
	        {
		        if (rectTransform.isHovered)
		        {
			        return true;
		        }
	        }

	        return false;
        }
    }
}
