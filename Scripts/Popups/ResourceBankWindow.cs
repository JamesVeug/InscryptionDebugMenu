using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace DebugMenu.Scripts.Popups;

public class ResourceBankPopup : BaseWindow
{
	public override string PopupName => "Resource Bank";
	public override Vector2 Size => new Vector2(1000, 1000);

	private Vector2 scrollPosition;
	private string resourceBankInfo;

	public ResourceBankPopup()
	{
		ColumnWidth = 200;
	}
	
	public override void OnGUI()
	{
		base.OnGUI();

		ResourceBank resourceBank = ResourceBank.instance;
		if (resourceBank == null)
		{
			Label($"null");
			return;
		}

		if (Button("Copy To Clipboard"))
		{
			GUIUtility.systemCopyBuffer = resourceBankInfo;
		}

		resourceBankInfo = "";
		int resourcesCount = resourceBank.resources.Count;
		int row = 0;
		for (int i = 0; i < resourcesCount; i++)
		{
			ResourceBank.Resource resource = resourceBank.resources[i];
			string resourcePath = $"{i} " + resource.path;
			Label(resourcePath);
			resourceBankInfo += resourcePath + "\n";

			string assetString = null;
			Object resourceAsset = resource.asset;
			if(resourceAsset == null)
			{
				assetString = "null";
			}
			else if (resourceAsset is GameObject resourceAssetGO)
			{
				assetString = $"GO({resourceAssetGO.name})";
			}
			else
			{
				assetString = $"{resourceAsset.GetType()}";
			}

			string text = $"{i} " + assetString;
			Label(text);
			resourceBankInfo += text + "\n";

			if (row * 40 > Size.y)
			{
				StartNewColumn();
			}

			row++;
		}
		
	}
}