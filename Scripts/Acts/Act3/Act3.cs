using BepInEx.Logging;
using DebugMenu.Scripts.Acts;

namespace DebugMenu.Scripts.Act3;

public class Act3 : BaseAct
{
	public Act3(ManualLogSource logger) : base(logger)
	{
	}

	public override void Update()
	{
		
	}
	
	public override void OnGUI()
	{
		GUIHelper.LabelHeader("Act 3");
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