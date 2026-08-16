using MegaCrit.Sts2.Core.Entities.Powers;
using STS2RitsuLib.Combat.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using SilverWolf999.Cards;

namespace SilverWolf999.Powers;

/// <summary>
/// 讲个段子的临时增笑：本回合内 +1 增笑，回合结束自动消失并撤销。
/// 模板内部维护两个状态：本包装能力 + 真实能力（LaughBoostPower）。
/// </summary>
[RegisterPower]
public class TemporaryLaughBoostPower : ModTemporaryAppliedPowerTemplate<JokeCard, LaughBoostPower>
{
    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://SilverWolf999/images/powers/temp_laugh_boost.png",
        BigIconPath: "res://SilverWolf999/images/powers/temp_laugh_boost_big.png"
    );
}
