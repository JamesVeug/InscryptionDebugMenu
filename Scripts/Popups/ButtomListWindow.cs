using UnityEngine;
using UnityEngine.SceneManagement;

namespace DebugMenu.Scripts.Popups;

public class ButtonListPopup : BaseWindow
{
	public override string PopupName => popupNameOverride;
	public override Vector2 Size => new Vector2(200, 500);

	private List<string> buttonNames = new List<string>();
	private string popupNameOverride = "Button List";
	private Action<string> callback;

	public override void OnGUI()
	{
		base.OnGUI();

		for (int i = 0; i < buttonNames.Count; i++)
		{
			string buttonName = buttonNames[i];
			if(Button(buttonName))
			{
				callback(buttonName);
				Plugin.Instance.ToggleWindow<ButtonListPopup>();
			}

			if (i * RowHeight >= Size.y)
			{
				StartNewColumn();
			}
		}
	}

	public static void OnGUI(DrawableGUI gui, string popupName, List<string> names, Action<string> callback)
	{
		if (gui.Button(popupName))
		{
			if (names == null || names.Count == 0)
			{
				names = new List<string>();
			}

			ButtonListPopup buttonListPopup = Plugin.Instance.ToggleWindow<ButtonListPopup>();
			buttonListPopup.popupNameOverride = popupName;
			buttonListPopup.callback = callback;
			buttonListPopup.buttonNames = names;
		}
	}
}