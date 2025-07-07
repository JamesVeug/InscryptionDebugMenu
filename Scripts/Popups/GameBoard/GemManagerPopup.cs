using DebugMenu.Scripts.Utils;
using DiskCardGame;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using UnityEngine;

namespace DebugMenu.Scripts.Popups.DeckEditorPopup;

public class GemManagerPopup : BaseWindow
{
    public override string PopupName => "Gem Resources";
    public override Vector2 Size => new(450f, 400f);

    public override void OnGUI()
    {
        base.OnGUI();
        if (GameFlowManager.Instance.CurrentGameState != GameState.CardBattle)
        {
            IsActive = false;
            return;
        }

        if (ResourcesManager.Instance != null)
        {
            LabelHeader("Player-Owned Gems");
            LabelHeader("Green: " + ResourcesManager.Instance.GemsOfType(GemType.Green) +
                "\nBlue: " + ResourcesManager.Instance.GemsOfType(GemType.Blue) +
                "\nOrange: " + ResourcesManager.Instance.GemsOfType(GemType.Orange), new(ColumnWidth, 100f));

            using (HorizontalScope(3))
            {
                if (Button("Add Green"))
                {
                    Plugin.Instance.StartCoroutine(ResourcesManager.Instance.AddGem(GemType.Green));
                }
                if (Button("Add Blue"))
                {
                    Plugin.Instance.StartCoroutine(ResourcesManager.Instance.AddGem(GemType.Blue));
                }
                if (Button("Add Orange"))
                {
                    Plugin.Instance.StartCoroutine(ResourcesManager.Instance.AddGem(GemType.Orange));
                }
            }
            using (HorizontalScope(3))
            {
                if (Button("Remove Green"))
                {
                    Plugin.Instance.StartCoroutine(ResourcesManager.Instance.LoseGem(GemType.Green));
                }
                if (Button("Remove Blue"))
                {
                    Plugin.Instance.StartCoroutine(ResourcesManager.Instance.LoseGem(GemType.Blue));
                }
                if (Button("Remove Orange"))
                {
                    Plugin.Instance.StartCoroutine(ResourcesManager.Instance.LoseGem(GemType.Orange));
                }
            }
            if (Button("Force Gem Update"))
            {
                ResourcesManager.Instance.ForceGemsUpdate();
            }
            if (Button("Clear Gems"))
            {
                while (ResourcesManager.Instance.gems.Count > 0)
                {
                    Plugin.Instance.StartCoroutine(ResourcesManager.Instance.LoseGem(ResourcesManager.Instance.gems[0]));
                }
            }
        }

        StartNewColumn();
        if (OpponentGemsManager.Instance == null)
            return;

        LabelHeader("Opponent-Owned Gem");
        LabelHeader("Green: " + OpponentGemsManager.Instance.GemsOfType(GemType.Green) +
                "\nBlue: " + OpponentGemsManager.Instance.GemsOfType(GemType.Blue) +
                "\nOrange: " + OpponentGemsManager.Instance.GemsOfType(GemType.Orange), new(ColumnWidth, 100f));

        using (HorizontalScope(3))
        {
            if (Button("Add Green"))
            {
                OpponentGemsManager.Instance.AddGem(GemType.Green);
            }
            if (Button("Add Blue"))
            {
                OpponentGemsManager.Instance.AddGem(GemType.Blue);
            }
            if (Button("Add Orange"))
            {
                OpponentGemsManager.Instance.AddGem(GemType.Orange);
            }
        }
        using (HorizontalScope(3))
        {
            if (Button("Remove Green"))
            {
                OpponentGemsManager.Instance.LoseGem(GemType.Green);
            }
            if (Button("Remove Blue"))
            {
                OpponentGemsManager.Instance.LoseGem(GemType.Blue);
            }
            if (Button("Remove Orange"))
            {
                OpponentGemsManager.Instance.LoseGem(GemType.Orange);
            }
        }
        if (Button("Force Gem Update"))
        {
            OpponentGemsManager.Instance.ForceGemsUpdate();
        }
        if (Button("Clear Gems"))
        {
            while (OpponentGemsManager.Instance.opponentGems.Count > 0)
            {
                OpponentGemsManager.Instance.LoseGem(OpponentGemsManager.Instance.opponentGems[0]);
            }
        }
    }
}