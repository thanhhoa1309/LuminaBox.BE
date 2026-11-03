using LuminaBox.Application.Interface;

namespace LuminaBox.Infrastructure.Commons
{
    public class CurrentTime : ICurrentTime
    {
        public DateTime GetCurrentTime() => DateTime.UtcNow;
    }
}
