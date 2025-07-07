using DebugMenu.Scripts.Acts;
using DiskCardGame;
using GrimoraMod;
using GrimoraMod.Saving;
using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace DebugMenu.Scripts.Grimora;

public static partial class GrimoraModHelper
{
    private static string FirstBoon = "";
    private static string SecondBoon = "";
    private static string _BoonToAdd = "";

    private static bool ModifyingFirst = true;
    private static bool GeneratingBoons = false;

    private static void ChangeBoonButton(string Boon, DebugWindow Window, ConfirmStoneButton confirmStone, ConfirmStoneButton rejectStone, MeshRenderer renderer, Material material, Color col1, Color col2, Color col3)
    {
        if (Window.Button(Boon.Replace("Boon_", ""), disabled: () => DisableBoon(Boon, confirmStone, rejectStone)))
        {
            ConfirmStoneButton target;
            if (ModifyingFirst)
            {
                FirstBoon = Boon;
                target = confirmStone;
            }
            else
            {
                SecondBoon = Boon;
                target = rejectStone;
            }

            renderer.material = material;
            target.Enter();
            target.SetColors(col1, col2, col3);
        }
    }
    internal static void OnGUIGoatEye(DebugWindow Window)
    {
        GoatEyeSequencer sequencer = GoatEyeSequencer.Instance;
        if (sequencer == null)
            return;

        ConfirmStoneButton confirmStone = (ConfirmStoneButton)typeof(GoatEyeSequencer).GetField("confirmStone", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(sequencer);
        ConfirmStoneButton rejectStone = (ConfirmStoneButton)typeof(GoatEyeSequencer).GetField("rejectStone", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(sequencer);
        MeshRenderer stoneQuad = (MeshRenderer)typeof(GoatEyeSequencer).GetField("StoneQuad", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(sequencer);
        MeshRenderer rejectQuad = (MeshRenderer)typeof(GoatEyeSequencer).GetField("rejectQuad", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(sequencer);

        using (Window.HorizontalScope(2))
        {
            Window.Label($"<b>First Boon:</b> {FirstBoon.Replace("Boon_", "")}");
            Window.Label($"<b>Second Boon:</b> {SecondBoon.Replace("Boon_", "")}");
        }

        Window.Toggle("Modify First Boon", ref ModifyingFirst);
        MeshRenderer targetRend = ModifyingFirst ? stoneQuad : rejectQuad;

        using (Window.HorizontalScope(2))
        {
            Window.Label($"<b>Starting Bones:</b>\n {Mathf.Max(0, GrimoraRunState.CurrentRun.regionTier - GrimoraRunState.CurrentRun.riggedDraws.Count)}");
            ChangeBoonButton("Boon_StartingDraw", Window, confirmStone, rejectStone, targetRend,
                AssetConstants.Boon1, GameColors.instance.darkSeafoam, GameColors.instance.darkSeafoam, GameColors.instance.brightSeafoam);
        }
        using (Window.HorizontalScope(2))
        {
            ChangeBoonButton("Boon_TerrainBones", Window, confirmStone, rejectStone, targetRend,
                AssetConstants.Boon2, GameColors.instance.darkGold, GameColors.instance.darkGold, GameColors.instance.brightGold);

            ChangeBoonButton("Boon_MaxEnergy", Window, confirmStone, rejectStone, targetRend,
                AssetConstants.Boon3, GameColors.instance.darkBlue, GameColors.instance.darkBlue, GameColors.instance.brightBlue);
        }
        using (Window.HorizontalScope(2))
        {
            ChangeBoonButton("Boon_BossBones", Window, confirmStone, rejectStone, targetRend,
                AssetConstants.Boon4, GameColors.instance.red, GameColors.instance.red, GameColors.instance.glowRed);

            ChangeBoonButton("Boon_EgyptCards", Window, confirmStone, rejectStone, targetRend,
                AssetConstants.Boon5, GameColors.instance.yellow, GameColors.instance.yellow, GameColors.instance.brightGold);
        }
        using (Window.HorizontalScope(2))
        {
            ChangeBoonButton("Boon_Pirates", Window, confirmStone, rejectStone, targetRend,
                AssetConstants.Boon6, GameColors.instance.brightBlue, GameColors.instance.brightBlue, GameColors.instance.glowSeafoam);

            ChangeBoonButton("Boon_TerrainSpawn", Window, confirmStone, rejectStone, targetRend,
                AssetConstants.Boon7, GameColors.instance.brightBlue, GameColors.instance.brightBlue, GameColors.instance.glowSeafoam);
        }
    }
    private static DrawableGUI.ButtonDisabledData DisableBoon(string boon, ConfirmStoneButton confirm, ConfirmStoneButton reject)
    {
        return new DrawableGUI.ButtonDisabledData(delegate
        {
            if (FirstBoon == boon || SecondBoon == boon)
                return true;

            if (GrimoraRunState.CurrentRun.riggedDraws.Contains(boon))
                return true;

            if (ModifyingFirst)
                return string.IsNullOrEmpty(FirstBoon) || confirm.currentState == HighlightedInteractable.State.NonInteractable;
            else
                return string.IsNullOrEmpty(SecondBoon) || reject.currentState == HighlightedInteractable.State.NonInteractable;
        });
    }

    [HarmonyPostfix, HarmonyPatch(typeof(GoatEyeSequencer), "EndSequence")]
    private static void ModifyFirstChoice()
    {
        if (!string.IsNullOrEmpty(FirstBoon) && FirstBoon != _BoonToAdd)
        {
            GrimoraRunState.CurrentRun.riggedDraws.Remove(_BoonToAdd);
            GrimoraRunState.CurrentRun.riggedDraws.Add(FirstBoon);
        }
    }
    [HarmonyPrefix, HarmonyPatch(typeof(GoatEyeSequencer), "ChooseBoon2")]
    private static void ModifySecondChoice(ref string Boon)
    {
        if (!string.IsNullOrEmpty(SecondBoon))
            Boon = SecondBoon;
    }

    [HarmonyPrefix, HarmonyPatch(typeof(GoatEyeSequencer), "StartSequence")]
    private static void ResetBoonCheck()
    {
        GeneratingBoons = true;
        FirstBoon = _BoonToAdd = SecondBoon = "";
    }

    private static class DetectBoon
    {
        private static MethodBase TargetMethod()
        {
            return typeof(CardRelatedExtension).GetMethod(nameof(CardRelatedExtension.GetRandomItem)).MakeGenericMethod(typeof(string));
        }
        private static void Postfix(string __result)
        {
            if (__result == null || !GeneratingBoons)
                return;

            if (string.IsNullOrEmpty(FirstBoon))
            {
                FirstBoon = __result;
                _BoonToAdd = __result;
            }
            else
            {
                SecondBoon = __result;
                GeneratingBoons = false;
            }
        }
    }
}