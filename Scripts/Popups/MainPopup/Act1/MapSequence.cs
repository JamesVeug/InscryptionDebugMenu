using DebugMenu.Scripts.Acts;
using DebugMenu.Scripts.Popups;
using DiskCardGame;
using InscryptionAPI.Regions;

namespace DebugMenu.Scripts.Act1;

public class MapSequence : BaseMapSequence
{
    public static bool RegionOverride = false;
    public static string RegionNameOverride = "";

    private readonly Act1 Act = null;
    private readonly DebugWindow Window = null;

    public MapSequence(Act1 act)
    {
        this.Act = act;
        this.Window = act.Window;
    }

    public override void OnGUI()
    {
        bool skipNextNode = Act1.SkipNextNode;
        if (Window.Toggle("Skip next node", ref skipNextNode))
        {
            ToggleSkipNextNode();
        }

        bool activateAllNodes = Act1.ActivateAllMapNodesActive;
        if (Window.Toggle("Activate all Map nodes", ref activateAllNodes))
        {
            ToggleAllNodes();
        }

        Act.DrawSequencesGUI();

        Window.Padding();

        Window.Label("Override Region");
        ButtonListPopup.OnGUI(Window, RegionNameOverride, "Override Region", RegionNameList, static (_, value, _) =>
        {
            RegionNameOverride = value;
        });
        Window.Toggle("Toggle Map Override", ref RegionOverride);
    }

    public override void ToggleSkipNextNode()
    {
        Act1.SkipNextNode = !Act1.SkipNextNode;
    }

    public override void ToggleAllNodes()
    {
        Act1.ActivateAllMapNodesActive = !Act1.ActivateAllMapNodesActive;
        MapNode node = Singleton<MapNodeManager>.Instance.ActiveNode;
        Singleton<MapNodeManager>.Instance.SetActiveNode(node);
    }

    private Tuple<List<string>, List<string>> RegionNameList()
    {
        List<string> regionsNames = RegionManager.AllRegionsCopy.ConvertAll((a) => a.name).ToList();
        return new Tuple<List<string>, List<string>>(regionsNames, regionsNames);
    }
}