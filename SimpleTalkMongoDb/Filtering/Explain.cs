using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Driver;

using SimpleTalkMongoDb.Configuration;

using System;
using System.Threading.Tasks;

namespace SimpleTalkMongoDb.Filtering
{
    internal static class Explain
    {
        public static void Main(string[] args)
        {
            MainAsync().Wait();
            Console.WriteLine("Press any key to exit!");
            Console.ReadLine();
        }
        private static async Task MainAsync()
        {

            var collection = SampleConfig.Collection;

            Console.WriteLine("Shows how we could get the query plan from the code");
            Console.WriteLine(" ".PadRight(70,'-'));
            var options = new FindOptions
            {
                Modifiers = new BsonDocument("$explain", true)
            };

            var explain = await collection.Find(x =>
                    (x.TerritoryId == 1 ||
                     x.TerritoryId == 2) && x.TotalDue > 1 && x.DueDate >= new DateTime(2014, 1, 1), options)
                .Project(new BsonDocument())
                .FirstOrDefaultAsync();
            var ws = new JsonWriterSettings
            {
                Indent = true,
            };
            Console.WriteLine(explain.ToJson(ws));

        }
    }
}
