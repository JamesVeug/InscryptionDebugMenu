using DebugMenu.Scripts.Acts;
using DiskCardGame;
using UnityEngine;

namespace DebugMenu.Scripts.Grimora;

public class ActGrimora : BaseAct
{
    public static List<CardInfo> lastUsedStarterDeck = null;

    public ActGrimora(DebugWindow window) : base(window)
    {
        m_mapSequence = new GrimoraMapSequence(this);
        m_cardBattleSequence = new GrimoraCardBattleSequence(window);
    }

    public override void OnGUI()
    {
        Window.LabelHeader("Grimora Act");
        if (RunState.Run.currentNodeId > 0 && Singleton<MapNodeManager>.m_Instance != null)
        {
            MapNode nodeWithId = Singleton<MapNodeManager>.Instance.GetNodeWithId(RunState.Run.currentNodeId);
            Window.Label("Current Node ID: " + RunState.Run.currentNodeId + "\nCurrent Node: " + nodeWithId?.name, new(0, 80f));
        }

        if (GrimoraModHelper.Enabled)
        {
            DrawCurrencyGUI();
            DrawItemsGUI();
        }

        Window.StartNewColumn();
        OnGUICurrentNode();
    }

    public override bool OnSpecialCardSequence(string nodeDataName)
    {
        if (GrimoraModHelper.Enabled)
        {
            switch (nodeDataName)
            {
                case "BoneyardBurial":
                    GrimoraModHelper.OnGUIBoneyardBurial(this.Window);
                    return true;
                case "CardMerge":
                    return true;
                case "ElectricChair":
                    GrimoraModHelper.OnGUIElectricChairNodeSequence(this.Window);
                    return true;
                case "GoatEye":
                    GrimoraModHelper.OnGUIGoatEye(this.Window);
                    return true;
                case "GravebardCamp":
                    GrimoraModHelper.OnGUIGravebardCamp(this.Window);
                    return true;
            }
        }
        return false;
    }

    public override void Restart()
    {
        SceneLoader.Load("finale_grimora");
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
}