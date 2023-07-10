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
	public override Vector2 Size => new(512f, 768f);

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
			GUILayout.Label("No deck selected.");
			return;
		}
		
		GUILayout.BeginArea(new Rect(5f, 25f, Size.x - 10f, Size.y / 4f - 25f));
		OnGUIDeckViewer();
		GUILayout.EndArea();
		
		GUILayout.BeginArea(new Rect(5f, Size.y / 4f, Size.x - 10f, Size.y / 4f * 3f + 5f));
		if (currentDeckEditorSelection == 0)
			OnGUICardSearcher();
		else
			OnGUICardEditor();

		GUILayout.EndArea();
		
	}

	private void OnGUICardEditor()
	{
		if (currentDeckEditorSelection - 1 >= CurrentDeck.Cards.Count)
			currentDeckEditorSelection--;

		if (CurrentDeck.Cards[currentDeckEditorSelection - 1] == null)
			return;

		CardInfo val = CurrentDeck.Cards[currentDeckEditorSelection - 1];
		DrawCardInfo.Result result = DrawCardInfo.OnGUI(val, CurrentDeck);

        if (result == DrawCardInfo.Result.Removed)
		{
			currentDeckEditorSelection =
				Mathf.Min(currentDeckEditorSelection, CurrentDeck.Cards.Count - 1);
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
				DeckReviewSequencer sequence = UnityEngine.Object.FindObjectOfType<DeckReviewSequencer>();
				if (sequence != null)
				{
					foreach (SelectableCard selectableCard in sequence.cardArray.displayedCards)
					{
						if (selectableCard?.Info == val)
							selectableCard.RenderCard();
					}
				}
			}
			// can't figure out, maybe later if requested
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
		GUILayout.BeginVertical(Array.Empty<GUILayoutOption>());
		GUILayout.Label("Card Finder", Array.Empty<GUILayoutOption>());
		lastCardSearch = GUILayout.TextField(lastCardSearch, Array.Empty<GUILayoutOption>());
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
		foundCardListScrollVector = GUILayout.BeginScrollView(foundCardListScrollVector, Array.Empty<GUILayoutOption>());
		if (lastSearchedList.Count > 0)
		{
			foreach (CardInfo lastSearched in lastSearchedList)
			{
				if (GUILayout.Button(lastSearched.name, Array.Empty<GUILayoutOption>()))
				{
					CardInfo obj = lastSearched.Clone() as CardInfo;
                    CurrentDeck.AddCard(obj);
					SaveManager.SaveToFile(false);
					currentDeckEditorSelection = CurrentDeck.Cards.Count;
				}
			}
		}
		else
		{
			GUILayout.Label("No Cards Found...", Array.Empty<GUILayoutOption>());
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
			if (name != null && name.ToLower().Contains(lower) || 
			    displayedName != null && displayedName.ToLower().Contains(lower))
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
		GUILayout.Label("Deck Viewer", Array.Empty<GUILayoutOption>());
		editDeckScrollVector = GUILayout.BeginScrollView(editDeckScrollVector, Array.Empty<GUILayoutOption>());
		deckCardArray = new string[CurrentDeck.Cards.Count + 1];
		deckCardArray[0] = "Add Card";
		for (int i = 0; i < CurrentDeck.Cards.Count; i++)
		{
			deckCardArray[i + 1] = CurrentDeck.Cards[i].displayedName;
		}

		currentDeckEditorSelection =
			GUILayout.SelectionGrid(currentDeckEditorSelection, deckCardArray, 2, Array.Empty<GUILayoutOption>());
		GUILayout.EndScrollView();
	}
}