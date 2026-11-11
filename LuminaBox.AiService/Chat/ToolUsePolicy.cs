namespace LuminaBox.AiService.Chat;

/// <summary>Fixed prompt texts of the anti-stall mechanism. Kept in code, not config: they are part of the logic.</summary>
internal static class ToolUsePolicy
{
    /// <summary>
    /// Appended to every system prompt. Explaining WHY ("your turn ends when you reply") works better on small
    /// models than a bare "do not announce".
    /// </summary>
    public const string SystemRules =
        """

        ## Tool rules
        - There is no "later". Your turn ends the moment you write a reply; nothing runs after it.
        - If you need data, call the tool NOW in this reply. Never write that you will check, fetch or look something up.
        - Never say "đợi mình một chút", "để mình kiểm tra", "mình sẽ lấy thông tin" or similar.
        - Never mention tool names or parameters to the user.
        - If the request is unclear, ask ONE short question instead of guessing.
        """;

    /// <summary>Sent as a USER message (mid-conversation system messages are weak or ignored by some chat templates).</summary>
    public const string Nudge =
        "[System notice] Your last reply only announced an action and called no tool, so the user received nothing. " +
        "Do it now: call the needed tool in this reply, or give the complete final answer. Do not announce anything.";

    public const string AnswerWithoutTools =
        "[System notice] No tool is needed. Give the complete final answer now with what you already know.";

    public const string RouterInstruction =
        "[System notice] Decide the single next step for the conversation above. " +
        "Reply with JSON only: {\"action\":\"call_tool\",\"tool\":\"<name>\",\"arguments\":{...}} " +
        "or {\"action\":\"answer\"} when no tool is needed. Available tools:\n";
}
