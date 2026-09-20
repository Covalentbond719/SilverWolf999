using Godot.Bridge;
using HarmonyLib;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;
using System.Reflection;
using STS2RitsuLib;
using STS2RitsuLib.Interop;
using SilverWolf999.Patches;

namespace SilverWolf999.Scripts;

// 必须要加的属性，用于注册Mod。字符串和初始化函数命名一致。
[ModInitializer(nameof(Init))]
public class Entry
{
    public const string ModId = "SilverWolf999";
    public static readonly Logger Logger = RitsuLibFramework.CreateLogger(ModId);

    // 初始化函数
    public static void Init()
    {
        ApplyHarmonyPatches();

        var assembly = Assembly.GetExecutingAssembly();
        RitsuLibFramework.EnsureGodotScriptsRegistered(assembly, Logger);
        // 自动注册内容
        ModTypeDiscoveryHub.RegisterModAssembly(ModId, assembly);
        // 使得tscn可以加载自定义脚本
        ScriptManagerBridge.LookupScriptsInAssembly(typeof(Entry).Assembly);
        Log.Info("Mod initialized!");
    }

    /// <summary>
    /// 应用 Harmony 补丁。
    /// 两个补丁：<see cref="AmberIntentDisplayPatch"/>（屏蔽敌人意图显示，让意图数字保持真实原伤害）
    /// 与 <see cref="AmberModifyDamageProbePatch"/>（诊断日志）。
    /// 补丁失败不影响琥珀的伤害压制效果（<c>ModifyDamageCap</c> 走原生钩子），
    /// 只会让意图数字显示成被压低的数值，因此整体 try/catch。
    /// </summary>
    private static void ApplyHarmonyPatches()
    {
        var assembly = Assembly.GetExecutingAssembly();
        try
        {
            new Harmony(ModId).PatchAll(assembly);

            var target = AccessTools.Method(
                typeof(MegaCrit.Sts2.Core.MonsterMoves.Intents.AttackIntent),
                nameof(MegaCrit.Sts2.Core.MonsterMoves.Intents.AttackIntent.GetSingleDamage));
            var info = target == null ? null : Harmony.GetPatchInfo(target);
            bool ours = info?.Prefixes != null
                && info.Prefixes.Any(p => p.PatchMethod.DeclaringType == typeof(AmberIntentDisplayPatch));

            AmberPatchLog.Report(
                ours
                    ? "Harmony 补丁已应用：AttackIntent.GetSingleDamage 意图显示屏蔽就位（琥珀只在实际结算时压值）"
                    : $"谨慎：意图显示屏蔽未生效！target={target?.ToString() ?? "<未找到>"} prefixes={info?.Prefixes?.Count ?? 0}（琥珀效果仍在，但敌人意图可能显示被压低的数值）",
                force: true);
        }
        catch (System.Exception ex)
        {
            AmberPatchLog.Report($"Harmony 补丁异常，意图显示屏蔽失效（琥珀效果仍在，但意图可能显示被压低的数值）：{ex}", force: true);
            Logger.Error(ex.ToString());
        }
    }
}
