using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.RelicPools;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using SilverWolf999.Powers;

namespace SilverWolf999.Relics;

/// <summary>
/// 999卡带：
/// 你每打出1张牌，获得1个"笑点"。
/// 每场战斗开始时，获得10层"好活当赏-剩余2回合"。
/// </summary>
[RegisterRelic(typeof(SharedRelicPool))]
public class TripleNineCartridgeRelic : ModRelicTemplate
{
    public override RelicRarity Rarity => RelicRarity.Rare;

    public override RelicAssetProfile AssetProfile => new(
        // 小图标（原版85x85）
        IconPath: "res://SilverWolf999/images/relics/triple_nine_cartridge.png",
        // 轮廓图标（原版85x85）
        IconOutlinePath: "res://SilverWolf999/images/relics/triple_nine_cartridge.png",
        // 大图标（原版256x256）
        BigIconPath: "res://SilverWolf999/images/relics/triple_nine_cartridge_big.png"
    );

    // 每打出1张牌，获得1个笑点
    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 只对遗物持有者自己打出的牌生效
        if (cardPlay.Player != Owner)
        {
            return;
        }

        await PowerCmd.Apply<LaughPointPower>(choiceContext, Owner.Creature, 1, Owner.Creature, null);
    }

    // 每场战斗开始时，获得10层"好活当赏-剩余2回合"
    public override async Task BeforeCombatStart()
    {
        Flash();
        await PowerCmd.Apply<AppreciationTwoPower>(new ThrowingPlayerChoiceContext(), Owner.Creature, 10, Owner.Creature, null);
    }
}
