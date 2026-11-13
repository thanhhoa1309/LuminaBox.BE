using System.Text.Json;
using LuminaBox.Contracts.AiChat;

namespace LuminaBox.AiService.Tools;

/// <summary>
/// Offers to lead the user to a screen: the FE shows a confirm button and, if pressed, opens the page (with
/// parameters such as a search text) and highlights the part they need. The options come from the FE with every request (<see cref="AiToolContext.Guides"/>), already
/// filtered by the user's role and the FE's exclusions; this service keeps no catalogue of its own.
/// Guides only move the user around the UI: real permissions are still enforced by the BE.
/// </summary>
public sealed class GuideUserTool : IAiTool
{
    private const int MaxParamLength = 100;

    public const string ToolName = "guide_user";

    public string Name => ToolName;

    /// <summary>Tool result for an offered screen (also used to rebuild past calls in the history).</summary>
    public static string OfferedResult(string guideId, object? parameters = null, GuideOption? option = null) =>
        Json(new
        {
            status = "offered",
            guide = guideId,
            page = option?.Label,
            purpose = option?.Description,
            @params = parameters,
            next = "Now explain the steps on that screen, then mention the \"Mở trang\" button.",
        });

    public string Description => BuildDescription([]);

    public string DisplayText => "Đang chuẩn bị đường dẫn cho bạn…";

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
        return Task.FromResult(OfferedResult(guide.Id, action.Params, guide));
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
        "Use it when the user asks WHERE or HOW to do something on the website, or asks you to find / search products. " +
        "Call it directly, without asking first: the website shows the user an \"Mở trang\" button and only goes there if they press it. " +
        "After calling it, still answer the question: say in 2-4 short steps what the user does on that screen " +
        "(what to press, what to fill in, how to finish), using the option's description and what the user told you, " +
        "then end with one line that they can press \"Mở trang\" below. Never say the page is already open. " +
        "Do not invent field names or buttons you are not sure about; keep those steps general instead. " +
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
