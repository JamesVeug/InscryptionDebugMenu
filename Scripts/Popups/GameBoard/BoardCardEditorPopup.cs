using DebugMenu.Scripts.Acts;
using DebugMenu.Scripts.Utils;
using DiskCardGame;
using GBC;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers.Extensions;
using UnityEngine;

namespace DebugMenu.Scripts.Popups.DeckEditorPopup;

public class BoardCardEditorPopup : BaseWindow
{
	public override string PopupName => "Card Editor";
	public override Vector2 Size => new(512f, 768f);

	public PlayableCard currentSelection = null;

	private Vector2 editDeckScrollVector = Vector2.zero;

	public override void OnGUI()
	{
		base.OnGUI();

		if (currentSelection == null)
		{
			GUILayout.Label("No card selected.");
			return;
		}
		
		GUILayout.BeginArea(new Rect(5f, Size.y / 4f, Size.x - 10f, Size.y / 2f));
        if (BoardOnGUI(currentSelection))
            currentSelection.RenderCard();
        GUILayout.EndArea();
		
	}

    public static bool BoardOnGUI(PlayableCard val)
    {
        if (val == null)
            return false;

        GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
        GUILayout.Label("Card Editor (" + val.name + ")", Array.Empty<GUILayoutOption>());

        if (val.Info.HasModFromCardMerge() && GUILayout.Button("Clear Merge Data", Array.Empty<GUILayoutOption>()))
        {
            foreach (CardModificationInfo mod in val.Info.Mods)
            {
                mod.fromCardMerge = false;
            }

            return true;
        }

        GUILayout.EndHorizontal();

        if (SaveManager.SaveFile.IsPart2)
        GUILayout.Button("No emissions in Act 2");
        else
        {
            CardModificationInfo emissionMod = val.Info.Mods.Find(a => a.singletonId == DrawCardInfo.EmissionMod);
            if (emissionMod != null)
            {
                if (GUILayout.Button("Unforce Emission", Array.Empty<GUILayoutOption>()))
                {
                    val.Info.Mods.Remove(emissionMod);
                    return true;
                }
            }
            else
            {
                if (GUILayout.Button("Force Emission", Array.Empty<GUILayoutOption>()))
                {
                    val.Info.Mods.Add(new() { singletonId = DrawCardInfo.EmissionMod });
                    return true;
                }
            }
        }

        CardModificationInfo portraitMod = val.Info.Mods.Find(a => a.singletonId == DrawCardInfo.PortraitMod);
        if (portraitMod != null)
        {
            if (GUILayout.Button("Unforce Alt Portrait", Array.Empty<GUILayoutOption>()))
            {
                val.Info.Mods.Remove(portraitMod);
                return true;
            }
        }
        else if (SaveManager.SaveFile.IsPart2 ? val.Info.HasPixelAlternatePortrait() : val.HasAlternatePortrait())
        {
            if (GUILayout.Button("Force Alt Portrait", Array.Empty<GUILayoutOption>()))
            {
                val.Info.Mods.Add(new() { singletonId = DrawCardInfo.PortraitMod });
                return true;
            }
        }
        else
        {
            GUILayout.Button("No Alt Portrait");
        }

        HandleCost(val);

        HandleStats(val);
        selectedTab = GUILayout.Toolbar(selectedTab, sigilsAbilities, Array.Empty<GUILayoutOption>());
        if (val != null)
        {
            if (selectedTab == 0)
                ManageAbilities(val);
            else
                ManageSpecialAbilities(val);
        }

        return false;
    }

    public static void ManageAbilities(PlayableCard currentCard)
    {
        abilityManagerIndex = GUILayout.Toolbar(abilityManagerIndex, abilityManagementTabs, Array.Empty<GUILayoutOption>());
        if (abilityManagerIndex == 0)
        {
            EditAbilities(currentCard);
        }
        else
        {
            AddAbilities(currentCard);
        }
    }

    private static void EditAbilities(PlayableCard currentCard)
    {
        GUILayout.Label("Sigils", Array.Empty<GUILayoutOption>());

        if (currentCard == null)
        {
            GUILayout.Label("No Card", Array.Empty<GUILayoutOption>());
            return;
        }

        List<AbilityManager.FullAbility> abilities = AbilityManager.AllAbilities.FindAll((a) => currentCard.AllAbilities().Contains(a.Id));
        var cardAbilityList = GetAbilitiesThatContain(abilities);
        if (cardAbilityList.Count <= 0)
        {
            GUILayout.Label("No Sigils", Array.Empty<GUILayoutOption>());
            return;
        }

        bool flag = false;
        NewPager(ref currentPageEdit, (cardAbilityList.Count - 1) / 8);
        int num = 0;
        int num2 = currentPageEdit * 8;
        while (num2 < currentPageEdit * 8 + 8 && num2 < cardAbilityList.Count)
        {
            AbilityManager.FullAbility negateAbility = cardAbilityList[num2];
            if (num % 2 == 0)
            {
                GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
                flag = true;
            }
            Texture val = AbilitiesUtil.LoadAbilityIcon(negateAbility.Id.ToString(), false, false);
            if (val == null)
            {
                if (GUILayout.Button(negateAbility.Info.rulebookName, new GUILayoutOption[2]
                {
                    GUILayout.Width((float)overrideSquareWidth),
                    GUILayout.Height((float)overrideSquareHeight)
                }))
                {
                    NewCardMod(currentCard, 0, 0, 0, negateAbility.Id);
                }
            }
            else if (GUILayout.Button(val, new GUILayoutOption[2]
                     {
                         GUILayout.Width((float)overrideSquareWidth),
                         GUILayout.Height((float)overrideSquareHeight)
                     }))
            {
                NewCardMod(currentCard, 0, 0, 0, negateAbility.Id);
            }
            if (num % 2 != 0)
            {
                GUILayout.EndHorizontal();
                flag = false;
            }
            num2++;
            num++;
        }
        if (flag)
        {
            GUILayout.EndHorizontal();
        }
    }

    private static List<SpecialTriggeredAbilityManager.FullSpecialTriggeredAbility> GetSpecialAbilitiesThatContain(List<SpecialTriggeredAbilityManager.FullSpecialTriggeredAbility> searchingList)
    {
        if (!Filter(ref specialAbilitySearch, ref specialAbilityGUIDSearch))
        {
            return searchingList;
        }

        var results = new List<SpecialTriggeredAbilityManager.FullSpecialTriggeredAbility>();
        foreach (SpecialTriggeredAbilityManager.FullSpecialTriggeredAbility searching in searchingList)
        {
            SpecialTriggeredAbilityManager.FullSpecialTriggeredAbility current = searching;
            if (specialAbilityGUIDSearch != "")
            {
                if (current.ModGUID == null || !current.ModGUID.Contains(specialAbilityGUIDSearch))
                {
                    continue;
                }
            }

            if (specialAbilitySearch != "")
            {
                string rulebookName = current.AbilityName;
                string abilityName = current.Id.ToString();
                if ((rulebookName == null || !rulebookName.ToLower().Contains(specialAbilitySearch.ToLower())) &&
                    !abilityName.Contains(specialAbilitySearch.ToLower()))
                {
                    continue;
                }
            }

            results.Add(current);
        }

        return results;
    }

    private static bool Filter(ref string nameString, ref string guidString)
    {
        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label("Filter Name", GUILayout.Width(80));
            nameString = GUILayout.TextField(nameString, Array.Empty<GUILayoutOption>());

            GUILayout.Label("Filter GUID", GUILayout.Width(80));
            guidString = GUILayout.TextField(guidString, Array.Empty<GUILayoutOption>());
        }

        if (nameString == "" && guidString == "")
        {
            return false;
        }

        return true;
    }

    private static List<AbilityManager.FullAbility> GetAbilitiesThatContain(List<AbilityManager.FullAbility> searchingList)
    {
        if (!Filter(ref abilitySearch, ref abilityGUIDSearch))
        {
            return searchingList;
        }

        var results = new List<AbilityManager.FullAbility>();
        foreach (AbilityManager.FullAbility searching in searchingList)
        {
            AbilityManager.FullAbility current = searching;
            if (abilityGUIDSearch != "")
            {
                if (current.ModGUID == null || !current.ModGUID.Contains(abilityGUIDSearch))
                {
                    continue;
                }
            }

            if (abilitySearch != "")
            {
                string rulebookName = current.Info?.rulebookName;
                string abilityName = current.Id.ToString();
                if ((rulebookName == null || !rulebookName.ToLower().Contains(abilitySearch.ToLower())) &&
                    !abilityName.Contains(abilitySearch.ToLower()))
                {
                    continue;
                }
            }

            results.Add(current);
        }

        return results;
    }

    private static void NewPager(ref int page, int max, int min = 0)
    {
        int num = page;
        GUILayout.BeginVertical(Array.Empty<GUILayoutOption>());
        GUILayout.Label("Page: " + num, Array.Empty<GUILayoutOption>());
        GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
        if (GUILayout.Button("<", Array.Empty<GUILayoutOption>()) && num > min)
        {
            num--;
        }
        if (GUILayout.Button(">", Array.Empty<GUILayoutOption>()) && num < max)
        {
            num++;
        }
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
        page = num;
    }

    private static void AddAbilities(PlayableCard currentCard)
    {
        GUILayout.Label("Sigils", Array.Empty<GUILayoutOption>());

        if (currentCard == null)
        {
            GUILayout.Label("No Card", Array.Empty<GUILayoutOption>());
            return;
        }

        var cardAbilityList = GetAbilitiesThatContain(AbilityManager.AllAbilities);

        bool flag = false;
        NewPager(ref currentPageAdd, (cardAbilityList.Count - 1) / 8);
        int num = 0;
        int num2 = currentPageAdd * 8;
        while (num2 < currentPageAdd * 8 + 8 && num2 < cardAbilityList.Count)
        {
            AbilityManager.FullAbility ability = cardAbilityList[num2];
            if (num % 2 == 0)
            {
                GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
                flag = true;
            }
            Texture val = AbilitiesUtil.LoadAbilityIcon(ability.Id.ToString());
            if (val == null)
            {
                if (GUILayout.Button(ability.Info.rulebookName, GUILayout.Width(overrideSquareWidth), GUILayout.Height(overrideSquareHeight)))
                {
                    NewCardMod(currentCard, 0, 0, ability.Id);
                }
            }
            else if (GUILayout.Button(val, GUILayout.Width(overrideSquareWidth), GUILayout.Height(overrideSquareHeight)))
            {
                NewCardMod(currentCard, 0, 0, ability.Id);
            }
            if (num % 2 != 0)
            {
                GUILayout.EndHorizontal();
                flag = false;
            }
            num2++;
            num++;
        }
        if (flag)
        {
            GUILayout.EndHorizontal();
        }
    }

    private static readonly float overrideSquareWidth = 150;
    private static readonly float overrideSquareHeight = 40;

    private static void ManageSpecialAbilities(PlayableCard currentCard)
    {
        specialAbilitySelector = GUILayout.Toolbar(specialAbilitySelector, specialAbilitySelectorList);
        if (specialAbilitySelector == 0)
        {
            RemoveSpecialAbility(currentCard);
        }
        else
        {
            AddSpecialAbility(currentCard);
        }
    }

    private static void AddSpecialAbility(PlayableCard currentCard)
    {
        GUILayout.Label("Special Abilities", Array.Empty<GUILayoutOption>());

        var abilities = SpecialTriggeredAbilityManager.AllSpecialTriggers;
        var result = GetSpecialAbilitiesThatContain(abilities);

        specialAbilityListVector = GUILayout.BeginScrollView(specialAbilityListVector);
        foreach (SpecialTriggeredAbilityManager.FullSpecialTriggeredAbility allSpecialAbility in result)
        {
            if (GUILayout.Button(allSpecialAbility.AbilityName, Array.Empty<GUILayoutOption>()))
            {
                NewCardMod(currentCard, specialAbility: allSpecialAbility.Id);
            }
        }
        GUILayout.EndScrollView();
    }

    private static void RemoveSpecialAbility(PlayableCard currentCard)
    {
        GUILayout.Label("Special Abilities", Array.Empty<GUILayoutOption>());

        var abilities = SpecialTriggeredAbilityManager.AllSpecialTriggers.FindAll((a) => currentCard.AllSpecialAbilities().Contains(a.Id));
        var result = GetSpecialAbilitiesThatContain(abilities);

        if (result.Count <= 0)
        {
            GUILayout.Label("No Special Abilities", Array.Empty<GUILayoutOption>());
            return;
        }

        specialAbilityListVector2 = GUILayout.BeginScrollView(specialAbilityListVector2);
        foreach (SpecialTriggeredAbilityManager.FullSpecialTriggeredAbility specialAbility2 in result)
        {
            SpecialTriggeredAbilityManager.FullSpecialTriggeredAbility current = specialAbility2;
            if (GUILayout.Button(current.AbilityName, Array.Empty<GUILayoutOption>()))
            {
                NewCardMod(currentCard, removeSpecialAbility: current.Id);
            }
        }
        GUILayout.EndScrollView();
    }

    private static void HandleStats(PlayableCard currentCard)
    {
        GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
        GUILayout.Label("Atk " + currentCard.Attack, Array.Empty<GUILayoutOption>());
        GUILayout.Label("HP " + currentCard.Health, Array.Empty<GUILayoutOption>());
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
        if (GUILayout.Button("-", Array.Empty<GUILayoutOption>()))
        {
            NewCardMod(currentCard, (currentCard.Attack > 0) ? (-1) : 0);
        }
        if (GUILayout.Button("+", Array.Empty<GUILayoutOption>()))
        {
            NewCardMod(currentCard, 1);
        }
        if (GUILayout.Button("-", Array.Empty<GUILayoutOption>()))
        {
            NewCardMod(currentCard, 0, (currentCard.Health > 0) ? (-1) : 0);
        }
        if (GUILayout.Button("+", Array.Empty<GUILayoutOption>()))
        {
            NewCardMod(currentCard, 0, 1);
        }
        GUILayout.EndHorizontal();
    }

    private static void NewCardMod(PlayableCard card, int attackAdjustment = 0, int healthAdjustment = 0,
        Ability ability = 0, Ability negateAbility = 0,
        int bloodCostAdjustment = 0, int boneCostAdjustment = 0, int energyCostAdjustment = 0,
        SpecialTriggeredAbility specialAbility = 0, SpecialTriggeredAbility removeSpecialAbility = 0,
        List<GemType> addGemCost = null, bool? gemified = null)
    {
        CardModificationInfo val = new()
        {
            attackAdjustment = attackAdjustment,
            healthAdjustment = healthAdjustment,
            bloodCostAdjustment = bloodCostAdjustment,
            bonesCostAdjustment = boneCostAdjustment,
            energyCostAdjustment = energyCostAdjustment
        };

        if (ability != Ability.None)
            val.abilities.Add(ability);

        if (negateAbility != Ability.None)
            val.negateAbilities.Add(negateAbility);

        if (addGemCost != null)
            val.addGemCost = addGemCost;

        if (gemified.HasValue)
            val.gemify = gemified.Value;

        if (specialAbility != SpecialTriggeredAbility.None)
            val.specialAbilities.Add(specialAbility);

        if (removeSpecialAbility != SpecialTriggeredAbility.None)
        {
            foreach (CardModificationInfo mod in card.Info.Mods.Concat(card.TemporaryMods))
            {
                if (mod.specialAbilities.Contains(removeSpecialAbility))
                {
                    mod.specialAbilities.Remove(removeSpecialAbility);
                    card.TriggerHandler.specialAbilities.Remove(card.TriggerHandler.specialAbilities.Find(x => x.Item1 == removeSpecialAbility));
                    RemoveSpecialAbility<SpecialCardBehaviour>(removeSpecialAbility.ToString(), card.gameObject);
                }
            }
        }

        card.AddTemporaryMod(val);
        foreach (var special in val.specialAbilities)
        {
            card.TriggerHandler.permanentlyAttachedBehaviours.Add(AddSpecialAbility<SpecialCardBehaviour>(special.ToString(), card.gameObject));
        }
    }
    private static void RemoveSpecialAbility<T>(string typeString, GameObject obj) where T : TriggerReceiver
    {
        Type type = CardTriggerHandler.GetType(typeString);
        T val = obj.GetComponent(type) as T;
        if (val != null)
        {
            UnityEngine.Object.Destroy(val);
        }
    }
    private static T AddSpecialAbility<T>(string typeString, GameObject obj) where T : TriggerReceiver
    {
        Type type = CardTriggerHandler.GetType(typeString);
        return obj.GetComponent(type) as T ?? obj.AddComponent(type) as T;

    }
    private static void HandleCost(PlayableCard currentCard)
    {
        GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());

        // Blood
        GUILayout.Label("Blood " + currentCard.BloodCost(), Array.Empty<GUILayoutOption>());
        if (GUILayout.Button("-", Array.Empty<GUILayoutOption>()))
            NewCardMod(currentCard, 0, 0, 0, 0, (currentCard.BloodCost() > 0) ? (-1) : 0);

        if (GUILayout.Button("+", Array.Empty<GUILayoutOption>()))
            NewCardMod(currentCard, 0, 0, 0, 0, 1);

        // Bone
        GUILayout.Label("Bone " + currentCard.BonesCost(), Array.Empty<GUILayoutOption>());
        if (GUILayout.Button("-", Array.Empty<GUILayoutOption>()))
            NewCardMod(currentCard, 0, 0, 0, 0, 0, (currentCard.BonesCost() > 0) ? (-1) : 0);

        if (GUILayout.Button("+", Array.Empty<GUILayoutOption>()))
            NewCardMod(currentCard, 0, 0, 0, 0, 0, 1);

        // Energy
        GUILayout.Label("Energy " + currentCard.EnergyCost, Array.Empty<GUILayoutOption>());
        if (GUILayout.Button("-", Array.Empty<GUILayoutOption>()))
            NewCardMod(currentCard, 0, 0, 0, 0, 0, 0, (currentCard.EnergyCost > 0) ? (-1) : 0);

        if (GUILayout.Button("+", Array.Empty<GUILayoutOption>()))
            NewCardMod(currentCard, 0, 0, 0, 0, 0, 0, 1);

        GUILayout.EndHorizontal();

        // Gems
        GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
        GUILayout.Label("Gems " + string.Join(",", currentCard.GemsCost()), Array.Empty<GUILayoutOption>());

        CardModificationInfo gemifiedMod = currentCard.Info.Mods.Concat(currentCard.TemporaryMods).ToList().Find((a) => a.gemify);
        if (gemifiedMod != null)
        {
            if (GUILayout.Button("Ungemify", Array.Empty<GUILayoutOption>()))
                currentCard.Info.Mods.Concat(currentCard.TemporaryMods).ToList().Remove(gemifiedMod);
        }
        else if (GUILayout.Button("Gemify", Array.Empty<GUILayoutOption>()))
        NewCardMod(currentCard, gemified: true);

        List<CardModificationInfo> gemMods = currentCard.Info.Mods.Concat(currentCard.TemporaryMods).ToList().FindAll((a) => a.addGemCost != null);

        foreach (GemType gemType in Enum.GetValues(typeof(GemType)))
        {
            CardModificationInfo gemMod = gemMods.Find((a) => a.addGemCost.Contains(gemType));
            GUILayout.Label(gemType.ToString());
            if (gemMod != null)
            {
                GUILayout.Label("+");
                if (GUILayout.Button("-"))
                currentCard.Info.Mods.Concat(currentCard.TemporaryMods).ToList().Remove(gemMod);
            }
            else
            {
                if (GUILayout.Button("+"))
                    NewCardMod(currentCard, addGemCost: new List<GemType>() { gemType });

                GUILayout.Label("-");
            }
        }
        GUILayout.EndHorizontal();
    }

    private static int selectedTab = 0;
    private static readonly string[] sigilsAbilities = new string[2] { "Sigils", "Special Abilities" };
    private static readonly string[] abilityManagementTabs = new string[2] { "Remove", "Add" };
    private static readonly string[] specialAbilitySelectorList = new string[2] { "Remove", "Add" };

    private static int abilityManagerIndex = 0;
    private static string abilitySearch = "";
    private static string abilityGUIDSearch = "";
    private static string specialAbilitySearch = "";
    private static string specialAbilityGUIDSearch = "";
    private static int currentPageEdit = 0;
    private static int currentPageAdd = 0;
    private static int specialAbilitySelector = 0;
    private static Vector2 specialAbilityListVector2 = Vector2.zero;
    private static Vector2 specialAbilityListVector = Vector2.zero;
}