using DiskCardGame;

namespace DebugMenu.Scripts.Utils;

public static class Helpers
{
	public static SpecialNodeData LastSpecialNodeData;
	
	private static Dictionary<string, string> m_itemNameToRulebookName = null;
	
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
}