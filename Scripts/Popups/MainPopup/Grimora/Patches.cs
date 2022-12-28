using System.Collections;
using System.Reflection;
using System.Reflection.Emit;
using DebugMenu.Scripts.All;
using DebugMenu.Scripts.Utils;
using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Regions;
using UnityEngine;

namespace DebugMenu.Scripts.Grimora;

[HarmonyPatch(typeof(SpecialNodeHandler), nameof(SpecialNodeHandler.StartSpecialNodeSequence), new Type[]{typeof(SpecialNodeData)})]
internal class SpecialNodeHandler_StartSpecialNodeSequence
{
	[HarmonyPostfix]
	private static bool Prefix(SpecialNodeData nodeData)
	{
		Helpers.LastSpecialNodeData = nodeData;
		return true;
	}
}