using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

using SimpleTalkMongoDb.Configuration;

namespace SimpleTalkMongoDb.Modern
{
    /// <summary>
    /// MongoDB 5.0: $setWindowFields computes over a window of neighbouring documents, like SQL's
    /// OVER (PARTITION BY ... ORDER BY ...): running totals, moving averages, ranks.
    /// </summary>
    internal static class WindowFunctions
    {
        private const int Territory = 4;

        public static async Task RunAsync()
        {
            var rows = await SampleConfig.Collection.Aggregate()
                // One row per territory and month
                .Group(
                    x => new { x.TerritoryId, Month = x.OrderDate.Truncate(DateTimeUnit.Month, 1, SalesCalendar.TimeZone) },
                    g => new MonthTotal { TerritoryId = g.Key.TerritoryId, Month = g.Key.Month, Total = g.Sum(x => x.TotalDue) })
                // Per territory, in month order: the total so far, a three-month moving average and a rank
                .SetWindowFields(
                    partitionBy: x => x.TerritoryId,
                    sortBy: Builders<MonthTotal>.Sort.Ascending(x => x.Month),
                    output: p => new
                    {
                        RunningTotal = p.Sum(x => x.Total, DocumentsWindow.Create(DocumentsWindow.Unbounded, DocumentsWindow.Current)),
                        MovingAverage = p.Average(x => x.Total, DocumentsWindow.Create(-2, 0)),
                        MonthNumber = p.DocumentNumber()
                    })
                .Match(new BsonDocument { { "TerritoryId", Territory }, { "Month", new BsonDocument("$gte", SalesCalendar.Utc(2013, 1, 1)) } })
                .Sort(new BsonDocument("Month", 1))
                .Limit(12)
                .ToListAsync();

            ConsoleEx.WriteLine($"$setWindowFields (MongoDB 5.0): territory {Territory} in 2013", ConsoleColor.Magenta);
            ConsoleEx.WriteLine($"{"Month",-8} {"#",3} {"Sales",14} {"Running total",16} {"3-month average",16}", ConsoleColor.Magenta);
            foreach (var r in rows)
            {
                ConsoleEx.WriteLine(
                    $"{SalesCalendar.Local(r["Month"].ToUniversalTime()):yyyy-MM} {r["MonthNumber"].ToInt32(),3} {r["Total"].ToDecimal(),14:N2} " +
                    $"{r["RunningTotal"].ToDecimal(),16:N2} {r["MovingAverage"].ToDecimal(),16:N2}", ConsoleColor.Yellow);
            }

            ConsoleEx.WriteLine("The running total starts with the territory's first month (2011), not with 2013:", ConsoleColor.Cyan);
            ConsoleEx.WriteLine("the window sees every earlier document in the partition, the $match only picks rows to show.", ConsoleColor.Cyan);
        }
    }
}
