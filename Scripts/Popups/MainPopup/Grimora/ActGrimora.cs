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
	}

	public override void Update()
	{
		
	}

	public override void OnGUI()
	{
		Window.LabelHeader("Grimora Act");;
		
		Window.Label("Inventory: ");
		foreach (string consumable in RunState.Run.consumables)
		{
			Window.Label("-" + consumable);
		}

		if (RunState.Run.currentNodeId > 0 && Singleton<MapNodeManager>.m_Instance != null)
		{
			MapNode nodeWithId = Singleton<MapNodeManager>.Instance.GetNodeWithId(RunState.Run.currentNodeId);
			Window.Label("Current Node: " + RunState.Run.currentNodeId + " = " + nodeWithId, 120);
			
		}
		
		Window.StartNewColumn();
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
				OnGUICardBattle();
				break;
			case GameState.Map:
				// Show map related buttons
				OnGUIMap();
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
		Window.Label("Sequencer: " + sequencer, 80);
		if (Window.Button("Reroll choices"))
		{
			sequencer.OnRerollChoices();
		}
	}

	private void OnGUIElectricChairNodeSequence()
	{
		Window.Label("TODO:");
	}

	private void OnGUICardBattle()
	{
		TurnManager turnManager = Singleton<TurnManager>.Instance;
		Window.Label("Opponent: " + turnManager.opponent);
		Window.Label("Difficulty: " + turnManager.opponent.Difficulty);
		Window.Label("Blueprint: " + turnManager.opponent.Blueprint.name);
		
		if (Window.Button("Auto win battle"))
		{
			LifeManager lifeManager = Singleton<LifeManager>.Instance;
			int lifeLeft = Mathf.Abs(lifeManager.Balance - 5);
			Plugin.Instance.StartCoroutine(lifeManager.ShowDamageSequence(lifeLeft, lifeLeft, false, 0.125f, null, 0f, false));
		}
		if (Window.Button("Auto lose battle"))
		{
			LifeManager lifeManager = Singleton<LifeManager>.Instance;
			int lifeLeft = Mathf.Abs(lifeManager.Balance - 5);
			Plugin.Instance.StartCoroutine(lifeManager.ShowDamageSequence(lifeLeft, lifeLeft, true, 0.125f, null, 0f, false));
		}
	}

	private void OnGUIMap()
	{
		// TODO:
	}

	public override void OnGUIRestart()
	{
		// TODO:
	}

	public override void OnGUIReload()
	{
		// TODO:
	}
}