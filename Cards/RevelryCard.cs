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
public class RevelryCard : ModCardTemplate
{
    // X费（打出时消耗全部能量）
    protected override bool HasEnergyCostX => true;

    // 消耗
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Exhaust,
    ];

    // 基础数值：每X 4点格挡（欢愉公式）/ 3个笑点（升级后格挡5）
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.ComputedBlock("Block", PunchlineDamage.Resolve, 4m, ValueProp.Move),
        ModCardVars.Int("Punchline", 3),
    ];

    // "欢愉"词条提示（格挡走公式）+ 好活当赏/增笑 + "笑点"
    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromKeyword(MyKeywords.Elation),
        HoverTipFactory.FromPower<CertifiedBangerTwoPower>(),
        HoverTipFactory.FromPower<LaughBoostPower>(),
        HoverTipFactory.FromPower<PunchlinePower>(),
    ];

    public RevelryCard() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int x = ResolveEnergyXValue();
        if (x <= 0)
        {
            return;
        }

        // 获得4X个笑点
        int punchline = DynamicVars["Punchline"].IntValue * x;
        await PowerCmd.Apply<PunchlinePower>(choiceContext, Owner.Creature, punchline, Owner.Creature, null);

        // 欢愉：获得4X点格挡（每X走公式，升级5X）
        decimal blockPerX = DynamicVars.EvaluateValueOrDefault("Block");
        await CreatureCmd.GainBlock(Owner.Creature, blockPerX * x, ValueProp.Move, cardPlay);
    }

    protected override void OnUpgrade()
    {
        // 格挡 4X → 5X
        DynamicVars["Block"].UpgradeValueBy(1m);
    }
}
