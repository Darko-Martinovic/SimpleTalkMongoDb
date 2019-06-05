using System;
using System.Threading.Tasks;

using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using SimpleTalkMongoDb.Configuration;
using SimpleTalkMongoDb.Pocos;

namespace SimpleTalkMongoDb.Filtering
{
    internal static class FilterForNullOrMissingFields
    {
        // Query for Null or Missing Fields. Based on MongoDB example 
        // https://docs.mongodb.com/manual/tutorial/query-for-null-fields/
        // Use BsonNull.Value with the MongoDB C# driver to query for null and C# null ( VB.NET Nothing ) for missing fields in MongoDB.
        public static void Main(string[] args)
        {
            MainAsync().Wait();
            Console.ReadLine();
        }

        private static async Task MainAsync()
        {
            var documents = new[]
            {
                new Tester {Id = 1, Item = BsonNull.Value},
                new Tester {Id = 2}
            };

            var collItems = SampleConfig.CollNullNonExistingElements;
            SampleConfig.DbSampleLookup.DropCollection(collItems.CollectionNamespace.CollectionName);
            collItems.InsertMany(documents);


            // Find items with NULL value
            await UsingIFindFluent(collItems);
            await UsingLinq(collItems);
            // Find documents with non-existing items
            await UsingIFindFluentNonExisting(collItems);
            await UsingLinqNonExisting(collItems);

        }

        private static async Task UsingIFindFluent(IMongoCollection<Tester> collItems)
        {
            var list = await (collItems.Find(x => x.Item.Equals(BsonNull.Value))).ToListAsync();

            ConsoleEx.WriteLine("Using IFindFluent", ConsoleColor.DarkYellow);
            Console.WriteLine();
            ConsoleEx.WriteLine("Find all items with NULL values", ConsoleColor.Magenta);
            ConsoleEx.WriteLine("------------------------------------------------------", ConsoleColor.DarkYellow);
            foreach (var e in list)
            {
                ConsoleEx.WriteLine(e.Id.ToString(), ConsoleColor.Blue);
            }
            Console.WriteLine();


        }

        private static async Task UsingLinq(IMongoCollection<Tester> collItems)
        {
            var list = await (collItems.AsQueryable().Where(x => x.Item.Equals(BsonNull.Value))).ToListAsync();

            ConsoleEx.WriteLine("Using MongoDB Linq", ConsoleColor.DarkYellow);
            Console.WriteLine();
            ConsoleEx.WriteLine("Find all items with NULL values", ConsoleColor.Magenta);
            ConsoleEx.WriteLine("------------------------------------------------------", ConsoleColor.DarkYellow);
            foreach (var e in list)
            {
                ConsoleEx.WriteLine(e.Id.ToString(), ConsoleColor.Blue);
            }
            Console.WriteLine();

        }

        private static async Task UsingIFindFluentNonExisting(IMongoCollection<Tester> collItems)
        {
            var list = await (collItems.Find(x => x.Item == null)).ToListAsync();

            ConsoleEx.WriteLine("Using IFindFluent", ConsoleColor.DarkYellow);
            Console.WriteLine();
            ConsoleEx.WriteLine("Find all documents in which Items does not exists", ConsoleColor.Magenta);
            ConsoleEx.WriteLine("------------------------------------------------------", ConsoleColor.DarkYellow);
            foreach (var e in list)
            {
                ConsoleEx.WriteLine(e.Id.ToString(), ConsoleColor.Blue);
            }
            Console.WriteLine();

        }

        private static async Task UsingLinqNonExisting(IMongoCollection<Tester> collItems)
        {
            var list = await (collItems.AsQueryable().Where(x => x.Item == null)).ToListAsync();

            ConsoleEx.WriteLine("Using MongoDB Linq", ConsoleColor.DarkYellow);
            Console.WriteLine();
            ConsoleEx.WriteLine("Find all documents in which Items does not exists", ConsoleColor.Magenta);
            ConsoleEx.WriteLine("------------------------------------------------------", ConsoleColor.DarkYellow);
            foreach (var e in list)
            {
                ConsoleEx.WriteLine(e.Id.ToString(), ConsoleColor.Blue);
            }
            Console.WriteLine();

        }


    }
    }
