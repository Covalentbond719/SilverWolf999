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

// 注册卡牌到铁甲战士卡池
[RegisterCard(typeof(NecrobinderCardPool))]
public class SunkAgainCard : ModCardTemplate
{
    // 基础数值：4点欢愉伤害（公式）；抽牌数（升级后走公式，未升级固定1）
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.ComputedDamage("Damage", PunchlineDamage.Resolve, 4m, ValueProp.Move),
        ModCardVars.Computed("Draw", PunchlineDamage.ResolveWhenUpgraded, 1m),
    ];

    // 悬浮提示："欢愉"词条 + 好活当赏/增笑
    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromKeyword(MyKeywords.Elation),
        HoverTipFactory.FromPower<CertifiedBangerTwoPower>(),
        HoverTipFactory.FromPower<LaughBoostPower>(),
    ];

    public SunkAgainCard() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 欢愉伤害
        decimal damage = DynamicVars.EvaluateValueOrDefault("Damage");
        await DamageCmd.Attack(damage)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target!)
            .Execute(choiceContext);

        // 抽牌：升级后抽牌数走欢愉公式（基础1，未升级固定1）
        decimal draw = DynamicVars.EvaluateValueOrDefault("Draw");
        if (draw > 0)
        {
            await CardPileCmd.Draw(choiceContext, draw, Owner);
        }
    }

    protected override void OnUpgrade()
    {
        // 伤害 4 -> 6；抽牌变为欢愉公式
        DynamicVars["Damage"].UpgradeValueBy(2m);
    }
}
