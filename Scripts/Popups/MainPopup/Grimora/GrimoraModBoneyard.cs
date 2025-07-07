using DebugMenu.Scripts.Acts;
using DiskCardGame;
using GrimoraMod;
using HarmonyLib;
using System.Collections;
using System.Reflection;
using UnityEngine;

namespace DebugMenu.Scripts.Grimora;

public static partial class GrimoraModHelper
{
    internal static int CurrentBoneyardEffect;

    internal static string EffectName(int effect)
    {
        return effect switch
        {
            2 => "Starve",
            3 => "Energy",
            4 => "Prisoner",
            5 => "Haunter",
            _ => "Brittle"
        };
    }
    internal static void OnGUIBoneyardBurial(DebugWindow Window)
    {
        BoneyardBurialSequencer sequencer = SpecialNodeHandler.Instance.cardStatBoostSequencer as BoneyardBurialSequencer;
        if (sequencer == null)
            return;

        FieldInfo effectrandomizer = typeof(BoneyardBurialSequencer).GetField("effectrandomizer", BindingFlags.NonPublic | BindingFlags.Static);
        CurrentBoneyardEffect = (int)effectrandomizer.GetValue(sequencer);

        Window.Label($"<b>Current Effect:</b> {EffectName(CurrentBoneyardEffect)}");
        using (Window.HorizontalScope(3))
        {
            Window.LabelBold("Change Effect");
            if (Window.Button("Brittle", disabled: () => new(() => CurrentBoneyardEffect == 1)))
            {
                effectrandomizer.SetValue(sequencer, 1);
                sequencer.selectionSlot.specificRenderers[0].material = sequencer.selectionSlot.specificRenderers[0].sharedMaterial = AssetConstants.BrittleGraveSelectionSlot;
            }
            if (Window.Button("Starve", disabled: () => new(() => CurrentBoneyardEffect == 2)))
            {
                effectrandomizer.SetValue(sequencer, 2);
                sequencer.selectionSlot.specificRenderers[0].material = sequencer.selectionSlot.specificRenderers[0].sharedMaterial = AssetConstants.StarveGraveSelectionSlot;
            }
        }
        using (Window.HorizontalScope(3))
        {
            if (Window.Button("Energy", disabled: () => new(() => CurrentBoneyardEffect == 3)))
            {
                effectrandomizer.SetValue(sequencer, 3);
                sequencer.selectionSlot.specificRenderers[0].material = sequencer.selectionSlot.specificRenderers[0].sharedMaterial = AssetConstants.EnergyGraveSelectionSlot;
            }
            if (Window.Button("Prisoner", disabled: () => new(() => CurrentBoneyardEffect == 4)))
            {
                effectrandomizer.SetValue(sequencer, 4);
                sequencer.selectionSlot.specificRenderers[0].material = sequencer.selectionSlot.specificRenderers[0].sharedMaterial = AssetConstants.PrisonerGraveSelectionSlot;
            }
            if (Window.Button("Haunter", disabled: () => new(() => CurrentBoneyardEffect == 5)))
            {
                effectrandomizer.SetValue(sequencer, 5);
                sequencer.selectionSlot.specificRenderers[0].material = sequencer.selectionSlot.specificRenderers[0].sharedMaterial = AssetConstants.HaunterGraveSelectionSlot;
            }
        }

        if (Window.Button("Apply Mod to Card", disabled: () => new(() => addingMod || sequencer.confirmStone.currentState != HighlightedInteractable.State.Interactable || sequencer.selectionSlot.Card == null)))
        {
            addingMod = true;
            Plugin.Instance.StartCoroutine(BoneyardMod(sequencer, sequencer.selectionSlot.Card.Info));
        }
    }
    private static IEnumerator BoneyardMod(BoneyardBurialSequencer sequencer, CardInfo card)
    {
        AudioController.Instance.PlaySound3D("card_blessing", MixerGroup.TableObjectsSFX, sequencer.selectionSlot.transform.position);
        GrimoraModHelper.BoneyardApplyModToCard(sequencer, card);
        sequencer.selectionSlot.Card.Anim.PlayTransformAnimation();
        yield return new WaitForSeconds(0.15f);
        sequencer.selectionSlot.Card.StatsLayer.SetEmissionColor(GameColors.Instance.darkLimeGreen);
        sequencer.selectionSlot.Card.SetInfo(card);
        sequencer.selectionSlot.Card.SetInteractionEnabled(true);
        yield return new WaitForSeconds(0.75f);
        addingMod = false;
    }

    [HarmonyReversePatch, HarmonyPatch(typeof(BoneyardBurialSequencer), "ApplyModToCard")]
    public static void BoneyardApplyModToCard(BoneyardBurialSequencer instance, CardInfo card) { }
}