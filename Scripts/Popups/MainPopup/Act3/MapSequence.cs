using System.Collections;
using BepInEx.Logging;
using DebugMenu.Scripts.Acts;
using DiskCardGame;
using UnityEngine;

namespace DebugMenu.Scripts.Act3;

public class MapSequence
{
	private readonly Act3 Act;
	private readonly DebugWindow Window;

	public MapSequence(Act3 act)
	{
		this.Act = act;
		this.Window = act.Window;
	}

	public void OnGUI()
	{
		if (Window.Toggle("Activate all Map nodes", ref Act1.Act1.ActivateAllMapNodesActive))
		{
			MapNode node = Singleton<MapNodeManager>.Instance.ActiveNode;
			Singleton<MapNodeManager>.Instance.SetActiveNode(node);
		}

		Act.DrawSequencesGUI();
	}
}