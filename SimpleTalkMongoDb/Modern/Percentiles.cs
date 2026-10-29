using MongoDB.Driver;
using MongoDB.Driver.Linq;

using SimpleTalkMongoDb.Configuration;

namespace SimpleTalkMongoDb.Modern
{
    /// <summary>
    /// MongoDB 7.0: $median and $percentile, as accumulators in $group (and as window functions).
    /// An average hides how skewed order values are; the median and the 90th and 99th percentiles show it.
    /// </summary>
    internal static class Percentiles
    {
        public static async Task RunAsync()
        {
            var territories = await SampleConfig.Collection.Aggregate()
                .Group(
                    x => x.TerritoryId,
                    g => new
                    {
                        TerritoryId = g.Key,
                        Orders = g.Count(),
                        Average = g.Average(x => x.TotalDue),
                        Median = g.Median(x => x.TotalDue),
                        P90AndP99 = g.Percentile(x => x.TotalDue, new[] { 0.9, 0.99 })
                    })
                .SortBy(x => x.TerritoryId)
                .ToListAsync();

            ConsoleEx.WriteLine("$median and $percentile (MongoDB 7.0): order values per territory", ConsoleColor.Magenta);
            ConsoleEx.WriteLine($"{"Territory",-9} {"Orders",7} {"Average",12} {"Median",12} {"90th",12} {"99th",12}", ConsoleColor.Magenta);
            foreach (var t in territories)
            {
                ConsoleEx.WriteLine(
                    $"{t.TerritoryId,-9} {t.Orders,7} {t.Average,12:N2} {t.Median,12:N2} {t.P90AndP99[0],12:N2} {t.P90AndP99[1],12:N2}",
                    ConsoleColor.Yellow);
            }

            ConsoleEx.WriteLine("In every territory the median order is far below the average: a few large orders pull the average up.", ConsoleColor.Cyan);
            ConsoleEx.WriteLine("The server computes them with the 'approximate' method; the driver asks for it.", ConsoleColor.Cyan);
        }
    }
}
