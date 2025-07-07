using DebugMenu.Scripts.Acts;
using DiskCardGame;
using GrimoraMod;
using HarmonyLib;
using System.Collections;
using UnityEngine;

namespace DebugMenu.Scripts.Grimora;

public static partial class GrimoraModHelper
{
    internal static float CurrentDeathChance = 0f;
    internal static float ModifyDeathChance = 0f;
    internal static bool DisableChairDeath = false;

    internal static void OnGUIElectricChairNodeSequence(DebugWindow Window)
    {
        ElectricChairSequencer sequencer = ElectricChairSequencer.Instance;
        using (Window.HorizontalScope(3))
        {
            Window.LabelBold("Chance to die:");
            if (Window.Button("+0.01"))
            {
                ModifyDeathChance += 0.01f;
                CurrentDeathChance += 0.01f;
            }
            if (Window.Button("+0.1"))
            {
                ModifyDeathChance += 0.1f;
                CurrentDeathChance += 0.1f;
            }
        }
        using (Window.HorizontalScope(3))
        {
            Window.Label($"{CurrentDeathChance:0.0}");
            if (Window.Button("-0.01"))
            {
                ModifyDeathChance -= 0.01f;
                CurrentDeathChance -= 0.01f;
            }
            if (Window.Button("-0.1"))
            {
                ModifyDeathChance -= 0.1f;
                CurrentDeathChance -= 0.1f;
            }
        }

        if (Window.Button("Apply Mod to Card", disabled: () => new(() => addingMod || sequencer.confirmStone.currentState != HighlightedInteractable.State.Interactable || sequencer.selectionSlot.Card == null)))
        {
            addingMod = true;
            Plugin.Instance.StartCoroutine(ElectricChairMod(sequencer, sequencer.selectionSlot.Card.Info));
        }
        Window.Toggle("No Chance Increase", ref DisableChairDeath);
    }

    private static IEnumerator ElectricChairMod(ElectricChairSequencer sequencer, CardInfo card)
    {
        AudioController.Instance.PlaySound3D("teslacoil_charge", MixerGroup.TableObjectsSFX, sequencer.selectionSlot.transform.position, 1f, 0.5f);
        GrimoraModHelper.ElectricChairApplyModToCard(sequencer, card);
        sequencer.selectionSlot.Card.Anim.PlayTransformAnimation();
        yield return new WaitForSeconds(0.15f);
        sequencer.selectionSlot.Card.StatsLayer.SetEmissionColor(GameColors.Instance.blue);
        sequencer.selectionSlot.Card.SetInfo(card);
        sequencer.selectionSlot.Card.SetInteractionEnabled(true);
        yield return new WaitForSeconds(0.75f);
        addingMod = false;
    }

    [HarmonyReversePatch, HarmonyPatch(typeof(ElectricChairSequencer), "ApplyModToCard")]
    public static void ElectricChairApplyModToCard(ElectricChairSequencer instance, CardInfo card) { }

    [HarmonyPostfix, HarmonyPatch(typeof(ElectricChairSequencer), "StartSequence")]
    public static void ResetTrackedDeathChance() => CurrentDeathChance = 0f;

    [HarmonyPostfix, HarmonyPatch(typeof(ElectricChairSequencer), "GetInitialChanceToDie")]
    private static void TrackInitialDeathChance(ref float __result)
    {
        if (DisableChairDeath)
            __result = 0;
        else if (ModifyDeathChance != 0f)
        {
            __result = ModifyDeathChance;
            ModifyDeathChance = 0f;
        }
        CurrentDeathChance = __result;
    }

    [HarmonyPostfix, HarmonyPatch(typeof(ElectricChairSequencer), "AddChanceToDieForSecondZap")]
    private static void TrackAddDeathChance(ref float __result)
    {
        if (DisableChairDeath)
        {
            __result -= CurrentDeathChance;
        }
        else if (ModifyDeathChance != 0f)
        {
            __result = ModifyDeathChance;
            ModifyDeathChance = 0f;
        }
        else
        {
            CurrentDeathChance += __result;
        }
    }
}