using DebugMenu.Scripts.Acts;
using DebugMenu.Scripts.Popups;
using DiskCardGame;
using InscryptionAPI.Regions;

namespace DebugMenu.Scripts.Magnificus;

public class ActMagnificusMapSequence : BaseMapSequence
{
    public static bool RegionOverride = false;

    private readonly ActMagnificus Act = null;
    private readonly DebugWindow Window = null;

    public ActMagnificusMapSequence(ActMagnificus act)
    {
        this.Act = act;
        this.Window = act.Window;
    }

    public override void OnGUI()
    {
        bool skipNextNode = ActMagnificus.SkipNextNode;
        if (Window.Toggle("Skip map events", ref skipNextNode))
            ToggleSkipNextNode();

        Act.DrawSequencesGUI();
    }

    public override void ToggleSkipNextNode()
    {
        ActMagnificus.SkipNextNode = !ActMagnificus.SkipNextNode;
    }

    public override void ToggleAllNodes()
    {

    }

    private Tuple<List<string>, List<string>> RegionNameList()
    {
        List<string> regionsNames = RegionManager.AllRegionsCopy.ConvertAll((a) => a.name).ToList();
        return new Tuple<List<string>, List<string>>(regionsNames, regionsNames);
    }
}