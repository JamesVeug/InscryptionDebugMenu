using System.Collections;
using BepInEx.Logging;
using DebugMenu.Scripts.Acts;
using DiskCardGame;
using GBC;
using UnityEngine;

namespace DebugMenu.Scripts.Act2;

public class CardBattleSequence
{
	private readonly Act2 Act;
	private readonly DebugWindow Window;

	public CardBattleSequence(Act2 act)
	{
		this.Act = act;
		this.Window = act.Window;
	}

	public void OnGUI()
	{			
		if (Window.Button("Draw Card"))
		{
			PixelCardDrawPiles drawPile = (Singleton<PixelCardDrawPiles>.Instance as PixelCardDrawPiles);
			if (drawPile)
			{
				if (drawPile.Deck.cards.Count > 0)
				{
					//drawPile.DrawCardFromDeck();
					Plugin.Instance.StartCoroutine(drawPile.DrawCardFromDeck());
				}
			}
			else
			{
				Plugin.Log.LogError("Could not draw card. Can't find CardDrawPiles!");
			}
		}

		using (Window.HorizontalScope(3))
		{
			Window.Label("Bones:\n" + Singleton<ResourcesManager>.Instance.PlayerBones);
			
			if (Window.Button("+5 bones"))
			{
				Plugin.Instance.StartCoroutine(Singleton<PixelResourcesManager>.Instance.AddBones(5));
			}

			if (Window.Button("-5 bones"))
			{
				int bones = 5;
				if (Singleton<PixelResourcesManager>.Instance.PlayerBones < 5)
				{
					bones = Singleton<PixelResourcesManager>.Instance.PlayerBones;
				}

				Plugin.Instance.StartCoroutine(Singleton<PixelResourcesManager>.Instance.SpendBones(bones));
			}
		}

		using (Window.HorizontalScope(3))
		{
			Window.Label("Scales:\n" + Singleton<PixelLifeManager>.Instance.Balance);
			
			if (Window.Button("Deal 2"))
			{
				PixelLifeManager lifeManager = Singleton<PixelLifeManager>.Instance;
				Plugin.Instance.StartCoroutine(lifeManager.ShowDamageSequence(2, 2, false, 0.125f, null, 0f, false));
			}

			if (Window.Button("Take 2"))
			{
				PixelLifeManager lifeManager = Singleton<PixelLifeManager>.Instance;
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
		}

		Window.Padding();
		
		using (Window.HorizontalScope(2))
		{
			if (Window.Button("Auto win battle"))
			{
				PixelLifeManager lifeManager = Singleton<PixelLifeManager>.Instance;
				int lifeLeft = Mathf.Abs(lifeManager.Balance - 5);
				Plugin.Instance.StartCoroutine(lifeManager.ShowDamageSequence(lifeLeft, lifeLeft, false, 0.125f, null,
					0f, false));
			}

			if (Window.Button("Auto lose battle"))
			{
				PixelLifeManager lifeManager = Singleton<PixelLifeManager>.Instance;
				int lifeLeft = Mathf.Abs(lifeManager.Balance - 5);
				Plugin.Instance.StartCoroutine(lifeManager.ShowDamageSequence(lifeLeft, lifeLeft, true, 0.125f, null,
					0f, false));
			}
		}
	}
}