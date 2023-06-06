using DebugMenu.Scripts.Utils;
using UnityEngine;

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

        int namesCount = buttonNames.Count; // 20
        int rows = Mathf.Max(Mathf.FloorToInt(Size.y / RowHeight) - 2, 1); // 600 / 40 = 15 
        int columns = Mathf.CeilToInt((float)namesCount / rows) + 1; // 20 / 15 = 4
        Rect scrollableAreaSize = new Rect(new Vector2(0, 0), new Vector2(columns * ColumnWidth + (columns - 1) * 10, rows * RowHeight));
        Rect scrollViewSize = new Rect(new Vector2(0, 0), Size - new Vector2(10, 25));
        position = GUI.BeginScrollView(scrollViewSize, position, scrollableAreaSize);

        LabelHeader(header);

        Label("Filter", new(0, RowHeight / 2));
        filterText = TextField(filterText, new(0, RowHeight / 2));

        DrawExtraTools();

        StartNewColumn();

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

            if (!IsFiltered(buttonName, buttonValue))
                continue;

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

    public virtual bool IsFiltered(string buttonName, string buttonValue)
    {
        return true;
    }

    public virtual void DrawExtraTools()
    {

    }

    public static bool OnGUI(DrawableGUI gui, string buttonText, string headerText, Func<Tuple<List<string>, List<string>>> GetDataCallback, Action<int, string, string> OnChoseButtonCallback, string metaData = null)
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
            return true;
        }

        return false;
    }
}