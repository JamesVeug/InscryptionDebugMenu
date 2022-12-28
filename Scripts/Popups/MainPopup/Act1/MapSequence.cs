using System.Collections;
using BepInEx.Logging;
using DebugMenu.Scripts.Acts;
using DiskCardGame;
using UnityEngine;

namespace DebugMenu.Scripts.Act1;

public class MapSequence
{
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
	}

	private IEnumerator RareCardSequence()
	{
		Singleton<GameFlowManager>.Instance.TransitionToGameState(GameState.SpecialCardSequence, new ChooseRareCardNodeData());
		yield return null;
	}
}