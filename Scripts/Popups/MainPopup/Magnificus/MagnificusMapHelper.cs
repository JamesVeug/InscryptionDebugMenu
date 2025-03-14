using DiskCardGame;
using GrimoraMod;
using HarmonyLib;
using MagnificusMod;
using System.Collections;
using System.Reflection;

namespace DebugMenu.Scripts.Magnificus;

public static partial class MagnificusModHelper
{
    [HarmonyPostfix, HarmonyPatch(typeof(NavigationZone3D), nameof(NavigationZone3D.ValidEvent))]
    private static void PreventEventTriggering(ref bool __result)
    {
        if (SaveManager.SaveFile.IsMagnificus && config.isometricMode && ActMagnificus.SkipNextNode)
        {
            __result = false;
        }
    }
    [HarmonyPrefix, HarmonyPatch(typeof(FirstPersonController), nameof(FirstPersonController.UpdateNavigationEvents))]
    private static bool PreventEventTriggering2()
    {
        if (SaveManager.SaveFile.IsMagnificus && config.isometricMode && ActMagnificus.SkipNextNode)
        {
            return false;
        }
        return true;
    }

    [HarmonyPrefix, HarmonyPatch(typeof(Generation), nameof(Generation.unIsometricTransition))]
    private static bool ResetGameStateToMap()
    {
        GameFlowManager.Instance.CurrentGameState = GameState.Map;
        return true;
    }

    [HarmonyPrefix, HarmonyPatch(typeof(Generation), nameof(Generation.transition))]
    private static bool ResetGameStateToMap(string location)
    {
        if (location != "tower")
            GameFlowManager.Instance.CurrentGameState = GameState.Map;

        return true;
    }
}
