using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace SilverWolf999.Powers;

/// <summary>
/// 我的回合，抽卡！（buff）：每获得 Step（12，升级后10）个"笑点"，抽1张牌。
/// 累计跨回合保留余数。
/// </summary>
[RegisterPower]
public class DrawOnPunchlinePower : ModPowerTemplate
{
    private class Data
    {
        public decimal Pending;
    }

    // 用于描述显示当前阈值
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("Step", 12m),
    ];

    protected override object InitInternalData()
    {
        return new Data();
    }

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    // 悬浮提示：预览"笑点"
    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<PunchlinePower>(),
    ];

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://SilverWolf999/images/powers/draw_on_punchline.png",
        BigIconPath: "res://SilverWolf999/images/powers/draw_on_punchline_big.png"
    );

    /// <summary>设置抽卡阈值（升级后6，默认8）</summary>
    public void SetStep(int step)
    {
        DynamicVars["Step"].BaseValue = step;
    }

    // 每当获得笑点时累计，每满 Step 抽1张牌
    public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (power is PunchlinePower && power.Owner == Owner && amount > 0)
        {
            var data = GetInternalData<Data>();
            data.Pending += amount;
            int step = DynamicVars["Step"].IntValue;
            if (step <= 0)
            {
                return;
            }
            while (data.Pending >= step)
            {
                data.Pending -= step;
                if (Owner.Player != null)
                {
                    await CardPileCmd.Draw(choiceContext, 1, Owner.Player);
                }
            }
        }
    }
}
