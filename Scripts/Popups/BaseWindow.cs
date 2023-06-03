using UnityEngine;

namespace DebugMenu.Scripts.Popups;

public abstract class BaseWindow : DrawableGUI
{
	public abstract string PopupName { get; } 
	public abstract Vector2 Size { get; }
	public virtual bool ClosableWindow => true; 
	
	protected Rect windowRect = new Rect(20f, 20f, 512f, 512f);
	protected bool isOpen = true;

	~BaseWindow()
	{
		Plugin.AllWindows.Remove(this);
	}

	public virtual void Update()
	{
		
	}

	public void OnWindowGUI()
	{
		int id = this.GetType().GetHashCode() + 100;
		windowRect = GUI.Window(id, windowRect, OnWindowDraw, PopupName);
	}

	private void OnWindowDraw(int windowID)
	{
		GUI.DragWindow(new Rect(25f, 0f, Size.x, 20f));
		if (ClosableWindow)
		{
			if (!OnClosableWindowDraw())
			{
				return;
			}
		}
		else
		{
			if (!OnToggleWindowDraw())
			{
				return;
			}
		}
		windowRect.Set(windowRect.x, windowRect.y, Size.x, Size.y);
		BeginDrawingGUI();
	}

	protected virtual void BeginDrawingGUI()
	{
		GUILayout.BeginArea(new Rect(5f, 25f, windowRect.width - 10f, windowRect.height));
		OnGUI();
		GUILayout.EndArea();
	}

	protected virtual bool OnToggleWindowDraw()
	{
		isOpen = GUI.Toggle(new Rect(5f, 0f, 20f, 20f), isOpen, "");
		if (!isOpen)
		{
			windowRect.Set(windowRect.x, windowRect.y, 120, 60);
			return false;
		}

		return true;
	}

	protected bool OnClosableWindowDraw()
	{
		if (GUI.Button(new Rect(5f, 0f, 20f, 20f), "X"))
		{
			Plugin.AllWindows.Remove(this);
			return false;
		}

		return true;
	}
}