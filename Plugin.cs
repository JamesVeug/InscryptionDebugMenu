using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using DebugMenu.Scripts.Act3;
using DebugMenu.Scripts.Grimora;
using DebugMenu.Scripts.Hotkeys;
using DebugMenu.Scripts.Magnificus;
using DebugMenu.Scripts.Popups;
using HarmonyLib;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace DebugMenu
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency("cyantist.inscryption.api", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("zorro.inscryption.infiniscryption.p03kayceerun", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("arackulele.inscryption.grimoramod", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("silenceman.inscryption.magnificusmod", BepInDependency.DependencyFlags.SoftDependency)]
    public class Plugin : BaseUnityPlugin
    {
        public const string PluginGuid = "jamesgames.inscryption.debugmenu";
        public const string PluginName = "Debug Menu";
        public const string PluginVersion = "1.5.3";

        public static Plugin Instance;
        public static ManualLogSource Log;
        public static HotkeyController Hotkeys;
        internal static Harmony HarmonyInstance;

        public static string PluginDirectory;
        public static float StartingFixedDeltaTime;

        public static List<BaseWindow> AllWindows = new();

        private GameObject blockerParent = null;
        private Canvas blockerParentCanvas = null;
        private List<WindowBlocker> activeRectTransforms = new();
        private List<WindowBlocker> rectTransformPool = new();

        private void OnDisable() => HarmonyInstance.UnpatchSelf();
        private void Awake()
        {
            Instance = this;
            Log = Logger;
            StartingFixedDeltaTime = Time.fixedDeltaTime;
            Hotkeys = new HotkeyController();

            HarmonyInstance = new(PluginGuid);
            PluginDirectory = this.Info.Location.Replace("DebugMenu.dll", "");
            blockerParent = new("DebugMenuBlocker")
            {
                layer = LayerMask.NameToLayer("UI")
            };
            blockerParentCanvas = blockerParent.AddComponent<Canvas>();
            blockerParentCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            blockerParentCanvas.sortingOrder = 32767;
            blockerParent.AddComponent<CanvasScaler>();
            blockerParent.AddComponent<GraphicRaycaster>();
            DontDestroyOnLoad(blockerParent);

            GrimoraModHelper._enabled = Chainloader.PluginInfos.ContainsKey("arackulele.inscryption.grimoramod");
            P03ModHelper._enabled = Chainloader.PluginInfos.ContainsKey("zorro.inscryption.infiniscryption.p03kayceerun");
            MagnificusModHelper._enabled = Chainloader.PluginInfos.ContainsKey("silenceman.inscryption.magnificusmod");

            HarmonyInstance.PatchAll(Assembly.GetExecutingAssembly());

            if (GrimoraModHelper.Enabled)
            {
                Log.LogDebug("Patching Grimora Mod");
                GrimoraModHelper.PatchGrimoraMod();
            }
            if (P03ModHelper.Enabled)
            {
                Log.LogDebug("Patching P03 Kaycee's Mod");
                P03ModHelper.PatchP03Mod();
            }
            if (MagnificusModHelper.Enabled)
            {
                Log.LogDebug("Patching Magnificus Mod");
                MagnificusModHelper.PatchMagnificuMod();
            }

            // Get all types of BaseWindow, instntiate them and add them to allwindows
            Type[] types = Assembly.GetExecutingAssembly().GetTypes();
            for (int i = 0; i < types.Length; i++)
            {
                Type type = types[i];
                if (type.IsSubclassOf(typeof(BaseWindow)) && !type.IsAbstract)
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
                    if (AllWindows[i].IsActive)
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
                if (AllWindows[i].IsActive)
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
            GameObject myGO = new("WindowBlocker", typeof(RectTransform), typeof(WindowBlocker));
            myGO.transform.SetParent(blockerParent.transform);
            myGO.layer = LayerMask.NameToLayer("UI");

            Image image = myGO.AddComponent<Image>();
            Color color = Color.magenta;
            color.a = 0; // hides the image
            image.color = color;

            RectTransform blocker = myGO.GetComponent<RectTransform>();
            blocker.sizeDelta = new Vector2(Screen.width / 4, Screen.height / 4);
            blocker.anchoredPosition = Vector2.zero;
            blocker.pivot = new Vector2(0f, 1f);
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
