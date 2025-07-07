using DebugMenu.Scripts.Acts;
using DiskCardGame;
using MagnificusMod;
using System.Collections;
using UnityEngine;

namespace DebugMenu.Scripts.Magnificus;

public class MagnificusCardBattleSequence : BaseCardBattleSequence
{
    public override int PlayerBones => MagnificusResourcesManager.Instance.PlayerBones;
    public override int ScalesBalance => MagnificusLifeManager.Instance.Balance;
    public override int PlayerEnergy => MagnificusResourcesManager.Instance.PlayerEnergy;
    public override int PlayerMaxEnergy => MagnificusResourcesManager.Instance.PlayerMaxEnergy;
    public override CardDrawPiles CardDrawPiles => Singleton<MagnificusCardDrawPiles>.Instance;
    private int PlayerLife => MagnificusLifeManager.Instance.PlayerLife;
    private int OpponentLife => MagnificusLifeManager.Instance.OpponentLife;

    public MagnificusCardBattleSequence(DebugWindow window) : base(window)
    {
        hasBones = false;
        hasEnergy = false;
    }

    public override void OnGUI()
    {
        base.OnGUI();
        if (MagnificusModHelper.Enabled)
            MagnificusModHelper.RefreshSpells(this.Window);
    }

    public override void ManageScaleDamage()
    {
        using (Window.HorizontalScope(3))
        {
            Window.Label($"Player\n {PlayerLife}");

            if (Window.Button("+2"))
                Plugin.Instance.StartCoroutine(TakeDamage(-2));

            if (Window.Button("-2"))
                Plugin.Instance.StartCoroutine(TakeDamage(2));
        }
        using (Window.HorizontalScope(3))
        {
            Window.Label($"Opponent\n {OpponentLife}");

            if (Window.Button("+2"))
                Plugin.Instance.StartCoroutine(DealDamage(-2));

            if (Window.Button("-2"))
                Plugin.Instance.StartCoroutine(DealDamage(2));
        }
    }

    public override IEnumerator TakeDamage(int amount)
    {
        dealingScaleDamage = true;
        if (Configs.DisablePlayerDamage || amount < 0)
        {
            Singleton<MagnificusLifeManager>.Instance.playerLife -= amount;
            MagnificusLifeManager.Instance.playerLifeCounter.ShowValue(Singleton<MagnificusLifeManager>.Instance.playerLife);
        }
        else
            yield return MagnificusMod.Generation.LifeManagerStuff(amount, true);

        dealingScaleDamage = false;
    }

    public override IEnumerator DealDamage(int amount)
    {
        dealingScaleDamage = true;
        if (Configs.DisableOpponentDamage || amount < 0)
        {
            Singleton<MagnificusLifeManager>.Instance.opponentLife -= amount;
            MagnificusLifeManager.Instance.opponentLifeCounter.ShowValue(Singleton<MagnificusLifeManager>.Instance.opponentLife);
        }
        else
            yield return MagnificusMod.Generation.LifeManagerStuff(amount, false);

        dealingScaleDamage = false;
    }

    public override void AutoLoseBattle()
    {
        if (!dealingScaleDamage)
            Plugin.Instance.StartCoroutine(TakeDamage(PlayerLife));
    }
    public override void AutoWinBattle()
    {
        if (!dealingScaleDamage)
            Plugin.Instance.StartCoroutine(DealDamage(OpponentLife));
    }

    public override IEnumerator ResetScale()
    {
        int startingPlayerHealth = 10;
        int startingOpponentHealth = 10;
        if (SaveManager.saveFile.ascensionActive)
        {
            if (Generation.challenges.Contains("FadingMox"))
            {
                startingPlayerHealth = KayceeStorage.FleetingLife;
            }
            if (Generation.challenges.Contains("MoreHpOpponent"))
            {
                startingOpponentHealth = 15;
            }
        }

        Singleton<MagnificusLifeManager>.Instance.playerLife = startingPlayerHealth;
        Singleton<MagnificusLifeManager>.Instance.opponentLife = startingOpponentHealth;
        MagnificusLifeManager.Instance.playerLifeCounter.ShowValue(startingPlayerHealth);
        MagnificusLifeManager.Instance.opponentLifeCounter.ShowValue(startingOpponentHealth);
        yield break;
    }

}