using UnityEngine;

namespace DebugMenu.Scripts;

public abstract class DrawableGUI
{
	public float TotalWidth => Columns * ColumnWidth + ((Columns - 1) * ColumnPadding);
	
	private float X = 0;
	private float Y = 0;
	private float ColumnWidth = 200;
	private float ColumnPadding = 10;
	private int Columns = 1;
	
	private GUIStyle LabelHeaderStyle = GUIStyle.none;

	public virtual void OnGUI()
	{
		LabelHeaderStyle = new GUIStyle(GUI.skin.label);
		LabelHeaderStyle.fontSize = 17;
		LabelHeaderStyle.alignment = TextAnchor.MiddleCenter;
		LabelHeaderStyle.fontStyle = FontStyle.Bold;

		Reset();
	}
	
	public virtual void Reset()
	{
		X = ColumnPadding;
		Y = 10;
	}

	public virtual void StartNewColumn()
	{
		X += ColumnWidth + ColumnPadding;
		Y = 10;
		Columns++;
	}
	
	/// <returns>Returns true if the button was pressed</returns>
	public virtual bool Button(string text)
	{
		float y = Y;
		Y += 40f;
		return GUI.Button(new Rect(X, y, ColumnWidth, 40f), text);
	}
	
	/// <returns>Returns True if the value changed</returns>
	public virtual bool Toggle(string text, ref bool value)
	{
		float y = Y;
		Y += 40f;
		bool toggle = GUI.Toggle(new Rect(X, y, ColumnWidth, 40f), value, text);
		if (toggle != value)
		{
			value = toggle;
			return true;
		}
		return false;
	}

	public virtual void Label(string text, float? height = null)
	{
		float h = height.HasValue ? height.Value : 40;
		float y = Y;
		Y += h;
		GUI.Label(new Rect(X, y, ColumnWidth, h), text);
	}

	public virtual void LabelHeader(string text)
	{
		float y = Y;
		Y += 40f;
		GUI.Label(new Rect(X, y, ColumnWidth, 40f), text, LabelHeaderStyle);
	}
}