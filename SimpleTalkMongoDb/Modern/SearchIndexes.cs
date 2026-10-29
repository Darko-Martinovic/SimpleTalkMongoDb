using MongoDB.Bson;
using MongoDB.Driver;

namespace SimpleTalkMongoDb.Modern
{
    /// <summary>
    /// Atlas Search and Vector Search indexes are built by a search process next to mongod
    /// (mongot), in MongoDB Atlas or the mongodb-atlas-local container. They are created like other
    /// indexes but build in the background, so a query has to wait until the index is queryable.
    /// </summary>
    internal static class SearchIndexes
    {
        private static readonly TimeSpan Timeout = TimeSpan.FromMinutes(2);

        /// <summary>Creates the index unless it exists, and waits until it can be queried.</summary>
        public static async Task EnsureAsync<T>(IMongoCollection<T> collection, string name, SearchIndexType type, BsonDocument definition)
        {
            try
            {
                var existing = await (await collection.SearchIndexes.ListAsync(name)).ToListAsync();
                if (existing.Count == 0)
                {
                    ConsoleEx.WriteLine($"Creating {type} index '{name}' on '{collection.CollectionNamespace.CollectionName}'...", ConsoleColor.Cyan);
                    await collection.SearchIndexes.CreateOneAsync(new CreateSearchIndexModel(name, type, definition));
                }
            }
            catch (MongoCommandException e)
            {
                throw new InvalidOperationException(
                    "Atlas Search and Vector Search need MongoDB Atlas or the mongodb-atlas-local container " +
                    $"(compose.yaml); this MongoDB has no search process. ({e.CodeName}: {e.ErrorMessage})", e);
            }

            var waited = System.Diagnostics.Stopwatch.StartNew();
            while (true)
            {
                var index = (await (await collection.SearchIndexes.ListAsync(name)).ToListAsync()).Single();
                if (index.GetValue("queryable", false).ToBoolean())
                {
                    return;
                }

                if (waited.Elapsed > Timeout)
                {
                    throw new TimeoutException($"Search index '{name}' isn't queryable after {Timeout.TotalMinutes} minutes: {index}");
                }

                await Task.Delay(TimeSpan.FromSeconds(1));
            }
        }
    }
}
