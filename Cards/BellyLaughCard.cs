using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;
using SilverWolf999.Powers;

namespace SilverWolf999.Cards;

// 注册卡牌到指定池（铁甲战士卡池）
[RegisterCard(typeof(NecrobinderCardPool))]
// 注册成铁甲战士的起始卡，后面是数量（2张）
[RegisterCharacterStarterCard(typeof(Necrobinder), 2)]
public class BellyLaughCard : ModCardTemplate
{
    // 基础耗能
    private const int energyCost = 1;
    // 卡牌类型
    private const CardType type = CardType.Attack;
    // 卡牌稀有度
    private const CardRarity rarity = CardRarity.Common;
    // 目标类型（AnyEnemy表示任意敌人）
    private const TargetType targetType = TargetType.AnyEnemy;
    // 是否在卡牌图鉴中显示
    private const bool shouldShowInCardLibrary = true;

    // 卡图资源（沿用现有的 TestCard.png；如需跟随类名，改回 {GetType().Name} 并重命名图片）
    public override CardAssetProfile AssetProfile => new(
        PortraitPath: "res://SilverWolf999/images/cards/TestCard.png"
    );

    // 卡牌旁出现的提示方框："欢愉"词条 + 好活当赏/增笑
    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromKeyword(MyKeywords.Elation),
        HoverTipFactory.FromPower<CertifiedBangerTwoPower>(),
        HoverTipFactory.FromPower<LaughBoostPower>(),
    ];

    // 伤害公式（与"笑点/Punchline"同一套，基础值 5）：见 PunchlineDamage.Resolve
    // 伤害 = (5 + 增笑) × (1 + 3x / (x + 24))，x = 好活当赏合计（剩余1回合 + 剩余2回合）
    // 卡牌基础数值：{Damage} 在卡面上实时显示公式结果
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.ComputedDamage("Damage", PunchlineDamage.Resolve, 6m, ValueProp.Move),
    ];

    public BellyLaughCard() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    // 打出时的效果逻辑
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        decimal damage = DynamicVars.EvaluateValueOrDefault("Damage", target: cardPlay.Target);
        await DamageCmd.Attack(damage)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target!)
            .Execute(choiceContext);
    }

    // 升级后的效果逻辑
    protected override void OnUpgrade()
    {
        DynamicVars["Damage"].UpgradeValueBy(2m);
    }
}
