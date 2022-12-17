using System.Collections;
using System.Reflection;
using System.Reflection.Emit;
using DebugMenu.Scripts.All;
using DiskCardGame;
using HarmonyLib;
using UnityEngine;

namespace DebugMenu.Scripts.SpeedTweeks;

[HarmonyPatch(typeof(DisclaimerScreen), "BetaScreensSequence")]
internal class Skip_Disclaimer
{
	[HarmonyPrefix]
	private static bool SkipDisclaimerPrefix()
	{
		return false;
	}

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
		Act1.Act1.lastUsedStarterDeck = starterDeck;
		Plugin.Log.LogInfo("New Starter Deck! With " + Act1.Act1.lastUsedStarterDeck.Count + " Cards!");
		return true;
	}

	[HarmonyPostfix]
	private static void SaveCardListPostfix()
	{
	}
}

[HarmonyPatch(typeof(MapNodeManager), "DoMoveToNewNode")]
internal class MoveToNode_Debug
{
	[HarmonyPrefix]
	private static bool MoveToNodePrefix()
	{
		return false;
	}

	[HarmonyPostfix]
	private static IEnumerator MoveToNodePostfix(IEnumerator previous, MapNodeManager __instance, int ___transitioningGridY, MapNode newNode)
	{
		__instance.MovingNodes = true;
		__instance.SetAllNodesInteractable(nodesInteractable: false);
		___transitioningGridY = newNode.Data.gridY;
		Singleton<ViewManager>.Instance.Controller.LockState = ViewLockState.Locked;
		yield return PlayerMarker.Instance.MoveToPoint(newNode.transform.position);
		if (Act1.Act1.isNodeDebugModeActive)
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

[HarmonyPatch(typeof(MapNode), nameof(MapNode.SetActive), new Type[]{typeof(bool)})]
internal class MapNode_SetActive
{
	[HarmonyPostfix]
	private static bool Prefix(MapNode __instance, ref bool active)
	{
		if (Act1.Act1.activateAllMapNodesActive)
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
		if (AllActs.blockAllInput)
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
		if (AllActs.blockAllInput)
		{
			__result = 0.0f;
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
