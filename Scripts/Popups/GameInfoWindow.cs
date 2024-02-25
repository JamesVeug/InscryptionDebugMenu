using BepInEx;
using BepInEx.Bootstrap;
using DebugMenu.Scripts.Utils;
using DiskCardGame;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DebugMenu.Scripts.Popups;

public class GameInfoPopup : BaseWindow
{
	public override string PopupName => "Game Info";
	public override Vector2 Size => new(220, 500);

	public float updateInterval = 0.5F;
 
	private float lastInterval;
	private int frames = 0;
	private int fps;

	public override void OnGUI()
	{
		base.OnGUI();
        string currentSeed;
        try { currentSeed = "Current Seed: " + SaveManager.SaveFile.GetCurrentRandomSeed(); }
        catch { currentSeed = "Current Seed: N/A"; }

        Label("FPS: " + fps);
        Label("Random Seed: " + SaveManager.SaveFile.randomSeed + "\n" + currentSeed);

        if (Button("Debug Tools"))
        {
            Plugin.Instance.ToggleWindow<ScriptDebugPopup>();
        }
        if (Button("Show AbilityInfos"))
        {
            Plugin.Instance.ToggleWindow<AbilityInfoPopup>();
        }
		if (Button("Show Dialogue Events"))
		{
			Plugin.Instance.ToggleWindow<DialogueEventPopup>();
		}
        if (Button("Show Resource Bank"))
        {
            Plugin.Instance.ToggleWindow<ResourceBankPopup>();
        }
        if (Button("Show Configs"))
        {
	        ShowConfigs();
        }
        int sceneCount = SceneManager.sceneCount;		
		LabelHeader($"Scenes ({sceneCount})");
		Scene activeScene = SceneManager.GetActiveScene();

		for (int i = 0; i < sceneCount; i++)
		{
			Scene scene = SceneManager.GetSceneAt(i);
			if (scene == activeScene)
			{
				Label($"{i}: {scene.name} (Active)");
			}
			else
			{
				Label($"{i}: {scene.name}");
			}
		}
	}

	public override void Update()
	{
		base.Update();
		++frames;
 
		float timeNow = Time.realtimeSinceStartup;
		if (timeNow > lastInterval + updateInterval)
		{
			fps = (int)(frames / (timeNow - lastInterval));
			frames = 0;
			lastInterval = timeNow;
		}
	}

	private void ShowConfigs()
	{
		ButtonListPopup.OnGUI(this, "Show Configs", "All Configs", Helpers.GetAllConfigs,
			(index, value, disableMatch) =>
			{
				PluginConfig config = Helpers.GetConfig(value);
				if (config != null)
				{
					ConfigPopup.Show(config);
				}
			});
	}
}