using UnityEngine;
using UnityEngine.SceneManagement;

namespace DebugMenu.Scripts.Popups;

public class GameInfoPopup : BaseWindow
{
	public override string PopupName => "Game Info";
	public override Vector2 Size => new Vector2(200, 500);

	public override void OnGUI()
	{
		base.OnGUI();
		Plugin.Log.LogInfo("GameInfoPopup");

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
}