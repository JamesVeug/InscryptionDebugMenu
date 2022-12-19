using BepInEx.Logging;
using DebugMenu.Scripts.Acts;
using UnityEngine;

namespace DebugMenu.Scripts.Act2;

public class Act2 : BaseAct
{
	public Act2(DebugWindow window) : base(window)
	{
	}

	public override void Update()
	{
		
	}
	
	public override void OnGUI()
	{
		Window.LabelHeader("Act 2");
		Window.Label("Support not started");
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