using System.Collections;
using BepInEx.Logging;
using DebugMenu.Scripts.Acts;
using DiskCardGame;
using UnityEngine;

namespace DebugMenu.Scripts.Grimora;

public class CardBattleSequence
{
	private readonly ActGrimora Act;
	private readonly DebugWindow Window;

	public CardBattleSequence(ActGrimora act)
	{
		this.Act = act;
		this.Window = act.Window;
	}

	public void OnGUI()
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
		
		Window.Padding();
			
		if (Window.Button("Draw Card"))
		{
			GrimoraCardDrawPiles part1CardDrawPiles = (Singleton<CardDrawPiles>.Instance as GrimoraCardDrawPiles);
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
		
		if (Window.Button("Draw Side Deck"))
		{
			GrimoraCardDrawPiles part1CardDrawPiles = (Singleton<CardDrawPiles>.Instance as GrimoraCardDrawPiles);
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