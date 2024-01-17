using BepInEx.Logging;
using DebugMenu.Scripts.Acts;
using DebugMenu.Scripts.Utils;
using DiskCardGame;
using GBC;
using UnityEngine;

namespace DebugMenu.Scripts.Act2;

public class Act2 : BaseAct
{
	public Act2(DebugWindow window) : base(window)
	{
		m_mapSequence = new Act2MapSequence(this);
		m_cardBattleSequence = new Act2CardBattleSequence(window);
	}
	
	public override void OnGUI()
	{
		Window.LabelHeader("Act 2");
		Window.Padding();

		using (Window.HorizontalScope(3))
		{
			Window.Label("Currency: \n" + SaveData.Data.currency);
			if (Window.Button("+5"))
			{
				SaveData.Data.currency += 5;
			}

			if (Window.Button("-5"))
			{
				SaveData.Data.currency = Mathf.Max(0, SaveData.Data.currency - 5);
			}
		}

		Window.StartNewColumn();
		OnGUICurrentNode();
	}
	
	private void OnGUICurrentNode()
	{
		if (GBCEncounterManager.Instance?.EncounterOccurring ?? false)
		{
			Window.LabelHeader("Encounter");
			m_cardBattleSequence.OnGUI();
			return;
		}
		
		Window.Label("Unhandled state type");
	}

	public override void OnGUIMinimal()
	{
		OnGUICurrentNode();
	}

	public override void Restart()
	{
		// TODO:
	}

	public override void Reload()
	{
		// TODO:
	}
}