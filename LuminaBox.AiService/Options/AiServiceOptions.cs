namespace LuminaBox.AiService.Options;

/// <summary>Behaviour of this service. Section <c>AiService</c>.</summary>
public sealed class AiServiceOptions
{
    public const string SectionName = "AiService";

    /// <summary>
    /// Shared secret the BE must send in the <c>x-api-key</c> metadata. Empty = no check (local dev only).
    /// </summary>
    public string? ApiKey { get; set; }

    /// <summary>Max LLM rounds per turn (each tool-call batch, nudge or forced routing costs one round).</summary>
    public int MaxToolRounds { get; set; } = 6;

    /// <summary>Persona / tone. The tool-use rules in <see cref="Chat.ToolUsePolicy"/> are always appended.</summary>
    public string SystemPrompt { get; set; } =
        "Bạn là trợ lý của LuminaBox, sàn bánh trung thu. Trả lời ngắn gọn, thân thiện bằng tiếng Việt.";

    public StallGuardOptions StallGuard { get; set; } = new();
}

/// <summary>
/// "Stall" = the model announces it will fetch data ("đợi mình chút nhé…") but ends the round without a tool call.
/// Section <c>AiService:StallGuard</c>.
/// </summary>
public sealed class StallGuardOptions
{
    public bool Enabled { get; set; } = true;

    /// <summary>Replies longer than this are treated as real answers, never as stalls.</summary>
    public int MaxStallLength { get; set; } = 300;

    /// <summary>How many times to re-ask the model in plain text before forcing a decision.</summary>
    public int MaxNudges { get; set; } = 1;

    /// <summary>
    /// After the nudges, make one extra call with a JSON-schema <c>format</c> (constrained decoding) so the model
    /// MUST pick a tool or "answer". Our code then runs the tool itself.
    /// </summary>
    public bool ForcedRouting { get; set; } = true;

    /// <summary>Shown when every attempt still stalls.</summary>
    public string FallbackMessage { get; set; } =
        "Xin lỗi, mình chưa lấy được thông tin lúc này. Bạn hỏi lại giúp mình nhé.";
}
