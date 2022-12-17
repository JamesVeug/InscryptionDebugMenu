using BepInEx.Logging;
using DebugMenu.Scripts.Acts;
using DiskCardGame;
using UnityEngine;

namespace DebugMenu.Scripts.Act1;

public class Act1 : BaseAct
{
	public static List<CardInfo> lastUsedStarterDeck = null;
	public static bool isNodeDebugModeActive = false;
	public static bool activateAllMapNodesActive = false;
	
	public Act1(ManualLogSource logger) : base(logger)
	{
	}

	public override void Update()
	{
		
	}

	public override void OnGUI()
	{
		GUIHelper.LabelHeader("Act 1");

		if (RunState.Run.currentNodeId > 0 && Singleton<MapNodeManager>.m_Instance != null)
		{
			MapNode nodeWithId = Singleton<MapNodeManager>.Instance.GetNodeWithId(RunState.Run.currentNodeId);
			GUIHelper.Label("Current Node: " + RunState.Run.currentNodeId + " = " + nodeWithId, 80);
			
		}
		
		if (GUIHelper.Button("Replenish Flames"))
		{
			RunState.Run.playerLives = RunState.Run.maxPlayerLives;
		}
		if (GUIHelper.Button("Add 5 Teeth"))
		{
			RunState.Run.currency += 5;
		}
		
		GUIHelper.StartNewColumn();
		OnGUICurrentNode();
	}

	private void OnGUICurrentNode()
	{
		GameFlowManager gameFlowManager = Singleton<GameFlowManager>.m_Instance;
		if (gameFlowManager == null)
		{
			return;
		}

		GUIHelper.LabelHeader(gameFlowManager.CurrentGameState.ToString());
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
				break;
			default:
				throw new ArgumentOutOfRangeException();
		}
	}

	private static void OnGUICardBattle()
	{
		if (GUIHelper.Button("Auto win battle"))
		{
			LifeManager lifeManager = Singleton<LifeManager>.Instance;
			int lifeLeft = Mathf.Abs(lifeManager.Balance - 5);
			Plugin.Instance.StartCoroutine(lifeManager.ShowDamageSequence(lifeLeft, lifeLeft, false, 0.125f, null, 0f, false));
		}
		if (GUIHelper.Button("Auto lose battle"))
		{
			LifeManager lifeManager = Singleton<LifeManager>.Instance;
			int lifeLeft = Mathf.Abs(lifeManager.Balance - 5);
			Plugin.Instance.StartCoroutine(lifeManager.ShowDamageSequence(lifeLeft, lifeLeft, true, 0.125f, null, 0f, false));
		}
	}

	public override void OnGUIRestart()
	{
		if (GUIHelper.Button("Restart"))
		{
			Restart();
		}
	}

	public override void OnGUIReload()
	{
		if (GUIHelper.Button("Reload"))
		{
			Reload();
		}
	}

	private void OnGUIMap()
	{
		GUIHelper.Toggle("Skip next node", ref isNodeDebugModeActive);
		if (GUIHelper.Toggle("Activate all Map nodes", ref activateAllMapNodesActive))
		{
			MapNode node = Singleton<MapNodeManager>.Instance.ActiveNode;
			Singleton<MapNodeManager>.Instance.SetActiveNode(node);
		}
	}

	private void Restart()
	{
		if (SaveFile.IsAscension)
		{
			NewAscensionGame();
		}
		else if (SaveManager.SaveFile.IsPart1)
		{
			RestartVanilla();
		}
	}
	
	private void Reload()
	{
		if (SaveFile.IsAscension)
		{
			if (AscensionSaveData.Data.currentRun != null)
			{
				ReloadKaycees();
			}
			else
			{
				NewAscensionGame();
			}
		}
		else
		{
			ReloadVanilla();
		}
	}

	private void NewAscensionGame()
	{
		FrameLoopManager.Instance.SetIterationDisabled(disabled: false);
		SceneLoader.Load("Ascension_Configure");
		if (lastUsedStarterDeck != null)
		{
			Log("New Game! With " + lastUsedStarterDeck.Count + " Cards!");
			AscensionSaveData.Data.NewRun(lastUsedStarterDeck);
			SaveManager.SaveToFile(saveActiveScene: false);
			MenuController.LoadGameFromMenu(newGameGBC: false);
			Singleton<InteractionCursor>.Instance.SetHidden(hidden: true);
		}
	}
	
	private void ReloadKaycees()
	{
		Log("Reloading Ascension...");
		FrameLoopManager.Instance.SetIterationDisabled(disabled: false);
		SceneLoader.Load("Ascension_Configure");
		FrameLoopManager.Instance.SetIterationDisabled(disabled: false);
		SaveManager.savingDisabled = false;
		MenuController.LoadGameFromMenu(newGameGBC: false);
	}
	
	private void ReloadVanilla()
	{
		Log("Reloading Vanilla...");
		FrameLoopManager.Instance.SetIterationDisabled(disabled: false);
		MenuController.ReturnToStartScreen();
		MenuController.LoadGameFromMenu(newGameGBC: false);
	}
	
	private void RestartVanilla()
	{
		Log("Restarting Vanilla...");
		FrameLoopManager.Instance.SetIterationDisabled(disabled: false);
		SaveManager.SaveFile.ResetPart1Run();
		SaveManager.SaveToFile(saveActiveScene: false);
		SceneLoader.Load("Part1_Cabin");
	}
}