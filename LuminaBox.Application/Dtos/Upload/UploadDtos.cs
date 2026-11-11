namespace LuminaBox.Application.Dtos.Upload
{
    public enum UploadPurpose
    {
        ProductImage,
        BoxImage,
        FeedbackImage,
        EventImage,
        BannerImage,
        RegistrationDocument
    }

    public class UploadFile
    {
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long Length { get; set; }
        public Stream Content { get; set; } = Stream.Null;
    }

    public class UploadResponse
    {
        public string Path { get; set; } = string.Empty;
        public string? Url { get; set; }
        public bool IsPrivate { get; set; }
        public string ContentType { get; set; } = string.Empty;
        public long Size { get; set; }
    }
}
