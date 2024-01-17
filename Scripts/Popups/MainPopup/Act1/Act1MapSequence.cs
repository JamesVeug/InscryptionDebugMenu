using System.Collections;
using System.Reflection;
using BepInEx.Logging;
using DebugMenu.Scripts.Acts;
using DebugMenu.Scripts.Popups;
using DebugMenu.Scripts.Utils;
using DiskCardGame;
using InscryptionAPI.Regions;
using UnityEngine;

namespace DebugMenu.Scripts.Act1;

public class Act1MapSequence : BaseMapSequence
{
	public static bool RegionOverride = false;
	public static string RegionNameOverride = "No region selected";  
	
	private readonly Act1 Act = null;
	private readonly DebugWindow Window = null;

	public Act1MapSequence(Act1 act)
	{
		this.Act = act;
		this.Window = act.Window;
	}

	public override void OnGUI()
	{
		bool skipNextNode = Act1.SkipNextNode;
		if (Window.Toggle("Skip next node", ref skipNextNode))
			ToggleSkipNextNode();
		
		bool activateAllNodes = Act1.ActivateAllMapNodesActive;
		if (Window.Toggle("Activate all Map nodes", ref activateAllNodes))
			ToggleAllNodes();

        Window.Toggle("Toggle Map Override", ref RegionOverride);
        Act.DrawSequencesGUI();
		
        Window.LabelHeader("<b>Override Region</b>");
        ButtonListPopup.OnGUI(Window, RegionNameOverride, "Override Region", RegionNameList, static (_, value, _) =>
        {
            RegionNameOverride = value;
        });
    }

	public override void ToggleSkipNextNode()
	{
		Act1.SkipNextNode = !Act1.SkipNextNode;
	}

	public override void ToggleAllNodes()
	{
		Act1.ActivateAllMapNodesActive = !Act1.ActivateAllMapNodesActive;
		if (MapNodeManager.m_Instance != null)
		{
            MapNode node = Singleton<MapNodeManager>.Instance.ActiveNode;
			if (node == null)
				return;

            Singleton<MapNodeManager>.Instance.SetActiveNode(node);
        }
	}

	private Tuple<List<string>, List<string>> RegionNameList()
	{
		List<string> regionsNames = RegionManager.AllRegionsCopy.ConvertAll((a) => a.name).ToList();
		return new Tuple<List<string>, List<string>>(regionsNames, regionsNames);
	}
}