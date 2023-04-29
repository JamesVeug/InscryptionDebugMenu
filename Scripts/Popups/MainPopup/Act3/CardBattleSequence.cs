using System.Collections;
using BepInEx.Logging;
using DebugMenu.Scripts.Acts;
using DiskCardGame;
using UnityEngine;

namespace DebugMenu.Scripts.Act3;

public class CardBattleSequence
{
	private readonly Act3 Act;
	private readonly DebugWindow Window;

	public CardBattleSequence(Act3 act)
	{
		this.Act = act;
		this.Window = act.Window;
	}

	public void OnGUI()
	{
		MapNodeManager mapNodeManager = Singleton<MapNodeManager>.m_Instance;
		if (mapNodeManager == null)
		{
			return;
		}
		
		MapNode nodeWithId =  mapNodeManager.GetNodeWithId(RunState.Run.currentNodeId);
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
			Part3CardDrawPiles part1CardDrawPiles = (Singleton<CardDrawPiles>.Instance as Part3CardDrawPiles);
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
		
		if (Window.Button("Draw Side deck"))
		{
			Part3CardDrawPiles part1CardDrawPiles = (Singleton<CardDrawPiles>.Instance as Part3CardDrawPiles);
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
				Plugin.Log.LogError("Could not draw side deck. Can't find CardDrawPiles!");
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
}