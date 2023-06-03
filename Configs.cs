using BepInEx.Configuration;

namespace DebugMenu;

public static class Configs
{
	public static bool DisableAllInput
	{
		get => m_disableDialogue.Value;
		set
		{
			m_disableDialogue.Value = value;
			Plugin.Instance.Config.Save();
		}
	}
	
	public static string Hotkeys
	{
		get => m_hotkeys.Value;
		set
		{
			m_hotkeys.Value = value;
			Plugin.Instance.Config.Save();
		}
	}
	
	public static bool ShowDebugMenu
	{
		get => m_showDebugMenu.Value;
		set
		{
			m_showDebugMenu.Value = value;
			Plugin.Instance.Config.Save();
		}
	}

	public static ConfigEntry<bool> m_disableDialogue = Bind("General", "Disable Dialogue", false, "Should all dialogue be disabled?");
	public static ConfigEntry<string> m_hotkeys = Bind("General", "Hotkeys", "", "Quick access buttons to control the debug menu. Use in-game menu to change them");
	public static ConfigEntry<bool> m_showDebugMenu = Bind("General", "Show Debug Menu", true, "Should the in-game debug menu window be shown?");
	
	private static ConfigEntry<T> Bind<T>(string section, string key, T defaultValue, string description)
	{
		return Plugin.Instance.Config.Bind(section, key, defaultValue, new ConfigDescription(description, null, Array.Empty<object>()));
	}
}