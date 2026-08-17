using MegaCrit.Sts2.Core.Entities.Cards;
using STS2RitsuLib.Content;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using SilverWolf999.Scripts;

namespace SilverWolf999.Cards;

// 自定义卡牌关键词（已统一为"欢愉"一词条，覆盖欢愉伤害+能量/格挡/状态效果）
// 名词解释（title/description）是全局的，写在 {modId}/localization/{Language}/card_keywords.json，
// 键为 {MODID}_KEYWORD_{id大写}（例如 SILVER_WOLF999_KEYWORD_ELATION）。
[RegisterOwnedCardKeyword(nameof(Elation))]
public class MyKeywords
{
    // 欢愉（Elation）：欢愉伤害与受欢愉影响的能量/格挡/状态效果，受增笑和好活当赏影响
    public static readonly CardKeyword Elation = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(Elation)).GetModCardKeyword();
}
