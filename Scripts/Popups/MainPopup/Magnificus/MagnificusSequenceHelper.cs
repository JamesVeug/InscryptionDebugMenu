using DebugMenu.Scripts.Popups;
using DebugMenu.Scripts.Utils;
using DiskCardGame;
using GrimoraMod;
using HarmonyLib;
using InscryptionAPI.Card;
using InscryptionAPI.Guid;
using InscryptionAPI.Helpers;
using MagnificusMod;
using Pixelplacement;
using System.Collections;
using System.Reflection;
using System.Reflection.Emit;
using System.Xml.Linq;
using UnityEngine;

namespace DebugMenu.Scripts.Magnificus;

public static partial class MagnificusModHelper
{
    internal static Type SpellChoiceType { get; private set; } = null;
    internal static Type EnchantType { get; private set; } = null;
    internal static CardChoicesSequencer SpellChoiceInstance { get; private set; }
    internal static CardChoicesSequencer EnchantInstance { get; private set; }
    internal static FieldInfo CardsFromDeck { get; private set; }
    internal static FieldInfo EnchantTier { get; private set; }
    internal static MethodInfo PickUpScript { get; private set; }
    internal static MethodInfo EnchantAbilities { get; private set; }

    internal static CardMetaCategory SpellPool = GuidManager.GetEnumValue<CardMetaCategory>(Guid, "MagSpellPool");

    internal static List<CardInfo> SpellPoolCards = null;

    internal static bool rerolling = false;

    internal static int rareRegion = -1;
    internal static List<string> clearedCache;

    public static void OnEnterLogic(bool doTransition)
    {
        try
        {
            GameObject.Find("Player").GetComponentInChildren<ViewManager>().CurrentView = View.FirstPerson;
            float x2 = GameObject.Find("PixelCameraParent").transform.position.x;
            float z2 = GameObject.Find("PixelCameraParent").transform.position.z;
            GameObject.Find("PixelCameraParent").transform.position = new Vector3(x2, 16.5f, z2);
            Tween.Position(GameObject.Find("PixelCameraParent").transform, new Vector3(x2, 16.5f, z2), 0f, 0f);
            GameObject.Find("Player").GetComponentInChildren<FirstPersonController>().enabled = true;
            GameObject.Find("Player").GetComponentInChildren<ViewController>().allowedViews = new List<View>();
            if (Generation.minimap && RunState.Run.regionTier > 0)
            {
                string name = ((Component)(object)Singleton<FirstPersonController>.Instance.currentZone).gameObject.name;
                if (name == "mirror")
                {
                    name = GameObject.Find("NavigationGrid").GetComponentInChildren<OcclusionArea>().gameObject.name;
                }
                string[] array2 = name.Split(new char[1] { 'x' });
                string[] array3 = array2[1].Split(new char[1] { ' ' });
                int num4 = int.Parse(array3[0]);
                string[] array4 = array3[1].Split(new char[1] { 'y' });
                int num5 = int.Parse(array4[1]);
                num4 /= 7;
                num5 /= 7;
                float num6 = 0.15f * (float)num5;
                float num7 = 0.15f * (float)num4;
                GameObject gameObject = GameObject.Find("playerMapNode");
                Tween.LocalPosition(gameObject.transform.Find("Header").Find("IconSprite"), new Vector3(-0.15f + num7, 0.15f - num6, 0f), 0.25f, 0f);
            }
        }
        catch
        {
            Plugin.Log.LogError("[MagnificusModSetUp] Error dealing with player stuff, bypassing logic.");
        }

        if (config.isometricMode && doTransition)
        {
            DoTransition();
        }
    }

    public static void HandleCardChoices(CardChoicesType type)
    {
        switch (type)
        {
            case CardChoicesType.Cost:
                Plugin.Log.LogInfo("Cost");
                OnEnterLogic(true);
                Generation.costSelect.eventToTrigger();
                break;
            case CardChoicesType.Random:
                Plugin.Log.LogInfo("Card Choice");
                OnEnterLogic(true);
                Generation.cardSelect.eventToTrigger();
                break;
        }
    }

    public static void HandleSequences(GameState state, NodeData data)
    {
        Type nodeType = data.GetType();
        string nodeName = nodeType.Name;
        Plugin.Log.LogInfo(nodeName);
        switch (nodeName)
        {
            case "Cauldron":
                Plugin.Log.LogInfo("Cauldron");
                OnEnterLogic(true);
                Generation.cauldronEvent.eventToTrigger();
                break;
            case "ChooseRareCardNodeData":
                Plugin.Instance.StartCoroutine(RareCardChoice(data));
                break;
            case "CopyNode":
                Plugin.Log.LogInfo("Copy");
                OnEnterLogic(true);
                Generation.copyCard.eventToTrigger();
                break;
            case "CustomNode1":
                Plugin.Log.LogInfo("Cost change");
                OnEnterLogic(true);
                Generation.costChange.eventToTrigger();
                break;
            case "CustomNode2":
                Plugin.Log.LogInfo("Shop");
                OnEnterLogic(true);
                Generation.shop.eventToTrigger();
                break;
            case "CustomNode3":
                Plugin.Log.LogInfo("Bleach");
                OnEnterLogic(true);
                Generation.bleach.eventToTrigger();
                break;
            case "CustomNode14":
                Plugin.Log.LogInfo("Enchant");
                OnEnterLogic(true);
                Generation.enchant.eventToTrigger();
                break;
            case "DeathCardEvent": // calls when player dies
                OnEnterLogic(false);
                Generation.deathCard.eventToTrigger();
                break;
            case "EdaxioNode":
                Plugin.Log.LogInfo("Edaxio");
                OnEnterLogic(true);
                Generation.edaxioNode.eventToTrigger();
                break;
            case "MergeNode":
                Plugin.Log.LogInfo("Merge");
                OnEnterLogic(true);
                Generation.mergeCard.eventToTrigger();
                break;
            case "PaintingEvent":
                Plugin.Log.LogInfo("painting");
                OnEnterLogic(true);
                Generation.cardPainting.eventToTrigger();
                break;
            case "SpellCardChoice":
                OnEnterLogic(true);
                Plugin.Log.LogInfo("SpellChoice");
                Generation.spellSelect.eventToTrigger();
                break;
            case "TradePeltsNodeData":
                Plugin.Log.LogInfo("Draft");
                OnEnterLogic(true);
                Generation.drafting.eventToTrigger();
                break;
            case "UpgradeSpellNode":
                Plugin.Log.LogInfo("upgrade spell");
                OnEnterLogic(true);
                Generation.spellUpgrade.eventToTrigger();
                break;
        }
    }

    public static IEnumerator RareCardChoice(NodeData data)
    {
        OnEnterLogic(true);
        Generation.setupNodeStuff(Singleton<MagnificusGameFlowManager>.Instance);
        Helpers.LastSpecialNodeData = new ChooseRareCardNodeData();
        GameFlowManager.Instance.CurrentGameState = GameState.SpecialCardSequence;

        RareCardChoicesSequencer sequencer = SpecialNodeHandler.Instance.rareCardChoiceSequencer;
        sequencer.box.gameObject.SetActive(value: false);
        Singleton<ViewManager>.Instance.SwitchToView(View.Default);
        Singleton<ViewManager>.Instance.Controller.LockState = ViewLockState.Locked;
        yield return new WaitForSeconds(0.3f);
        AudioController.Instance.StopAllLoops();
        AudioController.Instance.SetLoopAndPlay("School_of_Magicks");
        AudioController.Instance.SetLoopVolumeImmediate(0.55f);
        yield return new WaitForSeconds(1f);
        if (RunState.Run.playerLives > -1)
        {
            if (!SaveVariables.LearnedMechanics.Contains("beatbosswithdamage;") && RunState.Run.playerLives != RunState.Run.maxPlayerLives)
            {
                SaveVariables.LearnedMechanics += "beatbosswithdamage;";
                yield return Singleton<TextDisplayer>.Instance.ShowUntilInput("Hmm?", 0f, 0f, Emotion.None);
                yield return Singleton<TextDisplayer>.Instance.ShowUntilInput("Did you expect me to restore your [c:g1]life painting[c:]?", 0.5f, 0f, Emotion.None);
                yield return Singleton<TextDisplayer>.Instance.ShowUntilInput("Hmm.. That's not how it works here, challenger..", 0f, 0f, Emotion.None);
                yield return Singleton<TextDisplayer>.Instance.ShowUntilInput("Bleach is not easily restorable.", 0f, 0f, Emotion.None);
                yield return Singleton<TextDisplayer>.Instance.ShowUntilInput("You may only pick a [c:g3]rare card[c:].", 0.5f, 0f, Emotion.None);
            }
            else
            {
                yield return Singleton<TextDisplayer>.Instance.ShowThenClear("Choose wisely.", 2f, 0f, Emotion.None);
            }
            yield return new WaitForSeconds(0.5f);
            sequencer.selectableCards = sequencer.SpawnCards(3, GameObject.Find("GameTable").transform, new Vector3(-1.55f, 5.01f, 0.5f), 1.5f);
            SaveManager.saveFile.GetCurrentRandomSeed();
            List<CardChoice> list2 = new List<CardChoice>();
            for (int j = 0; j < 3; j++)
            {
                List<CardInfo> unlockedCards = CardLoader.GetUnlockedCards(CardMetaCategory.Rare, CardTemple.Wizard);
                unlockedCards.RemoveAll((CardInfo x) => ((UnityEngine.Object)(object)x).name == "MoxTriple" && x.HasTrait(Trait.Gem));
                CardInfo card = CardLoader.Clone(unlockedCards[SeededRandom.Range(0, unlockedCards.Count, SaveManager.saveFile.randomSeed)]);
                if (unlockedCards.Count >= 3)
                {
                    SaveManager.saveFile.randomSeed++;
                    while (list2.Exists((CardChoice x) => ((UnityEngine.Object)(object)x.CardInfo).name == ((UnityEngine.Object)(object)card).name))
                    {
                        card = CardLoader.Clone(unlockedCards[SeededRandom.Range(0, unlockedCards.Count, SaveManager.saveFile.randomSeed)]);
                        SaveManager.saveFile.randomSeed++;
                    }
                }
                list2.Add(new CardChoice
                {
                    CardInfo = card
                });
            }
            for (int k = 0; k < sequencer.selectableCards.Count; k++)
            {
                sequencer.selectableCards[k].gameObject.SetActive(value: true);
                sequencer.selectableCards[k].ChoiceInfo = list2[k];
                sequencer.selectableCards[k].Initialize(list2[k].CardInfo, (Action<SelectableCard>)sequencer.OnRewardChosen, (Action<SelectableCard>)sequencer.OnCardFlipped, startFlipped: true, (Action<SelectableCard>)sequencer.OnCardInspected);
                sequencer.selectableCards[k].SetEnabled(enabled: false);
                sequencer.selectableCards[k].SetFaceDown(faceDown: true, immediate: true);
            }
            Singleton<ViewManager>.Instance.SwitchToView(View.OpponentQueueCentered);
            Singleton<InteractionCursor>.Instance.InteractionDisabled = false;
            sequencer.SetCollidersEnabled(collidersEnabled: true);
            sequencer.gamepadGrid.enabled = true;
            sequencer.chosenReward = null;
        }
        else
        {
            if (!SaveVariables.LearnedMechanics.Contains("beatbosswithdamage;") && !SaveManager.saveFile.ascensionActive)
            {
                SaveVariables.LearnedMechanics += "beatbosswithdamage;";
                yield return Singleton<TextDisplayer>.Instance.ShowUntilInput("Hmm?", 0.5f, 0f, Emotion.None);
                yield return Singleton<TextDisplayer>.Instance.ShowUntilInput("Did you expect me to restore your [c:g1]life painting[c:]?", 1.5f, 0f, Emotion.None);
                yield return Singleton<TextDisplayer>.Instance.ShowUntilInput("Hmm.. That's not how it works here, challenger..", 0.5f, 0f, Emotion.None);
                yield return Singleton<TextDisplayer>.Instance.ShowUntilInput("Bleach is not easily restorable.", 1f, 0f, Emotion.None);
                yield return Singleton<TextDisplayer>.Instance.ShowUntilInput("You may only pick a [c:g3]rare card[c:].", 2f, 0f, Emotion.None);
            }
            else
            {
                yield return Singleton<TextDisplayer>.Instance.ShowThenClear("Choose wisely.", 2f, 0f, Emotion.None);
            }
            yield return new WaitForSeconds(0.5f);
            Singleton<ViewManager>.Instance.SwitchToView(View.OpponentQueueCentered);
            GameObject lifePainting = GameObject.Find("GameTable").transform.Find("LifePainting").gameObject;
            lifePainting.SetActive(value: true);
            lifePainting.transform.localPosition = new Vector3(1.45f, 7.54f, -0.4f);
            Tween.LocalRotation(lifePainting.transform, Quaternion.Euler(71f, 0f, 0f), 0.5f, 0f);
            yield return new WaitForSeconds(0.6f);
            CardInfo cardLol = CardLoader.GetCardByName("JuniorSage");
            for (int l = 0; l < 2; l++)
            {
                yield return new WaitForSeconds(Time.deltaTime);
                yield return new WaitForSeconds(Time.deltaTime);
                GameObject gameObject = UnityEngine.Object.Instantiate(Singleton<SelectableCardArray>.Instance.selectableCardPrefab);
                gameObject.transform.SetParent(GameObject.Find("GameTable").transform);
                SelectableCard component = gameObject.GetComponent<SelectableCard>();
                component.Initialize(cardLol, (Action<SelectableCard>)Singleton<SelectableCardArray>.Instance.OnCardSelected, (Action<SelectableCard>)null, startFlipped: false, (Action<SelectableCard>)Singleton<SelectableCardArray>.Instance.OnCardInspected);
                component.SetEnabled(enabled: false);
                component.SetCardback(TextureHelper.GetImageAsTexture("magcardback.png", typeof(Generation).Assembly));
                component.Anim.SetFaceDown(faceDown: true, immediate: true);
                Singleton<SelectableCardArray>.Instance.displayedCards.Add(component);
                float x2 = -1.5f;
                Tween.LocalPosition(lifePainting.transform, new Vector3(1.45f, 5.54f, -0.4f), 0.25f, 0f);
                if (l == 1)
                {
                    x2 = 1.5f;
                }
                Singleton<SelectableCardArray>.Instance.TweenInCard(component.transform, new Vector3(x2, 5.3f, 1f), 0f, tiltCard: false);
                component.Anim.PlayQuickRiffleSound();
                component.Initialize(cardLol, Generation.chooseBetweenLifeandRare);
                component.SetCardback(TextureHelper.GetImageAsTexture("magcardback.png", typeof(Generation).Assembly));
                component.Anim.SetFaceDown(faceDown: true, immediate: true);
                component.GetComponent<Collider>().enabled = true;
                if (l == 1)
                {
                    component.name = "lifePaintingJr";
                }
                else
                {
                    component.name = "rareChoice";
                }
            }
        }
        yield return new WaitUntil(() => sequencer.chosenReward != null);
        sequencer.chosenReward.transform.parent = null;
        Singleton<RuleBookController>.Instance.SetShown(shown: false);
        sequencer.gamepadGrid.enabled = false;
        if (((UnityEngine.Object)(object)sequencer.chosenReward.Info).name != "JuniorSage")
        {
            sequencer.deckPile.MoveCardToPile(sequencer.chosenReward, flipFaceDown: true, 0.25f, 1f);
            sequencer.AddChosenCardToDeck();
        }
        else
        {
            UnityEngine.Object.Destroy(sequencer.chosenReward.gameObject);
            Tween.LocalPosition(GameObject.Find("LifePainting").transform, new Vector3(1f, 27.5f, 1f), 0.5f, 0f);
            yield return Generation.WaitThenDisable(GameObject.Find("LifePainting"), 0.6f);
        }
        
        sequencer.CleanupMushrooms();
        yield return new WaitForSeconds(0.5f);
        Singleton<ViewManager>.Instance.SwitchToView(View.Default);
        yield return new WaitForSeconds(0.5f);
        sequencer.CleanUpCards(doTween: false);
        yield return new WaitForSeconds(0.3f);
        yield return sequencer.StartCoroutine(sequencer.deckPile.DestroyCards());
        sequencer.deckPile.gameObject.SetActive(value: false);
        ProgressionData.SetMechanicLearned(MechanicsConcept.RareCards);
        Tween.Position(GameObject.Find("GameTable").transform, new Vector3(GameObject.Find("GameTable").transform.position.x, -5.5f, GameObject.Find("GameTable").transform.position.z), 0.5f, 0f);
        Singleton<ViewManager>.Instance.SwitchToView(View.FirstPerson);
        yield return Generation.unIsometricTransition();
    }

    public static void TransitionToBattle(string sequenceId)
    {
        OnEnterLogic(true);
        switch (sequenceId)
        {
            case "BleachBattleSequencer":
                Generation.bleachBattle.eventToTrigger();
                break;
            case "BleeneSequencer":
                Generation.stimBattle.eventToTrigger();
                break;
            case "EspeararaSequencer":
                Generation.pikeBattle.eventToTrigger();
                break;
            case "GoobertSequencer":
                Generation.gooBattle.eventToTrigger();
                break;
            case "GoranjSequencer":
                Generation.gooBattle.eventToTrigger();
                break;
            case "OrluSequencer":
                Generation.pikeBattle.eventToTrigger();
                break;
            case "LonelyMageSequencer":
                Generation.stimBattle.eventToTrigger();
                break;
            default:
                Generation.cardBattle.eventToTrigger();
                break;
        }
    }
    public static void DoTransition()
    {
        if (Singleton<UIManager>.Instance.rightHintShown)
        {
            Singleton<UIManager>.Instance.SetControlsHintShown(shown: false);
        }

        MagnificusGameFlowManager.Instance.StartCoroutine(Generation.isometricTransition(Texture2D.blackTexture));
    }

    public static IEnumerator RerollSpellChoices()
    {
        Plugin.Log.LogDebug("Reroll Spell choices");
        rerolling = true;
        SpellPoolCards ??= AllSpellCards.FindAll(x => x.HasCardMetaCategory(SpellPool));
        if (SpellChoiceInstance == null)
        {
            Plugin.Log.LogDebug("Retrieve spell choice sequencer info");
            SpellChoiceType = AccessTools.TypeByName("spellchoice");
            SpellChoiceInstance = SpecialNodeHandler.Instance.GetComponent(SpellChoiceType) as CardChoicesSequencer;
            CardsFromDeck = SpellChoiceType.GetField("cardpickedfromdeck", BindingFlags.Public | BindingFlags.Instance);
            PickUpScript = SpellChoiceType.GetMethod("cardpickingupscropt", BindingFlags.Public | BindingFlags.Instance);
        }
        
        foreach (SelectableCard card in (List<SelectableCard>)CardsFromDeck.GetValue(SpellChoiceInstance))
        {
            card.ExitBoard(0f, Vector3.forward);
        }

        List<CardInfo> spellCopy = new(SpellPoolCards);
        List<CardInfo> selectedCards = new();
        List<SelectableCard> choices = new();

        for (int j = 0; j < 3; j++)
        {
            int selected = UnityEngine.Random.RandomRangeInt(0, spellCopy.Count);
            if (spellCopy.Count == 0)
            {
                spellCopy.Add(CardLoader.GetCardByName("mag_frostspell"));
                selected = 0;
            }
            selectedCards.Add(spellCopy[selected]);
            spellCopy.Remove(spellCopy[selected]);
        }

        for (int i = 0; i < 3; i++)
        {
            float a2 = 1.5f * i;
            GameObject gameObject = UnityEngine.Object.Instantiate(Singleton<SelectableCardArray>.Instance.selectableCardPrefab);
            gameObject.transform.SetParent(SpellChoiceInstance.transform);
            SelectableCard component = gameObject.GetComponent<SelectableCard>();
            component.Initialize(selectedCards[i], Singleton<SelectableCardArray>.Instance.OnCardSelected, null, startFlipped: false, Singleton<SelectableCardArray>.Instance.OnCardInspected);
            component.SetEnabled(enabled: false);
            component.Anim.PlayQuickRiffleSound();
            component.Initialize(selectedCards[i], (SelectableCard card) => PickUpScript.Invoke(SpellChoiceInstance, new object[1] { card }));
            component.GetComponent<Collider>().enabled = true;
            Singleton<SelectableCardArray>.Instance.displayedCards.Add(component);
            component.SetFaceDown(faceDown: true);
            component.FaceDown = true;
            component.SetCardback(TextureHelper.GetImageAsTexture("magcardback.png", typeof(MagnificusMod.Achievements).Assembly));
            yield return new WaitForSeconds(0.1f);
            choices.Add(component);
            Singleton<SelectableCardArray>.Instance.TweenInCard(component.transform, new Vector3(-1.5f + a2, 5.03f, -1.5f), 0f, tiltCard: true);
            component.CursorEntered = (Action<MainInputInteractable>)Delegate.Combine(component.CursorEntered, (Action<MainInputInteractable>)delegate
            {
                if (component.FaceDown)
                {
                    Tween.LocalRotation(component.transform, new Vector3(90f, 3f, 0f), 0.1f, 0f);
                }
                else
                {
                    Tween.LocalPosition(component.transform.Find("Quad").Find("CardBase"), new Vector3(0f, 0f, -0.1f), 0.1f, 0f);
                }
            });
            component.CursorExited = (Action<MainInputInteractable>)Delegate.Combine(component.CursorExited, (Action<MainInputInteractable>)delegate
            {
                Tween.LocalRotation(component.transform, new Vector3(90f, 0f, 0f), 0.1f, 0f);
                Tween.LocalPosition(component.transform.Find("Quad").Find("CardBase"), Vector3.zero, 0.1f, 0f);
            });
        }

        CardsFromDeck.SetValue(SpellChoiceInstance, choices);
        rerolling = false;
    }

    public static void HandleEnchantNode(BaseWindow window)
    {
        if (EnchantInstance == null)
        {
            Plugin.Log.LogDebug("Retrieve spell choice sequencer info");
            EnchantType = AccessTools.TypeByName("enchant");
            EnchantInstance = SpecialNodeHandler.Instance.GetComponent(EnchantType) as CardChoicesSequencer;
            EnchantTier = EnchantType.GetField("sacValue", BindingFlags.Public | BindingFlags.Instance);
            EnchantAbilities = EnchantType.GetMethod("generateAbilities", BindingFlags.Public | BindingFlags.Instance);
        }

        int currentVal = (int)EnchantTier.GetValue(EnchantInstance);
        window.LabelBold("Enchantment Tier: " + (int)EnchantTier.GetValue(EnchantInstance));
        window.LabelBold("Displeased: " + (SaveManager.SaveFile.ascensionActive && Generation.challenges.Contains("DyingBreath")));
        using (window.HorizontalScope(2))
        {
            if (window.Button("+1", disabled: () => new(() => currentVal > 4)))
            {
                currentVal++;
                EnchantTier.SetValue(EnchantInstance, currentVal);
                EnchantAbilities.Invoke(EnchantInstance, new object[1] { currentVal });
            }
            if (window.Button("-1", disabled: () => new(() => currentVal < 2)))
            {
                currentVal--;
                EnchantTier.SetValue(EnchantInstance, currentVal);
                EnchantAbilities.Invoke(EnchantInstance, new object[1] { currentVal });
            }
        }
        using (window.HorizontalScope(2))
        {
            if (window.Button("RESET"))
            {
                EnchantTier.SetValue(EnchantInstance, 1);
                EnchantAbilities.Invoke(EnchantInstance, new object[1] { 1 });
            }
            if (window.Button("MAX"))
            {
                EnchantTier.SetValue(EnchantInstance, 5);
                EnchantAbilities.Invoke(EnchantInstance, new object[1] { 5 });
            }
        }
    }
}
