using System.Text.Json;
using LuminaBox.Contracts.AiChat;

namespace LuminaBox.AiService.Tools;

/// <summary>
/// Leads the user to a screen: the FE opens the page and highlights the button.
/// The model only picks an id from <see cref="GuideCatalog"/>; it never sees routes or selectors.
/// </summary>
public sealed class GuideUserTool : IAiTool
{
    public string Name => "guide_user";

    public string Description =>
        "Open the right page of the LuminaBox website for the user and highlight the button they need. " +
        "Use it whenever the user asks WHERE or HOW to do something on the website (e.g. add an address, " +
        "change the password) instead of describing the steps. After calling it, tell the user in one short " +
        "sentence that the page is open and the highlighted button is what they need. Options: " +
        string.Join("; ", GuideCatalog.All.Select(g =>
            g.Roles.Length == 0 ? $"{g.Id} = {g.Description}" : $"{g.Id} = {g.Description} (only {string.Join("/", g.Roles)})"));

    public string DisplayText => "Đang mở đúng trang cho bạn…";

    public JsonElement ParametersSchema { get; } = JsonSerializer.SerializeToElement(new
    {
        type = "object",
        properties = new
        {
            guide = new
            {
                type = "string",
                @enum = GuideCatalog.All.Select(g => g.Id).ToArray(),
                description = "Which screen to open.",
            },
        },
        required = new[] { "guide" },
    });

    public Task<string> ExecuteAsync(AiToolContext context, JsonElement arguments, CancellationToken ct)
    {
        var id = arguments.ValueKind == JsonValueKind.Object && arguments.TryGetProperty("guide", out var g)
            ? g.GetString()
            : null;

        var guide = GuideCatalog.Find(id);
        if (guide is null)
            return Task.FromResult(Json(new { error = $"Unknown guide '{id}'. Pick one of the listed options." }));

        if (!GuideCatalog.IsAllowed(guide, context.UserRole))
            return Task.FromResult(Json(new { error = "This screen is not available for this user's account type. Explain that politely." }));

        context.UiActions.Add(new UiAction { Type = "guide", GuideId = guide.Id });
        return Task.FromResult(Json(new { status = "shown", page = guide.Label }));
    }

    private static string Json(object value) => JsonSerializer.Serialize(value, new JsonSerializerOptions
    {
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    });
}
