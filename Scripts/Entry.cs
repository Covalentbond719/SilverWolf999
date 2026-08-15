using Godot.Bridge;
using HarmonyLib;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;
using System.Reflection;
using STS2RitsuLib;
using STS2RitsuLib.Interop;

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
        // harmony可用，但是最好用ritsu的封装patch，见补丁系统一章
        // var harmony = new Harmony("com.example.testmod");
        // harmony.PatchAll();
        var assembly = Assembly.GetExecutingAssembly();
        RitsuLibFramework.EnsureGodotScriptsRegistered(assembly, Logger);
        // 自动注册内容
        ModTypeDiscoveryHub.RegisterModAssembly(ModId, assembly);
        // 使得tscn可以加载自定义脚本
        ScriptManagerBridge.LookupScriptsInAssembly(typeof(Entry).Assembly);
        Log.Info("Mod initialized!");
    }
}
