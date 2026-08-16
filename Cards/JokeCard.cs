using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Cards;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using SilverWolf999.Powers;

namespace SilverWolf999.Cards;

// 注册卡牌到指定池（这里是无色）
[RegisterCard(typeof(ColorlessCardPool))]
public class JokeCard : ModCardTemplate
{
    // 消耗；升级后去除
    public override IEnumerable<CardKeyword> CanonicalKeywords => IsUpgraded ? [] : [CardKeyword.Exhaust];

    // 悬浮提示：预览"增笑"
    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<LaughBoostPower>(),
    ];

    public JokeCard() : base(0, CardType.Skill, CardRarity.Common, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 本回合内获得1点增笑（临时包装能力，回合结束自动撤销）
        await PowerCmd.Apply<TemporaryLaughBoostPower>(choiceContext, Owner.Creature, 1, Owner.Creature, null);
    }

    // 升级：去除消耗
    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Exhaust);
    }
}
