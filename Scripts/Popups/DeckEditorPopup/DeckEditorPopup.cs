using DebugMenu.Scripts.Acts;
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

	private int currentDeckEditorSelection = 0;
	private Vector2 editDeckScrollVector = Vector2.zero;
	private string[] deckCardArray = null;
	private string lastCardSearch = "";
	private List<CardInfo> lastSearchedList = null;
	private Vector2 foundCardListScrollVector = Vector2.zero;

	public override void OnGUI()
	{
		base.OnGUI();

		DeckInfo currentDeck = CurrentDeck;
		if (currentDeck == null)
		{
			GUILayout.Label("No deck selected.", LabelHeaderStyleLeft);
			return;
		}
		
		GUILayout.BeginArea(new Rect(5f, 0f, Size.x - 10f, Size.y / 4f));
		OnGUIDeckViewer();
		GUILayout.EndArea();
		
		GUILayout.BeginArea(new Rect(5f, Size.y / 4f, Size.x - 10f, Size.y / 4f * 3f));
		if (currentDeckEditorSelection == -1)
			OnGUICardSearcher();
		else
			OnGUICardEditor();

		GUILayout.EndArea();
		
	}

	private void UpdateDeckReviewDisplay(bool amountChanged, CardInfo currentSelection)
	{
        DeckReviewSequencer sequence = UnityEngine.Object.FindObjectOfType<DeckReviewSequencer>();
		if (sequence == null)
			return;

		if (amountChanged)
		{
			ViewManager.Instance.CurrentView = View.Board;
			sequence.SetDeckReviewShown(false);
            ViewManager.Instance.CurrentView = View.MapDeckReview;
            sequence.SetDeckReviewShown(true);
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
	private void OnGUICardEditor() // currentDeckEditorSelection cannot be -1 here
	{
		if (currentDeckEditorSelection >= CurrentDeck.Cards.Count)
			currentDeckEditorSelection = CurrentDeck.Cards.Count - 1;

		if (CurrentDeck.Cards[currentDeckEditorSelection] == null)
			return;

		CardInfo val = CurrentDeck.Cards[currentDeckEditorSelection];
		DrawCardInfo.Result result = DrawCardInfo.OnGUI(val, null, CurrentDeck);

        if (result == DrawCardInfo.Result.Removed)
		{
			currentDeckEditorSelection = Mathf.Min(currentDeckEditorSelection, CurrentDeck.Cards.Count);
            if (ViewManager.m_Instance?.CurrentView == View.MapDeckReview)
            {
				UpdateDeckReviewDisplay(true, null);
            }
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
            if (ViewManager.m_Instance?.CurrentView == View.MapDeckReview)
			{
				UpdateDeckReviewDisplay(false, val);
			}
			// can't figure out how to update the GBC collection info dynamically, maybe later if requested
/*            if (SaveManager.SaveFile.IsPart2)
			{
                DeckBuildingUI deckUI = UnityEngine.Object.FindObjectOfType<DeckBuildingUI>();
				if (deckUI?.gameObject.activeSelf == true)
				{
                    foreach (PixelSelectableCard selectableCard in deckUI.collection.pageCards)
                    {
						Debug.Log($"{selectableCard != null}");
                        if (selectableCard?.Info == val)
                            selectableCard.RenderCard();
                    }
                }
                CollectionBookUI collectionUI = UnityEngine.Object.FindObjectOfType<CollectionBookUI>();
                if (collectionUI?.gameObject.activeSelf == true)
                {
                    foreach (PixelSelectableCard selectableCard in collectionUI.collectionUI.pageCards)
                    {
                        if (selectableCard?.Info == val)
                            selectableCard.RenderCard();
                    }
                }
            }*/
		}
	}

	private void OnGUICardSearcher()
	{
		GUILayout.BeginVertical();
		GUILayout.Label("Card Finder", Helpers.HeaderLabelStyle());
		lastCardSearch = GUILayout.TextField(lastCardSearch);
		lastSearchedList = new List<CardInfo>();
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

	private void FoundCardList()
	{
		foundCardListScrollVector = GUILayout.BeginScrollView(foundCardListScrollVector);
		if (lastSearchedList.Count > 0)
		{
			foreach (CardInfo lastSearched in lastSearchedList)
			{
				if (GUILayout.Button($"{lastSearched.DisplayedNameLocalized}\n({lastSearched.name})"))
				{
					CardInfo obj = lastSearched.Clone() as CardInfo;
                    CurrentDeck.AddCard(obj);
					SaveManager.SaveToFile(false);
					currentDeckEditorSelection = CurrentDeck.Cards.Count;
					if (ViewManager.m_Instance.CurrentView == View.MapDeckReview)
						UpdateDeckReviewDisplay(true, obj);
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
		deckCardArray = new string[CurrentDeck.Cards.Count];
		for (int i = 0; i < CurrentDeck.Cards.Count; i++)
		{
			deckCardArray[i] = CurrentDeck.Cards[i]?.displayedName ?? "Card not found!";
		}

		if (!adding)
		{
            currentDeckEditorSelection = GUILayout.SelectionGrid(currentDeckEditorSelection, deckCardArray, 2);
        }
        GUILayout.EndScrollView();
	}
}