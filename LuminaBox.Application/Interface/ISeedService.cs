namespace LuminaBox.Application.Interface
{
    public interface ISeedService
    {
        Task SeedAllDataAsync();
        Task ClearAllDataAsync();
    }
}
