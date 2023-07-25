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
		public Vector2 CurrentSize => currentSize;

		private readonly float originalX;
		private readonly Vector2 currentSize;
		private readonly int totalElements;
		private readonly bool horizontal;
		private readonly DrawableGUI scope;

		public LayoutScope(int totalElements, bool horizontal, DrawableGUI scope)
		{
			this.originalX = scope.X;
			this.totalElements = totalElements;
			this.horizontal = horizontal;
			this.scope = scope;
			this.currentSize = new Vector2(0, 0);
			scope.m_layoutScopes.Add(this);
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

	private readonly Dictionary<string, string> m_buttonGroups = new();
	private readonly List<LayoutScope> m_layoutScopes = new();

	// these can only be set to the correct values from within OnGUI
	// since they reference GUI for their style
	public GUIStyle LabelHeaderStyle = GUIStyle.none;
    public GUIStyle LabelBoldStyle = GUIStyle.none;
    public GUIStyle ButtonStyle = GUIStyle.none;
    public GUIStyle ButtonDisabledStyle = GUIStyle.none;

	public virtual void OnGUI()
	{
        LabelHeaderStyle = new(GUI.skin.label)
        {
            fontSize = 17,
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold
        };
        LabelBoldStyle = new(GUI.skin.label)
        {
            //fontSize = 14,
            fontStyle = FontStyle.Bold
        };
        ButtonStyle = new(GUI.skin.button)
        {
            wordWrap = true
        };
        ButtonDisabledStyle = new GUIStyle(ButtonStyle)
		{
			fontStyle = FontStyle.Bold
		};
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
    public virtual bool Button(string text, Vector2? size = null, string buttonGroup = null, Func<ButtonDisabledData> disabled = null)
	{
		(float x, float y, float w, float h) = GetPosition(size);

		GUIStyle style = ButtonStyle;
		bool wasPressed = false;
		
		ButtonDisabledData disabledData = disabled?.Invoke() ?? new ButtonDisabledData();
		bool isDisabled = disabledData.Disabled;
		if (isDisabled)
		{
			if (!string.IsNullOrEmpty(disabledData.Reason))
				GUI.Label(new Rect(x,y,w,h), text + "\n(" + disabledData.Reason + ")", ButtonDisabledStyle);
			else
                GUI.Label(new Rect(x, y, w, h), text, ButtonDisabledStyle);
        }
		else if (buttonGroup == null)
		{
			wasPressed = GUI.Button(new Rect(x, y, w, h), text, ButtonStyle);
		}
		else
		{
			// create the button group if it doesn't exist
			if (!m_buttonGroups.TryGetValue(buttonGroup, out string selectedButton))
			{
				m_buttonGroups[buttonGroup] = text;
			}

			// grey-out the text if the current button has been selected
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
	public virtual bool Toggle(string text, ref bool value, Vector2? size = null)
	{
		(float x, float y, float w, float h) = GetPosition(size);
		bool toggle = GUI.Toggle(new Rect(x,y,w,h), value, text);
		if (toggle != value)
		{
			value = toggle;
			return true;
		}
		return false;
	}
	
	public virtual bool Toggle(string text, ref ConfigEntry<bool> value, Vector2? size = null)
	{
		(float x, float y, float w, float h) = GetPosition(size);
		bool b = value.Value;
		bool toggle = GUI.Toggle(new Rect(x,y,w,h), b, text);
		if (toggle != b)
		{
			value.Value = toggle;
			return true;
		}
		return false;
	}

	public virtual void Label(string text, Vector2? size = null)
	{
		(float x, float y, float w, float h) = GetPosition(size);
		GUI.Label(new Rect(x, y,w,h), text);
	}

	public virtual void LabelHeader(string text, Vector2? size = null)
	{
		(float x, float y, float w, float h) = GetPosition(size);
		GUI.Label(new Rect(x,y,w,h), text, LabelHeaderStyle);
	}
    public virtual void LabelBold(string text, Vector2? size = null)
    {
        (float x, float y, float w, float h) = GetPosition(size);
        GUI.Label(new Rect(x, y, w, h), text, LabelBoldStyle);
    }

    public virtual object InputField(object value, Type type, Vector2? size = null)
	{
		if (type == typeof(int))
		{
			return IntField((int)value, size);
		}
		else if (type == typeof(float))
		{
			return FloatField((float)value, size);
		}
		else if (type == typeof(string))
		{
			return TextField((string)value, size);
		}
		else if (type == typeof(string))
		{
			bool t = (bool)value;
			Toggle("", ref t, size);
			return t;
		}
		else
		{
			Label("Unsupported type: " + type);
			return value;
		}
	}

	public virtual string TextField(string text, Vector2? size = null)
	{
		(float x, float y, float w, float h) = GetPosition(size);
		return GUI.TextField(new Rect(x, y, w, h), text);
	}

	public virtual int IntField(int text, Vector2? size = null)
	{
		(float x, float y, float w, float h) = GetPosition(size);

		string textField = GUI.TextField(new Rect(x, y, w, h), text.ToString());
		if (!int.TryParse(textField, out int result)) 
			return text;
		
		return result;
	}

	public virtual float FloatField(float text, Vector2? size = null)
	{
		(float x, float y, float w, float h) = GetPosition(size);

		string textField = GUI.TextField(new Rect(x, y, w, h), text.ToString());
		if (!float.TryParse(textField, out float result)) 
			return text;
		
		return result;
	}

	public virtual void Padding(Vector2? size = null)
	{
		float w = size.HasValue && size.Value.x != 0 ? size.Value.x : ColumnWidth;
		float h = size.HasValue && size.Value.y != 0 ? size.Value.y : RowHeight;
		float y = Y;
		Y += h;
		MaxHeight = Mathf.Max(MaxHeight, Y);
		GUI.Label(new Rect(X, y, w, h), "");
	}

	public (float X, float y, float w, float h) GetPosition(Vector2? size = null)
	{
		float x = X;
		float y = Y;
		float h = size.HasValue && size.Value.y != 0 ? size.Value.y : RowHeight;
		float w = size.HasValue && size.Value.x != 0 ? size.Value.x : ColumnWidth;
		
		bool verticallyAligned = m_layoutScopes.Count == 0 || !m_layoutScopes[m_layoutScopes.Count - 1].Horizontal;
		if (verticallyAligned)
		{
			Y += h;
		}
		else
		{
			if (!size.HasValue)
			{
				w = ColumnWidth / m_layoutScopes[m_layoutScopes.Count - 1].TotalElements;
			}

			X += w;
		}
		MaxHeight = Mathf.Max(MaxHeight, Y);
		
		return (x, y, w, h);
	}

	public IDisposable HorizontalScope(int elementCount)
	{
		return new LayoutScope(elementCount, true, this);
	}
    public IDisposable VerticalScope(int elementCount)
    {
        return new LayoutScope(elementCount, false, this);
    }
}