using BepInEx;
using DebugMenu.Scripts.Magnificus;
using DebugMenu.Scripts.Sequences;
using DiskCardGame;
using InscryptionAPI.Nodes;
using System.Reflection;

namespace DebugMenu.Scripts.Utils;
// card choices, card battles
public static partial class Helpers
{
    public static List<BaseTriggerSequence> ShownSequences => Configs.ShowAllSequences ? AllSequences : CurrentActSequences;
    public static List<BaseTriggerSequence> CurrentActSequences => GetCurrentSavedAct() switch
    {
        Acts.Act1 => GetAct1Sequences(),
        Acts.Act3 => GetAct3Sequences(),
        Acts.GrimoraAct => GetGrimoraSequences(),
        Acts.MagnificusAct => GetMagnificusSequences(),
        _ => AllSequences
    };
    public static List<BaseTriggerSequence> AllSequences
    {
        get
        {
            m_sequences ??= GetAllSequences();
            return m_sequences;
        }
    }

    private static List<BaseTriggerSequence> c_sequences = null;
    private static List<BaseTriggerSequence> m_sequences = null;

    private static List<BaseTriggerSequence> GetAllSequences()
    {
        List<BaseTriggerSequence> list = new();

        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
        Dictionary<Assembly, List<Type>> nodeTypes = new();
        Dictionary<Assembly, Type> assemblyToPluginType = new();
        foreach (Assembly a in assemblies)
        {
            foreach (Type type in a.GetTypes())
            {
                if (type.IsAbstract)
                    continue;

                if (type.IsSubclassOf(typeof(BaseTriggerSequence)))
                {
                    if (type != typeof(APIModdedSequence) && !type.IsAssignableFrom(typeof(SimpleStubSequence)))
                    {
                        BaseTriggerSequence sequence = (BaseTriggerSequence)Activator.CreateInstance(type);
                        list.Add(sequence);
                    }
                }
                else if (type.IsSubclassOf(typeof(BaseUnityPlugin)))
                {
                    assemblyToPluginType[a] = type;
                }
                else if (type.IsSubclassOf(typeof(NodeData)))
                {
                    if (nodeTypes.TryGetValue(a, out List<Type> types))
                    {
                        types.Add(type);
                    }
                    else
                    {
                        nodeTypes[a] = new List<Type>() { type };
                    }
                }
            }
        }

        // get all types that override SpecialNodeData
        foreach (KeyValuePair<Assembly, List<Type>> pair in nodeTypes)
        {
            foreach (Type type in pair.Value)
            {
                bool hasOverride = false;
                foreach (BaseTriggerSequence sequence in list)
                {
                    if (sequence is not SimpleTriggerSequences simpleTriggerSequences)
                        continue;

                    if (simpleTriggerSequences.NodeDataType == type)
                    {
                        hasOverride = true;
                        break;
                    }
                }

                if (hasOverride)
                    continue;

                if (assemblyToPluginType.TryGetValue(pair.Key, out Type pluginType))
                {
                    // A mod added this node type; get the plugin and find their GUID, then add it to the list
                    BaseUnityPlugin plugin = (BaseUnityPlugin)Plugin.Instance.GetComponent(pluginType);
                    SimpleStubSequence sequence = new()
                    {
                        ModGUID = plugin.Info.Metadata.GUID,
                        type = type,
                        gameState = GameState.SpecialCardSequence
                    };
                    list.Add(sequence);
                }
                else
                {
                    // This is a vanilla node type
                    SimpleStubSequence sequence = new()
                    {
                        ModGUID = null,
                        type = type,
                        gameState = type.IsAssignableFrom(typeof(CardBattleNodeData)) ? GameState.CardBattle : GameState.SpecialCardSequence
                    };
                    list.Add(sequence);
                }
            }
        }

        foreach (NewNodeManager.FullNode addedNode in NewNodeManager.NewNodes)
        {
            if (addedNode == null)
                continue;

            APIModdedSequence sequence = new()
            {
                ModGUID = addedNode.guid.IsNullOrWhiteSpace() ? "no guid" : addedNode.guid,
                CustomNodeData = addedNode
            };
            list.Add(sequence);
        }
        list.Remove(list.Find(x => x.SequenceName == "Special"));
        list.Remove(list.Find(x => x.SequenceName == "Custom Special"));
        list.Sort(SortSequences);
        return list;
    }

    private static int SortSequences(BaseTriggerSequence a, BaseTriggerSequence b)
    {
        // Sort so we have this ordering
        // 1. Vanilla sequences
        // - a. sort by ButtonName
        // 2. Modded sigils
        // - a. sort by GUID
        // - b. sort by ButtonName
        if (!a.ModGUID.IsNullOrWhiteSpace())
        {
            if (!b.ModGUID.IsNullOrWhiteSpace())
            {
                int sortbyGUID = String.Compare(a.ModGUID, b.ModGUID, StringComparison.Ordinal);
                if (sortbyGUID != 0)
                    return sortbyGUID;

                int sortByButtonName = String.Compare(a.SequenceName, b.SequenceName, StringComparison.Ordinal); // same GUID
                return sortByButtonName;
            }
            return 1;
        }

        if (!b.ModGUID.IsNullOrWhiteSpace())
            return -1;

        // Vanilla sequence - sort by ButtonName
        return String.Compare(a.SequenceName, b.SequenceName, StringComparison.Ordinal);
    }

    private static List<BaseTriggerSequence> m_act1Sequences = null;
    private static List<BaseTriggerSequence> m_act3Sequences = null;
    private static List<BaseTriggerSequence> m_grimoraSequences = null;
    private static List<BaseTriggerSequence> m_mag_sequences = null;

    private static List<BaseTriggerSequence> GetAct1Sequences()
    {
        // add card ability, attach gem, build a card, card bundle, creature transformer, modify side, overclock card, recycle card, unlock part3
        if (m_act1Sequences == null)
        {
            List<BaseTriggerSequence> sequences = new(AllSequences);
            sequences.Remove(sequences.Find(x => x.SequenceName == "Add Card Ability"));
            sequences.Remove(sequences.Find(x => x.SequenceName == "Attach Gem"));
            sequences.Remove(sequences.Find(x => x.SequenceName == "Card Bundle Choices"));
            sequences.Remove(sequences.Find(x => x.SequenceName == "Create Transformer"));
            sequences.Remove(sequences.Find(x => x.SequenceName == "Modify Side Deck"));
            sequences.Remove(sequences.Find(x => x.SequenceName == "Overclock Card"));
            sequences.Remove(sequences.Find(x => x.SequenceName == "Recycle Card"));
            sequences.Remove(sequences.Find(x => x.SequenceName == "Special"));
            sequences.Remove(sequences.Find(x => x.SequenceName == "Unlock Part3 Item"));
            m_act1Sequences = sequences;
        }
        return m_act1Sequences;
    }
    private static List<BaseTriggerSequence> GetAct3Sequences()
    {
        if (m_act3Sequences == null)
        {
            List<BaseTriggerSequence> sequences = new(AllSequences);
            List<BaseTriggerSequence> currentSeq = new()
            {
                sequences.Find(x => x.SequenceName == "3 Deathcard Choice"),
                sequences.Find(x => x.SequenceName == "3 Random Choice"),
                sequences.Find(x => x.SequenceName == "Add Card Ability"),
                sequences.Find(x => x.SequenceName == "Attach Gem"),
                sequences.Find(x => x.SequenceName == "Card Battle"),
                sequences.Find(x => x.SequenceName == "Boss Battle"),
                sequences.Find(x => x.SequenceName == "Create Transformer"),
                sequences.Find(x => x.SequenceName == "Modify Side Deck"),
                sequences.Find(x => x.SequenceName == "Overclock Card"),
                sequences.Find(x => x.SequenceName == "Recycle Card"),
                sequences.Find(x => x.SequenceName == "Trade Cards"),
                sequences.Find(x => x.SequenceName == "Unlock Part3 Item"),
            };
            currentSeq.AddRange(sequences.Where(x => !string.IsNullOrEmpty(x.ModGUID)));
            m_act3Sequences = currentSeq;
        }
        return m_act3Sequences;
    }
    private static List<BaseTriggerSequence> GetGrimoraSequences()
    {
        if (m_grimoraSequences == null)
        {
            List<BaseTriggerSequence> sequences = new(AllSequences);
            List<BaseTriggerSequence> currentSeq = new()
            {
                sequences.Find(x => x.SequenceName == "3 Random Choice"),
                sequences.Find(x => x.SequenceName == "Card Battle"),
                sequences.Find(x => x.SequenceName == "Boss Battle"),
                sequences.Find(x => x.SequenceName == "Choose Rare Card")
            };
            currentSeq.AddRange(sequences.Where(x => !string.IsNullOrEmpty(x.ModGUID)));
            m_grimoraSequences = currentSeq;
        }
        return m_grimoraSequences;
    }

    private static List<BaseTriggerSequence> GetMagnificusSequences()
    {
        if (m_mag_sequences == null)
        {
            List<BaseTriggerSequence> sequences = new(AllSequences);
            List<BaseTriggerSequence> currentSeq = new()
            {
                sequences.Find(x => x.SequenceName == "3 Random Choice"),
                sequences.Find(x => x.SequenceName == "3 Cost Choice"),
                sequences.Find(x => x.SequenceName == "Card Battle"),
                sequences.Find(x => x.SequenceName == "Boss Battle"),
                sequences.Find(x => x.SequenceName == "Choose Rare Card"),
                sequences.Find(x => x.SequenceName == "Trade Pelts"),
            };
            currentSeq.AddRange(sequences.Where(x => x.ModGUID == MagnificusModHelper.Guid));
            m_mag_sequences = currentSeq;
        }
        return m_mag_sequences;
    }
}