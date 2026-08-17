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
public class AhaStrikeCard : ModCardTemplate
{
    // 基础数值：6点伤害
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Damage(6m, ValueProp.Move),
    ];

    // 打击tag
    protected override HashSet<CardTag> CanonicalTags =>
    [
        CardTag.Strike,
    ];

    // 悬浮提示：预览"增笑"
    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<LaughBoostPower>(),
    ];

    public AhaStrikeCard() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 造成6点伤害
        await DamageCmd.Attack(DynamicVars["Damage"].IntValue)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target!)
            .Execute(choiceContext);

        // 本回合内+1增笑（临时）
        await PowerCmd.Apply<TemporaryLaughBoostPower>(choiceContext, Owner.Creature, 1, Owner.Creature, null);
    }

    protected override void OnUpgrade()
    {
        // 伤害 6 -> 9（暂定，可调整）
        DynamicVars["Damage"].UpgradeValueBy(3m);
    }
}
