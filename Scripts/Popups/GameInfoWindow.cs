using DiskCardGame;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DebugMenu.Scripts.Popups;

public class GameInfoPopup : BaseWindow
{
	public override string PopupName => "Game Info";
	public override Vector2 Size => new Vector2(200, 500);

	public float updateInterval = 0.5F;
 
	private float lastInterval;
	private int frames = 0;
	private int fps;

	public override void OnGUI()
	{
		base.OnGUI();

		Label("FPS: " + fps);
		Label("Seed: " + SaveManager.SaveFile.randomSeed);

		if (Button("Show Resource Bank"))
		{
			Plugin.Instance.ToggleWindow<ResourceBankPopup>();
		}
		
		int sceneCount = SceneManager.sceneCount;		
		LabelHeader($"Scenes {sceneCount}");
		Scene activeScene = SceneManager.GetActiveScene();

		for (int i = 0; i < sceneCount; i++)
		{
			Scene scene = SceneManager.GetSceneAt(i);
			if (scene == activeScene)
			{
				Label($"{i} {scene.name} Active");
			}
			else
			{
				Label($"{i} {scene.name}");
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
}