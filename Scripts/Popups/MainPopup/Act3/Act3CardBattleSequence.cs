using DebugMenu.Scripts.Acts;
using DiskCardGame;

namespace DebugMenu.Scripts.Act3;

public class Act3CardBattleSequence : BaseCardBattleSequence
{
    public override int PlayerBones => ResourcesManager.Instance.PlayerBones;
    public override int ScalesBalance => LifeManager.Instance.Balance;
    public override int PlayerEnergy => ResourcesManager.Instance.PlayerEnergy;
    public override int PlayerMaxEnergy => ResourcesManager.Instance.PlayerMaxEnergy;
    public override CardDrawPiles CardDrawPiles => Singleton<Part3CardDrawPiles>.Instance;
    public Act3CardBattleSequence(DebugWindow window) : base(window)
    {
    }

    public override void AddBones(int amount)
    {
        if (P03ModHelper.Enabled)
            base.AddBones(amount);
    }
    public override void RemoveBones(int amount)
    {
        if (P03ModHelper.Enabled)
            base.RemoveBones(amount);
    }
}