using System.Text.Json;
using LuminaBox.Contracts.AiChat;

namespace LuminaBox.AiService.Tools;

/// <summary>Per-turn data a tool may need (who is asking, from which page).</summary>
public sealed class AiToolContext
{
    public AiToolContext(
        string turnId, string userId, string userRole, string clientRoute, IReadOnlyList<GuideOption> guides)
    {
        TurnId = turnId;
        UserId = userId;
        UserRole = userRole;
        ClientRoute = clientRoute;
        Guides = guides;
    }

    public string TurnId { get; }
    public string UserId { get; }

    /// <summary>User | Organization | Manager, empty when unknown.</summary>
    public string UserRole { get; }

    /// <summary>Page the user is on, empty when unknown.</summary>
    public string ClientRoute { get; }

    /// <summary>
    /// Screens the FE can lead this user to (its map minus excluded nodes, filtered by role). Empty = no guide tool.
    /// </summary>
    public IReadOnlyList<GuideOption> Guides { get; }

    /// <summary>
    /// Screen actions a tool asks the FE to perform. The orchestrator streams them right after the tool finishes.
    /// </summary>
    public List<UiAction> UiActions { get; } = new();

    /// <summary>Last screen offered to the user in this turn (guide id), null when none.</summary>
    public string? OfferedGuideId { get; set; }
}

/// <summary>
/// A function the LLM may call. Register each implementation in DI as <see cref="IAiTool"/>;
/// <see cref="Chat.ChatOrchestrator"/> sends all of them to the model.
/// Tools that need business data should call the BE (gRPC / HTTP), never the database directly.
/// </summary>
public interface IAiTool
{
    /// <summary>snake_case, unique.</summary>
    string Name { get; }

    /// <summary>What it does and when to use it — the model picks tools from this text.</summary>
    string Description { get; }

    /// <summary>Line shown to the user while the tool runs.</summary>
    string DisplayText { get; }

    /// <summary>JSON Schema (type: object) of the arguments.</summary>
    JsonElement ParametersSchema { get; }

    /// <summary>Returns the result as text (usually JSON) for the model. Report bad input as a result, do not throw.</summary>
    Task<string> ExecuteAsync(AiToolContext context, JsonElement arguments, CancellationToken ct);

    /// <summary>False = do not offer this tool in this turn at all.</summary>
    bool IsAvailable(AiToolContext context) => true;

    /// <summary>Description for this turn (a tool may list only what this user can use).</summary>
    string GetDescription(AiToolContext context) => Description;

    /// <summary>Argument schema for this turn (e.g. an enum narrowed to this user's options).</summary>
    JsonElement GetParametersSchema(AiToolContext context) => ParametersSchema;
}
