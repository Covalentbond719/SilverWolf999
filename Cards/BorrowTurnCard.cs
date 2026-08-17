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
public class BorrowTurnCard : ModCardTemplate
{
    // 消耗
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Exhaust,
    ];

    public BorrowTurnCard() : base(0, CardType.Skill, CardRarity.Rare, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 丢弃所有手牌
        var handCards = PileType.Hand.GetPile(Owner).Cards.ToList();
        await CardCmd.Discard(choiceContext, handCards);

        // 从抽牌堆中选择3张牌加入手牌
        CardSelectorPrefs prefs = new(new LocString("cards", Id.Entry + ".selectionScreenPrompt"), 3);
        List<CardModel> drawCards = PileType.Draw.GetPile(Owner).Cards.ToList();
        var selected = (await CardSelectCmd.FromSimpleGrid(choiceContext, drawCards, Owner, prefs)).ToList();
        foreach (var card in selected)
        {
            await CardPileCmd.Add(card, PileType.Hand);
        }
    }

    // 升级：保留（原版词条）
    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Retain);
    }
}
