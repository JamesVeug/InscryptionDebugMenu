﻿using DebugMenu.Scripts.All;
using DebugMenu.Scripts.Grimora;
using DebugMenu.Scripts.Magnificus;
using DebugMenu.Scripts.Popups;
using DebugMenu.Scripts.Utils;
using DiskCardGame;
using UnityEngine;

namespace DebugMenu.Scripts.Acts;

public class DebugWindow : BaseWindow
{
	public enum ToggleStates
	{
		Off, Minimal, All
	}
	
	public override string PopupName => "Debug Menu";
	public override Vector2 Size => new(650, 420);
	public override bool ClosableWindow => false;
	public BaseAct CurrentAct => currentAct;
	public AllActs AllActs => allActs;

	private BaseAct currentAct = null;
	private ToggleStates currentState = ToggleStates.Off;

	private readonly AllActs allActs;
	private readonly Act1.Act1 act1;
	private readonly Act2.Act2 act2;
	private readonly Act3.Act3 act3;
	private ActGrimora actGrimora;
	private ActMagnificus actMagnificus;
	private Vector2 position;

	public DebugWindow() : base()
	{
		allActs = new AllActs(this);
		act1 = new Act1.Act1(this);
		act2 = new Act2.Act2(this);
		act3 = new Act3.Act3(this);
		actGrimora = new ActGrimora(this);
		actMagnificus = new ActMagnificus(this);
		IsActive = true;
		isOpen = false;
	}

	public override void Update()
	{
		currentAct = Helpers.GetCurrentSavedAct() switch
		{
			Helpers.Acts.Act1 => act1,
			Helpers.Acts.Act2 => act2,
			Helpers.Acts.Act3 => act3,
			Helpers.Acts.GrimoraAct => actGrimora,
			Helpers.Acts.MagnificusAct => actMagnificus,
			_ => null,
		};
        currentAct?.Update();
    }

	public override void OnGUI()
	{
		float scrollAreaWidth = Mathf.Max(TotalWidth, windowRect.width);
		float scrollAreaHeight = Mathf.Max(Height, windowRect.y);
		Rect contentSize = new(new Vector2(0, 0), new Vector2(scrollAreaWidth, scrollAreaHeight));
		Rect viewportSize = new(new Vector2(0, 0), Size - new Vector2(10, 0));
		position = GUI.BeginScrollView(viewportSize, position, contentSize);
		
		DrawToggleButtons();
		if (currentState > ToggleStates.Off)
		{
			base.OnGUI();
			if (currentState == ToggleStates.All || currentAct == null)
			{
				allActs.OnGUI();
				if (currentAct != null)
				{
					Padding();
					if (currentAct.Window.Button("Reload Act"))
                        currentAct.Reload();

                    if (currentAct.Window.Button("Restart Act"))
							currentAct.Restart();
                }
				StartNewColumn();
			}

			if (currentAct != null)
			{
				if (currentState == ToggleStates.Minimal)
					currentAct.OnGUIMinimal();
				else
					currentAct.OnGUI();
			}
		}

		GUI.EndScrollView();
	}

	private void DrawToggleButtons()
	{
		if (GUI.Button(new Rect(5f, 0f, 20f, 20f), "-"))
		{
			currentState = ToggleStates.Off;
			position = Vector2.zero;
		}

		if (GUI.Button(new Rect(25f, 0f, 20f, 20f), "+"))
		{
			currentState = ToggleStates.Minimal;
		}

		if (GUI.Button(new Rect(45F, 0f, 25f, 20f), "X"))
		{
			currentState = ToggleStates.All;
		}
	}

	protected override bool OnToggleWindowDraw()
	{
		switch (currentState)
		{
			case ToggleStates.Off:
				windowRect.Set(windowRect.x, windowRect.y, 100, 60);
				break;
			case ToggleStates.Minimal:
				windowRect.Set(windowRect.x, windowRect.y, ColumnWidth+40, Size.y);
				break;
			case ToggleStates.All:
				windowRect.Set(windowRect.x, windowRect.y, Size.x, Size.y);
				break;
		}
		BeginDrawingGUI();
		return false;
	}
}