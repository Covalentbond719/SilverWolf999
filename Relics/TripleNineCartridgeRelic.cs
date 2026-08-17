using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Models.Characters;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using SilverWolf999.Powers;

namespace SilverWolf999.Relics;

/// <summary>
/// 999卡带：
/// 你每打出1张牌，获得1个"笑点"（Punchline）。
/// 每场战斗开始时，获得5层"好活当赏-剩余2回合"（Certified Banger）。
/// </summary>
// 注册到共享遗物池 + 铁甲战士起始遗物
[RegisterRelic(typeof(SharedRelicPool))]
[RegisterCharacterStarterRelic(typeof(Necrobinder), 1)]
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

    // 悬浮提示：预览获得的能力
    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<PunchlinePower>(),
        HoverTipFactory.FromPower<CertifiedBangerTwoPower>(),
    ];

    // 每打出1张牌，获得1个笑点
    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 只对遗物持有者自己打出的牌生效
        if (cardPlay.Player != Owner)
        {
            return;
        }

        await PowerCmd.Apply<PunchlinePower>(choiceContext, Owner.Creature, 1, Owner.Creature, null);
    }

    // 每场战斗开始时，获得5层"好活当赏-剩余2回合"
    public override async Task BeforeCombatStart()
    {
        Flash();
        await PowerCmd.Apply<CertifiedBangerTwoPower>(new ThrowingPlayerChoiceContext(), Owner.Creature, 5, Owner.Creature, null);
    }
}
