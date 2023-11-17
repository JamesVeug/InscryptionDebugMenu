using DiskCardGame;
using InscryptionAPI.Card;
using UnityEngine;
using Object = System.Object;

namespace DebugMenu.Scripts.Utils;

public static class DrawCardInfo
{
    private static bool HasModFromCardMerge(CardInfo cardInfo, PlayableCard playableCard)
    {
        return cardInfo.HasModFromCardMerge() || (playableCard != null && playableCard.TemporaryMods.Exists(x => x.fromCardMerge));
    }

	public static Result OnGUI(CardInfo cardInfo, PlayableCard playableCard = null, DeckInfo deckInfo = null)
	{
		if (cardInfo == null)
			return Result.None;

        bool onBoard = playableCard != null;

		GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
		GUILayout.Label("Card Editor (" + cardInfo.name + ")", Array.Empty<GUILayoutOption>());

        // remove the selected card
        if (deckInfo != null && deckInfo.Cards.Count > 2 && GUILayout.Button("Remove", Array.Empty<GUILayoutOption>()))
		{
			deckInfo.RemoveCard(cardInfo);
			SaveManager.SaveToFile(false);
			return Result.Removed;
		}

        // clear card merge data (remove patches)
        if (HasModFromCardMerge(cardInfo, playableCard) && GUILayout.Button("Clear Merge Data", Array.Empty<GUILayoutOption>()))
		{
            cardInfo.Mods.ForEach(x => x.fromCardMerge = false);
            if (onBoard)
            {
                playableCard.TemporaryMods.ForEach(x => x.fromCardMerge = false);
            }
            else
            {
                SaveManager.SaveToFile(false);
            }
			return Result.Altered;
		}

        GUILayout.EndHorizontal();

		if (SaveManager.SaveFile.IsPart2)
		{
            GUILayout.Button("No emissions in Act 2");
        }
		else
		{
            CardModificationInfo emissionMod = cardInfo.Mods.Find(a => a.singletonId == EmissionMod);
            if (emissionMod != null)
            {
                if (GUILayout.Button("Unforce Emission", Array.Empty<GUILayoutOption>()))
                {
                    cardInfo.Mods.Remove(emissionMod);
					if (deckInfo != null)
					{
						deckInfo?.UpdateModDictionary();
						SaveManager.SaveToFile(false);
					}
					return Result.Altered;
                }
            }
            else
            {
                if (GUILayout.Button("Force Emission", Array.Empty<GUILayoutOption>()))
                {
                    CardModificationInfo emission = new() { singletonId = EmissionMod };
                    if (deckInfo != null)
					{
						deckInfo.ModifyCard(cardInfo, emission);
						SaveManager.SaveToFile(false);
					}
                    else
                    {
                        cardInfo.Mods.Add(emission);
                    }
                    return Result.Altered;
                }
            }
        }

        CardModificationInfo portraitMod = cardInfo.Mods.Find(a => a.singletonId == PortraitMod);
        if (portraitMod != null)
        {
            if (GUILayout.Button("Unforce Alt Portrait", Array.Empty<GUILayoutOption>()))
            {
                cardInfo.Mods.Remove(portraitMod);
				if (deckInfo != null)
				{
					deckInfo?.UpdateModDictionary();
					SaveManager.SaveToFile(false);
				}
				return Result.Altered;
            }
        }
        else if (SaveManager.SaveFile.IsPart2 ? cardInfo.HasPixelAlternatePortrait() : cardInfo.HasAlternatePortrait())
        {
            if (GUILayout.Button("Force Alt Portrait", Array.Empty<GUILayoutOption>()))
            {
                CardModificationInfo portrait = new() { singletonId = PortraitMod };
                if (deckInfo != null)
				{
                    deckInfo.ModifyCard(cardInfo, portrait);
                    SaveManager.SaveToFile(false);
                }
                else
                {
                    cardInfo.Mods.Add(portrait);
                }
                return Result.Altered;
            }
        }
		else
		{
			GUILayout.Button("No Alt Portrait");
        }

		// cost modifications
		if (HandleCost(cardInfo, playableCard, deckInfo))
			return Result.Altered;

        HandleTribes(cardInfo, playableCard, deckInfo);

        // shows the default and alternate portraits (none of the API portraits)
        GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
		DisplayPortraits(cardInfo);
		GUILayout.EndHorizontal();

        // stat modifications
        if (HandleStats(cardInfo, playableCard, deckInfo))
			return Result.Altered;

        if (cardInfo == null)
            return Result.None;

        int correctTab;
        if (onBoard)
        {
            boardSelectedTab = GUILayout.Toolbar(boardSelectedTab, sigilsAbilities, Array.Empty<GUILayoutOption>());
            correctTab = boardSelectedTab;
        }
        else
        {
            selectedTab = GUILayout.Toolbar(selectedTab, sigilsAbilities, Array.Empty<GUILayoutOption>());
            correctTab = selectedTab;
        }

        // manage adding/removing abilities and specials
        // special abilities are invisible so no need to update the render
        if (correctTab == 0)
        {
            if (ManageAbilities(cardInfo, playableCard, deckInfo))
                return Result.Altered;
        }
        else
            ManageSpecialAbilities(cardInfo, playableCard, deckInfo);

        return Result.None;
	}

    private static bool HandleTribes(CardInfo cardInfo, PlayableCard playableCard, DeckInfo deckInfo)
	{
		GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
		GUILayout.Label("Tribes: " + string.Join(",", cardInfo.tribes.Select(Helpers.GetTribeName)));
		GUILayout.EndHorizontal();
		return false;
	}
    private static void DisplayPortraits(CardInfo cardInfo)
	{
		bool isAct2 = Helpers.GetCurrentSavedAct() == Helpers.Acts.Act2;
		GUILayoutOption[] options = new GUILayoutOption[] { GUILayout.Width(114), GUILayout.Height(94)};
		
		// display pixel portrait first if it's Act 2
		if (isAct2)
		{
            if (cardInfo.pixelPortrait?.texture != null)
                GUILayout.Label(cardInfo.pixelPortrait.texture, options);

            if (cardInfo.HasPixelAlternatePortrait())
                GUILayout.Label(cardInfo.PixelAlternatePortrait().texture, options);

            if (cardInfo.portraitTex?.texture != null)
				GUILayout.Label(cardInfo.portraitTex.texture, options);

			if (cardInfo.HasAlternatePortrait())
				GUILayout.Label(cardInfo.alternatePortrait.texture, options);
		}
		else
		{
            if (cardInfo.portraitTex?.texture != null)
                GUILayout.Label(cardInfo.portraitTex.texture, options);

            if (cardInfo.HasAlternatePortrait())
                GUILayout.Label(cardInfo.alternatePortrait.texture, options);

            if (cardInfo.pixelPortrait?.texture != null)
                GUILayout.Label(cardInfo.pixelPortrait.texture, options);

            if (cardInfo.HasPixelAlternatePortrait())
                GUILayout.Label(cardInfo.PixelAlternatePortrait().texture, options);
        }
    }

	private static bool NewCardMod(DeckInfo deckInfo, CardInfo cardInfo, PlayableCard playableCard = null, int attackAdjustment = 0, int healthAdjustment = 0,
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
            IEnumerable<CardModificationInfo> allMods = cardInfo.Mods;
            if (playableCard != null)
                allMods.Concat(playableCard.TemporaryMods);

            foreach (CardModificationInfo mod in allMods)
            {
                if (mod.specialAbilities.Contains(removeSpecialAbility))
                {
                    mod.specialAbilities.Remove(removeSpecialAbility);
                    if (playableCard)
                    {
                        playableCard.TriggerHandler.specialAbilities.Remove(playableCard.TriggerHandler.specialAbilities.Find(x => x.Item1 == removeSpecialAbility));
                        RemoveSpecialAbility<SpecialCardBehaviour>(removeSpecialAbility.ToString(), playableCard.gameObject);
                    }
                }
            }
        }
        if (playableCard != null)
        {
            playableCard.AddTemporaryMod(val);
            foreach (var special in val.specialAbilities)
            {
                playableCard.TriggerHandler.permanentlyAttachedBehaviours.Add(AddSpecialAbility<SpecialCardBehaviour>(special.ToString(), playableCard.gameObject));
            }
        }
        else if (deckInfo != null)
		{
			deckInfo.ModifyCard(cardInfo, val);
			SaveManager.SaveToFile(false);
		}
		return true;
	}
    
    private static bool HandleCost(CardInfo currentCardInfo, PlayableCard currentCard, DeckInfo deckInfo)
	{
		GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());

        // Blood
        int blood = currentCard != null ? currentCard.BloodCost() : currentCardInfo.BloodCost;
        int bones = currentCard != null ? currentCard.BonesCost() : currentCardInfo.BonesCost;
        int energy = currentCard != null ? currentCard.EnergyCost : currentCardInfo.EnergyCost;
        List<GemType> gems = currentCard != null ? currentCard.GemsCost() : currentCardInfo.GemsCost;

        GUILayout.Label("Blood " + blood, Array.Empty<GUILayoutOption>());
		if (GUILayout.Button("-", Array.Empty<GUILayoutOption>()))
			return NewCardMod(deckInfo, currentCardInfo, currentCard, 0, 0, 0, 0, (blood > 0) ? (-1) : 0);

		if (GUILayout.Button("+", Array.Empty<GUILayoutOption>()))
            return NewCardMod(deckInfo, currentCardInfo, currentCard, 0, 0, 0, 0, 1);
		
		// Bone
		GUILayout.Label("Bone " + bones, Array.Empty<GUILayoutOption>());
		if (GUILayout.Button("-", Array.Empty<GUILayoutOption>()))
			return NewCardMod(deckInfo, currentCardInfo, currentCard, 0, 0, 0, 0, 0, (bones > 0) ? (-1) : 0);

		if (GUILayout.Button("+", Array.Empty<GUILayoutOption>()))
			return NewCardMod(deckInfo, currentCardInfo, currentCard, 0, 0, 0, 0, 0, 1);
		
		// Energy
		GUILayout.Label("Energy " + energy, Array.Empty<GUILayoutOption>());
		if (GUILayout.Button("-", Array.Empty<GUILayoutOption>()))
			return NewCardMod(deckInfo, currentCardInfo, currentCard, 0, 0, 0, 0, 0, 0, (energy > 0) ? (-1) : 0);

		if (GUILayout.Button("+", Array.Empty<GUILayoutOption>()))
			return NewCardMod(deckInfo, currentCardInfo, currentCard, 0, 0, 0, 0, 0, 0, 1);

		GUILayout.EndHorizontal();
		
		// Gems
		GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
		GUILayout.Label("Gems " + string.Join(",", gems), Array.Empty<GUILayoutOption>());
		
		CardModificationInfo gemifiedMod = currentCardInfo.Mods.Find((a) => a.gemify);
		if (gemifiedMod != null)
		{
			if (GUILayout.Button("Ungemify", Array.Empty<GUILayoutOption>()))
			{
                currentCardInfo.Mods.Remove(gemifiedMod);
				if (deckInfo != null)
				{
					deckInfo.UpdateModDictionary();
					SaveManager.SaveToFile(false);
				}
				return true;
			}
		}
		else if (GUILayout.Button("Gemify", Array.Empty<GUILayoutOption>()))
        {
            return NewCardMod(deckInfo, currentCardInfo, currentCard, gemified: true);
        }
		
		List<CardModificationInfo> gemMods = currentCardInfo.Mods.FindAll((a) => a.addGemCost != null);
		foreach (GemType gemType in Enum.GetValues(typeof(GemType)))
		{
			CardModificationInfo gemMod = gemMods.Find((a)=>a.addGemCost.Contains(gemType));
			GUILayout.Label(gemType.ToString());
			if (gemMod != null)
			{
				GUILayout.Label("+");
				if (GUILayout.Button("-"))
				{
					currentCardInfo.Mods.Remove(gemMod);
					if (deckInfo != null)
					{
						deckInfo.UpdateModDictionary();
						SaveManager.SaveToFile(false);
					}
					return true;
                }
			}
			else
			{
				if (GUILayout.Button("+"))
					return NewCardMod(deckInfo, currentCardInfo, currentCard, addGemCost: new List<GemType>() { gemType });
				
				GUILayout.Label("-");
			}
		}
		GUILayout.EndHorizontal();
		return false;
	}
    private static bool HandleStats(CardInfo currentCardInfo, PlayableCard currentCard, DeckInfo deckInfo)
	{
        int attack = currentCard != null ? currentCard.Attack : currentCardInfo.Attack;
        int health = currentCard != null ? currentCard.Health : currentCardInfo.Health;

        GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
		GUILayout.Label("Atk " + attack, Array.Empty<GUILayoutOption>());
		GUILayout.Label("HP " + health, Array.Empty<GUILayoutOption>());
		GUILayout.EndHorizontal();
		
        GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());

		if (GUILayout.Button("-", Array.Empty<GUILayoutOption>()))
			return NewCardMod(deckInfo, currentCardInfo, currentCard, (attack > 0) ? (-1) : 0);

		if (GUILayout.Button("+", Array.Empty<GUILayoutOption>()))
            return NewCardMod(deckInfo, currentCardInfo, currentCard, 1);

		if (GUILayout.Button("-", Array.Empty<GUILayoutOption>()))
            return NewCardMod(deckInfo, currentCardInfo, currentCard, 0, (health > 0) ? (-1) : 0);

		if (GUILayout.Button("+", Array.Empty<GUILayoutOption>()))
			return NewCardMod(deckInfo, currentCardInfo, currentCard, 0, 1);

		GUILayout.EndHorizontal();
		return false;
	}

    private static bool ManageAbilities(CardInfo currentCardInfo, PlayableCard currentCard, DeckInfo deckInfo)
	{
        if (currentCard != null)
        {
            boardAbilityManagerIndex = GUILayout.Toolbar(boardAbilityManagerIndex, abilityManagementTabs, Array.Empty<GUILayoutOption>());
            GUILayout.Label("Sigils", Array.Empty<GUILayoutOption>());
            if (currentCardInfo == null)
            {
                GUILayout.Label("No Card Selected!", Array.Empty<GUILayoutOption>());
                return false;
            }
            if (boardAbilityManagerIndex == 0)
                return EditAbilities(currentCard);

            else
                return AddAbilities(currentCard);

        }
        else
        {
            abilityManagerIndex = GUILayout.Toolbar(abilityManagerIndex, abilityManagementTabs, Array.Empty<GUILayoutOption>());
            GUILayout.Label("Sigils", Array.Empty<GUILayoutOption>());
            if (currentCardInfo == null)
            {
                GUILayout.Label("No Card Selected!", Array.Empty<GUILayoutOption>());
                return false;
            }
            if (abilityManagerIndex == 0)
                return EditAbilities(currentCardInfo, deckInfo);

            else
                return AddAbilities(currentCardInfo, deckInfo);

        }
	}
    private static bool AddAbilities(CardInfo currentCardInfo, DeckInfo deckInfo)
    {
        bool flag = false;
        var cardAbilityList = GetAbilitiesThatContain(AbilityManager.AllAbilities, false);
        NewPager(ref currentPageAdd, (cardAbilityList.Count - 1) / 8);
        int num = 0, num2 = currentPageAdd * 8;

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
                    return NewCardMod(deckInfo, currentCardInfo, ability: ability.Id);
            }
            else if (GUILayout.Button(val, GUILayout.Width(overrideSquareWidth), GUILayout.Height(overrideSquareHeight)))
            {
                return NewCardMod(deckInfo, currentCardInfo, ability: ability.Id);
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
            GUILayout.EndHorizontal();

        return false;
    }
    private static bool EditAbilities(CardInfo currentCardInfo, DeckInfo deckInfo)
	{
		List<AbilityManager.FullAbility> abilities = AbilityManager.AllAbilities.FindAll(a => currentCardInfo.Abilities.Contains(a.Id));
		var cardAbilityList = GetAbilitiesThatContain(abilities, false);
		if (cardAbilityList.Count <= 0)
		{
			GUILayout.Label("No Sigils", Array.Empty<GUILayoutOption>());
			return false;
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
			if (val == (Object)null)
			{
				if (GUILayout.Button(negateAbility.Info.rulebookName, new GUILayoutOption[2]
				{
					GUILayout.Width((float)overrideSquareWidth),
					GUILayout.Height((float)overrideSquareHeight)
				}))
				{
					return NewCardMod(deckInfo, currentCardInfo, negateAbility: negateAbility.Id);
				}
			}
			else if (GUILayout.Button(val, new GUILayoutOption[2]
			         {
				         GUILayout.Width((float)overrideSquareWidth),
				         GUILayout.Height((float)overrideSquareHeight)
			         }))
			{
				return NewCardMod(deckInfo, currentCardInfo, negateAbility: negateAbility.Id);
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
			GUILayout.EndHorizontal();

		return false;
	}

    private static bool AddAbilities(PlayableCard currentCard)
    {
        bool flag = false;
        var cardAbilityList = GetAbilitiesThatContain(AbilityManager.AllAbilities, true);

        NewPager(ref boardCurrentPageAdd, (cardAbilityList.Count - 1) / 8);
        int num = 0, num2 = boardCurrentPageAdd * 8;

        while (num2 < boardCurrentPageAdd * 8 + 8 && num2 < cardAbilityList.Count)
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
                    return NewCardMod(null, null, currentCard, ability: ability.Id);
            }
            else if (GUILayout.Button(val, GUILayout.Width(overrideSquareWidth), GUILayout.Height(overrideSquareHeight)))
            {
                return NewCardMod(null, null, currentCard, ability: ability.Id);
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
            GUILayout.EndHorizontal();

        return false;
    }
    private static bool EditAbilities(PlayableCard currentCard)
    {
        List<AbilityManager.FullAbility> abilities = AbilityManager.AllAbilities.FindAll((a) => currentCard.AllAbilities().Contains(a.Id));
        var cardAbilityList = GetAbilitiesThatContain(abilities, true);
        if (cardAbilityList.Count <= 0)
        {
            GUILayout.Label("No Sigils", Array.Empty<GUILayoutOption>());
            return false;
        }

        bool flag = false;
        NewPager(ref boardCurrentPageEdit, (cardAbilityList.Count - 1) / 8);
        int num = 0;
        int num2 = boardCurrentPageEdit * 8;
        while (num2 < boardCurrentPageEdit * 8 + 8 && num2 < cardAbilityList.Count)
        {
            AbilityManager.FullAbility negateAbility = cardAbilityList[num2];
            if (num % 2 == 0)
            {
                GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
                flag = true;
            }
            Texture val = AbilitiesUtil.LoadAbilityIcon(negateAbility.Id.ToString(), false, false);
            if (val == (Object)null)
            {
                if (GUILayout.Button(negateAbility.Info.rulebookName, new GUILayoutOption[2]
                {
                    GUILayout.Width((float)overrideSquareWidth),
                    GUILayout.Height((float)overrideSquareHeight)
                }))
                {
                    return NewCardMod(null, null, currentCard, negateAbility: negateAbility.Id);
                }
            }
            else if (GUILayout.Button(val, new GUILayoutOption[2]
                     {
                         GUILayout.Width((float)overrideSquareWidth),
                         GUILayout.Height((float)overrideSquareHeight)
                     }))
            {
                return NewCardMod(null, null, currentCard, negateAbility: negateAbility.Id);
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
            GUILayout.EndHorizontal();

        return false;
    }

    private static void ManageSpecialAbilities(CardInfo currentCardInfo, PlayableCard currentCard, DeckInfo deckInfo)
    {
        if (currentCard != null)
        {
            boardSpecialAbilitySelector = GUILayout.Toolbar(boardSpecialAbilitySelector, specialAbilitySelectorList);
            GUILayout.Label("Special Abilities", Array.Empty<GUILayoutOption>());
            if (boardSpecialAbilitySelector == 0)
                RemoveSpecialAbility(currentCard);
            else
                AddSpecialAbility(currentCard);
        }
        else
        {
            specialAbilitySelector = GUILayout.Toolbar(specialAbilitySelector, specialAbilitySelectorList);
            GUILayout.Label("Special Abilities", Array.Empty<GUILayoutOption>());
            if (specialAbilitySelector == 0)
                RemoveSpecialAbility(currentCardInfo, deckInfo);
            else
                AddSpecialAbility(currentCardInfo, deckInfo);
        }
    }
    private static void AddSpecialAbility(CardInfo currentCard, DeckInfo deckInfo)
    {
        var abilities = SpecialTriggeredAbilityManager.AllSpecialTriggers;
        var result = GetSpecialAbilitiesThatContain(abilities, true);

        specialAbilityListVector = GUILayout.BeginScrollView(specialAbilityListVector);
        foreach (SpecialTriggeredAbilityManager.FullSpecialTriggeredAbility allSpecialAbility in result)
        {
            if (GUILayout.Button(allSpecialAbility.AbilityName, Array.Empty<GUILayoutOption>()))
            {
                NewCardMod(deckInfo, currentCard, specialAbility: allSpecialAbility.Id);
            }
        }
        GUILayout.EndScrollView();
    }
    private static void RemoveSpecialAbility(CardInfo currentCard, DeckInfo deckInfo)
    {
        var abilities = SpecialTriggeredAbilityManager.AllSpecialTriggers.FindAll((a) => currentCard.SpecialAbilities.Contains(a.Id));
        var result = GetSpecialAbilitiesThatContain(abilities, false);

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
                NewCardMod(deckInfo, currentCard, removeSpecialAbility: current.Id);
            }
        }
        GUILayout.EndScrollView();
    }

    private static void AddSpecialAbility(PlayableCard currentCard)
    {
        var abilities = SpecialTriggeredAbilityManager.AllSpecialTriggers;
        var result = GetSpecialAbilitiesThatContain(abilities, true);

        boardSpecialAbilityListVector = GUILayout.BeginScrollView(boardSpecialAbilityListVector);
        foreach (SpecialTriggeredAbilityManager.FullSpecialTriggeredAbility allSpecialAbility in result)
        {
            if (GUILayout.Button(allSpecialAbility.AbilityName, Array.Empty<GUILayoutOption>()))
            {
                NewCardMod(null, null, currentCard, specialAbility: allSpecialAbility.Id);
            }
        }
        GUILayout.EndScrollView();
    }
    private static void RemoveSpecialAbility(PlayableCard currentCard)
    {
        var abilities = SpecialTriggeredAbilityManager.AllSpecialTriggers.FindAll((a) => currentCard.AllSpecialAbilities().Contains(a.Id));
        var result = GetSpecialAbilitiesThatContain(abilities, true);

        if (result.Count <= 0)
        {
            GUILayout.Label("No Special Abilities", Array.Empty<GUILayoutOption>());
            return;
        }

        boardSpecialAbilityListVector2 = GUILayout.BeginScrollView(boardSpecialAbilityListVector2);
        foreach (SpecialTriggeredAbilityManager.FullSpecialTriggeredAbility specialAbility2 in result)
        {
            SpecialTriggeredAbilityManager.FullSpecialTriggeredAbility current = specialAbility2;
            if (GUILayout.Button(current.AbilityName, Array.Empty<GUILayoutOption>()))
            {
                NewCardMod(null, null, currentCard, removeSpecialAbility: current.Id);
            }
        }
        GUILayout.EndScrollView();
    }

    private static T AddSpecialAbility<T>(string typeString, GameObject obj) where T : TriggerReceiver
    {
        Type type = CardTriggerHandler.GetType(typeString);
        return obj.GetComponent(type) as T ?? obj.AddComponent(type) as T;
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

    private static bool Filter(ref string nameString, ref string guidString)
	{
		using (new GUILayout.HorizontalScope())
		{
			GUILayout.Label("Filter Name", GUILayout.Width(80));
			nameString = GUILayout.TextField(nameString, Array.Empty<GUILayoutOption>());

			GUILayout.Label("Filter GUID", GUILayout.Width(80));
			guidString = GUILayout.TextField(guidString, Array.Empty<GUILayoutOption>());
		}

		if (string.IsNullOrEmpty(nameString) && string.IsNullOrEmpty(guidString))
			return false;

		return true;
	}
	private static List<AbilityManager.FullAbility> GetAbilitiesThatContain(List<AbilityManager.FullAbility> searchingList, bool board)
	{
        // modify search strings here
		if (board ? !Filter(ref boardAbilitySearch, ref boardAbilityGUIDSearch) : !Filter(ref abilitySearch, ref abilityGUIDSearch))
			return searchingList;

        // grab the latest search strings here for reference
        string search = board ? boardAbilitySearch : abilitySearch;
        string searchGUID = board ? boardAbilityGUIDSearch : abilityGUIDSearch;

		var results = new List<AbilityManager.FullAbility>();
		foreach (AbilityManager.FullAbility searching in searchingList)
		{
			if (searchGUID != "")
			{
				if (searching.ModGUID == null || !searching.ModGUID.Contains(searchGUID))
					continue;
			}

			if (search != "")
			{
                if (searching.Info == null)
                    continue;

				string rulebookName = searching.Info.rulebookName.ToLowerInvariant();
				string abilityName = searching.Info.name.ToLowerInvariant();
				if (!rulebookName.Contains(search.ToLowerInvariant()) && !abilityName.Contains(search.ToLowerInvariant()))
					continue;
			}
			results.Add(searching);
		}
		return results;
	}
    private static List<SpecialTriggeredAbilityManager.FullSpecialTriggeredAbility> GetSpecialAbilitiesThatContain(List<SpecialTriggeredAbilityManager.FullSpecialTriggeredAbility> searchingList, bool board)
    {
        // modify search strings here
        if (board ? !Filter(ref boardSpecialAbilitySearch, ref boardSpecialAbilityGUIDSearch) : !Filter(ref specialAbilitySearch, ref specialAbilityGUIDSearch))
            return searchingList;

        // grab the latest search strings here for reference
        string search = board ? boardSpecialAbilitySearch : specialAbilitySearch;
        string searchGUID = board ? boardSpecialAbilityGUIDSearch : specialAbilityGUIDSearch;

        var results = new List<SpecialTriggeredAbilityManager.FullSpecialTriggeredAbility>();
        foreach (SpecialTriggeredAbilityManager.FullSpecialTriggeredAbility searching in searchingList)
        {
            if (searchGUID != "")
            {
                if (searching.ModGUID == null || !searching.ModGUID.Contains(searchGUID))
                    continue;
            }

            if (search != "")
            {
                if (!searching.AbilityName.ToLowerInvariant().Contains(search.ToLowerInvariant()))
                    continue;
            }
            results.Add(searching);
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

    private static readonly string[] sigilsAbilities = new string[2] { "Sigils", "Special Abilities" };
    private static readonly string[] abilityManagementTabs = new string[2] { "Remove", "Add" };
    private static readonly string[] specialAbilitySelectorList = new string[2] { "Remove", "Add" };
    private static readonly float overrideSquareWidth = 150;
    private static readonly float overrideSquareHeight = 40;

    private static int selectedTab = 0;
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

    // these exist so the two menus don't interact with each other
    // yes, it's a straight copy-paste job, no I don't care
    private static int boardSelectedTab = 0;
    private static int boardAbilityManagerIndex = 0;
    private static string boardAbilitySearch = "";
    private static string boardAbilityGUIDSearch = "";
    private static string boardSpecialAbilitySearch = "";
    private static string boardSpecialAbilityGUIDSearch = "";
    private static int boardCurrentPageEdit = 0;
    private static int boardCurrentPageAdd = 0;
    private static int boardSpecialAbilitySelector = 0;
    private static Vector2 boardSpecialAbilityListVector2 = Vector2.zero;
    private static Vector2 boardSpecialAbilityListVector = Vector2.zero;

    public static readonly string EmissionMod = "DebugMenu:EmissionMod";
    public static readonly string PortraitMod = "DebugMenu:PortraitMod";

    public enum Result
    {
        None,
        Removed,
        Altered
    }
}