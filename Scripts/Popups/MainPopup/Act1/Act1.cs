﻿using DebugMenu.Scripts.Acts;
using DebugMenu.Scripts.Popups;
using DebugMenu.Scripts.Utils;
using DiskCardGame;
using UnityEngine;

namespace DebugMenu.Scripts.Act1;

public class Act1 : BaseAct
{
	public static List<CardInfo> lastUsedStarterDeck = null;
	public static bool SkipNextNode = false;
	public static bool ActivateAllMapNodesActive = false;

	public Act1(DebugWindow window) : base(window)
	{
		m_mapSequence = new Act1MapSequence(this);
		m_cardBattleSequence = new Act1CardBattleSequence(window);
	}

	public override void OnGUI()
	{
		MapNodeManager mapNodeManager = Singleton<MapNodeManager>.m_Instance;
		if (mapNodeManager?.nodes == null || RunState.Run == null)
			return;

		Window.LabelHeader("Act 1");
		
		if (RunState.Run.currentNodeId > 0)
		{
            MapNode nodeWithId = mapNodeManager.GetNodeWithId(RunState.Run.currentNodeId);
            Window.Label("Current Node ID: " + RunState.Run.currentNodeId + "\nCurrent Node: " + nodeWithId?.name, new(0, 120));
        }
		if (Window.Button("Replenish Candles"))
		{
            Plugin.Instance.StartCoroutine(CandleHolder.Instance.ReplenishFlamesSequence(0f));
            RunState.Run.playerLives = RunState.Run.maxPlayerLives;
			SaveManager.SaveToFile(false);
        }
        Window.LabelHeader("Currency: " + RunState.Run.currency);

        using (Window.HorizontalScope(4))
		{
            if (Window.Button("+1"))
                RunState.Run.currency ++;

            if (Window.Button("-1"))
                RunState.Run.currency = Mathf.Max(0, RunState.Run.currency - 1);

            if (Window.Button("+5"))
				RunState.Run.currency += 5;

			if (Window.Button("-5"))
				RunState.Run.currency = Mathf.Max(0, RunState.Run.currency - 5);
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
			return;

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
				if (nodeType == typeof(CardChoicesNodeData))
				{
					OnGUICardChoiceNodeSequence();
				}
				else
				{
					Window.Label("Unhandled node type");
					Window.Label(nodeType.FullName);
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
			sequencer.OnRerollChoices();
	}

	private void OnGUIMap()
	{
		m_mapSequence.OnGUI();
	}

	public override void Restart()
	{
		if (SaveFile.IsAscension)
			NewAscensionGame();
		else if (SaveManager.SaveFile.IsPart1)
			RestartVanilla();
	}

	public override void Reload()
	{
		if (SaveFile.IsAscension)
		{
			if (AscensionSaveData.Data.currentRun != null)
				ReloadKaycees();
			else
				NewAscensionGame();
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