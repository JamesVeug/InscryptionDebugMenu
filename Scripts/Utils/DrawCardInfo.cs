using DiskCardGame;
using GBC;
using InscryptionAPI.Card;
using InscryptionAPI.CardCosts;
using System.Transactions;
using UnityEngine;
using Object = System.Object;

namespace DebugMenu.Scripts.Utils;

public static class DrawCardInfo // used for the deck editors
{
    private static bool HasModFromCardMerge(CardInfo cardInfo, PlayableCard playableCard)
    {
        return cardInfo.HasModFromCardMerge() || (playableCard != null && playableCard.TemporaryMods.Exists(x => x.fromCardMerge));
    }

	public static Result OnGUI(CardInfo cardInfo, PlayableCard playableCard = null, DeckInfo deckInfo = null)
	{
		if (cardInfo == null)
			return Result.None;

        bool onBoard = playableCard != null && (playableCard.OnBoard || playableCard.QueuedSlot != null);

		GUILayout.BeginHorizontal();
		GUILayout.Label("Card Editor", Helpers.HeaderLabelStyle());
        GUILayout.Label("(" + cardInfo.name + ")", Helpers.HeaderLabelStyle());
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (HasModFromCardMerge(cardInfo, playableCard)) // clear card merge data (remove patches)
        {
            if (GUILayout.Button("Clear Merge Data"))
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
		}
        else
        {
            GUILayout.Label("Clear Merge Data", Helpers.DisabledButtonStyle());
        }

		if (SaveManager.SaveFile.IsPart2)
		{
            GUILayout.Label("No emissions in Act 2!", Helpers.DisabledButtonStyle());
        }
		else
		{
            CardModificationInfo emissionMod = cardInfo.Mods.Find(a => a.singletonId == EmissionMod);
            if (emissionMod != null)
            {
                if (GUILayout.Button("Unforce Emission"))
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
            else if (GUILayout.Button("Force Emission"))
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
        if (deckInfo != null && deckInfo.Cards.Count > 2 && GUILayout.Button("Remove Selected Card"))
        {
            // remove the selected card
            deckInfo.RemoveCard(cardInfo);
            SaveManager.SaveToFile(false);
            return Result.Removed;
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (HandleForcePortrait(cardInfo, deckInfo))
            return Result.Altered;

        GUILayout.EndHorizontal();

        // cost modifications
        if (HandleCost(cardInfo, playableCard, deckInfo))
			return Result.Altered;

        // shows the default and alternate portraits (none of the API portraits)
        GUILayout.BeginHorizontal();
		DisplayPortraits(cardInfo);
		GUILayout.EndHorizontal();

        HandleTribes(cardInfo, playableCard, deckInfo);

        // stat modifications
        if (HandleStats(cardInfo, playableCard, deckInfo))
			return Result.Altered;

        if (cardInfo == null)
            return Result.None;

        int correctTab;
        if (onBoard)
        {
            boardSelectedTab = GUILayout.Toolbar(boardSelectedTab, sigilsAbilities);
            correctTab = boardSelectedTab;
        }
        else
        {
            selectedTab = GUILayout.Toolbar(selectedTab, sigilsAbilities);
            correctTab = selectedTab;
        }

        // manage adding/removing abilities and specials
        // special abilities are invisible so no need to update the render
        if (correctTab == 0)
        {
            if (ManageAbilities(cardInfo, playableCard, deckInfo, onBoard))
                return Result.Altered;
        }
        else
            ManageSpecialAbilities(cardInfo, playableCard, deckInfo, onBoard);

        return Result.None;
	}

    private static bool HandleForcePortrait(CardInfo cardInfo, DeckInfo deckInfo)
    {
        bool hasForcedPortrait = cardInfo.Mods.Exists(
            x => x.singletonId == PortraitMod ||
            x.singletonId == ShieldPortraitMod ||
            x.singletonId == SacrificePortraitMod ||
            x.singletonId == TrapPortraitMod);

        if (hasForcedPortrait) // if we have a portrait mod
        {
            if (GUILayout.Button("Reset Portrait"))
            {
                cardInfo.Mods.RemoveAll(
                    x => x.singletonId == PortraitMod ||
                    x.singletonId == ShieldPortraitMod ||
                    x.singletonId == SacrificePortraitMod ||
                    x.singletonId == TrapPortraitMod);

                if (deckInfo != null)
                {
                    deckInfo?.UpdateModDictionary();
                    SaveManager.SaveToFile(false);
                }
                return true;
            }
        }
        else
        {
            GUILayout.Label("Reset Portrait", Helpers.DisabledButtonStyle());
        }

        bool hasAltPortrait = SaveManager.SaveFile.IsPart2 ? cardInfo.HasPixelAlternatePortrait() : cardInfo.HasAlternatePortrait();
        bool hasShieldPortrait = SaveManager.SaveFile.IsPart2 ? cardInfo.HasPixelBrokenShieldPortrait() : cardInfo.HasBrokenShieldPortrait();
        bool hasSacrificePortrait = SaveManager.SaveFile.IsPart2 ? cardInfo.HasPixelSacrificablePortrait() : cardInfo.HasSacrificablePortrait();
        bool hasTrapPortrait = SaveManager.SaveFile.IsPart2 ? cardInfo.HasPixelSteelTrapPortrait() : cardInfo.HasSteelTrapPortrait();
        if (hasAltPortrait && GUILayout.Button("Force Alt"))
        {
            cardInfo.Mods.RemoveAll(
                x => x.singletonId == PortraitMod ||
                x.singletonId == ShieldPortraitMod ||
                x.singletonId == SacrificePortraitMod ||
                x.singletonId == TrapPortraitMod);

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
            return true;
        }
        else
        {
            GUILayout.Label("No Alt Portrait", Helpers.DisabledButtonStyle());
        }
        if (hasShieldPortrait && GUILayout.Button("Force Shield"))
        {
            cardInfo.Mods.RemoveAll(
                x => x.singletonId == PortraitMod ||
                x.singletonId == ShieldPortraitMod ||
                x.singletonId == SacrificePortraitMod ||
                x.singletonId == TrapPortraitMod);

            CardModificationInfo portrait = new() { singletonId = ShieldPortraitMod };
            if (deckInfo != null)
            {
                deckInfo.ModifyCard(cardInfo, portrait);
                SaveManager.SaveToFile(false);
            }
            else
            {
                cardInfo.Mods.Add(portrait);
            }
            return true;
        }
        else
        {
            GUILayout.Label("No Shield Portrait", Helpers.DisabledButtonStyle());
        }
        if (hasSacrificePortrait && GUILayout.Button("Force Sacrifice"))
        {
            cardInfo.Mods.RemoveAll(
                x => x.singletonId == PortraitMod ||
                x.singletonId == ShieldPortraitMod ||
                x.singletonId == SacrificePortraitMod ||
                x.singletonId == TrapPortraitMod);

            CardModificationInfo portrait = new() { singletonId = SacrificePortraitMod };
            if (deckInfo != null)
            {
                deckInfo.ModifyCard(cardInfo, portrait);
                SaveManager.SaveToFile(false);
            }
            else
            {
                cardInfo.Mods.Add(portrait);
            }
            return true;
        }
        else
        {
            GUILayout.Label("No Sacrifice Portrait", Helpers.DisabledButtonStyle());
        }
        if (hasTrapPortrait && GUILayout.Button("Force Trap"))
        {
            cardInfo.Mods.RemoveAll(
                x => x.singletonId == PortraitMod ||
                x.singletonId == ShieldPortraitMod ||
                x.singletonId == SacrificePortraitMod ||
                x.singletonId == TrapPortraitMod);

            CardModificationInfo portrait = new() { singletonId = TrapPortraitMod };
            if (deckInfo != null)
            {
                deckInfo.ModifyCard(cardInfo, portrait);
                SaveManager.SaveToFile(false);
            }
            else
            {
                cardInfo.Mods.Add(portrait);
            }
            return true;
        }
        else
        {
            GUILayout.Label("No Trap Portrait", Helpers.DisabledButtonStyle());
        }
        return false;
    }
    private static bool HandleTribes(CardInfo cardInfo, PlayableCard playableCard, DeckInfo deckInfo)
	{
		GUILayout.BeginHorizontal();
		GUILayout.Label("<b>Tribes:</b> " + string.Join(",", cardInfo.tribes.Select(Helpers.GetTribeName)));
		GUILayout.EndHorizontal();
		return false;
	}
    
    private static bool DisplayPixelPortraits(CardInfo cardInfo, GUILayoutOption[] options)
    {
        bool hasPortrait = false;
        if (cardInfo.pixelPortrait?.texture != null)
        {
            hasPortrait = true;
            GUILayout.Label(cardInfo.pixelPortrait.texture, options);
        }
        if (cardInfo.HasPixelAlternatePortrait())
        {
            hasPortrait = true;
            GUILayout.Label(cardInfo.PixelAlternatePortrait().texture, options);
        }
        if (cardInfo.HasPixelBrokenShieldPortrait())
        {
            hasPortrait = true;
            GUILayout.Label(cardInfo.PixelBrokenShieldPortrait().texture, options);
        }
        if (cardInfo.HasPixelSacrificablePortrait())
        {
            hasPortrait = true;
            GUILayout.Label(cardInfo.PixelSacrificablePortrait().texture, options);
        }
        if (cardInfo.HasPixelSteelTrapPortrait())
        {
            hasPortrait = true;
            GUILayout.Label(cardInfo.PixelSteelTrapPortrait().texture, options);
        }

        return hasPortrait;
    }
    private static bool DisplayPortraits(CardInfo cardInfo, GUILayoutOption[] options)
    {
        bool hasPortrait = false;
        if (cardInfo.portraitTex?.texture != null)
        {
            hasPortrait = true;
            GUILayout.Label(cardInfo.portraitTex.texture, options);
        }
        if (cardInfo.HasAlternatePortrait())
        {
            hasPortrait = true;
            GUILayout.Label(cardInfo.alternatePortrait.texture, options);
        }
        if (cardInfo.HasBrokenShieldPortrait())
        {
            hasPortrait = true;
            GUILayout.Label(cardInfo.BrokenShieldPortrait().texture, options);
        }
        if (cardInfo.HasSacrificablePortrait())
        {
            hasPortrait = true;
            GUILayout.Label(cardInfo.SacrificablePortrait().texture, options);
        }
        if (cardInfo.HasSteelTrapPortrait())
        {
            hasPortrait = true;
            GUILayout.Label(cardInfo.SteelTrapPortrait().texture, options);
        }
        return hasPortrait;
    }

    private static void DisplayPortraits(CardInfo cardInfo)
	{
        bool showingPixel, showingNormal;
		bool isAct2 = Helpers.GetCurrentSavedAct() == Helpers.Acts.Act2;
		GUILayoutOption[] options = new GUILayoutOption[] { GUILayout.Width(114), GUILayout.Height(94)};

        if (isAct2) // display pixel portraits first if it's Act 2
        {
            showingPixel = DisplayPixelPortraits(cardInfo, options);
            showingNormal = DisplayPortraits(cardInfo, options);
		}
		else
		{
            showingNormal = DisplayPortraits(cardInfo, options);
            showingPixel = DisplayPixelPortraits(cardInfo, options);
        }
        if (!showingNormal && !showingPixel)
            GUILayout.Label("No portraits", options);
    }

	private static bool NewCardMod(DeckInfo deckInfo, CardInfo cardInfo, PlayableCard playableCard = null, int attackAdjustment = 0, int healthAdjustment = 0,
		Ability ability = 0, Ability negateAbility = 0,
		int bloodCostAdjustment = 0, int boneCostAdjustment = 0, int energyCostAdjustment = 0,
		SpecialTriggeredAbility specialAbility = 0, SpecialTriggeredAbility removeSpecialAbility = 0,
		List<GemType> addGemCost = null, List<GemType> removeGemCost = null, bool ? gemified = null, bool? nullifyGems = null)
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

        if (removeGemCost != null)
            val.RemoveGemsCost(removeGemCost.ToArray());

        if (nullifyGems.HasValue)
            val.nullifyGemsCost = nullifyGems.Value;

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
		GUILayout.BeginHorizontal();

        int blood = currentCard != null ? currentCard.BloodCost() : currentCardInfo.BloodCost;
        int bones = currentCard != null ? currentCard.BonesCost() : currentCardInfo.BonesCost;
        int energy = currentCard != null ? currentCard.EnergyCost : currentCardInfo.EnergyCost;
        List<GemType> gems = currentCard != null ? currentCard.GemsCost() : currentCardInfo.GemsCost;

        GUILayout.Label("<b>Blood:</b> " + blood);
		if (GUILayout.Button("-"))
			return NewCardMod(deckInfo, currentCardInfo, currentCard, 0, 0, 0, 0, (blood > 0) ? (-1) : 0);

		if (GUILayout.Button("+"))
            return NewCardMod(deckInfo, currentCardInfo, currentCard, 0, 0, 0, 0, 1);
		
		// Bone
		GUILayout.Label("<b>Bones:</b> " + bones);
		if (GUILayout.Button("-"))
			return NewCardMod(deckInfo, currentCardInfo, currentCard, 0, 0, 0, 0, 0, (bones > 0) ? (-1) : 0);

		if (GUILayout.Button("+"))
			return NewCardMod(deckInfo, currentCardInfo, currentCard, 0, 0, 0, 0, 0, 1);
		
		// Energy
		GUILayout.Label("<b>Energy:</b> " + energy);
		if (GUILayout.Button("-"))
			return NewCardMod(deckInfo, currentCardInfo, currentCard, 0, 0, 0, 0, 0, 0, (energy > 0) ? (-1) : 0);

		if (GUILayout.Button("+"))
			return NewCardMod(deckInfo, currentCardInfo, currentCard, 0, 0, 0, 0, 0, 0, 1);

        GUILayout.Label("<b>Mox:</b> " + string.Join(",", gems));
        CardModificationInfo gemifiedMod = currentCardInfo.Mods.Find((a) => a.gemify);
        if (gemifiedMod != null)
        {
            if (GUILayout.Button("Ungemify"))
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
        else if (GUILayout.Button("Gemify"))
        {
            return NewCardMod(deckInfo, currentCardInfo, currentCard, gemified: true);
        }
        GUILayout.EndHorizontal();
		
		// Gems
		GUILayout.BeginHorizontal();
		List<CardModificationInfo> addGemMods = currentCardInfo.Mods.FindAll(a => a.addGemCost != null);
        List<CardModificationInfo> removeGemMods = currentCardInfo.Mods.FindAll(a => a.HasRemovedAnyGemCost());
        foreach (GemType gemType in Enum.GetValues(typeof(GemType)))
		{
			GUILayout.Label(gemType.ToString());

            // if the current card possesses this gem type, check if it was obtained via a card mod
            if (gems.Contains(gemType))
            {
                GUILayout.Label("+", Helpers.DisabledButtonStyle());
                if (GUILayout.Button("-"))
                {
                    CardModificationInfo gemMod = addGemMods.Find(a => a.addGemCost.Contains(gemType));
                    if (gemMod != null)
                    {
                        // if the gemType was obtained via a mod, remove the mod
                        currentCardInfo.Mods.Remove(gemMod);
                    }
                    else
                    {
                        // if the gemType is innate, negate it
                        return NewCardMod(deckInfo, currentCardInfo, currentCard, removeGemCost: new() { gemType });
                    }

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
                {
                    // if the gemType we're trying to add is negated, un-negate it
                    CardModificationInfo removeMod = removeGemMods.Find(x => x.HasRemovedGemCost(gemType));
                    if (removeMod != null)
                    {
                        currentCardInfo.Mods.Remove(removeMod);
                        if (currentCardInfo.GemsCost.Contains(gemType))
                        {
                            if (deckInfo != null)
                            {
                                deckInfo.UpdateModDictionary();
                                SaveManager.SaveToFile(false);
                            }
                            return true;
                        }
                        // if the card doesn't have the gem even after un-negating it, add it as a mod
                    }
                    return NewCardMod(deckInfo, currentCardInfo, currentCard, addGemCost: new List<GemType>() { gemType });
                }

                GUILayout.Label("-", Helpers.DisabledButtonStyle());
            }
		}

        CardModificationInfo nullifyGems = currentCardInfo.Mods.Find((a) => a.nullifyGemsCost);
        if (nullifyGems != null)
        {
            if (GUILayout.Button("Un-Nullify Gems"))
            {
                currentCardInfo.Mods.Remove(nullifyGems);
                if (deckInfo != null)
                {
                    deckInfo.UpdateModDictionary();
                    SaveManager.SaveToFile(false);
                }
                return true;
            }
        }
        else if (GUILayout.Button("Nullify Gems"))
        {
            return NewCardMod(deckInfo, currentCardInfo, currentCard, nullifyGems: true);
        }

        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        var costs = currentCardInfo.GetCustomCosts();
        if (costs.Count == 0)
        {
            GUILayout.Label("");
        }
        else
        {
            foreach (var co in costs)
            {
                int customCost = currentCardInfo.GetCustomCost(co.CostName);
                CardModificationInfo mod = currentCardInfo.Mods.Find(x => x.CleanId() == CustomCostMod); // mod containing debug menu adjustments
                mod ??= new() { singletonId = CustomCostMod };

                GUILayout.Label($"<b>{co.CostName}</b>: {customCost}");

                if (GUILayout.Button("-"))
                {
                    currentCardInfo.Mods.Remove(mod);

                    int modAdjustment = mod.GetCustomCostIdValue(co.CostName) - 1;
                    mod.SetCustomCostId(co.CostName, modAdjustment);
                    if (currentCard != null)
                    {
                        currentCard.AddTemporaryMod(mod);
                    }
                    else if (deckInfo != null)
                    {
                        deckInfo.ModifyCard(currentCardInfo, mod);
                        SaveManager.SaveToFile(false);
                    }
                    return true;
                }
                if (GUILayout.Button("+"))
                {
                    currentCardInfo.Mods.Remove(mod);

                    int modAdjustment = mod.GetCustomCostIdValue(co.CostName) + 1;
                    mod.SetCustomCostId(co.CostName, modAdjustment);
                    if (currentCard != null)
                    {
                        currentCard.AddTemporaryMod(mod);
                    }
                    else if (deckInfo != null)
                    {
                        deckInfo.ModifyCard(currentCardInfo, mod);
                        SaveManager.SaveToFile(false);
                    }
                    return true;
                }
            }
        }
        GUILayout.EndHorizontal();
		return false;
	}
    private static bool HandleStats(CardInfo currentCardInfo, PlayableCard currentCard, DeckInfo deckInfo)
	{
        int attack = currentCard != null ? currentCard.Attack : currentCardInfo.Attack;
        int health = currentCard != null ? currentCard.Health : currentCardInfo.Health;

        GUILayout.BeginHorizontal();
		GUILayout.Label("<b>Atk:</b> " + attack);
        if (GUILayout.Button("-"))
            return NewCardMod(deckInfo, currentCardInfo, currentCard, (attack > 0) ? (-1) : 0);

        if (GUILayout.Button("+"))
            return NewCardMod(deckInfo, currentCardInfo, currentCard, 1);

        GUILayout.Label("<b>HP:</b> " + health);
        if (GUILayout.Button("-"))
            return NewCardMod(deckInfo, currentCardInfo, currentCard, 0, (health > 0) ? (-1) : 0);

        if (GUILayout.Button("+"))
            return NewCardMod(deckInfo, currentCardInfo, currentCard, 0, 1);

        GUILayout.EndHorizontal();
		
		return false;
	}

    private static bool ManageAbilities(CardInfo currentCardInfo, PlayableCard currentCard, DeckInfo deckInfo, bool onBoard)
	{
        int managerIndex;
        if (onBoard)
            managerIndex = boardAbilityManagerIndex = GUILayout.Toolbar(boardAbilityManagerIndex, abilityManagementTabs);
        else
            managerIndex = abilityManagerIndex = GUILayout.Toolbar(abilityManagerIndex, abilityManagementTabs);

        if (currentCardInfo == null)
        {
            GUILayout.Label("No card selected!", Helpers.HeaderLabelStyle());
            return false;
        }

        if (managerIndex == 0)
            return EditAbilities(currentCardInfo, currentCard, deckInfo, onBoard);
        else
            return AddAbilities(currentCardInfo, currentCard, deckInfo, onBoard);
	}
    private static bool AddAbilities(CardInfo currentCardInfo, PlayableCard currentCard, DeckInfo deckInfo, bool onBoard)
    {
        bool endRow = false;
        var cardAbilityList = GetAbilitiesThatContain(AbilityManager.AllAbilities, onBoard);
        if (onBoard)
            NewPager(ref boardCurrentPageAdd, (cardAbilityList.Count - 1) / sigilsPerPage);
        else
            NewPager(ref currentPageAdd, (cardAbilityList.Count - 1) / sigilsPerPage);

        int numAdded = 0;
        int currentIndex = (onBoard ? boardCurrentPageAdd : currentPageAdd) * sigilsPerPage;

        while (currentIndex < (onBoard ? boardCurrentPageAdd : currentPageAdd) * sigilsPerPage + sigilsPerPage && currentIndex < cardAbilityList.Count)
        {
            AbilityManager.FullAbility ability = cardAbilityList[currentIndex];
            if (numAdded == 0)
            {
                GUILayout.BeginHorizontal();
                endRow = true;
            }

            Texture val = AbilitiesUtil.LoadAbilityIcon(ability.Id.ToString());
            GUIContent content = new() { image = val, text = ability.Info.rulebookName };
            if (GUILayout.Button(content, GUILayout.Width(overrideSquareWidth), GUILayout.Height(overrideSquareHeight), new(GUILayoutOption.Type.alignStart, true)))
            {
                return NewCardMod(deckInfo, currentCardInfo, currentCard, ability: ability.Id);
            }
            numAdded++;
            currentIndex++;
            if (numAdded == sigilsPerRow)
            {
                GUILayout.EndHorizontal();
                endRow = false;
                numAdded = 0;
            }
        }

        if (endRow)
            GUILayout.EndHorizontal();

        return false;
    }
    private static bool EditAbilities(CardInfo currentCardInfo, PlayableCard currentCard, DeckInfo deckInfo, bool onBoard)
	{
        List<AbilityManager.FullAbility> abilities = new(AbilityManager.AllAbilities);
        if (onBoard)
            abilities.RemoveAll(x => !currentCard.AllAbilities().Contains(x.Id));
        else
            abilities.RemoveAll(x => !currentCardInfo.Abilities.Contains(x.Id));

		var cardAbilityList = GetAbilitiesThatContain(abilities, onBoard);
		if (cardAbilityList.Count <= 0)
		{
			GUILayout.Label("No Sigils");
			return false;
		}
		
		bool endRow = false;
        if (onBoard)
            NewPager(ref boardCurrentPageEdit, (cardAbilityList.Count - 1) / sigilsPerPage);
        else
            NewPager(ref currentPageEdit, (cardAbilityList.Count - 1) / sigilsPerPage);
        
        int numAdded = 0;
        int currentIndex = (onBoard ? boardCurrentPageEdit : currentPageEdit) * sigilsPerPage;
		while (currentIndex < (onBoard ? boardCurrentPageEdit : currentPageEdit) * sigilsPerPage + sigilsPerPage && currentIndex < cardAbilityList.Count)
		{
			AbilityManager.FullAbility abilityOnCard = cardAbilityList[currentIndex];
			if (numAdded == 0)
			{
				GUILayout.BeginHorizontal();
                endRow = true;
			}

            List<CardModificationInfo> allMods = currentCardInfo.Mods;
            if (onBoard)
                allMods.AddRange(currentCard.TemporaryMods);

            CardModificationInfo fromMod = allMods.Find(x => x.abilities != null && x.abilities.Contains(abilityOnCard.Id));
            Texture val = AbilitiesUtil.LoadAbilityIcon(abilityOnCard.Id.ToString());
            string text = "  " + abilityOnCard.Info.rulebookName;

            if (fromMod != null)
            {
                if (fromMod.fromCardMerge)
                {
                    text += "\n  <color=#69FFA0>(Merge)</color>";
                }
                else if (fromMod.fromTotem)
                {
                    text = "\n  <color=#FBAA4E>(Totem)</color>";
                }
                else if (fromMod.fromLatch)
                {
                    text = "\n  <color=#99E6FF>(Latch)</color>";
                }
                else if (fromMod.fromOverclock)
                {
                    text = "\n  <color=#FFF060>(Overclock)</color>";
                }
                else
                {
                    text += "\n  (Card Mod)";
                }
            }

            GUIContent content = new() { image = val, text = text };
            if (GUILayout.Button(content, GUILayout.Width(overrideSquareWidth), GUILayout.Height(overrideSquareHeight)))
            {
                return NewCardMod(deckInfo, currentCardInfo, currentCard, negateAbility: abilityOnCard.Id);
            }
            numAdded++;
            currentIndex++;
            if (numAdded == sigilsPerRow)
            {
                GUILayout.EndHorizontal();
                endRow = false;
                numAdded = 0;
            }
        }
		if (endRow)
			GUILayout.EndHorizontal();

		return false;
	}

    private static void ManageSpecialAbilities(CardInfo currentCardInfo, PlayableCard currentCard, DeckInfo deckInfo, bool onBoard)
    {
        int managerIndex;
        if (onBoard)
            managerIndex = boardSpecialAbilitySelector = GUILayout.Toolbar(boardSpecialAbilitySelector, abilityManagementTabs);
        else
            managerIndex = specialAbilitySelector = GUILayout.Toolbar(specialAbilitySelector, abilityManagementTabs);

        if (currentCardInfo == null)
        {
            GUILayout.Label("No card selected!", Helpers.HeaderLabelStyle());
            return;
        }

        if (managerIndex == 0)
            RemoveSpecialAbility(currentCardInfo, currentCard, deckInfo, onBoard);
        else
            AddSpecialAbility(currentCardInfo, currentCard, deckInfo, onBoard);
    }
    private static void AddSpecialAbility(CardInfo currentCardInfo, PlayableCard currentCard, DeckInfo deckInfo, bool onBoard)
    {
        var result = GetSpecialAbilitiesThatContain(SpecialTriggeredAbilityManager.AllSpecialTriggers, onBoard);

        specialAbilityListVector = GUILayout.BeginScrollView(specialAbilityListVector);
        foreach (SpecialTriggeredAbilityManager.FullSpecialTriggeredAbility allSpecialAbility in result)
        {
            if (GUILayout.Button(allSpecialAbility.AbilityName))
            {
                NewCardMod(deckInfo, currentCardInfo, currentCard, specialAbility: allSpecialAbility.Id);
            }
        }
        GUILayout.EndScrollView();
    }
    private static void RemoveSpecialAbility(CardInfo currentCardInfo, PlayableCard currentCard, DeckInfo deckInfo, bool onBoard)
    {
        List<SpecialTriggeredAbilityManager.FullSpecialTriggeredAbility> abilities = new(SpecialTriggeredAbilityManager.AllSpecialTriggers);
        if (onBoard)
            abilities.RemoveAll(x => !currentCard.AllSpecialAbilities().Contains(x.Id));
        else
            abilities.RemoveAll(x => !currentCardInfo.SpecialAbilities.Contains(x.Id));

        var result = GetSpecialAbilitiesThatContain(abilities, onBoard);
        if (result.Count <= 0)
        {
            GUILayout.Label("No Special Abilities");
            return;
        }

        specialAbilityListVector2 = GUILayout.BeginScrollView(specialAbilityListVector2);
        foreach (SpecialTriggeredAbilityManager.FullSpecialTriggeredAbility specialAbility2 in result)
        {
            SpecialTriggeredAbilityManager.FullSpecialTriggeredAbility current = specialAbility2;
            if (GUILayout.Button(current.AbilityName))
            {
                NewCardMod(deckInfo, currentCardInfo, currentCard, removeSpecialAbility: current.Id);
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
			nameString = GUILayout.TextField(nameString);

			GUILayout.Label("Filter GUID", GUILayout.Width(80));
			guidString = GUILayout.TextField(guidString);
		}

		if (string.IsNullOrEmpty(nameString) && string.IsNullOrEmpty(guidString))
			return false;

		return true;
	}
	private static List<AbilityManager.FullAbility> GetAbilitiesThatContain(List<AbilityManager.FullAbility> searchingList, bool board)
	{
        // modify search strings here
		if (!(board ? Filter(ref boardAbilitySearch, ref boardAbilityGUIDSearch) : Filter(ref abilitySearch, ref abilityGUIDSearch)))
			return searchingList;

        // grab the latest search strings here for reference
        string search = board ? boardAbilitySearch : abilitySearch;
        string searchGUID = board ? boardAbilityGUIDSearch : abilityGUIDSearch;

		List<AbilityManager.FullAbility> results = new();
		foreach (AbilityManager.FullAbility searching in searchingList)
		{
            if (searchGUID != "" && (searching.ModGUID == null || !searching.ModGUID.Contains(searchGUID)))
                continue;

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
            if (searchGUID != "" && (searching.ModGUID == null || !searching.ModGUID.Contains(searchGUID)))
                continue;

            if (search != "" && (!searching.AbilityName.ToLowerInvariant().Contains(search.ToLowerInvariant())))
                continue;

            results.Add(searching);
        }
        return results;
    }

	private static void NewPager(ref int page, int max, int min = 0)
	{
		int num = page;
		GUILayout.BeginHorizontal();
        GUILayout.Label("Page: " + num);
        if (num > min)
        {
            if (GUILayout.Button("<"))
                num--;
        }
        else
        {
            GUILayout.Label("<", Helpers.DisabledButtonStyle());
        }
        
        if (num < max)
        {
            if (GUILayout.Button(">"))
                num++;
        }
        else
        {
            GUILayout.Label(">", Helpers.DisabledButtonStyle());
        }
		
		GUILayout.EndHorizontal();
		page = num;
	}

    private static readonly string[] sigilsAbilities = new string[2] { "Sigils", "Special Abilities" };
    private static readonly string[] abilityManagementTabs = new string[2] { "Remove", "Add" };
    private const float overrideSquareWidth = 195f;
    private const float overrideSquareHeight = 40f;
    private const int sigilsPerRow = 3;
    private const int sigilsPerPage = 9;

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
    public static readonly string ShieldPortraitMod = "DebugMenu:ShieldPortraitMod";
    public static readonly string SacrificePortraitMod = "DebugMenu:SacrificePortraitMod";
    public static readonly string TrapPortraitMod = "DebugMenu:TrapPortraitMod";

    public static readonly string CustomCostMod = "DebugMenu:CustomCostMod";

    public enum Result
    {
        None,
        Removed,
        Altered
    }
}