using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using SilverWolf999.Powers;

namespace SilverWolf999.Cards;

// 注册卡牌到铁甲战士卡池
[RegisterCard(typeof(NecrobinderCardPool))]
public class LoanFutureCard : ModCardTemplate
{
    // 消耗；升级后去除
    public override IEnumerable<CardKeyword> CanonicalKeywords => IsUpgraded ? [] : [CardKeyword.Exhaust];

    // 悬浮提示：预览"隐藏分" + "下回合能量"
    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<HiddenMmrPower>(),
        HoverTipFactory.FromPower<EnergyNextTurnPower>(),
    ];

    public LoanFutureCard() : base(0, CardType.Skill, CardRarity.Common, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 立即获得5点隐藏分
        await PowerCmd.Apply<HiddenMmrPower>(choiceContext, Owner.Creature, 5, Owner.Creature, null);
        // 下个回合获得1点能量（原版"下回合能量"能力）
        await PowerCmd.Apply<EnergyNextTurnPower>(choiceContext, Owner.Creature, 1, Owner.Creature, null);
    }

    // 升级：去除消耗
    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Exhaust);
    }
}
