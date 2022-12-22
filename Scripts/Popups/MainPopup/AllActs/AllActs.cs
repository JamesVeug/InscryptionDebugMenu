﻿using BepInEx.Logging;
using DebugMenu.Scripts.Acts;
using DebugMenu.Scripts.Popups;
using UnityEngine;

namespace DebugMenu.Scripts.All;

public class AllActs : BaseAct
{
	public static bool blockAllInput = false;
	
	public AllActs(DebugWindow window) : base(window)
	{
	}

	public override void Update()
	{
		
	}
	
	public override void OnGUI()
	{
		Window.Toggle("Block all Input", ref blockAllInput);

		if (Window.Button("Show Game Info"))
		{
			Plugin.Instance.ToggleWindow<GameInfoPopup>();
		}
		
		if (Window.Button("0.1x"))
		{
			Log("Minimum Time Scale");
			SetTimeScale(0.1f);
		}
		if (Window.Button("1x"))
		{
			Log("Minimum Time Scale");
			SetTimeScale(1f);
		}
		if (Window.Button("5x"))
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