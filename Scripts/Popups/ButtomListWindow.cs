﻿using DebugMenu.Scripts.Utils;
using DiskCardGame;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DebugMenu.Scripts.Popups;

public class ButtonListPopup : BaseWindow
{
	public override string PopupName => popupNameOverride;
	public override Vector2 Size => new Vector2(630, 600);

	private string header = "";
	private string filterText = "";
	private List<string> buttonNames = new List<string>();
	private List<string> buttonValues = new List<string>();
	private string popupNameOverride = "Button List";
	private Action<int, string, string> callback;
	private Vector2 position;
	private string metaData;

	public override void OnGUI()
	{
		base.OnGUI();
		
		LabelHeader(header);

		Label("Filter", RowHeight / 2);
		filterText = TextField(filterText, RowHeight / 2);

		Label(""); // padding

		int namesCount = buttonNames.Count; // 20
		int rows = Mathf.Max(Mathf.FloorToInt(Size.y / RowHeight) - 1, 1); // 600 / 40 = 15 
		int columns = Mathf.CeilToInt((float)namesCount / rows); // 20 / 15 = 4
		Rect scrollableAreaSize = new Rect(new Vector2(0, 0), new Vector2(columns *  ColumnWidth + (columns - 1) * 10, rows * RowHeight));
		Rect scrollViewSize = new Rect(new Vector2(0, 0), Size - new Vector2(10, 25));
		position = GUI.BeginScrollView(scrollViewSize, position, scrollableAreaSize);
		
		int j = 0;
		for (int i = 0; i < namesCount; i++)
		{
			string buttonName = buttonNames[i];
			string buttonValue = buttonValues[i];
			if (!string.IsNullOrEmpty(filterText))
			{
				if (!buttonName.ContainsText(filterText, false) &&
				    !buttonValue.ContainsText(filterText, false))
				{
					continue;
				}
			}
			
			if (Button(buttonName))
			{
				callback(i, buttonValue, metaData);
				Plugin.Instance.ToggleWindow<ButtonListPopup>();
			}

			j++;
			if (j >= rows)
			{
				StartNewColumn();
				j = 0;
			}
		}
		
		GUI.EndScrollView();
	}
	
	public static void OnGUI(DrawableGUI gui, string buttonText, string headerText, Func<Tuple<List<string>, List<string>>> GetDataCallback, Action<int, string, string> OnChoseButtonCallback, string metaData=null)
	{
		if (gui.Button(buttonText))
		{
			Debug.Log("ButtonListPopup pressed " + buttonText);
			Tuple<List<string>, List<string>> data = GetDataCallback();

			ButtonListPopup buttonListPopup = Plugin.Instance.ToggleWindow<ButtonListPopup>();
			buttonListPopup.position = Vector2.zero;
			buttonListPopup.popupNameOverride = buttonText;
			buttonListPopup.callback = OnChoseButtonCallback;
			buttonListPopup.buttonNames = data.Item1;
			buttonListPopup.buttonValues = data.Item2;
			buttonListPopup.header = headerText;
			buttonListPopup.filterText = "";
			buttonListPopup.metaData = metaData;
		}
	}
}