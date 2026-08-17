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

// 注册卡牌到铁甲战士卡池
[RegisterCard(typeof(NecrobinderCardPool))]
public class MyTurnDrawCard : ModCardTemplate
{
    // 悬浮提示：预览授予的buff + "笑点"
    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<DrawOnPunchlinePower>(),
        HoverTipFactory.FromPower<PunchlinePower>(),
    ];

    public MyTurnDrawCard() : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 每获得12（升级后10）个笑点抽1张牌
        var buff = await PowerCmd.Apply<DrawOnPunchlinePower>(choiceContext, Owner.Creature, 1, Owner.Creature, null);
        if (buff != null)
        {
            buff.SetStep(IsUpgraded ? 10 : 12);
        }
    }
}
