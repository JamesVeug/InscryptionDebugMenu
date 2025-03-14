using DebugMenu.Scripts.Acts;
using DiskCardGame;
using UnityEngine;

namespace DebugMenu.Scripts.Act3;

public class Act3 : BaseAct
{
    public static List<CardInfo> lastUsedStarterDeck = null;

    public Act3(DebugWindow window) : base(window)
    {
        m_mapSequence = new Act3MapSequence(this);
        m_cardBattleSequence = new Act3CardBattleSequence(window);
    }

    public override void OnGUI()
    {
        MapNodeManager mapNodeManager = Singleton<MapNodeManager>.m_Instance;
        if (mapNodeManager == null || mapNodeManager.nodes == null || RunState.Run == null)
            return;

        Window.LabelHeader("Act 3");
        if (RunState.Run.currentNodeId > 0 && Singleton<MapNodeManager>.m_Instance != null)
        {
            MapNode nodeWithId = Singleton<MapNodeManager>.Instance.GetNodeWithId(RunState.Run.currentNodeId);
            Window.Label("Current Node ID: " + RunState.Run.currentNodeId + "\nCurrent Node: " + nodeWithId?.name, new(0, 80f));
        }

        DrawCurrencyGUI();
        DrawItemsGUI();
        Window.Padding();

        Window.StartNewColumn();
        OnGUICurrentNode();
    }
    private void DrawCurrencyGUI()
    {
        Window.LabelHeader("Currency: " + Part3SaveData.Data.currency);
        using (Window.HorizontalScope(4))
        {
            if (Window.Button("+1"))
                Part3SaveData.Data.currency++;

            if (Window.Button("-1"))
                Part3SaveData.Data.currency = Mathf.Max(0, Part3SaveData.Data.currency - 1);

            if (Window.Button("+5"))
                Part3SaveData.Data.currency += 5;

            if (Window.Button("-5"))
                Part3SaveData.Data.currency = Mathf.Max(0, Part3SaveData.Data.currency - 5);
        }
    }

    public override bool OnSpecialCardSequence(string nodeDataName)
    {
        if (nodeDataName == "Part3RareCardChoices")
        {
            OnGUICardChoiceNodeSequence();
            return true;
        }
        return base.OnSpecialCardSequence(nodeDataName);
    }

    public override void Restart()
    {

        if (P03ModHelper.Enabled && P03ModHelper.IsP03Run)
        {
            Log("Restarting P03 KCM...");
            P03ModHelper.NewAscensionGame(this);
        }
        else
        {
            Log("Restarting Part 3...");
            StoryEventsData.EraseEvent(StoryEvent.Part3Intro);
            SceneLoader.Load("Part3_Cabin");
        }
    }

    public override void Reload()
    {
        if (P03ModHelper.Enabled && P03ModHelper.IsP03Run)
        {
            Log("Reloading P03 KCM...");
            P03ModHelper.ReloadP03Run(this);
        }
        else
        {
            Log("Reloading Act 3...");
            base.Reload();
        }
    }
}