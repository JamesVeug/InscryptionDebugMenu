using DiskCardGame;
using InscryptionAPI.Card;
using UnityEngine;

namespace DebugMenu.Scripts.Popups;

public class AbilityInfoPopup : PaginatedWindow
{
    public override int NumElements => 45;

    private static AbilityInfo selectedInfo;

    public static List<AbilityManager.FullAbility> AllInfos = null;
    public static List<AbilityManager.FullAbility> VanillaInfos = null;
    public static List<AbilityManager.FullAbility> ModdedInfos = null;

    public override string PopupName => "Ability Infos";

    public override Vector2 Size => new(1050f, 950f);

    public AbilityInfoPopup()
    {
        ColumnWidth = 200f;
    }

    public override void HandleGuid()
    {
        using (HorizontalScope(2))
        {
            if (Button("Learn All", null, null, () => new("No ability infos")
            {
                Disabled = ModdedInfos == null || ModdedInfos.Count == 0
            }))
            {
                LearnAllAbilities(true);
            }

            if (Button("Unlearn All", null, null, () => new("No ability infos")
            {
                Disabled = ModdedInfos == null || ModdedInfos.Count == 0
            }))
            {
                LearnAllAbilities(false);
            }
        }
        base.HandleGuid();
    }
    public override bool HandleToggles()
    {
        bool toggleChanged = base.HandleToggles();
        toggleChanged = Toggle("Filter by Grimora", ref FilterByGrimora) || toggleChanged;
        toggleChanged = Toggle("Filter by Magnificus", ref FilterByMagnificus) || toggleChanged;
        return toggleChanged;
    }

    public override void OnGUI()
    {
        AllInfos = new(AbilityManager.AllAbilities);
        VanillaInfos = AbilityManager.BaseGameAbilities.ToList();
        ModdedInfos = AbilityManager.AllAbilities.FindAll(x => !string.IsNullOrEmpty(x.ModGUID));

        HandleOnGUI("Ability Infos", AllInfos, VanillaInfos, ModdedInfos);

        List<AbilityManager.FullAbility> infoToShow = new();
        if (ShowAll)
        {
            infoToShow = AllInfos;
        }
        else if (ShowVanilla && !ShowModded)
        {
            infoToShow = VanillaInfos;
        }
        else if (ShowModded)
        {
            infoToShow = ModdedInfos;
        }

        if (!string.IsNullOrEmpty(filterText))
        {
            filterText = filterText.ToLowerInvariant();
            if (FilterByGuid)
            {
                infoToShow.RemoveAll(x => string.IsNullOrEmpty(x.ModGUID) || !x.ModGUID.ToLowerInvariant().Contains(filterText));
            }
            else
            {
                List<AbilityManager.FullAbility> abs = new();
                foreach (AbilityManager.FullAbility ab in infoToShow)
                {
                    if (!int.TryParse(ab.Id.ToString(), out _) && ab.Id.ToString().ToLowerInvariant().Contains(filterText))
                        abs.Add(ab);
                    else if (!string.IsNullOrEmpty(ab.Info.rulebookName) && ab.Info.rulebookName.ToLowerInvariant().Contains(filterText))
                        abs.Add(ab);
                    else if (ab.AbilityBehavior != null && ab.AbilityBehavior.Name.ToLowerInvariant().Contains(filterText))
                        abs.Add(ab);
                }

                infoToShow = abs;
            }
        }

        if (FilterByAct1)
            infoToShow.RemoveAll(x => !Part1Ability(x.Info));

        if (FilterByAct2)
            infoToShow.RemoveAll(x => !Part2Ability(x.Info));

        if (FilterByAct3)
            infoToShow.RemoveAll(x => !Part3Ability(x.Info));

        if (FilterByAscension)
            infoToShow.RemoveAll(x => !AscensionAbility(x.Info));

        if (FilterByGrimora)
            infoToShow.RemoveAll(x => !GrimoraAbility(x.Info));

        if (FilterByMagnificus)
            infoToShow.RemoveAll(x => !MagnificusAbility(x.Info));

        HandlePages(infoToShow.Count);
        StartNewColumn();

        if (selectedInfo != null)
        {
            Label(FullAbilityInfo(selectedInfo, currentGuid), new Vector2(0, 600));
        }
        else if (infoToShow == null || infoToShow.Count == 0)
        {
            Label("No results");
            return;
        }
        StartNewColumn();

        startIndex = pageNum * NumElements;
        if (pageNum > infoToShow.Count / NumElements)
        {
            pageNum = infoToShow.Count / NumElements;
            endIndex = infoToShow.Count;
        }
        else
        {
            endIndex = Mathf.Min(infoToShow.Count, pageNum * NumElements + NumElements);
        }

        int row = 1;
        for (int i = startIndex; i < endIndex; i++)
        {
            AbilityInfo info = infoToShow[i].Info;
            string pluginGuid = infoToShow[i].ModGUID ?? "n/a";

            string textToDisplay = $"{info.rulebookName}\n({pluginGuid})";
            if (Button(textToDisplay, new(0f, 60f), null))
            {
                selectedInfo = info;
                currentGuid = pluginGuid;
            }
            if (row >= NumElements / 3)
            {
                StartNewColumn();
                row = 0;
            }
            row++;
        }
    }

    private void LearnAllAbilities(bool learnAll)
    {
        if (learnAll)
        {
            ProgressionData.Data.learnedAbilities.AddRange(AllInfos.Where((a) => !ProgressionData.Data.learnedAbilities.Contains(a.Id)).Select((a) => a.Id));
        }
        else
        {
            ProgressionData.Data.learnedAbilities.Clear();
        }
        SaveManager.SaveToFile(false);
    }

    private string FullAbilityInfo(AbilityInfo info, string guid = null)
    {
        guid ??= AbilityManager.AllAbilities.Find(x => x.Info == info)?.ModGUID ?? "";

        return
            "GUID:\n" + guid +
            "\nAbility: " + info.ability + 
            "\nRulebook Name:\n" + info.rulebookName + 
            "\nDescription:\n" + info.rulebookDescription +
            "\nPowerlevel: " + info.powerLevel + 
            "\nCanStack: " + info.canStack + 
            "\nOpponentUsable: " + info.opponentUsable +
            "\nFlipYIfOpponent: " + info.flipYIfOpponent +
            "\nActivated: " + info.activated + 
            "\nPassive: " + info.passive +
            "\nConduit: " + info.conduit +
            "\nConduitCell: " + info.conduitCell + 
            "\nHideSingleStack: " + info.GetHideSingleStacks() +
            "\nTriggersOncePerStack " + info.GetTriggersOncePerStack();
    }
    public override void PrintAllInfoToLog()
    {
        foreach (AbilityInfo info in AllInfos.Select(x => x.Info))
        {
            Plugin.Log.LogMessage(FullAbilityInfo(info) + "\n");
        }
        Plugin.Log.LogInfo($"Total All: {AllInfos.Count}");
    }

    public override void PrintVanillaInfoToLog()
    {
        foreach (AbilityInfo info in VanillaInfos.Select(x => x.Info))
        {
            Plugin.Log.LogMessage(FullAbilityInfo(info) + "\n");
        }
        Plugin.Log.LogInfo($"Total Vanilla: {VanillaInfos.Count}");
    }

    public override void PrintModdedInfoToLog()
    {
        foreach (AbilityInfo info in ModdedInfos.Select(x => x.Info))
        {
            Plugin.Log.LogMessage(FullAbilityInfo(info) + "\n");
        }
        Plugin.Log.LogInfo($"Total Modded: {ModdedInfos.Count}");
    }

    public override void PrintSelectedInfoToLog()
    {
        Dictionary<AbilityInfo, string> dictionary = new();
        foreach (AbilityManager.FullAbility full in AllInfos)
        {
            if (currentGuid == full.ModGUID && !dictionary.ContainsKey(full.Info))
            {
                dictionary.Add(full.Info, full.ModGUID);
            }
        }

        string arg = dictionary.Values.FirstOrDefault();
        foreach (KeyValuePair<AbilityInfo, string> item in dictionary)
        {
            Plugin.Log.LogMessage(FullAbilityInfo(item.Key, arg ?? ""));
        }
        Plugin.Log.LogInfo($"GUID: {arg ?? ("No GUID"),-30}");
        Plugin.Log.LogInfo($"Total Selected: {dictionary.Count}");
    }

    public bool Part1Ability(AbilityInfo info) => info.metaCategories.Exists(x => x == AbilityMetaCategory.Part1Rulebook);
    public bool Part2Ability(AbilityInfo info) => info.pixelIcon != null;
    public bool Part3Ability(AbilityInfo info) => info.metaCategories.Exists(x => x == AbilityMetaCategory.Part3Rulebook);
    public bool AscensionAbility(AbilityInfo info) => info.metaCategories.Exists(x => x == AbilityMetaCategory.AscensionUnlocked);
    public bool GrimoraAbility(AbilityInfo info) => info.metaCategories.Exists(x => x == AbilityMetaCategory.GrimoraRulebook);
    public bool MagnificusAbility(AbilityInfo info) => info.metaCategories.Exists(x => x == AbilityMetaCategory.MagnificusRulebook);
}