using BepInEx.Logging;
using DebugMenu.Scripts.Acts;
using DebugMenu.Scripts.Utils;
using DiskCardGame;
using UnityEngine;

namespace DebugMenu.Scripts.Act3;

public class Act3 : BaseAct
{
	private MapSequence m_mapSequence;
	private CardBattleSequence m_cardBattleSequence;
	
	public Act3(DebugWindow window) : base(window)
	{
		m_mapSequence = new MapSequence(this);
		m_cardBattleSequence = new CardBattleSequence(this);
	}

	public override void Update()
	{
		
	}
	
	public override void OnGUI()
	{
		MapNodeManager mapNodeManager = Singleton<MapNodeManager>.m_Instance;
		if (mapNodeManager == null || mapNodeManager.nodes == null || RunState.Run == null)
		{
			return;
		}
		
		Window.LabelHeader("Act 3");
		
		if (RunState.Run.currentNodeId > 0)
		{
			MapNode nodeWithId = mapNodeManager.GetNodeWithId(RunState.Run.currentNodeId);

			string s = null;
			if (nodeWithId != null)
			{
				s = nodeWithId.GetType() + " " + nodeWithId.nodeId + " " + nodeWithId.Data.prefabPath;
			}
			Window.Label("Current Node: " + RunState.Run.currentNodeId + " = " + s, 120);
		}

		Window.Padding();
	
		Window.Label("Currency: " + Part3SaveData.Data.currency);
		if (Window.Button("Add 5 Cash"))
		{
			Part3SaveData.Data.currency += 5;
		}
		if (Window.Button("Remove 5 Cash"))
		{
			RunState.Run.currency = Mathf.Max(0, Part3SaveData.Data.currency - 5);
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
				OnGUIMap();
				break;
			case GameState.FirstPerson3D:
				break;
			case GameState.SpecialCardSequence:
				SpecialNodeData nodeWithId = Helpers.LastSpecialNodeData;
				Type nodeType = nodeWithId.GetType();
				Window.Label("Unhandled node type");
				Window.Label(nodeType.FullName);
				break;
			default:
				Window.Label("Unhandled GameFlowState:");
				Window.Label(gameFlowManager.CurrentGameState.ToString());
				break;
		}
	}

	public override void OnGUIRestart()
	{
		// TODO:
	}

	public override void OnGUIReload()
	{
		// TODO:
	}

	private void OnGUIMap()
	{
		m_mapSequence.OnGUI();
	}
}