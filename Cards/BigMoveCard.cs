using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using SilverWolf999.Powers;

namespace SilverWolf999.Cards;

// 注册卡牌到铁甲战士卡池
[RegisterCard(typeof(NecrobinderCardPool))]
public class BigMoveCard : ModCardTemplate
{
    // 目标：升级后变为所有敌人
    public override TargetType TargetType => IsUpgraded ? TargetType.AllEnemies : TargetType.AnyEnemy;

    // 基础数值：虚弱/易伤层数（欢愉公式，基础2）
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Computed("Layers", PunchlineDamage.Resolve, 2m),
    ];

    // 悬浮提示："欢愉"词条 + 好活当赏/增笑 + 虚弱/易伤预览
    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromKeyword(MyKeywords.Elation),
        HoverTipFactory.FromPower<CertifiedBangerTwoPower>(),
        HoverTipFactory.FromPower<LaughBoostPower>(),
        HoverTipFactory.FromPower<WeakPower>(),
        HoverTipFactory.FromPower<VulnerablePower>(),
    ];

    public BigMoveCard() : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.AnyEnemy, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var combat = Owner.Creature.CombatState;
        if (combat == null)
        {
            return;
        }

        // 目标：未升级一名敌人，升级后所有敌人
        List<Creature> targets;
        if (IsUpgraded)
        {
            targets = combat.GetOpponentsOf(Owner.Creature).Where(c => c.IsAlive).ToList();
        }
        else
        {
            if (cardPlay.Target == null)
            {
                return;
            }
            targets = [cardPlay.Target];
        }

        // 欢愉：层数受公式控制（基础2）
        decimal layers = DynamicVars.EvaluateValueOrDefault("Layers");
        foreach (var target in targets)
        {
            await PowerCmd.Apply<WeakPower>(choiceContext, target, layers, Owner.Creature, this);
            await PowerCmd.Apply<VulnerablePower>(choiceContext, target, layers, Owner.Creature, this);
        }
    }
}
