namespace LuminaBox.Application.Commons
{
    public sealed class SupabaseStorageSettings
    {
        public const string ServiceKeyPlaceholder = "SET_SUPABASE_SERVICE_ROLE_KEY";

        public string Url { get; set; } = string.Empty;
        public string ServiceKey { get; set; } = string.Empty;
        public string PublicBucket { get; set; } = "images";
        public string PrivateBucket { get; set; } = "registration-documents";
        public long MaxImageBytes { get; set; } = 5 * 1024 * 1024;
        public long MaxDocumentBytes { get; set; } = 10 * 1024 * 1024;

        public bool IsConfigured =>
            !string.IsNullOrWhiteSpace(Url)
            && !string.IsNullOrWhiteSpace(ServiceKey)
            && ServiceKey != ServiceKeyPlaceholder;
    }
}
