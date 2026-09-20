using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using SilverWolf999.Patches;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace SilverWolf999.Powers;

/// <summary>
/// 琥珀（buff）：格挡不足时把这一击的伤害压到当前格挡值，使溢出伤害作废；
/// 本来就没有格挡时伤害照常。
///
/// 实现＝覆写 <see cref="ModifyDamageCap"/>（伤害上限钩子，位于 <c>Hook.ModifyDamage</c> 的
/// "加法 → 乘法 → 上限"末端，整段都跑在扣格挡之前）。
///
/// **意图显示屏蔽**：敌人头顶意图数字由 <c>AttackIntent.GetSingleDamage</c> 计算，
/// 它内部以"玩家自己为 target"裸调 <c>Hook.ModifyDamage</c>（`CardPreviewMode.None`），
/// 因此会连带吃到本能力的上限、把意图显示成被压低的数值。
/// 由 <see cref="AmberIntentDisplayPatch"/> 给那个方法打前缀，把上下文标成
/// <see cref="AmberPatchLog.InIntentDisplay"/>，本方法据此不介入 ⇒ 意图显示真实原伤害。
/// </summary>
[RegisterPower]
public class AmberPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    // 不挂 AdditionalHoverTips：来源卡牌 AmberCard 已预览本能力，
    // 再让本能力预览卡牌会形成"卡→能力→卡"的互相悬浮，故省略。

    /// <summary>
    /// 单次攻击的伤害上限：格挡不足时压到格挡值，让格挡刚好够吃下这一击。
    /// </summary>
    /// <remarks>
    /// 只在"不是意图显示 + 是自己 + 有格挡"时介入：
    ///   - 意图显示上下文（敌人头顶那个数字）→ 不介入，保证显示真实原伤害；
    ///   - 不是自己 → 不介入；
    ///   - 没有格挡 → 不介入（本来就没格挡时伤害照常；格挡被吃空后的后续段数同理）。
    ///
    /// 例1：4格挡挨8伤害 → 上限4 → 被格挡4、溢出0 → 不扣血。
    /// 例2：4格挡挨3伤害×3 → 第1次格挡够（不介入）、第2次上限1、第3次格挡为0不介入 → 合计扣3血。
    /// </remarks>
    public override decimal ModifyDamageCap(Creature? target, ValueProp props, Creature? dealer, CardModel? cardSource, CardPlay? cardPlay)
    {
        if (AmberPatchLog.InIntentDisplay || target is null || target != Owner)
        {
            return decimal.MaxValue;
        }
        int block = target.Block;
        if (block <= 0)
        {
            return decimal.MaxValue;
        }
        return block;
    }
}
