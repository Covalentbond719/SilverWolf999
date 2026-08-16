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
/// 如是众生欢笑不已（buff）：每获得1个"笑点"，获得1点"隐藏分"。
/// </summary>
[RegisterPower]
public class MirthPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    // 悬浮提示：预览涉及的能力
    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<PunchlinePower>(),
        HoverTipFactory.FromPower<HiddenMmrPower>(),
    ];

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://SilverWolf999/images/powers/mirth.png",
        BigIconPath: "res://SilverWolf999/images/powers/mirth_big.png"
    );

    // 每当笑点层数变化（只响应增加），按获得量 1:1 给隐藏分
    public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (power is PunchlinePower && power.Owner == Owner && amount > 0)
        {
            await PowerCmd.Apply<HiddenMmrPower>(choiceContext, Owner, (int)amount, Owner, null);
        }
    }
}
