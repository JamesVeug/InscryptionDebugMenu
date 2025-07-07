using DebugMenu.Scripts.Act1;
using DebugMenu.Scripts.Act3;
using DebugMenu.Scripts.All;
using DebugMenu.Scripts.Grimora;
using DebugMenu.Scripts.Magnificus;
using DebugMenu.Scripts.Utils;
using DiskCardGame;
using GBC;
using HarmonyLib;
using InscryptionAPI;
using InscryptionAPI.Card;
using InscryptionAPI.Regions;
using System.Collections;
using System.Reflection;
using UnityEngine;

namespace DebugMenu;

[HarmonyPatch]
internal class TransitionFromGameState
{
    [HarmonyPostfix, HarmonyPatch(typeof(GameFlowManager), nameof(GameFlowManager.TransitionFrom))]
    private static IEnumerator PassOverBreaks(IEnumerator result, GameState gameState)
    {
        if (MagnificusModHelper.Enabled && SaveManager.SaveFile.IsMagnificus)
        {
            yield break;
        }
        yield return result;
    }
}
[HarmonyPatch(typeof(SpecialNodeHandler), nameof(SpecialNodeHandler.StartSpecialNodeSequence), new Type[] { typeof(SpecialNodeData) })]
internal class SpecialNodeHandler_StartSpecialNodeSequence
{
    private static bool Prefix(SpecialNodeData nodeData)
    {
        Helpers.LastSpecialNodeData = nodeData;
        return true;
    }
}

[HarmonyPatch(typeof(DisclaimerScreen), "BetaScreensSequence")]
internal class Skip_Disclaimer
{
    [HarmonyPrefix]
    private static bool SkipDisclaimerPrefix() => false;

    [HarmonyPostfix]
    private static IEnumerator SkipDisclaimerPostfix(IEnumerator previous, string ___nextSceneName)
    {
        yield return new WaitForSeconds(0f);
        SceneLoader.Load(___nextSceneName);
    }
}

[HarmonyPatch(typeof(AscensionSaveData), "NewRun")]
internal class SaveCardList
{
    [HarmonyPrefix]
    private static bool SaveCardListPrefix(List<CardInfo> starterDeck)
    {
        if (P03ModHelper.Enabled && P03ModHelper.IsP03Run)
            Act3.lastUsedStarterDeck = starterDeck;

        else if (GrimoraModHelper.Enabled && SaveFile.IsAscension)
            ActGrimora.lastUsedStarterDeck = starterDeck;

        else
            Act1.lastUsedStarterDeck = starterDeck;

        Plugin.Log.LogInfo("New starter deck with " + starterDeck.Count + " cards!");
        Configs.CurrentRegionOverride = string.Empty;
        return true;
    }
}

[HarmonyPatch(typeof(MapNodeManager), "DoMoveToNewNode")]
internal class MoveToNode_Debug
{
    [HarmonyPrefix]
    private static bool MoveToNodePrefix() => false;

    [HarmonyPostfix]
    private static IEnumerator MoveToNodePostfix(IEnumerator previous, MapNodeManager __instance, int ___transitioningGridY, MapNode newNode)
    {
        if (MagnificusModHelper.Enabled && SaveManager.SaveFile.IsMagnificus)
        {
            yield break;
        }
        __instance.MovingNodes = true;
        __instance.SetAllNodesInteractable(nodesInteractable: false);
        ___transitioningGridY = newNode.Data.gridY;
        Singleton<ViewManager>.Instance.Controller.LockState = ViewLockState.Locked;
        yield return PlayerMarker.Instance.MoveToPoint(newNode.transform.position);
        if (Act1.SkipNextNode)
        {
            __instance.MovingNodes = false;
            RunState.Run.currentNodeId = newNode.nodeId;
            Singleton<GameFlowManager>.Instance.TransitionToGameState(GameState.Map);
            yield break;
        }
        yield return newNode.OnArriveAtNode();
        Singleton<ViewManager>.Instance.Controller.LockState = ViewLockState.Unlocked;
        __instance.MovingNodes = false;
        CustomCoroutine.WaitOnConditionThenExecute(() => !Singleton<GameFlowManager>.Instance.Transitioning, delegate
        {
            RunState.Run.currentNodeId = newNode.nodeId;
        });
    }
}

[HarmonyPatch(typeof(MapNode), nameof(MapNode.SetActive), new Type[] { typeof(bool) })]
internal class MapNode_SetActive
{
    [HarmonyPostfix]
    private static bool Prefix(MapNode __instance, ref bool active)
    {
        if (Act1.ActivateAllMapNodesActive)
        {
            active = true;
        }
        return true;
    }
}

[HarmonyPatch]
internal class InputButtons_Buttons
{
    public static IEnumerable<MethodBase> TargetMethods()
    {
        Type inputButtons = typeof(InputButtons);
        yield return AccessTools.Method(inputButtons, nameof(InputButtons.GetButton));
        yield return AccessTools.Method(inputButtons, nameof(InputButtons.GetButtonDown));
        yield return AccessTools.Method(inputButtons, nameof(InputButtons.GetButtonUp));
        yield return AccessTools.Method(inputButtons, nameof(InputButtons.GetButtonRepeating));
        yield return AccessTools.Method(inputButtons, nameof(InputButtons.AnyGamepadButton));
    }

    [HarmonyPostfix]
    private static void Postfix(ref bool __result)
    {
        if (AllActs.IsInputBlocked())
        {
            __result = false;
        }
    }
}

[HarmonyPatch]
internal class InputButtons_Axis
{
    public static IEnumerable<MethodBase> TargetMethods()
    {
        Type inputButtons = typeof(InputButtons);
        yield return AccessTools.Method(inputButtons, nameof(InputButtons.GetAxis));
    }

    [HarmonyPostfix]
    private static void Postfix(ref float __result)
    {
        if (AllActs.IsInputBlocked())
        {
            __result = 0.0f;
        }
    }
}


[HarmonyPatch(typeof(RegionManager), "GetAllRegionsForMapGeneration")]
internal class RegionManager_GetRandomRegionFromTier
{
    [HarmonyPostfix]
    private static bool Prefix(ref List<RegionData> __result)
    {
        if (Act1MapSequence.RegionOverride)
        {
            RegionData data = RegionManager.AllRegionsCopy.Find((a) => a.name == Act1MapSequence.RegionNameOverride);
            if (data != null) {
                __result.Clear();
                __result.Add(data);
                return false;
            }

            Plugin.Log.LogWarning($"Could not override map, region [{Act1MapSequence.RegionNameOverride}] could not be found!");
        }
        return true;
    }
}

[HarmonyPatch]
[HarmonyAfter(InscryptionAPIPlugin.ModGUID)]
internal class OtherRegionPatches {
    [HarmonyPostfix, HarmonyPatch(typeof(RunState), nameof(RunState.CurrentMapRegion), MethodType.Getter)]
    private static void FixRegionOverride(ref RegionData __result) {
        if (!string.IsNullOrEmpty(Configs.CurrentRegionOverride) && SaveManager.SaveFile.IsPart1) {
            RegionData data = RegionManager.AllRegionsCopy.Find((a) => a.name == Act1MapSequence.RegionNameOverride);
            if (data != null) {
                __result = data;
            }
            else {
                Configs.CurrentRegionOverride = string.Empty;
            }
        }
    }

    [HarmonyPostfix, HarmonyPatch(typeof(RunState), nameof(RunState.NextRegion))]
    private static void ResetCurrentRegionOverride() {
        Configs.CurrentRegionOverride = string.Empty;
    }
}

[HarmonyPatch(typeof(MapNodeManager), nameof(MapNodeManager.GetNodeWithId), new Type[] { typeof(int) })]
internal class MapNodeManager_GetNodeWithId
{
    [HarmonyPrefix]
    private static bool MoveToNodePrefix(MapNodeManager __instance, ref MapNode __result, int id)
    {
        __result = __instance.nodes.Find((x) => x != null && x.nodeId == id);
        return false;
    }
}


[HarmonyPatch]
internal class DisableDialogue_IEnumerator_Patch
{
    private static IEnumerable<MethodBase> TargetMethods()
    {
        yield return AccessTools.Method(typeof(TextDisplayer), nameof(TextDisplayer.PlayDialogueEvent));
        yield return AccessTools.Method(typeof(TextDisplayer), nameof(TextDisplayer.ShowUntilInput));
        yield return AccessTools.Method(typeof(TextDisplayer), nameof(TextDisplayer.ShowThenClear));
        yield return AccessTools.Method(typeof(CardSingleChoicesSequencer), nameof(CardSingleChoicesSequencer.TutorialTextSequence));
        yield return AccessTools.Method(typeof(GainConsumablesSequencer), nameof(GainConsumablesSequencer.LearnObjectSequence));
        yield return AccessTools.Method(typeof(DialogueHandler), nameof(DialogueHandler.PlayDialogueEvent));
        yield return AccessTools.Method(typeof(TextBox), nameof(TextBox.ShowUntilInput));
    }

    private static IEnumerator Postfix(IEnumerator enumerator)
    {
        if (Configs.DisableDialogue)
        {
            Singleton<InteractionCursor>.Instance.InteractionDisabled = false;
            yield break;
        }

        yield return enumerator;
    }
}

[HarmonyPatch]
internal class DisablePlayerDamagePatch
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(LifeManager), nameof(LifeManager.ShowDamageSequence))]
    private static IEnumerator PlayersReceiveNoDamage(IEnumerator enumerator, bool toPlayer)
    {
        if (Configs.DisablePlayerDamage && toPlayer)
            yield break;

        if (Configs.DisableOpponentDamage && !toPlayer)
            yield break;

        yield return enumerator;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(MagnificusLifeManager), nameof(MagnificusLifeManager.ShowLifeLoss))]
    private static IEnumerator PlayersReceiveNoDamageMagnificus(IEnumerator enumerator, bool player)
    {
        if (Configs.DisablePlayerDamage && player)
        {
            yield break;
        }

        if (Configs.DisableOpponentDamage && !player)
        {
            yield break;
        }

        yield return enumerator;
    }
}

[HarmonyPatch]
internal class DisableDialogue_Patch
{
    private static IEnumerable<MethodBase> TargetMethods()
    {
        yield return AccessTools.Method(typeof(TextDisplayer), nameof(TextDisplayer.ShowMessage));
    }

    private static bool Prefix() => !Configs.DisableDialogue;
}

[HarmonyPatch]
internal class EmissionAndPortraitPatches
{
    [HarmonyPostfix, HarmonyPatch(typeof(CardDisplayer3D), nameof(CardDisplayer3D.EmissionEnabledForCard))]
    private static void ForceEmission(CardRenderInfo renderInfo, PlayableCard playableCard, ref bool __result)
    {
        if (renderInfo.baseInfo.Mods.Exists(x => x.singletonId == DrawCardInfo.EmissionMod) || (playableCard != null && playableCard.temporaryMods.Exists(x => x.fromCardMerge)))
            __result = true;
    }

    [HarmonyPostfix, HarmonyPatch(typeof(CardDisplayer), nameof(CardDisplayer.DisplayInfo))]
    private static void ForceAlternatePortrait(CardDisplayer __instance, CardRenderInfo renderInfo)
    {
        if (__instance == null || renderInfo?.baseInfo == null)
            return;

        List<CardModificationInfo> mods = renderInfo.baseInfo.Mods;
        if (mods.Exists(x => x.singletonId == DrawCardInfo.PortraitMod))
        {
            __instance.SetPortrait(SaveManager.SaveFile.IsPart2 ? renderInfo.baseInfo.PixelAlternatePortrait() : renderInfo.baseInfo.alternatePortrait);
        }
        else if (mods.Exists(x => x.singletonId == DrawCardInfo.ShieldPortraitMod))
        {
            __instance.SetPortrait(SaveManager.SaveFile.IsPart2 ? renderInfo.baseInfo.PixelBrokenShieldPortrait() : renderInfo.baseInfo.BrokenShieldPortrait());
        }
        else if (mods.Exists(x => x.singletonId == DrawCardInfo.SacrificePortraitMod))
        {
            __instance.SetPortrait(SaveManager.SaveFile.IsPart2 ? renderInfo.baseInfo.PixelSacrificablePortrait() : renderInfo.baseInfo.SacrificablePortrait());
        }
        else if (mods.Exists(x => x.singletonId == DrawCardInfo.TrapPortraitMod))
        {
            __instance.SetPortrait(SaveManager.SaveFile.IsPart2 ? renderInfo.baseInfo.PixelSteelTrapPortrait() : renderInfo.baseInfo.SteelTrapPortrait());
        }
    }
    [HarmonyPostfix, HarmonyPatch(typeof(PixelCardDisplayer), nameof(PixelCardDisplayer.DisplayInfo))]
    private static void ForcePixelAlternatePortrait(PixelCardDisplayer __instance, CardRenderInfo renderInfo)
    {
        if (__instance == null || renderInfo?.baseInfo == null)
            return;

        List<CardModificationInfo> mods = renderInfo.baseInfo.Mods;
        if (mods.Exists(x => x.singletonId == DrawCardInfo.PortraitMod))
        {
            __instance.SetPortrait(renderInfo.baseInfo.PixelAlternatePortrait());
        }
        else if (mods.Exists(x => x.singletonId == DrawCardInfo.ShieldPortraitMod))
        {
            __instance.SetPortrait(renderInfo.baseInfo.PixelBrokenShieldPortrait());
        }
        else if (mods.Exists(x => x.singletonId == DrawCardInfo.SacrificePortraitMod))
        {
            __instance.SetPortrait(renderInfo.baseInfo.PixelSacrificablePortrait());
        }
        else if (mods.Exists(x => x.singletonId == DrawCardInfo.TrapPortraitMod))
        {
            __instance.SetPortrait(renderInfo.baseInfo.PixelSteelTrapPortrait());
        }
    }
}

/*
 NOTE: Need to fix this. It does block but it spams sounds at the same time. Need to find out why
 [HarmonyPatch]
internal class InteractionCursor_RaycastForInteractable
{
    static Type InteractionCursor = Type.GetType("DiskCardGame.InteractionCursor, Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
    
    static MethodInfo InputBlockedInfo = AccessTools.Method(typeof(InteractionCursor_RaycastForInteractable), nameof(InputBlocked), new Type[] {});
    
    public static IEnumerable<MethodBase> TargetMethods()
    {
        yield return AccessTools.Method(InteractionCursor, "RaycastForInteractable").MakeGenericMethod(typeof(InteractableBase));
    }

    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        // === We want to turn this

        // T result = default(T);
        // ...

        // === Into this

        // T result = default(T);
        // if(!InputBlocked()){
        //	...
        // }

        // ===
        
        List<CodeInstruction> codes = new List<CodeInstruction>(instructions);

        object gotoOperand = null;
        for (int i = 0; i < codes.Count; i++)
        {
	        CodeInstruction codeInstruction = codes[i];
	        if (codeInstruction.opcode == OpCodes.Brfalse)
	        {
		        gotoOperand = codeInstruction.operand;
		        break;
	        }
        }
        
        for (int i = 0; i < codes.Count; i++)
        {
            CodeInstruction codeInstruction = codes[i];
            if (codeInstruction.opcode == OpCodes.Ldarg_0)
            {
                // Call CustomSwitchToView instead of View
                codes.Insert(i++, new CodeInstruction(OpCodes.Callvirt, InputBlockedInfo)); 
                codes.Insert(i++, new CodeInstruction(OpCodes.Brtrue, gotoOperand));
                break;
            }
        }
        
        return codes;
    }

    public static bool InputBlocked()
    {
	    return AllActs.blockAllInput;
    }
}*/
