namespace LuminaBox.Application.Interface
{
    public interface ICurrentTime
    {
        /// <summary>Returns the current time in UTC (Npgsql timestamptz only accepts UTC).</summary>
        DateTime GetCurrentTime();
    }
}
