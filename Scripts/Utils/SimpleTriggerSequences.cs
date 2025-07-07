using DebugMenu.Scripts.Magnificus;
using DebugMenu.Scripts.Popups;
using DiskCardGame;
using InscryptionAPI.Nodes;
using System.Collections;

namespace DebugMenu.Scripts.Sequences;

public abstract class BaseTriggerSequence
{
    public abstract string SequenceName { get; }
    public string ModGUID { get; set; }
    public string ButtonName
    {
        get
        {
            if (ModGUID == null)
                return SequenceName;

            return $"{SequenceName}\n{ModGUID}";
        }
    }
    public abstract void Sequence();
}
/// <summary>
/// Sequences that simply transition to a new game state.
/// </summary>
public abstract class SimpleTriggerSequences : BaseTriggerSequence
{
    public abstract NodeData NodeData { get; }
    public abstract Type NodeDataType { get; }
    public virtual GameState GameState => GameState.SpecialCardSequence;

    public override void Sequence() => Plugin.Instance.StartCoroutine(SequenceCoroutine());
    public virtual IEnumerator SequenceCoroutine()
    {
        Singleton<GameFlowManager>.Instance.TransitionToGameState(GameState, NodeData);
        yield break;
    }
}

/// <summary>
/// Custom sequences added by mods using the API
/// </summary>
public class APIModdedSequence : BaseTriggerSequence
{
    public NewNodeManager.FullNode CustomNodeData;
    public override string SequenceName => CustomNodeData.name;
    public override void Sequence()
    {
        CustomSpecialNodeData nodeData = new(CustomNodeData);
        Singleton<GameFlowManager>.Instance.TransitionToGameState(GameState.SpecialCardSequence, nodeData);
    }
}

/// <summary>
/// All other Sequences that are a simple "Create node and trigger"
/// </summary>
public class SimpleStubSequence : SimpleTriggerSequences
{
    public override string SequenceName
    {
        get
        {
            // return name of NodeDataType separated by capital letters ignore NodeData
            // e.g. CardChoicesNodeData -> Card Choices
            string name = NodeDataType.Name.Replace("NodeData", "");
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

    public override IEnumerator SequenceCoroutine()
    {
        if (MagnificusModHelper.Enabled && SaveManager.SaveFile.IsMagnificus)
        {
            MagnificusModHelper.HandleSequences(GameState, NodeData);
            yield break;
        }

        yield return base.SequenceCoroutine();
    }
}

public class BossSequence : SimpleTriggerSequences
{
    public override string SequenceName => "Boss Battle";
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
    public override string SequenceName => "Card Battle";
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
    public override string SequenceName => "Totem Battle";
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

    public override string SequenceName => $"3 {ChoiceType} Choice";
    public override Type NodeDataType => typeof(CardChoicesNodeData);
    public override NodeData NodeData
    {
        get
        {
            CardChoicesNodeData data = new()
            {
                choicesType = ChoiceType
            };
            return data;
        }
    }

    public override IEnumerator SequenceCoroutine()
    {
        if (MagnificusModHelper.Enabled && SaveManager.SaveFile.IsMagnificus)
        {
            MagnificusModHelper.HandleCardChoices(ChoiceType);
            yield break;
        }

        yield return base.SequenceCoroutine();
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

public class DeathCardChoiceSequences : ThreeCardChoiceSequences
{
    public override CardChoicesType ChoiceType => CardChoicesType.Deathcard;
}