using System.Collections;
using BepInEx.Logging;
using DebugMenu.Scripts.Acts;
using DebugMenu.Scripts.Popups;
using DiskCardGame;
using InscryptionAPI.Regions;
using UnityEngine;

namespace DebugMenu.Scripts.Act1;

public class MapSequence
{
	public static bool RegionOverride = false;
	public static string RegionNameOverride = "";  
	
	private readonly Act1 Act;
	private readonly DebugWindow Window;

	public MapSequence(Act1 act)
	{
		this.Act = act;
		this.Window = act.Window;
	}

	public void OnGUI()
	{
		Window.Toggle("Skip next node", ref Act1.SkipNextNode);
		if (Window.Toggle("Activate all Map nodes", ref Act1.ActivateAllMapNodesActive))
		{
			MapNode node = Singleton<MapNodeManager>.Instance.ActiveNode;
			Singleton<MapNodeManager>.Instance.SetActiveNode(node);
		}

		if (Window.Button("Rare Card Sequence"))
		{
			Plugin.Instance.StartCoroutine(RareCardSequence());
		}
		
		Window.Padding();

		Window.Label("Override Region");
		ButtonListPopup.OnGUI(Window, RegionNameOverride, "Override Region", RegionNameList, static (_, value, _)=>
		{
			RegionNameOverride = value;
		});
		Window.Toggle("Toggle Map Override", ref RegionOverride);
	}

	private IEnumerator RareCardSequence()
	{
		Singleton<GameFlowManager>.Instance.TransitionToGameState(GameState.SpecialCardSequence, new ChooseRareCardNodeData());
		yield return null;
	}

	private Tuple<List<string>, List<string>> RegionNameList()
	{
		List<string> regionsNames = RegionManager.AllRegionsCopy.ConvertAll((a) => a.name).ToList();
		return new Tuple<List<string>, List<string>>(regionsNames, regionsNames);
	}
}