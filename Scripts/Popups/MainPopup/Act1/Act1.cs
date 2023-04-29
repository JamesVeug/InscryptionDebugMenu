using DebugMenu.Scripts.Acts;
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

	private MapSequence m_mapSequence;

	public Act1(DebugWindow window) : base(window)
	{
		m_mapSequence = new MapSequence(this);
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
		
		Window.LabelHeader("Act 1");
		
		if (RunState.Run.currentNodeId > 0)
		{
			MapNode nodeWithId = mapNodeManager.GetNodeWithId(RunState.Run.currentNodeId);
			Window.Label("Current Node: " + RunState.Run.currentNodeId + " = " + nodeWithId, 120);
		}
		
		if (Window.Button("Replenish Candles"))
		{
			RunState.Run.playerLives = RunState.Run.maxPlayerLives;
		}
		if (Window.Button("Add 5 Teeth"))
		{
			RunState.Run.currency += 5;
		}
		if (Window.Button("Remove 5 Teeth"))
		{
			RunState.Run.currency = Mathf.Max(0, RunState.Run.currency - 5);
		}

		Window.LabelHeader("Items");
		DrawItemsGUI();
		
		Window.StartNewColumn();
		OnGUICurrentNode();
	}

	public override void OnGUIMinimal()
	{
		OnGUICurrentNode();
	}

	private void DrawItemsGUI()
	{
		List<string> items = RunState.Run.consumables;

		for (int i = 0; i < RunState.Run.MaxConsumables; i++)
		{
			string consumable = i >= items.Count ? null : items[i];
			string itemRulebookName = Helpers.GetConsumableByName(consumable);
			string itemName = itemRulebookName != null ? itemRulebookName : consumable == null ? "None" : consumable;
			ButtonListPopup.OnGUI(Window, itemName, "Change Item " + (i+1), GetListsOfAllItems, OnChoseButtonCallback, i.ToString());
		}
	}

	private static void OnChoseButtonCallback(int chosenIndex, string chosenValue, string inventoryIndex)
	{
		List<string> currentItems = RunState.Run.consumables;
		int index = int.Parse(inventoryIndex);
		string selectedItem = index >= RunState.Run.consumables.Count ? null : RunState.Run.consumables[index];

		if (chosenValue == null)
		{
			ItemsManager.Instance.RemoveItemFromSaveData(selectedItem);
		}
		else
		{
			if (index >= currentItems.Count)
			{
				currentItems.Add(chosenValue);
			}
			else
			{
				currentItems[index] = chosenValue;
			}
		}

		foreach (ConsumableItemSlot slot in Singleton<ItemsManager>.Instance.consumableSlots)
		{
			if (slot.Item)
			{
				slot.DestroyItem();
			}
		}

		Singleton<ItemsManager>.Instance.UpdateItems(true);
	}

	private Tuple<List<string>, List<string>> GetListsOfAllItems()
	{
		List<ConsumableItemData> allConsumables = ItemsUtil.AllConsumables;

		List<string> names = new List<string>(allConsumables.Count);
		List<string> values = new List<string>(allConsumables.Count);
		
		names.Add("None"); // Option to set the item to null (Don't have an item in this slot)
		values.Add(null); // Option to set the item to null (Don't have an item in this slot) 
		for (int i = 0; i < allConsumables.Count; i++)
		{
			names.Add(allConsumables[i].rulebookName + "\n(" + allConsumables[i].name + ")");
			values.Add(allConsumables[i].name);
		}

		return new Tuple<List<string>, List<string>>(names, values);
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
		Window.Label("Sequencer: " + sequencer, 80);
		if (Window.Button("Reroll choices"))
		{
			sequencer.OnRerollChoices();
		}
	}

	private void OnGUICardBattle()
	{
		MapNode nodeWithId =  Singleton<MapNodeManager>.m_Instance.GetNodeWithId(RunState.Run.currentNodeId);
		if (nodeWithId.Data is CardBattleNodeData cardBattleNodeData)
		{
			Window.Label($"Difficulty: {cardBattleNodeData.difficulty} + {RunState.Run.DifficultyModifier}");
		}
		
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

		Window.Padding();
			
		if (Window.Button("Draw Card"))
		{
			Part1CardDrawPiles part1CardDrawPiles = (Singleton<CardDrawPiles>.Instance as Part1CardDrawPiles);
			if (part1CardDrawPiles)
			{
				if (part1CardDrawPiles.Deck.cards.Count > 0)
				{
					part1CardDrawPiles.pile.Draw();
					Plugin.Instance.StartCoroutine(part1CardDrawPiles.DrawCardFromDeck());
				}
			}
			else
			{
				Plugin.Log.LogError("Could not draw card. Can't find CardDrawPiles!");
			}
		}
		
		if (Window.Button("Draw Squirrel"))
		{
			Part1CardDrawPiles part1CardDrawPiles = (Singleton<CardDrawPiles>.Instance as Part1CardDrawPiles);
			if (part1CardDrawPiles)
			{
				if (part1CardDrawPiles.SideDeck.cards.Count > 0)
				{
					part1CardDrawPiles.SidePile.Draw();
					Plugin.Instance.StartCoroutine(part1CardDrawPiles.DrawFromSidePile());
				}
			}
			else
			{
				Plugin.Log.LogError("Could not draw squirrel. Can't find CardDrawPiles!");
			}
		}
		
		Window.Padding();
		
		if (Window.Button("Deal 2 Damage"))
		{
			LifeManager lifeManager = Singleton<LifeManager>.Instance;
			Plugin.Instance.StartCoroutine(lifeManager.ShowDamageSequence(2, 2, false, 0.125f, null, 0f, false));
		}
		
		if (Window.Button("Take 2 Damage"))
		{
			LifeManager lifeManager = Singleton<LifeManager>.Instance;
			Plugin.Instance.StartCoroutine(lifeManager.ShowDamageSequence(2, 2, true, 0.125f, null, 0f, false));
		}
	}

	public override void OnGUIRestart()
	{
		if (Window.Button("Restart"))
		{
			Restart();
		}
	}

	public override void OnGUIReload()
	{
		if (Window.Button("Reload"))
		{
			Reload();
		}
	}

	private void OnGUIMap()
	{
		m_mapSequence.OnGUI();
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