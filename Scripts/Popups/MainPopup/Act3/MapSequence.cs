using DebugMenu.Scripts.Acts;
using DiskCardGame;

namespace DebugMenu.Scripts.Act3;

public class MapSequence : BaseMapSequence
{
    private readonly Act3 Act;
    private readonly DebugWindow Window;

    public MapSequence(Act3 act)
    {
        this.Act = act;
        this.Window = act.Window;
    }

    public override void OnGUI()
    {
        bool activateAllNodes = Act1.Act1.ActivateAllMapNodesActive;
        if (Window.Toggle("Activate all Map nodes", ref activateAllNodes))
        {
            ToggleAllNodes();
        }

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
}