using UnityEngine;

namespace DebugMenu.Scripts;

public abstract class DrawableGUI
{
	private const float TopOffset = 20;
	
	public float TotalWidth => Columns * ColumnWidth + ((Columns - 1) * ColumnPadding);
	public float Height => MaxHeight + RowHeight;
	
	private float X = 0;
	private float Y = 0;
	protected float ColumnWidth = 200;
	protected float RowHeight = 40;
	protected float ColumnPadding = 5;
	private int Columns = 1;
	private float MaxHeight = 1;
	
	private GUIStyle LabelHeaderStyle = GUIStyle.none;
	private GUIStyle ButtonStyle = GUIStyle.none;

	public virtual void OnGUI()
	{
		LabelHeaderStyle = new GUIStyle(GUI.skin.label);
		LabelHeaderStyle.fontSize = 17;
		LabelHeaderStyle.alignment = TextAnchor.MiddleCenter;
		LabelHeaderStyle.fontStyle = FontStyle.Bold;

		ButtonStyle = new GUIStyle(GUI.skin.button);
		ButtonStyle.wordWrap = true;
		Reset();
	}
	
	public virtual void Reset()
	{
		X = ColumnPadding;
		Y = TopOffset;
		MaxHeight = 0;
		Columns = 0;
	}

	public virtual void StartNewColumn()
	{
		X += ColumnWidth + ColumnPadding;
		Y = TopOffset;
		Columns++;
	}
	
	/// <returns>Returns true if the button was pressed</returns>
	public virtual bool Button(string text, float? height = null)
	{
		float h = height.HasValue ? height.Value : RowHeight;
		float y = Y;
		Y += h;
		MaxHeight = Mathf.Max(MaxHeight, Y);
		return GUI.Button(new Rect(X, y, ColumnWidth, h), text, ButtonStyle);
	}
	
	/// <returns>Returns True if the value changed</returns>
	public virtual bool Toggle(string text, ref bool value)
	{
		float y = Y;
		Y += RowHeight;
		MaxHeight = Mathf.Max(MaxHeight, Y);
		bool toggle = GUI.Toggle(new Rect(X, y, ColumnWidth, RowHeight), value, text);
		if (toggle != value)
		{
			value = toggle;
			return true;
		}
		return false;
	}

	public virtual void Label(string text, float? height = null)
	{
		float h = height.HasValue ? height.Value : RowHeight;
		float y = Y;
		Y += h;
		MaxHeight = Mathf.Max(MaxHeight, Y);
		GUI.Label(new Rect(X, y, ColumnWidth, h), text);
	}

	public virtual void LabelHeader(string text)
	{
		float y = Y;
		Y += RowHeight;
		MaxHeight = Mathf.Max(MaxHeight, Y);
		GUI.Label(new Rect(X, y, ColumnWidth, RowHeight), text, LabelHeaderStyle);
	}

	public virtual string TextField(string text, float? height = null)
	{
		float h = height.HasValue ? height.Value : RowHeight;
		float y = Y;
		Y += h;
		MaxHeight = Mathf.Max(MaxHeight, Y);
		return GUI.TextField(new Rect(X, y, ColumnWidth, h), text);
	}
}