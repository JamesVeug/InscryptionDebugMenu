using BepInEx.Logging;
using DebugMenu.Scripts.Acts;

namespace DebugMenu.Scripts.Grimora;

public class ActGrimora : BaseAct
{
	public ActGrimora(ManualLogSource logger) : base(logger)
	{
	}

	public override void Update()
	{
		
	}
	
	public override void OnGUI()
	{
		GUIHelper.LabelHeader("Grimora Act");
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