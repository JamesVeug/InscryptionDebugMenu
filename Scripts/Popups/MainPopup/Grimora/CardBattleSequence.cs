﻿using System.Collections;
using BepInEx.Logging;
using DebugMenu.Scripts.Acts;
using DiskCardGame;
using UnityEngine;

namespace DebugMenu.Scripts.Grimora;

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
		TurnManager turnManager = Singleton<TurnManager>.Instance;
		Window.Label("Opponent: " + turnManager.opponent);
		Window.Label("Difficulty: " + turnManager.opponent.Difficulty);
		Window.Label("Blueprint: " + turnManager.opponent.Blueprint.name);

		base.OnGUI();
	}

	public override void DrawCard()
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

	public override void DrawSideDeck()
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

	public override void AddBones(int amount)
	{
		Plugin.Instance.StartCoroutine(Singleton<ResourcesManager>.Instance.AddBones(amount));
	}

	public override void RemoveBones(int amount)
	{
		int bones = amount;
		if (Singleton<ResourcesManager>.Instance.PlayerBones < amount)
		{
			bones = Singleton<ResourcesManager>.Instance.PlayerBones;
		}

		Plugin.Instance.StartCoroutine(Singleton<ResourcesManager>.Instance.SpendBones(bones));
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