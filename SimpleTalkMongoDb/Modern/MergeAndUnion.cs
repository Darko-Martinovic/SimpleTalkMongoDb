using MongoDB.Driver;
using MongoDB.Driver.Linq;

using SimpleTalkMongoDb.Configuration;
using SimpleTalkMongoDb.Pocos;

namespace SimpleTalkMongoDb.Modern
{
    /// <summary>
    /// MongoDB 4.2: $merge writes a pipeline's result into a collection, inserting or replacing
    /// document by document, so it can keep a summary (an on-demand materialized view) up to date.
    /// MongoDB 4.4: $unionWith adds another collection's documents to the pipeline, like UNION ALL.
    /// </summary>
    internal static class MergeAndUnion
    {
        public static async Task RunAsync()
        {
            await MergeAsync();
            Console.WriteLine();
            await UnionWithAsync();
        }

        private static async Task MergeAsync()
        {
            var db = SampleConfig.Db;
            await db.DropCollectionAsync("territoryMonthlySales");
            var monthly = db.GetCollection<MonthlySales>("territoryMonthlySales");

            ConsoleEx.WriteLine("$merge into an on-demand materialized view (MongoDB 4.2)", ConsoleColor.Magenta);

            // First the orders up to 2013, then all of them: the second run replaces the months it
            // recalculates and inserts the new ones, unlike $out, which replaces the whole collection
            foreach (var until in new[] { SalesCalendar.Utc(2014, 1, 1), DateTime.MaxValue })
            {
                await SampleConfig.Collection.Aggregate()
                    .Match(x => x.OrderDate < until)
                    .Group(
                        x => new TerritoryMonth { TerritoryId = x.TerritoryId, Month = x.OrderDate.Truncate(DateTimeUnit.Month, 1, SalesCalendar.TimeZone) },
                        g => new MonthlySales { Id = g.Key, Total = g.Sum(x => x.TotalDue), Orders = g.Count() })
                    .MergeAsync(monthly, new MergeStageOptions<MonthlySales>
                    {
                        WhenMatched = MergeStageWhenMatched.Replace,
                        WhenNotMatched = MergeStageWhenNotMatched.Insert
                    });

                var label = until == DateTime.MaxValue ? "all orders" : $"orders before {SalesCalendar.Local(until):yyyy}";
                ConsoleEx.WriteLine($"After merging {label}: {await monthly.CountDocumentsAsync(FilterDefinition<MonthlySales>.Empty)} territory-months", ConsoleColor.Cyan);
            }

            ConsoleEx.WriteLine("The three best territory-months:", ConsoleColor.Cyan);
            foreach (var m in await monthly.Find(FilterDefinition<MonthlySales>.Empty).SortByDescending(m => m.Total).Limit(3).ToListAsync())
            {
                ConsoleEx.WriteLine($"Territory {m.Id.TerritoryId,2}  {SalesCalendar.Local(m.Id.Month):yyyy-MM}  {m.Orders,4} orders  {m.Total,14:N2}", ConsoleColor.Yellow);
            }
        }

        private static async Task UnionWithAsync()
        {
            var db = SampleConfig.Db;
            ConsoleEx.WriteLine("$unionWith: two collections in one pipeline (MongoDB 4.4)", ConsoleColor.Magenta);

            // Split the orders into a collection per year, as an archive often is
            var years = new[] { 2012, 2013 };
            foreach (var year in years)
            {
                await SampleConfig.Collection.Aggregate()
                    .Match(x => x.OrderDate >= SalesCalendar.Utc(year, 1, 1) && x.OrderDate < SalesCalendar.Utc(year + 1, 1, 1))
                    .OutAsync(db.GetCollection<SalesHeader>($"sales{year}"));
            }

            // Then read both as one: the orders of 2012 followed by those of 2013, grouped by year
            var perYear = await db.GetCollection<SalesHeader>("sales2012").Aggregate()
                .UnionWith(db.GetCollection<SalesHeader>("sales2013"))
                .Group(x => x.OrderDate.Truncate(DateTimeUnit.Year, 1, SalesCalendar.TimeZone), g => new { Year = g.Key, Orders = g.Count(), Total = g.Sum(x => x.TotalDue) })
                .SortBy(x => x.Year)
                .ToListAsync();

            foreach (var y in perYear)
            {
                ConsoleEx.WriteLine($"{SalesCalendar.Local(y.Year):yyyy}  {y.Orders,5} orders  {y.Total,16:N2}", ConsoleColor.Yellow);
            }
        }
    }
}
