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
        // ReSharper disable once UnusedMember.Global
        // ReSharper disable once UnusedParameter.Global
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
                // The first document 
                new Items
                {
                    Item = "journal", Instock = new List<Instock> {new Instock("A", 5), new Instock("C", 15)}
                },
                new Items
                {
                    Item = "notebook", Instock = new List<Instock> {new Instock("C", 5)}
                },
                new Items
                {
                    Item = "paper", Instock = new List<Instock> {new Instock("A", 60), new Instock("B", 15)}
                },
                new Items
                {
                    Item = "planner", Instock = new List<Instock> {new Instock("A", 40), new Instock("B", 5)}
                },
                new Items
                {
                    Item = "postcard", Instock = new List<Instock> {new Instock("B", 15), new Instock("C", 35)}
                },


            };

            var collItems = SampleConfig.CollItems;
            dbSampleLookup.DropCollection(collItems.CollectionNamespace.CollectionName);
            collItems.InsertMany(documents);


            var filter = await collItems.Aggregate().Unwind<Items, Instock>(x => x.Instock).ToListAsync();
            var test = "";


        }
    }
}
