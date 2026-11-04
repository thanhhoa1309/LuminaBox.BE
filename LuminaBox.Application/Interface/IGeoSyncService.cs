using LuminaBox.Application.Dtos.Geography;

namespace LuminaBox.Application.Interface
{
    public interface IGeoSyncService
    {
        Task<GeoSyncReport> SyncAsync(CancellationToken cancellationToken = default);
    }
}
