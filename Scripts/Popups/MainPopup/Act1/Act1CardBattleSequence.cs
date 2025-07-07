using DebugMenu.Scripts.Acts;
using DiskCardGame;

namespace DebugMenu.Scripts.Act1;

public class Act1CardBattleSequence : BaseCardBattleSequence
{
    public override int PlayerBones => ResourcesManager.Instance.PlayerBones;
    public override int ScalesBalance => LifeManager.Instance.Balance;
    public override int PlayerEnergy => ResourcesManager.Instance.PlayerEnergy;
    public override int PlayerMaxEnergy => ResourcesManager.Instance.PlayerMaxEnergy;
    public override CardDrawPiles CardDrawPiles => Singleton<Part1CardDrawPiles>.Instance;
    public Act1CardBattleSequence(DebugWindow window) : base(window)
    {
    }
}