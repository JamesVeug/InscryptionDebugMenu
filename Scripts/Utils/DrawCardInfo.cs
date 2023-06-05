﻿using System.Collections.ObjectModel;
using DiskCardGame;
using InscryptionAPI.Card;
using UnityEngine;
using Object = System.Object;

namespace DebugMenu.Scripts.Utils;

public static class DrawCardInfo
{
	private static string[] sigilsAbilities = new string[2] { "Sigils", "Abilities" };

	private static string[] abilityManagementTabs = new string[2] { "Remove", "Add" };
	private static string[] specialAbilitySelectorList = new string[2] { "Remove", "Add" };
	private static float overrideSquareWidth = 150;
	private static float overrideSquareHeight = 40;
	
	private static int selectedTab = 0;
	private static int abilityManagerIndex = 0;
	private static string lastCardAbilitySearch = "";
	private static List<AbilityManager.FullAbility> cardAbilityList;
	private static string lastAbilitySearch = "";
	private static int currentPageEdit = 0;
	private static int currentPageAdd = 0;
	private static int specialAbilitySelector = 0;
	private static Vector2 specialAbilityListVector2 = Vector2.zero;
	private static Vector2 specialAbilityListVector = Vector2.zero;

	public enum Result
	{
		None,
		Removed,
	}
	
	public static Result OnGUI(CardInfo val, DeckInfo deckInfo)
	{
		if (val == null)
		{
			return Result.None;
		}
		
		GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
		GUILayout.Label("Card Editor (" + val.name + ")", Array.Empty<GUILayoutOption>());
		if (deckInfo.Cards.Count > 2 && GUILayout.Button("Remove", Array.Empty<GUILayoutOption>()))
		{
			deckInfo.RemoveCard(val);
			SaveManager.SaveToFile(false);
			return Result.Removed;
		}
		
		if (val.HasModFromCardMerge() && GUILayout.Button("Clear Merge Data", Array.Empty<GUILayoutOption>()))
		{
			foreach (CardModificationInfo mod in val.Mods)
			{
				mod.fromCardMerge = false;
			}
			
			SaveManager.SaveToFile(false);
			return Result.None;
		}
		GUILayout.EndHorizontal();
		
		HandleCost(val, deckInfo);
		
		GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
		HandlePortrait(val);
		GUILayout.EndHorizontal();

		HandleStats(val, deckInfo);
		selectedTab = GUILayout.Toolbar(selectedTab, sigilsAbilities, Array.Empty<GUILayoutOption>());
		if (val != null)
		{
			if (selectedTab == 0)
			{
				ManageAbilities(val, deckInfo);
			}
			else
			{
				EditSpecialAbilities(val, deckInfo);
			}
		}

		return Result.None;
	}

	private static void HandlePortrait(CardInfo cardInfo)
	{
		bool isAct2 = Helpers.GetCurrentSavedAct() == Helpers.Acts.Act2;
		GUILayoutOption[] options = new GUILayoutOption[] { GUILayout.Width(114), GUILayout.Height(94)};
		if (isAct2)
		{
			if (cardInfo.pixelPortrait != null && cardInfo.pixelPortrait.texture != null)
			{
				GUILayout.Label(cardInfo.pixelPortrait.texture, options);
			}
			if (cardInfo.portraitTex != null && cardInfo.portraitTex.texture != null)
			{
				GUILayout.Label(cardInfo.portraitTex.texture, options);
			}
			if (cardInfo.alternatePortrait != null && cardInfo.alternatePortrait.texture != null)
			{
				GUILayout.Label(cardInfo.alternatePortrait.texture, options);
			}
		}
		else
		{
			if (cardInfo.portraitTex != null && cardInfo.portraitTex.texture != null)
			{
				GUILayout.Label(cardInfo.portraitTex.texture, options);
			}
			if (cardInfo.alternatePortrait != null && cardInfo.alternatePortrait.texture != null)
			{
				GUILayout.Label(cardInfo.alternatePortrait.texture, options);
			}
			if (cardInfo.pixelPortrait != null && cardInfo.pixelPortrait.texture != null)
			{
				GUILayout.Label(cardInfo.pixelPortrait.texture, options);
			}
		}
	}

	private static void NewCardMod(DeckInfo deckInfo, CardInfo card, int attackAdjustment = 0, int healthAdjustment = 0, 
		Ability ability = 0, Ability negateAbility = 0, 
		int bloodCostAdjustment = 0, int boneCostAdjustment = 0, int energyCostAdjustment = 0, 
		SpecialTriggeredAbility specialAbility = 0, SpecialTriggeredAbility removeSpecialAbility = 0,
		List<GemType> addGemCost = null, bool? gemified = null)
	{
		CardModificationInfo val = new CardModificationInfo();
		val.attackAdjustment = attackAdjustment;
		val.healthAdjustment = healthAdjustment;
		if ((int)ability > 0)
		{
			val.abilities.Add(ability);
		}
		if ((int)negateAbility > 0)
		{
			val.negateAbilities.Add(negateAbility);
		}
		
		val.bloodCostAdjustment = bloodCostAdjustment;
		val.bonesCostAdjustment = boneCostAdjustment;
		val.energyCostAdjustment = energyCostAdjustment;

		if (addGemCost != null)
		{
			val.addGemCost = addGemCost;
		}
		
		if (gemified.HasValue)
		{
			val.gemify = gemified.Value;
		}
		
		if ((int)specialAbility > 0)
		{
			val.specialAbilities.Add(specialAbility);
		}
		if ((int)removeSpecialAbility > 0)
		{
			foreach (CardModificationInfo mod in card.Mods)
			{
				if (mod.specialAbilities.Contains(removeSpecialAbility))
				{
					mod.specialAbilities.Remove(removeSpecialAbility);
				}
			}
		}
		deckInfo.ModifyCard(card, val);
		SaveManager.SaveToFile(false);
	}

	private static void HandleCost(CardInfo currentCard, DeckInfo deckInfo)
	{
		GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
		
		// Blood
		GUILayout.Label("Blood " + currentCard.BloodCost, Array.Empty<GUILayoutOption>());
		if (GUILayout.Button("-", Array.Empty<GUILayoutOption>()))
		{
			NewCardMod(deckInfo, currentCard, 0, 0, 0, 0, (currentCard.BloodCost > 0) ? (-1) : 0);
		}
		if (GUILayout.Button("+", Array.Empty<GUILayoutOption>()))
		{
			NewCardMod(deckInfo, currentCard, 0, 0, 0, 0, 1);
		}
		
		// Bone
		GUILayout.Label("Bone " + currentCard.BonesCost, Array.Empty<GUILayoutOption>());
		if (GUILayout.Button("-", Array.Empty<GUILayoutOption>()))
		{
			NewCardMod(deckInfo, currentCard, 0, 0, 0, 0, 0, (currentCard.BonesCost > 0) ? (-1) : 0);
		}
		if (GUILayout.Button("+", Array.Empty<GUILayoutOption>()))
		{
			NewCardMod(deckInfo, currentCard, 0, 0, 0, 0, 0, 1);
		}
		
		// Energy
		GUILayout.Label("Energy " + currentCard.EnergyCost, Array.Empty<GUILayoutOption>());
		if (GUILayout.Button("-", Array.Empty<GUILayoutOption>()))
		{
			NewCardMod(deckInfo, currentCard, 0, 0, 0, 0, 0, 0, (currentCard.EnergyCost > 0) ? (-1) : 0);
		}
		if (GUILayout.Button("+", Array.Empty<GUILayoutOption>()))
		{
			NewCardMod(deckInfo, currentCard, 0, 0, 0, 0, 0, 0, 1);
		}
		GUILayout.EndHorizontal();
		
		// Gems
		GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
		GUILayout.Label("Gems " + string.Join(",", currentCard.GemsCost), Array.Empty<GUILayoutOption>());
		
		CardModificationInfo gemifiedMod = currentCard.Mods.Find((a) => a.gemify);
		if (gemifiedMod != null)
		{
			if (GUILayout.Button("Ungemify", Array.Empty<GUILayoutOption>()))
			{
				currentCard.Mods.Remove(gemifiedMod);
				deckInfo.UpdateModDictionary();
				SaveManager.SaveToFile(false);
			}
		}
		else
		{
			if (GUILayout.Button("Gemify", Array.Empty<GUILayoutOption>()))
			{
				NewCardMod(deckInfo, currentCard, gemified: true);
			}
		}
		
		List<CardModificationInfo> gemMods = currentCard.Mods.FindAll((a) => a.addGemCost != null);
		foreach (GemType gemType in Enum.GetValues(typeof(GemType)))
		{
			CardModificationInfo gemMod = gemMods.Find((a)=>a.addGemCost.Contains(gemType));
			GUILayout.Label(gemType.ToString());
			if (gemMod != null)
			{
				GUILayout.Label("+");
				
				if (GUILayout.Button("-"))
				{
					currentCard.Mods.Remove(gemMod);
					deckInfo.UpdateModDictionary();
					SaveManager.SaveToFile(false);
				}
			}
			else
			{
				if (GUILayout.Button("+"))
				{
					NewCardMod(deckInfo, currentCard, addGemCost: new List<GemType>() { gemType });
				}
				
				GUILayout.Label("-");
			}
		}
		GUILayout.EndHorizontal();
	}

	private static void HandleStats(CardInfo currentCard, DeckInfo deckInfo)
	{
		GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
		GUILayout.Label("Atk " + currentCard.Attack, Array.Empty<GUILayoutOption>());
		GUILayout.Label("HP " + currentCard.Health, Array.Empty<GUILayoutOption>());
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
		if (GUILayout.Button("-", Array.Empty<GUILayoutOption>()))
		{
			NewCardMod(deckInfo, currentCard, (currentCard.Attack > 0) ? (-1) : 0);
		}
		if (GUILayout.Button("+", Array.Empty<GUILayoutOption>()))
		{
			NewCardMod(deckInfo, currentCard, 1);
		}
		if (GUILayout.Button("-", Array.Empty<GUILayoutOption>()))
		{
			NewCardMod(deckInfo, currentCard, 0, (currentCard.Health > 0) ? (-1) : 0);
		}
		if (GUILayout.Button("+", Array.Empty<GUILayoutOption>()))
		{
			NewCardMod(deckInfo, currentCard, 0, 1);
		}
		GUILayout.EndHorizontal();
	}

	
	private static void ManageAbilities(CardInfo currentCard, DeckInfo deckInfo)
	{
		abilityManagerIndex = GUILayout.Toolbar(abilityManagerIndex, abilityManagementTabs, Array.Empty<GUILayoutOption>());
		if (abilityManagerIndex == 0)
		{
			EditAbilities(currentCard, deckInfo);
		}
		else
		{
			AddAbilities(currentCard, deckInfo);
		}
	}

	private static void EditAbilities(CardInfo currentCard, DeckInfo deckInfo)
	{
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_0214: Unknown result type (might be due to invalid IL or missing references)
		GUILayout.Label("Sigils", Array.Empty<GUILayoutOption>());
		lastCardAbilitySearch = GUILayout.TextField(lastCardAbilitySearch, Array.Empty<GUILayoutOption>());
		if (lastCardAbilitySearch != "")
		{
			List<AbilityManager.FullAbility> abilities = AbilityManager.AllAbilities.FindAll((a)=>currentCard.Abilities.Contains(a.Id));
			if (GetAbilitiesThatContain(lastCardAbilitySearch, out var results, abilities))
			{
				GUILayout.Label("Results Found:", Array.Empty<GUILayoutOption>());
				cardAbilityList = results;
			}
			else
			{
				GUILayout.Label("No Abilities Match", Array.Empty<GUILayoutOption>());
				cardAbilityList = AbilityManager.AllAbilities.FindAll((a)=>currentCard.Abilities.Contains(a.Id));
			}
		}
		else
		{
			GUILayout.Label("Not Searching", Array.Empty<GUILayoutOption>());
			cardAbilityList = AbilityManager.AllAbilities.FindAll((a)=>currentCard.Abilities.Contains(a.Id));
		}
		if (currentCard == null)
		{
			GUILayout.Label("No Card", Array.Empty<GUILayoutOption>());
			return;
		}
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
			if (val == (Object)null)
			{
				if (GUILayout.Button(negateAbility.Info.rulebookName, new GUILayoutOption[2]
				{
					GUILayout.Width((float)overrideSquareWidth),
					GUILayout.Height((float)overrideSquareHeight)
				}))
				{
					NewCardMod(deckInfo, currentCard, 0, 0, 0, negateAbility.Id);
				}
			}
			else if (GUILayout.Button(val, new GUILayoutOption[2]
			         {
				         GUILayout.Width((float)overrideSquareWidth),
				         GUILayout.Height((float)overrideSquareHeight)
			         }))
			{
				NewCardMod(deckInfo, currentCard, 0, 0, 0, negateAbility.Id);
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

	private static bool GetAbilitiesThatContain(string testString, out List<AbilityManager.FullAbility> results, List<AbilityManager.FullAbility> searchingList)
	{
		bool result = false;
		results = new List<AbilityManager.FullAbility>();
		foreach (AbilityManager.FullAbility searching in searchingList)
		{
			AbilityManager.FullAbility current = searching;
			string rulebookName = current.Info?.rulebookName;
			string abilityName = current.Id.ToString();
			if (rulebookName != null && (rulebookName.ToLower().Contains(testString.ToLower())) || abilityName.Contains(testString.ToLower()))
			{
				results.Add(current);
				result = true;
			}
		}
		return result;
	}
	
	private static void AddAbilities(CardInfo currentCard, DeckInfo deckInfo)
	{
		GUILayout.Label("Sigils", Array.Empty<GUILayoutOption>());
		lastAbilitySearch = GUILayout.TextField(lastAbilitySearch, Array.Empty<GUILayoutOption>());
		if (lastAbilitySearch != "")
		{
			if (GetAbilitiesThatContain(lastAbilitySearch, out List<AbilityManager.FullAbility> results, GetAllAbilities()))
			{
				GUILayout.Label("Results Found:", Array.Empty<GUILayoutOption>());
				cardAbilityList = results;
			}
			else
			{
				GUILayout.Label("No Abilities Match", Array.Empty<GUILayoutOption>());
				cardAbilityList = GetAllAbilities();
			}
		}
		else
		{
			GUILayout.Label("Not Searching", Array.Empty<GUILayoutOption>());
			cardAbilityList = GetAllAbilities();
		}
		if (currentCard == (Object)null)
		{
			GUILayout.Label("No Card", Array.Empty<GUILayoutOption>());
			return;
		}
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
			if (val == (Object)null)
			{
				if (GUILayout.Button(ability.Info.rulebookName, GUILayout.Width(overrideSquareWidth), GUILayout.Height(overrideSquareHeight)))
				{
					NewCardMod(deckInfo, currentCard, 0, 0, ability.Id);
				}
			}
			else if (GUILayout.Button(val, GUILayout.Width(overrideSquareWidth), GUILayout.Height(overrideSquareHeight)))
			{
				NewCardMod(deckInfo, currentCard, 0, 0, ability.Id);
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

	private static List<AbilityManager.FullAbility> GetAllAbilities()
	{
		return AbilityManager.AllAbilities;
	}

	private static void EditSpecialAbilities(CardInfo currentCard, DeckInfo deckInfo)
	{
		GUILayout.Label("Abilities", Array.Empty<GUILayoutOption>());
		specialAbilitySelector = GUILayout.Toolbar(specialAbilitySelector, specialAbilitySelectorList, Array.Empty<GUILayoutOption>());
		if (specialAbilitySelector == 0)
		{
			if (currentCard.SpecialAbilities.Count <= 0)
			{
				GUILayout.Label("No Abilities", Array.Empty<GUILayoutOption>());
				return;
			}
			specialAbilityListVector2 = GUILayout.BeginScrollView(specialAbilityListVector2, Array.Empty<GUILayoutOption>());
			foreach (SpecialTriggeredAbility specialAbility2 in currentCard.SpecialAbilities)
			{
				SpecialTriggeredAbility current = specialAbility2;
				if (GUILayout.Button(current.ToString(), Array.Empty<GUILayoutOption>()))
				{
					NewCardMod(deckInfo, currentCard, 0, 0, 0, 0, 0, 0, 0, 0, current);
				}
			}
			GUILayout.EndScrollView();
			return;
		}
		specialAbilityListVector = GUILayout.BeginScrollView(specialAbilityListVector, Array.Empty<GUILayoutOption>());
		foreach (SpecialTriggeredAbilityManager.FullSpecialTriggeredAbility allSpecialAbility in GetAllSpecialAbilities())
		{
			if (GUILayout.Button(GetSpecialAbilityName(allSpecialAbility), Array.Empty<GUILayoutOption>()))
			{
				NewCardMod(deckInfo, currentCard, 0, 0, 0, 0, 0, 0, 0, allSpecialAbility.Id);
			}
		}
		GUILayout.EndScrollView();
	}
	
	public static string GetSpecialAbilityName(SpecialTriggeredAbilityManager.FullSpecialTriggeredAbility ability)
	{
		if (ability.Id <= SpecialTriggeredAbility.NUM_ABILITIES)
		{
			return ability.ToString();
		}

		StatIconManager.FullStatIcon icon = StatIconManager.AllStatIcons.Find((a)=>a.VariableStatBehavior == ability.AbilityBehaviour);
		if (icon != null)
		{
			return icon.Info.rulebookName;
		}

		return null;
	}

	private static ObservableCollection<SpecialTriggeredAbilityManager.FullSpecialTriggeredAbility> GetAllSpecialAbilities()
	{
		return SpecialTriggeredAbilityManager.NewSpecialTriggers;
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

}