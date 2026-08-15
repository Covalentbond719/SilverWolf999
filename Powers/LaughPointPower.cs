using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace SilverWolf999.Powers;

/// <summary>
/// 笑点：回合结束时，对所有敌人造成伤害，随后本状态消失，
/// 并在下回合开始时获得相同层数的"好活当赏-剩余2回合"。
///
/// 伤害公式：伤害 = (2 + 增笑) × (1 + 3x / (x + 24))，其中 x 为笑点层数。
/// </summary>
[RegisterPower]
public class LaughPointPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://SilverWolf999/images/powers/laugh_point.png",
        BigIconPath: "res://SilverWolf999/images/powers/laugh_point_big.png"
    );

    // 描述里 {Damage} 显示的数值（仅显示用，实际伤害在触发时直接计算）
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(2m, ValueProp.Unpowered)];

    /// <summary>伤害 = (2 + 增笑) × (1 + 3x / (x + 24))，x = 笑点层数</summary>
    private decimal CalculateEndOfTurnDamage()
    {
        decimal x = Amount;
        decimal boost = Owner.GetPowerAmount<LaughBoostPower>();
        decimal baseDamage = Math.Max(2m + boost, 0m);
        return baseDamage * (1m + 3m * x / (x + 24m));
    }

    // 每当自身或增笑的层数变化时，刷新描述里的伤害显示
    public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (power == this || (power is LaughBoostPower && power.Owner == Owner))
        {
            DynamicVars.Damage.BaseValue = Math.Round(CalculateEndOfTurnDamage(), 2);
        }
        await Task.CompletedTask;
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        // 只在拥有者所在的阵营回合结束时触发
        if (side != Owner.Side)
        {
            return;
        }

        Flash();

        // 对全体存活敌人造成伤害（固定不吃力量加成，可被格挡）
        var combatState = Owner.CombatState;
        if (combatState == null)
        {
            return;
        }
        var enemies = combatState.GetOpponentsOf(Owner).Where(c => c.IsAlive).ToList();
        if (enemies.Count > 0)
        {
            await CreatureCmd.Damage(choiceContext, enemies, CalculateEndOfTurnDamage(), ValueProp.Unpowered, Owner);
        }

        // 清空自身，并在"下回合"转化为同层数的"好活当赏-剩余2回合"
        // （实际挂载发生在回合结束结算阶段，对玩家来说下回合开始时已可见）
        await PowerCmd.Apply<AppreciationTwoPower>(choiceContext, Owner, Amount, Owner, null);
        await PowerCmd.Remove(this);
    }
}
