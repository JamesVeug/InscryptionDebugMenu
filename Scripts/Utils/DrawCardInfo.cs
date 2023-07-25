using DiskCardGame;
using InscryptionAPI.Card;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;
using Object = System.Object;

namespace DebugMenu.Scripts.Utils;

public static class DrawCardInfo
{
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

	public static readonly string EmissionMod = "DebugMenu:EmissionMod";
    public static readonly string PortraitMod = "DebugMenu:PortraitMod";

    public enum Result
	{
		None,
		Removed,
		Altered
	}
	
	public static Result OnGUI(CardInfo val, DeckInfo deckInfo = null)
	{
		if (val == null)
			return Result.None;
		
		GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
		GUILayout.Label("Card Editor (" + val.name + ")", Array.Empty<GUILayoutOption>());
		if (deckInfo != null && deckInfo.Cards.Count > 2 && GUILayout.Button("Remove", Array.Empty<GUILayoutOption>()))
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

		if (SaveManager.SaveFile.IsPart2)
		{
            GUILayout.Button("No emissions in Act 2");
        }
		else
		{
            CardModificationInfo emissionMod = val.Mods.Find(a => a.singletonId == EmissionMod);
            if (emissionMod != null)
            {
                if (GUILayout.Button("Unforce Emission", Array.Empty<GUILayoutOption>()))
                {
                    val.Mods.Remove(emissionMod);
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
					if (deckInfo != null)
					{
						deckInfo?.ModifyCard(val, new() { singletonId = EmissionMod });
						SaveManager.SaveToFile(false);
					}
                    return Result.Altered;
                }
            }
        }

        CardModificationInfo portraitMod = val.Mods.Find(a => a.singletonId == PortraitMod);
        if (portraitMod != null)
        {
            if (GUILayout.Button("Unforce Alt Portrait", Array.Empty<GUILayoutOption>()))
            {
                val.Mods.Remove(portraitMod);
				if (deckInfo != null)
				{
					deckInfo?.UpdateModDictionary();
					SaveManager.SaveToFile(false);
				}
				return Result.Altered;
            }
        }
        else if (SaveManager.SaveFile.IsPart2 ? val.HasPixelAlternatePortrait() : val.HasAlternatePortrait())
        {
            if (GUILayout.Button("Force Alt Portrait", Array.Empty<GUILayoutOption>()))
            {
				if (deckInfo != null)
				{
                    deckInfo.ModifyCard(val, new() { singletonId = PortraitMod });
                    SaveManager.SaveToFile(false);
                }
                return Result.Altered;
            }
        }
		else
		{
			GUILayout.Button("No Alt Portrait");
        }

        HandleCost(val, deckInfo);
		
		GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
		HandlePortrait(val);
		GUILayout.EndHorizontal();

		HandleStats(val, deckInfo);
		selectedTab = GUILayout.Toolbar(selectedTab, sigilsAbilities, Array.Empty<GUILayoutOption>());
		if (val != null)
		{
			if (selectedTab == 0)
				ManageAbilities(val, deckInfo);
			else
				ManageSpecialAbilities(val, deckInfo);
		}

		return Result.None;
	}

	private static void HandlePortrait(CardInfo cardInfo)
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
	private static void NewCardMod(DeckInfo deckInfo, CardInfo card, int attackAdjustment = 0, int healthAdjustment = 0,
		Ability ability = 0, Ability negateAbility = 0,
		int bloodCostAdjustment = 0, int boneCostAdjustment = 0, int energyCostAdjustment = 0,
		SpecialTriggeredAbility specialAbility = 0, SpecialTriggeredAbility removeSpecialAbility = 0,
		List<GemType> addGemCost = null, bool? gemified = null)
	{
		CardModificationInfo val = new();
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
		if (deckInfo != null)
		{
			deckInfo.ModifyCard(card, val);
			SaveManager.SaveToFile(false);
		}
	}
	private static void HandleCost(CardInfo currentCard, DeckInfo deckInfo)
	{
		GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
		
		// Blood
		GUILayout.Label("Blood " + currentCard.BloodCost, Array.Empty<GUILayoutOption>());
		if (GUILayout.Button("-", Array.Empty<GUILayoutOption>()))
			NewCardMod(deckInfo, currentCard, 0, 0, 0, 0, (currentCard.BloodCost > 0) ? (-1) : 0);

		if (GUILayout.Button("+", Array.Empty<GUILayoutOption>()))
			NewCardMod(deckInfo, currentCard, 0, 0, 0, 0, 1);
		
		// Bone
		GUILayout.Label("Bone " + currentCard.BonesCost, Array.Empty<GUILayoutOption>());
		if (GUILayout.Button("-", Array.Empty<GUILayoutOption>()))
			NewCardMod(deckInfo, currentCard, 0, 0, 0, 0, 0, (currentCard.BonesCost > 0) ? (-1) : 0);

		if (GUILayout.Button("+", Array.Empty<GUILayoutOption>()))
			NewCardMod(deckInfo, currentCard, 0, 0, 0, 0, 0, 1);
		
		// Energy
		GUILayout.Label("Energy " + currentCard.EnergyCost, Array.Empty<GUILayoutOption>());
		if (GUILayout.Button("-", Array.Empty<GUILayoutOption>()))
			NewCardMod(deckInfo, currentCard, 0, 0, 0, 0, 0, 0, (currentCard.EnergyCost > 0) ? (-1) : 0);

		if (GUILayout.Button("+", Array.Empty<GUILayoutOption>()))
			NewCardMod(deckInfo, currentCard, 0, 0, 0, 0, 0, 0, 1);

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
				if (deckInfo != null)
				{
					deckInfo.UpdateModDictionary();
					SaveManager.SaveToFile(false);
				}
			}
		}
		else if (GUILayout.Button("Gemify", Array.Empty<GUILayoutOption>()))
        {
            NewCardMod(deckInfo, currentCard, gemified: true);
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
					if (deckInfo != null)
					{
						deckInfo.UpdateModDictionary();
						SaveManager.SaveToFile(false);
					}
				}
			}
			else
			{
				if (GUILayout.Button("+"))
					NewCardMod(deckInfo, currentCard, addGemCost: new List<GemType>() { gemType });
				
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
		GUILayout.Label("Sigils", Array.Empty<GUILayoutOption>());
		
		if (currentCard == null)
		{
			GUILayout.Label("No Card", Array.Empty<GUILayoutOption>());
			return;
		}
		
		List<AbilityManager.FullAbility> abilities = AbilityManager.AllAbilities.FindAll((a)=>currentCard.Abilities.Contains(a.Id));
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
	
	private static void AddAbilities(CardInfo currentCard, DeckInfo deckInfo)
	{
		GUILayout.Label("Sigils", Array.Empty<GUILayoutOption>());

		if (currentCard == (Object)null)
		{
			GUILayout.Label("No Card", Array.Empty<GUILayoutOption>());
			return;
		}
		
		var cardAbilityList = GetAbilitiesThatContain(GetAllAbilities());
		
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

	private static void ManageSpecialAbilities(CardInfo currentCard, DeckInfo deckInfo)
	{
		specialAbilitySelector = GUILayout.Toolbar(specialAbilitySelector, specialAbilitySelectorList);
		if (specialAbilitySelector == 0)
		{
			RemoveSpecialAbility(currentCard, deckInfo);
		}
		else
		{
			AddSpecialAbility(currentCard, deckInfo);
		}
	}

	private static void AddSpecialAbility(CardInfo currentCard, DeckInfo deckInfo)
	{
		GUILayout.Label("Special Abilities", Array.Empty<GUILayoutOption>());
		
		var abilities = SpecialTriggeredAbilityManager.AllSpecialTriggers;
		var result = GetSpecialAbilitiesThatContain(abilities);
		
		specialAbilityListVector = GUILayout.BeginScrollView(specialAbilityListVector);
		foreach (SpecialTriggeredAbilityManager.FullSpecialTriggeredAbility allSpecialAbility in result)
		{
			if (GUILayout.Button(allSpecialAbility.AbilityName, Array.Empty<GUILayoutOption>()))
			{
				NewCardMod(deckInfo, currentCard, specialAbility:allSpecialAbility.Id);
			}
		}
		GUILayout.EndScrollView();
	}

	private static void RemoveSpecialAbility(CardInfo currentCard, DeckInfo deckInfo)
	{
		GUILayout.Label("Special Abilities", Array.Empty<GUILayoutOption>());
		
		var abilities = SpecialTriggeredAbilityManager.AllSpecialTriggers.FindAll((a)=>currentCard.SpecialAbilities.Contains(a.Id));
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
				NewCardMod(deckInfo, currentCard, removeSpecialAbility:current.Id);
			}
		}
		GUILayout.EndScrollView();
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