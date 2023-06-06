﻿using DebugMenu.Scripts.Acts;

namespace DebugMenu.Scripts.Magnificus;

public class ActMagnificus : BaseAct
{
    public ActMagnificus(DebugWindow window) : base(window)
    {
    }

    public override void Update()
    {

    }

    public override void OnGUI()
    {
        Window.LabelHeader("Magnificus Act");
        Window.Label("Support not started");
    }

    public override void OnGUIMinimal()
    {
        OnGUI();
    }

    public override void Restart()
    {
        // TODO:
    }

    public override void Reload()
    {
        // TODO:
    }
}