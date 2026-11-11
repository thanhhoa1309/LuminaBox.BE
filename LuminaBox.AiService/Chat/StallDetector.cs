using System.Text.RegularExpressions;
using LuminaBox.AiService.Options;
using Microsoft.Extensions.Options;

namespace LuminaBox.AiService.Chat;

/// <summary>
/// Cheap heuristic: does a reply WITHOUT tool calls only announce an action instead of answering?
/// Called only when tools were offered. False positives cost one extra round, so it leans towards "stall"
/// for short replies, but never flags long replies or questions back to the user.
/// </summary>
public sealed partial class StallDetector
{
    // Promise phrases. Lower-case, matched on the normalised reply.
    private static readonly string[] Markers =
    {
        "đợi mình", "đợi em", "đợi tôi", "chờ mình", "chờ em", "chờ tôi", "chờ chút", "chờ xíu", "đợi chút", "đợi xíu",
        "một chút nhé", "1 chút nhé", "chút nhé", "giây lát", "vài giây", "lát nữa", "ngay đây", "ngay nhé",
        "đang kiểm tra", "đang tra", "đang tìm", "đang lấy", "đang xem", "đang truy",
        "let me check", "let me look", "let me fetch", "one moment", "hold on", "i'll check", "i will check",
        "fetching", "looking it up",
    };

    // "mình sẽ … kiểm tra", "để mình tra cứu", "em đi lấy" — a subject announcing a fetch verb.
    // Plain "để mình gợi ý…" is NOT matched: it usually comes with the real answer.
    [GeneratedRegex(@"(\bđể\s+(mình|em|tôi)|\b(mình|em|tôi)\s+(sẽ|đi))\s+(\S+\s+){0,2}?(kiểm tra|tra cứu|tra|lấy|tìm|xem|check)\b",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex WillFetchRegex();

    private readonly StallGuardOptions _options;

    public StallDetector(IOptions<AiServiceOptions> options) => _options = options.Value.StallGuard;

    public bool IsStall(string? reply)
    {
        // Ending the round with nothing at all is the worst stall.
        if (string.IsNullOrWhiteSpace(reply))
            return true;

        var text = reply.Trim();
        if (text.Length > _options.MaxStallLength)
            return false;

        // A question back to the user is a legitimate end of turn.
        if (text.EndsWith('?'))
            return false;

        // "Thông tin của bạn đây:" and then nothing.
        if (text.EndsWith(':'))
            return true;

        var lower = text.ToLowerInvariant();
        return Markers.Any(lower.Contains) || WillFetchRegex().IsMatch(lower);
    }
}
