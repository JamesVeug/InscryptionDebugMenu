using DebugMenu.Scripts.Utils;
using DiskCardGame;
using GBC;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers.Extensions;
using UnityEngine;

namespace DebugMenu.Scripts.Popups.DeckEditorPopup;

public class DeckEditorPopup : BaseWindow
{
    public override string PopupName => "Deck Editor";
    public override Vector2 Size => new(600f, 768f);

    private DeckInfo CurrentDeck => Helpers.CurrentDeck();
    private List<CardInfo> CurrentDeckCards => CurrentDeck.Cards ?? new();

    private int currentDeckEditorSelection = 0;
    private Vector2 editDeckScrollVector = Vector2.zero;
    private string[] deckCardArray = null;
    private string lastCardSearch = "";
    private List<CardInfo> lastSearchedList = null;
    private Vector2 foundCardListScrollVector = Vector2.zero;
    private static bool gbcAddToCollection = true;

    private static bool searchBySigil = false;

    public override void OnGUI()
    {
        base.OnGUI();
        if (CurrentDeckCards.Count == 0)
        {
            GUILayout.Label("No deck selected.", LabelHeaderStyleLeft);
            return;
        }

        GUILayout.BeginArea(new Rect(5f, 0f, Size.x - 10f, Size.y / 4f - 20f));
        OnGUIDeckViewer();
        GUILayout.EndArea();

        GUILayout.BeginArea(new Rect(5f, Size.y / 4f - 10f, Size.x - 10f, Size.y / 4f * 3f - 20f));
        if (currentDeckEditorSelection == -1)
            OnGUICardSearcher();
        else
            OnGUICardEditor();

        GUILayout.EndArea();

    }

    private void UpdateDeckReviewDisplay(bool amountChanged, CardInfo currentSelection)
    {
        if (SaveManager.SaveFile.IsPart2)
        {
            if (MenuController.m_Instance?.cardLibraryUI == null || !MenuController.Instance.cardLibraryUI.activeSelf)
                return;

            DeckBuildingUI ui = MenuController.Instance.cardLibraryUI.GetComponent<DeckBuildingUI>();
            if (ui == null)
                return;
            if (gbcAddToCollection)
                ui.Initialize();

            ui.UpdatePanelContents();
            AudioController.Instance.PlaySound2D("chipBlip2", MixerGroup.None, 0.4f, 0f, new AudioParams.Pitch(Mathf.Min(0.8f + SaveData.Data.deck.Cards.Count * 0.025f, 1.3f)));
        }
        else if (ViewManager.Instance?.CurrentView == View.MapDeckReview)
        {
            DeckReviewSequencer sequence = Singleton<DeckReviewSequencer>.m_Instance;
            if (sequence == null)
                return;

            if (amountChanged)
            {
                if (SaveManager.SaveFile.IsPart3)
                {
                    (sequence as Part3DeckReviewSequencer).OnMainDeckSelected();
                }
                else
                {
                    ViewManager.Instance.CurrentView = View.MapDefault;
                    sequence.SetDeckReviewShown(false);
                    ViewManager.Instance.CurrentView = View.MapDeckReview;
                    sequence.SetDeckReviewShown(true);
                }
            }
            else
            {
                SelectableCard card = sequence.cardArray.displayedCards.Find(x => x.Info == currentSelection);
                card.RenderInfo.attack = card.Info.Attack;
                card.RenderInfo.health = card.Info.Health;
                card.RenderInfo.energyCost = card.Info.EnergyCost;
                card.RenderCard();
            }
        }
    }

    private void OnGUICardEditor() // currentDeckEditorSelection cannot be -1 here
    {
        if (currentDeckEditorSelection >= CurrentDeckCards.Count)
            currentDeckEditorSelection = CurrentDeckCards.Count - 1;

        if (CurrentDeckCards[currentDeckEditorSelection] == null)
            return;

        CardInfo val = CurrentDeckCards[currentDeckEditorSelection];
        DrawCardInfo.Result result = DrawCardInfo.OnGUI(val, null, CurrentDeck);

        if (result == DrawCardInfo.Result.None)
            return;

        if (result == DrawCardInfo.Result.Removed)
        {
            currentDeckEditorSelection = Mathf.Min(currentDeckEditorSelection, CurrentDeckCards.Count);
        }
        else if (result == DrawCardInfo.Result.Altered)
        {
            // if the board exists, update player cards
            if (BoardManager.m_Instance != null)
            {
                foreach (PlayableCard card in BoardManager.Instance.GetPlayerCards())
                {
                    if (card.Info == val)
                        card.RenderCard();
                }
            }
            // update cards in hand
            if (PlayerHand.m_Instance != null)
            {
                foreach (PlayableCard hand in PlayerHand.Instance.CardsInHand)
                {
                    if (hand.Info == val)
                        hand.RenderCard();
                }
            }
            // update cards in deck review
        }

        UpdateDeckReviewDisplay(true, null);
    }

    private void OnGUICardSearcher()
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label("Card Finder", Helpers.HeaderLabelStyle());
        searchBySigil = GUILayout.Toggle(searchBySigil, "Search by abilities");

        if (SaveManager.SaveFile.IsPart2)
            gbcAddToCollection = GUILayout.Toggle(gbcAddToCollection, "Add to collection");
        
        GUILayout.EndHorizontal();

        lastCardSearch = GUILayout.TextField(lastCardSearch);
        lastSearchedList = new List<CardInfo>();
        if (lastCardSearch != "")
        {
            if (searchBySigil)
            {
                if (GetAbilitiesThatContain(lastCardSearch, out List<Ability> results))
                {
                    lastSearchedList.AddRange(CardManager.AllCardsCopy.Where(x => x.abilities != null && x.abilities.Exists(results.Contains)));
                }
            }
            else
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
        }
        else if (searchBySigil)
        {
            if (GetAbilitiesThatContain(lastCardSearch, out List<Ability> results))
            {
                lastSearchedList.AddRange(CardManager.AllCardsCopy.Where(x => x.abilities == null || x.abilities.Count == 0));
            }
        }
        else
        {
            lastSearchedList = CardManager.AllCardsCopy;
        }
        FoundCardList();
    }

    private void FoundCardList()
    {
        foundCardListScrollVector = GUILayout.BeginScrollView(foundCardListScrollVector);
        if (lastSearchedList.Count > 0)
        {
            foreach (CardInfo lastSearched in lastSearchedList)
            {
                string name = $"{lastSearched.DisplayedNameLocalized}\n({lastSearched.name})";
                if (GUILayout.Button(name))
                {
                    CardInfo obj = lastSearched.Clone() as CardInfo;
                    CurrentDeck.AddCard(obj);
                    if (SaveManager.SaveFile.IsPart2 && gbcAddToCollection)
                        SaveManager.SaveFile.CollectGBCCard(obj);

                    SaveManager.SaveToFile(false);
                    currentDeckEditorSelection = CurrentDeckCards.Count;
                    UpdateDeckReviewDisplay(true, obj);
                }
            }
        }
        else
        {
            GUILayout.Label("No Cards Found...");
        }
        GUILayout.EndScrollView();
        
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

    private bool GetAbilitiesThatContain(string search, out List<Ability> results)
    {
        results = new();
        foreach (AbilityManager.FullAbility searching in AbilityManager.AllAbilities)
        {
            if (searching.Info == null)
                continue;
            search = search.ToLowerInvariant();

            string abilityName = searching.Id.ToString();
            if (int.TryParse(abilityName, out _) && searching.AbilityBehavior != null)
                abilityName = searching.AbilityBehavior.Name;

            if (!searching.Info.rulebookName.ToLowerInvariant().Contains(search) && !abilityName.ToLowerInvariant().Contains(search))
                continue;

            results.Add(searching.Id);
        }
        return results.Count > 0;
    }

    private void OnGUIDeckViewer()
    {
        bool adding = false;
        GUILayout.BeginHorizontal();
        GUILayout.Label("Deck Viewer", LabelHeaderStyleLeft);
        if (GUILayout.Button("Add Card"))
        {
            adding = true;
            currentDeckEditorSelection = -1;
        }
        GUILayout.EndHorizontal();

        editDeckScrollVector = GUILayout.BeginScrollView(editDeckScrollVector);
        deckCardArray = new string[CurrentDeckCards.Count];
        for (int i = 0; i < CurrentDeckCards.Count; i++)
        {
            deckCardArray[i] = CurrentDeckCards[i]?.DisplayedNameLocalized ?? "Card not found!";
        }

        if (!adding)
        {
            currentDeckEditorSelection = GUILayout.SelectionGrid(currentDeckEditorSelection, deckCardArray, 2);
        }
        GUILayout.EndScrollView();
    }
}