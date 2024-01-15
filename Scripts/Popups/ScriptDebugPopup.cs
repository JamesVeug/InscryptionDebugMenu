using DebugMenu.Scripts.Utils;
using DiskCardGame;
using GBC;
using InscryptionAPI.Card;
using InscryptionAPI.Dialogue;
using System.Collections;
using System.Text;
using UnityEngine;
using static DialogueDataUtil;
using static InscryptionAPI.Dialogue.DialogueManager;

namespace DebugMenu.Scripts.Popups;

public class ScriptDebugPopup : BaseWindow
{
    public override string PopupName => "Debug Tools";

    public override Vector2 Size => new(500f, 400f);

    public override void OnGUI()
    {
        base.OnGUI();
        Toggle("DEBUG_KEYS_ENABLED", ref ScriptDefines.DEBUG_KEYS_ENABLED, new(500f, 40f));
        Toggle("DISABLE_ASCENSION_OILPAINTING <b>(new runs only)</b>", ref ScriptDefines.DISABLE_ASCENSION_OILPAINTING, new(500f, 40f));

        Label("Debug Keys (THESE ARE HARD-CODED AND CANNOT BE CHANGED):", new(500f, 40f));
        Label(
            "<b>Backquote (`):</b> Force Show Cursor" +
            "\n<b>Backquote:</b> Force On Fast Travel Node (Act 3)" +
            "\n<b>LCtrl + LShift + UpArrow:</b> Increase KCM challenge level <b>(KCM start screen)</b>" +
            "\n<b>LCtrl + LShift + DownArrow:</b> Decrease KCM challenge level <b>(KCM start screen)</b>" +
            "\n<b>LCtrl + LShift + C:</b> +1 Damage to Opponent (during battle)" +
            "\n<b>Backquote + NumPad1:</b> Toggle perspective UI camera" +
            "\n<b>Backquote + NumPad2:</b> Toggle orthogonal UI camera", new(500f, 500f));
    }
}