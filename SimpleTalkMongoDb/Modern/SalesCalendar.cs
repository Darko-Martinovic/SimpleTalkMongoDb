namespace SimpleTalkMongoDb.Modern
{
    /// <summary>
    /// The orders' dates are midnight in Central Europe, where the JSON files were exported, stored
    /// in UTC as 22:00 or 23:00 of the day before. Days, months and years are therefore counted in
    /// that time zone ($dateTrunc and friends take one since MongoDB 5.0), or an order of the 1st
    /// would fall into the previous month.
    /// </summary>
    internal static class SalesCalendar
    {
        public const string TimeZone = "Europe/Belgrade";

        private static readonly TimeZoneInfo Zone = TimeZoneInfo.FindSystemTimeZoneById(TimeZone);

        /// <summary>Midnight of a day in the sales' time zone, as the UTC instant MongoDB stores.</summary>
        public static DateTime Utc(int year, int month, int day = 1) =>
            TimeZoneInfo.ConvertTimeToUtc(new DateTime(year, month, day, 0, 0, 0, DateTimeKind.Unspecified), Zone);

        /// <summary>A stored UTC instant as a date in the sales' time zone.</summary>
        public static DateTime Local(DateTime utc) =>
            TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc, DateTimeKind.Utc), Zone);
    }
}
