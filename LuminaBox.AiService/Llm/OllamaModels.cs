using System.Text.Json;
using System.Text.Json.Serialization;

namespace LuminaBox.AiService.Llm;

// Wire models for Ollama POST /api/chat. Only the fields this service uses.

internal sealed class OllamaChatRequest
{
    [JsonPropertyName("model")] public required string Model { get; init; }
    [JsonPropertyName("messages")] public required List<OllamaMessage> Messages { get; init; }
    [JsonPropertyName("tools")] public List<OllamaTool>? Tools { get; init; }
    [JsonPropertyName("stream")] public bool Stream { get; init; } = true;
    [JsonPropertyName("think")] public bool Think { get; init; }
    [JsonPropertyName("options")] public OllamaRequestOptions? Options { get; init; }

    /// <summary>JSON Schema → Ollama constrains decoding so the output always matches it.</summary>
    [JsonPropertyName("format")] public JsonElement? Format { get; init; }
}

internal sealed class OllamaRequestOptions
{
    [JsonPropertyName("num_predict")] public int NumPredict { get; init; }
    [JsonPropertyName("num_ctx")] public int NumCtx { get; init; }
    [JsonPropertyName("temperature")] public double Temperature { get; init; }
    [JsonPropertyName("top_p")] public double? TopP { get; init; }
    [JsonPropertyName("top_k")] public int? TopK { get; init; }
}

internal sealed class OllamaMessage
{
    [JsonPropertyName("role")] public required string Role { get; init; } // system | user | assistant | tool
    [JsonPropertyName("content")] public string Content { get; init; } = string.Empty;
    [JsonPropertyName("tool_calls")] public List<OllamaToolCall>? ToolCalls { get; init; }
    [JsonPropertyName("tool_name")] public string? ToolName { get; init; } // role = tool only
}

internal sealed class OllamaTool
{
    [JsonPropertyName("type")] public string Type { get; init; } = "function";
    [JsonPropertyName("function")] public required OllamaFunction Function { get; init; }
}

internal sealed class OllamaFunction
{
    [JsonPropertyName("name")] public required string Name { get; init; }
    [JsonPropertyName("description")] public required string Description { get; init; }
    [JsonPropertyName("parameters")] public required JsonElement Parameters { get; init; } // JSON Schema
}

internal sealed class OllamaToolCall
{
    [JsonPropertyName("function")] public required OllamaToolCallFunction Function { get; init; }
}

internal sealed class OllamaToolCallFunction
{
    [JsonPropertyName("name")] public required string Name { get; init; }
    [JsonPropertyName("arguments")] public JsonElement Arguments { get; init; }
}

/// <summary>One NDJSON line of the streamed response.</summary>
internal sealed class OllamaStreamChunk
{
    [JsonPropertyName("message")] public OllamaChunkMessage? Message { get; init; }
    [JsonPropertyName("done")] public bool Done { get; init; }
    [JsonPropertyName("done_reason")] public string? DoneReason { get; init; }
    [JsonPropertyName("prompt_eval_count")] public int PromptEvalCount { get; init; }
    [JsonPropertyName("eval_count")] public int EvalCount { get; init; }
    [JsonPropertyName("error")] public string? Error { get; init; }
}

internal sealed class OllamaChunkMessage
{
    [JsonPropertyName("content")] public string? Content { get; init; }
    [JsonPropertyName("thinking")] public string? Thinking { get; init; }
    [JsonPropertyName("tool_calls")] public List<OllamaToolCall>? ToolCalls { get; init; }
}
