using MongoDB.Bson;
using MongoDB.Driver;

using SimpleTalkMongoDb.Configuration;

namespace SimpleTalkMongoDb.Modern
{
    /// <summary>
    /// MongoDB 3.6: change streams, a cursor of every insert, update and delete as it happens, that
    /// can be resumed where it left off. MongoDB 6.0: a collection can keep the document as it was
    /// before each change (pre-images), so an update or delete event can carry it.
    /// </summary>
    internal static class ChangeStreams
    {
        private const string CollectionName = "csOffers";
        private static readonly TimeSpan Wait = TimeSpan.FromSeconds(10);

        public static async Task RunAsync()
        {
            await ReplicaSet.EnsureAsync("Change streams");

            var db = SampleConfig.Db;
            await db.DropCollectionAsync(CollectionName);
            await db.CreateCollectionAsync(CollectionName, new CreateCollectionOptions<BsonDocument>
            {
                // 6.0: keep each document's state before a change, for fullDocumentBeforeChange
                ChangeStreamPreAndPostImagesOptions = new ChangeStreamPreAndPostImagesOptions { Enabled = true }
            });
            var offers = db.GetCollection<BsonDocument>(CollectionName);

            var options = new ChangeStreamOptions
            {
                // Post-images (6.0): the document as the change left it. UpdateLookup would read it later,
                // and find nothing once the delete has run
                FullDocument = ChangeStreamFullDocumentOption.WhenAvailable,
                FullDocumentBeforeChange = ChangeStreamFullDocumentBeforeChangeOption.WhenAvailable
            };
            // Only the kinds of change this example writes
            var pipeline = new EmptyPipelineDefinition<ChangeStreamDocument<BsonDocument>>()
                .Match(c => c.OperationType == ChangeStreamOperationType.Insert
                            || c.OperationType == ChangeStreamOperationType.Update
                            || c.OperationType == ChangeStreamOperationType.Delete);

            ConsoleEx.WriteLine("Change streams (MongoDB 3.6) with pre-images (6.0)", ConsoleColor.Magenta);
            var events = new List<ChangeStreamDocument<BsonDocument>>();
            using (var cursor = await offers.WatchAsync(pipeline, options))
            {
                // Another writer, as another application would be; the stream is already open
                var writer = WriteAsync(offers);
                events.AddRange(await ReadAsync(cursor, expected: 3));
                await writer;
            }

            foreach (var e in events)
            {
                ConsoleEx.WriteLine($"{e.OperationType,-7} _id {e.DocumentKey["_id"]}", ConsoleColor.Yellow);
                if (e.FullDocumentBeforeChange is { } before)
                {
                    ConsoleEx.WriteLine($"        before: {before}", ConsoleColor.Gray);
                }

                if (e.FullDocument is { } after)
                {
                    ConsoleEx.WriteLine($"        after:  {after}", ConsoleColor.Gray);
                }
            }
            Console.WriteLine();

            // Resume: a new stream from the first event's token sees the changes after it again,
            // as an application would after a restart, without missing any
            ConsoleEx.WriteLine("Resumed after the first event:", ConsoleColor.Cyan);
            options.StartAfter = events[0].ResumeToken;
            using (var resumed = await offers.WatchAsync(pipeline, options))
            {
                foreach (var e in await ReadAsync(resumed, expected: 2))
                {
                    ConsoleEx.WriteLine($"{e.OperationType,-7} _id {e.DocumentKey["_id"]}", ConsoleColor.Yellow);
                }
            }
        }

        private static async Task WriteAsync(IMongoCollection<BsonDocument> offers)
        {
            await offers.InsertOneAsync(new BsonDocument { { "_id", 1 }, { "description", "Autumn sale" }, { "discountPct", 10 } });
            await offers.UpdateOneAsync(new BsonDocument("_id", 1), new BsonDocument("$set", new BsonDocument("discountPct", 15)));
            await offers.DeleteOneAsync(new BsonDocument("_id", 1));
        }

        private static async Task<List<ChangeStreamDocument<BsonDocument>>> ReadAsync(
            IChangeStreamCursor<ChangeStreamDocument<BsonDocument>> cursor, int expected)
        {
            var events = new List<ChangeStreamDocument<BsonDocument>>();
            using var timeout = new CancellationTokenSource(Wait);
            while (events.Count < expected && await cursor.MoveNextAsync(timeout.Token))
            {
                events.AddRange(cursor.Current);
            }

            return events;
        }
    }
}
