using UnityEngine;

namespace DebugMenu.Scripts;

public static class GUIHelper
{
	private static float X = 0;
	private static float Y = 0;
	private static float Width = 200;
	
	private static GUIStyle LabelHeaderStyle = GUIStyle.none;

	static GUIHelper()
	{
		LabelHeaderStyle = new GUIStyle(GUI.skin.label);
		LabelHeaderStyle.fontSize = 17;
		LabelHeaderStyle.alignment = TextAnchor.MiddleCenter;
		LabelHeaderStyle.fontStyle = FontStyle.Bold;
	}
	
	public static void Reset()
	{
		X = 10;
		Y = 10;
	}

	public static void StartNewColumn()
	{
		X += Width + 10;
		Y = 10;
	}
	
	/// <returns>Returns true if the button was pressed</returns>
	public static bool Button(string text)
	{
		float y = Y;
		Y += 40f;
		return GUI.Button(new Rect(X, y, Width, 40f), text);
	}
	
	/// <returns>Returns True if the value changed</returns>
	public static bool Toggle(string text, ref bool value)
	{
		float y = Y;
		Y += 40f;
		bool toggle = GUI.Toggle(new Rect(X, y, Width, 40f), value, text);
		if (toggle != value)
		{
			value = toggle;
			return true;
		}
		return false;
	}

	public static void Label(string text, float? height = null)
	{
		float h = height.HasValue ? height.Value : 40;
		float y = Y;
		Y += h;
		GUI.Label(new Rect(X, y, Width, h), text);
	}

	public static void LabelHeader(string text)
	{
		float y = Y;
		Y += 40f;
		GUI.Label(new Rect(X, y, Width, 40f), text, LabelHeaderStyle);
	}
}