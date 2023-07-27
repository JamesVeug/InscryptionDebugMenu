using System.Collections;
using BepInEx.Logging;
using DebugMenu.Scripts.Acts;
using DiskCardGame;
using GBC;
using UnityEngine;

namespace DebugMenu.Scripts.Magnificus;

public class MagnificusCardBattleSequence : BaseCardBattleSequence
{
	public override int PlayerBones => MagnificusResourcesManager.Instance.PlayerBones;
	public override int ScalesBalance => MagnificusLifeManager.Instance.Balance;
	public override int PlayerEnergy => MagnificusResourcesManager.Instance.PlayerEnergy;
	public override int PlayerMaxEnergy => MagnificusResourcesManager.Instance.PlayerMaxEnergy;
    public override CardDrawPiles CardDrawPiles => Singleton<MagnificusCardDrawPiles>.Instance;
    public MagnificusCardBattleSequence(DebugWindow window) : base(window)
	{
	}
	
	public override void OnGUI()
	{
		base.OnGUI();
	}
}