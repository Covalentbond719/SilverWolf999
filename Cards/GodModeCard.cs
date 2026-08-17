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

// 注册到Token卡池（与小刀Shiv、君王之剑同类：只被效果创造，不进商店/奖励，无稀有度）
[RegisterCard(typeof(TokenCardPool))]
public class GodModeCard : ModCardTemplate
{
    // 保留
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Retain,
    ];

    // 卡牌旁出现的提示方框：预览"隐藏关：狼尊时刻"（升级后预览升级版）+ 无敌玩家buff
    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromCard<WolfMomentCard>(IsUpgraded),
        HoverTipFactory.FromPower<GodModePower>(),
    ];

    public GodModeCard() : base(0, CardType.Power, CardRarity.Token, TargetType.Self, false)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var combat = CombatState;
        if (combat == null)
        {
            return;
        }

        // 直接塞一张"狼尊时刻"到手牌（升级则塞升级版）
        var wolf = combat.CreateCard<WolfMomentCard>(Owner);
        if (IsUpgraded)
        {
            CardCmd.Upgrade(wolf);
        }
        await CardPileCmd.AddGeneratedCardToCombat(wolf, PileType.Hand, Owner);

        // 给"无敌玩家"buff（2层，回合结束自然流失1层，覆盖下2个回合开始）
        var buff = await PowerCmd.Apply<GodModePower>(choiceContext, Owner.Creature, 2, Owner.Creature, null);
        if (IsUpgraded && buff != null)
        {
            // 升级后，buff 回合开始塞的也是"狼尊时刻+"
            buff.SetAddUpgraded(true);
        }
    }
}
