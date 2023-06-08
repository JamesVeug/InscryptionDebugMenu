using System.Reflection;
using DebugMenu.Scripts.Acts;
using DebugMenu.Scripts.Popups;
using DebugMenu.Scripts.Utils;
using UnityEngine;

namespace DebugMenu.Scripts.Hotkeys;

public class HotkeyController
{
	public class FunctionData
	{
		public string ID;
		public Type[] Arguments;
		public string[] ArgumentNames;
		public Action<object[]> Callback;

		public void Invoke(object[] arguments)
		{
			Callback.Invoke(arguments);
		}
	}
	
	public class Hotkey
	{
		public KeyCode[] KeyCodes;
		public object[] Arguments;
		public string FunctionID;
	}

	public static Action<List<KeyCode>, KeyCode> OnHotkeyPressed = delegate { };
	public static KeyCode[] AllCodes = Enum.GetValues(typeof(KeyCode)).Cast<KeyCode>().ToArray();
	public static int MaxArgumentsInFunctions = 4;

	public List<FunctionData> AllFunctionData => m_allFunctionData;
	
	public List<Hotkey> Hotkeys = new List<Hotkey>();
	
	private Dictionary<string, FunctionData> m_functionIDToData = new Dictionary<string, FunctionData>();
	private List<FunctionData> m_allFunctionData = new List<FunctionData>();
	private List<KeyCode> m_pressedKeys = new List<KeyCode>();
	private bool m_hotkeyActivated = false;
	
	public HotkeyController()
	{
		InitializeFunctions();

		string hotkeys = Configs.Hotkeys.Trim();
		if (string.IsNullOrEmpty(hotkeys))
		{
			return;
		}
		
		string[] hotkeyStrings = hotkeys.Split(',');
		foreach (string hotkey in hotkeyStrings)
		{
			string[] split = hotkey.Trim().Split(':');
			if (split.Length == 0)
			{
				Plugin.Log.LogError($"Bad Hotkey format: '{hotkey}'");
				continue;
			}

			KeyCode[] keyCodes = DeserializeKeyCodes(split[0]);
			string functionID = DeserializeFunction(split, keyCodes, out FunctionData data);
			string[] argumentStrings = split.Length > 2 ? split.Skip(2).ToArray() : null;
			object[] arguments = ConvertArguments(argumentStrings, data);
			Hotkeys.Add(new Hotkey()
			{
				Arguments = arguments,
				KeyCodes = keyCodes,
				FunctionID = functionID
			});
		}
	}

	private string DeserializeFunction(string[] split, KeyCode[] keyCodes, out FunctionData data)
	{
		string functionID = split.Length > 1 ? split[1].Trim() : null;
		if (functionID == null)
		{
			functionID = m_allFunctionData[0].ID;
			data = m_allFunctionData[0];
			Plugin.Log.LogError($"No function specified for hotkey: '{keyCodes.Serialize()}'. Using default: '{functionID}'");
		}
		else if (!m_functionIDToData.TryGetValue(functionID, out data))
		{
			functionID = m_allFunctionData[0].ID;
			data = m_allFunctionData[0];
			Plugin.Log.LogError($"Bad function id: '{functionID}' for hotkey '{keyCodes.Serialize()}'. Using default: '{functionID}'");
		}

		return functionID;
	}

	private static KeyCode[] DeserializeKeyCodes(string split)
	{
		KeyCode[] keyCodes = split.Split('+').Select((a) =>
		{
			if (!Enum.TryParse(a, out KeyCode b))
			{
				Plugin.Log.LogError($"Unknown hotkey: '{a}'");
			}

			return b;
		}).ToArray();
		return keyCodes;
	}

	private static object[] ConvertArguments(string[] argumentStrings, FunctionData data)
	{
		object[] arguments = new object[data.Arguments.Length];
		if (data != null)
		{
			int totalArguments = argumentStrings == null ? 0 : argumentStrings.Length;
			for (int i = 0; i < Mathf.Min(totalArguments, data.Arguments.Length); i++)
			{
				// Convert from string to the type expected by the callback
				string argumentString = argumentStrings[i] == null ? null : argumentStrings[i].Trim();
				try
				{
					if (!string.IsNullOrEmpty(argumentString))
					{
						arguments[i] = Convert.ChangeType(argumentString, data.Arguments[i]);
					}
					else
					{
						Plugin.Log.LogError($"Given empty string for function '{data.ID} and argument {data.Arguments[i].Name}");
						arguments[i] = Helpers.GetDefaultValue(data.Arguments[i]);
					}
				}
				catch (Exception e)
				{
					Plugin.Log.LogError($"Failed to parse argument '{Helpers.ToLiteral(argumentString)}' from string to {data.Arguments[i].Name}");
					Plugin.Log.LogError(e);
					arguments[i] = Helpers.GetDefaultValue(data.Arguments[i]);
				}
			}
		}

		return arguments;
	}

	public void SaveHotKeys()
	{
		string format = "";
		foreach (Hotkey hotkey in Hotkeys)
		{
			string hotkeyString = hotkey.KeyCodes.Serialize("+");
			if (!string.IsNullOrEmpty(hotkey.FunctionID))
			{
				hotkeyString += ":" + hotkey.FunctionID;
			}

			if (hotkey.Arguments != null && hotkey.Arguments.Length > 0)
			{
				hotkeyString += ":" + string.Join(":", hotkey.Arguments);
			}

			if (format.Length > 0)
			{
				format += ",";
			}
			
			format += hotkeyString;
		}

		Plugin.Log.LogInfo($"Saved hotkeys");
		Configs.Hotkeys = format;
	}
	
	public void Update()
	{
		foreach (KeyCode code in AllCodes)
		{
			if (Input.GetKeyDown(code) && !m_pressedKeys.Contains(code))
			{
				m_pressedKeys.Add(code);
				HotkeysChanged(code, true);
			}
		}

		for (int i = 0; i < m_pressedKeys.Count; i++)
		{
			KeyCode pressedKey = m_pressedKeys[i];
			if (!Input.GetKey(pressedKey))
			{
				m_pressedKeys.Remove(pressedKey);
				HotkeysChanged(KeyCode.None, false);

				m_hotkeyActivated = false;
			}
		}
	}

	private void HotkeysChanged(KeyCode pressedButton, bool triggerHotkey)
	{
		if (triggerHotkey)
		{
			Hotkey activatedHotkey = null;
			foreach (Hotkey hotkey in Hotkeys)
			{
				if (m_hotkeyActivated)
				{
					continue;
				}

				if (hotkey.KeyCodes.Length == 0)
				{
					continue;
				}

				// all buttons from hotkey are pressed
				bool allHotkeysPressed = m_pressedKeys.Intersect(hotkey.KeyCodes).Count() == hotkey.KeyCodes.Length;
				if (allHotkeysPressed)
				{
					if (activatedHotkey == null || activatedHotkey.KeyCodes.Length < hotkey.KeyCodes.Length)
					{
						activatedHotkey = hotkey;
					}
				}
			}

			if (activatedHotkey != null)
			{
				if (m_functionIDToData.TryGetValue(activatedHotkey.FunctionID, out FunctionData data))
				{
					data.Invoke(activatedHotkey.Arguments);
					m_hotkeyActivated = true;
				}
				else
				{
					Plugin.Log.LogError("Hotkey callback not found: " + activatedHotkey.FunctionID);
				}
			}
		}

		if (pressedButton != KeyCode.None)
		{
			OnHotkeyPressed?.Invoke(m_pressedKeys, pressedButton);
		}
	}

	public void InitializeFunctions()
	{
		m_functionIDToData = new Dictionary<string, FunctionData>();
		
		// Show/hide debug menu
		Add("Debug Menu Show/Hide", (_) => Configs.ShowDebugMenu = !Configs.ShowDebugMenu);
		
		// Turn windows on/off
		GetToggleWindowData();
		
		// Map
		AddMethods("Map", static () => Plugin.Instance.GetWindow<DebugWindow>()?.CurrentAct?.MapSequence,
			(info) => info.IsAbstract);
		
		// Card Battles
		AddMethods("Battle", static () => Plugin.Instance.GetWindow<DebugWindow>()?.CurrentAct?.BattleSequence,
			(info) => info.IsAbstract);
		
		// Other
		Add("AllAct Reload", (args) => Plugin.Instance.GetWindow<DebugWindow>()?.CurrentAct?.Reload());
		Add("AllAct Restart", (args) => Plugin.Instance.GetWindow<DebugWindow>()?.CurrentAct?.Restart());
		AddMethods("AllAct", static () => Plugin.Instance.GetWindow<DebugWindow>()?.AllActs,
			(info) => info.GetBaseDefinition().DeclaringType == info.DeclaringType);


		m_allFunctionData = m_functionIDToData.Values.ToList();
		m_allFunctionData.Sort(SortFunctions);
		MaxArgumentsInFunctions = m_allFunctionData.Max(static (a) => a.Arguments.Length);
	}

	private static int SortFunctions(FunctionData a, FunctionData b)
	{
		return String.Compare(a.ID, b.ID, StringComparison.Ordinal);
	}

	public void GetToggleWindowData()
	{
		// get all sub-types of BaseWindow
		Type[] types = Assembly.GetAssembly(typeof(BaseWindow)).GetTypes();
		foreach (Type type in types)
		{
			if (type.IsAbstract || !type.IsSubclassOf(typeof(BaseWindow)))
			{
				continue;
			}

			FunctionData functionData = new FunctionData()
			{
				ID = type.Name + " ToggleWindow",
				Arguments = Array.Empty<Type>(),
				ArgumentNames = Array.Empty<string>(),
				Callback = (_) =>
				{
					Plugin.Instance.ToggleWindow(type);
				}
			};
			m_functionIDToData[functionData.ID] = functionData;
		}
	}

	public void Add(string id, Action<object[]> callback)
	{
		FunctionData functionData = new FunctionData()
		{
			ID = id,
			Arguments = Array.Empty<Type>(),
			ArgumentNames = Array.Empty<string>(),
			Callback = callback
		};
		m_functionIDToData[functionData.ID] = functionData;
	}
	
	public void AddMethods<T>(string idPrefix, Func<T> getter, Func<MethodInfo, bool> condition = null, BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
	{
		MethodInfo[] methodInfos = typeof(T).GetMethods(flags);
		foreach (MethodInfo info in methodInfos)
		{
			if (condition != null && !condition(info))
				continue;
		
			// ignore properties
			if (info.Name.StartsWith("get_") || info.Name.StartsWith("set_"))
				continue;
			
			string id = idPrefix + " " + info.Name;
			if (m_functionIDToData.ContainsKey(id))
				continue;
				
			FunctionData functionData = new FunctionData()
			{
				ID = id,
				Arguments = info.GetParameters().Select((a)=>a.ParameterType).ToArray(),
				ArgumentNames = info.GetParameters().Select((a)=>a.Name).ToArray(),
				Callback = (args) =>
				{
					T obj = getter();
					if (obj != null)
					{
						MethodInfo methodInfo = obj.GetType().GetMethod(info.Name);
						if (methodInfo != null)
						{
							methodInfo.Invoke(obj, args);
						}
					}
				}
			};
			m_functionIDToData[functionData.ID] = functionData;
		}
	}

	public Hotkey CreateNewHotkey()
	{
		return new Hotkey()
		{
			KeyCodes = new KeyCode[1] { KeyCode.End },
			FunctionID = m_allFunctionData[0].ID,
			Arguments = m_allFunctionData[0].Arguments.Select(Activator.CreateInstance).ToArray(),
		};
	}

	public FunctionData GetFunctionData(string id)
	{
		m_functionIDToData.TryGetValue(id, out FunctionData data);
		return data;
	}
};