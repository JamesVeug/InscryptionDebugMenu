using DebugMenu.Scripts.Utils;
using DiskCardGame;
using GBC;
using InscryptionAPI.Card;
using InscryptionAPI.Dialogue;
using System.Collections;
using System.Text;
using UnityEngine;
using static DialogueDataUtil;
using static InscryptionAPI.Dialogue.DialogueManager;

namespace DebugMenu.Scripts.Popups;

public class AbilityInfoPopup : BaseWindow
{
    private static bool FilterByGuid = false;

    private static bool ShowAll = true;

    private static bool ShowVanilla = true;

    private static bool ShowModded = true;

    private static bool FilterByAct1 = false;

    private static bool FilterByAct2 = false;

    private static bool FilterByAct3 = false;

    private static bool FilterByGrimora = false;

    private static bool FilterByMagnificus = false;

    private const int numElements = 45;

    public static int pageNum = 0;

    public static int startIndex = 0;

    public static int endIndex = numElements;

    private static AbilityInfo selectedInfo;
    private string filterText;

    private string currentGuid;

    public static List<AbilityInfo> AllInfos = null;

    public static List<AbilityInfo> VanillaInfos = null;

    public static List<AbilityInfo> ModdedInfos = null;

    public override string PopupName => "Ability Infos";

    public override Vector2 Size => new(1050f, 950f);

    public AbilityInfoPopup()
    {
        ColumnWidth = 200f;
    }

    public override void OnGUI()
    {
        base.OnGUI();

        AllInfos = AbilityManager.AllAbilityInfos;
        VanillaInfos = AbilityManager.BaseGameAbilities.Select(x => x.Info).ToList();
        ModdedInfos = AbilityManager.AllAbilities.Where(x => !string.IsNullOrEmpty(x.ModGUID)).Select(x => x.Info).ToList();

        if (AllInfos == null || AllInfos.Count == 0)
        {
            Label("No ability infos");
            return;
        }
        if (Button("Print All Ability Infos"))
        {
            PrintAllAbilitiesToLog();
        }
        if (Button("Print Vanilla Ability Infos", null, null, () => new("Couldn't load vanilla abilities!")
        {
            Disabled = VanillaInfos == null || VanillaInfos.Count == 0
        }))
        {
            PrintVanillaAbilitiesToLog();
        }
        if (Button("Print Modded Ability Infos", null, null, () => new("No ability infos")
        {
            Disabled = ModdedInfos == null || ModdedInfos.Count == 0
        }))
        {
            PrintModdedAbilitiesToLog();
        }
        if (Button("Print Ability Infos by Selected GUID"))
        {
            Dictionary<AbilityInfo, string> dictionary = new();
            foreach (AbilityInfo info in AllInfos)
            {
                string text = AbilityManager.AllAbilities.Find(x => x.Info == info)?.ModGUID;
                if (currentGuid == text && !dictionary.ContainsKey(info))
                {
                    dictionary.Add(info, text);
                }
            }
            PrintGUIDsToLog(dictionary);
        }
        
        Label("Selected GUID: " + currentGuid);
        
        Label("Search Filter", (Vector2?)new Vector2(0f, base.RowHeight / 2f));
        filterText = TextField(filterText, (Vector2?)new Vector2(0f, base.RowHeight / 2f));
        
        Toggle("Filter by GUID", ref FilterByGuid);
        
        if (!ShowVanilla || !ShowModded)
            ShowAll = false;

        bool toggleChanged = Toggle("Show All Abilities", ref ShowAll);
        if (toggleChanged)
            ShowModded = ShowVanilla = ShowAll;
            FilterByAct1 = FilterByAct2 = FilterByAct3 = FilterByGrimora = FilterByMagnificus = false;

        toggleChanged |= Toggle("Show Modded Abilities", ref ShowModded);
        if (toggleChanged && ShowModded)
            FilterByAct1 = FilterByAct2 = FilterByAct3 = FilterByAct3 = FilterByGrimora = FilterByMagnificus = false;

        toggleChanged |= Toggle("Show Vanilla Abilities", ref ShowVanilla);
        if (toggleChanged && ShowVanilla)
            FilterByAct1 = FilterByAct2 = FilterByAct3 = FilterByAct3 = FilterByGrimora = FilterByMagnificus = false;

        toggleChanged |= Toggle("Filter by Act 1", ref FilterByAct1);
        toggleChanged |= Toggle("Filter by Act 2", ref FilterByAct2);
        toggleChanged |= Toggle("Filter by Act 3", ref FilterByAct3);
        toggleChanged |= Toggle("Filter by Grimora", ref FilterByGrimora);
        toggleChanged |= Toggle("Filter by Magnificus", ref FilterByMagnificus);

        List<AbilityInfo> abilitiesToShow = new();
        if (ShowAll || (ShowVanilla && ShowModded))
        {
            abilitiesToShow = AllInfos;
        }
        else if (ShowVanilla && !ShowModded)
        {
            abilitiesToShow = VanillaInfos;
        }
        else if (ShowModded)
        {
            abilitiesToShow = ModdedInfos;
        }
        if (!string.IsNullOrEmpty(filterText))
        {
            if (FilterByGuid)
            {
                abilitiesToShow = abilitiesToShow.FindAll(x => (AbilityManager.AllAbilities.Find(y=> y.Info == x)?.ModGUID?.ContainsText(filterText, caseSensitive: false)).GetValueOrDefault());
            }
            else
            {
                abilitiesToShow = abilitiesToShow.FindAll((AbilityInfo x) =>
                x.name.ContainsText(filterText, caseSensitive: false)
                || x.rulebookName.ContainsText(filterText, caseSensitive: false));
            }
        }
        abilitiesToShow = abilitiesToShow.FindAll((AbilityInfo x) =>
        (!FilterByAct1 && !FilterByAct2 && !FilterByAct3 && !FilterByGrimora && !FilterByMagnificus)
        || (FilterByAct1 && Part1Ability(x))
        || (FilterByAct2 && Part2Ability(x))
        || (FilterByAct3 && Part3Ability(x))
        || (FilterByGrimora && GrimoraAbility(x))
        || (FilterByMagnificus && MagnificusAbility(x)));
        
        if (Button("Next Page"))
        {
            IncreasePageIndexes(abilitiesToShow.Count);
        }
        if (Button("Previous Page"))
        {
            DecreasePageIndexes(abilitiesToShow.Count);
        }
        StartNewColumn();
        if (selectedInfo != null)
        {
            Label(FullAbilityInfo(selectedInfo, currentGuid), new Vector2(0, 600));
        }
        else if (abilitiesToShow == null || abilitiesToShow.Count == 0)
        {
            Label("No abilities found");
            return;
        }
        StartNewColumn();

        if (toggleChanged)
        {
            if (pageNum > abilitiesToShow.Count / numElements)
            {
                pageNum = abilitiesToShow.Count / numElements;
                endIndex = abilitiesToShow.Count;
            }
            else
            {
                endIndex = pageNum * numElements + numElements;
            }
            startIndex = pageNum * numElements;
        }
        if (endIndex > abilitiesToShow.Count)
        {
            endIndex = abilitiesToShow.Count;
        }
        if (startIndex < 0)
        {
            startIndex = 0;
        }
        int row = 1;
        for (int i = startIndex; i < endIndex; i++)
        {
            AbilityInfo info = AllInfos.Find(x => x == abilitiesToShow[i]);
            string pluginGuid = AbilityManager.AllAbilities.Find(x => x.Info == info)?.ModGUID ?? "";

            if (!ShowAll && ((!ShowModded && pluginGuid != "") || (!ShowVanilla && pluginGuid == "")))
                continue;

            string textToDisplay = $"GUID: {pluginGuid}\nAbility: {info.rulebookName}";

            if (Button(textToDisplay, new(0f, 60f), null))
            {
                selectedInfo = info;
                currentGuid = pluginGuid;
            }
            if (row >= numElements / 3)
            {
                StartNewColumn();
                row = 0;
            }
            row++;
        }
    }

    public void PrintAllAbilitiesToLog()
    {
        foreach (AbilityInfo info in AllInfos)
        {
            Plugin.Log.LogMessage(FullAbilityInfo(info) + "\n");
        }
        Plugin.Log.LogInfo($"Total All: {AllInfos.Count}");
    }

    private string FullAbilityInfo(AbilityInfo info, string guid = null)
    {
        guid ??= AbilityManager.AllAbilities.Find(x => x.Info == info)?.ModGUID ?? "";

        return
            $"GUID: {guid}" +
            $"\nAbility: {info.name}" +
            $"\nRulebook Name: {info.rulebookName}" +
            $"\nDescription: {info.rulebookDescription}" +
            $"\nPowerlevel: {info.powerLevel}" +
            $"\nCanStack: {info.canStack}" +
            $"\nOpponentUsable: {info.opponentUsable}" +
            $"\nActivated: {info.activated}" +
            $"\nPassive: {info.passive}" +
            $"\nConduit: {info.conduit}" +
            $"\nConduitCell: {info.conduitCell}";
    }
    public void PrintVanillaAbilitiesToLog()
    {
        foreach (AbilityInfo info in VanillaInfos)
        {
            Plugin.Log.LogMessage(FullAbilityInfo(info) + "\n");
        }
        Plugin.Log.LogInfo($"Total Vanilla: {VanillaInfos.Count}");
    }

    public void PrintModdedAbilitiesToLog()
    {
        foreach (AbilityInfo info in ModdedInfos)
        {
            Plugin.Log.LogMessage(FullAbilityInfo(info) +"\n");
        }
        Plugin.Log.LogInfo($"Total Modded: {ModdedInfos.Count}");
    }

    public void PrintGUIDsToLog(Dictionary<AbilityInfo, string> dialogueEventsWithVariableString)
    {
        string arg = dialogueEventsWithVariableString.Values.FirstOrDefault();
        foreach (KeyValuePair<AbilityInfo, string> item in dialogueEventsWithVariableString)
        {
            Plugin.Log.LogMessage(FullAbilityInfo(item.Key, arg ?? ""));
        }
        Plugin.Log.LogInfo($"GUID: {arg ?? ("No GUID"),-30}");
        Plugin.Log.LogInfo($"Total Selected: {dialogueEventsWithVariableString.Count}");
    }

    private void IncreasePageIndexes(int maxCount)
    {
        pageNum++;
        if (endIndex == maxCount)
        {
            startIndex = pageNum = 0;
            endIndex = Mathf.Min(numElements, maxCount);
            return;
        }
        startIndex = pageNum * numElements;
        endIndex = pageNum * numElements + numElements;
        if (endIndex > maxCount)
        {
            endIndex = maxCount;
        }
    }

    private void DecreasePageIndexes(int maxCount)
    {
        pageNum--;
        if (startIndex == 0)
        {
            pageNum = maxCount / numElements;
            startIndex = pageNum * numElements;
            endIndex = maxCount;
            return;
        }
        startIndex = pageNum * numElements;
        endIndex = pageNum * numElements + numElements;
        if (startIndex < 0)
        {
            startIndex = 0;
        }
    }

    public bool Part1Ability(AbilityInfo info)
    {
        return info.metaCategories.Exists(x => x == AbilityMetaCategory.Part1Rulebook);
    }

    public bool Part2Ability(AbilityInfo info)
    {
        return info.pixelIcon != null;
    }

    public bool Part3Ability(AbilityInfo info)
    {
        return info.metaCategories.Exists(x => x == AbilityMetaCategory.Part3Rulebook);
    }

    public bool GrimoraAbility(AbilityInfo info)
    {
        return info.metaCategories.Exists(x => x == AbilityMetaCategory.GrimoraRulebook);
    }

    public bool MagnificusAbility(AbilityInfo info)
    {
        return info.metaCategories.Exists(x => x == AbilityMetaCategory.MagnificusRulebook);
    }
}