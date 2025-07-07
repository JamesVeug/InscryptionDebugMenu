using DebugMenu.Scripts.Utils;
using DiskCardGame;
using InscryptionAPI.Card;
using System.Collections;
using UnityEngine;

namespace DebugMenu.Scripts.Popups.DeckEditorPopup;

public class DrawCustomCardPopup : BaseWindow
{
    public override string PopupName => "Spawn Card to Hand";
    public override Vector2 Size => new(600f, 768f);

    private string lastCardSearch = "";
    private List<CardInfo> lastSearchedList = null;
    private Vector2 foundCardListScrollVector = Vector2.zero;

    public override void OnGUI()
    {
        if (GameFlowManager.m_Instance?.CurrentGameState != GameState.CardBattle)
        {
            IsActive = false;
            return;
        }
        base.OnGUI();
        OnGUICardSearcher();
    }

    private void OnGUICardSearcher()
    {
        GUILayout.Label("Card Finder", Helpers.HeaderLabelStyle());
        GUILayout.BeginVertical();
        lastSearchedList = new List<CardInfo>();
        lastCardSearch = GUILayout.TextField(lastCardSearch);
        if (lastCardSearch != "")
        {
            if (GetCardByName(lastCardSearch, out var result))
            {
                lastSearchedList.Add(CardManager.AllCardsCopy[result]);
            }
            else if (GetCardsThatContain(lastCardSearch, out List<int> results))
            {
                foreach (int item in results)
                {
                    lastSearchedList.Add(CardManager.AllCardsCopy[item]);
                }
            }
        }
        else
        {
            lastSearchedList = CardManager.AllCardsCopy;
        }
        FoundCardList();
    }

    private bool spawningCard = false;
    private void FoundCardList()
    {
        foundCardListScrollVector = GUILayout.BeginScrollView(foundCardListScrollVector);
        if (lastSearchedList.Count > 0)
        {
            foreach (CardInfo lastSearched in lastSearchedList)
            {
                string display = $"{lastSearched.DisplayedNameLocalized}\n({lastSearched.name})";

                if (spawningCard)
                {
                    GUILayout.Label(display, Helpers.DisabledButtonStyle());
                }
                else if (GUILayout.Button(display))
                {

                    CardInfo obj = lastSearched.Clone() as CardInfo;
                    Plugin.Instance.StartCoroutine(CardSpawner.Instance.SpawnCardToHand(obj, 0f));
                }
            }
        }
        else
        {
            GUILayout.Label("No Cards Found...");
        }
        GUILayout.EndScrollView();
        GUILayout.EndVertical();
    }

    private IEnumerator SpawnChosenCardToHand(CardInfo card)
    {
        spawningCard = true;
        yield return CardSpawner.Instance.SpawnCardToHand(card, 0f);
        spawningCard = false;
    }
    private bool GetCardsThatContain(string cardName, out List<int> results)
    {
        string lower = cardName.ToLower();
        bool result = false;
        results = new List<int>();
        for (int i = 0; i < CardManager.AllCardsCopy.Count; i++)
        {
            CardInfo allCard = CardManager.AllCardsCopy[i];
            string name = allCard.name;
            string displayedName = allCard.displayedName;
            if (name != null && name.ToLower().Contains(lower) || displayedName != null && displayedName.ToLower().Contains(lower))
            {
                results.Add(i);
                result = true;
            }
        }

        return result;
    }

    private bool GetCardByName(string cardName, out int index)
    {
        bool exists = false;
        index = -1;

        List<CardInfo> allCardsCopy = CardManager.AllCardsCopy;
        for (int i = 0; i < allCardsCopy.Count; i++)
        {
            CardInfo allCard = allCardsCopy[i];
            if (allCard.name == cardName)
            {
                index = i;
                exists = true;
                break;
            }
        }

        return exists;
    }
}