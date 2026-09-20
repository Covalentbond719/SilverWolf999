using System.Linq;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using SilverWolf999.Powers;
using SilverWolf999.Scripts;

namespace SilverWolf999.Patches;

/// <summary>
/// 琥珀（AmberPower）诊断日志器。复用 <see cref="Entry.Logger"/>。
/// 只打印前 <see cref="MaxReports"/> 条，避免刷屏。
/// </summary>
internal static class AmberPatchLog
{
    private const int MaxReports = 40;

    /// <summary>
    /// 是否正在计算"敌人头顶意图上显示的伤害"。
    /// 由 <see cref="AmberIntentDisplayPatch"/> 打开，覆盖 <c>AttackIntent.GetSingleDamage</c> 全过程。
    /// 该上下文里琥珀不介入，保证意图显示的是真实原伤害。
    /// </summary>
    public static bool InIntentDisplay;

    private static int _reports;

    public static void Report(string message, bool force = false)
    {
        if (!force && _reports >= MaxReports)
        {
            return;
        }
        _reports++;
        Entry.Logger.Info("[琥珀] " + message);
    }

    public static void ReportAlways(string message) => Report(message, force: true);
}

/// <summary>
/// **精确屏蔽敌人意图显示**：<c>AttackIntent.GetSingleDamage(targets, owner)</c> 是敌人头顶
/// 意图数字的唯一计算入口，它的实现是：
/// <code>
/// decimal num = DamageCalc();
/// Player me = LocalContext.GetMe(owner.CombatState);
/// if (me != null)
///     num = Hook.ModifyDamage(me.RunState, ..., me.Creature, owner, DamageCalc(),
///                             ValueProp.Move, null, null,
///                             ModifyDamageHookType.All, CardPreviewMode.None, out _);
/// return Math.Max(0, (int)num);
/// </code>
/// 注意它以**玩家自己为 target**、并用 <c>CardPreviewMode.None</c> 调 <c>ModifyDamage</c> ——
/// 所以它和"真正结算"的调用在参数上完全一致，靠 <c>ModifyDamage</c> 自身参数无法区分（实测确认）。
/// <c>SingleAttackIntent.GetTotalDamage</c> 与 <c>MultiAttackIntent.GetTotalDamage</c>
/// 也都转调 <c>GetSingleDamage</c>，因此补这一个方法即可覆盖全部攻击意图。
/// </summary>
[HarmonyPatch(typeof(AttackIntent), nameof(AttackIntent.GetSingleDamage))]
internal static class AmberIntentDisplayPatch
{
    private static void Prefix()
    {
        AmberPatchLog.InIntentDisplay = true;
    }

    private static void Postfix()
    {
        AmberPatchLog.InIntentDisplay = false;
    }
}

/// <summary>
/// 诊断用：记录每次 <c>Hook.ModifyDamage</c>（仅限"目标身上有琥珀"的调用）的
/// 入参、返回、以及当次是否处于意图显示上下文。
/// </summary>
[HarmonyPatch(typeof(Hook), nameof(Hook.ModifyDamage))]
internal static class AmberModifyDamageProbePatch
{
    private static void Postfix(
        CardPreviewMode previewMode,
        Creature? target,
        decimal damage,
        ref decimal __result)
    {
        if (target == null || target.GetPowerAmount<AmberPower>() <= 0)
        {
            return;
        }
        AmberPatchLog.Report(
            $"ModifyDamage mode={previewMode} intentDisplay={AmberPatchLog.InIntentDisplay} "
            + $"target={target} block={target.Block} in={damage} out={__result}");
    }
}
