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
/// 好活当赏-剩余2回合：暂时是纯粹的计数器。
/// 回合结束时消失，并在下回合开始时获得相同层数的"好活当赏-剩余1回合"。
/// </summary>
[RegisterPower]
public class AppreciationTwoPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://SilverWolf999/images/powers/appreciation_two.png",
        BigIconPath: "res://SilverWolf999/images/powers/appreciation_two_big.png"
    );

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side != Owner.Side)
        {
            return;
        }

        Flash();

        // 转化为同层数的"好活当赏-剩余1回合"，然后本状态消失
        await PowerCmd.Apply<AppreciationOnePower>(choiceContext, Owner, Amount, Owner, null);
        await PowerCmd.Remove(this);
    }
}
