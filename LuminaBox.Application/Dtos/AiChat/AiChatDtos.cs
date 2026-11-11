namespace LuminaBox.Application.Dtos.AiChat
{
    public enum AiChatRole
    {
        User,
        Assistant,
    }

    public enum AiChatEventType
    {
        Thinking,
        Content,
        ToolStarted,
        ToolFinished,
        Done,
        Error,

        /// <summary>Content received since the last reset was not the final answer: move it out of the answer bubble.</summary>
        RoundReset,

        /// <summary>The FE should act on screen, e.g. open a page and highlight a button (<c>ActionType = guide</c>).</summary>
        UiAction,
    }

    public class AiChatMessageDto
    {
        public AiChatRole Role { get; set; }
        public string Content { get; set; } = string.Empty;
    }

    /// <summary>Argument of the hub method <c>StreamChat</c>.</summary>
    public class AiChatRequest
    {
        /// <summary>Conversation so far, oldest first; the last message must be the user's new question.</summary>
        public List<AiChatMessageDto> Messages { get; set; } = new();

        public bool EnableThinking { get; set; } = true;

        /// <summary>Path of the page the user is on (e.g. <c>/</c>, <c>/account/profile</c>), without query string.</summary>
        public string? CurrentRoute { get; set; }
    }

    /// <summary>One item of the <c>StreamChat</c> stream. Fields not used by <see cref="Type"/> are null.</summary>
    public class AiChatStreamEvent
    {
        public string TurnId { get; set; } = string.Empty;
        public int Seq { get; set; }
        public AiChatEventType Type { get; set; }

        /// <summary>Thinking / Content: the new text to append.</summary>
        public string? Text { get; set; }

        /// <summary>ToolStarted / ToolFinished.</summary>
        public string? ToolCallId { get; set; }
        public string? ToolName { get; set; }
        public string? DisplayText { get; set; }
        public bool? Success { get; set; }

        /// <summary>Done.</summary>
        public string? FinishReason { get; set; }
        public int? PromptTokens { get; set; }
        public int? CompletionTokens { get; set; }

        /// <summary>Error.</summary>
        public string? ErrorCode { get; set; }
        public string? ErrorMessage { get; set; }

        /// <summary>RoundReset: <c>tool_call</c> (preamble before a tool) or <c>stall</c> (empty promise).</summary>
        public string? ResetReason { get; set; }

        /// <summary>UiAction: <c>guide</c>.</summary>
        public string? ActionType { get; set; }

        /// <summary>UiAction guide: id from the shared guide catalogue, e.g. <c>add_address</c>.</summary>
        public string? GuideId { get; set; }
    }

    /// <summary>What the BE sends to the AI service for one turn.</summary>
    public sealed record AiChatTurn(
        string TurnId,
        Guid UserId,
        string? UserRole,
        string? ClientRoute,
        IReadOnlyList<AiChatMessageDto> Messages,
        bool EnableThinking);
}
