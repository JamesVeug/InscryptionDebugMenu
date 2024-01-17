using DebugMenu.Scripts.Utils;
using UnityEngine;

namespace DebugMenu.Scripts.Popups;

public abstract class BaseWindow : DrawableGUI
{
	public abstract string PopupName { get; } 
	public abstract Vector2 Size { get; }

	public virtual bool ClosableWindow => true;

	public bool IsActive
	{
		get
		{
			return isActive;
		}
		set
		{
			isActive = value;
			windowBlocker.gameObject.SetActive(value);
		}
	}

	protected bool isActive = false;
	protected Rect windowRect = new(20f, 20f, 512f, 512f);
	protected bool isOpen = true;
	protected WindowBlocker windowBlocker;

	protected BaseWindow()
	{
		windowBlocker = Plugin.Instance.CreateWindowBlocker();
		windowBlocker.gameObject.name = $"{PopupName} Window Blocker";
		IsActive = false;
	}

	~BaseWindow()
	{
		Plugin.AllWindows.Remove(this);
	}

	public virtual void Update()
	{
		
	}

	private void SetMatrixGUI(float scalar)
	{
        Vector3 scale = new (scalar, scalar, 1f);
        GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, scale);
    }

	public void OnWindowGUI()
	{
		float scalar = GetDisplayScalar();
        SetMatrixGUI(scalar);

        int id = this.GetType().GetHashCode() + 100;
        windowRect = GUI.Window(id, windowRect, OnWindowDraw, PopupName);
		
		RectTransform blocker = windowBlocker.RectTransform;
		blocker.gameObject.name = PopupName + " Blocker";
		blocker.anchoredPosition = new(windowRect.position.x * scalar, Screen.height - (windowRect.position.y * scalar));
		blocker.sizeDelta = windowRect.size * scalar;
	}

	private void OnWindowDraw(int windowID)
	{
		GUI.DragWindow(new Rect(25f, 0f, Size.x, 20f));
		if (ClosableWindow)
		{
			if (!OnClosableWindowDraw())
                return;
        }
		else if (!OnToggleWindowDraw())
			return;

		windowRect.Set(windowRect.x, windowRect.y, Size.x, Size.y);
		BeginDrawingGUI();
	}

	protected virtual void BeginDrawingGUI()
	{
		GUILayout.BeginArea(new Rect(5f, 25f, windowRect.width, windowRect.height));
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
			IsActive = false;
			return false;
		}

		return true;
	}
}