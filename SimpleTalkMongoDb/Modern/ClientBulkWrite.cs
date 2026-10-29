using MongoDB.Bson;
using MongoDB.Driver;

using SimpleTalkMongoDb.Configuration;

namespace SimpleTalkMongoDb.Modern
{
    /// <summary>
    /// MongoDB 8.0: one bulk write from the client across several collections (and databases).
    /// Before, BulkWrite belonged to a collection, so writes to two collections took two calls.
    /// </summary>
    internal static class ClientBulkWrite
    {
        public static async Task RunAsync()
        {
            var db = SampleConfig.Db;
            await db.DropCollectionAsync("bulkOffers");
            await db.DropCollectionAsync("bulkAudit");
            var offers = new CollectionNamespace(db.DatabaseNamespace, "bulkOffers");
            var audit = new CollectionNamespace(db.DatabaseNamespace, "bulkAudit");

            BulkWriteModel[] writes =
            [
                new BulkWriteInsertOneModel<BsonDocument>(offers, new BsonDocument { { "_id", 100 }, { "description", "Winter sale" }, { "discountPct", 10 } }),
                new BulkWriteInsertOneModel<BsonDocument>(offers, new BsonDocument { { "_id", 101 }, { "description", "Spring sale" }, { "discountPct", 5 } }),
                new BulkWriteUpdateOneModel<BsonDocument>(offers, new BsonDocument("_id", 100), new BsonDocument("$set", new BsonDocument("discountPct", 20))),
                new BulkWriteDeleteOneModel<BsonDocument>(offers, new BsonDocument("_id", 101)),
                // The audit trail goes to another collection, in the same request
                new BulkWriteInsertOneModel<BsonDocument>(audit, new BsonDocument { { "action", "offers updated" }, { "at", DateTime.UtcNow } })
            ];

            var result = await SampleConfig.Client.BulkWriteAsync(writes, new ClientBulkWriteOptions
            {
                IsOrdered = true,
                VerboseResult = true
            });

            ConsoleEx.WriteLine("A client bulk write across two collections (MongoDB 8.0)", ConsoleColor.Magenta);
            ConsoleEx.WriteLine(
                $"inserted {result.InsertedCount}, matched {result.MatchedCount}, modified {result.ModifiedCount}, deleted {result.DeletedCount}",
                ConsoleColor.Cyan);
            if (result.InsertResults is { } inserts)
            {
                foreach (var (index, insert) in inserts.OrderBy(i => i.Key))
                {
                    ConsoleEx.WriteLine($"  write {index}: inserted _id {insert.DocumentId}", ConsoleColor.Gray);
                }
            }

            foreach (var ns in new[] { offers, audit })
            {
                var docs = await db.GetCollection<BsonDocument>(ns.CollectionName).Find(FilterDefinition<BsonDocument>.Empty).ToListAsync();
                ConsoleEx.WriteLine($"{ns.CollectionName}: {string.Join(", ", docs.Select(d => d.ToJson()))}", ConsoleColor.Yellow);
            }
        }
    }
}
