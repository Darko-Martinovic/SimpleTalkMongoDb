using System;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using SimpleTalkMongoDb.Configuration;
using SimpleTalkMongoDb.Pocos;

namespace SimpleTalkMongoDb.Filtering
{
    internal static class FilterNestedDocuments
    {
        // Query on Embedded/Nested Documents. Based on MongoDB example 
        // https://docs.mongodb.com/manual/tutorial/query-embedded-documents/
        //  An example of query operations on embedded/nested documents using the MongoCollection.Find()
        public static void Main(string[] args)
        {
            MainAsync().Wait();
            Console.ReadLine();
        }

        private static async Task MainAsync()
        {
            var documents = new[]
            {
                new Inventory
                {
                    Item = "journal", Qty =  25, Status = "A", S =  new Size() {H = 14, W = 21, Uom = MeasureType.Cm}
                },
                new Inventory
                {
                    Item = "notebook", Qty =  50 , Status = "A", S =  new Size() {H = 8.5m, W = 11, Uom = MeasureType.In}
                },
                new Inventory
                {
                    Item = "paper", Qty =  100 , Status = "D", S =  new Size() {H = 8.5m, W = 11, Uom = MeasureType.In}
                },
                new Inventory
                {
                    Item = "planner", Qty =  75 , Status = "D", S =  new Size() {H = 22.85m, W = 30, Uom = MeasureType.Cm}
                },
                new Inventory
                {
                    Item = "postcard", Qty =  45 , Status = "A", S =  new Size() {H = 10, W = 15.25m, Uom = MeasureType.Cm}
                },



            };

            var collItems = SampleConfig.CollInventory;
            SampleConfig.DbSampleLookup.DropCollection(collItems.CollectionNamespace.CollectionName);
            collItems.InsertMany(documents);


            // Find items with NULL value
            await UsingIFindFluent(collItems);
            await UsingLinq(collItems);
        }

        private static async Task UsingIFindFluent(IMongoCollection<Inventory> collItems)
        {

            // For example, the following query selects all documents where the field size equals the document { h: 14, w: 21, uom: "cm" }:

            var list = await collItems.Find(x => x.S.H == 14m && x.S.W == 21m && x.S.Uom == MeasureType.Cm).ToListAsync();

            ConsoleEx.WriteLine("Using IFindFluent", ConsoleColor.DarkYellow);
            Console.WriteLine();
            ConsoleEx.WriteLine(@"Find all documents where the field size equals the document { h: 14, w: 21, uom: 'cm' }:", ConsoleColor.Magenta);
            ConsoleEx.WriteLine("------------------------------------------------------", ConsoleColor.DarkYellow);
            foreach (var e in list)
            {
                ConsoleEx.WriteLine($"{e.Item} {e.Status}", ConsoleColor.Blue);
            }
            Console.WriteLine();


        }

        private static async Task UsingLinq(IMongoCollection<Inventory> collItems)
        {
            var list = await collItems.AsQueryable().Where(x => x.S.H == 14m && x.S.W == 21 && x.S.Uom == MeasureType.Cm).ToListAsync();

            ConsoleEx.WriteLine("Using MongoDB Linq", ConsoleColor.DarkYellow);
            Console.WriteLine();
            ConsoleEx.WriteLine("Find all documents where the field size equals the document { h: 14, w: 21, uom: 'cm' }:", ConsoleColor.Magenta);
            ConsoleEx.WriteLine("------------------------------------------------------", ConsoleColor.DarkYellow);
            foreach (var e in list)
            {
                ConsoleEx.WriteLine($"{e.Item} {e.Status}", ConsoleColor.Blue);
            }
            Console.WriteLine();

        }
    }
}
