using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

using SimpleTalkMongoDb.Configuration;

namespace SimpleTalkMongoDb.Modern
{
    /// <summary>
    /// MongoDB 5.1: $densify adds the missing steps of a sequence (here the days without sales).
    /// MongoDB 5.3: $fill gives the added documents values: a constant, the last value seen, or a
    /// linear interpolation.
    /// </summary>
    internal static class GapFilling
    {
        // Online orders in the United Kingdom, which had none on about half the days of June 2012
        private const int TerritoryId = 10;
        private static readonly DateTime From = SalesCalendar.Utc(2012, 6, 1);
        private static readonly DateTime To = SalesCalendar.Utc(2012, 7, 1);

        public static async Task RunAsync()
        {
            var days = await SampleConfig.Collection.Aggregate()
                .Match(x => x.OnlineOrderFlag && x.TerritoryId == TerritoryId && x.OrderDate >= From && x.OrderDate < To)
                .Group(
                    x => x.OrderDate.Truncate(DateTimeUnit.Day, 1, SalesCalendar.TimeZone),
                    g => new DayTotal { Day = g.Key, Total = g.Sum(x => x.TotalDue), Orders = g.Count() })
                // A document for every day of the month, sales or not. Steps of 24 hours from midnight
                // stay on midnight here, as June has no daylight saving change
                .Densify(x => x.Day, DensifyRange.DateTime(From, To, 1, DensifyDateTimeUnit.Days))
                // The added days have no Orders yet, which marks them; then $fill gives them 0
                .AppendStage<BsonDocument>(new BsonDocument("$set", new BsonDocument("Added", new BsonDocument("$not", new BsonArray { "$Orders" }))))
                .AppendStage<BsonDocument>(new BsonDocument("$fill", new BsonDocument("output", new BsonDocument
                {
                    { "Orders", new BsonDocument("value", 0) },
                    { "Total", new BsonDocument("value", new Decimal128(0)) }
                })))
                .Sort(new BsonDocument("_id", 1))
                .ToListAsync();

            ConsoleEx.WriteLine($"$densify and $fill (MongoDB 5.1 and 5.3): online orders in territory {TerritoryId}, June 2012", ConsoleColor.Magenta);
            foreach (var d in days)
            {
                var added = d["Added"].AsBoolean;
                ConsoleEx.WriteLine(
                    $"{SalesCalendar.Local(d["_id"].ToUniversalTime()):ddd dd}  {d["Orders"].ToInt32(),2} orders  {d["Total"].ToDecimal(),10:N2}{(added ? "  (added by $densify)" : "")}",
                    added ? ConsoleColor.DarkGray : ConsoleColor.Yellow);
            }

            ConsoleEx.WriteLine($"{days.Count(d => !d["Added"].AsBoolean)} days with sales, {days.Count(d => d["Added"].AsBoolean)} added", ConsoleColor.Cyan);
        }
    }
}
