using MongoDB.Driver;
using MongoDB.Driver.Linq;

using SimpleTalkMongoDb.Configuration;

namespace SimpleTalkMongoDb.Modern
{
    /// <summary>
    /// MongoDB 5.2: $top, $topN, $bottom, $maxN and friends pick documents inside a $group by their
    /// own sort order. AggregationSales finds each territory's best customer by sorting all the
    /// customers and keeping the $last; here the group asks for it directly, and for the best three.
    /// </summary>
    internal static class TopN
    {
        public static async Task RunAsync()
        {
            var byBest = Builders<CustomerTotal>.Sort.Descending(x => x.Total);

            var territories = await SampleConfig.Collection.Aggregate()
                // What each customer bought in each territory
                .Group(
                    x => new { x.TerritoryId, x.CustomerId },
                    g => new CustomerTotal { TerritoryId = g.Key.TerritoryId, CustomerId = g.Key.CustomerId, Total = g.Sum(x => x.TotalDue) })
                // Per territory: the best customer, the best three, and the three largest totals alone
                .Group(
                    x => x.TerritoryId,
                    g => new
                    {
                        TerritoryId = g.Key,
                        Best = g.Top(byBest, x => new { x.CustomerId, x.Total }),
                        BestThree = g.TopN(byBest, x => x.CustomerId, 3),
                        LargestTotals = g.MaxN(x => x.Total, 3)
                    })
                .SortByDescending(x => x.Best.Total)
                .ToListAsync();

            ConsoleEx.WriteLine("$top, $topN and $maxN in $group (MongoDB 5.2)", ConsoleColor.Magenta);
            ConsoleEx.WriteLine($"{"Territory",-9} {"Best customer",13} {"Total",14}  {"Best three customers",-22} Largest totals", ConsoleColor.Magenta);
            foreach (var t in territories)
            {
                ConsoleEx.WriteLine(
                    $"{t.TerritoryId,-9} {t.Best.CustomerId,13} {t.Best.Total,14:N2}  {string.Join(", ", t.BestThree),-22} " +
                    string.Join(", ", t.LargestTotals.Select(total => total.ToString("N0"))),
                    ConsoleColor.Yellow);
            }

            ConsoleEx.WriteLine("Territories 1, 5 and 6 show the same best customers as 'aggregation-sales', without its sort and $last.", ConsoleColor.Cyan);
        }
    }
}
