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
- **卡牌动态伤害显示**：`ModCardVars.ComputedDamage("Damage", 静态ctx工厂, baseValue, ValueProp)`（ctx 工厂签名 `decimal F(ComputedDynamicVarContext ctx)`，可用 `ctx.SourceCreature?.GetPowerAmount<T>()`、`ctx.BaseValue`）；打出时用 `DynamicVars.EvaluateValueOrDefault("Damage", target: cardPlay.Target)` 读实时值（**计算型变量不能读 BaseValue**，那是存储基础值）；`OnUpgrade` 里 `DynamicVars["Damage"].UpgradeValueBy(4m)` 改基础值。ComputedDamage 的预览会走力量/易伤修正，与 `DamageCmd.Attack` 实际结算一致
- `Hook` 派发时 `IterateHookListeners()` 每次调用都会物化新快照 → 回合结算中挂上的新能力**不会**在同一轮钩子里级联触发（可放心"消失时直接挂下一阶段"）
- 伤害标记：`ValueProp.Unpowered` = 不吃力量等加成的普通伤害（可被格挡）；要无视格挡再或上 `ValueProp.Unblockable`

## RitsuLib 自动注册与 ID

- `Entry.cs` 已调 `ModTypeDiscoveryHub.RegisterModAssembly`，内容类只需加特性：`[RegisterPower]`、`[RegisterRelic(typeof(SharedRelicPool))]`、`[RegisterCard(typeof(ColorlessCardPool))]`
- ID 规则：`{MODID}_{类别}_{类名全称}`（snake_case 大写）。例：`TestCard` → `SILVER_WOLF999_CARD_TEST_CARD`；`PunchlinePower` → `SILVER_WOLF999_POWER_PUNCHLINE_POWER`
- **数字会粘在前一个单词上**（`SilverWolf999` → `SILVER_WOLF999`，不是 `SILVER_WOLF_999`）。为避免歧义，类名**别带数字**（本次用 `TripleNineCartridgeRelic` 而非 `Cartridge999Relic`）
- 本地化：`SilverWolf999/localization/zhs/` 下 `cards.json` / `powers.json` / `relics.json`；`{Amount}` 只对 `smartDescription` 生效（普通 description 不展开）
- 图标规格：能力小图 64×64、大图 256×256（`PowerAssetProfile(IconPath, BigIconPath)`）；遗物 85×85 小/轮廓 + 256×256 大（`RelicAssetProfile(IconPath, IconOutlinePath, BigIconPath)`）。缺图时游戏大图会回退 `missing_power.png`

## 本模组已实现内容（现状）

- `Powers/PunchlinePower.cs` 笑点（代码名 Punchline）：回合结束（仅拥有者阵营）对全体存活敌人造成伤害，随后移除自身并挂同层数"好活当赏-剩余2回合"。**伤害公式：伤害 = (2 + 增笑) × (1 + 3x/(x+24))，x = 笑点层数**（用户后续可能再改公式；基础值 2 与增笑是加算，`Math.Max(..., 0m)` 保底）。描述里的 `{Damage}` 用 `DamageVar` + `AfterPowerAmountChanged`（过滤 `power == this || power is LaughBoostPower`）刷新显示，取 `Math.Round(..., 2)`；实际伤害传未取整的 decimal
- `Powers/LaughBoostPower.cs` 增笑：类似机器人"集中"的纯数值能力（无钩子），`AllowNegative => true`，每层使笑点基础伤害 +1（被笑点用 `Owner.GetPowerAmount<LaughBoostPower>()` 读取）
- `Powers/CertifiedBangerTwoPower.cs` 好活当赏-剩余2回合（代码名 Certified Banger，2 回合）：纯计数器，回合结束转同层数"好活当赏-剩余1回合"后移除
- `Powers/CertifiedBangerOnePower.cs` 好活当赏-剩余1回合（代码名 Certified Banger，1 回合）：纯计数器，回合结束移除
- `Relics/TripleNineCartridgeRelic.cs` 999卡带：每打出1张牌（仅自己）+1 笑点；每场战斗开始 +10 好活当赏-剩余2回合；稀有度暂定 Rare
- `Cards/BellyLaughCard.cs` 捧腹（无色攻击牌）：**伤害 = (6 + 增笑) × (1 + 3x/(x+24))，x = 好活当赏合计（剩余1回合 + 剩余2回合层数相加）**，与笑点同一套公式、基础值 6。卡面文本"造成{Damage:diff()}点[gold]欢愉伤害[/gold]"，{Damage} 为 ComputedDamage 动态值；打出用 `DynamicVars.EvaluateValueOrDefault("Damage", ...)` 读公式值再走 `DamageCmd.Attack`；升级基础值 +4。**注意：类名已从 TestCard 改为 BellyLaughCard，卡图路径硬编码指向现有 TestCard.png，若重命名图片需同步改 AssetProfile**
- `Cards/MyKeywords.cs` **自定义关键词"欢愉伤害"**（词条整体是"欢愉伤害"，代码名 Elation damage）：名词解释是**全局**的（绑定在 CardKeyword 上）：`[RegisterOwnedCardKeyword(nameof(ElationDamage))]` 注册 + `ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(ElationDamage)).GetModCardKeyword()` 取 CardKeyword + `card_keywords.json` 写 `{MODID}_KEYWORD_{id大写}.title/.description`（键 `SILVER_WOLF999_KEYWORD_ELATION_DAMAGE`）；卡牌侧用 `AdditionalHoverTips => [HoverTipFactory.FromKeyword(MyKeywords.ElationDamage)]` 挂提示框，描述里 `[gold]欢愉伤害[/gold]` 染色（教程：RitsuLib 01-04 添加卡牌属性）
- `Powers/HiddenMmrPower.cs` 隐藏分（Hidden MMR）：**累计到60时（仅一次，60的倍数不重复触发）塞一张"无敌玩家"到手牌**；**每30隐藏分给1力量（无门槛，从0开始）**（用 `AfterPowerAmountChanged` 过滤 `power == this`，内部 Data 记录已触发/已给力量，按 `Amount/30` 增量同步 `StrengthPower`）
- `Powers/MirthPower.cs` + `Cards/MirthCard.cs` 如是众生欢笑不已（金卡/无色/能力1费，英文名 Mirth）：打出给1层 MirthPower，**被动：每获得1个笑点→获得1隐藏分**（`AfterPowerAmountChanged` 过滤 `power is PunchlinePower && power.Owner==Owner && amount>0`，按增量 1:1 给 HiddenMmr）；**升级：固有**（`OnUpgrade` 里 `AddKeyword(CardKeyword.Innate)`，参考原版 Afterimage）
- `Powers/GodModePower.cs` 无敌玩家 buff：2层，**下2个回合开始时各塞一张"狼尊时刻"**（`AfterPlayerTurnStart` + `CreateCard`+`AddGeneratedCardToCombat`）；**回合结束自然流失1层**（`AfterSideTurnEnd` + `PowerCmd.TickDownDuration`，`AfterApplied` 里 `SkipNextDurationTick=true` 让当回合结束不流失）；升级标志 `SetAddUpgraded` 控制塞的是否升级版
- `Cards/GodModeCard.cs` 无敌玩家，启动！（无色稀有能力0费）：打出立即塞1张隐藏关：狼尊时刻（升级则 `CardCmd.Upgrade`）+ 给2层 GodModePower；**描述用 `{IfUpgraded:show:隐藏关：狼尊时刻+|隐藏关：狼尊时刻}` 显示升级版名字**；悬浮提示 `HoverTipFactory.FromCard<WolfMomentCard>(IsUpgraded)` 预览所加的牌
- **悬浮提示写法**：卡牌用 `AdditionalHoverTips`（`HoverTipFactory.FromCard<T>(bool upgrade)` 卡牌预览 / `HoverTipFactory.FromPower<T>()` 能力提示）；**能力/遗物用 `AdditionalHoverTips`（RitsuLib 的 `ExtraHoverTips` 是密封的，不能重写）**；GodModePower 的提示按内部 Data 动态返回升级版预览
- `Cards/WolfMomentCard.cs` 隐藏关：狼尊时刻（无色稀有攻击X费）：`HasEnergyCostX=>true` + `ResolveEnergyXValue()`，**先对全体敌人造成7欢愉伤害（未升级1次/升级后2次）**（`WithHitCount().TargetingAllOpponents`），**再对随机敌人造成7欢愉伤害（未升级X次/升级后X+1次）**（`TargetingRandomOpponents`）；升级不改伤害只改次数；描述用 `{IfUpgraded:show:升级文本|未升级文本}` 区分升级前后（升级文本在前）；消耗
- `Cards/PunchlineDamage.cs` 共享欢愉伤害公式工厂 `PunchlineDamage.Resolve(ctx)`：`(BaseValue+增笑)×(1+3x/(x+24))`，捧腹/狼尊时刻通过 ComputedDamage 传不同基础值（6/4）
- **塞牌 API（当前版）**：`combatState.CreateCard<T>(Player)` 创建 + `CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, Player creator)` 入手牌；升级实例用 `CardCmd.Upgrade(card)`（同步方法）；X费参考原版 Whirlwind/SwordBoomerang/StormOfSteel
- `Cards/SmileCard.cs` 莞尔（1费无色技能，稀有度暂定 Uncommon）：**[gold]欢愉[/gold]：获得公式格挡（基础5，`ComputedBlock` + 共享的 `PunchlineDamage.Resolve`，受增笑/好活当赏影响）+ 5笑点/4隐藏分/3好活当赏**，升级全 +2（7/6/5）；数值用动态变量（`{Block:diff()}` 等）；好活当赏给"剩余2回合"版
- **词条「欢愉」**（`MyKeywords.Joy`，键 `SILVER_WOLF999_KEYWORD_JOY`）：解释"好活当赏和增笑还会影响这种情况下的格挡和状态效果"——与"欢愉伤害"（ElationDamage）是两个不同的词条；莞尔悬浮提示挂 `HoverTipFactory.FromKeyword(MyKeywords.Joy)`
- `Cards/JokeCard.cs` 讲个段子（0费无色技能常见）：**本回合内获得1点增笑**（挂 `TemporaryLaughBoostPower` 临时包装）+ 消耗；**升级去除消耗**（`OnUpgrade` 里 `RemoveKeyword(CardKeyword.Exhaust)` + `CanonicalKeywords` 按 `IsUpgraded` 条件化，双保险）
- `Powers/TemporaryLaughBoostPower.cs` 临时增笑：**继承 RitsuLib `ModTemporaryAppliedPowerTemplate<JokeCard, LaughBoostPower>`**——模板内部维护两个状态（包装能力 + 真实能力镜像），回合结束自动撤销；Title 自动取来源卡牌名，无需本地化（教程：RitsuLib 01-05 添加新能力"临时能力"一节；原版参考 Hotfix/HotfixPower/TemporaryFocusPower）
- `Cards/CrosstalkCard.cs` 听段相声（1费无色能力稀有）：获得{Boost}点增笑（升级1→2）
- 控制台测试：`power SILVER_WOLF999_POWER_LAUGH_BOOST_POWER 5 0`、`power SILVER_WOLF999_POWER_PUNCHLINE_POWER 10 0`、`power SILVER_WOLF999_POWER_CERTIFIED_BANGER_TWO_POWER 10 0`、`power SILVER_WOLF999_POWER_HIDDEN_MMR_POWER 60 0`、`power SILVER_WOLF999_POWER_GOD_MODE_POWER 2 0`、`power SILVER_WOLF999_POWER_MIRTH_POWER 1 0`、`card SILVER_WOLF999_CARD_BELLY_LAUGH_CARD`、`card SILVER_WOLF999_CARD_GOD_MODE_CARD`、`card SILVER_WOLF999_CARD_WOLF_MOMENT_CARD`、`card SILVER_WOLF999_CARD_SMILE_CARD`、`card SILVER_WOLF999_CARD_MIRTH_CARD`、`card SILVER_WOLF999_CARD_JOKE_CARD`、`card SILVER_WOLF999_CARD_CROSSTALK_CARD`、`relic SILVER_WOLF999_RELIC_TRIPLE_NINE_CARTRIDGE_RELIC`
- **待办：遗物图标**。`SilverWolf999/images/relics/` 下目前是占位文本文件，需用户用同名 PNG 覆盖：`triple_nine_cartridge.png`（85×85）、`triple_nine_cartridge_big.png`（256×256）。能力图标已由用户侧 Godot 导入生成 `.import`，无需处理

## 沙箱/构建注意事项

- 本环境只允许写工作区；构建时的"Copy Mod"目标会因无法写游戏 mods 目录而报错（**编译本身成功**，非代码问题）。需要干净编译时，临时给两个部署 Target 加 `Condition="'$(DeployMods)' == 'true'"`，不带该属性构建即可跳过拷贝
- `dotnet new` 被沙箱拒绝（模板缓存）；`dotnet build` 可用（还原走已缓存的 NuGet 包，离线 OK）
- NU1900 警告（连不上华为云 NuGet 镜像）无害，忽略
- write 工具对"已删除的文件"有观察缓存：用 pwsh 删掉文件/目录后再写，会报 "file no longer exists"，先 read 一下该路径刷新即可
