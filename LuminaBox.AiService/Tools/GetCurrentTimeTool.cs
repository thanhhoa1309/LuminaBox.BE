using System.Text.Json;

namespace LuminaBox.AiService.Tools;

/// <summary>Sample tool with no external dependency — proves the tool-calling loop end to end.</summary>
public sealed class GetCurrentTimeTool : IAiTool
{
    private static readonly TimeZoneInfo VietnamTime = TimeZoneInfo.FindSystemTimeZoneById(
        OperatingSystem.IsWindows() ? "SE Asia Standard Time" : "Asia/Ho_Chi_Minh");

    public string Name => "get_current_time";

    public string Description =>
        "Get the current date and time in Vietnam (UTC+7). Use when the user asks about today, " +
        "the current time, or how many days are left until a date.";

    public string DisplayText => "Đang xem giờ hiện tại…";

    public JsonElement ParametersSchema { get; } = JsonDocument.Parse("""
        { "type": "object", "properties": {}, "required": [] }
        """).RootElement.Clone();

    public Task<string> ExecuteAsync(AiToolContext context, JsonElement arguments, CancellationToken ct)
    {
        var now = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, VietnamTime);
        return Task.FromResult(JsonSerializer.Serialize(new { now = now.ToString("yyyy-MM-dd HH:mm"), dayOfWeek = now.DayOfWeek.ToString() }));
    }
}
