using DebugMenu.Scripts.All;
using DebugMenu.Scripts.Grimora;
using DebugMenu.Scripts.Magnificus;
using DebugMenu.Scripts.Popups;
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
	public override Vector2 Size => new Vector2(700, 400);
	public override bool ClosableWindow => false;

	private BaseAct CurrentAct = null;
	private ToggleStates CurrentState = ToggleStates.Off;

	private AllActs allActs;
	private Act1.Act1 act1;
	private Act2.Act2 act2;
	private Act3.Act3 act3;
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
		isOpen = false;
	}

	public override void Update()
	{
		if (SaveManager.SaveFile.IsPart1 && GameFlowManager.m_Instance)
		{
			// Leshy
			CurrentAct = act1;
		}
		else if (SaveManager.SaveFile.IsPart2)
		{
			// GDC
			CurrentAct = act2;
		}
		else if (SaveManager.SaveFile.IsPart3)
		{
			// PO3
			CurrentAct = act3;
		}
		else if (SaveManager.SaveFile.IsGrimora)
		{
			// Grimora
			CurrentAct = actGrimora;
		}
		else if (SaveManager.SaveFile.IsMagnificus)
		{
			// Magnificus
			CurrentAct = actMagnificus;
		}
		else
		{
			// In main menu maybe???
			CurrentAct = null;
		}

		if (CurrentAct != null)
		{
			CurrentAct.Update();
		}
	}

	public override void OnGUI()
	{
		float scrollAreaWidth = Mathf.Max(TotalWidth, windowRect.width);
		float scrollAreaHeight = Mathf.Max(Height, windowRect.y);
		Rect contentSize = new Rect(new Vector2(0, 0), new Vector2(scrollAreaWidth, scrollAreaHeight));
		Rect viewportSize = new Rect(new Vector2(0, 0), Size - new Vector2(10, 0));
		position = GUI.BeginScrollView(viewportSize, position, contentSize);
		
		DrawToggleButtons();
		
		if (CurrentState > ToggleStates.Off)
		{
			base.OnGUI();

			if (CurrentState == ToggleStates.All || CurrentAct == null)
			{
				allActs.OnGUI();

				if (CurrentAct != null)
				{
					CurrentAct.Window.Padding();
					CurrentAct.OnGUIReload();
					CurrentAct.OnGUIRestart();
				}
				StartNewColumn();
			}

			if (CurrentAct != null)
			{
				
				if (CurrentState == ToggleStates.Minimal)
				{
					CurrentAct.OnGUIMinimal();
				}
				else
				{
					CurrentAct.OnGUI();
				}
			}
		}

		GUI.EndScrollView();
	}

	private void DrawToggleButtons()
	{
		if (GUI.Button(new Rect(5f, 0f, 20f, 20f), "-"))
		{
			CurrentState = ToggleStates.Off;
			position = Vector2.zero;
		}

		if (GUI.Button(new Rect(25f, 0f, 20f, 20f), "+"))
		{
			CurrentState = ToggleStates.Minimal;
		}

		if (GUI.Button(new Rect(45F, 0f, 25f, 20f), "X"))
		{
			CurrentState = ToggleStates.All;
		}
	}

	protected override bool OnToggleWindowDraw()
	{
		switch (CurrentState)
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