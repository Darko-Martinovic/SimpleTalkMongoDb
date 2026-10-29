using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

using SimpleTalkMongoDb.Configuration;

namespace SimpleTalkMongoDb.Modern
{
    /// <summary>
    /// MongoDB 5.0: a time series collection stores measurements (a time, a metadata field and
    /// values) in buckets per metadata value and time span, compressed, while the collection still
    /// reads like any other. MongoDB 6.0 added secondary indexes on the measurements.
    /// </summary>
    internal static class TimeSeries
    {
        private const string CollectionName = "salesTimeSeries";

        public static async Task RunAsync()
        {
            var db = SampleConfig.Db;
            await db.DropCollectionAsync(CollectionName);
            await db.CreateCollectionAsync(CollectionName, new CreateCollectionOptions
            {
                TimeSeriesOptions = new TimeSeriesOptions(
                    timeField: nameof(SalesMeasurement.OrderDate),
                    metaField: nameof(SalesMeasurement.TerritoryId),
                    granularity: TimeSeriesGranularity.Hours)
            });
            var measurements = db.GetCollection<SalesMeasurement>(CollectionName);

            // Every order becomes a measurement: when, where and how much
            var orders = await SampleConfig.Collection.Find(FilterDefinition<Pocos.SalesHeader>.Empty)
                .Project(x => new SalesMeasurement { OrderDate = x.OrderDate, TerritoryId = x.TerritoryId, TotalDue = x.TotalDue })
                .ToListAsync();
            await measurements.InsertManyAsync(orders);

            // 6.0: a secondary index on the metadata and the time
            await measurements.Indexes.CreateOneAsync(new CreateIndexModel<SalesMeasurement>(
                Builders<SalesMeasurement>.IndexKeys.Ascending(x => x.TerritoryId).Ascending(x => x.OrderDate)));

            ConsoleEx.WriteLine("A time series collection (MongoDB 5.0)", ConsoleColor.Magenta);
            var info = await (await db.ListCollectionsAsync(new ListCollectionsOptions
            {
                Filter = new BsonDocument("name", CollectionName)
            })).FirstAsync();
            ConsoleEx.WriteLine($"type: {info["type"]}, options: {info["options"]["timeseries"]}", ConsoleColor.Cyan);

            // The measurements are stored in buckets: one document per territory and time span
            var buckets = await db.GetCollection<BsonDocument>($"system.buckets.{CollectionName}")
                .CountDocumentsAsync(FilterDefinition<BsonDocument>.Empty);
            ConsoleEx.WriteLine($"{orders.Count} measurements are stored in {buckets} buckets", ConsoleColor.Cyan);
            Console.WriteLine();

            // Queried like any collection: monthly sales of territory 1 in the first half of 2014
            var months = await measurements.Aggregate()
                .Match(x => x.TerritoryId == 1 && x.OrderDate >= SalesCalendar.Utc(2014, 1, 1) && x.OrderDate < SalesCalendar.Utc(2014, 7, 1))
                .Group(
                    x => x.OrderDate.Truncate(DateTimeUnit.Month, 1, SalesCalendar.TimeZone),
                    g => new { Month = g.Key, Orders = g.Count(), Total = g.Sum(x => x.TotalDue) })
                .SortBy(x => x.Month)
                .ToListAsync();

            ConsoleEx.WriteLine("Territory 1, first half of 2014:", ConsoleColor.Cyan);
            foreach (var m in months)
            {
                ConsoleEx.WriteLine($"{SalesCalendar.Local(m.Month):yyyy-MM}  {m.Orders,4} orders  {m.Total,14:N2}", ConsoleColor.Yellow);
            }
        }
    }
}
