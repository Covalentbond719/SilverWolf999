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

// 注册卡牌到亡灵契约师卡池
[RegisterCard(typeof(NecrobinderCardPool))]
public class FirewallCard : ModCardTemplate
{
    // 固有 + 保留
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Innate,
        CardKeyword.Retain,
    ];

    // 悬浮提示：预览授予的buff
    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<FirewallPower>(),
    ];

    public FirewallCard() : base(3, CardType.Power, CardRarity.Rare, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 获得状态牌时将其消耗
        await PowerCmd.Apply<FirewallPower>(choiceContext, Owner.Creature, 1, Owner.Creature, null);
    }

    // 升级：3费 -> 2费
    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
