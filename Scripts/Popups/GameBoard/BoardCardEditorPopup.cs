using DebugMenu.Scripts.Acts;
using DebugMenu.Scripts.Utils;
using DiskCardGame;
using GBC;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers.Extensions;
using UnityEngine;

namespace DebugMenu.Scripts.Popups.DeckEditorPopup;

public class BoardCardEditorPopup : BaseWindow
{
    public override string PopupName => "Card Editor";
    public override Vector2 Size => new(512f, 768f);

    public PlayableCard currentSelection = null;

    public override void OnGUI()
    {
        base.OnGUI();

        if (currentSelection == null)
        {
            GUILayout.Label("No card selected.");
            return;
        }

        GUILayout.BeginArea(new Rect(5f, 25f, Size.x - 10f, Size.y));
        if (DrawCardInfo.OnGUI(currentSelection.Info, currentSelection) == DrawCardInfo.Result.Altered)
            currentSelection.RenderCard();

        GUILayout.EndArea();

    }
}