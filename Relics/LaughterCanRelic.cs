using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.RelicPools;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using SilverWolf999.Powers;

namespace SilverWolf999.Relics;

// 注册到亡灵契约师专属遗物池
[RegisterRelic(typeof(NecrobinderRelicPool))]
public class LaughterCanRelic : ModRelicTemplate
{
    public override RelicRarity Rarity => RelicRarity.Rare;

    public override RelicAssetProfile AssetProfile => new(
        IconPath: "res://SilverWolf999/images/relics/laughter_can.png",
        IconOutlinePath: "res://SilverWolf999/images/relics/laughter_can.png",
        BigIconPath: "res://SilverWolf999/images/relics/laughter_can_big.png"
    );

    // 悬浮提示：预览获得的增笑
    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<LaughBoostPower>(),
    ];

    // 战斗开始时，获得1点增笑
    public override async Task BeforeCombatStart()
    {
        Flash();
        await PowerCmd.Apply<LaughBoostPower>(new ThrowingPlayerChoiceContext(), Owner.Creature, 1, Owner.Creature, null);
    }
}
