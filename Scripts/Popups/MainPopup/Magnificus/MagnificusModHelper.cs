using DiskCardGame;

namespace DebugMenu.Scripts.Magnificus;

public static partial class MagnificusModHelper
{
    internal static bool _enabled;
    public static bool Enabled => _enabled;
    public const string Guid = "silenceman.inscryption.magnificusmod";

    internal static void PatchMagnificuMod()
    {
        Plugin.HarmonyInstance.PatchAll(typeof(MagnificusModHelper));
        _allSpellCards = ScriptableObjectLoader<CardInfo>.AllData.FindAll(x => x.HasTrait(Trait.EatsWarrens));
    }
}
