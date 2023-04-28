using DiskCardGame;

namespace DebugMenu.Scripts.Utils;

public static class Helpers
{
	public static SpecialNodeData LastSpecialNodeData;
	
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
}