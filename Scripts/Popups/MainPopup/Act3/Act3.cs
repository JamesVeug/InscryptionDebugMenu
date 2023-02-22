using BepInEx.Logging;
using DebugMenu.Scripts.Acts;

namespace DebugMenu.Scripts.Act3;

public class Act3 : BaseAct
{
	public Act3(DebugWindow window) : base(window)
	{
	}

	public override void Update()
	{
		
	}
	
	public override void OnGUI()
	{
		Window.LabelHeader("Act 3");
		Window.Label("Support not started");
	}

	public override void OnGUIMinimal()
	{
		OnGUI();
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