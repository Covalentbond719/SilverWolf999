# Agent 备忘（SilverWolf999 模组项目）

> 给以后的我：本文件是解决本模组几个需求（自定能力、遗物）时踩坑经验的速查。
> 项目性质：**杀戮尖塔2（Slay the Spire 2）C# 模组**，Godot 4.5.1 + RitsuLib 0.5.12。

## 关键路径

- 模组工程：本目录（`SilverWolf999.csproj` 构建后自动拷贝 dll/json 到游戏 mods 目录）
- 游戏：`E:\SteamLibrary\steamapps\common\Slay the Spire 2`（当前 sts2.dll 为 2026-08 测试版，**.NET 9**）
- 教程：`E:\Downloads\SlayTheSpire2ModdingTutorials-master\SlayTheSpire2ModdingTutorials-master\`
- 反编译旧源码：`E:\Downloads\sts2original`（2026-04 版，**API 已过时！**）
- RitsuLib XML 文档：`C:\Users\27897\.nuget\packages\sts2.ritsulib\0.5.12\lib\net9.0\STS2-RitsuLib.xml`（6MB，可 grep 成员签名）
- ILSpy 可用：`E:\Downloads\ILSpy_binaries_10.0.0.8282-preview2-x64`（本次未用，反射探针更省事）

## 最重要的一条教训

**教程和 `sts2original` 反编译源码是旧版 API，当前游戏（测试版）API 已改**。
例如：`PowerCmd.Apply` 现在第一个参数是 `PlayerChoiceContext`；回合结束钩子从 `AfterTurnEnd(choiceContext, side)` 改名为 `AfterSideTurnEnd(choiceContext, side, participants)`。
写代码前**务必先反射当前 sts2.dll 验证签名**，不要照抄旧源码/教程。

## 反射当前 API 的方法（已验证可行）

PowerShell 5.1 是 .NET Framework，加载不了 net9.0 程序集。方案：

1. 在工程里建临时探针 `D:\Documents\SilverWolf999\.probe\`（手写 `probe.csproj` 和 `Program.cs`，**不要用 `dotnet new`**——模板缓存目录被沙箱拒绝）
2. 临时在 `SilverWolf999.csproj` 里加 `<Compile Remove=".probe\**" />`（否则探针的 .cs 会被模组项目编进去）
3. `dotnet build` 探针（net10.0，无 PackageReference，离线可还原）→ `dotnet <dll>` 运行
4. 用 `AssemblyLoadContext` + `Resolving` 事件从游戏 data 目录补依赖加载 `sts2.dll`
5. 完事删除 `.probe\`，并**还原 csproj 的临时改动**

注意：`Assembly.GetType()` 在依赖缺失时会静默返回 null；沙箱拒绝写 `C:\Users\27897\AppData\Local\Temp`（工作区外），写文件只能在工作区内。

## 已验证的当前版本 API（2026-08 版）

- `PowerCmd.Apply<T>(PlayerChoiceContext choiceContext, Creature target, decimal amount, Creature? applier, CardModel? cardSource, bool silent = false)`（choiceContext 在前）
- `PowerCmd.Apply<T>(choiceContext, IEnumerable<Creature> targets, ...)`（多目标）；`PowerCmd.Remove(PowerModel)`；`PowerCmd.Decrement(PowerModel)`
- `CreatureCmd.Damage(choiceContext, IEnumerable<Creature> targets, decimal amount, ValueProp props, Creature dealer, ...)` —— 群伤用多目标重载
- 回合钩子（AbstractModel 虚方法）：`AfterSideTurnEnd(choiceContext, CombatSide, IEnumerable<Creature>)`、`BeforeSideTurnEnd...`、`AfterSideTurnStart(...)`、`AfterPlayerTurnStart(choiceContext, Player)`、`BeforeCombatStart()`（无参）、`AfterCardPlayed(choiceContext, CardPlay)`
- **`AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)` —— 现在第一个参数也是 choiceContext**（监听所有能力层数变化，用 `power == this` 或类型判断过滤）
- 能力钩子守卫写法：`if (side != Owner.Side) return;`（能力 `Owner` 是 **Creature**）
- 遗物钩子：`Owner` 是 **Player**（拿生物用 `Owner.Creature`）；打牌者用 `cardPlay.Player`；`BeforeCombatStart` 没有 choiceContext，用 `new ThrowingPlayerChoiceContext()`
- `Creature.GetPowerAmount<T>()` 读层数（无则 0）；`ICombatState.HittableEnemies` 可取可命中敌人
- `PowerModel.AllowNegative` 可覆写（集中/增笑这类可被扣到负的用 `true`）；`PowerModel.CanonicalVars` 是 `protected virtual`（powers 也能用 DynamicVar 显示数值）
- **能力描述显示动态数值**：`CanonicalVars => [new DamageVar(2m, ValueProp.Unpowered)]`（默认名 "Damage"），在钩子里 `DynamicVars.Damage.BaseValue = x` 刷新，smartDescription 用 `{Damage}` 引用。RitsuLib 的 `ComputedDynamicVar`/`ModCardVars` 是**卡牌专用**（factory 参数是 CardModel），能力不能用
- `Hook` 派发时 `IterateHookListeners()` 每次调用都会物化新快照 → 回合结算中挂上的新能力**不会**在同一轮钩子里级联触发（可放心"消失时直接挂下一阶段"）
- 伤害标记：`ValueProp.Unpowered` = 不吃力量等加成的普通伤害（可被格挡）；要无视格挡再或上 `ValueProp.Unblockable`

## RitsuLib 自动注册与 ID

- `Entry.cs` 已调 `ModTypeDiscoveryHub.RegisterModAssembly`，内容类只需加特性：`[RegisterPower]`、`[RegisterRelic(typeof(SharedRelicPool))]`、`[RegisterCard(typeof(ColorlessCardPool))]`
- ID 规则：`{MODID}_{类别}_{类名全称}`（snake_case 大写）。例：`TestCard` → `SILVER_WOLF999_CARD_TEST_CARD`；`LaughPointPower` → `SILVER_WOLF999_POWER_LAUGH_POINT_POWER`
- **数字会粘在前一个单词上**（`SilverWolf999` → `SILVER_WOLF999`，不是 `SILVER_WOLF_999`）。为避免歧义，类名**别带数字**（本次用 `TripleNineCartridgeRelic` 而非 `Cartridge999Relic`）
- 本地化：`SilverWolf999/localization/zhs/` 下 `cards.json` / `powers.json` / `relics.json`；`{Amount}` 只对 `smartDescription` 生效（普通 description 不展开）
- 图标规格：能力小图 64×64、大图 256×256（`PowerAssetProfile(IconPath, BigIconPath)`）；遗物 85×85 小/轮廓 + 256×256 大（`RelicAssetProfile(IconPath, IconOutlinePath, BigIconPath)`）。缺图时游戏大图会回退 `missing_power.png`

## 本模组已实现内容（现状）

- `Powers/LaughPointPower.cs` 笑点：回合结束（仅拥有者阵营）对全体存活敌人造成伤害，随后移除自身并挂同层数"好活当赏-剩余2回合"。**伤害公式：伤害 = (2 + 增笑) × (1 + 3x/(x+24))，x = 笑点层数**（用户后续可能再改公式；基础值 2 与增笑是加算，`Math.Max(..., 0m)` 保底）。描述里的 `{Damage}` 用 `DamageVar` + `AfterPowerAmountChanged`（过滤 `power == this || power is LaughBoostPower`）刷新显示，取 `Math.Round(..., 2)`；实际伤害传未取整的 decimal
- `Powers/LaughBoostPower.cs` 增笑：类似机器人"集中"的纯数值能力（无钩子），`AllowNegative => true`，每层使笑点基础伤害 +1（被笑点用 `Owner.GetPowerAmount<LaughBoostPower>()` 读取）
- `Powers/AppreciationTwoPower.cs` 好活当赏-剩余2回合：纯计数器，回合结束转同层数"好活当赏-剩余1回合"后移除
- `Powers/AppreciationOnePower.cs` 好活当赏-剩余1回合：纯计数器，回合结束移除
- `Relics/TripleNineCartridgeRelic.cs` 999卡带：每打出1张牌（仅自己）+1 笑点；每场战斗开始 +10 好活当赏-剩余2回合；稀有度暂定 Rare
- 控制台测试：`power SILVER_WOLF999_POWER_LAUGH_POINT_POWER 5 0`、`relic SILVER_WOLF999_RELIC_TRIPLE_NINE_CARTRIDGE_RELIC`
- **待办：遗物图标**。`SilverWolf999/images/relics/` 下目前是占位文本文件，需用户用同名 PNG 覆盖：`triple_nine_cartridge.png`（85×85）、`triple_nine_cartridge_big.png`（256×256）。能力图标已由用户侧 Godot 导入生成 `.import`，无需处理

## 沙箱/构建注意事项

- 本环境只允许写工作区；构建时的"Copy Mod"目标会因无法写游戏 mods 目录而报错（**编译本身成功**，非代码问题）。需要干净编译时，临时给两个部署 Target 加 `Condition="'$(DeployMods)' == 'true'"`，不带该属性构建即可跳过拷贝
- `dotnet new` 被沙箱拒绝（模板缓存）；`dotnet build` 可用（还原走已缓存的 NuGet 包，离线 OK）
- NU1900 警告（连不上华为云 NuGet 镜像）无害，忽略
- write 工具对"已删除的文件"有观察缓存：用 pwsh 删掉文件/目录后再写，会报 "file no longer exists"，先 read 一下该路径刷新即可
