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

// 注册卡牌到对应卡池
[RegisterCard(typeof(SilentCardPool))]
public class AmberCard : ModCardTemplate
{
    // 悬浮提示：预览授予的"琥珀"buff
    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<AmberPower>(),
    ];

    public AmberCard() : base(3, CardType.Power, CardRarity.Rare, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 你的格挡能抵消单次攻击的溢出伤害
        await PowerCmd.Apply<AmberPower>(choiceContext, Owner.Creature, 1, Owner.Creature, null);
    }

    // 升级：3费 -> 2费
    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
