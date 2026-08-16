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

namespace SilverWolf999.Cards;

// 注册卡牌到指定池（这里是无色）
[RegisterCard(typeof(ColorlessCardPool))]
public class WolfMomentCard : ModCardTemplate
{
    // X费（打出时消耗全部能量）
    protected override bool HasEnergyCostX => true;

    // 基础数值：每hit 7点欢愉伤害（公式实时计算）
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.ComputedDamage("Damage", PunchlineDamage.Resolve, 7m, ValueProp.Move),
    ];

    // "欢愉伤害"词条提示
    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromKeyword(MyKeywords.ElationDamage),
    ];

    // 消耗
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Exhaust,
    ];

    public WolfMomentCard() : base(0, CardType.Attack, CardRarity.Rare, TargetType.AllEnemies, true)
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
