using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using LuminaBox.AiService.Options;
using Microsoft.Extensions.Options;

namespace LuminaBox.AiService.Llm;

/// <summary>Thrown when Ollama cannot be reached or answers with an error.</summary>
public sealed class LlmException(string code, string message, Exception? inner = null) : Exception(message, inner)
{
    public string Code { get; } = code;
}

/// <summary>
/// Streams one round of <c>POST /api/chat</c> and yields every NDJSON line as soon as it arrives.
/// Cancelling <paramref name="ct"/> aborts the HTTP request, which makes Ollama stop generating.
/// </summary>
public sealed class OllamaChatClient
{
    public const string HttpClientName = "ollama";

    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
    };

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly OllamaOptions _options;
    private readonly ILogger<OllamaChatClient> _logger;

    public OllamaChatClient(IHttpClientFactory httpClientFactory, IOptions<OllamaOptions> options, ILogger<OllamaChatClient> logger)
    {
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
        _logger = logger;
    }

    internal async IAsyncEnumerable<OllamaStreamChunk> StreamAsync(
        List<OllamaMessage> messages,
        List<OllamaTool>? tools,
        bool think,
        [EnumeratorCancellation] CancellationToken ct,
        JsonElement? format = null,
        double? temperature = null)
    {
        var payload = new OllamaChatRequest
        {
            Model = _options.Model,
            Messages = messages,
            Tools = tools is { Count: > 0 } ? tools : null,
            Stream = true,
            Think = think,
            Format = format,
            Options = new OllamaRequestOptions
            {
                NumPredict = _options.NumPredict,
                NumCtx = _options.NumCtx,
                Temperature = temperature ?? _options.Temperature,
                TopP = _options.TopP,
                TopK = _options.TopK,
            },
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, "api/chat")
        {
            Content = JsonContent.Create(payload, options: Json),
        };
        if (!string.IsNullOrWhiteSpace(_options.ApiKey))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);

        var client = _httpClientFactory.CreateClient(HttpClientName);

        HttpResponseMessage response;
        try
        {
            // ResponseHeadersRead: start reading the body while the model is still generating.
            response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
        }
        catch (HttpRequestException ex)
        {
            throw new LlmException("llm_unavailable", $"Cannot reach Ollama at {_options.BaseUrl}.", ex);
        }

        using (response)
        {
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(ct);
                _logger.LogWarning("[Ollama] HTTP {Status}: {Body}", (int)response.StatusCode, body);
                throw new LlmException("llm_error", $"Ollama returned HTTP {(int)response.StatusCode}.");
            }

            await using var stream = await response.Content.ReadAsStreamAsync(ct);
            using var reader = new StreamReader(stream);

            while (await reader.ReadLineAsync(ct) is { } line)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var chunk = JsonSerializer.Deserialize<OllamaStreamChunk>(line, Json);
                if (chunk is null)
                    continue;
                if (chunk.Error is not null)
                    throw new LlmException("llm_error", chunk.Error);

                yield return chunk;

                if (chunk.Done)
                    yield break;
            }
        }
    }

    /// <summary>
    /// One non-thinking call whose output is forced to match <paramref name="schema"/> (constrained decoding).
    /// Returns the raw JSON text.
    /// </summary>
    internal async Task<string> CompleteJsonAsync(List<OllamaMessage> messages, JsonElement schema, CancellationToken ct)
    {
        var text = new System.Text.StringBuilder();
        await foreach (var chunk in StreamAsync(messages, tools: null, think: false, ct, format: schema, temperature: 0))
            text.Append(chunk.Message?.Content);
        return text.ToString();
    }
}
