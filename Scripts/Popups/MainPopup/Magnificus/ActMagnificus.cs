using BepInEx.Logging;
using DebugMenu.Scripts.Acts;
using DebugMenu.Scripts.Utils;
using DiskCardGame;

namespace DebugMenu.Scripts.Magnificus;

public class ActMagnificus : BaseAct
{
	public ActMagnificus(DebugWindow window) : base(window)
	{
		m_cardBattleSequence = new CardBattleSequence(window);
	}

	public override void Update()
	{
		
	}
	
	public override void OnGUI()
	{
		Window.LabelHeader("Magnificus Act");
		OnGUICurrentNode();
	}

	public override void OnGUIMinimal()
	{
		OnGUICurrentNode();
	}
	
	private void OnGUICurrentNode()
	{
		MagnificusGameFlowManager gameFlowManager = Singleton<MagnificusGameFlowManager>.m_Instance;
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

	private void OnGUIMap()
	{
		Window.Label("Support not started");
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