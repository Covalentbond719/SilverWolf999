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
public class MirthCard : ModCardTemplate
{
    // 卡牌旁出现的提示方框：预览涉及的能力
    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<PunchlinePower>(),
        HoverTipFactory.FromPower<HiddenMmrPower>(),
    ];

    public MirthCard() : base(1, CardType.Power, CardRarity.Rare, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 挂上"每获得1笑点→获得1隐藏分"的被动
        await PowerCmd.Apply<MirthPower>(choiceContext, Owner.Creature, 1, Owner.Creature, null);
    }

    // 升级：固有（参考原版 Afterimage）
    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Innate);
    }
}
