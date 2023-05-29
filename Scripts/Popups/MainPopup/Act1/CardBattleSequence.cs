using System.Collections;
using BepInEx.Logging;
using DebugMenu.Scripts.Acts;
using DiskCardGame;
using UnityEngine;

namespace DebugMenu.Scripts.Act1;

public class CardBattleSequence
{
	private readonly Act1 Act;
	private readonly DebugWindow Window;

	public CardBattleSequence(Act1 act)
	{
		this.Act = act;
		this.Window = act.Window;
	}

	public void OnGUI()
	{
		MapNode nodeWithId =  Singleton<MapNodeManager>.m_Instance.GetNodeWithId(RunState.Run.currentNodeId);
		if (nodeWithId.Data is CardBattleNodeData cardBattleNodeData)
		{
			Window.Label($"Difficulty: {cardBattleNodeData.difficulty} + {RunState.Run.DifficultyModifier}");
		}

		using (Window.HorizontalScope(2))
		{
			if (Window.Button("Auto win battle"))
			{
				LifeManager lifeManager = Singleton<LifeManager>.Instance;
				int lifeLeft = Mathf.Abs(lifeManager.Balance - 5);
				Plugin.Instance.StartCoroutine(lifeManager.ShowDamageSequence(lifeLeft, lifeLeft, false, 0.125f, null,
					0f, false));
			}

			if (Window.Button("Auto lose battle"))
			{
				LifeManager lifeManager = Singleton<LifeManager>.Instance;
				int lifeLeft = Mathf.Abs(lifeManager.Balance - 5);
				Plugin.Instance.StartCoroutine(lifeManager.ShowDamageSequence(lifeLeft, lifeLeft, true, 0.125f, null,
					0f, false));
			}
		}

		using (Window.HorizontalScope(2))
		{
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

			if (Window.Button("Draw Side Deck"))
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
					Plugin.Log.LogError("Could not draw side deck. Can't find CardDrawPiles!");
				}
			}
		}

		using (Window.HorizontalScope(3))
		{
			Window.Label("Bones:\n" + Singleton<ResourcesManager>.Instance.PlayerBones);
			
			if (Window.Button("+5"))
			{
				Plugin.Instance.StartCoroutine(Singleton<ResourcesManager>.Instance.AddBones(5));
			}

			if (Window.Button("-5"))
			{
				int bones = 5;
				if (Singleton<ResourcesManager>.Instance.PlayerBones < 5)
				{
					bones = Singleton<ResourcesManager>.Instance.PlayerBones;
				}

				Plugin.Instance.StartCoroutine(Singleton<ResourcesManager>.Instance.SpendBones(bones));
			}
		}

		using (Window.HorizontalScope(3))
		{
			Window.Label("Scales:\n" + Singleton<LifeManager>.Instance.Balance);
			
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

		using (Window.HorizontalScope(4))
		{
			Window.Label($"Energy: \n{ResourcesManager.Instance.PlayerEnergy}\\{ResourcesManager.Instance.PlayerMaxEnergy}");

			if (Window.Button("-1"))
			{
				ResourcesManager.Instance.StartCoroutine(ResourcesManager.Instance.SpendEnergy(1));
			}

			if (Window.Button("+1"))
			{
				ResourcesManager.Instance.StartCoroutine(ResourcesManager.Instance.AddEnergy(1));
			}

			if (Window.Button("Fill"))
			{
				ResourcesManager.Instance.StartCoroutine(ResourcesManager.Instance.RefreshEnergy());
			}
		}

		using (Window.HorizontalScope(4))
		{
			Window.Label("Max Energy");

			if (Window.Button("-1"))
			{
				ResourcesManager.Instance.StartCoroutine(ResourcesManager.Instance.AddMaxEnergy(-1));
			}

			if (Window.Button("+1"))
			{
				ResourcesManager.Instance.StartCoroutine(ResourcesManager.Instance.AddMaxEnergy(1));
			}

			if (Window.Button("MAX"))
			{
				for (int i = ResourcesManager.Instance.PlayerMaxEnergy; i < 6; i++)
				{
					Singleton<ResourceDrone>.Instance.OpenCell(i);
				}
				ResourcesManager.Instance.StartCoroutine(ResourcesManager.Instance.AddMaxEnergy(6));
			}
		}
	}
}