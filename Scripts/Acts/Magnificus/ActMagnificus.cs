using BepInEx.Logging;
using DebugMenu.Scripts.Acts;

namespace DebugMenu.Scripts.Magnificus;

public class ActMagnificus : BaseAct
{
	public ActMagnificus(ManualLogSource logger) : base(logger)
	{
	}

	public override void Update()
	{
		
	}
	
	public override void OnGUI()
	{
		GUIHelper.LabelHeader("Magnificus Act");
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