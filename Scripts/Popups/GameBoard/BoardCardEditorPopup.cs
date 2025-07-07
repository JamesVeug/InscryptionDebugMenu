using DebugMenu.Scripts.Utils;
using DiskCardGame;
using UnityEngine;

namespace DebugMenu.Scripts.Popups.DeckEditorPopup;

public class BoardCardEditorPopup : BaseWindow
{
    public override string PopupName => "Board Card Editor";
    public override Vector2 Size => new(600f, 768f);

    private PlayableCard currentSelection = null;

    private GameBoardPopup gameBoardPopup = null;
    public override void OnGUI()
    {
        base.OnGUI();
        if (GameFlowManager.m_Instance.CurrentGameState != GameState.CardBattle)
        {
            IsActive = false;
            return;
        }
        gameBoardPopup ??= Plugin.Instance.GetWindow<GameBoardPopup>();
        currentSelection = gameBoardPopup.currentSelection.Item1;
        if (currentSelection == null)
        {
            GUILayout.Label("No card selected!", Helpers.HeaderLabelStyle());
            return;
        }

        GUILayout.BeginArea(new Rect(5f, 25f, Size.x - 10f, Size.y));
        if (DrawCardInfo.OnGUI(currentSelection.Info, currentSelection) == DrawCardInfo.Result.Altered)
            currentSelection.RenderCard();

        GUILayout.EndArea();

    }
}