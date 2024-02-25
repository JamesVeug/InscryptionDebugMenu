using BepInEx.Configuration;
using DebugMenu.Scripts.Utils;
using DiskCardGame;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DebugMenu.Scripts.Popups;

public class ConfigPopup : BaseWindow
{
	public override string PopupName => "Configs";
	public override Vector2 Size => new(630, 600);

	private string header = "";
	private string filterText = "";
	private List<string> buttonNames = new();
	private List<string> buttonValues = new();
	private Action<int, string, string> callback;
	private Vector2 position;
	private string disableMatch;
	private PluginConfig Config;

	public override void OnGUI()
	{
		base.OnGUI();
		
		int namesCount = buttonNames.Count; // 20
		int rowsPerColumn = Mathf.Max(Mathf.FloorToInt(Size.y / RowHeight) - 2, 1); // 600 / 40 = 15 
		int columns = Mathf.CeilToInt((float)namesCount / rowsPerColumn) + 1; // 20 / 15 = 4
		Rect scrollableAreaSize = new(new Vector2(0, 0), new Vector2(columns *  ColumnWidth + (columns - 1) * 10, rowsPerColumn * RowHeight));
		Rect scrollViewSize = new(new Vector2(0, 0), Size - new Vector2(10, 25));
		position = GUI.BeginScrollView(scrollViewSize, position, scrollableAreaSize);
		
		LabelHeader(header);

		Label("Filter", new(0, RowHeight / 2));
		filterText = TextField(filterText, new(0, RowHeight / 2));
		
		if (Button("Save"))
		{
			Config.Config.Save();
		}
		
		if (Button("Reload"))
		{
			Config.Config.Reload();
		}
		
		if (Button("Clear"))
		{
			Config.Config.Clear();
		}
        
		StartNewColumn();

		int row = 0;
		for (int i = 0; i < Config.Config.ConfigDefinitions.Count; i++)
		{
			ConfigDefinition definition = Config.Config.ConfigDefinitions[i];
			if(!IsFiltered(definition.Key))
				continue;

			if (Button("X"))
			{
				Config.Config.Remove(definition);
			}
			
			Label(definition.Key);
			Label(definition.Section);

			DrawValue(definition, Config.Config[definition]);

			row++;
			if (row >= rowsPerColumn)
			{
				StartNewColumn();
				row = 0;
			}
		}
		
		GUI.EndScrollView();
	}

	private void DrawValue(ConfigDefinition key, ConfigEntryBase value)
	{
		if (value.SettingType == typeof(int))
		{
			int currentValue = (int)value.BoxedValue;
			int newValue = IntField(currentValue);
			if (currentValue != newValue)
			{
				value.BoxedValue = newValue;
			}
		}
		else if (value.SettingType == typeof(bool))
		{
			int currentValue = (int)value.BoxedValue;
			int newValue = IntField(currentValue);
			if (currentValue != newValue)
			{
				value.BoxedValue = newValue;
			}
		}
		else if (value.SettingType == typeof(float))
		{
			float currentValue = (float)value.BoxedValue;
			float newValue = FloatField(currentValue);
			if (Math.Abs(currentValue - newValue) > 0.00001f)
			{
				value.BoxedValue = newValue;
			}
		}
		else if (value.SettingType == typeof(string))
		{
			string currentValue = (string)value.BoxedValue;
			string newValue = TextField(currentValue);
			if (currentValue != newValue)
			{
				value.BoxedValue = newValue;
			}
		}
		else if (value.SettingType.IsEnum)
		{
			
		}
		else
		{
			Label("Unhandled type: " + value.SettingType.FullName);
		}
	}

	public virtual bool IsFiltered(string buttonName)
	{
		return true;
	}

	public static void Show(PluginConfig config)
	{
		ConfigPopup popup = Plugin.Instance.ToggleWindow<ConfigPopup>();
		popup.Config = config;
		
		config.Config.Keys
	}
}