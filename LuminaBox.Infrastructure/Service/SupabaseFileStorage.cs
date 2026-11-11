using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using LuminaBox.Application.Commons;
using LuminaBox.Application.Interface;
using LuminaBox.Application.Utils;

namespace LuminaBox.Infrastructure.Service
{
    public class SupabaseFileStorage : IFileStorage
    {
        public const string ClientName = "supabase-storage";

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly SupabaseStorageSettings _settings;
        private readonly ILogger<SupabaseFileStorage> _logger;

        public SupabaseFileStorage(IHttpClientFactory httpClientFactory, IOptions<SupabaseStorageSettings> settings, ILogger<SupabaseFileStorage> logger)
        {
            _httpClientFactory = httpClientFactory;
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task UploadAsync(string bucket, string path, Stream content, string contentType, CancellationToken cancellationToken = default)
        {
            var client = _httpClientFactory.CreateClient(ClientName);

            using var message = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/storage/v1/object/{bucket}/{path}");
            message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _settings.ServiceKey);
            message.Headers.Add("apikey", _settings.ServiceKey);
            message.Headers.Add("x-upsert", "false");
            message.Content = new StreamContent(content);
            message.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType);

            using var response = await client.SendAsync(message, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Supabase upload to {Bucket}/{Path} failed with {Status}: {Body}", bucket, path, (int)response.StatusCode, body);
                throw ErrorHelper.BadGateway("The file could not be stored.", "E20.6");
            }
        }

        public string PublicUrl(string bucket, string path) => $"{BaseUrl}/storage/v1/object/public/{bucket}/{path}";

        private string BaseUrl => _settings.Url.TrimEnd('/');
    }
}
