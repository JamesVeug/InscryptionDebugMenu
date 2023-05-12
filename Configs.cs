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

	public static ConfigEntry<bool> m_disableDialogue = Bind("General", "DisableDialogue", false, "Should all dialogue be disabled?");
	
	private static ConfigEntry<T> Bind<T>(string section, string key, T defaultValue, string description)
	{
		return Plugin.Instance.Config.Bind(section, key, defaultValue, new ConfigDescription(description, null, Array.Empty<object>()));
	}
}