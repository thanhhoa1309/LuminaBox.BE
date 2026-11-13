using System.Text.RegularExpressions;

namespace LuminaBox.Application.Utils
{
    /// <summary>
    /// Splits a free-text catalogue search ("bánh nhân thập cẩm") into lower-case words that must each match somewhere,
    /// instead of matching the whole sentence as one substring.
    /// </summary>
    public static class SearchTerms
    {
        private const int MaxTerms = 8;

        // Everything on the site is a mooncake: these phrases describe the whole catalogue, not one product.
        private static readonly string[] GenericPhrases = { "bánh trung thu", "trung thu" };

        // Filler words people type around the real keyword; they rarely appear in product data.
        private static readonly HashSet<string> FillerWords = new(StringComparer.Ordinal)
        {
            "bánh", "nhân", "vị", "loại", "mua", "tìm", "cho", "tôi", "mình", "có", "không", "của", "và", "với",
        };

        private static readonly Regex Separator = new(@"[\s,.;:!?/\\|()\[\]""'+\-]+", RegexOptions.Compiled);

        /// <summary>Distinct lower-case terms, at most <see cref="MaxTerms"/>; empty when there is nothing to search.</summary>
        public static IReadOnlyList<string> Split(string? search)
        {
            if (string.IsNullOrWhiteSpace(search))
                return Array.Empty<string>();

            var text = search.Trim().ToLowerInvariant().Normalize(System.Text.NormalizationForm.FormC);
            var words = ToWords(text);

            var stripped = text;
            foreach (var phrase in GenericPhrases)
                stripped = stripped.Replace(phrase, " ");
            var meaningful = ToWords(stripped).Where(w => !FillerWords.Contains(w)).ToList();

            // Only filler ("bánh", "trung thu"): search what was typed rather than return everything.
            return (meaningful.Count > 0 ? meaningful : words).Distinct().Take(MaxTerms).ToList();
        }

        private static List<string> ToWords(string text)
            => Separator.Split(text).Where(w => w.Length > 0).ToList();
    }
}
