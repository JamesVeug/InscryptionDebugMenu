using BepInEx.Logging;
using DebugMenu.Scripts.Acts;
using DebugMenu.Scripts.Utils;
using DiskCardGame;
using UnityEngine;

namespace DebugMenu.Scripts.Grimora;

public class ActGrimora : BaseAct
{
	public ActGrimora(DebugWindow window) : base(window)
	{
		m_mapSequence = new MapSequence(this);
		m_cardBattleSequence = new CardBattleSequence(window);
	}

	public override void Update()
	{
		
	}

	public override void OnGUI()
	{
		Window.LabelHeader("Grimora Act");

		if (RunState.Run.currentNodeId > 0 && Singleton<MapNodeManager>.m_Instance != null)
		{
			MapNode nodeWithId = Singleton<MapNodeManager>.Instance.GetNodeWithId(RunState.Run.currentNodeId);
			Window.Label("Current Node: " + RunState.Run.currentNodeId + " = " + nodeWithId, new(0, 120));
		}
		
		DrawItemsGUI();
		
		Window.StartNewColumn();
		OnGUICurrentNode();
	}

	public override void OnGUIMinimal()
	{
		OnGUICurrentNode();
	}

	private void OnGUICurrentNode()
	{
		GameFlowManager gameFlowManager = Singleton<GameFlowManager>.m_Instance;
		if (gameFlowManager == null)
		{
			return;
		}

		Window.LabelHeader(gameFlowManager.CurrentGameState.ToString());
		switch (gameFlowManager.CurrentGameState)
		{
			case GameState.CardBattle:
				m_cardBattleSequence.OnGUI();
				break;
			case GameState.Map:
				// Show map related buttons
				m_mapSequence.OnGUI();
				break;
			case GameState.FirstPerson3D:
				break;
			case GameState.SpecialCardSequence:
				SpecialNodeData nodeWithId = Helpers.LastSpecialNodeData;
				Type nodeType = nodeWithId.GetType();
				if (nodeType == typeof(CardChoicesNodeData))
				{
					OnGUICardChoiceNodeSequence();
				}
				else if (nodeType.ToString().ToLower().Contains("electricchair"))
				{
					OnGUIElectricChairNodeSequence();
				}
				else
				{
					Window.Label("Unhandled node type:");
					Window.Label(nodeType.ToString());
				}
				break;
			default:
				Window.Label("Unhandled GameFlowState:");
				Window.Label(gameFlowManager.CurrentGameState.ToString());
				break;
		}
	}

	private void OnGUICardChoiceNodeSequence()
	{
		CardSingleChoicesSequencer sequencer = Singleton<SpecialNodeHandler>.Instance.cardChoiceSequencer;
		Window.Label("Sequencer: " + sequencer, new(0, 80));
		if (Window.Button("Reroll choices"))
		{
			sequencer.OnRerollChoices();
		}
	}

	private void OnGUIElectricChairNodeSequence()
	{
		Window.Label("TODO:");
	}

	public override void Restart()
	{
		// TODO:
	}

	public override void Reload()
	{
		// TODO:
	}
}