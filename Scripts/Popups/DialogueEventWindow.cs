using DebugMenu.Scripts.Utils;
using DiskCardGame;
using GBC;
using InscryptionAPI.Dialogue;
using System.Collections;
using UnityEngine;
using static DialogueDataUtil;
using static InscryptionAPI.Dialogue.DialogueManager;

namespace DebugMenu.Scripts.Popups;

public class DialogueEventPopup : BaseWindow
{
    private static bool FilterByGuid = false;

    private static bool FilterByGroup = false;

    private static bool ShowAll = true;

    private static bool ShowVanilla = true;

    private static bool ShowModded = true;

    private static bool FilterByAct1 = false;

    private static bool FilterByAct2 = false;

    private static bool FilterByAct3 = false;

    private static bool FilterByFinale = false;

    private static bool FilterByAscension = false;

    private static bool PlayDialogue = false;

    private static bool PlayingDialogue = false;

    private const int numElements = 40;

    public static int pageNum = 0;

    public static int startIndex = 0;

    public static int endIndex = numElements;

    private string filterText;

    private string currentGuid;

    private string currentGroup;

    public static List<DialogueEvent> AllEvents = new();

    public static List<DialogueEvent> VanillaEvents = null;

    public static List<DialogueEvent> ModdedEvents = new();

    public override string PopupName => "Dialogue Events";

    public override Vector2 Size => new(1100f, 1000f);

    public DialogueEventPopup()
    {
        ColumnWidth = 200f;
    }

    public override void OnGUI()
    {
        base.OnGUI();

        if (SceneLoader.ActiveSceneName == "Start" || SceneLoader.ActiveSceneName == "Loading")
            PlayingDialogue = false;
        else if (SaveManager.SaveFile.IsPart2)
            PlayingDialogue = DialogueHandler.Instance?.Playing ?? false;
        else
            PlayingDialogue = TextDisplayer.Instance?.PlayingEvent ?? false;

        AllEvents = new(Data.events);
        VanillaEvents ??= JsonUtility.FromJson<DialogueData>((Resources.Load("Data/Dialogue/dialogue_data") as TextAsset).text)?.events;
        ModdedEvents = CustomDialogue.Select(x => x.DialogueEvent).ToList();

        if (AllEvents == null || AllEvents.Count == 0)
        {
            Label("No events");
            return;
        }
        if (Button("Print All Event IDs"))
        {
            PrintAllEventsToLog();
        }
        if (Button("Print Vanilla Event IDs", null, null, () => new("Couldn't load vanilla data!")
        {
            Disabled = VanillaEvents == null || VanillaEvents.Count == 0
        }))
        {
            PrintVanillaEventsToLog();
        }
        if (Button("Print Modded Event IDs", null, null, () => new("No events")
        {
            Disabled = ModdedEvents == null || ModdedEvents.Count == 0
        }))
        {
            PrintModdedEventsToLog();
        }
        if (Button("Printed Events by Selected GUID"))
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
        if (Button("Printed Events by Selected Group"))
        {
            Dictionary<DialogueEvent, string> dictionary2 = new();
            foreach (DialogueEvent allEvent in AllEvents)
            {
                string text2 = allEvent.groupId ?? "no group";
                if (text2 == currentGroup && !dictionary2.ContainsKey(allEvent))
                {
                    dictionary2.Add(allEvent, text2);
                }
            }
            PrintSelectedEventsToLog(dictionary2, "Group");
        }
        
        Label("Selected GUID: " + currentGuid);
        Label("Selected Group: " + currentGroup);
        
        Toggle("Play Dialogue When Selecting", ref PlayDialogue);
        
        Label("Search Filter", (Vector2?)new Vector2(0f, base.RowHeight / 2f));
        filterText = TextField(filterText, (Vector2?)new Vector2(0f, base.RowHeight / 2f));
        
        Toggle("Filter by GUID", ref FilterByGuid);
        Toggle("Filter by Group", ref FilterByGroup);
        
        if (!ShowVanilla || !ShowModded)
            ShowAll = false;

        bool toggleChanged = Toggle("Show All Events", ref ShowAll);
        if (toggleChanged)
            ShowModded = ShowVanilla = ShowAll;
            FilterByAct1 = FilterByAct2 = FilterByAct3 = FilterByAscension = FilterByFinale = false;

        toggleChanged |= Toggle("Show Modded Events", ref ShowModded);
        if (toggleChanged && ShowModded)
            FilterByAct1 = FilterByAct2 = FilterByAct3 = FilterByAct3 = FilterByFinale = FilterByAscension = false;

        toggleChanged |= Toggle("Show Vanilla Events", ref ShowVanilla);
        if (toggleChanged && ShowVanilla)
            FilterByAct1 = FilterByAct2 = FilterByAct3 = FilterByAct3 = FilterByFinale = FilterByAscension = false;

        toggleChanged |= Toggle("Filter by Act 1", ref FilterByAct1);
        toggleChanged |= Toggle("Filter by Act 2", ref FilterByAct2);
        toggleChanged |= Toggle("Filter by Act 3", ref FilterByAct3);
        toggleChanged |= Toggle("Filter by Finale", ref FilterByFinale);
        toggleChanged |= Toggle("Filter by Ascension", ref FilterByAscension);

        List<DialogueEvent> eventsToShow = new();
        if (ShowAll || (ShowVanilla && ShowModded))
        {
            eventsToShow = AllEvents;
        }
        else if (ShowVanilla && !ShowModded)
        {
            eventsToShow = VanillaEvents;
        }
        else if (ShowModded)
        {
            eventsToShow = ModdedEvents;
        }
        if (!string.IsNullOrEmpty(filterText))
        {
            if (FilterByGuid)
            {
                eventsToShow = eventsToShow.FindAll((DialogueEvent x) => (CustomDialogue.Find((Dialogue y) => y.DialogueEvent == x)?.PluginGUID?.ContainsText(filterText, caseSensitive: false)).GetValueOrDefault());
            }
            else if (FilterByGroup)
            {
                eventsToShow = eventsToShow.FindAll((DialogueEvent x) => x.groupId?.ContainsText(filterText, caseSensitive: false) ?? false);
            }
            else
            {
                eventsToShow = eventsToShow.FindAll((DialogueEvent x) => x.id?.ContainsText(filterText, caseSensitive: false) ?? false);
            }
        }
        eventsToShow = eventsToShow.FindAll((DialogueEvent x) => (!FilterByAct1 && !FilterByAct2 && !FilterByAct3 && !FilterByAscension && !FilterByFinale) || (FilterByAct1 && Part1Dialogue(x.groupId)) || (FilterByAct2 && Part2Dialogue(x.groupId)) || (FilterByAct3 && Part3Dialogue(x.groupId)) || (FilterByAscension && AscensionDialogue(x.groupId)) || (FilterByFinale && FinaleDialogue(x.groupId)));
        if (Button("Next Page"))
        {
            IncreasePageIndexes(eventsToShow.Count);
        }
        if (Button("Previous Page"))
        {
            DecreasePageIndexes(eventsToShow.Count);
        }
        StartNewColumn();
        if (eventsToShow == null || eventsToShow.Count == 0)
        {
            Label("No events to show");
            return;
        }
        if (toggleChanged)
        {
            if (pageNum > eventsToShow.Count / numElements)
            {
                pageNum = eventsToShow.Count / numElements;
                endIndex = eventsToShow.Count;
            }
            else
            {
                endIndex = pageNum * numElements + numElements;
            }
            startIndex = pageNum * numElements;
        }
        if (endIndex > eventsToShow.Count)
        {
            endIndex = eventsToShow.Count;
        }
        if (startIndex < 0)
        {
            startIndex = 0;
        }
        int row = 1;
        for (int i = startIndex; i < endIndex; i++)
        {
            Dialogue customDialogue = CustomDialogue.Find(x => x.DialogueEvent == eventsToShow[i]);
            string eventId = string.IsNullOrEmpty(eventsToShow[i].id) ? "no ID" : eventsToShow[i].id;
            string group = string.IsNullOrEmpty(eventsToShow[i].groupId) ? "no group" : eventsToShow[i].groupId;
            string pluginGuid = customDialogue?.PluginGUID ?? "";

            if (!ShowAll && ((!ShowModded && pluginGuid != "") || (!ShowVanilla && pluginGuid == "")))
                continue;

            string textToDisplay = eventsToShow[i].id + "\n" + group + "\n" + pluginGuid;

            if (Button(textToDisplay, new(0f, 60f), null, () => new("Playing dialogue")
            {
                Disabled = PlayingDialogue
            }))
            {
                if (PlayDialogue)
                {
                    PlayDialogueEventSafe(eventId, eventsToShow[i]);
                }
                currentGuid = pluginGuid;
                currentGroup = group;
            }
            if (row >= numElements / 4)
            {
                StartNewColumn();
                row = 0;
            }
            row++;
        }
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

    public void PrintAllEventsToLog()
    {
        foreach (DialogueEvent allEvent in AllEvents)
        {
            Plugin.Log.LogMessage($"Group: {allEvent.groupId,-30} | ID: {allEvent.id}");
        }
        Plugin.Log.LogInfo($"Total All: {AllEvents.Count}");
    }

    public void PrintVanillaEventsToLog()
    {
        foreach (DialogueEvent vanillaEvent in VanillaEvents)
        {
            Plugin.Log.LogMessage($"Group: {vanillaEvent.groupId,-30} | ID: {vanillaEvent.id}");
        }
        Plugin.Log.LogInfo($"Total Vanilla: {VanillaEvents.Count}");
    }

    public void PrintModdedEventsToLog()
    {
        foreach (Dialogue item in CustomDialogue)
        {
            Plugin.Log.LogMessage($"GUID: {item.PluginGUID,-30} | ID: {item.DialogueEvent.id}");
        }
        Plugin.Log.LogInfo($"Total Modded: {ModdedEvents.Count}");
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

    private void IncreasePageIndexes(int maxCount)
    {
        pageNum++;
        if (endIndex == maxCount)
        {
            startIndex = pageNum = 0;
            endIndex = Mathf.Min(numElements, maxCount);
            return;
        }
        startIndex = pageNum * numElements;
        endIndex = pageNum * numElements + numElements;
        if (endIndex > maxCount)
        {
            endIndex = maxCount;
        }
    }

    private void DecreasePageIndexes(int maxCount)
    {
        pageNum--;
        if (startIndex == 0)
        {
            pageNum = maxCount / numElements;
            startIndex = pageNum * numElements;
            endIndex = maxCount;
            return;
        }
        startIndex = pageNum * numElements;
        endIndex = pageNum * numElements + numElements;
        if (startIndex < 0)
        {
            startIndex = 0;
        }
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