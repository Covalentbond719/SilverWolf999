using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace SilverWolf999.Powers;

/// <summary>
/// 防火墙（buff）：你获得[状态牌]时，将其消耗。
/// </summary>
[RegisterPower]
public class FirewallPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://SilverWolf999/images/powers/firewall.png",
        BigIconPath: "res://SilverWolf999/images/powers/firewall_big.png"
    );

    // 获得状态牌时（进战斗），将其消耗（无 choiceContext，用 ThrowingPlayerChoiceContext）
    public override async Task AfterCardEnteredCombat(CardModel card)
    {
        if (card.Owner != Owner.Player)
        {
            return;
        }
        if (card.Type != CardType.Status)
        {
            return;
        }

        await CardCmd.Exhaust(new ThrowingPlayerChoiceContext(), card);
    }
}
