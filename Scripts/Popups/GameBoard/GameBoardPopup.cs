using DebugMenu.Scripts.Utils;
using DiskCardGame;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers.Extensions;
using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DebugMenu.Scripts.Popups;

public class GameBoardPopup : BaseWindow
{
	public override string PopupName => "Game Board";
	public override Vector2 Size => new(950f, 600f);
	private Vector2 buttonSize = new(width, 134f);
    private const float width = 100f;
    
	private Tuple<PlayableCard, CardSlot> currentSelection = null;
    private bool selectedQueue = false;

    private string lastCardSearch = "";
    private List<CardInfo> lastSearchedList = null;
    private Vector2 foundCardListScrollVector = Vector2.zero;

    public override void OnGUI()
	{
		base.OnGUI();

        int slotsPerSide = BoardManager.Instance.PlayerSlotsCopy.Count;

        LabelHeader("Queue Slots", new(width * slotsPerSide, RowHeight));
        DisplayPlayableCards(slotsPerSide, GetAllQueueCards(slotsPerSide));
        Padding(new(width * slotsPerSide, width));
        LabelHeader("Opponent Slots", new(width * slotsPerSide, RowHeight));
        DisplayCardSlots(slotsPerSide, BoardManager.Instance.OpponentSlotsCopy);
        Padding(new(width * slotsPerSide, width));
        LabelHeader("Player Slots", new(width * slotsPerSide, RowHeight));
        DisplayCardSlots(slotsPerSide, BoardManager.Instance.PlayerSlotsCopy);

        // create enough space to accommodate 5 buttons
        StartNewColumn();
        StartNewColumn();
        StartNewColumn();

        Label("Card Slots: " + BoardManager.Instance.AllSlotsCopy.Count);
        Label("Occupied Slots: " + BoardManager.Instance.AllSlotsCopy.Count(x => x.Card != null));
        LabelHeader("Current Selection");

        if (currentSelection == null)
        {
            Label("Nothing selected");
            return;
        }

        PlayableCard card = currentSelection.Item1;
        CardSlot slot = currentSelection.Item2;
        bool replace = false;
        if (selectedQueue)
            Label("Queue Slot");
        else if (slot.IsPlayerSlot)
            Label("Player Slot");
        else
            Label("Opponent Slot");
        Label("Slot Index: " + slot.Index);
        Label("Card: " + (card != null ? card.Info.DisplayedNameLocalized : "N/A"));
        if (card != null)
        {
            replace = true;
            if (Button("Kill card"))
            {
                Plugin.Instance.StartCoroutine(card.Die(false));
                currentSelection = new(null, slot);
                return;
            }

            if (Button("Kill card (triggerless)"))
            {
                Plugin.Instance.StartCoroutine(KillCardTriggerless(card));
                currentSelection = new(null, slot);
                return;
            }
        }
        OnGUICardSearcher(slot, replace);
    }

    private void OnGUICardSearcher(CardSlot slot, bool replacing)
    {
        (float x, float y, float w, float h) = GetPosition(new(300f, 300f));
        GUILayout.BeginArea(new(x, y, w, h));
        GUILayout.Label(replacing ? "Replace Card in Slot" : "Create Card in Slot", LabelHeaderStyle);
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
        FoundCardList(slot, replacing);
    }

    private void FoundCardList(CardSlot slot, bool replacingCard)
    {
        foundCardListScrollVector = GUILayout.BeginScrollView(foundCardListScrollVector);
        if (lastSearchedList.Count > 0)
        {
            foreach (CardInfo lastSearched in lastSearchedList)
            {
                if (GUILayout.Button($"{lastSearched.DisplayedNameLocalized}\n({lastSearched.name})"))
                {
                    CardInfo obj = lastSearched.Clone() as CardInfo;
                    // kill the card we're replacing to make room
                    if (replacingCard)
                    {
                        PlayableCard cardToReplace = slot.Card;
                        if (selectedQueue)
                            cardToReplace = QueuedCardFromSlot(slot);

                        Plugin.Instance.StartCoroutine(KillCardTriggerless(cardToReplace));
                    }
                    // create the card in selected slot and then update the selection
                    if (selectedQueue)
                    {
                        // do it this way since there's a delay when queuing a card
                        Plugin.Instance.StartCoroutine(QueueCardAndUpdateCurrentSelection(obj, slot));
                    }
                    else
                    {
                        Plugin.Instance.StartCoroutine(BoardManager.Instance.CreateCardInSlot(obj, slot));
                        currentSelection = new(slot.Card, slot);
                    }
                    return;
                }
            }
        }
        else
        {
            GUILayout.Label("No Cards Found...");
        }
        GUILayout.EndScrollView();
        GUILayout.EndArea();
    }

    private IEnumerator QueueCardAndUpdateCurrentSelection(CardInfo info, CardSlot slot)
    {
        yield return TurnManager.Instance.Opponent.QueueCard(info, slot);
        currentSelection = new(QueuedCardFromSlot(slot), slot);
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

    private IEnumerator KillCardTriggerless(PlayableCard card)
    {
        if (!card.Dead)
        {
            card.Dead = true;
            card.Anim.SetShielded(shielded: false);
            yield return card.Anim.ClearLatchAbility();
            if (card.HasAbility(Ability.PermaDeath))
            {
                card.Anim.PlayPermaDeathAnimation();
                yield return new WaitForSeconds(1.25f);
            }
            else
                card.Anim.PlayDeathAnimation();

            // remove pack gameobject if killing a mule-type enemy
            if (card.HasSpecialAbility(SpecialTriggeredAbility.PackMule))
                UnityEngine.Object.Destroy(card.GetComponent<PackMule>().pack.gameObject);

            card.UnassignFromSlot();
            card.StartCoroutine(card.DestroyWhenStackIsClear());
        }
    }
    private PlayableCard QueuedCardFromSlot(CardSlot slot)
    {
        return TurnManager.Instance.Opponent.Queue.Find(x => x.QueuedSlot == slot);
    }
    private void DisplayCardSlots(int numToDisplay, List<CardSlot> slots)
	{
		using (HorizontalScope(numToDisplay))
		{
            for (int i = 0; i < numToDisplay; i++)
			{
				PlayableCard card = slots[i].Card;
				string cardName = card != null ? $"{card.Info.DisplayedNameLocalized}\n{card.Info.name}" : "Empty";
				if (Button($"Slot {i}\n{cardName}", buttonSize))
                {
                    currentSelection = new(card, slots[i]);
                    selectedQueue = false;
                }
			}
        }
	}
    private void DisplayPlayableCards(int numToDisplay, List<PlayableCard> cards)
    {
        using (HorizontalScope(numToDisplay))
        {
            for (int i = 0; i < numToDisplay; i++)
            {
                PlayableCard card = cards[i];
                string cardName = card != null ? $"{card.Info.DisplayedNameLocalized}\n({card.Info.name})" : "Empty";
                if (Button($"Slot {i}\n{cardName}", buttonSize))
                {
                    currentSelection = new(card, cards[i]?.QueuedSlot ?? BoardManager.Instance.OpponentSlotsCopy.Find(x => x.Index == i));
                    selectedQueue = true;
                }
            }
        }
    }

    // returns a list of all queued slots plus null placeholders for empty queue slots
    private List<PlayableCard> GetAllQueueCards(int count)
	{
		List<PlayableCard> result = new();
		for (int i = 0; i < count; i++)
		{
			PlayableCard queuedCard = QueuedCardFromSlot(BoardManager.Instance.OpponentSlotsCopy[i]);
			result.Add(queuedCard);
		}
		return result;
	}
}