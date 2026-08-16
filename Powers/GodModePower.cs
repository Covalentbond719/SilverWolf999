using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using SilverWolf999.Cards;

namespace SilverWolf999.Powers;

/// <summary>
/// 无敌玩家（God Mode）buff：下2个回合开始时，各将一张"狼尊时刻"加入手牌。
/// 回合结束自然流失1层（2层持续2个回合；当回合结束不流失）。
/// </summary>
[RegisterPower]
public class GodModePower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    // 悬浮提示：预览"隐藏关：狼尊时刻"（升级后预览升级版）
    protected override IEnumerable<IHoverTip> AdditionalHoverTips
    {
        get
        {
            bool addUpgraded = GetInternalData<Data>()?.AddUpgraded ?? false;
            return [HoverTipFactory.FromCard<WolfMomentCard>(addUpgraded)];
        }
    }

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://SilverWolf999/images/powers/god_mode.png",
        BigIconPath: "res://SilverWolf999/images/powers/god_mode_big.png"
    );

    private class Data
    {
        public bool AddUpgraded;
    }

    protected override object InitInternalData()
    {
        return new Data();
    }

    /// <summary>设置之后回合开始塞入的"狼尊时刻"是否为升级版</summary>
    public void SetAddUpgraded(bool value)
    {
        GetInternalData<Data>().AddUpgraded = value;
    }

    // 当回合结束不流失，从下个回合开始算"下2个回合"
    public override async Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        SkipNextDurationTick = true;
        await Task.CompletedTask;
    }

    // 回合开始时，塞一张"狼尊时刻"到手牌
    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner.Player)
        {
            return;
        }

        var combat = Owner.CombatState;
        if (combat == null || Owner.Player == null)
        {
            return;
        }

        var wolf = combat.CreateCard<WolfMomentCard>(Owner.Player);
        if (GetInternalData<Data>().AddUpgraded)
        {
            CardCmd.Upgrade(wolf);
        }
        await CardPileCmd.AddGeneratedCardToCombat(wolf, PileType.Hand, Owner.Player);
    }

    // 回合结束自然流失1层（0层自动移除）
    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side != Owner.Side)
        {
            return;
        }

        await PowerCmd.TickDownDuration(this);
    }
}
