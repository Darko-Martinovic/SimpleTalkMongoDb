using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using SimpleTalkMongoDb.Configuration;
using SimpleTalkMongoDb.Pocos;

namespace SimpleTalkMongoDb.Filtering
{
    internal static class FilterDetailTest
    {
        // Query for a Document Nested in an Array. Based on MongoDB example 
        // https://docs.mongodb.com/manual/tutorial/query-array-of-documents/
        // Find documents that have WareHouse equal to A and the quantity equal 5
        public static void Main(string[] args)
        {
            MainAsync().Wait();
            Console.ReadLine();
        }

        private static async Task MainAsync()
        {
            await FilterDetail(SampleConfig.DbSampleLookup);

        }

        private static async Task FilterDetail(IMongoDatabase dbSampleLookup)
        {
            var documents = new[]
            {
                //
                // Creating a couple of documents. Note the concrete data is a little bit different than in MongoDB example
                //
                new Items
                {
                    Item = "journal", Details = new List<Instock> {new Instock("A", 5), new Instock("C", 15)}
                },
                new Items
                {
                    Item = "notebook", Details = new List<Instock> {new Instock("C", 5)}
                },
                new Items
                {
                    Item = "paper", Details = new List<Instock> {new Instock("A", 60), new Instock("B", 15)}
                },
                new Items
                {
                    Item = "planner", Details = new List<Instock> {new Instock("A", 40), new Instock("B", 5)}
                },
                new Items
                {
                    Item = "postcard", Details = new List<Instock> {new Instock("B", 15), new Instock("A", 5)}
                },


            };

            var collItems = SampleConfig.CollItems;
            dbSampleLookup.DropCollection(collItems.CollectionNamespace.CollectionName);
            collItems.InsertMany(documents);


           


            var list = await collItems.Aggregate().Unwind<Items, InStockHelper>(x => x.Details)
                .Project(x => new {x.Item, x.Details.WareHouse, x.Details.Qty})
                .Match(x => x.WareHouse == "A" && x.Qty == 5)
                .Project(x => new {x.Item}).ToListAsync();

            ConsoleEx.WriteLine("Using MongoDB aggregation framework UnWind + Match", ConsoleColor.DarkYellow);
            Console.WriteLine();
            ConsoleEx.WriteLine("Find out all items in WareHouse 'A' with quantity 5", ConsoleColor.Magenta);
            ConsoleEx.WriteLine("------------------------------------------------------", ConsoleColor.DarkYellow);
            foreach (var e in list)
            {
               ConsoleEx.WriteLine(e.Item,ConsoleColor.Blue );
            }


            Console.WriteLine();


        }
    }
}
