using DebugMenu.Scripts.Acts;
using BepInEx;
using DebugMenu.Scripts.Popups;
using DiskCardGame;
using InscryptionAPI.Card;
using UnityEngine;

namespace DebugMenu.Scripts.Act1;

public class Act1 : BaseAct
{
    public static List<CardInfo> lastUsedStarterDeck = null;
    public static bool SkipNextNode = false;
    public static bool ActivateAllMapNodesActive = false;
    public static Tribe SelectedTotemTribe = Tribe.None;
    public static Ability SelectedTotemAbility = Ability.None;

    public Act1(DebugWindow window) : base(window)
    {
        m_mapSequence = new Act1MapSequence(this);
        m_cardBattleSequence = new Act1CardBattleSequence(window);
    }

    public override void OnGUI()
    {
        MapNodeManager mapNodeManager = Singleton<MapNodeManager>.m_Instance;
        if (mapNodeManager?.nodes == null || RunState.Run == null)
            return;

        Window.LabelHeader("Act 1");
        if (RunState.Run.currentNodeId > 0)
        {
            MapNode nodeWithId = mapNodeManager.GetNodeWithId(RunState.Run.currentNodeId);
            Window.Label("Current Node ID: " + RunState.Run.currentNodeId + "\nCurrent Node: " + nodeWithId?.name, new(0, 80f));
        }

        DrawCurrencyGUI();
        DrawItemsGUI();
        DrawTotemGUI();
        Window.Padding();

        if (Window.Button("Replenish Candles"))
        {
            Plugin.Instance.StartCoroutine(CandleHolder.Instance.ReplenishFlamesSequence(0f));
            RunState.Run.playerLives = RunState.Run.maxPlayerLives;
            SaveManager.SaveToFile(false);
        }

        Window.StartNewColumn();
        OnGUICurrentNode();
    }

    public override bool OnSpecialCardSequence(string nodeDataName)
    {
        if (nodeDataName == "CardStatBoost")
            return true;

        if (nodeDataName == "ChooseEyeball")
            return true;

        if (nodeDataName == "RemoveCard")
            return true;

        return false;
    }
    private void DrawCurrencyGUI()
    {
        Window.LabelHeader("Currency: " + RunState.Run.currency);
        using (Window.HorizontalScope(4))
        {
            if (Window.Button("+1"))
                RunState.Run.currency++;

            if (Window.Button("-1"))
                RunState.Run.currency = Mathf.Max(0, RunState.Run.currency - 1);

            if (Window.Button("+5"))
                RunState.Run.currency += 5;

            if (Window.Button("-5"))
                RunState.Run.currency = Mathf.Max(0, RunState.Run.currency - 5);
        }
    }
    private void DrawTotemGUI()
    {
        Window.LabelHeader("Totem");
        string currentTotemTopName = Enum.GetName(typeof(Tribe), SelectedTotemTribe); ;
        string currentTotemBottomName = AbilitiesUtil.GetInfo(SelectedTotemAbility) == null ? "None" : AbilitiesUtil.GetInfo(SelectedTotemAbility).rulebookName;
        if (currentTotemTopName.IsNullOrWhiteSpace())
            currentTotemTopName = TribeManager.NewTribes.First((TribeManager.TribeInfo tribe) => { return tribe.tribe == SelectedTotemTribe; }).name;
        using (Window.HorizontalScope(2))
        {
            ButtonListPopup.OnGUI<ButtonListPopup>(Window, "Tribe: " + currentTotemTopName, "Change Totem Tribe", () =>
            {
                List<Tribe> allTribes = [Tribe.None, Tribe.Squirrel, Tribe.Bird, Tribe.Canine, Tribe.Hooved, Tribe.Reptile, Tribe.Insect];
                allTribes.AddRange(TribeManager.NewTribesTypes);

                List<string> names = new(allTribes.Count);
                List<string> values = new(allTribes.Count);

                for (int i = 0; i < allTribes.Count; i++)
                {
                    string name = null;
                    if (TribeManager.IsCustomTribe(allTribes[i]))
                    {
                        TribeManager.TribeInfo iTribe = TribeManager.NewTribes.First((TribeManager.TribeInfo tribe) => { return tribe.tribe == allTribes[i]; });
                        name = iTribe.guid + "_" + iTribe.name;
                    }
                    else
                    {
                        name = Enum.GetName(typeof(Tribe), allTribes[i]);
                    }
                    names.Add(name);
                    values.Add(name);
                }
                return new Tuple<List<string>, List<string>>(names, values);
            }, (int chosenIndex, string chosenValue, List<string> index) =>
            {
                Tribe chosenTribe = Tribe.None;
                Enum.TryParse<Tribe>(chosenValue, false, out chosenTribe);
                if (chosenTribe == Tribe.None && chosenValue != "None")
                {
                    chosenTribe = TribeManager.NewTribes.First((TribeManager.TribeInfo tribe) => { return tribe.guid + "_" + tribe.name == chosenValue; }).tribe;
                }
                SelectedTotemTribe = chosenTribe;
            });

            ButtonListPopup.OnGUI<ButtonListPopup>(Window, "Sigil: " + currentTotemBottomName, "Change Totem Sigil", () =>
            {
                List<AbilityManager.FullAbility> allAbilities = AbilityManager.AllAbilities;

                List<string> names = new(allAbilities.Count);
                List<string> values = new(allAbilities.Count);

                for (int i = 0; i < allAbilities.Count; i++)
                {
                    names.Add(allAbilities[i].Info.rulebookName + "\n(" + allAbilities[i].Info.name + ")");
                    values.Add(allAbilities[i].Info.name);
                }
                return new Tuple<List<string>, List<string>>(names, values);
            }, (int chosenIndex, string chosenValue, List<string> index) =>
            {
                Ability chosenAbility = Ability.None;
                if (chosenAbility == Ability.None && chosenValue != "None")
                {
                    chosenAbility = AbilityManager.AllAbilities.First((AbilityManager.FullAbility ability) => { return ability.Info.name == chosenValue; }).Id;
                }
                SelectedTotemAbility = chosenAbility;
            });
        }
        if (Window.Button("Set Totem"))
        {
            RunState.Run.totems.Clear();
            if (Part1ItemsManager.Instance.Totems.Count > 0)
            {
                Totem existingTotem = Part1ItemsManager.Instance.Totems[0];
                UnityEngine.Object.DestroyImmediate(existingTotem.gameObject);
                existingTotem = null;
            }
            if (SelectedTotemAbility != Ability.None && SelectedTotemTribe != Tribe.None)
            {
                TotemDefinition newTotem = new()
                {
                    tribe = SelectedTotemTribe,
                    ability = SelectedTotemAbility
                };
                RunState.Run.totems.Add(newTotem);
            }
            ItemsManager.Instance.UpdateItems(true);
        }
    }

    public override void Restart()
    {
        if (SaveFile.IsAscension)
            NewAscensionGame();
        else if (SaveManager.SaveFile.IsPart1)
            RestartVanilla();
    }

    public override void Reload()
    {
        if (SaveFile.IsAscension)
        {
            if (AscensionSaveData.Data.currentRun != null)
                ReloadKaycees();
            else
                NewAscensionGame();
        }
        else
        {
            Log("Reloading Vanilla...");
            base.Reload();
        }
    }

    private void NewAscensionGame()
    {
        FrameLoopManager.Instance.SetIterationDisabled(disabled: false);
        SceneLoader.Load("Ascension_Configure");
        if (lastUsedStarterDeck != null)
        {
            Log("New Ascension run with " + lastUsedStarterDeck.Count + " cards!");
            AscensionSaveData.Data.NewRun(lastUsedStarterDeck);
            SaveManager.SaveToFile(saveActiveScene: false);
            MenuController.LoadGameFromMenu(newGameGBC: false);
            Singleton<InteractionCursor>.Instance.SetHidden(hidden: true);
        }
    }

    private void RestartVanilla()
    {
        Log("Restarting Vanilla...");
        FrameLoopManager.Instance.SetIterationDisabled(disabled: false);
        SaveManager.SaveFile.ResetPart1Run();
        SaveManager.SaveToFile(saveActiveScene: false);
        SceneLoader.Load("Part1_Cabin");
    }
}