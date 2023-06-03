using DebugMenu.Scripts.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace DebugMenu.Scripts.Popups;

public class ResourceBankPopup : BaseWindow
{
	public override string PopupName => "Resource Bank";
	public override Vector2 Size => new Vector2(1000, 1000);

	private string resourceBankInfo;
	private string filterText;

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
			Label($"null resourceBank");
			return;
		}

		if (Button("Copy All To Clipboard"))
		{
			GUIUtility.systemCopyBuffer = resourceBankInfo;
		}
		
		Label("Filter", new(0, RowHeight / 2));
		filterText = TextField(filterText, new(0, RowHeight / 2));

		Label(""); // padding
		
		resourceBankInfo = "";
		int resourcesCount = resourceBank.resources.Count;
		int row = 0;
		for (int i = 0; i < resourcesCount; i++)
		{
			ResourceBank.Resource resource = resourceBank.resources[i];
			string path = resource.path;
			if (!string.IsNullOrEmpty(filterText) && !path.ContainsText(filterText, false))
			{
				continue;
			}

			string resourcePath = path + "\n";

			string assetString = null;
			Object resourceAsset = resource.asset;
			if(resourceAsset == null)
			{
				assetString = "null";
			}
			else if (resourceAsset is GameObject)
			{
				assetString = $"GameObject";
			}
			else
			{
				assetString = $"{resourceAsset.GetType()}";
			}

			resourcePath += assetString;
			if (Button(resourcePath, new(0, 60)))
			{
				GUIUtility.systemCopyBuffer = resourcePath;
			}
			resourceBankInfo += "\n" + resourcePath + "\n";

			if (row > 10)
			{
				StartNewColumn();
				row = 0;
			}

			row++;
		}
		
	}
}