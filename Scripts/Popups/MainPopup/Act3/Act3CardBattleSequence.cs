using System.Collections;
using BepInEx.Logging;
using DebugMenu.Scripts.Acts;
using DiskCardGame;
using UnityEngine;

namespace DebugMenu.Scripts.Act3;

public class Act3CardBattleSequence : BaseCardBattleSequence
{
	public override int PlayerBones => ResourcesManager.Instance.PlayerBones;
	public override int ScalesBalance => LifeManager.Instance.Balance;
	public override int PlayerEnergy => ResourcesManager.Instance.PlayerEnergy;
	public override int PlayerMaxEnergy => ResourcesManager.Instance.PlayerMaxEnergy;
	public override CardDrawPiles CardDrawPiles => Singleton<Part3CardDrawPiles>.Instance;
	public Act3CardBattleSequence(DebugWindow window) : base(window)
	{
	}

	public override void OnGUI()
	{
		MapNodeManager mapNodeManager = Singleton<MapNodeManager>.m_Instance;
		if (mapNodeManager == null)
			return;

        MapNode nodeWithId = mapNodeManager.GetNodeWithId(RunState.Run.currentNodeId);

        if (nodeWithId?.Data is CardBattleNodeData cardBattleNodeData)
			Window.Label($"Difficulty: {cardBattleNodeData.difficulty} + {RunState.Run.DifficultyModifier}");

        base.OnGUI();
	}

	public override void AddBones(int amount)
	{
		
	}
	public override void RemoveBones(int amount)
	{
		
	}
}