using MongoDB.Driver;

using System;
using System.Threading.Tasks;
using SimpleTalkMongoDb.Configuration;
using SimpleTalkMongoDb.Pocos;

namespace SimpleTalkMongoDb.Aggregation
{
    internal static class SampleUnwind
    {


        public static void Main()
        {
            MainAsync().Wait();
            Console.ReadLine();
        }

      
        private static async Task MainAsync()
        {
           
          
            var collection = SampleConfig.Collection;
           
            var result = await collection.Aggregate()
                .Unwind<SalesHeader, SalesDetailHelper>(x => x.Details)
                .Project(x => new {x.Details.SalesOrderDetailId}).ToListAsync();

            ConsoleEx.WriteLine("By applying Unwind on the whole collection the number of documents rapidly grows and it is equal to :" , ConsoleColor.Cyan);
            ConsoleEx.WriteLine(result.Count.ToString() , ConsoleColor.Yellow);
            ConsoleEx.WriteLine("Notice that this is the same number as 'SELECT COUNT(*) FROM Sales.SalesDetail'", ConsoleColor.Cyan);


        }


    }
}
