using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace SilverWolf999.Powers;

/// <summary>
/// 神说要有笑声（buff）：每消耗1点能量，获得"自身层数"个"笑点"。
/// 多次打出同一张能力卡会叠加本能力层数，从而每点能量给更多笑点。
/// </summary>
[RegisterPower]
public class DivineLaughterPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    // 悬浮提示：预览"笑点"
    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<PunchlinePower>(),
    ];

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://SilverWolf999/images/powers/divine_laughter.png",
        BigIconPath: "res://SilverWolf999/images/powers/divine_laughter_big.png"
    );

    // 每消耗1能量，获得"本能力层数 Amount"个笑点（无 choiceContext，用 ThrowingPlayerChoiceContext）
    public override async Task AfterEnergySpent(CardModel card, int amount)
    {
        if (card.Owner != Owner.Player || amount <= 0 || Amount <= 0)
        {
            return;
        }

        await PowerCmd.Apply<PunchlinePower>(new ThrowingPlayerChoiceContext(), Owner, Amount, Owner, null);
    }
}
