using DebugMenu.Scripts.Acts;
using DebugMenu.Scripts.Popups;
using DebugMenu.Scripts.Popups.DeckEditorPopup;
using UnityEngine;

namespace DebugMenu.Scripts.All;

public class AllActs : BaseAct
{
    public static bool blockAllInput = false;

    public AllActs(DebugWindow window) : base(window)
    {
    }

    public override void Update()
    {

    }

    public override void OnGUI()
    {
        Window.Toggle("Block all Input", ref blockAllInput);
        Window.Toggle("Disable all Dialogue", ref Configs.m_disableDialogue);

        using (Window.HorizontalScope(4))
        {
            Window.Label("Time Scale:");

            if (Window.Button("0.1x"))
            {
                Log("Minimum Time Scale");
                SetTimeScale(0.1f);
            }

            if (Window.Button("1x"))
            {
                Log("Minimum Time Scale");
                SetTimeScale(1f);
            }

            if (Window.Button("5x"))
            {
                Log("Minimum Time Scale");
                SetTimeScale(5f);
            }
        }

        if (Window.Button("Show Game Info"))
        {
            Plugin.Instance.ToggleWindow<GameInfoPopup>();
        }

        if (Window.Button("Change Hotkeys"))
        {
            Plugin.Instance.ToggleWindow<HotkeysPopup>();
        }

        if (Window.Button("Deck Editor"))
        {
            Plugin.Instance.ToggleWindow<DeckEditorPopup>();
        }
    }

    public override void OnGUIMinimal()
    {

    }

    public void SetTimeScale(float speed)
    {
        Time.timeScale = speed;
        Time.fixedDeltaTime = Plugin.StartingFixedDeltaTime * Time.timeScale;
    }

    public override void Restart()
    {
        // Nothing
    }

    public override void Reload()
    {
        // Nothing
    }
}