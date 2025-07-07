using DebugMenu.Scripts.Acts;

namespace DebugMenu.Scripts.Popups;

public class SequenceListPopup : ButtonListPopup
{
    private bool updateSequences = false;
    public override void OnGUI()
    {
        if (updateSequences)
        {
            Tuple<List<string>, List<string>> data = BaseAct.GetListsOfSequences();
            base.buttonNames = data.Item1;
            base.buttonValues = data.Item2;
        }
        updateSequences = false;
        base.OnGUI();
    }
    public override void DrawExtraTools()
    {
        updateSequences = base.Toggle("Show All sequences", ref Configs.m_showAllSeqs);
        base.Toggle("Hide mod GUIDs", ref Configs.m_hideGuids);
    }
    public override string ModifyButtonName(string name, string value)
    {
        return Configs.HideModGUIDs ? value : name;
    }
}