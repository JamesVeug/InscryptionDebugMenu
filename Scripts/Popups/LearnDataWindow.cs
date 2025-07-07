using DiskCardGame;
using InscryptionAPI.Card;
using InscryptionAPI.Guid;
using InscryptionAPI.Items.Extensions;
using UnityEngine;

namespace DebugMenu.Scripts.Popups;

public class LearnDataPopup : PaginatedWindow
{
    public override int NumElements => 45;
    public override string PopupName => "Progression Data";
    public override Vector2 Size => new(1050f, 950f);

    public Progression shownMenu = Progression.IntroducedCards;

    public static List<ProgressEntry> allEntries = null;
    public static List<ProgressEntry> entriesToShow = null;

    public static bool LearnedOnly = false;
    public static bool UnlearnedOnly = false;
    public LearnDataPopup()
    {
        ColumnWidth = 200f;
    }

    public override void HandleGuid()
    {
        LabelHeader("Current Category");
        LabelHeader(shownMenu.ToString() + "\n");
    }
    public override bool HandleToggles()
    {
        bool toggle = Toggle("Show Learned Only", ref LearnedOnly);
        if (toggle && LearnedOnly)
        {
            UnlearnedOnly = false;
        }
        toggle = Toggle("Show Unlearned Only", ref UnlearnedOnly);
        if (toggle && UnlearnedOnly)
        {
            LearnedOnly = false;
        }
        return toggle;
    }

    public override void OnGUI()
    {
        allEntries ??= CardManager.AllCardsCopy.Select(x => new ProgressEntry(x.name, Progression.IntroducedCards))
            .Concat(CardManager.AllCardsCopy.Select(x => new ProgressEntry(x.name, Progression.LearnedCards)))
            .Concat(AbilityManager.AllAbilities.Select(x => new ProgressEntry(x.Id)))
            .Concat(ItemsUtil.AllConsumables.Select(x => new ProgressEntry(x.name, Progression.IntroducedItems)))
            .Concat(GuidManager.GetValues<MechanicsConcept>().Select(x => new ProgressEntry(x)))
            .ToList();

        if (SaveManager.savingDisabled)
        {
            Label("Saving disabled; cannot modify ProgressionData.");
            return;
        }

        entriesToShow = allEntries.Where(x => x.progressionType == shownMenu).ToList();

        base.OnGUI();
        if (entriesToShow == null || entriesToShow.Count == 0)
        {
            Label("No results");
            return;
        }

        HandleGuid();

        if (Button("Learn All in Category"))
        {
            LearnAll(shownMenu);
            return;
        }
        if (Button("Unlearn All in Category"))
        {
            UnlearnAll(shownMenu);
            return;
        }
        if (Button("Learn All Data"))
        {
            LearnAll(Progression.IntroducedCards & Progression.LearnedCards & Progression.LearnedAbilities & Progression.IntroducedItems & Progression.LearnedMechanics);
            return;
        }
        if (Button("Unlearn All Data"))
        {
            UnlearnAll(Progression.IntroducedCards & Progression.LearnedCards & Progression.LearnedAbilities & Progression.IntroducedItems & Progression.LearnedMechanics);
            return;
        }

        if (Button("<b>Next Category</b>"))
        {
            shownMenu = shownMenu switch
            {
                Progression.IntroducedCards => Progression.LearnedCards,
                Progression.LearnedCards => Progression.LearnedAbilities,
                Progression.LearnedAbilities => Progression.IntroducedItems,
                Progression.IntroducedItems => Progression.LearnedMechanics,
                _ => Progression.IntroducedCards,
            };
            return;
        }

        HandleFilterText();
        if (HandleToggles())
            pageNum = startIndex = endIndex = 0;

        if (LearnedOnly)
        {
            switch (shownMenu)
            {
                case Progression.IntroducedCards:
                    entriesToShow.RemoveAll(x => !ProgressionData.Data.introducedCards.Contains(x.nameKey));
                    break;
                case Progression.LearnedCards:
                    entriesToShow.RemoveAll(x => !ProgressionData.Data.learnedCards.Contains(x.nameKey));
                    break;
                case Progression.LearnedAbilities:
                    entriesToShow.RemoveAll(x => !ProgressionData.Data.learnedAbilities.Contains(x.ability));
                    break;
                case Progression.IntroducedItems:
                    entriesToShow.RemoveAll(x => !ProgressionData.Data.introducedConsumables.Contains(x.nameKey));
                    break;
                case Progression.LearnedMechanics:
                    entriesToShow.RemoveAll(x => !ProgressionData.Data.learnedMechanics.Contains(x.mechanicConcept));
                    break;
            }
        }
        else if (UnlearnedOnly)
        {
            switch (shownMenu)
            {
                case Progression.IntroducedCards:
                    entriesToShow.RemoveAll(x => ProgressionData.Data.introducedCards.Contains(x.nameKey));
                    break;
                case Progression.LearnedCards:
                    entriesToShow.RemoveAll(x => ProgressionData.Data.learnedCards.Contains(x.nameKey));
                    break;
                case Progression.LearnedAbilities:
                    entriesToShow.RemoveAll(x => ProgressionData.Data.learnedAbilities.Contains(x.ability));
                    break;
                case Progression.IntroducedItems:
                    entriesToShow.RemoveAll(x => ProgressionData.Data.introducedConsumables.Contains(x.nameKey));
                    break;
                case Progression.LearnedMechanics:
                    entriesToShow.RemoveAll(x => ProgressionData.Data.learnedMechanics.Contains(x.mechanicConcept));
                    break;
            }
        }

        if (!string.IsNullOrEmpty(filterText))
        {
            filterText = filterText.ToLowerInvariant();
            if (FilterByGuid)
            {
                entriesToShow.RemoveAll(x => string.IsNullOrEmpty(x.ModGuid) || !x.ModGuid.ToLowerInvariant().Contains(filterText));
            }
            else
            {
                List<ProgressEntry> filteredEntries = new();
                foreach (ProgressEntry entry in entriesToShow)
                {
                    switch (shownMenu)
                    {
                        case Progression.LearnedAbilities:
                            if (!int.TryParse(entry.ability.ToString(), out _) && entry.ability.ToString().ToLowerInvariant().Contains(filterText))
                                filteredEntries.Add(entry);
                            else
                                goto default;
                            break;
                        case Progression.LearnedMechanics:
                            if (!int.TryParse(entry.mechanicConcept.ToString(), out _) && entry.mechanicConcept.ToString().ToLowerInvariant().Contains(filterText))
                                filteredEntries.Add(entry);
                            else
                                goto default;
                            break;
                        default:
                            if (!string.IsNullOrEmpty(entry.nameKey) && entry.nameKey.ToLowerInvariant().Contains(filterText))
                                filteredEntries.Add(entry);
                            else if (!string.IsNullOrEmpty(entry.DisplayName) && entry.DisplayName.ToLowerInvariant().Contains(filterText))
                                filteredEntries.Add(entry);
                            break;
                    }
                }

                entriesToShow = filteredEntries;
            }
        }

        HandlePages(entriesToShow.Count);
        StartNewColumn();

        if (entriesToShow == null || entriesToShow.Count == 0)
        {
            Label("No results");
            return;
        }
        StartNewColumn();

        startIndex = pageNum * NumElements;
        if (pageNum > entriesToShow.Count / NumElements)
        {
            pageNum = entriesToShow.Count / NumElements;
            endIndex = entriesToShow.Count;
        }
        else
        {
            endIndex = Mathf.Min(entriesToShow.Count, pageNum * NumElements + NumElements);
        }

        int row = 1;
        for (int i = startIndex; i < endIndex; i++)
        {
            ProgressEntry info = entriesToShow[i];
            string pluginGuid = entriesToShow[i].ModGuid ?? "n/a";

            string textToDisplay = $"{info.DisplayName}\n({pluginGuid})\nLearned: {info.HasLearnt()}";
            if (Button(textToDisplay, new(0f, 60f), null))
            {
                if (info.ability != Ability.None)
                {
                    LearnAbility(info.ability, !info.HasLearnt());
                }
                else if (info.mechanicConcept != MechanicsConcept.NUM_MECHANICS)
                {
                    LearnMechanic(info.mechanicConcept, !info.HasLearnt());
                }
                else if (!info.HasLearnt())
                {
                    LearnString(info.nameKey, shownMenu);
                }
                else
                {
                    UnlearnString(info.nameKey, shownMenu);
                }
                return;
            }

            if (row >= NumElements / 3)
            {
                StartNewColumn();
                row = 0;
            }
            row++;
        }
    }

    private void LearnAbility(Ability ability, bool setLearned)
    {
        if (setLearned)
            ProgressionData.SetAbilityLearned(ability);
        else
            ProgressionData.Data.learnedAbilities.Remove(ability);

        SaveManager.SaveToFile(false);
    }

    private void LearnMechanic(MechanicsConcept concept, bool setLearned)
    {
        if (setLearned)
            ProgressionData.SetMechanicLearned(concept);
        else
            ProgressionData.Data.learnedMechanics.Remove(concept);

        SaveManager.SaveToFile(false);
    }

    private void LearnString(string key, Progression selection)
    {
        if (selection == Progression.IntroducedCards || selection == Progression.LearnedCards)
        {
            CardInfo info = CardLoader.GetCardByName(key);
            if (info == null)
            {
                Plugin.Log.LogError($"Could not find CardInfo for card with name [{key}]");
                return;
            }

            if (selection == Progression.IntroducedCards)
                ProgressionData.SetCardIntroduced(info);
            else
                ProgressionData.SetCardLearned(info);
        }
        else if (selection == Progression.IntroducedItems)
        {
            ItemData data = ItemsUtil.GetConsumableByName(key);
            if (data == null)
            {
                Plugin.Log.LogError($"Could not find ItemData for item with name [{key}]");
                return;
            }
            ProgressionData.SetConsumableIntroduced(data);
        }

        SaveManager.SaveToFile(false);
    }
    private void UnlearnString(string key, Progression selection)
    {
        if (selection == Progression.IntroducedCards)
            ProgressionData.Data.introducedCards.Remove(key);
        else if (selection == Progression.LearnedCards)
            ProgressionData.Data.learnedCards.Remove(key);
        else if (selection == Progression.IntroducedItems)
            ProgressionData.Data.introducedConsumables.Remove(key);

        SaveManager.SaveToFile(false);
    }

    private void LearnAll(Progression selection)
    {
        if (selection.HasFlag(Progression.IntroducedCards))
        {
            Debug.Log("Learn cards");
            foreach (CardInfo info in CardManager.AllCardsCopy)
            {
                ProgressionData.SetCardIntroduced(info);
            }
        }

        if (selection.HasFlag(Progression.LearnedCards))
        {
            foreach (CardInfo info in CardManager.AllCardsCopy)
            {
                ProgressionData.SetCardLearned(info);
            }
        }

        if (selection.HasFlag(Progression.LearnedAbilities))
        {
            foreach (AbilityManager.FullAbility ability in AbilityManager.AllAbilities)
            {
                ProgressionData.SetAbilityLearned(ability.Id);
            }
        }

        if (selection.HasFlag(Progression.IntroducedItems))
        {
            foreach (ConsumableItemData data in ItemsUtil.AllConsumables)
            {
                ProgressionData.SetConsumableIntroduced(data);
            }
        }

        if (selection.HasFlag(Progression.LearnedMechanics))
        {
            foreach (MechanicsConcept concept in GuidManager.GetValues<MechanicsConcept>())
            {
                ProgressionData.SetMechanicLearned(concept);
            }
        }

        SaveManager.SaveToFile(false);
    }

    private void UnlearnAll(Progression selection)
    {
        if (selection.HasFlag(Progression.IntroducedCards))
            ProgressionData.Data.introducedCards.Clear();

        if (selection.HasFlag(Progression.LearnedCards))
            ProgressionData.Data.learnedCards.Clear();

        if (selection.HasFlag(Progression.LearnedAbilities))
            ProgressionData.Data.learnedAbilities.Clear();

        if (selection.HasFlag(Progression.IntroducedItems))
            ProgressionData.Data.introducedConsumables.Clear();

        if (selection.HasFlag(Progression.LearnedMechanics))
            ProgressionData.Data.learnedMechanics.Clear();

        SaveManager.SaveToFile(false);
    }

    public override void PrintAllInfoToLog() { }
    public override void PrintVanillaInfoToLog() { }
    public override void PrintModdedInfoToLog() { }
    public override void PrintSelectedInfoToLog() { }

    [Flags]
    public enum Progression
    {
        IntroducedCards = 1,
        LearnedCards = 2,
        LearnedAbilities = 4,
        IntroducedItems = 8,
        LearnedMechanics = 16
    }

    public class ProgressEntry
    {
        public string DisplayName;
        public string ModGuid;
        public bool HasLearnt()
        {
            return progressionType switch
            {
                Progression.IntroducedCards => ProgressionData.Data.introducedCards.Contains(nameKey),
                Progression.LearnedCards => ProgressionData.Data.learnedCards.Contains(nameKey),
                Progression.LearnedAbilities => ProgressionData.LearnedAbility(ability),
                Progression.IntroducedItems => ProgressionData.Data.introducedConsumables.Contains(nameKey),
                _ => ProgressionData.LearnedMechanic(mechanicConcept),
            };
        }
        public Progression progressionType;

        public string nameKey = null;
        public Ability ability = Ability.None;
        public MechanicsConcept mechanicConcept = MechanicsConcept.NUM_MECHANICS;

        public ProgressEntry(string key, Progression selection)
        {
            this.nameKey = key;
            this.progressionType = selection;
            switch (selection)
            {
                case Progression.IntroducedCards:
                    CardInfo info = CardLoader.GetCardByName(key);
                    DisplayName = info.DisplayedNameLocalized;
                    ModGuid = info.GetModPrefix();
                    break;
                case Progression.LearnedCards:
                    CardInfo info2 = CardLoader.GetCardByName(key);
                    DisplayName = info2.DisplayedNameLocalized;
                    ModGuid = info2.GetModPrefix();
                    break;
                case Progression.IntroducedItems:
                    ConsumableItemData data = ItemsUtil.GetConsumableByName(key);
                    DisplayName = data.rulebookName;
                    ModGuid = data.GetModPrefix();
                    break;
            }
        }
        public ProgressEntry(Ability ability)
        {
            AbilityManager.FullAbility info = AbilityManager.AllAbilities.AbilityByID(ability);
            this.DisplayName = info.Info.rulebookName;
            this.ModGuid = info.ModGUID;
            this.ability = ability;
            this.progressionType = Progression.LearnedAbilities;
        }
        public ProgressEntry(MechanicsConcept concept)
        {
            GuidManager.TryGetGuidAndKeyEnumValue(concept, out string guid, out string foo);
            if (string.IsNullOrEmpty(guid))
            {
                this.DisplayName = concept.ToString();
                this.ModGuid = null;
            }
            else
            {
                this.DisplayName = foo;
                this.ModGuid = guid;
            }
            this.mechanicConcept = concept;
            this.progressionType = Progression.LearnedMechanics;
        }
    }
}