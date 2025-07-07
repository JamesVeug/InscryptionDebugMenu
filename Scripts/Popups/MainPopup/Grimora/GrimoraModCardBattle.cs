using DebugMenu.Scripts.Acts;
using DiskCardGame;
using GrimoraMod;
using UnityEngine;

namespace DebugMenu.Scripts.Grimora;

public static partial class GrimoraModHelper
{
    internal static void DisplayHammer(DebugWindow Window)
    {
        Window.LabelHeader("Hammer");
        using (Window.HorizontalScope(3))
        {
            ConsumableItemSlot slot = GrimoraItemsManagerExt.Instance.HammerSlot;
            HammerItemExt item = slot?.Item as HammerItemExt;
            if (item == null)
                return;

            int gloss = Shader.PropertyToID("_GlossMapScale");
            Material mat = item.transform.Find("Handle").GetComponent<MeshRenderer>().material;

            Window.Label($"Counter: {HammerItemExt.useCounter}/3");
            if (Window.Button("Reset counter", disabled: () => new(() => GrimoraItemsManager.Instance.ActivatingItem)))
            {
                mat.SetFloat(gloss, 0f);
                if (HammerItemExt.useCounter > 2)
                {
                    slot.gameObject.SetActive(true);
                    slot.SetEnabled(true);
                    item.gameObject.SetActive(true);
                    item.PlayEnterAnimation();
                }
                HammerItemExt.useCounter = 0;
            }
            if (Window.Button("Increase counter", disabled: () => new(() => HammerItemExt.useCounter > 2 || GrimoraItemsManager.Instance.ActivatingItem)))
            {
                HammerItemExt.useCounter++;
                if (HammerItemExt.useCounter == 2)
                {
                    mat.SetFloat(gloss, 1f);
                }
                else if (HammerItemExt.useCounter == 3)
                {
                    item.PlayExitAnimation();
                    item.gameObject.SetActive(false);
                    slot.SetEnabled(false);
                    slot.gameObject.SetActive(false);
                }
            }
        }
    }
}
