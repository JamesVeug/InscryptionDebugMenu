using DebugMenu.Scripts.Acts;
using DiskCardGame;
using UnityEngine;

namespace DebugMenu.Scripts.Act1;

public class Act1 : BaseAct
{
    public static List<CardInfo> lastUsedStarterDeck = null;
    public static bool SkipNextNode = false;
    public static bool ActivateAllMapNodesActive = false;

    public Act1(DebugWindow window) : base(window)
    {
        m_mapSequence = new Act1MapSequence(this);
        m_cardBattleSequence = new Act1CardBattleSequence(window);
    }

    public override void OnGUI()
    {
        MapNodeManager mapNodeManager = Singleton<MapNodeManager>.m_Instance;
        if (mapNodeManager?.nodes == null || RunState.Run == null)
            return;

        Window.LabelHeader("Act 1");
        if (RunState.Run.currentNodeId > 0)
        {
            MapNode nodeWithId = mapNodeManager.GetNodeWithId(RunState.Run.currentNodeId);
            Window.Label("Current Node ID: " + RunState.Run.currentNodeId + "\nCurrent Node: " + nodeWithId?.name, new(0, 80f));
        }

        DrawCurrencyGUI();
        DrawItemsGUI();
        Window.Padding();

        if (Window.Button("Replenish Candles"))
        {
            Plugin.Instance.StartCoroutine(CandleHolder.Instance.ReplenishFlamesSequence(0f));
            RunState.Run.playerLives = RunState.Run.maxPlayerLives;
            SaveManager.SaveToFile(false);
        }

        Window.StartNewColumn();
        OnGUICurrentNode();
    }

    public override bool OnSpecialCardSequence(string nodeDataName)
    {
        if (nodeDataName == "CardStatBoost")
            return true;

        if (nodeDataName == "ChooseEyeball")
            return true;

        if (nodeDataName == "RemoveCard")
            return true;

        return false;
    }
    private void DrawCurrencyGUI()
    {
        Window.LabelHeader("Currency: " + RunState.Run.currency);
        using (Window.HorizontalScope(4))
        {
            if (Window.Button("+1"))
                RunState.Run.currency++;

            if (Window.Button("-1"))
                RunState.Run.currency = Mathf.Max(0, RunState.Run.currency - 1);

            if (Window.Button("+5"))
                RunState.Run.currency += 5;

            if (Window.Button("-5"))
                RunState.Run.currency = Mathf.Max(0, RunState.Run.currency - 5);
        }
    }

    public override void Restart()
    {
        if (SaveFile.IsAscension)
            NewAscensionGame();
        else if (SaveManager.SaveFile.IsPart1)
            RestartVanilla();
    }

    public override void Reload()
    {
        if (SaveFile.IsAscension)
        {
            if (AscensionSaveData.Data.currentRun != null)
                ReloadKaycees();
            else
                NewAscensionGame();
        }
        else
        {
            Log("Reloading Vanilla...");
            base.Reload();
        }
    }

    private void NewAscensionGame()
    {
        FrameLoopManager.Instance.SetIterationDisabled(disabled: false);
        SceneLoader.Load("Ascension_Configure");
        if (lastUsedStarterDeck != null)
        {
            Log("New Ascension run with " + lastUsedStarterDeck.Count + " cards!");
            AscensionSaveData.Data.NewRun(lastUsedStarterDeck);
            SaveManager.SaveToFile(saveActiveScene: false);
            MenuController.LoadGameFromMenu(newGameGBC: false);
            Singleton<InteractionCursor>.Instance.SetHidden(hidden: true);
        }
    }

    private void RestartVanilla()
    {
        Log("Restarting Vanilla...");
        FrameLoopManager.Instance.SetIterationDisabled(disabled: false);
        SaveManager.SaveFile.ResetPart1Run();
        SaveManager.SaveToFile(saveActiveScene: false);
        SceneLoader.Load("Part1_Cabin");
    }
}