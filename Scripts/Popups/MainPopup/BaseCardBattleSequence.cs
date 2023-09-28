using System.Collections;
using DebugMenu.Scripts.Popups;
using DiskCardGame;
using InscryptionAPI.Helpers;
using UnityEngine;
using static DebugMenu.Scripts.DrawableGUI;

namespace DebugMenu.Scripts.Acts;

public abstract class BaseCardBattleSequence
{
	public bool IsGBCBattle() => SceneLoader.ActiveSceneName == "GBC_CardBattle";

	public abstract int PlayerBones { get; }
	public abstract int ScalesBalance { get; }
	public abstract int PlayerEnergy { get; }
	public abstract int PlayerMaxEnergy { get; }
	public abstract CardDrawPiles CardDrawPiles { get; }
	public CardDrawPiles3D CardDrawPiles3D => CardDrawPiles as CardDrawPiles3D;

	protected readonly DebugWindow Window;

	protected bool hasSideDeck = true;
	protected bool hasBones = true;

	public BaseCardBattleSequence(DebugWindow window)
	{
		this.Window = window;
	}

    private ButtonDisabledData DisableCardDraw() => new()
    {
        Disabled = (SaveManager.SaveFile.IsPart2 && !IsGBCBattle()) || (CardDrawPiles?.Deck?.CardsInDeck).GetValueOrDefault() == 0
    };
    private ButtonDisabledData DisableSideDraw() => new()
    {
        Disabled = !hasSideDeck || (CardDrawPiles3D?.SideDeck?.CardsInDeck).GetValueOrDefault() == 0
    };
    private ButtonDisabledData DisableTutorDraw() => new()
    {
        Disabled = (SaveManager.SaveFile.IsPart2 && !IsGBCBattle()) || (CardDrawPiles?.Deck?.CardsInDeck).GetValueOrDefault() == 0
    };

    public virtual void OnGUI()
	{
		using (Window.HorizontalScope(3))
		{
			if (Window.Button("Draw Card", disabled: DisableCardDraw))
				DrawCard();

			if (Window.Button("Draw Side", disabled: DisableSideDraw))
				DrawSideDeck();

			if (Window.Button("Draw Tutor", disabled: DisableTutorDraw))
				Plugin.Instance.StartCoroutine(DrawTutor());
		}

		using (Window.HorizontalScope(3))
		{
			Window.Label("Bones:\n" + PlayerBones);

			Func<ButtonDisabledData> disableBones = hasBones ? null : () => new DrawableGUI.ButtonDisabledData("No bones in this act");
			if (Window.Button("+5", disabled: disableBones))
				AddBones(5);

			if (Window.Button("-5", disabled: disableBones))
				RemoveBones(5);
		}

		using (Window.HorizontalScope(3))
		{
			Window.Label("Scales:\n" + ScalesBalance);

			if (Window.Button("Deal 2 Damage"))
				DealDamage(2);

			if (Window.Button("Take 2 Damage"))
				TakeDamage(2);
		}

		using (Window.HorizontalScope(4))
		{
			Window.Label($"Energy: \n{PlayerEnergy}\\{PlayerMaxEnergy}");

			if (Window.Button("-1"))
				RemoveEnergy(1);

			if (Window.Button("+1"))
				AddEnergy(1);

			if (Window.Button("Fill"))
				FillEnergy();
		}

		using (Window.HorizontalScope(4))
		{
			Window.Label("Max Energy");

			if (Window.Button("-1"))
				RemoveMaxEnergy(1);

			if (Window.Button("+1"))
				AddMaxEnergy(1);

			if (Window.Button("MAX"))
				SetMaxEnergyToMax();
		}

		if (Window.Button("Show Game Board"))
			Plugin.Instance.ToggleWindow<GameBoardPopup>();

		using (Window.HorizontalScope(2))
		{
			if (Window.Button("Auto-win battle"))
				AutoWinBattle();

			if (Window.Button("Auto-lose battle"))
				AutoLoseBattle();
		}
	}

	public IEnumerator DrawTutor()
	{
		if (Singleton<CardDrawPiles>.Instance.Deck.CardsInDeck > 0)
		{
			yield return Singleton<CardDrawPiles>.Instance.Deck.Tutor();
			if (ViewManager.Instance != null)
				Singleton<ViewManager>.Instance.Controller.LockState = ViewLockState.Unlocked;
		}
	}

	private IEnumerator AddEnergyAndIncreaseLimit(int amount)
	{
		ResourcesManager instance = ResourcesManager.Instance;
		if (instance.PlayerEnergy + amount > instance.PlayerMaxEnergy)
			yield return instance.AddMaxEnergy(amount);

		yield return instance.AddEnergy(amount);
	}
	public virtual void DrawCard()
	{
        if (CardDrawPiles != null)
        {
            if (CardDrawPiles.Deck.cards.Count > 0)
            {
                CardDrawPiles3D?.pile.Draw();

                Plugin.Instance.StartCoroutine(CardDrawPiles.DrawCardFromDeck());
            }
        }
        else
        {
            Plugin.Log.LogError("Couldn't draw from deck; can't find CardDrawPiles!");
        }
    }
	public virtual void DrawSideDeck()
	{
        if (CardDrawPiles3D != null)
        {
            if (CardDrawPiles3D.SideDeck.cards.Count > 0)
            {
                CardDrawPiles3D.SidePile.Draw();
                Plugin.Instance.StartCoroutine(CardDrawPiles3D.DrawFromSidePile());
            }
        }
        else
        {
            Plugin.Log.LogError("Couldn't draw from side deck; can't find CardDrawPiles3D!");
        }
    }
	public virtual void AddBones(int amount)
	{
        Plugin.Instance.StartCoroutine(ResourcesManager.Instance.AddBones(amount));
    }
	public virtual void RemoveBones(int amount)
	{
        int bones = Mathf.Min(ResourcesManager.Instance.PlayerBones, amount);
        Plugin.Instance.StartCoroutine(ResourcesManager.Instance.SpendBones(bones));
    }
	public virtual void SetMaxEnergyToMax()
	{
		if (ResourceDrone.m_Instance != null)
		{
            for (int i = ResourcesManager.Instance.PlayerMaxEnergy; i < 6; i++)
                Singleton<ResourceDrone>.Instance.OpenCell(i);
        }

        ResourcesManager.Instance.StartCoroutine(ResourcesManager.Instance.AddMaxEnergy(6));
    }
	public virtual void AddMaxEnergy(int amount)
	{
        ResourcesManager.Instance.StartCoroutine(ResourcesManager.Instance.AddMaxEnergy(amount));
    }
	public virtual void RemoveMaxEnergy(int amount)
	{
        ResourcesManager.Instance.StartCoroutine(ResourcesManager.Instance.RemoveMaxEnergy(amount));
    }
	public virtual void FillEnergy()
	{
        ResourcesManager.Instance.StartCoroutine(ResourcesManager.Instance.RefreshEnergy());
    }
	public virtual void AddEnergy(int amount)
	{
        ResourcesManager.Instance.StartCoroutine(AddEnergyAndIncreaseLimit(amount));
    }
    public virtual void RemoveEnergy(int amount)
	{
        ResourcesManager.Instance.StartCoroutine(ResourcesManager.Instance.SpendEnergy(amount));
    }

    public virtual void TakeDamage(int amount)
    {
        LifeManager lifeManager = Singleton<LifeManager>.Instance;
		if (Configs.DisablePlayerDamage)
		{
            lifeManager.PlayerDamage += amount;
			if (lifeManager.scales != null)
				Plugin.Instance.StartCoroutine(lifeManager.scales.AddDamage(amount, amount, true, null));
        }
        else
			Plugin.Instance.StartCoroutine(lifeManager.ShowDamageSequence(amount, 2, true, 0.125f, null, 0f, false));
    }
    public virtual void DealDamage(int amount)
    {
        LifeManager lifeManager = Singleton<LifeManager>.Instance;
		if (Configs.DisableOpponentDamage)
		{
			lifeManager.OpponentDamage += amount;
            if (lifeManager.scales != null)
                Plugin.Instance.StartCoroutine(lifeManager.scales.AddDamage(amount, amount, false, null));
        }
		else
			Plugin.Instance.StartCoroutine(lifeManager.ShowDamageSequence(amount, 2, false, 0.125f, null, 0f, false));
    }
    public virtual void AutoLoseBattle()
	{
        LifeManager lifeManager = Singleton<LifeManager>.Instance;
        int lifeLeft = Mathf.Abs(lifeManager.Balance - 5);
        if (Configs.DisablePlayerDamage)
            lifeManager.PlayerDamage += lifeLeft;
        else
            Plugin.Instance.StartCoroutine(lifeManager.ShowDamageSequence(lifeLeft, lifeLeft, true, 0.125f, null, 0f, false));
    }
	public virtual void AutoWinBattle()
	{
        LifeManager lifeManager = Singleton<LifeManager>.Instance;
        int lifeLeft = Mathf.Abs(lifeManager.Balance - 5);
        if (Configs.DisableOpponentDamage)
            lifeManager.OpponentDamage += lifeLeft;
        else
            Plugin.Instance.StartCoroutine(lifeManager.ShowDamageSequence(lifeLeft, lifeLeft, false, 0.125f, null, 0f, false));
    }
}
