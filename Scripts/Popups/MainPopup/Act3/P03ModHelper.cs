using DiskCardGame;
using Infiniscryption.P03KayceeRun.Patchers;
using InscryptionAPI.Saves;

namespace DebugMenu.Scripts.Act3;

public static partial class P03ModHelper
{
    internal static bool _enabled;
    public static bool Enabled => _enabled;
    public static void PatchP03Mod() => Plugin.HarmonyInstance.PatchAll(typeof(P03ModHelper));

    internal static bool addingMod = false;

    public static bool IsP03Run => P03AscensionSaveData.IsP03Run;

    public static void ReloadP03Run(Act3 act)
    {
        if (AscensionSaveData.Data.currentRun != null)
        {
            RunBasedHoloMap.ClearWorldData();
            SaveManager.LoadFromFile();
            LoadingScreenManager.LoadScene("Part3_Cabin");
            SaveManager.savingDisabled = false;
        }
        else
            NewAscensionGame(act);
    }
    public static void NewAscensionGame(Act3 act)
    {
        FrameLoopManager.Instance.SetIterationDisabled(disabled: false);
        LoadingScreenManager.LoadScene("Ascension_Configure");
        RunBasedHoloMap.ClearWorldData();
        SaveManager.SaveFile.part3Data = null;
        ModdedSaveManager.SaveData.SetValue("zorro.inscryption.infiniscryption.p03kayceerun", "CopyOfPart3AscensionSave", null);

        Act3.lastUsedStarterDeck ??= StarterDecksUtil.GetInfo(StarterDecks.DEFAULT_STARTER_DECK).cards;
        if (Act3.lastUsedStarterDeck != null)
        {
            act.Log("New Game! With " + Act3.lastUsedStarterDeck.Count + " Cards!");
            AscensionSaveData.Data.NewRun(Act3.lastUsedStarterDeck);
            SaveManager.SaveToFile(saveActiveScene: false);
            MenuController.LoadGameFromMenu(newGameGBC: false);
            Singleton<InteractionCursor>.Instance.SetHidden(hidden: true);
        }
    }
}
