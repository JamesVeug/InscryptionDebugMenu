using DebugMenu.Scripts.Acts;
using DebugMenu.Scripts.Popups;
using DiskCardGame;
using InscryptionAPI.Regions;

namespace DebugMenu.Scripts.Act3;

public class Act3MapSequence : BaseMapSequence
{
    private readonly Act3 Act;
    private readonly DebugWindow Window;

    public Act3MapSequence(Act3 act)
    {
        this.Act = act;
        this.Window = act.Window;
    }

    public override void OnGUI()
    {
        Window.LabelBold("Current World:\n" + HoloMapAreaManager.Instance.CurrentWorld?.Id ?? "N/A");
        ButtonListPopup.OnGUI<ButtonListPopup>(Window, "Fast Travel", "Fast Travel", RegionNameList, static (_, value, _) =>
        {
            Singleton<HoloGameMap>.Instance.fastTravelMap.nodes.Find(x => x.world.Id == value)?.OnCursorSelectEnd();
        });

        Window.Padding();
        Act.DrawSequencesGUI();
    }

    public override void ToggleSkipNextNode()
    {

    }

    public override void ToggleAllNodes()
    {
        Act1.Act1.ActivateAllMapNodesActive = !Act1.Act1.ActivateAllMapNodesActive;
        MapNode node = Singleton<MapNodeManager>.Instance.ActiveNode;
        Singleton<MapNodeManager>.Instance.SetActiveNode(node);
    }

    private Tuple<List<string>, List<string>> RegionNameList()
    {
        List<string> regionsNames = Singleton<HoloGameMap>.Instance.fastTravelMap.nodes.Select(x => x.world.Id).ToList();
        return new Tuple<List<string>, List<string>>(regionsNames, regionsNames);
    }
}