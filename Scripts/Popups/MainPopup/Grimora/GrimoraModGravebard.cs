using DebugMenu.Scripts.Acts;
using DiskCardGame;
using GrimoraMod;
using HarmonyLib;
using System.Collections;
using System.Reflection;

namespace DebugMenu.Scripts.Grimora;

public static partial class GrimoraModHelper
{
    internal static void OnGUIGravebardCamp(DebugWindow Window)
    {
        GravebardCampSequencer sequencer = GravebardCampSequencer.Instance;
        if (sequencer == null)
            return;

        int price = (int)typeof(GravebardCampSequencer).GetField("price", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(sequencer);
        ConfirmStoneButton confirmStone = (ConfirmStoneButton)typeof(GravebardCampSequencer).GetField("confirmStone", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(sequencer);
        Window.LabelBold($"Current price: {price}");

        if (Window.Button("Generate Item", disabled: () => new(() => addingMod || confirmStone.currentState != HighlightedInteractable.State.Interactable || RunState.Run.consumables.Count < 3)))
        {
            addingMod = true;
            Plugin.Instance.StartCoroutine(GenerateItem(sequencer));
        }
        if (Window.Button("Tell Random Story"))
        {
            addingMod = true;
            Plugin.Instance.StartCoroutine(TellRandomStory(sequencer));
        }
    }
    private static IEnumerator GenerateItem(GravebardCampSequencer sequence)
    {
        yield return GravebardGenerateItem(sequence);
        addingMod = false;
    }
    private static IEnumerator TellRandomStory(GravebardCampSequencer sequence)
    {
        yield return GravebardTellStory(sequence);
        addingMod = false;
    }

    [HarmonyReversePatch, HarmonyPatch(typeof(GravebardCampSequencer), "GenerateItem")]
    public static IEnumerator GravebardGenerateItem(GravebardCampSequencer instance) { yield break; }

    [HarmonyReversePatch, HarmonyPatch(typeof(GravebardCampSequencer), "TellRandomStory")]
    public static IEnumerator GravebardTellStory(GravebardCampSequencer instance) { yield break; }
}