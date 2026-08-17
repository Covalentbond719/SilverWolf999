using System;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using SilverWolf999.Cards;

namespace SilverWolf999.Powers;

/// <summary>
/// 隐藏分（Hidden MMR）：
/// - 累计到60时（仅触发一次，60的其他倍数不重复触发），将一张"无敌玩家"加入手牌。
/// - 60以上时，每多出20隐藏分获得1点力量。
/// </summary>
[RegisterPower]
public class HiddenMmrPower : ModPowerTemplate
{
    private const int GodModeThreshold = 60;
    private const int StrengthStep = 30;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    // 悬浮提示：预览达到60时加入的"无敌玩家"
    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromCard<GodModeCard>(),
    ];

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://SilverWolf999/images/powers/hidden_mmr.png",
        BigIconPath: "res://SilverWolf999/images/powers/hidden_mmr_big.png"
    );

    // 描述显示用变量（Step=每30给1力量的步长）
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("Step", StrengthStep),
    ];

    private class Data
    {
        public bool GodModeCardGranted;
        public int GrantedStrength;
    }

    protected override object InitInternalData()
    {
        return new Data();
    }

    public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (power != this)
        {
            return;
        }

        var data = GetInternalData<Data>();

        // 累计到60时（仅一次），将一张"无敌玩家"加入手牌
        if (!data.GodModeCardGranted && Amount >= GodModeThreshold)
        {
            data.GodModeCardGranted = true;
            var combat = Owner.CombatState;
            if (combat != null && Owner.Player != null)
            {
                var godMode = combat.CreateCard<GodModeCard>(Owner.Player);
                await CardPileCmd.AddGeneratedCardToCombat(godMode, PileType.Hand, Owner.Player);
            }
        }

        // 每30隐藏分获得1力量（无门槛，从0开始按整数除法累计；按阈值增量同步实际力量层数）
        int targetStrength = Amount / StrengthStep;
        int delta = targetStrength - data.GrantedStrength;
        if (delta != 0)
        {
            data.GrantedStrength = targetStrength;
            await PowerCmd.Apply<StrengthPower>(choiceContext, Owner, delta, Owner, null);
        }
    }
}
