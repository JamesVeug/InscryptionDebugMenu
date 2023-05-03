using System.Collections;
using DebugMenu.Scripts.Popups;
using DiskCardGame;

namespace DebugMenu.Scripts.Sequences;

public abstract class BaseTriggerSequences
{
	public abstract string ButtonName { get; }
	public abstract void Sequence();
}

public abstract class SimpleTriggerSequences : BaseTriggerSequences
{
	public abstract NodeData NodeData { get; }
	public abstract Type NodeDataType { get; }
	public virtual GameState GameState => GameState.SpecialCardSequence;

	public override void Sequence()
	{
		Plugin.Instance.StartCoroutine(SequenceCoroutine());
	}
	
	public virtual IEnumerator SequenceCoroutine()
	{
		Singleton<GameFlowManager>.Instance.TransitionToGameState(GameState, NodeData);
		yield return null;
	}
}

public class StubSequence : SimpleTriggerSequences
{
	public override string ButtonName
	{
		get
		{
			// return name of NodeDataType separated by capital letters ignore NodeData
			// e.g. CardChoicesNodeData -> Card Choices
			string name = NodeDataType.Name;
			name = name.Replace("NodeData", "");
			
			for (int i = 1; i < name.Length; i++)
			{
				if (char.IsUpper(name[i]))
				{
					name = name.Insert(i, " ");
					i++;
				}
			}
			return name;
		}
	}

	public override NodeData NodeData => (NodeData)Activator.CreateInstance(NodeDataType);
	public override Type NodeDataType => type;
	public override GameState GameState => gameState;

	public Type type;
	public GameState gameState;
}

public class BossSequence : SimpleTriggerSequences
{
	public override string ButtonName => "Boss Battle";
	public override NodeData NodeData => null;
	public override Type NodeDataType => typeof(BossBattleNodeData);

	public override void Sequence()
	{
		TriggerCardBattleSequenceWindow window = Plugin.Instance.ToggleWindow<TriggerCardBattleSequenceWindow>();
		window.SelectedBattleType = TriggerCardBattleSequenceWindow.BattleType.BossBattle;
	}
}

public class CardBattleSequence : SimpleTriggerSequences
{
	public override string ButtonName => "Card Battle";
	public override NodeData NodeData => null;
	public override Type NodeDataType => typeof(CardBattleNodeData);

	public override void Sequence()
	{
		TriggerCardBattleSequenceWindow window = Plugin.Instance.ToggleWindow<TriggerCardBattleSequenceWindow>();
		window.SelectedBattleType = TriggerCardBattleSequenceWindow.BattleType.CardBattle;
	}
}

public class TotemBattleSequence : SimpleTriggerSequences
{
	public override string ButtonName => "Totem Battle";
	public override NodeData NodeData => null;
	public override Type NodeDataType => typeof(TotemBattleNodeData);

	public override void Sequence()
	{
		TriggerCardBattleSequenceWindow window = Plugin.Instance.ToggleWindow<TriggerCardBattleSequenceWindow>();
		window.SelectedBattleType = TriggerCardBattleSequenceWindow.BattleType.TotemBattle;
	}
}

public abstract class ThreeCardChoiceSequences : SimpleTriggerSequences
{
	public abstract CardChoicesType ChoiceType { get; }
	
	public override string ButtonName => $"3 {ChoiceType} Choice";
	public override Type NodeDataType => typeof(CardChoicesNodeData);
	public override NodeData NodeData
	{
		get
		{
			CardChoicesNodeData data = new CardChoicesNodeData();
			data.choicesType = ChoiceType;
			return data;
		}
	}
}

public class RandomChoiceSequences : ThreeCardChoiceSequences
{
	public override CardChoicesType ChoiceType => CardChoicesType.Random;
}

public class TribeChoiceSequences : ThreeCardChoiceSequences
{
	public override CardChoicesType ChoiceType => CardChoicesType.Tribe;
}

public class CostChoiceSequences : ThreeCardChoiceSequences
{
	public override CardChoicesType ChoiceType => CardChoicesType.Cost;
}

public class DeathcardChoiceSequences : ThreeCardChoiceSequences
{
	public override CardChoicesType ChoiceType => CardChoicesType.Deathcard;
}