namespace DebugMenu.Scripts.Grimora;

public static partial class GrimoraModHelper
{
    internal static bool _enabled;
    public static bool Enabled => _enabled;
    internal static void PatchGrimoraMod()
    {
        Plugin.HarmonyInstance.PatchAll(typeof(GrimoraModHelper));
        Plugin.HarmonyInstance.PatchAll(typeof(DetectBoon));
    }

    internal static bool addingMod = false;
}
