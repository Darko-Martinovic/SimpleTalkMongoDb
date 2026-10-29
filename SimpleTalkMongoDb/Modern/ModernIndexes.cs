using MongoDB.Bson;
using MongoDB.Driver;

using SimpleTalkMongoDb.Configuration;
using SimpleTalkMongoDb.Pocos;

namespace SimpleTalkMongoDb.Modern
{
    /// <summary>
    /// Index kinds added since MongoDB 4.0: wildcard (4.2), hidden (4.4), compound wildcard (7.0),
    /// with partial and TTL indexes, and clustered collections (5.3), which are stored in the
    /// order of their clustered key instead of having a separate _id index.
    /// </summary>
    internal static class ModernIndexes
    {
        private const string CollectionName = "salesIndexed";

        public static async Task RunAsync()
        {
            // A copy of the orders, so the other examples' query plans don't change
            var db = SampleConfig.Db;
            await SampleConfig.Collection.Aggregate().OutAsync(db.GetCollection<SalesHeader>(CollectionName));
            var sales = db.GetCollection<BsonDocument>(CollectionName);
            var keys = Builders<BsonDocument>.IndexKeys;

            ConsoleEx.WriteLine("Indexes added since MongoDB 4.0", ConsoleColor.Magenta);
            await sales.Indexes.CreateManyAsync(new[]
            {
                // 4.2: every field under Details, whatever its name, without listing them
                new CreateIndexModel<BsonDocument>(keys.Wildcard("Details"), new CreateIndexOptions { Name = "wildcard_details" }),
                // 7.0: a compound wildcard index, a known field plus every field under Details
                new CreateIndexModel<BsonDocument>(
                    // (the builders have no compound wildcard yet, so the keys are written as a document)
                    new BsonDocument { { "TerritoryId", 1 }, { "Details.$**", 1 } }, new CreateIndexOptions { Name = "territory_wildcard_details" }),
                // Partial: only the large orders are in the index, so it stays small
                new CreateIndexModel<BsonDocument>(
                    keys.Descending("TotalDue"),
                    new CreateIndexOptions<BsonDocument>
                    {
                        Name = "large_orders",
                        PartialFilterExpression = Builders<BsonDocument>.Filter.Gt("TotalDue", new Decimal128(100000))
                    })
            });

            // Which index answers a query, as explain shows it
            var byProduct = new BsonDocument("Details.ProductId", 776);
            var byTerritoryAndProduct = new BsonDocument { { "TerritoryId", 4 }, { "Details.ProductId", 776 } };
            var largeOrders = new BsonDocument("TotalDue", new BsonDocument("$gt", new Decimal128(150000)));
            await ShowPlanAsync("Orders with product 776", byProduct);
            await ShowPlanAsync("Orders in territory 4 with product 776", byTerritoryAndProduct);
            await ShowPlanAsync("Orders over 150,000", largeOrders);

            // 4.4: hide an index to see how queries do without it, without dropping and rebuilding it
            await HideAsync("large_orders", hidden: true);
            await ShowPlanAsync("Orders over 150,000, index hidden", largeOrders);
            await HideAsync("large_orders", hidden: false);
            Console.WriteLine();

            await TtlAndClusteredAsync(db);
        }

        private static async Task TtlAndClusteredAsync(IMongoDatabase db)
        {
            // TTL: the server deletes each session once 'expiresAt' has passed (it checks every minute)
            await db.DropCollectionAsync("sessions");
            var sessions = db.GetCollection<BsonDocument>("sessions");
            await sessions.Indexes.CreateOneAsync(new CreateIndexModel<BsonDocument>(
                Builders<BsonDocument>.IndexKeys.Ascending("expiresAt"),
                new CreateIndexOptions { Name = "expire_sessions", ExpireAfter = TimeSpan.Zero }));
            ConsoleEx.WriteLine("TTL index 'expire_sessions' on sessions.expiresAt: documents go once their time has passed", ConsoleColor.Cyan);

            // 5.3: a clustered collection, stored in _id order; good for range reads on _id
            await db.DropCollectionAsync("ordersByNumber");
            await db.CreateCollectionAsync("ordersByNumber", new CreateCollectionOptions<BsonDocument>
            {
                ClusteredIndex = new ClusteredIndexOptions<BsonDocument>()
            });
            var info = await (await db.ListCollectionsAsync(new ListCollectionsOptions
            {
                Filter = new BsonDocument("name", "ordersByNumber")
            })).FirstAsync();
            ConsoleEx.WriteLine($"Clustered collection 'ordersByNumber' (MongoDB 5.3): {info["options"]["clusteredIndex"]}", ConsoleColor.Cyan);
        }

        private static async Task ShowPlanAsync(string title, BsonDocument filter)
        {
            // Read lazily: a compound wildcard plan repeats '$_path' in its key pattern, which a BsonDocument refuses
            var explain = await SampleConfig.Db.RunCommandAsync<RawBsonDocument>(new BsonDocument
            {
                { "explain", new BsonDocument { { "find", CollectionName }, { "filter", filter } } },
                { "verbosity", "executionStats" }
            });
            var plan = explain["queryPlanner"]["winningPlan"].AsBsonDocument;
            var stats = explain["executionStats"];
            ConsoleEx.WriteLine(
                $"{title,-42} {Stages(plan),-30} {IndexName(plan),-28} examined {stats["totalDocsExamined"],6}, returned {stats["nReturned"],5}",
                ConsoleColor.Yellow);
        }

        // The stages from the top of the plan down, for example FETCH > IXSCAN
        private static string Stages(BsonDocument plan) =>
            plan.Contains("inputStage") ? $"{plan["stage"]} > {Stages(plan["inputStage"].AsBsonDocument)}" : plan["stage"].AsString;

        private static string IndexName(BsonDocument plan) =>
            plan.Contains("indexName") ? plan["indexName"].AsString
            : plan.Contains("inputStage") ? IndexName(plan["inputStage"].AsBsonDocument)
            : "(no index)";

        private static Task HideAsync(string index, bool hidden) =>
            SampleConfig.Db.RunCommandAsync<BsonDocument>(new BsonDocument
            {
                { "collMod", CollectionName },
                { "index", new BsonDocument { { "name", index }, { "hidden", hidden } } }
            });
    }
}
