using MegaCrit.Sts2.Core.Entities.Cards;
using STS2RitsuLib.Content;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using SilverWolf999.Scripts;

namespace SilverWolf999.Cards;

// 自定义卡牌关键词
// 名词解释（title/description）是全局的，写在 {modId}/localization/{Language}/card_keywords.json，
// 键为 {MODID}_KEYWORD_{id大写}（例如 SILVER_WOLF999_KEYWORD_ELATION_DAMAGE / _KEYWORD_JOY）。
[RegisterOwnedCardKeyword(nameof(ElationDamage))]
[RegisterOwnedCardKeyword(nameof(Joy))]
public class MyKeywords
{
    // 欢愉伤害（Elation damage）
    public static readonly CardKeyword ElationDamage = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(ElationDamage)).GetModCardKeyword();

    // 欢愉（Joy）：好活当赏/增笑影响格挡和状态效果的场合
    public static readonly CardKeyword Joy = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(Joy)).GetModCardKeyword();
}
