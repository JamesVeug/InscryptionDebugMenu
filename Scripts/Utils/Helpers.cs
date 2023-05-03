using System.Reflection;
using DebugMenu.Scripts.Act1;
using DebugMenu.Scripts.Popups;
using DebugMenu.Scripts.Sequences;
using DiskCardGame;
using InscryptionAPI.Card;
using InscryptionAPI.Encounters;

namespace DebugMenu.Scripts.Utils;

public static class Helpers
{
	public readonly static List<BaseTriggerSequences> Sequences = null;
	
	public static SpecialNodeData LastSpecialNodeData;
	
	private static Dictionary<string, string> m_itemNameToRulebookName = null;
	private static Action<EncounterBlueprintData> m_selectedBlueprint = null;
	private static Action<Opponent.Type> m_selectedOpponentType = null;
	private static Action<Tribe> m_selectedTribe;
	private static Action<Ability> m_selectedAbility;

	static Helpers()
	{
		Sequences = new List<BaseTriggerSequences>();
		
		// get all types that override ForceTriggerSequences
		foreach (Type type in Assembly.GetAssembly(typeof(BaseTriggerSequences)).GetTypes())
		{
			if (type.IsAbstract)
			{
				continue;
			}
			if (type.IsSubclassOf(typeof(BaseTriggerSequences)) && type != typeof(StubSequence))
			{
				BaseTriggerSequences sequence = (BaseTriggerSequences)Activator.CreateInstance(type);
				Sequences.Add(sequence);
			}
		}
		
		// get all types that override SpecialNodeData
		foreach (Type type in Assembly.GetAssembly(typeof(NodeData)).GetTypes())
		{
			if (type.IsAbstract)
			{
				continue;
			}
			
			if (type.IsSubclassOf(typeof(NodeData)))
			{
				bool overrideFound = false;
				foreach (BaseTriggerSequences sequence in Sequences)
				{
					if (sequence is not SimpleTriggerSequences simpleTriggerSequences)
					{
						continue;
					}
					
					if (simpleTriggerSequences.NodeDataType == type)
					{
						overrideFound = true;
						break;
					}
				}

				if (!overrideFound)
				{
					StubSequence sequence = new StubSequence();
					sequence.type = type;
					sequence.gameState = type.IsAssignableFrom(typeof(CardBattleNodeData)) ? GameState.CardBattle : GameState.SpecialCardSequence;
					Sequences.Add(sequence);
				}
			}
		}
		
		Sequences.Sort(static (a, b) =>
		{
			return String.Compare(a.ButtonName, b.ButtonName, StringComparison.Ordinal);
		});
	}

	public static bool ContainsText(this string text, string substring, bool caseSensitive = true)
	{
		if (string.IsNullOrEmpty(text))
		{
			if (string.IsNullOrEmpty(substring))
			{
				// null.ContainsText(null)
				// "".ContainsText("")
				return true;
			}
			
			// null.ContainsText("Hello)
			// "".ContainsText("Hello")
			return false;
		}
		else if (string.IsNullOrEmpty(substring))
		{
			// "Hello".ContainsText(null)
			// "Hello".ContainsText("")
			return false;
		}

		return text.IndexOf(substring, caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase) >= 0;
	}

	public static string GetConsumableByName(string consumableName)
	{
		if (string.IsNullOrEmpty(consumableName))
		{
			return null;
		}
		
		if (m_itemNameToRulebookName == null)
		{
			List<ConsumableItemData> list = ItemsUtil.AllConsumables;
			m_itemNameToRulebookName = new Dictionary<string, string>(list.Count);
			for (int i = 0; i < list.Count; i++)
			{
				ConsumableItemData data = list[i];
				m_itemNameToRulebookName[data.name] = data.rulebookName;
			}
			Plugin.Log.LogInfo("[GetConsumableByName] Loaded all items " + m_itemNameToRulebookName.Count);
		}
		
		if (m_itemNameToRulebookName.TryGetValue(consumableName, out string rulebookName))
		{
			return rulebookName;
		}

		ConsumableItemData itemData = ItemsUtil.GetConsumableByName(consumableName);
		if (itemData != null)
		{
			m_itemNameToRulebookName[consumableName] = itemData.rulebookName;
			return itemData.rulebookName;
		}

		Plugin.Log.LogInfo("[GetConsumableByName] Couldn't find name of item " + consumableName);
		return null;
	}
	
	public static IEnumerable<Type> FindDerivedTypes(Assembly assembly, Type baseType)
	{
		return assembly.GetTypes().Where(t => t != baseType && !t.IsAbstract && baseType.IsAssignableFrom(t));
	}
	
	public static void DrawBlueprintGUI(DrawableGUI window, Action<EncounterBlueprintData> OnBlueprintSelected)
	{
		m_selectedBlueprint = OnBlueprintSelected;
		ButtonListPopup.OnGUI(window, "Select Blueprint", "Select Blueprint", GetAllBlueprints, OnChoseBlueprintButtonCallback);
	}

	public static void OnChoseBlueprintButtonCallback(int chosenIndex, string chosenValue, string metaData)
	{
		List<EncounterBlueprintData> list = EncounterManager.AllEncountersCopy;
		if (chosenIndex < 0 || chosenIndex >= list.Count)
		{
			return;
		}

		m_selectedBlueprint(list[chosenIndex]);
	}

	private static Tuple<List<string>, List<string>> GetAllBlueprints()
	{
		List<EncounterBlueprintData> list = EncounterManager.AllEncountersCopy;
		List<string> names = new List<string>(list.Count);
		List<string> values = new List<string>(list.Count);
		for (int i = 0; i < list.Count; i++)
		{
			names.Add(list[i].name);
			values.Add(list[i].name);
		}

		return new Tuple<List<string>, List<string>>(names, values);
	}
	
	public static void DrawOpponentsGUI(DrawableGUI window, Action<Opponent.Type> OnOpponentSelected)
	{
		m_selectedOpponentType = OnOpponentSelected;
		ButtonListPopup.OnGUI(window, "Select Opponent", "Select Opponent", GetAllOpponents, OnChoseOpponentButtonCallback);
	}

	public static void OnChoseOpponentButtonCallback(int chosenIndex, string chosenValue, string metaData)
	{
		List<OpponentManager.FullOpponent> list = OpponentManager.AllOpponents;
		if (chosenIndex < 0 || chosenIndex >= list.Count)
		{
			return;
		}

		m_selectedOpponentType(list[chosenIndex].Id);
	}

	private static Tuple<List<string>, List<string>> GetAllOpponents()
	{
		List<OpponentManager.FullOpponent> list = OpponentManager.AllOpponents;
		List<string> names = new List<string>(list.Count);
		List<string> values = new List<string>(list.Count);
		for (int i = 0; i < list.Count; i++)
		{
			names.Add(list[i].Id.ToString());
			values.Add(list[i].Id.ToString());
		}

		return new Tuple<List<string>, List<string>>(names, values);
	}

	public static void DrawTribesGUI(TriggerCardBattleSequenceWindow window, Action<Tribe> callback)
	{
		m_selectedTribe = callback;
		ButtonListPopup.OnGUI(window, "Select Tribe", "Select Tribe", GetAllTribes, OnChoseTribeButtonCallback);
	}

	private static void OnChoseTribeButtonCallback(int chosenIndex, string chosenValue, string metaData)
	{
		List<Tribe> list = AllTribes();
		if (chosenIndex < 0 || chosenIndex >= list.Count)
		{
			return;
		}

		m_selectedTribe(list[chosenIndex]);
	}

	public static List<Tribe> AllTribes()
	{
		List<Tribe> tribes = new List<Tribe>();
		tribes.AddRange(Enum.GetValues(typeof(Tribe)).Cast<Tribe>());
		tribes.AddRange(TribeManager.tribes.Select((a)=>a.tribe));

		return tribes;
	}

	private static Tuple<List<string>, List<string>> GetAllTribes()
	{
		List<Tribe> list = AllTribes();
		List<string> names = new List<string>(list.Count);
		List<string> values = new List<string>(list.Count);
		foreach (Tribe tribe in list)
		{
			names.Add(tribe.ToString());
			values.Add(tribe.ToString());
		}

		return new Tuple<List<string>, List<string>>(names, values);
	}

	public static void DrawAbilitysGUI(TriggerCardBattleSequenceWindow window, Action<Ability> callback)
	{
		m_selectedAbility = callback;
		ButtonListPopup.OnGUI(window, "Select Ability", "Select Ability", GetAllAbilitys, OnChoseAbilityButtonCallback);
	}

	private static void OnChoseAbilityButtonCallback(int chosenIndex, string chosenValue, string metaData)
	{
		List<AbilityManager.FullAbility> list = AbilityManager.AllAbilities;
		if (chosenIndex < 0 || chosenIndex >= list.Count)
		{
			return;
		}

		m_selectedAbility(list[chosenIndex].Id);
	}

	private static Tuple<List<string>, List<string>> GetAllAbilitys()
	{
		List<AbilityManager.FullAbility> list = AbilityManager.AllAbilities;
		List<string> names = new List<string>(list.Count);
		List<string> values = new List<string>(list.Count);
		foreach (AbilityManager.FullAbility ability in list)
		{
			names.Add(ability.Info.rulebookName);
			values.Add(ability.Id.ToString());
		}

		return new Tuple<List<string>, List<string>>(names, values);
	}
}