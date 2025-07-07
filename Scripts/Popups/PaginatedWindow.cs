using UnityEngine;

namespace DebugMenu.Scripts.Popups;

public abstract class PaginatedWindow : BaseWindow
{
    public bool FilterByGuid = false;

    public bool ShowAll = true;
    public bool ShowVanilla = true;
    public bool ShowModded = true;

    public bool FilterByAct1 = false;
    public bool FilterByAct2 = false;
    public bool FilterByAct3 = false;
    public bool FilterByAscension = false;
    public bool FilterByGrimora = false;
    public bool FilterByMagnificus = false;

    public virtual int NumElements { get; set; } = 60;

    public int pageNum = 0;

    public int startIndex = 0;

    public int endIndex = 0;

    public string filterText;

    public string currentGuid;

    public virtual void HandleGuid()
    {
        Label("Selected GUID: " + currentGuid);
    }
    public virtual void HandleFilterText()
    {
        Label("Search Filter", (Vector2?)new Vector2(0f, base.RowHeight / 2f));
        filterText = TextField(filterText, (Vector2?)new Vector2(0f, base.RowHeight / 2f));
        Toggle("Filter by GUID", ref FilterByGuid);
    }
    public virtual bool HandleToggles()
    {
        if (!ShowVanilla || !ShowModded)
            ShowAll = false;

        bool toggleChanged = Toggle("Show All Abilities", ref ShowAll);
        if (toggleChanged)
        {
            ShowModded = ShowVanilla = ShowAll;
            ResetFilters();
        }

        toggleChanged = Toggle("Show Modded Abilities", ref ShowModded) || toggleChanged;
        if (toggleChanged && ShowModded)
        {
            ShowAll = ShowVanilla;
            ResetFilters();
        }

        toggleChanged = Toggle("Show Vanilla Abilities", ref ShowVanilla) || toggleChanged;
        if (toggleChanged && ShowVanilla)
        {
            ShowAll = ShowModded;
            ResetFilters();
        }

        toggleChanged = Toggle("Filter by Act 1", ref FilterByAct1) || toggleChanged;
        toggleChanged = Toggle("Filter by Act 2", ref FilterByAct2) || toggleChanged;
        toggleChanged = Toggle("Filter by Act 3", ref FilterByAct3) || toggleChanged;
        toggleChanged = Toggle("Filter by Ascension", ref FilterByAscension) || toggleChanged;
        return toggleChanged;
    }
    public void HandlePages(int infoCount)
    {
        if (Button("Next Page"))
        {
            IncreasePageIndeces(infoCount);
        }
        if (Button("Previous Page"))
        {
            DecreasePageIndeces(infoCount);
        }
    }

    public void HandleOnGUI<T>(string key, List<T> allInfo, List<T> vanillaInfo, List<T> modInfo)
    {
        base.OnGUI();
        if (allInfo == null || allInfo.Count == 0)
        {
            Label("No " + key);
            return;
        }

        if (Button("Print All " + key))
        {
            PrintAllInfoToLog();
        }
        if (Button("Print Vanilla " + key, disabled: () => new("Failed to load vanilla data!") { Disabled = vanillaInfo == null || vanillaInfo.Count == 0 }))
        {
            PrintVanillaInfoToLog();
        }
        if (Button("Print Modded " + key, disabled: () => new("No modded data") { Disabled = modInfo == null || modInfo.Count == 0 }))
        {
            PrintModdedInfoToLog();
        }
        if (Button("Print " + key + " by selected GUID"))
        {
            PrintSelectedInfoToLog();
        }

        HandleGuid();
        HandleFilterText();
        if (HandleToggles())
        {
            pageNum = startIndex = endIndex = 0;
        }
    }

    public abstract void PrintAllInfoToLog();
    public abstract void PrintVanillaInfoToLog();
    public abstract void PrintModdedInfoToLog();
    public abstract void PrintSelectedInfoToLog();
    public void ResetFilters() => FilterByAct1 = FilterByAct2 = FilterByAct3 = FilterByAct3 = FilterByAscension = FilterByGrimora = FilterByMagnificus = false;
    private void IncreasePageIndeces(int maxCount)
    {
        pageNum++;
        if (endIndex == maxCount)
        {
            startIndex = pageNum = 0;
            endIndex = Mathf.Min(NumElements, maxCount);
            return;
        }
        startIndex = pageNum * NumElements;
        endIndex = pageNum * NumElements + NumElements;
        if (endIndex > maxCount)
        {
            endIndex = maxCount;
        }
    }
    private void DecreasePageIndeces(int maxCount)
    {
        pageNum--;
        if (startIndex == 0)
        {
            pageNum = maxCount / NumElements;
            startIndex = pageNum * NumElements;
            endIndex = maxCount;
            return;
        }
        startIndex = pageNum * NumElements;
        endIndex = pageNum * NumElements + NumElements;
        if (startIndex < 0)
        {
            startIndex = 0;
        }
    }
}