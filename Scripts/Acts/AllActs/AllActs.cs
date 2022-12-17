using BepInEx.Logging;
using DebugMenu.Scripts.Acts;
using UnityEngine;

namespace DebugMenu.Scripts.All;

public class AllActs : BaseAct
{
	public static bool blockAllInput = false;
	
	public AllActs(ManualLogSource logger) : base(logger)
	{
	}

	public override void Update()
	{
		
	}
	
	public override void OnGUI()
	{
		GUIHelper.Toggle("Block all Input", ref blockAllInput);
		
		if (GUIHelper.Button("0.1x"))
		{
			Log("Minimum Time Scale");
			SetTimeScale(0.1f);
		}
		if (GUIHelper.Button("1x"))
		{
			Log("Minimum Time Scale");
			SetTimeScale(1f);
		}
		if (GUIHelper.Button("5x"))
		{
			Log("Minimum Time Scale");
			SetTimeScale(5f);
		}
	}

	private void SetTimeScale(float speed)
	{
		Time.timeScale = speed;
		Time.fixedDeltaTime = Plugin.StartingFixedDeltaTime * Time.timeScale;
	}

	public override void OnGUIRestart()
	{
		// Nothing
	}

	public override void OnGUIReload()
	{
		// Nothing
	}
}