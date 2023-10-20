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
    public override string PopupName => "Internal Debug Tools";

    public override Vector2 Size => new(500f, 750f);

    public override void OnGUI()
    {
        base.OnGUI();
        Toggle("DEBUG_KEYS_ENABLED", ref ScriptDefines.DEBUG_KEYS_ENABLED, new(500f, 40f));
        Toggle("DISABLE_ASCENSION_OILPAINTING (new runs only)", ref ScriptDefines.DISABLE_ASCENSION_OILPAINTING, new(500f, 40f));

        Label("Debug Keys (THESE ARE HARD-CODED AND CANNOT BE CHANGED):", new(500f, 40f));
        Label(
            "Backquote (`) - Force Show Cursor" +
            "\nBackquote - Force On Fast Travel Node (Act 3)" +
            "\nLCtrl + LShift + UpArrow - Increase KCM challenge level (KCM start screen)" +
            "\nLCtrl + LShift + DownArrow - Decrease KCM challenge level (KCM start screen)" +
            "\nLCtrl + LShift + C - +1 Damage to Opponent (during battle)" +
            "\nBackquote + NumPad1 - Toggle perspective UI camera" +
            "\nBackquote + NumPad2 - Toggle orthogonal UI camera", new(500f, 500f));
    }
}