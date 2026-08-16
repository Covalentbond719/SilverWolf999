using System;
using STS2RitsuLib.Cards.DynamicVars;
using SilverWolf999.Powers;

namespace SilverWolf999.Cards;

/// <summary>
/// 欢愉伤害公式（笑点公式）：
/// 伤害 = (基础值 + 增笑) × (1 + 3x / (x + 24))，x = 好活当赏合计（剩余1回合 + 剩余2回合）。
/// 各卡牌通过 ComputedDamage 传入不同的基础值。
/// </summary>
public static class PunchlineDamage
{
    public static decimal Resolve(ComputedDynamicVarContext ctx)
    {
        var creature = ctx.SourceCreature;
        decimal boost = creature?.GetPowerAmount<LaughBoostPower>() ?? 0;
        decimal appreciation = (creature?.GetPowerAmount<CertifiedBangerOnePower>() ?? 0)
                             + (creature?.GetPowerAmount<CertifiedBangerTwoPower>() ?? 0);
        decimal baseDamage = Math.Max(ctx.BaseValue + boost, 0m);
        return baseDamage * (1m + 3m * appreciation / (appreciation + 24m));
    }
}
