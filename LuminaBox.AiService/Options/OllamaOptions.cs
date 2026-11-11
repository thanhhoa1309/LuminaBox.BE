namespace LuminaBox.AiService.Options;

/// <summary>Where and how to call the LLM (Ollama <c>POST /api/chat</c>). Section <c>Ollama</c>.</summary>
public sealed class OllamaOptions
{
    public const string SectionName = "Ollama";

    /// <summary>Ollama base URL: <c>http://localhost:11434</c> locally, or the ngrok URL of the GPU machine.</summary>
    public string BaseUrl { get; set; } = "http://localhost:11434";

    /// <summary>Model tag, must support tool calling (and thinking if <see cref="ChatRequest.EnableThinking"/> is used).</summary>
    public string Model { get; set; } = "qwen3:8b";

    /// <summary>Optional <c>Authorization: Bearer</c> value when Ollama sits behind an auth proxy / protected tunnel.</summary>
    public string? ApiKey { get; set; }

    /// <summary>Max tokens generated per round. Thinking tokens count towards this limit.</summary>
    public int NumPredict { get; set; } = 4096;

    /// <summary>Qwen3 recommendation with thinking on: temperature 0.6, top_p 0.95, top_k 20.</summary>
    public double Temperature { get; set; } = 0.6;

    public double? TopP { get; set; } = 0.95;

    public int? TopK { get; set; } = 20;

    /// <summary>Context window. Must hold system prompt + tool definitions + history, or Ollama silently cuts the start.</summary>
    public int NumCtx { get; set; } = 16384;

    /// <summary>Upper bound for one whole round (connect + full stream).</summary>
    public int RequestTimeoutSeconds { get; set; } = 300;
}
