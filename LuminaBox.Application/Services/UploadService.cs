using Microsoft.Extensions.Options;
using LuminaBox.Application.Commons;
using LuminaBox.Application.Dtos.Upload;
using LuminaBox.Application.Interface;
using LuminaBox.Application.Utils;

namespace LuminaBox.Application.Services
{
    public class UploadService : IUploadService
    {
        private const string Organization = "Organization";
        private const string Manager = "Manager";

        private static readonly Dictionary<string, (string Extension, byte[][] Signatures)> ImageTypes = new()
        {
            ["image/jpeg"] = (".jpg", new[] { new byte[] { 0xFF, 0xD8, 0xFF } }),
            ["image/png"] = (".png", new[] { new byte[] { 0x89, 0x50, 0x4E, 0x47 } }),
            ["image/webp"] = (".webp", new[] { "RIFF"u8.ToArray() })
        };

        private static readonly (string Extension, byte[][] Signatures) PdfType = (".pdf", new[] { "%PDF"u8.ToArray() });

        private static readonly Dictionary<UploadPurpose, (string Folder, string[]? Roles, bool IsPrivate)> Rules = new()
        {
            [UploadPurpose.ProductImage] = ("products", new[] { Organization }, false),
            [UploadPurpose.BoxImage] = ("boxes", new[] { Organization }, false),
            [UploadPurpose.EventImage] = ("events", new[] { Organization }, false),
            [UploadPurpose.BannerImage] = ("banners", new[] { Organization, Manager }, false),
            [UploadPurpose.FeedbackImage] = ("feedbacks", null, false),
            [UploadPurpose.RegistrationDocument] = ("registration-documents", null, true)
        };

        private readonly IFileStorage _storage;
        private readonly IClaimsService _claimsService;
        private readonly ICurrentTime _currentTime;
        private readonly SupabaseStorageSettings _settings;

        public UploadService(IFileStorage storage, IClaimsService claimsService, ICurrentTime currentTime, IOptions<SupabaseStorageSettings> settings)
        {
            _storage = storage;
            _claimsService = claimsService;
            _currentTime = currentTime;
            _settings = settings.Value;
        }

        public async Task<UploadResponse> UploadAsync(UploadPurpose purpose, UploadFile file, IReadOnlyCollection<string> roles, CancellationToken cancellationToken = default)
        {
            if (!Rules.TryGetValue(purpose, out var rule))
                throw ErrorHelper.BadRequest("Upload purpose is invalid.", "E20.1");

            if (rule.Roles is not null && !rule.Roles.Any(roles.Contains))
                throw ErrorHelper.Forbidden("You cannot upload files for this purpose.", "E20.2");

            if (file.Length <= 0)
                throw ErrorHelper.BadRequest("The file is empty.", "E20.3");

            var contentType = file.ContentType.Trim().ToLowerInvariant();
            var allowsPdf = purpose == UploadPurpose.RegistrationDocument;
            (string Extension, byte[][] Signatures) type;

            if (ImageTypes.TryGetValue(contentType, out var imageType))
                type = imageType;
            else if (allowsPdf && contentType == "application/pdf")
                type = PdfType;
            else
                throw ErrorHelper.BadRequest(allowsPdf ? "Only JPG, PNG, WEBP or PDF files are allowed." : "Only JPG, PNG or WEBP images are allowed.", "E20.3");

            var maxBytes = type == PdfType ? _settings.MaxDocumentBytes : _settings.MaxImageBytes;
            if (file.Length > maxBytes)
                throw ErrorHelper.BadRequest($"The file must not exceed {maxBytes / (1024 * 1024)} MB.", "E20.4");

            using var buffer = new MemoryStream();
            await file.Content.CopyToAsync(buffer, cancellationToken);

            if (buffer.Length != file.Length || !MatchesSignature(buffer.GetBuffer(), type.Signatures, contentType))
                throw ErrorHelper.BadRequest("The file content does not match its type.", "E20.3");

            if (!_settings.IsConfigured)
                throw ErrorHelper.Internal("File storage is not configured.", "E20.5");

            buffer.Position = 0;
            var now = _currentTime.GetCurrentTime();
            var userId = _claimsService.GetRequiredUserId();
            var path = $"{rule.Folder}/{now:yyyy}/{now:MM}/{userId:N}/{Guid.NewGuid():N}{type.Extension}";
            var bucket = rule.IsPrivate ? _settings.PrivateBucket : _settings.PublicBucket;

            await _storage.UploadAsync(bucket, path, buffer, contentType, cancellationToken);

            return new UploadResponse
            {
                Path = path,
                Url = rule.IsPrivate ? null : _storage.PublicUrl(bucket, path),
                IsPrivate = rule.IsPrivate,
                ContentType = contentType,
                Size = file.Length
            };
        }

        private static bool MatchesSignature(byte[] data, byte[][] signatures, string contentType)
        {
            var matches = signatures.Any(signature => data.Length >= signature.Length && data.AsSpan(0, signature.Length).SequenceEqual(signature));

            if (matches && contentType == "image/webp")
                return data.Length >= 12 && data.AsSpan(8, 4).SequenceEqual("WEBP"u8);

            return matches;
        }
    }
}
