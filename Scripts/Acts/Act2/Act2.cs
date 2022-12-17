using BepInEx.Logging;
using DebugMenu.Scripts.Acts;
using UnityEngine;

namespace DebugMenu.Scripts.Act2;

public class Act2 : BaseAct
{
	public Act2(ManualLogSource logger) : base(logger)
	{
	}

	public override void Update()
	{
		
	}
	
	public override void OnGUI()
	{
		GUIHelper.LabelHeader("Act 2");
		GUIHelper.Label("Support not started");
	}

	public override void OnGUIRestart()
	{
		// TODO:
	}

	public override void OnGUIReload()
	{
		// TODO:
	}
}