using DebugMenu.Scripts.Acts;
using DebugMenu.Scripts.Popups;
using DiskCardGame;
using InscryptionAPI.Regions;
using System.Collections;
using UnityEngine;

namespace DebugMenu.Scripts.Act1;

public class Act1MapSequence : BaseMapSequence
{
    /// <summary>
    /// Overrides starting region for a newly generated map.
    /// </summary>
    public static bool RegionOverride = false;
    public static string RegionNameOverride = "No region selected";

    private readonly Act1 Act = null;
    private readonly DebugWindow Window = null;

    public Act1MapSequence(Act1 act)
    {
        this.Act = act;
        this.Window = act.Window;
    }

    public override void OnGUI()
    {
        if (PaperGameMap.m_Instance != null && PaperGameMap.Instance.ChangingRegion) {
            return;
        }

        bool skipNextNode = Act1.SkipNextNode;
        if (Window.Toggle("Skip next node", ref skipNextNode))
            ToggleSkipNextNode();

        bool activateAllNodes = Act1.ActivateAllMapNodesActive;
        if (Window.Toggle("Activate all Map nodes", ref activateAllNodes))
            ToggleAllNodes();

        

        Act.DrawSequencesGUI();

        Window.LabelHeader("<b>Override Region</b>");
        ButtonListPopup.OnGUI<ButtonListPopup>(Window, RegionNameOverride, "Override Region", RegionNameList, static (_, value, _) =>
        {
            RegionNameOverride = value;
        });

        if (Window.Button("Override Current Region", disabled: () => new(() => PaperGameMap.m_Instance == null || RegionNameOverride == null))) {
            Plugin.Instance.StartCoroutine(OverrideCurrentRegionSequence(PaperGameMap.Instance));
        }

        Window.Toggle("Override Map Next Run", ref RegionOverride);
    }

    public static IEnumerator OverrideCurrentRegionSequence(PaperGameMap instance) {
        RegionData data = RegionManager.AllRegionsCopy.Find((a) => a.name == Act1MapSequence.RegionNameOverride);
        if (data == null) {
            Plugin.Log.LogError($"[Act1MapSequence] Could not override current region, region [{RegionNameOverride}] could not found!");
            Configs.CurrentRegionOverride = string.Empty;
            yield break;
        }
        Configs.CurrentRegionOverride = RegionNameOverride;
        instance.ChangingRegion = true;
        Singleton<ViewManager>.Instance.Controller.LockState = ViewLockState.Locked;
        yield return instance.HideMapSequence();
        Singleton<ViewManager>.Instance.Controller.LockState = ViewLockState.Locked;
        Singleton<ExplorableAreaManager>.Instance.TweenHangingLightColors(GameColors.Instance.gold, GameColors.Instance.gold, 0.25f);
        int num = 13;
        if (SaveFile.IsAscension) {
            num++;
        }
        RunState.Run.map = MapGenerator.GenerateMap(data, 3, num, null);
        RunState.Run.currentNodeId = RunState.Run.map.RootNode.id;
        instance.DataReader.DestroyScenery();
        yield return new WaitForSeconds(0.5f);
        Singleton<ViewManager>.Instance.SwitchToView(View.MapDefault);
        yield return new WaitForSeconds(0.5f);
        Singleton<ExplorableAreaManager>.Instance.TweenHangingLightColors(data.boardLightColor, data.cardsLightColor, 0.25f);
        data.FadeInAmbientAudio();
        yield return instance.ShowMapSequence(0.75f);
        Singleton<MapNodeManager>.Instance.SetAllNodesInteractable(nodesInteractable: false);
        bool finalRegion = RunState.CurrentRegionTier == 3;
        if (!finalRegion) {
            ChallengeActivationUI.TryShowActivation(AscensionChallenge.AllTotems);
        }
        yield return new WaitForSeconds(1f);
        AudioController.Instance.SetLoopAndPlay((!finalRegion) ? (Singleton<GameFlowManager>.Instance as Part1GameFlowManager).GameTableLoopId : "cabin_ambience");
        AudioController.Instance.SetLoopVolumeImmediate(0f);
        AudioController.Instance.FadeInLoop(1f, (Singleton<GameFlowManager>.Instance as Part1GameFlowManager).GameTableLoopVolume);
        Singleton<MapNodeManager>.Instance.FindAndSetActiveNodeInteractable();
        Singleton<ViewManager>.Instance.Controller.LockState = ViewLockState.Unlocked;
        instance.ChangingRegion = false;
    }

    public override void ToggleSkipNextNode()
    {
        Act1.SkipNextNode = !Act1.SkipNextNode;
    }

    public override void ToggleAllNodes()
    {
        Act1.ActivateAllMapNodesActive = !Act1.ActivateAllMapNodesActive;
        if (MapNodeManager.m_Instance != null)
        {
            MapNode node = Singleton<MapNodeManager>.Instance.ActiveNode;
            if (node == null)
                return;

            Singleton<MapNodeManager>.Instance.SetActiveNode(node);
        }
    }

    private Tuple<List<string>, List<string>> RegionNameList()
    {
        List<string> regionsNames = new() { null };
        regionsNames.AddRange(RegionManager.AllRegionsCopy.ConvertAll((a) => a.name));
        return new Tuple<List<string>, List<string>>(regionsNames, regionsNames);
    }
}