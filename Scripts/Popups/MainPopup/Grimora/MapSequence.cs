using System.Collections;
using BepInEx.Logging;
using DebugMenu.Scripts.Acts;
using DiskCardGame;
using UnityEngine;

namespace DebugMenu.Scripts.Grimora;

public class MapSequence
{
	private readonly ActGrimora Act;
	private readonly DebugWindow Window;

	public MapSequence(ActGrimora act)
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
	}
}