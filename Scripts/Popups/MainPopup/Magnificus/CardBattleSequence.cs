using System.Collections;
using BepInEx.Logging;
using DebugMenu.Scripts.Acts;
using DiskCardGame;
using UnityEngine;

namespace DebugMenu.Scripts.Magnificus;

public class CardBattleSequence : BaseCardBattleSequence
{
	public override int PlayerBones => MagnificusResourcesManager.Instance.PlayerBones;
	public override int ScalesBalance => MagnificusLifeManager.Instance.Balance;
	public override int PlayerEnergy => MagnificusResourcesManager.Instance.PlayerEnergy;
	public override int PlayerMaxEnergy => MagnificusResourcesManager.Instance.PlayerMaxEnergy;
	
	public CardBattleSequence(DebugWindow window) : base(window)
	{
	}
	
	public override void OnGUI()
	{
		base.OnGUI();
	}

	public override void AutoLoseBattle()
	{
		MagnificusLifeManager lifeManager = Singleton<MagnificusLifeManager>.Instance;
		Plugin.Instance.StartCoroutine(lifeManager.ShowLifeLoss(true, lifeManager.playerLife));
	}

	public override void AutoWinBattle()
	{
		MagnificusLifeManager lifeManager = Singleton<MagnificusLifeManager>.Instance;
		Plugin.Instance.StartCoroutine(lifeManager.ShowLifeLoss(false, lifeManager.opponentLife));
	}

	public override void SetMaxEnergyToMax()
	{
		for (int i = MagnificusResourcesManager.Instance.PlayerMaxEnergy; i < 6; i++)
		{
			Singleton<ResourceDrone>.Instance.OpenCell(i);
		}

		MagnificusResourcesManager.Instance.StartCoroutine(MagnificusResourcesManager.Instance.AddMaxEnergy(6));
	}

	public override void AddMaxEnergy(int amount)
	{
		MagnificusResourcesManager.Instance.StartCoroutine(MagnificusResourcesManager.Instance.AddMaxEnergy(amount));
	}

	public override void RemoveMaxEnergy(int amount)
	{
		MagnificusResourcesManager.Instance.StartCoroutine(MagnificusResourcesManager.Instance.AddMaxEnergy(-amount));
	}

	public override void FillEnergy()
	{
		MagnificusResourcesManager.Instance.StartCoroutine(MagnificusResourcesManager.Instance.RefreshEnergy());
	}

	public override void AddEnergy(int amount)
	{
		MagnificusResourcesManager.Instance.StartCoroutine(MagnificusResourcesManager.Instance.AddEnergy(amount));
	}

	public override void RemoveEnergy(int amount)
	{
		MagnificusResourcesManager.Instance.StartCoroutine(MagnificusResourcesManager.Instance.SpendEnergy(amount));
	}

	public override void TakeDamage(int amount)
	{
		MagnificusLifeManager lifeManager = Singleton<MagnificusLifeManager>.Instance;
		Plugin.Instance.StartCoroutine(lifeManager.ShowDamageSequence(amount, 2, true, 0.125f, null, 0f, false));
	}

	public override void DealDamage(int amount)
	{
		MagnificusLifeManager lifeManager = Singleton<MagnificusLifeManager>.Instance;
		Plugin.Instance.StartCoroutine(lifeManager.ShowDamageSequence(amount, 2, false, 0.125f, null, 0f, false));
	}

	public override void RemoveBones(int amount)
	{
		int bones = amount;
		if (Singleton<MagnificusResourcesManager>.Instance.PlayerBones < amount)
		{
			bones = Singleton<MagnificusResourcesManager>.Instance.PlayerBones;
		}

		Plugin.Instance.StartCoroutine(Singleton<MagnificusResourcesManager>.Instance.SpendBones(bones));
	}

	public override void AddBones(int amount)
	{
		Plugin.Instance.StartCoroutine(Singleton<MagnificusResourcesManager>.Instance.AddBones(amount));
	}

	public override void DrawSideDeck()
	{
		MagnificusCardDrawPiles part1CardDrawPiles = (Singleton<CardDrawPiles>.Instance as MagnificusCardDrawPiles);
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

	public override void DrawCard()
	{
		MagnificusCardDrawPiles part1CardDrawPiles = (Singleton<CardDrawPiles>.Instance as MagnificusCardDrawPiles);
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
}