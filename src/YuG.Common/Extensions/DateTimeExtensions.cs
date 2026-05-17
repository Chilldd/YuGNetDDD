namespace YuG.Common.Extensions;

public static class DateTimeExtensions
{
    public static DateTime ToUtc(this DateTime dateTime) =>
        dateTime.Kind switch
        {
            DateTimeKind.Utc => dateTime,
            DateTimeKind.Local => dateTime.ToUniversalTime(),
            _ => DateTime.SpecifyKind(dateTime, DateTimeKind.Utc)
        };

    public static long ToUnixTimestamp(this DateTime dateTime) =>
        new DateTimeOffset(dateTime.ToUtc()).ToUnixTimeSeconds();

    public static DateTime StartOfDay(this DateTime dateTime) =>
        new(dateTime.Year, dateTime.Month, dateTime.Day, 0, 0, 0, dateTime.Kind);

    public static DateTime EndOfDay(this DateTime dateTime) =>
        new(dateTime.Year, dateTime.Month, dateTime.Day, 23, 59, 59, 999, dateTime.Kind);

    public static DateTime StartOfMonth(this DateTime dateTime) =>
        new(dateTime.Year, dateTime.Month, 1, 0, 0, 0, dateTime.Kind);

    public static DateTime EndOfMonth(this DateTime dateTime) =>
        new(dateTime.Year, dateTime.Month, DateTime.DaysInMonth(dateTime.Year, dateTime.Month), 23, 59, 59, 999, dateTime.Kind);

    public static int Age(this DateTime birthDate)
    {
        var today = DateTime.Today;
        var age = today.Year - birthDate.Year;
        if (birthDate.Date > today.AddYears(-age)) age--;
        return age;
    }
}
