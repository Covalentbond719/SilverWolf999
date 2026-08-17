using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using SilverWolf999.Powers;

namespace SilverWolf999.Cards;

// 注册到Token卡池（与小刀Shiv、君王之剑同类：只被效果创造，不进商店/奖励，无稀有度）
[RegisterCard(typeof(TokenCardPool))]
public class WolfMomentCard : ModCardTemplate
{
    // X费（打出时消耗全部能量）
    protected override bool HasEnergyCostX => true;

    // 基础数值：每hit 7点欢愉伤害（公式实时计算）
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.ComputedDamage("Damage", PunchlineDamage.Resolve, 7m, ValueProp.Move),
    ];

    // "欢愉"词条提示 + 好活当赏/增笑
    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromKeyword(MyKeywords.Elation),
        HoverTipFactory.FromPower<CertifiedBangerTwoPower>(),
        HoverTipFactory.FromPower<LaughBoostPower>(),
    ];

    // 消耗
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Exhaust,
    ];

    public WolfMomentCard() : base(0, CardType.Attack, CardRarity.Token, TargetType.AllEnemies, false)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int x = ResolveEnergyXValue();
        if (x <= 0)
        {
            return;
        }

        var combat = CombatState;
        if (combat == null)
        {
            return;
        }

        decimal damage = DynamicVars.EvaluateValueOrDefault("Damage");

        // 第一阶段：对全体敌人造成7欢愉伤害（未升级1次 / 升级后2次）
        int aoeHits = IsUpgraded ? 2 : 1;
        if (aoeHits > 0)
        {
            await DamageCmd.Attack(damage)
                .WithHitCount(aoeHits)
                .FromCard(this, cardPlay)
                .TargetingAllOpponents(combat)
                .Execute(choiceContext);
        }

        // 第二阶段：对随机敌人造成7欢愉伤害（未升级X次 / 升级后X+1次）
        int singleHits = IsUpgraded ? x + 1 : x;
        await DamageCmd.Attack(damage)
            .WithHitCount(singleHits)
            .FromCard(this, cardPlay)
            .TargetingRandomOpponents(combat)
            .Execute(choiceContext);
    }
}
