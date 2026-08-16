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

// 注册卡牌到指定池（这里是无色）
[RegisterCard(typeof(ColorlessCardPool))]
public class SmileCard : ModCardTemplate
{
    // 卡牌基础数值：5格挡（欢愉，公式计算） / 5笑点 / 4隐藏分 / 3好活当赏（升级后 7 / 6 / 5）
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.ComputedBlock("Block", PunchlineDamage.Resolve, 5m, ValueProp.Move),
        ModCardVars.Int("Punchline", 5),
        ModCardVars.Int("Mmr", 4),
        ModCardVars.Int("Banger", 3),
    ];

    // 卡牌旁出现的提示方框："欢愉"词条说明 + 预览获得的能力
    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromKeyword(MyKeywords.Joy),
        HoverTipFactory.FromPower<PunchlinePower>(),
        HoverTipFactory.FromPower<HiddenMmrPower>(),
        HoverTipFactory.FromPower<CertifiedBangerTwoPower>(),
    ];

    public SmileCard() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var creature = Owner.Creature;

        // 欢愉：获得公式格挡（基础5，受增笑/好活当赏影响）
        decimal block = DynamicVars.EvaluateValueOrDefault("Block");
        await CreatureCmd.GainBlock(creature, block, ValueProp.Move, cardPlay);

        await PowerCmd.Apply<PunchlinePower>(choiceContext, creature, DynamicVars["Punchline"].IntValue, creature, null);
        await PowerCmd.Apply<HiddenMmrPower>(choiceContext, creature, DynamicVars["Mmr"].IntValue, creature, null);
        // 好活当赏给"剩余2回合"版（与999卡带一致）
        await PowerCmd.Apply<CertifiedBangerTwoPower>(choiceContext, creature, DynamicVars["Banger"].IntValue, creature, null);
    }

    protected override void OnUpgrade()
    {
        // 5→7 / 4→6 / 3→5
        DynamicVars["Punchline"].UpgradeValueBy(2m);
        DynamicVars["Mmr"].UpgradeValueBy(2m);
        DynamicVars["Banger"].UpgradeValueBy(2m);
    }
}
