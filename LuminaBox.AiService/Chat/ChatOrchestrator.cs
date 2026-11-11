using System.Text;
using System.Text.Json;
using LuminaBox.AiService.Llm;
using LuminaBox.AiService.Options;
using LuminaBox.AiService.Tools;
using LuminaBox.Contracts.AiChat;
using Microsoft.Extensions.Options;

namespace LuminaBox.AiService.Chat;

/// <summary>
/// Runs one chat turn: LLM round → (tool calls → LLM round)* → done.
/// Every token / tool step is pushed through <c>emit</c> immediately, so the caller can stream it.
/// Exactly one terminal event (<see cref="Done"/> or <see cref="Error"/>) is emitted unless the turn is cancelled.
/// <para>
/// Anti-stall ladder when a round ends with no tool call but only announces one ("đợi mình chút nhé…"):
/// 1. prevention — <see cref="ToolUsePolicy.SystemRules"/> + stall replies removed from history;
/// 2. nudge — re-ask in a user message (<see cref="StallGuardOptions.MaxNudges"/>);
/// 3. forced routing — JSON-schema call, our code runs the chosen tool (<see cref="ForcedToolRouter"/>);
/// 4. fallback — fixed apology, never the stall text.
/// </para>
/// </summary>
public sealed class ChatOrchestrator
{
    private readonly OllamaChatClient _llm;
    private readonly ForcedToolRouter _router;
    private readonly StallDetector _stallDetector;
    private readonly IReadOnlyDictionary<string, IAiTool> _tools;
    private readonly AiServiceOptions _options;
    private readonly ILogger<ChatOrchestrator> _logger;

    public ChatOrchestrator(
        OllamaChatClient llm,
        ForcedToolRouter router,
        StallDetector stallDetector,
        IEnumerable<IAiTool> tools,
        IOptions<AiServiceOptions> options,
        ILogger<ChatOrchestrator> logger)
    {
        _llm = llm;
        _router = router;
        _stallDetector = stallDetector;
        _tools = tools.ToDictionary(t => t.Name, StringComparer.Ordinal);
        _options = options.Value;
        _logger = logger;
    }

    public async Task RunAsync(ChatRequest request, Func<ChatEvent, Task> emit, CancellationToken ct)
    {
        var seq = 0;
        Task Emit(ChatEvent e)
        {
            e.TurnId = request.TurnId;
            e.Seq = ++seq;
            return emit(e);
        }

        var guard = _options.StallGuard;
        var messages = BuildMessages(request);
        var toolDefs = _tools.Values.Select(ToOllamaTool).ToList();
        var toolContext = new AiToolContext(request.TurnId, request.UserId, request.UserRole, request.ClientRoute);
        var promptTokens = 0;
        var completionTokens = 0;

        var nudgesLeft = guard.MaxNudges;
        var routed = false;
        var toolsDisabled = false; // set when forced routing decided "answer without tools"

        try
        {
            for (var round = 0; round < _options.MaxToolRounds; round++)
            {
                var offerTools = !toolsDisabled && toolDefs.Count > 0;
                var content = new StringBuilder();
                var toolCalls = new List<OllamaToolCall>();
                OllamaStreamChunk? last = null;

                await foreach (var chunk in _llm.StreamAsync(messages, offerTools ? toolDefs : null, request.EnableThinking, ct))
                {
                    last = chunk;
                    var m = chunk.Message;
                    if (m is null)
                        continue;

                    if (!string.IsNullOrEmpty(m.Thinking))
                        await Emit(new ChatEvent { Thinking = new TextDelta { Text = m.Thinking } });

                    if (!string.IsNullOrEmpty(m.Content))
                    {
                        content.Append(m.Content);
                        await Emit(new ChatEvent { Content = new TextDelta { Text = m.Content } });
                    }

                    // Ollama may send tool_calls in any chunk, not only the last one.
                    if (m.ToolCalls is { Count: > 0 })
                        toolCalls.AddRange(m.ToolCalls);
                }

                promptTokens += last?.PromptEvalCount ?? 0;
                completionTokens += last?.EvalCount ?? 0;
                var text = content.ToString();

                // ── Normal tool round ─────────────────────────────────────────────
                if (toolCalls.Count > 0)
                {
                    // Text written next to tool calls is a preamble, not the answer.
                    if (text.Length > 0)
                        await Emit(new ChatEvent { RoundReset = new RoundReset { Reason = "tool_call" } });

                    // Keep the assistant turn (content + tool calls, never thinking) so the model sees what it asked for.
                    messages.Add(new OllamaMessage { Role = "assistant", Content = text, ToolCalls = toolCalls });
                    for (var i = 0; i < toolCalls.Count; i++)
                        await RunToolAndAppendAsync(toolCalls[i].Function, $"call_{round}_{i}", toolContext, messages, Emit, ct);
                    continue;
                }

                // ── Final answer? ─────────────────────────────────────────────────
                // Also checked when tools were withdrawn after forced routing: a promise there is still useless.
                var stalled = guard.Enabled && toolDefs.Count > 0 && _stallDetector.IsStall(text);
                if (!stalled)
                {
                    await EmitDone(last?.DoneReason ?? "stop");
                    return;
                }

                // ── Stall ladder ──────────────────────────────────────────────────
                if (text.Length > 0)
                {
                    await Emit(new ChatEvent { RoundReset = new RoundReset { Reason = "stall" } });
                    messages.Add(new OllamaMessage { Role = "assistant", Content = text }); // in-turn only, never persisted
                }

                if (nudgesLeft > 0)
                {
                    nudgesLeft--;
                    _logger.LogInformation("[AiChat] Turn {TurnId}: stall in round {Round}, nudging. Reply: {Reply}",
                        request.TurnId, round, Trim(text));
                    messages.Add(new OllamaMessage { Role = "user", Content = ToolUsePolicy.Nudge });
                    continue;
                }

                if (guard.ForcedRouting && !routed)
                {
                    routed = true;
                    var decision = await _router.DecideAsync(messages, _tools.Values.ToList(), ct);
                    _logger.LogInformation("[AiChat] Turn {TurnId}: stall persisted, forced routing chose {Tool}.",
                        request.TurnId, decision.ToolName ?? "answer");

                    if (decision.ToolName is not null)
                    {
                        // Write the call as if the model had emitted it, so the next round reads a normal history.
                        var call = new OllamaToolCallFunction { Name = decision.ToolName, Arguments = decision.Arguments };
                        messages.Add(new OllamaMessage
                        {
                            Role = "assistant",
                            ToolCalls = new List<OllamaToolCall> { new() { Function = call } },
                        });
                        await RunToolAndAppendAsync(call, $"call_{round}_forced", toolContext, messages, Emit, ct);
                    }
                    else
                    {
                        toolsDisabled = true;
                        messages.Add(new OllamaMessage { Role = "user", Content = ToolUsePolicy.AnswerWithoutTools });
                    }
                    continue;
                }

                break; // every rung used — fall through to the fallback answer
            }

            _logger.LogWarning("[AiChat] Turn {TurnId}: no usable answer after all attempts, sending fallback.", request.TurnId);
            await Emit(new ChatEvent { Content = new TextDelta { Text = guard.FallbackMessage } });
            await EmitDone("stall_fallback");
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            // Client stopped the turn: no terminal event, the stream is gone anyway.
            _logger.LogInformation("[AiChat] Turn {TurnId} cancelled by caller.", request.TurnId);
        }
        catch (LlmException ex)
        {
            _logger.LogWarning(ex, "[AiChat] Turn {TurnId} failed: {Code}", request.TurnId, ex.Code);
            await Emit(new ChatEvent { Error = new Error { Code = ex.Code, Message = ex.Message } });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[AiChat] Turn {TurnId} crashed.", request.TurnId);
            await Emit(new ChatEvent { Error = new Error { Code = "internal", Message = "AI service error." } });
        }

        Task EmitDone(string reason) => Emit(new ChatEvent
        {
            Done = new Done { FinishReason = reason, PromptTokens = promptTokens, CompletionTokens = completionTokens },
        });
    }

    private async Task RunToolAndAppendAsync(
        OllamaToolCallFunction call, string callId, AiToolContext context,
        List<OllamaMessage> messages, Func<ChatEvent, Task> emit, CancellationToken ct)
    {
        var result = await RunToolAsync(call, callId, context, emit, ct);
        messages.Add(new OllamaMessage { Role = "tool", ToolName = call.Name, Content = result });
    }

    private async Task<string> RunToolAsync(
        OllamaToolCallFunction call, string callId, AiToolContext context, Func<ChatEvent, Task> emit, CancellationToken ct)
    {
        if (!_tools.TryGetValue(call.Name, out var tool))
        {
            await emit(new ChatEvent { ToolFinished = new ToolFinished { CallId = callId, Name = call.Name, Success = false } });
            return JsonSerializer.Serialize(new { error = $"Unknown tool '{call.Name}'." });
        }

        await emit(new ChatEvent { ToolStarted = new ToolStarted { CallId = callId, Name = tool.Name, DisplayText = tool.DisplayText } });

        try
        {
            var result = await tool.ExecuteAsync(context, call.Arguments, ct);
            await emit(new ChatEvent { ToolFinished = new ToolFinished { CallId = callId, Name = tool.Name, Success = true } });

            foreach (var action in context.UiActions)
                await emit(new ChatEvent { UiAction = action });
            context.UiActions.Clear();

            return result;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // A broken tool must not kill the turn: tell the model and let it answer without the data.
            _logger.LogError(ex, "[AiChat] Tool {Tool} failed.", tool.Name);
            await emit(new ChatEvent { ToolFinished = new ToolFinished { CallId = callId, Name = tool.Name, Success = false } });
            return JsonSerializer.Serialize(new { error = "Tool failed, answer without this data." });
        }
    }

    private List<OllamaMessage> BuildMessages(ChatRequest request)
    {
        var messages = new List<OllamaMessage>(request.Messages.Count + 1)
        {
            new() { Role = "system", Content = _options.SystemPrompt + ToolUsePolicy.SystemRules + PageContext(request.ClientRoute) },
        };

        var dropped = 0;
        for (var i = 0; i < request.Messages.Count; i++)
        {
            var m = request.Messages[i];
            if (string.IsNullOrWhiteSpace(m.Content))
                continue;

            switch (m.Role)
            {
                case ChatRole.User:
                    messages.Add(new OllamaMessage { Role = "user", Content = m.Content });
                    break;

                // A stall reply left in history teaches the model to stall again (it imitates its own past turns).
                case ChatRole.Assistant when _options.StallGuard.Enabled && _stallDetector.IsStall(m.Content):
                    dropped++;
                    break;

                case ChatRole.Assistant:
                    messages.Add(new OllamaMessage { Role = "assistant", Content = m.Content });
                    break;
            }
        }

        if (dropped > 0)
            _logger.LogInformation("[AiChat] Turn {TurnId}: dropped {Count} stall replies from history.", request.TurnId, dropped);

        return messages;
    }

    private static string PageContext(string route)
        => string.IsNullOrWhiteSpace(route) ? string.Empty : $"\n\nThe user is currently on the website page: {route}";

    private static string Trim(string s) => s.Length <= 120 ? s : s[..120] + "…";

    private static OllamaTool ToOllamaTool(IAiTool tool) => new()
    {
        Function = new OllamaFunction { Name = tool.Name, Description = tool.Description, Parameters = tool.ParametersSchema },
    };
}
