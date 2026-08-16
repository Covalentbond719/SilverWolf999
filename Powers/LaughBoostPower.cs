using MegaCrit.Sts2.Core.Entities.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace SilverWolf999.Powers;

/// <summary>
/// 增笑：玩法定位类似机器人的"集中"。纯粹的数值能力，无自身逻辑。
/// 每层使"笑点"的回合结束伤害基础值 +1（被 PunchlinePower 读取）。
/// </summary>
[RegisterPower]
public class LaughBoostPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override bool AllowNegative => true;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://SilverWolf999/images/powers/laugh_boost.png",
        BigIconPath: "res://SilverWolf999/images/powers/laugh_boost_big.png"
    );
}
