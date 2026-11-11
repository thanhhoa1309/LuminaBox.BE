namespace LuminaBox.Application.Commons
{
    /// <summary>Connection to LuminaBox.AiService. Section <c>AiService</c>.</summary>
    public sealed class AiChatSettings
    {
        public const string SectionName = "AiService";

        /// <summary>gRPC address of the AI service (plain HTTP/2 inside the private network).</summary>
        public string Address { get; set; } = "http://localhost:5290";

        /// <summary>Sent as <c>x-api-key</c>; must match <c>AiService:ApiKey</c> on the AI service.</summary>
        public string ApiKey { get; set; } = string.Empty;

        /// <summary>gRPC deadline of one chat turn.</summary>
        public int TurnTimeoutSeconds { get; set; } = 300;

        /// <summary>Token deltas arriving within this window are merged into one SignalR message.</summary>
        public int DeltaFlushMilliseconds { get; set; } = 40;
    }
}
