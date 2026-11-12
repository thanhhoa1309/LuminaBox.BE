using System.Text;
using System.Text.Json;
using LuminaBox.AiService.Llm;
using LuminaBox.AiService.Tools;

namespace LuminaBox.AiService.Chat;

/// <summary>Result of a forced routing call. <see cref="ToolName"/> null = the model chose to answer without tools.</summary>
public sealed record RouterDecision(string? ToolName, JsonElement Arguments);

/// <summary>
/// Last resort against stalls. Ollama has no <c>tool_choice: "required"</c>, so we build it ourselves:
/// one call with a JSON-schema <c>format</c> — Ollama constrains decoding to the schema, so the model cannot
/// "promise" anything; it must output <c>{"action":"call_tool",...}</c> or <c>{"action":"answer"}</c>.
/// Our code then runs the chosen tool.
/// </summary>
public sealed class ForcedToolRouter
{
    private static readonly JsonElement EmptyObject = JsonDocument.Parse("{}").RootElement.Clone();

    private readonly OllamaChatClient _llm;
    private readonly ILogger<ForcedToolRouter> _logger;

    public ForcedToolRouter(OllamaChatClient llm, ILogger<ForcedToolRouter> logger)
    {
        _llm = llm;
        _logger = logger;
    }

    internal async Task<RouterDecision> DecideAsync(
        IReadOnlyList<OllamaMessage> conversation, IReadOnlyCollection<IAiTool> tools, AiToolContext context, CancellationToken ct)
    {
        var messages = new List<OllamaMessage>(conversation)
        {
            new() { Role = "user", Content = ToolUsePolicy.RouterInstruction + DescribeTools(tools, context) },
        };

        string raw;
        try
        {
            raw = await _llm.CompleteJsonAsync(messages, BuildSchema(tools), ct);
        }
        catch (LlmException ex)
        {
            _logger.LogWarning(ex, "[AiChat] Forced routing call failed — answering without tools.");
            return new RouterDecision(null, EmptyObject);
        }

        try
        {
            using var doc = JsonDocument.Parse(raw);
            var root = doc.RootElement;
            var action = root.TryGetProperty("action", out var a) ? a.GetString() : null;
            var tool = root.TryGetProperty("tool", out var t) && t.ValueKind == JsonValueKind.String ? t.GetString() : null;

            if (action == "call_tool" && tool is not null && tools.Any(x => x.Name == tool))
            {
                var args = root.TryGetProperty("arguments", out var g) && g.ValueKind == JsonValueKind.Object
                    ? g.Clone()
                    : EmptyObject;
                return new RouterDecision(tool, args);
            }
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "[AiChat] Forced routing returned invalid JSON: {Raw}", raw);
        }

        return new RouterDecision(null, EmptyObject);
    }

    private static string DescribeTools(IEnumerable<IAiTool> tools, AiToolContext context)
    {
        var sb = new StringBuilder();
        foreach (var t in tools)
            sb.Append("- ").Append(t.Name).Append(": ").Append(t.GetDescription(context))
              .Append(" Arguments schema: ").Append(t.GetParametersSchema(context).GetRawText()).Append('\n');
        return sb.ToString();
    }

    // Arguments stay a generic object: per-tool schemas (anyOf) are not reliably supported by every
    // Ollama version's grammar conversion. Each tool already validates its own arguments.
    private static JsonElement BuildSchema(IEnumerable<IAiTool> tools)
    {
        var schema = new
        {
            type = "object",
            properties = new
            {
                action = new { type = "string", @enum = new[] { "call_tool", "answer" } },
                tool = new { type = "string", @enum = tools.Select(t => t.Name).ToArray() },
                arguments = new { type = "object" },
            },
            required = new[] { "action" },
        };
        return JsonSerializer.SerializeToElement(schema);
    }
}
