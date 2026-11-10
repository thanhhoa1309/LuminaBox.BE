namespace LuminaBox.Application.Commons
{
    public static class RentalRules
    {
        public const int MinDays = 7;

        public static bool IsLongEnough(DateOnly? startDate, DateOnly? endDate) =>
            startDate.HasValue && endDate.HasValue && endDate.Value.DayNumber - startDate.Value.DayNumber + 1 >= MinDays;
    }
}
