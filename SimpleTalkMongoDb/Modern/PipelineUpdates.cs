using MongoDB.Bson;
using MongoDB.Driver;

using SimpleTalkMongoDb.Configuration;

namespace SimpleTalkMongoDb.Modern
{
    /// <summary>
    /// MongoDB 4.2: an update can be an aggregation pipeline, so a new value can be computed from
    /// other fields of the same document on the server, in one statement, with no read first.
    /// </summary>
    internal static class PipelineUpdates
    {
        private const string CollectionName = "modernOffers";

        public static async Task RunAsync()
        {
            // Work on a copy of the special offers
            var db = SampleConfig.Db;
            await db.DropCollectionAsync(CollectionName);
            var offers = db.GetCollection<BsonDocument>(CollectionName);
            await offers.InsertManyAsync(
                await db.GetCollection<BsonDocument>(SampleConfig.CollSpecialOffer.CollectionNamespace.CollectionName).Find(FilterDefinition<BsonDocument>.Empty).Limit(6).ToListAsync());

            // $set in a pipeline: 'label' joins two fields, 'isDiscount' compares one, and $$NOW is the server's clock
            var setStage = new BsonDocument("$set", new BsonDocument
            {
                { "label", new BsonDocument("$concat", new BsonArray { "$category", ": ", "$description" }) },
                { "isDiscount", new BsonDocument("$ne", new BsonArray { "$offerType", "No Discount" }) },
                { "updatedAt", "$$NOW" }
            });
            // $unset in the same pipeline removes a field
            var unsetStage = new BsonDocument("$unset", "category");

            var update = Builders<BsonDocument>.Update.Pipeline(new BsonDocument[] { setStage, unsetStage });
            var result = await offers.UpdateManyAsync(FilterDefinition<BsonDocument>.Empty, update);

            ConsoleEx.WriteLine("Update with an aggregation pipeline (MongoDB 4.2)", ConsoleColor.Magenta);
            ConsoleEx.WriteLine("The pipeline, as mongosh would write it:", ConsoleColor.Cyan);
            ConsoleEx.WriteLine($"db.{CollectionName}.updateMany({{}}, [{setStage}, {unsetStage}])", ConsoleColor.Gray);
            ConsoleEx.WriteLine($"Matched {result.MatchedCount}, modified {result.ModifiedCount}", ConsoleColor.Cyan);
            Console.WriteLine();

            ConsoleEx.WriteLine($"{"Id",-4} {"Discount",-9} Label", ConsoleColor.Magenta);
            foreach (var doc in await offers.Find(FilterDefinition<BsonDocument>.Empty).Sort(Builders<BsonDocument>.Sort.Ascending("_id")).ToListAsync())
            {
                ConsoleEx.WriteLine($"{doc["_id"],-4} {doc["isDiscount"],-9} {doc["label"]}", ConsoleColor.Yellow);
            }

            ConsoleEx.WriteLine("'category' is gone and every document has the server's 'updatedAt'.", ConsoleColor.Cyan);
        }
    }
}
