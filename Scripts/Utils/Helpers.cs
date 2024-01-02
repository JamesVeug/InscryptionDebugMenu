using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using BepInEx;
using BepInEx.Bootstrap;
using DebugMenu.Scripts.Popups;
using DiskCardGame;
using GBC;
using InscryptionAPI.Card;
using InscryptionAPI.Encounters;
using UnityEngine;

namespace DebugMenu.Scripts.Utils;

public static partial class Helpers
{
	public enum Acts
	{
		Unknown,
		Act1,
		Act2,
		Act3,
		GrimoraAct,
		MagnificusAct,
	}
	
	public static SpecialNodeData LastSpecialNodeData;
	
	private static Dictionary<string, string> m_itemNameToRulebookName = null;
	private static Action<EncounterBlueprintData> m_selectedBlueprint = null;
	private static Action<Opponent.Type> m_selectedOpponentType = null;
	private static Action<Tribe> m_selectedTribe;
	private static Action<Ability> m_selectedAbility;

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
		List<string> names = new(list.Count);
		List<string> values = new(list.Count);
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
		ButtonListPopup.OnGUI(window, "Select Opponent", "Select Opponent", GetAllOpponents, OnChoseOpponentButtonCallback, Opponent.Type.NUM_TYPES.ToString());
	}

	public static void OnChoseOpponentButtonCallback(int chosenIndex, string chosenValue, string metaData)
	{
		List<OpponentManager.FullOpponent> list = new(OpponentManager.AllOpponents);
		if (chosenIndex < 0 || chosenIndex >= list.Count)
			return;

		m_selectedOpponentType(list[chosenIndex].Id);
	}

	private static Tuple<List<string>, List<string>> GetAllOpponents()
	{
		List<OpponentManager.FullOpponent> list = new(OpponentManager.AllOpponents);
		List<string> names = new(list.Count);
		List<string> values = new(list.Count);
		for (int i = 0; i < list.Count; i++)
		{
            string name;
            if (list[i].Opponent != null && int.TryParse(list[i].Id.ToString(), out _))
			{
				name = list[i].Opponent.Name.Replace("Opponent", "") + $"\n({list[i].Id})";
			}
			else
			{
				name = list[i].Id.ToString();
			}
			names.Add(name);
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
		List<Tribe> tribes = new();
		tribes.AddRange(Enum.GetValues(typeof(Tribe)).Cast<Tribe>());
		tribes.AddRange(TribeManager.NewTribes.Select((a)=>a.tribe));

		return tribes;
	}

	private static Tuple<List<string>, List<string>> GetAllTribes()
	{
		List<Tribe> list = AllTribes();
		List<string> names = new(list.Count);
		List<string> values = new(list.Count);
		foreach (Tribe tribe in list)
		{
			names.Add(tribe.ToString());
			values.Add(tribe.ToString());
		}

		return new Tuple<List<string>, List<string>>(names, values);
	}

	public static string GetTribeName(Tribe tribe)
	{
		if(tribe >= Tribe.None && tribe <= Tribe.NUM_TRIBES)
			return tribe.ToString();

		foreach (TribeManager.TribeInfo info in TribeManager.NewTribes)
		{
			if (info.tribe == tribe)
			{
				return info.name;
			}
		}

		return tribe.ToString();
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
		List<string> names = new(list.Count);
		List<string> values = new(list.Count);
		foreach (AbilityManager.FullAbility ability in list)
		{
			names.Add(ability.Info.rulebookName);
			values.Add(ability.Id.ToString());
		}

		return new Tuple<List<string>, List<string>>(names, values);
	}

	public static string DumpAllInfoAsJSONUsingReflection(object o)
	{
		if (o == null)
		{
			return "null";
		}

		Dictionary<string, string> data = new();

		Type type = o.GetType();
		foreach (FieldInfo field in type.GetFields())
		{
			data[field.Name] = SerializeData(field.GetValue(o));
		}
		foreach (PropertyInfo property in type.GetProperties())
		{
			if (property.CanRead)
			{
				data[property.Name] = SerializeData(property.GetValue(o));
			}
		}

		return SaveManager.ToJSON(data);
	}

	private static string SerializeData(object o)
	{
		if (o == null)
		{
			return "null";
		}
		
		// if o is of type primitive then return the value
		if (o.GetType().IsPrimitive || o is string)
		{
			return o.ToString();
		}
		
		return SaveManager.ToJSON(o);
	}
	
	public static object GetDefaultValue(Type type)
	{
		// Validate parameters.
		if (type == null) throw new ArgumentNullException("type");

		// We want an Func<object> which returns the default.
		// Create that expression here.
		Expression<Func<object>> e = Expression.Lambda<Func<object>>(
			// Have to convert to object.
			Expression.Convert(
				// The default value, always get what the *code* tells us.
				Expression.Default(type), typeof(object)
			)
		);

		// Compile and return the value.
		return e.Compile()();
	}

	public static Acts GetCurrentSavedAct()
	{
        // Leshy
        if (SaveManager.SaveFile.IsPart1 && GameFlowManager.m_Instance)
			return Acts.Act1;

        // GBC
        if (SaveManager.SaveFile.IsPart2)
			return Acts.Act2;

        // PO3
        if (SaveManager.SaveFile.IsPart3)
			return Acts.Act3;

        // Grimora
        if (SaveManager.SaveFile.IsGrimora)
			return Acts.GrimoraAct;

        // Magnificus
        if (SaveManager.SaveFile.IsMagnificus)
			return Acts.MagnificusAct;

        // Main menu/transitional screens
        return Acts.Unknown;
	}

	public static DeckInfo CurrentDeck()
	{
		switch (GetCurrentSavedAct())
		{
			case Acts.Act2:
				return SaveData.Data.deck;

			case Acts.GrimoraAct:
				if (GrimoraModHelpers.GrimoraModIsActive())
					return GrimoraModHelpers.GetRunState().playerDeck;
				break;

			case Acts.Unknown:
				return null;
        }

		return SaveManager.SaveFile.CurrentDeck;
	}
	
	public static string ToLiteral(string input) 
	{
		StringBuilder literal = new(input.Length + 2);
		literal.Append("\"");
		foreach (var c in input) {
			switch (c) {
				case '\"': literal.Append("\\\""); break;
				case '\\': literal.Append(@"\\"); break;
				case '\0': literal.Append(@"\0"); break;
				case '\a': literal.Append(@"\a"); break;
				case '\b': literal.Append(@"\b"); break;
				case '\f': literal.Append(@"\f"); break;
				case '\n': literal.Append(@"\n"); break;
				case '\r': literal.Append(@"\r"); break;
				case '\t': literal.Append(@"\t"); break;
				case '\v': literal.Append(@"\v"); break;
				default:
					// ASCII printable character
					if (c >= 0x20 && c <= 0x7e) {
						literal.Append(c);
						// As UTF16 escaped character
					} else {
						literal.Append(@"\u");
						literal.Append(((int)c).ToString("x4"));
					}
					break;
			}
		}
		literal.Append("\"");
		return literal.ToString();
	}

    public static GUIStyle DisabledButtonStyle()
    {
        GUIStyle style = new(GUI.skin.button)
        {
            fontStyle = FontStyle.Bold,
            wordWrap = true
        };
        style.normal.background = style.active.background;
        style.hover.background = style.active.background;
        style.onNormal.background = style.active.background;
        style.onHover.background = style.active.background;
        style.onActive.background = style.active.background;
        style.onFocused.background = style.active.background;
        style.normal.textColor = Color.black;
        return style;
    }
    public static GUIStyle HeaderLabelStyle()
    {
        GUIStyle style = new(GUI.skin.label)
        {
            fontSize = 17,
            fontStyle = FontStyle.Bold
        };
        return style;
    }
    public static GUIStyle HeaderLabelStyleRight()
    {
        GUIStyle style = new(GUI.skin.label)
        {
            fontSize = 17,
            fontStyle = FontStyle.Bold,
			alignment = TextAnchor.MiddleRight
        };
        return style;
    }
}

public static class KeyCodeExtensions
{
	public static string Serialize(this KeyCode keyCode)
	{
		return keyCode.ToString();
	}
	
	public static string Serialize(this IEnumerable<KeyCode> keyCode, string separator = ",", bool includeUnassigned = false)
	{
		string serialized = "";
		if (keyCode != null)
		{
			serialized = string.Join(separator, keyCode.Select((a)=>a.Serialize()));
		}

		if (includeUnassigned && string.IsNullOrEmpty(separator))
		{
			serialized = "Unassigned";
		}
		return serialized;
	}
}