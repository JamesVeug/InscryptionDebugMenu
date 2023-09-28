using System.Collections;
using BepInEx.Logging;
using DebugMenu.Scripts.Acts;
using DiskCardGame;
using UnityEngine;

namespace DebugMenu.Scripts.Act1;

public class Act1CardBattleSequence : BaseCardBattleSequence
{
	public override int PlayerBones => ResourcesManager.Instance.PlayerBones;
	public override int ScalesBalance => LifeManager.Instance.Balance;
	public override int PlayerEnergy => ResourcesManager.Instance.PlayerEnergy;
	public override int PlayerMaxEnergy => ResourcesManager.Instance.PlayerMaxEnergy;
    public override CardDrawPiles CardDrawPiles => Singleton<Part1CardDrawPiles>.Instance;
    public Act1CardBattleSequence(DebugWindow window) : base(window)
	{
	}
	
	public override void OnGUI()
	{
		MapNode nodeWithId =  Singleton<MapNodeManager>.m_Instance.GetNodeWithId(RunState.Run.currentNodeId);
		if (nodeWithId.Data is CardBattleNodeData cardBattleNodeData)
		{
			Window.Label($"Difficulty: {cardBattleNodeData.difficulty + RunState.Run.DifficultyModifier} " +
                $"({cardBattleNodeData.difficulty} + {RunState.Run.DifficultyModifier})" +
				"\nTurn Number: " + TurnManager.Instance.TurnNumber);
		}

		base.OnGUI();
	}
}