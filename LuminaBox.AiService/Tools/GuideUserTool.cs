using System.Text.Json;
using LuminaBox.Contracts.AiChat;

namespace LuminaBox.AiService.Tools;

/// <summary>
/// Leads the user to a screen: the FE opens the page (with parameters such as a search text) and highlights the
/// part they need. The options come from the FE with every request (<see cref="AiToolContext.Guides"/>), already
/// filtered by the user's role and the FE's exclusions; this service keeps no catalogue of its own.
/// Guides only move the user around the UI: real permissions are still enforced by the BE.
/// </summary>
public sealed class GuideUserTool : IAiTool
{
    private const int MaxParamLength = 100;

    public string Name => "guide_user";

    public string Description => BuildDescription([]);

    public string DisplayText => "Đang hướng đến trang cho bạn…";

    public JsonElement ParametersSchema => BuildSchema([]);

    public bool IsAvailable(AiToolContext context) => context.Guides.Count > 0;

    public string GetDescription(AiToolContext context) => BuildDescription(context.Guides);

    public JsonElement GetParametersSchema(AiToolContext context) => BuildSchema(context.Guides);

    public Task<string> ExecuteAsync(AiToolContext context, JsonElement arguments, CancellationToken ct)
    {
        var id = ReadString(arguments, "guide");
        var guide = context.Guides.FirstOrDefault(g => g.Id == id);
        if (guide is null)
            return Task.FromResult(Json(new
            {
                error = $"'{id}' is not available for this user. Use one of the listed options, or explain politely that you cannot open that screen.",
            }));

        var action = new UiAction { Type = "guide", GuideId = guide.Id };
        foreach (var parameter in guide.Params)
        {
            var value = Clean(ReadString(arguments, parameter.Name));
            if (value is null)
                return Task.FromResult(Json(new { error = $"'{parameter.Name}' is required for {guide.Id}: {parameter.Description}" }));
            action.Params[parameter.Name] = value;
        }

        context.UiActions.Add(action);
        return Task.FromResult(Json(new { status = "shown", page = guide.Label, @params = action.Params }));
    }

    private static string? ReadString(JsonElement arguments, string name)
        => arguments.ValueKind == JsonValueKind.Object
           && arguments.TryGetProperty(name, out var value)
           && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;

    // The value ends up in a URL and an input box: keep it short, one line, no control characters.
    private static string? Clean(string? value)
    {
        if (value is null)
            return null;
        var cleaned = new string(value.Where(c => !char.IsControl(c)).ToArray()).Trim();
        if (cleaned.Length > MaxParamLength)
            cleaned = cleaned[..MaxParamLength].TrimEnd();
        return cleaned.Length == 0 ? null : cleaned;
    }

    private static string BuildDescription(IEnumerable<GuideOption> guides) =>
        "Open the right page of the LuminaBox website for the user and highlight the part they need. " +
        "Use it when the user asks WHERE or HOW to do something on the website, or asks you to find / search products, " +
        "instead of describing the steps. " +
        "If the user clearly wants to go there (\"ở đâu\", \"chỉ mình\", \"dẫn mình\", \"mở giúp\", \"tìm giúp\"), call it right away. " +
        "If it is unclear whether they want to be taken there, first offer in one short question " +
        "(e.g. \"Mình dẫn bạn tới đó nhé?\") and call it after they agree. " +
        "After calling it, say in one short sentence that the page is open. " +
        "Options: " + string.Join("; ", guides.Select(Describe));

    private static string Describe(GuideOption g) => g.Params.Count == 0
        ? $"{g.Id} = {g.Description}"
        : $"{g.Id} = {g.Description} (requires {string.Join(", ", g.Params.Select(p => p.Name))})";

    private static JsonElement BuildSchema(IEnumerable<GuideOption> guides)
    {
        var list = guides.ToList();
        var properties = new Dictionary<string, object>
        {
            ["guide"] = new { type = "string", @enum = list.Select(g => g.Id).ToArray(), description = "Which screen to open." },
        };
        foreach (var parameter in list.SelectMany(g => g.Params).DistinctBy(p => p.Name))
            properties[parameter.Name] = new { type = "string", description = parameter.Description };

        return JsonSerializer.SerializeToElement(new { type = "object", properties, required = new[] { "guide" } });
    }

    private static string Json(object value) => JsonSerializer.Serialize(value, new JsonSerializerOptions
    {
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    });
}
