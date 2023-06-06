using DebugMenu.Scripts.Popups;
using DiskCardGame;
using InscryptionAPI.Nodes;

namespace DebugMenu.Scripts.Acts;

public abstract class BaseCardBattleSequence
{
	public abstract int PlayerBones { get; }
	public abstract int ScalesBalance { get; }
	public abstract int PlayerEnergy { get; }
	public abstract int PlayerMaxEnergy { get; }
	
	protected readonly DebugWindow Window;
	
	protected bool hasSideDeck = true;
	protected bool hasBones = true;
	
	public BaseCardBattleSequence(DebugWindow window)
	{
		this.Window = window;
	}
	
	public virtual void OnGUI()
	{
		using (Window.HorizontalScope(3))
		{
			if (Window.Button("Draw Card"))
			{
				DrawCard();
			}

			Func<DrawableGUI.ButtonDisabledData> disabled = hasSideDeck ? default : () => new DrawableGUI.ButtonDisabledData("No Side deck in this act");
			if (Window.Button("Draw Side Deck", disabled:disabled))
			{
				DrawSideDeck();
			}

			if (Window.Button("Draw Tutor", disabled: () => new() { Disabled = !IsGBCBattle() || (Singleton<CardDrawPiles>.Instance?.Deck?.CardsInDeck).GetValueOrDefault() == 0 }))
			{
				Plugin.Instance.StartCoroutine(DrawTutor());
			}
		}

		using (Window.HorizontalScope(3))
		{
			Window.Label("Bones:\n" + PlayerBones);
			
			Func<DrawableGUI.ButtonDisabledData> disabled = hasBones ? default : () => new DrawableGUI.ButtonDisabledData("No bones in this act");
			if (Window.Button("+5", disabled: disabled))
			{
				AddBones(5);
			}

			if (Window.Button("-5", disabled: disabled))
			{
				RemoveBones(5);
			}
		}

		using (Window.HorizontalScope(3))
		{
			Window.Label("Scales:\n" + ScalesBalance);
			
			if (Window.Button("Deal 2 Damage"))
			{
				DealDamage(2);
			}

			if (Window.Button("Take 2 Damage"))
			{
				TakeDamage(2);
			}
		}

		using (Window.HorizontalScope(4))
		{
			Window.Label($"Energy: \n{PlayerEnergy}\\{PlayerMaxEnergy}");

			if (Window.Button("-1"))
			{
				RemoveEnergy(1);
			}

			if (Window.Button("+1"))
			{
				AddEnergy(1);
			}

			if (Window.Button("Fill"))
			{
				FillEnergy();
			}
		}

		using (Window.HorizontalScope(4))
		{
			Window.Label("Max Energy");

			if (Window.Button("-1"))
			{
				RemoveMaxEnergy(1);
			}

			if (Window.Button("+1"))
			{
				AddMaxEnergy(1);
			}

			if (Window.Button("MAX"))
			{
				SetMaxEnergyToMax();
			}
		}
		
		Window.Padding();

		using (Window.HorizontalScope(2))
		{
			if (Window.Button("Auto win battle"))
			{
				AutoWinBattle();
			}

			if (Window.Button("Auto lose battle"))
			{
				AutoLoseBattle();
			}
		}
	}
	
	public bool IsGBCBattle() => SceneLoader.ActiveSceneName == "GBC_CardBattle";
	public IEnumerator DrawTutor()
	{
		if (Singleton<CardDrawPiles>.Instance.Deck.CardsInDeck > 0)
		{
			yield return Singleton<CardDrawPiles>.Instance.Deck.Tutor();
		}
	}

	public abstract void DrawCard();
	public abstract void DrawSideDeck();
	public abstract void AddBones(int amount);
	public abstract void RemoveBones(int amount);

	public abstract void AutoLoseBattle();
	public abstract void AutoWinBattle();
	public abstract void SetMaxEnergyToMax();
	public abstract void AddMaxEnergy(int amount);
	public abstract void RemoveMaxEnergy(int amount);
	public abstract void FillEnergy();
	public abstract void AddEnergy(int amount);
	public abstract void RemoveEnergy(int amount);
	public abstract void TakeDamage(int amount);
	public abstract void DealDamage(int amount);
}