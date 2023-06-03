using System.Collections;
using BepInEx.Logging;
using DebugMenu.Scripts.Acts;
using DiskCardGame;
using UnityEngine;

namespace DebugMenu.Scripts.Act3;

public class CardBattleSequence : BaseCardBattleSequence
{
	public override int PlayerBones => ResourcesManager.Instance.PlayerBones;
	public override int ScalesBalance => LifeManager.Instance.Balance;
	public override int PlayerEnergy => ResourcesManager.Instance.PlayerEnergy;
	public override int PlayerMaxEnergy => ResourcesManager.Instance.PlayerMaxEnergy;
	
	public CardBattleSequence(DebugWindow window) : base(window)
	{
	}

	public override void OnGUI()
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

		base.OnGUI();
	}

	public override void DrawCard()
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

	public override void DrawSideDeck()
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

	public override void AddBones(int amount)
	{
		
	}

	public override void RemoveBones(int amount)
	{
		
	}

	public override void AutoLoseBattle()
	{
		LifeManager lifeManager = Singleton<LifeManager>.Instance;
		int lifeLeft = Mathf.Abs(lifeManager.Balance - 5);
		Plugin.Instance.StartCoroutine(lifeManager.ShowDamageSequence(lifeLeft, lifeLeft, true, 0.125f, null,
			0f, false));
	}

	public override void AutoWinBattle()
	{
		LifeManager lifeManager = Singleton<LifeManager>.Instance;
		int lifeLeft = Mathf.Abs(lifeManager.Balance - 5);
		Plugin.Instance.StartCoroutine(lifeManager.ShowDamageSequence(lifeLeft, lifeLeft, false, 0.125f, null,
			0f, false));
	}

	public override void SetMaxEnergyToMax()
	{
		for (int i = ResourcesManager.Instance.PlayerMaxEnergy; i < 6; i++)
		{
			Singleton<ResourceDrone>.Instance.OpenCell(i);
		}
		ResourcesManager.Instance.StartCoroutine(ResourcesManager.Instance.AddMaxEnergy(6));
	}

	public override void AddMaxEnergy(int amount)
	{
		ResourcesManager.Instance.StartCoroutine(ResourcesManager.Instance.AddMaxEnergy(amount));
	}

	public override void RemoveMaxEnergy(int amount)
	{
		ResourcesManager.Instance.StartCoroutine(ResourcesManager.Instance.AddMaxEnergy(-amount));
	}

	public override void FillEnergy()
	{
		ResourcesManager.Instance.StartCoroutine(ResourcesManager.Instance.RefreshEnergy());
	}

	public override void AddEnergy(int amount)
	{
		ResourcesManager.Instance.StartCoroutine(ResourcesManager.Instance.AddEnergy(amount));
	}

	public override void RemoveEnergy(int amount)
	{
		ResourcesManager.Instance.StartCoroutine(ResourcesManager.Instance.SpendEnergy(amount));
	}

	public override void TakeDamage(int amount)
	{
		LifeManager lifeManager = Singleton<LifeManager>.Instance;
		Plugin.Instance.StartCoroutine(lifeManager.ShowDamageSequence(amount, 2, true, 0.125f, null, 0f, false));
	}

	public override void DealDamage(int amount)
	{
		LifeManager lifeManager = Singleton<LifeManager>.Instance;
		Plugin.Instance.StartCoroutine(lifeManager.ShowDamageSequence(amount, 2, false, 0.125f, null, 0f, false));
	}
}