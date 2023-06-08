using System.Reflection;
using BepInEx;
using BepInEx.Bootstrap;
using DebugMenu.Scripts.Sequences;
using DiskCardGame;
using InscryptionAPI;
using InscryptionAPI.Nodes;
using UnityEngine;

namespace DebugMenu.Scripts.Utils;

public static partial class Helpers
{
	public static List<ABaseTriggerSequences> Sequences
	{
		get
		{
			if (m_sequences == null)
			{
				m_sequences = GetAllSequences();
			}

			return m_sequences;
		}
	}

	private static List<ABaseTriggerSequences> m_sequences = null;

	private static List<ABaseTriggerSequences> GetAllSequences()
	{
		List<ABaseTriggerSequences> list = new List<ABaseTriggerSequences>();

		// get all types that override ForceTriggerSequences
		Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
		Dictionary<Assembly, List<Type>> nodeTypes = new Dictionary<Assembly, List<Type>>();
		Dictionary<Assembly, Type> assemblyToPluginType = new Dictionary<Assembly, Type>();
		foreach (Assembly a in assemblies)
		{
			foreach (Type type in a.GetTypes())
			{
				if (type.IsAbstract)
				{
					continue;
				}

				if (type.IsSubclassOf(typeof(ABaseTriggerSequences)))
				{
					if (type != typeof(StubSequence) && type != typeof(APIModdedSequence) && type != typeof(ModdedStubSequence))
					{
						ABaseTriggerSequences sequence = (ABaseTriggerSequences)Activator.CreateInstance(type);
						list.Add(sequence);
					}
				}
				else if (type.IsSubclassOf(typeof(BaseUnityPlugin)))
				{
					assemblyToPluginType[a] = type;
				}
				else if (type.IsSubclassOf(typeof(NodeData)))
				{
					if(nodeTypes.TryGetValue(a, out List<Type> types))
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
		foreach (KeyValuePair<Assembly,List<Type>> pair in nodeTypes)
		{
			foreach (Type type in pair.Value)
			{
				bool hasOverride = false;
				foreach (ABaseTriggerSequences sequence in list)
				{
					if (sequence is not SimpleTriggerSequences simpleTriggerSequences)
					{
						continue;
					}

					if (simpleTriggerSequences.NodeDataType == type)
					{
						hasOverride = true;
						break;
					}
				}

				if (hasOverride)
				{
					continue;
				}

				if (assemblyToPluginType.TryGetValue(pair.Key, out Type pluginType))
				{
					// A mod added this node type
					// Get the plugin and find their guid
					// Then add it to the list
					BaseUnityPlugin plugin = (BaseUnityPlugin)Plugin.Instance.GetComponent(pluginType);
					ModdedStubSequence sequence = new ModdedStubSequence();
					sequence.ModGUID = plugin.Info.Metadata.GUID;
					sequence.type = type;
					sequence.gameState = GameState.SpecialCardSequence;
					list.Add(sequence);
				}
				else
				{
					// This is a vanilla node type
					StubSequence sequence = new StubSequence();
					sequence.type = type;
					sequence.gameState = type.IsAssignableFrom(typeof(CardBattleNodeData))
						? GameState.CardBattle
						: GameState.SpecialCardSequence;
					list.Add(sequence);
				}
			}
		}

		foreach (NewNodeManager.FullNode addedNode in NewNodeManager.NewNodes)
		{
			if (addedNode == null)
			{
				continue;
			}
			
			APIModdedSequence sequence = new APIModdedSequence();
			sequence.CustomNodeData = addedNode;
			list.Add(sequence);
		}

		list.Sort(SortSequences);

		return list;
	}

	private static int SortSequences(ABaseTriggerSequences a, ABaseTriggerSequences b)
	{
		// Sort so we have this ordering
		// Vanilla sigils first
		// - sort by button name
		// Modded sigils second
		// - sort by GUID
		// - sort by button name

		// sort so types that are not StubModdedSequence are first then sort by ButtonName
		if (a is IModdedSequence aModded)
		{
			if (b is IModdedSequence bModded)
			{
				int sortbyGUID = String.Compare(aModded.ModGUID, bModded.ModGUID, StringComparison.Ordinal);
				if (sortbyGUID != 0)
				{
					return sortbyGUID;
				}

				int sortByButtonName = String.Compare(a.ButtonName, b.ButtonName, StringComparison.Ordinal);
				return sortByButtonName;
			}

			return 1;
		}

		if (b is IModdedSequence)
		{
			return -1;
		}

		// Vanilla sigils - sort by button name
		return String.Compare(a.ButtonName, b.ButtonName, StringComparison.Ordinal);
	}
}