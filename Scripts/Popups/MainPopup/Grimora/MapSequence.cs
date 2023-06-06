using DebugMenu.Scripts.Acts;
using DiskCardGame;

namespace DebugMenu.Scripts.Grimora;

public class MapSequence : BaseMapSequence
{
    private readonly ActGrimora Act;
    private readonly DebugWindow Window;

    public MapSequence(ActGrimora act)
    {
        this.Act = act;
        this.Window = act.Window;
    }

    public override void OnGUI()
    {
        bool nodesActive = Act1.Act1.ActivateAllMapNodesActive;
        if (Window.Toggle("Activate all Map nodes", ref nodesActive))
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