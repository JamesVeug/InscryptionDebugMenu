using DebugMenu.Scripts.Utils;
using UnityEngine;

namespace DebugMenu.Scripts.Popups;

public class ButtonListPopup : BaseWindow
{
    public override string PopupName => popupNameOverride;
    public override Vector2 Size => new(630, 600);

    private string header = "";
    private string filterText = "";
    public List<string> buttonNames = new();
    public List<string> buttonValues = new();
    private string popupNameOverride = "Button List";
    private Action<int, string, List<string>> callback;

    private Vector2 position;
    private List<string> disableMatch;

    public override void OnGUI()
    {
        base.OnGUI();

        int namesCount = buttonNames.Count; // 20
        int rows = Mathf.Max(Mathf.FloorToInt(Size.y / RowHeight) - 2, 1); // 600 / 40 = 15 
        int columns = Mathf.CeilToInt((float)namesCount / rows) + 1; // 20 / 15 = 4
        Rect scrollableAreaSize = new(new Vector2(0, 0), new Vector2(columns * ColumnWidth + (columns - 1) * 10, rows * RowHeight));
        Rect scrollViewSize = new(new Vector2(0, 0), Size - new Vector2(10, 25));
        position = GUI.BeginScrollView(scrollViewSize, position, scrollableAreaSize);

        LabelHeader(header);

        Label("Filter", new(0, RowHeight / 2));
        filterText = TextField(filterText, new(0, RowHeight / 2));

        DrawExtraTools();
        StartNewColumn();

        int j = 0;
        for (int i = 0; i < namesCount; i++)
        {
            string buttonValue = buttonValues[i];
            string buttonName = ModifyButtonName(buttonNames[i], buttonValue);
            if (!string.IsNullOrEmpty(filterText))
            {
                if (!buttonName.ContainsText(filterText, false) && !buttonValue.ContainsText(filterText, false))
                    continue;
            }

            if (!IsWhitelisted(buttonName, buttonValue))
                continue;

            if (Button(buttonName, disabled: () => new() { Disabled = disableMatch.Contains(buttonValue) }))
            {
                callback(i, buttonValue, disableMatch);
                Plugin.Instance.ToggleWindow(this.GetType()); // close window
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

    public virtual bool IsWhitelisted(string buttonName, string buttonValue)
    {
        return true;
    }

    public virtual void DrawExtraTools()
    {

    }
    public virtual string ModifyButtonName(string name, string value)
    {
        return name;
    }

    public static bool OnGUI<T>(DrawableGUI gui, string buttonText, string headerText, Func<Tuple<List<string>, List<string>>> GetDataCallback, Action<int, string, List<string>> OnChoseButtonCallback, params string[] disableMatch)
        where T : ButtonListPopup
    {
        if (gui.Button(buttonText))
        {
            Debug.Log("ButtonListPopup pressed " + buttonText);
            Tuple<List<string>, List<string>> data = GetDataCallback();

            T buttonListPopup = (T)Plugin.Instance.ToggleWindow(typeof(T));
            buttonListPopup.position = Vector2.zero;
            buttonListPopup.popupNameOverride = buttonText;
            buttonListPopup.callback = OnChoseButtonCallback;
            buttonListPopup.buttonNames = data.Item1;
            buttonListPopup.buttonValues = data.Item2;
            buttonListPopup.header = headerText;
            buttonListPopup.filterText = "";
            buttonListPopup.disableMatch = disableMatch.ToList();
            return true;
        }

        return false;
    }
}