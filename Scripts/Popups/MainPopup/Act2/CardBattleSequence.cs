using DebugMenu.Scripts.Acts;
using DiskCardGame;
using GBC;
using UnityEngine;

namespace DebugMenu.Scripts.Act2;

public class CardBattleSequence : BaseCardBattleSequence
{
    public override int PlayerBones => IsGBCBattle() ? PixelResourcesManager.Instance.PlayerBones : 0;
    public override int ScalesBalance => IsGBCBattle() ? PixelLifeManager.Instance.Balance : 0;
    public override int PlayerEnergy => IsGBCBattle() ? PixelResourcesManager.Instance.PlayerEnergy : 0;
    public override int PlayerMaxEnergy => IsGBCBattle() ? PixelResourcesManager.Instance.PlayerMaxEnergy : 0;

    public CardBattleSequence(DebugWindow window) : base(window)
    {
        hasSideDeck = false;
    }

	public override void DrawCard()
	{
        PixelCardDrawPiles drawPile = PixelCardDrawPiles.Instance as PixelCardDrawPiles;
		if (drawPile)
		{
            if (drawPile.Deck.CardsInDeck > 0)
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

    public override void DrawSideDeck()
    {

    }

    public override void AddBones(int amount)
    {
        Plugin.Instance.StartCoroutine(Singleton<PixelResourcesManager>.Instance.AddBones(amount));
    }

    public override void RemoveBones(int amount)
    {
        int bones = amount;
        if (Singleton<PixelResourcesManager>.Instance.PlayerBones < amount)
        {
            bones = Singleton<PixelResourcesManager>.Instance.PlayerBones;
        }

        Plugin.Instance.StartCoroutine(Singleton<PixelResourcesManager>.Instance.SpendBones(bones));
    }

    public override void AutoLoseBattle()
    {
        PixelLifeManager lifeManager = Singleton<PixelLifeManager>.Instance;
        int lifeLeft = Mathf.Abs(lifeManager.Balance - 5);
        Plugin.Instance.StartCoroutine(lifeManager.ShowDamageSequence(lifeLeft, lifeLeft, true, 0.125f, null,
            0f, false));
    }

    public override void AutoWinBattle()
    {
        PixelLifeManager lifeManager = Singleton<PixelLifeManager>.Instance;
        int lifeLeft = Mathf.Abs(lifeManager.Balance - 5);
        Plugin.Instance.StartCoroutine(lifeManager.ShowDamageSequence(lifeLeft, lifeLeft, false, 0.125f, null,
            0f, false));
    }

    public override void SetMaxEnergyToMax()
    {

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
        PixelLifeManager lifeManager = Singleton<PixelLifeManager>.Instance;
        Plugin.Instance.StartCoroutine(lifeManager.ShowDamageSequence(amount, 2, true, 0.125f, null, 0f, false));
    }

    public override void DealDamage(int amount)
    {
        PixelLifeManager lifeManager = Singleton<PixelLifeManager>.Instance;
        Plugin.Instance.StartCoroutine(lifeManager.ShowDamageSequence(amount, 2, false, 0.125f, null, 0f, false));
    }
}