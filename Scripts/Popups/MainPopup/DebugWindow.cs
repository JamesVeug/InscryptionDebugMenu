using DebugMenu.Scripts.All;
using DebugMenu.Scripts.Grimora;
using DebugMenu.Scripts.Magnificus;
using DebugMenu.Scripts.Popups;
using DiskCardGame;
using UnityEngine;

namespace DebugMenu.Scripts.Acts;

public class DebugWindow : BaseWindow
{
	public override string PopupName => "Debug Menu";
	public override Vector2 Size => new Vector2(700, 400);

	private BaseAct CurrentAct = null;

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
		Rect scrollableAreaSize = new Rect(new Vector2(0, 0), new Vector2(TotalWidth, Height));
		Rect scrollViewSize = new Rect(new Vector2(0, 0), Size - new Vector2(10, 25));
		position = GUI.BeginScrollView(scrollViewSize, position, scrollableAreaSize);
		
		base.OnGUI();
	        
		allActs.OnGUI();
		if (CurrentAct != null)
		{
			CurrentAct.OnGUIReload();
			CurrentAct.OnGUIRestart();
		}
	        
		StartNewColumn();
		if (CurrentAct != null)
		{
			CurrentAct.OnGUI();
		}


		GUI.EndScrollView();
	}
}