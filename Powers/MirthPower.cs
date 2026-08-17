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
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace SilverWolf999.Powers;

/// <summary>
/// 如是众生欢笑不已（buff）：每获得5个"笑点"，获得10点"隐藏分"和3点"格挡"。
/// 升级后奖励 12 隐藏分 + 4 格挡（由卡牌通过 SetRewards 传入）。累计余数跨回合保留。
/// </summary>
[RegisterPower]
public class MirthPower : ModPowerTemplate
{
    private const int Threshold = 5;

    private class Data
    {
        public decimal Pending;
        public int MmrReward = 10;
        public int BlockReward = 3;
    }

    // 描述显示用变量（Step=触发阈值；Mmr/Block 由卡牌 SetRewards 按升级写入）
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("Step", Threshold),
        new DynamicVar("Mmr", 10m),
        new DynamicVar("Block", 3m),
    ];

    protected override object InitInternalData()
    {
        return new Data();
    }

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

    /// <summary>设置每次触发奖励的隐藏分/格挡（升级后 12/4）</summary>
    public void SetRewards(int mmr, int block)
    {
        var data = GetInternalData<Data>();
        data.MmrReward = mmr;
        data.BlockReward = block;
        DynamicVars["Mmr"].BaseValue = mmr;
        DynamicVars["Block"].BaseValue = block;
    }

    // 每当获得笑点时累计，每满4个触发一次奖励
    public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (power is PunchlinePower && power.Owner == Owner && amount > 0)
        {
            var data = GetInternalData<Data>();
            data.Pending += amount;
            while (data.Pending >= Threshold)
            {
                data.Pending -= Threshold;
                await PowerCmd.Apply<HiddenMmrPower>(choiceContext, Owner, data.MmrReward, Owner, null);
                await CreatureCmd.GainBlock(Owner, data.BlockReward, ValueProp.Unpowered, null);
            }
        }
    }
}
