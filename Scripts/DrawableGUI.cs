using BepInEx.Configuration;
using UnityEngine;

namespace DebugMenu.Scripts;

public abstract class DrawableGUI
{
	private const float TopOffset = 20;

	public struct ButtonDisabledData
	{
		public bool Disabled;
		public string Reason;

		public ButtonDisabledData(string reason)
		{
			Disabled = true;
			Reason = reason;
		}
	}

	public struct LayoutScope : IDisposable
	{
		public bool Horizontal => horizontal;
		public int TotalElements => totalElements;

		private readonly float originalX;
		private readonly int totalElements;
		private readonly bool horizontal;
		private readonly DrawableGUI scope;

		public LayoutScope(int totalElements, bool horizontal, DrawableGUI scope)
		{
			this.originalX = scope.X;
			this.totalElements = totalElements;
			this.horizontal = horizontal;
			this.scope = scope;
			scope.m_layoutScopes.Add(this);
			//scope.Y += scope.RowHeight;
		}
		
		public void Dispose()
		{
			scope.m_layoutScopes.Remove(this);
			if (horizontal)
			{
				scope.X = originalX;
				scope.Y += scope.RowHeight;
			}
		}
	}
	
	public float TotalWidth => Columns * ColumnWidth + ((Columns - 1) * ColumnPadding);
	public float Height => MaxHeight + RowHeight;
	
	private float X = 0;
	private float Y = 0;
	protected float ColumnWidth = 200;
	protected float RowHeight = 40;
	protected float ColumnPadding = 5;
	private int Columns = 1;
	private float MaxHeight = 1;

	private Dictionary<string, string> m_buttonGroups = new Dictionary<string, string>();
	private List<LayoutScope> m_layoutScopes = new List<LayoutScope>();

	private GUIStyle LabelHeaderStyle = GUIStyle.none;
	private GUIStyle ButtonStyle = GUIStyle.none;
	private GUIStyle ButtonDisabledStyle = GUIStyle.none;

	public virtual void OnGUI()
	{
		LabelHeaderStyle = new GUIStyle(GUI.skin.label);
		LabelHeaderStyle.fontSize = 17;
		LabelHeaderStyle.alignment = TextAnchor.MiddleCenter;
		LabelHeaderStyle.fontStyle = FontStyle.Bold;

		ButtonStyle = new GUIStyle(GUI.skin.button);
		ButtonStyle.wordWrap = true;
		
		ButtonDisabledStyle = new GUIStyle(ButtonStyle);
		ButtonDisabledStyle.fontStyle = FontStyle.Bold;
		ButtonDisabledStyle.normal.background = ButtonDisabledStyle.active.background;
		ButtonDisabledStyle.hover.background = ButtonDisabledStyle.active.background;
		ButtonDisabledStyle.onNormal.background = ButtonDisabledStyle.active.background;
		ButtonDisabledStyle.onHover.background = ButtonDisabledStyle.active.background;
		ButtonDisabledStyle.onActive.background = ButtonDisabledStyle.active.background;
		ButtonDisabledStyle.onFocused.background = ButtonDisabledStyle.active.background;
		ButtonDisabledStyle.normal.textColor = Color.black;

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
	public virtual bool Button(string text, float? height = null, string buttonGroup = null, Func<ButtonDisabledData> disabled = null)
	{
		(float x, float y, float w, float h) = GetPosition(height);

		GUIStyle style = ButtonStyle;
		bool wasPressed = false;
		
		ButtonDisabledData disabledData = disabled?.Invoke() ?? new ButtonDisabledData();
		bool isDisabled = disabledData.Disabled;
		if (isDisabled)
		{
			GUI.Label(new Rect(x,y,w,h), text + "\n(" + disabledData.Reason + ")", ButtonDisabledStyle);
		}
		else if (buttonGroup == null)
		{
			wasPressed = GUI.Button(new Rect(x, y, w, h), text, ButtonStyle);
		}
		else
		{
			if (!m_buttonGroups.TryGetValue(buttonGroup, out string selectedButton))
			{
				m_buttonGroups[buttonGroup] = text;
			}

			if (selectedButton == text)
			{
				style = ButtonDisabledStyle;
			}

			wasPressed = GUI.Button(new Rect(x,y,w,h), text, style);
			if (wasPressed)
			{
				m_buttonGroups[buttonGroup] = text;
			}
		}


		return wasPressed;
	}
	
	/// <returns>Returns True if the value changed</returns>
	public virtual bool Toggle(string text, ref bool value, float? height = null)
	{
		(float x, float y, float w, float h) = GetPosition(height);
		bool toggle = GUI.Toggle(new Rect(x,y,w,h), value, text);
		if (toggle != value)
		{
			value = toggle;
			return true;
		}
		return false;
	}
	
	public virtual bool Toggle(string text, ref ConfigEntry<bool> value, float? height = null)
	{
		(float x, float y, float w, float h) = GetPosition(height);
		bool b = value.Value;
		bool toggle = GUI.Toggle(new Rect(x,y,w,h), b, text);
		if (toggle != b)
		{
			value.Value = toggle;
			return true;
		}
		return false;
	}

	public virtual void Label(string text, float? height = null)
	{
		(float x, float y, float w, float h) = GetPosition(height);
		GUI.Label(new Rect(x, y,w,h), text);
	}

	public virtual void LabelHeader(string text, float? height = null)
	{
		(float x, float y, float w, float h) = GetPosition(height);
		GUI.Label(new Rect(x,y,w,h), text, LabelHeaderStyle);
	}

	public virtual string TextField(string text, float? height = null)
	{
		(float x, float y, float w, float h) = GetPosition(height);
		return GUI.TextField(new Rect(x, y, w, h), text);
	}

	public virtual int IntField(int text, float? height = null)
	{
		(float x, float y, float w, float h) = GetPosition(height);

		string textField = GUI.TextField(new Rect(x, y, w, h), text.ToString());
		if (!int.TryParse(textField, out int result)) 
			return text;
		
		return result;
	}

	public virtual void Padding(float? height = null)
	{
		float h = height.HasValue ? height.Value : RowHeight;
		float y = Y;
		Y += h;
		MaxHeight = Mathf.Max(MaxHeight, Y);
		GUI.Label(new Rect(X, y, ColumnWidth, h), "");
	}

	private (float X, float y, float w, float h) GetPosition(float? height = null)
	{
		float x = X;
		float y = Y;
		float h = height.HasValue ? height.Value : RowHeight;
		float w = ColumnWidth;
		
		bool verticallyAligned = m_layoutScopes.Count == 0 || !m_layoutScopes[m_layoutScopes.Count - 1].Horizontal;
		if (verticallyAligned)
		{
			Y += h;
		}
		else
		{
			w = ColumnWidth / m_layoutScopes[m_layoutScopes.Count - 1].TotalElements;
			X += w;
		}
		MaxHeight = Mathf.Max(MaxHeight, Y);
		
		return (x, y, w, h);
	}

	public IDisposable HorizontalScope(int elementCount)
	{
		return new LayoutScope(elementCount, true, this);
	}
}