using DiskCardGame;
using GBC;
using System.Collections;
using UnityEngine;
using static DialogueDataUtil;
using static InscryptionAPI.Dialogue.DialogueManager;

namespace DebugMenu.Scripts.Popups;

public class DialogueEventPopup : PaginatedWindow
{
    private static bool FilterByGroup = false;

    private static bool PlayDialogue = false;
    private static bool PlayingDialogue = false;

    private string currentGroup;

    public static List<DialogueEvent> AllEvents = new();

    public static List<DialogueEvent> VanillaEvents = null;

    public static List<DialogueEvent> ModdedEvents = new();

    public override string PopupName => "Dialogue Events";

    public override Vector2 Size => new(1050f, 950f);

    public DialogueEventPopup()
    {
        ColumnWidth = 200f;
    }

    public override void PrintAllInfoToLog()
    {
        foreach (DialogueEvent allEvent in AllEvents)
        {
            Plugin.Log.LogMessage($"Group: {allEvent.groupId,-30} | ID: {allEvent.id}");
        }
        Plugin.Log.LogInfo($"Total All: {AllEvents.Count}");
    }
    public override void PrintVanillaInfoToLog()
    {
        foreach (DialogueEvent vanillaEvent in VanillaEvents)
        {
            Plugin.Log.LogMessage($"Group: {vanillaEvent.groupId,-30} | ID: {vanillaEvent.id}");
        }
        Plugin.Log.LogInfo($"Total Vanilla: {VanillaEvents.Count}");
    }
    public override void PrintModdedInfoToLog()
    {
        foreach (Dialogue item in CustomDialogue)
        {
            Plugin.Log.LogMessage($"GUID: {item.PluginGUID,-30} | ID: {item.DialogueEvent.id}");
        }
        Plugin.Log.LogInfo($"Total Modded: {ModdedEvents.Count}");
    }
    public override void PrintSelectedInfoToLog()
    {
        Dictionary<DialogueEvent, string> dictionary = new();
        foreach (DialogueEvent e in AllEvents)
        {
            string text = CustomDialogue.Find((Dialogue x) => x.DialogueEvent == e)?.PluginGUID ?? "";
            if (currentGuid == text && !dictionary.ContainsKey(e))
            {
                dictionary.Add(e, text);
            }
        }
        PrintSelectedEventsToLog(dictionary, "GUID");
    }
    public override void HandleGuid()
    {
        base.HandleGuid();
        Label("Selected Group: " + currentGroup);
        Toggle("Play Dialogue When Selecting", ref PlayDialogue);
    }
    public override void HandleFilterText()
    {
        base.HandleFilterText();
        Toggle("Filter by Group", ref FilterByGroup);
    }
    public override bool HandleToggles()
    {
        bool toggleChanged = base.HandleToggles();
        toggleChanged = Toggle("Filter by Finale", ref FilterByGrimora) || toggleChanged;
        return toggleChanged;
    }

    public override void OnGUI()
    {
        if (SceneLoader.ActiveSceneName == "Start" || SceneLoader.ActiveSceneName == "Loading")
        {
            PlayingDialogue = false;
        }
        else if (SaveManager.SaveFile.IsPart2)
        {
            PlayingDialogue = DialogueHandler.m_Instance?.Playing ?? false;
        }
        else
        {
            PlayingDialogue = TextDisplayer.m_Instance?.PlayingEvent ?? false;
        }

        AllEvents = new(Data.events);
        VanillaEvents ??= JsonUtility.FromJson<DialogueData>((Resources.Load("Data/Dialogue/dialogue_data") as TextAsset).text)?.events;
        ModdedEvents = CustomDialogue.Select(x => x.DialogueEvent).ToList();

        HandleOnGUI("Events", AllEvents, VanillaEvents, ModdedEvents);

        List<DialogueEvent> infoToShow = new();
        if (ShowAll)
        {
            infoToShow = AllEvents;
        }
        else if (ShowVanilla && !ShowModded)
        {
            infoToShow = new(VanillaEvents);
        }
        else if (ShowModded)
        {
            infoToShow = ModdedEvents;
        }

        if (!string.IsNullOrEmpty(filterText))
        {
            filterText.ToLowerInvariant();
            if (FilterByGuid)
            {
                infoToShow.RemoveAll(x => CustomDialogue.Find(y => y.DialogueEvent == x)?.PluginGUID?.ToLowerInvariant().Contains(filterText) != true);
            }
            else if (FilterByGroup)
            {
                infoToShow.RemoveAll(x => string.IsNullOrEmpty(x.groupId) || !x.groupId.ToLowerInvariant().Contains(filterText));
            }
            else
            {
                infoToShow.RemoveAll(x => string.IsNullOrEmpty(x.id) || !x.id.ToLowerInvariant().Contains(filterText));
            }
        }

        if (FilterByAct1)
            infoToShow.RemoveAll(x => !Part1Dialogue(x.groupId));

        if (FilterByAct2)
            infoToShow.RemoveAll(x => !Part2Dialogue(x.groupId));

        if (FilterByAct3)
            infoToShow.RemoveAll(x => !Part3Dialogue(x.groupId));

        if (FilterByAscension)
            infoToShow.RemoveAll(x => !AscensionDialogue(x.groupId));

        if (FilterByGrimora)
            infoToShow.RemoveAll(x => !FinaleDialogue(x.groupId));

        HandlePages(infoToShow.Count);
        StartNewColumn();

        if (infoToShow == null || infoToShow.Count == 0)
        {
            Label("No results");
            return;
        }

        startIndex = pageNum * NumElements;
        if (pageNum > infoToShow.Count / NumElements)
        {
            pageNum = infoToShow.Count / NumElements;
            endIndex = infoToShow.Count;
        }
        else
        {
            endIndex = Mathf.Min(infoToShow.Count, pageNum * NumElements + NumElements);
        }

        int row = 1;
        for (int i = startIndex; i < endIndex; i++)
        {
            Dialogue customDialogue = CustomDialogue.Find(x => x.DialogueEvent == infoToShow[i]);
            string eventId = string.IsNullOrEmpty(infoToShow[i].id) ? "no ID" : infoToShow[i].id;
            string group = string.IsNullOrEmpty(infoToShow[i].groupId) ? "no group" : infoToShow[i].groupId;
            string pluginGuid = customDialogue?.PluginGUID ?? "";

            if (!ShowAll && ((!ShowModded && pluginGuid != "") || (!ShowVanilla && pluginGuid == "")))
                continue;

            string textToDisplay = infoToShow[i].id + "\n" + group + "\n" + pluginGuid;
            if (Button(textToDisplay, new(0f, 60f), null, () => new("Playing dialogue")
            {
                Disabled = PlayingDialogue
            }))
            {
                if (PlayDialogue)
                {
                    PlayDialogueEventSafe(eventId, infoToShow[i]);
                }
                currentGuid = pluginGuid;
                currentGroup = group;
            }
            if (row >= NumElements / 4)
            {
                StartNewColumn();
                row = 0;
            }
            row++;
        }
    }



    public void PrintSelectedEventsToLog(Dictionary<DialogueEvent, string> dialogueEventsWithVariableString, string stringName)
    {
        foreach (KeyValuePair<DialogueEvent, string> item in dialogueEventsWithVariableString)
        {
            Plugin.Log.LogMessage("ID: " + item.Key.id);
        }
        string arg = dialogueEventsWithVariableString.Values.FirstOrDefault() ?? ("No " + stringName);
        Plugin.Log.LogInfo($"{stringName}: {arg,-30}");
        Plugin.Log.LogInfo($"Total Selected: {dialogueEventsWithVariableString.Count}");
    }

    public static void PlayDialogueEventSafe(string eventId, DialogueEvent dialogueEvent)
    {
        Plugin.Log.LogInfo($"Playing dialogue event [{eventId}]");
        if (dialogueEvent == null)
        {
            Plugin.Log.LogError("Event is null");
            return;
        }
        if (!SaveManager.SaveFile.IsPart2)
        {
            Plugin.Instance.StartCoroutine(Dialogue3D(eventId, dialogueEvent));
        }
        else
        {
            Plugin.Instance.StartCoroutine(Dialogue2D(eventId, dialogueEvent));
        }
        Plugin.Log.LogInfo($"Finished playing event [{eventId}]");
    }
    private static IEnumerator Dialogue3D(string eventId, DialogueEvent dialogueEvent)
    {
        TextDisplayer textDisplayer = TextDisplayer.Instance;
        if (textDisplayer == null)
        {
            yield break;
        }
        if (textDisplayer.PlayingEvent)
        {
            textDisplayer.Interrupt();
        }
        yield return new WaitUntil(() => !textDisplayer.PlayingEvent);
        textDisplayer.PlayingEvent = PlayingDialogue = true;
        textDisplayer.skipToEnd = false;
        textDisplayer.startOfDialogueLines?.Invoke();
        int repeatIndex = DialogueEventsData.GetEventRepeatCount(eventId) - 1;
        List<DialogueEvent.Line> lines = dialogueEvent.GetLines(repeatIndex);
        foreach (DialogueEvent.Line item in lines)
        {
            textDisplayer.newDialogueEventLine?.Invoke(item);
            textDisplayer.ParseSpecialInstruction(item.specialInstruction);
            DialogueEvent.Speaker speaker = dialogueEvent.speakers[item.speakerIndex];
            string transformedMessage = DialogueParser.ParseDialogueCodes(Localization.Translate(item.text));
            Plugin.Log.LogMessage($"Speaker: {speaker} | Line: {transformedMessage}");
            if (!string.IsNullOrEmpty(transformedMessage))
            {
                textDisplayer.CurrentAdvanceMode = TextDisplayer.MessageAdvanceMode.Input;
                InteractionCursor.Instance.InteractionDisabled = true;
                ViewManager.Instance.OffsetFOV(-0.65f, 0.15f);
                UIManager.Instance.Effects.GetEffect<EyelidMaskEffect>().SetIntensity(0.4f, 0.15f);
                yield return new WaitForSeconds(0.15f);
                textDisplayer.triangleAnim.updateMode = AnimatorUpdateMode.Normal;
                textDisplayer.triangleAnim.ResetTrigger("clear");
                textDisplayer.triangleAnim.Play("idle", 0, 0f);
                textDisplayer.ShowMessage(transformedMessage, item.emotion, item.letterAnimation, speaker);
                yield return new WaitForSeconds(0.2f);
                textDisplayer.continuePressed = false;
                while (!textDisplayer.continuePressed)
                {
                    yield return new WaitForFixedUpdate();
                }
                textDisplayer.Clear();
                textDisplayer.triangleAnim.SetTrigger("clear");
                yield return new WaitForSeconds(0.05f);
                InteractionCursor.Instance.InteractionDisabled = false;
                ViewManager.Instance.OffsetFOV(0f, 0.15f);
                UIManager.Instance.Effects.GetEffect<EyelidMaskEffect>().SetIntensity(0f, 0.15f);
            }
            if (textDisplayer.skipToEnd)
            {
                break;
            }
        }
        textDisplayer.endOfDialogueLines?.Invoke();
        textDisplayer.PlayingEvent = PlayingDialogue = false;
    }
    private static IEnumerator Dialogue2D(string eventId, DialogueEvent dialogueEvent)
    {
        DialogueHandler dialogueHandler = DialogueHandler.Instance;
        TextBox textBox = TextBox.Instance;
        if (dialogueHandler == null || textBox == null)
        {
            yield break;
        }
        while (dialogueHandler.Playing)
        {
            textBox.forceEndLine = true;
        }
        dialogueHandler.Playing = PlayingDialogue = true;
        int repeatIndex = DialogueEventsData.GetEventRepeatCount(eventId) - 1;
        List<DialogueEvent.Line> lines = dialogueEvent.GetLines(repeatIndex);
        for (int i = 0; i < lines.Count; i++)
        {
            bool flag = i == 0;
            float delay = flag ? 0.25f : 0.1f;
            AudioController.Instance.SetLoopVolume(0.35f, 0.5f);
            InteractionCursor.Instance.InteractionDisabled = true;
            Plugin.Log.LogMessage("Line: " + lines[i].text);
            yield return textBox.InitiateShowMessage(Localization.Translate(lines[i].text), TextBox.Style.Nature, (DialogueSpeaker)null, TextBox.ScreenPosition.ForceTop, delay, flag);
            textBox.OnSpeakerEmotionChange(lines[i].emotion);
            if (textBox.sequentialText.PlayingMessage)
            {
                yield return new WaitForSeconds(0.1f);
            }
            textBox.continuePressed = false;
            while (textBox.sequentialText.PlayingMessage)
            {
                if (textBox.continuePressed)
                {
                    yield return textBox.SkipToEnd();
                }
                else
                {
                    yield return new WaitForEndOfFrame();
                }
            }
            if (!textBox.forceEndLine)
            {
                yield return textBox.WaitForContinueInput();
            }
            textBox.forceEndLine = false;
            InteractionCursor.Instance.InteractionDisabled = false;
            AudioController.Instance.SetLoopVolume(0.55f, 0.5f);
        }
        TextBox.Instance.Shake();
        yield return new WaitForSeconds(0.2f);
        TextBox.Instance.Hide();
        dialogueHandler.Playing = PlayingDialogue = false;
    }

    public bool Part1Dialogue(string groupId)
    {
        return groupId != null && !Part2Dialogue(groupId) && !Part3Dialogue(groupId) && !AscensionDialogue(groupId) && !FinaleDialogue(groupId);
    }
    public bool Part2Dialogue(string groupId)
    {
        return groupId?.StartsWith("GBC") ?? false;
    }
    public bool Part3Dialogue(string groupId)
    {
        return groupId != null && (groupId.StartsWith("Part 3") || groupId.StartsWith("Talking Angler") || groupId.StartsWith("Talking Blue Mage"));
    }
    public bool FinaleDialogue(string groupId)
    {
        return groupId?.StartsWith("Finale") ?? false;
    }
    public bool AscensionDialogue(string groupId)
    {
        return groupId?.StartsWith("Ascension") ?? false;
    }
}