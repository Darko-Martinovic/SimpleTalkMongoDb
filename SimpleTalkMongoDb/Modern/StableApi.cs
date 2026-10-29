using MongoDB.Bson;
using MongoDB.Driver;

using SimpleTalkMongoDb.Configuration;

namespace SimpleTalkMongoDb.Modern
{
    /// <summary>
    /// MongoDB 5.0: the Stable API. A client declares the API version it was written for, and the
    /// server keeps that behaviour across upgrades. With strict on, commands outside the version
    /// are refused, so code can't come to depend on them by accident.
    /// </summary>
    internal static class StableApi
    {
        public static async Task RunAsync()
        {
            var settings = MongoClientSettings.FromConnectionString(SampleConfig.MongoConnectionString);
            settings.ServerApi = new ServerApi(ServerApiVersion.V1, strict: true, deprecationErrors: true);
            var client = new MongoClient(settings);
            var sales = client.GetDatabase(SampleConfig.Db.DatabaseNamespace.DatabaseName)
                .GetCollection<BsonDocument>(SampleConfig.Collection.CollectionNamespace.CollectionName);

            ConsoleEx.WriteLine("A client on Stable API version 1, strict (MongoDB 5.0)", ConsoleColor.Magenta);

            // find and aggregate are part of version 1
            var count = await sales.CountDocumentsAsync(new BsonDocument("TerritoryId", 1));
            ConsoleEx.WriteLine($"countDocuments (an aggregation) works: {count} orders in territory 1", ConsoleColor.Yellow);

            // distinct is not, so a strict client is refused
            try
            {
                await sales.DistinctAsync<int>("TerritoryId", FilterDefinition<BsonDocument>.Empty);
            }
            catch (MongoCommandException e)
            {
                ConsoleEx.WriteLine($"distinct is refused: {e.CodeName} ({e.Code}): {e.ErrorMessage}", ConsoleColor.Red);
            }

            // The same answer within version 1: an aggregation that groups by territory
            var territories = await sales.Aggregate()
                .Group(new BsonDocument("_id", "$TerritoryId"))
                .Sort(new BsonDocument("_id", 1))
                .ToListAsync();
            ConsoleEx.WriteLine($"The territories with $group instead: {string.Join(", ", territories.Select(t => t["_id"]))}", ConsoleColor.Yellow);
        }
    }
}
