using DebugMenu.Scripts.Popups.DeckEditorPopup;
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
	public override Vector2 Size => new(900f, 600f);
	private Vector2 buttonSize = new(width, 134f);
    private const float width = 100f;
    
	private Tuple<PlayableCard, CardSlot> currentSelection = new(null, null);
    private bool selectedQueue = false;

    private string lastCardSearch = "";
    private List<CardInfo> lastSearchedList = null;
    private Vector2 foundCardListScrollVector = Vector2.zero;

    public override void OnGUI()
	{
		base.OnGUI();
        if (BoardManager.m_Instance == null)
            return;

        int slotsPerSide = BoardManager.Instance.PlayerSlotsCopy.Count;

        // create buttons for each card slot (plus the queue slots which aren't technically card slots? okay)
        LabelHeader("Queue Slots", new(width * slotsPerSide, RowHeight));
        DisplayCardSlots(slotsPerSide, GetAllQueueSlots(slotsPerSide), true);
        Padding(new(width * slotsPerSide, width));

        LabelHeader("Opponent Slots", new(width * slotsPerSide, RowHeight));
        DisplayCardSlots(slotsPerSide, BoardManager.Instance.OpponentSlotsCopy, false);
        Padding(new(width * slotsPerSide, width));

        LabelHeader("Player Slots", new(width * slotsPerSide, RowHeight));
        DisplayCardSlots(slotsPerSide, BoardManager.Instance.PlayerSlotsCopy, false);

        // create enough space to accommodate 5 buttons
        StartNewColumn();
        StartNewColumn();
        StartNewColumn();

        using (HorizontalScope(2))
        {
            Label("<b>Total Slots:</b> " + BoardManager.Instance.AllSlots.Count);
            Label("<b>Occupied Slots:</b> " + BoardManager.Instance.AllSlotsCopy.Count(x => x.Card != null));
        }
        
        using (HorizontalScope(2))
        {
            if (Button("Clear Board"))
                Plugin.Instance.StartCoroutine(BoardManager.Instance.ClearBoard());

            if (Button("Clear Queue"))
                Plugin.Instance.StartCoroutine(TurnManager.Instance.Opponent.ClearQueue());
        }

        LabelHeader("Current Selection", leftAligned: true);

        if (currentSelection.Item2 == null)
        {
            Label("Nothing selected");
            return;
        }

        PlayableCard card = currentSelection.Item1;
        CardSlot slot = currentSelection.Item2;
        
        string label = (selectedQueue ? "Queue" : (slot.IsPlayerSlot ? "Player" : "Opponent")) + " Slot";
        Label(label + "  |  Slot Index: " + slot.Index + "\nCard: " + (card != null ? card.Info.DisplayedNameLocalized : "N/A"));

        bool replace = false;
        if (card != null)
        {
            replace = true;
            using (HorizontalScope(3))
            {
                Label("Damage card");
                if (Button("-1 HP"))
                    Plugin.Instance.StartCoroutine(card.TakeDamage(1, null));
                if (Button("-5 HP"))
                    Plugin.Instance.StartCoroutine(card.TakeDamage(5, null));
            }
            using (HorizontalScope(3))
            {
                Label("Heal card");
                if (Button("+1 HP"))
                    card.HealDamage(1);
                if (Button("+5 HP"))
                    card.HealDamage(5);
            }
            using (HorizontalScope(3))
            {
                Label("Kill card");
                if (Button("Activate triggers"))
                {
                    Plugin.Instance.StartCoroutine(card.Die(false));
                    currentSelection = new(null, slot);
                    return;
                }

                if (Button("No triggers"))
                {
                    Plugin.Instance.StartCoroutine(KillCardTriggerless(card));
                    currentSelection = new(null, slot);
                    return;
                }
            }

            if (Button("Modify card"))
            {
                var window = Plugin.Instance.ToggleWindow<BoardCardEditorPopup>();
                window.currentSelection = currentSelection.Item1;
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
    private void DisplayCardSlots(int numToDisplay, List<CardSlot> slots, bool fromQueue)
	{
		using (HorizontalScope(numToDisplay))
		{
            for (int i = 0; i < numToDisplay; i++)
			{
				PlayableCard card = slots[i]?.Card;
                DisplayPlayableCard(i, card, slots[i], fromQueue);
			}
        }
	}
    private void DisplayPlayableCard(int index, PlayableCard card, CardSlot slot, bool fromQueue)
    {
        string cardName = card == null ? "Empty" : $"{card.Info.name}\n({card.Info.DisplayedNameLocalized})";
        if (Button($"<b>Slot {index}</b>\n{cardName}", buttonSize))
        {
            selectedQueue = fromQueue;
            if (selectedQueue)
                currentSelection = new(card, card?.QueuedSlot ?? BoardManager.Instance.OpponentSlotsCopy.Find(x => x.Index == index));
            else
                currentSelection = new(card, slot);
        }
    }

    // returns a list of all queued slots plus null placeholders for empty queue slots
    private List<CardSlot> GetAllQueueSlots(int count)
	{
		List<CardSlot> result = new();
		for (int i = 0; i < count; i++)
		{
			PlayableCard queuedCard = QueuedCardFromSlot(BoardManager.Instance.OpponentSlotsCopy[i]);
			result.Add(queuedCard?.QueuedSlot ?? null);
		}
		return result;
	}
}