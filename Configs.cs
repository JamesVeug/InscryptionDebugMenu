using BepInEx.Configuration;

namespace DebugMenu;

public static class Configs
{
    public enum WindowSizes
    {
        OneQuarter,
        Half,
        ThreeQuarters,
        Default,
        OneAndAQuarter,
        OneAndAHalf,
        OneAndThreeQuarters,
        Double
    }
	public static bool DisableAllInput
	{
		get => m_disableDialogue.Value;
		set
		{
			m_disableDialogue.Value = value;
			Plugin.Instance.Config.Save();
		}
	}
    public static bool DisablePlayerDamage
    {
        get => m_disablePlayerDamage.Value;
        set
        {
            m_disablePlayerDamage.Value = value;
            Plugin.Instance.Config.Save();
        }
    }
    public static bool DisableOpponentDamage
    {
        get => m_disableOpponentDamage.Value;
        set
        {
            m_disableOpponentDamage.Value = value;
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

    public static bool VerticalItems
    {
        get => m_verticalItems.Value;
        set
        {
            m_verticalItems.Value = value;
            Plugin.Instance.Config.Save();
        }
    }
    public static WindowSizes WindowSize
    {
        get => m_windowSize.Value;
        set
        {
            m_windowSize.Value = value;
            Plugin.Instance.Config.Save();
        }
    }

    public static bool InstantScales
    {
        get => m_instantScales.Value;
        set
        {
            m_instantScales.Value = value;
            Plugin.Instance.Config.Save();
        }
    }

    public static ConfigEntry<bool> m_disableDialogue = Bind("General", "Disable Dialogue", false, "Should all dialogue be disabled?");
    public static ConfigEntry<bool> m_disablePlayerDamage = Bind("General", "Disable Player Damage", false, "Should the player be immune to direct damage?");
    public static ConfigEntry<bool> m_disableOpponentDamage = Bind("General", "Disable Opponent Damage", false, "Should the opponent be immune to direct damage?");
    public static ConfigEntry<WindowSizes> m_windowSize = Bind("General", "Window Scale", WindowSizes.Default, "How big the menu windows should be.");
    public static ConfigEntry<string> m_hotkeys = Bind("General", "Hotkeys",
        "F1:AllAct SetTimeScale:1," +
        "LeftControl+F1:AllAct SetTimeScale:10," +
        "LeftShift+F1:AllAct SetTimeScale:0.1," +
        "BackQuote:Debug Menu Show/Hide," +
        "F5:AllAct Reload," +
        "F9:AllAct Restart," +
        "F4:Map ToggleSkipNextNode," +
        "F3:Map ToggleAllNodes," +
        "F2:Battle DrawCard," +
        "LeftShift+F2:Battle DrawSideDeck",
        "Quick access buttons to control the debug menu. Use the in-game menu to change them");
	public static ConfigEntry<bool> m_showDebugMenu = Bind("General", "Show Debug Menu", true, "Should the in-game debug menu window be shown?");
    public static ConfigEntry<bool> m_verticalItems = Bind("General", "Display Items Vertically", false, "Should the items be displayed on vertically?");
    public static ConfigEntry<bool> m_instantScales = Bind("General", "Instant Scales", false, "Makes the debug menu deal scale damage instantly instead of one at a time.");


    private static ConfigEntry<T> Bind<T>(string section, string key, T defaultValue, string description)
	{
		return Plugin.Instance.Config.Bind(section, key, defaultValue, new ConfigDescription(description, null, Array.Empty<object>()));
	}
}