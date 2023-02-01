using DiskCardGame;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DebugMenu.Scripts.Popups;

public class ButtonListPopup : BaseWindow
{
	public override string PopupName => popupNameOverride;
	public override Vector2 Size => new Vector2(600, 600);

	private List<string> buttonNames = new List<string>();
	private string popupNameOverride = "Button List";
	private Action<int> callback;
	private Vector2 position;

	public override void OnGUI()
	{
		base.OnGUI();

		int namesCount = buttonNames.Count; // 20
		int height = Mathf.Max(Mathf.FloorToInt(Size.y / RowHeight) - 1, 1); // 600 / 40 = 15 
		int width = Mathf.CeilToInt((float)namesCount / height); // 20 / 15 = 4
		Rect rect = new Rect(new Vector2(0, 0), new Vector2(width *  ColumnWidth, Size.y));
		Rect view = new Rect(new Vector2(0, 0), Size - new Vector2(50, 100));
		position = GUI.BeginScrollView(rect, position, view);
		
		int j = 0;
		for (int i = 0; i < namesCount; i++)
		{
			string buttonName = buttonNames[i];
			if(Button(buttonName))
			{
				callback(i);
				Plugin.Instance.ToggleWindow<ButtonListPopup>();
			}

			j++;
			if (j * RowHeight >= Size.y)
			{
				StartNewColumn();
				j = 0;
			}
		}
		
		GUI.EndScrollView();
	}

	public static void OnGUI(DrawableGUI gui, string popupName, List<string> names, Action<int> callback)
	{
		if (gui.Button(popupName))
		{
			Debug.Log("ButtonListPopup pressed " + popupName);
			if (names == null || names.Count == 0)
			{
				names = new List<string>();
			}

			ButtonListPopup buttonListPopup = Plugin.Instance.ToggleWindow<ButtonListPopup>();
			buttonListPopup.position = Vector2.zero;
			buttonListPopup.popupNameOverride = popupName;
			buttonListPopup.callback = callback;
			buttonListPopup.buttonNames = names;
		}
	}
}