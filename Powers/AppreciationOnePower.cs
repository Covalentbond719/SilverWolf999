using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace SilverWolf999.Powers;

/// <summary>
/// 好活当赏-剩余1回合：暂时是另一个纯粹的计数器。
/// 回合结束时消失。
/// </summary>
[RegisterPower]
public class AppreciationOnePower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://SilverWolf999/images/powers/appreciation_one.png",
        BigIconPath: "res://SilverWolf999/images/powers/appreciation_one_big.png"
    );

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side != Owner.Side)
        {
            return;
        }

        Flash();

        // 纯计数器：回合结束直接消失
        await PowerCmd.Remove(this);
    }
}
