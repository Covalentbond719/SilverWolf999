using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Cards;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace SilverWolf999.Cards;

// 注册卡牌到铁甲战士卡池
[RegisterCard(typeof(NecrobinderCardPool))]
public class OutOfCardsCard : ModCardTemplate
{
    // 消耗
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Exhaust,
    ];

    public OutOfCardsCard() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 从弃牌堆中选择一张牌加入手牌
        CardSelectorPrefs prefs = new(new LocString("cards", Id.Entry + ".selectionScreenPrompt"), 1);
        List<CardModel> discardCards = PileType.Discard.GetPile(Owner).Cards.ToList();
        CardModel? selected = (await CardSelectCmd.FromSimpleGrid(choiceContext, discardCards, Owner, prefs)).FirstOrDefault();
        if (selected != null)
        {
            await CardPileCmd.Add(selected, PileType.Hand);
        }

        // 获得1能量
        await PlayerCmd.GainEnergy(1, Owner);

        // 升级：额外抽1张牌
        if (IsUpgraded)
        {
            await CardPileCmd.Draw(choiceContext, 1, Owner);
        }
    }
}
