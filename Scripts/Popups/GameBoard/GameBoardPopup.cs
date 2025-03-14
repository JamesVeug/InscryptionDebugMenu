using DebugMenu.Scripts.Popups.DeckEditorPopup;
using DebugMenu.Scripts.Utils;
using DiskCardGame;
using InscryptionAPI.Card;
using System.Collections;
using UnityEngine;

namespace DebugMenu.Scripts.Popups;

public class GameBoardPopup : BaseWindow
{
    public override string PopupName => "Game Board";
    public override Vector2 Size => new(850f, 600f);

    private const float width = 100f;

    public Tuple<PlayableCard, CardSlot> currentSelection = new(null, null);
    public bool selectedQueue = false;

    private string lastCardSearch = "";
    private List<CardInfo> lastSearchedList = null;
    private Vector2 foundCardListScrollVector = Vector2.zero;
    private Vector2 buttonSize = new(width, 134f);
    private ButtonDisabledData NoCard => new() { Disabled = currentSelection?.Item1 == null };
    public override void OnGUI()
    {
        if (GameFlowManager.m_Instance?.CurrentGameState != GameState.CardBattle)
        {
            IsActive = false;
            return;
        }
        base.OnGUI();
        if (BoardManager.m_Instance == null)
            return;

        int slotsPerSide = BoardManager.Instance.PlayerSlotsCopy.Count;
        LabelHeader("\nTotal Slots: " + BoardManager.Instance.AllSlots.Count + "\nOccupied Slots: " + BoardManager.Instance.AllSlotsCopy.Count(x => x.Card != null), new(200f, 80f), leftAligned: true);

        DisplayQueuedCards(slotsPerSide, GetAllQueuedCards(slotsPerSide));
        Padding(new(width * slotsPerSide, 120f));

        DisplayCardSlots(slotsPerSide, BoardManager.Instance.OpponentSlotsCopy);
        Padding(new(width * slotsPerSide, width));

        DisplayCardSlots(slotsPerSide, BoardManager.Instance.PlayerSlotsCopy);
        StartNewColumn(0);

        using (HorizontalScope(2))
        {
            if (Button("Clear Board", new(100f, 0), disabled: () => new(() => BoardManager.Instance.CardsOnBoard.Count == 0)))
            {
                foreach (PlayableCard item in BoardManager.Instance.CardsOnBoard)
                {
                    if (item != null)
                        RemoveFromBoard(item);
                }
            }
            if (Button("Clear Queue", new(100f, 0), disabled: () => new(() => TurnManager.Instance.Opponent.Queue.Count == 0)))
            {
                foreach (PlayableCard item in TurnManager.Instance.Opponent.Queue)
                {
                    item.ExitBoard(0.4f, Vector3.zero);
                }
                TurnManager.Instance.Opponent.Queue.Clear();
            }
        }
        using (HorizontalScope(2))
        {
            if (Button("Exit Board", new(100f, 0), disabled: () => NoCard))
            {
                RemoveFromBoard(currentSelection.Item1);
                currentSelection = new(null, currentSelection.Item2);
            }
            if (Button("Switch Sides", new(100f, 0), disabled: () => new(() => selectedQueue || currentSelection.Item1 == null)))
            {
                PlayableCard currentCard = currentSelection.Item1;
                CardSlot opposingSlot = currentSelection.Item2.opposingSlot;
                if (opposingSlot.Card != null)
                {
                    if (opposingSlot.IsPlayerSlot)
                    {
                        Plugin.Instance.StartCoroutine(AddBoardToHand(opposingSlot.Card));
                    }
                    else
                    {
                        Plugin.Instance.StartCoroutine(TurnManager.Instance.Opponent.ReturnCardToQueue(opposingSlot.Card, 0.25f));
                    }
                }
                currentCard.SetIsOpponentCard(!currentCard.OpponentCard);
                currentCard.transform.eulerAngles += new Vector3(0f, 0f, -180f);
                Plugin.Instance.StartCoroutine(BoardManager.Instance.AssignCardToSlot(currentCard, opposingSlot));
                currentSelection = new(null, currentSelection.Item2);
            }
        }
        StartNewColumn(0);
        if (Button("Add to Hand", new(100f, 0), disabled: () => NoCard))
        {
            RemoveFromBoard(currentSelection.Item1);
            Plugin.Instance.StartCoroutine(AddBoardToHand(currentSelection.Item1));
            currentSelection = new(null, currentSelection.Item2);
        }
        if (Button("Add to Queue", new(100f, 0), disabled: () => new(() => selectedQueue || currentSelection.Item1 == null)))
        {
            currentSelection.Item1.UnassignFromSlot();
            if (!currentSelection.Item1.OpponentCard)
            {
                currentSelection.Item1.SetIsOpponentCard(true);
                currentSelection.Item1.Slot = currentSelection.Item1.Slot.opposingSlot;
            }
            Plugin.Instance.StartCoroutine(TurnManager.Instance.Opponent.ReturnCardToQueue(currentSelection.Item1, 0.25f));
            currentSelection = new(null, currentSelection.Item2);
        }
        StartNewColumn(0);

        LabelHeader("Current Selection", leftAligned: true);
        if (currentSelection.Item2 == null)
        {
            Label("Nothing selected");
            return;
        }

        PlayableCard card = currentSelection.Item1;
        CardSlot slot = currentSelection.Item2;
        string label = (selectedQueue ? " (Queue" : (slot.IsPlayerSlot ? " (Player" : " (Opponent")) + " Slot)";
        Label("<b>Slot Index:</b> " + slot.Index + label + "\n<b>Card:</b> " + (card != null ? card.Info.DisplayedNameLocalized : "N/A"));

        using (HorizontalScope(2))
        {
            if (Button("Modify Slot", disabled: () => new(() => selectedQueue)))
            {
                Plugin.Instance.ToggleWindow<ModifySlotPopup>();
            }
            if (Button("Modify card", disabled: () => new(() => card == null)))
            {
                Plugin.Instance.ToggleWindow<BoardCardEditorPopup>();
            }
        }
        bool replace = card != null;
        if (replace)
        {
            using (HorizontalScope(5))
            {
                Label("Modify HP");
                if (Button("-1"))
                    Plugin.Instance.StartCoroutine(card.TakeDamage(1, null));
                if (Button("+1"))
                    card.HealDamage(1);
                if (Button("-5"))
                    Plugin.Instance.StartCoroutine(card.TakeDamage(5, null));
                if (Button("+5"))
                    card.HealDamage(5);
            }
            using (HorizontalScope(3))
            {
                Label("Kill card");
                if (Button("Activate Triggers"))
                {
                    Plugin.Instance.StartCoroutine(card.Die(false));
                    currentSelection = new(null, slot);
                    return;
                }

                if (Button("Ignore Triggers"))
                {
                    Plugin.Instance.StartCoroutine(KillCardTriggerless(card));
                    currentSelection = new(null, slot);
                    return;
                }
            }
            using (HorizontalScope(2))
            {
                if (Button("Reset damage", disabled: () => new(() => card.Status.damageTaken > 0)))
                {
                    card.Status.damageTaken = 0;
                    card.OnStatsChanged();
                }
                if (Button("Reset shield", disabled: () => new(() => !card.Status.lostShield)))
                {
                    card.ResetShield();
                }
            }
        }

        OnGUICardSearcher(slot, replace);
    }

    private IEnumerator AddBoardToHand(PlayableCard card)
    {
        CardInfo info = card.Info.Clone() as CardInfo;
        PlayableCardStatus status = new(card.Status);
        RemoveFromBoard(card);
        yield return CardSpawner.Instance.SpawnCardToHand(info, card.TemporaryMods, 0.25f, cardSpawnedCallback: (PlayableCard c) => c.Status = status);
    }
    private void RemoveFromBoard(PlayableCard card)
    {
        SpecialCardBehaviour[] components = card.GetComponents<SpecialCardBehaviour>();
        for (int i = 0; i < components.Length; i++)
        {
            components[i].OnCleanUp();
        }
        card.ExitBoard(0.25f, Vector3.zero);
    }
    private void OnGUICardSearcher(CardSlot slot, bool replacing)
    {
        (float x, float y, float w, float h) = GetPosition(new(ColumnWidth, 300f));
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
                if (GUILayout.Button($"{lastSearched.DisplayedNameLocalized}\n({lastSearched.name})", Helpers.ButtonWidth(ColumnWidth - 30f)))
                {
                    CardInfo obj = lastSearched.Clone() as CardInfo;
                    Plugin.Instance.StartCoroutine(FillChosenSlot(slot, obj, selectedQueue, replacingCard));
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

    private IEnumerator FillChosenSlot(CardSlot slot, CardInfo info, bool queueSlot, bool replacingCard)
    {
        if (replacingCard) // kill the card we're replacing to make room
        {
            PlayableCard cardToKill = queueSlot ? QueuedCardFromSlot(slot) : slot.Card;
            yield return KillCardTriggerless(cardToKill);
        }

        if (queueSlot)
        {
            yield return TurnManager.Instance.Opponent.QueueCard(info, slot);
            currentSelection = new(QueuedCardFromSlot(slot), slot);
        }
        else
        {
            yield return BoardManager.Instance.CreateCardInSlot(info, slot);
            currentSelection = new(slot.Card, slot);
        }
    }

    private bool GetCardsThatContain(string cardName, out List<int> results)
    {
        string lower = cardName.ToLower();
        bool atLeastOneResult = false;
        results = new List<int>();
        for (int i = 0; i < CardManager.AllCardsCopy.Count; i++)
        {
            CardInfo allCard = CardManager.AllCardsCopy[i];
            string name = allCard.name;
            string displayedName = allCard.displayedName;
            if ((name != null && name.ToLower().Contains(lower)) || (displayedName != null && displayedName.ToLower().Contains(lower)))
            {
                results.Add(i);
                atLeastOneResult = true;
            }
        }

        return atLeastOneResult;
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
    private PlayableCard QueuedCardFromSlot(CardSlot slot) => TurnManager.Instance.Opponent.Queue.Find(x => x.QueuedSlot == slot);
    private void DisplayCardSlots(int numToDisplay, List<CardSlot> slots)
    {
        string name = slots[0].IsPlayerSlot ? "Player" : "Opponent";
        using (HorizontalScope(numToDisplay))
        {
            for (int i = 0; i < numToDisplay; i++)
            {
                PlayableCard card = slots[i]?.Card;
                DisplayPlayableCard(name, i, card, slots[i], false);
            }
        }
    }
    private void DisplayQueuedCards(int numToDisplay, List<PlayableCard> cards)
    {
        using (HorizontalScope(numToDisplay))
        {
            for (int i = 0; i < numToDisplay; i++)
            {
                DisplayPlayableCard("Queue", i, cards[i], null, true);
            }
        }
    }
    private void DisplayPlayableCard(string slotName, int index, PlayableCard card, CardSlot slot, bool fromQueue)
    {
        string cardName = card == null ? "Empty" : $"{card.Info.name}\n({card.Info.DisplayedNameLocalized})";
        if (Button($"<b>{slotName}\nSlot {index}</b>\n{cardName}", buttonSize))
        {
            selectedQueue = fromQueue;
            if (selectedQueue)
                currentSelection = new(card, BoardManager.Instance.OpponentSlotsCopy.Find(x => x.Index == index));
            else
                currentSelection = new(card, slot);
        }
    }

    // returns a list of all queued slots plus null placeholders for empty queue slots
    private List<PlayableCard> GetAllQueuedCards(int count)
    {
        List<PlayableCard> result = new();
        for (int i = 0; i < count; i++)
        {
            PlayableCard queuedCard = QueuedCardFromSlot(BoardManager.Instance.OpponentSlotsCopy[i]);
            result.Add(queuedCard ?? null);
        }
        return result;
    }
}