# Agent 备忘

> 项目性质：**杀戮尖塔2（Slay the Spire 2）C# 模组**，Godot 4.5.1 + RitsuLib 0.5.12。
> 说明：本文件内容应与当前工程代码/本地化保持一致，改实装前先核对此部分，改实装后要修改此文件。

## 关键路径

- 模组工程：本目录（`SilverWolf999.csproj` 构建后自动拷贝 dll/json 到游戏 mods 目录）
- 游戏：`E:\SteamLibrary\steamapps\common\Slay the Spire 2`（当前 sts2.dll 为 2026-08 测试版，**.NET 9**）
- 教程：`E:\Downloads\SlayTheSpire2ModdingTutorials-master\SlayTheSpire2ModdingTutorials-master\`
- 反编译旧源码：`E:\Downloads\sts2original`（2026-04 版，**API 已过时！**）
- RitsuLib XML 文档：`C:\Users\27897\.nuget\packages\sts2.ritsulib\0.5.12\lib\net9.0\STS2-RitsuLib.xml`（6MB，可 grep 成员签名）
- ILSpy 反编译库（可写探针直接读当前 DLL 的 C#）：`E:\Downloads\ILSpy_binaries_10.0.0.8282-preview2-x64\ICSharpCode.Decompiler.dll` + Mono.Cecil.dll

## 最重要的一条教训

**教程和 `sts2original` 反编译源码是旧版 API，当前游戏（测试版）API 已改**。
例如：`PowerCmd.Apply` 现在第一个参数是 `PlayerChoiceContext`；回合结束钩子从 `AfterTurnEnd(choiceContext, side)` 改名为 `AfterSideTurnEnd(choiceContext, side, participants)`。
写代码前**务必先反射/反编译当前 sts2.dll 验证签名**，不要照抄旧源码/教程。

## 反射当前 API 的方法（已验证可行）

PowerShell 5.1 是 .NET Framework，加载不了 net9.0 程序集。两种方案：

1. **反射探针**：在工程里建临时 `D:\Documents\SilverWolf999\.probe\`（手写 `probe.csproj` + `Program.cs`，**不要用 `dotnet new`**——模板缓存目录被沙箱拒绝）；临时在 `SilverWolf999.csproj` 加 `<Compile Remove=".probe\**" />`（否则探针 .cs 会被模组项目编进去）；`dotnet build` 探针（net10.0，无 PackageReference，离线可还原）→ `dotnet <dll>` 运行；用 `AssemblyLoadContext` + `Resolving` 事件从游戏 data 目录补依赖加载 `sts2.dll`。完事删除 `.probe\` 并还原 csproj。
2. **反编译探针**：引用 `ICSharpCode.Decompiler.dll`，`new CSharpDecompiler(sts2.dll, new DecompilerSettings())` + `decompiler.TypeSystem.MainModule.TypeDefinitions` 找类型，`DecompileAsString(token)` 输出成员 C#（不加载程序集，直接读 dll，最可靠）。用于看方法体（如 `Hook.ModifyDamage` 管线、`AfterEnergySpent` 签名）。

注意：`Assembly.GetType()` 在依赖缺失时会静默返回 null；沙箱拒绝写工作区外的 temp，写文件只能在工作区内。

## 已验证的当前版本 API（2026-08 版）

- `PowerCmd.Apply<T>(PlayerChoiceContext choiceContext, Creature target, decimal amount, Creature? applier, CardModel? cardSource, bool silent = false)`（choiceContext 在前）；`PowerCmd.Remove(PowerModel)`；`PowerCmd.Decrement`；`PowerCmd.TickDownDuration`
- `CreatureCmd.Damage(choiceContext, IEnumerable<Creature>, decimal, ValueProp, Creature dealer, ...)` 群伤；`CreatureCmd.GainBlock(Creature, decimal, ValueProp, CardPlay?, bool fast)`
- 回合钩子（AbstractModel 虚方法）：`AfterSideTurnEnd(choiceContext, CombatSide, IEnumerable<Creature>)`、`AfterSideTurnStart(...)`、`AfterPlayerTurnStart(choiceContext, Player)`、`BeforeCombatStart()`（无参）、`AfterCardPlayed(choiceContext, CardPlay)`、**`AfterEnergySpent(CardModel card, int amount)`（无 choiceContext，用 `new ThrowingPlayerChoiceContext()`）**
- `AfterPowerAmountChanged(PlayerChoiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)`（第一个参数也是 choiceContext；监听所有能力变化，用 `power == this` 或类型判断过滤）
- 能力钩子守卫：`if (side != Owner.Side) return;`（能力 `Owner` 是 **Creature**）；遗物/卡牌 `Owner` 是 **Player**（用 `Owner.Creature` 拿生物）
- `Creature.GetPowerAmount<T>()` 读层数（无则 0）；`ICombatState.HittableEnemies` / `GetOpponentsOf(Owner)` 取敌人
- `PowerModel.AllowNegative` 可覆写；`PowerModel.CanonicalVars` 是 `protected virtual`
- **能力描述动态数值**：`CanonicalVars => [new DamageVar(x, ValueProp.Unpowered)]`，钩子里 `DynamicVars.Damage.BaseValue = ...` 刷新，smartDescription 用 `{Damage}`。RitsuLib `ComputedDynamicVar`/`ModCardVars` 是**卡牌专用**（factory 参数是 CardModel），能力不能用
- **卡牌动态数值**：`ModCardVars.ComputedDamage/ComputedBlock/Computed/Int("名", 工厂或基础值, ...)`；打出时用 `DynamicVars.EvaluateValueOrDefault("名", target: ...)` 读实时值（**计算型变量别读 BaseValue**，那是存储基础值）；`OnUpgrade` 里 `DynamicVars["名"].UpgradeValueBy(n)` 改基础值
- `DamageCmd.Attack(decimal).WithHitCount(n).FromCard(this,cardPlay).TargetingAllOpponents/TargetingRandomOpponents(combat).Execute(choiceContext)`；X费：`HasEnergyCostX=>true`（覆盖）+ cost 0 + `ResolveEnergyXValue()`
- `Hook` 派发每次 `IterateHookListeners()` 物化新快照 → 回合结算中挂新能力不会同轮级联
- 伤害标记：`ValueProp.Unpowered` = 不吃力量/虚弱/易伤（三者都要求 powered）；`ValueProp.Move` = 标准吃修正
- **塞牌**：`combatState.CreateCard<T>(Player)` + `CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, creator)`；升级实例 `CardCmd.Upgrade(card)`
- **选牌**：`CardSelectorPrefs(new LocString("cards", Id.Entry+".selectionScreenPrompt"), 数量)` + `CardSelectCmd.FromSimpleGrid(choiceContext, 牌堆.Cards, Owner, prefs)` + `CardPileCmd.Add(card, PileType.Hand)`；**选牌卡必须在 cards.json 配 `selectionScreenPrompt`，否则抛异常**
- **能量/抽牌**：`PlayerCmd.GainEnergy(decimal, Player)`；`CardPileCmd.Draw(choiceContext, decimal, Player)`（支持小数）

## RitsuLib 自动注册与 ID

- `Entry.cs` 已调 `ModTypeDiscoveryHub.RegisterModAssembly`，内容类只需加特性：`[RegisterPower]`、`[RegisterRelic(typeof(SharedRelicPool))]`、`[RegisterCard(typeof(NecrobinderCardPool))]`
- **本模组所有角色卡都在亡灵契约师（Necrobinder）卡池**（`MegaCrit.Sts2.Core.Models.CardPools.NecrobinderCardPool`）；**捧腹开怀是 Necrobinder 起始卡×2**，**999卡带是 Necrobinder 起始遗物**；无敌玩家/隐藏关狼尊时刻是 **Token 池**（无角色，`CardRarity.Token`，不进货架/图鉴）
- ID 规则：`{MODID}_{类别}_{类名全称}`（snake_case 大写）。例：`BellyLaughCard` → `SILVER_WOLF999_CARD_BELLY_LAUGH_CARD`；`PunchlinePower` → `SILVER_WOLF999_POWER_PUNCHLINE_POWER`
- **数字会粘在前一个单词上**（`SilverWolf999` → `SILVER_WOLF999`）。为避免歧义，类名**别带数字**
- 本地化：`SilverWolf999/localization/zhs/` 下 `cards.json` / `powers.json` / `relics.json` / `card_keywords.json`；`{Amount}` 只对 `smartDescription` 生效（description 不展开）
- 图标规格：能力小图 64×64、大图 256×256（`PowerAssetProfile(IconPath, BigIconPath)`）；遗物 85×85 小/轮廓 + 256×256 大（`RelicAssetProfile(IconPath, IconOutlinePath, BigIconPath)`）；缺图回退 `missing_power.png`

## 本模组已实现内容（与当前代码/本地化一致）

> 卡牌显示名 = 本地化 title；类名 = 代码名。角色卡全在 Necrobinder 池。

### 核心公式 `Cards/PunchlineDamage.cs`

- `Resolve(ctx)`：**伤害/数值 = (基础值 + 增笑) × (1 + 3x/(x+24))**，x = 好活当赏合计（剩余1回合 + 剩余2回合层数之和）；`Math.Max(基础+增笑, 0)` 保底。各卡经 Computed 变量传不同基础值。
- `ResolveWhenUpgraded(ctx)`：**升级后才走公式，未升级返回基础值**（用于"升级才带欢愉"）。

### 能力 `Powers/`

- `PunchlinePower` 笑点：回合结束发动"阿哈时刻"，对全体存活敌人造成公式伤害（**基础 4**，Unpowered 不吃力量），随后移除自身并把同层数转为"好活当赏-剩余2回合"；描述 `{Damage}` 用 DamageVar + `AfterPowerAmountChanged`（过滤 `power==this || power is LaughBoostPower`）刷新
- `LaughBoostPower` 增笑：纯数值（AllowNegative），每层使欢愉公式基础值+1（被 `Resolve` 读取）
- `CertifiedBangerTwoPower`/`OnePower` 好活当赏-剩余1/2回合：纯计数器；Two 回合结束转同层 One 后移除，One 回合结束移除
- `HiddenMmrPower` 隐藏分：累计到 60 时（仅一次）塞一张"无敌玩家，启动！"；**每 30 给 1 力量（无门槛，`Amount/30` 增量同步 StrengthPower）**；悬浮预览无敌玩家
- `MirthPower` 如是众生欢笑不已：**每获得 5 个笑点 → 10 隐藏分 + 3 格挡**（升级 12/4，卡牌 `SetRewards` 传入；累计余数跨回合；格挡 Unpowered 平值）
- `DivineLaughterPower` 神说要有笑声：每消耗 1 能量 → 获得"本能力层数 Amount"个笑点（`AfterEnergySpent`，**多次打出能力卡叠加层数后每能量给更多笑点**；smartDescription 用 `{Amount}`）
- `DrawOnPunchlinePower` 我的回合，抽卡！：**每获得 12（升级 10）个笑点抽 1 张**，跨回合累计；阈值经 `SetStep` + DynamicVar "Step"
- `GodModePower` 无敌玩家：2 层，下 2 个回合开始各塞一张隐藏关：狼尊时刻；**回合结束自然流失 1 层**（`SkipNextDurationTick` + `TickDownDuration`）；升级标志 `SetAddUpgraded` 控制塞升级版
- `TemporaryLaughBoostPower` 临时增笑：`ModTemporaryAppliedPowerTemplate<JokeCard, LaughBoostPower>`——内部维护包装能力+真实增笑镜像，回合结束自动撤销；Title 取来源卡牌名
- `FirewallPower` 防火墙：获得状态牌时将其消耗（`AfterCardEnteredCombat`）

### 卡牌 `Cards/`（角色卡，Necrobinder 池）

- `BellyLaughCard` 捧腹开怀（1费攻击常见，**Necrobinder 起始×2**）：**6 欢愉伤害（公式）**，升级 +2（→8）；卡图硬编码 TestCard.png
- `SunkAgainCard` 又沉底了？（1费攻击常见）：**4 欢愉伤害（公式，升级 +2）** + 抽 1 张；**升级抽牌数走公式**（`ResolveWhenUpgraded`，基础 1）
- `AhaStrikeCard` 阿哈，打击！（1费攻击常见）：6 伤害 + 本回合临时增笑 1 + **Strike tag**（`CanonicalTags => [CardTag.Strike]`）；升级 +3
- `JokeCard` 灵魂段子手（0费技能常见）：本回合临时增笑 1 + 消耗；升级去消耗
- `CrosstalkCard` 笑话酿的酒（1费能力稀有）：获得 {Boost} 增笑，升级 1→2
- `SmileCard` 喜剧人（1费技能罕见）：**升级后** 欢愉 5 公式格挡（`ResolveWhenUpgraded`）+ 5笑点/4隐藏分/3好活当赏；升级全 +2（→8/6/5）
- `MirthCard` 如是众生欢笑不已（1费能力稀有）：给 1 层 MirthPower；升级奖励 12/4（**无固有**）
- `DivineLaughterCard` 神说要有笑声（1费能力罕见）：给 1 层 DivineLaughterPower；升级固有
- `LoanFutureCard` 贷款未来（0费技能常见，消耗）：**立即 +5 隐藏分 + 下回合 +1 能量（原版 `EnergyNextTurnPower`）**；升级去消耗
- `BigMoveCard` 整个大活（2费技能罕见）：欢愉：公式 2 层虚弱 + 2 层易伤；升级目标变全体（覆写 `TargetType`）
- `OutOfCardsCard` 你以为我没牌了？（0费技能罕见，消耗）：弃牌堆选 1 张入手 + 1 能量；升级额外抽 1
- `MyTurnDrawCard` 我的回合，抽卡！（1费能力罕见）：给 1 层 DrawOnPunchlinePower；升级阈值 10
- `BorrowTurnCard` 向天再借一回合（0费技能稀有，消耗）：丢所有手牌 + 从抽牌堆选 3 张入手；升级加保留
- `RevelryCard` 给你一只酱板鸭（X费技能罕见，消耗）：**3X 笑点 + 欢愉：4X 公式格挡**；升级格挡 4X→5X
- `FirewallCard` 防火墙（3费能力罕见）：**固有+保留**，打出给 1 层 FirewallPower（**获得状态牌时将其消耗**，`AfterCardEnteredCombat` 守卫 `card.Owner==Owner.Player && card.Type==CardType.Status` → `CardCmd.Exhaust`）；**升级：3费→2费（`EnergyCost.UpgradeBy(-1)`）**

### Token 卡（无角色）

- `GodModeCard` 无敌玩家，启动！（**保留** `CardKeyword.Retain`，0费能力 Token）：打出塞 1 张隐藏关：狼尊时刻（升级则升级版 `CardCmd.Upgrade`）+ 给 2 层 GodModePower；描述用 `{IfUpgraded:show:...}` 显示升级版名
- `WolfMomentCard` 隐藏关：狼尊时刻（X费攻击 Token，消耗）：**先对全体敌人造成 7 欢愉伤害（未升级 1 次/升级 2 次）`WithHitCount().TargetingAllOpponents`，再对随机敌人 7 欢愉伤害（未升级 X 次/升级 X+1 次）`TargetingRandomOpponents`**；升级不改伤害只改次数

### 遗物

- `TripleNineCartridgeRelic` 999卡带（**Necrobinder 起始遗物** + SharedRelicPool，Rare）：每打出 1 张牌（仅自己）+1 笑点；战斗开始 +5 好活当赏-剩余2回合；**图标已由用户提供**（`triple_nine_cartridge.png` / `_big.png` 为真实 PNG）
- `TripleNineGuardRelic` 999安全卫士（**Necrobinder 专属池** `NecrobinderRelicPool`，Common）：战斗开始 +1 人工制品（`BeforeCombatStart` + `PowerCmd.Apply<ArtifactPower>`）；悬浮预览人工制品
- `LaughterCanRelic` 笑声罐头（**Necrobinder 专属池**，Rare）：战斗开始 +1 增笑；悬浮预览增笑
  > 说明：Necrobinder 专属遗物注册到 `NecrobinderRelicPool`（非 SharedRelicPool）；原版：`RelicRarity.Common/Rare`

### 词条与悬浮

- **词条统一为一个「欢愉」**（`MyKeywords.Elation`，键 `SILVER_WOLF999_KEYWORD_ELATION`）：描述=「[gold]欢愉伤害[/gold]和受到[gold]欢愉[/gold]影响的[gold]能量[/gold]、[gold]格挡[/gold]、[gold]状态效果[/gold]等，会被[gold]增笑[/gold]和[gold]好活当赏[/gold]影响。」；**已删除单独的「欢愉伤害」词条**；卡牌描述统一用 `[gold]欢愉[/gold]伤害`
- **好活当赏悬浮文本**（写入 `CERTIFIED_BANGER_TWO/ONE_POWER` 的 description/smartDescription）：「阿哈时刻结束后，消耗的笑点会被计入好活当赏。好活当赏会以同种方式影响卡牌的欢愉伤害和受到欢愉影响的效果，至多使得它们被提升到4倍。」
- **凡显示欢愉悬浮的卡都挂** `FromPower<LaughBoostPower>()` + `FromPower<CertifiedBangerTwoPower>()`（捧腹/狼尊/又沉底/整个大活/喜剧人/酱板鸭）
- **悬浮提示写法/注意**：卡牌/能力/遗物都用 `AdditionalHoverTips`（**RitsuLib 的 `ExtraHoverTips` 是密封的，不能重写**）；用 `HoverTipFactory.FromCard<T>(bool upgrade)`（卡牌预览）/ `FromPower<T>()`（能力提示）/ `FromKeyword()`（词条）；条件式（升级才挂）用 getter 按 `IsUpgraded` 返回；有时按内部 Data 动态返回（如 GodModePower 升级版预览）；**加 FromPower 需确保卡文件有 `using SilverWolf999.Powers;`**
- 底层公式/能力实现与 UI 展示解耦：改词条只需动 `card_keywords.json` + 各卡 `AdditionalHoverTips`

### 控制台测试（战斗内）

```
power SILVER_WOLF999_POWER_LAUGH_BOOST_POWER 5 0
power SILVER_WOLF999_POWER_PUNCHLINE_POWER 10 0
power SILVER_WOLF999_POWER_CERTIFIED_BANGER_TWO_POWER 10 0
power SILVER_WOLF999_POWER_HIDDEN_MMR_POWER 80 0      # 60给无敌玩家 + 每30给1力量
power SILVER_WOLF999_POWER_MIRTH_POWER 1 0
power SILVER_WOLF999_POWER_DRAW_ON_PUNCHLINE_POWER 1 0
power SILVER_WOLF999_POWER_DIVINE_LAUGHTER_POWER 1 0
power SILVER_WOLF999_POWER_GOD_MODE_POWER 2 0
card SILVER_WOLF999_CARD_BELLY_LAUGH_CARD
card SILVER_WOLF999_CARD_SUNK_AGAIN_CARD
card SILVER_WOLF999_CARD_AHA_STRIKE_CARD
card SILVER_WOLF999_CARD_JOKE_CARD
card SILVER_WOLF999_CARD_CROSSTALK_CARD
card SILVER_WOLF999_CARD_SMILE_CARD
card SILVER_WOLF999_CARD_MIRTH_CARD
card SILVER_WOLF999_CARD_DIVINE_LAUGHTER_CARD
card SILVER_WOLF999_CARD_LOAN_FUTURE_CARD
card SILVER_WOLF999_CARD_BIG_MOVE_CARD
card SILVER_WOLF999_CARD_OUT_OF_CARDS_CARD
card SILVER_WOLF999_CARD_MY_TURN_DRAW_CARD
card SILVER_WOLF999_CARD_BORROW_TURN_CARD
card SILVER_WOLF999_CARD_REVELRY_CARD
card SILVER_WOLF999_CARD_FIREWALL_CARD
relic SILVER_WOLF999_RELIC_TRIPLE_NINE_CARTRIDGE_RELIC
relic SILVER_WOLF999_RELIC_TRIPLE_NINE_GUARD_RELIC
relic SILVER_WOLF999_RELIC_LAUGHTER_CAN_RELIC
```
> 无敌玩家/隐藏关狼尊时刻是 Token 卡，无法直接 `card` 拿，通过 隐藏分60 → 无敌玩家 → 狼尊时刻 链式获得。

### 待办

- 目前无（遗物图标已由用户补齐）。

## 沙箱/构建注意事项

- 本环境只允许写工作区；构建时"Copy Mod"目标会因无法写游戏 mods 目录而报错（**编译本身成功**，非代码问题）。需要干净编译时，临时给两个部署 Target 加 `Condition="'$(DeployMods)' == 'true'"`，不带该属性构建即跳过拷贝
- `dotnet new` 被沙箱拒绝（模板缓存）；`dotnet build` 可用（还原走已缓存 NuGet 包，离线 OK）
- NU1900 警告（连不上华为云 NuGet 镜像）无害，忽略
- write 工具对"已删除的文件"有观察缓存：用 pwsh 删掉文件/目录后再写会报 "file no longer exists"，先 read 刷新；csproj 若被 pwsh 改过，edit 前先 read
