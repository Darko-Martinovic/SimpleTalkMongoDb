using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Driver;

using SimpleTalkMongoDb.Configuration;
using SimpleTalkMongoDb.Pocos;

using System;
using System.Threading.Tasks;

namespace SimpleTalkMongoDb.Filtering
{
    internal static class Explain
    {
        public static Task RunAsync() => MainAsync();
        private static async Task MainAsync()
        {

            var collection = SampleConfig.Collection;

            Console.WriteLine("Shows how we could get the query plan from the code");
            Console.WriteLine(" ".PadRight(70,'-'));
            var filter = Builders<SalesHeader>.Filter.Where(x =>
                (x.TerritoryId == 1 ||
                 x.TerritoryId == 2) && x.TotalDue > 1 && x.DueDate >= new DateTime(2014, 1, 1));

            // The $explain query modifier is gone (the driver dropped FindOptions.Modifiers in 3.0),
            // so the query is wrapped in the explain command, as mongosh's explain() does
            var command = new BsonDocument
            {
                {
                    "explain", new BsonDocument
                    {
                        { "find", collection.CollectionNamespace.CollectionName },
                        { "filter", filter.Render(new RenderArgs<SalesHeader>(collection.DocumentSerializer, collection.Settings.SerializerRegistry)) }
                    }
                },
                { "verbosity", "executionStats" }
            };

            var explain = await SampleConfig.Db.RunCommandAsync<BsonDocument>(command);
            var ws = new JsonWriterSettings
            {
                Indent = true,
            };
            Console.WriteLine(explain.ToJson(ws));

        }
    }
}
