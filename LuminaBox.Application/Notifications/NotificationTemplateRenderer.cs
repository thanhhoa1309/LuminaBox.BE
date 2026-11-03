using System.Text.RegularExpressions;

namespace LuminaBox.Application.Notifications
{
    public static class NotificationTemplateRenderer
    {
        private static readonly Regex PlaceholderRegex = new(@"\{(\w+)\}", RegexOptions.Compiled);

        // Placeholders without a token value are rendered as empty strings
        public static string Render(string template, IReadOnlyDictionary<string, string> tokens)
            => PlaceholderRegex.Replace(
                template,
                match => tokens.TryGetValue(match.Groups[1].Value, out var value) ? value : string.Empty);
    }
}
