using DebugMenu.Scripts.Utils;
using DiskCardGame;
using HarmonyLib;

namespace DebugMenu.Scripts.Grimora;

[HarmonyPatch(typeof(SpecialNodeHandler), nameof(SpecialNodeHandler.StartSpecialNodeSequence), new Type[] { typeof(SpecialNodeData) })]
internal class SpecialNodeHandler_StartSpecialNodeSequence
{
    private static bool Prefix(SpecialNodeData nodeData)
    {
        Helpers.LastSpecialNodeData = nodeData;
        return true;
    }
}
