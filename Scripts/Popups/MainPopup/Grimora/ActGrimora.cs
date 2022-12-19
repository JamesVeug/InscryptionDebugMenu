using BepInEx.Logging;
using DebugMenu.Scripts.Acts;

namespace DebugMenu.Scripts.Grimora;

public class ActGrimora : BaseAct
{
	public ActGrimora(DebugWindow window) : base(window)
	{
	}

	public override void Update()
	{
		
	}
	
	public override void OnGUI()
	{
		Window.LabelHeader("Grimora Act");
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