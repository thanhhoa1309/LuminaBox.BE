using LuminaBox.Application.Dtos.Upload;

namespace LuminaBox.Application.Interface
{
    public interface IUploadService
    {
        Task<UploadResponse> UploadAsync(UploadPurpose purpose, UploadFile file, IReadOnlyCollection<string> roles, CancellationToken cancellationToken = default);
    }

    public interface IFileStorage
    {
        Task UploadAsync(string bucket, string path, Stream content, string contentType, CancellationToken cancellationToken = default);
        string PublicUrl(string bucket, string path);
    }
}
