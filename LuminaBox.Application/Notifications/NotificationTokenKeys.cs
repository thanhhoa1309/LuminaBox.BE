namespace LuminaBox.Application.Notifications
{
    public static class NotificationTokenKeys
    {
        // Filled by the dispatcher from the current user, never by the catalog
        public const string ActorName = "actorName";

        public const string BrandName = "brandName";
        public const string OrderCode = "orderCode";
        public const string CommissionRate = "commissionRate";
        public const string Reason = "reason";
        public const string StallCode = "stallCode";

        public static IReadOnlyDictionary<string, string> Create(
            string? brandName = null,
            string? orderCode = null,
            string? commissionRate = null,
            string? reason = null,
            string? stallCode = null)
        {
            var tokens = new Dictionary<string, string>();

            Add(tokens, BrandName, brandName);
            Add(tokens, OrderCode, orderCode);
            Add(tokens, CommissionRate, commissionRate);
            Add(tokens, Reason, reason);
            Add(tokens, StallCode, stallCode);

            return tokens;
        }

        private static void Add(Dictionary<string, string> tokens, string key, string? value)
        {
            if (!string.IsNullOrWhiteSpace(value))
                tokens[key] = value;
        }
    }
}
