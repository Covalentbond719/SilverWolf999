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

// 注册卡牌到铁甲战士卡池
[RegisterCard(typeof(NecrobinderCardPool))]
public class MirthCard : ModCardTemplate
{
    // 基础数值：每次奖励 10隐藏分 + 3格挡（升级后 12 / 4）
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("Mmr", 10),
        ModCardVars.Int("Block", 3),
    ];

    // 卡牌旁出现的提示方框：预览授予的buff + 涉及的能力
    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<MirthPower>(),
        HoverTipFactory.FromPower<PunchlinePower>(),
        HoverTipFactory.FromPower<HiddenMmrPower>(),
    ];

    public MirthCard() : base(1, CardType.Power, CardRarity.Rare, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 挂上"每获得4个笑点→10隐藏分+3格挡"的被动（升级后 12/4）
        var buff = await PowerCmd.Apply<MirthPower>(choiceContext, Owner.Creature, 1, Owner.Creature, null);
        if (buff != null)
        {
            buff.SetRewards(DynamicVars["Mmr"].IntValue, DynamicVars["Block"].IntValue);
        }
    }

    // 升级：奖励提升（10→12 隐藏分，3→4 格挡）
    protected override void OnUpgrade()
    {
        DynamicVars["Mmr"].UpgradeValueBy(2m);
        DynamicVars["Block"].UpgradeValueBy(1m);
    }
}
