using DebugMenu.Scripts.Acts;
using DiskCardGame;
using GBC;
using UnityEngine;

namespace DebugMenu.Scripts.Act2;

public class Act2 : BaseAct
{
    public Act2(DebugWindow window) : base(window)
    {
        m_mapSequence = new Act2MapSequence(this);
        m_cardBattleSequence = new Act2CardBattleSequence(window);
    }

    public override void OnGUI()
    {
        Window.LabelHeader("Act 2");
        Window.Padding();

        DrawCurrencyGUI();

        Window.StartNewColumn();
        OnGUICurrentNode();
    }

    public override void OnGUICurrentNode()
    {
        if (GBCEncounterManager.Instance?.EncounterOccurring ?? false)
        {
            Window.LabelHeader("Encounter");
            m_cardBattleSequence.OnGUI();
            return;
        }

        Window.Label("Unhandled GameState type!");
    }
    private void DrawCurrencyGUI()
    {
        Window.LabelHeader("Currency: " + SaveData.Data.currency);
        using (Window.HorizontalScope(4))
        {
            if (Window.Button("+1"))
                SaveData.Data.currency++;

            if (Window.Button("-1"))
                SaveData.Data.currency = Mathf.Max(0, SaveData.Data.currency - 1);

            if (Window.Button("+5"))
                SaveData.Data.currency += 5;

            if (Window.Button("-5"))
                SaveData.Data.currency = Mathf.Max(0, SaveData.Data.currency - 5);
        }
    }

    public override void Restart()
    {
        Log("Restarting GBC...");
        FrameLoopManager.Instance.SetIterationDisabled(disabled: false);
        MenuController.LoadGameFromMenu(newGameGBC: true);
    }

    public override void Reload()
    {
        Log("Reloading GBC...");
        base.Reload();
    }
}