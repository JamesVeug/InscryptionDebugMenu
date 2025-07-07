using DebugMenu.Scripts.Utils;
using DiskCardGame;
using InscryptionAPI.Slots;
using System.Collections;
using UnityEngine;

namespace DebugMenu.Scripts.Popups.DeckEditorPopup;

public class ModifySlotPopup : BaseWindow
{
    public override string PopupName => "Slot Modification";
    public override Vector2 Size => new(240f, 480f);

    private string lastCardSearch = "";
    private List<SlotModificationManager.ModificationType> lastSearchedList = null;
    private Vector2 foundCardListScrollVector = Vector2.zero;

    private CardSlot currentSelection = null;
    private bool changingSlot = false;

    private GameBoardPopup gameBoardPopup = null;
    public override void OnGUI()
    {
        if (GameFlowManager.m_Instance?.CurrentGameState != GameState.CardBattle)
        {
            IsActive = false;
            return;
        }
        gameBoardPopup ??= Plugin.Instance.GetWindow<GameBoardPopup>();
        currentSelection = gameBoardPopup.currentSelection.Item2;
        if (currentSelection == null)
            return;

        base.OnGUI();

        //modificationTypes ??= GuidManager.GetValues<SlotModificationManager.ModificationType>();
        SlotModificationManager.ModificationType type = currentSelection.GetSlotModification();
        string header = currentSelection.IsPlayerSlot ? "Player" : (gameBoardPopup.selectedQueue ? "Queue" : "Opponent");

        LabelHeader($"{header} Slot ({currentSelection.Index})");
        Label("<b>Current Slot Modification:</b> " + type.ToString());
        if (Button("Reset Slot Behaviour", disabled: () => new(() => changingSlot || currentSelection.GetSlotModification() == SlotModificationManager.ModificationType.NoModification)))
            Plugin.Instance.StartCoroutine(ChangeSlotModification(SlotModificationManager.ModificationType.NoModification));

        GUILayout.BeginArea(new Rect(5f, Size.y / 3f, Size.x - 10f, Size.y * 2f / 3f - 40f));
        OnGUISlotSearcher();
        GUILayout.EndArea();
    }

    private void OnGUISlotSearcher()
    {
        GUILayout.BeginVertical();
        GUILayout.BeginHorizontal();
        GUILayout.Label("Slot Mod Finder", Helpers.HeaderLabelStyle());
        GUILayout.EndHorizontal();
        lastCardSearch = GUILayout.TextField(lastCardSearch);
        lastSearchedList = new List<SlotModificationManager.ModificationType>();
        if (lastCardSearch != "")
        {
            if (GetSlotModByName(lastCardSearch, out var result))
            {
                lastSearchedList.Add(SlotModificationManager.AllModificationTypes[result]);
            }
            else if (GetSlotModsThatContain(lastCardSearch, out List<int> results))
            {
                foreach (int item in results)
                {
                    lastSearchedList.Add(SlotModificationManager.AllModificationTypes[item]);
                }
            }
        }
        else
        {
            lastSearchedList = SlotModificationManager.AllModificationTypes;
        }
        FoundCardList();
    }

    private void FoundCardList()
    {
        foundCardListScrollVector = GUILayout.BeginScrollView(foundCardListScrollVector);
        if (lastSearchedList.Count > 0)
        {
            foreach (SlotModificationManager.ModificationType lastSearched in lastSearchedList)
            {
                SlotModificationManager.Info info = SlotModificationManager.AllModificationInfos.InfoByID(lastSearched);
                string display = info.Name + $"\n({info.RulebookName})";
                if (changingSlot || currentSelection.GetSlotModification() == lastSearched)
                {
                    GUILayout.Label(display, Helpers.DisabledButtonStyle());
                }
                else if (GUILayout.Button(display))
                {
                    Plugin.Instance.StartCoroutine(ChangeSlotModification(lastSearched));
                }
            }
        }
        else
        {
            GUILayout.Label("No ModificationTypes Found...");
        }
        GUILayout.EndScrollView();

        GUILayout.EndVertical();
    }

    private IEnumerator ChangeSlotModification(SlotModificationManager.ModificationType modType)
    {
        changingSlot = true;
        yield return currentSelection.SetSlotModification(modType);
        changingSlot = false;
    }
    private bool GetSlotModsThatContain(string cardName, out List<int> results)
    {
        string lower = cardName.ToLower();
        bool result = false;
        results = new List<int>();
        for (int i = 0; i < SlotModificationManager.AllModificationInfos.Count; i++)
        {
            SlotModificationManager.Info info = SlotModificationManager.AllModificationInfos[i];
            bool matchName = info.Name != null && info.Name.ToLowerInvariant().Contains(lower);
            bool matchRulebook = info.RulebookName != null && info.RulebookName.ToLowerInvariant().Contains(lower);
            if (matchName || matchRulebook)
            {
                results.Add(i);
                result = true;
            }
        }

        return result;
    }

    private bool GetSlotModByName(string cardName, out int index)
    {
        bool exists = false;
        index = -1;

        for (int i = 0; i < SlotModificationManager.AllModificationTypes.Count; i++)
        {
            SlotModificationManager.Info info = SlotModificationManager.AllModificationInfos[i];
            if ((info.Name != null && info.Name.ToLowerInvariant() == cardName) || (info.RulebookName != null && info.RulebookName.ToLowerInvariant() == cardName))
            {
                index = i;
                exists = true;
                break;
            }
        }

        return exists;
    }
}