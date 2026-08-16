using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Cards;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using SilverWolf999.Powers;

namespace SilverWolf999.Cards;

// 注册卡牌到指定池（这里是无色）
[RegisterCard(typeof(ColorlessCardPool))]
public class CrosstalkCard : ModCardTemplate
{
    // 基础数值：获得1点增笑（升级2点）
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("Boost", 1),
    ];

    // 悬浮提示：预览"增笑"
    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<LaughBoostPower>(),
    ];

    public CrosstalkCard() : base(1, CardType.Power, CardRarity.Rare, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 获得永久增笑
        await PowerCmd.Apply<LaughBoostPower>(choiceContext, Owner.Creature, DynamicVars["Boost"].IntValue, Owner.Creature, null);
    }

    // 升级：1 -> 2
    protected override void OnUpgrade()
    {
        DynamicVars["Boost"].UpgradeValueBy(1m);
    }
}
