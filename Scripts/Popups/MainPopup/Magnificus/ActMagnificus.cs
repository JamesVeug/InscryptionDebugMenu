using DebugMenu.Scripts.Acts;
using DiskCardGame;
using UnityEngine;

namespace DebugMenu.Scripts.Magnificus;

public class ActMagnificus : BaseAct
{
    public static bool SkipNextNode = false;
    public ActMagnificus(DebugWindow window) : base(window)
    {
        m_mapSequence = new ActMagnificusMapSequence(this);
        m_cardBattleSequence = new MagnificusCardBattleSequence(window);
    }

    public override void Update()
    {
    }

    public override void OnGUI()
    {
        Window.LabelHeader("Magnificus' Act");
        DrawCurrencyGUI();

        Window.StartNewColumn();
        OnGUICurrentNode();
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

    public override string GetSpecialNodeName(string nodeDataName)
    {
        return nodeDataName switch
        {
            "CustomNode1" => nodeDataName + " (ChangeCost)",
            "CustomNode2" => nodeDataName + " (Shop)",
            "CustomNode3" => nodeDataName + " (Bleach)",
            "CustomNode14" => nodeDataName + " (Enchant)",
            _ => base.GetSpecialNodeName(nodeDataName),
        };
    }

    public override bool OnSpecialCardSequence(string nodeDataName)
    {
        if (nodeDataName == "SpellCardChoice")
        {
            if (Window.Button("Reroll choices", disabled: () => new(() => MagnificusModHelper.rerolling)))
            {
                Plugin.Instance.StartCoroutine(MagnificusModHelper.RerollSpellChoices());
            }
            return true;
        }
        if (nodeDataName == "CustomNode14 (Enchant)")
        {
            MagnificusModHelper.HandleEnchantNode(this.Window);
            return true;
        }
        if (nodeDataName == "CustomNodeDeck")
        {
            GameFlowManager.Instance.CurrentGameState = GameState.Map;
            return true;
        }
        return false;
    }

    public override void Restart()
    {
        SceneLoader.Load("finale_magnificus");
    }
}