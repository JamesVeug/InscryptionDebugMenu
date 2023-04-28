﻿using BepInEx.Logging;
using DebugMenu.Scripts.Acts;
using DebugMenu.Scripts.Popups;
using InscryptionAPI.Regions;
using UnityEngine;

namespace DebugMenu.Scripts.All;

public class AllActs : BaseAct
{
	public static bool blockAllInput = false;
	
	public static bool RegionOverride = false;
	public static string RegionNameOverride = "";  
	
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

		ButtonListPopup.OnGUI(Window, RegionNameOverride, "Override Region", RegionNameList, static (_, value, _)=>
		{
			RegionNameOverride = value;
		});
		Window.Toggle("Toggle Map Override", ref RegionOverride);
	}

	public override void OnGUIMinimal()
	{
		
	}

	private Tuple<List<string>, List<string>> RegionNameList()
	{
		List<string> regionsNames = RegionManager.AllRegionsCopy.ConvertAll((a) => a.name).ToList();
		return new Tuple<List<string>, List<string>>(regionsNames, regionsNames);
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